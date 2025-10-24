/**
 * @fileoverview Option 1: Unified Dashboard View - All information visible in single scrolling page
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { AvatarGroupModule } from 'primeng/avatargroup';
import { TimelineModule } from 'primeng/timeline';
import { MessageModule } from 'primeng/message';

// Services
import {
  OpportunityDemoService,
  DemoOpportunity,
} from '@shared/services/api/opportunity-demo.service';

/**
 * @class OpportunityOption1Component
 * @description Unified Dashboard View - displays all opportunity information in a single scrolling page
 * with 5W framework (What, Who, Why, When, Where). Emphasizes complete context at a glance.
 *
 * @example
 * ```html
 * <app-opportunity-option1></app-opportunity-option1>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-option1',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    CardModule,
    BadgeModule,
    TagModule,
    ChipModule,
    DividerModule,
    AvatarModule,
    AvatarGroupModule,
    TimelineModule,
    MessageModule,
  ],
  templateUrl: './opportunity-option1.component.html',
  styleUrls: ['./opportunity-option1.component.scss'],
})
export class OpportunityOption1Component implements OnInit {
  private demoService = inject(OpportunityDemoService);

  opportunity = signal<DemoOpportunity | null>(null);
  loading = signal(true);
  aiSuggestions = signal<string[]>([]);
  showAIPanel = signal(true);

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
    this.loadAISuggestions();
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

  private loadAISuggestions(): void {
    this.demoService.getAISuggestions().subscribe({
      next: (suggestions) => {
        this.aiSuggestions.set(suggestions);
      },
    });
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
}
