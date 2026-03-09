/**
 * @fileoverview Role-Based Access Control (RBAC) Test Helper
 * Provides role definitions, authentication helpers, and permission mock 
 * configurations for comprehensive role-based E2E testing.
 * 
 * System Roles (from BaseRole.cs):
 * - SYSTEM_ADMIN (Administrator) - Full system access
 * - PARTNER_GLOB_ADMIN - Partnership admin with full CRUD + export/import
 * - PARTNER_USER - Partnership user, create/edit capabilities
 * - ORG_UNIT_ADMIN - Org-unit scoped administration
 * - UNOPS_GEN_USER (GENUSER) - General read-only user
 * 
 * Permission Flags (from PermissionNames.cs):
 * canRead, canCreate, canUpdate, canDelete, canExport, canImport,
 * canActivate, canClose, canArchive, canApprove, canUnapprove, isTeamMember
 */

import { Page } from '@playwright/test';
import { setupAPIMocks } from './api-mocks.helper';

// ============================================================
// ROLE DEFINITIONS
// ============================================================

/**
 * Permission flags for route-level access checks
 */
export interface RoutePermissions {
  canRead: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  canExport: boolean;
  canImport: boolean;
  canApprove: boolean;
  canActivate: boolean;
  canClose: boolean;
  canArchive: boolean;
}

/**
 * Permission flags for entity-level (record-level) checks
 */
export interface EntityPermissions {
  canView: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canSubmit: boolean;
  canApprove: boolean;
  canActivate: boolean;
  canCancel: boolean;
}

/**
 * User claims for authentication mock
 */
export interface UserClaims {
  email: string;
  name: string;
  roles: string[];
  isInternal: boolean;
  sub: string;
}

/**
 * Complete role configuration
 */
export interface RoleConfig {
  name: string;
  claims: UserClaims;
  partnerPermissions: RoutePermissions;
  contactPermissions: RoutePermissions;
  interactionPermissions: RoutePermissions;
  opportunityPermissions: RoutePermissions;
  partnerEntityPermissions: EntityPermissions;
  opportunityEntityPermissions: EntityPermissions;
  canAccessAdmin: boolean;
  canAccessUserManagement: boolean;
  canAccessAIPrompts: boolean;
  canAccessEntityManager: boolean;
}

// ============================================================
// ROLE CONFIGURATIONS
// ============================================================

/**
 * SYSTEM_ADMIN / Administrator - Full system access
 */
export const SYSTEM_ADMIN: RoleConfig = {
  name: 'System Administrator',
  claims: {
    email: 'admin@playwright.local',
    name: 'System Admin',
    // Administrator role + PARTNER_GLOB_ADMIN to trigger isAdmin() check in sidebar
    // (isAdmin() only checks for PARTNER_GLOB_ADMIN or ORG_UNIT_ADMIN)
    roles: ['Administrator', 'PARTNER_GLOB_ADMIN', 'Internal'],
    isInternal: true,
    sub: '10001',
  },
  partnerPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: true, canActivate: true,
    canClose: true, canArchive: true,
  },
  contactPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  interactionPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  opportunityPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: true, canActivate: true,
    canClose: true, canArchive: true,
  },
  partnerEntityPermissions: {
    canView: true, canEdit: true, canDelete: true,
    canSubmit: true, canApprove: true, canActivate: true, canCancel: true,
  },
  opportunityEntityPermissions: {
    canView: true, canEdit: true, canDelete: true,
    canSubmit: true, canApprove: true, canActivate: true, canCancel: true,
  },
  canAccessAdmin: true,
  canAccessUserManagement: true,
  canAccessAIPrompts: true,
  canAccessEntityManager: true,
};

/**
 * PARTNER_GLOB_ADMIN - Partnership admin with full CRUD + export/import
 */
export const PARTNER_GLOB_ADMIN: RoleConfig = {
  name: 'Partner Global Admin',
  claims: {
    email: 'partner.admin@playwright.local',
    name: 'Partner Global Admin',
    roles: ['PARTNER_GLOB_ADMIN', 'Internal'],
    isInternal: true,
    sub: '10002',
  },
  partnerPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: true,
    canClose: true, canArchive: true,
  },
  contactPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  interactionPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  opportunityPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: true,
    canExport: true, canImport: true, canApprove: false, canActivate: true,
    canClose: true, canArchive: true,
  },
  partnerEntityPermissions: {
    canView: true, canEdit: true, canDelete: true,
    canSubmit: true, canApprove: false, canActivate: true, canCancel: true,
  },
  opportunityEntityPermissions: {
    canView: true, canEdit: true, canDelete: true,
    canSubmit: true, canApprove: false, canActivate: true, canCancel: true,
  },
  canAccessAdmin: true,
  canAccessUserManagement: true,
  canAccessAIPrompts: true,
  canAccessEntityManager: true,
};

/**
 * PARTNER_USER - Partnership user, create/edit only
 */
export const PARTNER_USER: RoleConfig = {
  name: 'Partner User',
  claims: {
    email: 'partner.user@playwright.local',
    name: 'Partner User',
    roles: ['PartnerUser', 'Internal'],
    isInternal: true,
    sub: '10003',
  },
  partnerPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  contactPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  interactionPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  opportunityPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  partnerEntityPermissions: {
    canView: true, canEdit: true, canDelete: false,
    canSubmit: true, canApprove: false, canActivate: false, canCancel: false,
  },
  opportunityEntityPermissions: {
    canView: true, canEdit: true, canDelete: false,
    canSubmit: true, canApprove: false, canActivate: false, canCancel: false,
  },
  // Partner User has NO admin access per sidebar.component.ts
  canAccessAdmin: false,
  canAccessUserManagement: false,
  canAccessAIPrompts: false,
  canAccessEntityManager: false,
};

/**
 * ORG_UNIT_ADMIN - Org-unit scoped administration
 */
export const ORG_UNIT_ADMIN: RoleConfig = {
  name: 'Org Unit Admin',
  claims: {
    email: 'orgunit.admin@playwright.local',
    name: 'Org Unit Admin',
    roles: ['ORG_UNIT_ADMIN', 'Internal'],
    isInternal: true,
    sub: '10004',
  },
  partnerPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: true, canImport: false, canApprove: false, canActivate: true,
    canClose: false, canArchive: false,
  },
  contactPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: true, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  interactionPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: true, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  opportunityPermissions: {
    canRead: true, canCreate: true, canUpdate: true, canDelete: false,
    canExport: true, canImport: false, canApprove: false, canActivate: true,
    canClose: false, canArchive: false,
  },
  partnerEntityPermissions: {
    canView: true, canEdit: true, canDelete: false,
    canSubmit: true, canApprove: false, canActivate: true, canCancel: false,
  },
  opportunityEntityPermissions: {
    canView: true, canEdit: true, canDelete: false,
    canSubmit: true, canApprove: false, canActivate: true, canCancel: false,
  },
  canAccessAdmin: true,
  canAccessUserManagement: true,
  canAccessAIPrompts: false,
  canAccessEntityManager: false,
};

/**
 * UNOPS_GEN_USER / GENUSER - General read-only user
 */
export const GENERAL_USER: RoleConfig = {
  name: 'General User',
  claims: {
    email: 'general.user@playwright.local',
    name: 'General User',
    roles: ['GENUSER'],
    isInternal: false,
    sub: '10005',
  },
  partnerPermissions: {
    canRead: true, canCreate: false, canUpdate: false, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  contactPermissions: {
    canRead: true, canCreate: false, canUpdate: false, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  interactionPermissions: {
    canRead: true, canCreate: false, canUpdate: false, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  opportunityPermissions: {
    canRead: true, canCreate: false, canUpdate: false, canDelete: false,
    canExport: false, canImport: false, canApprove: false, canActivate: false,
    canClose: false, canArchive: false,
  },
  partnerEntityPermissions: {
    canView: true, canEdit: false, canDelete: false,
    canSubmit: false, canApprove: false, canActivate: false, canCancel: false,
  },
  opportunityEntityPermissions: {
    canView: true, canEdit: false, canDelete: false,
    canSubmit: false, canApprove: false, canActivate: false, canCancel: false,
  },
  // General User has NO admin access - read-only across the system
  canAccessAdmin: false,
  canAccessUserManagement: false,
  canAccessAIPrompts: false,
  canAccessEntityManager: false,
};

/** All role configurations for iteration */
export const ALL_ROLES: RoleConfig[] = [
  SYSTEM_ADMIN,
  PARTNER_GLOB_ADMIN,
  PARTNER_USER,
  ORG_UNIT_ADMIN,
  GENERAL_USER,
];

// ============================================================
// AUTHENTICATION & MOCK SETUP HELPERS
// ============================================================

/**
 * Authenticate as a specific role and navigate to a target page.
 * Sets up all necessary API mocks with role-specific permission responses.
 * 
 * @param page - Playwright page object
 * @param role - Role configuration to authenticate as
 * @param targetUrl - URL to navigate to after authentication
 */
export async function authenticateAsRole(
  page: Page,
  role: RoleConfig,
  targetUrl: string
): Promise<void> {
  console.log(`[Role Auth] Authenticating as ${role.name} (${role.claims.email})...`);

  // Step 1: Clear existing state
  await page.context().clearCookies();

  // Step 2: Setup base API mocks
  await setupAPIMocks(page);

  // Step 3: Override user claims with role-specific claims
  await page.unroute(url => url.toString().includes('/user/claims'));
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    console.log(`[Role Mock] /user/claims -> ${role.name}`);
    const ROLE_CLAIM_TYPE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const claims = [
      { type: 'email', value: role.claims.email },
      { type: 'name', value: role.claims.name },
      { type: 'sub', value: role.claims.sub },
    ];
    // Add all roles with the correct claim type URI
    for (const r of role.claims.roles) {
      claims.push({ type: ROLE_CLAIM_TYPE, value: r });
    }
    if (role.claims.isInternal) {
      claims.push({ type: 'IsInternal', value: 'true' });
      claims.push({ type: 'IAPAuthenticated', value: 'true' });
    }
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(claims),
    });
  });

  // Step 4: Override /api/role/user to return role-specific data
  await setupUserRoleMock(page, role);

  // Step 5: Override route-level permission checks
  await setupRoutePermissionMocks(page, role);

  // Step 6: Override entity-level permission checks
  await setupEntityPermissionMocks(page, role);

  // Step 7: Set authentication cookies
  await page.context().addCookies([
    {
      name: 'dev-user-email',
      value: role.claims.email,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: false,
      secure: false,
      sameSite: 'Lax',
    },
    {
      name: 'DevIAPAuth',
      value: role.claims.email,
      domain: '127.0.0.1',
      path: '/',
      httpOnly: true,
      secure: false,
      sameSite: 'Lax',
    },
  ]);

  // Step 8: Navigate to target page
  const baseURL = 'http://localhost:4200';
  const fullUrl = targetUrl.startsWith('http') ? targetUrl : `${baseURL}${targetUrl}`;
  console.log(`[Role Auth] Navigating to ${fullUrl}...`);
  await page.goto(fullUrl);
  await page.waitForLoadState('load', { timeout: 15000 });
  await page.waitForTimeout(2000);
  console.log(`[Role Auth] ${role.name} authenticated and page loaded`);
}

/**
 * Setup /api/role/user mock to return the user's roles.
 * The sidebar uses this endpoint to determine which admin menu items to show.
 */
async function setupUserRoleMock(page: Page, role: RoleConfig): Promise<void> {
  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/role/user') && !urlString.includes('/api/role/all');
  }, async (route) => {
    console.log(`[Role Mock] /api/role/user -> ${role.name}`);

    // Build role list based on role config
    const roles: Array<{ name: string; isAdmin: boolean }> = [];
    for (const r of role.claims.roles) {
      const upperRole = r.toUpperCase();
      roles.push({
        name: r,
        isAdmin: upperRole === 'ADMINISTRATOR' || upperRole === 'PARTNER_GLOB_ADMIN' || upperRole === 'ORG_UNIT_ADMIN',
      });
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(roles),
    });
  });
}

/**
 * Setup route-level permission mocks based on role
 * Intercepts /api/permissions/check/{path} calls
 */
async function setupRoutePermissionMocks(page: Page, role: RoleConfig): Promise<void> {
  // Unroute existing catch-all that handles permissions
  // We'll add a more specific route handler BEFORE the catch-all

  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/permissions/check/');
  }, async (route) => {
    const url = route.request().url();
    const path = url.split('/api/permissions/check/')[1] || '';
    console.log(`[Role Mock] Permission check: ${path} -> ${role.name}`);

    let permissions: RoutePermissions;
    let entity = 'Unknown';

    // Determine which entity's permissions to return based on path
    if (path.includes('partner') && !path.includes('contact')) {
      permissions = role.partnerPermissions;
      entity = 'Partner';
    } else if (path.includes('contact')) {
      permissions = role.contactPermissions;
      entity = 'Contact';
    } else if (path.includes('interaction')) {
      permissions = role.interactionPermissions;
      entity = 'Interaction';
    } else if (path.includes('opportunit')) {
      permissions = role.opportunityPermissions;
      entity = 'Opportunity';
    } else if (path.includes('user-management')) {
      // User management - specific admin sub-route
      permissions = {
        canRead: role.canAccessUserManagement,
        canCreate: role.canAccessUserManagement,
        canUpdate: role.canAccessUserManagement,
        canDelete: role.canAccessUserManagement,
        canExport: role.canAccessUserManagement,
        canImport: role.canAccessUserManagement,
        canApprove: false,
        canActivate: false,
        canClose: false,
        canArchive: false,
      };
      entity = 'Admin';
    } else if (path.includes('ai-prompt')) {
      // AI prompts management - specific admin sub-route
      permissions = {
        canRead: role.canAccessAIPrompts,
        canCreate: role.canAccessAIPrompts,
        canUpdate: role.canAccessAIPrompts,
        canDelete: role.canAccessAIPrompts,
        canExport: role.canAccessAIPrompts,
        canImport: role.canAccessAIPrompts,
        canApprove: false,
        canActivate: false,
        canClose: false,
        canArchive: false,
      };
      entity = 'Admin';
    } else if (path.includes('entity-manager') || path.includes('entity-artifact')) {
      // Entity manager - specific admin sub-route
      permissions = {
        canRead: role.canAccessEntityManager,
        canCreate: role.canAccessEntityManager,
        canUpdate: role.canAccessEntityManager,
        canDelete: role.canAccessEntityManager,
        canExport: role.canAccessEntityManager,
        canImport: role.canAccessEntityManager,
        canApprove: false,
        canActivate: false,
        canClose: false,
        canArchive: false,
      };
      entity = 'Admin';
    } else if (path.includes('admin')) {
      // General admin routes
      permissions = {
        canRead: role.canAccessAdmin,
        canCreate: role.canAccessAdmin,
        canUpdate: role.canAccessAdmin,
        canDelete: role.canAccessAdmin,
        canExport: role.canAccessAdmin,
        canImport: role.canAccessAdmin,
        canApprove: false,
        canActivate: false,
        canClose: false,
        canArchive: false,
      };
      entity = 'Admin';
    } else {
      // Default - use partner permissions as fallback
      permissions = role.partnerPermissions;
      entity = 'Default';
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        hasAccess: permissions.canRead,
        route: url,
        entity: entity,
        permissions: permissions,
      }),
    });
  });
}

/**
 * Setup entity-level permission mocks based on role
 * Intercepts /api/{entity}/{id}/permissions calls
 */
async function setupEntityPermissionMocks(page: Page, role: RoleConfig): Promise<void> {
  // Partner entity permissions
  await page.unroute(url => /\/api\/partner\/\d+\/permissions/.test(url.toString()));
  await page.route(url => {
    return /\/api\/partner\/\d+\/permissions/.test(url.toString());
  }, async (route) => {
    console.log(`[Role Mock] Partner entity permissions -> ${role.name}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(role.partnerEntityPermissions),
    });
  });

  // Opportunity entity permissions
  await page.unroute(url => /\/api\/opportunity\/\d+\/permissions/.test(url.toString()));
  await page.route(url => {
    return /\/api\/opportunity\/\d+\/permissions/.test(url.toString());
  }, async (route) => {
    console.log(`[Role Mock] Opportunity entity permissions -> ${role.name}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(role.opportunityEntityPermissions),
    });
  });
}

// ============================================================
// TEST UTILITY FUNCTIONS
// ============================================================

/**
 * Check if a button-like element is visible on the page.
 * Tries multiple selector strategies for robustness.
 * 
 * @param page - Playwright page object
 * @param options - Search options (testId, text, or both)
 * @returns true if element is visible
 */
export async function isActionButtonVisible(
  page: Page,
  options: { testId?: string; text?: string | RegExp; ariaLabel?: string }
): Promise<boolean> {
  const selectors: string[] = [];

  if (options.testId) {
    selectors.push(`[data-testid="${options.testId}"]`);
  }
  if (options.ariaLabel) {
    selectors.push(`button[aria-label="${options.ariaLabel}"]`);
  }

  // Try testId and ariaLabel selectors first
  for (const selector of selectors) {
    const el = page.locator(selector);
    const visible = await el.isVisible().catch(() => false);
    if (visible) return true;
  }

  // Try text-based search (multiple strategies for robustness)
  if (options.text) {
    const textPattern = options.text instanceof RegExp ? options.text : new RegExp(options.text, 'i');
    // Strategy A: getByRole (most resilient for translated text)
    const roleBtn = page.getByRole('button', { name: textPattern }).first();
    if (await roleBtn.isVisible().catch(() => false)) return true;
    // Strategy B: locator with filter
    const btn = page.locator('button, p-button, a.p-button, [role="button"]').filter({ hasText: textPattern }).first();
    if (await btn.isVisible().catch(() => false)) return true;
    // Strategy C: link styled as button
    const linkBtn = page.getByRole('link', { name: textPattern }).first();
    if (await linkBtn.isVisible().catch(() => false)) return true;
  }

  return false;
}

/**
 * Wait for permissions to load after page navigation
 * @param page - Playwright page object
 */
export async function waitForRolePermissions(page: Page): Promise<void> {
  // Wait for permission API calls to complete
  await page.waitForTimeout(3000);
  // Wait for any loading overlays
  const overlay = page.locator('.bg-black.bg-opacity-50').first();
  const isOverlayVisible = await overlay.isVisible({ timeout: 1000 }).catch(() => false);
  if (isOverlayVisible) {
    await overlay.waitFor({ state: 'hidden', timeout: 10000 }).catch(() => {});
  }
}

/**
 * Check if user was redirected away from a page (access denied)
 * @param page - Playwright page object
 * @param expectedPath - The path the user should NOT be on (was denied)
 * @returns true if user was redirected (access denied working)
 */
export async function wasAccessDenied(page: Page, expectedPath: string): Promise<boolean> {
  const currentUrl = page.url();

  // Check if redirected to access-denied page
  if (currentUrl.includes('access-denied') || currentUrl.includes('unauthorized')) {
    return true;
  }

  // Check if Access Denied text is visible
  const accessDenied = page.getByText(/Access Denied|Unauthorized|Forbidden|Not Authorized/i).first();
  const denied = await accessDenied.isVisible().catch(() => false);
  if (denied) return true;

  // Check if redirected to a different page entirely (e.g., home)
  if (!currentUrl.includes(expectedPath)) {
    return true;
  }

  return false;
}

/**
 * Check if the sidebar/menu shows a specific navigation item.
 * Uses multiple selector strategies for PrimeNG menu components.
 * @param page - Playwright page object
 * @param menuText - Text of the menu item to find
 * @returns true if menu item is visible
 */
export async function isMenuItemVisible(
  page: Page,
  menuText: string | RegExp
): Promise<boolean> {
  // Strategy 1: PrimeNG menu item text class
  const primeMenuItem = page.locator('.p-menuitem-text').filter({ hasText: menuText }).first();
  if (await primeMenuItem.isVisible().catch(() => false)) return true;

  // Strategy 2: Any link/span in sidebar with matching text
  const sidebarItem = page.locator('app-sidebar a, app-sidebar span, app-menu a, app-menu span').filter({ hasText: menuText }).first();
  if (await sidebarItem.isVisible().catch(() => false)) return true;

  // Strategy 3: Any element in the navigation area with matching text  
  const navItem = page.locator('nav a, nav span, .sidebar a, .sidebar span, .layout-sidebar a, .layout-sidebar span').filter({ hasText: menuText }).first();
  if (await navItem.isVisible().catch(() => false)) return true;

  // Strategy 4: Broader search - any element on page matching menu text (as fallback)
  const anyMatch = page.locator('li, a').filter({ hasText: menuText }).first();
  if (await anyMatch.isVisible().catch(() => false)) return true;

  return false;
}
