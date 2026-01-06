import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable, signal, ViewContainerRef, effect } from '@angular/core';
import { Observable, map, throwError, timer, Subject, of } from 'rxjs';
import { catchError, mergeMap, retry, retryWhen, tap, switchMap, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { FetchStreamService } from '@shared/services/utils';
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
  ChatSession,
  ChatMessage, 
  ChatFile,
  ContentPart
} from '@features/ai/widgets/ai-assistant/ai-assistant.model';
import { ComponentResolverService } from '@shared/services/utils/component-resolver.service';


// Legacy interface - will be replaced by unified ChatSession
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
  chatMessages: any[]; // Legacy format
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
    'audio/mpeg',
    'audio/webm',
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

  // UNIFIED STATE MANAGEMENT - Single ChatSession for all operations
  readonly currentChatSession = signal<ChatSession | null>(null);
  readonly currentSessionId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly isLoadingSession = signal(false);
  readonly sessionTitle = signal<string>('');
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
  
  // Public method to manually emit chat history changes
  public emitChatHistoryChanged(): void {
    this._chatHistoryChanged.next();
  }

  private _streamingChunk = new Subject<any>();
  streamingChunk$ = this._streamingChunk.asObservable();

  constructor(
    private http: HttpClient,
    private fetchStreamService: FetchStreamService,
    private router: Router,
    private componentResolverService: ComponentResolverService
  ) {
    // NO AUTOMATIC EFFECTS - Manual control only to prevent unwanted triggers
  }

  // Server communication with streaming using unified model
  public sendMessageToServer(
    sessionId: string, 
    userMessage: ChatMessage, 
    files?: ChatFile[], 
    state?: any
  ): Observable<void> {
    // DEBUG: Log incoming session ID for this request
    console.log(`🔵 [AI-SERVICE sendMessageToServer] Called with sessionId: '${sessionId}' (empty: ${!sessionId})`);
    console.log(`🔵 [AI-SERVICE sendMessageToServer] currentSessionId signal value: '${this.currentSessionId()}'`);
    
    // Track if we've created a streaming message for this conversation
    let streamingMessage: ChatMessage | null = null;
    let messageIndex = -1;

    // Use streaming response method
    return this.chatWithFilesStreaming(userMessage, sessionId, files, state).pipe(
      tap(({ data, complete }: { data: any, complete: boolean }) => {
        // Create a placeholder streaming message for the MODEL'S response
        // This ensures the chat UI shows that a response is being generated
        if (!streamingMessage) {
          streamingMessage = {
            id: this.generateId(),
            timestamp: Date.now(),
            invocationId: this.generateInvocationId(),
            role: "model", // This is the MODEL's response placeholder
            content: {
              parts: [{ text: '' }], // Empty text part for streaming
              role: "model"
            },
            actions: { stateDelta: {}, artifactDelta: {}, requestedAuthConfigs: {} },
            longRunningToolIds: [],
            isUser: false, // Computed from role === "model"
            files: [],
            sources: [],
            suggestedUserResponses: []
          } as ChatMessage;
          
          // Add to current ChatSession's chatMessages array (single source of truth)
          const currentSession = this.currentChatSession();
          if (currentSession) {
            currentSession.chatMessages.push(streamingMessage);
            messageIndex = currentSession.chatMessages.length - 1;
            
            // Manually emit chat history change for new messages (not loaded sessions)
            if (!this._isLoadingPastChat) {
              this.emitChatHistoryChanged();
            }
          }
        }
      }),
      // Complete the observable when the stream finishes
      map(() => void 0),
      catchError(error => {
        this.isLoading.set(false);
        this.addSystemMessage('Sorry, there was an error processing your message. Please try again.');
        return of();
      }),
      finalize(() => {
        this.isLoading.set(false);
      })
    );
  }

  // Streaming chat method with unified model
  chatWithFilesStreaming(
    userMessage: ChatMessage, 
    sessionId?: string, 
    files?: ChatFile[], 
    state?: any
  ): Observable<{ data: any, complete: boolean }> {
    // Separate files with GCS paths from raw files
    const gcsFiles = files
      ?.filter(f => f.gcsPath)
      .map(f => ({ gcsPath: f.gcsPath!, name: f.name || '', mimeType: f.mediaType })) || [];
    
    const rawFiles = files
      ?.filter(f => !f.gcsPath && f.file)
      .map(f => f.file!)
      .filter(file => file != null) || [];
    
    const formData = this.createStreamingChatFormData({
      message: userMessage.content.parts[0]?.text || '', // Extract text from unified model
      sessionId,
      files: rawFiles.length > 0 ? rawFiles : undefined,
      gcsFiles: gcsFiles.length > 0 ? gcsFiles : undefined,
      state,
      streaming: true
    });
    
    // DEBUG: Log what session_id we're sending to the server
    console.log(`🔵 [AI-SERVICE] Sending chat request with session_id: '${sessionId}' (empty: ${!sessionId})`);
    
    // Use FetchStreamService to handle streaming with interceptor support
    return this.fetchStreamService.streamRequest(`${this.aiAssistantUrl}/chat`, {
      method: 'POST',
      body: formData
    }).pipe(
      switchMap(chunk => this.parseStreamingChunks(chunk)),
      tap(({ data, complete }) => {
        // Handle session ID from first chunk BEFORE processing content
        // The backend sends a special first chunk with just session_id and timestamp
        const sessionIdFromServer = data?.session_id || data?.sessionId;
        
        // DEBUG: Log every chunk that has a session_id
        if (sessionIdFromServer) {
          console.log(`🟢 [AI-SERVICE] Received session_id from server: '${sessionIdFromServer}'`);
          const currentStoredId = this.currentSessionId();
          console.log(`🟢 [AI-SERVICE] Current stored session_id: '${currentStoredId}' (will update: ${!currentStoredId})`);
          
          if (!currentStoredId) {
            this.currentSessionId.set(sessionIdFromServer);
            console.log(`✅ [AI-SERVICE] Session ID stored: '${sessionIdFromServer}'`);
            
            // Also update the current chat session's ID and sync the title signal
            const currentSession = this.currentChatSession();
            if (currentSession && (!currentSession.session.id || currentSession.session.id === '')) {
              currentSession.session.id = sessionIdFromServer;
              
              // Update the sessionTitle signal from the session object
              if (currentSession.session.title) {
                this.sessionTitle.set(currentSession.session.title);
              }
            }
          }
        }
        
        // Process content chunks AFTER session ID handling
        // The backend sends a special first chunk with only {session_id, timestamp}
        // Skip that chunk and only process chunks that have actual content
        if (data?.content?.parts) {
          // This is a real content chunk - emit for UI rendering
          this._streamingChunk.next(data);
          
          // CRITICAL: Also update the ChatMessage in the session with accumulated text
          // This ensures that when switching modes, the session has the full content
          this.updateChatMessageFromChunk(data);
        }
      }),
      finalize(() => {
        // When the stream completes, emit a completion signal
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
    
    // Add streaming flag (always streaming)
    formData.append('streaming', 'true');
    
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
    
    // Add GCS file paths if provided (preferred over raw files)
    if (requestData.gcsFiles && requestData.gcsFiles.length > 0) {
      // Send GCS paths as JSON array for backend to process
      formData.append('gcs_files', JSON.stringify(requestData.gcsFiles));
    }
    // Add raw files as fallback if no GCS files
    else if (requestData.files && requestData.files.length > 0) {
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
  chat(formdata: FormData): Observable<HttpResponse<any>> {
    return this.http.post<any>(
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

  /**
   * Update ChatMessage in session with accumulated text from streaming chunks
   * This ensures the session state matches what's rendered in the UI
   */
  private updateChatMessageFromChunk(chunk: any): void {
    const currentSession = this.currentChatSession();
    if (!currentSession || !chunk.invocationId) {
      return;
    }

    // Find the ChatMessage with matching invocationId
    let targetMessage = currentSession.chatMessages.find(
      msg => msg.invocationId === chunk.invocationId
    );

    // If not found, this is a new AI response - create it
    // AI responses may not have role set in chunks, so we check if it's NOT a user message
    const isUserChunk = chunk.role === 'user' || chunk.isUser === true || chunk.author === 'user';
    
    if (!targetMessage && !isUserChunk) {
      // Ensure AI message timestamp is after the most recent user message
      // This guarantees correct chronological order when sorting
      const lastMessage = currentSession.chatMessages[currentSession.chatMessages.length - 1];
      const aiMessageTimestamp = lastMessage && lastMessage.timestamp 
        ? lastMessage.timestamp + 1  // 1ms after the last message (should be the user message)
        : (chunk.timestamp || Date.now());
      
      targetMessage = {
        id: this.generateId(),
        timestamp: aiMessageTimestamp,
        invocationId: chunk.invocationId,
        role: 'model',
        content: {
          parts: [],
          role: 'model'
        },
        actions: { stateDelta: {}, artifactDelta: {}, requestedAuthConfigs: {} },
        longRunningToolIds: [],
        isUser: false,
        files: [],
        sources: chunk.sources || [],
        suggestedUserResponses: chunk.suggestedUserResponses || []
      };
      currentSession.chatMessages.push(targetMessage);
    }

    if (!targetMessage) {
      return;
    }

    // Process each part in the chunk
    if (chunk.content?.parts) {
      chunk.content.parts.forEach((chunkPart: any, partIndex: number) => {
        // Determine the part type
        const partType = chunkPart.thought ? 'thought' : 'text';
        
        // Find existing part of the same type in the message
        let existingPart = targetMessage!.content.parts.find(
          (p: any) => (p.thought && chunkPart.thought) || (!p.thought && !chunkPart.thought && p.text !== undefined)
        );

        if (existingPart) {
          // Update existing part - accumulate text if partial, replace if final
          if (chunk.partial === true) {
            // Partial chunk - concatenate text
            if (chunkPart.text) {
              existingPart.text = (existingPart.text || '') + chunkPart.text;
            }
          } else {
            // Final chunk - replace with complete text
            if (chunkPart.text !== undefined) {
              existingPart.text = chunkPart.text;
            }
            // Copy other properties
            if (chunkPart.thought !== undefined) {
              existingPart.thought = chunkPart.thought;
            }
            if (chunkPart.functionCall) {
              existingPart.functionCall = chunkPart.functionCall;
            }
            if (chunkPart.functionResponse) {
              existingPart.functionResponse = chunkPart.functionResponse;
            }
          }
        } else {
          // New part - add it to the message
          targetMessage!.content.parts.push({ ...chunkPart });
        }
      });
    }

    // Update other message properties from final chunk
    if (chunk.partial === false || chunk.partial === undefined) {
      if (chunk.sources) {
        targetMessage.sources = chunk.sources;
      }
      if (chunk.suggestedUserResponses) {
        targetMessage.suggestedUserResponses = chunk.suggestedUserResponses;
      }
    }
  }

  // UNIFIED SESSION MANAGEMENT
  public clearConversation(): void {
    this.currentChatSession.set(null);
    this.sessionTitle.set('');
    this.sessionStarred.set(false);
    this.sessionArchived.set(false);
    
    // No active session, just reset state
    this.currentSessionId.set(null);
    this.isLoading.set(false);
    
    // Mark as manual new chat (not first page load)
    this.isFirstPageLoad.set(false);
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
    
    // Clear current session data before loading new session
    this.currentChatSession.set(null);
    this.sessionTitle.set('');
    this.sessionStarred.set(false);
    this.sessionArchived.set(false);
    
    this.currentSessionId.set(sessionId);
    
    return this.loadSessionWithChats(sessionId).pipe(
      map(() => void 0),
      finalize(() => {
        this.isLoading.set(false);
        this.isLoadingSession.set(false);
        this._isLoadingPastChat = false;
      })
    );
  }

  // Load session with chat history from API using unified model
  loadSessionWithChats(sessionId: string): Observable<SessionWithChats> {
    return this.http.post<SessionWithChats>(`${this.aiAssistantUrl}/get-session`, {
      sessionId: sessionId
    }).pipe(
      tap(sessionData => {
        // Update current session state
        this.currentSessionId.set(sessionId);
        this.sessionTitle.set(sessionData.session.title);
        this.sessionStarred.set(sessionData.session.starred);
        this.sessionArchived.set(sessionData.session.archived);
        
        // Convert to unified model format with defensive role assignment
        const chatMessages: ChatMessage[] = sessionData.chatMessages.map(msg => ({
          id: msg.id || this.generateId(),
          timestamp: msg.timestamp || Date.now(),
          invocationId: msg.invocationId || this.generateInvocationId(),
          role: msg.content?.role || (msg.author === "user" ? "user" : "model"), // Defensive: use content.role first, fallback to author
          content: msg.content || { parts: [{ text: msg.text || '' }], role: msg.content?.role || (msg.author === "user" ? "user" : "model") },
          actions: msg.actions || { stateDelta: {}, artifactDelta: {}, requestedAuthConfigs: {} },
          longRunningToolIds: msg.longRunningToolIds || [],
          isUser: msg.content?.role === "user" || msg.author === "user", // Defensive: check both role and author
          files: msg.files || [],
          sources: msg.sources || [],
          suggestedUserResponses: msg.suggestedUserResponses || []
        }));
        
        // Always create a completely new ChatSession to ensure clean state
        this.currentChatSession.set({
          session: {
            id: sessionData.session.id,
            timestamp: new Date(sessionData.session.startTime).getTime(),
            userId: sessionData.session.userId,
            status: sessionData.session.status,
            title: sessionData.session.title,
            starred: sessionData.session.starred,
            archived: sessionData.session.archived
          },
          chatMessages: chatMessages
        });
        
        // Message processing is now handled by the panel component
        // when the session is loaded or switched
      })
    );
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

  // UTILITY METHODS
  public setViewContainerRef(viewContainerRef: ViewContainerRef) {
    this.viewContainerRef = viewContainerRef;
  }

  private isValidMessage(message: string, files: ChatFile[]): boolean {
    return Boolean(message.trim() || files.length);
  }

  private addSystemMessage(message: string): void {
    const systemMessage: ChatMessage = {
      id: this.generateId(),
      timestamp: Date.now(),
      invocationId: this.generateInvocationId(),
      role: "model",
      content: {
        parts: [{ text: message }],
        role: "model"
      },
      actions: { stateDelta: {}, artifactDelta: {}, requestedAuthConfigs: {} },
      longRunningToolIds: [],
      isUser: false,
      files: [],
      sources: [],
      suggestedUserResponses: []
    };

    const currentSession = this.currentChatSession();
    if (currentSession) {
      currentSession.chatMessages.push(systemMessage);
    }
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  private generateInvocationId(): string {
    return 'e-' + Math.random().toString(36).substr(2, 9);
  }

  private getCurrentUserId(): number {
    // This should come from auth service
    return parseInt(localStorage.getItem('user_id') || '0', 10);
  }
}
