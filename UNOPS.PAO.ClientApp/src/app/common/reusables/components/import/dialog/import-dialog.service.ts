import { Injectable, inject, signal } from '@angular/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ImportDialogComponent } from './import-dialog.component';
import { ImportFooterComponent } from './footer/import-dialog-footer.component';
import { Observable, Subject, forkJoin, of, timer } from 'rxjs';
import { WritableSignal } from '@angular/core';
import { ImportService } from '../import.service';
import { FeedbackDialogService } from '../../../../pages/services/feedback-dialog.service';
import { ImportGoogleSheetService } from '../import-google-sheet.service';
import { catchError, finalize, mergeMap, map, timeout } from 'rxjs/operators';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../../../services/notification.service';
import { LoadingOverlayService } from '../../../components/loading-overlay/loading-overlay.component';
import { ManualEntryDialogComponent } from './manual-entry/manual-entry-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ImportDialogService {
  private dialogService = inject(DialogService);
  private dialogRef: DynamicDialogRef | null = null;
  private _onClose = new Subject<any>();
  private notificationService = inject(NotificationService);
  private confirmationService = inject(ConfirmationService);
  private loadingOverlayService = inject(LoadingOverlayService);

  // Notification info
  private notificationId: number | null = null;
  private userId: string | null = null;
  private notificationMessage: string | null = null;

  data = signal<Array<any>>([])
  importErrors = signal<Map<number, any>>(new Map()); // Store import error details by row index

  isLoading = signal(false);

  // Define a reasonable batch size for bulk operations
  private batchSize = 100;

  private _fileUrl = signal<string>('');
  importService = inject(ImportService);
  feedbackDialogService = inject(FeedbackDialogService);
  importGoogleSheetService = inject(ImportGoogleSheetService);

  // New signal to track selected rows for import
  selectedRows = signal<Array<any>>([]);

  // Track the current import type
  private currentImportType: string | null = null;

  setNotificationInfo(notificationId: number, userId: string, message: string): void {
    this.notificationId = notificationId;
    this.userId = userId;
    this.notificationMessage = message;
  }

  clearNotificationInfo(): void {
    this.notificationId = null;
    this.userId = null;
    this.notificationMessage = null;
  }

  markNotificationAsRead(): void {
    if (this.notificationId && this.userId) {
      this.notificationService.markAsRead(this.notificationId, this.userId).subscribe({
        next: () => {
          this.clearNotificationInfo();
        },
        error: (error) => {
          console.error('Error marking notification as read:', error);
        }
      });
    }
  }

  openImportDialog(header: string = 'Import'): Observable<any> {
    // Check if we have data before opening the dialog
    const currentData = this.data();
    if (!currentData || currentData.length === 0) {
      console.warn('Attempting to open import dialog with no data');
      this.feedbackDialogService.showWarningToast({ 
        detail: 'No data available to import' 
      });
    }
    
    // Update header with record count if from notification
    let dialogHeader = header;
    if (this.notificationMessage && currentData && currentData.length > 0) {
      dialogHeader = `${header} - ${currentData.length} records ready`;
    }
    
    this.dialogRef = this.dialogService.open(ImportDialogComponent, {
      header: dialogHeader,
      width: '90vw',
      height: '100vh',
      closable: true,
      templates: {
        footer: ImportFooterComponent
      }
    });

    // Clear previous subscribers
    this._onClose = new Subject<any>();

    // Subscribe to dialog close and forward the result
    this.dialogRef.onClose.subscribe(result => {
      this._onClose.next(result);
      this._onClose.complete();
      this.dialogRef = null;
    });

    return this._onClose.asObservable();
  }

  /**
   * Close the dialog with an optional result
   */
  closeDialog(result?: any): void {
    if (this.dialogRef) {
      this.dialogRef.close(result);
    }
    // Clear import errors when dialog is closed to prevent state leakage
    this.clearImportErrorDetails();
    console.log('🧹 Cleared import errors on dialog close');
  }

  /**
   * Handle cancel with confirmation
   */
  cancelImport(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel this operation? All data will be discarded.',
      header: 'Cancel Operation',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Check if there's an active file analysis to cancel
        if (this.importService.getActiveJobId()) {
          this.cancelFileAnalysis();
          return;
        }
        
        // If from notification, mark as read
        if (this.notificationId) {
          this.markNotificationAsRead();
        }
        
        // Always refresh on cancel in case some records were already imported
        window.dispatchEvent(new CustomEvent('refresh-listview'));
        
        this.closeDialog('canceled');
        this.data.set([]);
        this.feedbackDialogService.showInfoToast({ 
          detail: 'Operation canceled'
        });
      }
    });
  }

  /**
   * Cancel an active file analysis that's being processed asynchronously
   */
  cancelFileAnalysis(): void {
    this.isLoading.set(true);
    this.loadingOverlayService.show('Cancelling file analysis...');
    
    this.importService.cancelAnalysis().subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        
        // If from notification, mark as read
        if (this.notificationId) {
          this.markNotificationAsRead();
        }
        
        this.closeDialog('canceled');
        this.data.set([]);
        this.selectedRows.set([]);
        
        this.feedbackDialogService.showInfoToast({ 
          detail: 'File analysis canceled successfully'
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        this.feedbackDialogService.showErrorToast({ 
          detail: 'Error cancelling file analysis: ' + (error.message || 'Unknown error')
        });
      }
    });
  }

  /**
   * Get the file URL signal
   */
  getFileUrl(): WritableSignal<string> {
    return this._fileUrl;
  }

  /**
   * Try to parse a string value that might be JSON
   */
  private tryParseJson(jsonString: string): any[] {
    try {
      const parsed = JSON.parse(jsonString);
      if (Array.isArray(parsed)) {
        return parsed;
      } else if (typeof parsed === 'object' && parsed !== null) {
        return [parsed];
      }
    } catch (e) {
      console.error('Failed to parse JSON string:', e);
    }
    return [];
  }

  /**
   * Normalize record data from notifications to ensure it's in the correct format
   */
  private normalizeRecordData(inputData: any): any[] {
    // Special case: if inputData is an object with numeric keys (looks like an array but is an object)
    if (typeof inputData === 'object' && inputData !== null && !Array.isArray(inputData)) {
      const keys = Object.keys(inputData);
      if (keys.length > 0 && keys.every(key => !isNaN(Number(key)))) {
        return Object.values(inputData);
      }
    }
    
    // If it's already an array with elements, use it directly
    if (Array.isArray(inputData) && inputData.length > 0) {
      // Check if the array items themselves need processing
      if (inputData.length === 1) {
        const item = inputData[0];
        
        // If the item is a string, try to parse it as JSON
        if (typeof item === 'string') {
          const parsedResult = this.tryParseJson(item);
          if (parsedResult.length > 0) {
            return parsedResult;
          }
        }
        
        // If the item has a 'records' property
        if (typeof item === 'object' && item !== null && 'records' in item) {
          const records = item.records;
          if (typeof records === 'string') {
            const parsedResult = this.tryParseJson(records);
            if (parsedResult.length > 0) {
              return parsedResult;
            }
          } else if (Array.isArray(records) && records.length > 0) {
            return records;
          }
        }
      }
      
      // If we get here, just return the input array
      return inputData;
    }
    
    // If it's a string, try to parse it
    if (typeof inputData === 'string') {
      const parsedResult = this.tryParseJson(inputData);
      if (parsedResult.length > 0) {
        return parsedResult;
      }
    }
    
    // If it's an object but not an array, wrap it in an array
    if (typeof inputData === 'object' && inputData !== null) {
      return [inputData];
    }
    
    // Default: return empty array for null/undefined or wrap primitive values
    return inputData ? [inputData] : [];
  }

  // Set the selected rows
  setSelectedRows(rows: any[]): void {
    this.selectedRows.set(rows);
  }

  // Get the selected rows for import
  getSelectedRowsForImport(): any[] {
    const selected = this.selectedRows();
    // Only return selected rows, don't fall back to all data
    return selected || [];
  }

  /**
   * Set the current import type
   */
  setImportType(type: string): void {
    this.currentImportType = type;
  }

  /**
   * Get the current import type
   */
  getImportType(): string {
    return this.currentImportType || 'contact';
  }

  /**
   * Apply default values to all records before import
   * This ensures all fields have a value (default or user-provided)
   */
  private applyDefaultValuesToRecords(records: any[]): any[] {
    if (!records || records.length === 0) {
      return [];
    }

    // Get default values based on the import type
    const type = this.currentImportType || 'contact';
    const defaultValues = this.getDefaultValuesForType(type);
    
    // Apply defaults to each record
    return records.map(record => {
      // Create a new object with default values for any fields not in the record
      return {
        ...defaultValues,  // Start with all default values
        ...record          // Override with values from the record
      };
    });
  }

  /**
   * Get default values for the specified entity type
   */
  private getDefaultValuesForType(type: string): Partial<any> {
    switch (type.toLowerCase()) {
      case 'partner':
        return this.getDefaultPartnerValues();
      case 'interaction':
        return this.getDefaultInteractionValues();
      case 'contact':
      default:
        return this.getDefaultContactValues();
    }
  }

  /**
   * Get default values for all Contact fields
   */
  private getDefaultContactValues(): Partial<any> {
    return {
      salutation: '',
      firstName: '',
      middleName: '',
      lastName: null,
      suffix: '',
      title: '',
      pronouns: '',
      birthDate: null,
      partner: null,
      email: null,
      phone: '',
      mobile: '',
      otherPhone: '',
      fax: '',
      department: '',
      description: '',
      status: 'Active',
      contactNumber: '',
      assistant: '',
      assistantPhone: '',
      assistantEmail: '',
      mailingStreet: '',
      mailingStreet2: '',
      mailingCity: '',
      mailingStateProvince: '',
      mailingPostalCode: '',
      mailingCountry: ''
    };
  }

  /**
   * Get default values for all Partner fields
   */
  private getDefaultPartnerValues(): Partial<any> {
    return {
      name: '',
      partnerShortDescription: '',
      partnerLongDescription: '',
      status: 'Draft',
      partnerApprovalStatus: 'NotApproved',
      partnerCategoryId: null,
      liaisonOfficeId: null,
      partnerFocalPointUserId: null,
    };
  }

  /**
   * Get default values for all Interaction fields
   */
  private getDefaultInteractionValues(): Partial<any> {
    return {
      id: '',
      type: '',
      date: new Date().toISOString(),
      subject: '',
      description: '',
      contactId: '',
      contactIds: [],
      partnerIds: [],
      userIds: [],
      organizationHierarchyIds: [],
      emailAddresses: [],
      phoneNumbers: [],
      location: ''
    };
  }

  /**
   * Open manual entry dialog for Google Sheet URL and sheet name
   */
  openManualEntryDialog(type: string): void {
    // Set the import type
    this.setImportType(type);
    
    const dialogRef = this.dialogService.open(ManualEntryDialogComponent, {
      header: `Import ${type.charAt(0).toUpperCase() + type.slice(1)}s - Manual Entry`,
      width: '600px',
      height: 'auto',
      closable: true,
      data: {
        entityType: type
      }
    });

    dialogRef.onClose.subscribe((result) => {
      if (result && result.url && result.sheetName) {
        // Extract sheet ID from URL
        const sheetId = this.extractSheetIdFromUrl(result.url);
        if (sheetId) {
          this.processManualEntry(sheetId, result.sheetName, type);
        } else {
          this.feedbackDialogService.showErrorToast({
            detail: 'Invalid Google Sheet URL. Please check the URL and try again.',
            life: 5000
          });
        }
      }
    });
  }

  /**
   * Extract Google Sheet ID from URL
   */
  private extractSheetIdFromUrl(url: string): string | null {
    try {
      // Match patterns like:
      // https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit...
      // https://docs.google.com/spreadsheets/d/{SHEET_ID}/...
      const match = url.match(/\/spreadsheets\/d\/([a-zA-Z0-9-_]+)/);
      return match ? match[1] : null;
    } catch (error) {
      console.error('Error extracting sheet ID from URL:', error);
      return null;
    }
  }

  /**
   * Process manual entry with sheet ID and custom sheet name
   */
  private processManualEntry(sheetId: string, sheetName: string, type: string): void {
    this.isLoading.set(true);
    
    let sheetType = type;
    if (type === 'user_role_import') {
      sheetType = 'User Role Import';
    }
    
    this.loadingOverlayService.show(`Processing ${sheetType} spreadsheet (${sheetName}), please wait...`);
    
    this.feedbackDialogService.showInfoToast({ 
      detail: `Processing ${sheetType} spreadsheet (${sheetName}), please wait...`,
      life: 3000
    });

    const timeoutDuration = 300000; // 5 minutes timeout

    // Call analyzeFile with custom sheet name
    this.importService.analyzeFile(sheetId, type.includes('user_role') ? type : `bulk_${type}_action`, sheetName).pipe(
      timeout(timeoutDuration),
      catchError((error) => {
        console.error('🔍 Caught error in analyzeFileManual pipe:', error);
        throw error;
      })
    ).subscribe({
      next: (response: any) => {
        this.handleAnalyzeFileResponse(response, sheetType, sheetId);
      },
      error: (error) => {
        this.handleAnalyzeFileError(error, sheetType);
      }
    });
  }

  openGoogleSheetPicker(type: string) {
    try {
      
      // Set the import type before doing anything else
      this.setImportType(type);
      
      // Test if the service is available
      if (!this.importGoogleSheetService) {
        throw new Error('Google Sheet service is not available');
      }
      
      this.importGoogleSheetService.openPicker().subscribe({
              next: (sheetId) => {
          // Check if the picker was canceled
          if (sheetId === 'CANCELED') {
            this.isLoading.set(false);
            this.loadingOverlayService.hide();
            return;
          }
        
        // When a file is selected, show loading indicator and message
        this.isLoading.set(true);

        let sheetType = type;
        if (type === 'user_role_import') {
          sheetType = 'User Role Import';
        }
        this.loadingOverlayService.show(`Pre-processing ${sheetType} spreadsheet, please wait...`);

        
        this.feedbackDialogService.showInfoToast({ 
          detail: `Pre-processing ${sheetType} spreadsheet, please wait...`,
          life: 3000
        });

        const timeoutDuration = 300000; // 5 minutes timeout
        const timeout$ = timer(timeoutDuration).pipe(
          map(() => {
            throw new Error('Request timed out. Please try again.');
          })
        );

        this.importService.analyzeFile(sheetId, type.includes('user_role') ? type : `bulk_${type}_action`).pipe(
          timeout(timeoutDuration),
          catchError((error) => {
            console.error('🔍 Caught error in analyzeFile pipe:', error);
            throw error;
          })
        ).subscribe({
          next: (response: any) => {
            this.handleAnalyzeFileResponse(response, sheetType, sheetId);
          },
          error: (error) => {
            this.handleAnalyzeFileError(error, sheetType);
          }
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        console.error('Error opening Google Drive picker:', error);
        
        // Extract the detailed error message if available
        let errorMessage = 'Error opening Google Drive: Unknown error';
        
        if (error.error) {
          if (error.error.details) {
            errorMessage = `Error: ${error.error.details}`;
          } else if (error.error.message) {
            errorMessage = `Error: ${error.error.message}`;
          } else if (typeof error.error === 'string') {
            errorMessage = `Error: ${error.error}`;
          }
        } else if (error.message) {
          errorMessage = `Error: ${error.message}`;
        }
        
        this.feedbackDialogService.showErrorToast({ 
          detail: errorMessage,
          life: 7000 // Show longer since it's a detailed message
        });
        
        // Close the dialog when an error occurs
        this.closeDialog();
      }
    });
  } catch (error) {
    console.error('🔍 Synchronous error in openGoogleSheetPicker:', error);
    this.isLoading.set(false);
    this.loadingOverlayService.hide();
    
    this.feedbackDialogService.showErrorToast({ 
      detail: 'Unexpected error occurred: ' + (error instanceof Error ? error.message : 'Unknown error'),
      life: 7000
    });
    
    // Close the dialog when an error occurs
    this.closeDialog();
  }
}

  setData(data: any[]) {
    if (!data || data.length === 0) {
      console.warn('Empty or null data provided to ImportDialogService.setData()');
      this.data.set([]);
      // Clear import errors even for empty data
      this.clearImportErrorDetails();
      return;
    }

    // Normalize the data to ensure it's in the right format
    const normalizedData = this.normalizeRecordData(data);
    
    // Set the normalized data
    this.data.set(normalizedData);
    // Clear any previous import errors when new data is loaded
    this.clearImportErrorDetails();
    console.log('🧹 Cleared previous import errors for new data load');
  }

  /**
   * Open dialog for synchronous/direct import (no notification)
   * @param data The data to import
   * @param type The type of import (e.g., 'entity')
   * @returns Observable of dialog result
   */
  openSynchronousImport(data: any[], type: string): Observable<any> {
    // Clear any previous state from previous imports
    this.clearImportErrorDetails();
    console.log('🧹 Cleared previous import state for new synchronous import');
    
    this.setImportType(type);
    
    // First set the data
    this.setData(data);
    
    // Clear any previous notification info to ensure this is treated as synchronous
    this.clearNotificationInfo();
    
    // Then open the dialog
    const header = `Import ${type.charAt(0).toUpperCase() + type.slice(1)} - ${data.length} records`;
    return this.openImportDialog(header);
  }

  /**
   * Trigger import process
   */
  triggerImport(type: string) {
    // Ensure we use the explicitly set import type
    
    // Get the selected rows for import
    const selectedData = this.getSelectedRowsForImport();
    console.log('🔍 Import triggered - Selected data:', selectedData);
    console.log('🔍 Total records in dialog:', this.data().length);
    console.log('🔍 Selected records for import:', selectedData.length);
    
    if (!selectedData || selectedData.length === 0) {
      this.feedbackDialogService.showWarningToast({ 
        detail: 'No rows selected for import' 
      });
      return;
    }
    
    // Apply default values to all selected records before import
    const dataWithDefaults = this.applyDefaultValuesToRecords(selectedData);
    
    this.isLoading.set(true);
    this.loadingOverlayService.show(`Importing ${dataWithDefaults.length} records...`);
    
    // Execute the import with the specified type
    this.importService.bulkUpload(dataWithDefaults, type).subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        
        console.log('🔍 Raw import response:', response);
        
        let parsedResponse: any;
        try {
          // Handle different response formats
          if (typeof response.message === 'string') {
            parsedResponse = JSON.parse(response.message);
          } else if (response.message && typeof response.message === 'object') {
            parsedResponse = response.message;
          } else if (typeof response === 'object') {
            parsedResponse = response;
          } else {
            throw new Error('Unexpected response format');
          }
        } catch (error) {
          console.error('Error parsing import response:', error);
          console.error('Raw response:', response);
          // Fallback for unparseable responses
          const errorMessage = error instanceof Error ? error.message : 'Unknown parsing error';
          parsedResponse = {
            IsSuccess: false,
            ErrorDetails: [`Failed to parse import response: ${errorMessage}`],
            message: 'Import processing error'
          };
        }
        
        console.log('🔍 Parsed import response:', parsedResponse);
        
        // Handle partial success/failure scenarios
        // Priority: ErrorDetails (new structured format) > Errors (backward compatibility)
        const errorDetails = parsedResponse.ErrorDetails || 
                           parsedResponse.Errors || 
                           parsedResponse.errors || 
                           parsedResponse.errorDetails || 
                           [];
        
        console.log('🔍 Extracted error details:', errorDetails);
        const totalRecords = dataWithDefaults.length;
        const failedRecords = errorDetails.length;
        const successfulRecords = totalRecords - failedRecords;
        
        // Check if the backend explicitly marked this as a failure
        if (parsedResponse.IsSuccess === false || failedRecords > 0) {
          // There are some failures or backend marked as failed
          if (successfulRecords > 0 && failedRecords > 0) {
            // Partial success - some records imported, some failed
            this.handlePartialImportSuccess(successfulRecords, failedRecords, errorDetails);
          } else {
            // Complete failure - no records imported or backend failure
            const errorMessage = errorDetails.length > 0 
              ? `Import failed: ${errorDetails.join(', ')}`
              : 'Import failed: No records were imported. Ensure the basic mandatory fields are filled in';
            
            this.feedbackDialogService.showErrorToast({ 
              detail: errorMessage
            });
          }
          return;
        }
        
        // Complete success - all records imported
        this.feedbackDialogService.showSuccessToast({ 
          detail: `Import successful: All ${dataWithDefaults.length} records imported successfully` 
        });
        
        // Mark notification as read if this was from a notification
        if (this.notificationId) {
          this.markNotificationAsRead();
        }
        
        // Trigger refresh of the list view if importing entities
        window.dispatchEvent(new CustomEvent('refresh-listview'));
        
        this.closeDialog();
        this.data.set([]);
        this.selectedRows.set([]);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        
        // Extract the detailed error message if available
        let errorMessage = 'Import failed: Unknown error';
        
        if (error.error) {
          if (error.error.details) {
            errorMessage = `Import failed: ${error.error.details}`;
          } else if (error.error.message) {
            errorMessage = `Import failed: ${error.error.message}`;
          } else if (typeof error.error === 'string') {
            errorMessage = `Import failed: ${error.error}`;
          }
        } else if (error.message) {
          errorMessage = `Import failed: ${error.message}`;
        }
        
        this.feedbackDialogService.showErrorToast({ 
          detail: errorMessage,
          life: 7000 // Show longer since it's a detailed message
        });
      }
    });
  }

  /**
   * Show internal duplicate error dialog with detailed information
   */
  public showInternalDuplicateError(response: any, type: string): void {
    const duplicateInfo = response.internalDuplicates;
    if (!duplicateInfo) {
      this.feedbackDialogService.showErrorToast({ 
        detail: 'Internal duplicates found in the file. Please fix the duplicates and try again.' 
      });
      return;
    }

    // Create detailed message about the duplicates
    const fileId = response.fileId;
    const fileIdDisplay = fileId ? ` (Sheet ID: ${fileId})` : '';
    let detailsHtml = `
      <div class="internal-duplicates-dialog">
        <p><strong>Duplicate records found within your uploaded file${fileIdDisplay}:</strong></p>
        <div class="duplicate-summary mb-3">
          <p>• Total records: ${duplicateInfo.totalRecords}</p>
          <p>• Clean records: ${duplicateInfo.cleanRecords}</p>
          <p>• Duplicate groups: ${duplicateInfo.totalGroups}</p>
          <p>• Total duplicate records: ${duplicateInfo.totalDuplicateRecords}</p>
        </div>
        <div class="duplicate-details">
          <p><strong>Duplicate Groups:</strong></p>
    `;

    duplicateInfo.duplicateGroups.forEach((group: any, index: number) => {
      detailsHtml += `
        <div class="duplicate-group mb-2 p-2" style="border-left: 3px solid #ff6b35; background: #fff5f5;">
          <p><strong>Group ${index + 1}:</strong></p>
          <p>Master Record (Row ${group.masterRowNumber}): ${this.formatRecordForDisplay(group.masterRecord, type)}</p>
          <p>Duplicate Rows: ${group.duplicateRowNumbers.join(', ')}</p>
          <p>Match Reason: ${group.matchReasons.join(', ')}</p>
        </div>
      `;
    });

    detailsHtml += `
        </div>
        <div class="mt-3">
          <p><strong>Please fix these duplicates in your Google Sheet and try importing again.</strong></p>
        </div>
      </div>
    `;

    // Show confirmation dialog with detailed information
    const headerText = fileId 
      ? `Internal Duplicates Found in ${type.charAt(0).toUpperCase() + type.slice(1)} File (Sheet ID: ${fileId})`
      : `Internal Duplicates Found in ${type.charAt(0).toUpperCase() + type.slice(1)} File`;
      
    this.confirmationService.confirm({
      message: detailsHtml,
      header: headerText,
      acceptLabel: 'OK',
      rejectLabel: '',
      acceptButtonStyleClass: 'p-button-primary',
      rejectVisible: false,
      dismissableMask: true,
      accept: () => {
        // Just close the dialog
      }
    });
  }

  /**
   * Format a record for display in the duplicate error dialog
   */
  private formatRecordForDisplay(record: any, type: string): string {
    if (!record) return 'N/A';
    
    try {
      switch (type.toLowerCase()) {
        case 'contact':
          return `${record.firstName || ''} ${record.lastName || ''} (${record.email || 'No email'})`.trim();
        case 'partner':
          return `${record.name || 'Unnamed'} ${record.partnerShortDescription ? '- ' + record.partnerShortDescription : ''}`.trim();
        case 'interaction':
          const formattedDate = this.formatDateForDisplay(record.date);
          return `${record.type || 'Unknown type'}: ${record.subject || 'No subject'} (${formattedDate})`.trim();
        default:
          return JSON.stringify(record).substring(0, 100) + '...';
      }
    } catch (error) {
      return 'Error displaying record';
    }
  }

  /**
   * Format date for display in error messages and dialogs
   */
  private formatDateForDisplay(dateValue: any): string {
    if (!dateValue) return 'No date';
    
    try {
      const date = typeof dateValue === 'string' ? new Date(dateValue) : dateValue;
      
      if (date instanceof Date && !isNaN(date.getTime())) {
        // Use a simple, readable format
        return date.toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'short', 
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      }
      
      return dateValue.toString();
    } catch (error) {
      return dateValue ? dateValue.toString() : 'Invalid date';
    }
  }

  /**
   * Handle partial import success - some records succeeded, some failed
   */
  private handlePartialImportSuccess(successfulRecords: number, failedRecords: number, errorDetails: any[]): void {
    // Store error details for visual highlighting and detailed error display
    this.storeImportErrorDetails(errorDetails);
    
    // Remove successfully imported records from the dialog
    this.removeSuccessfulRecordsFromDialog(errorDetails);
    
    // Get summary of error types for better user feedback
    const errorSummary = this.getErrorSummary(errorDetails);
    
    // Show detailed feedback to user with error details
    const messageHtml = `
      <div class="partial-import-result">
        <p><strong>Partial Import Completed:</strong></p>
        <div class="import-summary mb-3">
          <p class="text-green-600">✅ Successfully imported: ${successfulRecords} records</p>
          <p class="text-red-600">❌ Failed to import: ${failedRecords} records</p>
        </div>
        ${errorSummary ? `<div class="mb-3"><p><strong>Common Issues:</strong></p><ul class="text-sm text-gray-700 ml-4">${errorSummary}</ul></div>` : ''}
        <p><strong>Failed records are highlighted in red and remain in the dialog for you to review and re-import.</strong></p>
        <p class="text-sm text-gray-600">Review the error details for each failed record, make corrections, and try importing them again.</p>
      </div>
    `;

    // Show confirmation dialog with detailed information
    this.confirmationService.confirm({
      message: messageHtml,
      header: 'Partial Import Success',
      acceptLabel: 'Continue Editing',
      rejectLabel: 'Close Dialog',
      acceptButtonStyleClass: 'p-button-primary',
      rejectButtonStyleClass: 'p-button-secondary',
      dismissableMask: true,
      accept: () => {
        // User wants to continue editing failed records
        this.feedbackDialogService.showInfoToast({
          detail: `${failedRecords} failed records remain highlighted for editing. Review error details and fix issues before re-importing.`,
          life: 7000
        });
      },
      reject: () => {
        // User wants to close the dialog
        // Clear error details when closing
        this.clearImportErrorDetails();
        // Trigger refresh when closing after partial success
        window.dispatchEvent(new CustomEvent('refresh-listview'));
        this.closeDialog();
        this.data.set([]);
        this.selectedRows.set([]);
      }
    });

    // Trigger refresh of the list view for successfully imported records
    window.dispatchEvent(new CustomEvent('refresh-listview'));

    // Mark notification as read if this was from a notification
    if (this.notificationId) {
      this.markNotificationAsRead();
    }
  }

  /**
   * Handle the response from analyzeFile API call
   */
  private handleAnalyzeFileResponse(response: any, sheetType: string, sheetId?: string): void {
    // Check for error response
    if (response.intent === 'Error') {
      console.error('🔍 File analysis returned error:', response.error);
      this.isLoading.set(false);
      this.loadingOverlayService.hide();
      
      this.feedbackDialogService.showErrorToast({ 
        detail: response.message || 'Error processing file. Please try again.',
        life: 7000
      });
      
      // Close the dialog when an error occurs
      this.closeDialog();
      return;
    }
    
    if (response.intent === 'Processing') {
      // If this is an asynchronous operation
      const jobId = this.importService.getActiveJobId();
      const jobInfo = jobId ? ` (Job ID: ${jobId})` : '';
      
      this.isLoading.set(false);
      this.loadingOverlayService.hide();
      this.data.set([]);
      this.feedbackDialogService.showInfoToast({ 
        detail: `Pre-processing ${sheetType} spreadsheet${jobInfo}. ${response.message}`,
        life: 5000
      });
      return;
    } else if (response.intent === 'InternalDuplicatesFound') {
      // Handle internal duplicates found in the uploaded file
      this.isLoading.set(false);
      this.loadingOverlayService.hide();
      this.showInternalDuplicateError(response, sheetType);
      return;
    } else if (response.intent === 'Success') {
      // Parse the records from the response
      let parsedRecords;
      try {
        parsedRecords = JSON.parse(response.records);
      } catch (error) {
        console.error('Error parsing records:', error);
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        this.feedbackDialogService.showErrorToast({ 
          detail: 'Error processing file data. Please try again.' 
        });
        return;
      }
      
      // Set the data (without auto-detection, using the explicit type)
      this.data.set(parsedRecords);
      
      // Only open the dialog if we have data
      if (this.data() && this.data().length > 0) {
        const headerSuffix = sheetId ? ` (Sheet ID: ${sheetId})` : '';
        this.openImportDialog(`Import ${sheetType}(s) - ${this.data().length} records${headerSuffix}`);
        // Keep loading state until dialog opens then set to false
        setTimeout(() => {
          this.isLoading.set(false);
          this.loadingOverlayService.hide();
        }, 500);
      } else {
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        this.feedbackDialogService.showWarningToast({ 
          detail: 'No data available to import' 
        });
      }
    }
  }

  /**
   * Handle errors from analyzeFile API call
   */
  private handleAnalyzeFileError(error: any, sheetType: string): void {
    this.isLoading.set(false);
    this.loadingOverlayService.hide();
    console.error('Error analyzing file:', error);
    
    // Extract the detailed error message if available
    let errorMessage = 'Error analyzing file: Unknown error';
    
    if (error.error) {
      if (error.error.details) {
        errorMessage = `Error: ${error.error.details}`;
      } else if (error.error.message) {
        errorMessage = `Error: ${error.error.message}`;
      } else if (typeof error.error === 'string') {
        errorMessage = `Error: ${error.error}`;
      }
    } else if (error.message) {
      errorMessage = `Error: ${error.message}`;
    }
    
    this.feedbackDialogService.showErrorToast({ 
      detail: errorMessage,
      life: 7000 // Show longer since it's a detailed message
    });
    
    // Close the dialog when an error occurs
    this.closeDialog();
  }

  /**
   * Remove successfully imported records from the dialog, leaving only failed records
   */
  private removeSuccessfulRecordsFromDialog(errorDetails: any[]): void {
    try {
      const currentData = this.data();
      const currentSelectedRows = this.selectedRows();
      
      console.log('🔍 Attempting to remove successful records. Total current records:', currentData.length);
      console.log('🔍 Error details received:', errorDetails);
      
      // Create a set of failed record identifiers for quick lookup
      const failedRecordIds = new Set();
      errorDetails.forEach((error, index) => {
        const recordId = error.recordId || 
                        error.index || 
                        error.id || 
                        error._importRowId ||
                        error.rowId ||
                        error.rowIndex ||
                        index; // Fallback to error index
        failedRecordIds.add(recordId);
      });
      
      console.log('🔍 Failed record IDs:', Array.from(failedRecordIds));
      
      // If we have same number of errors as records, it means all failed (don't filter)
      if (failedRecordIds.size >= currentData.length) {
        console.log('⚠️ All records failed, keeping all records in dialog');
        return;
      }
      
      // If we don't have any error identifiers, keep all records as safety measure
      if (failedRecordIds.size === 0) {
        console.warn('⚠️ No failed record identifiers found, keeping all records in dialog');
        return;
      }
      
      // Filter data to keep only failed records
      // Try multiple matching strategies
      const failedRecords = currentData.filter((record, index) => {
        // Strategy 1: Match by _importRowId
        if (record._importRowId && failedRecordIds.has(record._importRowId)) {
          return true;
        }
        
        // Strategy 2: Match by record.id
        if (record.id && failedRecordIds.has(record.id)) {
          return true;
        }
        
        // Strategy 3: Match by index position
        if (failedRecordIds.has(index)) {
          return true;
        }
        
        // Strategy 4: Match by the numerical part of _importRowId if it exists
        if (record._importRowId && typeof record._importRowId === 'string') {
          const importIdIndex = parseInt(record._importRowId.replace('import-', ''));
          if (!isNaN(importIdIndex) && failedRecordIds.has(importIdIndex)) {
            return true;
          }
        }
        
        return false;
      });
      
      // Filter selected rows to keep only failed records
      const failedSelectedRows = currentSelectedRows.filter((record, index) => {
        // Use same matching logic as above
        if (record._importRowId && failedRecordIds.has(record._importRowId)) {
          return true;
        }
        if (record.id && failedRecordIds.has(record.id)) {
          return true;
        }
        if (failedRecordIds.has(index)) {
          return true;
        }
        if (record._importRowId && typeof record._importRowId === 'string') {
          const importIdIndex = parseInt(record._importRowId.replace('import-', ''));
          if (!isNaN(importIdIndex) && failedRecordIds.has(importIdIndex)) {
            return true;
          }
        }
        return false;
      });
      
      console.log(`✅ Filtered records: ${currentData.length} -> ${failedRecords.length} (removed ${currentData.length - failedRecords.length} successful)`);
      console.log(`✅ Filtered selections: ${currentSelectedRows.length} -> ${failedSelectedRows.length}`);
      
      // Update the data and selected rows
      this.data.set(failedRecords);
      this.selectedRows.set(failedSelectedRows);
      
      // Auto-select all failed records for user convenience
      if (failedRecords.length > 0) {
        const allFailedRecords = [...failedRecords];
        this.selectedRows.set(allFailedRecords);
        console.log(`✅ Auto-selected ${allFailedRecords.length} failed records for re-import`);
      }
      
      // Force refresh to sync component state
      setTimeout(() => {
        this.refreshSelection();
      }, 100);
      
    } catch (error) {
      console.error('Error removing successful records from dialog:', error);
      // On error, keep all records to avoid data loss
    }
  }

  /**
   * Store import error details for visual highlighting and detailed error display
   */
  private storeImportErrorDetails(errorDetails: any[]): void {
    const errorMap = new Map<number, any>();
    const currentData = this.data();
    
    console.log('🔍 Processing import error details:', errorDetails);
    console.log('🔍 Current data sample:', currentData.slice(0, 2));
    
    errorDetails.forEach((error, index) => {
      // Try multiple ways to extract record identifier
      let recordId = error.recordId || 
                    error.index || 
                    error.id || 
                    error._importRowId ||
                    error.rowId ||
                    error.rowIndex;
      
      // If no explicit ID, try to match by record order/index as fallback
      if (recordId === undefined) {
        recordId = index; // Use error index as fallback
        console.log(`⚠️ No recordId found for error ${index}, using index as fallback`);
      }
      
      // Extract meaningful error message
      let message = 'Import failed';
      if (typeof error === 'string') {
        message = error;
      } else if (error) {
        message = error.message || 
                 error.error || 
                 error.errorMessage || 
                 error.description || 
                 'Import failed - see details';
      }
      
      const errorInfo = {
        message: message,
        details: error.details || error.validationErrors || error.errors || [],
        field: error.field || null,
        value: error.value || null,
        originalError: error // Keep original for debugging
      };
      
      errorMap.set(recordId, errorInfo);
      console.log(`🔍 Mapped error for recordId ${recordId}:`, errorInfo.message);
    });
    
    this.importErrors.set(errorMap);
    console.log('✅ Stored import errors for', errorMap.size, 'records');
  }

  /**
   * Clear import error details
   */
  clearImportErrorDetails(): void {
    this.importErrors.set(new Map());
  }

  /**
   * Get error summary for user feedback
   */
  private getErrorSummary(errorDetails: any[]): string {
    const errorTypes = new Map<string, number>();
    
    errorDetails.forEach(error => {
      // Try to extract a meaningful error message from various possible structures
      let message = 'Unknown error';
      
      if (typeof error === 'string') {
        message = error;
      } else if (error) {
        message = error.message || 
                 error.error || 
                 error.errorMessage || 
                 error.description || 
                 error.details || 
                 (typeof error === 'object' ? JSON.stringify(error) : 'Unknown error');
      }
      
      const count = errorTypes.get(message) || 0;
      errorTypes.set(message, count + 1);
    });
    
    const summaryItems = Array.from(errorTypes.entries())
      .map(([message, count]) => `<li>${message} (${count} record${count > 1 ? 's' : ''})</li>`)
      .slice(0, 5); // Limit to top 5 error types
    
    return summaryItems.length > 0 ? summaryItems.join('') : '';
  }

  /**
   * Check if a record has import errors
   */
  hasImportError(recordId: any): boolean {
    return this.importErrors().has(recordId);
  }

  /**
   * Get import error details for a record
   */
  getImportError(recordId: any): any {
    return this.importErrors().get(recordId);
  }

  /**
   * Force refresh of selection state - useful after filtering records
   */
  refreshSelection(): void {
    // Trigger reactivity by setting the same values
    const currentData = this.data();
    const currentSelection = this.selectedRows();
    
    // Force signals to update
    this.data.set([...currentData]);
    this.selectedRows.set([...currentSelection]);
    
    console.log('🔄 Refreshed selection state:', currentSelection.length, 'records selected out of', currentData.length, 'total');
  }
}
