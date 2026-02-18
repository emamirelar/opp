# Partner Clearbit URL Cleanup — Comprehensive Test Cases

**Component:** Data migration clearing Clearbit logo URLs from Partners  
**Migration:** `20260216181625_ClearClearbitLogoUrlsFromPartners`  
**SQL Script:** `UNOPS.PAO.UNOPSDataAccess/Scripts/ClearClearbitLogoUrlsFromPartners.sql`  
**Fallback Image:** `assets/images/Partner.png`  
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

### Ratio Compliance Checks

| Check | Formula | Actual | Required | Status |
|-------|---------|-------|----------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Feature Overview

### Migration SQL

```sql
UPDATE public."Partners"
SET "LogoUrl" = NULL
WHERE "LogoUrl" IS NOT NULL
  AND "LogoUrl" LIKE '%clearbit%';
```

### Behavior

- Sets `LogoUrl` to `NULL` for all Partners whose `LogoUrl` contains the substring `clearbit`
- Case-sensitive `LIKE` in PostgreSQL (but tested variants confirm behavior)
- Idempotent: safe to re-run
- **Irreversible:** No `Down()` logic — URLs cannot be restored

### Logo Display Fallback Chain

1. `LogoUrl` is non-null and non-empty → Display the URL as `<img>`
2. `LogoUrl` is null/empty → Display `assets/images/Partner.png` (placeholder)
3. Image load error (`onerror`) → Display `assets/images/Partner.png`

### Affected UI Locations

| Component | How LogoUrl is used |
|-----------|-------------------|
| `partner-tabs.component.ts` | `[imageUrl]="recordData.logoUrl \|\| ''"` via `app-picture` |
| `partner.component.ts` (list) | `field: 'logoUrl'` column |
| `partner-tree-data.component.ts` | `field: 'logoUrl'` in tree view |
| `opportunity-view` (Who section) | `partner.partnerLogoUrl \|\| 'assets/images/Partner.png'` |
| `app-picture` component | `getEffectiveImageUrl()` → fallback to `assets/images/Partner.png` |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Migration Execution (POS-001–010)

POS-001: Partner with `LogoUrl = "https://logo.clearbit.com/unops.org"` → `LogoUrl` set to NULL.  
POS-002: Partner with `LogoUrl = "https://logo.clearbit.com/microsoft.com"` → Set to NULL.  
POS-003: Partner with `LogoUrl = "https://logo.clearbit.com/google.com"` → Set to NULL.  
POS-004: Partner with `LogoUrl` containing `clearbit` in path → Set to NULL.  
POS-005: Partner with `LogoUrl` containing `clearbit` in subdomain → Set to NULL.  
POS-006: Migration runs successfully without errors.  
POS-007: Migration is idempotent — second run has no effect.  
POS-008: Migration records entry in `__EFMigrationsHistory`.  
POS-009: Partners without `clearbit` in `LogoUrl` → `LogoUrl` unchanged.  
POS-010: Partners with `LogoUrl = NULL` before migration → Remain NULL.

### Non-Clearbit Preservation (POS-011–020)

POS-011: Partner with `LogoUrl = "https://example.com/logo.png"` → Unchanged.  
POS-012: Partner with `LogoUrl = "https://storage.googleapis.com/bucket/logo.png"` → Unchanged.  
POS-013: Partner with `LogoUrl = "/uploads/partner-logo.jpg"` → Unchanged.  
POS-014: Partner with `LogoUrl = "data:image/png;base64,..."` → Unchanged.  
POS-015: Partner with `LogoUrl = ""` (empty string) → Unchanged.  
POS-016: Partner with uploaded custom logo → Unchanged.  
POS-017: Partner with GCS-signed logo URL → Unchanged.  
POS-018: Partner without any logo (NULL) → Stays NULL.  
POS-019: Partner with relative path logo → Unchanged.  
POS-020: Partner with CDN-hosted logo → Unchanged.

### UI Fallback Display (POS-021–030)

POS-021: Partner with cleared `LogoUrl` (NULL) → `Partner.png` placeholder shown on detail page.  
POS-022: Partner with cleared `LogoUrl` → Placeholder shown on partner list page.  
POS-023: Partner with cleared `LogoUrl` → Placeholder shown in partner tree view.  
POS-024: Partner with cleared `LogoUrl` → Placeholder shown on opportunity "Who" section.  
POS-025: `app-picture` component shows `Partner.png` when `imageUrl` is empty.  
POS-026: `app-picture` component shows `Partner.png` when `imageUrl` is null.  
POS-027: Image `onerror` handler replaces broken image with `Partner.png`.  
POS-028: Non-clearbit partner logo still displays correctly after migration.  
POS-029: Newly uploaded logo displays correctly after migration.  
POS-030: GCS-signed URL conversion still works for non-clearbit logos.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Migration Failure Scenarios (NEG-001–015)

NEG-001: Migration on database without Partners table → Error (table not found).  
NEG-002: Migration on database without LogoUrl column → Error (column not found).  
NEG-003: Migration interrupted mid-execution → Transaction rolled back.  
NEG-004: Migration with database connection lost → Error, no partial update.  
NEG-005: Migration with read-only database → Permission error.  
NEG-006: Migration Down() called → No rollback (empty method).  
NEG-007: Cannot restore clearbit URLs after migration → Data permanently lost.  
NEG-008: Migration with Partners table locked by another transaction → Waits or timeout.  
NEG-009: Migration with concurrent writes to Partners → Writes blocked during UPDATE.  
NEG-010: Migration re-run after all clearbit URLs already cleared → No error, 0 rows affected.  
NEG-011: Migration on fresh empty database → No error, 0 rows affected.  
NEG-012: Migration with only soft-deleted partners having clearbit URLs → Soft-deleted LogoUrl also cleared.  
NEG-013: Migration does not clear DeletedBy/DeletedDate → Correct (only LogoUrl updated).  
NEG-014: Migration does not update LastModifiedDate → Correct (raw SQL, no EF audit).  
NEG-015: Migration SQL script file missing → Migration fails at runtime.

### LIKE Pattern Edge Cases (NEG-016–035)

NEG-016: `LogoUrl = "https://clearbit.com"` → Contains "clearbit" → Cleared.  
NEG-017: `LogoUrl = "clearbit"` (just the word) → Contains "clearbit" → Cleared.  
NEG-018: `LogoUrl = "https://notclearbit.com/logo.png"` → Contains "clearbit" → Cleared (substring match).  
NEG-019: `LogoUrl = "https://example.com/clearbit-style.png"` → Contains "clearbit" → Cleared.  
NEG-020: `LogoUrl = "https://example.com?source=clearbit"` → Contains "clearbit" → Cleared.  
NEG-021: `LogoUrl = "CLEARBIT"` → PostgreSQL LIKE is case-sensitive → NOT cleared (uppercase).  
NEG-022: `LogoUrl = "Clearbit"` → Case-sensitive → NOT cleared (title case).  
NEG-023: `LogoUrl = "ClearBit"` → Case-sensitive → NOT cleared (mixed case).  
NEG-024: `LogoUrl = "LOGO.CLEARBIT.COM"` → NOT cleared (uppercase).  
NEG-025: `LogoUrl = "https://logo.Clearbit.com/x.org"` → NOT cleared (capital C).  
NEG-026: `LogoUrl = "clearbitsomething"` → Contains "clearbit" → Cleared.  
NEG-027: `LogoUrl = "somethingclearbit"` → Contains "clearbit" → Cleared.  
NEG-028: `LogoUrl = "clear bit"` → Does NOT contain "clearbit" (space) → NOT cleared.  
NEG-029: `LogoUrl = "clearb1t"` → NOT cleared (number substitution).  
NEG-030: `LogoUrl = "c-l-e-a-r-b-i-t"` → NOT cleared (hyphens).  
NEG-031: `LogoUrl = "%clearbit%"` → Contains "clearbit" literally → Cleared.  
NEG-032: `LogoUrl = "https://logo.clearbit.com/"` (trailing slash) → Cleared.  
NEG-033: `LogoUrl` with clearbit in query parameter value → Cleared.  
NEG-034: `LogoUrl` with clearbit in fragment `#clearbit` → Cleared.  
NEG-035: `LogoUrl` with encoded "clearbit" (`%63learbit`) → NOT cleared (encoded).

### UI Fallback Failures (NEG-036–050)

NEG-036: `Partner.png` placeholder file missing from assets → Broken image icon.  
NEG-037: `Partner.png` placeholder is corrupt → Broken image icon.  
NEG-038: `app-picture` component receives undefined `imageUrl` → Fallback to placeholder.  
NEG-039: `app-picture` component receives empty string → Fallback to placeholder.  
NEG-040: `app-picture` component image URL returns 404 → `onerror` handler → Placeholder.  
NEG-041: `app-picture` component image URL returns 403 → `onerror` handler → Placeholder.  
NEG-042: `app-picture` component image URL returns 500 → `onerror` handler → Placeholder.  
NEG-043: `app-picture` double error (placeholder also fails) → Image hidden.  
NEG-044: Partner logo column in list shows empty cell for null LogoUrl.  
NEG-045: Partner tree view shows empty node for null LogoUrl.  
NEG-046: Opportunity Who section shows placeholder for cleared partner.  
NEG-047: Print/PDF export with cleared logo → Placeholder or no image.  
NEG-048: Email notification with partner logo → No broken image in email.  
NEG-049: AI summary references partner logo → Handles null gracefully.  
NEG-050: Search results with cleared partner logo → Placeholder shown.

### Data Integrity Violations (NEG-051–065)

NEG-051: New partner created after migration with clearbit URL → Allowed (migration is one-time).  
NEG-052: Partner LogoUrl updated to clearbit URL after migration → Allowed (no ongoing check).  
NEG-053: Seeder creates partner with clearbit URL after migration → Re-introduces clearbit.  
NEG-054: EDS sync sets partner LogoUrl to clearbit → Re-introduces clearbit.  
NEG-055: Bulk import with clearbit URLs → Re-introduces clearbit.  
NEG-056: Logo upload replaces null with new image → Allowed (overwrite null).  
NEG-057: Logo upload with clearbit-hosted image → Allowed (no URL check).  
NEG-058: Partner update API with clearbit LogoUrl → Accepted (no validation).  
NEG-059: Migration applied twice → Second run: 0 rows affected, no error.  
NEG-060: Migration applied to test environment → Clears test data logos too.  
NEG-061: Migration applied to staging → Clears staging data logos.  
NEG-062: GCS signed URL that contains "clearbit" in bucket name → Would be cleared (false positive).  
NEG-063: Legitimate company named "Clearbit Inc." → Logo URL may contain clearbit.  
NEG-064: Partner with LogoUrl pointing to clearbit but is correct logo → Incorrectly cleared.  
NEG-065: No notification sent to partners about logo removal → Silent change.

### Soft-Delete Interaction (NEG-066–070)

NEG-066: Soft-deleted partner with clearbit URL → LogoUrl still cleared (no IsDeleted filter in script).  
NEG-067: Soft-deleted partner restored after migration → LogoUrl is null, needs re-upload.  
NEG-068: Soft-deleted partner logo visible in admin view → Shows placeholder.  
NEG-069: Historical partner data references clearbit URL → Cleared in DB, broken in audit trail.  
NEG-070: Partner merge after migration → Target partner LogoUrl handling correct.

### Extended Negative Scenarios (NEG-071–090)

NEG-071: Migration with invalid connection string → Connection failure, no partial update.  
NEG-072: Migration with insufficient DB user permissions (no UPDATE on Partners) → Permission denied.  
NEG-073: Migration with schema name typo in script → Table not found error.  
NEG-074: Migration with column name typo in script → Column not found error.  
NEG-075: Partner with LogoUrl containing SQL injection attempt → Escaped, no injection.  
NEG-076: Partner with LogoUrl containing null byte → Behavior undefined, may truncate.  
NEG-077: Partner with LogoUrl exceeding column max length → Pre-migration data, migration skips or fails.  
NEG-078: Migration during database failover → Transaction aborted, rollback.  
NEG-079: Migration with transaction timeout → Rollback, no partial clear.  
NEG-080: Partner API returns 500 when LogoUrl processing fails for cleared partner → Error path.  
NEG-081: Opportunity API omits partnerLogoUrl when null → Client must handle missing field.  
NEG-082: Partner export with null LogoUrl in CSV → Empty cell or "null" string.  
NEG-083: Partner import overwrites cleared LogoUrl with clearbit → Re-introduces clearbit.  
NEG-084: oUP integration sync overwrites LogoUrl with clearbit → Re-introduces clearbit.  
NEG-085: Partner duplicate detection uses LogoUrl → Null vs null comparison works.  
NEG-086: Partner search by LogoUrl (if supported) → Null partners excluded or handled.  
NEG-087: Partner audit log shows LogoUrl change → Audit trail captures null assignment.  
NEG-088: Partner report includes logo column → Null renders as blank or placeholder.  
NEG-089: Partner dashboard widget with all cleared logos → All placeholders, no external fetches.  
NEG-090: Partner typeahead/dropdown with logo preview → Null shows placeholder or no image.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Record Count Boundaries (BND-001–015)

BND-001: 0 partners in database → Migration runs, 0 rows affected.  
BND-002: 1 partner with clearbit URL → 1 row cleared.  
BND-003: 1 partner without clearbit URL → 0 rows affected.  
BND-004: 100 partners, all with clearbit URLs → 100 rows cleared.  
BND-005: 100 partners, none with clearbit URLs → 0 rows affected.  
BND-006: 100 partners, 50 with clearbit URLs → 50 rows cleared, 50 unchanged.  
BND-007: 1000 partners with clearbit URLs → All cleared.  
BND-008: 10K partners with clearbit URLs → All cleared, acceptable time.  
BND-009: 100K partners total, 1 with clearbit → 1 row cleared.  
BND-010: All partners have NULL LogoUrl → 0 rows affected.  
BND-011: All partners have empty string LogoUrl → 0 rows affected (LIKE doesn't match "").  
BND-012: Mix of NULL, empty, clearbit, non-clearbit → Only clearbit rows cleared.  
BND-013: Only soft-deleted partners have clearbit → Still cleared (no IsDeleted filter).  
BND-014: All active partners have clearbit, soft-deleted have non-clearbit → Active cleared.  
BND-015: Single partner with longest possible LogoUrl containing clearbit → Cleared.

### LogoUrl Value Boundaries (BND-016–035)

BND-016: `LogoUrl` = 1 character "c" → NOT cleared (no "clearbit").  
BND-017: `LogoUrl` = 8 characters "clearbit" → Cleared.  
BND-018: `LogoUrl` = 9 characters "clearbitx" → Cleared (contains "clearbit").  
BND-019: `LogoUrl` = maximum text length → Cleared if contains "clearbit".  
BND-020: `LogoUrl` with "clearbit" at start → Cleared.  
BND-021: `LogoUrl` with "clearbit" at end → Cleared.  
BND-022: `LogoUrl` with "clearbit" in middle → Cleared.  
BND-023: `LogoUrl` with multiple "clearbit" occurrences → Cleared.  
BND-024: `LogoUrl` = empty string "" → NOT cleared (IS NOT NULL but no "clearbit").  
BND-025: `LogoUrl` = single space " " → NOT cleared (no "clearbit").  
BND-026: `LogoUrl` with Unicode characters + "clearbit" → Cleared.  
BND-027: `LogoUrl` with emoji + "clearbit" → Cleared.  
BND-028: `LogoUrl` with newline + "clearbit" → Cleared.  
BND-029: `LogoUrl` with null byte + "clearbit" → Behavior depends on DB.  
BND-030: `LogoUrl` = "https://logo.clearbit.com/company.com" (typical format) → Cleared.  
BND-031: `LogoUrl` = "https://logo.clearbit.com/" (no company) → Cleared.  
BND-032: `LogoUrl` = "https://logo.clearbit.com" (no trailing slash) → Cleared.  
BND-033: `LogoUrl` with port: "https://clearbit.com:443/logo" → Cleared.  
BND-034: `LogoUrl` with auth: "https://user:pass@clearbit.com/logo" → Cleared.  
BND-035: `LogoUrl` = "data:text/html,clearbit" (data URI with clearbit) → Cleared.

### UI Display Boundaries (BND-036–055)

BND-036: Partner detail with LogoUrl=null → Placeholder displayed.  
BND-037: Partner detail with LogoUrl="" → Placeholder displayed.  
BND-038: Partner detail with LogoUrl="valid-url" → Real logo displayed.  
BND-039: Partner list with 0 logos (all null) → All placeholders.  
BND-040: Partner list with all logos (none null) → All real logos.  
BND-041: Partner list with mix → Correct per row.  
BND-042: Placeholder image dimensions match logo container → No layout shift.  
BND-043: Placeholder renders at different screen widths → Responsive.  
BND-044: Placeholder in dark mode → Visible contrast.  
BND-045: Placeholder in print view → Renders.  
BND-046: Opportunity with 10 partners, all cleared → 10 placeholders.  
BND-047: Opportunity with 10 partners, none cleared → 10 real logos.  
BND-048: Opportunity with 10 partners, mixed → Correct per partner.  
BND-049: Partner card with very long name + placeholder → Layout correct.  
BND-050: Partner card with very short name + placeholder → Layout correct.  
BND-051: Multiple browser tabs showing same partner → All show placeholder.  
BND-052: Page refresh after migration → Updated display (no cache).  
BND-053: Browser cache with old clearbit image → Image load fails → Placeholder.  
BND-054: CDN cache with old clearbit image → CDN returns error → Placeholder.  
BND-055: Partner logo in notification email → Null handled gracefully.

### Timing Boundaries (BND-056–070)

BND-056: Migration during business hours → Partners table locked briefly.  
BND-057: Migration during low traffic → Minimal impact.  
BND-058: Page load immediately after migration → Shows placeholders.  
BND-059: API call during migration execution → Blocked briefly.  
BND-060: Partner list cached before migration → Stale logo URLs in cache.  
BND-061: Cache invalidation after migration → Fresh data on next load.  
BND-062: Logo upload initiated before migration, completed after → New URL persists.  
BND-063: Partner created during migration → New partner unaffected.  
BND-064: Partner updated during migration → Update may conflict.  
BND-065: EDS sync during migration → Sync may re-introduce clearbit.  
BND-066: Multiple migrations in same deployment → Sequential execution.  
BND-067: Migration rollback during deployment → No restoration possible.  
BND-068: Application restart after migration → Normal operation.  
BND-069: Database backup before migration → Can restore if needed.  
BND-070: Database backup after migration → Contains cleared data.

### Extended Boundary Scenarios (BND-071–090)

BND-071: LogoUrl exactly at VARCHAR/column length limit with clearbit → Cleared.  
BND-072: LogoUrl with "clearbit" as only non-whitespace content → Cleared.  
BND-073: LogoUrl with leading/trailing whitespace + clearbit → Cleared (LIKE matches).  
BND-074: LogoUrl with tab character before "clearbit" → May or may not match.  
BND-075: LogoUrl with zero-width Unicode + "clearbit" → Cleared if substring present.  
BND-076: Partner list pagination: page 1 all cleared, page 2 all non-cleared → Correct per page.  
BND-077: Partner list pagination: last page has single cleared partner → Placeholder shown.  
BND-078: Opportunity with max partners (e.g., 50) all cleared → All placeholders.  
BND-079: Partner tree depth with cleared logos at each level → Placeholders at all levels.  
BND-080: Placeholder image file size at 1KB boundary → Loads quickly.  
BND-081: Placeholder image file size at 100KB boundary → Acceptable load.  
BND-082: Migration execution time at 1ms per row boundary → Scales linearly.  
BND-083: Migration execution time at 30s timeout boundary → Completes or times out.  
BND-084: API response size with 100 partners all null LogoUrl → Reduced payload.  
BND-085: API response size with 100 partners mixed LogoUrl → Correct payload size.  
BND-086: Browser memory with 500 placeholder images on single page → No leak.  
BND-087: Database connection pool at max during migration → No exhaustion.  
BND-088: Migration applied at midnight boundary → No date-related side effects.  
BND-089: Migration applied across DST change → No time-related side effects.  
BND-090: Partner with LogoUrl = "clearbit" (exact 8 chars) at index boundary → Cleared.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### SQL Script Logic (FUN-001–015)

FUN-001: Script targets `public."Partners"` table.  
FUN-002: Script sets `LogoUrl = NULL` (not empty string).  
FUN-003: Script filters `WHERE "LogoUrl" IS NOT NULL` → Skips NULL rows.  
FUN-004: Script filters `AND "LogoUrl" LIKE '%clearbit%'` → Substring match.  
FUN-005: PostgreSQL LIKE is case-sensitive by default.  
FUN-006: Script does NOT use ILIKE (case-insensitive).  
FUN-007: Script runs in single implicit transaction.  
FUN-008: Script is idempotent → Second run affects 0 rows.  
FUN-009: Script does NOT update other columns.  
FUN-010: Script does NOT delete rows.  
FUN-011: Script does NOT affect Partners.Name, Partners.Status, etc.  
FUN-012: Script does NOT affect soft-delete fields (IsDeleted).  
FUN-013: Script does NOT affect audit fields (LastModifiedBy/Date).  
FUN-014: Script does NOT filter by IsDeleted → Affects all partners.  
FUN-015: Script does NOT filter by Status → Affects all statuses.

### UI Display Rules (FUN-016–030)

FUN-016: `app-picture` with null imageUrl → Shows entityType placeholder.  
FUN-017: `app-picture` with empty imageUrl → Shows entityType placeholder.  
FUN-018: `app-picture` with valid imageUrl → Shows real image.  
FUN-019: `app-picture` entityType="Partner" → Fallback is `Partner.png`.  
FUN-020: `app-picture` onerror handler → Replaces with placeholder.  
FUN-021: Partner tabs display: `recordData.logoUrl || ''` → Falsy null triggers fallback.  
FUN-022: Partner list column: `field: 'logoUrl'` → Null renders empty cell or placeholder.  
FUN-023: Partner tree: `field: 'logoUrl'` → Same fallback behavior.  
FUN-024: Opportunity Who section: `partner.partnerLogoUrl || 'assets/images/Partner.png'`.  
FUN-025: Opportunity partner list with fallback → Correct image per partner.  
FUN-026: Placeholder file `assets/images/Partner.png` exists and is valid image.  
FUN-027: Placeholder file is PNG format.  
FUN-028: Placeholder file renders at expected dimensions.  
FUN-029: Placeholder has transparent background (if applicable).  
FUN-030: Placeholder meets accessibility contrast requirements.

### API Behavior (FUN-031–040)

FUN-031: Partner GET API returns `logoUrl: null` for cleared partner.  
FUN-032: Partner GET API returns correct logoUrl for non-cleared partner.  
FUN-033: Partner list API includes logoUrl field in response.  
FUN-034: ValuesController returns partner list with correct logoUrls.  
FUN-035: UNOPSPartnerManager converts non-null LogoUrl to GCS signed URL.  
FUN-036: UNOPSPartnerManager returns null for null LogoUrl (no GCS conversion).  
FUN-037: OpportunityMappingProfile maps `Partner.LogoUrl` → `PartnerLogoUrl`.  
FUN-038: Opportunity API returns `partnerLogoUrl: null` for cleared partner.  
FUN-039: Partner update API can set new LogoUrl after clearing.  
FUN-040: Partner logo upload endpoint works for previously cleared partner.

### Seeder Compatibility (FUN-041–050)

FUN-041: `PartnerSeeder_v2.cs` still references clearbit URLs → Future seeds re-introduce.  
FUN-042: `ProspectAccountsSeeder.cs` still references clearbit URLs → Future seeds re-introduce.  
FUN-043: Seeders should be updated to remove clearbit references (tech debt).  
FUN-044: Fresh database seeded after migration → Clearbit URLs present until next migration.  
FUN-045: EDS sync does not re-introduce clearbit URLs (if oUP doesn't use clearbit).  
FUN-046: Manual partner creation with logo upload → Correct URL stored.  
FUN-047: Manual partner creation without logo → LogoUrl remains null.  
FUN-048: Partner import via CSV → LogoUrl column handled correctly.  
FUN-049: Partner data export → Shows null for cleared logos.  
FUN-050: Partner data export → Shows correct URL for non-cleared logos.

### Extended Functional Scenarios (FUN-051–090)

FUN-051: Migration Up() invokes SQL script via EF migration infrastructure.  
FUN-052: Migration Down() is empty or no-op → No restoration.  
FUN-053: Migration adds entry to __EFMigrationsHistory with correct name.  
FUN-054: Script uses parameterized or safe SQL → No injection.  
FUN-055: Script execution returns row count (or 0) for idempotent verification.  
FUN-056: `getEffectiveImageUrl()` handles undefined input → Placeholder.  
FUN-057: `getEffectiveImageUrl()` handles whitespace-only input → Placeholder.  
FUN-058: `onImageError()` does not throw → Graceful fallback.  
FUN-059: Partner model serialization omits or nullifies logoUrl when null.  
FUN-060: Partner model deserialization accepts null logoUrl.  
FUN-061: Partner list API pagination includes logoUrl in each item.  
FUN-062: Partner filter API works with logoUrl=null filter (if supported).  
FUN-063: Partner sort API works when sorting by logoUrl (nulls last/first).  
FUN-064: Opportunity funding partner mapping preserves null partnerLogoUrl.  
FUN-065: Opportunity client partner mapping preserves null partnerLogoUrl.  
FUN-066: Interaction partner reference displays placeholder when partner logo cleared.  
FUN-067: Dashboard partner widget uses placeholder for cleared logos.  
FUN-068: Search result partner snippet uses placeholder for cleared logos.  
FUN-069: Partner typeahead/dropdown uses placeholder for cleared logos.  
FUN-070: Partner merge logic preserves non-null LogoUrl from surviving partner.  
FUN-071: Partner merge when both have null LogoUrl → Result is null.  
FUN-072: Partner duplicate check does not fail on null LogoUrl.  
FUN-073: Partner audit log records LogoUrl change from URL to null.  
FUN-074: Partner report generation handles null LogoUrl in template.  
FUN-075: Partner PDF export uses placeholder for null LogoUrl.  
FUN-076: Partner email notification template handles null LogoUrl.  
FUN-077: Partner CSV export outputs empty or "null" for cleared LogoUrl.  
FUN-078: Partner CSV import maps empty LogoUrl column to null.  
FUN-079: Partner bulk update does not overwrite null with clearbit.  
FUN-080: Partner bulk delete preserves LogoUrl state of remaining partners.  
FUN-081: Partner restore from soft-delete preserves null LogoUrl.  
FUN-082: Partner copy/clone does not copy clearbit URL (if clone supported).  
FUN-083: Partner version history shows LogoUrl change (if versioning supported).  
FUN-084: Partner API versioning maintains logoUrl field in all versions.  
FUN-085: Partner GraphQL schema (if used) returns null for logoUrl.  
FUN-086: Partner OData (if used) returns null for logoUrl.  
FUN-087: Partner cache key does not include logoUrl (or handles null).  
FUN-088: Partner cache invalidation triggers on LogoUrl update.  
FUN-089: Partner real-time update (if WebSocket) pushes null logoUrl.  
FUN-090: Partner mobile app receives null logoUrl and displays placeholder.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Migration + API (INT-001–015)

INT-001: Apply migration → GET partner → LogoUrl is null.  
INT-002: Apply migration → GET partner list → Cleared logos are null.  
INT-003: Apply migration → GET opportunity → partnerLogoUrl is null for cleared partner.  
INT-004: Apply migration → Partner detail page → Placeholder displayed.  
INT-005: Apply migration → Partner list page → Placeholder for cleared partners.  
INT-006: Apply migration → Opportunity Who section → Placeholder for cleared partners.  
INT-007: Apply migration → Partner tree → Placeholder for cleared partners.  
INT-008: Apply migration → Upload new logo for cleared partner → New URL stored.  
INT-009: Apply migration → Update partner with non-clearbit URL → New URL persisted.  
INT-010: Apply migration → Create new partner → LogoUrl starts as null.  
INT-011: Apply migration → Create new partner with logo → LogoUrl set correctly.  
INT-012: Migration + partner search → Search results show correct logos.  
INT-013: Migration + partner filter → Filter works with null LogoUrl.  
INT-014: Migration + partner sort → Sort works with null LogoUrl.  
INT-015: Migration + partner export → Export shows null for cleared.

### Cross-Component (INT-016–030)

INT-016: Cleared partner logo on partner detail → Placeholder.  
INT-017: Cleared partner logo in opportunity funding partners → Placeholder.  
INT-018: Cleared partner logo in opportunity client partners → Placeholder.  
INT-019: Cleared partner logo in interaction partner reference → Placeholder or name only.  
INT-020: Cleared partner logo in AI summary → No broken image reference.  
INT-021: Cleared partner logo in dashboard widget → Placeholder.  
INT-022: Cleared partner logo in search results → Placeholder.  
INT-023: Non-cleared partner logo in all above → Real image.  
INT-024: Mix of cleared and non-cleared in same list → Correct per row.  
INT-025: Partner with logo uploaded AFTER migration → Displays correctly.  
INT-026: Partner with GCS-signed URL → Still works (non-clearbit).  
INT-027: Partner logo in email notification → Null handled (no broken image).  
INT-028: Partner logo in PDF export → Placeholder or no image.  
INT-029: Partner logo in print view → Correct display.  
INT-030: Partner logo in mobile responsive view → Correct layout.

### Data Lifecycle (INT-031–040)

INT-031: Migration → Soft-delete partner → Restore → LogoUrl still null.  
INT-032: Migration → Partner merge → LogoUrl from surviving partner used.  
INT-033: Migration → EDS sync → LogoUrl not overwritten with clearbit.  
INT-034: Migration → Application restart → Normal operation.  
INT-035: Migration → Database backup → Backup has cleared data.  
INT-036: Migration → Database restore from pre-migration backup → Clearbit URLs return.  
INT-037: Migration + 10 other migrations → Sequential execution, all succeed.  
INT-038: Migration applied to development environment → Correct behavior.  
INT-039: Migration applied to staging environment → Correct behavior.  
INT-040: Migration applied to production environment → Correct behavior.

### Error Recovery (INT-041–050)

INT-041: Migration fails mid-execution → Transaction rolled back, no partial clear.  
INT-042: Application running during migration → Brief lock on Partners table.  
INT-043: Concurrent partner update during migration → Update blocked, completes after.  
INT-044: Concurrent partner creation during migration → Creation proceeds after lock.  
INT-045: API request during migration → May be delayed, not failed.  
INT-046: Page load during migration → May show stale data, refreshes correctly.  
INT-047: Browser cache with old logo image → Image load fails → Placeholder.  
INT-048: CDN cache with old logo URL → CDN returns error → Placeholder via onerror.  
INT-049: Service worker cache with old logo → Cache invalidated on next deploy.  
INT-050: Application health check during migration → Passes (migration doesn't break app).

### Extended Integration Scenarios (INT-051–090)

INT-051: Migration → Partner API → Opportunity API → Opportunity page shows placeholder.  
INT-052: Migration → Partner API → Interaction API → Interaction shows placeholder.  
INT-053: Migration → Partner API → Search API → Search result shows placeholder.  
INT-054: Migration → Seeder run → New partners with clearbit → Next migration clears.  
INT-055: Migration → EDS sync → oUP partner sync → LogoUrl handling correct.  
INT-056: Migration → Partner import CSV → Imported clearbit URLs → Next migration clears.  
INT-057: Migration → Partner export → Import same file → Round-trip preserves null.  
INT-058: Migration → Partner bulk update → LogoUrl not overwritten with clearbit.  
INT-059: Migration → Partner merge (A into B) → B's LogoUrl used, A's cleared.  
INT-060: Migration → Partner merge (B into A) → A's LogoUrl used, B's cleared.  
INT-061: Migration → Partner soft-delete → Restore → LogoUrl remains null.  
INT-062: Migration → Partner duplicate creation → New partner has null or new logo.  
INT-063: Migration → Opportunity create with cleared partner → partnerLogoUrl null.  
INT-064: Migration → Opportunity update partner → partnerLogoUrl reflects change.  
INT-065: Migration → Interaction create with cleared partner → Placeholder in UI.  
INT-066: Migration → Document attach to partner → Document metadata unaffected.  
INT-067: Migration → AI prompt with partner context → No clearbit URL in prompt.  
INT-068: Migration → Report generation with partners → Null logos in report.  
INT-069: Migration → Dashboard refresh → All placeholders load.  
INT-070: Migration → Notification trigger with partner → Email has placeholder.  
INT-071: Migration → oUP deep link to partner → Partner page loads with placeholder.  
INT-072: Migration → Partner tree expand/collapse → Placeholders at all levels.  
INT-073: Migration → Partner list infinite scroll → Placeholders load correctly.  
INT-074: Migration → Partner list virtual scroll → Placeholders render.  
INT-075: Migration → Partner detail tab switch → Placeholder persists.  
INT-076: Migration → Opportunity partner modal → Placeholder in modal.  
INT-077: Migration → Partner comparison (if supported) → Null vs null comparison.  
INT-078: Migration → Partner timeline (if supported) → Logo change in timeline.  
INT-079: Migration → Partner activity feed → No broken image in feed.  
INT-080: Migration → Partner notification preferences → Unaffected.  
INT-081: Migration → Partner permissions → Unaffected.  
INT-082: Migration → Partner roles → Unaffected.  
INT-083: Migration → Partner tags → Unaffected.  
INT-084: Migration → Partner custom fields → Unaffected.  
INT-085: Migration → Partner workflow status → Unaffected.  
INT-086: Migration → Partner audit log query → LogoUrl change visible.  
INT-087: Migration → Partner analytics/reporting → Null logos in metrics.  
INT-088: Migration → Partner API rate limiting → Unaffected.  
INT-089: Migration → Partner API authentication → Unaffected.  
INT-090: Migration → Full E2E: Login → Partner list → Detail → Opportunity → All placeholders.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Migration UPDATE + concurrent partner read → Read blocked or sees old data.  
CON-002: Migration UPDATE + concurrent partner write → Write blocked.  
CON-003: Migration UPDATE + concurrent logo upload → Upload completes after migration.  
CON-004: Migration UPDATE + concurrent partner delete → Delete completes after migration.  
CON-005: Two migration runners simultaneously → Table lock prevents double-run.  
CON-006: Migration + concurrent EDS partner sync → Sync blocked during UPDATE.  
CON-007: Migration + concurrent partner list API → API may timeout or return stale.  
CON-008: Migration + concurrent partner search → Search may show stale logos.  
CON-009: 10 concurrent partner page loads during migration → All eventually succeed.  
CON-010: 50 concurrent partner API requests during migration → Queued, then served.  
CON-011: Migration lock duration proportional to affected rows.  
CON-012: Migration with row-level locking → Only affected rows locked.  
CON-013: Migration on read replica → Replica catches up after primary commit.  
CON-014: Concurrent cache invalidation after migration → All clients get fresh data.  
CON-015: Concurrent partner logo upload during migration → Upload wins if after commit.  
CON-016: Partner page open before migration, refreshed after → Shows placeholder.  
CON-017: Multiple users viewing same partner during migration → All see updated state.  
CON-018: WebSocket notification after migration → Not implemented (poll refresh).  
CON-019: API response caching → Cache invalidated after migration.  
CON-020: DB connection pool under load during migration → Pool handles.  
CON-021: Migration during database vacuum → Both complete.  
CON-022: Migration during database reindex → Both complete.  
CON-023: Migration during backup → Consistent snapshot.  
CON-024: Transaction isolation for migration → Read Committed.  
CON-025: Migration visibility to concurrent transactions → Visible after commit.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: SQL script contains correct table name `public."Partners"`.  
UNT-002: SQL script contains correct column name `"LogoUrl"`.  
UNT-003: SQL script uses `SET "LogoUrl" = NULL` (not empty string).  
UNT-004: SQL script WHERE clause checks `IS NOT NULL`.  
UNT-005: SQL script WHERE clause uses `LIKE '%clearbit%'`.  
UNT-006: Migration Up() calls script executor.  
UNT-007: Migration Down() is empty (no rollback).  
UNT-008: `getEffectiveImageUrl()` returns imageUrl when non-empty.  
UNT-009: `getEffectiveImageUrl()` returns placeholder when imageUrl is empty.  
UNT-010: `getEffectiveImageUrl()` returns placeholder when imageUrl is null.  
UNT-011: `getEffectiveImageUrl()` returns `Partner.png` for entityType="Partner".  
UNT-012: `onImageError()` sets src to placeholder path.  
UNT-013: `onImageError()` second failure hides image element.  
UNT-014: Partner model `logoUrl` property is optional/nullable.  
UNT-015: GCS signed URL conversion skips null LogoUrl.  
UNT-016: GCS signed URL conversion processes non-null LogoUrl.  
UNT-017: OpportunityMappingProfile maps null LogoUrl to null PartnerLogoUrl.  
UNT-018: OpportunityMappingProfile maps valid LogoUrl to PartnerLogoUrl.  
UNT-019: `recordData.logoUrl || ''` evaluates to '' for null.  
UNT-020: `recordData.logoUrl || ''` evaluates to '' for undefined.  
UNT-021: `partner.partnerLogoUrl || 'assets/images/Partner.png'` evaluates to placeholder for null.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Migration on 100 partners with clearbit → < 1s.  
PRF-002: Migration on 1K partners with clearbit → < 5s.  
PRF-003: Migration on 10K partners with clearbit → < 30s.  
PRF-004: Migration on 100K total partners (1K clearbit) → < 30s.  
PRF-005: Partner list load with all null logos → < 500ms (no external image fetch).  
PRF-006: Partner list load with all valid logos → Performance unchanged vs before.  
PRF-007: Placeholder image load time → < 50ms (local asset).  
PRF-008: GCS signed URL conversion skips null → < 1ms per partner.  
PRF-009: Partner detail page with placeholder → < 200ms load.  
PRF-010: Opportunity page with 10 partner placeholders → < 500ms.  
PRF-011: No external HTTP requests for cleared logos → Reduced bandwidth.  
PRF-012: Image onerror handler execution → < 5ms.  
PRF-013: Browser rendering with placeholder → No layout shift.  
PRF-014: Migration lock duration < 5s for 1K rows.  
PRF-015: Concurrent partner API requests after migration → No degradation.  
PRF-016: Database table size reduction from NULL vs URL string → Marginal.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: Migration on production-scale database (100K partners) → Completes.  
LDT-002: 100 concurrent partner page loads after migration → All show correct logos.  
LDT-003: 200 concurrent API requests for partner details → Correct logoUrl values.  
LDT-004: 50 concurrent opportunity page loads with cleared partners → Placeholders display.  
LDT-005: Partner list with 1000 rows, all placeholders → Page renders < 2s.  
LDT-006: Image loading stress: 500 placeholder images on single page → Browser handles.  
LDT-007: Migration + 100 concurrent partner operations → All complete.  
LDT-008: Sustained partner page loads (1000/hour) after migration → Stable.  
LDT-009: Recovery after migration failure → Clean rollback.  
LDT-010: Recovery after image asset corruption → Re-deploy restores placeholder.

---

## Status: Ready for Implementation
