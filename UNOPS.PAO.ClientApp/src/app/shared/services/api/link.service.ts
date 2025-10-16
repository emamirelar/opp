import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Link, LinkRequest, UpdateLinkRequest, EntityType } from '../../models/link.model';
import {PaginationResponse} from '../../models/pagination-response.model';

@Injectable({
  providedIn: 'root'
})
export class LinkService {
  private apiUrl = `/api/links`;

  constructor(private http: HttpClient) {}

  getAll(
    entity?: EntityType,
    entityId?: number,
    pageIndex: number = 1,
    pageSize: number = 10,
    orderBy?: string,
    ascending?: boolean
  ): Observable<HttpResponse<PaginationResponse<Link>>> {
    let params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    if (entity) params = params.set('entity', entity);
    if (entityId) params = params.set('entityId', entityId.toString());
    if (orderBy) params = params.set('orderBy', orderBy);
    if (ascending !== undefined) params = params.set('ascending', ascending.toString());

    return this.http.get<PaginationResponse<Link>>(this.apiUrl, {
      params,
      observe: 'response'
    });
  }

  create(link: LinkRequest): Observable<HttpResponse<Link>> {
    return this.http.post<Link>(this.apiUrl, link, { observe: 'response' });
  }

  update(link: UpdateLinkRequest): Observable<HttpResponse<void>> {
    return this.http.put<void>(this.apiUrl, link, { observe: 'response' });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}?id=${id}`);
  }
}
