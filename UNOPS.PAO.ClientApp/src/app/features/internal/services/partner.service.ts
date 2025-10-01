import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {Partner} from '../models/partner.model';
import { ImportDialogService } from '../../../common/reusables/components/import/dialog/import-dialog.service';

@Injectable({
  providedIn: 'root',
})
export class PartnerService {
  http = inject(HttpClient);
  private importDialogService = inject(ImportDialogService);

  private partnerData = signal([]);
  allPartners = this.partnerData.asReadonly();
  isLoading = signal(false);

  public readonly apiUrl = `/api/partner`;

  constructor() { }

  getClassicSearchUrl(): string {
    return `${this.apiUrl}`;
  }

  getAllPartners() {
    this.isLoading.set(true);
    this.http.get(`/api/partner`).subscribe({
      next: (data: any) => {
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
  createPartner( requestJson: object ): Observable<any> {
    this.isLoading.set( true );
    return this.http.post<any>(this.apiUrl, requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updatePartnerById( requestJson: any ){

    this.isLoading.set( true );
    return this.http.put(this.apiUrl, requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  deletePartnerById(id: any) {
    this.isLoading.set(true);
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  getAllContactsById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get(`${this.apiUrl}/${recordId}/contacts`).pipe(tap(
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
  
  approvePartner(requestJson: any) {
    this.isLoading.set(true);
    return this.http.post(`${this.apiUrl}/${requestJson.id}/approve`, requestJson).pipe(tap(
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
    return this.http.post(`${this.apiUrl}/${id}/activate`, {}).pipe(tap(
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
  detectDuplicates(partnerData: any): Observable<any> {
    // Use the centralized duplicate detection method from ImportDialogService
    return this.importDialogService.detectDuplicatesForEntity(partnerData, 'partner');
  }
  
}
