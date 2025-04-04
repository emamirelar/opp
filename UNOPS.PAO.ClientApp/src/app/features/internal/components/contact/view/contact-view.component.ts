import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { PanelModule } from 'primeng/panel';
import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { DocumentService } from '../../../services/document.service';
import { DriveDocumentUploadComponent } from '../../../overrides/reusables/components/document/drive/upload/document-drive-upload.component';
import { ParentEntityType } from '../../../overrides/interfaces/types';
import { DocumentLinkModel } from '../../../overrides/interfaces/types';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { ContactService } from '../../../services/contact.service';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import { EntityType } from '../../../../../common/models/link.model';
import { LinkListComponent } from '../../../../../common/reusables/components/link/list/link-list.component';
import {DatePipe, JsonPipe} from '@angular/common';
import { DialogService } from 'primeng/dynamicdialog';
import {Avatar} from 'primeng/avatar';
import {FeedbackDialogService} from '../../../../../common/pages/services/feedback-dialog.service';
import {ContactEditDialogComponent} from '../edit-dialog/contact-edit-dialog.component';
import {ContactEditDialogFooterComponent} from '../edit-dialog/footer/contact-edit-dialog-footer.component';
import { Contact } from '../../../models/contact.model';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-contact-view',
  imports: [
    TranslateModule,
    PanelModule,
    DocumentUploadComponent,
    DriveDocumentUploadComponent,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    DividerModule,
    BlockUI,
    MessageModule,
    LinkListComponent,
    DatePipe,
    Avatar,
    JsonPipe,
    RouterLink
  ],
  templateUrl: './contact-view.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService]
})
export class ContactViewComponent implements OnInit, OnDestroy {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  recordPermissions = signal<any>({});
  documentService = inject(DocumentService);
  cachedDataService = inject(CachedDataService);
  contactService = inject(ContactService);
  languageService = inject(LanguageService);
  cdr = inject(ChangeDetectorRef);

  infoLoading = signal<boolean>(false);

  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);

  private langChangeSubscription: Subscription = new Subscription();
  recordId: string = '';
  recordData = signal<Contact>({});

  readonly entityTypeContact = EntityType.Contact;

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get("recordId") || '';

        if (this.recordId != '') {
          this._loadRecordDetails();
        }
      }
    });
  }

  _loadRecordDetails() {
    this.infoLoading.set(true);
    this.contactService.getContactById(this.recordId).subscribe({
      next: (data: any) => {
        this.recordData.set(data);
        this.infoLoading.set(false);
      }
    });
  }

  handleOnCancelClick(event: MouseEvent) {
    this.router.navigate(['contacts']);
  }

  handleEditClick() {
    const requestingSaveSignal = signal<boolean>(false);
    
    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: 'Edit Contact',
      width: '90vw',
      style: { maxWidth: '800px' },
      closable: true,
      templates: {
        footer: ContactEditDialogFooterComponent
      },
      data: {
        mode: 'edit',
        record: this.recordData(),
        requestingSaveSignal
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        this._loadRecordDetails();
      }
    });
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  onFileUploaded(response: any) {
    const formData = new FormData();
    for (let file of response.files) {
      formData.append('file', file);
      formData.append('parentEntityType', ParentEntityType.Contact.toString());
      formData.append('parentEntityId', this.recordId);
      formData.append('name', file.name);
      formData.append('documentTypeId', '1');
    }

    this.documentService.uploadUnopsFiles(formData).subscribe({
      next: (response: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedbackDialogService.showErrorDialog({ detail: 'Unable to upload file!' });
      },
    });
  }

  onDriveFileUploaded(response: any) {
    // TODO: allow more than one file to be uploaded if multiple is set to true
    const file = response[0];
    const req: DocumentLinkModel = {
      link: file.url,
      name: file.name,
      type: file.mimeType,
      parentEntityType: ParentEntityType.Contact,
      parentEntityId: parseInt(this.recordId),
    };

    this.documentService.linkUnopsFiles(req).subscribe({
      next: (response: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedbackDialogService.showErrorDialog({ detail: 'Unable to upload file!' });
      },
    });
  }

  onFileSelected(event: any) {
    console.log('Files selected:', event);
  }

  onFileRemoved(event: any) {
    console.log('File removed:', event);
  }

  onFilesCleared() {
    console.log('All files cleared');
  }
  protected readonly EntityType = EntityType;
}
