import { SocialUser } from '@abacritt/angularx-social-login';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient) {}

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

  public user() {
    return this.http.get<UserClaim[]>('/user/claims');
  }

  public isInternal() {
    return this.http.get<boolean>('/user/isInternal');
  }

  public isLogedIn(): Observable<boolean> {
    return this.user().pipe(
      map((userClaims) => {
        const hasClaims = userClaims.length > 0;
        return !hasClaims ? false : true;
      }),
      catchError((error) => {
        return of(false);
      }),
    );
  }

  public isAdmin(): Observable<boolean> {
    return this.getUserRoles().pipe(
      map(roles => roles.includes('opp_admin') || true),
      catchError(() => of(false))
    );
  }

  public getUserRoles(): Observable<string[]> {
    return this.user().pipe(
      map(claims => {
        const roleClaims = claims.filter(claim => claim.type === 'role');
        return roleClaims.map(claim => claim.value);
      }),
      catchError(() => of([]))
    );
  }

  public logOut() {
    return this.http.post('/user/logout', {});
  }
}

export interface UserClaim {
  type: string;
  value: string;
}
