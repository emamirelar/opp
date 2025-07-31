import {Component, ViewChild, ElementRef, Input, ViewContainerRef, inject, effect, OnInit, NgZone, ChangeDetectorRef} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { FileUploadModule } from 'primeng/fileupload';
import { TooltipModule } from 'primeng/tooltip';
import { TranslatePipe } from '@ngx-translate/core';
import { AiAssistantData } from './ai-assistant.data';
import { signal } from '@angular/core';
import { LayoutService } from '../../../layouts/services/layout.service';
import { AiAssistantScanComponent } from './scan/ai-assistant-scan.component';
import { SafeUrlPipe } from './safe-url.pipe';
import { FileUploadComponent } from '../../../components/file-upload/file-upload.component';
import { AiAssistantService } from '../../../../features/internal/services/ai-assistant.service';

@Component({
  selector: 'app-ai-assistant',
  templateUrl: './ai-assistant.component.html',
  standalone: true,
  styleUrls: ['./ai-assistant.component.css'],
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    DialogModule,
    TextareaModule,
    ScrollPanelModule,
    FileUploadModule,
    TooltipModule,
    TranslatePipe,
    AiAssistantScanComponent,
    SafeUrlPipe,
    FileUploadComponent
  ]
})
export class AiAssistantComponent implements OnInit {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  @ViewChild('scanComponent') private scanComponent!: AiAssistantScanComponent;
  @ViewChild('fileUploadComponent') private fileUploadComponent!: FileUploadComponent;
  @Input() viewContainerRef!: ViewContainerRef;  // Accept ViewContainerRef

  firstScroll = signal(true);
  message = signal('');
  selectedFiles = signal<{ file: File, name: string, content: string }[]>([]);
  isDragging = signal(false);
  loading = signal(false);
  isFullscreen = signal(false);
  layoutService = inject(LayoutService);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);
  private aiAssistantService = inject(AiAssistantService);
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  isRecording = signal(false);
  audioBlob = signal<Blob | null>(null);
  
  // File validation errors
  fileValidationErrors = signal<string[]>([]);
  
  // Example prompts for welcome message
  examplePrompts = [
    { text: 'aiAssistant.examplePrompt1', icon: 'pi pi-search' },
    { text: 'aiAssistant.examplePrompt2', icon: 'pi pi-file-edit' },
    { text: 'aiAssistant.examplePrompt3', icon: 'pi pi-chart-line' }
  ];

  constructor(
    public aiAssistantData: AiAssistantData
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
  }

  ngOnInit(): void {
    // Initialize with default empty value to avoid undefined
    this.message.set('');
    
    // Load fullscreen state from localStorage instead of cookies
    this.loadFullscreenState();
    
    if (this.viewContainerRef) {
      this.aiAssistantData.setViewContainerRef(this.viewContainerRef);
    }
    
    // Ensure change detection runs
    this.cdr.detectChanges();
  }

  // Fullscreen state management using localStorage
  private loadFullscreenState(): void {
    const fullscreen = localStorage.getItem('aiAssistantFullscreen');
    this.isFullscreen.set(fullscreen === 'true');
  }

  private saveFullscreenState(): void {
    localStorage.setItem('aiAssistantFullscreen', this.isFullscreen().toString());
  }

  // Toggle fullscreen state
  toggleFullscreen(): void {
    this.isFullscreen.set(!this.isFullscreen());
    this.saveFullscreenState();
  }

  // Handle closing the AI Assistant - reset to small screen
  closeAiAssistant(): void {
    // Reset to small screen mode
    this.isFullscreen.set(false);
    this.saveFullscreenState();
    
    // Close the AI Assistant
    this.layoutService.onAIAssistantToggle();
  }

  // Handle example prompt click
  selectExamplePrompt(promptKey: string): void {
    // You can customize this based on the actual prompt text you want to send
    const promptTexts: { [key: string]: string } = {
      'aiAssistant.examplePrompt1': 'Help me analyze this partner data',
      'aiAssistant.examplePrompt2': 'Draft a partnership proposal',
      'aiAssistant.examplePrompt3': 'Generate insights from recent interactions'
    };
    
    this.message.set(promptTexts[promptKey] || '');
  }

  // Safe method to update message that won't trigger ExpressionChangedAfterItHasBeenCheckedError
  updateMessage(value: string): void {
    this.ngZone.run(() => {
      this.message.set(value);
      this.cdr.detectChanges();
    });
  }

  // File upload methods
  onFilesSelected(files: File[]): void {
    console.log('🔍 Files selected:', files.map(f => f.name));
    
    // Filter out duplicate files (by name and size)
    const currentFiles = this.selectedFiles();
    const newFiles = files.filter(newFile => {
      return !currentFiles.some(existing => 
        existing.file.name === newFile.name && 
        existing.file.size === newFile.size
      );
    });

    if (newFiles.length === 0) {
      console.log('⚠️ All files already selected, skipping duplicates');
      return;
    }

    // Convert new files to our format
    const newFileData = newFiles.map(file => ({
      file,
      name: file.name,
      content: ''
    }));

    this.ngZone.run(() => {
      // APPEND to existing files instead of replacing
      const updatedFiles = [...currentFiles, ...newFileData];
      
      this.selectedFiles.set(updatedFiles);
      this.fileValidationErrors.set([]);
      console.log('✅ selectedFiles updated:', this.selectedFiles().map(f => f.name));
      this.cdr.detectChanges();
    });
  }

  onFileValidationErrors(errors: string[]): void {
    console.log('⚠️ File validation errors:', errors);
    this.ngZone.run(() => {
      this.fileValidationErrors.set(errors);
      this.cdr.detectChanges();
    });
  }

  removeFile(index: number): void {
    this.ngZone.run(() => {
    this.selectedFiles.update(files => files.filter((_, i) => i !== index));
      
      // Also update the FileUploadComponent to stay in sync
      if (this.fileUploadComponent) {
        const remainingFiles = this.selectedFiles().map(f => f.file);
        // Update the internal state of the component without triggering events
        this.fileUploadComponent.selectedFiles = remainingFiles.map(file => ({
          file,
          id: this.generateId(),
          name: file.name,
          size: file.size,
          type: file.type,
          status: 'pending' as const
        }));
      }
      
      this.cdr.detectChanges();
    });
  }

  clearAllFiles(): void {
    this.ngZone.run(() => {
      this.selectedFiles.set([]);
      this.fileValidationErrors.set([]);
      if (this.fileUploadComponent) {
        this.fileUploadComponent.clearAllFiles();
      }
      this.cdr.detectChanges();
    });
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  hasValidFiles(): boolean {
    return this.selectedFiles().length > 0 && this.fileValidationErrors().length === 0;
  }

  getFileUploadStatus(): string {
    const files = this.selectedFiles();
    const errors = this.fileValidationErrors();
    
    if (files.length === 0) return '';
    if (errors.length > 0) return `⚠️ ${errors.length} file validation error(s)`;
    return `📎 ${files.length} file(s) ready`;
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
      this.onFilesSelected(Array.from(files));
    }
  }

  // Webcam methods
  startCamera(): void {
    this.scanComponent.show();
  }

  onImageCaptured(file: File): void {
    this.onFilesSelected([file]);
  }

  // Message handling
  sendMessage(): void {
    const currentMessage = this.message();
    const currentFiles = this.selectedFiles();

    if (currentMessage.trim() || currentFiles.length > 0) {
      // Clear message and files before the operation
      this.ngZone.run(() => {
        this.message.set('');
        this.loading.set(true);
        this.cdr.detectChanges();
      });

      // Convert files to ChatFile format
      const chatFiles = currentFiles.map(f => ({
        file: f.file,
        name: f.name,
        content: ''
      }));

      this.aiAssistantData.sendMessage(currentMessage, chatFiles).subscribe({
        next: () => {
          this.ngZone.run(() => {
            this.selectedFiles.set([]);
            this.fileValidationErrors.set([]);
            this.loading.set(false);
            
            // Clear the enhanced file upload component
            if (this.fileUploadComponent) {
              this.fileUploadComponent.clearAllFiles();
            }
            
            this.cdr.detectChanges();
          });
        },
        error: (error) => {
          console.error('Failed to send message:', error);
          this.ngZone.run(() => {
            this.loading.set(false);
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
    return this.loading();
  }

  public async startRecording(): Promise<void> {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream, {
        mimeType: 'audio/webm' // We'll use webm for recording as it's widely supported
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

      // Stop all audio tracks
      this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
  }

  private async processAudioMessage(audioBlob: Blob): Promise<void> {
    try {
      // Convert audio blob to a File and add it to selected files
      const audioFile = new File([audioBlob], 'audio-message.mp3', { type: 'audio/mpeg' });
      this.onFilesSelected([audioFile]);
    } catch (error) {
      console.error('Error processing audio message:', error);
    }
  }
}
