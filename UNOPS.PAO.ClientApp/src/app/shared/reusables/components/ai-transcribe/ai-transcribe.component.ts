import { Component, EventEmitter, Input, Output, ViewChild, ElementRef, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { GeminiService } from '@ai/services/gemini.service';
import { MessageService } from 'primeng/api';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-ai-transcribe',
  standalone: true,
  imports: [
    CommonModule,
    Button,
    MenuModule,
    TranslateModule,
    DialogModule,
    TooltipModule
  ],
  templateUrl: './ai-transcribe.component.html',
  styleUrls: ['./ai-transcribe.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AiTranscribeComponent {
  @Input() transcribeType: string = 'default';
  @Output() transcriptionCompleted = new EventEmitter<any>();

  @ViewChild('menu') private menu: any;
  @ViewChild('fileInput') private fileInput!: ElementRef;
  @ViewChild('audioInput') private audioInput!: ElementRef;
  @ViewChild('canvas') private canvasElement!: ElementRef;
  @ViewChild('video') private videoElement!: ElementRef;

  isUploading = signal(false);
  uploadedFile = signal<{ file: File, preview: SafeUrl | null } | null>(null);
  stream: MediaStream | null = null;
  showCamera = signal(false);

  transcribeMenuItems: MenuItem[] = [];

  constructor(
    private translateService: TranslateService,
    private sanitizer: DomSanitizer,
    private geminiService: GeminiService,
    private messageService: MessageService
  ) {
    this.initTranscribeMenu();
  }

  private initTranscribeMenu(): void {
    this.transcribeMenuItems = [
      {
        label: this.translateService.instant('button.takePhoto'),
        icon: 'pi pi-camera',
        command: () => {
          this.startCamera();
        }
      },
      {
        label: this.translateService.instant('button.uploadImage'),
        icon: 'pi pi-image',
        command: () => {
          this.selectImage();
        }
      },
      {
        label: this.translateService.instant('button.uploadAudio'),
        icon: 'pi pi-volume-up',
        command: () => {
          this.selectAudio();
        }
      }
    ];
  }

  toggleMenu(event: Event): void {
    if (this.menu) {
      this.menu.toggle(event);
    }
  }

  onFileSelect(event: any): void {
    const files = event.files || event.target?.files;
    if (!files?.length) return;

    const file = files[0];

    // Preview for image files
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.uploadedFile.set({
          file: file,
          preview: this.sanitizer.bypassSecurityTrustUrl(e.target.result)
        });
      };
      reader.readAsDataURL(file);
    } else {
      // For audio files
      this.uploadedFile.set({
        file: file,
        preview: null
      });
    }

    // Reset the input
    if (event.target?.value) {
      event.target.value = '';
    }
  }

  getAudioUrl(file: File | undefined | null): SafeUrl | string {
    if (!file) return '';

    const url = URL.createObjectURL(file);
    return this.sanitizer.bypassSecurityTrustUrl(url);
  }

  selectImage(): void {
    this.fileInput.nativeElement.click();
  }

  selectAudio(): void {
    this.audioInput.nativeElement.click();
  }

  startCamera(): void {
    this.showCamera.set(true);
    navigator.mediaDevices.getUserMedia({ video: true })
      .then(stream => {
        this.stream = stream;
        if (this.videoElement) {
          this.videoElement.nativeElement.srcObject = stream;
        }
      })
      .catch(err => {
        console.error('Camera error:', err);
        this.showErrorMessage('message.cameraError');
      });
  }

  stopCamera(): void {
    if (this.stream) {
      this.stream.getTracks().forEach(track => track.stop());
      this.stream = null;
    }
    this.showCamera.set(false);
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
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.uploadedFile.set({
            file: file,
            preview: this.sanitizer.bypassSecurityTrustUrl(e.target.result)
          });
        };
        reader.readAsDataURL(file);
      }
      this.stopCamera();
    }, 'image/jpeg');
  }

  transcribeFile(): void {
    if (!this.uploadedFile()) return;

    this.isUploading.set(true);

    // Use GeminiService to scan the file with the specified type
    this.geminiService.scanFile(this.uploadedFile()!.file, this.transcribeType)
      .subscribe({
        next: (response: any) => {
          // Process the response and emit the data to the parent component
          if (response) {
            this.transcriptionCompleted.emit(response);
            this.showSuccessMessage('message.preFillSuccess');
          } else {
            this.showErrorMessage('message.noDataExtracted');
          }
          this.uploadedFile.set(null);
          this.isUploading.set(false);
        },
        error: (error) => {
          console.error('Error transcribing data:', error);
          this.showErrorMessage('message.errorPreFilling');
          this.uploadedFile.set(null);
          this.isUploading.set(false);
        }
      });
  }

  private showSuccessMessage(messageKey: string): void {
    this.messageService.add({
      severity: 'success',
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant(messageKey)
    });
  }

  private showErrorMessage(messageKey: string, error?: any): void {
    this.messageService.add({
      severity: 'error',
      summary: this.translateService.instant('message.error'),
      detail: this.translateService.instant(messageKey)
    });
    if (error) {
      console.error(error);
    }
  }
}
