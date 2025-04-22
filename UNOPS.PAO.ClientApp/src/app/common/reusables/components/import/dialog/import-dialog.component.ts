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
import { NgForOf, NgClass, JsonPipe, TitleCasePipe } from '@angular/common';
import { ImportDialogService } from './import-dialog.service';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule, CheckboxChangeEvent } from 'primeng/checkbox';
import { ComponentResolverService } from '../../../../../features/internal/services/component-resolver.service';
import { ContactEditDialogComponent } from '../../../../../features/internal/components/contact/edit-dialog/contact-edit-dialog.component';
import { ContactEditDialogFooterComponent } from '../../../../../features/internal/components/contact/edit-dialog/footer/contact-edit-dialog-footer.component';
import { Contact } from '../../../../../features/internal/models/contact.model';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';
import { PartnerEditDialogComponent } from '../../../../../features/internal/components/partner/edit-dialog/partner-edit-dialog.component';
import { PartnerEditDialogFooterComponent } from '../../../../../features/internal/components/partner/edit-dialog/footer/partner-edit-dialog-footer.component';

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
    NgForOf,
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
  // Make Math available to the template
  Math = Math;

  // Current import type being displayed
  currentImportType = computed(() => this.importDialogService.getImportType());

  errorMessage = signal<string>('');
  selectedRows = signal<any[]>([]);
  validationErrors = signal<Map<number, string[]>>(new Map());
  rowsWithMissingRequired = signal<number[]>([]);
  showMissingRequiredBanner = signal<boolean>(false);

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
    { field: 'name', header: 'Name', required: true, label: 'Name', type: 'text', sortable: false },
    { field: 'shortName', header: 'Short Name', required: true, label: 'Short Name', type: 'text', sortable: false },
    { field: 'status', header: 'Status', required: false, label: 'Status', type: 'text', sortable: false },
    { field: 'newEngagement', header: 'New Engagement', required: true, label: 'New Engagement', type: 'text', sortable: false },
    { field: 'phone', header: 'Phone', required: false, label: 'Phone', type: 'text', sortable: false },
    { field: 'website', header: 'Website', required: false, label: 'Website', type: 'text', sortable: false },
    { field: 'pooledFund', header: 'Pooled Fund', required: true, label: 'Pooled Fund', type: 'text', sortable: false },
    { field: 'ddRequired', header: 'DD Required', required: true, label: 'DD Required', type: 'text', sortable: false },
    { field: 'ddeacDone', header: 'DDEAC Done', required: true, label: 'DDEAC Done', type: 'text', sortable: false },
    { field: 'eacReference', header: 'EAC Reference', required: false, label: 'EAC Reference', type: 'text', sortable: false },
    { field: 'globalKeyAccount', header: 'Global Key Account', required: false, label: 'Global Key Account', type: 'text', sortable: false },
    { field: 'unSecretariatEntity', header: 'UN Secretariat Entity', required: false, label: 'UN Secretariat Entity', type: 'text', sortable: false },
    { field: 'levyPotentiallyApplies', header: 'Levy Potentially Applies', required: true, label: 'Levy Potentially Applies', type: 'text', sortable: false },
    { field: 'reasonForLevyNotApplying', header: 'Reason For Levy Not Applying', required: false, label: 'Reason For Levy Not Applying', type: 'text', sortable: false },
    { field: 'levyTreatment', header: 'Levy Treatment', required: false, label: 'Levy Treatment', type: 'text', sortable: false },
    { field: 'address1Street', header: 'Street', required: false, label: 'Street', type: 'text', sortable: false },
    { field: 'address1Street2', header: 'Street 2', required: false, label: 'Street 2', type: 'text', sortable: false },
    { field: 'address1City', header: 'City', required: false, label: 'City', type: 'text', sortable: false },
    { field: 'address1StateProvince', header: 'State/Province', required: false, label: 'State/Province', type: 'text', sortable: false },
    { field: 'address1PostalCode', header: 'Postal Code', required: false, label: 'Postal Code', type: 'text', sortable: false },
    { field: 'address1Country', header: 'Country', required: false, label: 'Country', type: 'text', sortable: false },
  ];

  ngOnInit(): void {
    // Set the table columns based on the current import type
    this.updateColumnsForEntityType();
    
    // Immediately check data on init
    this.checkAndProcessData();
    
    // Setup effect to update paginated data when data changes
    effect(() => {
      const allData = this.importDialogService.data();
      console.log('Data changed in ImportDialogComponent effect:', allData);
      
      if (allData && allData.length > 0) {
        console.log(`Setting totalRecords to ${allData.length}`);
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

  // Update the table columns based on the current import type
  private updateColumnsForEntityType(): void {
    const entityType = this.importDialogService.getImportType().toLowerCase();
    
    if (entityType === 'partner') {
      this.columns = this.partnerColumns;
    } else {
      // Default to contact columns
      this.columns = this.contactColumns;
    }
  }
  
  // Check and process data - can be called multiple times if needed
  checkAndProcessData(): void {
    // Update columns for the current entity type
    this.updateColumnsForEntityType();
    
    // Log initial state
    const initialData = this.importDialogService.data();
    console.log('Processing data in ImportDialogComponent:', initialData);
    
    if (initialData && initialData.length > 0) {
      // Add a unique non-conflicting ID to each row for selection purposes
      const processedData = initialData.map((item, index) => {
        return { ...item, _importRowId: `import-${index}` };
      });
      
      // Update the data in the service with the processed data
      this.importDialogService.setData(processedData);
      
      console.log(`Setting totalRecords to ${processedData.length}`);
      this.totalRecords.set(processedData.length);
      this.updatePaginatedData();
      
      // Check for missing required fields
      this.checkMissingRequiredFields();
      
      // If no data was displayed, try forcing detection
      if (this.paginatedData().length === 0) {
        console.log('Forcing update of paginated data');
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
    
    console.log(`Updating paginated data: data.length=${allData?.length}, firstIndex=${firstIndex}, rowsPerPage=${rowsPerPage}`);
    
    if (!allData || allData.length === 0) {
      console.warn('No data available for pagination');
      this.paginatedData.set([]);
      this.totalRecords.set(0);
      this.selectedRows.set([]);
      return;
    }

    // Update total records if it doesn't match the data length
    if (this.totalRecords() !== allData.length) {
      console.log(`Fixing totalRecords: ${this.totalRecords()} → ${allData.length}`);
      this.totalRecords.set(allData.length);
    }
    
    // Ensure firstIndex doesn't exceed the bounds of the data
    if (firstIndex >= allData.length) {
      const newFirstIndex = 0;
      console.warn(`First index ${firstIndex} exceeds data length ${allData.length}, resetting to ${newFirstIndex}`);
      this.first.set(newFirstIndex);
      
      const newPaginatedResult = allData.slice(newFirstIndex, newFirstIndex + rowsPerPage);
      console.log(`Paginating data (reset): ${newFirstIndex} to ${Math.min(newFirstIndex + rowsPerPage, allData.length)}, result length: ${newPaginatedResult.length}`);
      this.paginatedData.set(newPaginatedResult);
      return;
    }
    
    // Normal pagination
    const endIndex = Math.min(firstIndex + rowsPerPage, allData.length);
    const paginatedResult = allData.slice(firstIndex, endIndex);
    console.log(`Paginating data: ${firstIndex} to ${endIndex}, result length: ${paginatedResult.length}, total: ${allData.length}`);
    
    this.paginatedData.set(paginatedResult);
  }

  onPageChange(event: any): void {
    console.log('Page change event:', event);
    
    // Verify the event has expected properties
    if (!event || typeof event.first !== 'number' || typeof event.rows !== 'number') {
      console.error('Invalid page change event:', event);
      return;
    }
    
    // Verify that we're not exceeding data bounds
    const dataLength = this.importDialogService.data().length;
    if (event.first >= dataLength) {
      console.warn(`Invalid page: first index (${event.first}) exceeds data length (${dataLength})`);
      event.first = 0;
    }
    
    // Store current selection before changing page
    const currentSelection = this.selectedRows();
    
    // Update pagination values
    this.first.set(event.first);
    this.rows.set(event.rows);
    this.rowsModel = event.rows;
    
    console.log(`Page changed to: first=${this.first()}, rows=${this.rows()}`);
    
    // Update paginated data (which now preserves selection)
    this.updatePaginatedData();
    
    // Log the results after update
    setTimeout(() => {
      console.log(`After page change: ${this.paginatedData().length} items in current page, ${currentSelection.length} selected overall`);
    }, 0);
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

  // Select all rows in the current dataset (including rows with missing required fields)
  selectAllRows(): void {
    const allData = this.importDialogService.data();
    
    if (!allData || allData.length === 0) {
      console.warn('No data to select from');
      return;
    }
    
    // Select ALL rows, including those with missing required fields
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
    
    console.log(`Selected all rows: ${newSelection.length} total rows, including ${missingRequiredRows.length} with missing fields`);
  }

  // Get the currently selected rows (used for import)
  getSelectedRowsForImport(): any[] {
    const selectedData = this.selectedRows();
    console.log('Returning selected rows for import:', selectedData.length);
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
    
    console.log(`Toggle select all: ${this.selectedRows().length} rows now selected`);
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
    console.log('Selection changed:', event.length, 'rows selected on current page');
    
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
    
    // Create a new selection by combining:
    // 1. Rows selected from other pages (not visible on current page)
    // 2. Rows selected on the current page from the event
    const newSelection = [
      ...selectionsFromOtherPages,
      ...event
    ];
    
    // Avoid duplicates by creating a unique set based on _importRowId
    const uniqueSelection = [...new Map(newSelection.map(item => 
      [item._importRowId, item]
    )).values()];
    
    // Update the selection state
    this.selectedRows.set(uniqueSelection);
    
    // Update the service with the selected rows for import
    this.importDialogService.setSelectedRows(uniqueSelection);
    
    console.log(`Selection updated: ${uniqueSelection.length} rows selected in total`);
    
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
    
    // Determine which edit dialog to use based on the entity type
    let dialogComponent: any;
    let dialogFooterComponent: any;
    let dialogHeader: string;
    
    const entityType = this.importDialogService.getImportType().toLowerCase();
    
    switch (entityType) {
      case 'partner':
        // Use the partner edit dialog
        dialogComponent = PartnerEditDialogComponent;
        dialogFooterComponent = PartnerEditDialogFooterComponent;
        dialogHeader = 'Edit Partner Import Data';
        break;
      
      case 'contact':
      default:
        // Use the contact edit dialog as default
        dialogComponent = ContactEditDialogComponent;
        dialogFooterComponent = ContactEditDialogFooterComponent;
        dialogHeader = 'Edit Contact Import Data';
        break;
    }
    
    // Open the appropriate edit dialog
    const dialogRef = this.componentResolverService.dialogService.open(
      dialogComponent, 
      {
        header: dialogHeader,
        width: '40vw',
        breakpoints: { '960px': '95vw' },
        closable: true,
        templates: {
          footer: dialogFooterComponent
        },
        data: {
          mode: 'edit',
          record: rowCopy,
          requestingSaveSignal: signal<boolean>(false)
        }
      }
    );
    
    // Subscribe to dialog close events to update the row data
    dialogRef.onClose.subscribe((result: any) => {
      if (result && result._updated) {
        // Update the row data in the table with the edited data
        delete result._updated; // Remove the temporary flag
        delete result.isImportEdit; // Remove the import edit flag
        
        // Update the original row data while preserving _importRowId
        const importRowId = row._importRowId;
        Object.assign(row, result);
        row._importRowId = importRowId;
        
        // Also update the data in the service
        const allData = this.importDialogService.data();
        const rowIndex = allData.findIndex(item => item._importRowId === importRowId);
        
        // Update the data array
        const updatedData = allData.map(item => {
          if (item._importRowId === importRowId) {
            return { ...result, _importRowId: importRowId };
          }
          return item;
        });
        
        // Update data in service
        this.importDialogService.setData(updatedData);
        
        // Check if the updated row still has missing required fields
        const missingFields = this.columns
          .filter(col => {
            return col.required && !result[col.field];
          })
          .map(col => col.field);
        
        if (missingFields.length > 0) {
          // Still has missing required fields, update the rowsWithMissingRequired
          const currentMissingRows = this.rowsWithMissingRequired();
          if (!currentMissingRows.includes(rowIndex)) {
            // Add this row to the missing required rows
            this.rowsWithMissingRequired.set([...currentMissingRows, rowIndex]);
          }
          
          // Show a warning about still missing fields
          this.feedbackDialogService.showWarningToast({
            detail: `Row updated but still missing required fields: ${missingFields.join(', ')}`,
            life: 5000
          });
        } else {
          // No missing required fields for this row, remove from rowsWithMissingRequired if it was there
          const currentMissingRows = this.rowsWithMissingRequired();
          if (currentMissingRows.includes(rowIndex)) {
            // Remove this row from the missing required rows
            this.rowsWithMissingRequired.set(currentMissingRows.filter(index => index !== rowIndex));
          }
          
          // Show success message
          this.feedbackDialogService.showSuccessToast({ 
            detail: 'Row updated successfully' 
          });
        }
        
        // Update banner visibility
        const newMissingRows = this.rowsWithMissingRequired();
        this.showMissingRequiredBanner.set(newMissingRows.length > 0);
      }
    });
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
    console.log(`Missing required fields check: ${rowsWithMissing.length} rows with missing fields`);
  }

  // Force refresh of the data view
  refreshData(): void {
    const currentData = this.importDialogService.data();
    console.log('Refreshing data view with', currentData?.length || 0, 'records');
    
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
}
