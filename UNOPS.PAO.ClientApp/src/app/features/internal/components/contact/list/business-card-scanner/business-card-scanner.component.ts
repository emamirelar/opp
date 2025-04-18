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

  retake(): void {
    this.capturedImage = null;
    this.startCamera();
  }

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
