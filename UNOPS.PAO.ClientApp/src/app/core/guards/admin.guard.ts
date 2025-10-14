import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { map, tap } from 'rxjs/operators';

/**
 * Guard to check if the current user has admin permissions
 * Redirects to access-denied if not an admin user
 */
export const adminGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAdmin().pipe(
    tap(isAdmin => {
      if (!isAdmin) {
        router.navigate(['/access-denied']);
      }
    })
  );
}; 
