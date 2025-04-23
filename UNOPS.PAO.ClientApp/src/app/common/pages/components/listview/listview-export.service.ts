import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { ExportGoogleSheetService } from '../../../reusables/components/export/export-google-sheet.service';
import { FeedbackDialogService } from '../../../reusables/services/feedback-dialog.service';
import { switchMap, tap, catchError, map } from 'rxjs/operators';
import { ConfirmationService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class ListviewExportService {
  private http = inject(HttpClient);
  private exportGoogleSheetService = inject(ExportGoogleSheetService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private confirmationService = inject(ConfirmationService);

  /**
   * Exports data to a Google Sheet by fetching all data from the backend without pagination
   * @param entityName Name of the entity being exported (e.g., "Contact", "Partner")
   * @param apiUrl The API endpoint URL to fetch data from
   * @param searchText Optional search text for filtering
   * @param sortField Optional field to sort by
   * @param sortOrder Optional sort direction
   * @param transformFn Optional function to transform the data for export
   * @returns Observable with the result of the export operation
   */
  exportToGoogleSheet<T extends object>(
    entityName: string,
    apiUrl: string,
    searchText?: string,
    sortField?: string,
    sortOrder?: 'asc' | 'desc',
    transformFn?: (data: any[]) => Record<string, any>[]
  ): Observable<{ id: string, url: string }> {
    // Show loading message
    this.feedbackDialogService.showInfoToast({
      detail: `Preparing ${entityName.toLowerCase()}s for export...`,
      sticky: true
    });

    // Prepare query parameters for backend - exclude pagination parameters
    const queryParams: any = {
      // Include export=true flag to signal this is an export operation
      export: true
    };
    
    if (searchText) {
      queryParams.searchText = searchText;
    }
    
    if (sortField) {
      queryParams.orderBy = sortField;
      queryParams.ascending = sortOrder === 'asc';
    }

    // Fetch all records from backend - no pagination parameters means all records
    return this.http.get<any>(apiUrl, { params: queryParams }).pipe(
      map(response => {
        // Handle different response formats
        const records = response.records || response.data || response;
        const totalCount = response.totalCount || response.total || records.length;
        
        return {
          data: records,
          total: totalCount
        };
      }),
      switchMap(response => {
        if (!response.data || response.data.length === 0) {
          this.feedbackDialogService.showWarningToast({
            detail: `No ${entityName.toLowerCase()}s found to export`
          });
          return throwError(() => new Error(`No ${entityName.toLowerCase()}s found to export`));
        }

        // Choose transform function based on entity type if none provided
        let finalTransformFn = transformFn ?? this.defaultTransform;
        
        // For contacts, use a specialized transform
        if (!transformFn && entityName.toLowerCase() === 'contact') {
          finalTransformFn = this.contactTransform;
        }

        // Transform data with the selected function
        const exportableData = finalTransformFn(response.data);

        // Generate a timestamp for the filename
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
        const fileName = `${entityName}s Export ${timestamp}`;

        // Export the data to Google Sheets
        return this.exportGoogleSheetService.exportToSheet(exportableData, fileName);
      }),
      tap(() => this.feedbackDialogService.clearAll()),
      tap(result => {
        // Show success confirmation dialog
        this.confirmationService.confirm({
          message: `${entityName}s exported successfully! <a href="${result.url}" target="_blank" style="text-decoration: underline; color: blue;">Click here</a> to open the spreadsheet.`,
          header: 'Export Complete',
          icon: 'pi pi-check-circle',
          acceptVisible: true,
          rejectVisible: false,
          acceptLabel: 'OK',
          closeOnEscape: true,
          dismissableMask: true
        });
      }),
      catchError(error => {
        this.feedbackDialogService.showErrorToast({
          detail: `Failed to export ${entityName.toLowerCase()}s: ` + (error.message || 'Unknown error')
        });
        throw error;
      })
    );
  }

  /**
   * Special transform function for Contact entities
   * Formats contact data with specific field names and order
   */
  private contactTransform(contacts: any[]): Record<string, any>[] {
    return contacts.map(contact => {
      return {
        ID: contact.id || '',
        Salutation: contact.salutation || '',
        FirstName: contact.firstName || '',
        MiddleName: contact.middleName || '',
        LastName: contact.lastName || '',
        Suffix: contact.suffix || '',
        Title: contact.title || '',
        Pronouns: contact.pronouns || '',
        Partner: contact.partner?.name || '',
        Email: contact.email || '',
        Phone: contact.phone || '',
        Mobile: contact.mobile || '',
        OtherPhone: contact.otherPhone || '',
        Fax: contact.fax || '',
        Department: contact.department || '',
        Description: contact.description || '',
        Status: contact.status || '',
        ContactNumber: contact.contactNumber || '',
        Assistant: contact.assistant || '',
        AssistantPhone: contact.assistantPhone || '',
        AssistantEmail: contact.assistantEmail || '',
        MailingStreet: contact.mailingStreet || '',
        MailingStreet2: contact.mailingStreet2 || '',
        MailingCity: contact.mailingCity || '',
        MailingStateProvince: contact.mailingStateProvince || '',
        MailingPostalCode: contact.mailingPostalCode || '',
        MailingCountry: contact.mailingCountry || ''
      };
    });
  }

  /**
   * Default data transformation that keeps all properties
   * and converts each property to a readable format
   */
  private defaultTransform<T extends object>(data: T[]): Record<string, any>[] {
    if (data.length === 0) return [];

    return data.map(item => {
      const result: Record<string, any> = {};
      
      // Convert all keys to proper case (e.g., 'firstName' to 'First Name')
      Object.entries(item).forEach(([key, value]) => {
        if (typeof value !== 'object' || value === null) {
          // Format the key for display - convert camelCase to Title Case with spaces
          const formattedKey = key
            .replace(/([A-Z])/g, ' $1') // Add space before capital letters
            .replace(/^./, str => str.toUpperCase()) // Capitalize first letter
            .trim();
          
          result[formattedKey] = value === null || value === undefined ? '' : value;
        }
      });
      
      return result;
    });
  }
} 