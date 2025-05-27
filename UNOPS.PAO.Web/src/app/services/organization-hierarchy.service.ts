import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OrganizationHierarchyTreeModel } from '../models/organization-hierarchy.model';

@Injectable({
  providedIn: 'root'
})
export class OrganizationHierarchyService {
  private apiUrl = `${environment.apiUrl}/organization-hierarchy`;

  constructor(private http: HttpClient) { }

  getOrganizationHierarchy(): Observable<OrganizationHierarchyTreeModel[]> {
    return this.http.get<OrganizationHierarchyTreeModel[]>(this.apiUrl);
  }
} 