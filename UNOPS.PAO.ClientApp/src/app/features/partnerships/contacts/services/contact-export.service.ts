import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { switchMap, tap, catchError, map } from 'rxjs/operators';
import { Contact } from '../models/contact.model';
import { ContactService } from './contact.service';
import { ExportGoogleSheetService } from '@features/import-export/services/export-google-sheet.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { ConfirmationService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class ContactExportService {
  private contactService = inject(ContactService);
  private exportGoogleSheetService = inject(ExportGoogleSheetService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private confirmationService = inject(ConfirmationService);

  /**
   * Exports contacts to a new Google Sheet
   * @param fileName Name for the exported file
   * @param searchText Optional search text to filter contacts
   * @param pageSize Number of records to export (default: 1000)
   * @returns Observable with the result of the export operation
   */
  exportToGoogleSheet(
    fileName: string = 'Contacts Export',
    searchText?: string,
    pageSize: number = 1000
  ): Observable<{ id: string, url: string }> {
    // Show loading message
    this.feedbackDialogService.showInfoToast({
      detail: 'Preparing contacts for export...',
      sticky: true
    });

    // Load contacts data
    return this.contactService.getContacts({
      page: 0,
      pageSize: pageSize,
      searchText: searchText || ''
    }).pipe(
      tap(() => this.feedbackDialogService.clearAll()),
      switchMap(response => {
        // Create a more export-friendly format of contacts
        const exportableContacts = response.data.map(contact => {
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

        if (exportableContacts.length === 0) {
          this.feedbackDialogService.showWarningToast({
            detail: 'No contacts found to export'
          });
          throw new Error('No contacts found to export');
        }

        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
        const finalFileName = `${fileName} ${timestamp}`;

        return this.exportGoogleSheetService.exportToSheet(exportableContacts, finalFileName).pipe(
          map(result => ({ ...result, recordCount: exportableContacts.length }))
        );
      }),
      tap(result => {
        // Clear any existing toasts
        this.feedbackDialogService.clearAll();
        
        // Automatically open the Google Sheet in a new tab
        window.open(result.url, '_blank');
        
        // Show success confirmation dialog with record count
        this.confirmationService.confirm({
          message: `Contacts exported successfully!<br><br><strong>${result.recordCount} records</strong> have been exported to Google Sheets.<br><br><a href="${result.url}" target="_blank" style="text-decoration: underline; color: #007bff; font-weight: bold; padding: 4px 8px; border: 1px solid #007bff; border-radius: 4px; background-color: #f8f9fa;">📊 Open Spreadsheet</a>`,
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
          detail: 'Failed to export contacts: ' + (error.message || 'Unknown error')
        });
        throw error;
      })
    );
  }
} 
