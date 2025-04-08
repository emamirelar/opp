import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {Observable, map, of} from 'rxjs';
import { Contact } from '../../../../features/internal/models/contact.model';

export interface AnalyzeFileRequest {
  type: string;
  fileId: string;
}

export interface BulkUploadRequest {
  type: string;
  records: any[];
}

export interface ImportAnalysisResponse {
    type: string;
    records: any[];
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

@Injectable({
  providedIn: 'root',
})
export class ImportService {
  private readonly apiUrl = '/api/import';

  constructor(private http: HttpClient) {}

  /**
   * Analyze a Google Sheet file by its ID
   * @param fileId The Google Sheets ID
   * @param type The type of data being imported (e.g., 'bulk_contact_action')
   */
  analyzeFile(fileId: string, type: string): Observable<ImportAnalysisResponse> {
    const payload: AnalyzeFileRequest = {
      type,
      fileId
    };

    // return this.http.post<ImportAnalysisResponse>(`${this.apiUrl}/analyse-file`, payload);
    return of({
      type: 'string',
      records: EXAMPLE_CONTACTS,
    });
  }

  /**
   * Perform a bulk upload of records
   * @param records Array of records to upload
   * @param type The type of data being uploaded (e.g., 'bulk_contact_action')
   */
  bulkUpload(records: any[], type: string): Observable<any> {
    const payload: BulkUploadRequest = {
      type,
      records
    };
    //return this.http.post(`${this.apiUrl}/bulk-upload`, payload);
    return of({});
  }
}
