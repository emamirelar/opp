import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { TranslateModule } from '@ngx-translate/core';

export interface DuplicateDetectionInfo {
  hasDuplicates: boolean;
  totalDuplicates?: number;
  highConfidence?: number;
  mediumConfidence?: number;
  lowConfidence?: number;
  topDuplicate?: {
    entityId: number;
    entityType: string;
    score: number;
    matchReason: string;
    searchType: string;
    matchedData?: any;
  };
}

@Component({
  selector: 'app-duplicate-indicator',
  standalone: true,
  imports: [
    CommonModule,
    TooltipModule,
    BadgeModule,
    ButtonModule,
    OverlayPanelModule,
    TagModule,
    DividerModule,
    TranslateModule
  ],
  template: `
    <div class="duplicate-indicator-container">
      @if (duplicateInfo?.hasDuplicates) {
        <!-- Duplicate Found Indicator -->
        <div class="relative">
          <button 
            type="button"
            class="duplicate-button duplicate-found"
            [class.high-confidence]="confidenceLevel() === 'high'"
            [class.medium-confidence]="confidenceLevel() === 'medium'"
            [class.low-confidence]="confidenceLevel() === 'low'"
            (click)="op.toggle($event)"
            [pTooltip]="getTooltipText()"
            tooltipPosition="top"
          >
            <i class="pi pi-exclamation-triangle"></i>
            <span class="duplicate-count">{{ duplicateInfo?.totalDuplicates }}</span>
          </button>
          
          <!-- Confidence Level Badge -->
          <div class="confidence-badge" [ngClass]="confidenceLevel()">
            {{ confidenceLevel().toUpperCase() }}
          </div>
        </div>

        <!-- Detailed Duplicate Information Overlay -->
        <p-overlayPanel #op [showCloseIcon]="true" [style]="{ width: '400px' }">
          <div class="duplicate-details">
            <!-- Header -->
            <div class="duplicate-header">
              <div class="flex items-center gap-2">
                <i class="pi pi-exclamation-triangle text-orange-500 text-xl"></i>
                <h4 class="m-0 text-lg font-semibold text-gray-800">
                  {{ 'DUPLICATE_DETECTION.duplicatesFound' | translate }}
                </h4>
              </div>
              <p-tag 
                [value]="(duplicateInfo?.totalDuplicates || 0) + ' ' + ('DUPLICATE_DETECTION.duplicates' | translate)"
                severity="warn"
                [rounded]="true"
              ></p-tag>
            </div>

            <p-divider></p-divider>

            <!-- Confidence Breakdown -->
            <div class="confidence-breakdown">
              <h5 class="text-sm font-medium text-gray-600 mb-3">
                {{ 'DUPLICATE_DETECTION.confidenceLevels' | translate }}
              </h5>
              
              @if ((duplicateInfo?.highConfidence || 0) > 0) {
                <div class="confidence-item high">
                  <div class="confidence-bar">
                    <div class="confidence-fill" [style.width.%]="getConfidencePercentage('high')"></div>
                  </div>
                  <span class="confidence-label">
                    {{ 'DUPLICATE_DETECTION.highConfidence' | translate }}: 
                    <strong>{{ duplicateInfo?.highConfidence }}</strong>
                  </span>
                </div>
              }
              
              @if ((duplicateInfo?.mediumConfidence || 0) > 0) {
                <div class="confidence-item medium">
                  <div class="confidence-bar">
                    <div class="confidence-fill" [style.width.%]="getConfidencePercentage('medium')"></div>
                  </div>
                  <span class="confidence-label">
                    {{ 'DUPLICATE_DETECTION.mediumConfidence' | translate }}: 
                    <strong>{{ duplicateInfo?.mediumConfidence }}</strong>
                  </span>
                </div>
              }
              
              @if ((duplicateInfo?.lowConfidence || 0) > 0) {
                <div class="confidence-item low">
                  <div class="confidence-bar">
                    <div class="confidence-fill" [style.width.%]="getConfidencePercentage('low')"></div>
                  </div>
                  <span class="confidence-label">
                    {{ 'DUPLICATE_DETECTION.lowConfidence' | translate }}: 
                    <strong>{{ duplicateInfo?.lowConfidence }}</strong>
                  </span>
                </div>
              }
            </div>

            <!-- Top Duplicate Details -->
            @if (duplicateInfo?.topDuplicate; as topDuplicate) {
              <p-divider></p-divider>
              
              <div class="top-duplicate">
                <h5 class="text-sm font-medium text-gray-600 mb-3">
                  {{ 'DUPLICATE_DETECTION.topMatch' | translate }}
                </h5>
                
                <div class="top-duplicate-card">
                  <div class="flex justify-between items-start mb-2">
                    <div class="duplicate-id">
                      <span class="text-xs text-gray-500">ID:</span>
                      <span class="font-medium text-blue-600">#{{ topDuplicate.entityId }}</span>
                    </div>
                    <div class="match-score">
                      <span class="score-value">{{ ((topDuplicate.score || 0) * 100) | number:'1.1-1' }}%</span>
                    </div>
                  </div>
                  
                  <div class="match-reason">
                    <span class="text-xs text-gray-500">{{ 'DUPLICATE_DETECTION.matchReason' | translate }}:</span>
                    <span class="reason-text">{{ topDuplicate.matchReason }}</span>
                  </div>
                  
                  <!-- Quick Link -->
                  <div class="quick-link mt-2">
                    <a 
                      [href]="getEntityUrl(topDuplicate.entityId)" 
                      target="_blank"
                      class="quick-link-btn"
                    >
                      <i class="pi pi-external-link mr-1"></i>
                      {{ 'DUPLICATE_DETECTION.viewRecord' | translate }} #{{ topDuplicate.entityId }}
                    </a>
                  </div>
                  
                  @if (topDuplicate.matchedData) {
                    <div class="matched-data mt-3">
                      <span class="text-xs text-gray-500 block mb-1">
                        {{ 'DUPLICATE_DETECTION.matchedFields' | translate }}:
                      </span>
                      <div class="matched-fields">
                        @for (field of getMatchedDataArray(topDuplicate.matchedData); track field.key) {
                          <div class="matched-field">
                            <span class="field-name">{{ field.key }}:</span>
                            <span class="field-value">{{ field.value }}</span>
                          </div>
                        }
                      </div>
                    </div>
                  }
                  
                  <!-- View Details Button -->
                  <div class="mt-3 pt-2 border-t border-gray-100">
                    <button 
                      type="button" 
                      class="view-details-btn"
                      (click)="viewDuplicateDetails(topDuplicate.entityId || 0)"
                    >
                      <i class="pi pi-external-link mr-1"></i>
                      {{ 'DUPLICATE_DETECTION.viewDetails' | translate }}
                    </button>
                  </div>
                </div>
              </div>
            }
          </div>
        </p-overlayPanel>
      } @else {
        <!-- No Duplicates Indicator -->
        <button 
          type="button"
          class="duplicate-button no-duplicates"
          [pTooltip]="'DUPLICATE_DETECTION.uniqueRecordTooltip' | translate"
          tooltipPosition="top"
        >
          <i class="pi pi-check-circle"></i>
        </button>
      }
    </div>
  `,
  styleUrls: ['./duplicate-indicator.component.scss']
})
export class DuplicateIndicatorComponent {
  @Input() duplicateInfo?: DuplicateDetectionInfo;
  @Input() entityType: string = 'Contact';

  confidenceLevel = computed(() => {
    if (!this.duplicateInfo?.hasDuplicates) return 'none';
    
    const high = this.duplicateInfo.highConfidence || 0;
    const medium = this.duplicateInfo.mediumConfidence || 0;
    const low = this.duplicateInfo.lowConfidence || 0;
    
    if (high > 0) return 'high';
    if (medium > 0) return 'medium';
    if (low > 0) return 'low';
    return 'none';
  });

  getTooltipText(): string {
    if (!this.duplicateInfo?.hasDuplicates) {
      return 'DUPLICATE_DETECTION.uniqueRecordTooltip';
    }
    
    const total = this.duplicateInfo.totalDuplicates || 0;
    const confidence = this.confidenceLevel();
    return `${total} DUPLICATE_DETECTION.duplicateTooltip - ${confidence} DUPLICATE_DETECTION.${confidence}ConfidenceTooltip`;
  }



  getConfidencePercentage(level: 'high' | 'medium' | 'low'): number {
    if (!this.duplicateInfo) return 0;
    
    const total = this.duplicateInfo.totalDuplicates || 0;
    if (total === 0) return 0;
    
    const count = this.duplicateInfo[`${level}Confidence`] || 0;
    return (count / total) * 100;
  }

  getMatchedDataArray(matchedData: any): Array<{key: string, value: any}> {
    if (!matchedData) return [];
    
    if (typeof matchedData === 'string') {
      try {
        matchedData = JSON.parse(matchedData);
      } catch {
        return [];
      }
    }
    
    return Object.entries(matchedData).map(([key, value]) => ({
      key: this.formatFieldName(key),
      value: value
    }));
  }

  private formatFieldName(fieldName: string): string {
    // Convert camelCase to Title Case
    return fieldName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  getEntityUrl(entityId: number): string {
    const entityType = this.entityType.toLowerCase();
    let route = '';
    
    // Map entity types to correct URLs
    switch (entityType) {
      case 'contact':
      case 'contacts':
        route = `/partnerships/contacts/${entityId}`;
        break;
      case 'partner':
      case 'partners':
        route = `/partnerships/partners/${entityId}`;
        break;
      case 'interaction':
      case 'interactions':
        route = `/partnerships/interactions/${entityId}`;
        break;
      default:
        route = `/partnerships/${entityType}/${entityId}`;
        break;
    }
    
    // Construct full URL with protocol, host, hash and route
    return `${window.location.protocol}//${window.location.host}/#${route}`;
  }

  viewDuplicateDetails(entityId: number): void {
    // Navigate to the duplicate record details in a new tab
    const url = this.getEntityUrl(entityId);
    window.open(url, '_blank');
  }
}
