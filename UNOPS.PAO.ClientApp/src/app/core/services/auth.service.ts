import {SocialUser} from '@abacritt/angularx-social-login';
import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {BehaviorSubject, catchError, map, Observable, of} from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  // Cache IAP authentication status to prevent excessive API calls
  private iapAuthenticationChecked = false;
  private iapAuthenticationStatus = false;
  private redirectCounter = 0;  // Track how many times redirect attempts happen
  private isCheckingAuth = false; // Flag to prevent concurrent checks

  constructor(private http: HttpClient) {
    this.checkAuthStatus();
  }

  // Check current authentication status when service is initialized
  private checkAuthStatus() {
    // First check for dev cookie to avoid unnecessary API calls
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    if (devCookie) {
      // If we have a dev cookie, create a basic user info without making API calls
      const email = devCookie.substring('dev-user-email='.length);

      // Generate appropriate roles
      const roles = [];
      if (email.endsWith('@unops.org')) {
        roles.push('Internal');
        if (email.toLowerCase().includes('admin')) {
          roles.push('Administrator');
        }
      } else {
        roles.push('Partner');
      }
      roles.push('User');

      const userInfo: UserInfo = {
        name: email,
        email: email,
        isInternal: email.endsWith('@unops.org'),
        isIapAuthenticated: true,
        roles: roles
      };

      // Update the current user subject
      this.currentUserSubject.next(userInfo);
      return;
    }

    // If no dev cookie, try the regular authentication check
    this.isLogedIn().pipe(
      catchError(() => of(false))
    ).subscribe(isLoggedIn => {
      if (isLoggedIn) {
        this.user().pipe(
          catchError(error => {
            return of([]);
          })
        ).subscribe(claims => {
          if (claims && claims.length) {
            const userInfo: UserInfo = this.parseUserInfoFromClaims(claims);
            this.currentUserSubject.next(userInfo);
          }
        });
      }
    });
  }

  private parseUserInfoFromClaims(claims: UserClaim[]): UserInfo {
    const userInfo: UserInfo = {
      name: '',
      email: '',
      isInternal: false,
      isIapAuthenticated: false,
      roles: []
    };

    claims.forEach(claim => {
      if (claim.type === 'name') userInfo.name = claim.value;
      if (claim.type === 'email') userInfo.email = claim.value;
      if (claim.type === 'IsInternal') userInfo.isInternal = claim.value.toLowerCase() === 'true';
      if (claim.type === 'IAPAuthenticated') userInfo.isIapAuthenticated = claim.value.toLowerCase() === 'true';
      if (claim.type === 'role') {
        if (!userInfo.roles.includes(claim.value)) {
          userInfo.roles.push(claim.value);
        }
      }
    });

    return userInfo;
  }

  public signUp(userEmail: string, password: string) {
    return this.http.post('/user/register', {
      email: userEmail,
      password: password,
    });
  }

  public googleSignIn(user: SocialUser) {
    return this.http.post('/user/googleSignIn', {
      provider: user.provider,
      idToken: user.idToken,
    });
  }

  public logIn(userName: string, password: string) {
    return this.http.post('/user/login?useCookies=true', {
      email: userName,
      password: password,
    });
  }

  private createSyntheticClaimsFromCookie(): UserClaim[] {
    // Get the dev cookie
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));

    if (!devCookie) {
      return [];
    }

    // Extract email from cookie
    const email = devCookie.substring('dev-user-email='.length);

    // Create basic claims
    const claims: UserClaim[] = [
      { type: 'name', value: email },
      { type: 'email', value: email },
      { type: 'IsInternal', value: email.includes('@unops.org') ? 'true' : 'false' },
      { type: 'IAPAuthenticated', value: 'true' }
    ];

    // Add default roles based on email domain
    // UNOPS employees get Internal role
    if (email.endsWith('@unops.org')) {
      claims.push({ type: 'role', value: 'Internal' });

      // If email contains admin, also give Administrator role
      if (email.toLowerCase().includes('admin')) {
        claims.push({ type: 'role', value: 'Administrator' });
      }
    } else {
      // Non-UNOPS emails get Partner role by default
      claims.push({ type: 'role', value: 'Partner' });
    }

    // Everyone gets basic User role
    claims.push({ type: 'role', value: 'User' });

    return claims;
  }

  // Check if we have a dev cookie
  public hasDevCookie(): boolean {
    const cookies = document.cookie.split(';').map(c => c.trim());
    return cookies.some(c => c.startsWith('dev-user-email='));
  }

  // Get user info from the auth test endpoint
  public getAuthInfo(): Observable<any> {
    // Check for dev cookie first to avoid unnecessary API calls
    if (this.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      const email = devCookie!.substring('dev-user-email='.length);

      // Generate roles based on email
      const roles = [];
      if (email.endsWith('@unops.org')) {
        roles.push('Internal');
        if (email.toLowerCase().includes('admin')) {
          roles.push('Administrator');
        }
      } else {
        roles.push('Partner');
      }
      roles.push('User');

      // Return synthetic auth info without making API call
      return of({
        email: email,
        hasIapEmailHeader: true,
        iapAuthenticated: true,
        devMode: true,
        roles: roles,
        isInternal: email.endsWith('@unops.org')
      });
    }

    return this.http.get('/api/dev/check-iap-simulation').pipe(
      catchError(error => {
        return of({
          hasIapEmailHeader: false,
          iapAuthenticated: false,
          roles: []
        });
      })
    );
  }

  public user(): Observable<UserClaim[]> {
    // If we have a dev cookie, don't even try the API - just use synthetic claims
    /*if (this.hasDevCookie()) {
      return of(this.createSyntheticClaimsFromCookie());
    }*/

    // Otherwise try the API with fallback to synthetic claims
    return this.http.get<UserClaim[]>('/user/claims').pipe(
      map(claims => {
        return claims;
      }),
      catchError(error => {
        console.error('DEBUG - Error getting user claims from API:', error);
        // If API fails but we have a dev cookie, use synthetic claims
        if (this.hasDevCookie()) {
          const syntheticClaims = this.createSyntheticClaimsFromCookie();
          return of(syntheticClaims);
        }

        // If no dev cookie, just return an empty array
        return of([]);
      })
    );
  }

  // Check if user has a specific role
  public hasRole(role: string): Observable<boolean> {
    // Fast path for dev cookies
    if (this.hasDevCookie()) {
      const cookies = document.cookie.split(';').map(c => c.trim());
      const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
      const email = devCookie!.substring('dev-user-email='.length);

      // Determine roles based on email
      let hasRequestedRole = false;

      // Administrator role check
      if (role === 'Administrator') {
        hasRequestedRole = email.toLowerCase().includes('admin');
      }

      // Internal role check
      else if (role === 'Internal') {
        hasRequestedRole = email.endsWith('@unops.org');
      }

      // Partner role check
      else if (role === 'Partner') {
        hasRequestedRole = !email.endsWith('@unops.org');
      }

      // User role - everyone has this
      else if (role === 'User') {
        hasRequestedRole = true;
      }

      return of(hasRequestedRole);
    }

    // Otherwise use the current user from the behavior subject
    return this.currentUser$.pipe(
      map(user => user?.roles.includes(role) || false)
    );
  }

  // Reset the redirect counter and authentication state
  public resetAuthenticationState(): void {
    this.redirectCounter = 0;
    this.iapAuthenticationChecked = false;
    this.iapAuthenticationStatus = false;
    this.isCheckingAuth = false;
  }

  // Check if user has IAP authentication
  public isIapAuthenticated(): Observable<boolean> {
    // Check for cookie directly - most reliable approach
    const cookies = document.cookie.split(';').map(c => c.trim());
    const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
    if (devCookie) {
      // No need to log or do extra processing here
      return of(true);
    }

    // Prevent excessive API calls: use cached result if available
    if (this.iapAuthenticationChecked) {
      return of(this.iapAuthenticationStatus);
    }

    // Simple check to avoid loops
    if (this.isCheckingAuth) {
      return of(false);
    }

    // Set checking flag
    this.isCheckingAuth = true;

    // Check if we're in a development environment
    const hostname = window.location.hostname;
    const isDevelopment = hostname === 'localhost' || hostname.includes('localhost') || hostname.startsWith('dev-');

    if (isDevelopment) {
      // Only make the dev simulation check call in development environments
      return this.http.get<any>('/api/dev/check-iap-simulation').pipe(
        map(result => {
          // Check only for the header
          const isAuthenticated = result && result.hasIapHeader === true;

          // Cache result
          this.iapAuthenticationChecked = true;
          this.iapAuthenticationStatus = isAuthenticated;

          // Reset checking flag
          this.isCheckingAuth = false;

          return isAuthenticated;
        }),
        catchError((error) => {
          // On error, reset flags and return false
          this.isCheckingAuth = false;
          return of(false);
        })
      );
    } else {
      // In test/production, use the user claims endpoint to check authentication
      return this.http.get<UserClaim[]>('/user/claims').pipe(
        map(claims => {
          const isAuthenticated = claims.length > 0;

          // Cache result
          this.iapAuthenticationChecked = true;
          this.iapAuthenticationStatus = isAuthenticated;

          // Reset checking flag
          this.isCheckingAuth = false;

          return isAuthenticated;
        }),
        catchError(() => {
          this.isCheckingAuth = false;
          return of(false);
        })
      );
    }
  }

  public isLogedIn(): Observable<boolean> {
    // First check for dev cookie - if present, user is definitely logged in
    if (this.hasDevCookie()) {
      return of(true);
    }

    return this.user().pipe(
      map((userClaims) => {
        return userClaims.length > 0;
      }),
      catchError((error) => {
        return of(false);
      }),
    );
  }

  public isAdmin(): Observable<boolean> {
    return this.getUserRoles().pipe(
      map(roles => {
        return roles.includes('PARTNER_GLOB_ADMIN') || roles.includes('ORG_UNIT_ADMIN');
      }),
      catchError((error) => {
        console.error('DEBUG - Error in isAdmin():', error);
        return of(false);
      })
    );
  }

  public isGlobalAdmin(): Observable<boolean> {
    return this.getUserRoles().pipe(
      map(roles => {
        return roles.includes('PARTNER_GLOB_ADMIN');
      }),
      catchError((error) => {
        console.error('DEBUG - Error in isGlobalAdmin():', error);
        return of(false);
      })
    );
  }

  public getUserRoles(): Observable<string[]> {
    return this.user().pipe(
      map(claims => {
        const roleClaims = claims.filter(claim => claim.type === 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role');
        return roleClaims.map(claim => claim.value.toUpperCase());
      }),
      catchError((error) => {
        console.error('DEBUG - Error in getUserRoles():', error);
        return of([]);
      })
    );
  }
}

export interface UserClaim {
  type: string;
  value: string;
}

export interface UserInfo {
  name: string;
  email: string;
  isInternal: boolean;
  isIapAuthenticated: boolean;
  roles: string[];
}
