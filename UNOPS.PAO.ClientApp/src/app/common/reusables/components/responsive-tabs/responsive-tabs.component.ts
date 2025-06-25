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
  template: `
    <!-- Mobile dropdown -->
    <div class="block md:hidden p-4 bg-white shadow-sm rounded-lg">
      <p-dropdown
        [options]="tabs"
        [ngModel]="getActiveTab()"
        (onChange)="onTabChange($event)"
        optionLabel="translatedLabel"
        [placeholder]="dropdownPlaceholder"
        class="w-full"
        [disabled]="disabled">
        <ng-template pTemplate="selectedItem" let-selectedOption>
          <div class="flex items-center gap-2" *ngIf="selectedOption">
            <span *ngIf="selectedOption.icon" class="material-symbols-outlined text-xl">{{ selectedOption.icon }}</span>
            <span>{{ selectedOption.translatedLabel }}</span>
          </div>
        </ng-template>
        <ng-template pTemplate="item" let-option>
          <div class="flex items-center gap-2" [class.opacity-50]="option.disabled">
            <span *ngIf="option.icon" class="material-symbols-outlined text-xl">{{ option.icon }}</span>
            <span>{{ option.translatedLabel }}</span>
          </div>
        </ng-template>
      </p-dropdown>
    </div>

    <!-- Desktop tabs -->
    <div class="hidden md:block">
      <p-tabs [value]="activeRoute" [class]="tabsClass">
        <p-tablist [class]="tabListClass">
          <p-tab 
            *ngFor="let tab of tabs"
            [value]="tab.route"
            [routerLink]="tab.route"
            [disabled]="tab.disabled"
            [class]="getTabClass(tab)"
            class="flex items-center !gap-2 text-inherit">
            <span *ngIf="tab.icon" class="material-symbols-outlined text-xl">{{ tab.icon }}</span>
            <span>{{ tab.label | translate }}</span>
          </p-tab>
        </p-tablist>
      </p-tabs>
    </div>
  `,
  styles: [`
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
    }
    
    :host ::ng-deep .p-tab-disabled {
      opacity: 0.5;
      pointer-events: none;
    }
  `]
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
    const currentUrl = this.router.url;

    // Find the matching tab based on the current URL
    const matchingTab = this.tabs.find(tab => 
      !tab.disabled && (currentUrl === tab.route || currentUrl.startsWith(tab.route + '/'))
    );

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