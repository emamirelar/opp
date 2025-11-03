/**
 * @fileoverview DST (Digital Strategy & Transformation) Section Component
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, computed, inject, input, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { BadgeModule } from 'primeng/badge';

// Models
import { Opportunity, DSTSeverity } from '@shared/models/opportunity.model';

/**
 * @class OpportunityDstSectionComponent
 * @description Component for displaying DST Insights & Recommendations section.
 * Shows AI-powered complexity scores, risks, recommendations, and similar opportunities.
 * 
 * @example
 * ```html
 * <app-opportunity-dst-section
 *   [opportunity]="opportunity()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-dst-section',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DividerModule,
    TagModule,
    ChipModule,
    BadgeModule,
  ],
  templateUrl: './opportunity-dst-section.component.html',
  styleUrls: ['./opportunity-dst-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityDstSectionComponent {
  /**
   * @description The opportunity data containing DST analysis
   * @type {Signal<Opportunity>}
   * @since 1.0.0
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Check if DST analysis data is available
   * @type {Signal<boolean>}
   * @since 1.0.0
   */
  readonly hasDSTAnalysis = computed(() => {
    const opp = this.opportunity();
    return !!(opp && opp.dstAnalysis);
  });

  /**
   * @description Get severity class for PrimeNG tag component
   * @param {DSTSeverity} severity - The severity level
   * @returns {string} PrimeNG severity class
   * @since 1.0.0
   */
  getSeverityClass(severity: DSTSeverity): 'danger' | 'warning' | 'success' {
    switch (severity) {
      case 'High':
        return 'danger';
      case 'Medium':
        return 'warning';
      case 'Low':
        return 'success';
      default:
        return 'warning';
    }
  }

  /**
   * @description Format currency for display
   * @param {number} amount - The amount to format
   * @returns {string} Formatted currency string
   * @since 1.0.0
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  /**
   * @description Handle refresh analysis action
   * Placeholder for future AI analysis refresh functionality
   * @since 1.0.0
   */
  refreshAnalysis(): void {
    // TODO: Implement AI analysis refresh when backend is ready
    console.log('Refresh DST Analysis');
  }

  /**
   * @description Handle add risk to register action
   * Placeholder for future risk register integration
   * @since 1.0.0
   */
  addToRiskRegister(): void {
    // TODO: Implement risk register integration when backend is ready
    console.log('Add risks to risk register');
  }

  /**
   * @description Handle accept recommendation action
   * @param {number} recommendationId - The recommendation ID to accept
   * @since 1.0.0
   */
  acceptRecommendation(recommendationId: number): void {
    // TODO: Implement recommendation acceptance when backend is ready
    console.log('Accept recommendation:', recommendationId);
  }

  /**
   * @description Handle dismiss recommendation action
   * @param {number} recommendationId - The recommendation ID to dismiss
   * @since 1.0.0
   */
  dismissRecommendation(recommendationId: number): void {
    // TODO: Implement recommendation dismissal when backend is ready
    console.log('Dismiss recommendation:', recommendationId);
  }

  /**
   * @description View details of similar opportunity
   * @param {number} opportunityId - The opportunity ID to view
   * @since 1.0.0
   */
  viewSimilarOpportunity(opportunityId: number): void {
    // TODO: Implement navigation to similar opportunity when backend is ready
    console.log('View similar opportunity:', opportunityId);
  }
}

