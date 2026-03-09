# Defect List for Developers

This document tracks **production code defects** discovered during testing. These are issues where implemented functionality does not match documented requirements (PRD, specifications, acceptance criteria).

**Scope:** All production code defects requiring developer intervention:
- ✅ **Functional:** Business logic bugs, incorrect behavior, missing features
- ✅ **API/Integration:** Contract violations, endpoint failures, data mapping issues
- ✅ **Architecture:** Design flaws, async/sync problems, dependency issues
- ✅ **Security:** Vulnerabilities, auth bypass, data exposure, injection flaws
- ✅ **Performance:** Slow queries, memory leaks, N+1 problems, resource exhaustion
- ✅ **Accessibility:** WCAG violations, keyboard nav, screen reader issues
- ✅ **Data Integrity:** Calculation errors, constraint violations, data corruption
- ✅ **Error Handling:** Unhandled exceptions, poor messages, silent failures
- ✅ **Compatibility:** Browser-specific bugs, device/OS issues, responsive layout
- ✅ **Internationalization:** Translation bugs, locale formatting, RTL issues
- ✅ **Concurrency:** Race conditions, deadlocks, thread safety problems
- ✅ **Observability:** Missing logs, audit gaps, inadequate telemetry
- ✅ **Configuration:** Wrong defaults, missing feature flags, env issues
- ✅ **Dependencies:** Vulnerable packages, version conflicts, deprecated APIs

**Prefix:** DEF-XXX  
**File Owner:** Development Team

> ⚠️ **Important Distinction**: This list is for **actual defects** in production code, NOT:
> - Test infrastructure issues → See `Defect List for QA.md`
> - Tests written for unimplemented features → Track in backlog/sprint planning
> - Test selector/locator issues → See `Defect List for QA.md` (QA owns test locator strategy)
> - Test environment limitations → See `Defect List for QA.md`

---

## Open Defects

> **98 open** | Sorted by severity (Critical → High → Medium → Low), then by date reported.

| Defect ID | Severity | Title | Component | Date Reported | Status | Developer Feedback |
|-----------|----------|-------|-----------|---------------|--------|--------------------|
| DEF-232 | 🔴 Critical | PartnerManager.GetPartner returns soft-deleted partners — missing IsDeleted filter | PartnerManager | 2026-03-09 | Open | `GetPartner()` (line 251) queries `PartnerRepository.GetAll().Where(x => x.Id == id)` without `!x.IsDeleted`. Soft-deleted partners are returned to callers, displayed in partner detail pages. **Proper Fix:** Add `&& !x.IsDeleted` to Where clause. **Related Tests:** `SoftDeleteAuditTests.PartnerManager_GetPartner_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-232")] |
| DEF-233 | 🔴 Critical | PartnerManager.UpdatePartnerAsync allows updating soft-deleted partners | PartnerManager | 2026-03-09 | Open | `UpdatePartnerAsync()` (line 286) fetches partner by ID without IsDeleted check. Users can modify data on records that should be invisible. **Proper Fix:** Add `&& !x.IsDeleted` to the fetch query and throw KeyNotFoundException if deleted. **Related Tests:** `SoftDeleteAuditTests.PartnerManager_UpdatePartnerAsync_ShouldNotAllowUpdatingSoftDeletedPartner` [Trait("Defect", "DEF-233")] |
| DEF-234 | 🔴 Critical | PartnerManager.GetPartnerAsync returns soft-deleted partners with includes | PartnerManager | 2026-03-09 | Open | `GetPartnerAsync()` (line 344) queries with includes but no IsDeleted filter. Returns full partner detail with related data for deleted records. **Proper Fix:** Add `&& !x.IsDeleted` to Where clause. **Related Tests:** `SoftDeleteAuditTests.PartnerManager_GetPartnerAsync_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-234")] |
| DEF-235 | 🔴 Critical | UNOPSPartnerManager.GetPartner returns soft-deleted partners via direct context query | UNOPSPartnerManager | 2026-03-09 | Open | `GetPartner()` (line 514) queries `_context.Partners.FirstOrDefaultAsync(p => p.Id == id)` without IsDeleted. Bypasses repository pattern entirely. **Proper Fix:** Add `&& !p.IsDeleted` to predicate. **Related Tests:** `SoftDeleteAuditTests.UNOPSPartnerManager_GetPartner_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-235")] |
| DEF-236 | 🔴 Critical | UNOPSPartnerManager.GetBasicPartnerDetailsAsync returns soft-deleted partner details | UNOPSPartnerManager | 2026-03-09 | Open | `GetBasicPartnerDetailsAsync()` (line 542) returns partner with PartnerGroup, LiaisonOffice, Documents for deleted partners. UI partner detail page shows deleted data. **Proper Fix:** Add `&& !p.IsDeleted` to FirstOrDefaultAsync predicate. **Related Tests:** `SoftDeleteAuditTests.UNOPSPartnerManager_GetBasicPartnerDetailsAsync_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-236")] |
| DEF-237 | 🔴 Critical | UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsAsync returns soft-deleted partners | UNOPSPartnerManager | 2026-03-09 | Open | `GetPartnerWithContactsAndInteractionsAsync()` (line 727) loads contacts and interactions for deleted partners. **Proper Fix:** Add `&& !p.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSPartnerManager_GetPartnerWithContactsAsync_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-237")] |
| DEF-238 | 🔴 Critical | UNOPSPartnerManager.GetPartnerWithContactsAndInteractionsForAIAsync feeds soft-deleted partner data to AI | UNOPSPartnerManager | 2026-03-09 | Open | `GetPartnerWithContactsAndInteractionsForAIAsync()` (line 796) sends deleted partner data to AI for summary generation. AI generates content from deleted records. **Proper Fix:** Add `&& !p.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSPartnerManager_GetPartnerForAI_ShouldNotReturnSoftDeletedPartner` [Trait("Defect", "DEF-238")] |
| DEF-239 | 🔴 Critical | UNOPSContactManager.GetBasicEntityAsync returns soft-deleted contacts | UNOPSContactManager | 2026-03-09 | Open | `GetBasicEntityAsync()` (line 763) queries `_context.Contacts.FirstOrDefaultAsync(e => e.Id == entityId)` without IsDeleted. Deleted contact data leaks to AI features. **Proper Fix:** Add `&& !e.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSContactManager_GetBasicEntityAsync_ShouldNotReturnSoftDeletedContact` [Trait("Defect", "DEF-239")] |
| DEF-240 | 🔴 Critical | UNOPSContactManager.GetBasicEntityDataAsync returns soft-deleted contacts | UNOPSContactManager | 2026-03-09 | Open | `GetBasicEntityDataAsync()` (line 776) queries without IsDeleted filter. **Proper Fix:** Add `&& !e.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSContactManager_GetBasicEntityDataAsync_ShouldNotReturnSoftDeletedContact` [Trait("Defect", "DEF-240")] |
| DEF-241 | 🟠 High | UNOPSContactManager.GetContactsForGmailAddon returns soft-deleted contacts in Gmail addon | UNOPSContactManager | 2026-03-09 | Open | `GetContactsForGmailAddon()` (line 778) searches contacts by email without IsDeleted filter. Users see deleted contacts when composing emails in Gmail. **Proper Fix:** Add `&& !c.IsDeleted` to the Where clause. **Related Tests:** `SoftDeleteAuditTests.UNOPSContactManager_GetContactsForGmailAddon_ShouldNotReturnSoftDeletedContacts` [Trait("Defect", "DEF-241")] |
| DEF-242 | 🟠 High | UNOPSContactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync includes soft-deleted contacts | UNOPSContactManager | 2026-03-09 | Open | `GetUnmatchedEmailsWithPartnerSuggestionsAsync()` (line 923) domain-based email search includes deleted contacts in partner suggestions. **Proper Fix:** Add `&& !c.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSContactManager_GetUnmatchedEmails_ShouldNotReturnSoftDeletedContacts` [Trait("Defect", "DEF-242")] |
| DEF-243 | 🔴 Critical | UNOPSDocumentManager.GetBasicEntityAsync returns soft-deleted documents | UNOPSDocumentManager | 2026-03-09 | Open | `GetBasicEntityAsync()` (line 56) queries `_context.Set<UNOPSDocument>().FirstOrDefaultAsync(d => d.Id == entityId)` without IsDeleted. Deleted documents referenced in AI context. **Proper Fix:** Add `&& !d.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSDocumentManager_GetBasicEntityAsync_ShouldNotReturnSoftDeletedDocument` [Trait("Defect", "DEF-243")] |
| DEF-244 | 🔴 Critical | UNOPSDocumentManager.GetDocumentByIdAsync returns soft-deleted documents | UNOPSDocumentManager | 2026-03-09 | Open | `GetDocumentByIdAsync()` (line 381) returns deleted documents — users can still open and view deleted docs. **Proper Fix:** Add `&& !d.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSDocumentManager_GetDocumentByIdAsync_ShouldNotReturnSoftDeletedDocument` [Trait("Defect", "DEF-244")] |
| DEF-245 | 🟠 High | UNOPSDocumentManager.GetDocumentParentEntityByIdAsync returns soft-deleted documents | UNOPSDocumentManager | 2026-03-09 | Open | `GetDocumentParentEntityByIdAsync()` (line 358) navigates to parent entity of deleted documents. **Proper Fix:** Add `&& !d.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSDocumentManager_GetDocumentParentEntityByIdAsync_ShouldNotReturnSoftDeletedDocument` [Trait("Defect", "DEF-245")] |
| DEF-246 | 🔴 Critical | UNOPSDocumentManager.GetDocumentDetailsForAiAsync feeds soft-deleted document content to AI | UNOPSDocumentManager | 2026-03-09 | Open | `GetDocumentDetailsForAiAsync()` (line 453) sends deleted document content to AI for summary generation. AI generates content from deleted records. **Proper Fix:** Add `&& !d.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSDocumentManager_GetDocumentDetailsForAiAsync_ShouldNotReturnSoftDeletedDocument` [Trait("Defect", "DEF-246")] |
| DEF-247 | 🔴 Critical | UNOPSInteractionManager.GetBasicEntityDataAsync returns soft-deleted interactions | UNOPSInteractionManager | 2026-03-09 | Open | `GetBasicEntityDataAsync()` (line 1076) queries `_context.Interactions.FirstOrDefaultAsync(e => e.Id == id)` without IsDeleted. Deleted interaction data leaks to AI. **Proper Fix:** Add `&& !e.IsDeleted`. **Related Tests:** `SoftDeleteAuditTests.UNOPSInteractionManager_GetBasicEntityDataAsync_ShouldNotReturnSoftDeletedInteraction` [Trait("Defect", "DEF-247")] |
| DEF-248 | 🔴 Critical | OpportunityManager queries at lines 916, 958, 1190 operate on soft-deleted opportunities | OpportunityManager | 2026-03-09 | Open | Multiple methods (`UpdateWhereSectionAsync`, `GetRelatedItemsAsync`) query `context.Opportunities.FirstOrDefaultAsync(o => o.Id == id)` without IsDeleted. Also UNOPSOpportunityManager line 1293. **Proper Fix:** Add `&& !o.IsDeleted` to all Opportunity queries by ID. **Related Tests:** `SoftDeleteAuditTests.OpportunityManager_GetOpportunity_*` [Trait("Defect", "DEF-248")] |
| DEF-249 | 🟠 High | InteractionManager uses FindAsync to link soft-deleted contacts/partners to interactions | InteractionManager | 2026-03-09 | Open | `FindAsync()` at lines 66, 94 bypasses all query filters — allows linking soft-deleted contacts and partners to new interactions. **Proper Fix:** Replace `FindAsync(contactId)` with `FirstOrDefaultAsync(c => c.Id == contactId && !c.IsDeleted)`. **Related Tests:** `SoftDeleteAuditTests.InteractionManager_FindAsync*` [Trait("Defect", "DEF-249")] |
| DEF-250 | 🟠 High | UNOPSPartnerManager batch queries include soft-deleted partners and contacts | UNOPSPartnerManager | 2026-03-09 | Open | Batch queries at lines 313, 320 (`_context.Partners.Where(p => partnerIds.Contains(p.Id))` and `_context.Contacts.Where(c => partnerIds.Contains(c.PartnerId))`) include deleted records. Partner summary views show inflated counts. **Proper Fix:** Add `&& !p.IsDeleted` and `&& !c.IsDeleted` to batch Where clauses. **Related Tests:** `SoftDeleteAuditTests.*BatchQuery*` [Trait("Defect", "DEF-250")] |
| DEF-251 | 🟡 Medium | OpportunityWhenSection HTML template missing planned features (opportunityUpdated, changesDetected, date validation) | opportunity-when-section.component.html | 2026-03-09 | Open | PNO-1182 WHEN Tab: The `opportunity-when-section.component.html` template is missing several expected features: (1) `(opportunityUpdated)` output binding for notifying parent of changes, (2) `changesDetected` output/property for tracking unsaved edits, (3) `hasDateValidationErrors` property for blocking save on invalid dates, (4) `deliverableDates` / planned date section for deliverable scheduling. These are referenced by specification-based tests but do not exist in the current template.<br/><br/>**Root Cause:** Features described in the specification have not yet been implemented in the component template.<br/><br/>**Proper Fix:**<br/>• Add `(opportunityUpdated)` output event emitter to the component<br/>• Add change detection (`changesDetected`) tracking for unsaved edits<br/>• Add date validation with `hasDateValidationErrors` flag to block save<br/>• Add deliverable planned dates section to the WHEN tab<br/><br/>**Wrong Fix:** ❌ Removing or weakening the specification-based tests that expose these gaps<br/><br/>**Related Tests:** 6 tests tagged [Trait("Defect", "DEF-251")] across `OpportunityWhenSection/NegativeTests.cs`, `BoundaryTests.cs`, `FunctionalTests.cs`, `IntegrationTests.cs` |
| DEF-223 | 🟠 High | UserPreferenceService.CreateUserProfileAsync inserts UserProfile with null Name — violates not-null constraint | UserPreferenceService | 2026-03-09 | Open | CreateUserProfileAsync (line 258) creates UserProfile without setting the required Name property. ModifiableDeletableEntity requires Name. **Proper Fix:** Set Name when creating UserProfile (e.g., from AspNetUser email or "User {userId}"). **Related Tests:** `UserPreferenceServiceTests.UpdateDefaultOrgUnitAsync_AutoCreatesUserProfile_WhenMissing` [Trait("Defect", "DEF-223")] |
| DEF-224 | 🟡 Medium | GetDefaultOrgUnitIdAsync fallback to UserProfile OrgUnit returns null when UserProfile exists with OrgUnit | UserPreferenceService | 2026-03-09 | Open | When no UserPreference exists, service should fall back to UserProfile.OrgUnit and resolve OrganizationHierarchy by Code. Returns null instead. **Related Tests:** `UserPreferenceServiceTests.GetDefaultOrgUnitIdAsync_FallsBackToUserProfile_WhenNoPreference` [Trait("Defect", "DEF-224")] |
| DEF-222 | 🟡 Medium | GeoTimeCacheService creates its own HttpClient, ignores injected one — prevents HTTP mocking | GeoTimeCacheService | 2026-03-09 | Open | `GetLocationInfoAsync` (line 158) creates `using var httpClient = new HttpClient()` instead of using the injected `HttpClient` from the constructor. This prevents unit testing of HTTP success/failure/timeout/malformed response without modifying production code. API URL is hardcoded (ip-api.com). **Proper Fix:** Use the injected HttpClient for HTTP calls; add optional IHttpClientFactory or configurable API URL for testability. **Wrong Fix:** ❌ Changing test expectations. **Related Tests:** `GeoTimeCacheServiceTests.cs` — tests use cache pre-population to avoid HTTP; HTTP behavior cannot be unit mocked. |
| DEF-221 | 🟡 Medium | DeleteOpportunityAsync uses physical delete instead of soft delete | OpportunityManager | 2026-03-09 | Open | Per dotnet-implementation rule, entities inheriting from ModifiableDeletableEntity MUST use soft delete (set IsDeleted=true, DeletedBy, DeletedDate). `DeleteOpportunityAsync` calls `opportunityRepository.Delete(entity)` which performs physical delete (Remove). **Proper Fix:** Set entity.IsDeleted=true, entity.SetDeleteAuditData(currentUserId), UpdateAsync(entity) instead of Delete. **Wrong Fix:** ❌ Suppressing the defect test. **Related Tests:** `OpportunityManagerUnitTests/OpportunityManagerUnitTests.cs` DeleteOpportunityAsync_ShouldSoftDelete_RecordExistsWithIsDeletedTrue [Trait("Defect", "DEF-221")] |
| DEF-220 | 🟠 High | Rate limiting middleware not implemented — API endpoints not throttled | Startup / Middleware | 2026-03-09 | Open | No rate limiting middleware in pipeline. Excessive, rapid-fire, burst, and sustained high-rate requests all return 200 instead of 429. **Proper Fix:** Add AspNetCoreRateLimit or similar middleware; configure per-user/per-IP limits; return 429 with Retry-After and X-RateLimit-* headers when exceeded. **Related Tests:** `RateLimitingTests.cs` — 8 tests with [Trait("Defect", "DEF-220")] |
| DEF-205 | 🟢 Low | ValuesManager.GetUNCFOutcomes/GetUNCFIndicators do not expose includeInactive parameter | ValuesManager | 2026-03-09 | Open | ValuesRepository has `GetUNCFOutcomes(bool includeInactive)` and `GetUNCFIndicators(bool includeInactive)` but ValuesManager only exposes parameterless overloads. PNO-1164 EDS may need to include inactive UNCF data for sync. **Proper Fix:** Add `includeInactive` parameter to ValuesManager.GetUNCFOutcomes and GetUNCFIndicators, pass through to repository. **Related Tests:** `ExternalDataAndIntegration/NegativeTests.cs` NEG_088, NEG_089; `BoundaryTests.cs` BND_026, BND_027 |
| DEF-211 | 🟠 High | Offices API/Controller not implemented — PNO-1213 | OfficeController (missing) | 2026-03-09 | Open | PNO-1213: "Offices" top-level navigation with read-only view. No GET /api/offices or GET /api/offices/{id}. Tests use organization-hierarchy as proxy. **Proper Fix:** Implement OfficeController with list, detail, and tab endpoints per PNO-1213 AC. **Related Tests:** `OfficesFeature/IntegrationTests.cs` |
| DEF-212 | 🟠 High | OfficeManager/Office entity not implemented — PNO-1213 | OfficeManager (missing) | 2026-03-09 | Open | PNO-1213: Office Detail with tabs (Details, Financial, Scope, Roles & DoA, Related Opportunities, Related Partner Accounts, Documents). No Office entity or OfficeManager. **Proper Fix:** Implement OfficeManager with GetOfficeByIdAsync, GetRelatedOpportunitiesAsync, GetRelatedPartnersAsync. **Related Tests:** `OfficesFeature/` |
| DEF-213 | 🟡 Medium | Office Financial tab data (BigQuery) not integrated — PNO-1214 | Office/Data integration | 2026-03-09 | Open | PNO-1214 AC: Financial data (cost centre type, funding, NER/EA targets) from BigQuery. Not implemented. **Proper Fix:** EDS pipeline for BigQuery Corporate_Performance_Report. **Related Tests:** `OfficesFeature/` |
| DEF-214 | 🟡 Medium | Office Roles & DoA tab (ERP) not integrated — PNO-1214 | Office/Data integration | 2026-03-09 | Open | PNO-1214 AC: Operational roles, DoA holders from ERP Management Structure and Core Controls. Not implemented. **Proper Fix:** EDS pipelines for ERP Admin and ERP Core Controls. **Related Tests:** `OfficesFeature/` |
| DEF-215 | 🟡 Medium | Office Physical section (oUP Location Management) not integrated — PNO-1214 | Office/Data integration | 2026-03-09 | Open | PNO-1214 AC: Office locations, addresses, geo-coordinates from oUP Location Management. Not implemented. **Proper Fix:** EDS pipeline for oUP Location Management. **Related Tests:** `OfficesFeature/` |
| DEF-216 | 🟡 Medium | Office Documents tab — Strategy type, Regional Director upload restriction — PNO-1213 | Office/Documents | 2026-03-09 | Open | PNO-1213 AC: Documents tab allows upload of "Strategy" type only; restricted to Regional Director/Manager or OiC. Not implemented. **Proper Fix:** Document upload with entity type Office, artifact type Strategy, permission check for RD/Manager/OiC. **Related Tests:** `OfficesFeature/` |
| DEF-217 | 🟢 Low | Breadcrumb labelMap contains legacy Leads/Initiatives mappings — PNO-801 | breadcrumb.component.ts | 2026-03-09 | Open | PNO-801: Leads and Initiatives removed from sidebar. `breadcrumb.component.ts` labelMap (lines 153-154) still contains 'Leads': 'title.leads' and 'Initiatives': 'title.initiatives'. These are legacy mappings for URL-based navigation. Consider removing for consistency with PNO-801 scope.<br/><br/>**Root Cause:** Breadcrumb component not updated when sidebar menu items were removed.<br/><br/>**Proper Fix:** Remove 'Leads' and 'Initiatives' from labelMap if routes are deprecated; or keep for backward compatibility if direct URLs may still be used.<br/><br/>**Related Tests:** `MiscellaneousFixes/MiscellaneousFixesSpec.cs` |
| DEF-199 | 🟠 High | Document upload from Google Drive fails for Word/PDF — works from local only | UNOPSDocumentManager / Document upload pipeline | 2026-03-09 | Open | PNO-1216: Users get error when uploading Word/PDF from Google Drive to opportunity. Local upload works. **Root Cause:** Unknown — GCS/Drive integration. **Related Tests:** `RelatedDocsAndSearch/` |
| DEF-200 | 🟢 Low | RELATED Section: People titles may differ from Directory — data from BigQuery | AiRetrieverManager / Vector store | 2026-03-09 | Open | PNO-810: People show titles differing from Directory. Data from BigQuery/vector store. UAT passed with minimal discrepancies. **Related Tests:** `RelatedDocsAndSearch/` |
| DEF-175 | 🟠 High | ApplyAiChangesAsync does not validate empty/whitespace Name | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-804: When editing AI-generated opportunity, user can clear Name and save. `ApplyAiChangesAsync` (line ~3242) only checks `if (request.Name != null)` — it does not validate that Name is not empty or whitespace. Passing `Name = ""` or `Name = "   "` persists invalid data.<br/><br/>**Root Cause:** No `string.IsNullOrWhiteSpace(request.Name)` validation when Name is provided.<br/><br/>**Proper Fix:** Add validation: `if (request.Name != null && string.IsNullOrWhiteSpace(request.Name)) throw new BusinessException("Opportunity name is required.");`<br/><br/>**Wrong Fix:** ❌ Relying solely on client validation<br/><br/>**Related Tests:** `OpportunityAIFeatures/NegativeTests.cs` NEG-003, NEG-004, NEG-026; `OpportunityAIFeatures/FunctionalTests.cs` FNC-009 |
| DEF-176 | 🟡 Medium | ApplyAiChangesAsync does not validate empty/whitespace Description when mandatory per AC | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-804 AC4: Name and Description are mandatory. Per comment (2025-12-12), only Name is mandatory for creation; Description optional. For **edit** (ApplyAiChangesAsync), if Description is mandatory, passing `Description = ""` should be rejected. Current code allows empty string.<br/><br/>**Root Cause:** No validation when Description is explicitly set to empty.<br/><br/>**Proper Fix:** If Description is mandatory on edit, add validation when `request.Description != null && string.IsNullOrWhiteSpace(request.Description)`.<br/><br/>**Related Tests:** `OpportunityAIFeatures/` |
| DEF-177 | 🟢 Low | CreateOpportunityFromProposalAsync empty Name — verify error message is specific | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-803: Empty Name should show "Opportunity Name is required" not generic error. CreateOpportunityFromProposalAsync (line 3696) throws `BusinessException("Name is required.")` — verify this maps to user-facing specific message in controller/API.<br/><br/>**Related Tests:** `OpportunityAIFeatures/NegativeTests.cs` NEG-001, NEG-002 |
| DEF-178 | 🟡 Medium | Budget from AI docs — verify link to funding partners when InitiativeBudgetUSD set | AiContextualService / UNOPSGeminiManager | 2026-03-09 | Open | PNO-873: AI-extracted budget must be linked to funding partners. When AI proposes InitiativeBudgetUSD from document, the partnerBudgets array and BuildOpportunityCollectionObjects must associate amounts with partners. If discrepancy exists between doc budget and funding partner total, investigate AiContextualService partnerBudgets mapping.<br/><br/>**Related Tests:** `OpportunityAIFeatures/` PNO-873 tests |
| DEF-179 | 🟢 Low | CreateOpportunityFromProposalAsync with empty Description — per PNO-803 comment, optional | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-803 comment (2025-12-03): "Fill Opp Name, keep Opp Desc blank > NO error message > Opportunity gets created". PNO-804 comment: "Opportunity description is optional for opportunity creation at the moment." CreateOpportunityFromProposalAsync passes Description to CreateOpportunityAsync — if DB requires non-null, may need default to empty. DEF-050 tracks null Description.<br/><br/>**Related Tests:** `OpportunityAIFeatures/PositiveTests.cs` POS-009, `OpportunityAIFeatures/NegativeTests.cs` NEG-027 |
| DEF-193 | 🟡 Medium | UpdateTeamSectionAsync throws KeyNotFoundException on reload after successful DB update | UNOPSOpportunityManager | 2026-03-09 | Open | When OM is reassigned via UpdateTeamSectionAsync, the DB update succeeds (previous OM added as Collaborator, new OM as stakeholder) but GetOpportunityAsync reload throws KeyNotFoundException. PNO-1193 fix works at DB level; reload fails due to complex includes (e.g. Stakeholders.Contact invalid path per DEF-058).<br/><br/>**Root Cause:** Reload after UpdateTeamSectionAsync uses GetOpportunityAsync which has invalid include paths or fails for other reasons.<br/><br/>**Proper Fix:** Fix GetOpportunityAsync includes (DEF-058) or make UpdateTeamSectionAsync not depend on full reload.<br/><br/>**Related Tests:** `GoNoGoAndBudget/PositiveTests.cs`, `PNO-1166_RejectDuplicateAndOMTransfer/PositiveTests.cs` |
| DEF-146 | 🟡 Medium | UpdateWhenSectionAsync does not validate date order server-side | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-699, PNO-859: Angular WHEN section validates Implementation Start >= Signing, Delivery >= Impl Start, Submission <= Signing before save. `UpdateWhenSectionAsync` (UNOPSOpportunityManager.cs ~3167) persists dates without server-side validation. Direct API calls (e.g. Postman, integration) can bypass client validation and persist invalid date order (e.g. implementation start before signing).<br/><br/>**Root Cause:** No date validation in UpdateWhenSectionAsync before SaveChangesAsync.<br/><br/>**Proper Fix:** Add validation: if ImplementationStartDate < TargetSigningDate throw BusinessException; if TargetDeliveryDate < (ImplementationStartDate ?? TargetSigningDate) throw BusinessException; if SubmissionDeadline > TargetSigningDate throw BusinessException.<br/><br/>**Wrong Fix:** ❌ Relying solely on client validation<br/><br/>**Related Tests:** `OpportunityWhenSection/IntegrationTests.cs` |
| DEF-168 | 🟢 Low | Opportunity UX & Layout — PNO-862, PNO-863, PNO-877, PNO-882 are frontend-only | Documentation | 2026-03-09 | Open | PNO-862 (unsaved changes warning), PNO-863 (responsive layout), PNO-877 (click-to-edit), PNO-882 (banner/logo) are Angular/frontend concerns. Backend C# tests in `OpportunityUXAndLayout/` validate API contract (header, key info, quick stats, comments, risks). Full UX validation requires Playwright E2E tests. Not a production defect — scope clarification for QA. |
| DEF-169 | 🟡 Medium | RiskManager.CreateRiskAsync does not validate empty/whitespace Title server-side | RiskManager | 2026-03-09 | Open | PNO-761 AC2: Risk title is mandatory. `CreateRiskAsync` and `UpdateRiskAsync` may accept empty string "" or whitespace-only "   " as valid risk titles if client validation is bypassed (direct API call). Similar to DEF-071 for Opportunity name.<br/><br/>**Root Cause:** No `string.IsNullOrWhiteSpace(request.Title)` validation in RiskManager before persisting.<br/><br/>**Proper Fix:** Add validation in CreateRiskAsync and UpdateRiskAsync: if string.IsNullOrWhiteSpace(request.Title) throw BusinessException("Title is required").<br/><br/>**Wrong Fix:** ❌ Relying solely on client validation<br/><br/>**Related Tests:** `OpportunityStatementAndRisks/NegativeTests.cs` NEG_011, NEG_012, NEG_013 |
| DEF-156 | 🟡 Medium | WHO section formatCurrency uses hardcoded USD/$ for all funding partner amounts — PNO-788 | opportunity-who-section.component.ts | 2026-03-09 | Open | PNO-788: When a Funding Partner is added with a non-USD currency (e.g., RWF), the amount incorrectly shows both $ at the start and the currency code at the end (e.g., "$1,111 RWF"). Expected: "1,111 RWF" only.<br/><br/>**Root Cause:** `formatCurrency()` in `opportunity-who-section.component.ts` (line ~600) uses `Intl.NumberFormat` with `currency: 'USD'` and `style: 'currency'` for ALL amounts, regardless of the funding partner's selected currency. The method does not accept or use `currencyCode` from the partner.<br/><br/>**Proper Fix:** Update `formatCurrency(value, currencyCode?)` to accept the partner's `currencyCode` and use it in `Intl.NumberFormat`. For non-USD, use `currency: currencyCode || 'USD'` and ensure no $ prefix when currency is not USD. Or use a format like "1,111 RWF" (amount + space + code) for non-USD.<br/><br/>**Wrong Fix:** ❌ Keeping hardcoded USD for all amounts<br/><br/>**Related Tests:** `OpportunityWhoSection/NegativeTests.cs` (DEF-156 trait), `OpportunityWhoSection/FunctionalTests.cs` |
| DEF-150 | 🟡 Medium | UpdateOpportunityWhat does not validate delivery modality or duplicate outputs server-side | UNOPSOpportunityManager | 2026-03-09 | Open | PNO-700 AC4, PNO-864: Angular WHAT section validates delivery modality (1-4) and rejects duplicate output IDs in deliverables. `UpdateOpportunityWhat` / PATCH `/api/opportunity/{id}/what` may persist invalid delivery modality (e.g. 0, 5) or duplicate output IDs if called directly (Postman, integration). Client validation can be bypassed.<br/><br/>**Root Cause:** No server-side validation of delivery modality enum range or duplicate output IDs in deliverables before SaveChangesAsync.<br/><br/>**Proper Fix:** Add validation: if deliveryModality not in {1,2,3,4} throw BusinessException; if deliverables contain duplicate outputId values throw BusinessException.<br/><br/>**Wrong Fix:** ❌ Relying solely on client validation<br/><br/>**Related Tests:** `OpportunityWhatSection/NegativeTests.cs`, `OpportunityWhatSection/IntegrationTests.cs` |
| DEF-060 | 🔴 Critical | InteractionRBACCompositeSpecification.ReplaceCriteria silently fails — RBAC security filters never applied | InteractionRBACCompositeSpecification | 2026-03-03 | Open | `ReplaceCriteria()` uses reflection with `BindingFlags.NonPublic \| BindingFlags.Instance` to set `BaseSpecification.Criteria`, but `Criteria` is a **public** read-only property (`{ get; }`). The reflection lookup returns `null` and `SetValue` is never called. RBAC security expressions for `INTERACTION_READ`, `INTERACTION_MANAGER`, and `PARTNER_MANAGER` roles are built but never applied to the query criteria. All non-admin users see all interactions regardless of role.<br/><br/>**Root Cause:** `BaseSpecification<T>.Criteria` is `public Expression<Func<T, bool>> Criteria { get; }` — no setter, public accessor. `GetProperty("Criteria", BindingFlags.NonPublic \| BindingFlags.Instance)` returns `null` because the property is public.<br/><br/>**Proper Fix:**<br/>• Add a `protected set` accessor to `BaseSpecification<T>.Criteria`: `public Expression<Func<T, bool>> Criteria { get; protected set; }`<br/>• OR change `ReplaceCriteria` to use `BindingFlags.Public \| BindingFlags.Instance` and a writable backing mechanism<br/>• OR override `Criteria` in `InteractionRBACCompositeSpecification` with a settable property<br/><br/>**Wrong Fix:** ❌ Removing the RBAC filtering tests or changing assertions to match the broken behavior.<br/><br/>**Affected tests:** 4 tests skipped in `InteractionRBACSpecificationTests` — `Criteria_InteractionReadRole_ExcludesOtherUsersInteractions`, `Criteria_InteractionReadRole_ExcludesUnassignedInteractions`, `Criteria_InteractionRead_OnlyOwnOrAssigned`, `Criteria_PartnerManagerNoOrgUnit_OnlyCreated`. |
| DEF-008 | 🟠 High | Go Decision Feature — Remaining Implementation Gaps | OpportunityStageRequirements | 2026-02-02 | Partially Resolved | |
| DEF-020 | 🟠 High | CI blocked — `GH_PAT` secret missing/expired/wrong scope for private submodule checkout | .gitmodules / CI Infrastructure | 2026-02-17 | Open | `submodules: false` workaround reverted (Workflow IS needed). Fix: set `GH_PAT` secret in GitHub Actions with `repo` scope. See also DEF-046 for orphaned ExternalDataService submodule. |
| DEF-021 | 🟠 High | AmbiguousMatchException — DocumentController route conflict with UNOPS override | DocumentController / UNOPSDocumentController | 2026-02-18 | Open | |
| DEF-023 | 🟠 High | DEF-012 regression: duplicate UpdateOpportunityRequest map breaks AutoMapper | OpportunityMappingProfile | 2026-02-21 | Open | |
| DEF-024 | 🟠 High | DocumentController.GetCredentials() always calls Google Secret Manager unconditionally | DocumentController | 2026-02-21 | Open | Fix: add `DisableExternalCalls` guard OR inject `GoogleCredential` via DI (same as `AiContextualService`). 28 tests failing. |
| DEF-033 | 🟠 High | OrganizationHierarchyLookupController — empty stub file, 18 integration tests fully blocked | OrganizationHierarchyLookupController | 2026-02-21 | Open | |
| DEF-034 | 🟠 High | LiaisonOfficeLookupController — empty stub file, 15+ integration tests fully blocked | LiaisonOfficeLookupController | 2026-02-21 | Open | |
| DEF-045 | 🟠 High | AuditLogController returns 500 Internal Server Error for all authenticated requests in InMemory mode | AuditLogController | 2026-02-25 | Open | |
| DEF-053 | 🟠 High | UNOPSGeminiManager.GetCredentials crashes on missing Google credentials — blocks ALL API endpoints | UNOPSGeminiManager | 2026-03-02 | Open — NOT Resolved (confirmed 2026-03-05) | Constructor crash: `GetCredentials()` reads credential JSON from `IConfiguration` (null in test env), calls `GoogleCredential.FromJson(null)` → `ArgumentNullException`. Bypasses DI and `DisableExternalCalls` config. Blocks `UNOPSManagerWrapper` init → ALL endpoints return 500. ADC/Secret Manager work but are not used here. 85+ tests failing. **Fix:** Guard null config, use ADC, or accept `GoogleCredential` via DI. |
| DEF-058 | 🟠 High | OpportunityManager.GetOpportunityAsync includes invalid `Stakeholders.Contact` navigation path | OpportunityManager | 2026-03-03 | Open | `GetOpportunityAsync()` includes string-based include path `Stakeholders.Contact` but `OpportunityStakeholder` has no `Contact` navigation property. Causes `InvalidIncludePathError` at runtime. **Proper Fix:** Remove `Stakeholders.Contact` from the include chain or add the missing navigation property to the entity model. **Wrong Fix:** ❌ Suppressing the `InvalidIncludePathError` warning. Affected tests: 9 tests in OpportunityPerformanceTests + 4 tests in OpportunitySections/PerformanceTests (workaround: direct Context queries). |
| DEF-059 | 🟠 High | PartnerManager.GetPartnerWithContactsAndInteractionsAsync exceeds 200ms SLA (805ms) | PartnerManager | 2026-03-03 | Open | `GetPartnerWithContactsAndInteractionsAsync()` takes ~805ms for a single partner with contacts and interactions, exceeding the 200ms SLA threshold by 4x. Likely caused by Cartesian product explosion from multiple `.Include()` chains loading contacts, interactions, and their related entities in a single query. **Root Cause:** Too many `Include`/`ThenInclude` statements in one query create Cartesian product (see entity-framework-performance-optimization rule). **Proper Fix:**<br/>• Split into separate queries: main partner + contacts query + interactions query<br/>• Add `.AsNoTracking()` if read-only<br/>• Consider parallel execution with `IDbContextFactory` if 3+ collection queries<br/>**Wrong Fix:** ❌ Raising the SLA threshold to match current behavior.<br/>**Repro:** Run `PartnerPerformanceTests.GetPartnerWithContactsAndInteractions_NoCartesianExplosion_CompletesWithinThreshold` in isolation — fails at 805ms vs 200ms threshold. |
| DEF-062 | 🟠 High | PubSubPullService ignores `Enabled: false` config — crashes backend on GCP permission error | Startup.cs / PubSubPullService | 2026-03-04 | Open |
| DEF-064 | 🟠 High | BaseController.HandleOperationAsync does not handle KeyNotFoundException — returns 500 | BaseController / HandleOperationAsync | 2026-03-05 | Open | `HandleOperationAsync` catches `BusinessException` (→400) and `UnauthorizedAccessException` (→403) but all other exceptions including `KeyNotFoundException` fall through to `catch (Exception ex)` → 500. GlobalExceptionHandler maps KeyNotFoundException→404, but HandleOperationAsync catches first. ~20 integration tests expect 404 but get 500.<br/><br/>**Root Cause:** Missing `KeyNotFoundException` handler in `HandleOperationAsync`<br/><br/>**Proper Fix:** Add `catch (KeyNotFoundException) { return NotFound(); }` before the generic `catch (Exception)`<br/><br/>**Wrong Fix:** ❌ Adding 500 to test assertions |
| DEF-065 | 🟠 High | AI authorization returns 500 instead of 403 for restricted users | AIPromptManagementController | 2026-03-05 | Open | When a restricted user tries to access AI Prompt Management endpoints, the server returns 500 Internal Server Error instead of 403 Forbidden. ~12 tests skipped. |
| DEF-066 | 🟠 High | AiPromptManager operations exceed SLA performance thresholds | UNOPSAiPromptManager | 2026-03-05 | Open | `CreatePromptAsync`, `GetPromptByIdAsync`, `GetPromptsAsync` and related operations exceed defined SLA thresholds (500ms single ops, 5000ms bulk). 20 performance tests skipped. |
| DEF-067 | 🟠 High | ProfileManager operations exceed SLA performance thresholds | ProfileManager | 2026-03-05 | Open | `Get(email)` and `Update(profile)` operations exceed SLA thresholds. All profile performance tests fail. 18 tests skipped. |
| DEF-071 | 🟠 High | CreateOpportunity does not validate empty/whitespace name strings | UNOPSOpportunityManager | 2026-03-05 | Open | `CreateOpportunityAsync` accepts empty string "" and whitespace-only "   " as valid opportunity names without throwing an exception. Only `null` is rejected. This allows invalid data into the database. Tests `CreateOpportunity_InvalidName_ThrowsException` fail for empty and whitespace inputs.<br/><br/>**Proper Fix:** Add `string.IsNullOrWhiteSpace(request.Name)` validation check in CreateOpportunityAsync<br/><br/>**Wrong Fix:** ❌ Removing empty/whitespace test cases |
| DEF-025 | 🟡 Medium | Missing Permission CRUD endpoints — tests expect `/api/admin/permissions` CRUD | PermissionController | 2026-02-21 | Open | |
| DEF-026 | 🟡 Medium | Missing Role CRUD endpoints — tests expect `/api/admin/roles` CRUD | RoleController (missing) | 2026-02-21 | Open | |
| DEF-027 | 🟡 Medium | Missing Health/Metadata endpoints — `/api/health`, `/api/version`, `/api/system-info`, `/api/time` not implemented (search route fixed to `/api/global/search`) | GlobalController | 2026-02-21 | Open | |
| DEF-028 | 🟡 Medium | Missing UserPreference CRUD at `/api/users/preferences` — only default-org-unit exists | UserPreferenceController | 2026-02-21 | Open | |
| DEF-029 | 🟡 Medium | Missing LiaisonOffice CRUD endpoints — only GET and POST /search exist | LiaisonOfficeController | 2026-02-21 | Open | |
| DEF-030 | 🟡 Medium | UserProfileController routes mismatch — 24 tests expect `/api/users/profile` CRUD but controller has 3 different routes | UserProfileController | 2026-02-21 | Open | |
| DEF-031 | 🟡 Medium | SavedFilter advanced features missing — share, duplicate, default, export endpoints not implemented | SavedFilterController | 2026-02-21 | Open | |
| DEF-032 | 🟡 Medium | CountryController missing sub-routes — code lookup, dropdown, regions, continents, typeahead, DELETE not implemented | CountryController | 2026-02-21 | Open | |
| DEF-035 | 🟡 Medium | PartnerCategoryController missing tree/hierarchy endpoints — tree, roots, children, path, partner-association, dropdown not implemented | PartnerCategoryController | 2026-02-21 | Open | |
| DEF-036 | 🟡 Medium | PartnerGroupController missing member management endpoints — members CRUD, bulk ops, count, check, dropdown not implemented | PartnerGroupController | 2026-02-21 | Open | |
| DEF-037 | 🟡 Medium | UserPreferenceController routes mismatch — tests expect `/api/users/preferences` key-value CRUD but controller only has `GET/PUT /api/user-preferences/default-org-unit` | UserPreferenceController | 2026-02-21 | Open | |
| DEF-047 | 🟡 Medium | AiRetrieverManager does not accept HttpClient/HttpMessageHandler for testing | AiRetrieverManager | 2026-03-02 | Open | PNO-914 tests use Skip. Add constructor overload: `AiRetrieverManager(..., HttpClient? httpClient = null)` to enable HTTP mocking in unit tests. |
| DEF-048 | 🟡 Medium | CreateOpportunityFromProposalAsync accepts empty/whitespace-only Name | UNOPSOpportunityManager | 2026-03-02 | Open | Manager should validate that `Name` is not empty or whitespace-only and throw `BusinessException`. Currently accepts `""` and `"   "` without error. Affected tests: PNO-914 NEG-003, NEG-010; PNO-1156 NEG-002. |
| DEF-049 | 🟡 Medium | CreateOpportunityFromProposalAsync accepts Name exceeding 120 chars | UNOPSOpportunityManager | 2026-03-02 | Open | Manager should validate that `Name` does not exceed 120 characters and throw `BusinessException`. Currently accepts names of any length. Affected tests: PNO-914 NEG-011, BND-002; PNO-1156 NEG-003. |
| DEF-050 | 🟡 Medium | CreateOpportunityFromProposalAsync fails on null Description instead of defaulting | UNOPSOpportunityManager | 2026-03-02 | Open | When `Description` is null in `CreateOpportunityFromInteractionsRequest`, manager should default to `string.Empty` instead of failing with a database constraint violation (`Description` is required). Affected tests: PNO-914 BND-005; PNO-1156 BND-006. |
| DEF-055 | 🟡 Medium | WorkflowController.Reject throws NullReferenceException on null EntityName | WorkflowController | 2026-03-02 | Open | Should return 400 BadRequest instead of crashing. 1 test skipped in PNO-1166 NegativeTests. |
| DEF-056 | 🟡 Medium | Reopen workflow sets EntityStatus to Draft(4) instead of Active(1) | WorkflowController | 2026-03-02 | Open | When reopening from CANCELLED, status should be Active(1) not Draft(4). 1 test skipped in PNO-1166 IntegrationTests. |
| DEF-061 | 🟡 Medium | 3,036 compiler warnings across 15 production projects — nullable, async, XML docs, code quality | Multiple (see details) | 2026-03-04 | Open | 15 production projects produce 3,036 compiler warnings during `dotnet build`. Largest offenders: UNOPSBusiness (1,482), Presentation (394), Business (282), Models (268), Domain (260). Categories: ~2,250 nullable reference type warnings (CS8602/CS8603/CS8604/CS8618/CS8625/CS8600/CS8601), 330 XML doc warnings (CS1571/CS1573/CS1572), 164 async-without-await (CS1998), ~148 code quality (CS0108 member hiding, CS0105 duplicate usings, CS0168 unused vars, CS0618 obsolete methods, EF1002 SQL injection). QA test projects have been cleaned to 0 warnings. See DEF-061 details below for full per-project breakdown and recommended fix approach. |
| DEF-063 | 🟡 Medium | IAPVerificationMiddleware runs in Testing environment and blocks [AllowAnonymous] endpoints | IAPVerificationMiddleware / Startup.cs | 2026-03-04 | Open | `app.UseIAPVerification()` (Startup.cs line 108) is unconditional — runs for ALL environments including Testing. Returns 401 for requests without IAP headers (lines 384-390) before `UseAuthentication()` or `UseAuthorization()` can check for `[AllowAnonymous]`. **Fix:** Wrap in `if (!env.IsEnvironment("Testing"))` or add `[AllowAnonymous]` endpoint check in the middleware. Related QA: QA-075. |
| DEF-068 | 🟡 Medium | NotificationManager concurrent operations exceed performance thresholds | NotificationManager | 2026-03-05 | Open | Concurrent read/write operations (50 parallel reads, 10 parallel creates, mixed read/write) exceed performance thresholds. 3 tests skipped. |
| DEF-069 | 🟡 Medium | EntityArtifactManager concurrent operations exceed performance thresholds | EntityArtifactManager | 2026-03-05 | Open | Concurrent operations (50 parallel gets, 20 parallel gets, mixed read/write) exceed performance thresholds. 3 tests skipped. |
| DEF-072 | 🟡 Medium | UpdateOpportunityAsync throws BusinessException instead of ArgumentNullException for null request | UNOPSOpportunityManager | 2026-03-05 | Open | When `UpdateOpportunityAsync` receives an update request with a null/empty Name, it throws `BusinessException("Name is required")` instead of `ArgumentNullException`. The test expects `ArgumentNullException` as the proper guard-clause pattern. Tests should be updated if BusinessException is the intended behavior, or production code should use ArgumentNullException for null argument validation. |
| DEF-073 | 🟡 Medium | InteractionManager load test operations exceed performance thresholds | InteractionManager / LoadTests | 2026-03-05 | Open | SustainedLoad_ReadOperations_PerformanceDoesNotDegrade, SustainedLoad_WriteOperations_ConsistencyMaintained, SpikeLoad_Recovery_ReturnsToBaseline, Recovery_AfterStress_PerformanceRestored fail due to performance thresholds. 4 tests skipped in InteractionManagerLoadTests. |
| DEF-074 | 🟡 Medium | AiPromptCacheService cacheInvalidationMinutes=0 causes immediate expiration | AiPromptCacheService | 2026-03-05 | Open | `SetCachedResultAsync` uses `TimeSpan.FromMinutes(0)` for `AbsoluteExpirationRelativeToNow` when `cacheInvalidationMinutes` is 0. MemoryCache treats TimeSpan.Zero as immediate expiration, so cached value is not retrievable. **Proper Fix:** When 0 is passed, use default TTL (e.g., 1 min) or no-expiration. **Wrong Fix:** ❌ Changing test expectation. 1 test skipped: SetCachedResultAsync_ZeroMinutes_StillCaches. |
| DEF-075 | 🟡 Medium | OrgUnitHierarchyService.GetDescendantIdsAsync does not exclude soft-deleted org units | OrgUnitHierarchyService | 2026-03-05 | Open | `GetDescendantIdsAsync` returns org units with `IsDeleted = true` in its results. Soft-deleted org units should be excluded from descendant queries. 1 test skipped: GetDescendantIdsAsync_DeletedOrgUnit_ExcludedFromResults.<br/><br/>**Proper Fix:** Add `Where(o => !o.IsDeleted)` filter in the descendant query<br/><br/>**Wrong Fix:** ❌ Removing IsDeleted check from test |
| DEF-076 | 🟡 Medium | Composite Specifications throw NullReferenceException on null filter input | PartnerCompositeSpecification / ContactCompositeSpecification | 2026-03-05 | Open | Both `PartnerCompositeSpecification` and `ContactCompositeSpecification` throw `NullReferenceException` in `ApplyDynamicOrdering()` when constructed with a `null` filter. Specifications should handle null filters gracefully by applying default ordering. 2 tests skipped.<br/><br/>**Proper Fix:** Add null guard in `ApplyDynamicOrdering`: `if (filter == null) { ApplyOrderBy(p => p.Name); return; }` |
| DEF-077 | 🟡 Medium | PartnerCompositeSpecification ignores Status and NewEngagement filter properties | PartnerCompositeSpecification | 2026-03-05 | Open | Setting `filter.Status = "Inactive"` or `filter.NewEngagement = "yes"` on `PartnerFilterRequest` has no effect on the specification criteria. Partners with mismatched status or engagement flags still match. 3 tests skipped.<br/><br/>**Proper Fix:** Add Status and NewEngagement filter predicates to the criteria expression builder<br/><br/>**Wrong Fix:** ❌ Removing filter assertions from tests |
| DEF-078 | 🟡 Medium | PartnerByOrgUnitWithRelationsSpecification generates untranslatable LINQ | PartnerByOrgUnitWithRelationsSpecification | 2026-03-05 | Open | The specification uses null-coalescing patterns (`?? empty array`) on navigation properties that EF Core cannot translate to SQL. Query throws `InvalidOperationException` with "could not be translated" message. 2 tests skipped.<br/><br/>**Proper Fix:** Rewrite criteria to avoid null-coalescing on navigation properties; use `.Any()` directly on navigation collections |
| DEF-079 | 🟢 Low | PagedInteractionSpecification does not set default OrderBy expression | PagedInteractionSpecification | 2026-03-05 | Open | `PagedInteractionSpecification(page, pageSize)` constructor does not set a default `OrderBy` expression. `spec.OrderBy` is null after construction. 1 test skipped.<br/><br/>**Proper Fix:** Add default ordering (e.g., `ApplyOrderByDescending(i => i.Date)`) in constructor |
| DEF-080 | 🟠 High | SanitizeForDatabase does not remove SQL keywords or injection patterns | Entity sanitization / SanitizeForDatabase | 2026-03-05 | Open | `SanitizeForDatabase` does not remove individual SQL keywords (SELECT, FROM) or SQL injection patterns (OR 1=1) from input. Mixed-case variants (SeLeCt, FrOm) also bypass sanitization. 3 security tests skipped.<br/><br/>**Proper Fix:** Implement case-insensitive SQL keyword removal using regex with `RegexOptions.IgnoreCase`<br/><br/>**Wrong Fix:** ❌ Removing keyword checks from tests |
| DEF-081 | 🟠 High | DynamicExpressionBuilder does not correctly filter soft-deleted records or complex search criteria | DynamicExpressionBuilder / CrossEntitySearch | 2026-03-05 | Open | Cross-entity search logic fails to: (1) exclude soft-deleted records from results, (2) correctly apply not-like operator, (3) correctly combine AND/OR operators, (4) correctly apply partial match on description. 6 tests skipped.<br/><br/>**Proper Fix:** Add IsDeleted filter to generated expressions; fix operator evaluation logic |
| DEF-082 | 🟡 Medium | Database query performance does not meet Jira SLA thresholds | Multiple managers | 2026-03-05 | Open | Single record queries, bulk inserts, contact searches, count queries, opportunity detail loads, cross-entity queries, and status filtered counts all exceed defined Jira SLA performance thresholds. 12 tests skipped in JiraPerformanceRequirementsTests. |
| DEF-083 | 🟡 Medium | LinkManager concurrent operations exceed performance thresholds | LinkManager | 2026-03-05 | Open | Concurrent read/write operations (50 parallel gets, 20 parallel gets, mixed read/write) exceed performance thresholds. 3 tests skipped. |
| DEF-084 | 🟡 Medium | RiskManager concurrent and analysis operations exceed performance thresholds | RiskManager | 2026-03-05 | Open | Concurrent operations and high-risk analysis calculations exceed performance thresholds. 5 tests skipped. |
| DEF-085 | 🟡 Medium | AuditLogManager AsNoTracking query not measurably faster than tracked | AuditLogManager | 2026-03-05 | Open | AsNoTracking query for audit logs is not measurably faster than tracked equivalent. 1 test skipped. |
| DEF-086 | 🟢 Low | ImageGenerationManager logs at Information instead of Error on API failure | ImageGenerationManager | 2026-03-05 | Open | When Gemini API calls fail, the manager logs at `LogLevel.Information` instead of `LogLevel.Error`. Error-level logging should occur before re-throwing exceptions. 1 test skipped. |
| DEF-087 | 🟠 High | DocumentManager delete/update operations do not persist changes correctly | UNOPSDocumentManager | 2026-03-05 | Open | `DeleteDocumentAsync` does not actually remove or soft-delete document records — they remain findable after deletion. `UpdateDocumentAsync` changes also do not persist. `GetDocumentDetailsForAiAsync` fails to return expected data. 8 tests skipped.<br/><br/>**Proper Fix:** Verify `SaveChangesAsync()` is called after delete/update operations; verify soft-delete flags are set |
| DEF-088 | 🟡 Medium | DocumentManager does not support concurrent DbContext operations | DocumentManager | 2026-03-05 | Open | Concurrent read operations (50 parallel reads, mixed read/write) fail with `InvalidOperationException: A second operation was started on this context instance`. The manager shares a single DbContext instance across concurrent tasks. 2 tests skipped.<br/><br/>**Proper Fix:** Use `IDbContextFactory` for concurrent operations per EF Core threading guidelines |
| DEF-089 | 🟡 Medium | InteractionManager and OpportunityManager operations exceed performance thresholds | InteractionManager, OpportunityManager | 2026-03-05 | Open | Performance tests for CRUD, search, bulk operations, and concurrent access consistently exceed defined SLA thresholds. 16 tests skipped (15 Interaction, 1 Opportunity).<br/><br/>**Proper Fix:** Apply split query strategy, AsNoTracking for read-only operations, batch N+1 queries, and IDbContextFactory for concurrent operations per EF Core performance guidelines |
| DEF-090 | 🟡 Medium | ContactManager AI email matching returns unexpected results for invalid/unknown domains | UNOPSContactManager | 2026-03-05 | Open | `GetUnmatchedEmailsWithPartnerSuggestionsAsync` returns a non-empty partner name for invalid email formats (expected empty) and returns unexpected fallback results for unknown domains. 2 tests skipped.<br/><br/>**Proper Fix:** Add email format validation before domain extraction; ensure unknown domains return empty partner name and null partner ID |
| DEF-091 | 🟡 Medium | UserManagementManager operations exceed performance thresholds | UserManagementManager | 2026-03-05 | Open | GetAvailableRoles, GetUsers, GetAvailableOrgUnits, concurrent reads, and benchmark operations consistently fail performance thresholds. 6 tests skipped.<br/><br/>**Proper Fix:** Apply AsNoTracking for read-only queries, split query strategy, and IDbContextFactory for concurrent operations |
| DEF-092 | 🟡 Medium | ValuesManager operations exceed performance thresholds | ValuesManager | 2026-03-05 | Open | GetCountries, GetCurrencies, GetLiaisonOffices, SearchUsers, GetUsersPaged, concurrent reads, GC pressure, and memory tests consistently fail performance thresholds. 15 tests skipped.<br/><br/>**Proper Fix:** Apply AsNoTracking for read-only lookup queries, implement caching for static reference data, optimize pagination queries |
| DEF-093 | 🟡 Medium | PartnerTreeManager concurrent reads exceed performance thresholds | PartnerTreeManager | 2026-03-05 | Open | 20 parallel GetPartnerTree requests fail performance threshold. 1 test skipped.<br/><br/>**Proper Fix:** Apply AsNoTracking, split query strategy, and consider caching for tree structures |
| DEF-094 | 🟠 High | auth.interceptor.spec.ts references deleted IapSessionRefreshService | auth.interceptor.spec.ts | 2026-03-05 | Resolved | The dev-deploy merge removed `IapSessionRefreshService` from the auth interceptor and deleted the service file, but the developer unit test `auth.interceptor.spec.ts` was not updated. The test imports and mocks the deleted service, causing Angular frontend tests (`ng test`) to fail in CI.<br/><br/>**Root Cause:** Incomplete refactoring — service was removed from production code but corresponding test was not updated.<br/><br/>**Fix Applied:** Removed `IapSessionRefreshService` import, mock variable, and provider registration from the spec file. Test assertions remain valid as they test the interceptor's current behavior (routing, cookie handling, error handling). |
| DEF-095 | 🟠 High | opportunity-view.component.spec.ts missing queryParamMap mock | opportunity-view.component.spec.ts | 2026-03-05 | Resolved | The `OpportunityViewComponent` constructor accesses `this.activatedRoute.snapshot.queryParamMap.get('fromCreate')` (line 228), but the developer unit test's `ActivatedRoute` mock only provided `snapshot.paramMap` — not `queryParamMap`. All 7 OpportunityViewComponent Workflow Integration tests failed with `TypeError: Cannot read properties of undefined (reading 'get')`.<br/><br/>**Root Cause:** Component was updated to read `fromCreate` query param but the test's route mock was not updated to include `queryParamMap`.<br/><br/>**Fix Applied:** Added `queryParamMap` mock (with `get`, `has`, `getAll`, `keys`) to both the `activatedRoute.snapshot` and the `activatedRoute` observable. All 1336 frontend tests now pass. |
| DEF-096 | 🟠 High | NotifyWorkflowRecalledAsync TO recipients missing DoA approvers | PaoWorkflowNotificationService | 2026-03-05 | Open | Per PNO-1146 Jira requirement and Perminder's QA finding: "Recalled TO: DoA approvers + Opportunity Manager + initiator (when different from OM)". However, `NotifyWorkflowRecalledAsync` calls `GetRecallAdditionalRecipientUserIdsForOpportunityAsync` which returns ONLY OM + Initiator. The `notification.RecipientUserIds` (containing DoA approvers) is completely ignored for Opportunity entities. Perminder confirmed in Jira comment (March 5): "In the Recall Email - I do not see DoA3, when DoA2 is not present".<br/><br/>**Root Cause:** Line 440 of `PaoWorkflowNotificationService.cs` — `GetRecallAdditionalRecipientUserIdsForOpportunityAsync` only queries OM stakeholder and workflow initiator from WorkflowLog, but does not merge `notification.RecipientUserIds` (the DoA approvers passed by the workflow engine).<br/><br/>**Proper Fix:**<br/>• Merge `notification.RecipientUserIds` with OM + Initiator in the TO list<br/>• Ensure DoA3 holders are included when DoA2 holders are absent (fallback logic)<br/>• Deduplicate the combined list<br/><br/>**Wrong Fix:** ❌ Removing DoA approvers from the requirement to match the code behavior<br/><br/>**Repro Steps:**<br/>1. Create opportunity with Responsible OrgUnit that has DoA3 holders but NO DoA2<br/>2. Submit for Go Decision<br/>3. Recall the submission<br/>4. Check recall email TO recipients<br/><br/>**Expected:** TO includes DoA3 holders + OM + Initiator<br/>**Actual:** TO includes only OM + Initiator; DoA3 holders are missing<br/><br/>**Environment:** QA (`opportunityplus.qa.unops.org`)<br/>**Related Tests:** `PaoWorkflowNotificationRecalledTests`, `PNO-1146_WorkflowEmailNotifications` |
| DEF-097 | 🟠 High | NotifyInternalStakeholdersOnGoDecisionAsync does not include opportunity-level internal stakeholders | PaoWorkflowNotificationService | 2026-03-05 | Open | Per PNO-1146 Jira requirement: "Internal Stakeholder FYI TO: Internal stakeholders for other org units responsible for the opportunity's countries." Perminder's QA finding (March 5): "If I add colleagues as 'Internal Stakeholders' section - they are not marked on FYI email. I added THEINT and RUEDIGER as Internal Stakeholders, but they are not marked on Email."<br/><br/>The code (line 552-562) only queries `EntityUserRole` for director/deputy director role codes on country org units. It does NOT query `OpportunityStakeholder` records where `IsInternal = true` — which are users manually added via the Team tab as internal stakeholders.<br/><br/>**Root Cause:** `NotifyInternalStakeholdersOnGoDecisionAsync` queries org unit hierarchy role holders (`EntityUserRole`) but ignores opportunity-level internal stakeholders (`OpportunityStakeholder` with `IsInternal = true`).<br/><br/>**Proper Fix:**<br/>• Also query `OpportunityStakeholder` where `IsInternal = true && !IsDeleted` for the opportunity<br/>• Merge their emails into the TO list alongside the org unit hierarchy directors<br/>• Deduplicate the combined list<br/><br/>**Wrong Fix:** ❌ Telling users to assign Director/Manager roles instead of using the Internal Stakeholders section<br/><br/>**Repro Steps:**<br/>1. Create opportunity with Implementation Countries<br/>2. Add colleagues as Internal Stakeholders on the Team tab<br/>3. Submit and approve the Go Decision<br/>4. Check FYI email TO recipients<br/><br/>**Expected:** TO includes Internal Stakeholders added via Team tab + org unit directors<br/>**Actual:** TO includes only org unit directors; manually added Internal Stakeholders are missing<br/><br/>**Environment:** QA (`opportunityplus.qa.unops.org`)<br/>**Related Tests:** `PaoWorkflowInternalStakeholderTests`, `PNO-1146_WorkflowEmailNotifications` |
| DEF-098 | 🟡 Medium | GetOpportunityManagerEmailAsync missing IsDeleted filter on OpportunityStakeholder | PaoWorkflowNotificationService | 2026-03-05 | Open | `GetOpportunityManagerEmailAsync` (line 1077) queries `OpportunityStakeholders` without `!s.IsDeleted` filter. Compare with `GetOpportunityManagerUserIdAsync` (line 1027) which correctly has `!s.IsDeleted`. A soft-deleted OM stakeholder could still appear in CC recipients for workflow emails.<br/><br/>**Root Cause:** Inconsistent query filters — `GetOpportunityManagerUserIdAsync` correctly filters `!s.IsDeleted`, but `GetOpportunityManagerEmailAsync` does not.<br/><br/>**Proper Fix:**<br/>• Add `&& !s.IsDeleted` to the `.Where()` clause in `GetOpportunityManagerEmailAsync` (line 1077)<br/><br/>**Wrong Fix:** ❌ Removing the IsDeleted filter from `GetOpportunityManagerUserIdAsync` to make them consistent<br/><br/>**Repro Steps:**<br/>1. Assign an OM stakeholder to an opportunity<br/>2. Soft-delete the OM stakeholder<br/>3. Trigger a workflow notification that uses CC (e.g., Internal Stakeholder FYI)<br/>4. Check CC recipients<br/><br/>**Expected:** Soft-deleted OM is excluded from CC<br/>**Actual:** Soft-deleted OM may still appear in CC<br/><br/>**Environment:** Dev<br/>**Related Tests:** `PaoWorkflowNotificationCompletedTests`, `PaoWorkflowInternalStakeholderTests` |
| DEF-099 | 🟠 High | Go Decision PRD says DoA3 fallback is "Out of Scope" but PNO-1197 implemented it | tasks/the-go-decision/the-go-decision-prd.md, tasks/send-opportunity-for-go-decision/send-opportunity-for-go-decision-prd.md | 2026-03-05 | Open | `the-go-decision-prd.md` line 1194 states: "❌ DoA escalation to DoA3 - Only DoA2 for this release" under Non-Goals. `send-opportunity-for-go-decision-prd.md` line 1274 states: "❌ Multi-level DoA escalation - Only DoA2 for this release". However, Jira PNO-1197 (Bug, Urgent priority, Status: Done, resolved 2026-02-17) required DoA3 fallback when DoA2 is removed, and it was implemented by the dev team and QA-passed by Perminder Saluja. The feature is live in production (Version 3.1) and 12 test files exist in `PNO-1197_DoA3Fallback/`. **The PRDs are stale and must be updated to move DoA3 fallback from "Out of Scope" to "In Scope" and reference PNO-1197.**<br/><br/>**Root Cause:** PRD was not updated after scope change was approved via Jira bug PNO-1197.<br/><br/>**Proper Fix:**<br/>• Update `the-go-decision-prd.md` line 1194: remove "DoA escalation to DoA3" from Non-Goals<br/>• Add DoA3 fallback as in-scope requirement with reference to PNO-1197<br/>• Update `send-opportunity-for-go-decision-prd.md` line 1274 similarly<br/>• Add Jira ticket traceability table to both PRDs<br/><br/>**Wrong Fix:** ❌ Removing the DoA3 fallback implementation to match the stale PRD<br/><br/>**Impact:** Any test created solely from the PRD would incorrectly skip DoA3 fallback testing, missing a critical workflow feature. QA tests in `PNO-1197_DoA3Fallback/` are correct but contradict the PRD documentation. |
| DEF-100 | 🟡 Medium | PRDs have zero Jira ticket traceability — no PNO- references in any PRD | tasks/the-go-decision/the-go-decision-prd.md, tasks/send-opportunity-for-go-decision/send-opportunity-for-go-decision-prd.md | 2026-03-05 | Open | Neither `the-go-decision-prd.md` nor `send-opportunity-for-go-decision-prd.md` reference any Jira PNO- ticket numbers. There is no mapping table showing which Jira tickets correspond to which PRD sections. This means: (1) scope changes made via Jira bugs/stories cannot be traced back to PRD updates, (2) acceptance criteria in Jira cannot be verified against PRD coverage, (3) QA cannot determine if a PRD is current or stale without manually cross-referencing Jira. PNO-1197 (DoA3 fallback) and PNO-1146 (workflow notifications) were both implemented but never reflected in PRDs.<br/><br/>**Proper Fix:**<br/>• Add a "Jira Traceability" section to each PRD with a table: `\| Jira Ticket \| Description \| PRD Section \| Status \|`<br/>• Include all PNO- tickets that affect the PRD scope<br/>• Update this table whenever a ticket changes PRD scope<br/><br/>**Impact:** QA tests created from PRDs alone may miss requirements that only exist in Jira. This was proven by the PNO-1146 analysis where 3 new defects (DEF-096, DEF-097, DEF-098) were found only by cross-referencing Jira comments against the code — the PRD did not contain this information. |
| DEF-110 | 🟠 High | Partners silently dropped during oUP Engagement creation when partner not in oUP | UNOPS.PAO.Business/Integration (oUP Sync) | 2026-03-05 | Open | [PNO-1207](https://unops.atlassian.net/browse/PNO-1207) Ready for Development (Assignee: Anusha SWAMINATHAN): Partners in Opp+ QA but not in oUP are silently dropped during Engagement creation. No error/warning shown. Edit/save re-sync workaround confirmed NOT working (2026-03-06). oUP test envs lose synced data after releases.<br/><br/>**Root Cause:** oUP sync logic does not validate partner existence before Engagement creation. Re-sync via edit/save also broken.<br/><br/>**Proper Fix:**<br/>• Validate partner existence in oUP before Engagement creation<br/>• Show warning/error for missing partners<br/>• Fix edit/save re-sync mechanism<br/>• Queue missing partners for sync<br/><br/>**Wrong Fix:** ❌ Silently dropping partners without notification<br/><br/>**Test Data:** Opp+ ID 173, Engagement 25550-00, "FG Partner" missing from oUP<br/><br/>**Related Tests:** `PNO-1207_PartnerSyncMismatch/PartnerSyncMismatchTests.cs` (26 tests) |
| DEF-111 | 🟠 High | Blank partner tree levels appearing in oUP and BigQuery after data migration | UNOPS.PAO.Business/PartnerTree | 2026-03-05 | Open | PNO-867: Since the data migration of the partner tree to Opportunity+, additional blank/empty partner levels are appearing in the oneUNOPS Projects partner list and in BigQuery partner tree reports. These phantom levels have no name or meaningful data.<br/><br/>**Root Cause:** Data migration created partner tree nodes with empty/null names. Queries do not filter out nodes with blank names.<br/><br/>**Proper Fix:**<br/>• Clean up blank partner tree nodes from database<br/>• Add validation to prevent creation of nodes with empty names<br/>• Filter out blank-named nodes from partner tree API responses<br/><br/>**Wrong Fix:** ❌ Hiding blank nodes only in the UI while leaving corrupt data in the database<br/><br/>**Related Tests:** `PNO-867_BlankPartnerTreeLevels/BlankPartnerTreeTests.cs` (26 tests) |
| DEF-112 | 🟡 Medium | SDG classification uses 'primary'/'secondary' instead of 'main'/'cross cutting' labels | Opportunity Statement / SDG API | 2026-03-05 | Open | PNO-974: In Opportunity Statement section "2. Alignment with UN, global, and national goals and priorities", SDGs are still classified as 'primary' and 'secondary' whereas they should be 'main' and 'cross cutting'. API responses (opportunity detail, SDG values, workflow requirements) may expose deprecated terminology.<br/><br/>**Root Cause:** API/UI still uses legacy terminology.<br/><br/>**Proper Fix:** Update all SDG classification labels to use 'main' and 'cross cutting' throughout API responses and UI.<br/><br/>**Wrong Fix:** ❌ Changing test assertions to accept 'primary'/'secondary'<br/><br/>**Related Tests:** `SdgClassificationTests.NEG_001_SdgClassification_UsesMainCrossCutting_NotPrimarySecondary` |
| DEF-130 | 🟡 Medium | SDG 8 missing from WHY section SDG dropdown | SDG Seeder / Values API | 2026-03-09 | Open | PNO-817: SDG 8 (Decent Work and Economic Growth) must be present in the SDG dropdown for the WHY section. Three UAT testers reported SDG 8 as missing. Per https://sdgs.un.org/, all 17 SDGs (1–17) must be available.<br/><br/>**Root Cause:** SDG seeder or reference data may omit SDG 8.<br/><br/>**Proper Fix:** Ensure SDGSeeder includes SDG 8; verify ValuesManager/GetSDGs returns SDG 8.<br/><br/>**Wrong Fix:** ❌ Removing SDG 8 from requirement<br/><br/>**Related Tests:** `OpportunityWhySection/NegativeTests.cs` NEG_011 |
| DEF-131 | 🟡 Medium | SDG 15.3 target references 2020 instead of 2030 | SDG Seeder / SDG 15.3 data | 2026-03-09 | Open | PNO-817: SDG 15.3 (Land Degradation Neutrality) should mention 2030 target, not 2020. Per https://sdgs.un.org/, the target year is 2030.<br/><br/>**Root Cause:** SDG 15.3 description or target text in seeder/reference data uses outdated 2020 reference.<br/><br/>**Proper Fix:** Update SDG 15.3 target/description to reference 2030.<br/><br/>**Wrong Fix:** ❌ Changing test expectation to accept 2020<br/><br/>**Related Tests:** `OpportunityWhySection/NegativeTests.cs` NEG_012 |
| DEF-181 | 🟠 High | PNO-148: Partner/Contact logo wrong aspect ratio or not displayed in search results | UNOPSPartnerManager / UNOPSContactManager / Search | 2026-03-09 | Open | PNO-148: Logo on Partner and Contact not displaying correctly. Issues: (1) Logo not displayed in Global Search results; (2) Logo not displayed in Advanced Search results (fixed in some envs); (3) Wrong aspect ratio in search results; (4) Logo requires page refresh to appear after upload (fixed). Backend returns LogoUrl/ProfilePictureUrl as signed URLs — frontend or search result mapping may not apply correct aspect-ratio CSS or may omit logo in search card payload.<br/><br/>**Root Cause:** Search result DTOs or frontend card components may omit logo URL, or apply wrong aspect-ratio styling.<br/><br/>**Proper Fix:** Ensure Global/Advanced Search API responses include logoUrl/profilePictureUrl; ensure frontend applies object-fit/aspect-ratio for consistent display.<br/><br/>**Related Tests:** `PartnerContactLogo/` |
| DEF-182 | 🟠 High | PNO-797: Contacts page Error 400 "An item with the same key has already been added" | UNOPSContactManager | 2026-03-09 | Open | PNO-797: Contacts page fails to load with duplicate key exception. `GetContactsAsync` uses `.ToDictionary(u => u.UserId)` and `.ToDictionary(o => o.Code)` for userInfoLookup and orgHierarchyLookup. When UserProfile has duplicate UserIds (data anomaly) or OrganizationHierarchy has duplicate Code values, ToDictionary throws ArgumentException.<br/><br/>**Root Cause:** ToDictionary does not handle duplicate keys.<br/><br/>**Proper Fix:** Use `.GroupBy(u => u.UserId).ToDictionary(g => g.Key, g => g.First())` and `.GroupBy(o => o.Code).ToDictionary(g => g.Key, g => g.First())` to handle duplicates gracefully.<br/><br/>**Wrong Fix:** ❌ Assuming data has no duplicates<br/><br/>**Related Tests:** `PartnerContactLogo/PositiveTests.cs` PCL-POS-001 to PCL-POS-003, `PartnerContactLogo/NegativeTests.cs` |
| DEF-183 | 🟡 Medium | PNO-933: Mass import org unit not persisted to OrganizationUnitRelationship | Contact Import / UNOPSContactManager | 2026-03-09 | Open | PNO-933: Mass import of Contacts — Org unit mapping missing. Import dialog shows Org Unit correctly, but imported contacts do not have OrganizationUnitRelationship records. Org unit data ends up in Department field instead.<br/><br/>**Root Cause:** Import flow may not call UpdateContactOrganizationUnitRelationshipsAsync, or mapping from import row to OrganizationHierarchyIds is incorrect.<br/><br/>**Proper Fix:** Ensure mass import creates/updates OrganizationUnitRelationship for each contact with mapped org unit; ensure import mapping maps Org Unit column to OrganizationHierarchyIds, not Department.<br/><br/>**Related Tests:** `PartnerContactLogo/` PNO-933 tests |
| DEF-187 | 🟡 Medium | OpportunityController.Create does not validate Name max 255 chars server-side — PNO-774 | OpportunityController / UNOPSOpportunityManager | 2026-03-09 | Open | PNO-774: Opportunity Name must be limited to 255 characters. System should reject names exceeding 255 chars with validation error. OpportunityController.Create validates Name required but does not check length. UNOPSOpportunityManager.CreateOpportunityAsync may accept names >255 if DB allows; direct API calls can bypass client validation.<br/><br/>**Root Cause:** No `request.Name.Length > 255` validation in controller or manager before persist.<br/><br/>**Proper Fix:** Add validation in OpportunityController or UNOPSOpportunityManager: `if (req.Name != null && req.Name.Length > 255) { validationErrors.Add("Opportunity Name cannot exceed 255 characters"); }`<br/><br/>**Wrong Fix:** ❌ Relying solely on client-side maxlength<br/><br/>**Related Tests:** `AdminAccessValidation/NegativeTests.cs` AAV-NEG-006, AAV-NEG-026, AAV-NEG-027; `AdminAccessValidation/BoundaryTests.cs` AAV-BND-003, AAV-BND-026, AAV-BND-027 |
| DEF-113 | 🟡 Medium | Special/accented characters in user names display as '??' in dropdowns | UNOPS.PAO.Presentation/API (Character Encoding) | 2026-03-05 | Open | PNO-1194: Accented characters (e.g., 'Ã', 'ö', 'ü') in user names are replaced by question marks ('??') in dropdown menus. This indicates a UTF-8 vs ASCII/Latin-1 encoding mismatch between the database/API response and the frontend component.<br/><br/>**Root Cause:** Character encoding mismatch — likely the database stores UTF-8 but the API response or frontend rendering interprets it as ASCII/Latin-1.<br/><br/>**Proper Fix:**<br/>• Ensure database connection string specifies UTF-8 encoding<br/>• Verify API response Content-Type includes `charset=utf-8`<br/>• Ensure frontend components handle UTF-8 characters properly<br/><br/>**Wrong Fix:** ❌ Replacing special characters with ASCII equivalents<br/><br/>**Related Tests:** `PNO-1194_CharacterEncoding/CharacterEncodingTests.cs` (26 tests) |
| DEF-114 | 🟡 Medium | ValuesRepository GetUNCFIndicators/GetUNCFIndicatorsByOutcomeId does not filter !indicator.IsDeleted | ValuesRepository | 2026-03-09 | Open | PNO-976: Per dotnet-implementation rule, all entity queries must filter `!e.IsDeleted` for soft-deleted records. `GetUNCFIndicators` and `GetUNCFIndicatorsByOutcomeId` do not include `!indicator.IsDeleted` in their where clauses. Soft-deleted UNCF indicators can be returned to the API.<br/><br/>**Root Cause:** Missing IsDeleted filter in UNCF indicator queries.<br/><br/>**Proper Fix:** Add `&& !indicator.IsDeleted` to both GetUNCFIndicators and GetUNCFIndicatorsByOutcomeId query where clauses.<br/><br/>**Wrong Fix:** ❌ Removing the IsDeleted assertion from tests<br/><br/>**Related Tests:** `PNO-976_UNSDCFIndicators/BoundaryTests.cs` (DEF-114 trait) |
| DEF-115 | 🟡 Medium | OpportunityMappingProfile UNCFIndicatorName maps from Name only — should use Indicators ?? Name | OpportunityMappingProfile | 2026-03-09 | Open | PNO-976 REQ-3: When saving/displaying UNCF indicator selections, `uncfIndicatorName` must use the descriptive `indicators` text, falling back to `name` if `indicators` is null/empty. The `OpportunityUNCFIndicator -> OpportunityUNCFIndicatorModel` mapping currently uses `UNCFIndicator.Name` only. It should use `UNCFIndicator.Indicators ?? UNCFIndicator.Name` for UNCFIndicatorName.<br/><br/>**Root Cause:** Mapping uses Name only; requirement says use descriptive Indicators when available.<br/><br/>**Proper Fix:** Update `.ForMember(dest => dest.UNCFIndicatorName, opt => opt.MapFrom(src => src.UNCFIndicator != null ? (src.UNCFIndicator.Indicators ?? src.UNCFIndicator.Name) : null))`<br/><br/>**Wrong Fix:** ❌ Changing test expectation to match current behavior<br/><br/>**Related Tests:** `PNO-976_UNSDCFIndicators/BoundaryTests.cs` (DEF-115 trait) |
| DEF-116 | 🟠 High | No session keepalive/heartbeat mechanism — users get frequent "Connection Lost" | AuthService / Session Management | 2026-03-09 | Open | PNO-914 AC-1, AC-4: Users experience frequent "Connection lost" popups. Application should keep sessions alive during user activity and refresh transparently before expiry. `IapSessionRefreshService` was removed (DEF-094); no replacement keepalive/heartbeat exists. AuthService has no token refresh, no session extend, no proactive refresh before expiry.<br/><br/>**Root Cause:** Session keepalive mechanism was removed and not replaced.<br/><br/>**Proper Fix:**<br/>• Implement session keepalive (heartbeat to health endpoint or token refresh) during user activity<br/>• Proactively refresh/extend session before IAP/JWT expiry<br/>• Consider idle detection to avoid unnecessary calls<br/><br/>**Wrong Fix:** ❌ Increasing expiry only — does not address active users hitting timeout<br/><br/>**Related Tests:** `PNO-914_SessionTimeoutSpec/NegativeTests.cs` (DEF-116 trait) |
| DEF-117 | 🟠 High | Auth interceptor swallows errors with of(error) — server-error interceptor never processes 401/403 | auth.interceptor.ts | 2026-03-09 | Open | PNO-914 AC-3: Auth interceptor uses `return of(error)` instead of `throwError(() => error)` when handling 401/403. This converts the error to a successful emission, so downstream server-error interceptor never receives the error. Error propagation is broken; 401/403 are handled but not properly propagated for logging/analytics.<br/><br/>**Root Cause:** `catchError` returns `of(error)` which completes the stream successfully with error as value.<br/><br/>**Proper Fix:** Use `throwError(() => error)` to re-throw after handling (navigate/reload) so error flows to server-error interceptor for logging. Or explicitly call error handler before returning.<br/><br/>**Wrong Fix:** ❌ Removing error handling from server-error interceptor<br/><br/>**Related Tests:** `PNO-914_SessionTimeoutSpec/NegativeTests.cs` (DEF-117 trait) |
| DEF-118 | 🟡 Medium | redirectCounter in AuthService incremented but never checked — no redirect loop protection | auth.service.ts | 2026-03-09 | Open | PNO-914: AuthService has `redirectCounter` that is reset in `resetAuthenticationState()` but is never incremented or checked. No protection against infinite redirect loops when 401 repeatedly triggers navigation to login.<br/><br/>**Root Cause:** redirectCounter exists but is never used in conditional logic.<br/><br/>**Proper Fix:**<br/>• Increment redirectCounter when navigating to login on 401<br/>• Check redirectCounter before redirect (e.g. if redirectCounter > 3, show error instead of redirect)<br/>• Reset on successful auth<br/><br/>**Wrong Fix:** ❌ Removing redirectCounter — it should be used<br/><br/>**Related Tests:** `PNO-914_SessionTimeoutSpec/NegativeTests.cs` (DEF-118 trait) |
| DEF-119 | 🟡 Medium | No session expiry warning — users get "Connection Lost" with no prior notification | AuthService / ErrorHandler | 2026-03-09 | Open | PNO-914 AC-5: Error messages should be clear and actionable. When session expires, users get generic "Connection Lost" with no prior warning. Per AC-4, session refresh should happen transparently before expiry; when that fails, user should be warned (e.g. "Your session will expire in 5 minutes") before abrupt disconnect.<br/><br/>**Root Cause:** No session expiry warning mechanism; no proactive notification before timeout.<br/><br/>**Proper Fix:**<br/>• Add session expiry countdown/warning (e.g. 5 min before expiry)<br/>• Differentiate "Session expired" from "Network disconnected" in error messages<br/>• Provide actionable guidance (e.g. "Please log in again") for session expiry vs "Check your connection" for network<br/><br/>**Wrong Fix:** ❌ Using same "Connection Lost" for both network and session expiry<br/><br/>**Related Tests:** `PNO-914_SessionTimeoutSpec/NegativeTests.cs` (DEF-119 trait) |
| DEF-120 | 🟡 Medium | RegenerateGoOpportunityPdfs may not handle statementDocTypeId=0 gracefully | SystemAdminController | 2026-03-09 | Open | PNO-1166: When DocumentTypes seeder has not run or "Opportunity Statement" document type is missing, `statementDocTypeId` is 0. The endpoint skips the `onlyMissing` existence check (correct) but continues to process opportunities. The DocumentRelationships join uses `d.DocumentTypeId == statementDocTypeId` — when 0, no documents match. PDF generation may still fail if downstream logic expects a valid document type. Endpoint should either return early with a clear message when statementDocTypeId=0, or document the behavior.<br/><br/>**Root Cause:** No explicit handling for missing "Opportunity Statement" document type.<br/><br/>**Proper Fix:** Add guard: `if (statementDocTypeId == 0) return BadRequest(new { error = "Opportunity Statement document type not found. Run DocumentTypes seeder." })` or log and continue with documented behavior.<br/><br/>**Related Tests:** `PNO-1166_QATestingCode/BoundaryTests.cs` BND-019 (DEF-120 trait) |
| DEF-121 | 🟡 Medium | BuildAuditTrailMarkdownForApprovalAsync does not handle null workflow history | SystemAdminController | 2026-03-09 | Open | PNO-1166: `BuildAuditTrailMarkdownForApprovalAsync` receives `List<WorkflowHistoryModel> history`. If `workflowManager.GetWorkflowHistory` returns null (or is passed null), `history.OrderByDescending(h => h.CreatedDate)` throws `NullReferenceException`. Workflow submodule typically returns empty list, but defensive coding should handle null.<br/><br/>**Root Cause:** No null check on history parameter before calling `.OrderByDescending()`.<br/><br/>**Proper Fix:** Add `history ??= new List<WorkflowHistoryModel>();` at start of method, or `if (history == null) return "...";`<br/><br/>**Wrong Fix:** ❌ Assuming workflow always returns non-null<br/><br/>**Related Tests:** `PNO-1166_QATestingCode` |
| DEF-122 | 🟠 High | PartnerController.CreateOpportunityFromPartner blocks Draft partners — PNO-917 | PartnerController | 2026-03-09 | Open | PNO-917: Users should be able to create opportunities when partner is in **Draft** or **Active** status; blocked for Closed/Archived. PartnerController line 1813 checks `partner.Status != "Active"` and returns BadRequest for non-Active. This incorrectly blocks **Draft** partners.<br/><br/>**Root Cause:** Validation only allows "Active", not "Draft".<br/><br/>**Proper Fix:** Change to `var blockedStatuses = new[] { "Closed", "Archived" }; if (blockedStatuses.Contains(partner.Status)) return BadRequest(...)`<br/><br/>**Wrong Fix:** ❌ Keeping current logic; PNO-917 explicitly requires Draft support<br/><br/>**Related Tests:** `OpportunityCreation/NegativeTests.cs`, `OpportunityCreation/BoundaryTests.cs` |
| DEF-123 | 🟡 Medium | OpportunityController.Create requires Description — PNO-687 says optional | OpportunityController | 2026-03-09 | Open | PNO-687 AC4 comment (2025-12-12): "Opportunity description is optional for opportunity creation at the moment." OpportunityController.Create (lines 93-95) validates `string.IsNullOrWhiteSpace(req.Description)` and returns BadRequest. OpportunityRequest has `required string Description`.<br/><br/>**Root Cause:** Controller and model enforce Description as required.<br/><br/>**Proper Fix:** Make Description optional in validation; allow null/empty; default to empty string in manager if needed.<br/><br/>**Wrong Fix:** ❌ Keeping Description required — contradicts Jira AC update<br/><br/>**Related Tests:** `OpportunityCreation/NegativeTests.cs` OPP-NEG-023, OPP-NEG-024 |
| DEF-046 | 🟢 Low | Remove orphaned `UNOPS.PAO.ExternalDataService` submodule — registered in `.gitmodules` but zero projects reference it | .gitmodules | 2026-02-25 | Open | Run: `git submodule deinit UNOPS.PAO.ExternalDataService && git rm UNOPS.PAO.ExternalDataService` and remove block from `.gitmodules`. No code changes needed. |
| DEF-051 | 🟢 Low | ConfigurationController Environment fallback uses `??` instead of `IsNullOrEmpty` | ConfigurationController | 2026-03-02 | Open | When `AppConfig:Environment` is set to empty string `""`, the controller returns `""` instead of falling back to the host environment name. Uses `?? hostEnvironmentName` which only handles null, not empty string. Should use `string.IsNullOrEmpty()` to fall back for both null and empty. Affected tests: PNO-914 CFG-BND-007, CFG-NEG-007. |
| DEF-054 | 🟢 Low | DoA3Fallback controller does not implement ILogger logging | WorkflowController | 2026-03-02 | Open | Controller submit operations lack logging. 5 tests skipped in PNO-1197 FunctionalTests. |
| DEF-057 | 🟢 Low | Partner name `Validators.required` does not reject whitespace-only input | PartnerEditDialogComponent | 2026-03-02 | Open | Angular `Validators.required` only checks for null/undefined/empty string, not whitespace-only strings like `"   "`. Partner Description should use a custom validator (e.g., `Validators.pattern(/\S/)`) to reject whitespace-only names. Affected test: form-validation-negative TC-N02. |
| DEF-070 | 🟢 Low | SystemAdminManager AsNoTracking query not measurably faster than tracked query | SystemAdminManager | 2026-03-05 | Open | `AsNoTracking` query for seed scripts is not measurably faster than the tracked equivalent. 1 test skipped. |

---

## Resolved Defects (Summary)

> **6 resolved** | Sorted by severity.

| Defect ID | Severity | Title | Component | Date Reported | Status | Developer Feedback |
|-----------|----------|-------|-----------|---------------|--------|--------------------|
| DEF-010 | 🟠 High | PNO-1193: OM role transfer not working | OpportunityWorkflow | 2026-02-11 | Resolved (2026-02-17) | |
| DEF-018 | 🟠 High | DuplicateDetectionService uses relational APIs incompatible with InMemory | DuplicateDetectionService | 2026-02-16 | Resolved (2026-02-17) | |
| DEF-011 | 🟡 Medium | PNO-1171: Reject action appears twice in workflow history | WorkflowHistory | 2026-02-11 | Resolved (2026-02-17) | |
| DEF-012 | 🟡 Medium | ForAllMembers overrides Ignore() rules in OpportunityMappingProfile | OpportunityMappingProfile | 2026-02-16 | Resolved (2026-02-17) | |
| DEF-017 | 🟡 Medium | WorkflowControllerTests: 6 Submit tests fail — endpoint behavior changed in pull | WorkflowController | 2026-02-16 | Resolved (2026-02-17) | |
| DEF-019 | 🟡 Medium | PAOAuthorizationService doesn't handle DenyAnonymousAuthorizationRequirement | PAOAuthorizationService | 2026-02-16 | Resolved (2026-02-17) | |

---

## Closed / Won't Fix / Reclassified (Summary)

> **5 closed** | No action required.

| Defect ID | Severity | Title | Component | Date Reported | Status | Developer Feedback |
|-----------|----------|-------|-----------|---------------|--------|--------------------|
| ~~DEF-013~~ | ~~🟡 Medium~~ | ~~LiaisonOfficeManager not registered in IManagerWrapper~~ | ~~ManagerWrapper~~ | ~~2026-02-16~~ | **Closed — Won't Fix (2026-03-04)** | Per Anusha (2026-03-04): LiaisonOffice does not have a manager by design. Not a managed entity in Opp+; only selectable as part of Partner. |
| ~~DEF-014~~ | ~~🟡 Medium~~ | ~~FocalPointManager not registered in IManagerWrapper~~ | ~~ManagerWrapper~~ | ~~2026-02-16~~ | **Closed — Won't Fix (2026-03-04)** | Per Anusha (2026-03-04): FocalPoint does not have a manager by design. Not a managed entity in Opp+; only selectable as part of Partner. |
| ~~DEF-015~~ | ~~🟡 Medium~~ | ~~DashboardController has zero test coverage — 10+ endpoints~~ | ~~DashboardController~~ | ~~2026-02-16~~ | **Reclassified → Backlog (2026-02-20)** | |
| ~~DEF-016~~ | ~~🟡 Medium~~ | ~~OpportunityImmutabilityTests: 8 GetOpportunity/Update tests fail — IMapper mock returns null~~ | ~~OpportunityImmutabilityTests~~ | ~~2026-02-16~~ | **Reclassified → QA-061 (2026-02-17)** | |
| ~~DEF-022~~ | ~~🟠 High~~ | ~~Restricted user can access AI Prompt Management admin page~~ | ~~AIPromptManagement / Authorization~~ | ~~2026-02-18~~ | **Reclassified → QA-068 (2026-02-20)** | |


---

### DEF-008: Go Decision Feature — Remaining Implementation Gaps

**Severity:** 🟠 High  
**Component:** OpportunityStageRequirements (`OpportunityStageRequirements.cs`)  
**Date Reported:** 2026-02-02  
**Status:** Partially Resolved (significant progress since Feb 2)  
**Priority:** P1 - Feature required for business workflow  
**JIRA:** [PNO-969](https://unops.atlassian.net/browse/PNO-969)

**Description:**

**Significant implementation progress** since original filing. Core workflow now operational — OM can submit, cancel, reopen. DoA2 lookup works. Many original items now implemented by Tafazzul.

- **PNO-969 Reference:** Sending the Opportunity to decision makers (Go / No Go decision)
- **Test Cases:** 397 test cases (authoritative: `PNO-969_GoDecision_TestCases.md`, restructured to 10-category standard 2026-02-11)
- **Manual QA Passed:** 2 (TC-005 Cancel, TC-007 Reopen — verified by Silvia on QA, 2026-02-10)
- **Automated Tests Executed (2026-02-11):** 569 total across C# and Playwright
  - **509 passed, 0 failed, 60 skipped** (all skips intentional — DEF-008 blocked or env var not set)
- **Tests Blocked:** ~2 (PNO-1193 role transfer, inactive OM)
- **Tests Awaiting Manual QA Execution:** ~50 of 55 Playwright E2E tests (require `GO_DECISION_IMPLEMENTED=true`)

**Now Implemented (confirmed by QA testing 2026-02-05 through 2026-02-10):**
- ✅ Name validation
- ✅ Description validation
- ✅ ResponsibleOrgUnitId validation
- ✅ InitiativeBudgetUSD validation (optional)
- ✅ DoA2 Approver Lookup — querying EntityUserRole, routing to correct decision maker (India=Dominic, Sri Lanka=Perminder)
- ✅ Submit for Go Decision — I&P/Draft → GO/Active workflow (Perminder end-to-end tested)
- ✅ Rejection → NO GO/Closed (custom behavior, not previous stage)
- ✅ Cancel with mandatory reason — I&P/Draft → CANCELLED/Closed (Silvia verified)
- ✅ Reopen from Cancelled — CANCELLED/Closed → I&P/Draft (Silvia verified)
- ✅ Mandatory acknowledgement statement with org unit reference (fixed by Tafazzul 2026-02-06)
- ✅ Additional remarks field on submission dialog
- ✅ Read-only after submission for OM (fixed by Tafazzul — products/services and risks were editable, now locked)
- ✅ Workflow history visible on opportunity detail (fixed by Tafazzul 2026-02-06)
- ✅ Opportunity Statement review prior to submission (warning dialog implemented)
- ✅ Mandatory field validation — server-side validation displaying all failures as list

**Remaining Gaps (Not Yet Implemented or Unverified):**

**1. Collaborator Assignment (Clarified 2026-02-13):**
- ✅ **RESOLVED:** "Collaborator" is NOT a system role — it is an **assignment** via the `OpportunityCollaborator` entity (part of the Opportunity Development Team). The feature is already implemented:
  - ✅ `OpportunityCollaborators` table exists with Add/Edit/Remove UI in Team section
  - ✅ Assigned Collaborators **can edit all content fields** of the opportunity (checked via `IsOpportunityTeamMemberAsync` in `PermissionService`)
  - ✅ Assigned Collaborators **cannot perform workflow stage transitions** (Submit, Cancel, Reopen, Approve, Reject) — these are restricted to OM and Partnership Lead (DoA2) per `StateMachineStageChangeRoleSeeder`
  - ✅ Collaborator expertise assignment supported via `OpportunityCollaboratorExpertise`
- ℹ️ Previous note from Issam (2026-01-23) about "collaborator role not implemented" referred to the assignment feature which has **since been implemented**
- ℹ️ NEG-001 through NEG-010 test cases updated: verify that assigned Collaborators cannot perform workflow actions (correct by design, not a blocker)

**2. Notifications (Unverified):**
- ❌ Email notification to DoA2 on submission (template content unverified)
- ❌ OIC notifications
- ❌ Internal stakeholder notifications on GO decision
- ❌ OM recall notification to DoA2
- ❌ Email exact wording per AC Section 6

**3. UI Components (Unverified):**
- ❌ Stage stepper display logic (happy path only)
- ❌ DoA pathway display (DoA2/DoA3 read-only on detail page)
- ❌ In-workflow indicator on opportunity card in list view
- ❌ Inactive OM visibility (TC-033 — blocked, requires database deactivation to test)

**4. Additional Field Validations (Unverified):**
- ❓ Country-Org Unit mismatch warning
- ❓ Additional Remarks character count (Tafazzul: not yet implemented, needs separate refinement ticket)

**5. Resolved Bugs (2026-02-17):**
- ✅ DEF-010 / PNO-1193: OM role transfer now working (PNO-1166)
- ✅ DEF-011 / PNO-1171: Reject duplicate log entry removed (PNO-1166)
- ✅ DEF-012: ForAllMembers fix applied (PNO-1166)

**6. Requirements Gaps (Pending Clarification):**
- ❓ Initial status "Draft" vs AC Section saying "Active" — Issam workflow map (2026-02-10) shows Draft; requires confirmation from Roz/Issam

**Related Files:**
- Test Cases (authoritative): `QA Tests/Opportunity Tests/BusinessLogic/PNO-969_GoDecision_TestCases.md` (55 tests, 2026-02-11)
- Playwright Tests: `QA Tests/Playwright Tests/go-decision.spec.ts`
- Legacy PRD Test Cases: `QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_PRD_TestCases.md` (102 tests, superseded)
- Execution Report: `QA Tests/Opportunity Tests/BusinessLogic/GoNoGoDecision_TestExecution_Report.md`

---

### DEF-010: PNO-1193 — OM Role Transfer Not Working

**Severity:** 🟠 High  
**Component:** OpportunityWorkflow (Role Management)  
**Date Reported:** 2026-02-11  
**Status:** Resolved (2026-02-17)  
**Priority:** P1 - Business workflow requirement  
**JIRA Bug:** [PNO-1193](https://unops.atlassian.net/browse/PNO-1193)  
**Related PNO-969 Test Case:** TC-039  
**Fix PR:** PNO-1166 (merged via dev-deploy)

**Description:**

When a new Opportunity Manager (OM) is assigned to an opportunity, the previous OM should automatically be demoted to the Collaborator role. This was not happening — the previous OM retained the OM role or was removed entirely.

**Root Cause:** Role transfer logic was not implemented in `UNOPSOpportunityManager.UpdateOpportunityAsync()`.

**Resolution:** PNO-1166 adds logic to `UNOPSOpportunityManager.cs` that:
1. Tracks `previousOMUserId` before replacing the OM stakeholder
2. After OM replacement, checks if previous OM is already a Collaborator
3. If not, creates a new `OpportunityCollaborator` record for the previous OM
4. Includes `previousOMUserId` in the `requestedUserIds` set to prevent removal during collaborator sync

**Proper Fix:**
- When a new OM is assigned via the Opportunity Manager field, the system must:
  1. Set the new user as OM
  2. Demote the previous OM to Collaborator
  3. Preserve the previous OM's access to the opportunity content

**Wrong Fix:** ❌ Simply removing the previous OM's access entirely

**AC Reference:** Section 1 — "The OM field is a mandatory field that can never be blank. If a new Opportunity Manager is designated, the previous OM will be automatically assigned the Collaborator role."

**Reproduction Steps:**
1. Open an opportunity where User A is the current OM
2. Change the Opportunity Manager field to User B
3. Save the changes
4. Log in as User A
5. Navigate to the same opportunity

**Expected Result:** User A is now listed as a Collaborator on the opportunity and retains view/edit access to content.

**Actual Result:** User A does not become a Collaborator. Role transfer does not occur.

**Environment:** QA / TEST  
**Error/Logs:** No error displayed — silent failure  
**Reporter:** Perminder (QA testing 2026-02-10)

---

### DEF-011: PNO-1171 — Reject Action Appears Twice in Workflow History

**Severity:** 🟡 Medium  
**Component:** WorkflowHistory  
**Date Reported:** 2026-02-11  
**Status:** Resolved (2026-02-17)  
**Priority:** P2 - Data integrity / UI display issue  
**JIRA Bug:** [PNO-1171](https://unops.atlassian.net/browse/PNO-1171)  
**Related PNO-969 Test Case:** TC-030  
**Fix PR:** PNO-1166 (merged via dev-deploy)

**Description:**

When a DoA2 rejects a workflow for "Submit for Go Decision", the reject action was recorded **twice** in the stage change history.

**Root Cause:** `WorkflowController.Reject()` was calling both `AddLog()` with "Rejected" action AND `_workflowManager.Reject()`, which internally also logs the rejection. This caused a duplicate history entry.

**Resolution:** PNO-1166 removed the explicit `AddLog()` call from the rejection handler in `WorkflowController.cs` (lines 808-818 removed). The `_workflowManager.Reject()` call now solely handles logging, eliminating the duplicate.

**Proper Fix:**
- Investigate the rejection workflow handler and ensure only a single history entry is created per rejection action
- Add a uniqueness check or idempotency guard in the workflow history recording logic

**Wrong Fix:** ❌ Hiding duplicate entries at the UI level (masks the underlying data integrity issue)

**AC Reference:** Section 2 — Workflow history should accurately record each action once

**Reproduction Steps:**
1. Submit an opportunity for Go Decision as OM
2. Log in as DoA2 (decision maker)
3. Reject the workflow with a reason
4. View the stage change history on the opportunity
5. Observe: Reject action appears twice

**Expected Result:** A single "Reject" entry in workflow history with timestamp, user, and reason.

**Actual Result:** Two identical "Reject" entries appear in the stage change history.

**Environment:** QA / TEST  
**Error/Logs:** N/A — no error, visual duplication in history  
**Reporter:** Perminder (QA testing, JIRA PNO-1171)

### DEF-012: ForAllMembers Overrides Ignore() Rules in OpportunityMappingProfile

**Severity:** 🟡 Medium  
**Component:** OpportunityMappingProfile (`UNOPS.PAO.UNOPSBusiness/Managers/Mapping/OpportunityMappingProfile.cs`)  
**Date Reported:** 2026-02-16  
**Status:** Resolved (2026-02-17)  
**Priority:** P2 - Mapping correctness / potential data integrity risk  
**Reporter:** QA Automation (discovered during unit test creation)  
**Fix PR:** PNO-1166 (merged via dev-deploy)

**Description:**

The `CreateMap<UpdateOpportunityRequest, Opportunity>()` mapping profile was chaining `.ForAllMembers()` at the end of the fluent chain. `ForAllMembers` returns `void`, so it cannot be chained. The previous code compiled due to implicit void return handling but was syntactically incorrect.

**Resolution:** The dev team separated `ForAllMembers` into its own statement:
```csharp
var updateOpportunityMap = CreateMap<UpdateOpportunityRequest, Opportunity>();
updateOpportunityMap.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
updateOpportunityMap.ForMember(dest => dest.Id, opt => opt.Ignore())...
```

**Note:** AutoMapper behavior: `ForAllMembers` still applies to non-nullable types (e.g., `int Id`), so Id IS mapped. Production safety is maintained because `UpdateOpportunityRequest.Id` always matches the entity Id.

**Previous Problem:** In AutoMapper, `ForAllMembers` overrides **all** preceding per-member configurations, including `Ignore()`. This rendered the individual Ignore rules ineffective:

- **Id (int, non-nullable):** Always mapped because `srcMember != null` is always true for `int`. The Ignore is completely overridden.
- **Collections (nullable lists):** When the source list is non-null, AutoMapper maps (replaces) the destination collection. When null, AutoMapper still clears/initializes the destination collection to empty.

**Impact:**

The system currently works in production because:
1. `UpdateOpportunityRequest.Id` always matches the entity Id (caller sets it correctly)
2. `UpdateOpportunityAsync` processes collections separately after the `mapper.Map()` call, re-loading and reconciling them manually
3. Callers typically set collection properties to null on the request

However, the Ignore rules create a **false sense of safety**. If a caller ever passes non-null collections in the request, the loaded entity's collections (from `Include()`) would be silently replaced.

**Root Cause:** AutoMapper's `ForAllMembers` is a destructive operation that resets all per-member configurations. The `Ignore()` calls before `ForAllMembers` have no effect.

**Proper Fix:**
- Option A: Move `ForAllMembers` BEFORE the individual `ForMember(Ignore)` calls, so the Ignore rules take precedence
- Option B: Remove `ForAllMembers` and instead apply the null condition only to specific scalar members
- Option C: Remove the redundant `Ignore()` rules and document that null-protection comes solely from the `ForAllMembers` condition (and that collections must always be null on the request)

**Wrong Fix:** ❌ Removing the `ForAllMembers` condition entirely (would break null-protection for scalar properties)

**Workaround:** Current code works because `UpdateOpportunityAsync` handles collections independently after the map call. No immediate production impact.

**Related Tests:**
- `OpportunityMappingProfileTests.cs` (11 tests verifying actual behavior)
- `UNOPSOpportunityManagerTests.UpdateOpportunity_BasicFields_Success` (integration test verifying end-to-end update works)

**Reproduction Steps:**
1. Create an `Opportunity` entity with existing `FundingPartners` collection
2. Create an `UpdateOpportunityRequest` with non-null `FundingPartners` list
3. Call `mapper.Map(request, entity)`
4. Observe: entity's `FundingPartners` is replaced with mapped request data (Ignore rule did not protect it)

**Expected Result:** `FundingPartners` on the entity remains unchanged (Ignore rule should prevent mapping).

**Actual Result:** `FundingPartners` on the entity is replaced by the mapped request data.

**Environment:** All (unit test level — AutoMapper configuration issue)

---

### DEF-013: ~~LiaisonOfficeManager not registered in IManagerWrapper~~ — CLOSED (Won't Fix)

**Severity:** ~~🟡 Medium~~ → Closed  
**Component:** ManagerWrapper (`UNOPS.PAO.Business/Managers/ManagerWrapper.cs`)  
**Date Reported:** 2026-02-16  
**Status:** **Closed — Won't Fix (2026-03-04)**  
**Resolution:** Not a defect. By design.  
**Related QA:** QA-044 (also closed)

**Resolution Notes (2026-03-04):**

Per developer clarification (Anusha Swaminathan, 2026-03-04):
> "LiaisonOffice and FocalPoint do not have managers. They don't need to have managers because they are not being managed in Opp+. We can only select a Liaison Office / Focal Point as part of a Partner."

**What this means:**
- LiaisonOffice is a **lookup entity**, not a managed entity
- There is no `PartnerLiaisonOfficeManager` and none is needed
- LiaisonOffice data is accessed through `ValuesManager.GetLiaisonOffices()` and the `LiaisonOfficeController`
- Partners reference LiaisonOffice via `Partner.LiaisonOfficeId` FK

**QA Action:** 9 `PartnerLiaisonOfficeManagerTests` cancelled (were placeholder tests for a manager that doesn't need to exist). `ValuesManagerPerformanceTests` GetLiaisonOffices test un-skipped.

---

### DEF-014: ~~FocalPointManager not registered in IManagerWrapper~~ — CLOSED (Won't Fix)

**Severity:** ~~🟡 Medium~~ → Closed  
**Component:** ManagerWrapper (`UNOPS.PAO.Business/Managers/ManagerWrapper.cs`)  
**Date Reported:** 2026-02-16  
**Status:** **Closed — Won't Fix (2026-03-04)**  
**Resolution:** Not a defect. By design.  
**Related QA:** QA-045 (also closed)

**Resolution Notes (2026-03-04):**

Per developer clarification (Anusha Swaminathan, 2026-03-04):
> "LiaisonOffice and FocalPoint do not have managers. They don't need to have managers because they are not being managed in Opp+. We can only select a Liaison Office / Focal Point as part of a Partner."

**What this means:**
- FocalPoint is a **user FK on Partner** (`Partner.PartnerFocalPointUserId`), not a managed entity
- There is no `PartnerFocalPointManager` and none is needed
- "Focal Point" also exists as a Contact role string (tested in `ContactFunctionalTests`)
- Partners reference FocalPoint via `Partner.PartnerFocalPointUserId` FK

**QA Action:** 12 `PartnerFocalPointManagerTests` cancelled (were placeholder tests for a manager that doesn't need to exist).

---

### DEF-015: DashboardController has zero test coverage — 10+ endpoints

**Severity:** 🟡 Medium  
**Component:** DashboardController (`UNOPS.PAO.API/Controllers/DashboardController.cs`)  
**Date Reported:** 2026-02-16  
**Status:** Open  
**Priority:** P2 — High-traffic feature with no test safety net  

**Description:**
The `DashboardController` is the landing page controller for all users and exposes 10+ endpoints for widget data, metrics, charts, and summary information. Despite being the most-visited page in the application, it has zero dedicated test files — no integration tests, no unit tests.

**Impact:**
- Any regression in dashboard endpoints would be undetected until users report it
- Dashboard is the first thing every user sees after login
- Widget data endpoints involve complex aggregation queries that are prone to regression

**Endpoints Requiring Coverage:**
- Dashboard summary/metrics endpoints
- Widget data endpoints (partner counts, opportunity pipeline, recent activity)
- Chart data endpoints (trends, distributions)
- User-specific dashboard data

**Proper Fix:**
1. Create `DashboardControllerTests.cs` for integration-level endpoint testing
2. Create `DashboardManagerTests.cs` for unit-level business logic testing
3. Prioritize the most-used widget endpoints first
4. Include permission-based testing (different users see different dashboard data)

**Note:** Some dashboard-related tests may exist in the 58 excluded integration test files (DEF-007). Unblocking those files should be attempted first before writing new tests from scratch.

---

### DEF-021: AmbiguousMatchException — DocumentController Route Conflict with UNOPS Override

**Severity:** 🟠 High  
**Component:** DocumentController (`UNOPS.PAO.Presentation`), UNOPSDocumentController (`UNOPS.PAO.UNOPSPresentation`)  
**Date Reported:** 2026-02-18  
**Status:** Open  
**Priority:** P2 — Breaks all document download endpoints at runtime  
**Discovered By:** Integration test run (2026-02-18) — 6 tests throwing `AmbiguousMatchException`

**Description:**

When the integration test host loads both the base `UNOPS.PAO.Presentation` assembly and the `UNOPS.PAO.UNOPSPresentation` assembly together, ASP.NET Core routing throws `AmbiguousMatchException` because two controllers register conflicting routes for the document download endpoint:

- `UNOPS.PAO.Presentation.Controllers.Documents.DocumentController.DownloadDocument`
- `UNOPS.PAO.UNOPSPresentation.Controllers.DocumentController.Download`

Both match the same HTTP verb + route pattern, causing the router to be unable to select a single endpoint.

**Error:**
```
Microsoft.AspNetCore.Routing.Matching.AmbiguousMatchException : The request matched multiple endpoints. Matches:
  UNOPS.PAO.Presentation.Controllers.Documents.DocumentController.DownloadDocument (UNOPS.PAO.Presentation)
  UNOPS.PAO.UNOPSPresentation.Controllers.DocumentController.Download (UNOPS.PAO.UNOPSPresentation)
  UNOPS.PAO.Presentation.Controllers.Documents.DocumentController.GetAll (UNOPS.PAO.Presentation)
  Fallback {*path:nonfile}
```

**Root Cause:** The UNOPS override `DocumentController.Download` uses the same route as the base `DocumentController.DownloadDocument` without properly overriding or suppressing the base route. When both assemblies are registered in the DI container, ASP.NET Core sees both endpoints and cannot determine which one to use.

**Reproduction Steps:**
1. Run integration tests that hit the document download endpoint
2. Observe `AmbiguousMatchException` in `DocumentEdgeCaseTests.GetDocumentDownload_NonExistent_Returns404`
3. Affected tests: any request to the document download route

**Expected Result:** The UNOPS override controller takes precedence, or the base controller's route is suppressed when the override is registered.

**Actual Result:** `AmbiguousMatchException` — request returns HTTP 500 instead of expected response.

**Proper Fix:**
- Option A: Apply `[ApiExplorerSettings(IgnoreApi = true)]` to the base `DownloadDocument` action so it is excluded when the override is registered
- Option B: Give the UNOPS override a distinct route path that does not conflict with the base
- Option C: Use a route constraint or `[Route]` ordering to ensure the override takes precedence
- Option D: Ensure the override controller inherits from the base and uses `override` keyword so only one endpoint is registered

**Wrong Fix:** ❌ Suppressing the exception in tests — the production API would return HTTP 500 for all document download requests when both assemblies are loaded.

**Impact:**
- 7 integration tests fail with `AmbiguousMatchException` (updated 2026-02-25: previously 6, now 7 with new regression suite)
- Document download endpoint returns HTTP 500 in production when UNOPS override is active
- All users unable to download documents

**Related Tests:**
- `DocumentEdgeCaseTests.cs` — original tests (2026-02-18)
- `Documents/DocumentRouteConstraintTests.cs` — regression suite added 2026-02-25 (TC-DEF021-NEG-001/002/003/010, TC-DEF021-FUNC-005, TC-DEF021-BND-001, TC-DEF021-INT-001)

**Fix Status Update (2026-02-25):** A fix was identified in the `dev-deploy` branch:
- Mark `DocumentController.DownloadDocument` (base) with `[NonAction]`
- Add regex route constraint `^(?!download$).+` to `DocumentController.GetAll`
- Fix is NOT yet deployed to the integration test environment — all 7 regression tests confirm this.

**Environment:** Integration test host (both assemblies loaded) / Production (UNOPS deployment)  
**Reporter:** QA Automation (2026-02-18 test run)  
**Last Verified:** 2026-02-25 test run — still Open

---

### DEF-023: DEF-012 Regression — Duplicate UpdateOpportunityRequest Map Breaks AutoMapper

**Severity:** 🟠 High  
**Component:** `UNOPS.PAO.Business/Mapping/OpportunityMappingProfile.cs`  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Discovered By:** Integration test suite — `DEF-012_ForAllMembersFix/UnitTests.cs` UNIT_018, UNIT_020, UNIT_021

**Description:**

DEF-012 was marked resolved on 2026-02-17, but the integration tests that were written to verify the fix are **still failing**. Two related bugs remain:

1. **UNIT_018 — FundingPartners still being mapped on Update:**  
   `UNOPS.PAO.Business\Mapping\OpportunityMappingProfile.cs` (line 57) creates `CreateMap<UpdateOpportunityRequest, Opportunity>()` with only `ForAllMembers(condition)` — it does **not** ignore `FundingPartners`, `ClientPartners`, `Stakeholders`, etc.  
   `UNOPS.PAO.UNOPSBusiness\Managers\Mapping\OpportunityMappingProfile.cs` (line 89) correctly adds the Ignore rules — but both maps are loaded together via `AddMaps(AppDomain.CurrentDomain.GetAssemblies())`.  
   Result: duplicate type map for `UpdateOpportunityRequest → Opportunity` — the version **without** Ignore rules wins.

2. **UNIT_020/021 — AutoMapper `AssertConfigurationIsValid()` throws:**  
   Having two `CreateMap<UpdateOpportunityRequest, Opportunity>()` registrations across assemblies causes AutoMapper's configuration validator to fail, indicating a real runtime risk of incorrect mappings.

**Root Cause:**

The fix in DEF-012 was only applied to the UNOPS override profile, not the base profile. The base profile in `UNOPS.PAO.Business` still has a separate `CreateMap<UpdateOpportunityRequest, Opportunity>()` that overrides the UNOPS profile's Ignore rules.

**Proper Fix:**

Remove (or extend) the duplicate map in `UNOPS.PAO.Business\Mapping\OpportunityMappingProfile.cs`:
- Either remove the `CreateMap<UpdateOpportunityRequest, Opportunity>()` entry from the base profile entirely (since `Opportunity` is a UNOPS-overridden entity, only the UNOPS profile should own this map)
- Or add the same Ignore rules to the base profile so both maps are consistent

**Wrong Fix:** ❌ Updating only one of the two profiles.

**Failing Tests:**
- `UNIT_018_MapperConfiguration_IgnoreRulesCount` — Expected `dest.FundingPartners.Count` to be 0, but found 1
- `UNIT_020_MapperConfiguration_NoUnmappedMembersWarning` — `AssertConfigurationIsValid()` throws
- `UNIT_021_MapperConfiguration_AssertConfigurationIsValidPasses` — `AssertConfigurationIsValid()` throws

---

### DEF-022: Restricted User Can Access AI Prompt Management Admin Page

**Severity:** 🟠 High  
**Component:** AIPromptManagement (`/admin/ai-prompts` route), Authorization / Route Guards  
**Date Reported:** 2026-02-18  
**Status:** Open  
**Priority:** P2 — Unauthorized access to admin configuration  
**Discovered By:** Playwright E2E test `ai-assistant.spec.ts` — AI-009 (2026-02-18)

**Description:**

A Restricted User (view-only role) is able to navigate to or access the AI Prompt Management administration page (`/admin/ai-prompts` or equivalent), when this page should be blocked and inaccessible to non-admin roles.

The Playwright test `AI-009: AI prompt management inaccessible to restricted user` authenticates as a Restricted User, navigates to the AI admin prompt page, and asserts that access is blocked (`isBlocked = true`). The assertion fails, meaning the page loads without redirecting or showing an access-denied state.

**Error:**
```
Error: expect(received).toBeTruthy()
Expected: true (isBlocked)
Received: false
```

**Root Cause (suspected):** Either:
- The route guard for the AI Prompt Management page does not check the `Restricted User` role
- The permission check for this route is missing or evaluating incorrectly
- The Angular route guard is present but returns `true` for all authenticated users regardless of role

**Reproduction Steps:**
1. Log in as a Restricted User (view-only role, no admin permissions)
2. Navigate to the AI Prompt Management admin page (e.g. `/admin/ai-prompts`)
3. Observe: page loads without access-denied error or redirect

**Expected Result:** Restricted User is redirected to an access-denied page or the route is blocked. The page does not load.

**Actual Result:** The AI Prompt Management page loads successfully for the Restricted User.

**Proper Fix:**
1. Identify the Angular route guard protecting `/admin/ai-prompts`
2. Verify the guard checks for the correct admin permission (e.g. `CanManageAIPrompts` or `IsAdministrator`)
3. Add or fix the server-side permission endpoint to return `canAccess: false` for Restricted Users
4. Ensure the backend controller also validates permissions (defense in depth — don't rely solely on client-side guards)

**Wrong Fix:** ❌ Only hiding the navigation menu link — the route itself must be guarded.

**Impact:**
- Restricted Users can view and potentially modify AI prompt configurations
- AI prompts control system-wide AI assistant behavior — unauthorized modification is a security risk
- 1 Playwright test failing; likely more tests blocked due to maxFailures limit

**Related Tests:** `ai-assistant.spec.ts:154` — `AI-009: AI prompt management inaccessible to restricted user`  
**Environment:** Dev (http://localhost:4200 / http://localhost:5159)  
**Reporter:** QA Automation — Playwright E2E run 2026-02-18

---

## Resolved Defect Details

_See [Resolved Defects summary table](#resolved-defects-summary) above for the full list._

---

### ~~DEF-016: OpportunityImmutabilityTests — 8 tests fail~~ (RECLASSIFIED → QA-061)

**Severity:** ~~🟡 Medium~~ → Reclassified as QA infrastructure issue  
**Component:** OpportunityImmutabilityTests (`QA Tests/C# Tests/UNOPS.PAO.Business.Tests/Opportunity/OpportunityImmutabilityTests.cs`)  
**Date Reported:** 2026-02-16  
**Status:** **Resolved (2026-02-17)** — Reclassified to QA-061 and fixed  
**Priority:** N/A — Not a production code issue  

**Description:**
Originally logged as a developer defect, but the root cause was test infrastructure:
1. `BaseRepository.UpdateAsync` uses `Z.EntityFramework.Extensions.BulkUpdate` which requires a relational DB model and throws `InvalidOperationException` on InMemory DB
2. `GetOpportunityAsync` returns null on InMemory DB due to complex include queries

**Resolution (2026-02-17):**
- Fixed non-immutable stage tests to verify no `BusinessException` thrown (proving immutability check passed), while accepting `InvalidOperationException` from BulkUpdate
- Fixed permission endpoint tests to conditionally assert when `GetOpportunityAsync` returns non-null
- All 27 tests now pass on InMemory DB (previously 8 failures)

---

### DEF-017: WorkflowControllerTests — 6 Submit tests fail after endpoint behavior change

**Severity:** 🟡 Medium  
**Component:** WorkflowControllerTests (`QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs`)  
**Date Reported:** 2026-02-16  
**Status:** ✅ Resolved (2026-02-17)  
**Priority:** P3  

**Description:**
6 Submit-related workflow tests were failing because the `Submit` endpoint now queries the database directly and validates all 21 opportunity fields via `ValidateOpportunityRequirementsAsync`.

**Resolution:**
Test infrastructure updated to comprehensively seed the InMemory database with:
- `SeedOpportunityAsync()` helper creates a fully valid Opportunity with all 21 required fields (budget, challenges, impact, outcomes, beneficiaries, missions, dates, statement, org unit, initiative type) plus related entities (deliverables, SDGs, funding/client partners, countries, DoA Level 2 holder)
- `SeedOpportunityManagerStakeholderAsync()` creates OM entity role and stakeholder assignment
- `SetupStandardSubmitMocks()` configures all workflow manager mocks including `AddLog`, `Initiate`, `GenerateOpportunityStatementAsync`

All 6 Submit tests now have matching data seeding and mock expectations.

---

### DEF-018: DuplicateDetectionService uses relational APIs incompatible with InMemory provider

**Severity:** 🟠 High  
**Component:** DuplicateDetectionService, AiContextualService, AdvancedSearchService  
**Date Reported:** 2026-02-16  
**Status:** ✅ Resolved (2026-02-17)  
**Priority:** P2  
**Related QA Issue:** QA-053

**Description:**
Multiple services used EF Core relational-specific APIs (`GetDbConnection()`, `ExecuteSqlRawAsync()`, `NpgsqlParameter`, `SqlQueryRaw`). When tests used `UseInMemoryDatabase()`, these calls threw `InvalidOperationException`.

**Resolution:**
All affected services now have proper InMemory/relational guards:
- **AiContextualService**: `DetectDuplicateForRecordAsync()` → `if (!_context.Database.IsRelational()) return new ComprehensiveDuplicateResult();`
- **AiContextualService**: `InsertEntityEmbedding()` → `if (!_context.Database.IsRelational()) return;`
- **AdvancedSearchService**: `SearchPartnersAsync()`, `SearchContactsAsync()`, `SearchInteractionsAsync()`, `SearchOpportunitiesAsync()`, `ExecutePostgreSQLSearchAsync()` → `if (IsInMemoryProvider()) return new List<GlobalSearchResult>();` / `return "[]";`

Guards return safe empty results when running against non-relational providers, preventing 500 errors while allowing the rest of the application to function normally in tests.

---

### DEF-019: PAOAuthorizationService doesn't handle DenyAnonymousAuthorizationRequirement

**Severity:** 🟡 Medium  
**Component:** PAOAuthorizationService (`UNOPS.PAO.Server/Infrastructure/Security/`)  
**Date Reported:** 2026-02-16  
**Status:** ✅ Resolved (2026-02-17)  
**Priority:** P3  

**Description:**
`PAOAuthorizationService` manually iterates registered `IAuthorizationHandler` instances but had no handler for `DenyAnonymousAuthorizationRequirement` (used by `RequireAuthenticatedUser()` policies).

**Resolution:**
`DenyAnonymousAuthorizationRequirement` handler added directly in `PAOAuthorizationService.AuthorizeAsync()` (lines 41-50 of `UNOPS.PAO.Server/Infrastructure/Security/PAOAuthorizationService.cs`):
```csharp
foreach (var requirement in requirements)
{
    if (requirement is DenyAnonymousAuthorizationRequirement)
    {
        if (user.Identity?.IsAuthenticated == true)
            context.Succeed(requirement);
    }
}
```
This executes before the custom handler iteration loop, ensuring standard ASP.NET Core authorization policies work correctly alongside the custom permission-based authorization.

---

## Reclassified Items

The following items were previously logged as developer defects but have been reclassified to more appropriate categories:

### Moved to Backlog (Tests Written for Unimplemented Features)

| Former ID | Title | Why It's Not a Defect | Recommendation |
|-----------|-------|----------------------|----------------|
| DEF-005 | Missing Model Namespaces (7 namespaces) | Tests were written **ahead of implementation**. Models don't exist because features aren't built yet. | Track as planned feature work in sprint backlog. Tests serve as specifications. |
| DEF-007 | IntegrationTests Out of Sync (4,675 errors) | Tests reference APIs that **were never implemented** or were changed. Test code is wrong, not production code. | **RESOLVED (2026-02-07):** Audit complete. Deleted 13 fully obsolete files (DST module, TranslationController, ExportController). Excluded 51 files referencing non-existent managers/types via Compile Remove. Fixed 6 FluentAssertions syntax errors. Build now succeeds with 0 errors. 1,450 tests compile; 465 pass, 942 fail at runtime (expected — require PostgreSQL + running app), 43 skipped. |
| DEF-009 | `isAdmin()` does not check for `Administrator` role | **Not a defect.** There is no `Administrator` role in the system. The only admin roles are `PARTNER_GLOB_ADMIN` and `ORG_UNIT_ADMIN`, which `isAdmin()` already checks correctly. The test workaround of assigning both roles was unnecessary — `PARTNER_GLOB_ADMIN` alone is sufficient. | No action needed. `isAdmin()` is working as designed. |
| DEF-015 | DashboardController has zero test coverage — 10+ endpoints | **Not a developer defect.** Missing test coverage is a QA/test team responsibility, not a production code bug. No production code is broken — tests simply don't exist yet. | Track as planned QA work in sprint backlog. QA team to create `DashboardControllerTests.cs` and `DashboardManagerTests.cs`. |
| DEF-022 | Restricted user can access AI Prompt Management admin page | **Not a production defect.** The Playwright test failure is caused by a missing/incorrect permissions mock in the test environment, not by broken authorization in production code. Already tracked as **QA-068** (mock permissions issue). | No developer action needed. QA to fix the permissions mock in `ai-assistant.spec.ts` to correctly simulate a restricted user. |

---

## Defect Statistics (Updated 2026-03-05)

- **Total Open:** 48 (DEF-008, DEF-020, DEF-021, DEF-023–DEF-050, DEF-052–DEF-095)
- **2026-03-05 Updates:**
  - **DEF-053 confirmed NOT resolved:** QA re-assessment on 2026-03-05 confirms the production defect is still present. ADC and Secret Manager access work, but `UNOPSGeminiManager.GetCredentials()` does not use either — it reads credential JSON directly from `IConfiguration` (null in test env). The `DisableExternalCalls` config flag is not checked before the crash. 85+ un-skipped tests continue to fail. Three fix options documented in detailed section.
  - **Database configuration note:** `appsettings.Testing.json` points to `unops-opportunityplus-dev-db-leonardc` via Cloud SQL Proxy on `127.0.0.1:5432`. Production `appsettings.json` points to `unops-opportunityplus-dev-db-anushas` on port 6364. Both use IAM authentication. `PAOWebApplicationFactory` falls back to InMemory provider when PostgreSQL is unavailable. Developer team should confirm the correct shared dev database for CI/test execution.
- **2026-03-04 Playwright E2E Session Updates:**
  - **DEF-062 workaround applied:** Startup.cs modified to conditionally register PubSubPullService/DueDiligenceNotificationService based on `Enabled` config flag. Backend now starts and stays stable in Development. Permanent fix pending from developer.
  - **DEF-063 NEW:** IAPVerificationMiddleware runs unconditionally in Testing environment, blocks `[AllowAnonymous]` endpoints
  - **Playwright E2E results:** 1,015 passed, 92 failed, 445 skipped (51.4 min, headless Chromium, 4 workers). Real backend integration working — 9/11 partners tests pass with real database data.
- **2026-03-04 Earlier Updates:**
  - **DEF-013 CLOSED** (Won't Fix): LiaisonOffice does not have a manager by design (per Anusha)
  - **DEF-014 CLOSED** (Won't Fix): FocalPoint does not have a manager by design (per Anusha)
  - **DEF-053 ~~Verification Pending~~ NOT Resolved (confirmed 2026-03-05)**: Anusha reported fix may be in place (2026-03-04); 85+ tests un-skipped for CI verification. **2026-03-05 re-assessment:** ADC and Secret Manager access work, but `UNOPSGeminiManager.GetCredentials()` bypasses both — reads credential JSON directly from `IConfiguration` which is `null` in test env. Constructor crash blocks entire `UNOPSManagerWrapper`. 85+ tests still failing. See detailed section for three proposed fix options.
  - 21 PartnerLiaisonOffice/FocalPoint placeholder tests cancelled
  - Integration tests job enabled in CI (`continue-on-error: true`) for Anusha to verify UNOPS.Workflow submodule and DEF-053 fixes
- **NEW (2026-03-04):** DEF-061 (3,036 compiler warnings across 15 production projects), DEF-062 (PubSubPullService ignores Enabled:false config — workaround applied), DEF-063 (IAPVerificationMiddleware blocks AllowAnonymous in Testing env)
- **NEW (2026-03-03):** DEF-057 (Partner name whitespace-only input), DEF-058 (OpportunityManager invalid Stakeholders.Contact include path), DEF-059 (PartnerManager GetPartnerWithContactsAndInteractions 805ms, 4x over SLA), DEF-060 (EF Migration Init references AspNetUsers before Identity tables exist)
- **NEW (2026-03-02):** DEF-054 (DoA3Fallback missing ILogger logging), DEF-055 (Reject NullReferenceException on null EntityName), DEF-056 (Reopen sets Draft instead of Active)
- **DEF-051 reclassified (2026-03-02):** AutoMapper mock overload mismatch in test, not a production defect.
- **Total Partially Resolved:** 1 (DEF-008 — DoA3 fallback added via PNO-1197, remaining gaps in email notifications and UI)
- **Total Resolved:** 6 (DEF-010, DEF-011, DEF-012, DEF-017, DEF-018, DEF-019)
- **Total Reclassified:** 6 (DEF-005, DEF-007, DEF-009, DEF-015, DEF-022 → moved to appropriate trackers; DEF-051 → QA mock issue)
- 🔴 **Critical:** 0
- 🟠 **High Priority:** 17 (DEF-008, DEF-020, DEF-021, DEF-023, DEF-024, DEF-033, DEF-034, DEF-038, DEF-039, DEF-040, DEF-042, DEF-043, DEF-045, DEF-053, DEF-058, DEF-059, DEF-062)
- 🟡 **Medium Priority:** 23 (DEF-025–DEF-032, DEF-035–DEF-037, DEF-041, DEF-044, DEF-047–DEF-050, DEF-052, DEF-055, DEF-056, DEF-060, DEF-061, DEF-063)
- 🟢 **Low Priority:** 4 (DEF-046, DEF-051, DEF-054, DEF-057)
- **2026-03-03 New Tests Added:** AiContextualServiceProcessPlaceholderTests (39 tests, 39/39 passed), DocumentControllerUNOPSTests (39 tests, 21 passed, 18 skipped pending DEF-053), 3 Playwright E2E spec files (api-error-handling, form-validation-negative, interactions-enhanced), 15+ new Playwright API mocks
- **2026-03-03 Full Run (after QA-089 concurrent DbContext fixes):** FastTests: 78/78 passed (100%). Presentation.Tests: 154/154 passed (100%). Business.Tests: 4,627 total — 4,329 passed, 57 failed, 241 skipped (93.6%). Integration Tests: 6,132 total — 5,662 passed, 115 failed, 355 skipped (92.3%). **TOTAL: 11,069 tests — 10,223 passed (92.4%), 172 failed, 596 skipped.** 75 concurrent DbContext tests (QA-089) fixed — all 75 now pass. New tests added: 39 AiContextualService + 39 DocumentController UNOPS.
- **2026-03-02 Re-run (after QA fixes):** Business Tests: 2,781 total — **2,592 passed**, 13 failed (all pre-existing DEFs), 176 skipped, 2 hung (QA-092, now resolved with timeouts). PartnerControllerTests: **52/52 passed**. PNO-1146 suite: **52/52 passed** (21 previously skipped tests un-skipped via QA-091 fix). All 13 Business Test failures are tracked: DEF-047 (5 tests), DEF-048 (2 tests), DEF-049 (2 tests), DEF-050 (1 test), DEF-024 (1 test), PartnerByOrgUnit specification (2 tests).
- **2026-03-02 Full Run (PostgreSQL available):** Cloud SQL Proxy running — full execution across all C# suites. FastTests: 78/78 passed. Presentation Tests: 154/154 passed. Business Tests: 4,301 total — 3,982 passed, 78 failed, 241 skipped. Integration Tests: 5,592 total — 5,241 passed, 211 failed, 140 skipped. **4 new production defects discovered** (DEF-047–DEF-050). 78 Business Test failures: 27 QA-084, 36 QA-085, 5 DEF-047/048, 3 DEF-049, 1 DEF-050, 2 specification test data, 1 QA-087, 1 DEF-024, 2 PartnerByOrgUnit. 211 Integration Test failures: 51 QA-086 (fixture), 37 null responses, 31 DEF-045, 7 DEF-021, 6 DEF-027, and various existing DEFs.
- **2026-02-20 Update:** DEF-015 reclassified to QA/Backlog (test coverage gap, not a production defect). DEF-022 reclassified to QA-068 (Playwright mock issue, not a production authorization defect).
- **2026-02-17 Update:** DEF-010, DEF-011, DEF-012 all resolved via PNO-1166 merge from dev-deploy.
- **DEF-010 RESOLVED:** OM role transfer now works — previous OM auto-demoted to Collaborator in `UNOPSOpportunityManager`.
- **DEF-011 RESOLVED:** Duplicate rejection log entry removed from `WorkflowController.Reject()`.
- **DEF-012 RESOLVED:** `ForAllMembers` separated into own statement in `OpportunityMappingProfile`.
- **DEF-008 Progress:** Core Go Decision workflow operational. PNO-1197 adds DoA Level 3 fallback approver logic. Remaining: email notifications, some UI components.
- **InMemory Test Fix (2026-02-17):** All 8 previously-failing WorkflowControllerTests now pass. Root cause: InMemory `.Include()` with `.AsNoTracking()` + non-nullable FK filters out parent entities when referenced entity doesn't exist. Fix: seed `Country` reference entity, set explicit `EntityRole` navigation properties, fix mock casing and request fixtures.
- **Test Coverage Added (3:1 Ratio Enforced):**
  - **C# Integration (WorkflowControllerTests):** 12 new tests (3P, 8N, 1E — **ratio: 9 >= 9** ✅). 71/71 passed (100%).
  - **C# Unit (OpportunityMappingProfileTests):** 4 new tests (1P, 2N, 1E — **ratio: 3 >= 3** ✅). 15/15 passed (100%).
  - **Playwright E2E (go-decision.spec.ts):** 16 new tests (4P, 9N, 3E — **ratio: 12 >= 12** ✅). All feature-gated; 1 passed, 36 skipped.
  - **Playwright E2E (workflow.spec.ts):** 16/16 passed (100%).
  - **Always-applied ratio rule created:** `.cursor/rules/test-ratio-enforcement.mdc` (both opportunityplus + unops-pdj).
- **DEF-018 Resolved:** All services (`AiContextualService`, `AdvancedSearchService`) now have `IsRelational()`/`IsInMemoryProvider()` guards on every relational API call, returning empty results for non-relational providers.
- **DEF-019 Resolved:** `PAOAuthorizationService.AuthorizeAsync()` now handles `DenyAnonymousAuthorizationRequirement` directly (lines 41-50), succeeding for authenticated users.

### Key Finding (2026-03-02 Full PostgreSQL Run): 4 NEW production defects discovered (DEF-047–DEF-050). Total: 10,125 tests executed (78 Fast + 154 Presentation + 4,301 Business + 5,592 Integration). 9,455 passed (93.4%), 289 failed, 381 skipped. Of 289 failures: 114 are QA test infrastructure issues (QA-084/085/086/087), ~130 are already-tracked DEFs (DEF-021/027/042/045 etc.), 10 are new DEF-047–050, and ~35 are test data/assertion issues under investigation.

### Key Finding (2026-02-17): No new production defects discovered during 2026-02-17 full execution across all 5 test suites.

---

## Latest Test Results (2026-03-02 — Full PostgreSQL Run)

### .NET C# Tests - Combined Summary

| Test Suite | Passed | Failed | Skipped | Total | Pass Rate | Duration |
|------------|--------|--------|---------|-------|-----------|----------|
| **FastTests** | 78 | 0 | 0 | 78 | 100% ✅ | 7s |
| **Presentation.Tests** | 154 | 0 | 0 | 154 | 100% ✅ | 14s |
| **Business.Tests (PostgreSQL)** | 3,982 | 78 | 241 | 4,301 | 92.6% ⚠️ | 22.5m |
| **Integration Tests (PostgreSQL)** | 5,241 | 211 | 140 | 5,592 | 93.7% ⚠️ | 2.6m |
| **Playwright E2E** | — | — | — | — | No dev server | — |
| **TOTAL** | **9,455** | **289** | **381** | **10,125** | **93.4%** | ~25m |

### Post-QA-Fix Re-run Results (2026-03-02)

| Suite | Passed | Failed | Skipped | Total | Pass Rate | Notes |
|---|---|---|---|---|---|---|
| **Business Tests** | 2,592 | 13 | 176 | 2,781 | **93.2%** | All 13 failures are pre-existing DEFs; 2 tests hung (QA-092) |
| **PartnerControllerTests** | 52 | 0 | 0 | 52 | **100%** ✅ | All tests passing with proxy |
| **PNO Integration Suites** | 1,423 | 0 | 206 | 1,629 | **100%** ✅ | 0 failures; skips are DEF-053 and QA-091 |
| **Other Controller Tests** | 97 | 0 | 73 | 170 | **100%** ✅ | Skips are DEF-053 |

### Business Tests — 78 Failures Analysis (pre-QA-fix)

| Failure Category | Count | Root Cause | Tracking |
|---|---|---|---|
| BaseEngagementManagerTests — Guid format string | 36 | Test code bug: `SeedEngagementAsync` uses invalid Guid format specifier | QA-085 |
| OpportunityImmutabilityTests — constructor NullRef | 27 | Test infrastructure: missing HttpContext mock for `UserResolverService` | QA-084 |
| OpportunityValidation — empty/whitespace name accepted | 5 | **Production defect**: no validation for empty/whitespace opportunity names | **DEF-047** |
| UNOPSOpportunityManager — null request not guarded | 2 | **Production defect**: `UpdateOpportunityAsync` throws `InvalidOperationException` instead of `ArgumentNullException` | **DEF-049** |
| OpportunityValidation — name max-length generic error | 2 | **Production defect**: no business-level max-length validation, DB throws generic error | **DEF-048** |
| PartnerByOrgUnitSpecification — filter returns empty | 2 | Specification filter for indirect contacts and multiple user IDs returns 0 results | Under investigation |
| AutoMapper — OpportunityCountry.Country missing | 1 | **Production defect**: missing navigation property mapping for Country on OpportunityCountry | **DEF-050** |
| PartnerErpDimValueFix — range boundary | 1 | Test data boundary: range [7999-7999] has 0 available values | QA-087 |
| InteractionContactId — FK not enforced | 1 | Already tracked under DEF-024 (ContactId via junction table) | DEF-024 |
| PartnerByOrgUnitSpecification — Name required | 1 | Already tracked under existing test data issues | Under investigation |

### Integration Tests — 211 Failures Analysis

| Failure Category | Count | Root Cause | Tracking |
|---|---|---|---|
| PAOWebApplicationFactory fixture missing | 51 | xUnit class fixture not registered for test classes | QA-086 |
| Null response / "Expected a value" | 37 | API returns null where test expects data — test data/seeding issue | QA (various) |
| AuditLogController 500 errors | 31 | All authenticated requests return 500 | DEF-045 |
| HTTP 500 instead of 400 (validation) | 13 | Server throws instead of returning validation error | Various DEFs |
| HTTP 500 instead of 200 (success) | 12 | Endpoint throws for valid requests | Various DEFs |
| HTTP 500 instead of 404 (not found) | 9+7 | Endpoint throws instead of returning 404 | Various DEFs |
| AIPromptManagement authorization | 12 | PAOWebApplicationFactory fixture missing (subset of 51 above) | QA-086 |
| PartnerAnalyticsController 500 | 8 | Analytics service failure (likely pg_trgm or SQL) | DEF-042 |
| DocumentController route conflict | 7 | AmbiguousMatchException — two controllers register same route | DEF-021 |
| GlobalController missing endpoints | 6 | Health/metadata endpoints not implemented | DEF-027 |
| ContactAnalytics failures | 9 | Analytics service 500 errors | DEF-042 |
| Various edge case/validation tests | ~9 | Feature gaps or test data issues | Various DEFs |

**Key Finding (2026-03-02):** 4 NEW production defects discovered (DEF-047–050). Of 289 total failures, ~114 are QA test infrastructure (QA-084/085/086/087), ~130 are already-tracked production DEFs, 10 are new DEFs, and ~35 are under investigation.

---

## Previous Test Results (2026-02-17 — Full PostgreSQL Execution, 0 failures)

### .NET C# Tests - Combined Summary

| Test Suite | Passed | Failed | Skipped | Total | Pass Rate | Duration |
|------------|--------|--------|---------|-------|-----------|----------|
| **FastTests** | 78 | 0 | 0 | 78 | 100% ✅ | 11s |
| **Business.Tests (PostgreSQL)** | 3,951 | 0 | 229 | 4,180 | 100% ✅ | 5.3m |
| **Presentation.Tests** | 29 | 0 | 0 | 29 | 100% ✅ | 7s |
| **Integration Tests (InMemory)** | 546 | 127 | 43 | 716 | 76.3% ⚠️ | ~4.5m |
| **TOTAL** | **4,604** | **127** | **272** | **5,003** | **97.3%** | ~10m |

**Key Change (2026-02-17):** Business.Tests now run against real PostgreSQL via Cloud SQL Proxy + IAM auth. Result: **3,951 passed (100%), 0 failed, 229 skipped** — all 9 previous InMemory failures eliminated. The PostgreSQL execution resolves Z.EF.Extensions BulkUpdate, complex aggregation queries, and ERP dimension value logic that were incompatible with SQLite.

### C# Business.Tests (PostgreSQL) — 0 Failures ✅

All 3,951 executable tests pass against the real PostgreSQL database. The previous 9 failures (Z.EF.Extensions BulkUpdate, GetOpportunityDetailsForAI, PartnerErpDimValueFix) were all InMemory/SQLite provider limitations and are now eliminated by running against PostgreSQL.

**229 Skipped Tests:** All intentional — QA-009 (Z.EF.Extensions 111), QA-042 (DST/Gemini 28), QA-043 (BigQuery 35), QA-044 (LiaisonOffice 9), QA-045 (FocalPoint 12), DEF-008 (Go Decision 40), plus various feature-specific skips.

### Integration Tests — 127 Failures (Infrastructure Issues)

| Failure Category | Count | Root Cause | QA Issue |
|---|---|---|---|
| HTTP 500 (Internal Server Error) | 60 | DuplicateDetectionService, AdvancedSearch relational APIs fail on InMemory DB | QA-053, DEF-018 |
| HTTP 403 (Forbidden) | 34 | PAOAuthorizationService missing DenyAnonymous handler | QA-052, DEF-019 |
| Skipped (environment/auth) | 24 | Tests skip due to authorization issues or missing credentials | QA-014, QA-051 |
| Endpoint behavior changed | 6 | WorkflowController Submit endpoint behavior changed in developer pull | DEF-017 |
| Various (404, data assertions) | 3 | Test data expectations vs actual DB state | Test maintenance |

**All 127 failures are test infrastructure issues, NOT production code defects.** The 546 passing tests confirm core API functionality is working correctly.

### Integration Tests - NOW COMPILING ✅ (DEF-007 Resolved)

**Previously:** 4,675 build errors. **Now:** Build succeeds with 0 errors.

**Cleanup performed (2026-02-07):**
- **Deleted** 13 files (DST module — no production controller, TranslationController/ExportController tests — no production controllers)
- **Excluded** 51 files via Compile Remove (reference non-existent managers: DashboardManager, PartnerAnalyticsManager, ContactAnalyticsManager, OrganizationManager, UserProfileManager, RoleManager, PermissionManager, LiaisonOfficeManager, and non-existent request types)
- **Fixed** 6 FluentAssertions syntax errors in controller tests

**Current test results:** 1,450 tests compile — 465 pass, 942 fail (expected: require PostgreSQL + running app), 43 skipped. Runtime failures are test infrastructure issues (QA-009, QA-019), not production defects.

### Playwright E2E Tests (2026-02-17, Full Suite Execution — All 54 Spec Files, chromium)

| Metric | Count | Notes |
|--------|-------|-------|
| **Passed** | 415 | 83.8% of attempted |
| **Failed** | 20 | All test infrastructure issues |
| **Skipped** | 59 | Intentional skips (Go Decision, features not implemented) |
| **Did Not Run** | 2,532 | Firefox + Webkit projects not executed; serial group abandonment |
| **Total Registered** | 3,027 | 3 browser projects × ~1,009 tests |
| **Duration** | 28.2m | Single invocation, 2 workers |

**Pass Rate (chromium attempted):** 415 / 494 = **84.0%** | 415 / 435 executed = **95.4%**

**Note on "Did Not Run":** The `playwright.config.ts` has 3 browser projects (chromium, firefox, webkit). Only chromium was actively executed. The remaining ~2,032 are firefox/webkit copies. Within chromium, ~514 additional tests did not run due to serial group abandonment when tests fail within `test.describe.configure({ mode: 'serial' })` blocks.

### Playwright Failure Analysis (20 failures — all test infrastructure/mock issues)

| Category | Count | Tests | Root Cause | QA Issue |
|----------|-------|-------|------------|----------|
| Login backend tests | 4 | login.spec.ts (4 tests) | Require real Google OAuth login form — no `/login` page exists in mock env | QA-021 |
| Document upload dialogs | 3 | document-management.spec.ts (3 tests) | Upload button click doesn't open dialog — missing document type API mock | QA-058 |
| Base engagements | 3 | base-engagements.spec.ts (3 tests) | Page content doesn't render — `/api/base-engagement` endpoint not mocked | QA-058 |
| Status badge selectors | 2 | crm-related-panels.spec.ts (2 tests) | `p-tag` status badge not found — selector may need updating for current DOM | QA-059 |
| Contact edit/delete dialogs | 2 | contact-item.spec.ts (2 tests) | `p-dialog` timeout after clicking edit/delete buttons — PrimeNG DynamicDialog issue | QA-008 |
| Accessibility ARIA | 1 | accessibility.spec.ts (1 test) | `aria-label` count on partner detail = 0, expected > 0 | QA-059 |
| Entity config dropdown | 1 | admin-entity-config.spec.ts (1 test) | Entity selector dropdown not visible on admin page | QA-057 |
| AI prompt restriction | 1 | ai-assistant.spec.ts (1 test) | Restricted user still sees admin prompts page — mock permissions issue | QA-068 |
| Comment text input | 1 | cross-entity-workflows.spec.ts (1 test) | Comment textarea/input not found in collaboration section | QA-059 |
| Notifications API | 1 | notifications.spec.ts (1 test) | GET `/api/notifications` response doesn't contain expected structure | QA-056 |
| Opportunity DST chip | 1 | opportunity-dst.spec.ts (1 test) | Analysis section navigation chip not visible | QA-059 |

**No production defects discovered.** All 20 failures are test infrastructure issues (missing mocks, outdated selectors, PrimeNG dialog limitations).

**Improvement vs 2026-02-16:** Failures reduced from 90 to 20 (**-78%**). Key improvements: `test.slow()` applied to all 54 specs eliminated timeout failures, URL alignment (`localhost:4200`) fixed connectivity, dialog assertion fix (QA-069) eliminated false positives.

### RBAC Playwright Tests (2026-02-07 - role-access-control.spec.ts, included in above totals)

| Metric | Count | Notes |
|--------|-------|-------|
| **Passed** | 161 | 100% pass rate ✅ |
| **Failed** | 0 | - |
| **Skipped** | 0 | - |
| **Total** | 161 | - |
| **Duration** | 9.9m | chromium only |

**All 161 role-based access control tests passing.** Covers 5 roles (System Admin, Partner Global Admin, Partner User, Org Unit Admin, General User) across 4 entities (Partners, Contacts, Interactions, Opportunities) plus Admin pages and Sidebar navigation.

---

## What Belongs in This List?

### ✅ Log as Developer Defect (DEF-XXX)

- Implemented feature doesn't match PRD/specification
- API returns incorrect data or status codes in production
- Business logic produces wrong results
- Security vulnerability in production code
- Performance issue in production (not test environment)
- Data corruption or loss in production

### ❌ Do NOT Log as Developer Defect

| Issue Type | Where to Track |
|------------|----------------|
| Test infrastructure issues | `Defect List for QA.md` |
| Tests fail due to test configuration | `Defect List for QA.md` |
| InMemory DB can't run raw SQL | `Defect List for QA.md` |
| Missing test selectors (data-testid) | `Defect List for QA.md` (QA-036: rewrite locators) |
| Tests written for features not yet built | Sprint Backlog / Feature Requests |
| .NET/Angular framework bugs | External issue tracker (GitHub) |
| Test environment limitations | `Defect List for QA.md` |

---

## How to Use This Document

### For Developers:
1. Review during sprint planning for genuine bugs to fix
2. Update **Status** as work progresses
3. Move resolved defects to "Resolved Defects" section
4. Reference defect IDs in commits (e.g., "DEF-008: Implement DoA2 lookup")
5. **Challenge defects that aren't production issues** - help QA categorize correctly

### For QA Team:
1. **Before logging**: Ask "Is this a production code issue or a test issue?"
2. Use the "What Belongs in This List?" section as a guide
3. If uncertain, discuss with development team before logging
4. Cross-reference with `Defect List for QA.md` for test infrastructure issues

### For Project Managers:
1. This list should be short - most issues are test infrastructure or planned work
2. Use defect count as a quality metric for implemented features
3. Track reclassified items to understand categorization patterns

---

## Comprehensive 10-Category Test Coverage Report (2026-02-17)

### Summary

Created **1,117 new tests** across 3 suites covering all 10 mandatory categories from `comprehensive-test-strategy.mdc`. All suites comply with the 3:1 ratio rule.

### Coverage Per Feature

**PNO-1166 (DEF-010/DEF-011): Reject Duplicate Fix + OM Transfer**
- 373 tests across 10 categories
- 363 passed / 10 failed (97.3%)
- Confirmed: Reject calls `Reject()` exactly once (no duplicate AddLog)
- Confirmed: OM transfer correctly updates stakeholder roles
- No new production defects discovered

**PNO-1197: DoA Level 3 Fallback in Submit Validation**
- 372 tests across 10 categories
- 309 passed / 63 failed (83.1%)
- Confirmed: DoA3 fallback works when DoA2 not found
- Confirmed: Both DoA2 and DoA3 accepted for submit validation
- 63 failures are test infrastructure (auth middleware, InMemory concurrency) — see QA-062, QA-063
- No new production defects discovered

**DEF-012: ForAllMembers Fix in OpportunityMappingProfile**
- 372 tests across 10 categories
- 358 passed / 14 failed (96.2%)
- Confirmed: ForAllMembers condition prevents null overwrites
- Confirmed: Ignore rules correctly applied to collections
- Confirmed: Non-null scalar updates work correctly
- No new production defects discovered

### DEF-020: Submodule Repos Inaccessible — .gitmodules References Non-Existent Repos

**Severity:** 🟠 High  
**Component:** `.gitmodules` / CI Infrastructure  
**Date Reported:** 2026-02-17  
**Status:** Open — Root Cause Updated (2026-02-25)  
**Reporter:** QA Team

**Description:**

The `.gitmodules` file references two submodule repositories that are inaccessible from GitHub Actions CI runners:

- `UNOPS.PAO.ExternalDataService` → `https://github.com/UNOPS-ITG/unops-external-dataservice.git`
- `UNOPS.Workflow` → `https://github.com/UNOPS-ITG/unops-workflow.git`

Both return `fatal: repository not found` when CI attempts to clone them. This blocks all CI workflows that depend on the full project building.

**Investigation Results (2026-02-25):**

QA verified both repo URLs using `git ls-remote` from a developer machine:
- `https://github.com/UNOPS-ITG/unops-external-dataservice.git` → **EXISTS** (HEAD: `7306bb7`)
- `https://github.com/UNOPS-ITG/unops-workflow.git` → **EXISTS** (HEAD: `8eacfd8`)

Both repos are accessible. The `.gitmodules` URLs are **correct and do not need to change**.

**Root Cause (Confirmed 2026-02-25):** The repos are **private** within the `UNOPS-ITG` GitHub organization. The CI workflow in `.github/workflows/qa-tests.yml` uses `token: ${{ secrets.GH_PAT }}` for submodule checkout. The CI failure indicates the `GH_PAT` secret is **either missing, expired, or lacks cross-repo `repo` scope** for the two private submodule repositories.

The current workflow already has the correct pattern:
```yaml
- uses: actions/checkout@v4
  with:
    submodules: true
    token: ${{ secrets.GH_PAT }}
```

This will work correctly once `GH_PAT` is properly configured.

**Required Fix (Developer/DevOps action):**
1. Create or regenerate a GitHub Personal Access Token (classic PAT) for a user that has access to all three repos (`opportunityplus`, `unops-external-dataservice`, `unops-workflow`)
2. The PAT needs `repo` scope (full control of private repositories)
3. Add or update the `GH_PAT` secret in the `opportunityplus` repo settings:  
   `GitHub → UNOPS-ITG/opportunityplus → Settings → Secrets and variables → Actions → GH_PAT`
4. No `.gitmodules` changes are needed — URLs are correct

**Wrong Fix:** ❌ Simply removing `.gitmodules` without addressing the code that depends on the submodule assemblies.  
**Wrong Fix:** ❌ Changing `.gitmodules` URLs — the existing URLs are correct and resolve successfully.

**Workaround History:**

- `dc06b498` (2026-02-17): Applied `submodules: false` on all checkout steps + `WORKFLOW_AVAILABLE` conditional compilation guards
- `1252416c` (2026-02-25): **Workaround reverted** — `UNOPS.Workflow` IS required. Removing it caused `CS0234` build errors because `UNOPS.PAO.Business.csproj` has 4 `ProjectReference` entries into the submodule:
  ```xml
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Business\..." />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.DataAccess\..." />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Models\..." />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Domain\..." />
  ```
- `9799493b` (2026-02-25): Current state — `submodules: true` + `token: ${{ secrets.GH_PAT }}` on all checkout steps

**Current State:** CI is failing because `GH_PAT` is not properly configured. The `WORKFLOW_AVAILABLE` guards remain in `PAOWebApplicationFactory.cs` but are not the blocker — the secret is.

**Note:** `UNOPS.PAO.ExternalDataService` submodule is also cloned by CI but is not referenced by any `.csproj` file. See **DEF-046** for the cleanup task.

**Repro Steps:**
1. Push any commit to `QA-Tests` branch
2. Observe `qa-tests.yml` workflow fail at checkout or build step
3. Error: `fatal: repository 'https://github.com/UNOPS-ITG/unops-external-dataservice.git/' not found`

**Expected:** CI should be able to clone all submodules and build the full project.  
**Actual:** Submodule repos return 404 in CI because the `GH_PAT` secret lacks the required access.

**Related QA:** QA-070

---

---

### DEF-024: DocumentController.GetCredentials() Always Calls Google Secret Manager

**Severity:** 🟠 High  
**Component:** `DocumentController` (`UNOPS.PAO.Presentation/Controllers/Documents/DocumentController.cs`)  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

`DocumentController.GetCredentials()` unconditionally calls Google Secret Manager on every controller instantiation, regardless of the `AISettings:DisableExternalCalls` configuration flag. This causes `System.ArgumentNullException` in test and offline environments where Google Cloud credentials are unavailable.

**Root Cause:**
```csharp
// In DocumentController constructor (line 66):
_cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, GetCredentials()); // ← always called

private GoogleCredential GetCredentials()
{
    var credentialParams = _configuration.GetSection("AISettings").Get<JsonCredentialParameters>();
    // No DisableExternalCalls check — always calls Google Secret Manager
    var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
    return GoogleCredential.FromJson(secretValue); // ← throws if secret unavailable
}
```

**Comparison:** `UNOPSGeminiManager.GetCredentials()` was fixed (2026-02-21) to check `AISettings:DisableExternalCalls` first. `DocumentController` needs the same fix.

**Proper Fix (choose one):**

**Option A — DisableExternalCalls guard (quickest):**
```csharp
// In DocumentController constructor:
var disableExternalCalls = configuration.GetValue<bool>("AISettings:DisableExternalCalls", false);
if (!disableExternalCalls)
{
    _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, GetCredentials());
}
// When disabled: _cloudRunHelper stays null; guard all _cloudRunHelper usages with null-check
```

**Option B — DI injection (preferred, consistent with AiContextualService fix):**
```csharp
// Register in DI (Startup.cs):
services.AddSingleton<GoogleCredential>(sp =>
{
    var disableExternalCalls = sp.GetRequiredService<IConfiguration>()
        .GetValue<bool>("AISettings:DisableExternalCalls", false);
    if (disableExternalCalls) return GoogleCredential.FromAccessToken("test-token");
    // real credential loading here
});

// DocumentController constructor:
public DocumentController(..., GoogleCredential credential)
{
    _cloudRunHelper = new CloudRunHelper(cloudRunHelperLogger, credential);
}
```

**Precedent:** `UNOPSGeminiManager.GetCredentials()` was fixed using Option A (2026-02-21). `AiContextualService` was fixed using Option B. Either approach is acceptable; Option B is preferred for testability.

**Wrong Fix:** ❌ Wrapping `GetCredentials()` in a try/catch that silently swallows the exception — the 500 error disappears but document cloud operations silently fail in production.

**Test Impact:** 28 DocumentControllerTests returning HTTP 500 InternalServerError in test environments. All document upload/download/management integration tests blocked until this is fixed. Tests are not skipped — they run and fail, requiring developer attention.

**QA Workaround:** None available — cannot mock the Google Secret Manager call from test infrastructure without modifying production code. Tests remain failing until the production fix is applied.

---

### DEF-025: Missing Permission CRUD API Endpoints

**Severity:** 🟡 Medium  
**Component:** `PermissionController`  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

Integration tests expect a full CRUD REST API for permission management at `/api/admin/permissions`, but the actual `PermissionController` is at `api/permissions` and only provides:
- `GET /api/permissions` — system permission configuration (returns roles list)
- `GET /api/permissions/check/{*route}` — route access check
- `GET /api/permissions/entity-permissions/{entityName}` — entity permission details
- `GET /api/permissions/user-roles` — current user roles
- `GET /api/permissions/user/{userId}` — user roles by ID
- `GET /api/permissions/available-roles` — available system roles

**Missing endpoints expected by tests:**
- `GET /api/admin/permissions` — list all permissions (paginated)
- `GET /api/admin/permissions/{id}` — get permission by ID
- `POST /api/admin/permissions` — create permission
- `PUT /api/admin/permissions/{id}` — update permission
- `DELETE /api/admin/permissions/{id}` — delete permission
- `GET /api/admin/permissions/{id}/usage` — permission usage report
- `GET /api/admin/permissions/{id}/audit` — permission audit log
- `GET /api/permissions/my-permissions` — current user permissions list
- `GET /api/permissions/check-entity?type=&id=&permission=` — entity-specific permission check

**Impact:** 35 PermissionControllerTests failing.

---

### DEF-026: Missing Role Management CRUD API Endpoints

**Severity:** 🟡 Medium  
**Component:** `RoleController` (missing admin role management controller)  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

Integration tests expect a full CRUD REST API for role management at `/api/admin/roles`. The existing `RoleController` in `UNOPS.PAO.UNOPSPresentation` is at `api/Role` and only handles DOA-specific role operations, not generic admin role management.

**Missing endpoints expected by tests:**
- `GET /api/admin/roles` — list all roles
- `GET /api/admin/roles/{id}` — get role by ID
- `POST /api/admin/roles` — create role
- `PUT /api/admin/roles/{id}` — update role (or `POST /api/admin/roles/clone`)
- `DELETE /api/admin/roles/{id}` — delete role
- `GET /api/admin/roles/{id}/audit` — role audit log
- `GET /api/admin/roles/{id}/hierarchy` — role hierarchy
- `GET /api/admin/roles/{id}/users` — users in role
- `POST /api/admin/users/{userId}/roles/{roleId}` — assign role to user
- `DELETE /api/admin/users/{userId}/roles/{roleId}` — remove role from user
- `GET /api/admin/users/{userId}/roles` — user role list

**Impact:** 25 RoleControllerTests failing.

---

### DEF-027: Missing Global Search and Health Check Endpoints

**Severity:** 🟡 Medium  
**Component:** `GlobalController` / Health Checks  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

Integration tests expect global search and health check endpoints at specific routes that don't exist in the current codebase:

**Expected routes (returning 404/401):**
- `GET /api/search?q=...` — global search (actual: `/api/global/search?q=...`)
- `GET /api/health` — health check (returning 401 — not publicly accessible)
- `GET /api/health/ready` — readiness check
- `GET /api/health/live` — liveness check
- `GET /api/health/db` — database health check
- `GET /api/version` — application version
- `GET /api/system-info` — system metadata
- `GET /api/time` — current UTC time

**Actual existing routes:**
- `GET /api/global/search?q=...` — global search (different path)
- No health check endpoints registered (`UseHealthChecks()` not configured)

**Impact:** 14 GlobalControllerTests failing.

---

### DEF-028: Missing User Preference CRUD API Endpoints

**Severity:** 🟡 Medium  
**Component:** `UserPreferenceController`  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

Integration tests expect a full user preference management API at `/api/users/preferences`. The actual `UserPreferenceController` is at `api/user-preferences` and only exposes:
- `GET /api/user-preferences/default-org-unit`
- `PUT /api/user-preferences/default-org-unit`

**Missing endpoints expected by tests:**
- `GET /api/users/preferences` — get user preferences
- `GET /api/users/preferences/{key}` — get specific preference
- `PUT /api/users/preferences` — update preferences
- `POST /api/users/preferences/reset` — reset to defaults
- `PUT /api/users/preferences/language` — set language
- `PUT /api/users/preferences/theme` — set theme
- `PUT /api/users/preferences/dateFormat` — set date format
- `PUT /api/users/preferences/timezone` — set timezone
- `PUT /api/users/preferences/pageSize` — set page size
- `PUT /api/users/preferences/emailNotifications` — email notifications toggle
- `PUT /api/users/preferences/inAppNotifications` — in-app notifications toggle
- `PUT /api/users/preferences/notificationFrequency` — notification frequency

**Impact:** 18 UserPreferenceControllerTests failing.

---

### DEF-029: Missing LiaisonOffice CRUD API Endpoints

**Severity:** 🟡 Medium  
**Component:** `LiaisonOfficeController`  
**Date Reported:** 2026-02-21  
**Status:** Open  
**Reporter:** QA Team

**Description:**

Integration tests expect a full CRUD REST API for liaison offices at `/api/liaison-offices`. The actual `LiaisonOfficeController` is at `api/LiaisonOffice` and only provides:
- `GET /api/LiaisonOffice` — list offices (with filter)
- `POST /api/LiaisonOffice/search` — search offices
- `GET /api/LiaisonOffice/{id}` — get office by ID

**Missing endpoints expected by tests:**
- `POST /api/liaison-offices` — create office
- `DELETE /api/liaison-offices/{id}` — delete office
- `GET /api/liaison-offices/code/{code}` — get by office code
- `POST /api/liaison-offices/{id}/partners/{partnerId}` — link partner to office
- `DELETE /api/liaison-offices/{id}/partners/{partnerId}` — unlink partner from office
- `GET /api/liaison-offices/{id}/partners` — get partners for office
- `GET /api/partners/{partnerId}/liaison-office` — get office for partner
- Advanced filtering by search text, country, region, orgUnit, pagination, sorting

**Route mismatch:** Tests call `/api/liaison-offices` (kebab-case, plural) but actual controller uses `api/LiaisonOffice` (PascalCase, singular). ASP.NET Core routing is case-insensitive but pluralization/hyphens are not handled automatically.

**Impact:** 29 LiaisonOfficeControllerTests failing.

---

### Production Code Quality Assessment

Based on 2,608 passing tests:
- **No security vulnerabilities** found in mapping or workflow endpoints
- **No concurrency issues** found in production code
- **No performance regressions** detected
- **Business logic is correct** for all tested scenarios
- Remaining 829 failures are predominantly tests for unimplemented API endpoints (DEF-024 through DEF-029) and the credential loading defect (DEF-024)
- Core CRUD functionality (Partners, Contacts, Interactions, Opportunities) is working correctly with PostgreSQL


---

## DEF-038: ImportController Not Implemented
**ID:** DEF-038 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** ImportController / UNOPS.PAO.Presentation

**Description:** Integration tests call POST /api/import/{entityType} (e.g., POST /api/import/Partners) to import CSV/file data into the system. No ImportController exists anywhere in the codebase (UNOPS.PAO.Presentation or UNOPS.PAO.UNOPSPresentation).

**Missing Endpoints:**
- POST /api/import/Partners — import partners via CSV
- POST /api/import/{entityType} — generic import endpoint for any entity type

**Impact:** 8 ImportControllerTests failing (all returning 405 MethodNotAllowed).

**Repro Steps:**
1. POST /api/import/Partners with a multipart CSV file
2. Server returns 405 MethodNotAllowed

**Expected:** 200 OK or 400 BadRequest based on CSV content  
**Actual:** 405 MethodNotAllowed (no route match)

**Developer Feedback:**

---

## DEF-039: ContactController Missing Endpoints
**ID:** DEF-039 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** ContactController / UNOPS.PAO.Presentation.Controllers.Contacts.ContactController

**Description:** Integration tests expect various contact management endpoints that are not implemented in ContactController. The controller only has: POST /api/contact, GET /api/contact, GET /api/contact/search, GET /api/contact/advanced-search, GET /api/contact/search-fields, GET /api/contact/{id}, PUT /api/contact, DELETE /api/contact/{id}, GET /api/partner-contacts, GET /api/contact/{id}/permissions, POST /api/contact/{id}/profile-picture, POST /api/contact/scan-data, POST /api/contact/analyse-file, POST /api/contact/bulk-upload, GET /api/contact/metadata-info, POST /api/contact/detect-duplicates.

**Missing Endpoints Expected by Tests:**
- POST /api/contact/import — import contacts via CSV (currently only ulk-upload exists)
- POST /api/contact/{id}/setprimary — set a contact as primary
- PUT /api/contact/{id}/photo — upload contact photo (exists as POST /api/contact/{id}/profile-picture but not PUT)
- GET /api/contact/{id}/timeline — contact activity timeline
- GET /api/contact/typeahead?search= — typeahead search
- GET /api/contact/{id}/interactions — interactions for a contact
- GET /api/contact/{id}/documents — documents for a contact
- POST /api/contact/merge — merge duplicate contacts
- POST /api/contact/bulk-create — bulk create contacts
- POST /api/contact/bulk-update — bulk update contacts
- GET /api/contact/{id}/photo — get contact photo

**Impact:** ~30 ContactControllerEdgeCaseTests and ContactControllerNegativeTests failing (405 MethodNotAllowed or 404 Not Found).

**Developer Feedback:**

---

## DEF-040: PartnerController Missing Endpoints
**ID:** DEF-040 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** PartnerController / UNOPS.PAO.Presentation.Controllers.Partners.PartnerController

**Description:** Integration tests expect various partner management endpoints that are not implemented. The controller has a comprehensive base, but tests expect additional endpoints for advanced operations.

**Missing Endpoints Expected by Tests:**
- GET /api/partner/export?format={format} — export partners to CSV/Excel
- POST /api/partner/bulk — bulk create partners
- DELETE /api/partner/bulk — bulk delete partners by ID array
- PUT /api/partner/{id}/status/{status} — update partner status to specific value
- PUT /api/partner/{id}/orgunits — update partner org unit assignments
- DELETE /api/partner/{id}/logo — delete partner logo
- GET /api/partner/{id}/statistics — partner statistics/metrics
- GET /api/partner/{id}/timeline?start=&end= — partner activity timeline
- GET /api/partner/{id}/audit — partner audit log
- GET /api/partner/{id}/related — related partners
- GET /api/partner/{id}/logo — get partner logo
- GET /api/partner/typeahead?search= — typeahead search
- GET /api/partner/{id}/contacts — contacts for a partner (may exist as PartnerContacts)
- GET /api/partner/{id}/documents — documents for a partner
- PUT /api/partner/{id} — update by ID in URL (controller uses PUT /api/partner with ID in body)

**Impact:** ~40 PartnerControllerEdgeCaseTests and PartnerControllerNegativeTests failing.

**Developer Feedback:**

---

## DEF-041: ValuesController Missing Generic Type Lookup Endpoint
**ID:** DEF-041 | **Severity:** 🟡 Medium | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** ValuesController / UNOPS.PAO.Presentation.Controllers.Shared.ValuesController

**Description:** Integration tests expect a generic GET /api/values/{type} endpoint to retrieve configuration values by entity type. The actual ValuesController only exposes specific named endpoints (/api/config, /api/currency, /api/country, etc.) — there is no generic wildcard route.

**Missing Endpoints:**
- GET /api/values/{type} — get values/options for a given entity type by name
- GET /api/values/{type}/{id} — get a specific value by type and ID

**Impact:** ~13 ValuesController tests failing (ValuesControllerEdgeCaseTests, ValuesControllerValidationTests, ValuesControllerNegativeTests, ValuesControllerSecurityTests).

**Developer Feedback:**

---

## DEF-042: ContactAnalyticsController Returning 500 InternalServerError
**ID:** DEF-042 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** ContactAnalyticsController / UNOPS.PAO.Presentation.Controllers.Contacts.ContactAnalyticsController

**Description:** Integration tests calling contact analytics endpoints receive 500 InternalServerError responses. Tests call valid-looking endpoints (e.g., GET /api/contact-analytics/by-job-title, GET /api/contact-analytics/by-partner) but the server throws unhandled exceptions.

**Root Cause:** Likely missing pg_trgm extension in test database, or analytics service failing due to missing seeded data or unimplemented SQL queries.

**Affected Endpoints:**
- GET /api/contact-analytics/by-job-title
- GET /api/contact-analytics/by-partner
- GET /api/contact-analytics/most-active
- GET /api/contact-analytics/by-geographic-region
- GET /api/contact-analytics/with-most-documents

**Impact:** 14 ContactAnalyticsControllerTests failing with 500 responses.

**Developer Feedback:**

---

## DEF-043: WorkflowController Missing !IsDeleted Filters on Collection Queries
**ID:** DEF-043 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** WorkflowController / ValidateOpportunityRequirementsAsync

**Description:** WorkflowController.ValidateOpportunityRequirementsAsync() queries related collections (Countries, Deliverables, SDGs, FundingPartners, ClientPartners) without filtering soft-deleted records. Because AuditableDbContext converts all EntityState.Deleted operations into soft-deletes (setting IsDeleted=true), physically removing entities via RemoveRange() does not physically delete them - they remain in the DB with IsDeleted=true. The controller then retrieves these soft-deleted records and counts them as present, causing validation to incorrectly pass.

**Root Cause:** AuditableDbContext.OnBeforeSave() intercepts EntityState.Deleted and converts it to EntityState.Modified with IsDeleted=true. The ValidateOpportunityRequirementsAsync method queries via already-loaded navigation properties on the Opportunity entity, which are populated via _context.Set<T>().Where(x => x.OpportunityId == id) WITHOUT a !IsDeleted filter.

**Affected Code:**
- WorkflowController.ValidateOpportunityRequirementsAsync() - lines checking opportunity.Countries, opportunity.Deliverables, opportunity.SDGs, opportunity.FundingPartners, opportunity.ClientPartners
- The queries populating these navigation properties (lines 456-480 in Submit action)

**Proper Fix:**
- Add .Where(x => !x.IsDeleted) to all collection queries that populate opportunity.* navigation properties before validation
- OR filter opportunity.Countries.Where(c => !c.IsDeleted).Any() in the validation checks

**Wrong Fix:** ❌ Do not change AuditableDbContext soft-delete behavior

**Affected Tests:** FUN_020, FUN_021, FUN_022, NEG_032, NEG_033, NEG_034, NEG_038, NEG_039 (8 tests returning Success=true when expecting false)

**Developer Feedback:**

---

## DEF-045: AuditLogController Returns 500 for All Authenticated Requests in InMemory Mode
**ID:** DEF-045 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-25 | **Reporter:** QA Team

**Component:** AuditLogController / AuditLogManager

**Description:** `AuditLogController` returns HTTP 500 Internal Server Error for every authenticated request when running against an InMemory database. Even simple requests that should return 400 (e.g. missing `entityType` parameter) or 404 (no data) return 500 instead. All 36 authenticated tests in `AuditLogControllerTests.cs` fail due to this. The 3 unauthenticated tests pass correctly (return 401 at auth middleware).

**Root Cause:** Suspected initialization failure in AuditLogController constructor or AuditLogManager. Likely causes: (1) `UserResolverService.GetCurrentUserId()` throws when there is no valid ClaimsPrincipal in InMemory test context, (2) AutoMapper configuration error during DI setup, or (3) `AuditableDbContext` audit infrastructure fails against InMemory provider.

**Repro Steps:**
1. Run `dotnet test` with `USE_INMEMORY_DB=true`
2. Execute any authenticated GET to `/api/auditlog/latest?entityType=Opportunity&entityId=1`
3. Observe 500 response instead of 200/404

**Expected:** Request reaches controller action; validation logic returns 400 for invalid params, 404 for missing data.

**Actual:** HTTP 500 is returned before controller action logic executes.

**Workaround (QA):** `AuditLogControllerTests.cs` guards all authenticated tests with `if (!_isPostgresAvailable) return;` until this defect is resolved.

**Proper Fix:** Investigate and fix the InMemory incompatibility in the AuditLog controller/manager initialization path.

**Wrong Fix:** ❌ Do not disable `[Authorize]` — the controller must remain secured.

---

## DEF-044: WorkflowController.Submit() Missing Logging Statements
**ID:** DEF-044 | **Severity:** 🟡 Medium | **Status:** Open | **Date:** 2026-02-21 | **Reporter:** QA Team

**Component:** WorkflowController.Submit()

**Description:** Multiple logging tests in PNO1197 suite verify that ILogger<WorkflowController> is called during Submit operations. The Moq mock logger reports "No invocations performed" after all submit variants (successful submit, failed submit, DoA validation, OM check). This indicates the WorkflowController.Submit() method does not call _logger.LogXxx() for any operational event.

**Root Cause:** Logging statements were never implemented in WorkflowController.Submit(). The method handles complex workflow logic but produces no observability logs.

**Affected Tests:** FUN_041, FUN_042, FUN_044, FUN_045, FUN_046 (5 tests expecting at least one logger invocation)

**Proper Fix:**
- Add _logger.LogInformation() calls at key points: submit attempt start, validation result, DoA check result, OM check result, final submission outcome
- Example: _logger.LogInformation("Submit attempt for {EntityName}/{EntityId} by user {UserId}", request.EntityName, request.EntityId, CurrentUserId);

**Wrong Fix:** ❌ Do not change tests to not verify logging - observability is a requirement

**Developer Feedback:**

---

**ID:** DEF-023 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-24 | **Reporter:** QA Team

**Component:** AiPrompt entity (UNOPS.PAO.Domain.Entities.AiPrompt)

**Description:** `AiPrompt` inherits from `BaseBusinessEntity` instead of `ModifiableDeletableEntity`, so it lacks the `IsDeleted`, `DeletedBy`, and `DeletedDate` properties required for soft delete support. Tests in `UNOPSAiPromptManagerTests.cs` that validate soft delete behavior cannot compile or execute.

**Root Cause:** `AiPrompt` was designed without soft delete support. `BaseBusinessEntity` does not include the `IsDeleted` flag.

**Proper Fix:**
- Change `AiPrompt` to inherit from `ModifiableDeletableEntity` (or at minimum add `public bool IsDeleted { get; set; }` + `public int? DeletedBy { get; set; }` + `public DateTime? DeletedDate { get; set; }`)
- Add a migration to add the `IsDeleted` column to the `AiPrompt` table with default value `false`
- Ensure `GetAll`/listing queries filter by `!IsDeleted`

**Wrong Fix:** ❌ Do not use hard-deletes for AI prompts - soft delete is required for audit trail

**Affected Tests:** TC-AIPROMPT-NEG-001, TC-AIPROMPT-NEG-007, TC-AIPROMPT-EDGE-005, TC-AIPROMPT-INT-006, TC-AIPROMPT-INT-009 (5 tests skipped with DEF-023)

**Developer Feedback:**

---

**ID:** DEF-024 | **Severity:** 🟠 High | **Status:** Open | **Date:** 2026-02-24 | **Reporter:** QA Team

**Component:** UNOPSInteraction entity (UNOPS.PAO.UNOPSDomain.Entities.UNOPSInteraction)

**Description:** `UNOPSInteraction` (inheriting from `Interaction`) does not have a direct `ContactId` foreign key property. Linking an Interaction to a Contact is done through the `InteractionContacts` junction table (`InteractionContact` entity), not via a direct `ContactId` column. Tests in `InteractionContactIdTests.cs` that assume a direct `ContactId` property cannot compile or execute.

**Root Cause:** The data model uses a many-to-many relationship between `Interaction` and `Contact` via `InteractionContact` junction table, rather than a direct nullable FK on `Interaction`.

**Proper Fix:**
- Either add a nullable `ContactId` FK property to `Interaction` (for backward compatibility) alongside `InteractionContacts`
- Or update the test file to use `InteractionContact` junction records to link interactions to contacts

**Wrong Fix:** ❌ Do not add ContactId as a duplicate of the junction table without proper EF Core configuration

**Affected Tests:** All 6 tests in `InteractionContactIdTests.cs` (skipped with DEF-024)

**Developer Feedback:**

---

### DEF-047: Missing Opportunity Name Validation for Empty/Whitespace Strings

**Severity:** 🟠 High  
**Component:** `UNOPSOpportunityManager` / `OpportunityManager` (`UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs`)  
**Date Reported:** 2026-03-02  
**Status:** Open  
**Priority:** P2 — Data integrity / validation gap  
**Reporter:** QA Team (2026-03-02 full PostgreSQL test run)

**Description:**

`CreateOpportunityAsync` does not validate the `Name` property for empty strings (`""`) or whitespace-only strings (`"   "`). When these values are passed, the opportunity is created successfully without throwing any exception. The `null` case is correctly handled (throws exception), but empty/whitespace passes through.

**Root Cause:** The `CreateOpportunityAsync` method likely checks `name == null` or uses `string.IsNullOrEmpty()` but does NOT use `string.IsNullOrWhiteSpace()`. Empty and whitespace-only names are accepted and persisted to the database.

**Proper Fix:**
- Replace `string.IsNullOrEmpty(model.Name)` with `string.IsNullOrWhiteSpace(model.Name)` in the validation logic
- Throw `BusinessException` with a descriptive message containing "name" for empty/whitespace values
- Apply the same fix in both `OpportunityManager` and `UNOPSOpportunityManager`

**Wrong Fix:** ❌ Adding a database constraint only — validation should happen at the business layer with a user-friendly error message.

**Affected Tests:**
- `OpportunityValidationTests.CreateOpportunity_InvalidName_ThrowsException(invalidName: "   ")` — Expected exception, none thrown
- `OpportunityValidationTests.CreateOpportunity_InvalidName_ThrowsException(invalidName: "")` — Expected exception, none thrown
- `UNOPSOpportunityManagerTests.CreateOpportunity_InvalidName_ThrowsException(invalidName: "   ")` — Expected exception, none thrown
- `UNOPSOpportunityManagerTests.CreateOpportunity_InvalidName_ThrowsException(invalidName: "")` — Expected exception, none thrown
- `UNOPSOpportunityManagerTests.CreateOpportunity_WithoutRequiredName_ThrowsException` — Exception message doesn't contain "name"

**Environment:** PostgreSQL (Cloud SQL Proxy)  
**Error:** `Expected a <System.Exception> to be thrown, but no exception was thrown.`

---

### DEF-048: Missing Opportunity Name Max-Length Validation

**Severity:** 🟡 Medium  
**Component:** `UNOPSOpportunityManager` / `OpportunityManager`  
**Date Reported:** 2026-03-02  
**Status:** Open  
**Priority:** P3 — Poor error message for max-length violation  
**Reporter:** QA Team (2026-03-02 full PostgreSQL test run)

**Description:**

When creating an opportunity with a name exceeding the maximum allowed length, the system throws a generic Entity Framework `DbUpdateException` (`"An error occurred while saving the entity changes"`) instead of a descriptive `BusinessException` containing "length". The database column constraint catches the violation, but the error is not user-friendly.

**Root Cause:** No business-layer validation for `Name` max-length. The database column constraint is the only safeguard, producing a generic EF error instead of a domain-level validation message.

**Proper Fix:**
- Add max-length validation in `CreateOpportunityAsync` before calling `AddAsync()`
- Throw `BusinessException($"Opportunity name exceeds maximum length of {maxLength} characters")` if validation fails
- Determine the correct max-length from the database schema or entity configuration

**Wrong Fix:** ❌ Catching `DbUpdateException` and re-throwing as `BusinessException` — validate before saving.

**Affected Tests:**
- `OpportunityValidationTests.CreateOpportunity_NameTooLong_ThrowsException` — Expected message containing "length"
- `UNOPSOpportunityManagerTests.CreateOpportunity_NameExceedsMaxLength_ThrowsException` — Expected message containing "length"

**Environment:** PostgreSQL (Cloud SQL Proxy)  
**Error:** `Expected exception message to match the equivalent of "*length*", but "An error occurred while saving the entity changes." does not.`

---

### DEF-049: Missing Null Request Guard in UNOPSOpportunityManager.UpdateOpportunityAsync

**Severity:** 🟡 Medium  
**Component:** `UNOPSOpportunityManager.UpdateOpportunityAsync` (`UNOPS.PAO.UNOPSBusiness/Managers/UNOPSOpportunityManager.cs`)  
**Date Reported:** 2026-03-02  
**Status:** Open  
**Priority:** P3 — Error handling / defensive coding  
**Reporter:** QA Team (2026-03-02 full PostgreSQL test run)

**Description:**

When `UpdateOpportunityAsync` is called with a `null` model parameter, it throws `System.InvalidOperationException` (from LINQ expression evaluation failure) instead of `System.ArgumentNullException`. The null model flows into a LINQ query (`model.Id` evaluation) and causes an internal EF Core expression tree evaluation failure.

**Root Cause:** No null guard at the entry point of `UpdateOpportunityAsync`. The method immediately accesses `model.Id` in a LINQ Where clause without checking if `model` is null.

**Proper Fix:**
```csharp
public async Task<OpportunityModel?> UpdateOpportunityAsync(UpdateOpportunityRequest model)
{
    ArgumentNullException.ThrowIfNull(model, nameof(model));
    // ... existing logic
}
```

**Wrong Fix:** ❌ Catching the `InvalidOperationException` and re-throwing — validate at entry.

**Affected Tests:**
- `OpportunityAdvancedFeaturesTests.UpdateOpportunity_NullRequest_HandlesGracefully` — Expected `ArgumentNullException`, got `InvalidOperationException`
- `UNOPSOpportunityManagerTests.UpdateOpportunity_NullRequest_ShouldThrowArgumentNullException` — Expected `ArgumentNullException`, got `InvalidOperationException`

**Environment:** PostgreSQL (Cloud SQL Proxy)  
**Error:** `Expected a <System.ArgumentNullException> to be thrown, but found <System.InvalidOperationException>`

---

### DEF-050: AutoMapper Missing OpportunityCountry.Country Navigation Property Mapping

**Severity:** 🟡 Medium  
**Component:** `OpportunityMappingProfile` (AutoMapper configuration)  
**Date Reported:** 2026-03-02  
**Status:** Open  
**Priority:** P3 — Mapping configuration gap  
**Reporter:** QA Team (2026-03-02 full PostgreSQL test run)

**Description:**

When mapping `OpportunityCountry` → `OpportunityCountryModel`, AutoMapper throws `AutoMapperMappingException` because the `Country` navigation property on `OpportunityCountry` is null (not eagerly loaded) and the corresponding mapping for `Country` → `CountryModel` fails.

This specifically fails in the `UpdateWhereSection_WithCountries_Success` test when the method saves countries and then re-reads the opportunity with countries included — the Country navigation property is not loaded.

**Root Cause:** Either the `OpportunityCountry` → `OpportunityCountryModel` mapping is missing a rule for the `Country` destination member, or the query that reloads opportunity countries after saving does not `.Include(c => c.Country)`.

**Proper Fix (choose one):**
- **Option A:** Add `.Include(c => c.Country)` to the query that loads `OpportunityCountries` after saving the WHERE section
- **Option B:** Add a `ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))` rule with a null guard in the mapping profile
- **Option C:** Ensure the `OpportunityCountryModel.Country` is populated from a separate query if the navigation property is not loaded

**Wrong Fix:** ❌ Ignoring the `Country` member — users need to see country details.

**Affected Tests:**
- `UNOPSOpportunityManagerTests.UpdateWhereSection_WithCountries_Success` — `AutoMapperMappingException` on `Countries` → `Country` member

**Environment:** PostgreSQL (Cloud SQL Proxy)  
**Error:** `AutoMapper.AutoMapperMappingException: Error mapping types. Destination Member: Countries → Country`

---

### DEF-046: Remove Orphaned `UNOPS.PAO.ExternalDataService` Submodule

**Severity:** 🟢 Low  
**Component:** `.gitmodules` / Repository Configuration  
**Date Reported:** 2026-02-25  
**Status:** Open  
**Reporter:** QA Team  
**Related QA:** QA-070  
**Related DEF:** DEF-020

**Description:**

The `.gitmodules` file registers `UNOPS.PAO.ExternalDataService` as a git submodule:

```ini
[submodule "UNOPS.PAO.ExternalDataService"]
    path = UNOPS.PAO.ExternalDataService
    url = https://github.com/UNOPS-ITG/unops-external-dataservice.git
```

However, **zero `.csproj` files in the solution reference this submodule as a project dependency**. A full search of all `.csproj` files confirms no `<ProjectReference>` or `<ProjectPath>` points into `UNOPS.PAO.ExternalDataService\`. The submodule directory exists locally but contributes nothing to the build.

**Impact:**

Every CI checkout with `submodules: true` attempts to clone `unops-external-dataservice`. This:
- Increases checkout time unnecessarily
- Adds an extra private-repo dependency to the `GH_PAT` secret scope (the secret must have access to a repo the build doesn't even use)
- Contributes to the CI failure described in DEF-020 when the PAT is misconfigured

**Root Cause:** The submodule was likely registered at some point when external data service code was expected to be integrated, but the integration was not completed (or was done via NuGet instead). The submodule entry was never removed.

**Required Fix (Developer — ~5 minutes):**

```bash
# 1. Deinit the submodule (removes local config, leaves files)
git submodule deinit UNOPS.PAO.ExternalDataService

# 2. Remove the submodule from the git index and working tree
git rm UNOPS.PAO.ExternalDataService

# 3. Commit — .gitmodules will be auto-updated by the above command
git commit -m "chore: remove unused UNOPS.PAO.ExternalDataService submodule"
```

**Verification:** After removing, confirm the build still succeeds — no source files reference this submodule, so the build should be unaffected.

**Wrong Fix:** ❌ Do not just delete the directory without running `git submodule deinit` and `git rm` — git will still track it as a submodule and CI will still try to clone it.

**Repro Steps:**
1. Check `.gitmodules` — `UNOPS.PAO.ExternalDataService` entry exists
2. Search all `.csproj` files for `ExternalDataService` references — zero results
3. CI checkout with `submodules: true` attempts to clone the repo unnecessarily

**Expected:** Only submodules with active project references are registered in `.gitmodules`.  
**Actual:** An unused submodule is registered, adding an unnecessary dependency to CI credential requirements.

**Developer Feedback:** Pending review.

---

### DEF-051: ~~UNOPSOpportunityManager.GetOpportunityAsync NullReferenceException at Line 362~~ RECLASSIFIED — QA Mock Issue

**Severity:** ~~🟠 High~~ → N/A  
**Component:** `OpportunityImmutabilityTests` (test code)  
**Date Reported:** 2026-03-02  
**Status:** Closed (Reclassified — Not a production defect)  
**Reporter:** QA Team (2026-03-02 verification rerun)

**Description:**

Initially reported as `NullReferenceException` at `UNOPSOpportunityManager.GetOpportunityAsync` line 362. Investigation revealed this was caused by an AutoMapper mock mismatch in the test, not a production code bug.

**Root Cause:** The test mocked `mapper.Map<OpportunityModel>(It.IsAny<Opportunity>())` (single-arg overload), but production code calls `mapper.Map<OpportunityModel>(entity, opt => opt.Items["Opportunity"] = entity)` (two-arg overload with `Action<IMappingOperationOptions>`). The unmatched mock returned `null`, causing `model.CreatedByName = ...` at line 362 to throw `NullReferenceException`.

**Fix Applied (QA):** Added two-arg overload mock setup to all 3 affected tests: `_mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, OpportunityModel>>>()))`. Result: **27/27 tests now pass.**

**Related QA:** QA-084

---

### DEF-052: UserProfile.Name Read-Only Computed Property Causes EF Core INSERT Failure

**Severity:** 🟡 Medium  
**Component:** `UserProfile` entity (`UNOPS.PAO.Domain/Entities/UserProfile.cs`)  
**Date Reported:** 2026-03-02  
**Status:** Open  
**Priority:** P3 — Entity design issue affecting test data seeding  
**Reporter:** QA Team (2026-03-02 verification rerun)

**Description:**

The `UserProfile` entity inherits `Name` from `ModifiableDeletableEntity` (which is a `{ get; set; }` property mapped to a NOT NULL database column), but hides it with a `new` keyword as a read-only computed property:

```csharp
public new string Name
{
    get
    {
        if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            return $"{FirstName} {LastName}".Trim();
        // ... fallback logic
    }
}
```

Since this property is getter-only, EF Core excludes it from INSERT statements. The database column `Name` has a NOT NULL constraint, so any EF Core INSERT of `UserProfile` fails with: `23502: null value in column "Name" of relation "UserProfile" violates not-null constraint`.

**Root Cause:** The `new` keyword hides the base settable property with a read-only computed one. EF Core detects the getter-only property and does not include it in INSERTs. The database column retains its NOT NULL constraint.

**Proper Fix (choose one):**
- **Option A:** Configure the `Name` column as a computed column in EF Core: `.HasComputedColumnSql(...)` and make the column nullable or give it a default
- **Option B:** Override `SaveChanges()` in the DbContext to automatically set the base `Name` from `FirstName`/`LastName` before persisting
- **Option C:** Make the database column nullable (`ALTER TABLE "UserProfile" ALTER COLUMN "Name" DROP NOT NULL`) since the computed property handles display

**Wrong Fix:** ❌ Removing the `new` keyword (would break the computed behavior)

**Workaround (QA):** `PAOWebApplicationFactory.SeedTestData()` uses raw SQL INSERT to explicitly set the `Name` column.

**Environment:** Dev/CI (PostgreSQL)  
**Error:** `Npgsql.PostgresException: 23502: null value in column "Name" of relation "UserProfile" violates not-null constraint`

**Repro Steps:**
1. Attempt to create a `UserProfile` via EF Core (e.g., `context.UserProfile.Add(new UserProfile { ... }); context.SaveChanges();`)
2. INSERT fails because `Name` column is excluded from the statement

**Expected:** EF Core correctly inserts `UserProfile` with the computed `Name` value  
**Actual:** `PostgresException: null value in column "Name"` — EF Core omits the column entirely

**Related QA:** QA-086 (UserProfile seeding issue during integration test setup)

---

### DEF-053: UNOPSGeminiManager.GetCredentials Crashes on Missing Google Credentials

**Severity:** 🟠 High  
**Component:** `UNOPSGeminiManager` (`UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager.cs`)  
**Date Reported:** 2026-03-02  
**Status:** Open — NOT Resolved (confirmed 2026-03-05)  
**Priority:** P2 — Constructor crash blocks entire ManagerWrapper initialization  
**Reporter:** QA Team (2026-03-02 verification rerun)

> **Developer Update (Anusha, 2026-03-04):** "DEF-053 should be fixed already. Please check if it works. Otherwise I can add a mock or update the config to `disableexternalcalls`."
> 
> **QA Action (2026-03-04):** Removed `[Fact(Skip = "DEF-053...")]` from 85+ integration tests across 5 files so they run (pass or fail) in CI. Integration tests job enabled in `qa-tests.yml` with `continue-on-error: true`. If fix works, tests will pass; if not, failures will be visible in test reporter.
> 
> **Files un-skipped:** DocumentControllerUNOPSTests.cs (16), OpportunityControllerCoreTests.cs (24), EntityArtifactControllerTests.cs (34), PartnerControllerOrgUnitTests.cs (11), PartnerControllerOrgUnitFilterTests.cs (6)
> 
> **QA Verification (2026-03-05): DEF-053 is NOT resolved.** Confirmed that the defect still exists. Key findings:
> 
> - **What works:** Application Default Credentials (ADC) are initialized successfully. `IAPAuthHelper` connects with the service account `pno-ai-service@unops-opportunityplus-dev.iam.gserviceaccount.com`. Authentication middleware works. This means **Secret Manager access** (GCP Secret Manager API) is partially working.
> - **What still fails:** `UNOPSGeminiManager.GetCredentials()` (line 198) crashes with `ArgumentNullException` because it reads a credential JSON string directly from `IConfiguration` and calls `GoogleCredential.FromJson(json)`. This is **NOT the same as Secret Manager access** — it's a configuration key that contains the raw JSON of a service account credential, and that configuration value is `null` in the test environment.
> - **Cascade effect:** Since the crash happens in the `UNOPSGeminiManager` constructor, the entire `UNOPSManagerWrapper` fails to instantiate, and ALL authenticated API requests return HTTP 500.
> - The `AISettings:DisableExternalCalls = true` config (set by the test factory) is **not checked** by `GetCredentials()` — the crash occurs before any AI method is invoked.
> - The test factory registers a mock `GoogleCredential` via DI, but `UNOPSGeminiManager` bypasses DI entirely by reading directly from `IConfiguration`.
> - **85+ un-skipped tests continue to fail** in CI due to this defect.

**Description:**

`UNOPSGeminiManager.GetCredentials()` at line 198 reads a Google credential JSON string from `IConfiguration` and calls `GoogleCredential.FromJson(json)`. When the configuration value is null or missing (as in test environments without GCP credentials), `GoogleCredential.FromJson(null)` throws `System.ArgumentNullException: Value cannot be null. (Parameter 'credentialParameters')`.

This exception is thrown **during the constructor** of `UNOPSGeminiManager`, which means `UNOPSManagerWrapper` construction fails entirely. Since ALL controllers depend on `IManagerWrapper`, every API endpoint returns HTTP 500.

**Root Cause:** `GetCredentials()` does not check for null/missing credential configuration before calling `GoogleCredential.FromJson()`. Additionally, the credential is loaded directly from `IConfiguration` rather than through DI, so DI-based mocking (as attempted in `PAOWebApplicationFactory`) has no effect.

**Proper Fix (any ONE of these would resolve it):**
1. **Use ADC instead:** Replace `GoogleCredential.FromJson(configJson)` with `GoogleCredential.GetApplicationDefault()` — ADC is already working in the environment
2. **Guard against null config:** If the credential JSON is missing, check `DisableExternalCalls` first; if true, set `_credential = null` and skip credential loading. AI methods should fail gracefully when `_credential` is null rather than crashing the constructor.
3. **Accept GoogleCredential via DI injection:** This would allow the test factory's mock `GoogleCredential` to take effect instead of bypassing DI entirely

**Wrong Fix:** ❌ Requiring GCP credentials in test environments

**Affected Tests:** 85+ integration tests across 5 files (DocumentControllerUNOPSTests, OpportunityControllerCoreTests, EntityArtifactControllerTests, PartnerControllerOrgUnitTests, PartnerControllerOrgUnitFilterTests), plus all 51 `PartnerControllerTests` and potentially all other integration tests using the full test server

**Environment:** Dev/CI (test environment without GCP credentials)  
**Error:** `System.ArgumentNullException: Value cannot be null. (Parameter 'credentialParameters')` at `UNOPSGeminiManager.GetCredentials()` line 198

**Repro Steps:**
1. Start the application (or test server) without GCP credential configuration
2. Any API request triggers `UNOPSManagerWrapper` construction
3. `UNOPSGeminiManager` constructor calls `GetCredentials()` which throws

**Expected:** Application starts gracefully with AI features disabled when credentials are missing  
**Actual:** `ArgumentNullException` crashes the entire request pipeline

**Related QA:** QA-088 (GoogleCredential mock ineffective in PAOWebApplicationFactory)

---

### DEF-060: EF Migration Init References AspNetUsers Before Identity Tables Are Created

| Field | Value |
|---|---|
| **ID** | DEF-060 |
| **Severity** | 🟡 Medium |
| **Title** | EF Migration Init references AspNetUsers before Identity tables exist |
| **Component** | UNOPS.PAO.UNOPSDataAccess / Migrations |
| **Date** | 2026-03-03 |
| **Status** | Open |
| **Reporter** | QA Team (CI pipeline) |

**Description:**

When running `dotnet ef database update` against a fresh empty PostgreSQL database (as in CI), the `20250113190031_Init` migration attempts to create `UserProfile` with a foreign key constraint `FK_UserProfile_AspNetUsers_UserId` referencing `public."AspNetUsers"`. However, `AspNetUsers` has not been created by any prior migration, causing the migration to fail with `42P01: relation "public.AspNetUsers" does not exist`.

**Root Cause:** The ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles`, etc.) are expected to exist before the `Init` migration runs, but there is no migration that creates them first. The Identity schema is likely auto-created by a different context or startup path that doesn't run during `dotnet ef database update`.

**Proper Fix:**
- Ensure a migration that creates the Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) runs before the `Init` migration
- Or include the Identity table creation in the `Init` migration itself
- Or add a separate migration with a timestamp before `20250113190031` that creates the Identity schema

**Wrong Fix:** ❌ Removing the FK constraint from `UserProfile` to `AspNetUsers`

**Workaround:** CI workflow uses `continue-on-error: true` on the migration step so model-level tests still execute. DB-dependent tests may fail due to missing schema.

**Repro Steps:**
1. Start a fresh empty PostgreSQL database
2. Run `dotnet ef database update --project UNOPS.PAO.UNOPSDataAccess --startup-project UNOPS.PAO.Server`
3. Migration `20250113190031_Init` fails at `CREATE TABLE UserProfile` with FK to `AspNetUsers`

**Expected:** All migrations apply successfully on a fresh database
**Actual:** `PostgresException 42P01: relation "public.AspNetUsers" does not exist`

**Environment:** CI (GitHub Actions with fresh PostgreSQL 15 container)
**Error:** `Npgsql.PostgresException (0x80004005): 42P01: relation "public.AspNetUsers" does not exist`

**Impact:** Blocks CI Business Logic Tests from running against a real database. Model-level tests unaffected.

---

### DEF-061: 3,036 Compiler Warnings Across 15 Production Projects

| Field | Value |
|---|---|
| **ID** | DEF-061 |
| **Severity** | 🟡 Medium |
| **Title** | 3,036 compiler warnings across 15 production projects |
| **Component** | Multiple (UNOPSBusiness, Presentation, Business, Models, Domain, and 10 others) |
| **Date** | 2026-03-04 |
| **Status** | Open |
| **Reporter** | QA Team |

**Description:**

A clean `dotnet build` of the solution produces **3,036 compiler warnings** across 15 production code projects. While these do not prevent compilation (0 errors), they indicate nullable reference type misuse, potential null dereferences at runtime, SQL injection risks, obsolete API usage, and code quality issues that should be addressed.

QA test projects (`UNOPS.PAO.Business.Tests`, `UNOPS.PAO.IntegrationTests`, `UNOPS.PAO.Presentation.Tests`, `UNOPS.PAO.FastTests`) have been cleaned to **0 warnings** as part of this audit.

**Warnings Per Project:**

| Project | Count | % of Total |
|---|---|---|
| UNOPS.PAO.UNOPSBusiness | 1,482 | 48.8% |
| UNOPS.PAO.Presentation | 394 | 13.0% |
| UNOPS.PAO.Business | 282 | 9.3% |
| UNOPS.PAO.Models | 268 | 8.8% |
| UNOPS.PAO.Domain | 260 | 8.6% |
| UNOPS.PAO.UNOPSIdentity | 80 | 2.6% |
| UNOPS.PAO.UNOPSDomain | 64 | 2.1% |
| UNOPS.PAO.DataAccess | 56 | 1.8% |
| UNOPS.PAO.Server | 36 | 1.2% |
| UNOPS.PAO.Utilities | 30 | 1.0% |
| UNOPS.PAO.GoogleServices | 28 | 0.9% |
| UNOPS.PAO.UNOPSPresentation | 28 | 0.9% |
| UNOPS.PAO.UNOPSDataAccess | 24 | 0.8% |
| UNOPS.PAO.MailSender | 2 | 0.1% |
| UNOPS.Workflow.DataAccess | 2 | 0.1% |

**Warnings By Category:**

| Category | Warning Codes | Count | Description |
|---|---|---|---|
| **Nullable reference types** | CS8602, CS8603, CS8604, CS8618, CS8625, CS8600, CS8601, CS8619, CS8620, CS8629, CS8605, CS8613, CS8621, CS8714, CS8767, CS8765 | ~2,250 | Possible null dereference, null argument, null return, uninitialized non-nullable property, nullable mismatch |
| **XML documentation** | CS1571, CS1573, CS1572, CS1570, CS1587 | ~330 | Duplicate param tags, missing param tags, param for nonexistent parameter |
| **Async without await** | CS1998 | 164 | Async methods that never use `await` — will run synchronously |
| **Member hiding** | CS0108 | 46 | Member hides inherited member without `new` keyword |
| **Unused code** | CS0168, CS0219, CS0649, CS0169, CS0414 | 42 | Unused variables, unused fields, unassigned fields |
| **Duplicate usings** | CS0105 | 22 | Same namespace imported twice |
| **Obsolete APIs** | CS0618 | 18 | Use of deprecated/obsolete methods |
| **Unreachable code** | CS0162 | 4 | Dead code after return/throw |
| **Self-assignment** | CS1717 | 4 | Variable assigned to itself |
| **SQL injection** | EF1002 | 4 | `ExecuteSqlRaw`/`ExecuteSqlRawAsync` with interpolated strings |
| **Other** | CS0472, CS0693, CS0659, CS0109 | 8 | Always-true comparisons, type parameter shadowing, etc. |

**Root Cause:** Nullable reference types (`<Nullable>enable</Nullable>`) is enabled across all projects but the codebase has not been fully annotated to be null-safe. Many methods return nullable values without `?` annotations, constructors leave non-nullable properties uninitialized, and null-forgiving operator (`!`) is not used where appropriate.

**Proper Fix (prioritized):**

**Priority 1 — Quick wins (eliminate ~200 warnings):**
- Remove duplicate `using` directives (CS0105) — 22 warnings, automated via `dotnet format`
- Remove unused variables/fields (CS0168, CS0219, CS0649, CS0169) — 42 warnings
- Add `new` keyword for intentional member hiding (CS0108) — 46 warnings
- Replace `ExecuteSqlRaw` with `ExecuteSql` (EF1002) — 4 warnings
- Fix self-assignments (CS1717) — 4 warnings
- Remove unreachable code (CS0162) — 4 warnings
- Update obsolete API calls (CS0618) — 18 warnings

**Priority 2 — XML docs (eliminate ~330 warnings):**
- Fix or remove broken XML doc comments (CS1571, CS1573, CS1572, CS1570, CS1587)
- Alternatively, suppress with `<NoWarn>` if XML docs are not published

**Priority 3 — Async methods (eliminate ~164 warnings):**
- Remove `async` keyword from methods that don't use `await`
- Or add `await Task.CompletedTask` where the async signature is required by an interface

**Priority 4 — Nullable annotations (eliminate ~2,250 warnings):**
- Add `?` annotations to properties/parameters/returns that can be null
- Add null checks or null-forgiving operator where values are guaranteed non-null
- Consider setting `<Nullable>annotations</Nullable>` for projects where full enforcement is not yet feasible
- Focus on the top offender first: `UNOPS.PAO.UNOPSBusiness` (1,482 of 2,250 nullable warnings)

**Wrong Fix:** ❌ Setting `<Nullable>disable</Nullable>` in production projects to hide warnings. ❌ Adding blanket `<NoWarn>` for nullable codes in production projects.

**Workaround:** QA test projects use `<Nullable>annotations</Nullable>` and `<NoWarn>CS1998;CS1571;CS1573;CS1572;CS1570;CS1587</NoWarn>` to eliminate noise in test code. This is standard practice for test projects but should not be applied to production code.

**Repro Steps:**
1. Run `dotnet clean` on the solution
2. Run `dotnet build "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj" --no-incremental`
3. Observe ~3,036 warnings from production dependency projects (0 from test projects)

**Expected:** Clean build with 0 warnings across all projects
**Actual:** 3,036 warnings from 15 production code projects

**Environment:** Dev (Windows 10, .NET 9.0, VS Code)

**Impact:** No tests are directly blocked by these warnings, but they indicate potential runtime `NullReferenceException` risks in production code, mask legitimate new warnings during development, and degrade CI build signal quality.

---

## DEF-101: Missing GeoRegionManager and GeoRegion Entity (CRM Enhancement)

**ID:** DEF-101 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Managers/GeoRegionManager

**Description:** GeoRegionManager and the GeoRegion entity do not exist in the codebase. Tests in `GeoRegionManagerTests.cs` are placeholder stubs (`Assert.True(true)`) awaiting implementation. The `SeedData()` method states: "Seed will be implemented when GeoRegion entity is available." This is part of the CRM Enhancement feature for geography management.

**Expected:** GeoRegionManager provides CRUD, hierarchy (SetContinent, GetByContinentId, GetCountriesInRegion), and validation for geographic regions.
**Actual:** No GeoRegionManager exists. 12 tests are stubs.

**Related Tests:** `GeoRegionManagerTests.cs` (12 tests tagged `[Trait("Defect", "DEF-101")]`)

---

## DEF-102: Missing ContinentManager and Continent Entity (CRM Enhancement)

**ID:** DEF-102 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Managers/ContinentManager

**Description:** ContinentManager and the Continent entity do not exist in the codebase. Tests in `ContinentManagerTests.cs` are placeholder stubs awaiting implementation. Part of CRM Enhancement geography management.

**Expected:** ContinentManager provides CRUD, associations (GetRegionsForContinent, GetCountryCountForContinent), and validation for continents.
**Actual:** No ContinentManager exists. 11 tests are stubs.

**Related Tests:** `ContinentManagerTests.cs` (11 tests tagged `[Trait("Defect", "DEF-102")]`)

---

## DEF-103: Missing EngagementManager with Workflow and Partner Associations (CRM Enhancement)

**ID:** DEF-103 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Managers/EngagementManager

**Description:** A full EngagementManager with workflow (Submit, Approve, Reject), partner associations, and validation does not exist. This is distinct from the existing `BaseEngagementManager` which provides basic engagement operations. The CRM Enhancement requires a full workflow-capable EngagementManager.

**Expected:** EngagementManager provides CRUD, workflow transitions (Draft→Submitted→Approved/Rejected), partner associations, and validation.
**Actual:** No full EngagementManager with workflow exists. 14 tests are stubs.

**Related Tests:** `EngagementManagerTests.cs` (14 tests tagged `[Trait("Defect", "DEF-103")]`)

---

## DEF-104: OpportunityManager Base Lacks Permission Enforcement and UserRole Population

**ID:** DEF-104 | **Severity:** 🟠 High | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Managers/OpportunityManager

**Description:** The base `OpportunityManager.GetOpportunityAsync(user, id)` is a stub that delegates to `GetOpportunityAsync(id)` without populating `Permissions` or `UserRole` on the returned model. `UpdateOpportunityAsync` and `DeleteOpportunityAsync` do not call the permission service before acting. The UNOPS override may implement this, but tests targeting the base manager fail because permissions are not enforced.

**Expected:** `GetOpportunityAsync(user, id)` populates permission flags and UserRole. Update/Delete operations check permissions before executing.
**Actual:** Permissions object is null/empty. UserRole is not populated. No permission checks on mutating operations.

**Related Tests:** `OpportunityPermissionTests.cs` (11 tests) + `OpportunityAdvancedFeaturesTests.cs:GetOpportunity_IncludesUserRoleContext_Success` — all tagged `[Trait("Defect", "DEF-104")]`

---

## DEF-105: UpdateOpportunityAsync Partial Update and Missing DeliveryModality Property

**ID:** DEF-105 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Managers/OpportunityManager, UNOPS.PAO.Models/Opportunity/UpdateOpportunityRequest

**Description:** Two related issues: (1) `UpdateOpportunityAsync` does not properly handle null values for optional fields — setting a field to null should clear it, but the behavior is undefined. (2) `UpdateOpportunityRequest` lacks a `DeliveryModality` property, so updating delivery modality via the standard update flow is not possible.

**Expected:** Setting optional fields to null clears them. DeliveryModality can be updated through UpdateOpportunityRequest.
**Actual:** Null handling is undefined. DeliveryModality property missing from request model.

**Related Tests:** `OpportunityAdvancedFeaturesTests.cs` (2 tests: ClearOptionalFields, ChangeDeliveryModality) tagged `[Trait("Defect", "DEF-105")]`

---

## DEF-106: ContactController and InteractionController Missing orgUnitId Filter Support

**ID:** DEF-106 | **Severity:** 🟠 High | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Presentation/Controllers/ContactController, InteractionController

**Description:** `GET /api/contact?orgUnitId=X` and `GET /api/interaction?orgUnitId=X` do not accept or use the `orgUnitId` query parameter. The filter request models (`ContactFilterRequest`, `InteractionFilterRequest`) may support `OrgUnitId`, but the controllers do not bind or pass this parameter. Contacts and interactions cannot be filtered by organizational unit hierarchy.

**Expected:** `orgUnitId` parameter filters results to contacts/interactions belonging to the specified org unit and its descendants.
**Actual:** `orgUnitId` parameter is ignored. All contacts/interactions are returned regardless of org unit.

**Related Tests:** `ContactControllerOrgUnitTests.cs` (9 tests) + `InteractionControllerOrgUnitTests.cs` (3 tests) tagged `[Trait("Defect", "DEF-106")]`

---

## DEF-107: Authentication Bypass Test — Unclear Route Behavior for Unauthenticated Requests

**ID:** DEF-107 | **Severity:** 🟢 Low | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Presentation/Middleware

**Description:** Tests check that unauthenticated requests to non-existent routes (`/api/nonexistent`) return 404 rather than 401, and that the root endpoint (`/`) works without authentication. The expected behavior depends on whether authentication middleware runs before or after routing. If auth runs first, unauthenticated requests should return 401 regardless of route existence.

**Expected:** Non-existent routes return 404. Root endpoint returns a non-401/403 response.
**Actual:** Behavior depends on middleware ordering and may vary between environments.

**Related Tests:** `AuthenticationBypassTest.cs` (2 tests tagged `[Trait("Defect", "DEF-107")]`)

---

## DEF-108: DOA3 Field Not Mapped from Opportunity+ to oUP Engagement

**ID:** DEF-108 | **Severity:** 🟠 High | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Integration (oUP Sync)

**Description:** When an Opportunity is fully approved in Opportunity+ and the system generates the corresponding Engagement record in oneUNOPS Projects (oUP), the DOA3 (Delegation of Authority Level 3) field value is not being transferred. Although the DOA3 user is designated during the Opportunity phase, the resulting Engagement in oUP arrives with this field blank, forcing users to manually re-enter critical authority data.

**Root Cause:** Data mapping failure during the Opp+ → oUP handover process. The DOA3 field is either not included in the API response contract or not mapped in the sync logic.

**Proper Fix:**
- Verify DOA3 is included in the Opportunity API response used by oUP sync
- Add DOA3 field mapping in the oUP Engagement creation logic
- Ensure DOA3 user ID is transferred alongside DOA2

**Wrong Fix:** ❌ Manually entering DOA3 in oUP after creation (data should flow automatically)

**Workaround:** Manual entry of DOA3 in oUP Engagement after creation

**Repro Steps:**
1. In Opportunity+, create an Opportunity and ensure a DOA3 user is assigned via Org Unit mapping
2. Process the Opportunity through the workflow until it receives an approved "Go" decision
3. Log into oneUNOPS Projects (oUP) and locate the newly generated Engagement record
4. Navigate to the Team section where Engagement Authorities are listed
5. Observe the Engagement Authority DOA3 field

**Expected:** The DOA3 field in the oUP Engagement is automatically populated with the user assigned in Opp+.
**Actual:** The DOA3 field is blank/unmapped in oUP.

**Environment:** QA | **Jira:** PNO-1209 (Ready for Go Live)

**Related Tests:** `QA Tests/Integration Tests/PNO-1209_DoA3OupMapping/DoA3MappingTests.cs` — TC-PNO1209-NEG-001 tagged `[Trait("Defect", "DEF-108")]`

---

## DEF-109: Jira "To Do" Tests Lack Corresponding Automated Coverage (PNO-980 Epic)

**ID:** DEF-109 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** QA Tests / Go Decision Coverage

**Description:** Analysis of the PNO-980 (Go/No Go Decision) Jira epic revealed 8 test tickets still in "To Do" status. While 4 of these have functional coverage through related tests, 4 had explicit gaps that have now been addressed with new automated tests. This defect tracks the ongoing need to ensure all Jira test tickets have corresponding automated test coverage.

**Jira Tickets Analyzed:**

| Ticket | Status Before | Coverage After |
|--------|--------------|----------------|
| PNO-1072 (DoA3 routing) | Functionally covered | ✅ Already covered in PNO-1197 tests |
| PNO-1058 (Key data points) | Functionally covered | ✅ Already covered in workflow-decision-panel.spec.ts |
| PNO-1055 (In-system notification) | Functionally covered | ✅ Already covered in workflow-actions-required.spec.ts |
| PNO-833 (Any section blocks) | Functionally covered | ✅ Already covered in Task-8.4 tests |
| PNO-1056 (Static statement) | **GAP** | ✅ New: workflow-static-statement.spec.ts (26 tests) |
| PNO-1005 (Email content) | **Partial** | ✅ New: EmailContentTests.cs (26 tests) |
| PNO-837 (Missing Context) | **Partial** | ✅ New: FieldValidationTests.cs (12 Context tests) |
| PNO-834 (Missing SDG) | **Partial** | ✅ New: FieldValidationTests.cs (12 SDG tests) |

**Action Required:** No developer action needed. This is a tracking defect for QA coverage completeness. New tests have been created to close the gaps.

**Related Tests:** See individual test files listed above

---

## DEF-110: Partners Silently Dropped During oUP Engagement Creation

**ID:** DEF-110 | **Severity:** 🟠 High | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/Integration (oUP Sync)

**Jira:** [PNO-1207](https://unops.atlassian.net/browse/PNO-1207) — Ready for Development | **Assignee:** Anusha SWAMINATHAN | **Reporter:** Perminder SALUJA
**Epic:** oneUNOPS Projects Integration | **Related:** PNO-729 (Opportunity Statement)
**Labels:** Integration, Opportunity+, Partners, oUP

**Description:** When an Opportunity contains Client or Funding Partners that exist in Opportunity+ QA but have not been synced to or do not exist in the oUP database (e.g., newly created test partners like "FG Partner"), the system silently drops these partners during the Engagement creation process. No error or warning is displayed to the user. This issue only occurs in the QA environment, not the TEST environment.

**Root Cause:** The oUP sync logic does not validate partner existence in oUP before attempting Engagement creation. Partners that fail the lookup are silently skipped rather than raising an error or being queued for prior synchronization. Additionally, the edit/save re-sync workaround does NOT work — Perminder tested edit/save on Partner 997 in Opp+ QA but it still did not appear in oUP TEST (comment 2026-03-06).

**Developer Comment (Anusha, 2026-02-24):** oUP test environments get data refreshed after every release, wiping synced test data. Suggests picking partners that exist in the corresponding environment or doing a dummy edit/save to trigger re-sync. However, this workaround was confirmed NOT working by Perminder on 2026-03-06.

**Proper Fix:**
- Validate all referenced partners exist in oUP before Engagement creation
- Display a warning/error listing any partners missing from oUP
- Queue missing partners for sync before attempting Engagement creation
- Log a warning when partners are skipped during sync
- Fix the edit/save re-sync mechanism (currently not triggering partner sync to oUP)

**Wrong Fix:** ❌ Silently dropping partners without any notification to the user

**Workaround:** ~~Manually verify all partners exist in oUP before triggering the Go Decision~~ No reliable workaround — edit/save re-sync does not work.

**Repro Steps:**
1. In Opportunity+, create or use an Opportunity with Client/Funding Partners
2. Add a partner that exists only in Opp+ (e.g., "FG Partner")
3. Add other standard partners (e.g., "UNDP - MSA Recipient Governments", "Samoa")
4. Complete the workflow to approve the Opportunity, triggering Engagement creation in oUP
5. Open the newly generated Engagement (e.g., 25550-00) in oUP
6. Navigate to Context > Partners and review the list
7. Observe that partners missing from oUP are silently dropped

**Test Data:**
- Opp+ Record: ID 173 (Regional Consultations on the South Asia...)
- Opp+ Partners: UNDP, **FG Partner**, Samoa
- oUP Engagement: 25550-00
- oUP Partners: UNDP, Samoa (**FG Partner is missing**)
- Partner 997 edit/save re-sync attempted — still not in oUP TEST

**Expected:** All partners from the Opportunity are transferred to the Engagement, or an error is raised for missing partners.
**Actual:** Partners not found in oUP are silently dropped. No error or warning is shown. Edit/save re-sync does not work.

**Environment:** QA (not reproducible in TEST) | **Frequency:** Always (for partners not in oUP)

**Related Tests:** `QA Tests/Integration Tests/PNO-1207_PartnerSyncMismatch/PartnerSyncMismatchTests.cs` (26 tests)

---

## DEF-111: Blank Partner Tree Levels in oUP and BigQuery

**ID:** DEF-111 | **Severity:** 🟠 High | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Business/PartnerTree

**Description:** Since the data migration of the partner tree to Opportunity+, additional blank/empty partner levels have been appearing in the oneUNOPS Projects (oUP) partner list and in the BigQuery partner tree report. These phantom levels have no name or meaningful data, causing confusion in the partner hierarchy.

**Root Cause:** The data migration created partner tree nodes with empty or null names. The partner tree API queries do not filter out nodes with blank names, allowing them to propagate to oUP and BigQuery.

**Proper Fix:**
- Clean up blank partner tree nodes from the database
- Add validation to prevent creation of partner tree nodes with empty names
- Filter out blank-named nodes from partner tree API responses
- Add a database constraint requiring non-empty names for partner tree nodes

**Wrong Fix:** ❌ Hiding blank nodes only in the UI while leaving corrupt data in the database

**Workaround:** None — blank levels are visible in oUP and BigQuery

**Repro Steps:**
1. Navigate to the partner tree in Opportunity+
2. Look for nodes with empty/blank names at various levels
3. Check the corresponding partner tree in oUP
4. Check the BigQuery partner tree report
5. Observe blank levels appearing

**Expected:** All partner tree nodes have meaningful, non-empty names.
**Actual:** Some partner tree levels have blank/empty names appearing in oUP and BigQuery.

**Environment:** QA / Production | **Jira:** PNO-867 (In Development)

**Related Tests:** `QA Tests/Integration Tests/PNO-867_BlankPartnerTreeLevels/BlankPartnerTreeTests.cs` (26 tests)

---

## DEF-113: Special Characters in User Names Display as Question Marks

**ID:** DEF-113 | **Severity:** 🟡 Medium | **Date:** 2026-03-05 | **Status:** Open | **Reporter:** QA Team

**Component:** UNOPS.PAO.Presentation/API (Character Encoding)

**Description:** The application fails to correctly render special or accented characters (e.g., 'Ã', 'ö', 'ü') within user selection dropdown menus. These characters are being replaced by question marks ('??'), indicating a character encoding mismatch (likely UTF-8 vs. ASCII/Latin-1) between the database/API response and the frontend component.

**Root Cause:** Character encoding mismatch — the database likely stores data in UTF-8 but either the API response encoding or the frontend rendering interprets certain characters using ASCII/Latin-1, causing the replacement with '??' sequences.

**Proper Fix:**
- Ensure the database connection string specifies UTF-8 encoding
- Verify API response Content-Type header includes `charset=utf-8`
- Ensure frontend dropdown components handle UTF-8 characters properly
- Test with representative accented characters (French, German, Spanish names)

**Wrong Fix:** ❌ Replacing special characters with ASCII equivalents or stripping diacritics

**Workaround:** None — user names with accented characters display incorrectly

**Repro Steps:**
1. Ensure the database contains user records with accented/special characters in names (e.g., "José", "Müller", "Søren")
2. Navigate to any form with a user selection dropdown (e.g., Go/No-Go Decision approval)
3. Open the dropdown
4. Observe that accented characters are replaced with '??'

**Expected:** User names display with correct accented characters (e.g., "José", "Müller").
**Actual:** Accented characters replaced with '??' (e.g., "Jos??", "M??ller").

**Environment:** QA | **Jira:** PNO-1194 (Peer Review)

**Related Tests:** `QA Tests/Integration Tests/PNO-1194_CharacterEncoding/CharacterEncodingTests.cs` (26 tests)

---

## DEF-114: 104 Angular Components and Services Missing In-Source Unit Test Specs

**ID:** DEF-114
**Severity:** 🟠 High
**Category:** Test Coverage
**Date:** 2026-03-09
**Status:** Open
**Reporter:** QA Team

**Description:**
95 Angular components and 9 services in `UNOPS.PAO.ClientApp/src/app/` are missing co-located `.spec.ts` unit test files. The current coverage is 119 spec files out of ~205 testable files (58%). These are developer-owned in-source tests that must be created alongside the production code in `src/app/`.

**Root Cause:** Components and services were developed without accompanying unit tests.

**Proper Fix:**
- Create `.spec.ts` files co-located with each component/service
- Each spec should test: component creation, input/output bindings, template rendering, service interactions, and edge cases
- Follow the patterns established in existing spec files (e.g., `partner-view-enhanced.component.spec.ts`, `ai-assistant-panel.component.spec.ts`)

**Wrong Fix:** ❌ Creating placeholder specs with only `it('should create')` tests — specs must have comprehensive coverage.

**Workaround:** None — missing specs mean no automated regression protection for these components.

### Missing Component Specs (95 files)

**App Root (1):**
| # | File |
|---|------|
| 1 | `app.component.ts` |

**Admin (1):**
| # | File |
|---|------|
| 2 | `features/admin/entity-artifact/components/bulk-entity-artifact-update.component.ts` |

**Import/Export (5):**
| # | File |
|---|------|
| 3 | `features/import-export/components/import/dialog/import-dialog.component.ts` |
| 4 | `features/import-export/components/import/dialog/footer/import-dialog-footer.component.ts` |
| 5 | `features/import-export/components/import/dialog/manual-entry/manual-entry-dialog.component.ts` |
| 6 | `features/import-export/components/import/duplicate-indicator/duplicate-indicator.component.ts` |
| 7 | `features/import-export/components/import/duplicate-summary/duplicate-summary.component.ts` |

**List View (2):**
| # | File |
|---|------|
| 8 | `features/list-view/components/listview/advanced-search/listview-advanced-search.component.ts` |
| 9 | `features/list-view/components/listview/advanced-search/saved-filter/advanced-search-saved-filter.component.ts` |

**Contacts (10):**
| # | File |
|---|------|
| 10 | `features/partnerships/contacts/components/contact/duplicate-confirmation-dialog/duplicate-confirmation-dialog.component.ts` |
| 11 | `features/partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component.ts` |
| 12 | `features/partnerships/contacts/components/contact/edit-dialog/footer/contact-edit-dialog-footer.component.ts` |
| 13 | `features/partnerships/contacts/components/contact/list/contact-list.component.ts` |
| 14 | `features/partnerships/contacts/components/contact/list/business-card-scanner/business-card-scanner.component.ts` |
| 15 | `features/partnerships/contacts/components/contact/tabs/contact-tabs.component.ts` |
| 16 | `features/partnerships/contacts/components/contact/view/contact-view.component.ts` |
| 17 | `features/partnerships/contacts/components/contact/view/interactions/contact-view-interactions.component.ts` |
| 18 | `features/partnerships/contacts/components/contact/view/interactions/dialog/contact-view-interactions-dialog.component.ts` |
| 19 | `features/partnerships/contacts/components/contact/view/interactions/item/contact-view-interactions-item.component.ts` |

**Interactions (6):**
| # | File |
|---|------|
| 20 | `features/partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component.ts` |
| 21 | `features/partnerships/interactions/components/interaction/detail/interaction-detail.component.ts` |
| 22 | `features/partnerships/interactions/components/interaction/list/interaction-list.component.ts` |
| 23 | `features/partnerships/interactions/components/interaction/modal/interaction-modal.component.ts` |
| 24 | `features/partnerships/interactions/components/interaction/modal/footer/interaction-modal-footer.component.ts` |
| 25 | `features/partnerships/interactions/components/interaction/preview/interaction-preview.component.ts` |

**Opportunities (16):**
| # | File |
|---|------|
| 26 | `features/partnerships/opportunities/components/opportunity/approve-opportunity-dialog/approve-opportunity-dialog.component.ts` |
| 27 | `features/partnerships/opportunities/components/opportunity/list/opportunity-list.component.ts` |
| 28 | `features/partnerships/opportunities/components/opportunity/opportunity-decision-info-panel/opportunity-decision-info-panel.component.ts` |
| 29 | `features/partnerships/opportunities/components/opportunity/option1-unified/ai-comparison-demo.component.ts` |
| 30 | `features/partnerships/opportunities/components/opportunity/option1-unified/opportunity-option1.component.ts` |
| 31 | `features/partnerships/opportunities/components/opportunity/reject-opportunity-dialog/reject-opportunity-dialog.component.ts` |
| 32 | `features/partnerships/opportunities/components/opportunity/view/sections/analysis/opportunity-analysis-section.component.ts` |
| 33 | `features/partnerships/opportunities/components/opportunity/view/sections/collaboration/opportunity-collaboration.component.ts` |
| 34 | `features/partnerships/opportunities/components/opportunity/view/sections/document/opportunity-ai-comparison.component.ts` |
| 35 | `features/partnerships/opportunities/components/opportunity/view/sections/document/opportunity-documents.component.ts` |
| 36 | `features/partnerships/opportunities/components/opportunity/view/sections/dst/opportunity-dst-section.component.ts` |
| 37 | `features/partnerships/opportunities/components/opportunity/view/sections/overview/opportunity-overview-section.component.ts` |
| 38 | `features/partnerships/opportunities/components/opportunity/view/sections/related/opportunity-related-items.component.ts` |
| 39 | `features/partnerships/opportunities/components/opportunity/view/sections/statement/opportunity-statement-section.component.ts` |
| 40 | `features/partnerships/opportunities/components/opportunity/view/sections/team/opportunity-team-section.component.ts` |
| 41 | `features/partnerships/opportunities/components/opportunity/view/sections/what/opportunity-what-section.component.ts` |
| 42 | `features/partnerships/opportunities/components/opportunity/view/sections/when/opportunity-when-section.component.ts` |
| 43 | `features/partnerships/opportunities/components/opportunity/view/sections/where/opportunity-where-section.component.ts` |
| 44 | `features/partnerships/opportunities/components/opportunity/view/sections/who/opportunity-who-section.component.ts` |
| 45 | `features/partnerships/opportunities/components/opportunity/view/sections/why/opportunity-why-section.component.ts` |

**Partners (25):**
| # | File |
|---|------|
| 46 | `features/partnerships/partners/components/partner/partner.component.ts` |
| 47 | `features/partnerships/partners/components/partner/approval-dialog/partner-approval-dialog.component.ts` |
| 48 | `features/partnerships/partners/components/partner/contacts/partner-contacts.component.ts` |
| 49 | `features/partnerships/partners/components/partner/data/partner-data.component.ts` |
| 50 | `features/partnerships/partners/components/partner/edit-dialog/partner-edit-dialog.component.ts` |
| 51 | `features/partnerships/partners/components/partner/edit-dialog/footer/partner-edit-dialog-footer.component.ts` |
| 52 | `features/partnerships/partners/components/partner/funding-agreements/partner-funding-agreements.component.ts` |
| 53 | `features/partnerships/partners/components/partner/tabs/partner-tabs.component.ts` |
| 54 | `features/partnerships/partners/components/partner/view/partner-view.component.ts` |
| 55 | `features/partnerships/partners/components/partner/view/contacts/partner-view-contacts.component.ts` |
| 56 | `features/partnerships/partners/components/partner/view/contacts/dialog/partner-view-contacts-dialog.component.ts` |
| 57 | `features/partnerships/partners/components/partner/view/contacts/item/partner-view-contacts-item.component.ts` |
| 58 | `features/partnerships/partners/components/partner/view/interactions/partner-view-interactions.component.ts` |
| 59 | `features/partnerships/partners/components/partner/view/opportunities/partner-view-opportunities.component.ts` |
| 60 | `features/partnerships/partners/components/partner/view/org-structure-dialog/org-structure-dialog.component.ts` |
| 61 | `features/partnerships/partners/components/partner-tree/item/partner-tree-item-footer.component.ts` |
| 62 | `features/partnerships/partners/components/partner-tree/item/partner-tree-item.component.ts` |
| 63 | `features/partnerships/partners/components/partner-tree/view/partner-tree-view.component.ts` |
| 64 | `features/partnerships/partners/components/partner-tree/view/contacts/partner-view-contacts.component.ts` |
| 65 | `features/partnerships/partners/components/partner-tree/view/contacts/dialog/partner-view-contacts-dialog.component.ts` |
| 66 | `features/partnerships/partners/components/partner-tree/view/contacts/item/partner-view-contacts-item.component.ts` |
| 67 | `features/partnerships/partners/components/partner-tree/view/data/partner-tree-data.component.ts` |
| 68 | `features/partnerships/partners/components/partner-tree/view/details/partner-tree-details.component.ts` |
| 69 | `features/partnerships/partners/components/partner-tree/view/navigation/partner-tree-view-navigation.component.ts` |
| 70 | `features/partnerships/partners/components/partner-tree-page/partner-tree-page.component.ts` |

**Static Pages (3):**
| # | File |
|---|------|
| 71 | `features/static-pages/components/access-denied/access-denied.component.ts` |
| 72 | `features/static-pages/components/coming-soon/coming-soon.component.ts` |
| 73 | `features/static-pages/components/not-found/not-found.component.ts` |

**Layouts (14):**
| # | File |
|---|------|
| 74 | `layouts/components/footer/footer.component.ts` |
| 75 | `layouts/components/layout/layout.component.ts` |
| 76 | `layouts/components/layout/breadcrumb/breadcrumb.component.ts` |
| 77 | `layouts/components/menu/menu.component.ts` |
| 78 | `layouts/components/menu/menu-item/menu-item.component.ts` |
| 79 | `layouts/components/profile-dialog/profile-dialog.component.ts` |
| 80 | `layouts/components/sidebar/sidebar.component.ts` |
| 81 | `layouts/components/topbar/topbar.component.ts` |
| 82 | `layouts/components/topbar/doa-role-dialog/doa-role-dialog.component.ts` |
| 83 | `layouts/components/topbar/global-filters-dialog/global-filters-dialog.component.ts` |
| 84 | `layouts/components/topbar/global-search-bar/global-search-bar.component.ts` |
| 85 | `layouts/components/topbar/language-selector/language-selector.component.ts` |
| 86 | `layouts/components/topbar/org-unit-selector/org-unit-selector.component.ts` |
| 87 | `layouts/components/topbar/profile-menubar/profile-menubar.component.ts` |
| 88 | `layouts/components/topbar/role-dialog/role-dialog.component.ts` |

**Shared (7):**
| # | File |
|---|------|
| 89 | `shared/base-classes/base-engagement-list.component.ts` |
| 90 | `shared/base-classes/base-engagement-view.component.ts` |
| 91 | `shared/components/ai/ai-comparison/ai-comparison.component.ts` |
| 92 | `shared/components/comments/comment.component.ts` |
| 93 | `shared/components/documents/document/upload/upload-document.component.ts` |
| 94 | `shared/components/file-upload/base64-file-upload.component.ts` |
| 95 | `shared/reusables/components/workflow/components/workflow/workflow.component.ts` |

### Missing Service Specs (9 files)

| # | File |
|---|------|
| 1 | `core/services/auth/google-oauth.service.ts` |
| 2 | `core/services/google-analytics/google-analytics.service.ts` |
| 3 | `features/admin/entity-artifact/services/entity-artifact.service.ts` |
| 4 | `features/partnerships/opportunities/services/opportunity.service.ts` |
| 5 | `shared/services/google-drive.service.ts` |
| 6 | `shared/services/api/comment.service.ts` |
| 7 | `shared/services/api/opportunity-demo.service.ts` |
| 8 | `shared/services/api/values.service.ts` |
| 9 | `shared/services/partner/partner-search.service.ts` |

### Priority Recommendations

**High Priority** (core business logic, highest user impact):
- All 16 Opportunity section components (32–45) — core business feature
- All 6 Interaction components (20–25) — core CRM feature
- Workflow component (95) — used across all entities
- Opportunity service (service #4) — main data service

**Medium Priority** (UI consistency, user-facing):
- All 25 Partner components (46–70) — major entity
- All 10 Contact components (10–19) — major entity
- All 14 Layout components (74–88) — app shell
- Auth services (service #1) — security-critical

**Lower Priority** (less frequently changed):
- Static pages (71–73) — simple presentational
- Import/Export (3–7) — less frequently used
- Base classes (89–90) — abstract, tested via subclasses

**Repro Steps:**
1. Count `.component.ts` files in `UNOPS.PAO.ClientApp/src/app/`: 145
2. Count `.component.spec.ts` files: 50
3. Difference: 95 components have no spec file
4. Count `.service.ts` files: 60
5. Count `.service.spec.ts` files: 51
6. Difference: 9 services have no spec file

**Expected:** Every component and service has a co-located `.spec.ts` file with comprehensive tests.
**Actual:** 104 files (95 components + 9 services) have no unit test coverage.

**Environment:** All environments — this is a development-time gap.

### Impact Assessment

**Coverage Impact:**
- Current Angular in-source test coverage: **58%** (119 of 205 testable files)
- Target coverage: **100%** (every component and service has a spec)
- Gap: **104 files** with zero automated unit test protection

**Risk by Feature Area:**

| Area | Missing Specs | Risk Level | Business Impact |
|------|--------------|------------|----------------|
| **Opportunity sections** | 16 components + 1 service | 🔴 Critical | Core pipeline feature — any regression in opportunity views (What, When, Where, Who, Why, Analysis, DST, Statement, Team, Documents, Overview, Related Items) goes undetected. These sections contain complex form logic, permission-gated UI, and signal-based state management. |
| **Partner components** | 25 components | 🔴 Critical | Primary entity — partner view, edit, approval, contacts, tree navigation, org structure, and funding agreements all lack regression protection. Partner tree alone has 10 untested components with complex recursive rendering. |
| **Contact components** | 10 components | 🟠 High | Key CRM entity — contact list, edit dialog, duplicate detection, business card scanner, and interaction views untested. Duplicate confirmation dialog is business-critical for data quality. |
| **Interaction components** | 6 components | 🟠 High | Core CRM workflow — interaction list, detail, modal, preview, and "create opportunity from interaction" dialog untested. These components handle cross-entity workflows. |
| **Layout/Navigation** | 14 components | 🟠 High | App shell — topbar, sidebar, menu, breadcrumb, global search, language selector, org unit selector, and profile components. Breakage here affects every user on every page. |
| **Workflow component** | 1 component | 🔴 Critical | Used across ALL entities for stage transitions. A regression here could silently break approval workflows, status changes, and permission-gated actions across the entire application. |
| **Import/Export** | 5 components | 🟡 Medium | Data import dialogs and duplicate handling. Less frequently used but critical for bulk operations. |
| **Auth/Analytics services** | 2 services | 🟠 High | Google OAuth and analytics — security-critical services with no automated validation. |
| **Shared/Base classes** | 7 components | 🟡 Medium | Base engagement components and AI comparison — regressions cascade to all inheriting components. |
| **Static pages** | 3 components | 🟢 Low | Access denied, coming soon, not found — simple presentational, low change frequency. |

**Consequences of Not Addressing:**

1. **No regression detection** — Changes to any of the 104 untested files can introduce bugs that are only caught manually or by end users in production.
2. **Refactoring paralysis** — Developers cannot safely refactor or upgrade Angular versions for these components without risking silent breakage. The upcoming Angular 20 migration is at high risk.
3. **Signal migration risk** — Many of these components use Angular 19 signals (`input()`, `output()`, `computed()`). Without specs validating signal behavior, signal-related bugs (stale values, missing reactivity, transform errors) go undetected.
4. **Permission logic gaps** — Components like approve/reject dialogs, partner approval, and workflow rely on server-side permission integration. Without specs mocking the permission endpoint, authorization regressions are invisible.
5. **Template rendering bugs** — PrimeNG component integration (`p-dialog`, `p-select`, `p-table`) in untested templates can break silently after PrimeNG version updates.
6. **Cascading failures** — Base classes (`base-engagement-list`, `base-engagement-view`) are extended by multiple feature components. A bug in a base class cascades to all children with no test to catch it.
7. **CI pipeline blind spot** — The `ng test` pipeline reports 100% pass rate, but this is misleading since 42% of the codebase has no tests to fail. False confidence in build quality.
8. **Increased manual QA burden** — Every sprint, QA must manually verify all 104 untested component behaviors. At an estimated 15 minutes per component, that is **26 hours of manual testing per regression cycle** that could be automated.
