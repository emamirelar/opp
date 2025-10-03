import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, AfterViewInit, output, signal, computed, Input, ViewChild, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CachedDataService } from '@shared/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';


import { FeedbackDialogService } from '@shared/services/feedback-dialog.service';
import { DocumentService } from '@features/shared/services/document.service';
import { ParentEntityType } from '@features/shared/overrides/interfaces/types';
import { DocumentLinkModel } from '@features/shared/overrides/interfaces/types';
import { DocumentComponent } from '@shared/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '@features/shared/overrides/reusables/components/document/gdrive/document-gdrive.component';
import { PictureComponent } from '@shared/reusables/components/picture/picture.component';
import { AiPanelComponent } from '@shared/reusables/components/ai-panel/ai-panel.component';
import { GeminiService } from '@ai/services/gemini.service';

//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '@shared/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

//PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { CardModule } from 'primeng/card';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { LinkListComponent } from '@shared/reusables/components/link/list/link-list.component';
import { EntityType } from '@shared/models/link.model';
import { ContactEditDialogFooterComponent } from '../edit-dialog/footer/contact-edit-dialog-footer.component';
import { ContactEditDialogComponent } from '../edit-dialog/contact-edit-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { PermissionUtilityService } from '@core/services/permission-utility.service';


/**
 * @uiEntity ContactView
 * @route /partnerships/contacts/:recordId/details
 * @description View and edit detailed contact information including personal details, professional information, communication preferences, and associated documents. Central place for managing all aspects of an individual contact person.
 * @capabilities view_contact_details, edit_contact_info, upload_photo, manage_documents, view_links, update_preferences, edit_address, update_status
 * @synonyms contact_details, person_profile, individual_view, contact_information
 * @mandatoryFields firstName, lastName, email, title, partnerId
 * @help_when_stuck This page shows complete contact information. Click Edit to modify details, use the photo area to upload a new contact photo, or scroll down to see documents and links. All contact fields are organized by category for easy access.
 * @common_tasks
 *   - Editing contact info: Click the Edit button and modify the form fields
 *   - Uploading photo: Click on the photo/avatar area to upload a new contact image
 *   - Viewing interactions: Go to the Interactions tab to see communication history
 *   - Managing documents: Scroll down to the Documents section to upload or view files
 *   - Updating contact details: Edit personal, professional, or address information
 *   - Managing links: Add or edit related links and references
 */

@Component({
  selector: 'app-contact-view',
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    DividerModule,
    MessageModule,
    LinkListComponent,
    CheckboxModule,
    AiPanelComponent,
    RouterModule,
    ConfirmDialogModule
  ],
  templateUrl: './contact-view.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService, ConfirmationService],
  styles: [`
    :host ::ng-deep .custom-avatar-size {
      width: 5rem !important;
      height: 5rem !important;
      font-size: 2.5rem !important;
    }
  `]
})
export class ContactViewComponent implements OnInit, AfterViewInit, OnDestroy {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  documentService = inject(DocumentService);
  contactService = inject(ContactService);
  languageService = inject(LanguageService);
  permissionUtilityService = inject(PermissionUtilityService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  geminiService = inject(GeminiService);

  infoLoading = signal<boolean>(false);
  showContactInfo = signal<boolean>(false);
  showCommentDialog = false;

  // Computed properties for additional info visibility
  showAdditionalInfo = computed(() => {
    const data = this.recordData();
    return data.id || data.title || data.department || data.status || data.pronouns || data.description ||
           data.assistant || data.assistantPhone || data.assistantEmail ||
           data.mailingStreet || data.mailingCity || data.mailingCountry;
  });

  showFullContent = signal<boolean>(false);

  shouldShowSeeMoreButton = computed(() => {
    return this.showAdditionalInfo() && !this.showFullContent();
  });

  shouldShowSeeLessButton = computed(() => {
    return this.showAdditionalInfo() && this.showFullContent();
  });

  // Width tracking for responsive layout
  componentWidth = signal<number>(0);
  private widthTrackingInterval?: ReturnType<typeof setInterval>;
  private resizeObserver?: ResizeObserver;

  // Computed responsive layout classes based on component width
  responsiveLayoutClasses = computed(() => {
    const width = this.componentWidth();
    
    // Use component width to determine layout
    // For container widths >= 700px, use side-by-side layout
    // For container widths < 700px, use stacked layout
    // Default to stacked layout when width is 0 (measuring)
    const useSideBySideLayout = width >= 700;
    
    if (useSideBySideLayout) {
      return {
        container: 'flex flex-col gap-8',
        mainLayout: 'flex flex-row gap-8 items-stretch',
        leftColumn: 'flex flex-col gap-8 h-full sticky top-0 w-[60%]',
        rightColumn: 'w-[40%] flex flex-col gap-8'
      };
    } else {
      return {
        container: 'flex flex-col gap-8',
        mainLayout: 'flex flex-col gap-8',
        leftColumn: 'flex flex-col gap-8 h-full w-full',
        rightColumn: 'w-full flex flex-col gap-8'
      };
    }
  });

  // Debug method to check current width and layout (can be called from browser console)
  getCurrentWidth() {
    return {
      componentWidth: this.componentWidth(),
      useSideBySideLayout: this.componentWidth() >= 700,
      layoutClasses: this.responsiveLayoutClasses()
    };
  }

  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
  confirmationService = inject(ConfirmationService);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createInstancePermissions('Contact');
  recordPermissions = this.permissionUtils.recordPermissions;

  private langChangeSubscription: Subscription = new Subscription();
  
  // Input property for recordId when used in AI layout
  @Input() recordId: string = '';
  
  
  recordData = signal<Contact>({});

  readonly entityTypeContact = EntityType.Contact;

  @ViewChild('linkListComponent') linkListComponent!: LinkListComponent;
  @ViewChild('gdriveComponent') gdriveComponent!: GDriveDocumentComponent;
  @ViewChild('widthTracker', { static: false }) widthTracker?: ElementRef;

  ngAfterViewInit() {
    // Start width tracking after the view is fully initialized
    setTimeout(() => {
      this.startWidthTracking();
    }, 100);
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
    if (this.widthTrackingInterval) {
      clearInterval(this.widthTrackingInterval);
    }
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
  }

  ngOnInit() {
    
    // If recordId is provided via Input (AI layout), load data directly
    if (this.recordId && this.recordId !== '') {
      this._loadRecordDetails();
      return;
    }

    // Otherwise, use the route-based logic (normal navigation)
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

    // Width tracking will be initialized in ngAfterViewInit
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

  getUploadProfilePictureUrl() {
    return this.contactService.getUploadProfilePictureUrl(this.recordId);
  }

  toggleFullContent() {
    this.showFullContent.update(value => !value);
  }

  _handleOnViewContacts() {
    this.showCommentDialog = true;
  }

  _handleOnViewContactsDaialogClose() {
    this.showCommentDialog = false;
  }

  onSummaryRefresh() {
  }

  onSummaryLoaded(data: any) {
  }

  onSummaryError(error: any) {
    console.error('Summary error:', error);
  }

  onNewsRefresh() {
  }

  onNewsLoaded(data: any) {
  }

  onNewsError(error: any) {
    console.error('News error:', error);
  }

  /**
   * Opens the add link dialog by calling the link list component's openEditDialog method
   */
  openAddLinkDialog() {
    if (this.linkListComponent) {
      this.linkListComponent.openEditDialog();
    }
  }

  /**
   * Opens the Google Drive picker by calling the GDrive component's openGoogleDrivePicker method
   */
  openGoogleDriveDialog() {
    if (this.gdriveComponent) {
      this.gdriveComponent.openGoogleDrivePicker();
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    setTimeout(() => {
      this.updateComponentWidth();
    }, 10);
  }

  private startWidthTracking() {
    // Initial width measurement with multiple attempts
    this.attemptWidthMeasurement();

    // Use ResizeObserver for more efficient width tracking if available
    if (typeof ResizeObserver !== 'undefined' && this.widthTracker?.nativeElement) {
      this.resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const width = entry.contentRect.width;
          if (width > 0) {
            const currentWidth = this.componentWidth();
            if (currentWidth !== width) {
              this.componentWidth.set(width);
              this.cdr.detectChanges();
            }
          }
        }
      });
      
      this.resizeObserver.observe(this.widthTracker.nativeElement);
    } else {
      // Fallback to polling for older browsers
      this.widthTrackingInterval = setInterval(() => {
        this.updateComponentWidth();
      }, 100);
    }
  }

  private attemptWidthMeasurement(attempts: number = 0) {
    if (attempts > 10) {
      return;
    }

    if (this.updateComponentWidth()) {
      // Width measurement successful
    } else {
      // Try again after a short delay
      setTimeout(() => {
        this.attemptWidthMeasurement(attempts + 1);
      }, 50);
    }
  }

  private updateComponentWidth(): boolean {
    if (typeof window !== 'undefined' && this.widthTracker?.nativeElement) {
      const element = this.widthTracker.nativeElement;
      const width = element.offsetWidth || element.clientWidth || 0;
      
      if (width > 0) {
        const currentWidth = this.componentWidth();
        if (currentWidth !== width) {
          this.componentWidth.set(width);
          // Trigger change detection
          this.cdr.detectChanges();
        }
        return true;
      }
    }
    return false;
  }

  /**
   * @uiButton delete_contact
   * @description Permanently deletes a contact record after confirmation dialog
   * @label Delete
   * @icon pi pi-trash
   * @when_to_use When a contact was recorded incorrectly or is no longer relevant (use with caution)
   * @permissions CONTACT_DELETE
   */
  deleteContact(): void {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('contact.detail.error.deletePermissionDenied'),
        summary: this.translateService.instant('common.error.permissionDenied')
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.translateService.instant('contact.detail.confirmation.deleteMessage'),
      header: this.translateService.instant('contact.detail.confirmation.deleteHeader'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.contactService.deleteContactById(this.recordId).subscribe({
          next: () => {
            this.feedbackDialogService.showSuccessToast({
              detail: this.translateService.instant('contact.detail.success.deleted')
            });
            this.router.navigate(['/partnerships/contacts']);
          },
          error: (error) => {
            console.error('Error deleting contact:', error);
            this.feedbackDialogService.showErrorToast({
              detail: this.translateService.instant('contact.detail.error.deleteFailed')
            });
          }
        });
      }
    });
  }

}
