import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { ExportGoogleSheetService } from '@features/import-export/services/export-google-sheet.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { switchMap, tap, catchError, map } from 'rxjs/operators';
import { ConfirmationService } from 'primeng/api';
import { SearchParams } from './listview.model';

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
   * @param searchTextOrParams Optional search text or search parameters for filtering
   * @param sortField Optional field to sort by
   * @param sortOrder Optional sort direction
   * @param transformFn Optional function to transform the data for export
   * @returns Observable with the result of the export operation
   */
  exportToGoogleSheet<T extends object>(
    entityName: string,
    apiUrl: string,
    searchTextOrParams?: string | SearchParams,
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
    
    // Handle different search parameter types
    if (typeof searchTextOrParams === 'string') {
      // Handle simple string search (backward compatibility)
      if (searchTextOrParams) {
        queryParams.query = searchTextOrParams;
      }
    } else if (searchTextOrParams) {
      // Handle advanced search with SearchParams object
      if (searchTextOrParams.generalSearch) {
        queryParams.query = searchTextOrParams.generalSearch;
      }
      
      if (searchTextOrParams.fieldSearches && searchTextOrParams.fieldSearches.length > 0) {
        queryParams.filters = JSON.stringify(searchTextOrParams.fieldSearches);
      }
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
        
        // For partners, use a specialized transform
        if (!transformFn && entityName.toLowerCase() === 'partner') {
          finalTransformFn = this.partnerTransform;
        }
        
        // For interactions, use a specialized transform
        if (!transformFn && entityName.toLowerCase() === 'interaction') {
          finalTransformFn = this.interactionTransform;
        }

        // Transform data with the selected function
        const exportableData = finalTransformFn(response.data);

        // Generate a timestamp for the filename
        const timestamp = new Date().toISOString().replace(/[:.]/g, '-').substring(0, 19);
        const fileName = `${entityName}s Export ${timestamp}`;

        // Export the data to Google Sheets
        return this.exportGoogleSheetService.exportToSheet(exportableData, fileName).pipe(
          map(result => ({ ...result, recordCount: exportableData.length }))
        );
      }),
      tap(() => this.feedbackDialogService.clearAll()),
      tap(result => {
        // Automatically open the Google Sheet in a new tab
        window.open(result.url, '_blank');
        
        // Show success confirmation dialog with record count
        this.confirmationService.confirm({
          message: `${entityName}s exported successfully!<br><br><strong>${result.recordCount} records</strong> have been exported to Google Sheets.<br><br><a href="${result.url}" target="_blank" style="text-decoration: underline; color: #007bff; font-weight: bold; padding: 4px 8px; border: 1px solid #007bff; border-radius: 4px; background-color: #f8f9fa;">📊 Open Spreadsheet</a>`,
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
   * Special transform function for Partner entities
   * Formats partner data with current Partner entity fields
   */
  private partnerTransform(partners: any[]): Record<string, any>[] {
    return partners.map(partner => {
      return {
        ID: partner.id || '',
        Name: partner.name || '',
        ShortDescription: partner.partnerShortDescription || '',
        LongDescription: partner.partnerLongDescription || '',
        Status: partner.status || '',
        PartnerGroupId: partner.partnerGroupId || '',
        PartnerGroupName: partner.partnerGroupName || '',
        PartnerCategoryId: partner.partnerCategoryId || '',
        PartnerCategoryName: partner.partnerCategoryName || '',
        LiaisonOfficeId: partner.liaisonOfficeId || '',
        PartnerFocalPointUserId: partner.partnerFocalPointUserId || '',
        KeyGlobalPartner: partner.keyGlobalPartner || false,
        UNSecretariatPartner: partner.unSecretariatPartner || false,
        UNAndStateEntity: partner.unAndStateEntity || false,
        PartnerApprovalStatus: partner.partnerApprovalStatus || '',
        PartnerApprovalDate: partner.partnerApprovalDate || '',
        PartnerApprovalReference: partner.partnerApprovalReference || '',
        PartnerApprovedBy: partner.partnerApprovedBy || '',
        PartnerLevyStatus: partner.partnerLevyStatus || '',
        PooledFund: partner.pooledFund || false,
        CanCreateNewOpportunities: partner.canCreateNewOpportunities || false,
        ReasonForNoNewOpportunity: partner.reasonForNoNewOpportunity || '',
        DueDiligenceRequired: partner.dueDiligenceRequired || false,
        DueDiligenceApproval: partner.dueDiligenceApproval || '',
        DueDiligenceApprovalDate: partner.dueDiligenceApprovalDate || '',
        DueDiligenceExpiryDate: partner.dueDiligenceExpiryDate || '',
        ErpDimValue: partner.erpDimValue || '',
        CreatedDate: partner.createdDate || '',
        LastModifiedDate: partner.lastModifiedDate || '',
        CreatedBy: partner.createdBy || '',
        LastModifiedBy: partner.lastModifiedBy || ''
      };
    });
  }

  /**
   * Special transform function for Interaction entities
   * Formats interaction data with specific field names and order
   */
  private interactionTransform(interactions: any[]): Record<string, any>[] {
    return interactions.map(interaction => {
      return {
        ID: interaction.id || '',
        Type: interaction.type || '',
        Date: interaction.date || '',
        Subject: interaction.subject || '',
        Description: interaction.description || '',
        ContactId: interaction.contactId || '',
        ContactName: interaction.contactName || '',
        Status: interaction.status || '',
        Location: interaction.location || '',
        CreatedBy: interaction.createdBy || ''
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
        // Skip permissions field entirely
        if (key === 'permissions') {
          return;
        }
        
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
