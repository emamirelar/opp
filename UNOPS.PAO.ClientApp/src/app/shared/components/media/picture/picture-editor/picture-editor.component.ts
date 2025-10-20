import { Component, OnInit, Output, EventEmitter, inject, Input, computed, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { FormsModule } from '@angular/forms';
import { ImageCropperComponent, ImageCroppedEvent, LoadedImage } from 'ngx-image-cropper';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { PictureEditorDataLoaderService } from './picture-editor-data-loader.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';

@Component({
  selector: 'app-picture-editor',
  templateUrl: './picture-editor.component.html',
  styleUrls: ['./picture-editor.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    FileUploadModule,
    ProgressSpinnerModule,
    TranslateModule,
    FormsModule,
    ImageCropperComponent
  ],
  providers: [PictureEditorDataLoaderService],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PictureEditorComponent implements OnInit {

  @Input() set uploadUrl(value: string) {
    if (value) {
      this.dataLoader.setUploadUrl(value);
    }
  }

  @Output() onSaveImage = new EventEmitter<string>();

  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig, { optional: true });
  private readonly sanitizer = inject(DomSanitizer);
  private readonly dataLoader = inject(PictureEditorDataLoaderService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly cdr = inject(ChangeDetectorRef);

  imageChangedEvent: any = '';
  imageBase64: string | null = null;
  croppedImage: SafeUrl | undefined;
  croppedBlob: Blob | null = null;
  isProcessing: boolean = false;
  isCropperReady: boolean = false;
  isDraggingOver: boolean = false;

  readonly isUploading = computed(() => this.dataLoader.isLoading());
  readonly uploadProgress = computed(() => this.dataLoader.uploadProgress());

  ngOnInit(): void {
    if (this.dialogConfig?.data?.uploadUrl) {
      this.dataLoader.setUploadUrl(this.dialogConfig.data.uploadUrl);
    }
  }

  hide(): void {
    this.dialogRef.close();
  }

  handleFileInput(file: File): void {
    if (!file) return;

    this.resetState();
    this.isProcessing = true;
    this.cdr.markForCheck();

    this.readFileAsBase64(file).then((base64) => {
      this.imageBase64 = base64;
      this.imageChangedEvent = { target: { files: [file] } };
      this.cdr.markForCheck();
    }).catch(() => {
      this.showError('message.failedToLoadImage');
      this.isProcessing = false;
      this.cdr.markForCheck();
    });
  }

  handleFileUpload(event: any): void {
    const file = event.files?.[0];
    if (file) {
      this.handleFileInput(file);
    }
  }

  handleDragEnter(event: DragEvent): void {
    event.preventDefault();
    this.isDraggingOver = true;
    this.cdr.markForCheck();
  }

  handleDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDraggingOver = false;
    this.cdr.markForCheck();
  }

  handleDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDraggingOver = false;
    this.cdr.markForCheck();
    if (event.dataTransfer?.files?.length) {
      this.handleFileInput(event.dataTransfer.files[0]);
    }
  }

  imageCropped(event: ImageCroppedEvent): void {
    if (!event.blob || !event.objectUrl) return;

    this.croppedBlob = event.blob;
    this.croppedImage = this.sanitizer.bypassSecurityTrustUrl(event.objectUrl);
  }

  imageLoaded(): void {
    this.isProcessing = false;
    this.cdr.markForCheck();
  }

  cropperReady(): void {
    this.isProcessing = false;
    this.isCropperReady = true;
    this.cdr.markForCheck();
  }

  loadImageFailed(): void {
    this.showError('message.failedToLoadImage');
    this.isProcessing = false;
    this.isCropperReady = false;
    this.cdr.markForCheck();
  }

  applyChanges(): void {
    if (!this.croppedBlob || !this.isCropperReady) {
      this.showError('message.failedToLoadImage');
      return;
    }

    const file = new File([this.croppedBlob], 'profile-picture.jpg', { type: 'image/jpeg' });

    this.dataLoader.uploadImage(file).subscribe({
      next: (imageUrl: string) => {
        this.onSaveImage.emit(imageUrl);
        this.dialogRef.close(imageUrl);
      },
      error: () => {
        this.showError('message.failedToUploadImage');
      }
    });
  }

  private showError(messageKey: string): void {
    this.feedbackService.showErrorToast({
      detail: this.translateService.instant(messageKey)
    });
  }

  private resetState(): void {
    this.isCropperReady = false;
    this.croppedBlob = null;
    this.croppedImage = undefined;
  }

  private readFileAsBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e: any) => resolve(e.target.result);
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }
}

