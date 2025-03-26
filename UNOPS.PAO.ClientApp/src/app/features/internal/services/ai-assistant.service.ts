import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
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

  constructor(private http: HttpClient) {}


  // Get all sessions for the current user
  getUserSessions(): Observable<HttpResponse<SessionData[]>> {
    return this.http.post<SessionData[]>(`${this.aiAssistantUrl}/get-user-sessions`, {}, { observe: 'response' });
  }

  // Get details for a specific session
  getSessionDetails(sessionId: string): Observable<HttpResponse<SessionData[]>> {
    return this.http.post<SessionData[]>(
      `${this.aiAssistantUrl}/get-session`,
      { sessionId } as AiAssistantSessionRequest,
      { observe: 'response' }
    );
  }

  // Create a new chat session
  createSession(): Observable<HttpResponse<SessionResponse>> {
    return this.http.post<SessionResponse>(`${this.aiAssistantUrl}/create-session`, {}, { observe: 'response' });
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
      {},
      { observe: 'response' }
    );
  }
}
