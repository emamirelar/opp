import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { DocumentLinkModel } from '../overrides/interfaces/types';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  http = inject( HttpClient );
  isLoading = signal(false);

  constructor() { }

  uploadUnopsFiles(formData: FormData) {
    this.isLoading.set( true );

    return this.http.post(`/api/unops/document/upload`, formData).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      }));
  }

  linkUnopsFiles(body: DocumentLinkModel) {
    this.isLoading.set( true );

    return this.http.post(`/api/unops/document/link`, body).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      }));
  }

  uploadFiles(formData: FormData) {
    this.isLoading.set( true );

    return this.http.post(`/api/document`, formData).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      }));
  }
}
