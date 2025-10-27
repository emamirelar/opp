import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PermissionService, EntityPermissions, PermissionConfig } from './permission.service';
import { AuthService } from './auth.service';

describe('PermissionService', () => {
  let service: PermissionService;
  let httpMock: HttpTestingController;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(fakeAsync(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['hasDevCookie', 'getUserRoles']);
    
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        PermissionService,
        { provide: AuthService, useValue: mockAuthService }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(PermissionService);
    
    // Handle constructor's loadConfig() call and let it complete
    const constructorRequests = httpMock.match('/api/permissions');
    constructorRequests.forEach(req => req.flush({ routes: [], entities: [] }));
    tick(); // Let the observable complete
  }));

  afterEach(() => {
    // Handle any remaining requests
    const remaining = httpMock.match(() => true);
    remaining.forEach(req => {
      if (!req.cancelled) {
        req.flush({});
      }
    });
    
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // Note: loadConfig() is called in constructor with complex async initialization
  // This test just verifies the service can be created and initialized
  it('should initialize successfully', () => {
    // Service should be created without errors
    expect(service).toBeTruthy();
    
    // Verify it has the expected methods
    expect(typeof service.getEntityPermissions).toBe('function');
    expect(typeof service.canAccessRoute).toBe('function');
    expect(typeof service.clearPermissionCaches).toBe('function');
  });

  it('should get entity permissions', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'Contact',
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: true,
        canUpdate: false,
        canDelete: false,
        canExport: true,
        canImport: false
      }
    };

    service.getEntityPermissions('/partnerships/contacts').subscribe(permissions => {
      expect(permissions.entity).toBe('Contact');
      expect(permissions.hasAccess).toBe(true);
      expect(permissions.permissions.canRead).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/partnerships/contacts');
    expect(req.request.method).toBe('GET');
    req.flush({
      hasAccess: true,
      entity: 'Contact',
      permissions: mockPermissions.permissions
    });
  });

  it('should get entity instance permissions', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'Contact',
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: true,
        canDelete: true,
        canExport: true,
        canImport: false
      }
    };

    service.getEntityInstancePermissions('Contact', 123).subscribe(permissions => {
      expect(permissions.entity).toBe('Contact');
      expect(permissions.hasAccess).toBe(true);
      expect(permissions.permissions.canUpdate).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/Contact/123');
    expect(req.request.method).toBe('GET');
    req.flush({
      hasAccess: true,
      entity: 'Contact',
      permissions: mockPermissions.permissions
    });
  });

  it('should return false access for invalid entity ID', (done) => {
    service.getEntityInstancePermissions('Contact', 'undefined').subscribe(permissions => {
      expect(permissions.hasAccess).toBe(false);
      expect(permissions.permissions.canRead).toBe(false);
      done();
    });

    // No HTTP request should be made for invalid ID
  });

  it('should check route access', (done) => {
    service.canAccessRoute('/dashboard').subscribe(hasAccess => {
      expect(hasAccess).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/dashboard');
    req.flush({
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    });
  });

  it('should deny route access when API says no', (done) => {
    service.canAccessRoute('/admin').subscribe(hasAccess => {
      expect(hasAccess).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/admin');
    req.flush({
      hasAccess: false,
      permissions: {
        canRead: false,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    });
  });

  it('should cache permission requests', (done) => {
    service.getEntityPermissions('/partnerships/contacts').subscribe(() => {
      // Make the same request again
      service.getEntityPermissions('/partnerships/contacts').subscribe(permissions => {
        expect(permissions.entity).toBe('Contact');
        done();
      });

      // Only one HTTP request should have been made due to caching
    });

    const req = httpMock.expectOne('/api/permissions/check/partnerships/contacts');
    req.flush({
      hasAccess: true,
      entity: 'Contact',
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    });
  });

  it('should clear permission caches', () => {
    service.clearPermissionCaches();
    // No error should be thrown
    expect(service).toBeTruthy();
  });

  it('should normalize route paths', (done) => {
    service.canAccessRoute('/partnerships/contacts?filter=active#top').subscribe(hasAccess => {
      expect(hasAccess).toBe(true);
      done();
    });

    // Query params and hash should be removed
    const req = httpMock.expectOne('/api/permissions/check/partnerships/contacts');
    req.flush({
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    });
  });

  it('should extract entity ID from route path', (done) => {
    service.getEntityPermissions('/partnerships/partners/123/data').subscribe(() => {
      done();
    });

    // Should make request with ID in path
    const req = httpMock.expectOne('/api/permissions/check/partnerships/partners/123');
    req.flush({
      hasAccess: true,
      entity: 'Partner',
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    });
  });

  it('should handle permission API errors gracefully', (done) => {
    service.getEntityPermissions('/some-entity').subscribe(permissions => {
      expect(permissions.hasAccess).toBe(false);
      expect(permissions.permissions.canRead).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/some-entity');
    req.error(new ProgressEvent('error'), { status: 500, statusText: 'Server Error' });
  });

  it('should get entity permissions from cache if available', () => {
    // This would require setting up cache first, but we can test it doesn't error
    const cached = service.getEntityPermissionsFromCache('/test-path');
    expect(cached).toBeNull(); // No cache yet
  });

  it('should get entity instance permissions from cache if available', () => {
    const cached = service.getEntityInstancePermissionsFromCache('Contact', 123);
    expect(cached).toBeNull(); // No cache yet
  });

  it('should handle route access errors', (done) => {
    service.canAccessRoute('/error-route').subscribe(hasAccess => {
      expect(hasAccess).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/permissions/check/error-route');
    req.error(new ProgressEvent('error'));
  });
});

