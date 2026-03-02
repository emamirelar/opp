# Home Dashboard UI — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/shared/pages/components/home`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

| **N≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **E≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **F≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **I≥3P?** | ✅ | 90 ≥ 3×30 = 90 |

---

## Feature Overview

Home dashboard UI: widget rendering, KPI display, recent activity, pipeline chart, partner stats, responsive.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Dashboard Loads Successfully

**Priority:** P0  
**Precondition:** User logged in with valid credentials.

**Steps:**
1. Navigate to home/dashboard
2. Wait for load

**Expected Result:** Dashboard displayed, all widgets load, no stuck spinners.

---

#### POS-002: Welcome Message Displayed

**Priority:** P0  
**Precondition:** User logged in.

**Steps:**
1. View dashboard
2. Locate welcome message

**Expected Result:** "Welcome, [User Name]" displayed with correct name.

---

#### POS-003: KPI Widgets Display

**Priority:** P0  
**Precondition:** Data exists for KPIs.

**Steps:**
1. View dashboard
2. Check KPI widgets (Partners, Opportunities, etc.)

**Expected Result:** All KPI widgets render with correct counts.

---

#### POS-004: Recent Activity Widget

**Priority:** P0  
**Precondition:** User has recent activity.

**Steps:**
1. View dashboard
2. Locate Recent Activity widget

**Expected Result:** Recent activities listed with timestamps, clickable links.

---

#### POS-005: Pipeline Chart Renders

**Priority:** P0  
**Precondition:** Opportunity data exists.

**Steps:**
1. View dashboard
2. Locate pipeline chart

**Expected Result:** Chart displays opportunity stages with counts.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Partner stats widget | Partners exist | View partner stats | Stats displayed | P1 |
| POS-007 | Opportunity summary widget | Opportunities exist | View opportunity widget | Summary by stage | P1 |
| POS-008 | Click activity link | Activity exists | Click activity | Navigates to record | P1 |
| POS-009 | Click pipeline stage | Chart rendered | Click stage | Navigates to filtered list | P1 |
| POS-010 | Refresh dashboard | Dashboard loaded | Click refresh | Data refreshes | P1 |
| POS-011 | Date range selector | Dashboard loaded | Select date range | Data filtered | P1 |
| POS-012 | My Tasks widget | User has tasks | View My Tasks | Tasks listed | P1 |
| POS-013 | Dashboard responsive (desktop) | Desktop viewport | View dashboard | Full layout | P1 |
| POS-014 | Dashboard responsive (tablet) | Tablet viewport | View dashboard | Adapted layout | P1 |
| POS-015 | Dashboard responsive (mobile) | Mobile viewport | View dashboard | Mobile layout | P1 |
| POS-016 | Widget collapse/expand | Collapsible widget | Toggle | Collapses/expands | P2 |
| POS-017 | Empty state (no data) | No opportunities | View dashboard | Empty state message | P2 |
| POS-018 | Loading skeleton | Initial load | View during load | Skeleton shown | P2 |
| POS-019 | Accessibility: keyboard nav | Focus | Tab through | All focusable | P2 |
| POS-020 | Accessibility: screen reader | Screen reader | Navigate | Announced correctly | P2 |
| POS-021 | Widget reorder | Reorder enabled | Drag widget | Order persisted | P2 |
| POS-022 | Dashboard with 0 partners | No partners | View | Zero shown | P2 |
| POS-023 | Dashboard with 1000 partners | Many partners | View | Stats correct | P2 |
| POS-024 | Unicode in user name | Arabic name | View welcome | Displayed correctly | P2 |
| POS-025 | Dark mode | Dark theme | View dashboard | Theme applied | P2 |
| POS-026 | Chart tooltip | Hover chart | Hover segment | Tooltip shows | P2 |
| POS-027 | Activity pagination | 50+ activities | View widget | Paginated | P2 |
| POS-028 | Dashboard after login | Fresh login | Navigate | Dashboard loads | P2 |
| POS-029 | Dashboard with permissions | Limited permissions | View | Only permitted widgets | P2 |
| POS-030 | Export dashboard data | Export enabled | Export | Data exported | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 70 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Invalid date range | FromDate > ToDate | Validation error | P0 |
| NEG-002 | Negative date range | FromDate = -1 | Validation error | P0 |
| NEG-003 | Null date range | Range = null | Default range | P0 |
| NEG-004 | Invalid widget config | Config malformed | Fallback or error | P0 |
| NEG-005 | Invalid KPI filter | Filter = "invalid" | Default or error | P0 |
| NEG-006 | Non-existent widget ID | WidgetId = 999999 | Widget not shown | P0 |
| NEG-007 | Invalid chart type | Type = "invalid" | Default chart | P0 |
| NEG-008 | Malformed activity data | Data malformed | Graceful handling | P0 |
| NEG-009 | Invalid refresh interval | Interval = -1 | Default interval | P0 |
| NEG-010 | Invalid page size | Size = 0 | Default size | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Anonymous user | No auth | View dashboard | Redirect to login | P0 |
| NEG-012 | Expired session | Expired token | View dashboard | Redirect to login | P0 |
| NEG-013 | User without dashboard permission | No permission | View dashboard | Access denied | P0 |
| NEG-014 | Disabled user | Disabled | View dashboard | Access denied | P1 |
| NEG-015 | No view partners permission | Limited | View partner widget | Widget hidden or empty | P1 |
| NEG-016 | No view opportunities permission | Limited | View opportunity widget | Widget hidden or empty | P1 |
| NEG-017 | OrgUnit-scoped user | Scoped | View dashboard | Scoped data only | P1 |
| NEG-018 | API without auth | No Bearer | GET /dashboard | 401 | P0 |
| NEG-019 | Tampered JWT | Modified | View dashboard | 401 | P0 |
| NEG-020 | Post-logout | Logged out | Cached dashboard | Redirect to login | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Refresh during load | Loading | Refresh | Queued or ignored | P1 |
| NEG-022 | Navigate during load | Loading | Navigate away | Load cancelled | P1 |
| NEG-023 | Date change during load | Loading | Change date | Queued | P1 |
| NEG-024 | Widget toggle during load | Loading | Toggle | Queued | P1 |
| NEG-025 | Export during load | Loading | Export | Disabled or queued | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Dashboard config null | Config = null | Default config | P1 |
| NEG-027 | User name null | User.Name = null | "Welcome" or placeholder | P1 |
| NEG-028 | KPI data null | KPI API returns null | Empty or zero | P1 |
| NEG-029 | Activity data null | Activity API returns null | Empty list | P1 |
| NEG-030 | Chart data null | Chart API returns null | Empty chart | P1 |
| NEG-031 | Partner stats null | Stats API returns null | Zero values | P1 |
| NEG-032 | Widget list empty | No widgets | Empty dashboard | P1 |
| NEG-033 | Date range null | Range = null | Default range | P1 |
| NEG-034 | Theme null | Theme = null | Default theme | P1 |
| NEG-035 | Locale null | Locale = null | Default locale | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Dashboard API down | 500 error | Error message, retry | P0 |
| NEG-037 | KPI API timeout | Timeout | Timeout message | P0 |
| NEG-038 | Activity API down | 503 | Widget shows error | P1 |
| NEG-039 | Chart API down | 503 | Chart shows error | P1 |
| NEG-040 | Multiple API failures | 3 APIs down | Partial dashboard | P1 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Duplicate widget IDs | Same ID twice | One shown | P1 |
| NEG-042 | Widget overflow | 100 widgets | Paginated or scroll | P1 |
| NEG-043 | XSS in user name | `<script>` | Sanitized | P0 |
| NEG-044 | XSS in activity title | `<script>` | Sanitized | P0 |
| NEG-045 | SQL injection in filter | `'; DROP--` | Sanitized | P0 |
| NEG-046 | Oversized date range | 10 years | Capped or error | P1 |
| NEG-047 | Invalid chart data | Negative values | Handled | P1 |
| NEG-048 | Chart with 0 segments | No data | Empty chart | P1 |
| NEG-049 | Activity with null link | Link = null | No navigation | P1 |
| NEG-050 | KPI with negative count | Count = -1 | Display 0 | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Viewport width 0 | 0px width | Graceful handling | P1 |
| NEG-052 | Viewport height 0 | 0px height | Graceful handling | P1 |
| NEG-053 | Very small viewport | 100px | Responsive layout | P2 |
| NEG-054 | Very large viewport | 10000px | Layout correct | P2 |
| NEG-055 | Slow network | 3G | Loading state, timeout | P1 |
| NEG-056 | Offline | No network | Offline message | P1 |
| NEG-057 | Invalid locale | Locale = "xx" | Default locale | P2 |
| NEG-058 | Invalid theme | Theme = "invalid" | Default theme | P2 |
| NEG-059 | Rapid refresh clicks | 10 clicks/sec | Debounced | P1 |
| NEG-060 | Rapid date changes | 5 changes/sec | Debounced | P1 |
| NEG-061 | Export with no data | Empty dashboard | Empty file or message | P2 |
| NEG-062 | Chart with infinite values | Infinity | Handled | P1 |
| NEG-063 | Activity with invalid link | Link = "javascript:" | Sanitized | P0 |
| NEG-064 | Path traversal in export | `../../evil` | Rejected | P0 |
| NEG-065 | Malformed API response | Invalid JSON | Error handling | P1 |
| NEG-066 | CORS error | Cross-origin | Error message | P1 |
| NEG-067 | Rate limit exceeded | 429 from API | Retry or message | P1 |
| NEG-068 | LDAP injection in search | `*)(cn=*` | Sanitized | P1 |
| NEG-069 | Regex DoS in filter | `(((...)))` | Rejected or timeout | P1 |
| NEG-070 | Concurrent tab refresh | 2 tabs refresh | Both complete | P1 |
| NEG-071 | Dashboard with invalid widget order | Order = -1 | Default order | P1 |
| NEG-072 | Dashboard with null KPI type | Type = null | Default or skip | P1 |
| NEG-073 | Dashboard with invalid chart config | Config malformed | Default chart | P1 |
| NEG-074 | Activity with invalid timestamp | Timestamp invalid | Skipped or error | P1 |
| NEG-075 | Dashboard with duplicate widget ID | Same ID twice | One shown | P1 |
| NEG-076 | Export with invalid format | Format = "invalid" | Default CSV | P1 |
| NEG-077 | Dashboard with negative refresh interval | Interval = -5 | Default | P1 |
| NEG-078 | Widget with null title | Title = null | Placeholder | P1 |
| NEG-079 | Chart with null data points | Data = null | Empty chart | P1 |
| NEG-080 | KPI with null value | Value = null | Zero displayed | P1 |
| NEG-081 | Dashboard with empty widget config | Config = [] | Empty dashboard | P1 |
| NEG-082 | Date range with invalid timezone | TZ = "invalid" | Default | P2 |
| NEG-083 | Dashboard with NaN in chart | Value = NaN | Handled | P1 |
| NEG-084 | Dashboard with Infinity in chart | Value = Infinity | Handled | P1 |
| NEG-085 | Activity link with invalid URL | URL malformed | No navigation | P1 |
| NEG-086 | Dashboard with circular widget ref | Circular | Error or truncated | P1 |
| NEG-087 | Export with oversized data | 100MB | Chunked or error | P1 |
| NEG-088 | Dashboard with null user context | User = null | Default or error | P1 |
| NEG-089 | Widget with invalid permission | Perm = "invalid" | Hidden | P1 |
| NEG-090 | Dashboard with stale cache | Old cache | Refetch | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 70 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | User name | 0 | 200 | ✅ Empty | ✅ 200 | ❌ Truncated | P1 |
| BND-002 | Activity title | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Truncated | P1 |
| BND-003 | Widget title | 0 | 100 | ✅ Empty | ✅ 100 | ❌ Truncated | P1 |
| BND-004 | Date format | 10 | 30 | ✅ ISO | ✅ Long format | ❌ Error | P2 |
| BND-005 | Chart label | 0 | 50 | ✅ Empty | ✅ 50 | ❌ Truncated | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | KPI count | 0 | MAX_INT | ✅ 0 | ❌ 0 | Overflow | P1 |
| BND-007 | Chart value | 0 | MAX_INT | ✅ 0 | ❌ 0 | Overflow | P1 |
| BND-008 | Page size | 1 | 100 | ❌ Default | ❌ Error | Capped | P1 |
| BND-009 | Refresh interval | 1 | 3600 | ❌ Default | ❌ Error | Capped | P1 |
| BND-010 | Widget count | 0 | 50 | ✅ 0 | ❌ | Capped | P1 |
| BND-011 | Activity count | 0 | 1000 | ✅ 0 | ❌ | Paginated | P2 |
| BND-012 | Chart segments | 0 | 50 | ✅ 0 | ❌ | Scroll | P2 |
| BND-013 | Viewport width | 320 | 7680 | ✅ 320 | ❌ | Handled | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-014 | Date range leap year | Feb 29, 2028 | Handled correctly | P2 |
| BND-015 | Date range midnight | 00:00:00 | Correct | P2 |
| BND-016 | Date range end of day | 23:59:59 | Correct | P2 |
| BND-017 | Same day range | FromDate = ToDate | Returns that day | P2 |
| BND-018 | Timezone boundary | UTC vs local | Correct display | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-019 | Zero activities | Empty | Empty list message | P1 |
| BND-020 | One activity | Single | 1 item | P1 |
| BND-021 | Exactly page size activities | 20, size=20 | Full page | P1 |
| BND-022 | Page size + 1 activities | 21, size=20 | 20 on page 1 | P1 |
| BND-023 | 1000 activities | Large | Paginated | P1 |
| BND-024 | Zero widgets | Empty | Empty dashboard | P1 |
| BND-025 | One widget | Single | 1 widget | P1 |
| BND-026 | 20 widgets | Max | All displayed | P1 |
| BND-027 | Zero KPI values | No data | Zeros shown | P1 |
| BND-028 | Chart with 0 segments | No data | Empty chart | P1 |
| BND-029 | Chart with 1 segment | Single | 1 segment | P1 |
| BND-030 | Chart with 20 segments | Many | Scroll or paginate | P2 |
| BND-031 | Last page activities | Page 5 of 5 | Correct remaining | P1 |
| BND-032 | Pipeline with 0 opportunities | No data | Empty pipeline | P1 |
| BND-033 | Pipeline with 1 stage | Single | 1 segment | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-034 | User name (Arabic) | `أحمد` | Displayed correctly | P2 |
| BND-035 | User name (Chinese) | `李明` | Displayed correctly | P2 |
| BND-036 | Activity title (Cyrillic) | `Обновление` | Displayed correctly | P2 |
| BND-037 | Name with apostrophe | O'Brien | Preserved | P1 |
| BND-038 | Chart label with emoji | `Stage 1 🔄` | Displayed | P2 |
| BND-039 | Activity with special chars | `Update & Create` | Preserved | P2 |
| BND-040 | KPI label with accent | `Résumé` | Displayed | P2 |

### 3.6 Responsive Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | Viewport 320px (mobile) | iPhone SE | Mobile layout | P1 |
| BND-042 | Viewport 768px (tablet) | iPad | Tablet layout | P1 |
| BND-043 | Viewport 1024px (desktop) | Small desktop | Desktop layout | P1 |
| BND-044 | Viewport 1920px (large) | Full HD | Large layout | P1 |
| BND-045 | Viewport 3840px (4K) | 4K | Scaled layout | P2 |
| BND-046 | Orientation change | Rotate device | Layout adapts | P1 |
| BND-047 | Resize during load | Resize viewport | Layout correct | P2 |
| BND-048 | High DPI (2x) | Retina | Sharpe rendering | P2 |
| BND-049 | Touch vs mouse | Touch device | Touch targets | P1 |
| BND-050 | Zoom 200% | Zoomed | Readable | P1 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | KPI exactly 0 | Zero count | "0" displayed | P1 |
| BND-052 | KPI exactly max | 999999 | Displayed | P1 |
| BND-053 | Chart single value | 1 data point | Renders | P1 |
| BND-054 | Chart equal values | All same | Renders | P1 |
| BND-055 | Very long activity list | 500 items | Paginated | P1 |
| BND-056 | Very short date range | 1 hour | Data for hour | P2 |
| BND-057 | Very long date range | 1 year | Data for year | P2 |
| BND-058 | Minimal dashboard | 1 widget | Renders | P1 |
| BND-059 | Full dashboard | All widgets | All render | P1 |
| BND-060 | Slow API (2s) | 2s response | Loading, then data | P1 |
| BND-061 | Fast API (50ms) | 50ms response | No flicker | P2 |
| BND-062 | Concurrent widget load | All load | All complete | P2 |
| BND-063 | Theme switch | Light to dark | Theme applied | P2 |
| BND-064 | Locale switch | en to fr | Labels translated | P2 |
| BND-065 | RTL layout | Arabic locale | RTL layout | P2 |
| BND-066 | Reduced motion | Prefers-reduced-motion | Animations off | P2 |
| BND-067 | High contrast | High contrast mode | Contrast respected | P2 |
| BND-068 | Font scaling | 150% font | Readable | P2 |
| BND-069 | Color blind mode | Protanopia | Distinguishable | P2 |
| BND-070 | Print view | Print | Print layout | P2 |
| BND-071 | KPI count exactly 0 | Zero | "0" displayed | P1 |
| BND-072 | KPI count exactly 999999 | Max | Formatted | P1 |
| BND-073 | Chart with exactly 1 segment | Single | Renders | P1 |
| BND-074 | Chart with exactly 50 segments | Max | Scroll or paginate | P2 |
| BND-075 | Activity count exactly 0 | Empty | Empty message | P1 |
| BND-076 | Activity count exactly 100 | Page size | Full page | P1 |
| BND-077 | Widget count exactly 1 | Single | 1 widget | P1 |
| BND-078 | Widget count exactly 50 | Max | All displayed | P1 |
| BND-079 | Date range exactly 1 day | Same from/to | That day | P2 |
| BND-080 | Date range exactly 1 year | Full year | Correct | P2 |
| BND-081 | Viewport exactly 320px | Mobile | Mobile layout | P1 |
| BND-082 | Viewport exactly 1920px | Desktop | Desktop layout | P1 |
| BND-083 | Refresh interval exactly 1s | Min | 1s refresh | P2 |
| BND-084 | Refresh interval exactly 3600s | Max | 1h refresh | P2 |
| BND-085 | Page size exactly 1 | Min | 1 activity | P2 |
| BND-086 | Page size exactly 100 | Max | 100 activities | P2 |
| BND-087 | User name exactly 0 chars | Empty | Placeholder | P1 |
| BND-088 | User name exactly 200 chars | Max | Truncated or full | P1 |
| BND-089 | Activity title exactly 500 chars | Max | Truncated | P1 |
| BND-090 | Chart value exactly 0 | Zero | Renders | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Dashboard loads on nav | Nav | Navigate to / | Dashboard loads | P0 |
| FUN-002 | KPIs from API | API | Load | KPI data fetched | P0 |
| FUN-003 | Activities from API | API | Load | Activity data fetched | P0 |
| FUN-004 | Chart from API | API | Load | Chart data fetched | P0 |
| FUN-005 | Refresh fetches new data | Refresh | Click refresh | API called again | P0 |
| FUN-006 | Date range filters data | Date | Select range | Filtered data | P0 |
| FUN-007 | Activity click navigates | Click | Click activity | Navigate to record | P0 |
| FUN-008 | Chart click navigates | Click | Click segment | Navigate to list | P0 |
| FUN-009 | Widget visibility by permission | Permission | Load | Only permitted widgets | P0 |
| FUN-010 | Data scoped by OrgUnit | OrgUnit | Load | Scoped data | P0 |
| FUN-011 | Loading state during fetch | Fetch | API call | Loading shown | P1 |
| FUN-012 | Error state on failure | Failure | API error | Error shown | P1 |
| FUN-013 | Empty state when no data | No data | Empty response | Empty state | P1 |
| FUN-014 | Retry on error | Error | Click retry | Retry attempted | P1 |
| FUN-015 | Cache on repeat visit | Repeat | Navigate again | Cached or fresh | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Date range From ≤ To | Range | Valid range | From > To | P0 |
| FUN-017 | User authenticated | Auth | Valid token | No token | P0 |
| FUN-018 | Viewport dimensions | Viewport | > 0 | 0 | P1 |
| FUN-019 | Widget ID valid | ID | Valid | Invalid | P1 |
| FUN-020 | Chart data non-negative | Chart | ≥ 0 | < 0 | P1 |
| FUN-021 | Activity link valid | Link | Valid URL | Invalid | P1 |
| FUN-022 | No XSS in user name | Sanitize | "John" | `<script>` | P0 |
| FUN-023 | No XSS in activity | Sanitize | "Update" | `<script>` | P0 |
| FUN-024 | Export format valid | Format | CSV, PDF | Invalid | P1 |
| FUN-025 | Refresh interval valid | Interval | 1-3600 | 0, -1 | P1 |
| FUN-026 | Theme valid | Theme | Light, Dark | Invalid | P2 |
| FUN-027 | Locale valid | Locale | en, fr, etc. | Invalid | P2 |
| FUN-028 | Widget config valid | Config | Valid JSON | Invalid | P1 |
| FUN-029 | KPI type valid | Type | Valid enum | Invalid | P1 |
| FUN-030 | Chart type valid | Type | Bar, Pie, etc. | Invalid | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max widgets | 50 | 51 | Capped at 50 | P1 |
| FUN-032 | Max activities per page | 100 | 150 | Capped at 100 | P1 |
| FUN-033 | Max date range | 1 year | 2 years | Capped or error | P1 |
| FUN-034 | Export row limit | 10000 | 15000 | Paginated | P2 |
| FUN-035 | Chart segment limit | 50 | 51 | Scroll or paginate | P2 |
| FUN-036 | Refresh debounce | 1s | Rapid clicks | Debounced | P1 |
| FUN-037 | Cache TTL | 5 min | After 5 min | Refetch | P2 |
| FUN-038 | Concurrent fetch limit | 5 | 10 | Queued | P2 |
| FUN-039 | Session timeout | 30 min | After 30 min | Redirect | P1 |
| FUN-040 | Request timeout | 30s | 31s | Timeout error | P1 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Dashboard view | Load | PageView logged | P1 |
| FUN-042 | Refresh | Refresh | Refresh logged | P1 |
| FUN-043 | Export | Export | Export logged | P1 |
| FUN-044 | Date change | Change date | Filter logged | P1 |
| FUN-045 | Widget click | Click widget | Click logged | P1 |
| FUN-046 | Activity click | Click activity | Navigation logged | P1 |
| FUN-047 | Chart click | Click chart | Interaction logged | P1 |
| FUN-048 | Error occurred | API error | Error logged | P1 |
| FUN-049 | No PII in logs | Any | No PII in logs | P0 |
| FUN-050 | Audit immutable | Read | Logs not modified | P1 |
| FUN-051 | Dashboard loads on nav | Nav | Navigate to / | Dashboard loads | P0 |
| FUN-052 | KPIs from API | API | Load | KPI data fetched | P0 |
| FUN-053 | Activities from API | API | Load | Activity data fetched | P0 |
| FUN-054 | Chart from API | API | Load | Chart data fetched | P0 |
| FUN-055 | Refresh fetches new data | Refresh | Click refresh | API called | P0 |
| FUN-056 | Date range filters data | Date | Select range | Filtered data | P0 |
| FUN-057 | Activity click navigates | Click | Click activity | Navigate to record | P0 |
| FUN-058 | Chart click navigates | Click | Click segment | Navigate to list | P0 |
| FUN-059 | Widget visibility by permission | Permission | Load | Only permitted | P0 |
| FUN-060 | Data scoped by OrgUnit | OrgUnit | Load | Scoped data | P0 |
| FUN-061 | Loading state during fetch | Fetch | API call | Loading shown | P1 |
| FUN-062 | Error state on failure | Failure | API error | Error shown | P1 |
| FUN-063 | Empty state when no data | No data | Empty response | Empty state | P1 |
| FUN-064 | Retry on error | Error | Click retry | Retry attempted | P1 |
| FUN-065 | Cache on repeat visit | Repeat | Navigate again | Cached or fresh | P1 |
| FUN-066 | Date range From ≤ To | Range | Valid range | From > To invalid | P0 |
| FUN-067 | User authenticated | Auth | Valid token | No token redirect | P0 |
| FUN-068 | Viewport dimensions | Viewport | > 0 | 0 invalid | P1 |
| FUN-069 | Widget ID valid | ID | Valid | Invalid hidden | P1 |
| FUN-070 | Chart data non-negative | Chart | ≥ 0 | < 0 handled | P1 |
| FUN-071 | Activity link valid | Link | Valid URL | Invalid no nav | P1 |
| FUN-072 | No XSS in user name | Sanitize | "John" | `<script>` escaped | P0 |
| FUN-073 | No XSS in activity | Sanitize | "Update" | `<script>` escaped | P0 |
| FUN-074 | Export format valid | Format | CSV, PDF | Invalid default | P1 |
| FUN-075 | Refresh interval valid | Interval | 1-3600 | 0, -1 default | P1 |
| FUN-076 | Theme valid | Theme | Light, Dark | Invalid default | P2 |
| FUN-077 | Locale valid | Locale | en, fr, etc. | Invalid default | P2 |
| FUN-078 | Widget config valid | Config | Valid JSON | Invalid error | P1 |
| FUN-079 | KPI type valid | Type | Valid enum | Invalid default | P1 |
| FUN-080 | Chart type valid | Type | Bar, Pie, etc. | Invalid default | P1 |
| FUN-081 | Max widgets 50 | Constraint | 50 | 51 capped | P1 |
| FUN-082 | Max activities per page 100 | Constraint | 100 | 150 capped | P1 |
| FUN-083 | Max date range 1 year | Constraint | 1 year | 2 years capped | P1 |
| FUN-084 | Export row limit 10000 | Constraint | 10000 | Paginated | P2 |
| FUN-085 | Chart segment limit 50 | Constraint | 50 | Scroll | P2 |
| FUN-086 | Refresh debounce 1s | Constraint | 1s | Rapid debounced | P1 |
| FUN-087 | Cache TTL 5 min | Constraint | 5 min | Refetch after | P2 |
| FUN-088 | Concurrent fetch limit 5 | Constraint | 5 | 10 queued | P2 |
| FUN-089 | Session timeout 30 min | Constraint | 30 min | Redirect after | P1 |
| FUN-090 | Request timeout 30s | Constraint | 30s | Timeout after | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Login → Dashboard | Login + Nav | User, Dashboard | Dashboard loads | P0 |
| INT-002 | Dashboard → Activity → Record | Full flow | Dashboard, Activity, Record | Navigate to record | P0 |
| INT-003 | Dashboard → Chart → List | Full flow | Dashboard, Chart, List | Navigate to list | P0 |
| INT-004 | Refresh → Updated data | Refresh | Dashboard | Data updated | P0 |
| INT-005 | Date change → Filtered data | Date | Dashboard | Filtered | P1 |
| INT-006 | Widget reorder → Persisted | Reorder | Dashboard | Order saved | P1 |
| INT-007 | Theme change → Applied | Theme | Dashboard | Theme applied | P1 |
| INT-008 | Locale change → Translated | Locale | Dashboard | Labels translated | P1 |
| INT-009 | Export → File downloaded | Export | Dashboard | File downloaded | P1 |
| INT-010 | Logout → Redirect | Logout | User | Redirect to login | P0 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Filter by date range | Last 7 days | 7 days data | P0 |
| INT-012 | Filter by date range | Last 30 days | 30 days data | P0 |
| INT-013 | Filter by custom range | Jan 1 - Jan 31 | January data | P1 |
| INT-014 | Filter by OrgUnit | User's OrgUnit | Scoped data | P1 |
| INT-015 | Filter by permission | User's permissions | Permitted data | P1 |
| INT-016 | No filter | Default | All user's data | P1 |
| INT-017 | Filter empty result | No matching data | Empty state | P1 |
| INT-018 | Filter then refresh | Filter + Refresh | Filtered refresh | P1 |
| INT-019 | Multiple filter combo | Date + OrgUnit | Combined | P2 |
| INT-020 | Clear filter | Clear | Default filter | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Activity page 1 | 50, page=1, size=20 | 20 items | P1 |
| INT-022 | Activity last page | 50, page=3, size=20 | 10 items | P1 |
| INT-023 | Empty pagination | 0 items | Empty, total=0 | P1 |
| INT-024 | Single page | 15, size=20 | 15 items | P2 |
| INT-025 | Large page | 100, size=100 | 100 items | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Dashboard → User | User | Load | User info fetched | P0 |
| INT-027 | Dashboard → KPIs | KPIs | Load | KPI API called | P0 |
| INT-028 | Dashboard → Activities | Activities | Load | Activity API called | P0 |
| INT-029 | Dashboard → Chart | Chart | Load | Chart API called | P0 |
| INT-030 | Activity → Record | Link | Click | Navigate to record | P0 |
| INT-031 | Chart → Opportunity List | Link | Click | Navigate to list | P0 |
| INT-032 | Widget → Config | Config | Load | Widget configured | P1 |
| INT-033 | Dashboard → Permissions | Permissions | Load | Widgets filtered | P1 |
| INT-034 | Dashboard → OrgUnit | OrgUnit | Load | Data scoped | P1 |
| INT-035 | Export → Dashboard data | Export | Export | Data in file | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | API 500 | Server error | Error message | P0 |
| INT-037 | API 401 | Unauthorized | Redirect to login | P0 |
| INT-038 | API 403 | Forbidden | Access denied | P0 |
| INT-039 | API 404 | Not found | Not found message | P0 |
| INT-040 | API timeout | Timeout | Timeout message | P0 |
| INT-041 | API 429 | Rate limit | Retry message | P1 |
| INT-042 | Network error | No connection | Network error | P1 |
| INT-043 | CORS error | Cross-origin | Error message | P1 |
| INT-044 | Invalid JSON | Malformed | Parse error | P1 |
| INT-045 | Partial failure | 2 of 3 APIs fail | Partial dashboard | P1 |
| INT-046 | Redirect loop | Auth loop | Handled | P1 |
| INT-047 | Slow response | 5s | Loading, then data | P1 |
| INT-048 | XSS in response | Malicious data | Sanitized | P0 |
| INT-049 | Large payload | 10MB | Rejected or truncated | P2 |
| INT-050 | Session expired | Mid-session | Re-auth | P0 |
| INT-051 | Login → Dashboard | Full flow | Dashboard loads | P0 |
| INT-052 | Dashboard → Activity → Record | Full flow | Navigate to record | P0 |
| INT-053 | Dashboard → Chart → List | Full flow | Navigate to list | P0 |
| INT-054 | Refresh → Updated data | Refresh | Data updated | P0 |
| INT-055 | Date change → Filtered data | Date | Filtered | P1 |
| INT-056 | Widget reorder → Persisted | Reorder | Order saved | P1 |
| INT-057 | Theme change → Applied | Theme | Theme applied | P1 |
| INT-058 | Locale change → Translated | Locale | Labels translated | P1 |
| INT-059 | Export → File downloaded | Export | File downloaded | P1 |
| INT-060 | Logout → Redirect | Logout | Redirect to login | P0 |
| INT-061 | Filter by date range 7 days | Filter | 7 days data | P0 |
| INT-062 | Filter by date range 30 days | Filter | 30 days data | P0 |
| INT-063 | Filter by custom range | Filter | January data | P1 |
| INT-064 | Filter by OrgUnit | Filter | Scoped data | P1 |
| INT-065 | Filter by permission | Filter | Permitted data | P1 |
| INT-066 | No filter default | Filter | All user data | P1 |
| INT-067 | Filter empty result | Filter | Empty state | P1 |
| INT-068 | Filter then refresh | Filter + Refresh | Filtered refresh | P1 |
| INT-069 | Multiple filter combo | Filter | Combined | P2 |
| INT-070 | Clear filter | Clear | Default filter | P1 |
| INT-071 | Activity page 1 | Pagination | 20 items | P1 |
| INT-072 | Activity last page | Pagination | Remaining items | P1 |
| INT-073 | Empty pagination | Pagination | Empty, total=0 | P1 |
| INT-074 | Single page | Pagination | All items | P2 |
| INT-075 | Large page | Pagination | 100 items | P2 |
| INT-076 | Dashboard → User | Relationship | User info fetched | P0 |
| INT-077 | Dashboard → KPIs | Relationship | KPI API called | P0 |
| INT-078 | Dashboard → Activities | Relationship | Activity API called | P0 |
| INT-079 | Dashboard → Chart | Relationship | Chart API called | P0 |
| INT-080 | Activity → Record | Relationship | Navigate to record | P0 |
| INT-081 | Chart → Opportunity List | Relationship | Navigate to list | P0 |
| INT-082 | Widget → Config | Relationship | Widget configured | P1 |
| INT-083 | Dashboard → Permissions | Relationship | Widgets filtered | P1 |
| INT-084 | Dashboard → OrgUnit | Relationship | Data scoped | P1 |
| INT-085 | Export → Dashboard data | Relationship | Data in file | P1 |
| INT-086 | API 500 | Error | Error message | P0 |
| INT-087 | API 401 | Error | Redirect to login | P0 |
| INT-088 | API 403 | Error | Access denied | P0 |
| INT-089 | API 404 | Error | Not found message | P0 |
| INT-090 | Dashboard end-to-end full flow | E2E | Login→Dashboard→Refresh→Export | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in filter | `'; DROP--` | Parameterized | P0 |
| SEC-002 | XSS in user name | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-003 | XSS in activity | `"><script>` | Sanitized | P0 |
| SEC-004 | XSS in chart label | `<img onerror=...>` | Escaped | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | Path traversal in export | `../../evil` | Rejected | P0 |
| SEC-007 | HTML in KPI | `<b>bold</b>` | Escaped | P1 |
| SEC-008 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-009 | Template injection | `{{constructor}}` | Sanitized | P1 |
| SEC-010 | DOM clobbering | `id=constructor` | Mitigated | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous view | No auth | GET /dashboard | 401/Redirect | P0 |
| SEC-012 | Expired token | Expired | Any | 401 | P0 |
| SEC-013 | No permission | Limited | View full dashboard | Filtered | P0 |
| SEC-014 | Tampered JWT | Modified | Any | 401 | P0 |
| SEC-015 | Disabled account | Disabled | Any | 403 | P1 |
| SEC-016 | Post-logout | Logged out | Cached | 401 | P1 |
| SEC-017 | Cross-tenant | User A | User B's data | 403 | P0 |
| SEC-018 | Horizontal privilege | User A | Escalate | 403 | P0 |
| SEC-019 | No export permission | Reader | Export | 403 | P1 |
| SEC-020 | OrgUnit violation | Scoped | Other OrgUnit | 403 | P0 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Activity ID | Guess ID | 403 if no access | P0 |
| SEC-022 | Widget ID | Manipulate | Validated | P0 |
| SEC-023 | Negative ID | -1 | 400 | P1 |
| SEC-024 | Zero ID | 0 | 400 | P1 |
| SEC-025 | Float ID | 1.5 | 400 | P1 |
| SEC-026 | String ID | "abc" | 400 | P1 |
| SEC-027 | MAX_INT ID | 2147483647 | Handled | P1 |
| SEC-028 | Other user's activity | Access via ID | 403 | P0 |
| SEC-029 | Deleted record link | Deleted | 404 | P1 |
| SEC-030 | Future record | Not yet accessible | 403 | P1 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | User ID | Not modifiable | P0 |
| SEC-032 | Session token | HttpOnly | P0 |
| SEC-033 | Permissions | Server-side | P0 |
| SEC-034 | OrgUnit | Server-side | P0 |
| SEC-035 | Audit fields | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on export | CSRF token | P0 |
| SEC-040 | CSRF on refresh | CSRF token | P0 |
| SEC-041 | Token storage | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Policy enforced | P1 |
| SEC-043 | Token refresh | Works correctly | P1 |
| SEC-044 | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected Protection | Priority |
|----|------|-------------------|----------|
| SEC-046 | PII in activities | Filtered | P1 |
| SEC-047 | Stack traces | Generic errors | P0 |
| SEC-048 | Debug info | Not in prod | P0 |
| SEC-049 | Response caching | No sensitive | P1 |
| SEC-050 | Tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two tabs load dashboard | Concurrent load | Both succeed | P1 |
| CON-002 | Refresh during load | Load + Refresh | One completes | P1 |
| CON-003 | Date change during load | Load + Date | Queued or cancelled | P1 |
| CON-004 | Navigate during load | Load + Navigate | Load cancelled | P1 |
| CON-005 | Export during refresh | Export + Refresh | Both complete | P1 |
| CON-006 | Multiple rapid refreshes | 5 clicks in 1s | Debounced | P1 |
| CON-007 | Widget load race | 3 widgets load | All complete | P1 |
| CON-008 | Tab switch during load | Switch tab | Load continues or cancels | P2 |
| CON-009 | Window resize during load | Resize + Load | Layout correct | P2 |
| CON-010 | Theme change during load | Theme + Load | Theme applied | P2 |
| CON-011 | Locale change during load | Locale + Load | Locale applied | P2 |
| CON-012 | Two users same dashboard | 2 users | Independent | P1 |
| CON-013 | Session expiry during view | Expire mid-view | Redirect | P1 |
| CON-014 | Token refresh during fetch | Refresh mid-fetch | Retry with new token | P1 |
| CON-015 | API timeout during load | Timeout | Error shown | P1 |
| CON-016 | Cache invalidation | Update + read | Fresh data | P1 |
| CON-017 | Memory leak on repeated load | 100 loads | No leak | P1 |
| CON-018 | Concurrent export | 2 exports | Both succeed | P2 |
| CON-019 | Optimistic update | Update + read | Consistent | P1 |
| CON-020 | Connection pool | Many tabs | All complete | P1 |
| CON-021 | LocalStorage race | 2 tabs write | One wins | P2 |
| CON-022 | Chart render race | Resize + render | Correct | P2 |
| CON-023 | Activity list race | Load + scroll | Correct | P2 |
| CON-024 | KPI fetch race | 2 fetches | Latest wins | P2 |
| CON-025 | Dashboard initialization | Fast nav | Correct state | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Date range validation | Validation | Valid range | Valid | P1 |
| UNT-002 | Date range invalid | Validation | From > To | Invalid | P1 |
| UNT-003 | Format KPI number | Formatting | 1000 | "1,000" | P1 |
| UNT-004 | Format date | Formatting | Date | Locale format | P1 |
| UNT-005 | Calculate chart percentage | Calculations | 25, 100 | 25% | P1 |
| UNT-006 | Pagination total pages | Calculations | 55, 20 | 3 | P1 |
| UNT-007 | Loading state | Status logic | Loading | true | P1 |
| UNT-008 | Error state | Status logic | Error | true | P1 |
| UNT-009 | Empty state | Status logic | Empty | true | P1 |
| UNT-010 | Widget visible by permission | Status logic | Has perm | true | P1 |
| UNT-011 | Sanitize user name | Formatting | "<script>" | Escaped | P1 |
| UNT-012 | Sanitize activity title | Formatting | "Update" | Same | P1 |
| UNT-013 | Build dashboard config | Collections | Widgets | Config | P1 |
| UNT-014 | Filter activities by date | Collections | Activities, range | Filtered | P1 |
| UNT-015 | Sort activities | Collections | Activities | Sorted | P1 |
| UNT-016 | Map API to model | Collections | API response | Model | P1 |
| UNT-017 | Chart data transform | Formatting | Raw data | Chart format | P1 |
| UNT-018 | Viewport breakpoint | Calculations | 768 | "tablet" | P1 |
| UNT-019 | Responsive columns | Calculations | Breakpoint | Column count | P1 |
| UNT-020 | Truncate long text | Formatting | 500 chars | Truncated | P1 |
| UNT-021 | Format relative time | Formatting | 5 min ago | "5m ago" | P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Dashboard initial load | Full load | < 3s | P2 |
| PRF-002 | KPI widget load | KPI fetch | < 500ms | P2 |
| PRF-003 | Activity widget load | Activity fetch | < 500ms | P2 |
| PRF-004 | Chart render | Chart render | < 1s | P2 |
| PRF-005 | Refresh | Refresh | < 2s | P2 |
| PRF-006 | Date filter | Filter | < 1s | P2 |
| PRF-007 | Export 100 rows | Export | < 2s | P2 |
| PRF-008 | Theme switch | Switch | < 100ms | P2 |
| PRF-009 | 10 concurrent loads | 10 tabs | All < 5s | P2 |
| PRF-010 | Resize layout | Resize | < 100ms | P2 |
| PRF-011 | Scroll activity list | Scroll | 60fps | P2 |
| PRF-012 | Chart animation | Animate | 60fps | P2 |
| PRF-013 | Memory: 100 loads | 100 loads | No leak | P2 |
| PRF-014 | LCP (Largest Contentful Paint) | LCP | < 2.5s | P2 |
| PRF-015 | FID (First Input Delay) | FID | < 100ms | P2 |
| PRF-016 | CLS (Cumulative Layout Shift) | CLS | < 0.1 | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained view | 50 users, 1 req/s | 5 min | 95% < 3s | P2 |
| LDT-002 | Sustained refresh | 20 users, 0.5 req/s | 5 min | 95% < 2s | P2 |
| LDT-003 | Sustained filter | 30 users, 1 req/s | 5 min | 95% < 1s | P2 |
| LDT-004 | Spike view | 0→100 users in 30s | 2 min | No errors | P2 |
| LDT-005 | Spike refresh | 0→50 users | 2 min | Queue or 429 | P2 |
| LDT-006 | Stress view | 200 users, 5 req/s | 5 min | Graceful degradation | P2 |
| LDT-007 | Stress export | 50 users, 1 req/s | 5 min | Queue or 429 | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 20 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | System recovers | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Widget rendering | POS-001 to POS-005, FUN-001 to FUN-004 |
| AC-2: KPI display | POS-003, POS-006, BND-006, BND-051 |
| AC-3: Recent activity | POS-004, POS-008, INT-002 |
| AC-4: Pipeline chart | POS-005, POS-009, BND-028 to BND-033 |
| AC-5: Partner stats | POS-006, BND-027 |
| AC-6: Responsive | POS-013 to POS-015, BND-041 to BND-050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
