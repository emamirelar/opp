import { Injectable, inject, signal } from '@angular/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ImportDialogComponent } from './import-dialog.component';
import { ImportFooterComponent } from './footer/import-dialog-footer.component';
import { Observable, Subject, forkJoin, of } from 'rxjs';
import { WritableSignal } from '@angular/core';
import { ImportService } from '../import.service';
import { FeedbackDialogService } from '../../../../pages/services/feedback-dialog.service';
import { ImportGoogleSheetService } from '../import-google-sheet.service';
import { catchError, finalize, mergeMap } from 'rxjs/operators';
import { ConfirmationService } from 'primeng/api';
import { NotificationService } from '../../../../services/notification.service';
import { LoadingOverlayService } from '../../../components/loading-overlay/loading-overlay.component';

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
    if (selected && selected.length > 0) {
      return selected;
    }
    // If no explicit selection, use all data
    return this.data();
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
      shortName: '',
      status: 'Active',
      newEngagement: null,
      phone: '',
      website: '',
      pooledFund: null,
      ddRequired: null,
      ddeacDone: null,
      eacReference: '',
      globalKeyAccount: false,
      unSecretariatEntity: false,
      levyPotentiallyApplies: null,
      reasonForLevyNotApplying: null,
      levyTreatment: null,
      address1Street: '',
      address1Street2: '',
      address1City: '',
      address1StateProvince: '',
      address1PostalCode: '',
      address1Country: ''
    };
  }

  openGoogleSheetPicker(type: string) {
    // Set the import type before doing anything else
    this.setImportType(type);
    
    // Show loading state while opening the picker
    this.isLoading.set(true);
    this.loadingOverlayService.show('Opening Google Drive, please wait...');
    
    // Open the picker
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
        this.loadingOverlayService.show(`Pre-processing ${type} spreadsheet, please wait...`);
        
        this.feedbackDialogService.showInfoToast({ 
          detail: `Pre-processing ${type} spreadsheet, please wait...`,
          life: 3000
        });
        
        // Process the selected file
        this.importService.analyzeFile(sheetId,`bulk_${type}_action`).subscribe({
          next: (response: any) => {
            if (response.intent === 'Processing') {
              // If this is an asynchronous operation
              const jobId = this.importService.getActiveJobId();
              const jobInfo = jobId ? ` (Job ID: ${jobId})` : '';
              
              this.isLoading.set(false);
              this.loadingOverlayService.hide();
              this.data.set([]);
              this.feedbackDialogService.showInfoToast({ 
                detail: `Pre-processing ${type} spreadsheet${jobInfo}. ${response.message}`,
                life: 5000
              });
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
                this.openImportDialog(`Import ${type}(s) - ${this.data().length} records`);
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
      }
    });
  }

  setData(data: any[]) {
    if (!data || data.length === 0) {
      console.warn('Empty or null data provided to ImportDialogService.setData()');
      this.data.set([]);
      return;
    }

    // Normalize the data to ensure it's in the right format
    const normalizedData = this.normalizeRecordData(data);
    
    // Set the normalized data
    this.data.set(normalizedData);
  }

  /**
   * Open dialog for synchronous/direct import (no notification)
   * @param data The data to import
   * @param type The type of import (e.g., 'entity')
   * @returns Observable of dialog result
   */
  openSynchronousImport(data: any[], type: string): Observable<any> {
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
        var parsedResponse = JSON.parse(response.message);
        if (parsedResponse.IsSuccess == false) {
          this.isLoading.set(false);
          this.loadingOverlayService.hide();
          this.feedbackDialogService.showErrorToast({ 
            detail: 'Import failed with one or more errors. Ensure the basic mandatory fields are filled in'
          });
          return;
        }
        this.isLoading.set(false);
        this.loadingOverlayService.hide();
        this.feedbackDialogService.showSuccessToast({ detail: 'Import successful' });
        
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
}
