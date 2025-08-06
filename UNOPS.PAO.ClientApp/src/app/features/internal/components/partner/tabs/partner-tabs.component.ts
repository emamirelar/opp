import { Component, OnInit } from '@angular/core';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Partner } from '../../../models/partner.model';
import { PictureComponent } from '@common/reusables/components/picture/picture.component';
import { GoBackComponent } from '../../../../../common/reusables/components/go-back/go-back.component';
import { ResponsiveTabsComponent, ResponsiveTabItem } from '../../../../../common/reusables/components/responsive-tabs';

/**
 * @uiEntity PartnerTabs
 * @route /partnerships/partners/:recordId
 * @description Partner detail navigation interface with tabs for different aspects of partner information. Provides organized access to partner details, contacts, interactions, funding agreements, and analytics data.
 * @capabilities navigate_partner_sections, view_partner_details, manage_partner_contacts, view_partner_interactions, access_partner_data, upload_logo
 * @synonyms partner_navigation, partner_details, partner_tabs, partner_sections, organization_tabs
 * @mandatoryFields recordId
 * @help_when_stuck Use the tabs to navigate between different sections of partner information. The partner logo and basic info are always visible at the top. Each tab shows different aspects like organizational details, contacts, interactions, or analytics.
 * @common_tasks
 *   - Viewing partner details: Click on the main Details tab
 *   - Managing contacts: Switch to Contacts tab to see people associated with this partner
 *   - Checking interactions: Switch to Interactions tab to see communication history
 *   - Viewing funding: Go to Funding & Agreements tab for financial information
 *   - Accessing analytics: Use Dashboard tab for partner performance data
 *   - Uploading logo: Click on the logo area to upload a new partner logo
 */

@Component({
  selector: 'app-partner-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, PictureComponent, GoBackComponent, ResponsiveTabsComponent],
  template: `
    <div class="flex flex-col gap-8">
      <!-- Header section -->
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
              {{ recordData.name }}
            </div>
          </div>
        </div>
      </div>

      <!-- Responsive tabs -->
      <app-responsive-tabs 
        [tabs]="tabs"
        [dropdownPlaceholder]="'Select tab'">
      </app-responsive-tabs>

      <!-- Router outlet -->
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
export class PartnerTabsComponent implements OnInit {
  recordId: string = '';
  tabs: ResponsiveTabItem[] = [];
  recordData: Partner = {} as Partner;

  constructor(
    private activatedRoute: ActivatedRoute
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
        icon: 'info'
      },
      {
        label: 'title.contacts',
        route: `/partnerships/partners/${this.recordId}/contacts`,
        icon: 'contacts'
      },
      {
        label: 'title.interactions',
        route: `/partnerships/partners/${this.recordId}/interactions`,
        icon: 'chat'
      },
      {
        label: 'title.fundingAndAgreements',
        route: `/partnerships/partners/${this.recordId}/funding-agreements`,
        icon: 'attach_money'
      },
      {
        label: 'title.dashboard',
        route: `/partnerships/partners/${this.recordId}/data`,
        icon: 'bar_chart'
      },
    ];
  }

  getUploadLogoUrl(): string {
    return `/api/partners/${this.recordId}/upload-logo`;
  }

  _loadRecordDetails(): void {
    console.log('Logo updated, reloading partner details...');
  }

  isMobile(): boolean {
    return window.innerWidth <= 768;
  }
}
