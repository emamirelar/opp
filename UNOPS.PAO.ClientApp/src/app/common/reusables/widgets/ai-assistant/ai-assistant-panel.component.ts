import {Component, ViewChild, ElementRef, Input, ViewContainerRef, inject, effect, OnInit, OnDestroy, NgZone, ChangeDetectorRef, Output, EventEmitter} from '@angular/core';
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
import { TranslatePipe } from '@ngx-translate/core';
import { AiAssistantData } from './ai-assistant.data';
import { signal } from '@angular/core';
import { LayoutService } from '../../../layouts/services/layout.service';
import { AiAssistantScanComponent } from './scan/ai-assistant-scan.component';
import { SafeUrlPipe } from './safe-url.pipe';
import { ContentRendererComponent } from './content-renderer/content-renderer.component';
import { Router } from '@angular/router';
import { EntityPanelService } from '../../../services/entity-panel.service';
import { GlobalFilterService } from '../../../../services/global-filter.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../essentials/services/auth.service';

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
    ContentRendererComponent
  ]
})
export class AiAssistantPanelComponent implements OnInit, OnDestroy {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  @ViewChild('scanComponent') private scanComponent!: AiAssistantScanComponent;
  @ViewChild('sessionMenu') private sessionMenu!: Menu;
  @Input() viewContainerRef!: ViewContainerRef;
  @Input() hideHeader: boolean = false; // Hide header in fullscreen mode
  @Input() rightPanelEntityType: string | null = null; // Entity type in right panel
  @Input() rightPanelEntityId: string | null = null; // Entity ID in right panel
  @Input() mode: 'overlay' | 'fullscreen' = 'overlay'; // Mode determines card click behavior
  @Output() cardClicked = new EventEmitter<any>();

  firstScroll = signal(true);
  message = signal('');
  selectedFiles = signal<{ file: File, name: string, content: string }[]>([]);
  isProcessingFile = signal(false);
  isDragging = signal(false);
  loading = signal(false);
  layoutService = inject(LayoutService);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);
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
  
  // Sequential content display
  contentDisplayState = signal<{[messageIndex: number]: number}>({});
  
  // User info for personalized greeting
  userName = signal<string>('');
  
  // Example prompts for welcome message - Gemini style business-specific
  examplePrompts = [
    { 
      text: 'Find qualified partners for infrastructure projects in West Africa', 
      icon: 'pi pi-search',
      category: 'Partner Search'
    },
    { 
      text: 'Create a proposal summary for a climate resilience project', 
      icon: 'pi pi-file-edit',
      category: 'Proposal Writing'
    },
    { 
      text: 'Analyze partnership trends in renewable energy sector', 
      icon: 'pi pi-chart-line',
      category: 'Data Analysis'
    },
    { 
      text: 'Draft an engagement strategy for local NGOs', 
      icon: 'pi pi-users',
      category: 'Engagement'
    },
    { 
      text: 'Review compliance requirements for new partnerships', 
      icon: 'pi pi-shield',
      category: 'Compliance'
    },
    { 
      text: 'Generate a partnership impact report template', 
      icon: 'pi pi-file-pdf',
      category: 'Reporting'
    }
  ];

  constructor(
    public aiAssistantData: AiAssistantData,
    private router: Router,
    private entityPanelService: EntityPanelService,
    private globalFilterService: GlobalFilterService,
    private http: HttpClient,
    private authService: AuthService
  ) {
    effect(() => {
      const chatHistory = this.aiAssistantData.chatHistory();
      if (chatHistory.length > 0) {
        this.scrollToBottom(!this.firstScroll());
      }
      if (this.firstScroll()) {
        this.firstScroll.set(false);
      }
    });

    // Build session menu items whenever sessions or current session change
    effect(() => {
      const sessions = this.aiAssistantData.userSessions();
      const currentSessionId = this.aiAssistantData.currentSessionId();
      this.buildSessionMenuItems();
    });

    // Manage dots animation based on loading state
    effect(() => {
      const isLoading = this.aiAssistantData.isLoading();
      if (isLoading) {
        this.startGeneratingDotsAnimation();
      } else {
        this.stopGeneratingDotsAnimation();
      }
    });

    // Initialize content display for new messages
    effect(() => {
      const chatHistory = this.aiAssistantData.chatHistory();
      chatHistory.forEach((message, messageIndex) => {
        if (!message.isUser && message.result && message.result.length > 0) {
          const displayState = this.contentDisplayState();
          if (!(messageIndex in displayState)) {
            this.initializeContentDisplay(messageIndex, true);
          }
        }
      });
    });
  }

  ngOnInit(): void {
    this.message.set('');
    this.loadUserInfo();
    
    if (this.viewContainerRef) {
      this.aiAssistantData.setViewContainerRef(this.viewContainerRef);
    }
    
    // Listen for chat history changes to scroll to bottom for new messages
    this.aiAssistantData.chatHistoryChanged$.subscribe(() => {
      // Use a small delay to ensure the DOM has updated
      setTimeout(() => {
        this.scrollToBottom(true); // Use smooth scroll for new messages
      }, 100);
    });
    
    this.cdr.detectChanges();
  }

  ngOnDestroy(): void {
    this.stopGeneratingDotsAnimation();
  }

  // Handle closing the AI Assistant
  closeAiAssistant(): void {
    this.layoutService.onAIAssistantToggle();
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

  // Message handling
  sendMessage(): void {
    const currentMessage = this.message();
    const currentFiles = this.selectedFiles();

    if (currentMessage.trim() || currentFiles.length > 0) {
      this.ngZone.run(() => {
        this.message.set('');
        this.cdr.detectChanges();
      });

      const chatFiles = currentFiles.map(f => ({
        file: f.file,
        name: f.name,
        content: ''
      }));

      // Build enhanced state object with screen context parameters for the enhanced screen context agent
      const state = {
        screen_url: this.extractCurrentRoute(),
        user_focus_context: this.rightPanelEntityType && this.rightPanelEntityId ? 
          `/${this.rightPanelEntityType.toLowerCase()}s/${this.rightPanelEntityId}` : '',
        user_email: localStorage.getItem('user_email')
      };

      this.aiAssistantData.sendMessage(currentMessage, chatFiles, state).subscribe({
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

  // Extract URL structure generically by skipping prefixes
  private extractUrlStructure(): {entity: string, id: number|null, section: string|null, queryParams: string} {
    try {
      const url = new URL(window.location.href);
      const pathSegments = url.pathname.split('/').filter(segment => segment.length > 0);
      const queryParams = url.search;
      
      // Skip first segment (prefix) generically
      const meaningfulSegments = pathSegments.slice(1);
      
      let entity = '';
      let id: number | null = null;
      let section: string | null = null;
      
      if (meaningfulSegments.length > 0) {
        // First meaningful segment is the entity
        const rawEntity = meaningfulSegments[0];
        
        // Normalize entity name
        if (rawEntity === 'partners') {
          entity = 'Partner';
        } else if (rawEntity === 'contacts') {
          entity = 'Contact';
        } else if (rawEntity === 'interactions') {
          entity = 'Interaction';
        } else if (rawEntity === 'partner-tree') {
          entity = 'PartnerTree';
        } else {
          // Capitalize first letter for any other entity
          entity = rawEntity.charAt(0).toUpperCase() + rawEntity.slice(1);
        }
        
        // Second segment might be an ID
        if (meaningfulSegments.length > 1) {
          const idSegment = meaningfulSegments[1];
          const parsedId = parseInt(idSegment, 10);
          if (!isNaN(parsedId)) {
            id = parsedId;
          }
        }
        
        // Third segment might be a section
        if (meaningfulSegments.length > 2) {
          section = meaningfulSegments[2];
        }
      }
      
      return {
        entity,
        id,
        section,
        queryParams
      };
      
    } catch (error) {
      console.error('Error extracting URL structure:', error);
      return {
        entity: '',
        id: null,
        section: null,
        queryParams: ''
      };
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
    return this.aiAssistantData.isLoading();
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
    this.editingTitle.set(this.aiAssistantData.sessionTitle());
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
    if (newTitle && newTitle !== this.aiAssistantData.sessionTitle()) {
      this.aiAssistantData.updateTitle(newTitle).subscribe({
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
    this.aiAssistantData.toggleStar().subscribe({
      error: (error) => console.error('Failed to toggle star:', error)
    });
  }

  toggleArchive(): void {
    this.aiAssistantData.toggleArchive().subscribe({
      error: (error) => console.error('Failed to toggle archive:', error)
    });
  }

  // Follow-up suggestion handling
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

  // Get suggested user responses from the most recent message
  getLatestSuggestedUserResponses(): string[] {
    const chatHistory = this.aiAssistantData.chatHistory();
    if (chatHistory.length === 0) return [];
    
    // Get the most recent AI message (not user message)
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const message = chatHistory[i];
      if (!message.isUser && message.suggestedUserResponses && message.suggestedUserResponses.length > 0) {
        return message.suggestedUserResponses;
      }
    }
    
    return [];
  }

  // Session dropdown methods
  toggleSessionMenu(event: Event): void {
    // Refresh sessions when opening dropdown
    if (!this.sessionMenu.visible) {
      this.aiAssistantData.loadUserSessions().subscribe({
        error: (error) => console.error('Failed to load sessions:', error)
      });
    }
    this.sessionMenu.toggle(event);
  }

  private buildSessionMenuItems(): void {
    const sessions = this.aiAssistantData.userSessions();
    const currentSessionId = this.aiAssistantData.currentSessionId();
    
    const validSessions = sessions.filter(session => session.id);
    
    if (validSessions.length === 0) {
      // Show placeholder when no sessions exist
      const menuItems: MenuItem[] = [{
        label: 'No chat history available',
        icon: 'pi pi-inbox',
        disabled: true,
        styleClass: 'text-gray-500'
      }];
      this.sessionMenuItems.set(menuItems);
      return;
    }
    
    // Sort sessions by most recent first and limit to show recent chats
    const sortedSessions = validSessions
      .sort((a, b) => {
        const aTime = (a as any).lastMessageTime || (a as any).startTime || 0;
        const bTime = (b as any).lastMessageTime || (b as any).startTime || 0;
        return new Date(bTime).getTime() - new Date(aTime).getTime();
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
        label: (session as any).title || 'Untitled Chat',
        icon: session.id === currentSessionId ? 'pi pi-check' : 'pi pi-comment',
        command: () => this.switchToSession(session.id!),
        styleClass: session.id === currentSessionId ? 'font-bold bg-blue-50' : '',
        title: (session as any).title || 'Untitled Chat' // Tooltip
      }))
    ];

    this.sessionMenuItems.set(menuItems);
  }

  switchToSession(sessionId: string): void {
    this.sessionMenu.hide();
    this.ngZone.run(() => {
      this.aiAssistantData.switchToSession(sessionId).subscribe({
        error: (error) => console.error('Failed to switch session:', error)
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
      console.warn('No text content to copy');
      return;
    }

    if (navigator.clipboard && window.isSecureContext) {
      // Use the modern clipboard API
      navigator.clipboard.writeText(textToCopy).then(() => {
        // Could show a toast notification here
        console.log('Message copied to clipboard');
      }).catch(err => {
        console.error('Failed to copy text: ', err);
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
      const successful = document.execCommand('copy');
      if (successful) {
        console.log('Message copied to clipboard (fallback)');
      } else {
        console.error('Failed to copy text using fallback method');
      }
    } catch (err) {
      console.error('Failed to copy text: ', err);
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

  // Open AI assistant in fullscreen mode
  openFullscreen(): void {
    // For hash-based routing, we need to construct the URL properly
    const baseUrl = window.location.origin + window.location.pathname;
    const currentSessionId = this.aiAssistantData.currentSessionId();
    
    // Include sessionId in URL if available
    const hashUrl = currentSessionId 
      ? `${baseUrl}#/ai/${currentSessionId}`
      : `${baseUrl}#/ai`;
    
    window.open(hashUrl, '_blank');
  }

  // Sequential content display methods
  shouldShowContentItem(messageIndex: number, itemIndex: number): boolean {
    const displayState = this.contentDisplayState();
    const chatHistory = this.aiAssistantData.chatHistory();
    
    // Find the most recent AI message index
    let mostRecentAiMessageIndex = -1;
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const message = chatHistory[i];
      if (message && !message.isUser && message.result && message.result.length > 0) {
        mostRecentAiMessageIndex = i;
        break;
      }
    }
    
    // If this is not the most recent AI message, show all content immediately
    if (messageIndex !== mostRecentAiMessageIndex) {
      return true;
    }
    
    // For the most recent AI message, apply sequential display
    const visibleItems = displayState[messageIndex] || 0;
    return itemIndex < visibleItems;
  }

  // Check if sources should be displayed (after content is complete)
  shouldShowSources(messageIndex: number): boolean {
    const chatHistory = this.aiAssistantData.chatHistory();
    const message = chatHistory[messageIndex];
    
    // If message is from history, always show sources immediately
    if (message && message.isFromHistory) {
      return true;
    }
    
    // For new messages, check if it's the most recent and if content is complete
    if (this.isNewMessage(messageIndex)) {
      const displayState = this.contentDisplayState();
      const visibleItems = displayState[messageIndex] || 0;
      const totalItems = message.result ? message.result.length : 0;
      
      // Show sources only when all content items are visible
      return visibleItems > totalItems;
    }
    
    // For older messages, show sources immediately
    return true;
  }

  onContentItemComplete(messageIndex: number, itemIndex: number): void {
    const chatHistory = this.aiAssistantData.chatHistory();
    
    // Find the most recent AI message index
    let mostRecentAiMessageIndex = -1;
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const message = chatHistory[i];
      if (message && !message.isUser && message.result && message.result.length > 0) {
        mostRecentAiMessageIndex = i;
        break;
      }
    }
    
    // Only apply sequential logic to the most recent AI message
    if (messageIndex !== mostRecentAiMessageIndex) {
      return;
    }
    
    const message = chatHistory[messageIndex];
    const totalItems = message.result ? message.result.length : 0;
    
    this.contentDisplayState.update(state => {
      const newState = { ...state };
      const currentVisible = newState[messageIndex] || 0;
      
      // Show the next item after a brief delay for sequential effect
      setTimeout(() => {
        this.contentDisplayState.update(s => {
          const nextVisible = Math.max(currentVisible, itemIndex + 2);
          
          // If this is the last item, add extra count to trigger sources display
          const finalVisible = (itemIndex + 1 >= totalItems) ? totalItems + 1 : nextVisible;
          
          return {
            ...s,
            [messageIndex]: finalVisible
          };
        });
      }, 200); // 200ms delay between items
      
      return newState;
    });
  }

  // Initialize content display for new messages
  private initializeContentDisplay(messageIndex: number, hasContent: boolean): void {
    if (hasContent) {
      const chatHistory = this.aiAssistantData.chatHistory();
      
      // Find the most recent AI message index
      let mostRecentAiMessageIndex = -1;
      for (let i = chatHistory.length - 1; i >= 0; i--) {
        const message = chatHistory[i];
        if (message && !message.isUser && message.result && message.result.length > 0) {
          mostRecentAiMessageIndex = i;
          break;
        }
      }
      
      this.contentDisplayState.update(state => ({
        ...state,
        [messageIndex]: messageIndex === mostRecentAiMessageIndex ? 1 : 999 // Show first item for new, all items for old
      }));
    }
  }

  // Check if a message is the most recent AI message (for typewriter effect)
  isNewMessage(messageIndex: number): boolean {
    const chatHistory = this.aiAssistantData.chatHistory();
    const message = chatHistory[messageIndex];
    
    // If message is from history, never apply typewriter effect
    if (message && message.isFromHistory) {
      return false;
    }
    
    // Find the most recent AI message index
    let mostRecentAiMessageIndex = -1;
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const msg = chatHistory[i];
      if (msg && !msg.isUser && msg.result && msg.result.length > 0) {
        mostRecentAiMessageIndex = i;
        break;
      }
    }
    
    // Only the most recent AI message should have typewriter effect (and not from history)
    return messageIndex === mostRecentAiMessageIndex;
  }

  // Handler for cardClicked event from content-renderer/entity-grid
  onCardClicked(event: { entityType: string, entityId: string, rowData: any }): void {
    if (this.mode === 'overlay') {
      // In overlay mode, navigate to the entity page
      this.navigateToEntity(event.entityType, event.entityId, event.rowData);
    } else {
      // In fullscreen mode, use the entity panel service for modal and emit for right panel
      this.entityPanelService.openPanel(event.entityType, event.entityId, event.rowData);
      this.cardClicked.emit(event);
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
      console.log('🔗 AI Assistant - Navigating to entity:', route);
      this.router.navigate([route]);
    } else {
      console.warn('🔗 AI Assistant - Could not build route for entity:', entityType, entityId);
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
      console.error('Error downloading file:', error);
    }
  }

  decodeBase64Text(data: string): string {
    try {
      return atob(data);
    } catch (error) {
      return 'Unable to decode text content';
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
    if (!mimeType) return 'File';
    
    if (mimeType.includes('word')) return 'Word Document';
    if (mimeType.includes('excel') || mimeType.includes('sheet')) return 'Excel Spreadsheet';
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return 'PowerPoint Presentation';
    if (mimeType.includes('zip')) return 'Archive';
    if (mimeType.includes('json')) return 'JSON File';
    if (mimeType.includes('xml')) return 'XML File';
    
    return mimeType.split('/')[1]?.toUpperCase() || 'File';
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
      console.error('Base64 decode error:', err);
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
    
    console.log('Base64 Analysis:', analysis);
    
    // Try to clean and test the data
    const cleaned = this.cleanBase64Data(data);
    console.log('Cleaned data valid:', this.isValidBase64(cleaned));
    console.log('Original length:', data.length, 'Cleaned length:', cleaned.length);
    
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

  testCleanedImage(inline: any): void {
    const cleaned = this.cleanBase64Data(inline.data);
    const dataUrl = `data:${inline.mimeType};base64,${cleaned}`;
    
    console.log('Testing cleaned image:', {
      originalLength: inline.data.length,
      cleanedLength: cleaned.length,
      validAfterCleaning: this.isValidBase64(cleaned)
    });
    
    // Create a test image to see if it loads
    const img = new Image();
    img.onload = () => {
      console.log('✅ Cleaned image loads successfully!');
      console.log('Image dimensions:', img.width, 'x', img.height);
    };
    img.onerror = () => console.error('❌ Cleaned image still fails to load');
    img.src = dataUrl;
  }

  onImageError(event: any, inline: any): void {
    const dataUrl = `data:${inline.mimeType};base64,${inline.data}`;
    console.error('Image failed to load:', {
      mimeType: inline.mimeType,
      dataLength: inline.data?.length,
      dataPrefix: inline.data?.substring(0, 50),
      constructedUrl: dataUrl.substring(0, 100),
      isValidBase64: this.isValidBase64(inline.data),
      event: event
    });
    
    // Test if data URL is valid
    const testImg = new Image();
    testImg.onload = () => console.log('✅ Data URL is valid, image can load');
    testImg.onerror = () => console.error('❌ Data URL is invalid');
    testImg.src = dataUrl;
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
              this.userName.set('User');
            }
          },
          error: (error) => {
            console.error('Error loading user info:', error);
            this.userName.set('User');
          }
        });
      },
      error: (error) => {
        console.error('Error getting user claims:', error);
        this.userName.set('User');
      }
    });
  }

  /**
   * Regenerate the last AI response
   */
  regenerateMessage(messageIndex: number): void {
    const chatHistory = this.aiAssistantData.chatHistory();
    const message = chatHistory[messageIndex];
    
    if (!message || message.isUser) {
      return;
    }

    // Find the previous user message
    let userMessageIndex = messageIndex - 1;
    while (userMessageIndex >= 0 && !chatHistory[userMessageIndex].isUser) {
      userMessageIndex--;
    }

    if (userMessageIndex >= 0) {
      const userMessage = chatHistory[userMessageIndex];
      
      // Remove all messages after the user message
      const newHistory = chatHistory.slice(0, userMessageIndex + 1);
      this.aiAssistantData.chatHistory.set(newHistory);
      
      // Resend the user message
      this.aiAssistantData.sendMessage(
        userMessage.text || '',
        userMessage.files || [],
        this.buildMessageState()
      ).subscribe({
        next: () => {
          console.log('Message regenerated successfully');
        },
        error: (error) => {
          console.error('Error regenerating message:', error);
        }
      });
    }
  }

  /**
   * Select an example prompt and populate the message input
   */
  selectExamplePrompt(promptText: string): void {
    this.message.set(promptText);
    // Scroll to input area
    setTimeout(() => {
      this.scrollToBottom();
    }, 100);
  }

  /**
   * Build message state object with screen context parameters
   */
  private buildMessageState(): any {
    return {
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
  }

} 