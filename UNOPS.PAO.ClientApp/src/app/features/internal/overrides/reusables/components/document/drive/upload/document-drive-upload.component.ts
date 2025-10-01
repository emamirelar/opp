import { CommonModule, NgFor, NgIf } from '@angular/common';
import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FeedbackDialogService } from '../../../../../../../../common/pages/services/feedback-dialog.service';
import { DrivePickerService } from '../../../../drive-picker.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

declare const google: any;

@Component({
  selector: 'app-document-drive-upload',
  standalone: true,
  templateUrl: './document-drive-upload.component.html',
  styleUrl: './document-drive-upload.component.scss',
  imports: [NgIf, NgFor, FileUploadModule, ButtonModule, DialogModule, CommonModule, TranslateModule],
  providers: [FileUploadModule]
})
export class DriveDocumentUploadComponent  {
  @Input() accept: string = 'image/*,application/pdf';
  @Input() maxFileSize: number = 1000000;
  @Input() multiple: boolean = true;
  @Output() fileUploaded = new EventEmitter<any>();
  @Output() fileSelected = new EventEmitter<any>();
  @Output() fileRemoved = new EventEmitter<any>();
  @Output() filesCleared = new EventEmitter<void>();
  private _driveIntegrationServiceSubscription;
  selectedDriveFiles: any[] = [];

  constructor(private feedbackService: FeedbackDialogService, public driveService: DrivePickerService, private translateService: TranslateService) {
    this._driveIntegrationServiceSubscription = this.driveService.onFilesSelectedEmitter.subscribe({
      next: (event: any) => {
        this.onSelect(event)
      },
    });
  }

  onUpload(filesToUpload: any) {
    this.fileUploaded.emit(filesToUpload);
  }

  onSelect(event: any) {
    this.selectedDriveFiles.push(...event.files);
    this.feedbackService.showInfoToast({detail: this.translateService.instant('messages.filesReadyForUpload', {count: event.files.length})});
    this.fileSelected.emit(event);
  }

  onRemove(fileIndex: number) {
    this.selectedDriveFiles.splice(fileIndex, 1);

    this.feedbackService.showInfoToast({detail: this.translateService.instant('messages.success.fileRemoved')});
    this.fileRemoved.emit(event);
  }

  clearFiles() {
    this.selectedDriveFiles = [];
    this.feedbackService.showInfoToast({detail: this.translateService.instant('messages.success.allFilesCleared')});
    this.filesCleared.emit();
  }
}
