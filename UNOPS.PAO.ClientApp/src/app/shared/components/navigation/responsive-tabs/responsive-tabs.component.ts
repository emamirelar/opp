import { Component, Input, OnInit, OnDestroy, inject, HostListener } from '@angular/core';
import { Router, NavigationEnd, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { DropdownModule } from 'primeng/dropdown';
import { filter, Subscription } from 'rxjs';
import { ResponsiveTabItem } from './responsive-tabs.model';

@Component({
  selector: 'app-responsive-tabs',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TranslateModule, 
    RouterModule,
    Tabs, 
    TabList, 
    Tab, 
    DropdownModule
  ],
  templateUrl: './responsive-tabs.component.html',
  styleUrls: ['./responsive-tabs.component.scss']
})
export class ResponsiveTabsComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private translateService = inject(TranslateService);

  @Input() tabs: ResponsiveTabItem[] = [];
  @Input() disabled: boolean = false;
  @Input() dropdownPlaceholder: string = 'Select tab';
  @Input() tabsClass: string = '';
  @Input() tabListClass: string = '';
  @Input() activeTabClass: string = '';
  @Input() inactiveTabClass: string = '';
  @Input() breakpoint: number = 768; // Breakpoint for mobile/desktop switch

  activeRoute: string = '';
  isMobileView: boolean = false;
  private routerSubscription: Subscription | null = null;

  ngOnInit(): void {
    // Initialize mobile view detection
    this.updateViewMode();
    
    // Translate tab labels
    this.updateTranslatedLabels();

    // Set initial active tab
    this.updateActiveTab();

    // Subscribe to router events to update active tab on navigation
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveTab();
      });

    // Subscribe to language changes to update translations
    this.translateService.onLangChange.subscribe(() => {
      this.updateTranslatedLabels();
    });
  }

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any): void {
    this.updateViewMode();
  }

  private updateViewMode(): void {
    this.isMobileView = window.innerWidth <= this.breakpoint;
  }

  private updateTranslatedLabels(): void {
    this.tabs = this.tabs.map(tab => ({
      ...tab,
      translatedLabel: this.translateService.instant(tab.label)
    }));
  }

  private updateActiveTab(): void {
    const currentUrl = this.router.url.split('?')[0]; // Remove query parameters
    const currentUrlSegments = currentUrl.split('/').filter(segment => segment);

    // Find the matching tab based on the current URL
    // Sort tabs by route length (descending) to match the most specific route first
    const sortedTabs = [...this.tabs].sort((a, b) => b.route.length - a.route.length);
    
    const matchingTab = sortedTabs.find(tab => {
      if (tab.disabled) return false;
      
      const tabRoute = tab.route.split('?')[0]; // Remove query parameters from tab route
      const tabRouteSegments = tabRoute.split('/').filter(segment => segment);
      
      // Exact match
      if (currentUrl === tabRoute) return true;
      
      // Check if current URL starts with tab route (for child routes)
      if (currentUrl.startsWith(tabRoute + '/')) return true;
      
      // For routes like /partnerships/partners/123, match with /partnerships/partners/123/contacts
      if (tabRouteSegments.length > currentUrlSegments.length) return false;
      
      // Check segment by segment
      return tabRouteSegments.every((segment, index) => 
        currentUrlSegments[index] === segment
      );
    });

    // Use the matching tab's route, or default to the first non-disabled tab
    const firstActiveTab = this.tabs.find(tab => !tab.disabled);
    this.activeRoute = matchingTab ? matchingTab.route : (firstActiveTab?.route || '');
  }

  getActiveTab(): ResponsiveTabItem | null {
    return this.tabs.find(tab => tab.route === this.activeRoute) || null;
  }

  onTabChange(event: any): void {
    const selectedTab = event.value as ResponsiveTabItem;
    if (selectedTab && !selectedTab.disabled) {
      this.router.navigate([selectedTab.route]);
    }
  }

  getTabClass(tab: ResponsiveTabItem): string {
    const baseClass = 'flex items-center !gap-2 text-inherit';
    const isActive = tab.route === this.activeRoute;
    
    let additionalClasses = '';
    if (isActive && this.activeTabClass) {
      additionalClasses += ` ${this.activeTabClass}`;
    } else if (!isActive && this.inactiveTabClass) {
      additionalClasses += ` ${this.inactiveTabClass}`;
    }
    
    if (tab.disabled) {
      additionalClasses += ' p-tab-disabled';
    }

    return `${baseClass}${additionalClasses}`;
  }

  /**
   * Public method to programmatically set active tab
   */
  setActiveTab(route: string): void {
    const tab = this.tabs.find(t => t.route === route && !t.disabled);
    if (tab) {
      this.router.navigate([route]);
    }
  }

  /**
   * Public method to add a new tab
   */
  addTab(tab: ResponsiveTabItem): void {
    this.tabs.push(tab);
    this.updateTranslatedLabels();
  }

  /**
   * Public method to remove a tab
   */
  removeTab(route: string): void {
    this.tabs = this.tabs.filter(tab => tab.route !== route);
  }

  /**
   * Public method to enable/disable a tab
   */
  setTabDisabled(route: string, disabled: boolean): void {
    const tab = this.tabs.find(t => t.route === route);
    if (tab) {
      tab.disabled = disabled;
    }
  }
}
