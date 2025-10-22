import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SavedFilterService } from './saved-filter.service';
import {
  SavedFilter,
  CreateSavedFilterRequest,
  UpdateSavedFilterRequest,
  SavedFilterSearchRequest,
  SavedFilterSearchResponse,
  ApplySavedFilterResponse,
  FilterStatistics
} from '@shared/interfaces/saved-filter.interface';

describe('SavedFilterService', () => {
  let service: SavedFilterService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SavedFilterService]
    });

    service = TestBed.inject(SavedFilterService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('createSavedFilter', () => {
    it('should create a new saved filter', (done) => {
      const request: CreateSavedFilterRequest = {
        name: 'My Filter',
        entityType: 'Contact',
        isAdvancedSearch: false,
        searchText: 'test',
        orderBy: 'name',
        ascending: true
      };
      const mockResponse: SavedFilter = {
        id: 1,
        name: 'My Filter',
        entityType: 'Contact',
        isAdvancedSearch: false,
        searchText: 'test',
        orderBy: 'name',
        ascending: true,
        usageCount: 0,
        createdDate: new Date('2024-01-01'),
        modifiedDate: new Date('2024-01-01'),
        createdBy: 'user1',
        modifiedBy: 'user1'
      };

      expect(service.isLoading()).toBe(false);

      service.createSavedFilter(request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.isLoading()).toBe(false);
        done();
      });

      expect(service.isLoading()).toBe(true);
      
      const req = httpMock.expectOne('/api/savedfilter');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should set isLoading to false on error', (done) => {
      const request: CreateSavedFilterRequest = {
        name: 'Test',
        entityType: 'Contact',
        isAdvancedSearch: false,
        ascending: true
      };

      service.createSavedFilter(request).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/savedfilter');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('updateSavedFilter', () => {
    it('should update an existing saved filter', (done) => {
      const request: UpdateSavedFilterRequest = {
        id: 1,
        name: 'Updated Filter',
        entityType: 'Partner',
        isAdvancedSearch: false,
        searchCriteria: { status: 'active' },
        orderBy: 'name',
        ascending: true
      };
      const mockResponse: SavedFilter = {
        id: 1,
        name: 'Updated Filter',
        entityType: 'Partner',
        isAdvancedSearch: false,
        searchCriteria: { status: 'active' },
        orderBy: 'name',
        ascending: true,
        usageCount: 5,
        createdDate: new Date('2024-01-01'),
        modifiedDate: new Date('2024-01-15'),
        createdBy: 'user1',
        modifiedBy: 'user2'
      };

      service.updateSavedFilter(request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.isLoading()).toBe(false);
        done();
      });

      const req = httpMock.expectOne('/api/savedfilter');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should set isLoading to false on error', (done) => {
      const request: UpdateSavedFilterRequest = {
        id: 1,
        name: 'Test',
        entityType: 'Contact',
        isAdvancedSearch: false,
        ascending: true
      };

      service.updateSavedFilter(request).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/savedfilter');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('deleteSavedFilter', () => {
    it('should delete a saved filter', (done) => {
      const filterId = 123;

      service.deleteSavedFilter(filterId).subscribe(() => {
        expect(service.isLoading()).toBe(false);
        done();
      });

      const req = httpMock.expectOne('/api/savedfilter/123');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should set isLoading to false on error', (done) => {
      service.deleteSavedFilter(1).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/savedfilter/1');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('getSavedFilter', () => {
    it('should get a specific saved filter by ID', (done) => {
      const mockFilter: SavedFilter = {
        id: 1,
        name: 'Test Filter',
        entityType: 'Contact',
        isAdvancedSearch: false,
        searchCriteria: {},
        orderBy: 'name',
        ascending: true,
        usageCount: 3,
        createdDate: new Date('2024-01-01'),
        modifiedDate: new Date('2024-01-10'),
        createdBy: 'user1',
        modifiedBy: 'user1'
      };

      service.getSavedFilter(1).subscribe(filter => {
        expect(filter).toEqual(mockFilter);
        expect(service.isLoading()).toBe(false);
        done();
      });

      const req = httpMock.expectOne('/api/savedfilter/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockFilter);
    });
  });

  describe('getSavedFilters', () => {
    it('should get saved filters with pagination', (done) => {
      const request: SavedFilterSearchRequest = { pageIndex: 1, pageSize: 10 };
      const mockResponse: SavedFilterSearchResponse = {
        records: [
          {
            id: 1,
            name: 'Filter 1',
            entityType: 'Contact',
            isAdvancedSearch: false,
            ascending: true,
            usageCount: 1,
            createdDate: new Date('2024-01-01'),
            modifiedDate: new Date('2024-01-01'),
            createdBy: 'user1',
            modifiedBy: 'user1'
          },
          {
            id: 2,
            name: 'Filter 2',
            entityType: 'Partner',
            isAdvancedSearch: false,
            ascending: true,
            usageCount: 2,
            createdDate: new Date('2024-01-02'),
            modifiedDate: new Date('2024-01-02'),
            createdBy: 'user2',
            modifiedBy: 'user2'
          }
        ],
        totalCount: 2,
        pageIndex: 1,
        pageSize: 10,
        totalPages: 1
      };

      service.getSavedFilters(request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter');
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('pageIndex')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      req.flush(mockResponse);
    });

    it('should include entity type filter', (done) => {
      const request: SavedFilterSearchRequest = { entityType: 'Contact', pageIndex: 1, pageSize: 10 };

      service.getSavedFilters(request).subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter');
      expect(req.request.params.get('entityType')).toBe('Contact');
      req.flush({ records: [], totalCount: 0, pageIndex: 1, pageSize: 10, totalPages: 0 });
      done();
    });

    it('should include search text filter', (done) => {
      const request = { searchText: 'test', pageIndex: 1, pageSize: 10 };

      service.getSavedFilters(request).subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter');
      expect(req.request.params.get('searchText')).toBe('test');
      req.flush({ records: [], totalCount: 0, pageIndex: 1, pageSize: 10, totalPages: 0 });
      done();
    });

    it('should include orderBy and ascending filters', (done) => {
      const request = { orderBy: 'name', ascending: true, pageIndex: 1, pageSize: 10 };

      service.getSavedFilters(request).subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter');
      expect(req.request.params.get('orderBy')).toBe('name');
      expect(req.request.params.get('ascending')).toBe('true');
      req.flush({ records: [], totalCount: 0, pageIndex: 1, pageSize: 10, totalPages: 0 });
      done();
    });
  });

  describe('applySavedFilter', () => {
    it('should apply a saved filter', (done) => {
      const mockResponse: ApplySavedFilterResponse = {
        filterId: 1,
        name: 'Active Filter',
        entityType: 'Contact',
        isAdvancedSearch: false,
        searchCriteria: { status: 'active' },
        orderBy: 'name',
        ascending: true,
        pageIndex: 1,
        pageSize: 10
      };

      service.applySavedFilter(1, 1, 10).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter/1/apply');
      expect(req.request.method).toBe('GET');
      expect(req.request.params.get('pageIndex')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      req.flush(mockResponse);
    });

    it('should use default pagination values', (done) => {
      const mockResponse: ApplySavedFilterResponse = {
        filterId: 1,
        name: 'Filter',
        entityType: 'Contact',
        isAdvancedSearch: false,
        ascending: true,
        pageIndex: 1,
        pageSize: 10
      };

      service.applySavedFilter(1).subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter/1/apply');
      expect(req.request.params.get('pageIndex')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      req.flush(mockResponse);
      done();
    });
  });

  describe('getFilterStatistics', () => {
    it('should get filter statistics', (done) => {
      const mockStats: FilterStatistics = {
        totalFilters: 10,
        filtersByEntityType: {
          Contact: 4,
          Partner: 3,
          Project: 3
        },
        mostUsedFilters: [
          {
            id: 1,
            name: 'Popular Filter',
            entityType: 'Contact',
            usageCount: 50
          }
        ]
      };

      service.getFilterStatistics().subscribe(stats => {
        expect(stats).toEqual(mockStats);
        done();
      });

      const req = httpMock.expectOne('/api/savedfilter/statistics');
      expect(req.request.method).toBe('GET');
      req.flush(mockStats);
    });

    it('should get statistics for specific entity type', (done) => {
      service.getFilterStatistics('Contact').subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter/statistics');
      expect(req.request.params.get('entityType')).toBe('Contact');
      req.flush({ totalFilters: 5, publicFilters: 2, privateFilters: 3, mostUsed: [] });
      done();
    });
  });

  describe('helper methods', () => {
    it('should get saved filters for entity', (done) => {
      service.getSavedFiltersForEntity('Partner', 1, 50).subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter');
      expect(req.request.params.get('entityType')).toBe('Partner');
      expect(req.request.params.get('pageIndex')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('50');
      req.flush({ records: [], totalCount: 0, pageIndex: 1, pageSize: 50, totalPages: 0 });
      done();
    });

    it('should get most used filters', (done) => {
      service.getMostUsedFilters('Contact').subscribe();

      const req = httpMock.expectOne(req => req.url === '/api/savedfilter/statistics');
      expect(req.request.params.get('entityType')).toBe('Contact');
      req.flush({ totalFilters: 0, publicFilters: 0, privateFilters: 0, mostUsed: [] });
      done();
    });
  });
});

