# Path-Based URL Routing Migration — Comprehensive Test Plan

**Feature:** Migration from Hash-Based (`/#/`) to Path-Based (`/`) URL Routing  
**Commit:** "Refactor routing to use path-based URLs instead of hash-based URLs across the application"  
**Created:** 2026-02-17  
**Author:** QA Team  
**Status:** Ready for Execution  
**Priority:** CRITICAL — Breaking change affecting entire application navigation

---

## 1. Scope

This test plan covers the application-wide migration from Angular `HashLocationStrategy` (`/#/path`) to `PathLocationStrategy` (`/path`) routing. This change affects every page, navigation link, bookmark, deep link, and external reference in the application.

### In Scope

| Area | Description |
|------|-------------|
| **Direct URL Navigation** | Typing path-based URLs directly in browser address bar |
| **Deep Linking** | Direct access to entity detail pages (e.g., `/partnerships/partners/123`) |
| **Browser History** | Back/forward navigation with path-based URLs |
| **Page Refresh** | F5/Ctrl+R on any page maintains correct route |
| **External Links** | Links from emails, notifications, oUP integration use correct format |
| **Legacy Hash URLs** | Behavior when users visit `/#/partnerships/partners` (old bookmarks) |
| **Navigation Components** | Sidebar, topbar, tabs, breadcrumbs all generate correct URLs |
| **Programmatic Navigation** | `router.navigate()`, `routerLink` all use path-based routes |
| **Route Guards** | Authentication and permission guards work with path-based routing |
| **404 Handling** | Invalid paths return proper not-found page |
| **oUP Deep Links** | External system links to opportunities use correct URL format |
| **Login Redirect** | Post-login redirect preserves intended path-based URL |

### Out of Scope

| Area | Reason |
|------|--------|
| Server-side rendering | Not implemented |
| SEO/meta tags | Not applicable for internal application |

---

## 2. Impact Assessment

### Routes Affected

| Route Pattern | Example Path | Feature |
|---------------|-------------|---------|
| `/` | Homepage/Dashboard | Dashboard |
| `/login` | Login page | Authentication |
| `/partnerships/partners` | Partner list | Partnerships |
| `/partnerships/partners/:id` | Partner detail | Partnerships |
| `/partnerships/contacts` | Contact list | Partnerships |
| `/partnerships/contacts/:id` | Contact detail | Partnerships |
| `/partnerships/interactions` | Interaction list | Partnerships |
| `/partnerships/interactions/:id` | Interaction detail | Partnerships |
| `/partnerships/opportunities` | Opportunity list | Opportunities |
| `/partnerships/opportunities/:id` | Opportunity detail | Opportunities |
| `/admin/*` | Admin pages | Administration |
| `/ai/*` | AI assistant pages | AI Features |
| `/search/*` | Search results | Search |

### Known Issues from Migration

| Issue | File | Status |
|-------|------|--------|
| `navigation.helper.ts` uses wrong paths (`/partners` instead of `/partnerships/partners`) | `QA Tests/Playwright Tests/helpers/navigation.helper.ts` | Needs fix |
| `form-validation.spec.ts` uses `/opportunities` instead of `/partnerships/opportunities` | `QA Tests/Playwright Tests/form-validation.spec.ts` | Needs fix |
| `go-decision.spec.ts` uses `/opportunities/${id}` instead of `/partnerships/opportunities/${id}` | `QA Tests/Playwright Tests/go-decision.spec.ts` | Needs fix |
| Outdated "hash-based routing" comments in multiple spec files | Various | Needs cleanup |
| `oup-integration.spec.ts` documents URL as `/#/partnerships/opportunities/<id>` | `QA Tests/Playwright Tests/oup-integration.spec.ts` | Needs fix |
| `README_PLAYWRIGHT.md` examples use `/#/` format | `QA Tests/Playwright Tests/README_PLAYWRIGHT.md` | Needs update |

---

## 3. Test Environment

### Prerequisites

| Requirement | Details |
|-------------|---------|
| **Application** | Running with path-based routing (no `useHash` in router config) |
| **Web Server** | Configured for HTML5 History API fallback (all routes serve `index.html`) |
| **Browsers** | Chrome (latest), Firefox (latest), Edge (latest), Safari (latest) |

### Server Configuration Requirements

For path-based routing to work, the web server MUST be configured to return `index.html` for all routes (HTML5 History API fallback). Without this:
- Direct URL navigation will return 404
- Page refresh will return 404
- Deep links will fail

---

## 4. Related Test Documentation

| Document | Coverage |
|----------|----------|
| `QA Tests/Playwright Tests/navigation-tabs.spec.ts` | Tab navigation (existing, needs URL assertions added) |
| `QA Tests/E2E Test Scenarios/E2E-TEST-SCENARIOS.md` | E2E scenario mapping |
| `QA Tests/Test Plans/url-routing-migration-plan.md` | This plan |
| `QA Tests/Opportunity Tests/BusinessLogic/URLRoutingMigration_TestCases.md` | **NEW** — Detailed test cases |

---

## 5. Test Categories and Counts

### Pre-Implementation Ratio Calculation

```
Planned Positive Tests: P = 35

Core category minimums:
- Negative: Max(50, 2 × 35) = Max(50, 70) = 70
- Edge/Boundary: Max(50, 2 × 35) = Max(50, 70) = 70
- Functional: 50 (FIXED)
- Integration: 50 (FIXED)

Additional category minimums:
- Unit: 21 (FIXED)
- Concurrency: 25 (FIXED)
- Performance: 16 (FIXED)
- Load: 10 (FIXED)
- (Security: OUT OF SCOPE for QA)

Individual ratio checks (each must pass):
- N≥3P: Negative ≥ 3 × P (70 ≥ 105 for P=35)
- E≥3P: Edge ≥ 3 × P (70 ≥ 105 for P=35)
- F≥3P: Functional ≥ 3 × P (50 ≥ 105 for P=35)
- I≥3P: Integration ≥ 3 × P (50 ≥ 105 for P=35)
Result: ✅ PASS when N,E,F,I each ≥ 3×P
```

### Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 35 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 70 | Max(50, 2×35=70) | ✅ |
| 3 | Boundary Tests | §3 | 70 | Max(50, 2×35=70) | ✅ |
| 4 | Functional Tests | §4 | 50 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 50 | ≥50 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **347** | **≥462** | ⬜ |

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## 6. Test Suites

### Suite 1: URL Routing Migration

**Test Cases Document:** `QA Tests/Opportunity Tests/BusinessLogic/URLRoutingMigration_TestCases.md`

**Playwright Test Files:**

| File | Category | Count |
|------|----------|-------|
| `url-routing-navigation.spec.ts` | §1 Positive + §4 Functional | Navigation tests |
| `url-routing-deep-links.spec.ts` | §1 Positive + §3 Boundary | Deep linking tests |
| `url-routing-legacy.spec.ts` | §2 Negative + §3 Boundary | Legacy hash URL tests |
| `url-routing-guards.spec.ts` | §2 Negative + §5 Integration | Auth guard tests |

---

## 7. Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Server not configured for History API fallback | High | Critical | Verify nginx/IIS config before testing |
| Legacy bookmarks broken for existing users | High | High | Test hash URL redirection/stripping |
| External systems sending hash URLs (oUP) | Medium | High | Test and document required updates |
| Email notification links using old format | Medium | High | Verify email templates updated |
| Deep links to entity details fail | Medium | Critical | Test all entity type deep links |
| Browser caching serves old hash-based app | Low | Medium | Test with cache clear |

---

## 8. Execution Strategy

### Phase 1: Smoke Tests (Immediate)
1. Direct navigation to each top-level route
2. Page refresh on each route
3. Browser back/forward

### Phase 2: Deep Link Tests
1. All entity detail pages via direct URL
2. Invalid entity IDs
3. Parameterized routes

### Phase 3: Legacy Compatibility
1. Hash-based URL handling
2. Bookmark migration
3. External link formats

### Phase 4: Cross-Browser
1. Chrome, Firefox, Edge, Safari
2. Mobile responsive routes

---

## 9. Entry / Exit Criteria

### Entry Criteria
- [ ] Application deployed with path-based routing
- [ ] Web server configured for HTML5 History API fallback
- [ ] All existing Playwright tests updated for path-based URLs

### Exit Criteria
- [ ] All 347 test cases executed
- [ ] Pass rate ≥ 95%
- [ ] Zero critical/high defects open
- [ ] Ratio compliance verified (N≥3P, E≥3P, F≥3P, I≥3P)
- [ ] Navigation helper paths corrected
- [ ] All outdated hash-based routing comments removed
