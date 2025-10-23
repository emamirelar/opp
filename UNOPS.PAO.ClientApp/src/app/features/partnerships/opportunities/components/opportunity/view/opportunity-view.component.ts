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

// Services
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils';
import { OpportunityService } from '../../../services/opportunity.service';
import { Opportunity } from '../../../models/opportunity.model';

// Components
import { GoBackComponent } from '@shared/components/navigation/go-back/go-back.component';

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
    GoBackComponent
  ],
  templateUrl: './opportunity-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService]
})
export class OpportunityViewComponent implements OnInit, OnDestroy {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  opportunityService = inject(OpportunityService);
  permissionUtilityService = inject(PermissionUtilityService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  feedbackDialogService = inject(FeedbackDialogService);
  confirmationService = inject(ConfirmationService);
  private pageContextService = inject(PageContextService);

  infoLoading = signal<boolean>(false);
  recordId: string = '';
  recordData = signal<Opportunity>({
    name: '',
    description: ''
  });

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createInstancePermissions('Opportunity');
  recordPermissions = this.permissionUtils.recordPermissions;

  // Computed properties for conditional display
  showAdditionalInfo = computed(() => {
    const data = this.recordData();
    return data.workflowStageName || data.responsibleOrgUnitName || 
           data.partnershipAgreementReference || data.initiativeBudgetUSD ||
           data.targetSigningDate || data.targetDeliveryDate || 
           data.proposedInitiativeTypeName;
  });

  showFullContent = signal<boolean>(false);

  shouldShowSeeMoreButton = computed(() => {
    return this.showAdditionalInfo() && !this.showFullContent();
  });

  shouldShowSeeLessButton = computed(() => {
    return this.showAdditionalInfo() && this.showFullContent();
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
  _loadRecordDetails() {
    this.infoLoading.set(true);
    this.opportunityService.getOpportunityById(+this.recordId).subscribe({
      next: (data: Opportunity) => {
        this.recordData.set(data);
        this.infoLoading.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading opportunity details:', error);
        this.infoLoading.set(false);
        this.feedbackDialogService.showErrorToast({
          detail: 'Failed to load opportunity details',
          summary: 'Error'
        });
      }
    });
  }

  handleEditClick() {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit this opportunity',
        summary: 'Permission Denied'
      });
      return;
    }

    // TODO: Implement edit dialog
    this.feedbackDialogService.showInfoToast({
      detail: 'Edit functionality coming soon',
      summary: 'Info'
    });
  }

  deleteOpportunity(): void {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to delete this opportunity',
        summary: 'Permission Denied'
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.translateService.instant('message.confirmation.deleteOpportunity'),
      header: this.translateService.instant('title.deleteOpportunity'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        if (this.recordData().id) {
          this.opportunityService.deleteOpportunityById(this.recordData().id!).subscribe({
            next: () => {
              this.feedbackDialogService.showSuccessToast({
                detail: this.translateService.instant('message.opportunity.deletedSuccessfully')
              });
              this.router.navigate(['/partnerships/opportunities']);
            },
            error: (error) => {
              console.error('Error deleting opportunity:', error);
              this.feedbackDialogService.showErrorToast({
                detail: this.translateService.instant('message.opportunity.deleteFailed')
              });
            }
          });
        }
      }
    });
  }

  toggleFullContent() {
    this.showFullContent.update(value => !value);
  }

  formatCurrency(value: number | undefined | null): string {
    if (value === undefined || value === null) return '';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  }

  formatDate(date: Date | undefined | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}

