import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { filter, Subscription } from 'rxjs';

interface TabItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-partner-tabs',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, Tabs, TabList, Tab],
  template: `
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
      
      <div class="mt-8">
        <router-outlet></router-outlet>
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
  private routerSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.recordId = this.activatedRoute.snapshot.paramMap.get('recordId') || '';
    
    // Create tabs based on recordId
    this.tabs = [
      {
        label: 'Partner Details',
        route: `/partner/${this.recordId}`
      },
      {
        label: 'Partner Data',
        route: `/partner/${this.recordId}/data`
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
