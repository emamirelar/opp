import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {Partner} from '../models/partner.model';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { PaginationResponse } from '@shared/models/pagination-response.model';
import { DuplicateDetectionResponse, ApprovalRequest, PartnerContactsResponse } from '@shared/models/api-responses.model';
import { Contact } from '@partnerships/contacts/models/contact.model';

@Injectable({
  providedIn: 'root',
})
export class PartnerService {
  http = inject(HttpClient);
  private importDialogService = inject(ImportDialogService);

  private partnerData = signal<Partner[]>([]);
  allPartners = this.partnerData.asReadonly();
  isLoading = signal(false);

  public readonly apiUrl = `/api/partner`;

  constructor() { }

  getClassicSearchUrl(): string {
    return `${this.apiUrl}`;
  }

  getAllPartners() {
    this.isLoading.set(true);
    this.http.get<PaginationResponse<Partner>>(`/api/partner`).subscribe({
      next: (data) => {
        this.partnerData.set(data.records);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }

  getPartnerById(recordId: string) : Observable<Partner> {
    this.isLoading.set(true);
    return this.http.get<Partner>(`${this.apiUrl}/${recordId}`).pipe(tap(
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
   * Creates a partner with duplicate detection handling
   * Returns either the created partner or duplicate detection response
   */
  createPartner( requestJson: Partner ): Observable<Partner | DuplicateDetectionResponse> {
    this.isLoading.set( true );
    return this.http.post<Partner | DuplicateDetectionResponse>(this.apiUrl, requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updatePartnerById( requestJson: Partner ){

    this.isLoading.set( true );
    return this.http.put<Partner>(this.apiUrl, requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  deletePartnerById(id: string | number) {
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

  getAllContactsById(recordId: string): Observable<Contact[]> {
    this.isLoading.set(true);
    return this.http.get<Contact[]>(`${this.apiUrl}/${recordId}/contacts`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  getUploadLogoUrl(recordId: string) {
    return `${this.apiUrl}/${recordId}/logo`;
  }
  
  approvePartner(requestJson: ApprovalRequest) {
    this.isLoading.set(true);
    return this.http.post<Partner>(`${this.apiUrl}/${requestJson.id}/approve`, requestJson).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  unapprovePartner(requestJson: any) {
    this.isLoading.set(true);
    return this.http.post(`${this.apiUrl}/${requestJson.id}/unapprove`, requestJson).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  activatePartner(id: string) {
    this.isLoading.set(true);
    return this.http.post<Partner>(`${this.apiUrl}/${id}/activate`, {}).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  closePartner(requestJson: any) {
    this.isLoading.set(true);
    return this.http.post<Partner>(`${this.apiUrl}/${requestJson.id}/close`, requestJson).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  archivePartner(requestJson: any) {
    this.isLoading.set(true);
    return this.http.post<Partner>(`${this.apiUrl}/${requestJson.id}/archive`, requestJson).pipe(tap(
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
   * Detects duplicates for partner records using the centralized ImportDialogService method
   */
  detectDuplicates(partnerData: Partner): Observable<DuplicateDetectionResponse | null> {
    // Use the centralized duplicate detection method from ImportDialogService
    return this.importDialogService.detectDuplicatesForEntity(partnerData, 'partner');
  }

  /**
   * Creates a new opportunity from a partner with the partner pre-populated
   * @param partnerId ID of the partner
   * @param request Request with opportunity name and partner role
   * @returns Created opportunity
   */
  createOpportunityFromPartner(partnerId: number, request: CreateOpportunityFromPartnerRequest): Observable<any> {
    this.isLoading.set(true);
    return this.http.post<any>(`${this.apiUrl}/${partnerId}/create-opportunity`, request).pipe(tap({
      next: () => {
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    }));
  }

  /**
   * Gets all opportunities related to a partner (funding or client partner)
   * @param partnerId ID of the partner
   * @returns List of related opportunities
   */
  getPartnerOpportunities(partnerId: number): Observable<any[]> {
    this.isLoading.set(true);
    return this.http.get<any[]>(`${this.apiUrl}/${partnerId}/opportunities`).pipe(tap({
      next: () => {
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    }));
  }
  
}

/**
 * Request interface for creating opportunity from partner
 */
export interface CreateOpportunityFromPartnerRequest {
  name: string;
  partnerRole: 'funding' | 'client' | 'both';
  description?: string;
}
