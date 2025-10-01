import { Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { ProgressBarModule } from 'primeng/progressbar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-duplicate-summary',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    BadgeModule,
    ProgressBarModule,
    TranslateModule,
    DividerModule
  ],
  template: `
    @if (duplicateRows && duplicateRows.length > 0) {
      <div class="duplicate-summary-compact">
        <div class="summary-stats">
          <div class="stat-item">
            <span class="stat-value">{{ duplicateRows.length }}</span>
            <span class="stat-label">{{ 'import.duplicateSummary.duplicates' | translate }}</span>
          </div>
          <div class="stat-separator">|</div>
          <div class="stat-item">
            <span class="stat-value">{{ uniqueRecords() }}</span>
            <span class="stat-label">{{ 'import.duplicateSummary.unique' | translate }}</span>
          </div>
          <div class="stat-separator">|</div>
          <div class="stat-item">
            <span class="stat-value">{{ duplicatePercentage() }}%</span>
            <span class="stat-label">{{ 'import.duplicateSummary.duplicateRate' | translate }}</span>
          </div>
        </div>
        
        <div class="confidence-summary">
          @if (highConfidenceCount() > 0) {
            <span class="confidence-badge high">{{ highConfidenceCount() }} {{ 'import.duplicateSummary.confidence.high' | translate }}</span>
          }
          @if (mediumConfidenceCount() > 0) {
            <span class="confidence-badge medium">{{ mediumConfidenceCount() }} {{ 'import.duplicateSummary.confidence.medium' | translate }}</span>
          }
          @if (lowConfidenceCount() > 0) {
            <span class="confidence-badge low">{{ lowConfidenceCount() }} {{ 'import.duplicateSummary.confidence.low' | translate }}</span>
          }
        </div>
        
        <div class="recommendation-compact" [ngClass]="recommendationSeverity()">
          <i [class]="recommendationIcon()"></i>
          <span>{{ recommendationMessage() }}</span>
        </div>
      </div>
    }
  `,
  styleUrls: ['./duplicate-summary.component.scss']
})
export class DuplicateSummaryComponent {
  @Input() duplicateRows: any[] = [];
  @Input() totalRecords: number = 0;
  
  private translateService = inject(TranslateService);

  uniqueRecords = computed(() => this.totalRecords - this.duplicateRows.length);
  
  duplicatePercentage = computed(() => {
    if (this.totalRecords === 0) return 0;
    return Math.round((this.duplicateRows.length / this.totalRecords) * 100);
  });

  highConfidenceCount = computed(() => {
    return this.duplicateRows.reduce((count, row) => {
      return count + (row.duplicateInfo?.highConfidence || 0);
    }, 0);
  });

  mediumConfidenceCount = computed(() => {
    return this.duplicateRows.reduce((count, row) => {
      return count + (row.duplicateInfo?.mediumConfidence || 0);
    }, 0);
  });

  lowConfidenceCount = computed(() => {
    return this.duplicateRows.reduce((count, row) => {
      return count + (row.duplicateInfo?.lowConfidence || 0);
    }, 0);
  });

  highConfidencePercentage = computed(() => {
    if (this.duplicateRows.length === 0) return 0;
    return (this.highConfidenceCount() / this.duplicateRows.length) * 100;
  });

  mediumConfidencePercentage = computed(() => {
    if (this.duplicateRows.length === 0) return 0;
    return (this.mediumConfidenceCount() / this.duplicateRows.length) * 100;
  });

  lowConfidencePercentage = computed(() => {
    if (this.duplicateRows.length === 0) return 0;
    return (this.lowConfidenceCount() / this.duplicateRows.length) * 100;
  });

  recommendationSeverity = computed(() => {
    const highCount = this.highConfidenceCount();
    const duplicateRate = this.duplicatePercentage();
    
    if (highCount > 0 || duplicateRate > 50) return 'high-risk';
    if (duplicateRate > 25) return 'medium-risk';
    return 'low-risk';
  });

  recommendationIcon = computed(() => {
    const severity = this.recommendationSeverity();
    switch (severity) {
      case 'high-risk': return 'pi pi-exclamation-triangle';
      case 'medium-risk': return 'pi pi-info-circle';
      default: return 'pi pi-check-circle';
    }
  });

  recommendationTitle = computed(() => {
    const severity = this.recommendationSeverity();
    switch (severity) {
      case 'high-risk': return this.translateService.instant('import.duplicateSummary.risk.high.title');
      case 'medium-risk': return this.translateService.instant('import.duplicateSummary.risk.medium.title');
      default: return this.translateService.instant('import.duplicateSummary.risk.low.title');
    }
  });

  recommendationMessage = computed(() => {
    const severity = this.recommendationSeverity();
    const highCount = this.highConfidenceCount();
    
    switch (severity) {
      case 'high-risk': 
        return this.translateService.instant('import.duplicateSummary.risk.high.message', { count: highCount });
      case 'medium-risk': 
        return this.translateService.instant('import.duplicateSummary.risk.medium.message');
      default: 
        return this.translateService.instant('import.duplicateSummary.risk.low.message');
    }
  });
}
