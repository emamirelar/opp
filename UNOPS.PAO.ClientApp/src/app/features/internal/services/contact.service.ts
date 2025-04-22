import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Contact } from '../models/contact.model';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  http = inject(HttpClient);

  public readonly apiUrl = `/api/contact`;
  private contactData = signal<any[]>([]);
  allContacts = this.contactData.asReadonly();

  isLoading = signal(false);

  constructor() { }

  getAll(params: any): Observable<any> {
    this.isLoading.set(true);
    return this.http.get<any>(this.apiUrl, { observe: 'response', params })
      .pipe(
        tap(() => this.isLoading.set(false))
      );
  }

  getUrl(){
    return this.apiUrl
  }

  getUploadProfilePictureUrl(contactId: string): string {
    return `${this.apiUrl}/${contactId}/profile-picture`;
  }

  getAllContacts() {
    this.isLoading.set(true);
    this.http.get(this.apiUrl).subscribe({
      next: (data: any) => {
        this.contactData.set(data.records);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading contacts:', err);
        this.isLoading.set(false);
      },
    });
  }

  getContactById(id: string): Observable<Contact> {
    this.isLoading.set(true);
    return this.http.get<Contact>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => this.isLoading.set(false))
      );
  }

  createContact( contact: Contact ): Observable<Contact> {

    this.isLoading.set( true );
    return this.http.post<Contact>(this.apiUrl, contact).pipe(tap(
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
    return this.http.put(this.apiUrl, contact).pipe(tap(
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
}
