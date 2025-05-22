import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Role {
  name: string;
  description: string;
}

export interface UserRoles {
  userId: string;
  email: string;
  roles: Role[];
}

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  constructor(private http: HttpClient) {}

  getUserRoles(userId: string): Observable<UserRoles> {
    return this.http.get<UserRoles>(`/api/permissions/user/${userId}`);
  }
} 