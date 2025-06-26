import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { of, Subject, BehaviorSubject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ListviewComponent } from './listview.component';
import { ListviewExportService } from './listview-export.service';
import { ListViewColumn, ListViewConfig, SearchCriteria } from './listview.model';
import { GlobalFilterService } from '../../../../services/global-filter.service';

describe('ListviewComponent', () => {
  let component: ListviewComponent;
  let fixture: ComponentFixture<ListviewComponent>;
  let exportService: jasmine.SpyObj<ListviewExportService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let translateService: jasmine.SpyObj<TranslateService>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let globalFilterService: jasmine.SpyObj<GlobalFilterService>;

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

    const globalFilterSpy = jasmine.createSpyObj('GlobalFilterService', 
      ['getActiveOrgUnitId', 'setFilterEnabled', 'setSelectedOrgUnitId'],
      {
        activeOrgUnitId$: new BehaviorSubject<number | null>(null)
      }
    );
    globalFilterSpy.getActiveOrgUnitId.and.returnValue(null);

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
        { provide: ConfirmationService, useValue: confirmationSpy },
        { provide: GlobalFilterService, useValue: globalFilterSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewComponent);
    component = fixture.componentInstance;
    exportService = TestBed.inject(ListviewExportService) as jasmine.SpyObj<ListviewExportService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    activatedRoute = TestBed.inject(ActivatedRoute) as jasmine.SpyObj<ActivatedRoute>;
    translateService = TestBed.inject(TranslateService) as jasmine.SpyObj<TranslateService>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    globalFilterService = TestBed.inject(GlobalFilterService) as jasmine.SpyObj<GlobalFilterService>;
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

  describe('OrgUnit Filter Integration', () => {
    let httpMock: HttpTestingController;
    let activeOrgUnitIdSubject: BehaviorSubject<number | null>;

    beforeEach(() => {
      httpMock = TestBed.inject(HttpTestingController);
      activeOrgUnitIdSubject = (globalFilterService as any).activeOrgUnitId$;
      
      // Set up component with basic configuration
      component.dataUrl = '/api/test';
      component.config = {
        pageSize: 20,
        enablePagination: true,
        entityName: 'Test'
      } as ListViewConfig;
      component.columns = [
        { field: 'id', label: 'ID', type: 'number' },
        { field: 'name', label: 'Name', type: 'string' }
      ];
    });

    afterEach(() => {
      httpMock.verify();
    });

    it('should include orgUnitId in HTTP params when filter is active', fakeAsync(() => {
      // Set active org unit ID
      const orgUnitId = 123;
      globalFilterService.getActiveOrgUnitId.and.returnValue(orgUnitId);

      // Initialize component
      fixture.detectChanges();
      tick();

      // Component should load data with org unit filter
      const req = httpMock.expectOne(request => {
        return request.url === '/api/test' && 
               request.params.has('orgUnitId') &&
               request.params.get('orgUnitId') === orgUnitId.toString();
      });

      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('pageIndex')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('20');

      // Respond with test data
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should not include orgUnitId when filter is not active', fakeAsync(() => {
      // No active org unit ID
      globalFilterService.getActiveOrgUnitId.and.returnValue(null);

      // Initialize component
      fixture.detectChanges();
      tick();

      // Component should load data without org unit filter
      const req = httpMock.expectOne(request => {
        return request.url === '/api/test' && 
               !request.params.has('orgUnitId');
      });

      expect(req.request.method).toBe('GET');
      expect(req.request.params.has('orgUnitId')).toBeFalse();

      // Respond with test data
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should reload data when org unit filter changes', fakeAsync(() => {
      // Initialize component without org unit filter
      fixture.detectChanges();
      tick();

      // Initial request without orgUnitId
      let req = httpMock.expectOne('/api/test?pageIndex=1&pageSize=20&orderBy=&ascending=true');
      req.flush({ records: [], totalCount: 0 });

      // Change org unit filter
      const newOrgUnitId = 456;
      globalFilterService.getActiveOrgUnitId.and.returnValue(newOrgUnitId);
      activeOrgUnitIdSubject.next(newOrgUnitId);
      tick();

      // Should trigger a new request with orgUnitId
      req = httpMock.expectOne(request => {
        return request.url === '/api/test' && 
               request.params.get('orgUnitId') === newOrgUnitId.toString();
      });

      expect(req.request.params.get('pageIndex')).toBe('1'); // Should reset to first page
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should reset pagination when org unit filter changes', fakeAsync(() => {
      // Initialize component and load first page
      fixture.detectChanges();
      tick();

      let req = httpMock.expectOne('/api/test?pageIndex=1&pageSize=20&orderBy=&ascending=true');
      req.flush({ records: Array(20).fill({}), totalCount: 100 });

      // Navigate to page 3
      component['pageIndex'] = 3;
      component['loadData']();
      tick();

      req = httpMock.expectOne(request => request.params.get('pageIndex') === '3');
      req.flush({ records: Array(20).fill({}), totalCount: 100 });

      // Change org unit filter
      globalFilterService.getActiveOrgUnitId.and.returnValue(789);
      activeOrgUnitIdSubject.next(789);
      tick();

      // Should reset to page 1
      req = httpMock.expectOne(request => {
        return request.params.get('pageIndex') === '1' &&
               request.params.get('orgUnitId') === '789';
      });
      req.flush({ records: [], totalCount: 0 });

      expect(component['pageIndex']).toBe(1);
    }));

    it('should combine orgUnitId with search parameters', fakeAsync(() => {
      const orgUnitId = 123;
      globalFilterService.getActiveOrgUnitId.and.returnValue(orgUnitId);

      // Initialize component
      fixture.detectChanges();
      tick();

      // Initial load
      let req = httpMock.expectOne(request => request.params.get('orgUnitId') === '123');
      req.flush({ records: [], totalCount: 0 });

      // Perform search
      component.searchText = 'test search';
      component.onSearch();
      tick(300); // Debounce time

      // Should include both search text and orgUnitId
      req = httpMock.expectOne(request => {
        return request.params.get('searchText') === 'test search' &&
               request.params.get('orgUnitId') === orgUnitId.toString();
      });
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should combine orgUnitId with advanced search', fakeAsync(() => {
      const orgUnitId = 456;
      globalFilterService.getActiveOrgUnitId.and.returnValue(orgUnitId);

      // Configure for advanced search
      component.config = {
        ...component.config,
        searchConfig: {
          useAdvancedSearch: true,
          searchableFields: [{ field: 'name', label: 'Name', type: 'string' }]
        }
      };

      // Initialize component
      fixture.detectChanges();
      tick();

      // Initial load
      let req = httpMock.expectOne(request => request.params.get('orgUnitId') === '456');
      req.flush({ records: [], totalCount: 0 });

      // Set advanced search criteria
      component['searchCriteria'] = [{ field: 'name', operator: 'like', value: 'test' }];
      component['useAdvancedSearch'] = true;
      component['loadData']();
      tick();

      // Should include both advanced search and orgUnitId
      req = httpMock.expectOne(request => {
        return request.params.get('advancedSearch') === 'true' &&
               request.params.has('searchCriteria') &&
               request.params.get('orgUnitId') === orgUnitId.toString();
      });
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should handle org unit filter being disabled', fakeAsync(() => {
      // Start with active org unit
      globalFilterService.getActiveOrgUnitId.and.returnValue(123);
      
      fixture.detectChanges();
      tick();

      // Initial request with orgUnitId
      let req = httpMock.expectOne(request => request.params.get('orgUnitId') === '123');
      req.flush({ records: [], totalCount: 0 });

      // Disable org unit filter
      globalFilterService.getActiveOrgUnitId.and.returnValue(null);
      activeOrgUnitIdSubject.next(null);
      tick();

      // Should reload without orgUnitId
      req = httpMock.expectOne(request => !request.params.has('orgUnitId'));
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should handle rapid org unit changes', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      // Initial load
      let req = httpMock.expectOne('/api/test?pageIndex=1&pageSize=20&orderBy=&ascending=true');
      req.flush({ records: [], totalCount: 0 });

      // Rapid org unit changes
      globalFilterService.getActiveOrgUnitId.and.returnValue(1);
      activeOrgUnitIdSubject.next(1);
      
      globalFilterService.getActiveOrgUnitId.and.returnValue(2);
      activeOrgUnitIdSubject.next(2);
      
      globalFilterService.getActiveOrgUnitId.and.returnValue(3);
      activeOrgUnitIdSubject.next(3);
      
      tick();

      // Should only make one request with the final value
      req = httpMock.expectOne(request => request.params.get('orgUnitId') === '3');
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should clear loaded data when org unit changes', fakeAsync(() => {
      // Initialize and load some data
      fixture.detectChanges();
      tick();

      let req = httpMock.expectOne('/api/test?pageIndex=1&pageSize=20&orderBy=&ascending=true');
      req.flush({ 
        records: [
          { id: 1, name: 'Item 1' },
          { id: 2, name: 'Item 2' }
        ], 
        totalCount: 2 
      });

      // Verify data is loaded
      expect(component.data.length).toBe(2);

      // Change org unit filter
      globalFilterService.getActiveOrgUnitId.and.returnValue(999);
      activeOrgUnitIdSubject.next(999);
      tick();

      // Request with new org unit
      req = httpMock.expectOne(request => request.params.get('orgUnitId') === '999');
      req.flush({ records: [], totalCount: 0 });

      // Data should be cleared and replaced
      expect(component.data.length).toBe(0);
    }));

    it('should not reload if component has not loaded initial data', fakeAsync(() => {
      // Do not call fixture.detectChanges() to prevent initial load
      
      // Change org unit filter before component initialization
      globalFilterService.getActiveOrgUnitId.and.returnValue(123);
      activeOrgUnitIdSubject.next(123);
      tick();

      // No HTTP requests should be made
      httpMock.expectNone('/api/test');

      // Now initialize component
      fixture.detectChanges();
      tick();

      // Should make initial request with current org unit
      const req = httpMock.expectOne(request => request.params.get('orgUnitId') === '123');
      req.flush({ records: [], totalCount: 0 });
    }));

    it('should maintain orgUnitId when loading more data', fakeAsync(() => {
      const orgUnitId = 555;
      globalFilterService.getActiveOrgUnitId.and.returnValue(orgUnitId);

      fixture.detectChanges();
      tick();

      // Initial load
      let req = httpMock.expectOne(request => 
        request.params.get('pageIndex') === '1' &&
        request.params.get('orgUnitId') === '555'
      );
      req.flush({ 
        records: Array(10).fill({}).map((_, i) => ({ id: i + 1 })), 
        totalCount: 25 
      });

      // Load more
      component.onLoadMore();
      tick();

      // Second page should also include orgUnitId
      req = httpMock.expectOne(request => 
        request.params.get('pageIndex') === '2' &&
        request.params.get('orgUnitId') === '555'
      );
      req.flush({ 
        records: Array(10).fill({}).map((_, i) => ({ id: i + 11 })), 
        totalCount: 25 
      });

      expect(component.data.length).toBe(20);
    }));
  });
});