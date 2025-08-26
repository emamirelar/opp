import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';

interface UserInfo {
  userId: number;
  name: string;
  firstName?: string;
  lastName?: string;
  userEmail: string;
  orgUnit: string;
  orgUnitDescription?: string;
  supervisorId?: number;
  supervisorName?: string;
  supervisorEmail?: string;
  dutyStation?: string;
  position?: string;
  textToSpeech?: boolean;
  language?: string;
  createdDate?: string;
  lastModifiedDate?: string;
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
      [style]="{width: '600px'}" 
      header="User Profile" 
      [modal]="true"
      [draggable]="false"
      [resizable]="false">
      <div class="flex flex-col gap-4 p-4">
        <!-- Personal Information Section -->
        <div class="section">
          <h3 class="section-header">Personal Information</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field">
              <label class="font-semibold">Full Name</label>
              <div class="mt-2">{{userInfo?.name || 'N/A'}}</div>
            </div>
            <div class="field">
              <label class="font-semibold">Email</label>
              <div class="mt-2">{{userInfo?.userEmail || 'N/A'}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.firstName">
              <label class="font-semibold">First Name</label>
              <div class="mt-2">{{userInfo?.firstName}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.lastName">
              <label class="font-semibold">Last Name</label>
              <div class="mt-2">{{userInfo?.lastName}}</div>
            </div>
          </div>
        </div>

        <!-- Work Information Section -->
        <div class="section">
          <h3 class="section-header">Work Information</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field">
              <label class="font-semibold">Organization Unit</label>
              <div class="mt-2">
                <div>{{userInfo?.orgUnit || 'N/A'}}</div>
                <div *ngIf="userInfo?.orgUnitDescription" class="text-sm text-gray-600 mt-1">{{userInfo?.orgUnitDescription}}</div>
              </div>
            </div>
            <div class="field" *ngIf="userInfo?.position">
              <label class="font-semibold">Position</label>
              <div class="mt-2">{{userInfo?.position}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.dutyStation">
              <label class="font-semibold">Duty Station</label>
              <div class="mt-2">{{userInfo?.dutyStation}}</div>
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
        </div>

        <!-- Preferences Section -->
        <div class="section" *ngIf="userInfo?.language || userInfo?.textToSpeech !== undefined">
          <h3 class="section-header">Preferences</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field" *ngIf="userInfo?.language">
              <label class="font-semibold">Language</label>
              <div class="mt-2">{{getLanguageName(userInfo?.language)}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.textToSpeech !== undefined">
              <label class="font-semibold">Text-to-Speech</label>
              <div class="mt-2">
                <span class="px-2 py-1 rounded text-sm" 
                      [ngClass]="userInfo?.textToSpeech ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'">
                  {{userInfo?.textToSpeech ? 'Enabled' : 'Disabled'}}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- System Information Section -->
        <div class="section" *ngIf="userInfo?.createdDate || userInfo?.lastModifiedDate">
          <h3 class="section-header">System Information</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field" *ngIf="userInfo?.createdDate">
              <label class="font-semibold">Created Date</label>
              <div class="mt-2 text-sm text-gray-600">{{formatDate(userInfo?.createdDate)}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.lastModifiedDate">
              <label class="font-semibold">Last Modified</label>
              <div class="mt-2 text-sm text-gray-600">{{formatDate(userInfo?.lastModifiedDate)}}</div>
            </div>
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
      .section {
        border-bottom: 1px solid #e5e7eb;
        padding-bottom: 1rem;
        margin-bottom: 1rem;
      }
      .section:last-child {
        border-bottom: none;
        margin-bottom: 0;
      }
      .section-header {
        font-size: 1.1rem;
        font-weight: 600;
        color: #374151;
        margin-bottom: 1rem;
        padding-bottom: 0.5rem;
        border-bottom: 2px solid #3b82f6;
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

  formatDate(dateString?: string): string {
    if (!dateString) return 'N/A';
    
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateString;
    }
  }

  getLanguageName(languageCode?: string): string {
    const languages: { [key: string]: string } = {
      'en': 'English',
      'fr': 'French',
      'es': 'Spanish',
      'ar': 'Arabic',
      'zh': 'Chinese',
      'hi': 'Hindi',
      'ru': 'Russian',
      'pt': 'Portuguese',
      'de': 'German',
      'ja': 'Japanese'
    };
    
    return languages[languageCode?.toLowerCase() || ''] || languageCode || 'Not specified';
  }
} 