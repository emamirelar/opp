import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { GeminiResponse, GeminiType } from '../models/gemini.model';


@Injectable({
  providedIn: 'root',
})
export class GeminiService {
  private readonly apiUrl = '/api';

  constructor(private http: HttpClient) {}

  // Original method for Gemini process-data
  get(id: string, type: GeminiType): Observable<string> {
    return this.http.post<GeminiResponse>(`${this.apiUrl}/process-data`, { id, type }, { observe: 'response' }).pipe(
      map(response => {
        if (!response.body?.candidates?.[0]?.content?.parts) {
          return '';
        }
        return response.body?.candidates[0].content.parts
          .map(part => part.text.replace('```markdown\n',''))
          .join('');
      })
    );
  }

  // Method to scan files using Gemini
  scanFile(file: File, type: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('type', type);
    return this.http.post(`${this.apiUrl}/scan-data`, formData)
      .pipe(map(this.parseGeminiResponseToJson));
  }

  private parseGeminiResponseToJson(body: any) {
    if (!body.candidates?.[0]?.content?.parts?.[0]?.text) {
      return '';
    }
    const cleanedMessage = body?.candidates?.[0]?.content?.parts?.[0]?.text
      .replace(/^```json\s*/, '')
      .replace(/```/, '');
    return JSON.parse(cleanedMessage);
  }
}
