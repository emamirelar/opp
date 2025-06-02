import { Component, ElementRef, OnInit, ViewChild, inject, signal, effect, Signal } from '@angular/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { DatePickerModule } from 'primeng/datepicker';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { DatePipe } from '@angular/common';
import { InteractionService } from '../../../../../services/interaction.service';
import { InteractionModalComponent } from '../../../../interaction/modal/interaction-modal.component';
import { DialogService } from 'primeng/dynamicdialog';
import { ContactViewInteractionsItemComponent } from '../item/contact-view-interactions-item.component';
import { GroupedInteraction, InteractionViewModel } from '../interaction-view.model';
import { map } from 'rxjs/operators';
import { Interaction as InteractionModel } from '../../../../../models/interaction.model';
import { InteractionType } from '../../../../../models/interaction-type.enum';
import { InteractionFilterParams } from '../../../../../models/interaction-filter-params.model';

@Component({
  selector: 'app-contact-view-interactions-dialog',
  standalone: true,
  imports: [
    DialogModule,
    InputTextModule,
    FormsModule,
    DropdownModule,
    DatePickerModule,
    ProgressSpinnerModule,
    ButtonModule,
    TooltipModule,
    IconFieldModule,
    InputIconModule,
    DatePipe,
    ContactViewInteractionsItemComponent
  ],
  providers: [DialogService],
  templateUrl: './contact-view-interactions-dialog.component.html'
})
export class ContactViewInteractionsDialogComponent implements OnInit {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  private dialogConfig = inject(DynamicDialogConfig);
  private dialogService = inject(DialogService);
  private interactionService = inject(InteractionService);

  // Data properties
  contactId?: string;
  searchText = signal('');
  searchTextModel = ''; // For ngModel binding
  dateRange: Date[] = [];
  selectedTypeFilter: InteractionType | null = null;
  isLoading = signal<boolean>(false);
  currentPage = 0;
  itemsPerPage = 10;
  hasMoreData = true;
  totalCount = 0;

  // Debounce timer
  private searchDebounceTimer?: any;

  // Type filter options
  typeFilterOptions = Object.values(InteractionType).map(type => ({
    label: type,
    value: type
  }));

  // Interactions data
  interactions: InteractionViewModel[] = [];
  groupedInteractions: GroupedInteraction[] = [];

  constructor() {
    // Effect to watch for search text changes
    effect(() => {
      // Get the current search text value from the signal
      const currentSearchText = this.searchText();
      
      // Trigger search with debounce
      this.debounceSearch(currentSearchText);
    });
  }

  ngOnInit() {
    // Get contactId from dialog config
    this.contactId = this.dialogConfig.data?.contactId;

    if (this.contactId) {
      this.loadData();
    }
  }

  // Debounce mechanism for search
  private debounceSearch(text: string): void {
    // Clear any existing timer
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }

    // Set a new timer to execute the search after 500ms
    this.searchDebounceTimer = setTimeout(() => {
      this.loadData();
    }, 500);
  }
  
  loadData(): void {
    if (!this.contactId) return;
    
    this.isLoading.set(true);
    this.currentPage = 0;
    this.hasMoreData = true;

    const filterParams: InteractionFilterParams = {
      contactId: Number(this.contactId),
      pageIndex: this.currentPage,
      pageSize: this.itemsPerPage,
      orderBy: 'date',
      ascending: 'false'
    };

    // Add type filter if selected
    if (this.selectedTypeFilter) {
      filterParams.type = this.selectedTypeFilter;
    }

    // Add date range filters if selected
    if (this.dateRange && this.dateRange.length === 2) {
      filterParams.fromDate = this.formatDateForApi(this.dateRange[0]);
      filterParams.toDate = this.formatDateForApi(this.dateRange[1], true);
    }

    // Add search text if provided
    const currentSearchText = this.searchText();
    if (currentSearchText && currentSearchText.trim() !== '') {
      filterParams.searchText = currentSearchText.trim();
    }

    this.interactionService.getAll(filterParams)
      .pipe(
        map(response => ({
          records: response.body?.records.map(i => this.mapToViewModel(i)) || [],
          totalCount: response.body?.totalCount || 0
        }))
      )
      .subscribe(data => {
        this.interactions = data.records;
        this.totalCount = data.totalCount;
        this.hasMoreData = this.interactions.length < data.totalCount;
        this.updateGroupedInteractions();
        this.isLoading.set(false);
      });
  }

  // This method triggers immediately for non-search filters
  onFilterChange(): void {
    this.loadData();
  }

  // Method for handling search text input
  onSearchTextChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTextModel = value;
    this.searchText.set(value);
  }

  loadMoreData(): void {
    if (!this.hasMoreData || this.isLoading() || !this.contactId) return;

    this.isLoading.set(true);
    this.currentPage++;

    const filterParams: InteractionFilterParams = {
      contactId: Number(this.contactId),
      pageIndex: this.currentPage,
      pageSize: this.itemsPerPage,
      orderBy: 'date',
      ascending: 'false'
    };

    // Add type filter if selected
    if (this.selectedTypeFilter) {
      filterParams.type = this.selectedTypeFilter;
    }

    // Add date range filters if selected
    if (this.dateRange && this.dateRange.length === 2) {
      filterParams.fromDate = this.formatDateForApi(this.dateRange[0]);
      filterParams.toDate = this.formatDateForApi(this.dateRange[1], true);
    }

    // Add search text if provided
    const currentSearchText = this.searchText();
    if (currentSearchText && currentSearchText.trim() !== '') {
      filterParams.searchText = currentSearchText.trim();
    }

    this.interactionService.getAll(filterParams)
      .pipe(
        map(response => ({
          records: response.body?.records.map(i => this.mapToViewModel(i)) || [],
          totalCount: response.body?.totalCount || 0
        }))
      )
      .subscribe(data => {
        // Add new interactions to the existing array
        this.interactions = [...this.interactions, ...data.records];
        this.totalCount = data.totalCount;
        
        // Check if we have more data
        this.hasMoreData = this.interactions.length < data.totalCount;
        
        // Update grouped interactions
        this.updateGroupedInteractions();
        this.isLoading.set(false);
      });
  }

  private formatDateForApi(date: Date, isEndDate: boolean = false): string {
    if (!date) return '';
    
    const d = new Date(date);
    if (isEndDate) {
      d.setHours(23, 59, 59, 999);
    } else {
      d.setHours(0, 0, 0, 0);
    }
    
    return d.toISOString();
  }

  private mapToViewModel(interaction: InteractionModel): InteractionViewModel {
    const limitWords = (text: string, limit: number = 20): string => {
      if (!text) return '';
      const words = text.split(' ');
      if (words.length <= limit) return text;
      return words.slice(0, limit).join(' ') + '...';
    };

    return {
      id: interaction.id,
      type: interaction.type.toString(),
      date: new Date(interaction.date),
      description: limitWords(interaction.description || ''),
      status: interaction.status,
    };
  }

  updateGroupedInteractions(): void {
    // Group the interactions by month/year
    const grouped = this.interactions.reduce((acc, interaction) => {
      const date = new Date(interaction.date);
      const month = date.toLocaleString('default', { month: 'long' });
      const year = date.getFullYear();
      const key = `${month}-${year}`;

      if (!acc[key]) {
        acc[key] = {
          month,
          year,
          interactions: []
        };
      }
      acc[key].interactions.push(interaction);
      return acc;
    }, {} as Record<string, GroupedInteraction>);

    // Convert to array and sort by date (newest first)
    this.groupedInteractions = Object.values(grouped).sort((a, b) => {
      const dateA = new Date(a.year, new Date(`${a.month} 1`).getMonth());
      const dateB = new Date(b.year, new Date(`${b.month} 1`).getMonth());
      return dateB.getTime() - dateA.getTime();
    });
  }

  onScroll(event: Event): void {
    if (!this.hasMoreData || this.isLoading()) return;

    const element = event.target as HTMLElement;
    const scrollPosition = element.scrollTop + element.clientHeight;
    const scrollHeight = element.scrollHeight;

    // Load more when the user is near the bottom (within 200px)
    if (scrollHeight - scrollPosition < 200) {
      this.loadMoreData();
    }
  }

  clearDateFilter(): void {
    this.dateRange = [];
    this.loadData();
  }

  clearAllFilters(): void {
    this.searchText.set('');
    this.selectedTypeFilter = null;
    this.dateRange = [];
    this.loadData();
  }

  openInteractionModal(interaction: InteractionViewModel): void {
    const dialogRef = this.dialogService.open(InteractionModalComponent, {
      header: 'Interaction Details',
      width: '50rem',
      breakpoints: {'1199px': '95vw'},
      data: {
        record: interaction
      }
    });

    dialogRef.onClose.subscribe(result => {
      if (result) {
        // Refresh data
        this.loadData();
      }
    });
  }

  getCurrentMonth(): string {
    return new Date().toLocaleString('default', { month: 'long' });
  }

  getCurrentYear(): number {
    return new Date().getFullYear();
  }

  isPreviousMonth(month: string, year: number): boolean {
    const today = new Date();
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1);
    return month === lastMonth.toLocaleString('default', { month: 'long' }) && year === lastMonth.getFullYear();
  }

  isOlderMonth(month: string, year: number): boolean {
    return !this.isPreviousMonth(month, year) &&
           (year < this.getCurrentYear() ||
           (year === this.getCurrentYear() && new Date(`${month} 1`).getMonth() < new Date().getMonth() - 1));
  }

  getMonthsAgo(month: string, year: number): number {
    const today = new Date();
    const given = new Date(year, new Date(`${month} 1`).getMonth());
    const diffMonths = (today.getFullYear() - given.getFullYear()) * 12 + (today.getMonth() - given.getMonth());
    return diffMonths;
  }
}
