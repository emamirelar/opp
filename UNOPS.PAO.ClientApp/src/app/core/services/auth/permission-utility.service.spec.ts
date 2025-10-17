import { TestBed } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { of, Subject } from 'rxjs';
import { PermissionUtilityService } from './permission-utility.service';
import { PermissionService, EntityPermissions } from './permission.service';
import { ChangeDetectorRef } from '@angular/core';

describe('PermissionUtilityService', () => {
  let service: PermissionUtilityService;
  let mockPermissionService: jasmine.SpyObj<PermissionService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let routerEventsSubject: Subject<any>;

  beforeEach(() => {
    routerEventsSubject = new Subject();
    
    mockPermissionService = jasmine.createSpyObj('PermissionService', [
      'getEntityPermissions',
      'getEntityInstancePermissions',
      'clearPermissionCaches'
    ]);
    
    // Create router mock with proper property definitions
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    Object.defineProperty(mockRouter, 'events', {
      value: routerEventsSubject.asObservable(),
      writable: false
    });
    Object.defineProperty(mockRouter, 'url', {
      value: '/test',
      writable: true,
      configurable: true
    });

    TestBed.configureTestingModule({
      providers: [
        PermissionUtilityService,
        { provide: PermissionService, useValue: mockPermissionService },
        { provide: Router, useValue: mockRouter }
      ]
    });

    service = TestBed.inject(PermissionUtilityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should create entity permissions with default values', () => {
    const result = service.createEntityPermissions('Contact');

    expect(result.entityPermissions).toBeDefined();
    expect(result.permissionsLoading).toBeDefined();
    expect(result.loadPermissions).toBeDefined();
    expect(result.entityPermissions().entity).toBe('Contact');
    expect(result.entityPermissions().hasAccess).toBe(false);
  });

  it('should load entity permissions', (done) => {
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

    mockPermissionService.getEntityPermissions.and.returnValue(of(mockPermissions));
    mockPermissionService.clearPermissionCaches.and.stub();

    const result = service.createEntityPermissions('Contact');
    
    expect(result.permissionsLoading()).toBe(true);

    result.loadPermissions(mockRouter);

    setTimeout(() => {
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
      expect(mockPermissionService.getEntityPermissions).toHaveBeenCalledWith('/test');
      expect(result.entityPermissions().hasAccess).toBe(true);
      expect(result.permissionsLoading()).toBe(false);
      done();
    }, 10);
  });

  it('should redirect to access-denied when user lacks permissions', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'Contact',
      hasAccess: false,
      permissions: {
        canRead: false,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    mockPermissionService.getEntityPermissions.and.returnValue(of(mockPermissions));
    mockPermissionService.clearPermissionCaches.and.stub();

    const result = service.createEntityPermissions('Contact');
    result.loadPermissions(mockRouter);

    setTimeout(() => {
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      done();
    }, 10);
  });

  it('should create instance permissions', () => {
    const result = service.createInstancePermissions('Partner');

    expect(result.recordPermissions).toBeDefined();
    expect(result.loadPermissions).toBeDefined();
    expect(result.recordPermissions().entity).toBe('Partner');
    expect(result.recordPermissions().hasAccess).toBe(false);
  });

  it('should load instance permissions', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'Partner',
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: true,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    mockPermissionService.getEntityInstancePermissions.and.returnValue(of(mockPermissions));
    mockPermissionService.clearPermissionCaches.and.stub();

    const result = service.createInstancePermissions('Partner');
    result.loadPermissions('123');

    setTimeout(() => {
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
      expect(mockPermissionService.getEntityInstancePermissions).toHaveBeenCalledWith('Partner', '123');
      expect(result.recordPermissions().hasAccess).toBe(true);
      done();
    }, 10);
  });

  it('should not load instance permissions without entity ID', () => {
    const result = service.createInstancePermissions('Partner');
    result.loadPermissions('');

    expect(mockPermissionService.getEntityInstancePermissions).not.toHaveBeenCalled();
  });

  it('should handle permission load errors', (done) => {
    mockPermissionService.getEntityPermissions.and.returnValue(
      of({
        entity: 'Contact',
        hasAccess: false,
        permissions: {
          canRead: false,
          canCreate: false,
          canUpdate: false,
          canDelete: false,
          canExport: false,
          canImport: false
        }
      })
    );
    mockPermissionService.clearPermissionCaches.and.stub();

    const result = service.createEntityPermissions('Contact');
    result.loadPermissions(mockRouter);

    setTimeout(() => {
      expect(result.permissionsLoading()).toBe(false);
      done();
    }, 10);
  });

  it('should clear caches on navigation', () => {
    mockPermissionService.clearPermissionCaches.and.stub();

    routerEventsSubject.next(new NavigationEnd(1, '/old', '/new'));

    expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
  });

  it('should not clear caches on same route navigation', () => {
    mockPermissionService.clearPermissionCaches.and.stub();

    routerEventsSubject.next(new NavigationEnd(1, '/same', '/same'));
    
    // First call clears cache
    expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalledTimes(1);
    
    routerEventsSubject.next(new NavigationEnd(2, '/same', '/same'));
    
    // Should not clear again for same route
    expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalledTimes(1);
  });

  it('should manually clear caches', () => {
    mockPermissionService.clearPermissionCaches.and.stub();

    service.clearCaches();

    expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
  });

  it('should check if can read', () => {
    const permissions: EntityPermissions = {
      entity: 'Test',
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    expect(service.canRead(permissions)).toBe(true);
  });

  it('should check if can create', () => {
    const permissions: EntityPermissions = {
      entity: 'Test',
      hasAccess: true,
      permissions: {
        canRead: false,
        canCreate: true,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    expect(service.canCreate(permissions)).toBe(true);
  });

  it('should check if can update', () => {
    const permissions: EntityPermissions = {
      entity: 'Test',
      hasAccess: true,
      permissions: {
        canRead: false,
        canCreate: false,
        canUpdate: true,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    expect(service.canUpdate(permissions)).toBe(true);
  });

  it('should check if can delete', () => {
    const permissions: EntityPermissions = {
      entity: 'Test',
      hasAccess: true,
      permissions: {
        canRead: false,
        canCreate: false,
        canUpdate: false,
        canDelete: true,
        canExport: false,
        canImport: false
      }
    };

    expect(service.canDelete(permissions)).toBe(true);
  });

  it('should work with ChangeDetectorRef', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'Contact',
      hasAccess: true,
      permissions: {
        canRead: true,
        canCreate: false,
        canUpdate: false,
        canDelete: false,
        canExport: false,
        canImport: false
      }
    };

    mockPermissionService.getEntityPermissions.and.returnValue(of(mockPermissions));
    mockPermissionService.clearPermissionCaches.and.stub();

    const mockCdr = jasmine.createSpyObj<ChangeDetectorRef>('ChangeDetectorRef', ['detectChanges']);

    const result = service.createEntityPermissions('Contact');
    result.loadPermissions(mockRouter, mockCdr);

    setTimeout(() => {
      expect(mockCdr.detectChanges).toHaveBeenCalled();
      done();
    }, 10);
  });
});

