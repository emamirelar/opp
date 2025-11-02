/**
 * @fileoverview Opportunity Analysis Section Component - Displays quick stats and AI insights
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, signal, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';

// Models
import { Opportunity, InsightType } from '@shared/models/opportunity.model';

/**
 * @class OpportunityAnalysisSectionComponent
 * @description Displays analysis section with quick stats from backend, AI insights, and quick actions.
 * This component is read-only and displays statistics and insights provided by the backend.
 * 
 * @example
 * ```html
 * <app-opportunity-analysis-section
 *   [opportunity]="opportunity()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-analysis-section',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DividerModule,
  ],
  templateUrl: './opportunity-analysis-section.component.html',
  styleUrls: ['./opportunity-analysis-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityAnalysisSectionComponent {
  // Services
  private readonly translateService = inject(TranslateService);

  /**
   * @description Input signal for opportunity data from parent
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Get icon class based on insight type
   * @param {InsightType} type - The insight type
   * @returns {string} Icon class string
   */
  getInsightIcon(type: InsightType): string {
    switch (type) {
      case 'info':
        return 'pi pi-info-circle text-blue-600';
      case 'warning':
        return 'pi pi-exclamation-triangle text-yellow-600';
      case 'success':
        return 'pi pi-check-circle text-green-600';
      default:
        return 'pi pi-info-circle text-blue-600';
    }
  }

  /**
   * @description Format currency value
   * @param {number} value - Currency value to format
   * @returns {string} Formatted currency string
   */
  formatCurrency(value: number | null | undefined): string {
    if (value == null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  /**
   * @description Format date value
   * @param {string | Date} dateValue - Date value to format
   * @returns {string} Formatted date string
   */
  formatDate(dateValue: string | Date | null | undefined): string {
    if (!dateValue) return '-';
    
    const date = typeof dateValue === 'string' ? new Date(dateValue) : dateValue;
    
    if (isNaN(date.getTime())) return '-';
    
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  /**
   * @description Handle View DST Analysis action
   * Scrolls to the DST section on the page
   */
  onViewDSTAnalysis(): void {
    // Scroll to DST section
    const dstSection = document.getElementById('section-dst');
    if (dstSection) {
      dstSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  /**
   * @description Handle Generate Budget Draft action
   */
  onGenerateBudgetDraft(): void {
    // TODO: Implement Budget Draft generation when backend is ready
    console.log('Generate Budget Draft clicked');
  }
}

