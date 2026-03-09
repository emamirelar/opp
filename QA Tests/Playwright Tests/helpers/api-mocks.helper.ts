/**
 * @fileoverview API Mocking Helper
 * Provides route mocking for backend APIs during E2E tests
 * 
 * QA-041 FIX: Added DEBUG_MOCKS flag to control verbose logging.
 * Previously every mock call logged to console, generating ~30 lines per test x 600+ tests
 * = 18,000+ log lines that consumed Node.js heap memory and caused crashes after ~287 tests.
 */

import { Page } from '@playwright/test';
import referenceData from '../fixtures/reference-data.json';
import partnersFixture from '../fixtures/partners.json';
import contactsFixture from '../fixtures/contacts.json';
import opportunitiesFixture from '../fixtures/opportunities.json';
import dashboardFixture from '../fixtures/dashboard.json';
import interactionsFixture from '../fixtures/interactions.json';

/**
 * Set to true to enable verbose mock logging (useful for debugging individual tests).
 * Set to false for full suite runs to prevent memory exhaustion from console output.
 * Can also be controlled via PLAYWRIGHT_DEBUG_MOCKS=true environment variable.
 */
const DEBUG_MOCKS = process.env.PLAYWRIGHT_DEBUG_MOCKS === 'true';

/** Conditional logger that only outputs when DEBUG_MOCKS is enabled */
function mockLog(message: string): void {
  if (DEBUG_MOCKS) {
    console.log(message);
  }
}

/**
 * List of restricted user emails that should receive view-only permissions.
 * Used by permission mocks to differentiate admin vs restricted user responses.
 */
const RESTRICTED_MOCK_USERS = [
  'test-readonly@playwright.local',
  'test-no-permissions@playwright.local',
  'viewer@example.com',
  'doa2@example.com',
  'collaborator@example.com',
  'other-user@example.com',
  'partner.user@test.local',
  'general.user@test.local',
];

/** Go Decision workflow stages for opportunity */
const OPPORTUNITY_STAGES = {
  IDENTIFY_PROFILE: 'IDENTIFY & PROFILE',
  GO: 'GO',
  NO_GO: 'NO GO',
  CANCELLED: 'CANCELLED',
};

/** In-memory state for workflow mocks (enables Cancel/Reopen/Submit transitions in tests) */
let workflowMockState: Record<number, { stage: string; status: string; isInWorkflow: boolean }> = {};

/**
 * Reset the workflow mock state. Call in beforeEach to ensure test isolation.
 */
export function resetWorkflowMockState(): void {
  workflowMockState = {};
}

/**
 * Setup API mocks for authentication and configuration.
 * @param page - Playwright page object
 * @param userEmail - Optional user email to customize permission responses.
 *   Restricted users receive view-only permissions (canEdit: false, canDelete: false).
 *   Default/admin users receive full permissions.
 */
export async function setupAPIMocks(page: Page, userEmail?: string): Promise<void> {
  // Reset workflow state for each test to ensure isolation
  workflowMockState = {};

  const isRestrictedUser = userEmail ? RESTRICTED_MOCK_USERS.includes(userEmail) : false;
  mockLog('[API Mock] Setting up route interceptions...');
  
  // Mock /api/configuration endpoint
  await page.route(url => url.toString().includes('/api/configuration'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/configuration');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        // Mock configuration data
        appName: 'Opportunity+',
        version: '1.0.0',
        environment: 'test',
        // Mock Google API credentials to suppress console errors
        googleClientId: 'mock-google-client-id-for-testing',
        googleApiKey: 'mock-google-api-key-for-testing',
      }),
    });
  });

  // Mock /user/claims endpoint - Return empty array (not authenticated)
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    mockLog('[API Mock] Intercepted: /user/claims');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]), // Empty array = not authenticated
    });
  });

  // Mock /api/global/preferred-language endpoint
  await page.route(url => url.toString().includes('/api/global/preferred-language'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/global/preferred-language');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ language: 'en' }), // Default to English
    });
  });

  // Mock /user/login endpoint - Authentication endpoint (must be before catch-all)
  await page.route(url => url.toString().includes('/user/login'), async (route) => {
    mockLog('[API Mock] Intercepted: /user/login (authentication)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
      // Set authentication cookie
      headers: {
        'Set-Cookie': 'dev-user-email=test@unops.org; Path=/; HttpOnly'
      }
    });
  });

  // Mock /user/register endpoint
  await page.route(url => url.toString().includes('/user/register'), async (route) => {
    mockLog('[API Mock] Intercepted: /user/register');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock /user/googleSignIn endpoint
  await page.route(url => url.toString().includes('/user/googleSignIn'), async (route) => {
    mockLog('[API Mock] Intercepted: /user/googleSignIn');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // ==========================================
  // FORM DATA ENDPOINTS - Required for dialog rendering
  // ==========================================
  
  // Mock /api/values/partners - Partners dropdown
  await page.route(url => url.toString().includes('/api/values/partners'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/partners');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.partners),
    });
  });

  // Mock /api/values/organization-units - Organization units dropdown
  await page.route(url => url.toString().includes('/api/values/organization-units'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/organization-units');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.organizationUnits),
    });
  });

  // Mock /api/partner-tree-structure - Hierarchical partner structure
  await page.route(url => url.toString().includes('/api/partner-tree-structure'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/partner-tree-structure');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.partnerTreeStructure),
    });
  });

  // Mock /api/values/liaison-offices - Liaison offices dropdown
  await page.route(url => url.toString().includes('/api/values/liaison-offices'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/liaison-offices');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.liaisonOffices),
    });
  });

  // Mock /api/values/contacts - Contacts dropdown
  await page.route(url => url.toString().includes('/api/values/contacts'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/contacts');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.contacts),
    });
  });

  // Mock /api/values/users/paged - Users paged endpoint (POST)
  await page.route(url => url.toString().includes('/api/values/users/paged'), async (route) => {
    mockLog('[API Mock] Intercepted: POST /api/values/users/paged');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        items: referenceData.users,
        totalCount: referenceData.users.length,
        pageIndex: 1,
        pageSize: 20,
      }),
    });
  });

  // ==========================================
  // REFERENCE DATA ENDPOINTS - Required for CachedDataService
  // These are loaded globally on app init for dropdown options
  // ==========================================
  
  // Mock /api/values/salutations - Salutation dropdown (Mr., Ms., Dr., etc.)
  await page.route(url => url.toString().includes('/api/values/salutations'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/salutations');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.salutations),
    });
  });

  // Mock /api/values/status - Status dropdown (Active, Inactive, etc.)
  await page.route(url => url.toString().includes('/api/values/status'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/status');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.statuses),
    });
  });

  // Mock /api/values/pronouns - Pronouns dropdown
  await page.route(url => url.toString().includes('/api/values/pronouns'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/pronouns');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.pronouns),
    });
  });

  // Mock /api/values/countries - Countries dropdown
  await page.route(url => url.toString().includes('/api/values/countries'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/countries');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.countries),
    });
  });

  // Mock /api/values/states - States/Provinces dropdown
  await page.route(url => url.toString().includes('/api/values/states'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/states');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.states),
    });
  });

  // ==========================================
  // ENTITY LIST ENDPOINTS - Required for listview rendering
  // ==========================================

  // Mock /api/partner (list) - Partner list data
  await page.route(url => {
    const urlString = url.toString();
    // Match /api/partner with optional query params, but NOT /api/partner/{id} or /api/partner-tree-structure
    return /\/api\/partner(\?|$)/.test(urlString) && 
           !urlString.includes('/api/partner-tree-structure') &&
           !urlString.includes('/api/partner/');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/partner (list)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(partnersFixture.list),
    });
  });

  // Mock /api/partner/search - Partner search endpoint
  await page.route(url => /\/api\/partner\/search/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/partner/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(partnersFixture.search),
    });
  });

  // Mock /api/contact (list) - Contact list data
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/contact(\?|$)/.test(urlString) && !urlString.includes('/api/contact/');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/contact (list)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(contactsFixture.list),
    });
  });

  // Mock /api/contact/search - Contact search endpoint
  await page.route(url => /\/api\/contact\/search/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/contact/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(contactsFixture.search),
    });
  });

  // Mock /api/interaction or /api/interactions (list) - Interaction list data
  // The Angular app uses /api/interactions (plural) for the list endpoint
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/interactions?(\?|$)/.test(urlString) && !urlString.includes('/api/interaction/');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/interaction(s) (list)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(interactionsFixture.list),
    });
  });

  // Mock /api/interaction/search - Interaction search endpoint
  await page.route(url => /\/api\/interaction\/search/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/interaction/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(interactionsFixture.search),
    });
  });

  // Mock /api/opportunity (list) - Opportunity list data
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/opportunity(\?|$)/.test(urlString) && !urlString.includes('/api/opportunity/');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/opportunity (list)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(opportunitiesFixture.list),
    });
  });

  // Mock /api/opportunity/search - Opportunity search endpoint
  await page.route(url => /\/api\/opportunity\/search/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: GET /api/opportunity/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(opportunitiesFixture.search),
    });
  });

  // ==========================================
  // ENTITY DETAIL ENDPOINTS - Required for detail pages
  // ==========================================
  
  // Mock /api/partner/{id} - Partner detail
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/partner\/\d+$/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const partnerId = url.match(/\/api\/partner\/(\d+)/)?.[1] || '1';
    mockLog(`[API Mock] Intercepted: /api/partner/${partnerId}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: parseInt(partnerId),
        name: 'Test Partner Organization',
        type: 'Organization',
        status: 'Active',
        description: 'This is a test partner for automated E2E testing',
        website: 'https://test-partner.org',
        email: 'contact@test-partner.org',
        phone: '+1-555-0123',
        address: '123 Test Street, Test City, TC 12345',
        country: 'United States',
        partnerType: { id: 1, name: 'Government' },
        stage: 'Active',
        workflowStatus: 'Active',
        createdDate: '2024-01-01T00:00:00Z',
        lastModifiedDate: '2024-06-15T12:00:00Z',
        createdBy: 'system',
        lastModifiedBy: 'system',
        permissions: isRestrictedUser ? { canView: true, canEdit: false, canUpdate: false, canDelete: false } : { canView: true, canEdit: true, canUpdate: true, canDelete: true },
        // Tab configuration data
        contacts: [
          { id: 1, firstName: 'John', lastName: 'Smith', email: 'john@test.com' },
          { id: 2, firstName: 'Jane', lastName: 'Doe', email: 'jane@test.com' }
        ],
        interactions: [],
        opportunities: [],
        documents: [],
      }),
    });
  });

  // Mock /api/partner/{id}/permissions - Partner permissions
  // Returns restricted permissions for restricted users, full permissions for admin
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/partner\/\d+\/permissions/.test(urlString);
  }, async (route) => {
    mockLog(`[API Mock] Intercepted: /api/partner/{id}/permissions (restricted=${isRestrictedUser})`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(isRestrictedUser ? {
        canView: true,
        canEdit: false,
        canDelete: false,
        canSubmit: false,
        canApprove: false,
        canActivate: false,
        canCancel: false,
      } : {
        canView: true,
        canEdit: true,
        canDelete: true,
        canSubmit: true,
        canApprove: false,
        canActivate: true,
        canCancel: false,
      }),
    });
  });

  // Mock /api/opportunity/{id} - Opportunity detail (comprehensive mock with Go Decision stages)
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/opportunity\/\d+$/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const opportunityId = url.match(/\/api\/opportunity\/(\d+)/)?.[1] || '1';
    const id = parseInt(opportunityId);
    mockLog(`[API Mock] Intercepted: /api/opportunity/${opportunityId}`);

    // Use workflowMockState if updated by POST, else default by ID
    const stored = workflowMockState[id];
    let stageByIdRange = stored?.stage ?? OPPORTUNITY_STAGES.IDENTIFY_PROFILE;
    let statusByIdRange = stored?.status ?? 'Draft';
    if (!stored) {
      if (id === 10) {
        stageByIdRange = OPPORTUNITY_STAGES.CANCELLED;
        statusByIdRange = 'Closed';
      } else if (id === 11) {
        stageByIdRange = OPPORTUNITY_STAGES.NO_GO;
        statusByIdRange = 'Closed';
      } else if (id === 12) {
        stageByIdRange = OPPORTUNITY_STAGES.IDENTIFY_PROFILE;
        statusByIdRange = 'Active';
      }
    }
    
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: id,
        name: `Test Opportunity ${id}`,
        title: `Test Opportunity ${id}`,
        description: 'This is a comprehensive test opportunity for automated E2E testing. It covers infrastructure development and capacity building initiatives.',
        status: statusByIdRange,
        stage: stageByIdRange,
        workflowStatus: stageByIdRange,
        value: 1500000,
        currency: 'USD',
        estimatedValue: 1500000,
        probability: 75,
        expectedCloseDate: '2026-12-31T00:00:00Z',
        startDate: '2026-01-01T00:00:00Z',
        endDate: '2026-12-31T00:00:00Z',
        createdDate: '2025-01-01T00:00:00Z',
        lastModifiedDate: '2025-06-15T12:00:00Z',
        createdBy: 'system',
        lastModifiedBy: 'system',
        partner: { id: 1, name: 'UNICEF Regional Office' },
        organizationUnit: { id: 1, name: 'HQ - Headquarters', code: 'HQ' },
        opportunityType: { id: 1, name: 'New Business' },
        sector: { id: 1, name: 'Infrastructure' },
        country: 'United States',
        region: 'North America',
        // Team section data
        opportunityManager: { id: 1, name: 'Test User', email: 'test@unops.org', position: 'Programme Manager' },
        collaborators: [
          { id: 1, userId: 1, name: 'Jane Doe', expertise: ['Project Management', 'Technical Expertise'] },
          { id: 2, userId: 2, name: 'Bob Johnson', expertise: ['Financial Management'] },
        ],
        stakeholders: [
          { id: 1, userId: 1, role: 'Opportunity Manager', name: 'Test User' },
          { id: 2, userId: 2, role: 'Collaborator', name: 'Jane Doe' },
        ],
        // WHY section data
        sdgs: [
          { id: 1, name: 'No Poverty', number: 1, isPrimary: true },
          { id: 4, name: 'Quality Education', number: 4, isPrimary: false },
          { id: 13, name: 'Climate Action', number: 13, isPrimary: false },
        ],
        beneficiaryCount: 50000,
        beneficiaryBreakdown: { women: 25000, men: 20000, children: 5000 },
        unCooperationFramework: { id: 1, name: 'UN Sustainable Development Cooperation Framework' },
        highRiskChecklist: [],
        // WHAT section data
        scope: 'Comprehensive project scope covering infrastructure development, capacity building, and knowledge transfer across 5 countries.',
        deliverables: [
          { id: 1, name: 'Training Program', description: 'Staff training across all regions', date: '2026-06-30T00:00:00Z' },
          { id: 2, name: 'Infrastructure Assessment', description: 'Assessment of current facilities', date: '2026-03-31T00:00:00Z' },
        ],
        initiativeType: { id: 1, name: 'Technical Assistance' },
        // Related entities
        contacts: [
          { id: 1, firstName: 'John', lastName: 'Smith', email: 'john@test.com' },
          { id: 2, firstName: 'Jane', lastName: 'Doe', email: 'jane@test.com' },
        ],
        interactions: [
          { id: 1, subject: 'Initial Meeting', date: '2025-01-15T10:00:00Z' },
          { id: 2, subject: 'Follow-up Discussion', date: '2025-02-20T14:00:00Z' },
        ],
        documents: [],
        risks: [
          { id: 1, name: 'Budget Overrun', category: 'Financial', likelihood: 'Medium', impact: 'High' },
        ],
      }),
    });
  });

  // Mock /api/opportunity/{id}/permissions - Opportunity permissions
  // Collaborator: canEdit content but NOT workflow (canSubmit, canCancel)
  const isCollaboratorUser = userEmail === 'collaborator@example.com';
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/opportunity\/\d+\/permissions/.test(urlString);
  }, async (route) => {
    mockLog(`[API Mock] Intercepted: /api/opportunity/{id}/permissions (restricted=${isRestrictedUser}, collaborator=${isCollaboratorUser})`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(isCollaboratorUser ? {
        canView: true,
        canEdit: true,
        canDelete: false,
        canSubmit: false,
        canApprove: false,
        canActivate: false,
        canCancel: false,
      } : isRestrictedUser ? {
        canView: true,
        canEdit: false,
        canDelete: false,
        canSubmit: false,
        canApprove: false,
        canActivate: false,
        canCancel: false,
      } : {
        canView: true,
        canEdit: true,
        canDelete: false,
        canSubmit: true,
        canApprove: false,
        canActivate: true,
        canCancel: false,
      }),
    });
  });

  // Mock /api/contact/{id} - Contact detail
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/contact\/\d+$/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const contactId = url.match(/\/api\/contact\/(\d+)/)?.[1] || '1';
    mockLog(`[API Mock] Intercepted: /api/contact/${contactId}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: parseInt(contactId),
        firstName: 'John',
        lastName: 'Smith',
        name: 'John Smith',
        email: 'john.smith@test.com',
        phone: '+1-555-0123',
        title: 'Director',
        department: 'Partnerships',
        status: 'Active',
        partner: { id: 1, name: 'Test Partner Organization' },
        permissions: isRestrictedUser ? { canView: true, canEdit: false, canUpdate: false, canDelete: false } : { canView: true, canEdit: true, canUpdate: true, canDelete: true },
        createdDate: '2024-01-01T00:00:00Z',
        lastModifiedDate: '2024-06-15T12:00:00Z',
      }),
    });
  });

  // Mock /api/interaction/{id} - Interaction detail
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/interaction\/\d+$/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const interactionId = url.match(/\/api\/interaction\/(\d+)/)?.[1] || '1';
    mockLog(`[API Mock] Intercepted: /api/interaction/${interactionId}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: parseInt(interactionId),
        subject: 'Test Interaction',
        description: 'This is a test interaction for E2E testing',
        type: 'Meeting',
        date: '2024-06-15T10:00:00Z',
        duration: 60,
        status: 'Completed',
        partner: { id: 1, name: 'Test Partner Organization' },
        contacts: [
          { id: 1, firstName: 'John', lastName: 'Smith', email: 'john@test.com' }
        ],
        createdDate: '2024-01-01T00:00:00Z',
        lastModifiedDate: '2024-06-15T12:00:00Z',
      }),
    });
  });

  // Mock /api/contact/{id}/permissions - Contact permissions (required for contact detail page)
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/contact\/\d+\/permissions/.test(urlString);
  }, async (route) => {
    mockLog('[API Mock] Intercepted: /api/contact/{id}/permissions');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(isRestrictedUser ? {
        canView: true,
        canEdit: false,
        canDelete: false,
      } : {
        canView: true,
        canEdit: true,
        canDelete: true,
      }),
    });
  });

  // Mock /api/interaction/{id}/permissions - Interaction permissions (required for interaction detail page)
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/interaction\/\d+\/permissions/.test(urlString);
  }, async (route) => {
    mockLog('[API Mock] Intercepted: /api/interaction/{id}/permissions');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(isRestrictedUser ? {
        canView: true,
        canEdit: false,
        canDelete: false,
      } : {
        canView: true,
        canEdit: true,
        canDelete: true,
      }),
    });
  });

  // ==========================================
  // DASHBOARD ENDPOINTS - Required for home/dashboard page
  // ==========================================

  // Mock /api/dashboard/content - Combined dashboard data (DashboardCombinedResponse)
  // Must match DashboardCombinedResponse interface: myPartners, myContacts, myInteractions,
  // myOpportunities, draftPartners, draftContacts, draftInteractions, draftOpportunities,
  // orgUnitRecentUpdates, orgUnitName
  await page.route(url => url.toString().includes('/api/dashboard/content'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/dashboard/content');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(dashboardFixture.content),
    });
  });

  // Mock /api/dashboard/org-unit-recent-updates - Used when loading "View All" recent activity
  await page.route(url => url.toString().includes('/api/dashboard/org-unit-recent-updates'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/dashboard/org-unit-recent-updates');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(dashboardFixture.orgUnitRecentUpdates),
    });
  });

  // Mock /api/global/search - Cross-entity search (SearchResponse: availableEntities, results)
  await page.route(url => url.toString().includes('/api/global/search'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/global/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        availableEntities: ['partners', 'contacts', 'interactions', 'opportunities'],
        results: {
          partners: [],
          contacts: [],
          interactions: [],
          opportunities: [],
        },
      }),
    });
  });

  // ==========================================
  // NOTIFICATION ENDPOINTS - Required for topbar notification panel
  // ==========================================

  // Mock GET /api/notifications - List notifications (unread and all)
  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/notifications') &&
           !urlString.match(/\/api\/notifications\/\d+\//);
  }, async (route) => {
    const request = route.request();
    const url = request.url();

    if (request.method() === 'GET') {
      mockLog('[API Mock] Intercepted: GET /api/notifications');
      // Return sample notifications for panel rendering (empty state also valid)
      const mockNotifications = [
        {
          id: 1,
          message: 'Test notification - Opportunity review requested',
          category: 'workflow_approval',
          responseType: 'Pending',
          entity: 'Opportunity',
          entityId: 1,
          status: 'Pending',
          isRead: false,
          createdAt: new Date().toISOString(),
        },
        {
          id: 2,
          message: 'Partner document uploaded successfully',
          category: 'document',
          responseType: 'Done',
          status: 'Done',
          isRead: true,
          createdAt: new Date(Date.now() - 86400000).toISOString(),
        },
      ];
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(mockNotifications),
      });
    } else if (request.method() === 'PUT') {
      mockLog('[API Mock] Intercepted: PUT /api/notifications (mark read or update)');
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      });
    } else {
      await route.continue();
    }
  });

  // Mock PUT /api/notifications/{id}/read and /api/notifications/{id}/update
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/notifications\/\d+\/(read|update)/.test(urlString);
  }, async (route) => {
    mockLog('[API Mock] Intercepted: PUT /api/notifications/{id}/read or update');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({}),
    });
  });

  // ==========================================
  // WORKFLOW AND STAGE ENDPOINTS (Go Decision)
  // ==========================================

  const isCollaborator = userEmail === 'collaborator@example.com';

  // Mock GET /api/workflow/pending-approvals - Pending workflow approvals for Actions Required card
  await page.route(url => url.toString().includes('/api/workflow/pending-approvals'), async (route) => {
    if (route.request().method() !== 'GET') {
      await route.continue();
      return;
    }
    mockLog('[API Mock] Intercepted: GET /api/workflow/pending-approvals');
    // Return mock pending approvals for DoA2 user (used by workflow-actions-required.spec.ts)
    const mockPendingApprovals = [
      {
        entityName: 'Opportunity',
        entityId: 12,
        entityDisplayName: 'Healthcare Capacity Building - Go Decision Pending',
        currentStage: 'IDENTIFY & PROFILE',
        pendingStage: 'GO',
        submittedBy: 'Test OM',
        submittedOn: new Date().toISOString(),
        orgUnitName: 'HQ - Headquarters',
        submissionComment: 'Ready for review',
      },
    ];
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(mockPendingApprovals),
    });
  });

  // Mock /api/workflow/{entity} - Workflow stages list (no /id)
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/workflow\/\w+(\?|$)/.test(urlString) && !/\/api\/workflow\/\w+\/\d+/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const entityMatch = url.match(/\/api\/workflow\/(\w+)/);
    const entityName = entityMatch?.[1] || 'opportunity';
    mockLog(`[API Mock] Intercepted: /api/workflow/${entityName}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { stageCode: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, displayName: 'Identify & Profile', sequence: 1 },
        { stageCode: OPPORTUNITY_STAGES.GO, displayName: 'GO', sequence: 2 },
        { stageCode: OPPORTUNITY_STAGES.NO_GO, displayName: 'NO GO', sequence: 3 },
        { stageCode: OPPORTUNITY_STAGES.CANCELLED, displayName: 'Cancelled', sequence: 4 },
      ]),
    });
  });

  // Helper to get workflow state (uses in-memory state for state transitions)
  const getWorkflowState = (id: number) => {
    const stored = workflowMockState[id];
    if (stored) return stored;
    if (id === 10) return { stage: OPPORTUNITY_STAGES.CANCELLED, status: 'Closed', isInWorkflow: false };
    if (id === 11) return { stage: OPPORTUNITY_STAGES.NO_GO, status: 'Closed', isInWorkflow: false };
    if (id === 12) return { stage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, status: 'Active', isInWorkflow: true };
    return { stage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, status: 'Draft', isInWorkflow: false };
  };

  // Mock /api/workflow/{entity}/{id} - Workflow state (ID-aware for opportunity)
  await page.route(url => {
    const urlString = url.toString();
    return /\/api\/workflow\/\w+\/\d+$/.test(urlString);
  }, async (route) => {
    const url = route.request().url();
    const match = url.match(/\/api\/workflow\/\w+\/(\d+)/);
    const id = match ? parseInt(match[1], 10) : 1;
    const entityMatch = url.match(/\/api\/workflow\/(\w+)\//);
    const entityName = entityMatch?.[1] || 'opportunity';
    mockLog(`[API Mock] Intercepted: /api/workflow/${entityName}/${id}`);

    const { stage: currentStage, isInWorkflow } = getWorkflowState(id);

    // Collaborator: no workflow actions
    const nextActions = isCollaborator ? [] : (
      currentStage === OPPORTUNITY_STAGES.IDENTIFY_PROFILE && !isInWorkflow
        ? [
            { actionName: 'Submit for Go', newStage: OPPORTUNITY_STAGES.GO, sequence: 1, comment: 'mandatory', requiresApproval: true },
            { actionName: 'Cancel', newStage: OPPORTUNITY_STAGES.CANCELLED, sequence: 2, comment: 'mandatory', requiresApproval: false },
          ]
        : currentStage === OPPORTUNITY_STAGES.CANCELLED || currentStage === OPPORTUNITY_STAGES.NO_GO
          ? [{ actionName: 'Reopen', newStage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, sequence: 1, comment: 'mandatory', requiresApproval: false }]
          : isInWorkflow
            ? [
                { actionName: 'Recall', newStage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, sequence: 1, comment: 'mandatory', requiresApproval: false },
                { actionName: 'Approve', newStage: OPPORTUNITY_STAGES.GO, sequence: 2, comment: 'optional', requiresApproval: false },
                { actionName: 'Reject', newStage: OPPORTUNITY_STAGES.NO_GO, sequence: 3, comment: 'mandatory', requiresApproval: false },
              ]
            : []
    );

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        stage: currentStage,
        displayName: currentStage,
        comment: '',
        nextActions,
        isInWorkflow,
      }),
    });
  });

  // Mock /api/workflow/{entity}/{id}/details - Workflow details (for in-workflow)
  await page.route(url => url.toString().includes('/api/workflow/') && url.toString().includes('/details'), async (route) => {
    const url = route.request().url();
    const match = url.match(/\/api\/workflow\/\w+\/(\d+)/);
    const id = match ? parseInt(match[1], 10) : 1;
    mockLog(`[API Mock] Intercepted: /api/workflow/.../details`);
    const isInWorkflow = id === 12;
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        currentStage: isInWorkflow ? OPPORTUNITY_STAGES.IDENTIFY_PROFILE : OPPORTUNITY_STAGES.CANCELLED,
        canRecall: isInWorkflow && !isCollaborator,
        recallComment: 'mandatory',
        canApprove: isInWorkflow && !isCollaborator,
        approvalComment: 'optional',
        canReject: isInWorkflow && !isCollaborator,
        rejectionComment: 'mandatory',
        approvers: [],
      }),
    });
  });

  // Mock /api/workflow/{entity}/{id}/requirements - Stage requirements (ID 2 = missing statement)
  await page.route(url => url.toString().includes('/api/workflow/') && url.toString().includes('/requirements'), async (route) => {
    const url = route.request().url();
    const match = url.match(/\/api\/workflow\/\w+\/(\d+)/);
    const id = match ? parseInt(match[1], 10) : 1;
    mockLog(`[API Mock] Intercepted: /api/workflow/.../requirements`);
    const unmetForId2 = id === 2
      ? [{ message: 'Opportunity Statement has not yet been generated', requirementType: 'OpportunityStatement' }]
      : [];
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(unmetForId2),
    });
  });

  // Mock /api/workflow/{entity}/{id}/history - Workflow history
  await page.route(url => url.toString().includes('/api/workflow/') && url.toString().includes('/history'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/workflow/.../history');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { fromStage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, toStage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, action: 'Created', comment: '', performedOn: '2025-01-01T00:00:00Z' },
      ]),
    });
  });

  // Mock POST /api/workflow/submit, cancel, reopen, recall - Workflow actions (stateful)
  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/workflow/submit') || urlString.includes('/api/workflow/cancel') ||
      urlString.includes('/api/workflow/reopen') || urlString.includes('/api/workflow/recall') ||
      urlString.includes('/api/workflow/approve') || urlString.includes('/api/workflow/reject');
  }, async (route) => {
    const url = route.request().url();
    const method = route.request().method();
    mockLog(`[API Mock] Intercepted: ${method} ${url}`);
    if (method !== 'POST') {
      await route.continue();
      return;
    }
    let newStage = OPPORTUNITY_STAGES.GO;
    let entityId = 1;
    try {
      const body = route.request().postDataJSON();
      entityId = body?.entityId ?? body?.EntityId ?? 1;
      // ID 2 = missing Opportunity Statement — return requirements not met for TC-020
      if (url.includes('/submit') && entityId === 2) {
        await route.fulfill({
          status: 400,
          contentType: 'application/json',
          body: JSON.stringify({
            success: false,
            requirementsNotMet: true,
            unmetRequirements: ['Opportunity Statement has not yet been generated'],
            errorMessage: 'Opportunity Statement has not yet been generated',
          }),
        });
        return;
      }
      if (url.includes('/cancel')) {
        newStage = OPPORTUNITY_STAGES.CANCELLED;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.CANCELLED, status: 'Closed', isInWorkflow: false };
      } else if (url.includes('/reopen')) {
        newStage = OPPORTUNITY_STAGES.IDENTIFY_PROFILE;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, status: 'Draft', isInWorkflow: false };
      } else if (url.includes('/recall')) {
        newStage = OPPORTUNITY_STAGES.IDENTIFY_PROFILE;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, status: 'Draft', isInWorkflow: false };
      } else if (url.includes('/submit')) {
        newStage = OPPORTUNITY_STAGES.GO;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.IDENTIFY_PROFILE, status: 'Active', isInWorkflow: true };
      } else if (url.includes('/approve')) {
        newStage = OPPORTUNITY_STAGES.GO;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.GO, status: 'Active', isInWorkflow: false };
      } else if (url.includes('/reject')) {
        newStage = OPPORTUNITY_STAGES.NO_GO;
        workflowMockState[entityId] = { stage: OPPORTUNITY_STAGES.NO_GO, status: 'Closed', isInWorkflow: false };
      }
    } catch {
      // Ignore parse errors
    }
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        success: true,
        newStage,
        stage: newStage,
        displayName: newStage,
        nextActions: [],
        isInWorkflow: false,
      }),
    });
  });

  // ==========================================
  // DOCUMENT ENDPOINTS - Required for document tabs and AI comparison
  // ==========================================

  // Mock /api/document/entity/{entityType}/{entityId} - Document list for entity
  await page.route(url => /\/api\/document\/entity\/\w+\/\d+/.test(url.toString()), async (route) => {
    const url = route.request().url();
    const match = url.match(/\/api\/document\/entity\/(\w+)\/(\d+)/);
    const entityType = match?.[1] || 'Partner';
    const entityId = match?.[2] || '1';
    mockLog(`[API Mock] Intercepted: /api/document/entity/${entityType}/${entityId}`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 1, name: 'Partnership Agreement.pdf', type: 'Contract', size: 245000, createdDate: '2024-06-01T00:00:00Z', aiTranscribed: true, mimeType: 'application/pdf' },
        { id: 2, name: 'Meeting Notes.docx', type: 'Report', size: 52000, createdDate: '2024-06-10T00:00:00Z', aiTranscribed: false, mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' },
      ]),
    });
  });

  // Mock /api/document-transcribe (POST) - AI document transcription
  await page.route(url => url.toString().includes('/api/document-transcribe'), async (route) => {
    mockLog('[API Mock] Intercepted: POST /api/document-transcribe');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        name: 'Transcribed Document',
        description: 'AI-extracted description from document content',
        estimatedValue: 1500000,
        currency: 'USD',
        sector: 'Infrastructure',
        country: 'Kenya',
        transcriptionStatus: 'completed',
      }),
    });
  });

  // Mock /api/auditlog/latest - Latest audit log entry (used by AI comparison)
  await page.route(url => url.toString().includes('/api/auditlog/latest'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/auditlog/latest');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 1,
        entityId: 1,
        entityType: 'Opportunity',
        action: 'Update',
        jsonData: JSON.stringify({
          name: 'Infrastructure Development Program',
          description: 'Current description before AI changes',
          estimatedValue: 1500000,
        }),
        createdDate: new Date().toISOString(),
        createdBy: 'system',
      }),
    });
  });

  // Mock /api/auditlog - Audit log list
  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/auditlog') && !urlString.includes('/api/auditlog/latest');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: /api/auditlog');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    });
  });

  // ==========================================
  // ADDITIONAL REFERENCE DATA - CachedDataService and form dropdowns
  // ==========================================

  // Mock /api/values/partner-groups - Partner group dropdown
  await page.route(url => url.toString().includes('/api/values/partner-groups'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/partner-groups');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.partnerGroups),
    });
  });

  // Mock /api/values/users/search - User search for DOA/team dialogs
  await page.route(url => url.toString().includes('/api/values/users/search'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/users/search');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.usersSearch),
    });
  });

  // Mock /api/values/sdg - Sustainable Development Goals
  await page.route(url => {
    const urlString = url.toString();
    return urlString.includes('/api/values/sdg') && !urlString.includes('/api/values/sdg-indicators');
  }, async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/sdg');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.sdgs),
    });
  });

  // Mock /api/values/sdg-indicators - SDG Indicators
  await page.route(url => url.toString().includes('/api/values/sdg-indicators'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/sdg-indicators');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.sdgIndicators),
    });
  });

  // Mock /api/values/currency - Currency dropdown
  await page.route(url => url.toString().includes('/api/values/currency'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/currency');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.currencies),
    });
  });

  // Mock /api/partner/{id}/interactions - Partner interactions for Create Opportunity
  await page.route(url => /\/api\/partner\/\d+\/interactions/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: /api/partner/{id}/interactions');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(interactionsFixture.partnerInteractions),
    });
  });

  // Mock /api/interactions-brief - Brief interaction list for Create Opportunity dialog
  await page.route(url => url.toString().includes('/api/interactions-brief'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/interactions-brief');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(interactionsFixture.interactionsBrief),
    });
  });

  // Mock /api/values/gemini-models - AI model selection
  await page.route(url => url.toString().includes('/api/values/gemini-models'), async (route) => {
    mockLog('[API Mock] Intercepted: /api/values/gemini-models');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(referenceData.geminiModels),
    });
  });

  // Mock /api/opportunity/{id}/apply-ai-changes (POST) - Apply AI-suggested changes
  await page.route(url => /\/api\/opportunity\/\d+\/apply-ai-changes/.test(url.toString()), async (route) => {
    const url = route.request().url();
    const id = url.match(/\/api\/opportunity\/(\d+)/)?.[1] || '1';
    mockLog(`[API Mock] Intercepted: POST /api/opportunity/${id}/apply-ai-changes`);
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: parseInt(id),
        name: 'Updated Opportunity',
        success: true,
      }),
    });
  });

  // Mock /api/contact/{id}/profile-picture - Contact profile picture
  await page.route(url => /\/api\/contact\/\d+\/profile-picture/.test(url.toString()), async (route) => {
    mockLog('[API Mock] Intercepted: /api/contact/{id}/profile-picture');
    await route.fulfill({
      status: 404,
      contentType: 'application/json',
      body: JSON.stringify({ message: 'No profile picture' }),
    });
  });

  // Catch-all for any other /api/ and /user/ calls - return smart defaults based on URL pattern
  await page.route(url => {
    const urlString = url.toString();
    return (urlString.includes('/api/') || urlString.includes('/user/')) && 
           !urlString.includes('/api/configuration') &&
           !urlString.includes('/user/claims') &&
           !urlString.includes('/user/login') &&
           !urlString.includes('/user/register') &&
           !urlString.includes('/user/googleSignIn') &&
           !urlString.includes('/api/global/preferred-language') &&
           !urlString.includes('/api/values/partners') &&
           !urlString.includes('/api/values/organization-units') &&
           !urlString.includes('/api/partner-tree-structure') &&
           !urlString.includes('/api/values/liaison-offices') &&
           !urlString.includes('/api/values/contacts') &&
           !urlString.includes('/api/values/users/paged') &&
           !urlString.includes('/api/values/users/search') &&
           !urlString.includes('/api/values/salutations') &&
           !urlString.includes('/api/values/status') &&
           !urlString.includes('/api/values/pronouns') &&
           !urlString.includes('/api/values/countries') &&
           !urlString.includes('/api/values/states') &&
           !urlString.includes('/api/values/partner-groups') &&
           !urlString.includes('/api/values/sdg') &&
           !urlString.includes('/api/values/currency') &&
           !urlString.includes('/api/values/gemini-models') &&
           !urlString.includes('/api/document-transcribe') &&
           !urlString.includes('/api/auditlog') &&
           !urlString.includes('/api/interactions-brief') &&
           // Exclude the entity list endpoints (handled above)
           !/\/api\/partner(\?|$)/.test(urlString) &&
           !/\/api\/partner\/search/.test(urlString) &&
           !/\/api\/contact(\?|$)/.test(urlString) &&
           !/\/api\/contact\/search/.test(urlString) &&
           !/\/api\/interactions?(\?|$)/.test(urlString) &&
           !/\/api\/interaction\/search/.test(urlString) &&
           !/\/api\/opportunity(\?|$)/.test(urlString) &&
           !/\/api\/opportunity\/search/.test(urlString) &&
           // Exclude the entity detail endpoints (handled above)
           // CRITICAL: Use $ anchor to match ONLY exact detail URLs, NOT sub-resources
           // Without $, patterns like /api/opportunity/1/generate-images would be excluded
           // from the catch-all AND not matched by the specific mock, causing proxy hangs
           !/\/api\/partner\/\d+$/.test(urlString) &&
           !/\/api\/partner\/\d+\/permissions/.test(urlString) &&
           !/\/api\/partner\/\d+\/interactions/.test(urlString) &&
           !/\/api\/opportunity\/\d+$/.test(urlString) &&
           !/\/api\/opportunity\/\d+\/permissions/.test(urlString) &&
           !/\/api\/opportunity\/\d+\/apply-ai-changes/.test(urlString) &&
           !/\/api\/contact\/\d+$/.test(urlString) &&
           !/\/api\/contact\/\d+\/profile-picture/.test(urlString) &&
           !/\/api\/interaction\/\d+$/.test(urlString) &&
           !/\/api\/document\/entity\//.test(urlString) &&
           // Exclude only workflow URLs with entity AND id (handled above)
           !/\/api\/workflow\/\w+\/\d+/.test(urlString) &&
           !urlString.includes('/api/workflow/');
  }, async (route) => {
    const url = route.request().url();
    const method = route.request().method();
    mockLog(`[API Mock] Catch-all intercepted: ${method} ${url}`);
    
    // Smart responses based on URL patterns
    if (method === 'GET') {
      // /api/permissions - Permission config (PermissionService constructor)
      if (url.includes('/api/permissions') && !url.includes('/api/permissions/check/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            permissions: [],
            roles: [],
          }),
        });
        return;
      }
      // /api/dev/check-iap-simulation - Dev IAP auth check
      if (url.includes('/api/dev/check-iap-simulation')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ isIapSimulation: true }),
        });
        return;
      }
      // Permission check endpoints - return correct structure matching Angular PermissionService expectations
      // Restricted users get view-only permissions
      if (url.includes('/api/permissions/check/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            hasAccess: true, // ✅ Required field
            route: url, // ✅ Required field
            entity: 'Contact', // ✅ Required field
            permissions: isRestrictedUser ? {
              canRead: true,
              canCreate: false,
              canUpdate: false,
              canDelete: false,
              canExport: false,
              canImport: false,
              canApprove: false,
              canActivate: false,
              canClose: false,
              canArchive: false,
            } : {
              canRead: true, // ✅ Note: canRead, not canView
              canCreate: true,
              canUpdate: true, // ✅ Note: canUpdate, not canEdit
              canDelete: true,
              canExport: true,
              canImport: true,
              canApprove: false,
              canActivate: false,
              canClose: false,
              canArchive: false,
            }
          }),
        });
      }
      // Entity configuration - /api/entities for entity manager dropdown
      else if (url.includes('/api/entities') && !url.includes('/api/entity-configuration/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { entityName: 'Partner', value: 'Organization/Partner', translatedLabel: 'Partner' },
            { entityName: 'Contact', value: 'Contact', translatedLabel: 'Contact' },
            { entityName: 'Interaction', value: 'Interaction', translatedLabel: 'Interaction' },
            { entityName: 'Opportunity', value: 'Opportunity', translatedLabel: 'Opportunity' },
          ]),
        });
      }
      // Entity configuration endpoints - return config with fields for entity manager
      else if (url.includes('/api/entity-configuration/')) {
        const match = url.match(/\/api\/entity-configuration\/([^/?]+)/);
        const entityName = match ? decodeURIComponent(match[1]) : 'Partner';
        if (entityName && !entityName.includes('save') && !entityName.includes('export') && !entityName.includes('fields')) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              id: 1,
              entityName,
              description: `Configuration for ${entityName}`,
              fields: [
                { id: 1, fieldName: 'Name', dataType: 'string', showInListView: true, listViewOrder: 1, listViewLabel: 'Name', listViewType: 'text' },
                { id: 2, fieldName: 'Status', dataType: 'string', showInListView: true, listViewOrder: 2, listViewLabel: 'Status', listViewType: 'text' },
              ],
              listViewFields: [
                { id: 1, fieldName: 'Name', listViewOrder: 1, listViewLabel: 'Name', listViewType: 'text' },
                { id: 2, fieldName: 'Status', listViewOrder: 2, listViewLabel: 'Status', listViewType: 'text' },
              ],
            }),
          });
        } else {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify([]),
          });
        }
      }
      // Role endpoints
      else if (url.includes('/api/role/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // Organization hierarchy - expects array, not object
      else if (url.includes('/api/organization-hierarchy')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]), // Returns array directly
        });
      }
      // User preferences
      else if (url.includes('/api/user-preferences/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({}),
        });
      }
      // Base engagement - list and detail
      else if (url.includes('/api/base-engagement')) {
        const idMatch = url.match(/\/api\/base-engagement\/(\d+)/);
        if (idMatch) {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              id: parseInt(idMatch[1]),
              name: 'Test Base Engagement',
              description: 'Test engagement for E2E',
              status: 'Active',
              createdDate: '2024-01-01T00:00:00Z',
            }),
          });
        } else {
          await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
              records: [
                { id: 1, name: 'Test Engagement 1', status: 'Active', createdDate: '2024-01-01T00:00:00Z' },
                { id: 2, name: 'Test Engagement 2', status: 'Active', createdDate: '2024-02-01T00:00:00Z' },
              ],
              totalCount: 2,
            }),
          });
        }
      }
      // SavedFilter
      else if (url.includes('/api/SavedFilter')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // User info
      else if (url.includes('/api/user-info/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: '12345',
            email: 'test@unops.org',
            name: 'Test User',
          }),
        });
      }
      // AI assistant
      else if (url.includes('/ai-assistant/')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
      // Document types - required for document upload dialog
      else if (url.includes('/api/document-type')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(referenceData.documentTypes),
        });
      }
      // Comments endpoint - required for collaboration section
      else if (url.includes('/api/comment') || url.includes('/api/collaboration')) {
        const idMatch = url.match(/\/(\d+)\/comments/);
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(idMatch ? [
            { id: 1, text: 'Initial review completed', author: 'Test User', createdDate: '2024-06-15T10:00:00Z', isPinned: false },
            { id: 2, text: 'Budget approved for phase 1', author: 'Jane Doe', createdDate: '2024-06-16T14:00:00Z', isPinned: true },
          ] : []),
        });
      }
      // Entity artifacts endpoint
      else if (url.includes('/api/entity-artifact')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            records: [
              { id: 1, name: 'Partner Logo', entityType: 'Partner', artifactType: 'Image', status: 'Active', createdDate: '2024-01-01T00:00:00Z' },
              { id: 2, name: 'Contact Photo', entityType: 'Contact', artifactType: 'Image', status: 'Active', createdDate: '2024-02-01T00:00:00Z' },
            ],
            totalCount: 2,
          }),
        });
      }
      // Translation endpoint for admin
      else if (url.includes('/api/translation') || url.includes('/api/translations')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            records: [
              { id: 1, key: 'partner.name', en: 'Name', fr: 'Nom', es: 'Nombre', pt: 'Nome' },
              { id: 2, key: 'partner.status', en: 'Status', fr: 'Statut', es: 'Estado', pt: 'Estado' },
            ],
            totalCount: 2,
          }),
        });
      }
      // Link endpoints for partner/entity links
      else if (url.includes('/api/link') || url.includes('/links')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { id: 1, title: 'Partner Website', url: 'https://example.org', type: 'External', createdDate: '2024-01-01T00:00:00Z' },
          ]),
        });
      }
      // AI prompt management endpoint
      else if (url.includes('/api/ai-prompt') || url.includes('/api/aiprompt')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { id: 1, name: 'Default Summary Prompt', category: 'Summary', isActive: true },
            { id: 2, name: 'Risk Assessment Prompt', category: 'Risk', isActive: true },
          ]),
        });
      }
      // Default for other GET requests
      else {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([]),
        });
      }
    } else {
      // For POST/PUT/DELETE, return success
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ success: true }),
      });
    }
  });

  mockLog('[API Mock] All API routes configured (including catch-all)');
}

/**
 * Setup authenticated user claims
 * @param page - Playwright page object
 * @param email - User email
 */
export async function setupAuthenticatedUserMock(page: Page, email: string): Promise<void> {
  // Mock /user/claims endpoint - Return authenticated user claims
  await page.unroute(url => url.toString().includes('/user/claims')); // Remove existing mock
  await page.route(url => url.toString().includes('/user/claims'), async (route) => {
    mockLog('[API Mock] Intercepted: /user/claims (authenticated)');
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { type: 'email', value: email },
        { type: 'name', value: 'Test User' },
        { type: 'role', value: 'Administrator' },
      ]),
    });
  });
}

/**
 * Setup camera/MediaDevices API mocks for business card scanner
 * @param page - Playwright page object
 */
export async function setupCameraMocks(page: Page): Promise<void> {
  mockLog('[API Mock] Setting up camera/MediaDevices mocks...');
  
  await page.addInitScript(() => {
    // Mock getUserMedia for camera access
    if (navigator.mediaDevices) {
      navigator.mediaDevices.getUserMedia = async (constraints: MediaStreamConstraints) => {
        console.log('[Camera Mock] getUserMedia called with constraints:', constraints);
        
        // ✅ Create a real MediaStream using canvas captureStream for browser compatibility
        // This creates an actual MediaStream that can be assigned to video.srcObject
        const canvas = document.createElement('canvas');
        canvas.width = 1280;
        canvas.height = 720;
        
        // Draw a test pattern so the video element has content
        const ctx = canvas.getContext('2d');
        if (ctx) {
          ctx.fillStyle = '#1a1a1a';
          ctx.fillRect(0, 0, canvas.width, canvas.height);
          ctx.fillStyle = '#00ff00';
          ctx.font = '48px Arial';
          ctx.textAlign = 'center';
          ctx.fillText('MOCK CAMERA', canvas.width / 2, canvas.height / 2);
          ctx.fillText('Test Environment', canvas.width / 2, canvas.height / 2 + 60);
        }
        
        // ✅ captureStream() returns a REAL MediaStream that the browser accepts
        const stream = canvas.captureStream(30); // 30 FPS
        
        // Add required methods to the stream
        const originalGetTracks = stream.getTracks.bind(stream);
        stream.getTracks = () => {
          const tracks = originalGetTracks();
          // Enhance tracks with required methods if not present
          tracks.forEach(track => {
            if (!track.getSettings) {
              (track as any).getSettings = () => ({
                width: 1280,
                height: 720,
                aspectRatio: 16/9,
                frameRate: 30,
                facingMode: 'environment',
              });
            }
          });
          return tracks;
        };
        
        console.log('[Camera Mock] Created real MediaStream from canvas');
        return Promise.resolve(stream);
      };
      
      // Mock enumerateDevices
      navigator.mediaDevices.enumerateDevices = async () => {
        console.log('[Camera Mock] enumerateDevices called');
        return [
          {
            kind: 'videoinput',
            deviceId: 'mock-camera-1',
            label: 'Mock Camera (front)',
            groupId: 'mock-group-1',
            toJSON: () => ({}),
          },
          {
            kind: 'videoinput',
            deviceId: 'mock-camera-2',
            label: 'Mock Camera (back)',
            groupId: 'mock-group-1',
            toJSON: () => ({}),
          },
        ] as MediaDeviceInfo[];
      };
      
      // Mock getSupportedConstraints
      navigator.mediaDevices.getSupportedConstraints = () => ({
        aspectRatio: true,
        facingMode: true,
        frameRate: true,
        height: true,
        width: true,
        deviceId: true,
      });
    }
  });
  
  mockLog('[API Mock] Camera/MediaDevices mocks configured');
}

/**
 * Clear all API mocks
 * @param page - Playwright page object
 */
export async function clearAPIMocks(page: Page): Promise<void> {
  // Unroute all routes - Playwright allows unrouting all at once
  await page.unrouteAll({ behavior: 'ignoreErrors' });
  mockLog('[API Mock] All API routes cleared');
}
