import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { GeminiResponse, GeminiType } from '../models/gemini.models';

@Injectable({
  providedIn: 'root',
})
export class GeminiService {
  private http = inject(HttpClient);

  private readonly API_URL = '/api/process-data';

  get(partnerId: string, type: GeminiType): Observable<string> {
    return this.http.post<GeminiResponse>(this.API_URL, { partnerId, type }).pipe(
      map(response => {
        if (!response.candidates?.[0]?.content?.parts) {
          return '';
        }
        return response.candidates[0].content.parts
          .map(part => part.text)
          .join('');
      })
    );
  }
}
