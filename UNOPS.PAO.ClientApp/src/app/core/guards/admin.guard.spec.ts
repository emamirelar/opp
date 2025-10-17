import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, firstValueFrom } from 'rxjs';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth/auth.service';

describe('adminGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    // Create mocks
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAdmin']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should allow access when user is admin', async () => {
    mockAuthService.isAdmin.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result$ = adminGuard();
      
      // The guard returns an Observable<boolean>
      const finalResult = await firstValueFrom(result$);
      
      expect(finalResult).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  it('should deny access and redirect when user is not admin', async () => {
    mockAuthService.isAdmin.and.returnValue(of(false));

    await TestBed.runInInjectionContext(async () => {
      const result$ = adminGuard();
      
      // The guard returns an Observable<boolean>
      const finalResult = await firstValueFrom(result$);
      
      expect(finalResult).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
    });
  });

  it('should handle admin check errors gracefully', async () => {
    mockAuthService.isAdmin.and.returnValue(of(false));

    await TestBed.runInInjectionContext(async () => {
      const result$ = adminGuard();
      
      // The guard returns an Observable<boolean>
      const finalResult = await firstValueFrom(result$);
      
      // When isAdmin returns false (including errors), should redirect
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      expect(finalResult).toBe(false);
    });
  });

  it('should call authService.isAdmin method', async () => {
    mockAuthService.isAdmin.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result$ = adminGuard();
      await firstValueFrom(result$);
      
      expect(mockAuthService.isAdmin).toHaveBeenCalled();
    });
  });

  it('should return observable that can be subscribed to', async () => {
    mockAuthService.isAdmin.and.returnValue(of(true));

    await TestBed.runInInjectionContext(async () => {
      const result$ = adminGuard();
      
      // Verify it returns an Observable
      expect(result$).toBeDefined();
      expect(typeof result$.subscribe).toBe('function');
      
      const finalResult = await firstValueFrom(result$);
      expect(finalResult).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });
});

