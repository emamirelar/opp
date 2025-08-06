import { Component, OnInit, ViewChild, ElementRef, Output, EventEmitter, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TranslateModule } from '@ngx-translate/core';
import { FileUploadModule } from 'primeng/fileupload';
import { DynamicDialogRef } from 'primeng/dynamicdialog';

import { from, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import {ProgressSpinner} from 'primeng/progressspinner';
import { Contact } from '../../../../models/contact.model';
import { GeminiService } from '../../../../services/gemini.service';

/**
 * @uiEntity BusinessCardScanner
 * @route Modal dialog (opened from contact list)
 * @description AI-powered business card scanner that captures images and extracts contact information automatically using Google's Gemini AI
 * @capabilities capture_image, upload_image, scan_text, extract_contact_data, toggle_camera, retake_photo
 * @synonyms card_scanner, contact_scanner, ai_scanner, business_card_reader
 * @mandatoryFields image_capture
 * @help_when_stuck Position the business card within the overlay frame and ensure good lighting. You can either capture a new photo with your camera or upload an existing image. The AI will extract contact details automatically.
 * @common_tasks
 *   - Scanning a business card: Position card in frame and click Capture, then click Scan
 *   - Using existing image: Click Upload Image and select a photo from your device
 *   - Improving quality: Use Retake if the image isn't clear enough
 *   - Switching cameras: Use the camera toggle button on mobile devices
 *   - Extracting data: After capturing/uploading, click Scan to process with AI
 */

@Component({
  selector: 'app-business-card-scanner',
  templateUrl: './business-card-scanner.component.html',
  standalone: true,
  imports: [CommonModule, ButtonModule, MessageModule, TranslateModule, FileUploadModule, ProgressSpinner]
})
export class BusinessCardScannerComponent implements OnInit, OnDestroy {
  @ViewChild('video') videoElement!: ElementRef;
  @ViewChild('canvas') canvasElement!: ElementRef;
  @Output() onScannedContact = new EventEmitter<Contact>();

  private geminiService = inject(GeminiService);
  private dialogRef = inject(DynamicDialogRef);

  stream: MediaStream | null = null;
  capturedImage: string | null = null;
  scanning: boolean = false;
  error: string | null = null;
  isFrontCamera: boolean = false;

  ngOnInit() {
    this.startCamera();
  }

  ngOnDestroy() {
    this.stopCamera();
  }

  hide() {
    this.stopCamera();
    this.capturedImage = null;
    this.error = null;
    this.dialogRef.close();
  }

  startCamera(): void {
    const facingMode = this.isFrontCamera ? 'user' : 'environment';
    
    from(navigator.mediaDevices.getUserMedia({ 
      video: { 
        facingMode: facingMode 
      } 
    }))
      .pipe(
        tap(stream => {
          this.stream = stream;
          this.videoElement.nativeElement.srcObject = stream;
        }),
        catchError(err => {
          this.error = 'Failed to access camera. Please ensure you have granted camera permissions.';
          console.error('Camera error:', err);
          return of(null);
        })
      )
      .subscribe();
  }

  /**
   * @uiButton toggle_camera
   * @description Switches between front and back camera on mobile devices for better card positioning
   * @label Toggle Camera
   * @icon pi pi-sort-alt
   * @when_to_use When the current camera angle isn't optimal for capturing the business card clearly
   * @permissions Camera access required
   */
  toggleCamera(): void {
    this.isFrontCamera = !this.isFrontCamera;
    this.stopCamera();
    this.startCamera();
  }

  stopCamera(): void {
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.stream = null;
    }
  }

  /**
   * @uiButton capture_image
   * @description Captures a photo of the business card using the camera for AI processing
   * @label Capture
   * @icon pi pi-camera
   * @when_to_use When you have positioned the business card within the frame and want to take a photo for scanning
   * @permissions Camera access required
   */
  captureImage(): void {
    const video = this.videoElement.nativeElement;
    const canvas = this.canvasElement.nativeElement;
    const context = canvas.getContext('2d');

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    this.capturedImage = canvas.toDataURL('image/jpeg');
    this.stopCamera();
  }

  /**
   * @uiButton scan_business_card
   * @description Processes the captured business card image using AI to extract contact information
   * @label Scan
   * @icon pi pi-search
   * @when_to_use After capturing or uploading a business card image, to automatically extract contact details
   * @permissions AI service access required
   */
  scanBusinessCard(): void {
    if (!this.capturedImage) return;

    this.scanning = true;
    this.error = null;

    const base64Image = this.capturedImage.split(',')[1];
    const byteCharacters = atob(base64Image);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += 512) {
      const slice = byteCharacters.slice(offset, offset + 512);
      const byteNumbers = new Array(slice.length);
      for (let i = 0; i < slice.length; i++) {
        byteNumbers[i] = slice.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);
      byteArrays.push(byteArray);
    }

    const file = new File(byteArrays, 'scanned-card.jpg', { type: 'image/jpeg' });

    this.geminiService.scanFile(file, 'contact_action')
      .pipe(
        map(result => {
          this.dialogRef.close(result);
          return result;
        }),
        catchError(error => {
          const errorMessage = error instanceof Error ? error.message : 'An unknown error occurred';
          this.error = 'Failed to scan business card. Please try again.';
          console.error('Scanning error:', errorMessage);
          return of(null);
        }),
        tap(() => {
          this.scanning = false;
        })
      )
      .subscribe();
  }

  /**
   * @uiButton retake_photo
   * @description Clears the current captured image and restarts the camera to take a new photo
   * @label Retake
   * @icon pi pi-refresh
   * @when_to_use When the captured image quality is poor or the business card wasn't positioned correctly
   * @permissions Camera access required
   */
  retake(): void {
    this.capturedImage = null;
    this.startCamera();
  }

  /**
   * @uiButton upload_business_card_image
   * @description Uploads an existing business card image from the device for AI processing
   * @label Upload Image
   * @icon pi pi-upload
   * @when_to_use When you have an existing photo of a business card saved on your device instead of taking a new one
   * @permissions File system access required
   */
  handleFileUpload(event: any): void {
    const file = event.files[0];
    if (file) {
      this.scanning = true;
      this.error = null;

      this.geminiService.scanFile(file, 'contact_action')
        .pipe(
          map(result => {
            this.dialogRef.close(result);
            return result;
          }),
          catchError(error => {
            const errorMessage = error instanceof Error ? error.message : 'An unknown error occurred';
            this.error = 'Failed to scan business card. Please try again.';
            console.error('Scanning error:', errorMessage);
            return of(null);
          }),
          tap(() => {
            this.scanning = false;
          })
        )
        .subscribe();
    }
  }
}
