import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ConfirmationService } from 'primeng/api';
import { of, Subject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

import { ListviewComponent } from './listview.component';
import { ListviewExportService } from './listview-export.service';
import { ListViewColumn, ListViewConfig, SearchCriteria } from './listview.model';
import { GlobalFilterService } from '@core/services/filters';
import { AuthService } from '@core/services/auth';
import { GlobalFiltersDialogService } from '@core/services/filters';
import { GlobalFilters, UserPreferenceService } from '@core/services/user';

describe('ListviewComponent', () => {
  let component: ListviewComponent;
  let fixture: ComponentFixture<ListviewComponent>;
  let exportService: jasmine.SpyObj<ListviewExportService>;
  let router: jasmine.SpyObj<Router>;
  let activatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let translateService: TranslateService;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let globalFilterService: jasmine.SpyObj<GlobalFilterService>;
  let authService: jasmine.SpyObj<AuthService>;
  let userPreferenceService: jasmine.SpyObj<UserPreferenceService>;
  let globalFiltersDialogService: jasmine.SpyObj<GlobalFiltersDialogService>;

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
    routerSpy.navigate.and.returnValue(Promise.resolve(true));
    
    const activatedRouteSpy = jasmine.createSpyObj('ActivatedRoute', [], {
      snapshot: {
        queryParams: {}
      }
    });

    const confirmationSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);

    const globalFilterSpy = jasmine.createSpyObj('GlobalFilterService', ['setFilterEnabled'], {
      filtersChanged$: new Subject<void>()
    });

    const authSpy = jasmine.createSpyObj('AuthService', ['user']);
    authSpy.user.and.returnValue(of([{ type: 'userId', value: '1' }]));

    const userPreferenceSpy = jasmine.createSpyObj('UserPreferenceService', ['getGlobalFilters']);
    const emptyFilters: GlobalFilters = {
      orgUnitId: null,
      orgUnitName: null,
      relatedToMe: false,
      dateOn: null,
      dateFrom: null,
      dateTo: null
    };
    userPreferenceSpy.getGlobalFilters.and.returnValue(of(emptyFilters));

    const globalFiltersDialogSpy = jasmine.createSpyObj('GlobalFiltersDialogService', ['openDialog']);

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
        { provide: ConfirmationService, useValue: confirmationSpy },
        { provide: GlobalFilterService, useValue: globalFilterSpy },
        { provide: AuthService, useValue: authSpy },
        { provide: UserPreferenceService, useValue: userPreferenceSpy },
        { provide: GlobalFiltersDialogService, useValue: globalFiltersDialogSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ListviewComponent);
    component = fixture.componentInstance;
    exportService = TestBed.inject(ListviewExportService) as jasmine.SpyObj<ListviewExportService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    activatedRoute = TestBed.inject(ActivatedRoute) as jasmine.SpyObj<ActivatedRoute>;
    translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.returnValue('Translated text');
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    globalFilterService = TestBed.inject(GlobalFilterService) as jasmine.SpyObj<GlobalFilterService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    userPreferenceService = TestBed.inject(UserPreferenceService) as jasmine.SpyObj<UserPreferenceService>;
    globalFiltersDialogService = TestBed.inject(GlobalFiltersDialogService) as jasmine.SpyObj<GlobalFiltersDialogService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default values', () => {
      expect(component.viewMode).toBe('card');
      expect(component['state']().searchText).toBe('');
      expect(component['state']().pageIndex).toBe(1);
      expect(component['state']().pageSize).toBe(20);
    });

    it('should initialize with provided config', () => {
      component.config = mockConfig;
      expect(component.config.pageSize).toBe(20);
      expect(component.config.enableSearch).toBe(true);
    });
  });

  describe('Column Configuration', () => {
    it('should accept column configuration', () => {
      fixture.componentRef.setInput('columns', mockColumns);
      expect(component.columns().length).toBe(3);
      expect(component.columns()[0].label).toBe('Name');
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
      component['state'].update(s => ({ ...s, searchText: 'test' }));
      component.searchValue = 'test';

      component.clearSearch();

      expect(component['state']().searchText).toBe('');
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

      expect(component.isAdvancedSearch()).toBe(true);
      expect(component.searchValue).toBe('');
    });

    it('should switch back to simple search mode', () => {
      component['state'].update(s => ({ ...s, isAdvancedSearchMode: true }));
      component.searchCriteria = [
        { field: 'name', value: 'test', label: 'Name', operator: 'like' }
      ];

      component.switchToSimpleSearch();

      expect(component.isAdvancedSearch()).toBe(false);
      expect(component.searchCriteria.length).toBe(0);
    });
  });

  describe('Pagination', () => {
    it('should handle pagination properties', () => {
      expect(component['state']().pageIndex).toBe(1);
      expect(component['state']().pageSize).toBe(20);
      
      component.dataLoader.setPagination(20, 10);
      expect(component['state']().pageIndex).toBe(3);
      expect(component['state']().pageSize).toBe(10);
    });
  });

  describe('Sorting', () => {
    it('should handle sort change', () => {
      const sortEvent = { field: 'name', order: 1 };
      spyOn(component.sortChange, 'emit');

      component.onSortChange(sortEvent);

      expect(component.currentSortField()).toBe('name');
      expect(component.currentSortOrder()).toBe('asc');
      expect(component.sortChange.emit).toHaveBeenCalledWith({ field: 'name', order: 'asc' });
    });

    it('should handle descending sort', () => {
      const sortEvent = { field: 'name', order: -1 };

      component.onSortChange(sortEvent);

      expect(component.currentSortOrder()).toBe('desc');
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

  describe('Responsive Mode', () => {
    it('should force mobile mode when configured', () => {
      component.config = {
        ...mockConfig,
        forceMobileMode: true
      };
      component['state'].update(s => ({ ...s, componentWidth: 1200 }));

      expect(component.isMobileMode()).toBe(true);
    });

    it('should use auto-switch threshold for mobile mode', () => {
      component.config = {
        ...mockConfig,
        autoSwitchToCardView: true,
        autoSwitchMinWidth: 768
      };
      component['state'].update(s => ({ ...s, componentWidth: 500 }));

      expect(component.isMobileMode()).toBe(true);
    });

    it('should not enable mobile mode for wide layouts', () => {
      component.config = {
        ...mockConfig,
        autoSwitchToCardView: true,
        autoSwitchMinWidth: 768
      };
      component['state'].update(s => ({ ...s, componentWidth: 900 }));

      expect(component.isMobileMode()).toBe(false);
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


  describe('Window Resize Handler', () => {
    it('should handle window resize', () => {
      spyOn(component as any, 'checkComponentWidth');

      component.onWindowResize();

      expect(component['checkComponentWidth']).toHaveBeenCalled();
    });
  });

  describe('Filter Param Integration', () => {
    let httpMock: HttpTestingController;

    beforeEach(() => {
      httpMock = TestBed.inject(HttpTestingController);
      
      component.dataUrl = '/api/test';
      component.config = {
        pageSize: 20,
        enablePagination: true,
        entityName: 'Test'
      } as ListViewConfig;
      fixture.componentRef.setInput('columns', [
        { field: 'id', label: 'ID', type: 'number' },
        { field: 'name', label: 'Name', type: 'string' }
      ]);
    });

    afterEach(() => {
      httpMock.verify();
    });

    it('should include filterActive param by default', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      const req = httpMock.expectOne(request => {
        return request.url === '/api/test' &&
          request.params.get('pageIndex') === '1' &&
          request.params.get('pageSize') === '20' &&
          request.params.get('filterActive') === 'true';
      });

      req.flush({ records: [], totalCount: 0 });
    }));

    it('should set filterActive to false when filters are disabled', fakeAsync(() => {
      fixture.detectChanges();
      tick();

      let req = httpMock.expectOne(request => request.params.get('filterActive') === 'true');
      req.flush({ records: [], totalCount: 0 });

      component.toggleGlobalFilter();
      tick();

      req = httpMock.expectOne(request => request.params.get('filterActive') === 'false');
      req.flush({ records: [], totalCount: 0 });
    }));
  });
});
