import {Injectable, signal} from '@angular/core';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';
import { SessionData } from '../../../../features/internal/models/ai-assistant.model';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError, tap, switchMap, finalize } from 'rxjs/operators';

export interface ChatMessage {
  text?: string;
  isUser?: boolean;
  timestamp?: Date;
  files?: { name?: string, content?: string }[];
}

@Injectable({
  providedIn: 'root',
})
export class AiAssistantData {
  readonly chatHistory = signal<ChatMessage[]>([]);
  readonly currentSessionId = signal<string | null>(null);
  readonly isLoading = signal(false);

  constructor(private aiAssistantService: AiAssistantService) {}

  public loadLatestSession(): void {
    this.loadOrCreateSession().subscribe();
  }

  loadOrCreateSession(): Observable<any> {
    this.isLoading.set(true);

    return this.aiAssistantService.getUserSessions().pipe(
      switchMap(sessionsResponse => {
        if (sessionsResponse?.body && sessionsResponse.body.length > 0) {
          return this.getHistoryFromServer(sessionsResponse.body);
        } else {
          return this.createNewSession();
        }
      }),
      catchError(error => {
        console.error('Error in session management:', error);
        return throwError(() => error);
      }),
      finalize(() => {
        this.isLoading.set(false);
      })
    );
  }

  private createNewSession() {
    return this.aiAssistantService.createSession().pipe(
      tap(newSessionResponse => {
        if (newSessionResponse?.body?.sessionId) {
          this.currentSessionId.set(newSessionResponse.body.sessionId);
          this.isLoading.set(false);
        } else {
          throw new Error('Failed to create new session');
        }
      })
    );
  }

  private getHistoryFromServer(sessions: SessionData[]): Observable<void> {
    if (!sessions.length) {
      return of();
    }

    const lastSession: SessionData = sessions[sessions.length - 1];
    const sessionId = lastSession?.id;

    if (!sessionId) {
      console.error('Invalid session data received');
      return of();
    }

    this.currentSessionId.set(sessionId);

    return this.getSessionDetails(sessionId);
  }

  private parseJsonInMessage(message: string): string {
    console.log(message)
    try {
      message = message.replace(/^```json\s*/, '');
      message = message.replace(/```$/, '');
      return JSON.parse(message)["Message"];
    } catch (error) {
      return message;
    }
  }

  private getSessionDetails(sessionId: string) {
    return this.aiAssistantService.getSessionDetails(sessionId).pipe(
      tap(detailsResponse => {
        if (detailsResponse?.body) {
          const history = detailsResponse.body[0]['chats']?.map(item => ({
            text: this.parseJsonInMessage(item.message || ''),
            isUser: item.sender === 'user',
            timestamp: item.timestamp ? new Date(item.timestamp) : undefined
          })).filter(Boolean);
          this.chatHistory.set(history as ChatMessage[]);
        }
      }),
      catchError(error => {
        console.error('Error fetching session details:', error);
        this.chatHistory.set([]);
        return of(void 0);
      }),
      map(() => void 0)
    );
  }

  sendMessage(message: string, files: { name: string, content: string }[] = []): Observable<void> {
    if (!message.trim() && !files.length) {
      return of();
    }

    const userMessage: ChatMessage = {
      text: message,
      isUser: true,
      timestamp: new Date(),
      files: [...files]
    };
    this.addMessage(userMessage);

    const sessionId = this.currentSessionId();
    if (!sessionId) {
      this.addMessage({
        text: 'No active session. Please try refreshing the page.',
        isUser: false,
        timestamp: new Date()
      });
      return of();
    }

    return this.aiAssistantService.chat(sessionId, message).pipe(
      tap(response => {
        const text = response.body?.candidates[0].content.parts[0].text;
        if (text) {
          const aiMessage: ChatMessage = {
            text: this.parseJsonInMessage(text || ''),
            isUser: false,
            timestamp: new Date()
          };
          this.addMessage(aiMessage);
        }
      }),
      catchError(error => {
        console.error('Error sending message:', error);
        this.addMessage({
          text: 'Sorry, there was an error processing your message. Please try again.',
          isUser: false,
          timestamp: new Date()
        });
        return of(void 0);
      }),
      map(() => void 0)
    );
  }

  addMessage(message: ChatMessage) {
    this.chatHistory.update(history => [...history, message]);
  }

  clearHistory() {
    this.chatHistory.set([]);
    this.isLoading.set(true);
    const sessionId = this.currentSessionId();
    if (sessionId) {
      this.aiAssistantService.endSession(sessionId).subscribe(()=> this.createNewSession().subscribe());
    }
  }
}
