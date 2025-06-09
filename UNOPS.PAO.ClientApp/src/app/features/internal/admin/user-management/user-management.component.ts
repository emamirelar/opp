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
import { AuthService } from '../../../../essentials/services/auth.service';

interface UserManagementModel {
  userId: number;
  name: string;
  email: string;
  orgUnit: string;
  orgUnitCode?: string;
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
  private authService = inject(AuthService);

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
  isPartnerUser = signal<boolean>(false);
  isSelfManagementEnabled = signal<boolean>(false);

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

  // Computed value for other roles (excluding PARTNER_USER)
  otherUserRoles = computed(() => {
    const user = this.selectedUser();
    if (!user) return [];
    return user.roles.filter(role => role !== 'PARTNER_USER');
  });

  // Permission computed values
  canRead = computed(() => this.entityPermissions().permissions.canRead);
  canUpdate = computed(() => this.entityPermissions().permissions.canUpdate);
  hasAccess = computed(() => this.entityPermissions().hasAccess);

  // Current user role signals
  currentUserRoles = signal<string[]>([]);
  
  // Computed values for role-based UI logic
  isOrgUnitAdmin = computed(() => 
    this.currentUserRoles().includes('ORG_UNIT_ADMIN') && 
    !this.currentUserRoles().includes('PARTNER_GLOB_ADMIN')
  );
  
  isOrgUnitFilterDisabled = computed(() => this.isOrgUnitAdmin());

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
            
            this.router.navigate(['/access-denied']);
            return;
          }
          
          
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          
          // Load data only after permissions are confirmed
          if (permissions.hasAccess) {
            // Load current user roles first, then load other data
            this.loadCurrentUserRoles().then(() => {
              // After roles are loaded, load the rest of the data
              this.loadAvailableRoles();
              this.loadUsers(); // This will now use the correct showMyOrgUnitOnly setting
            });
          }
          
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading role impersonation permissions:', error);
          this.permissionsLoading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Access Error',
            detail: 'Unable to verify permissions for role impersonation'
          });
          this.cdr.detectChanges();
        }
      });
  }

  private async loadCurrentUserRoles(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.authService.getUserRoles().subscribe({
        next: (roles) => {
          this.currentUserRoles.set(roles);
          
          // If user is ORG_UNIT_ADMIN (but not PARTNER_GLOB_ADMIN), automatically enable org unit filtering
          if (this.isOrgUnitAdmin()) {
            this.showMyOrgUnitOnly.set(true);
            
          }
          
          this.cdr.detectChanges();
          resolve();
        },
        error: (error) => {
          console.error('Error loading current user roles:', error);
          reject(error);
        }
      });
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
    
    // Only reset org unit filter if user is not ORG_UNIT_ADMIN
    if (!this.isOrgUnitAdmin()) {
      this.showMyOrgUnitOnly.set(false);
    }
    
    this.orgUnitFilter.set('');
    this.first.set(0);
    this.loadUsers();
  }

  editUser(user: UserManagementModel) {
    this.selectedUser.set(user);
    this.isPartnerUser.set(user.roles.includes('PARTNER_USER'));
    this.loadOrgUnitSelfManagementStatus(user.orgUnitCode || user.orgUnit);
    this.editDialogVisible.set(true);
  }

  private async loadOrgUnitSelfManagementStatus(orgUnitCode: string) {
    // ORG_UNIT_ADMIN users cannot modify organization self-management settings
    if (this.isOrgUnitAdmin()) {
      this.isSelfManagementEnabled.set(false);
      return;
    }
    
    try {
      const status = await this.userManagementService.getOrgUnitSelfManagementStatus(orgUnitCode);
      this.isSelfManagementEnabled.set(status);
    } catch (error) {
      console.error('Error loading org unit self-management status:', error);
      this.isSelfManagementEnabled.set(false);
    }
  }

  async saveUserRoles() {
    const user = this.selectedUser();
    if (!user) return;

    try {
      // Get current roles excluding PARTNER_USER
      const otherRoles = user.roles.filter(role => role !== 'PARTNER_USER');
      
      // Build new roles array: keep other roles and add PARTNER_USER if checked
      const newRoles = this.isPartnerUser() 
        ? [...otherRoles, 'PARTNER_USER']
        : otherRoles;

      const request = {
        roles: newRoles
      };

      // Update user roles
      const updatedUser: UserManagementModel = await this.userManagementService.updateUserRoles(user.userId, request);
      
      // Update organization unit self-management setting only for PARTNER_GLOB_ADMIN users
      if (!this.isOrgUnitAdmin()) {
        const orgUnitCode = user.orgUnitCode || user.orgUnit;
        if (orgUnitCode) {
          await this.userManagementService.updateOrgUnitSelfManagement(orgUnitCode, {
            isSelfManagementEnabled: this.isSelfManagementEnabled()
          });
        }
      }
      
      // Update the user in the list
      const currentUsers = this.users();
      const userIndex = currentUsers.findIndex(u => u.userId === user.userId);
      if (userIndex !== -1) {
        currentUsers[userIndex] = updatedUser;
        this.users.set([...currentUsers]);
      }

      this.editDialogVisible.set(false);
      
      const successMessage = this.isOrgUnitAdmin() 
        ? 'User partnership access updated successfully'
        : 'User permissions and organization settings updated successfully';
        
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: successMessage
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
    this.isPartnerUser.set(false);
    this.isSelfManagementEnabled.set(false);
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