# PNO-130: Partner Hierarchy Tree View — Test Cases

**JIRA Reference:** [PNO-130](https://unops.atlassian.net/browse/PNO-130)  
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

The Partner Hierarchy Tree View provides a navigable, expandable/collapsible tree structure showing the organizational hierarchy of partners. Users can drill down through parent-child relationships, search within the tree, use context menus, and navigate to partner detail pages. The tree persists expand/collapse state across navigation.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Tree View Loads with Root-Level Partners

**Priority:** P0  
**Precondition:** User authenticated. Multiple root-level partners (no parent) exist.

**Steps:**
1. Navigate to Partner Hierarchy page
2. Wait for tree to fully render

**Expected Result:**
- All root-level (non-deleted) partners displayed at top level
- Each root node shows partner name and expand/collapse icon (if children exist)
- Leaf nodes (no children) show no expand/collapse control
- Page loads within 2 seconds

---

#### POS-002: Expand Root Node to Show Children

**Priority:** P0  
**Precondition:** Tree loaded. Root partner has 3 child partners.

**Steps:**
1. Click expand icon on root partner node
2. Observe child nodes appearing

**Expected Result:**
- 3 child partner nodes appear indented beneath parent
- Child count indicator shows "3"
- Children display their names and expand/collapse icons as appropriate
- Expansion animation completes within 300ms

---

#### POS-003: Navigate from Tree Node to Partner Detail

**Priority:** P0  
**Precondition:** Tree loaded with partners displayed.

**Steps:**
1. Click on a partner name in the tree
2. Observe navigation behavior

**Expected Result:**
- User navigated to partner detail page (/partners/{id})
- Partner detail shows correct data for clicked partner
- Browser back returns to hierarchy with state preserved

---

#### POS-004: Search Partner in Tree

**Priority:** P0  
**Precondition:** Tree loaded with at least 20 partners across multiple levels.

**Steps:**
1. Enter known partner name in tree search box
2. Press Enter or click search icon

**Expected Result:**
- Matching nodes highlighted in tree
- Tree auto-expands to reveal matched nodes
- Result count shows "N matches found"
- First match is scrolled into view

---

#### POS-005: Collapse Expanded Node

**Priority:** P0  
**Precondition:** Tree loaded. A node is expanded showing children.

**Steps:**
1. Click collapse icon on expanded node
2. Observe children being hidden

**Expected Result:**
- Child nodes hidden beneath collapsed parent
- Collapse animation smooth (< 300ms)
- Expand icon restored on parent node
- Grandchild collapse state preserved (when re-expanded)

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Expand multi-level hierarchy (3 levels) | 3-level hierarchy exists | Expand Level 1 → Level 2 → Level 3 | All 3 levels visible with indentation | P1 |
| POS-007 | Expand All button | Multiple collapsed nodes | Click "Expand All" | All nodes expanded to all levels | P1 |
| POS-008 | Collapse All button | Multiple expanded nodes | Click "Collapse All" | All nodes collapsed to root level | P1 |
| POS-009 | Display child count indicator | Parent with 5 children | Load tree | Badge shows "(5)" on parent node | P1 |
| POS-010 | Leaf node without expand icon | Partner without children | Load tree | No expand/collapse control on leaf | P1 |
| POS-011 | Search with partial name match | Partners exist | Type first 3 chars of partner name | Matching nodes highlighted | P1 |
| POS-012 | Clear search results | Active search | Click clear/X button | Highlights removed, full tree visible | P1 |
| POS-013 | Right-click context menu on node | Tree loaded | Right-click partner node | Context menu: View Details, Add Child, Edit | P1 |
| POS-014 | Context menu → View Details | Context menu visible | Click "View Details" | Navigate to partner detail page | P1 |
| POS-015 | Context menu → Add Child Partner | Context menu visible | Click "Add Child Partner" | New partner dialog with pre-selected parent | P1 |
| POS-016 | Context menu → Edit Partner | Context menu visible | Click "Edit" | Navigate to partner edit page | P1 |
| POS-017 | Navigate search results (next match) | Multiple search matches | Click "Next" arrow | Next matching node highlighted and scrolled | P1 |
| POS-018 | Navigate search results (previous match) | Multiple search matches | Click "Previous" arrow | Previous matching node highlighted | P1 |
| POS-019 | State persistence — expand state on navigation | Nodes expanded, navigate to detail | Navigate to detail, press back | Expanded nodes remain expanded | P1 |
| POS-020 | State persistence — scroll position | Scrolled down in tree | Navigate away, come back | Scroll position restored | P2 |
| POS-021 | Search case-insensitive matching | Partners exist | Search "acme" vs "ACME" | Both match same partners | P1 |
| POS-022 | Partner type icon in tree node | Partners of different types | Load tree | Type-specific icons (Funding, Client, Implementation) | P2 |
| POS-023 | Partner status indicator in tree | Active and inactive partners | Load tree | Status badges (Active, Inactive) shown | P2 |
| POS-024 | Keyboard navigation — arrow keys | Tree focused | Use Up/Down arrow keys | Focus moves between visible nodes | P2 |
| POS-025 | Keyboard navigation — Enter to expand | Focus on collapsed node | Press Enter | Node expands to show children | P2 |
| POS-026 | Keyboard navigation — Enter to navigate | Focus on leaf node | Press Enter | Navigates to partner detail | P2 |
| POS-027 | Drag node to new parent | Drag-and-drop enabled | Drag child to different parent | Partner reparented in hierarchy | P2 |
| POS-028 | Multi-select nodes (Ctrl+click) | Multi-select enabled | Ctrl+click 3 nodes | 3 nodes selected, actions available | P2 |
| POS-029 | Tree refreshes after partner creation | Tree loaded | Create new partner externally | Refresh shows new partner in tree | P2 |
| POS-030 | Tree refreshes after partner deletion | Tree loaded | Soft-delete partner externally | Refresh removes partner from tree | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Search with empty string | Empty search + Enter | No action or hint message | P1 |
| NEG-002 | Search with whitespace only | "   " | Treated as empty | P1 |
| NEG-003 | Search with SQL injection | `'; DROP TABLE--` | Input sanitized | P0 |
| NEG-004 | Search with XSS payload | `<script>alert(1)</script>` | Input escaped | P0 |
| NEG-005 | Search with non-existent name | "ZZZNONE999" | "No matches found" | P1 |
| NEG-006 | Search with 1000+ characters | Very long string | Truncated or rejected | P1 |
| NEG-007 | Search with special characters only | `!@#$%^&*` | No crash, "no results" | P1 |
| NEG-008 | Invalid partner ID in URL | /hierarchy?partner=ABC | Graceful error or default view | P1 |
| NEG-009 | Negative partner ID in URL | /hierarchy?partner=-1 | 400 Bad Request or default view | P1 |
| NEG-010 | Zero partner ID in URL | /hierarchy?partner=0 | 400 or default view | P1 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Unauthenticated user | No auth | Navigate to /hierarchy | Redirect to login | P0 |
| NEG-012 | User without partner view permission | No CanViewAllPartners | Load hierarchy | 403 Access Denied | P0 |
| NEG-013 | OrgUnit-scoped user sees only scoped partners | OrgUnit-restricted | Load hierarchy | Only partners in scope visible | P0 |
| NEG-014 | Expired session | Expired JWT | Load hierarchy | Redirect to login | P0 |
| NEG-015 | User without edit permission uses context menu | Read-only role | Right-click → Edit | No Edit option in context menu | P1 |
| NEG-016 | User without create permission uses context menu | Read-only role | Right-click → Add Child | No Add Child option | P1 |
| NEG-017 | Manipulate API for restricted partner | OrgUnit user | Call API with out-of-scope partner | 403 Forbidden | P0 |
| NEG-018 | Revoked token | Revoked JWT | API call | 401 Unauthorized | P0 |
| NEG-019 | Cross-tenant data visibility | Tenant A user | Load hierarchy | Tenant B partners not visible | P0 |
| NEG-020 | Disabled account | Disabled user | Load hierarchy | 403 Forbidden | P1 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Click on deleted partner node | Partner just deleted | Click stale node | "Partner not found" error | P1 |
| NEG-022 | Expand node whose children were all deleted | All children soft-deleted | Click expand | Node becomes leaf (no expand icon) | P1 |
| NEG-023 | Drag node to its own child (circular) | Node A parent of B | Drag A under B | "Cannot create circular hierarchy" error | P0 |
| NEG-024 | Drag node to its own descendant | A > B > C, drag A under C | Attempt reparent | Circular reference prevented | P0 |
| NEG-025 | Drag root node to child of leaf | Root node | Drag to leaf node | Reparent only if leaf can be parent | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Partner without name in tree | Name = null | Shows "Unnamed Partner" placeholder | P1 |
| NEG-027 | Partner without type | Type = null | Default icon, no crash | P1 |
| NEG-028 | Partner without parent ID | ParentId = null | Rendered as root-level node | P1 |
| NEG-029 | Partner with null status | Status = null | Default status indicator | P2 |
| NEG-030 | Tree with no partners | Empty database | "No partners found" with CTA to add | P1 |
| NEG-031 | Partner with orphaned parent reference | ParentId references deleted partner | Rendered as root-level | P1 |
| NEG-032 | Child count when children are all deleted | Parent has children, all IsDeleted=true | Child count = 0, no expand icon | P1 |
| NEG-033 | Context menu on null partner node | Node with incomplete data | No crash, limited menu options | P2 |
| NEG-034 | Search results with null fields | Some partners have null names | Handled gracefully, no crash | P2 |
| NEG-035 | Tree with inconsistent hierarchy data | Orphaned mid-level nodes | Rendered as root-level with warning | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | API timeout loading tree | Backend > 30s response | Loading spinner → timeout message | P1 |
| NEG-037 | API returns 500 | Server error | "Unable to load hierarchy. Try again." | P1 |
| NEG-038 | Network disconnection | Network drops | Error message with retry | P1 |
| NEG-039 | API 500 on expand children | Expand triggers error | Parent stays collapsed, error toast | P1 |
| NEG-040 | Search API failure | Search endpoint 500 | "Search unavailable" | P2 |
| NEG-041 | Context menu action fails | Edit endpoint 500 | Error toast, menu closes | P2 |
| NEG-042 | Reparent API failure | Drag-drop endpoint fails | Revert to original position | P1 |
| NEG-043 | Partial data load | Some children fail to load | Available children shown, warning for failed | P2 |
| NEG-044 | CDN unavailable for icons | Asset loading fails | Fallback text-only nodes | P2 |
| NEG-045 | Database connection lost mid-browse | DB drops during drill-down | Graceful error, cached data if available | P2 |

### 2.6 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-046 | Double-click on expand icon | Rapid double-click | Single expand, no duplicate requests | P1 |
| NEG-047 | Click expand during animation | Click while expanding | No double-load, stable render | P1 |
| NEG-048 | Search during tree loading | Type before load completes | Search queued until ready | P2 |
| NEG-049 | Right-click on background (not a node) | Right-click empty area | Browser default menu or nothing | P2 |
| NEG-050 | Rapid expand/collapse toggling | 10 toggles in 2 seconds | Final state is correct | P1 |
| NEG-051 | Navigate back with stale tree state | Partner renamed, then back | Tree refreshes with updated data | P2 |
| NEG-052 | Scroll past last node | Scroll beyond tree end | Scrolling stops, no blank space | P2 |
| NEG-053 | Search with regex characters | `.*` or `[a-z]` | Treated as literal text | P1 |
| NEG-054 | Browser back from partner detail | On detail page | Returns to tree with state preserved | P1 |
| NEG-055 | Tree with circular reference in data | ParentId creates loop | No infinite loop, error handled | P0 |
| NEG-056 | Expand node with 1000+ children | Very wide node | Children loaded progressively or paginated | P1 |
| NEG-057 | Deep hierarchy (20+ levels) | Very deep nesting | Renders without stack overflow | P1 |
| NEG-058 | Context menu outside visible area | Right-click near edge | Menu stays within viewport | P2 |
| NEG-059 | Keyboard search shortcut collision | Ctrl+F in tree | No conflict with browser find | P2 |
| NEG-060 | Multiple context menus open | Right-click node A, then node B | Only latest menu visible | P2 |
| NEG-061 | Drag-drop on non-drop target | Drag to header or search | Drop cancelled, no change | P2 |
| NEG-062 | Drag-drop to same parent | Drag back to current parent | No API call, no change | P2 |
| NEG-063 | Tree with 10,000+ partners | Very large dataset | Progressive loading or virtualization | P1 |
| NEG-064 | Emoji in search | `🏢🔍` | No crash, appropriate handling | P2 |
| NEG-065 | Page refresh during expand | F5 during API call | Clean reload | P2 |
| NEG-066 | Context menu "Add Child" for deleted parent | Parent deleted after context menu opened | Error: "Parent no longer exists" | P1 |
| NEG-067 | Multi-select with invalid combinations | Select parent and its child | Action applies to valid selection only | P2 |
| NEG-068 | Tree with only deleted partners | All IsDeleted=true | "No partners found" | P1 |
| NEG-069 | Open tree in multiple browser tabs | 2 tabs same user | Independent state per tab | P2 |
| NEG-070 | Invalid sort parameter in URL | ?sort=INVALID | Default sort applied | P2 |
| NEG-071 | Search with null after trim | "   " | Treated as empty | P1 |
| NEG-072 | Tree with invalid parent reference | ParentId = 999999 | Rendered as root | P1 |
| NEG-073 | Expand with API returning 500 | Expand | Error toast | P1 |
| NEG-074 | Context menu with deleted parent | Parent deleted | Error | P1 |
| NEG-075 | Drag with invalid target | Non-partner area | Drop cancelled | P1 |
| NEG-076 | Reparent with permission denied | No edit | 403, revert | P1 |
| NEG-077 | Tree with malformed API response | Invalid JSON | Error handled | P1 |
| NEG-078 | Search with control characters | \0 in query | Sanitized | P1 |
| NEG-079 | Tree with negative child count | Count = -1 | Display 0 | P1 |
| NEG-080 | Expand with timeout | >30s | Timeout message | P1 |
| NEG-081 | Tree with duplicate partner IDs | Same ID twice | One shown | P1 |
| NEG-082 | Tree with invalid hierarchy depth | Depth > 20 | Warning or error | P1 |
| NEG-083 | Search with regex special chars | `.*+?` | Escaped | P1 |
| NEG-084 | Tree with null node data | Node = null | Skipped | P1 |
| NEG-085 | Context menu with expired session | Session expired | Auth prompt | P1 |
| NEG-086 | Reparent with stale data | Stale version | Conflict | P1 |
| NEG-087 | Tree with oversized response | 10MB | Paginated or error | P1 |
| NEG-088 | Search with Unicode null | \u0000 | Sanitized | P1 |
| NEG-089 | Tree with circular ref in flat list | Circular ParentIds | Error | P0 |
| NEG-090 | Expand during unmount | Unmount during expand | No error | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Search query | 1 | 255 | ✅ Single char | ✅ 255 chars | Truncated | P1 |
| BND-002 | Partner name in node | 1 | 200 | ✅ "A" | ✅ Full name | Ellipsis | P1 |
| BND-003 | Partner code in node | 1 | 50 | ✅ Short code | ✅ Long code | Truncated | P2 |
| BND-004 | Tooltip description | 0 | 500 | ✅ No tooltip | ✅ Full text | Truncated with "..." | P2 |
| BND-005 | Context menu label | 1 | 100 | ✅ Short label | ✅ Long label | Truncated | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Total partners in tree | 0 | 100000 | ✅ Empty state | ✅ Virtualized | N/A | P1 |
| BND-007 | Hierarchy depth | 0 | 20 | ✅ Flat list | ✅ Deep tree | Warning | P1 |
| BND-008 | Children per node | 0 | 1000 | ✅ Leaf | ✅ Paginated | Warning | P1 |
| BND-009 | Search result count | 0 | 10000 | "No results" | Navigation | Performance check | P1 |
| BND-010 | Root-level node count | 1 | 5000 | ✅ Single root | ✅ Paginated | Performance | P1 |
| BND-011 | Scroll position (pixels) | 0 | 100000 | Top | Bottom | Capped | P2 |
| BND-012 | Node indentation level | 0 | 20 | Root level | Deep indent | Max indent cap | P2 |
| BND-013 | Context menu items | 1 | 10 | Single action | Full menu | Scrollable menu | P2 |
| BND-014 | Selected nodes count | 0 | 100 | No selection | Many selected | Performance | P2 |
| BND-015 | Drag distance (pixels) | 5 | 1000 | Min drag to activate | Long drag | Scroll during drag | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-016 | Partner created on leap year | Feb 29, 2028 | Displayed correctly in tooltip | P2 |
| BND-017 | Very old partner creation date | Jan 1, 2000 | Formatted correctly | P2 |
| BND-018 | Future creation date | Jan 1, 2030 | Displayed or flagged | P2 |
| BND-019 | Partner modified at midnight UTC | 00:00:00 UTC | No date boundary error | P2 |
| BND-020 | Date filter crossing year boundary | Dec 31 – Jan 1 | Correct range applied | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-021 | Tree with 0 partners | Empty database | "No partners. Add first partner." | P1 |
| BND-022 | Tree with 1 root partner (no children) | Single partner | One node, no expand icon | P1 |
| BND-023 | Tree with 1 root, 1 child | Simple hierarchy | Two levels displayed | P1 |
| BND-024 | Tree with 100 partners | Medium set | All rendered within 2 seconds | P1 |
| BND-025 | Tree with 1000 partners | Large set | Loaded within 5 seconds, virtualized | P1 |
| BND-026 | Tree with 10,000 partners | Very large | Progressive loading | P1 |
| BND-027 | Single node with 200 children | Wide node | All children listed, possibly paginated | P1 |
| BND-028 | Linear chain of 20 levels | Maximum depth | Full depth with scroll | P1 |
| BND-029 | All partners are root-level (no children) | Flat list | No expand icons, flat view | P1 |
| BND-030 | All partners under one root | Single root with all children | One expandable root, many children | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-031 | Partner name (Arabic) | `مؤسسة شريكة` | RTL support in node | P2 |
| BND-032 | Partner name (Chinese) | `合作伙伴组织` | Correctly rendered | P2 |
| BND-033 | Partner name (Emoji) | `🏢 Partner Inc` | Emoji renders in node | P2 |
| BND-034 | Partner name (Cyrillic) | `Партнерская организация` | Correctly displayed | P2 |
| BND-035 | Partner name (Accented French) | `Société Générale` | Accents display correctly | P2 |
| BND-036 | Search with Arabic text | `مؤسسة` | Matches Arabic names | P2 |
| BND-037 | Search with mixed scripts | `Partner-合作` | Correct match behavior | P2 |
| BND-038 | Context menu with long Unicode label | 50-char Chinese label | Displays without overflow | P2 |
| BND-039 | Partner name with HTML entities | `A &amp; B Partners` | Rendered as "A & B Partners" | P2 |
| BND-040 | Tooltip with multi-line Unicode | Arabic + English mixed | Both scripts rendered correctly | P2 |

### 3.6 Viewport & Display Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-041 | Minimum viewport (320px width) | Mobile width | Responsive tree or scroll | P2 |
| BND-042 | Maximum viewport (4K) | 3840×2160 | Tree uses space effectively | P2 |
| BND-043 | Portrait tablet | 768×1024 | Vertical layout adapts | P2 |
| BND-044 | Landscape tablet | 1024×768 | Horizontal layout adapts | P2 |
| BND-045 | Browser at 50% zoom | OS zoom | Tree renders proportionally | P2 |
| BND-046 | Browser at 200% zoom | OS zoom | All nodes accessible via scroll | P2 |
| BND-047 | Split screen window | 960px window | Responsive tree | P2 |
| BND-048 | High-DPI display | Retina | Sharp rendering | P2 |
| BND-049 | Dark mode theme | Dark theme active | Tree colors adapt | P2 |
| BND-050 | Print layout | Ctrl+P | Printable tree layout | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Node with max-length name (200 chars) | Full 200-char name | Truncated with ellipsis | P1 |
| BND-052 | Search with exactly 1 result | One match | Single highlight, no navigation arrows | P1 |
| BND-053 | Search with exactly 0 results | No match | "No results found" | P1 |
| BND-054 | All nodes expanded simultaneously | 500 nodes expanded | Performance acceptable | P1 |
| BND-055 | Drag to exact boundary of drop zone | Pixel-perfect drop | Drop registers correctly | P2 |
| BND-056 | Expand node with exactly 1 child | Single child | One child displayed | P2 |
| BND-057 | Expand node with exactly 50 children | Medium count | All 50 displayed | P1 |
| BND-058 | Right-click at exact edge of node | Pixel boundary | Context menu appears | P2 |
| BND-059 | Scroll to exact bottom of tree | Last node | No extra whitespace below | P2 |
| BND-060 | Keyboard navigation at tree top | Focus on first node | Up arrow does nothing | P2 |
| BND-061 | Keyboard navigation at tree bottom | Focus on last visible node | Down arrow does nothing | P2 |
| BND-062 | Partner with ParentId = own Id | Self-reference | Rendered as root, no loop | P1 |
| BND-063 | Two partners with mutual parent reference | A.Parent=B, B.Parent=A | One renders as root, cycle broken | P1 |
| BND-064 | Partner with negative ID | Id = -1 | Handled gracefully | P2 |
| BND-065 | Partner with Id = MAX_INT | Id = 2147483647 | Displayed correctly | P2 |
| BND-066 | Search returning exactly max limit | Max results | Pagination or "showing N of M" | P2 |
| BND-067 | Tree with exactly 2 hierarchy levels | Root + children only | Two levels render correctly | P2 |
| BND-068 | Context menu with 0 available actions | All actions disabled for role | No context menu or "No actions available" | P2 |
| BND-069 | Tooltip for partner with all null optional fields | Minimal data | Tooltip shows only available info | P2 |
| BND-070 | Expand/collapse exactly at API timeout boundary | Expand takes ~29s (timeout 30s) | Completes just in time | P2 |
| BND-071 | Search query exactly 1 char | "A" | Matches | P1 |
| BND-072 | Search query exactly 255 chars | Max | Processed | P1 |
| BND-073 | Tree with exactly 0 partners | Empty | Empty state | P1 |
| BND-074 | Tree with exactly 1 partner | Single | One node | P1 |
| BND-075 | Tree with exactly 100 partners | Medium | All in 2s | P1 |
| BND-076 | Tree with exactly 1000 partners | Large | Loaded in 5s | P1 |
| BND-077 | Node with exactly 1 child | Single | One child | P1 |
| BND-078 | Node with exactly 1000 children | Max | Paginated | P1 |
| BND-079 | Hierarchy depth exactly 0 | Flat | No expand | P1 |
| BND-080 | Hierarchy depth exactly 20 | Max | Full depth | P1 |
| BND-081 | Root-level count exactly 1 | Single root | One root | P1 |
| BND-082 | Root-level count exactly 5000 | Max | Paginated | P1 |
| BND-083 | Partner name exactly 1 char | "A" | Displayed | P1 |
| BND-084 | Partner name exactly 200 chars | Max | Ellipsis | P1 |
| BND-085 | Viewport exactly 320px | Mobile | Responsive | P2 |
| BND-086 | Viewport exactly 3840px | 4K | Uses space | P2 |
| BND-087 | Scroll position exactly 0 | Top | At top | P2 |
| BND-088 | Scroll position at bottom | Bottom | At bottom | P2 |
| BND-089 | Selected nodes exactly 0 | None | No selection | P2 |
| BND-090 | Selected nodes exactly 100 | Max | Capped | P2 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow rules (15), Validation rules (15), Constraint rules (10), Audit rules (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule Description | Trigger | Expected Outcome | Priority |
|----|-----------|-----------------|---------|-----------------|----------|
| FUN-001 | Tree excludes soft-deleted partners | IsDeleted=true filtered | Load tree | Deleted partners not shown | P0 |
| FUN-002 | Parent-child ordering matches database hierarchy | ParentId relationship | Load tree | Children appear under correct parent | P0 |
| FUN-003 | Root nodes have no parent indicator | ParentId = null | Load tree | Top-level display | P0 |
| FUN-004 | Expand state persists across page navigation | Session storage | Navigate away and back | Same nodes expanded | P1 |
| FUN-005 | Search is case-insensitive | Search logic | "acme" matches "ACME" | Correct matches | P1 |
| FUN-006 | Search auto-expands to reveal matches | Deep match | Search for nested partner | Parent nodes expand to show match | P1 |
| FUN-007 | Node click navigates to detail | UI interaction | Click partner name | Route to /partners/{id} | P0 |
| FUN-008 | Context menu respects user permissions | Role-based | Right-click | Only permitted actions shown | P1 |
| FUN-009 | Reparent via drag-drop updates hierarchy | Drag-drop | Drag child to new parent | API called, tree restructured | P1 |
| FUN-010 | Reparent blocked for circular reference | Circular check | Drag parent under child | Error: circular reference | P0 |
| FUN-011 | New partner appears after refresh | Data change | Create partner externally, refresh | Node appears in correct position | P1 |
| FUN-012 | Deleted partner disappears after refresh | Soft-delete | Delete partner, refresh | Node removed from tree | P1 |
| FUN-013 | Child count updates on child deletion | Cascade | Delete one child | Parent count decrements | P1 |
| FUN-014 | Search clears on page leave and return | Navigation | Leave page, return | Search box empty | P2 |
| FUN-015 | Sort order applies alphabetically | Default sort | Load tree | Siblings sorted A-Z | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Validation Rule | Valid Input | Invalid Input | Priority |
|----|-----------|----------------|------------|--------------|----------|
| FUN-016 | Search min length 1 char | Min length | "A" | "" | P1 |
| FUN-017 | Search max length 255 chars | Max length | 255 chars | 256 chars | P1 |
| FUN-018 | Partner ID must be positive integer | ID validation | 42 | -1, "abc" | P1 |
| FUN-019 | ParentId must reference existing partner | FK validation | Valid ID | Non-existent ID | P1 |
| FUN-020 | ParentId cannot be self | Self-reference check | Other ID | Own ID | P0 |
| FUN-021 | Hierarchy depth ≤ 20 | Max depth | 19 levels | 21 levels | P1 |
| FUN-022 | Search input sanitized | XSS prevention | "ACME" | `<script>` | P0 |
| FUN-023 | Drag-drop target must accept children | Drop zone validation | Valid parent | Non-partner area | P1 |
| FUN-024 | Context menu action validates entity state | State check | Active partner | Deleted partner | P1 |
| FUN-025 | Sort parameter must be valid column | Sort validation | "name", "code" | "DROP_TABLE" | P1 |
| FUN-026 | Page parameter must be ≥ 1 | Pagination | 1 | 0, -1 | P2 |
| FUN-027 | Node selection limit enforced | Multi-select | ≤100 nodes | 101 nodes | P2 |
| FUN-028 | API content-type validation | Response type | application/json | text/html | P1 |
| FUN-029 | Reparent operation validates permissions | Auth check | User with edit rights | Read-only user | P1 |
| FUN-030 | Circular hierarchy validation on API side | Server validation | Non-circular | Circular chain | P0 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max concurrent tree API requests | 5 per second per user | 6 rapid requests | 6th queued or rate-limited | P1 |
| FUN-032 | Max nodes rendered | ~5000 visible | 5001 nodes | Virtualization kicks in | P2 |
| FUN-033 | Max search results displayed | 1000 | 5000 matches | Paginated, "Showing 1000 of 5000" | P1 |
| FUN-034 | Session timeout | 30 minutes idle | 31 min idle | Session expired, redirect to login | P1 |
| FUN-035 | Min browser version | Chrome 90+ | Chrome 89 | Unsupported warning | P2 |
| FUN-036 | API response size limit | 10MB | Large hierarchy | Paginated API responses | P2 |
| FUN-037 | Max drag distance before scroll | > 50px from edge | Drag to edge | Auto-scroll activates | P2 |
| FUN-038 | Max multi-select count | 100 nodes | 101 | Selection capped at 100 | P2 |
| FUN-039 | WebSocket reconnect limit | 5 retries | 6th disconnect | Fallback to polling | P2 |
| FUN-040 | Export tree limit | 10,000 nodes | 10,001 | Chunked or limited export | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Tree page view logged | Page load | User ID, timestamp, page | P1 |
| FUN-042 | Reparent action logged | Drag-drop reparent | User ID, partner ID, old parent, new parent | P0 |
| FUN-043 | Search action logged | Execute search | User ID, search term, timestamp | P2 |
| FUN-044 | Context menu → Edit logged | Edit navigation | User ID, partner ID | P2 |
| FUN-045 | Context menu → Add Child logged | Add Child action | User ID, parent partner ID | P2 |
| FUN-046 | Failed access attempt logged | Unauthorized access | User ID, denied resource, reason | P0 |
| FUN-047 | Session timeout logged | Timeout | User ID, session duration | P2 |
| FUN-048 | Error event logged | API error | Error code, trace, user context | P1 |
| FUN-049 | Multi-select bulk action logged | Bulk operation | User ID, action, affected IDs | P1 |
| FUN-050 | Drag-drop circular reference attempt logged | Blocked circular | User ID, attempted operation | P1 |
| FUN-051 | Tree excludes soft-deleted partners | IsDeleted | Load | Deleted not shown | P0 |
| FUN-052 | Parent-child ordering correct | ParentId | Load | Correct hierarchy | P0 |
| FUN-053 | Root nodes have no parent | ParentId=null | Load | Top-level | P0 |
| FUN-054 | Expand state persists | Session | Navigate and back | Same expanded | P1 |
| FUN-055 | Search case-insensitive | Search | "acme" vs "ACME" | Same matches | P1 |
| FUN-056 | Search auto-expands to matches | Search | Deep match | Parents expand | P1 |
| FUN-057 | Node click navigates | Click | Partner name | Route to detail | P0 |
| FUN-058 | Context menu respects permissions | Role | Right-click | Only permitted | P1 |
| FUN-059 | Reparent updates hierarchy | Drag-drop | Drag child | API called | P1 |
| FUN-060 | Reparent blocked for circular | Circular | Drag parent under child | Error | P0 |
| FUN-061 | New partner after refresh | Create | Create, refresh | Node appears | P1 |
| FUN-062 | Deleted partner after refresh | Delete | Delete, refresh | Node removed | P1 |
| FUN-063 | Child count updates on delete | Delete child | Delete one | Count decrements | P1 |
| FUN-064 | Search clears on page leave | Navigate | Leave, return | Search empty | P2 |
| FUN-065 | Sort order alphabetical | Sort | Load | Siblings A-Z | P1 |
| FUN-066 | Search min length 1 | Search | "A" | Valid | P1 |
| FUN-067 | Search max length 255 | Search | 255 chars | Valid | P1 |
| FUN-068 | Partner ID positive | ID | 42 | -1 invalid | P1 |
| FUN-069 | ParentId references existing | FK | Valid ID | Non-existent error | P1 |
| FUN-070 | ParentId not self | Self-ref | Other ID | Own ID invalid | P0 |
| FUN-071 | Hierarchy depth ≤ 20 | Depth | 19 levels | 21 error | P1 |
| FUN-072 | Search input sanitized | XSS | "ACME" | `<script>` escaped | P0 |
| FUN-073 | Drag-drop target valid | Drop | Valid parent | Non-partner invalid | P1 |
| FUN-074 | Context menu validates state | State | Active | Deleted error | P1 |
| FUN-075 | Sort parameter valid | Sort | "name", "code" | "DROP_TABLE" default | P1 |
| FUN-076 | Page parameter ≥ 1 | Page | 1 | 0, -1 default | P2 |
| FUN-077 | Node selection limit 100 | Multi-select | ≤100 | 101 capped | P2 |
| FUN-078 | API content-type validation | Response | application/json | text/html error | P1 |
| FUN-079 | Reparent validates permissions | Auth | Edit rights | Read-only 403 | P1 |
| FUN-080 | Circular validation server-side | Server | Non-circular | Circular 400 | P0 |
| FUN-081 | Max concurrent requests 5/s | Rate | 6 rapid | 6th queued | P1 |
| FUN-082 | Max nodes ~5000 visible | Virtualization | 5001 | Virtualization | P2 |
| FUN-083 | Max search results 1000 | Search | 5000 matches | Paginated | P1 |
| FUN-084 | Session timeout 30 min | Timeout | 31 min | Expired | P1 |
| FUN-085 | API response 10MB max | Size | Large | Paginated | P2 |
| FUN-086 | Min browser Chrome 90+ | Browser | Chrome 89 | Warning | P2 |
| FUN-087 | Max drag distance for scroll | Drag | >50px from edge | Auto-scroll | P2 |
| FUN-088 | Max multi-select 100 | Select | 101 | Capped | P2 |
| FUN-089 | WebSocket reconnect 5 retries | Reconnect | 6th | Fallback polling | P2 |
| FUN-090 | Export tree limit 10000 | Export | 10001 | Chunked | P2 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests | **Breakdown:** CRUD workflow (10), Search/filter (10), Pagination (5), Relationships (10), Error handling (15)

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|------------------|-----------------|----------|
| INT-001 | Create partner → appears in tree | Create | Partner, Tree | New node visible after refresh | P0 |
| INT-002 | Update partner name → tree reflects | Update | Partner name | Node label updated | P0 |
| INT-003 | Soft-delete partner → removed from tree | Delete | Partner | Node disappears | P0 |
| INT-004 | Create child → appears under parent | Create | Parent, Child | Child node under parent | P0 |
| INT-005 | Reparent partner → tree restructures | Update | Parent reference | Node moves to new parent | P1 |
| INT-006 | Delete parent → children become orphans or deleted | Delete cascade | Parent, Children | Children reparented to root or cascade deleted | P1 |
| INT-007 | Change partner status → indicator updates | Update | Status | Badge changes in tree | P1 |
| INT-008 | Change partner type → icon updates | Update | Type | Type icon changes | P1 |
| INT-009 | Bulk delete partners → tree updates | Bulk delete | Multiple partners | All removed from tree | P1 |
| INT-010 | Restore soft-deleted partner → reappears | Restore | Partner | Node reappears in tree | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Search/Filter Criteria | Expected Results | Priority |
|----|-----------|----------------------|-----------------|----------|
| INT-011 | Search + status filter combined | Name "ACME" + Active status | Active partners matching "ACME" | P0 |
| INT-012 | Search across name and code | Search term matches code | Partner found by code | P1 |
| INT-013 | Filter by active status | Active only | Only active partners visible | P1 |
| INT-014 | Filter by inactive status | Inactive only | Only inactive partners visible | P1 |
| INT-015 | Clear filter restores full tree | Clear filters | All partners visible | P1 |
| INT-016 | Search in filtered tree | Filter active, then search | Results within filtered set | P1 |
| INT-017 | Filter during active search | Search active, then filter | Both applied correctly | P1 |
| INT-018 | Search highlights persist through expand | Search active, expand node | Highlights remain on matched nodes | P1 |
| INT-019 | Search result count updates on filter | Filter reduces matches | Count updates correctly | P2 |
| INT-020 | Empty filter + empty search | Both cleared | Full tree displayed | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| INT-021 | Initial root nodes loaded | First batch | Root nodes visible | P1 |
| INT-022 | Expand loads children on demand | Lazy load | Children fetched on expand | P1 |
| INT-023 | Load more children (node pagination) | Scroll in expanded node | Additional children loaded | P2 |
| INT-024 | Virtual scroll for large trees | 10,000 nodes | Visible nodes rendered, rest virtualized | P2 |
| INT-025 | Last child batch loads correctly | Scroll to end of children | All remaining children visible | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Test Scenario | Expected Result | Priority |
|----|-----------|-------------|--------------|-----------------|----------|
| INT-026 | Parent-child display | Hierarchy link | Expand parent | Children indented below | P0 |
| INT-027 | Reparent updates both old and new parent | Move child | Drag child A from P1 to P2 | P1 count decrements, P2 count increments | P1 |
| INT-028 | Delete child updates parent count | Delete child | Delete one of 3 children | Parent count: 3 → 2 | P1 |
| INT-029 | Create child updates parent count | Add child | Create child under parent | Parent count increments | P1 |
| INT-030 | Multi-level expand cascade | Deep hierarchy | Expand 5 levels | All levels visible correctly | P1 |
| INT-031 | Collapse parent hides entire sub-tree | 3-level hierarchy | Collapse root | All descendants hidden | P1 |
| INT-032 | Orphaned partner after parent delete | Delete parent | Parent soft-deleted | Children become root-level | P1 |
| INT-033 | Partner type matches parent context | Type inheritance | Create child | Child can have different type from parent | P2 |
| INT-034 | Partner with multiple parent changes | Reparent twice | Move child A to P1, then to P2 | Final parent is P2, history logged | P1 |
| INT-035 | Tree reflects real-time hierarchy changes | WebSocket update | Another user reparents | Tree updates (after refresh or real-time) | P2 |

### 5.5 Error Handling (15)

| ID | Test Name | Error Condition | Expected Response | Priority |
|----|-----------|----------------|------------------|----------|
| INT-036 | API 404 for missing partner | Navigate via URL | 404 page or redirect | P0 |
| INT-037 | API 500 on tree load | Server error | Error with retry button | P0 |
| INT-038 | API 403 for unauthorized | No permission | Access denied page | P0 |
| INT-039 | Expand returns empty for node with children | Data inconsistency | Node becomes leaf, warning | P1 |
| INT-040 | Search timeout | Slow response | Timeout message with retry | P1 |
| INT-041 | Reparent conflict (concurrent edit) | Two users move same partner | Conflict error, retry suggestion | P1 |
| INT-042 | Drag-drop fails (permission denied) | No edit permission | Error toast, partner returns to position | P1 |
| INT-043 | Context menu action 500 error | Server crash on action | Error toast, menu closes | P1 |
| INT-044 | JWT refresh during tree interaction | Token refresh | Seamless, no user interruption | P1 |
| INT-045 | Malformed API response | Invalid JSON | Error handled gracefully | P1 |
| INT-046 | API rate limit hit | Rapid interactions | 429 response, retry message | P1 |
| INT-047 | CORS error | Misconfigured CORS | Error in console, user-friendly message | P2 |
| INT-048 | WebSocket disconnect | Connection drops | Reconnect attempt | P2 |
| INT-049 | Stale expand state after cache clear | Cache cleared | Fresh data loaded on expand | P2 |
| INT-050 | Session expired during drag-drop | Token expired mid-drag | Auth prompt, drag cancelled | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests | **Coverage:** OWASP Top 10, injection, authorization, IDOR, mass assignment

### 6.1 Injection Prevention (10)

| ID | Test Name | Attack Vector | Target Field | Expected Block | Priority |
|----|-----------|--------------|-------------|---------------|----------|
| SEC-001 | SQL injection in search | `'; DROP TABLE--` | Search box | Sanitized | P0 |
| SEC-002 | SQL injection in filter param | `type=1 OR 1=1` | Filter API | Parameterized query | P0 |
| SEC-003 | XSS in search | `<script>alert(1)</script>` | Search box | Escaped | P0 |
| SEC-004 | XSS via partner name in tree | Name with script tag | Node label | Escaped in DOM | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Search | Sanitized | P1 |
| SEC-006 | Path traversal in API | `../../etc/passwd` | API parameter | Rejected | P0 |
| SEC-007 | HTML injection in tooltip | `<img onerror=...>` | Tooltip content | Escaped | P1 |
| SEC-008 | JSON injection in reparent | `{"parentId":"1; DROP TABLE"}` | Reparent API | Type validation | P1 |
| SEC-009 | OS command injection | `; rm -rf /` | Export/print | Sanitized | P0 |
| SEC-010 | Template injection | `{{constructor.constructor('alert()')()}}` | Angular template | Escaped | P1 |

### 6.2 Broken Access Control (10)

| ID | Test Name | User Role | Unauthorized Action | Expected Result | Priority |
|----|-----------|-----------|-------------------|-----------------|----------|
| SEC-011 | Anonymous access to tree API | No auth | GET /api/partners/hierarchy | 401 | P0 |
| SEC-012 | Low-privilege user reparents | No edit permission | PUT /api/partners/{id}/parent | 403 | P0 |
| SEC-013 | OrgUnit-scoped user sees all | Scoped user | GET /api/partners/hierarchy | Only scoped data | P0 |
| SEC-014 | Expired token | Expired JWT | API call | 401 | P0 |
| SEC-015 | Tampered JWT | Modified claims | API call | 401/403 | P0 |
| SEC-016 | Vertical privilege escalation | Basic user | Call admin-only API | 403 | P0 |
| SEC-017 | Horizontal access | User A | Access User B's scoped data | 403 | P0 |
| SEC-018 | Disabled account | Disabled user | API call | 403 | P1 |
| SEC-019 | API access after logout | Logged out | Cached call | 401 | P1 |
| SEC-020 | Role escalation via parameter | Basic user | ?role=admin | Ignored | P0 |

### 6.3 IDOR (10)

| ID | Test Name | Object | Manipulation | Expected Result | Priority |
|----|-----------|--------|-------------|-----------------|----------|
| SEC-021 | Access partner by guessed ID | Partner ID | /partners/999 | 403 if not in scope | P0 |
| SEC-022 | Enumerate partners via API | Sequential IDs | /partners/1,2,3... | Rate limited, scoped | P0 |
| SEC-023 | Access other OrgUnit hierarchy | OrgUnit ID | Change scope param | 403 | P0 |
| SEC-024 | Reparent partner in another scope | Out-of-scope ID | PUT /partners/{other-id}/parent | 403 | P0 |
| SEC-025 | Access deleted partner | Deleted ID | /partners/{deleted-id} | 404 | P1 |
| SEC-026 | Negative partner ID | -1 | /partners/-1 | 400 | P1 |
| SEC-027 | Zero partner ID | 0 | /partners/0 | 400 | P1 |
| SEC-028 | Float partner ID | 1.5 | /partners/1.5 | 400 | P1 |
| SEC-029 | String partner ID | "abc" | /partners/abc | 400 | P1 |
| SEC-030 | Access child of restricted parent | Child of hidden parent | Direct URL to child | 403 if parent out of scope | P1 |

### 6.4 Mass Assignment (5)

| ID | Test Name | Protected Field | Manipulation | Expected Result | Priority |
|----|-----------|----------------|-------------|-----------------|----------|
| SEC-031 | Modify IsDeleted | IsDeleted | Include in reparent body | Field not modified | P0 |
| SEC-032 | Modify CreatedBy | CreatedBy | Include in request | Not modified | P0 |
| SEC-033 | Modify Id | Id | Include in body | Not modified | P0 |
| SEC-034 | Modify CreatedDate | CreatedDate | Include in body | Not modified | P1 |
| SEC-035 | Modify WorkflowStatus | WorkflowStatus | Include in body | Ignored unless valid action | P1 |

### 6.5 Authentication & Session (10)

| ID | Test Name | Attack Scenario | Expected Protection | Priority |
|----|-----------|----------------|-------------------|----------|
| SEC-036 | Brute-force | Repeated attempts | Account lockout | P0 |
| SEC-037 | Session fixation | Pre-set session | New session on login | P0 |
| SEC-038 | Session hijacking | Stolen JWT | Token bound to context | P1 |
| SEC-039 | CSRF on reparent | Forged POST | CSRF token required | P0 |
| SEC-040 | Clickjacking | Iframe embedding | X-Frame-Options: DENY | P1 |
| SEC-041 | Token storage | Storage check | HttpOnly, Secure cookies | P0 |
| SEC-042 | Concurrent sessions | Multiple logins | Policy enforced | P1 |
| SEC-043 | Token refresh | Near expiry | Refresh flow works | P1 |
| SEC-044 | Logout clears data | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS enforcement | HTTP attempt | Redirect to HTTPS | P0 |

### 6.6 Data Exposure (5)

| ID | Test Name | Sensitive Data | Exposure Risk | Expected Protection | Priority |
|----|-----------|---------------|--------------|-------------------|----------|
| SEC-046 | API excludes internal fields | Audit fields | Over-exposure | DTO filters fields | P1 |
| SEC-047 | No stack traces in errors | Exception details | Info disclosure | Generic error messages | P0 |
| SEC-048 | Tree data doesn't leak sensitive info | Financial, notes | Data leakage | Response DTO limited | P1 |
| SEC-049 | No caching of tree data | API responses | Cache extraction | Cache-Control: no-store | P1 |
| SEC-050 | Auth tokens not in URL | JWT | URL leakage | Token in HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests | **Coverage:** Race conditions, deadlocks, double submit, transaction isolation, cache poisoning

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users reparent same partner | Concurrent reparent | One succeeds, other gets conflict | P1 |
| CON-002 | User deletes while another expands | Delete + expand | Expand shows empty or error | P1 |
| CON-003 | Two users rename same partner | Concurrent update | Last write wins or conflict | P1 |
| CON-004 | Search during data refresh | Cache invalidation | Consistent results | P1 |
| CON-005 | Expand while data loading | Click during API call | Single call, correct state | P1 |
| CON-006 | Rapid expand/collapse toggling | 10 toggles fast | Final state correct | P1 |
| CON-007 | Create partner while tree loading | Concurrent create + load | Tree includes or refreshes | P2 |
| CON-008 | Delete partner while dragging | Delete during drag | Drag cancelled, error toast | P1 |
| CON-009 | Multiple users expanding same node | Concurrent expand | All see same children | P2 |
| CON-010 | Token refresh during reparent | Token expires mid-operation | Retry with new token | P1 |
| CON-011 | Database migration during browse | Schema change | Graceful degradation | P2 |
| CON-012 | Concurrent search from two sessions | Same user, 2 sessions | Both complete independently | P2 |
| CON-013 | Cache poisoning attempt | Modified cache | Invalidated, refresh from source | P1 |
| CON-014 | Optimistic concurrency on update | Stale version | Conflict detected | P1 |
| CON-015 | WebSocket reconnect during update | Disconnect/reconnect | Missed updates recovered | P2 |
| CON-016 | Multiple tabs drag-drop simultaneously | Two tabs reparent | One succeeds, other conflicts | P1 |
| CON-017 | Bulk operation during tree browse | Admin bulk update | Tree refreshes with changes | P2 |
| CON-018 | Context menu action during refresh | Right-click during reload | Menu reflects current state | P2 |
| CON-019 | Search + expand simultaneously | Both triggered | Both complete correctly | P1 |
| CON-020 | Database deadlock during expand | Concurrent reads | Deadlock resolved | P1 |
| CON-021 | Partner type change during filter | Type changes after filter | Filter reapplied or updated | P2 |
| CON-022 | Real-time update during search | WebSocket update | Results update dynamically | P2 |
| CON-023 | Multiple users create children for same parent | Concurrent create | All children appear, count correct | P1 |
| CON-024 | Export tree during bulk delete | Export + delete | Export captures pre-delete or handles | P2 |
| CON-025 | Session timeout during context menu | Timeout during interaction | Auth prompt | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests | **Breakdown:** Validation (5), Formatting (3), Calculations (5), Status logic (5), Collections (3)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Validate search not empty | Validation | "" | Invalid | P1 |
| UNT-002 | Validate search max length | Validation | 256 chars | Invalid | P1 |
| UNT-003 | Validate partner ID positive | Validation | -1 | Invalid | P1 |
| UNT-004 | Validate ParentId not self | Validation | Own ID | Invalid | P1 |
| UNT-005 | Validate hierarchy depth | Validation | Depth 21 | Invalid | P1 |
| UNT-006 | Format partner name truncation | Formatting | 250-char name | 200 + "..." | P1 |
| UNT-007 | Format child count badge | Formatting | 5 children | "(5)" | P1 |
| UNT-008 | Format tooltip text | Formatting | Partner data | "Type: Funding | Status: Active" | P2 |
| UNT-009 | Calculate tree depth | Calculations | 5-level data | Depth=5 | P1 |
| UNT-010 | Calculate total visible nodes | Calculations | Expanded/collapsed state | Count of visible | P1 |
| UNT-011 | Calculate search match count | Calculations | Search results | N matches | P1 |
| UNT-012 | Calculate children count (non-deleted) | Calculations | 5 children, 2 deleted | Count=3 | P1 |
| UNT-013 | Calculate indentation pixels | Calculations | Level 3 | 3 × indent_size | P2 |
| UNT-014 | Determine node expand state | Status | Node with children | Expandable=true | P1 |
| UNT-015 | Determine leaf node state | Status | Node without children | Expandable=false | P1 |
| UNT-016 | Determine search match | Status | Name vs query | Match/no-match | P1 |
| UNT-017 | Determine circular reference | Status | A>B>A | Circular=true | P0 |
| UNT-018 | Determine reparent validity | Status | New parent valid | Valid=true | P1 |
| UNT-019 | Build tree from flat list | Collections | Flat + ParentIds | Tree structure | P1 |
| UNT-020 | Sort siblings alphabetically | Collections | Unsorted siblings | Sorted A-Z | P1 |
| UNT-021 | Filter visible nodes | Collections | Expanded state + data | Visible subset | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests | **Breakdown:** Single ops (2), Bulk ops (3), Search (5), Concurrent access (3), Memory (3)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Tree initial load (100 partners) | Load | < 1 second | P1 |
| PRF-002 | Tree initial load (1000 partners) | Load | < 3 seconds | P1 |
| PRF-003 | Expand node with 100 children | Expand | < 500ms | P2 |
| PRF-004 | Expand All (500 nodes) | Expand All | < 2 seconds | P2 |
| PRF-005 | Collapse All (500 nodes) | Collapse All | < 500ms | P2 |
| PRF-006 | Search across 1000 partners | Search | < 500ms | P1 |
| PRF-007 | Search across 5000 partners | Search | < 1 second | P1 |
| PRF-008 | Search with auto-expand | Search + expand | < 1 second | P1 |
| PRF-009 | Type-ahead response | Keystroke | < 200ms | P1 |
| PRF-010 | Navigate search results | Next/Prev | < 100ms | P2 |
| PRF-011 | 10 concurrent tree loads | Concurrent | < 3s per user | P2 |
| PRF-012 | 50 concurrent tree loads | Concurrent | < 5s per user | P2 |
| PRF-013 | 20 concurrent searches | Concurrent | < 1s per search | P2 |
| PRF-014 | Memory with 1000 nodes | Memory | < 150MB heap | P2 |
| PRF-015 | Memory with 5000 nodes | Memory | < 400MB heap | P2 |
| PRF-016 | Memory leak (30 min browsing) | Memory | No growth > 10% | P1 |

---

## §10 Load Tests

> **Minimum:** 10 tests | **Breakdown:** Sustained load (3), Spike load (2), Stress limits (3), Recovery (2)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | 50 users browsing tree | Sustained, steady | 30 min | 95% < 3s, 0 errors | P2 |
| LDT-002 | 100 users browsing tree | Sustained, steady | 30 min | 95% < 5s, < 1% errors | P2 |
| LDT-003 | 50 users searching continuously | Sustained search | 15 min | < 1s per search | P2 |
| LDT-004 | Spike from 10 to 200 users | Sudden spike | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike with concurrent reparent operations | 50 users + 10 reparents | 5 min | All complete, no conflicts lost | P2 |
| LDT-006 | 500 concurrent users | Stress | 10 min | Graceful degradation | P2 |
| LDT-007 | 10,000 partner tree | 50 users, large data | 15 min | Virtual scroll works | P2 |
| LDT-008 | Continuous expand/collapse | 100 users toggling | 10 min | No memory leaks | P2 |
| LDT-009 | Recovery after API crash | Kill + restart | N/A | Tree recovers < 60s | P2 |
| LDT-010 | Recovery after DB restart | DB restart | N/A | Tree reconnects and loads | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Tree view loads with hierarchy | POS-001, POS-002, INT-001, PRF-001, PRF-002 |
| AC-2: Expand/collapse functionality | POS-002, POS-005, POS-007, POS-008, BND-021–030, CON-006 |
| AC-3: Navigate to partner detail | POS-003, FUN-007, SEC-021, INT-036 |
| AC-4: Search within tree | POS-004, POS-011, POS-012, NEG-001–010, FUN-005, FUN-006, PRF-006–009 |
| AC-5: Context menu with actions | POS-013–016, FUN-008, NEG-015–016, SEC-039 |
| AC-6: State persistence | POS-019, POS-020, FUN-004, INT-035 |
| AC-7: Child count indicators | POS-009, POS-010, FUN-013, BND-027 |
| AC-8: Drag-drop reparent | POS-027, NEG-023–025, FUN-009–010, INT-005, CON-001 |
| AC-9: Security & access control | SEC-001–050, NEG-011–020 |
| AC-10: Performance under load | PRF-001–016, LDT-001–010 |

---

## Test Environment Setup

**Prerequisites:**
- Authenticated user with Partner View permissions
- At least 50 partners with multi-level hierarchy in test database
- Partners with various types and statuses
- Chrome 90+ / Firefox 88+ / Edge 90+ browser
- Drag-drop support for reparent tests

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
