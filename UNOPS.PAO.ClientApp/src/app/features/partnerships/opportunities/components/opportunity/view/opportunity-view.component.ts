/**
 * @fileoverview Opportunity View Component - Unified Dashboard View
 * @author UNOPS Opportunity+ System Development Team
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { AvatarModule } from 'primeng/avatar';

// Services
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils';
import { OpportunityService } from '../../../services/opportunity.service';
import { Opportunity } from '@shared/models/opportunity.model';
import { CommentComponent } from '@shared/components/comments/comment.component';
import { OpportunityWhatSectionComponent } from './sections/opportunity-what-section.component';
import { ValuesService } from '@app/shared/services/api/values.service';

/**
 * @class OpportunityViewComponent
 * @description Unified Dashboard View - displays all opportunity information in a single scrolling page
 * with comprehensive details. Uses real API data from the Opportunity backend.
 *
 * @example
 * ```html
 * <app-opportunity-view></app-opportunity-view>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-view',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DividerModule,
    MessageModule,
    RouterModule,
    ConfirmDialogModule,
    CardModule,
    BadgeModule,
    TagModule,
    ChipModule,
    AvatarModule,
    CommentComponent,
    OpportunityWhatSectionComponent,
  ],
  templateUrl: './opportunity-view.component.html',
  styleUrls: ['./opportunity-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService]
})
export class OpportunityViewComponent implements OnInit, OnDestroy {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  opportunityService = inject(OpportunityService);
  valuesService = inject(ValuesService);
  permissionUtilityService = inject(PermissionUtilityService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  feedbackDialogService = inject(FeedbackDialogService);
  confirmationService = inject(ConfirmationService);
  private pageContextService = inject(PageContextService);

  // State
  loading = signal<boolean>(true);
  recordId: string = '';
  opportunity = signal<Opportunity | null>(null);
  showAIPanel = signal<boolean>(true); // AI Assistant panel toggle state

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createInstancePermissions('Opportunity');
  recordPermissions = this.permissionUtils.recordPermissions;

  // Computed properties for conditional display
  showAdditionalInfo = computed(() => {
    const data = this.opportunity();
    if (!data) return false;
    return data.workflowStageName || data.responsibleOrgUnitName || 
           data.partnershipAgreementReference || data.initiativeBudgetUSD ||
           data.targetSigningDate || data.targetDeliveryDate || 
           data.proposedInitiativeTypeName;
  });

  // Get opportunity manager from stakeholders (internal stakeholder with "Opportunity Manager" role)
  opportunityManager = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders || opp.stakeholders.length === 0) return null;
    
    // Find the first internal stakeholder with "Opportunity Manager" role
    const manager = opp.stakeholders.find(s => 
      s.isInternal && 
      s.entityRoleName && 
      s.entityRoleName.toLowerCase().includes('opportunity') &&
      s.entityRoleName.toLowerCase().includes('manager')
    );
    
    return manager ? (manager.userName || manager.userEmail || '-') : '-';
  });

  showFullContent = signal<boolean>(false);

  shouldShowSeeMoreButton = computed(() => {
    return this.showAdditionalInfo() && !this.showFullContent();
  });

  shouldShowSeeLessButton = computed(() => {
    return this.showAdditionalInfo() && this.showFullContent();
  });

  // Computed stats from backend or calculated from child entities
  totalFunding = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.totalFundingUSD || 0;
  });

  totalFees = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.totalFeeAmountUSD || 0;
  });

  fundingPartnerCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.fundingPartnerCount || opp.fundingPartners?.length || 0;
  });

  clientPartnerCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.clientPartnerCount || opp.clientPartners?.length || 0;
  });

  stakeholderCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.stakeholderCount || opp.stakeholders?.length || 0;
  });

  internalStakeholderCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.internalStakeholderCount || 
           opp.stakeholders?.filter(s => s.isInternal).length || 0;
  });

  externalStakeholderCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.externalStakeholderCount || 
           opp.stakeholders?.filter(s => !s.isInternal).length || 0;
  });

  deliverableCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.deliverableCount || opp.deliverables?.length || 0;
  });

  countryCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.countryCount || opp.countries?.length || 0;
  });

  sdgCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.stats?.sdgCount || opp.sdGs?.length || 0;
  });

  // Filtered stakeholder lists
  internalStakeholders = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders) return [];
    return opp.stakeholders.filter(s => s.isInternal);
  });

  externalStakeholders = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders) return [];
    return opp.stakeholders.filter(s => !s.isInternal);
  });

  primarySDG = computed(() => {
    const opp = this.opportunity();
    if (!opp) return null;
    return opp.sdGs?.find(s => s.isPrimary) || null;
  });

  ngOnInit() {
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);

    // Subscribe to route parameter changes
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        const newRecordId = paramMap.get("recordId") || '';
        if (newRecordId) {
          this.recordId = newRecordId;
          this._loadRecordDetails();
        }
      }
    });
  }

  ngOnDestroy(): void {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();
  }

  /**
   * Load opportunity record details
   */
  private _loadRecordDetails() {
    this.loading.set(true);
    this.opportunityService.getOpportunityById(+this.recordId).subscribe({
      next: (data: Opportunity) => {
        this.opportunity.set(data);
        this.loading.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading opportunity details:', error);
        this.loading.set(false);
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('message.opportunity.loadFailed'),
          summary: this.translateService.instant('message.error')
        });
      }
    });
  }

  /**
   * Handle edit button click
   */
  handleEditClick() {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.noPermissionToEdit'),
        summary: this.translateService.instant('message.permissionDenied')
      });
      return;
    }

    // TODO: Implement edit dialog
    this.feedbackDialogService.showInfoToast({
      detail: this.translateService.instant('message.opportunity.editComingSoon'),
      summary: this.translateService.instant('message.info')
    });
  }

  /**
   * Delete opportunity with confirmation
   */
  deleteOpportunity(): void {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.noPermissionToDelete'),
        summary: this.translateService.instant('message.permissionDenied')
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.translateService.instant('message.confirmation.deleteOpportunity'),
      header: this.translateService.instant('title.deleteOpportunity'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        if (this.opportunity()?.id) {
          this.opportunityService.deleteOpportunityById(this.opportunity()!.id!).subscribe({
            next: () => {
              this.feedbackDialogService.showSuccessToast({
                detail: this.translateService.instant('message.opportunity.deletedSuccessfully'),
                summary: this.translateService.instant('message.success')
              });
              this.router.navigate(['/partnerships/opportunities']);
            },
            error: (error) => {
              console.error('Error deleting opportunity:', error);
              // Error handled by global interceptor
            }
          });
        }
      }
    });
  }

  /**
   * Handle opportunity update from child section components
   * This method receives the full updated opportunity from child components
   * and refreshes the master opportunity signal, triggering updates across all sections
   */
  handleOpportunityUpdate(updatedOpportunity: Opportunity): void {
    // Replace entire opportunity signal with fresh data from backend
    this.opportunity.set(updatedOpportunity);
    
    // Angular signals automatically notify ALL child components
    // All sections will re-render with latest data
    this.cdr.detectChanges();
  }

  /**
   * Toggle full content display
   */
  toggleFullContent() {
    this.showFullContent.update(value => !value);
  }

  /**
   * Format currency value
   */
  formatCurrency(value: number | undefined | null): string {
    if (value === undefined || value === null) return '-';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  /**
   * Format date value
   */
  formatDate(date: Date | string | undefined | null): string {
    if (!date) return '-';
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return dateObj.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  /**
   * Toggle AI Assistant panel visibility
   */
  toggleAIPanel(): void {
    this.showAIPanel.set(!this.showAIPanel());
  }

  /**
   * Get status severity class for badges
   */
  getStatusSeverity(status: string | undefined): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    if (!status) return 'secondary';
    switch (status.toLowerCase()) {
      case 'active':
        return 'success';
      case 'pending':
        return 'warn';
      case 'onhold':
        return 'danger';
      case 'inactive':
        return 'secondary';
      default:
        return 'info';
    }
  }

  /**
   * Get risk score severity for country risk badges
   */
  getRiskScoreSeverity(riskScore: number): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    if (riskScore >= 8) return 'danger';
    if (riskScore >= 6) return 'warn';
    if (riskScore >= 4) return 'info';
    return 'success';
  }
}

