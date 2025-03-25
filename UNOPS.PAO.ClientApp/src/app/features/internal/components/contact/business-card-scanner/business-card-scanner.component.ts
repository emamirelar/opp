import { Component, OnInit, ViewChild, ElementRef, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Dialog } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TranslateModule } from '@ngx-translate/core';
import { FileUploadModule } from 'primeng/fileupload';

import { from, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { GeminiService } from '../../../services/gemini.service';
import {Contact} from '../../../models/contact.model';
import {ProgressSpinner} from 'primeng/progressspinner';

@Component({
  selector: 'app-business-card-scanner',
  templateUrl: './business-card-scanner.component.html',
  standalone: true,
  imports: [CommonModule, Dialog, ButtonModule, MessageModule, TranslateModule, FileUploadModule, ProgressSpinner]
})
export class BusinessCardScannerComponent {
  @ViewChild('video') videoElement!: ElementRef;
  @ViewChild('canvas') canvasElement!: ElementRef;
  @Output() onScannedContact = new EventEmitter<Contact>();

  private geminiService = inject(GeminiService);

  visible = false;
  stream: MediaStream | null = null;
  capturedImage: string | null = null;
  scanning: boolean = false;
  error: string | null = null;

  show() {
    this.visible = true;
    this.startCamera();
  }

  hide() {
    this.visible = false;
    this.stopCamera();
    this.capturedImage = null;
    this.error = null;
  }

  startCamera(): void {
    from(navigator.mediaDevices.getUserMedia({ video: true }))
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
          this.onScannedContact.emit(result);
          this.hide();
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
            this.onScannedContact.emit(result);
            this.hide();
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
