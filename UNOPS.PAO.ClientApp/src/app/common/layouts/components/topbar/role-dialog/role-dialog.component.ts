import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { MultiSelectModule } from 'primeng/multiselect';
import { ButtonModule } from 'primeng/button';
import { FormsModule } from '@angular/forms';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { RoleService, Role, UserRoles } from '../../../../../essentials/services/role.service';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-role-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    MultiSelectModule,
    ButtonModule,
    FormsModule,
    ConfirmDialogModule
  ],
  template: `
    <p-dialog 
      header="Impersonate Roles (only for testing)" 
      [(visible)]="visible" 
      [style]="{ width: '600px' }" 
      [modal]="true"
      [closable]="true"
      [closeOnEscape]="true"
      styleClass="p-8">
      <div class="flex flex-col gap-6">
        <div class="user-info">
          <p class="text-xl font-semibold mb-2">User Email</p>
          <p class="text-gray-600 text-lg">{{ userRoles?.email }}</p>
        </div>
        <div class="roles-selection">
          <p class="text-xl font-semibold mb-4">Your Roles</p>
          <p-multiSelect
            [options]="availableRoles"
            [(ngModel)]="selectedRoles"
            optionLabel="name"
            optionValue="name"
            [style]="{ width: '100%' }"
            [panelStyle]="{ width: '100%', minHeight: '300px' }"
            [appendTo]="'body'"
            styleClass="w-full"
            [dropdownIcon]="'pi pi-chevron-down'"
            [overlayOptions]="{
              styleClass: 'w-[600px] min-h-[300px]',
              appendTo: 'body',
              baseZIndex: 1000,
              autoZIndex: true
            }"
            placeholder="Select roles">
            <ng-template let-role pTemplate="item">
              <div class="py-3 px-4 hover:bg-gray-100 rounded-md transition-colors">
                <span class="text-base">{{ role.name }}</span>
              </div>
            </ng-template>
          </p-multiSelect>
        </div>
      </div>
      <ng-template pTemplate="footer">
        <div class="flex justify-end gap-4 mt-6">
          <p-button 
            label="Cancel" 
            icon="pi pi-times" 
            (onClick)="hideDialog()" 
            styleClass="p-button-text">
          </p-button>
          <p-button 
            label="Save" 
            icon="pi pi-check" 
            (onClick)="saveRoles()" 
            [loading]="saving">
          </p-button>
        </div>
      </ng-template>
    </p-dialog>
    
    <p-confirmDialog></p-confirmDialog>
  `,
  styles: [`
    :host ::ng-deep {
      .p-multiselect-panel {
        width: 600px !important;
        min-width: 600px !important;
      }
      .p-multiselect-items-wrapper {
        max-height: 300px !important;
      }
    }
  `]
})
export class RoleDialogComponent implements OnInit {
  visible: boolean = false;
  availableRoles: Role[] = [];
  selectedRoles: string[] = [];
  userRoles: UserRoles | null = null;
  saving: boolean = false;

  constructor(
    private roleService: RoleService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit() {
    this.loadRoles();
  }

  show() {
    this.visible = true;
    this.loadRoles();
  }

  hideDialog() {
    this.visible = false;
  }

  private loadRoles() {
    // Load available roles
    this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        this.availableRoles = roles;
        
        // Load user's current roles
        this.roleService.getUserRoles().subscribe({
          next: (userRoles) => {
            this.userRoles = userRoles;
            this.selectedRoles = userRoles.roles;
          },
          error: (error) => {
            console.error('Error loading user roles:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to load user roles'
            });
          }
        });
      },
      error: (error) => {
        console.error('Error loading available roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load available roles'
        });
      }
    });
  }

  saveRoles() {
    this.saving = true;
    var genUser = true;
    if (this.selectedRoles.length === 0 || !this.selectedRoles.includes('UNOPS_GEN_USER')) {
      this.selectedRoles.push('UNOPS_GEN_USER');
      genUser = false;
    }
    this.roleService.updateUserRoles(this.selectedRoles).subscribe({
      next: () => {
        var message = 'Roles updated successfully. ';
        if (!genUser) {
          message += 'UNOPS_GEN_USER is a default role, hence it has been added to your roles automatically. ';
        }
        message += 'Would you like to refresh the page to see the changes?';
        
        this.hideDialog();
        
        // Show confirmation dialog for page refresh
        this.confirmationService.confirm({
          message: message,
          header: 'Refresh Page',
          icon: 'pi pi-refresh',
          acceptLabel: 'Yes, Refresh',
          rejectLabel: 'No, Later',
          accept: () => {
            window.location.reload();
          },
          reject: () => {
            // User chose not to refresh, do nothing
          }
        });
      },
      error: (error) => {
        console.error('Error updating roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to update roles'
        });
      },
      complete: () => {
        this.saving = false;
      }
    });
  }
} 