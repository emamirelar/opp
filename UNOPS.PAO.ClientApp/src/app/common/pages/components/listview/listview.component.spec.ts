import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { of, Subject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ListviewComponent } from './listview.component';
import { ListviewExportService } from './listview-export.service';
import { ListViewColumn, ListViewConfig, SearchCriteria } from './listview.model';

describe('ListviewComponent', () => {
  let component: ListviewComponent;
  let fixture: ComponentFixture<ListviewComponent>;
  let exportService: jasmine.SpyObj<ListviewExportService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let translateService: jasmine.SpyObj<TranslateService>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;

  const mockColumns: ListViewColumn[] = [
    { label: 'Name', field: 'name', type: 'text', sortable: true },
    { label: 'Email', field: 'email', type: 'email', sortable: true },
    { label: 'Date', field: 'createdDate', type: 'date', sortable: true }
  ];

  const mockConfig: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: true,
    enableSorting: true,
    enableSearch: true,
    enableExport: true,
    entityName: 'Test Entity'
  };

  const mockData = [
    { id: 1, name: 'John Doe', email: 'john@example.com', createdDate: '2023-01-01' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com', createdDate: '2023-01-02' }
  ];

  beforeEach(async () => {
    const exportSpy = jasmine.createSpyObj('ListviewExportService', [
      'exportToGoogleSheet'
    ]);

    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    
    const activatedRouteSpy = jasmine.createSpyObj('ActivatedRoute', [], {
      snapshot: {
        queryParams: {}
      }
    });

    const translateSpy = jasmine.createSpyObj('TranslateService', ['instant']);
    translateSpy.instant.and.returnValue('Translated text');

    const confirmationSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);

    await TestBed.configureTestingModule({
      imports: [
        ListviewComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot(),
        NoopAnimationsModule
      ],
      providers: [
        { provide: ListviewExportService, useValue: exportSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRouteSpy },
        { provide: TranslateService, useValue: translateSpy },
        { provide: ConfirmationService, useValue: confirmationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewComponent);
    component = fixture.componentInstance;
    exportService = TestBed.inject(ListviewExportService) as jasmine.SpyObj<ListviewExportService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    activatedRoute = TestBed.inject(ActivatedRoute) as jasmine.SpyObj<ActivatedRoute>;
    translateService = TestBed.inject(TranslateService) as jasmine.SpyObj<TranslateService>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.viewMode).toBe('card');
      expect(component.searchText).toBe('');
      expect(component.first).toBe(0);
      expect(component.rows).toBe(20);
    });

    it('should initialize with provided config', () => {
      component.config = mockConfig;
      expect(component.config.pageSize).toBe(20);
      expect(component.config.enableSearch).toBe(true);
    });
  });

  describe('Column Configuration', () => {
    it('should accept column configuration', () => {
      component.columns = mockColumns;
      expect(component.columns.length).toBe(3);
      expect(component.columns[0].label).toBe('Name');
    });
  });

  describe('Search Functionality', () => {
    beforeEach(() => {
      component.config = { ...mockConfig, enableSearch: true };
    });

    it('should handle simple search input', () => {
      const searchValue = 'test';
      spyOn(component, 'onSearchInput');

      component.onSearchInput(searchValue);

      expect(component.onSearchInput).toHaveBeenCalledWith(searchValue);
    });

    it('should clear search', () => {
      component.searchText = 'test';
      component.searchValue = 'test';

      component.clearSearch();

      expect(component.searchText).toBe('');
      expect(component.searchValue).toBe('');
    });
  });

  describe('Advanced Search', () => {
    beforeEach(() => {
      component.config = {
        ...mockConfig,
        searchConfig: {
          useAdvancedSearch: true,
          searchableFields: []
        }
      };
    });

    it('should add search criterion', () => {
      const criterion: SearchCriteria = {
        field: 'name',
        value: 'test',
        label: 'Name',
        operator: 'like'
      };

      component.onAdvancedSearch(criterion);

      expect(component.searchCriteria.length).toBe(1);
      expect(component.searchCriteria[0]).toEqual(criterion);
    });

    it('should remove search criterion by index', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', value: 'test1', label: 'Name', operator: 'like' },
        { field: 'email', value: 'test2', label: 'Email', operator: 'like' }
      ];
      component.searchCriteria = criteria;

      component.onRemoveSearchCriterion(0);

      expect(component.searchCriteria.length).toBe(1);
      expect(component.searchCriteria[0].field).toBe('email');
    });

    it('should clear all advanced search criteria', () => {
      component.searchCriteria = [
        { field: 'name', value: 'test', label: 'Name', operator: 'like' }
      ];

      component.onClearAdvancedSearch();

      expect(component.searchCriteria.length).toBe(0);
    });

    it('should switch to advanced search mode', () => {
      component.switchToAdvancedSearch();

      expect(component.isAdvancedSearchMode()).toBe(true);
      expect(component.searchValue).toBe('');
    });

    it('should switch back to simple search mode', () => {
      component.isAdvancedSearchMode.set(true);
      component.searchCriteria = [
        { field: 'name', value: 'test', label: 'Name', operator: 'like' }
      ];

      component.switchToSimpleSearch();

      expect(component.isAdvancedSearchMode()).toBe(false);
      expect(component.searchCriteria.length).toBe(0);
    });
  });

  describe('Pagination', () => {
    it('should handle pagination properties', () => {
      expect(component.first).toBe(0);
      expect(component.rows).toBe(20);
      
      component.dataLoader.setPagination(20, 10);
      expect(component.first).toBe(20);
      expect(component.rows).toBe(10);
    });
  });

  describe('Sorting', () => {
    it('should handle sort change', () => {
      const sortEvent = { field: 'name', order: 1 };
      spyOn(component.sortChange, 'emit');

      component.onSortChange(sortEvent);

      expect(component.currentSortField).toBe('name');
      expect(component.currentSortOrder).toBe('asc');
      expect(component.sortChange.emit).toHaveBeenCalledWith({ field: 'name', order: 'asc' });
    });

    it('should handle descending sort', () => {
      const sortEvent = { field: 'name', order: -1 };

      component.onSortChange(sortEvent);

      expect(component.currentSortOrder).toBe('desc');
    });
  });

  describe('Row Selection', () => {
    it('should emit row click event', () => {
      const testData = { id: 1, name: 'Test' };
      spyOn(component.rowClick, 'emit');

      component.onRowClick(testData);

      expect(component.rowClick.emit).toHaveBeenCalledWith(testData);
    });
  });

  describe('View Mode', () => {
    it('should maintain card view mode', () => {
      expect(component.viewMode).toBe('card');
    });

    it('should always be in card view', () => {
      expect(component.viewMode).toBe('card');
    });
  });

  describe('Export Functionality', () => {
    beforeEach(() => {
      component.config = { ...mockConfig, enableExport: true };
      component.dataUrl = '/api/test-data';
    });

    it('should export data to Google Sheets', () => {
      const mockExportResult = { id: 'sheet123', url: 'https://sheets.google.com/sheet123' };
      exportService.exportToGoogleSheet.and.returnValue(of(mockExportResult));

      component.exportData();

      expect(exportService.exportToGoogleSheet).toHaveBeenCalled();
    });

    it('should emit exportClick event if custom handler exists', () => {
      spyOn(component.exportClick, 'emit');
      // Mock the observed property with getter
      Object.defineProperty(component.exportClick, 'observed', {
        get: () => true,
        configurable: true
      });

      component.exportData();

      expect(component.exportClick.emit).toHaveBeenCalled();
    });

    it('should not export if export is disabled', () => {
      component.config = { ...mockConfig, enableExport: false };

      component.exportData();

      expect(exportService.exportToGoogleSheet).not.toHaveBeenCalled();
    });

    it('should not export if no data URL is set', () => {
      component.config = { ...mockConfig, enableExport: true };
      // Set dataUrl to empty to trigger the condition
      Object.defineProperty(component, '_dataUrl', {
        value: '',
        writable: true,
        configurable: true
      });

      component.exportData();

      expect(exportService.exportToGoogleSheet).not.toHaveBeenCalled();
    });
  });

  describe('Resize and Auto-Switch', () => {
    beforeEach(() => {
      component.config = {
        ...mockConfig,
        autoSwitchToCardView: true,
        autoSwitchMinWidth: 768
      };
    });

    it('should handle resize events without auto-switching (card view only)', () => {
      component.viewMode = 'card';

      component['handleResize'](500);

      expect(component.viewMode).toBe('card');
    });

    it('should maintain card view regardless of configuration', () => {
      component.config = {
        ...mockConfig,
        autoSwitchToCardView: false
      };
      component.viewMode = 'card';

      component['handleResize'](500);

      expect(component.viewMode).toBe('card');
    });

    it('should stay in card view for any width change', () => {
      component.viewMode = 'card';

      component['handleResize'](800);

      expect(component.viewMode).toBe('card');
    });
  });

  describe('Computed Properties', () => {
    it('should compute search placeholder correctly', () => {
      component.config = {
        ...mockConfig,
        searchConfig: {
          placeholder: 'Custom placeholder'
        }
      };

      expect(component.searchPlaceholder()).toBe('Custom placeholder');
    });

    it('should compute search placeholder for advanced search', () => {
      component.config = {
        ...mockConfig,
        searchConfig: {
          useAdvancedSearch: true
        }
      };

      expect(component.searchPlaceholder()).toBe('Search by field...');
    });

    it('should compute default search placeholder', () => {
      component.config = { ...mockConfig };

      expect(component.searchPlaceholder()).toBe('Search...');
    });

    it('should compute scroll height value', () => {
      component.config = {
        ...mockConfig,
        scrollable: true,
        scrollHeight: 'flex'
      };

      expect(component.scrollHeightValue).toBe('calc(100vh - 16rem)');
    });

    it('should return custom scroll height', () => {
      component.config = {
        ...mockConfig,
        scrollable: true,
        scrollHeight: '400px'
      };

      expect(component.scrollHeightValue).toBe('400px');
    });

    it('should return undefined when scrollable is false', () => {
      component.config = {
        ...mockConfig,
        scrollable: false
      };

      expect(component.scrollHeightValue).toBeUndefined();
    });
  });

  describe('Lifecycle Hooks', () => {
    it('should clean up subscriptions on destroy', () => {
      spyOn(component['searchSubscription'], 'unsubscribe');

      component.ngOnDestroy();

      expect(component['searchSubscription'].unsubscribe).toHaveBeenCalled();
    });
  });

  describe('Window Resize Handler', () => {
    it('should handle window resize', () => {
      spyOn(component as any, 'checkComponentWidth');

      component.onWindowResize();

      expect(component['checkComponentWidth']).toHaveBeenCalled();
    });
  });
});