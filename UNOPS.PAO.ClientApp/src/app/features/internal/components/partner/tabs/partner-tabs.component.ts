import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { filter, Subscription } from 'rxjs';
import { Partner } from '../../../models/partner.model';
import {PictureComponent} from '@common/reusables/components/picture/picture.component';
import {Button} from 'primeng/button';
import { GoBackComponent } from '../../../../../common/reusables/components/go-back/go-back.component';

interface TabItem {
  label: string;
  route: string;
  translatedLabel?: string;
}

@Component({
  selector: 'app-partner-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, FormsModule, Tabs, TabList, Tab, TooltipModule, PictureComponent, Button, GoBackComponent, DropdownModule],
  template: `

  <div class="flex flex-col gap-8">
    <!-- Back button -->



      <div class="flex flex-col items-center">

        <div class="w-full flex justify-between items-center">

          <div class="font-semibold text-gray-400">
            {{ 'title.partner' | translate }}
          </div>

          <app-go-back></app-go-back>

        </div>

        <div class="flex gap-2 md:gap-4 w-full items-center">
      <app-picture
        [imageUrl]="recordData.logoUrl || ''"
        [uploadUrl]="getUploadLogoUrl()"
        [altText]="'Partner logo'"
        [size]="isMobile() ? 'extra-small' : 'small'"
        (imageChanged)="_loadRecordDetails()"
      />
        <div class="flex flex-col">

          <div class="text-2xl md:text-4xl font-bold">
            {{ recordData?.name }}
          </div>
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
export class PartnerTabsComponent implements OnInit, OnDestroy {
  recordId: string = '';
  activeRoute: string = '';

  tabs: TabItem[] = [];
  recordData: Partner = {} as Partner;
  private routerSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private location: Location,
    private translateService: TranslateService
  ) {}

  ngOnInit(): void {
    this.recordId = this.activatedRoute.snapshot.paramMap.get('recordId') || '';

    // Get the resolved data from the route
    this.activatedRoute.data.subscribe(data => {
      this.recordData = data['partnerData'];
    });

    // Create tabs based on recordId
    this.tabs = [
      {
        label: 'title.details',
        route: `/partnerships/partners/${this.recordId}`,
        translatedLabel: this.translateService.instant('title.details')
      },
      {
        label: 'title.contacts',
        route: `/partnerships/partners/${this.recordId}/contacts`,
        translatedLabel: this.translateService.instant('title.contacts')
      },
      {
        label: 'title.interactions',
        route: `/partnerships/partners/${this.recordId}/interactions`,
        translatedLabel: this.translateService.instant('title.interactions')
      },
      {
        label: 'title.fundingAndAgreements',
        route: `/partnerships/partners/${this.recordId}/funding-agreements`,
        translatedLabel: this.translateService.instant('title.fundingAndAgreements')
      },
      {
        label: 'title.dashboard',
        route: `/partnerships/partners/${this.recordId}/data`,
        translatedLabel: this.translateService.instant('title.dashboard')
      },
    ];

    // Set initial active tab
    this.updateActiveTab();

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

    // Find the matching tab based on the current URL
    const matchingTab = this.tabs.find(tab => currentUrl === tab.route || currentUrl.startsWith(tab.route + '/'));

    // Use the matching tab's route, or default to the first tab
    this.activeRoute = matchingTab ? matchingTab.route : this.tabs[0].route;
  }


  getUploadLogoUrl(): string {
    // Return the upload URL for partner logo
    return `/api/partners/${this.recordId}/upload-logo`;
  }

  _loadRecordDetails(): void {
    // Reload partner data after logo change
    // You might want to call a service to refresh the data
    console.log('Logo updated, reloading partner details...');
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
}
