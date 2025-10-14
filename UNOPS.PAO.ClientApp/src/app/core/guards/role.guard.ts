import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { map, catchError, of, switchMap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

/**
 * Guard that checks if the user has any of the specified roles
 * @param allowedRoles An array of roles that are allowed to access the route
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    
    
    const router = inject(Router);
    const auth = inject(AuthService);
    const http = inject(HttpClient);
    
    if (!allowedRoles || allowedRoles.length === 0) {
      console.warn('[ROLE-GUARD] No roles specified, allowing access by default');
      return true;
    }
    
    // If the special value 'ALL' is included, everyone is allowed
    if (allowedRoles.includes('ALL')) {
      
      return true;
    }
    
    // Use the backend permission service to check the route
    // This centralizes permission logic to use our shared JSON configuration
    return http.get<{
      route: string; 
      hasAccess: boolean; 
      entity?: string; 
      permissions?: {
        canRead: boolean;
        canCreate: boolean;
        canUpdate: boolean;
        canDelete: boolean;
      }
    }>(`/api/permissions/check/${state.url}`).pipe(
      map(response => {
        const hasAccess = response.hasAccess;
        
        if (!hasAccess) {
          
          router.navigate(['/']);
          return false;
        }
        
        // Log the detailed permissions for debugging
        if (response.entity && response.permissions) {
          
        }
        
        return true;
      }),
      catchError(() => {
        // Fall back to local role checking if backend is not available
        
        
        // Fast path: For dev environment we can check cookies directly
        if (auth.hasDevCookie()) {
          const cookies = document.cookie.split(';').map(c => c.trim());
          const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
          if (devCookie) {
            const email = devCookie.substring('dev-user-email='.length);
            
            // Check roles
            let hasRequiredRole = false;
            
            // Administrator has access to everything
            if (allowedRoles.includes('Administrator') && email.toLowerCase().includes('admin')) {
              hasRequiredRole = true;
            }
            // Internal role check
            else if (allowedRoles.includes('Internal') && email.endsWith('@unops.org')) {
              hasRequiredRole = true;
            }
            // Partner role check
            else if (allowedRoles.includes('Partner') && email.includes('partner')) {
              hasRequiredRole = true;
            }
            // External role check
            else if (allowedRoles.includes('External') && email.includes('example.com')) {
              hasRequiredRole = true;
            }
            
            
            
            if (!hasRequiredRole) {
              
              router.navigate(['/']);
              return of(false);
            }
            
            return of(true);
          }
        }
        
        // Standard path: Check user roles against required roles
        return auth.getUserRoles().pipe(
          map(userRoles => {
            // Administrator always has access
            if (userRoles.includes('Administrator')) {
              return true;
            }
            
            // Check if user has any of the allowed roles
            const hasRole = allowedRoles.some(role => userRoles.includes(role));
            
            if (!hasRole) {
              
              router.navigate(['/']);
              return false;
            }
            
            return true;
          }),
          catchError(() => {
            console.error('[ROLE-GUARD] Error checking roles, redirecting to home');
            router.navigate(['/']);
            return of(false);
          })
        );
      })
    );
  };
}; 
