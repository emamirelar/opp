import {Injectable, signal, ViewChild, ViewContainerRef, effect} from '@angular/core';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';
import { SessionData } from '../../../../features/internal/models/ai-assistant.model';
import { Observable, of, throwError, Subject } from 'rxjs';
import { map, catchError, tap, switchMap, finalize, filter } from 'rxjs/operators';
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

// Removed ProgressiveRenderEvent - no longer needed


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
  readonly isFirstPageLoad = signal<boolean>(true); // Track if this is first page load vs manual new chat

  // Subject to emit chat history changes (for scrolling to bottom)
  private _chatHistoryChanged = new Subject<void>();
  chatHistoryChanged$ = this._chatHistoryChanged.asObservable();

  // Streaming state tracking - persists across streaming events for the same message
  private streamingSessionActive = false;
  private lastChunkType: string | null = null;
  private lastWasPartial = false;
  private chunkTypeOrder = 0;

  // Removed progressive rendering subject - content renders directly from arrays

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

    // Mark as no longer first page load when user sends first message
    if (this.isFirstPageLoad()) {
      this.isFirstPageLoad.set(false);
    }

    // Reset streaming types for new prompt
    this.resetStreamingTypes();

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
    
    // Reset streaming types when starting a new conversation
    this.resetStreamingTypes();
    
    // Reset streaming session tracking
    this.streamingSessionActive = false;
    this.lastChunkType = null;
    this.lastWasPartial = false;
    this.chunkTypeOrder = 0;
    console.log('🗑️ Conversation cleared - reset streaming tracking variables');
  }

  private resetStreamingTypes(): void {
    // This method is called when starting a new conversation to reset
    // any streaming state. Since streaming state is per-message and not global,
    // this serves as a marker for when we start fresh conversations.
    console.log('🔄 Reset streaming types for new conversation');
  }

  // Removed triggerProgressiveRender - content renders directly from arrays

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
        isFromHistory: true, // Messages from session history should not have typewriter effect
        inlineData: (chat.inlineData || chat.InlineData) ? (chat.inlineData || chat.InlineData).map((inline: any) => {
          console.log('Processing inline data:', inline);
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
      (message.result && message.result.length > 0) ||
      (message.inlineData && message.inlineData.length > 0)
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
    console.log('   Suggested user responses:', responseData.suggestedUserResponses?.length || 0);
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
      suggestedUserResponses: responseData.suggestedUserResponses || [],
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
    files?: File[], 
    state?: any
  ): Observable<void> {
    // Track if we've created a streaming message for this conversation
    let streamingMessage: ChatMessage | null = null;
    let messageIndex = -1;
    
    // Track separate arrays for each chunk type and their arrival order
    let streamingChunkTypes: {
      thoughts: any[];
      functionCall: any[];
      functionResponse: any[];
      markdown: any[];
      mermaid: any[];
      chart: any[];
      card: any[];
      [key: string]: any[]; // Allow for additional types
    } = {
      thoughts: [],
      functionCall: [],
      functionResponse: [],
      markdown: [],
      mermaid: [],
      chart: [],
      card: []
    };
    
    // Initialize streaming session tracking
    this.streamingSessionActive = true;
    this.lastChunkType = null;
    this.lastWasPartial = false;
    this.chunkTypeOrder = 0;
    
    console.log('🚀 Starting new message stream - reset instance tracking variables');

    // Use streaming response method
    return this.aiAssistantService.chatWithFilesStreaming(
      message,
      sessionId,
      files,
      state
    ).pipe(
      tap(({ data, complete }: { data: any, complete: boolean }) => {
        // Process streaming data chunks as they come in
        this.processStreamingData(data, complete, (streamingContent) => {
          if (!streamingMessage) {
            // Create initial streaming message with separate arrays for chunk types
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
              // Add separate arrays for each chunk type
              streamingTypes: {
                thoughts: [],
                functionCall: [],
                functionResponse: [],
                markdown: [],
                mermaid: [],
                chart: [],
                card: []
              }
            } as ChatMessage & { streamingTypes: typeof streamingChunkTypes };
            this.addMessage(streamingMessage);
            messageIndex = this.chatHistory().length - 1;
          }
          
          // Process each streaming chunk with type-specific handling
          if (streamingContent && streamingContent.result && streamingMessage) {
            const messageWithTypes = streamingMessage as ChatMessage & { streamingTypes: typeof streamingChunkTypes };
            
            // Track changes for conditional chatHistory updates
            let hasChanges = false;
            
            // Track the final chunk type and partial state for updating tracking variables
            let finalChunkType = this.lastChunkType;
            let finalWasPartial = this.lastWasPartial;
            
            // Track changes by chunk type to enable selective updates
            let changedChunkTypes = new Set<string>();
            
            streamingContent.result.forEach((newChunk: any) => {
              const chunkType = this.normalizeChunkType(newChunk.type);
              const isPartial = newChunk.partial === true; // Explicitly check for true, undefined/missing = false
              const invocationId = newChunk.invocationId || streamingContent.invocationId || data.invocationId || `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
              
              console.log('🌊 Processing chunk:', {
                type: chunkType,
                partial: isPartial,
                rawPartial: newChunk.partial,
                invocationId: invocationId,
                content: this.getMessagePreview(newChunk.message),
                lastChunkType: this.lastChunkType,
                lastWasPartial: this.lastWasPartial
              });
              
              // Handle multiple items per chunk type with proper stream completion logic
              
              // Ensure the chunk type array exists
              if (!messageWithTypes.streamingTypes[chunkType]) {
                messageWithTypes.streamingTypes[chunkType] = [];
              }
              
              // Determine if this chunk continues the current stream or starts a new one
              // IMPORTANT: Use the original tracking variables, not the updated ones from previous parts in this chunk
              const hasExistingItems = messageWithTypes.streamingTypes[chunkType]?.length > 0;
              const lastItem = hasExistingItems ? messageWithTypes.streamingTypes[chunkType][messageWithTypes.streamingTypes[chunkType].length - 1] : null;
              const lastItemCompleted = lastItem ? lastItem.completed === true : false;
              
              // Fix: When partial is undefined/false and we have existing items of the same type, 
              // we should update the last item UNLESS it's already completed
              const isContinuingCurrentStream = (
                chunkType === this.lastChunkType && // Same chunk type as previous
                (this.lastWasPartial === true ||    // Previous chunk was partial, so this continues the stream
                 (hasExistingItems && !lastItemCompleted && !isPartial)) // OR: we have existing items, last isn't completed, and current is not partial (final update)
              );
              
              console.log('🔀 Stream continuation check:', {
                chunkType,
                lastChunkType: this.lastChunkType,
                lastWasPartial: this.lastWasPartial,
                currentPartial: isPartial,
                hasExistingItems,
                lastItemCompleted,
                isContinuingCurrentStream,
                logic: isContinuingCurrentStream ? 'CONTINUE_STREAM' : 'NEW_ITEM'
              });
              
              if (isContinuingCurrentStream && messageWithTypes.streamingTypes[chunkType].length > 0) {
                // CONTINUE CURRENT STREAM: Update the last item in the array
                const currentItemIndex = messageWithTypes.streamingTypes[chunkType].length - 1;
                const currentItem = messageWithTypes.streamingTypes[chunkType][currentItemIndex];
                
                console.log(`📝 Attempting to update ${chunkType}[${currentItemIndex}]:`, {
                  currentItemPartial: currentItem.partial,
                  currentItemCompleted: currentItem.completed,
                  newIsPartial: isPartial,
                  shouldUpdate: !currentItem.completed
                });
                
                // Only update if the current item is not already completed
                if (!currentItem.completed) {
                  // Handle different message types appropriately
                  let updatedMessage = newChunk.message || currentItem.message;
                  let hasContentChanged = false;
                  
                  if (chunkType === 'card' || chunkType === 'functionResponse') {
                    // For structured data (cards, function responses), replace completely
                    updatedMessage = newChunk.message || currentItem.message;
                    hasContentChanged = JSON.stringify(currentItem.message) !== JSON.stringify(newChunk.message);
                  } else {
                    // For text content (markdown, thoughts), concatenate
                    const currentText = String(currentItem.message || '');
                    const newText = String(newChunk.message || '');
                    if ( !isPartial ) {
                      updatedMessage = newText;
                    } else {
                      updatedMessage = currentText + newText;
                    }
                    hasContentChanged = currentText !== newText;
                  }
                  
                  const willBeCompleted = isPartial === false || isPartial === undefined;
                  
                  // CRITICAL: For cards, once they're completed, freeze their object reference
                  if (chunkType === 'card' && willBeCompleted && hasContentChanged) {
                    // Update in place to maintain object reference
                    currentItem.message = updatedMessage;
                    currentItem.partial = isPartial;
                    currentItem.timestamp = Date.now();
                    currentItem.completed = true;
                    
                    console.log(`🔒 FROZE card[${currentItemIndex}] object reference - will not change again:`, {
                      renderingId: currentItem.renderingId,
                      completed: true,
                      messageSize: this.getMessagePreview(updatedMessage)
                    });
                    
                    hasChanges = true;
                    changedChunkTypes.add(chunkType);
                  } else {
                    // Standard update for non-cards or non-completed items
                    const updatedItem = {
                      ...currentItem,
                      message: updatedMessage,
                      partial: isPartial,
                      timestamp: hasContentChanged ? Date.now() : currentItem.timestamp,
                      completed: willBeCompleted
                    };
                    
                    messageWithTypes.streamingTypes[chunkType][currentItemIndex] = updatedItem;
                    console.log(`🔄 Updated ${chunkType}[${currentItemIndex}] (continuing stream), size: ${this.getMessagePreview(updatedMessage)}, completed: ${updatedItem.completed}, partial: ${updatedItem.partial}`);
                    
                    if (hasContentChanged) {
                      hasChanges = true;
                      changedChunkTypes.add(chunkType);
                    }
                  }
                } else {
                  // Item is already completed - NEVER modify completed cards
                  if (chunkType === 'card') {
                    console.log(`🔒 Ignoring update for completed card[${currentItemIndex}] - object reference frozen`);
                  } else {
                    // Non-card items can still update partial/completed status
                    const updatedItem = {
                      ...currentItem,
                      partial: isPartial,
                      completed: (isPartial === false || isPartial === undefined) || currentItem.completed
                    };
                    
                    messageWithTypes.streamingTypes[chunkType][currentItemIndex] = updatedItem;
                    console.log(`⏹️ Skipped content update for ${chunkType}[${currentItemIndex}] - stream already completed`);
                  }
                }
              } else {
                // START NEW STREAM: Create a new item in the array
                const newItemIndex = messageWithTypes.streamingTypes[chunkType].length;
                
                console.log(`🚀 Creating NEW item for ${chunkType}[${newItemIndex}]:`, {
                  reason: !isContinuingCurrentStream ? 
                    (chunkType !== this.lastChunkType ? 'new-type' : 'stream-completed') : 'first-of-type',
                  currentArrayLength: newItemIndex,
                  isContinuingCurrentStream: isContinuingCurrentStream,
                  chunkTypesMatch: chunkType === this.lastChunkType,
                  lastWasPartial: this.lastWasPartial
                });
                
                const newItem = {
                  message: newChunk.message || '',
                  type: chunkType,
                  partial: isPartial,
                  invocationId: invocationId,
                  renderingId: `${invocationId}-${chunkType}-${newItemIndex}`,
                  arrivalOrder: messageWithTypes.streamingTypes[chunkType].length === 0 ? this.chunkTypeOrder++ : messageWithTypes.streamingTypes[chunkType][0].arrivalOrder,
                  timestamp: Date.now(),
                  completed: isPartial === false || isPartial === undefined // Mark as completed when partial is explicitly false or undefined
                };
                
                // Enhanced logging for card completion
                if (chunkType === 'card') {
                  console.log(`🎯 CARD CREATION:`, {
                    chunkType: chunkType,
                    itemIndex: newItemIndex,
                    isPartial: isPartial,
                    rawPartial: newChunk.partial,
                    completed: newItem.completed,
                    renderingId: newItem.renderingId,
                    willFreeze: newItem.completed
                  });
                  
                  // If creating a completed card, immediately freeze its reference
                  if (newItem.completed) {
                    console.log(`🔒 IMMEDIATELY FREEZING new card[${newItemIndex}] - created as completed`);
                  }
                }
                
                messageWithTypes.streamingTypes[chunkType].push(newItem);
                
                const reason = !isContinuingCurrentStream ? 
                  (chunkType !== this.lastChunkType ? 'new-type' : 'stream-completed') : 'first-of-type';
                console.log(`🆕 Created ${chunkType}[${newItemIndex}] (${reason}), size: ${this.getMessagePreview(newItem.message)}, completed: ${newItem.completed}, partial: ${newItem.partial}`);
                hasChanges = true;
                changedChunkTypes.add(chunkType);
              }
              
              // Track final chunk type and partial state (will be used to update tracking variables after processing all parts)
              finalChunkType = chunkType;
              finalWasPartial = isPartial;
            });
            
            // CRITICAL: Update tracking variables AFTER processing all parts in this chunk
            // This ensures multi-part chunks don't interfere with each other's stream continuation logic
            if (finalChunkType !== this.lastChunkType || finalWasPartial !== this.lastWasPartial) {
              console.log(`🔄 Final tracking update: lastChunkType: ${this.lastChunkType} → ${finalChunkType}, lastWasPartial: ${this.lastWasPartial} → ${finalWasPartial}`);
              this.lastChunkType = finalChunkType;
              this.lastWasPartial = finalWasPartial;
            }
            
            // Update other message properties
            streamingMessage.entity = streamingContent.entity || streamingMessage.entity;
            streamingMessage.suggestedUserResponses = streamingContent.suggestedUserResponses || streamingMessage.suggestedUserResponses || [];
            streamingMessage.sources = streamingContent.sources || streamingMessage.sources || [];
            
            const totalChunks = Object.values(messageWithTypes.streamingTypes).reduce((sum, arr) => sum + arr.length, 0);
            const chunkTypeSummary = Object.entries(messageWithTypes.streamingTypes)
              .filter(([_, arr]) => arr.length > 0)
              .map(([chunkType, arr]) => `${chunkType}:${arr.length}`)
              .join(', ');
            
            console.log('📊 Streaming state:', {
              totalChunks,
              chunkTypeSummary,
              multipleItemsPerTypeAllowed: true
            });
            
            // Debug: Show the structure for verification
            Object.entries(messageWithTypes.streamingTypes)
              .filter(([_, arr]) => arr.length > 0)
              .forEach(([type, arr]) => {
                console.log(`📋 ${type}: ${arr.length} items`, arr.map((item, i) => {
                  const status = item.completed ? '(completed)' : (item.partial ? '(partial)' : '(final)');
                  return `[${i}] ${status} ${this.getMessagePreview(item.message)}`;
                }));
              });
            
            // Update the message in the chat history - only when there are actual changes
            // Use more granular updates to prevent unnecessary re-renders of completed content
            
            if (hasChanges) {
              // Special handling to prevent unnecessary re-renders of completed cards
              const hasCompletedCards = Object.entries(messageWithTypes.streamingTypes).some(([chunkType, items]) => 
                chunkType === 'card' && Array.isArray(items) && items.some((item: any) => item.completed === true)
              );
              
              console.log(`🎯 CARD RE-RENDER PREVENTION CHECK:`, {
                hasCompletedCards: hasCompletedCards,
                changedChunkTypes: Array.from(changedChunkTypes),
                cardChanged: changedChunkTypes.has('card'),
                willUseSelectiveUpdate: hasCompletedCards && !changedChunkTypes.has('card'),
                currentCardItems: messageWithTypes.streamingTypes['card']?.length || 0,
                cardObjectReferences: messageWithTypes.streamingTypes['card']?.map((item: any, i: number) => 
                  `[${i}] ${item.renderingId} completed:${item.completed}`
                ) || []
              });
              
              // If we have completed cards and the changes are only to non-card content, 
              // we need to be more careful about the update to maintain object references
              if (hasCompletedCards && !changedChunkTypes.has('card')) {
                console.log('🎯 Selective update: preserving completed card references while updating other content');
                
                this.chatHistory.update(history => {
                  if (messageIndex >= 0 && messageIndex < history.length && streamingMessage) {
                    const updated = [...history];
                    const currentMessage = updated[messageIndex];
                    
                    // Create a new streamingTypes object but preserve card array references if cards haven't changed
                    const newStreamingTypes = { ...messageWithTypes.streamingTypes };
                    
                    // Preserve the existing card array reference to prevent re-rendering
                    if (currentMessage.streamingTypes?.['card'] && !changedChunkTypes.has('card')) {
                      newStreamingTypes['card'] = currentMessage.streamingTypes['card'];
                    }
                    
                    updated[messageIndex] = {
                      ...streamingMessage,
                      streamingTypes: newStreamingTypes,
                      timestamp: new Date() // Update timestamp only when there are actual changes
                    };
                    return updated;
                  }
                  return history;
                });
                console.log('🔄 Updated chat history with selective preservation of card references');
              } else {
                // Standard update for cases where cards have changed or no completed cards exist
                this.chatHistory.update(history => {
                  if (messageIndex >= 0 && messageIndex < history.length && streamingMessage) {
                    const updated = [...history];
                    // Create a completely new message object to ensure change detection
                    updated[messageIndex] = {
                      ...streamingMessage,
                      streamingTypes: { ...messageWithTypes.streamingTypes },
                      timestamp: new Date() // Update timestamp only when there are actual changes
                    };
                    return updated;
                  }
                  return history;
                });
                console.log('🔄 Updated chat history with full update - cards changed or no completed cards');
              }
              
              console.log('📊 Change summary:', {
                totalChanges: hasChanges,
                changedChunkTypes: Array.from(changedChunkTypes),
                hasCompletedCards: hasCompletedCards,
                updateStrategy: hasCompletedCards && !changedChunkTypes.has('card') ? 'selective' : 'full'
              });
            } else {
              console.log('⏭️ Skipped chat history update - no content changes');
            }
          }
        });
        
        // Note: isLoading and streaming session tracking reset is now handled in finalize operator
        if (complete) {
          console.log('🏁 Stream marked as complete - processing finished');
        }
      }),
      // Remove the filter - we want to process all chunks, but only complete the Observable when done
      filter(({ complete }: { data: any, complete: boolean }) => complete), // Only complete when done
      map(() => void 0), // Convert to void
      catchError(error => {
        console.error('Error in streaming response:', error);
        this.isLoading.set(false);
        this.addSystemMessage({message:'Sorry, there was an error processing your message. Please try again.'});
        return of();
      }),
      finalize(() => {
        // Only set loading to false when the Observable actually completes
        this.isLoading.set(false);
        
        // Reset streaming session tracking
        this.streamingSessionActive = false;
        this.lastChunkType = null;
        this.lastWasPartial = false;
        this.chunkTypeOrder = 0;
        console.log('🏁 Streaming session completed - reset tracking variables');
      })
    );
  }

  private normalizeChunkType(type: string): string {
    // Normalize chunk types to consistent naming
    if (!type) return 'markdown';
    
    const lowerType = type.toLowerCase();
    switch (lowerType) {
      case 'thought':
      case 'thoughts':
        return 'thoughts';
      case 'functioncall':
      case 'function_call':
      case 'function-call':
        return 'functionCall';
      case 'functionresponse':
      case 'function_response':
      case 'function-response':
        return 'functionResponse';
      case 'markdown':
      case 'text':
        return 'markdown';
      case 'mermaid':
        return 'mermaid';
      case 'chart':
      case 'graph':
        return 'chart';
      case 'card':
        return 'card';
      default:
        return lowerType; // Allow for additional types
    }
  }

  private getMessagePreview(message: any): string {
    if (typeof message === 'string') {
      return message.substring(0, 50) + (message.length > 50 ? '...' : '');
    }
    if (Array.isArray(message)) {
      return `Array(${message.length} items)`;
    }
    if (message && typeof message === 'object') {
      return `Object(${Object.keys(message).length} keys)`;
    }
    return String(message || 'empty');
  }

  private processStreamingData(data: any, isComplete: boolean, onStreamingContent?: (content: any) => void): void {
    console.log('🌊 Processing streaming data:', {
      role: data.content?.role,
      complete: isComplete,
      partsCount: data.content?.parts?.length || 0,
      hasFunctionResponse: data.content?.parts?.some((p: any) => p.functionResponse) || false
    });
    
    // Handle different types of streaming data
    if (data.content?.parts) {
      // Check if this is a functionResponse chunk (can come with role='user')
      const hasFunctionResponse = data.content.parts.some((part: any) => part.functionResponse);
      console.log('🔍 FunctionResponse check:', hasFunctionResponse);
      
      // Extract content from partial chunks for real-time streaming
      const shouldProcess = (!isComplete && data.content.role === 'model') || hasFunctionResponse;
      console.log('🎯 Should process:', shouldProcess, {
        modelAndNotComplete: !isComplete && data.content.role === 'model',
        hasFunctionResponse: hasFunctionResponse
      });
      
      if (shouldProcess) {
        const streamingResponse = this.buildResponseFromStreamingData(data);
        if (streamingResponse && onStreamingContent) {
          console.log('🔄 Streaming partial content:', streamingResponse);
          onStreamingContent(streamingResponse);
        } else if (!hasFunctionResponse) {
          // Only handle partial responses for model responses, not functionResponse
          this.handlePartialStreamingResponse(data);
        }
      }
      
      // Handle final complete response (user echo - just cleanup)
      if (isComplete && !hasFunctionResponse) {
        console.log('🏁 Stream completed - final user echo received');
        // No additional processing needed for the user echo
      }
    }
    
    // Handle session ID updates from streaming
    if (data.session_id && !this.currentSessionId()) {
      console.log('New session created from streaming:', data.session_id);
      this.currentSessionId.set(data.session_id);
            // Refresh sessions list to include the new session
            this.loadUserSessions().subscribe();
            // Navigate to the new session URL
      this.router.navigate(['/ai', data.session_id], { replaceUrl: true });
    }
  }

  private buildResponseFromStreamingData(data: any): any {
    console.log('🏗️ Building response from streaming data:', {
      role: data.content?.role,
      partsCount: data.content?.parts?.length || 0,
      hasFunctionResponse: data.content?.parts?.some((p: any) => p.functionResponse) || false,
      invocationId: data.invocationId
    });
    
    // Build a response object from streaming data that matches the expected format
    const response: {
      result: any[];
      entity: any;
      suggestedUserResponses: any[];
      sources: any[];
      invocationId?: string;
    } = {
      result: [],
      entity: null,
      suggestedUserResponses: [],
      sources: []
    };

    if (data.content?.parts) {
      // Extract the root-level partial flag and invocationId
      const rootPartial = data.partial === true; // Root level partial flag
      const rootInvocationId = data.invocationId; // Root level invocation ID
      
      console.log('🔍 buildResponseFromStreamingData - root level flags:', {
        partial: rootPartial,
        rawPartial: data.partial,
        invocationId: rootInvocationId
      });
      
      // Process parts to build result array
      const resultItems: any[] = [];
      let hasUserFacingContent = false;
      
      for (const part of data.content.parts) {
        // Include all text content EXCEPT functionCall content
        if (part.text && !part.functionCall) {
          const isThought = part.thought === true;
          
          // Render ALL text content including thoughts, but mark them appropriately
          // CRITICAL: Include the root-level partial flag and invocationId in each result item
          resultItems.push({
            type: isThought ? 'thought' : 'markdown',
            message: part.text, // Keep original formatting including leading/trailing whitespace
            partial: rootPartial,           // Transfer root-level partial flag
            invocationId: rootInvocationId  // Transfer root-level invocation ID
          });
          hasUserFacingContent = true;
          
          if (isThought) {
            console.log('💭 Thought content (displayed):', part.text.substring(0, 100) + '...', 'partial:', rootPartial);
          } else {
            console.log('📝 Regular content (displayed):', part.text.substring(0, 100) + '...', 'partial:', rootPartial);
          }
        }
        
        // Handle function calls (these are NOT displayed but could be tracked for debugging)
        if (part.functionCall) {
          console.log('🔧 Function call (not displayed):', part.functionCall.name, 'partial:', rootPartial);
          // If we later want to track function calls, we'd add them to resultItems here with the partial flag
        }
        
        // Handle function responses
        if (part.functionResponse) {
          console.log('🔧 Function response detected:', {
            name: part.functionResponse.name,
            hasResponse: !!part.functionResponse.response,
            hasResult: !!part.functionResponse.response?.result,
            partial: rootPartial
          });
          
          // Special handling for invoke_api_tool responses
          if (part.functionResponse.name === 'invoke_api_tool' && part.functionResponse.response?.result) {
            try {
              console.log('📊 Processing invoke_api_tool response for display');
              const parsedResult = JSON.parse(part.functionResponse.response.result);
              
              // Extract records from the response for the card data
              let cardData = parsedResult;
              if (parsedResult.response?.records) {
                console.log('📋 Found records in response.records, extracting for card');
                cardData = parsedResult.response.records;
              } else if (parsedResult.records) {
                console.log('📋 Found records at root level, extracting for card');
                cardData = parsedResult.records;
              } else {
                console.log('📋 No records found, using full parsed result for card');
              }
              
              console.log('🎯 Card data extracted:', {
                recordCount: Array.isArray(cardData) ? cardData.length : 'not array',
                dataType: typeof cardData,
                hasRecords: !!(parsedResult.response?.records || parsedResult.records),
                sampleRecord: Array.isArray(cardData) && cardData.length > 0 ? Object.keys(cardData[0] || {}) : 'no sample'
              });
              
              // Try to determine entity type from the API call or data structure
              let entityType = 'partner'; // Default fallback to partner
              if (parsedResult.api_call?.includes('/api/partner')) {
                entityType = 'partner';
              } else if (parsedResult.api_call?.includes('/api/contact')) {
                entityType = 'contact';
              } else if (parsedResult.api_call?.includes('/api/interaction')) {
                entityType = 'interaction';
              }
              
              // Add the parsed result as a card type to be displayed
              resultItems.push({
                type: 'card',
                message: cardData,                // Use extracted records array directly as the message
                entity: entityType,              // Set entity type for proper grid configuration
                partial: false,                  // Always false for completed function responses
                invocationId: rootInvocationId, // Transfer root-level invocation ID
                completed: true,                // Mark as completed
                parsedData: cardData,           // Include extracted card data for rendering  
                fullResponse: parsedResult      // Keep full response for reference if needed
              });
              hasUserFacingContent = true;
              
              console.log('✅ Added invoke_api_tool result as card type for display:', {
                entityType: entityType,
                recordCount: Array.isArray(cardData) ? cardData.length : 'not array',
                apiCall: parsedResult.api_call
              });
            } catch (error) {
              console.error('❌ Failed to parse invoke_api_tool response result:', error);
              console.error('❌ Raw result:', part.functionResponse.response.result);
            }
          } else {
            console.log('ℹ️ FunctionResponse not processed:', {
              name: part.functionResponse.name,
              isInvokeApiTool: part.functionResponse.name === 'invoke_api_tool',
              hasValidResponse: !!(part.functionResponse.response?.result)
            });
          }
        }
      }
      
      if (hasUserFacingContent) {
        response.result = resultItems;
        response.invocationId = rootInvocationId; // Include invocationId at response level too
      }
    }

    console.log('🏗️ Built streaming response:', {
      resultCount: response.result.length,
      hasPartialItems: response.result.some((item: any) => item.partial === true),
      invocationId: response.invocationId
    });

    return response.result.length > 0 ? response : null;
  }

  private processFinalStreamingResponse(responseData: any): void {
    console.log('🎯 Processing final streaming response:', responseData);
    
    // Check if response contains a structured result
    if (responseData.result && Array.isArray(responseData.result)) {
      console.log('✅ STREAMING - Processing structured result array:', responseData.result);
      this.addStructuredMessage(responseData);
        } else {
      // Fallback for text-only responses
      const textContent = this.extractTextFromStreamingData(responseData);
      if (textContent) {
        this.addSystemMessage({message: textContent});
      }
    }
  }

  private handlePartialStreamingResponse(data: any): void {
    // For now, we'll just log partial responses
    // In the future, we could implement real-time updates to the UI
    console.log('🔄 Partial streaming response (thoughts/intermediate):', data);
  }

  private extractTextFromStreamingData(data: any): string {
    if (data.content?.parts) {
      const textParts = data.content.parts
        .filter((part: any) => part.text && !part.thought && !part.functionCall)
        .map((part: any) => part.text) // Keep original formatting including leading/trailing whitespace
        .filter((text: string) => text.length > 0);
      
      return textParts.join('\n\n');
    }
    return '';
  }

  private processEventsResponse(parsedResponse: any): void {
    if (parsedResponse.events && Array.isArray(parsedResponse.events)) {
      console.log('Events array:', parsedResponse.events);
      
      // Get the last event from the events array that contains user-facing text
      const events = parsedResponse.events;
      const resultItems: any[] = [];
      
      // Look for the last event with actual user-facing content
      for (let i = events.length - 1; i >= 0; i--) {
        const event = events[i];
        if (event?.content?.parts) {
          // Process ALL parts in the parts array, not just the first one
          for (const part of event.content.parts) {
            if (part?.text && typeof part.text === 'string' && part.text.trim()) {
              const text = part.text; // Keep original formatting including leading/trailing whitespace
              // Skip agent-to-agent communications
              if (!text.includes('[user_request_agent]') && 
                  !text.includes('For context:') && 
                  text.trim() !== 'Processing...') {
                
                // Check if this is a thought or regular content
                const isThought = part.thought === true;
                
                // Create a result item for this text
                const resultItem = {
                  type: isThought ? 'thought' : 'markdown',
                  message: text
                };
                
                resultItems.push(resultItem);
                console.log(`Adding ${isThought ? 'thought' : 'regular'} content:`, text);
              }
            }
          }
          
          // If we found any content, stop looking at earlier events
          if (resultItems.length > 0) {
            break;
          }
        }
      }
      
      if (resultItems.length > 0) {
        console.log('Adding structured message with', resultItems.length, 'items:', resultItems);
        
        // Create structured message with all text parts
        const structuredMessage = {
          result: resultItems,
          entity: null,
          suggestedUserResponses: [],
          sources: []
        };
        
        this.addStructuredMessage(structuredMessage);
        return;
      }
      
      // Fallback: if no content found in the new way, try the old way
      let userFacingText = '';
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
        console.log('Adding user-facing message (fallback):', userFacingText);
        
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
          /\{[\s\S]*"suggestedUserResponses"[\s\S]*\}/,       // Look for any JSON with "suggestedUserResponses" property
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

