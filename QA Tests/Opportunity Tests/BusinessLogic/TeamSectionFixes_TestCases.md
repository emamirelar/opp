# Team Section Fixes — Comprehensive Test Cases

**Component:** Opportunity Team Section — Decision Pathway, Collaborators, Stakeholders, Deduplication  
**Frontend:** `opportunity-team-section.component.ts/html`  
**Backend:** `UNOPSOpportunityManager.cs` — Collaborator CRUD, Expertise Assignment  
**API:** `PATCH /api/opportunity/{id}/team`, `GET /api/opportunity/collaborator-expertises`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30 = 90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30 = 90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30 = 90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30 = 90 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **462** | ✅ |

### Four Individual Ratio Checks

| Check | Formula | Actual | Required | Status |
|-------|---------|--------|---------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Feature Overview

### Team Section Components

| Sub-feature | Description |
|-------------|-------------|
| **Decision Making Pathway** | Displays DoA Level 2 and DoA Level 3 stakeholders grouped by org unit |
| **Collaborators** | Users assigned to the Opportunity Development Team with expertise areas |
| **Stakeholders** | Auto-populated and manually added stakeholders with roles and org units |
| **OM (Opportunity Manager)** | Primary owner; previous OM auto-added as collaborator on transfer |
| **Org Unit / Initiative Type** | Organizational assignment with responsible/normally responsible distinction |

### Key Fixes Applied

1. **Decision pathway deduplication** — Filters duplicate org units by `orgUnitId + roleId`
2. **Collaborator expertise merge** — Existing collaborator re-added merges expertise instead of creating duplicate
3. **Stakeholder deduplication** — Prefers entries with more complete data (userName, position)
4. **OM transfer** — Previous OM auto-added as collaborator if not already present

### Data Model

```
OpportunityCollaborator: OpportunityId, UserId, Name, AddedDate, AddedBy
OpportunityCollaboratorExpertise: CollaboratorId, ExpertiseTypeId
OpportunityStakeholder: OpportunityId, UserId, RoleId, OrgUnitId
```

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Collaborator Management (POS-001–012)

POS-001: Add collaborator with valid user → Created with AddedDate and AddedBy.  
POS-002: Add collaborator with expertise areas → Expertise records created.  
POS-003: Add multiple collaborators → All created, unique per user.  
POS-004: Remove collaborator → Soft-deleted from opportunity.  
POS-005: Edit collaborator expertise → Expertise updated.  
POS-006: Collaborator list displays user name, email, expertise.  
POS-007: Available collaborator users excludes already-added users.  
POS-008: Collaborator add dialog opens correctly.  
POS-009: Collaborator edit dialog pre-fills current expertise.  
POS-010: Collaborator remove shows confirmation dialog.  
POS-011: Collaborator expertise dropdown loads all expertise types.  
POS-012: Collaborator with no expertise → Created with empty expertise list.

### Expertise Merge (POS-013–018)

POS-013: Re-add existing collaborator with NEW expertise → Merges expertise (no duplicate user).  
POS-014: Re-add existing collaborator with SAME expertise → No change (idempotent).  
POS-015: Re-add existing collaborator with mixed expertise → Union of old + new.  
POS-016: Merged expertise visible in collaborator detail.  
POS-017: Merged expertise count correct after merge.  
POS-018: Expertise merge does not duplicate the collaborator row.

### Decision Pathway (POS-019–025)

POS-019: Decision pathway displays DoA2 and DoA3 stakeholders.  
POS-020: Decision pathway groups stakeholders by org unit.  
POS-021: Decision pathway shows `isNormallyResponsible` distinction.  
POS-022: Decision pathway shows `countryName` for each org unit.  
POS-023: `responsibleOrgUnitDecisionMakingPathway` computed correctly.  
POS-024: `normallyResponsibleDecisionMakingPathway` computed correctly.  
POS-025: Decision pathway updates when org unit changes.

### Stakeholder Deduplication (POS-026–030)

POS-026: Auto-populated stakeholders deduplicated by `orgUnitId + roleId`.  
POS-027: Deduplication prefers entry with userName populated.  
POS-028: Deduplication prefers entry with position populated.  
POS-029: Manually added stakeholder not affected by auto-population dedup.  
POS-030: Stakeholder list shows only unique entries.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Collaborator Validation Failures (NEG-001–020)

NEG-001: Add collaborator without selecting user → Validation error.  
NEG-002: Add collaborator with non-existent user ID → Error.  
NEG-003: Add collaborator with soft-deleted user → Error or filtered out.  
NEG-004: Add collaborator to non-existent opportunity → 404.  
NEG-005: Add collaborator to soft-deleted opportunity → 404.  
NEG-006: Add collaborator without permission → 403.  
NEG-007: Add collaborator when opportunity is immutable (NO GO) → Blocked.  
NEG-008: Add collaborator when opportunity is in approval workflow → Blocked.  
NEG-009: Add duplicate collaborator (same user, same opportunity, both active) → Unique constraint violation.  
NEG-010: Add collaborator with invalid expertise ID → Error.  
NEG-011: Add collaborator with expertise ID=0 → Validation error.  
NEG-012: Add collaborator with negative expertise ID → Validation error.  
NEG-013: Remove collaborator that doesn't exist → 404.  
NEG-014: Remove collaborator from wrong opportunity → Not found.  
NEG-015: Remove collaborator without permission → 403.  
NEG-016: Edit collaborator expertise without permission → 403.  
NEG-017: Add collaborator via API with malformed JSON → 400.  
NEG-018: Add collaborator via API with missing required fields → 400.  
NEG-019: Add collaborator via API with empty body → 400.  
NEG-020: Add collaborator via API unauthenticated → 401.

### Stakeholder Validation Failures (NEG-021–035)

NEG-021: Add stakeholder without selecting user → Validation error.  
NEG-022: Add stakeholder without selecting role → Validation error.  
NEG-023: Add duplicate stakeholder (same user + role) → Blocked by dedup check.  
NEG-024: Add stakeholder to immutable opportunity → Blocked.  
NEG-025: Add stakeholder to opportunity in workflow → Blocked.  
NEG-026: Add stakeholder with non-existent user → Error.  
NEG-027: Add stakeholder with non-existent role → Error.  
NEG-028: Add stakeholder without permission → 403.  
NEG-029: Remove stakeholder without permission → 403.  
NEG-030: Auto-populate stakeholders when no org unit assigned → Empty or error.  
NEG-031: Auto-populate stakeholders when org unit has no DoA holders → Empty pathway.  
NEG-032: Auto-populate stakeholders from wrong org unit → Incorrect data.  
NEG-033: Stakeholder with disabled user account → Should be flagged or excluded.  
NEG-034: Stakeholder with null userName → Dedup still works (falls back).  
NEG-035: Stakeholder with null position → Dedup still works (lower priority).

### Decision Pathway Failures (NEG-036–050)

NEG-036: Decision pathway with no DoA2 holders → Empty DoA2 section.  
NEG-037: Decision pathway with no DoA3 holders → Empty DoA3 section.  
NEG-038: Decision pathway with no org unit assigned → Empty pathway.  
NEG-039: Decision pathway with soft-deleted DoA holders → Excluded.  
NEG-040: Decision pathway with disabled DoA holders → Excluded or flagged.  
NEG-041: Decision pathway with duplicate org units → Deduplicated.  
NEG-042: Decision pathway with null countryName → Handled (no crash).  
NEG-043: Decision pathway with null orgUnitName → Handled.  
NEG-044: Decision pathway after org unit change → Refreshes correctly.  
NEG-045: Decision pathway with circular org unit references → No infinite loop.  
NEG-046: Decision pathway computed signal with empty input → Returns empty array.  
NEG-047: Decision pathway computed signal with null input → Returns empty array.  
NEG-048: Responsible org unit not found → Fallback handled.  
NEG-049: Normally responsible org unit not found → Fallback handled.  
NEG-050: Decision pathway with 0 stakeholders → Displays empty state message.

### OM Transfer Failures (NEG-051–060)

NEG-051: Change OM to non-existent user → Error.  
NEG-052: Change OM to soft-deleted user → Error.  
NEG-053: Change OM to same user (no change) → No-op.  
NEG-054: Change OM without permission → 403.  
NEG-055: Change OM on immutable opportunity → Blocked.  
NEG-056: Change OM on opportunity in workflow → Blocked.  
NEG-057: Change OM when previous OM is disabled → Still added as collaborator.  
NEG-058: Change OM rapidly twice → Both transitions handled.  
NEG-059: Change OM when collaborator limit reached → Error or warning.  
NEG-060: Change OM API with missing new OM ID → 400.

### Expertise Merge Failures (NEG-061–070)

NEG-061: Merge expertise with null expertise list → No change to existing.  
NEG-062: Merge expertise with empty expertise list → No change.  
NEG-063: Merge expertise with invalid expertise IDs → Error for invalid IDs.  
NEG-064: Merge expertise when collaborator was soft-deleted → New record or error.  
NEG-065: Merge expertise without permission → 403.  
NEG-066: Merge expertise on immutable opportunity → Blocked.  
NEG-067: Concurrent expertise merge for same collaborator → One wins.  
NEG-068: Expertise merge with database error → Transaction rolled back.  
NEG-069: Expertise merge with very large expertise list → Handled.  
NEG-070: Expertise type that no longer exists → FK violation or filtered.

### Team Section Extended Failures (NEG-071–090)

NEG-071: PATCH team with opportunityId as string "abc" → 400 type error.  
NEG-072: PATCH team with collaborator list containing null userId → 400.  
NEG-073: PATCH team with expertise array containing non-integer → 400.  
NEG-074: GET collaborator-expertises unauthenticated → 401.  
NEG-075: Add collaborator when user has no org unit assignment → Handled per business rules.  
NEG-076: Stakeholder with soft-deleted role → Excluded or error.  
NEG-077: Stakeholder with soft-deleted org unit → Excluded or error.  
NEG-078: Decision pathway with org unit having no country → Handled (no crash).  
NEG-079: Re-add collaborator with soft-deleted expertise type → Invalid IDs filtered.  
NEG-080: Team section load when opportunity has no team data → Empty state.  
NEG-081: Edit collaborator when collaborator was removed by another user → 404 or conflict.  
NEG-082: Add stakeholder with user from different tenant (multi-tenant) → Blocked.  
NEG-083: PATCH team with negative collaborator count → Validation error.  
NEG-084: Decision pathway with malformed org unit hierarchy → No infinite recursion.  
NEG-085: Collaborator expertise GET when no expertise types configured → Empty array.  
NEG-086: OM change when new OM has no access to opportunity → Blocked.  
NEG-087: Stakeholder dedup with conflicting userName/position across entries → Deterministic result.  
NEG-088: Team section save with stale ETag/version → Optimistic concurrency error.  
NEG-089: Add collaborator with user who is already OM → Handled (no self-duplicate).  
NEG-090: PATCH team with Content-Type other than application/json → 415.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Collaborator Count Boundaries (BND-001–015)

BND-001: 0 collaborators on opportunity → Empty team section.  
BND-002: 1 collaborator → Displayed correctly.  
BND-003: 10 collaborators → All displayed.  
BND-004: 50 collaborators → Scrollable list or pagination.  
BND-005: 100 collaborators → Performance acceptable.  
BND-006: Collaborator with 0 expertise areas → Valid, no expertise shown.  
BND-007: Collaborator with 1 expertise area → Displayed.  
BND-008: Collaborator with 10 expertise areas → All displayed.  
BND-009: Collaborator with all available expertise areas → All displayed.  
BND-010: Add then remove then re-add same collaborator → Works (filtered index allows).  
BND-011: 5 cycles of add/remove same collaborator → All soft-deleted + 1 active.  
BND-012: Collaborator name at 1 character → Displayed.  
BND-013: Collaborator name at max length → Displayed, truncated if needed.  
BND-014: Collaborator with Unicode name → Displayed correctly.  
BND-015: Collaborator with very long email → Displayed, truncated if needed.

### Stakeholder Count Boundaries (BND-016–030)

BND-016: 0 stakeholders → Empty section.  
BND-017: 1 stakeholder → Displayed.  
BND-018: 20 stakeholders → All displayed.  
BND-019: 100 stakeholders → Scrollable.  
BND-020: Auto-populated stakeholders: 0 → Empty auto-populated section.  
BND-021: Auto-populated stakeholders: 1 → Displayed.  
BND-022: Auto-populated stakeholders: 50 → All displayed.  
BND-023: Duplicate auto-populated stakeholders before dedup: 10 → After dedup: fewer.  
BND-024: All auto-populated stakeholders identical → Deduped to 1.  
BND-025: All auto-populated stakeholders unique → None removed.  
BND-026: Stakeholder with orgUnitId=1 and roleId=1 → Valid composite key.  
BND-027: Stakeholder with orgUnitId=MAX_INT and roleId=MAX_INT → Valid.  
BND-028: Stakeholder with null userName → Lower dedup priority.  
BND-029: Stakeholder with null position → Lower dedup priority.  
BND-030: Stakeholder with both userName and position → Higher dedup priority.

### Decision Pathway Boundaries (BND-031–045)

BND-031: Pathway with 0 org units → Empty pathway.  
BND-032: Pathway with 1 org unit, 1 DoA2 → Single entry.  
BND-033: Pathway with 1 org unit, multiple DoA2 holders → Multiple entries for same unit.  
BND-034: Pathway with 5 org units → 5 groups.  
BND-035: Pathway with 20 org units → Performance acceptable.  
BND-036: Pathway with DoA2 but no DoA3 → DoA2 section only.  
BND-037: Pathway with DoA3 but no DoA2 → DoA3 section (fallback scenario).  
BND-038: Pathway with both DoA2 and DoA3 → Both sections.  
BND-039: Pathway after org unit change → Refreshed immediately.  
BND-040: Pathway with duplicate org unit IDs → Deduplicated.  
BND-041: Normally responsible org units: 0 → No normally responsible section.  
BND-042: Normally responsible org units: 1 → Single entry.  
BND-043: Normally responsible org units: 10 → All displayed.  
BND-044: Responsible org unit same as normally responsible → Both shown.  
BND-045: Responsible org unit different from normally responsible → Both shown separately.

### Expertise Merge Boundaries (BND-046–060)

BND-046: Merge 0 new expertise with 0 existing → No change.  
BND-047: Merge 1 new expertise with 0 existing → 1 expertise after merge.  
BND-048: Merge 0 new expertise with 5 existing → 5 expertise after merge (unchanged).  
BND-049: Merge 5 new with 5 existing (all different) → 10 after merge.  
BND-050: Merge 5 new with 5 existing (all same) → 5 after merge (no duplicates).  
BND-051: Merge 5 new with 5 existing (3 overlap) → 7 after merge.  
BND-052: `[...new Set([...existing, ...new])]` dedup → Correct union.  
BND-053: Expertise IDs at min value (1) → Merged correctly.  
BND-054: Expertise IDs at max value → Merged correctly.  
BND-055: Expertise IDs as strings → Type handling correct.  
BND-056: Expertise IDs as numbers → Type handling correct.  
BND-057: Expertise IDs with nulls in array → Filtered out.  
BND-058: Expertise IDs with duplicates in new array → Deduped.  
BND-059: Expertise IDs with duplicates in existing array → Deduped.  
BND-060: Very large expertise list (100 items) → Handled.

### Team API Boundaries (BND-061–070)

BND-061: `PATCH /api/opportunity/{id}/team` with empty body → No change.  
BND-062: `PATCH /api/opportunity/{id}/team` with all fields → All updated.  
BND-063: `PATCH /api/opportunity/{id}/team` with partial fields → Only specified updated.  
BND-064: Team update with opportunityId=0 → Validation error.  
BND-065: Team update with negative opportunityId → Validation error.  
BND-066: Team update with non-existent opportunityId → 404.  
BND-067: Collaborator expertise GET endpoint → Returns all expertise types.  
BND-068: Collaborator expertise GET endpoint → Types sorted alphabetically.  
BND-069: Team section reload → Data consistent with API.  
BND-070: Team section save + immediate reload → Saved data persists.

### Extended Boundary Cases (BND-071–090)

BND-071: OpportunityId at INT_MAX → Handled or validation error.  
BND-072: UserId at INT_MAX for collaborator → Handled.  
BND-073: AddedDate at epoch (1970-01-01) → Displayed correctly.  
BND-074: AddedDate at far future → Handled.  
BND-075: Org unit name empty string → Handled in pathway display.  
BND-076: Country name with special chars (é, ñ, 中文) → Displayed correctly.  
BND-077: Position field at max DB length → Truncated or displayed.  
BND-078: Expertise type name at 1 character → Displayed.  
BND-079: Role name with leading/trailing spaces → Trimmed or displayed.  
BND-080: Collaborator list with exactly 99 items → Add 100th succeeds.  
BND-081: Stakeholder list with exactly 99 items → Add 100th succeeds.  
BND-082: Decision pathway with 1 org unit, 0 DoA holders → Empty pathway.  
BND-083: Merge expertise: new list order different from existing → Union order deterministic.  
BND-084: Dedup with three entries: A (userName), B (position), C (neither) → A preferred.  
BND-085: Dedup with A and B both having userName → First or deterministic.  
BND-086: PATCH with collaborators array length 0 → Clears collaborators or no-op.  
BND-087: PATCH with expertise array length 0 → Clears expertise.  
BND-088: Org unit ID = 0 → Validation error or excluded.  
BND-089: Role ID = 0 → Validation error or excluded.  
BND-090: Timezone boundary: AddedDate in UTC vs local → Consistent display.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Collaborator CRUD (FUN-001–012)

FUN-001: Add collaborator → `OpportunityCollaborator` row created with IsDeleted=false.  
FUN-002: Add collaborator → `AddedDate` set to current time.  
FUN-003: Add collaborator → `AddedBy` set to current user ID.  
FUN-004: Remove collaborator → `IsDeleted` set to true, `DeletedBy`/`DeletedDate` set.  
FUN-005: Remove collaborator → Row not physically deleted.  
FUN-006: Available users list excludes current collaborators.  
FUN-007: Available users list includes all active users.  
FUN-008: Add collaborator with expertise → `OpportunityCollaboratorExpertise` rows created.  
FUN-009: Remove collaborator → Expertise records also soft-deleted.  
FUN-010: Edit collaborator expertise → Old expertise removed, new added.  
FUN-011: Collaborator list sorted by name.  
FUN-012: Collaborator list shows expertise badges/tags.

### Deduplication Rules (FUN-013–025)

FUN-013: `confirmCollaboratorDialog()` checks for existing collaborator before adding.  
FUN-014: Existing collaborator detected → Expertise merged, no new row.  
FUN-015: New collaborator → New row created with expertise.  
FUN-016: Merged expertise uses `[...new Set()]` for deduplication.  
FUN-017: Stakeholder dedup by `orgUnitId + roleId` composite key.  
FUN-018: Stakeholder dedup prefers entry with `userName` populated.  
FUN-019: Stakeholder dedup prefers entry with `position` populated.  
FUN-020: Stakeholder dedup between two entries with equal data → First kept.  
FUN-021: Org unit dedup: `normallyResponsibleOrgUnits` uses `find()` to prevent duplicates.  
FUN-022: Collaborator confirm dialog shows merge message when existing found.  
FUN-023: Collaborator confirm dialog shows add message when new user.  
FUN-024: Stakeholder confirm dialog checks for duplicate user-role combination.  
FUN-025: Deduplication runs on every computed signal update.

### Decision Pathway Logic (FUN-026–038)

FUN-026: `decisionMakingPathwayStakeholders` computed → Filters DoA2 and DoA3.  
FUN-027: Pathway groups by org unit correctly.  
FUN-028: Pathway shows `isNormallyResponsible` flag per group.  
FUN-029: Pathway shows `countryName` per org unit.  
FUN-030: `responsibleOrgUnitDecisionMakingPathway` → Filters by responsible.  
FUN-031: `normallyResponsibleDecisionMakingPathway` → Filters by normally responsible.  
FUN-032: Pathway reactive to org unit changes.  
FUN-033: Pathway reactive to stakeholder changes.  
FUN-034: Pathway template iterates groups correctly.  
FUN-035: Pathway header shows "Opportunity Decision Making Pathway (DoA2 and DoA3)".  
FUN-036: Pathway empty state shown when no DoA holders.  
FUN-037: Pathway displays DoA level label per stakeholder.  
FUN-038: Pathway displays user name and position per stakeholder.

### OM Transfer Logic (FUN-039–050)

FUN-039: OM change triggers `addPreviousOMAsCollaborator` logic.  
FUN-040: Previous OM checked against existing collaborators.  
FUN-041: Previous OM not in collaborators → Added as new collaborator.  
FUN-042: Previous OM already in collaborators → Not duplicated.  
FUN-043: Previous OM added with `AddedBy` = user who changed OM.  
FUN-044: Previous OM added with `AddedDate` = current time.  
FUN-045: Previous OM collaborator has default expertise (none).  
FUN-046: New OM set on opportunity → `OpportunityManager` field updated.  
FUN-047: Team section refreshes after OM change.  
FUN-048: `isCollaborator` check uses `context.OpportunityCollaborators`.  
FUN-049: OM change audit trail created.  
FUN-050: OM change notification sent (if applicable).

### Extended Functional Logic (FUN-051–090)

FUN-051: Soft-deleted collaborators excluded from `availableCollaboratorUsers`.  
FUN-052: Soft-deleted stakeholders excluded from pathway computation.  
FUN-053: `IsDeleted` filter applied in all collaborator queries.  
FUN-054: `IsDeleted` filter applied in all stakeholder queries.  
FUN-055: Collaborator expertise FK to ExpertiseType validated before insert.  
FUN-056: Stakeholder RoleId FK validated before insert.  
FUN-057: Stakeholder OrgUnitId FK validated before insert.  
FUN-058: OpportunityId FK validated on all team operations.  
FUN-059: UserId FK validated for collaborator add.  
FUN-060: UserId FK validated for stakeholder add.  
FUN-061: Pathway computed signal memoized (no redundant recompute).  
FUN-062: Dedup computed signal memoized.  
FUN-063: Available users computed excludes soft-deleted users.  
FUN-064: Pathway groups sorted by org unit name.  
FUN-065: Collaborator list excludes users with IsDeleted=true.  
FUN-066: Stakeholder list excludes entries with IsDeleted=true.  
FUN-067: Expertise merge preserves order (existing first, then new).  
FUN-068: Expertise merge does not affect other collaborators.  
FUN-069: Remove collaborator does not affect stakeholders.  
FUN-070: Remove stakeholder does not affect collaborators.  
FUN-071: Auto-populate stakeholders uses correct org unit from opportunity.  
FUN-072: Auto-populate stakeholders filters by DoA level (2 and 3).  
FUN-073: Manual stakeholder add does not trigger auto-populate overwrite.  
FUN-074: Org unit change triggers stakeholder re-population.  
FUN-075: Pathway uses `isNormallyResponsible` from org unit config.  
FUN-076: Pathway uses `countryName` from org unit or related entity.  
FUN-077: Collaborator add validates user is active.  
FUN-078: Collaborator add validates user exists.  
FUN-079: Stakeholder add validates role exists and not deleted.  
FUN-080: Stakeholder add validates org unit exists and not deleted.  
FUN-081: Team PATCH validates all collaborator IDs exist.  
FUN-082: Team PATCH validates all expertise IDs exist.  
FUN-083: OM change validates new OM user exists.  
FUN-084: OM change validates new OM user is active.  
FUN-085: Dedup tie-breaker: userName > position > first.  
FUN-086: Pathway empty when org unit has no DoA config.  
FUN-087: Pathway fallback: DoA3 when no DoA2.  
FUN-088: Collaborator expertise GET returns only active expertise types.  
FUN-089: Team section load fetches collaborators with expertise.  
FUN-090: Team section load fetches stakeholders with role and org unit.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Collaborator End-to-End (INT-001–015)

INT-001: Add collaborator via UI → Verify in DB → Verify in API response.  
INT-002: Remove collaborator via UI → Verify soft-delete in DB.  
INT-003: Re-add collaborator via UI → Verify new row + merge.  
INT-004: Edit expertise via UI → Verify update in DB.  
INT-005: Add collaborator with expertise → Verify expertise in API response.  
INT-006: Add 5 collaborators → List shows all 5.  
INT-007: Remove 2 of 5 → List shows 3.  
INT-008: PATCH team API → Collaborators updated correctly.  
INT-009: GET collaborator-expertises → Returns all expertise types.  
INT-010: Collaborator with soft-deleted user → Not shown in available users.  
INT-011: Collaborator with disabled user → Behavior per business rules.  
INT-012: Collaborator across page refresh → Data persists.  
INT-013: Collaborator across browser sessions → Data persists.  
INT-014: Collaborator on immutable opportunity → Add/remove blocked.  
INT-015: Collaborator on opportunity in workflow → Add/remove blocked.

### Stakeholder End-to-End (INT-016–025)

INT-016: Auto-populate stakeholders from org unit → Correct DoA holders.  
INT-017: Change org unit → Stakeholders re-populated.  
INT-018: Add manual stakeholder → Persisted in DB.  
INT-019: Remove manual stakeholder → Soft-deleted.  
INT-020: Duplicate stakeholder check → Blocked with message.  
INT-021: Auto-populated + manual stakeholders → Both displayed.  
INT-022: Stakeholder list on opportunity detail → Correct data.  
INT-023: Stakeholder list in API response → Correct data.  
INT-024: Stakeholder dedup across re-population → No duplicates.  
INT-025: Stakeholder count in submission requirements → Correct.

### Decision Pathway End-to-End (INT-026–035)

INT-026: Opportunity with org unit → Decision pathway shows DoA2/DoA3.  
INT-027: Opportunity without org unit → Decision pathway empty.  
INT-028: Change org unit → Decision pathway updates.  
INT-029: DoA2 user disabled → Removed from pathway.  
INT-030: DoA3 fallback (no DoA2) → DoA3 shown as decision maker.  
INT-031: Multiple org units → Pathway groups correctly.  
INT-032: Pathway on opportunity detail page → Correct layout.  
INT-033: Pathway on opportunity print/export → Included.  
INT-034: Pathway data matches API response.  
INT-035: Pathway reactive to real-time stakeholder changes.

### OM Transfer End-to-End (INT-036–045)

INT-036: Change OM → Previous OM in collaborator list.  
INT-037: Change OM → New OM as Opportunity Manager.  
INT-038: Change OM → All other team data preserved.  
INT-039: Change OM twice → Both previous OMs as collaborators.  
INT-040: Change OM when previous OM already collaborator → No duplicate.  
INT-041: Change OM via API → Correct behavior.  
INT-042: Change OM audit trail → Visible in history.  
INT-043: Change OM notification → Sent to stakeholders.  
INT-044: Change OM on opportunity with many collaborators → Correct.  
INT-045: Change OM and save → Persisted across refresh.

### Error Recovery (INT-046–050)

INT-046: DB error during collaborator add → Transaction rolled back.  
INT-047: DB error during expertise merge → No partial update.  
INT-048: Network error during team save → Error message, retry possible.  
INT-049: Concurrent team updates → Last write wins or conflict detection.  
INT-050: Session timeout during team edit → Re-auth required.

### Extended Integration Flows (INT-051–090)

INT-051: Full flow: Create opportunity → Add org unit → Auto-populate stakeholders → Verify pathway.  
INT-052: Full flow: Add collaborator → Add expertise → Remove expertise → Verify DB state.  
INT-053: Full flow: Change OM → Verify previous OM collaborator → Change OM again → Verify both.  
INT-054: API → Manager → DbContext: Collaborator add propagates correctly.  
INT-055: API → Manager → DbContext: Stakeholder add propagates correctly.  
INT-056: UI → API → DB: Team section save round-trip.  
INT-057: UI → API → DB: Collaborator expertise merge round-trip.  
INT-058: Permission check → API → Manager: 403 when unauthorized.  
INT-059: Opportunity stage change → Team section permissions update.  
INT-060: Org unit change → Stakeholder re-fetch → Pathway recompute.  
INT-061: Collaborator add → Available users list updates (excludes new).  
INT-062: Collaborator remove → Available users list updates (includes removed).  
INT-063: Expertise type added to system → Appears in collaborator dropdown.  
INT-064: Expertise type soft-deleted → Excluded from dropdown.  
INT-065: User soft-deleted → Excluded from collaborator/stakeholder selection.  
INT-066: Role soft-deleted → Excluded from stakeholder role dropdown.  
INT-067: Org unit soft-deleted → Pathway excludes it.  
INT-068: Multi-tab: Edit team in tab A → Refresh tab B → Sees latest.  
INT-069: Multi-tab: Add collaborator in tab A → Tab B list stale until refresh.  
INT-070: API versioning: Team PATCH v1 → Backward compatible.  
INT-071: Localization: Team section labels in all 4 languages.  
INT-072: Localization: Pathway headers in all 4 languages.  
INT-073: Audit: Collaborator add creates audit entry.  
INT-074: Audit: Stakeholder add creates audit entry.  
INT-075: Audit: OM change creates audit entry.  
INT-076: Notification: Collaborator add (if configured).  
INT-077: Notification: OM change (if configured).  
INT-078: Export: Team section in opportunity export.  
INT-079: Export: Pathway in opportunity export.  
INT-080: Import: Team data import (if supported).  
INT-081: Search: Collaborator name in opportunity search.  
INT-082: Search: Stakeholder in opportunity search.  
INT-083: Report: Team section in opportunity report.  
INT-084: Dashboard: Opportunities by collaborator count.  
INT-085: Workflow: Submit for approval with incomplete team → Validation.  
INT-086: Workflow: Approval with team changes → Correct state.  
INT-087: Deep link: Opportunity team section deep link loads correctly.  
INT-088: Breadcrumb: Team section in opportunity breadcrumb.  
INT-089: Accessibility: Team section keyboard navigable.  
INT-090: Accessibility: Pathway readable by screen reader.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users adding same collaborator simultaneously → One succeeds (unique constraint).  
CON-002: Two users removing same collaborator → Both soft-delete (idempotent).  
CON-003: Add and remove same collaborator simultaneously → One operation wins.  
CON-004: Concurrent expertise merge for same collaborator → Final expertise correct.  
CON-005: Concurrent OM change → Last change wins.  
CON-006: Concurrent stakeholder dedup computation → Consistent result.  
CON-007: Concurrent team PATCH requests → Both applied or conflict.  
CON-008: Concurrent auto-populate stakeholders → No duplicates.  
CON-009: Decision pathway computed during concurrent stakeholder update → Consistent.  
CON-010: Two users editing team section simultaneously → Both see latest data on refresh.  
CON-011: Collaborator add during page refresh → One operation completes.  
CON-012: Expertise merge during concurrent page load → Consistent data.  
CON-013: OM change during collaborator add → Both complete.  
CON-014: Stakeholder dedup during concurrent auto-populate → Correct result.  
CON-015: Filtered unique index under concurrent collaborator operations → Enforced.  
CON-016: Concurrent team saves from two tabs → No data corruption.  
CON-017: Concurrent expertise type loading → Single API call (cached).  
CON-018: Rapid collaborator add/remove clicks → Debounced or serialized.  
CON-019: Concurrent decision pathway re-computation → Signal recomputes once.  
CON-020: DbContextFactory used for parallel team queries → Independent contexts.  
CON-021: Optimistic concurrency on team update → Detected and handled.  
CON-022: Concurrent team save during approval submission → One blocked.  
CON-023: Cache invalidation after team change → Next read sees latest.  
CON-024: Concurrent collaborator dialog opens → Modal prevents double entry.  
CON-025: Concurrent stakeholder dialog opens → Modal prevents double entry.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `autoPopulatedStakeholders` computed → Deduplicates by orgUnitId + roleId.  
UNT-002: Dedup prefers entry with userName over entry without.  
UNT-003: Dedup prefers entry with position over entry without.  
UNT-004: Dedup with all entries identical → Returns 1.  
UNT-005: Dedup with all entries unique → Returns all.  
UNT-006: `confirmCollaboratorDialog` → Detects existing collaborator.  
UNT-007: `confirmCollaboratorDialog` → Calls merge for existing.  
UNT-008: `confirmCollaboratorDialog` → Calls add for new user.  
UNT-009: `[...new Set([...existing, ...new])]` → Correct union.  
UNT-010: `[...new Set()]` with empty existing → Returns new.  
UNT-011: `[...new Set()]` with empty new → Returns existing.  
UNT-012: `decisionMakingPathwayStakeholders` → Filters DoA2 and DoA3 only.  
UNT-013: `decisionMakingPathwayStakeholders` → Groups by org unit.  
UNT-014: `responsibleOrgUnitDecisionMakingPathway` → Correct filter.  
UNT-015: `normallyResponsibleDecisionMakingPathway` → Correct filter.  
UNT-016: `normallyResponsibleOrgUnits` → Dedup by `orgUnit.id`.  
UNT-017: `availableCollaboratorUsers` → Excludes existing collaborators.  
UNT-018: OM transfer: previous OM not in collaborators → Returns true for add.  
UNT-019: OM transfer: previous OM already collaborator → Returns false for add.  
UNT-020: `confirmStakeholderDialog` → Checks user-role duplicate.  
UNT-021: Team PATCH request body → Correct JSON structure.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Team section load with 50 collaborators → < 500ms.  
PRF-002: Team section load with 100 stakeholders → < 500ms.  
PRF-003: Decision pathway computation → < 100ms.  
PRF-004: Stakeholder deduplication on 100 entries → < 50ms.  
PRF-005: Collaborator expertise merge → < 50ms.  
PRF-006: Available users list computation → < 200ms.  
PRF-007: Team PATCH API call → < 500ms.  
PRF-008: Collaborator add API call → < 300ms.  
PRF-009: Collaborator remove API call → < 300ms.  
PRF-010: Expertise types GET endpoint → < 200ms.  
PRF-011: OM transfer with auto-collaborator add → < 500ms.  
PRF-012: Decision pathway update after org unit change → < 200ms.  
PRF-013: Computed signal recomputation → < 10ms.  
PRF-014: Team section render with 50 collaborators + 100 stakeholders → < 1s.  
PRF-015: Collaborator dialog open → < 200ms.  
PRF-016: Stakeholder dialog open → < 200ms.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 20 concurrent team section loads → All complete < 2s.  
LDT-002: 50 concurrent collaborator adds across different opportunities → All succeed.  
LDT-003: 100 concurrent team PATCH requests → All succeed or conflict detected.  
LDT-004: Decision pathway computation under load (50 concurrent) → < 500ms each.  
LDT-005: Stakeholder dedup under load → Consistent results.  
LDT-006: Expertise merge under concurrent load → No duplicates.  
LDT-007: OM transfer under concurrent load → One OM at a time.  
LDT-008: Available users list under load → Consistent exclusion.  
LDT-009: Recovery after team section API error → Retry succeeds.  
LDT-010: Sustained team edits (100/hour) → Stable performance.

---

## Status: Ready for Implementation
