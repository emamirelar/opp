import { Component, inject } from '@angular/core';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';

export interface DuplicateDetectionResponse {
  success: boolean;
  action: 'duplicateConfirmation' | 'created';
  message: string;
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
    <div class="flex flex-col gap-4 p-4">
      <div class="flex items-center gap-3 mb-4">
        <i class="pi pi-exclamation-triangle text-orange-500 text-2xl"></i>
        <h2 class="text-xl font-semibold">{{ 'contact.duplicateDetected' | translate }}</h2>
      </div>

      <p class="text-gray-700 mb-4">{{ data.message }}</p>

      <div *ngIf="data.duplicateInfo" class="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-4">
        <h4 class="font-semibold mb-3 text-yellow-800">{{ 'contact.duplicateSummary' | translate }}</h4>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-3 mb-4">
          <div *ngIf="(data.duplicateInfo?.highConfidence || 0) > 0" 
               class="bg-red-100 border border-red-200 rounded-lg p-3 text-center">
            <div class="text-2xl font-bold text-red-600">{{ data.duplicateInfo.highConfidence }}</div>
            <div class="text-sm text-red-700">{{ 'contact.highConfidence' | translate }}</div>
          </div>
          
          <div *ngIf="(data.duplicateInfo?.mediumConfidence || 0) > 0" 
               class="bg-orange-100 border border-orange-200 rounded-lg p-3 text-center">
            <div class="text-2xl font-bold text-orange-600">{{ data.duplicateInfo.mediumConfidence }}</div>
            <div class="text-sm text-orange-700">{{ 'contact.mediumConfidence' | translate }}</div>
          </div>
          
          <div *ngIf="(data.duplicateInfo?.lowConfidence || 0) > 0" 
               class="bg-yellow-100 border border-yellow-200 rounded-lg p-3 text-center">
            <div class="text-2xl font-bold text-yellow-600">{{ data.duplicateInfo.lowConfidence }}</div>
            <div class="text-sm text-yellow-700">{{ 'contact.lowConfidence' | translate }}</div>
          </div>
        </div>
      </div>

      <div *ngIf="data.duplicateInfo?.topDuplicate" class="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-4">
        <h4 class="font-semibold mb-3 text-gray-800">{{ 'contact.topMatch' | translate }}</h4>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <div class="text-sm text-gray-600">{{ 'contact.contactId' | translate }}</div>
            <div class="font-medium">#{{ data.duplicateInfo!.topDuplicate!.entityId }}</div>
          </div>
          
          <div>
            <div class="text-sm text-gray-600">{{ 'contact.matchScore' | translate }}</div>
            <div class="font-medium">{{ (data.duplicateInfo!.topDuplicate!.score * 100) | number:'1.1-1' }}%</div>
          </div>
          
          <div class="md:col-span-2">
            <div class="text-sm text-gray-600">{{ 'contact.matchReason' | translate }}</div>
            <div class="font-medium">{{ data.duplicateInfo!.topDuplicate!.matchReason }}</div>
          </div>
        </div>

        <div *ngIf="data.duplicateInfo!.topDuplicate!.matchedData" class="mt-4">
          <p-divider></p-divider>
          <h5 class="font-medium mb-2 text-gray-700">{{ 'contact.matchedData' | translate }}</h5>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-2">
            <div *ngFor="let item of getMatchedDataArray(data.duplicateInfo!.topDuplicate!.matchedData)" 
                 class="bg-white p-2 rounded border text-sm">
              <span class="font-medium text-gray-600">{{ item.key }}:</span>
              <span class="ml-2">{{ item.value || 'N/A' }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-orange-50 border border-orange-200 rounded-lg p-4 mb-6">
        <div class="flex items-start gap-3">
          <i class="pi pi-info-circle text-orange-500 mt-1"></i>
          <div>
            <p class="text-orange-800 font-medium">{{ 'contact.warningTitle' | translate }}</p>
            <p class="text-orange-700 text-sm mt-1">{{ 'contact.warningMessage' | translate }}</p>
          </div>
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
          {{ 'contact.createAnyway' | translate }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-width: 500px;
      max-width: 700px;
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

  get data(): DuplicateDetectionResponse {
    return this.dialogConfig.data;
  }

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  getMatchedDataArray(matchedData: any): {key: string, value: any}[] {
    if (!matchedData) return [];
    
    return Object.keys(matchedData).map(key => ({
      key: this.formatFieldName(key),
      value: matchedData[key]
    })).filter(item => item.value); // Only show fields with values
  }

  private formatFieldName(key: string): string {
    // Convert camelCase to readable format
    return key
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/^./, str => str.toUpperCase());
  }
}
