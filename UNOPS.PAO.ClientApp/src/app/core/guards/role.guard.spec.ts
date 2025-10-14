import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';
import { of, throwError } from 'rxjs';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('roleGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockHttpClient: jasmine.SpyObj<HttpClient>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    // Create spies
    mockAuthService = jasmine.createSpyObj('AuthService', [
      'hasDevCookie',
      'getUserRoles',
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockHttpClient = jasmine.createSpyObj('HttpClient', ['get']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
        { provide: HttpClient, useValue: mockHttpClient },
      ],
    });

    // Create mock route and state
    mockRoute = {} as ActivatedRouteSnapshot;
    mockState = { url: '/admin/users' } as RouterStateSnapshot;

    // Clear cookies
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });

  it('should allow access when no roles specified', () => {
    const guard = roleGuard([]);
    const result = guard(mockRoute, mockState);

    expect(result).toBeTrue();
  });

  it('should allow access when "ALL" role is specified', () => {
    const guard = roleGuard(['ALL']);
    const result = guard(mockRoute, mockState);

    expect(result).toBeTrue();
  });

  it('should allow access when backend returns hasAccess true', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(
      of({ route: '/admin/users', hasAccess: true })
    );

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeTrue();
      expect(mockHttpClient.get).toHaveBeenCalledWith(
        '/api/permissions/check//admin/users'
      );
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  it('should deny access and redirect when backend returns hasAccess false', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(
      of({ route: '/admin/users', hasAccess: false })
    );

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeFalse();
      expect(mockHttpClient.get).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
      done();
    });
  });

  it('should fallback to dev cookie check when backend fails', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend error')));
    mockAuthService.hasDevCookie.and.returnValue(true);

    // Set admin dev cookie
    document.cookie = 'dev-user-email=admin@unops.org';

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeTrue();
      expect(mockAuthService.hasDevCookie).toHaveBeenCalled();
      done();
    });
  });

  it('should check user roles when backend fails and no dev cookie', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend error')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['Administrator']));

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeTrue();
      expect(mockAuthService.getUserRoles).toHaveBeenCalled();
      done();
    });
  });

  it('should deny access when user does not have required role', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend error')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['Partner']));

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeFalse();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
      done();
    });
  });

  it('should allow admin access to everything', (done) => {
    const guard = roleGuard(['Partner']);
    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend error')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['Administrator']));

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeTrue();
      done();
    });
  });

  it('should handle errors in getUserRoles gracefully', (done) => {
    const guard = roleGuard(['Administrator']);
    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend error')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(
      throwError(() => new Error('Role check error'))
    );

    const result = guard(mockRoute, mockState);

    if (typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    result.subscribe((canActivate) => {
      expect(canActivate).toBeFalse();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
      done();
    });
  });

  afterEach(() => {
    // Clean up cookies
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });
});

