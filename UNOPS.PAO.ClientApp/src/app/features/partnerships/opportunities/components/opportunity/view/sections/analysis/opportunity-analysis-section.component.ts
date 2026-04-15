/**
 * @fileoverview Opportunity Analysis Section Component - Displays quick stats and AI insights
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';

// Models
import { Opportunity, InsightType, InsightPriority } from '@shared/models/opportunity.model';

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
    TagModule,
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
   * @description Input signal to trigger insights refresh when any section saves
   * Parent should increment this value when any section saves successfully
   * @type {Signal<number>}
   * @since 2.0.0
   */
  readonly sectionSaveTrigger = input<number>(0);

  /**
   * @description AI-generated insights passed from parent (prevents duplicate API calls)
   * @type {Signal<any[]>}
   * @since 2.1.0
   */
  readonly insights = input<any[]>([]);

  /**
   * @description AI-generated suggestions passed from parent (prevents duplicate API calls)
   * @type {Signal<any[]>}
   * @since 2.1.0
   */
  readonly suggestions = input<any[]>([]);

  /**
   * @description Loading state for insights passed from parent
   * @type {Signal<boolean>}
   * @since 2.1.0
   */
  readonly loadingInsights = input<boolean>(false);

  /**
   * @description True when insights are being refreshed after a section save (includes delay before API call)
   * @type {Signal<boolean>}
   * @since 2.1.0
   */
  readonly insightsRefreshingPending = input<boolean>(false);

  /**
   * @description Computed: true when insights are loading or refreshing (show loading indicator)
   */
  readonly isRefreshingInsights = computed(
    () => this.loadingInsights() || this.insightsRefreshingPending()
  );

  /**
   * @description Error message for insights loading passed from parent
   * @type {Signal<string | null>}
   * @since 2.1.0
   */
  readonly insightsError = input<string | null>(null);

  /**
   * @description Output event to request parent component to refresh insights
   * @type {OutputEmitterRef<void>}
   * @since 2.1.0
   */
  readonly refreshRequested = output<void>();

  constructor() {
    // NOTE: Insights loading removed from child component to prevent duplicate API calls
    // The parent component now loads insights once and passes them as input signals
    // This eliminates the duplicate getInsights() API call that was occurring on every page load
    
    // TODO: In the future, add an output event to request parent to refresh insights
    // when sectionSaveTrigger changes, so the parent can reload insights after saves
  }

  /**
   * @description Request parent component to refresh insights
   * Emits an event that tells the parent to reload insights from the API
   * @returns {void}
   * @since 2.1.0
   */
  refreshInsights(): void {
    this.refreshRequested.emit();
  }

  /**
   * @description Map insight priority to PrimeNG Tag severity
   * @param {InsightPriority} priority - The insight priority level
   * @returns {string} PrimeNG tag severity
   */
  getInsightPrioritySeverity(priority: InsightPriority): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    switch (priority) {
      case 'high':
        return 'danger';
      case 'medium':
        return 'warn';
      case 'low':
        return 'info';
      default:
        return 'secondary';
    }
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
   * @description Handle View Risks Analysis action
   * Scrolls to the Risks section on the page
   */
  onViewDSTAnalysis(): void {
    // Scroll to Risks section
    const risksSection = document.getElementById('section-risks');
    if (risksSection) {
      risksSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
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
   * @param {any} suggestion - The suggestion with action target
   */
  onSuggestionAction(suggestion: any): void {
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
   * @param {string} actionTarget - The section identifier (WHAT, WHERE, WHY, WHO, TEAM, WHEN)
   * @returns {string} Localized button label
   */
  getActionLabel(actionTarget: string): string {
    const labelMap: Record<string, string> = {
      'WHAT': this.translateService.instant('button.goToWhatSection'),
      'WHERE': this.translateService.instant('button.goToWhereSection'),
      'WHY': this.translateService.instant('button.goToWhySection'),
      'WHO': this.translateService.instant('button.goToWhoSection'),
      'TEAM': this.translateService.instant('button.goToTeamSection'),
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

