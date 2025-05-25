import { Injectable, inject, signal, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { PermissionService, EntityPermissions } from './permission.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionUtilityService {
  private permissionService = inject(PermissionService);

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
        canDelete: false
      }
    });
    
    const permissionsLoading = signal<boolean>(true);

    const loadPermissions = (router: Router, cdr?: ChangeDetectorRef) => {
      permissionsLoading.set(true);
      
      // Get current route path for permission checking
      const currentPath = router.url;
      
      // First check cache
      const cachedPermissions = this.permissionService.getEntityPermissionsFromCache(currentPath);
      if (cachedPermissions) {
        entityPermissions.set(cachedPermissions);
        permissionsLoading.set(false);
        cdr?.detectChanges();
        return;
      }
      
      // If not cached, load from server
      this.permissionService.getEntityPermissions(currentPath)
        .subscribe({
          next: (permissions) => {
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
        canDelete: false
      }
    });

    const loadPermissions = (entityId: string, cdr?: ChangeDetectorRef) => {
      if (!entityId) return;
      
      // Clear permission caches when loading a new entity
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
                canDelete: false
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
   * Utility method to check if user can create entities
   */
  canCreate(entityPermissions: EntityPermissions): boolean {
    return entityPermissions.permissions.canCreate;
  }

  /**
   * Utility method to check if user can update entities
   */
  canUpdate(entityPermissions: EntityPermissions): boolean {
    return entityPermissions.permissions.canUpdate;
  }

  /**
   * Utility method to check if user can delete entities
   */
  canDelete(entityPermissions: EntityPermissions): boolean {
    return entityPermissions.permissions.canDelete;
  }

  /**
   * Utility method to check if user can read entities
   */
  canRead(entityPermissions: EntityPermissions): boolean {
    return entityPermissions.permissions.canRead;
  }
} 