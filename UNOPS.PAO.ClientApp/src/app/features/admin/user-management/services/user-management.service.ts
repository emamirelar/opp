import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

interface UserManagementModel {
  userId: number;
  name: string;
  email: string;
  orgUnit: string;
  orgUnitDescription?: string;
  roles: string[];
  rolesDisplay: string;
  lastModifiedDate?: Date;
  isActive: boolean;
}

interface RoleModel {
  id: number;
  name: string;
  description: string;
}

interface OrgUnitModel {
  id: number;
  name: string;
  description?: string;
  code: string;
}

interface UserManagementRequest {
  pageIndex: number;
  pageSize: number;
  searchTerm?: string;
  roleFilter?: string[];
  showMyOrgUnitOnly: boolean;
  orgUnitFilter?: number[];
  sortBy?: string;
  sortDirection?: string;
}

interface UpdateUserRolesRequest {
  roles: string[];
}

interface UpdateOrgUnitSelfManagementRequest {
  isSelfManagementEnabled: boolean;
}

interface PaginationResponse<T> {
  records: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private http = inject(HttpClient);
  private readonly baseUrl = '/api/user-management';

  async getUsers(request: any): Promise<PaginationResponse<UserManagementModel>> {
    const response = await firstValueFrom(
      this.http.post<PaginationResponse<UserManagementModel>>(`${this.baseUrl}/users`, request)
    );
    return response;
  }

  async getUserById(userId: number): Promise<UserManagementModel> {
    const response = await firstValueFrom(
      this.http.get<UserManagementModel>(`${this.baseUrl}/users/${userId}`)
    );
    return response;
  }

  async updateUserRoles(userId: number, request: UpdateUserRolesRequest): Promise<UserManagementModel> {
    const response = await firstValueFrom(
      this.http.put<UserManagementModel>(`${this.baseUrl}/users/${userId}/roles`, request)
    );
    return response;
  }

  async getAvailableRoles(): Promise<RoleModel[]> {
    const response = await firstValueFrom(
      this.http.get<RoleModel[]>(`${this.baseUrl}/roles`)
    );
    return response;
  }

  async getCurrentUserOrgUnit(): Promise<string> {
    const response = await firstValueFrom(
      this.http.get<string>(`${this.baseUrl}/current-user-org-unit`)
    );
    return response;
  }

  async updateOrgUnitSelfManagement(orgUnitCode: string, request: UpdateOrgUnitSelfManagementRequest): Promise<void> {
    await firstValueFrom(
      this.http.put<void>(`${this.baseUrl}/org-units/${orgUnitCode}/self-management`, request)
    );
  }

  async getOrgUnitSelfManagementStatus(orgUnitCode: string): Promise<boolean> {
    const response = await firstValueFrom(
      this.http.get<{ isSelfManagementEnabled: boolean }>(`${this.baseUrl}/org-units/${orgUnitCode}/self-management`)
    );
    return response.isSelfManagementEnabled;
  }

  async analyzeUserRoleFile(fileId: string, type: string): Promise<any> {
    const payload = { type, fileId };
    const response = await firstValueFrom(
      this.http.post<any>(`${this.baseUrl}/analyse-file`, payload)
    );
    return response;
  }

  async bulkUploadUserRoles(records: any[], type: string): Promise<any> {
    const payload = { type, records };
    const response = await firstValueFrom(
      this.http.post<any>(`${this.baseUrl}/bulk-upload`, payload)
    );
    return response;
  }

  async resolveUserIds(userIds: number[]): Promise<{[key: number]: {name: string, email: string}}> {
    const response = await firstValueFrom(
      this.http.post<{[key: number]: {name: string, email: string}}>(`${this.baseUrl}/resolve-users`, { userIds })
    );
    return response;
  }

  async resolveRoleIds(roleIds: number[]): Promise<{[key: number]: {name: string, description: string}}> {
    const response = await firstValueFrom(
      this.http.post<{[key: number]: {name: string, description: string}}>(`${this.baseUrl}/resolve-roles`, { roleIds })
    );
    return response;
  }

  async getAvailableOrgUnits(): Promise<OrgUnitModel[]> {
    const response = await firstValueFrom(
      this.http.get<OrgUnitModel[]>(`${this.baseUrl}/org-units`)
    );
    return response;
  }
} 
