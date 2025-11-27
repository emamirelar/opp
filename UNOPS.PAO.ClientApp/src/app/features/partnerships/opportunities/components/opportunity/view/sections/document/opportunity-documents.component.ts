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
      fieldPath: 'partnerReference',
      displayName: 'Partner Reference',
    },
    {
      fieldPath: 'responsibleOrgUnitName',
      displayName: 'Responsible Organization Unit',
    },
    {
      fieldPath: 'proposedInitiativeTypeName',
      displayName: 'Initiative Type',
    },
    {
      fieldPath: 'initiativeBudgetUSD',
      displayName: 'Budget (USD)',
      formatFn: (value) =>
        value != null
          ? `$${value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
          : '-',
    },
    {
      fieldPath: 'partnershipAgreementReference',
      displayName: 'Partnership Agreement Reference',
    },
    {
      fieldPath: 'targetSigningDate',
      displayName: 'Target Signing Date',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'targetDeliveryDate',
      displayName: 'Target Delivery Date',
      formatFn: (value) => (value ? new Date(value).toLocaleDateString() : '-'),
    },
    {
      fieldPath: 'strategicAlignment',
      displayName: 'Strategic Alignment',
    },
    {
      fieldPath: 'resultsFocus',
      displayName: 'Results Focus',
    },
    {
      fieldPath: 'intendedImpactOutcomes',
      displayName: 'Intended Impact & Outcomes',
    },
    {
      fieldPath: 'expectedBeneficiaries',
      displayName: 'Expected Beneficiaries',
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
      displayName: 'Deliverables',
      formatFn: (value) =>
        Array.isArray(value)
          ? `${value.length} deliverable(s)`
          : '0 deliverables',
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
        // File is already PDF - just link it (old behavior)
        const linkModel = {
          link: this.googleDriveLink,
          googleId: this.googleDriveId,
          name: this.selectedGoogleDriveFile.name || '',
          type: this.selectedGoogleDriveFile.mimeType || '',
          parentEntityName: 'Opportunity',
          parentEntityId: this.opportunityId(),
          documentTypeId: this.selectedDocumentType,
        };

        this.documentService.linkFile(linkModel).subscribe({
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
                'message.document.linkedSuccessfully',
              ),
            });

            this.loadDocuments();
          },
          error: (error: any) => {
            this.uploading.set(false);
            console.error('Link error:', error);
          },
        });
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
          // Show success feedback
          this.feedbackService.showSuccessToast({
            summary: this.translateService.instant('message.success'),
            detail: this.translateService.instant(
              'message.opportunity.updatedSuccessfully',
            ),
          });

          // Close the comparison dialog
          this.showComparisonDialog.set(false);
          this.aiExtractedData.set(null);

          // Emit event to parent to reload entire opportunity view
          this.opportunityUpdated.emit();

          // Reload documents to reflect any document-related changes
          this.loadDocuments();
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
        transformed.fundingPartners = value
          .map((partner: any) => partner.partnerId)
          .filter((id: number) => id != null);
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
      // Handle stakeholders - extract entity role IDs
      else if (key === 'stakeholders' && Array.isArray(value)) {
        transformed.stakeholders = value
          .map((stakeholder: any) => stakeholder.entityRoleId)
          .filter((id: number) => id != null);
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
}
