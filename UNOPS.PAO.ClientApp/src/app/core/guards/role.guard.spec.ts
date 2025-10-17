import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { of, throwError, isObservable, firstValueFrom } from 'rxjs';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth/auth.service';

describe('roleGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockHttpClient: jasmine.SpyObj<HttpClient>;

  beforeEach(() => {
    // Create mocks
    mockAuthService = jasmine.createSpyObj('AuthService', ['hasDevCookie', 'getUserRoles']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockHttpClient = jasmine.createSpyObj('HttpClient', ['get']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
        { provide: HttpClient, useValue: mockHttpClient }
      ]
    });
  });

  it('should allow access when no roles are specified', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };
    const guardFn = roleGuard([]);

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      expect(result).toBe(true);
    });
  });

  it('should allow access when "ALL" role is specified', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };
    const guardFn = roleGuard(['ALL']);

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      expect(result).toBe(true);
    });
  });

  it('should allow access when backend confirms access', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };
    const guardFn = roleGuard(['Administrator']);

    mockHttpClient.get.and.returnValue(of({ 
      route: '/dashboard', 
      hasAccess: true 
    }));

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree;
      }
      
      expect(finalResult).toBe(true);
    });
  });

  it('should deny access and redirect when backend denies access', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };
    const guardFn = roleGuard(['Administrator']);

    mockHttpClient.get.and.returnValue(of({ 
      route: '/dashboard', 
      hasAccess: false 
    }));

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree;
      }
      
      expect(finalResult).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });
  });

  it('should fallback to local role checking when backend fails', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };
    const guardFn = roleGuard(['Administrator']);

    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend unavailable')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['Administrator']));

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree;
      }
      
      expect(finalResult).toBe(true);
    });
  });

  it('should allow Administrator access to any route', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/admin' };
    const guardFn = roleGuard(['SuperUser']);

    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend unavailable')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['Administrator']));

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree;
      }
      
      expect(finalResult).toBe(true);
    });
  });

  it('should deny access when user lacks required role', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/admin' };
    const guardFn = roleGuard(['Administrator']);

    mockHttpClient.get.and.returnValue(throwError(() => new Error('Backend unavailable')));
    mockAuthService.hasDevCookie.and.returnValue(false);
    mockAuthService.getUserRoles.and.returnValue(of(['User']));

    await TestBed.runInInjectionContext(async () => {
      const result = guardFn(mockRoute, mockState);
      
      let finalResult: boolean | UrlTree;
      if (typeof result === 'boolean') {
        finalResult = result;
      } else if (result instanceof UrlTree) {
        finalResult = result;
      } else if (isObservable(result)) {
        finalResult = await firstValueFrom(result) as boolean | UrlTree;
      } else {
        finalResult = await result as boolean | UrlTree;
      }
      
      expect(finalResult).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });
  });
});

