import { NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { FeedbackDialogService } from '../../../services/feedback-dialog.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  templateUrl: './document-upload.component.html',
  styleUrl: './document-upload.component.scss',
  imports: [FileUploadModule, TranslateModule],
  providers: [FileUploadModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class DocumentUploadComponent {
  @Input() accept: string = 'image/*,application/pdf';
  @Input() maxFileSize: number = 1000000;
  @Input() multiple: boolean = true;
  @Output() fileUploaded = new EventEmitter<any>();
  @Output() fileSelected = new EventEmitter<any>();
  @Output() fileRemoved = new EventEmitter<any>();
  @Output() filesCleared = new EventEmitter<void>();

  uploadedFiles: any[] = [];

  constructor(private feedbackService: FeedbackDialogService) { }

  onUpload(event: any) {
    for (let file of event.files) {
      this.uploadedFiles.push(file);
    }

    this.fileUploaded.emit(event);
  }

  onSelect(event: any) {
    this.feedbackService.showInfoToast({ detail: `${event.currentFiles.length} file(s) ready for upload.` });
    this.fileSelected.emit(event);
  }

  onRemove(event: any) {
    const index = this.uploadedFiles.indexOf(event.file);
    if (index >= 0) {
      this.uploadedFiles.splice(index, 1);
    }

    this.feedbackService.showInfoToast({ detail: 'File removed successfully!' });
    this.fileRemoved.emit(event);
  }

  clearFiles() {
    this.uploadedFiles = [];
    this.feedbackService.showInfoToast({ detail: 'All files have been cleared!' });
    this.filesCleared.emit();
  }
}
