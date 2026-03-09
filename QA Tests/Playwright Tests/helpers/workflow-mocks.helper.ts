/**
 * @fileoverview Shared Workflow Mock Helpers
 * Centralises duplicated mock setup for notifications, opportunity detail,
 * opportunity permissions, and pending-approval route handlers used across
 * workflow-* spec files.
 *
 * @author UNOPS Opportunity+ QA Team
 */

import { Page } from '@playwright/test';

// ---------------------------------------------------------------------------
// Notification mock helpers
// ---------------------------------------------------------------------------

export interface NotificationMock {
  id: number;
  message: string;
  category: string;
  responseType: string;
  entity?: string;
  entityId?: number;
  status: string;
  isRead: boolean;
  createdAt: string;
}

/**
 * Create a workflow approval notification payload with sensible defaults.
 */
export function createWorkflowNotification(
  overrides: Partial<NotificationMock> = {},
): NotificationMock {
  return {
    id: 1,
    message:
      'Opportunity "Healthcare Capacity Building" requires your Go/No-Go decision',
    category: 'workflow_approval',
    responseType: 'Pending',
    entity: 'Opportunity',
    entityId: 12,
    status: 'Pending',
    isRead: false,
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

/**
 * Mock the GET /api/notifications endpoint and the mark-read sub-route.
 * Pass an array of notifications to return, or an empty array for "no notifications".
 */
export function setupNotificationsMock(
  page: Page,
  notifications: Array<Record<string, unknown>>,
): ReturnType<Page['route']> {
  return page.route('**/api/notifications**', async (route) => {
    const url = route.request().url();
    if (
      route.request().method() === 'GET' &&
      !url.match(/\/api\/notifications\/\d+\//)
    ) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(notifications),
      });
    } else if (url.match(/\/api\/notifications\/\d+\/read/)) {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    } else {
      await route.continue();
    }
  });
}

// ---------------------------------------------------------------------------
// Opportunity mock helpers
// ---------------------------------------------------------------------------

/**
 * Default opportunity payload suitable for workflow / statement tests.
 * Call with overrides to customise individual fields.
 */
export function getOpportunityPayload(
  id: number,
  nameOrOverrides:
    | string
    | Partial<Record<string, unknown>> = 'Healthcare Capacity Building',
): Record<string, unknown> {
  const name =
    typeof nameOrOverrides === 'string' ? nameOrOverrides : (nameOrOverrides.name as string) ?? 'Healthcare Capacity Building';
  const overrides =
    typeof nameOrOverrides === 'object' ? nameOrOverrides : {};

  return {
    id,
    name,
    title: name,
    description: 'Test opportunity',
    status: 'Active',
    stage: 'SEND FOR GO DECISION',
    workflowStatus: 'SEND FOR GO DECISION',
    isInWorkflow: true,
    value: 2000000,
    currency: 'USD',
    estimatedValue: 2000000,
    probability: 80,
    expectedCloseDate: '2026-12-31T00:00:00Z',
    targetSigningDate: '2026-06-30T00:00:00Z',
    startDate: '2026-01-01T00:00:00Z',
    endDate: '2026-12-31T00:00:00Z',
    createdDate: '2025-01-01T00:00:00Z',
    lastModifiedDate: '2025-06-15T12:00:00Z',
    createdBy: 'system',
    lastModifiedBy: 'system',
    partner: { id: 1, name: 'UNICEF' },
    organizationUnit: { id: 1, name: 'HQ', code: 'HQ' },
    responsibleOrgUnitName: 'HQ - Headquarters',
    proposedInitiativeTypeName: 'Technical Assistance',
    initiativeBudgetUSD: 2000000,
    opportunityType: { id: 1, name: 'New Business' },
    sector: { id: 1, name: 'Health' },
    country: 'United States',
    region: 'North America',
    opportunityManager: { id: 1, name: 'Test OM', email: 'om@test.org' },
    collaborators: [],
    stakeholders: [],
    sdgs: [],
    beneficiaryCount: 10000,
    beneficiaryBreakdown: {},
    unCooperationFramework: null,
    highRiskChecklist: [],
    scope: 'Test scope',
    deliverables: [],
    initiativeType: { id: 1, name: 'Technical Assistance' },
    contacts: [],
    interactions: [],
    documents: [],
    fundingPartners: [],
    clientPartners: [],
    risks: [],
    opportunityStatementMarkdown:
      '# Static Statement Snapshot\n\nThis is the snapshot at submission.',
    ...overrides,
  };
}

/**
 * Opportunity payload that includes funding/client partners and risks
 * (used by the Decision Info Panel tests).
 */
export function getWorkflowOpportunityPayload(
  id: number,
  overrides: Partial<Record<string, unknown>> = {},
): Record<string, unknown> {
  return getOpportunityPayload(id, {
    name: 'Healthcare Capacity Building - Go Decision',
    fundingPartners: [
      {
        partnerName: 'Partner A',
        ddStatus: 'Approved',
        ddExpiryDate: '2027-01-01T00:00:00Z',
      },
    ],
    clientPartners: [
      { partnerName: 'Partner B', ddStatus: 'Pending', ddExpiryDate: null },
    ],
    risks: [
      {
        id: 1,
        title: 'Budget Risk',
        riskCategoryName: 'Financial',
        riskImpactLevelName: 'High',
        preDefinedHighRiskId: null,
      },
    ],
    permissions: {
      canView: true,
      canEdit: false,
      canApprove: true,
      isApprovalPending: true,
    },
    ...overrides,
  });
}

/**
 * Mock GET /api/opportunity/{id} with a specific payload (or error status).
 */
export function setupOpportunityMock(
  page: Page,
  oppId: number,
  payload: Record<string, unknown> | null,
  status: number = 200,
): ReturnType<Page['route']> {
  return page.route(`**/api/opportunity/${oppId}$`, async (route) => {
    if (payload === null) {
      await route.fulfill({ status });
      return;
    }
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(payload),
    });
  });
}

/**
 * Mock GET /api/opportunity/{id}/permissions with a specific permissions object.
 */
export function setupOpportunityPermissionsMock(
  page: Page,
  oppId: number,
  permissions: Record<string, boolean>,
): ReturnType<Page['route']> {
  return page.route(
    `**/api/opportunity/${oppId}/permissions**`,
    async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(permissions),
      });
    },
  );
}

// ---------------------------------------------------------------------------
// Pending approvals mock helpers
// ---------------------------------------------------------------------------

export interface PendingApproval {
  entityName: string;
  entityId: number;
  entityDisplayName: string;
  currentStage: string;
  pendingStage: string;
  submittedBy: string;
  submittedOn: string;
  orgUnitName: string;
  submissionComment: string;
}

/**
 * Create a pending approval object with sensible defaults.
 */
export function createPendingApproval(
  overrides: Partial<PendingApproval> = {},
): PendingApproval {
  return {
    entityName: 'Opportunity',
    entityId: 12,
    entityDisplayName:
      'Healthcare Capacity Building - Go Decision Pending',
    currentStage: 'IDENTIFY & PROFILE',
    pendingStage: 'GO',
    submittedBy: 'Test OM',
    submittedOn: new Date().toISOString(),
    orgUnitName: 'HQ - Headquarters',
    submissionComment: 'Ready for review',
    ...overrides,
  };
}

/**
 * Mock GET /api/workflow/pending-approvals with the given items, or an
 * error response when `statusOrItems` is a number.
 */
export function setupPendingApprovalsMock(
  page: Page,
  itemsOrStatus: PendingApproval[] | number,
): ReturnType<Page['route']> {
  return page.route(
    '**/api/workflow/pending-approvals**',
    async (route) => {
      if (route.request().method() !== 'GET') {
        await route.continue();
        return;
      }
      if (typeof itemsOrStatus === 'number') {
        await route.fulfill({
          status: itemsOrStatus,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Simulated error' }),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(itemsOrStatus),
      });
    },
  );
}

// ---------------------------------------------------------------------------
// Standard permission sets (common across workflow specs)
// ---------------------------------------------------------------------------

export const FULL_PERMISSIONS = {
  canView: true,
  canEdit: true,
  canDelete: false,
  canSubmit: true,
  canApprove: false,
  canActivate: true,
  canCancel: false,
};

export const READONLY_PERMISSIONS = {
  canView: true,
  canEdit: false,
  canDelete: false,
  canSubmit: false,
  canApprove: false,
  canActivate: false,
  canCancel: false,
};

export const APPROVER_PERMISSIONS = {
  canView: true,
  canEdit: false,
  canDelete: false,
  canSubmit: false,
  canApprove: true,
  canActivate: false,
  canCancel: false,
};
