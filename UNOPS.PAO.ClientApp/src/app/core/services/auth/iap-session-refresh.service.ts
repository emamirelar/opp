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
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class IapSessionRefreshService {
  private readonly http = inject(HttpClient);
  private sessionRefresherIframe: HTMLIFrameElement | null = null;

  /**
   * Shared promise so concurrent 401 callers all wait for the same refresh attempt.
   * Prevents race condition where second caller gets `false` and redirects to login
   * while the first caller's refresh is still in progress.
   */
  private activeRefreshPromise: Promise<boolean> | null = null;

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
    if (!this.shouldRun() || this.sessionRefresherIframe) {
      return;
    }

    const refresherUrl = `${window.location.origin}/?gcp-iap-mode=SESSION_REFRESHER`;

    const iframe = document.createElement('iframe');
    iframe.src = refresherUrl;
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = 'none';
    iframe.style.position = 'absolute';
    iframe.style.display = 'none';
    iframe.setAttribute('aria-hidden', 'true');
    iframe.setAttribute('tabindex', '-1');

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
    }
  }

  /**
   * @description Reactive session refresh via DO_SESSION_REFRESH (on 401).
   * If a refresh is already in progress, all callers share the same promise
   * to avoid race conditions where a second caller redirects to login prematurely.
   */
  refreshSession(): Promise<boolean> {
    if (!this.shouldRun()) {
      return Promise.resolve(false);
    }

    if (this.activeRefreshPromise) {
      return this.activeRefreshPromise;
    }

    const refreshUrl = `${window.location.origin}/?gcp-iap-mode=DO_SESSION_REFRESH`;

    this.activeRefreshPromise = this.doRefresh(refreshUrl).finally(() => {
      this.activeRefreshPromise = null;
    });

    return this.activeRefreshPromise;
  }

  private async doRefresh(refreshUrl: string): Promise<boolean> {
    try {
      return await this.openRefreshWindow(refreshUrl);
    } catch {
      return false;
    }
  }

  /**
   * @description Opens DO_SESSION_REFRESH in a popup window and polls until session is restored.
   * Falls back to iframe if popup is blocked.
   */
  private openRefreshWindow(url: string): Promise<boolean> {
    return new Promise((resolve) => {
      const refreshWindow = window.open(url, '_iap_session_refresh', 'width=1,height=1');

      if (!refreshWindow) {
        this.iframeFallbackRefresh(url).then(resolve);
        return;
      }

      let attempts = 0;
      const maxAttempts = 30;

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
      return response.status === 200 && Array.isArray(response.body) && response.body.length > 0;
    } catch {
      return false;
    }
  }

  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
