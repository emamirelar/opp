import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { DocumentLinkModel } from '@app/shared/interfaces';

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

  /**
   * Get documents by entity type and ID (for Opportunity feature)
   */
  getDocumentsByEntity(entityType: string, entityId: number) {
    this.isLoading.set(true);
    return this.http.get(`/api/document/entity/${entityType}/${entityId}`).pipe(
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

  /**
   * Upload document (for Opportunity feature)
   */
  uploadDocument(file: File, parentEntityName: string, parentEntityId: number, documentTypeId?: number) {
    const formData = new FormData();
    formData.append('File', file);
    formData.append('ParentEntityName', parentEntityName);
    formData.append('ParentEntityId', parentEntityId.toString());
    if (documentTypeId) {
      formData.append('DocumentTypeId', documentTypeId.toString());
    }
    return this.uploadFile(formData);
  }

  /**
   * Link document (for Opportunity feature)
   */
  linkDocument(link: string, googleId: string, parentEntityName: string, parentEntityId: number, documentTypeId?: number) {
    const body: DocumentLinkModel = {
      link,
      googleId,
      name: '',
      type: '',
      parentEntityName,
      parentEntityId,
      documentTypeId: documentTypeId || 0
    };
    return this.linkFile(body);
  }

  /**
   * Delete document by ID (for Opportunity feature)
   */
  deleteDocument(documentId: number) {
    return this.delete(documentId);
  }

  /**
   * Download document by ID (for Opportunity feature)
   */
  downloadDocument(documentId: number) {
    return this.download(documentId);
  }

  /**
   * Transcribe document using AI (for Opportunity feature)
   */
  transcribeDocument(documentId: number) {
    this.isLoading.set(true);
    return this.http.post(`/api/document-transcribe`, { id: documentId }).pipe(
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

  /**
   * Get viewable URL for a document (signed URL for GCS documents)
   */
  getDocumentViewUrl(documentId: number) {
    this.isLoading.set(true);
    return this.http.get<{ url: string; type: string; mimeType?: string }>(`/api/document/view-url/${documentId}`).pipe(
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

  /**
   * Convert markdown to Google Doc via AI API. Returns the Google Doc URL.
   */
  convertMarkdownToDoc(markdownContent = '# Test Document\n\nThis is sample **markdown** content.') {
    this.isLoading.set(true);
    const body = { data: markdownContent, filename: 'Generated_Document' };
    return this.http
      .post<{ googleDocUrl: string }>(`/api/document/convert-markdown-to-doc`, body)
      .pipe(
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

  /**
   * Get partner-document associations for a specific document
   */
  getPartnerDocumentAssociation(documentId: number) {
    this.isLoading.set(true);
    return this.http.get<{ documentId: number; partners: Array<{ partnerId: number; partnerType: string }> }>(`/api/opportunity/retrieve-partner-document-association/${documentId}`).pipe(
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
