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

export const EXAMPLE_CONTACTS: Contact[] = [
    {
      id: '001',
      salutation: 'Mr',
      firstName: 'John',
      lastName: 'Doe',
      suffix: 'Jr.',
      title: 'Sales Manager',
      pronouns: 'He/Him',
      email: 'john.doe@example.com',
      phone: '+1234567890',
      mobile: '+1234567890',
      department: 'Sales',
      contactNumber: '+1234567890',
      mailingStreet: '123 Main St',
      mailingCity: 'Anytown',
      mailingStateProvince: 'CA',
      mailingPostalCode: '12345',
      mailingCountry: 'USA',
      status: 'Active'
    },
    {
      id: '002',
      salutation: 'Ms',
      firstName: 'Jane',
      lastName: 'Smith',
      title: 'Project Director',
      pronouns: 'She/Her',
      email: 'jane.smith@example.com',
      phone: '+1987654321',
      mobile: '+1987654321',
      department: 'Operations',
      mailingStreet: '456 Oak Ave',
      mailingCity: 'New City',
      mailingStateProvince: 'NY',
      mailingPostalCode: '67890',
      mailingCountry: 'USA',
      status: 'Active'
    },
    {
      id: '003',
      salutation: 'Dr',
      firstName: 'Ahmed',
      lastName: 'Hassan',
      title: 'Technical Advisor',
      email: 'ahmed.hassan@example.com',
      phone: '+4412345678',
      department: 'Technical Advisory',
      mailingStreet: '78 High Street',
      mailingCity: 'London',
      mailingCountry: 'United Kingdom',
      status: 'Active'
    }
  ];

export const EXAMPLE_PARTNERS: Partner[] = [
    {
      id: '001',
      name: 'Acme Corporation',
      shortName: 'Acme',
      status: 'Active',
      newEngagement: 'Yes',
      phone: '+1234567890',
      website: 'www.acmecorp.com',
      address1City: 'Business City',
      address1Country: 'USA'
    },
    {
      id: '002',
      name: 'Global Solutions Inc.',
      shortName: 'GSI',
      status: 'Active',
      newEngagement: 'No',
      phone: '+4412345678',
      website: 'www.globalsolutions.com',
      address1City: 'Geneva',
      address1Country: 'Switzerland'
    },
    {
      id: '003',
      name: 'Tech Innovations Ltd.',
      shortName: 'TIL',
      status: 'Active',
      newEngagement: 'Yes',
      phone: '+6598765432',
      website: 'www.techinnovations.com',
      address1City: 'Singapore',
      address1Country: 'Singapore'
    }
  ];

export const EXAMPLE_INTERACTIONS: Interaction[] = [
    {
      id: 1,
      type: InteractionType.Email,
      date: '2024-01-15T10:30:00.000Z',
      subject: 'Project Kick-off Meeting',
      description: 'Discussed project timeline and deliverables with the partner team.',
      contactId: 1,
      contactName: 'John Doe',
      status: 'Active',
      contactIds: [1],
      partnerIds: [1],
      userIds: [1],
      emailAddresses: ['john.doe@example.com'],
      phoneNumbers: ['+1234567890'],
      location: 'Virtual Meeting',
      orgUnitId: 1,
      createdBy: 1
    },
    {
      id: 2,
      type: InteractionType.VirtualMeeting,
      date: '2024-01-20T14:00:00.000Z',
      subject: 'Technical Review Session',
      description: 'Technical review of proposed solutions and implementation approach.',
      contactId: 2,
      contactName: 'Jane Smith',
      status: 'Active',
      contactIds: [2],
      partnerIds: [2],
      userIds: [1, 2],
      emailAddresses: ['jane.smith@example.com'],
      phoneNumbers: ['+1987654321'],
      location: 'Conference Room A',
      orgUnitId: 1,
      createdBy: 1
    },
    {
      id: 3,
      type: InteractionType.Phone,
      date: '2024-01-25T16:15:00.000Z',
      subject: 'Follow-up Call',
      description: 'Follow-up discussion on project progress and next steps.',
      contactId: 3,
      contactName: 'Ahmed Hassan',
      status: 'Active',
      contactIds: [3],
      partnerIds: [3],
      userIds: [1],
      emailAddresses: ['ahmed.hassan@example.com'],
      phoneNumbers: ['+4412345678'],
      location: 'Phone Call',
      orgUnitId: 1,
      createdBy: 1
    }
  ];

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
