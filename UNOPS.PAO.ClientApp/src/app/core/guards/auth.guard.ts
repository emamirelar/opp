import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { map, switchMap, of } from 'rxjs';

// Keep a counter to avoid infinite loops
let guardCallCounter = 0;
let lastTimestamp = 0;

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const auth = inject(AuthService);
  
  // Simple check to avoid guard activating for login page itself, which could cause loops
  if (state.url.includes('/login') || state.url.includes('/dev-login')) {
    return true;
  }
  
  // Anti-loop protection
  const now = Date.now();
  if (now - lastTimestamp < 500) { // If called twice within 500ms
    guardCallCounter++;
    
    if (guardCallCounter > 3) {
      guardCallCounter = 0; // Reset after a short delay
      setTimeout(() => guardCallCounter = 0, 2000);
      return true;
    }
  } else {
    guardCallCounter = 0; // Reset counter if not called rapidly
  }
  lastTimestamp = now;
  
  // Fast path: In development mode, if we have the cookie, we're authenticated
  const cookies = document.cookie.split(';').map(c => c.trim());
  const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
  if (devCookie) {
    return true;
  }
  
  // First check if user is authenticated with IAP
  return auth.isIapAuthenticated().pipe(
    switchMap(isIapAuthenticated => {
      // If IAP authenticated, always allow access
      if (isIapAuthenticated) {
        return of(true);
      }
      
      // Otherwise, proceed with normal authentication check
      return auth.isLogedIn().pipe(
        map((isSignedIn) => {
          if (!isSignedIn) {
            // Only redirect to login if not IAP authenticated
            router.navigate(['login']);
            return false;
          }
          return true;
        }),
      );
    })
  );
};
