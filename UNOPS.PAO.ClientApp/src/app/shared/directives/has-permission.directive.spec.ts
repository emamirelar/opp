import { HasPermissionDirective } from './has-permission.directive';
import { TestBed } from '@angular/core/testing';
import { TemplateRef, ViewContainerRef } from '@angular/core';
import { AuthService, UserInfo } from '@core/services/auth/auth.service';
import { BehaviorSubject } from 'rxjs';

describe('HasPermissionDirective', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockTemplateRef: jasmine.SpyObj<TemplateRef<any>>;
  let mockViewContainer: jasmine.SpyObj<ViewContainerRef>;
  let currentUserSubject: BehaviorSubject<UserInfo | null>;

  beforeEach(() => {
    currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
    
    mockAuthService = jasmine.createSpyObj('AuthService', ['hasPermission'], {
      currentUser$: currentUserSubject.asObservable()
    });
    
    mockTemplateRef = jasmine.createSpyObj('TemplateRef', ['createEmbeddedView']);
    mockViewContainer = jasmine.createSpyObj('ViewContainerRef', ['createEmbeddedView', 'clear']);
    
    TestBed.configureTestingModule({});
  });

  it('should create an instance', () => {
    const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
    expect(directive).toBeTruthy();
  });

  describe('permission checking with single permission', () => {
    it('should show element when user has required permission', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator', 'Internal']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
    });

    it('should hide element when user does not have required permission', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Internal']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();
      expect(mockViewContainer.clear).toHaveBeenCalled();
    });

    it('should hide element when user is null', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      currentUserSubject.next(null);
      
      expect(mockViewContainer.clear).toHaveBeenCalled();
      expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();
    });
  });

  describe('permission checking with multiple permissions', () => {
    it('should show element when user has any of the required permissions', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = ['Administrator', 'Manager', 'Editor'];
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Editor', 'Internal']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
    });

    it('should hide element when user has none of the required permissions', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = ['Administrator', 'Manager'];
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Internal', 'Viewer']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();
    });

    it('should show element when user has multiple matching permissions', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = ['Administrator', 'Manager'];
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator', 'Manager', 'Internal']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
    });
  });

  describe('element visibility state management', () => {
    it('should not create view multiple times if already shown', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator']
      };
      
      // Emit user twice
      currentUserSubject.next(mockUser);
      currentUserSubject.next(mockUser);
      
      // Should only create view once
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledTimes(1);
    });

    it('should clear and recreate view when permission changes from denied to granted', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      // First emit without permission
      const userWithoutPermission: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Internal']
      };
      
      currentUserSubject.next(userWithoutPermission);
      expect(mockViewContainer.clear).toHaveBeenCalled();
      
      // Then emit with permission
      const userWithPermission: UserInfo = {
        name: 'Test User',
        email: 'admin@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator', 'Internal']
      };
      
      currentUserSubject.next(userWithPermission);
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalledWith(mockTemplateRef);
    });

    it('should clear view when permission changes from granted to denied', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Administrator';
      
      directive.ngOnInit();
      
      // First emit with permission
      const userWithPermission: UserInfo = {
        name: 'Test User',
        email: 'admin@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator']
      };
      
      currentUserSubject.next(userWithPermission);
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalled();
      
      // Reset spy
      mockViewContainer.clear.calls.reset();
      
      // Then emit without permission
      const userWithoutPermission: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Internal']
      };
      
      currentUserSubject.next(userWithoutPermission);
      expect(mockViewContainer.clear).toHaveBeenCalled();
    });
  });

  describe('different permission types', () => {
    it('should handle Internal role permission', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Internal';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Internal']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalled();
    });

    it('should handle Partner role permission', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'Partner';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Partner User',
        email: 'partner@example.com',
        isInternal: false,
        isIapAuthenticated: true,
        roles: ['Partner']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalled();
    });

    it('should handle custom role permissions', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'CustomRole';
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'test@example.com',
        isInternal: false,
        isIapAuthenticated: true,
        roles: ['CustomRole', 'OtherRole']
      };
      
      currentUserSubject.next(mockUser);
      
      expect(mockViewContainer.createEmbeddedView).toHaveBeenCalled();
    });

    it('should be case-sensitive when checking permissions', () => {
      const directive = new HasPermissionDirective(mockTemplateRef, mockViewContainer, mockAuthService);
      directive.appHasPermission = 'administrator'; // lowercase
      
      directive.ngOnInit();
      
      const mockUser: UserInfo = {
        name: 'Test User',
        email: 'admin@unops.org',
        isInternal: true,
        isIapAuthenticated: true,
        roles: ['Administrator'] // capitalized
      };
      
      currentUserSubject.next(mockUser);
      
      // Should not match due to case difference
      expect(mockViewContainer.createEmbeddedView).not.toHaveBeenCalled();
    });
  });
});

