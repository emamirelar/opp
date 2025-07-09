import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, throwError, timer } from 'rxjs';
import { catchError, mergeMap, retry, retryWhen } from 'rxjs/operators';
import {
  AiAssistantRequest,
  AiAssistantSessionRequest,
  ChatHistoryItem,
  SessionData,
  SessionResponse
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

  constructor(private http: HttpClient) {}


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

  // Chat with AiAssistant AI
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
