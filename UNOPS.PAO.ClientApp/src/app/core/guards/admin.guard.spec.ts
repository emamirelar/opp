import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from '../services/auth.service';
import { of } from 'rxjs';

describe('adminGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    // Create spies
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAdmin']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    // Configure TestBed
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    });
  });

  it('should allow access when user is admin', (done) => {
    mockAuthService.isAdmin.and.returnValue(of(true));

    const result = adminGuard();

    result.subscribe((canActivate) => {
      expect(canActivate).toBeTrue();
      expect(mockAuthService.isAdmin).toHaveBeenCalled();
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      done();
    });
  });

  it('should deny access and redirect when user is not admin', (done) => {
    mockAuthService.isAdmin.and.returnValue(of(false));

    const result = adminGuard();

    result.subscribe((canActivate) => {
      expect(canActivate).toBeFalse();
      expect(mockAuthService.isAdmin).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      done();
    });
  });
});

