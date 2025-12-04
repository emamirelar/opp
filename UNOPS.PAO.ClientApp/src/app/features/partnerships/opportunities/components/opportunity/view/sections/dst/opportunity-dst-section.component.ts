/**
 * @fileoverview DST (Digital Strategy & Transformation) Section Component
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, computed, inject, input, signal, ChangeDetectionStrategy, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { DialogModule } from 'primeng/dialog';
import { FloatLabelModule } from 'primeng/floatlabel';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { TreeSelectModule } from 'primeng/treeselect';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { TreeNode } from 'primeng/api';

// Models
import {
  Opportunity,
  DSTSeverity,
  SimilarProject,
  SimilarProjectsResponse,
  SimilarOpportunity,
  SimilarOpportunitiesResponse,
  RelevantPerson,
  RelevantPeopleResponse,
  Risk,
  AIRiskRecommendation,
  RiskCreateRequest,
  RiskLookupsResponse,
  RiskCategoryHierarchyResponse,
  RiskCategoryModel,
  RiskTypeModel,
  RiskProbabilityModel,
  RiskProximityModel,
  RiskImpactLevelModel,
  RiskResponseTypeModel,
} from '@shared/models/opportunity.model';
import { OpportunityService } from '../../../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';

/**
 * @class OpportunityDstSectionComponent
 * @description Component for displaying DST Insights & Recommendations section.
 * Shows AI-powered complexity scores, risks, recommendations, and similar opportunities.
 * 
 * @example
 * ```html
 * <app-opportunity-dst-section
 *   [opportunity]="opportunity()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-dst-section',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    DividerModule,
    TagModule,
    ChipModule,
    BadgeModule,
    AvatarModule,
    DialogModule,
    FloatLabelModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    MessageModule,
    TooltipModule,
    TreeSelectModule,
    CheckboxModule,
  ],
  templateUrl: './opportunity-dst-section.component.html',
  styleUrls: ['./opportunity-dst-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpportunityDstSectionComponent {
  // Injected services
  private readonly opportunityService = inject(OpportunityService);
  private readonly feedbackService = inject(FeedbackDialogService);

  /**
   * @description The opportunity data containing DST analysis
   * @type {Signal<Opportunity>}
   * @since 1.0.0
   */
  readonly opportunity = input.required<Opportunity>();
  
  /**
   * @description Track the last loaded opportunity ID to prevent duplicate API calls
   * @type {number | null}
   * @private
   * @since 1.0.0
   */
  private lastLoadedOpportunityId: number | null = null;

  /**
   * @description Signal for similar projects data
   * @type {WritableSignal<SimilarProject[] | null>}
   * @since 1.0.0
   */
  readonly similarProjects = signal<SimilarProject[] | null>(null);

  /**
   * @description Signal for full similar projects response
   * @type {WritableSignal<SimilarProjectsResponse | null>}
   * @since 1.0.0
   */
  readonly similarProjectsResponse = signal<SimilarProjectsResponse | null>(null);

  /**
   * @description Loading state for similar projects
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly loadingSimilarProjects = signal<boolean>(false);

  /**
   * @description Error message for similar projects loading
   * @type {WritableSignal<string | null>}
   * @since 1.0.0
   */
  readonly similarProjectsError = signal<string | null>(null);

  /**
   * @description Signal for similar opportunities data
   * @type {WritableSignal<SimilarOpportunity[] | null>}
   * @since 1.0.0
   */
  readonly similarOpportunities = signal<SimilarOpportunity[] | null>(null);

  /**
   * @description Signal for full similar opportunities response
   * @type {WritableSignal<SimilarOpportunitiesResponse | null>}
   * @since 1.0.0
   */
  readonly similarOpportunitiesResponse = signal<SimilarOpportunitiesResponse | null>(null);

  /**
   * @description Loading state for similar opportunities
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly loadingSimilarOpportunities = signal<boolean>(false);

  /**
   * @description Error message for similar opportunities loading
   * @type {WritableSignal<string | null>}
   * @since 1.0.0
   */
  readonly similarOpportunitiesError = signal<string | null>(null);

  /**
   * @description Signal for relevant people data
   * @type {WritableSignal<RelevantPerson[] | null>}
   * @since 1.0.0
   */
  readonly relevantPeople = signal<RelevantPerson[] | null>(null);

  /**
   * @description Full relevant people response
   * @type {WritableSignal<RelevantPeopleResponse | null>}
   * @since 1.0.0
   */
  readonly relevantPeopleResponse = signal<RelevantPeopleResponse | null>(null);

  /**
   * @description Loading state for relevant people
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly loadingRelevantPeople = signal<boolean>(false);

  /**
   * @description Error message for relevant people loading
   * @type {WritableSignal<string | null>}
   * @since 1.0.0
   */
  readonly relevantPeopleError = signal<string | null>(null);

  /**
   * @description Signal for risks from the register
   * @type {WritableSignal<Risk[]>}
   * @since 1.0.0
   */
  readonly risks = signal<Risk[]>([]);

  /**
   * @description Loading state for risks
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly loadingRisks = signal<boolean>(false);

  /**
   * @description Signal for AI-generated recommendations
   * @type {WritableSignal<AIRiskRecommendation[]>}
   * @since 1.0.0
   */
  readonly recommendations = signal<AIRiskRecommendation[]>([]);

  /**
   * @description Loading state for recommendations
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly loadingRecommendations = signal<boolean>(false);

  /**
   * @description Show/hide add risk dialog
   * @type {boolean}
   * @since 1.0.0
   */
  showAddRiskDialog = false;

  /**
   * @description Acknowledgement that user has reviewed all organizational high risks
   * AC1: User must acknowledge they've reviewed all applicable high risks
   * Loaded from and persisted to the Opportunity entity
   * @type {boolean}
   * @since 2.0.0
   */
  get highRiskAcknowledged(): boolean {
    return this.opportunity()?.highRisksAcknowledged ?? false;
  }

  set highRiskAcknowledged(value: boolean) {
    this.updateHighRiskAcknowledgement(value);
  }

  private isUpdatingAcknowledgement = false;

  /**
   * @description Show validation errors in dialog
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly showDialogValidationError = signal<boolean>(false);

  /**
   * @description Processing state for risk submission
   * @type {WritableSignal<boolean>}
   * @since 1.0.0
   */
  readonly isProcessingRisk = signal<boolean>(false);

  /**
   * @description Processing state for risk deletion
   * @type {WritableSignal<boolean>}
   * @since 2.0.0
   */
  readonly isDeletingRisk = signal<boolean>(false);

  /**
   * @description New risk form data (oUP aligned mandatory fields)
   * @since 2.0.0
   */
  newRisk: {
    title: string;
    description: string;
    recommendation: string;
    riskTypeId: number | null;
    riskCategoryId: number | null;
    riskProbabilityId: number | null;
    riskProximityId: number | null;
    riskImpactLevelId: number | null;
    riskResponseTypeId: number | null;
    preDefinedHighRiskId?: number | null; // Link to PreDefinedHighRisk if from checklist
    impact: number;
  } = {
    title: '',
    description: '',
    recommendation: '',
    riskTypeId: null,
    riskCategoryId: null,
    riskProbabilityId: null,
    riskProximityId: null,
    riskImpactLevelId: null,
    riskResponseTypeId: null,
    impact: 2
  };

  /**
   * @description Legacy impact options for dropdown (backward compatibility)
   * @since 1.0.0
   */
  readonly impactOptions = [
    { label: 'Low', value: 1 },
    { label: 'Medium', value: 2 },
    { label: 'High', value: 3 }
  ];

  /**
   * @description Risk lookups data (types, probabilities, proximities, impact levels, response types)
   * @since 2.0.0
   */
  readonly riskLookups = signal<RiskLookupsResponse | null>(null);

  /**
   * @description Risk categories hierarchical data
   * @since 2.0.0
   */
  readonly riskCategories = signal<RiskCategoryHierarchyResponse | null>(null);

  /**
   * @description Risk categories as TreeNode array for p-treeselect
   * Only Level 3 categories are selectable
   * @since 2.0.0
   */
  readonly categoryTreeNodes = computed<TreeNode[]>(() => {
    const categories = this.riskCategories();
    if (!categories?.categories) return [];
    return this.convertCategoriesToTreeNodes(categories.categories);
  });

  /**
   * @description Selected category node for TreeSelect binding
   * @since 2.0.0
   */
  selectedCategoryNode: TreeNode | null = null;

  /**
   * @description Loading state for risk lookups
   * @since 2.0.0
   */
  readonly loadingRiskLookups = signal<boolean>(false);

  /**
   * @description Computed: filtered response types based on selected risk type
   * @since 2.0.0
   */
  readonly filteredResponseTypes = computed(() => {
    const lookups = this.riskLookups();
    const selectedTypeId = this.newRisk.riskTypeId;

    if (!lookups || !selectedTypeId) {
      return lookups?.responseTypes || [];
    }

    const selectedType = lookups.riskTypes.find(t => t.id === selectedTypeId);
    if (!selectedType) {
      return lookups.responseTypes;
    }

    // Filter response types based on risk type (Threat vs Opportunity)
    if (selectedType.code === 'THREAT') {
      return lookups.responseTypes.filter(rt => rt.validForThreat);
    } else if (selectedType.code === 'OPPORTUNITY') {
      return lookups.responseTypes.filter(rt => rt.validForOpportunity);
    }

    return lookups.responseTypes;
  });

  /**
   * @description Computed: check if response type is mandatory based on selected risk type
   * @since 2.0.0
   */
  readonly isResponseTypeMandatory = computed(() => {
    const lookups = this.riskLookups();
    const selectedTypeId = this.newRisk.riskTypeId;

    if (!lookups || !selectedTypeId) {
      return false;
    }

    const selectedType = lookups.riskTypes.find(t => t.id === selectedTypeId);
    return selectedType?.isResponseTypeMandatory || false;
  });

  /**
   * @description LocalStorage key for dismissed recommendations (per opportunity)
   * @since 2.0.0
   */
  private readonly DISMISSED_RECOMMENDATIONS_KEY = 'opportunity_dismissed_recommendations';

  /**
   * @description Signal for tracking dismissed recommendation stable identifiers (persisted to localStorage)
   * Uses oupQuestionId for predefined risks, stableIdentifier for others
   * @type {WritableSignal<Set<string>>}
   * @since 2.0.0
   */
  readonly dismissedRecommendations = signal<Set<string>>(new Set());

  /**
   * @description Get dismissed oupQuestionIds for sending to backend
   * @type {Signal<number[]>}
   * @since 2.0.0
   */
  readonly dismissedOupQuestionIds = computed(() => {
    const dismissed = this.dismissedRecommendations();
    return Array.from(dismissed)
      .filter(id => id.startsWith('oup_'))
      .map(id => parseInt(id.replace('oup_', ''), 10))
      .filter(id => !isNaN(id));
  });

  /**
   * @description Filtered recommendations (excluding dismissed ones and existing risks)
   * Note: Backend already filters, this is a safety net for client-side filtering
   * @type {Signal<AIRiskRecommendation[]>}
   * @since 2.0.0
   */
  readonly visibleRecommendations = computed(() => {
    const recommendations = this.recommendations();
    const dismissed = this.dismissedRecommendations();
    const existingRisks = this.risks();

    if (!recommendations) return [];

    // Get existing risk titles for deduplication (safety net)
    const existingTitles = new Set(
      existingRisks.map(r => r.title.toLowerCase().trim())
    );

    return recommendations.filter((rec: AIRiskRecommendation) => {
      // Exclude dismissed recommendations (by stable identifier)
      const stableId = this.getStableIdentifier(rec);
      if (dismissed.has(stableId)) {
        return false;
      }

      // Exclude recommendations that match existing risk titles (safety net)
      if (existingTitles.has(rec.title.toLowerCase().trim())) {
        return false;
      }

      return true;
    });
  });

  /**
   * @description Get stable identifier for a recommendation
   * Uses oupQuestionId for predefined risks, sourceRiskId for vector store, fallback to title hash
   * @param rec - The recommendation
   * @returns Stable identifier string
   * @since 2.0.0
   */
  private getStableIdentifier(rec: AIRiskRecommendation): string {
    // Use server-provided stable identifier if available
    if (rec.stableIdentifier) {
      return rec.stableIdentifier;
    }
    // Fallback: construct it ourselves
    if (rec.oupQuestionId) {
      return `oup_${rec.oupQuestionId}`;
    }
    if (rec.sourceRiskId) {
      return `vs_${rec.sourceRiskId}`;
    }
    // Last resort: hash the title
    const normalizedTitle = rec.title.toLowerCase().trim().substring(0, 50);
    return `hash_${btoa(normalizedTitle).replace(/[^a-zA-Z0-9]/g, '')}`;
  }

  /**
   * @description Check if DST analysis data is available
   * @type {Signal<boolean>}
   * @since 1.0.0
   */
  readonly hasDSTAnalysis = computed(() => {
    const opp = this.opportunity();
    return !!(opp && opp.dstAnalysis);
  });

  /**
   * @description Constructor - Setup effect to watch for opportunity changes
   * @since 1.0.0
   */
  constructor() {
    // Load dismissed recommendations from localStorage
    this.loadDismissedFromStorage();

    // Load risk lookups and categories once (they don't change per opportunity)
    this.loadRiskLookups();
    this.loadRiskCategories();

    // Effect to load data when opportunity ID changes
    effect(() => {
      const opp = this.opportunity();

      // Only load if we have a valid opportunity and it's different from the last loaded one
      if (opp && opp.id && opp.id !== this.lastLoadedOpportunityId) {
        console.log('🔄 DST Section: Opportunity changed, loading DST data for ID:', opp.id);
        this.lastLoadedOpportunityId = opp.id;

        // Load all DST data for the new opportunity
        this.loadDSTRisks();
        this.loadDSTRecommendations();
        this.loadSimilarOpportunities();
        this.loadSimilarProjects();
        this.loadRelevantPeople();
      }
    });
  }

  /**
   * @description Load dismissed recommendations from localStorage
   * @since 2.0.0
   */
  private loadDismissedFromStorage(): void {
    try {
      const stored = localStorage.getItem(this.DISMISSED_RECOMMENDATIONS_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) {
          this.dismissedRecommendations.set(new Set(parsed));
        }
      }
    } catch (e) {
      console.warn('Failed to load dismissed recommendations from storage:', e);
    }
  }

  /**
   * @description Save dismissed recommendations to localStorage
   * @since 2.0.0
   */
  private saveDismissedToStorage(): void {
    try {
      const dismissed = Array.from(this.dismissedRecommendations());
      localStorage.setItem(this.DISMISSED_RECOMMENDATIONS_KEY, JSON.stringify(dismissed));
    } catch (e) {
      console.warn('Failed to save dismissed recommendations to storage:', e);
    }
  }

  /**
   * @description Load risk lookups (types, probabilities, proximities, impact levels, response types)
   * @since 2.0.0
   */
  loadRiskLookups(): void {
    this.loadingRiskLookups.set(true);

    this.opportunityService.getRiskLookups().subscribe({
      next: (response: RiskLookupsResponse) => {
        this.loadingRiskLookups.set(false);
        this.riskLookups.set(response);
        console.log('📚 Risk lookups loaded:', response);
      },
      error: (error: Error) => {
        this.loadingRiskLookups.set(false);
        console.error('Error loading risk lookups:', error);
      }
    });
  }

  /**
   * @description Load risk categories (hierarchical)
   * @since 2.0.0
   */
  loadRiskCategories(): void {
    this.opportunityService.getRiskCategories().subscribe({
      next: (response: RiskCategoryHierarchyResponse) => {
        this.riskCategories.set(response);
        console.log('📁 Risk categories loaded:', response);
      },
      error: (error: Error) => {
        console.error('Error loading risk categories:', error);
      }
    });
  }

  /**
   * @description Convert risk categories to TreeNode format for p-treeselect
   * Only Level 3 (leaf) categories are selectable
   * @param categories The hierarchical category models
   * @returns TreeNode array compatible with PrimeNG TreeSelect
   * @since 2.0.0
   */
  private convertCategoriesToTreeNodes(categories: RiskCategoryModel[]): TreeNode[] {
    return categories.map(category => this.categoryToTreeNode(category));
  }

  /**
   * @description Convert a single category to TreeNode recursively
   * @param category The category model
   * @returns TreeNode for PrimeNG TreeSelect
   * @since 2.0.0
   */
  private categoryToTreeNode(category: RiskCategoryModel): TreeNode {
    const node: TreeNode = {
      key: category.id.toString(),
      label: category.name,
      data: category,
      selectable: category.isSelectable, // Only Level 3 is selectable
      children: category.children?.length
        ? category.children.map(child => this.categoryToTreeNode(child))
        : undefined
    };
    return node;
  }

  /**
   * @description Handle category selection from TreeSelect
   * @param event The selection event containing the selected node
   * @since 2.0.0
   */
  onCategorySelect(event: { node: TreeNode }): void {
    if (event.node?.data?.id) {
      this.newRisk.riskCategoryId = event.node.data.id;
      console.log('📁 Category selected:', event.node.data);
    }
  }

  /**
   * @description Find a TreeNode by category ID in the tree hierarchy
   * @param categoryId The category ID to find
   * @returns TreeNode or null if not found
   * @since 2.0.0
   */
  private findCategoryNodeById(categoryId: number): TreeNode | null {
    const nodes = this.categoryTreeNodes();
    return this.searchTreeNodes(nodes, categoryId);
  }

  /**
   * @description Recursively search tree nodes for a category ID
   * @param nodes The nodes to search
   * @param categoryId The category ID to find
   * @returns TreeNode or null if not found
   * @since 2.0.0
   */
  private searchTreeNodes(nodes: TreeNode[], categoryId: number): TreeNode | null {
    for (const node of nodes) {
      if (node.data?.id === categoryId) {
        return node;
      }
      if (node.children?.length) {
        const found = this.searchTreeNodes(node.children, categoryId);
        if (found) return found;
      }
    }
    return null;
  }

  /**
   * @description Get severity class for PrimeNG tag component
   * @param {DSTSeverity} severity - The severity level
   * @returns {string} PrimeNG severity class
   * @since 1.0.0
   */
  getSeverityClass(severity: DSTSeverity): 'danger' | 'warning' | 'success' {
    switch (severity) {
      case 'High':
        return 'danger';
      case 'Medium':
        return 'warning';
      case 'Low':
        return 'success';
      default:
        return 'warning';
    }
  }

  /**
   * @description Format currency for display
   * @param {number} amount - The amount to format
   * @returns {string} Formatted currency string
   * @since 1.0.0
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  /**
   * @description Handle refresh analysis action
   * Placeholder for future AI analysis refresh functionality
   * @since 1.0.0
   */
  refreshAnalysis(): void {
    // TODO: Implement AI analysis refresh when backend is ready
    console.log('Refresh DST Analysis');
  }

  /**
   * @description Handle add risk to register action - opens dialog
   * @since 1.0.0
   */
  addToRiskRegister(): void {
    this.resetNewRiskForm();
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;
  }

  /**
   * @description Cancel add risk dialog
   * @since 1.0.0
   */
  cancelAddRisk(): void {
    this.showAddRiskDialog = false;
    this.resetNewRiskForm();
    this.showDialogValidationError.set(false);
  }

  /**
   * @description Reset new risk form to defaults
   * @since 2.0.0
   */
  private resetNewRiskForm(): void {
    this.newRisk = {
      title: '',
      description: '',
      recommendation: '',
      riskTypeId: null,
      riskCategoryId: null,
      riskProbabilityId: null,
      riskProximityId: null,
      riskImpactLevelId: null,
      riskResponseTypeId: null,
      impact: 2
    };
    this.selectedCategoryNode = null;
  }

  /**
   * @description Validate new risk form fields
   * @returns {boolean} True if form is valid
   * @since 2.0.0
   */
  private validateNewRiskForm(): boolean {
    // Check mandatory fields
    if (!this.newRisk.title) return false;
    if (!this.newRisk.riskTypeId) return false;
    if (!this.newRisk.riskCategoryId) return false;
    if (!this.newRisk.riskProbabilityId) return false;
    if (!this.newRisk.riskProximityId) return false;
    if (!this.newRisk.riskImpactLevelId) return false;

    // Check conditional mandatory field (response type)
    if (this.isResponseTypeMandatory() && !this.newRisk.riskResponseTypeId) {
      return false;
    }

    return true;
  }

  /**
   * @description Confirm and save new risk
   * @since 1.0.0
   */
  confirmAddRisk(): void {
    // Validate required fields
    if (!this.validateNewRiskForm()) {
      this.showDialogValidationError.set(true);
      return;
    }

    this.isProcessingRisk.set(true);

    const request: RiskCreateRequest = {
      entityId: this.opportunity().id,
      title: this.newRisk.title,
      riskTypeId: this.newRisk.riskTypeId!,
      riskCategoryId: this.newRisk.riskCategoryId!,
      riskProbabilityId: this.newRisk.riskProbabilityId!,
      riskProximityId: this.newRisk.riskProximityId!,
      riskImpactLevelId: this.newRisk.riskImpactLevelId!,
      riskResponseTypeId: this.newRisk.riskResponseTypeId,
      description: this.newRisk.description || undefined,
      recommendation: this.newRisk.recommendation || undefined,
      impact: this.newRisk.impact
    };

    this.opportunityService.addDSTRisk(this.opportunity().id, request).subscribe({
      next: (createdRisk: Risk) => {
        this.isProcessingRisk.set(false);
        this.showAddRiskDialog = false;
        this.showDialogValidationError.set(false);

        // Add the new risk to the list
        this.risks.update(risks => [...risks, createdRisk]);

        this.feedbackService.showSuccessToast({
          summary: 'Success',
          detail: 'Risk added to register successfully'
        });

        // Reset form
        this.resetNewRiskForm();
      }
    });
  }

  /**
   * @description Show confirmation dialog before deleting a risk
   * @param {Risk} risk - The risk to delete
   * @since 2.0.0
   */
  confirmDeleteRisk(risk: Risk): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: 'Delete Risk',
        detail: `Are you sure you want to delete the risk "${risk.title}"? This action cannot be undone.`
      },
      () => {
        this.deleteRisk(risk);
      }
    );
  }

  /**
   * @description Delete a risk from the risk register
   * @param {Risk} risk - The risk to delete
   * @since 2.0.0
   */
  private deleteRisk(risk: Risk): void {
    this.isDeletingRisk.set(true);

    this.opportunityService.deleteDSTRisk(this.opportunity().id, risk.id).subscribe({
      next: () => {
        this.isDeletingRisk.set(false);

        // Remove the risk from the list
        this.risks.update(risks => risks.filter(r => r.id !== risk.id));

        this.feedbackService.showSuccessToast({
          summary: 'Success',
          detail: 'Risk deleted successfully'
        });

        console.log(`🗑️ [DST] Deleted risk: ${risk.title} (ID: ${risk.id})`);
      },
      error: (error: Error) => {
        this.isDeletingRisk.set(false);
        console.error('❌ [DST] Failed to delete risk:', error);
        // Global error handler will show toast
      }
    });
  }

  /**
   * @description Handle risk type change - clear response type if switching types
   * @since 2.0.0
   */
  onRiskTypeChange(): void {
    // Clear response type when changing risk type (different options available)
    this.newRisk.riskResponseTypeId = null;
  }

  /**
   * @description Load DST risks from the register
   * @since 1.0.0
   */
  loadDSTRisks(): void {
    this.loadingRisks.set(true);
    
    this.opportunityService.getDSTRisks(this.opportunity().id).subscribe({
      next: (response) => {
        this.loadingRisks.set(false);
        this.risks.set(response.risks);
      },
      error: (error: any) => {
        this.loadingRisks.set(false);
        console.error('Error loading DST risks:', error);
      }
    });
  }

  /**
   * @description Load AI-generated DST recommendations
   * Sends dismissed oupQuestionIds to backend for server-side filtering
   * Uses caching - backend won't call LLM if prompt data hasn't changed
   * @since 2.0.0
   */
  loadDSTRecommendations(): void {
    this.loadingRecommendations.set(true);
    
    // Get dismissed oupQuestionIds to pass to backend
    const dismissedIds = this.dismissedOupQuestionIds();
    console.log('📋 [DST] Loading recommendations with dismissed IDs:', dismissedIds);
    
    this.opportunityService.getDSTRecommendations(this.opportunity().id, dismissedIds).subscribe({
      next: (response) => {
        this.loadingRecommendations.set(false);
        this.recommendations.set(response.recommendations);
        console.log(`✅ [DST] Loaded ${response.recommendations.length} recommendations`);
      },
      error: (error: any) => {
        this.loadingRecommendations.set(false);
        console.error('Error loading DST recommendations:', error);
      }
    });
  }

  /**
   * @description Get severity class based on impact level
   * @param {number} impact - Impact level (1=Low, 2=Medium, 3=High)
   * @returns {string} PrimeNG severity class
   * @since 1.0.0
   */
  getImpactSeverity(impact: number): 'success' | 'warn' | 'danger' {
    if (impact === 1) return 'success'; // Low
    if (impact === 2) return 'warn';    // Medium
    return 'danger'; // High
  }

  /**
   * @description Get impact label text
   * @param {number} impact - Impact level (1=Low, 2=Medium, 3=High)
   * @returns {string} Impact label
   * @since 1.0.0
   */
  getImpactLabel(impact: number): string {
    const option = this.impactOptions.find(opt => opt.value === impact);
    return option ? option.label : 'Unknown';
  }

  /**
   * @description Add an AI recommendation to the risk register
   * @param {AIRiskRecommendation} recommendation - The recommendation to add
   * @since 1.0.0
   */
  addRecommendationToRegister(recommendation: AIRiskRecommendation): void {
    // Pre-fill the dialog with recommendation data
    this.newRisk = {
      title: recommendation.title,
      description: recommendation.description,
      recommendation: recommendation.recommendation,
      riskTypeId: null,
      riskCategoryId: null,
      riskProbabilityId: null,
      riskProximityId: null,
      riskImpactLevelId: null,
      riskResponseTypeId: null,
      impact: 2
    };
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;
  }

  /**
   * @description Get display label for risk impact level
   * @param {Risk} risk - The risk to get impact label for
   * @returns {string} Impact level display name
   * @since 2.0.0
   */
  getRiskImpactDisplay(risk: Risk): string {
    return risk.riskImpactLevelName || this.getImpactLabel(risk.impact);
  }

  /**
   * @description Get display label for risk type
   * @param {Risk} risk - The risk to get type label for
   * @returns {string} Risk type display name
   * @since 2.0.0
   */
  getRiskTypeDisplay(risk: Risk): string {
    return risk.riskTypeName || 'Unknown';
  }

  /**
   * @description Get display label for risk probability
   * @param {Risk} risk - The risk to get probability label for
   * @returns {string} Probability display name
   * @since 2.0.0
   */
  getRiskProbabilityDisplay(risk: Risk): string {
    return risk.riskProbabilityName || 'N/A';
  }

  /**
   * @description Get display label for risk proximity
   * @param {Risk} risk - The risk to get proximity label for
   * @returns {string} Proximity display name
   * @since 2.0.0
   */
  getRiskProximityDisplay(risk: Risk): string {
    return risk.riskProximityName || 'N/A';
  }

  /**
   * @description Get display label for risk category (full path)
   * @param {Risk} risk - The risk to get category label for
   * @returns {string} Category full path
   * @since 2.0.0
   */
  getRiskCategoryDisplay(risk: Risk): string {
    return risk.riskCategoryFullPath || risk.riskCategoryName || 'N/A';
  }

  /**
   * @description Get severity class based on impact level ID or legacy impact
   * @param {Risk} risk - The risk to get severity for
   * @returns {string} PrimeNG severity class
   * @since 2.0.0
   */
  getRiskSeverity(risk: Risk): 'success' | 'warn' | 'danger' {
    // Use new impact level if available
    if (risk.riskImpactLevelName) {
      const name = risk.riskImpactLevelName.toLowerCase();
      if (name.includes('very high') || name.includes('high')) return 'danger';
      if (name.includes('medium')) return 'warn';
      return 'success'; // Low or Very Low
    }

    // Fallback to legacy impact
    return this.getImpactSeverity(risk.impact);
  }

  /**
   * @description Accept a recommendation and add it to the risk register
   * For predefined high risks, applies defaults: THREAT, HIGH probability/impact, WITHIN_ONE_MONTH, REDUCE
   * @param {AIRiskRecommendation} recommendation - The recommendation to accept
   * @since 2.0.0
   */
  acceptRecommendation(recommendation: AIRiskRecommendation): void {
    const lookups = this.riskLookups();
    
    // Check if this is a predefined high risk (has oupQuestionId)
    const isPredefinedHighRisk = recommendation.sourceType === 'PREDEFINED_HIGH_RISK' && recommendation.oupQuestionId;
    
    if (isPredefinedHighRisk && lookups) {
      // Apply defaults for predefined high risks
      const threatType = lookups.riskTypes.find((t: RiskTypeModel) => t.code === 'THREAT');
      const highProbability = lookups.probabilities.find((p: RiskProbabilityModel) => p.code === 'HIGH');
      const highImpact = lookups.impactLevels.find((i: RiskImpactLevelModel) => i.code === 'HIGH');
      const withinOneMonth = lookups.proximities.find((p: RiskProximityModel) => p.code === 'WITHIN_ONE_MONTH');
      const reduceResponse = lookups.responseTypes.find((r: RiskResponseTypeModel) => r.code === 'REDUCE');
      
      this.newRisk = {
        title: recommendation.title,
        description: recommendation.description,
        recommendation: recommendation.recommendation,
        riskTypeId: threatType?.id ?? null,
        riskCategoryId: recommendation.riskCategoryId ?? null, // From PreDefinedHighRisk
        riskProbabilityId: highProbability?.id ?? null,
        riskProximityId: withinOneMonth?.id ?? null,
        riskImpactLevelId: highImpact?.id ?? null,
        riskResponseTypeId: reduceResponse?.id ?? null,
        preDefinedHighRiskId: recommendation.preDefinedHighRiskId ?? null,
        impact: 3 // HIGH for legacy field
      };
      
      console.log('✅ [DST] Applying predefined high risk defaults:', this.newRisk);
      
      // Set the category tree node for the TreeSelect
      if (recommendation.riskCategoryId) {
        this.selectedCategoryNode = this.findCategoryNodeById(recommendation.riskCategoryId);
      }
    } else {
      // For non-predefined risks, let user select all fields
      this.newRisk = {
        title: recommendation.title,
        description: recommendation.description,
        recommendation: recommendation.recommendation,
        riskTypeId: null,
        riskCategoryId: null,
        riskProbabilityId: null,
        riskProximityId: null,
        riskImpactLevelId: null,
        riskResponseTypeId: null,
        impact: 2 // Default to Medium for legacy
      };
      this.selectedCategoryNode = null;
    }
    
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;

    // Also dismiss this recommendation from the list after opening the dialog
    this.dismissRecommendation(recommendation);
  }

  /**
   * @description Dismiss a recommendation from the view (persists to localStorage)
   * Uses stable identifiers (oupQuestionId for predefined, sourceRiskId for vector store)
   * @param {AIRiskRecommendation} recommendation - The recommendation to dismiss
   * @since 2.0.0
   */
  dismissRecommendation(recommendation: AIRiskRecommendation): void {
    const stableId = this.getStableIdentifier(recommendation);
    const dismissed = this.dismissedRecommendations();
    const newDismissed = new Set(dismissed);
    newDismissed.add(stableId);
    this.dismissedRecommendations.set(newDismissed);

    console.log(`🚫 [DST] Dismissed recommendation: ${stableId} (${recommendation.title})`);

    // Persist to localStorage
    this.saveDismissedToStorage();
  }

  /**
   * @description Update the high risk acknowledgement status
   * Persists to the backend when changed
   * @param {boolean} acknowledged - Whether the user has acknowledged the high risks
   * @since 2.0.0
   */
  private updateHighRiskAcknowledgement(acknowledged: boolean): void {
    if (this.isUpdatingAcknowledgement) return;

    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    this.isUpdatingAcknowledgement = true;

    this.opportunityService.acknowledgeHighRisks(opportunityId, acknowledged).subscribe({
      next: () => {
        console.log(`✅ [DST] High risk acknowledgement updated: ${acknowledged}`);
        // Update the local opportunity object
        const opp = this.opportunity();
        if (opp) {
          opp.highRisksAcknowledged = acknowledged;
        }
        this.isUpdatingAcknowledgement = false;
      },
      error: (error: Error) => {
        console.error('❌ [DST] Failed to update high risk acknowledgement:', error);
        this.feedbackService.showErrorToast({
          summary: 'Error',
          detail: 'Failed to save acknowledgement. Please try again.'
        });
        this.isUpdatingAcknowledgement = false;
      }
    });
  }

  /**
   * @description Clear all dismissed recommendations (for testing/debugging)
   * @since 2.0.0
   */
  clearDismissedRecommendations(): void {
    this.dismissedRecommendations.set(new Set());
    localStorage.removeItem(this.DISMISSED_RECOMMENDATIONS_KEY);
    console.log('🗑️ [DST] Cleared all dismissed recommendations');
  }

  /**
   * @description View details of similar opportunity
   * @param {number} opportunityId - The opportunity ID to view
   * @since 1.0.0
   */
  viewSimilarOpportunity(opportunityId: number): void {
    // TODO: Implement navigation to similar opportunity when backend is ready
    console.log('View similar opportunity:', opportunityId);
  }

  /**
   * @description Load similar opportunities using semantic search based on embeddings
   * Uses vector similarity to find opportunities with similar characteristics
   * @since 1.0.0
   */
  loadSimilarOpportunities(): void {
    const opportunityId = this.opportunity().id;

    this.loadingSimilarOpportunities.set(true);
    this.similarOpportunitiesError.set(null);

    this.opportunityService.getSimilarOpportunities(opportunityId, 6).subscribe({
      next: (response: SimilarOpportunitiesResponse) => {
        this.loadingSimilarOpportunities.set(false);
        this.similarOpportunitiesResponse.set(response);
        this.similarOpportunities.set(response.similarOpportunities);

        if (response.similarOpportunities.length === 0) {
          console.log('No similar opportunities found');
        }
      },
      error: (error: any) => {
        this.loadingSimilarOpportunities.set(false);
        const errorMessage = error.error?.error || error.message || 'Failed to load similar opportunities';
        this.similarOpportunitiesError.set(errorMessage);
        console.error('Error loading similar opportunities:', errorMessage);
      }
    });
  }

  /**
   * @description Refresh similar opportunities - clears cache and reloads the data
   * @since 1.0.0
   */
  refreshSimilarOpportunities(): void {
    // Clear existing data and reload
    this.similarOpportunities.set(null);
    this.similarOpportunitiesResponse.set(null);
    this.loadSimilarOpportunities();
  }

  /**
   * @description Load similar projects using AI-powered semantic search
   * Extracts keywords from opportunity context and searches vector store for similar projects
   * @since 1.0.0
   */
  loadSimilarProjects(invalidateCache: boolean = false): void {
    const opportunityId = this.opportunity().id;

    this.loadingSimilarProjects.set(true);
    this.similarProjectsError.set(null);

    this.opportunityService.getSimilarProjects(opportunityId, 6, invalidateCache).subscribe({
      next: (response: SimilarProjectsResponse) => {
        this.loadingSimilarProjects.set(false);
        this.similarProjectsResponse.set(response);
        this.similarProjects.set(response.similarProjects);
        
        // Debug logging to verify IDs are being received
        console.log('📊 [SIMILAR-PROJECTS] Received response:', {
          count: response.similarProjects.length,
          projects: response.similarProjects.map(p => ({
            projectId: p.projectId,
            hasId: !!p.projectId,
            description: p.description?.substring(0, 50)
          }))
        });
      },
      error: (error: any) => {
        this.loadingSimilarProjects.set(false);
        const errorMessage = error.error?.error || error.message || 'Failed to load similar projects';
        this.similarProjectsError.set(errorMessage);
        this.feedbackService.showErrorToast({
          summary: 'Error',
          detail: errorMessage
        });
      }
    });
  }

  /**
   * @description Refresh similar projects - clears cache and reloads the data
   * @since 1.0.0
   */
  refreshSimilarProjects(): void {
    // Clear existing data and reload with cache invalidation
    this.similarProjects.set(null);
    this.similarProjectsResponse.set(null);
    this.loadSimilarProjects(true); // Force cache invalidation
  }

  /**
   * @description Load relevant people from corporate directory using AI-powered semantic search
   * Extracts role keywords from opportunity context and searches vector store for relevant people
   * @since 1.0.0
   */
  loadRelevantPeople(invalidateCache: boolean = false): void {
    const opportunityId = this.opportunity().id;

    this.loadingRelevantPeople.set(true);
    this.relevantPeopleError.set(null);

    this.opportunityService.getRelevantPeople(opportunityId, 6, invalidateCache).subscribe({
      next: (response: RelevantPeopleResponse) => {
        this.loadingRelevantPeople.set(false);
        this.relevantPeopleResponse.set(response);
        this.relevantPeople.set(response.relevantPeople);
        
        // Debug logging to verify IDs are being received
        console.log('👥 [RELEVANT-PEOPLE] Received response:', {
          count: response.relevantPeople.length,
          people: response.relevantPeople.map(p => ({
            personId: p.personId,
            hasId: !!p.personId,
            name: p.name
          }))
        });
      },
      error: (error: any) => {
        this.loadingRelevantPeople.set(false);
        const errorMessage = error.error?.error || error.message || 'Failed to load relevant people';
        this.relevantPeopleError.set(errorMessage);
        this.feedbackService.showErrorToast({
          summary: 'Error',
          detail: errorMessage
        });
      }
    });
  }

  /**
   * @description Refresh relevant people - clears cache and reloads the data
   * @since 1.0.0
   */
  refreshRelevantPeople(): void {
    // Clear existing data and reload with cache invalidation
    this.relevantPeople.set(null);
    this.relevantPeopleResponse.set(null);
    this.loadRelevantPeople(true); // Force cache invalidation
  }

  /**
   * @description Get initials from person's name for avatar
   * @param {string | null} name - Person's full name
   * @returns {string} Initials (max 2 characters) or '?' if no name
   * @since 1.0.0
   */
  getInitials(name: string | null): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  /**
   * @description Get badge severity based on relevance score
   * @param {number} score - Relevance score (0-100)
   * @returns {string} PrimeNG severity class
   * @since 1.0.0
   */
  getRelevanceSeverity(score: number): 'success' | 'info' | 'warn' {
    if (score >= 80) return 'success';
    if (score >= 60) return 'info';
    return 'warn';
  }

  /**
   * @description Navigate to opportunity details in a new tab
   * @param {number} opportunityId - The opportunity ID to navigate to
   * @since 1.0.0
   */
  navigateToOpportunity(opportunityId: number): void {
    const url = `/#/partnerships/opportunities/${opportunityId}`;
    window.open(url, '_blank');
  }

  /**
   * @description Format budget amount as USD currency
   * @param {number | null} amount - The budget amount
   * @returns {string} Formatted currency string
   * @since 1.0.0
   */
  formatBudget(amount: number | null): string {
    if (amount == null) return 'N/A';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  }

  /**
   * @description View project details (navigates to external project view or shows details)
   * @param {string} projectId - The project ID to view
   * @since 1.0.0
   */
  viewProjectDetails(projectId: string): void {
    // TODO: Implement navigation to project details view (likely external link to oneUNOPS)
    console.log('View project details:', projectId);
    this.feedbackService.showInfoToast({
      summary: 'Project Details',
      detail: `Project ID: ${projectId}. External navigation will be implemented.`
    });
  }

  /**
   * @description Open project URL in new tab
   * @param {string | null | undefined} url - The project URL to open
   * @since 1.0.0
   */
  openProjectUrl(url: string | null | undefined): void {
    if (url) {
      window.open(url, '_blank');
    }
  }

  /**
   * @description Format partners list - show first partner and +n if more
   * @param {string} partners - Comma-separated partners list
   * @returns {string} Formatted partners string
   * @since 1.0.0
   */
  formatPartners(partners: string): string {
    const partnerList = partners.split(',').map(p => p.trim()).filter(p => p);
    if (partnerList.length === 0) return '';
    if (partnerList.length === 1) return partnerList[0];
    return `${partnerList[0]} +${partnerList.length - 1}`;
  }

  /**
   * @description Format countries list - show first country and +n if more
   * @param {string} countries - Comma-separated countries list
   * @returns {string} Formatted countries string
   * @since 1.0.0
   */
  formatCountries(countries: string): string {
    const countryList = countries.split(',').map(c => c.trim()).filter(c => c);
    if (countryList.length === 0) return '';
    if (countryList.length === 1) return countryList[0];
    return `${countryList[0]} +${countryList.length - 1}`;
  }
}

