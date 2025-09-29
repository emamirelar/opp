import { ChangeDetectionStrategy, Component, inject, OnInit, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DialogModule } from 'primeng/dialog';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { BadgeModule } from 'primeng/badge';
import { ActivatedRoute, Router } from '@angular/router';
import { ListViewColumn, ListViewConfig } from '../../../../common/pages/components/listview/listview.model';
import { ListviewCardComponent } from '../../../../common/pages/components/listview/card/listview-card.component';
import { EntityConfigurationService } from '../../services/entity-configuration.service';
import { HttpClient } from '@angular/common/http';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { Subject } from 'rxjs';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { GlobalFilterService } from '../../../../services/global-filter.service';
import { UserPreferenceService, GlobalFilters } from '../../../../services/user-preference.service';
import { OrganizationHierarchyService } from '../../../../services/organization-hierarchy.service';
import { AuthService } from '../../../../essentials/services/auth.service';
import { GlobalFiltersDialogService } from '../../../../services/global-filters-dialog.service';

/**
 * @uiEntity SearchResult
 * @route /search
 * @description Unified search results page displaying all matching records across multiple entity types (Partners, Contacts, Interactions). Supports tabbed view by entity type with filtering and global filters.
 * @capabilities search_all_entities, filter_by_entity_type, global_filters, view_search_snippets, navigate_to_records
 * @synonyms find, lookup, global_search, unified_search, cross_entity_search
 * @mandatoryFields search_query
 * @help_when_stuck Use the search box in the header to perform searches. Results are automatically categorized by entity type (Partners, Contacts, Interactions). Click on tabs to view specific entity types. Enable global filters if you want to restrict search to your organization or specific criteria.
 * @common_tasks
 *   - Searching across all entities: Use the main search box, results appear automatically
 *   - Viewing specific entity results: Click on tabs (Partners, Contacts, Interactions)
 *   - Applying global filters: Toggle the global filter switch to restrict to your org unit
 *   - Opening a record: Click on any search result card to navigate to the detailed view
 *   - Refining search: Modify your search query in the header search box
 */

interface SearchMetadata {
  matchedField?: string;
  searchType?: string;
  matchCriteria?: string;
  score?: number;
  snippet?: string;
}

interface EnhancedSearchResult {
  id: number;
  _searchMetadata?: SearchMetadata;
  [key: string]: any; // Allow all original entity properties to flow through
}

interface SearchResponse {
  availableEntities: string[];
  results: {
    [entityType: string]: EnhancedSearchResult[];
  };
}

interface EntityTab {
  key: string;
  label: string;
  count: number;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-search-result',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,

    DialogModule,
    CardModule,
    ButtonModule,
    TableModule,
    BadgeModule,
    ListviewCardComponent,
    ReactiveFormsModule,
    FormsModule,
    ToggleSwitchModule
  ],
  templateUrl: './search-result.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
      
      .p-tabpanels {
        padding: 0;
      }

      .search-highlight {
        background-color: #fef3c7 !important;
        background-image: linear-gradient(120deg, #fef3c7 0%, #fde047 100%) !important;
        padding: 2px 4px !important;
        border-radius: 3px !important;
        font-weight: 600 !important;
        color: #854d0e !important;
        text-shadow: 0 1px 0 rgba(255, 255, 255, 0.5) !important;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1) !important;
      }

      /* Mobile-specific improvements */
      @media (max-width: 768px) {
        /* Better mobile scrolling */
        .h-\\[calc\\(100vh-240px\\)\\] {
          height: calc(100vh - 200px) !important;
          -webkit-overflow-scrolling: touch;
        }

        /* Improved card interactions on mobile */
        .transform {
          transition: transform 0.15s ease-in-out;
        }

        /* Better focus states for mobile dropdown */
        select:focus {
          outline: none;
          ring: 2px;
          ring-offset: 2px;
        }

        /* Improved touch targets */
        button, select {
          min-height: 44px;
        }

        /* Better spacing for mobile cards */
        .space-y-4 > * + * {
          margin-top: 1rem !important;
        }
      }
    }
    
    .search-highlight {
      background-color: #fef3c7;
      background-image: linear-gradient(120deg, #fef3c7 0%, #fde047 100%);
      padding: 2px 4px;
      border-radius: 3px;
      font-weight: 600;
      color: #854d0e;
      text-shadow: 0 1px 0 rgba(255, 255, 255, 0.5);
      box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
    }
    
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    .animate-fadeIn {
      animation: fadeIn 0.3s ease-out;
    }

    .metadata-section {
      transition: opacity 0.2s ease-in-out, transform 0.2s ease-in-out;
    }
    
    .metadata-hidden {
      opacity: 0;
      transform: translateY(-5px);
      pointer-events: none;
      height: 0;
      overflow: hidden;
    }
    
    .metadata-visible {
      opacity: 1;
      transform: translateY(0);
      pointer-events: auto;
      height: auto;
    }

    /* Enhanced mobile card styling */
    @media (max-width: 640px) {
      .rounded-lg {
        border-radius: 0.75rem;
      }
      
      .shadow-sm {
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
      }
      
      .hover\\:shadow-md:hover {
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
      }
    }
  `
})
export class SearchResultComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private entityConfigurationService = inject(EntityConfigurationService);
  private globalFilterService = inject(GlobalFilterService);
  private userPreferenceService = inject(UserPreferenceService);
  private organizationHierarchyService = inject(OrganizationHierarchyService);
  private authService = inject(AuthService);
  private globalFiltersDialogService = inject(GlobalFiltersDialogService);
  private translateService = inject(TranslateService);
  private destroy$ = new Subject<void>();

  searchQuery = signal<string>('');
  searchResponse: SearchResponse | null = null;
  entityTabs: EntityTab[] = [];
  activeTabKey: string = 'all';
  isLoading = signal(false);
  
  // Toggle for showing/hiding search metadata - optimized with computed signal
  _showSearchMetadata = signal(false);
  showSearchMetadata = computed(() => this._showSearchMetadata());
  
  // Search control for the page search bar
  currentSearchTerm = signal<string>('');
  searchControl = new FormControl('');

  // Entity columns loaded from configuration service
  contactColumns = signal<ListViewColumn[]>([]);
  partnerColumns = signal<ListViewColumn[]>([]);
  interactionColumns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(false);

  // Memoization cache for metadata checks
  private metadataCache = new Map<string, boolean>();
  private metadataPropertiesCache = new Map<string, any>();

  // Global filter information
  isGlobalFilterActive = signal(false);
  activeOrgUnitName = signal<string>('');
  globalFilters = signal<GlobalFilters | null>(null);
  currentUserId = signal<string>('');
  activeFilterLabels = signal<string[]>([]);
  
  // Filter toggle state - tracks whether filters are temporarily disabled
  isFilterTemporarilyDisabled = signal(false);

  ngOnInit(): void {
    this.loadAllEntityColumns();
    this.loadGlobalFilterInfo();
    
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const query = params['q'] || '';
        this.searchQuery.set(query);
        this.searchControl.setValue(query);
        this.currentSearchTerm.set(query);
        if (query.length > 0) {
          this.performUnifiedSearch(query);
        }
      });

    // Setup search control with debounce
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(value => {
        this.currentSearchTerm.set(value || '');
      });

    // Subscribe to global filter changes
    this.globalFilterService.activeOrgUnitId$
      .pipe(takeUntil(this.destroy$))
      .subscribe(activeOrgUnitId => {
        this.isGlobalFilterActive.set(activeOrgUnitId !== null || this.hasOtherActiveFilters());
        if (activeOrgUnitId) {
          this.loadOrgUnitName(activeOrgUnitId);
        } else {
          this.activeOrgUnitName.set('');
        }
        this.updateActiveFilterLabels();
      });

    // Subscribe to global filter changes (when filters are saved)
    this.globalFilterService.filtersChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        // Reload global filter information
        this.loadGlobalFilterInfo();
        
        // Refresh search results if we have a search query
        const currentQuery = this.searchQuery();
        if (currentQuery && currentQuery.length > 0) {
          this.performUnifiedSearch(currentQuery);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Enhanced search using the unified endpoint
  private performUnifiedSearch(term: string): void {
    if (!term || term.length < 2) {
      this.clearResults();
      return;
    }

    this.isLoading.set(true);

    // Call the unified search endpoint without the 3-result limit
    const filterActive = !this.isFilterTemporarilyDisabled();
    this.http.get<SearchResponse>('/api/global/search', {
      params: { 
        q: term, 
        fullResults: 'true',
        filterActive: filterActive.toString()
      }
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (response) => {
        this.processUnifiedSearchResults(response);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching unified search results', err);
        this.isLoading.set(false);
        this.clearResults();
      }
    });
  }

  private processUnifiedSearchResults(response: SearchResponse): void {
    this.clearMetadataCache(); // Clear cache when new results are loaded
    this.searchResponse = response;
    this.buildEntityTabs();
  }

  private buildEntityTabs(): void {
    if (!this.searchResponse) return;

    const tabs: EntityTab[] = [];

    // Add entity-specific tabs only (no "All" tab)
    Object.entries(this.searchResponse.results).forEach(([entityType, results]) => {
      if (results.length > 0) {
        tabs.push({
          key: entityType,
          label: this.capitalizeFirstLetter(entityType),
          count: results.length,
          icon: this.getEntityIcon(entityType),
          color: this.getEntityColor(entityType)
        });
      }
    });

    this.entityTabs = tabs;
    
    // Set active tab to first available entity type
    if (tabs.length > 0) {
      this.activeTabKey = tabs[0].key;
    }
  }

  private clearResults(): void {
    this.searchResponse = null;
    this.entityTabs = [];
    this.activeTabKey = 'all';
    this.clearMetadataCache(); // Clear cache when clearing results
  }

  // Clear metadata cache for performance optimization
  private clearMetadataCache(): void {
    this.metadataCache.clear();
    this.metadataPropertiesCache.clear();
  }

  // Load global filter information
  private loadGlobalFilterInfo(): void {
    // Get current user ID
    this.authService.user().subscribe({
      next: (claims) => {
        const userIdClaim = claims.find(c => c.type === 'userId');
        if (userIdClaim) {
          this.currentUserId.set(userIdClaim.value);
          
          // Load user's global filters
                        this.userPreferenceService.getGlobalFilters(userIdClaim.value)
                .pipe(takeUntil(this.destroy$))
                .subscribe({
                  next: (filters) => {
                    this.globalFilters.set(filters);
                    
                    // Update global filter active status
                    this.isGlobalFilterActive.set(this.hasOtherActiveFilters());
                    
                    this.updateActiveFilterLabels();
                  },
                  error: (error) => {
                    console.error('Error loading global filters:', error);
                  }
                });
        }
      },
      error: (error) => {
        console.error('Error getting user claims:', error);
      }
    });
  }

  // Load organization unit name
  private loadOrgUnitName(orgUnitId: number): void {
    this.organizationHierarchyService.getOrganizationHierarchy()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (hierarchy) => {
          const orgUnit = this.findOrgUnitInHierarchy(hierarchy, orgUnitId);
          if (orgUnit) {
            this.activeOrgUnitName.set(orgUnit.data.name);
          } else {
            this.activeOrgUnitName.set(`Org Unit ${orgUnitId}`);
          }
        },
        error: (error) => {
          console.error('Error loading organization hierarchy:', error);
          this.activeOrgUnitName.set(`Org Unit ${orgUnitId}`);
        }
      });
  }

  // Helper method to find org unit in hierarchy
  private findOrgUnitInHierarchy(nodes: any[], orgUnitId: number): any {
    for (const node of nodes) {
      if (node.data && node.data.id === orgUnitId) {
        return node;
      }
      if (node.children && node.children.length > 0) {
        const found = this.findOrgUnitInHierarchy(node.children, orgUnitId);
        if (found) return found;
      }
    }
    return null;
  }

  // Update active filter labels for display
  private updateActiveFilterLabels(): void {
    const labels: string[] = [];
    const filters = this.globalFilters();
    
    // Add org unit filter - show the org unit name or "All organizational units" if it's the root/null
    const activeOrgUnitId = this.globalFilterService.getActiveOrgUnitId();
    if (activeOrgUnitId !== null) {
      if (this.activeOrgUnitName()) {
        labels.push(this.activeOrgUnitName());
      } else {
        // If we have an org unit ID but no name yet, show a placeholder
        labels.push(`Org Unit ${activeOrgUnitId}`);
      }
    } else if (filters && filters.orgUnitId !== null && filters.orgUnitId !== undefined) {
      // Handle case where filters show an org unit but service doesn't have it yet
      if (this.activeOrgUnitName()) {
        labels.push(this.activeOrgUnitName());
      } else {
        labels.push(`Org Unit ${filters.orgUnitId}`);
      }
    }
    
    if (filters) {
      // Add related to me filter
      if (filters.relatedToMe) {
        labels.push('Related to Me');
      }
      
      // Add date filters
      if (filters.dateOn) {
        labels.push(`Date: ${new Date(filters.dateOn).toLocaleDateString()}`);
      } else if (filters.dateFrom || filters.dateTo) {
        const from = filters.dateFrom ? new Date(filters.dateFrom).toLocaleDateString() : '';
        const to = filters.dateTo ? new Date(filters.dateTo).toLocaleDateString() : '';
        if (from && to) {
          labels.push(`Date: ${from} - ${to}`);
        } else if (from) {
          labels.push(`Date: from ${from}`);
        } else if (to) {
          labels.push(`Date: until ${to}`);
        }
      }
    }
    
    this.activeFilterLabels.set(labels);
    
    // Update the global filter active state based on whether we have any filters
    const hasActiveFilters = activeOrgUnitId !== null || this.hasOtherActiveFilters();
    this.isGlobalFilterActive.set(hasActiveFilters);
  }

  private loadAllEntityColumns(): void {
    this.columnsLoading.set(true);
    
    // Load Contact columns
    this.entityConfigurationService.getEntityListViewConfiguration('Contact')
      .subscribe({
        next: (columns) => {
          this.contactColumns.set(this.processColumns(columns, 'Contact'));
        },
        error: (error) => {
          console.error('Failed to load contact columns:', error);
        }
      });
    
    // Load Partner columns
    this.entityConfigurationService.getEntityListViewConfiguration('Partner')
      .subscribe({
        next: (columns) => {
          this.partnerColumns.set(this.processColumns(columns, 'Partner'));
        },
        error: (error) => {
          console.error('Failed to load partner columns:', error);
        }
      });
    
    // Load Interaction columns
    this.entityConfigurationService.getEntityListViewConfiguration('Interaction')
      .subscribe({
        next: (columns) => {
          this.interactionColumns.set(this.processColumns(columns, 'Interaction'));
          this.columnsLoading.set(false);
        },
        error: (error) => {
          console.error('Failed to load interaction columns:', error);
          this.columnsLoading.set(false);
        }
      });
  }

  private processColumns(columns: any[], entityType: string): ListViewColumn[] {
    return columns.map(col => {
      const processedColumn: ListViewColumn = {
        field: col.field,
        label: col.label,
        type: col.type,
        sortable: col.sortable,
        width: col.width,
        ellipsis: col.ellipsis,
        helperText: col.helperText
      };

      // Handle nested field paths for template functions
      if (col.field && col.field.includes('.') && col.type !== 'template') {
        processedColumn.templateFn = (rowData: any) => {
          const value = this.getNestedProperty(rowData, col.field);
          return value !== undefined && value !== null ? String(value) : '';
        };
        processedColumn.type = 'template';
      }

      // Add template function for template type columns
      if (col.type === 'template' && col.templatePattern) {
        processedColumn.templateFn = this.createTemplateFunction(col.templatePattern);
      }

      // Handle interaction type columns - ensure interaction icons are displayed
      if (entityType === 'Interaction' && col.field === 'type') {
        processedColumn.type = 'interactionIcon';
      }
      
      // Handle any column that might need interaction icon treatment
      if (entityType === 'Interaction' && (col.field === 'interactionType' || col.field === 'Type')) {
        processedColumn.type = 'interactionIcon';
      }

      return processedColumn;
    });
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((o, p) => o?.[p], obj);
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;
      
      // Replace field placeholders like {name}, {shortName} with actual values
      const fieldMatches = templatePattern.match(/\{([^}]+)\}/g);
      if (fieldMatches) {
        fieldMatches.forEach(match => {
          const fieldName = match.replace(/[{}]/g, '');
          const fieldValue = this.getNestedProperty(rowData, fieldName) || '';
          result = result.replace(match, fieldValue);
        });
      }
      
      return result.trim();
    };
  }

  // Get current results based on active tab
  get currentResults(): EnhancedSearchResult[] {
    if (!this.searchResponse) return [];
    
    return this.searchResponse.results[this.activeTabKey] || [];
  }

  // Get columns for the active tab
  get currentColumns(): ListViewColumn[] {
    switch (this.activeTabKey) {
      case 'contacts':
        return this.contactColumns();
      case 'partners':
        return this.partnerColumns();
      case 'interactions':
        return this.interactionColumns();
      default:
        return [];
    }
  }

  // Card configuration for search results
  get cardConfig(): ListViewConfig {
    return {
      pageSize: 50,
      enablePagination: false,
      enableSorting: false,
      enableSearch: false,
      enableExport: false,
      defaultViewMode: 'card',
      showViewModeToggle: false,
      autoSwitchToCardView: false,
      forceMobileMode: false,  // Allow responsive behavior like contact/partner pages
      searchMetadata: {
        enabled: true,
        defaultVisible: this.showSearchMetadata(),
        extractMetadata: (item: any) => item._searchMetadata,
        searchQuery: this.searchQuery()
      }
    };
  }



  // Helper methods for tabs
  selectTab(tabKey: string): void {
    this.activeTabKey = tabKey;
  }

  // Capitalize the first letter of a string
  capitalizeFirstLetter(str: string): string {
    if (!str) return '';
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  private getEntityIcon(entityType: string): string {
    switch (entityType) {
      case 'contacts': return 'contacts';
      case 'partners': return 'corporate_fare';
      case 'interactions': return 'chat';
      default: return 'help';
    }
  }

  private getEntityColor(entityType: string): string {
    switch (entityType) {
      case 'contacts': return 'blue';
      case 'partners': return 'green';
      case 'interactions': return 'purple';
      default: return 'gray';
    }
  }

  // Navigation methods
  onCardClick(result: EnhancedSearchResult): void {
    // Navigate based on the active tab (entity type) instead of result.type
    if (this.activeTabKey === 'contacts') {
      this.router.navigate(['/partnerships/contacts', result.id]);
    } else if (this.activeTabKey === 'partners') {
      this.router.navigate(['/partnerships/partners', result.id]);
    } else if (this.activeTabKey === 'interactions') {
      this.router.navigate(['/partnerships/interactions', result.id]);
    }
  }

  // Toggle filter functionality
  toggleGlobalFilter(): void {
    const currentlyDisabled = this.isFilterTemporarilyDisabled();
    
    if (currentlyDisabled) {
      // Re-enable filters
      this.isFilterTemporarilyDisabled.set(false);
      this.globalFilterService.setFilterEnabled(true);
    } else {
      // Temporarily disable filters
      this.isFilterTemporarilyDisabled.set(true);
      this.globalFilterService.setFilterEnabled(false);
    }
    
    // Trigger search with new filter state
    const currentQuery = this.searchQuery();
    if (currentQuery) {
      this.performSearch(currentQuery);
    }
  }
  
  // Check if we should show filter controls
  shouldShowFilterToggle(): boolean {
    // Show toggle if there are active filters OR if filters are temporarily disabled
    return this.isGlobalFilterActive() && (this.activeFilterLabels().length > 0 || this.isFilterTemporarilyDisabled());
  }
  
  // Get display text for toggle button
  getToggleButtonText(): string {
    return this.isFilterTemporarilyDisabled() 
      ? this.translateService.instant('search.applyFilter')
      : this.translateService.instant('search.showAll');
  }
  
  // Get record count display text
  getRecordCountText(): string {
    const currentCount = this.getTotalResultsCount();
    
    if (this.isFilterTemporarilyDisabled() || !this.isGlobalFilterActive()) {
      return this.translateService.instant('search.showingAllRecords', { total: currentCount });
    } else {
      // When filters are active, we show the filtered count
      return this.translateService.instant('search.showingAllRecords', { total: currentCount });
    }
  }

  // Open global filters dialog
  openGlobalFiltersDialog(): void {
    this.globalFiltersDialogService.openDialog();
  }

  // Optimized metadata helper methods with memoization
  hasSearchMetadata(result: EnhancedSearchResult): boolean {
    const cacheKey = `metadata_${result.id}`;
    if (this.metadataCache.has(cacheKey)) {
      return this.metadataCache.get(cacheKey)!;
    }
    
    const hasMetadata = !!result._searchMetadata;
    this.metadataCache.set(cacheKey, hasMetadata);
    return hasMetadata;
  }

  hasSearchType(metadata?: SearchMetadata): boolean {
    if (!metadata) return false;
    const cacheKey = `searchType_${JSON.stringify(metadata)}`;
    if (this.metadataPropertiesCache.has(cacheKey)) {
      return this.metadataPropertiesCache.get(cacheKey);
    }
    
    const hasType = !!metadata.searchType;
    this.metadataPropertiesCache.set(cacheKey, hasType);
    return hasType;
  }

  hasMatchedField(metadata?: SearchMetadata): boolean {
    if (!metadata) return false;
    const cacheKey = `matchedField_${JSON.stringify(metadata)}`;
    if (this.metadataPropertiesCache.has(cacheKey)) {
      return this.metadataPropertiesCache.get(cacheKey);
    }
    
    const hasField = !!metadata.matchedField;
    this.metadataPropertiesCache.set(cacheKey, hasField);
    return hasField;
  }

  hasScore(metadata?: SearchMetadata): boolean {
    if (!metadata) return false;
    const cacheKey = `score_${JSON.stringify(metadata)}`;
    if (this.metadataPropertiesCache.has(cacheKey)) {
      return this.metadataPropertiesCache.get(cacheKey);
    }
    
    const hasScore = !!metadata.score;
    this.metadataPropertiesCache.set(cacheKey, hasScore);
    return hasScore;
  }

  hasSnippet(metadata?: SearchMetadata): boolean {
    if (!metadata) return false;
    const cacheKey = `snippet_${JSON.stringify(metadata)}`;
    if (this.metadataPropertiesCache.has(cacheKey)) {
      return this.metadataPropertiesCache.get(cacheKey);
    }
    
    const hasSnippet = !!metadata.snippet;
    this.metadataPropertiesCache.set(cacheKey, hasSnippet);
    return hasSnippet;
  }

  getSearchTypeLabel(metadata?: SearchMetadata): string {
    if (!metadata?.searchType) return '';
    
    switch (metadata.searchType) {
      case 'semantic-search': return 'AI Search';
      case 'field-search': return 'Exact Match';
      default: return 'Search';
    }
  }

  getSearchTypeColor(metadata?: SearchMetadata): string {
    if (!metadata?.searchType) return 'gray';
    
    switch (metadata.searchType) {
      case 'semantic-search': return 'purple';
      case 'field-search': return 'green';
      default: return 'gray';
    }
  }

  getMatchedField(metadata?: SearchMetadata): string {
    return metadata?.matchedField || '';
  }

  getScorePercentage(metadata?: SearchMetadata): number {
    if (!metadata?.score) return 0;
    
    // Convert score to percentage and cap at 100%
    // PostgreSQL similarity scores can vary in range, so we normalize them
    const percentage = Math.round(metadata.score * 100);
    return Math.min(percentage, 100);
  }

  getSnippet(metadata?: SearchMetadata): string {
    return metadata?.snippet || '';
  }

  highlightSearchTerms(text: string, searchTerm: string): string {
    if (!text || !searchTerm) return text;
    
    // Escape special regex characters in search term
    const escapedSearchTerm = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    
    // Split search term into individual words for better highlighting
    const words = escapedSearchTerm.split(/\s+/).filter(word => word.length > 0);
    
    let highlightedText = text;
    
    // Highlight each word separately
    words.forEach(word => {
      if (word.length > 1) { // Only highlight words with 2+ characters
        const regex = new RegExp(`(${word})`, 'gi');
        highlightedText = highlightedText.replace(regex, '<span class="search-highlight">$1</span>');
      }
    });
    
    return highlightedText;
  }

  // Helper method to get total results count
  getTotalResultsCount(): number {
    if (!this.searchResponse) return 0;
    return Object.values(this.searchResponse.results).flat().length;
  }

  // Helper method to get entity title for display
  getEntityTitle(entity: EnhancedSearchResult): string {
    if (this.activeTabKey === 'contacts') {
      const firstName = entity['firstName'] || '';
      const middleName = entity['middleName'] || '';
      const lastName = entity['lastName'] || '';
      return [firstName, middleName, lastName].filter(n => n).join(' ') || 'Unknown Contact';
    } else if (this.activeTabKey === 'partners') {
      return entity['name'] || entity['shortName'] || 'Unknown Partner';
    } else if (this.activeTabKey === 'interactions') {
      return entity['title'] || entity['subject'] || 'Unknown Interaction';
    }
    return 'Unknown Entity';
  }



  // Perform search from the page search bar
  performSearch(query: string): void {
    if (query && query.length > 0) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { q: query },
        queryParamsHandling: 'merge'
      });
    }
  }

  // Clear search
  clearSearch(): void {
    this.searchControl.setValue('');
    this.currentSearchTerm.set('');
  }

  // Go to results page (if needed for navigation)
  goToResultsPage(): void {
    const query = this.searchControl.value;
    if (query) {
      this.performSearch(query);
    }
  }

  // TrackBy functions for better performance
  trackByResultId(index: number, result: EnhancedSearchResult): number {
    return result.id || index;
  }

  trackByTabKey(index: number, tab: EntityTab): string {
    return tab.key;
  }

  // Check if any global filters are active
  private hasOtherActiveFilters(): boolean {
    const filters = this.globalFilters();
    if (!filters) return false;
    
    return !!(
      filters.orgUnitId ||
      filters.relatedToMe ||
      filters.dateOn ||
      filters.dateFrom ||
      filters.dateTo
    );
  }
}
