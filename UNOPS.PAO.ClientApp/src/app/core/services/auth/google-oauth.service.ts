/**
 * @fileoverview Google OAuth Token Management Service
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { SocialAuthService, GoogleLoginProvider, SocialUser } from '@abacritt/angularx-social-login';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * @interface GoogleOAuthToken
 * @description Interface for Google OAuth token information
 */
export interface GoogleOAuthToken {
  idToken: string;
  expiresAt: number; // Timestamp in milliseconds
  user: SocialUser;
}

/**
 * @class GoogleOAuthService
 * @description Centralized service for managing Google OAuth authentication and ID tokens.
 * Handles token validation, refresh, and provides tokens to components that need them.
 * 
 * @example
 * ```typescript
 * constructor(private googleOAuth: GoogleOAuthService) {}
 * 
 * async myMethod() {
 *   const token = await this.googleOAuth.getValidIdToken();
 *   // Use token for API calls
 * }
 * ```
 * 
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root'
})
export class GoogleOAuthService {
  private readonly socialAuthService = inject(SocialAuthService);
  private currentToken$ = new BehaviorSubject<GoogleOAuthToken | null>(null);

  /**
   * @description Observable of current OAuth token state
   * @type {Observable<GoogleOAuthToken | null>}
   */
  public readonly token$: Observable<GoogleOAuthToken | null> = this.currentToken$.asObservable();

  constructor() {
    // Subscribe to auth state changes
    this.socialAuthService.authState.subscribe(user => {
      if (user && user.idToken) {
        this.updateToken(user);
      } else {
        this.currentToken$.next(null);
      }
    });
  }

  /**
   * @description Update the stored token with new user data
   * @param {SocialUser} user - The authenticated social user
   * @returns {void}
   * @private
   */
  private updateToken(user: SocialUser): void {
    // Google ID tokens typically expire in 1 hour (3600 seconds)
    // We'll set expiration to 55 minutes to have a buffer
    const expiresAt = Date.now() + (55 * 60 * 1000);
    
    const token: GoogleOAuthToken = {
      idToken: user.idToken,
      expiresAt,
      user
    };
    
    this.currentToken$.next(token);
    console.log('🔑 Google OAuth token updated');
  }

  /**
   * @description Check if the current token is valid (exists and not expired)
   * @returns {boolean} True if token is valid, false otherwise
   * @example
   * ```typescript
   * if (this.googleOAuth.isTokenValid()) {
   *   // Use existing token
   * } else {
   *   // Need to authenticate
   * }
   * ```
   * @since 1.0.0
   */
  public isTokenValid(): boolean {
    const token = this.currentToken$.value;
    
    if (!token || !token.idToken) {
      return false;
    }
    
    // Check if token is expired
    const now = Date.now();
    if (now >= token.expiresAt) {
      console.log('⏰ Google OAuth token has expired');
      return false;
    }
    
    return true;
  }

  /**
   * @description Get the current ID token if it's valid
   * @returns {string | null} The ID token or null if invalid/missing
   * @example
   * ```typescript
   * const token = this.googleOAuth.getCurrentIdToken();
   * if (token) {
   *   // Use token
   * }
   * ```
   * @since 1.0.0
   */
  public getCurrentIdToken(): string | null {
    if (!this.isTokenValid()) {
      return null;
    }
    
    return this.currentToken$.value?.idToken || null;
  }

  /**
   * @description Get a valid ID token, triggering authentication if necessary
   * @param {boolean} forceRefresh - Force a fresh authentication even if token is valid
   * @returns {Promise<string>} Promise that resolves with the ID token
   * @throws {Error} If authentication fails
   * @example
   * ```typescript
   * try {
   *   const token = await this.googleOAuth.getValidIdToken();
   *   // Use token for API call
   * } catch (error) {
   *   console.error('Authentication failed:', error);
   * }
   * ```
   * @since 1.0.0
   */
  public async getValidIdToken(forceRefresh: boolean = false): Promise<string> {
    // If token is valid and not forcing refresh, return it
    if (!forceRefresh && this.isTokenValid()) {
      const token = this.getCurrentIdToken();
      if (token) {
        console.log('🔑 Using existing Google OAuth token');
        return token;
      }
    }

    // Token is invalid or expired, need to authenticate
    console.log('🔐 Triggering Google OAuth authentication...');
    
    try {
      // If forcing refresh, sign out first to ensure fresh token
      if (forceRefresh) {
        try {
          await this.socialAuthService.signOut();
          console.log('🔄 Signed out for token refresh');
        } catch (signOutError) {
          console.warn('Sign out failed (may not be signed in):', signOutError);
        }
      }

      // Trigger Google sign-in
      const user = await this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);

      if (!user || !user.idToken) {
        throw new Error('Failed to obtain Google ID token');
      }

      // Update token in the service
      this.updateToken(user);

      console.log('✅ Google OAuth authentication successful');
      return user.idToken;
    } catch (error) {
      console.error('❌ Google OAuth authentication failed:', error);
      throw new Error('Google authentication failed. Please try again.');
    }
  }

  /**
   * @description Refresh the current token by forcing re-authentication
   * @returns {Promise<string>} Promise that resolves with the new ID token
   * @example
   * ```typescript
   * const freshToken = await this.googleOAuth.refreshToken();
   * ```
   * @since 1.0.0
   */
  public async refreshToken(): Promise<string> {
    return this.getValidIdToken(true);
  }

  /**
   * @description Sign out from Google OAuth
   * @returns {Promise<void>}
   * @example
   * ```typescript
   * await this.googleOAuth.signOut();
   * ```
   * @since 1.0.0
   */
  public async signOut(): Promise<void> {
    try {
      await this.socialAuthService.signOut();
      this.currentToken$.next(null);
      console.log('👋 Signed out from Google OAuth');
    } catch (error) {
      console.error('Error signing out:', error);
      throw error;
    }
  }

  /**
   * @description Get the current authenticated user information
   * @returns {SocialUser | null} The current user or null if not authenticated
   * @example
   * ```typescript
   * const user = this.googleOAuth.getCurrentUser();
   * if (user) {
   *   console.log('User:', user.name, user.email);
   * }
   * ```
   * @since 1.0.0
   */
  public getCurrentUser(): SocialUser | null {
    const token = this.currentToken$.value;
    return token?.user || null;
  }
}

