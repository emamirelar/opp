import {Component, ViewChild, ElementRef, Input, ViewContainerRef, inject, effect, OnInit, OnDestroy, AfterViewInit, NgZone, ChangeDetectorRef, Output, EventEmitter} from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { TooltipModule } from 'primeng/tooltip';
import { MenuModule } from 'primeng/menu';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { signal, computed } from '@angular/core';
import { LayoutService } from '../../../layouts/services/layout.service';
import { AiAssistantScanComponent } from './scan/ai-assistant-scan.component';
import { SafeUrlPipe } from './safe-url.pipe';
import { Router } from '@angular/router';
import { GlobalFilterService } from '../../../../services/global-filter.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../essentials/services/auth.service';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';
import { ChatSession, ChatMessage, ChatFile } from './ai-assistant.model';
import { Observable, map, catchError, of } from 'rxjs';
import { DynamicContentService } from './dynamic-content.service';
import { PageContextService } from '../../../services/page-context.service';

@Component({
  selector: 'app-ai-assistant-panel',
  templateUrl: './ai-assistant-panel.component.html',
  standalone: true,
  styleUrls: ['./ai-assistant-panel.component.css'],
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    ButtonModule,
    TextareaModule,
    ScrollPanelModule,
    TooltipModule,
    MenuModule,
    TranslatePipe,
    AiAssistantScanComponent,
    SafeUrlPipe,
    // ContentRendererComponent - removed, now created dynamically
  ]
})
export class AiAssistantPanelComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  @ViewChild('dynamicContentContainer', { read: ViewContainerRef }) private dynamicContentContainer!: ViewContainerRef;
  @ViewChild('scanComponent') private scanComponent!: AiAssistantScanComponent;
  @ViewChild('sessionMenu') private sessionMenu!: Menu;
  @Input() viewContainerRef!: ViewContainerRef;
  @Input() hideHeader: boolean = false; // Hide header in fullscreen mode
  @Input() rightPanelEntityType: string | null = null; // Entity type in right panel
  @Input() rightPanelEntityId: string | null = null; // Entity ID in right panel
  @Input() mode: 'overlay' | 'fullscreen' = 'overlay'; // Mode determines card click behavior
  @Output() cardClicked = new EventEmitter<any>();

  // Mobile detection
  isMobile = computed(() => {
    if (typeof window !== 'undefined') {
      return window.innerWidth <= 768;
    }
    return false;
  });

  // Mobile keyboard detection
  private initialViewportHeight = signal(0);
  isMobileKeyboardActive = computed(() => {
    if (typeof window !== 'undefined' && this.isMobile()) {
      const currentHeight = window.visualViewport?.height || window.innerHeight;
      const initialHeight = this.initialViewportHeight();
      // Consider keyboard active if viewport height decreased by more than 150px
      return initialHeight > 0 && (initialHeight - currentHeight) > 150;
    }
    return false;
  });

  // UNIFIED MODEL STATE - Use ChatSession from service
  currentChatSession = computed(() => this.aiAssistantService.currentChatSession());
  chatMessages = computed(() => this.currentChatSession()?.chatMessages || []);
  
  firstScroll = signal(true);
  message = signal('');
  selectedFiles = signal<{ file: File, name: string, content: string }[]>([]);
  isProcessingFile = signal(false);
  isDragging = signal(false);
  loading = signal(false);
  layoutService = inject(LayoutService);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);
  private dynamicContentService = inject(DynamicContentService);
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  isRecording = signal(false);
  audioBlob = signal<Blob | null>(null);
  isEditingTitle = signal(false);
  editingTitle = signal('');
  sessionMenuItems = signal<MenuItem[]>([]);
  
  // Dynamic dots for generating message
  generatingDots = signal(1);
  private generatingInterval?: number;
  
  // User info for personalized greeting
  userName = signal<string>('');
  
  // Resize listener reference for cleanup
  private resizeListener?: () => void;
  private visualViewportListener?: () => void;
  
  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();
  
  // Buffer for chunks that arrive before ViewChild is available
  private chunkBuffer: any[] = [];
  private isSwitchingSession: boolean = false; // Flag to prevent reactive effect during session switching
  private viewInitialized = false;
  private hasRenderedInitialMessages = false; // Flag to prevent duplicate rendering of initial messages
  private isCurrentlyRendering = false; // Flag to prevent concurrent rendering calls
  
  // Check if AI is currently in fullscreen mode (on AI route)
  isInFullscreenMode = computed(() => {
    return this.router.url.startsWith('/ai');
  });
  
  // Example prompts for welcome message - initialized after translation service is available
  examplePrompts: { text: string, icon: string, category: string }[] = [];

  constructor(
    public aiAssistantService: AiAssistantService,
    private router: Router,
    private globalFilterService: GlobalFilterService,
    private http: HttpClient,
    private authService: AuthService,
    private translateService: TranslateService,
    private pageContextService: PageContextService
  ) {
    // Effects must be in constructor (injection context)
    
    // Manual session menu building - call when sessions change
    effect(() => {
      const sessions = this.aiAssistantService.userSessions();
      this.buildSessionMenuItems();
    });
    
    // Manual dots animation management - call when loading state changes
    effect(() => {
      const isLoading = this.aiAssistantService.isLoading();
      if (isLoading) {
        this.startGeneratingDotsAnimation();
      } else {
        this.stopGeneratingDotsAnimation();
      }
    });
    
    // Reactive rendering effect - render messages when they become available
    effect(() => {
      const messages = this.chatMessages();
      const isLoading = this.aiAssistantService.isLoading();
      
      // Only render if:
      // 1. We have messages
      // 2. Not currently loading/streaming
      // 3. Haven't rendered these messages yet
      // 4. View is initialized
      // 5. Not switching sessions
      if (messages.length > 0 && 
          !isLoading && 
          !this.hasRenderedInitialMessages && 
          this.viewInitialized && 
          !this.isSwitchingSession) {
        // Use setTimeout to ensure this runs after the current change detection cycle
        setTimeout(() => {
          this.renderExistingMessages();
        }, 100);
      }
    });
  }

  ngOnInit(): void {
    this.message.set('');
    this.loadUserInfo();
    // this.initializeExamplePrompts();
    
    if (this.viewContainerRef) {
      this.aiAssistantService.setViewContainerRef(this.viewContainerRef);
    }
    
    // Load user sessions on initialization
    this.aiAssistantService.loadUserSessions().subscribe({
      next: () => {
        this.buildSessionMenuItems();
      },
      error: (error) => console.error('Failed to load initial sessions:', error)
    });
    
    // Manual scroll handling - no automatic effects
    this.aiAssistantService.chatHistoryChanged$.subscribe(() => {
      // Use a small delay to ensure the DOM has updated
      setTimeout(() => {
        this.scrollToBottom(true); // Use smooth scroll for new messages
      }, 100);
    });

    // Listen for streaming chunks and process them with dynamic content service
    this.aiAssistantService.streamingChunk$
      .pipe(takeUntil(this.destroy$))
      .subscribe((chunk: any) => {
        if (chunk) {
          if (this.viewInitialized && this.dynamicContentContainer) {
            // ViewChild is available
            this.dynamicContentService.setViewContainer(this.dynamicContentContainer);
            this.dynamicContentService.setCardClickCallback(this.onCardClicked.bind(this));
            
            // Check if there are buffered chunks that need to be processed first
            if (this.chunkBuffer.length > 0) {
              // Process all buffered chunks first
              this.chunkBuffer.forEach((bufferedChunk, index) => {
                this.dynamicContentService.processChunk(bufferedChunk);
              });
              
              // Clear the buffer
              this.chunkBuffer = [];
            }
            
            // ALWAYS process the current chunk (it's not in the buffer)
            this.dynamicContentService.processChunk(chunk);
          } else {
            // ViewChild not yet available, buffer the chunk
            this.chunkBuffer.push(chunk);
          }
        }
      });

    // Listen for window resize to update mobile detection
    if (typeof window !== 'undefined') {
      // Initialize viewport height for keyboard detection
      this.initialViewportHeight.set(window.visualViewport?.height || window.innerHeight);
      
      this.resizeListener = () => {
        this.cdr.markForCheck();
      };
      window.addEventListener('resize', this.resizeListener);
      
      // Listen for visual viewport changes (keyboard show/hide)
      if (window.visualViewport) {
        const visualViewportListener = () => {
          this.cdr.markForCheck();
        };
        window.visualViewport.addEventListener('resize', visualViewportListener);
        
        // Store the listener for cleanup
        this.visualViewportListener = visualViewportListener;
      }
    }
    
    this.cdr.detectChanges();
  }

  ngAfterViewInit(): void {
    // Mark view as initialized and process any buffered chunks
    this.viewInitialized = true;
    
    if (this.dynamicContentContainer) {
      this.dynamicContentService.setViewContainer(this.dynamicContentContainer);
      this.dynamicContentService.setCardClickCallback(this.onCardClicked.bind(this));
      
      // Process any buffered chunks first
      if (this.chunkBuffer.length > 0) {
        this.chunkBuffer.forEach((chunk, index) => {
          this.dynamicContentService.processChunk(chunk);
        });
        this.chunkBuffer = [];
      }
      
      // Check if there are existing messages in the session that need to be rendered
      // This handles the case when switching between sidebar and fullscreen modes
      const existingMessages = this.chatMessages();
      if (existingMessages.length > 0 && !this.aiAssistantService.isLoading() && !this.hasRenderedInitialMessages) {
        // Use setTimeout to ensure the view is fully initialized
        setTimeout(() => {
          this.renderExistingMessages();
        }, 50);
      }
    }
  }

  ngOnDestroy(): void {
    this.stopGeneratingDotsAnimation();
    
    // Complete the destroy subject to unsubscribe from all observables
    this.destroy$.next();
    this.destroy$.complete();
    
    // Clean up window resize listener
    if (typeof window !== 'undefined' && this.resizeListener) {
      window.removeEventListener('resize', this.resizeListener);
    }
    
    // Clean up visual viewport listener
    if (typeof window !== 'undefined' && window.visualViewport && this.visualViewportListener) {
      window.visualViewport.removeEventListener('resize', this.visualViewportListener);
    }
  }

  // Handle closing the AI Assistant
  closeAiAssistant(): void {
    // Check if we're on the AI route
    if (this.router.url.startsWith('/ai')) {
      if (this.isMobile()) {
        // On mobile, navigate back to the previous route immediately
        const previousRoute = this.getPreviousRoute();
        this.router.navigate([previousRoute], { replaceUrl: true });
      } else {
        // On desktop, navigate back to home and open AI assistant in popup mode
        this.router.navigate(['/']);
        // After navigation, open the AI assistant in popup mode
        setTimeout(() => {
          this.layoutService.onAIAssistantToggle();
        }, 100);
      }
    } else {
      // In overlay mode, close the overlay
      this.layoutService.onAIAssistantToggle();
    }
  }

  // Get previous route for mobile navigation
  private getPreviousRoute(): string {
    // Try to get from session storage first
    const storedRoute = sessionStorage.getItem('ai-assistant-previous-route');
    if (storedRoute && storedRoute !== '/ai') {
      return storedRoute;
    }
    return '/'; // Default fallback
  }



  updateMessage(value: string): void {
    this.ngZone.run(() => {
      this.message.set(value);
      this.cdr.detectChanges();
    });
  }

  onFileSelect(event: any): void {
    this.isProcessingFile.set(true);
    const files = event.files || event.target?.files || (event.dataTransfer?.files);

    if (!files?.length) {
      this.isProcessingFile.set(false);
      return;
    }

    this.processFiles([files[0]]);

    if (event.target?.value) {
      event.target.value = '';
    }
  }

  onPaste(event: ClipboardEvent): void {
    const clipboardData = event.clipboardData;
    if (!clipboardData) return;

    // Check if there are any files in the clipboard
    const files = Array.from(clipboardData.files);
    
    if (files.length > 0) {
      // Filter for image files
      const imageFiles = files.filter(file => file.type.startsWith('image/'));
      
      if (imageFiles.length > 0) {
        // Prevent default paste behavior for images
        event.preventDefault();
        
        // Process the first image file
        this.isProcessingFile.set(true);
        this.processFiles([imageFiles[0]]);
        
        // Clear any existing message text since we're sending an image
        if (this.message().trim() === '') {
          // Optionally show a placeholder message that an image was pasted
        }
      }
    }
    
    // If no image files, let the default paste behavior handle text
  }

  private async processFiles(files: File[]): Promise<void> {
    try {
      const contents = await Promise.all(files.map(file => this.readFileAsBase64(file)));
      this.selectedFiles.set([{ file: files[0], name: files[0].name, content: '' }]);
    } catch (error) {
      console.error('Error processing files:', error);
    } finally {
      this.isProcessingFile.set(false);
    }
  }

  private readFileAsBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = error => reject(error);
      reader.readAsDataURL(file);
    });
  }

  removeFile(index: number): void {
    this.selectedFiles.update(files => files.filter((_, i) => i !== index));
  }

  // Drag and drop handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const files = event.dataTransfer?.files;
    if (files) {
      this.onFileSelect({ target: { files } });
    }
  }

  // Webcam methods
  startCamera(): void {
    this.scanComponent.show();
  }

  onImageCaptured(file: File): void {
    this.processFiles([file]);
  }

  // Extract current route from hash-based or path-based routing
  private extractCurrentRoute(): string {
    try {
      const hash = window.location.hash;
      const pathname = window.location.pathname;
      const search = window.location.search;
      
      // Check if using hash-based routing
      if (hash && hash.startsWith('#/')) {
        // Extract route from hash (remove the # and keep the /)
        return hash.substring(1) + search;
      }
      
      // Fallback to path-based routing
      return pathname + search;
    } catch (error) {
      console.error('Error extracting current route:', error);
      return '/';
    }
  }

  // UNIFIED MESSAGE HANDLING - Works with ChatSession model
  sendMessage(): void {
    // Ensure view container is available before processing
    if (!this.dynamicContentContainer) {
      console.warn('⚠️ Dynamic content container not available, cannot send message');
      return;
    }

    const currentMessage = this.message();
    const currentFiles = this.selectedFiles();

    if (currentMessage.trim() || currentFiles.length > 0) {
      this.ngZone.run(() => {
        this.message.set('');
        this.cdr.detectChanges();
      });

      const chatFiles: ChatFile[] = currentFiles.map(f => ({
        file: f.file,
        name: f.name,
        content: ''
      }));

      // Build enhanced state object with screen context parameters for the enhanced screen context agent
      const state = this.buildMessageState();

      // 1. Create proper USER message object (same as service)
      const userMessage: ChatMessage = {
        id: this.generateId(),
        timestamp: Date.now(),
        invocationId: this.generateInvocationId(),
        role: "user",
        content: {
          parts: [{ 
            text: currentMessage,
            partial: false // User messages are always complete
          }],
          role: "user"
        },
        actions: { stateDelta: {}, artifactDelta: {}, requestedAuthConfigs: {} },
        longRunningToolIds: [],
        isUser: true,
        files: chatFiles,
        sources: [],
        suggestedUserResponses: []
      };

      // 2. Add to current ChatSession's chatMessages array (single source of truth)
      const currentSession = this.aiAssistantService.currentChatSession();
      if (currentSession) {
        currentSession.chatMessages.push(userMessage);
        // Manually emit chat history change for new user message
        this.aiAssistantService.emitChatHistoryChanged();
      } else {
        // Create new ChatSession if none exists
        const newTitle = currentMessage.substring(0, 200) + (currentMessage.length > 200 ? "..." : "");
        this.aiAssistantService.currentChatSession.set({
          session: {
            id: '', // Will be set by server
            timestamp: Date.now(),
            userId: this.getCurrentUserId(),
            status: "Active",
            title: newTitle,
            starred: false,
            archived: false
          },
          chatMessages: [userMessage]
        });
        // Update the sessionTitle signal immediately
        this.aiAssistantService.sessionTitle.set(newTitle);
        // Manually emit chat history change for new session
        this.aiAssistantService.emitChatHistoryChanged();
      }

      // Mark as no longer first page load when user sends first message
      if (this.aiAssistantService.isFirstPageLoad()) {
        this.aiAssistantService.isFirstPageLoad.set(false);
      }

      // 3. Process user message immediately for instant feedback
      if (this.dynamicContentContainer) {
        this.dynamicContentService.setViewContainer(this.dynamicContentContainer);
        this.dynamicContentService.setCardClickCallback(this.onCardClicked.bind(this));
        this.dynamicContentService.processChunk(userMessage);
      }

      // 4. Send to server directly (service will handle streaming response)
      const sessionId = this.aiAssistantService.currentSessionId() || '';
      this.aiAssistantService.isLoading.set(true);
      this.aiAssistantService.sendMessageToServer(sessionId, userMessage, chatFiles, state).subscribe({
        next: () => {
          this.ngZone.run(() => {
            this.selectedFiles.set([]);
            this.cdr.detectChanges();
          });
        },
        error: (error) => {
          console.error('Failed to send message:', error);
          this.ngZone.run(() => {
            this.cdr.detectChanges();
          });
        }
      });
    }
  }

  private scrollToBottom(smooth = true): void {
    try {
      requestAnimationFrame(() => {
        setTimeout(() => {
          const chatContainer = this.chatContainer?.nativeElement;
          if (chatContainer) {
            const scrollHeight = chatContainer.scrollHeight;
            if (scrollHeight) {
              chatContainer.scrollTo({
                top: scrollHeight,
                behavior: smooth ? 'smooth' : 'instant'
              });
            }
          }
        }, 50);
      });
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  public isWaitingResponse(): boolean {
    return this.aiAssistantService.isLoading();
  }

  public async startRecording(): Promise<void> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream, {
        mimeType: 'audio/webm'
      });
      this.audioChunks = [];

      this.mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          this.audioChunks.push(event.data);
        }
      };

      this.mediaRecorder.onstop = () => {
        const audioBlob = new Blob(this.audioChunks, { type: 'audio/mpeg' });
        this.audioBlob.set(audioBlob);
        this.processAudioMessage(audioBlob);
      };

      this.mediaRecorder.start();
      this.isRecording.set(true);
    } catch (error) {
      console.error('Error starting recording:', error);
    }
  }

  public stopRecording(): void {
    if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
      this.mediaRecorder.stop();
      this.isRecording.set(false);
      this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
  }

  private async processAudioMessage(audioBlob: Blob): Promise<void> {
    try {
      const base64Audio = await this.blobToBase64(audioBlob);
      this.selectedFiles.set([{ file: new File([audioBlob], 'audio-message.mp3', { type: 'audio/mpeg' }), name: 'audio-message.mp3', content: '' }]);
    } catch (error) {
      console.error('Error processing audio message:', error);
    }
  }

  private blobToBase64(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        if (typeof reader.result === 'string') {
          resolve(reader.result);
        } else {
          reject(new Error('Failed to convert blob to base64'));
        }
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  // Title editing methods
  startEditingTitle(): void {
    this.editingTitle.set(this.aiAssistantService.sessionTitle());
    this.isEditingTitle.set(true);
    // Focus the input after the view updates
    setTimeout(() => {
      const titleInput = document.querySelector('input[placeholder="Enter title"]') as HTMLInputElement;
      if (titleInput) {
        titleInput.focus();
        titleInput.select();
      }
    }, 0);
  }

  saveTitle(): void {
    const newTitle = this.editingTitle().trim();
    if (newTitle && newTitle !== this.aiAssistantService.sessionTitle()) {
      this.aiAssistantService.updateTitle(newTitle).subscribe({
        next: () => {
          this.isEditingTitle.set(false);
        },
        error: (error) => {
          console.error('Failed to update title:', error);
          this.isEditingTitle.set(false);
        }
      });
    } else {
      this.isEditingTitle.set(false);
    }
  }

  cancelEditingTitle(): void {
    this.isEditingTitle.set(false);
    this.editingTitle.set('');
  }

  // Star and Archive methods
  toggleStar(): void {
    this.aiAssistantService.toggleStar().subscribe({
      error: (error) => console.error('Failed to toggle star:', error)
    });
  }

  toggleArchive(): void {
    this.aiAssistantService.toggleArchive().subscribe({
      error: (error) => console.error('Failed to toggle archive:', error)
    });
  }

  // Follow-up response handling
  selectUserResponse(followUpText: string): void {
    this.message.set(followUpText);
    // Focus the textarea for user convenience
    setTimeout(() => {
      const textarea = document.querySelector('textarea[pTextarea]') as HTMLTextAreaElement;
      if (textarea) {
        textarea.focus();
      }
    }, 0);
  }

  // Get suggested user responses from the most recent message using unified model
  getLatestSuggestedUserResponses(): string[] {
    const chatMessages = this.chatMessages();
    if (chatMessages.length === 0) return [];
    
    // Get the most recent AI message (not user message)
    for (let i = chatMessages.length - 1; i >= 0; i--) {
      const message = chatMessages[i];
      if (message.role === "model" && message.suggestedUserResponses && message.suggestedUserResponses.length > 0) {
        return message.suggestedUserResponses;
      }
    }
    
    return [];
  }

  // Session dropdown methods
  toggleSessionMenu(event: Event): void {
    // Refresh sessions when opening dropdown
    if (!this.sessionMenu.visible) {
      this.aiAssistantService.loadUserSessions().subscribe({
        error: (error) => console.error('Failed to load sessions:', error)
      });
    }
    this.sessionMenu.toggle(event);
  }

  private buildSessionMenuItems(): void {
    const sessions = this.aiAssistantService.userSessions();
    const currentSessionId = this.aiAssistantService.currentSessionId();
    
    const validSessions = sessions.filter(session => session.id);
    
    if (validSessions.length === 0) {
      // Show placeholder when no sessions exist
      const menuItems: MenuItem[] = [{
        label: this.translateService.instant('aiAssistant.noChatsAvailable'),
        icon: 'pi pi-inbox',
        disabled: true,
        styleClass: 'text-gray-500'
      }];
      this.sessionMenuItems.set(menuItems);
      return;
    }
    
    // Sort sessions by lastUpdated timestamp in descending order (most recent first)
    const sortedSessions = validSessions
      .sort((a, b) => {
        const aTime = a.lastUpdated || a.startTime || 0;
        const bTime = b.lastUpdated || b.startTime || 0;
        // Handle both numeric timestamps and date strings for backward compatibility
        const aTimestamp = typeof aTime === 'number' ? aTime : new Date(aTime).getTime();
        const bTimestamp = typeof bTime === 'number' ? bTime : new Date(bTime).getTime();
        return bTimestamp - aTimestamp;
      })
      .slice(0, 50); // Limit to 50 most recent chats for performance
    
    const menuItems: MenuItem[] = [
      // Add header
      {
        label: `Recent Chats (${validSessions.length})`,
        icon: 'pi pi-history',
        disabled: true,
        styleClass: 'font-semibold text-sm bg-gray-50 border-b',
        separator: true
      },
      // Add chat sessions
      ...sortedSessions.map(session => ({
        label: session.title || this.translateService.instant('aiAssistant.untitledChat'),
        icon: session.id === currentSessionId ? 'pi pi-check' : 'pi pi-comment',
        command: () => this.switchToSession(session.id!),
        styleClass: session.id === currentSessionId ? 'font-bold bg-blue-50' : '',
        title: session.title || this.translateService.instant('aiAssistant.untitledChat') // Tooltip
      }))
    ];

    this.sessionMenuItems.set(menuItems);
  }

  /**
   * Render existing messages from the current session
   * Used when component is initialized with existing messages (e.g., switching between modes)
   */
  private renderExistingMessages(): void {
    // Prevent concurrent rendering calls
    if (this.isCurrentlyRendering) {
      return;
    }
    
    if (!this.dynamicContentContainer) {
      console.warn('⚠️ Dynamic content container not available, cannot render existing messages');
      return;
    }

    const chatMessages = this.chatMessages();
    if (chatMessages.length === 0) {
      return;
    }

    // Mark as currently rendering
    this.isCurrentlyRendering = true;

    try {
      // Clear any existing components first to avoid duplicates
      this.dynamicContentService.clearAllComponents();
      
      // Ensure view container and callback are set
      this.dynamicContentService.setViewContainer(this.dynamicContentContainer);
      this.dynamicContentService.setCardClickCallback(this.onCardClicked.bind(this));
      
      // Sort messages by timestamp to ensure correct chronological order
      const sortedMessages = [...chatMessages].sort((a, b) => a.timestamp - b.timestamp);
      
      // Process each message through the dynamic content service
      sortedMessages.forEach((message) => {
        this.dynamicContentService.processChunk(message);
      });
      
      // Mark that we've rendered initial messages
      this.hasRenderedInitialMessages = true;
      
      // Scroll to bottom after rendering
      setTimeout(() => {
        this.scrollToBottom(false); // Use instant scroll for existing messages
      }, 100);
    } finally {
      // Reset the rendering flag
      this.isCurrentlyRendering = false;
    }
  }

  switchToSession(sessionId: string): void {
    // Ensure view container is available before switching sessions
    if (!this.dynamicContentContainer) {
      console.warn('⚠️ Dynamic content container not available, cannot switch session');
      return;
    }

    this.isSwitchingSession = true; // Prevent reactive effect from running
    this.sessionMenu.hide();
    this.ngZone.run(() => {
      // Clear existing dynamic components and buffer before loading new session
      this.dynamicContentService.clearAllComponents();
      this.chunkBuffer = [];
      this.hasRenderedInitialMessages = false; // Reset flag for new session
      this.isCurrentlyRendering = false; // Reset rendering flag
      
      // Force a synchronous clearing by triggering change detection
      this.cdr.detectChanges();
      
      // Use requestAnimationFrame to ensure clearing is complete before loading new session
      requestAnimationFrame(() => {
        this.aiAssistantService.switchToSession(sessionId).subscribe({
          next: () => {
            // Use setTimeout to ensure the view is fully rendered and clearing is complete
            setTimeout(() => {
              // Render the loaded messages
              this.renderExistingMessages();
              
              // Reset flag after processing is complete
              setTimeout(() => {
                this.isSwitchingSession = false;
              }, 50); // Small delay to ensure processing is complete
            }, 100);
          },
          error: (error) => {
            console.error('Failed to switch session:', error);
            this.isSwitchingSession = false; // Reset flag on error
          }
        });
      });
    });
  }

  // Copy message text to clipboard
  copyMessageText(message: any): void {
    let textToCopy = '';
    
    if (message.result && message.result.length > 0) {
      // For structured content, extract text from all result items
      textToCopy = message.result.map((item: any) => item.message).join('\n\n');
    } else if (message.text) {
      // For regular text messages
      textToCopy = message.text;
    }
    
    if (!textToCopy) {
      return;
    }

    if (navigator.clipboard && window.isSecureContext) {
      // Use the modern clipboard API
      navigator.clipboard.writeText(textToCopy).then(() => {
        // Could show a toast notification here
      }).catch(err => {
        this.fallbackCopyTextToClipboard(textToCopy);
      });
    } else {
      // Fallback for older browsers
      this.fallbackCopyTextToClipboard(textToCopy);
    }
  }

  private fallbackCopyTextToClipboard(text: string): void {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    
    // Avoid scrolling to bottom
    textArea.style.top = '0';
    textArea.style.left = '0';
    textArea.style.position = 'fixed';
    textArea.style.opacity = '0';
    
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    
    try {
      document.execCommand('copy');
    } catch (err) {
      // Silent error handling for clipboard operations
    }
    
    document.body.removeChild(textArea);
  }

  // Dynamic dots animation methods
  private startGeneratingDotsAnimation(): void {
    if (this.generatingInterval) return; // Already running

    this.generatingDots.set(1);
    this.generatingInterval = window.setInterval(() => {
      this.generatingDots.update(dots => {
        return dots >= 3 ? 1 : dots + 1;
      });
    }, 500); // Change dots every 500ms
  }

  private stopGeneratingDotsAnimation(): void {
    if (this.generatingInterval) {
      clearInterval(this.generatingInterval);
      this.generatingInterval = undefined;
    }
    this.generatingDots.set(1);
  }

  // Get dynamic dots string for generating message
  getGeneratingDotsText(): string {
    return '.'.repeat(this.generatingDots());
  }

  // UNIFIED NAVIGATION - Expand to fullscreen with proper session data handling
  onExpandToFullScreen(): void {
    const currentSession = this.currentChatSession();
    if (currentSession?.session?.id) {
      // Only load user sessions if we don't already have them
      if (this.aiAssistantService.userSessions().length === 0) {
        this.aiAssistantService.loadUserSessions().subscribe();
      }
      
      // Pass current session data to full screen component
      // This avoids reloading from server
      this.router.navigate(['/ai'], {
        queryParams: { sessionId: currentSession.session.id },
        state: { 
          chatSession: currentSession,
          preserveData: true 
        }
      });
    } else {
      // No current session, navigate to AI without session
      this.router.navigate(['/ai']);
    }
  }

  // Open AI assistant in fullscreen mode (legacy method)
  openFullscreen(): void {
    this.onExpandToFullScreen();
  }

  // Minimize AI assistant from fullscreen mode
  minimizeFullscreen(): void {
    // Navigate back to previous route or home page when minimizing from fullscreen
    const previousRoute = this.getPreviousRoute();
    
    // Navigate to the previous route (or home if none exists)
    this.router.navigate([previousRoute === '/ai' ? '/' : previousRoute]).then(() => {
      // After navigation, explicitly open the AI assistant in sidebar/popup mode (not toggle)
      setTimeout(() => {
        this.layoutService.layoutState.update(state => ({
          ...state,
          aiAssistantActive: true
        }));
      }, 100);
    });
  }

  // Toggle between fullscreen and popup modes
  toggleFullscreen(): void {
    if (this.isInFullscreenMode()) {
      this.minimizeFullscreen();
    } else {
      this.openFullscreen();
    }
  }

  // Clear conversation
  clearConversation(): void {
    // Clear dynamic components
    this.dynamicContentService.clearAllComponents();
    
    // Clear any buffered chunks as well
    this.chunkBuffer = [];
    
    // Reset rendering flags
    this.hasRenderedInitialMessages = false;
    this.isCurrentlyRendering = false;
    
    // Force change detection to ensure clearing is complete
    this.cdr.detectChanges();
    
    this.aiAssistantService.clearConversation();
  }

  // Handler for cardClicked event from content-renderer/entity-grid
  onCardClicked(event: { entityType: string, entityId: string, rowData: any }): void {
    // Check if we're on a mobile device
    const isMobile = this.layoutService.isMobile();
    
    if (this.isInFullscreenMode() && !isMobile) {
      // In fullscreen mode on non-mobile devices, emit for right panel
      this.cardClicked.emit(event);
    } else {
      // In overlay/sidebar mode OR on mobile devices (even in fullscreen), navigate to the entity page
      this.navigateToEntity(event.entityType, event.entityId, event.rowData);
    }
  }

  // Get appropriate icon for source based on URL or type
  getSourceIcon(source: any): string {
    if (!source.url) {
      return 'pi pi-file';
    }

    const url = source.url.toLowerCase();
    const title = source.title?.toLowerCase() || '';
    const description = source.description?.toLowerCase() || '';

    // Google Drive/Docs
    if (url.includes('docs.google.com') || url.includes('drive.google.com')) {
      if (url.includes('/document/') || title.includes('document')) {
        return 'pi pi-file-edit';
      } else if (url.includes('/spreadsheets/') || title.includes('spreadsheet')) {
        return 'pi pi-table';
      } else if (url.includes('/presentation/') || title.includes('presentation')) {
        return 'pi pi-chart-bar';
      }
      return 'pi pi-google';
    }

    // PDF files
    if (url.includes('.pdf') || title.includes('.pdf') || description.includes('pdf')) {
      return 'pi pi-file-pdf';
    }

    // Web search
    if (url.includes('google.com/search') || title.includes('google search')) {
      return 'pi pi-search';
    }

    // Local files
    if (title.includes('file:') || description.includes('file:')) {
      return 'pi pi-folder';
    }

    // Default for web links
    return 'pi pi-globe';
  }

  // Open source link in a new tab
  openSourceLink(source: any): void {
    if (source.url) {
      window.open(source.url, '_blank', 'noopener,noreferrer');
    }
  }

  // Navigate to entity page (for overlay mode)
  private navigateToEntity(entityType: string, entityId: string, rowData: any): void {
    const route = this.buildEntityRoute(entityType, entityId, rowData);
    if (route) {
      this.router.navigate([route]);
    }
  }

  // Build entity route (similar to EntityGridComponent.buildEntityUrl)
  private buildEntityRoute(entityType: string, entityId: number | string, rowData: any): string | null {
    switch (entityType?.toLowerCase()) {
      case 'partner':
        return `/partnerships/partners/${entityId}`;
      case 'contact':
        if (rowData.partnerId) {
          return `/partnerships/partners/${rowData.partnerId}/contacts/${entityId}`;
        }
        return `/contacts/${entityId}`;
      case 'interaction':
        return `/interactions/${entityId}`;
      case 'partneragreement':
      case 'partnership':
        return `/partnerships/agreements/${entityId}`;
      default:
        const routeSegment = entityType.toLowerCase().replace(/\s+/g, '-');
        return `/${routeSegment}s/${entityId}`;
    }
  }

  // Helper methods for inline data handling
  getFileTypeCategory(mimeType: string): string {
    if (!mimeType) return 'unknown';
    
    if (mimeType.startsWith('image/')) return 'image';
    if (mimeType.startsWith('audio/')) return 'audio';
    if (mimeType.startsWith('video/')) return 'video';
    if (mimeType === 'application/pdf') return 'pdf';
    if (mimeType.startsWith('text/')) return 'text';
    if (mimeType.includes('document') || 
        mimeType.includes('word') || 
        mimeType.includes('excel') || 
        mimeType.includes('powerpoint') ||
        mimeType.includes('presentation') ||
        mimeType.includes('sheet')) return 'document';
    
    return 'unknown';
  }

  downloadInlineFile(inline: any, defaultFileName: string): void {
    try {
      const byteCharacters = atob(inline.data);
      const byteNumbers = new Array(byteCharacters.length);
      for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);
      const blob = new Blob([byteArray], { type: inline.mimeType });
      
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = defaultFileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      // Silent error handling for file download
    }
  }

  decodeBase64Text(data: string): string {
    try {
      return atob(data);
    } catch (error) {
      return this.translateService.instant('aiAssistant.unableToDecodeText');
    }
  }

  getFileIcon(mimeType: string): string {
    if (!mimeType) return 'pi pi-file text-gray-400';
    
    if (mimeType.includes('word')) return 'pi pi-file-word text-blue-600';
    if (mimeType.includes('excel') || mimeType.includes('sheet')) return 'pi pi-file-excel text-green-600';
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return 'pi pi-file text-orange-600';
    if (mimeType.includes('zip') || mimeType.includes('archive')) return 'pi pi-file-archive text-purple-600';
    
    return 'pi pi-file text-gray-400';
  }

  getFileTypeName(mimeType: string): string {
    if (!mimeType) return this.translateService.instant('aiAssistant.fileTypes.file');
    
    if (mimeType.includes('word')) return this.translateService.instant('aiAssistant.fileTypes.wordDocument');
    if (mimeType.includes('excel') || mimeType.includes('sheet')) return this.translateService.instant('aiAssistant.fileTypes.excelSpreadsheet');
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return this.translateService.instant('aiAssistant.fileTypes.powerpointPresentation');
    if (mimeType.includes('zip')) return this.translateService.instant('aiAssistant.fileTypes.archive');
    if (mimeType.includes('json')) return this.translateService.instant('aiAssistant.fileTypes.jsonFile');
    if (mimeType.includes('xml')) return this.translateService.instant('aiAssistant.fileTypes.xmlFile');
    
    return mimeType.split('/')[1]?.toUpperCase() || this.translateService.instant('aiAssistant.fileTypes.file');
  }

  getFileName(mimeType: string): string {
    if (!mimeType) return 'file';
    
    const extensions: { [key: string]: string } = {
      'application/pdf': 'document.pdf',
      'application/msword': 'document.doc',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': 'document.docx',
      'application/vnd.ms-excel': 'spreadsheet.xls',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': 'spreadsheet.xlsx',
      'application/vnd.ms-powerpoint': 'presentation.ppt',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation': 'presentation.pptx',
      'application/zip': 'archive.zip',
      'application/json': 'data.json',
      'application/xml': 'data.xml',
      'text/plain': 'text.txt',
      'text/csv': 'data.csv'
    };
    
    return extensions[mimeType] || `file.${mimeType.split('/')[1] || 'bin'}`;
  }

  isValidBase64(str: string): boolean {
    if (!str) return false;
    try {
      // Check if it's valid base64
      const decoded = atob(str);
      const reencoded = btoa(decoded);
      return reencoded === str;
    } catch (err) {
      return false;
    }
  }

  analyzeBase64Data(data: string): any {
    if (!data) return { error: 'No data' };
    
    const invalidChars = data.match(/[^A-Za-z0-9+/=]/g);
    const uniqueInvalidChars = [...new Set(invalidChars || [])];
    
    const analysis = {
      length: data.length,
      hasInvalidChars: !/^[A-Za-z0-9+/]*={0,2}$/.test(data),
      invalidCharsCount: invalidChars?.length || 0,
      uniqueInvalidChars: uniqueInvalidChars,
      uniqueInvalidCharCodes: uniqueInvalidChars.map(c => `'${c}' (${c.charCodeAt(0)})`),
      properPadding: data.endsWith('=') || data.endsWith('==') || !data.includes('='),
      firstChars: data.substring(0, 100),
      lastChars: data.substring(data.length - 100),
      sampleInvalidPositions: this.findInvalidCharPositions(data, 10)
    };
    
    // Try to clean and test the data
    const cleaned = this.cleanBase64Data(data);
    
    return analysis;
  }

  findInvalidCharPositions(data: string, maxSamples: number): any[] {
    const samples = [];
    for (let i = 0; i < data.length && samples.length < maxSamples; i++) {
      const char = data[i];
      if (!/[A-Za-z0-9+/=]/.test(char)) {
        samples.push({
          position: i,
          char: char,
          charCode: char.charCodeAt(0),
          context: data.substring(Math.max(0, i-10), i+10)
        });
      }
    }
    return samples;
  }

  cleanBase64Data(data: string): string {
    if (!data) return data;
    
    // Remove any whitespace, newlines, or invalid characters
    let cleaned = data.replace(/[^A-Za-z0-9+/=]/g, '');
    
    // Fix padding if needed
    const remainder = cleaned.length % 4;
    if (remainder > 0) {
      cleaned += '='.repeat(4 - remainder);
    }
    
    return cleaned;
  }

  onImageError(event: any, inline: any): void {
    // Silent error handling for image loading failures
  }

  openImageModal(inline: any): void {
    // Create a modal overlay for viewing large images
    const modal = document.createElement('div');
    modal.className = 'fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-75 cursor-pointer';
    modal.onclick = () => document.body.removeChild(modal);

    const img = document.createElement('img');
    img.src = `data:${inline.mimeType};base64,${inline.data}`;
    img.className = 'max-w-[95vw] max-h-[95vh] object-contain rounded-lg';
    img.onclick = (e) => e.stopPropagation();

    // Add close button
    const closeBtn = document.createElement('button');
    closeBtn.innerHTML = '×';
    closeBtn.className = 'absolute top-4 right-4 text-white text-3xl font-bold bg-black bg-opacity-50 rounded-full w-10 h-10 flex items-center justify-center hover:bg-opacity-75 transition-colors';
    closeBtn.onclick = () => document.body.removeChild(modal);

    modal.appendChild(img);
    modal.appendChild(closeBtn);
    document.body.appendChild(modal);
  }

  /**
   * Load user information for personalized greeting
   */
  private loadUserInfo(): void {
    // Get email from claims to pass as parameter
    this.authService.user().subscribe({
      next: (claims) => {
        const emailClaim = claims.find(c => c.type === 'email' || 
                                     c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress');
        
        const email = emailClaim?.value;
        const apiUrl = email ? `/api/user-info/current?email=${encodeURIComponent(email)}` : '/api/user-info/current';
        
        this.http.get<any>(apiUrl).subscribe({
          next: (response) => {
            // Extract user info from the nested response structure
            const userInfoData = response.userInfoWithOrgSettings || response;
            
            if (userInfoData) {
              const name = userInfoData.name || email || 'User';
              this.userName.set(name);
            } else {
              console.warn('No user info data received from API');
              this.userName.set(this.translateService.instant('aiAssistant.user'));
            }
          },
          error: (error) => {
            this.userName.set('User');
          }
        });
      },
      error: (error) => {
        this.userName.set('User');
      }
    });
  }

  /**
   * Build message state object with screen context parameters and AUTOMATIC page data extraction
   * 
   * NO COMPONENT CHANGES NEEDED - automatically grabs data from the active page component!
   */
  private buildMessageState(): any {
    const baseState = {
      screen_url: this.extractCurrentRoute(),
      user_focus_context: this.rightPanelEntityType && this.rightPanelEntityId ? 
        `/${this.rightPanelEntityType.toLowerCase()}s/${this.rightPanelEntityId}` : '',
      user_email: localStorage.getItem('user_email'),
      url_entity_type: this.rightPanelEntityType || '',
      url_entity_id: this.rightPanelEntityId || '',
      url_section: '',
      url_query_params: window.location.search || '',
      global_filter_enabled: this.globalFilterService.isFilterEnabled(),
      global_org_unit_id: this.globalFilterService.getActiveOrgUnitId()
    };

    // AUTOMATICALLY extract page context from the currently active component
    // This extracts ALL data properties (partner, interactions, contacts, etc.)
    // without requiring ANY component changes!
    const pageContext = this.pageContextService.getPageContextForAI({
      maxArrayLength: 20,  // Limit arrays to 20 items
      maxDepth: 3,         // Limit object depth to 3 levels
      includePrivateProps: false  // Skip private properties
    });

    // Add page context if available
    if (pageContext) {
      return {
        ...baseState,
        page_context_auto: pageContext  // "auto" to distinguish from manual registration
      };
    }

    return baseState;
  }

  // Utility methods for ChatMessage creation
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