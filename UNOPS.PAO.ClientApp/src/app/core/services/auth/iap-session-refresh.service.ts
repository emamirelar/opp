/**
 * @fileoverview IAP Session Refresh Service - Proactively refreshes IAP/GCIP session before 1-hour expiry
 * @author UNOPS Opportunity+ System Development Team
 *
 * GCIP access tokens expire after 1 hour (non-configurable). This service uses IAP's
 * DO_SESSION_REFRESH to extend the session before expiry, preventing "Connection Lost" errors.
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
      return false;
    }

    if (this.isRefreshing) {
      return false;
    }

    this.isRefreshing = true;

    try {
      const refreshUrl = this.getRefreshUrl();
      await this.loadRefreshPage(refreshUrl);
      await this.delay(1000);
      const verified = await this.verifySession();
      if (verified) {
        console.log('[IAP-SESSION] Session refreshed successfully');
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
    if (!this.shouldRun() || this.refreshTimerId !== null) {
      return;
    }

    console.log('[IAP-SESSION] Starting proactive session refresh (every 50 min)');
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

      const cleanup = () => {
        if (iframe.parentNode) {
          iframe.parentNode.removeChild(iframe);
        }
      };

      iframe.onload = () => {
        cleanup();
        resolve();
      };

      iframe.onerror = () => {
        cleanup();
        reject(new Error('IAP session refresh iframe failed to load'));
      };

      document.body.appendChild(iframe);
      iframe.src = url;

      setTimeout(() => {
        if (iframe.parentNode) {
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
