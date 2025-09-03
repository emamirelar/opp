import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal, computed, Type } from '@angular/core';
import { DynamicDialogConfig } from 'primeng/dynamicdialog';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ProgressBarModule } from 'primeng/progressbar';
import { CardModule } from 'primeng/card';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageModule } from 'primeng/message';
import { BlockUIModule } from 'primeng/blockui';
import { StepperModule } from 'primeng/stepper';
import { FeedbackDialogService } from '../../../../pages/services/feedback-dialog.service';
import { NgClass, JsonPipe, TitleCasePipe } from '@angular/common';
import { ImportDialogService } from './import-dialog.service';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule, CheckboxChangeEvent } from 'primeng/checkbox';
import { ChipModule } from 'primeng/chip';
import { ComponentResolverService } from '../../../../../features/internal/services/component-resolver.service';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';
import { ImportService } from '../import.service';
import { DuplicateIndicatorComponent } from '../duplicate-indicator/duplicate-indicator.component';
import { UserManagementService } from '../../../../../features/internal/admin/user-management/user-management.service';
import { DuplicateSummaryComponent } from '../duplicate-summary/duplicate-summary.component';

// Custom interface for import columns that extends ListViewColumn
interface ImportColumn extends ListViewColumn {
  header: string; // Used instead of label for display in the import table
  required?: boolean; // Whether this field is required for import
}

@Component({
  selector: 'app-import-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    ButtonModule,
    TableModule,
    InputTextModule,
    DialogModule,
    ProgressBarModule,
    CardModule,
    FormsModule,
    ReactiveFormsModule,
    MessageModule,
    BlockUIModule,
    StepperModule,
    NgClass,
    PaginatorModule,
    ProgressSpinnerModule,
    DropdownModule,
    TooltipModule,
    CheckboxModule,
    ChipModule,
    TitleCasePipe,
    DuplicateIndicatorComponent,
    DuplicateSummaryComponent
  ],
  templateUrl: './import-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportDialogComponent implements OnInit {
  feedbackDialogService = inject(FeedbackDialogService);
  importDialogService = inject(ImportDialogService);
  componentResolverService = inject(ComponentResolverService);
  importService = inject(ImportService);
  translateService = inject(TranslateService);
  userManagementService = inject(UserManagementService);
  // Make Math available to the template
  Math = Math;

  // Current import type being displayed
  currentImportType = computed(() => this.importDialogService.getImportType());

  errorMessage = signal<string>('');
  selectedRows = signal<any[]>([]);
  validationErrors = signal<Map<number, string[]>>(new Map());
  rowsWithMissingRequired = signal<number[]>([]);
  showMissingRequiredBanner = signal<boolean>(false);
  
  // Duplicate detection properties
  duplicateRows = signal<any[]>([]);
  nonDuplicateRows = signal<any[]>([]);
  showDuplicateWarning = signal<boolean>(false);
  duplicateWarningMessage = signal<string>('');

  // Pagination properties
  first = signal(0);
  rows = signal(10);
  rowsModel = 10; // For dropdown binding
  totalRecords = signal(0);
  paginatedData = signal<any[]>([]);

  // Table columns configuration
  columns: ImportColumn[] = [];

  // Contact-specific columns
  contactColumns: ImportColumn[] = [
    { field: 'salutation', header: 'contact.salutation', required: false, label: 'Salutation', type: 'text', sortable: false },
    { field: 'firstName', header: 'contact.firstName', required: false, label: 'First Name', type: 'text', sortable: false },
    { field: 'middleName', header: 'contact.middleName', required: false, label: 'Middle Name', type: 'text', sortable: false },
    { field: 'lastName', header: 'contact.lastName', required: true, label: 'Last Name', type: 'text', sortable: false },
    { field: 'suffix', header: 'contact.suffix', required: false, label: 'Suffix', type: 'text', sortable: false },
    { field: 'title', header: 'contact.title', required: true, label: 'Title', type: 'text', sortable: false },
    { field: 'pronouns', header: 'contact.pronouns', required: false, label: 'Pronouns', type: 'text', sortable: false },
    { field: 'birthDate', header: 'contact.birthDate', required: false, label: 'Birth Date', type: 'text', sortable: false },
    { field: 'partnerId', header: 'contact.partnerId', required: true, label: 'Partner ID', type: 'text', sortable: false },
    { field: 'email', header: 'contact.email', required: true, label: 'Email', type: 'text', sortable: false },
    { field: 'phone', header: 'contact.phone', required: false, label: 'Phone', type: 'text', sortable: false },
    { field: 'mobile', header: 'contact.mobile', required: false, label: 'Mobile', type: 'text', sortable: false },
    { field: 'otherPhone', header: 'contact.otherPhone', required: false, label: 'Other Phone', type: 'text', sortable: false },
    { field: 'fax', header: 'contact.fax', required: false, label: 'Fax', type: 'text', sortable: false },
    { field: 'department', header: 'contact.department', required: false, label: 'Department', type: 'text', sortable: false },
    { field: 'description', header: 'contact.description', required: false, label: 'Description', type: 'text', sortable: false },
    { field: 'status', header: 'contact.status', required: false, label: 'Status', type: 'text', sortable: false },
    { field: 'contactNumber', header: 'contact.contactNumber', required: false, label: 'Contact Number', type: 'text', sortable: false },
    { field: 'assistant', header: 'contact.assistant', required: false, label: 'Assistant', type: 'text', sortable: false },
    { field: 'assistantPhone', header: 'contact.assistantPhone', required: false, label: 'Assistant Phone', type: 'text', sortable: false },
    { field: 'assistantEmail', header: 'contact.assistantEmail', required: false, label: 'Assistant Email', type: 'text', sortable: false },
    { field: 'mailingStreet', header: 'contact.mailingStreet', required: false, label: 'Mailing Street', type: 'text', sortable: false },
    { field: 'mailingStreet2', header: 'contact.mailingStreet2', required: false, label: 'Mailing Street 2', type: 'text', sortable: false },
    { field: 'mailingCity', header: 'contact.mailingCity', required: false, label: 'Mailing City', type: 'text', sortable: false },
    { field: 'mailingStateProvince', header: 'contact.mailingStateProvince', required: false, label: 'Mailing State/Province', type: 'text', sortable: false },
    { field: 'mailingPostalCode', header: 'contact.mailingPostalCode', required: false, label: 'Mailing Postal Code', type: 'text', sortable: false },
    { field: 'mailingCountry', header: 'contact.mailingCountry', required: false, label: 'Mailing Country', type: 'text', sortable: false },
  ];

  // Partner-specific columns (all fields from Partner.cs, filtered by permissions)
  allPartnerColumns: ImportColumn[] = [
    // Essential Fields
    { field: 'name', header: 'partner.name', required: true, label: 'Name', type: 'text', sortable: false },
    { field: 'partnerShortDescription', header: 'partner.shortName', required: false, label: 'Short Name', type: 'text', sortable: false },
    { field: 'partnerLongDescription', header: 'partner.longDescription', required: false, label: 'Long Description', type: 'text', sortable: false },
    
    // Classification & Organization
    { field: 'partnerCategoryId', header: 'partner.partnerCategory', required: false, label: 'Partner Category', type: 'text', sortable: false },
    { field: 'partnerGroupCode', header: 'partner.partnerGroup', required: false, label: 'Partner Group', type: 'text', sortable: false },
    { field: 'liaisonOfficeId', header: 'partner.liaisonOffice', required: false, label: 'Liaison Office', type: 'text', sortable: false },
    { field: 'partnerFocalPointUserId', header: 'partner.partnerFocalPoint', required: false, label: 'Partner Focal Point', type: 'text', sortable: false },
    // Status & Operational
    { field: 'status', header: 'partner.status', required: false, label: 'Status', type: 'text', sortable: false },
  ];

  // Filtered partner columns based on user permissions
  partnerColumns: ImportColumn[] = [];

  // Interaction-specific columns
  interactionColumns: ImportColumn[] = [
    { field: 'type', header: 'interaction.type', required: true, label: 'Type', type: 'text', sortable: false },
    { field: 'date', header: 'interaction.date', required: true, label: 'Date', type: 'text', sortable: false },
    { field: 'subject', header: 'interaction.subject', required: true, label: 'Subject', type: 'text', sortable: false },
    { field: 'description', header: 'interaction.description', required: false, label: 'Description', type: 'text', sortable: false },
    { field: 'contactId', header: 'interaction.contact', required: false, label: 'Contact', type: 'text', sortable: false },
    { field: 'location', header: 'interaction.location', required: false, label: 'Location', type: 'text', sortable: false },
    { field: 'contactIds', header: 'interaction.contactIds', required: false, label: 'Contact IDs', type: 'text', sortable: false },
    { field: 'partnerIds', header: 'interaction.partnerIds', required: false, label: 'Partner IDs', type: 'text', sortable: false },
    { field: 'userIds', header: 'interaction.userIds', required: false, label: 'User IDs', type: 'text', sortable: false },
    { field: 'emailAddresses', header: 'interaction.emailAddresses', required: false, label: 'Email Addresses', type: 'text', sortable: false },
    { field: 'phoneNumbers', header: 'interaction.phoneNumbers', required: false, label: 'Phone Numbers', type: 'text', sortable: false },
    { field: 'organizationHierarchyIds', header: 'interaction.organizationUnitIds', required: false, label: 'Organization Unit IDs', type: 'text', sortable: false }
  ];

  // User Role-specific columns
  userRoleColumns: ImportColumn[] = [
    { field: 'userDisplay', header: 'User', required: true, label: 'User', type: 'text', sortable: false },
    { field: 'roleDisplay', header: 'Role(s)', required: true, label: 'Role(s)', type: 'text', sortable: false }
  ];

  // Create data effect in the constructor to ensure injection context
  constructor() {
    // Setup effect to update paginated data when data changes
    effect(() => {
      // Update columns again if the import type changes
      this.updateColumnsForEntityType();
      
      const allData = this.importDialogService.data();
      
      if (allData && allData.length > 0) {
        this.totalRecords.set(allData.length);
        // When data changes, ensure we start back at page 1
        this.first.set(0);
        this.updatePaginatedData();
        
        // Check for missing required fields
        this.checkMissingRequiredFields();
        
        // Reset loading state if it's still active
        setTimeout(() => {
          if (this.importDialogService.isLoading()) {
            this.importDialogService.isLoading.set(false);
          }
        }, 100);
      } else {
        this.totalRecords.set(0);
        this.paginatedData.set([]);
        this.selectedRows.set([]);
      }
    });
  }

  async ngOnInit(): Promise<void> {
    // Set the table columns based on the current import type (with permissions)
    await this.updateColumnsForEntityType();
    
    // Immediately check data on init
    this.checkAndProcessData();
  }

  // Update the table columns based on the current import type and user permissions
  private async updateColumnsForEntityType(): Promise<void> {
    const entityType = this.importDialogService.getImportType().toLowerCase();
    
    if (entityType === 'partner') {
      // Filter partner columns based on user permissions
      this.partnerColumns = await this.filterColumnsByPermissions(this.allPartnerColumns, 'Partner');
      this.columns = this.partnerColumns;
    } else if (entityType === 'interaction') {
      this.columns = this.interactionColumns;
    } else if (entityType === 'user_role_import') {
      this.columns = this.userRoleColumns;
    } else {
      // Default to contact columns
      this.columns = this.contactColumns;
    }
  }

  // Filter columns based on user permissions for canUpdate
  private async filterColumnsByPermissions(allColumns: ImportColumn[], entityName: string): Promise<ImportColumn[]> {
    try {
      // Get entity permissions from the backend
      const response = await fetch(`/api/${entityName.toLowerCase()}/permissions`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        console.warn(`Failed to get permissions for ${entityName}, showing all columns`);
        return allColumns;
      }

      const permissions = await response.json();
      
      // If user has no update restrictions (canEditFields is null), show all columns
      if (!permissions.canEditFields || permissions.canEditFields.length === 0) {
        return allColumns;
      }

      // Filter columns based on canEditFields permissions
      const allowedFields = new Set(permissions.canEditFields);
      
      // Always include required fields (like 'name')
      const filteredColumns = allColumns.filter(column => 
        column.required || allowedFields.has(column.field)
      );

      return filteredColumns;
    } catch (error) {
      console.error(`Error checking permissions for ${entityName}:`, error);
      // On error, return all columns to avoid breaking functionality
      return allColumns;
    }
  }
  
  // Check and process data - can be called multiple times if needed
  async checkAndProcessData(): Promise<void> {
    // Use the explicitly set import type from the service
    const entityType = this.importDialogService.getImportType();
    
    
    // Update columns for the current entity type
    this.updateColumnsForEntityType();
    
    // Process the data
    const initialData = this.importDialogService.data();
    
    if (initialData && initialData.length > 0) {
      // Add a unique non-conflicting ID to each row for selection purposes
      let processedData = initialData.map((item, index) => {
        return { ...item, _importRowId: `import-${index}` };
      });

      // Enrich user role data if this is a user role import
      if (this.isUserRoleImport()) {
        try {
          processedData = await this.enrichUserRoleData(processedData);
        } catch (error) {
          console.error('Error enriching user role data:', error);
          // Continue with original data if enrichment fails
        }
      }
      
      // Update the data in the service with the processed data
      this.importDialogService.data.set(processedData);
      this.totalRecords.set(processedData.length);
      this.updatePaginatedData();
      
      // Check for missing required fields
      this.checkMissingRequiredFields();
      
      // Auto-select all valid rows (exclude rows with errors or missing required fields)
      this.selectValidRows();
      
      // If no data was displayed, try forcing detection
      if (this.paginatedData().length === 0) {
        setTimeout(() => {
          this.updatePaginatedData();
        }, 0);
      }
    } else {
      console.warn('No data available for processing');
      this.totalRecords.set(0);
      this.paginatedData.set([]);
    }
  }

  updatePaginatedData(): void {
    const allData = this.importDialogService.data();
    const firstIndex = this.first();
    const rowsPerPage = this.rows();
    
    if (!allData || allData.length === 0) {
      console.warn('No data available for pagination');
      this.paginatedData.set([]);
      this.totalRecords.set(0);
      this.selectedRows.set([]);
      return;
    }

    // Process duplicate detection and add duplicateInfo to each record
    const processedData = this.processDuplicateDetection(allData);

    // Update total records if it doesn't match the data length
    if (this.totalRecords() !== processedData.length) {
      this.totalRecords.set(processedData.length);
    }
    
    // Ensure firstIndex doesn't exceed the bounds of the data
    if (firstIndex >= processedData.length) {
      const newFirstIndex = 0;
      console.warn(`First index ${firstIndex} exceeds data length ${processedData.length}, resetting to ${newFirstIndex}`);
      this.first.set(newFirstIndex);
      
      const newPaginatedResult = processedData.slice(newFirstIndex, newFirstIndex + rowsPerPage);
      this.paginatedData.set(newPaginatedResult);
      return;
    }
    
    // Normal pagination
    const endIndex = Math.min(firstIndex + rowsPerPage, processedData.length);
    const paginatedResult = processedData.slice(firstIndex, endIndex);
    
    this.paginatedData.set(paginatedResult);
  }

  onPageChange(event: any): void {
    // Verify the event has expected properties
    if (!event || typeof event.first !== 'number' || typeof event.rows !== 'number') {
      console.error('Invalid page change event:', event);
      return;
    }
    
    // Verify that we're not exceeding data bounds
    const dataLength = this.importDialogService.data().length;
    if (event.first >= dataLength) {
      event.first = 0;
    }
    
    // Store current selection before changing page
    const currentSelection = this.selectedRows();
    
    // Update pagination values
    this.first.set(event.first);
    this.rows.set(event.rows);
    this.rowsModel = event.rows;
    
    // Update paginated data (which now preserves selection)
    this.updatePaginatedData();
  }

  onRowsPerPageChange(event: any): void {
    // Reset to first page when changing rows per page
    this.first.set(0);
    this.rows.set(event.value);
    this.updatePaginatedData();
  }

  getFieldHeader(fieldName: string): string {
    const column = this.columns.find(col => col.field === fieldName);
    return column ? column.header : fieldName;
  }

  hasErrors(rowIndex: number): boolean {
    // Adjust the row index to account for pagination
    const actualRowIndex = this.first() + rowIndex;
    return this.validationErrors().has(actualRowIndex);
  }

  getRowErrors(rowIndex: number): string[] {
    // Adjust the row index to account for pagination
    const actualRowIndex = this.first() + rowIndex;
    return this.validationErrors().get(actualRowIndex) || [];
  }

  hasMissingRequiredRows(): boolean {
    return this.rowsWithMissingRequired().length > 0;
  }

  /**
   * Process duplicate detection and add duplicateInfo to each record
   */
  private processDuplicateDetection(data: any[]): any[] {
    const processedData = [...data];
    const duplicateRows: any[] = [];
    const nonDuplicateRows: any[] = [];

    processedData.forEach((record, index) => {
      
      // Check if record has duplicateDetection from the new AI service
      const duplicateDetection = record.duplicateDetection;
      
      if (duplicateDetection?.hasDuplicates) {
        // Use the new duplicateDetection structure
        record.duplicateInfo = {
          isDuplicate: true,
          hasDuplicates: duplicateDetection.hasDuplicates,
          totalDuplicates: duplicateDetection.totalDuplicates,
          highConfidence: duplicateDetection.highConfidence,
          mediumConfidence: duplicateDetection.mediumConfidence,
          lowConfidence: duplicateDetection.lowConfidence,
          topDuplicate: duplicateDetection.topDuplicate,
          tooltip: `${duplicateDetection.totalDuplicates} duplicate(s) found`
        };
        
        duplicateRows.push(record);
      } else if (record.similarityEntityId) {
        // Fallback to old similarity detection for backward compatibility
        const entityId = record.similarityEntityId;
        const similarityScore = record.similarityScore || 0;
        const similarityPercentage = Math.round(similarityScore * 100);
        
        record.duplicateInfo = {
          isDuplicate: true,
          hasDuplicates: true,
          totalDuplicates: 1,
          highConfidence: similarityScore > 0.8 ? 1 : 0,
          mediumConfidence: similarityScore > 0.5 && similarityScore <= 0.8 ? 1 : 0,
          lowConfidence: similarityScore <= 0.5 ? 1 : 0,
          topDuplicate: {
            entityId: entityId,
            score: similarityScore,
            matchReason: 'Legacy similarity match',
            entityType: this.getEntityTypeFromImportType()
          },
          tooltip: `Duplicate found (${similarityPercentage}% similarity)`
        };
        
        duplicateRows.push(record);
      } else {
        // No duplicate found
        record.duplicateInfo = {
          isDuplicate: false,
          hasDuplicates: false,
          totalDuplicates: 0,
          highConfidence: 0,
          mediumConfidence: 0,
          lowConfidence: 0,
          topDuplicate: null,
          tooltip: 'Unique record'
        };
        nonDuplicateRows.push(record);
      }
    });

    // Update signals
    this.duplicateRows.set(duplicateRows);
    this.nonDuplicateRows.set(nonDuplicateRows);
    
    // Show warning if duplicates found
    if (duplicateRows.length > 0) {
      this.showDuplicateWarning.set(true);
      this.duplicateWarningMessage.set(
        `${duplicateRows.length} duplicate(s) found and auto-deselected. Review and manually select if needed.`
      );
    } else {
      this.showDuplicateWarning.set(false);
    }

    return processedData;
  }

  /**
   * Get entity type from current import type
   */
  getEntityTypeFromImportType(): string {
    const importType = this.currentImportType();
    switch (importType) {
      case 'contact':
        return 'contacts';
      case 'partner':
        return 'partners';
      case 'interaction':
        return 'interactions';
      case 'user_role_import':
        return 'user_role';
      default:
        return 'contacts';
    }
  }

  /**
   * Get translated header text
   */
  public getTranslatedHeader(key: string): string {
    try {
      const translated = this.translateService.instant(key);
      
      // Debug: log first few translations
      if (key === 'DUPLICATE_DETECTION.duplicate' || key === 'contact.firstName') {
        console.log(`Key: '${key}' -> Translation: '${translated}' -> Same? ${translated === key}`);
      }
      
      // If translation returns the key itself, it means translation failed
      if (translated !== key) {
        return translated;
      }
      
      // Fallback: try to find a better translation or return a formatted version
      const keyParts = key.split('.');
      const fieldName = keyParts[keyParts.length - 1];
      
      // Convert camelCase to Title Case
      return fieldName.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
    } catch (error) {
      console.error('Translation error:', error);
      return key.split('.').pop() || key;
    }
  }

  /**
   * Generate entity URL for opening in new tab
   */
  private getEntityUrl(entityType: string, entityId: string): string {
    return `/#/partnerships/${entityType}/${entityId}`;
  }

  // Select all rows in the current dataset (including rows with missing required fields and duplicates)
  selectAllRows(): void {
    const allData = this.importDialogService.data();
    
    if (!allData || allData.length === 0) {
      console.warn('No data to select from');
      return;
    }
    
    // Select ALL rows, including those with missing required fields and duplicates
    const newSelection = [...allData];
    
    // Update the local selection state
    this.selectedRows.set(newSelection);
    
    // Update the service with the selected rows
    this.importDialogService.setSelectedRows(newSelection);
    
    // Check if any rows with missing fields are being selected and show a warning
    const missingRequiredRows = this.rowsWithMissingRequired();
    if (missingRequiredRows.length > 0) {
      this.feedbackDialogService.showWarningToast({
        detail: `You've selected ${missingRequiredRows.length} rows with missing required fields`,
        life: 3000
      });
    }

    // Check if any duplicate rows are being selected and show a warning
    const duplicateRows = this.duplicateRows();
    if (duplicateRows.length > 0) {
      this.feedbackDialogService.showWarningToast({
        detail: `You've selected ${duplicateRows.length} duplicate rows. These will be processed as new records.`,
        life: 3000
      });
    }
  }

  // Get the currently selected rows (used for import)
  getSelectedRowsForImport(): any[] {
    const selectedData = this.selectedRows();
    return selectedData;
  }

  // Check if all records are selected (including those with missing required fields)
  areAllRowsSelected(): boolean {
    const allData = this.importDialogService.data();
    const selectedRows = this.selectedRows();
    
    // Check if all records (including those with missing fields) are selected
    return selectedRows.length === allData.length;
  }
  
  toggleSelectAll(event: CheckboxChangeEvent): void {
    // We don't need to call stopPropagation as CheckboxChangeEvent is not a DOM event
    if (this.areAllRowsSelected()) {
      // Clear all selections
      this.selectedRows.set([]);
    } else {
      // Select all rows, including those with missing required fields
      this.selectAllRows();
    }
    
    // Update the service
    this.importDialogService.setSelectedRows(this.selectedRows());
  }

  // Method to check if we have a mixed selection (not all rows selected)
  hasMixedSelection(): boolean {
    const selectedRows = this.selectedRows();
    const allData = this.importDialogService.data();
    
    // If we have some rows selected but not all rows, it's a mixed state
    return selectedRows.length > 0 && selectedRows.length < allData.length;
  }

  // Update selected rows when selection changes
  onSelectionChange(event: any[]): void {
    
    // Get the existing selection that might include rows from other pages
    const currentSelection = this.selectedRows();
    const paginatedRows = this.paginatedData();
    const allData = this.importDialogService.data();
    
    // Store the current page range
    const startIndex = this.first();
    const endIndex = Math.min(startIndex + this.rows(), allData.length);
    
    // Create a Set of IDs of rows on the current page for quick lookup
    const currentPageRowIds = new Set(paginatedRows.map(row => row._importRowId));
    
    // Keep selections from other pages (not on the current page)
    const selectionsFromOtherPages = currentSelection.filter(row => 
      !currentPageRowIds.has(row._importRowId)
    );
    
    // Filter out duplicate rows from the current page selection
    const nonDuplicateEventRows = event.filter(row => !row.duplicateInfo?.isDuplicate);
    
    // Create a new selection by combining:
    // 1. Rows selected from other pages (not visible on current page)
    // 2. Non-duplicate rows selected on the current page from the event
    const newSelection = [
      ...selectionsFromOtherPages,
      ...nonDuplicateEventRows
    ];
    
    // Avoid duplicates by creating a unique set based on _importRowId
    const uniqueSelection = [...new Map(newSelection.map(item => 
      [item._importRowId, item]
    )).values()];
    
    // Update the selection state
    this.selectedRows.set(uniqueSelection);
    
    // Update the service with the selected rows for import
    this.importDialogService.setSelectedRows(uniqueSelection);
    
    // Check if we should still show the warning banner
    // This ensures the banner updates properly when rows are manually selected
    if (this.rowsWithMissingRequired().length > 0) {
      // Check if any rows with missing fields are now selected
      const missingRequiredRows = this.rowsWithMissingRequired();
      
      const selectedRowsWithMissingFields = uniqueSelection.filter(selectedRow => {
        const rowIndex = allData.findIndex(item => item._importRowId === selectedRow._importRowId);
        return missingRequiredRows.includes(rowIndex);
      });
      
      // If there are selected rows with missing fields, show a warning toast
      if (selectedRowsWithMissingFields.length > 0) {
        this.feedbackDialogService.showWarningToast({
          detail: `You've selected ${selectedRowsWithMissingFields.length} rows with missing required fields`,
          life: 3000
        });
      }
    }
  }

  // Implement the edit row functionality
  editRow(row: any, event: Event): void {
    // Prevent the event from propagating (to avoid row selection change)
    event.stopPropagation();
    
    // Make a copy of the row data to avoid reference issues
    const rowCopy = { ...row };
    
    // Flag to indicate this is an import edit
    rowCopy.isImportEdit = true;
    // Flag to tell the component not to save to the server
    rowCopy.skipServerSave = true;
    
    
    
    // Get the entity type
    const entityType = this.importDialogService.getImportType().toLowerCase();
    
    
    // Store a reference to the current row in a temporary map for later access
    // We'll use the importRowId to identify this row when it's updated
    const importRowId = row._importRowId;
    
    // Create a custom save handler for the dialog
    const importSaveHandler = signal<boolean>(false);
    
    // First letter should be capitalized for the component name lookup
    const componentName = entityType.charAt(0).toUpperCase() + entityType.slice(1);
    
    
    try {
      // Create a modified special version of the record for dialog compatibility
      const dialogRecord = { ...rowCopy };
      
      // Special handling for each entity type to ensure form is populated correctly
      if (entityType === 'partner') {
        // For partner, explicitly set certain fields that the form expects
        // organizationHierarchyIds is used directly, no conversion needed
        dialogRecord.partnerCategoryId = dialogRecord.partnerCategoryId || null;
        dialogRecord.partnerApprovalReference = dialogRecord.partnerApprovalReference || '';
        
        // Make sure id is present and formatted appropriately
        if (dialogRecord.id !== undefined && dialogRecord.id !== null) {
          // Ensure id is a string since the component expects a string recordId
          dialogRecord.recordId = String(dialogRecord.id); 
        }
      }
      
      // Log the prepared record
      
      
      // Use the resolver method with additional parameters to identify this as a custom dialog
      // Define our custom behavior through the dialogRecord object
      dialogRecord._importSaveHandler = importSaveHandler;
      dialogRecord._customOpen = true;
      
      // Open the dialog directly
      const componentData = this.componentResolverService['componentMap'][componentName];
      if (!componentData) {
        throw new Error(`Component not found for ${componentName}`);
      }
      
      // Open the dialog with our custom configuration
      const dialogRef = this.componentResolverService.dialogService.open(componentData.component, {
        header: `Edit ${componentName}`,
        width: '40vw',
        breakpoints: { '960px': '95vw' },
        closable: true,
        templates: {
          footer: componentData.footer
        },
        data: {
          mode: 'edit',
          record: dialogRecord,
          requestingSaveSignal: signal<boolean>(false)
        }
      });
      
      // Handle dialog close event to update the row in the table
      dialogRef.onClose.subscribe(result => {
        
        
        if (result && (result._updated || typeof result === 'object')) {
          
          
          // Find the row in the data array
          const allData = this.importDialogService.data();
          const rowIndex = allData.findIndex(item => item._importRowId === importRowId);
          
          if (rowIndex !== -1) {
            // Create a new array with the updated row
            const updatedData = [...allData];
            
            // If result is directly the updated record
            if (result._updated) {
              // Keep the importRowId from the original row
              result._importRowId = importRowId;
              updatedData[rowIndex] = result;
            } 
            // If we just need to apply changes from dialog form
            else if (typeof result === 'object') {
              // Copy all properties from the result to the original row
              const updatedRow = { ...allData[rowIndex], ...result };
              updatedData[rowIndex] = updatedRow;
            }
            
            // Update the data in the service
            this.importDialogService.data.set(updatedData);
            
            // Update paginated data and trigger change detection
            this.updatePaginatedData();
            
            // Check for missing required fields
            this.checkRowForMissingFields(updatedData[rowIndex]);
          }
        }
        
        // Always trigger a refresh-listview event for compatibility
        window.dispatchEvent(new CustomEvent('refresh-listview'));
      });
      
      
    } catch (error) {
      console.error('Error opening edit dialog:', error);
      this.feedbackDialogService.showErrorToast({ 
        detail: 'Error opening edit dialog' 
      });
    }
  }
  
  // Helper method to check if a row has missing required fields
  checkRowForMissingFields(row: any): void {
    // Find the row index in the data array
    const allData = this.importDialogService.data();
    const importRowId = row._importRowId;
    const rowIndex = allData.findIndex(item => item._importRowId === importRowId);
    
    if (rowIndex === -1) {
      console.warn('Row not found in data array');
      return;
    }
    
    // Check for missing required fields
    const missingFields = this.columns
      .filter(col => {
        return col.required && !row[col.field];
      })
      .map(col => col.field);
    
    if (missingFields.length > 0) {
      // Still has missing required fields
      const currentMissingRows = this.rowsWithMissingRequired();
      if (!currentMissingRows.includes(rowIndex)) {
        // Add this row to the missing required rows
        this.rowsWithMissingRequired.set([...currentMissingRows, rowIndex]);
      }
      
      if (missingFields.length > 0) {
        // Show a warning about still missing fields
        this.feedbackDialogService.showWarningToast({
          detail: `Row updated but still missing required fields: ${missingFields.join(', ')}`,
          life: 5000
        });
      }
    } else {
      // No missing required fields for this row
      const currentMissingRows = this.rowsWithMissingRequired();
      if (currentMissingRows.includes(rowIndex)) {
        // Remove this row from the missing required rows
        this.rowsWithMissingRequired.set(currentMissingRows.filter(index => index !== rowIndex));
      }
    }
    
    // Update banner visibility
    const newMissingRows = this.rowsWithMissingRequired();
    this.showMissingRequiredBanner.set(newMissingRows.length > 0);
  }

  // Check for missing required fields in all rows
  checkMissingRequiredFields(): void {
    const allData = this.importDialogService.data();
    const rowsWithMissing: number[] = [];
    
    allData.forEach((row, index) => {
      const missingFields = this.columns
        .filter(col => col.required && !row[col.field])
        .map(col => col.field);
        
      if (missingFields.length > 0) {
        rowsWithMissing.push(index);
      }
    });
    
    // Update the list of rows with missing required fields
    this.rowsWithMissingRequired.set(rowsWithMissing);
    
    // Update banner visibility based on whether any rows have missing fields
    this.showMissingRequiredBanner.set(rowsWithMissing.length > 0);
  }

  // Get mandatory fields information for the current entity type
  getMandatoryFieldsInfo(): string {
    const importType = this.currentImportType();
    const requiredFields = this.columns
      .filter(col => col.required)
      .map(col => this.getTranslatedHeader(col.header))
      .join(', ');

    switch (importType) {
      case 'Contact':
        return `Mandatory fields for contacts: ${requiredFields}`;
      case 'Partner':
        return `Mandatory fields for partners: ${requiredFields}`;
      case 'Interaction':
        return `Mandatory fields for interactions: ${requiredFields}`;
      default:
        return `Mandatory fields: ${requiredFields}`;
    }
  }

  // Check if we should show the mandatory fields info banner
  shouldShowMandatoryFieldsInfo(): boolean {
    return this.columns.some(col => col.required) && this.importDialogService.data().length > 0;
  }

  // Force refresh of the data view
  refreshData(): void {
    const currentData = this.importDialogService.data();
    
    if (currentData && currentData.length > 0) {
      // Reset to first page
      this.first.set(0);
      
      // Clear and rebuild existing data
      this.checkAndProcessData();
      
      // Clear selections and reselect valid rows
      this.selectedRows.set([]);
      
      // Update paginated data with the latest data
      this.updatePaginatedData();
      
      // Check for missing required fields and deselect problematic rows
      this.checkMissingRequiredFields();
    } else {
      // Reset everything if no data
      this.totalRecords.set(0);
      this.paginatedData.set([]);
      this.selectedRows.set([]);
      this.rowsWithMissingRequired.set([]);
      this.showMissingRequiredBanner.set(false);
    }
  }

  // Select only valid rows (exclude rows with errors, missing required fields, or duplicates)
  selectValidRows(): void {
    const allData = this.importDialogService.data();
    
    if (!allData || allData.length === 0) {
      console.warn('No data to select from');
      return;
    }
    
    // Get rows with missing required fields
    const missingRequiredRows = this.rowsWithMissingRequired();
    
    // Get rows with validation errors
    const validationErrorRows = Array.from(this.validationErrors().keys());
    
    // Filter out rows with either missing required fields, validation errors, or duplicates
    const validRows = allData.filter((row, index) => {
      const hasMissingRequired = missingRequiredRows.includes(index);
      const hasValidationError = validationErrorRows.includes(index);
      const isDuplicate = row.duplicateInfo?.isDuplicate;
      
      return !hasMissingRequired && !hasValidationError && !isDuplicate;
    });
    
    // Update the local selection state
    this.selectedRows.set(validRows);
    
    // Update the service with the selected rows
    this.importDialogService.setSelectedRows(validRows);
  }

  // Check if current import is a user role import
  isUserRoleImport(): boolean {
    return this.currentImportType().toLowerCase() === 'user_role_import';
  }

  /**
   * Enrich user role import data with user names and role names
   */
  async enrichUserRoleData(data: any[]): Promise<any[]> {
    if (!this.isUserRoleImport() || !data.length) {
      return data;
    }

    try {
      // Extract unique user IDs and role IDs
      const userIds = new Set<number>();
      const roleIds = new Set<number>();

      data.forEach(record => {
        if (record.userId && !isNaN(Number(record.userId))) {
          userIds.add(Number(record.userId));
        }
        if (record.roleIds && Array.isArray(record.roleIds)) {
          record.roleIds.forEach((roleId: any) => {
            if (!isNaN(Number(roleId))) {
              roleIds.add(Number(roleId));
            }
          });
        }
      });

      // Fetch user and role data in parallel
      const [userLookup, roleLookup] = await Promise.all([
        userIds.size > 0 ? this.userManagementService.resolveUserIds(Array.from(userIds)) : Promise.resolve({}),
        roleIds.size > 0 ? this.userManagementService.resolveRoleIds(Array.from(roleIds)) : Promise.resolve({})
      ]);

      // Enrich the data
      return data.map(record => {
        const enrichedRecord = { ...record };
        
        // Enrich user information
        if (record.userId && (userLookup as any)[record.userId]) {
          const userInfo = (userLookup as any)[record.userId];
          enrichedRecord.userDisplay = userInfo.name || userInfo.email || `User ${record.userId}`;
          enrichedRecord.userEmail = userInfo.email;
          enrichedRecord.userName = userInfo.name;
        } else {
          enrichedRecord.userDisplay = `User ${record.userId}`;
        }

        // Enrich role information
        if (record.roleIds && Array.isArray(record.roleIds)) {
          enrichedRecord.roleNames = record.roleIds.map((roleId: any) => {
            const roleInfo = (roleLookup as any)[roleId];
            return roleInfo ? roleInfo.name : `Role ${roleId}`;
          });
          enrichedRecord.roleDisplay = enrichedRecord.roleNames.join(', ');
        } else {
          enrichedRecord.roleNames = [];
          enrichedRecord.roleDisplay = '';
        }

        return enrichedRecord;
      });
    } catch (error) {
      console.error('Error enriching user role data:', error);
      // Return original data if enrichment fails
      return data;
    }
  }

  /**
   * Get role severity for styling (same as user management component)
   */
  getRoleSeverity(role: string): string {
    switch (role) {
      case 'PARTNER_GLOB_ADMIN':
        return 'danger';
      case 'ORG_UNIT_ADMIN':
        return 'warning';
      case 'PARTNER_USER':
        return 'info';
      default:
        return 'secondary';
    }
  }


}
