import { Component, OnInit, OnDestroy, signal, computed, inject, ChangeDetectorRef } from '@angular/core';
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

import { MessageService, ConfirmationService, MenuItem } from 'primeng/api';
import { UserManagementService } from './user-management.service';
import { PermissionService, EntityPermissions } from '@core/services/auth';
import { AuthService } from '@core/services/auth';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { MenuModule } from 'primeng/menu';

interface UserManagementModel {
  userId: number;
  name: string;
  email: string;
  orgUnit: string;
  orgUnitCode?: string;
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

interface PaginationResponse<T> {
  records: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
}

/**
 * @uiEntity UserManagement
 * @route /admin/user-management
 * @description Administrative interface for managing user permissions, roles, and organizational access. Allows viewing, editing, and managing user role assignments across the organization.
 * @capabilities view_users, edit_user_roles, assign_permissions, filter_by_org_unit, manage_access_levels, bulk_operations
 * @synonyms user_administration, permission_management, role_assignment, access_control
 * @mandatoryFields user_selection, role_assignment
 * @help_when_stuck Use filters to find specific users (search by name/email, filter by role or org unit). Click "Edit" on any user to modify their role assignments. Use "My Org Unit Only" toggle to focus on your organizational unit. Different roles provide different levels of access to system features.
 * @common_tasks
 *   - Finding a user: Use the search box or role/org unit filters
 *   - Changing user roles: Click "Edit" button, modify role checkboxes, and save
 *   - Filtering by organization: Toggle "My Org Unit Only" or use org unit dropdown
 *   - Managing access levels: Assign Admin, Standard User, or custom roles as appropriate
 *   - Bulk management: Use table filters and pagination for efficient user management
 */

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
    TooltipModule,
    MenuModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit, OnDestroy {
  private userManagementService = inject(UserManagementService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private permissionService = inject(PermissionService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private importDialogService = inject(ImportDialogService);

  // Permission signals
  entityPermissions = signal<EntityPermissions>({
    entity: 'UserManagement',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false,
      canExport: false,
      canImport: false
    }
  });
  permissionsLoading = signal<boolean>(true);

  // Signals for reactive state management
  users = signal<UserManagementModel[]>([]);
  totalRecords = signal<number>(0);
  loading = signal<boolean>(false);
  importing = signal<boolean>(false);
  availableRoles = signal<RoleModel[]>([]);
  
  // Dialog state
  editDialogVisible = signal<boolean>(false);
  selectedUser = signal<UserManagementModel | null>(null);
  isPartnerUser = signal<boolean>(false);
  isSelfManagementEnabled = signal<boolean>(false);

  // Filter and pagination state
  searchTerm = signal<string>('');
  roleFilter = signal<string[]>([]);
  showMyOrgUnitOnly = signal<boolean>(false);
  orgUnitFilter = signal<number[]>([]);
  
  // Org unit options for multi-select
  orgUnitOptions = signal<{label: string, value: number}[]>([]);
  
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

  // Store reference to the refresh event listener for cleanup
  private refreshEventListener?: () => void;

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
    
    // Listen for refresh events from import operations
    this.refreshEventListener = () => {
      this.loadUsers();
    };
    window.addEventListener('refresh-listview', this.refreshEventListener);
  }

  ngOnDestroy() {
    // Clean up event listener
    if (this.refreshEventListener) {
      window.removeEventListener('refresh-listview', this.refreshEventListener);
    }
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
              // After roles are loaded, load the rest of the data (but NOT users yet)
              this.loadAvailableRoles();
              this.loadOrgUnits();
              // Load users LAST to ensure all role-based settings are properly applied
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
        roleFilter: this.roleFilter().length > 0 ? this.roleFilter() : undefined,
        showMyOrgUnitOnly: this.showMyOrgUnitOnly(),
        orgUnitFilter: this.orgUnitFilter().length > 0 ? this.orgUnitFilter() : undefined,
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

  async loadOrgUnits() {
    try {
      const orgUnits = await this.userManagementService.getAvailableOrgUnits();
      this.orgUnitOptions.set(orgUnits.map((ou: any) => ({ 
        label: ou.name, 
        value: ou.id 
      })));
    } catch (error) {
      console.error('Error loading org units:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load organization units'
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
    this.roleFilter.set([]);
    
    // Only reset org unit filter if user is not ORG_UNIT_ADMIN
    if (!this.isOrgUnitAdmin()) {
      this.showMyOrgUnitOnly.set(false);
    }
    
    this.orgUnitFilter.set([]);
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

  getStatusSeverity(isActive: boolean): "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined {
    return isActive ? 'success' : 'danger';
  }

  getStatusText(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  // Import menu items
  importMenuItems = signal<MenuItem[]>([
    {
      label: 'Select from Google Drive',
      icon: 'pi pi-google',
      command: () => this.openGooglePickerImport(),
      title: 'Select a Google Sheet from your Drive. Make sure to set the sheet to "Anyone with the link can view" for public access.'
    },
    {
      label: 'Manual Entry',
      icon: 'pi pi-link',
      command: () => this.openManualEntryImport(),
      title: 'Paste a Google Sheet URL directly and specify the sheet name'
    }
  ]);

  /**
   * Opens the import dialog for user role assignments
   */
  openImportDialog(): void {
    // This method now shows the import menu instead of directly opening the picker
    // The actual menu is handled in the template via p-menu
  }

  /**
   * Open Google Picker for import (original flow)
   */
  openGooglePickerImport(): void {
    // Check if user has update permissions
    if (!this.canUpdate()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to import user roles'
      });
      return;
    }

    // Use Google Picker to select and import user role data
    // This will automatically open the import dialog after file selection and analysis
    this.importDialogService.openGoogleSheetPicker('user_role_import');
  }

  /**
   * Open manual entry dialog for import
   */
  openManualEntryImport(): void {
    // Check if user has update permissions
    if (!this.canUpdate()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Permission Denied',
        detail: 'You do not have permission to import user roles'
      });
      return;
    }

    this.importDialogService.openManualEntryDialog('user_role_import');
  }
} 
