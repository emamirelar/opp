import { TestBed } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { of, throwError, Subject } from 'rxjs';
import { PermissionUtilityService } from './permission-utility.service';
import { PermissionService, EntityPermissions } from './permission.service';

describe('PermissionUtilityService', () => {
  let service: PermissionUtilityService;
  let mockPermissionService: jasmine.SpyObj<PermissionService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockChangeDetectorRef: jasmine.SpyObj<ChangeDetectorRef>;
  let routerEvents: Subject<any>;

  const mockEntityPermissions: EntityPermissions = {
    entity: 'Contact',
    hasAccess: true,
    permissions: {
      canRead: true,
      canCreate: true,
      canUpdate: true,
      canDelete: false
    }
  };

  const mockNoAccessPermissions: EntityPermissions = {
    entity: 'Contact',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false
    }
  };

  beforeEach(() => {
    routerEvents = new Subject();
    
    const permissionServiceSpy = jasmine.createSpyObj('PermissionService', [
      'getEntityPermissions',
      'getEntityInstancePermissions',
      'clearPermissionCaches'
    ]);
    
    const routerSpy = jasmine.createSpyObj('Router', ['navigate'], {
      events: routerEvents.asObservable(),
      url: '/test-path'
    });
    
    const cdrSpy = jasmine.createSpyObj('ChangeDetectorRef', ['detectChanges']);

    TestBed.configureTestingModule({
      providers: [
        PermissionUtilityService,
        { provide: PermissionService, useValue: permissionServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(PermissionUtilityService);
    mockPermissionService = TestBed.inject(PermissionService) as jasmine.SpyObj<PermissionService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockChangeDetectorRef = cdrSpy;

    mockPermissionService.getEntityPermissions.and.returnValue(of(mockEntityPermissions));
    mockPermissionService.getEntityInstancePermissions.and.returnValue(of(mockEntityPermissions));
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('route change handling', () => {
    it('should clear permission caches when route changes', () => {
      const navigationEvent = new NavigationEnd(1, '/new-path', '/new-path');
      
      routerEvents.next(navigationEvent);
      
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
    });

    it('should not clear caches for same route', () => {
      const navigationEvent1 = new NavigationEnd(1, '/same-path', '/same-path');
      const navigationEvent2 = new NavigationEnd(2, '/same-path', '/same-path');
      
      routerEvents.next(navigationEvent1);
      mockPermissionService.clearPermissionCaches.calls.reset();
      
      routerEvents.next(navigationEvent2);
      
      expect(mockPermissionService.clearPermissionCaches).not.toHaveBeenCalled();
    });

    it('should only process NavigationEnd events', () => {
      const otherEvent = { type: 'NavigationStart' };
      
      routerEvents.next(otherEvent);
      
      expect(mockPermissionService.clearPermissionCaches).not.toHaveBeenCalled();
    });
  });

  describe('createEntityPermissions', () => {
    it('should create entity permissions with initial state', () => {
      const result = service.createEntityPermissions('Contact');
      
      expect(result.entityPermissions()).toEqual({
        entity: 'Contact',
        hasAccess: false,
        permissions: {
          canRead: false,
          canCreate: false,
          canUpdate: false,
          canDelete: false
        }
      });
      expect(result.permissionsLoading()).toBe(true);
    });

    it('should load permissions successfully', () => {
      const result = service.createEntityPermissions('Contact');
      
      result.loadPermissions(mockRouter, mockChangeDetectorRef);
      
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
      expect(mockPermissionService.getEntityPermissions).toHaveBeenCalledWith('/test-path');
      expect(result.entityPermissions()).toEqual(mockEntityPermissions);
      expect(result.permissionsLoading()).toBe(false);
      expect(mockChangeDetectorRef.detectChanges).toHaveBeenCalled();
    });

    it('should navigate to access-denied when no access', () => {
      mockPermissionService.getEntityPermissions.and.returnValue(of(mockNoAccessPermissions));
      const result = service.createEntityPermissions('Contact');
      
      result.loadPermissions(mockRouter, mockChangeDetectorRef);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/access-denied']);
      expect(result.entityPermissions()).toEqual(mockNoAccessPermissions);
    });

    it('should handle permission loading errors', () => {
      const error = new Error('Permission error');
      mockPermissionService.getEntityPermissions.and.returnValue(throwError(() => error));
      spyOn(console, 'error');
      
      const result = service.createEntityPermissions('Contact');
      result.loadPermissions(mockRouter, mockChangeDetectorRef);
      
      expect(console.error).toHaveBeenCalledWith('Error loading Contact permissions:', error);
      expect(result.permissionsLoading()).toBe(false);
      expect(mockChangeDetectorRef.detectChanges).toHaveBeenCalled();
    });

    it('should work without ChangeDetectorRef', () => {
      const result = service.createEntityPermissions('Contact');
      
      expect(() => result.loadPermissions(mockRouter)).not.toThrow();
      expect(result.entityPermissions()).toEqual(mockEntityPermissions);
    });
  });

  describe('createInstancePermissions', () => {
    it('should create instance permissions with initial state', () => {
      const result = service.createInstancePermissions('Contact');
      
      expect(result.recordPermissions()).toEqual({
        entity: 'Contact',
        hasAccess: false,
        permissions: {
          canRead: false,
          canCreate: false,
          canUpdate: false,
          canDelete: false
        }
      });
    });

    it('should load instance permissions successfully', () => {
      const entityId = '123';
      const result = service.createInstancePermissions('Contact');
      
      result.loadPermissions(entityId, mockChangeDetectorRef);
      
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
      expect(mockPermissionService.getEntityInstancePermissions).toHaveBeenCalledWith('Contact', entityId);
      expect(result.recordPermissions()).toEqual(mockEntityPermissions);
      expect(mockChangeDetectorRef.detectChanges).toHaveBeenCalled();
    });

    it('should not load permissions for empty entityId', () => {
      const result = service.createInstancePermissions('Contact');
      
      result.loadPermissions('', mockChangeDetectorRef);
      result.loadPermissions(null as any, mockChangeDetectorRef);
      result.loadPermissions(undefined as any, mockChangeDetectorRef);
      
      expect(mockPermissionService.getEntityInstancePermissions).not.toHaveBeenCalled();
    });

    it('should handle instance permission loading errors', () => {
      const error = new Error('Instance permission error');
      mockPermissionService.getEntityInstancePermissions.and.returnValue(throwError(() => error));
      spyOn(console, 'error');
      
      const result = service.createInstancePermissions('Contact');
      result.loadPermissions('123', mockChangeDetectorRef);
      
      expect(console.error).toHaveBeenCalledWith('Error loading Contact instance permissions:', error);
      expect(result.recordPermissions()).toEqual({
        entity: 'Contact',
        hasAccess: false,
        permissions: {
          canRead: false,
          canCreate: false,
          canUpdate: false,
          canDelete: false
        }
      });
      expect(mockChangeDetectorRef.detectChanges).toHaveBeenCalled();
    });

    it('should work without ChangeDetectorRef', () => {
      const result = service.createInstancePermissions('Contact');
      
      expect(() => result.loadPermissions('123')).not.toThrow();
      expect(result.recordPermissions()).toEqual(mockEntityPermissions);
    });
  });

  describe('clearCaches', () => {
    it('should clear permission caches', () => {
      service.clearCaches();
      
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalled();
    });
  });

  describe('permission utility methods', () => {
    it('should check canRead permission', () => {
      expect(service.canRead(mockEntityPermissions)).toBe(true);
      expect(service.canRead(mockNoAccessPermissions)).toBe(false);
    });

    it('should check canCreate permission', () => {
      expect(service.canCreate(mockEntityPermissions)).toBe(true);
      expect(service.canCreate(mockNoAccessPermissions)).toBe(false);
    });

    it('should check canUpdate permission', () => {
      expect(service.canUpdate(mockEntityPermissions)).toBe(true);
      expect(service.canUpdate(mockNoAccessPermissions)).toBe(false);
    });

    it('should check canDelete permission', () => {
      expect(service.canDelete(mockEntityPermissions)).toBe(false);
      expect(service.canDelete(mockNoAccessPermissions)).toBe(false);
    });
  });

  describe('integration scenarios', () => {
    it('should handle complete permission loading workflow', () => {
      const result = service.createEntityPermissions('Partner');
      
      // Initial state
      expect(result.permissionsLoading()).toBe(true);
      expect(result.entityPermissions().hasAccess).toBe(false);
      
      // Load permissions
      result.loadPermissions(mockRouter, mockChangeDetectorRef);
      
      // Final state
      expect(result.permissionsLoading()).toBe(false);
      expect(result.entityPermissions().hasAccess).toBe(true);
      expect(service.canRead(result.entityPermissions())).toBe(true);
      expect(service.canCreate(result.entityPermissions())).toBe(true);
      expect(service.canUpdate(result.entityPermissions())).toBe(true);
      expect(service.canDelete(result.entityPermissions())).toBe(false);
    });

    it('should handle route change during permission loading', () => {
      const result = service.createEntityPermissions('Contact');
      
      // Start loading permissions
      result.loadPermissions(mockRouter, mockChangeDetectorRef);
      
      // Route changes - should trigger cache clear
      const navigationEvent = new NavigationEnd(1, '/new-path', '/new-path');
      routerEvents.next(navigationEvent);
      
      // Cache should be cleared twice: once for loadPermissions, once for route change
      expect(mockPermissionService.clearPermissionCaches).toHaveBeenCalledTimes(2);
    });
  });
});
