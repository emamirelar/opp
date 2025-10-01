import { HttpClient, HttpErrorResponse, HttpResponse, HttpEventType } from '@angular/common/http';
import { Injectable, signal, ViewContainerRef, effect } from '@angular/core';
import { Observable, map, throwError, timer, Subject, of } from 'rxjs';
import { catchError, mergeMap, retry, retryWhen, tap, filter, switchMap, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { FetchStreamService } from '../../../common/services/fetch-stream.service';
import {
  AiAssistantRequest,
  AiAssistantSessionRequest,
  ChatHistoryItem,
  SessionData,
  SessionResponse,
  FileUpload,
  FileValidationResult,
  ChatRequestData,
  AiAssistantRequestWithFiles
} from '../models/ai-assistant.model';
import {GeminiResponse} from '../models/gemini.model';
import { 
  AiResponse, 
  ChatMessage, 
  ChatFile,
  getUrlPageByAiResponseCategory 
} from '../../../common/reusables/widgets/ai-assistant/ai-assistant.model';
import { ComponentResolverService } from './component-resolver.service';


export interface SessionWithChats {
  session: {
    id: string;
    startTime: string;
    endTime?: string;
    userId: number;
    status: string;
    title: string;
    archived: boolean;
    starred: boolean;
  };
  chatMessages: ChatMessage[];
}

@Injectable({
  providedIn: 'root',
})
export class AiAssistantService {
  private readonly apiUrl = '/api';
  private readonly aiAssistantUrl = `${this.apiUrl}/ai-assistant`;
  private readonly maxRetries = 3;

  // File upload configuration
  private readonly maxFileSize = 10 * 1024 * 1024; // 10MB
  
  private readonly allowedFileTypes = [
    // Images
    'image/jpeg',
    'image/png', 
    'image/gif',
    'image/webp',
    // Audio files
    'audio/wav',
    'audio/mp3',
    'audio/aiff', 
    'audio/aac',
    'audio/ogg',
    'audio/flac',
    // Documents
    'application/pdf',
    'text/plain',
    'text/csv',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // .docx
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
    'application/vnd.openxmlformats-officedocument.presentationml.presentation', // .pptx
    'application/msword', // .doc
    'application/vnd.ms-excel', // .xls
    'application/vnd.ms-powerpoint' // .ppt
  ];

  // STATE MANAGEMENT - Moved from AiAssistantData
  readonly chatHistory = signal<ChatMessage[]>([]);
  readonly currentSessionId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly isLoadingSession = signal(false);
  readonly sessionTitle = signal<string>('New Chat');
  readonly sessionStarred = signal<boolean>(false);
  readonly sessionArchived = signal<boolean>(false);
  readonly userSessions = signal<SessionData[]>([]);
  readonly isLoadingSessions = signal(false);
  readonly textToSpeech = signal(false);
  readonly isFirstPageLoad = signal<boolean>(true);

  // Private state
  private viewContainerRef?: ViewContainerRef;
  private _titleGeneratedForSession: { [key: string]: boolean } = {};
  private _isLoadingPastChat = false;

  // Streaming subjects
  private _chatHistoryChanged = new Subject<void>();
  chatHistoryChanged$ = this._chatHistoryChanged.asObservable();

  private _streamingChunk = new Subject<any>();
  streamingChunk$ = this._streamingChunk.asObservable();

  constructor(
    private http: HttpClient,
    private fetchStreamService: FetchStreamService,
    private router: Router,
    private componentResolverService: ComponentResolverService
  ) {
    // Effect to emit chat history changes only for new messages, not loaded chats
    effect(() => {
      const chatHistory = this.chatHistory();
      
      // Only emit when chat history changes and we're not loading a past chat
      if (chatHistory.length > 0 && !this._isLoadingPastChat) {
        this._chatHistoryChanged.next();
      }
    });
  }

  // Enhanced chat method with file support
  chatWithFiles(requestData: ChatRequestData): Observable<HttpResponse<AiResponse>> {
    const formData = this.createChatFormData(requestData);
    
    return this.http.post<AiResponse>(
      `${this.aiAssistantUrl}/chat`,
      formData,
      { observe: 'response' }
    ).pipe(
      this.addIapRetryStrategy<HttpResponse<AiResponse>>()
    );
  }

  // Enhanced chat method with simple parameters (backward compatible)
  chatWithFilesSimple(
    message: string, 
    sessionId?: string, 
    files?: File[], 
    state?: any
  ): Observable<HttpResponse<AiResponse>> {
    return this.chatWithFiles({
      message,
      sessionId,
      files,
      state
    });
  }

  // Streaming chat method with HttpClient - SIMPLIFIED
  chatWithFilesStreaming(
    message: string, 
    sessionId?: string, 
    files?: File[], 
    state?: any
  ): Observable<{ data: any, complete: boolean }> {
    const formData = this.createStreamingChatFormData({
      message,
      sessionId,
      files,
      state,
      streaming: true
    });
    
    // Use FetchStreamService to handle streaming with interceptor support
    return this.fetchStreamService.streamRequest(`${this.aiAssistantUrl}/chat`, {
      method: 'POST',
      body: formData
    }).pipe(
      // tap(chunk => {
      //   console.log('[ai-assistant-service] Raw chunk:', chunk);
      // }),
      switchMap(chunk => this.parseStreamingChunks(chunk)),
      tap(({ data, complete }) => {
        // DIRECT STREAMING: Emit all chunks immediately to streamingChunk$
        if (data?.content?.parts) {
          this._streamingChunk.next(data);
        }
        
        // Handle session ID updates
        if (data.session_id && !this.currentSessionId()) {
          this.currentSessionId.set(data.session_id);
          this.loadUserSessions().subscribe();
          this.router.navigate(['/ai', data.session_id], { replaceUrl: true });
        }
      }),
      finalize(() => {
         // When the stream completes, emit a completion signal after a small delay
        // This ensures all chunks have been processed before marking as complete
        setTimeout(() => {
          this._streamingChunk.next({ 
            streamCompleted: true, 
            timestamp: Date.now() 
          });
         }, 100);
      }),
      catchError(error => {
        console.error('[ai-assistant-service] Streaming error:', error);
        return throwError(() => error);
      })
    );
  }

  // Helper method to parse streaming chunks from fetch stream
  private parseStreamingChunks(chunkText: string): Observable<{ data: any, complete: boolean }> {
    return new Observable(observer => {
      try {
        // Parse SSE chunks (server sends data with "data:" prefix)
        const trimmedChunk = chunkText.replace(/^data:\s*/, '').trim();
        
        if (trimmedChunk) {
          try {
            const data = JSON.parse(trimmedChunk);
            
            // Emit the data and complete this inner observable immediately
            // This allows switchMap to process each chunk and continue to the next
            observer.next({ data, complete: false });
            observer.complete();
            
          } catch (parseError) {
            console.warn('🌊 [FRONTEND] Failed to parse chunk:', parseError, 'Chunk:', trimmedChunk);
            // Skip malformed chunks but complete the observable
            observer.complete();
          }
        } else {
          // Empty chunk, just complete
          observer.complete();
        }
      } catch (error) {
        observer.error(error);
      }
    });
  }

  // Helper method to parse complete streaming response
  // private parseCompleteStreamingResponse(responseText: string): Observable<{ data: any, complete: boolean }> {
  //   return new Observable(observer => {
  //     try {
  //       // Parse the final response
  //       const lines = responseText.split('\n');
  //       const dataLines = lines.filter(line => line.trim() && line.startsWith('data: '));
        
  //       if (dataLines.length > 0) {
  //         const lastDataLine = dataLines[dataLines.length - 1];
  //         try {
  //           const data = JSON.parse(lastDataLine.slice(6)); // Remove 'data: ' prefix
  //           observer.next({ data, complete: true });
  //         } catch (parseError) {
  //           console.warn('Failed to parse final streaming response:', parseError);
  //           observer.next({ data: { message: 'Stream completed' }, complete: true });
  //         }
  //       } else {
  //         observer.next({ data: { message: 'Stream completed' }, complete: true });
  //       }
        
  //       observer.complete();
  //     } catch (error) {
  //       observer.error(error);
  //     }
  //   });
  // }

  // Get personalized suggestions for the user
  getSuggestions(): Observable<any> {
    return this.http.get(`${this.apiUrl}/ai-assistant/generate-suggestions`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error fetching suggestions:', error);
        return throwError(() => new Error('Failed to fetch suggestions'));
      })
    );
  }

  // Helper method to create FormData for chat requests
  private createChatFormData(requestData: ChatRequestData): FormData {
    const formData = new FormData();
    
    // Add message
    formData.append('Message', requestData.message);
    
    // Add session ID if provided
    if (requestData.sessionId) {
      formData.append('sessionId', requestData.sessionId);
    }
    
    // Add state if provided
    if (requestData.state) {
      const stateString = typeof requestData.state === 'string' 
        ? requestData.state 
        : JSON.stringify(requestData.state);
      formData.append('State', stateString);
    }
    
    // Add files if provided
    if (requestData.files && requestData.files.length > 0) {
      // Validate files first
      const validation = this.validateFiles(requestData.files);
      
      if (validation.invalid.length > 0) {
        // Log warnings for invalid files but continue with valid ones
        validation.invalid.forEach(item => {
          console.warn(`[AI-ASSISTANT] Invalid file skipped: ${item.error}`);
        });
      }
      
      // Add valid files to FormData
      validation.valid.forEach((file, index) => {
        formData.append('Files', file, file.name);
      });
    }
    
    return formData;
  }

  // Helper method to create FormData for streaming chat requests
  private createStreamingChatFormData(requestData: ChatRequestData & { streaming?: boolean }): FormData {
    const formData = new FormData();
    
    // Add message
    formData.append('message', requestData.message);
    
    // Add session ID if provided
    if (requestData.sessionId) {
      formData.append('session_id', requestData.sessionId);
    }
    
    // Add streaming flag
    formData.append('streaming', requestData.streaming ? 'true' : 'false');
    
    // Add app_name (required by backend)
    formData.append('app_name', 'opportunityplus');
    
    // Add user information (these should come from auth service, but using defaults for now)
    formData.append('user_id', localStorage.getItem('user_id') || '');
    formData.append('user_email', localStorage.getItem('user_email') || '');
    
    // Add state if provided
    if (requestData.state) {
      const stateString = typeof requestData.state === 'string' 
        ? requestData.state 
        : JSON.stringify(requestData.state);
      formData.append('state', stateString);
    }
    
    // Add files if provided
    if (requestData.files && requestData.files.length > 0) {
      // Validate files first
      const validation = this.validateFiles(requestData.files);
      
      if (validation.invalid.length > 0) {
        // Log warnings for invalid files but continue with valid ones
        validation.invalid.forEach(item => {
          console.warn(`[AI-ASSISTANT] Invalid file skipped: ${item.error}`);
        });
      }
      
      // Add valid files to FormData
      validation.valid.forEach((file, index) => {
        formData.append('files', file, file.name);
      });
    }
    
    return formData;
  }

  // File validation methods
  validateFile(file: File): boolean {
    return this.validateFileSize(file) && this.validateFileType(file);
  }

  private validateFileSize(file: File): boolean {
    if (file.size > this.maxFileSize) {
      throw new Error(`File "${file.name}" is too large. Maximum size is ${this.formatFileSize(this.maxFileSize)}`);
    }
    return true;
  }

  private validateFileType(file: File): boolean {
    if (!this.allowedFileTypes.includes(file.type)) {
      throw new Error(`File type "${file.type}" is not supported. File: "${file.name}"`);
    }
    return true;
  }

  validateFiles(files: File[]): FileValidationResult {
    const valid: File[] = [];
    const invalid: { file: File; error: string }[] = [];
    
    files.forEach(file => {
      try {
        if (this.validateFile(file)) {
          valid.push(file);
        }
      } catch (error) {
        invalid.push({ 
          file, 
          error: (error as Error).message 
        });
      }
    });
    
    return { valid, invalid };
  }

  // Get file preview (for images)
  getFilePreview(file: File): Promise<string | null> {
    return new Promise((resolve) => {
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => resolve(e.target?.result as string);
        reader.onerror = () => resolve(null);
        reader.readAsDataURL(file);
      } else {
        resolve(null);
      }
    });
  }

  // Utility methods
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getFileIcon(mimeType: string): string {
    if (mimeType.startsWith('image/')) return '🖼️';
    if (mimeType === 'application/pdf') return '📄';
    if (mimeType.includes('word') || mimeType.includes('document')) return '📝';
    if (mimeType.includes('excel') || mimeType.includes('spreadsheet')) return '📊';
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return '📊';
    if (mimeType.startsWith('text/')) return '📋';
    return '📁';
  }

  isImageFile(file: File): boolean {
    return file.type.startsWith('image/');
  }

  // Configuration getters
  get maxFileSizeBytes(): number {
    return this.maxFileSize;
  }

  get maxFileSizeMB(): number {
    return this.maxFileSize / (1024 * 1024);
  }

  get supportedFileTypes(): string[] {
    return [...this.allowedFileTypes];
  }

  // Original methods remain unchanged for backward compatibility

  // Get all sessions for the current user
  getUserSessions(): Observable<HttpResponse<SessionData[]>> {
    return this.http.post<SessionData[]>(
      `${this.aiAssistantUrl}/get-user-sessions`, 
      {}, 
      { observe: 'response' }
    ).pipe(
      this.addIapRetryStrategy<HttpResponse<SessionData[]>>()
    );
  }

  // Get details for a specific session
  getSessionDetails(sessionId: string): Observable<HttpResponse<SessionData[]>> {
    return this.http.post<SessionData[]>(
      `${this.aiAssistantUrl}/get-session`,
      { sessionId } as AiAssistantSessionRequest,
      { observe: 'response' }
    ).pipe(
      this.addIapRetryStrategy<HttpResponse<SessionData[]>>()
    );
  }

  // Create a new AI assistant session
  createSession(): Observable<HttpResponse<{ sessionId: string }>> {
    return this.http.post<{ sessionId: string }>(
      `${this.aiAssistantUrl}/create-session`,
      {},
      { observe: 'response' }
    ).pipe(
      this.addIapRetryStrategy<HttpResponse<{ sessionId: string }>>()
    );
  }

  // End a chat session
  endSession(sessionId: string): Observable<HttpResponse<{ success: boolean }>> {
    return this.http.post<{ success: boolean }>(
      `${this.aiAssistantUrl}/end-session`,
      { sessionId } as AiAssistantSessionRequest,
      { observe: 'response' }
    );
  }

  // Chat with AiAssistant AI (original method - backward compatible)
  chat(formdata: FormData): Observable<HttpResponse<AiResponse>> {
    return this.http.post<AiResponse>(
      `${this.aiAssistantUrl}/chat`,
      formdata,
      { observe: 'response' }
    );
  }

  //Set Text to Speech
  toggleAccessibility(textToSpeech: boolean, sessionId: string): Observable<HttpResponse<{ success: boolean }>> {
    return this.http.post<{ success: boolean }>(
      `${this.aiAssistantUrl}/accessibility`,
      { textToSpeech, sessionId },
      { observe: 'response' }
    );
  }

  // Update session star status
  updateSessionStar(sessionId: string, starred: boolean): Observable<HttpResponse<{ success: boolean }>> {
    return this.http.post<{ success: boolean }>(
      `${this.aiAssistantUrl}/update-star`,
      { sessionId, starred },
      { observe: 'response' }
    );
  }

  // Update session archive status
  updateSessionArchive(sessionId: string, archived: boolean): Observable<HttpResponse<{ success: boolean }>> {
    return this.http.post<{ success: boolean }>(
      `${this.aiAssistantUrl}/update-archive`,
      { sessionId, archived },
      { observe: 'response' }
    );
  }

  // Update session title
  updateSessionTitle(sessionId: string, title: string): Observable<HttpResponse<{ success: boolean }>> {
    return this.http.post<{ success: boolean }>(
      `${this.aiAssistantUrl}/update-title`,
      { sessionId, title },
      { observe: 'response' }
    );
  }

  // Generate a title for a session (GET, sessionId as query param)
  generateTitle(sessionId: string): Observable<HttpResponse<{ title: string }>> {
    return this.http.get<{ title: string }>(
      `${this.aiAssistantUrl}/generate-title?sessionId=${encodeURIComponent(sessionId)}`,
      { observe: 'response' }
    );
  }

  // Helper method for IAP retry strategy
  private addIapRetryStrategy<T>() {
    return retryWhen<T>(errors => 
      errors.pipe(
        mergeMap((error, count) => {
          // Only retry on 401 errors
          if (error instanceof HttpErrorResponse && error.status === 401 && count < this.maxRetries) {
            
            // Exponential backoff
            return timer(1000 * Math.pow(2, count));
          }
          
          console.error('[AI-ASSISTANT] API call failed after retries or non-401 error', error);
          return throwError(() => error);
        })
      )
    );
  }

  // HIGH-LEVEL METHODS - Moved from AiAssistantData

  public setViewContainerRef(viewContainerRef: ViewContainerRef) {
    this.viewContainerRef = viewContainerRef;
  }

  public sendMessage(
    message: string, 
    files: ChatFile[] = [], 
    state?: any
  ): Observable<void> {
    if (!this.isValidMessage(message, files)) {
      return of();
    }

    // Mark as no longer first page load when user sends first message
    if (this.isFirstPageLoad()) {
      this.isFirstPageLoad.set(false);
    }

    this.addUserMessage(message, files);
    this.isLoading.set(true);

    // Allow empty sessionId for new conversations - backend will create one
    const sessionId = this.currentSessionId() || '';
    
    // Enhanced: Support multiple files instead of just the first one
    const fileObjects = files.map(chatFile => chatFile.file).filter(file => file != null);
    return this.sendMessageToServer(sessionId, message, fileObjects, state);
  }

  public clearConversation(): void {
    this.chatHistory.set([]);
    this.sessionTitle.set('New Chat');
    this.sessionStarred.set(false);
    this.sessionArchived.set(false);
    
    // No active session, just reset state
    this.currentSessionId.set(null);
    this.isLoading.set(false);
    
    // Mark as manual new chat (not first page load)
    this.isFirstPageLoad.set(false);
  }

  public toggleStar(): Observable<void> {
    const sessionId = this.currentSessionId();
    if (!sessionId) {
      return of();
    }

    const newStarredState = !this.sessionStarred();
    
    return this.updateSessionStar(sessionId, newStarredState).pipe(
      tap(response => {
        if (response?.body?.success) {
          this.sessionStarred.set(newStarredState);
        }
      }),
      catchError(error => {
        return of();
      }),
      map(() => void 0)
    );
  }

  public toggleArchive(): Observable<void> {
    const sessionId = this.currentSessionId();
    if (!sessionId) {
      return of();
    }

    const newArchivedState = !this.sessionArchived();
    
    return this.updateSessionArchive(sessionId, newArchivedState).pipe(
      tap(response => {
        if (response?.body?.success) {
          this.sessionArchived.set(newArchivedState);
        }
      }),
      catchError(error => {
        return of();
      }),
      map(() => void 0)
    );
  }

  public updateTitle(newTitle: string): Observable<void> {
    const sessionId = this.currentSessionId();
    if (!sessionId || !newTitle.trim()) {
      return of();
    }

    return this.updateSessionTitle(sessionId, newTitle.trim()).pipe(
      tap(response => {
        if (response?.body?.success) {
          this.sessionTitle.set(newTitle.trim());
          // Update the session in the list
          this.userSessions.update(sessions => 
            sessions.map(session => 
              session.id === sessionId 
                ? { ...session, title: newTitle.trim() } 
                : session
            )
          );
        }
      }),
      catchError(error => {
        return of();
      }),
      map(() => void 0)
    );
  }

  public loadUserSessions(): Observable<void> {
    this.isLoadingSessions.set(true);
    
    return this.getUserSessions().pipe(
      tap(response => {
        if (response?.body && Array.isArray(response.body)) {
          this.userSessions.set(response.body);
        }
      }),
      catchError(error => {
        return of();
      }),
      finalize(() => this.isLoadingSessions.set(false)),
      map(() => void 0)
    );
  }

  public switchToSession(sessionId: string): Observable<void> {
    if (sessionId === this.currentSessionId()) {
      return of(); // Already on this session
    }
    this.isLoadingSession.set(true);
    this.isLoading.set(true);
    this._isLoadingPastChat = true;
    this.currentSessionId.set(sessionId);
    
    return this.fetchSessionDetails(sessionId).pipe(
      finalize(() => {
        this.isLoading.set(false);
        this.isLoadingSession.set(false);
        this._isLoadingPastChat = false;
      })
    );
  }

  onTextToSpeechToggle(): void {
    this.isLoading.set(true);
    const sessionId = this.currentSessionId();
    const textToSpeech = !this.textToSpeech();

    if (sessionId) {
      this.toggleAccessibility(textToSpeech, sessionId).subscribe(response => {
          this.isLoading.set(false);
          if (response?.body?.success) {
            this.textToSpeech.set(textToSpeech);
          } else {
            throw new Error('Error with setting text to speech value.');
          }
        });
    }
  }

  // PRIVATE HELPER METHODS

  private sendMessageToServer(
    sessionId: string, 
    message: string, 
    files?: File[], 
    state?: any
  ): Observable<void> {
    // Track if we've created a streaming message for this conversation
    let streamingMessage: ChatMessage | null = null;
    let messageIndex = -1;

    // Use streaming response method
    return this.chatWithFilesStreaming(
      message,
      sessionId,
      files,
      state
    ).pipe(
      tap(({ data, complete }: { data: any, complete: boolean }) => {
        // Create a placeholder streaming message for the chat history if needed
        // This ensures the chat UI shows that a response is being generated
        if (!streamingMessage) {
          streamingMessage = {
            text: '',
            isUser: false,
            timestamp: new Date(),
            files: [],
            result: [],
            entity: undefined,
            suggestedUserResponses: [],
            sources: [],
            isFromHistory: false,
          } as ChatMessage;
          this.addMessage(streamingMessage);
          messageIndex = this.chatHistory().length - 1;
        }
      }),
      // Complete the observable when the stream finishes
      map(() => void 0),
      catchError(error => {
        this.isLoading.set(false);
        this.addSystemMessage({message:'Sorry, there was an error processing your message. Please try again.'});
        return of();
      }),
      finalize(() => {
        this.isLoading.set(false);
      })
    );
  }

  private isValidMessage(message: string, files: ChatFile[]): boolean {
    return Boolean(message.trim() || files.length);
  }

  private addUserMessage(message: string, files: ChatFile[]): void {
    if (files.length > 0) {
      const file = files[0]?.file;
      if (file)
      {
        files[0].mediaType = file?.type.split('/')[0];
        files[0].mediaUrl = URL.createObjectURL(file);
      }
    }
    this.addMessage({
      text: message,
      isUser: true,
      timestamp: new Date(),
      files,
      isFromHistory: false
    });
  }

  private addSystemMessage(aiResponse: AiResponse ): void {
    if (aiResponse.intent === 'Action') {
      if (aiResponse.url) {
        // Navigation
        this.router.navigateByUrl(aiResponse.url);
      } else {
        var record = aiResponse.rawMessage ? JSON.parse(aiResponse.rawMessage) : {};
        this.componentResolverService.loadComponent(aiResponse.entity, this.viewContainerRef, record);
      }
    }

    this.addMessage({
      text: aiResponse.message,
      isUser: false,
      timestamp: new Date(),
      files: aiResponse.files || [],
      isFromHistory: this._isLoadingPastChat
    });
  }

  private addMessage(message: ChatMessage): void {
    this.chatHistory.update(history => {
      const updated = [...history, message];

      // Check if we need to generate a title: after the second model message in a new session
      const isModel = (msg: ChatMessage) => !msg.isUser;
      const modelMessages = updated.filter(isModel);
      if (
        modelMessages.length === 2 &&
        this.currentSessionId() &&
        this.sessionTitle() === 'New Chat' &&
        !this._titleGeneratedForSession?.[this.currentSessionId()!]
      ) {
        // Mark as generated to avoid duplicate calls
        if (!this._titleGeneratedForSession) this._titleGeneratedForSession = {};
        this._titleGeneratedForSession[this.currentSessionId()!] = true;
        this.generateTitle(this.currentSessionId()!).subscribe({
          next: (resp) => {
            const newTitle = resp.body?.title?.trim();
            if (newTitle) {
              this.sessionTitle.set(newTitle);
              // Also update the session in the list
              this.userSessions.update(sessions =>
                sessions.map(session =>
                  session.id === this.currentSessionId()
                    ? { ...session, title: newTitle }
                    : session
                )
              );
            }
          },
          error: (err) => {
          }
        });
      }
      return updated;
    });
  }

  private fetchSessionDetails(sessionId: string): Observable<void> {
    this.isLoadingSession.set(true);
    return this.getSessionDetails(sessionId).pipe(
      tap(detailsResponse => {
        const sessionData = detailsResponse?.body as unknown as SessionWithChats;
        if (sessionData?.session) {
          // Update session details
          this.sessionTitle.set(sessionData.session.title);
          this.sessionStarred.set(sessionData.session.starred);
          this.sessionArchived.set(sessionData.session.archived);
          
          // Also update the session in the list to keep it in sync
          this.userSessions.update(sessions =>
            sessions.map(session =>
              session.id === sessionId
                ? { ...session, title: sessionData.session.title, starred: sessionData.session.starred, archived: sessionData.session.archived }
                : session
            )
          );
          
          // Update chat history
          if (sessionData.chatMessages) {
            const history = this.processNewChatHistory(sessionData.chatMessages);
            this.chatHistory.set(history);
          }
        } else if (detailsResponse?.body?.[0]?.chats) {
          // Fallback for old response structure
          const history = this.processSessionHistory(detailsResponse.body[0].chats);
          this.chatHistory.set(history);
        }
      }),
      catchError(error => {
        this.chatHistory.set([]);
        return of();
      }),
      map(() => void 0)
    );
  }

  private processSessionHistory(chats: any[]): ChatMessage[] {
    return chats
      .map(chat => {
          const files: ChatFile[] = [{
            mediaUrl: chat.mediaUrl,
            mediaType: chat.mediaType
          }];
          return {
            text: this.parseMessageContent(chat.message || ''),
            isUser: chat.sender === 'user',
            timestamp: chat.timestamp ? new Date(chat.timestamp) : new Date(),
            files
          }
      })
      .filter(message => message.text);
  }

  private processNewChatHistory(chatMessages: any[]): ChatMessage[] {
    const mapped = chatMessages.map(chat => {
      const message: ChatMessage = {
        text: chat.text ? this.parseMessageContent(chat.text) : '',
        isUser: chat.role === 'user',
        timestamp: new Date(),
        files: [],
        isFromHistory: true,
        inlineData: (chat.inlineData || chat.InlineData) ? (chat.inlineData || chat.InlineData).map((inline: any) => {
          return {
            data: inline.data || inline.Data,
            mimeType: inline.mimeType || inline.MimeType
          };
        }) : []
      };
      // For model messages, check if the text contains structured data
      if (!message.isUser && message.text) {
        try {
          const parsed = JSON.parse(message.text);
          if (parsed.result && Array.isArray(parsed.result)) {
            message.result = parsed.result;
            message.entity = parsed.entity;
            message.suggestedUserResponses = parsed.suggestedUserResponses || [];
            message.sources = parsed.sources || [];
            message.text = '';
          }
        } catch (e) {
          // Not JSON, keep as regular text
        }
      }
      return message;
    });

    const filtered = mapped.filter(message =>
      (message.text && message.text.trim() !== '') ||
      (message.result && message.result.length > 0) ||
      (message.inlineData && message.inlineData.length > 0)
    );

    return filtered;
  }

  private parseMessageContent(message: string): string {
    try {
      const cleanedMessage = message
        .replace(/^```json\s*/, '')
        .replace(/```/, '');
      return cleanedMessage;
    } catch (error) {
      return message;
    }
  }
}
