import {Component, ViewChild, ElementRef, Input, ViewContainerRef, inject, effect, OnInit, OnDestroy, NgZone, ChangeDetectorRef, Output, EventEmitter} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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

@Component({
  selector: 'app-ai-assistant-panel',
  templateUrl: './ai-assistant-panel.component.html',
  standalone: true,
  styleUrls: ['./ai-assistant-panel.component.css'],
  imports: [
    CommonModule,
    FormsModule,
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
  
  // Example prompts for welcome message
  examplePrompts = [
    { text: 'aiAssistant.examplePrompt1', icon: 'pi pi-search' },
    { text: 'aiAssistant.examplePrompt2', icon: 'pi pi-file-edit' },
    { text: 'aiAssistant.examplePrompt3', icon: 'pi pi-chart-line' }
  ];

  constructor(
    public aiAssistantData: AiAssistantData,
    private router: Router,
    private entityPanelService: EntityPanelService
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

    // Build session menu items whenever sessions change
    effect(() => {
      const sessions = this.aiAssistantData.userSessions();
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

  // Handle example prompt click
  selectExamplePrompt(promptKey: string): void {
    const promptTexts: { [key: string]: string } = {
      'aiAssistant.examplePrompt1': 'Help me analyze this partner data',
      'aiAssistant.examplePrompt2': 'Draft a partnership proposal',
      'aiAssistant.examplePrompt3': 'Generate insights from recent interactions'
    };
    
    this.message.set(promptTexts[promptKey] || '');
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

      // Get current URL including domain
      const currentUrl = window.location.href;

      this.aiAssistantData.sendMessage(currentMessage, chatFiles, currentUrl).subscribe({
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
  selectFollowUp(followUpText: string): void {
    this.message.set(followUpText);
    // Focus the textarea for user convenience
    setTimeout(() => {
      const textarea = document.querySelector('textarea[pTextarea]') as HTMLTextAreaElement;
      if (textarea) {
        textarea.focus();
      }
    }, 0);
  }

  // Get follow-ups from the most recent message
  getLatestFollowUps(): string[] {
    const chatHistory = this.aiAssistantData.chatHistory();
    if (chatHistory.length === 0) return [];
    
    // Get the most recent AI message (not user message)
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const message = chatHistory[i];
      if (!message.isUser && message.followUps && message.followUps.length > 0) {
        return message.followUps;
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
        label: 'No old chats', // This will be handled by PrimeNG's translation if configured
        icon: 'pi pi-inbox',
        disabled: true,
        styleClass: 'text-gray-500'
      }];
      this.sessionMenuItems.set(menuItems);
      return;
    }
    
    const menuItems: MenuItem[] = validSessions.map(session => ({
      label: (session as any).title || 'Untitled Chat',
      icon: session.id === currentSessionId ? 'pi pi-check' : 'pi pi-comment',
      command: () => this.switchToSession(session.id!),
      styleClass: session.id === currentSessionId ? 'font-bold' : ''
    }));

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
    const hashUrl = `${baseUrl}#/ai`;
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
    
    this.contentDisplayState.update(state => {
      const newState = { ...state };
      const currentVisible = newState[messageIndex] || 0;
      
      // Show the next item after a brief delay for sequential effect
      setTimeout(() => {
        this.contentDisplayState.update(s => ({
          ...s,
          [messageIndex]: Math.max(currentVisible, itemIndex + 2) // Show current + next item
        }));
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
    
    // Find the most recent AI message index
    let mostRecentAiMessageIndex = -1;
    for (let i = chatHistory.length - 1; i >= 0; i--) {
      const message = chatHistory[i];
      if (message && !message.isUser && message.result && message.result.length > 0) {
        mostRecentAiMessageIndex = i;
        break;
      }
    }
    
    // Only the most recent AI message should have typewriter effect
    return messageIndex === mostRecentAiMessageIndex;
  }

  // Handler for cardClicked event from content-renderer/entity-grid
  onCardClicked(event: { entityType: string, entityId: string, rowData: any }): void {
    this.entityPanelService.openPanel(event.entityType, event.entityId, event.rowData);
    this.cardClicked.emit(event);
  }
} 