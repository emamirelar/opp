import { Component, ViewChild, ElementRef, Input, ViewContainerRef } from '@angular/core';
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
    TranslatePipe
  ]
})
export class AiAssistantComponent {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  @Input() viewContainerRef!: ViewContainerRef;  // Accept ViewContainerRef

  // Local UI state signals
  visible = false;
  message = signal('');
  selectedFiles = signal<{ name: string, content: string }[]>([]);
  isProcessingFile = signal(false);
  isDragging = signal(false);
  fileUploadEnabled = signal(false);
  isWaitingResponse = signal(false);

  constructor(
    public aiAssistantData: AiAssistantData  // Make it public to access in template
  ) {}

  ngOnInit() {
    this.aiAssistantData.initializeSession();
  }

  toggleChat() {
    this.visible = !this.visible;
    this.scrollToBottom(false);
  }

  private scrollToBottom(smooth = true): void {
    try {
      setTimeout(() => {
        const dialogContent = this.chatContainer.nativeElement.closest('.p-dialog-content');
        if (dialogContent) {
          dialogContent.scrollTo({
            top: dialogContent.scrollHeight,
            behavior: smooth ? 'smooth' : 'instant'
          });
        }
      }, 100);
    } catch (err) { }
  }

  sendMessage() {
    const currentMessage = this.message();
    const currentFiles = this.selectedFiles();
    this.scrollToBottom();

    if ((currentMessage.trim() || (this.fileUploadEnabled() && currentFiles.length > 0))) {
      this.message.set('');
      this.isWaitingResponse.set(true);
      this.aiAssistantData.sendMessage(currentMessage, currentFiles).subscribe({
        next: () => {
          this.scrollToBottom();
          // Clear the message input and selected files
          this.message.set('');
          this.selectedFiles.set([]);
          this.isWaitingResponse.set(false);
        },
        error: (error) => {
          console.error('Failed to send message:', error);
          this.isWaitingResponse.set(false);
        }
      });
    }
  }

  onFileSelect(event: any) {
    if (!this.fileUploadEnabled()) return;
    this.isProcessingFile.set(true);
    const files = event.files;

    Promise.all(
      Array.from(files).map(file => this.readFileAsBase64(file as File))
    ).then(contents => {
      this.selectedFiles.update(current => [
        ...current,
        ...contents.map((content, index) => ({
          name: files[index].name,
          content
        }))
      ]);
    }).catch(error => {
      console.error('Error reading files:', error);
    }).finally(() => {
      this.isProcessingFile.set(false);
    });
  }

  readFileAsBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        resolve(result);
      };
      reader.onerror = (error) => {
        reject(error);
      };
      reader.readAsDataURL(file);
    });
  }

  insertFileReference(file: { name: string, content: string }) {
    const fileRef = `[FILE:${file.name}]`;
    this.message.update(current => current ? `${current} ${fileRef}` : fileRef);
  }

  removeFile(index: number) {
    this.selectedFiles.update(files => files.filter((_, i) => i !== index));
  }

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
    if (!this.fileUploadEnabled()) return;
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    if (event.dataTransfer?.files.length) {
      this.isProcessingFile.set(true);
      const files = event.dataTransfer.files;

      Promise.all(
        Array.from(files).map(file => this.readFileAsBase64(file))
      ).then(contents => {
        this.selectedFiles.update(current => [
          ...current,
          ...contents.map((content, index) => ({
            name: files[index].name,
            content
          }))
        ]);
      }).catch(error => {
        console.error('Error reading dropped files:', error);
      }).finally(() => {
        this.isProcessingFile.set(false);
      });
    }
  }
}
