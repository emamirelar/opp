/**
 * @fileoverview Opportunity View Component - Unified Dashboard View
 * @author UNOPS Opportunity+ System Development Team
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed, ViewChild, ElementRef, AfterViewInit, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

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

// Services
import { FeedbackDialogService } from '@shared/services/ui';
import { PermissionUtilityService } from '@core/services/auth';
import { PageContextService } from '@shared/services/utils';
import { OpportunityService } from '../../../services/opportunity.service';
import { Opportunity } from '@shared/models/opportunity.model';
import { OpportunityCollaborationComponent } from './sections/collaboration/opportunity-collaboration.component';
import { OpportunityAnalysisSectionComponent } from './sections/analysis/opportunity-analysis-section.component';
import { OpportunityWhatSectionComponent } from './sections/what/opportunity-what-section.component';
import { OpportunityWhySectionComponent } from './sections/why/opportunity-why-section.component';
import { OpportunityWhoSectionComponent } from './sections/who/opportunity-who-section.component';
import { OpportunityWhereSectionComponent } from './sections/where/opportunity-where-section.component';
import { OpportunityWhenSectionComponent } from './sections/when/opportunity-when-section.component';
import { OpportunityDstSectionComponent } from './sections/dst/opportunity-dst-section.component';
import { OpportunityRelatedItemsComponent } from './sections/related/opportunity-related-items.component';
import { OpportunityDocumentsComponent } from './sections/document/opportunity-documents.component';
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
    FormsModule,
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
    OpportunityCollaborationComponent,
    OpportunityAnalysisSectionComponent,
    OpportunityWhatSectionComponent,
    OpportunityWhySectionComponent,
    OpportunityWhoSectionComponent,
    OpportunityWhereSectionComponent,
    OpportunityWhenSectionComponent,
    OpportunityDstSectionComponent,
    OpportunityRelatedItemsComponent,
    OpportunityDocumentsComponent,
  ],
  templateUrl: './opportunity-view.component.html',
  styleUrls: ['./opportunity-view.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [ConfirmationService]
})
export class OpportunityViewComponent implements OnInit, AfterViewInit, OnDestroy {
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
  documentsCollapsed = signal(true); // Document panel state
  activeSection = signal<string>(''); // Active section for navigation - will be set from route params
  showDropdown = signal(false); // Navigation dropdown vs chips
  selectedSection: { id: string; label: string; icon: string } | null = null;

  @ViewChild('navigationSizer', { read: ElementRef })
  navigationSizer?: ElementRef;
  @ViewChild('chipsContainer', { read: ElementRef })
  chipsContainer?: ElementRef;
  @ViewChild('relatedItemsComponent')
  relatedItemsComponent?: OpportunityRelatedItemsComponent;

  private checkTimeout?: number;
  private resizeObserver?: ResizeObserver;
  private intersectionObserver?: IntersectionObserver;
  private chipsRequiredWidth: number = 0;
  private hasInitialized = false;
  private hasScrollSpyInitialized = false; // Track scroll spy initialization separately
  private navigationInProgress = false;
  private isScrolling = false; // Flag to prevent URL updates during programmatic scrolling
  private scrollTimeout?: number; // Debounce timeout for scroll spy
  private lastManualNavigationTime: number = 0; // Track last manual navigation

  // Section navigation configuration
  sections = [
    { id: 'analysis', label: 'Analysis', icon: 'pi-chart-bar' },
    { id: 'what', label: 'What', icon: 'pi-briefcase' },
    { id: 'why', label: 'Why', icon: 'pi-lightbulb' },
    { id: 'who', label: 'Who', icon: 'pi-users' },
    { id: 'where', label: 'Where', icon: 'pi-globe' },
    { id: 'when', label: 'When', icon: 'pi-calendar' },
    { id: 'dst', label: 'DST', icon: 'pi-chart-line' },
    { id: 'related', label: 'Related', icon: 'pi-link' },
    { id: 'collaboration', label: 'Comments', icon: 'pi-comments' },
  ];

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

  // AI Suggestions state and filtering
  allSuggestions = signal<any[]>([]);
  
  // Computed suggestions filtered by section
  whoSuggestions = computed(() => 
    this.allSuggestions().filter(s => s.actionTarget === 'WHO')
  );
  
  whereSuggestions = computed(() => 
    this.allSuggestions().filter(s => s.actionTarget === 'WHERE')
  );
  
  whatSuggestions = computed(() => 
    this.allSuggestions().filter(s => s.actionTarget === 'WHAT')
  );
  
  whySuggestions = computed(() => 
    this.allSuggestions().filter(s => s.actionTarget === 'WHY')
  );
  
  whenSuggestions = computed(() => 
    this.allSuggestions().filter(s => s.actionTarget === 'WHEN')
  );

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

  documentsChevronIcon = computed(() => {
    return this.documentsCollapsed() ? 'pi-chevron-right' : 'pi-chevron-left';
  });

  constructor() {
    // Effect to setup observers once data is loaded
    effect(() => {
      const isLoaded = !this.loading();

      if (isLoaded) {
        untracked(() => {
          // Initialize navigation
          if (!this.hasInitialized) {
            setTimeout(() => this.initializeNavigation(), 200);
          }
          
          // Initialize scroll spy
          if (!this.hasScrollSpyInitialized) {
            setTimeout(() => this.setupScrollSpy(), 300);
          }
        });
      }
    });
  }

  ngOnInit() {
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);

    // Subscribe to route parameter changes for both recordId and section
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        const newRecordId = paramMap.get("recordId") || '';
        const section = paramMap.get("section");
        
        // Set the section FIRST before any other logic
        // This prevents the "analysis" default from triggering navigation
        if (section && this.isValidSection(section)) {
          this.activeSection.set(section);
          this.updateSelectedSection();
          // Set manual navigation time to prevent scroll spy interference
          this.lastManualNavigationTime = Date.now();
          this.isScrolling = true;
        } else if (!section && !this.activeSection()) {
          // Only default to analysis if no section provided AND no section already set
          this.activeSection.set('analysis');
          this.updateSelectedSection();
        }
        
        // Only reload data if recordId actually changed
        if (newRecordId && newRecordId !== this.recordId) {
          this.recordId = newRecordId;
          this._loadRecordDetails(section || 'analysis');
        } else if (newRecordId && !this.recordId) {
          // Initial load
          this.recordId = newRecordId;
          this._loadRecordDetails(section || 'analysis');
        }

        // Handle section scrolling (without reloading data)
        if (section && this.isValidSection(section)) {
          // If data is already loaded, scroll immediately
          if (!this.loading()) {
            setTimeout(() => this.scrollToSectionInternal(section), 100);
          }
        } else if (!section && !this.activeSection()) {
          // Default to analysis if no section in URL and no section already set
          const currentUrl = this.router.url.split('?')[0];
          // Only navigate if we're not already at analysis
          if (!currentUrl.endsWith('/analysis')) {
            this.router.navigate([currentUrl, 'analysis'], { replaceUrl: true });
          }
        }
      }
    });
  }

  ngAfterViewInit(): void {
    // Observers will be set up via effect after data loads
  }

  ngOnDestroy(): void {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();
    
    // Cleanup navigation observer
    if (this.checkTimeout) {
      clearTimeout(this.checkTimeout);
    }
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
    // Cleanup scroll spy observer
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
    // Cleanup scroll timeout
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
    }
  }

  /**
   * Load opportunity record details
   */
  private _loadRecordDetails(targetSection?: string) {
    this.loading.set(true);
    this.opportunityService.getOpportunityById(+this.recordId).subscribe({
      next: (data: Opportunity) => {
        this.opportunity.set(data);
        this.loading.set(false);
        this.cdr.detectChanges();
        
        // Load AI suggestions for sections
        this._loadSuggestions();
        
        // If a target section was specified, scroll to it after data loads
        if (targetSection && this.isValidSection(targetSection)) {
          setTimeout(() => {
            this.scrollToSectionInternal(targetSection);
          }, 500); // Longer delay to ensure DOM is fully rendered
        }
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
   * Load AI suggestions for the opportunity
   */
  private _loadSuggestions(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    this.opportunityService.getInsights(opportunityId).subscribe({
      next: (response) => {
        // Extract suggestions and set them
        this.allSuggestions.set(response.suggestions || []);
      },
      error: (error) => {
        console.error('Error loading suggestions:', error);
        // Silent failure - suggestions are optional enhancement
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
    
    // Refresh related items to reflect any changes in partners/stakeholders
    if (this.relatedItemsComponent) {
      this.relatedItemsComponent.loadSourceInteractions();
    }
    
    // Angular signals automatically notify ALL child components
    // All sections will re-render with latest data
    this.cdr.detectChanges();
  }

  /**
   * @description Reload opportunity data from API (e.g., after AI changes)
   * @returns {void}
   */
  reloadOpportunity(): void {
    console.log('🔄 Reloading opportunity data...');
    if (this.recordId) {
      // Get the current active section to maintain scroll position
      const currentSection = this.activeSection();
      this._loadRecordDetails(currentSection);
    }
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

  /**
   * Initialize navigation resize observer
   */
  private initializeNavigation(): void {
    if (this.hasInitialized) {
      return;
    }

    if (!this.navigationSizer || !this.chipsContainer) {
      return;
    }

    // Step 1: Measure required width for chips (ONCE, while in chips mode)
    this.showDropdown.set(false); // Ensure we're in chips mode
    setTimeout(() => {
      if (!this.chipsContainer) return;

      this.chipsRequiredWidth = this.chipsContainer.nativeElement.scrollWidth;

      // Step 2: Setup ResizeObserver ONLY on the container (not the chips)
      this.resizeObserver = new ResizeObserver(() => {
        this.checkNavigationFit();
      });

      this.resizeObserver.observe(this.navigationSizer!.nativeElement);

      this.hasInitialized = true;

      // Initial check
      this.checkNavigationFit();
    }, 100);
  }

  /**
   * Check if navigation fits and switch between chips and dropdown
   */
  private checkNavigationFit(): void {
    // Debounce checks
    if (this.checkTimeout) {
      clearTimeout(this.checkTimeout);
    }

    this.checkTimeout = window.setTimeout(() => {
      if (!this.navigationSizer || this.chipsRequiredWidth === 0) {
        return;
      }

      // Get current container width
      const containerWidth = this.navigationSizer.nativeElement.clientWidth;

      // Compare stored chips width vs container width
      const needsDropdown = this.chipsRequiredWidth > containerWidth - 20; // 20px buffer

      // Only update if different
      if (needsDropdown !== this.showDropdown()) {
        this.showDropdown.set(needsDropdown);
      }
    }, 100);
  }

  /**
   * Update selected section for dropdown
   */
  private updateSelectedSection(): void {
    this.selectedSection =
      this.sections.find((section) => section.id === this.activeSection()) ||
      null;
  }

  /**
   * Handle section dropdown change
   */
  onSectionDropdownChange(event: any): void {
    if (event.value) {
      this.activeSection.set(event.value.id);
      this.scrollToSection(event.value.id);
    }
  }

  /**
   * Get active section
   */
  getActiveSection(): { id: string; label: string; icon: string } | null {
    return (
      this.sections.find((section) => section.id === this.activeSection()) ||
      null
    );
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

    // Update URL with the section parameter
    const currentUrl = this.router.url;
    const urlSegments = currentUrl.split('/');
    
    // Check if we already have a section in the URL
    const lastSegment = urlSegments[urlSegments.length - 1];
    const isSection = this.isValidSection(lastSegment);
    
    if (isSection) {
      // Replace existing section
      this.router.navigate([...urlSegments.slice(0, -1), sectionId], { replaceUrl: true });
    } else {
      // Add section to URL
      this.router.navigate([...urlSegments, sectionId], { replaceUrl: true });
    }

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
   * Internal scroll to section logic
   * Called for both programmatic navigation and URL-based navigation
   */
  private scrollToSectionInternal(sectionId: string): void {
    // Track manual navigation time to prevent scroll spy interference
    this.lastManualNavigationTime = Date.now();
    this.isScrolling = true;

    // For the first section (analysis), scroll to the opportunity header
    if (sectionId === 'analysis') {
      const headerElement = document.getElementById('opportunity-header');
      if (headerElement) {
        headerElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
      
      // Reset scrolling flag after animation - shorter delay
      setTimeout(() => {
        this.isScrolling = false;
      }, 800); // Reduced from 1500ms to 800ms
      return;
    }

    // For other sections, scroll to the section with offset
    const element = document.getElementById(`section-${sectionId}`);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // Reset scrolling flag after animation - shorter delay
    setTimeout(() => {
      this.isScrolling = false;
    }, 800); // Reduced from 1500ms to 800ms
  }

  /**
   * Validate section ID
   */
  private isValidSection(section: string): boolean {
    return this.sections.some(s => s.id === section);
  }

  /**
   * Setup scroll spy using IntersectionObserver
   * Automatically updates active section and URL as user scrolls
   * Only blocks during programmatic scrolling, works immediately for manual scrolling
   */
  private setupScrollSpy(): void {
    if (this.hasScrollSpyInitialized) {
      return;
    }

    // Create an intersection observer to detect which section is in view
    const observerOptions = {
      root: null, // Use viewport as root
      rootMargin: '-20% 0px -70% 0px', // Trigger when section enters top 30% of viewport
      threshold: 0, // Trigger as soon as any part is visible
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
        // Find the section that's most visible
        let mostVisibleEntry: IntersectionObserverEntry | undefined;
        let maxVisibility = 0;

        entries.forEach((entry) => {
          if (entry.isIntersecting && entry.intersectionRatio > maxVisibility) {
            maxVisibility = entry.intersectionRatio;
            mostVisibleEntry = entry;
          }
        });

        if (mostVisibleEntry && mostVisibleEntry.target instanceof HTMLElement) {
          const sectionId = mostVisibleEntry.target.id.replace('section-', '');
          
          // Only update if it's a different section
          if (sectionId && this.isValidSection(sectionId) && sectionId !== this.activeSection()) {
            this.activeSection.set(sectionId);
            this.updateSelectedSection();

            // Update URL without causing a scroll
            const currentUrl = this.router.url.split('?')[0];
            const urlSegments = currentUrl.split('/');
            const lastSegment = urlSegments[urlSegments.length - 1];
            const isSection = this.isValidSection(lastSegment);

            if (isSection) {
              // Replace existing section in URL
              this.router.navigate([...urlSegments.slice(0, -1), sectionId], { replaceUrl: true });
            } else {
              // Add section to URL
              this.router.navigate([...urlSegments, sectionId], { replaceUrl: true });
            }

            // Trigger change detection
            this.cdr.detectChanges();
          }
        }
      }, 150); // Reduced to 150ms for more responsive updates
    }, observerOptions);

    // Observe all section elements
    let observedCount = 0;
    this.sections.forEach((section) => {
      const element = document.getElementById(`section-${section.id}`);
      if (element) {
        this.intersectionObserver!.observe(element);
        observedCount++;
      }
    });

    this.hasScrollSpyInitialized = true;
    console.log(`Scroll spy initialized and observing ${observedCount} sections`);
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
    const colorMap: { [key: string]: string} = {
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
}

