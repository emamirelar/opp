import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { DocumentLinkModel } from '../interfaces/document.interface';

@Injectable({
  providedIn: 'root',
})
export class DocumentService {
  http = inject(HttpClient);
  isLoading = signal(false);

  constructor() {}

  getDocumentTypesByEntityName(entityName: string) {
    this.isLoading.set(true);
    return this.http.get('/api/document-type/' + entityName).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }

  getDocuments(entityName: string, entityId: string) {
    this.isLoading.set(true);
    return this.http.get('/api/document/' + entityName + '/' + entityId).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }

  uploadFile(formData: FormData) {
    this.isLoading.set(true);

    return this.http.post(`/api/document/upload`, formData).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }

  linkFile(body: DocumentLinkModel) {
    this.isLoading.set(true);

    return this.http.post(`/api/document/link`, body).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }

  delete(documentId: number) {
    this.isLoading.set(true);

    return this.http.delete('/api/document/' + documentId).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }

  download(documentId: number) {
    this.isLoading.set(true);

    return this.http.get('/api/document/download/' + documentId, { responseType: 'blob' }).pipe(
      tap({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
        },
      })
    );
  }
}
