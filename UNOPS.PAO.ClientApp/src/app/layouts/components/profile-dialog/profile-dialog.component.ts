import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

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
    FormsModule,
    TranslateModule
  ],
  template: `
    <p-dialog 
      [(visible)]="visible" 
      [style]="{width: '600px'}" 
      [header]="'profile.title' | translate" 
      [modal]="true"
      [draggable]="false"
      [resizable]="false">
      <div class="flex flex-col gap-4 p-4">
        <!-- Personal Information Section -->
        <div class="section">
          <h3 class="section-header">{{'profile.personal_information' | translate}}</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field">
              <label class="font-semibold">{{'profile.full_name' | translate}}</label>
              <div class="mt-2">{{userInfo?.name || ('common.not_available' | translate)}}</div>
            </div>
            <div class="field">
              <label class="font-semibold">{{'profile.email' | translate}}</label>
              <div class="mt-2">{{userInfo?.userEmail || ('common.not_available' | translate)}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.firstName">
              <label class="font-semibold">{{'profile.first_name' | translate}}</label>
              <div class="mt-2">{{userInfo?.firstName}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.lastName">
              <label class="font-semibold">{{'profile.last_name' | translate}}</label>
              <div class="mt-2">{{userInfo?.lastName}}</div>
            </div>
          </div>
        </div>

        <!-- Work Information Section -->
        <div class="section">
          <h3 class="section-header">{{'profile.work_information' | translate}}</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field">
              <label class="font-semibold">{{'profile.organization_unit' | translate}}</label>
              <div class="mt-2">
                <div>{{userInfo?.orgUnit || ('common.not_available' | translate)}}</div>
                <div *ngIf="userInfo?.orgUnitDescription" class="text-sm text-gray-600 mt-1">{{userInfo?.orgUnitDescription}}</div>
              </div>
            </div>
            <div class="field" *ngIf="userInfo?.position">
              <label class="font-semibold">{{'profile.position' | translate}}</label>
              <div class="mt-2">{{userInfo?.position}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.dutyStation">
              <label class="font-semibold">{{'profile.duty_station' | translate}}</label>
              <div class="mt-2">{{userInfo?.dutyStation}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.supervisorName || userInfo?.supervisorEmail">
              <label class="font-semibold">{{'profile.supervisor' | translate}}</label>
              <div class="mt-2">
                <div *ngIf="userInfo?.supervisorName">{{userInfo?.supervisorName}}</div>
                <div *ngIf="userInfo?.supervisorEmail" class="text-sm text-gray-600">{{userInfo?.supervisorEmail}}</div>
                <div *ngIf="!userInfo?.supervisorName && !userInfo?.supervisorEmail">{{'common.not_available' | translate}}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Preferences Section -->
        <div class="section" *ngIf="userInfo?.language || userInfo?.textToSpeech !== undefined">
          <h3 class="section-header">{{'profile.preferences' | translate}}</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field" *ngIf="userInfo?.language">
              <label class="font-semibold">{{'profile.language' | translate}}</label>
              <div class="mt-2">{{getLanguageName(userInfo?.language) | translate}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.textToSpeech !== undefined">
              <label class="font-semibold">{{'profile.text_to_speech' | translate}}</label>
              <div class="mt-2">
                <span class="px-2 py-1 rounded text-sm" 
                      [ngClass]="userInfo?.textToSpeech ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'">
                  {{userInfo?.textToSpeech ? ('common.enabled' | translate) : ('common.disabled' | translate)}}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- System Information Section -->
        <div class="section" *ngIf="userInfo?.createdDate || userInfo?.lastModifiedDate">
          <h3 class="section-header">{{'profile.system_information' | translate}}</h3>
          <div class="grid grid-cols-2 gap-4">
            <div class="field" *ngIf="userInfo?.createdDate">
              <label class="font-semibold">{{'profile.created_date' | translate}}</label>
              <div class="mt-2 text-sm text-gray-600">{{formatDate(userInfo?.createdDate)}}</div>
            </div>
            <div class="field" *ngIf="userInfo?.lastModifiedDate">
              <label class="font-semibold">{{'profile.last_modified' | translate}}</label>
              <div class="mt-2 text-sm text-gray-600">{{formatDate(userInfo?.lastModifiedDate)}}</div>
            </div>
          </div>
        </div>
      </div>
      <ng-template pTemplate="footer">
        <div class="flex justify-end">
          <p-button [label]="'common.close' | translate" (click)="visible = false"></p-button>
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
    // Use translation keys for language names
    const languageKeys: { [key: string]: string } = {
      'en': 'languages.english',
      'fr': 'languages.french',
      'es': 'languages.spanish',
      'ar': 'languages.arabic',
      'zh': 'languages.chinese',
      'hi': 'languages.hindi',
      'ru': 'languages.russian',
      'pt': 'languages.portuguese',
      'de': 'languages.german',
      'ja': 'languages.japanese'
    };
    
    const key = languageKeys[languageCode?.toLowerCase() || ''];
    return key ? key : (languageCode || 'languages.not_specified');
  }
} 
