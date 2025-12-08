/**
 * @fileoverview Component for managing Entity Artifacts - dynamic data associated with entities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, OnInit, signal, computed, inject, ChangeDetectorRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Router } from '@angular/router';

// PrimeNG imports
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { FileUploadModule } from 'primeng/fileupload';

// File upload components
import { Base64FileUploadComponent, Base64FileData } from '@shared/components/file-upload/base64-file-upload.component';

import { MessageService } from 'primeng/api';
import {
  EntityArtifactService,
  EntityTypeOption,
  ArtifactTypeResponse,
  EntityRecordOption,
  EntityArtifactRequest,
  EntityArtifactResponse
} from '../services/entity-artifact.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionService, EntityPermissions } from '@core/services/auth';

/**
 * @class EntityArtifactManagerComponent
 * @description Administrative interface for managing Entity Artifacts. Allows users to create and update
 * artifact data for various entities like Countries, Partners, Organizations, etc.
 * Supports different data types: string, number, date, document, and JSON.
 * 
 * @example
 * ```html
 * <!-- Usage in routing -->
 * <app-entity-artifact-manager></app-entity-artifact-manager>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-entity-artifact-manager',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    SelectModule,
    ButtonModule,
    InputTextModule,
    InputTextarea,
    InputNumberModule,
    DatePickerModule,
    ProgressSpinnerModule,
    ToastModule,
    CardModule,
    MessageModule,
    FloatLabelModule,
    FileUploadModule,
    Base64FileUploadComponent
  ],
  providers: [MessageService],
  templateUrl: './entity-artifact-manager.component.html',
  styleUrls: ['./entity-artifact-manager.component.scss']
})
export class EntityArtifactManagerComponent implements OnInit {
  private readonly entityArtifactService = inject(EntityArtifactService);
  private readonly messageService = inject(MessageService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly translateService = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly permissionService = inject(PermissionService);

  // State signals
  readonly entityTypes = signal<EntityTypeOption[]>([]);
  readonly selectedEntityType = signal<string | null>(null);
  readonly artifactTypes = signal<ArtifactTypeResponse[]>([]);
  readonly selectedArtifactType = signal<ArtifactTypeResponse | null>(null);
  readonly entityRecords = signal<EntityRecordOption[]>([]);
  readonly selectedEntityRecord = signal<EntityRecordOption | null>(null);
  readonly currentArtifact = signal<EntityArtifactResponse | null>(null);

  // Form state
  readonly valueText = signal<string>('');
  readonly valueNumber = signal<number | null>(null);
  readonly valueBoolean = signal<boolean | null>(null);
  readonly valueDate = signal<Date | null>(null);
  readonly documentId = signal<number | null>(null);
  readonly uploadedFile = signal<Base64FileData | null>(null);
  readonly existingDocumentData = signal<Base64FileData | null>(null);
  readonly showDocumentUploadPanel = signal<boolean>(false);

  // Loading states
  readonly loadingEntityTypes = signal<boolean>(false);
  readonly loadingArtifactTypes = signal<boolean>(false);
  readonly loadingEntityRecords = signal<boolean>(false);
  readonly loadingArtifact = signal<boolean>(false);
  readonly saving = signal<boolean>(false);
  readonly permissionsLoading = signal<boolean>(true);

  // Validation state
  readonly showValidationError = signal<boolean>(false);

  // Document upload configuration
  readonly acceptedMIMETypes = 'application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,image/*';

  // Permissions
  readonly entityPermissions = signal<EntityPermissions>({
    entity: 'EntityArtifactManager',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false,
      canExport: false,
      canImport: false
    }
  });

  // Computed values
  readonly hasAccessToManage = computed(() => this.entityPermissions().permissions.canUpdate);
  readonly canViewOnly = computed(() => this.entityPermissions().permissions.canRead && !this.entityPermissions().permissions.canUpdate);

  /**
   * @description Computed property that returns the data type of the selected artifact
   * @type {Signal<string | null>}
   * @since 1.0.0
   */
  readonly selectedDataType = computed(() => {
    const artifactType = this.selectedArtifactType();
    return artifactType?.artifactDataTypeName?.toLowerCase() || null;
  });

  /**
   * @description Computed property that determines if the form is valid for submission
   * @type {Signal<boolean>}
   * @since 1.0.0
   */
  readonly isFormValid = computed(() => {
    const hasEntityType = !!this.selectedEntityType();
    const hasArtifactType = !!this.selectedArtifactType();
    const hasEntityRecord = !!this.selectedEntityRecord();
    const hasValue = this.hasArtifactValue();
    
    return hasEntityType && hasArtifactType && hasEntityRecord && hasValue;
  });

  ngOnInit() {
    this.loadPermissions();
  }

  /**
   * @description Load user permissions for this component
   * @returns {void}
   * @since 1.0.0
   */
  private loadPermissions() {
    this.permissionsLoading.set(true);
    
    const currentPath = this.router.url;
    
    this.permissionService.getEntityPermissions(currentPath)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (permissions) => {
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          
          if (!permissions.hasAccess) {
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('entityArtifact.errors.accessDenied'),
              detail: this.translateService.instant('entityArtifact.errors.noPermissionToAccess')
            });
            this.router.navigate(['/access-denied']);
            return;
          }
          
          this.loadEntityTypes();
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading permissions:', error);
          this.permissionsLoading.set(false);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('entityArtifact.errors.error'),
            detail: this.translateService.instant('entityArtifact.errors.failedToLoadPermissions')
          });
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * @description Load available entity types from the backend
   * @returns {void}
   * @since 1.0.0
   */
  private loadEntityTypes() {
    this.loadingEntityTypes.set(true);
    
    this.entityArtifactService.getEntityTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (types) => {
          this.entityTypes.set(types);
          this.loadingEntityTypes.set(false);
        },
        error: (error) => {
          console.error('Error loading entity types:', error);
          this.loadingEntityTypes.set(false);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('entityArtifact.errors.error'),
            detail: this.translateService.instant('entityArtifact.errors.failedToLoadEntityTypes')
          });
        }
      });
  }

  /**
   * @description Handle entity type selection change
   * @returns {void}
   * @since 1.0.0
   */
  onEntityTypeChange() {
    // Reset dependent fields
    this.selectedArtifactType.set(null);
    this.selectedEntityRecord.set(null);
    this.artifactTypes.set([]);
    this.entityRecords.set([]);
    this.currentArtifact.set(null);
    this.resetFormValues();
    this.showValidationError.set(false);

    const entityType = this.selectedEntityType();
    if (!entityType) return;

    // Load artifact types for selected entity
    this.loadArtifactTypes(entityType);
  }

  /**
   * @description Load artifact types filtered by entity type
   * @param {string} entityType - The selected entity type
   * @returns {void}
   * @since 1.0.0
   */
  private loadArtifactTypes(entityType: string) {
    this.loadingArtifactTypes.set(true);

    this.entityArtifactService.getArtifactTypesByEntityType(entityType)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (types) => {
          this.artifactTypes.set(types);
          this.loadingArtifactTypes.set(false);
        },
        error: (error) => {
          console.error('Error loading artifact types:', error);
          this.loadingArtifactTypes.set(false);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('entityArtifact.errors.error'),
            detail: this.translateService.instant('entityArtifact.errors.failedToLoadArtifactTypes')
          });
        }
      });
  }

  /**
   * @description Handle artifact type selection change
   * @returns {void}
   * @since 1.0.0
   */
  onArtifactTypeChange() {
    // Reset dependent fields
    this.selectedEntityRecord.set(null);
    this.entityRecords.set([]);
    this.currentArtifact.set(null);
    this.resetFormValues();
    this.showValidationError.set(false);

    const entityType = this.selectedEntityType();
    if (!entityType) return;

    // Load entity records
    this.loadEntityRecords(entityType);
  }

  /**
   * @description Load entity records for the selected entity type
   * @param {string} entityType - The selected entity type
   * @returns {void}
   * @since 1.0.0
   */
  private loadEntityRecords(entityType: string) {
    this.loadingEntityRecords.set(true);

    this.entityArtifactService.getEntityRecords(entityType)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (records) => {
          this.entityRecords.set(records);
          this.loadingEntityRecords.set(false);
        },
        error: (error) => {
          console.error('Error loading entity records:', error);
          this.loadingEntityRecords.set(false);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('entityArtifact.errors.error'),
            detail: this.translateService.instant('entityArtifact.errors.failedToLoadEntityRecords')
          });
        }
      });
  }

  /**
   * @description Handle entity record selection change
   * @returns {void}
   * @since 1.0.0
   */
  onEntityRecordChange() {
    this.currentArtifact.set(null);
    this.resetFormValues();
    this.showValidationError.set(false);

    const entityType = this.selectedEntityType();
    const entityRecord = this.selectedEntityRecord();
    const artifactType = this.selectedArtifactType();

    if (!entityType || !entityRecord || !artifactType) return;

    // Load existing artifact value if available
    this.loadExistingArtifact(entityType, entityRecord.id, artifactType.id);
  }

  /**
   * @description Load existing artifact value for the selected combination
   * @param {string} entityType - The entity type
   * @param {number} entityId - The entity ID
   * @param {number} artifactTypeId - The artifact type ID
   * @returns {void}
   * @since 1.0.0
   */
  private loadExistingArtifact(entityType: string, entityId: number, artifactTypeId: number) {
    this.loadingArtifact.set(true);

    this.entityArtifactService.getEntityArtifact(entityType, entityId, artifactTypeId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (artifact) => {
          if (artifact) {
            // Existing artifact found - populate form
            this.currentArtifact.set(artifact);
            this.populateFormFromArtifact(artifact);
          } else {
            // No artifact exists yet - show empty form for new creation
            this.currentArtifact.set(null);
            this.resetFormValues();
          }
          this.loadingArtifact.set(false);
        },
        error: (error) => {
          console.error('Error loading artifact:', error);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('entityArtifact.errors.error'),
            detail: this.translateService.instant('entityArtifact.errors.failedToLoadArtifact')
          });
          this.loadingArtifact.set(false);
        }
      });
  }

  /**
   * @description Populate form fields from an existing artifact
   * @param {EntityArtifactResponse} artifact - The artifact to populate from
   * @returns {void}
   * @since 1.0.0
   */
  private populateFormFromArtifact(artifact: EntityArtifactResponse) {
    // For non-document types, populate form fields normally
    if (this.selectedDataType() !== 'document') {
      this.valueText.set(artifact.valueText || '');
    }
    this.valueNumber.set(artifact.valueNumber);
    this.valueBoolean.set(artifact.valueBoolean);
    this.valueDate.set(artifact.valueDate ? new Date(artifact.valueDate) : null);
    this.documentId.set(artifact.documentId);
    
    // For document type artifacts, check for GCS URL in ValueText (new format)
    // or base64 in ValueJson (legacy format)
    if (this.selectedDataType() === 'document') {
      // Check for new GCS format: URL in ValueText, metadata in ValueJson
      if (artifact.valueText && (artifact.valueText.startsWith('gs://') || artifact.valueText.startsWith('https://storage.'))) {
        // New GCS format - parse metadata from ValueJson
        let documentMetadata: { fileName?: string; mimeType?: string; fileSize?: number } = {};
        if (artifact.valueJson) {
          try {
            documentMetadata = JSON.parse(artifact.valueJson);
          } catch (error) {
            console.error('Error parsing document metadata from ValueJson:', error);
          }
        }
        
        // Create a placeholder document data object for display
        const gcsDocumentData: Base64FileData = {
          fileName: documentMetadata.fileName || artifact.name || 'Document',
          fileType: documentMetadata.mimeType || 'application/octet-stream',
          fileSize: documentMetadata.fileSize || 0,
          base64Content: '' // Empty because document is stored in GCS
        };
        
        this.existingDocumentData.set(gcsDocumentData);
        this.showDocumentUploadPanel.set(false); // Hide upload panel initially
        console.log('Existing GCS document loaded:', gcsDocumentData.fileName);
      }
      // Check for legacy base64 format in ValueJson
      else if (artifact.valueJson) {
        try {
          const documentData = JSON.parse(artifact.valueJson) as Base64FileData;
          if (documentData && documentData.base64Content) {
            this.existingDocumentData.set(documentData);
            this.showDocumentUploadPanel.set(false); // Hide upload panel initially
            console.log('Existing legacy document loaded:', documentData.fileName);
          }
        } catch (error) {
          console.error('Error parsing document data from ValueJson:', error);
          this.existingDocumentData.set(null);
          this.showDocumentUploadPanel.set(true); // Show upload panel if parsing fails
        }
      } else {
        this.existingDocumentData.set(null);
        this.showDocumentUploadPanel.set(true); // Show upload panel if no existing document
      }
    }
  }

  /**
   * @description Reset all form values to their initial state
   * @returns {void}
   * @since 1.0.0
   */
  private resetFormValues() {
    this.valueText.set('');
    this.valueNumber.set(null);
    this.valueBoolean.set(null);
    this.valueDate.set(null);
    this.documentId.set(null);
    this.uploadedFile.set(null);
    this.existingDocumentData.set(null);
    this.showDocumentUploadPanel.set(true);
  }

  /**
   * @description Handle file selection from the file upload component
   * @param {Base64FileData} fileData - The uploaded file data with base64 content
   * @returns {void}
   * @since 1.0.0
   */
  onFileSelected(fileData: Base64FileData): void {
    this.uploadedFile.set(fileData);
    console.log('File selected:', fileData.fileName, 'Size:', fileData.fileSize);
  }

  /**
   * @description Handle file clear from the file upload component
   * @returns {void}
   * @since 1.0.0
   */
  onFileCleared(): void {
    this.uploadedFile.set(null);
    console.log('File cleared');
  }

  /**
   * @description Show the upload panel to replace the existing document
   * @returns {void}
   * @since 1.0.0
   */
  onReplaceDocument(): void {
    this.showDocumentUploadPanel.set(true);
    this.uploadedFile.set(null);
    console.log('Replace document mode activated');
  }

  /**
   * @description Cancel document replacement and go back to viewing existing document
   * @returns {void}
   * @since 1.0.0
   */
  onCancelReplaceDocument(): void {
    if (this.existingDocumentData()) {
      this.showDocumentUploadPanel.set(false);
      this.uploadedFile.set(null);
      console.log('Cancelled document replacement');
    }
  }

  /**
   * @description Download the existing document
   * @returns {void}
   * @since 1.0.0
   */
  onDownloadDocument(): void {
    const docData = this.existingDocumentData();
    const artifact = this.currentArtifact();
    
    if (!docData) {
      return;
    }

    // Check if this is a GCS document (no base64 content)
    if (!docData.base64Content && artifact) {
      // Get signed URL from backend and download
      const entityType = this.selectedEntityType();
      const entityRecord = this.selectedEntityRecord();
      const artifactType = this.selectedArtifactType();
      
      if (entityType && entityRecord && artifactType) {
        this.entityArtifactService.getDocumentUrl(entityType, entityRecord.id, artifactType.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (response) => {
              if (response && response.url) {
                // Create a temporary link and trigger download
                const link = document.createElement('a');
                link.href = response.url;
                link.download = response.fileName || docData.fileName;
                link.target = '_blank';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                console.log('GCS Document download triggered:', response.fileName);
              }
            },
            error: (error) => {
              console.error('Error getting document URL:', error);
              this.feedbackService.showErrorToast({
                summary: this.translateService.instant('entityArtifact.errors.error'),
                detail: this.translateService.instant('entityArtifact.errors.downloadFailed')
              });
            }
          });
      }
      return;
    }

    // Legacy base64 download
    try {
      // Create a data URL from the base64 content
      const dataUrl = `data:${docData.fileType};base64,${docData.base64Content}`;
      
      // Create a temporary link element
      const link = document.createElement('a');
      link.href = dataUrl;
      link.download = docData.fileName;
      
      // Trigger download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      console.log('Document downloaded:', docData.fileName);
    } catch (error) {
      console.error('Error downloading document:', error);
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('entityArtifact.errors.error'),
        detail: this.translateService.instant('entityArtifact.errors.downloadFailed')
      });
    }
  }

  /**
   * @description Preview the existing document in a new tab
   * @returns {void}
   * @since 1.0.0
   */
  onPreviewDocument(): void {
    const docData = this.existingDocumentData();
    const artifact = this.currentArtifact();
    
    if (!docData) {
      return;
    }

    // Check if this is a GCS document (no base64 content)
    if (!docData.base64Content && artifact) {
      // Get signed URL from backend and open in new tab
      const entityType = this.selectedEntityType();
      const entityRecord = this.selectedEntityRecord();
      const artifactType = this.selectedArtifactType();
      
      if (entityType && entityRecord && artifactType) {
        this.entityArtifactService.getDocumentUrl(entityType, entityRecord.id, artifactType.id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (response) => {
              if (response && response.url) {
                // Open signed URL in new tab
                window.open(response.url, '_blank');
                console.log('GCS Document preview opened:', response.fileName);
              }
            },
            error: (error) => {
              console.error('Error getting document URL:', error);
              this.feedbackService.showErrorToast({
                summary: this.translateService.instant('entityArtifact.errors.error'),
                detail: this.translateService.instant('entityArtifact.errors.previewFailed')
              });
            }
          });
      }
      return;
    }

    // Legacy base64 preview
    try {
      // Create a data URL from the base64 content
      const dataUrl = `data:${docData.fileType};base64,${docData.base64Content}`;
      
      const isImage = docData.fileType.startsWith('image/');
      
      // Open in a new window
      const newWindow = window.open('', '_blank', 'width=1200,height=800');
      if (newWindow) {
        newWindow.document.write(`
          <!DOCTYPE html>
          <html>
          <head>
            <title>${docData.fileName}</title>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <style>
              * {
                margin: 0;
                padding: 0;
                box-sizing: border-box;
              }
              
              html, body {
                width: 100%;
                height: 100%;
                overflow: hidden;
              }
              
              ${isImage ? `
                body {
                  display: flex;
                  justify-content: center;
                  align-items: center;
                  background: #2d2d2d;
                  padding: 20px;
                }
                
                img {
                  max-width: 100%;
                  max-height: 100%;
                  width: auto;
                  height: auto;
                  object-fit: contain;
                  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
                }
              ` : `
                body {
                  background: #525659;
                }
                
                iframe {
                  width: 100%;
                  height: 100%;
                  border: none;
                  display: block;
                }
              `}
            </style>
          </head>
          <body>
            ${isImage 
              ? `<img src="${dataUrl}" alt="${docData.fileName}" />`
              : `<iframe src="${dataUrl}"></iframe>`
            }
          </body>
          </html>
        `);
        newWindow.document.close();
      } else {
        // Fallback if popup blocker prevented opening
        this.onDownloadDocument();
      }
      
      console.log('Document previewed:', docData.fileName);
    } catch (error) {
      console.error('Error previewing document:', error);
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('entityArtifact.errors.error'),
        detail: this.translateService.instant('entityArtifact.errors.previewFailed')
      });
    }
  }

  /**
   * @description Check if the form has a valid artifact value based on data type
   * @returns {boolean}
   * @since 1.0.0
   */
  private hasArtifactValue(): boolean {
    const dataType = this.selectedDataType();
    
    switch (dataType) {
      case 'string':
      case 'text':
        return !!this.valueText() && this.valueText().trim().length > 0;
      case 'number':
      case 'numeric':
      case 'decimal':
        return this.valueNumber() !== null;
      case 'boolean':
      case 'bool':
        return this.valueBoolean() !== null;
      case 'date':
      case 'datetime':
        return this.valueDate() !== null;
      case 'document':
        // Valid if there's a newly uploaded file OR an existing document
        return this.uploadedFile() !== null || this.existingDocumentData() !== null;
      default:
        return false;
    }
  }

  /**
   * @description Save (upsert) the entity artifact
   * @returns {void}
   * @since 1.0.0
   */
  onSave() {
    // Validate form
    if (!this.isFormValid()) {
      this.showValidationError.set(true);
      return;
    }

    const entityType = this.selectedEntityType();
    const entityRecord = this.selectedEntityRecord();
    const artifactType = this.selectedArtifactType();

    if (!entityType || !entityRecord || !artifactType) {
      this.showValidationError.set(true);
      return;
    }

    this.saving.set(true);

    // For document type with a newly uploaded file, use the GCS upload endpoint
    if (this.selectedDataType() === 'document' && this.uploadedFile()) {
      const fileData = this.uploadedFile()!;
      
      // Convert base64 to File object
      const byteString = atob(fileData.base64Content);
      const arrayBuffer = new ArrayBuffer(byteString.length);
      const uint8Array = new Uint8Array(arrayBuffer);
      for (let i = 0; i < byteString.length; i++) {
        uint8Array[i] = byteString.charCodeAt(i);
      }
      const blob = new Blob([uint8Array], { type: fileData.fileType });
      const file = new File([blob], fileData.fileName, { type: fileData.fileType });

      this.entityArtifactService.uploadDocumentArtifact(
        entityType,
        entityRecord.id,
        artifactType.id,
        artifactType.artifactTypeCode,
        file,
        fileData.fileName,
        'User Input'
      )
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (artifact) => {
            this.currentArtifact.set(artifact);
            this.saving.set(false);
            this.showValidationError.set(false);
            
            // Update existing document data with new GCS info
            this.existingDocumentData.set({
              fileName: fileData.fileName,
              fileType: fileData.fileType,
              fileSize: fileData.fileSize,
              base64Content: '' // Empty because document is stored in GCS
            });
            this.uploadedFile.set(null);
            
            // Hide the upload panel and show the existing document card
            this.showDocumentUploadPanel.set(false);
            
            this.feedbackService.showSuccessToast({
              summary: this.translateService.instant('entityArtifact.success.saved'),
              detail: this.translateService.instant('entityArtifact.success.savedDetail')
            });
          },
          error: (error) => {
            console.error('Error uploading document artifact:', error);
            this.saving.set(false);
            // Error handled by global interceptor
          }
        });
      return;
    }

    // For non-document types or document type without new file (keeping existing)
    const request: EntityArtifactRequest = {
      entityType: entityType,
      entityId: entityRecord.id,
      artifactTypeId: artifactType.id,
      valueText: this.selectedDataType() !== 'document' ? (this.valueText() || null) : null,
      valueNumber: this.valueNumber(),
      valueBoolean: this.valueBoolean(),
      valueDate: this.valueDate()?.toISOString() || null,
      valueJson: null, // No longer storing base64 in valueJson
      documentId: this.documentId(),
      source: 'User Input'
    };

    this.entityArtifactService.upsertEntityArtifact(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (artifact) => {
          this.currentArtifact.set(artifact);
          this.saving.set(false);
          this.showValidationError.set(false);
          
          // Hide the upload panel and show the existing document card
          this.showDocumentUploadPanel.set(false);
          
          this.feedbackService.showSuccessToast({
            summary: this.translateService.instant('entityArtifact.success.saved'),
            detail: this.translateService.instant('entityArtifact.success.savedDetail')
          });
        },
        error: (error) => {
          console.error('Error saving artifact:', error);
          this.saving.set(false);
          // Error handled by global interceptor
        }
      });
  }

  /**
   * @description Format file size for display
   * @param {number} bytes - File size in bytes
   * @returns {string} Formatted file size string
   * @since 1.0.0
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }

  /**
   * @description Clear the form and reset all selections
   * @returns {void}
   * @since 1.0.0
   */
  onClear() {
    this.selectedEntityType.set(null);
    this.selectedArtifactType.set(null);
    this.selectedEntityRecord.set(null);
    this.artifactTypes.set([]);
    this.entityRecords.set([]);
    this.currentArtifact.set(null);
    this.resetFormValues();
    this.showValidationError.set(false);
  }
}

