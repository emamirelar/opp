import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, HostListener, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, filter, Subject, takeUntil } from 'rxjs';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { ListviewCardComponent } from '../../../../pages/components/listview/card/listview-card.component';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { EntityConfigurationService } from '../../../../../features/internal/services/entity-configuration.service';
import { ListViewColumn, ListViewConfig } from '../../../../pages/components/listview/listview.model';

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
  selector: 'app-global-search-bar',
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, ListviewCardComponent],
  templateUrl: './global-search-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  styles: `
    :host {
      display: block;
      width: 100%;
      max-width: 740px;
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
    
    .entity-tab {
      transition: all 0.2s ease;
    }
    
    .entity-tab:hover {
      transform: translateY(-1px);
    }
    
    .snippet-text {
      line-height: 1.4;
      max-height: 2.8em;
      overflow: hidden;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
    }
    
    .mobile-dropdown {
      max-height: 200px;
      overflow-y: auto;
    }
    
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    .fade-in {
      animation: fadeIn 0.3s ease;
    }
  `
})
export class GlobalSearchBarComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private entityConfigurationService = inject(EntityConfigurationService);
  private readonly breakpoint = 992;

  translateService = inject(TranslateService);

  searchControl = new FormControl('');
  showResults = false;
  isExpanded = false;
  recentSearches: string[] = [];
  isLoading = signal(false);
  private isUserInteracting = false;

  // Enhanced search state
  searchResponse: SearchResponse | null = null;
  entityTabs: EntityTab[] = [];
  activeTabKey: string = 'all';
  showMobileDropdown = signal(false);
  
  // Entity columns loaded from configuration service
  contactColumns = signal<ListViewColumn[]>([]);
  partnerColumns = signal<ListViewColumn[]>([]);
  interactionColumns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(false);
  
  // Current results based on active tab
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
      pageSize: 3,
      enablePagination: false,
      enableSorting: false,
      enableSearch: false,
      enableExport: false,
      defaultViewMode: 'card',
      showViewModeToggle: false,
      autoSwitchToCardView: false,
      forceMobileMode: false
    };
  }

  // Check if we're on mobile
  get isMobile(): boolean {
    return window.innerWidth < this.breakpoint;
  }

  private destroy$ = new Subject<void>();

  @ViewChild('searchContainer') searchContainer!: ElementRef;

  ngOnInit(): void {
    this.loadRecentSearches();
    this.loadAllEntityColumns();

    // Read query parameters from URL
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        if (params['q']) {
          this.searchControl.setValue(params['q']);
        }
      });

    // Subscribe to search input changes
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter(term => term !== null),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      if (term && term.length > 2) {
        this.performUnifiedSearch(term as string);
      } else {
        this.clearResults();
      }
    });

    this.checkScreenSize();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchFocus(): void {
    this.isUserInteracting = true;
    this.showResults = true;
    
    if (this.searchControl.value && this.searchControl.value.length > 2) {
      this.performUnifiedSearch(this.searchControl.value);
    }
    
    setTimeout(() => {
      this.isUserInteracting = false;
    }, 300);
  }

  onKeydown(event: KeyboardEvent): void {
    this.isUserInteracting = true;
    
    setTimeout(() => {
      this.isUserInteracting = false;
    }, 500);
  }

  onTouchStart(): void {
    this.isUserInteracting = true;
    
    setTimeout(() => {
      this.isUserInteracting = false;
    }, 300);
  }

  toggleExpand(): void {
    this.isExpanded = true;
    setTimeout(() => {
      this.onSearchFocus();
    }, 100);
  }

  closeSearch(): void {
    if (this.isMobile) {
      this.isExpanded = false;
    }
    this.showResults = false;
    this.showMobileDropdown.set(false);
    this.clearSearch();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    this.clearResults();
  }

  private clearResults(): void {
    this.searchResponse = null;
    this.entityTabs = [];
    this.activeTabKey = 'all';
    this.showResults = false;
  }

  selectSearchItem(term: string): void {
    this.searchControl.setValue(term);
    this.goToResultsPage();
  }

  selectResult(result: EnhancedSearchResult): void {
    // Use the entity's title for recent searches
    const entityName = this.getEntityTitle(result);
    this.addToRecentSearches(entityName);
    this.clearResults();
    
    if (this.isMobile) {
      this.isExpanded = false;
    }

    // Navigate based on the active tab (entity type) instead of result.type
    if (this.activeTabKey === 'contacts') {
      this.router.navigate(['/partnerships/contacts', result.id]);
    } else if (this.activeTabKey === 'partners') {
      this.router.navigate(['/partner', result.id]);
    } else if (this.activeTabKey === 'interactions') {
      this.router.navigate(['/interactions', result.id]);
    }
  }

  goToResultsPage(): void {
    const currentSearchTerm = this.searchControl.value || '';

    if (currentSearchTerm.length > 0) {
      this.addToRecentSearches(currentSearchTerm);
      this.clearResults();
      
      if (this.isMobile) {
        this.isExpanded = false;
      }
      
      this.router.navigate(['/search'], {
        queryParams: { q: currentSearchTerm }
      });
    }
  }

  // Enhanced search using the new unified endpoint
  private performUnifiedSearch(term: string): void {
    if (!term || term.length < 2) {
      this.clearResults();
      return;
    }

    this.isLoading.set(true);

    // Call the new unified search endpoint
    this.http.get<SearchResponse>('/api/global/search', {
      params: { q: term }
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
    this.searchResponse = response;
    this.buildEntityTabs();
    
    // Show results if we have any
    if (Object.keys(response.results).length > 0) {
      this.showResults = true;
    }
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

  selectTab(tabKey: string): void {
    this.activeTabKey = tabKey;
    this.showMobileDropdown.set(false);
  }

  toggleMobileDropdown(): void {
    this.showMobileDropdown.set(!this.showMobileDropdown());
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

  getInitials(name: string): string {
    if (!name) return 'N/A';
    return name
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  // Get search type indicator
  getSearchTypeLabel(metadata?: SearchMetadata): string {
    if (!metadata?.searchType) return '';
    
    switch (metadata.searchType) {
      case 'semantic-search': return 'AI Search';
      case 'field-search': return 'Exact Match';
      default: return 'Search';
    }
  }

  // Get search type color
  getSearchTypeColor(metadata?: SearchMetadata): string {
    if (!metadata?.searchType) return 'gray';
    
    switch (metadata.searchType) {
      case 'semantic-search': return 'purple';
      case 'field-search': return 'green';
      default: return 'gray';
    }
  }

  // Highlight search terms in snippet
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

  // Get active tab details for mobile dropdown
  get activeTabDetails(): EntityTab | undefined {
    return this.entityTabs.find(t => t.key === this.activeTabKey);
  }

  // Get entity type display name (without 's' and capitalized)
  getEntityTypeDisplayName(type: string): string {
    if (!type || typeof type !== 'string') return '';
    const singular = type.slice(0, -1);
    return this.capitalizeFirstLetter(singular);
  }

  // Capitalize the first letter of a string
  capitalizeFirstLetter(str: string): string {
    if (!str) return '';
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  // Check if result has search metadata
  hasSearchMetadata(result: EnhancedSearchResult): boolean {
    return !!result._searchMetadata;
  }

  // Check if metadata has search type
  hasSearchType(metadata?: SearchMetadata): boolean {
    return !!metadata?.searchType;
  }

  // Check if metadata has matched field
  hasMatchedField(metadata?: SearchMetadata): boolean {
    return !!metadata?.matchedField;
  }

  // Check if metadata has score
  hasScore(metadata?: SearchMetadata): boolean {
    return !!metadata?.score;
  }

  // Check if metadata has snippet
  hasSnippet(metadata?: SearchMetadata): boolean {
    return !!metadata?.snippet;
  }

  // Get matched field from metadata
  getMatchedField(metadata?: SearchMetadata): string {
    return metadata?.matchedField || '';
  }

  // Get score percentage
  getScorePercentage(metadata?: SearchMetadata): number {
    if (!metadata?.score) return 0;
    
    // Convert score to percentage and cap at 100%
    // PostgreSQL similarity scores can vary in range, so we normalize them
    const percentage = Math.round(metadata.score * 100);
    return Math.min(percentage, 100);
  }

  // Get snippet from metadata
  getSnippet(metadata?: SearchMetadata): string {
    return metadata?.snippet || '';
  }

  // Get result avatar CSS classes
  getResultAvatarClasses(resultType: string): string {
    switch (resultType) {
      case 'contacts': return 'bg-blue-100 text-blue-600';
      case 'partners': return 'bg-green-100 text-green-600';
      case 'interactions': return 'bg-purple-100 text-purple-600';
      default: return 'bg-gray-100 text-gray-600';
    }
  }

  // Get entity type badge CSS classes
  getEntityTypeBadgeClasses(resultType: string): string {
    switch (resultType) {
      case 'contacts': return 'bg-blue-100 text-blue-700';
      case 'partners': return 'bg-green-100 text-green-700';
      case 'interactions': return 'bg-purple-100 text-purple-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }

  // Check if active tab matches key
  isActiveTab(tabKey: string): boolean {
    return this.activeTabKey === tabKey;
  }

  // Get search type badge CSS classes
  getSearchTypeBadgeClasses(metadata?: SearchMetadata): string {
    const color = this.getSearchTypeColor(metadata);
    return `px-2 py-1 text-xs rounded-full bg-${color}-100 text-${color}-700`;
  }

  private loadRecentSearches(): void {
    try {
      const saved = localStorage.getItem('recentSearches');
      this.recentSearches = saved ? JSON.parse(saved) : [];
    } catch (e) {
      console.error('Failed to load recent searches', e);
      this.recentSearches = [];
    }
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

      // Handle interaction type columns
      if (entityType === 'Interaction' && col.field === 'type' && col.type === 'text') {
        processedColumn.type = 'interactionIcon';
      }

      return processedColumn;
    });
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current && current[key], obj);
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;
      const regex = /\{\{([^}]+)\}\}/g;
      return result.replace(regex, (match, fieldPath) => {
        const value = this.getNestedProperty(rowData, fieldPath.trim());
        return value !== undefined && value !== null ? String(value) : '';
      });
    };
  }

  private addToRecentSearches(term: string): void {
    this.recentSearches = this.recentSearches.filter(t => t !== term);
    this.recentSearches.unshift(term);
    this.recentSearches = this.recentSearches.slice(0, 5);

    try {
      localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
    } catch (e) {
      console.error('Failed to save recent searches', e);
    }
  }

  clearRecentSearches(): void {
    this.recentSearches = [];
    try {
      localStorage.removeItem('recentSearches');
    } catch (e) {
      console.error('Failed to clear recent searches', e);
    }
  }

  @HostListener('window:resize')
  checkScreenSize(): void {
    if (window.innerWidth >= this.breakpoint) {
      this.isExpanded = true;
      this.showMobileDropdown.set(false);
    } else if (!this.searchControl.value) {
      this.isExpanded = false;
    }
  }

  @HostListener('document:click', ['$event'])
  handleOutsideClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    
    if (this.searchContainer && !this.searchContainer.nativeElement.contains(target)) {
      this.showMobileDropdown.set(false);
      
      if (this.showResults) {
        this.showResults = false;
      }

      if (this.isMobile && this.isExpanded && !this.searchControl.value?.trim() && !this.isUserInteracting) {
        if (!target.matches('input, textarea, [contenteditable], button')) {
          setTimeout(() => {
            if (!this.searchControl.value?.trim() && !this.showResults && !this.isUserInteracting) {
              this.isExpanded = false;
            }
          }, 150);
        }
      }
    }
  }

  // Helper methods to extract display values from original entities
  getEntityTitle(entity: EnhancedSearchResult): string {
    if (this.activeTabKey === 'contacts') {
      const firstName = entity['firstName'] || '';
      const lastName = entity['lastName'] || '';
      return `${firstName} ${lastName}`.trim() || 'Unknown Contact';
    } else if (this.activeTabKey === 'partners') {
      return entity['name'] || 'Unknown Partner';
    } else if (this.activeTabKey === 'interactions') {
      return entity['subject'] || entity['description'] || 'Unknown Interaction';
    }
    return 'Unknown';
  }

  getEntitySubtitle(entity: EnhancedSearchResult): string | null {
    if (this.activeTabKey === 'contacts') {
      const title = entity['title'] || '';
      const partnerName = entity['partner']?.['name'] || '';
      if (title && partnerName) {
        return `${title} at ${partnerName}`;
      }
      return title || partnerName || null;
    } else if (this.activeTabKey === 'partners') {
      const categoryName = entity['partnerCategoryName'] || '';
      const groupName = entity['partnerGroupName'] || '';
      const officeName = entity['partnerOffice']?.['name'] || '';
      const city = entity['address1City'] || '';
      const country = entity['address1Country'] || '';
      
      const type = categoryName || groupName;
      const location = officeName || (city && country ? `${city}, ${country}` : city || country);
      
      if (type && location) {
        return `${type} • ${location}`;
      }
      return type || location || null;
    } else if (this.activeTabKey === 'interactions') {
      const interactionType = entity['type'] || '';
      const date = entity['date'] ? new Date(entity['date']).toLocaleDateString() : '';
      return date ? `${interactionType} • ${date}` : interactionType;
    }
    return null;
  }

  getEntityInitials(entity: EnhancedSearchResult): string {
    const title = this.getEntityTitle(entity);
    if (!title) return 'N/A';
    return title
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  getEntityDisplayName(entityType: string): string {
    return this.capitalizeFirstLetter(entityType.slice(0, -1)); // Remove 's' and capitalize
  }
}
