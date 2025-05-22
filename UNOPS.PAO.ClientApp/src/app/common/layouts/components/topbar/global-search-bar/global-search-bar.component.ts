import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, HostListener, inject, signal, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, filter, Subject, takeUntil, forkJoin } from 'rxjs';
import { DialogService } from 'primeng/dynamicdialog';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ContactService } from '../../../../../features/internal/services/contact.service';
import { PartnerService } from '../../../../../features/internal/services/partner.service';

interface SearchResult {
  id: string;
  title: string;
  subtitle?: string;
  type: string;
  [key: string]: any;
}

@Component({
  selector: 'app-global-search-bar',
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './global-search-bar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  styles: `:host {
    display: block;
    width: 100%;
    max-width: 740px;
  }`

})
export class GlobalSearchBarComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);
  private contactService = inject(ContactService);
  private partnerService = inject(PartnerService);

  translateService = inject(TranslateService);
  
  searchControl = new FormControl('');
  showResults = false;
  isExpanded = false;
  recentSearches: string[] = [];
  filteredResults: SearchResult[] = [];
  isLoading = signal(false);
  
  allResults: SearchResult[] = [];
  
  private destroy$ = new Subject<void>();
  
  @ViewChild('searchContainer') searchContainer!: ElementRef;
  
  ngOnInit(): void {
    // Load recent searches from localStorage
    this.loadRecentSearches();
    
    // Read query parameters from URL
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.searchControl.setValue(params['q']);
      });
    
    // Subscribe to search input changes
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      filter(term => term !== null),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      if (term && term.length > 2) {
        this.fetchSearchResults(term as string);
      } else {
        this.filteredResults = [];
      }
    });

    // Set initial expanded state based on screen size
    this.checkScreenSize();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  onSearchFocus(): void {
    this.showResults = true;
    if (this.searchControl.value) {
      this.fetchSearchResults(this.searchControl.value);
    }
  }

  
  toggleExpand(): void {
    this.isExpanded = true;
    // When expanded, also show the search focus
    setTimeout(() => {
      this.onSearchFocus();
    }, 100);
  }

  closeSearch(): void {
    // On mobile, collapse the search bar
    if (window.innerWidth < 768) {
      this.isExpanded = false;
    }
    this.clearSearch();
  }
  
  clearSearch(): void {
    this.searchControl.setValue('');
    this.showResults = false;
  }
  
  selectSearchItem(term: string): void {
    this.searchControl.setValue(term);
    this.goToResultsPage();
  }
  
  selectResult(result: SearchResult): void {
    this.addToRecentSearches(result.title);
    this.clearSearch();
    // Navigate to the appropriate detail page based on result type
    if (result.type === 'contact') {
      this.router.navigate(['/partnerships/contacts', result.id]);
    } else if (result.type === 'partner') {
      this.router.navigate(['/partner', result.id]);
    }
    this.clearSearch();
  }
  
  goToResultsPage(): void {
    const currentSearchTerm = this.searchControl.value || '';
    
    // Only navigate if we have an active search term
    if (currentSearchTerm.length > 0) {
      // Add to recent searches
      this.addToRecentSearches(currentSearchTerm);
      this.clearSearch();
      // // Navigate to search page with query parameter
      this.router.navigate(['/search'], { 
        queryParams: { q: currentSearchTerm }
      });
    }
  }
  
  getInitials(name: string): string {
    return name
      .split(' ')
      .map(part => part.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }
  
  private fetchSearchResults(term: string): void {
    if (!term || term.length < 2) {
      this.filteredResults = [];
      return;
    }
    
    this.isLoading.set(true);

    // Create params for API calls
    const params = new HttpParams()
      .set('pageIndex', '1')
      .set('pageSize', '5')
      .set('searchText', term);

    // Make two parallel API calls
    forkJoin({
      contacts: this.http.get(this.contactService.getClassicSearchUrl(), { params }),
      partners: this.http.get(this.partnerService.getClassicSearchUrl(), { params })
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (results) => {
        this.processSearchResults(results, term);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching search results', err);
        this.isLoading.set(false);
        this.filteredResults = [];
      }
    });
  }

  private processSearchResults(results: any, term: string): void {
    const contactResults = results.contacts?.records || [];
    const partnerResults = results.partners?.records || [];
    
    // Map contact results to SearchResult format
    const mappedContacts: SearchResult[] = contactResults.map((contact: any) => ({
      id: contact.id,
      title: `${contact.firstName} ${contact.lastName}`,
      subtitle: contact.email || contact.mobile,
      type: 'contact',
      ...contact
    }));
    
    // Map partner results to SearchResult format
    const mappedPartners: SearchResult[] = partnerResults.map((partner: any) => ({
      id: partner.id,
      title: partner.name,
      subtitle: partner.type || '',
      type: 'partner',
      ...partner
    }));
    
    // Combine and limit to top 5 results
    this.allResults = [...mappedContacts, ...mappedPartners];
    this.filteredResults = this.allResults.slice(0, 5);
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
  
  private addToRecentSearches(term: string): void {
    // Remove if already exists (to bring to top)
    this.recentSearches = this.recentSearches.filter(t => t !== term);
    
    // Add to beginning of array
    this.recentSearches.unshift(term);
    
    // Keep only the most recent 5 searches
    this.recentSearches = this.recentSearches.slice(0, 5);
    
    // Save to localStorage
    try {
      localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
    } catch (e) {
      console.error('Failed to save recent searches', e);
    }
  }

  @HostListener('window:resize')
  checkScreenSize(): void {
    // Auto-expand on larger screens
    if (window.innerWidth >= 768) {
      this.isExpanded = true;
    } else if (!this.searchControl.value) {
      // On small screens, collapse if no search text
      this.isExpanded = false;
    }
  }

  @HostListener('document:click', ['$event'])
  handleOutsideClick(event: MouseEvent): void {
    if (this.showResults && this.searchContainer && !this.searchContainer.nativeElement.contains(event.target)) {
      this.showResults = false;
    }
  }
}
