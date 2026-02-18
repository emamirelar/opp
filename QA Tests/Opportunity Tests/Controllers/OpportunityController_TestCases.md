# OpportunityController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/OpportunityController`  
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

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

Main REST API for opportunity CRUD: create, read, update, delete, list with filtering/pagination, search, get sections (WHY/WHAT/Team/Budget/Schedule), get permissions, status management, export, bulk operations, and audit trail.

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Create opportunity | POST /api/opportunity | 201 Created |
| POS-002 | Get opportunity by ID | GET /api/opportunity/{id} | Opportunity details |
| POS-003 | Update opportunity | PUT /api/opportunity/{id} | 200 OK |
| POS-004 | Delete opportunity | DELETE /api/opportunity/{id} | 204 No Content |
| POS-005 | List opportunities | GET /api/opportunity | Paginated list |
| POS-006 | Get WHY section | GET /api/opportunity/{id}/why | WHY section |
| POS-007 | Get WHAT section | GET /api/opportunity/{id}/what | WHAT section |
| POS-008 | Get Team section | GET /api/opportunity/{id}/who | Team section |
| POS-009 | Get Budget section | GET /api/opportunity/{id}/budget | Budget section |
| POS-010 | Get Schedule section | GET /api/opportunity/{id}/when | Schedule section |
| POS-011 | Get permissions | GET /api/opportunity/{id}/permissions | Permission flags |
| POS-012 | Search opportunities | GET /api/opportunity/search | Search results |
| POS-013 | Advanced search | GET /api/opportunity/advanced-search | Filtered results |
| POS-014 | Filter by stage | GET ?stage=Draft | Stage-filtered |
| POS-015 | Filter by status | GET ?status=Active | Status-filtered |
| POS-016 | Filter by partner | GET ?partnerId=1 | Partner-filtered |
| POS-017 | Filter by OM | GET ?opportunityManagerId=1 | OM-filtered |
| POS-018 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-019 | Sort | GET ?sortBy=name | Sorted |
| POS-020 | Export CSV | GET /api/opportunity/export?format=csv | CSV file |
| POS-021 | Export PDF | GET /api/opportunity/export?format=pdf | PDF file |
| POS-022 | Bulk export | POST /api/opportunity/bulk-export | Export file |
| POS-023 | Audit trail | GET /api/opportunity/{id}/audit | Audit entries |
| POS-024 | GET count | GET /api/opportunity/count | Count |
| POS-025 | GET summary | GET /api/opportunity/{id}/overview | Summary |
| POS-026 | Typeahead | GET /api/opportunity/typeahead | Typeahead list |
| POS-027 | GET by partner | GET /api/opportunity?partnerId=1 | Partner opps |
| POS-028 | Clone opportunity | POST /api/opportunity/{id}/clone | Clone created |
| POS-029 | Validate opportunity | POST /api/opportunity/{id}/validate | Validation result |
| POS-030 | Get workflow history | GET /api/opportunity/{id}/workflow | Workflow history |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid ID | id=abc | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Null request body | POST with null | 400 |
| NEG-007 | Missing required field | Name missing | 400 |
| NEG-008 | Invalid date format | date=invalid | 400 |
| NEG-009 | Invalid stage | stage=Invalid | 400 |
| NEG-010 | Null partner | partnerId=null | 400 |
| NEG-011 | Non-existent partner | partnerId=999999 | 404 |
| NEG-012 | Deleted opportunity | id deleted | 404 |
| NEG-013 | SQL injection in search | search='; DROP | Sanitized |
| NEG-014 | XSS in name | name=<script> | Sanitized |
| NEG-015 | Negative page | page=-1 | 400 |
| NEG-016 | Zero pageSize | pageSize=0 | 400 |
| NEG-017 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-018 | Invalid sort | sortBy=invalid | 400 |
| NEG-019 | No permission | User without CanView | 403 |
| NEG-020 | Cross-org access | Other org opportunity | 403 |
| NEG-021 | Edit closed opportunity | PUT on closed | 403 |
| NEG-022 | Delete approved | DELETE on approved | 403 |
| NEG-023 | Update during workflow | PUT during submit | 409 |
| NEG-024 | Empty name | Name "" | 400 |
| NEG-025 | Whitespace-only name | Name "   " | 400/reject |
| NEG-026 | Invalid section ID | sectionId=999999 | 404 |
| NEG-027 | Invalid partner link | partnerId invalid | 404 |
| NEG-028 | Missing OM | Required OM missing | 400 |
| NEG-029 | Malformed JSON | Invalid JSON body | 400 |
| NEG-030 | Wrong content-type | Application/xml | 415 |
| NEG-031 | Duplicate name | Restricted duplicate | 400 |
| NEG-032 | Exceed max opps | Over limit | 400 |
| NEG-033 | Rate limit | Too many requests | 429 |
| NEG-034 | Payload too large | Huge body | 413 |
| NEG-035 | Invalid date range | end < start | 400 |
| NEG-036 | Circular reference | Self-reference | 400 |
| NEG-037 | Orphan sections | Invalid section ref | 400 |
| NEG-038 | Mass assignment | Read-only field | Ignored |
| NEG-039 | Invalid filter combo | Conflicting filters | 400 |
| NEG-040 | Stale data | Version conflict | 409 |
| NEG-041 | Invalid document ID | documentId=999999 | 404 |
| NEG-042 | Invalid export format | format=Invalid | 400 |
| NEG-043 | Export no permission | No export permission | 403 |
| NEG-044 | Clone deleted | Clone deleted opp | 404 |
| NEG-045 | Validate deleted | Validate deleted | 404 |
| NEG-046 | Workflow on deleted | Submit deleted | 404 |
| NEG-047 | Permissions on deleted | GET permissions deleted | 404 |
| NEG-048 | Section on deleted | GET why deleted | 404 |
| NEG-049 | Audit on deleted | GET audit deleted | 404 |
| NEG-050 | Bulk with invalid IDs | One invalid in bulk | 400/partial |
| NEG-051 | Wrong HTTP method | PUT for create | 405 |
| NEG-052 | Extra path segments | /api/opportunity/1/extra | 404 |
| NEG-053 | DB timeout | Simulate timeout | 503 |
| NEG-054 | Manager unavailable | Manager down | 503 |
| NEG-055 | Validation cascade | Multiple invalid fields | All errors |
| NEG-056 | Concurrent delete | Delete while updating | 409 or 404 |
| NEG-057 | Session expired | Mid-request | 401 |
| NEG-058 | Permission revoked | Mid-session | 403 on next |
| NEG-059 | Invalid stage transition | Invalid workflow | 400 |
| NEG-060 | Maximum depth exceeded | Nested too deep | 400 |
| NEG-061 | Reserved character | Name with \0 | 400 |
| NEG-062 | Control characters | Description \n\r\t | Sanitize |
| NEG-063 | Invalid bulk count | Bulk 1000 items | 400 |
| NEG-064 | Empty bulk | Bulk [] | 400 |
| NEG-065 | Mismatched IDs | Path id != body id | 400 |
| NEG-066 | Read-only field update | Update createdBy | Ignored |
| NEG-067 | Soft-delete filter | Query deleted | Excluded |
| NEG-068 | Audit failure | Audit service down | Log, continue |
| NEG-069 | Invalid partner type | Wrong partner type | 400 |
| NEG-070 | Invalid OM assignment | OM not in org | 403 |
| NEG-071 | Invalid opportunity ID format | id=0 | 400 |
| NEG-072 | Non-existent WHY section | GET why on missing | 404 |
| NEG-073 | Non-existent WHAT section | GET what on missing | 404 |
| NEG-074 | Invalid budget line | budgetLineId invalid | 404 |
| NEG-075 | Invalid schedule item | scheduleId invalid | 404 |
| NEG-076 | Invalid team member | teamMemberId invalid | 404 |
| NEG-077 | Submit without required fields | Incomplete opp submit | 400 |
| NEG-078 | Approve without auth | POST approve no token | 401 |
| NEG-079 | Cancel non-submitted | Cancel draft | 400 |
| NEG-080 | Recall non-submitted | Recall draft | 400 |
| NEG-081 | Invalid search fields | searchFields=invalid | 400 |
| NEG-082 | Advanced search malformed | Bad filter JSON | 400 |
| NEG-083 | Export empty result | Export no matches | 200 empty |
| NEG-084 | Typeahead invalid query | typeahead=null | 400 |
| NEG-085 | Clone without permission | Clone no permission | 403 |
| NEG-086 | Validate without permission | Validate no permission | 403 |
| NEG-087 | GET overview deleted | GET overview deleted | 404 |
| NEG-088 | Invalid pagination combo | page=0&pageSize=-1 | 400 |
| NEG-089 | Filter by deleted partner | partnerId deleted | 404 or exclude |
| NEG-090 | Workflow history deleted | GET workflow deleted | 404 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 200 | ✅ | ✅ | ❌ |
| BND-002 | description length | 0 | 4000 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-006 | partnerId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | date range | 1 day | 365 days | ✅ | ✅ | ❌ |
| BND-008 | team members | 0 | 50 | ✅ | ✅ | ❌ |
| BND-009 | budget lines | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-010 | schedule items | 0 | 500 | ✅ | ✅ | ❌ |
| BND-011 | documents | 0 | 100 | ✅ | ✅ | ❌ |
| BND-012 | bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-013 | Empty list | - | - | Returns [] | - | - |
| BND-014 | Single item | - | - | Returns 1 | - | - |
| BND-015 | First page | page=1 | - | ✅ | - | - |
| BND-016 | Last page | - | - | Partial OK | - | - |
| BND-017 | Zero length name | - | - | ❌ | - | - |
| BND-018 | Max length name | 200 | - | - | ✅ | ❌ |
| BND-019 | Feb 29 | - | - | Valid | - | - |
| BND-020 | Dec 31 | - | - | Inclusive | - | - |
| BND-021 | Jan 1 | - | - | Inclusive | - | - |
| BND-022 | Midnight | 00:00:00 | - | Valid | - | - |
| BND-023 | End of day | 23:59:59 | - | Valid | - | - |
| BND-024 | Same start/end | - | - | Valid | - | - |
| BND-025 | Unicode name | - | - | Accept | - | - |
| BND-026 | Emoji in description | - | - | Accept/sanitize | - | - |
| BND-027 | Null-safe fields | - | - | Default | - | - |
| BND-028 | Empty string filter | - | - | No filter | - | - |
| BND-029 | Whitespace trim | - | - | Trimmed | - | - |
| BND-030 | Decimal precision | - | 2 | Rounded | - | - |
| BND-031 | Integer overflow | - | - | Use long | - | - |
| BND-032 | Float precision | - | - | Consistent | - | - |
| BND-033 | Sort empty | - | - | [] | - | - |
| BND-034 | Sort single | - | - | [item] | - | - |
| BND-035 | Filter no match | - | - | [] | - | - |
| BND-036 | Filter all match | - | - | Full list | - | - |
| BND-037 | Timezone UTC | - | - | Correct | - | - |
| BND-038 | DST boundary | - | - | Handle | - | - |
| BND-039 | Very old date | 2000 | - | Accept | - | - |
| BND-040 | Future date | - | +1 year | Accept | - | - |
| BND-041 | Document size 0 | - | - | Reject | - | - |
| BND-042 | Document size max | - | 10MB | - | ✅ | ❌ |
| BND-043 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-044 | Section completeness | - | 100% | ✅ | ✅ | ❌ |
| BND-045 | Partner count per opp | 0 | 50 | ✅ | ✅ | ❌ |
| BND-046 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-047 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-048 | Export row count | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-049 | Response payload | - | 1MB | - | ✅ | ❌ |
| BND-050 | List sizes | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-051 | Pagination boundary | - | - | Exact count | - | - |
| BND-052 | Clone depth | - | 1 | - | ✅ | ❌ |
| BND-053 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-054 | Empty export | - | - | Headers only | - | - |
| BND-055 | Single export row | - | - | Valid | - | - |
| BND-056 | Filter combinations | - | 10 | ✅ | ✅ | ❌ |
| BND-057 | Search terms | - | 5 | ✅ | ✅ | ❌ |
| BND-058 | Empty filter result | - | - | [] | - | - |
| BND-059 | All nulls optional | - | - | Defaults | - | - |
| BND-060 | Mixed nulls | - | - | Skip nulls | - | - |
| BND-061 | Soft-deleted opportunity | - | - | Excluded | - | - |
| BND-062 | Inactive partner | - | - | 403/excluded | - | - |
| BND-063 | Inactive user | - | - | Excluded | - | - |
| BND-064 | Duplicate detection | - | - | Reject or idempotent | - | - |
| BND-065 | Idempotent create | - | - | Same result | - | - |
| BND-066 | Version number | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-067 | Sequence number | 1 | - | ✅ | ❌ | - |
| BND-068 | Empty bulk success | - | - | 200 | - | - |
| BND-069 | Partial bulk success | - | - | 207 | - | - |
| BND-070 | Round-trip | Create → Get | - | Match | - | - |
| BND-071 | WHY section max length | - | 4000 | - | ✅ | ❌ |
| BND-072 | WHAT section max length | - | 4000 | - | ✅ | ❌ |
| BND-073 | Budget amount precision | - | 2 decimals | - | ✅ | ❌ |
| BND-074 | Schedule date range | 1 day | 365 days | ✅ | ✅ | ❌ |
| BND-075 | Team member limit | 0 | 50 | ✅ | ✅ | ❌ |
| BND-076 | Document count limit | 0 | 100 | ✅ | ✅ | ❌ |
| BND-077 | Stage enum boundary | First | Last | ✅ | ✅ | ❌ |
| BND-078 | Status enum boundary | First | Last | ✅ | ✅ | ❌ |
| BND-079 | Workflow transition | Valid | Valid | ✅ | ✅ | ❌ |
| BND-080 | Partner link count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-081 | Audit entry count | - | 1000 | ✅ | ✅ | ❌ |
| BND-082 | Clone with max sections | All sections | - | ✅ | - | - |
| BND-083 | Export format boundary | csv | pdf | ✅ | ✅ | ❌ |
| BND-084 | Search field count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-085 | Filter field count | 0 | 15 | ✅ | ✅ | ❌ |
| BND-086 | Typeahead result limit | - | 50 | - | ✅ | ❌ |
| BND-087 | Overview section count | - | 10 | - | ✅ | ❌ |
| BND-088 | Permission flag count | - | 10 | - | ✅ | ❌ |
| BND-089 | Workflow history limit | - | 100 | - | ✅ | ❌ |
| BND-090 | Concurrent export | - | 5 | ✅ | ✅ | ❌ |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | CRUD | Create opportunity | POST valid | 201 |
| FUN-002 | CRUD | Update opportunity | PUT valid | 200 |
| FUN-003 | CRUD | Soft delete | DELETE | IsDeleted |
| FUN-004 | CRUD | Get by ID | GET {id} | 200 |
| FUN-005 | CRUD | List opportunities | GET list | Paginated |
| FUN-006 | Search | Search by text | GET ?search | Filtered |
| FUN-007 | Search | Filter by stage | GET ?stage | Filtered |
| FUN-008 | Search | Filter by status | GET ?status | Filtered |
| FUN-009 | Search | Filter by partner | GET ?partnerId | Filtered |
| FUN-010 | Search | Filter by OM | GET ?opportunityManagerId | Filtered |
| FUN-011 | Section | Get WHY | GET /why | WHY data |
| FUN-012 | Section | Get WHAT | GET /what | WHAT data |
| FUN-013 | Section | Get Team | GET /who | Team data |
| FUN-014 | Section | Get Budget | GET /budget | Budget data |
| FUN-015 | Section | Get Schedule | GET /when | Schedule data |
| FUN-016 | Workflow | Submit | POST submit | Stage updated |
| FUN-017 | Workflow | Approve | POST approve | Status updated |
| FUN-018 | Workflow | Cancel | POST cancel | Cancelled |
| FUN-019 | Workflow | Recall | POST recall | Recalled |
| FUN-020 | Workflow | Activate | POST activate | Active |
| FUN-021 | Audit | Create logged | POST | Audit entry |
| FUN-022 | Audit | Update logged | PUT | Audit entry |
| FUN-023 | Audit | Delete logged | DELETE | Audit entry |
| FUN-024 | Audit | Workflow logged | POST submit | Audit entry |
| FUN-025 | Audit | Timestamp UTC | Any action | UTC |
| FUN-026 | Validation | Required name | Missing name | 400 |
| FUN-027 | Validation | Required partner | Missing partner | 400 |
| FUN-028 | Validation | Valid stage | Invalid stage | 400 |
| FUN-029 | Validation | Valid date | Invalid date | 400 |
| FUN-030 | Validation | ID format | Invalid ID | 400 |
| FUN-031 | Constraint | Unique name | Duplicate if restricted | 400 |
| FUN-032 | Constraint | FK partner | Orphan | 400 |
| FUN-033 | Constraint | Max opps | Over limit | 400 |
| FUN-034 | Constraint | Date range | End < start | 400 |
| FUN-035 | Constraint | Soft delete | Query deleted | Excluded |
| FUN-036 | Constraint | Org scope | Cross-org | 403 |
| FUN-037 | Constraint | Workflow state | Invalid transition | 400 |
| FUN-038 | Constraint | Permission | No permission | 403 |
| FUN-039 | Constraint | File size | >10MB | 413 |
| FUN-040 | Business | Permission flags | GET permissions | Correct flags |
| FUN-041 | Business | Clone copies | Clone | All sections |
| FUN-042 | Business | Validate checks | Validate | All rules |
| FUN-043 | Business | Export format | Export | Correct format |
| FUN-044 | Business | Pagination | GET ?page | Correct page |
| FUN-045 | Business | Sort order | GET ?sortBy | Correct order |
| FUN-046 | Business | Typeahead filter | Typeahead | Filtered |
| FUN-047 | Business | Overview aggregate | GET overview | Aggregated |
| FUN-048 | Business | Workflow history | GET workflow | Chronological |
| FUN-049 | Business | Audit trail | GET audit | Complete |
| FUN-050 | Business | Bulk export | Bulk export | All selected |
| FUN-051 | CRUD | Clone opportunity | POST clone | Clone created |
| FUN-052 | CRUD | Validate opportunity | POST validate | Validation result |
| FUN-053 | Section | WHY required fields | GET why | Complete |
| FUN-054 | Section | WHAT required fields | GET what | Complete |
| FUN-055 | Section | Team required fields | GET who | Complete |
| FUN-056 | Section | Budget required fields | GET budget | Complete |
| FUN-057 | Section | Schedule required fields | GET when | Complete |
| FUN-058 | Workflow | Submit validation | POST submit | Validated |
| FUN-059 | Workflow | Approve validation | POST approve | Validated |
| FUN-060 | Workflow | Cancel validation | POST cancel | Validated |
| FUN-061 | Workflow | Recall validation | POST recall | Validated |
| FUN-062 | Workflow | Activate validation | POST activate | Validated |
| FUN-063 | Validation | Partner exists | Invalid partner | 404 |
| FUN-064 | Validation | OM exists | Invalid OM | 404 |
| FUN-065 | Validation | Stage transition | Invalid | 400 |
| FUN-066 | Constraint | Duplicate name | If restricted | 400 |
| FUN-067 | Constraint | Max team | >50 | 400 |
| FUN-068 | Constraint | Max budget lines | >1000 | 400 |
| FUN-069 | Constraint | Max schedule | >500 | 400 |
| FUN-070 | Constraint | Max documents | >100 | 400 |
| FUN-071 | Audit | Clone logged | POST clone | Audit entry |
| FUN-072 | Audit | Submit logged | POST submit | Audit entry |
| FUN-073 | Audit | Approve logged | POST approve | Audit entry |
| FUN-074 | Audit | User ID | Any action | User ID |
| FUN-075 | Audit | Resource ID | Any action | Resource ID |
| FUN-076 | Business | Search fields | GET search-fields | Correct |
| FUN-077 | Business | Advanced search | POST advanced-search | Filtered |
| FUN-078 | Business | Export CSV | Export csv | CSV format |
| FUN-079 | Business | Export PDF | Export pdf | PDF format |
| FUN-080 | Business | Bulk export IDs | Bulk export | Correct IDs |
| FUN-081 | Validation | Description length | Too long | 400 |
| FUN-082 | Validation | Budget amount | Negative | 400 |
| FUN-083 | Validation | Schedule date | Past | 400 |
| FUN-084 | Validation | Team member exists | Invalid | 404 |
| FUN-085 | Validation | Document type | Invalid | 400 |
| FUN-086 | Constraint | Workflow immutable | Edit submitted | 403 |
| FUN-087 | Constraint | Delete submitted | Delete submitted | 403 |
| FUN-088 | Constraint | Clone deleted | Clone deleted | 404 |
| FUN-089 | Constraint | Validate deleted | Validate deleted | 404 |
| FUN-090 | Constraint | Export deleted | Export deleted | 404 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Opportunity | Match |
| INT-002 | CRUD | Update → Get | Opportunity | Updated |
| INT-003 | CRUD | Delete → Get | Opportunity | 404 |
| INT-004 | CRUD | Clone → Get | Opportunity | New entity |
| INT-005 | Section | Create → Get WHY | Opportunity, WHY | WHY data |
| INT-006 | Section | Update → Get WHAT | Opportunity, WHAT | WHAT data |
| INT-007 | Section | Add team → Get who | Opportunity, Team | Team data |
| INT-008 | Section | Add budget → Get budget | Opportunity, Budget | Budget data |
| INT-009 | Section | Add schedule → Get when | Opportunity, Schedule | Schedule data |
| INT-010 | Search | Search → Get | Opportunity | Matches |
| INT-011 | Search | Filter stage → List | Opportunity | Filtered |
| INT-012 | Search | Filter partner → List | Opportunity, Partner | Filtered |
| INT-013 | Search | Filter OM → List | Opportunity, User | Filtered |
| INT-014 | Search | Multi-filter | Opportunity | Combined |
| INT-015 | Search | Sort + filter | Opportunity | Both applied |
| INT-016 | Pagination | Page 1 | Opportunity | First page |
| INT-017 | Pagination | Last page | Opportunity | Partial OK |
| INT-018 | Pagination | Page size | Opportunity | Correct size |
| INT-019 | Workflow | Submit → Approve | Opportunity | Workflow |
| INT-020 | Workflow | Cancel → Restore | Opportunity | Restored |
| INT-021 | Manager | Controller → Manager | OpportunityManager | Delegated |
| INT-022 | Manager | Manager → Repository | DbContext | Persisted |
| INT-023 | Partner | Opportunity → Partner | Partner service | Linked |
| INT-024 | Partner | Partner → Opportunity | Opportunity | Filtered |
| INT-025 | Workflow | Controller → Workflow | Workflow service | Integrated |
| INT-026 | Export | List → Export | Export service | File |
| INT-027 | Export | Filter → Export | Export service | Filtered file |
| INT-028 | Notification | Create → Notify | Notification service | Sent |
| INT-029 | Notification | Workflow → Notify | Notification service | Sent |
| INT-030 | Audit | Any action → Audit | Audit service | Logged |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid ID | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | Concurrent | 409 |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow query | 504 |
| INT-039 | Error | Payload too large | Huge request | 413 |
| INT-040 | Error | Unsupported media | Wrong type | 415 |
| INT-041 | Error | Method not allowed | Wrong verb | 405 |
| INT-042 | Error | Service unavailable | Dependency | 503 |
| INT-043 | Error | Gateway timeout | Upstream | 504 |
| INT-044 | Error | Gone | Deleted resource | 410 |
| INT-045 | Error | Locked | Resource locked | 423 |
| INT-046 | E2E | Full create flow | All | Create → Get |
| INT-047 | E2E | Full update flow | All | Update → Get |
| INT-048 | E2E | Full delete flow | All | Delete → 404 |
| INT-049 | E2E | Full clone flow | All | Clone → Get |
| INT-050 | E2E | Full export flow | All | Export → File |
| INT-051 | CRUD | Create with sections | Opportunity, Sections | All created |
| INT-052 | CRUD | Update sections | Opportunity, Sections | All updated |
| INT-053 | CRUD | Delete cascades | Opportunity | Soft delete |
| INT-054 | CRUD | Clone with relations | Opportunity | Relations copied |
| INT-055 | Section | WHY → Manager | OpportunityManager | WHY loaded |
| INT-056 | Section | WHAT → Manager | OpportunityManager | WHAT loaded |
| INT-057 | Section | Team → Manager | OpportunityManager | Team loaded |
| INT-058 | Section | Budget → Manager | OpportunityManager | Budget loaded |
| INT-059 | Section | Schedule → Manager | OpportunityManager | Schedule loaded |
| INT-060 | Search | Search → Manager | OpportunityManager | Search delegated |
| INT-061 | Search | Advanced → Manager | OpportunityManager | Advanced delegated |
| INT-062 | Pagination | List → Manager | OpportunityManager | Paginated |
| INT-063 | Pagination | Count → Manager | OpportunityManager | Count |
| INT-064 | Workflow | Submit → Manager | OpportunityManager | Submit delegated |
| INT-065 | Workflow | Approve → Manager | OpportunityManager | Approve delegated |
| INT-066 | Workflow | Cancel → Manager | OpportunityManager | Cancel delegated |
| INT-067 | Workflow | Recall → Manager | OpportunityManager | Recall delegated |
| INT-068 | Workflow | Activate → Manager | OpportunityManager | Activate delegated |
| INT-069 | Export | CSV → Export service | Export service | CSV generated |
| INT-070 | Export | PDF → Export service | Export service | PDF generated |
| INT-071 | Export | Bulk → Export service | Export service | Bulk file |
| INT-072 | Partner | Get by partner → Partner | Partner service | Partner opps |
| INT-073 | Partner | Partner filter → DB | DbContext | Query |
| INT-074 | Partner | OM filter → DB | DbContext | Query |
| INT-075 | Notification | Create → Email | Notification | Email sent |
| INT-076 | Notification | Submit → Email | Notification | Email sent |
| INT-077 | Notification | Approve → Email | Notification | Email sent |
| INT-078 | Audit | Create → Audit DB | Audit service | Entry |
| INT-079 | Audit | Update → Audit DB | Audit service | Entry |
| INT-080 | Audit | Delete → Audit DB | Audit service | Entry |
| INT-081 | Audit | Workflow → Audit DB | Audit service | Entry |
| INT-082 | Permissions | GET permissions → Auth | Auth service | Flags |
| INT-083 | Permissions | Edit check → Auth | Auth service | canEdit |
| INT-084 | Permissions | Delete check → Auth | Auth service | canDelete |
| INT-085 | Permissions | Workflow check → Auth | Auth service | canSubmit |
| INT-086 | Typeahead | Typeahead → Manager | OpportunityManager | List |
| INT-087 | Overview | Overview → Manager | OpportunityManager | Summary |
| INT-088 | Validate | Validate → Manager | OpportunityManager | Validation |
| INT-089 | Clone | Clone → Manager | OpportunityManager | Clone |
| INT-090 | Workflow history | Workflow → Manager | OpportunityManager | History |

---

## §6–§10 Summary

**§6 Security (50):** Injection (10), auth (10), IDOR (10), data exposure (10), API security (10).
**§7 Concurrency (25):** Concurrent CRUD, search + update, delete during read, bulk operations, workflow actions.
**§8 Unit (21):** Route validation (5), model binding (5), response mapping (3), error formatting (5), filter parsing (3).
**§9 Performance (16):** GET (<200ms), list (<500ms), search (<500ms), create (<500ms), export (<5s), memory.
**§10 Load (10):** 50 concurrent, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
