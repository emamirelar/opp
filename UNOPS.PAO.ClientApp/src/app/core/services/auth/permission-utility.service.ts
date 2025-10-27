import { Injectable, inject, signal, ChangeDetectorRef } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { PermissionService, EntityPermissions } from './permission.service';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PermissionUtilityService {
  private permissionService = inject(PermissionService);
  private router = inject(Router);
  private lastRoute: string = '';

  constructor() {
    // Listen to router navigation events and clear cache when route changes
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      const newRoute = event.url;
      
      // Only clear cache if the route actually changed
      if (this.lastRoute !== newRoute) {
        
        this.permissionService.clearPermissionCaches();
        this.lastRoute = newRoute;
      }
    });
  }

  /**
   * Creates permission signals and loading logic for entity list components
   * @param entityName The name of the entity (e.g., 'Contact', 'Partner')
   * @returns Object containing permission signals and load function
   */
  createEntityPermissions(entityName: string) {
    const entityPermissions = signal<EntityPermissions>({
      entity: entityName,
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
    
    const permissionsLoading = signal<boolean>(true);

    const loadPermissions = (router: Router, cdr?: ChangeDetectorRef) => {
      permissionsLoading.set(true);
      
      // Clear cache before loading to ensure fresh permissions
      this.permissionService.clearPermissionCaches();
      
      // Get current route path for permission checking
      const currentPath = router.url;
      
      // Load from server (cache was cleared above)
      this.permissionService.getEntityPermissions(currentPath)
        .subscribe({
          next: (permissions) => {
            if (!permissions.hasAccess) {
              
              this.router.navigate(['/access-denied']);
            }
            
            entityPermissions.set(permissions);
            permissionsLoading.set(false);
            cdr?.detectChanges();
          },
          error: (error) => {
            console.error(`Error loading ${entityName} permissions:`, error);
            permissionsLoading.set(false);
            cdr?.detectChanges();
          }
        });
    };

    return {
      entityPermissions,
      permissionsLoading,
      loadPermissions
    };
  }

  /**
   * Creates permission signals and loading logic for entity instance components (view/edit)
   * @param entityName The name of the entity (e.g., 'Contact', 'Partner')
   * @returns Object containing permission signals and load function
   */
  createInstancePermissions(entityName: string) {
    const recordPermissions = signal<EntityPermissions>({
      entity: entityName,
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

    const loadPermissions = (entityId: string, cdr?: ChangeDetectorRef) => {
      if (!entityId) return;
      
      // Clear permission caches when loading a new entity instance
      this.permissionService.clearPermissionCaches();
      
      // Load permissions for the specific entity instance
      this.permissionService.getEntityInstancePermissions(entityName, entityId)
        .subscribe({
          next: (permissions) => {
            
            recordPermissions.set(permissions);
            cdr?.detectChanges();
          },
          error: (error) => {
            console.error(`Error loading ${entityName} instance permissions:`, error);
            // Set default permissions on error
            recordPermissions.set({
              entity: entityName,
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
            cdr?.detectChanges();
          }
        });
    };

    return {
      recordPermissions,
      loadPermissions
    };
  }

  /**
   * Manually clear all permission caches
   * Use this when you need to force a refresh of permissions
   */
  clearCaches() {
    
    this.permissionService.clearPermissionCaches();
  }

  // Utility methods for checking specific permissions
  canRead(permissions: EntityPermissions): boolean {
    return permissions.permissions.canRead;
  }

  canCreate(permissions: EntityPermissions): boolean {
    return permissions.permissions.canCreate;
  }

  canUpdate(permissions: EntityPermissions): boolean {
    return permissions.permissions.canUpdate;
  }

  canDelete(permissions: EntityPermissions): boolean {
    return permissions.permissions.canDelete;
  }
} 
