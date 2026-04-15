/**
 * @fileoverview Opportunity Alt Component - Clean-room rebuild of the opportunity view
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  computed,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

import { DocumentService } from '@shared/services/api/document.service';
import { FeedbackDialogService } from '@shared/services/ui';

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
    FormsModule,
    RouterModule,
    TranslateModule,
    AvatarModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    ChipModule,
    DividerModule,
    MessageModule,
    ProgressSpinnerModule,
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
  private readonly documentService = inject(DocumentService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translate = inject(TranslateService);

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  readonly loading = signal(false);
  readonly opportunity = signal<Opportunity | null>(null);
  readonly recordId = signal<number | null>(null);

  readonly sidePaneExpanded = signal(false);
  readonly activePaneSection = signal<'notifications' | 'documents' | 'ai'>('notifications');
  readonly canUpdate = signal(true);

  docPanelOpen = true;
  aiPanelOpen = false;

  // --- Document signals ---
  readonly documents = signal<any[]>([]);
  readonly documentsLoading = signal(false);
  readonly documentSearch = signal('');
  readonly selectedDocIds = signal<Set<number>>(new Set());

  readonly filteredDocuments = computed(() => {
    const query = this.documentSearch().toLowerCase().trim();
    const docs = this.documents();
    if (!query) return docs;
    return docs.filter((d: any) => (d.name || '').toLowerCase().includes(query));
  });

  // --- Header signals ---
  readonly opportunityName = computed(() => this.opportunity()?.name ?? '');
  readonly stageName = computed(() => this.opportunity()?.workflowStatus ?? '');
  readonly stageNumber = computed(() => {
    const stage = this.stageName();
    const match = stage.match(/^(\d+)\s*-/);
    return match ? match[1] : null;
  });
  readonly stageLabel = computed(() => {
    const stage = this.stageName();
    const idx = stage.indexOf('-');
    return idx >= 0 ? stage.substring(idx + 1).trim() : stage;
  });
  readonly stageTotalSteps = computed(() => 2);
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
  fundingPartnersCardConfig!: DashboardCardConfig;
  deliverablesCardConfig!: DashboardCardConfig;
  keyDatesCardConfig!: DashboardCardConfig;
  countriesAndSdgsCardConfig!: DashboardCardConfig;

  private initCardConfigs(): void {
    this.fundingPartnersCardConfig = {
      icon: 'account_balance',
      iconColor: 'bg-unops-primary/10',
      title: this.translate.instant('label.fundingPartners'),
      subtitle: this.translate.instant('label.alt.cardSubtitle.fundingPartners'),
      size: 'auto',
      showViewAll: false,
    };

    this.deliverablesCardConfig = {
      icon: 'inventory_2',
      iconColor: 'bg-unops-accent-orange/10',
      title: this.translate.instant('label.deliverables'),
      subtitle: this.translate.instant('label.alt.cardSubtitle.deliverables'),
      size: 'auto',
      showViewAll: false,
    };

    this.keyDatesCardConfig = {
      icon: 'calendar_month',
      iconColor: 'bg-unops-info/10',
      title: this.translate.instant('label.alt.keyDates'),
      subtitle: this.translate.instant('label.alt.cardSubtitle.keyDates'),
      size: 'auto',
      showViewAll: false,
    };

    this.countriesAndSdgsCardConfig = {
      icon: 'public',
      iconColor: 'bg-unops-success/10',
      title: this.translate.instant('label.alt.countriesAndSdgs'),
      subtitle: this.translate.instant('label.alt.cardSubtitle.countriesAndSdgs'),
      size: 'auto',
      showViewAll: false,
    };
  }

  private subscriptions = new Subscription();

  ngOnInit(): void {
    this.initCardConfigs();
    this.subscriptions.add(
      this.route.params.subscribe(params => {
        const id = params['recordId'];
        if (id) {
          const numericId = Number(id);
          this.recordId.set(numericId);
          this.loadOpportunity(numericId);
          this.loadDocuments();
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

  expandSidePane(section: 'notifications' | 'documents' | 'ai'): void {
    this.activePaneSection.set(section);
    this.sidePaneExpanded.set(true);
  }

  collapseSidePane(): void {
    this.sidePaneExpanded.set(false);
  }

  // --- Document methods ---

  private loadDocuments(): void {
    const id = this.recordId();
    if (!id) return;
    this.documentsLoading.set(true);
    this.subscriptions.add(
      this.documentService.getDocumentsByEntity('Opportunity', id).subscribe({
        next: (docs: any) => {
          this.documents.set(Array.isArray(docs) ? docs : []);
          this.documentsLoading.set(false);
        },
        error: () => {
          this.documentsLoading.set(false);
        },
      })
    );
  }

  isDocSelected(docId: number): boolean {
    return this.selectedDocIds().has(docId);
  }

  toggleDocSelection(docId: number): void {
    const current = new Set(this.selectedDocIds());
    if (current.has(docId)) {
      current.delete(docId);
    } else {
      current.add(docId);
    }
    this.selectedDocIds.set(current);
  }

  downloadDocument(doc: any): void {
    this.subscriptions.add(
      this.documentService.downloadDocument(doc.id).subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = doc.name || 'document';
          a.click();
          window.URL.revokeObjectURL(url);
        },
      })
    );
  }

  deleteDocument(doc: any): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translate.instant('button.delete'),
        detail: this.translate.instant('message.confirmDeleteDocument'),
      },
      () => {
        this.subscriptions.add(
          this.documentService.deleteDocument(doc.id).subscribe({
            next: () => {
              this.loadDocuments();
              this.feedbackService.showSuccessToast({
                summary: this.translate.instant('message.success'),
                detail: this.translate.instant('message.documentDeleted'),
              });
            },
          })
        );
      }
    );
  }

  triggerFileUpload(): void {
    this.fileInput?.nativeElement?.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.recordId()) return;

    this.subscriptions.add(
      this.documentService.uploadDocument(file, 'Opportunity', this.recordId()!).subscribe({
        next: () => {
          this.loadDocuments();
          this.feedbackService.showSuccessToast({
            summary: this.translate.instant('message.success'),
            detail: this.translate.instant('message.documentUploaded'),
          });
        },
      })
    );
    input.value = '';
  }

  navigateToOriginal(): void {
    const id = this.recordId();
    if (id) {
      this.router.navigate(['/partnerships/opportunities', id]);
    } else {
      this.router.navigate(['/partnerships/opportunities']);
    }
  }

  onPartnerImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/Partner.png';
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
