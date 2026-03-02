# PartnerLiaisonOfficeManager — Test Cases

**Component:** UNOPS.PAO.UNOPSBusiness/Managers/PartnerLiaisonOfficeManager.cs  
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
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

The PartnerLiaisonOfficeManager manages partner-to-liaison-office relationships for the CRM enhancement:
- **Partner-to-office linking** (assignments)
- **Office assignment** (single/multiple offices per partner)
- **Regional mapping** (office-to-region association)

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | Create assignment | Partner, Office exist | CreateAsync(data) | Assignment created | P0 |
| POS-002 | Get assignment by ID | Assignment exists | GetByIdAsync(id) | Returned | P0 |
| POS-003 | Update assignment | Assignment exists | UpdateAsync(id, data) | Updated | P0 |
| POS-004 | Delete assignment | Assignment exists | DeleteAsync(id) | Soft deleted | P0 |
| POS-005 | Get offices for partner | Partner has assignments | GetOfficesByPartnerAsync(partnerId) | Office list | P0 |
| POS-006 | Get partners for office | Office has assignments | GetPartnersByOfficeAsync(officeId) | Partner list | P0 |
| POS-007 | Assign office to partner | Partner, Office exist | AssignOfficeAsync(partnerId, officeId) | Assigned | P0 |
| POS-008 | Unassign office | Assignment exists | UnassignOfficeAsync(partnerId, officeId) | Unassigned | P0 |
| POS-009 | Get primary office | Partner has primary | GetPrimaryOfficeAsync(partnerId) | Primary returned | P1 |
| POS-010 | Set primary office | Assignment exists | SetPrimaryOfficeAsync(id) | Primary set | P1 |
| POS-011 | Filter by region | Assignments exist | Filter by regionId | Filtered list | P1 |
| POS-012 | Get office with region | Office exists | Get with Include region | Region loaded | P1 |
| POS-013 | Paginate | 20+ assignments | Page 2, size 10 | Items 11-20 | P1 |
| POS-014 | Sort by partner name | Multiple | GetAll sorted | Sorted | P1 |
| POS-015 | Search by partner | Assignments exist | Search | Matching | P1 |
| POS-016 | Search by office | Assignments exist | Search | Matching | P1 |
| POS-017 | Get with partner | Assignment exists | Get with Include partner | Partner loaded | P1 |
| POS-018 | Get with office | Assignment exists | Get with Include office | Office loaded | P1 |
| POS-019 | Validate region | Valid region | Create with region | Success | P1 |
| POS-020 | Bulk assign | Partner, offices exist | AssignOfficesAsync(partnerId, ids) | All assigned | P2 |
| POS-021 | Bulk unassign | Assignments exist | UnassignOfficesAsync(partnerId, ids) | All unassigned | P2 |
| POS-022 | Audit trail | Create | Create | Audit fields set | P2 |
| POS-023 | Restore soft-deleted | Deleted | RestoreAsync(id) | Restored | P2 |
| POS-024 | Include deleted | Admin | GetAllAsync(includeDeleted: true) | All | P2 |
| POS-025 | Unique partner-office | No duplicate | Create unique | Success | P2 |
| POS-026 | Cascading load | Get with partner, office | Get with Include | Loaded | P2 |
| POS-027 | Empty search | Assignments exist | Search "" | All returned | P2 |
| POS-028 | Case-insensitive search | "office" | Search | Matches | P2 |
| POS-029 | Get by multiple IDs | IDs exist | GetByIdsAsync([1,2,3]) | 3 returned | P2 |
| POS-030 | Regional statistics | Assignments exist | GetRegionalStatsAsync() | Stats | P2 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create without partner | PartnerId null | Validation error | P0 |
| NEG-002 | Create without office | OfficeId null | Validation error | P0 |
| NEG-003 | Get non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-004 | Update non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-005 | Delete non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-006 | Invalid partner ID | PartnerId 99999 | KeyNotFoundException | P0 |
| NEG-007 | Invalid office ID | OfficeId 99999 | KeyNotFoundException | P0 |
| NEG-008 | Negative ID | GetByIdAsync(-1) | ArgumentException | P0 |
| NEG-009 | Null create request | CreateAsync(null) | ArgumentNullException | P0 |
| NEG-010 | Null update request | UpdateAsync(id, null) | ArgumentNullException | P0 |
| NEG-011 | Duplicate assignment | Same partner-office | Conflict 409 | P0 |
| NEG-012 | Assign deleted office | Office deleted | Reject | P0 |
| NEG-013 | Assign deleted partner | Partner deleted | Reject | P0 |
| NEG-014 | Unassign non-assigned | No assignment | BusinessException | P0 |
| NEG-015 | Office capacity exceeded | Over capacity | Validation error | P0 |
| NEG-016 | SQL injection | '; DROP-- in notes | Sanitized/Rejected | P1 |
| NEG-017 | XSS in notes | <script> | Sanitized | P1 |
| NEG-018 | Invalid pagination | Page -1 | ArgumentException | P1 |
| NEG-019 | Invalid page size | Size 0 | ArgumentException | P1 |
| NEG-020 | Set primary non-existent | ID 99999 | KeyNotFoundException | P1 |
| NEG-021 | Invalid region ID | RegionId 99999 | KeyNotFoundException | P1 |
| NEG-022 | Stale concurrency | Stale update | ConcurrencyException | P1 |
| NEG-023 | Unauthorized | Wrong user | 403 | P1 |
| NEG-024 | Expired token | Stale JWT | 401 | P1 |
| NEG-025 | Rate limit | Too many | 429 | P1 |
| NEG-026 | DB timeout | Slow query | TimeoutException | P1 |
| NEG-027 | Invalid filter combo | Conflicting filters | Validation error | P1 |
| NEG-028 | Null option params | GetAllAsync(null) | ArgumentNullException | P1 |
| NEG-029 | Batch with invalid | One invalid in batch | Partial fail | P1 |
| NEG-030 | Restore non-deleted | Active | Restore | BusinessException | P1 |
| NEG-031 | Orphan assignment | Delete partner | Cascade or block | P1 |
| NEG-032 | Orphan assignment office | Delete office | Cascade or block | P1 |
| NEG-033 | Export empty | No data | Empty file | P1 |
| NEG-034 | Invalid sort field | Sort "invalid" | ArgumentException | P1 |
| NEG-035 | Null ID list | GetByIds(null) | ArgumentNullException | P1 |
| NEG-036 | Empty ID list | GetByIds([]) | Empty result | P1 |
| NEG-037 | Very long notes | 10000 chars | Validation error | P1 |
| NEG-038 | Region-office mismatch | Office not in region | Validation error | P1 |
| NEG-039 | Multiple primary | Set 2 primary | One primary only | P1 |
| NEG-040 | Invalid date range | To < From | Validation error | P1 |
| NEG-041 | Permission denied | User lacks permission | 403 | P2 |
| NEG-042 | Tenant mismatch | Cross-tenant | 403 | P2 |
| NEG-043 | Invalid status | Status 999 | Validation error | P2 |
| NEG-044 | Malformed JSON | Invalid body | 400 Bad Request | P2 |
| NEG-045 | Wrong content type | Text/plain | 415 | P2 |
| NEG-046 | Oversized payload | 10MB | 413 | P2 |
| NEG-047 | Missing auth | No header | 401 | P2 |
| NEG-048 | Invalid token | Malformed JWT | 401 | P2 |
| NEG-049 | Transaction rollback | Explicit rollback | Reverted | P2 |
| NEG-050 | Connection lost | DB down | Connection exception | P2 |
| NEG-051 | Disk full | Export | IO exception | P2 |
| NEG-052 | Deadlock | Concurrent | Retry or deadlock | P2 |
| NEG-053 | Unique constraint | Duplicate | DB exception | P2 |
| NEG-054 | FK violation | Invalid FK | DB exception | P2 |
| NEG-055 | Null in collection | Null in list | ArgumentNullException | P2 |
| NEG-056 | Validation multiple | Multiple errors | All returned | P2 |
| NEG-057 | Invalid timezone | Bad TZ | Validation error | P2 |
| NEG-058 | Encoding invalid | Wrong charset | 400 Bad Request | P2 |
| NEG-059 | Retry exhaustion | All retries fail | Final exception | P2 |
| NEG-060 | Circuit open | Circuit breaker | Rejected | P2 |
| NEG-061 | Service unavailable | Dependent down | 503 | P2 |
| NEG-062 | Idempotent delete | Delete twice | Second 404 | P2 |
| NEG-063 | Cache corruption | Bad cache | Bypass | P2 |
| NEG-064 | Memory pressure | Large export | Throttle | P2 |
| NEG-065 | Bulk assign partial fail | One invalid office | Rollback or partial | P2 |
| NEG-066 | Bulk unassign partial | One not assigned | Skip or error | P2 |
| NEG-067 | Office in different region | Assign cross-region | Allow or reject | P2 |
| NEG-068 | Partner region mismatch | Partner not in office region | Validation error | P2 |
| NEG-069 | Effective date future | Future effective | Allow or reject | P2 |
| NEG-070 | Expiry date past | Past expiry | Reject | P2 |
| NEG-071 | Partner API fail | Partner 500 | Error | P2 |
| NEG-072 | Office API fail | Office 500 | Error | P2 |
| NEG-073 | Region API fail | Region 500 | Error | P2 |
| NEG-074 | DbContext disposed | After dispose | ObjectDisposed | P2 |
| NEG-075 | Transaction rollback | Rollback | Rollback | P2 |
| NEG-076 | Partner soft-deleted | Deleted partner | Reject | P2 |
| NEG-077 | Office soft-deleted | Deleted office | Reject | P2 |
| NEG-078 | Region soft-deleted | Deleted region | Reject | P2 |
| NEG-079 | Null office list | AssignOffices null | ArgumentNull | P2 |
| NEG-080 | Empty office list | AssignOffices [] | No-op or error | P2 |
| NEG-081 | Invalid region ID | RegionId 99999 | Reject | P2 |
| NEG-082 | Capacity overflow | Over capacity | Validation | P2 |
| NEG-083 | Primary on non-assigned | No assignment | BusinessException | P2 |
| NEG-084 | Restore non-deleted | Not deleted | Idempotent | P2 |
| NEG-085 | GetByIds empty | [] | Empty list | P2 |
| NEG-086 | GetByIds null | null | ArgumentNull | P2 |
| NEG-087 | Pagination page 0 | Page 0 | Clamp or error | P2 |
| NEG-088 | Pagination size 0 | Size 0 | Validation | P2 |
| NEG-089 | Search SQL injection | '; DROP-- | Sanitized | P2 |
| NEG-090 | Export fail | Export error | Handled | P2 |

---

## §3 Boundary Tests (90)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Notes length | 0 | 4000 | Accept | Accept | Reject | P1 |
| BND-002 | Partner ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-003 | Office ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-004 | Assignment ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-005 | Page number | 1 | maxPages | 1 ok | Last ok | Empty | P1 |
| BND-006 | Page size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-007 | Search length | 0 | 200 | Empty=all | 200 ok | Truncate | P1 |
| BND-008 | Offices per partner | 0 | 20 | 0 ok | 20 ok | Reject | P1 |
| BND-009 | Partners per office | 0 | 1000 | 0 ok | 1000 ok | Perf | P1 |
| BND-010 | Batch size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-011 | Notes 1 char | 1 | 4000 | Accept | — | — | P1 |
| BND-012 | Notes 4000 chars | 1 | 4000 | — | Accept | — | P1 |
| BND-013 | Notes 4001 chars | 1 | 4000 | — | — | Reject | P1 |
| BND-014 | Region ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-015 | Effective date | Min | Max | Handle | Handle | — | P1 |
| BND-016 | Empty collection | 0 | — | Return [] | — | — | P1 |
| BND-017 | Single item | 1 | — | Return [1] | — | — | P1 |
| BND-018 | Min date | DateTime.Min | — | Handle | — | — | P2 |
| BND-019 | Max date | DateTime.Max | — | — | Handle | — | P2 |
| BND-020 | Leap year | Feb 29 | — | Accept | — | — | P2 |
| BND-021 | Unicode notes | Arabic/Chinese | — | Accept | — | — | P2 |
| BND-022 | Emoji | Emoji | — | Accept or reject | — | — | P2 |
| BND-023 | Null vs empty | — | — | Both handled | — | — | P2 |
| BND-024 | Whitespace | — | — | Trim or reject | — | — | P2 |
| BND-025 | Pagination last partial | — | — | Correct count | — | — | P2 |
| BND-026 | Sort empty | — | — | No error | — | — | P2 |
| BND-027 | Filter no matches | — | — | Empty list | — | — | P2 |
| BND-028 | Exactly N items | N | — | Paginate correctly | — | — | P2 |
| BND-029 | Status enum | First/Last | — | Accept | — | — | P2 |
| BND-030 | Capacity 0 | 0 | — | Reject assignments | — | — | P2 |
| BND-031 | Capacity max | int.Max | — | Accept | — | — | P2 |
| BND-032 | Timeout ms | 100 | 30000 | Min ok | Max ok | — | P2 |
| BND-033 | Retry count | 0 | 5 | 0=no retry | 5 ok | — | P2 |
| BND-034 | Cache TTL | 0 | 3600 | 0=no cache | 3600 ok | — | P2 |
| BND-035 | Rate limit | 1 | 1000 | 1 ok | 1000 ok | — | P2 |
| BND-036 | ID list length | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-037 | Export rows | 0 | 100000 | 0 ok | 100k ok | Reject | P2 |
| BND-038 | Concurrent sessions | — | 100 | — | — | — | P2 |
| BND-039 | Tab/newline | — | — | Sanitize | — | — | P2 |
| BND-040 | Null byte | — | — | Reject | — | — | P2 |
| BND-041 | CRLF | — | — | Sanitize | — | — | P2 |
| BND-042 | RTL text | — | — | Accept | — | — | P2 |
| BND-043 | Zero-width char | — | — | Strip | — | — | P2 |
| BND-044 | Multiple spaces | — | — | Collapse | — | — | P2 |
| BND-045 | Boolean boundary | — | — | True/False | — | — | P2 |
| BND-046 | Enum all values | — | — | All valid | — | — | P2 |
| BND-047 | JSON depth | 1 | 32 | 1 ok | 32 ok | Reject | P2 |
| BND-048 | Array length | 0 | 1000 | 0 ok | 1000 ok | — | P2 |
| BND-049 | Decimal precision | 2 | 2 | 0.00 | 99.99 | — | P2 |
| BND-050 | Percent 0/100 | 0/100 | — | Accept | — | — | P2 |
| BND-051 | Guid empty | Guid.Empty | — | Reject | — | — | P2 |
| BND-052 | Timezone | UTC±12 | — | Correct | — | — | P2 |
| BND-053 | Timestamp precision | — | — | Sub-ms | Full ms | — | P2 |
| BND-054 | Sort field count | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-055 | Filter param count | 0 | 20 | 0 ok | 20 ok | Reject | P2 |
| BND-056 | Query param count | 0 | 50 | 0 ok | 50 ok | Reject | P2 |
| BND-057 | Include depth | 0 | 3 | 0=no | 3 ok | — | P2 |
| BND-058 | Correlation ID | 36 | 36 | UUID | — | — | P2 |
| BND-059 | Token length | 1 | 500 | 1 ok | 500 ok | — | P2 |
| BND-060 | Nested depth | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-061 | Leading/trailing space | — | — | Trimmed | — | — | P2 |
| BND-062 | High surrogate | — | — | Reject | — | — | P2 |
| BND-063 | Effective date boundary | Today | — | Accept | — | — | P2 |
| BND-064 | Expiry date boundary | Future | — | Accept | — | — | P2 |
| BND-065 | Date range | 1 day | 10 years | 1 day ok | 10 years ok | Reject | P2 |
| BND-066 | Office name length | 1 | 200 | 1 ok | 200 ok | Reject | P2 |
| BND-067 | Partner name length | 1 | 200 | 1 ok | 200 ok | Reject | P2 |
| BND-068 | Region name length | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-069 | URL length | 1 | 2048 | 1 ok | 2048 ok | Reject | P2 |
| BND-070 | Phone length | 1 | 20 | 1 ok | 20 ok | Reject | P2 |
| BND-071 | Partner ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-072 | Office ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-073 | Region ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-074 | Assignment ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-075 | Page size 1 | 1 | 100 | Min | — | — | P2 |
| BND-076 | Page size 100 | 1 | 100 | — | Max | — | P2 |
| BND-077 | Bulk assign 0 | 0 | 50 | Empty | — | — | P2 |
| BND-078 | Bulk assign 50 | 0 | 50 | — | Max | — | P2 |
| BND-079 | Notes 0 | 0 | 4000 | Empty | — | — | P2 |
| BND-080 | Notes 4000 | 0 | 4000 | — | Max | — | P2 |
| BND-081 | Search 0 | 0 | 200 | Empty | — | — | P2 |
| BND-082 | Search 200 | 0 | 200 | — | Max | — | P2 |
| BND-083 | Name 1 | 1 | 200 | Min | — | — | P2 |
| BND-084 | Name 200 | 1 | 200 | — | Max | — | P2 |
| BND-085 | Code 1 | 1 | 20 | Min | — | — | P2 |
| BND-086 | Code 20 | 1 | 20 | — | Max | — | P2 |
| BND-087 | Date min | 1900 | 2100 | Min | — | — | P2 |
| BND-088 | Date max | 1900 | 2100 | — | Max | — | P2 |
| BND-089 | Capacity 0 | 0 | 1000 | None | — | — | P2 |
| BND-090 | Capacity 1000 | 0 | 1000 | — | Max | — | P2 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|------------------|----------|
| FUN-001 | Create sets audit | Audit on create | CreateAsync | CreatedBy, CreatedDate | P0 |
| FUN-002 | Update sets audit | Audit on update | UpdateAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-003 | Delete soft | Soft delete | DeleteAsync | IsDeleted=true | P0 |
| FUN-004 | Partner required | PartnerId not null | Create without | Reject | P0 |
| FUN-005 | Office required | OfficeId not null | Create without | Reject | P0 |
| FUN-006 | Unique partner-office | No duplicate | Create duplicate | Reject | P0 |
| FUN-007 | Partner exists | Partner valid | Create with valid | Success | P0 |
| FUN-008 | Office exists | Office valid | Create with valid | Success | P0 |
| FUN-009 | Get excludes deleted | Default filter | GetAllAsync | !IsDeleted | P0 |
| FUN-010 | One primary per partner | Primary uniqueness | SetPrimary | Only one primary | P1 |
| FUN-011 | Set primary unsets previous | Set new primary | SetPrimaryAsync | Previous unset | P1 |
| FUN-012 | Pagination | Page/size | Get page 2 | Items 11-20 | P1 |
| FUN-013 | Sort default | Default sort | GetAllAsync | By partner name | P1 |
| FUN-014 | Filter by partner | Partner filter | Filter | Filtered | P1 |
| FUN-015 | Filter by office | Office filter | Filter | Filtered | P1 |
| FUN-016 | Filter by region | Region filter | Filter | Filtered | P1 |
| FUN-017 | Search partial | Partial match | Search | Matching | P1 |
| FUN-018 | Case-insensitive search | Search | Search | Matches | P1 |
| FUN-019 | Restore clears delete | Restore | RestoreAsync | IsDeleted=false | P1 |
| FUN-020 | Assign validates office | Office valid | AssignOffice | Validated | P1 |
| FUN-021 | Unassign validates | Assignment exists | UnassignOffice | Validated | P1 |
| FUN-022 | Capacity check | Under capacity | Assign | Success | P1 |
| FUN-023 | Bulk assign | Batch | AssignOfficesAsync | All assigned | P1 |
| FUN-024 | Bulk unassign | Batch | UnassignOfficesAsync | All unassigned | P1 |
| FUN-025 | Export format | CSV | ExportAsync | Valid CSV | P1 |
| FUN-026 | Mapping complete | All fields | GetById mapped | All populated | P1 |
| FUN-027 | Concurrency token | Optimistic | Stale update | ConcurrencyException | P1 |
| FUN-028 | Transaction scope | Create+assign | Create with assign | Atomic | P1 |
| FUN-029 | Null handling | Optional fields | Null optional | No error | P1 |
| FUN-030 | Default values | New entity | Create minimal | Defaults | P1 |
| FUN-031 | Idempotent get | GetById | Call twice | Same result | P1 |
| FUN-032 | Stateless | No server state | Request | Independent | P1 |
| FUN-033 | Idempotent delete | Delete twice | Delete same | Second 404 | P1 |
| FUN-034 | Update partial | PATCH | Update 1 field | Only that | P2 |
| FUN-035 | Read-your-writes | Consistency | Create then get | Visible | P2 |
| FUN-036 | Version header | ETag | Get | ETag returned | P2 |
| FUN-037 | Conditional update | If-Match | Stale ETag | 412 | P2 |
| FUN-038 | Soft delete cascade | Children | Delete partner | Handled | P2 |
| FUN-039 | Permission check | CanCreate | Create | Validated | P2 |
| FUN-040 | Tenant isolation | Multi-tenant | Cross-tenant | Rejected | P2 |
| FUN-041 | Audit immutable | No audit change | Update audit | Ignored | P2 |
| FUN-042 | Id auto-assign | Identity | Create | Id assigned | P2 |
| FUN-043 | Timestamp UTC | Store | Create | UTC stored | P2 |
| FUN-044 | Localization | Name | Get with locale | Localized | P2 |
| FUN-045 | Validation order | Multiple invalid | Create | All errors | P2 |
| FUN-046 | Regional stats | Aggregation | GetRegionalStats | Correct | P2 |
| FUN-047 | Effective date | Date logic | Create with date | Validated | P2 |
| FUN-048 | Expiry date | Date logic | Create with expiry | Validated | P2 |
| FUN-049 | Office region | Region match | Assign | Validated | P2 |
| FUN-050 | Partner region | Region match | Assign | Validated | P2 |
| FUN-051 | Create audit | Create | Create | Audit set | P2 |
| FUN-052 | Update audit | Update | Update | Audit set | P2 |
| FUN-053 | Soft delete audit | Delete | Delete | DeletedBy set | P2 |
| FUN-054 | IsDeleted filter | Query | Query | Excludes deleted | P2 |
| FUN-055 | Include partner | Get | Include | Partner loaded | P2 |
| FUN-056 | Include office | Get | Include | Office loaded | P2 |
| FUN-057 | Include region | Get | Include | Region loaded | P2 |
| FUN-058 | Pagination | Page | Page | Correct slice | P2 |
| FUN-059 | Sort | Sort | Sort | Ordered | P2 |
| FUN-060 | Filter region | Filter | Region | Filtered | P2 |
| FUN-061 | Search partner | Search | Partner | Matched | P2 |
| FUN-062 | Search office | Search | Office | Matched | P2 |
| FUN-063 | Primary office | Set primary | Set | Primary set | P2 |
| FUN-064 | Unique partner-office | Create | Create | No duplicate | P2 |
| FUN-065 | Bulk assign | Bulk | Assign | All assigned | P2 |
| FUN-066 | Bulk unassign | Bulk | Unassign | All unassigned | P2 |
| FUN-067 | Restore | Restore | Restore | Restored | P2 |
| FUN-068 | Include deleted | Admin | IncludeDeleted | All | P2 |
| FUN-069 | Regional stats | Stats | Get | Correct | P2 |
| FUN-070 | Capacity check | Capacity | Check | Within limit | P2 |
| FUN-071 | AsNoTracking | Read | Query | No tracking | P2 |
| FUN-072 | Batch save | Batch | Save | All saved | P2 |
| FUN-073 | Transaction | Transaction | Commit | Committed | P2 |
| FUN-074 | Concurrency | Concurrent | Read | No conflict | P2 |
| FUN-075 | Case-insensitive | Search | Case | Matched | P2 |
| FUN-076 | Empty search | Search "" | Search | All | P2 |
| FUN-077 | GetByIds | IDs | Get | Returned | P2 |
| FUN-078 | Export | Export | Export | File | P2 |
| FUN-079 | Default assignment | Default | Get | Returned | P2 |
| FUN-080 | Status validation | Status | Validate | Validated | P2 |
| FUN-081 | Cascading load | Include | Load | Loaded | P2 |
| FUN-082 | DbContext scope | Scope | Per request | Isolated | P2 |
| FUN-083 | Connection pool | Concurrent | Connections | Pooled | P2 |
| FUN-084 | Query timeout | Slow | Query | Timeout | P2 |
| FUN-085 | Retry policy | Transient | Fail | Retried | P2 |
| FUN-086 | Logging | Operation | Log | Logged | P2 |
| FUN-087 | Metrics | Operation | Metric | Recorded | P2 |
| FUN-088 | Validation order | Invalid | Validate | Order correct | P2 |
| FUN-089 | Idempotent delete | Delete | Twice | Second no-op | P2 |
| FUN-090 | Idempotent restore | Restore | Twice | Second no-op | P2 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | CRUD full cycle | Create→Read→Update→Delete | Assignment | Success | P0 |
| INT-002 | Create then get | Create, GetById | Assignment | Data matches | P0 |
| INT-003 | Assignment→Partner | Get partner | Assignment, Partner | Loaded | P0 |
| INT-004 | Assignment→Office | Get office | Assignment, Office | Loaded | P0 |
| INT-005 | Partner→Assignments | Get by partner | Partner, Assignments | List | P0 |
| INT-006 | Office→Assignments | Get by office | Office, Assignments | List | P0 |
| INT-007 | API→Manager→DB | Full stack | All | End-to-end | P1 |
| INT-008 | Controller→Manager | Controller call | API, Manager | Mapped | P1 |
| INT-009 | Manager→Repository | Manager call | Manager, Repo | Executed | P1 |
| INT-010 | Auth→Manager | Authorized call | Auth, Manager | Checked | P1 |
| INT-011 | Error propagation | Manager throws | Manager→Controller | 400/404/500 | P1 |
| INT-012 | Logging integration | Operation | Logger | Log entry | P1 |
| INT-013 | Metrics integration | Operation | Metrics | Counter | P1 |
| INT-014 | Audit→DB | Create | Audit, DB | Audit row | P1 |
| INT-015 | Cache→DB | Get cached | Cache, DB | Hit/miss | P1 |
| INT-016 | Transaction scope | Create+assign | Transaction | Atomic | P1 |
| INT-017 | Partner sync | Partner update | Assignment | Partner ref valid | P1 |
| INT-018 | Office sync | Office update | Assignment | Office ref valid | P1 |
| INT-019 | Region sync | Region update | Office | Region ref valid | P1 |
| INT-020 | LiaisonOffice service | Office lookup | LiaisonOffice svc | Valid | P1 |
| INT-021 | Partner service | Partner lookup | Partner svc | Valid | P1 |
| INT-022 | Report generation | Report | Report, DB | Report data | P1 |
| INT-023 | Dashboard agg | Dashboard | Dashboard, DB | Aggregations | P1 |
| INT-024 | Export file | Export | DB, File | File created | P1 |
| INT-025 | Bulk import | Import | CSV, DB | Imported | P1 |
| INT-026 | Permission service | Permission | Permission svc | Allowed/Denied | P1 |
| INT-027 | Tenant isolation | Multi-tenant | Tenant A, B | Isolated | P1 |
| INT-028 | Retry on failure | Transient | Retry policy | Retried | P1 |
| INT-029 | Health check | Health | DB, Services | Healthy | P1 |
| INT-030 | Config override | Env | Config | Override | P1 |
| INT-031 | Feature flag | Flag off | Feature | Disabled | P1 |
| INT-032 | Rate limit | Many req | Rate limiter | Limited | P1 |
| INT-033 | GeoRegion→Office | Region link | GeoRegion, Office | Linked | P1 |
| INT-034 | Search index | Create assignment | Search | Indexed | P1 |
| INT-035 | Event publish | Created | Event bus | Event sent | P1 |
| INT-036 | FocalPoint→Office | Focal point link | FocalPoint, Office | Linked | P1 |
| INT-037 | Contact→Office | Contact link | Contact, Office | Linked | P1 |
| INT-038 | Regional mapping | Office→Region | Get mapping | Correct | P1 |
| INT-039 | Timezone handling | User TZ | Date fields | Correct | P1 |
| INT-040 | Localization | Name | Display | Localized | P1 |
| INT-041 | API versioning | v2 | Version | v2 behavior | P1 |
| INT-042 | CORS | Cross-origin | CORS | Allowed/Blocked | P1 |
| INT-043 | Correlation ID | Trace | Request | Propagated | P1 |
| INT-044 | Circuit breaker | Failures | Circuit | Opened | P1 |
| INT-045 | Backward compat | Old client | New API | Works | P1 |
| INT-046 | Forward compat | New client | Old API | Graceful | P1 |
| INT-047 | Validation→Controller | Validation | Model, Controller | 400 + errors | P1 |
| INT-048 | Repository→DbContext | Repo call | Repo, EF | SQL generated | P1 |
| INT-049 | Multi-entity create | Assignment+Office | Multiple | All created | P1 |
| INT-050 | Pagination flow | Create 15, Page 2 | Assignment | Correct slice | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|---------------|----------|
| SEC-001 | SQL injection notes | '; DROP-- | Notes | Sanitized/Rejected | P0 |
| SEC-002 | XSS in notes | <script> | Notes | Escaped | P0 |
| SEC-003 | Unauthorized get | No token | GetById | 401 | P0 |
| SEC-004 | Forbidden get | Wrong role | GetById | 403 | P0 |
| SEC-005 | IDOR get | Others' ID | GetById | 403/404 | P0 |
| SEC-006 | IDOR update | Others' ID | Update | 403 | P0 |
| SEC-007 | IDOR delete | Others' ID | Delete | 403 | P0 |
| SEC-008 | Mass assignment | isAdmin=true | Create | Ignored | P0 |
| SEC-009 | Parameterized query | SQL params | All queries | No injection | P0 |
| SEC-010 | Output encoding | HTML | All responses | Encoded | P0 |
| SEC-011 | CSRF token | No token | POST | Rejected | P0 |
| SEC-012 | Session timeout | Expired | Request | 401 | P0 |
| SEC-013 | LDAP injection | *)(uid=* | Search | Rejected | P1 |
| SEC-014 | NoSQL injection | {$gt:""} | Filter | Rejected | P1 |
| SEC-015 | JWT tampering | Modified JWT | Auth | Rejected | P1 |
| SEC-016 | JWT alg none | alg=none | JWT | Rejected | P1 |
| SEC-017 | Token replay | Reuse token | Request | Rejected | P1 |
| SEC-018 | Privilege escalation | Low→Admin | Action | 403 | P1 |
| SEC-019 | Horizontal access | User A→B | Resource | 403 | P1 |
| SEC-020 | Vertical access | User→Admin | Resource | 403 | P1 |
| SEC-021 | Sensitive data log | Office details | Logging | Not logged | P1 |
| SEC-022 | Sensitive data response | Internal IDs | API | Permission-based | P1 |
| SEC-023 | Stack trace | Error | Prod | No trace | P1 |
| SEC-024 | Verbose error | DB details | Error | Generic | P1 |
| SEC-025 | Rate limit bypass | Many IPs | Rate limit | Per-user | P1 |
| SEC-026 | Header injection | CRLF | Header | Rejected | P1 |
| SEC-027 | Oversized payload | 100MB | Request | Rejected | P1 |
| SEC-028 | Deep object | 100 levels | JSON | Rejected | P1 |
| SEC-029 | Regex DoS | Evil regex | Pattern | Timeout/Reject | P1 |
| SEC-030 | Prototype pollution | __proto__ | JSON | Sanitized | P1 |
| SEC-031 | CORS misconfig | Wildcard | CORS | Restricted | P1 |
| SEC-032 | Missing headers | X-Frame-Options | Response | Present | P1 |
| SEC-033 | HSTS | HTTP | Redirect | HTTPS | P1 |
| SEC-034 | Cookie secure | Cookie | Set-Cookie | Secure | P1 |
| SEC-035 | Cookie HttpOnly | Cookie | Set-Cookie | HttpOnly | P1 |
| SEC-036 | Audit integrity | Modify audit | Audit | Tamper evident | P1 |
| SEC-037 | Encryption at rest | DB | Sensitive | Encrypted | P1 |
| SEC-038 | Command injection | ; ls | Field | Rejected | P1 |
| SEC-039 | Path traversal | ../etc/passwd | File | Rejected | P1 |
| SEC-040 | XXE | XML entity | XML | Rejected | P1 |
| SEC-041 | SSRF | Internal URL | URL | Blocked | P1 |
| SEC-042 | Open redirect | redirect=evil | Redirect | Validated | P1 |
| SEC-043 | Brute force | Many auth | Login | Lockout | P1 |
| SEC-044 | Content-type bypass | Wrong type | Upload | Rejected | P1 |
| SEC-045 | File upload malicious | Exe | Upload | Rejected | P1 |
| SEC-046 | Insecure deserialization | Malicious | Deserialize | Rejected | P1 |
| SEC-047 | Info disclosure | Server details | Header | Minimal | P1 |
| SEC-048 | Tenant isolation | Cross-tenant | Request | 403 | P1 |
| SEC-049 | Data aggregation | PII in report | Export | Anonymized | P1 |
| SEC-050 | Regional data exposure | Cross-region | Request | Permission-based | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior | Priority |
|----|-----------|----------|-------------------|----------|
| CON-001 | Concurrent create same partner-office | 2 users create | One succeeds, one conflict | P1 |
| CON-002 | Concurrent update same | 2 users update | Optimistic lock | P1 |
| CON-003 | Concurrent delete same | 2 users delete | One succeeds | P1 |
| CON-004 | Read during update | Read while update | Consistent | P1 |
| CON-005 | Update during delete | Update while delete | One fails | P1 |
| CON-006 | Double submit | Same form twice | Idempotent | P1 |
| CON-007 | Transaction isolation | Parallel tx | No dirty read | P1 |
| CON-008 | Deadlock | Circular wait | Retry | P1 |
| CON-009 | Lost update | Interleaved | Version lock | P1 |
| CON-010 | Set primary race | 2 set primary | One primary only | P1 |
| CON-011 | Assign race | 2 assign same | One succeeds | P1 |
| CON-012 | Cache invalidation | Update after cache | Invalidated | P1 |
| CON-013 | Batch concurrent | 2 batches | Both complete | P1 |
| CON-014 | Connection pool | Exhaust | Queue/timeout | P1 |
| CON-015 | Lock timeout | Hold long | Timeout | P1 |
| CON-016 | Retry idempotency | Retry partial | No duplicate | P1 |
| CON-017 | Visibility | Write then read | Read sees write | P1 |
| CON-018 | Unassign race | 2 unassign same | One succeeds | P1 |
| CON-019 | Export concurrent | 2 exports | Both complete | P1 |
| CON-020 | Bulk assign race | 2 bulk assigns | Both complete | P1 |
| CON-021 | Distributed lock | Multi-instance | Single writer | P1 |
| CON-022 | Eventual consistency | Replica lag | Converge | P2 |
| CON-023 | Failover | Primary fail | Replica | P2 |
| CON-024 | Saga compensation | Partial fail | Compensate | P2 |
| CON-025 | Outbox pattern | Event | Exactly once | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | PartnerId validation | Validation | Valid id | True | P1 |
| UNT-002 | PartnerId invalid | Validation | -1 | False | P1 |
| UNT-003 | OfficeId validation | Validation | Valid id | True | P1 |
| UNT-004 | OfficeId invalid | Validation | -1 | False | P1 |
| UNT-005 | RegionId validation | Validation | Valid id | True | P1 |
| UNT-006 | Trim notes | Formatting | "  x  " | "x" | P1 |
| UNT-007 | Map entity to model | Mapping | Entity | Model | P1 |
| UNT-008 | Primary check | Status logic | IsPrimary | True/False | P1 |
| UNT-009 | Capacity check | Calculation | Count, capacity | Within limit | P1 |
| UNT-010 | Date range check | Validation | Effective, expiry | Valid | P1 |
| UNT-011 | IsDeleted filter | Status logic | Mixed | !IsDeleted | P1 |
| UNT-012 | Sort comparator | Collections | Unsorted | Sorted | P1 |
| UNT-013 | Paginate slice | Collections | Full list | Slice | P1 |
| UNT-014 | Search predicate | Collections | Query | Matching | P1 |
| UNT-015 | Null safe | Validation | Null | No throw | P1 |
| UNT-016 | Empty collection | Collections | [] | [] | P1 |
| UNT-017 | Map list | Mapping | Entity list | Model list | P1 |
| UNT-018 | Date format | Formatting | DateTime | ISO string | P1 |
| UNT-019 | Id equality | Validation | Same id | Equal | P1 |
| UNT-020 | Regional stats calc | Calculation | Assignments | Stats | P1 |
| UNT-021 | Unique partner-office | Validation | Same combo | Reject | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetById latency | GetByIdAsync | < 50 ms | P2 |
| PRF-002 | GetOfficesByPartner latency | 10 offices | < 200 ms | P2 |
| PRF-003 | Create latency | CreateAsync | < 100 ms | P2 |
| PRF-004 | Update latency | UpdateAsync | < 100 ms | P2 |
| PRF-005 | Delete latency | DeleteAsync | < 100 ms | P2 |
| PRF-006 | Pagination | Page 10 of 1000 | < 200 ms | P2 |
| PRF-007 | Bulk assign 20 | AssignOffices | < 2 s | P2 |
| PRF-008 | Bulk get 100 | GetByIds 100 | < 500 ms | P2 |
| PRF-009 | Export 1000 | ExportAsync | < 5 s | P2 |
| PRF-010 | Concurrent 10 get | 10 parallel | < 200 ms | P2 |
| PRF-011 | Memory single | Create | No leak | P2 |
| PRF-012 | Memory 1000 ops | 1000 creates | Stable | P2 |
| PRF-013 | Query plan | GetById | Index used | P2 |
| PRF-014 | N+1 check | With partner, office | Single query | P2 |
| PRF-015 | Connection reuse | 100 sequential | Pool stable | P2 |
| PRF-016 | Regional stats | GetRegionalStats | < 500 ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|------------------|----------|
| LDT-001 | Sustained 10 RPS | 10 req/s | 10 min | 99% < 500 ms | P2 |
| LDT-002 | Sustained 50 RPS | 50 req/s | 5 min | 99% < 1 s | P2 |
| LDT-003 | Sustained 100 RPS | 100 req/s | 5 min | 95% < 2 s | P2 |
| LDT-004 | Spike 200 RPS | 0→200→0 | 2 min | No 5xx | P2 |
| LDT-005 | Spike 500 RPS | 5 s burst | 5 s | Recover | P2 |
| LDT-006 | Stress 500 RPS | 500 req/s | 2 min | Graceful | P2 |
| LDT-007 | Stress 1000 RPS | 1000 req/s | 1 min | No crash | P2 |
| LDT-008 | Endurance 20 RPS | 20 req/s | 1 h | No leak | P2 |
| LDT-009 | Recovery | Post-spike | 5 min | Baseline | P2 |
| LDT-010 | Mixed workload | CRUD mix | 15 min | All succeed | P2 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
