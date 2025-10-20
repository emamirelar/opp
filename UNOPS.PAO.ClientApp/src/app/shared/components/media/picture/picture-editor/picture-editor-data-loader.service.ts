import { HttpClient, HttpEventType, HttpEvent, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal, computed } from '@angular/core';
import { Observable, catchError, of, tap, map, filter } from 'rxjs';

interface UploadResponse {
  imageUrl: string;
}

@Injectable()
export class PictureEditorDataLoaderService {
  private http = inject(HttpClient);
  
  // State signals
  private loadingState = signal<boolean>(false);
  private errorState = signal<boolean>(false);
  private progressState = signal<number>(0);
  private uploadUrlState = signal<string>('');
  
  // Computed signals
  isLoading = computed(() => this.loadingState());
  hasError = computed(() => this.errorState());
  uploadProgress = computed(() => this.progressState());
  
  /**
   * Set the API endpoint URL for uploading images
   * Expected format: '/api/contact/123/profile-picture'
   */
  setUploadUrl(url: string): void {
    this.uploadUrlState.set(url);
  }
  
  /**
   * Upload image to the server
   */
  uploadImage(file: File): Observable<string> {
    const url = this.uploadUrlState();
    if (!url) {
      console.error('Upload URL not set');
      return of('');
    }
    
    this.loadingState.set(true);
    this.errorState.set(false);
    this.progressState.set(0);
    
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<any>(url, formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      tap(event => {
        // Handle upload progress events
        if (event.type === HttpEventType.UploadProgress && event.total) {
          this.progressState.set(Math.round(100 * event.loaded / event.total));
        }
        
        // Handle response event
        if (event.type === HttpEventType.Response) {
          this.loadingState.set(false);
          this.progressState.set(100);
        }
      }),
      // Filter to get only the full response
      filter((event): event is HttpResponse<UploadResponse> => event.type === HttpEventType.Response),
      // Extract the returned URL from the response
      map(response => response.body?.imageUrl || ''),
      catchError(err => {
        this.loadingState.set(false);
        this.errorState.set(true);
        console.error('Error uploading image:', err);
        return of('');
      })
    );
  }
} 
