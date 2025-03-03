import { Injectable } from '@angular/core';
import {HttpClient, HttpResponse} from '@angular/common/http';
import { Observable } from 'rxjs';
import { Interaction } from '../models/interaction.model';

@Injectable({
  providedIn: 'root'
})
export class InteractionService {
  private apiUrl = `/api/interactions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<HttpResponse<Interaction[]>> {
    return this.http.get<Interaction[]>(this.apiUrl, { observe: 'response' });
  }

  getById(id: number): Observable<HttpResponse<Interaction>> {
    return this.http.get<Interaction>(`${this.apiUrl}/${id}`, { observe: 'response' });
  }

  create(interaction: Interaction): Observable<HttpResponse<Interaction>> {
    return this.http.post<Interaction>(this.apiUrl, interaction, { observe: 'response' });
  }

  update(id: number, interaction: Interaction): Observable<HttpResponse<Interaction>> {
    return this.http.put<Interaction>(`${this.apiUrl}`, interaction, { observe: 'response' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
