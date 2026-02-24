/**
 * @fileoverview IAP Session Refresh Service for GCIP / Identity Platform
 * @author UNOPS Opportunity+ System Development Team
 *
 * GCIP access tokens expire after 1 hour (non-configurable, inherited from Firebase).
 * For external identities, Google recommends a persistent SESSION_REFRESHER iframe
 * that continuously keeps the session alive in the background.
 *
 * @see https://cloud.google.com/iap/docs/external-identity-sessions
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class IapSessionRefreshService {
  private readonly http = inject(HttpClient);
  private sessionRefresherIframe: HTMLIFrameElement | null = null;
  private isRefreshing = false;

  /**
   * @description Whether IAP session management should be active (non-dev, non-localhost)
   */
  shouldRun(): boolean {
    const cookies = document.cookie.split(';').map((c) => c.trim());
    const hasDevCookie = cookies.some((c) => c.startsWith('dev-user-email='));
    if (hasDevCookie) {
      return false;
    }

    const hostname = window.location.hostname;
    return (
      hostname !== 'localhost' &&
      hostname !== '127.0.0.1' &&
      !hostname.includes('localhost')
    );
  }

  /**
   * @description Embed a persistent SESSION_REFRESHER iframe.
   * IAP continuously refreshes the session in the background via this iframe.
   * Call once when the user is authenticated.
   */
  startSessionRefresher(): void {
    if (!this.shouldRun()) {
      console.log('[IAP-SESSION] startSessionRefresher skipped: shouldRun()=false');
      return;
    }

    if (this.sessionRefresherIframe) {
      console.log('[IAP-SESSION] SESSION_REFRESHER iframe already active');
      return;
    }

    const refresherUrl = `${window.location.origin}/?gcp-iap-mode=SESSION_REFRESHER`;
    console.log('[IAP-SESSION] Embedding persistent SESSION_REFRESHER iframe', {
      url: refresherUrl,
      hostname: window.location.hostname,
    });

    const iframe = document.createElement('iframe');
    iframe.src = refresherUrl;
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = 'none';
    iframe.style.position = 'absolute';
    iframe.style.display = 'none';
    iframe.setAttribute('aria-hidden', 'true');
    iframe.setAttribute('tabindex', '-1');

    iframe.onload = () => {
      console.log('[IAP-SESSION] SESSION_REFRESHER iframe loaded');
    };
    iframe.onerror = () => {
      console.warn('[IAP-SESSION] SESSION_REFRESHER iframe failed to load');
    };

    document.body.appendChild(iframe);
    this.sessionRefresherIframe = iframe;
  }

  /**
   * @description Remove the SESSION_REFRESHER iframe (e.g. on logout)
   */
  stopSessionRefresher(): void {
    if (this.sessionRefresherIframe) {
      if (this.sessionRefresherIframe.parentNode) {
        this.sessionRefresherIframe.parentNode.removeChild(this.sessionRefresherIframe);
      }
      this.sessionRefresherIframe = null;
      console.log('[IAP-SESSION] SESSION_REFRESHER iframe removed');
    }
  }

  /**
   * @description Reactive session refresh via DO_SESSION_REFRESH (on 401).
   * Opens a popup window for reauthentication per Google's recommended pattern.
   */
  async refreshSession(): Promise<boolean> {
    if (!this.shouldRun()) {
      console.log('[IAP-SESSION] refreshSession skipped: shouldRun()=false');
      return false;
    }

    if (this.isRefreshing) {
      console.log('[IAP-SESSION] refreshSession skipped: already refreshing');
      return false;
    }

    this.isRefreshing = true;
    const refreshUrl = `${window.location.origin}/?gcp-iap-mode=DO_SESSION_REFRESH`;
    console.log('[IAP-SESSION] Starting reactive session refresh (popup)', { url: refreshUrl });

    try {
      const success = await this.openRefreshWindow(refreshUrl);
      if (success) {
        console.log('[IAP-SESSION] Reactive session refresh succeeded');
      } else {
        console.warn('[IAP-SESSION] Reactive session refresh failed');
      }
      return success;
    } catch (err) {
      console.warn('[IAP-SESSION] Reactive session refresh error:', err);
      return false;
    } finally {
      this.isRefreshing = false;
    }
  }

  /**
   * @description Opens DO_SESSION_REFRESH in a popup window and polls until session is restored.
   * Follows Google's recommended pattern for programmatic 401 handling.
   */
  private openRefreshWindow(url: string): Promise<boolean> {
    return new Promise((resolve) => {
      const refreshWindow = window.open(url, '_iap_session_refresh', 'width=1,height=1');

      if (!refreshWindow) {
        console.warn('[IAP-SESSION] Popup blocked - falling back to iframe');
        this.iframeFallbackRefresh(url).then(resolve);
        return;
      }

      let attempts = 0;
      const maxAttempts = 20;

      const checkSession = () => {
        attempts++;

        if (refreshWindow.closed || attempts >= maxAttempts) {
          if (refreshWindow && !refreshWindow.closed) {
            refreshWindow.close();
          }
          this.verifySession().then(resolve);
          return;
        }

        fetch('/favicon.ico', {
          method: 'GET',
          credentials: 'include',
          headers: { 'X-Requested-With': 'XMLHttpRequest' },
        })
          .then((response) => {
            if (response.status === 401) {
              setTimeout(checkSession, 500);
            } else {
              refreshWindow.close();
              resolve(true);
            }
          })
          .catch(() => {
            setTimeout(checkSession, 500);
          });
      };

      setTimeout(checkSession, 1000);
    });
  }

  /**
   * @description Fallback: load DO_SESSION_REFRESH in hidden iframe if popup is blocked
   */
  private async iframeFallbackRefresh(url: string): Promise<boolean> {
    await this.loadIframe(url);
    await this.delay(2000);
    return this.verifySession();
  }

  private loadIframe(url: string): Promise<void> {
    return new Promise((resolve) => {
      const iframe = document.createElement('iframe');
      iframe.style.display = 'none';
      iframe.style.width = '0';
      iframe.style.height = '0';
      iframe.style.border = 'none';
      iframe.style.position = 'absolute';

      let done = false;
      const cleanup = () => {
        if (iframe.parentNode) {
          iframe.parentNode.removeChild(iframe);
        }
      };

      iframe.onload = () => {
        if (!done) {
          done = true;
        }
        cleanup();
        resolve();
      };
      iframe.onerror = () => {
        if (!done) {
          done = true;
        }
        cleanup();
        resolve();
      };

      document.body.appendChild(iframe);
      iframe.src = url;

      setTimeout(() => {
        if (!done) {
          done = true;
          cleanup();
          resolve();
        }
      }, 5000);
    });
  }

  private async verifySession(): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.http.get('/user/claims', {
          responseType: 'json',
          observe: 'response',
        })
      );
      const success =
        response.status === 200 && Array.isArray(response.body) && response.body.length > 0;
      console.log('[IAP-SESSION] verifySession result:', {
        status: response.status,
        claimsCount: Array.isArray(response.body) ? response.body.length : 0,
        success,
      });
      return success;
    } catch (err) {
      const status = err instanceof HttpErrorResponse ? err.status : 'unknown';
      console.warn('[IAP-SESSION] verifySession failed:', { status, error: err });
      return false;
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
