import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { of, isObservable, firstValueFrom } from 'rxjs';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth/auth.service';

describe('authGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    // Create mocks
    mockAuthService = jasmine.createSpyObj('AuthService', ['isIapAuthenticated', 'isLogedIn']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should allow access to login page', (done) => {
    const mockRoute: any = {};
    const mockState: any = { url: '/login' };

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockState);
      
      // For login page, guard returns true immediately
      expect(result).toBe(true);
      done();
    });
  });

  it('should allow access when user is IAP authenticated', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };

    mockAuthService.isIapAuthenticated.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result = authGuard(mockRoute, mockState);
      
      // Handle different return types
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree; // Promise
      }
      
      expect(finalResult).toBe(true);
    });
  });

  it('should allow access when user is logged in', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };

    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result = authGuard(mockRoute, mockState);
      
      // Handle different return types
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree; // Promise
      }
      
      expect(finalResult).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  it('should redirect to login when user is not authenticated', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };

    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(false));

    await TestBed.runInInjectionContext(async () => {
      const result = authGuard(mockRoute, mockState);
      
      // Handle different return types
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree; // Promise
      }
      
      expect(finalResult).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
    });
  });

  it('should allow access to dev-login page', (done) => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dev-login' };

    TestBed.runInInjectionContext(() => {
      const result = authGuard(mockRoute, mockState);
      
      expect(result).toBe(true);
      done();
    });
  });
});

