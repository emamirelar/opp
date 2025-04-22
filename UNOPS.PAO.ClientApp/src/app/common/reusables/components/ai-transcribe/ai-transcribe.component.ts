import { Component, EventEmitter, Input, Output, ViewChild, ElementRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { GeminiService } from '../../../../features/internal/services/gemini.service';
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
  template: `
    <div class="flex flex-col p-3 border border-gray-200 rounded-lg bg-gray-50">
      <div *ngIf="!uploadedFile()" class="flex">
        <!-- Hidden file inputs -->
        <input #fileInput type="file" class="hidden" accept="image/*" (change)="onFileSelect($event)">
        <input #audioInput type="file" class="hidden" accept="audio/*" (change)="onFileSelect($event)">
        
        <!-- Dropdown menu button -->
        <div class="inline-flex">
          <p-button 
            icon="pi pi-bolt" 
            [label]="'button.preFillOptions' | translate"
            (onClick)="toggleMenu($event)"
            [outlined]="true"
            class="w-full">
          </p-button>
          <p-menu #menu [model]="transcribeMenuItems" [popup]="true" [appendTo]="'body'"></p-menu>
        </div>
      </div>
      
      <!-- Preview area if file selected -->
      <div *ngIf="uploadedFile()" class="mt-3 flex flex-col items-center">
        <!-- Image preview -->
        <div *ngIf="uploadedFile()?.preview" class="mb-2 max-w-full max-h-32 overflow-hidden">
          <img [src]="uploadedFile()?.preview" alt="Preview" class="max-w-full max-h-32 object-contain">
        </div>
        
        <!-- Audio preview -->
        <div *ngIf="!uploadedFile()?.preview && uploadedFile()?.file && uploadedFile()?.file?.type?.startsWith('audio/')" class="w-full mb-2">
          <audio controls class="w-full h-8">
            <source [src]="getAudioUrl(uploadedFile()?.file)" type="audio/mpeg">
          </audio>
        </div>
        
        <div class="flex gap-2 mt-2">
          <p-button 
            icon="pi pi-times" 
            [label]="'button.remove' | translate"
            (onClick)="uploadedFile.set(null)"
            [outlined]="true"
            severity="danger"
            size="small">
          </p-button>
          
          <p-button 
            icon="pi pi-check" 
            [label]="'button.preFillFrom' | translate"
            (onClick)="transcribeFile()"
            [loading]="isUploading()"
            size="small">
          </p-button>
        </div>
      </div>
    </div>

    <!-- Camera capture dialog -->
    <p-dialog
      [visible]="showCamera()"
      (visibleChange)="showCamera.set($event)"
      [modal]="true"
      [draggable]="false"
      [resizable]="false"
      [style]="{width: '90vw', maxWidth: '640px'}"
      (onHide)="stopCamera()"
      [header]="'title.takePhoto' | translate">

      <div class="flex flex-col gap-4">
        <div class="relative w-full aspect-video bg-black rounded-lg overflow-hidden">
          <video #video class="w-full h-full object-cover" autoplay playsinline></video>
          <div class="absolute inset-0 flex items-center justify-center">
            <div class="w-4/5 h-3/5 border-2 border-white/50 rounded-lg shadow-[0_0_0_9999px_rgba(0,0,0,0.5)]"></div>
          </div>
        </div>

        <div class="flex justify-end gap-2">
          <p-button
            icon="pi pi-times"
            (onClick)="stopCamera()"
            [label]="'button.cancel' | translate"
            class="p-button-secondary">
          </p-button>
          <p-button
            icon="pi pi-camera"
            (onClick)="captureImage()"
            [label]="'button.capture' | translate">
          </p-button>
        </div>
      </div>
    </p-dialog>

    <!-- Hidden canvas for image processing -->
    <canvas #canvas class="hidden"></canvas>
  `,
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