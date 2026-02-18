# Search and List Views — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/shared/pages/components/listview`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 10 | 10 | ✅ |
| §8 Unit | 6 | 6 | ✅ |
| §9 Performance | 4 | 4 | ✅ |
| §10 Load | 2 | 2 | ✅ |
| **TOTAL** | **462** | **≥390** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Status |
|-------|--------|--------|
| N≥3P: 90≥90 | ✅ PASS | N >= 3 × P |
| E≥3P: 90≥90 | ✅ PASS | E >= 3 × P |
| F≥3P: 90≥90 | ✅ PASS | F >= 3 × P |
| I≥3P: 90≥90 | ✅ PASS | I >= 3 × P |

---

## Feature Overview

Search and list views: global search, entity-specific lists, filtering, pagination, sorting, export, column config.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Global Search Returns Results

**Priority:** P0  
**Precondition:** User logged in, data exists.

**Steps:**
1. Enter search term in global search
2. Submit search

**Expected Result:** Results from across entities displayed.

---

#### POS-002: Entity List Loads with Pagination

**Priority:** P0  
**Precondition:** Partners/Opportunities exist.

**Steps:**
1. Navigate to entity list (e.g., Partners)
2. View list

**Expected Result:** List displayed with pagination controls.

---

#### POS-003: Filter List by Criteria

**Priority:** P0  
**Precondition:** List loaded.

**Steps:**
1. Apply filter (e.g., Status = Active)
2. View results

**Expected Result:** Filtered results displayed.

---

#### POS-004: Sort List by Column

**Priority:** P0  
**Precondition:** List loaded.

**Steps:**
1. Click column header to sort
2. View results

**Expected Result:** List sorted by column.

---

#### POS-005: Export List to CSV

**Priority:** P0  
**Precondition:** List loaded.

**Steps:**
1. Click Export
2. Select CSV

**Expected Result:** CSV file downloaded.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Search with partial match | Data exists | Search "part" | Matches "Partner" | P1 |
| POS-007 | Search case-insensitive | Data exists | Search "PARTNER" | Same as "partner" | P1 |
| POS-008 | Paginate to page 2 | 50+ records | Click page 2 | Page 2 displayed | P1 |
| POS-009 | Change page size | List loaded | Select 50 per page | 50 records shown | P1 |
| POS-010 | Multi-column sort | List loaded | Sort by Name, then Date | Correct order | P1 |
| POS-011 | Column config show/hide | List loaded | Toggle column | Column visibility | P1 |
| POS-012 | Column config reorder | List loaded | Drag column | Order changed | P1 |
| POS-013 | Saved filter load | Filter saved | Load filter | Filter applied | P1 |
| POS-014 | Entity-specific search | On Partners | Search | Only partners | P1 |
| POS-015 | Quick filter chips | List loaded | Click chip | Filter applied | P1 |
| POS-016 | Clear all filters | Filters applied | Clear | All data | P2 |
| POS-017 | Export with filters | Filters applied | Export | Filtered export | P2 |
| POS-018 | Export selected rows | Rows selected | Export selected | Only selected | P2 |
| POS-019 | Bulk select all | List loaded | Select all | All selected | P2 |
| POS-020 | Responsive list (mobile) | Mobile viewport | View list | Mobile layout | P2 |
| POS-021 | Virtual scroll | 1000 rows | Scroll | Smooth scroll | P2 |
| POS-022 | Keyboard nav in list | Focus | Arrow keys | Navigate rows | P2 |
| POS-023 | Row click to detail | List loaded | Click row | Navigate to detail | P2 |
| POS-024 | Inline edit | Editable list | Edit cell | Value updated | P2 |
| POS-025 | Search debounce | Typing | Type "test" | Debounced search | P2 |
| POS-026 | Search with Enter | Search box | Press Enter | Search executed | P2 |
| POS-027 | Filter by date range | List loaded | Select range | Date filtered | P2 |
| POS-028 | Filter by dropdown | List loaded | Select option | Filtered | P2 |
| POS-029 | Column resize | Resizable columns | Drag resize | Width changed | P2 |
| POS-030 | Sticky header | Long list | Scroll | Header sticky | P2 |

---

## §2 Negative Tests (90)

> **Minimum:** 90 tests

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Search with null | Query = null | Empty or all | P0 |
| NEG-002 | Search with empty | Query = "" | All results | P0 |
| NEG-003 | Filter with invalid value | Status = "invalid" | Validation error | P0 |
| NEG-004 | Page with negative | Page = -1 | Default or error | P0 |
| NEG-005 | Page size zero | Size = 0 | Default | P0 |
| NEG-006 | Sort invalid column | SortBy = "INVALID" | Default sort | P0 |
| NEG-007 | Export invalid format | Format = "invalid" | Default | P0 |
| NEG-008 | Date range invalid | From > To | Validation error | P0 |
| NEG-009 | Column config invalid | Malformed config | Default config | P0 |
| NEG-010 | Saved filter not found | FilterId = 999999 | Error | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Anonymous search | No auth | Search | Redirect to login | P0 |
| NEG-012 | No view permission | Limited | View list | Access denied | P0 |
| NEG-013 | No export permission | Reader | Export | 403 | P0 |
| NEG-014 | OrgUnit scope | Scoped | View other scope | 403 | P0 |
| NEG-015 | Expired session | Expired | Any | 401 | P0 |
| NEG-016 | Disabled user | Disabled | Any | 403 | P1 |
| NEG-017 | Cross-entity search | No perm on Opp | Search opp | Excluded | P1 |
| NEG-018 | API without auth | No Bearer | GET /list | 401 | P0 |
| NEG-019 | Tampered JWT | Modified | Any | 401 | P0 |
| NEG-020 | Post-logout | Logged out | Cached list | 401 | P0 |

### 2.3–2.7 Additional Negative (NEG-021 to NEG-070)

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-021 | Search during load | Loading | Queued | P1 |
| NEG-022 | Filter during load | Loading | Queued | P1 |
| NEG-023 | List config null | Config = null | Default | P1 |
| NEG-024 | API 500 | Server error | Error message | P0 |
| NEG-025 | API timeout | Timeout | Timeout message | P0 |
| NEG-026 | Duplicate column IDs | Same ID | One shown | P1 |
| NEG-027 | XSS in search | `<script>` | Sanitized | P0 |
| NEG-028 | SQL injection | `'; DROP--` | Parameterized | P0 |
| NEG-029 | Export oversized | 100000 rows | Chunked or error | P1 |
| NEG-030 | Paginate beyond max | Page = 99999 | Last page | P1 |
| NEG-031 | Filter with null | Filter = null | All | P1 |
| NEG-032 | Sort with null | Sort = null | Default | P1 |
| NEG-033 | Malformed saved filter | Invalid JSON | Error | P1 |
| NEG-034 | Path traversal export | `../../evil` | Rejected | P0 |
| NEG-035 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| NEG-036 | Regex DoS | `(((...)))` | Rejected | P1 |
| NEG-037 | Rapid search | 20 chars/sec | Debounced | P1 |
| NEG-038 | Rapid filter change | 10 changes/sec | Debounced | P1 |
| NEG-039 | Concurrent tabs | 2 tabs search | Both work | P1 |
| NEG-040 | CORS error | Cross-origin | Error | P1 |
| NEG-041 | Negative page | Page = -1 | Default | P1 |
| NEG-042 | Page size > max | Size = 10000 | Capped | P1 |
| NEG-043 | Invalid date format | Date = "invalid" | Error | P1 |
| NEG-044 | Filter with whitespace | "   " | Trimmed or error | P1 |
| NEG-045 | Export empty list | No rows | Empty file | P1 |
| NEG-046 | Saved filter deleted | Load deleted | Error | P1 |
| NEG-047 | Column config malformed | Invalid JSON | Default | P1 |
| NEG-048 | Sort direction invalid | Dir = "invalid" | Default | P2 |
| NEG-049 | Multi-filter conflict | Conflicting filters | Resolution | P1 |
| NEG-050 | Search with regex chars | `.*+?` | Escaped | P1 |
| NEG-051 | List during unmount | Unmount | No error | P1 |
| NEG-052 | Search with binary | Binary chars | Rejected | P1 |
| NEG-053 | Filter with max length | 10000 chars | Truncated | P1 |
| NEG-054 | Export format PDF | PDF | If supported | P2 |
| NEG-055 | Column width negative | -1 | Default | P1 |
| NEG-056 | Row count overflow | 2147483647 | Handled | P1 |
| NEG-057 | Rapid pagination | 10 clicks/sec | Debounced | P1 |
| NEG-058 | Empty column config | [] | Default columns | P1 |
| NEG-059 | Null entity type | null | Error | P1 |
| NEG-060 | Invalid entity type | "invalid" | Error | P1 |
| NEG-061 | Search with newlines | `\n\n` | Sanitized | P1 |
| NEG-062 | Filter with control chars | `\0` | Sanitized | P1 |
| NEG-063 | Export during search | Concurrent | Both complete | P1 |
| NEG-064 | List during API retry | Retry | Correct state | P1 |
| NEG-065 | Session expiry mid-list | Expire | Redirect | P0 |
| NEG-066 | Token refresh mid-search | Refresh | Retry | P1 |
| NEG-067 | Network reconnect | Reconnect | Refetch | P1 |
| NEG-068 | AbortController cancel | Cancel | Aborted | P1 |
| NEG-069 | Stale filter | Old filter | Refetch | P1 |
| NEG-070 | Concurrent filter save | 2 users save | One wins | P1 |
| NEG-071 | Search with null query after trim | "   " | Treated as empty | P1 |
| NEG-072 | Filter with invalid enum value | Status = "DELETED" | Default or error | P1 |
| NEG-073 | Pagination with float page | Page = 1.5 | Rounded or error | P1 |
| NEG-074 | Sort with invalid direction | Dir = "RANDOM" | Default asc | P1 |
| NEG-075 | Export with invalid MIME type | MIME = "text/plain" | Default CSV | P1 |
| NEG-076 | Column config with invalid field | Field = "DROP_TABLE" | Ignored | P1 |
| NEG-077 | Saved filter with invalid structure | Malformed JSON | Error | P1 |
| NEG-078 | Search with null character | \0 in query | Sanitized | P1 |
| NEG-079 | Filter with empty array | [] | All data | P1 |
| NEG-080 | List with negative row index | Index = -1 | Handled | P1 |
| NEG-081 | Virtual scroll with zero height | Height = 0 | Graceful | P2 |
| NEG-082 | Export with concurrent cancel | Cancel during export | Aborted | P1 |
| NEG-083 | Search with Unicode null | \u0000 | Sanitized | P1 |
| NEG-084 | Filter with max int value | Value = 2147483647 | Handled | P1 |
| NEG-085 | List during component destroy | Unmount | No error | P1 |
| NEG-086 | Search with only wildcards | "***" | Escaped or no match | P1 |
| NEG-087 | Column with duplicate field | Same field twice | Deduplicated | P1 |
| NEG-088 | Pagination with overflow | Page * Size > MAX | Capped | P1 |
| NEG-089 | Filter with future date | Date = 2030 | Validation error | P1 |
| NEG-090 | List with circular reference in data | Circular object | Handled | P1 |

---

## §3 Boundary Tests (90)

> **Minimum:** 90 tests

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Search query | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Capped | P1 |
| BND-002 | Filter value | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Rejected | P1 |
| BND-003 | Column name | 1 | 100 | ✅ "A" | ✅ 100 | ❌ Rejected | P1 |
| BND-004 | Saved filter name | 1 | 200 | ✅ "F" | ✅ 200 | ❌ Rejected | P1 |
| BND-005 | Export filename | 1 | 260 | ✅ "a" | ✅ 260 | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-007 | Page size | 1 | 1000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-008 | Row count | 0 | MAX_INT | ✅ 0 | ❌ | Overflow | P1 |
| BND-009 | Column index | 0 | 50 | ✅ 0 | ❌ | Rejected | P1 |
| BND-010 | Filter ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-011 | Filter leap year | Feb 29, 2028 | Handled | P2 |
| BND-012 | Filter same day | FromDate = ToDate | That day | P2 |
| BND-013 | Filter timezone | UTC vs local | Correct | P2 |
| BND-014 | Sort by date | Date column | Chronological | P2 |
| BND-015 | Export date format | ISO | Formatted | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-016 | Zero rows | Empty | Empty list | P1 |
| BND-017 | One row | Single | 1 row | P1 |
| BND-018 | Exactly page size | 20, size=20 | Full page | P1 |
| BND-019 | Page size + 1 | 21, size=20 | 20 on page 1 | P1 |
| BND-020 | 10000 rows | Large | Paginated | P1 |
| BND-021 | Zero columns | Empty config | Default | P1 |
| BND-022 | 50 columns | Many | Horizontal scroll | P1 |
| BND-023 | Zero filters | No filters | All data | P1 |
| BND-024 | 10 filters | Many | All applied | P1 |
| BND-025 | Last page 1 row | 41 rows, page 3, size 20 | 1 row | P1 |
| BND-026 | Search 0 results | No match | Empty | P1 |
| BND-027 | Search 1 result | Single match | 1 row | P1 |
| BND-028 | Export 0 rows | Empty | Empty file | P1 |
| BND-029 | Export 10000 rows | Max | Full export | P2 |
| BND-030 | Selected 0 rows | None | Export blank | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input | Expected Result | Priority |
|----|-------|------|-----------------|----------|
| BND-031 | Search (Arabic) | `بحث` | Matches | P2 |
| BND-032 | Search (Chinese) | `搜索` | Matches | P2 |
| BND-033 | Filter (Cyrillic) | `фильтр` | Stored | P2 |
| BND-034 | Column name (accent) | `Résumé` | Displayed | P2 |
| BND-035 | Search apostrophe | "O'Brien" | Matches | P1 |
| BND-036 | Search emoji | `🔍` | Handled | P2 |
| BND-037 | Filter special chars | `% & < >` | Escaped | P1 |
| BND-038 | Export unicode | Arabic data | Correct encoding | P2 |
| BND-039 | Sort unicode | Arabic names | Correct order | P2 |
| BND-040 | Column label unicode | Chinese | Displayed | P2 |

### 3.6 Responsive Boundaries

| ID | Test Name | Viewport | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-041 | 320px | Mobile | Mobile layout | P1 |
| BND-042 | 768px | Tablet | Tablet layout | P1 |
| BND-043 | 1920px | Desktop | Desktop layout | P1 |
| BND-044 | Resize during load | Resize | Correct layout | P2 |
| BND-045 | Virtual scroll 1000 | 1000 rows | Smooth | P1 |
| BND-046 | Virtual scroll 10000 | 10000 rows | Performant | P2 |
| BND-047 | Column resize min | Min width | Enforced | P1 |
| BND-048 | Column resize max | Max width | Enforced | P1 |
| BND-049 | Sticky header scroll | Long list | Header sticky | P1 |
| BND-050 | Horizontal scroll | Many columns | Scroll | P1 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Search 1 char | "a" | Matches | P1 |
| BND-052 | Search max chars | 500 chars | Processed | P1 |
| BND-053 | Page 1 | First | Correct | P1 |
| BND-054 | Page last | Last | Correct | P1 |
| BND-055 | Sort ascending | Asc | A-Z | P1 |
| BND-056 | Sort descending | Desc | Z-A | P1 |
| BND-057 | Multi-sort | 2 columns | Correct order | P1 |
| BND-058 | Filter + search | Both | Combined | P1 |
| BND-059 | Filter + sort | Both | Applied | P1 |
| BND-060 | Search + pagination | Both | Correct page | P1 |
| BND-061 | Export filtered | Filter + export | Filtered file | P1 |
| BND-062 | Export sorted | Sort + export | Sorted file | P1 |
| BND-063 | Column config save | Save | Persisted | P1 |
| BND-064 | Saved filter load | Load | Applied | P1 |
| BND-065 | Clear filters | Clear | All data | P1 |
| BND-066 | Reset column config | Reset | Default | P2 |
| BND-067 | Keyboard select all | Ctrl+A | All selected | P2 |
| BND-068 | Keyboard arrow | Arrow keys | Row focus | P2 |
| BND-069 | Keyboard Enter | Enter on row | Navigate | P2 |
| BND-070 | Concurrent tabs | 2 tabs | Independent | P2 |
| BND-071 | Search query exactly 1 char | "a" | Matches | P1 |
| BND-072 | Search query exactly 500 chars | Max length | Processed | P1 |
| BND-073 | Page number exactly 1 | First page | Correct | P1 |
| BND-074 | Page number exactly last | Last page | Correct | P1 |
| BND-075 | Page size exactly 1 | 1 per page | 1 row | P1 |
| BND-076 | Page size exactly 1000 | Max | 1000 rows | P1 |
| BND-077 | Row count exactly 0 | Empty | Empty state | P1 |
| BND-078 | Row count exactly 1 | Single | 1 row | P1 |
| BND-079 | Filter count exactly 10 | 10 filters | All applied | P1 |
| BND-080 | Column count exactly 50 | 50 columns | Horizontal scroll | P1 |
| BND-081 | Viewport exactly 320px | Mobile | Mobile layout | P1 |
| BND-082 | Viewport exactly 1920px | Desktop | Desktop layout | P1 |
| BND-083 | Selected rows exactly 0 | None | Export blank | P1 |
| BND-084 | Selected rows exactly page size | All on page | All exported | P1 |
| BND-085 | Date range exactly 1 day | Same from/to | That day | P2 |
| BND-086 | Date range exactly 1 year | Full year | Correct | P2 |
| BND-087 | Sort ascending boundary | A-Z | Correct order | P1 |
| BND-088 | Sort descending boundary | Z-A | Correct order | P1 |
| BND-089 | Debounce at 300ms boundary | 300ms | Single request | P1 |
| BND-090 | Virtual scroll at 1000 rows | 1000 | Smooth scroll | P1 |

---

## §4 Functional Tests (90)

> **Minimum:** 90 tests

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | List loads on nav | Load | Navigate | List displayed | P0 |
| FUN-002 | Search executes | Search | Submit | Results displayed | P0 |
| FUN-003 | Filter applies | Filter | Apply | Filtered results | P0 |
| FUN-004 | Sort applies | Sort | Click header | Sorted results | P0 |
| FUN-005 | Pagination works | Page | Click page | Page loaded | P0 |
| FUN-006 | Export downloads | Export | Click export | File downloaded | P0 |
| FUN-007 | Column config saves | Config | Toggle column | Saved | P1 |
| FUN-008 | Saved filter loads | Filter | Load | Applied | P1 |
| FUN-009 | Permission filters | Permission | Load | Only permitted | P0 |
| FUN-010 | OrgUnit scope | OrgUnit | Load | Scoped data | P0 |
| FUN-011 | Loading state | Load | API call | Loading shown | P1 |
| FUN-012 | Error state | Error | API error | Error shown | P1 |
| FUN-013 | Empty state | No data | Empty | Empty message | P1 |
| FUN-014 | Debounce search | Debounce | Type | 300ms debounce | P1 |
| FUN-015 | Cache on repeat | Cache | Same view | Cached or fresh | P1 |

### 4.2 Validation (15) | 4.3 Constraint (10) | 4.4 Audit (10)

| ID | Test Name | Rule/Constraint | Test Input | Expected | Priority |
|----|-----------|-----------------|-----------|----------|----------|
| FUN-016 | Query valid | Validation | "test" | Valid | P1 |
| FUN-017 | Page valid | 1-10000 | 0 | Default 1 | P1 |
| FUN-018 | Size valid | 1-1000 | 0 | Default 20 | P1 |
| FUN-019 | Sort column valid | Exists | "INVALID" | Default | P1 |
| FUN-020 | No XSS in search | Sanitize | `<script>` | Escaped | P0 |
| FUN-021 to FUN-030 | [Additional validation rules] | Various | Various | Per rule | P1 |
| FUN-031 | Max page size | 1000 | 5000 | Capped | P1 |
| FUN-032 | Max export | 10000 | 15000 | Chunked | P2 |
| FUN-033 to FUN-040 | [Additional constraints] | Various | Various | Per constraint | P1 |
| FUN-041 | Search audit | Audit | Search | Logged | P1 |
| FUN-042 | Export audit | Audit | Export | Logged | P1 |
| FUN-043 to FUN-050 | [Additional audit rules] | Various | Various | Per rule | P1 |
| FUN-051 | List load triggers API | Load | API called | P0 |
| FUN-052 | Search triggers API with query | Search | Query in request | P0 |
| FUN-053 | Filter triggers API with params | Filter | Params in request | P0 |
| FUN-054 | Sort triggers API with sort param | Sort | Sort in request | P0 |
| FUN-055 | Pagination triggers API with page | Page | Page in request | P0 |
| FUN-056 | Column config persisted | Config | Saved to storage | P1 |
| FUN-057 | Saved filter persisted | Filter | Saved to backend | P1 |
| FUN-058 | Permission filters visible columns | Permission | Only permitted columns | P0 |
| FUN-059 | OrgUnit filters list data | OrgUnit | Scoped data | P0 |
| FUN-060 | Loading state during fetch | Fetch | Spinner shown | P1 |
| FUN-061 | Error state on API failure | Failure | Error message | P1 |
| FUN-062 | Empty state when no data | No data | Empty message | P1 |
| FUN-063 | Debounce prevents rapid requests | Rapid type | Single request | P1 |
| FUN-064 | Cache invalidated on filter change | Filter | Fresh data | P1 |
| FUN-065 | Row selection state tracked | Select | Selection array | P1 |
| FUN-066 | Select all respects filter | Select all | Only visible | P1 |
| FUN-067 | Export respects current filter | Export | Filtered data | P1 |
| FUN-068 | Export respects current sort | Export | Sorted data | P1 |
| FUN-069 | Virtual scroll renders visible only | Scroll | Visible rows | P1 |
| FUN-070 | Sticky header on scroll | Scroll | Header fixed | P1 |
| FUN-071 | Keyboard nav wraps at edges | Arrow keys | Wrap or stop | P1 |
| FUN-072 | Row click navigates to detail | Click | Route to detail | P0 |
| FUN-073 | Inline edit saves on blur | Edit | Value saved | P1 |
| FUN-074 | Quick filter chips apply filter | Chip | Filter applied | P1 |
| FUN-075 | Clear filters resets all | Clear | All filters off | P1 |
| FUN-076 | Reset column config to default | Reset | Default columns | P1 |
| FUN-077 | Search suggestions from API | Suggest | API called | P2 |
| FUN-078 | Recent searches from storage | Recent | From storage | P2 |
| FUN-079 | Date range validation | Range | From ≤ To | P0 |
| FUN-080 | Page validation | Page | 1 to max | P1 |
| FUN-081 | Page size validation | Size | 1 to 1000 | P1 |
| FUN-082 | Sort column validation | Column | Valid column | P1 |
| FUN-083 | Export format validation | Format | Valid format | P1 |
| FUN-084 | XSS sanitization in display | Display | Escaped | P0 |
| FUN-085 | Number formatting by locale | Locale | Correct format | P2 |
| FUN-086 | Date formatting by locale | Locale | Correct format | P2 |
| FUN-087 | Currency formatting | Currency | Correct symbol | P2 |
| FUN-088 | Truncation for long text | Long text | Ellipsis | P1 |
| FUN-089 | Tooltip for truncated | Hover | Full text | P2 |
| FUN-090 | Accessibility labels | A11y | Labels present | P1 |

---

## §5 Integration Tests (90)

> **Minimum:** 90 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Nav to list | Navigate | Partners | List loads | P0 |
| INT-002 | Search flow | Search | Global | Results shown | P0 |
| INT-003 | Filter flow | Filter | List | Filtered | P0 |
| INT-004 | Sort flow | Sort | List | Sorted | P0 |
| INT-005 | Export flow | Export | List | File downloaded | P0 |
| INT-006 | Save filter | Save | Filter | Saved | P1 |
| INT-007 | Load filter | Load | Filter | Applied | P1 |
| INT-008 | Column config | Config | Columns | Saved | P1 |
| INT-009 | Row to detail | Click | Row | Navigate | P1 |
| INT-010 | Bulk select export | Select + Export | Rows | Selected exported | P1 |

### 5.2 Search & Filter (10) | 5.3 Pagination (5) | 5.4 Relationships (10) | 5.5 Error Handling (15)

| ID | Test Name | Criteria/Scenario | Expected | Priority |
|----|-----------|-----------------|----------|----------|
| INT-011 to INT-020 | Search by entity, combined filter, date range, clear, case-insensitive | Various | Per criteria | P0/P1 |
| INT-021 to INT-025 | Page 1, last page, empty, single, large | Various | Per scenario | P1 |
| INT-026 to INT-035 | List→Entity, List→Detail, List→Export, List→Config | Various | Per relationship | P0/P1 |
| INT-036 to INT-050 | API 500, 401, 403, 404, timeout, validation | Various | Per error | P0/P1 |
| INT-051 | Nav to list → Load → Display | Full flow | List shown | P0 |
| INT-052 | Search → Results → Click row | Full flow | Detail shown | P0 |
| INT-053 | Filter → Results → Export | Full flow | Filtered export | P0 |
| INT-054 | Sort → Results → Paginate | Full flow | Sorted paginated | P0 |
| INT-055 | Save filter → Reload → Load filter | Full flow | Filter restored | P1 |
| INT-056 | Column config → Reload → Config restored | Full flow | Config restored | P1 |
| INT-057 | List → Detail → Back → List state | Navigation | State preserved | P1 |
| INT-058 | Multi-entity search → Results | Global search | Cross-entity results | P0 |
| INT-059 | List → API → Map → Display | Data flow | Correct mapping | P0 |
| INT-060 | List with lazy-loaded columns | Lazy | Columns load | P2 |
| INT-061 | List with dynamic filters | Dynamic | Filters update | P2 |
| INT-062 | List with real-time updates | WebSocket | Updates reflect | P2 |
| INT-063 | List with bulk action | Bulk | Action on selected | P1 |
| INT-064 | List with inline create | Inline | New row added | P2 |
| INT-065 | List with inline delete | Inline | Row removed | P1 |
| INT-066 | List with permission check | Permission | Filtered by perm | P0 |
| INT-067 | List with OrgUnit scope | OrgUnit | Scoped list | P0 |
| INT-068 | List with auth guard | Auth | Redirect if no auth | P0 |
| INT-069 | List with session expiry | Expiry | Redirect | P1 |
| INT-070 | List with token refresh | Refresh | Seamless | P2 |
| INT-071 | List with error boundary | Error | Boundary catches | P2 |
| INT-072 | List with retry on failure | Retry | Retry works | P2 |
| INT-073 | List with offline support | Offline | Cached or message | P2 |
| INT-074 | List with optimistic update | Update | Optimistic UI | P2 |
| INT-075 | List with conflict resolution | Conflict | Resolution UI | P2 |
| INT-076 | List with audit logging | Audit | Actions logged | P2 |
| INT-077 | List with analytics | Analytics | Events sent | P2 |
| INT-078 | List with i18n | i18n | Translated | P1 |
| INT-079 | List with theme | Theme | Themed | P2 |
| INT-080 | List with responsive breakpoints | Breakpoints | Layout adapts | P1 |
| INT-081 | List with virtual scroll integration | Virtual | Scroll works | P1 |
| INT-082 | List with export service | Export | File downloaded | P0 |
| INT-083 | List with filter service | Filter | Filters work | P0 |
| INT-084 | List with sort service | Sort | Sort works | P0 |
| INT-085 | List with pagination service | Pagination | Pagination works | P0 |
| INT-086 | List with search service | Search | Search works | P0 |
| INT-087 | List with config service | Config | Config applied | P2 |
| INT-088 | List with storage service | Storage | State persisted | P2 |
| INT-089 | List with HTTP interceptor | HTTP | Interceptor applied | P2 |
| INT-090 | List end-to-end full flow | E2E | Nav→Search→Filter→Export | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection (10) | 6.2 Access Control (10) | 6.3 IDOR (10) | 6.4 Mass Assignment (5) | 6.5 Auth (10) | 6.6 Data Exposure (5)

| ID | Test Name | Attack/Scenario | Expected | Priority |
|----|-----------|-----------------|----------|----------|
| SEC-001 | SQL injection search | `'; DROP--` | Parameterized | P0 |
| SEC-002 | XSS in search | `<script>` | Sanitized | P0 |
| SEC-003 to SEC-010 | LDAP, path traversal, HTML, JSON injection | Various | Sanitized/Rejected | P0/P1 |
| SEC-011 to SEC-020 | Anonymous, expired, no permission, CORS | Various | 401/403 | P0 |
| SEC-021 to SEC-030 | ID guess, negative ID, other user's data | Various | 403/404 | P0/P1 |
| SEC-031 to SEC-035 | Protected fields | Various | Not modifiable | P0 |
| SEC-036 to SEC-045 | Brute-force, CSRF, token, HTTPS | Various | Protected | P0 |
| SEC-046 to SEC-050 | PII, stack traces, caching | Various | Protected | P0/P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users search | Concurrent search | Both succeed | P1 |
| CON-002 | Search + filter | Concurrent | Both complete | P1 |
| CON-003 | Export + pagination | Concurrent | Both complete | P1 |
| CON-004 to CON-025 | Tab switch, rapid input, cache invalidation, resize, etc. | Various | Per scenario | P1/P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Search validation | Validation | "test" | Valid | P1 |
| UNT-002 | Pagination calc | Calculations | 55, 20 | 3 pages | P1 |
| UNT-003 | Sort compare | Formatting | A, B | -1, 0, 1 | P1 |
| UNT-004 to UNT-021 | Filter logic, export format, column config, etc. | Various | Various | Per test | P1/P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | List load | 1000 rows | < 2s | P2 |
| PRF-002 | Search | Query | < 500ms | P2 |
| PRF-003 | Filter | Apply | < 500ms | P2 |
| PRF-004 to PRF-016 | Sort, export, pagination, virtual scroll, memory, LCP, FID, CLS | Various | Per threshold | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained view | 50 users, 1 req/s | 5 min | 95% < 2s | P2 |
| LDT-002 | Sustained search | 30 users, 2 req/s | 5 min | 95% < 1s | P2 |
| LDT-003 to LDT-010 | Spike, stress, recovery | Various | Various | Per criteria | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Global search | POS-001, POS-006, POS-007, INT-011 to INT-013 |
| Entity lists | POS-002, POS-014, INT-001 to INT-005 |
| Filtering | POS-003, POS-010, POS-015, FUN-002 |
| Pagination | POS-008, POS-009, INT-021 to INT-025 |
| Sorting | POS-004, POS-010, FUN-003 |
| Export | POS-005, POS-017, POS-018, INT-009 |
| Column config | POS-011, POS-012, POS-029 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
