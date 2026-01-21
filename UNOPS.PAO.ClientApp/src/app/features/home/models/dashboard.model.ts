/**
 * @fileoverview Lightweight TypeScript models for Dashboard API responses
 * These models match the optimized backend DashboardModels.cs
 * 
 * PERFORMANCE: These interfaces only include fields needed for dashboard display,
 * reducing the response payload by ~90% compared to full entity models.
 * 
 * @author UNOPS Opportunity+ System Development Team
 */

/**
 * Lightweight partner model for dashboard display.
 * Only contains essential fields needed for partner cards and lists.
 */
export interface DashboardPartner {
  id: number;
  name: string;
  status: string | null;
  createdDate: string | null;
  lastModifiedDate: string | null;
}

/**
 * Lightweight contact model for dashboard display.
 * Only contains essential fields needed for contact cards and lists.
 */
export interface DashboardContact {
  id: number;
  firstName: string | null;
  lastName: string | null;
  title: string | null;
  status: string | null;
  createdDate: string | null;
  lastModifiedDate: string | null;
}

/**
 * Lightweight interaction model for dashboard display.
 * Only contains essential fields needed for interaction cards and lists.
 */
export interface DashboardInteraction {
  id: number;
  type: string | null;
  subject: string | null;
  description: string | null;
  date: string | null;
  status: string | null;
  createdDate: string | null;
  lastModifiedDate: string | null;
}

/**
 * Lightweight opportunity model for dashboard display.
 * Only contains essential fields needed for opportunity cards and lists.
 */
export interface DashboardOpportunity {
  id: number;
  name: string;
  status: string | null;
  stage: string | null;
  userRole: string | null;
  createdDate: string | null;
  lastModifiedDate: string | null;
}

/**
 * Recent update model for the "Recent Activity" section.
 * Represents any entity type (Partner, Contact, Interaction, Opportunity).
 */
export interface DashboardRecentUpdate {
  id: number;
  name: string;
  type: 'Partner' | 'Contact' | 'Interaction' | 'Opportunity';
  lastModifiedDate: string | null;
  lastModifiedBy: number;
  lastModifiedByName: string | null;
  status: string | null;
}

/**
 * Combined response model from the optimized dashboard endpoint.
 * All lists use lightweight models instead of full entity models.
 * 
 * Expected response size: ~80-90% smaller than using full models
 */
export interface DashboardCombinedResponse {
  // My Workspace data (non-draft, all statuses)
  myPartners: DashboardPartner[];
  myContacts: DashboardContact[];
  myInteractions: DashboardInteraction[];
  myOpportunities: DashboardOpportunity[];
  
  // Draft items requiring attention
  draftPartners: DashboardPartner[];
  draftContacts: DashboardContact[];
  draftInteractions: DashboardInteraction[];
  draftOpportunities: DashboardOpportunity[];
  
  // Recent activity from org unit
  orgUnitRecentUpdates: DashboardRecentUpdate[];
  orgUnitName: string;
  orgUnitId: number | null;
}

/**
 * Internal view model for the dashboard component.
 * Transforms the API response into a format more convenient for the template.
 */
export interface DashboardData {
  myPartners: DashboardPartner[];
  myContacts: DashboardContact[];
  myInteractions: DashboardInteraction[];
  myOpportunities: DashboardOpportunity[];
  draftActions: {
    partners: DashboardPartner[];
    contacts: DashboardContact[];
    interactions: DashboardInteraction[];
    opportunities: DashboardOpportunity[];
  };
  orgUnitRecentUpdates: DashboardRecentUpdate[];
  orgUnitName: string;
}

/**
 * Union type for draft action items (used in "Actions Required" section)
 */
export type DraftActionItem = DashboardPartner | DashboardContact | DashboardInteraction | DashboardOpportunity;

