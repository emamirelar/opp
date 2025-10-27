import { Component, input, OnDestroy, inject } from '@angular/core';

//Prime NG
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { GDriveAddLinkComponent } from './add-link/document-gdrive-addlink.component';
import { TranslateModule } from '@ngx-translate/core';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-document-gdrive',
  standalone: true,
  imports: [CommonModule, ButtonModule, DialogModule, GDriveAddLinkComponent, TranslateModule],
  templateUrl: './document-gdrive.component.html',
  styleUrls: ['./document-gdrive.component.scss'],
})
export class GDriveDocumentComponent implements OnDestroy {
  entityName = input<string>('');
  entityId = input<string>('');
  appDocumentRef = input<any>(null);
  acceptedMIMETypes = input<string>('');
  disabled = input<boolean>(false);
  showUploadButton = input<boolean>(true);
  showAddLinkDialog: boolean = false;

  private drivePickerService = inject(DrivePickerService);

  ngOnDestroy(): void {
    // Reset MimeTypes when component is destroyed
    this.drivePickerService.setAcceptedMIMETypes('');
  }

  handleOnSelectDriveBtnClick() {
    this.openGoogleDrivePicker();
  }

  /**
   * Opens the Google Drive picker for file selection
   */
  openGoogleDrivePicker() {
    // Set accepted MIME types for the picker
    this.drivePickerService.setAcceptedMIMETypes(this.acceptedMIMETypes());
    
    // Subscribe to file selection events
    const subscription: Subscription = this.drivePickerService.onFilesSelectedEmitter.subscribe({
      next: (event: any) => {
        this.handleSelectedFiles(event);
        subscription.unsubscribe(); // Clean up subscription after handling
      }
    });

    // Directly open the Google Drive picker
    this.drivePickerService.openPicker();
  }

  private handleSelectedFiles(event: any) {
    if (event.files && event.files.length > 0) {
      // Add the selected files directly to the main document component
      // The main document component should handle displaying these with inline editing
      if (this.appDocumentRef()) {
        this.appDocumentRef().addPendingFiles(event.files);
      }
    }
  }

  handleOnAddLinkBtnClick(addLinkComponent: any) {
    addLinkComponent.addLinks();
  }

  handleOnAddLinkDialogClose(addLinkComponent: any) {
    addLinkComponent.clear();
  }

  handleOnAddLinkSuccess() {
    this.showAddLinkDialog = false;
    if (this.appDocumentRef() !== null) {
      this.appDocumentRef().load();
    }
  }
}