import { HttpClient, HttpErrorResponse, HttpResponse, HttpEventType } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, throwError, timer } from 'rxjs';
import { catchError, mergeMap, retry, retryWhen, tap, filter, switchMap } from 'rxjs/operators';
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
import { AiResponse } from '../../../common/reusables/widgets/ai-assistant/ai-assistant.model';


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

  constructor(private http: HttpClient) {}

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

  // Streaming chat method with HttpClient
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
    
    // Use HttpClient to ensure interceptors are applied for authentication
    return this.http.post(`${this.aiAssistantUrl}/chat`, formData, {
      headers: {
        'Accept': 'text/event-stream',
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0'
      },
      responseType: 'text',
      observe: 'events',
      reportProgress: true
    }).pipe(
      filter(event => event.type === HttpEventType.DownloadProgress || event.type === HttpEventType.Response),
      switchMap(event => {
        if (event.type === HttpEventType.Response) {
          // Handle final response
          console.log('🌊 [FRONTEND] Stream completed');
          return this.parseCompleteStreamingResponse(event.body || '');
        } else if (event.type === HttpEventType.DownloadProgress) {
          // Handle streaming chunks
          const partialText = (event as any).partialText || '';
          return this.parseStreamingChunks(partialText);
        }
        return [];
      }),
      catchError(error => {
        console.error('🌊 [FRONTEND] HttpClient streaming error:', error);
        return throwError(() => error);
      })
    );
  }

  // Helper method to parse streaming chunks from HttpClient
  private parseStreamingChunks(partialText: string): Observable<{ data: any, complete: boolean }> {
    return new Observable(observer => {
      try {
        // Parse Server-Sent Events format
        const lines = partialText.split('\n');
        const dataLines = lines.filter(line => line.trim() && line.startsWith('data: '));
        
        for (const line of dataLines) {
          try {
            const dataStr = line.slice(6).trim(); // Remove 'data: ' prefix
            if (dataStr) {
              const data = JSON.parse(dataStr);
              const isComplete = this.isCompleteResponse(data);
              
              console.log(`🌊 [FRONTEND] HttpClient chunk parsed, complete: ${isComplete}`);
              observer.next({ data, complete: isComplete });
            }
          } catch (parseError) {
            console.warn('[FRONTEND] Failed to parse HttpClient streaming data:', parseError);
          }
        }
        
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  // Helper method to parse complete streaming response
  private parseCompleteStreamingResponse(responseText: string): Observable<{ data: any, complete: boolean }> {
    return new Observable(observer => {
      try {
        // Parse the final response
        const lines = responseText.split('\n');
        const dataLines = lines.filter(line => line.trim() && line.startsWith('data: '));
        
        if (dataLines.length > 0) {
          const lastDataLine = dataLines[dataLines.length - 1];
          try {
            const data = JSON.parse(lastDataLine.slice(6)); // Remove 'data: ' prefix
            observer.next({ data, complete: true });
          } catch (parseError) {
            console.warn('Failed to parse final streaming response:', parseError);
            observer.next({ data: { message: 'Stream completed' }, complete: true });
          }
        } else {
          observer.next({ data: { message: 'Stream completed' }, complete: true });
        }
        
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  // Helper method to determine if a streaming response is complete
  private isCompleteResponse(data: any): boolean {
    // The final chunk is identified by content.role === 'user' 
    // which appears to be the user message echo at the end of streaming
    if (data.content?.role === 'user') {
      console.log('🏁 Detected final chunk (user message echo)');
      return true;
    }
    
    // All other chunks (model responses, partial chunks, etc.) are intermediate
    return false;
  }

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
      
      console.log(`[AI-ASSISTANT] Added ${validation.valid.length} valid files to request`);
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
      
      console.log(`[AI-ASSISTANT] Added ${validation.valid.length} valid files to streaming request`);
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
}
