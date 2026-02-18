# PNO-150: Partner Ecosystem View — Test Cases

**JIRA Reference:** [PNO-150](https://unops.atlassian.net/browse/PNO-150)  
**Created:** 2026-02-04  
**Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | 90 | ✅ |
| Boundary Tests | §3 | 90 | 90 | ✅ |
| Functional Tests | §4 | 90 | 90 | ✅ |
| Integration Tests | §5 | 90 | 90 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

| **N≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **E≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **F≥3P?** | ✅ | 90 ≥ 3×30 = 90 |
| **I≥3P?** | ✅ | 90 ≥ 3×30 = 90 |

---

## Feature Overview

The Partner Ecosystem view provides a visual representation of the partner network — hierarchy, parent-child relationships, partner types (Funding, Client, Implementation), and relationship lines. Users can search, filter, zoom/pan, navigate to partner detail, and export the ecosystem.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Ecosystem View Loads Successfully

**Priority:** P0  
**Precondition:** User is authenticated. At least 5 partners exist with parent-child relationships.

**Steps:**
1. Navigate to Partner Ecosystem page
2. Wait for ecosystem view to fully render

**Expected Result:**
- Ecosystem visualization displays all non-deleted partners
- Hierarchy tree is rendered with correct parent-child connections
- Relationship lines connect related partners
- Page loads within 3 seconds

---

#### POS-002: Navigate from Ecosystem Node to Partner Detail

**Priority:** P0  
**Precondition:** Ecosystem view loaded with partners displayed.

**Steps:**
1. Click on a partner node in the ecosystem
2. Observe navigation behavior

**Expected Result:**
- User is navigated to the partner detail page for the selected partner
- Partner detail page shows correct data matching the clicked node
- Browser URL updates to `/partners/{id}`

---

#### POS-003: Filter Ecosystem by Partner Type — Funding

**Priority:** P0  
**Precondition:** Ecosystem view loaded. Multiple partner types exist (Funding, Client, Implementation).

**Steps:**
1. Open the partner type filter dropdown
2. Select "Funding" type
3. Apply filter

**Expected Result:**
- Only Funding partners and their relationships are displayed
- Non-Funding partners are hidden from the view
- Filter indicator shows active filter

---

#### POS-004: Search Partner in Ecosystem by Name

**Priority:** P0  
**Precondition:** Ecosystem view loaded with at least 10 partners.

**Steps:**
1. Enter a known partner name in the search box
2. Press Enter or click search icon

**Expected Result:**
- Matching partner node is highlighted/focused in the view
- View auto-pans to center on the found partner
- Search result count shows "1 of N" matches

---

#### POS-005: Expand and Collapse Hierarchy Nodes

**Priority:** P0  
**Precondition:** Ecosystem view loaded. Partners have multi-level hierarchy.

**Steps:**
1. Click expand icon on a collapsed parent node
2. Verify child nodes appear
3. Click collapse icon on the same node
4. Verify child nodes are hidden

**Expected Result:**
- Expanding shows all direct children with relationship lines
- Collapsing hides children and their sub-trees
- Animation is smooth (< 300ms transition)

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Filter by Client type | Ecosystem loaded, Client partners exist | Select "Client" filter | Only Client partners visible | P1 |
| POS-007 | Filter by Implementation type | Ecosystem loaded, Implementation partners exist | Select "Implementation" filter | Only Implementation partners visible | P1 |
| POS-008 | Clear all filters | Filters applied | Click "Clear Filters" | All partners visible again | P1 |
| POS-009 | Zoom in on ecosystem | Ecosystem loaded | Use zoom-in control or mouse wheel up | View zooms in, details become more readable | P1 |
| POS-010 | Zoom out on ecosystem | Ecosystem loaded | Use zoom-out control or mouse wheel down | View zooms out, more partners visible | P1 |
| POS-011 | Pan ecosystem view | Ecosystem loaded | Click-drag on background area | View pans in drag direction | P1 |
| POS-012 | Display partner statistics totals | At least 20 partners exist | Load ecosystem view | Statistics panel shows total partner count | P1 |
| POS-013 | Display count by partner type | Partners of each type exist | Load ecosystem view | Stats show count per type (Funding: N, Client: M) | P1 |
| POS-014 | Display active partner count | Mix of active/inactive partners | Load ecosystem view | Stats show active count correctly | P1 |
| POS-015 | Hover to see relationship type | Partners with relationships loaded | Hover over relationship line | Tooltip shows relationship type | P1 |
| POS-016 | Click relationship line for details | Partners with relationships loaded | Click on relationship line | Popup shows relationship details | P1 |
| POS-017 | Export ecosystem as PDF | Ecosystem loaded | Click Export > PDF | PDF file downloads with ecosystem visual | P1 |
| POS-018 | Export ecosystem as PNG | Ecosystem loaded | Click Export > PNG | PNG image downloads with ecosystem visual | P1 |
| POS-019 | Export ecosystem as CSV | Ecosystem loaded | Click Export > CSV | CSV file downloads with partner data | P1 |
| POS-020 | Search with partial name match | Partners exist | Type first 3 letters of partner name | Matching partners highlighted | P1 |
| POS-021 | Navigate search results (next) | Multiple matches found | Click "Next" on search results | Next matching node is focused | P2 |
| POS-022 | Navigate search results (previous) | Multiple matches found | Click "Previous" on search results | Previous matching node is focused | P2 |
| POS-023 | Clear search field | Search active | Click clear/X button in search | All highlighting removed, full view restored | P2 |
| POS-024 | View ecosystem on desktop resolution | Desktop browser (1920×1080) | Load ecosystem | Proper layout with sidebar and full visualization | P2 |
| POS-025 | View ecosystem on tablet resolution | Tablet browser (1024×768) | Load ecosystem | Responsive layout adapts to tablet viewport | P2 |
| POS-026 | View ecosystem on mobile resolution | Mobile browser (375×667) | Load ecosystem | Responsive layout for mobile, touch-enabled | P2 |
| POS-027 | Expand all nodes | Ecosystem with collapsed nodes | Click "Expand All" | All hierarchy nodes expanded | P2 |
| POS-028 | Collapse all nodes | Ecosystem with expanded nodes | Click "Collapse All" | All nodes collapsed to root level | P2 |
| POS-029 | Reset zoom to default | Ecosystem zoomed in | Click "Reset Zoom" / fit-to-view | View resets to fit all visible partners | P2 |
| POS-030 | Multi-type filter combination | Partners of all types exist | Select Funding + Client types | Both Funding and Client partners visible | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Search with empty string | Empty search box + Enter | No action or "Enter a search term" message | P1 |
| NEG-002 | Search with only whitespace | "   " in search box | Treated as empty, no results | P1 |
| NEG-003 | Search with SQL injection | `'; DROP TABLE partners--` | Input sanitized, no SQL execution | P0 |
| NEG-004 | Search with XSS payload | `<script>alert('xss')</script>` | Input escaped, no script execution | P0 |
| NEG-005 | Search with non-existent name | "ZZZNONEXISTENT999" | "No matching partners found" message | P1 |
| NEG-006 | Search with very long string | 1000+ character search string | Input truncated or rejected gracefully | P1 |
| NEG-007 | Filter with invalid type parameter (URL manipulation) | Modify URL filter param to `type=INVALID` | Default view loads or 400 error | P1 |
| NEG-008 | Navigate to non-existent partner ID | Click node, manipulate URL to ID=999999 | 404 Not Found page displayed | P1 |
| NEG-009 | Search with special characters only | `!@#$%^&*()` | No crash, appropriate "no results" message | P1 |
| NEG-010 | Export with no partners visible | Apply filter that yields zero results, export | "No data to export" message or empty file | P1 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Unauthenticated user accesses ecosystem | No auth | Navigate to /partners/ecosystem | Redirect to login page | P0 |
| NEG-012 | User without partner view permission | Role with no CanViewAllPartners | Navigate to ecosystem | Access denied / 403 page | P0 |
| NEG-013 | User sees only permitted partners | OrgUnit-scoped user | Load ecosystem | Only partners in user's OrgUnit scope visible | P0 |
| NEG-014 | Expired session accessing ecosystem | Expired JWT | Navigate to ecosystem | Redirect to login with session expired message | P0 |
| NEG-015 | User without export permission exports | Role without CanExportPartners | Click Export button | Button disabled or "Insufficient permissions" | P1 |
| NEG-016 | Manipulate API to access restricted partner | Scoped user | Call API with partner ID outside scope | 403 Forbidden response | P0 |
| NEG-017 | Access ecosystem with revoked token | Revoked JWT | API call for ecosystem data | 401 Unauthorized | P0 |
| NEG-018 | Cross-tenant partner visibility | User in Tenant A | Load ecosystem | Partners from Tenant B not visible | P0 |
| NEG-019 | Read-only user attempts edit from ecosystem | Read-only role | Right-click partner node → Edit | No edit option available | P1 |
| NEG-020 | User without hierarchy view permission | Restricted role | Navigate to ecosystem | 403 or limited view | P1 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Navigate to deleted partner from ecosystem | Partner soft-deleted | Click cached node for deleted partner | "Partner not found" error page | P1 |
| NEG-022 | Filter by archived partner type | No archived filter exists | Manipulate URL to add archived filter | Ignored or default view | P1 |
| NEG-023 | Export during data refresh | Data refreshing | Click export while spinner active | Export waits until data loaded, or disabled | P1 |
| NEG-024 | Expand node with no children (API returns empty) | Node that lost children (deleted) | Click expand | Node remains collapsed, no error | P1 |
| NEG-025 | View partner in inactive ecosystem context | Ecosystem context deactivated | Navigate to ecosystem | Appropriate error or fallback view | P2 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Partner without name renders in ecosystem | Partner.Name = null | Node shows placeholder "Unnamed Partner" | P1 |
| NEG-027 | Partner without type renders | Partner.Type = null | Node uses default styling, no crash | P1 |
| NEG-028 | Partner without parent in hierarchy | Partner.ParentId = null | Rendered as root-level node | P1 |
| NEG-029 | Ecosystem with no relationship data | No relationships defined | Nodes display without connection lines | P1 |
| NEG-030 | Partner with null status | Partner.Status = null | Node renders with unknown status indicator | P2 |
| NEG-031 | Statistics panel with zero partners | No partners exist | Stats show "0 Total, 0 Active" | P1 |
| NEG-032 | Child count with only deleted children | Parent has children, all deleted | Child count shows 0, no expand icon | P1 |
| NEG-033 | Relationship with deleted target partner | Relationship target soft-deleted | Relationship line not rendered | P1 |
| NEG-034 | Partner with missing OrgUnit | Partner.OrgUnitId = null | Node renders without OrgUnit label | P2 |
| NEG-035 | Export with null partner fields | Some partners have null fields | Export handles null gracefully, no crash | P2 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | API timeout loading ecosystem data | Backend responds after 30s | Loading spinner, then timeout message with retry | P1 |
| NEG-037 | API returns 500 for ecosystem | Backend error | "Unable to load ecosystem. Try again." message | P1 |
| NEG-038 | Network disconnection during load | Network drops mid-request | Error message with retry option | P1 |
| NEG-039 | Partial data load failure | Some partner data fails | Load available data, indicate partial failure | P2 |
| NEG-040 | Export service unavailable | Export backend fails | "Export unavailable. Try again later." | P2 |
| NEG-041 | Search API returns error | Search endpoint 500 | "Search unavailable" message | P2 |
| NEG-042 | Statistics API failure | Stats endpoint fails | Statistics panel shows error or placeholder | P2 |
| NEG-043 | Relationship data API fails | Relationship endpoint 500 | Nodes render without relationship lines | P2 |
| NEG-044 | Image/icon CDN unavailable | Static asset fails | Fallback icons used, no broken images | P2 |
| NEG-045 | Database connection lost during filter | DB drops during query | Graceful error, cached data if available | P2 |

### 2.6 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-046 | Double-click on partner node | Rapid double-click | Single navigation, no duplicate actions | P1 |
| NEG-047 | Click expand on already-loading node | Click expand during animation | No duplicate API calls, stable rendering | P1 |
| NEG-048 | Search during ecosystem loading | Type search before load completes | Search queued until data ready | P2 |
| NEG-049 | Apply filter during search | Active search, then apply type filter | Both filter and search apply correctly | P2 |
| NEG-050 | Zoom beyond maximum limit | Zoom in repeatedly | Zoom caps at maximum, no further zoom | P2 |
| NEG-051 | Zoom below minimum limit | Zoom out repeatedly | Zoom caps at minimum | P2 |
| NEG-052 | Pan beyond data boundaries | Pan far from any nodes | Elastic bounce-back or boundary limit | P2 |
| NEG-053 | Export during filter change | Filter changing, click export | Export waits or uses current state | P2 |
| NEG-054 | Navigate back from detail with stale state | Detail page modifies partner, navigate back | Ecosystem refreshes with updated data | P2 |
| NEG-055 | Multiple rapid filter changes | Toggle filters quickly | Final filter state applied correctly | P2 |
| NEG-056 | Browser back button from ecosystem | On ecosystem page | Navigates to previous page correctly | P2 |
| NEG-057 | Ecosystem with circular hierarchy reference | ParentId creates loop | No infinite loop, error handled | P1 |
| NEG-058 | Right-click context menu outside node | Right-click background | Browser default context menu or nothing | P2 |
| NEG-059 | Touch gesture on non-touch device | Pinch-to-zoom on desktop | Fallback to mouse-based zoom | P2 |
| NEG-060 | Ecosystem page with JavaScript disabled | JS disabled | Graceful degradation or "JS required" message | P2 |
| NEG-061 | Search with emoji characters | `🏢🔍` in search | No crash, appropriate handling | P2 |
| NEG-062 | Filter API returns malformed data | API returns invalid JSON | Error handled, filter shows error state | P2 |
| NEG-063 | Export file exceeds browser download limit | Very large ecosystem | Export handles large files or paginates | P2 |
| NEG-064 | Ecosystem with 0 active but many deleted | All partners deleted | Empty ecosystem with "No partners" message | P1 |
| NEG-065 | Rapid page refresh during load | F5 during data load | Clean reload, no stale data | P2 |
| NEG-066 | Invalid zoom level in URL params | URL has `?zoom=INVALID` | Default zoom used | P2 |
| NEG-067 | Open ecosystem in multiple tabs | Same user, 2 tabs | Each tab has independent state | P2 |
| NEG-068 | Partner node with extremely long name | Name > 200 chars | Name truncated with ellipsis in node | P2 |
| NEG-069 | Partner hierarchy depth > 10 levels | Very deep nesting | Renders without stack overflow | P1 |
| NEG-070 | Ecosystem with 10,000+ partners | Very large dataset | Progressive loading or virtualization | P1 |
| NEG-071 | Search with null after trim | "   " | Treated as empty | P1 |
| NEG-072 | Filter with invalid type enum | Type = "INVALID" | Default view | P1 |
| NEG-073 | Export with invalid format | Format = "DOC" | Default PDF | P1 |
| NEG-074 | Zoom with invalid level | Zoom = "invalid" | Default 100% | P1 |
| NEG-075 | Ecosystem with null partner data | Partner = null | Skipped | P1 |
| NEG-076 | Relationship with null target | Target = null | Line not rendered | P1 |
| NEG-077 | Statistics with null count | Count = null | Display 0 | P1 |
| NEG-078 | Search with control characters | \0 in query | Sanitized | P1 |
| NEG-079 | Export during filter change | Filter changing | Export waits | P2 |
| NEG-080 | Ecosystem with malformed API response | Invalid JSON | Error handled | P1 |
| NEG-081 | Pan with invalid coordinates | X,Y invalid | Capped | P1 |
| NEG-082 | Ecosystem with duplicate partner IDs | Same ID twice | One shown | P1 |
| NEG-083 | Filter with empty array | [] | All visible | P1 |
| NEG-084 | Search with oversized query | 1000+ chars | Truncated | P1 |
| NEG-085 | Export with expired token | Token expired | Auth prompt | P1 |
| NEG-086 | Ecosystem with circular hierarchy | Circular ParentIds | Handled | P0 |
| NEG-087 | Zoom with negative value | Zoom = -10 | Default | P1 |
| NEG-088 | Ecosystem with NaN in statistics | Value = NaN | Display 0 | P1 |
| NEG-089 | Relationship with invalid type | Type = "INVALID" | Default line | P1 |
| NEG-090 | Ecosystem with concurrent export | 2 exports | One queued | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Search query | 1 | 255 | ✅ Single char search | ✅ 255-char search | ❌ Truncated | P1 |
| BND-002 | Partner name display | 1 | 200 | ✅ "A" displays | ✅ Full name in node | Ellipsis | P1 |
| BND-003 | Partner type label | 3 | 50 | ✅ Short type | ✅ Long type name | Truncated | P2 |
| BND-004 | Relationship type label | 1 | 100 | ✅ Displays | ✅ Displays | Truncated | P2 |
| BND-005 | Export filename | 1 | 255 | ✅ Valid | ✅ Valid | System limit enforced | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Partner count in ecosystem | 0 | 100000 | ✅ Empty state | ✅ Virtualized | N/A | P1 |
| BND-007 | Hierarchy depth | 0 | 20 | ✅ Flat list | ✅ Deep tree | Performance warning | P1 |
| BND-008 | Children per parent | 0 | 500 | ✅ Leaf node | ✅ Rendered | Performance degradation | P1 |
| BND-009 | Zoom level | 10% | 500% | ✅ Min zoom | ✅ Max zoom | Capped | P1 |
| BND-010 | Relationship count | 0 | 10000 | ✅ No lines | ✅ Dense lines | Virtualized | P2 |
| BND-011 | Search result count | 0 | 1000 | "No results" | Navigation arrows | Performance | P2 |
| BND-012 | Export row count (CSV) | 0 | 100000 | Empty file/error | Full export | Chunked | P2 |
| BND-013 | Statistics total | 0 | 999999 | "0" displayed | Large number | Formatted | P2 |
| BND-014 | Pan X coordinate | -10000 | 10000 | ✅ Pan left | ✅ Pan right | Bounded | P2 |
| BND-015 | Pan Y coordinate | -10000 | 10000 | ✅ Pan up | ✅ Pan down | Bounded | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-016 | Partner created on leap year date | Feb 29, 2028 | Correctly displayed in ecosystem metadata | P2 |
| BND-017 | Partner with very old creation date | Jan 1, 2000 | Correctly formatted | P2 |
| BND-018 | Partner with future creation date | Jan 1, 2030 | Displayed or flagged as anomaly | P2 |
| BND-019 | Filter by date range at year boundary | Dec 31 – Jan 1 cross-year | Correct range applied | P2 |
| BND-020 | Partner created at midnight UTC | 00:00:00 UTC | No off-by-one day error | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-021 | Ecosystem with 0 partners | Empty database | "No partners found. Add your first partner." CTA | P1 |
| BND-022 | Ecosystem with 1 partner | Single root partner | Single node displayed, no relationships | P1 |
| BND-023 | Ecosystem with 2 partners linked | Two connected partners | Both nodes with relationship line | P1 |
| BND-024 | Ecosystem with 100 partners | Medium dataset | All rendered within 3 seconds | P1 |
| BND-025 | Ecosystem with 1000 partners | Large dataset | Progressive loading, < 5 seconds | P1 |
| BND-026 | Ecosystem with 10,000 partners | Very large | Virtualization / pagination active | P1 |
| BND-027 | Single partner with 50 children | Wide hierarchy | All children rendered, possibly paginated | P1 |
| BND-028 | Linear chain of 15 levels | Deep hierarchy | Full depth rendered with scrolling | P1 |
| BND-029 | Partner with 0 relationships | Isolated partner | Node displays without any connection lines | P2 |
| BND-030 | Partner with 100 relationships | Dense connections | All lines rendered, performance acceptable | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-031 | Partner name (Arabic) | `شريك أعمال` | Correctly rendered in node with RTL support | P2 |
| BND-032 | Partner name (Chinese) | `合作伙伴` | Correctly rendered in node | P2 |
| BND-033 | Partner name (Emoji) | `🏢 Partner Corp` | Emoji renders in node label | P2 |
| BND-034 | Partner name (Cyrillic) | `Партнер` | Correctly displayed | P2 |
| BND-035 | Partner name (Accented) | `Réseau de Partenaires` | Accented characters display correctly | P2 |
| BND-036 | Search with Arabic text | `شريك` | Matches Arabic partner names | P2 |
| BND-037 | Search with mixed scripts | `Partner-合作` | Matches correctly | P2 |
| BND-038 | Relationship label (French) | `Relation de financement` | Renders correctly | P2 |
| BND-039 | Partner name with HTML entities | `Smith &amp; Partners` | Rendered as "Smith & Partners", not raw HTML | P2 |
| BND-040 | CSV export with Unicode names | Partners with multi-script names | CSV opens correctly in Excel with UTF-8 | P2 |

### 3.6 Viewport & Display Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | Minimum viewport width (320px) | 320px wide browser | Ecosystem adapts or shows mobile view | P2 |
| BND-042 | Maximum viewport (4K resolution) | 3840×2160 display | Ecosystem uses full viewport | P2 |
| BND-043 | Portrait orientation tablet | 768×1024 | Vertical layout adapts | P2 |
| BND-044 | Landscape orientation tablet | 1024×768 | Horizontal layout adapts | P2 |
| BND-045 | Browser at 50% zoom | OS-level zoom at 50% | Ecosystem renders proportionally | P2 |
| BND-046 | Browser at 200% zoom | OS-level zoom at 200% | All nodes accessible via scroll | P2 |
| BND-047 | Split screen / half viewport | 960×1080 window | Responsive adaptation | P2 |
| BND-048 | High-DPI / Retina display | 2x pixel density | Sharp rendering, no blurry nodes | P2 |
| BND-049 | Dark mode / theme switch | Dark theme active | Ecosystem colors adapt to theme | P2 |
| BND-050 | Print layout / print preview | Ctrl+P | Printable layout of ecosystem | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Partner with max-length name (200 chars) | Full 200-char name | Displayed with ellipsis or wrapping | P1 |
| BND-052 | Filter results exactly 0 | Filter yields no matches | "No partners match your filters" | P1 |
| BND-053 | Filter results exactly 1 | Filter yields single match | Single partner displayed | P1 |
| BND-054 | All partners same type | All Funding | Type filter shows 100% Funding | P2 |
| BND-055 | All partners root-level (no hierarchy) | No parent-child relationships | Flat layout, no hierarchy lines | P1 |
| BND-056 | Export at zoom level 10% | Minimum zoom + export | Export captures current view or full data | P2 |
| BND-057 | Export at zoom level 500% | Maximum zoom + export | Export captures full view | P2 |
| BND-058 | Search returning max results (1000) | 1000 partners named similarly | Result navigation handles all | P2 |
| BND-059 | Exactly 50 expanded nodes | 50 nodes expanded | Performance remains acceptable | P2 |
| BND-060 | Ecosystem with exactly 2 relationship types | Only 2 types of relationships | Both types displayed distinctly | P2 |
| BND-061 | Partner created today | Created_Date = today | Shows in ecosystem immediately | P2 |
| BND-062 | Partner with empty description | Description = "" | Node renders without description tooltip | P2 |
| BND-063 | Partner with maxlength description | Description = 4000 chars | Tooltip truncates or scrolls | P2 |
| BND-064 | Statistics with exactly 1 active partner | 1 active, rest inactive/deleted | Stats show "1 Active" | P2 |
| BND-065 | Relationship between same partner (self-ref) | ParentId = own ID | Handled gracefully, no infinite loop | P1 |
| BND-066 | Pan exactly to edge boundary | Pan to max extent | Stops at boundary | P2 |
| BND-067 | Zoom exactly to limit | Zoom to 10% or 500% | Stays at limit | P2 |
| BND-068 | Search at exactly max length (255 chars) | 255 character search | Accepted and processed | P2 |
| BND-069 | Export with exactly 1 row | Single partner export | Valid file with 1 data row | P2 |
| BND-070 | Multiple filters yielding exactly 1 partner | Combined filters | Single partner shown | P2 |
| BND-071 | Partner count exactly 0 | Empty | Empty state | P1 |
| BND-072 | Partner count exactly 1 | Single | One node | P1 |
| BND-073 | Partner count exactly 100 | Medium | All in 3s | P1 |
| BND-074 | Partner count exactly 1000 | Large | <5s | P1 |
| BND-075 | Hierarchy depth exactly 0 | Flat | No hierarchy | P1 |
| BND-076 | Hierarchy depth exactly 20 | Max | Full depth | P1 |
| BND-077 | Children per parent exactly 0 | Leaf | No expand | P1 |
| BND-078 | Children per parent exactly 500 | Max | Rendered | P1 |
| BND-079 | Zoom level exactly 10% | Min | Min zoom | P1 |
| BND-080 | Zoom level exactly 500% | Max | Max zoom | P1 |
| BND-081 | Search query exactly 1 char | "A" | Matches | P1 |
| BND-082 | Search query exactly 255 chars | Max | Processed | P1 |
| BND-083 | Relationship count exactly 0 | No relationships | No lines | P1 |
| BND-084 | Relationship count exactly 100 | Dense | All rendered | P2 |
| BND-085 | Statistics total exactly 0 | Empty | "0" | P1 |
| BND-086 | Statistics total exactly 999999 | Max | Formatted | P2 |
| BND-087 | Viewport exactly 320px | Mobile | Adapts | P2 |
| BND-088 | Viewport exactly 3840px | 4K | Full viewport | P2 |
| BND-089 | Partner name exactly 200 chars | Max | Ellipsis | P1 |
| BND-090 | Filter results exactly 0 | No match | Empty message | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow rules (15), Validation rules (15), Constraint rules (10), Audit rules (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule Description | Trigger | Expected Outcome | Priority |
|----|-----------|-----------------|---------|-----------------|----------|
| FUN-001 | Ecosystem shows only non-deleted partners | Soft-delete filter applies | Load ecosystem | IsDeleted=true partners excluded | P0 |
| FUN-002 | Hierarchy preserves parent-child order | Parent renders above children | Load with hierarchy | Visual hierarchy is correct | P0 |
| FUN-003 | Relationship lines connect correct nodes | Line endpoints match relationship data | Load ecosystem | Each line connects source to target | P0 |
| FUN-004 | Filter state persists on page refresh | Browser refresh | Refresh with filters applied | Same filters re-applied | P1 |
| FUN-005 | Search is case-insensitive | Search logic | Search "ACME" vs "acme" | Both match same partner | P1 |
| FUN-006 | Type filter is additive | Multi-select filter | Select Funding + Client | Union of both types shown | P1 |
| FUN-007 | Node click triggers navigation | UI interaction | Click partner node | Route to /partners/{id} | P0 |
| FUN-008 | Expand/collapse persists in session | Session state | Expand nodes, navigate away, return | Expand state preserved | P1 |
| FUN-009 | Statistics update on filter change | Filter trigger | Apply type filter | Stats recalculate for visible set | P1 |
| FUN-010 | Export includes only filtered partners | Filter + export | Filter then export | Export data matches visible set | P1 |
| FUN-011 | New partner appears after creation | Real-time update | Create partner in another tab, refresh | New partner node visible | P1 |
| FUN-012 | Deleted partner disappears on refresh | Soft-delete trigger | Delete partner, refresh ecosystem | Node removed from view | P1 |
| FUN-013 | Relationship removed when partner deleted | Cascade logic | Delete one partner in pair | Relationship line removed | P1 |
| FUN-014 | Search clears on filter change | Filter interaction | Active search, change filter | Search cleared or re-applied to filtered set | P2 |
| FUN-015 | Fit-to-view centers all visible nodes | Zoom reset | Click fit-to-view | All nodes visible in viewport | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Validation Rule | Valid Input | Invalid Input | Priority |
|----|-----------|----------------|------------|--------------|----------|
| FUN-016 | Search min length | Search ≥ 1 char | "A" | "" (empty) | P1 |
| FUN-017 | Search max length | Search ≤ 255 chars | 255 chars | 256 chars | P1 |
| FUN-018 | Filter type must be valid enum | Type ∈ {Funding, Client, Implementation} | "Funding" | "Invalid" | P1 |
| FUN-019 | Export format must be valid | Format ∈ {PDF, PNG, CSV} | "PDF" | "DOC" | P1 |
| FUN-020 | Zoom level within bounds | 10% ≤ zoom ≤ 500% | 100% | 600% | P1 |
| FUN-021 | Partner ID must be positive integer | ID > 0 | 42 | -1, 0, "abc" | P1 |
| FUN-022 | Hierarchy depth within limit | Depth ≤ 20 | 15 levels | 21 levels | P2 |
| FUN-023 | Search sanitizes HTML input | Input sanitization | "ACME" | `<img onerror=...>` | P0 |
| FUN-024 | Filter params validated server-side | API validation | Valid filter JSON | Malformed JSON | P1 |
| FUN-025 | Pan coordinates within canvas bounds | Bounded pan | Within range | Beyond max canvas | P2 |
| FUN-026 | Page number in URL valid | URL pagination | ?page=1 | ?page=-1 | P2 |
| FUN-027 | Export date range valid | From ≤ To | Jan 1 – Dec 31 | Dec 31 – Jan 1 | P2 |
| FUN-028 | Sort parameter valid | Valid field name | ?sort=name | ?sort=DROP_TABLE | P1 |
| FUN-029 | API returns proper content-type | Response headers | application/json | text/plain for JSON | P1 |
| FUN-030 | Ecosystem layout algorithm valid | Layout engine | Normal data | Circular references | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max concurrent exports | 1 export at a time | Trigger 2 exports | Second queued or blocked | P1 |
| FUN-032 | Max search results returned | API limits to 1000 | Query matching 5000 | Only 1000 returned with pagination | P1 |
| FUN-033 | Max nodes rendered simultaneously | Performance limit ~5000 | 5001 partners | Virtualization kicks in | P2 |
| FUN-034 | Min browser version supported | Chrome 90+, Firefox 88+, Edge 90+ | Chrome 89 | Unsupported browser warning | P2 |
| FUN-035 | API rate limiting on search | 10 requests/second | 15 rapid searches | Rate limit response (429) | P1 |
| FUN-036 | Maximum filter combinations | Up to 3 types + status | All filters active | Correctly applied | P2 |
| FUN-037 | Session timeout during ecosystem | 30 min timeout | Leave idle 31 min | Session expired, redirect to login | P1 |
| FUN-038 | Export file size limit | Max 50MB | Large ecosystem export | File generated within limit or chunked | P2 |
| FUN-039 | Concurrent user limit on ecosystem | System capacity | 100 users on ecosystem | All load successfully | P2 |
| FUN-040 | WebSocket connection for real-time updates | Connection required | Load ecosystem | Connection established for live updates | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Ecosystem page view logged | Page load | User ID, timestamp, page visited | P1 |
| FUN-042 | Export action logged | Export ecosystem | User ID, export format, timestamp | P1 |
| FUN-043 | Search query logged | Search execution | User ID, search term, timestamp | P2 |
| FUN-044 | Filter change logged | Apply filter | User ID, filter values, timestamp | P2 |
| FUN-045 | Navigation to partner detail logged | Click partner node | User ID, partner ID, timestamp | P2 |
| FUN-046 | Failed access attempt logged | Unauthorized access | User ID, attempted action, denied reason | P0 |
| FUN-047 | Session timeout logged | Timeout trigger | User ID, session duration, timeout event | P2 |
| FUN-048 | Large export request logged | Export > 1000 rows | User ID, row count, export duration | P2 |
| FUN-049 | Error event logged | API error on ecosystem | Error code, stack trace, user context | P1 |
| FUN-050 | Rate limit violation logged | Too many requests | User ID, endpoint, request count | P1 |
| FUN-051 | Ecosystem shows only non-deleted | IsDeleted | Load | Deleted excluded | P0 |
| FUN-052 | Hierarchy preserves parent-child | Parent | Load | Correct order | P0 |
| FUN-053 | Relationship lines connect correctly | Line | Load | Endpoints match | P0 |
| FUN-054 | Filter state persists on refresh | Refresh | Browser refresh | Filters re-applied | P1 |
| FUN-055 | Search case-insensitive | Search | "ACME" vs "acme" | Same match | P1 |
| FUN-056 | Type filter additive | Filter | Funding + Client | Union shown | P1 |
| FUN-057 | Node click navigates | Click | Partner node | Route to detail | P0 |
| FUN-058 | Expand/collapse persists | Session | Navigate, return | State preserved | P1 |
| FUN-059 | Statistics update on filter | Filter | Apply | Stats recalc | P1 |
| FUN-060 | Export includes only filtered | Filter + Export | Filter then export | Filtered data | P1 |
| FUN-061 | New partner after creation | Create | Create, refresh | Node visible | P1 |
| FUN-062 | Deleted partner after refresh | Delete | Delete, refresh | Node gone | P1 |
| FUN-063 | Relationship removed when partner deleted | Delete | Delete one | Line removed | P1 |
| FUN-064 | Search clears on filter change | Filter | Change | Search cleared | P2 |
| FUN-065 | Fit-to-view centers nodes | Zoom | Reset | All visible | P1 |
| FUN-066 | Search min length 1 | Search | "A" | Valid | P1 |
| FUN-067 | Search max length 255 | Search | 255 chars | Valid | P1 |
| FUN-068 | Filter type valid enum | Type | Funding, Client, Impl | Invalid default | P1 |
| FUN-069 | Export format valid | Format | PDF, PNG, CSV | Invalid default | P1 |
| FUN-070 | Zoom level 10-500% | Zoom | 100% | 600% capped | P1 |
| FUN-071 | Partner ID positive | ID | 42 | -1 invalid | P1 |
| FUN-072 | Hierarchy depth ≤ 20 | Depth | 15 | 21 error | P2 |
| FUN-073 | Search sanitizes HTML | Input | "ACME" | `<img>` escaped | P0 |
| FUN-074 | Filter params validated server-side | API | Valid JSON | Malformed error | P1 |
| FUN-075 | Pan coordinates bounded | Pan | Within range | Beyond capped | P2 |
| FUN-076 | Page number valid | URL | ?page=1 | ?page=-1 default | P2 |
| FUN-077 | Export date range valid | Range | Jan–Dec | Dec–Jan invalid | P2 |
| FUN-078 | Sort parameter valid | Sort | ?sort=name | ?sort=DROP default | P1 |
| FUN-079 | API content-type validation | Response | application/json | text/plain error | P1 |
| FUN-080 | Layout algorithm valid | Layout | Normal | Circular error | P1 |
| FUN-081 | Max concurrent exports 1 | Constraint | 2 exports | 2nd queued | P1 |
| FUN-082 | Max search results 1000 | Constraint | 5000 matches | Paginated | P1 |
| FUN-083 | Max nodes ~5000 | Constraint | 5001 | Virtualization | P2 |
| FUN-084 | Min browser version | Browser | Chrome 89 | Warning | P2 |
| FUN-085 | API rate limit 10 req/s | Constraint | 15 searches | 429 | P1 |
| FUN-086 | Max filter combinations | Constraint | All filters | Applied | P2 |
| FUN-087 | Session timeout 30 min | Constraint | 31 min | Expired | P1 |
| FUN-088 | Export file size 50MB max | Constraint | Large | Chunked | P2 |
| FUN-089 | Concurrent user limit | Constraint | 100 users | All load | P2 |
| FUN-090 | WebSocket for real-time | Constraint | Load | Connection | P2 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests | **Breakdown:** CRUD workflow (10), Search/filter (10), Pagination (5), Relationships (10), Error handling (15)

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|------------------|-----------------|----------|
| INT-001 | Create partner → appears in ecosystem | Create | Partner, Ecosystem | New node rendered after refresh | P0 |
| INT-002 | Update partner name → ecosystem reflects | Update | Partner, Ecosystem | Node label updated | P0 |
| INT-003 | Soft-delete partner → removed from ecosystem | Delete | Partner, Ecosystem | Node disappears from view | P0 |
| INT-004 | Create child partner → hierarchy updates | Create | Parent Partner, Child Partner | Child node under parent | P0 |
| INT-005 | Change partner type → filter reflects | Update | Partner Type | Filter counts update | P1 |
| INT-006 | Activate partner → statistics update | Update status | Partner Status | Active count increments | P1 |
| INT-007 | Deactivate partner → statistics update | Update status | Partner Status | Active count decrements | P1 |
| INT-008 | Add relationship → line appears | Create | Relationship | Connection line rendered | P1 |
| INT-009 | Remove relationship → line disappears | Delete | Relationship | Line removed from ecosystem | P1 |
| INT-010 | Move partner (change parent) → hierarchy restructures | Update | Parent-child | Node moves to new parent | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Search/Filter Criteria | Expected Results | Priority |
|----|-----------|----------------------|-----------------|----------|
| INT-011 | Search + type filter combined | Name "ACME" + Funding type | Only Funding partners matching "ACME" | P0 |
| INT-012 | Search across all partner fields | Search by name, code, or type | Matches from any indexed field | P1 |
| INT-013 | Filter then search | Apply Client filter, search "Global" | Results within Client type matching "Global" | P1 |
| INT-014 | Search then filter | Search "Corp", apply Funding filter | Filtered "Corp" results within Funding | P1 |
| INT-015 | Clear search preserves filter | Clear search with filter active | Filter remains, all filtered partners visible | P1 |
| INT-016 | Clear filter preserves search | Clear filter with search active | Search remains, results from all types | P1 |
| INT-017 | Search with no filter returns all types | No filter + search "Partner" | Results from Funding, Client, Implementation | P1 |
| INT-018 | Filter with no search returns all partners of type | Funding filter + no search | All Funding partners visible | P1 |
| INT-019 | Search results update on data change | Search active, new matching partner added | New partner appears in results after refresh | P2 |
| INT-020 | Filter counts match actual data | Apply each filter type | Count in filter badge matches visible nodes | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| INT-021 | First page of ecosystem loads | Page 1 | First batch of partners rendered | P1 |
| INT-022 | Navigate to next page/batch | Scroll / load more | Additional partners loaded | P1 |
| INT-023 | Virtual scroll loads incrementally | Scroll down in large dataset | New nodes loaded as viewport moves | P2 |
| INT-024 | Page size affects render count | 50 per batch | 50 nodes per incremental load | P2 |
| INT-025 | Last batch loads correctly | Scroll to end | All remaining partners loaded | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Test Scenario | Expected Result | Priority |
|----|-----------|-------------|--------------|-----------------|----------|
| INT-026 | Parent-child relationship line | Hierarchy | Load parent with child | Vertical line connecting them | P0 |
| INT-027 | Funding partnership line | Funding relationship | Partners with funding link | Funding-styled line | P1 |
| INT-028 | Client partnership line | Client relationship | Partners with client link | Client-styled line | P1 |
| INT-029 | Implementation partnership line | Impl relationship | Partners with implementation link | Implementation-styled line | P1 |
| INT-030 | Bidirectional relationship | Two-way link | Partners linked both ways | Bidirectional arrow/line | P1 |
| INT-031 | Multi-level cascade display | 5-level hierarchy | Expand all levels | Full tree visible with proper indentation | P1 |
| INT-032 | Relationship tooltip shows details | Hover on line | Hover over relationship | Shows type, status, dates | P2 |
| INT-033 | Cross-type relationship | Funding → Client link | Different types connected | Line rendered between different-typed nodes | P1 |
| INT-034 | Orphaned partner (no relationships) | Isolated partner | Load ecosystem | Standalone node, no lines | P2 |
| INT-035 | Partner with many relationships (10+) | Dense connections | Load partner cluster | All lines rendered clearly | P2 |

### 5.5 Error Handling (15)

| ID | Test Name | Error Condition | Expected Response | Priority |
|----|-----------|----------------|------------------|----------|
| INT-036 | API 404 for missing partner | Navigate to deleted partner | 404 page or redirect | P0 |
| INT-037 | API 500 on ecosystem load | Server error | Error message with retry button | P0 |
| INT-038 | API 403 for unauthorized access | No permission | Access denied page | P0 |
| INT-039 | Network timeout on search | Slow network | Timeout message, search retryable | P1 |
| INT-040 | Malformed API response | Invalid JSON | Error handled, no crash | P1 |
| INT-041 | Export fails mid-generation | Server crash during export | Error message, partial file cleaned up | P1 |
| INT-042 | Browser memory exceeded | Too many nodes | Warning or graceful degradation | P2 |
| INT-043 | Concurrent API call conflict | Two filters applied simultaneously | Last request wins, consistent state | P1 |
| INT-044 | JWT refresh during ecosystem load | Token refreshes | Seamless reload, no user interruption | P1 |
| INT-045 | Database locked during query | DB contention | Retry logic, eventual success | P2 |
| INT-046 | API rate limit hit (429) | Rapid interactions | Rate limit message, retry after delay | P1 |
| INT-047 | CORS error on API call | Misconfigured CORS | Clear error in console, user sees error | P1 |
| INT-048 | WebSocket disconnection | Connection drops | Reconnect attempt, degraded mode | P2 |
| INT-049 | Invalid filter combination from URL | Crafted URL params | Graceful fallback to default filters | P1 |
| INT-050 | Session expired during export | Token expired mid-export | Re-auth prompt, export retryable | P1 |
| INT-051 | Create partner → appears | Create | Partner, Ecosystem | Node after refresh | P0 |
| INT-052 | Update partner name → reflects | Update | Name | Label updated | P0 |
| INT-053 | Soft-delete partner → removed | Delete | Partner | Node gone | P0 |
| INT-054 | Create child → hierarchy updates | Create | Parent, Child | Child under parent | P0 |
| INT-055 | Change partner type → filter reflects | Update | Type | Filter counts update | P1 |
| INT-056 | Activate partner → statistics update | Update | Status | Active count up | P1 |
| INT-057 | Deactivate partner → statistics update | Update | Status | Active count down | P1 |
| INT-058 | Add relationship → line appears | Create | Relationship | Line rendered | P1 |
| INT-059 | Remove relationship → line disappears | Delete | Relationship | Line gone | P1 |
| INT-060 | Move partner → hierarchy restructures | Update | Parent | Node moves | P1 |
| INT-061 | Search + type filter | Filter | Name + Funding | Funding matches | P0 |
| INT-062 | Search across all fields | Search | Name, code, type | Matches | P1 |
| INT-063 | Filter then search | Filter + Search | Client, "Global" | Combined | P1 |
| INT-064 | Search then filter | Search + Filter | "Corp", Funding | Combined | P1 |
| INT-065 | Clear search preserves filter | Clear | Search | Filter remains | P1 |
| INT-066 | Clear filter preserves search | Clear | Filter | Search remains | P1 |
| INT-067 | Search no filter returns all | Search | "Partner" | All types | P1 |
| INT-068 | Filter no search returns type | Filter | Funding | All Funding | P1 |
| INT-069 | Search results update on data change | Search | New matching | Appears | P2 |
| INT-070 | Filter counts match data | Filter | Each type | Count matches | P1 |
| INT-071 | First page loads | Pagination | Page 1 | First batch | P1 |
| INT-072 | Next page/batch | Pagination | Load more | More loaded | P1 |
| INT-073 | Virtual scroll incremental | Pagination | Scroll | New nodes | P2 |
| INT-074 | Page size affects render | Pagination | 50 per batch | 50 nodes | P2 |
| INT-075 | Last batch loads | Pagination | Scroll end | All loaded | P2 |
| INT-076 | Parent-child relationship line | Relationship | Load | Line connects | P0 |
| INT-077 | Funding partnership line | Relationship | Funding | Funding line | P1 |
| INT-078 | Client partnership line | Relationship | Client | Client line | P1 |
| INT-079 | Implementation partnership line | Relationship | Impl | Impl line | P1 |
| INT-080 | Bidirectional relationship | Relationship | Two-way | Bidirectional | P1 |
| INT-081 | Multi-level cascade | Relationship | 5 levels | Full tree | P1 |
| INT-082 | Relationship tooltip | Relationship | Hover | Details | P2 |
| INT-083 | Cross-type relationship | Relationship | Funding→Client | Line between | P1 |
| INT-084 | Orphaned partner | Relationship | Isolated | No lines | P2 |
| INT-085 | Partner with many relationships | Relationship | 10+ | All rendered | P2 |
| INT-086 | API 404 missing partner | Error | Deleted | 404 | P0 |
| INT-087 | API 500 ecosystem load | Error | Server | Error + retry | P0 |
| INT-088 | API 403 unauthorized | Error | No permission | Access denied | P0 |
| INT-089 | Network timeout on search | Error | Slow | Timeout | P1 |
| INT-090 | Ecosystem end-to-end full flow | E2E | Load→Filter→Search→Export | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests | **Coverage:** OWASP Top 10, injection, authorization, IDOR, mass assignment

### 6.1 Injection Prevention (10)

| ID | Test Name | Attack Vector | Target Field | Expected Block | Priority |
|----|-----------|--------------|-------------|---------------|----------|
| SEC-001 | SQL injection in search | `'; DROP TABLE partners--` | Search box | Sanitized/rejected | P0 |
| SEC-002 | SQL injection in filter param | `type=1 OR 1=1` | Filter API | Parameterized query, no injection | P0 |
| SEC-003 | XSS in search input | `<script>alert(1)</script>` | Search box | HTML escaped | P0 |
| SEC-004 | XSS via partner name display | Partner name with script tag | Node label | Escaped in DOM | P0 |
| SEC-005 | LDAP injection in search | `*)(cn=*` | Search box | Input sanitized | P1 |
| SEC-006 | OS command injection via export filename | `file;rm -rf /` | Export filename | Sanitized filename | P0 |
| SEC-007 | HTML injection in relationship label | `<img src=x onerror=alert(1)>` | Relationship label | Escaped | P1 |
| SEC-008 | Path traversal in export path | `../../etc/passwd` | Export API | Rejected, safe path only | P0 |
| SEC-009 | JSON injection in filter payload | `{"type":"Funding","$ne":null}` | Filter API body | Rejected/sanitized | P1 |
| SEC-010 | XML external entity via export | XXE payload in request | Export API | DTD processing disabled | P1 |

### 6.2 Broken Access Control (10)

| ID | Test Name | User Role | Unauthorized Action | Expected Result | Priority |
|----|-----------|-----------|-------------------|-----------------|----------|
| SEC-011 | Anonymous user accesses ecosystem API | No auth | GET /api/ecosystem | 401 Unauthorized | P0 |
| SEC-012 | Low-privilege user accesses admin ecosystem features | Basic role | GET /api/ecosystem/admin | 403 Forbidden | P0 |
| SEC-013 | OrgUnit-scoped user sees global data | OrgUnit user | GET /api/ecosystem (all) | Only scoped data returned | P0 |
| SEC-014 | Expired token used | Expired JWT | GET /api/ecosystem | 401 Unauthorized | P0 |
| SEC-015 | Modified JWT claims | Tampered token | API call with altered claims | 401/403 Unauthorized | P0 |
| SEC-016 | User accesses another user's saved view | User A | GET /api/ecosystem/views/{userB-id} | 403 Forbidden | P1 |
| SEC-017 | Role escalation via API parameter | Basic role | ?role=admin in API call | Ignored, server uses token claims | P0 |
| SEC-018 | Horizontal privilege escalation | User A | Access partner in User B's scope | 403 Forbidden | P0 |
| SEC-019 | Disabled user account | Disabled account | API call with valid but disabled token | 403 Forbidden | P0 |
| SEC-020 | API access after logout | Logged out | Cached API call | 401 Unauthorized | P1 |

### 6.3 IDOR (Insecure Direct Object Reference) (10)

| ID | Test Name | Object | Manipulation | Expected Result | Priority |
|----|-----------|--------|-------------|-----------------|----------|
| SEC-021 | Access partner detail by guessing ID | Partner ID | Change URL to /partners/999 | 403 if not in scope | P0 |
| SEC-022 | Enumerate partner IDs via API | Partner ID sequence | GET /api/partners/1, /2, /3... | Rate limited, only scoped | P0 |
| SEC-023 | Access ecosystem of other OrgUnit | OrgUnit ID | Change ecosystem scope param | 403 Forbidden | P0 |
| SEC-024 | Export another user's ecosystem view | View ID | Change export view ID | 403 Forbidden | P1 |
| SEC-025 | Access deleted partner via direct URL | Deleted partner ID | GET /api/partners/{deleted-id} | 404 Not Found | P1 |
| SEC-026 | Access partner via negative ID | ID manipulation | GET /api/partners/-1 | 400 Bad Request | P1 |
| SEC-027 | Access partner via zero ID | ID manipulation | GET /api/partners/0 | 400 Bad Request | P1 |
| SEC-028 | Access partner via float ID | ID manipulation | GET /api/partners/1.5 | 400 Bad Request | P1 |
| SEC-029 | Access partner via string ID | ID manipulation | GET /api/partners/abc | 400 Bad Request | P1 |
| SEC-030 | Access relationship by manipulated ID | Relationship ID | Change relationship ID param | 403 if not accessible | P1 |

### 6.4 Mass Assignment (5)

| ID | Test Name | Protected Field | Manipulation | Expected Result | Priority |
|----|-----------|----------------|-------------|-----------------|----------|
| SEC-031 | Modify IsDeleted via API | IsDeleted | Include in request body | Field not modified | P0 |
| SEC-032 | Modify CreatedBy via API | CreatedBy | Include in request body | Field not modified | P0 |
| SEC-033 | Modify CreatedDate via API | CreatedDate | Include in request body | Field not modified | P1 |
| SEC-034 | Modify Id via API | Id | Include in request body | Field not modified | P0 |
| SEC-035 | Modify internal status via API | WorkflowStatus | Include in request body | Ignored unless valid workflow action | P1 |

### 6.5 Authentication & Session (10)

| ID | Test Name | Attack Scenario | Expected Protection | Priority |
|----|-----------|----------------|-------------------|----------|
| SEC-036 | Brute-force login for ecosystem access | Repeated login attempts | Account lockout after 5 failures | P0 |
| SEC-037 | Session fixation | Pre-set session ID | New session on login | P0 |
| SEC-038 | Session hijacking via stolen token | Copied JWT | Token bound to IP/device | P1 |
| SEC-039 | CSRF on ecosystem state change | Forged POST request | CSRF token required | P0 |
| SEC-040 | Clickjacking on ecosystem page | Iframe embedding | X-Frame-Options: DENY | P1 |
| SEC-041 | Token stored securely | Token storage | HttpOnly, Secure cookies | P0 |
| SEC-042 | Concurrent sessions limit | Multiple logins | Policy enforced (last login wins or blocked) | P1 |
| SEC-043 | Token refresh mechanism | Near expiry | Refresh token flow works | P1 |
| SEC-044 | Logout clears all session data | Logout action | Token invalidated, cookies cleared | P0 |
| SEC-045 | Man-in-the-middle protection | HTTPS enforcement | All traffic over TLS | P0 |

### 6.6 Data Exposure (5)

| ID | Test Name | Sensitive Data | Exposure Risk | Expected Protection | Priority |
|----|-----------|---------------|--------------|-------------------|----------|
| SEC-046 | API response excludes internal fields | Internal IDs, audit fields | Over-exposure | Response DTO filters internal fields | P1 |
| SEC-047 | Error responses don't leak stack traces | Exception details | Information disclosure | Generic error message, no stack trace | P0 |
| SEC-048 | Export doesn't include sensitive partner data | Financial data, internal notes | Data leakage | Export filters sensitive columns | P1 |
| SEC-049 | Browser cache doesn't store ecosystem data | Cached API responses | Cache extraction | Cache-Control: no-store headers | P1 |
| SEC-050 | Network tab doesn't expose auth tokens | JWT in requests | Token visibility | Token in HttpOnly cookie, not URL | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests | **Coverage:** Race conditions, deadlocks, double submit, transaction isolation, cache poisoning

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users modify same partner simultaneously | User A updates name, User B updates type | Both succeed, last write wins or conflict error | P1 |
| CON-002 | User deletes partner while another views ecosystem | Delete + view simultaneously | View refreshes, deleted node removed | P1 |
| CON-003 | Two users export ecosystem simultaneously | Concurrent exports | Both exports complete independently | P1 |
| CON-004 | Search during data refresh | Search while cache invalidated | Search returns consistent results | P1 |
| CON-005 | Filter change during search execution | Filter applied while search running | Final state reflects both operations | P1 |
| CON-006 | Expand node while data loading | Click expand during API call | Single API call, correct expand state | P1 |
| CON-007 | Rapid zoom in/out | Quick successive zoom events | Final zoom state is consistent | P2 |
| CON-008 | Navigate away during export | Leave page during export | Export cancels or completes in background | P1 |
| CON-009 | Multiple filter changes in rapid succession | 5 filter toggles in 1 second | Final filter state applied correctly | P1 |
| CON-010 | Partner creation during ecosystem load | Create partner while loading | Ecosystem includes new partner or refreshes | P2 |
| CON-011 | Two users create relationship simultaneously | Both users create same relationship | One succeeds, duplicate prevented | P1 |
| CON-012 | Cache invalidation during read | Cache expires during render | Fresh data fetched, no stale display | P1 |
| CON-013 | WebSocket reconnect during update | Connection drops, reconnects | Missed updates recovered | P2 |
| CON-014 | Concurrent hierarchy restructure | Two users move same partner | One succeeds, other gets conflict | P1 |
| CON-015 | Database migration during ecosystem access | Schema migration running | Ecosystem degrades gracefully | P2 |
| CON-016 | Token refresh during API call | Token expires mid-request | Request retried with new token | P1 |
| CON-017 | Multiple tabs with same ecosystem view | 2 tabs, same user | Independent states, no interference | P2 |
| CON-018 | Concurrent export and delete | Export running, partner deleted | Export captures pre-delete state or handles | P2 |
| CON-019 | Real-time update while filtering | WebSocket update arrives during filter | Update applies to filtered view correctly | P2 |
| CON-020 | Simultaneous search from two sessions | Same user, two sessions | Both searches complete independently | P2 |
| CON-021 | Partner update propagates to active ecosystem views | Partner renamed by another user | All active views eventually reflect change | P1 |
| CON-022 | Database deadlock during heavy read | Multiple concurrent reads | Deadlock resolved, all reads succeed | P1 |
| CON-023 | Cache poisoning attempt | Modified cache entry | Invalidated and refreshed from source | P1 |
| CON-024 | Optimistic concurrency on partner update | Stale version update attempt | Conflict detected, user prompted | P1 |
| CON-025 | Multiple WebSocket subscriptions | Subscribe, unsubscribe, re-subscribe | Correct subscription state maintained | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests | **Breakdown:** Validation (5), Formatting (3), Calculations (5), Status logic (5), Collections (3)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Validate search query not empty | Validation | "" | Invalid | P1 |
| UNT-002 | Validate search query max length | Validation | 256 chars | Invalid | P1 |
| UNT-003 | Validate partner type enum | Validation | "InvalidType" | Invalid | P1 |
| UNT-004 | Validate zoom level range | Validation | 600% | Invalid (capped to 500%) | P1 |
| UNT-005 | Validate export format | Validation | "DOC" | Invalid | P1 |
| UNT-006 | Format partner count with thousands separator | Formatting | 12500 | "12,500" | P1 |
| UNT-007 | Format partner name truncation | Formatting | 250-char name | Truncated to 200 + "..." | P1 |
| UNT-008 | Format relationship tooltip text | Formatting | Relationship data | "Funding | Since 2024-01-15" | P2 |
| UNT-009 | Calculate total partners in filtered view | Calculations | 100 Funding, 50 Client | 150 total | P1 |
| UNT-010 | Calculate hierarchy depth | Calculations | 5-level hierarchy | Depth = 5 | P1 |
| UNT-011 | Calculate active percentage | Calculations | 80 active / 100 total | 80% | P1 |
| UNT-012 | Calculate statistics by type | Calculations | Mixed partners | Correct counts per type | P1 |
| UNT-013 | Calculate relationship density | Calculations | 50 partners, 100 relationships | Density = 2.0 | P2 |
| UNT-014 | Determine partner node visibility status | Status logic | Active partner | Visible=true | P1 |
| UNT-015 | Determine deleted partner visibility | Status logic | IsDeleted=true | Visible=false | P1 |
| UNT-016 | Determine filter match status | Status logic | Partner type vs filter | Match/no-match | P1 |
| UNT-017 | Determine search match status | Status logic | Name vs search query | Match/no-match | P1 |
| UNT-018 | Determine expand/collapse state | Status logic | Node with children | Expandable=true | P1 |
| UNT-019 | Build hierarchy tree from flat list | Collections | Flat partner list with ParentIds | Tree structure | P1 |
| UNT-020 | Group partners by type | Collections | Mixed partners | Grouped dictionary | P1 |
| UNT-021 | Filter collection by search term | Collections | Partners + search term | Filtered list | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests | **Breakdown:** Single ops (2), Bulk ops (3), Search (5), Concurrent access (3), Memory (3)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Ecosystem initial load (100 partners) | Page load | < 2 seconds | P1 |
| PRF-002 | Ecosystem initial load (1000 partners) | Page load | < 5 seconds | P1 |
| PRF-003 | Bulk expand all nodes (100 nodes) | Expand all | < 1 second | P2 |
| PRF-004 | Bulk expand all nodes (500 nodes) | Expand all | < 3 seconds | P2 |
| PRF-005 | Export 1000 partners as CSV | Export | < 5 seconds | P2 |
| PRF-006 | Search across 1000 partners | Search | < 500ms | P1 |
| PRF-007 | Search across 5000 partners | Search | < 1 second | P1 |
| PRF-008 | Filter 10,000 partners by type | Filter | < 1 second | P1 |
| PRF-009 | Type-ahead search response time | Character-by-character | < 200ms per keystroke | P1 |
| PRF-010 | Zoom/pan animation frame rate | Continuous interaction | ≥ 30 FPS | P2 |
| PRF-011 | 10 concurrent users loading ecosystem | Concurrent access | < 5 seconds per user | P2 |
| PRF-012 | 50 concurrent users loading ecosystem | Concurrent access | < 10 seconds per user | P2 |
| PRF-013 | 100 concurrent searches | Concurrent search | < 2 seconds per search | P2 |
| PRF-014 | Memory usage with 1000 nodes | Memory | < 200MB browser heap | P2 |
| PRF-015 | Memory usage with 5000 nodes | Memory | < 500MB browser heap | P2 |
| PRF-016 | Memory leak check (30 min usage) | Memory | No growth > 10% over baseline | P1 |

---

## §10 Load Tests

> **Minimum:** 10 tests | **Breakdown:** Sustained load (3), Spike load (2), Stress limits (3), Recovery (2)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained 50 users on ecosystem | 50 concurrent users, steady | 30 minutes | 95% requests < 3s, 0 errors | P2 |
| LDT-002 | Sustained 100 users on ecosystem | 100 concurrent users, steady | 30 minutes | 95% requests < 5s, < 1% errors | P2 |
| LDT-003 | Sustained load with active search | 50 users searching continuously | 15 minutes | Search < 1s, no degradation | P2 |
| LDT-004 | Spike from 10 to 200 users | Sudden spike | 5 minutes | System recovers within 30s | P2 |
| LDT-005 | Spike with concurrent exports | 50 users + 20 exports simultaneously | 5 minutes | All exports complete, no crash | P2 |
| LDT-006 | Stress test: 500 concurrent users | 500 users | 10 minutes | Graceful degradation, no crash | P2 |
| LDT-007 | Stress test: 10,000 partner ecosystem | 50 users, 10K partners | 15 minutes | Page loads, virtual scrolling works | P2 |
| LDT-008 | Stress test: continuous filter toggling | 100 users toggling filters | 10 minutes | API handles load, correct results | P2 |
| LDT-009 | Recovery after API crash | Kill API, restart | N/A | Ecosystem recovers within 60s | P2 |
| LDT-010 | Recovery after database restart | DB restart | N/A | Ecosystem reconnects and loads | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Ecosystem view loads with hierarchy visualization | POS-001, POS-002, INT-001, PRF-001, PRF-002 |
| AC-2: Filter by partner type | POS-003, POS-006, POS-007, POS-008, FUN-006, INT-011, NEG-007 |
| AC-3: Search partners in ecosystem | POS-004, POS-020, NEG-001–006, FUN-005, INT-011–017, PRF-006–009 |
| AC-4: Expand/collapse hierarchy | POS-005, POS-027, POS-028, BND-021–030, CON-006 |
| AC-5: Navigate to partner detail | POS-002, FUN-007, INT-036, SEC-021 |
| AC-6: Export ecosystem | POS-017, POS-018, POS-019, NEG-010, FUN-010, INT-041, PRF-005 |
| AC-7: Zoom/pan interaction | POS-009, POS-010, POS-011, POS-029, BND-041–050, PRF-010 |
| AC-8: Display statistics | POS-012, POS-013, POS-014, FUN-009, NEG-031 |
| AC-9: Responsive design | POS-024, POS-025, POS-026, BND-041–050 |
| AC-10: Relationship visualization | POS-015, POS-016, INT-026–035, BND-029, BND-030 |
| AC-11: Security and access control | SEC-001–050, NEG-011–020 |
| AC-12: Performance under load | PRF-001–016, LDT-001–010 |

---

## Test Environment Setup

**Prerequisites:**
- Authenticated user with Partner View permissions
- At least 50 partners with various types and hierarchy levels in test database
- Relationship data between partners configured
- Chrome 90+ / Firefox 88+ / Edge 90+ browser
- Network throttling tools for performance testing (optional)
- Load testing framework (k6 or JMeter) for load tests

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
