import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FileUpload } from '@ai/models/ai-assistant.model';
import { AiAssistantService } from '@ai/services/ai-assistant.service';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Input() maxSizeMB: number = 10;
  @Input() multiple: boolean = true;
  @Input() acceptedTypes: string[] = [];
  @Input() autoUpload: boolean = false;
  
  @Output() filesSelected = new EventEmitter<File[]>();
  @Output() filesChanged = new EventEmitter<File[]>();
  @Output() validationErrors = new EventEmitter<string[]>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  selectedFiles: FileUpload[] = [];
  errors: string[] = [];
  isDragOver = false;
  isUploading = false;
  uploadProgress = 0;

  constructor(
    public aiService: AiAssistantService,
    private translateService: TranslateService
  ) {
    // Use AI service defaults if not provided
    if (this.acceptedTypes.length === 0) {
      this.acceptedTypes = this.aiService.supportedFileTypes;
    }
  }

  onFileSelected(event: any) {
    const files = Array.from(event.target.files as FileList);
    this.processFiles(files);
    // Clear the input so the same file can be selected again
    event.target.value = '';
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
    
    const files = Array.from(event.dataTransfer?.files || []);
    this.processFiles(files);
  }

  private async processFiles(files: File[]) {
    this.errors = [];
    
    // Validate files using the AI service
    const validation = this.aiService.validateFiles(files);
    
    // Add error messages for invalid files
    if (validation.invalid.length > 0) {
      this.errors = validation.invalid.map(item => item.error);
      this.validationErrors.emit(this.errors);
    }
    
    // Process valid files
    for (const file of validation.valid) {
      const fileUpload: FileUpload = {
        file,
        id: this.generateId(),
        name: file.name,
        size: file.size,
        type: file.type,
        status: 'pending'
      };
      
      // Generate preview for images
      if (this.aiService.isImageFile(file)) {
        try {
          const preview = await this.aiService.getFilePreview(file);
          if (preview) {
            fileUpload.preview = preview;
          }
        } catch (error) {
          console.warn('Failed to generate preview for', file.name, error);
        }
      }
      
      // Add to list (replace all if not multiple, otherwise append)
      if (!this.multiple) {
        this.selectedFiles = [fileUpload];
      } else {
        this.selectedFiles.push(fileUpload);
      }
    }
    
    this.emitFiles();
  }

  removeFile(index: number) {
    if (index >= 0 && index < this.selectedFiles.length) {
      this.selectedFiles.splice(index, 1);
      this.emitFiles();
    }
  }

  clearAllFiles() {
    this.selectedFiles = [];
    this.errors = [];
    this.emitFiles();
  }

  private emitFiles() {
    const files = this.selectedFiles.map(fu => fu.file);
    this.filesSelected.emit(files);
    this.filesChanged.emit(files);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  getUploadIcon(): string {
    if (this.selectedFiles.length > 0) {
      return '✅';
    }
    return this.isDragOver ? '📥' : '📎';
  }

  getFileTypeDisplay(mimeType: string): string {
    if (mimeType.startsWith('image/')) return this.translateService.instant('fileUpload.fileTypes.image');
    if (mimeType === 'application/pdf') return this.translateService.instant('fileUpload.fileTypes.pdf');
    if (mimeType.includes('word') || mimeType.includes('document')) return this.translateService.instant('fileUpload.fileTypes.document');
    if (mimeType.includes('excel') || mimeType.includes('spreadsheet')) return this.translateService.instant('fileUpload.fileTypes.spreadsheet');
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return this.translateService.instant('fileUpload.fileTypes.presentation');
    if (mimeType.startsWith('text/')) return this.translateService.instant('fileUpload.fileTypes.text');
    return this.translateService.instant('fileUpload.fileTypes.file');
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'pending': return this.translateService.instant('fileUpload.status.ready');
      case 'uploading': return this.translateService.instant('fileUpload.status.uploading');
      case 'completed': return this.translateService.instant('fileUpload.status.uploaded');
      case 'error': return this.translateService.instant('fileUpload.status.error');
      default: return '';
    }
  }

  // Public methods for external control
  getFiles(): File[] {
    return this.selectedFiles.map(fu => fu.file);
  }

  getFileUploads(): FileUpload[] {
    return [...this.selectedFiles];
  }

  hasFiles(): boolean {
    return this.selectedFiles.length > 0;
  }

  hasErrors(): boolean {
    return this.errors.length > 0;
  }

  // Method to trigger file picker programmatically
  openFilePicker() {
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.click();
    }
  }

  // Method to update file status (useful for upload progress)
  updateFileStatus(fileId: string, status: FileUpload['status'], progress?: number) {
    const fileUpload = this.selectedFiles.find(f => f.id === fileId);
    if (fileUpload) {
      fileUpload.status = status;
      if (progress !== undefined) {
        fileUpload.uploadProgress = progress;
      }
    }
  }

  // Method to set upload progress
  setUploadProgress(progress: number, uploading: boolean = true) {
    this.uploadProgress = Math.max(0, Math.min(100, progress));
    this.isUploading = uploading;
  }
} 
