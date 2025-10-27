import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, UserClaim, UserInfo } from './auth.service';
import { SocialUser } from '@abacritt/angularx-social-login';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    // Clear cookies before each test
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
    
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });

    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(AuthService);
    
    // Clear any pending requests from constructor
    const constructorRequests = httpMock.match(() => true);
    constructorRequests.forEach(req => {
      if (!req.cancelled) {
        req.flush([]);
      }
    });
  });

  afterEach(() => {
    // Handle any remaining requests
    const remaining = httpMock.match(() => true);
    remaining.forEach(req => {
      if (!req.cancelled) {
        req.flush({});
      }
    });
    
    httpMock.verify();
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should check if dev cookie exists', () => {
    expect(service.hasDevCookie()).toBe(false);
    
    document.cookie = 'dev-user-email=test@unops.org';
    expect(service.hasDevCookie()).toBe(true);
    
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
  });

  it('should sign up a user', (done) => {
    const email = 'newuser@example.com';
    const password = 'password123';
    const mockResponse = { success: true };

    service.signUp(email, password).subscribe(response => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/user/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email, password });
    req.flush(mockResponse);
  });

  it('should perform Google sign in', (done) => {
    const mockUser: SocialUser = {
      provider: 'GOOGLE',
      idToken: 'mock-id-token',
      id: '123',
      email: 'test@example.com',
      name: 'Test User',
      photoUrl: '',
      firstName: 'Test',
      lastName: 'User',
      authToken: '',
      authorizationCode: '',
      response: {}
    };

    const mockResponse = { success: true };

    service.googleSignIn(mockUser).subscribe(response => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/user/googleSignIn');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.provider).toBe('GOOGLE');
    expect(req.request.body.idToken).toBe('mock-id-token');
    req.flush(mockResponse);
  });

  it('should log in a user', (done) => {
    const userName = 'user@example.com';
    const password = 'password123';
    const mockResponse = { success: true };

    service.logIn(userName, password).subscribe(response => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/user/login?useCookies=true');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: userName, password });
    req.flush(mockResponse);
  });

  it('should get user claims from API', (done) => {
    const mockClaims: UserClaim[] = [
      { type: 'name', value: 'Test User' },
      { type: 'email', value: 'test@unops.org' },
      { type: 'role', value: 'Internal' }
    ];

    service.user().subscribe(claims => {
      expect(claims.length).toBe(3);
      expect(claims[0].type).toBe('name');
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    expect(req.request.method).toBe('GET');
    req.flush(mockClaims);
  });

  it('should return empty array when user claims API fails', (done) => {
    service.user().subscribe(claims => {
      expect(claims).toEqual([]);
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.error(new ProgressEvent('error'), { status: 401, statusText: 'Unauthorized' });
  });

  it('should check if user is logged in', (done) => {
    const mockClaims: UserClaim[] = [
      { type: 'email', value: 'test@unops.org' }
    ];

    service.isLogedIn().subscribe(isLoggedIn => {
      expect(isLoggedIn).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.flush(mockClaims);
  });

  it('should return false when user is not logged in', (done) => {
    service.isLogedIn().subscribe(isLoggedIn => {
      expect(isLoggedIn).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.flush([]);
  });

  it('should check if user is IAP authenticated with dev cookie', (done) => {
    // Use dev cookie approach which bypasses hostname check
    document.cookie = 'dev-user-email=test@unops.org';

    service.isIapAuthenticated().subscribe(isAuthenticated => {
      expect(isAuthenticated).toBe(true);
      document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
      done();
    });

    // No HTTP request should be made with dev cookie
  });

  it('should return user roles from claims', (done) => {
    const mockClaims: UserClaim[] = [
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'administrator' },
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'internal' }
    ];

    service.getUserRoles().subscribe(roles => {
      expect(roles.length).toBe(2);
      expect(roles).toContain('ADMINISTRATOR');
      expect(roles).toContain('INTERNAL');
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.flush(mockClaims);
  });

  it('should check if user has specific role using dev cookie', (done) => {
    // Use dev cookie which has direct role checking
    document.cookie = 'dev-user-email=user@unops.org';

    service.hasRole('Internal').subscribe(hasRole => {
      expect(hasRole).toBe(true); // @unops.org email gets Internal role
      document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
      done();
    });
  });

  it('should check if user is admin', (done) => {
    const mockClaims: UserClaim[] = [
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'PARTNER_GLOB_ADMIN' }
    ];

    service.isAdmin().subscribe(isAdmin => {
      expect(isAdmin).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.flush(mockClaims);
  });

  it('should check if user is global admin', (done) => {
    const mockClaims: UserClaim[] = [
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'PARTNER_GLOB_ADMIN' }
    ];

    service.isGlobalAdmin().subscribe(isGlobalAdmin => {
      expect(isGlobalAdmin).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/user/claims');
    req.flush(mockClaims);
  });

  it('should get auth info with dev cookie', (done) => {
    document.cookie = 'dev-user-email=admin@unops.org';

    service.getAuthInfo().subscribe(authInfo => {
      expect(authInfo.email).toBe('admin@unops.org');
      expect(authInfo.iapAuthenticated).toBe(true);
      expect(authInfo.devMode).toBe(true);
      expect(authInfo.roles).toContain('Internal');
      expect(authInfo.roles).toContain('Administrator');
      done();
    });

    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
  });

  it('should reset authentication state', () => {
    service.resetAuthenticationState();
    // This method resets internal state, no assertions needed beyond no errors
    expect(service).toBeTruthy();
  });

  it('should return false when IAP not authenticated', (done) => {
    // Reset auth state to allow new check
    service.resetAuthenticationState();

    service.isIapAuthenticated().subscribe(isAuthenticated => {
      // Without dev cookie or IAP headers, should return false
      expect(isAuthenticated).toBe(false);
      done();
    });

    // Check if there's a request and error it
    const requests = httpMock.match(() => true);
    if (requests.length > 0) {
      requests[0].error(new ProgressEvent('error'));
    }
  });

  it('should parse user info from dev cookie', (done) => {
    // Set dev cookie with admin email
    document.cookie = 'dev-user-email=admin@unops.org';

    // Get auth info which will parse the dev cookie
    service.getAuthInfo().subscribe(authInfo => {
      expect(authInfo.email).toBe('admin@unops.org');
      expect(authInfo.isInternal).toBe(true);
      expect(authInfo.iapAuthenticated).toBe(true);
      expect(authInfo.devMode).toBe(true);
      expect(authInfo.roles).toContain('Internal');
      expect(authInfo.roles).toContain('Administrator');
      
      document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
      done();
    });
  });
});

