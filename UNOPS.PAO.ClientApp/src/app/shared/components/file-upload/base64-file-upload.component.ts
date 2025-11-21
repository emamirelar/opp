/**
 * @fileoverview Simple file upload component that converts files to base64
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG imports
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

/**
 * @interface Base64FileData
 * @description Data structure for uploaded file with base64 content
 */
export interface Base64FileData {
  fileName: string;
  fileType: string;
  fileSize: number;
  base64Content: string;
}

/**
 * @class Base64FileUploadComponent
 * @description Component for uploading files and converting them to base64
 * Provides a simple file upload interface that converts selected files to base64 format
 * for storage without requiring external file storage systems.
 * 
 * @example
 * ```html
 * <app-base64-file-upload
 *   [acceptedFileTypes]="'.pdf,.doc,.docx'"
 *   [maxFileSize]="5242880"
 *   [disabled]="false"
 *   (fileSelected)="handleFileSelected($event)"
 *   (fileCleared)="handleFileCleared()">
 * </app-base64-file-upload>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-base64-file-upload',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    FileUploadModule,
    ButtonModule,
    MessageModule
  ],
  templateUrl: './base64-file-upload.component.html',
  styleUrls: ['./base64-file-upload.component.scss']
})
export class Base64FileUploadComponent {
  /**
   * @description Accepted file types (e.g., '.pdf,.doc,.docx')
   * @type {Signal<string>}
   * @default '.pdf,.doc,.docx,.xlsx,.png,.jpg,.jpeg'
   * @since 1.0.0
   */
  readonly acceptedFileTypes = input<string>('.pdf,.doc,.docx,.xlsx,.png,.jpg,.jpeg');

  /**
   * @description Maximum file size in bytes
   * @type {Signal<number>}
   * @default 5242880 (5MB)
   * @since 1.0.0
   */
  readonly maxFileSize = input<number>(5242880); // 5MB default

  /**
   * @description Whether the upload is disabled
   * @type {Signal<boolean>}
   * @default false
   * @since 1.0.0
   */
  readonly disabled = input<boolean>(false);

  /**
   * @description Event emitted when a file is selected and converted to base64
   * @type {OutputEmitterRef<Base64FileData>}
   * @param {Base64FileData} fileData - The uploaded file data with base64 content
   * @since 1.0.0
   */
  readonly fileSelected = output<Base64FileData>();

  /**
   * @description Event emitted when the file is cleared
   * @type {OutputEmitterRef<void>}
   * @since 1.0.0
   */
  readonly fileCleared = output<void>();

  // Component state
  readonly selectedFile = signal<Base64FileData | null>(null);
  readonly uploading = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  /**
   * @description Handle file selection from the file input
   * @param {Event} event - The file input change event
   * @returns {void}
   * @since 1.0.0
   */
  onFileSelect(event: any): void {
    const file: File = event.files[0];
    
    if (!file) {
      return;
    }

    // Validate file size
    if (file.size > this.maxFileSize()) {
      const maxSizeMB = this.maxFileSize() / 1048576;
      this.error.set(`File size exceeds maximum allowed size of ${maxSizeMB}MB`);
      return;
    }

    this.error.set(null);
    this.uploading.set(true);

    // Convert file to base64
    const reader = new FileReader();
    
    reader.onload = () => {
      const base64String = reader.result as string;
      // Remove data URL prefix (e.g., "data:application/pdf;base64,")
      const base64Content = base64String.split(',')[1];

      const fileData: Base64FileData = {
        fileName: file.name,
        fileType: file.type,
        fileSize: file.size,
        base64Content: base64Content
      };

      this.selectedFile.set(fileData);
      this.uploading.set(false);
      this.fileSelected.emit(fileData);
    };

    reader.onerror = () => {
      this.error.set('Error reading file. Please try again.');
      this.uploading.set(false);
    };

    reader.readAsDataURL(file);
  }

  /**
   * @description Clear the selected file
   * @returns {void}
   * @since 1.0.0
   */
  onClear(): void {
    this.selectedFile.set(null);
    this.error.set(null);
    this.fileCleared.emit();
  }

  /**
   * @description Format file size for display
   * @param {number} bytes - File size in bytes
   * @returns {string} Formatted file size string
   * @since 1.0.0
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}

