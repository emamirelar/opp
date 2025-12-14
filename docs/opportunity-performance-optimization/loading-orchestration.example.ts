/**
 * @fileoverview Example Implementation of Loading Orchestration for Opportunity View
 * @author UNOPS Opportunity+ System Development Team
 * 
 * This file demonstrates how to implement the loading orchestration pattern
 * in the opportunity-view.component.ts file.
 */

import { signal, computed, effect } from '@angular/core';
import { 
  LoadingProgress, 
  LoadingSectionKey, 
  LoadingSectionStatus,
  DEFAULT_LOADING_PROGRESS 
} from './loading-progress.interface';

/**
 * Example: Add these properties to OpportunityViewComponent
 */
export class OpportunityViewComponentExample {
  
  // ========================================
  // LOADING PROGRESS STATE
  // ========================================
  
  /**
   * @description Loading progress signal tracking all sections
   */
  readonly loadingProgress = signal<LoadingProgress>(DEFAULT_LOADING_PROGRESS);

  /**
   * @description Computed progress percentage (0-100)
   */
  readonly progressPercentage = computed(() => {
    const progress = this.loadingProgress();
    if (progress.total === 0) return 0;
    return Math.round((progress.completed / progress.total) * 100);
  });

  /**
   * @description Computed progress message for display
   */
  readonly progressMessage = computed(() => {
    const progress = this.loadingProgress();
    if (progress.completed === progress.total) {
      return 'All data loaded successfully';
    }
    return `Loading: ${progress.currentSection} (${progress.completed} of ${progress.total} sections)`;
  });

  /**
   * @description Show progress bar only while loading
   */
  readonly showProgressBar = computed(() => {
    const progress = this.loadingProgress();
    return progress.completed < progress.total;
  });

  /**
   * @description Track if all loading is complete
   */
  readonly allLoadingComplete = computed(() => {
    const progress = this.loadingProgress();
    return progress.completed === progress.total;
  });

  // ========================================
  // CONSTRUCTOR - Setup Effects
  // ========================================
  
  constructor() {
    // Effect to show completion notification
    effect(() => {
      const isComplete = this.allLoadingComplete();
      
      if (isComplete) {
        // Wait a moment, then show success notification
        setTimeout(() => {
          console.log('✅ All sections loaded successfully');
          // Optional: Show toast notification
          // this.feedbackDialogService.showSuccessToast({
          //   summary: 'Loading Complete',
          //   detail: 'All data loaded successfully',
          //   life: 2000
          // });
        }, 500);
      }
    });
  }

  // ========================================
  // LOADING ORCHESTRATION METHODS
  // ========================================

  /**
   * @description Main record loading method with orchestrated section loading
   * @param targetSection Optional section to scroll to after loading
   */
  private _loadRecordDetails(targetSection?: string) {
    this.loading.set(true);
    
    // Reset progress to initial state
    this.resetLoadingProgress();
    
    // STEP 1: Load main opportunity data
    this.updateLoadingProgress('opportunity', 'loading', 'Opportunity Data');
    
    this.opportunityService.getOpportunityById(+this.recordId).subscribe({
      next: (data: Opportunity) => {
        this.opportunity.set(data);
        this.loading.set(false);
        this.updateLoadingProgress('opportunity', 'completed');
        
        // STEP 2: Load insights (required by Analysis section)
        this.updateLoadingProgress('insights', 'loading', 'AI Insights');
        this._loadInsights();
        
        // STEP 3: Generate banner images (background, non-blocking)
        if (data.name && data.description && !data.opportunityBannerImage) {
          this._generateBannerImages(data.id);
        }
        
        // STEP 4: Trigger section data loading in visual order (top to bottom)
        this._orchestrateSectionLoading();
        
        // STEP 5: Handle initial scroll if needed
        if (
          this.shouldScrollAfterDataLoad &&
          targetSection &&
          this.isValidSection(targetSection)
        ) {
          this.pendingScrollTarget = targetSection;
          this.shouldScrollAfterDataLoad = false;
          this.waitForContentAndScroll();
        } else {
          this.isInitialLoad = false;
        }
      },
      error: (error) => {
        console.error('❌ Error loading opportunity details:', error);
        this.loading.set(false);
        this.updateLoadingProgress('opportunity', 'error', 'Opportunity Data', error.message);
        // Error toast handled by global interceptor
      },
    });
  }

  /**
   * @description Load AI insights and suggestions (SINGLE API CALL)
   * Updates progress when complete
   */
  private _loadInsights(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) {
      this.updateLoadingProgress('insights', 'error', 'AI Insights', 'No opportunity ID');
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
        this.updateLoadingProgress('analysis', 'completed', 'Analysis');
        
        console.log('✅ Insights loaded successfully:', {
          insightCount: response.insights?.length || 0,
          suggestionCount: response.suggestions?.length || 0,
        });
      },
      error: (error) => {
        console.error('❌ Error loading insights:', error);
        this.insightsError.set('Failed to load AI insights');
        this.insightsLoading.set(false);
        this.updateLoadingProgress('insights', 'error', 'AI Insights', error.message);
        this.updateLoadingProgress('analysis', 'error', 'Analysis', 'Insights failed to load');
      },
    });
  }

  /**
   * @description Orchestrate section loading in visual order (top to bottom)
   * Uses sequential delays to match section display order and prevent connection exhaustion
   */
  private _orchestrateSectionLoading(): void {
    console.log('🎬 Starting orchestrated section loading...');

    // Analysis section uses insights loaded above - marked complete in _loadInsights()
    
    // DST Section - Risks (immediate)
    setTimeout(() => {
      this.updateLoadingProgress('dstRisks', 'loading', 'Risk Assessment');
      console.log('📊 Loading DST Risks...');
      // DST component will load risks immediately and call onDSTRisksLoaded()
    }, 200);
    
    // DST Section - Recommendations (+500ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstRecommendations', 'loading', 'AI Recommendations');
      console.log('💡 Loading DST Recommendations...');
      // DST component will load recommendations and call onDSTRecommendationsLoaded()
    }, 700);
    
    // DST Section - Similar Opportunities (+1000ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstSimilarOpportunities', 'loading', 'Similar Opportunities');
      console.log('🔍 Loading Similar Opportunities...');
      // DST component will load similar opportunities and call onDSTSimilarOpportunitiesLoaded()
    }, 1200);
    
    // DST Section - Similar Projects (+1500ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstSimilarProjects', 'loading', 'Similar Projects');
      console.log('📁 Loading Similar Projects...');
      // DST component will load similar projects and call onDSTSimilarProjectsLoaded()
    }, 1700);
    
    // DST Section - Relevant People (+2000ms)
    setTimeout(() => {
      this.updateLoadingProgress('dstRelevantPeople', 'loading', 'Relevant People');
      console.log('👥 Loading Relevant People...');
      // DST component will load relevant people and call onDSTRelevantPeopleLoaded()
    }, 2200);
    
    // Related Items Section (+2500ms)
    setTimeout(() => {
      this.updateLoadingProgress('relatedItems', 'loading', 'Related Items');
      console.log('🔗 Loading Related Items...');
      // Related items component will call onRelatedItemsLoaded() when complete
    }, 2700);
    
    // Documents Panel (+3000ms)
    setTimeout(() => {
      this.updateLoadingProgress('documents', 'loading', 'Documents');
      console.log('📄 Loading Documents...');
      // Documents component will call onDocumentsLoaded() when complete
    }, 3200);
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
        endTime: status === 'completed' || status === 'error' ? Date.now() : undefined,
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

  // ========================================
  // CHILD COMPONENT CALLBACKS
  // ========================================
  
  /**
   * @description Called by DST section when risks are loaded
   */
  onDSTRisksLoaded(): void {
    this.updateLoadingProgress('dstRisks', 'completed', 'Risk Assessment');
  }

  /**
   * @description Called by DST section when recommendations are loaded
   */
  onDSTRecommendationsLoaded(): void {
    this.updateLoadingProgress('dstRecommendations', 'completed', 'AI Recommendations');
  }

  /**
   * @description Called by DST section when similar opportunities are loaded
   */
  onDSTSimilarOpportunitiesLoaded(): void {
    this.updateLoadingProgress('dstSimilarOpportunities', 'completed', 'Similar Opportunities');
  }

  /**
   * @description Called by DST section when similar projects are loaded
   */
  onDSTSimilarProjectsLoaded(): void {
    this.updateLoadingProgress('dstSimilarProjects', 'completed', 'Similar Projects');
  }

  /**
   * @description Called by DST section when relevant people are loaded
   */
  onDSTRelevantPeopleLoaded(): void {
    this.updateLoadingProgress('dstRelevantPeople', 'completed', 'Relevant People');
  }

  /**
   * @description Called by related items component when data is loaded
   */
  onRelatedItemsLoaded(): void {
    this.updateLoadingProgress('relatedItems', 'completed', 'Related Items');
  }

  /**
   * @description Called by documents component when data is loaded
   */
  onDocumentsLoaded(): void {
    this.updateLoadingProgress('documents', 'completed', 'Documents');
  }

  /**
   * @description Handle loading errors from child components
   */
  onSectionLoadError(sectionKey: LoadingSectionKey, error: string): void {
    this.updateLoadingProgress(sectionKey, 'error', undefined, error);
  }
}

