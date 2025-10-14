import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { routePermissionGuard } from './route-permission.guard';
import { PermissionService } from '../services/auth';
import { of, throwError, Observable } from 'rxjs';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, GuardResult } from '@angular/router';

describe('routePermissionGuard', () => {
  let mockPermissionService: jasmine.SpyObj<PermissionService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    // Create spies
    mockPermissionService = jasmine.createSpyObj('PermissionService', [
      'canAccessRoute',
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: PermissionService, useValue: mockPermissionService },
        { provide: Router, useValue: mockRouter },
      ],
    });

    // Create mock route and state
    mockRoute = {} as ActivatedRouteSnapshot;
    mockState = { url: '/admin/users' } as RouterStateSnapshot;
  });

  it('should allow access when user has permission', (done) => {
    mockPermissionService.canAccessRoute.and.returnValue(of(true));

    const result = routePermissionGuard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeTrue();
      expect(mockPermissionService.canAccessRoute).toHaveBeenCalledWith(
        '/admin/users'
      );
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  it('should deny access and redirect when user lacks permission', (done) => {
    mockPermissionService.canAccessRoute.and.returnValue(of(false));

    const result = routePermissionGuard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeFalse();
      expect(mockPermissionService.canAccessRoute).toHaveBeenCalledWith(
        '/admin/users'
      );
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      done();
    });
  });

  it('should handle errors and redirect to access-denied', (done) => {
    mockPermissionService.canAccessRoute.and.returnValue(
      throwError(() => new Error('Permission check failed'))
    );

    const result = routePermissionGuard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeFalse();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      done();
    });
  });
});

