import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { TooltipModule } from 'primeng/tooltip';
import { filter, Subscription } from 'rxjs';
import { PartnerTreeViewNavigationComponent } from "../../partner-tree/view/navigation/partner-tree-view-navigation.component";
import { Partner } from '../../../models/partner.model';

interface TabItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-partner-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, Tabs, TabList, Tab, PartnerTreeViewNavigationComponent, TooltipModule],
  template: `
  <div class="flex flex-col gap-8"> 
    @if(recordData.partnerCategoryId){
      <div class="flex items-center gap-2 mt-2">
        <a [routerLink]="['/partner-tree', recordData.partnerCategoryId]"
        pTooltip="{{recordData.partnerCategoryName}}"
        class="text-lg text-gray-7e00 font-semibold hover:text-gray-500 cursor-pointer whitespace-nowrap overflow-hidden text-ellipsis max-w-1/3">
            {{recordData.partnerCategoryName}}
        </a>

        @if(recordData.partnerGroupId){
          
        <div class="px-2 text-lg">
          <i class="pi pi-angle-right"></i>
        </div>

        <a [routerLink]="['/partner-tree', recordData.partnerGroupId]"
        pTooltip="{{recordData.partnerGroupName}}"
        class="text-lg text-gray-7e00 font-semibold hover:text-gray-500 cursor-pointer whitespace-nowrap overflow-hidden text-ellipsis max-w-1/3">
            {{recordData.partnerGroupName}}
        </a>
        }
      </div>
      }
      <p-tabs [value]="activeRoute">
        <p-tablist>
          <p-tab *ngFor="let tab of tabs" 
                [value]="tab.route" 
                [routerLink]="tab.route" 
                class="flex items-center !gap-2 text-inherit">
            <span>{{ tab.label }}</span>
          </p-tab>
        </p-tablist>
      </p-tabs>
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
        label: 'Partner Details',
        route: `/partnerships/partners/${this.recordId}`
      },
      {
        label: 'Partner Data',
        route: `/partnerships/partners/${this.recordId}/data`
      }
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
    const activeTabIndex = currentUrl.includes('/data') ? 1 : 0;
    this.activeRoute = this.tabs[activeTabIndex].route;
  }
}
