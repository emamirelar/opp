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
    let entity = type.split('_')[0];
    return this.http.post(`${this.apiUrl}/${entity}/scan-data`, formData)
      .pipe(map(this.parseGeminiResponseToJson));
  }

  private parseGeminiResponseToJson(body: any) {
    // Handle the new direct JSON response format from our enhanced AI Transcribe
    if (typeof body === 'string') {
      try {
        // If it's a string that looks like JSON, parse it
        const trimmed = body.trim();
        if (trimmed.startsWith('{') && trimmed.endsWith('}')) {
          return JSON.parse(trimmed);
        }
        // If it's wrapped in markdown code blocks, clean it
        const cleanedMessage = trimmed
          .replace(/^```json\s*/, '')
          .replace(/```$/, '');
        return JSON.parse(cleanedMessage);
      } catch (e) {
        console.error('Failed to parse JSON response:', e);
        return '';
      }
    }
    
    // Handle the old Gemini API format (fallback)
    if (!body.candidates?.[0]?.content?.parts?.[0]?.text) {
      return body; // Return as-is if it's already an object
    }
    const cleanedMessage = body?.candidates?.[0]?.content?.parts?.[0]?.text
      .replace(/^```json\s*/, '')
      .replace(/```/, '');
    return JSON.parse(cleanedMessage);
  }
}
