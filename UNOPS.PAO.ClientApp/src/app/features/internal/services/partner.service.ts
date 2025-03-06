import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PartnerService {
  http = inject(HttpClient);

  private partnerData = signal([]);
  allPartners = this.partnerData.asReadonly();
  isLoading = signal(false);

  constructor() { }

  getAllPartners() {
    this.isLoading.set(true);
    this.http.get(`/api/partner`).subscribe({
      next: (data: any) => {
        this.partnerData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }

  getPartnerById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get(`/api/partner/${recordId}`).pipe(tap(
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
    return this.http.post('/api/partner', requestJson).pipe(tap(
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
    return this.http.put('/api/partner', requestJson).pipe(tap(
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
    return this.http.delete(`/api/partner/${id}`).pipe(tap(
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
    return this.http.get('/api/partner/' + recordId + '/contacts').pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }
}
