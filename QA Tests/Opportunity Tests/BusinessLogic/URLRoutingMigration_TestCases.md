# URL Routing Migration (Hash → Path) — Comprehensive Test Cases

**Component:** Path-Based URL Routing Migration  
**Test Plan:** `QA Tests/Test Plans/url-routing-migration-plan.md`  
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
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Ratio Compliance Checks (MANDATORY)

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| **N ≥ 3P** | Negative ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| **E ≥ 3P** | Edge/Boundary ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| **F ≥ 3P** | Functional ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |
| **I ≥ 3P** | Integration ≥ 3 × Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| **Direct URL navigation** | POS-001–010, NEG-001–015, BND-001–015, FUN-001–010 |
| **Deep linking to entities** | POS-011–018, NEG-011–020, BND-016–030, FUN-011–020 |
| **Browser history (back/forward)** | POS-019–023, NEG-021–030, BND-031–045, FUN-021–035 |
| **Page refresh** | POS-024–028, NEG-031–045, BND-046–060, FUN-036–045 |
| **Legacy hash URL handling** | POS-029–030, NEG-031–045, BND-061–070, FUN-036–050 |
| **Navigation components** | —, NEG-046–060, BND-071–080, FUN-051–070 |
| **External links (email, oUP)** | NEG-061–070, BND-081–090, FUN-071–080 |
| **Route guards & auth** | FUN-046–050, INT-011–020 |
| **Route config & strategy** | NEG-071–080, FUN-081–090, INT-051–070 |
| **Server & deployment** | NEG-081–090, INT-071–090 |

---

## §1 Positive Tests (Happy Path) — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### Direct URL Navigation (POS-001–010)

**POS-001: Navigate to homepage via `/`**  
Steps: Enter `http://localhost:4200/` in browser address bar.  
Expected: Dashboard/homepage loads correctly. URL remains `/`. No `#` in URL.

**POS-002: Navigate to partner list via `/partnerships/partners`**  
Steps: Enter `/partnerships/partners` in address bar.  
Expected: Partner list page loads. URL is `/partnerships/partners`.

**POS-003: Navigate to contact list via `/partnerships/contacts`**  
Expected: Contact list loads correctly.

**POS-004: Navigate to interaction list via `/partnerships/interactions`**  
Expected: Interaction list loads correctly.

**POS-005: Navigate to opportunity list via `/partnerships/opportunities`**  
Expected: Opportunity list loads correctly.

**POS-006: Navigate to login via `/login`**  
Expected: Login page loads (if unauthenticated) or redirects to home (if authenticated).

**POS-007: Navigate to admin area via `/admin`**  
Precondition: User has admin permissions.  
Expected: Admin page loads correctly.

**POS-008: Navigate to AI assistant via `/ai`**  
Expected: AI assistant page loads.

**POS-009: Navigate to search via `/search`**  
Expected: Search page loads.

**POS-010: URL bar shows path-based URL (no hash) on all pages**  
Steps: Navigate through sidebar to each main section.  
Expected: URL bar never contains `#` or `/#/`.

### Deep Linking (POS-011–018)

**POS-011: Deep link to partner detail `/partnerships/partners/1`**  
Expected: Partner detail page loads for partner ID 1.

**POS-012: Deep link to contact detail `/partnerships/contacts/1`**  
Expected: Contact detail page loads.

**POS-013: Deep link to interaction detail `/partnerships/interactions/1`**  
Expected: Interaction detail page loads.

**POS-014: Deep link to opportunity detail `/partnerships/opportunities/1`**  
Expected: Opportunity detail page loads.

**POS-015: Deep link shared via copy-paste to another user**  
Steps: User A copies URL from address bar → Sends to User B → User B pastes in browser.  
Expected: Same page loads for User B (after auth).

**POS-016: Deep link from email notification**  
Precondition: User receives email with opportunity link.  
Steps: Click link in email.  
Expected: Navigates to correct opportunity detail page.

**POS-017: Deep link from oUP integration**  
Precondition: oUP system generates Opportunity+ deep link.  
Expected: Link navigates to correct opportunity.

**POS-018: Deep link with query parameters preserved**  
Steps: Navigate to `/partnerships/partners?filter=active`.  
Expected: Page loads with filter applied, query params preserved.

### Browser History (POS-019–023)

**POS-019: Browser back button returns to previous page**  
Steps: Navigate Home → Partners → Partner Detail → Click Back.  
Expected: Returns to Partners list. URL updates correctly.

**POS-020: Browser forward button returns to next page**  
Steps: After POS-019, click Forward.  
Expected: Returns to Partner Detail. URL updates correctly.

**POS-021: Multiple back navigations through history stack**  
Steps: Navigate through 5 pages → Click Back 4 times.  
Expected: Each click shows correct previous page with correct URL.

**POS-022: Back button from entity detail returns to list**  
Steps: Navigate to partner detail → Click Back.  
Expected: Returns to partner list (not homepage).

**POS-023: History entry for each navigation action**  
Steps: Navigate through 3 pages → Check `window.history.length`.  
Expected: History length increased by 3.

### Page Refresh (POS-024–028)

**POS-024: F5 on homepage preserves page**  
Steps: Navigate to `/` → Press F5.  
Expected: Homepage reloads at `/`.

**POS-025: F5 on partner detail preserves page**  
Steps: Navigate to `/partnerships/partners/1` → Press F5.  
Expected: Same partner detail page reloads.

**POS-026: Ctrl+R on opportunity list preserves page**  
Expected: Opportunity list reloads at correct URL.

**POS-027: Refresh on admin page preserves page**  
Expected: Admin page reloads correctly.

**POS-028: Refresh preserves scroll position (in-memory scrolling)**  
Steps: Scroll down on partner list → Refresh.  
Expected: Page reloads (scroll position may reset but route is preserved).

### Legacy Hash URL Handling (POS-029–030)

**POS-029: Legacy hash URL `/#/partnerships/partners` redirected**  
Steps: Enter `http://localhost:4200/#/partnerships/partners` in address bar.  
Expected: Redirected to `/partnerships/partners` (hash stripped) or page loads correctly.

**POS-030: Legacy hash URL `/#/login` handled**  
Expected: Redirected to `/login` or login page loads.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Invalid Route Paths (NEG-001–015)

NEG-001: Navigate to non-existent route `/nonexistent` → 404 Not Found page.  
NEG-002: Navigate to `/partnerships/invalid` → 404 Not Found page.  
NEG-003: Navigate to `/partnerships/partners/invalid-id` (non-numeric) → Error page or redirect.  
NEG-004: Navigate to `/partnerships/partners/-1` (negative ID) → 404 or error.  
NEG-005: Navigate to `/partnerships/partners/0` → 404 or error.  
NEG-006: Navigate to `/partnerships/partners/999999999` (non-existent) → 404.  
NEG-007: Navigate to `//partnerships/partners` (double slash) → Handled gracefully.  
NEG-008: Navigate to `/partnerships//partners` (double slash mid-path) → Handled.  
NEG-009: Navigate to `/PARTNERSHIPS/PARTNERS` (wrong case) → Case-sensitive handling.  
NEG-010: Navigate to `/partnerships/partners/` (trailing slash) → Works or redirects.  
NEG-011: Navigate to deeply nested invalid path `/a/b/c/d/e/f/g` → 404.  
NEG-012: Navigate to path with spaces `/partnerships/partners/ 1` → Error handled.  
NEG-013: Navigate to path with special chars `/partnerships/partners/<script>` → Sanitized.  
NEG-014: Navigate to path with encoded chars `/partnerships/partners/%3Cscript%3E` → Sanitized.  
NEG-015: Navigate to path with query string on 404 route `/nonexistent?param=value` → 404.

### Authentication & Authorization (NEG-016–030)

NEG-016: Unauthenticated access to `/partnerships/partners` → Redirect to `/login`.  
NEG-017: Unauthenticated access to `/partnerships/partners/1` → Redirect to `/login`.  
NEG-018: Unauthenticated access to `/admin` → Redirect to `/login`.  
NEG-019: Login redirect preserves intended URL → After login, navigates to `/partnerships/partners`.  
NEG-020: Login redirect preserves deep link → After login, navigates to `/partnerships/partners/1`.  
NEG-021: Unauthorized access to admin route → Access Denied page.  
NEG-022: Unauthorized access to AI features → Access Denied page.  
NEG-023: Token expiry during navigation → Redirect to `/login`.  
NEG-024: Session timeout on protected route → Redirect to `/login`.  
NEG-025: Login with invalid credentials → Stays on `/login` with error.  
NEG-026: Navigate to `/login` when already authenticated → Redirect to `/`.  
NEG-027: Logout clears route history → Back button doesn't access protected routes.  
NEG-028: Browser back after logout → Redirect to `/login`.  
NEG-029: Direct URL to route requiring specific permission → Access Denied if no permission.  
NEG-030: Manipulated URL parameter (IDOR) → Authorization check prevents data access.

### Hash URL Rejection/Handling (NEG-031–045)

NEG-031: Hash URL `/#/partnerships/partners` does not create double navigation.  
NEG-032: Hash URL `/#/partnerships/partners/1` → Single redirect, no loop.  
NEG-033: Hash URL `/#/admin` → Stripped and redirected.  
NEG-034: Hash URL `/#/login` → Handled without infinite redirect.  
NEG-035: Hash URL with query params `/#/partners?filter=x` → Params preserved after strip.  
NEG-036: Mixed URL `/#/partnerships/partners#section` → Double hash handled.  
NEG-037: Fragment-only URL `/#section` → Not confused with hash routing.  
NEG-038: Hash URL in `window.location.hash` manipulation via JS → No client-side exploit.  
NEG-039: Old Angular hash format `#!` (hashbang) → Handled gracefully.  
NEG-040: Repeated hash stripping `/#/#/partnerships` → Single clean redirect.  
NEG-041: Hash URL with port number `http://localhost:4200/#/partners` → Port preserved, hash stripped.  
NEG-042: Hash URL with HTTPS `https://app.example.com/#/partners` → Scheme preserved.  
NEG-043: Hash URL with basic auth in URL → Rejected.  
NEG-044: Hash URL longer than 2048 chars → Handled without crash.  
NEG-045: Hash URL with unicode path segments → Encoded correctly.

### Navigation Error States (NEG-046–060)

NEG-046: Network error during route change → Error message, no white screen.  
NEG-047: Server 500 error on route data load → Error page shown.  
NEG-048: Route guard throws exception → Graceful fallback.  
NEG-049: Lazy-loaded module fails to download → Retry or error message.  
NEG-050: Route resolver rejects → Error page or redirect.  
NEG-051: Navigation cancelled by guard → Previous route preserved.  
NEG-052: Navigation to same route → No unnecessary reload.  
NEG-053: Rapid navigation (click 10 links in 1 second) → Final route loads correctly.  
NEG-054: Navigation during ongoing HTTP request → Request cancelled or completed.  
NEG-055: Window.location.href set to invalid path → 404 page.  
NEG-056: window.history.pushState with invalid state → Angular router handles.  
NEG-057: popstate event with unexpected state → Graceful handling.  
NEG-058: Route change during form submission → Confirmation dialog or form preserved.  
NEG-059: Route with unsaved changes → Navigate away warning.  
NEG-060: Fragment navigation within same page `#section` → Scrolls, doesn't re-route.

### External Link Failures (NEG-061–070)

NEG-061: Email link with old hash format → Stripped and redirected.  
NEG-062: Email link with wrong domain → Shows error.  
NEG-063: oUP link with old hash format → Handled.  
NEG-064: oUP link with incorrect path structure → 404.  
NEG-065: Notification link to deleted entity → 404.  
NEG-066: Notification link to entity user lacks permission for → Access Denied.  
NEG-067: Bookmark to route that was renamed → 404.  
NEG-068: Shared link with expired session → Redirect to login, then to link.  
NEG-069: Link with additional unexpected query params → Ignored, page loads.  
NEG-070: Link with malformed URL encoding → Decoded safely or error.

### Route Config & Migration Failures (NEG-071–090)

NEG-071: Invalid `returnUrl` with open redirect `/login?returnUrl=https://evil.com` → Rejected or sanitized.  
NEG-072: `returnUrl` with path traversal `/login?returnUrl=/../../../etc/passwd` → Rejected.  
NEG-073: `returnUrl` with javascript protocol → Rejected.  
NEG-074: Route to soft-deleted entity `/partnerships/partners/123` (IsDeleted=true) → 404 or access denied.  
NEG-075: Route with XSS in query param `/partnerships/partners?name=<script>alert(1)</script>` → Sanitized.  
NEG-076: Route with invalid entity type `/partnerships/invalidtype/1` → 404.  
NEG-077: Route guard returns false without redirect config → Graceful fallback.  
NEG-078: Route resolver timeout → Error page or retry.  
NEG-079: Lazy module chunk 404 (deleted/moved) → Error message, no white screen.  
NEG-080: Circular redirect A→B→A in route config → Detected, no infinite loop.  
NEG-081: Server returns 404 for path-based route (History API fallback misconfigured) → User sees app error page.  
NEG-082: Base href mismatch (app served from subpath, base is `/`) → Routes fail; document config.  
NEG-083: Route with duplicate path params → Handled or error.  
NEG-084: Route with conflicting path and matrix params → Defined behavior.  
NEG-085: Invalid pagination param `/partnerships/partners?page=-1` → Default or error.  
NEG-086: Invalid sort param `/partnerships/partners?sort=invalid` → Default or error.  
NEG-087: Route with encoded null byte `%00` in path → Sanitized or 404.  
NEG-088: Route with CRLF in path (header injection attempt) → Rejected.  
NEG-089: Route to entity that was soft-deleted during navigation → 404 or stale data handled.  
NEG-090: Route with empty path segment `/partnerships//partners` → Normalized or 404.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### URL Length Boundaries (BND-001–015)

BND-001: URL at exactly 1 char path `/` → Homepage loads.  
BND-002: URL at 100 chars total → Loads normally.  
BND-003: URL at 2000 chars (typical browser limit) → Loads normally.  
BND-004: URL at 2048 chars (IE/Edge limit) → Loads or graceful error.  
BND-005: URL at 8192 chars (Chrome limit) → Loads or graceful error.  
BND-006: URL at 65536 chars → Browser truncates or server rejects.  
BND-007: Query string at exactly 0 chars → Route loads without params.  
BND-008: Query string at 1000 chars → Params parsed correctly.  
BND-009: Query string at 2000 chars → Params parsed correctly.  
BND-010: Path with 1 segment `/partners` → Routes correctly.  
BND-011: Path with 2 segments `/partnerships/partners` → Routes correctly.  
BND-012: Path with 3 segments `/partnerships/partners/1` → Routes correctly.  
BND-013: Path with 10 segments (deep nesting) → 404 or last valid route.  
BND-014: Entity ID at minimum valid (1) → Detail page loads.  
BND-015: Entity ID at maximum integer → Detail page loads (if exists) or 404.

### Unicode & Encoding Boundaries (BND-016–030)

BND-016: URL with ASCII path `/partnerships/partners` → Works.  
BND-017: URL with encoded space `%20` in path → Decoded correctly.  
BND-018: URL with `+` in path → Handled as literal or space.  
BND-019: URL with encoded slash `%2F` in path → Not confused with path separator.  
BND-020: URL with Unicode chars in query string `?name=José` → Encoded correctly.  
BND-021: URL with emoji in query string `?q=🏢` → Encoded/decoded correctly.  
BND-022: URL with Chinese characters in query → Encoded correctly.  
BND-023: URL with Arabic characters in query → Encoded correctly.  
BND-024: URL with null byte `%00` → Sanitized.  
BND-025: URL with CRLF `%0D%0A` → Sanitized (prevent header injection).  
BND-026: URL with backslash `\` → Converted to `/` or rejected.  
BND-027: URL with dot segments `/../` → Resolved or rejected (path traversal).  
BND-028: URL with double encoding `%2520` → Single decode, no double decode.  
BND-029: URL with fragment `#section` → Fragment preserved, routing unaffected.  
BND-030: URL with mixed case route `/Partnerships/Partners` → Case handling defined.

### Browser History Boundaries (BND-031–045)

BND-031: History stack with 1 entry → Back button disabled/goes to browser default.  
BND-032: History stack with 10 entries → All back/forward work.  
BND-033: History stack with 100 entries → Performance acceptable.  
BND-034: History stack with entries from different domains → Back exits app.  
BND-035: Back button from first page in session → Exits app to browser default.  
BND-036: Forward button at latest page → No action.  
BND-037: Back then navigate to new page → Forward history cleared.  
BND-038: History entry for route with params → Params preserved on back.  
BND-039: History entry for route with scroll position → Scroll position handled.  
BND-040: History replace (not push) → Back skips replaced entry.  
BND-041: Rapid back/forward clicking (20 times in 2 seconds) → Stable state.  
BND-042: Back button during page load → Previous page shown.  
BND-043: Forward button during page load → Next page shown.  
BND-044: History navigation to page that now requires auth → Redirect to login.  
BND-045: History with mix of path and hash entries (migration period) → Both handled.

### Page Refresh Boundaries (BND-046–060)

BND-046: Refresh on `/` → Homepage reloads.  
BND-047: Refresh on `/partnerships/partners` → List reloads.  
BND-048: Refresh on `/partnerships/partners/1` → Detail reloads with correct data.  
BND-049: Refresh on `/admin` → Admin page reloads.  
BND-050: Refresh on `/login` → Login page reloads.  
BND-051: Refresh on route with query params → Params preserved.  
BND-052: Refresh on route with fragment → Fragment preserved.  
BND-053: Refresh after form partially filled → Form state handled (warn or preserve).  
BND-054: Refresh during API call → Call cancelled, page reloads clean.  
BND-055: Refresh with expired session → Redirect to login.  
BND-056: Hard refresh (Ctrl+Shift+R) → Bypasses cache, page loads.  
BND-057: Refresh on 404 page → 404 page reloads.  
BND-058: Refresh on Access Denied page → Access Denied reloads.  
BND-059: Refresh 10 times rapidly → App stays stable.  
BND-060: Refresh on page with lazy-loaded module → Module reloads.

### Cross-Browser Boundaries (BND-061–070)

BND-061: Chrome: all routes work with path-based URLs.  
BND-062: Firefox: all routes work with path-based URLs.  
BND-063: Edge: all routes work with path-based URLs.  
BND-064: Safari: all routes work with path-based URLs.  
BND-065: Chrome mobile: routes work on mobile viewport.  
BND-066: Safari iOS: routes work on iPhone.  
BND-067: Incognito/Private mode: routes work without cookies.  
BND-068: Browser with JavaScript disabled → Graceful fallback or message.  
BND-069: Browser with cookies disabled → Auth redirect handled.  
BND-070: Browser with strict content security policy → Routes still work.

### Route & Param Boundaries (BND-071–090)

BND-071: URL at exactly 2047 chars → Loads or defined behavior.  
BND-072: URL at exactly 2049 chars → Handled per browser.  
BND-073: Entity ID at exactly 2147483647 (max int32) → Loads or 404.  
BND-074: Entity ID at exactly 2147483648 (overflow) → Handled (null/error).  
BND-075: Query param count at 0 → Route loads.  
BND-076: Query param count at 50 → All parsed or limit applied.  
BND-077: Query param value at 500 chars → Parsed correctly.  
BND-078: Path segment at 1 char → Routes correctly.  
BND-079: Path segment at 500 chars → Routes or 404.  
BND-080: Fragment at 0 chars → No fragment, routing unaffected.  
BND-081: Fragment at 2000 chars → Preserved for in-page scroll.  
BND-082: Route with exactly 1 query param → Param applied.  
BND-083: Route with mixed encoding (UTF-8 + percent) → Decoded correctly.  
BND-084: Empty path segment in middle `/partnerships//partners` → Normalized.  
BND-085: Route with only numeric path segments → Routes correctly.  
BND-086: Route with alphanumeric ID `/partnerships/partners/abc123` → Error or custom handling.  
BND-087: History entry at browser limit (e.g., 50) → No crash.  
BND-088: Route param at min length (1 char) → Parsed.  
BND-089: Route param at max practical length → Parsed or truncated.  
BND-090: Route with BOM in path (UTF-8 BOM) → Stripped or rejected.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### URL Format Rules (FUN-001–010)

FUN-001: All application URLs use path-based format (no `#`).  
FUN-002: URL does not contain `/#/` anywhere.  
FUN-003: URL does not contain `#!` (hashbang).  
FUN-004: URL preserves scheme (http/https).  
FUN-005: URL preserves port number.  
FUN-006: URL preserves hostname.  
FUN-007: URL path starts with `/`.  
FUN-008: Query parameters appended with `?` not `#?`.  
FUN-009: Multiple query params joined with `&`.  
FUN-010: Fragment identifiers (for in-page scrolling) use single `#`.

### Deep Link Rules (FUN-011–020)

FUN-011: Partner detail deep link loads correct partner data.  
FUN-012: Contact detail deep link loads correct contact data.  
FUN-013: Interaction detail deep link loads correct interaction data.  
FUN-014: Opportunity detail deep link loads correct opportunity data.  
FUN-015: Deep link URL can be copied from address bar and shared.  
FUN-016: Browser back from deep link returns to referrer.  
FUN-017: Page title updates for deep linked page.  
FUN-018: Active sidebar item highlighted for deep linked page.  
FUN-019: Active tab highlighted for deep linked page.  
FUN-020: Breadcrumb shows correct hierarchy for deep linked page.

### Navigation Component Rules (FUN-021–035)

FUN-021: Sidebar "Partners" link navigates to `/partnerships/partners`.  
FUN-022: Sidebar "Contacts" link navigates to `/partnerships/contacts`.  
FUN-023: Sidebar "Interactions" link navigates to `/partnerships/interactions`.  
FUN-024: Sidebar "Opportunities" link navigates to `/partnerships/opportunities`.  
FUN-025: Sidebar "Dashboard" link navigates to `/`.  
FUN-026: Page refresh on `/partnerships/partners` reloads partner list.  
FUN-027: Page refresh on `/partnerships/contacts/5` reloads contact #5.  
FUN-028: Click partner row → URL changes to `/partnerships/partners/{id}`.  
FUN-029: Click contact row → URL changes to `/partnerships/contacts/{id}`.  
FUN-030: Click opportunity row → URL changes to `/partnerships/opportunities/{id}`.  
FUN-031: Tab navigation within entity detail updates URL correctly.  
FUN-032: "Create New" dialog does NOT change URL (dialog overlay).  
FUN-033: Close dialog returns to same URL.  
FUN-034: Pagination does NOT change route (uses query params or state).  
FUN-035: Sort/filter does NOT change route (uses query params or state).

### Legacy Hash Handling Rules (FUN-036–045)

FUN-036: `auth.helper.ts` strips `/#/` prefix before navigation.  
FUN-037: `base.page.ts` strips `#/` prefix before navigation.  
FUN-038: Hash URL stripped on first navigation, not on every navigation.  
FUN-039: No redirect loop when visiting hash URL.  
FUN-040: No duplicate history entries from hash stripping.  
FUN-041: Email links generated by system use path-based format.  
FUN-042: oUP integration links use path-based format.  
FUN-043: Notification links use path-based format.  
FUN-044: API response URLs (if any) use path-based format.  
FUN-045: Error page "return home" link uses path-based format.

### Route Guard Rules (FUN-046–050)

FUN-046: Auth guard redirects to `/login` for unauthenticated users.  
FUN-047: Auth guard stores intended URL for post-login redirect.  
FUN-048: Permission guard shows Access Denied for unauthorized routes.  
FUN-049: Route guard executes on direct URL access (not just navigation).  
FUN-050: Route guard executes on page refresh.

### Route Config & Strategy Rules (FUN-051–070)

FUN-051: Router uses PathLocationStrategy (not HashLocationStrategy).  
FUN-052: Base href matches deployment path (e.g., `/` for root).  
FUN-053: Wildcard route `**` catches unknown paths → 404 page.  
FUN-054: Redirect from `/` to default route (if configured) works.  
FUN-055: Child routes resolve with correct parent path.  
FUN-056: Lazy-loaded routes load on first access.  
FUN-057: Route with `pathMatch: 'full'` matches exactly.  
FUN-058: Route with `pathMatch: 'prefix'` matches prefix.  
FUN-059: Route `redirectTo` preserves query params when configured.  
FUN-060: Route `data` property accessible in component.  
FUN-061: Route `resolve` runs before component activation.  
FUN-062: `canActivate` guard blocks navigation when returning false.  
FUN-063: `canDeactivate` guard prompts on unsaved changes.  
FUN-064: `canActivateChild` applies to child routes.  
FUN-065: Route params inherited by child components.  
FUN-066: `scrollPositionRestoration` works on navigation.  
FUN-067: `anchorScrolling` scrolls to fragment on load.  
FUN-068: `initialNavigation` runs on app bootstrap.  
FUN-069: `urlUpdateStrategy` updates URL correctly.  
FUN-070: `malformedUrlHandling` rejects invalid URLs gracefully.

### Migration & Compatibility Rules (FUN-071–090)

FUN-071: All `routerLink` directives use path-based paths.  
FUN-072: All `router.navigate()` calls use path-based paths.  
FUN-073: All `router.navigateByUrl()` calls use path-based URLs.  
FUN-074: `RouterLink` with `[routerLink]` array generates correct href.  
FUN-075: `RouterLink` with `queryParams` generates correct URL.  
FUN-076: `RouterLink` with `fragment` generates correct URL.  
FUN-077: `ActivatedRoute` provides correct path params.  
FUN-078: `ActivatedRoute` provides correct query params.  
FUN-079: `Router.events` emits correct navigation events.  
FUN-080: `Router.url` returns path-based URL.  
FUN-081: `Location.path()` returns path without hash.  
FUN-082: `Location.go()` navigates to path-based URL.  
FUN-083: `Location.replaceState()` updates history correctly.  
FUN-084: `Location.prepareExternalUrl()` returns path-based URL.  
FUN-085: `APP_BASE_HREF` matches server config.  
FUN-086: Server returns index.html for all routes (History API fallback).  
FUN-087: No `useHash: true` in router config.  
FUN-088: `provideRouter()` or `RouterModule` uses path strategy.  
FUN-089: Deep link from external system uses `/partnerships/...` format.  
FUN-090: Bookmark migration: old hash URLs redirect to path URLs.

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: 3×30 = 90** | ✅ COMPLIANT

### Navigation + Data Loading (INT-001–010)

INT-001: Navigate to partner list → Partners loaded from API.  
INT-002: Navigate to partner detail → Partner data loaded by ID.  
INT-003: Navigate to contact list → Contacts loaded.  
INT-004: Navigate to opportunity list → Opportunities loaded.  
INT-005: Deep link to partner → Correct API call made with ID.  
INT-006: Deep link to opportunity → All tabs/sections load.  
INT-007: Navigate between list and detail → Data refreshed each time.  
INT-008: Navigate to same route → No unnecessary API calls.  
INT-009: Navigate with query params → API called with filter params.  
INT-010: Route change cancels pending API calls for previous route.

### Authentication + Routing (INT-011–020)

INT-011: Login → Redirect to intended URL.  
INT-012: Login → Deep link redirect preserves entity ID.  
INT-013: Token refresh during navigation → Seamless transition.  
INT-014: Logout → All protected routes inaccessible.  
INT-015: Session timeout → Next navigation redirects to login.  
INT-016: Login from `/login?returnUrl=/partnerships/partners/1` → Redirects after auth.  
INT-017: OAuth callback URL works with path-based routing.  
INT-018: Multi-tab login → Both tabs navigate correctly.  
INT-019: Concurrent logout in one tab → Other tab redirects on next action.  
INT-020: Role change → Route permissions updated on next navigation.

### Notifications + Links (INT-021–030)

INT-021: Email notification link → Opens correct opportunity page.  
INT-022: In-app notification click → Navigates to correct entity.  
INT-023: Actions Required card link → Opens correct opportunity.  
INT-024: oUP deep link → Opens correct opportunity.  
INT-025: Shared URL from one user → Same page for another user (with permission).  
INT-026: Link generated by backend → Uses path-based format.  
INT-027: Workflow notification link → Opens opportunity at correct section.  
INT-028: Error notification link → Opens error details page.  
INT-029: Notification with expired entity → Shows 404 or "not found".  
INT-030: Notification with deleted entity → Shows 404 or "not found".

### Search + Navigation (INT-031–040)

INT-031: Search result click → Navigates to entity detail with correct URL.  
INT-032: Advanced search with filters → URL reflects search state.  
INT-033: Back from search result → Returns to search results.  
INT-034: Cross-entity search result → Navigates to correct entity type.  
INT-035: Search with empty results → No navigation error.  
INT-036: Listview filter change → URL query params updated.  
INT-037: Listview sort change → URL query params updated (if applicable).  
INT-038: Listview pagination → Route preserved, page state updated.  
INT-039: Create entity from list → Navigate to new entity detail.  
INT-040: Delete entity → Navigate back to list.

### Error Recovery + Navigation (INT-041–050)

INT-041: API error on route → Error page with "Go Home" link using path-based URL.  
INT-042: 404 page → "Return to Dashboard" link works.  
INT-043: Access Denied page → "Return to Dashboard" link works.  
INT-044: Network error → Retry navigation successful.  
INT-045: Server restart during navigation → Page reloads after reconnect.  
INT-046: Multiple rapid navigations → Final state correct.  
INT-047: Navigation during file upload → Upload preserved or warned.  
INT-048: Navigation during form save → Save completes before navigation.  
INT-049: Error boundary catches route-related errors → Error page shown.  
INT-050: Angular router error event logged → Diagnostics available.

### Routing + Services Integration (INT-051–070)

INT-051: Routing + HttpClient interceptor → Auth header on API calls for protected routes.  
INT-052: Routing + Auth service → Login state sync with route guards.  
INT-053: Routing + Permission service → Permission check before route activation.  
INT-054: Routing + Partner API → Correct partner loaded for `/partnerships/partners/{id}`.  
INT-055: Routing + Opportunity API → Correct opportunity for `/partnerships/opportunities/{id}`.  
INT-056: Routing + Contact API → Correct contact for `/partnerships/contacts/{id}`.  
INT-057: Routing + Interaction API → Correct interaction for `/partnerships/interactions/{id}`.  
INT-058: Routing + Admin API → Admin data loaded for `/admin` routes.  
INT-059: Routing + AI API → AI context loaded for `/ai` routes.  
INT-060: Routing + Search API → Search results for `/search` with query.  
INT-061: Routing + Notification service → Notification links use path-based URLs.  
INT-062: Routing + Breadcrumb service → Breadcrumb updates on route change.  
INT-063: Routing + Sidebar state → Active item reflects current route.  
INT-064: Routing + Topbar state → Topbar reflects current route.  
INT-065: Routing + Tab state → Tab selection reflects route params.  
INT-066: Routing + Listview state → Filter/sort/pagination in URL.  
INT-067: Routing + Form state → Unsaved changes guard on navigation.  
INT-068: Routing + Dialog state → Dialog close preserves route.  
INT-069: Routing + oUP integration → oUP links use path-based format.  
INT-070: Routing + Email template service → Email links use path-based format.

### Routing + Entity & Workflow Integration (INT-071–090)

INT-071: Routing + Entity create → New entity ID in URL after create.  
INT-072: Routing + Entity update → URL unchanged, data refreshed.  
INT-073: Routing + Entity delete → Navigate to list, URL updated.  
INT-074: Routing + Soft delete → Entity 404 for soft-deleted ID.  
INT-075: Routing + Search result → Result click navigates with path URL.  
INT-076: Routing + Filter change → Query params updated, API called.  
INT-077: Routing + Sort change → Query params updated (if applicable).  
INT-078: Routing + Pagination → Page param in URL or state.  
INT-079: Routing + Export → Export does not change route.  
INT-080: Routing + Import → Import completion navigates correctly.  
INT-081: Routing + Audit trail → Audit links use path-based URLs.  
INT-082: Routing + Error boundary → Error recovery link uses path URL.  
INT-083: Routing + Retry logic → Retry preserves intended route.  
INT-084: Routing + Cache invalidation → Stale data refreshed on route.  
INT-085: Routing + Logout flow → Post-logout route is `/login`.  
INT-086: Routing + Token refresh → Navigation continues during refresh.  
INT-087: Routing + Session expiry → Redirect to login with returnUrl.  
INT-088: Routing + Role change → Re-evaluation on next navigation.  
INT-089: Routing + Permission change → Access Denied if permission revoked.  
INT-090: Routing + Server History API fallback → All routes serve index.html.

---

## §6 Security Tests — OUT OF SCOPE

> Security testing is handled by the Infrastructure and Security teams per project policy.

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: 10 users navigating to same route simultaneously → All see correct page.  
CON-002: 10 users navigating to different routes simultaneously → All see correct pages.  
CON-003: Same user in 5 tabs navigating to different routes → Each tab independent.  
CON-004: Simultaneous login from two browsers → Both establish correct routes.  
CON-005: Navigation during concurrent API response → Route not corrupted.  
CON-006: Back button during pending navigation → Clean state.  
CON-007: Multiple route guard checks in parallel → All resolve correctly.  
CON-008: Concurrent lazy module loads → Both succeed.  
CON-009: Rapid sidebar clicks (5 different items in 1 second) → Final route correct.  
CON-010: Notification click during page transition → Correct page shown.  
CON-011: Deep link + sidebar click race → One wins cleanly.  
CON-012: Page refresh during route transition → Clean reload.  
CON-013: Two tabs: login in one, navigate in other → Auth state consistent.  
CON-014: Two tabs: logout in one, navigate in other → Redirect to login.  
CON-015: Concurrent history manipulation → No state corruption.  
CON-016: Route change + WebSocket message → Both processed.  
CON-017: Browser prefetch of route + user navigation → No duplicate loads.  
CON-018: Service worker caching + route change → Correct version served.  
CON-019: Concurrent URL manipulation via address bar + sidebar → One wins.  
CON-020: Tab switch + back button → Correct history per tab.  
CON-021: Multiple redirects in chain (login → intended → sub-route) → Final destination correct.  
CON-022: Concurrent route resolver execution → Both complete.  
CON-023: Navigation event during destruction of previous component → No error.  
CON-024: Parallel lazy chunk downloads → Both succeed.  
CON-025: Race between canDeactivate and canActivate guards → Clean outcome.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (UNT-001–005)

UNT-001: `isValidRoute('/partnerships/partners')` returns true.  
UNT-002: `isValidRoute('/nonexistent')` returns false.  
UNT-003: `isValidRoute('/#/partnerships/partners')` returns false (hash format).  
UNT-004: `stripHash('/#/partnerships/partners')` returns `/partnerships/partners`.  
UNT-005: `stripHash('/partnerships/partners')` returns `/partnerships/partners` (no-op).

### Formatting (UNT-006–008)

UNT-006: `buildEntityUrl('partner', 1)` returns `/partnerships/partners/1`.  
UNT-007: `buildEntityUrl('opportunity', 5)` returns `/partnerships/opportunities/5`.  
UNT-008: `buildListUrl('contacts')` returns `/partnerships/contacts`.

### Calculations (UNT-009–013)

UNT-009: `parseEntityId('/partnerships/partners/123')` returns `123`.  
UNT-010: `parseEntityId('/partnerships/partners/abc')` returns `null`.  
UNT-011: `getEntityType('/partnerships/partners/1')` returns `'partner'`.  
UNT-012: `getEntityType('/partnerships/opportunities/1')` returns `'opportunity'`.  
UNT-013: `getEntityType('/admin')` returns `null` (not an entity route).

### Status Logic (UNT-014–018)

UNT-014: `isAuthenticatedRoute('/partnerships/partners')` returns true.  
UNT-015: `isAuthenticatedRoute('/login')` returns false.  
UNT-016: `isProtectedRoute('/admin')` returns true (requires admin).  
UNT-017: `isProtectedRoute('/partnerships/partners')` returns true (requires auth).  
UNT-018: `getReturnUrl('/login?returnUrl=/partnerships/partners/1')` returns `/partnerships/partners/1`.

### Collections (UNT-019–021)

UNT-019: `getAllRoutes()` returns all registered route paths.  
UNT-020: `getProtectedRoutes()` returns only routes requiring auth.  
UNT-021: `getPublicRoutes()` returns only public routes (login, 404).

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

### Single Operations (PRF-001–002)

PRF-001: Route change (sidebar click) completes in < 300ms.  
PRF-002: Deep link initial load completes in < 2s.

### Bulk Operations (PRF-003–005)

PRF-003: Navigate through 10 routes sequentially → Total time < 10s.  
PRF-004: Lazy module load (first access) < 1s.  
PRF-005: Lazy module load (cached) < 100ms.

### Search (PRF-006–010)

PRF-006: Route guard evaluation < 50ms.  
PRF-007: Hash stripping (legacy URL conversion) < 10ms.  
PRF-008: URL parsing and route matching < 20ms.  
PRF-009: Query parameter parsing < 10ms.  
PRF-010: History pushState < 5ms.

### Concurrent Access (PRF-011–013)

PRF-011: 10 tabs loading same route < 3s per tab.  
PRF-012: 50 concurrent deep link requests < 5s average.  
PRF-013: Route change under CPU load < 500ms.

### Memory (PRF-014–016)

PRF-014: Memory stable after 100 route changes (no leak).  
PRF-015: History stack memory < 10MB for 1000 entries.  
PRF-016: Lazy module memory released on route away.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

### Sustained Load (LDT-001–003)

LDT-001: 100 concurrent users navigating for 10 minutes → Server stable.  
LDT-002: 50 users performing deep link access per second → All succeed.  
LDT-003: 200 page refreshes per minute for 5 minutes → Server handles.

### Spike Load (LDT-004–005)

LDT-004: 500 simultaneous deep link accesses (spike) → 95th percentile < 5s.  
LDT-005: 100 concurrent route guard evaluations → All resolve < 1s.

### Stress Limits (LDT-006–008)

LDT-006: Maximum concurrent route changes before degradation.  
LDT-007: Maximum history entries before browser slowdown.  
LDT-008: Maximum concurrent lazy module downloads before timeout.

### Recovery (LDT-009–010)

LDT-009: Server recovery after routing load spike → Normal within 30s.  
LDT-010: Client recovery after 100 rapid route changes → Stable state within 5s.

---

## Status: Ready for Implementation

**Next Steps:**
1. Fix known issues in Playwright test helpers (`navigation.helper.ts`, `form-validation.spec.ts`, `go-decision.spec.ts`)
2. Create Playwright spec files for URL routing tests
3. Execute smoke tests on deployed application
4. Cross-browser testing
