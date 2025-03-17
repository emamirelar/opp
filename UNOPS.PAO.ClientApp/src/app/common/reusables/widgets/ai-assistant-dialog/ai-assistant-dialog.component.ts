import { Component, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { FileUploadModule } from 'primeng/fileupload';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-ai-assistant-dialog',
  templateUrl: './ai-assistant-dialog.component.html',
  standalone: true,
  styles: [`
    :host ::ng-deep .p-button-label {
      display: none !important;
    }
  `],
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    DialogModule,
    TextareaModule,
    ScrollPanelModule,
    FileUploadModule,
    TooltipModule
  ]
})
export class AiAssistantDialogComponent {
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  visible = false;
  message = '';
  chatHistory: { text: string; isUser: boolean; timestamp: Date; files?: { name: string, content: string }[] }[] = [];
  selectedFiles: { name: string, content: string }[] = [];
  isProcessingFile = false;
  isDragging = false;
  private readonly STORAGE_KEY = 'ai-assistant-chat-history';

  constructor() {
    this.loadChatHistory();
  }

  private loadChatHistory(): void {
    const savedHistory = localStorage.getItem(this.STORAGE_KEY);
    if (savedHistory) {
      try {
        const parsed = JSON.parse(savedHistory);
        // Convert string dates back to Date objects
        this.chatHistory = parsed.map((msg: any) => ({
          ...msg,
          timestamp: new Date(msg.timestamp)
        }));
      } catch (error) {
        console.error('Error loading chat history:', error);
        this.chatHistory = [];
      }
    }
  }

  private saveChatHistory(): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.chatHistory));
    } catch (error) {
      console.error('Error saving chat history:', error);
    }
  }

  toggleChat() {
    this.visible = !this.visible;
    if (this.visible) {
      this.scrollToBottom(false);
    }
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
    if (this.message.trim() || this.selectedFiles.length > 0) {
      // Add user message to chat history
      this.chatHistory.push({
        text: this.message,
        isUser: true,
        timestamp: new Date(),
        files: [...this.selectedFiles]
      });
      this.saveChatHistory();
      this.scrollToBottom();

      // Simulate response (in a real app, you would call a service here)
      setTimeout(() => {
        this.chatHistory.push({
          text: 'This is a simulated response. In a real application, this would come from a backend service.',
          isUser: false,
          timestamp: new Date()
        });
        this.saveChatHistory();
        this.scrollToBottom();
      }, 1000);

      // Clear the message input and selected files
      this.message = '';
      this.selectedFiles = [];
    }
  }

  onFileSelect(event: any) {
    this.isProcessingFile = true;
    const files = event.files;

    for (const file of files) {
      this.readFileAsBase64(file).then(content => {
        this.selectedFiles.push({
          name: file.name,
          content: content
        });
      }).catch(error => {
        console.error('Error reading file:', error);
      }).finally(() => {
        this.isProcessingFile = false;
      });
    }
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
    // Insert file reference at cursor position or at the end of message
    const fileRef = `[FILE:${file.name}]`;
    this.message += this.message ? ` ${fileRef}` : fileRef;
  }

  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  clearHistory() {
    this.chatHistory = [];
    this.saveChatHistory();
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    
    if (event.dataTransfer && event.dataTransfer.files.length > 0) {
      const files = event.dataTransfer.files;
      this.isProcessingFile = true;
      
      for (let i = 0; i < files.length; i++) {
        const file = files[i];
        this.readFileAsBase64(file).then(content => {
          this.selectedFiles.push({
            name: file.name,
            content: content
          });
        }).catch(error => {
          console.error('Error reading dropped file:', error);
        }).finally(() => {
          if (i === files.length - 1) {
            this.isProcessingFile = false;
          }
        });
      }
    }
  }
}
