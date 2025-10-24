/**
 * @fileoverview Option 2: Tabbed Content Organization - Organized by logical sections
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG
import { TabViewModule } from 'primeng/tabview';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageModule } from 'primeng/message';

// Services
import {
  OpportunityDemoService,
  DemoOpportunity,
} from '@shared/services/api/opportunity-demo.service';

/**
 * @class OpportunityOption2Component
 * @description Tabbed Content Organization - information grouped by logical categories
 * with focused work environment and reduced clutter.
 *
 * @example
 * ```html
 * <app-opportunity-option2></app-opportunity-option2>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-option2',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    TabViewModule,
    PanelModule,
    ButtonModule,
    CardModule,
    BadgeModule,
    TagModule,
    ChipModule,
    DividerModule,
    AvatarModule,
    ProgressBarModule,
    MessageModule,
  ],
  templateUrl: './opportunity-option2.component.html',
  styleUrls: ['./opportunity-option2.component.scss'],
})
export class OpportunityOption2Component implements OnInit {
  private demoService = inject(OpportunityDemoService);

  opportunity = signal<DemoOpportunity | null>(null);
  loading = signal(true);
  activeTabIndex = signal(0);
  showAIPanel = signal(true);

  // Tab-specific AI suggestions
  tabAISuggestions = computed(() => {
    const tabIndex = this.activeTabIndex();
    const suggestions: { [key: number]: string[] } = {
      0: [
        'All required information complete for Overview',
        'Consider adding more detail to deliverable descriptions',
      ],
      1: [
        'Client contact information is missing',
        'Recommend adding gender advisor to team',
      ],
      2: [
        'Fee calculations verified',
        'Currency risk noted for EUR commitment',
      ],
      3: [
        'Timeline feasible based on similar projects',
        'Monsoon season constraints identified',
      ],
      4: [
        'Myanmar flagged as high-risk context',
        'Consider phased implementation approach',
      ],
      5: [
        '4 risks identified requiring attention',
        'View full DST report for detailed analysis',
      ],
    };
    return suggestions[tabIndex] || [];
  });

  // Computed properties
  totalFunding = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.fundingPartners.reduce((sum, p) => sum + p.amount, 0);
  });

  totalFees = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.fundingPartners.reduce((sum, p) => sum + p.feeAmount, 0);
  });

  ngOnInit(): void {
    this.loadOpportunity();
  }

  private loadOpportunity(): void {
    this.loading.set(true);
    this.demoService.getDemoOpportunity().subscribe({
      next: (opp) => {
        this.opportunity.set(opp);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading demo opportunity:', error);
        this.loading.set(false);
      },
    });
  }

  onTabChange(event: any): void {
    this.activeTabIndex.set(event.index);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getSeverityClass(severity: string): string {
    const severityMap: { [key: string]: string } = {
      High: 'danger',
      Medium: 'warning',
      Low: 'info',
    };
    return severityMap[severity] || 'info';
  }

  toggleAIPanel(): void {
    this.showAIPanel.update((v) => !v);
  }

  getTabCompleteness(tabIndex: number): number {
    const completeness: { [key: number]: number } = {
      0: 100, // Overview
      1: 75, // Stakeholders
      2: 90, // Finances
      3: 85, // Timeline
      4: 100, // Geography
      5: 100, // AI Insights
    };
    return completeness[tabIndex] || 0;
  }
}
