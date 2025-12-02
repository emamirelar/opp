/**
 * @fileoverview Opportunity Analysis Section Component - Displays quick stats and AI insights
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, signal, inject, ChangeDetectionStrategy, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TooltipModule } from 'primeng/tooltip';

// Models
import { Opportunity, InsightType, OpportunityInsight, OpportunitySuggestion, OpportunityInsightsResponse } from '@shared/models/opportunity.model';

// Services
import { OpportunityService } from '../../../../../services/opportunity.service';

/**
 * @class OpportunityAnalysisSectionComponent
 * @description Displays analysis section with quick stats from backend, AI-generated insights, and suggestions.
 * This component loads AI insights on-demand and displays them with appropriate styling.
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
    TooltipModule,
  ],
  templateUrl: './opportunity-analysis-section.component.html',
  styleUrls: ['./opportunity-analysis-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityAnalysisSectionComponent {
  // Services
  private readonly translateService = inject(TranslateService);
  private readonly opportunityService = inject(OpportunityService);

  /**
   * @description Input signal for opportunity data from parent
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Loading state for insights
   */
  readonly loadingInsights = signal<boolean>(false);

  /**
   * @description AI-generated insights
   */
  readonly insights = signal<OpportunityInsight[]>([]);

  /**
   * @description AI-generated suggestions
   */
  readonly suggestions = signal<OpportunitySuggestion[]>([]);

  /**
   * @description Error message for insights loading
   */
  readonly insightsError = signal<string | null>(null);

  /**
   * @description Track last loaded opportunity ID to prevent duplicate calls
   */
  private lastLoadedOpportunityId: number | null = null;

  constructor() {
    // Use effect to reactively load insights when opportunity changes
    effect(() => {
      const opp = this.opportunity();
      if (opp?.id && opp.id !== this.lastLoadedOpportunityId) {
        this.lastLoadedOpportunityId = opp.id;
        this.loadInsights();
      }
    });
  }

  /**
   * @description Load AI insights and suggestions for the opportunity
   */
  private loadInsights(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    this.loadingInsights.set(true);
    this.insightsError.set(null);

    this.opportunityService.getInsights(opportunityId).subscribe({
      next: (response: OpportunityInsightsResponse) => {
        // Add unique IDs for tracking
        const insightsWithIds = response.insights.map((insight, idx: number) => ({
          ...insight,
          id: idx
        }));
        const suggestionsWithIds = response.suggestions.map((suggestion, idx: number) => ({
          ...suggestion,
          id: idx
        }));

        this.insights.set(insightsWithIds as any);
        this.suggestions.set(suggestionsWithIds as any);
        this.loadingInsights.set(false);
      },
      error: (error: any) => {
        console.error('Error loading insights:', error);
        this.insightsError.set('Failed to load AI insights');
        this.loadingInsights.set(false);
      }
    });
  }

  /**
   * @description Manually refresh insights
   */
  refreshInsights(): void {
    this.loadInsights();
  }

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

  /**
   * @description Handle suggestion action click - clicks the appropriate section button
   * @param {OpportunitySuggestion} suggestion - The suggestion with action target
   */
  onSuggestionAction(suggestion: OpportunitySuggestion): void {
    if (!suggestion.actionTarget) {
      console.warn('No action target specified for suggestion:', suggestion);
      return;
    }

    // Map AI target (WHAT) to section label (What)
    const targetLabel = suggestion.actionTarget.charAt(0) + suggestion.actionTarget.slice(1).toLowerCase();

    // Find the section button by its text content (What, Where, Why, Who, When)
    const buttons = document.querySelectorAll('button');
    const targetButton = Array.from(buttons).find(btn => {
      const buttonText = btn.textContent?.trim();
      return buttonText === targetLabel;
    });

    if (targetButton) {
      targetButton.click();
      console.log(`Clicked section button: ${targetLabel}`);
    } else {
      console.warn(`Section button not found for: ${suggestion.actionTarget} (looking for: ${targetLabel})`);
    }
  }

  /**
   * @description Get button label based on action target
   * @param {string} actionTarget - The section identifier (WHAT, WHERE, WHY, WHO, WHEN)
   * @returns {string} Localized button label
   */
  getActionLabel(actionTarget: string): string {
    const labelMap: Record<string, string> = {
      'WHAT': this.translateService.instant('button.goToWhatSection'),
      'WHERE': this.translateService.instant('button.goToWhereSection'),
      'WHY': this.translateService.instant('button.goToWhySection'),
      'WHO': this.translateService.instant('button.goToWhoSection'),
      'WHEN': this.translateService.instant('button.goToWhenSection')
    };
    return labelMap[actionTarget] || this.translateService.instant('button.viewDetails');
  }

  /**
   * @description Scroll to a specific section on the page
   * @param {string} sectionId - The ID of the section to scroll to
   */
  scrollToSection(sectionId: string): void {
    const section = document.getElementById(sectionId);
    if (section) {
      section.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}

