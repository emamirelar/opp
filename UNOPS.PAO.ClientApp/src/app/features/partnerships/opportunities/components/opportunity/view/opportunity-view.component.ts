/**
 * @fileoverview Opportunity View Component - Unified Dashboard View
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnDestroy,
  OnInit,
  signal,
  computed,
  ViewChild,
  ElementRef,
  AfterViewInit,
  effect,
  untracked,
  HostListener,
} from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule, ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

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
import { FileUploadModule } from 'primeng/fileupload';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { SelectModule } from 'primeng/select';
import { MarkdownModule } from 'ngx-markdown';

// Workflow components
import { StageWorkflowComponent } from '@shared/reusables/components/workflow/components/stage-workflow/stage-workflow.component';
import {
  RequirementsValidationComponent,
  RequirementClickEvent,
} from '@shared/reusables/components/workflow/components/requirements-validation/requirements-validation.component';
import { WorkflowService } from '@shared/reusables/components/workflow/services/workflow.service';
import { WorkflowHistoryModel } from '@shared/reusables/components/workflow/models/workflow.models';

// Services
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService, AuthService } from '@core/services/auth';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';
import { PageContextService } from '@shared/services/utils';
import { OpportunityService } from '../../../services/opportunity.service';
import { Opportunity, GoDecisionPayload, NoGoDecisionPayload, Risk } from '@shared/models/opportunity.model';
import { CustomStageChangeResult } from '@shared/reusables/components/workflow/models/workflow.models';
import {
  LoadingProgress,
  LoadingSectionKey,
  LoadingSectionStatus,
  DEFAULT_LOADING_PROGRESS,
} from '@shared/models/loading-progress.interface';
import { OpportunityCollaborationComponent } from './sections/collaboration/opportunity-collaboration.component';
import { OpportunityAnalysisSectionComponent } from './sections/analysis/opportunity-analysis-section.component';
import { OpportunityOverviewSectionComponent } from './sections/overview/opportunity-overview-section.component';
import { OpportunityWhatSectionComponent } from './sections/what/opportunity-what-section.component';
import { OpportunityWhySectionComponent } from './sections/why/opportunity-why-section.component';
import { OpportunityWhoSectionComponent } from './sections/who/opportunity-who-section.component';
import { OpportunityWhereSectionComponent } from './sections/where/opportunity-where-section.component';
import { OpportunityWhenSectionComponent } from './sections/when/opportunity-when-section.component';
import { OpportunityDstSectionComponent } from './sections/dst/opportunity-dst-section.component';
import { OpportunityTeamSectionComponent } from './sections/team/opportunity-team-section.component';
import { OpportunityRelatedItemsComponent } from './sections/related/opportunity-related-items.component';
import { OpportunityDocumentsComponent } from './sections/document/opportunity-documents.component';
import { OpportunityStatementSectionComponent } from './sections/statement/opportunity-statement-section.component';
import { ValuesService } from '@app/shared/services/api/values.service';

// Go/No-Go Decision Components
import { ApproveOpportunityDialogComponent } from '../approve-opportunity-dialog/approve-opportunity-dialog.component';
import { RejectOpportunityDialogComponent } from '../reject-opportunity-dialog/reject-opportunity-dialog.component';
import { OpportunityDecisionInfoPanelComponent } from '../opportunity-decision-info-panel/opportunity-decision-info-panel.component';

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
    FormsModule,
    ReactiveFormsModule,
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
    FileUploadModule,
    TooltipModule,
    DropdownModule,
    SelectModule,
    MarkdownModule,
    StageWorkflowComponent,
    RequirementsValidationComponent,
    OpportunityCollaborationComponent,
    OpportunityAnalysisSectionComponent,
    OpportunityOverviewSectionComponent,
    OpportunityWhatSectionComponent,
    OpportunityWhySectionComponent,
    OpportunityWhoSectionComponent,
    OpportunityWhereSectionComponent,
    OpportunityWhenSectionComponent,
    OpportunityDstSectionComponent,
    OpportunityTeamSectionComponent,
    OpportunityRelatedItemsComponent,
    OpportunityDocumentsComponent,
    OpportunityStatementSectionComponent,
    ApproveOpportunityDialogComponent,
    RejectOpportunityDialogComponent,
    OpportunityDecisionInfoPanelComponent,
  ],
  templateUrl: './opportunity-view.component.html',
  styleUrls: ['./opportunity-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService],
})
export class OpportunityViewComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  router = inject(Router);
  private location = inject(Location);
  activatedRoute = inject(ActivatedRoute);
  opportunityService = inject(OpportunityService);
  valuesService = inject(ValuesService);
  permissionUtilityService = inject(PermissionUtilityService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  feedbackDialogService = inject(FeedbackDialogService);
  confirmationService = inject(ConfirmationService);
  private pageContextService = inject(PageContextService);
  private authService = inject(AuthService);
  private googleOAuthService = inject(GoogleOAuthService);
  private workflowService = inject(WorkflowService);

  // State
  loading = signal<boolean>(true);
  isRegeneratingBanner = signal<boolean>(false);
  recordId: string = '';
  opportunity = signal<Opportunity | null>(null);
  baseEngagementNumber = signal<string | null>(null);
  oupBaseUrl = signal<string>('');

  // Loading Progress State
  readonly loadingProgress = signal<LoadingProgress>(DEFAULT_LOADING_PROGRESS);
  
  // Computed progress percentage (0-100)
  readonly progressPercentage = computed(() => {
    const progress = this.loadingProgress();
    if (progress.total === 0) return 0;
    return Math.round((progress.completed / progress.total) * 100);
  });

  // Computed progress message for display
  readonly progressMessage = computed(() => {
    const progress = this.loadingProgress();
    if (progress.completed === progress.total) {
      return this.translateService.instant('message.allDataLoaded');
    }
    const currentLabel = progress.currentSection
      ? this.translateService.instant(progress.currentSection)
      : '';
    return this.translateService.instant('message.loadingProgress', {
      current: progress.completed,
      total: progress.total,
      section: currentLabel,
    });
  });

  // Show progress bar only while loading
  readonly showProgressBar = computed(() => {
    const progress = this.loadingProgress();
    return progress.completed < progress.total;
  });

  // Track if all loading is complete
  readonly allLoadingComplete = computed(() => {
    const progress = this.loadingProgress();
    return progress.completed === progress.total;
  });
  showAIPanel = signal<boolean>(true); // AI Assistant panel toggle state
  documentsCollapsed = signal(true); // Document panel state
  activeSection = signal<string>(''); // Active section for navigation - will be set from route params
  headerScrolled = signal<boolean>(false); // Header shrunk state when scrolled
  innerWidth = signal<number>(window.innerWidth); // Track window width for responsive behavior
  
  // Unsaved changes tracking for sticky save bar (Option 2 UX)
  sectionsWithUnsavedChanges = signal<Set<string>>(new Set());
  readonly hasUnsavedChanges = computed(() => this.sectionsWithUnsavedChanges().size > 0);
  
  // Document upload trigger - incremented when documents are uploaded to notify WHAT section to refresh AI recommendations
  documentUploadTrigger = signal<number>(0);
  
  // Section save trigger - incremented when any section saves to notify WHAT section to refresh framework status
  sectionSaveTrigger = signal<number>(0);

  // FormGroup for requirements validation - mirrors opportunity fields
  // Used by app-requirements-validation to validate workflow stage transition requirements
  opportunityForm = new FormGroup({
    name: new FormControl(''),
    description: new FormControl(''),
    challenges: new FormControl(''),
    expectedImpact: new FormControl(''),
    expectedOutcomes: new FormControl(''),
    opportunityStatementMarkdown: new FormControl(''),
    initiativeBudgetUSD: new FormControl<number | null>(null),
    unopsMissions: new FormControl<unknown[]>([]),
    unopsMissionsNotApplicable: new FormControl<boolean>(false),
    sdgs: new FormControl<unknown[]>([]),
    fundingPartners: new FormControl<unknown[]>([]),
    clientPartners: new FormControl<unknown[]>([]),
    deliverables: new FormControl<unknown[]>([]),
    countries: new FormControl<unknown[]>([]),
    targetSigningDate: new FormControl<Date | null>(null),
    implementationStartDate: new FormControl<Date | null>(null),
    targetDeliveryDate: new FormControl<Date | null>(null),
    responsibleOrgUnitId: new FormControl<number | null>(null),
    proposedInitiativeTypeId: new FormControl<number | null>(null),
    beneficiariesToBeDetermined: new FormControl<boolean>(false),
    estimatedDirectBeneficiaries: new FormControl<number | null>(null),
    estimatedIndirectBeneficiaries: new FormControl<number | null>(null),
    stakeholders: new FormControl<unknown[]>([]),
  });

  @ViewChild('contentScrollContainer', { read: ElementRef })
  contentScrollContainer?: ElementRef;
  @ViewChild('chipsContainer', { read: ElementRef })
  chipsContainer?: ElementRef;
  @ViewChild('chipsSizerDiv', { read: ElementRef })
  chipsSizerDiv?: ElementRef;
  @ViewChild('stageWorkflowComponent')
  stageWorkflowComponent?: StageWorkflowComponent;
  @ViewChild('relatedItemsComponent')
  relatedItemsComponent?: OpportunityRelatedItemsComponent;
  @ViewChild('risksSection')
  dstSectionComponent?: OpportunityDstSectionComponent;
  @ViewChild(OpportunityAnalysisSectionComponent)
  analysisSectionComponent?: OpportunityAnalysisSectionComponent;
  @ViewChild(OpportunityWhySectionComponent)
  whySectionComponent?: OpportunityWhySectionComponent;
  @ViewChild(OpportunityDocumentsComponent)
  documentsComponent?: OpportunityDocumentsComponent;
  @ViewChild(OpportunityOverviewSectionComponent)
  overviewSectionComponent?: OpportunityOverviewSectionComponent;
  @ViewChild(OpportunityWhatSectionComponent)
  whatSectionComponent?: OpportunityWhatSectionComponent;
  @ViewChild(OpportunityWhoSectionComponent)
  whoSectionComponent?: OpportunityWhoSectionComponent;
  @ViewChild(OpportunityTeamSectionComponent)
  teamSectionComponent?: OpportunityTeamSectionComponent;
  @ViewChild(OpportunityWhereSectionComponent)
  whereSectionComponent?: OpportunityWhereSectionComponent;
  @ViewChild(OpportunityWhenSectionComponent)
  whenSectionComponent?: OpportunityWhenSectionComponent;

  private intersectionObserver?: IntersectionObserver;
  private navigationInProgress = false;
  private isScrolling = false; // Flag to prevent URL updates during programmatic scrolling
  private scrollTimeout?: number; // Debounce timeout for scroll spy
  private lastManualNavigationTime: number = 0; // Track last manual navigation
  private isInitialLoad = true; // Track if this is the initial page load
  private pendingScrollTarget: string | null = null; // Store pending scroll target
  private scrollCheckInterval?: number; // Interval to check if content is loaded
  private shouldScrollAfterDataLoad = false; // Flag to allow scrolling after data loads (only set on initial load with section in URL)
  private resizeObserver?: ResizeObserver; // Observer for chip container resizing

  // Section navigation configuration
  sections = [
    { id: 'analysis', label: 'Analysis', icon: 'pi-chart-bar' },
    { id: 'overview', label: 'label.opportunity.overview', icon: 'pi-file' },
    { id: 'what', label: 'What', icon: 'pi-briefcase' },
    { id: 'why', label: 'Why', icon: 'pi-lightbulb' },
    { id: 'who', label: 'Who', icon: 'pi-users' },
    { id: 'where', label: 'Where', icon: 'pi-globe' },
    { id: 'when', label: 'When', icon: 'pi-calendar' },
    { id: 'risks', label: 'Risks', icon: 'pi-chart-line' },
    { id: 'related', label: 'Related', icon: 'pi-link' },
    { id: 'collaboration', label: 'Comments', icon: 'pi-comments' },
    { id: 'statement', label: 'Statement', icon: 'pi-file-edit' },
    { id: 'team', label: 'label.opportunity.team', icon: 'pi-building' },
  ];

  // Chip overflow management
  visibleChips = signal<typeof this.sections>([]);
  overflowChips = signal<typeof this.sections>([]);
  readonly hasOverflowChips = computed(() => this.overflowChips().length > 0);

  // Permission management using utility service
  private permissionUtils =
    this.permissionUtilityService.createInstancePermissions('Opportunity');
  recordPermissions = this.permissionUtils.recordPermissions;

  // Computed canUpdate based on opportunity permissions (from backend including stakeholder check)
  canUpdate = computed(() => {
    const opp = this.opportunity();
    
    // If opportunity has inline permissions, use them as the source of truth
    // The backend sets canUpdate=false when the opportunity is immutable (GO, NO GO, CANCELLED stages)
    if (opp?.permissions) {
      // Explicitly check if canUpdate is false (immutable or no permission)
      if (opp.permissions.canUpdate === false) {
        return false;
      }
      // If canUpdate is explicitly true, allow editing
      if (opp.permissions.canUpdate === true) {
        return true;
      }
    }
    
    // Fallback to recordPermissions from utility service only if inline permissions are not available
    return this.permissionUtilityService.canUpdate(this.recordPermissions());
  });

  // Computed permission for changing workflow stage
  // Note: Workflow actions (Recall, Approve, Reject) should be available even when canUpdate is false
  // due to approval pending status. The workflow component will verify specific permissions (canRecall, canApprove)
  canChangeStage = computed(() => {
    const opp = this.opportunity();
    if (!opp?.id) return false;
    
    // If in immutable stage (GO, NO GO, CANCELLED), no workflow actions allowed
    if (opp.permissions?.isImmutable) return false;
    
    // If in approval pending status, allow workflow actions (the workflow component
    // will check specific permissions like canRecall, canApprove from backend)
    if (opp.permissions?.isApprovalPending || opp.isInWorkflow) return true;
    
    // Otherwise, check update permission (for initiating workflow submissions)
    return this.canUpdate();
  });

  // ===== Go/No-Go Decision State =====

  /**
   * @description Whether the entity is in an immutable state (after Go/No-Go decision)
   */
  isImmutable = computed(() => {
    const opp = this.opportunity();
    return opp?.permissions?.isImmutable ?? false;
  });

  /**
   * @description Whether the entity is currently in an approval workflow (Approval Pending status)
   */
  isApprovalPending = computed(() => {
    const opp = this.opportunity();
    return opp?.permissions?.isApprovalPending ?? false;
  });

  /**
   * @description Whether to show the instructional guidance for decision makers
   * Shows when user is an approver and opportunity is in pending approval workflow stage
   */
  showDecisionGuidance = computed(() => {
    const opp = this.opportunity();
    if (!opp) return false;
    
    // Check if opportunity is in workflow and pending approval (SEND FOR GO DECISION stage)
    const stage = opp.stage?.toUpperCase() || '';
    const isPendingGoDecision = stage.includes('SEND FOR GO DECISION') || stage.includes('PENDING');
    const isInWorkflow = opp.isInWorkflow;
    
    // Check if current user can take workflow actions (is an approver)
    const canApprove = this.canChangeStage();
    
    return isPendingGoDecision && isInWorkflow && canApprove;
  });

  /**
   * @description Whether to show the decision info panel
   * Same conditions as showDecisionGuidance
   */
  showDecisionInfoPanel = computed(() => this.showDecisionGuidance());

  /**
   * @description Instructional guidance text for decision makers
   */
  readonly instructionalGuidanceText = 'workflow.goDecision.guidance.message';

  // Dialog visibility signals
  showApproveDialog = signal<boolean>(false);
  showRejectDialog = signal<boolean>(false);

  // Workflow action loading overlay state
  // When true, shows a full-page blur overlay while workflow actions are in progress
  workflowActionInProgress = signal<boolean>(false);

  // Risks loaded from DST section (for decision info panel)
  opportunityRisks = signal<Risk[]>([]);

  // Workflow submission comment (sender remarks)
  workflowSubmissionComment = signal<string | null>(null);

  // Promise resolvers for custom stage change handler
  private approveDialogResolver: ((result: CustomStageChangeResult) => void) | null = null;
  private rejectDialogResolver: ((result: CustomStageChangeResult) => void) | null = null;

  // Computed properties for conditional display
  showAdditionalInfo = computed(() => {
    const data = this.opportunity();
    if (!data) return false;
    return (
      data.stage ||
      data.responsibleOrgUnitName ||
      data.partnershipAgreementReference ||
      data.initiativeBudgetUSD ||
      data.targetSigningDate ||
      data.targetDeliveryDate ||
      data.proposedInitiativeTypeName
    );
  });

  // Signal to store the current user's email (loaded from claims)
  private currentUserEmail = signal<string>('');

  // Get opportunity manager from stakeholders (internal stakeholder with "Opportunity Manager" role)
  opportunityManager = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders || opp.stakeholders.length === 0) return null;

    // Find the first internal stakeholder with "Opportunity Manager" role
    const manager = opp.stakeholders.find(
      (s) =>
        s.isInternal &&
        s.entityRoleName &&
        s.entityRoleName.toLowerCase().includes('opportunity') &&
        s.entityRoleName.toLowerCase().includes('manager'),
    );

    return manager ? manager.userName || manager.userEmail || '-' : '-';
  });

  // Get the opportunity manager stakeholder (for OM email comparison)
  private opportunityManagerStakeholder = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders || opp.stakeholders.length === 0) return null;

    return opp.stakeholders.find(
      (s) =>
        s.isInternal &&
        s.entityRoleName &&
        s.entityRoleName.toLowerCase().includes('opportunity') &&
        s.entityRoleName.toLowerCase().includes('manager'),
    ) || null;
  });

  // Determine if the current user is the Opportunity Manager
  isCurrentUserOpportunityManager = computed(() => {
    const email = this.currentUserEmail();
    const omStakeholder = this.opportunityManagerStakeholder();
    
    if (!email || !omStakeholder?.userEmail) return false;
    
    // Compare emails (case-insensitive)
    return omStakeholder.userEmail.toLowerCase() === email.toLowerCase();
  });

  // Check if target signing date is overdue (in the past) and opportunity is still in Identify & Profile or Decide stage
  isTargetSigningDateOverdue = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.targetSigningDate) return false;

    const targetDate = new Date(opp.targetSigningDate);
    const now = new Date();
    
    // Check if date is in the past
    if (targetDate >= now) return false;

    // Check if opportunity is in Identify & Profile or Decide stage
    const stageName = opp.stage?.toLowerCase() || '';
    const isInEarlyStage = 
      stageName.includes('identify') || 
      stageName.includes('profile') || 
      stageName.includes('decide');

    return isInEarlyStage;
  });

  showFullContent = signal<boolean>(false);

  shouldShowSeeMoreButton = computed(() => {
    return this.showAdditionalInfo() && !this.showFullContent();
  });

  shouldShowSeeLessButton = computed(() => {
    return this.showAdditionalInfo() && this.showFullContent();
  });

  // Computed OUP engagement URL
  oupEngagementUrl = computed(() => {
    const baseUrl = this.oupBaseUrl();
    const engagementNumber = this.baseEngagementNumber();
    if (baseUrl && engagementNumber) {
      return `${baseUrl}/${engagementNumber}/engagement/overview`;
    }
    return null;
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
    return (
      opp.stats?.internalStakeholderCount ||
      opp.stakeholders?.filter((s) => s.isInternal).length ||
      0
    );
  });

  externalStakeholderCount = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return (
      opp.stats?.externalStakeholderCount ||
      opp.stakeholders?.filter((s) => !s.isInternal).length ||
      0
    );
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

  // AI Insights and Suggestions state and filtering
  allInsights = signal<any[]>([]);
  allSuggestions = signal<any[]>([]);
  insightsLoading = signal<boolean>(false);
  insightsError = signal<string | null>(null);

  // Computed suggestions filtered by section
  whoSuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'WHO'),
  );

  whereSuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'WHERE'),
  );

  whatSuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'WHAT'),
  );

  whySuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'WHY'),
  );

  whenSuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'WHEN'),
  );

  teamSuggestions = computed(() =>
    this.allSuggestions().filter((s) => s.actionTarget === 'TEAM'),
  );

  // Filtered stakeholder lists
  internalStakeholders = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders) return [];
    return opp.stakeholders.filter((s) => s.isInternal);
  });

  externalStakeholders = computed(() => {
    const opp = this.opportunity();
    if (!opp || !opp.stakeholders) return [];
    return opp.stakeholders.filter((s) => !s.isInternal);
  });

  primarySDG = computed(() => {
    const opp = this.opportunity();
    if (!opp) return null;
    return opp.sdGs?.find((s) => s.isPrimary) || null;
  });

  documentsChevronIcon = computed(() => {
    return this.documentsCollapsed() ? 'pi-chevron-right' : 'pi-chevron-left';
  });

  constructor() {
    // Effect to setup scroll spy once data is loaded
    effect(() => {
      const isLoaded = !this.loading();

      if (isLoaded) {
        untracked(() => {
          setTimeout(() => this.setupScrollSpy(), 300);
        });
      }
    });

    // Effect to setup scroll listener for header shrinking once data is loaded
    effect(() => {
      const opp = this.opportunity();
      const isLoading = this.loading();

      // Wait for data to load and element to be available
      if (opp && !isLoading && !this.scrollListenerAttached) {
        untracked(() => {
          // Small delay to ensure DOM is updated
          setTimeout(() => {
            this.setupScrollListener();
          }, 100);
        });
      }
    });

    // Effect to recalculate chip overflow when data loads
    effect(() => {
      const isLoaded = !this.loading();
      const opp = this.opportunity();

      if (isLoaded && opp) {
        untracked(() => {
          // Recalculate chip overflow after data loads and DOM updates
          setTimeout(() => {
            this.calculateChipOverflow();
          }, 200);
        });
      }
    });

    // Initially show all chips (will be recalculated after view init)
    this.visibleChips.set(this.sections);

    // Effect to reload insights when any section saves
    effect(() => {
      const trigger = this.sectionSaveTrigger();
      
      // Only reload if trigger has changed (skip initial value of 0)
      if (trigger > 0) {
        console.log('🔄 Parent: Section save detected, reloading insights');
        
        // Delay to prevent overwhelming the backend
        setTimeout(() => {
          this._loadInsights();
        }, 3000);
      }
    });

    // Effect to show completion notification and auto-hide progress bar
    effect(() => {
      const isComplete = this.allLoadingComplete();
      
      if (isComplete) {
        console.log('✅ All sections loaded successfully');
        // Progress bar will auto-hide after staying complete for 2 seconds
        setTimeout(() => {
          // The progress bar automatically hides due to showProgressBar computed property
        }, 2000);
      }
    });

    // Effect to watch DST section loading states and update progress
    effect(() => {
      const dstComponent = this.dstSectionComponent;
      if (!dstComponent) return;

      // Watch risks loading
      const risksLoading = dstComponent.loadingRisks();
      if (!risksLoading && this.loadingProgress().sections.dstRisks.status === 'loading') {
        untracked(() => this.onDSTRisksLoaded());
      }

      // Watch recommendations loading
      const recsLoading = dstComponent.loadingRecommendations();
      if (!recsLoading && this.loadingProgress().sections.dstRecommendations.status === 'loading') {
        untracked(() => this.onDSTRecommendationsLoaded());
      }

      // Watch similar opportunities loading
      const simOpsLoading = dstComponent.loadingSimilarOpportunities();
      if (!simOpsLoading && this.loadingProgress().sections.dstSimilarOpportunities.status === 'loading') {
        untracked(() => this.onDSTSimilarOpportunitiesLoaded());
      }

      // Watch similar projects loading
      const simProjsLoading = dstComponent.loadingSimilarProjects();
      if (!simProjsLoading && this.loadingProgress().sections.dstSimilarProjects.status === 'loading') {
        untracked(() => this.onDSTSimilarProjectsLoaded());
      }

      // Watch relevant people loading
      const peopleLoading = dstComponent.loadingRelevantPeople();
      if (!peopleLoading && this.loadingProgress().sections.dstRelevantPeople.status === 'loading') {
        untracked(() => this.onDSTRelevantPeopleLoaded());
      }
    });

    // Effect to watch related items component loading
    effect(() => {
      const relatedComponent = this.relatedItemsComponent;
      if (!relatedComponent) return;

      const isLoading = relatedComponent.isLoading();
      if (!isLoading && this.loadingProgress().sections.relatedItems.status === 'loading') {
        untracked(() => this.onRelatedItemsLoaded());
      }
    });

    // Effect to watch documents component loading
    effect(() => {
      const docsComponent = this.documentsComponent;
      if (!docsComponent) return;

      const isLoading = docsComponent.loading();
      if (!isLoading && this.loadingProgress().sections.documents.status === 'loading') {
        untracked(() => this.onDocumentsLoaded());
      }
    });

    // Effect to sync opportunityForm when opportunity data changes
    // This enables requirements validation to work with the current data
    effect(() => {
      const opp = this.opportunity();
      if (opp) {
        untracked(() => {
          this.syncOpportunityFormValues(opp);
        });
      }
    });
  }

  ngOnInit() {
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);

    // Load current user email from claims for OM comparison
    this.authService.user().subscribe({
      next: (claims) => {
        const emailClaim = claims.find(c => 
          c.type === 'email' || 
          c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
        );
        if (emailClaim?.value) {
          this.currentUserEmail.set(emailClaim.value);
        }
      }
    });

    // Fetch OUP base URL from configuration
    this.valuesService.getConfig().subscribe({
      next: (config: any) => {
        if (config?.oupSettings?.baseUrl) {
          this.oupBaseUrl.set(config.oupSettings.baseUrl);
        }
      },
      error: (error) => {
        console.warn('Failed to load OUP settings:', error);
      }
    });

    // Subscribe to route parameter changes for both recordId and section
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        const newRecordId = paramMap.get('recordId') || '';
        const section = paramMap.get('section');

        // Check for initial load FIRST (when recordId is empty or undefined)
        if (newRecordId && (!this.recordId || this.recordId === '')) {
          // Initial load - set section and allow scrolling to specified section after data loads
          this.recordId = newRecordId;
          this.shouldScrollAfterDataLoad = section ? true : false;
          
          if (section && this.isValidSection(section)) {
            this.activeSection.set(section);
            this.lastManualNavigationTime = Date.now();
            this.isScrolling = true;
          } else if (!section) {
            this.activeSection.set('analysis');
          }
          
          // Load permissions for this specific opportunity instance
          this.permissionUtils.loadPermissions(this.recordId, this.cdr);
          this._loadRecordDetails(section || 'analysis');
        } else if (newRecordId && newRecordId !== this.recordId) {
          // Record ID changed (navigating to different record) - don't auto-scroll
          this.recordId = newRecordId;
          this.shouldScrollAfterDataLoad = false;
          
          if (section && this.isValidSection(section)) {
            this.activeSection.set(section);
          } else if (!section) {
            this.activeSection.set('analysis');
          }
          
          // Load permissions for the new opportunity instance
          this.permissionUtils.loadPermissions(this.recordId, this.cdr);
          this._loadRecordDetails(section || 'analysis');
        }
        // IMPORTANT: If recordId hasn't changed, this is just a section change from scroll spy
        // DO NOT update activeSection here - let scroll spy handle it to avoid double updates

        // NOTE: Section scrolling is handled in two places ONLY:
        // 1. Initial load with section in URL: _loadRecordDetails() with shouldScrollAfterDataLoad=true
        // 2. User clicking section chips: scrollToSection() method
        // 3. User scrolling: scroll spy updates URL and activeSection - paramMap should NOT interfere
      },
    });
  }

  private scrollListenerAttached = false;

  ngAfterViewInit(): void {
    // Observers will be set up via effect after data loads
    
    // Setup resize observer for chip overflow management
    // Wait a bit to ensure the sizer div is rendered
    setTimeout(() => {
      this.setupChipOverflowObserver();
    }, 50);
  }

  /**
   * Setup scroll listener to shrink header on scroll
   */
  private setupScrollListener(): void {
    const scrollElement = this.contentScrollContainer?.nativeElement;

    if (scrollElement) {
      scrollElement.addEventListener('scroll', () => {
        const scrollTop = scrollElement.scrollTop;
        // Shrink header after scrolling past the banner (240px banner height, hide after scrolling ~100px)
        const newHeaderScrolled = scrollTop > 100;
        if (this.headerScrolled() !== newHeaderScrolled) {
          this.headerScrolled.set(newHeaderScrolled);
          // Manually trigger change detection to ensure UI updates immediately
          this.cdr.detectChanges();
        }
      });

      this.scrollListenerAttached = true;
      console.log('✅ Scroll listener added successfully');
    } else {
      console.error(
        '❌ contentScrollContainer not found! Cannot attach scroll listener',
      );
    }
  }

  /**
   * Syncs the opportunityForm values from the opportunity data.
   * This allows the requirements-validation component to validate against current data.
   */
  private syncOpportunityFormValues(opp: Opportunity): void {
    this.opportunityForm.patchValue({
      name: opp.name || '',
      description: opp.description || '',
      challenges: opp.challenges || '',
      expectedImpact: opp.expectedImpact || '',
      expectedOutcomes: opp.expectedOutcomes || '',
      opportunityStatementMarkdown: opp.opportunityStatementMarkdown || '',
      initiativeBudgetUSD: opp.initiativeBudgetUSD ?? null,
      unopsMissions: opp.unopsMissions || [],
      unopsMissionsNotApplicable: opp.unopsMissionsNotApplicable || false,
      sdgs: opp.sdGs || [],
      fundingPartners: opp.fundingPartners || [],
      clientPartners: opp.clientPartners || [],
      deliverables: opp.deliverables || [],
      countries: opp.countries || [],
      targetSigningDate: opp.targetSigningDate ? new Date(opp.targetSigningDate) : null,
      implementationStartDate: opp.implementationStartDate ? new Date(opp.implementationStartDate) : null,
      targetDeliveryDate: opp.targetDeliveryDate ? new Date(opp.targetDeliveryDate) : null,
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? null,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? null,
      beneficiariesToBeDetermined: opp.beneficiariesToBeDetermined || false,
      estimatedDirectBeneficiaries: opp.estimatedDirectBeneficiaries ?? null,
      estimatedIndirectBeneficiaries: opp.estimatedIndirectBeneficiaries ?? null,
      stakeholders: opp.stakeholders || [],
    }, { emitEvent: true });
  }

  ngOnDestroy(): void {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();

    // Cleanup scroll spy observer
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
    // Cleanup resize observer
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
    // Cleanup scroll check interval
    if (this.scrollCheckInterval) {
      clearInterval(this.scrollCheckInterval);
    }
    // Cleanup scroll timeout
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }
  }

  /**
   * Load opportunity record details
   * 
   * NOTE: This component coordinates multiple child sections that make AI-powered API calls:
   * - Analysis Section: AI insights (delayed 2.5s) - handled by child component
   * - DST Section: Risks (immediate), Recommendations (0.5s), Similar Opportunities (1s), 
   *   Similar Projects (1.5s), Relevant People (2s)
   * 
   * The staggered loading prevents connection exhaustion and ensures the notifications
   * polling endpoint continues to work properly.
   */
  private _loadRecordDetails(targetSection?: string) {
    this.loading.set(true);

    // Reset progress to initial state
    this.resetLoadingProgress();

    // STEP 1: Load main opportunity data
    this.updateLoadingProgress('opportunity', 'loading');

    this.opportunityService.getOpportunityById(+this.recordId).subscribe({
      next: (response: any) => {
        // Handle new response structure with opportunity and baseEngagementNumber
        const data: Opportunity = response.opportunity || response;
        this.opportunity.set(data);
        
        // Store base engagement number if present
        if (response.baseEngagementNumber) {
          this.baseEngagementNumber.set(response.baseEngagementNumber);
        } else {
          this.baseEngagementNumber.set(null);
        }
        
        this.loading.set(false);
        this.updateLoadingProgress('opportunity', 'completed');

        // Generate banner images if name and description exist but no banner image yet
        if (data.name && data.description && !data.opportunityBannerImage) {
          this._generateBannerImages(data.id);
        }

        // STEP 2: Load AI insights and suggestions (required by Analysis section)
        this.updateLoadingProgress('insights', 'loading');
        this._loadInsights();

        // STEP 3: Trigger section data loading in visual order (top to bottom)
        this._orchestrateSectionLoading();

        // Only scroll after data loads if this is the initial page load with a section in the URL
        // This prevents scrolling on every data reload when navigating between sections
        if (
          this.shouldScrollAfterDataLoad &&
          targetSection &&
          this.isValidSection(targetSection)
        ) {
          this.pendingScrollTarget = targetSection;
          this.shouldScrollAfterDataLoad = false;

          // Wait for ALL section content to load before scrolling
          this.waitForContentAndScroll();
        } else {
          // Mark initial load as complete even if we didn't scroll
          this.isInitialLoad = false;
        }
      },
      error: (error) => {
        console.error('❌ Error loading opportunity details:', error);
        this.loading.set(false);
        this.updateLoadingProgress(
          'opportunity',
          'error',
          undefined,
          error.message
        );
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant(
            'message.opportunity.loadFailed',
          ),
          summary: this.translateService.instant('message.error'),
        });
      },
    });
  }

  /**
   * Generate banner and thumbnail images for the opportunity using AI
   */
  private _generateBannerImages(opportunityId: number): void {
    console.log('[OpportunityView] Generating banner images for opportunity', opportunityId);
    this.opportunityService.generateOpportunityImages(opportunityId).subscribe({
      next: (updatedOpportunity) => {
        console.log('[OpportunityView] Banner images generated successfully');
        // Update opportunity with generated images
        this.opportunity.set(updatedOpportunity);
        this.cdr.markForCheck();
      },
      error: (error) => {
        console.warn('[OpportunityView] Failed to generate banner images:', error);
        // Silently fail - don't show error to user as this is non-critical
      }
    });
  }

  /**
   * Regenerate banner image (user-triggered action)
   */
  regenerateBannerImage(): void {
    const currentOpportunity = this.opportunity();
    if (!currentOpportunity) return;

    this.isRegeneratingBanner.set(true);
    
    this.opportunityService.generateOpportunityImages(currentOpportunity.id).subscribe({
      next: (updatedOpportunity) => {
        this.opportunity.set(updatedOpportunity);
        this.isRegeneratingBanner.set(false);
        this.cdr.markForCheck();
        
        this.feedbackDialogService.showSuccessToast({
          detail: this.translateService.instant('message.opportunity.bannerRegenerated'),
          summary: this.translateService.instant('message.success'),
        });
      },
      error: (error) => {
        console.error('[OpportunityView] Error regenerating banner:', error);
        this.isRegeneratingBanner.set(false);
        
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('message.opportunity.bannerRegenerationFailed'),
          summary: this.translateService.instant('message.error'),
        });
      }
    });
  }

  /**
   * Load AI insights and suggestions for the opportunity (SINGLE API CALL)
   * This data is then passed to child components to avoid duplicate API requests
   */
  private _loadInsights(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) {
      this.updateLoadingProgress('insights', 'error', undefined, 'No opportunity ID');
      return;
    }

    this.insightsLoading.set(true);
    this.insightsError.set(null);

    this.opportunityService.getInsights(opportunityId).subscribe({
      next: (response) => {
        // Store both insights and suggestions for use across child components
        this.allInsights.set(response.insights || []);
        this.allSuggestions.set(response.suggestions || []);
        this.insightsLoading.set(false);
        this.updateLoadingProgress('insights', 'completed');

        // Mark analysis section as complete (it uses insights from parent)
        this.updateLoadingProgress('analysis', 'completed');

        console.log('✅ Insights loaded successfully:', {
          insightCount: response.insights?.length || 0,
          suggestionCount: response.suggestions?.length || 0,
        });
      },
      error: (error) => {
        console.error('❌ Error loading insights:', error);
        this.insightsError.set('Failed to load AI insights');
        this.insightsLoading.set(false);
        this.updateLoadingProgress('insights', 'error', undefined, error.message);
        this.updateLoadingProgress(
          'analysis',
          'error',
          undefined,
          'Insights failed to load'
        );
        // Silent failure for user - insights/suggestions are optional enhancements
      },
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
        summary: this.translateService.instant('message.permissionDenied'),
      });
      return;
    }

    // TODO: Implement edit dialog
    this.feedbackDialogService.showInfoToast({
      detail: this.translateService.instant(
        'message.opportunity.editComingSoon',
      ),
      summary: this.translateService.instant('message.info'),
    });
  }

  /**
   * @description Update loading progress for a section
   * @param sectionKey The section to update
   * @param status New status for the section
   * @param label Optional updated label
   * @param error Optional error message
   */
  private updateLoadingProgress(
    sectionKey: LoadingSectionKey,
    status: LoadingSectionStatus['status'],
    label?: string,
    error?: string
  ): void {
    this.loadingProgress.update((progress) => {
      const updatedSections = { ...progress.sections };
      const section = updatedSections[sectionKey];

      // Update section status
      updatedSections[sectionKey] = {
        ...section,
        status,
        label: label || section.label,
        error,
        startTime: status === 'loading' ? Date.now() : section.startTime,
        endTime:
          status === 'completed' || status === 'error' ? Date.now() : undefined,
      };

      // Calculate completed count
      const completed = Object.values(updatedSections).filter(
        (s) => s.status === 'completed' || s.status === 'error'
      ).length;

      // Find currently loading section
      const currentLoadingSection = Object.values(updatedSections).find(
        (s) => s.status === 'loading'
      );

      return {
        ...progress,
        sections: updatedSections,
        completed,
        currentSection: currentLoadingSection?.label || '',
      };
    });

    // Log progress update
    const progress = this.loadingProgress();
    console.log(
      `📈 Progress: ${progress.completed}/${progress.total} | ${sectionKey}: ${status}${error ? ` (${error})` : ''}`
    );
  }

  /**
   * @description Reset loading progress to initial state
   */
  private resetLoadingProgress(): void {
    this.loadingProgress.set(DEFAULT_LOADING_PROGRESS);
    console.log('🔄 Loading progress reset');
  }

  /**
   * @description Orchestrate section loading in visual order (top to bottom)
   * Uses sequential delays to match section display order and prevent connection exhaustion
   */
  private _orchestrateSectionLoading(): void {
    console.log('🎬 Starting orchestrated section loading...');

    // Analysis section uses insights loaded above - mark as complete
    setTimeout(() => {
      this.updateLoadingProgress('analysis', 'completed');
    }, 100);

    // DST Section - Risks (immediate)
    setTimeout(() => {
      this.updateLoadingProgress('dstRisks', 'loading');
      console.log('📊 Loading DST Risks...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.dstRisks.status === 'loading') {
          console.log('⏱️ Auto-completing dstRisks (timeout)');
          this.onDSTRisksLoaded();
        }
      }, 5000);
    }, 200);

    // DST Section - Recommendations (+500ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstRecommendations', 'loading');
      console.log('💡 Loading DST Recommendations...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.dstRecommendations.status === 'loading') {
          console.log('⏱️ Auto-completing dstRecommendations (timeout)');
          this.onDSTRecommendationsLoaded();
        }
      }, 5000);
    }, 700);

    // DST Section - Similar Opportunities (+1000ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstSimilarOpportunities', 'loading');
      console.log('🔍 Loading Similar Opportunities...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.dstSimilarOpportunities.status === 'loading') {
          console.log('⏱️ Auto-completing dstSimilarOpportunities (timeout)');
          this.onDSTSimilarOpportunitiesLoaded();
        }
      }, 5000);
    }, 1200);

    // DST Section - Similar Projects (+1500ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstSimilarProjects', 'loading');
      console.log('📁 Loading Similar Projects...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.dstSimilarProjects.status === 'loading') {
          console.log('⏱️ Auto-completing dstSimilarProjects (timeout)');
          this.onDSTSimilarProjectsLoaded();
        }
      }, 5000);
    }, 1700);

    // DST Section - Relevant People (+2000ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstRelevantPeople', 'loading');
      console.log('👥 Loading Relevant People...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.dstRelevantPeople.status === 'loading') {
          console.log('⏱️ Auto-completing dstRelevantPeople (timeout)');
          this.onDSTRelevantPeopleLoaded();
        }
      }, 5000);
    }, 2200);

    // Related Items Section (+2500ms)
    setTimeout(() => {
      this.updateLoadingProgress('relatedItems', 'loading');
      console.log('🔗 Loading Related Items...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.relatedItems.status === 'loading') {
          console.log('⏱️ Auto-completing relatedItems (timeout)');
          this.onRelatedItemsLoaded();
        }
      }, 5000);
    }, 2700);

    // Documents Panel (+3000ms)
    setTimeout(() => {
      this.updateLoadingProgress('documents', 'loading');
      console.log('📄 Loading Documents...');
      // Auto-complete after 5 seconds if not marked complete by effect
      setTimeout(() => {
        if (this.loadingProgress().sections.documents.status === 'loading') {
          console.log('⏱️ Auto-completing documents (timeout)');
          this.onDocumentsLoaded();
        }
      }, 5000);
    }, 3200);
  }

  /**
   * @description Called by DST section when risks are loaded
   */
  onDSTRisksLoaded(): void {
    this.updateLoadingProgress('dstRisks', 'completed');
  }

  /**
   * @description Called by DST section when recommendations are loaded
   */
  onDSTRecommendationsLoaded(): void {
    this.updateLoadingProgress('dstRecommendations', 'completed');
  }

  /**
   * @description Called by DST section when similar opportunities are loaded
   */
  onDSTSimilarOpportunitiesLoaded(): void {
    this.updateLoadingProgress('dstSimilarOpportunities', 'completed');
  }

  /**
   * @description Called by DST section when similar projects are loaded
   */
  onDSTSimilarProjectsLoaded(): void {
    this.updateLoadingProgress('dstSimilarProjects', 'completed');
  }

  /**
   * @description Called by DST section when relevant people are loaded
   */
  onDSTRelevantPeopleLoaded(): void {
    this.updateLoadingProgress('dstRelevantPeople', 'completed');
  }

  /**
   * @description Called by related items component when data is loaded
   */
  onRelatedItemsLoaded(): void {
    this.updateLoadingProgress('relatedItems', 'completed');
  }

  /**
   * @description Called by documents component when data is loaded
   */
  onDocumentsLoaded(): void {
    this.updateLoadingProgress('documents', 'completed');
  }

  /**
   * @description Handle loading errors from child components
   */
  onSectionLoadError(sectionKey: LoadingSectionKey, error: string): void {
    this.updateLoadingProgress(sectionKey, 'error', undefined, error);
  }

  /**
   * Delete opportunity with confirmation
   */
  deleteOpportunity(): void {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.noPermissionToDelete'),
        summary: this.translateService.instant('message.permissionDenied'),
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.translateService.instant(
        'message.confirmation.deleteOpportunity',
      ),
      header: this.translateService.instant('title.deleteOpportunity'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        if (this.opportunity()?.id) {
          this.opportunityService
            .deleteOpportunityById(this.opportunity()!.id!)
            .subscribe({
              next: () => {
                this.feedbackDialogService.showSuccessToast({
                  detail: this.translateService.instant(
                    'message.opportunity.deletedSuccessfully',
                  ),
                  summary: this.translateService.instant('message.success'),
                });
                this.router.navigate(['/partnerships/opportunities']);
              },
              error: (error) => {
                console.error('Error deleting opportunity:', error);
                // Error handled by global interceptor
              },
            });
        }
      },
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
    
    // Note: Source interactions are only reloaded on specific actions (e.g., partner changes from WHO section)
    // Do NOT reload source interactions on every opportunity update to avoid unnecessary API calls
    
    // Angular signals automatically notify ALL child components - no manual detectChanges() needed
    // All sections will re-render with latest data
    
    // Notify WHAT section and DST section to refresh AI-powered data (framework status, recommendations, etc.)
    this.handleSectionSaveComplete();
  }
  
  /**
   * Handle opportunity update that also requires refreshing related items
   * Use this when partners or stakeholders change (from WHO section)
   */
  handleOpportunityUpdateWithRelatedItems(updatedOpportunity: Opportunity): void {
    this.handleOpportunityUpdate(updatedOpportunity);
    
    // Refresh related items to reflect changes in partners/stakeholders
    if (this.relatedItemsComponent) {
      this.relatedItemsComponent.loadSourceInteractions();
    }
  }

  /**
   * Handle the "Not Applicable" flag change for UNOPS Missions from WHY section.
   * Updates the form control to enable real-time requirements validation.
   * @param value The new value of the flag
   */
  handleUnopsMissionsNotApplicableChange(value: boolean): void {
    this.opportunityForm.get('unopsMissionsNotApplicable')?.setValue(value);
  }

  /**
   * @description Reload opportunity data from API (e.g., after AI changes)
   * @returns {void}
   */
  reloadOpportunity(): void {
    if (this.recordId) {
      // Reload data without scrolling (user's scroll position is maintained)
      this.shouldScrollAfterDataLoad = false;
      this._loadRecordDetails();
    }
  }

  /**
   * @description Handle manual insights refresh request from Analysis Section
   * @returns {void}
   */
  handleInsightsRefresh(): void {
    console.log('🔄 Manual insights refresh requested');
    this._loadInsights();
  }

  /**
   * @description Handle document upload/link events from the documents component
   * Reloads the opportunity AND triggers AI recommendations refresh in the WHAT section
   * @returns {void}
   */
  handleDocumentUploaded(): void {
    // Increment the document upload trigger to notify WHAT section to refresh AI recommendations
    this.documentUploadTrigger.update(v => v + 1);
    
    // Also reload the opportunity data
    this.reloadOpportunity();
  }

  /**
   * @description Track when a section has unsaved changes
   * @param {string} sectionId - The section identifier (e.g., 'what', 'why', 'who')
   */
  handleSectionChangesDetected(sectionId: string): void {
    const currentSections = this.sectionsWithUnsavedChanges();
    const updatedSections = new Set(currentSections);
    updatedSections.add(sectionId);
    this.sectionsWithUnsavedChanges.set(updatedSections);
  }

  /**
   * @description Clear unsaved changes tracking for a section
   * @param {string} sectionId - The section identifier
   */
  handleSectionChangesSaved(sectionId: string): void {
    const currentSections = this.sectionsWithUnsavedChanges();
    const updatedSections = new Set(currentSections);
    updatedSections.delete(sectionId);
    this.sectionsWithUnsavedChanges.set(updatedSections);
  }

  /**
   * @description Handle section save completion - notifies WHAT section to refresh framework status
   * Called when any section successfully saves data
   */
  handleSectionSaveComplete(): void {
    // Increment the section save trigger to notify WHAT section to refresh framework status
    this.sectionSaveTrigger.update(v => v + 1);
  }

  /**
   * @description Handle successful workflow stage change
   * Reloads opportunity data to reflect the new stage and workflow status
   * Note: Success toast is shown by the specific action handler (e.g., Reopen, Cancel, Submit)
   */
  handleStageChangeSuccess(): void {
    // Reload the opportunity to get the updated stage and workflow status
    this.reloadOpportunity();
    // Note: Success feedback is handled by the specific action (Cancel, Reopen, Submit, etc.)
    // to show action-specific messages instead of a generic one
  }

  /**
   * @description Handle workflow action in progress state change
   * Shows/hides a full-page blur overlay while workflow actions are executing
   * @param {boolean} inProgress - true when action starts, false when action completes
   */
  handleWorkflowActionInProgressChange(inProgress: boolean): void {
    this.workflowActionInProgress.set(inProgress);
  }

  /**
   * @description Handle successful GO submission
   * Generates a PDF of the Opportunity Statement after successful submission to GO stage
   * @param {object} data - Event data containing entityName, entityId, and newStage
   */
  async handleGoSubmissionSuccess(data: { entityName: string; entityId: number; newStage: string }): Promise<void> {
    console.log('📄 GO submission successful, generating statement PDF...', data);

    if (!this.documentsComponent) {
      console.error('❌ Documents component not available for PDF generation');
      return;
    }

    // Fetch fresh opportunity data to get the updated statement markdown
    // (the backend regenerates the statement during submission)
    try {
      const response: any = await firstValueFrom(
        this.opportunityService.getOpportunityById(data.entityId)
      );
      const freshOpp = response.opportunity || response;
      
      // Update local opportunity signal with fresh data
      this.opportunity.set(freshOpp);

      const markdown = freshOpp.opportunityStatementMarkdown;
      if (!markdown) {
        console.warn('⚠️ No opportunity statement markdown available - skipping PDF generation');
        return;
      }

      // Generate PDF with submission filename: Opportunity_<ID>_Submission_YYYYMMDD_HHMM.pdf
      // Include time so multiple submissions (after recall) can be differentiated
      const now = new Date();
      const dateStr = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
      const timeStr = `${String(now.getHours()).padStart(2, '0')}${String(now.getMinutes()).padStart(2, '0')}`;
      const pdfFileName = `Opportunity_${data.entityId}_Submission_${dateStr}_${timeStr}.pdf`;
      
      await this.documentsComponent.generateStatementPdf(
        markdown,
        data.entityId,
        pdfFileName
      );
    } catch (error) {
      console.error('❌ Failed to generate statement PDF:', error);
      // PDF generation errors are already handled in the documents component
      // with appropriate user feedback, so we just log here
    }
  }

  /**
   * @description Handle successful GO approval
   * Generates a PDF of the Opportunity Statement after successful approval (GO decision)
   * Includes an audit trail footer with submission and approval details
   * @param {object} data - Event data containing entityName, entityId, and approvedStage
   */
  async handleGoApprovalSuccess(data: { entityName: string; entityId: number; approvedStage: string }): Promise<void> {
    console.log('📄 GO approval successful, generating statement PDF...', data);

    if (!this.documentsComponent) {
      console.error('❌ Documents component not available for PDF generation');
      return;
    }

    try {
      // Fetch fresh opportunity data
      const response: any = await firstValueFrom(
        this.opportunityService.getOpportunityById(data.entityId)
      );
      const freshOpp = response.opportunity || response;
      
      // Update local opportunity signal with fresh data
      this.opportunity.set(freshOpp);

      const markdown = freshOpp.opportunityStatementMarkdown;
      if (!markdown) {
        console.warn('⚠️ No opportunity statement markdown available - skipping PDF generation');
        return;
      }

      // Fetch workflow history to get submission and approval details
      const history = await firstValueFrom(
        this.workflowService.getStageChangeHistory('Opportunity', data.entityId.toString())
      );

      // Build the audit trail section
      const auditTrailMarkdown = this.buildAuditTrailMarkdown(history, freshOpp);

      // Combine statement with audit trail
      const fullMarkdown = markdown + '\n\n' + auditTrailMarkdown;

      // Generate PDF with approval filename: Opportunity_<ID>_Approved_YYYYMMDD.pdf
      const now = new Date();
      const dateStr = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
      const pdfFileName = `Opportunity_${data.entityId}_Approved_${dateStr}.pdf`;
      
      await this.documentsComponent.generateStatementPdf(
        fullMarkdown,
        data.entityId,
        pdfFileName
      );
    } catch (error) {
      console.error('❌ Failed to generate statement PDF:', error);
      // PDF generation errors are already handled in the documents component
      // with appropriate user feedback, so we just log here
    }
  }

  /**
   * @description Build audit trail markdown section for the approved opportunity statement PDF
   * Includes submission details, decision details, and acknowledgment statement
   * @param {WorkflowHistoryModel[]} history - Workflow history entries
   * @param {Opportunity} opportunity - The opportunity data
   * @returns {string} Markdown formatted audit trail section
   */
  private buildAuditTrailMarkdown(history: WorkflowHistoryModel[], opportunity: any): string {
    // Find the submission (Submit action) and approval (Approve action) records
    // Sort by date descending to get the most recent
    const sortedHistory = [...history].sort((a, b) => {
      const dateA = a.performedOn ? new Date(a.performedOn).getTime() : 0;
      const dateB = b.performedOn ? new Date(b.performedOn).getTime() : 0;
      return dateB - dateA;
    });

    const submitRecord = sortedHistory.find(h => h.action?.toLowerCase() === 'submit');
    const approveRecord = sortedHistory.find(h => h.action?.toLowerCase() === 'approve');

    // Format date helper
    const formatDate = (date: Date | string | undefined): string => {
      if (!date) return 'N/A';
      const d = new Date(date);
      return d.toLocaleDateString('en-GB', { 
        day: '2-digit', 
        month: 'short', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    };

    // Build acknowledgment statement with actual values
    const orgUnitName = opportunity?.responsibleOrgUnitName || 'N/A';
    const initiativeType = opportunity?.proposedInitiativeTypeName || 'initiative';
    const acknowledgmentStatement = `I confirm that ${orgUnitName} has the capacity and capability to deliver this ${initiativeType} within the proposed timeline and budget.`;

    // Build the audit trail markdown
    let auditTrail = `
---

## Go Decision Audit Trail

### Submission Details
| Field | Value |
|-------|-------|
| **Date of Submission** | ${formatDate(submitRecord?.performedOn)} |
| **Submitted By** | ${submitRecord?.performedBy?.userName || 'N/A'} |
| **Position Title** | ${submitRecord?.performedBy?.positionTitle || 'N/A'} |
| **Remarks for Decision Maker** | ${submitRecord?.comment || 'None provided'} |

### Decision Details
| Field | Value |
|-------|-------|
| **Date of Decision** | ${formatDate(approveRecord?.performedOn)} |
| **Decision Maker** | ${approveRecord?.performedBy?.userName || 'N/A'} |
| **DOA Level** | ${approveRecord?.performedBy?.doaLevel || 'N/A'} |
| **Position Title** | ${approveRecord?.performedBy?.positionTitle || 'N/A'} |
| **Acknowledged Statement** | ${acknowledgmentStatement} |
| **Decision Rationale** | ${approveRecord?.comment || 'None provided'} |

---
`;

    return auditTrail;
  }

  // ===== Go/No-Go Decision Handlers =====

  /**
   * @description Custom stage change handler for Go/No-Go decisions
   * Opens the appropriate dialog when Approve or Reject actions are triggered
   * @param {string} nextStage - The target stage name
   * @param {string} actionName - The action being performed (Approve, Reject, etc.)
   * @returns {Promise<CustomStageChangeResult | undefined>} Result with proceed flag and comment
   */
  customStageChangeHandler = async (
    nextStage: string,
    actionName: string
  ): Promise<CustomStageChangeResult | undefined> => {
    // Only intercept Approve and Reject actions for opportunity-specific dialogs
    if (actionName === 'Approve') {
      this.showApproveDialog.set(true);
      return new Promise<CustomStageChangeResult>((resolve) => {
        this.approveDialogResolver = resolve;
      });
    }
    
    if (actionName === 'Reject') {
      this.showRejectDialog.set(true);
      return new Promise<CustomStageChangeResult>((resolve) => {
        this.rejectDialogResolver = resolve;
      });
    }
    
    // For other actions, return undefined to use default behavior
    return undefined;
  };

  /**
   * @description Handle Go decision confirmation from approve dialog
   * @param {GoDecisionPayload} payload - The approval payload with rationale and executive
   */
  async onApproveConfirmed(payload: GoDecisionPayload): Promise<void> {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    // The dialog already handles the API call and shows success/error toasts
    // We resolve with proceed: false to prevent the workflow component from making a duplicate API call
    if (this.approveDialogResolver) {
      this.approveDialogResolver({
        proceed: false, // API call already made by the dialog
        comment: payload.rationale,
      });
      this.approveDialogResolver = null;
    }

    // Generate Approved PDF after successful Go decision (includes audit trail)
    await this.handleGoApprovalSuccess({
      entityName: 'Opportunity',
      entityId: opportunityId,
      approvedStage: 'GO',
    });

    // Reload opportunity to ensure UI is fully updated
    this.reloadOpportunity();
  }

  /**
   * @description Handle No-Go decision confirmation from reject dialog
   * @param {NoGoDecisionPayload} payload - The rejection payload with rationale
   */
  onRejectConfirmed(payload: NoGoDecisionPayload): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    // The dialog already handles the API call and shows success/error toasts
    // We resolve with proceed: false to prevent the workflow component from making a duplicate API call
    if (this.rejectDialogResolver) {
      this.rejectDialogResolver({
        proceed: false, // API call already made by the dialog
        comment: payload.rationale,
      });
      this.rejectDialogResolver = null;
    }

    // Reload opportunity to reflect new stage
    this.reloadOpportunity();
  }

  /**
   * @description Handle dialog cancellation
   * Resolves the promise with proceed: false to cancel the workflow action
   */
  onDialogCancel(): void {
    if (this.approveDialogResolver) {
      this.approveDialogResolver({ proceed: false });
      this.approveDialogResolver = null;
    }
    if (this.rejectDialogResolver) {
      this.rejectDialogResolver({ proceed: false });
      this.rejectDialogResolver = null;
    }
  }

  /**
   * @description Update risks signal when DST section loads risks
   * @param {Risk[]} risks - Array of risks from DST section
   */
  updateRisks(risks: Risk[]): void {
    this.opportunityRisks.set(risks);
  }

  /**
   * @description Save all sections with unsaved changes
   */
  saveAllSections(): void {
    const sectionsToSave = Array.from(this.sectionsWithUnsavedChanges());
    
    if (sectionsToSave.length === 0) {
      return;
    }

    // Trigger save on each section with unsaved changes
    sectionsToSave.forEach(sectionId => {
      switch (sectionId) {
        case 'overview':
          if (this.overviewSectionComponent) {
            this.overviewSectionComponent.saveSection();
          }
          break;
        case 'what':
          if (this.whatSectionComponent) {
            this.whatSectionComponent.saveSection();
          }
          break;
        case 'why':
          if (this.whySectionComponent) {
            this.whySectionComponent.saveSection();
          }
          break;
        case 'who':
          if (this.whoSectionComponent) {
            this.whoSectionComponent.saveSection();
          }
          break;
        case 'team':
          if (this.teamSectionComponent) {
            this.teamSectionComponent.saveSection();
          }
          break;
        case 'where':
          if (this.whereSectionComponent) {
            this.whereSectionComponent.saveSection();
          }
          break;
        case 'when':
          if (this.whenSectionComponent) {
            this.whenSectionComponent.saveSection();
          }
          break;
        case 'risks':
          if (this.dstSectionComponent) {
            this.dstSectionComponent.saveSection();
          }
          break;
      }
    });
  }

  /**
   * @description Discard all unsaved changes
   */
  discardAllChanges(): void {
    const sectionsToDiscard = Array.from(this.sectionsWithUnsavedChanges());
    
    if (sectionsToDiscard.length === 0) {
      return;
    }

    // Show confirmation dialog
    this.feedbackDialogService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.discardChanges'),
        detail: this.translateService.instant('message.confirmDiscardAllChanges')
      },
      () => {
        // Trigger cancel/revert on each section with unsaved changes
        sectionsToDiscard.forEach(sectionId => {
          switch (sectionId) {
            case 'overview':
              if (this.overviewSectionComponent) {
                this.overviewSectionComponent.cancelEditing();
              }
              break;
            case 'what':
              if (this.whatSectionComponent) {
                this.whatSectionComponent.cancelEditing();
              }
              break;
            case 'why':
              if (this.whySectionComponent) {
                this.whySectionComponent.cancelEditing();
              }
              break;
            case 'who':
              if (this.whoSectionComponent) {
                this.whoSectionComponent.cancelEditing();
              }
              break;
            case 'team':
              if (this.teamSectionComponent) {
                this.teamSectionComponent.cancelEditing();
              }
              break;
            case 'where':
              if (this.whereSectionComponent) {
                this.whereSectionComponent.cancelEditing();
              }
              break;
            case 'when':
              if (this.whenSectionComponent) {
                this.whenSectionComponent.cancelEditing();
              }
              break;
            case 'risks':
              if (this.dstSectionComponent) {
                this.dstSectionComponent.cancelEditing();
              }
              break;
          }
        });

        // Clear all unsaved changes tracking
        this.sectionsWithUnsavedChanges.set(new Set());
        
        this.feedbackDialogService.showInfoToast({
          summary: this.translateService.instant('message.changesDiscarded'),
          detail: this.translateService.instant('message.allChangesDiscarded')
        });
      }
    );
  }

  /**
   * @description Handle section click to enter edit mode when user clicks on a section
   * @param {string} sectionId - The section identifier
   * @param {Event} event - The click event
   */
  handleSectionClick(sectionId: string, event: Event): void {
    // Get the target element
    const target = event.target as HTMLElement;
    
    // Don't trigger edit mode if clicking on interactive elements
    if (
      target.closest('button') ||
      target.closest('a') ||
      target.closest('input') ||
      target.closest('textarea') ||
      target.closest('select') ||
      target.closest('.p-button') ||
      target.closest('.p-inputtext') ||
      target.closest('.p-select') ||
      target.closest('.p-dropdown') ||
      target.closest('.p-calendar') ||
      target.closest('[role="button"]')
    ) {
      return;
    }

    // Only allow section click if user has update permissions and section doesn't have unsaved changes
    if (!this.canUpdate() || this.sectionsWithUnsavedChanges().has(sectionId)) {
      return;
    }

    // Trigger edit mode for the clicked section
    switch (sectionId) {
      case 'overview':
        this.overviewSectionComponent?.startEditing();
        break;
      case 'what':
        this.whatSectionComponent?.startEditing();
        break;
      case 'why':
        this.whySectionComponent?.startEditing();
        break;
      case 'who':
        this.whoSectionComponent?.startEditing();
        break;
      case 'team':
        this.teamSectionComponent?.startEditing();
        break;
      case 'where':
        this.whereSectionComponent?.startEditing();
        break;
      case 'when':
        this.whenSectionComponent?.startEditing();
        break;
      case 'risks':
        this.dstSectionComponent?.startEditing();
        break;
    }
  }

  /**
   * @description Handle Escape key press to exit edit mode
   * @param {KeyboardEvent} event - The keyboard event
   */
  // TAD: DISABLED FOR NOW - WILL RE-ENABLE LATER IF NEEDED (MAY BE TOO CONFUSING FOR USERS - USERS MAY LOSE DATA IF THEY PRESS ESC BY MISTAKE)
  // @HostListener('document:keydown.escape', ['$event'])
  // handleEscapeKey(event: KeyboardEvent): void {
  //   // Don't handle escape if user is in a dialog or modal
  //   const target = event.target as HTMLElement;
  //   if (target.closest('.p-dialog') || target.closest('[role="dialog"]')) {
  //     return;
  //   }

  //   // Check each section component and cancel editing if in edit mode
  //   if (this.overviewSectionComponent?.isEditing?.()) {
  //     this.overviewSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.whatSectionComponent?.isEditing?.()) {
  //     this.whatSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.whySectionComponent?.isEditing?.()) {
  //     this.whySectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.whoSectionComponent?.isEditing?.()) {
  //     this.whoSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.teamSectionComponent?.isEditing?.()) {
  //     this.teamSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.whereSectionComponent?.isEditing?.()) {
  //     this.whereSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.whenSectionComponent?.isEditing?.()) {
  //     this.whenSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }

  //   if (this.dstSectionComponent?.isEditing?.()) {
  //     this.dstSectionComponent.cancelEditing();
  //     event.preventDefault();
  //     return;
  //   }
  // }

  /**
   * @description Handle window resize to update innerWidth signal
   * @param {Event} event - The resize event
   */
  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    this.innerWidth.set((event.target as Window).innerWidth);
    // Chip overflow is handled by ResizeObserver on the sizer div
  }

  /**
   * Toggle full content display
   */
  toggleFullContent() {
    this.showFullContent.update((value) => !value);
  }

  /**
   * Open OUP engagement in new tab
   */
  openOUPEngagement(): void {
    const url = this.oupEngagementUrl();
    if (url) {
      window.open(url, '_blank');
    }
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
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
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
  getStatusSeverity(
    status: string | undefined,
  ): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
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
  getRiskScoreSeverity(
    riskScore: number,
  ): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    if (riskScore >= 8) return 'danger';
    if (riskScore >= 6) return 'warn';
    if (riskScore >= 4) return 'info';
    return 'success';
  }

  /**
   * Scroll to section and update URL
   * Called when user clicks navigation chips/dropdown
   */
  scrollToSection(sectionId: string): void {
    if (this.navigationInProgress) {
      return;
    }

    this.navigationInProgress = true;
    this.isScrolling = true; // Set flag to prevent scroll spy from updating URL
    this.lastManualNavigationTime = Date.now(); // Track when manual navigation occurred
    this.activeSection.set(sectionId);

    // Update URL with the section parameter using Location API to avoid component reload
    const currentUrl = this.router.url.split('?')[0]; // Remove query params
    const urlSegments = currentUrl.split('/');

    // Check if we already have a section in the URL
    const lastSegment = urlSegments[urlSegments.length - 1];
    const isSection = this.isValidSection(lastSegment);

    let newUrl: string;
    if (isSection) {
      // Replace existing section in URL
      newUrl = [...urlSegments.slice(0, -1), sectionId].join('/');
    } else {
      // Add section to URL
      newUrl = [...urlSegments, sectionId].join('/');
    }

    // Use Location.replaceState() instead of Router.navigate() to update URL
    // without triggering Angular's routing mechanism and component reload
    this.location.replaceState(newUrl);

    // Scroll to the section
    this.scrollToSectionInternal(sectionId);

    setTimeout(() => {
      this.navigationInProgress = false;
      // Clear scrolling flag after animation completes - shorter delay
      setTimeout(() => {
        this.isScrolling = false;
      }, 800); // Reduced from 1500ms to 800ms
    }, 100);
  }

  /**
   * Handle click on a requirement item in the requirements validation panel.
   * Navigates to the section containing the required field and scrolls to the specific field.
   * @param event - The requirement click event containing section and field information
   */
  handleRequirementClick(event: RequirementClickEvent): void {
    if (event.section && this.isValidSection(event.section)) {
      this.scrollToSection(event.section);

      // After scrolling to section, scroll to the specific field and highlight it
      if (event.fieldName) {
        // Use a delay to allow section scroll to complete
        setTimeout(() => {
          this.scrollToFieldAndHighlight(event.fieldName!);
        }, 600);
      }
    }
  }

  /**
   * Scroll to a specific field element and apply highlight effect.
   * @param fieldName - The field name to scroll to (matches id="field-{fieldName}")
   */
  private scrollToFieldAndHighlight(fieldName: string): void {
    const fieldElement = document.getElementById(`field-${fieldName}`);
    if (fieldElement) {
      // Scroll the field into view with smooth behavior
      fieldElement.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      });

      // Add highlight effect
      fieldElement.classList.add('field-highlight');

      // Remove highlight after animation completes
      setTimeout(() => {
        fieldElement.classList.remove('field-highlight');
      }, 2000);
    }
  }

  /**
   * Wait for all section content to load before scrolling
   * Polls until all asynchronous content has finished loading
   */
  private waitForContentAndScroll(): void {
    // Don't scroll if the window doesn't have focus (prevents stealing focus)
    if (!document.hasFocus()) {
      this.pendingScrollTarget = null;
      this.isInitialLoad = false;
      this.isScrolling = false;
      return;
    }

    // Wait 500ms for ViewChild components to initialize before starting polling
    setTimeout(() => {
      let checkCount = 0;
      const maxChecks = 100; // Maximum 10 seconds (100 * 100ms)

      this.scrollCheckInterval = window.setInterval(() => {
        checkCount++;

        // Check main opportunity loading
        const mainLoading = this.loading();

        // Check DST section loading (risks, recommendations, similar opportunities, projects, people)
        let dstLoading = false;
        let dstDetails = '';
        if (this.dstSectionComponent) {
          const risks = this.dstSectionComponent.loadingRisks();
          const recs = this.dstSectionComponent.loadingRecommendations();
          const simOps = this.dstSectionComponent.loadingSimilarOpportunities();
          const simProjs = this.dstSectionComponent.loadingSimilarProjects();
          const people = this.dstSectionComponent.loadingRelevantPeople();
          dstLoading = risks || recs || simOps || simProjs || people;
          dstDetails = `[risks=${risks}, recs=${recs}, simOps=${simOps}, simProjs=${simProjs}, people=${people}]`;
        } else {
          dstDetails = '[component not initialized]';
        }

        // Check Analysis section loading (AI insights)
        const analysisLoading = this.analysisSectionComponent
          ? this.analysisSectionComponent.loadingInsights()
          : false;

        // Check Why section loading (targets, indicators)
        const whyLoading = this.whySectionComponent
          ? this.whySectionComponent.loadingTargets()
          : false;

        // Check Documents section loading
        const documentsLoading = this.documentsComponent
          ? this.documentsComponent.loading() ||
            this.documentsComponent.uploading()
          : false;

        // Check Related Items section loading
        const relatedItemsLoading = this.relatedItemsComponent
          ? this.relatedItemsComponent.isLoading()
          : false;

        const allContentLoaded =
          !mainLoading &&
          !dstLoading &&
          !analysisLoading &&
          !whyLoading &&
          !documentsLoading &&
          !relatedItemsLoading;

        // If all content is loaded OR we've exceeded max checks, scroll now
        if (allContentLoaded || checkCount >= maxChecks) {
          clearInterval(this.scrollCheckInterval);
          this.scrollCheckInterval = undefined;

          if (this.pendingScrollTarget) {
            const target = this.pendingScrollTarget;
            this.pendingScrollTarget = null;

            // Don't auto-reset isScrolling flag - we'll control it manually with longer delay
            this.scrollToSectionInternal(target, false);
            this.isInitialLoad = false;

            // Extend the isScrolling flag for longer (3 seconds) after initial route-based scroll
            // to prevent scroll spy from immediately detecting other visible sections while content settles
            setTimeout(() => {
              this.isScrolling = false;
            }, 3000);
          }
        }
      }, 100); // Check every 100ms
    }, 500); // Wait 500ms for ViewChild initialization
  }

  /**
   * Internal scroll to section logic
   * Called for both programmatic navigation and URL-based navigation
   * @param sectionId - The section to scroll to
   * @param resetScrollingFlag - If true, resets isScrolling flag after animation (default: true)
   */
  private scrollToSectionInternal(
    sectionId: string,
    resetScrollingFlag: boolean = true,
  ): void {
    // Track manual navigation time to prevent scroll spy interference
    this.lastManualNavigationTime = Date.now();
    this.isScrolling = true;

    // Wait for content scroll container to be available
    setTimeout(() => {
      if (!this.contentScrollContainer?.nativeElement) {
        return;
      }

      // For the first section (analysis), scroll to the top of the content
      if (sectionId === 'analysis') {
        this.contentScrollContainer.nativeElement.scrollTo({
          top: 0,
          behavior: 'smooth',
        });

        // Reset scrolling flag after animation if requested
        if (resetScrollingFlag) {
          setTimeout(() => {
            this.isScrolling = false;
          }, 800);
        }
        return;
      }

      // For other sections, scroll to the section within the content container
      const element = document.getElementById(`section-${sectionId}`);
      if (element) {
        const container = this.contentScrollContainer.nativeElement;
        const elementTop = element.offsetTop;
        const containerTop = container.offsetTop;
        const scrollPosition = elementTop - containerTop - 20; // 20px offset for spacing

        container.scrollTo({ top: scrollPosition, behavior: 'smooth' });
      }

      // Reset scrolling flag after animation if requested
      if (resetScrollingFlag) {
        setTimeout(() => {
          this.isScrolling = false;
        }, 800);
      }
    }, 50);
  }

  /**
   * Validate section ID
   */
  private isValidSection(section: string): boolean {
    return this.sections.some((s) => s.id === section);
  }

  /**
   * Setup scroll spy using IntersectionObserver
   * Automatically updates active section and URL as user scrolls
   * Only blocks during programmatic scrolling, works immediately for manual scrolling
   */
  private setupScrollSpy(): void {
    // Create an intersection observer to detect which section is in view
    const observerOptions = {
      root: this.contentScrollContainer?.nativeElement || null,
      rootMargin: '-10% 0px -50% 0px', // Wider detection zone: triggers when section is in top 40% of viewport (10% to 50% from top)
      threshold: [0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0], // Multiple thresholds for better visibility tracking
    };

    this.intersectionObserver = new IntersectionObserver((entries) => {
      // ONLY block during active programmatic scrolling (not for manual scrolling)
      if (this.isScrolling) {
        return;
      }

      // Clear any existing timeout
      if (this.scrollTimeout) {
        clearTimeout(this.scrollTimeout);
      }

      // Small debounce to group rapid events
      this.scrollTimeout = window.setTimeout(() => {
        // Get all currently observed sections and calculate their visibility
        const visibleSections: Array<{
          id: string;
          ratio: number;
          visiblePixels: number;
          element: Element;
        }> = [];

        const containerRect = this.contentScrollContainer?.nativeElement.getBoundingClientRect();
        if (!containerRect) return;

        // Check all sections for visibility (always do comprehensive check)
        this.sections.forEach((section) => {
          const element = document.getElementById(`section-${section.id}`);
          if (element) {
            const rect = element.getBoundingClientRect();
            
            // Check if section is in viewport
            const isVisible = rect.top < containerRect.bottom && rect.bottom > containerRect.top;
            if (isVisible) {
              // Calculate visible area
              const visibleTop = Math.max(rect.top, containerRect.top);
              const visibleBottom = Math.min(rect.bottom, containerRect.bottom);
              const visibleHeight = visibleBottom - visibleTop;
              const ratio = rect.height > 0 ? visibleHeight / rect.height : 0;
              
              if (visibleHeight > 0) {
                visibleSections.push({
                  id: section.id,
                  ratio: ratio,
                  visiblePixels: visibleHeight,
                  element: element,
                });
              }
            }
          }
        });

        // Find the section with the best visibility score
        // Prioritize sections that are in the upper portion of the viewport
        // by combining ratio and position weighting
        if (visibleSections.length > 0) {
          const mostVisible = visibleSections.reduce((prev, current) => {
            const prevRect = prev.element.getBoundingClientRect();
            const currentRect = current.element.getBoundingClientRect();
            
            // Calculate position scores (sections closer to top get higher score)
            const prevPosition = Math.max(0, containerRect.top - prevRect.top);
            const currentPosition = Math.max(0, containerRect.top - currentRect.top);
            
            // Combined score: ratio (60% weight) + visible pixels (20% weight) + position preference (20% weight)
            const prevScore = (prev.ratio * 0.6) + (Math.min(prev.visiblePixels / 500, 1) * 0.2) + (prevPosition > 0 ? 0.2 : 0);
            const currentScore = (current.ratio * 0.6) + (Math.min(current.visiblePixels / 500, 1) * 0.2) + (currentPosition > 0 ? 0.2 : 0);
            
            return currentScore > prevScore ? current : prev;
          });

          const sectionId = mostVisible.id;

          // Only update if it's a different section
          if (
            sectionId &&
            this.isValidSection(sectionId) &&
            sectionId !== this.activeSection()
          ) {
            this.activeSection.set(sectionId);

            // Update URL using Location API to avoid triggering Angular router
            const currentUrl = this.router.url.split('?')[0];
            const urlSegments = currentUrl.split('/');
            const lastSegment = urlSegments[urlSegments.length - 1];
            const isSection = this.isValidSection(lastSegment);

            let newUrl: string;
            if (isSection) {
              // Replace existing section in URL
              newUrl = [...urlSegments.slice(0, -1), sectionId].join('/');
            } else {
              // Add section to URL
              newUrl = [...urlSegments, sectionId].join('/');
            }

            // Use Location.replaceState() instead of Router.navigate() to update URL
            // without triggering Angular's routing mechanism
            this.location.replaceState(newUrl);

            // Signal automatically triggers change detection - no manual call needed
          }
        }
      }, 100); // Reduced to 100ms for more responsive updates
    }, observerOptions);

    // Observe all section elements
    this.sections.forEach((section) => {
      const element = document.getElementById(`section-${section.id}`);
      if (element) {
        this.intersectionObserver!.observe(element);
      }
    });
  }

  /**
   * Toggle documents panel
   */
  toggleDocumentsPanel(): void {
    this.documentsCollapsed.update((v) => !v);
  }

  /**
   * Get file icon for document
   */
  getFileIcon(fileType: string): string {
    const iconMap: { [key: string]: string } = {
      pdf: 'pi-file-pdf',
      docx: 'pi-file-word',
      xlsx: 'pi-file-excel',
      pptx: 'pi-file-powerpoint',
      default: 'pi-file',
    };
    return iconMap[fileType] || iconMap['default'];
  }

  /**
   * Get file icon color for document
   */
  getFileIconColor(fileType: string): string {
    const colorMap: { [key: string]: string } = {
      pdf: 'text-red-500',
      docx: 'text-blue-500',
      xlsx: 'text-green-500',
      pptx: 'text-orange-500',
      default: 'text-gray-500',
    };
    return colorMap[fileType] || colorMap['default'];
  }

  /**
   * Handle file upload
   */
  onFileUpload(event: any): void {
    console.log('File uploaded:', event);
    // TODO: Implement file upload
  }

  /**
   * @description Setup resize observer for the sizer div to detect container width changes
   * This approach ensures we respond to any container width changes, not just window resizes
   */
  private setupChipOverflowObserver(): void {
    if (!this.chipsSizerDiv?.nativeElement) {
      // If sizer div not ready, try again later
      setTimeout(() => {
        this.setupChipOverflowObserver();
      }, 100);
      return;
    }

    this.resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        // Only recalculate if the width actually changed
        if (entry.contentRect.width > 0) {
          this.calculateChipOverflow();
        }
      }
    });

    this.resizeObserver.observe(this.chipsSizerDiv.nativeElement);
  }

  /**
   * @description Calculate which chips fit in the available width and which overflow
   */
  private calculateChipOverflow(): void {
    if (!this.chipsSizerDiv?.nativeElement || !this.chipsContainer?.nativeElement) {
      return;
    }

    // Use the sizer div width as the available container width
    const sizerDiv = this.chipsSizerDiv.nativeElement as HTMLElement;
    const containerWidth = sizerDiv.clientWidth;
    
    // If container has no width yet, try again later
    if (containerWidth === 0) {
      setTimeout(() => this.calculateChipOverflow(), 100);
      return;
    }
    
    const gap = 8; // gap-2 in Tailwind (0.5rem = 8px)
    
    // Create temporary elements to measure chip widths
    const tempContainer = document.createElement('div');
    tempContainer.style.visibility = 'hidden';
    tempContainer.style.position = 'absolute';
    tempContainer.style.whiteSpace = 'nowrap';
    document.body.appendChild(tempContainer);

    const chipWidths: number[] = [];
    
    // Measure each chip's width
    this.sections.forEach((section) => {
      const tempChip = document.createElement('button');
      tempChip.className = 'flex items-center gap-2 px-4 py-2 rounded-full bg-unops-surface-primary text-unops-neutral-700 font-medium text-sm whitespace-nowrap';
      tempChip.innerHTML = `
        <i class="pi ${section.icon} text-sm"></i>
        <span>${this.translateService.instant(section.label)}</span>
      `;
      tempContainer.appendChild(tempChip);
      chipWidths.push(tempChip.offsetWidth);
      tempContainer.removeChild(tempChip);
    });

    // Measure the "More..." dropdown width
    const moreDropdown = document.createElement('div');
    moreDropdown.className = 'flex items-center gap-2 px-4 py-2 rounded-full bg-unops-surface-primary text-unops-neutral-700 font-medium text-sm whitespace-nowrap';
    moreDropdown.innerHTML = `
      <i class="pi pi-ellipsis-h text-sm"></i>
      <span>More...</span>
    `;
    tempContainer.appendChild(moreDropdown);
    const moreChipWidth = moreDropdown.offsetWidth;
    tempContainer.removeChild(moreDropdown);

    document.body.removeChild(tempContainer);

    // First pass: Calculate how many chips fit WITHOUT the "More..." chip
    let totalWidth = 0;
    let visibleCount = 0;
    
    for (let i = 0; i < chipWidths.length; i++) {
      const chipWidth = chipWidths[i];
      const gapWidth = i > 0 ? gap : 0;
      const newWidth = totalWidth + chipWidth + gapWidth;
      
      if (newWidth <= containerWidth) {
        totalWidth = newWidth;
        visibleCount++;
      } else {
        break;
      }
    }

    // If all chips fit, we're done
    if (visibleCount === chipWidths.length) {
      this.visibleChips.set(this.sections);
      this.overflowChips.set([]);
      this.cdr.markForCheck();
      return;
    }

    // Not all chips fit, so we need the "More..." dropdown
    // Recalculate with space reserved for "More..." chip
    totalWidth = 0;
    visibleCount = 0;
    const availableWidth = containerWidth - moreChipWidth - gap;
    
    for (let i = 0; i < chipWidths.length; i++) {
      const chipWidth = chipWidths[i];
      const gapWidth = i > 0 ? gap : 0;
      const newWidth = totalWidth + chipWidth + gapWidth;
      
      if (newWidth <= availableWidth) {
        totalWidth = newWidth;
        visibleCount++;
      } else {
        break;
      }
    }

    // Ensure at least one chip is always visible
    if (visibleCount === 0) {
      visibleCount = 1;
    }

    // Update visible and overflow chips
    this.visibleChips.set(this.sections.slice(0, visibleCount));
    this.overflowChips.set(this.sections.slice(visibleCount));
    
    // Trigger change detection
    this.cdr.markForCheck();
  }
}
