import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { filter, Subscription } from 'rxjs';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { PictureComponent } from '@shared/reusables/components/picture/picture.component';
import { GoBackComponent } from '@shared/reusables/components/go-back/go-back.component';
import { ContactService } from '@partnerships/contacts/services/contact.service';

/**
 * @uiEntity ContactTabs
 * @route /partnerships/contacts/:recordId
 * @description Contact detail navigation interface with tabs for different aspects of contact information. Provides organized access to contact details, interactions, and related information.
 * @capabilities navigate_contact_sections, view_contact_details, manage_contact_interactions, access_contact_data
 * @synonyms contact_navigation, contact_details, contact_tabs, contact_sections
 * @mandatoryFields recordId
 * @help_when_stuck Use the tabs to navigate between different sections of contact information. The contact photo and basic info are always visible at the top. Each tab shows different aspects like personal details, interactions, or related data.
 * @common_tasks
 *   - Viewing contact details: Click on the main Details tab
 *   - Checking interactions: Switch to Interactions tab to see communication history
 *   - Navigating between sections: Click on tab headers to switch views
 *   - Going back: Use the back button to return to the contact list
 */

interface TabItem {
  label: string;
  route: string;
  translatedLabel?: string;
}

@Component({
  selector: 'app-contact-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule, Tabs, TabList, Tab, TooltipModule, PictureComponent, GoBackComponent, DropdownModule],
  template: `

  <div class="flex flex-col gap-8">
    <!-- Back button -->



      <div class="flex flex-col items-center">

        <div class="w-full flex justify-between items-center">

          <div class="font-semibold text-gray-400">
            {{ 'title.contact' | translate }}
          </div>

          <app-go-back></app-go-back>

        </div>

        <div class="flex gap-2 md:gap-4 w-full items-center">
      <app-picture
        [imageUrl]="recordData.profilePictureUrl || ''"
        [uploadUrl]="getUploadProfilePictureUrl()"
        [altText]="'Contact profile picture'"
        [size]="isMobile() ? 'extra-small' : 'small'"
        [disabled]="!recordData.permissions?.canUpdate"
        (imageChanged)="_loadRecordDetails()"
      />
        <div class="flex flex-col">

          <div class="text-2xl md:text-4xl font-bold">
            {{ getContactDisplayName() }}
          </div>
          <div class="text-lg text-gray-600">
            {{ recordData.title }}
            @if(recordData.department){
              - {{ recordData.department }}
            }
          </div>
          @if(recordData.partner?.name) {
            <div class="text-primary font-semibold">
              {{ recordData.partner?.name }}
            </div>
          }
      </div>
      </div>
    </div>
      <!-- Mobile dropdown -->
      <div class="block md:hidden p-4 bg-white shadow-sm rounded-lg">
        <p-dropdown
          [options]="tabs"
          [ngModel]="getActiveTab()"
          (onChange)="onTabChange($event)"
          optionLabel="translatedLabel"
          placeholder="Select tab"
          class="w-full">
        </p-dropdown>
      </div>
      
      <!-- Desktop tabs -->
      <div class="hidden md:block">
        <p-tabs [value]="activeRoute">
          <p-tablist>
            <p-tab *ngFor="let tab of tabs"
                  [value]="tab.route"
                  [routerLink]="tab.route"
                  class="flex items-center !gap-2 text-inherit">
              <span>{{ tab.label | translate }}</span>
            </p-tab>
          </p-tablist>
        </p-tabs>
      </div>
      <div>
        <router-outlet></router-outlet>
      </div>
  </div>
  `,
  styles: `
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
    }
  `
})
export class ContactTabsComponent implements OnInit, OnDestroy {
  recordId: string = '';
  activeRoute: string = '';

  tabs: TabItem[] = [];
  recordData: Contact = {} as Contact;
  private routerSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private location: Location,
    private translateService: TranslateService,
    private contactService: ContactService
  ) {}

  ngOnInit(): void {
    // Subscribe to parameter changes to handle back navigation properly
    this.activatedRoute.paramMap.subscribe(paramMap => {
      this.recordId = paramMap.get('recordId') || '';

      // Update tabs when recordId changes
      this.tabs = [
        {
          label: 'title.details',
          route: `/partnerships/contacts/${this.recordId}`,
          translatedLabel: this.translateService.instant('title.details')
        },
        {
          label: 'title.interactions',
          route: `/partnerships/contacts/${this.recordId}/interactions`,
          translatedLabel: this.translateService.instant('title.interactions')
        }
      ];

      // Update active tab after tabs are refreshed
      this.updateActiveTab();
    });

    // Get the resolved data from the route
    this.activatedRoute.data.subscribe(data => {
      this.recordData = data['contactData'];
    });

    // Subscribe to router events to update active tab on navigation
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveTab();
      });
  }

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  private updateActiveTab(): void {
    const currentUrl = this.router.url;

    // First try exact match
    let matchingTab = this.tabs.find(tab => currentUrl === tab.route);
    
    if (!matchingTab) {
      // If no exact match, find the longest route that matches
      // Sort by length descending to prioritize more specific matches
      const sortedTabs = [...this.tabs].sort((a, b) => b.route.length - a.route.length);
      matchingTab = sortedTabs.find(tab => currentUrl.startsWith(tab.route + '/'));
    }

    // Use the matching tab's route, or default to the first tab
    this.activeRoute = matchingTab ? matchingTab.route : this.tabs[0].route;
  }

  getUploadProfilePictureUrl(): string {
    return this.contactService.getUploadProfilePictureUrl(this.recordId);
  }

  _loadRecordDetails(): void {
    // Reload contact data after profile picture change
  }

  getActiveTab(): TabItem | null {
    return this.tabs.find(tab => tab.route === this.activeRoute) || null;
  }

  onTabChange(event: any): void {
    const selectedTab = event.value as TabItem;
    if (selectedTab) {
      this.router.navigate([selectedTab.route]);
    }
  }

  isMobile(): boolean {
    return window.innerWidth <= 768;
  }

  getContactDisplayName(): string {
    const parts = [
      this.recordData?.salutation,
      this.recordData?.firstName,
      this.recordData?.middleName,
      this.recordData?.lastName,
      this.recordData?.suffix
    ].filter(part => part && part.trim() !== '');

    return parts.join(' ') || 'Contact';
  }
}
