import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {Partner} from '../models/partner.model';

@Injectable({
  providedIn: 'root',
})
export class PartnerService {
  http = inject(HttpClient);

  private partnerData = signal([]);
  allPartners = this.partnerData.asReadonly();
  isLoading = signal(false);

  public readonly apiUrl = `/api/partner`;

  constructor() { }

  getClassicSearchUrl(): string {
    return `${this.apiUrl}/classic-search`;
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

  createPartner( requestJson: object ){

    this.isLoading.set( true );
    return this.http.post(this.apiUrl, requestJson).pipe(tap(
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
}
