import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map, catchError, of, switchMap } from 'rxjs';
import { HttpClient } from '@angular/common/http';

/**
 * Guard that checks if the user has any of the specified roles
 * @param allowedRoles An array of roles that are allowed to access the route
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    console.log(`[ROLE-GUARD] Checking roles for route: ${state.url}`);
    
    const router = inject(Router);
    const auth = inject(AuthService);
    const http = inject(HttpClient);
    
    if (!allowedRoles || allowedRoles.length === 0) {
      console.warn('[ROLE-GUARD] No roles specified, allowing access by default');
      return true;
    }
    
    // If the special value 'ALL' is included, everyone is allowed
    if (allowedRoles.includes('ALL')) {
      console.log('[ROLE-GUARD] ALL role specified, allowing access');
      return true;
    }
    
    // Use the backend permission service to check the route
    // This centralizes permission logic to use our shared JSON configuration
    return http.get<{route: string, hasAccess: boolean}>(`/api/permissions/check/${state.url}`).pipe(
      map(response => {
        const hasAccess = response.hasAccess;
        
        if (!hasAccess) {
          console.log(`[ROLE-GUARD] Backend denied access to ${state.url}, redirecting to home`);
          router.navigate(['/']);
          return false;
        }
        
        return true;
      }),
      catchError(() => {
        // Fall back to local role checking if backend is not available
        console.log('[ROLE-GUARD] Error checking permissions from backend, falling back to local checks');
        
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
            
            console.log(`[ROLE-GUARD] Dev user ${email} access to ${state.url}: ${hasRequiredRole}`);
            
            if (!hasRequiredRole) {
              console.log(`[ROLE-GUARD] User ${email} doesn't have required roles, redirecting to home`);
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
              console.log(`[ROLE-GUARD] User doesn't have required roles (${allowedRoles.join(', ')}), redirecting to home`);
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