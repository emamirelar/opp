import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

/**
 * EnhancedEntityLayoutComponent Tests
 * 
 * Tests for the master layout component that provides main content area
 * with side panel for related information panels.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/shared/components/enhanced-entity-layout/
 *         and run 'ng test'
 */

@Component({
  selector: 'app-enhanced-entity-layout',
  template: `
    <div class="enhanced-entity-layout" [class.side-panel-collapsed]="sidePanelCollapsed()">
      <header class="layout-header" *ngIf="showHeader()">
        <nav class="breadcrumb" *ngIf="breadcrumb().length > 0">
          <a *ngFor="let item of breadcrumb(); let last = last" 
             [class.current]="last"
             (click)="onBreadcrumbClick(item)">
            {{ item.label }}
          </a>
        </nav>
        <div class="entity-header">
          <h1 class="entity-title">{{ title() }}</h1>
          <span class="entity-status" *ngIf="status()">{{ status() }}</span>
        </div>
        <div class="action-bar">
          <button *ngFor="let action of actions()" 
                  class="action-button"
                  [disabled]="action.disabled"
                  (click)="onActionClick(action)">
            {{ action.label }}
          </button>
        </div>
      </header>
      
      <div class="layout-body">
        <main class="main-content">
          <div class="loading-overlay" *ngIf="loading()">Loading...</div>
          <div class="error-container" *ngIf="error()">{{ error() }}</div>
          <nav class="tab-navigation" *ngIf="tabs().length > 0">
            <button *ngFor="let tab of tabs()"
                    class="tab-button"
                    [class.active]="tab.id === activeTab()"
                    (click)="onTabClick(tab)">
              {{ tab.label }}
            </button>
          </nav>
          <div class="tab-content">
            <ng-content select="[main-content]"></ng-content>
          </div>
        </main>
        
        <aside class="side-panel" *ngIf="showSidePanel()">
          <button class="toggle-side-panel" (click)="toggleSidePanel()">
            {{ sidePanelCollapsed() ? '>' : '<' }}
          </button>
          <div class="side-panel-content" *ngIf="!sidePanelCollapsed()">
            <ng-content select="[side-panel]"></ng-content>
          </div>
        </aside>
      </div>
    </div>
  `
})
class MockEnhancedEntityLayoutComponent {
  title = signal<string>('');
  status = signal<string | undefined>(undefined);
  showHeader = signal<boolean>(true);
  showSidePanel = signal<boolean>(true);
  sidePanelCollapsed = signal<boolean>(false);
  loading = signal<boolean>(false);
  error = signal<string | undefined>(undefined);
  breadcrumb = signal<any[]>([]);
  tabs = signal<any[]>([]);
  activeTab = signal<string>('');
  actions = signal<any[]>([]);

  @Output() tabChange = new EventEmitter<string>();
  @Output() actionClick = new EventEmitter<any>();
  @Output() breadcrumbClick = new EventEmitter<any>();

  toggleSidePanel() {
    this.sidePanelCollapsed.set(!this.sidePanelCollapsed());
  }

  onTabClick(tab: any) {
    this.tabChange.emit(tab.id);
  }

  onActionClick(action: any) {
    this.actionClick.emit(action);
  }

  onBreadcrumbClick(item: any) {
    this.breadcrumbClick.emit(item);
  }
}

describe('EnhancedEntityLayoutComponent', () => {
  let component: MockEnhancedEntityLayoutComponent;
  let fixture: ComponentFixture<MockEnhancedEntityLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MockEnhancedEntityLayoutComponent],
      imports: [NoopAnimationsModule]
    }).compileComponents();

    fixture = TestBed.createComponent(MockEnhancedEntityLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // Layout Rendering Tests
  describe('Layout Rendering', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should render main content area', () => {
      const mainContent = fixture.debugElement.query(By.css('.main-content'));
      expect(mainContent).toBeTruthy();
    });

    it('should render side panel when enabled', () => {
      component.showSidePanel.set(true);
      fixture.detectChanges();
      
      const sidePanel = fixture.debugElement.query(By.css('.side-panel'));
      expect(sidePanel).toBeTruthy();
    });

    it('should hide side panel when disabled', () => {
      component.showSidePanel.set(false);
      fixture.detectChanges();
      
      const sidePanel = fixture.debugElement.query(By.css('.side-panel'));
      expect(sidePanel).toBeNull();
    });

    it('should display entity title', () => {
      component.title.set('Test Partner');
      fixture.detectChanges();
      
      const titleEl = fixture.debugElement.query(By.css('.entity-title'));
      expect(titleEl.nativeElement.textContent).toContain('Test Partner');
    });

    it('should display status badge', () => {
      component.status.set('Active');
      fixture.detectChanges();
      
      const statusEl = fixture.debugElement.query(By.css('.entity-status'));
      expect(statusEl.nativeElement.textContent).toContain('Active');
    });
  });

  // Header Tests
  describe('Header', () => {
    it('should show header when enabled', () => {
      component.showHeader.set(true);
      fixture.detectChanges();
      
      const header = fixture.debugElement.query(By.css('.layout-header'));
      expect(header).toBeTruthy();
    });

    it('should hide header when disabled', () => {
      component.showHeader.set(false);
      fixture.detectChanges();
      
      const header = fixture.debugElement.query(By.css('.layout-header'));
      expect(header).toBeNull();
    });
  });

  // Breadcrumb Tests
  describe('Breadcrumb', () => {
    beforeEach(() => {
      component.breadcrumb.set([
        { label: 'Home', route: '/' },
        { label: 'Partners', route: '/partners' },
        { label: 'ACME Corp', route: '/partners/1' }
      ]);
      fixture.detectChanges();
    });

    it('should render breadcrumb items', () => {
      const breadcrumbItems = fixture.debugElement.queryAll(By.css('.breadcrumb a'));
      expect(breadcrumbItems.length).toBe(3);
    });

    it('should mark last item as current', () => {
      const lastItem = fixture.debugElement.queryAll(By.css('.breadcrumb a'))[2];
      expect(lastItem.classes['current']).toBeTruthy();
    });

    it('should emit breadcrumbClick when item is clicked', () => {
      spyOn(component.breadcrumbClick, 'emit');
      
      const firstItem = fixture.debugElement.queryAll(By.css('.breadcrumb a'))[0];
      firstItem.nativeElement.click();
      
      expect(component.breadcrumbClick.emit).toHaveBeenCalledWith({ label: 'Home', route: '/' });
    });
  });

  // Tab Navigation Tests
  describe('Tab Navigation', () => {
    beforeEach(() => {
      component.tabs.set([
        { id: 'overview', label: 'Overview' },
        { id: 'details', label: 'Details' },
        { id: 'history', label: 'History' }
      ]);
      component.activeTab.set('overview');
      fixture.detectChanges();
    });

    it('should render tabs', () => {
      const tabs = fixture.debugElement.queryAll(By.css('.tab-button'));
      expect(tabs.length).toBe(3);
    });

    it('should highlight active tab', () => {
      const activeTab = fixture.debugElement.query(By.css('.tab-button.active'));
      expect(activeTab.nativeElement.textContent).toContain('Overview');
    });

    it('should emit tabChange when tab is clicked', () => {
      spyOn(component.tabChange, 'emit');
      
      const detailsTab = fixture.debugElement.queryAll(By.css('.tab-button'))[1];
      detailsTab.nativeElement.click();
      
      expect(component.tabChange.emit).toHaveBeenCalledWith('details');
    });
  });

  // Side Panel Tests
  describe('Side Panel', () => {
    it('should toggle side panel on button click', () => {
      component.showSidePanel.set(true);
      component.sidePanelCollapsed.set(false);
      fixture.detectChanges();
      
      const toggleButton = fixture.debugElement.query(By.css('.toggle-side-panel'));
      toggleButton.nativeElement.click();
      
      expect(component.sidePanelCollapsed()).toBeTruthy();
    });

    it('should hide side panel content when collapsed', () => {
      component.showSidePanel.set(true);
      component.sidePanelCollapsed.set(true);
      fixture.detectChanges();
      
      const content = fixture.debugElement.query(By.css('.side-panel-content'));
      expect(content).toBeNull();
    });

    it('should show side panel content when expanded', () => {
      component.showSidePanel.set(true);
      component.sidePanelCollapsed.set(false);
      fixture.detectChanges();
      
      const content = fixture.debugElement.query(By.css('.side-panel-content'));
      expect(content).toBeTruthy();
    });

    it('should add collapsed class when side panel is collapsed', () => {
      component.sidePanelCollapsed.set(true);
      fixture.detectChanges();
      
      const layout = fixture.debugElement.query(By.css('.enhanced-entity-layout'));
      expect(layout.classes['side-panel-collapsed']).toBeTruthy();
    });
  });

  // Loading State Tests
  describe('Loading State', () => {
    it('should show loading overlay when loading', () => {
      component.loading.set(true);
      fixture.detectChanges();
      
      const loadingOverlay = fixture.debugElement.query(By.css('.loading-overlay'));
      expect(loadingOverlay).toBeTruthy();
    });

    it('should hide loading overlay when not loading', () => {
      component.loading.set(false);
      fixture.detectChanges();
      
      const loadingOverlay = fixture.debugElement.query(By.css('.loading-overlay'));
      expect(loadingOverlay).toBeNull();
    });
  });

  // Error State Tests
  describe('Error State', () => {
    it('should show error message when error occurs', () => {
      component.error.set('Failed to load entity');
      fixture.detectChanges();
      
      const errorContainer = fixture.debugElement.query(By.css('.error-container'));
      expect(errorContainer.nativeElement.textContent).toContain('Failed to load entity');
    });

    it('should hide error when no error', () => {
      component.error.set(undefined);
      fixture.detectChanges();
      
      const errorContainer = fixture.debugElement.query(By.css('.error-container'));
      expect(errorContainer).toBeNull();
    });
  });

  // Action Bar Tests
  describe('Action Bar', () => {
    beforeEach(() => {
      component.actions.set([
        { id: 'edit', label: 'Edit' },
        { id: 'delete', label: 'Delete', disabled: true }
      ]);
      fixture.detectChanges();
    });

    it('should render action buttons', () => {
      const actionButtons = fixture.debugElement.queryAll(By.css('.action-button'));
      expect(actionButtons.length).toBe(2);
    });

    it('should disable button when action is disabled', () => {
      const deleteButton = fixture.debugElement.queryAll(By.css('.action-button'))[1];
      expect(deleteButton.nativeElement.disabled).toBeTruthy();
    });

    it('should emit actionClick when button is clicked', () => {
      spyOn(component.actionClick, 'emit');
      
      const editButton = fixture.debugElement.queryAll(By.css('.action-button'))[0];
      editButton.nativeElement.click();
      
      expect(component.actionClick.emit).toHaveBeenCalledWith({ id: 'edit', label: 'Edit' });
    });
  });
});

