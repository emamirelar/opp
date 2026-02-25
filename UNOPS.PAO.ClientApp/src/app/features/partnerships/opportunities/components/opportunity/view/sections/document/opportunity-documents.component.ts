/**
 * @fileoverview Opportunity Documents Component - Manages document uploads and links for opportunities
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  inject,
  input,
  output,
  signal,
  computed,
  effect,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

// PrimeNG imports
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { FloatLabelModule } from 'primeng/floatlabel';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';

// Services
import { DocumentService } from '@shared/services/api/document.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { OpportunityService } from '@app/features/partnerships/opportunities/services/opportunity.service';
import { GoogleDriveService } from '@shared/services/google-drive.service';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';

// Components
import {
  AiComparisonComponent,
  FieldMapping,
} from '@shared/components/ai/ai-comparison/ai-comparison.component';

declare const google: any;

/**
 * @class OpportunityDocumentsComponent
 * @description Component for managing documents in the opportunity view sidebar.
 * Supports uploading documents from local system and linking documents from Google Drive.
 *
 * @example
 * ```html
 * <app-opportunity-documents
 *   [opportunityId]="opportunity().id"
 *   [collapsed]="documentsCollapsed()"
 *   (collapsedChange)="toggleDocumentsPanel()">
 * </app-opportunity-documents>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-documents',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    BadgeModule,
    TooltipModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    MessageModule,
    SelectModule,
    FloatLabelModule,
    CheckboxModule,
    FormsModule,
    AiComparisonComponent,
  ],
  templateUrl: './opportunity-documents.component.html',
  styleUrls: ['./opportunity-documents.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityDocumentsComponent implements OnInit {
  private readonly documentService = inject(DocumentService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);
  private readonly drivePickerService = inject(DrivePickerService);
  private readonly opportunityService = inject(OpportunityService);
  private readonly googleDriveService = inject(GoogleDriveService);
  private readonly googleOAuthService = inject(GoogleOAuthService);

  // Google Drive auth for Office file conversion
  private googleDriveAuthAvailable = false;

  // Conversion progress
  private isConvertingFile = false;
  private conversionMessage = '';

  /**
   * @description Opportunity ID to fetch documents for
   * @type {Signal<number>}
   */
  readonly opportunityId = input.required<number>();

  /**
   * @description The full opportunity object with partner details
   * @type {Signal<any>}
   */
  readonly opportunity = input.required<any>();

  /**
   * @description Event emitted when opportunity data changes and parent should reload
   * @type {OutputEmitterRef<void>}
   */
  readonly opportunityUpdated = output<void>();

  /**
   * @description Whether the documents panel is collapsed
   * @type {Signal<boolean>}
   * @default false
   */
  readonly collapsed = input<boolean>(false);

  /**
   * @description Whether the user can update the opportunity (and thus manage documents)
   * @type {Signal<boolean>}
   * @default false
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Output event emitted when the collapse/expand button is clicked
   * @type {OutputEmitterRef<void>}
   */
  readonly togglePanel = output<void>();

  /**
   * @description Documents list
   * @type {Signal<any[]>}
   */
  documents = signal<any[]>([]);

  /**
   * @description Document types list
   * @type {Signal<any[]>}
   */
  documentTypes = signal<any[]>([]);

  /**
   * @description Selected document type for upload
   * @type {number | null}
   */
  selectedDocumentType: number | null = null;

  /**
   * @description Loading state
   * @type {Signal<boolean>}
   */
  loading = signal<boolean>(false);

  /**
   * @description Show upload dialog
   * @type {Signal<boolean>}
   */
  showUploadDialog = signal<boolean>(false);

  /**
   * @description Show link dialog
   * @type {Signal<boolean>}
   */
  showLinkDialog = signal<boolean>(false);

  /**
   * @description Selected file for upload
   * @type {File | null}
   */
  selectedFile: File | null = null;

  /**
   * @description Google Drive link input
   * @type {string}
   */
  googleDriveLink = '';

  /**
   * @description Google Drive ID extracted from link
   * @type {string}
   */
  googleDriveId = '';

  /**
   * @description Selected Google Drive file object
   * @type {any}
   */
  selectedGoogleDriveFile: any = null;

  /**
   * @description Uploading state
   * @type {Signal<boolean>}
   */
  uploading = signal<boolean>(false);

  /**
   * @description Show validation error in upload dialog
   * @type {Signal<boolean>}
   */
  showUploadValidationError = signal<boolean>(false);

  /**
   * @description Show validation error in link dialog
   * @type {Signal<boolean>}
   */
  showLinkValidationError = signal<boolean>(false);

  /**
   * @description ID of document currently being transcribed by AI
   * @type {Signal<number | null>}
   */
  transcribingDocId = signal<number | null>(null);

  /**
   * @description Show AI comparison dialog
   * @type {Signal<boolean>}
   */
  showComparisonDialog = signal<boolean>(false);

  /**
   * @description AI-extracted opportunity data
   * @type {Signal<any>}
   */
  aiExtractedData = signal<any>(null);

  /**
   * @description Show partner selection dialog for document tagging
   * @type {Signal<boolean>}
   */
  showPartnerTagDialog = signal<boolean>(false);

  /**
   * @description Document being tagged with partner
   * @type {any}
   */
  documentBeingTagged: any = null;

  /**
   * @description Selected funding partners for document
   * @type {number[]}
   */
  selectedFundingPartners: number[] = [];

  /**
   * @description Selected client partners for document
   * @type {number[]}
   */
  selectedClientPartners: number[] = [];
  
  /**
   * @description Original funding partners (before changes)
   * @type {number[]}
   */
  private originalFundingPartners: number[] = [];
  
  /**
   * @description Original client partners (before changes)
   * @type {number[]}
   */
  private originalClientPartners: number[] = [];
  
  /**
   * @description Show partner tag validation error
   * @type {Signal<boolean>}
   */
  showPartnerTagValidationError = signal<boolean>(false);
  
  /**
   * @description Check if partner selection has changed
   * @type {Signal<boolean>}
   */
  readonly hasPartnerSelectionChanged = signal<boolean>(false);

  /**
   * @description Field mappings for opportunity comparison display
   * @type {FieldMapping[]}
   */
  readonly opportunityFieldMappings: FieldMapping[] = [
    {
      fieldPath: 'name',
      displayName: 'Opportunity Name',
    },
    {
      fieldPath: 'description',
      displayName: 'Description',
    },
    {
      fieldPath: 'responsibleOrgUnitName',
      displayName: 'Responsible Organization Unit',
    },
    {
      fieldPath: 'proposedInitiativeTypeId',
      displayName: 'Initiative Type (ID)',
    },
    {
      fieldPath: 'proposedInitiativeTypeName',
      displayName: 'Initiative Type',
    },
    {
      fieldPath: 'initiativeBudgetUSD',
      displayName: 'Total Budget (USD)',
      formatFn: (value) =>
        value != null
          ? `$${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
          : '-',
    },
    {
      fieldPath: 'targetSigningDate',
      displayName: 'Target Signing Date',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'isTargetSigningDateFirm',
      displayName: 'Firm Signing Date',
      formatFn: (value) => (value ? 'Yes' : 'No'),
    },
    {
      fieldPath: 'signingDateNotes',
      displayName: 'Signing Date Notes',
    },
    {
      fieldPath: 'submissionDeadline',
      displayName: 'Proposal Submission Deadline',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'implementationStartDate',
      displayName: 'Implementation Start Date',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'targetDeliveryDate',
      displayName: 'Target Delivery Date',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'challenges',
      displayName: 'Context and Challenges',
    },
    {
      fieldPath: 'resultsFocus',
      displayName: 'Results Focus',
    },
    {
      fieldPath: 'expectedImpact',
      displayName: 'Expected Impact',
    },
    {
      fieldPath: 'expectedOutcomes',
      displayName: 'Expected Outcomes',
    },
    {
      fieldPath: 'expectedBeneficiaries',
      displayName: 'Expected Beneficiaries',
    },
    {
      fieldPath: 'estimatedDirectBeneficiaries',
      displayName: 'Estimated Direct Beneficiaries',
      formatFn: (value) =>
        value != null ? `${value.toLocaleString('en-US')} people` : '-',
    },
    {
      fieldPath: 'estimatedIndirectBeneficiaries',
      displayName: 'Estimated Indirect Beneficiaries',
      formatFn: (value) =>
        value != null ? `${value.toLocaleString('en-US')} people` : '-',
    },
    {
      fieldPath: 'beneficiariesToBeDetermined',
      displayName: 'Beneficiaries To Be Determined',
      formatFn: (value) => (value ? 'Yes' : 'No'),
    },
    {
      fieldPath: 'deliveryModality',
      displayName: 'Delivery Modality',
      formatFn: (value) => {
        if (!value) return '-';
        const modalityMap: { [key: number]: string } = {
          1: 'Not Yet Known',
          2: 'All Direct - UNOPS will be delivering all Products & Services directly',
          3: 'All Grant Support - All Products & Services will be delivered via Grant Support',
          4: 'Mixed - Some Products and Services will be delivered via Grant Support Modality'
        };
        return modalityMap[value as number] || `Unknown (${value})`;
      },
    },
    {
      fieldPath: 'miscExternalStakeholders',
      displayName: 'External Stakeholders (Other)',
    },
    {
      fieldPath: 'externalStakeholderNotes',
      displayName: 'External Stakeholder Notes',
    },
   {
      fieldPath: 'fundingPartners',
      displayName: 'Funding Partners',
      formatFn: (value) =>
        Array.isArray(value) ? `${value.length} partner(s)` : '0 partners',
    },
    {
      fieldPath: 'clientPartners',
      displayName: 'Client Partners',
      formatFn: (value) =>
        Array.isArray(value) ? `${value.length} partner(s)` : '0 partners',
    },
    {
      fieldPath: 'stakeholders',
      displayName: 'Stakeholders',
      formatFn: (value) =>
        Array.isArray(value)
          ? `${value.length} stakeholder(s)`
          : '0 stakeholders',
    },
    {
      fieldPath: 'deliverables',
      displayName: 'Products & Services',
      formatFn: (value) =>
        Array.isArray(value)
          ? `${value.length} product(s) & service(s)`
          : '0 products & services',
    },
    {
      fieldPath: 'countries',
      displayName: 'Countries',
      formatFn: (value) =>
        Array.isArray(value) ? `${value.length} country/ies` : '0 countries',
    },
    {
      fieldPath: 'sdGs',
      displayName: 'Sustainable Development Goals (SDGs)',
      formatFn: (value) =>
        Array.isArray(value) ? `${value.length} SDG(s)` : '0 SDGs',
    },
    {
      fieldPath: 'unopsMissions',
      displayName: 'Alignment to UNOPS Strategic Missions',
      formatFn: (value) =>
        Array.isArray(value)
          ? `${value.length} mission(s)`
          : '0 missions',
    },
  ];

  /**
   * @description Computed chevron icon based on collapsed state
   * @type {Signal<string>}
   */
  chevronIcon = computed(() => {
    return this.collapsed() ? 'pi-chevron-right' : 'pi-chevron-left';
  });

  /**
   * @description Computed document count
   * @type {Signal<number>}
   */
  documentCount = computed(() => {
    return this.documents().length;
  });

  constructor() {
    // Effect to reload documents when opportunity ID changes
    effect(() => {
      const oppId = this.opportunityId();
      if (oppId) {
        this.loadDocuments();
      }
    });
  }

  ngOnInit(): void {
    // Load document types
    this.loadDocumentTypes();

    // Initialize Google Drive auth for Office file conversion
    // Note: GoogleDriveService now has built-in retry logic to wait for configuration to load
    this.googleDriveService.initializeAuth().subscribe({
      next: (authAvailable) => {
        this.googleDriveAuthAvailable = authAvailable;
        if (authAvailable) {
        } else {
          console.warn(
            '⚠️ Google Drive auth not available - Office file conversion will not be possible',
          );
          console.warn(
            'Check console for detailed error messages from GoogleDriveService',
          );
        }
      },
      error: (error) => {
        console.error('❌ Failed to initialize Google Drive auth:', error);
        this.googleDriveAuthAvailable = false;
      },
    });
  }

  /**
   * @description Toggle the panel collapsed state
   * @returns {void}
   */
  onTogglePanel(): void {
    this.togglePanel.emit();
  }

  /**
   * @description Load document types for Opportunity entity
   * @returns {void}
   */
  loadDocumentTypes(): void {
    this.documentService.getDocumentTypesByEntityName('Opportunity').subscribe({
      next: (types: any) => {
        const documentTypesArray = types.records || [];
        this.documentTypes.set(documentTypesArray);
      },
      error: (error: any) => {
        console.error('Error loading document types:', error);
        this.documentTypes.set([]); // Ensure it's always an array even on error
      },
    });
  }

  /**
   * @description Load documents for the opportunity
   * @returns {void}
   */
  loadDocuments(): void {
    this.loading.set(true);
    this.documentService
      .getDocumentsByEntity('Opportunity', this.opportunityId())
      .subscribe({
        next: (docs: any) => {
          // Ensure docs is always an array
          const documentArray = Array.isArray(docs) ? docs : [];
          this.documents.set(documentArray);
          this.loading.set(false);
        },
        error: (error: any) => {
          console.error('Error loading documents:', error);
          this.documents.set([]); // Ensure it's always an array even on error
          this.loading.set(false);
        },
      });
  }

  /**
   * @description Open upload dialog
   * @returns {void}
   */
  openUploadDialog(): void {
    this.selectedFile = null;
    this.selectedDocumentType = null;
    this.showUploadValidationError.set(false);
    this.showUploadDialog.set(true);
  }

  /**
   * @description Open Google Drive picker to link documents
   * @returns {void}
   */
  openLinkDialog(): void {
    // Set accepted MIME types for documents (PDF, Word, Excel, PowerPoint)
    const acceptedMIMETypes = [
      'application/pdf',
      'application/vnd.google-apps.document',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/msword',
      'application/vnd.google-apps.spreadsheet',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/vnd.ms-excel',
      'application/vnd.google-apps.presentation',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation',
      'application/vnd.ms-powerpoint',
    ].join(',');

    this.drivePickerService.setAcceptedMIMETypes(acceptedMIMETypes);

    // Subscribe to file selection events
    const subscription =
      this.drivePickerService.onFilesSelectedEmitter.subscribe({
        next: (event: any) => {
          this.handleGoogleDriveFilesSelected(event);
          subscription.unsubscribe(); // Clean up subscription
        },
      });

    // Open the Google Drive picker
    this.drivePickerService.openPicker();
  }

  /**
   * @description Handle files selected from Google Drive picker
   * @param {any} event - Event containing selected files
   * @returns {void}
   */
  private handleGoogleDriveFilesSelected(event: any): void {
    if (event.files && event.files.length > 0) {
      const selectedFiles = event.files;

      // If multiple files selected, link them one by one
      selectedFiles.forEach((file: any) => {
        const googleId = file.id;
        const fileName = file.name;
        const fileUrl =
          file.url || `https://drive.google.com/file/d/${googleId}/view`;

        // Show a dialog to select document type for this file
        this.showGoogleDriveFileDialog(file);
      });
    }
  }

  /**
   * @description Show dialog to select document type for Google Drive file
   * @param {any} file - Google Drive file object
   * @returns {void}
   */
  private showGoogleDriveFileDialog(file: any): void {
    this.selectedGoogleDriveFile = file;
    this.googleDriveLink =
      file.url || `https://drive.google.com/file/d/${file.id}/view`;
    this.googleDriveId = file.id;
    this.selectedDocumentType = null;
    this.showLinkValidationError.set(false);
    this.showLinkDialog.set(true);
  }

  /**
   * @description Cancel upload dialog
   * @returns {void}
   */
  cancelUpload(): void {
    this.showUploadDialog.set(false);
    this.selectedFile = null;
    this.selectedDocumentType = null;
    this.showUploadValidationError.set(false);
  }

  /**
   * @description Cancel link dialog
   * @returns {void}
   */
  cancelLink(): void {
    this.showLinkDialog.set(false);
    this.googleDriveLink = '';
    this.googleDriveId = '';
    this.selectedGoogleDriveFile = null;
    this.selectedDocumentType = null;
    this.showLinkValidationError.set(false);
  }

  /**
   * @description Handle file selection
   * @param {Event} event - File input change event
   * @returns {void}
   */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.showUploadValidationError.set(false);
    }
  }

  /**
   * @description Confirm file upload
   * @returns {void}
   */
  async confirmUpload(): Promise<void> {
    // Validate
    if (!this.selectedFile || !this.selectedDocumentType) {
      this.showUploadValidationError.set(true);
      return;
    }

    // Check file type - allow Office files AND PDFs
    const fileExt =
      '.' + this.selectedFile.name.split('.').pop()?.toLowerCase();
    const isOfficeFile = this.googleDriveService.isMicrosoftOfficeFile(
      this.selectedFile.type,
    );
    const isPdf = fileExt === '.pdf';

    if (!isOfficeFile && !isPdf) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant(
          'message.document.unsupportedFileType',
        ),
      });
      return;
    }

    this.uploading.set(true);

    try {
      // Convert Office files to PDF if needed
      let fileToUpload = this.selectedFile;
      if (isOfficeFile) {
        // If auth not available, try to initialize it now
        if (!this.googleDriveAuthAvailable) {
          try {
            const authAvailable = await firstValueFrom(
              this.googleDriveService.initializeAuth(),
            );
            this.googleDriveAuthAvailable = authAvailable;

            if (!authAvailable) {
              this.feedbackService.showErrorToast({
                summary: this.translateService.instant('message.error'),
                detail:
                  'Google Drive authorization failed. Please check your configuration and try again.',
              });
              this.uploading.set(false);
              return;
            }
          } catch (error) {
            console.error('❌ Failed to initialize Google Drive auth:', error);
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('message.error'),
              detail:
                'Failed to initialize Google Drive authorization. Please refresh the page and try again.',
            });
            this.uploading.set(false);
            return;
          }
        }

        // Show conversion progress
        this.isConvertingFile = true;
        this.conversionMessage = `Converting "${this.selectedFile.name}" to PDF...`;

        try {
          const result = await this.processLocalFile(this.selectedFile);
          fileToUpload = result;
        } catch (error: any) {
          this.uploading.set(false);
          this.isConvertingFile = false;
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: `Failed to convert Office file: ${error.message || 'Unknown error'}`,
          });
          return;
        } finally {
          this.isConvertingFile = false;
        }
      }

      const formData = new FormData();
      formData.append('File', fileToUpload);
      formData.append('Name', fileToUpload.name);
      formData.append('ParentEntityName', 'Opportunity');
      formData.append('ParentEntityId', this.opportunityId().toString());
      formData.append('DocumentTypeId', this.selectedDocumentType.toString());
      formData.append('UploadToGCS', 'true'); // Upload to Google Cloud Storage (string 'true' will be parsed as boolean)

      this.documentService.uploadFile(formData).subscribe({
        next: (doc: any) => {
          this.uploading.set(false);
          this.showUploadDialog.set(false);
          this.selectedFile = null;
          this.selectedDocumentType = null;
          this.showUploadValidationError.set(false);

          this.feedbackService.showSuccessToast({
            summary: this.translateService.instant('message.success'),
            detail: this.translateService.instant(
              'message.document.uploadedSuccessfully',
            ),
          });

          this.loadDocuments();
        },
        error: (error: any) => {
          this.uploading.set(false);
          console.error('Upload error:', error);
        },
      });
    } catch (error) {
      this.uploading.set(false);
      console.error('Upload preparation error:', error);
    }
  }

  /**
   * @description Confirm Google Drive link - exports as PDF and uploads to GCS
   * @returns {Promise<void>}
   */
  async confirmLink(): Promise<void> {
    // Validate - only need document type since we already have the Google Drive link and ID
    if (!this.selectedDocumentType) {
      this.showLinkValidationError.set(true);
      return;
    }

    if (
      !this.selectedGoogleDriveFile ||
      !this.googleDriveId ||
      !this.googleDriveLink
    ) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant(
          'message.document.noGoogleDriveFileSelected',
        ),
      });
      return;
    }

    this.uploading.set(true);

    try {
      // Check if file needs PDF conversion
      const needsConversion = this.googleDriveService.needsPdfConversion(
        this.selectedGoogleDriveFile.mimeType || '',
      );

      if (needsConversion) {
        // If auth not available, try to initialize it now
        if (!this.googleDriveAuthAvailable) {
          try {
            const authAvailable = await firstValueFrom(
              this.googleDriveService.initializeAuth(),
            );
            this.googleDriveAuthAvailable = authAvailable;

            if (!authAvailable) {
              this.feedbackService.showErrorToast({
                summary: this.translateService.instant('message.error'),
                detail:
                  'Google Drive authorization failed. Please check your configuration and try again.',
              });
              this.uploading.set(false);
              return;
            }
          } catch (error) {
            console.error('❌ Failed to initialize Google Drive auth:', error);
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('message.error'),
              detail:
                'Failed to initialize Google Drive authorization. Please refresh the page and try again.',
            });
            this.uploading.set(false);
            return;
          }
        }

        // Show conversion progress
        this.isConvertingFile = true;
        this.conversionMessage = `Exporting "${this.selectedGoogleDriveFile.name}" from Drive as PDF...`;

        try {
          // Export Drive file as PDF
          const result = await firstValueFrom(
            this.googleDriveService.exportDriveFileAsPdf(
              this.googleDriveId,
              this.selectedGoogleDriveFile.name || '',
            ),
          );

          // Convert base64 to File object
          const blob = this.base64ToBlob(result.data, result.mimeType);
          const pdfFile = new File([blob], result.name, {
            type: result.mimeType,
          });

          // Upload PDF to GCS
          const formData = new FormData();
          formData.append('File', pdfFile);
          formData.append('Name', result.name);
          formData.append('ParentEntityName', 'Opportunity');
          formData.append('ParentEntityId', this.opportunityId().toString());
          formData.append(
            'DocumentTypeId',
            this.selectedDocumentType.toString(),
          );
          formData.append('UploadToGCS', 'true');
          formData.append('Link', this.googleDriveLink); // Keep original Drive link
          formData.append('GoogleId', this.googleDriveId); // Keep Google Drive ID

          this.isConvertingFile = false;

          // Upload to server
          this.documentService.uploadFile(formData).subscribe({
            next: (doc: any) => {
              this.uploading.set(false);
              this.showLinkDialog.set(false);
              this.googleDriveLink = '';
              this.googleDriveId = '';
              this.selectedGoogleDriveFile = null;
              this.selectedDocumentType = null;
              this.showLinkValidationError.set(false);

              this.feedbackService.showSuccessToast({
                summary: this.translateService.instant('message.success'),
                detail: this.translateService.instant(
                  'message.document.linkedAndConvertedSuccessfully',
                ),
              });

              this.loadDocuments();
            },
            error: (error: any) => {
              this.uploading.set(false);
              console.error('Upload error:', error);
            },
          });
        } catch (error: any) {
          this.uploading.set(false);
          this.isConvertingFile = false;
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: `Failed to export Drive file: ${error.message || 'Unknown error'}`,
          });
        }
      } else {
        // File is already PDF - download from Google Drive and upload to GCS
        // This ensures the file is stored in our GCS bucket for reliable access
        
        // If auth not available, try to initialize it now
        if (!this.googleDriveAuthAvailable) {
          try {
            const authAvailable = await firstValueFrom(
              this.googleDriveService.initializeAuth(),
            );
            this.googleDriveAuthAvailable = authAvailable;

            if (!authAvailable) {
              this.feedbackService.showErrorToast({
                summary: this.translateService.instant('message.error'),
                detail:
                  'Google Drive authorization failed. Please check your configuration and try again.',
              });
              this.uploading.set(false);
              return;
            }
          } catch (error) {
            console.error('❌ Failed to initialize Google Drive auth:', error);
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('message.error'),
              detail:
                'Failed to initialize Google Drive authorization. Please refresh the page and try again.',
            });
            this.uploading.set(false);
            return;
          }
        }

        // Show download progress
        this.isConvertingFile = true;
        this.conversionMessage = `Downloading "${this.selectedGoogleDriveFile.name}" from Drive...`;

        try {
          // Download the PDF file from Google Drive
          const result = await firstValueFrom(
            this.googleDriveService.downloadDriveFile(
              this.googleDriveId,
              this.selectedGoogleDriveFile.name || '',
              this.selectedGoogleDriveFile.mimeType || 'application/pdf',
            ),
          );

          // Convert base64 to File object
          const blob = this.base64ToBlob(result.data, result.mimeType);
          const pdfFile = new File([blob], result.name, {
            type: result.mimeType,
          });

          // Upload PDF to GCS
          const formData = new FormData();
          formData.append('File', pdfFile);
          formData.append('Name', result.name);
          formData.append('ParentEntityName', 'Opportunity');
          formData.append('ParentEntityId', this.opportunityId().toString());
          formData.append(
            'DocumentTypeId',
            this.selectedDocumentType.toString(),
          );
          formData.append('UploadToGCS', 'true');
          formData.append('Link', this.googleDriveLink); // Keep original Drive link
          formData.append('GoogleId', this.googleDriveId); // Keep Google Drive ID

          this.isConvertingFile = false;

          // Upload to server
          this.documentService.uploadFile(formData).subscribe({
            next: (doc: any) => {
              this.uploading.set(false);
              this.showLinkDialog.set(false);
              this.googleDriveLink = '';
              this.googleDriveId = '';
              this.selectedGoogleDriveFile = null;
              this.selectedDocumentType = null;
              this.showLinkValidationError.set(false);

              this.feedbackService.showSuccessToast({
                summary: this.translateService.instant('message.success'),
                detail: this.translateService.instant(
                  'message.document.uploadedSuccessfully',
                ),
              });

              this.loadDocuments();
            },
            error: (error: any) => {
              this.uploading.set(false);
              console.error('Upload error:', error);
            },
          });
        } catch (error: any) {
          this.uploading.set(false);
          this.isConvertingFile = false;
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: `Failed to download Drive file: ${error.message || 'Unknown error'}`,
          });
        }
      }
    } catch (error) {
      this.uploading.set(false);
      console.error('Link preparation error:', error);
    }
  }

  /**
   * @description Extract Google Drive ID from URL
   * @param {string} url - Google Drive URL
   * @returns {string | null} Extracted ID or null
   */
  private extractGoogleDriveId(url: string): string | null {
    // Match various Google Drive URL formats
    const patterns = [/\/file\/d\/([^\/]+)/, /id=([^&]+)/, /\/d\/([^\/]+)/];

    for (const pattern of patterns) {
      const match = url.match(pattern);
      if (match && match[1]) {
        return match[1];
      }
    }

    return null;
  }

  /**
   * @description Get file icon class based on file type
   * @param {string} fileType - File MIME type
   * @returns {string} PrimeNG icon class
   */
  getFileIcon(fileType: string): string {
    const iconMap: { [key: string]: string } = {
      'application/pdf': 'pi-file-pdf',
      'application/msword': 'pi-file-word',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document':
        'pi-file-word',
      'application/vnd.ms-excel': 'pi-file-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet':
        'pi-file-excel',
      'application/vnd.ms-powerpoint': 'pi-file',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation':
        'pi-file',
      default: 'pi-file',
    };
    return iconMap[fileType] || iconMap['default'];
  }

  /**
   * @description Get file icon color based on file type
   * @param {string} fileType - File MIME type
   * @returns {string} Tailwind color class
   */
  getFileIconColor(fileType: string): string {
    const colorMap: { [key: string]: string } = {
      'application/pdf': 'text-red-500',
      'application/msword': 'text-blue-500',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document':
        'text-blue-500',
      'application/vnd.ms-excel': 'text-green-500',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet':
        'text-green-500',
      default: 'text-gray-500',
    };
    return colorMap[fileType] || colorMap['default'];
  }

  /**
   * @description Download document
   * @param {any} doc - Document to download
   * @returns {void}
   */
  downloadDocument(doc: any): void {
    if (!doc.id) return;

    this.documentService.downloadDocument(doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = doc.name || 'document';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Download error:', error);
      },
    });
  }

  /**
   * @description View document (opens in new tab for GCS documents)
   * @param {any} doc - Document to view
   * @returns {void}
   */
  viewDocument(doc: any): void {
    if (!doc.id) return;

    this.documentService.getDocumentViewUrl(doc.id).subscribe({
      next: (response) => {
        if (response.type === 'gcs' || response.type === 'link') {
          // Open in new tab
          window.open(response.url, '_blank');
        } else if (response.type === 'blob') {
          // Download blob
          this.downloadDocument(doc);
        }
      },
      error: (error) => {
        console.error('View error:', error);
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: this.translateService.instant('message.document.viewFailed'),
        });
      },
    });
  }

  /**
   * @description Delete document
   * @param {any} doc - Document to delete
   * @returns {void}
   */
  deleteDocument(doc: any): void {
    if (!doc.id) return;

    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('title.deleteDocument'),
        detail: this.translateService.instant(
          'message.confirmation.deleteDocument',
        ),
      },
      () => {
        this.documentService.deleteDocument(doc.id!).subscribe({
          next: () => {
            this.feedbackService.showSuccessToast({
              summary: this.translateService.instant('message.success'),
              detail: this.translateService.instant(
                'message.document.deletedSuccessfully',
              ),
            });
            this.loadDocuments();
          },
          error: (error) => {
            console.error('Delete error:', error);
          },
        });
      },
    );
  }

  /**
   * @description Trigger AI transcription for a document
   * @param {any} doc - Document to transcribe
   * @returns {void}
   */
  aiTranscribe(doc: any): void {
    if (!doc.id) return;

    this.transcribingDocId.set(doc.id);

    this.documentService.transcribeDocument(doc.id).subscribe({
      next: (response: any) => {
        this.transcribingDocId.set(null);

        // Parse the AI response
        let extractedData;
        try {
          // Parse the response if it's a string
          const parsedResponse =
            typeof response === 'string' ? JSON.parse(response) : response;

          // Extract JSON from Gemini response structure
          // Response structure: candidates[0].content.parts[0].text
          let jsonText = parsedResponse;

          if (
            parsedResponse.candidates &&
            parsedResponse.candidates.length > 0
          ) {
            const candidate = parsedResponse.candidates[0];
            if (
              candidate.content &&
              candidate.content.parts &&
              candidate.content.parts.length > 0
            ) {
              jsonText = candidate.content.parts[0].text;
            }
          }

          // Remove markdown code block markers if present
          if (typeof jsonText === 'string') {
            jsonText = jsonText
              .replace(/```json\n?/g, '')
              .replace(/```\n?$/g, '')
              .trim();
          }

          // Parse the actual opportunity data
          extractedData =
            typeof jsonText === 'string' ? JSON.parse(jsonText) : jsonText;
        } catch (e) {
          console.error('❌ Failed to parse AI response:', e);
          console.error('❌ Response that failed:', response);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: this.translateService.instant(
              'message.document.aiTranscribeFailed',
            ),
          });
          return;
        }

        // Show success message
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant(
            'message.document.aiTranscribeSuccess',
          ),
        });

        // Store AI extracted data
        this.aiExtractedData.set(extractedData);

        // Open comparison dialog - reusable component will fetch audit log internally
        this.showComparisonDialog.set(true);
      },
      error: (error) => {
        this.transcribingDocId.set(null);
        console.error('AI Transcribe error:', error);
      },
    });
  }

  /**
   * @description Handle applying selected changes from AI comparison
   * @param {any} changes - Selected changes to apply
   * @returns {void}
   */
  handleApplyChanges(changes: any): void {
    // Transform the changes to match the backend API format
    const transformedChanges = this.transformAiChangesToApiFormat(changes);

    // Call the API to apply the changes
    this.opportunityService
      .applyAiChanges(this.opportunityId(), transformedChanges)
      .subscribe({
        next: (updatedOpportunity: any) => {
          // **CRITICAL**: Clear AI extracted data immediately to prevent stale comparisons
          this.aiExtractedData.set(null);
          
          // Close the comparison dialog FIRST
          this.showComparisonDialog.set(false);

          // Show success feedback
          this.feedbackService.showSuccessToast({
            summary: this.translateService.instant('message.success'),
            detail: this.translateService.instant(
              'message.opportunity.updatedSuccessfully',
            ),
          });

          // Emit event to parent to reload entire opportunity view
          // The parent will reload the opportunity, which will update the input signal
          // This ensures fresh data before the next comparison
          this.opportunityUpdated.emit();

          // Wait for opportunity to reload, then reload documents
          // Using setTimeout to ensure the opportunity data is refreshed
          setTimeout(() => {
            this.loadDocuments();
          }, 500);
        },
        error: (error: any) => {
          console.error('❌ Error applying AI changes:', error);
          // Error is handled by global HTTP interceptor
          // But we should close the dialog anyway
          this.showComparisonDialog.set(false);
        },
      });
  }

  /**
   * @description Transform AI-extracted data to match backend API format
   * Converts complex objects to simple ID arrays and strings
   * @param {any} aiChanges - AI-extracted changes
   * @returns {any} Transformed changes for API
   */
  private transformAiChangesToApiFormat(aiChanges: any): any {
    const transformed: any = {};

    for (const key in aiChanges) {
      const value = aiChanges[key];

      // Handle funding partners - extract partner IDs
      if (key === 'fundingPartners' && Array.isArray(value)) {
        // Send full funding partner objects with amounts and currency
        transformed.fundingPartners = value
          .filter((partner: any) => partner.partnerId != null)
          .map((partner: any) => ({
            partnerId: partner.partnerId,
            amount: partner.amount || null,
            fundedAmount: partner.amount || null, // Alias
            currencyId: partner.currencyId || null,
            percentage: partner.percentage || null,
            feePercentage: partner.feePercentage || null,
            feeAmount: partner.feeAmount || null,
            feeAmountUSD: partner.feeAmountUSD || null,
            isAmountBasedFee: partner.isAmountBasedFee || false,
            partnershipAgreementReference: partner.partnershipAgreementReference || null,
            documentId: partner.documentId || null,
            isPooledContribution: partner.isPooledContribution || false,
            selectedPartnerAgreementNumber: partner.selectedPartnerAgreementNumber || null
          }));
      }
      // Handle client partners - extract partner IDs
      else if (key === 'clientPartners' && Array.isArray(value)) {
        transformed.clientPartners = value
          .map((partner: any) => partner.partnerId)
          .filter((id: number) => id != null);
      }
      // Handle countries - extract country IDs from nested structure
      else if (key === 'countries' && Array.isArray(value)) {
        transformed.countries = value
          .map(
            (countryItem: any) =>
              countryItem.country?.id || countryItem.countryId,
          )
          .filter((id: number) => id != null);
      }
      // Handle SDGs - extract SDG IDs
      else if (key === 'sdGs' && Array.isArray(value)) {
        transformed.sdGs = value
          .map((sdg: any) => sdg.sdgId)
          .filter((id: number) => id != null);
      }
      // Handle UNOPS Missions - extract unopsMissionId to API format
      else if (key === 'unopsMissions' && Array.isArray(value)) {
        transformed.unopsMissions = value
          .filter((m: any) => m.unopsMissionId != null)
          .map((m: any) => ({ unopsMissionId: m.unopsMissionId }));
      }
      // Handle stakeholders - convert to stakeholder request format with userId and entityRoleId
      else if (key === 'stakeholders' && Array.isArray(value)) {
        transformed.stakeholders = value
          .filter(
            (stakeholder: any) =>
              stakeholder.userId != null && stakeholder.entityRoleId != null,
          )
          .map((stakeholder: any) => ({
            userId: stakeholder.userId,
            entityRoleId: stakeholder.entityRoleId,
            notes: stakeholder.notes || null,
          }));
      }
      // Handle deliverables - convert to deliverable request format
      else if (key === 'deliverables' && Array.isArray(value)) {
        transformed.deliverables = value
          .filter((deliverable: any) => deliverable.outputId != null)
          .map((deliverable: any) => ({
            outputId: deliverable.outputId,
            quantity: deliverable.quantity || null,
            notes: deliverable.notes || null,
          }));
      }
      // Handle all other properties - pass through as-is
      else {
        transformed[key] = value;
      }
    }

    return transformed;
  }

  /**
   * @description Process local file - convert Office files to PDF using Google Drive
   * @param {File} file - File to process
   * @returns {Promise<File>} Processed file (converted to PDF if it was Office file)
   * @private
   */
  private async processLocalFile(file: File): Promise<File> {
    // Check if it's a Microsoft Office file
    if (
      this.googleDriveAuthAvailable &&
      this.googleDriveService.isMicrosoftOfficeFile(file.type)
    ) {
      try {
        const result = await firstValueFrom(
          this.googleDriveService.convertLocalOfficeFileToPdf(file),
        );

        // Convert the base64 data back to a File object
        const blob = this.base64ToBlob(result.data, result.mimeType);
        const convertedFile = new File([blob], result.name, {
          type: result.mimeType,
        });
        return convertedFile;
      } catch (error) {
        console.error(`Failed to convert Office file ${file.name}:`, error);
        throw error;
      }
    }

    // Not an Office file or no auth - return as-is
    return file;
  }

  /**
   * @description Convert base64 string to Blob
   * @param {string} base64 - Base64 string
   * @param {string} mimeType - MIME type
   * @returns {Blob} Blob object
   * @private
   */
  private base64ToBlob(base64: string, mimeType: string): Blob {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  }

  /**
   * @description Open dialog to tag document with partners (Partner Results Framework)
   * @param {any} doc - Document to tag
   * @returns {void}
   */
  openPartnerTagDialog(doc: any): void {
    this.documentBeingTagged = doc;
    this.selectedFundingPartners = [];
    this.selectedClientPartners = [];
    this.showPartnerTagValidationError.set(false);
    this.hasPartnerSelectionChanged.set(false);
    
    // Call API to retrieve existing partner-document associations
    if (doc.id) {
      this.documentService.getPartnerDocumentAssociation(doc.id).subscribe({
        next: (response: any) => {
          if (response && response.partners) {
            // Pre-select partners based on existing associations
            response.partners.forEach((partner: any) => {
              if (
                partner.partnerType === 'funding' &&
                !this.selectedFundingPartners.includes(partner.partnerId)
              ) {
                this.selectedFundingPartners.push(partner.partnerId);
              } else if (
                partner.partnerType === 'client' &&
                !this.selectedClientPartners.includes(partner.partnerId)
              ) {
                this.selectedClientPartners.push(partner.partnerId);
              }
            });
          }
          
          // Store original selections for comparison
          this.originalFundingPartners = [...this.selectedFundingPartners];
          this.originalClientPartners = [...this.selectedClientPartners];
          
          // Open dialog after loading associations
          this.showPartnerTagDialog.set(true);
        },
        error: (error) => {
          console.error('Error retrieving partner-document associations:', error);
          
          // Store empty original selections
          this.originalFundingPartners = [];
          this.originalClientPartners = [];
          
          // Open dialog anyway even if API call fails
          this.showPartnerTagDialog.set(true);
        },
      });
    } else {
      // If no document ID, just open the dialog with empty original selections
      this.originalFundingPartners = [];
      this.originalClientPartners = [];
      this.showPartnerTagDialog.set(true);
    }
  }

  /**
   * @description Cancel partner tagging
   * @returns {void}
   */
  cancelPartnerTagging(): void {
    this.showPartnerTagDialog.set(false);
    this.documentBeingTagged = null;
    this.selectedFundingPartners = [];
    this.selectedClientPartners = [];
    this.originalFundingPartners = [];
    this.originalClientPartners = [];
    this.showPartnerTagValidationError.set(false);
    this.hasPartnerSelectionChanged.set(false);
  }

  /**
   * @description Confirm partner tagging for document
   * @returns {void}
   */
  confirmPartnerTagging(): void {
    // Check if there has been any change from original selection
    const hasChange = this.checkIfPartnerSelectionChanged();
    
    // If no change, show validation error
    if (!hasChange) {
      this.showPartnerTagValidationError.set(true);
      return;
    }

    const opp = this.opportunity();
    if (!opp || !this.documentBeingTagged) return;

    // Call the new API endpoint to tag the document with partners
    this.opportunityService.tagDocumentToPartners(
      opp.id!,
      this.documentBeingTagged.id,
      this.selectedFundingPartners,
      this.selectedClientPartners
    ).subscribe({
      next: () => {
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.document.partnerTaggedSuccessfully')
        });
        
        this.showPartnerTagDialog.set(false);
        this.documentBeingTagged = null;
        this.selectedFundingPartners = [];
        this.selectedClientPartners = [];
        this.originalFundingPartners = [];
        this.originalClientPartners = [];
        this.showPartnerTagValidationError.set(false);
        this.hasPartnerSelectionChanged.set(false);
        
        // Emit event to reload opportunity
        this.opportunityUpdated.emit();
      },
      error: (error) => {
        console.error('Error tagging document with partners:', error);
      }
    });
  }
  
  /**
   * @description Check if partner selection has changed from original
   * @returns {boolean} True if there's been any change
   * @private
   */
  private checkIfPartnerSelectionChanged(): boolean {
    // Compare current selection with original
    const fundingChanged = 
      this.selectedFundingPartners.length !== this.originalFundingPartners.length ||
      !this.selectedFundingPartners.every(id => this.originalFundingPartners.includes(id)) ||
      !this.originalFundingPartners.every(id => this.selectedFundingPartners.includes(id));
    
    const clientChanged = 
      this.selectedClientPartners.length !== this.originalClientPartners.length ||
      !this.selectedClientPartners.every(id => this.originalClientPartners.includes(id)) ||
      !this.originalClientPartners.every(id => this.selectedClientPartners.includes(id));
    
    return fundingChanged || clientChanged;
  }
  
  /**
   * @description Toggle funding partner selection
   * @param {number} partnerId - Partner ID to toggle
   * @returns {void}
   */
  toggleFundingPartner(partnerId: number): void {
    const index = this.selectedFundingPartners.indexOf(partnerId);
    if (index > -1) {
      this.selectedFundingPartners.splice(index, 1);
    } else {
      this.selectedFundingPartners.push(partnerId);
    }
    this.showPartnerTagValidationError.set(false);
    
    // Update change detection signal
    this.hasPartnerSelectionChanged.set(this.checkIfPartnerSelectionChanged());
  }

  /**
   * @description Toggle client partner selection
   * @param {number} partnerId - Partner ID to toggle
   * @returns {void}
   */
  toggleClientPartner(partnerId: number): void {
    const index = this.selectedClientPartners.indexOf(partnerId);
    if (index > -1) {
      this.selectedClientPartners.splice(index, 1);
    } else {
      this.selectedClientPartners.push(partnerId);
    }
    this.showPartnerTagValidationError.set(false);
    
    // Update change detection signal
    this.hasPartnerSelectionChanged.set(this.checkIfPartnerSelectionChanged());
  }

  /**
   * @description Check if funding partner is selected
   * @param {number} partnerId - Partner ID
   * @returns {boolean} True if selected
   */
  isFundingPartnerSelected(partnerId: number): boolean {
    return this.selectedFundingPartners.includes(partnerId);
  }

  /**
   * @description Check if client partner is selected
   * @param {number} partnerId - Partner ID
   * @returns {boolean} True if selected
   */
  isClientPartnerSelected(partnerId: number): boolean {
    return this.selectedClientPartners.includes(partnerId);
  }

  /**
   * @description Get partners associated with a document
   * @param {any} doc - Document
   * @returns {any[]} Array of partners
   */
  getDocumentPartners(doc: any): any[] {
    const opp = this.opportunity();
    if (!opp || !doc) return [];

    const partners: any[] = [];

    // Find funding partners linked to this document
    const fundingPartners = (opp.fundingPartners || []).filter(
      (fp: any) => fp.documentId === doc.id,
    );
    fundingPartners.forEach((fp: any) => {
      partners.push({
        ...fp,
        type: 'funding',
        typeName: this.translateService.instant('label.fundingPartner'),
      });
    });

    // Find client partners linked to this document
    const clientPartners = (opp.clientPartners || []).filter(
      (cp: any) => cp.documentId === doc.id,
    );
    clientPartners.forEach((cp: any) => {
      partners.push({
        ...cp,
        type: 'client',
        typeName: this.translateService.instant('label.clientPartner'),
      });
    });

    return partners;
  }

  /**
   * @description Generate Opportunity Statement PDF from markdown content
   * Converts markdown to Google Doc, exports as PDF, uploads to GCS, and creates document record.
   * @param {string} markdown - The opportunity statement markdown content
   * @param {number} opportunityId - The opportunity ID
   * @param {string} pdfFileName - The name for the generated PDF file (e.g., "Opportunity_123_Submission_20260204.pdf")
   * @returns {Promise<boolean>} True if successful, false otherwise
   */
  async generateStatementPdf(
    markdown: string,
    opportunityId: number,
    pdfFileName: string
  ): Promise<boolean> {
    if (!markdown || !opportunityId || !pdfFileName) {
      console.error('❌ generateStatementPdf: Missing required parameters');
      return false;
    }

    console.log('📄 Starting Opportunity Statement PDF generation...');
    console.log(`   Opportunity ID: ${opportunityId}`);
    console.log(`   PDF Filename: ${pdfFileName}`);

    let googleDocId: string | null = null;

    try {
      // Step 1: Get Google OAuth token for AI API
      let idToken: string;
      try {
        console.log('🔑 Requesting Google OAuth token...');
        idToken = await this.googleOAuthService.getValidIdToken();
        console.log('✅ Google OAuth token obtained');
      } catch (authError: any) {
        console.error('❌ Google authentication failed:', authError);
        const errorMessage = authError?.message || authError?.error || 'Unknown error';
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: `Google authentication failed: ${errorMessage}. If a popup was blocked, please allow popups and try again.`,
        });
        return false;
      }

      // Step 2: Convert markdown to Google Doc using AI API
      console.log('🔄 Converting markdown to Google Doc...');
      const formData = new FormData();
      const markdownBlob = new Blob([markdown], { type: 'text/markdown' });
      const tempDocName = `Opportunity_${opportunityId}_Statement_Temp`;
      formData.append('file', markdownBlob, tempDocName);
      formData.append('data', JSON.stringify({ name: tempDocName }));

      const response = await fetch(
        'https://api.ai.unops.org/v1/convert/markdown-to-google-doc',
        {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${idToken}`,
          },
          body: formData,
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error('❌ AI API Error:', errorText);
        
        // Handle auth errors with retry
        if (response.status === 401 || response.status === 403) {
          console.log('🔄 Token expired, refreshing and retrying...');
          try {
            await this.googleOAuthService.refreshToken();
            // Retry once
            return this.generateStatementPdf(markdown, opportunityId, pdfFileName);
          } catch (refreshError) {
            console.error('❌ Token refresh failed:', refreshError);
          }
        }
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: 'Failed to convert statement to document. Please try again.',
        });
        return false;
      }

      const result = await response.json();
      console.log('✅ Google Doc created:', result);

      // Extract Google Doc ID from the response URL
      // URL format: https://docs.google.com/document/d/{fileId}/edit
      // API returns URL nested in result.data.data.url or result.data.url
      const docUrl = result.data?.data?.url || result.data?.url || result.url || '';
      const docIdMatch = docUrl.match(/\/document\/d\/([^\/]+)/);
      if (!docIdMatch) {
        console.error('❌ Could not extract Google Doc ID from URL:', docUrl);
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: 'Failed to process created document. Please try again.',
        });
        return false;
      }
      googleDocId = docIdMatch[1];
      console.log(`📄 Google Doc ID: ${googleDocId}`);

      // Step 3: Initialize Google Drive auth if needed
      if (!this.googleDriveAuthAvailable) {
        try {
          const authAvailable = await firstValueFrom(
            this.googleDriveService.initializeAuth()
          );
          this.googleDriveAuthAvailable = authAvailable;
          if (!authAvailable) {
            console.error('❌ Google Drive auth not available');
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('message.error'),
              detail: 'Google Drive authorization required. Please try again.',
            });
            return false;
          }
        } catch (error) {
          console.error('❌ Failed to initialize Google Drive auth:', error);
          return false;
        }
      }

      // Step 4: Export Google Doc as PDF
      console.log('🔄 Exporting Google Doc as PDF...');
      const pdfResult = await firstValueFrom(
        this.googleDriveService.exportDriveFileAsPdf(googleDocId!, tempDocName)
      );
      console.log('✅ PDF exported successfully');

      // Step 5: Convert base64 PDF to File object
      const pdfBlob = this.base64ToBlob(pdfResult.data, pdfResult.mimeType);
      const pdfFile = new File([pdfBlob], pdfFileName, {
        type: 'application/pdf',
      });

      // Step 6: Find "Opportunity Statement" document type ID
      const documentTypes = this.documentTypes();
      const statementType = documentTypes.find(
        (dt: any) => dt.name === 'Opportunity Statement'
      );
      if (!statementType) {
        console.error('❌ Opportunity Statement document type not found');
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: 'Document type configuration error. Please contact support.',
        });
        return false;
      }

      // Step 7: Upload PDF to GCS and create document record
      console.log('🔄 Uploading PDF to storage...');
      const uploadFormData = new FormData();
      uploadFormData.append('File', pdfFile);
      uploadFormData.append('Name', pdfFileName);
      uploadFormData.append('ParentEntityName', 'Opportunity');
      uploadFormData.append('ParentEntityId', opportunityId.toString());
      uploadFormData.append('DocumentTypeId', statementType.id.toString());
      uploadFormData.append('UploadToGCS', 'true');

      await firstValueFrom(this.documentService.uploadFile(uploadFormData));
      console.log('✅ Opportunity Statement uploaded successfully');

      // Step 8: Delete the temporary Google Doc (best effort - don't fail if this fails)
      try {
        console.log('🧹 Cleaning up temporary Google Doc...');
        await this.deleteGoogleDriveFile(googleDocId!, idToken);
        console.log('✅ Temporary Google Doc deleted');
      } catch (cleanupError) {
        console.warn('⚠️ Failed to delete temporary Google Doc:', cleanupError);
        // Don't fail the overall operation for cleanup errors
      }

      this.feedbackService.showSuccessToast({
        summary: this.translateService.instant('message.success'),
        detail: 'Opportunity Statement PDF generated successfully.',
      });

      // Reload documents list to show the new PDF after a short delay
      // to ensure the server has processed the upload
      setTimeout(() => {
        this.loadDocuments();
      }, 500);

      return true;
    } catch (error: any) {
      console.error('❌ Error generating statement PDF:', error);
      
      // Try to clean up the Google Doc if it was created
      if (googleDocId) {
        try {
          const idToken = await this.googleOAuthService.getValidIdToken();
          await this.deleteGoogleDriveFile(googleDocId, idToken);
        } catch (cleanupError) {
          console.warn('⚠️ Failed to clean up Google Doc:', cleanupError);
        }
      }

      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: `Failed to generate PDF: ${error.message || 'Unknown error'}`,
      });
      return false;
    }
  }

  /**
   * @description Delete a file from Google Drive
   * @param {string} fileId - Google Drive file ID
   * @param {string} accessToken - Google OAuth access token
   * @returns {Promise<void>}
   */
  private async deleteGoogleDriveFile(fileId: string, accessToken: string): Promise<void> {
    const response = await fetch(
      `https://www.googleapis.com/drive/v3/files/${fileId}`,
      {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      }
    );

    if (!response.ok && response.status !== 404) {
      throw new Error(`Failed to delete file: ${response.statusText}`);
    }
  }
}
