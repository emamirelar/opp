import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal, computed, Input, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { DocumentService } from '../../../services/document.service';
import { ParentEntityType } from '../../../overrides/interfaces/types';
import { DocumentLinkModel } from '../../../overrides/interfaces/types';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';


//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
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
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { PartnerService } from '../../../services/partner.service';
import { PartnerContactsComponent } from '../contacts/partner-contacts.component';
import { LinkListComponent } from '../../../../../common/reusables/components/link/list/link-list.component';
import { EntityType } from '../../../../../common/models/link.model';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerViewContactsComponent } from './contacts/partner-view-contacts.component';
import { Partner, getPrimaryOrganizationUnit } from '../../../models/partner.model';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { AiPanelComponent } from '../../../../../common/reusables/components/ai-panel/ai-panel.component';
import { GeminiService } from '../../../services/gemini.service';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { GoBackComponent } from '../../../../../common/reusables/components/go-back/go-back.component';
import { PartnerEditDialogComponent } from '../edit-dialog/partner-edit-dialog.component';
import { PartnerEditDialogFooterComponent } from '../edit-dialog/footer/partner-edit-dialog-footer.component';
import { PartnerApprovalDialogComponent } from '../approval-dialog/partner-approval-dialog.component';
import { AuthService } from '../../../../../essentials/services/auth.service';
import { EntityTagsComponent } from '../../../../../common/components/entity-tags/entity-tags.component';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { BaseEngagementListComponent } from '../../base-engagement/base-engagement-list.component';

/**
 * @uiEntity Partner
 * @route /partnerships/partners/:id
 * @description View and edit detailed partner information including contact details, address, organizational data, and associated documents. Central place for managing all aspects of a partner organization.
 * @capabilities view_partner_details, edit_partner_info, upload_logo, manage_documents, view_contacts, create_interactions, edit_address, update_status
 * @synonyms organization_details, partner_profile, entity_view, collaborator_info
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 * @help_when_stuck This page shows complete partner information. Click Edit to modify details, use tabs to navigate between sections, or click the logo area to upload a new partner logo. All fields are organized by category for easy access.
 * @common_tasks
 *   - Editing partner info: Click the Edit button and modify the form fields
 *   - Uploading logo: Click on the logo/image area to upload a new partner logo
 *   - Viewing contacts: Go to the Contacts tab to see people associated with this partner
 *   - Adding interactions: Go to Interactions tab and click 'Add Interaction'
 *   - Managing documents: Scroll down to the Documents section to upload or view files
 *   - Updating address: Edit the address fields in the Contact Information section
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
@Component({
  selector: 'app-partner-view',
  imports: [
    CommonModule,
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    AutoFocusModule,
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    FormsModule,
    PartnerContactsComponent,
    LinkListComponent,
    TooltipModule,
    AiPanelComponent,
    RouterModule,
    ConfirmDialogModule,
    EntityTagsComponent,
    BaseEngagementListComponent,
  ],
  templateUrl: './partner-view.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService, ConfirmationService],
  styles: [`
    :host ::ng-deep .custom-avatar-size {
      width: 5rem !important;
      height: 5rem !important;
      font-size: 2.5rem !important;
    }

    .ai-panel .p-panel {
      box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05) !important;
      border-radius: 0.5rem !important;
    }
    .ai-panel ::ng-deep .p-panel-content {
      border-bottom-left-radius: 8px !important;
      border-bottom-right-radius: 8px !important;
    }

  `]
})
export class PartnerViewComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  documentService = inject(DocumentService);
  dialogService = inject(DialogService);

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  partnerService = inject(PartnerService);
  geminiService = inject(GeminiService);
  translateService = inject(TranslateService);
  languageService = inject(LanguageService);
  cdr = inject( ChangeDetectorRef);
  permissionService = inject(PermissionUtilityService);
  authService = inject(AuthService);
  confirmationService = inject(ConfirmationService);

  // Permission management using utility service
  private permissionUtils = this.permissionService.createInstancePermissions('Partner');
  recordPermissions = this.permissionUtils.recordPermissions;

  private langChangeSubscription: Subscription = new Subscription();
  onRecordCreationSuccess = output();

  // Input property for recordId when used in AI layout
  @Input() recordId: string = '';

  // Input property to control AI panel visibility
  private _showAiPanel: boolean = true;
  @Input()
  get showAiPanel(): boolean {
    return this._showAiPanel;
  }
  set showAiPanel(value: boolean | null | undefined) {
    this._showAiPanel = value === false ? false : true; // Default to true unless explicitly false
  }

  showValidationFailedError = signal<boolean>(false);
  recordData = signal<Partner>({});
  showCommentDialog = false;
  entityTypePartner = EntityType.Partner;
  infoLoading = signal<boolean>(false);

  // ViewChild reference for link list component
  @ViewChild('linkListComponent') linkListComponent!: LinkListComponent;
  
  // ViewChild reference for document component
  @ViewChild('appDocument') documentComponent!: DocumentComponent;
  
  // ViewChild reference for GDrive document component
  @ViewChild('gdriveComponent') gdriveComponent!: GDriveDocumentComponent;

  //To be handled by permissions later so that only PRM Admin has this value set to true
  showAdditionalInfo = signal<boolean>(true);

  // See More functionality for Partner Information
  showFullContent = signal<boolean>(false);

  // Computed values for See More functionality
  shouldShowSeeMoreButton = computed(() => {
    return this.showAdditionalInfo() && !this.showFullContent();
  });

  shouldShowSeeLessButton = computed(() => {
    return this.showAdditionalInfo() && this.showFullContent();
  });

  // Helper method to get primary organization unit
  getPrimaryOrganizationUnit = getPrimaryOrganizationUnit;

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  ngOnInit() {
    console.log('PartnerView ngOnInit - showAiPanel value:', this.showAiPanel);

    // Check admin role
    this.authService.isAdmin().subscribe({
      next: (isAdmin) => {
        this.isAdmin.set(isAdmin);
      },
      error: (error) => {
        console.error('Error checking admin role:', error);
        this.isAdmin.set(false);
      }
    });

    // If recordId is provided via Input (AI layout), load data directly
    if (this.recordId && this.recordId !== '') {
      console.log('Using input recordId:', this.recordId);
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
            if (data['partnerData']) {
              const partnerData = data['partnerData'];
              this.recordData.set(partnerData);

              // Extract permissions from the resolver data if they exist
              if (partnerData.permissions) {
                this.recordPermissions.set({
                  entity: 'Partner',
                  hasAccess: true,
                  permissions: partnerData.permissions
                });
              }

              this.infoLoading.set(false);
            } else {
              // Fallback to loading details directly if resolver data isn't available
              this._loadRecordDetails();
            }
          });

          // Load permissions for this specific partner
          // Permissions are now extracted from the partner response directly
        }
      }
    });

    this.activatedRoute.queryParamMap.subscribe({
      next: (paramMap) => {
        if (this.recordId != '' && paramMap.get('show-contacts')?.toLowerCase() == 'true') {
          this._handleOnViewContacts();
        } else {
          this.showCommentDialog = false;
        }
      },
    });
  }

  _loadRecordDetails() {
    //fetch record details
    this.infoLoading.set(true);
    this.partnerService.getPartnerById(this.recordId).subscribe({
      next: (data: any) => {
        this.recordData.set(data);

        // Extract permissions from the response if they exist
        if (data.permissions) {
          this.recordPermissions.set({
            entity: 'Partner',
            hasAccess: true,
            permissions: data.permissions
          });
        }

        this.infoLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading partner details:', error);
        this.infoLoading.set(false);
      }
    });
  }

  handleOnCancelClick(event: MouseEvent) {
    this.router.navigate(['partners']);
  }

  // AI Panel Event Handlers
  onSummaryRefresh() {

  }

  onSummaryLoaded(data: string) {

  }

  onSummaryError(error: Error) {
    console.error('Summary error:', error);
  }

  onNewsRefresh() {

  }

  onNewsLoaded(data: string) {

  }

  onNewsError(error: Error) {
    console.error('News error:', error);
  }

  _handleOnViewContacts() {
    this.showCommentDialog = true;

    this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: {
        'show-contacts': true,
      },
    });
  }

  _handleOnViewContactsDaialogClose() {
    this.showCommentDialog = false;

    this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: {},
    });
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  onFileUploaded(response: any) {
    const formData = new FormData();
    for (let file of response.files) {
      formData.append('file', file);
      formData.append('parentEntityType', ParentEntityType.Partner.toString());
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
      parentEntityType: ParentEntityType.Partner,
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

  /**
   * @uiButton edit_partner
   * @description Opens the partner editing dialog with form fields for modifying partner organization information
   * @label Edit Partner
   * @icon pi pi-pencil
   * @when_to_use When partner information needs updating, correcting partner details, or adding new organizational information
   * @permissions PARTNER_UPDATE
   */
  handleEditClick() {
    // Check if user has update permission
    if (!this.permissionService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit this partner',
        summary: 'Permission Denied'
      });
      return;
    }

    const requestingSaveSignal = signal<boolean>(false);

    const ref = this.dialogService.open(PartnerEditDialogComponent, {
      header: 'Edit Partner',
      width: '90vw',
      style: { maxWidth: '800px' },
      closable: true,
      templates: {
        footer: PartnerEditDialogFooterComponent
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

  getUploadLogoUrl() {
    return this.partnerService.getUploadLogoUrl(this.recordId);
  }

  /**
   * Check if current user is admin (Partnership Global Admin)
   */
  isAdmin = signal<boolean>(false);

  /**
   * Check if current user can edit the partner
   * Rules:
   * - User must have update permissions
   * - If partner is approved, only admin users can edit
   * - If partner is not approved, regular users with permissions can edit
   */
  canEditPartner = computed(() => {
    const hasUpdatePermission = this.recordPermissions().permissions.canUpdate;
    const isApproved = this.recordData().partnerApprovalStatus === 'Approved';

    if (!hasUpdatePermission) {
      return false;
    }

    // If partner is approved, only admin can edit
    if (isApproved) {
      return this.isAdmin();
    }

    // If partner is not approved, any user with update permission can edit
    return true;
  });

  /**
   * @uiButton approve_partner
   * @description Opens approval confirmation dialog and then approval dialog for users to approve partners
   * @label Approve
   * @icon pi pi-check-circle
   * @when_to_use When partner needs to be approved and user has approval privileges
   * @permissions canApprove
   */
  handleApprovalClick() {
    console.log('Approval button clicked for partner:', this.recordData().name);

    // Show confirmation dialog
    this.confirmationService.confirm({
      message: `Are you sure you want to approve the partner "${this.recordData().name}"? This action cannot be undone.`,
      header: 'Confirm Approval',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        console.log('Approval confirmed, opening approval dialog');
        this.openApprovalDialog();
      },
      reject: () => {
        console.log('Approval cancelled');
      }
    });
  }

  /**
   * Opens the approval dialog with approval-related fields
   */
  private openApprovalDialog() {
    const ref = this.dialogService.open(PartnerApprovalDialogComponent, {
      header: 'Partner Approval',
      width: '90vw',
      style: { maxWidth: '800px' },
      closable: true,
      data: {
        partner: this.recordData()
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Reload partner details to show updated approval status
        this._loadRecordDetails();
      }
    });
  }

  /**
   * @uiButton activate_partner
   * @description Opens activation confirmation dialog and activates the partner
   * @label Activate
   * @icon pi pi-power-off
   * @when_to_use When partner needs to be activated and user has activation privileges
   * @permissions canActivate
   */
  handleActivateClick() {
    console.log('Activate button clicked for partner:', this.recordData().name);

    // Check if required fields are missing before proceeding
    const partner = this.recordData();
    const missingFields = this.checkRequiredFieldsForActivation(partner);

    if (missingFields.length > 0) {
      // Open edit dialog with activation validation mode
      this.openEditDialogForActivation();
      return;
    }

    // Show confirmation dialog
    this.confirmationService.confirm({
      message: this.translateService.instant('message.confirmPartnerActivation', {
        partnerName: this.recordData().name
      }),
      header: this.translateService.instant('message.confirmActivation'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        console.log('Activation confirmed, calling API');
        this.activatePartner();
      },
      reject: () => {
        console.log('Activation cancelled');
      }
    });
  }

  /**
   * Calls the activate API endpoint
   */
  private activatePartner() {
    this.partnerService.activatePartner(this.recordId).subscribe({
      next: (result) => {
        console.log('Partner activated successfully:', result);
        this.feedbackDialogService.showSuccessToast({
          detail: this.translateService.instant('message.partnerActivatedSuccessfully', {
            partnerName: this.recordData().name
          })
        });
        // Reload partner details to show updated status and permissions
        this._loadRecordDetails();
      },
      error: (error) => {
        console.error('Error activating partner:', error);
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('message.failedToActivatePartner')
        });
      }
    });
  }

  /*selectOrganizationalStructure(type: 'summary' | 'risk' | 'news') {


    const ref = this.dialogService.open(OrgStructureDialogComponent, {
      header: 'Select Organizational Structure',
      width: '95vw',
      height: '95vh',
      style: {
        maxWidth: '1400px',
        maxHeight: '900px',
        backgroundColor: 'white',
        padding: '0'
      },
      contentStyle: {
        padding: '0',
        overflow: 'hidden',
        backgroundColor: 'white'
      },
      baseZIndex: 10000,
      dismissableMask: true,
      closeOnEscape: true,
      closable: true,
      data: {
        type: type,
        partnerId: this.recordId
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {

        // Refresh the corresponding panel based on type
        switch (type) {
          case 'summary':
            this.refreshSummaryOfInteractions();
            break;
          case 'risk':
            this.refreshRiskProfile();
            break;
          case 'news':
            this.refreshPartnerNews();
            break;
        }
      }
    });
  }*/

  toggleFullContent() {
    this.showFullContent.set(!this.showFullContent());
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
   * Opens the upload document dialog by calling the document component's openUploadDialog method
   */
  openUploadDialog() {
    if (this.documentComponent) {
      this.documentComponent.openUploadDialog();
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



  // Note: To document buttons/actions, add @uiButton JSDoc comments above existing methods
  // Example for documenting existing methods:
  // /**
  //  * @uiButton edit_partner
  //  * @description Switches to edit mode for partner information
  //  * @label Edit Partner
  //  * @icon pi pi-pencil
  //  * @when_to_use When partner information needs updating, correcting details, adding new information
  //  * @permissions PARTNER_UPDATE
  //  */
  // existingEditMethod() { ... }

  /**
   * Check if required fields for activation are missing
   */
  private checkRequiredFieldsForActivation(partner: any): string[] {
    const missingFields: string[] = [];

    if (!partner.name) {
      missingFields.push('name');
    }
    if (!partner.partnerShortDescription) {
      missingFields.push('partnerShortDescription');
    }
    if (!partner.partnerCategoryId) {
      missingFields.push('partnerCategoryId');
    }
    if (!partner.partnerGroupId) {
      missingFields.push('partnerGroupId');
    }
    if (!partner.liaisonOfficeId) {
      missingFields.push('liaisonOfficeId');
    }

    return missingFields;
  }

  /**
   * Opens the edit dialog in activation validation mode
   */
  private openEditDialogForActivation() {
    const requestingSaveSignal = signal<boolean>(false);

    const ref = this.dialogService.open(PartnerEditDialogComponent, {
      header: this.translateService.instant('title.partnerTitles.completeRequiredFields'),
      width: '90vw',
      style: { maxWidth: '800px' },
      closable: true,
      templates: {
        footer: PartnerEditDialogFooterComponent
      },
      data: {
        mode: 'edit',
        record: this.recordData(),
        validationMode: 'activate',
        requestingSaveSignal
      }
    });

    ref.onClose.subscribe((result: any) => {
      if (result === "saved" || (result && result.id)) {
        // Partner was updated, refresh the data and try activation again
        this._loadRecordDetails();
      }
    });
  }


  
  /**
   * Convert recordId string to number for use with BaseEngagementListComponent
   */
  get partnerIdAsNumber(): number | undefined {
    return this.recordId ? parseInt(this.recordId, 10) : undefined;
  }

}
