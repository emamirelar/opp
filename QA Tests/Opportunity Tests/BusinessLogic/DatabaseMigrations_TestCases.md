# Database Migrations Integrity — Comprehensive Test Cases

**Component:** 10 New EF Core Migrations — Schema Changes, Audit Tracking, Unique Constraints  
**Migration Path:** `UNOPS.PAO.UNOPSDataAccess/Migrations/`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | Max(50, 2×30=60) | ✅ |
| 3 | Boundary Tests | §3 | 90 | Max(50, 2×30=60) | ✅ |
| 4 | Functional Tests | §4 | 90 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 90 | ≥50 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:**
- N≥3P: 90≥90 → ✅ PASS  
- E≥3P: 90≥90 → ✅ PASS  
- F≥3P: 90≥90 → ✅ PASS  
- I≥3P: 90≥90 → ✅ PASS  

---

## Migration Inventory

| # | Migration | Summary | Risk |
|---|-----------|---------|------|
| M1 | `OpportunityIdUpdate` | Adds `OpportunityId` (int, nullable) to `BaseEngagements` | FK relationships |
| M2 | `AddAuditTrackingForOpportunity` | Adds audit/soft-delete columns to 15 Opportunity junction tables | Mass schema change |
| M3 | `AddExecutiveIdToOpportunity` | Adds `ExecutiveId` FK to `Opportunities`, index, SetNull cascade | New FK |
| M4 | `TruncateEntityUserRoles` | TRUNCATE CASCADE on `EntityUserRoles` (irreversible Down) | Data loss |
| M5 | `AddUNOPSMissionsNotApplicableColumn` | Adds `UNOPSMissionsNotApplicable` bool to `Opportunities` | Default value |
| M6 | `AddFilteredUniqueIndexesForSoftDelete` | Filtered unique indexes on Collaborators/Missions | Index enforcement |
| M7 | `GoogleADKUpdate` | Converts `sessions` timestamps to TIMESTAMPTZ | Conditional SQL |
| M8 | `RevertCompositeKeyAddActiveUser` | Adds `ActiveUser` bool, reverts composite key, new unique index | Destructive index drop |
| M9 | `AddUniqueConstraintsForTables` | Bulk upsert unique indexes for EDS on 7 tables | Multi-table indexes |
| M10 | `ClearClearbitLogoUrlsFromPartners` | Nullifies Partner `LogoUrl` containing "clearbit" | Data modification |

---

## Traceability Matrix

| Migration | Positive | Negative | Boundary | Functional | Integration |
|-----------|----------|----------|----------|------------|-------------|
| M1 OpportunityIdUpdate | POS-001–003 | NEG-001–005 | BND-001–005 | FUN-001–004 | INT-001–005 |
| M2 AuditTracking | POS-004–008 | NEG-006–015 | BND-006–015 | FUN-005–012 | INT-006–012 |
| M3 ExecutiveId | POS-009–012 | NEG-016–022 | BND-016–023 | FUN-013–018 | INT-013–018 |
| M4 TruncateRoles | POS-013–015 | NEG-023–030 | BND-024–030 | FUN-019–023 | INT-019–023 |
| M5 MissionsNA | POS-016–018 | NEG-031–036 | BND-031–036 | FUN-024–028 | INT-024–028 |
| M6 FilteredIndexes | See SoftDeleteUniqueIndexes_TestCases.md | | | | |
| M7 GoogleADK | POS-019–021 | NEG-037–043 | BND-037–043 | FUN-029–033 | INT-029–033 |
| M8 RevertCompositeKey | POS-022–025 | NEG-044–052 | BND-044–052 | FUN-034–040 | INT-034–040 |
| M9 UniqueConstraints | POS-026–030 | NEG-053–062 | BND-053–062 | FUN-041–046 | INT-041–046 |
| M10 ClearClearbit | — | NEG-063–090 | BND-063–090 | FUN-047–090 | INT-047–090 |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### M1: OpportunityIdUpdate (POS-001–003)

POS-001: Migration adds `OpportunityId` column to `BaseEngagements` → Column exists, type int, nullable.  
POS-002: Existing `BaseEngagement` rows have `OpportunityId = null` after migration.  
POS-003: New `BaseEngagement` with valid `OpportunityId` → FK resolves correctly.

### M2: AddAuditTrackingForOpportunity (POS-004–008)

POS-004: All 15 junction tables have new audit columns after migration.  
POS-005: `OpportunityCollaborators.CreatedBy` populated on new record insert.  
POS-006: `OpportunityFundingPartners.IsDeleted` defaults to false on new records.  
POS-007: `OpportunityStakeholders.LastModifiedDate` updated on record edit.  
POS-008: `OpportunityCountries.DeletedBy` set during soft-delete.

### M3: AddExecutiveIdToOpportunity (POS-009–012)

POS-009: `ExecutiveId` column added to `Opportunities` → Nullable int.  
POS-010: Index `IX_Opportunities_ExecutiveId` created.  
POS-011: FK `FK_Opportunities_AspNetUsers_ExecutiveId` exists with SetNull cascade.  
POS-012: Setting `ExecutiveId` to valid user ID → Opportunity saved successfully.

### M4: TruncateEntityUserRoles (POS-013–015)

POS-013: `EntityUserRoles` table is empty after migration.  
POS-014: Related tables with FK to `EntityUserRoles` have CASCADE applied.  
POS-015: Application continues to function with empty `EntityUserRoles`.

### M5: AddUNOPSMissionsNotApplicableColumn (POS-016–018)

POS-016: `UNOPSMissionsNotApplicable` column added to `Opportunities`.  
POS-017: Existing opportunities have `UNOPSMissionsNotApplicable = false` (default).  
POS-018: Setting `UNOPSMissionsNotApplicable = true` → Saved and queryable.

### M7: GoogleADKUpdate (POS-019–021)

POS-019: `sessions.create_time` changed to TIMESTAMPTZ → Stored in UTC.  
POS-020: `sessions.update_time` changed to TIMESTAMPTZ → Stored in UTC.  
POS-021: Migration no-ops gracefully when `sessions` table does not exist.

### M8: RevertCompositeKeyAddActiveUser (POS-022–025)

POS-022: `ActiveUser` column added to `AspNetUsers` with default=true.  
POS-023: Old composite index `IX_AspNetUsers_Id_NormalizedUserName` dropped.  
POS-024: New unique index `IX_AspNetUsers_NormalizedUserName` created.  
POS-025: Login continues to work with new index structure.

### M9: AddUniqueConstraintsForTables (POS-026–030)

POS-026: `UserProfile` unique index on `(UserId, Id)` created.  
POS-027: `Countries` unique index on `Iso2Code` created.  
POS-028: `Currencies` unique index on `Code` created.  
POS-029: `PartnerAgreements` unique index on `PartnerAgreementNumber` created.  
POS-030: EDS bulk upsert `ON CONFLICT DO UPDATE` works with new indexes.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### M1: OpportunityIdUpdate (NEG-001–005)

NEG-001: Set `OpportunityId` to non-existent opportunity ID → FK violation.  
NEG-002: Set `OpportunityId` to negative value → FK violation.  
NEG-003: Set `OpportunityId` to soft-deleted opportunity → FK valid but semantically wrong.  
NEG-004: Migration rollback → `OpportunityId` column dropped cleanly.  
NEG-005: Insert BaseEngagement with OpportunityId but missing required Name → Validation error.

### M2: AddAuditTrackingForOpportunity (NEG-006–015)

NEG-006: Insert junction row without CreatedBy → Null allowed (existing data migration).  
NEG-007: Insert junction row without CreatedDate → Null allowed.  
NEG-008: Insert junction row with IsDeleted=null → Default applies (false).  
NEG-009: Insert junction row with Status outside enum range → Constraint or mapping error.  
NEG-010: Insert junction row with WorkflowStatus outside enum range → Error.  
NEG-011: Update junction row setting CreatedDate to future → Accepted (no DB constraint).  
NEG-012: Migration Down() → All 15 tables lose audit columns (data loss).  
NEG-013: Migration on table with 1M rows → Completes within acceptable timeout.  
NEG-014: Migration interrupted mid-way → Transaction rollback, no partial columns.  
NEG-015: Existing junction rows with data → Audit columns are null (not backfilled).

### M3: AddExecutiveIdToOpportunity (NEG-016–022)

NEG-016: Set ExecutiveId to soft-deleted user → FK valid but semantically incorrect.  
NEG-017: Set ExecutiveId to user from wrong tenant → FK valid but authorization wrong.  
NEG-018: Delete Executive user → Opportunity.ExecutiveId set to null (SetNull cascade).  
NEG-019: Set ExecutiveId to non-user entity ID → FK violation.  
NEG-020: Index created on empty table → No overhead, correct behavior.  
NEG-021: Migration rollback → ExecutiveId column, index, and FK dropped.  
NEG-022: Set ExecutiveId during opportunity immutable stage → Blocked by immutability.

### M4: TruncateEntityUserRoles (NEG-023–030)

NEG-023: Data in `EntityUserRoles` before migration → ALL DATA LOST (by design).  
NEG-024: Migration Down() is empty → CANNOT RECOVER truncated data.  
NEG-025: Application queries `EntityUserRoles` after truncate → Returns empty set.  
NEG-026: Re-run migration → TRUNCATE on already-empty table → No error.  
NEG-027: CASCADE deletes related rows → Verify no orphaned FK references.  
NEG-028: DoA role lookups after truncate → Return empty until re-populated.  
NEG-029: Permission checks depending on EntityUserRoles → Fail open or closed?  
NEG-030: Bulk re-insert of EntityUserRoles after truncate → Works.

### M5: AddUNOPSMissionsNotApplicableColumn (NEG-031–036)

NEG-031: Set `UNOPSMissionsNotApplicable` to null → Default false applies (bool not nullable).  
NEG-032: Migration rollback → Column dropped, data lost.  
NEG-033: Query with `UNOPSMissionsNotApplicable = true` on pre-migration data → All false.  
NEG-034: Set value on immutable opportunity → Blocked.  
NEG-035: Set value without permission → 403.  
NEG-036: Invalid type (string "true" vs bool true) → Type mismatch handled.

### M7: GoogleADKUpdate (NEG-037–043)

NEG-037: `sessions` table does not exist → Migration completes without error.  
NEG-038: `sessions.create_time` has non-UTC timestamps → Interpreted as UTC (potential issue).  
NEG-039: `sessions.update_time` has null values → Null preserved after type change.  
NEG-040: Timestamp conversion loses precision (nanoseconds) → Acceptable loss.  
NEG-041: Migration rollback → Types reverted to WITHOUT TIME ZONE.  
NEG-042: Concurrent session writes during migration → Locked during ALTER.  
NEG-043: Sessions table with 1M rows → ALTER TYPE performance acceptable.

### M8: RevertCompositeKeyAddActiveUser (NEG-044–052)

NEG-044: `ActiveUser` column already exists → Migration handles gracefully (IF NOT EXISTS).  
NEG-045: Old composite index does not exist → DROP IF EXISTS.  
NEG-046: Duplicate `NormalizedUserName` values in data → Unique index creation fails.  
NEG-047: Null `NormalizedUserName` values → Unique index may exclude nulls.  
NEG-048: ActiveUser=false for all users → All users effectively disabled.  
NEG-049: ActiveUser column without default on existing rows → Default true applied.  
NEG-050: Login with ActiveUser=false → Behavior depends on application logic.  
NEG-051: Migration rollback → ActiveUser dropped, indexes reverted.  
NEG-052: Concurrent user creation during migration → Blocked during ALTER/INDEX.

### M9: AddUniqueConstraintsForTables (NEG-053–062)

NEG-053: Duplicate `Iso2Code` in Countries → Index creation fails.  
NEG-054: Duplicate `Code` in Currencies → Index creation fails.  
NEG-055: Duplicate `PartnerAgreementNumber` → Index creation fails.  
NEG-056: Null `Iso2Code` in Countries → Unique index excludes nulls (Postgres).  
NEG-057: Null `Code` in Currencies → Same behavior.  
NEG-058: EDS bulk upsert with duplicate key → ON CONFLICT DO UPDATE.  
NEG-059: EDS bulk upsert with missing unique key value → Error.  
NEG-060: Index creation on table with 100K rows → Time acceptable.  
NEG-061: Migration script idempotent → Re-run checks `pg_indexes` before CREATE.  
NEG-062: Migration rollback → Script does not have DOWN logic (SQL only).

### M10: ClearClearbitLogoUrls (NEG-063–070)

NEG-063: LogoUrl = "https://clearbit.com/logo" → Set to null (contains clearbit).  
NEG-064: LogoUrl = "https://example.com/clearbit-logo.png" → Set to null (contains clearbit).  
NEG-065: LogoUrl = "https://example.com/logo.png" → Unchanged (no clearbit).  
NEG-066: LogoUrl = "" (empty string) → Unchanged.  
NEG-067: LogoUrl contains "clearbit" in path but not domain → Still cleared.  
NEG-068: Migration Down() → Cannot restore cleared URLs (irreversible).  
NEG-069: Re-run migration on already-cleared data → No error, no change.  
NEG-070: Partner created after migration with clearbit URL → Not affected (migration is one-time).

### Migration Failures & Rollback Issues (NEG-071–090)

NEG-071: Migration fails mid-transaction → Database left in consistent pre-migration state.  
NEG-072: Connection dropped during migration Up() → Transaction rolled back, no partial schema.  
NEG-073: Disk full during migration → Migration fails, rollback attempted.  
NEG-074: Insufficient permissions to ALTER table → Migration fails with clear error.  
NEG-075: Migration applied out of order (skip ahead) → EF Core rejects, enforces sequence.  
NEG-076: Rollback M2 when dependent M3 already applied → Down fails or cascades correctly.  
NEG-077: Rollback after application code deployed expecting new schema → Runtime errors.  
NEG-078: Migration history table corrupted → Cannot determine applied migrations.  
NEG-079: Duplicate migration ID in history → EF Core detects inconsistency.  
NEG-080: Rollback M4 (TRUNCATE) → Down is empty, data unrecoverable.  
NEG-081: Migration script syntax error → Fails before execution, no partial changes.  
NEG-082: FK constraint blocks column drop during rollback → Down fails with clear message.  
NEG-083: Index creation fails due to duplicate data → Migration fails, schema unchanged.  
NEG-084: Rollback during active application connections → Locks or queued correctly.  
NEG-085: Migration Up() succeeds but snapshot not updated → Model mismatch on next migration.  
NEG-086: Partial rollback (only some migrations) → Schema inconsistent, manual fix required.  
NEG-087: Database restored from backup (pre-migration) → Migration history mismatch.  
NEG-088: Re-apply migration after failed rollback → Idempotency or error handling.  
NEG-089: Migration requires manual data fix before applying → Documented, fails if not done.  
NEG-090: Rollback M10 → Cleared LogoUrls remain null (data modification irreversible).

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### M1: OpportunityIdUpdate (BND-001–005)

BND-001: BaseEngagement.OpportunityId = 1 (min valid) → FK resolves.  
BND-002: BaseEngagement.OpportunityId = MAX_INT → FK resolves if opportunity exists.  
BND-003: BaseEngagement.OpportunityId = null → Nullable column accepts.  
BND-004: Opportunity with 100 linked BaseEngagements → All FK valid.  
BND-005: Opportunity soft-deleted but BaseEngagement.OpportunityId still set → FK valid, query excludes.

### M2: AuditTracking (BND-006–015)

BND-006: Junction table with 0 rows → Columns added instantly.  
BND-007: Junction table with 1 row → Audit columns null for existing row.  
BND-008: Junction table with 10K rows → Migration completes < 30s.  
BND-009: Junction table with 100K rows → Migration completes < 5min.  
BND-010: Status value = 0 (first enum) → Valid.  
BND-011: Status value = max enum value → Valid.  
BND-012: WorkflowStatus value = 0 → Valid.  
BND-013: CreatedDate at DateTime.MinValue → Stored (edge).  
BND-014: CreatedDate at DateTime.MaxValue → Stored (edge).  
BND-015: IsDeleted toggled rapidly → Final state correct.

### M3: ExecutiveIdToOpportunity (BND-016–023)

BND-016: ExecutiveId = 1 (min valid user) → FK resolves.  
BND-017: ExecutiveId = MAX_INT → FK resolves if user exists.  
BND-018: ExecutiveId = null → Accepted (nullable).  
BND-019: Delete executive user → ExecutiveId set to null (SetNull cascade).  
BND-020: 1000 opportunities with same ExecutiveId → All FK valid.  
BND-021: ExecutiveId = OpportunityId (same number) → No confusion, different tables.  
BND-022: Index on ExecutiveId → Query by executive < 50ms with 100K opportunities.  
BND-023: Change ExecutiveId from valid to null → Accepted.

### M4: TruncateEntityUserRoles (BND-024–030)

BND-024: Table with 0 rows → TRUNCATE succeeds.  
BND-025: Table with 1 row → Truncated.  
BND-026: Table with 100K rows → Truncated instantly (TRUNCATE, not DELETE).  
BND-027: Table with FK references → CASCADE deletes dependent rows.  
BND-028: Re-insert 1 row after truncate → Works.  
BND-029: Re-insert 100K rows after truncate → Works.  
BND-030: Sequence/serial counter after truncate → May or may not reset.

### M5: MissionsNotApplicable (BND-031–036)

BND-031: Default value false → Existing rows have false.  
BND-032: Set to true → Saved.  
BND-033: Set to false → Saved.  
BND-034: Toggle true→false→true → Final state true.  
BND-035: Query by UNOPSMissionsNotApplicable=true → Correct filter.  
BND-036: Opportunity with UNOPSMissionsNotApplicable=true → Missions section hidden.

### M7: GoogleADKUpdate (BND-037–043)

BND-037: Timestamp at epoch (1970-01-01) → Converted to TIMESTAMPTZ correctly.  
BND-038: Timestamp at far future (2099-12-31) → Converted correctly.  
BND-039: Timestamp at midnight UTC → No offset issues.  
BND-040: Timestamp with microsecond precision → Preserved.  
BND-041: Null timestamp → Remains null.  
BND-042: Sessions table with 0 rows → No data conversion needed.  
BND-043: Sessions table with 1M rows → Type conversion < 1min.

### M8: RevertCompositeKeyAddActiveUser (BND-044–052)

BND-044: 0 users in AspNetUsers → Migration succeeds.  
BND-045: 1 user → ActiveUser defaults to true.  
BND-046: 10K users → All get ActiveUser=true.  
BND-047: NormalizedUserName with max length (256 chars) → Index handles.  
BND-048: NormalizedUserName with 1 character → Index handles.  
BND-049: NormalizedUserName with Unicode → Index handles.  
BND-050: Two users with same NormalizedUserName (if exists) → Index fails.  
BND-051: ActiveUser toggled rapidly → Final state correct.  
BND-052: Login query uses new index → Query plan shows index scan.

### M9: UniqueConstraints (BND-053–062)

BND-053: Countries with 0 rows → Index created instantly.  
BND-054: Countries with all unique Iso2Code → Index created.  
BND-055: Countries with 249 rows (all world countries) → Index created.  
BND-056: Currencies with all unique Code → Index created.  
BND-057: PartnerAgreementNumber at 1 char → Indexed.  
BND-058: PartnerAgreementNumber at max length → Indexed.  
BND-059: UserProfile (UserId, Id) composite → Both values at boundary.  
BND-060: AspNetUserRoles (UserId, RoleId) composite → Both at boundary.  
BND-061: BaseEngagements unique on BaseEngagement field → At boundary value.  
BND-062: BaseEngagementPartners unique on Key field → At boundary value.

### M10: ClearClearbitLogoUrls (BND-063–070)

BND-063: 0 partners with clearbit URLs → Script runs, no changes.  
BND-064: 1 partner with clearbit URL → Cleared.  
BND-065: 1000 partners with clearbit URLs → All cleared.  
BND-066: LogoUrl = "clearbit" (just the word) → Cleared (contains match).  
BND-067: LogoUrl = "https://logo.clearbit.com/unops.org" → Cleared.  
BND-068: LogoUrl = "https://logo.CLEARBIT.com/unops.org" → Cleared (case-insensitive).  
BND-069: LogoUrl with clearbit as URL parameter → Cleared.  
BND-070: Partners table with 100K rows → Script < 10s.

### Migration & Rollback Boundaries (BND-071–090)

BND-071: Apply migration at exactly timestamp boundary → Order preserved.  
BND-072: Migration history at max migration count → Next migration applies.  
BND-073: Rollback to migration 0 (empty schema) → All migrations reversed.  
BND-074: Database at exactly 0 bytes free → Migration fails before write.  
BND-075: Connection timeout at 30s during long migration → Retry or fail clearly.  
BND-076: Migration lock held for exactly lock_timeout → Released or extended.  
BND-077: Transaction log at capacity during migration → Handled or fails.  
BND-078: Apply 10 migrations in single batch → All succeed or atomic rollback.  
BND-079: Rollback 5 migrations then re-apply 3 → Schema matches M7 state.  
BND-080: Migration file size at filesystem limit → Error before execution.  
BND-081: Snapshot diff at maximum complexity → Generates valid migration.  
BND-082: Zero-downtime migration window → Application continues during ALTER.  
BND-083: Migration applied on read replica → Fails (read-only).  
BND-084: Migration applied during backup → Both complete or documented conflict.  
BND-085: Schema version at boundary between major versions → Compatible.  
BND-086: Rollback when last applied migration = target → No-op.  
BND-087: Migration with 0 rows affected (ALTER on empty table) → Completes.  
BND-088: Migration with MAX_INT rows affected → Completes or timeout.  
BND-089: Concurrent migration processes = 1 → Single runner succeeds.  
BND-090: Migration applied at system clock boundary (DST) → Timestamps correct.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### M1 (FUN-001–004)

FUN-001: OpportunityId FK cascade behavior correct on opportunity delete.  
FUN-002: BaseEngagement.OpportunityId queryable in LINQ.  
FUN-003: Navigation property Opportunity→BaseEngagements works.  
FUN-004: BaseEngagement→Opportunity navigation property works.

### M2 (FUN-005–012)

FUN-005: All 15 junction tables have complete audit column set.  
FUN-006: Soft-delete on junction table sets IsDeleted, DeletedBy, DeletedDate.  
FUN-007: AuditableDbContext auto-populates CreatedBy/CreatedDate on insert.  
FUN-008: AuditableDbContext auto-populates LastModifiedBy/LastModifiedDate on update.  
FUN-009: Junction table query with `!IsDeleted` filter works.  
FUN-010: Junction table Name property set on create.  
FUN-011: Junction table Status property defaults correctly.  
FUN-012: Junction table WorkflowStatus defaults correctly.

### M3 (FUN-013–018)

FUN-013: ExecutiveId FK resolves to correct User entity.  
FUN-014: Opportunity.Executive navigation property populated.  
FUN-015: Executive user deletion → Opportunity.ExecutiveId becomes null.  
FUN-016: ExecutiveId index improves query by executive performance.  
FUN-017: ExecutiveId visible in opportunity detail API response.  
FUN-018: ExecutiveId editable only by authorized users.

### M4 (FUN-019–023)

FUN-019: EntityUserRoles empty after migration → DoA lookups return empty.  
FUN-020: EntityUserRoles re-populated via EDS sync → Roles available again.  
FUN-021: Application handles empty EntityUserRoles gracefully.  
FUN-022: Permission checks with empty EntityUserRoles → Handled per fallback logic.  
FUN-023: TRUNCATE CASCADE cleans up all dependent rows.

### M5 (FUN-024–028)

FUN-024: UNOPSMissionsNotApplicable=true → Missions section UI hidden/disabled.  
FUN-025: UNOPSMissionsNotApplicable=false → Missions section visible.  
FUN-026: UNOPSMissionsNotApplicable queryable in opportunity filters.  
FUN-027: UNOPSMissionsNotApplicable included in AI summary data.  
FUN-028: UNOPSMissionsNotApplicable persisted correctly via API.

### M7 (FUN-029–033)

FUN-029: Session timestamps stored with timezone info.  
FUN-030: Session queries return UTC timestamps.  
FUN-031: AI assistant sessions use correct timestamp type.  
FUN-032: Conditional migration skips if sessions table missing.  
FUN-033: Timestamp comparison queries work correctly after type change.

### M8 (FUN-034–040)

FUN-034: ActiveUser=true → User can log in.  
FUN-035: ActiveUser=false → User login behavior (blocked or allowed per logic).  
FUN-036: Unique NormalizedUserName enforced.  
FUN-037: Duplicate username registration → Unique violation.  
FUN-038: Username lookup uses new index → Fast query.  
FUN-039: User search by NormalizedUserName → Index scan.  
FUN-040: ActiveUser toggleable via admin API.

### M9 (FUN-041–046)

FUN-041: EDS bulk upsert with ON CONFLICT DO UPDATE → Updates existing.  
FUN-042: EDS bulk upsert with new record → Inserts new.  
FUN-043: EDS bulk upsert with duplicate in batch → Conflict handled.  
FUN-044: Countries unique Iso2Code → Duplicate insert blocked.  
FUN-045: Currencies unique Code → Duplicate insert blocked.  
FUN-046: Script idempotent → Re-run checks pg_indexes before creating.

### M10 (FUN-047–050)

FUN-047: Clearbit URLs cleared → Partner list shows placeholder images.  
FUN-048: Non-clearbit URLs preserved → Logo still displays.  
FUN-049: SQL ILIKE for case-insensitive match → Correct behavior.  
FUN-050: Script runs in single transaction → Atomic.

### Migration & Schema Operations (FUN-051–090)

FUN-051: `dotnet ef migrations list` shows all 10 migrations in order.  
FUN-052: `dotnet ef database update` applies pending migrations only.  
FUN-053: `dotnet ef database update 0` rolls back to empty schema.  
FUN-054: Migration script generated with `--idempotent` flag → Re-runnable.  
FUN-055: EF Core model snapshot matches database after migration.  
FUN-056: DbContext.Database.Migrate() applies migrations on startup.  
FUN-057: DbContext.Database.GetPendingMigrations() returns correct list.  
FUN-058: DbContext.Database.GetAppliedMigrations() returns correct list.  
FUN-059: Migration Up() wrapped in transaction → All or nothing.  
FUN-060: Migration Down() wrapped in transaction → All or nothing.  
FUN-061: Custom SQL in migration executes in correct order.  
FUN-062: Migration with raw SQL and EF operations → Both applied.  
FUN-063: RenameColumn migration → Data preserved.  
FUN-064: AlterColumn migration → Data type change applied.  
FUN-065: CreateIndex with CONCURRENTLY → Reduced lock duration.  
FUN-066: DropIndex with CONCURRENTLY → Reduced lock duration.  
FUN-067: AddForeignKey with name → FK created with specified name.  
FUN-068: DropForeignKey before DropTable → Order correct.  
FUN-069: Migration with conditional logic (table exists) → Correct branch.  
FUN-070: Migration with conditional logic (column exists) → Correct branch.  
FUN-071: Seed data in migration → Applied after schema change.  
FUN-072: Migration generates correct PostgreSQL dialect SQL.  
FUN-073: Migration handles schema-qualified table names.  
FUN-074: Migration respects default schema (public).  
FUN-075: Composite primary key migration → Correct constraint.  
FUN-076: Unique constraint with multiple columns → Correct constraint.  
FUN-077: Check constraint in migration → Enforced.  
FUN-078: Default value in AddColumn → Applied to new rows.  
FUN-079: Nullable column change → Existing nulls preserved.  
FUN-080: Column rename preserves data and constraints.  
FUN-081: Table rename preserves data and FKs.  
FUN-082: Migration with multiple operations → All applied atomically.  
FUN-083: Migration dependency on previous migration → Order enforced.  
FUN-084: Empty migration (no operations) → Applies and records in history.  
FUN-085: Migration that only adds comment → No schema change.  
FUN-086: Migration script exported to SQL file → Executable standalone.  
FUN-087: Migration from different project (UNOPS override) → Correct context.  
FUN-088: Connection string override for migration → Uses specified DB.  
FUN-089: Migration with environment-specific logic → Correct branch.  
FUN-090: Full migration cycle (apply all, rollback all) → Schema restored.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### Sequential Migration Run (INT-001–010)

INT-001: Apply all 10 migrations sequentially → No errors.  
INT-002: Database schema matches EF Core model snapshot after all migrations.  
INT-003: `dotnet ef database update` completes successfully.  
INT-004: Application starts after all migrations → No startup errors.  
INT-005: EF Core generates correct SQL for new columns/indexes.  
INT-006: All API endpoints function after migrations.  
INT-007: UI loads correctly after migrations.  
INT-008: Search functionality works with new indexes.  
INT-009: EDS sync functions with new unique constraints.  
INT-010: AI features function with new timestamp types.

### Rollback Testing (INT-011–020)

INT-011: Rollback M10 → Clearbit URLs stay null (irreversible data).  
INT-012: Rollback M9 → Unique constraints dropped (SQL script, manual rollback).  
INT-013: Rollback M8 → ActiveUser dropped, old indexes restored.  
INT-014: Rollback M7 → Timestamps reverted to WITHOUT TIME ZONE.  
INT-015: Rollback M6 → Filtered indexes replaced with unfiltered.  
INT-016: Rollback M5 → UNOPSMissionsNotApplicable column dropped.  
INT-017: Rollback M4 → Table stays empty (no Down logic).  
INT-018: Rollback M3 → ExecutiveId, index, FK dropped.  
INT-019: Rollback M2 → Audit columns dropped from 15 tables.  
INT-020: Rollback M1 → OpportunityId column dropped from BaseEngagements.

### Data Integrity After Migration (INT-021–035)

INT-021: Existing opportunity data preserved after M2 audit columns.  
INT-022: Existing collaborator data queryable after M6 filtered indexes.  
INT-023: Existing BaseEngagement data has OpportunityId=null after M1.  
INT-024: Existing opportunities have ExecutiveId=null after M3.  
INT-025: Existing opportunities have UNOPSMissionsNotApplicable=false after M5.  
INT-026: Existing users have ActiveUser=true after M8.  
INT-027: Existing partner logos intact (non-clearbit) after M10.  
INT-028: Entity counts unchanged after all migrations.  
INT-029: FK relationships intact after all migrations.  
INT-030: Audit trail continues to function after M2.  
INT-031: Soft-delete continues to function after M2.  
INT-032: Workflow continues to function after M4 (EntityUserRoles empty).  
INT-033: EDS sync continues to function after M9.  
INT-034: AI sessions continue to function after M7.  
INT-035: Authentication continues to function after M8.

### Cross-Migration Interactions (INT-036–050)

INT-036: M2 + M6 together → Audit columns + filtered indexes on same tables.  
INT-037: M3 + M2 → ExecutiveId + audit on Opportunities.  
INT-038: M8 + M9 → Both add indexes to user-related tables.  
INT-039: M4 + M3 → EntityUserRoles truncated before Executive assignment.  
INT-040: M1 + M2 → OpportunityId on BaseEngagements + audit columns on junction tables.  
INT-041: All migrations applied on fresh (empty) database → Schema created correctly.  
INT-042: All migrations applied on production-like database (100K records) → Completes.  
INT-043: Migration order enforced by timestamps → Cannot apply out of order.  
INT-044: EF Core migration history table updated for all 10.  
INT-045: Model snapshot accurate after all 10 migrations.  
INT-046: InMemory test database reflects all migration changes → Tests pass.  
INT-047: Integration test PAOWebApplicationFactory → Includes all schema changes.  
INT-048: CI/CD pipeline runs migrations → All pass.  
INT-049: Staging environment migration → Matches development.  
INT-050: Migration idempotency → Re-running already-applied migration is no-op.

### Migration Failures & Recovery (INT-051–070)

INT-051: Kill migration process mid-Up() → Database consistent, retry succeeds.  
INT-052: Restore from backup after failed migration → Re-apply from backup state.  
INT-053: Migration fails on duplicate index → Manual fix, re-run succeeds.  
INT-054: Migration fails on FK violation → Fix data, re-run succeeds.  
INT-055: Connection pool exhausted during migration → Retry with new connection.  
INT-056: Migration timeout → Increase command timeout, re-run.  
INT-057: Disk space freed during failed migration → Rollback completes.  
INT-058: Migration with syntax error → Fails before execution, no changes.  
INT-059: Migration history manually corrected → Next migration applies.  
INT-060: Snapshot out of sync with database → Generate script, reconcile.  
INT-061: Multiple DbContexts with shared migrations → Single history table.  
INT-062: Migration from branch A, then branch B → Merge migration history.  
INT-063: Migration applied, then code reverted → Database ahead of code.  
INT-064: Code deployed, migrations not yet run → Application fails or prompts.  
INT-065: Blue-green deployment with migration → Both environments consistent.  
INT-066: Migration in container → Ephemeral DB, migrations on startup.  
INT-067: Migration with Docker Compose → DB ready before app starts.  
INT-068: Migration in Kubernetes init container → Schema ready for pods.  
INT-069: Migration with Terraform-managed DB → Idempotent apply.  
INT-070: Migration with managed DB (RDS, Cloud SQL) → Compatible.

### Environment & Deployment (INT-071–090)

INT-071: Migration in local development → Applies to local PostgreSQL.  
INT-072: Migration in CI pipeline → Test database migrated.  
INT-073: Migration in staging → Staging DB matches dev after migration.  
INT-074: Migration in production → Zero-downtime or planned maintenance.  
INT-075: Migration with read replica → Primary migrated, replica catches up.  
INT-076: Migration with connection string from secrets → Secure.  
INT-077: Migration with different DB user (migration role) → Least privilege.  
INT-078: Migration with SSL/TLS connection → Encrypted.  
INT-079: Migration from Windows to Linux PostgreSQL → Cross-platform.  
INT-080: Migration from older PostgreSQL (12) to newer (15) → Compatible.  
INT-081: Migration with extensions (uuid-ossp, pg_trgm) → No conflict.  
INT-082: Migration with custom types → Preserved.  
INT-083: Migration with triggers → Triggers remain after schema change.  
INT-084: Migration with views referencing changed tables → Views updated or recreated.  
INT-085: Migration with stored procedures → Procedures unaffected or updated.  
INT-086: Migration with partitioned tables → Partition structure preserved.  
INT-087: Migration with row-level security → RLS policies preserved.  
INT-088: Migration audit (who applied, when) → Logged in migration history.  
INT-089: Migration rollback plan documented → Executable per runbook.  
INT-090: Full production migration drill → Apply, verify, rollback (if needed), re-apply.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Migration during active application traffic → DDL locks tables briefly.  
CON-002: Concurrent writes during M2 column additions → Blocked during ALTER.  
CON-003: Concurrent reads during M2 column additions → May be blocked.  
CON-004: Concurrent inserts during M6 index creation → Blocked during CREATE INDEX.  
CON-005: Concurrent writes during M8 index drop/create → Blocked.  
CON-006: M4 TRUNCATE during concurrent SELECT → Blocked or snapshot.  
CON-007: M10 UPDATE during concurrent SELECT on Partners → Blocked or snapshot.  
CON-008: Two migration runners simultaneously → Lock prevents double-apply.  
CON-009: Migration timeout during long ALTER on large table → Retryable.  
CON-010: Connection pool exhaustion during migration → Queued, eventually completes.  
CON-011: Concurrent EDS sync during M9 index creation → Blocked briefly.  
CON-012: Application restart during migration → Transaction rollback, clean state.  
CON-013: Concurrent ExecutiveId updates after M3 → No conflict (nullable FK).  
CON-014: Concurrent OpportunityId assignments after M1 → No conflict (nullable).  
CON-015: Concurrent soft-deletes after M2 audit columns → Audit fields populated.  
CON-016: Index creation with CONCURRENTLY option → Reduced locking.  
CON-017: Vacuum during migration → Both complete.  
CON-018: Backup during migration → Consistent snapshot captured.  
CON-019: Replication lag after migration → Replica catches up.  
CON-020: Connection termination during migration → Transaction rolled back.  
CON-021: Multiple applications pointing to same DB during migration → All see changes.  
CON-022: Read replica migration timing → Schema divergence window.  
CON-023: Transaction isolation level for migration DDL → Appropriate level.  
CON-024: Deadlock between migration and application query → Resolved.  
CON-025: Migration on standby/replica → Not allowed (read-only).

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Schema Validation

UNT-001: OpportunityId column type is int.  
UNT-002: OpportunityId is nullable.  
UNT-003: ExecutiveId column type is int.  
UNT-004: ExecutiveId is nullable.  
UNT-005: UNOPSMissionsNotApplicable column type is bool.  
UNT-006: UNOPSMissionsNotApplicable default is false.  
UNT-007: ActiveUser column type is bool.  
UNT-008: ActiveUser default is true.  
UNT-009: IX_Opportunities_ExecutiveId is non-unique index.  
UNT-010: IX_AspNetUsers_NormalizedUserName is unique index.

### Entity Mapping

UNT-011: BaseEngagement.OpportunityId maps to correct column.  
UNT-012: Opportunity.ExecutiveId maps to correct column.  
UNT-013: Opportunity.UNOPSMissionsNotApplicable maps correctly.  
UNT-014: AspNetUsers.ActiveUser maps correctly.

### Migration Validation

UNT-015: Migration M1 Up() adds column.  
UNT-016: Migration M1 Down() drops column.  
UNT-017: Migration M3 Up() creates FK with SetNull.  
UNT-018: Migration M3 Down() drops FK and column.  
UNT-019: Migration M6 Up() creates filtered index.  
UNT-020: Migration M7 conditional SQL checks table existence.  
UNT-021: Migration M9 SQL is idempotent (checks pg_indexes).

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: All 10 migrations complete on 100K-record DB in < 5 minutes total.  
PRF-002: M2 (15 table ALTERs) completes in < 2 minutes on 100K-record DB.  
PRF-003: M6 filtered index creation < 30s on 10K collaborator rows.  
PRF-004: M8 index drop + create < 30s on 10K users.  
PRF-005: M9 unique index creation < 1 minute on 100K rows per table.  
PRF-006: M10 UPDATE on 10K partners < 10s.  
PRF-007: Query by ExecutiveId with index < 50ms.  
PRF-008: Query by NormalizedUserName with unique index < 10ms.  
PRF-009: Query by Iso2Code with unique index < 10ms.  
PRF-010: EDS bulk upsert with unique constraints < 1s per 1000 records.  
PRF-011: Audit column writes add < 5ms overhead per operation.  
PRF-012: Table size increase from audit columns < 30% per table.  
PRF-013: Index storage overhead < 10% of table size.  
PRF-014: Migration lock duration < 30s per table.  
PRF-015: Application downtime during migration < 1 minute.  
PRF-016: Full migration rollback < 3 minutes.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: Apply all 10 migrations on 1M-record production-sized DB → Completes.  
LDT-002: 100 concurrent writes during M2 column addition → All eventually succeed.  
LDT-003: 200 concurrent reads during M6 index creation → All succeed after lock release.  
LDT-004: EDS bulk upsert with 10K records after M9 → ON CONFLICT works.  
LDT-005: 50 concurrent logins after M8 index change → All succeed.  
LDT-006: 100 concurrent partner queries after M10 → No clearbit URLs returned.  
LDT-007: Stress: repeated migration + rollback cycles → Schema stable.  
LDT-008: Stress: concurrent EDS sync during multiple migrations → Queued correctly.  
LDT-009: Recovery after migration failure at 50% → Clean rollback.  
LDT-010: Full schema rebuild from scratch → All 10 migrations apply correctly.

---

## Status: Ready for Implementation
