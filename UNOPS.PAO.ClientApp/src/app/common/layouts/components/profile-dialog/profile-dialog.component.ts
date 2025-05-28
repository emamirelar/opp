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
  orgUnitDescription?: string;
  supervisorId: number;
  supervisorName?: string;
  supervisorEmail?: string;
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
          <div class="mt-2">
            <div>{{userInfo?.orgUnit || 'N/A'}}</div>
            <div *ngIf="userInfo?.orgUnitDescription" class="text-sm text-gray-600 mt-1">{{userInfo?.orgUnitDescription}}</div>
          </div>
        </div>
        <div class="field" *ngIf="userInfo?.supervisorName || userInfo?.supervisorEmail">
          <label class="font-semibold">Supervisor</label>
          <div class="mt-2">
            <div *ngIf="userInfo?.supervisorName">{{userInfo?.supervisorName}}</div>
            <div *ngIf="userInfo?.supervisorEmail" class="text-sm text-gray-600">{{userInfo?.supervisorEmail}}</div>
            <div *ngIf="!userInfo?.supervisorName && !userInfo?.supervisorEmail">N/A</div>
          </div>
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