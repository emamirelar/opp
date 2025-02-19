import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const auth = inject(AuthService);

  return auth.isLogedIn().pipe(
    map((isSignedIn) => {
      if (!isSignedIn) {
        router.navigate(['login']);
        return false;
      }
      return true;
    }),
  );
};
