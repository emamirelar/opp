# Soft-Delete Filtered Unique Indexes — Comprehensive Test Cases

**Component:** Filtered Unique Indexes on OpportunityCollaborators and OpportunityUNOPSMissions  
**Migration:** `20260204234211_AddFilteredUniqueIndexesForSoftDelete`  
**DbContext:** `AppDbContext.OnModelCreating()` — `HasIndex().IsUnique().HasFilter("\"IsDeleted\" = false")`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | N ≥ 3×P (90) | ✅ |
| 3 | Boundary Tests | §3 | 90 | E ≥ 3×P (90) | ✅ |
| 4 | Functional Tests | §4 | 90 | F ≥ 3×P (90) | ✅ |
| 5 | Integration Tests | §5 | 90 | I ≥ 3×P (90) | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Four Individual Ratio Checks (MANDATORY)

| Check | Formula | Actual | Required | Status |
|-------|---------|--------|----------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Feature Overview

The system adds **filtered unique indexes** on two junction tables to enforce uniqueness only for non-deleted (active) records. This allows soft-deleted records to be "replaced" by new records with the same key combination.

### Affected Tables

| Table | Unique Index Columns | Filter | Purpose |
|-------|---------------------|--------|---------|
| **OpportunityCollaborators** | `(OpportunityId, UserId)` | `"IsDeleted" = false` | Prevent duplicate active collaborator assignments |
| **OpportunityUNOPSMissions** | `(OpportunityId, UNOPSMissionId)` | `"IsDeleted" = false` | Prevent duplicate active mission assignments |

### Behavior

```
Scenario: Re-adding a soft-deleted collaborator
1. Add Collaborator (User=5, Opp=10) → Row 1 created (IsDeleted=false)
2. Soft-delete Collaborator → Row 1 updated (IsDeleted=true)
3. Re-add same Collaborator (User=5, Opp=10) → Row 2 created (IsDeleted=false) ✅
   - Old index would FAIL here (duplicate key)
   - New filtered index ALLOWS this (only checks IsDeleted=false rows)
```

### DbContext Configuration

```csharp
// OpportunityCollaborator
entity.HasIndex(x => new { x.OpportunityId, x.UserId })
    .IsUnique()
    .HasFilter("\"IsDeleted\" = false");

// OpportunityUNOPSMission
entity.HasIndex(x => new { x.OpportunityId, x.UNOPSMissionId })
    .IsUnique()
    .HasFilter("\"IsDeleted\" = false");
```

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### OpportunityCollaborator — Add/Delete/Re-add (POS-001–015)

POS-001: Add collaborator (User=A, Opp=1) → Created successfully, IsDeleted=false.  
POS-002: Add different collaborator (User=B, Opp=1) → Created, no conflict.  
POS-003: Add same user to different opportunity (User=A, Opp=2) → No conflict.  
POS-004: Soft-delete collaborator (User=A, Opp=1) → IsDeleted=true.  
POS-005: Re-add same collaborator after soft-delete (User=A, Opp=1) → New row, IsDeleted=false.  
POS-006: Soft-delete and re-add 3 times → All 3 soft-deleted rows + 1 active row exist.  
POS-007: Multiple collaborators on same opportunity → All unique, no conflicts.  
POS-008: Same user as collaborator on 10 different opportunities → All succeed.  
POS-009: Collaborator re-added with different additional fields → New row independent.  
POS-010: Query active collaborators → Returns only IsDeleted=false rows.  
POS-011: Query all collaborators (including deleted) → Returns all rows.  
POS-012: Count active collaborators → Excludes soft-deleted.  
POS-013: Unique index name matches migration: `IX_OpportunityCollaborators_OpportunityId_UserId`.  
POS-014: Filtered index visible in PostgreSQL `pg_indexes` catalog.  
POS-015: Index filter condition = `"IsDeleted" = false` in database.

### OpportunityUNOPSMission — Add/Delete/Re-add (POS-016–030)

POS-016: Add mission (Mission=A, Opp=1) → Created successfully.  
POS-017: Add different mission (Mission=B, Opp=1) → No conflict.  
POS-018: Add same mission to different opportunity (Mission=A, Opp=2) → No conflict.  
POS-019: Soft-delete mission (Mission=A, Opp=1) → IsDeleted=true.  
POS-020: Re-add same mission after soft-delete → New row, IsDeleted=false.  
POS-021: Soft-delete and re-add 3 times → Multiple soft-deleted + 1 active.  
POS-022: Multiple missions on same opportunity → All unique.  
POS-023: Same mission on 10 different opportunities → All succeed.  
POS-024: Query active missions → Returns only IsDeleted=false.  
POS-025: Count active missions → Excludes soft-deleted.  
POS-026: Unique index name: `IX_OpportunityUNOPSMissions_OpportunityId_UNOPSMissionId`.  
POS-027: Filtered index visible in PostgreSQL catalog.  
POS-028: Both indexes created by migration → Verified in DB schema.  
POS-029: Migration rollback (Down) restores unfiltered indexes.  
POS-030: EF Core model snapshot reflects filtered indexes.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: N ≥ 3×P (90)** | ✅ COMPLIANT

### Duplicate Active Record Violations (NEG-001–020)

NEG-001: Add duplicate active collaborator (same User+Opp, both IsDeleted=false) → Unique constraint violation.  
NEG-002: Add duplicate active mission (same Mission+Opp, both IsDeleted=false) → Unique constraint violation.  
NEG-003: Add collaborator, then add same collaborator WITHOUT soft-deleting first → Violation.  
NEG-004: Add mission, then add same mission WITHOUT soft-deleting first → Violation.  
NEG-005: Bulk insert with duplicate collaborator key → Partial failure or rollback.  
NEG-006: Bulk insert with duplicate mission key → Partial failure or rollback.  
NEG-007: API call to add already-active collaborator → 400 or 409 Conflict.  
NEG-008: API call to add already-active mission → 400 or 409 Conflict.  
NEG-009: Direct SQL INSERT violating filtered unique index → PostgreSQL error.  
NEG-010: EF Core SaveChanges with duplicate → DbUpdateException.  
NEG-011: Two concurrent adds of same collaborator → One fails.  
NEG-012: Two concurrent adds of same mission → One fails.  
NEG-013: Add collaborator with same UserId but null OpportunityId → FK violation.  
NEG-014: Add mission with same MissionId but null OpportunityId → FK violation.  
NEG-015: Add collaborator with non-existent UserId → FK violation.  
NEG-016: Add mission with non-existent MissionId → FK violation.  
NEG-017: Add collaborator with OpportunityId=0 → FK violation.  
NEG-018: Add mission with OpportunityId=0 → FK violation.  
NEG-019: Add collaborator with negative UserId → Validation error.  
NEG-020: Add mission with negative MissionId → Validation error.

### Soft-Delete Logic Failures (NEG-021–040)

NEG-021: Hard-delete (physical remove) collaborator → Row physically removed, re-add works.  
NEG-022: Hard-delete mission → Same behavior.  
NEG-023: Set IsDeleted=true but leave DeletedBy/DeletedDate null → Incomplete audit.  
NEG-024: Set IsDeleted=false on already-active record → No change, no error.  
NEG-025: Set IsDeleted=false on soft-deleted record when active record exists → Unique violation.  
NEG-026: Bulk undelete soft-deleted collaborators → Unique violations if duplicates.  
NEG-027: Bulk undelete soft-deleted missions → Same.  
NEG-028: Update UserId on active collaborator to match another active → Unique violation.  
NEG-029: Update MissionId on active mission to match another active → Unique violation.  
NEG-030: Swap UserId between two active collaborators → Intermediate state may violate.  
NEG-031: SQL UPDATE setting IsDeleted=false for all rows → Unique violations on duplicates.  
NEG-032: Restore soft-deleted collaborator without checking for active duplicate → Violation.  
NEG-033: Restore soft-deleted mission without checking → Violation.  
NEG-034: Concurrent soft-delete + re-add → Race condition, one fails.  
NEG-035: Concurrent restore + new insert with same key → One fails.  
NEG-036: Delete collaborator by wrong ID → Wrong record soft-deleted.  
NEG-037: Delete mission by wrong ID → Wrong record soft-deleted.  
NEG-038: Soft-delete with transaction rollback → Row remains active.  
NEG-039: Soft-delete during concurrent read → Reader sees old or new state.  
NEG-040: Soft-delete on opportunity that is immutable (NO GO stage) → Blocked.

### Data Integrity Failures (NEG-041–055)

NEG-041: Insert collaborator with null UserId → Not null constraint.  
NEG-042: Insert collaborator with null OpportunityId → Not null constraint.  
NEG-043: Insert mission with null MissionId → Not null constraint.  
NEG-044: Insert mission with null OpportunityId → Not null constraint.  
NEG-045: Insert collaborator for soft-deleted opportunity → FK valid but semantically wrong.  
NEG-046: Insert mission for soft-deleted opportunity → Same.  
NEG-047: Insert collaborator for non-existent opportunity → FK violation.  
NEG-048: Insert mission for non-existent opportunity → FK violation.  
NEG-049: Update OpportunityId on collaborator → FK check on new value.  
NEG-050: Update OpportunityId on mission → FK check on new value.  
NEG-051: Insert collaborator without setting Name property → DB constraint (ModifiableDeletableEntity).  
NEG-052: Insert mission without setting Name property → DB constraint.  
NEG-053: Insert with status field invalid → Enum validation.  
NEG-054: Insert without audit fields (CreatedBy, CreatedDate) → DB constraint.  
NEG-055: Insert with future CreatedDate → Accepted but semantically odd.

### Migration-Specific Failures (NEG-056–070)

NEG-056: Run migration on DB with existing duplicate active rows → Migration fails.  
NEG-057: Run migration Down() → Filtered indexes dropped, unfiltered restored.  
NEG-058: Migration Down() + existing soft-deleted duplicates → Unfiltered index may fail.  
NEG-059: Migration re-run (idempotency) → Fails if indexes already exist.  
NEG-060: Migration on empty table → Succeeds (no data to violate).  
NEG-061: Migration on table with only soft-deleted rows → Succeeds.  
NEG-062: Migration with pending transactions → Transaction isolation respected.  
NEG-063: Index creation on large table (100K rows) → Completes within timeout.  
NEG-064: Index creation with concurrent writes → Writes blocked during index creation.  
NEG-065: Drop index + immediate insert of duplicate → Duplicate allowed (no index).  
NEG-066: Filter syntax error in HasFilter → Migration generation fails.  
NEG-067: PostgreSQL version compatibility for partial indexes → Supported (9.5+).  
NEG-068: Index name collision with existing index → Migration drops old first.  
NEG-069: Concurrent migration execution → Lock prevents double-run.  
NEG-070: Migration applied to wrong schema → Schema prefix handles.

### Domain-Specific Negative (NEG-071–090)

NEG-071: Add collaborator when User is soft-deleted → FK valid but semantically invalid.  
NEG-072: Add mission when UNOPSMission entity is soft-deleted → Same.  
NEG-073: Add collaborator to opportunity in Rejected status → Business rule blocks.  
NEG-074: Add mission to opportunity in Rejected status → Same.  
NEG-075: Add collaborator when opportunity WorkflowStatus is Archived → Blocked.  
NEG-076: Add mission when opportunity WorkflowStatus is Archived → Blocked.  
NEG-077: Update collaborator UserId to soft-deleted user → FK valid, business rule may block.  
NEG-078: Update mission UNOPSMissionId to soft-deleted mission → Same.  
NEG-079: Insert collaborator with UserId from different tenant (multi-tenant) → Blocked.  
NEG-080: Insert mission with UNOPSMissionId from inactive mission type → Validation.  
NEG-081: Add collaborator when opportunity has max collaborators limit → Business rule.  
NEG-082: Add mission when opportunity has max missions limit → Same.  
NEG-083: Restore collaborator when user no longer has access → Permission denied.  
NEG-084: Restore mission when mission entity was hard-deleted → FK violation.  
NEG-085: Insert collaborator with duplicate in same transaction (two INSERTs) → Second fails.  
NEG-086: Insert mission with duplicate in same transaction → Same.  
NEG-087: HasFilter with wrong column name (e.g. "isdeleted") → Index creation fails.  
NEG-088: HasFilter with wrong boolean literal → Index may not filter correctly.  
NEG-089: Add collaborator when opportunity is in Submit for Approval → Stage-dependent block.  
NEG-090: Add mission when opportunity is in Submit for Approval → Same.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: E ≥ 3×P (90)** | ✅ COMPLIANT

### Record Count Boundaries (BND-001–020)

BND-001: 0 collaborators on opportunity → No index involvement.  
BND-002: 1 active collaborator → Index has 1 entry.  
BND-003: 100 active collaborators → All unique, index works.  
BND-004: 1000 active collaborators → Performance acceptable.  
BND-005: 0 soft-deleted + 1 active → Index enforces uniqueness on 1.  
BND-006: 1 soft-deleted + 1 active (same key) → Both exist, index passes.  
BND-007: 10 soft-deleted + 1 active (same key) → All exist, index passes.  
BND-008: 100 soft-deleted + 0 active (same key) → No unique constraint issue.  
BND-009: 0 missions on opportunity → No index involvement.  
BND-010: 1 active mission → Index has 1 entry.  
BND-011: 100 active missions → All unique.  
BND-012: 1000 active missions → Performance acceptable.  
BND-013: Mixed: 50 active + 50 soft-deleted collaborators → Correct filtering.  
BND-014: Mixed: 50 active + 50 soft-deleted missions → Correct filtering.  
BND-015: Opportunity with maximum allowed collaborators → Index handles.  
BND-016: Opportunity with maximum allowed missions → Index handles.  
BND-017: Single user as collaborator on 100 opportunities → No cross-opportunity conflict.  
BND-018: Single mission on 100 opportunities → No cross-opportunity conflict.  
BND-019: All collaborators soft-deleted on opportunity → 0 active, no constraint.  
BND-020: All missions soft-deleted → 0 active, no constraint.

### ID Value Boundaries (BND-021–040)

BND-021: OpportunityId = 1 (minimum valid) → Index works.  
BND-022: OpportunityId = MAX_INT → Index works.  
BND-023: UserId = 1 (minimum valid) → Index works.  
BND-024: UserId = MAX_INT → Index works.  
BND-025: MissionId = 1 → Index works.  
BND-026: MissionId = MAX_INT → Index works.  
BND-027: Composite key (1, 1) → Works.  
BND-028: Composite key (MAX_INT, MAX_INT) → Works.  
BND-029: Composite key (1, MAX_INT) → Works.  
BND-030: Composite key (MAX_INT, 1) → Works.  
BND-031: Two records with adjacent IDs (User=5 and User=6, same Opp) → Both active, no conflict.  
BND-032: Two records with same UserId, adjacent OpportunityIds → No conflict.  
BND-033: Collaborator with UserId that matches OpportunityId → No confusion.  
BND-034: Mission with MissionId that matches OpportunityId → No confusion.  
BND-035: Record with all FK IDs = 1 → Valid if entities exist.  
BND-036: IsDeleted transition: false→true → Index entry removed from filter.  
BND-037: IsDeleted transition: true→false (restore) → Index entry added, checked.  
BND-038: IsDeleted transition: false→false → No change.  
BND-039: IsDeleted transition: true→true → No change.  
BND-040: IsDeleted default value = false → New records in index.

### Concurrent Operation Boundaries (BND-041–055)

BND-041: Add + soft-delete same record simultaneously → One completes first.  
BND-042: Soft-delete + re-add simultaneously → One may fail.  
BND-043: Two adds of same key simultaneously → One fails (unique constraint).  
BND-044: Add during index rebuild → Record either in or out of index.  
BND-045: Soft-delete during query → Query sees consistent snapshot.  
BND-046: Bulk add 100 collaborators in single transaction → All or none.  
BND-047: Bulk soft-delete 100 collaborators in single transaction → All or none.  
BND-048: Bulk add with 1 duplicate in batch → Entire batch fails.  
BND-049: Concurrent bulk operations on different opportunities → Independent.  
BND-050: Transaction rollback after add → Index entry removed.  
BND-051: Transaction rollback after soft-delete → Index entry restored.  
BND-052: Savepoint within transaction + partial rollback → Index consistent.  
BND-053: Nested transactions with collaborator changes → Correct final state.  
BND-054: Long-running transaction holding row lock → Other writers wait.  
BND-055: Connection pool exhaustion during index check → Queued.

### Query Boundaries (BND-056–070)

BND-056: Query with filter `!IsDeleted` → Only active records.  
BND-057: Query without filter → All records (active + deleted).  
BND-058: Include(`Collaborators.Where(c => !c.IsDeleted)`) → Filtered correctly.  
BND-059: Count with `!IsDeleted` → Correct active count.  
BND-060: Count without filter → Total count.  
BND-061: Any() with `!IsDeleted` → True if active records exist.  
BND-062: Any() on empty opportunity → False.  
BND-063: FirstOrDefault with `!IsDeleted` → Returns active record.  
BND-064: FirstOrDefault without filter → May return deleted record.  
BND-065: OrderBy on filtered collection → Correct ordering.  
BND-066: GroupBy on filtered collection → Correct grouping.  
BND-067: Distinct on filtered collection → No duplicates.  
BND-068: Join with filtered collaborators → Correct join results.  
BND-069: Subquery with filtered missions → Correct subquery results.  
BND-070: Aggregate (Sum/Avg) on filtered collection → Correct values.

### Domain-Specific Boundaries (BND-071–090)

BND-071: Opportunity at Draft stage with 0 collaborators → Add succeeds.  
BND-072: Opportunity at Active stage with 1 collaborator → Re-add after delete succeeds.  
BND-073: Opportunity at NO GO stage → Add collaborator blocked, soft-delete allowed.  
BND-074: Opportunity at Rejected stage → Add collaborator blocked.  
BND-075: Opportunity transitioning Draft→Active with collaborators → Index consistent.  
BND-076: Last active collaborator soft-deleted → Opportunity has 0 active collaborators.  
BND-077: Last active mission soft-deleted → Opportunity has 0 active missions.  
BND-078: Collaborator with Role=Owner vs Role=Viewer → Same index (UserId+OppId).  
BND-079: Mission with different MissionType → Same index (MissionId+OppId).  
BND-080: Name property at max length (ModifiableDeletableEntity) → Index unaffected.  
BND-081: Status=Active vs Status=Inactive on collaborator → Index filters IsDeleted only.  
BND-082: WorkflowStatus change on opportunity → Child index entries unchanged.  
BND-083: DeletedDate at DateTime.MinValue (edge) → Filter still correct.  
BND-084: DeletedDate at DateTime.MaxValue → Filter still correct.  
BND-085: CreatedDate = DeletedDate (immediate delete) → Index entry removed.  
BND-086: Re-add within same second as soft-delete → New row, new CreatedDate.  
BND-087: Collaborator added then opportunity soft-deleted → Collaborator query filtered.  
BND-088: Mission added then UNOPSMission soft-deleted → Mission FK still valid.  
BND-089: Index entry count equals active row count → Verified via pg_index.  
BND-090: Zero-length string for Name (if allowed) → Index on (OppId, UserId) unaffected.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: F ≥ 3×P (90)** | ✅ COMPLIANT

### Index Enforcement Rules (FUN-001–015)

FUN-001: Active duplicate collaborator blocked by unique index.  
FUN-002: Active duplicate mission blocked by unique index.  
FUN-003: Soft-deleted + new active (same key) allowed by filtered index.  
FUN-004: Multiple soft-deleted (same key) allowed.  
FUN-005: Index filter condition matches DbContext HasFilter.  
FUN-006: Index applies to INSERT operations.  
FUN-007: Index applies to UPDATE operations (changing UserId to existing).  
FUN-008: Index does NOT apply to soft-deleted rows.  
FUN-009: Index constraint error returns correct PostgreSQL error code.  
FUN-010: EF Core translates unique violation to DbUpdateException.  
FUN-011: Unique violation message identifies conflicting columns.  
FUN-012: Unique violation does not corrupt existing data.  
FUN-013: Unique violation rolls back entire transaction.  
FUN-014: Index works correctly with AsNoTracking queries.  
FUN-015: Index works correctly with tracked queries.

### Soft-Delete Workflow Rules (FUN-016–030)

FUN-016: Soft-delete sets IsDeleted=true.  
FUN-017: Soft-delete sets DeletedBy to current user.  
FUN-018: Soft-delete sets DeletedDate to DateTime.UtcNow.  
FUN-019: Soft-delete does NOT physically remove the row.  
FUN-020: After soft-delete, same key can be re-inserted.  
FUN-021: Re-insert creates new row (not update of deleted row).  
FUN-022: Re-insert has new Id (auto-increment).  
FUN-023: Re-insert has new CreatedBy/CreatedDate.  
FUN-024: Re-insert has IsDeleted=false.  
FUN-025: Old soft-deleted row retains original audit fields.  
FUN-026: Query with `!IsDeleted` excludes soft-deleted rows.  
FUN-027: Dropdown/typeahead excludes soft-deleted collaborators.  
FUN-028: Dropdown/typeahead excludes soft-deleted missions.  
FUN-029: API response for opportunity excludes soft-deleted children.  
FUN-030: Opportunity detail page shows only active collaborators/missions.

### Migration Rules (FUN-031–040)

FUN-031: Migration Up() drops old unfiltered indexes.  
FUN-032: Migration Up() creates new filtered indexes.  
FUN-033: Migration Down() drops filtered indexes.  
FUN-034: Migration Down() restores unfiltered indexes.  
FUN-035: DbContext HasFilter matches migration filter string exactly.  
FUN-036: Index name consistent between DbContext and migration.  
FUN-037: Model snapshot reflects filtered index configuration.  
FUN-038: New migrations generated after this one respect filtered indexes.  
FUN-039: EF Core `dotnet ef migrations` includes filter in generated code.  
FUN-040: Index visible in PostgreSQL `\di` output.

### Audit Rules (FUN-041–050)

FUN-041: Soft-delete of collaborator creates audit trail.  
FUN-042: Re-add of collaborator creates audit trail.  
FUN-043: Soft-delete of mission creates audit trail.  
FUN-044: Re-add of mission creates audit trail.  
FUN-045: Audit records include correct entity type.  
FUN-046: Audit records include correct action (Delete, Create).  
FUN-047: Audit records include correct user.  
FUN-048: Audit records include correct timestamp.  
FUN-049: Unique constraint violation logged as warning.  
FUN-050: Successful re-add after soft-delete logged as info.

### Domain-Specific Functional (FUN-051–090)

FUN-051: OpportunityCollaborator inherits ModifiableDeletableEntity → Name required.  
FUN-052: OpportunityUNOPSMission inherits ModifiableDeletableEntity → Name required.  
FUN-053: Collaborator Role field independent of unique index.  
FUN-054: Mission MissionType independent of unique index.  
FUN-055: GetOpportunityDetailsForAIAsync excludes soft-deleted collaborators.  
FUN-056: GetOpportunityDetailsForAIAsync excludes soft-deleted missions.  
FUN-057: Opportunity list/detail API filters collaborators by IsDeleted.  
FUN-058: Opportunity list/detail API filters missions by IsDeleted.  
FUN-059: Permission endpoint for collaborator respects soft-delete.  
FUN-060: Permission endpoint for mission respects soft-delete.  
FUN-061: Workflow component shows only active collaborators.  
FUN-062: Workflow component shows only active missions.  
FUN-063: Team section on opportunity excludes soft-deleted.  
FUN-064: Mission section on opportunity excludes soft-deleted.  
FUN-065: Add collaborator API validates no active duplicate before insert.  
FUN-066: Add mission API validates no active duplicate before insert.  
FUN-067: Soft-delete collaborator API sets DeletedBy from current user.  
FUN-068: Soft-delete mission API sets DeletedBy from current user.  
FUN-069: Re-add collaborator creates new audit record (Create action).  
FUN-070: Re-add mission creates new audit record (Create action).  
FUN-071: Include chain Opportunity→Collaborators filters IsDeleted.  
FUN-072: Include chain Opportunity→Missions filters IsDeleted.  
FUN-073: Batch query for collaborators uses filtered index.  
FUN-074: Batch query for missions uses filtered index.  
FUN-075: DbContextFactory parallel queries each respect filtered index.  
FUN-076: AsNoTracking on collaborator query does not affect index.  
FUN-077: AsNoTracking on mission query does not affect index.  
FUN-078: Split query strategy loads only active collaborators.  
FUN-079: Split query strategy loads only active missions.  
FUN-080: EDS (Entity Data Service) respects filtered unique index on upsert.  
FUN-081: Data import/export excludes soft-deleted by default.  
FUN-082: Report generation counts only active collaborators.  
FUN-083: Report generation counts only active missions.  
FUN-084: Search index excludes soft-deleted collaborator assignments.  
FUN-085: Search index excludes soft-deleted mission assignments.  
FUN-086: oUP integration sync excludes soft-deleted children.  
FUN-087: Deep link to opportunity shows only active collaborators.  
FUN-088: Deep link to opportunity shows only active missions.  
FUN-089: Rules engine evaluates only active collaborators.  
FUN-090: Rules engine evaluates only active missions.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: I ≥ 3×P (90)** | ✅ COMPLIANT

### CRUD Workflow (INT-001–015)

INT-001: Create opportunity → Add collaborator → Verify in DB.  
INT-002: Create opportunity → Add mission → Verify in DB.  
INT-003: Add → Soft-delete → Re-add collaborator → 2 rows in DB (1 deleted, 1 active).  
INT-004: Add → Soft-delete → Re-add mission → 2 rows in DB.  
INT-005: Add → Soft-delete → Re-add → Soft-delete → Re-add → 3 rows (2 deleted, 1 active).  
INT-006: Add collaborator via API → Verify response and DB state.  
INT-007: Delete collaborator via API → Verify soft-delete in DB.  
INT-008: Re-add collaborator via API → Verify new row in DB.  
INT-009: Add mission via API → Verify response and DB state.  
INT-010: Delete mission via API → Verify soft-delete.  
INT-011: Re-add mission via API → Verify new row.  
INT-012: Duplicate active collaborator via API → 400/409 error.  
INT-013: Duplicate active mission via API → 400/409 error.  
INT-014: Add collaborator to immutable (NO GO) opportunity → Blocked.  
INT-015: Soft-delete collaborator on immutable opportunity → Blocked.

### Search & Filter (INT-016–025)

INT-016: List collaborators API → Returns only active (IsDeleted=false).  
INT-017: List missions API → Returns only active.  
INT-018: Opportunity detail includes active collaborators only.  
INT-019: Opportunity detail includes active missions only.  
INT-020: Search by collaborator user → Finds opportunity.  
INT-021: Search by mission → Finds opportunity.  
INT-022: Filter opportunities by collaborator count → Correct (active only).  
INT-023: Filter opportunities by mission count → Correct (active only).  
INT-024: Pagination on collaborators → Correct count.  
INT-025: Sort collaborators by name → Correct ordering.

### Relationship Integrity (INT-026–035)

INT-026: Delete user → Collaborator FK behavior correct (cascade/set null).  
INT-027: Delete mission entity → Mission FK behavior correct.  
INT-028: Delete opportunity → Collaborator/mission cascade behavior.  
INT-029: Soft-delete opportunity → Child collaborator queries filtered.  
INT-030: Restore opportunity → Child collaborators visible again.  
INT-031: Collaborator → User navigation property loads correctly.  
INT-032: Mission → UNOPSMission navigation property loads correctly.  
INT-033: Opportunity → Collaborators collection loads only active.  
INT-034: Opportunity → Missions collection loads only active.  
INT-035: Include chain: Opportunity → Collaborators → User → Works with filter.

### Error Handling (INT-036–050)

INT-036: Unique violation → Transaction rolled back, no partial state.  
INT-037: Unique violation → Correct HTTP error code returned.  
INT-038: Unique violation → Error message identifies duplicate key.  
INT-039: FK violation (non-existent user) → Correct error.  
INT-040: FK violation (non-existent mission) → Correct error.  
INT-041: Concurrent add of same key → One succeeds, one gets unique violation.  
INT-042: Concurrent soft-delete + re-add → Consistent final state.  
INT-043: DB connection failure during add → Transaction rolled back.  
INT-044: Timeout during add → No partial row inserted.  
INT-045: Large batch insert with one duplicate → Entire batch fails.  
INT-046: EDS bulk upsert respects filtered unique index.  
INT-047: Data import respects filtered unique index.  
INT-048: Migration rollback restores original index behavior.  
INT-049: Index rebuild after data corruption → Index consistent.  
INT-050: Vacuum/analyze after many soft-deletes → Index statistics updated.

### Domain-Specific Integration (INT-051–090)

INT-051: Add collaborator → OpportunityManager → Verify in OpportunityDetailModel.  
INT-052: Add mission → OpportunityManager → Verify in OpportunityDetailModel.  
INT-053: GetOpportunityDetailsForAIAsync → Collaborators filtered.  
INT-054: GetOpportunityDetailsForAIAsync → Missions filtered.  
INT-055: Partner contact as collaborator → User resolution correct.  
INT-056: UNOPSMission from dropdown → Mission resolution correct.  
INT-057: OpportunityController add collaborator → Full request/response cycle.  
INT-058: OpportunityController add mission → Full request/response cycle.  
INT-059: OpportunityController delete collaborator → Soft-delete verified.  
INT-060: OpportunityController delete mission → Soft-delete verified.  
INT-061: Permission endpoint /opportunity/{id}/permissions → Collaborator count correct.  
INT-062: Permission endpoint → Mission count correct.  
INT-063: Angular opportunity-item component → Team section shows active only.  
INT-064: Angular opportunity-item component → Mission section shows active only.  
INT-065: Submit for approval with collaborators → Validation passes.  
INT-066: Submit for approval with missions → Validation passes.  
INT-067: The Go Decision flow with collaborators → Correct state.  
INT-068: The Go Decision flow with missions → Correct state.  
INT-069: AI prompt with collaborator context → Excludes soft-deleted.  
INT-070: AI prompt with mission context → Excludes soft-deleted.  
INT-071: Rules engine opportunity evaluation → Active collaborators only.  
INT-072: Rules engine opportunity evaluation → Active missions only.  
INT-073: oUP sync export → Excludes soft-deleted collaborators.  
INT-074: oUP sync export → Excludes soft-deleted missions.  
INT-075: Deep link /opportunity/{id} → Collaborators loaded correctly.  
INT-076: Deep link /opportunity/{id} → Missions loaded correctly.  
INT-077: URL routing migration → Opportunity routes work with collaborators.  
INT-078: URL routing migration → Opportunity routes work with missions.  
INT-079: Cloud Scheduler AI refresh → Uses filtered collaborator data.  
INT-080: Cloud Scheduler AI refresh → Uses filtered mission data.  
INT-081: Database migration sequence → Filtered indexes applied in order.  
INT-082: Geography indices → No conflict with collaborator/mission indexes.  
INT-083: Soft-delete unique indexes + DbContextFactory → Concurrent queries.  
INT-084: Soft-delete unique indexes + split query → Performance correct.  
INT-085: Audit trail integration → Collaborator add/delete logged.  
INT-086: Audit trail integration → Mission add/delete logged.  
INT-087: Clearbit partner cleanup → Does not affect collaborator assignments.  
INT-088: Clearbit partner cleanup → Does not affect mission assignments.  
INT-089: Submit approval dialog UX → Collaborator list reflects soft-delete.  
INT-090: Submit approval dialog UX → Mission list reflects soft-delete.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users add same collaborator simultaneously → One succeeds, one gets unique violation.  
CON-002: Two users add same mission simultaneously → Same behavior.  
CON-003: Soft-delete + re-add race → Final state has exactly 1 active record.  
CON-004: Add + add of same key from two transactions → Second waits, then fails.  
CON-005: Concurrent soft-deletes of same record → Both succeed (idempotent).  
CON-006: Concurrent adds of different collaborators → Both succeed.  
CON-007: Concurrent adds of different missions → Both succeed.  
CON-008: Transaction A adds, Transaction B queries → B sees old or new based on isolation.  
CON-009: Transaction A soft-deletes, Transaction B re-adds → Depends on commit order.  
CON-010: Read Committed isolation → Correct behavior for filtered index.  
CON-011: Serializable isolation → Prevents phantom reads on index.  
CON-012: 10 concurrent adds to same opportunity (different users) → All succeed.  
CON-013: 10 concurrent adds to same opportunity (same user) → 1 succeeds, 9 fail.  
CON-014: Bulk insert during concurrent single insert → Correct locking.  
CON-015: Index check during concurrent UPDATE → Correct enforcement.  
CON-016: Soft-delete during active query → Query sees consistent snapshot.  
CON-017: Re-add during active Include() query → Consistent snapshot.  
CON-018: Connection pool exhaustion during concurrent adds → Queued, not corrupted.  
CON-019: DbContext per-request isolation → No cross-request state.  
CON-020: Optimistic concurrency on collaborator update → Detected.  
CON-021: Parallel DbContextFactory contexts → Independent index checks.  
CON-022: Concurrent migration + DML → Migration locks table.  
CON-023: Deadlock between two collaborator operations → Detected and retried.  
CON-024: Concurrent audit trail writes for collaborator changes → All persisted.  
CON-025: Cache invalidation after collaborator add/delete → Fresh data on next query.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: IsCollaboratorActive returns true when IsDeleted=false.  
UNT-002: IsCollaboratorActive returns false when IsDeleted=true.  
UNT-003: IsMissionActive returns true when IsDeleted=false.  
UNT-004: IsMissionActive returns false when IsDeleted=true.  
UNT-005: HasActiveCollaborator(oppId, userId) returns true when active record exists.  
UNT-006: HasActiveCollaborator returns false when only soft-deleted exists.  
UNT-007: HasActiveCollaborator returns false when no record exists.  
UNT-008: HasActiveMission(oppId, missionId) returns true when active.  
UNT-009: HasActiveMission returns false when only soft-deleted.  
UNT-010: HasActiveMission returns false when no record.  
UNT-011: GetActiveCollaborators filters IsDeleted=false.  
UNT-012: GetActiveMissions filters IsDeleted=false.  
UNT-013: SoftDelete sets IsDeleted, DeletedBy, DeletedDate.  
UNT-014: SoftDelete does not change Id or CreatedBy.  
UNT-015: CanAddCollaborator returns true when no active duplicate.  
UNT-016: CanAddCollaborator returns false when active duplicate exists.  
UNT-017: CanAddMission returns true when no active duplicate.  
UNT-018: CanAddMission returns false when active duplicate exists.  
UNT-019: FilterExpression generates correct SQL WHERE clause.  
UNT-020: IndexConfiguration includes IsUnique=true.  
UNT-021: IndexConfiguration includes correct HasFilter value.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Add collaborator < 50ms.  
PRF-002: Soft-delete collaborator < 50ms.  
PRF-003: Re-add collaborator after soft-delete < 50ms.  
PRF-004: Add mission < 50ms.  
PRF-005: Unique violation detection < 10ms.  
PRF-006: Query active collaborators (100 active, 100 deleted) < 50ms.  
PRF-007: Query active missions (100 active, 100 deleted) < 50ms.  
PRF-008: Filtered index scan vs full table scan → Index faster.  
PRF-009: Bulk add 100 collaborators in single transaction < 500ms.  
PRF-010: Bulk soft-delete 100 collaborators < 500ms.  
PRF-011: Index overhead on INSERT < 5ms per row.  
PRF-012: Index overhead on UPDATE < 5ms per row.  
PRF-013: Table with 10K collaborator rows → Index lookup < 10ms.  
PRF-014: Table with 100K total rows (50K deleted) → Index lookup < 20ms.  
PRF-015: Memory usage stable after 1000 add/delete cycles.  
PRF-016: VACUUM ANALYZE improves query plan after many soft-deletes.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 100 concurrent collaborator adds per minute → All unique, no violations.  
LDT-002: 50 concurrent soft-delete + re-add cycles → All consistent.  
LDT-003: 200 concurrent queries on filtered index → 95th percentile < 50ms.  
LDT-004: Spike: 100 adds in 5 seconds → Index handles.  
LDT-005: Spike: 100 soft-deletes in 5 seconds → Index handles.  
LDT-006: Stress: max concurrent adds before unique violation false positive.  
LDT-007: Stress: table at 1M rows → Index performance still acceptable.  
LDT-008: Index maintenance cost under sustained write load.  
LDT-009: Recovery after index corruption → REINDEX restores.  
LDT-010: Recovery after mass soft-delete → VACUUM + queries stable.

---

## Status: Ready for Implementation
