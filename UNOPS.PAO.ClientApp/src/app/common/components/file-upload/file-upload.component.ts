import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileUpload } from '../../../features/internal/models/ai-assistant.model';
import { AiAssistantService } from '../../../features/internal/services/ai-assistant.service';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="file-upload-container">
      <!-- Upload Area -->
      <div class="file-upload-area" 
           [class.drag-over]="isDragOver"
           [class.has-files]="selectedFiles.length > 0"
           (dragover)="onDragOver($event)"
           (dragleave)="onDragLeave($event)" 
           (drop)="onDrop($event)"
           (click)="fileInput.click()">
        
        <input #fileInput 
               type="file" 
               [multiple]="multiple"
               [accept]="acceptedTypes.join(',')"
               (change)="onFileSelected($event)"
               style="display: none;">
        
        <div class="upload-content">
          <div class="upload-icon" [innerHTML]="getUploadIcon()"></div>
          <div class="upload-text">
            <div class="primary-text" *ngIf="!selectedFiles.length">
              Drop files here or click to browse
            </div>
            <div class="primary-text" *ngIf="selectedFiles.length">
              {{selectedFiles.length}} file(s) selected
            </div>
            <div class="secondary-text">
              Max {{maxSizeMB}}MB per file • Supports images, documents, PDFs
            </div>
          </div>
        </div>
      </div>

      <!-- File List -->
      <div class="file-list" *ngIf="selectedFiles.length">
        <div class="file-list-header">
          <span>Attached Files ({{selectedFiles.length}})</span>
          <button class="clear-all-btn" (click)="clearAllFiles()" type="button">
            Clear All
          </button>
        </div>
        
        <div class="file-items">
          <div class="file-item" *ngFor="let fileUpload of selectedFiles; let i = index" 
               [class.file-error]="fileUpload.status === 'error'">
            
            <!-- File Preview -->
            <div class="file-preview">
              <img *ngIf="fileUpload.preview" 
                   [src]="fileUpload.preview" 
                   [alt]="fileUpload.name"
                   class="image-preview">
              <div *ngIf="!fileUpload.preview" 
                   class="file-icon"
                   [innerHTML]="aiService.getFileIcon(fileUpload.type)">
              </div>
            </div>
            
            <!-- File Info -->
            <div class="file-info">
              <div class="file-name" [title]="fileUpload.name">
                {{fileUpload.name}}
              </div>
              <div class="file-details">
                <span class="file-size">{{aiService.formatFileSize(fileUpload.size)}}</span>
                <span class="file-type">{{getFileTypeDisplay(fileUpload.type)}}</span>
              </div>
              <div class="file-status" *ngIf="fileUpload.status">
                <span [class]="'status-' + fileUpload.status">
                  {{getStatusText(fileUpload.status)}}
                </span>
              </div>
            </div>
            
            <!-- Actions -->
            <div class="file-actions">
              <button class="remove-btn" 
                      (click)="removeFile(i)" 
                      type="button"
                      [title]="'Remove ' + fileUpload.name">
                <span class="remove-icon">×</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Error Messages -->
      <div class="error-messages" *ngIf="errors.length">
        <div class="error-header">
          <span class="error-icon">⚠️</span>
          <span>File Upload Issues</span>
        </div>
        <div class="error-list">
          <div class="error-item" *ngFor="let error of errors">
            {{error}}
          </div>
        </div>
      </div>

      <!-- Upload Progress (for future use) -->
      <div class="upload-progress" *ngIf="isUploading">
        <div class="progress-bar">
          <div class="progress-fill" [style.width.%]="uploadProgress"></div>
        </div>
        <div class="progress-text">
          Uploading files... {{uploadProgress}}%
        </div>
      </div>
    </div>
  `,
  styles: [`
    .file-upload-container {
      width: 100%;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }

    .file-upload-area {
      border: 2px dashed #d1d5db;
      border-radius: 12px;
      padding: 24px;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s ease;
      background: #f9fafb;
      position: relative;
      min-height: 120px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .file-upload-area:hover {
      border-color: #3b82f6;
      background: #eff6ff;
    }

    .file-upload-area.drag-over {
      border-color: #1d4ed8;
      background: #dbeafe;
      transform: scale(1.02);
    }

    .file-upload-area.has-files {
      border-color: #10b981;
      background: #ecfdf5;
    }

    .upload-content {
      pointer-events: none;
    }

    .upload-icon {
      font-size: 2.5rem;
      margin-bottom: 12px;
      opacity: 0.7;
    }

    .primary-text {
      font-size: 1.1rem;
      font-weight: 500;
      color: #374151;
      margin-bottom: 6px;
    }

    .secondary-text {
      font-size: 0.875rem;
      color: #6b7280;
    }

    .file-list {
      margin-top: 20px;
    }

    .file-list-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
      font-weight: 500;
      color: #374151;
    }

    .clear-all-btn {
      background: #ef4444;
      color: white;
      border: none;
      border-radius: 6px;
      padding: 4px 12px;
      font-size: 0.8rem;
      cursor: pointer;
      transition: background-color 0.2s;
    }

    .clear-all-btn:hover {
      background: #dc2626;
    }

    .file-items {
      space-y: 8px;
    }

    .file-item {
      display: flex;
      align-items: center;
      padding: 12px;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      background: #ffffff;
      transition: all 0.2s ease;
      margin-bottom: 8px;
    }

    .file-item:hover {
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .file-item.file-error {
      border-color: #ef4444;
      background: #fef2f2;
    }

    .file-preview {
      width: 48px;
      height: 48px;
      border-radius: 6px;
      overflow: hidden;
      margin-right: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f3f4f6;
      flex-shrink: 0;
    }

    .image-preview {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .file-icon {
      font-size: 1.5rem;
    }

    .file-info {
      flex: 1;
      min-width: 0;
    }

    .file-name {
      font-weight: 500;
      color: #111827;
      font-size: 0.9rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      margin-bottom: 4px;
    }

    .file-details {
      display: flex;
      gap: 12px;
      font-size: 0.8rem;
      color: #6b7280;
    }

    .file-status {
      margin-top: 4px;
      font-size: 0.75rem;
    }

    .status-pending { color: #6b7280; }
    .status-uploading { color: #3b82f6; }
    .status-completed { color: #10b981; }
    .status-error { color: #ef4444; }

    .file-actions {
      margin-left: 12px;
    }

    .remove-btn {
      background: #ef4444;
      color: white;
      border: none;
      border-radius: 50%;
      width: 28px;
      height: 28px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s ease;
      font-size: 1.2rem;
      line-height: 1;
    }

    .remove-btn:hover {
      background: #dc2626;
      transform: scale(1.1);
    }

    .remove-icon {
      font-weight: bold;
    }

    .error-messages {
      margin-top: 16px;
      border: 1px solid #fecaca;
      border-radius: 8px;
      background: #fef2f2;
      overflow: hidden;
    }

    .error-header {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      background: #fee2e2;
      font-weight: 500;
      color: #991b1b;
      border-bottom: 1px solid #fecaca;
    }

    .error-list {
      padding: 0;
    }

    .error-item {
      padding: 8px 16px;
      color: #dc2626;
      font-size: 0.875rem;
      border-bottom: 1px solid #fecaca;
    }

    .error-item:last-child {
      border-bottom: none;
    }

    .upload-progress {
      margin-top: 16px;
      padding: 16px;
      background: #f0f9ff;
      border-radius: 8px;
      border: 1px solid #bae6fd;
    }

    .progress-bar {
      width: 100%;
      height: 8px;
      background: #e0f2fe;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 8px;
    }

    .progress-fill {
      height: 100%;
      background: #0ea5e9;
      transition: width 0.3s ease;
    }

    .progress-text {
      text-align: center;
      font-size: 0.875rem;
      color: #0369a1;
      font-weight: 500;
    }

    /* Responsive design */
    @media (max-width: 640px) {
      .file-upload-area {
        padding: 16px;
        min-height: 100px;
      }
      
      .upload-icon {
        font-size: 2rem;
      }
      
      .primary-text {
        font-size: 1rem;
      }
      
      .file-item {
        padding: 8px;
      }
      
      .file-preview {
        width: 40px;
        height: 40px;
        margin-right: 8px;
      }
    }
  `]
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

  constructor(public aiService: AiAssistantService) {
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
          fileUpload.preview = await this.aiService.getFilePreview(file);
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
    if (mimeType.startsWith('image/')) return 'Image';
    if (mimeType === 'application/pdf') return 'PDF';
    if (mimeType.includes('word') || mimeType.includes('document')) return 'Document';
    if (mimeType.includes('excel') || mimeType.includes('spreadsheet')) return 'Spreadsheet';
    if (mimeType.includes('powerpoint') || mimeType.includes('presentation')) return 'Presentation';
    if (mimeType.startsWith('text/')) return 'Text';
    return 'File';
  }

  getStatusText(status: string): string {
    switch (status) {
      case 'pending': return 'Ready';
      case 'uploading': return 'Uploading...';
      case 'completed': return 'Uploaded';
      case 'error': return 'Error';
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