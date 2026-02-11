# OrganizationHierarchyManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager`  
**Created:** 2026-02-04  
**Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 35 | 30-50 | ✅ |
| Negative Tests | §2 | 70 | Max(50, 2×35)=70 | ✅ |
| Boundary Tests | §3 | 70 | Max(50, 2×35)=70 | ✅ |
| Functional Tests | §4 | 50 | ≥50 | ✅ |
| Integration Tests | §5 | 50 | ≥50 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **397** | **≥347** | ✅ |

**3:1 Ratio Check:** (N + B) = 140 ≥ 3 × P = 105 → ✅ PASS

---

## Feature Overview

Manages the organizational hierarchy (Region → Hub → OrgUnit). Key features: CRUD for org units, parent-child relationships, type-based filtering (Region/Hub/OrgUnit), tree traversal (children, descendants, ancestors), moving units between parents, code uniqueness, entity relationships (Partners, Users), search, deletion rules, user permission filtering, audit fields, and soft delete.

---

## §1 Positive Tests — 35 tests

### P0 Detailed (5)

#### POS-001: Create OrgUnit with Valid Data
**Priority:** P0 | **Precondition:** Parent OrgUnit (Hub) exists.
**Steps:** CreateOrgUnitAsync(Name, Code, Type=OrgUnit, ParentId)
**Expected:** Created with Id, audit fields, IsDeleted=false

#### POS-002: Get OrgUnit by ID with Related Data
**Priority:** P0 | **Precondition:** OrgUnit exists with children and partners.
**Steps:** GetByIdAsync(id) with includes
**Expected:** OrgUnit returned, children and partners loaded (!IsDeleted)

#### POS-003: Get Full Hierarchy Tree
**Priority:** P0 | **Precondition:** Multi-level hierarchy (Region > Hub > OrgUnit).
**Steps:** GetHierarchyTreeAsync()
**Expected:** Complete tree structure, Region at root, Hubs as children, OrgUnits under Hubs

#### POS-004: Move OrgUnit to New Parent
**Priority:** P0 | **Precondition:** OrgUnit exists under Hub A, Hub B exists.
**Steps:** MoveOrgUnitAsync(orgUnitId, newParentId=hubB)
**Expected:** ParentId updated, hierarchy restructured, audit fields set

#### POS-005: Soft Delete OrgUnit
**Priority:** P0 | **Precondition:** OrgUnit exists with no children or partners.
**Steps:** DeleteOrgUnitAsync(id)
**Expected:** IsDeleted=true, DeletedBy/Date set

### P1/P2 Tabular (30)

| ID | Test Name | Steps | Expected | Pr |
|----|-----------|-------|----------|----|
| POS-006 | Create Region | Create Type=Region | Root-level created | P1 |
| POS-007 | Create Hub under Region | Create Type=Hub, ParentId=region | Hub under region | P1 |
| POS-008 | Create OrgUnit under Hub | Create Type=OrgUnit, ParentId=hub | OrgUnit under hub | P1 |
| POS-009 | Update OrgUnit name | UpdateAsync(new name) | Name changed | P1 |
| POS-010 | Update OrgUnit code | UpdateAsync(new code) | Code changed | P1 |
| POS-011 | Get direct children | GetChildrenAsync(parentId) | Direct children returned | P1 |
| POS-012 | Get all descendants | GetDescendantsAsync(parentId) | All descendants recursively | P1 |
| POS-013 | Get ancestors | GetAncestorsAsync(orgUnitId) | Parent chain to root | P1 |
| POS-014 | Filter by type (Region) | GetByType(Region) | Only regions | P1 |
| POS-015 | Filter by type (Hub) | GetByType(Hub) | Only hubs | P1 |
| POS-016 | Filter by type (OrgUnit) | GetByType(OrgUnit) | Only org units | P1 |
| POS-017 | Search by name | SearchAsync("Europe") | Matching units | P1 |
| POS-018 | Search by code | SearchAsync("EU-001") | Matching by code | P1 |
| POS-019 | Paginate org units | GetWithPagination(page=1) | Paginated results | P1 |
| POS-020 | Get org units for user | GetForUserAsync(userId) | User's permitted units | P1 |
| POS-021 | Get partners for OrgUnit | GetPartnersAsync(orgUnitId) | OrgUnit's partners | P1 |
| POS-022 | Get users for OrgUnit | GetUsersAsync(orgUnitId) | OrgUnit's users | P1 |
| POS-023 | Sort by name | Sort(name, asc) | Alphabetical | P2 |
| POS-024 | Sort by code | Sort(code, asc) | By code | P2 |
| POS-025 | Sort by type | Sort(type) | Grouped by type | P2 |
| POS-026 | Code uniqueness check | Create with unique code | Accepted | P1 |
| POS-027 | Update description | UpdateAsync(description) | Updated | P2 |
| POS-028 | Update sort order | UpdateAsync(sortOrder) | Order changed | P2 |
| POS-029 | Map entity to model | mapper.Map | All fields | P2 |
| POS-030 | Get typeahead | GetTypeaheadAsync | Id+Name list | P2 |
| POS-031 | Get org unit count | GetCountAsync | Non-deleted count | P2 |
| POS-032 | Restore deleted | RestoreAsync | IsDeleted=false | P2 |
| POS-033 | Get flat list of all | GetAllAsync | All non-deleted | P2 |
| POS-034 | Audit trail retrieval | GetAuditAsync | History entries | P2 |
| POS-035 | Get OrgUnit with partners count | GetByIdWithCount | Partner count loaded | P2 |

---

## §2 Negative Tests — 70 tests

| ID | Category | Scenario | Expected | Pr |
|----|----------|---------|----------|----|
| NEG-001 | Input | Null Name | BusinessException | P0 |
| NEG-002 | Input | Empty Name | BusinessException | P0 |
| NEG-003 | Input | Null Code | BusinessException (if required) | P0 |
| NEG-004 | Input | Duplicate Code | BusinessException: code exists | P0 |
| NEG-005 | Input | Invalid Type | BusinessException | P0 |
| NEG-006 | Input | Non-existent ParentId | KeyNotFoundException | P0 |
| NEG-007 | Input | Deleted ParentId | BusinessException | P0 |
| NEG-008 | Input | Update non-existent | KeyNotFoundException | P0 |
| NEG-009 | Input | Delete non-existent | KeyNotFoundException | P0 |
| NEG-010 | Input | SQL injection in Name | Parameterized | P0 |
| NEG-011 | Auth | No auth | Unauthorized | P0 |
| NEG-012 | Auth | No create perm | Unauthorized | P0 |
| NEG-013 | Auth | No update perm | Unauthorized | P0 |
| NEG-014 | Auth | No delete perm | Unauthorized | P0 |
| NEG-015 | Auth | Scoped user out of scope | Unauthorized | P0 |
| NEG-016 | Auth | Expired token | Unauthorized | P0 |
| NEG-017 | Auth | Tampered JWT | Unauthorized | P0 |
| NEG-018 | Auth | Disabled account | Unauthorized | P1 |
| NEG-019 | Auth | Post-logout | Unauthorized | P1 |
| NEG-020 | Auth | Role escalation | Ignored | P0 |
| NEG-021 | Hierarchy | Hub without Region parent | BusinessException: invalid parent type | P0 |
| NEG-022 | Hierarchy | OrgUnit without Hub parent | BusinessException: invalid parent type | P0 |
| NEG-023 | Hierarchy | Region with Region parent | BusinessException: regions are root | P0 |
| NEG-024 | Hierarchy | Move to self | BusinessException: self-reference | P0 |
| NEG-025 | Hierarchy | Move to own descendant | BusinessException: circular | P0 |
| NEG-026 | Hierarchy | Move OrgUnit under OrgUnit | BusinessException: invalid (if rules restrict) | P1 |
| NEG-027 | Hierarchy | Create depth > max | BusinessException: max depth | P1 |
| NEG-028 | Hierarchy | Move creates depth > max | BusinessException | P1 |
| NEG-029 | Hierarchy | Delete with children | BusinessException: has children | P0 |
| NEG-030 | Hierarchy | Delete with active partners | BusinessException: has partners | P0 |
| NEG-031 | State | Update deleted | BusinessException | P1 |
| NEG-032 | State | Delete already deleted | No-op or error | P1 |
| NEG-033 | State | Move deleted OrgUnit | BusinessException | P1 |
| NEG-034 | State | Move to deleted parent | BusinessException | P1 |
| NEG-035 | State | Assign partner to deleted OrgUnit | BusinessException | P1 |
| NEG-036 | Null | All nulls on create | Multiple errors | P1 |
| NEG-037 | Null | Null request | ArgumentNull | P1 |
| NEG-038 | Null | Whitespace Name | BusinessException | P1 |
| NEG-039 | Null | Null Type | BusinessException | P1 |
| NEG-040 | Null | Null ParentId for Hub | BusinessException: parent required | P1 |
| NEG-041 | Dep | DB connection lost | Exception | P1 |
| NEG-042 | Dep | DB timeout | Timeout | P1 |
| NEG-043 | Dep | AutoMapper missing | MappingException | P2 |
| NEG-044 | Dep | Constraint violation | BusinessException | P1 |
| NEG-045 | Dep | Connection pool exhausted | Graceful error | P1 |
| NEG-046 | XSS | XSS in Name | Sanitized | P0 |
| NEG-047 | XSS | XSS in Code | Sanitized | P0 |
| NEG-048 | XSS | XSS in Description | Sanitized | P1 |
| NEG-049 | Length | Name > 200 chars | Validation error | P1 |
| NEG-050 | Length | Code > 50 chars | Validation error | P1 |
| NEG-051 | Length | Description > 4000 | Validation error | P2 |
| NEG-052 | ID | Negative ID | Not found | P1 |
| NEG-053 | ID | Zero ID | Not found | P1 |
| NEG-054 | ID | Float ID | 400 | P1 |
| NEG-055 | ID | String ID | 400 | P1 |
| NEG-056 | Page | Page = 0 | Default | P2 |
| NEG-057 | Page | PageSize = -1 | Error | P2 |
| NEG-058 | Page | PageSize > 1000 | Capped | P2 |
| NEG-059 | Sort | Invalid column | Default | P2 |
| NEG-060 | Search | Empty search | All or empty | P1 |
| NEG-061 | Search | Regex chars | Escaped | P1 |
| NEG-062 | Tree | Circular reference A→B→A | Detected, prevented | P0 |
| NEG-063 | Tree | Very deep hierarchy (25 levels) | Rejected or warning | P1 |
| NEG-064 | Delete | Delete Region with Hubs | BusinessException: has children | P0 |
| NEG-065 | Delete | Delete Hub with OrgUnits | BusinessException: has children | P0 |
| NEG-066 | Code | Duplicate code different case | "EU-001" vs "eu-001" | Case-insensitive unique | P1 |
| NEG-067 | Code | Code with special chars | Validated | P2 |
| NEG-068 | Move | Move to same parent | No-op | P2 |
| NEG-069 | Bulk | Batch delete with mixed valid/invalid | Valid deleted, invalid error | P1 |
| NEG-070 | Multiple | Multiple validation errors | All returned | P1 |

---

## §3 Boundary Tests — 70 tests

| ID | Category | Scenario | Expected | Pr |
|----|----------|---------|----------|----|
| BND-001 | String | Name 1 char | Accepted | P1 |
| BND-002 | String | Name 200 chars | Accepted | P1 |
| BND-003 | String | Name 201 chars | Rejected | P1 |
| BND-004 | String | Code 1 char | Accepted | P1 |
| BND-005 | String | Code 50 chars | Accepted | P1 |
| BND-006 | String | Code 51 chars | Rejected | P1 |
| BND-007 | String | Description 0 | Accepted | P2 |
| BND-008 | String | Description 4000 | Accepted | P2 |
| BND-009 | String | Description 4001 | Rejected | P2 |
| BND-010 | Numeric | OrgUnit ID = 1 | Retrieved | P1 |
| BND-011 | Numeric | OrgUnit ID MAX_INT | Handled | P2 |
| BND-012 | Numeric | ParentId = 1 | Valid parent | P1 |
| BND-013 | Numeric | Page 1 | First page | P1 |
| BND-014 | Numeric | Page 10000 | Handled | P2 |
| BND-015 | Numeric | PageSize 1 | 1 result | P1 |
| BND-016 | Numeric | PageSize 1000 | Max page | P1 |
| BND-017 | Collection | 0 org units | Empty tree | P1 |
| BND-018 | Collection | 1 region only | Single root | P1 |
| BND-019 | Collection | 1 region + 1 hub | Two levels | P1 |
| BND-020 | Collection | 100 org units | Loaded <2s | P1 |
| BND-021 | Collection | 1000 org units | Loaded <5s | P1 |
| BND-022 | Collection | 10,000 org units | Performance test | P1 |
| BND-023 | Collection | Region with 0 hubs | No children | P1 |
| BND-024 | Collection | Hub with 0 org units | No children | P1 |
| BND-025 | Collection | Hub with 100 org units | All listed | P1 |
| BND-026 | Collection | Hub with 1000 org units | Paginated | P1 |
| BND-027 | Depth | Depth 0 (root) | Valid | P1 |
| BND-028 | Depth | Depth 1 (Hub) | Valid | P1 |
| BND-029 | Depth | Depth 2 (OrgUnit) | Valid | P1 |
| BND-030 | Depth | Depth = max (e.g. 10) | Valid | P1 |
| BND-031 | Depth | Depth = max + 1 | Rejected | P1 |
| BND-032 | Unicode | Name Arabic | Stored | P2 |
| BND-033 | Unicode | Name Chinese | Stored | P2 |
| BND-034 | Unicode | Name Cyrillic | Stored | P2 |
| BND-035 | Unicode | Name French accents | Stored | P2 |
| BND-036 | Unicode | Code with dashes | "EU-001" stored | P1 |
| BND-037 | Unicode | Code with underscores | "EU_001" stored | P2 |
| BND-038 | Unicode | Name with apostrophe | "St. John's" | P2 |
| BND-039 | Date | Created leap year | Correct | P2 |
| BND-040 | Date | Created midnight UTC | No boundary error | P2 |
| BND-041 | Date | Very old creation | Handled | P2 |
| BND-042 | Search | 1-char search | Matches | P1 |
| BND-043 | Search | 255-char search | Processed | P1 |
| BND-044 | Search | Exact match | Found | P2 |
| BND-045 | Search | Partial match | Found | P2 |
| BND-046 | Width | Root with 1 child | Simple tree | P2 |
| BND-047 | Width | Root with 500 children | Wide tree | P1 |
| BND-048 | Width | Each hub with 50 OrgUnits | Balanced tree | P2 |
| BND-049 | Ancestors | Root node (0 ancestors) | Empty list | P1 |
| BND-050 | Ancestors | Leaf node (2 ancestors) | [Hub, Region] | P1 |
| BND-051 | Descendants | Root with all descendants | Complete subtree | P1 |
| BND-052 | Descendants | Leaf with 0 descendants | Empty | P1 |
| BND-053 | Move | Move to sibling | Valid reparent | P1 |
| BND-054 | Move | Move from depth 2 to depth 1 | Valid | P1 |
| BND-055 | Move | Move from depth 1 to depth 2 | Valid if type allows | P1 |
| BND-056 | Code | Code exactly min (1 char) | Accepted | P1 |
| BND-057 | Code | Code exactly max (50 chars) | Accepted | P1 |
| BND-058 | Count | Partners count for empty OrgUnit | 0 | P2 |
| BND-059 | Count | Partners count for OrgUnit with 100 | 100 | P2 |
| BND-060 | Count | Users count for empty OrgUnit | 0 | P2 |
| BND-061 | Sort | Each available column | Works | P1 |
| BND-062 | Page | Exactly page size items | Full page, no next | P1 |
| BND-063 | Page | PageSize + 1 items | 2 pages | P1 |
| BND-064 | Page | Last page with 1 item | Single item | P2 |
| BND-065 | Type | Each valid type | Accepted | P1 |
| BND-066 | Type | Type at enum boundaries | Handled | P2 |
| BND-067 | Tree | Flat list (all root) | Valid | P1 |
| BND-068 | Tree | All under single root | Single branch | P1 |
| BND-069 | Tree | Balanced binary tree | Correct structure | P2 |
| BND-070 | Tree | Highly unbalanced (linear chain) | Handled | P1 |

---

## §4-§10 (Functional through Load Tests)

Following the same pattern as other migrated files, organized by subsections:

### §4 Functional Tests — 50 tests
**4.1 Workflow (15):** Hierarchy rules (Region→Hub→OrgUnit), type enforcement, soft-delete excludes, audit fields, move validation, code uniqueness, descendant/ancestor traversal, pagination defaults, search case-insensitivity, child count updates on deletion, permission-based filtering, sort order enforcement, tree build excluding deleted, new unit appears after refresh, delete cascades blocked.

**4.2 Validation (15):** Name required, Code unique, Type valid enum, ParentId valid (exists/not-deleted), hierarchy type rules (Hub needs Region parent, OrgUnit needs Hub parent), circular reference prevention, max depth check, XSS prevention, API content-type, code format validation, move target validation, sort parameter validation, description max length, code case-insensitive uniqueness, move doesn't break depth.

**4.3 Constraints (10):** Max page size 1000, FK parent exists, unique code DB constraint, soft-delete no cascade to partners, children block deletion, partners block deletion, max tree depth, search result limit, batch operation limit, concurrent move limit.

**4.4 Audit (10):** Create audit, update audit, delete audit, move audit (old parent, new parent), name change audit, code change audit, read no audit, failed operation no audit, batch audit, type change audit.

### §5 Integration Tests — 50 tests
**5.1 CRUD (10):** Full lifecycle, create→listed, delete→excluded, update→persisted, create Region+Hub+OrgUnit chain, move→hierarchy updated, restore deleted, batch create, update code→searchable, create with all optional.

**5.2 Search & Filter (10):** Search by name, search by code, filter by type, combined search+filter, case-insensitive, empty results, filter excludes deleted, search within subtree, filter by multiple types, clear filters.

**5.3 Pagination (5):** Page 1, last page, empty, single page, max page size.

**5.4 Relationships (10):** OrgUnit→Partners, OrgUnit→Users, OrgUnit→Parent, OrgUnit→Children, Region→Hubs, Hub→OrgUnits, delete OrgUnit→partners unaffected, move→scope changes, OrgUnit→Opportunities (via partners), audit trail.

**5.5 Error Handling (15):** Invalid data 400, not found 404, unauthorized 403, circular reference 400, delete with children 400, delete with partners 400, duplicate code 400, invalid hierarchy type 400, DB timeout 500, concurrency 409, malformed request 400, rate limit 429, SQL injection sanitized, large payload 413, session expired 401.

### §6 Security Tests — 50 tests
**6.1 Injection (10):** SQL Name, SQL search, XSS Name, XSS Code, LDAP, path traversal, HTML injection, JSON injection, template injection, OS command.

**6.2 Access Control (10):** Anonymous, no permission, scoped violation, expired token, tampered JWT, vertical escalation, horizontal access, disabled account, post-logout, role escalation.

**6.3 IDOR (10):** Guess ID, enumerate, deleted ID, other OrgUnit, negative ID, zero ID, float ID, string ID, MAX_INT, other user's OrgUnit.

**6.4 Mass Assignment (5):** IsDeleted, CreatedBy, CreatedDate, Id, Code (read-only after creation).

**6.5 Auth & Session (10):** Brute-force, session fixation, hijacking, CSRF create, CSRF delete, token storage, concurrent sessions, token refresh, logout, HTTPS.

**6.6 Data Exposure (5):** Internal fields, stack traces, sensitive OrgUnit data, cache, tokens in URL.

### §7 Concurrency Tests — 25 tests
Two users update same OrgUnit, concurrent move operations, concurrent create under same parent, move during read, delete during read, concurrent code assignment (uniqueness), rapid hierarchy changes, DB deadlock, token refresh during move, bulk operations concurrent, cache invalidation, optimistic concurrency, concurrent delete attempts, session timeout, parallel parent lookups, concurrent search, move during deletion, create during migration, tree rebuild during browse, connection pool, multiple users creating children, concurrent type changes, export during modification, ancestor query during move, real-time update propagation.

### §8 Unit Tests — 21 tests
**Validation (5):** Null name, invalid type, duplicate code, circular reference, max depth.
**Formatting (3):** Name trim, code normalize, hierarchy path format.
**Calculations (5):** Tree depth, descendant count, ancestor chain, children count (non-deleted), subtree size.
**Status (5):** IsDeleted check, valid parent type, circular reference detection, move validity, deletion eligibility.
**Collections (3):** Build tree from flat, filter by type, sort siblings.

### §9 Performance Tests — 16 tests
Create single (<200ms), get with includes (<300ms), build tree 100 units (<500ms), build tree 1000 (<2s), get descendants 500 (<1s), search 1000 (<500ms), search 5000 (<1s), paginate 10,000 (<500ms/page), move operation (<500ms), count query (<100ms), 10 concurrent creates (<1s), 50 concurrent reads (<500ms), tree build with 10,000 units (<5s), memory 10,000 (<200MB), memory 50,000 (<500MB), memory leak check (no >10%).

### §10 Load Tests — 10 tests
50 concurrent ops (30min, 95%<500ms), 100 concurrent reads (30min, 95%<300ms), 50 concurrent searches (15min, <1s), spike 10→200 (5min, recovery <30s), spike + moves (5min, all correct), 500 concurrent (10min, graceful), 100K units in DB (15min, <1s), continuous CRUD (10min, stable), recovery DB crash (<60s), recovery service restart (<30s).

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|-----------|
| Hierarchy (Region→Hub→OrgUnit) | POS-006–008, NEG-021–028, BND-027–031 |
| Code uniqueness | POS-026, NEG-004, NEG-066, FUN validation |
| Move operations | POS-004, NEG-024–028, NEG-033–034 |
| Deletion rules | POS-005, NEG-029–030, NEG-064–065 |
| Tree traversal | POS-011–013, BND-049–052 |
| Type filtering | POS-014–016, BND-065–066 |
| Security | SEC-001–050 |
| Performance | PRF-001–016, LDT-001–010 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
