import { Component, OnInit, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';

// PrimeNG imports
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { MultiSelectModule } from 'primeng/multiselect';
import { PaginatorModule } from 'primeng/paginator';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CheckboxModule } from 'primeng/checkbox';
import { ChipModule } from 'primeng/chip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

import { MessageService, ConfirmationService } from 'primeng/api';
import { UserManagementService } from './user-management.service';
import { PermissionService, EntityPermissions } from '../../../../essentials/services/permission.service';

interface UserManagementModel {
  userId: number;
  name: string;
  email: string;
  orgUnit: string;
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

interface UserManagementRequest {
  pageIndex: number;
  pageSize: number;
  searchTerm?: string;
  roleFilter?: string;
  showMyOrgUnitOnly: boolean;
  orgUnitFilter?: string;
  sortBy?: string;
  sortDirection?: string;
}

interface PaginationResponse<T> {
  records: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    DialogModule,
    MultiSelectModule,
    PaginatorModule,
    ToastModule,
    ConfirmDialogModule,
    CheckboxModule,
    ChipModule,
    ProgressSpinnerModule,
    TooltipModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  private userManagementService = inject(UserManagementService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private permissionService = inject(PermissionService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  // Permission signals
  entityPermissions = signal<EntityPermissions>({
    entity: 'UserManagement',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false
    }
  });
  permissionsLoading = signal<boolean>(true);

  // Signals for reactive state management
  users = signal<UserManagementModel[]>([]);
  totalRecords = signal<number>(0);
  loading = signal<boolean>(false);
  availableRoles = signal<RoleModel[]>([]);
  
  // Dialog state
  editDialogVisible = signal<boolean>(false);
  selectedUser = signal<UserManagementModel | null>(null);
  selectedUserRoles = signal<string[]>([]);

  // Filter and pagination state
  searchTerm = signal<string>('');
  roleFilter = signal<string>('');
  showMyOrgUnitOnly = signal<boolean>(false);
  orgUnitFilter = signal<string>('');
  
  first = signal<number>(0);
  rows = signal<number>(50);
  sortBy = signal<string>('name');
  sortDirection = signal<string>('asc');

  // Computed values
  roleOptions = computed(() => 
    this.availableRoles().map(role => ({ label: role.name, value: role.name }))
  );

  // Permission computed values
  canRead = computed(() => this.entityPermissions().permissions.canRead);
  canUpdate = computed(() => this.entityPermissions().permissions.canUpdate);
  hasAccess = computed(() => this.entityPermissions().hasAccess);

  ngOnInit() {
    this.loadPermissions();
  }

  private loadPermissions() {
    this.permissionsLoading.set(true);
    
    // Clear cache before loading to ensure fresh permissions
    this.permissionService.clearPermissionCaches();
    
    // Get current route path for permission checking
    const currentPath = this.router.url;
    
    // Load from server (cache was cleared above)
    this.permissionService.getEntityPermissions(currentPath)
      .subscribe({
        next: (permissions) => {
          if (!permissions.hasAccess) {
            console.log(`[USER-MANAGEMENT] No access to user management for route ${currentPath}`);
            this.router.navigate(['/access-denied']);
            return;
          }
          console.log(`[USER-MANAGEMENT] Loaded user management permissions for route ${currentPath}:`, permissions);
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          
          // Load data only after permissions are confirmed
          if (permissions.hasAccess) {
            this.loadAvailableRoles();
            this.loadUsers();
          }
          
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading user management permissions:', error);
          this.permissionsLoading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Access Error',
            detail: 'Unable to verify permissions for user management'
          });
          this.cdr.detectChanges();
        }
      });
  }

  async loadUsers() {
    this.loading.set(true);
    try {
      const request: UserManagementRequest = {
        pageIndex: Math.floor(this.first() / this.rows()),
        pageSize: this.rows(),
        searchTerm: this.searchTerm() || undefined,
        roleFilter: this.roleFilter() || undefined,
        showMyOrgUnitOnly: this.showMyOrgUnitOnly(),
        orgUnitFilter: this.orgUnitFilter() || undefined,
        sortBy: this.sortBy(),
        sortDirection: this.sortDirection()
      };

      const response: PaginationResponse<UserManagementModel> = await this.userManagementService.getUsers(request);
      this.users.set(response.records);
      this.totalRecords.set(response.totalCount);
    } catch (error) {
      console.error('Error loading users:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load users'
      });
    } finally {
      this.loading.set(false);
    }
  }

  async loadAvailableRoles() {
    try {
      const roles: RoleModel[] = await this.userManagementService.getAvailableRoles();
      this.availableRoles.set(roles);
    } catch (error) {
      console.error('Error loading roles:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load available roles'
      });
    }
  }

  onPageChange(event: any) {
    this.first.set(event.first);
    this.rows.set(event.rows);
    this.loadUsers();
  }

  onSort(event: any) {
    this.sortBy.set(event.field);
    this.sortDirection.set(event.order === 1 ? 'asc' : 'desc');
    this.loadUsers();
  }

  onSearch() {
    this.first.set(0);
    this.loadUsers();
  }

  onFilterChange() {
    this.first.set(0);
    this.loadUsers();
  }

  clearFilters() {
    this.searchTerm.set('');
    this.roleFilter.set('');
    this.showMyOrgUnitOnly.set(false);
    this.orgUnitFilter.set('');
    this.first.set(0);
    this.loadUsers();
  }

  editUser(user: UserManagementModel) {
    this.selectedUser.set(user);
    this.selectedUserRoles.set([...user.roles]);
    this.editDialogVisible.set(true);
  }

  async saveUserRoles() {
    const user = this.selectedUser();
    if (!user) return;

    try {
      const request = {
        roles: this.selectedUserRoles()
      };

      const updatedUser: UserManagementModel = await this.userManagementService.updateUserRoles(user.userId, request);
      
      // Update the user in the list
      const currentUsers = this.users();
      const userIndex = currentUsers.findIndex(u => u.userId === user.userId);
      if (userIndex !== -1) {
        currentUsers[userIndex] = updatedUser;
        this.users.set([...currentUsers]);
      }

      this.editDialogVisible.set(false);
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'User roles updated successfully'
      });
    } catch (error) {
      console.error('Error updating user roles:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to update user roles'
      });
    }
  }

  cancelEdit() {
    this.editDialogVisible.set(false);
    this.selectedUser.set(null);
    this.selectedUserRoles.set([]);
  }

  getRoleSeverity(role: string): string {
    switch (role) {
      case 'PARTNER_GLOB_ADMIN':
        return 'danger';
      case 'ORG_UNIT_ADMIN':
        return 'warning';
      case 'PARTNER_USER':
        return 'info';
      default:
        return 'secondary';
    }
  }

  getStatusSeverity(isActive: boolean): string {
    return isActive ? 'success' : 'danger';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }
} 