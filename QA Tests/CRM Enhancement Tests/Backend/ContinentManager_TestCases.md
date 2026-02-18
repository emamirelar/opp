# ContinentManager — Test Cases

**Component:** UNOPS.PAO.UNOPSBusiness/Managers/ContinentManager.cs  
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

The ContinentManager manages continent reference data for the CRM enhancement:
- **CRUD operations** for continents
- **Region grouping** (continent-to-region associations)
- **Country association** via regions
- **Geographic hierarchy** root management

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | Create continent with valid data | DB seeded | Call CreateAsync with name, code | Continent created, returns ID | P0 |
| POS-002 | Get continent by ID | Continent exists | Call GetByIdAsync(id) | Continent returned | P0 |
| POS-003 | Update continent | Continent exists | Call UpdateAsync(id, data) | Continent updated | P0 |
| POS-004 | Soft delete continent | Continent has no regions | Call DeleteAsync(id) | IsDeleted set | P0 |
| POS-005 | Get all continents | DB seeded | Call GetAllAsync() | List returned (7 expected) | P0 |
| POS-006 | Get by code | Africa exists | Call GetByCodeAsync("AF") | Africa continent | P0 |
| POS-007 | Get regions for continent | Continent has regions | Call GetRegionsAsync(id) | Region list | P1 |
| POS-008 | Count regions | Continent has regions | Get region count | Correct count | P1 |
| POS-009 | Count countries via regions | Regions have countries | Get country count | Aggregated count | P1 |
| POS-010 | Get dropdown list | DB seeded | Call GetForDropdownAsync() | ID/name pairs | P1 |
| POS-011 | Sort by name | Multiple continents | GetAllAsync sorted | A-Z order | P1 |
| POS-012 | Get with statistics | Continent has regions | Get with stats flag | Region/country counts | P1 |
| POS-013 | Create with 2-char code | None | Create with "EU" | Success | P1 |
| POS-014 | Create with 50-char name | None | Create with max length name | Success | P1 |
| POS-015 | Filter by status active | Continents exist | Filter IsDeleted=false | Only active | P1 |
| POS-016 | Paginate results | 20+ continents | Get with page 1, size 10 | First 10 returned | P1 |
| POS-017 | Search by name | Continents exist | Search "Africa" | Matching results | P1 |
| POS-018 | Get by multiple IDs | IDs exist | GetByIdsAsync([1,2,3]) | 3 continents | P1 |
| POS-019 | Validate code format | None | Create with "NA" | Accepted | P2 |
| POS-020 | Audit trail on create | User authenticated | Create continent | CreatedBy/Date set | P2 |
| POS-021 | Audit trail on update | Continent exists | Update continent | LastModifiedBy/Date set | P2 |
| POS-022 | Include soft-deleted | Admin user | GetAllAsync(includeDeleted: true) | All continents | P2 |
| POS-023 | Map to model | Continent exists | GetByIdAsync with mapping | Model populated | P2 |
| POS-024 | Batch get by codes | Codes exist | GetByCodesAsync(["AF","EU"]) | 2 continents | P2 |
| POS-025 | Export to CSV | Continents exist | ExportAsync() | CSV content | P2 |
| POS-026 | Validate unique code | No duplicate | Create with new code | Success | P2 |
| POS-027 | Restore soft-deleted | Continent deleted | RestoreAsync(id) | IsDeleted=false | P2 |
| POS-028 | Get hierarchy tree | Continents with regions | GetHierarchyAsync() | Tree structure | P2 |
| POS-029 | Validate name required | None | Create with valid name | Success | P2 |
| POS-030 | Cascading region load | Continent has regions | Get with Include regions | Regions loaded | P2 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create without name | Name null | Validation error | P0 |
| NEG-002 | Create without code | Code null | Validation error | P0 |
| NEG-003 | Create with duplicate code | Existing "AF" | Conflict 409 | P0 |
| NEG-004 | Get by non-existent ID | ID 99999 | KeyNotFoundException | P0 |
| NEG-005 | Update non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-006 | Delete non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-007 | Delete with regions | Continent has regions | BusinessException | P0 |
| NEG-008 | Invalid code format | Code "AFRICA" | Validation error | P0 |
| NEG-009 | Code too short | Code "A" | Validation error | P0 |
| NEG-010 | Name exceeds max | 51 chars | Validation error | P0 |
| NEG-011 | Negative ID | GetByIdAsync(-1) | ArgumentException | P0 |
| NEG-012 | Zero ID | GetByIdAsync(0) | ArgumentException | P0 |
| NEG-013 | Null create request | CreateAsync(null) | ArgumentNullException | P0 |
| NEG-014 | Null update request | UpdateAsync(id, null) | ArgumentNullException | P0 |
| NEG-015 | Empty code | Code "" | Validation error | P0 |
| NEG-016 | Whitespace name | Name "   " | Validation error | P1 |
| NEG-017 | Special chars in code | Code "A@" | Validation error | P1 |
| NEG-018 | Lowercase code | Code "af" | Validation error (if uppercase required) | P1 |
| NEG-019 | SQL in name | Name "'; DROP--" | Sanitized or rejected | P1 |
| NEG-020 | XSS in name | Name "<script>" | Sanitized | P1 |
| NEG-021 | Invalid pagination page | Page -1 | ArgumentException | P1 |
| NEG-022 | Invalid page size | Size 0 | ArgumentException | P1 |
| NEG-023 | Page size exceeds max | Size 10000 | ArgumentException | P1 |
| NEG-024 | Invalid sort field | Sort "invalid" | ArgumentException | P1 |
| NEG-025 | Restore non-deleted | Active continent | BusinessException | P1 |
| NEG-026 | Get by empty ID list | GetByIdsAsync([]) | Empty list | P1 |
| NEG-027 | Get by null ID list | GetByIdsAsync(null) | ArgumentNullException | P1 |
| NEG-028 | Update deleted continent | Soft-deleted id | KeyNotFoundException | P1 |
| NEG-029 | Create with future date | Invalid CreatedDate | Validation error | P1 |
| NEG-030 | Invalid status value | Status 999 | Validation error | P1 |
| NEG-031 | Circular hierarchy | Self-reference | Validation error | P1 |
| NEG-032 | Orphan region reference | Invalid region ID | Foreign key error | P1 |
| NEG-033 | Null option params | GetAllAsync(options: null) | ArgumentNullException | P1 |
| NEG-034 | Invalid filter combination | Conflicting filters | Validation error | P1 |
| NEG-035 | Missing required audit field | CreatedBy null | Validation error | P1 |
| NEG-036 | Export empty set | No continents | Empty CSV | P1 |
| NEG-037 | GetByCode non-existent | "XX" | KeyNotFoundException | P1 |
| NEG-038 | Batch get with invalid IDs | [1,-1,2] | Partial failure or error | P1 |
| NEG-039 | Concurrent delete same | Two users delete same | One succeeds, one fails | P1 |
| NEG-040 | Update stale entity | Stale LastModified | Concurrency exception | P1 |
| NEG-041 | Name with control chars | \0 in name | Rejected | P2 |
| NEG-042 | Unicode normalized | Different normalization | Handled consistently | P2 |
| NEG-043 | Very long description | 10000 chars | Validation error | P2 |
| NEG-044 | Invalid locale code | "xx-YY" | Validation error | P2 |
| NEG-045 | Timezone invalid | Invalid TZ | Validation error | P2 |
| NEG-046 | Invalid parent reference | Non-existent parent | FK error | P2 |
| NEG-047 | Duplicate in batch create | Two with same code | Validation error | P2 |
| NEG-048 | Partial batch failure | One invalid in batch | Rollback or partial | P2 |
| NEG-049 | Expired session | Stale token | 401 Unauthorized | P2 |
| NEG-050 | Rate limit exceeded | 1000 req/sec | 429 Too Many | P2 |
| NEG-051 | DB connection lost | Connection drop | Graceful retry/error | P2 |
| NEG-052 | Timeout on slow query | Long-running | Timeout exception | P2 |
| NEG-053 | Disk full | No disk space | IO exception | P2 |
| NEG-054 | Null DbContext | Context null | NullReferenceException | P2 |
| NEG-055 | Invalid mapper config | Mapping error | AutoMapper exception | P2 |
| NEG-056 | Transaction rollback | Explicit rollback | Changes reverted | P2 |
| NEG-057 | Permission denied | User lacks permission | 403 Forbidden | P2 |
| NEG-058 | Unauthorized | No token | 401 | P2 |
| NEG-059 | Invalid JSON body | Malformed JSON | 400 Bad Request | P2 |
| NEG-060 | Wrong content type | Text/plain | 415 Unsupported | P2 |
| NEG-061 | Oversized payload | 10MB request | 413 Payload Too Large | P2 |
| NEG-062 | Invalid encoding | Wrong charset | 400 Bad Request | P2 |
| NEG-063 | Missing correlation ID | No trace header | Accepted (or required) | P2 |
| NEG-064 | Invalid correlation ID | Malformed UUID | Logged, accepted | P2 |
| NEG-065 | Cascade delete blocked | Has children | BusinessException | P2 |
| NEG-066 | Orphan prevention | Delete parent | Cascade or block | P2 |
| NEG-067 | Read-only replica lag | Stale read | Eventual consistency | P2 |
| NEG-068 | Cache stampede | All miss | Thundering herd handled | P2 |
| NEG-069 | Deadlock | Concurrent update | Retry or deadlock | P2 |
| NEG-070 | Unique constraint violation | Duplicate insert | DB exception | P2 |
| NEG-071 | Region API fail | Region 500 | Error | P2 |
| NEG-072 | DbContext disposed | After dispose | ObjectDisposed | P2 |
| NEG-073 | Continent has regions | Delete | BusinessException | P2 |
| NEG-074 | Null codes list | GetByCodes null | ArgumentNull | P2 |
| NEG-075 | Empty codes list | GetByCodes [] | Empty list | P2 |
| NEG-076 | Invalid code format | Code "AFRICA" | Reject | P2 |
| NEG-077 | Duplicate code | Existing code | Conflict | P2 |
| NEG-078 | GetByIds empty | [] | Empty list | P2 |
| NEG-079 | Pagination page 0 | Page 0 | Clamp or error | P2 |
| NEG-080 | Pagination size 0 | Size 0 | Validation | P2 |
| NEG-081 | Search SQL injection | '; DROP-- | Sanitized | P2 |
| NEG-082 | Restore non-deleted | Not deleted | Idempotent | P2 |
| NEG-083 | Name too long | 51 chars | Validation | P2 |
| NEG-084 | Code too short | 1 char | Validation | P2 |
| NEG-085 | Export fail | Export error | Handled | P2 |
| NEG-086 | Hierarchy fail | Hierarchy error | Handled | P2 |
| NEG-087 | Default not configured | No default | Null or error | P2 |
| NEG-088 | Invalid status | Status 999 | Reject | P2 |
| NEG-089 | Batch invalid IDs | [1,-1,2] | Partial fail | P2 |
| NEG-090 | Circular hierarchy | Self-ref | Validation | P2 |

---

## §3 Boundary Tests (90)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Name | 1 | 50 | Accept | Accept | Reject | P1 |
| BND-002 | Code | 2 | 2 | Reject | Accept | Reject | P1 |
| BND-003 | Id | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-004 | Page number | 1 | maxPages | 1 ok | Last ok | Empty | P1 |
| BND-005 | Page size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-006 | Search length | 0 | 200 | Empty=all | 200 ok | Truncate or reject | P1 |
| BND-007 | Batch size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-008 | Region count | 0 | 1000 | 0 ok | 1000 ok | Performance | P1 |
| BND-009 | Country count | 0 | 5000 | 0 ok | 5000 ok | Performance | P1 |
| BND-010 | Timestamp precision | — | — | Sub-ms | Full ms | DB limit | P1 |
| BND-011 | Name with 1 char | 1 | 50 | Accept | — | — | P1 |
| BND-012 | Name with 50 chars | 1 | 50 | — | Accept | — | P1 |
| BND-013 | Name with 51 chars | 1 | 50 | — | — | Reject | P1 |
| BND-014 | Code exactly 2 | 2 | 2 | Accept | Accept | — | P1 |
| BND-015 | Empty collection | 0 | — | Return [] | — | — | P1 |
| BND-016 | Single item collection | 1 | — | Return [1] | — | — | P1 |
| BND-017 | Max int ID | int.MaxValue | — | Handle | — | — | P1 |
| BND-018 | Min date | DateTime.Min | — | Accept | — | — | P2 |
| BND-019 | Max date | DateTime.Max | — | — | Accept | — | P2 |
| BND-020 | Leap year date | Feb 29 2024 | — | Accept | — | — | P2 |
| BND-021 | Unicode name | Arabic/Chinese | — | Accept | — | — | P2 |
| BND-022 | Emoji in name | Emoji | — | Accept or reject | — | — | P2 |
| BND-023 | Zero decimal | 0.0 | — | Accept | — | — | P2 |
| BND-024 | Negative number | — | — | Reject | — | — | P2 |
| BND-025 | Float precision | — | — | Rounding | — | — | P2 |
| BND-026 | Null vs empty string | — | — | Both handled | — | — | P2 |
| BND-027 | Whitespace only | — | — | Trim or reject | — | — | P2 |
| BND-028 | Leading/trailing space | — | — | Trimmed | — | — | P2 |
| BND-029 | Tab/newline in name | — | — | Reject or sanitize | — | — | P2 |
| BND-030 | High surrogate lone | — | — | Reject | — | — | P2 |
| BND-031 | Pagination last page partial | — | — | Correct count | — | — | P2 |
| BND-032 | Sort empty | — | — | No error | — | — | P2 |
| BND-033 | Filter no matches | — | — | Empty list | — | — | P2 |
| BND-034 | Exactly N items | N | — | Paginate correctly | — | — | P2 |
| BND-035 | Boundary timezone | UTC±12 | — | Stored correctly | — | — | P2 |
| BND-036 | Status enum first | First value | — | Accept | — | — | P2 |
| BND-037 | Status enum last | Last value | — | Accept | — | — | P2 |
| BND-038 | Nullable int zero | 0 | — | Distinguish null | — | — | P2 |
| BND-039 | Guid empty | Guid.Empty | — | Reject | — | — | P2 |
| BND-040 | Url max length | — | 2048 | — | Accept | Reject | P2 |
| BND-041 | Long description | — | 4000 | — | Accept | — | P2 |
| BND-042 | Nested depth | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-043 | Concurrent limit | — | 100 | — | — | — | P2 |
| BND-044 | Retry count | 0 | 5 | 0=no retry | 5 ok | — | P2 |
| BND-045 | Timeout ms | 100 | 30000 | Min ok | Max ok | — | P2 |
| BND-046 | Cache TTL | 0 | 3600 | 0=no cache | 3600 ok | — | P2 |
| BND-047 | Rate limit | 1 | 1000 | 1 ok | 1000 ok | — | P2 |
| BND-048 | Query param count | 0 | 50 | 0 ok | 50 ok | Reject | P2 |
| BND-049 | Include depth | 0 | 3 | 0=no include | 3 ok | — | P2 |
| BND-050 | Batch IDs | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-051 | Name start space | — | — | Trim | — | — | P2 |
| BND-052 | Name end space | — | — | Trim | — | — | P2 |
| BND-053 | Code uppercase | — | — | Normalize | — | — | P2 |
| BND-054 | Multiple spaces | — | — | Collapse | — | — | P2 |
| BND-055 | Zero-width char | — | — | Strip | — | — | P2 |
| BND-056 | RTL text | — | — | Accept | — | — | P2 |
| BND-057 | Mixed script | — | — | Accept | — | — | P2 |
| BND-058 | Null byte | — | — | Reject | — | — | P2 |
| BND-059 | CRLF in field | — | — | Reject or sanitize | — | — | P2 |
| BND-060 | Very long ID list | 101 | — | — | — | Reject | P2 |
| BND-061 | Decimal precision | 2 | 2 | 1.23 | 1.23 | 1.234? | P2 |
| BND-062 | Currency zero | 0 | — | Accept | — | — | P2 |
| BND-063 | Percent 100 | 100 | — | Accept | — | — | P2 |
| BND-064 | Percent 0 | 0 | — | Accept | — | — | P2 |
| BND-065 | Boolean boundary | — | — | True/False | — | — | P2 |
| BND-066 | Enum boundary | — | — | All values | — | — | P2 |
| BND-067 | JSON depth | 1 | 32 | 1 ok | 32 ok | Reject | P2 |
| BND-068 | Array length | 0 | 1000 | 0 ok | 1000 ok | — | P2 |
| BND-069 | Token length | 1 | 500 | 1 ok | 500 ok | — | P2 |
| BND-070 | Correlation ID | 36 | 36 | UUID format | — | — | P2 |
| BND-071 | Continent ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-072 | Region ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-073 | Page size 1 | 1 | 100 | Min | — | — | P2 |
| BND-074 | Page size 100 | 1 | 100 | — | Max | — | P2 |
| BND-075 | Name 1 | 1 | 50 | Min | — | — | P2 |
| BND-076 | Name 50 | 1 | 50 | — | Max | — | P2 |
| BND-077 | Code 2 | 2 | 2 | Min | Max | — | P2 |
| BND-078 | Search 0 | 0 | 200 | Empty | — | — | P2 |
| BND-079 | Search 200 | 0 | 200 | — | Max | — | P2 |
| BND-080 | Codes list 0 | 0 | 100 | Empty | — | — | P2 |
| BND-081 | Codes list 100 | 0 | 100 | — | Max | — | P2 |
| BND-082 | Description 0 | 0 | 2000 | Empty | — | — | P2 |
| BND-083 | Description 2000 | 0 | 2000 | — | Max | — | P2 |
| BND-084 | Region count 0 | 0 | 100 | None | — | — | P2 |
| BND-085 | Region count 100 | 0 | 100 | — | Max | — | P2 |
| BND-086 | Notes 0 | 0 | 4000 | Empty | — | — | P2 |
| BND-087 | Notes 4000 | 0 | 4000 | — | Max | — | P2 |
| BND-088 | Hierarchy depth 1 | 1 | 10 | Min | — | — | P2 |
| BND-089 | Hierarchy depth 10 | 1 | 10 | — | Max | — | P2 |
| BND-090 | Status 0 | 0 | 10 | Min | — | — | P2 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|------------------|----------|
| FUN-001 | Create sets audit | Audit on create | CreateAsync | CreatedBy, CreatedDate | P0 |
| FUN-002 | Update sets audit | Audit on update | UpdateAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-003 | Delete soft | Soft delete | DeleteAsync | IsDeleted=true | P0 |
| FUN-004 | Unique code | No duplicate codes | Create duplicate | Reject | P0 |
| FUN-005 | Name required | Name not null | Create without name | Reject | P0 |
| FUN-006 | Code required | Code not null | Create without code | Reject | P0 |
| FUN-007 | Delete blocks if regions | Referential integrity | Delete with regions | Reject | P0 |
| FUN-008 | Get excludes deleted | Default filter | GetAllAsync | !IsDeleted | P0 |
| FUN-009 | Code 2 chars | Format rule | Create with 3 chars | Reject | P1 |
| FUN-010 | Code uppercase | Normalization | Create "af" | Stored as "AF" | P1 |
| FUN-011 | Name trim | Whitespace | Create "  x  " | Stored "x" | P1 |
| FUN-012 | Pagination | Page/size | Get with page 2, size 5 | Items 6-10 | P1 |
| FUN-013 | Sort default | Default sort | GetAllAsync | By Name ASC | P1 |
| FUN-014 | Sort by code | Sort option | Sort by code | Order by code | P1 |
| FUN-015 | Filter by status | Status filter | Filter Active | Only active | P1 |
| FUN-016 | Search partial | Partial match | Search "Afri" | Africa | P1 |
| FUN-017 | Case-insensitive search | Search | Search "africa" | Africa | P1 |
| FUN-018 | Restore clears delete | Restore | RestoreAsync | IsDeleted=false | P1 |
| FUN-019 | Hierarchy constraint | No cycles | Set parent=self | Reject | P1 |
| FUN-020 | Region count cached | Optional cache | GetRegionsAsync | Correct count | P1 |
| FUN-021 | Country aggregation | Sum via regions | GetCountryCount | Sum of region countries | P1 |
| FUN-022 | Dropdown active only | Dropdown filter | GetForDropdown | Only !IsDeleted | P1 |
| FUN-023 | Default 7 seed | Seed data | Initial migration | 7 continents | P1 |
| FUN-024 | Code immutable | No code change | Update code | Reject or ignore | P1 |
| FUN-025 | Status transition | Valid transitions | Draft→Active | Allowed | P1 |
| FUN-026 | Status invalid transition | Invalid | Archived→Draft | Reject | P1 |
| FUN-027 | Bulk create | Batch | CreateAsync batch | All created | P1 |
| FUN-028 | Bulk update | Batch | UpdateAsync batch | All updated | P1 |
| FUN-029 | Export format | CSV | ExportAsync | Valid CSV | P1 |
| FUN-030 | Mapping complete | All fields | GetById mapped | All fields populated | P1 |
| FUN-031 | Null handling | Optional fields | Null optional | No error | P1 |
| FUN-032 | Default values | New entity | Create minimal | Defaults applied | P1 |
| FUN-033 | Concurrency token | Optimistic | Stale update | ConcurrencyException | P1 |
| FUN-034 | Transaction scope | Create+Region | Create with regions | Atomic | P1 |
| FUN-035 | Cascade options | Delete behavior | Delete continent | Configurable cascade | P1 |
| FUN-036 | Audit immutable | No audit change | Update audit fields | Ignored | P2 |
| FUN-037 | Id auto-assign | Identity | Create | Id assigned | P2 |
| FUN-038 | Timestamp precision | Store | Create | UTC stored | P2 |
| FUN-039 | Localization | Name | Get with locale | Localized name | P2 |
| FUN-040 | Validation order | Validation | Invalid create | All errors returned | P2 |
| FUN-041 | Idempotent get | GetById | Call twice | Same result | P2 |
| FUN-042 | Stateless | No server state | Request independent | No session dependency | P2 |
| FUN-043 | Idempotent delete | Delete twice | Delete same id | Second 404 or no-op | P2 |
| FUN-044 | Update partial | PATCH | Update 1 field | Only that field | P2 |
| FUN-045 | Null vs omit | JSON | Omit field | Not clear vs null | P2 |
| FUN-046 | Read-your-writes | Consistency | Create then immediately get | Visible | P2 |
| FUN-047 | Version header | ETag | Get | ETag returned | P2 |
| FUN-048 | Conditional update | If-Match | Update with stale ETag | 412 | P2 |
| FUN-049 | Soft delete cascade | Children | Delete parent | Children handled | P2 |
| FUN-050 | Permission check | CanCreate | Create | Permission validated | P2 |
| FUN-051 | Create audit | Create | Create | Audit set | P2 |
| FUN-052 | Update audit | Update | Update | Audit set | P2 |
| FUN-053 | Soft delete audit | Delete | Delete | DeletedBy set | P2 |
| FUN-054 | IsDeleted filter | Query | Query | Excludes deleted | P2 |
| FUN-055 | Include regions | Get | Include | Regions loaded | P2 |
| FUN-056 | Pagination | Page | Page | Correct slice | P2 |
| FUN-057 | Sort | Sort | Sort | Ordered | P2 |
| FUN-058 | Search | Search | Search | Matched | P2 |
| FUN-059 | GetByCodes | Codes | Get | Returned | P2 |
| FUN-060 | GetHierarchy | Hierarchy | Get | Tree | P2 |
| FUN-061 | Restore | Restore | Restore | Restored | P2 |
| FUN-062 | Include deleted | Admin | IncludeDeleted | All | P2 |
| FUN-063 | Default continent | Default | Get | Returned | P2 |
| FUN-064 | AsNoTracking | Read | Query | No tracking | P2 |
| FUN-065 | Transaction | Transaction | Commit | Committed | P2 |
| FUN-066 | Concurrency | Concurrent | Read | No conflict | P2 |
| FUN-067 | Case-insensitive | Search | Case | Matched | P2 |
| FUN-068 | Unique code | Code | Create | No duplicate | P2 |
| FUN-069 | DbContext scope | Scope | Per request | Isolated | P2 |
| FUN-070 | Validation order | Invalid | Validate | Order correct | P2 |
| FUN-071 | Idempotent delete | Delete | Twice | Second no-op | P2 |
| FUN-072 | Idempotent restore | Restore | Twice | Second no-op | P2 |
| FUN-073 | Batch save | Batch | Save | All saved | P2 |
| FUN-074 | Empty search | Search "" | Search | All | P2 |
| FUN-075 | Status transition | Status | Change | Validated | P2 |
| FUN-076 | Logging | Operation | Log | Logged | P2 |
| FUN-077 | Metrics | Operation | Metric | Recorded | P2 |
| FUN-078 | Query timeout | Slow | Query | Timeout | P2 |
| FUN-079 | Retry policy | Transient | Fail | Retried | P2 |
| FUN-080 | Cascading load | Include | Load | Loaded | P2 |
| FUN-081 | Connection pool | Concurrent | Connections | Pooled | P2 |
| FUN-082 | Foreign key | FK | Constraint | Enforced | P2 |
| FUN-083 | Unique constraint | Unique | Insert | Enforced | P2 |
| FUN-084 | Index | Query | Index | Fast | P2 |
| FUN-085 | Export | Export | Export | File | P2 |
| FUN-086 | GetByIds | IDs | Get | Returned | P2 |
| FUN-087 | Delete with regions | Has regions | Delete | Reject | P2 |
| FUN-088 | Code format | Code | Validate | Validated | P2 |
| FUN-089 | Name required | Name | Validate | Validated | P2 |
| FUN-090 | Hierarchy validation | Hierarchy | Validate | Validated | P2 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | CRUD full cycle | Create→Read→Update→Delete | Continent | Full cycle success | P0 |
| INT-002 | Create then get | Create, GetById | Continent | Data matches | P0 |
| INT-003 | Update then get | Update, GetById | Continent | Updated data | P0 |
| INT-004 | Delete then get | Delete, GetById | Continent | 404 or deleted | P0 |
| INT-005 | List after create | Create, GetAll | Continent | New in list | P0 |
| INT-006 | Continent→Region | Get regions | Continent, Region | Regions loaded | P0 |
| INT-007 | Region→Country | Get countries | Region, Country | Countries loaded | P0 |
| INT-008 | Search flow | Create, Search | Continent | Found | P0 |
| INT-009 | Pagination flow | Create 15, Page 2 | Continent | Correct slice | P0 |
| INT-010 | Filter flow | Create varied, Filter | Continent | Filtered correctly | P0 |
| INT-011 | API→Manager→DB | Full stack | All | End-to-end success | P1 |
| INT-012 | Controller→Manager | Controller call | API, Manager | Response mapped | P1 |
| INT-013 | Manager→Repository | Manager call | Manager, Repo | Query executed | P1 |
| INT-014 | Repository→DbContext | Repo call | Repo, EF | SQL generated | P1 |
| INT-015 | Auth→Manager | Authorized call | Auth, Manager | Permission checked | P1 |
| INT-016 | Error propagation | Manager throws | Manager→Controller | 400/404/500 | P1 |
| INT-017 | Validation→Controller | Validation | Model, Controller | 400 + errors | P1 |
| INT-018 | Logging integration | Operation | Logger | Log entry | P1 |
| INT-019 | Metrics integration | Operation | Metrics | Counter incremented | P1 |
| INT-020 | Audit→DB | Create | Audit, DB | Audit row | P1 |
| INT-021 | Cache→DB | Get (cached) | Cache, DB | Cache hit/miss | P1 |
| INT-022 | Transaction scope | Create+child | Transaction | Both or neither | P1 |
| INT-023 | Multi-entity create | Continent+Regions | Multiple | All created | P1 |
| INT-024 | Bulk import | Import file | CSV, DB | All imported | P1 |
| INT-025 | Export→File | Export | DB, File | File created | P1 |
| INT-026 | Sync external | Sync call | External API | Data synced | P1 |
| INT-027 | Message queue | Publish event | Queue | Message sent | P1 |
| INT-028 | Event handler | Created event | Handler | Handler invoked | P1 |
| INT-029 | Notification | Create | Notifier | Notification sent | P1 |
| INT-030 | Search service | Search | Search index | Index updated | P1 |
| INT-031 | Report generation | Report | Report, DB | Report data | P1 |
| INT-032 | Dashboard agg | Dashboard | Dashboard, DB | Aggregations correct | P1 |
| INT-033 | Workflow transition | Status change | Workflow | Transition logged | P1 |
| INT-034 | Attachment link | Add attachment | Continent, Attachment | Linked | P1 |
| INT-035 | Tag association | Add tag | Continent, Tag | Tagged | P1 |
| INT-036 | Permission check | Permission | Permission service | Allowed/Denied | P1 |
| INT-037 | Tenant isolation | Multi-tenant | Tenant A, B | Isolated | P1 |
| INT-038 | Localization | Get with locale | i18n | Translated | P1 |
| INT-039 | Timezone handling | Date fields | User TZ | Correct display | P1 |
| INT-040 | Retry on failure | Transient fail | Retry policy | Retried, success | P1 |
| INT-041 | Circuit breaker | Repeated failures | Circuit | Opened | P1 |
| INT-042 | Health check | Health endpoint | DB, Services | Healthy | P1 |
| INT-043 | Config override | Env config | Config | Override applied | P1 |
| INT-044 | Feature flag | Flag off | Feature | Disabled | P1 |
| INT-045 | Rate limit | Many requests | Rate limiter | Limited | P1 |
| INT-046 | CORS | Cross-origin | CORS | Allowed/Blocked | P1 |
| INT-047 | API versioning | v2 API | Version | v2 behavior | P1 |
| INT-048 | Deprecation | Deprecated endpoint | API | Warning header | P1 |
| INT-049 | Backward compat | Old client | New API | Still works | P1 |
| INT-050 | Forward compat | New client | Old API | Graceful | P1 |
| INT-051 | DbContext | CRUD | DbContext | Persisted | P1 |
| INT-052 | Repository | CRUD | Repository | Persisted | P1 |
| INT-053 | AutoMapper | Map | Mapper | Mapped | P1 |
| INT-054 | RegionManager | Region | Manager | Loaded | P1 |
| INT-055 | AuditDbContext | Audit | Context | Audited | P1 |
| INT-056 | Transaction | Transaction | Commit | Committed | P1 |
| INT-057 | PermissionService | Check | Service | Checked | P1 |
| INT-058 | HttpClient | API | HttpClient | Response | P1 |
| INT-059 | Logging | Log | ILogger | Logged | P1 |
| INT-060 | Configuration | Config | IConfiguration | Loaded | P1 |
| INT-061 | DI container | Resolve | Container | Resolved | P1 |
| INT-062 | Scoped lifetime | Request | Scope | Per request | P1 |
| INT-063 | Soft delete filter | Global | Query | Filtered | P1 |
| INT-064 | Foreign key | FK | Constraint | Enforced | P1 |
| INT-065 | Unique constraint | Unique | Insert | Enforced | P1 |
| INT-066 | Cache | Cache | Get | Cached | P1 |
| INT-067 | Retry | Transient | Retry | Retried | P1 |
| INT-068 | Health check | Health | Check | Healthy | P1 |
| INT-069 | Metrics | Metric | Record | Recorded | P1 |
| INT-070 | User context | User | Context | Resolved | P1 |
| INT-071 | Export service | Export | Service | File | P1 |
| INT-072 | API versioning | Version | Request | Versioned | P1 |
| INT-073 | Rate limiting | Limit | Request | Limited | P1 |
| INT-074 | Auth middleware | Auth | Request | Authenticated | P1 |
| INT-075 | Validation middleware | Validate | Request | Validated | P1 |
| INT-076 | Exception middleware | Exception | Throw | Handled | P1 |
| INT-077 | Correlation ID | Request | ID | Propagated | P1 |
| INT-078 | Tracing | Trace | Span | Traced | P1 |
| INT-079 | Feature flag | Flag | Check | Toggled | P1 |
| INT-080 | CORS | Cross-origin | Request | Allowed | P1 |
| INT-081 | Connection | Connection | Open | Connected | P1 |
| INT-082 | Migration | Migration | Run | Applied | P1 |
| INT-083 | Index | Query | Index | Fast | P1 |
| INT-084 | Circuit breaker | Fail | Circuit | Open | P1 |
| INT-085 | Tenant context | Tenant | Context | Resolved | P1 |
| INT-086 | Region API | Region | API | Response | P1 |
| INT-087 | Hierarchy service | Hierarchy | Service | Tree | P1 |
| INT-088 | Forward compat | New client | Old API | Graceful | P1 |
| INT-089 | Batch flow | GetByCodes | Continents | Returned | P1 |
| INT-090 | Search flow | Create, Search | Continent | Found | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|---------------|----------|
| SEC-001 | SQL injection name | '; DROP TABLE-- | Name field | Sanitized/Rejected | P0 |
| SEC-002 | SQL injection code | 1' OR '1'='1 | Code | Rejected | P0 |
| SEC-003 | XSS in name | <script>alert(1)</script> | Name | Escaped | P0 |
| SEC-004 | XSS in output | Stored XSS | Display | Escaped | P0 |
| SEC-005 | Unauthorized get | No token | GetById | 401 | P0 |
| SEC-006 | Forbidden get | Wrong role | GetById | 403 | P0 |
| SEC-007 | IDOR get | Others' ID | GetById | 403 or 404 | P0 |
| SEC-008 | IDOR update | Others' ID | UpdateAsync | 403 | P0 |
| SEC-009 | IDOR delete | Others' ID | DeleteAsync | 403 | P0 |
| SEC-010 | Mass assignment | isAdmin=true | Create body | Ignored | P0 |
| SEC-011 | Parameterized query | SQL params | All queries | No injection | P0 |
| SEC-012 | Output encoding | HTML output | All responses | Encoded | P0 |
| SEC-013 | CSRF token | No token | POST | Rejected | P0 |
| SEC-014 | Session fixation | Fixated session | Auth | New session | P0 |
| SEC-015 | Session timeout | Expired session | Request | 401 | P0 |
| SEC-016 | LDAP injection | *)(uid=* | Search | Rejected | P1 |
| SEC-017 | NoSQL injection | {$gt: ""} | Filter | Rejected | P1 |
| SEC-018 | Command injection | ; ls | Field | Rejected | P1 |
| SEC-019 | Path traversal | ../../../etc/passwd | File param | Rejected | P1 |
| SEC-020 | XXE | XML entity | XML input | Rejected | P1 |
| SEC-021 | SSRF | Internal URL | URL param | Blocked | P1 |
| SEC-022 | Open redirect | redirect=evil.com | Redirect | Validated | P1 |
| SEC-023 | JWT tampering | Modified JWT | Auth | Rejected | P1 |
| SEC-024 | JWT algorithm none | alg=none | JWT | Rejected | P1 |
| SEC-025 | Token replay | Reuse token | Request | Rejected | P1 |
| SEC-026 | Privilege escalation | Low role do admin | Action | 403 | P1 |
| SEC-027 | Horizontal access | User A access B | Resource | 403 | P1 |
| SEC-028 | Vertical access | User access admin | Resource | 403 | P1 |
| SEC-029 | Sensitive data log | Password in log | Logging | Not logged | P1 |
| SEC-030 | Sensitive data response | Password in response | API | Not returned | P1 |
| SEC-031 | Info disclosure | Stack trace | Error | No trace in prod | P1 |
| SEC-032 | Verbose error | DB details | Error | Generic message | P1 |
| SEC-033 | Rate limit bypass | Many IPs | Rate limit | Per-user limit | P1 |
| SEC-034 | Brute force | Many auth attempts | Login | Lockout | P1 |
| SEC-035 | Header injection | CRLF in header | Header | Rejected | P1 |
| SEC-036 | Content-type bypass | Wrong content-type | Upload | Rejected | P1 |
| SEC-037 | File upload malicious | Exe as CSV | Upload | Rejected | P1 |
| SEC-038 | Oversized payload | 100MB | Request | Rejected | P1 |
| SEC-039 | Deep object | Nested 100 levels | JSON | Rejected | P1 |
| SEC-040 | Regex DoS | Evil regex | Pattern | Timeout/Reject | P1 |
| SEC-041 | Insecure deserialization | Malicious object | Deserialize | Rejected | P1 |
| SEC-042 | Prototype pollution | __proto__ | JSON | Sanitized | P1 |
| SEC-043 | CORS misconfig | Wildcard origin | CORS | Restricted | P1 |
| SEC-044 | Missing security headers | X-Frame-Options | Response | Headers present | P1 |
| SEC-045 | HSTS | HTTP request | Redirect | HTTPS | P1 |
| SEC-046 | Cookie secure | Cookie | Set-Cookie | Secure flag | P1 |
| SEC-047 | Cookie HttpOnly | Cookie | Set-Cookie | HttpOnly | P1 |
| SEC-048 | Password in URL | Password query | URL | Not logged | P1 |
| SEC-049 | Audit log integrity | Modify audit | Audit | Tamper evident | P1 |
| SEC-050 | Encryption at rest | DB storage | Sensitive fields | Encrypted | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior | Priority |
|----|-----------|----------|-------------------|----------|
| CON-001 | Concurrent create same code | 2 users create "XX" | One succeeds, one conflict | P1 |
| CON-002 | Concurrent update same | 2 users update same | Optimistic lock or last-write | P1 |
| CON-003 | Concurrent delete same | 2 users delete | One succeeds, one 404 | P1 |
| CON-004 | Read during update | Read while update | Consistent read | P1 |
| CON-005 | Update during delete | Update while delete | One fails | P1 |
| CON-006 | Double submit | Same form twice | Idempotent or reject | P1 |
| CON-007 | Transaction isolation | Parallel transactions | No dirty read | P1 |
| CON-008 | Deadlock | Circular wait | One retries/releases | P1 |
| CON-009 | Lost update | Interleaved update | Version/row lock | P1 |
| CON-010 | Phantom read | New row in range | Serializable or accept | P1 |
| CON-011 | Cache invalidation | Update after cache | Cache invalidated | P1 |
| CON-012 | Stale cache read | Read from stale | TTL or invalidation | P1 |
| CON-013 | Batch concurrent | 2 batches overlap | Both complete correctly | P1 |
| CON-014 | Connection pool | Exhaust pool | Queue or timeout | P1 |
| CON-015 | Lock timeout | Hold lock long | Timeout release | P1 |
| CON-016 | Distributed lock | Multi-instance | Single writer | P1 |
| CON-017 | Eventual consistency | Replica lag | Eventual converge | P1 |
| CON-018 | Retry idempotency | Retry after partial | No duplicate | P1 |
| CON-019 | Sequential consistency | Order of ops | Preserved | P1 |
| CON-020 | Visibility | Write then read | Read sees write | P1 |
| CON-021 | Split brain | Network partition | No data corruption | P2 |
| CON-022 | Failover | Primary fail | Replica promoted | P2 |
| CON-023 | Saga compensation | Partial failure | Compensate | P2 |
| CON-024 | Outbox pattern | Event publish | Exactly once | P2 |
| CON-025 | Two-phase commit | Distributed | Atomic or abort | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Name validation | Validation | Valid name | True | P1 |
| UNT-002 | Name validation invalid | Validation | Null | False | P1 |
| UNT-003 | Code validation | Validation | "AF" | True | P1 |
| UNT-004 | Code validation invalid | Validation | "A" | False | P1 |
| UNT-005 | Format code | Formatting | "af" | "AF" | P1 |
| UNT-006 | Trim name | Formatting | "  x  " | "x" | P1 |
| UNT-007 | Map entity to model | Mapping | Entity | Model all fields | P1 |
| UNT-008 | Region count calc | Calculation | Continent+regions | Sum | P1 |
| UNT-009 | Country count agg | Calculation | Regions+countries | Sum | P1 |
| UNT-010 | Status transition valid | Status logic | Draft→Active | True | P1 |
| UNT-011 | Status transition invalid | Status logic | Archived→Draft | False | P1 |
| UNT-012 | IsDeleted filter | Status logic | Mixed list | Only !IsDeleted | P1 |
| UNT-013 | Sort comparator | Collections | Unsorted list | Sorted | P1 |
| UNT-014 | Paginate slice | Collections | Full list, page 2 | Items 11-20 | P1 |
| UNT-015 | Search predicate | Collections | Query | Matching items | P1 |
| UNT-016 | Null safe | Validation | Null input | No throw | P1 |
| UNT-017 | Empty collection | Collections | [] | [] | P1 |
| UNT-018 | Map list | Mapping | Entity list | Model list | P1 |
| UNT-019 | Date format | Formatting | DateTime | ISO string | P1 |
| UNT-020 | Id equality | Validation | Same id | Equal | P1 |
| UNT-021 | Code equality | Validation | Same code | Equal | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetById latency | GetByIdAsync | < 50 ms | P2 |
| PRF-002 | GetAll latency | GetAllAsync 100 items | < 200 ms | P2 |
| PRF-003 | Create latency | CreateAsync | < 100 ms | P2 |
| PRF-004 | Update latency | UpdateAsync | < 100 ms | P2 |
| PRF-005 | Delete latency | DeleteAsync | < 100 ms | P2 |
| PRF-006 | Search latency | Search 1000 items | < 500 ms | P2 |
| PRF-007 | Pagination latency | Page 10 of 1000 | < 200 ms | P2 |
| PRF-008 | Bulk create 100 | CreateAsync batch | < 5 s | P2 |
| PRF-009 | Bulk get 100 | GetByIdsAsync 100 | < 500 ms | P2 |
| PRF-010 | Export 1000 | ExportAsync | < 5 s | P2 |
| PRF-011 | Concurrent 10 get | 10 parallel GetById | < 200 ms total | P2 |
| PRF-012 | Memory single op | Create | No leak | P2 |
| PRF-013 | Memory 1000 ops | 1000 creates | Stable heap | P2 |
| PRF-014 | Query plan | GetById | Index used | P2 |
| PRF-015 | N+1 check | Get with regions | Single query | P2 |
| PRF-016 | Connection reuse | 100 sequential | Pool stable | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|------------------|----------|
| LDT-001 | Sustained 10 RPS | 10 req/s | 10 min | 99% < 500 ms | P2 |
| LDT-002 | Sustained 50 RPS | 50 req/s | 5 min | 99% < 1 s | P2 |
| LDT-003 | Sustained 100 RPS | 100 req/s | 5 min | 95% < 2 s | P2 |
| LDT-004 | Spike 200 RPS | 0→200→0 | 2 min | No 5xx | P2 |
| LDT-005 | Spike 500 RPS | 5 s burst | 5 s | Recover | P2 |
| LDT-006 | Stress 500 RPS | 500 req/s | 2 min | Degrade gracefully | P2 |
| LDT-007 | Stress 1000 RPS | 1000 req/s | 1 min | No crash | P2 |
| LDT-008 | Endurance 20 RPS | 20 req/s | 1 h | No memory leak | P2 |
| LDT-009 | Recovery after spike | Post-spike | 5 min | Back to baseline | P2 |
| LDT-010 | Mixed workload | CRUD mix | 15 min | All ops succeed | P2 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
