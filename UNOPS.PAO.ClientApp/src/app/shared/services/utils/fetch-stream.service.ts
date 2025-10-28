import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class FetchStreamService {

  constructor(private httpClient: HttpClient) {}

  /**
   * Creates a streaming fetch request that applies Angular HTTP interceptors
   * @param url The URL to make the request to
   * @param options Request options including method, body, headers
   * @returns Observable that emits streaming data chunks
   */
  streamRequest(url: string, options: {
    method?: string;
    body?: any;
    headers?: { [key: string]: string };
  } = {}): Observable<string> {
    
    // First, get the intercepted headers by making a HEAD request through HttpClient
    return this.httpClient.request('HEAD', url, {
      headers: options.headers,
      observe: 'response'
    }).pipe(
      switchMap((response) => {
        // Extract headers from the intercepted request
        const interceptedHeaders: { [key: string]: string } = {};
        
        // Get common auth headers that interceptors typically add
        const authHeader = response.headers.get('Authorization');
        if (authHeader) {
          interceptedHeaders['Authorization'] = authHeader;
        }

        // Create the streaming observable
        return new Observable<string>(observer => {
          this.performFetchStream(url, options, interceptedHeaders, observer);
          
          // Return cleanup function
          return () => {
            // Cleanup will be handled in performFetchStream
          };
        });
      })
    );
  }

  private async performFetchStream(
    url: string, 
    options: any, 
    interceptedHeaders: { [key: string]: string },
    observer: any
  ) {
    let abortController: AbortController | null = null;
    
    try {
      abortController = new AbortController();
      
      // Merge original headers with intercepted headers
      const headers = {
        'Accept': 'text/event-stream',
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0',
        ...options.headers,
        ...interceptedHeaders
      };

      const response = await fetch(url, {
        method: options.method || 'POST',
        headers,
        body: options.body,
        credentials: 'include', // Include cookies for authentication
        signal: abortController.signal
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const reader = response.body?.getReader();
      if (!reader) {
        throw new Error('Response body is not readable');
      }

      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        
        if (done) {
          // console.log('🌊 [FETCH-STREAM] Stream completed');
          observer.complete();
          break;
        }

        // Decode the chunk and add to buffer
        const chunk = decoder.decode(value, { stream: true });
        buffer += chunk;

        // Process complete lines (SSE format)
        const lines = buffer.split('\n');
        buffer = lines.pop() || ''; // Keep incomplete line in buffer

        for (const line of lines) {
          if (line.trim()) {
            observer.next(line);
          }
        }
      }
    } catch (error) {
      if (error instanceof Error && error.name === 'AbortError') {
        console.log('🌊 [FETCH-STREAM] Stream aborted');
        observer.complete();
      } else {
        console.error('🌊 [FETCH-STREAM] Error:', error);
        observer.error(error);
      }
    }
  }
}
