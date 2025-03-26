import {Component, ViewChild, ElementRef, Input, ViewContainerRef, inject, effect} from '@angular/core';
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
    SafeUrlPipe
  ]
})
export class AiAssistantComponent {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  @ViewChild('scanComponent') private scanComponent!: AiAssistantScanComponent;

  firstScroll = signal(true);
  message = signal('');
  selectedFiles = signal<{ file: File, name: string, content: string }[]>([]);
  isProcessingFile = signal(false);
  isDragging = signal(false);
  loading = signal(false);
  layoutService = inject(LayoutService);
  private mediaRecorder: MediaRecorder | null = null;
  private audioChunks: Blob[] = [];
  isRecording = signal(false);
  audioBlob = signal<Blob | null>(null);

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

      // Replace any existing files with the new one
      this.selectedFiles.set([{
        file: files[0],
        name: files[0].name,
        content: contents[0]
      }]);
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
      this.message.set('');
      this.loading.set(true);

      // Convert files to ChatFile format
      const chatFiles = currentFiles.map(f => ({
        file: f.file,
        name: f.name,
        content: f.content
      }));

      this.aiAssistantData.sendMessage(currentMessage, chatFiles).subscribe({
        next: () => {
          this.message.set('');
          this.selectedFiles.set([]);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Failed to send message:', error);
          this.loading.set(false);
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
      // Convert audio blob to base64
      const base64Audio = await this.blobToBase64(audioBlob);

      // Replace any existing files with the audio file
      this.selectedFiles.set([{
        file: new File([audioBlob], 'audio-message.mp3', { type: 'audio/mpeg' }),
        name: 'audio-message.mp3',
        content: base64Audio
      }]);
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
}
