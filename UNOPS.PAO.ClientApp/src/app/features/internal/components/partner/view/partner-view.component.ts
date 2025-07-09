import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal, computed, Input } from '@angular/core';
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
import { PictureComponent } from "../../../../../common/reusables/components/picture/picture.component";

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
import { PartnerService } from '../../../services/partner.service';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { PartnerContactsComponent } from '../contacts/partner-contacts.component';
import { GeminiService } from '../../../services/gemini.service';
import { LinkListComponent } from "../../../../../common/reusables/components/link/list/link-list.component";
import { EntityType } from '../../../../../common/models/link.model';
import { PartnerEditDialogFooterComponent } from '../edit-dialog/footer/partner-edit-dialog-footer.component';
import { PartnerEditDialogComponent } from '../edit-dialog/partner-edit-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerViewContactsComponent } from './contacts/partner-view-contacts.component';
import { Partner } from '../../../models/partner.model';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { AiPanelComponent } from '../../../../../common/reusables/components/ai-panel/ai-panel.component';
import { GoBackComponent } from '../../../../../common/reusables/components/go-back/go-back.component';

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
    PictureComponent,
    PartnerViewContactsComponent,
    TooltipModule,
    AiPanelComponent,
    RouterModule,
    GoBackComponent,
  ],
  templateUrl: './partner-view.component.html',
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

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  ngOnInit() {
    console.log('PartnerView ngOnInit - showAiPanel value:', this.showAiPanel);
    
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

}
