/**
 * @fileoverview Component for bulk updating Entity Artifacts via CSV upload
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
import { MultiSelectModule } from 'primeng/multiselect';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TableModule } from 'primeng/table';
import { ChipModule } from 'primeng/chip';
import { TagModule } from 'primeng/tag';
import { FileUploadModule } from 'primeng/fileupload';

import { MessageService } from 'primeng/api';
import {
  EntityArtifactService,
  EntityTypeOption,
  ArtifactTypeResponse,
  EntityUniqueIdExampleResponse,
  BulkTemplateDownloadRequest,
  BulkEntityArtifactRequest,
  BulkEntityArtifactRowRequest,
  BulkEntityArtifactResponse,
  BulkEntityArtifactRowResult
} from '../services/entity-artifact.service';

import { PermissionService, EntityPermissions } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @class BulkEntityArtifactUpdateComponent
 * @description Administrative interface for bulk updating Entity Artifacts via CSV upload.
 * Allows users to download a template, fill it with data, and upload for batch processing.
 * Supports different data types and provides detailed results for each cell processed.
 * 
 * @example
 * ```html
 * <!-- Usage in routing -->
 * <app-bulk-entity-artifact-update></app-bulk-entity-artifact-update>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-bulk-entity-artifact-update',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    SelectModule,
    MultiSelectModule,
    ButtonModule,
    ProgressSpinnerModule,
    ToastModule,
    MessageModule,
    FloatLabelModule,
    TableModule,
    ChipModule,
    TagModule,
    FileUploadModule
  ],
  providers: [MessageService],
  templateUrl: './bulk-entity-artifact-update.component.html',
  styleUrls: ['./bulk-entity-artifact-update.component.scss']
})
export class BulkEntityArtifactUpdateComponent implements OnInit {
  private readonly entityArtifactService = inject(EntityArtifactService);
  private readonly messageService = inject(MessageService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly translateService = inject(TranslateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly permissionService = inject(PermissionService);

  // Entity permissions
  readonly entityPermissions = signal<EntityPermissions>({
    entity: 'EntityArtifact',
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

  // State signals
  readonly entityTypes = signal<EntityTypeOption[]>([]);
  readonly selectedEntityType = signal<string | null>(null);
  readonly artifactTypes = signal<ArtifactTypeResponse[]>([]);
  readonly selectedArtifactTypeIds = signal<number[]>([]);
  readonly uniqueIdExample = signal<EntityUniqueIdExampleResponse | null>(null);

  // Loading states
  readonly permissionsLoading = signal(true);
  readonly loadingEntityTypes = signal(false);
  readonly loadingArtifactTypes = signal(false);
  readonly loadingUniqueIdExample = signal(false);
  readonly downloadingTemplate = signal(false);
  readonly processingUpload = signal(false);

  // Validation
  readonly showValidationError = signal(false);

  // Upload state
  readonly uploadedFile = signal<File | null>(null);
  readonly uploadResults = signal<BulkEntityArtifactResponse | null>(null);

  // Computed
  readonly canDownloadTemplate = computed(() => {
    return this.selectedEntityType() && 
           this.selectedArtifactTypeIds().length > 0 &&
           !this.downloadingTemplate();
  });

  readonly canUploadFile = computed(() => {
    return this.selectedEntityType() && 
           this.selectedArtifactTypeIds().length > 0 &&
           this.uploadedFile() &&
           !this.processingUpload();
  });

  readonly availableArtifactTypes = computed(() => {
    // Filter out document type artifacts
    return this.artifactTypes().filter(at => 
      at.artifactDataTypeName?.toLowerCase() !== 'document'
    );
  });

  ngOnInit(): void {
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
              summary: this.translateService.instant('bulkEntityArtifact.errors.accessDenied'),
              detail: this.translateService.instant('bulkEntityArtifact.errors.noPermissionToAccess')
            });
            this.router.navigate(['/access-denied']);
            return;
          }
          
          this.loadEntityTypes();
          this.cdr.detectChanges();
        },
        error: (error: any) => {
          console.error('Error loading permissions:', error);
          this.permissionsLoading.set(false);
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('bulkEntityArtifact.errors.error'),
            detail: this.translateService.instant('bulkEntityArtifact.errors.failedToLoadPermissions')
          });
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * @description Load all available entity types
   * @returns {void}
   * @since 1.0.0
   */
  private loadEntityTypes() {
    this.loadingEntityTypes.set(true);
    this.entityArtifactService.getEntityTypes().subscribe({
      next: (types) => {
        this.entityTypes.set(types);
        this.loadingEntityTypes.set(false);
      },
      error: (error: any) => {
        console.error('Error loading entity types:', error);
        this.loadingEntityTypes.set(false);
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('bulkEntityArtifact.errors.error'),
          detail: this.translateService.instant('bulkEntityArtifact.errors.failedToLoadEntityTypes')
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
    this.artifactTypes.set([]);
    this.selectedArtifactTypeIds.set([]);
    this.uniqueIdExample.set(null);
    this.uploadedFile.set(null);
    this.uploadResults.set(null);
    this.showValidationError.set(false);

    const entityType = this.selectedEntityType();
    if (!entityType) return;

    // Load artifact types and unique ID example
    this.loadArtifactTypes(entityType);
    this.loadUniqueIdExample(entityType);
  }

  /**
   * @description Load artifact types for selected entity type
   * @param {string} entityType - The selected entity type
   * @returns {void}
   * @since 1.0.0
   */
  private loadArtifactTypes(entityType: string) {
    this.loadingArtifactTypes.set(true);
    this.entityArtifactService.getArtifactTypesByEntityType(entityType).subscribe({
      next: (types) => {
        this.artifactTypes.set(types);
        
        // Automatically preselect all available artifact types (excluding document types)
        const availableTypes = types.filter(at => 
          at.artifactDataTypeName?.toLowerCase() !== 'document'
        );
        this.selectedArtifactTypeIds.set(availableTypes.map(at => at.id));
        
        this.loadingArtifactTypes.set(false);
      },
      error: (error: any) => {
        console.error('Error loading artifact types:', error);
        this.loadingArtifactTypes.set(false);
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('bulkEntityArtifact.errors.error'),
          detail: this.translateService.instant('bulkEntityArtifact.errors.failedToLoadArtifactTypes')
        });
      }
    });
  }

  /**
   * @description Load unique identifier example for entity type
   * @param {string} entityType - The selected entity type
   * @returns {void}
   * @since 1.0.0
   */
  private loadUniqueIdExample(entityType: string) {
    this.loadingUniqueIdExample.set(true);
    this.entityArtifactService.getBulkUniqueIdExample(entityType).subscribe({
      next: (example) => {
        this.uniqueIdExample.set(example);
        this.loadingUniqueIdExample.set(false);
      },
      error: (error: any) => {
        console.error('Error loading unique ID example:', error);
        this.loadingUniqueIdExample.set(false);
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('bulkEntityArtifact.errors.error'),
          detail: this.translateService.instant('bulkEntityArtifact.errors.failedToLoadUniqueIdExample')
        });
      }
    });
  }

  /**
   * @description Handle artifact type selection change
   * @returns {void}
   * @since 1.0.0
   */
  onArtifactTypeSelectionChange() {
    this.uploadedFile.set(null);
    this.uploadResults.set(null);
    this.showValidationError.set(false);
  }

  /**
   * @description Download CSV template for bulk import
   * @returns {void}
   * @since 1.0.0
   */
  onDownloadTemplate() {
    if (!this.canDownloadTemplate()) {
      this.showValidationError.set(true);
      return;
    }

    const request: BulkTemplateDownloadRequest = {
      entityType: this.selectedEntityType()!,
      artifactTypeIds: this.selectedArtifactTypeIds()
    };

    this.downloadingTemplate.set(true);
    this.entityArtifactService.downloadBulkTemplate(request).subscribe({
      next: (blob) => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `EntityArtifact_BulkImport_${this.selectedEntityType()}_${new Date().toISOString().split('T')[0]}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);

        this.downloadingTemplate.set(false);
        this.feedbackService.showSuccessToast({
          summary: 'Success',
          detail: 'Template downloaded successfully'
        });
      }
    });
  }

  /**
   * @description Handle file upload
   * @param {any} event - File upload event
   * @returns {void}
   * @since 1.0.0
   */
  onFileUpload(event: any) {
    const file = event.files[0];
    if (file) {
      this.uploadedFile.set(file);
      this.uploadResults.set(null);
    }
  }

  /**
   * @description Handle file clear
   * @returns {void}
   * @since 1.0.0
   */
  onFileClear() {
    this.uploadedFile.set(null);
    this.uploadResults.set(null);
  }

  /**
   * @description Process uploaded CSV file
   * @returns {void}
   * @since 1.0.0
   */
  async onProcessUpload() {
    if (!this.canUploadFile()) {
      this.showValidationError.set(true);
      return;
    }

    const file = this.uploadedFile();
    if (!file) return;

    this.processingUpload.set(true);

    try {
      // Read and parse CSV file
      const csvText = await this.readFileAsText(file);
      const parsedData = this.parseCSV(csvText);

      // Create bulk upsert request
      const request: BulkEntityArtifactRequest = {
        entityType: this.selectedEntityType()!,
        rows: parsedData.rows,
        columnToArtifactTypeMapping: parsedData.columnMapping
      };

      // Send to backend
      this.entityArtifactService.bulkUpsertEntityArtifacts(request).subscribe({
        next: (response) => {
          this.uploadResults.set(response);
          this.processingUpload.set(false);

          this.feedbackService.showSuccessToast({
            summary: 'Processing Complete',
            detail: `Processed ${response.totalRows} rows. ${response.successfulRows} successful, ${response.failedRows} failed.`
          });
        }
      });
    } catch (error: any) {
      this.processingUpload.set(false);
      this.feedbackService.showErrorToast({
        summary: 'Error',
        detail: error.message || 'Failed to process file'
      });
    }
  }

  /**
   * @description Read file as text
   * @param {File} file - File to read
   * @returns {Promise<string>} File content as text
   * @since 1.0.0
   */
  private readFileAsText(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target?.result as string);
      reader.onerror = (e) => reject(e);
      reader.readAsText(file);
    });
  }

  /**
   * @description Parse CSV content
   * @param {string} csvText - CSV content
   * @returns {object} Parsed data with rows and column mapping
   * @since 1.0.0
   */
  private parseCSV(csvText: string): { rows: BulkEntityArtifactRowRequest[], columnMapping: { [key: number]: number } } {
    const lines = csvText.split('\n').map(line => line.trim()).filter(line => line.length > 0);
    
    if (lines.length < 2) {
      throw new Error('CSV file must have at least 2 rows (header and data)');
    }

    // Skip first row (header)
    // Check if row 2 is data type hints or actual data
    let dataStartIndex = 1;
    if (lines.length > 1) {
      const secondRow = this.parseCSVLine(lines[1]);
      // Check if second row is data type hints (starts with "Example:" or contains data type hints like "(text)")
      if (secondRow[0] && (secondRow[0].toLowerCase().startsWith('example:') || secondRow[0].includes('('))) {
        // Row 2 is data type hints, start from row 3
        dataStartIndex = 2;
      } else {
        // Row 2 is actual data, start from row 2
        dataStartIndex = 1;
      }
    }

    const dataRows = lines.slice(dataStartIndex);

    if (dataRows.length === 0) {
      throw new Error('CSV file contains no data rows');
    }

    // Build column to artifact type mapping
    const columnMapping: { [key: number]: number } = {};
    const selectedIds = this.selectedArtifactTypeIds();
    for (let i = 0; i < selectedIds.length; i++) {
      columnMapping[i] = selectedIds[i];
    }

    // Parse data rows
    const rows: BulkEntityArtifactRowRequest[] = [];
    dataRows.forEach((line, index) => {
      const cells = this.parseCSVLine(line);
      
      if (cells.length < 2) return; // Skip empty rows
      
      const uniqueId = cells[0].trim();
      if (!uniqueId) return; // Skip rows without unique ID

      const cellValues: { [key: number]: string } = {};
      for (let i = 1; i < cells.length; i++) {
        cellValues[i - 1] = cells[i];
      }

      rows.push({
        rowNumber: dataStartIndex + index + 1, // Actual row number in the spreadsheet
        uniqueId,
        cellValues
      });
    });

    return { rows, columnMapping };
  }

  /**
   * @description Parse a single CSV line handling quoted values
   * @param {string} line - CSV line
   * @returns {string[]} Parsed cells
   * @since 1.0.0
   */
  private parseCSVLine(line: string): string[] {
    const cells: string[] = [];
    let currentCell = '';
    let insideQuotes = false;

    for (let i = 0; i < line.length; i++) {
      const char = line[i];

      if (char === '"') {
        if (insideQuotes && line[i + 1] === '"') {
          // Escaped quote
          currentCell += '"';
          i++;
        } else {
          // Toggle quotes
          insideQuotes = !insideQuotes;
        }
      } else if (char === ',' && !insideQuotes) {
        // End of cell
        cells.push(currentCell);
        currentCell = '';
      } else {
        currentCell += char;
      }
    }

    // Add last cell
    cells.push(currentCell);

    return cells;
  }

  /**
   * @description Clear all form data
   * @returns {void}
   * @since 1.0.0
   */
  onClear() {
    this.selectedEntityType.set(null);
    this.selectedArtifactTypeIds.set([]);
    this.artifactTypes.set([]);
    this.uniqueIdExample.set(null);
    this.uploadedFile.set(null);
    this.uploadResults.set(null);
    this.showValidationError.set(false);
  }

  /**
   * @description Get severity for result status
   * @param {boolean} success - Whether the operation was successful
   * @returns {string} Severity level
   * @since 1.0.0
   */
  getResultSeverity(success: boolean): string {
    return success ? 'success' : 'danger';
  }

  /**
   * @description Get cell status badge severity
   * @param {boolean} success - Whether the cell was successful
   * @param {boolean} skipped - Whether the cell was skipped
   * @returns {string} Badge severity
   * @since 1.0.0
   */
  getCellStatusSeverity(success: boolean, skipped: boolean): string {
    if (skipped) return 'secondary';
    return success ? 'success' : 'danger';
  }

  /**
   * @description Get cell status label
   * @param {boolean} success - Whether the cell was successful
   * @param {boolean} skipped - Whether the cell was skipped
   * @returns {string} Status label
   * @since 1.0.0
   */
  getCellStatusLabel(success: boolean, skipped: boolean): string {
    if (skipped) return 'Skipped';
    return success ? 'Success' : 'Failed';
  }
}

