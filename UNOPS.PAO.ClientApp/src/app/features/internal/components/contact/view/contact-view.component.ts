import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { PanelModule } from 'primeng/panel';
import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { DocumentService } from '../../../services/document.service';
import { DriveDocumentUploadComponent } from '../../../overrides/reusables/components/document/drive/upload/document-drive-upload.component';
import { ParentEntityType } from '../../../overrides/interfaces/types';
import { DocumentLinkModel } from '../../../overrides/interfaces/types';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { ContactService } from '../../../services/contact.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EntityType } from '../../../../../common/models/link.model';
import { LinkListComponent } from '../../../../../common/reusables/components/link/list/link-list.component';
import { AsyncPipe, DatePipe, JsonPipe } from '@angular/common';
import { DialogService } from 'primeng/dynamicdialog';
import { AvatarModule } from 'primeng/avatar';
import { FeedbackDialogService } from '../../../../../common/reusables/services/feedback-dialog.service';
import { ContactEditDialogComponent } from '../edit-dialog/contact-edit-dialog.component';
import { ContactEditDialogFooterComponent } from '../edit-dialog/footer/contact-edit-dialog-footer.component';
import { Contact } from '../../../models/contact.model';
import { ContactViewInteractionsComponent } from './interactions/contact-view-interactions.component';
import { PictureComponent } from '../../../../../common/reusables/components/picture/picture.component';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';

@Component({
  selector: 'app-contact-view',
  imports: [
    TranslateModule,
    PanelModule,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    DividerModule,
    MessageModule,
    LinkListComponent,
    DatePipe,
    AsyncPipe,
    AvatarModule,
    RouterLink,
    ContactViewInteractionsComponent,
    PictureComponent
  ],
  templateUrl: './contact-view.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService],
  styles: [`
    :host ::ng-deep .custom-avatar-size {
      width: 5rem !important;
      height: 5rem !important;
      font-size: 2.5rem !important;
    }
  `]
})
export class ContactViewComponent implements OnInit, OnDestroy {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  documentService = inject(DocumentService);
  contactService = inject(ContactService);
  languageService = inject(LanguageService);
  permissionUtilityService = inject(PermissionUtilityService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);

  infoLoading = signal<boolean>(false);
  showContactInfo = signal<boolean>(false);

  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createInstancePermissions('Contact');
  recordPermissions = this.permissionUtils.recordPermissions;

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
          // Check if data is already available from the resolver
          this.activatedRoute.parent?.data.subscribe(data => {
            if (data['contactData']) {
              const contactData = data['contactData'];
              this.recordData.set(contactData);
              
              // Extract permissions from the resolver data if they exist
              if (contactData.permissions) {
                this.recordPermissions.set({
                  entity: 'Contact',
                  hasAccess: true,
                  permissions: contactData.permissions
                });
              }
              
              this.infoLoading.set(false);
            } else {
              // Fallback to loading details directly if resolver data isn't available
              this._loadRecordDetails();
            }
          });
          
          // Load permissions for this specific contact
          // Permissions are now extracted from the contact response directly
        }
      }
    });
  }

  /**
   * Load contact record details
   */
  _loadRecordDetails() {
    this.infoLoading.set(true);
    this.contactService.getContactById(this.recordId).subscribe({
      next: (data: any) => {
        this.recordData.set(data);
        
        // Extract permissions from the response if they exist
        if (data.permissions) {
          this.recordPermissions.set({
            entity: 'Contact',
            hasAccess: true,
            permissions: data.permissions
          });
        }
        
        this.infoLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading contact details:', error);
        this.infoLoading.set(false);
      }
    });
  }

  handleEditClick() {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit this contact',
        summary: 'Permission Denied'
      });
      return;
    }

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

  onFileUploaded(response: any) {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to upload documents for this contact',
        summary: 'Permission Denied'
      });
      return;
    }

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
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to upload documents for this contact',
        summary: 'Permission Denied'
      });
      return;
    }

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
    
  }

  onFileRemoved(event: any) {
    
  }

  onFilesCleared() {
    
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }
}
