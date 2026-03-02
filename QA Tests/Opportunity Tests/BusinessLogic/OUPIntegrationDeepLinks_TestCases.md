# oUP Integration + Deep Links — Comprehensive Test Cases

**Component:** oUP External Integration — BaseEngagement Linking, Deep Links, "Go to oUP" Button  
**URL Format:** `{OUPSettings:BaseUrl}/{engagementNumber}/engagement/overview`  
**Deep Link Format:** `/partnerships/opportunities/{id}` (path-based)  
**Frontend:** `opportunity-view.component.ts` — `oupBaseUrl`, `baseEngagementNumber`, `oupEngagementUrl`  
**Backend:** `OpportunityController.cs` — Returns `baseEngagementNumber`, `ValuesController.cs` — Returns `oupSettings.baseUrl`  
**Entity:** `BaseEngagement` — `OpportunityId`, `EngagementNumber`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
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

**Ratio Compliance Checks:**

| Check | Formula | Actual | Required | Status |
|-------|---------|--------|----------|--------|
| N ≥ 3P | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 | ✅ |
| E ≥ 3P | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 | ✅ |
| F ≥ 3P | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 | ✅ |
| I ≥ 3P | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 | ✅ |

---

## Feature Overview

### oUP Outbound Link ("Go to oUP")

```typescript
oupEngagementUrl = computed(() => {
    const baseUrl = this.oupBaseUrl();                    // from config
    const engagementNumber = this.baseEngagementNumber(); // from BaseEngagement
    if (baseUrl && engagementNumber) {
        return `${baseUrl}/${engagementNumber}/engagement/overview`;
    }
    return null;
});
```

**Example:** `https://projects-test.unops.org/12345/engagement/overview`

### Inbound Deep Link (from oUP to Opportunity+)

**Path-based:** `/partnerships/opportunities/{id}`  
**Legacy hash:** `/#/partnerships/opportunities/{id}` (should redirect)

### Data Flow

```
Opportunity+ → OpportunityController → BaseEngagement → EngagementNumber → URL → oUP
oUP → Deep Link → /partnerships/opportunities/{id} → Opportunity+ Router → Opportunity Detail
```

### Configuration

| Setting | Source | Purpose |
|---------|--------|---------|
| `OUPSettings:BaseUrl` | `appsettings.json` | Base URL for oUP system |
| `baseEngagementNumber` | `BaseEngagement.EngagementNumber` | oUP engagement identifier |

### Known QA Issues

- **QA-014:** oUP integration tests blocked (missing `OUP_BASE_URL`, credentials)
- **QA-015:** "Go to oUP" button only fully testable in production environment

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### Go to oUP Button (POS-001–012)

POS-001: Opportunity linked to BaseEngagement → "Go to oUP" button visible.  
POS-002: "Go to oUP" button URL format: `{baseUrl}/{engagementNumber}/engagement/overview`.  
POS-003: Clicking "Go to oUP" opens oUP in new tab.  
POS-004: Button opens correct engagement overview page.  
POS-005: Button visible on opportunity detail page.  
POS-006: Button text is translated (en/fr/es/pt).  
POS-007: Button has correct icon/styling.  
POS-008: Button URL uses configured `OUPSettings:BaseUrl`.  
POS-009: Button URL includes correct `engagementNumber`.  
POS-010: Button target attribute = `_blank` (new tab).  
POS-011: Button rel attribute = `noopener noreferrer` (security).  
POS-012: Button visible regardless of opportunity status (NO GO, GO, etc.).

### Configuration & API (POS-013–022)

POS-013: `ValuesController` returns `oupSettings.baseUrl` from configuration.  
POS-014: Config API returns baseUrl as non-null string.  
POS-015: `OpportunityController` queries `BaseEngagements` for opportunity.  
POS-016: `OpportunityController` returns `baseEngagementNumber` in response.  
POS-017: `BaseEngagement` filtered by `OpportunityId` and `!IsDeleted`.  
POS-018: `OpportunityController` returns null `baseEngagementNumber` when no link.  
POS-019: Multiple BaseEngagements → First non-deleted used.  
POS-020: BaseEngagement with valid EngagementNumber → Correct URL built.  
POS-021: Frontend `oupBaseUrl` signal populated from config service.  
POS-022: Frontend `baseEngagementNumber` signal populated from opportunity API.

### Deep Links Inbound (POS-023–030)

POS-023: Navigate to `/partnerships/opportunities/123` → Opportunity 123 detail page.  
POS-024: Deep link from external system → Correct opportunity loaded.  
POS-025: Deep link with valid opportunity ID → Detail page renders.  
POS-026: Deep link after login → Redirected to opportunity.  
POS-027: Deep link shared via email → Opens correct opportunity.  
POS-028: Deep link bookmarked → Returns to same opportunity.  
POS-029: Deep link in browser address bar → Routes correctly.  
POS-030: Deep link preserves URL after page load → No redirect flicker.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Missing Configuration (NEG-001–015)

NEG-001: `OUPSettings:BaseUrl` not configured → Button hidden or disabled.  
NEG-002: `OUPSettings:BaseUrl` = null → `oupEngagementUrl` returns null → Button hidden.  
NEG-003: `OUPSettings:BaseUrl` = "" → Button hidden.  
NEG-004: `OUPSettings:BaseUrl` = invalid URL → Button may display but link broken.  
NEG-005: `OUPSettings:BaseUrl` with trailing slash → URL has double slash.  
NEG-006: `OUPSettings:BaseUrl` without protocol → URL malformed.  
NEG-007: `OUPSettings:BaseUrl` with wrong protocol (ftp://) → Link opens wrong handler.  
NEG-008: Config API fails → `oupBaseUrl` not set → Button hidden.  
NEG-009: Config API returns empty object → `oupSettings` undefined → Button hidden.  
NEG-010: Config API timeout → Frontend retries or shows error.  
NEG-011: Config API returns wrong environment URL → Links to wrong oUP instance.  
NEG-012: Config API returns production URL in dev → Security concern.  
NEG-013: Config API returns dev URL in production → Functional issue.  
NEG-014: Config changes without app restart → Stale URL used.  
NEG-015: Config with special characters in URL → Encoded correctly.

### Missing BaseEngagement (NEG-016–030)

NEG-016: Opportunity not linked to BaseEngagement → `baseEngagementNumber` = null → Button hidden.  
NEG-017: BaseEngagement exists but `IsDeleted = true` → Not found → Button hidden.  
NEG-018: BaseEngagement exists but `OpportunityId` doesn't match → Not found.  
NEG-019: BaseEngagement with null `EngagementNumber` → Button hidden.  
NEG-020: BaseEngagement with empty `EngagementNumber` → Button hidden.  
NEG-021: BaseEngagement query error → API returns without engagement number.  
NEG-022: Multiple BaseEngagements, all soft-deleted → No link available.  
NEG-023: BaseEngagement for different opportunity → Wrong link (FK mismatch).  
NEG-024: BaseEngagement created after page load → Button not shown until refresh.  
NEG-025: BaseEngagement deleted after page load → Button shows stale link.  
NEG-026: BaseEngagement.EngagementNumber with special characters → URL encoded.  
NEG-027: BaseEngagement.EngagementNumber with spaces → URL encoded or error.  
NEG-028: BaseEngagement.EngagementNumber with Unicode → URL encoded.  
NEG-029: BaseEngagement.EngagementNumber very long (1000 chars) → URL too long.  
NEG-030: BaseEngagement.OpportunityId = 0 → FK violation.

### Deep Link Failures (NEG-031–050)

NEG-031: Deep link with non-existent opportunity ID → 404 page.  
NEG-032: Deep link with negative opportunity ID → 404 or error.  
NEG-033: Deep link with ID = 0 → 404 or error.  
NEG-034: Deep link with non-numeric ID → Router error.  
NEG-035: Deep link with soft-deleted opportunity → 404 or access denied.  
NEG-036: Deep link with SQL injection in ID → Sanitized, 404.  
NEG-037: Deep link with XSS in URL → Escaped, no execution.  
NEG-038: Deep link with path traversal → Blocked by router.  
NEG-039: Deep link without authentication → Redirected to login, then back.  
NEG-040: Deep link without authorization → Access denied page.  
NEG-041: Legacy hash URL `/#/partnerships/opportunities/123` → Should redirect.  
NEG-042: Deep link with extra path segments → 404 or ignored.  
NEG-043: Deep link with query parameters → Handled or ignored.  
NEG-044: Deep link with fragment → Handled or ignored.  
NEG-045: Deep link with encoded characters → Decoded correctly.  
NEG-046: Deep link from incognito window → Login required.  
NEG-047: Deep link from different browser → Login required.  
NEG-048: Deep link with expired session → Re-auth then redirect.  
NEG-049: Deep link with case mismatch in path → Route handles or 404.  
NEG-050: Deep link to wrong route `/partnerships/opportunity/123` (singular) → 404.

### oUP System Failures (NEG-051–065)

NEG-051: oUP system down → "Go to oUP" button navigates to error page.  
NEG-052: oUP engagement not found (wrong number) → oUP 404.  
NEG-053: oUP engagement deleted → oUP 404 or error.  
NEG-054: oUP URL format changed → Button links to wrong page.  
NEG-055: oUP requires separate authentication → User must log in to oUP.  
NEG-056: oUP session expired → oUP login page shown.  
NEG-057: oUP rate limit → Too many requests error.  
NEG-058: oUP SSL certificate expired → Browser security warning.  
NEG-059: oUP returns redirect → Button follows redirect.  
NEG-060: oUP returns 500 → Server error page.  
NEG-061: Network error reaching oUP → Browser connection error.  
NEG-062: DNS resolution failure for oUP → Browser error.  
NEG-063: Firewall blocks oUP access → Connection timeout.  
NEG-064: Proxy required for oUP → Configuration needed.  
NEG-065: oUP on different domain → CORS not relevant (new tab).

### Data Integrity Failures (NEG-066–070)

NEG-066: BaseEngagement.EngagementNumber changed in oUP → Old URL invalid.  
NEG-067: BaseEngagement.EngagementNumber out of sync → Wrong engagement.  
NEG-068: EDS sync fails → BaseEngagement data stale.  
NEG-069: Multiple BaseEngagements for same opportunity → First used, may be wrong.  
NEG-070: Opportunity ID reused after hard delete → Deep link points to wrong record.

### ValuesController & Environment URL Failures (NEG-071–080)

NEG-071: `ValuesController` endpoint returns 500 → Frontend handles gracefully.  
NEG-072: `ValuesController` returns null for `oupSettings` → Button hidden.  
NEG-073: `ValuesController` returns malformed JSON → Parsing error handled.  
NEG-074: `ValuesController` requires auth but called unauthenticated → 401 handled.  
NEG-075: Environment URL mismatch (dev config in staging) → Wrong oUP instance.  
NEG-076: `OUPSettings:BaseUrl` for wrong environment in appsettings → Links fail.  
NEG-077: Multiple environment configs loaded → Correct one selected.  
NEG-078: `ValuesController` cache returns stale environment URL → Refresh needed.  
NEG-079: Environment variable override of BaseUrl missing → Fallback to config.  
NEG-080: `ValuesController` rate limited → Frontend retry or fallback.

### Path-Based URL & BaseEngagement Edge Failures (NEG-081–090)

NEG-081: Path-based URL `/partnerships/opportunities/` (trailing, no ID) → 404.  
NEG-082: Path-based URL with double slashes → Router rejects or normalizes.  
NEG-083: BaseEngagement.EngagementNumber = "0" → May produce invalid oUP URL.  
NEG-084: BaseEngagement linked to soft-deleted opportunity → Query excludes.  
NEG-085: BaseEngagement.OpportunityId nullable but null → Not returned.  
NEG-086: OpportunityController returns 404 → baseEngagementNumber never fetched.  
NEG-087: Path-based deep link with GUID format → Not matched by :id route.  
NEG-088: Deep link with ID exceeding MAX_INT → Overflow or error.  
NEG-089: Go to oUP button clicked while URL recomputing → Stale or null URL.  
NEG-090: BaseEngagement query times out → API returns error, button hidden.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### URL Construction Boundaries (BND-001–020)

BND-001: `baseUrl = "https://projects.unops.org"` + `num = "12345"` → Valid URL.  
BND-002: `baseUrl = "https://projects-test.unops.org"` → Valid URL (test env).  
BND-003: `baseUrl = "https://projects-dev.unops.org"` → Valid URL (dev env).  
BND-004: `baseUrl` with port `https://projects.unops.org:8443` → Valid URL.  
BND-005: `baseUrl` with path `https://projects.unops.org/v2` → Valid URL.  
BND-006: `baseUrl` with trailing slash → Double slash in URL.  
BND-007: `baseUrl` without trailing slash → Correct URL.  
BND-008: `engagementNumber = "1"` → Shortest valid number.  
BND-009: `engagementNumber = "123456789"` → Typical length.  
BND-010: `engagementNumber = "000001"` → Leading zeros preserved.  
BND-011: `engagementNumber = "ABC-123"` → Alphanumeric with dash.  
BND-012: `engagementNumber` at max length → Long URL but valid.  
BND-013: Constructed URL total length < 2083 chars (IE limit) → Accessible.  
BND-014: Constructed URL total length > 2083 chars → May fail in some browsers.  
BND-015: URL with all ASCII characters in engagement number → Encoded.  
BND-016: URL with Unicode in engagement number → Percent-encoded.  
BND-017: URL with special characters `/#?&=` in engagement number → Encoded.  
BND-018: Both `baseUrl` and `engagementNumber` null → Returns null (button hidden).  
BND-019: `baseUrl` null, `engagementNumber` valid → Returns null.  
BND-020: `baseUrl` valid, `engagementNumber` null → Returns null.

### Deep Link Boundaries (BND-021–040)

BND-021: `/partnerships/opportunities/1` → Valid (min ID).  
BND-022: `/partnerships/opportunities/999999` → Valid (large ID).  
BND-023: `/partnerships/opportunities/2147483647` → Valid (MAX_INT).  
BND-024: `/partnerships/opportunities/2147483648` → Overflow, invalid.  
BND-025: `/partnerships/opportunities/-1` → Invalid.  
BND-026: `/partnerships/opportunities/0` → Invalid or handled.  
BND-027: `/partnerships/opportunities/abc` → Invalid, router error.  
BND-028: `/partnerships/opportunities/12.34` → Invalid decimal.  
BND-029: `/partnerships/opportunities/` → Missing ID, 404.  
BND-030: `/partnerships/opportunities` → List page, not detail.  
BND-031: `/PARTNERSHIPS/OPPORTUNITIES/123` → Case sensitivity depends on router.  
BND-032: `/partnerships/opportunities/123/` → Trailing slash handled.  
BND-033: `/partnerships/opportunities/123?tab=team` → Query params passed.  
BND-034: `/partnerships/opportunities/123#section` → Fragment handled.  
BND-035: `/partnerships/opportunities/123/edit` → Nested route or 404.  
BND-036: Deep link URL with spaces → Encoded to `%20`.  
BND-037: Deep link URL with + → Decoded to space or literal.  
BND-038: Deep link URL with % → Decoded or literal.  
BND-039: Deep link after login redirect → Original URL preserved.  
BND-040: Deep link with query params after login → Params preserved.

### BaseEngagement Boundaries (BND-041–055)

BND-041: 0 BaseEngagements for opportunity → No oUP link.  
BND-042: 1 BaseEngagement → Link to that engagement.  
BND-043: 2 BaseEngagements (1 deleted, 1 active) → Link to active.  
BND-044: 2 active BaseEngagements → First used.  
BND-045: 10 BaseEngagements → First non-deleted used.  
BND-046: BaseEngagement created immediately before query → Visible.  
BND-047: BaseEngagement soft-deleted immediately before query → Not visible.  
BND-048: BaseEngagement.EngagementNumber = "" → Treated as no link.  
BND-049: BaseEngagement.EngagementNumber = null → Treated as no link.  
BND-050: BaseEngagement.EngagementNumber = " " → Treated as no link or whitespace URL.  
BND-051: BaseEngagement.OpportunityId at MIN → FK resolves if opportunity exists.  
BND-052: BaseEngagement.OpportunityId at MAX_INT → FK resolves if exists.  
BND-053: BaseEngagement with IsDeleted=true + active record → Active returned.  
BND-054: All BaseEngagements soft-deleted → No link.  
BND-055: BaseEngagement created via EDS sync → Correct EngagementNumber.

### Config Boundaries (BND-056–070)

BND-056: Config loaded on app startup → Available before first opportunity load.  
BND-057: Config loaded after app startup → May be null during initial load.  
BND-058: Config changed at runtime → Stale until next config reload.  
BND-059: Config with different environments → Correct URL per environment.  
BND-060: Config API returns in < 100ms → No delay.  
BND-061: Config API returns in > 5s → Timeout handling.  
BND-062: Config cached in frontend → Subsequent requests fast.  
BND-063: Config cache invalidation → Fresh config on next load.  
BND-064: Config with multiple oUP settings → Correct setting used.  
BND-065: Config missing oUP section entirely → Button hidden gracefully.  
BND-066: Config with extra oUP settings → Extra ignored.  
BND-067: Config with base URL for staging → Staging oUP used.  
BND-068: Config with base URL for production → Production oUP used.  
BND-069: Config across deployments → Updated correctly.  
BND-070: Config in test environment → Test oUP URL or mock.

### ValuesController & Environment URL Boundaries (BND-071–082)

BND-071: ValuesController returns BaseUrl for dev environment → Dev oUP URL.  
BND-072: ValuesController returns BaseUrl for test environment → Test oUP URL.  
BND-073: ValuesController returns BaseUrl for production → Prod oUP URL.  
BND-074: ValuesController BaseUrl exactly at 2083 chars → Edge of URL limit.  
BND-075: ValuesController BaseUrl 1 char over limit → Truncation or error.  
BND-076: ValuesController called before app init → Null or default.  
BND-077: ValuesController called after config reload → Fresh BaseUrl.  
BND-078: Environment-specific appsettings section → Correct BaseUrl per env.  
BND-079: OUPSettings:BaseUrl in appsettings.Development → Dev URL.  
BND-080: OUPSettings:BaseUrl in appsettings.Production → Prod URL.  
BND-081: BaseUrl with subpath `/engagement` → URL concatenation correct.  
BND-082: BaseUrl with query string (invalid) → Stripped or error.

### Path-Based URL & Go to oUP Button Boundaries (BND-083–090)

BND-083: Path `/partnerships/opportunities/123` exact match → Routes.  
BND-084: Path with leading slash only → Root or 404.  
BND-085: Go to oUP button visibility at exact moment both signals populated → No flicker.  
BND-086: Go to oUP button when baseUrl loads 1ms before engagementNumber → Brief hidden then shown.  
BND-087: Go to oUP button when engagementNumber loads 1ms before baseUrl → Brief hidden then shown.  
BND-088: EngagementNumber at boundary "0" vs null → Distinct handling.  
BND-089: BaseEngagement.EngagementNumber exactly 255 chars → URL length boundary.  
BND-090: Path-based deep link with ID = 1 vs ID = 01 → Same opportunity or distinct.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### URL Construction (FUN-001–012)

FUN-001: `oupEngagementUrl` computed from `oupBaseUrl` + `baseEngagementNumber`.  
FUN-002: URL format: `{baseUrl}/{engagementNumber}/engagement/overview`.  
FUN-003: URL returns null when `baseUrl` is falsy.  
FUN-004: URL returns null when `engagementNumber` is falsy.  
FUN-005: URL returns null when both are falsy.  
FUN-006: URL returns valid string when both are truthy.  
FUN-007: Computed signal reactivity → URL updates when inputs change.  
FUN-008: Button visibility bound to `oupEngagementUrl()` truthiness.  
FUN-009: Button `href` attribute set to computed URL.  
FUN-010: Button opens new tab (target="_blank").  
FUN-011: Button has noopener/noreferrer for security.  
FUN-012: Button disabled when URL is null.

### Backend Data Flow (FUN-013–025)

FUN-013: `OpportunityController.Get` queries `BaseEngagements` by `OpportunityId`.  
FUN-014: Query filters `!IsDeleted`.  
FUN-015: `FirstOrDefaultAsync` returns first non-deleted engagement.  
FUN-016: Returns `EngagementNumber` if BaseEngagement found.  
FUN-017: Returns null if no BaseEngagement found.  
FUN-018: Response includes `baseEngagementNumber` alongside `opportunity`.  
FUN-019: `ValuesController` reads `OUPSettings:BaseUrl` from `IConfiguration`.  
FUN-020: Config endpoint returns `oupSettings.baseUrl` in response object.  
FUN-021: Frontend `valuesService.getConfig()` calls config endpoint.  
FUN-022: Frontend stores `oupBaseUrl` in signal from config response.  
FUN-023: Frontend stores `baseEngagementNumber` in signal from opportunity response.  
FUN-024: Signals trigger computed URL recalculation.  
FUN-025: Computed URL available before template renders (no flicker).

### Deep Link Routing (FUN-026–038)

FUN-026: Angular router matches `/partnerships/opportunities/:id` route.  
FUN-027: Route parameter `:id` extracted correctly.  
FUN-028: Route guard checks authentication.  
FUN-029: Unauthenticated user redirected to login with return URL.  
FUN-030: After login, user redirected to original deep link.  
FUN-031: Route guard checks authorization.  
FUN-032: Unauthorized user shown access denied page.  
FUN-033: Route resolver loads opportunity data by ID.  
FUN-034: Non-existent opportunity ID → 404 page.  
FUN-035: Route transitions use Angular router navigation.  
FUN-036: Browser back button works after deep link navigation.  
FUN-037: Browser forward button works after deep link navigation.  
FUN-038: Deep link URL visible in browser address bar.

### BaseEngagement Rules (FUN-039–050)

FUN-039: BaseEngagement entity has `OpportunityId` FK.  
FUN-040: BaseEngagement entity has `EngagementNumber` property.  
FUN-041: BaseEngagement inherits audit/soft-delete from base class.  
FUN-042: BaseEngagement.IsDeleted filters correctly.  
FUN-043: BaseEngagement created by EDS sync → `EngagementNumber` populated.  
FUN-044: BaseEngagement linked to correct opportunity.  
FUN-045: One opportunity can have multiple BaseEngagements.  
FUN-046: One BaseEngagement linked to one opportunity.  
FUN-047: BaseEngagement without OpportunityId → Not linked.  
FUN-048: BaseEngagement.OpportunityId = newly added nullable column.  
FUN-049: BaseEngagement query performance with index.  
FUN-050: BaseEngagement list API endpoint functional.

### Go to oUP Button & ValuesController (FUN-051–065)

FUN-051: Go to oUP button renders only when oupEngagementUrl is non-null.  
FUN-052: Go to oUP button uses computed URL as href.  
FUN-053: Go to oUP button translation key resolves in all locales.  
FUN-054: ValuesController config endpoint returns JSON with oupSettings.  
FUN-055: ValuesController BaseUrl mapped from OUPSettings:BaseUrl.  
FUN-056: ValuesController response cached by frontend config service.  
FUN-057: ValuesController called on app bootstrap or first opportunity view.  
FUN-058: Go to oUP button aria-label for accessibility.  
FUN-059: Go to oUP button keyboard navigable.  
FUN-060: Go to oUP button focus management after click.  
FUN-061: ValuesController respects environment (dev/test/prod).  
FUN-062: OUPSettings section in appsettings structure correct.  
FUN-063: Go to oUP button does not block opportunity page load.  
FUN-064: Go to oUP button placement in opportunity header/toolbar.  
FUN-065: ValuesController BaseUrl used only for oUP link construction.

### Path-Based URLs & Deep Link Flow (FUN-066–080)

FUN-066: Path-based route `/partnerships/opportunities/:id` registered.  
FUN-067: Path-based URL without hash used for deep links.  
FUN-068: Legacy hash URL redirects to path-based equivalent.  
FUN-069: Deep link route param :id passed to opportunity resolver.  
FUN-070: Deep link with valid ID loads OpportunityItemComponent.  
FUN-071: Deep link return URL preserved during login redirect.  
FUN-072: Deep link with query params (e.g. ?tab=team) passed to component.  
FUN-073: Path-based URL supports browser history.  
FUN-074: Path-based URL supports bookmarking.  
FUN-075: Path-based URL supports sharing via email.  
FUN-076: Deep link from oUP "View in Opportunity+" link → Correct path.  
FUN-077: RouterLink or href to path-based URL navigates correctly.  
FUN-078: Path-based URL canonical for opportunity detail.  
FUN-079: Deep link route has correct route order (no conflict with list).  
FUN-080: Path-based URL encoding/decoding of ID correct.

### BaseEngagement & Environment Integration (FUN-081–090)

FUN-081: BaseEngagement query uses OpportunityId index.  
FUN-082: BaseEngagement IsDeleted filter applied in same query.  
FUN-083: OpportunityController merges BaseEngagement data into response.  
FUN-084: BaseEngagement EngagementNumber format validated (if applicable).  
FUN-085: Environment-specific BaseUrl in appsettings.{Environment}.json.  
FUN-086: Dev environment uses projects-dev.unops.org or equivalent.  
FUN-087: Test environment uses projects-test.unops.org or equivalent.  
FUN-088: Production environment uses projects.unops.org or equivalent.  
FUN-089: BaseEngagement list filtered by partner when applicable.  
FUN-090: BaseEngagement soft-delete excludes from all queries.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### oUP Link End-to-End (INT-001–015)

INT-001: Create opportunity → Link BaseEngagement → "Go to oUP" button appears.  
INT-002: Button URL matches expected format.  
INT-003: Click button → New tab opens with oUP URL.  
INT-004: Config API → Frontend → Computed URL → Button → Correct chain.  
INT-005: Opportunity without BaseEngagement → No button.  
INT-006: Soft-delete BaseEngagement → Button disappears on refresh.  
INT-007: Create BaseEngagement via EDS → Button appears on refresh.  
INT-008: Change EngagementNumber → Button URL updates on refresh.  
INT-009: Button visible on all opportunity statuses (Draft, Active, Closed).  
INT-010: Button visible on all stages (I&P, GO, NO GO, CANCELLED).  
INT-011: Button on page load → No flicker or delayed appearance.  
INT-012: Multiple opportunities with different BaseEngagements → Each has own URL.  
INT-013: Opportunity detail API response includes baseEngagementNumber.  
INT-014: Config API response includes oupSettings.baseUrl.  
INT-015: Full integration: config + opportunity + BaseEngagement → Correct button.

### Deep Link End-to-End (INT-016–030)

INT-016: External link `/partnerships/opportunities/123` → Loads opportunity 123.  
INT-017: Deep link after login → Correct opportunity.  
INT-018: Deep link from email → Correct opportunity.  
INT-019: Deep link from external system → Correct opportunity.  
INT-020: Deep link with browser refresh → Same opportunity.  
INT-021: Deep link with browser back/forward → Correct navigation.  
INT-022: Deep link to Draft opportunity → Loads if authorized.  
INT-023: Deep link to Closed opportunity → Loads (read-only).  
INT-024: Deep link to NO GO opportunity → Loads (read-only).  
INT-025: Deep link to non-existent opportunity → 404 page.  
INT-026: Deep link without login → Login → Redirect → Opportunity.  
INT-027: Legacy hash URL → Redirect to path-based URL.  
INT-028: Deep link shared between users → Each sees per their permissions.  
INT-029: Deep link in multiple browser tabs → All load correctly.  
INT-030: Deep link during app deployment → May show maintenance or load correctly.

### Cross-Component (INT-031–050)

INT-031: oUP link + opportunity permissions → Button visible if canView.  
INT-032: oUP link + workflow status → Button visible regardless of workflow.  
INT-033: oUP link + AI panel → Both functional.  
INT-034: oUP link + statement section → Both functional.  
INT-035: Deep link + route guard → Guard evaluates before load.  
INT-036: Deep link + lazy loading → Module loaded on demand.  
INT-037: Config + multiple features → oUP config doesn't conflict.  
INT-038: BaseEngagement list endpoint → Returns linked engagements.  
INT-039: Partner BaseEngagements endpoint → Returns correct subset.  
INT-040: BaseEngagement + opportunity export → Engagement number included.  
INT-041: Config API error → Button hidden, no crash.  
INT-042: Opportunity API error → No BaseEngagement data, button hidden.  
INT-043: Database error during BaseEngagement query → API returns gracefully.  
INT-044: oUP system down → User sees oUP error page (not Opportunity+ error).  
INT-045: Network error during config load → Retry or error state.  
INT-046: Deep link with stale cookie → Re-authentication flow.  
INT-047: Deep link with invalid token → Login redirect.  
INT-048: BaseEngagement FK violation → Error handled in API.  
INT-049: Config reload after deployment → Fresh config.  
INT-050: EDS sync failure → BaseEngagement data stale but not broken.

### ValuesController & Environment Integration (INT-051–065)

INT-051: ValuesController + frontend config service → BaseUrl available.  
INT-052: ValuesController in dev environment → Dev BaseUrl returned.  
INT-053: ValuesController in test environment → Test BaseUrl returned.  
INT-054: ValuesController in production → Prod BaseUrl returned.  
INT-055: ValuesController + OpportunityController → Full oUP link chain.  
INT-056: Environment switch (dev→test) → Config reflects new BaseUrl.  
INT-057: ValuesController called on SPA load → Config available early.  
INT-058: ValuesController + multiple opportunity views → Single config fetch.  
INT-059: ValuesController failure + fallback → Opportunity page still loads.  
INT-060: OUPSettings section missing → ValuesController returns null/empty.  
INT-061: ValuesController + Go to oUP button → Correct URL built.  
INT-062: ValuesController + BaseEngagement → EngagementNumber in URL.  
INT-063: ValuesController + environment variable override → Override used.  
INT-064: ValuesController + appsettings hierarchy → Correct env wins.  
INT-065: ValuesController + caching → No repeated config calls.

### Path-Based URLs & Deep Link Integration (INT-066–080)

INT-066: Path-based deep link + Angular router → Correct component.  
INT-067: Path-based deep link + route resolver → Opportunity loaded.  
INT-068: Path-based deep link + auth guard → Login redirect with return.  
INT-069: Path-based deep link from oUP → Correct opportunity.  
INT-070: Path-based deep link + query params → Handled by component.  
INT-071: Path-based URL + SSR (if applicable) → Correct URL.  
INT-072: Path-based deep link + CDN → Correct routing.  
INT-073: Path-based deep link + base href → URL correct.  
INT-074: Path-based deep link + reverse proxy → Correct routing.  
INT-075: Path-based deep link shared between environments → Same format.  
INT-076: Path-based deep link + browser extensions → No interference.  
INT-077: Path-based deep link + PWA → Same behavior.  
INT-078: Path-based deep link + mobile browser → Correct navigation.  
INT-079: Path-based deep link + external OAuth → Return URL preserved.  
INT-080: Path-based deep link + session timeout → Re-auth flow.  

### BaseEngagement & oUP Integration (INT-081–090)

INT-081: BaseEngagement + EDS sync + opportunity view → Button appears.  
INT-082: BaseEngagement + soft delete + opportunity view → Button hidden.  
INT-083: BaseEngagement + multiple per opportunity → First non-deleted used.  
INT-084: BaseEngagement + OpportunityController → baseEngagementNumber in response.  
INT-085: BaseEngagement + ValuesController → Full URL construction.  
INT-086: BaseEngagement + Go to oUP → Correct engagement in oUP.  
INT-087: BaseEngagement migration + existing opportunities → No regression.  
INT-088: BaseEngagement + opportunity permissions → Button respects canView.  
INT-089: BaseEngagement + opportunity export → EngagementNumber in export.  
INT-090: BaseEngagement + deep link + Go to oUP → Both flows work together.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users loading same opportunity → Both see oUP button (or not).  
CON-002: Config API called concurrently → Same result for all.  
CON-003: Opportunity API called concurrently → Same baseEngagementNumber.  
CON-004: BaseEngagement created during opportunity load → May or may not show.  
CON-005: BaseEngagement deleted during opportunity load → May or may not hide.  
CON-006: Concurrent deep link access → Both resolve correctly.  
CON-007: Deep link + concurrent login → One login, one redirect.  
CON-008: Config change + concurrent page load → Transition handled.  
CON-009: EDS sync + concurrent opportunity view → Engagement data consistent.  
CON-010: Multiple tabs with different opportunities → Each has correct oUP link.  
CON-011: Computed signal update during concurrent data load → Correct final URL.  
CON-012: oUP link click during page transition → Opens correct URL.  
CON-013: Deep link click during app loading → Queued until ready.  
CON-014: Concurrent BaseEngagement queries → Independent results.  
CON-015: Config cache + concurrent invalidation → Fresh data served.  
CON-016: Browser history + concurrent deep links → History stack correct.  
CON-017: Route guard + concurrent auth check → Single evaluation.  
CON-018: Concurrent EDS sync updates → Last write for BaseEngagement wins.  
CON-019: Config API rate limiting → Cached, not repeated.  
CON-020: Multiple opportunity loads → BaseEngagement queries independent.  
CON-021: Deep link + concurrent page navigation → Router resolves one.  
CON-022: oUP tab opened + Opportunity+ tab refresh → Both independent.  
CON-023: Config signal update + URL recomputation → Atomic.  
CON-024: BaseEngagement signal update + URL recomputation → Atomic.  
CON-025: Concurrent access to BaseEngagement list API → Consistent.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `oupEngagementUrl` computed with both inputs → Returns formatted URL.  
UNT-002: `oupEngagementUrl` with null baseUrl → Returns null.  
UNT-003: `oupEngagementUrl` with null engagementNumber → Returns null.  
UNT-004: `oupEngagementUrl` with both null → Returns null.  
UNT-005: `oupEngagementUrl` with empty baseUrl → Returns null (falsy).  
UNT-006: `oupEngagementUrl` with empty engagementNumber → Returns null (falsy).  
UNT-007: URL format string interpolation correct.  
UNT-008: URL path segments: `/{number}/engagement/overview` → Correct.  
UNT-009: Config response parsing → `oupSettings.baseUrl` extracted.  
UNT-010: Opportunity response parsing → `baseEngagementNumber` extracted.  
UNT-011: BaseEngagement query filter → `OpportunityId == id && !IsDeleted`.  
UNT-012: BaseEngagement `FirstOrDefaultAsync` → Returns first match or null.  
UNT-013: Deep link route pattern → `/partnerships/opportunities/:id`.  
UNT-014: Route parameter extraction → `:id` parsed to number.  
UNT-015: Route guard authentication check → Returns true/false.  
UNT-016: Route guard authorization check → Returns true/false.  
UNT-017: Button visibility condition → `oupEngagementUrl() !== null`.  
UNT-018: Button href binding → Set to computed URL.  
UNT-019: Button target binding → `_blank`.  
UNT-020: Config signal default → null before load.  
UNT-021: BaseEngagement number signal default → null before load.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Config API response < 200ms.  
PRF-002: Opportunity API (including BaseEngagement query) < 500ms.  
PRF-003: BaseEngagement `FirstOrDefaultAsync` < 50ms.  
PRF-004: Computed URL calculation < 1ms.  
PRF-005: Button render after data load < 50ms.  
PRF-006: Deep link route resolution < 100ms.  
PRF-007: Route guard evaluation < 50ms.  
PRF-008: Login redirect + return < 3s.  
PRF-009: Config caching → Second load < 5ms.  
PRF-010: Page load with oUP button < 500ms total.  
PRF-011: Deep link page load < 1s total.  
PRF-012: Multiple opportunity loads with BaseEngagement queries → Each < 500ms.  
PRF-013: Config API under load (100 concurrent) → < 500ms each.  
PRF-014: BaseEngagement index lookup → Index scan, not table scan.  
PRF-015: Signal computation chain → Reactive, no unnecessary recalculations.  
PRF-016: oUP tab opening → Immediate (no app processing).

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 100 concurrent opportunity loads with oUP links → All correct.  
LDT-002: 50 concurrent deep link accesses → All resolve.  
LDT-003: 200 concurrent config API requests → All return correct.  
LDT-004: Sustained deep link usage (100/hour) → Stable.  
LDT-005: Spike: 50 deep links in 5 seconds → All resolve.  
LDT-006: 100 concurrent BaseEngagement queries → All succeed.  
LDT-007: Config API under sustained load → Cached, stable.  
LDT-008: Recovery after config API failure → Config cached, button still works.  
LDT-009: Recovery after database failure → BaseEngagement queries resume.  
LDT-010: Recovery after oUP outage → Buttons still show, oUP recovers independently.

---

## Status: Ready for Implementation
