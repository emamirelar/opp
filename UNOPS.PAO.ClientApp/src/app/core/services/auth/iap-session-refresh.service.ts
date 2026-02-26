/**
 * @fileoverview IAP Session Refresh Service for GCIP / Identity Platform
 * @author UNOPS Opportunity+ System Development Team
 *
 * GCIP access tokens expire after 1 hour (non-configurable, inherited from Firebase).
 * Hidden iframes cannot complete the GCIP auth-ui token exchange due to cross-origin
 * cookie restrictions. A first-party popup window is required — it opens off-screen
 * and auto-closes so the user never sees it.
 *
 * @see https://cloud.google.com/iap/docs/external-identity-sessions
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** Proactive refresh: 45 minutes (15-minute buffer before 1-hour expiry) */
const PROACTIVE_REFRESH_MS = 45 * 60 * 1000;

@Injectable({
  providedIn: 'root',
})
export class IapSessionRefreshService {
  private readonly http = inject(HttpClient);
  private proactiveTimerId: ReturnType<typeof setInterval> | null = null;

  /**
   * Shared promise so concurrent 401 callers all wait for the same refresh attempt.
   * Prevents race condition where a second caller redirects to login
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
   * @description Start proactive session refresh every 45 minutes.
   * Call once when the user is authenticated.
   */
  startProactiveRefresh(): void {
    if (!this.shouldRun() || this.proactiveTimerId !== null) {
      return;
    }

    this.proactiveTimerId = setInterval(() => {
      this.refreshSession();
    }, PROACTIVE_REFRESH_MS);
  }

  /**
   * @description Stop proactive refresh (e.g. on logout)
   */
  stopProactiveRefresh(): void {
    if (this.proactiveTimerId !== null) {
      clearInterval(this.proactiveTimerId);
      this.proactiveTimerId = null;
    }
  }

  /**
   * @description Refresh the IAP session via a popup window with DO_SESSION_REFRESH.
   * The popup opens off-screen and auto-closes after the refresh completes.
   * If a refresh is already in progress, all callers share the same promise.
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
      return await this.openRefreshPopup(refreshUrl);
    } catch {
      return false;
    }
  }

  /**
   * Opens DO_SESSION_REFRESH in a popup positioned off-screen.
   * The popup auto-closes after 3 seconds — just enough time for IAP
   * to process the refresh and set the session cookie.
   */
  private openRefreshPopup(url: string): Promise<boolean> {
    return new Promise((resolve) => {
      const popup = window.open(
        url,
        '_iap_refresh',
        'width=1,height=1,left=-9999,top=-9999,menubar=no,toolbar=no,location=no,status=no'
      );

      if (!popup) {
        resolve(false);
        return;
      }

      setTimeout(() => {
        try {
          if (!popup.closed) {
            popup.close();
          }
        } catch {
          // Cross-origin — popup already navigated away
        }

        this.delay(500).then(() => {
          this.verifySession().then(resolve);
        });
      }, 3000);
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
