import { HttpClient, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap, map } from 'rxjs';
import { Contact } from '../models/contact.model';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { PaginationResponse } from '@shared/models/pagination-response.model';
import { DuplicateDetectionResponse, ContactQueryParams } from '@shared/models/api-responses.model';

export interface ContactsParams {
  page: number;
  pageSize: number;
  searchText?: string;
  sortField?: string;
  sortOrder?: 'asc' | 'desc';
}

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  http = inject(HttpClient);
  private importDialogService = inject(ImportDialogService);

  public readonly apiUrl = `/api/contact`;
  private contactData = signal<Contact[]>([]);
  allContacts = this.contactData.asReadonly();

  isLoading = signal(false);

  constructor() { }

  getAll(params: ContactQueryParams): Observable<HttpResponse<PaginationResponse<Contact>>> {
    this.isLoading.set(true);
    return this.http.get<PaginationResponse<Contact>>(this.apiUrl, { observe: 'response', params })
      .pipe(
        tap({
          next: () => this.isLoading.set(false),
          error: () => this.isLoading.set(false)
        })
      );
  }

  getUrl(){
    return this.apiUrl
  }

  getClassicSearchUrl(): string {
    return `${this.apiUrl}`;
  }

  getUploadProfilePictureUrl(contactId: string): string {
    return `${this.apiUrl}/${contactId}/profile-picture`;
  }

  getAllContacts() {
    this.isLoading.set(true);
    this.http.get<PaginationResponse<Contact>>(this.apiUrl).subscribe({
      next: (data) => {
        this.contactData.set(data.records);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading contacts:', err);
        this.isLoading.set(false);
      },
    });
  }

  /**
   * Get contacts with pagination and search
   * @param params Parameters for filtering and pagination
   * @returns Observable with paginated contact data
   */
  getContacts(params: ContactsParams): Observable<{ data: Contact[], total: number }> {
    this.isLoading.set(true);

    const queryParams: ContactQueryParams = {
      page: params.page,
      pageSize: params.pageSize
    };

    if (params.searchText) {
      queryParams.searchText = params.searchText;
    }

    if (params.sortField) {
      queryParams.sortField = params.sortField;
      queryParams.sortOrder = params.sortOrder || 'asc';
    }

    return this.http.get<PaginationResponse<Contact>>(`${this.apiUrl}`, { params: queryParams })
      .pipe(
        map((response) => ({
          data: response.records || [],
          total: response.totalCount || 0
        })),
        tap({
          next: () => this.isLoading.set(false),
          error: () => this.isLoading.set(false)
        })
      );
  }

  getContactById(id: string): Observable<Contact> {
    this.isLoading.set(true);
    return this.http.get<Contact>(`${this.apiUrl}/${id}`)
      .pipe(
        tap({
          next: () => this.isLoading.set(false),
          error: () => this.isLoading.set(false)
        })
      );
  }

  /**
   * Creates a contact with duplicate detection handling
   * Returns either the created contact or duplicate detection response
   */
  createContact( contact: Contact ): Observable<Contact | DuplicateDetectionResponse> {
    this.isLoading.set( true );
    return this.http.post<Contact | DuplicateDetectionResponse>(this.apiUrl, contact).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updateContactById( contact: Contact ): Observable<Contact> {

    this.isLoading.set( true );
    return this.http.put<Contact>(this.apiUrl, contact).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  deleteContactById(id: string | number) {
    this.isLoading.set(true);
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  /**
   * Detects duplicates for contact records using the centralized ImportDialogService method
   */
  detectDuplicates(contactData: Contact): Observable<DuplicateDetectionResponse | null> {
    // Use the centralized duplicate detection method from ImportDialogService
    return this.importDialogService.detectDuplicatesForEntity(contactData, 'contact');
  }
}
