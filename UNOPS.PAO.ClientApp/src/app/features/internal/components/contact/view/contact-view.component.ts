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
import { PermissionService, EntityPermissions } from '../../../../../essentials/services/permission.service';

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
  permissionService = inject(PermissionService);
  translateService = inject(TranslateService);

  infoLoading = signal<boolean>(false);
  showContactInfo = signal<boolean>(false);

  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);

  private langChangeSubscription: Subscription = new Subscription();
  recordId: string = '';
  recordData = signal<Contact>({});

  // Track user permissions for the Contact entity
  permissions$ = new BehaviorSubject<EntityPermissions>({
    entity: 'Contact',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false
    }
  });

  // Convert BehaviorSubject to signal for reactivity
  permissions = toSignal(this.permissions$, {
    initialValue: {
      entity: 'Contact',
      hasAccess: false,
      permissions: {
        canRead: false,
        canCreate: false,
        canUpdate: false,
        canDelete: false
      }
    }
  });

  // Computed property to check if user can edit or create
  canEditOrCreate$ = computed(() => {
    const permissions = this.permissions();
    return permissions.permissions?.canUpdate || permissions.permissions?.canCreate || false;
  });

  readonly entityTypeContact = EntityType.Contact;

  constructor() {}

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
    // Clear permission caches when leaving the view
    this.permissionService.clearPermissionCaches();
  }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get("recordId") || '';

        if (this.recordId != '') {
          this._loadRecordDetails();
          this._loadPermissions();
        }
      }
    });
  }

  /**
   * Load permissions for the current contact
   */
  _loadPermissions() {
    // Get instance-specific permissions for this contact
    this.permissionService.getEntityInstancePermissions('Contact', this.recordId)
      .subscribe({
        next: (permissions: EntityPermissions) => {
          console.log('Loaded permissions for contact:', this.recordId, permissions);
          this.permissions$.next(permissions);
        },
        error: (error) => {
          console.error('Error loading permissions for contact:', this.recordId, error);
          // Set default permissions on error
          this.permissions$.next({
            entity: 'Contact',
            hasAccess: false,
            permissions: {
              canRead: false,
              canCreate: false,
              canUpdate: false,
              canDelete: false
            }
          });
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
        this.infoLoading.set(false);
        
        // Check if contact data includes permissions and apply them
        if (data.permissions) {
          const permissions: EntityPermissions = {
            entity: 'Contact',
            hasAccess: true,
            permissions: data.permissions
          };
          this.permissions$.next(permissions);
        }
      },
      error: (error) => {
        console.error('Error loading contact details:', error);
        this.infoLoading.set(false);
      }
    });
  }

  handleOnCancelClick(event: MouseEvent) {
    this.router.navigate(['contacts']);
  }

  handleEditClick() {
    // Check if user has update or create permission
    const permissions = this.permissions();
    const canUpdate = permissions.permissions?.canUpdate || false;
    const canCreate = permissions.permissions?.canCreate || false;
    
    if (!canUpdate && !canCreate) {
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
        this._loadPermissions();
      }
    });
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  onFileUploaded(response: any) {
    // Check if user has update permission
    const permissions = this.permissions();
    if (!permissions.permissions?.canUpdate) {
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
    const permissions = this.permissions();
    if (!permissions.permissions?.canUpdate) {
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

  getUploadProfilePictureUrl() {
    return this.contactService.getUploadProfilePictureUrl(this.recordId);
  }
}
