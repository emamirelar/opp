import { Component, OnInit, OnDestroy, inject, signal, DestroyRef, HostListener, ElementRef, computed, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { RouterModule, Router } from '@angular/router';
import { ChartModule } from 'primeng/chart';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { forkJoin, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { PartnerService } from '@partnerships/partners/services/partner.service';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { InteractionService } from '@partnerships/interactions/services/interaction.service';
import { GlobalFilterService } from '@core/services/filters';
import { PermissionService } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { LayoutService } from '@layouts/services/layout.service';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerEditDialogComponent } from '@partnerships/partners/components/partner/edit-dialog/partner-edit-dialog.component';
import { PartnerEditDialogFooterComponent } from '@partnerships/partners/components/partner/edit-dialog/footer/partner-edit-dialog-footer.component';
import { ContactEditDialogComponent } from '@partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component';
import { InteractionModalComponent } from '@partnerships/interactions/components/interaction/modal/interaction-modal.component';
import { CreateOpportunityFromInteractionsDialogComponent } from '@partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component';
import { Partner } from '@partnerships/partners/models/partner.model';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { Interaction } from '@partnerships/interactions/models/interaction.model';
import { Opportunity } from '@shared/models/opportunity.model';
import { CreateOpportunityFromInteractionsConfig } from '@partnerships/interactions/models/interaction-selection.model';
import { DashboardCardComponent, DashboardCardConfig, DashboardCardFilter } from '@app/shared/components/data-display/dashboard-card';

interface DashboardData {
  myPartners: Partner[];
  myContacts: Contact[];
  myInteractions: Interaction[];
  myOpportunities: Opportunity[];
  draftActions: {
    partners: Partner[];
    contacts: Contact[];
    interactions: Interaction[];
    opportunities: Opportunity[];
  };
  orgUnitRecentUpdates: RecentUpdate[];
  orgUnitName: string;
}

interface RecentUpdate {
  id: number;
  name: string;
  type: 'Partner' | 'Contact' | 'Interaction' | 'Opportunity';
  lastModifiedDate: string;
  lastModifiedBy: number;
  lastModifiedByName?: string;
  status: string;
  entityData?: any; // Additional entity-specific data
}

interface OrgUnitRecentUpdatesResponse {
  updates: RecentUpdate[];
  orgUnitName: string;
  orgUnitId?: number;
}

interface DashboardSummary {
  totalMyPartners: number;
  totalMyContacts: number;
  totalMyInteractions: number;
  totalMyOpportunities: number;
  totalDraftActions: number;
}

@Component({
  selector: 'app-home-dashboard',
  templateUrl: './home-dashboard.component.html',
  styleUrls: ['./home-dashboard.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    SkeletonModule,
    TagModule,
    TooltipModule,
    RouterModule,
    ChartModule,
    HttpClientModule,
    DashboardCardComponent,
    CreateOpportunityFromInteractionsDialogComponent
  ],
  providers: [DialogService]
})
export class HomeDashboardComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private router = inject(Router);
  private partnerService = inject(PartnerService);
  private contactService = inject(ContactService);
  private interactionService = inject(InteractionService);
  private globalFilterService = inject(GlobalFilterService);
  private destroyRef = inject(DestroyRef);
  private permissionService = inject(PermissionService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private dialogService = inject(DialogService);
  public layoutService = inject(LayoutService);
  private elementRef = inject(ElementRef);

  // Dashboard Card Configurations specific to home dashboard
  private readonly DASHBOARD_CARD_CONFIGS = {
    ACTIONS_REQUIRED: {
      icon: 'priority_high',
      iconColor: 'bg-unops-warning/10',
      title: 'Actions Required',
      subtitle: 'Items need attention',
      size: 'tall',
      emptyStateIcon: 'check_circle',
      emptyStateTitle: 'All caught up!',
      emptyStateMessage: 'No actions require your attention at this time.',
      showFilters: true,
      showViewAll: true,
      viewAllText: 'View All'
    } as DashboardCardConfig,

    RECENT_ACTIVITY: {
      icon: 'history',
      iconColor: 'bg-unops-info/10',
      title: 'Recent Activity',
      subtitle: 'Latest updates',
      size: 'tall',
      emptyStateIcon: 'history',
      emptyStateTitle: 'No recent activity',
      emptyStateMessage: 'No updates to show.',
      showFilters: true,
      showViewAll: true,
      viewAllText: 'View All'
    } as DashboardCardConfig,

    MY_WORKSPACE: {
      icon: 'dashboard',
      iconColor: 'bg-unops-primary/10',
      title: 'My Workspace',
      subtitle: 'Quick access to your data',
      size: 'tall',
      emptyStateIcon: 'dashboard',
      emptyStateTitle: 'No items yet',
      emptyStateMessage: 'No partners or contacts yet',
      showFilters: true,
      showViewAll: true,
      viewAllText: 'View All'
    } as DashboardCardConfig,

    RECENT_INTERACTIONS: {
      icon: 'chat',
      iconColor: 'bg-unops-accent-orange/10',
      title: 'Recent Interactions & Communications',
      subtitle: 'Latest activity',
      size: 'fixed',
      emptyStateIcon: 'chat',
      emptyStateTitle: 'No interactions yet',
      emptyStateMessage: 'Start logging your communications and meetings',
      emptyStateActionLabel: 'Log First Interaction',
      showFilters: false,
      showViewAll: true,
      viewAllText: 'View All'
    } as DashboardCardConfig
  };

  loading = signal(true);
  error = signal<string | null>(null);
  dashboardData = signal<DashboardData | null>(null);
  summary = signal<DashboardSummary>({
    totalMyPartners: 0,
    totalMyContacts: 0,
    totalMyInteractions: 0,
    totalMyOpportunities: 0,
    totalDraftActions: 0
  });

  // Panel expansion state - only one panel can be expanded at a time
  expandedPanel = signal<string | null>(null);

  // Interaction chart filtering
  selectedInteractionType = signal<string | null>(null);
  filteredInteractions = signal<Interaction[]>([]);
  selectedInteractionColor = signal<string | null>(null);

  // Draft actions chart filtering
  selectedDraftActionType = signal<string | null>(null);
  filteredDraftActions = signal<any[]>([]);
  selectedDraftActionColor = signal<string | null>(null);

  // Org Unit Recent Updates filtering
  selectedOrgUnitUpdateType = signal<string | null>(null);

  // Navigation loading state
  navigatingToEntity = signal<string | null>(null);

  // Permission signals
  partnerPermissions = signal<any>({ permissions: { canCreate: false } });
  contactPermissions = signal<any>({ permissions: { canCreate: false } });
  interactionPermissions = signal<any>({ permissions: { canCreate: false } });
  opportunityPermissions = signal<any>({ permissions: { canCreate: false } });
  permissionsLoading = signal(true);

  // Live timestamp
  lastUpdatedTime = signal<string>('just now');
  private timestampInterval?: ReturnType<typeof setInterval>;
  private lastDataLoadTime = new Date();

  // Width tracking using resizer div
  @ViewChild('widthTracker', { static: false }) widthTracker?: ElementRef;
  private widthTrackingInterval?: ReturnType<typeof setInterval>;

  // Mobile detection
  isMobile = signal<boolean>(false);

  // Component width tracking for responsive quick actions
  componentWidth = signal<number>(0);

  // Computed signal to determine if quick actions should use vertical layout
  // This is now based purely on the measured content width
  shouldUseVerticalLayout = computed(() => {
    const screenIsMobile = this.isMobile();
    const componentWidth = this.componentWidth();
    
    // Always use vertical on mobile screens
    if (screenIsMobile) {
      return true;
    }
    
    const threshold = 750; // Horizontal layout needs 750px of content width
    
    // Use vertical layout only when measured content width is below threshold
    return componentWidth > 0 && componentWidth < threshold;
  });

  // Computed signal to determine if dashboard cards should use single-column layout
  // Based on content width, not screen size
  shouldUseCardVerticalLayout = computed(() => {
    const componentWidth = this.componentWidth();
    
    // For cards, we need more space since we're showing 3 columns
    // Use single column layout when width is below 900px
    const threshold = 900; // 3-column layout needs at least 900px
    
    return componentWidth > 0 && componentWidth < threshold;
  });

  // Computed signal to determine if last updated label should be hidden
  // Hide "Last Updated" text to give more space for buttons before switching to vertical layout
  shouldHideLastUpdated = computed(() => {
    const componentWidth = this.componentWidth();
    
    // Hide "Last Updated" text at 900px to provide graceful degradation
    // This creates a buffer zone where text is hidden but horizontal layout is maintained
    const hideThreshold = 900; // Hide text to save ~200px and enable horizontal layout down to 800px
    
    return componentWidth > 0 && componentWidth < hideThreshold;
  });

  // Dynamic content test mode
  showDynamicContentTest = signal<boolean>(false);

  // UNCOMMENT BELOW TO ENABLE DUMMY DATA TESTING FOR "VIEW ALL" FUNCTIONALITY
  // useDummyData = signal(false);

  // Dashboard Card Configurations
  get actionsRequiredConfig(): DashboardCardConfig {
    return {
      ...this.DASHBOARD_CARD_CONFIGS.ACTIONS_REQUIRED,
      subtitle: `${this.getTotalDraftActions()} items need attention`
    };
  }

  get recentActivityConfig(): DashboardCardConfig {
    return {
      ...this.DASHBOARD_CARD_CONFIGS.RECENT_ACTIVITY,
      subtitle: `Latest updates from ${this.dashboardData()?.orgUnitName || 'your organization'}`
    };
  }

  get myWorkspaceConfig(): DashboardCardConfig {
    return {
      ...this.DASHBOARD_CARD_CONFIGS.MY_WORKSPACE,
      subtitle: 'Quick access to your data'
    };
  }

  get recentInteractionsConfig(): DashboardCardConfig {
    const baseConfig = {
      ...this.DASHBOARD_CARD_CONFIGS.RECENT_INTERACTIONS,
      subtitle: `${this.summary().totalMyInteractions} interactions • Latest activity`
    };
    
    // Only show the empty state action button if user has create permission
    if (!this.permissionsLoading() && this.interactionPermissions().permissions.canCreate) {
      return baseConfig;
    } else {
      // Remove the action button if no permission
      const { emptyStateActionLabel, ...configWithoutAction } = baseConfig;
      return configWithoutAction;
    }
  }

  // Dashboard Card Filters
  get actionsRequiredFilters(): DashboardCardFilter[] {
    const types = this.getDraftActionTypes();
    return types.map(type => ({
      id: type,
      label: type,
      count: this.getDraftActionCount(type),
      active: this.selectedDraftActionType() === type
    }));
  }

  get recentActivityFilters(): DashboardCardFilter[] {
    const types = this.getOrgUnitUpdateTypes();
    return types.map(type => ({
      id: type,
      label: `${this.getOrgUnitUpdateCount(type)} ${type}${this.getOrgUnitUpdateCount(type) === 1 ? '' : 's'}`,
      count: this.getOrgUnitUpdateCount(type),
      active: this.selectedOrgUnitUpdateType() === type
    }));
  }

  get myWorkspaceFilters(): DashboardCardFilter[] {
    const data = this.dashboardData();
    if (!data) return [];
    
    // Don't show filters if there are no items to filter
    const totalItems = data.myPartners.length + data.myContacts.length;
    if (totalItems === 0) return [];
    
    // Only show filters if there are multiple types of items or multiple items of one type
    const hasPartners = data.myPartners.length > 0;
    const hasContacts = data.myContacts.length > 0;
    
    // If only one type exists and it has only one item, don't show filters
    if (totalItems === 1) return [];
    
    // If both types exist or one type has multiple items, show filters
    const filters: DashboardCardFilter[] = [];
    
    if (hasPartners) {
      filters.push({
        id: 'Partner',
        label: `${data.myPartners.length} Partners`,
        count: data.myPartners.length,
        active: this.selectedOrgUnitUpdateType() === 'Partner'
      });
    }
    
    if (hasContacts) {
      filters.push({
        id: 'Contact',
        label: `${data.myContacts.length} Contacts`,
        count: data.myContacts.length,
        active: this.selectedOrgUnitUpdateType() === 'Contact'
      });
    }
    
    return filters;
  }

  // Chart data for interactions pie chart
  interactionsChartData = signal<any>(null);
  interactionsChartOptions = signal<any>({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          color: '#6B7280',
          font: {
            size: 11
          }
        }
      },
      tooltip: {
        callbacks: {
          label: (context: any) => {
            const label = context.label || '';
            const value = context.parsed || 0;
            const total = context.dataset.data.reduce((sum: number, val: number) => sum + val, 0);
            const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
            return `${label}: ${value} (${percentage}%)`;
          }
        }
      }
    }
  });

  // Chart data for actionable items bar chart
  actionableChartData = signal<any>(null);
  actionableChartOptions = signal<any>({
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context: any) => {
            const label = context.label || '';
            const value = context.parsed.y || 0;
            return `${label}: ${value} items`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1,
          color: '#6B7280'
        },
        grid: {
          color: '#E5E7EB'
        }
      },
      x: {
        ticks: {
          color: '#6B7280'
        },
        grid: {
          display: false
        }
      }
    }
  });

  ngOnInit() {
    this.checkMobileView();
    this.loadDashboardData();
    this.loadPermissions();
    this.startTimestampUpdates();

    // Initialize component width tracking with polling approach
    this.startWidthTracking();

    // Subscribe to global filter changes to automatically refresh dashboard
    this.globalFilterService.filtersChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadDashboardData();
      });
  }

  ngOnDestroy() {
    if (this.timestampInterval) {
      clearInterval(this.timestampInterval);
    }
    if (this.widthTrackingInterval) {
      clearInterval(this.widthTrackingInterval);
    }
  }

  private loadPermissions() {
    this.permissionsLoading.set(true);
    
    // Load permissions for all four entities
    forkJoin({
      partners: this.permissionService.getEntityPermissions('/partnerships/partners').pipe(
        catchError(() => of({ permissions: { canCreate: false } }))
      ),
      contacts: this.permissionService.getEntityPermissions('/partnerships/contacts').pipe(
        catchError(() => of({ permissions: { canCreate: false } }))
      ),
      interactions: this.permissionService.getEntityPermissions('/partnerships/interactions').pipe(
        catchError(() => of({ permissions: { canCreate: false } }))
      ),
      opportunities: this.permissionService.getEntityPermissions('/partnerships/opportunities').pipe(
        catchError(() => of({ permissions: { canCreate: false } }))
      )
    }).subscribe({
      next: (permissions) => {
        this.partnerPermissions.set(permissions.partners);
        this.contactPermissions.set(permissions.contacts);
        this.interactionPermissions.set(permissions.interactions);
        this.opportunityPermissions.set(permissions.opportunities);
        this.permissionsLoading.set(false);
      },
      error: () => {
        this.permissionsLoading.set(false);
      }
    });
  }

  private startTimestampUpdates() {
    this.lastDataLoadTime = new Date();
    this.updateTimestamp();
    
    // Update timestamp every 30 seconds
    this.timestampInterval = setInterval(() => {
      this.updateTimestamp();
    }, 30000);
  }

  private updateTimestamp() {
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - this.lastDataLoadTime.getTime()) / 1000);
    
    if (diffInSeconds < 60) {
      this.lastUpdatedTime.set('just now');
    } else if (diffInSeconds < 3600) {
      const minutes = Math.floor(diffInSeconds / 60);
      this.lastUpdatedTime.set(`${minutes} minute${minutes > 1 ? 's' : ''} ago`);
    } else if (diffInSeconds < 86400) {
      const hours = Math.floor(diffInSeconds / 3600);
      this.lastUpdatedTime.set(`${hours} hour${hours > 1 ? 's' : ''} ago`);
    } else {
      const days = Math.floor(diffInSeconds / 86400);
      this.lastUpdatedTime.set(`${days} day${days > 1 ? 's' : ''} ago`);
    }
  }

  // Helper method to check if user has any create permissions
  hasAnyCreatePermission(): boolean {
    return this.partnerPermissions().permissions.canCreate || 
           this.contactPermissions().permissions.canCreate || 
           this.interactionPermissions().permissions.canCreate ||
           this.opportunityPermissions().permissions.canCreate;
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    this.checkMobileView();
    this.updateComponentWidth();
  }

  private checkMobileView() {
    // Consider mobile if width is less than 768px (Tailwind's md breakpoint)
    const isMobile = typeof window !== 'undefined' && window.innerWidth < 768;
    this.isMobile.set(isMobile);
  }

  private startWidthTracking() {
    // Initial width measurement
    setTimeout(() => {
      this.updateComponentWidth();
    }, 100);

    // Poll for width changes every 100ms - this will catch AI panel resizing
    this.widthTrackingInterval = setInterval(() => {
      this.updateComponentWidth();
    }, 100);
  }

  private updateComponentWidth() {
    if (typeof window !== 'undefined' && this.widthTracker?.nativeElement) {
      const element = this.widthTracker.nativeElement;
      const width = element.offsetWidth || element.clientWidth || 0;
      this.componentWidth.set(width);
    }
  }

  private loadDashboardData() {
    this.loading.set(true);
    this.error.set(null);
    this.lastDataLoadTime = new Date();

    // UNCOMMENT BELOW TO ENABLE DUMMY DATA TESTING FOR "VIEW ALL" FUNCTIONALITY
    // if (this.useDummyData()) {
    //   console.log('Loading dummy data for testing...');
    //   this.loadDummyDashboardData();
    //   return;
    // }

    // Load user's partners using dedicated dashboard API
    const myPartners$ = this.http.get<any>(`/api/dashboard/my-partners`, {
      params: {
        pageSize: '1000'
      }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading my partners:', err);
        return of([]);
      })
    );

    // Load user's contacts using dedicated dashboard API
    const myContacts$ = this.http.get<any>(`/api/dashboard/my-contacts`, {
      params: {
        pageSize: '1000'
      }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading my contacts:', err);
        return of([]);
      })
    );

    // Load user's interactions using dedicated dashboard API
    const myInteractions$ = this.http.get<any>(`/api/dashboard/my-interactions`, {
      params: { pageSize: '1000' }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading my interactions:', err);
        return of([]);
      })
    );

    // Load user's draft partners using dedicated dashboard API
    const draftPartners$ = this.http.get<any>(`/api/dashboard/my-draft-partners`, {
      params: {
        pageSize: '1000'
      }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading draft partners:', err);
        return of([]);
      })
    );

    // Load user's draft contacts using dedicated dashboard API
    const draftContacts$ = this.http.get<any>(`/api/dashboard/my-draft-contacts`, {
      params: {
        pageSize: '1000'
      }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading draft contacts:', err);
        return of([]);
      })
    );

    // Load user's draft interactions using dedicated dashboard API
    const draftInteractions$ = this.http.get<any>(`/api/dashboard/my-draft-interactions`, {
      params: { pageSize: '1000' }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading draft interactions:', err);
        return of([]);
      })
    );

    // Load user's opportunities using dedicated dashboard API
    const myOpportunities$ = this.http.get<any>(`/api/dashboard/my-opportunities`, {
      params: { pageSize: '1000' }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading my opportunities:', err);
        return of([]);
      })
    );

    // Load user's draft opportunities using dedicated dashboard API
    const draftOpportunities$ = this.http.get<any>(`/api/dashboard/my-draft-opportunities`, {
      params: { pageSize: '1000' }
    }).pipe(
      map(response => response.records || []),
      catchError(err => {
        console.error('Error loading draft opportunities:', err);
        return of([]);
      })
    );

    // Load recent updates from current organization unit (top 10)
    const orgUnitRecentUpdates$ = this.getOrgUnitRecentUpdates().pipe(
      catchError(err => {
        console.error('Error loading org unit recent updates:', err);
        return of({
          updates: [],
          orgUnitName: 'your organization unit'
        });
      })
    );

    // Combine all requests
    forkJoin({
      myPartners: myPartners$,
      myContacts: myContacts$,
      myInteractions: myInteractions$,
      myOpportunities: myOpportunities$,
      draftPartners: draftPartners$,
      draftContacts: draftContacts$,
      draftInteractions: draftInteractions$,
      draftOpportunities: draftOpportunities$,
      orgUnitRecentUpdates: orgUnitRecentUpdates$
    }).subscribe({
      next: (data) => {
        const dashboardData: DashboardData = {
          myPartners: data.myPartners,
          myContacts: data.myContacts,
          myInteractions: data.myInteractions,
          myOpportunities: data.myOpportunities,
          draftActions: {
            partners: data.draftPartners,
            contacts: data.draftContacts,
            interactions: data.draftInteractions,
            opportunities: data.draftOpportunities
          },
          orgUnitRecentUpdates: data.orgUnitRecentUpdates.updates,
          orgUnitName: data.orgUnitRecentUpdates.orgUnitName
        };

        this.dashboardData.set(dashboardData);
        this.updateSummary(dashboardData);
        this.updateInteractionsChart(dashboardData);
        this.updateActionableChart(dashboardData);
        this.loading.set(false);
        
        // Update timestamp immediately after data loads
        this.updateTimestamp();
      },
      error: (err) => {
        console.error('Error loading dashboard data:', err);
        this.error.set('Failed to load dashboard data. Please try again.');
        this.loading.set(false);
        
        // Update timestamp even on error since we attempted to refresh
        this.updateTimestamp();
      }
    });
  }

  /* UNCOMMENT BELOW TO ENABLE DUMMY DATA TESTING FOR "VIEW ALL" FUNCTIONALITY
  private loadDummyDashboardData() {
    // Simulate loading delay for realism
    setTimeout(() => {
      const dashboardData: DashboardData = {
        myPartners: this.generateDummyPartners(15), // More than the 2 shown in normal view
        myContacts: this.generateDummyContacts(20), // More than the 1 shown in normal view
        myInteractions: this.generateDummyInteractions(25), // More than the 3 shown in normal view
        draftActions: {
          partners: this.generateDummyPartners(8).map(p => ({ ...p, status: 'Draft' })), // More than the 3 shown
          contacts: this.generateDummyContacts(12).map(c => ({ ...c, status: 'Draft' })), // More than the 3 shown
          interactions: this.generateDummyInteractions(10).map(i => ({ ...i, status: 'Draft' })) // More than the 3 shown
        },
        orgUnitRecentUpdates: this.generateDummyRecentUpdates(18), // More than the 3 shown in normal view
        orgUnitName: 'Test Organization Unit (Dummy Data)'
      };

      this.dashboardData.set(dashboardData);
      this.updateSummary(dashboardData);
      this.updateInteractionsChart(dashboardData);
      this.updateActionableChart(dashboardData);
      this.loading.set(false);
      
      // Update timestamp immediately after dummy data loads
      this.updateTimestamp();
      
      console.log('Dummy data loaded:', {
        partners: dashboardData.myPartners.length,
        contacts: dashboardData.myContacts.length,
        interactions: dashboardData.myInteractions.length,
        draftPartners: dashboardData.draftActions.partners.length,
        draftContacts: dashboardData.draftActions.contacts.length,
        draftInteractions: dashboardData.draftActions.interactions.length,
        recentUpdates: dashboardData.orgUnitRecentUpdates.length
      });
    }, 500); // 500ms delay to simulate loading
  }
  */

  private updateSummary(data: DashboardData) {
    const summary: DashboardSummary = {
      totalMyPartners: data.myPartners.length,
      totalMyContacts: data.myContacts.length,
      totalMyInteractions: data.myInteractions.length,
      totalMyOpportunities: data.myOpportunities.length,
      totalDraftActions: data.draftActions.partners.length + 
                        data.draftActions.contacts.length + 
                        data.draftActions.interactions.length +
                        data.draftActions.opportunities.length
    };
    this.summary.set(summary);
  }

  private updateInteractionsChart(data: DashboardData) {
    // Group interactions by type
    const interactionsByType = data.myInteractions.reduce((acc: any, interaction: any) => {
      const type = interaction.type || 'Unknown';
      acc[type] = (acc[type] || 0) + 1;
      return acc;
    }, {});

    const interactionTypes = Object.keys(interactionsByType);
    const interactionCounts = Object.values(interactionsByType);

    if (interactionTypes.length === 0) {
      this.interactionsChartData.set(null);
      return;
    }

    // Generate colors for pie chart
    const colors = [
      '#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444', 
      '#06B6D4', '#84CC16', '#F97316', '#EC4899', '#6366F1'
    ];

    const chartData = {
      labels: interactionTypes,
      datasets: [{
        data: interactionCounts,
        backgroundColor: colors.slice(0, interactionTypes.length),
        borderWidth: 2,
        borderColor: '#FFFFFF'
      }]
    };
    this.interactionsChartData.set(chartData);
  }

  private updateActionableChart(data: DashboardData) {
    const actionableData = [
      { label: 'Partners', count: data.draftActions.partners.length },
      { label: 'Contacts', count: data.draftActions.contacts.length },
      { label: 'Interactions', count: data.draftActions.interactions.length },
      { label: 'Opportunities', count: data.draftActions.opportunities.length }
    ];

    // Always show all categories, including zero values
    const chartData = {
      labels: actionableData.map(item => item.label),
      datasets: [{
        label: 'Draft Items',
        data: actionableData.map(item => item.count),
        backgroundColor: [
          '#F59E0B', // Orange for Partners
          '#EF4444', // Red for Contacts  
          '#8B5CF6', // Purple for Interactions
          '#3B82F6'  // Blue for Opportunities
        ],
        borderColor: [
          '#D97706',
          '#DC2626', 
          '#7C3AED',
          '#2563EB'
        ],
        borderWidth: 1,
        borderRadius: 4
      }]
    };
    this.actionableChartData.set(chartData);
  }

  navigateToPartners() {
    // Navigate directly to partners page
    this.navigateToPartnersPage();
  }

  navigateToPartnersPage() {
    // Navigate to full Partners page with user filter
    this.router.navigate(['/partnerships/partners'], { 
      queryParams: { relatedToMe: 'true' } 
    });
  }

  // New modal opening methods for Quick Actions
  openNewPartnerModal() {
    const ref = this.dialogService.open(PartnerEditDialogComponent, {
      header: 'New Partner',
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      templates: {
        footer: PartnerEditDialogFooterComponent
      },
      data: {
        mode: 'new',
        record: {},
        requestingSaveSignal: signal<boolean>(false)
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Refresh dashboard data
        this.loadDashboardData();
      }
    });
  }

  openNewContactModal() {
    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: 'New Contact',
      width: '90vw',
      style: { maxWidth: '800px' },
      closable: true,
      data: {
        mode: 'new',
        record: {}
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Refresh dashboard data
        this.loadDashboardData();
      }
    });
  }

  openNewInteractionModal() {
    const ref = this.dialogService.open(InteractionModalComponent, {
      header: 'New Interaction',
      width: '90%',
      height: '90%',
      modal: true,
      closable: true,
      data: {
        initialData: {}
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Refresh dashboard data
        this.loadDashboardData();
      }
    });
  }

  // Dialog state for Create Opportunity
  showCreateOpportunityDialog = signal(false);
  createOpportunityConfig = signal<CreateOpportunityFromInteractionsConfig>({
    partnerId: 0,
    partnerName: '',
    mode: 'list-view',
    preSelectedInteractionIds: []
  });

  openNewOpportunityModal() {
    this.createOpportunityConfig.set({
      partnerId: 0, // No specific partner - user can select
      partnerName: '',
      mode: 'list-view', // From dashboard
      preSelectedInteractionIds: []
    });
    this.showCreateOpportunityDialog.set(true);
  }

  handleOpportunityCreated(result: any) {
    if (result) {
      // Refresh dashboard data
      this.loadDashboardData();
      
      // Navigate to the new opportunity if ID is returned
      if (result.id) {
        this.router.navigate(['/partnerships/opportunities', result.id]);
      }
    }
    this.showCreateOpportunityDialog.set(false);
  }

  navigateToContacts() {
    // Navigate directly to contacts page
    this.navigateToContactsPage();
  }

  navigateToContactsPage() {
    // Navigate to full Contacts page with user filter
    this.router.navigate(['/partnerships/contacts'], { 
      queryParams: { relatedToMe: 'true' } 
    });
  }

  // Panel expansion methods
  expandPanel(panelName: string) {
    const currentExpanded = this.expandedPanel();
    // Toggle: if same panel is clicked, collapse it; otherwise expand the new one
    this.expandedPanel.set(currentExpanded === panelName ? null : panelName);
  }

  collapsePanel() {
    this.expandedPanel.set(null);
  }

  isExpanded(panelName: string): boolean {
    return this.expandedPanel() === panelName;
  }

  // Helper methods to get truncated items for normal view (max 3 items per panel)
  // On mobile, returns all items; on desktop, returns truncated items
  getTruncatedDraftActions(limit: number = 3) {
    const items = this.getDisplayedDraftActions();
    return this.isMobile() ? items : items.slice(0, limit);
  }

  getTruncatedOrgUnitUpdates(limit: number = 3) {
    const items = this.getDisplayedOrgUnitUpdates();
    return this.isMobile() ? items : items.slice(0, limit);
  }

  getTruncatedPartners(limit: number = 3) {
    const items = this.dashboardData()?.myPartners || [];
    return this.isMobile() ? items : items.slice(0, limit);
  }

  getTruncatedContacts(limit: number = 3) {
    const items = this.dashboardData()?.myContacts || [];
    return this.isMobile() ? items : items.slice(0, limit);
  }

  getTruncatedInteractions(limit: number = 3) {
    const items = this.getDisplayedInteractions();
    return this.isMobile() ? items : items.slice(0, limit);
  }

  // Get remaining counts for "View All" links
  getRemainingDraftActionsCount(): number {
    return Math.max(0, this.getDisplayedDraftActions().length - 3);
  }

  getRemainingOrgUnitUpdatesCount(): number {
    return Math.max(0, this.getDisplayedOrgUnitUpdates().length - 3);
  }

  getRemainingPartnersCount(): number {
    return Math.max(0, (this.dashboardData()?.myPartners.length || 0) - 3);
  }

  getRemainingContactsCount(): number {
    return Math.max(0, (this.dashboardData()?.myContacts.length || 0) - 3);
  }

  getRemainingInteractionsCount(): number {
    return Math.max(0, this.getDisplayedInteractions().length - 3);
  }

  // Get combined remaining count for workspace (partners + contacts)
  getRemainingWorkspaceCount(): number {
    return this.getRemainingPartnersCount() + this.getRemainingContactsCount();
  }

  navigateToInteractions() {
    // Navigate to Interactions list view with user filter
    this.router.navigate(['/partnerships/interactions'], { 
      queryParams: { relatedToMe: 'true' } 
    });
  }

  navigateToDraftActions() {
    // Navigate with filter for draft entities
    this.router.navigate(['/partnerships/partners'], { 
      queryParams: { status: 'Draft' } 
    });
  }

  onInteractionChartClick(event: any) {
    // Handle pie chart segment click to filter interactions by type
    
    if (event && event.element && typeof event.element.index !== 'undefined') {
      const dataIndex = event.element.index;
      const chartData = this.interactionsChartData();
      
      if (chartData && chartData.labels && dataIndex < chartData.labels.length) {
        const selectedType = chartData.labels[dataIndex];
        const selectedColor = chartData.datasets[0].backgroundColor[dataIndex];
        
        
        // Filter interactions by the selected type and display them
        this.showInteractionsByType(selectedType, selectedColor);
      }
    }
  }

  private showInteractionsByType(interactionType: string, color?: string) {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return;

    // Filter interactions by type
    const filtered = dashboardData.myInteractions.filter(
      (interaction: any) => (interaction.type || 'Unknown') === interactionType
    );

    // Update signals
    this.selectedInteractionType.set(interactionType);
    this.filteredInteractions.set(filtered);
    this.selectedInteractionColor.set(color || null);
  }

  clearInteractionFilter() {
    this.selectedInteractionType.set(null);
    this.filteredInteractions.set([]);
    this.selectedInteractionColor.set(null);
  }

  getInteractionBackgroundClasses(): string {
    const color = this.selectedInteractionColor();
    if (!color) return 'bg-gray-50 hover:bg-gray-100';
    
    // Map chart colors to light background classes
    const colorMap: { [key: string]: string } = {
      '#3B82F6': 'bg-blue-50 hover:bg-blue-100',     // Blue
      '#10B981': 'bg-emerald-50 hover:bg-emerald-100', // Emerald
      '#8B5CF6': 'bg-violet-50 hover:bg-violet-100',   // Violet
      '#F59E0B': 'bg-amber-50 hover:bg-amber-100',     // Amber
      '#EF4444': 'bg-red-50 hover:bg-red-100',         // Red
      '#06B6D4': 'bg-cyan-50 hover:bg-cyan-100',       // Cyan
      '#84CC16': 'bg-lime-50 hover:bg-lime-100',       // Lime
      '#F97316': 'bg-orange-50 hover:bg-orange-100',   // Orange
      '#EC4899': 'bg-pink-50 hover:bg-pink-100',       // Pink
      '#6366F1': 'bg-indigo-50 hover:bg-indigo-100'    // Indigo
    };
    
    return colorMap[color] || 'bg-gray-50 hover:bg-gray-100';
  }

  onDraftActionsChartClick(event: any) {
    // Handle bar chart click to filter draft actions by type
    
    if (event && event.element && typeof event.element.index !== 'undefined') {
      const dataIndex = event.element.index;
      const chartData = this.actionableChartData();
      
      
      if (chartData && chartData.labels && dataIndex < chartData.labels.length) {
        const selectedType = chartData.labels[dataIndex];
        const selectedColor = chartData.datasets[0].backgroundColor[dataIndex];
        
        
        // Filter draft actions by the selected type and display them
        this.showDraftActionsByType(selectedType, selectedColor);
      }
    }
  }

  private showDraftActionsByType(actionType: string, color?: string) {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return;

    let filtered: any[] = [];
    
    // Get the appropriate draft items based on type
    switch (actionType) {
      case 'Partners':
        filtered = dashboardData.draftActions.partners;
        break;
      case 'Contacts':
        filtered = dashboardData.draftActions.contacts;
        break;
      case 'Interactions':
        filtered = dashboardData.draftActions.interactions;
        break;
    }

    // Update signals
    this.selectedDraftActionType.set(actionType);
    this.filteredDraftActions.set(filtered);
    this.selectedDraftActionColor.set(color || null);
  }

  clearDraftActionFilter() {
    this.selectedDraftActionType.set(null);
    this.filteredDraftActions.set([]);
    this.selectedDraftActionColor.set(null);
  }

  getDraftActionBackgroundClasses(): string {
    const color = this.selectedDraftActionColor();
    if (!color) return 'bg-orange-50 hover:bg-orange-100'; // Default orange for drafts
    
    // Map chart colors to light background classes for draft actions
    const colorMap: { [key: string]: string } = {
      '#F59E0B': 'bg-orange-50 hover:bg-orange-100',   // Orange for Partners
      '#EF4444': 'bg-red-50 hover:bg-red-100',         // Red for Contacts
      '#8B5CF6': 'bg-violet-50 hover:bg-violet-100'    // Violet for Interactions
    };
    
    return colorMap[color] || 'bg-orange-50 hover:bg-orange-100';
  }

  navigateToEntity(entityType: string, entityId: number | null | undefined) {

    if (entityId === null || entityId === undefined) {
      console.warn('navigateToEntity: No entityId provided');
      return;
    }
    
    // Set loading state immediately for visual feedback
    const entityKey = `${entityType}-${entityId}`;
    this.navigatingToEntity.set(entityKey);
    
    const routes = {
      'Partner': ['partnerships', 'partners', entityId.toString()],
      'Contact': ['partnerships', 'contacts', entityId.toString()],
      'Interaction': ['partnerships', 'interactions', entityId.toString()],
      'Opportunity': ['partnerships', 'opportunities', entityId.toString()]
    };
    
    const routeSegments = routes[entityType as keyof typeof routes];
    
    if (routeSegments) {
      // Use router navigation with promise for better performance
      this.router.navigate(routeSegments).then(
        (success) => {
          if (success) {
          } else {
            console.warn('Navigation failed');
            this.navigatingToEntity.set(null); // Clear loading state if navigation fails
          }
        },
        (error) => {
          console.error('Navigation error:', error);
          this.navigatingToEntity.set(null); // Clear loading state on error
        }
      );
    } else {
      console.error('No route found for entityType:', entityType);
      this.navigatingToEntity.set(null); // Clear loading state
    }
  }

  private getOrgUnitRecentUpdates(): Observable<{updates: RecentUpdate[], orgUnitName: string}> {
    // Use the new dashboard API endpoint for org unit recent updates
    return this.http.get<OrgUnitRecentUpdatesResponse>('/api/dashboard/org-unit-recent-updates', {
      params: {
        pageSize: '10'
      }
    }).pipe(
      map(response => {
        return {
          updates: response.updates,
          orgUnitName: response.orgUnitName
        };
      }),
      catchError(err => {
        console.error('Error loading org unit recent updates from dashboard API:', err);
        return of({
          updates: [],
          orgUnitName: 'your organization unit'
        });
      })
    );
  }

  refreshDashboard() {
    this.loadDashboardData();
  }

  toggleDynamicContentTest() {
    this.showDynamicContentTest.set(!this.showDynamicContentTest());
  }

  formatDate(dateString: string | Date | null | undefined): string {
    if (!dateString) return 'No date';
    
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return 'Invalid date';
    
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getEntityStatusSeverity(status: string | null | undefined): "success" | "info" | "warn" | "secondary" | "contrast" | "danger" | undefined {
    switch (status?.toLowerCase()) {
      case 'active': return 'success';
      case 'draft': return 'warn';
      case 'inactive': return 'secondary';
      case 'closed': return 'danger';
      case 'archived': return 'contrast';
      default: return 'info';
    }
  }



  getEntityId(id: any): number | null {
    if (id === null || id === undefined) { 
      return null;
    }
    
    const result = typeof id === 'string' ? parseInt(id, 10) : id;
    return result;
  }

  isEntityNavigating(entityType: string, entityId: number | null | undefined): boolean {
    if (entityId === null || entityId === undefined) return false;
    const entityKey = `${entityType}-${entityId}`;
    return this.navigatingToEntity() === entityKey;
  }

  getDisplayDate(entity: any): string {
    const date = entity.lastModifiedDate || entity.createdDate;
    return this.formatDate(date);
  }

  getUpdateIcon(type: string): string {
    switch (type) {
      case 'Partner': return 'pi pi-users';
      case 'Contact': return 'pi pi-user';
      case 'Interaction': return 'pi pi-comments';
      case 'Opportunity': return 'pi pi-briefcase';
      default: return 'pi pi-circle';
    }
  }

  getUpdateIconClass(type: string): string {
    switch (type) {
      case 'Partner': return 'partner-icon';
      case 'Contact': return 'contact-icon';
      case 'Interaction': return 'interaction-icon';
      case 'Opportunity': return 'opportunity-icon';
      default: return 'default-icon';
    }
  }

  getInteractionTypes(): string[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData || !dashboardData.myInteractions) return [];
    
    // Get unique interaction types
    const types = new Set(dashboardData.myInteractions.map((interaction: any) => interaction.type || 'Unknown'));
    return Array.from(types).sort();
  }

  setInteractionFilter(type: string) {
    this.selectedInteractionType.set(type);
    this.showInteractionsByType(type);
  }

  getDisplayedInteractions(): any[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData || !dashboardData.myInteractions) return [];
    
    const selectedType = this.selectedInteractionType();
    if (!selectedType) {
      return dashboardData.myInteractions;
    }
    
    return dashboardData.myInteractions.filter(
      (interaction: any) => (interaction.type || 'Unknown') === selectedType
    );
  }

  getTotalDraftActions(): number {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return 0;
    
    return dashboardData.draftActions.partners.length + 
           dashboardData.draftActions.contacts.length + 
           dashboardData.draftActions.interactions.length +
           dashboardData.draftActions.opportunities.length;
  }

  getDraftActionTypes(): string[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return [];
    
    const types: string[] = [];
    if (dashboardData.draftActions.partners.length > 0) types.push('Partners');
    if (dashboardData.draftActions.contacts.length > 0) types.push('Contacts');
    if (dashboardData.draftActions.interactions.length > 0) types.push('Interactions');
    if (dashboardData.draftActions.opportunities.length > 0) types.push('Opportunities');
    
    return types;
  }

  getDraftActionCount(type: string): number {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return 0;
    
    switch (type) {
      case 'Partners':
        return dashboardData.draftActions.partners.length;
      case 'Contacts':
        return dashboardData.draftActions.contacts.length;
      case 'Interactions':
        return dashboardData.draftActions.interactions.length;
      case 'Opportunities':
        return dashboardData.draftActions.opportunities.length;
      default:
        return 0;
    }
  }

  setDraftActionFilter(type: string) {
    this.selectedDraftActionType.set(type);
    this.showDraftActionsByType(type);
  }

  getDisplayedDraftActions(): any[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return [];
    
    const selectedType = this.selectedDraftActionType();
    if (!selectedType) {
      // Return all draft actions combined
      return [
        ...dashboardData.draftActions.partners,
        ...dashboardData.draftActions.contacts,
        ...dashboardData.draftActions.interactions,
        ...dashboardData.draftActions.opportunities
      ];
    }
    
    switch (selectedType) {
      case 'Partners':
        return dashboardData.draftActions.partners;
      case 'Contacts':
        return dashboardData.draftActions.contacts;
      case 'Interactions':
        return dashboardData.draftActions.interactions;
      case 'Opportunities':
        return dashboardData.draftActions.opportunities;
      default:
        return [];
    }
  }

  getDraftActionEntityType(item: any): string {
    const dashboardData = this.dashboardData();
    if (!dashboardData) return 'Partner';
    
    // Determine entity type based on which array the item belongs to
    if (dashboardData.draftActions.partners.some((p: any) => p.id === item.id)) return 'Partner';
    if (dashboardData.draftActions.contacts.some((c: any) => c.id === item.id)) return 'Contact';
    if (dashboardData.draftActions.interactions.some((i: any) => i.id === item.id)) return 'Interaction';
    if (dashboardData.draftActions.opportunities.some((o: any) => o.id === item.id)) return 'Opportunity';
    
    return 'Partner'; // Default fallback
  }

  getDraftActionDisplayName(item: any): string {
    const entityType = this.getDraftActionEntityType(item);
    
    switch (entityType) {
      case 'Partner':
        return item.name || 'Unnamed Partner';
      case 'Contact':
        return `${item.firstName || ''} ${item.lastName || ''}`.trim() || 'Unnamed Contact';
      case 'Interaction':
        return item.subject || 'Untitled Interaction';
      case 'Opportunity':
        return item.name || 'Untitled Opportunity';
      default:
        return 'Unknown Item';
    }
  }

  getDraftActionType(item: any): string {
    return this.getDraftActionEntityType(item);
  }

  getDraftActionDescription(item: any): string {
    const entityType = this.getDraftActionEntityType(item);
    
    switch (entityType) {
      case 'Contact':
        return item.title || '';
      case 'Interaction':
        return item.description || '';
      case 'Opportunity':
        return item.description || '';
      default:
        return '';
    }
  }

  // Org Unit Recent Updates filtering methods (client-side filtering)
  getOrgUnitUpdateTypes(): string[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData || !dashboardData.orgUnitRecentUpdates) return [];
    
    const types = new Set<string>();
    dashboardData.orgUnitRecentUpdates.forEach(update => {
      if (update.type) {
        types.add(update.type);
      }
    });
    
    return Array.from(types).sort();
  }

  getOrgUnitUpdateCount(type: string): number {
    const dashboardData = this.dashboardData();
    if (!dashboardData || !dashboardData.orgUnitRecentUpdates) return 0;
    
    return dashboardData.orgUnitRecentUpdates.filter(update => update.type === type).length;
  }

  setOrgUnitUpdateFilter(type: string) {
    this.selectedOrgUnitUpdateType.set(type);
  }

  clearOrgUnitUpdateFilter() {
    this.selectedOrgUnitUpdateType.set(null);
  }

  // Dashboard Card Event Handlers
  onActionsFilterClick(filter: DashboardCardFilter): void {
    this.setDraftActionFilter(filter.id);
  }

  onActivityFilterClick(filter: DashboardCardFilter): void {
    this.setOrgUnitUpdateFilter(filter.id);
  }

  onWorkspaceFilterClick(filter: DashboardCardFilter): void {
    this.setOrgUnitUpdateFilter(filter.id);
  }

  getDisplayedOrgUnitUpdates(): RecentUpdate[] {
    const dashboardData = this.dashboardData();
    if (!dashboardData || !dashboardData.orgUnitRecentUpdates) return [];
    
    const selectedType = this.selectedOrgUnitUpdateType();
    if (!selectedType) {
      return dashboardData.orgUnitRecentUpdates;
    }
    
    return dashboardData.orgUnitRecentUpdates.filter(update => update.type === selectedType);
  }

  // UNCOMMENT BELOW TO ENABLE DUMMY DATA TESTING FOR "VIEW ALL" FUNCTIONALITY
  // toggleDummyData() {
  //   this.useDummyData.set(!this.useDummyData());
  //   this.loadDashboardData();
  // }

  /* UNCOMMENT BELOW TO ENABLE DUMMY DATA TESTING FOR "VIEW ALL" FUNCTIONALITY
  
  // Dummy data generators for testing "View All" functionality
  private generateDummyPartners(count: number = 15): Partner[] {
    const partners: Partner[] = [];
    const companyNames = [
      'Acme Corporation', 'Global Solutions Inc.', 'Tech Innovations Ltd.', 'Future Dynamics',
      'Strategic Partners LLC', 'International Holdings', 'Prime Ventures', 'Digital Enterprises',
      'Advanced Systems', 'Elite Consulting', 'Progressive Industries', 'Summit Technologies',
      'Apex Solutions', 'Pinnacle Group', 'Metropolitan Services', 'Continental Corp.',
      'Universal Partners', 'Premier Associates', 'Executive Solutions', 'Leading Edge Inc.'
    ];
    const statuses = ['Active', 'Pending', 'Unknown'];
    
    for (let i = 1; i <= count; i++) {
      partners.push({
        id: i.toString(), // Partner ID should be string
        name: companyNames[i % companyNames.length] + ` ${Math.floor(i / companyNames.length) + 1}`,
        partnerDescription: companyNames[i % companyNames.length] + ` ${Math.floor(i / companyNames.length) + 1}`,
        status: statuses[i % statuses.length],
        lastModifiedDate: new Date(this.generateRandomDate(-30)),
        createdDate: new Date(this.generateRandomDate(-60)),
        // Add other Partner properties as needed
      } as Partner);
    }
    return partners;
  }

  private generateDummyContacts(count: number = 20): Contact[] {
    const contacts: Contact[] = [];
    const firstNames = [
      'John', 'Jane', 'Michael', 'Sarah', 'David', 'Emma', 'Robert', 'Lisa', 'James', 'Mary',
      'Christopher', 'Jennifer', 'Daniel', 'Patricia', 'Matthew', 'Linda', 'Anthony', 'Elizabeth',
      'Mark', 'Barbara', 'Paul', 'Susan', 'Steven', 'Jessica', 'Kenneth', 'Dorothy'
    ];
    const lastNames = [
      'Smith', 'Johnson', 'Williams', 'Brown', 'Jones', 'Garcia', 'Miller', 'Davis', 'Rodriguez',
      'Martinez', 'Hernandez', 'Lopez', 'Gonzalez', 'Wilson', 'Anderson', 'Thomas', 'Taylor',
      'Moore', 'Jackson', 'Martin', 'Lee', 'Perez', 'Thompson', 'White', 'Harris', 'Sanchez'
    ];
    const titles = [
      'CEO', 'CTO', 'Marketing Director', 'Project Manager', 'Sales Manager', 'Operations Manager',
      'Business Analyst', 'Senior Developer', 'HR Manager', 'Finance Director', 'Product Manager',
      'Regional Director', 'Account Manager', 'Technical Lead', 'Consultant'
    ];
    const statuses = ['Active', 'Pending', 'Unknown'];
    
    for (let i = 1; i <= count; i++) {
      contacts.push({
        id: i.toString(), // Contact ID should be string
        firstName: firstNames[i % firstNames.length],
        lastName: lastNames[i % lastNames.length],
        title: titles[i % titles.length],
        status: statuses[i % statuses.length],
        lastModifiedDate: new Date(this.generateRandomDate(-30)),
        createdDate: new Date(this.generateRandomDate(-60)),
        // Add other Contact properties as needed
      } as Contact);
    }
    return contacts;
  }

  private generateDummyInteractions(count: number = 25): Interaction[] {
    const interactions: Interaction[] = [];
    const types: InteractionType[] = [
      InteractionType.Email,
      InteractionType.Call, 
      InteractionType.VirtualMeeting,
      InteractionType.InPersonMeeting,
      InteractionType.Chat
    ];
    const subjects = [
      'Project Status Update', 'Partnership Discussion', 'Contract Review', 'Technical Assessment',
      'Budget Planning', 'Strategy Meeting', 'Quarterly Review', 'Proposal Discussion',
      'Implementation Planning', 'Performance Review', 'Client Check-in', 'Solution Demo',
      'Requirements Gathering', 'Risk Assessment', 'Progress Report', 'Training Session'
    ];
    const descriptions = [
      'Discussed project milestones and deliverables', 'Reviewed contract terms and conditions',
      'Addressed technical requirements and specifications', 'Evaluated partnership opportunities',
      'Analyzed budget allocation and resource planning', 'Coordinated implementation timeline',
      'Assessed project risks and mitigation strategies', 'Demonstrated platform capabilities',
      'Gathered detailed business requirements', 'Reviewed quarterly performance metrics'
    ];
    const statuses = ['Completed', 'Pending', 'Draft'];
    
    for (let i = 1; i <= count; i++) {
      interactions.push({
        id: i, // Interaction ID should remain number
        type: types[i % types.length],
        subject: subjects[i % subjects.length] + ` #${i}`,
        description: descriptions[i % descriptions.length],
        status: statuses[i % statuses.length],
        date: this.generateRandomDate(-30),
        contactId: 1, // Required field
        contactIds: [1],
        partnerIds: [1],
        emailAddresses: [],
        phoneNumbers: [],
        location: 'Virtual',
        createdBy: 1,
        createdDate: this.generateRandomDate(-30),
        lastModifiedDate: this.generateRandomDate(-15),
        // Add other required Interaction properties
      } as Interaction);
    }
    return interactions;
  }

  private generateDummyRecentUpdates(count: number = 18): RecentUpdate[] {
    const updates: RecentUpdate[] = [];
    const types: ('Partner' | 'Contact' | 'Interaction')[] = ['Partner', 'Contact', 'Interaction'];
    const names = [
      'Global Tech Solutions', 'Sarah Johnson', 'Project Kickoff Meeting',
      'Innovation Partners LLC', 'Michael Chen', 'Client Status Update',
      'Strategic Ventures Inc.', 'Lisa Anderson', 'Requirements Review',
      'Digital Dynamics Corp.', 'David Rodriguez', 'Partnership Discussion',
      'Elite Consulting Group', 'Emma Thompson', 'Technical Assessment',
      'Premier Solutions Ltd.', 'James Wilson', 'Budget Planning Session'
    ];
    const users = [
      'John Smith', 'Jane Doe', 'Mike Johnson', 'Sarah Wilson', 'David Brown',
      'Lisa Davis', 'Robert Taylor', 'Emma Anderson', 'James Garcia', 'Mary Martinez'
    ];
    
    for (let i = 1; i <= count; i++) {
      updates.push({
        id: i,
        name: names[i % names.length] + ` ${Math.floor(i / names.length) + 1}`,
        type: types[i % types.length],
        lastModifiedDate: this.generateRandomDate(-15),
        lastModifiedBy: users[i % users.length],
        status: 'Active'
      });
    }
    return updates;
  }

  private generateRandomDate(daysAgo: number): string {
    const date = new Date();
    date.setDate(date.getDate() + Math.floor(Math.random() * daysAgo));
    return date.toISOString();
  }
  
  */
}
