/**
 * @fileoverview Decision Info Panel component for Go/No-Go decision makers
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { TagModule } from 'primeng/tag';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';

// Models
import {
  Opportunity,
  OpportunityFundingPartner,
  OpportunityClientPartner,
  Risk,
} from '@shared/models/opportunity.model';

/**
 * Interface for concerning DD status information
 */
interface ConcerningDDStatus {
  partnerName: string;
  partnerType: 'Funding' | 'Client';
  ddStatus: string;
  ddExpiryDate: Date | null;
}

/**
 * Interface for high risk information
 */
interface HighRiskInfo {
  title: string;
  categoryName: string | null;
  impactLevel: string | null;
  isPredefined: boolean;
}

/**
 * @class OpportunityDecisionInfoPanelComponent
 * @description Displays key information for decision makers during Go/No-Go workflow.
 * Highlights partner DD statuses, high risks, time to signing, and sender remarks.
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-decision-info-panel',
  templateUrl: './opportunity-decision-info-panel.component.html',
  styleUrl: './opportunity-decision-info-panel.component.scss',
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    TagModule,
    MessageModule,
    DividerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityDecisionInfoPanelComponent {
  /**
   * @description The opportunity being reviewed
   * @type {InputSignal<Opportunity>}
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Optional workflow submission comment/remarks from submitter
   * @type {InputSignal<string | null | undefined>}
   */
  readonly senderRemarks = input<string | null | undefined>(null);

  /**
   * @description Optional list of risks loaded separately
   * @type {InputSignal<Risk[]>}
   */
  readonly risks = input<Risk[]>([]);

  /**
   * @description Computed initiative type name
   */
  readonly initiativeType = computed(() => {
    return this.opportunity()?.proposedInitiativeTypeName || 'Not specified';
  });

  /**
   * @description Computed time to signing in days
   * Negative values indicate past due
   */
  readonly timeToSigning = computed(() => {
    const opp = this.opportunity();
    if (!opp?.targetSigningDate) return null;

    const targetDate = new Date(opp.targetSigningDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    targetDate.setHours(0, 0, 0, 0);

    const diffTime = targetDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays;
  });

  /**
   * @description Formatted time to signing display
   */
  readonly timeToSigningDisplay = computed(() => {
    const days = this.timeToSigning();
    if (days === null) return 'Not set';
    if (days < 0) return `${Math.abs(days)} days overdue`;
    if (days === 0) return 'Today';
    if (days === 1) return '1 day';
    return `${days} days`;
  });

  /**
   * @description Time to signing severity for styling
   */
  readonly timeToSigningSeverity = computed((): 'success' | 'warn' | 'danger' | 'info' => {
    const days = this.timeToSigning();
    if (days === null) return 'info';
    if (days < 0) return 'danger';
    if (days <= 14) return 'warn';
    return 'success';
  });

  /**
   * @description Partners with concerning DD statuses (Pending, Expired, Expiring Soon, Not Approved)
   */
  readonly concerningDDStatuses = computed((): ConcerningDDStatus[] => {
    const opp = this.opportunity();
    if (!opp) return [];

    const concerning: ConcerningDDStatus[] = [];
    const concerningStatuses = ['Pending', 'Expired', 'Expiring Soon', 'Not Approved', 'Required'];

    // Check funding partners
    if (opp.fundingPartners) {
      opp.fundingPartners.forEach((partner: OpportunityFundingPartner) => {
        if (partner.ddStatus && concerningStatuses.some((s) => partner.ddStatus?.includes(s))) {
          concerning.push({
            partnerName: partner.partnerName,
            partnerType: 'Funding',
            ddStatus: partner.ddStatus,
            ddExpiryDate: partner.ddExpiryDate,
          });
        }
      });
    }

    // Check client partners
    if (opp.clientPartners) {
      opp.clientPartners.forEach((partner: OpportunityClientPartner) => {
        if (partner.ddStatus && concerningStatuses.some((s) => partner.ddStatus?.includes(s))) {
          concerning.push({
            partnerName: partner.partnerName,
            partnerType: 'Client',
            ddStatus: partner.ddStatus,
            ddExpiryDate: partner.ddExpiryDate,
          });
        }
      });
    }

    return concerning;
  });

  /**
   * @description High risks that require attention
   * Includes predefined high risks and risks with high impact level
   */
  readonly highRisks = computed((): HighRiskInfo[] => {
    const riskList = this.risks();
    if (!riskList || riskList.length === 0) return [];

    return riskList
      .filter((risk: Risk) => {
        // Include if it's a predefined high risk
        if (risk.preDefinedHighRiskId) return true;
        // Include if impact level is High (typically numericValue = 3 or name contains 'High')
        if (risk.riskImpactLevelName?.toLowerCase().includes('high')) return true;
        // Legacy: impact = 3 means High
        if (risk.impact === 3) return true;
        return false;
      })
      .map((risk: Risk) => ({
        title: risk.preDefinedHighRiskTitle || risk.title,
        categoryName: risk.riskCategoryName,
        impactLevel: risk.riskImpactLevelName,
        isPredefined: !!risk.preDefinedHighRiskId,
      }));
  });

  /**
   * @description Whether there are any concerning items to display
   */
  readonly hasConcerningItems = computed(() => {
    return this.concerningDDStatuses().length > 0 || this.highRisks().length > 0;
  });

  /**
   * @description Budget display formatted
   */
  readonly budgetDisplay = computed(() => {
    const opp = this.opportunity();
    if (!opp?.initiativeBudgetUSD) return 'Not specified';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      maximumFractionDigits: 0,
    }).format(opp.initiativeBudgetUSD);
  });

  /**
   * @description Responsible org unit name
   */
  readonly orgUnitName = computed(() => {
    return this.opportunity()?.responsibleOrgUnitName || 'Not specified';
  });
}
