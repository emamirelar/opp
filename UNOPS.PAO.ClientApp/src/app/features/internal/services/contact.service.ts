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

  getUrl(){
    return this.apiUrl
  }

  getAllContacts() {
    this.isLoading.set(true);
    this.http.get(this.apiUrl).subscribe({
      next: (data: any) => {
        console.log('Contact data loaded:', data);
        console.log('Contact data type:', typeof data);

        // Handle pagination response
        if (data && typeof data === 'object') {
          if (data.items && Array.isArray(data.items)) {
            console.log('Paginated response detected, items count:', data.items.length);
            if (data.items.length > 0) {
              console.log('Sample item:', data.items[0]);
              console.log('Keys:', Object.keys(data.items[0]));
            }
            this.contactData.set(data.items);
          } 
          // Handle array response
          else if (Array.isArray(data)) {
            console.log('Array response detected, length:', data.length);
            if (data.length > 0) {
              console.log('Sample item:', data[0]);
              console.log('Keys:', Object.keys(data[0]));
            }
            this.contactData.set(data);
          }
          // Special case for some API responses that return an object with numeric keys
          else if (Object.keys(data).every(key => !isNaN(Number(key)))) {
            const arrayData = Object.values(data);
            console.log('Converting object with numeric keys to array, length:', arrayData.length);
            this.contactData.set(arrayData);
          }
          // Try to extract the items from a wrapped response
          else if ('data' in data) {
            console.log('Found data property in response');
            const responseData = Array.isArray(data.data) ? data.data : [data.data];
            this.contactData.set(responseData);
          }
          else {
            console.log('Unknown data format, storing empty array');
            this.contactData.set([]);
          }
        } else {
          console.log('No valid data returned, storing empty array');
          this.contactData.set([]);
        }
        
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading contacts:', err);
        this.isLoading.set(false);
      },
    });
  }

  getContactById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get(`${this.apiUrl}/${recordId}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
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
