/// <summary>
/// Miscellaneous Fixes Specification — PNO-805, PNO-801
///
/// Requirements validated:
/// - PNO-805: AI Opportunity creation — Opportunity Manager must be the logged-in user, NOT the service account.
///   When using AI Assistant to create a new Opportunity, the user must be recorded as the Opportunity Manager.
///   Tests: CreateOpportunityFromProposalAsync with currentUserId; AssignCreatorAsOpportunityManagerAsync.
///   Service account IDs (0, -1) must NOT be assigned as OM.
///
/// - PNO-801: Remove "Leads" and "Initiatives" from the side panel.
///   Frontend-only change — validated via Playwright E2E tests. Backend C# tests have no direct surface.
///   DEF-217: breadcrumb.component.ts labelMap still contains legacy Leads/Initiatives mappings.
///
/// Testable surface (backend):
/// - UNOPSOpportunityManager.CreateOpportunityFromProposalAsync(request, currentUserId)
/// - UNOPSOpportunityManager.AssignCreatorAsOpportunityManagerAsync(opportunityId, userId)
///
/// Defects found:
/// - DEF-217: Breadcrumb labelMap contains legacy Leads/Initiatives mappings (PNO-801 scope)
/// </summary>
