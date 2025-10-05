import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {Observable, map, of, catchError, throwError} from 'rxjs';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { Partner } from '@partnerships/partners/models/partner.model';
import { Interaction } from '@partnerships/interactions/models/interaction.model';
import { InteractionType } from '@partnerships/interactions/models/interaction-type.enum';

export interface AnalyzeFileRequest {
  type: string;
  fileId: string;
  sheetName?: string; // Optional: Custom sheet name for manual entry
}

export interface CancelAnalysisRequest {
  jobId: string;
}

export interface BulkUploadRequest {
  type: string;
  records: any[];
}

export interface ImportAnalysisResponse {
    type: string;
    records: any[];
    jobId?: string; // PubSub job ID for async operations
    intent?: string; // 'Success' | 'Processing' | 'InternalDuplicatesFound' | 'Error'
    internalDuplicates?: {
        totalGroups: number;
        totalDuplicateRecords: number;
        totalRecords: number;
        cleanRecords: number;
        duplicateGroups: Array<{
            masterRowNumber: number;
            duplicateRowNumbers: number[];
            matchReasons: string[];
            masterRecord: any;
            duplicateRecords: any[];
        }>;
    };
    message?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ImportService {
  private readonly apiUrl = '/api';
  private processingFile = false;
  private activeJobId: string | null = null;

  constructor(private http: HttpClient) {}

  /**
   * Get the correct entity-specific API endpoint based on the import type
   * @param type The import type (e.g., 'bulk_partner_action', 'bulk_contact_action')
   * @returns The entity-specific API endpoint
   */
  private getEntitySpecificEndpoint(type: string): string {
    // Extract entity type from the import type and map to correct APIDictionary paths
    if (type.includes('partner') && !type.includes('partnercategory') && !type.includes('partnergroup')) {
      return `${this.apiUrl}/partner`;  // Singular: /api/partner
    } else if (type.includes('partnercategory')) {
      return `${this.apiUrl}/partnercategory`;   // Singular: /api/partnercategory
    } else if (type.includes('partnergroup')) {
      return `${this.apiUrl}/partnergroup`;   // Singular: /api/partnergroup
    } else if (type.includes('contact')) {
      return `${this.apiUrl}/contact`;   // Singular: /api/contact
    } else if (type.includes('interaction')) {
      return `${this.apiUrl}/interactions`; // Plural: /api/interactions
    } else if (type.includes('user_role')) {
      return `${this.apiUrl}/user-management`; // User role imports: /api/user-management
    } else {
      // Default to the original import endpoint if entity cannot be determined
      console.warn(`Unknown import type: ${type}. Using default import endpoint.`);
      return `${this.apiUrl}/import`;
    }
  }

  /**
   * Get the active job ID if one exists
   */
  getActiveJobId(): string | null {
    return this.activeJobId;
  }

  /**
   * Check if a file is currently being processed
   */
  isProcessingFile(): boolean {
    return this.processingFile;
  }

  /**
   * Analyze a Google Sheet file by its ID
   * @param fileId The Google Sheets ID
   * @param type The type of data being imported (e.g., 'bulk_contact_action')
   */
  analyzeFile(fileId: string, type: string, sheetName?: string): Observable<ImportAnalysisResponse> {
    console.log('🔍 ImportService.analyzeFile called with:', { fileId, type, sheetName });
    this.processingFile = true;
    const payload: AnalyzeFileRequest = {
      type,
      fileId,
      ...(sheetName && { sheetName }) // Only include sheetName if provided
    };

    // Determine the entity-specific endpoint based on the type
    const entityEndpoint = this.getEntitySpecificEndpoint(type);
    console.log('🔍 Using endpoint:', `${entityEndpoint}/analyse-file`);
    console.log('🔍 Payload:', payload);
    
    return this.http.post<ImportAnalysisResponse>(`${entityEndpoint}/analyse-file`, payload)
      .pipe(
        map(response => {
          console.log('🔍 ImportService.analyzeFile success response:', response);
          this.processingFile = false;
          
          // Store the job ID if this is an async operation
          if (response && response.jobId) {
            this.activeJobId = response.jobId;
          }
          
          return response;
        }),
        catchError(error => {
          console.error('🔍 ImportService.analyzeFile error caught:', error);
          this.processingFile = false;
          this.activeJobId = null;
          throw error;
        })
      );
    /*return of({
      type: 'string',
      records: EXAMPLE_CONTACTS,
    });*/
  }


  /**
   * Cancel an in-progress file analysis
   * @returns Observable indicating success/failure of cancellation request
   */
  cancelAnalysis(): Observable<any> {
    if (!this.activeJobId) {
      return of({ success: false, message: 'No active analysis job to cancel' });
    }

    const jobId = this.activeJobId;
    const payload: CancelAnalysisRequest = {
      jobId
    };
    
    // Reset state first
    this.processingFile = false;
    this.activeJobId = null;
    
    return this.http.post(`${this.apiUrl}/cancel-analysis`, payload)
      .pipe(
        catchError(error => {
          console.error('Error cancelling analysis:', error);
          throw error;
        })
      );
  }

  /**
   * Perform a bulk upload of records
   * @param records Array of records to upload
   * @param type The type of data being uploaded (e.g., 'bulk_contact_action')
   */
  bulkUpload(records: any[], type: string): Observable<any> {
    // Process records to ensure proper handling - delete empty/falsy properties
    const processedRecords = records.map(record => {
      const processedRecord = { ...record };
    
      // Keep original logic for these specific properties - delete if falsy
      const specialProperties = ['createdBy', 'lastModifiedBy', 'deletedBy', 'id'];
      specialProperties.forEach(prop => {
        if (!processedRecord[prop]) {
          delete processedRecord[prop];
      }
      });
      
      // IMPORTANT: Always preserve _importRowId for error matching (don't delete even if falsy)
      // This is crucial for matching failed records back to the dialog
      
      // For all other properties, delete if empty string to avoid serialization issues
      Object.keys(processedRecord).forEach(prop => {
        if (!specialProperties.includes(prop) && prop !== '_importRowId' && processedRecord[prop] === '') {
          delete processedRecord[prop];
        }
      });
      
      return processedRecord;
    });

    const payload: BulkUploadRequest = {
      type,
      records: processedRecords
    };
    // Determine the entity-specific endpoint based on the type
    const entityEndpoint = this.getEntitySpecificEndpoint(type);
    
    return this.http.post(`${entityEndpoint}/bulk-upload`, payload);
  }
}
