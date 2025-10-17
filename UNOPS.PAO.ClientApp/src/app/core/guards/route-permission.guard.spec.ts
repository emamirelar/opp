import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { of, throwError, isObservable, firstValueFrom } from 'rxjs';
import { routePermissionGuard } from './route-permission.guard';
import { PermissionService } from '../services/auth/permission.service';

describe('routePermissionGuard', () => {
  let mockPermissionService: jasmine.SpyObj<PermissionService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    // Create mocks
    mockPermissionService = jasmine.createSpyObj('PermissionService', ['canAccessRoute']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: PermissionService, useValue: mockPermissionService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should allow access when permission service grants access', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };

    mockPermissionService.canAccessRoute.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result = routePermissionGuard(mockRoute, mockState);
      
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
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  it('should deny access and redirect to access-denied when permission is denied', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/admin' };

    mockPermissionService.canAccessRoute.and.returnValue(of(false));

    await TestBed.runInInjectionContext(async () => {
      const result = routePermissionGuard(mockRoute, mockState);
      
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
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
    });
  });

  it('should redirect to access-denied on service error', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/dashboard' };

    mockPermissionService.canAccessRoute.and.returnValue(
      throwError(() => new Error('Service error'))
    );

    await TestBed.runInInjectionContext(async () => {
      const result = routePermissionGuard(mockRoute, mockState);
      
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
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
    });
  });

  it('should check correct route URL', async () => {
    const mockRoute: any = {};
    const mockState: any = { url: '/partnerships/contacts' };

    mockPermissionService.canAccessRoute.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      routePermissionGuard(mockRoute, mockState);
      
      expect(mockPermissionService.canAccessRoute).toHaveBeenCalledWith('/partnerships/contacts');
    });
  });
});

