/**
 * @fileoverview Opportunity Alt Component - Clean-room rebuild of the opportunity view
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnDestroy,
  OnInit,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { PanelModule } from 'primeng/panel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { DashboardCardComponent, DashboardCardConfig } from '@app/shared/components/data-display/dashboard-card';
import { OpportunityService } from '@partnerships/opportunities/services/opportunity.service';
import { Opportunity } from '@shared/models/opportunity.model';
import { Subscription } from 'rxjs';

/**
 * @class OpportunityAltComponent
 * @description Clean-room rebuild of the opportunity view using only PrimeNG and shared components.
 * Sakai-style dashboard layout with summary stat cards and content panels.
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-alt',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    AvatarModule,
    ButtonModule,
    CardModule,
    ChipModule,
    DividerModule,
    MessageModule,
    PanelModule,
    ProgressSpinnerModule,
    TableModule,
    TagModule,
    TooltipModule,
    DashboardCardComponent,
  ],
  templateUrl: './opportunity-alt.component.html',
  styleUrl: './opportunity-alt.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityAltComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly opportunityService = inject(OpportunityService);

  readonly loading = signal(false);
  readonly opportunity = signal<Opportunity | null>(null);
  readonly recordId = signal<number | null>(null);

  // --- Header signals ---
  readonly opportunityName = computed(() => this.opportunity()?.name ?? '');
  readonly stageName = computed(() => this.opportunity()?.workflowStatus ?? '');
  readonly hasRecord = computed(() => this.recordId() !== null);
  readonly orgUnitName = computed(() => this.opportunity()?.responsibleOrgUnitName ?? '');
  readonly initiativeType = computed(() => this.opportunity()?.proposedInitiativeTypeName ?? '');

  // --- Stat card signals ---
  readonly budgetDisplay = computed(() => {
    const budget = this.opportunity()?.initiativeBudgetUSD;
    return budget != null ? budget : null;
  });

  readonly totalFundingDisplay = computed(() => {
    const funding = this.opportunity()?.stats?.totalFundingUSD;
    return funding != null ? funding : null;
  });

  readonly fundingPartnerCount = computed(() => this.opportunity()?.stats?.fundingPartnerCount ?? 0);
  readonly clientPartnerCount = computed(() => this.opportunity()?.stats?.clientPartnerCount ?? 0);
  readonly partnerCount = computed(() => this.opportunity()?.stats?.totalPartnerCount ?? 0);
  readonly deliverableCount = computed(() => this.opportunity()?.stats?.deliverableCount ?? 0);
  readonly countryCount = computed(() => this.opportunity()?.stats?.countryCount ?? 0);
  readonly sdgCount = computed(() => this.opportunity()?.stats?.sdgCount ?? 0);
  readonly daysToSigning = computed(() => this.opportunity()?.stats?.daysToTargetSigningDate ?? null);

  // --- Collection signals ---
  readonly fundingPartners = computed(() => this.opportunity()?.fundingPartners ?? []);
  readonly deliverables = computed(() => this.opportunity()?.deliverables ?? []);
  readonly countries = computed(() => this.opportunity()?.countries ?? []);
  readonly sdgs = computed(() => this.opportunity()?.sdGs ?? []);

  // --- Key dates ---
  readonly targetSigningDate = computed(() => this.opportunity()?.targetSigningDate ?? null);
  readonly implementationStartDate = computed(() => this.opportunity()?.implementationStartDate ?? null);
  readonly targetDeliveryDate = computed(() => this.opportunity()?.targetDeliveryDate ?? null);
  readonly submissionDeadline = computed(() => this.opportunity()?.submissionDeadline ?? null);

  // --- Dashboard card configs ---
  readonly fundingPartnersCardConfig: DashboardCardConfig = {
    icon: 'account_balance',
    iconColor: 'bg-unops-primary/10',
    title: 'Funding Partners',
    subtitle: 'Financial contributors to this opportunity',
    size: 'auto',
    showViewAll: false,
  };

  readonly deliverablesCardConfig: DashboardCardConfig = {
    icon: 'inventory_2',
    iconColor: 'bg-unops-accent-orange/10',
    title: 'Deliverables',
    subtitle: 'Products and services',
    size: 'auto',
    showViewAll: false,
  };

  readonly keyDatesCardConfig: DashboardCardConfig = {
    icon: 'calendar_month',
    iconColor: 'bg-unops-info/10',
    title: 'Key Dates',
    subtitle: 'Timeline milestones',
    size: 'auto',
    showViewAll: false,
  };

  readonly countriesAndSdgsCardConfig: DashboardCardConfig = {
    icon: 'public',
    iconColor: 'bg-unops-success/10',
    title: 'Countries & SDGs',
    subtitle: 'Geographic and alignment coverage',
    size: 'auto',
    showViewAll: false,
  };

  private subscriptions = new Subscription();

  ngOnInit(): void {
    this.subscriptions.add(
      this.route.params.subscribe(params => {
        const id = params['recordId'];
        if (id) {
          const numericId = Number(id);
          this.recordId.set(numericId);
          this.loadOpportunity(numericId);
        } else {
          this.recordId.set(null);
          this.opportunity.set(null);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private loadOpportunity(id: number): void {
    this.loading.set(true);
    this.subscriptions.add(
      this.opportunityService.getOpportunityById(id).subscribe({
        next: (response: any) => {
          const data: Opportunity = response.opportunity || response;
          this.opportunity.set(data);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        },
      })
    );
  }

  navigateToOriginal(): void {
    const id = this.recordId();
    if (id) {
      this.router.navigate(['/partnerships/opportunities', id]);
    } else {
      this.router.navigate(['/partnerships/opportunities']);
    }
  }

  getStageSeverity(): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    const stage = this.stageName()?.toLowerCase();
    if (!stage) return 'info';
    if (stage.includes('active') || stage.includes('go')) return 'success';
    if (stage.includes('pending') || stage.includes('approval')) return 'warn';
    if (stage.includes('reject') || stage.includes('no go') || stage.includes('cancelled')) return 'danger';
    if (stage.includes('draft')) return 'secondary';
    return 'info';
  }
}
