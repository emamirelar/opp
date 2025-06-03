import { ChangeDetectionStrategy, Component, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { TooltipModule } from 'primeng/tooltip';
import { filter, Subscription } from 'rxjs';
import { PartnerTree } from '../../../models/partner-tree.model';
import { PartnerTreeViewNavigationComponent } from './navigation/partner-tree-view-navigation.component';

interface TabItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-partner-tree-view',
  imports: [
    CommonModule, 
    RouterModule, 
    TranslateModule, 
    Tabs, 
    TabList, 
    Tab, 
    TooltipModule,
    PartnerTreeViewNavigationComponent
  ],
  template: `
  <div class="flex flex-col gap-8">
    <!-- Navigation component at the top -->
    <app-partner-tree-view-navigation></app-partner-tree-view-navigation>
    
    <!-- Tabs -->
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
    
    <!-- Router outlet for tab content -->
    <div>
      <router-outlet></router-outlet>
    </div>
  </div>
  `,
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
    }
  `
})
export class PartnerTreeViewComponent implements OnInit, OnDestroy {
  recordId: string = '';
  activeRoute: string = '';
  
  tabs: TabItem[] = [];
  recordData: PartnerTree = {} as PartnerTree;
  private routerSubscription: Subscription | null = null;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.recordId = this.activatedRoute.snapshot.paramMap.get('recordId') || '';
    
    // Get the resolved data from the route
    this.activatedRoute.data.subscribe(data => {
      this.recordData = data['partnerTreeData']?.data || {};
    });
    
    // Create tabs based on recordId
    this.tabs = [
      {
        label: 'Details',
        route: `/admin/partner-tree/${this.recordId}`
      },
      {
        label: 'Dashboard',
        route: `/admin/partner-tree/${this.recordId}/data`
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
