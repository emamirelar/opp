import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

/**
 * BaseEntityViewComponent Tests
 * 
 * Tests for the reusable base entity view component that provides
 * common layout structure, header, tabs, and action buttons.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/shared/components/base-entity-view/
 *         and run 'ng test'
 */

// Mock component for testing (replace with actual import when available)
@Component({
  selector: 'app-base-entity-view',
  standalone: true,
  imports: [],
  template: `
    <div class="entity-view" [class.loading]="loading()">
      <header class="entity-header">
        <h1 class="entity-title">{{ title() }}</h1>
        @if (subtitle()) {
          <span class="entity-subtitle">{{ subtitle() }}</span>
        }
        @if (status()) {
          <span class="entity-status">{{ status() }}</span>
        }
      </header>
      @if (tabs().length > 0) {
        <nav class="entity-tabs">
          @for (tab of tabs(); track tab.id) {
            <button
              class="tab-button"
              [class.active]="tab.id === activeTab()"
              [disabled]="tab.disabled"
              (click)="onTabClick(tab)">
              {{ tab.label }}
              @if (tab.count) {
                <span class="tab-count">{{ tab.count }}</span>
              }
            </button>
          }
        </nav>
      }
      <div class="entity-actions">
        @for (action of actions(); track action.id) {
          <button
            class="action-button"
            [disabled]="action.disabled || action.loading"
            (click)="onActionClick(action, $event)">
            {{ action.label }}
          </button>
        }
      </div>
      @if (loading()) {
        <div class="loading-overlay">Loading...</div>
      }
      @if (error()) {
        <div class="error-message">{{ error() }}</div>
      }
      <ng-content></ng-content>
    </div>
  `
})
class MockBaseEntityViewComponent {
  title = signal<string>('');
  subtitle = signal<string | undefined>(undefined);
  status = signal<string | undefined>(undefined);
  loading = signal<boolean>(false);
  error = signal<string | undefined>(undefined);
  tabs = signal<any[]>([]);
  activeTab = signal<string>('');
  actions = signal<any[]>([]);
  recordPermissions = signal<any>({ permissions: { canUpdate: true, canDelete: true, canRead: true } });
  
  @Output() tabChange = new EventEmitter<string>();
  @Output() actionClick = new EventEmitter<any>();
  @Output() backClick = new EventEmitter<void>();

  onTabClick(tab: any) {
    this.tabChange.emit(tab.id);
  }

  onActionClick(action: any, event: Event) {
    this.actionClick.emit({ action, event });
  }
}

describe('BaseEntityViewComponent', () => {
  let component: MockBaseEntityViewComponent;
  let fixture: ComponentFixture<MockBaseEntityViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoopAnimationsModule, MockBaseEntityViewComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MockBaseEntityViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // Rendering Tests
  describe('Rendering', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should display entity title', () => {
      component.title.set('Test Partner');
      fixture.detectChanges();
      
      const titleEl = fixture.debugElement.query(By.css('.entity-title'));
      expect(titleEl.nativeElement.textContent).toContain('Test Partner');
    });

    it('should display entity subtitle when provided', () => {
      component.title.set('Partner');
      component.subtitle.set('Subsidiary of ACME Corp');
      fixture.detectChanges();
      
      const subtitleEl = fixture.debugElement.query(By.css('.entity-subtitle'));
      expect(subtitleEl.nativeElement.textContent).toContain('Subsidiary of ACME Corp');
    });

    it('should not display subtitle when not provided', () => {
      component.title.set('Partner');
      component.subtitle.set(undefined);
      fixture.detectChanges();
      
      const subtitleEl = fixture.debugElement.query(By.css('.entity-subtitle'));
      expect(subtitleEl).toBeNull();
    });

    it('should display entity status badge', () => {
      component.status.set('Active');
      fixture.detectChanges();
      
      const statusEl = fixture.debugElement.query(By.css('.entity-status'));
      expect(statusEl.nativeElement.textContent).toContain('Active');
    });
  });

  // Loading State Tests
  describe('Loading State', () => {
    it('should show loading overlay when loading is true', () => {
      component.loading.set(true);
      fixture.detectChanges();
      
      const loadingEl = fixture.debugElement.query(By.css('.loading-overlay'));
      expect(loadingEl).toBeTruthy();
    });

    it('should hide loading overlay when loading is false', () => {
      component.loading.set(false);
      fixture.detectChanges();
      
      const loadingEl = fixture.debugElement.query(By.css('.loading-overlay'));
      expect(loadingEl).toBeNull();
    });

    it('should add loading class to container', () => {
      component.loading.set(true);
      fixture.detectChanges();
      
      const containerEl = fixture.debugElement.query(By.css('.entity-view'));
      expect(containerEl.classes['loading']).toBeTruthy();
    });
  });

  // Error State Tests
  describe('Error State', () => {
    it('should show error message when error is set', () => {
      component.error.set('Failed to load entity');
      fixture.detectChanges();
      
      const errorEl = fixture.debugElement.query(By.css('.error-message'));
      expect(errorEl.nativeElement.textContent).toContain('Failed to load entity');
    });

    it('should hide error message when error is cleared', () => {
      component.error.set(undefined);
      fixture.detectChanges();
      
      const errorEl = fixture.debugElement.query(By.css('.error-message'));
      expect(errorEl).toBeNull();
    });
  });

  // Tab Navigation Tests
  describe('Tab Navigation', () => {
    beforeEach(() => {
      component.tabs.set([
        { id: 'overview', label: 'Overview' },
        { id: 'details', label: 'Details' },
        { id: 'contacts', label: 'Contacts', count: 5 },
        { id: 'disabled', label: 'Disabled', disabled: true }
      ]);
      component.activeTab.set('overview');
      fixture.detectChanges();
    });

    it('should render all tabs', () => {
      const tabButtons = fixture.debugElement.queryAll(By.css('.tab-button'));
      expect(tabButtons.length).toBe(4);
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

    it('should display tab count badge', () => {
      const contactsTab = fixture.debugElement.queryAll(By.css('.tab-button'))[2];
      const countBadge = contactsTab.query(By.css('.tab-count'));
      expect(countBadge.nativeElement.textContent).toContain('5');
    });

    it('should disable tab when disabled is true', () => {
      const disabledTab = fixture.debugElement.queryAll(By.css('.tab-button'))[3];
      expect(disabledTab.nativeElement.disabled).toBeTruthy();
    });
  });

  // Action Button Tests
  describe('Action Buttons', () => {
    beforeEach(() => {
      component.actions.set([
        { id: 'edit', label: 'Edit' },
        { id: 'delete', label: 'Delete', disabled: true },
        { id: 'save', label: 'Saving...', loading: true }
      ]);
      fixture.detectChanges();
    });

    it('should render all action buttons', () => {
      const actionButtons = fixture.debugElement.queryAll(By.css('.action-button'));
      expect(actionButtons.length).toBe(3);
    });

    it('should emit actionClick when action is clicked', () => {
      spyOn(component.actionClick, 'emit');
      
      const editButton = fixture.debugElement.queryAll(By.css('.action-button'))[0];
      editButton.nativeElement.click();
      
      expect(component.actionClick.emit).toHaveBeenCalled();
    });

    it('should disable action button when disabled', () => {
      const deleteButton = fixture.debugElement.queryAll(By.css('.action-button'))[1];
      expect(deleteButton.nativeElement.disabled).toBeTruthy();
    });

    it('should disable action button when loading', () => {
      const saveButton = fixture.debugElement.queryAll(By.css('.action-button'))[2];
      expect(saveButton.nativeElement.disabled).toBeTruthy();
    });
  });
});

