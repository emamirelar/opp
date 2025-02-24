import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  http = inject(HttpClient);

  private contactData = signal([]);
  allContacts = this.contactData.asReadonly();
  isLoading = signal(false);

  constructor() { }

  getAllContacts() {
    this.isLoading.set(true);
    this.http.get(`/api/contact`).subscribe({
      next: (data: any) => {
        this.contactData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }

  getContactById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get(`/api/contact/${recordId}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  createContact( requestJson: object ){

    this.isLoading.set( true );
    return this.http.post('/api/contact', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updateContactById( requestJson: any ){

    this.isLoading.set( true );
    return this.http.put('/api/contact', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  deleteContactById(id: any) {
    this.isLoading.set(true);
    return this.http.delete(`/api/contact/${id}`).pipe(tap(
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