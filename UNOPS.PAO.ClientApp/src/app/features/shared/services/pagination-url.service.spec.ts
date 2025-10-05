import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { of, BehaviorSubject } from 'rxjs';
import { PaginationUrlService } from './pagination-url.service';
import { PaginationParams } from '@shared/models/pagination-params.model';

describe('PaginationUrlService', () => {
  let service: PaginationUrlService;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: jasmine.SpyObj<ActivatedRoute>;
  let queryParamsSubject: BehaviorSubject<any>;

  beforeEach(() => {
    queryParamsSubject = new BehaviorSubject({});
    
    const routerSpy = jasmine.createSpyObj('Router', ['navigate'], {
      getCurrentNavigation: jasmine.createSpy().and.returnValue({
        extractedUrl: { queryParams: {} }
      })
    });
    
    const activatedRouteSpy = jasmine.createSpyObj('ActivatedRoute', [], {
      queryParams: queryParamsSubject.asObservable()
    });

    TestBed.configureTestingModule({
      providers: [
        PaginationUrlService,
        { provide: Router, useValue: routerSpy },
        { provide: ActivatedRoute, useValue: activatedRouteSpy }
      ]
    });

    service = TestBed.inject(PaginationUrlService);
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockActivatedRoute = TestBed.inject(ActivatedRoute) as jasmine.SpyObj<ActivatedRoute>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCurrentPaginationParams', () => {
    it('should return default values for empty query params', (done) => {
      queryParamsSubject.next({});
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params).toEqual({
          pageIndex: 1,
          pageSize: 10,
          orderBy: undefined,
          ascending: undefined
        });
        done();
      });
    });

    it('should parse numeric values correctly', (done) => {
      queryParamsSubject.next({
        pageIndex: '3',
        pageSize: '25'
      });
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(3);
        expect(params.pageSize).toBe(25);
        done();
      });
    });

    it('should handle string parameters', (done) => {
      queryParamsSubject.next({
        pageIndex: '2',
        pageSize: '15',
        orderBy: 'name',
        ascending: 'true'
      });
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params).toEqual({
          pageIndex: 2,
          pageSize: 15,
          orderBy: 'name',
          ascending: 'true'
        });
        done();
      });
    });

    it('should handle invalid numeric values with defaults', (done) => {
      queryParamsSubject.next({
        pageIndex: 'invalid',
        pageSize: 'also-invalid'
      });
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(1); // Default for NaN
        expect(params.pageSize).toBe(10); // Default for NaN
        done();
      });
    });

    it('should handle zero and negative values', (done) => {
      queryParamsSubject.next({
        pageIndex: '0',
        pageSize: '-5'
      });
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(0);
        expect(params.pageSize).toBe(-5);
        done();
      });
    });

    it('should preserve all parameters when present', (done) => {
      queryParamsSubject.next({
        pageIndex: '5',
        pageSize: '50',
        orderBy: 'email',
        ascending: 'false',
        extraParam: 'should-be-ignored'
      });
      
      service.getCurrentPaginationParams().subscribe(params => {
        expect(params).toEqual({
          pageIndex: 5,
          pageSize: 50,
          orderBy: 'email',
          ascending: 'false'
        });
        done();
      });
    });
  });

  describe('updatePaginationParams', () => {
    beforeEach(() => {
      // Mock getCurrentNavigation to return current query params
      mockRouter.getCurrentNavigation.and.returnValue({
        extractedUrl: {
          queryParams: {
            pageIndex: '1',
            pageSize: '10',
            existingParam: 'value'
          }
        }
      } as any);
    });

    it('should merge new parameters with existing ones', () => {
      const updates: Partial<PaginationParams> = {
        pageIndex: 2,
        orderBy: 'name'
      };
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: 2,
          pageSize: '10',
          existingParam: 'value',
          orderBy: 'name'
        },
        queryParamsHandling: 'merge'
      });
    });

    it('should override existing parameters', () => {
      const updates: Partial<PaginationParams> = {
        pageIndex: 3,
        pageSize: 25
      };
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: 3,
          pageSize: 25,
          existingParam: 'value'
        },
        queryParamsHandling: 'merge'
      });
    });

    it('should handle single parameter update', () => {
      const updates: Partial<PaginationParams> = {
        ascending: 'true'
      };
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: '1',
          pageSize: '10',
          existingParam: 'value',
          ascending: 'true'
        },
        queryParamsHandling: 'merge'
      });
    });

    it('should handle empty updates', () => {
      const updates: Partial<PaginationParams> = {};
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: '1',
          pageSize: '10',
          existingParam: 'value'
        },
        queryParamsHandling: 'merge'
      });
    });

    it('should handle null getCurrentNavigation', () => {
      mockRouter.getCurrentNavigation.and.returnValue(null);
      
      const updates: Partial<PaginationParams> = {
        pageIndex: 2
      };
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: 2
        },
        queryParamsHandling: 'merge'
      });
    });

    it('should handle undefined extractedUrl', () => {
      mockRouter.getCurrentNavigation.and.returnValue({
        extractedUrl: undefined
      } as any);
      
      const updates: Partial<PaginationParams> = {
        pageIndex: 2
      };
      
      service.updatePaginationParams(updates);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith([], {
        relativeTo: mockActivatedRoute,
        queryParams: {
          pageIndex: 2
        },
        queryParamsHandling: 'merge'
      });
    });
  });

  describe('integration scenarios', () => {
    it('should work with complete pagination workflow', (done) => {
      // Initial state
      queryParamsSubject.next({
        pageIndex: '1',
        pageSize: '10'
      });
      
      service.getCurrentPaginationParams().subscribe(initialParams => {
        expect(initialParams.pageIndex).toBe(1);
        expect(initialParams.pageSize).toBe(10);
        
        // Update pagination
        service.updatePaginationParams({
          pageIndex: 2,
          orderBy: 'name',
          ascending: 'true'
        });
        
        expect(mockRouter.navigate).toHaveBeenCalled();
        done();
      });
    });

    it('should handle rapid parameter updates', () => {
      const updates1: Partial<PaginationParams> = { pageIndex: 2 };
      const updates2: Partial<PaginationParams> = { pageSize: 25 };
      const updates3: Partial<PaginationParams> = { orderBy: 'email' };
      
      service.updatePaginationParams(updates1);
      service.updatePaginationParams(updates2);
      service.updatePaginationParams(updates3);
      
      expect(mockRouter.navigate).toHaveBeenCalledTimes(3);
    });

    it('should preserve non-pagination parameters', () => {
      mockRouter.getCurrentNavigation.and.returnValue({
        extractedUrl: {
          queryParams: {
            pageIndex: '1',
            searchTerm: 'test',
            filter: 'active',
            customParam: 'preserve-me'
          }
        }
      } as any);
      
      service.updatePaginationParams({ pageIndex: 3 });
      
      const lastCall = mockRouter.navigate.calls.mostRecent();
      const calledParams = lastCall?.args[1]?.queryParams;
      
      expect(calledParams?.['searchTerm']).toBe('test');
      expect(calledParams?.['filter']).toBe('active');
      expect(calledParams?.['customParam']).toBe('preserve-me');
      expect(calledParams?.['pageIndex']).toBe(3);
    });
  });
});