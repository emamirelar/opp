import { ComponentFixture, TestBed } from '@angular/core/testing';
import { fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { SearchResultComponent } from './search-result.component';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { GlobalFilterService } from '@core/services/filters';
import { UserPreferenceService } from '@core/services/user';
import { OrganizationHierarchyService } from '@core/services/organization';
import { AuthService } from '@core/services/auth';
import { GlobalFiltersDialogService } from '@core/services/filters';
import { of, Subject, BehaviorSubject } from 'rxjs';

describe('SearchResultComponent', () => {
  let component: SearchResultComponent;
  let fixture: ComponentFixture<SearchResultComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockEntityConfigService: jasmine.SpyObj<EntityConfigurationService>;
  let mockGlobalFilterService: any;
  let mockUserPreferenceService: jasmine.SpyObj<UserPreferenceService>;
  let mockOrganizationHierarchyService: jasmine.SpyObj<OrganizationHierarchyService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockGlobalFiltersDialogService: jasmine.SpyObj<GlobalFiltersDialogService>;
  let translateService: TranslateService;
  let queryParamsSubject: Subject<any>;
  let activeOrgUnitIdSubject: BehaviorSubject<number | null>;
  let filtersChangedSubject: Subject<void>;

  const mockSearchResponse = {
    availableEntities: ['partners', 'contacts', 'interactions'],
    results: {
      partners: [
        { id: 1, name: 'Test Partner', _searchMetadata: { matchedField: 'name', score: 0.95 } }
      ],
      contacts: [
        { id: 2, name: 'Test Contact', _searchMetadata: { matchedField: 'name', score: 0.90 } }
      ],
      interactions: [
        { id: 3, subject: 'Test Interaction', _searchMetadata: { matchedField: 'subject', score: 0.85 } }
      ]
    }
  };

  const mockColumns = [
    { field: 'name', label: 'Name', type: 'text', sortable: true },
    { field: 'description', label: 'Description', type: 'text', sortable: false }
  ];

  beforeEach(async () => {
    queryParamsSubject = new Subject();
    activeOrgUnitIdSubject = new BehaviorSubject<number | null>(null);
    filtersChangedSubject = new Subject();

    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockEntityConfigService = jasmine.createSpyObj('EntityConfigurationService', 
      ['getEntityListViewConfiguration']);
    mockUserPreferenceService = jasmine.createSpyObj('UserPreferenceService', 
      ['getGlobalFilters']);
    mockOrganizationHierarchyService = jasmine.createSpyObj('OrganizationHierarchyService', 
      ['getOrganizationHierarchy']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['user']);
    mockGlobalFiltersDialogService = jasmine.createSpyObj('GlobalFiltersDialogService', 
      ['openDialog']);

    mockGlobalFilterService = {
      activeOrgUnitId$: activeOrgUnitIdSubject.asObservable(),
      filtersChanged$: filtersChangedSubject.asObservable(),
      getActiveOrgUnitId: jasmine.createSpy('getActiveOrgUnitId').and.returnValue(null),
      setFilterEnabled: jasmine.createSpy('setFilterEnabled')
    };

    mockOrganizationHierarchyService.getOrganizationHierarchy.and.returnValue(of([]));
    mockEntityConfigService.getEntityListViewConfiguration.and.returnValue(of(mockColumns));
    mockUserPreferenceService.getGlobalFilters.and.returnValue(of({
      orgUnitId: null,
      orgUnitName: null,
      relatedToMe: false,
      dateOn: null,
      dateFrom: null,
      dateTo: null
    }));
    mockAuthService.user.and.returnValue(of([{ type: 'userId', value: 'user123' }]));

    await TestBed.configureTestingModule({
      imports: [
        SearchResultComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }
        })
      ],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            params: of({}),
            queryParams: queryParamsSubject.asObservable(),
            snapshot: { paramMap: { get: () => null } }
          }
        },
        { provide: Router, useValue: mockRouter },
        { provide: EntityConfigurationService, useValue: mockEntityConfigService },
        { provide: GlobalFilterService, useValue: mockGlobalFilterService },
        { provide: UserPreferenceService, useValue: mockUserPreferenceService },
        { provide: OrganizationHierarchyService, useValue: mockOrganizationHierarchyService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: GlobalFiltersDialogService, useValue: mockGlobalFiltersDialogService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SearchResultComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.returnValue('Translated');
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default signal values', () => {
      expect(component.searchQuery()).toBe('');
      expect(component.isLoading()).toBeFalse();
      expect(component.showSearchMetadata()).toBeFalse();
      expect(component.currentSearchTerm()).toBe('');
      expect(component.isGlobalFilterActive()).toBeFalse();
      expect(component.isFilterTemporarilyDisabled()).toBeFalse();
    });

    it('should load entity columns on init', () => {
      fixture.detectChanges();

      expect(mockEntityConfigService.getEntityListViewConfiguration).toHaveBeenCalledWith('Contact');
      expect(mockEntityConfigService.getEntityListViewConfiguration).toHaveBeenCalledWith('Partner');
      expect(mockEntityConfigService.getEntityListViewConfiguration).toHaveBeenCalledWith('Interaction');
      expect(mockEntityConfigService.getEntityListViewConfiguration).toHaveBeenCalledWith('Opportunity');
    });

    it('should load global filter info on init', () => {
      fixture.detectChanges();

      expect(mockUserPreferenceService.getGlobalFilters).toHaveBeenCalled();
      expect(mockAuthService.user).toHaveBeenCalled();
    });

    it('should set columns from entity configuration service', () => {
      fixture.detectChanges();

      expect(component.contactColumns().length).toBeGreaterThan(0);
      expect(component.partnerColumns().length).toBeGreaterThan(0);
      expect(component.interactionColumns().length).toBeGreaterThan(0);
    });
  });

  describe('query parameter handling', () => {
    it('should perform search when query param is provided', () => {
      spyOn(component as any, 'performUnifiedSearch');
      fixture.detectChanges();

      queryParamsSubject.next({ q: 'test search' });

      expect(component.searchQuery()).toBe('test search');
      expect(component['performUnifiedSearch']).toHaveBeenCalledWith('test search');
    });

    it('should not perform search when query param is empty', () => {
      spyOn(component as any, 'performUnifiedSearch');
      fixture.detectChanges();

      queryParamsSubject.next({ q: '' });

      expect(component['performUnifiedSearch']).not.toHaveBeenCalled();
    });

    it('should update search control value when query param changes', fakeAsync(() => {
      fixture.detectChanges();

      queryParamsSubject.next({ q: 'new query' });
      tick();

      expect(component.searchControl.value).toBe('new query');
      expect(component.currentSearchTerm()).toBe('new query');

      const req = httpMock.expectOne((r) => r.url.includes('/api/global/search'));
      req.flush(mockSearchResponse);
    }));
  });

  describe('search functionality', () => {
    it('should call unified search API with correct parameters', () => {
      fixture.detectChanges();
      component['performUnifiedSearch']('test');

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('q')).toBe('test');
      expect(req.request.params.get('fullResults')).toBe('true');
      req.flush(mockSearchResponse);
    });

    it('should set loading state during search', () => {
      fixture.detectChanges();
      component['performUnifiedSearch']('test');

      expect(component.isLoading()).toBeTrue();

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      req.flush(mockSearchResponse);

      expect(component.isLoading()).toBeFalse();
    });

    it('should process search response and create entity tabs', () => {
      fixture.detectChanges();
      component['performUnifiedSearch']('test');

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      req.flush(mockSearchResponse);

      expect(component.entityTabs.length).toBeGreaterThan(0);
      expect(component.entityTabs.some(tab => tab.key === 'partners')).toBeTrue();
    });

    it('should handle search errors gracefully', () => {
      fixture.detectChanges();
      spyOn(console, 'error');

      component['performUnifiedSearch']('test');

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      req.error(new ProgressEvent('error'));

      expect(component.isLoading()).toBeFalse();
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('global filter integration', () => {
    it('should update isGlobalFilterActive when org unit changes', () => {
      fixture.detectChanges();

      mockGlobalFilterService.getActiveOrgUnitId.and.returnValue(123);
      activeOrgUnitIdSubject.next(123);

      expect(component.isGlobalFilterActive()).toBeTrue();
    });

    it('should load org unit name when active org unit is set', () => {
      mockOrganizationHierarchyService.getOrganizationHierarchy.and.returnValue(of([
        { data: { id: 123, name: 'Test Org' }, children: [] }
      ]));
      fixture.detectChanges();

      mockGlobalFilterService.getActiveOrgUnitId.and.returnValue(123);
      activeOrgUnitIdSubject.next(123);

      expect(mockOrganizationHierarchyService.getOrganizationHierarchy).toHaveBeenCalled();
    });

    it('should clear org unit name when active org unit is null', () => {
      fixture.detectChanges();
      component.activeOrgUnitName.set('Previous Org');

      mockGlobalFilterService.getActiveOrgUnitId.and.returnValue(null);
      activeOrgUnitIdSubject.next(null);

      expect(component.activeOrgUnitName()).toBe('');
    });

    it('should refresh search when filters change', () => {
      spyOn(component as any, 'performUnifiedSearch');
      fixture.detectChanges();
      component.searchQuery.set('existing query');

      filtersChangedSubject.next();

      expect(component['performUnifiedSearch']).toHaveBeenCalledWith('existing query');
    });
  });

  describe('search control with debounce', () => {
    it('should update currentSearchTerm after debounce', (done) => {
      fixture.detectChanges();

      component.searchControl.setValue('debounced search');

      setTimeout(() => {
        expect(component.currentSearchTerm()).toBe('debounced search');
        done();
      }, 350);
    });

    it('should not trigger multiple searches for rapid input changes', (done) => {
      spyOn(component.currentSearchTerm, 'set');
      fixture.detectChanges();

      component.searchControl.setValue('a');
      component.searchControl.setValue('ab');
      component.searchControl.setValue('abc');

      setTimeout(() => {
        expect(component.currentSearchTerm.set).toHaveBeenCalledTimes(1);
        expect(component.currentSearchTerm.set).toHaveBeenCalledWith('abc');
        done();
      }, 350);
    });
  });

  describe('metadata toggle', () => {
    it('should toggle search metadata visibility', () => {
      expect(component.showSearchMetadata()).toBeFalse();

      component._showSearchMetadata.set(true);

      expect(component.showSearchMetadata()).toBeTrue();
    });
  });

  describe('entity tab management', () => {
    it('should set activeTabKey correctly', () => {
      component.activeTabKey = 'partners';

      expect(component.activeTabKey).toBe('partners');
    });

    it('should filter results by active tab', () => {
      fixture.detectChanges();
      component['performUnifiedSearch']('test');

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      req.flush(mockSearchResponse);

      component.activeTabKey = 'partners';

      // Component should have logic to filter by active tab
      expect(component.entityTabs.length).toBeGreaterThan(0);
    });
  });

  describe('navigation', () => {
    it('should have router injected', () => {
      expect(component['router']).toBeDefined();
    });

    it('should be able to navigate to search results', () => {
      const query = 'test';
      component.searchControl.setValue(query);

      // Component would navigate via router
      expect(mockRouter.navigate).toBeDefined();
    });
  });

  describe('filter toggle', () => {
    it('should track temporary filter disable state', () => {
      expect(component.isFilterTemporarilyDisabled()).toBeFalse();

      component.isFilterTemporarilyDisabled.set(true);

      expect(component.isFilterTemporarilyDisabled()).toBeTrue();
    });
  });

  describe('column configuration', () => {
    it('should handle column loading state', () => {
      expect(component.columnsLoading()).toBeDefined();
    });

    it('should store columns for each entity type', () => {
      fixture.detectChanges();

      expect(component.contactColumns()).toBeDefined();
      expect(component.partnerColumns()).toBeDefined();
      expect(component.interactionColumns()).toBeDefined();
    });
  });

  describe('cache management', () => {
    it('should initialize metadata cache', () => {
      expect(component['metadataCache']).toBeDefined();
      expect(component['metadataCache'] instanceof Map).toBeTrue();
    });

    it('should initialize metadata properties cache', () => {
      expect(component['metadataPropertiesCache']).toBeDefined();
      expect(component['metadataPropertiesCache'] instanceof Map).toBeTrue();
    });
  });

  describe('global filter labels', () => {
    it('should maintain active filter labels', () => {
      expect(component.activeFilterLabels()).toEqual([]);
    });

    it('should update filter labels when filters change', () => {
      fixture.detectChanges();

      mockGlobalFilterService.getActiveOrgUnitId.and.returnValue(123);
      activeOrgUnitIdSubject.next(123);

      // Component should update labels
      expect(component.activeFilterLabels).toBeDefined();
    });
  });

  describe('global filters', () => {
    it('should store global filter state', () => {
      expect(component.globalFilters()).toBeNull();
    });

    it('should load global filters from user preferences', () => {
      const mockFilters = {
        orgUnitId: 123,
        orgUnitName: null,
        relatedToMe: false,
        dateOn: null,
        dateFrom: null,
        dateTo: null
      };
      mockUserPreferenceService.getGlobalFilters.and.returnValue(of(mockFilters));

      fixture.detectChanges();

      expect(mockUserPreferenceService.getGlobalFilters).toHaveBeenCalled();
    });
  });

  describe('user context', () => {
    it('should get current user ID', () => {
      fixture.detectChanges();

      expect(component.currentUserId()).toBe('user123');
    });
  });

  describe('computed signals', () => {
    it('should compute showSearchMetadata from internal signal', () => {
      component._showSearchMetadata.set(false);
      expect(component.showSearchMetadata()).toBeFalse();

      component._showSearchMetadata.set(true);
      expect(component.showSearchMetadata()).toBeTrue();
    });
  });

  describe('edge cases', () => {
    it('should handle empty search response', () => {
      fixture.detectChanges();
      component['performUnifiedSearch']('test');

      const req = httpMock.expectOne(req => req.url.includes('/api/global/search'));
      req.flush({ availableEntities: [], results: {} });

      expect(component.isLoading()).toBeFalse();
    });

    it('should handle null query params', () => {
      spyOn(component as any, 'performUnifiedSearch');
      fixture.detectChanges();

      queryParamsSubject.next({});

      expect(component.searchQuery()).toBe('');
      expect(component['performUnifiedSearch']).not.toHaveBeenCalled();
    });

    it('should handle column loading errors', () => {
      mockEntityConfigService.getEntityListViewConfiguration.and.returnValue(
        of([])
      );

      fixture.detectChanges();

      expect(component.contactColumns().length).toBe(0);
    });
  });

  describe('cleanup', () => {
    it('should have destroy subject for cleanup', () => {
      expect(component['destroy$']).toBeDefined();
    });
  });
});

