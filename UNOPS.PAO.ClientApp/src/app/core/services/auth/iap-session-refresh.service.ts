/**
 * @fileoverview IAP Session Refresh Service - Proactively refreshes IAP/GCIP session before 1-hour expiry
 * @author UNOPS Opportunity+ System Development Team
 *
 * GCIP access tokens expire after 1 hour (non-configurable). This service uses IAP's
 * DO_SESSION_REFRESH to extend the session before expiry, preventing "Connection Lost" errors.
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

/** Refresh interval: 50 minutes (10-minute buffer before 1-hour expiry) */
const REFRESH_INTERVAL_MS = 50 * 60 * 1000;

@Injectable({
  providedIn: 'root',
})
/**
 * @class IapSessionRefreshService
 * @description Proactively refreshes IAP session to prevent 1-hour disconnection.
 * Uses IAP's DO_SESSION_REFRESH and runs a timer when user is authenticated in IAP mode.
 */
export class IapSessionRefreshService {
  private readonly http = inject(HttpClient);
  private refreshTimerId: ReturnType<typeof setInterval> | null = null;
  private isRefreshing = false;

  /**
   * @description Check if we're in IAP mode (production-like, no dev cookie)
   */
  shouldRun(): boolean {
    const cookies = document.cookie.split(';').map((c) => c.trim());
    const hasDevCookie = cookies.some((c) => c.startsWith('dev-user-email='));
    if (hasDevCookie) {
      return false;
    }

    const hostname = window.location.hostname;
    const isLocalOrDev =
      hostname === 'localhost' ||
      hostname === '127.0.0.1' ||
      hostname.includes('localhost');

    return !isLocalOrDev;
  }

  /**
   * @description Refresh IAP session via DO_SESSION_REFRESH, returns true if successful
   */
  async refreshSession(): Promise<boolean> {
    if (!this.shouldRun()) {
      console.log('[IAP-SESSION] refreshSession skipped: shouldRun()=false (dev cookie or localhost)');
      return false;
    }

    if (this.isRefreshing) {
      console.log('[IAP-SESSION] refreshSession skipped: already refreshing');
      return false;
    }

    this.isRefreshing = true;
    const refreshUrl = this.getRefreshUrl();
    console.log('[IAP-SESSION] Starting session refresh', { url: refreshUrl, hostname: window.location.hostname });

    try {
      await this.loadRefreshPage(refreshUrl);
      console.log('[IAP-SESSION] Refresh iframe loaded, waiting 1s before verification');
      await this.delay(1000);
      const verified = await this.verifySession();
      if (verified) {
        console.log('[IAP-SESSION] Session refreshed successfully - /user/claims returned 200');
      } else {
        console.warn('[IAP-SESSION] Session refresh verification failed - /user/claims returned 401 or empty');
      }
      return verified;
    } catch (err) {
      console.warn('[IAP-SESSION] Session refresh failed:', err);
      return false;
    } finally {
      this.isRefreshing = false;
    }
  }

  /**
   * @description Start proactive refresh timer (call when user is authenticated)
   */
  startProactiveRefresh(): void {
    if (!this.shouldRun()) {
      console.log('[IAP-SESSION] startProactiveRefresh skipped: shouldRun()=false');
      return;
    }
    if (this.refreshTimerId !== null) {
      console.log('[IAP-SESSION] startProactiveRefresh skipped: timer already running');
      return;
    }

    console.log('[IAP-SESSION] Starting proactive session refresh (every 50 min)', {
      hostname: window.location.hostname,
      nextRefreshIn: '50 minutes',
    });
    this.refreshTimerId = setInterval(() => {
      this.refreshSession();
    }, REFRESH_INTERVAL_MS);
  }

  /**
   * @description Stop proactive refresh timer
   */
  stopProactiveRefresh(): void {
    if (this.refreshTimerId) {
      clearInterval(this.refreshTimerId);
      this.refreshTimerId = null;
      console.log('[IAP-SESSION] Stopped proactive session refresh');
    }
  }

  private getRefreshUrl(): string {
    const base = window.location.origin + '/';
    return `${base}?gcp-iap-mode=DO_SESSION_REFRESH`;
  }

  private loadRefreshPage(url: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const iframe = document.createElement('iframe');
      iframe.style.display = 'none';
      iframe.style.position = 'absolute';
      iframe.style.width = '0';
      iframe.style.height = '0';
      iframe.style.border = 'none';

      let resolved = false;
      const cleanup = () => {
        if (iframe.parentNode) {
          iframe.parentNode.removeChild(iframe);
        }
      };

      iframe.onload = () => {
        if (!resolved) {
          resolved = true;
          console.log('[IAP-SESSION] Refresh iframe onload fired');
        }
        cleanup();
        resolve();
      };

      iframe.onerror = () => {
        if (!resolved) {
          resolved = true;
          console.warn('[IAP-SESSION] Refresh iframe onerror fired');
        }
        cleanup();
        reject(new Error('IAP session refresh iframe failed to load'));
      };

      document.body.appendChild(iframe);
      iframe.src = url;
      console.log('[IAP-SESSION] Refresh iframe created and loading', { url });

      setTimeout(() => {
        if (!resolved) {
          resolved = true;
          console.warn('[IAP-SESSION] Refresh iframe timeout (5s) - iframe may have redirected to auth-ui');
        }
        if (iframe.parentNode) {
          cleanup();
        }
        resolve();
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
