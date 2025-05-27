import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Role {
  id: number;
  name: string;
}

export interface UserRoles {
  email: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private baseUrl = 'api/role';

  constructor(private http: HttpClient) { }

  getAllRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(`${this.baseUrl}/all`);
  }

  getUserRoles(): Observable<UserRoles> {
    return this.http.get<UserRoles>(`${this.baseUrl}/user`);
  }

  updateUserRoles(roles: string[]): Observable<any> {
    return this.http.put(`${this.baseUrl}/update`, roles);
  }
} 