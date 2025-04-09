import {Injectable, signal, ViewChild, ViewContainerRef} from '@angular/core';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';
import { SessionData } from '../../../../features/internal/models/ai-assistant.model';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError, tap, switchMap, finalize } from 'rxjs/operators';
import {
  ChatMessage,
  ChatFile,
  AiResponse,
  getUrlPageByAiResponseCategory
} from './ai-assistant.model';
import { Router } from '@angular/router';
import { ComponentResolverService } from '../../../../features/internal/services/component-resolver.service';


@Injectable({
  providedIn: 'root',
})
export class AiAssistantData {
  readonly chatHistory = signal<ChatMessage[]>([]);
  readonly currentSessionId = signal<string | null>(null);
  readonly isLoading = signal(false);
  private viewContainerRef?: ViewContainerRef; // Store ViewContainerRef
  currentModelMessage: any = {};
  textToSpeech = signal(false);

  constructor(
    private aiAssistantService: AiAssistantService,
    private router: Router,
    private componentResolverService: ComponentResolverService
  ) {
    this.loadOrCreateSession().subscribe({
      error: (error) => console.error('Failed to initialize session:', error)
    });
  }

  public setViewContainerRef(viewContainerRef: ViewContainerRef) {
    this.viewContainerRef = viewContainerRef;
  }

  public sendMessage(message: string, files: ChatFile[] = []): Observable<void> {
    if (!this.isValidMessage(message, files)) {
      return of();
    }

    this.addUserMessage(message, files);

    const sessionId = this.currentSessionId();
    if (!sessionId) {
      this.addSystemMessage({message:'No active session. Please try refreshing the page.'});
      return of();
    }
    return this.sendMessageToServer(sessionId, message, files[0]?.file);
  }

  public clearConversation(): void {
    this.chatHistory.set([]);
    this.isLoading.set(true);

    const sessionId = this.currentSessionId();
    if (sessionId) {
      this.aiAssistantService.endSession(sessionId).subscribe({
        next: () => this.createNewSession().subscribe(),
        error: (error) => console.error('Failed to end session:', error)
      });
    }
  }

  private loadOrCreateSession(): Observable<void> {
    this.isLoading.set(true);

    return this.aiAssistantService.getUserSessions().pipe(
      switchMap(sessionsResponse => {
        if (this.hasValidSessions(sessionsResponse?.body)) {
          return this.getHistoryFromServer(sessionsResponse.body);
        }
        return this.createNewSession();
      }),
      catchError(error => {
        console.error('Session management error:', error);
        return throwError(() => new Error('Failed to load or create session'));
      }),
      finalize(() => this.isLoading.set(false))
    );
  }

  private hasValidSessions(sessions: SessionData[] | null | undefined): sessions is SessionData[] {
    return Array.isArray(sessions) && sessions.length > 0;
  }

  private createNewSession(): Observable<void> {
    return this.aiAssistantService.createSession().pipe(
      tap(response => {
        this.isLoading.set(false);
        if (response?.body?.sessionId) {
          this.currentSessionId.set(response.body.sessionId);
        } else {
          throw new Error('Invalid session response');
        }
      }),
      map(() => void 0)
    );
  }

  private getHistoryFromServer(sessions: SessionData[]): Observable<void> {
    const lastSession = sessions[sessions.length - 1];
    if (!lastSession?.id) {
      console.error('Invalid session data received');
      return of();
    }

    this.currentSessionId.set(lastSession.id);
    return this.fetchSessionDetails(lastSession.id);
  }

  private fetchSessionDetails(sessionId: string): Observable<void> {
    return this.aiAssistantService.getSessionDetails(sessionId).pipe(
      tap(detailsResponse => {
        if (detailsResponse?.body?.[0]?.chats) {
          const history = this.processSessionHistory(detailsResponse.body[0].chats);
          this.chatHistory.set(history);
        }
      }),
      catchError(error => {
        console.error('Error fetching session details:', error);
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
      files
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
      files: aiResponse.files
    });
  }

  private handleActionResponse(aiResponse: AiResponse): void {
    const pageUrl = getUrlPageByAiResponseCategory(aiResponse?.entity || '');

    if (!pageUrl) {
      this.addSystemMessage({message: 'Sorry, I cannot navigate to that page.'});
      return;
    }

    this.router.navigate([pageUrl], {
      state: { data: aiResponse },
      queryParams: { openNewDialog: 'true' }
    });
  }

  private addMessage(message: ChatMessage): void {
    this.chatHistory.update(history => [...history, message]);
  }

  private sendMessageToServer(sessionId: string, message: string, file?: File): Observable<void> {
    const formData = new FormData();
    formData.append("sessionId", sessionId);
    formData.append("message", message);
    if (file) {
        formData.append("file", file);
    }
    // TODO: Add the file to the formData
    return this.aiAssistantService.chat(formData).pipe(
      tap(response => {
        const serverResponse: any = response.body;
        let text = serverResponse?.message;
        if (text) {
          text = this.parseMessageContent(text);
          if (serverResponse) {
            serverResponse.message = text;
          }
          this.addSystemMessage(serverResponse);
        }
      }),
      catchError(error => {
        console.error('Error sending message:', error);
        this.addSystemMessage({message:'Sorry, there was an error processing your message. Please try again.'});
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
      this.aiAssistantService.toggleAccessibility(textToSpeech, sessionId).subscribe(response => {
          this.isLoading.set(false);
          if (response?.body?.success) {
            this.textToSpeech.set(textToSpeech);
          } else {
            throw new Error('Error with setting text to speech value.');
          }
        });
    }
  }
}
