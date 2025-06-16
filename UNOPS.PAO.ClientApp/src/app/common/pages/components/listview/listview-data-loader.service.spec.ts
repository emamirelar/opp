import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';

import { ListviewDataLoaderService } from './listview-data-loader.service';
import { SearchCriteria, ListViewData } from './listview.model';

describe('ListviewDataLoaderService', () => {
  let service: ListviewDataLoaderService;
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;

  const mockData = [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
  ];

  const mockListViewData: ListViewData<any> = {
    records: mockData,
    totalCount: 2
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ListviewDataLoaderService]
    });

    service = TestBed.inject(ListviewDataLoaderService);
    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should initialize with default values', () => {
      expect(service.isLoading()).toBe(false);
      expect(service.hasError()).toBe(false);
      expect(service.currentPageData()).toEqual([]);
      expect(service.totalRecordsCount()).toBe(0);
    });
  });

  describe('URL Configuration', () => {
    it('should set URL', () => {
      const testUrl = '/api/test-data';
      service.setUrl(testUrl);
      
      expect(service['url']).toBe(testUrl);
    });
  });

  describe('Direct Data Setting', () => {
    it('should set data directly', () => {
      service.setData(mockData);

      expect(service.currentPageData()).toEqual(mockData);
      expect(service.totalRecordsCount()).toBe(2);
      expect(service['url']).toBe('');
    });
  });

  describe('Pagination', () => {
    it('should set pagination parameters', () => {
      service.setPagination(2, 50);

      expect(service['pageIndex']).toBe(3); // 0-based to 1-based conversion
      expect(service['pageSize']).toBe(50);
    });

    it('should handle zero-based page index', () => {
      service.setPagination(0, 20);

      expect(service['pageIndex']).toBe(1);
      expect(service['pageSize']).toBe(20);
    });
  });

  describe('Sorting', () => {
    it('should set sorting parameters', () => {
      service.setSorting('name', 'desc');

      expect(service['sortField']).toBe('name');
      expect(service['sortOrder']).toBe('desc');
    });

    it('should set ascending sort', () => {
      service.setSorting('email', 'asc');

      expect(service['sortField']).toBe('email');
      expect(service['sortOrder']).toBe('asc');
    });
  });

  describe('Simple Search', () => {
    it('should set search text', () => {
      service.setSearchText('test search');

      expect(service['searchText']).toBe('test search');
    });

    it('should clear advanced search criteria when setting search text', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);
      service.setAdvancedSearchEnabled(false);

      service.setSearchText('simple search');

      expect(service['searchText']).toBe('simple search');
      expect(service['searchCriteria']).toEqual([]);
    });

    it('should get search params for simple search', () => {
      service.setSearchText('test');
      service.setMyOfficeFilter(true);

      const params = service.getSearchParams();

      expect(params.generalSearch).toBe('test');
      expect(params.myOfficeOnly).toBe(true);
      expect(params.fieldSearches).toBeUndefined();
    });
  });

  describe('Advanced Search', () => {
    beforeEach(() => {
      service.setAdvancedSearchEnabled(true);
    });

    it('should enable advanced search', () => {
      service.setAdvancedSearchEnabled(true);

      expect(service['useAdvancedSearch']).toBe(true);
    });

    it('should set search criteria', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];

      service.setSearchCriteria(criteria);

      expect(service['searchCriteria']).toEqual(criteria);
      expect(service['searchText']).toBe('');
    });

    it('should add search criterion', () => {
      const criterion: SearchCriteria = {
        field: 'name', 
        label: 'Name', 
        value: 'test', 
        operator: 'like'
      };

      service.addSearchCriterion(criterion);

      expect(service['searchCriteria']).toContain(criterion);
    });

    it('should remove search criterion by field', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test1', operator: 'like' },
        { field: 'email', label: 'Email', value: 'test2', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);

      service.removeSearchCriterion('name');

      expect(service['searchCriteria'].length).toBe(1);
      expect(service['searchCriteria'][0].field).toBe('email');
    });

    it('should remove search criterion by index', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test1', operator: 'like' },
        { field: 'email', label: 'Email', value: 'test2', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);

      service.removeSearchCriterionByIndex(0);

      expect(service['searchCriteria'].length).toBe(1);
      expect(service['searchCriteria'][0].field).toBe('email');
    });

    it('should handle invalid index when removing criterion', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);

      service.removeSearchCriterionByIndex(5);

      expect(service['searchCriteria'].length).toBe(1);
    });

    it('should clear all search criteria', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);
      service.setSearchText('simple search');
      service.setMyOfficeFilter(true);

      service.clearSearchCriteria();

      expect(service['searchCriteria']).toEqual([]);
      expect(service['searchText']).toBe('');
      expect(service['myOfficeOnly']).toBe(false);
    });

    it('should get search params for advanced search', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);
      service.setMyOfficeFilter(true);

      const params = service.getSearchParams();

      expect(params.fieldSearches).toEqual(criteria);
      expect(params.myOfficeOnly).toBe(true);
      expect(params.generalSearch).toBeUndefined();
    });
  });

  describe('My Office Filter', () => {
    it('should set My Office filter', () => {
      service.setMyOfficeFilter(true);

      expect(service.getMyOfficeFilter()).toBe(true);
    });

    it('should get My Office filter state', () => {
      service.setMyOfficeFilter(false);

      expect(service.getMyOfficeFilter()).toBe(false);
    });
  });

  describe('Data Loading', () => {
    beforeEach(() => {
      service.setUrl('/api/test-data');
    });

    it('should load data and set loading state', () => {
      service.loadData();

      expect(service.isLoading()).toBe(true);
      expect(service.hasError()).toBe(false);

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      expect(req.request.method).toBe('GET');

      req.flush(mockListViewData);

      expect(service.isLoading()).toBe(false);
      expect(service.currentPageData()).toEqual(mockData);
      expect(service.totalRecordsCount()).toBe(2);
    });

    it('should handle array response format', () => {
      service.loadData();

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.flush(mockData);

      expect(service.currentPageData()).toEqual(mockData);
      expect(service.totalRecordsCount()).toBe(2);
    });

    it('should handle object response format with records property', () => {
      service.loadData();

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.flush(mockListViewData);

      expect(service.currentPageData()).toEqual(mockData);
      expect(service.totalRecordsCount()).toBe(2);
    });

    it('should handle error response', () => {
      service.loadData();

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.error(new ErrorEvent('Network error'));

      expect(service.isLoading()).toBe(false);
      expect(service.hasError()).toBe(true);
      expect(service.currentPageData()).toEqual([]);
      expect(service.totalRecordsCount()).toBe(0);
    });

    it('should not make request when URL is empty', () => {
      service.setUrl('');
      service.loadData();

      httpMock.expectNone(() => true);
    });
  });

  describe('HTTP Parameters', () => {
    beforeEach(() => {
      service.setUrl('/api/test-data');
    });

    it('should include pagination parameters', () => {
      service.setPagination(1, 25);
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               req.params.get('pageIndex') === '2' &&
               req.params.get('pageSize') === '25';
      });

      req.flush(mockData);
    });

    it('should include sorting parameters', () => {
      service.setSorting('name', 'desc');
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               req.params.get('orderBy') === 'name' &&
               req.params.get('ascending') === 'false';
      });

      req.flush(mockData);
    });

    it('should include simple search parameters', () => {
      service.setAdvancedSearchEnabled(false);
      service.setSearchText('test search');
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               req.params.get('searchText') === 'test search';
      });

      req.flush(mockData);
    });

    it('should include advanced search parameters', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setAdvancedSearchEnabled(true);
      service.setSearchCriteria(criteria);
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               req.params.get('advancedSearch') === 'true' &&
               req.params.get('searchCriteria') === JSON.stringify(criteria);
      });

      req.flush(mockData);
    });

    it('should include My Office filter parameter', () => {
      service.setMyOfficeFilter(true);
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               req.params.get('myOfficeOnly') === 'true';
      });

      req.flush(mockData);
    });

    it('should not include empty search text', () => {
      service.setSearchText('   ');
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               !req.params.has('searchText');
      });

      req.flush(mockData);
    });

    it('should not include My Office filter when disabled', () => {
      service.setMyOfficeFilter(false);
      service.loadData();

      const req = httpMock.expectOne(req => {
        return req.url === '/api/test-data' &&
               !req.params.has('myOfficeOnly');
      });

      req.flush(mockData);
    });
  });

  describe('Data Response Handling', () => {
    beforeEach(() => {
      service.setUrl('/api/test-data');
    });

    it('should handle response with data property', () => {
      service.loadData();

      const response = {
        data: mockData,
        total: 2
      };

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.flush(response);

      expect(service.currentPageData()).toEqual(mockData);
      expect(service.totalRecordsCount()).toBe(2);
    });

    it('should handle malformed response', () => {
      service.loadData();

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.flush({ invalid: 'response' });

      expect(service.currentPageData()).toEqual([]);
      expect(service.totalRecordsCount()).toBe(0);
    });

    it('should handle null response', () => {
      service.loadData();

      const req = httpMock.expectOne(req => req.url === '/api/test-data');
      req.flush(null);

      expect(service.currentPageData()).toEqual([]);
      expect(service.totalRecordsCount()).toBe(0);
    });
  });

  describe('Search Mode Transitions', () => {
    it('should clear advanced search when switching to simple search', () => {
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      
      service.setAdvancedSearchEnabled(true);
      service.setSearchCriteria(criteria);
      
      service.setAdvancedSearchEnabled(false);
      service.setSearchText('simple search');

      expect(service['searchCriteria']).toEqual([]);
      expect(service['searchText']).toBe('simple search');
    });

    it('should clear simple search when switching to advanced search', () => {
      service.setAdvancedSearchEnabled(false);
      service.setSearchText('simple search');
      
      service.setAdvancedSearchEnabled(true);
      const criteria: SearchCriteria[] = [
        { field: 'name', label: 'Name', value: 'test', operator: 'like' }
      ];
      service.setSearchCriteria(criteria);

      expect(service['searchText']).toBe('');
      expect(service['searchCriteria']).toEqual(criteria);
    });
  });
});