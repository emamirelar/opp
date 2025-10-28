import { Component, ElementRef, OnInit, ViewChild, inject, signal, effect } from '@angular/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { Router } from '@angular/router';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { PartnerViewContactsItemComponent } from '../item/partner-view-contacts-item.component';
import { ContactViewModel, GroupedContact } from '../../../../../models/contact-view.model';
import { map } from 'rxjs/operators';
import { CommonModule } from '@angular/common';

// Define the interface locally
interface ContactFilterParams {
  partnerId?: number;
  pageIndex?: number;
  pageSize?: number;
  orderBy?: string;
  ascending?: string;
  searchText?: string;
}

@Component({
  selector: 'app-partner-view-contacts-dialog',
  standalone: true,
  imports: [
    DialogModule,
    InputTextModule,
    FormsModule,
    ProgressSpinnerModule,
    ButtonModule,
    TooltipModule,
    IconFieldModule,
    InputIconModule,
    PartnerViewContactsItemComponent,
    CommonModule
  ],
  templateUrl: './partner-view-contacts-dialog.component.html'
})
export class PartnerViewContactsDialogComponent implements OnInit {
  @ViewChild('scrollContainer') scrollContainer?: ElementRef;

  private dialogConfig = inject(DynamicDialogConfig);
  private contactService = inject(ContactService);
  private router = inject(Router);

  // Data properties
  partnerId?: string;
  searchText = signal('');
  searchTextModel = ''; // For ngModel binding
  isLoading = signal<boolean>(false);
  currentPage = 0;
  itemsPerPage = 10;
  hasMoreData = true;
  totalCount = 0;

  // Debounce timer
  private searchDebounceTimer?: any;

  // Contacts data
  contacts: ContactViewModel[] = [];
  groupedContacts: GroupedContact[] = [];
  displayedGroups: GroupedContact[] = [];

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
    // Get partnerId from dialog config
    this.partnerId = this.dialogConfig.data?.partnerId;

    if (this.partnerId) {
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
    if (!this.partnerId) return;
    
    this.isLoading.set(true);
    this.currentPage = 0;
    this.hasMoreData = true;

    const filterParams: ContactFilterParams = {
      partnerId: Number(this.partnerId),
      pageIndex: this.currentPage,
      pageSize: this.itemsPerPage,
      orderBy: 'lastName',
      ascending: 'true'
    };

    // Add search text if provided
    const currentSearchText = this.searchText();
    if (currentSearchText && currentSearchText.trim() !== '') {
      filterParams.searchText = currentSearchText.trim();
    }

    this.contactService.getAll(filterParams)
      .pipe(
        map(response => ({
          records: response.body?.records.map((c: any) => this.mapToViewModel(c)) || [],
          totalCount: response.body?.totalCount || 0
        }))
      )
      .subscribe((data: any) => {
        this.contacts = data.records;
        this.totalCount = data.totalCount;
        this.hasMoreData = this.contacts.length < data.totalCount;
        this.updateGroupedContacts();
        this.isLoading.set(false);
      });
  }

  // Method for handling search text input
  onSearchTextChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTextModel = value;
    this.searchText.set(value);
  }

  loadMoreData(): void {
    if (!this.hasMoreData || this.isLoading() || !this.partnerId) return;

    this.isLoading.set(true);
    this.currentPage++;

    const filterParams: ContactFilterParams = {
      partnerId: Number(this.partnerId),
      pageIndex: this.currentPage,
      pageSize: this.itemsPerPage,
      orderBy: 'lastName',
      ascending: 'true'
    };

    // Add search text if provided
    const currentSearchText = this.searchText();
    if (currentSearchText && currentSearchText.trim() !== '') {
      filterParams.searchText = currentSearchText.trim();
    }

    this.contactService.getAll(filterParams)
      .pipe(
        map(response => ({
          records: response.body?.records.map((c: any) => this.mapToViewModel(c)) || [],
          totalCount: response.body?.totalCount || 0
        }))
      )
      .subscribe((data: any) => {
        // Add new contacts to the existing array
        this.contacts = [...this.contacts, ...data.records];
        this.totalCount = data.totalCount;
        
        // Check if we have more data
        this.hasMoreData = this.contacts.length < data.totalCount;
        
        // Update grouped contacts
        this.updateGroupedContacts();
        this.isLoading.set(false);
      });
  }

  private mapToViewModel(contact: any): ContactViewModel {
    return {
      id: contact.id,
      firstName: contact.firstName || '',
      lastName: contact.lastName || '',
      title: contact.title,
      email: contact.email,
      phone: contact.phone,
      profilePictureUrl: contact.profilePictureUrl
    };
  }

  updateGroupedContacts(): void {
    // Group the contacts by first letter of last name
    const grouped = this.contacts.reduce((acc, contact) => {
      const letter = (contact.lastName || '').charAt(0).toUpperCase();
      const key = letter || '#';

      if (!acc[key]) {
        acc[key] = {
          letter: key,
          contacts: []
        };
      }
      acc[key].contacts.push(contact);
      return acc;
    }, {} as Record<string, GroupedContact>);

    // Convert to array and sort alphabetically
    this.groupedContacts = Object.values(grouped).sort((a, b) => a.letter.localeCompare(b.letter));
    this.displayedGroups = this.groupedContacts;
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

  openContactView(contact: ContactViewModel): void {
    this.router.navigate(['/contact', contact.id]);
    (inject(DynamicDialogRef).close(false));
  }
} 
