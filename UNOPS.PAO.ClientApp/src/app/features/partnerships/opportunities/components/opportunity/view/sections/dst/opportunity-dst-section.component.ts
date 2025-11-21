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
import { FormsModule } from '@angular/forms';

// Models
import { Opportunity, DSTSeverity, SimilarProject, SimilarProjectsResponse, SimilarOpportunity, SimilarOpportunitiesResponse, RelevantPerson, RelevantPeopleResponse, Risk, AIRiskRecommendation, RiskCreateRequest } from '@shared/models/opportunity.model';
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
    TooltipModule
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
   * @description New risk form data
   * @since 1.0.0
   */
  newRisk = {
    title: '',
    description: '',
    recommendation: '',
    impact: 2 // Default to Medium
  };

  /**
   * @description Impact options for dropdown
   * @since 1.0.0
   */
  readonly impactOptions = [
    { label: 'Low', value: 1 },
    { label: 'Medium', value: 2 },
    { label: 'High', value: 3 }
  ];

  /**
   * @description Signal for tracking dismissed recommendation indexes
   * @type {WritableSignal<Set<number>>}
   * @since 1.0.0
   */
  readonly dismissedRecommendations = signal<Set<number>>(new Set());

  /**
   * @description Filtered recommendations (excluding dismissed ones)
   * @type {Signal<AIRiskRecommendation[]>}
   * @since 1.0.0
   */
  readonly visibleRecommendations = computed(() => {
    const recommendations = this.recommendations();
    const dismissed = this.dismissedRecommendations();
    
    if (!recommendations) return [];
    
    return recommendations.filter((_: AIRiskRecommendation, index: number) => !dismissed.has(index));
  });

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
    this.newRisk = {
      title: '',
      description: '',
      recommendation: '',
      impact: 2
    };
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;
  }

  /**
   * @description Cancel add risk dialog
   * @since 1.0.0
   */
  cancelAddRisk(): void {
    this.showAddRiskDialog = false;
    this.newRisk = {
      title: '',
      description: '',
      recommendation: '',
      impact: 2
    };
    this.showDialogValidationError.set(false);
  }

  /**
   * @description Confirm and save new risk
   * @since 1.0.0
   */
  confirmAddRisk(): void {
    // Validate required fields
    if (!this.newRisk.title || !this.newRisk.description || !this.newRisk.recommendation) {
      this.showDialogValidationError.set(true);
      return;
    }

    this.isProcessingRisk.set(true);

    const request: RiskCreateRequest = {
      entityId: this.opportunity().id,
      title: this.newRisk.title,
      description: this.newRisk.description,
      recommendation: this.newRisk.recommendation,
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
        this.newRisk = {
          title: '',
          description: '',
          recommendation: '',
          impact: 2
        };
      },
      error: (error: any) => {
        this.isProcessingRisk.set(false);
        this.feedbackService.showErrorToast({
          summary: 'Error',
          detail: error.error?.error || error.message || 'Failed to add risk'
        });
      }
    });
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
   * @since 1.0.0
   */
  loadDSTRecommendations(): void {
    this.loadingRecommendations.set(true);
    
    this.opportunityService.getDSTRecommendations(this.opportunity().id).subscribe({
      next: (response) => {
        this.loadingRecommendations.set(false);
        this.recommendations.set(response.recommendations);
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
      impact: 2 // Default to Medium, user can change
    };
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;
  }

  /**
   * @description Accept a recommendation and add it to the risk register
   * @param {AIRiskRecommendation} recommendation - The recommendation to accept
   * @param {number} index - The index of the recommendation in the list
   * @since 1.0.0
   */
  acceptRecommendation(recommendation: AIRiskRecommendation, index: number): void {
    // Pre-fill the dialog with recommendation data
    this.newRisk = {
      title: recommendation.title,
      description: recommendation.description,
      recommendation: recommendation.recommendation,
      impact: 2 // Default to Medium, user can change
    };
    this.showDialogValidationError.set(false);
    this.showAddRiskDialog = true;
    
    // Also dismiss this recommendation from the list after opening the dialog
    this.dismissRecommendation(index);
  }

  /**
   * @description Dismiss a recommendation from the view
   * @param {number} index - The index of the recommendation to dismiss
   * @since 1.0.0
   */
  dismissRecommendation(index: number): void {
    const dismissed = this.dismissedRecommendations();
    const newDismissed = new Set(dismissed);
    newDismissed.add(index);
    this.dismissedRecommendations.set(newDismissed);
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
  loadSimilarProjects(): void {
    const opportunityId = this.opportunity().id;

    this.loadingSimilarProjects.set(true);
    this.similarProjectsError.set(null);

    this.opportunityService.getSimilarProjects(opportunityId, 6).subscribe({
      next: (response: SimilarProjectsResponse) => {
        this.loadingSimilarProjects.set(false);
        this.similarProjectsResponse.set(response);
        this.similarProjects.set(response.similarProjects);
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
    // Clear existing data and reload
    this.similarProjects.set(null);
    this.similarProjectsResponse.set(null);
    this.loadSimilarProjects();
  }

  /**
   * @description Load relevant people from corporate directory using AI-powered semantic search
   * Extracts role keywords from opportunity context and searches vector store for relevant people
   * @since 1.0.0
   */
  loadRelevantPeople(): void {
    const opportunityId = this.opportunity().id;

    this.loadingRelevantPeople.set(true);
    this.relevantPeopleError.set(null);

    this.opportunityService.getRelevantPeople(opportunityId, 6).subscribe({
      next: (response: RelevantPeopleResponse) => {
        this.loadingRelevantPeople.set(false);
        this.relevantPeopleResponse.set(response);
        this.relevantPeople.set(response.relevantPeople);
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
    // Clear existing data and reload
    this.relevantPeople.set(null);
    this.relevantPeopleResponse.set(null);
    this.loadRelevantPeople();
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

