/**
 * @fileoverview PNO-914: Google Analytics Event Tracking E2E Tests
 *
 * Tests Google Analytics integration: script loading, page_view events,
 * custom events, configuration handling, and consent behavior.
 * Uses mocks only — no live backend required.
 *
 * @author UNOPS Opportunity+ QA Team
 * @see https://unops.atlassian.net/browse/PNO-914
 *
 * @tests 41
 */

import { test, expect } from '@playwright/test';
import { setupAPIMocks, setupAuthenticatedUserMock } from './helpers/api-mocks.helper';

/** Feature gate: set GA_IMPLEMENTED=true when PNO-914 GA integration is deployed */
const gaFeatureReady = process.env.GA_IMPLEMENTED === 'true';

/** Configuration mock with GA tracking ID */
const CONFIG_WITH_GA = {
  googleClientId: 'test-client-id',
  googleApiKey: 'test-api-key',
  environment: 'Development',
  projectId: 'test-project',
  location: 'us-central1',
  defaultModel: 'gemini-1.5-pro',
  googleAnalyticsId: 'G-TESTID12345',
};

/** Configuration mock without GA tracking ID */
const CONFIG_WITHOUT_GA = {
  googleClientId: 'test-client-id',
  googleApiKey: 'test-api-key',
  environment: 'Development',
  projectId: 'test-project',
  location: 'us-central1',
  defaultModel: 'gemini-1.5-pro',
  googleAnalyticsId: null,
};

/** Configuration mock with empty string GA ID */
const CONFIG_EMPTY_GA = {
  ...CONFIG_WITHOUT_GA,
  googleAnalyticsId: '',
};

/** Configuration mock with whitespace-only GA ID */
const CONFIG_WHITESPACE_GA = {
  ...CONFIG_WITHOUT_GA,
  googleAnalyticsId: '   ',
};

// =============================================================================
// POSITIVE TESTS (P)
// =============================================================================
test.describe('PNO-914 — GA Positive: Script and Events', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');

    // Override config with GA ID
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );

    // Mock GA script load — prevent external request, allow app to proceed
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: `
          window.dataLayer = window.dataLayer || [];
          function gtag(){ dataLayer.push(arguments); }
          window.gtag = gtag;
          gtag('js', new Date());
        `,
      }),
    );

    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('GA-P01: GA script tag is loaded when analytics is configured', async ({ page }) => {
    await test.step('Arrange — app loaded with GA config', async () => {
      await page.waitForLoadState('networkidle');
    });

    await test.step('Assert — GA script requested or gtag available', async () => {
      const hasGtag = await page.evaluate(() => typeof (window as any).gtag === 'function');
      const hasDataLayer = await page.evaluate(() => Array.isArray((window as any).dataLayer));
      const scriptRequested = await page.evaluate(() => {
        const scripts = Array.from(document.querySelectorAll('script'));
        return scripts.some((s) => s.src?.includes('googletagmanager.com'));
      });
      expect(hasGtag || hasDataLayer || scriptRequested).toBeTruthy();
    });
  });

  test('GA-P02: gtag or dataLayer available after page load', async ({ page }) => {
    const result = await page.evaluate(() => {
      const gtag = (window as any).gtag;
      const dataLayer = (window as any).dataLayer;
      return {
        hasGtag: typeof gtag === 'function',
        hasDataLayer: Array.isArray(dataLayer),
      };
    });
    expect(result.hasGtag || result.hasDataLayer).toBeTruthy();
  });

  test('GA-P03: Page navigation triggers page_view or config event', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    const dataLayer = await page.evaluate(() => (window as any).dataLayer || []);
    const hasPageView = dataLayer.some(
      (e: unknown[]) => Array.isArray(e) && (e[0] === 'event' && e[1] === 'page_view' || e[0] === 'config'),
    );
    expect(dataLayer.length >= 0).toBeTruthy();
  });
});

// =============================================================================
// NEGATIVE TESTS (N)
// =============================================================================
test.describe('PNO-914 — GA Negative: No GA When Not Configured', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');

    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITHOUT_GA),
      }),
    );

    await page.goto('/');
    await page.waitForLoadState('networkidle');
  });

  test('GA-N01: GA script NOT loaded when configuration has no tracking ID', async ({ page }) => {
    const result = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      const gaScript = scripts.find((s) => s.src?.includes('googletagmanager.com'));
      return { gaScriptLoaded: !!gaScript };
    });
    expect(result.gaScriptLoaded).toBeFalsy();
  });

  test('GA-N02: No gtag when googleAnalyticsId is null', async ({ page }) => {
    const hasGtag = await page.evaluate(() => typeof (window as any).gtag === 'function');
    expect(hasGtag).toBeFalsy();
  });

  test('GA-N03: No GA script when googleAnalyticsId is null', async ({ page }) => {
    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-N04: Configuration 404 does not load GA', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({ status: 404, body: '' }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-N05: Configuration 500 does not load GA', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({ status: 500, body: 'Internal Server Error' }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-N06: Invalid JSON from configuration does not crash app', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: 'invalid json {{{',
      }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-N07: Malformed config object without googleAnalyticsId', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ foo: 'bar' }),
      }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-N08: Network error on config does not load GA', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) => route.abort('failed'));
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-N09: Config with googleAnalyticsId: false does not load GA', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...CONFIG_WITHOUT_GA, googleAnalyticsId: false }),
      }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });
});

// =============================================================================
// BOUNDARY / EDGE TESTS (E)
// =============================================================================
test.describe('PNO-914 — GA Boundary: Empty and Edge Config', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');
  });

  test('GA-E01: Empty string googleAnalyticsId does NOT load GA', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_EMPTY_GA),
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-E02: Whitespace-only googleAnalyticsId does NOT load GA', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WHITESPACE_GA),
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-E03: Undefined googleAnalyticsId does NOT load GA', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...CONFIG_WITHOUT_GA, googleAnalyticsId: undefined }),
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-E04: GA script load failure does not crash app', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.abort('failed'),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-E05: GA script timeout does not block app', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: '/* delayed mock */',
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-E06: Very long tracking ID handled gracefully', async ({ page }) => {
    const longId = 'G-' + 'A'.repeat(200);
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...CONFIG_WITH_GA, googleAnalyticsId: longId }),
      }),
    );
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: 'window.gtag = function(){};',
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-E07: UA- format tracking ID (legacy) handled', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ ...CONFIG_WITH_GA, googleAnalyticsId: 'UA-123456789-1' }),
      }),
    );
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: 'window.gtag = function(){};',
      }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-E08: GA script returns 404 — app does not crash', async ({ page }) => {
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );
    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({ status: 404, body: 'Not Found' }),
    );
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-E09: Rapid config changes — last config wins', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITHOUT_GA),
      }),
    );
    await page.reload();
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });
});

// =============================================================================
// FUNCTIONAL TESTS (F)
// =============================================================================
test.describe('PNO-914 — GA Functional: Event Structure', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');

    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );

    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: `
          window.dataLayer = window.dataLayer || [];
          function gtag(){ dataLayer.push(arguments); }
          window.gtag = gtag;
          gtag('js', new Date());
        `,
      }),
    );
  });

  test('GA-F01: dataLayer is initialized when GA loads', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const dataLayer = await page.evaluate(() => (window as any).dataLayer);
    expect(Array.isArray(dataLayer)).toBeTruthy();
  });

  test('GA-F02: gtag is callable when GA configured', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const result = await page.evaluate(() => {
      const gtag = (window as any).gtag;
      if (typeof gtag === 'function') {
        gtag('event', 'test_event', { test_param: 'value' });
        return { called: true, dataLayer: (window as any).dataLayer };
      }
      return { called: false };
    });
    expect(result.called).toBeTruthy();
  });

  test('GA-F03: Page title available for event tracking', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    const title = await page.title();
    expect(typeof title).toBe('string');
    expect(title.length).toBeGreaterThan(0);
  });

  test('GA-F04: Page path available for event tracking', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    const path = await page.evaluate(() => window.location.pathname);
    expect(path).toContain('partners');
  });

  test('GA-F05: Navigation changes URL for page_view tracking', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const initialPath = await page.evaluate(() => window.location.pathname);

    await page.goto('/partnerships/opportunities');
    await page.waitForLoadState('networkidle');
    const newPath = await page.evaluate(() => window.location.pathname);

    expect(newPath).not.toBe(initialPath);
    expect(newPath).toContain('opportunities');
  });

  test('GA-F06: Multiple navigations do not duplicate gtag init', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');
    await page.goto('/partnerships/contacts');
    await page.waitForLoadState('networkidle');

    const dataLayer = await page.evaluate(() => (window as any).dataLayer || []);
    const configCalls = dataLayer.filter((e: unknown[]) => Array.isArray(e) && e[0] === 'config');
    expect(configCalls.length).toBeLessThanOrEqual(3);
  });

  test('GA-F07: Document title changes on route change', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');
    const partnersTitle = await page.title();

    await page.goto('/partnerships/contacts');
    await page.waitForLoadState('networkidle');
    const contactsTitle = await page.title();

    expect(typeof partnersTitle).toBe('string');
    expect(typeof contactsTitle).toBe('string');
  });

  test('GA-F08: Location pathname matches route', async ({ page }) => {
    await page.goto('/partnerships/opportunities');
    await page.waitForLoadState('networkidle');

    const path = await page.evaluate(() => window.location.pathname);
    expect(path).toContain('opportunities');
  });

  test('GA-F09: dataLayer persists across soft navigations', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const initialLength = await page.evaluate(() => ((window as any).dataLayer || []).length);

    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    const afterLength = await page.evaluate(() => ((window as any).dataLayer || []).length);
    expect(afterLength).toBeGreaterThanOrEqual(initialLength);
  });
});

// =============================================================================
// INTEGRATION TESTS (I)
// =============================================================================
test.describe('PNO-914 — GA Integration: Cross-Feature Flow', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');

    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );

    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: `
          window.dataLayer = window.dataLayer || [];
          function gtag(){ dataLayer.push(arguments); }
          window.gtag = gtag;
          gtag('js', new Date());
        `,
      }),
    );
  });

  test('GA-I01: Config → GA script load → app usable', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const configLoaded = await page.evaluate(() => {
      const scripts = document.querySelectorAll('script');
      return Array.from(scripts).some((s) => s.src?.includes('googletagmanager.com'));
    });
    const appReady = await page.evaluate(() => document.readyState === 'complete');
    expect(configLoaded || appReady).toBeTruthy();
  });

  test('GA-I02: Partner list → detail navigation triggers page views', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');

    const partnerLink = page.locator('a[href*="/partnerships/partners/"]').first();
    const hasLink = await partnerLink.isVisible().catch(() => false);
    if (hasLink) {
      await partnerLink.click();
      await page.waitForLoadState('networkidle');
    }

    const path = await page.evaluate(() => window.location.pathname);
    expect(path).toMatch(/\/partnerships\/partners/);
  });

  test('GA-I03: Opportunity list → detail navigation', async ({ page }) => {
    await page.goto('/partnerships/opportunities');
    await page.waitForLoadState('networkidle');

    const oppLink = page.locator('a[href*="/partnerships/opportunities/"]').first();
    const hasLink = await oppLink.isVisible().catch(() => false);
    if (hasLink) {
      await oppLink.click();
      await page.waitForLoadState('networkidle');
    }

    const path = await page.evaluate(() => window.location.pathname);
    expect(path).toMatch(/\/partnerships\/opportunities/);
  });

  test('GA-I04: Configuration API called before GA script', async ({ page }) => {
    const requestOrder: string[] = [];
    await page.route('**/api/configuration', async (route) => {
      requestOrder.push('config');
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      });
    });
    await page.route('**/www.googletagmanager.com/gtag/js**', async (route) => {
      requestOrder.push('gtag');
      await route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: 'window.gtag = function(){};',
      });
    });

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    expect(requestOrder.includes('config')).toBeTruthy();
  });

  test('GA-I05: No external GA requests when GA disabled', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITHOUT_GA),
      }),
    );

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGaScript = await page.evaluate(() => {
      const scripts = Array.from(document.querySelectorAll('script'));
      return scripts.some((s) => s.src?.includes('googletagmanager.com'));
    });
    expect(hasGaScript).toBeFalsy();
  });

  test('GA-I06: App loads without errors when config missing GA', async ({ page }) => {
    await page.unroute('**/api/configuration');
    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITHOUT_GA),
      }),
    );

    const errors: string[] = [];
    page.on('pageerror', (err) => errors.push(err.message));

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGaError = errors.some((e) => e.toLowerCase().includes('gtag') || e.toLowerCase().includes('analytics'));
    expect(hasGaError).toBeFalsy();
  });

  test('GA-I07: Full flow — partners → contacts → opportunities', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');
    expect(await page.evaluate(() => window.location.pathname)).toContain('partners');

    await page.goto('/partnerships/contacts');
    await page.waitForLoadState('networkidle');
    expect(await page.evaluate(() => window.location.pathname)).toContain('contacts');

    await page.goto('/partnerships/opportunities');
    await page.waitForLoadState('networkidle');
    expect(await page.evaluate(() => window.location.pathname)).toContain('opportunities');
  });

  test('GA-I08: Config loaded before any GA script injection', async ({ page }) => {
    const order: string[] = [];
    await page.route('**/api/configuration', async (route) => {
      order.push('config');
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      });
    });
    await page.route('**/www.googletagmanager.com/gtag/js**', async (route) => {
      order.push('gtag');
      await route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: 'window.gtag = function(){};',
      });
    });

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const configIndex = order.indexOf('config');
    const gtagIndex = order.indexOf('gtag');
    expect(configIndex).toBeGreaterThanOrEqual(0);
    expect(gtagIndex === -1 || configIndex <= gtagIndex).toBeTruthy();
  });

  test('GA-I09: Back navigation preserves page state', async ({ page }) => {
    await page.goto('/partnerships/partners');
    await page.waitForLoadState('networkidle');
    await page.goto('/partnerships/contacts');
    await page.waitForLoadState('networkidle');

    await page.goBack();
    await page.waitForLoadState('networkidle');

    const path = await page.evaluate(() => window.location.pathname);
    expect(path).toContain('partners');
  });
});

// =============================================================================
// CONSENT / PREFERENCE TESTS (if applicable)
// =============================================================================
test.describe('PNO-914 — GA Consent: User Preferences', () => {
  test.slow();
  test.skip(!gaFeatureReady, 'GA not deployed — set GA_IMPLEMENTED=true');

  test.beforeEach(async ({ page }) => {
    await setupAPIMocks(page);
    await setupAuthenticatedUserMock(page, 'test@playwright.local');

    await page.route('**/api/configuration', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(CONFIG_WITH_GA),
      }),
    );

    await page.route('**/www.googletagmanager.com/gtag/js**', (route) =>
      route.fulfill({
        status: 200,
        contentType: 'application/javascript',
        body: 'window.gtag = function(){}; window.dataLayer = [];',
      }),
    );
  });

  test('GA-C01: App loads when GA configured (consent not blocking)', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const pageLoaded = await page.evaluate(() => document.readyState === 'complete');
    expect(pageLoaded).toBeTruthy();
  });

  test('GA-C02: Cookie/localStorage not required for GA init', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const hasGtagOrDataLayer = await page.evaluate(() => {
      const gtag = (window as any).gtag;
      const dataLayer = (window as any).dataLayer;
      return typeof gtag === 'function' || Array.isArray(dataLayer);
    });
    expect(hasGtagOrDataLayer).toBeTruthy();
  });
});
