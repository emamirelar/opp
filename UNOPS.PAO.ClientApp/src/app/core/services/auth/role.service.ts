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

export interface DoaRoleAssignment {
  entityId: number;      // Organization hierarchy ID
  userId: number;        // User ID
  roleName: string;      // DOA Role Name ('DoA2' or 'DoA3') - backend looks up EntityRoleId
  entityType: string;    // Always 'OrganizationHierarchy'
}

export interface DoaRoleAssignmentResponse {
  success: boolean;
  message: string;
  assignedCount: number;
}

export interface ExistingDoaRole {
  id: number;
  entityId: number;
  orgUnitCode: string;
  orgUnitName: string;
  userId: number;
  userName: string;
  userEmail: string;
  entityRoleId: number;
  roleName: string;
  roleCode: string;
  createdDate: string;
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

  /**
   * Assigns DOA roles (DOA2 or DOA3) to users for specific organization hierarchies.
   * Inserts records into EntityUserRoles table.
   */
  assignDoaRoles(assignments: DoaRoleAssignment[]): Observable<DoaRoleAssignmentResponse> {
    return this.http.post<DoaRoleAssignmentResponse>(`${this.baseUrl}/assign-doa-roles`, assignments);
  }

  /**
   * Gets all existing DOA role assignments from EntityUserRoles table.
   */
  getDoaRoles(): Observable<ExistingDoaRole[]> {
    return this.http.get<ExistingDoaRole[]>(`${this.baseUrl}/doa-roles`);
  }

  /**
   * Deletes a DOA role assignment by ID.
   */
  deleteDoaRole(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.baseUrl}/doa-roles/${id}`);
  }
} 
