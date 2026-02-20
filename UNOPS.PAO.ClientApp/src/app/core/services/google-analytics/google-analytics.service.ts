/**
 * @fileoverview Google Analytics service - loads gtag.js only when enabled via configuration
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ConfigurationService } from '@core/services/configuration';

declare global {
  interface Window {
    dataLayer?: unknown[];
    gtag?: (...args: unknown[]) => void;
  }
}

/**
 * @class GoogleAnalyticsService
 * @description Injects Google Analytics (gtag.js) when enabled in configuration.
 * Typically enabled for Dev/Test/QA and disabled for Production.
 *
 * @example
 * ```typescript
 * // Called from APP_INITIALIZER after config loads
 * gaService.initializeIfEnabled();
 * ```
 *
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root',
})
export class GoogleAnalyticsService {
  private readonly document = inject(DOCUMENT);
  private readonly configService = inject(ConfigurationService);

  /**
   * @description Initializes Google Analytics only when enabled in configuration.
   * Requires config to be loaded first (called from APP_INITIALIZER).
   */
  initializeIfEnabled(): void {
    const config = this.configService.getConfig();
    const enabled = config?.googleAnalyticsEnabled === true;
    const measurementId = config?.googleAnalyticsMeasurementId;

    if (!enabled || !measurementId || typeof measurementId !== 'string') {
      return;
    }

    this.injectGtagScript(measurementId.trim());
  }

  private injectGtagScript(measurementId: string): void {
    const win = this.document.defaultView as Window | null;
    if (!win) return;
    if (win.gtag) return; // Already loaded

    const script1 = this.document.createElement('script');
    script1.async = true;
    script1.src = `https://www.googletagmanager.com/gtag/js?id=${measurementId}`;
    this.document.head.appendChild(script1);

    const script2 = this.document.createElement('script');
    script2.textContent = `
      window.dataLayer = window.dataLayer || [];
      function gtag(){dataLayer.push(arguments);}
      gtag('js', new Date());
      gtag('config', '${measurementId}');
    `;
    this.document.head.appendChild(script2);
  }
}
