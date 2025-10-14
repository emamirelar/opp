import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth';
import { of, Observable } from 'rxjs';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, GuardResult } from '@angular/router';

describe('authGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    // Create spies
    mockAuthService = jasmine.createSpyObj('AuthService', [
      'isIapAuthenticated',
      'isLogedIn',
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    });

    // Create mock route and state
    mockRoute = {} as ActivatedRouteSnapshot;
    mockState = { url: '/dashboard' } as RouterStateSnapshot;

    // Clear document cookies before each test
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });

  it('should allow access to login page without authentication', () => {
    mockState.url = '/login';

    const result = authGuard(mockRoute, mockState);

    expect(result).toBeTrue();
    expect(mockAuthService.isIapAuthenticated).not.toHaveBeenCalled();
  });

  it('should allow access to dev-login page without authentication', () => {
    mockState.url = '/dev-login';

    const result = authGuard(mockRoute, mockState);

    expect(result).toBeTrue();
    expect(mockAuthService.isIapAuthenticated).not.toHaveBeenCalled();
  });

  it('should allow access when dev cookie is present', () => {
    // Set dev cookie
    document.cookie = 'dev-user-email=test@example.com';

    const result = authGuard(mockRoute, mockState);

    expect(result).toBeTrue();
    expect(mockAuthService.isIapAuthenticated).not.toHaveBeenCalled();
  });

  it('should allow access when IAP authenticated', (done) => {
    mockAuthService.isIapAuthenticated.and.returnValue(of(true));

    const result = authGuard(mockRoute, mockState);

    if (result instanceof UrlTree || typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeTrue();
      expect(mockAuthService.isIapAuthenticated).toHaveBeenCalled();
      expect(mockAuthService.isLogedIn).not.toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  it('should allow access when not IAP authenticated but logged in', (done) => {
    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(true));

    const result = authGuard(mockRoute, mockState);

    if (result instanceof UrlTree || typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeTrue();
      expect(mockAuthService.isIapAuthenticated).toHaveBeenCalled();
      expect(mockAuthService.isLogedIn).toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  it('should deny access and redirect to login when not authenticated', (done) => {
    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(false));

    const result = authGuard(mockRoute, mockState);

    if (result instanceof UrlTree || typeof result === 'boolean') {
      fail('Expected Observable');
      return;
    }

    (result as Observable<boolean | UrlTree>).subscribe((canActivate: boolean | UrlTree) => {
      expect(canActivate).toBeFalse();
      expect(mockAuthService.isIapAuthenticated).toHaveBeenCalled();
      expect(mockAuthService.isLogedIn).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
      done();
    });
  });

  it('should implement anti-loop protection for rapid calls', () => {
    mockState.url = '/dashboard';
    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(false));

    // Call guard multiple times rapidly
    for (let i = 0; i < 5; i++) {
      authGuard(mockRoute, mockState);
    }

    // After 4 rapid calls, the 5th should return true (anti-loop protection)
    const fifthCall = authGuard(mockRoute, mockState);
    expect(fifthCall).toBeTrue();
  });

  afterEach(() => {
    // Clean up cookies
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });
});

