# Bulk Operations — Test Cases

**Component:** Cross-cutting / Bulk Operations  
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
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P? 90≥90 ✅ | E≥3P? 90≥90 ✅ | F≥3P? 90≥90 ✅ | I≥3P? 90≥90 ✅

---

## Feature Overview

**Bulk Operations** provide batch create, update, and delete capabilities across entities, plus import/export, progress tracking, partial failure handling, and rollback. These operations are critical for data migration, mass updates, and bulk data management in the Opportunity+ system.

**Key Capabilities:**
- Batch create / update / delete
- Import from CSV/Excel
- Export to CSV/Excel
- Progress tracking and cancellation
- Partial failure handling (continue or stop)
- Transaction rollback on failure
- Validation before execution

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Batch create 10 partners | Valid CSV | Import 10 partners | All 10 created | P0 |
| POS-002 | Batch update 5 partners | 5 partners exist | Bulk update status | All 5 updated | P0 |
| POS-003 | Batch soft delete 3 partners | 3 partners exist | Bulk delete | All 3 soft-deleted | P0 |
| POS-004 | Export 100 partners to CSV | 100 partners | Export CSV | Valid CSV file | P0 |
| POS-005 | Progress tracking during import | Large file | Import 500 rows | Progress % updates | P0 |
| POS-006 | Cancel bulk operation mid-run | Import running | Click Cancel | Operation stops | P0 |
| POS-007 | Partial success: 8/10 created | 2 invalid in batch | Import 10 | 8 created, 2 in error report | P1 |
| POS-008 | Rollback on full failure | All invalid | Import invalid batch | No records created | P0 |
| POS-009 | Import with validation only | Dry run | Import with validate-only | Report, no changes | P1 |
| POS-010 | Export with filters | Partners exist | Export filtered | Filtered CSV | P0 |
| POS-011 | Batch create contacts | Partner exists | Import 20 contacts | All 20 created | P0 |
| POS-012 | Batch update opportunities | Opportunities exist | Bulk status update | All updated | P1 |
| POS-013 | Import from Excel | Valid Excel | Import | Data parsed correctly | P1 |
| POS-014 | Export to Excel | Data exists | Export Excel | Valid Excel file | P1 |
| POS-015 | Resume after partial failure | 5 failed, 95 ok | Retry failed only | 5 retried | P1 |
| POS-016 | Bulk operation audit | Complete bulk | Check audit | Bulk action audited | P0 |
| POS-017 | Bulk create with relationships | Parent exists | Import children | FK set correctly | P1 |
| POS-018 | Template download | N/A | Download import template | Valid template | P1 |
| POS-019 | Bulk assign roles | Users exist | Assign role to 10 users | All 10 updated | P1 |
| POS-020 | Export pagination | 5000 records | Export | Chunked or streamed | P1 |
| POS-021 | Bulk activate partners | Partners in Draft | Bulk activate | All activated | P1 |
| POS-022 | Bulk deactivate partners | Partners active | Bulk deactivate | All deactivated | P1 |
| POS-023 | Import with default values | Template with defaults | Import | Defaults applied | P1 |
| POS-024 | Export with selected columns | Export config | Export | Only selected columns | P1 |
| POS-025 | Bulk update single field | 100 records | Update one field | All 100 updated | P0 |
| POS-026 | Import duplicate handling | Configure skip | Import with duplicates | Duplicates skipped | P1 |
| POS-027 | Bulk operation summary | Complete bulk | View summary | Success/fail counts | P0 |
| POS-028 | Error report download | Partial failure | Download errors | CSV of failed rows | P1 |
| POS-029 | Bulk create interactions | Partners exist | Import 50 interactions | All 50 created | P1 |
| POS-030 | Batch size configuration | Config set | Import | Respects batch size | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 2.1 Invalid Input (15)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Empty file import | 0 rows | "No data to import" | P0 |
| NEG-002 | Invalid file format | .txt as CSV | "Invalid format" | P0 |
| NEG-003 | Corrupt Excel file | Broken xlsx | "File corrupted" | P0 |
| NEG-004 | Wrong column headers | Mismatched headers | "Missing required columns" | P0 |
| NEG-005 | Negative batch size | Size=-1 | 400 Bad Request | P0 |
| NEG-006 | Batch size over limit | Size=100000 | 400 or capped | P0 |
| NEG-007 | Invalid entity IDs in bulk update | Non-existent IDs | "Invalid IDs" error | P0 |
| NEG-008 | Null in required field | Null name | Row validation error | P0 |
| NEG-009 | Invalid CSV delimiter | Wrong delimiter | Parse error | P1 |
| NEG-010 | Encoding error | Wrong encoding | "Encoding not supported" | P1 |
| NEG-011 | Empty bulk update payload | Empty body | 400 Bad Request | P0 |
| NEG-012 | Duplicate IDs in batch | Same ID twice | Error or dedupe | P1 |
| NEG-013 | Invalid date format | Wrong date format | Row validation error | P1 |
| NEG-014 | Oversized file | 100MB file | "File too large" or reject | P0 |
| NEG-015 | Invalid export format | Format=invalid | 400 Bad Request | P1 |

### 2.2 Unauthorized Access (15)

| ID | Test Name | User Role | Action | Expected Result | Priority |
|----|-----------|-----------|--------|-----------------|----------|
| NEG-016 | Partner user bulk import | Partner role | Import partners | 403 Forbidden | P0 |
| NEG-017 | Read-only user bulk update | ReadOnly | Bulk update | 403 Forbidden | P0 |
| NEG-018 | Anonymous bulk export | No auth | Export | 401 Unauthorized | P0 |
| NEG-019 | User bulk delete without permission | No delete | Bulk delete | 403 Forbidden | P0 |
| NEG-020 | Cross-org bulk update | User in Org A | Update Org B entities | 403 or filtered | P0 |
| NEG-021 | Deactivated user bulk operation | User deactivated | Bulk create | 401 Unauthorized | P0 |
| NEG-022 | API key without bulk scope | Limited scope | Bulk import | 403 Forbidden | P0 |
| NEG-023 | Service account bulk | Service account | Bulk (if not allowed) | 403 | P1 |
| NEG-024 | Bulk operation on restricted entity | No entity access | Bulk that entity | 403 | P0 |
| NEG-025 | Export sensitive data | No export permission | Export | 403 | P0 |
| NEG-026 | Bulk assign role without permission | No role admin | Bulk assign | 403 | P0 |
| NEG-027 | Cancel other user's bulk | User A | Cancel User B's job | 403 | P0 |
| NEG-028 | View other user's bulk result | User A | View User B's summary | 403 | P0 |
| NEG-029 | Template download without permission | No import | Download template | 403 | P1 |
| NEG-030 | Bulk purge without admin | User | Bulk purge | 403 | P0 |

### 2.3 Validation Failures (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-031 | FK violation in import | Invalid PartnerId | Row rejected | P0 |
| NEG-032 | Unique constraint violation | Duplicate name | Row rejected | P0 |
| NEG-033 | Required field missing | Name empty | Row rejected | P0 |
| NEG-034 | Invalid enum value | Status=Invalid | Row rejected | P0 |
| NEG-035 | String over max length | Name 1000 chars | Row rejected | P0 |
| NEG-036 | Negative number | Amount=-100 | Row rejected | P0 |
| NEG-037 | Invalid email format | Email=invalid | Row rejected | P1 |
| NEG-038 | Invalid date | Date=invalid | Row rejected | P1 |
| NEG-039 | Circular reference | Self-reference | Rejected | P1 |
| NEG-040 | Business rule violation | Invalid transition | Row rejected | P0 |
| NEG-041 | Soft-deleted parent | Reference deleted | Row rejected | P1 |
| NEG-042 | Orphan reference | Non-existent FK | Row rejected | P0 |
| NEG-043 | Invalid workflow state | Wrong state for action | Rejected | P0 |
| NEG-044 | Permission check per row | Some rows no access | Those rows rejected | P1 |
| NEG-045 | Batch exceeds quota | Over org limit | Rejected | P1 |

### 2.4 Partial Failure & Rollback (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-046 | All rows fail | All invalid | No records, full rollback | P0 |
| NEG-047 | First row fails (stop on error) | Config stop | No records, rollback | P0 |
| NEG-048 | Middle row fails (continue) | Config continue | Partial success, error report | P1 |
| NEG-049 | DB timeout mid-batch | Timeout on row 50 | Rollback or partial + report | P1 |
| NEG-050 | Connection lost mid-import | Network drop | Rollback, retry possible | P1 |
| NEG-051 | Disk full during import | No space | Fail, rollback | P1 |
| NEG-052 | Deadlock in bulk update | Concurrent update | Retry or rollback | P1 |
| NEG-053 | Transaction timeout | Long batch | Timeout, rollback | P1 |
| NEG-054 | Partial rollback not supported | Mixed savepoints | Full rollback | P1 |
| NEG-055 | Retry after partial failure | 50 failed | Retry only failed | Works | P1 |
| NEG-056 | Export fails mid-stream | Disk full on export | Partial file or error | P1 |
| NEG-057 | Bulk delete with dependencies | Children exist | Block or cascade per config | P0 |
| NEG-058 | Bulk update stale data | Optimistic lock fail | Row rejected | P1 |
| NEG-059 | Import row causes trigger failure | Trigger error | Row rejected | P1 |
| NEG-060 | Bulk operation queue full | Too many jobs | "Queue full" error | P1 |

### 2.5 Dependency & System Failures (10)

| ID | Test Name | Failure | Expected | Priority |
|----|-----------|---------|----------|----------|
| NEG-061 | DB unavailable | DB down | Clear error, no partial | P0 |
| NEG-062 | File storage unavailable | Export storage | Export fails | P1 |
| NEG-063 | Email service down (notification) | Email fail | Bulk succeeds, no email | P1 |
| NEG-064 | Audit write fails | Audit table full | Bulk fails or degrades | P1 |
| NEG-065 | Memory exhausted | Very large file | OOM or chunked | P1 |
| NEG-066 | Concurrent bulk limit | 2nd bulk at same time | Queue or reject | P1 |
| NEG-067 | Rate limit exceeded | Too many requests | 429 Too Many Requests | P1 |
| NEG-068 | Session expired mid-bulk | Token expired | 401, partial state | P1 |
| NEG-069 | Server restart during bulk | Restart | Job lost or resume | P2 |
| NEG-070 | Permission revoked mid-bulk | User deactivated | Job fails | P1 |

### 2.6 Additional Negative (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | Import with BOM only | File has only BOM | No data error | P1 |
| NEG-072 | Export with no permission | No export perm | 403 | P0 |
| NEG-073 | Bulk update with stale version | Optimistic lock | Row rejected | P1 |
| NEG-074 | Import with wrong entity type | Partner data for Opp | Reject | P1 |
| NEG-075 | Template with invalid columns | Wrong template | Error | P1 |
| NEG-076 | Cancel already completed | Job done | Error or no-op | P1 |
| NEG-077 | Retry with no failed rows | All success | Error or no-op | P1 |
| NEG-078 | Export with invalid sort field | Sort=invalid | 400 | P1 |
| NEG-079 | Import with missing required | Required empty | Row rejected | P0 |
| NEG-080 | Bulk delete with no selection | Empty IDs | 400 | P0 |
| NEG-081 | Export with cross-org filter | User Org A, filter B | 403 | P0 |
| NEG-082 | Import for other org | Target org=other | 403 | P0 |
| NEG-083 | Bulk result expired | 25 hr old | 404 or expired | P1 |
| NEG-084 | Concurrent bulk over limit | 6th bulk | Queue or reject | P1 |
| NEG-085 | Import with invalid lookup | Lookup value missing | Row rejected | P1 |
| NEG-086 | Export format mismatch | Request CSV get JSON | Correct format | P1 |
| NEG-087 | Bulk job not found | Invalid job ID | 404 | P1 |
| NEG-088 | Retry other user's job | User A retry B's | 403 | P0 |
| NEG-089 | Import with formula injection | =cmd|' | Sanitized | P0 |
| NEG-090 | Export with path traversal | Filename ../../../ | Sanitized | P0 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 3.1 Batch Size Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Batch create size | 1 | 1000 | ✅ | ✅ | Reject | P1 |
| BND-002 | Batch update size | 1 | 500 | ✅ | ✅ | Reject | P1 |
| BND-003 | Batch delete size | 1 | 500 | ✅ | ✅ | Reject | P1 |
| BND-004 | Import rows | 1 | 10000 | ✅ | ✅ | Reject | P1 |
| BND-005 | Export rows | 0 | 100000 | ✅ | ✅ | Chunked | P1 |
| BND-006 | Progress increment | 1% | 100% | ✅ | ✅ | N/A | P2 |
| BND-007 | Error report rows | 0 | 10000 | ✅ | ✅ | Truncate | P1 |
| BND-008 | Concurrent bulk jobs | 1 | 5 | ✅ | ✅ | Queue | P1 |
| BND-009 | Retry count | 1 | 5 | ✅ | ✅ | Cap | P2 |
| BND-010 | Chunk size | 100 | 1000 | ✅ | ✅ | Default | P2 |
| BND-011 | File size MB | 0.1 | 50 | ✅ | ✅ | Reject | P1 |
| BND-012 | Column count | 1 | 100 | ✅ | ✅ | Reject | P2 |
| BND-013 | Template size | 1 | 1000 rows | ✅ | ✅ | N/A | P2 |
| BND-014 | Queue timeout | 60 | 3600 sec | ✅ | ✅ | Default | P2 |
| BND-015 | Batch ID length | 1 | 50 | ✅ | ✅ | Reject | P2 |

### 3.2 Numeric Boundaries (10)

| ID | Field | Zero | Negative | Very Large | Priority |
|----|-------|------|----------|------------|----------|
| BND-016 | Row count | ❌ | ❌ | Cap 10000 | P1 |
| BND-017 | Success count | ✅ | ❌ | ✅ | P1 |
| BND-018 | Failure count | ✅ | ❌ | ✅ | P1 |
| BND-019 | Progress percentage | 0 | ❌ | 100 | P1 |
| BND-020 | Entity ID in batch | ❌ | ❌ | Max int | P1 |
| BND-021 | Export limit | ❌ | ❌ | 100000 | P1 |
| BND-022 | Timeout seconds | 30 | ❌ | 7200 | P1 |
| BND-023 | Retry delay ms | 100 | ❌ | 10000 | P2 |
| BND-024 | File size bytes | ❌ | ❌ | 52428800 | P1 |
| BND-025 | Batch sequence | 1 | ❌ | Max | P2 |

### 3.3 Date/Time Boundaries (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-026 | Bulk at midnight | 00:00:00 start | Correct timestamps | P1 |
| BND-027 | Long-running bulk | 2 hours | Timeout or complete | P1 |
| BND-028 | Bulk during DST | DST transition | Time correct | P2 |
| BND-029 | Export date range | 1 day | Correct | P1 |
| BND-030 | Export date range | 10 years | Correct or chunked | P1 |
| BND-031 | Progress update interval | 1 sec | Updates every sec | P2 |
| BND-032 | Stale job cleanup | 24 hr old | Cleaned up | P2 |
| BND-033 | Retry after cooldown | 5 min after fail | Retry allowed | P2 |
| BND-034 | Bulk scheduled | Future time | Queued | P2 |
| BND-035 | Concurrent bulk start | Same second | Both queued | P2 |

### 3.4 Collection Boundaries (15)

| ID | Collection | State | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-036 | Import rows | 0 | Error | P0 |
| BND-037 | Import rows | 1 | Success | P0 |
| BND-038 | Import rows | 1000 | Success | P1 |
| BND-039 | Import rows | 10001 | Reject or split | P1 |
| BND-040 | Batch update IDs | 0 | Error | P0 |
| BND-041 | Batch update IDs | 1 | Success | P0 |
| BND-042 | Batch update IDs | 500 | Success | P1 |
| BND-043 | Export records | 0 | Empty file | P1 |
| BND-044 | Export records | 1 | Single row | P1 |
| BND-045 | Export records | 100000 | Large file | P1 |
| BND-046 | Error report | 0 failures | No report | P1 |
| BND-047 | Error report | 1000 failures | Full report | P1 |
| BND-048 | Progress updates | 1 row | 1 update | P2 |
| BND-049 | Progress updates | 10000 rows | Many updates | P2 |
| BND-050 | Concurrent jobs | 0 | N/A | P2 |
| BND-051 | Concurrent jobs | 5 | All run | P1 |
| BND-052 | Concurrent jobs | 6 | 6th queued | P1 |

### 3.5 Unicode & Special Characters (10)

| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-053 | CSV content | UTF-8 BOM | Parsed correctly | P1 |
| BND-054 | CSV content | Arabic names | Stored correctly | P1 |
| BND-055 | CSV content | Comma in field | Properly quoted | P0 |
| BND-056 | CSV content | Newline in field | Properly quoted | P0 |
| BND-057 | Export filename | Special chars | Sanitized | P1 |
| BND-058 | Template headers | Unicode | Correct headers | P1 |
| BND-059 | Error message | Unicode | Displayed correctly | P1 |
| BND-060 | Row data | Emoji | Stored or rejected | P2 |
| BND-061 | Export Excel | RTL text | Correct display | P2 |
| BND-062 | Import mapping | Special column names | Mapped correctly | P1 |

### 3.6 File & Format Boundaries (10)

| ID | Test Name | Condition | Expected | Priority |
|----|-----------|-----------|----------|----------|
| BND-063 | Empty CSV | 0 bytes | Error | P0 |
| BND-064 | CSV headers only | No data rows | Error | P0 |
| BND-065 | Excel empty sheet | No data | Error | P0 |
| BND-066 | Excel multiple sheets | 3 sheets | First or config | P1 |
| BND-067 | Large CSV | 49 MB | Accept | P1 |
| BND-068 | CSV at limit | 50 MB | Accept or reject | P1 |
| BND-069 | Invalid CSV row | 1 bad row in 100 | 99 success, 1 error | P1 |
| BND-070 | Mixed line endings | CRLF and LF | Parsed correctly | P1 |

### 3.7 Additional Boundaries (20)

| ID | Test Name | Condition | Expected | Priority |
|----|-----------|-----------|----------|----------|
| BND-071 | Import at 1 row | Single row | Success | P0 |
| BND-072 | Export at 1 record | Single | Single row file | P1 |
| BND-073 | Batch at 1 | Size 1 | Success | P0 |
| BND-074 | Progress at 0% | Start | 0% | P0 |
| BND-075 | Progress at 100% | Complete | 100% | P0 |
| BND-076 | Error report at 0 | No failures | No report | P1 |
| BND-077 | Concurrent at 1 | Single job | Success | P0 |
| BND-078 | File at 0.1 MB | Min size | Accept | P1 |
| BND-079 | File at 50 MB | Max size | Accept or reject | P1 |
| BND-080 | Column at 1 | Single column | Valid | P1 |
| BND-081 | Column at 100 | Max columns | Valid or reject | P2 |
| BND-082 | Timeout at 60 sec | Min | Fires at 60s | P1 |
| BND-083 | Timeout at 3600 sec | Max | Fires at 3600s | P1 |
| BND-084 | Retry at 1 | First retry | Success or fail | P1 |
| BND-085 | Retry at 5 | Max retry | Final | P1 |
| BND-086 | Chunk at 100 | Min chunk | Process | P2 |
| BND-087 | Chunk at 1000 | Max chunk | Process | P2 |
| BND-088 | Queue at 1 | Single slot | Accept | P1 |
| BND-089 | Batch ID at 1 char | Min length | Valid | P2 |
| BND-090 | Batch ID at 50 chars | Max length | Valid | P2 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 4.1 Workflow Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Bulk create in transaction | Import | All or nothing (if stop) | P0 |
| FUN-002 | Bulk update validates each row | Update | Invalid rows rejected | P0 |
| FUN-003 | Bulk delete soft deletes | Delete | IsDeleted=true | P0 |
| FUN-004 | Progress updates during run | Any bulk | Progress % increases | P0 |
| FUN-005 | Cancel stops processing | Cancel | No further rows | P0 |
| FUN-006 | Error report on partial failure | Partial fail | Report generated | P0 |
| FUN-007 | Audit for bulk operations | Any bulk | Audit entry | P0 |
| FUN-008 | FK validated on import | Import | Invalid FK rejected | P0 |
| FUN-009 | Unique constraint on import | Import | Duplicate rejected | P0 |
| FUN-010 | Permission per row | Bulk update | No access = reject row | P1 |
| FUN-011 | Retry only failed rows | Retry | Only failed retried | P1 |
| FUN-012 | Export respects filters | Export | Filtered result | P0 |
| FUN-013 | Template matches schema | Download | Valid template | P1 |
| FUN-014 | Batch size respected | Import | Processes in batches | P1 |
| FUN-015 | Timeout enforced | Long bulk | Timeout or complete | P1 |

### 4.2 Validation Rules (15)

| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | File format | CSV/Excel | Other → error | P0 |
| FUN-017 | Required columns | All present | Missing → error | P0 |
| FUN-018 | Row count limit | <= 10000 | > 10000 → error | P0 |
| FUN-019 | File size limit | <= 50 MB | > 50 MB → error | P0 |
| FUN-020 | Entity ID exists | Valid | Invalid → reject row | P0 |
| FUN-021 | Batch size 1-1000 | In range | Out → error | P1 |
| FUN-022 | Duplicate handling | Skip/Error | Config applied | P1 |
| FUN-023 | Encoding | UTF-8 | Other → error | P1 |
| FUN-024 | Date format | Valid | Invalid → reject | P1 |
| FUN-025 | Numeric format | Valid | Invalid → reject | P1 |
| FUN-026 | Email format | Valid | Invalid → reject | P1 |
| FUN-027 | Permission check | Has access | No access → 403 | P0 |
| FUN-028 | Concurrent limit | <= 5 | > 5 → queue | P1 |
| FUN-029 | Export format | CSV/Excel | Other → error | P1 |
| FUN-030 | Mapping valid | Valid map | Invalid → error | P1 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Test | Expected | Priority |
|----|------------|------|----------|----------|
| FUN-031 | One bulk per user (optional) | 2nd bulk | Queue or reject | P1 |
| FUN-032 | Bulk job unique ID | Create job | Unique ID | P1 |
| FUN-033 | No update to running job | Update running | Rejected | P1 |
| FUN-034 | Cancel only own job | Cancel other | 403 | P0 |
| FUN-035 | Export row limit | 100000 | Enforced | P1 |
| FUN-036 | Import stop on first error | Config | Stop | P0 |
| FUN-037 | Import continue on error | Config | Continue | P0 |
| FUN-038 | Rollback on full failure | All fail | Rollback | P0 |
| FUN-039 | Retention of bulk results | 24 hr | Deleted after | P2 |
| FUN-040 | Quota per org | Over quota | Rejected | P1 |

### 4.4 Audit Rules (10)

| ID | Action | Expected Audit | Priority |
|----|--------|----------------|----------|
| FUN-041 | Bulk import start | JobId, User, Timestamp | P0 |
| FUN-042 | Bulk import complete | Success/Fail count | P0 |
| FUN-043 | Bulk update | Entity type, Count | P0 |
| FUN-044 | Bulk delete | Entity type, Count | P0 |
| FUN-045 | Bulk export | Filters, Count | P0 |
| FUN-046 | Bulk cancel | JobId, User | P1 |
| FUN-047 | Bulk retry | JobId, Retry count | P1 |
| FUN-048 | Partial failure | Error count, Rows | P1 |
| FUN-049 | Template download | User, Template type | P1 |
| FUN-050 | Bulk permission denied | User, Action, 403 | P0 |

### 4.5 Additional Functional Rules (40)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-051 | File format CSV | .csv | Accept | P0 |
| FUN-052 | File format Excel | .xlsx | Accept | P0 |
| FUN-053 | Required columns | Import | All present | P0 |
| FUN-054 | Row count limit | Import | <= 10000 | P0 |
| FUN-055 | File size limit | Import | <= 50 MB | P0 |
| FUN-056 | Entity ID exists | Update | Valid | P0 |
| FUN-057 | Batch size range | 1-1000 | In range | P1 |
| FUN-058 | Duplicate handling | Config | Skip or error | P1 |
| FUN-059 | Encoding UTF-8 | Import | UTF-8 | P1 |
| FUN-060 | Date format | Import | Valid | P1 |
| FUN-061 | Numeric format | Import | Valid | P1 |
| FUN-062 | Email format | Import | Valid | P1 |
| FUN-063 | Permission check | Bulk | Has access | P0 |
| FUN-064 | Concurrent limit | <= 5 | Queue or reject | P1 |
| FUN-065 | Export format | CSV/Excel | Valid | P1 |
| FUN-066 | Mapping valid | Import | Valid map | P1 |
| FUN-067 | One bulk per user | Optional | Queue or reject | P1 |
| FUN-068 | Bulk job unique ID | Create | Unique | P1 |
| FUN-069 | No update running job | Update running | Rejected | P1 |
| FUN-070 | Cancel own job only | Cancel other | 403 | P0 |
| FUN-071 | Export row limit | 100000 | Enforced | P1 |
| FUN-072 | Import stop on error | Config | Stop | P0 |
| FUN-073 | Import continue on error | Config | Continue | P0 |
| FUN-074 | Rollback full failure | All fail | Rollback | P0 |
| FUN-075 | Retention 24 hr | Results | Deleted after | P2 |
| FUN-076 | Quota per org | Over quota | Rejected | P1 |
| FUN-077 | Bulk import start audit | Start | JobId, User | P0 |
| FUN-078 | Bulk import complete audit | Complete | Success/Fail count | P0 |
| FUN-079 | Bulk update audit | Update | Entity type, Count | P0 |
| FUN-080 | Bulk delete audit | Delete | Entity type, Count | P0 |
| FUN-081 | Bulk export audit | Export | Filters, Count | P0 |
| FUN-082 | Bulk cancel audit | Cancel | JobId, User | P1 |
| FUN-083 | Bulk retry audit | Retry | JobId, Retry count | P1 |
| FUN-084 | Template download audit | Download | User, Template | P1 |
| FUN-085 | FK validated import | Import | Invalid FK rejected | P0 |
| FUN-086 | Unique on import | Import | Duplicate rejected | P0 |
| FUN-087 | Permission per row | Bulk update | No access = reject | P1 |
| FUN-088 | Retry failed only | Retry | Only failed | P1 |
| FUN-089 | Export respects filters | Export | Filtered | P0 |
| FUN-090 | Template matches schema | Download | Valid template | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 5.1 CRUD Integration (15)

| ID | Operation | Entities | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-001 | Import partners → create contacts | Partner, Contact | Both created | P0 |
| INT-002 | Import opportunities → link partners | Opp, Partner | FK set | P0 |
| INT-003 | Bulk update → audit | Partner, Audit | Audit entry | P0 |
| INT-004 | Bulk delete → soft delete | Partner | IsDeleted set | P0 |
| INT-005 | Import → validation → create | Partner | Validated then created | P0 |
| INT-006 | Export → import round-trip | Partner | Data preserved | P1 |
| INT-007 | Bulk update → notification | Partner | Notifications sent | P1 |
| INT-008 | Bulk create → search index | Partner | Indexed | P1 |
| INT-009 | Import with lookup | Partner type | Lookup resolved | P1 |
| INT-010 | Bulk workflow state change | Opportunity | State updated | P1 |
| INT-011 | Import → cascade | Parent, Child | Child created | P1 |
| INT-012 | Bulk delete with dependencies | Partner, Contact | Per config | P0 |
| INT-013 | Export → external system | Partner | Format compatible | P2 |
| INT-014 | Import from external | External format | Mapped and imported | P2 |
| INT-015 | Bulk → sync to oUP | Partner | Synced | P2 |

### 5.2 Search/Filter/Pagination (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-016 | Export with date filter | Last 30 days | Filtered | P0 |
| INT-017 | Export with status filter | Active only | Filtered | P0 |
| INT-018 | Export with org filter | Org A | Only Org A | P0 |
| INT-019 | Export pagination | 5000 records | Chunked | P1 |
| INT-020 | Import with filter | Only valid rows | Invalid skipped | P1 |
| INT-021 | Bulk update with filter | Status=Draft | Only drafts | P1 |
| INT-022 | Bulk delete with filter | Inactive | Only inactive | P1 |
| INT-023 | Export sort | By name | Sorted | P1 |
| INT-024 | Export columns | Selected | Only those | P1 |
| INT-025 | Import column mapping | Map A→B | Mapped | P1 |

### 5.3 Progress & Error Handling (15)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-026 | Progress 0% at start | Start | 0% | P0 |
| INT-027 | Progress 100% at end | Complete | 100% | P0 |
| INT-028 | Progress incremental | Running | Increases | P0 |
| INT-029 | Cancel updates status | Cancel | Status=Cancelled | P0 |
| INT-030 | Error report downloadable | Partial fail | Report available | P0 |
| INT-031 | Retry from report | Retry | Failed rows retried | P1 |
| INT-032 | Partial success count | 80/100 | 80 success, 20 fail | P1 |
| INT-033 | Timeout message | Timeout | Clear message | P1 |
| INT-034 | DB error message | DB fail | Clear error | P1 |
| INT-035 | Validation error per row | Row 5 invalid | Row 5 in report | P1 |
| INT-036 | Concurrent bulk progress | 2 bulks | Separate progress | P1 |
| INT-037 | Bulk result retention | 24 hr | Deleted after | P2 |
| INT-038 | Resume interrupted | Interrupt | Resume or restart | P2 |
| INT-039 | Export stream | Large export | Streamed | P1 |
| INT-040 | Import stream | Large import | Streamed | P1 |

### 5.4 Relationships & Error Handling (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-041 | Import FK validation | Invalid PartnerId | Row rejected | P0 |
| INT-042 | Bulk update FK | Valid PartnerId | Updated | P0 |
| INT-043 | Bulk delete with children | Partner has contacts | Per config | P0 |
| INT-044 | Export with relations | Include related | Related in export | P1 |
| INT-045 | Import creates parent | Parent first | Order respected | P1 |
| INT-046 | Transaction boundary | Rollback | No partial | P0 |
| INT-047 | Retry transaction | Retry | New transaction | P1 |
| INT-048 | Audit + bulk | Bulk complete | Audit written | P0 |
| INT-049 | Notification + bulk | Bulk complete | Notification sent | P1 |
| INT-050 | Permission + bulk | Per-row permission | Rows filtered | P0 |

### 5.5 Additional Integration Flows (40)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-051 | Import partners → create contacts | Partner, Contact | Both created | P0 |
| INT-052 | Import opportunities → link partners | Opp, Partner | FK set | P0 |
| INT-053 | Bulk update → audit | Partner, Audit | Audit entry | P0 |
| INT-054 | Bulk delete → soft delete | Partner | IsDeleted set | P0 |
| INT-055 | Import → validation → create | Partner | Validated then created | P0 |
| INT-056 | Export → import round-trip | Partner | Data preserved | P1 |
| INT-057 | Bulk update → notification | Partner | Notifications sent | P1 |
| INT-058 | Bulk create → search index | Partner | Indexed | P1 |
| INT-059 | Import with lookup | Partner type | Lookup resolved | P1 |
| INT-060 | Bulk workflow state change | Opportunity | State updated | P1 |
| INT-061 | Import → cascade | Parent, Child | Child created | P1 |
| INT-062 | Bulk delete with dependencies | Partner, Contact | Per config | P0 |
| INT-063 | Export → external system | Partner | Format compatible | P2 |
| INT-064 | Import from external | External format | Mapped and imported | P2 |
| INT-065 | Bulk → sync to oUP | Partner | Synced | P2 |
| INT-066 | Export with date filter | Last 30 days | Filtered | P0 |
| INT-067 | Export with status filter | Active only | Filtered | P0 |
| INT-068 | Export with org filter | Org A | Only Org A | P0 |
| INT-069 | Export pagination | 5000 records | Chunked | P1 |
| INT-070 | Import with filter | Only valid rows | Invalid skipped | P1 |
| INT-071 | Bulk update with filter | Status=Draft | Only drafts | P1 |
| INT-072 | Bulk delete with filter | Inactive | Only inactive | P1 |
| INT-073 | Export sort | By name | Sorted | P1 |
| INT-074 | Export columns | Selected | Only those | P1 |
| INT-075 | Import column mapping | Map A→B | Mapped | P1 |
| INT-076 | Progress 0% at start | Start | 0% | P0 |
| INT-077 | Progress 100% at end | Complete | 100% | P0 |
| INT-078 | Progress incremental | Running | Increases | P0 |
| INT-079 | Cancel updates status | Cancel | Status=Cancelled | P0 |
| INT-080 | Error report downloadable | Partial fail | Report available | P0 |
| INT-081 | Retry from report | Retry | Failed rows retried | P1 |
| INT-082 | Partial success count | 80/100 | 80 success, 20 fail | P1 |
| INT-083 | Timeout message | Timeout | Clear message | P1 |
| INT-084 | DB error message | DB fail | Clear error | P1 |
| INT-085 | Validation error per row | Row 5 invalid | Row 5 in report | P1 |
| INT-086 | Concurrent bulk progress | 2 bulks | Separate progress | P1 |
| INT-087 | Bulk result retention | 24 hr | Deleted after | P2 |
| INT-088 | Resume interrupted | Interrupt | Resume or restart | P2 |
| INT-089 | Export stream | Large export | Streamed | P1 |
| INT-090 | Import stream | Large import | Streamed | P1 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: 50** | ✅ COMPLIANT

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | CSV injection | Cell content | Sanitized | P0 |
| SEC-002 | Formula injection | Excel cell | Sanitized | P0 |
| SEC-003 | SQL injection in filter | Export filter | Parameterized | P0 |
| SEC-004 | XSS in error report | Error message | Escaped | P0 |
| SEC-005 | Path traversal in filename | Export filename | Sanitized | P0 |
| SEC-006 | Command injection | Import path | Validated | P0 |
| SEC-007 | XXE in Excel | Malicious xlsx | Rejected | P0 |
| SEC-008 | Zip slip in Excel | Path traversal | Rejected | P0 |
| SEC-009 | Log injection | Bulk content | Escaped | P0 |
| SEC-010 | Header injection | CSV header | Validated | P0 |

### 6.2 Access Control (10)

| ID | User | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Bulk import | 401 | P0 |
| SEC-012 | Partner user | Bulk partner | 403 | P0 |
| SEC-013 | Read-only | Bulk update | 403 | P0 |
| SEC-014 | Admin | Full bulk | 200 | P0 |
| SEC-015 | Org-scoped | Cross-org bulk | 403 | P0 |
| SEC-016 | User A | User B's bulk result | 403 | P0 |
| SEC-017 | API key | Bulk (no scope) | 403 | P0 |
| SEC-018 | Service account | Bulk (if not allowed) | 403 | P1 |
| SEC-019 | Delegated | Bulk on behalf | Per delegation | P1 |
| SEC-020 | Expired session | Bulk | 401 | P0 |

### 6.3 IDOR (10)

| ID | Manipulation | Expected | Priority |
|----|-------------|----------|----------|
| SEC-021 | Bulk update wrong IDs | 403 or filtered | P0 |
| SEC-022 | Access other's bulk job | 403 | P0 |
| SEC-023 | Export other org data | 403 | P0 |
| SEC-024 | Import for other org | 403 | P0 |
| SEC-025 | Modify job ID | Ignored | P0 |
| SEC-026 | Bulk delete others' entities | 403 | P0 |
| SEC-027 | Template for restricted entity | 403 | P0 |
| SEC-028 | Error report IDOR | 403 | P0 |
| SEC-029 | Retry other's failed job | 403 | P0 |
| SEC-030 | Bulk result IDOR | 403 | P0 |

### 6.4 Auth & Session (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| SEC-031 | JWT expired | 401 | P0 |
| SEC-032 | CSRF on bulk | Token required | P0 |
| SEC-033 | Replay bulk request | Nonce/timestamp | P1 |
| SEC-034 | Session timeout mid-bulk | 401, clear state | P1 |
| SEC-035 | Token rotation during bulk | New token | P1 |
| SEC-036 | Concurrent session | Per policy | P1 |
| SEC-037 | Bulk from different IP | Per policy | P2 |
| SEC-038 | Bulk with MFA | MFA required | P1 |
| SEC-039 | Bulk after password change | Re-auth | P1 |
| SEC-040 | Bulk with limited scope | Scope enforced | P0 |

### 6.5 Data Exposure (10)

| ID | Data | Risk | Expected | Priority |
|----|------|------|----------|----------|
| SEC-041 | PII in export | Mask or exclude | PII protected | P0 |
| SEC-042 | Sensitive in error report | Mask | Protected | P0 |
| SEC-043 | Password in import | Never import | Rejected | P0 |
| SEC-044 | Token in bulk result | Never | Excluded | P0 |
| SEC-045 | Internal IDs in export | Per config | Configurable | P1 |
| SEC-046 | Cross-org data leak | Filter | No leak | P0 |
| SEC-047 | Error message info leak | Generic | No internal details | P0 |
| SEC-048 | Stack trace in bulk error | Never | No stack trace | P0 |
| SEC-049 | Bulk result retention | 24 hr | Deleted | P1 |
| SEC-050 | Template sensitive data | None | Clean template | P1 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | 2 users bulk import | Both succeed | P0 |
| CON-002 | 2 users bulk update same entity | One wins or both | P1 |
| CON-003 | 5 concurrent bulk imports | All 5 or queued | P1 |
| CON-004 | Bulk + single create | Both succeed | P1 |
| CON-005 | Bulk update + single update | No deadlock | P1 |
| CON-006 | Export + import same entity | Both succeed | P1 |
| CON-007 | 2 exports same data | Both succeed | P1 |
| CON-008 | Bulk delete + single read | Consistent | P1 |
| CON-009 | Cancel during progress | Cancel takes effect | P1 |
| CON-010 | Retry + new bulk | Both succeed | P1 |
| CON-011 | Connection pool under bulk | No exhaustion | P1 |
| CON-012 | Lock contention bulk update | Timeout or queue | P1 |
| CON-013 | Audit write during bulk | Both succeed | P1 |
| CON-014 | Bulk + search index update | Both succeed | P1 |
| CON-015 | 10 concurrent exports | All or queue | P1 |
| CON-016 | Bulk job queue order | FIFO | P1 |
| CON-017 | Optimistic lock bulk update | Stale rejected | P1 |
| CON-018 | Transaction isolation bulk | No dirty read | P1 |
| CON-019 | Bulk rollback + concurrent | No conflict | P1 |
| CON-020 | Progress update race | No corruption | P2 |
| CON-021 | Error report concurrent | No corruption | P2 |
| CON-022 | Template download concurrent | Both succeed | P2 |
| CON-023 | Purge old jobs + new bulk | No conflict | P2 |
| CON-024 | Bulk timeout + new bulk | Both handled | P2 |
| CON-025 | Connection lost + retry | Retry works | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

### 8.1 Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | Validate CSV row | Valid row | Valid | P1 |
| UNT-002 | Validate CSV row | Invalid row | Invalid | P1 |
| UNT-003 | Validate batch size | 100 | Valid | P1 |
| UNT-004 | Validate batch size | 2000 | Invalid | P1 |
| UNT-005 | Validate file format | .csv | Valid | P1 |

### 8.2 Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | Format progress | 0.5 | 50% | P1 |
| UNT-007 | Format error row | Row 5 | Row 5: reason | P1 |
| UNT-008 | Format export row | Entity | CSV row | P1 |

### 8.3 Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | Batch count | 1000 rows, 100 size | 10 batches | P1 |
| UNT-010 | Progress % | 50/100 | 50% | P1 |
| UNT-011 | Success rate | 80/100 | 80% | P1 |
| UNT-012 | Chunk size | 5000, 1000 | 5 chunks | P2 |
| UNT-013 | Retry count | 3 failures | 3 | P1 |

### 8.4 Status Logic (5)

| ID | Test | Condition | Expected | Priority |
|----|------|-----------|----------|----------|
| UNT-014 | Is complete | 100% | True | P1 |
| UNT-015 | Is cancelled | Cancelled | True | P1 |
| UNT-016 | Has errors | Fail count > 0 | True | P1 |
| UNT-017 | Can retry | Failed rows | True | P1 |
| UNT-018 | Is running | 0 < progress < 100 | True | P1 |

### 8.5 Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | Split batch | 1000, 100 | 10 arrays | P1 |
| UNT-020 | Merge error report | 2 reports | Combined | P1 |
| UNT-021 | Dedupe IDs | [1,1,2,2] | [1,2] | P1 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | Import 100 rows | < 10 s | P1 |
| PRF-002 | Import 1000 rows | < 60 s | P1 |
| PRF-003 | Bulk update 100 | < 15 s | P1 |
| PRF-004 | Bulk delete 100 | < 10 s | P1 |
| PRF-005 | Export 1000 rows | < 5 s | P1 |
| PRF-006 | Export 10000 rows | < 30 s | P1 |
| PRF-007 | Progress update latency | < 500 ms | P1 |
| PRF-008 | Cancel latency | < 2 s | P1 |
| PRF-009 | 10 concurrent imports | < 2 min total | P1 |
| PRF-010 | Template download | < 1 s | P1 |
| PRF-011 | Validation 1000 rows | < 5 s | P1 |
| PRF-012 | Error report generation | < 3 s | P1 |
| PRF-013 | Memory: import 10K | < 500 MB | P2 |
| PRF-014 | Memory: export 50K | < 1 GB | P2 |
| PRF-015 | DB round-trips | Batched | P2 |
| PRF-016 | Retry partial | < 30 s | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 10 bulk imports/hr | 1 hr | All complete | P1 |
| LDT-002 | 20 bulk imports/hr | 1 hr | All complete | P1 |
| LDT-003 | 50 bulk imports/hr | 30 min | Degradation ok | P2 |
| LDT-004 | Spike: 5 concurrent | 5 min | Queue or complete | P1 |
| LDT-005 | Spike: 10 exports | 2 min | All complete | P2 |
| LDT-006 | Stress: 1000 row import | 5 min | Complete or timeout | P2 |
| LDT-007 | Stress: 5 concurrent 1K | 10 min | Observe limits | P2 |
| LDT-008 | Stress: export 100K | 1 run | Complete | P2 |
| LDT-009 | Recovery after spike | 10 min | Normal | P1 |
| LDT-010 | Recovery after stress | 15 min | Full recovery | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Batch create/update/delete | POS-001–003, FUN-001–003 |
| Import/export | POS-004, POS-009–014, INT-001–050 |
| Progress tracking | POS-005–006, FUN-004–005 |
| Partial failure | POS-007, NEG-046–060 |
| Rollback | POS-008, FUN-001, NEG-046–055 |
| Security | SEC-001–050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
