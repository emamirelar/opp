import { ChangeDetectionStrategy, Component, effect, inject, OnInit, signal, computed, Type } from '@angular/core';
import { DynamicDialogConfig } from 'primeng/dynamicdialog';
import { TranslateModule } from '@ngx-translate/core';
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
import { ComponentResolverService } from '../../../../../features/internal/services/component-resolver.service';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';
import { ImportService } from '../import.service';

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
    TitleCasePipe
  ],
  templateUrl: './import-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportDialogComponent implements OnInit {
  feedbackDialogService = inject(FeedbackDialogService);
  importDialogService = inject(ImportDialogService);
  componentResolverService = inject(ComponentResolverService);
  importService = inject(ImportService);
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
    { field: 'duplicateInfo', header: 'Duplicate', required: false, label: 'Duplicate', type: 'text', sortable: false },
    { field: 'salutation', header: 'Salutation', required: false, label: 'Salutation', type: 'text', sortable: false },
    { field: 'firstName', header: 'First Name', required: false, label: 'First Name', type: 'text', sortable: false },
    { field: 'middleName', header: 'Middle Name', required: false, label: 'Middle Name', type: 'text', sortable: false },
    { field: 'lastName', header: 'Last Name', required: true, label: 'Last Name', type: 'text', sortable: false },
    { field: 'suffix', header: 'Suffix', required: false, label: 'Suffix', type: 'text', sortable: false },
    { field: 'title', header: 'Title', required: false, label: 'Title', type: 'text', sortable: false },
    { field: 'pronouns', header: 'Pronouns', required: false, label: 'Pronouns', type: 'text', sortable: false },
    { field: 'birthDate', header: 'Birth Date', required: false, label: 'Birth Date', type: 'text', sortable: false },
    { field: 'partnerId', header: 'Partner ID', required: true, label: 'Partner ID', type: 'text', sortable: false },
    { field: 'email', header: 'Email', required: true, label: 'Email', type: 'text', sortable: false },
    { field: 'phone', header: 'Phone', required: false, label: 'Phone', type: 'text', sortable: false },
    { field: 'mobile', header: 'Mobile', required: false, label: 'Mobile', type: 'text', sortable: false },
    { field: 'otherPhone', header: 'Other Phone', required: false, label: 'Other Phone', type: 'text', sortable: false },
    { field: 'fax', header: 'Fax', required: false, label: 'Fax', type: 'text', sortable: false },
    { field: 'department', header: 'Department', required: false, label: 'Department', type: 'text', sortable: false },
    { field: 'description', header: 'Description', required: false, label: 'Description', type: 'text', sortable: false },
    { field: 'status', header: 'Status', required: false, label: 'Status', type: 'text', sortable: false },
    { field: 'contactNumber', header: 'Contact Number', required: false, label: 'Contact Number', type: 'text', sortable: false },
    { field: 'assistant', header: 'Assistant', required: false, label: 'Assistant', type: 'text', sortable: false },
    { field: 'assistantPhone', header: 'Assistant Phone', required: false, label: 'Assistant Phone', type: 'text', sortable: false },
    { field: 'assistantEmail', header: 'Assistant Email', required: false, label: 'Assistant Email', type: 'text', sortable: false },
    { field: 'mailingStreet', header: 'Mailing Street', required: false, label: 'Mailing Street', type: 'text', sortable: false },
    { field: 'mailingStreet2', header: 'Mailing Street 2', required: false, label: 'Mailing Street 2', type: 'text', sortable: false },
    { field: 'mailingCity', header: 'Mailing City', required: false, label: 'Mailing City', type: 'text', sortable: false },
    { field: 'mailingStateProvince', header: 'Mailing State/Province', required: false, label: 'Mailing State/Province', type: 'text', sortable: false },
    { field: 'mailingPostalCode', header: 'Mailing Postal Code', required: false, label: 'Mailing Postal Code', type: 'text', sortable: false },
    { field: 'mailingCountry', header: 'Mailing Country', required: false, label: 'Mailing Country', type: 'text', sortable: false },
  ];

  // Partner-specific columns
  partnerColumns: ImportColumn[] = [
    { field: 'duplicateInfo', header: 'Duplicate', required: false, label: 'Duplicate', type: 'text', sortable: false },
    { field: 'name', header: 'Name', required: true, label: 'Name', type: 'text', sortable: false },
    { field: 'partnerShortDescription', header: 'Short Name', required: true, label: 'Short Name', type: 'text', sortable: false },
    { field: 'partnerLongDescription', header: 'Long Description', required: false, label: 'Long Description', type: 'text', sortable: false },
    { field: 'status', header: 'Status', required: false, label: 'Status', type: 'text', sortable: false },
    { field: 'canCreateNewOpportunities', header: 'New Engagement', required: true, label: 'New Engagement', type: 'text', sortable: false },
    { field: 'pooledFund', header: 'Pooled Fund', required: true, label: 'Pooled Fund', type: 'text', sortable: false },
    { field: 'dueDiligenceRequired', header: 'DD Required', required: true, label: 'DD Required', type: 'text', sortable: false },
    { field: 'dueDiligenceApproval', header: 'DD Approval', required: true, label: 'DD Approval', type: 'text', sortable: false },
    { field: 'partnerLevyStatus', header: 'Levy Status', required: true, label: 'Levy Status', type: 'text', sortable: false },
    { field: 'keyGlobalPartner', header: 'Key Global', required: false, label: 'Key Global', type: 'text', sortable: false },
    { field: 'unSecretariatPartner', header: 'UN Secretariat', required: false, label: 'UN Secretariat', type: 'text', sortable: false },
    { field: 'unAndStateEntity', header: 'UN State Entity', required: false, label: 'UN State Entity', type: 'text', sortable: false },
    { field: 'reasonForNoNewOpportunity', header: 'Reason No New Opportunity', required: false, label: 'Reason No New Opportunity', type: 'text', sortable: false },
    { field: 'levyTreatment', header: 'Levy Treatment', required: false, label: 'Levy Treatment', type: 'text', sortable: false },
    { field: 'partnerGroupCode', header: 'Partner Group', required: false, label: 'Partner Group', type: 'text', sortable: false },
  ];

  // Interaction-specific columns
  interactionColumns: ImportColumn[] = [
    { field: 'duplicateInfo', header: 'Duplicate', required: false, label: 'Duplicate', type: 'text', sortable: false },
    { field: 'type', header: 'Type', required: true, label: 'Type', type: 'text', sortable: false },
    { field: 'date', header: 'Date', required: true, label: 'Date', type: 'text', sortable: false },
    { field: 'subject', header: 'Subject', required: true, label: 'Subject', type: 'text', sortable: false },
    { field: 'description', header: 'Description', required: false, label: 'Description', type: 'text', sortable: false },
    { field: 'contactId', header: 'Contact', required: false, label: 'Contact', type: 'text', sortable: false },
    { field: 'location', header: 'Location', required: false, label: 'Location', type: 'text', sortable: false },
    { field: 'contactIds', header: 'Contact IDs', required: false, label: 'Contact IDs', type: 'text', sortable: false },
    { field: 'partnerIds', header: 'Partner IDs', required: false, label: 'Partner IDs', type: 'text', sortable: false },
    { field: 'userIds', header: 'User IDs', required: false, label: 'User IDs', type: 'text', sortable: false },
    { field: 'emailAddresses', header: 'Email Addresses', required: false, label: 'Email Addresses', type: 'text', sortable: false },
    { field: 'phoneNumbers', header: 'Phone Numbers', required: false, label: 'Phone Numbers', type: 'text', sortable: false },
    { field: 'organizationHierarchyIds', header: 'Organization Unit IDs', required: false, label: 'Organization Unit IDs', type: 'text', sortable: false }
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

  ngOnInit(): void {
    // Set the table columns based on the current import type
    this.updateColumnsForEntityType();
    
    // Immediately check data on init
    this.checkAndProcessData();
  }

  // Update the table columns based on the current import type
  private updateColumnsForEntityType(): void {
    const entityType = this.importDialogService.getImportType().toLowerCase();
    
    if (entityType === 'partner') {
      this.columns = this.partnerColumns;
    } else if (entityType === 'interaction') {
      this.columns = this.interactionColumns;
    } else {
      // Default to contact columns
      this.columns = this.contactColumns;
    }
  }
  
  // Check and process data - can be called multiple times if needed
  checkAndProcessData(): void {
    // Use the explicitly set import type from the service
    const entityType = this.importDialogService.getImportType();
    
    
    // Update columns for the current entity type
    this.updateColumnsForEntityType();
    
    // Process the data
    const initialData = this.importDialogService.data();
    
    if (initialData && initialData.length > 0) {
      // Add a unique non-conflicting ID to each row for selection purposes
      const processedData = initialData.map((item, index) => {
        return { ...item, _importRowId: `import-${index}` };
      });
      
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
      
      // Check if record has similarityEntityId (duplicate detected)
      // Also check for similarityEntityId in different case variations
      const hasDuplicate = record.similarityEntityId;
      
      if (hasDuplicate) {
        const entityId = record.similarityEntityId;
        
        const similarityScore = record.similarityScore || 0;
        
        const similarityPercentage = Math.round(similarityScore * 100);
        
        // Create duplicate info with link
        const entityType = this.getEntityTypeFromImportType();
        const entityUrl = this.getEntityUrl(entityType, entityId);
        
        record.duplicateInfo = {
          isDuplicate: true,
          entityId: entityId,
          similarityScore: similarityScore,
          similarityPercentage: similarityPercentage,
          entityUrl: entityUrl,
          tooltip: `Duplicate found (${similarityPercentage}% similarity)`
        };
        
        duplicateRows.push(record);
      } else {
        // No duplicate found
        record.duplicateInfo = {
          isDuplicate: false,
          tooltip: 'Unique record'
        };
        nonDuplicateRows.push(record);
      }
    });

    // Update signals
    this.duplicateRows.set(duplicateRows);
    this.nonDuplicateRows.set(nonDuplicateRows);
    
    console.log(`Duplicate detection complete: ${duplicateRows.length} duplicates, ${nonDuplicateRows.length} unique records`);
    console.log('Duplicate rows:', duplicateRows);
    console.log('Non-duplicate rows:', nonDuplicateRows);
    
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
  private getEntityTypeFromImportType(): string {
    const importType = this.currentImportType();
    switch (importType) {
      case 'contact':
        return 'contacts';
      case 'partner':
        return 'partners';
      case 'interaction':
        return 'interactions';
      default:
        return 'contacts';
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
}
