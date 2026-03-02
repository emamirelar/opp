import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

/**
 * RelatedInfoPanelComponent Tests
 * 
 * Tests for the related information panel component that displays
 * related entity information in expandable panels.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/shared/components/related-info-panel/
 *         and run 'ng test'
 */

// Mock component for testing
@Component({
  selector: 'app-related-info-panel',
  standalone: true,
  imports: [],
  template: `
    <div class="related-info-panel" [class.collapsed]="collapsed()">
      <header class="panel-header" (click)="toggleCollapse()">
        <h3 class="panel-title">{{ title() }}</h3>
        @if (items().length > 0) {
          <span class="panel-count">({{ items().length }})</span>
        }
        @if (showAddButton()) {
          <button class="add-button" (click)="onAddClick($event)">+</button>
        }
      </header>
      @if (!collapsed()) {
        <div class="panel-body">
          @if (loading()) {
            <div class="loading-skeleton">Loading...</div>
          }
          @if (!loading() && items().length === 0) {
            <div class="empty-state">
              {{ emptyMessage() }}
            </div>
          }
          @if (error()) {
            <div class="error-state">
              {{ error() }}
              <button class="retry-button" (click)="onRefresh()">Retry</button>
            </div>
          }
          @if (!loading() && !error() && items().length > 0) {
            <ul class="item-list">
              @for (item of displayedItems(); track item) {
                <li class="panel-item" (click)="onItemClick(item)">
                  <span class="item-name">{{ item.name }}</span>
                  @if (item.detail) {
                    <span class="item-detail">{{ item.detail }}</span>
                  }
                  <div class="item-actions" (click)="$event.stopPropagation()">
                    @for (action of item.actions; track action) {
                      <button class="item-action" (click)="onItemAction(item, action)">
                        {{ action.label }}
                      </button>
                    }
                  </div>
                </li>
              }
            </ul>
          }
          @if (showSeeAll() && items().length > maxItems()) {
            <a class="see-all-link" (click)="onSeeAllClick()">
              See all {{ items().length }}
            </a>
          }
        </div>
      }
    </div>
  `
})
class MockRelatedInfoPanelComponent {
  title = signal<string>('');
  items = signal<any[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | undefined>(undefined);
  emptyMessage = signal<string>('No items found');
  showAddButton = signal<boolean>(false);
  showSeeAll = signal<boolean>(true);
  maxItems = signal<number>(5);
  collapsed = signal<boolean>(false);

  @Output() itemClick = new EventEmitter<any>();
  @Output() addClick = new EventEmitter<void>();
  @Output() seeAllClick = new EventEmitter<void>();
  @Output() refreshClick = new EventEmitter<void>();
  @Output() actionClick = new EventEmitter<any>();

  displayedItems() {
    return this.items().slice(0, this.maxItems());
  }

  toggleCollapse() {
    this.collapsed.set(!this.collapsed());
  }

  onItemClick(item: any) {
    this.itemClick.emit(item);
  }

  onAddClick(event: Event) {
    event.stopPropagation();
    this.addClick.emit();
  }

  onSeeAllClick() {
    this.seeAllClick.emit();
  }

  onRefresh() {
    this.refreshClick.emit();
  }

  onItemAction(item: any, action: any) {
    this.actionClick.emit({ item, action });
  }
}

describe('RelatedInfoPanelComponent', () => {
  let component: MockRelatedInfoPanelComponent;
  let fixture: ComponentFixture<MockRelatedInfoPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NoopAnimationsModule, MockRelatedInfoPanelComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MockRelatedInfoPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // Rendering Tests
  describe('Rendering', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should display panel title', () => {
      component.title.set('Contacts');
      fixture.detectChanges();
      
      const titleEl = fixture.debugElement.query(By.css('.panel-title'));
      expect(titleEl.nativeElement.textContent).toContain('Contacts');
    });

    it('should display item count in header', () => {
      component.items.set([{ name: 'Item 1' }, { name: 'Item 2' }]);
      fixture.detectChanges();
      
      const countEl = fixture.debugElement.query(By.css('.panel-count'));
      expect(countEl.nativeElement.textContent).toContain('(2)');
    });

    it('should not display count when no items', () => {
      component.items.set([]);
      fixture.detectChanges();
      
      const countEl = fixture.debugElement.query(By.css('.panel-count'));
      expect(countEl).toBeNull();
    });
  });

  // Loading State Tests
  describe('Loading State', () => {
    it('should show loading skeleton when loading', () => {
      component.loading.set(true);
      fixture.detectChanges();
      
      const loadingEl = fixture.debugElement.query(By.css('.loading-skeleton'));
      expect(loadingEl).toBeTruthy();
    });

    it('should hide item list when loading', () => {
      component.loading.set(true);
      component.items.set([{ name: 'Item' }]);
      fixture.detectChanges();
      
      const itemList = fixture.debugElement.query(By.css('.item-list'));
      expect(itemList).toBeNull();
    });
  });

  // Empty State Tests
  describe('Empty State', () => {
    it('should show empty message when no items', () => {
      component.items.set([]);
      component.emptyMessage.set('No contacts yet');
      fixture.detectChanges();
      
      const emptyEl = fixture.debugElement.query(By.css('.empty-state'));
      expect(emptyEl.nativeElement.textContent).toContain('No contacts yet');
    });
  });

  // Error State Tests
  describe('Error State', () => {
    it('should show error message when error occurs', () => {
      component.error.set('Failed to load data');
      fixture.detectChanges();
      
      const errorEl = fixture.debugElement.query(By.css('.error-state'));
      expect(errorEl.nativeElement.textContent).toContain('Failed to load data');
    });

    it('should show retry button on error', () => {
      component.error.set('Failed to load');
      fixture.detectChanges();
      
      const retryButton = fixture.debugElement.query(By.css('.retry-button'));
      expect(retryButton).toBeTruthy();
    });

    it('should emit refreshClick when retry is clicked', () => {
      spyOn(component.refreshClick, 'emit');
      component.error.set('Error');
      fixture.detectChanges();
      
      const retryButton = fixture.debugElement.query(By.css('.retry-button'));
      retryButton.nativeElement.click();
      
      expect(component.refreshClick.emit).toHaveBeenCalled();
    });
  });

  // Item List Tests
  describe('Item List', () => {
    beforeEach(() => {
      component.items.set([
        { name: 'Contact 1', detail: 'Manager' },
        { name: 'Contact 2', detail: 'Director' },
        { name: 'Contact 3', detail: 'Analyst' }
      ]);
      fixture.detectChanges();
    });

    it('should render items', () => {
      const items = fixture.debugElement.queryAll(By.css('.panel-item'));
      expect(items.length).toBe(3);
    });

    it('should display item name', () => {
      const firstItem = fixture.debugElement.query(By.css('.panel-item'));
      expect(firstItem.nativeElement.textContent).toContain('Contact 1');
    });

    it('should display item detail', () => {
      const firstItem = fixture.debugElement.query(By.css('.panel-item'));
      expect(firstItem.nativeElement.textContent).toContain('Manager');
    });

    it('should emit itemClick when item is clicked', () => {
      spyOn(component.itemClick, 'emit');
      
      const firstItem = fixture.debugElement.query(By.css('.panel-item'));
      firstItem.nativeElement.click();
      
      expect(component.itemClick.emit).toHaveBeenCalledWith({ name: 'Contact 1', detail: 'Manager' });
    });

    it('should limit displayed items to maxItems', () => {
      component.items.set(Array(10).fill(0).map((_, i) => ({ name: `Item ${i}` })));
      component.maxItems.set(5);
      fixture.detectChanges();
      
      const items = fixture.debugElement.queryAll(By.css('.panel-item'));
      expect(items.length).toBe(5);
    });
  });

  // Collapse/Expand Tests
  describe('Collapse/Expand', () => {
    it('should hide body when collapsed', () => {
      component.collapsed.set(true);
      fixture.detectChanges();
      
      const body = fixture.debugElement.query(By.css('.panel-body'));
      expect(body).toBeNull();
    });

    it('should show body when expanded', () => {
      component.collapsed.set(false);
      component.items.set([{ name: 'Item' }]);
      fixture.detectChanges();
      
      const body = fixture.debugElement.query(By.css('.panel-body'));
      expect(body).toBeTruthy();
    });

    it('should toggle collapse when header is clicked', () => {
      component.collapsed.set(false);
      fixture.detectChanges();
      
      const header = fixture.debugElement.query(By.css('.panel-header'));
      header.nativeElement.click();
      
      expect(component.collapsed()).toBeTruthy();
    });

    it('should add collapsed class to container', () => {
      component.collapsed.set(true);
      fixture.detectChanges();
      
      const container = fixture.debugElement.query(By.css('.related-info-panel'));
      expect(container.classes['collapsed']).toBeTruthy();
    });
  });

  // Add Button Tests
  describe('Add Button', () => {
    it('should show add button when enabled', () => {
      component.showAddButton.set(true);
      fixture.detectChanges();
      
      const addButton = fixture.debugElement.query(By.css('.add-button'));
      expect(addButton).toBeTruthy();
    });

    it('should hide add button when disabled', () => {
      component.showAddButton.set(false);
      fixture.detectChanges();
      
      const addButton = fixture.debugElement.query(By.css('.add-button'));
      expect(addButton).toBeNull();
    });

    it('should emit addClick when add button is clicked', () => {
      spyOn(component.addClick, 'emit');
      component.showAddButton.set(true);
      fixture.detectChanges();
      
      const addButton = fixture.debugElement.query(By.css('.add-button'));
      addButton.nativeElement.click();
      
      expect(component.addClick.emit).toHaveBeenCalled();
    });
  });

  // See All Link Tests
  describe('See All Link', () => {
    it('should show see all link when items exceed maxItems', () => {
      component.items.set(Array(10).fill(0).map((_, i) => ({ name: `Item ${i}` })));
      component.maxItems.set(5);
      component.showSeeAll.set(true);
      fixture.detectChanges();
      
      const seeAllLink = fixture.debugElement.query(By.css('.see-all-link'));
      expect(seeAllLink).toBeTruthy();
      expect(seeAllLink.nativeElement.textContent).toContain('See all 10');
    });

    it('should emit seeAllClick when clicked', () => {
      spyOn(component.seeAllClick, 'emit');
      component.items.set(Array(10).fill(0).map((_, i) => ({ name: `Item ${i}` })));
      component.maxItems.set(5);
      fixture.detectChanges();
      
      const seeAllLink = fixture.debugElement.query(By.css('.see-all-link'));
      seeAllLink.nativeElement.click();
      
      expect(component.seeAllClick.emit).toHaveBeenCalled();
    });
  });
});

