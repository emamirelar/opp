import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/auth';
import { map, catchError, of } from 'rxjs';

/**
 * Guard that checks if the user has permission to access a route based on the centralized JSON config
 * This removes the need to hardcode roles in the route definitions
 */
export const routePermissionGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const permissionService = inject(PermissionService);
  
  return permissionService.canAccessRoute(state.url).pipe(
    map(hasAccess => {
      if (!hasAccess) {
        router.navigate(['/access-denied']);
        return false;
      }
      
      return true;
    }),
    catchError(error => {
      router.navigate(['/access-denied']);
      return of(false);
    })
  );
}; 
