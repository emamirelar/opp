import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, map, of, tap, switchMap } from 'rxjs';
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

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private permissionConfig$ = new BehaviorSubject<PermissionConfig | null>(null);
  private isLoadingConfig = false;

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
   * Check if the user has access to a specific route
   */
  canAccessRoute(route: string): Observable<boolean> {
    // Normalize the route by removing query parameters and ensuring it starts with /
    const normalizedRoute = this.normalizeRoutePath(route);
    
    return this.http.get<{route: string, hasAccess: boolean}>(`/api/permissions/check/${normalizedRoute.startsWith('/') ? normalizedRoute.substring(1) : normalizedRoute}`).pipe(
      map(response => response.hasAccess),
      catchError(() => {
        // Fall back to local checking using the config
        return this.getConfig().pipe(
          switchMap(config => {
            // Find the route in the config
            const routeConfig = this.findRouteConfig(config.routes, normalizedRoute);
            if (!routeConfig) {
              return of(false);
            }
            
            // Check if ALL is allowed
            if (routeConfig.allowedRoles.includes('ALL')) {
              return of(true);
            }
            
            // Check user roles - first using dev cookies if available
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
                if (routeConfig.allowedRoles.includes('Internal') && email.endsWith('@unops.org')) {
                  return of(true);
                }
                
                if (routeConfig.allowedRoles.includes('Partner') && email.includes('partner')) {
                  return of(true);
                }
                
                if (routeConfig.allowedRoles.includes('External') && email.includes('example.com')) {
                  return of(true);
                }
                
                return of(false);
              }
            }
            
            // Fall back to AuthService role check
            return this.authService.getUserRoles().pipe(
              map(userRoles => {
                // Administrator always has access
                if (userRoles.includes('Administrator')) {
                  return true;
                }
                
                // Check if any role matches
                return routeConfig.allowedRoles.some(role => userRoles.includes(role));
              }),
              catchError(() => of(false))
            );
          })
        );
      })
    );
  }

  /**
   * Normalize a route path for permission checking
   * This makes the route consistent with how it's defined in the permission config
   */
  private normalizeRoutePath(route: string): string {
    if (!route) {
      return '/';
    }
    
    // Ensure route starts with slash
    route = route.startsWith('/') ? route : '/' + route;
    
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
    
    // Split the route into segments
    const segments = route.split('/');
    
    // Normalize segments that are likely parameters (numbers, guids)
    for (let i = 0; i < segments.length; i++) {
      const segment = segments[i];
      
      // Skip empty segments
      if (!segment) {
        continue;
      }
      
      // Check if segment is a number or a GUID
      if (/^\d+$/.test(segment) || 
          /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(segment)) {
        // Replace with generic ID marker to match route patterns
        segments[i] = ':id';
      }
    }
    
    return segments.join('/');
  }

  /**
   * Find a route configuration from a path
   */
  private findRouteConfig(routes: RoutePermission[], path: string): RoutePermission | null {
    path = path.startsWith('/') ? path.substring(1) : path;
    
    // First try direct match
    for (const route of routes) {
      const routePath = route.path.startsWith('/') ? route.path.substring(1) : route.path;
      if (routePath === path) {
        return route;
      }
      
      // Check children
      if (route.children && path.startsWith(routePath)) {
        const remainingPath = path.substring(routePath.length);
        const childPath = remainingPath.startsWith('/') ? remainingPath.substring(1) : remainingPath;
        
        for (const child of route.children) {
          if (child.path === childPath) {
            return child;
          }
        }
      }
    }
    
    return null;
  }
} 