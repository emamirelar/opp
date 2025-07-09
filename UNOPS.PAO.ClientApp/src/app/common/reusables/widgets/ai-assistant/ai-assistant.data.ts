import {Injectable, signal, ViewChild, ViewContainerRef, effect} from '@angular/core';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';
import { SessionData } from '../../../../features/internal/models/ai-assistant.model';
import { Observable, of, throwError, Subject } from 'rxjs';
import { map, catchError, tap, switchMap, finalize } from 'rxjs/operators';
import {
  ChatMessage,
  ChatFile,
  AiResponse,
  getUrlPageByAiResponseCategory
} from './ai-assistant.model';
import { Router } from '@angular/router';
import { ComponentResolverService } from '../../../../features/internal/services/component-resolver.service';

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
export class AiAssistantData {
  readonly chatHistory = signal<ChatMessage[]>([]);
  readonly currentSessionId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly isLoadingSession = signal(false); // NEW: for session loading only
  readonly sessionTitle = signal<string>('New Chat');
  readonly sessionStarred = signal<boolean>(false);
  readonly sessionArchived = signal<boolean>(false);
  readonly userSessions = signal<SessionData[]>([]);
  readonly isLoadingSessions = signal(false);
  private viewContainerRef?: ViewContainerRef; // Store ViewContainerRef
  currentModelMessage: any = {};
  textToSpeech = signal(false);
  private _titleGeneratedForSession: { [key: string]: boolean } = {};
  private _isLoadingPastChat = false; // Flag to track when loading past chats

  // Subject to emit chat history changes (for scrolling to bottom)
  private _chatHistoryChanged = new Subject<void>();
  chatHistoryChanged$ = this._chatHistoryChanged.asObservable();

  constructor(
    private aiAssistantService: AiAssistantService,
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
    // Don't auto-load sessions - let the welcome screen show by default
    // Sessions will be loaded when the user opens the chat history dropdown
  }

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

    this.addUserMessage(message, files);
    this.isLoading.set(true);

    // Allow empty sessionId for new conversations - backend will create one
    const sessionId = this.currentSessionId() || '';
    return this.sendMessageToServer(sessionId, message, files[0]?.file, state);
  }

  public clearConversation(): void {
    this.chatHistory.set([]);
    this.sessionTitle.set('New Chat');
    this.sessionStarred.set(false);
    this.sessionArchived.set(false);
    
    const sessionId = this.currentSessionId();
    if (sessionId) {
      // End current session but don't create a new one
      this.aiAssistantService.endSession(sessionId).subscribe({
        next: () => {
          this.currentSessionId.set(null);
          this.isLoading.set(false);
          // Refresh sessions list
          this.loadUserSessions().subscribe();
        },
        error: (error) => {
          console.error('Failed to end session:', error);
          this.currentSessionId.set(null);
          this.isLoading.set(false);
        }
      });
    } else {
      // No active session, just reset state
      this.currentSessionId.set(null);
      this.isLoading.set(false);
    }
  }

  private loadOrCreateSession(): Observable<void> {
    this.isLoading.set(true);

    return this.aiAssistantService.getUserSessions().pipe(
      switchMap(sessionsResponse => {
        // Store sessions list
        if (sessionsResponse?.body && Array.isArray(sessionsResponse.body)) {
          this.userSessions.set(sessionsResponse.body);
        }
        
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
    this._isLoadingPastChat = true; // Set flag to indicate loading past chat
    return this.fetchSessionDetails(lastSession.id).pipe(
      finalize(() => {
        this._isLoadingPastChat = false; // Reset flag after loading
      })
    );
  }

  private fetchSessionDetails(sessionId: string): Observable<void> {
    this.isLoadingSession.set(true); // Set session loading flag
    return this.aiAssistantService.getSessionDetails(sessionId).pipe(
      tap(detailsResponse => {
        const sessionData = detailsResponse?.body as unknown as SessionWithChats;
        if (sessionData?.session) {
          // Update session details
          this.sessionTitle.set(sessionData.session.title);
          this.sessionStarred.set(sessionData.session.starred);
          this.sessionArchived.set(sessionData.session.archived);
          
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

  private processNewChatHistory(chatMessages: any[]): ChatMessage[] {
    console.log('RAW chatMessages:', chatMessages);

    const mapped = chatMessages.map(chat => {
      const message: ChatMessage = {
        text: chat.text ? this.parseMessageContent(chat.text) : '',
        isUser: chat.role === 'user',
        timestamp: new Date(),
        files: [],
        isFromHistory: true // Messages from session history should not have typewriter effect
      };
      // For model messages, check if the text contains structured data
      if (!message.isUser && message.text) {
        try {
          const parsed = JSON.parse(message.text);
          if (parsed.result && Array.isArray(parsed.result)) {
            message.result = parsed.result;
            message.entity = parsed.entity;
            message.followUps = parsed.followUps || [];
            message.sources = parsed.sources || []; // Extract sources from historical data
            message.text = ''; // Clear text since we have structured content
          }
        } catch (e) {
          // Not JSON, keep as regular text
        }
      }
      return message;
    });

    console.log('MAPPED messages:', mapped);

    const filtered = mapped.filter(message =>
      (message.text && message.text.trim() !== '') ||
      (message.result && message.result.length > 0)
    );

    console.log('FILTERED messages:', filtered);

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
      isFromHistory: false // User messages from sendMessage are always new
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
      isFromHistory: this._isLoadingPastChat // Set flag based on loading state
    });
  }

  private addStructuredMessage(responseData: any): void {
    console.log('🎯 ADDING STRUCTURED MESSAGE');
    console.log('   Result items:', responseData.result?.length || 0);
    console.log('   Follow-ups:', responseData.followUps?.length || 0);
    console.log('   Entity:', responseData.entity);

    // Transform markdown result items with mermaid code blocks into separate mermaid items
    let newResult: any[] = [];
    if (responseData.result && Array.isArray(responseData.result)) {
      responseData.result.forEach((item: any) => {
        if (item.type === 'markdown' && typeof item.message === 'string') {
          // Regex to find all mermaid code blocks
          const mermaidRegex = /```mermaid\n([\s\S]*?)```/g;
          let lastIndex = 0;
          let match;
          let found = false;
          while ((match = mermaidRegex.exec(item.message)) !== null) {
            found = true;
            // Add any markdown before this code block as a markdown item
            if (match.index > lastIndex) {
              const before = item.message.slice(lastIndex, match.index);
              if (before.trim()) {
                newResult.push({ type: 'markdown', message: before });
              }
            }
            // Add the mermaid code block as a mermaid item
            newResult.push({ type: 'mermaid', message: match[1] });
            lastIndex = mermaidRegex.lastIndex;
          }
          // Add any markdown after the last code block
          if (found && lastIndex < item.message.length) {
            const after = item.message.slice(lastIndex);
            if (after.trim()) {
              newResult.push({ type: 'markdown', message: after });
            }
          }
          // If no mermaid code blocks, keep the original item
          if (!found) {
            newResult.push(item);
          }
        } else {
          newResult.push(item);
        }
      });
    } else {
      newResult = responseData.result || [];
    }

    const structuredMessage = {
      text: '', // Will be populated from result items
      isUser: false,
      timestamp: new Date(),
      files: [],
      result: newResult,
      entity: responseData.entity,
      followUps: responseData.followUps || [],
      sources: responseData.sources || [],
      isFromHistory: this._isLoadingPastChat // Set flag based on loading state
    };

    console.log('   Final message object:', structuredMessage);
    this.addMessage(structuredMessage);
    console.log('✅ STRUCTURED MESSAGE ADDED TO CHAT HISTORY');
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
        this.aiAssistantService.generateTitle(this.currentSessionId()!).subscribe({
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
            console.error('Failed to generate session title:', err);
          }
        });
      }
      return updated;
    });
  }

  private sendMessageToServer(
    sessionId: string, 
    message: string, 
    file?: File, 
    state?: any
  ): Observable<void> {
    const formData = new FormData();
    formData.append("sessionId", sessionId);
    formData.append("message", message);
    if (file) {
        formData.append("file", file);
    }
    if (state) {
        formData.append("state", JSON.stringify(state));
    }
    // TODO: Add the file to the formData
    return this.aiAssistantService.chat(formData).pipe(
      tap(response => {
        this.isLoading.set(false);
        const serverResponse: any = response.body;
        
        console.log('Raw server response:', serverResponse);
        console.log('Response type:', typeof serverResponse);
        
        // Model response is always expected to be in JSON format
        if (serverResponse) {
          let parsedResponse: any;
          
          // Handle direct JSON object response (Angular HTTP client auto-parses JSON)
          if (typeof serverResponse === 'object') {
            console.log('Processing JSON object response directly');
            parsedResponse = serverResponse;
          }
          // Handle response as string (need to parse JSON)
          else if (typeof serverResponse === 'string') {
            console.log('Parsing JSON string response');
            try {
              parsedResponse = JSON.parse(serverResponse);
            } catch (e) {
              console.error('Error parsing JSON response:', e);
              // If it's not valid JSON, treat it as plain text
              this.addSystemMessage({message: serverResponse});
              return;
            }
          }
          else {
            console.error('Unexpected response type:', typeof serverResponse);
            this.addSystemMessage({message: 'Sorry, I received an unexpected response format. Please try again.'});
            return;
          }
          
          console.log('Parsed response structure:', parsedResponse);
          
          // Check if response contains a new sessionId (for new conversations)
          if (parsedResponse.session_id && !this.currentSessionId()) {
            console.log('New session created:', parsedResponse.session_id);
            this.currentSessionId.set(parsedResponse.session_id);
            // Refresh sessions list to include the new session
            this.loadUserSessions().subscribe();
          }

          // Process the parsed JSON response - check for new format first
          if (parsedResponse.result && Array.isArray(parsedResponse.result)) {
            console.log('✅ DIRECT JSON - Processing structured result array:', parsedResponse.result);
            console.log('✅ DIRECT JSON - Follow-ups:', parsedResponse.followUps);
            console.log('✅ DIRECT JSON - Adding structured message...');
            this.addStructuredMessage(parsedResponse);
          } else if (parsedResponse.events && Array.isArray(parsedResponse.events)) {
            console.log('🔄 EVENTS - Processing events from JSON response');
            this.processEventsResponse(parsedResponse);
          } else if (parsedResponse.message) {
            console.log('📝 LEGACY - Processing legacy message format');
            // Fallback for older response format
            const text = this.parseMessageContent(parsedResponse.message);
            this.addSystemMessage({message: text});
          } else {
            console.error('❌ No result, events or message found in JSON response:', parsedResponse);
            console.log('Available properties:', Object.keys(parsedResponse));
            
            // Try to extract structured data from any possible location
            let foundStructuredData = false;
            
            // Check if any property contains structured data
            for (const key in parsedResponse) {
              const value = parsedResponse[key];
              if (typeof value === 'string') {
                try {
                  const possibleJson = JSON.parse(value);
                  if (possibleJson.result && Array.isArray(possibleJson.result)) {
                    console.log('✅ EXTRACTED - Found structured data in property:', key);
                    this.addStructuredMessage(possibleJson);
                    foundStructuredData = true;
                    break;
                  }
                } catch (e) {
                  // Not JSON, continue checking other properties
                }
              }
            }
            
            if (!foundStructuredData) {
              // Show raw response as fallback
              this.addSystemMessage({message: JSON.stringify(parsedResponse, null, 2)});
            }
          }
        } else {
          console.error('No response received from server');
          this.addSystemMessage({message: 'Sorry, I did not receive a response from the server. Please try again.'});
        }
      }),
      catchError(error => {
        console.error('Error sending message:', error);
        this.isLoading.set(false);
        this.addSystemMessage({message:'Sorry, there was an error processing your message. Please try again.'});
        return of();
      }),
      map(() => void 0)
    );
  }

  private processEventsResponse(parsedResponse: any): void {
    if (parsedResponse.events && Array.isArray(parsedResponse.events)) {
      console.log('Events array:', parsedResponse.events);
      
      // Get the last event from the events array that contains user-facing text
      const events = parsedResponse.events;
      let userFacingText = '';
      
      // Look for the last event with actual user-facing content
      for (let i = events.length - 1; i >= 0; i--) {
        const event = events[i];
        if (event?.content?.parts?.[0]?.text) {
          const text = event.content.parts[0].text;
          // Skip agent-to-agent communications
          if (!text.includes('[user_request_agent]') && 
              !text.includes('For context:') && 
              text.trim() !== 'Processing...') {
            userFacingText = text;
            break;
          }
        }
      }
      
      if (userFacingText) {
        console.log('Adding user-facing message:', userFacingText);
        
        // Check if the text contains JSON wrapped in markdown code blocks
        console.log('🔍 EVENTS - Checking for JSON in markdown code blocks...');
        console.log('🔍 EVENTS - Text to check:', userFacingText);
        
        // Try multiple regex patterns to catch different markdown formats
        let jsonMatch = userFacingText.match(/```json\s*([\s\S]*?)\s*```/);
        if (!jsonMatch) {
          // Try without spaces around json
          jsonMatch = userFacingText.match(/```json([\s\S]*?)```/);
        }
        if (!jsonMatch) {
          // Try with escaped newlines
          jsonMatch = userFacingText.match(/```json\\n([\s\S]*?)\\n```/);
        }
        if (!jsonMatch) {
          // Try matching the exact format from your example
          jsonMatch = userFacingText.match(/```json\\n\{([^`]*)\}\\n```/);
          if (jsonMatch) {
            // Reconstruct the full JSON object
            jsonMatch[1] = '{' + jsonMatch[1] + '}';
          }
        }
        if (!jsonMatch) {
          // Try a more flexible pattern for any content between ```json and ```
          jsonMatch = userFacingText.match(/```json[^{]*(\{[\s\S]*?\})[^}]*```/);
        }
        console.log('🔍 EVENTS - JSON match result:', jsonMatch);
        
        if (jsonMatch) {
          try {
            console.log('✅ EVENTS - Found JSON in markdown code block, extracting...');
            const extractedJson = jsonMatch[1].trim();
            console.log('✅ EVENTS - Extracted JSON:', extractedJson);
            
            // Handle multiple levels of escaped characters in the JSON
            let cleanedJson = extractedJson;
            
            // First pass: handle standard JSON escaping
            cleanedJson = cleanedJson
              .replace(/\\n/g, '\n')
              .replace(/\\"/g, '"')
              .replace(/\\'/g, "'")
              .replace(/\\\\/g, '\\');
            
            console.log('✅ EVENTS - After first cleaning pass:', cleanedJson);

            // Final step: Escape any remaining literal newlines within string values for valid JSON
            // This regex finds string values and escapes newlines within them
            let finalJson = cleanedJson;
            try {
              // Replace literal newlines within quoted strings with \\n
              finalJson = cleanedJson.replace(/"([^"]*?)"/g, (match, content) => {
                // Escape newlines, carriage returns, and tabs within string content
                const escapedContent = content
                  .replace(/\n/g, '\\n')
                  .replace(/\r/g, '\\r')
                  .replace(/\t/g, '\\t');
                return `"${escapedContent}"`;
              });
              
              console.log('✅ EVENTS - Final JSON with escaped newlines:', finalJson);
            } catch (e) {
              console.warn('⚠️ EVENTS - Could not escape string newlines, using original');
              finalJson = cleanedJson;
            }

            cleanedJson = finalJson;
            
            // Second pass: handle double-escaped content within strings
            try {
              // Parse once to get the structure
              const tempParsed = JSON.parse(cleanedJson);
              if (tempParsed.result && Array.isArray(tempParsed.result)) {
                tempParsed.result.forEach((item: any) => {
                  if (item.message && typeof item.message === 'string') {
                    // Clean double-escaped newlines in message content
                    item.message = item.message
                      .replace(/\\\\n/g, '\n')
                      .replace(/\\n/g, '\n');
                  }
                });
                cleanedJson = JSON.stringify(tempParsed);
                console.log('✅ EVENTS - After message content cleaning:', cleanedJson);
              }
            } catch (e) {
              console.log('⚠️ EVENTS - Could not pre-process message content, using basic cleaning');
            }
            
            console.log('✅ EVENTS - Cleaned JSON:', cleanedJson);
            
            
            const structuredData = JSON.parse(finalJson);
            
            if (structuredData.result && Array.isArray(structuredData.result)) {
              console.log('✅ EVENTS - Processing extracted structured data:', structuredData);
              this.addStructuredMessage(structuredData);
              return;
            }
          } catch (e) {
            console.error('❌ EVENTS - Failed to parse JSON from markdown code block:', e);
            console.error('❌ EVENTS - Raw extracted text:', jsonMatch[1]);
          }
        }
        
        // Check if the text itself is direct JSON (starts with { and ends with })
        const trimmedText = userFacingText.trim();
        if (trimmedText.startsWith('{') && trimmedText.endsWith('}')) {
          try {
            console.log('🔍 EVENTS - Detected direct JSON, attempting to parse...');
            console.log('🔍 EVENTS - Raw text to parse:', trimmedText);
            
            // Handle escaped newlines and quotes in the JSON string
            let cleanedJson = trimmedText;
            try {
              // Try parsing as-is first
              const structuredData = JSON.parse(cleanedJson);
              
              if (structuredData.result && Array.isArray(structuredData.result)) {
                console.log('✅ EVENTS - Successfully parsed direct JSON structured data:', structuredData);
                this.addStructuredMessage(structuredData);
                return;
              }
            } catch (parseError) {
              console.log('🔧 EVENTS - Initial parse failed, trying to clean escaped characters...');
              // If that fails, try to handle common escaping issues
              cleanedJson = trimmedText
                .replace(/\\n/g, '\n')
                .replace(/\\"/g, '"')
                .replace(/\\'/g, "'")
                .replace(/\\\\/g, '\\');
              
              const structuredData = JSON.parse(cleanedJson);
              
              if (structuredData.result && Array.isArray(structuredData.result)) {
                console.log('✅ EVENTS - Successfully parsed cleaned JSON structured data:', structuredData);
                this.addStructuredMessage(structuredData);
                return;
              }
            }
          } catch (e) {
            console.error('❌ EVENTS - Failed to parse direct JSON after cleaning:', e);
            console.error('❌ EVENTS - Original text:', trimmedText);
          }
        }
        
        // Additional check: try to find JSON patterns even if not perfectly formatted
        // This handles cases where the JSON might be wrapped in extra text or not perfectly formatted
        const jsonPatterns = [
          /\{[\s\S]*"result"[\s\S]*\}/,           // Look for any JSON with "result" property
          /\{[\s\S]*"followUps"[\s\S]*\}/,       // Look for any JSON with "followUps" property
          /\{[\s\S]*"type"[\s\S]*"markdown"[\s\S]*\}/  // Look for markdown type responses
        ];
        
        for (const pattern of jsonPatterns) {
          const match = userFacingText.match(pattern);
          if (match) {
            try {
              console.log('🔍 EVENTS - Found JSON pattern, attempting to parse...');
              const potentialJson = match[0];
              console.log('🔍 EVENTS - Potential JSON:', potentialJson);
              
              const structuredData = JSON.parse(potentialJson);
              
              if (structuredData.result && Array.isArray(structuredData.result)) {
                console.log('✅ EVENTS - Successfully parsed pattern-matched JSON:', structuredData);
                this.addStructuredMessage(structuredData);
                return;
              }
            } catch (e) {
              console.log('⚠️ EVENTS - Pattern match failed to parse:', e);
              continue; // Try next pattern
            }
          }
        }
        
        // Fallback to regular message processing
        this.addSystemMessage({message: userFacingText});
      } else {
        console.warn('No user-facing text found in events');
        this.addSystemMessage({message: 'I processed your request but have no response to display.'});
      }
    } else {
      console.error('Invalid events structure:', parsedResponse);
      this.addSystemMessage({message: 'Sorry, I received an unexpected response format.'});
    }
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

  public toggleStar(): Observable<void> {
    const sessionId = this.currentSessionId();
    if (!sessionId) {
      return of();
    }

    const newStarredState = !this.sessionStarred();
    
    return this.aiAssistantService.updateSessionStar(sessionId, newStarredState).pipe(
      tap(response => {
        if (response?.body?.success) {
          this.sessionStarred.set(newStarredState);
        }
      }),
      catchError(error => {
        console.error('Error updating star status:', error);
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
    
    return this.aiAssistantService.updateSessionArchive(sessionId, newArchivedState).pipe(
      tap(response => {
        if (response?.body?.success) {
          this.sessionArchived.set(newArchivedState);
        }
      }),
      catchError(error => {
        console.error('Error updating archive status:', error);
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

    return this.aiAssistantService.updateSessionTitle(sessionId, newTitle.trim()).pipe(
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
        console.error('Error updating title:', error);
        return of();
      }),
      map(() => void 0)
    );
  }

  public loadUserSessions(): Observable<void> {
    this.isLoadingSessions.set(true);
    
    return this.aiAssistantService.getUserSessions().pipe(
      tap(response => {
        if (response?.body && Array.isArray(response.body)) {
          this.userSessions.set(response.body);
        }
      }),
      catchError(error => {
        console.error('Error loading sessions:', error);
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
    this.isLoadingSession.set(true); // Set session loading flag
    this.isLoading.set(true);
    this._isLoadingPastChat = true; // Set flag to indicate loading past chat
    this.currentSessionId.set(sessionId);
    
    return this.fetchSessionDetails(sessionId).pipe(
      finalize(() => {
        this.isLoading.set(false);
        this.isLoadingSession.set(false); // Reset session loading flag
        this._isLoadingPastChat = false; // Reset flag after loading
      })
    );
  }

  public updateSessionStar(sessionId: string, starred: boolean): Observable<void> {
    return this.aiAssistantService.updateSessionStar(sessionId, starred).pipe(
      tap(response => {
        if (response?.body?.success) {
          // Update current session state if it matches
          if (sessionId === this.currentSessionId()) {
            this.sessionStarred.set(starred);
          }
          // Update the session in the list
          this.userSessions.update(sessions => 
            sessions.map(session => 
              session.id === sessionId 
                ? { ...session, starred: starred } as any
                : session
            )
          );
        }
      }),
      catchError(error => {
        console.error('Error updating star status:', error);
        return of();
      }),
      map(() => void 0)
    );
  }

  public updateSessionArchive(sessionId: string, archived: boolean): Observable<void> {
    return this.aiAssistantService.updateSessionArchive(sessionId, archived).pipe(
      tap(response => {
        if (response?.body?.success) {
          // Update current session state if it matches
          if (sessionId === this.currentSessionId()) {
            this.sessionArchived.set(archived);
          }
          // Update the session in the list
          this.userSessions.update(sessions => 
            sessions.map(session => 
              session.id === sessionId 
                ? { ...session, archived: archived } as any
                : session
            )
          );
        }
      }),
      catchError(error => {
        console.error('Error updating archive status:', error);
        return of();
      }),
      map(() => void 0)
    );
  }
}
