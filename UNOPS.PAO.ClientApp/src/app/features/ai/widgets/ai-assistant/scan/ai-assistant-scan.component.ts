import { Component, ViewChild, ElementRef, Output, EventEmitter } from '@angular/core';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import {TranslateModule} from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { from, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { signal } from '@angular/core';

@Component({
  selector: 'app-ai-assistant-scan',
  standalone: true,
  imports: [
    CommonModule,
    Button,
    Dialog,
    TranslateModule
  ],
  templateUrl: './ai-assistant-scan.component.html'
})
export class AiAssistantScanComponent {
  @ViewChild('video') private videoElement!: ElementRef;
  @ViewChild('canvas') private canvasElement!: ElementRef;
  @Output() onImageCaptured = new EventEmitter<File>();

  showWebcam = signal(false);
  stream: MediaStream | null = null;

  show(): void {
    this.showWebcam.set(true);
    this.startCamera();
  }

  private startCamera(): void {
    from(navigator.mediaDevices.getUserMedia({ video: true }))
      .pipe(
        tap(stream => {
          this.stream = stream;
          if (this.videoElement) {
            this.videoElement.nativeElement.srcObject = stream;
          }
        }),
        catchError(err => {
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
    this.showWebcam.set(false);
  }

  captureImage(): void {
    if (!this.videoElement || !this.canvasElement) return;

    const video = this.videoElement.nativeElement;
    const canvas = this.canvasElement.nativeElement;
    const context = canvas.getContext('2d');

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    canvas.toBlob((blob: Blob | null) => {
      if (blob) {
        const file = new File([blob], 'webcam-capture.jpg', { type: 'image/jpeg' });
        this.onImageCaptured.emit(file);
      }
      this.stopCamera();
    }, 'image/jpeg');
  }
}
