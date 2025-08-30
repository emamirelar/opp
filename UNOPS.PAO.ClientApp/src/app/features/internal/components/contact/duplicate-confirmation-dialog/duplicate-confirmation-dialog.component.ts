import { Component, inject } from '@angular/core';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';
import { Router } from '@angular/router';

export interface DuplicateDetectionResponse {
  success: boolean;
  action: 'duplicateConfirmation' | 'created';
  message: string;
  entityType?: string; // Added to support different entity types
  duplicateInfo?: {
    totalDuplicates: number;
    highConfidence: number;
    mediumConfidence: number;
    lowConfidence: number;
    topDuplicate?: {
      entityId: number;
      score: number;
      matchReason: string;
      matchedData: any;
    };
  };
  confirmationRequired?: boolean;
  originalData?: any;
}

@Component({
  selector: 'app-duplicate-confirmation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    DialogModule,
    TranslateModule,
    CardModule,
    BadgeModule,
    DividerModule
  ],
  template: `
    <div class="p-4">
      <p class="text-gray-700 mb-4">{{ data.message }}</p>

      <div *ngIf="data.duplicateInfo?.topDuplicate" class="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
        <div class="flex items-center justify-between">
          <div>
            <div class="font-medium text-gray-800 mb-1">{{ data.duplicateInfo!.topDuplicate!.matchReason }}</div>
            <div class="text-sm text-gray-600">
              {{ 'DUPLICATE_DETECTION.matchScore' | translate }}: {{ (data.duplicateInfo!.topDuplicate!.score * 100) | number:'1.1-1' }}%
            </div>
          </div>
          
          <button 
            type="button"
            class="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors font-medium"
            (click)="viewRecord(data.duplicateInfo!.topDuplicate!.entityId)">
            <i class="pi pi-external-link mr-2"></i>
            {{ 'DUPLICATE_DETECTION.viewRecord' | translate }}
          </button>
        </div>
      </div>

      <div class="flex justify-end gap-3">
        <button 
          type="button" 
          class="px-4 py-2 text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          (click)="cancel()">
          {{ 'button.cancel' | translate }}
        </button>
        
        <button 
          type="button" 
          class="px-4 py-2 bg-orange-500 text-white rounded-lg hover:bg-orange-600 transition-colors font-medium"
          (click)="confirm()">
          {{ getCreateAnywayTranslation() | translate }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-width: 400px;
      max-width: 500px;
    }
    
    @media (max-width: 768px) {
      :host {
        min-width: 300px;
        max-width: 95vw;
      }
    }
  `]
})
export class DuplicateConfirmationDialogComponent {
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);
  private router = inject(Router);

  get data(): DuplicateDetectionResponse {
    return this.dialogConfig.data;
  }

  get entityType(): string {
    return this.data.entityType || this.detectEntityTypeFromMessage() || 'contact';
  }

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  viewRecord(entityId: number): void {
    const entityType = this.entityType.toLowerCase();
    let route = '';
    
    switch (entityType) {
      case 'contact':
        route = `/partnerships/contacts/${entityId}`;
        break;
      case 'partner':
        route = `/partnerships/partners/${entityId}`;
        break;
      case 'interaction':
        route = `/partnerships/interactions/${entityId}`;
        break;
      default:
        route = `/partnerships/contacts/${entityId}`;
    }
    
    // Open in new tab
    window.open(route, '_blank');
  }



  getCreateAnywayTranslation(): string {
    const entityType = this.entityType.toLowerCase();
    return `DUPLICATE_DETECTION.create${this.capitalizeFirst(entityType)}Anyway`;
  }

  private detectEntityTypeFromMessage(): string {
    const message = this.data.message?.toLowerCase() || '';
    if (message.includes('contact')) return 'contact';
    if (message.includes('partner')) return 'partner';
    if (message.includes('interaction')) return 'interaction';
    return 'contact'; // default fallback
  }

  private capitalizeFirst(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }
}
