import { Injectable } from '@angular/core';
import {HttpClient, HttpResponse, HttpParams} from '@angular/common/http';
import { Observable } from 'rxjs';
import { Interaction } from '../models/interaction.model';
import { PaginationResponse } from '../../../common/models/pagination-response.model';
import {PaginationParams, toHttpParams} from '../../../common/models/pagination-params.model';

@Injectable({
  providedIn: 'root'
})
export class InteractionService {
  private apiUrl = `/api/interactions`;

  constructor(private http: HttpClient) {}

  getAll(queryParams: PaginationParams): Observable<HttpResponse<PaginationResponse<Interaction>>> {
    return this.http.get<PaginationResponse<Interaction>>(`${this.apiUrl}`, {
      params: toHttpParams(queryParams),
      observe: 'response'
    });
  }

  getById(id: number): Observable<HttpResponse<Interaction>> {
    return this.http.get<Interaction>(`${this.apiUrl}/${id}`, { observe: 'response' });
  }

  create(interaction: Interaction): Observable<HttpResponse<Interaction>> {
    return this.http.post<Interaction>(this.apiUrl, interaction, { observe: 'response' });
  }

  update(interaction: Interaction): Observable<HttpResponse<Interaction>> {
    return this.http.put<Interaction>(`${this.apiUrl}`, interaction, { observe: 'response' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
