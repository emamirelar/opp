import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';

interface UserInfo {
  userId: number;
  name: string;
  userEmail: string;
  orgUnit: string;
  supervisorId: number;
}

@Component({
  selector: 'app-profile-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    FormsModule
  ],
  template: `
    <p-dialog 
      [(visible)]="visible" 
      [style]="{width: '450px'}" 
      header="User Profile" 
      [modal]="true"
      [draggable]="false"
      [resizable]="false">
      <div class="flex flex-col gap-4 p-4">
        <div class="field">
          <label class="font-semibold">Name</label>
          <div class="mt-2">{{userInfo?.name || 'N/A'}}</div>
        </div>
        <div class="field">
          <label class="font-semibold">Email</label>
          <div class="mt-2">{{userInfo?.userEmail || 'N/A'}}</div>
        </div>
        <div class="field">
          <label class="font-semibold">Organization Unit</label>
          <div class="mt-2">{{userInfo?.orgUnit || 'N/A'}}</div>
        </div>
      </div>
      <ng-template pTemplate="footer">
        <div class="flex justify-end">
          <p-button label="Close" (click)="visible = false"></p-button>
        </div>
      </ng-template>
    </p-dialog>
  `,
  styles: [`
    :host ::ng-deep {
      .p-dialog-header {
        padding: 1.5rem;
      }
      .p-dialog-content {
        padding: 0;
      }
      .field {
        padding: 0.5rem 0;
      }
    }
  `]
})
export class ProfileDialogComponent {
  visible: boolean = false;
  userInfo: UserInfo | null = null;

  show(userInfo: UserInfo) {
    this.userInfo = userInfo;
    this.visible = true;
  }
} 