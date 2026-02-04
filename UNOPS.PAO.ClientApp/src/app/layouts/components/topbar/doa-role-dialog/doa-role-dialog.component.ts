import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { FormsModule } from '@angular/forms';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { RoleService, DoaRoleAssignment } from '@core/services/auth';
import { MessageService, ConfirmationService } from 'primeng/api';
import { HttpClient } from '@angular/common/http';
import { AutoCompleteModule, AutoCompleteCompleteEvent } from 'primeng/autocomplete';

interface OrgUnit {
  id: number;
  code: string;
  name: string;
  type?: number;
}

interface User {
  id: number;
  email: string;
  name: string;
}

interface DoaRoleOption {
  label: string;
  value: string;
  roleName: string;  // Name used to look up EntityRoleId in backend
}

interface PendingAssignment {
  id: number;
  orgUnit: OrgUnit;
  user: User;
  doaRole: DoaRoleOption;
}

@Component({
  selector: 'app-doa-role-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    DropdownModule,
    ButtonModule,
    TableModule,
    FormsModule,
    ConfirmDialogModule,
    TooltipModule,
    AutoCompleteModule
  ],
  template: `
    <p-dialog 
      header="Assign DOA Roles" 
      [(visible)]="visible" 
      [style]="{ width: '800px' }" 
      [modal]="true"
      [closable]="true"
      [closeOnEscape]="true"
      styleClass="p-6">
      
      <div class="flex flex-col gap-6">
        <!-- Form Section -->
        <div class="grid grid-cols-3 gap-4">
          <!-- Org Unit Dropdown -->
          <div class="flex flex-col gap-2">
            <label class="font-semibold text-sm">Org Unit</label>
            <p-autoComplete
              [(ngModel)]="selectedOrgUnit"
              [suggestions]="filteredOrgUnits"
              (completeMethod)="filterOrgUnits($event)"
              field="name"
              [dropdown]="true"
              [forceSelection]="true"
              placeholder="Search org unit..."
              styleClass="w-full"
              [style]="{ width: '100%' }">
              <ng-template let-orgUnit pTemplate="item">
                <div class="flex flex-col">
                  <span class="font-medium">{{ orgUnit.code }}</span>
                  <span class="text-sm text-gray-600">{{ orgUnit.name }}</span>
                </div>
              </ng-template>
            </p-autoComplete>
          </div>

          <!-- User Dropdown -->
          <div class="flex flex-col gap-2">
            <label class="font-semibold text-sm">User</label>
            <p-autoComplete
              [(ngModel)]="selectedUser"
              [suggestions]="filteredUsers"
              (completeMethod)="filterUsers($event)"
              field="name"
              [dropdown]="true"
              [forceSelection]="true"
              placeholder="Search user..."
              styleClass="w-full"
              [style]="{ width: '100%' }">
              <ng-template let-user pTemplate="item">
                <div class="flex flex-col">
                  <span class="font-medium">{{ user.name }}</span>
                  <span class="text-sm text-gray-600">{{ user.email }}</span>
                </div>
              </ng-template>
            </p-autoComplete>
          </div>

          <!-- DOA Role Dropdown -->
          <div class="flex flex-col gap-2">
            <label class="font-semibold text-sm">DOA Role</label>
            <p-dropdown
              [(ngModel)]="selectedDoaRole"
              [options]="doaRoleOptions"
              optionLabel="label"
              placeholder="Select DOA Role"
              styleClass="w-full"
              [style]="{ width: '100%' }">
            </p-dropdown>
          </div>
        </div>

        <!-- Add Button -->
        <div class="flex justify-end">
          <p-button 
            label="Add to List" 
            icon="pi pi-plus" 
            (onClick)="addAssignment()"
            [disabled]="!canAdd()">
          </p-button>
        </div>

        <!-- Pending Assignments Table -->
        <div class="border rounded-lg overflow-hidden" *ngIf="pendingAssignments.length > 0">
          <p-table [value]="pendingAssignments" styleClass="p-datatable-sm">
            <ng-template pTemplate="header">
              <tr>
                <th>Org Unit</th>
                <th>User</th>
                <th>DOA Role</th>
                <th style="width: 80px">Actions</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-assignment>
              <tr>
                <td>
                  <div class="flex flex-col">
                    <span class="font-medium">{{ assignment.orgUnit.code }}</span>
                    <span class="text-sm text-gray-600">{{ assignment.orgUnit.name }}</span>
                  </div>
                </td>
                <td>
                  <div class="flex flex-col">
                    <span class="font-medium">{{ assignment.user.name }}</span>
                    <span class="text-sm text-gray-600">{{ assignment.user.email }}</span>
                  </div>
                </td>
                <td>{{ assignment.doaRole.label }}</td>
                <td>
                  <p-button 
                    icon="pi pi-trash" 
                    severity="danger" 
                    [text]="true"
                    pTooltip="Remove"
                    (onClick)="removeAssignment(assignment)">
                  </p-button>
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="4" class="text-center text-gray-500 py-4">
                  No assignments added yet
                </td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <!-- Empty State -->
        <div *ngIf="pendingAssignments.length === 0" class="text-center text-gray-500 py-8 border rounded-lg">
          <i class="pi pi-users text-4xl mb-4"></i>
          <p>Add DOA role assignments using the form above</p>
        </div>
      </div>

      <ng-template pTemplate="footer">
        <div class="flex justify-between items-center mt-4">
          <span class="text-sm text-gray-600">
            {{ pendingAssignments.length }} assignment(s) pending
          </span>
          <div class="flex gap-4">
            <p-button 
              label="Cancel" 
              icon="pi pi-times" 
              (onClick)="hideDialog()" 
              styleClass="p-button-text">
            </p-button>
            <p-button 
              label="Save All" 
              icon="pi pi-check" 
              (onClick)="saveAssignments()" 
              [loading]="saving"
              [disabled]="pendingAssignments.length === 0">
            </p-button>
          </div>
        </div>
      </ng-template>
    </p-dialog>
    
    <p-confirmDialog></p-confirmDialog>
  `,
  styles: [`
    :host ::ng-deep {
      .p-dropdown, .p-autocomplete {
        width: 100% !important;
      }
      .p-autocomplete-input {
        width: 100% !important;
      }
    }
  `]
})
export class DoaRoleDialogComponent implements OnInit {
  visible: boolean = false;
  saving: boolean = false;
  
  // Dropdown data
  orgUnits: OrgUnit[] = [];
  filteredOrgUnits: OrgUnit[] = [];
  users: User[] = [];
  filteredUsers: User[] = [];
  
  doaRoleOptions: DoaRoleOption[] = [
    { label: 'DOA Level 2', value: 'DoA2_OrganizationHierarchy', roleName: 'DoA2' },
    { label: 'DOA Level 3', value: 'DoA3_OrganizationHierarchy', roleName: 'DoA3' }
  ];

  // Selected values
  selectedOrgUnit: OrgUnit | null = null;
  selectedUser: User | null = null;
  selectedDoaRole: DoaRoleOption | null = null;

  // Pending assignments table
  pendingAssignments: PendingAssignment[] = [];
  private assignmentIdCounter: number = 0;

  constructor(
    private roleService: RoleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.loadData();
  }

  show() {
    this.visible = true;
    this.loadData();
    this.resetForm();
  }

  hideDialog() {
    if (this.pendingAssignments.length > 0) {
      this.confirmationService.confirm({
        message: 'You have unsaved assignments. Are you sure you want to close?',
        header: 'Confirm Close',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          this.visible = false;
          this.pendingAssignments = [];
          this.resetForm();
        }
      });
    } else {
      this.visible = false;
      this.resetForm();
    }
  }

  private loadData() {
    // Load organization units
    this.http.get<OrgUnit[]>('/api/values/organization-units').subscribe({
      next: (orgUnits) => {
        this.orgUnits = orgUnits;
        this.filteredOrgUnits = [...orgUnits];
      },
      error: (error) => {
        console.error('Error loading org units:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load organization units'
        });
      }
    });

    // Load users
    this.http.get<User[]>('/api/values/users/search?maxResults=100').subscribe({
      next: (users) => {
        this.users = users;
        this.filteredUsers = [...users];
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load users'
        });
      }
    });
  }

  filterOrgUnits(event: AutoCompleteCompleteEvent) {
    const query = event.query.toLowerCase();
    this.filteredOrgUnits = this.orgUnits.filter(orgUnit => 
      orgUnit.name.toLowerCase().includes(query) || 
      orgUnit.code.toLowerCase().includes(query)
    );
  }

  filterUsers(event: AutoCompleteCompleteEvent) {
    const query = event.query.toLowerCase();
    if (query.length < 2) {
      this.filteredUsers = [];
      return;
    }
    
    // Call the user search API
    this.http.get<User[]>(`/api/values/users/search?searchTerm=${encodeURIComponent(query)}&maxResults=20`).subscribe({
      next: (users) => {
        this.filteredUsers = users;
      },
      error: () => {
        this.filteredUsers = [];
      }
    });
  }

  canAdd(): boolean {
    return this.selectedOrgUnit !== null && 
           this.selectedUser !== null && 
           this.selectedDoaRole !== null;
  }

  addAssignment() {
    if (!this.canAdd()) return;

    // Check for duplicate
    const isDuplicate = this.pendingAssignments.some(a => 
      a.orgUnit.id === this.selectedOrgUnit!.id &&
      a.user.id === this.selectedUser!.id &&
      a.doaRole.value === this.selectedDoaRole!.value
    );

    if (isDuplicate) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Duplicate',
        detail: 'This assignment already exists in the list'
      });
      return;
    }

    const assignment: PendingAssignment = {
      id: ++this.assignmentIdCounter,
      orgUnit: this.selectedOrgUnit!,
      user: this.selectedUser!,
      doaRole: this.selectedDoaRole!
    };

    this.pendingAssignments.push(assignment);
    this.resetForm();

    this.messageService.add({
      severity: 'success',
      summary: 'Added',
      detail: 'Assignment added to the list',
      life: 2000
    });
  }

  removeAssignment(assignment: PendingAssignment) {
    this.pendingAssignments = this.pendingAssignments.filter(a => a.id !== assignment.id);
  }

  saveAssignments() {
    if (this.pendingAssignments.length === 0) return;

    this.saving = true;

    // Convert to API format - pass roleName, backend will look up EntityRoleId
    const assignments: DoaRoleAssignment[] = this.pendingAssignments.map(a => ({
      entityId: a.orgUnit.id,
      userId: a.user.id,
      roleName: a.doaRole.roleName,
      entityType: 'OrganizationHierarchy'
    }));

    this.roleService.assignDoaRoles(assignments).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${this.pendingAssignments.length} DOA role(s) assigned successfully`
        });
        this.pendingAssignments = [];
        this.visible = false;
        this.resetForm();
      },
      error: (error) => {
        console.error('Error saving DOA roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.message || 'Failed to save DOA role assignments'
        });
      },
      complete: () => {
        this.saving = false;
      }
    });
  }

  private resetForm() {
    this.selectedOrgUnit = null;
    this.selectedUser = null;
    this.selectedDoaRole = null;
  }
}
