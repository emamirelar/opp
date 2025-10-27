import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { PaginationUrlService } from './pagination-url.service';
import { of } from 'rxjs';

describe('PaginationUrlService', () => {
  let service: PaginationUrlService;
  let mockActivatedRoute: any;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockActivatedRoute = {
      queryParams: of({ pageIndex: '1', pageSize: '10' })
    };

    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'getCurrentNavigation']);
    mockRouter.getCurrentNavigation.and.returnValue(null);

    TestBed.configureTestingModule({
      providers: [
        PaginationUrlService,
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter }
      ]
    });

    service = TestBed.inject(PaginationUrlService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getCurrentPaginationParams', () => {
    it('should return pagination params from URL', (done) => {
      mockActivatedRoute.queryParams = of({ 
        pageIndex: '2', 
        pageSize: '20',
        orderBy: 'name',
        ascending: 'true'
      });

      // Create new service instance with updated route
      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(2);
        expect(params.pageSize).toBe(20);
        expect(params.orderBy).toBe('name');
        expect(params.ascending).toBe('true');
        done();
      });
    });

    it('should use default pageIndex of 1 when not provided', (done) => {
      mockActivatedRoute.queryParams = of({ pageSize: '10' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(1);
        done();
      });
    });

    it('should use default pageSize of 10 when not provided', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: '1' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageSize).toBe(10);
        done();
      });
    });

    it('should handle invalid pageIndex as NaN and return default 1', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: 'invalid', pageSize: '10' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(1);
        done();
      });
    });

    it('should handle invalid pageSize as NaN and return default 10', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: '1', pageSize: 'invalid' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageSize).toBe(10);
        done();
      });
    });

    it('should handle empty query params', (done) => {
      mockActivatedRoute.queryParams = of({});

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(1);
        expect(params.pageSize).toBe(10);
        expect(params.orderBy).toBeUndefined();
        expect(params.ascending).toBeUndefined();
        done();
      });
    });

    it('should handle negative page numbers', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: '-5', pageSize: '10' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(-5); // Service doesn't validate, just converts
        done();
      });
    });

    it('should handle zero page numbers', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: '0', pageSize: '0' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(0);
        expect(params.pageSize).toBe(0);
        done();
      });
    });

    it('should handle floating point page numbers', (done) => {
      mockActivatedRoute.queryParams = of({ pageIndex: '2.5', pageSize: '10.8' });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.pageIndex).toBe(2.5);
        expect(params.pageSize).toBe(10.8);
        done();
      });
    });

    it('should handle ascending as different values', (done) => {
      mockActivatedRoute.queryParams = of({ 
        pageIndex: '1', 
        pageSize: '10',
        ascending: false 
      });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.ascending).toBe('false');
        done();
      });
    });
  });

  describe('updatePaginationParams', () => {
    it('should update pagination params in URL', () => {
      service.updatePaginationParams({ pageIndex: 3, pageSize: 50 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          relativeTo: mockActivatedRoute,
          queryParams: { pageIndex: 3, pageSize: 50 },
          queryParamsHandling: 'merge'
        })
      );
    });

    it('should update only pageIndex', () => {
      service.updatePaginationParams({ pageIndex: 5 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { pageIndex: 5 }
        })
      );
    });

    it('should update only pageSize', () => {
      service.updatePaginationParams({ pageSize: 100 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { pageSize: 100 }
        })
      );
    });

    it('should update orderBy', () => {
      service.updatePaginationParams({ orderBy: 'createdDate' });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { orderBy: 'createdDate' }
        })
      );
    });

    it('should update ascending', () => {
      service.updatePaginationParams({ ascending: 'false' });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { ascending: 'false' }
        })
      );
    });

    it('should update multiple params at once', () => {
      service.updatePaginationParams({ 
        pageIndex: 2, 
        pageSize: 25,
        orderBy: 'name',
        ascending: 'true'
      });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { 
            pageIndex: 2, 
            pageSize: 25,
            orderBy: 'name',
            ascending: 'true'
          }
        })
      );
    });

    it('should merge with existing params when navigation exists', () => {
      const mockNavigation = {
        extractedUrl: {
          queryParams: { existingParam: 'value' }
        }
      };
      mockRouter.getCurrentNavigation.and.returnValue(mockNavigation as any);

      service.updatePaginationParams({ pageIndex: 2 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { existingParam: 'value', pageIndex: 2 }
        })
      );
    });

    it('should handle empty updates', () => {
      service.updatePaginationParams({});

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: {}
        })
      );
    });

    it('should use merge query params handling', () => {
      service.updatePaginationParams({ pageIndex: 1 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParamsHandling: 'merge'
        })
      );
    });

    it('should navigate relative to current route', () => {
      service.updatePaginationParams({ pageIndex: 1 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          relativeTo: mockActivatedRoute
        })
      );
    });
  });

  describe('filter and sort parameter handling', () => {
    it('should handle sort ascending', (done) => {
      mockActivatedRoute.queryParams = of({ 
        pageIndex: '1',
        pageSize: '10',
        orderBy: 'name',
        ascending: 'true'
      });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.orderBy).toBe('name');
        expect(params.ascending).toBe('true');
        done();
      });
    });

    it('should handle sort descending', (done) => {
      mockActivatedRoute.queryParams = of({ 
        pageIndex: '1',
        pageSize: '10',
        orderBy: 'createdDate',
        ascending: 'false'
      });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.orderBy).toBe('createdDate');
        expect(params.ascending).toBe('false');
        done();
      });
    });

    it('should handle orderBy without ascending', (done) => {
      mockActivatedRoute.queryParams = of({ 
        pageIndex: '1',
        pageSize: '10',
        orderBy: 'status'
      });

      const newService = TestBed.inject(PaginationUrlService);

      newService.getCurrentPaginationParams().subscribe(params => {
        expect(params.orderBy).toBe('status');
        expect(params.ascending).toBeUndefined();
        done();
      });
    });

    it('should update sort parameters', () => {
      service.updatePaginationParams({ 
        orderBy: 'updatedDate',
        ascending: 'false'
      });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { 
            orderBy: 'updatedDate',
            ascending: 'false'
          }
        })
      );
    });

    it('should reset to first page when changing sort order', () => {
      service.updatePaginationParams({ 
        pageIndex: 1,
        orderBy: 'name',
        ascending: 'true'
      });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { 
            pageIndex: 1,
            orderBy: 'name',
            ascending: 'true'
          }
        })
      );
    });
  });

  describe('pagination state management', () => {
    it('should handle pagination forward', () => {
      service.updatePaginationParams({ pageIndex: 2 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { pageIndex: 2 }
        })
      );
    });

    it('should handle pagination backward', () => {
      service.updatePaginationParams({ pageIndex: 1 });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { pageIndex: 1 }
        })
      );
    });

    it('should handle page size changes', () => {
      service.updatePaginationParams({ 
        pageIndex: 1, // Reset to first page when changing size
        pageSize: 50 
      });

      expect(mockRouter.navigate).toHaveBeenCalledWith(
        [],
        jasmine.objectContaining({
          queryParams: { 
            pageIndex: 1,
            pageSize: 50 
          }
        })
      );
    });

    it('should handle sequential page changes', () => {
      service.updatePaginationParams({ pageIndex: 1 });
      service.updatePaginationParams({ pageIndex: 2 });
      service.updatePaginationParams({ pageIndex: 3 });

      expect(mockRouter.navigate).toHaveBeenCalledTimes(3);
    });
  });
});

