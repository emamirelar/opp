import { Injectable, inject } from '@angular/core';
import {HttpClient, HttpResponse, HttpParams} from '@angular/common/http';
import { Observable } from 'rxjs';
import { Interaction } from '../models/interaction.model';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { PaginationResponse } from '@shared/models/pagination-response.model';
import {PaginationParams, toHttpParams} from '@shared/models/pagination-params.model';
import { InteractionFilterParams } from '../models/interaction-filter-params.model';
import { map } from 'rxjs/operators';
import { DuplicateDetectionResponse } from '@shared/models/api-responses.model';

@Injectable({
  providedIn: 'root'
})
export class InteractionService {
  private apiUrl = `/api/interactions`;

  getClassicSearchUrl(): string {
    return this.apiUrl;
  }

  constructor(private http: HttpClient, private importDialogService: ImportDialogService) {}

  getAll(queryParams: InteractionFilterParams): Observable<HttpResponse<PaginationResponse<Interaction>>> {
    return this.http.get<PaginationResponse<Interaction>>(`${this.apiUrl}`, {
      params: toHttpParams(queryParams),
      observe: 'response'
    });
  }

  getById(id: number): Observable<HttpResponse<Interaction>> {
    return this.http.get<Interaction>(`${this.apiUrl}/${id}`, { observe: 'response' });
  }

  /**
   * Creates an interaction with duplicate detection handling
   * Returns either the created interaction or duplicate detection response
   */
  create(interaction: Interaction): Observable<HttpResponse<Interaction | DuplicateDetectionResponse>> {
    return this.http.post<Interaction | DuplicateDetectionResponse>(this.apiUrl, interaction, { observe: 'response' });
  }

  update(interaction: Interaction): Observable<HttpResponse<Interaction>> {
    return this.http.put<Interaction>(`${this.apiUrl}`, interaction, { observe: 'response' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Detects duplicates for interaction records using the centralized ImportDialogService method
   */
  detectDuplicates(interactionData: Interaction): Observable<DuplicateDetectionResponse | null> {
    // Use the centralized duplicate detection method from ImportDialogService
    return this.importDialogService.detectDuplicatesForEntity(interactionData, 'interaction');
  }
}
