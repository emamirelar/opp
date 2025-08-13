import { Component, OnInit, inject, signal } from '@angular/core';
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

import { PartnerService } from '../../../../features/internal/services/partner.service';
import { ContactService } from '../../../../features/internal/services/contact.service';
import { InteractionService } from '../../../../features/internal/services/interaction.service';
import { Partner } from '../../../../features/internal/models/partner.model';
import { Contact } from '../../../../features/internal/models/contact.model';
import { Interaction } from '../../../../features/internal/models/interaction.model';

interface DashboardData {
  myPartners: Partner[];
  myContacts: Contact[];
  myInteractions: Interaction[];
  draftActions: {
    partners: Partner[];
    contacts: Contact[];
    interactions: Interaction[];
  };
  orgUnitRecentUpdates: RecentUpdate[];
}

interface RecentUpdate {
  id: number;
  name: string;
  type: 'Partner' | 'Contact' | 'Interaction';
  lastModifiedDate: string;
  lastModifiedBy: string;
  status: string;
  entityData?: any; // Additional entity-specific data
}

interface DashboardSummary {
  totalMyPartners: number;
  totalMyContacts: number;
  totalMyInteractions: number;
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
    HttpClientModule
  ]
})
export class HomeDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private partnerService = inject(PartnerService);
  private contactService = inject(ContactService);
  private interactionService = inject(InteractionService);

  loading = signal(true);
  error = signal<string | null>(null);
  dashboardData = signal<DashboardData | null>(null);
  summary = signal<DashboardSummary>({
    totalMyPartners: 0,
    totalMyContacts: 0,
    totalMyInteractions: 0,
    totalDraftActions: 0
  });

  // Visibility toggles for list views
  showPartnersListView = signal(false);
  showContactsListView = signal(false);
  showOrgUnitUpdatesView = signal(false);

  // Interaction chart filtering
  selectedInteractionType = signal<string | null>(null);
  filteredInteractions = signal<Interaction[]>([]);
  selectedInteractionColor = signal<string | null>(null);

  // Draft actions chart filtering
  selectedDraftActionType = signal<string | null>(null);
  filteredDraftActions = signal<any[]>([]);
  selectedDraftActionColor = signal<string | null>(null);

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
    this.loadDashboardData();
  }

  private loadDashboardData() {
    this.loading.set(true);
    this.error.set(null);

    // Load ALL user's entities (created or last modified by user, excluding Draft status)
    // Using large pageSize to get all records, ignoring global filters
    const myPartners$ = this.http.get<any>(`/api/partner`, {
      params: {
        pageIndex: '1',
        pageSize: '1000', // Large number to get all user records
        orderBy: 'lastModifiedDate',
        ascending: 'false',
        relatedToMe: 'true', // User-specific filter
        ignoreGlobalFilters: 'true' // Bypass any global filters
      }
    }).pipe(
      map(response => (response.records || []).filter((p: any) => p.status !== 'Draft')),
      catchError(err => {
        console.error('Error loading my partners:', err);
        return of([]);
      })
    );

    const myContacts$ = this.http.get<any>(`/api/contact`, {
      params: {
        pageIndex: '1',
        pageSize: '1000', // Large number to get all user records
        orderBy: 'lastModifiedDate',
        ascending: 'false',
        relatedToMe: 'true', // User-specific filter
        ignoreGlobalFilters: 'true' // Bypass any global filters
      }
    }).pipe(
      map(response => {
        // Filter out Draft contacts to show only Active contacts
        return (response.records || []).filter((c: any) => c.status !== 'Draft');
      }),
      catchError(err => {
        console.error('Error loading my contacts:', err);
        return of([]);
      })
    );

    const myInteractions$ = this.interactionService.getAll({
      pageIndex: 1,
      pageSize: 1000, // Large number to get all user records
      orderBy: 'date',
      ascending: 'false'
    }).pipe(
      map(response => (response.body?.records || []).filter((i: any) => i.status !== 'Draft')),
      catchError(err => {
        console.error('Error loading my interactions:', err);
        return of([]);
      })
    );

    // Load ALL draft entities (that need action) - ignoring global filters
    const draftPartners$ = this.http.get<any>(`/api/partner`, {
      params: {
        pageIndex: '1',
        pageSize: '1000',
        orderBy: 'createdDate',
        ascending: 'false',
        status: 'Draft',
        ignoreGlobalFilters: 'true'
      }
    }).pipe(
      map(response => {
        // Ensure only Draft status records are shown in Actions Required
        return (response.records || []).filter((p: any) => p.status === 'Draft');
      }),
      catchError(err => {
        console.error('Error loading draft partners:', err);
        return of([]);
      })
    );

    const draftContacts$ = this.http.get<any>(`/api/contact`, {
      params: {
        pageIndex: '1',
        pageSize: '1000',
        orderBy: 'createdDate',
        ascending: 'false',
        status: 'Draft',
        ignoreGlobalFilters: 'true'
      }
    }).pipe(
      map(response => {
        // Ensure only Draft status records are shown in Actions Required
        return (response.records || []).filter((c: any) => c.status === 'Draft');
      }),
      catchError(err => {
        console.error('Error loading draft contacts:', err);
        return of([]);
      })
    );

    const draftInteractions$ = this.interactionService.getAll({
      pageIndex: 1,
      pageSize: 1000,
      orderBy: 'createdDate',
      ascending: 'false'
    }).pipe(
      map(response => (response.body?.records || []).filter((i: any) => i.status === 'Draft')),
      catchError(err => {
        console.error('Error loading draft interactions:', err);
        return of([]);
      })
    );

    // Load recent updates from current organization unit (top 10)
    const orgUnitRecentUpdates$ = this.getOrgUnitRecentUpdates().pipe(
      catchError(err => {
        console.error('Error loading org unit recent updates:', err);
        return of([]);
      })
    );

    // Combine all requests
    forkJoin({
      myPartners: myPartners$,
      myContacts: myContacts$,
      myInteractions: myInteractions$,
      draftPartners: draftPartners$,
      draftContacts: draftContacts$,
      draftInteractions: draftInteractions$,
      orgUnitRecentUpdates: orgUnitRecentUpdates$
    }).subscribe({
      next: (data) => {
        const dashboardData: DashboardData = {
          myPartners: data.myPartners,
          myContacts: data.myContacts,
          myInteractions: data.myInteractions,
          draftActions: {
            partners: data.draftPartners,
            contacts: data.draftContacts,
            interactions: data.draftInteractions
          },
          orgUnitRecentUpdates: data.orgUnitRecentUpdates
        };

        this.dashboardData.set(dashboardData);
        this.updateSummary(dashboardData);
        this.updateInteractionsChart(dashboardData);
        this.updateActionableChart(dashboardData);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading dashboard data:', err);
        this.error.set('Failed to load dashboard data. Please try again.');
        this.loading.set(false);
      }
    });
  }

  private updateSummary(data: DashboardData) {
    const summary: DashboardSummary = {
      totalMyPartners: data.myPartners.length,
      totalMyContacts: data.myContacts.length,
      totalMyInteractions: data.myInteractions.length,
      totalDraftActions: data.draftActions.partners.length + 
                        data.draftActions.contacts.length + 
                        data.draftActions.interactions.length
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
      { label: 'Interactions', count: data.draftActions.interactions.length }
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
          '#8B5CF6'  // Purple for Interactions
        ],
        borderColor: [
          '#D97706',
          '#DC2626', 
          '#7C3AED'
        ],
        borderWidth: 1,
        borderRadius: 4
      }]
    };
    this.actionableChartData.set(chartData);
  }

  navigateToPartners() {
    // Toggle the visibility of the Partners list view
    this.showPartnersListView.set(!this.showPartnersListView());
  }

  navigateToPartnersPage() {
    // Navigate to full Partners page with user filter
    this.router.navigate(['/partnerships/partners'], { 
      queryParams: { relatedToMe: 'true' } 
    });
  }

  navigateToContacts() {
    // Toggle the visibility of the Contacts list view
    this.showContactsListView.set(!this.showContactsListView());
  }

  navigateToContactsPage() {
    // Navigate to full Contacts page with user filter
    this.router.navigate(['/partnerships/contacts'], { 
      queryParams: { relatedToMe: 'true' } 
    });
  }

  toggleOrgUnitUpdates() {
    // Toggle the visibility of the Org Unit Recent Updates list view
    this.showOrgUnitUpdatesView.set(!this.showOrgUnitUpdatesView());
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
    console.log('Chart click event:', event);
    
    if (event && event.element && typeof event.element.index !== 'undefined') {
      const dataIndex = event.element.index;
      const chartData = this.interactionsChartData();
      
      console.log('Data index:', dataIndex, 'Chart data:', chartData);
      
      if (chartData && chartData.labels && dataIndex < chartData.labels.length) {
        const selectedType = chartData.labels[dataIndex];
        const selectedColor = chartData.datasets[0].backgroundColor[dataIndex];
        
        console.log('Selected interaction type:', selectedType, 'Selected color:', selectedColor);
        
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
    console.log('Draft actions chart click event:', event);
    
    if (event && event.element && typeof event.element.index !== 'undefined') {
      const dataIndex = event.element.index;
      const chartData = this.actionableChartData();
      
      console.log('Data index:', dataIndex, 'Chart data:', chartData);
      
      if (chartData && chartData.labels && dataIndex < chartData.labels.length) {
        const selectedType = chartData.labels[dataIndex];
        const selectedColor = chartData.datasets[0].backgroundColor[dataIndex];
        
        console.log('Selected draft action type:', selectedType, 'Selected color:', selectedColor);
        
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
    console.log('navigateToEntity called with:', { entityType, entityId });
    
    if (entityId === null || entityId === undefined) {
      console.warn('navigateToEntity: No entityId provided');
      return;
    }
    
    const routes = {
      'Partner': `/partnerships/partners/${entityId}`,
      'Contact': `/partnerships/contacts/${entityId}`,
      'Interaction': `/partnerships/interactions/${entityId}`
    };
    
    const route = routes[entityType as keyof typeof routes];
    console.log('Navigating to route:', route);
    
    if (route) {
      this.router.navigate([route]);
    } else {
      console.error('No route found for entityType:', entityType);
    }
  }

  private getOrgUnitRecentUpdates(): Observable<RecentUpdate[]> {
    // Get recent updates from all entity types in the current org unit
    // We'll make parallel calls to all three endpoints and combine the results
    
    const recentPartners$ = this.http.get<any>(`/api/partner`, {
      params: {
        pageIndex: '1',
        pageSize: '20', // Get more than 10 to allow for filtering
        orderBy: 'lastModifiedDate',
        ascending: 'false'
      }
    }).pipe(
      map(response => (response.records || []).map((partner: any) => ({
        id: partner.id,
        name: partner.name || 'Unnamed Partner',
        type: 'Partner' as const,
        lastModifiedDate: partner.lastModifiedDate || partner.createdDate,
        lastModifiedBy: partner.lastModifiedBy || partner.createdBy || 'Unknown',
        status: partner.status || 'Unknown',
        entityData: partner
      }))),
      catchError(err => {
        console.error('Error loading recent partners:', err);
        return of([]);
      })
    );

    const recentContacts$ = this.http.get<any>(`/api/contact`, {
      params: {
        pageIndex: '1',
        pageSize: '20',
        orderBy: 'lastModifiedDate',
        ascending: 'false'
      }
    }).pipe(
      map(response => (response.records || []).map((contact: any) => ({
        id: contact.id,
        name: `${contact.firstName || ''} ${contact.lastName || ''}`.trim() || 'Unnamed Contact',
        type: 'Contact' as const,
        lastModifiedDate: contact.lastModifiedDate || contact.createdDate,
        lastModifiedBy: contact.lastModifiedBy || contact.createdBy || 'Unknown',
        status: contact.status || 'Unknown',
        entityData: contact
      }))),
      catchError(err => {
        console.error('Error loading recent contacts:', err);
        return of([]);
      })
    );

    const recentInteractions$ = this.http.get<any>(`/api/interactions`, {
      params: {
        pageIndex: '1',
        pageSize: '20',
        orderBy: 'lastModifiedDate',
        ascending: 'false'
      }
    }).pipe(
      map(response => (response.records || []).map((interaction: any) => ({
        id: interaction.id,
        name: interaction.subject || 'Unnamed Interaction',
        type: 'Interaction' as const,
        lastModifiedDate: interaction.date,
        lastModifiedBy: interaction.lastModifiedBy || interaction.createdBy || 'Unknown',
        status: interaction.status || 'Unknown',
        entityData: interaction
      }))),
      catchError(err => {
        console.error('Error loading recent interactions:', err);
        return of([]);
      })
    );

    // Combine all recent updates and sort by date
    return forkJoin({
      partners: recentPartners$,
      contacts: recentContacts$,
      interactions: recentInteractions$
    }).pipe(
      map(data => {
        const allUpdates = [
          ...data.partners,
          ...data.contacts,
          ...data.interactions
        ];

        // Sort by last modified date (most recent first) and take top 10
        console.log('Org Unit Recent Updates Debug:', {
          partners: data.partners.length,
          contacts: data.contacts.length,
          interactions: data.interactions.length,
          total: allUpdates.length,
          sample: allUpdates.slice(0, 2) // Show first 2 items for debugging
        });
        
        const filtered = allUpdates.filter(update => update.lastModifiedDate); // Filter out items without dates
        console.log('Filtered updates:', filtered.length, 'items with dates');
        
        const sorted = filtered.sort((a, b) => new Date(b.lastModifiedDate).getTime() - new Date(a.lastModifiedDate).getTime());
        const final = sorted.slice(0, 10); // Take only top 10
        
        console.log('Final org unit updates:', final.length, 'items');
        return final;
      })
    );
  }

  refreshDashboard() {
    this.loadDashboardData();
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

  getEntityStatusSeverity(status: string | null | undefined): string {
    switch (status?.toLowerCase()) {
      case 'active': return 'success';
      case 'draft': return 'warning';
      case 'inactive': return 'secondary';
      case 'closed': return 'danger';
      default: return 'info';
    }
  }

  getCurrentDate(): Date {
    return new Date();
  }

  getEntityId(id: any): number | null {
    if (id === null || id === undefined) {
      console.log('getEntityId called with:', id, 'returning: null');
      return null;
    }
    
    const result = typeof id === 'string' ? parseInt(id, 10) : id;
    console.log('getEntityId called with:', id, 'returning:', result);
    return result;
  }

  getDisplayDate(entity: any): string {
    const date = entity.lastModifiedDate || entity.createdDate || this.getCurrentDate();
    return this.formatDate(date);
  }

  getUpdateIcon(type: string): string {
    switch (type) {
      case 'Partner': return 'pi pi-users';
      case 'Contact': return 'pi pi-user';
      case 'Interaction': return 'pi pi-comments';
      default: return 'pi pi-circle';
    }
  }

  getUpdateIconClass(type: string): string {
    switch (type) {
      case 'Partner': return 'partner-icon';
      case 'Contact': return 'contact-icon';
      case 'Interaction': return 'interaction-icon';
      default: return 'default-icon';
    }
  }
}
