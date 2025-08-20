import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, tap, switchMap, shareReplay } from 'rxjs';
import { AuthService } from './auth.service';

export interface ApiEndpoint {
  path: string;
  methods: string[];
  allowedRoles: string[];
}

export interface RoutePermission {
  path: string;
  name: string;
  allowedRoles: string[];
  apiEndpoints?: ApiEndpoint[];
  children?: RoutePermission[];
}

export interface EntityPermission {
  name: string;
  permissions: Record<string, string[]>;
}

export interface PermissionConfig {
  routes: RoutePermission[];
  entities: EntityPermission[];
}

export interface EntityPermissions {
  route?: string;
  entity: string;
  hasAccess: boolean;
  permissions: {
    canRead: boolean;
    canCreate: boolean;
    canUpdate: boolean;
    canDelete: boolean;
    canApprove?: boolean;
    canActivate?: boolean;
    canClose?: boolean;
    canArchive?: boolean;
  };
}

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private permissionConfig$ = new BehaviorSubject<PermissionConfig | null>(null);
  private isLoadingConfig = false;
  
  // Cache for entity permissions
  private entityPermissionsCache = new Map<string, Observable<EntityPermissions>>();
  private entityInstancePermissionsCache = new Map<string, Observable<EntityPermissions>>();
  private currentEntityId: string | undefined;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { 
    this.loadConfig();
  }

  /**
   * Load permission configuration from the backend
   */
  loadConfig(): Observable<PermissionConfig> {
    if (this.isLoadingConfig) {
      return this.permissionConfig$.pipe(
        map(config => {
          if (!config) {
            throw new Error('Configuration still loading');
          }
          return config;
        })
      );
    }

    this.isLoadingConfig = true;

    return this.http.get<PermissionConfig>('/api/permissions').pipe(
      tap(config => {
        this.permissionConfig$.next(config);
        this.isLoadingConfig = false;
      }),
      catchError(error => {
        console.error('[PERMISSION-SERVICE] Error loading permission configuration', error);
        this.isLoadingConfig = false;
        
        // Create minimal config for fallback
        const minimalConfig: PermissionConfig = {
          routes: [
            {
              path: '/',
              name: 'Home',
              allowedRoles: ['ALL']
            }
          ],
          entities: []
        };
        
        this.permissionConfig$.next(minimalConfig);
        return of(minimalConfig);
      })
    );
  }

  /**
   * Get the current permission configuration
   */
  getConfig(): Observable<PermissionConfig> {
    if (this.permissionConfig$.getValue()) {
      return this.permissionConfig$.pipe(
        map(config => config as PermissionConfig)
      );
    }
    
    return this.loadConfig();
  }

  /**
   * Gets the full permissions response for an entity, route, or specific instance
   * @private
   */
  private getPermissionsResponse(path: string, id?: string | number): Observable<EntityPermissions> {
    // Don't normalize the path if an ID is explicitly provided
    let permissionUrl: string;
    
    if (id && id !== 'undefined' && id !== undefined) {
      // For explicit ID calls, use direct path construction
      permissionUrl = `/api/permissions/check/${path}/${id}`;
    } else {
      // For path-based calls, use normalization
      const { path: normalizedPath, entityId } = this.normalizeRoutePath(path);
      permissionUrl = `/api/permissions/check/${normalizedPath}`;
      
      // Only append entityId if it's valid
      if (entityId && entityId !== 'undefined') {
        permissionUrl += `/${entityId}`;
      }
    }
    
    // Check cache first
    const cached = this.entityPermissionsCache.get(permissionUrl);
    if (cached) {
      return cached;
    }
    
    // If not in cache, make the request and cache it
    const response = this.http.get<{
      route?: string;
      hasAccess: boolean;
      entity?: string;
      permissions: {
        canRead: boolean;
        canCreate: boolean;
        canUpdate: boolean;
        canDelete: boolean;
      };
    }>(permissionUrl).pipe(
      map(response => {
        return {
          route: response.route,
          entity: response.entity || path,
          hasAccess: !!response.hasAccess,
          permissions: response.permissions
        } as EntityPermissions;
      }),
      catchError(error => {
        console.error(`[PERMISSION-SERVICE] Error fetching permissions for ${permissionUrl}`, error);
        return of({
          entity: path,
          hasAccess: false,
          permissions: {
            canRead: false,
            canCreate: false,
            canUpdate: false,
            canDelete: false
          }
        });
      }),
      shareReplay(1)
    );
    
    this.entityPermissionsCache.set(permissionUrl, response);
    return response;
  }

  /**
   * Gets the full permission set for an entity or route
   * @param path The route path or entity name
   * @returns Observable with complete permission details
   */
  getEntityPermissions(path: string): Observable<EntityPermissions> {
    return this.getPermissionsResponse(path);
  }

  /**
   * Gets the full permission set for a specific entity instance
   * @param entityName The name of the entity (e.g., 'Contact', 'Partner')
   * @param id The ID of the entity instance
   * @returns Observable with complete permission details
   */
  getEntityInstancePermissions(entityName: string, id: string | number): Observable<EntityPermissions> {
    // Validate ID before making the call
    if (!id || id === 'undefined' || id === undefined) {
      console.warn(`[PERMISSION-SERVICE] Invalid ID provided for ${entityName}`);
      return of({
        entity: entityName,
        hasAccess: false,
        permissions: {
          canRead: false,
          canCreate: false,
          canUpdate: false,
          canDelete: false
        }
      });
    }
    return this.getPermissionsResponse(entityName, id);
  }

  /**
   * Check if the user has access to a specific route
   */
  canAccessRoute(route: string): Observable<boolean> {
    return this.getPermissionsResponse(route).pipe(
      map(response => response.hasAccess)
    );
  }

  /**
   * Gets entity permissions directly from cache if available
   * @param path The route path or entity name
   * @returns EntityPermissions if cached, null otherwise
   */
  getEntityPermissionsFromCache(path: string, id?: string | number): EntityPermissions | null {
    let cacheKey: string;
    
    if (id && id !== 'undefined' && id !== undefined) {
      cacheKey = `/api/permissions/check/${path}/${id}`;
    } else {
      const { path: normalizedPath, entityId } = this.normalizeRoutePath(path);
      cacheKey = `/api/permissions/check/${normalizedPath}`;
      if (entityId && entityId !== 'undefined') {
        cacheKey += `/${entityId}`;
      }
    }
    
    const cached = this.entityPermissionsCache.get(cacheKey);
    if (cached) {
      let latestValue: EntityPermissions | null = null;
      cached.subscribe(value => {
        latestValue = value;
      });
      return latestValue;
    }
    
    return null;
  }

  /**
   * Normalize a route path for permission checking and extract entity ID if present
   * @private
   */
  private normalizeRoutePath(route: string): { path: string, entityId?: string } {
    if (!route) {
      return { path: '' };
    }
    
    // Remove query parameters
    const queryParamIndex = route.indexOf('?');
    if (queryParamIndex > -1) {
      route = route.substring(0, queryParamIndex);
    }
    
    // Remove hash/fragment part
    const hashIndex = route.indexOf('#');
    if (hashIndex > -1) {
      route = route.substring(0, hashIndex);
    }
    
    // Remove leading and trailing slashes
    route = route.replace(/^\/+|\/+$/g, '');

    // Extract entity ID if present (e.g., partnerships/contacts/123)
    const segments = route.split('/');
    let entityId: string | undefined;
    
    // Only extract ID if it's a valid number and not undefined
    const lastSegment = segments[segments.length - 1];
    if (segments.length > 2 && !isNaN(Number(lastSegment)) && lastSegment !== 'undefined') {
      entityId = segments.pop(); // Remove the ID from segments
      route = segments.join('/'); // Rejoin without the ID
    }
    
    return { path: route, entityId };
  }

  /**
   * Clears the permission caches
   * Call this when navigating away or when permissions need to be refreshed
   */
  clearPermissionCaches() {
    const entityCacheSize = this.entityPermissionsCache.size;
    const instanceCacheSize = this.entityInstancePermissionsCache.size;
    
    
    
    this.entityPermissionsCache.clear();
    this.entityInstancePermissionsCache.clear();
    this.currentEntityId = undefined;
    
    
  }

  /**
   * Check if the current user has any of the specified roles
   * @private
   */
  private checkUserRoles(allowedRoles: string[]): Observable<boolean> {
    // Check dev cookies if available
    if (this.authService.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      if (devCookie) {
        const email = devCookie.substring('dev-user-email='.length);
        
        // Administrator has access to everything
        if (email.toLowerCase().includes('admin')) {
          return of(true);
        }
        
        // Check other roles
        if (allowedRoles.includes('Internal') && email.endsWith('@unops.org')) {
          return of(true);
        }
        
        if (allowedRoles.includes('Partner') && email.includes('partner')) {
          return of(true);
        }
        
        if (allowedRoles.includes('External') && email.includes('example.com')) {
          return of(true);
        }
        
        return of(false);
      }
    }
    
    // Get user roles from auth service
    return this.authService.getUserRoles().pipe(
      map(userRoles => {
        // Administrator always has access
        if (userRoles.includes('Administrator')) {
          return true;
        }
        
        // Check if any role matches
        return allowedRoles.some(role => userRoles.includes(role));
      }),
      catchError(() => of(false))
    );
  }

  /**
   * Gets entity instance permissions directly from cache if available
   * @param entityName The name of the entity (e.g., 'contact', 'partner')
   * @param id The ID of the entity instance
   * @returns EntityPermissions if cached, null otherwise
   */
  getEntityInstancePermissionsFromCache(entityName: string, id: string | number): EntityPermissions | null {
    const cacheKey = `${entityName.toLowerCase()}_${id}`;
    const cached = this.entityInstancePermissionsCache.get(cacheKey);
    
    if (cached) {
      // Get the latest value from the Observable
      let latestValue: EntityPermissions | null = null;
      cached.subscribe(value => {
        latestValue = value;
      });
      return latestValue;
    }
    
    return null;
  }
} 