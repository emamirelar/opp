import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { OrganizationHierarchyTreeModel } from '../models/organization-hierarchy.model';

@Injectable({
  providedIn: 'root'
})
export class OrganizationHierarchyService {
  private apiUrl = '/api/organization-hierarchy';

  constructor(private http: HttpClient) { }

  getOrganizationHierarchy(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      map(response => {
        console.log('Service response:', response);
        return response;
      })
    );
  }
} 