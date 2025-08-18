import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {Observable, map, of, catchError} from 'rxjs';
import { Contact } from '../../../../features/internal/models/contact.model';
import { Partner } from '../../../../features/internal/models/partner.model';
import { Interaction } from '../../../../features/internal/models/interaction.model';
import { InteractionType } from '../../../../features/internal/models/interaction-type.enum';

export interface AnalyzeFileRequest {
  type: string;
  fileId: string;
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
}

@Injectable({
  providedIn: 'root',
})
export class ImportService {
  private readonly apiUrl = '/api/import';
  private processingFile = false;
  private activeJobId: string | null = null;

  constructor(private http: HttpClient) {}

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
  analyzeFile(fileId: string, type: string): Observable<ImportAnalysisResponse> {
    this.processingFile = true;
    const payload: AnalyzeFileRequest = {
      type,
      fileId
    };

    return this.http.post<ImportAnalysisResponse>(`${this.apiUrl}/analyse-file`, payload)
      .pipe(
        map(response => {
          this.processingFile = false;
          
          // Store the job ID if this is an async operation
          if (response && response.jobId) {
            this.activeJobId = response.jobId;
          }
          
          return response;
        }),
        catchError(error => {
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
      
      // For all other properties, delete if empty string to avoid serialization issues
      Object.keys(processedRecord).forEach(prop => {
        if (!specialProperties.includes(prop) && processedRecord[prop] === '') {
          delete processedRecord[prop];
        }
      });
      
      return processedRecord;
    });

    const payload: BulkUploadRequest = {
      type,
      records: processedRecords
    };
    return this.http.post(`${this.apiUrl}/bulk-upload`, payload);
  }
}
