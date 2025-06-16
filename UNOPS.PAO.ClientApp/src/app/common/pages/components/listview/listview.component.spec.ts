import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { of, Subject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ListviewComponent } from './listview.component';
import { ListviewDataLoaderService } from './listview-data-loader.service';
import { ListviewExportService } from './listview-export.service';
import { ListViewColumn, ListViewConfig, SearchCriteria } from './listview.model';

describe('ListviewComponent', () => {
  let component: ListviewComponent;
  let fixture: ComponentFixture<ListviewComponent>;
  let dataLoaderService: jasmine.SpyObj<ListviewDataLoaderService>;
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
    const dataLoaderSpy = jasmine.createSpyObj('ListviewDataLoaderService', [
      'setUrl', 'loadData', 'setPagination', 'setSorting', 'setSearchText', 
      'setAdvancedSearchEnabled', 'setSearchCriteria', 'addSearchCriterion',
      'removeSearchCriterionByIndex', 'clearSearchCriteria', 'getSearchParams',
      'setMyOfficeFilter'
    ], {
      isLoading: jasmine.createSpy().and.returnValue(false),
      hasError: jasmine.createSpy().and.returnValue(false),
      currentPageData: jasmine.createSpy().and.returnValue(mockData),
      totalRecordsCount: jasmine.createSpy().and.returnValue(2)
    });

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
        { provide: ListviewDataLoaderService, useValue: dataLoaderSpy },
        { provide: ListviewExportService, useValue: exportSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRouteSpy },
        { provide: TranslateService, useValue: translateSpy },
        { provide: ConfirmationService, useValue: confirmationSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewComponent);
    component = fixture.componentInstance;
    dataLoaderService = TestBed.inject(ListviewDataLoaderService) as jasmine.SpyObj<ListviewDataLoaderService>;
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
      expect(component.viewMode).toBe('table');
      expect(component.searchText).toBe('');
      expect(component.first).toBe(0);
      expect(component.searchCriteria).toEqual([]);
    });

    it('should set up search debounce with default time', () => {
      expect(component.searchDebounceTime).toBe(500);
    });

    it('should initialize with provided config', () => {
      component.config = mockConfig;
      expect(component.config.pageSize).toBe(20);
      expect(component.config.enableSearch).toBe(true);
    });
  });

  describe('Data URL Configuration', () => {
    it('should set data URL and load data', () => {
      const testUrl = '/api/test-data';
      component.dataUrl = testUrl;

      expect(dataLoaderService.setUrl).toHaveBeenCalledWith(testUrl);
      expect(dataLoaderService.loadData).toHaveBeenCalled();
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
      component.config = mockConfig;
      component.columns = mockColumns;
    });

    it('should handle simple search input', () => {
      const searchValue = 'test search';
      component.onSearchInput(searchValue);
      
      // Should trigger debounced search
      expect(component.searchText).toBe('');
    });

    it('should execute search with debounce', (done) => {
      const searchValue = 'test search';
      dataLoaderService.getSearchParams.and.returnValue({ generalSearch: searchValue });
      
      spyOn(component.searchChange, 'emit');
      
      component.onSearchInput(searchValue);
      
      setTimeout(() => {
        expect(dataLoaderService.setSearchText).toHaveBeenCalledWith(searchValue);
        expect(dataLoaderService.setPagination).toHaveBeenCalledWith(0, component.rows);
        expect(dataLoaderService.loadData).toHaveBeenCalled();
        done();
      }, 600);
    });

    it('should clear search', () => {
      component.searchText = 'existing search';
      dataLoaderService.getSearchParams.and.returnValue({});
      spyOn(component.searchChange, 'emit');

      component.clearSearch();

      expect(component.searchText).toBe('');
      expect(dataLoaderService.setSearchText).toHaveBeenCalledWith('');
      expect(component.searchChange.emit).toHaveBeenCalled();
      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });
  });

  describe('Advanced Search', () => {
    beforeEach(() => {
      const advancedConfig = {
        ...mockConfig,
        searchConfig: {
          useAdvancedSearch: true,
          searchableFields: [
            { field: 'name', label: 'Name', type: 'string' as const, operators: ['is', 'is not', 'like', 'not like'] },
            { field: 'email', label: 'Email', type: 'string' as const, operators: ['is', 'is not', 'like', 'not like'] }
          ]
        }
      };
      component.config = advancedConfig;
      component.columns = mockColumns;
    });

    it('should add search criterion', () => {
      const criterion: SearchCriteria = {
        field: 'name',
        label: 'Name',
        value: 'test',
        operator: 'like'
      };

      component.onAdvancedSearch(criterion);

      expect(component.searchCriteria).toContain(criterion);
      expect(dataLoaderService.addSearchCriterion).toHaveBeenCalledWith(criterion);
      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });

    it('should remove search criterion by index', () => {
      const criterion: SearchCriteria = {
        field: 'name',
        label: 'Name',
        value: 'test',
        operator: 'like'
      };
      component.searchCriteria = [criterion];

      component.onRemoveSearchCriterion(0);

      expect(component.searchCriteria.length).toBe(0);
      expect(dataLoaderService.removeSearchCriterionByIndex).toHaveBeenCalledWith(0);
      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });

    it('should clear all advanced search criteria', () => {
      component.searchCriteria = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      dataLoaderService.getSearchParams.and.returnValue({});
      spyOn(component.searchChange, 'emit');

      component.onClearAdvancedSearch();

      expect(component.searchCriteria.length).toBe(0);
      expect(dataLoaderService.clearSearchCriteria).toHaveBeenCalled();
      expect(component.searchChange.emit).toHaveBeenCalled();
      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });

    it('should switch to advanced search mode', () => {
      component.switchToAdvancedSearch();

      expect(component.isAdvancedSearchMode()).toBe(true);
      expect(dataLoaderService.setAdvancedSearchEnabled).toHaveBeenCalledWith(true);
      expect(component.searchValue).toBe('');
      expect(component.searchText).toBe('');
    });

    it('should switch back to simple search mode', () => {
      component.isAdvancedSearchMode.set(true);
      component.searchCriteria = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];

      component.switchToSimpleSearch();

      expect(component.isAdvancedSearchMode()).toBe(false);
      expect(dataLoaderService.setAdvancedSearchEnabled).toHaveBeenCalledWith(false);
      expect(component.searchCriteria.length).toBe(0);
      expect(dataLoaderService.clearSearchCriteria).toHaveBeenCalled();
    });
  });

  describe('Pagination', () => {
    it('should handle page change', () => {
      const pageEvent = { first: 20, rows: 20 };
      spyOn(component.pageChange, 'emit');

      component.onPageChange(pageEvent);

      expect(component.first).toBe(20);
      expect(component.rows).toBe(20);
      expect(component.pageChange.emit).toHaveBeenCalledWith(pageEvent);
      expect(dataLoaderService.setPagination).toHaveBeenCalledWith(1, 20);
      expect(dataLoaderService.loadData).toHaveBeenCalled();
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
      expect(dataLoaderService.setSorting).toHaveBeenCalledWith('name', 'asc');
      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });

    it('should handle descending sort', () => {
      const sortEvent = { field: 'name', order: -1 };

      component.onSortChange(sortEvent);

      expect(component.currentSortOrder).toBe('desc');
      expect(dataLoaderService.setSorting).toHaveBeenCalledWith('name', 'desc');
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
    it('should set view mode', () => {
      spyOn(component.viewModeChange, 'emit');

      component.setViewMode('card');

      expect(component.viewMode).toBe('card');
      expect(component.viewModeChange.emit).toHaveBeenCalledWith('card');
    });

    it('should track user selected view mode', () => {
      component.setViewMode('card', true);
      expect(component['userSelectedViewMode']).toBe('card');
    });

    it('should not track auto-switched view mode', () => {
      component.setViewMode('card', false);
      expect(component['userSelectedViewMode']).toBeNull();
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
      dataLoaderService.getSearchParams.and.returnValue({});

      component.exportData();

      expect(exportService.exportToGoogleSheet).toHaveBeenCalledWith(
        'Test Entity',
        '/api/test-data',
        {},
        undefined,
        undefined,
        undefined
      );
    });

    it('should emit exportClick event if custom handler exists', () => {
      spyOn(component.exportClick, 'emit');
      Object.defineProperty(component.exportClick, 'observed', {
        get: () => true
      });

      component.exportData();

      expect(component.exportClick.emit).toHaveBeenCalled();
      expect(exportService.exportToGoogleSheet).not.toHaveBeenCalled();
    });

    it('should not export if export is disabled', () => {
      component.config = { ...mockConfig, enableExport: false };

      component.exportData();

      expect(exportService.exportToGoogleSheet).not.toHaveBeenCalled();
    });

    it('should not export if no data URL is set', () => {
      component.dataUrl = '';

      component.exportData();

      expect(exportService.exportToGoogleSheet).not.toHaveBeenCalled();
    });
  });

  describe('URL Search Criteria Sync', () => {
    it('should sync search criteria to URL', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      component.searchCriteria = criteria;

      component['syncSearchCriteriaToUrl']();

      expect(router.navigate).toHaveBeenCalledWith([], {
        relativeTo: activatedRoute,
        queryParams: {
          searchCriteria: JSON.stringify(criteria),
          advancedSearch: 'true'
        },
        replaceUrl: true
      });
    });

    it('should clear search criteria from URL when empty', () => {
      component.searchCriteria = [];

      component['syncSearchCriteriaToUrl']();

      expect(router.navigate).toHaveBeenCalledWith([], {
        relativeTo: activatedRoute,
        queryParams: {},
        replaceUrl: true
      });
    });
  });

  describe('My Office Filter', () => {
    it('should handle My Office filter change', () => {
      component.onMyOfficeFilterChanged(true);

      expect(dataLoaderService.setMyOfficeFilter).toHaveBeenCalledWith(true);
      expect(dataLoaderService.loadData).toHaveBeenCalled();
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

    it('should auto-switch to card view when width is below threshold', () => {
      component.viewMode = 'table';
      spyOn(component, 'setViewMode');

      component['handleResize'](500);

      expect(component.setViewMode).toHaveBeenCalledWith('card', false);
      expect(component.isAutoSwitchedToCardView).toBe(true);
    });

    it('should not auto-switch when disabled', () => {
      component.config = {
        ...mockConfig,
        autoSwitchToCardView: false
      };
      component.viewMode = 'table';
      spyOn(component, 'setViewMode');

      component['handleResize'](500);

      expect(component.setViewMode).not.toHaveBeenCalled();
    });

    it('should switch back to table view when width increases and was auto-switched', () => {
      component.viewMode = 'card';
      component.isAutoSwitchedToCardView = true;
      component['userSelectedViewMode'] = null;
      spyOn(component, 'setViewMode');

      component['handleResize'](800);

      expect(component.setViewMode).toHaveBeenCalledWith('table', false);
      expect(component.isAutoSwitchedToCardView).toBe(false);
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
      component.config = mockConfig;

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
      const subscription = jasmine.createSpyObj('Subscription', ['unsubscribe']);
      component['searchSubscription'] = subscription;

      component.ngOnDestroy();

      expect(subscription.unsubscribe).toHaveBeenCalled();
    });

    it('should clean up resize observer on destroy', () => {
      const resizeObserver = jasmine.createSpyObj('ResizeObserver', ['disconnect']);
      component['resizeObserver'] = resizeObserver;

      component.ngOnDestroy();

      expect(resizeObserver.disconnect).toHaveBeenCalled();
    });
  });

  describe('Window Resize Handler', () => {
    it('should handle window resize', () => {
      spyOn(component as any, 'checkComponentWidth');

      component.onWindowResize();

      expect(component['checkComponentWidth']).toHaveBeenCalled();
    });
  });

  describe('Refresh Data Handler', () => {
    it('should refresh data when window refresh event is triggered', () => {
      component.refreshData();

      expect(dataLoaderService.loadData).toHaveBeenCalled();
    });
  });
});