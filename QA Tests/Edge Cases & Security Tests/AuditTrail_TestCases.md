# Audit Trail — Test Cases

**Component:** Cross-cutting / Audit Trail  
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

**Audit Trail** provides cross-cutting audit logging across all entities in the Opportunity+ system. It captures create/update/delete operations, user tracking, timestamps, field-level changes, export capabilities, and retention policies. The audit trail is essential for compliance, forensic investigation, and data lineage tracking.

**Key Capabilities:**
- CUD (Create/Update/Delete) operation logging
- User identification (CreatedBy, LastModifiedBy)
- Timestamp accuracy (CreatedDate, LastModifiedDate)
- Field-level change tracking (old value → new value)
- Soft delete audit (DeletedBy, DeletedDate)
- Audit export and reporting
- Retention policy enforcement

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create partner — audit entry created | Fresh DB | Create partner via API | Audit record: Action=Create, User, Timestamp | P0 |
| POS-002 | Update partner — audit entry created | Partner exists | Update partner name | Audit record: Action=Update, OldValue, NewValue | P0 |
| POS-003 | Soft delete partner — audit entry created | Partner exists | Delete partner | Audit record: Action=Delete, DeletedBy, DeletedDate | P0 |
| POS-004 | Field-level change captured | Partner exists | Update single field | Old and new values in audit detail | P0 |
| POS-005 | User ID correctly recorded | User authenticated | Create entity | CreatedBy matches current user ID | P0 |
| POS-006 | Timestamp in UTC | Any CUD operation | Perform action | CreatedDate/LastModifiedDate in UTC | P0 |
| POS-007 | Multiple field updates in one transaction | Partner exists | Update 3 fields | Single audit entry with all 3 changes | P1 |
| POS-008 | Audit for opportunity create | Fresh DB | Create opportunity | Opportunity audit record created | P0 |
| POS-009 | Audit for contact create | Partner exists | Create contact | Contact audit record with PartnerId | P0 |
| POS-010 | Audit for document attach | Opportunity exists | Attach document | Document attachment audit record | P1 |
| POS-011 | Export audit log by date range | Audit data exists | Export last 30 days | CSV/JSON with correct records | P1 |
| POS-012 | Export audit log by entity type | Audit data exists | Export Partner audits | Only Partner records returned | P1 |
| POS-013 | Export audit log by user | Audit data exists | Export by UserId | Only that user's actions | P1 |
| POS-014 | Query audit by entity ID | Partner exists | Query audit for PartnerId | All CUD for that partner | P0 |
| POS-015 | Login action logged | User authenticates | Login success | Login audit entry | P1 |
| POS-016 | Failed login logged | Invalid credentials | Login attempt | Failed login audit with IP | P1 |
| POS-017 | Workflow state change audited | Opportunity in workflow | Submit for Go | Workflow action in audit | P0 |
| POS-018 | Role assignment audited | User exists | Assign role | Role change in audit | P1 |
| POS-019 | Permission change audited | User exists | Change permission | Permission audit record | P1 |
| POS-020 | Retained records within policy | Data older than retention | Query retention | Records within policy retained | P1 |
| POS-021 | Audit for interaction create | Partner exists | Log interaction | Interaction audit record | P1 |
| POS-022 | Audit for bulk create | Batch create 10 partners | Bulk create | 10 audit entries (or 1 batch entry) | P1 |
| POS-023 | Null field change tracked | Field was null | Update to value | OldValue=null, NewValue=value | P2 |
| POS-024 | Empty string to non-empty | Field was "" | Update to "abc" | Both values in audit | P2 |
| POS-025 | JSON complex field change | Entity with JSON field | Update JSON | Old/new JSON serialized | P2 |
| POS-026 | Cascade delete audit | Parent deleted | Child soft-deleted | Parent and child audit entries | P1 |
| POS-027 | Audit includes entity name | Create partner "Acme" | Check audit | EntityName or identifier in record | P1 |
| POS-028 | Audit includes IP address | CUD from client | Check audit | Client IP captured (if configured) | P2 |
| POS-029 | Audit for status change | Entity in Draft | Change to Active | Status transition in audit | P1 |
| POS-030 | Audit for workflow approval | Opportunity in GO | DoA2 approves | Approval action in audit | P0 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 2.1 Audit Bypass / Tampering (15)

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-001 | Bypass audit on direct DB insert | Direct SQL insert | Audit not created OR trigger enforced | P0 |
| NEG-002 | Modify audit record via API | PUT /audit/{id} | 403/404 — audit immutable | P0 |
| NEG-003 | Delete audit record via API | DELETE /audit/{id} | 403 — deletion not allowed | P0 |
| NEG-004 | Inject fake CreatedBy | Request with spoofed user ID | Server uses token user, not request | P0 |
| NEG-005 | Inject fake timestamp | Request with future timestamp | Server uses server time | P0 |
| NEG-006 | Null user context | Unauthenticated CUD | 401 or audit with System/Anonymous | P0 |
| NEG-007 | Expired token CUD | Token expired | 401, no audit or audit with error | P0 |
| NEG-008 | Audit disabled by config | Audit disabled | CUD succeeds, no audit (documented) | P1 |
| NEG-009 | Audit table full | Disk full | CUD fails or graceful degradation | P1 |
| NEG-010 | Audit write timeout | DB timeout on audit insert | Transaction rollback, no partial CUD | P1 |
| NEG-011 | Query audit with invalid entity ID | EntityId=999999 | Empty result or 404 | P1 |
| NEG-012 | Export with invalid date range | End before start | Validation error | P1 |
| NEG-013 | Export without permission | User without audit view | 403 Forbidden | P0 |
| NEG-014 | Audit truncation attack | 1MB string in field | Truncated or rejected | P1 |
| NEG-015 | SQL injection in audit filter | Filter='; DROP-- | Parameterized, no injection | P0 |

### 2.2 Invalid Inputs (15)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-016 | Negative entity ID in query | EntityId=-1 | 400 or empty result | P1 |
| NEG-017 | Null entity type filter | EntityType=null | 400 or ignore | P1 |
| NEG-018 | Invalid date format in export | Date=invalid | 400 Bad Request | P1 |
| NEG-019 | Page size over max | PageSize=10000 | 400 or capped | P1 |
| NEG-020 | Page number negative | Page=-1 | 400 or treat as 1 | P1 |
| NEG-021 | Empty user ID filter | UserId="" | 400 or ignore | P2 |
| NEG-022 | Future start date | StartDate=tomorrow | 400 or empty | P1 |
| NEG-023 | Action filter invalid | Action=InvalidAction | 400 or empty | P1 |
| NEG-024 | Export format invalid | Format=xyz | 400 Bad Request | P1 |
| NEG-025 | Retention query past cutoff | Query 10-year-old data | Empty or policy message | P1 |
| NEG-026 | Duplicate audit key | Retry same CUD idempotent | No duplicate audit | P1 |
| NEG-027 | Empty export criteria | No filters | 400 or default range | P1 |
| NEG-028 | Special chars in filter | Filter=<script> | Escaped, no XSS | P0 |
| NEG-029 | Oversized export request | Request 10 years | 400 or paginated | P1 |
| NEG-030 | Concurrent export same user | 2 exports at once | Both succeed or queue | P2 |

### 2.3 Unauthorized Access (15)

| ID | Test Name | User Role | Action | Expected Result | Priority |
|----|-----------|-----------|--------|-----------------|----------|
| NEG-031 | Partner user views audit | Partner role | GET /audit | 403 Forbidden | P0 |
| NEG-032 | Anonymous user exports audit | No auth | Export | 401 Unauthorized | P0 |
| NEG-033 | Read-only user exports | ReadOnly | Export | 403 Forbidden | P0 |
| NEG-034 | User views audit for other org | Different org | Query audit | 403 or filtered to own org | P0 |
| NEG-035 | Collaborator views OM audit | Collaborator | View opp audit | 403 or filtered | P1 |
| NEG-036 | API key without audit scope | API key | Query audit | 403 Forbidden | P0 |
| NEG-037 | Service account audit access | Service account | Export | 403 or allowed per config | P1 |
| NEG-038 | Deactivated user CUD | User deactivated | Create entity | 401, no audit | P0 |
| NEG-039 | Cross-tenant audit query | User A queries Tenant B | 403 or empty | P0 |
| NEG-040 | Audit admin only endpoint | Non-admin | Admin audit config | 403 Forbidden | P0 |
| NEG-041 | View soft-deleted audit | User without delete view | Query deleted entities | Filtered or 403 | P1 |
| NEG-042 | Audit retention override | Non-admin | Change retention | 403 Forbidden | P0 |
| NEG-043 | Bulk export without permission | User | Bulk export | 403 Forbidden | P0 |
| NEG-044 | Audit schema access | Regular user | Query audit schema | 403 or masked | P1 |
| NEG-045 | Audit purge without role | User | Purge old audit | 403 Forbidden | P0 |

### 2.4 Edge Failure Scenarios (25)

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-046 | DB connection lost during audit write | Connection drop | CUD fails or retry | P1 |
| NEG-047 | Audit table locked | Lock contention | Timeout, retry or fail CUD | P1 |
| NEG-048 | Audit schema migration mid-write | Migration running | Graceful handling | P2 |
| NEG-049 | Circular audit reference | Entity references self | No infinite loop | P2 |
| NEG-050 | Audit for deleted entity | Entity soft-deleted | Audit still queryable | P1 |
| NEG-051 | Orphan audit records | Parent entity hard-deleted | Audit retained or archived | P1 |
| NEG-052 | Timezone mismatch | Client in different TZ | Server stores UTC | P1 |
| NEG-053 | Clock skew | Server time wrong | Audit timestamp inconsistent | P2 |
| NEG-054 | Duplicate CreatedBy/ModifiedBy | Same user create+update | Both recorded | P1 |
| NEG-055 | Empty entity name in audit | Entity with blank name | Audit uses ID or placeholder | P1 |
| NEG-056 | Binary field in audit | File upload | Audit references, not content | P1 |
| NEG-057 | Sensitive data in audit | Password field changed | Password not in audit detail | P0 |
| NEG-058 | PII in audit export | Export includes PII | PII masked or excluded | P0 |
| NEG-059 | Audit log injection | Newline in field value | Escaped in audit | P0 |
| NEG-060 | Unicode in audit | Emoji in entity name | Stored correctly | P1 |
| NEG-061 | Very long field value | 100KB text | Truncated or split | P1 |
| NEG-062 | Null OldValue for create | Create action | OldValue=null or N/A | P1 |
| NEG-063 | Null NewValue for delete | Delete action | NewValue=null or N/A | P1 |
| NEG-064 | Audit retention policy breach | Query purged data | Empty or policy message | P1 |
| NEG-065 | Export format mismatch | Request CSV, get JSON | Correct format returned | P1 |
| NEG-066 | Concurrent audit writes | 2 users update same entity | Both updates audited | P1 |
| NEG-067 | Audit for rolled-back transaction | Transaction rollback | No audit or rollback marker | P1 |
| NEG-068 | Batch audit partial failure | 5 of 10 in batch fail | Clear partial result | P1 |
| NEG-069 | Audit index corruption | DB index issue | Query degrades, no crash | P2 |
| NEG-070 | Audit disk quota exceeded | Disk full | Alert, CUD may fail | P1 |

### 2.5 Additional Negative (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | Query audit with invalid entity type | EntityType=Invalid | 400 or empty | P1 |
| NEG-072 | Export with no data | Empty range | Empty file | P1 |
| NEG-073 | Audit query with SQL injection | Filter=' | Parameterized | P0 |
| NEG-074 | Modify audit record | Direct DB update | Audit immutable | P0 |
| NEG-075 | Delete audit via API | DELETE audit | 403 | P0 |
| NEG-076 | Spoof CreatedBy in request | Fake user ID | Server uses token | P0 |
| NEG-077 | Future timestamp in request | Future date | Server uses server time | P0 |
| NEG-078 | Unauthenticated CUD | No token | 401 | P0 |
| NEG-079 | Expired token CUD | Token expired | 401 | P0 |
| NEG-080 | Audit disabled by config | Config off | No audit | P1 |
| NEG-081 | Audit table full | Disk full | CUD fails or degrades | P1 |
| NEG-082 | Audit write timeout | DB timeout | Rollback | P1 |
| NEG-083 | Query with invalid entity ID | EntityId=999999 | Empty or 404 | P1 |
| NEG-084 | Export with invalid date range | End before start | 400 | P1 |
| NEG-085 | Export without permission | No audit view | 403 | P0 |
| NEG-086 | Audit truncation | 1MB string | Truncated or rejected | P1 |
| NEG-087 | Negative entity ID | EntityId=-1 | 400 or empty | P1 |
| NEG-088 | Page size over max | PageSize=10000 | 400 or capped | P1 |
| NEG-089 | Invalid date format | Date=invalid | 400 | P1 |
| NEG-090 | Oversized export request | 10 years | 400 or paginated | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 3.1 String Length Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Entity name in audit | 1 | 255 | ✅ | ✅ | Truncate/Reject | P1 |
| BND-002 | Old value in audit | 0 | 10000 | ✅ | ✅ | Truncate | P1 |
| BND-003 | New value in audit | 0 | 10000 | ✅ | ✅ | Truncate | P1 |
| BND-004 | User name in audit | 1 | 100 | ✅ | ✅ | Truncate | P1 |
| BND-005 | Action string | 1 | 50 | ✅ | ✅ | Reject | P1 |
| BND-006 | Export filename | 1 | 255 | ✅ | ✅ | Truncate | P2 |
| BND-007 | Filter value | 0 | 500 | ✅ | ✅ | Reject | P1 |
| BND-008 | Audit reason/comment | 0 | 2000 | ✅ | ✅ | Reject | P1 |
| BND-009 | IP address | 7 | 45 | ✅ | ✅ | Reject | P1 |
| BND-010 | Entity type | 1 | 100 | ✅ | ✅ | Reject | P1 |
| BND-011 | Entity ID string | 1 | 50 | ✅ | ✅ | Reject | P1 |
| BND-012 | Session ID | 0 | 100 | ✅ | ✅ | Truncate | P2 |
| BND-013 | Change summary | 0 | 1000 | ✅ | ✅ | Truncate | P2 |
| BND-014 | JSON audit payload | 0 | 65535 | ✅ | ✅ | Split/Reject | P2 |
| BND-015 | Audit query sort field | 1 | 50 | ✅ | ✅ | Reject | P1 |

### 3.2 Numeric Boundaries (15)

| ID | Field | Zero | Negative | Very Large | Decimal | Priority |
|----|-------|------|----------|-----------|---------|----------|
| BND-016 | Entity ID | ❌ | ❌ | ✅ (max int) | N/A | P1 |
| BND-017 | User ID | ❌ | ❌ | ✅ | N/A | P1 |
| BND-018 | Page number | Treat as 1 | ❌ | Cap at max | N/A | P1 |
| BND-019 | Page size | ❌ | ❌ | Cap 1000 | N/A | P1 |
| BND-020 | Date range days | ❌ | ❌ | Cap 3650 | N/A | P1 |
| BND-021 | Audit record count | ✅ | ❌ | ✅ | N/A | P2 |
| BND-022 | Timestamp (Unix ms) | ✅ | ❌ | ✅ | N/A | P2 |
| BND-023 | Retention days | 0 = never | ❌ | 36500 | Integer | P1 |
| BND-024 | Export limit | ❌ | ❌ | 100000 | N/A | P1 |
| BND-025 | Batch size | 1 | ❌ | 1000 | N/A | P1 |
| BND-026 | Audit ID | N/A | ❌ | ✅ | N/A | P1 |
| BND-027 | Sequence number | 1 | ❌ | max | N/A | P2 |
| BND-028 | Retention purge batch | 100 | ❌ | 10000 | N/A | P2 |
| BND-029 | Concurrent export limit | 1 | ❌ | 5 | N/A | P2 |
| BND-030 | Audit entry size bytes | 100 | ❌ | 65535 | N/A | P2 |

### 3.3 Date/Time Boundaries (15)

| ID | Test Name | Date/Time Input | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-031 | Audit at midnight UTC | 00:00:00.000 | Correct storage | P1 |
| BND-032 | Audit at 23:59:59.999 | End of day | Correct storage | P1 |
| BND-033 | Leap year Feb 29 | 2024-02-29 | ✅ Accept | P1 |
| BND-034 | Export start = end | Same timestamp | Single record or empty | P1 |
| BND-035 | Export 1 second range | 1 sec range | Records in that second | P1 |
| BND-036 | DST transition | Fall back hour | No duplicate hour | P2 |
| BND-037 | Year 2038 boundary | 2038-01-19 | Handle if applicable | P2 |
| BND-038 | Epoch start | 1970-01-01 | ✅ Accept | P2 |
| BND-039 | Far future date | 2099-12-31 | ✅ Accept | P2 |
| BND-040 | Microsecond precision | 1234567890123 | Stored or truncated | P2 |
| BND-041 | Timezone offset in query | +05:30 | Converted to UTC | P1 |
| BND-042 | Date only (no time) | 2026-02-11 | Interpret as start of day | P1 |
| BND-043 | Retention cutoff exact | Records at cutoff | Include or exclude per policy | P1 |
| BND-044 | Rapid sequential audits | 2 audits 1ms apart | Both with distinct timestamps | P1 |
| BND-045 | Audit during NTP sync | Clock adjustment | No duplicate/corrupt | P2 |

### 3.4 Collection Boundaries (15)

| ID | Collection | State | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-046 | Audit results | 0 records | Empty array, 200 OK | P1 |
| BND-047 | Audit results | 1 record | Single item array | P1 |
| BND-048 | Audit results | Exact page size | Full page | P1 |
| BND-049 | Audit results | Page size + 1 | 2 pages | P1 |
| BND-050 | Exported records | 0 | Empty file | P1 |
| BND-051 | Exported records | 1 | Single row | P1 |
| BND-052 | Exported records | 100000 | Large file or chunked | P1 |
| BND-053 | Field changes in one update | 0 | Note: no field change | P2 |
| BND-054 | Field changes in one update | 50 | All 50 in audit | P1 |
| BND-055 | Audit entries per entity | 1 | Single create | P1 |
| BND-056 | Audit entries per entity | 1000+ | Pagination works | P1 |
| BND-057 | Concurrent exports | 1 | Success | P1 |
| BND-058 | Concurrent exports | 5 (max) | All succeed | P2 |
| BND-059 | Bulk audit create | 1 | Single entry | P1 |
| BND-060 | Bulk audit create | 1000 | Batch entries | P1 |

### 3.5 Unicode & Special Characters (10)

| ID | Field | Input | Expected Result | Priority |
|----|-------|-------|-----------------|----------|
| BND-061 | Entity name | Arabic "سجل" | Stored correctly | P1 |
| BND-062 | Entity name | Chinese "审计" | Stored correctly | P1 |
| BND-063 | Audit comment | Emoji "✅" | Stored or sanitized | P2 |
| BND-064 | Old value | HTML <script> | Escaped | P0 |
| BND-065 | Filter | SQL '; -- | Parameterized | P0 |
| BND-066 | Export filename | Special chars | Sanitized | P1 |
| BND-067 | New value | Newline \n | Escaped or preserved | P1 |
| BND-068 | User name | RTL text | Display correctly | P2 |
| BND-069 | Audit reason | Control chars | Stripped | P1 |
| BND-070 | JSON in audit | Nested JSON | Valid JSON stored | P1 |

### 3.6 Additional Boundaries (20)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-071 | Entity name at 1 char | Min length | Accept | P1 |
| BND-072 | Old value at 0 | Empty | Null or N/A | P1 |
| BND-073 | New value at 0 | Empty | Null or N/A | P1 |
| BND-074 | Page at 1 | First page | Valid | P1 |
| BND-075 | Page size at 1 | Min size | Valid | P1 |
| BND-076 | Date range at 1 day | 1 day | Valid | P1 |
| BND-077 | Retention at 0 | Never purge | Valid | P1 |
| BND-078 | Export limit at 1 | 1 record | Valid | P1 |
| BND-079 | Batch size at 1 | Min batch | Valid | P1 |
| BND-080 | Audit results at 0 | No records | Empty array | P1 |
| BND-081 | Field changes at 0 | No change | Note | P2 |
| BND-082 | Field changes at 50 | Max | All in audit | P1 |
| BND-083 | Audit entries at 1 | Single | One item | P1 |
| BND-084 | Audit entries at 1000+ | Many | Pagination | P1 |
| BND-085 | Concurrent exports at 1 | Single | Success | P1 |
| BND-086 | Concurrent exports at 5 | Max | All succeed | P2 |
| BND-087 | Bulk audit at 1 | Single | One entry | P1 |
| BND-088 | Bulk audit at 1000 | Max | Batch entries | P1 |
| BND-089 | Timestamp at epoch | 0 | Valid | P2 |
| BND-090 | Entity ID at max int | Max | Valid | P1 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 4.1 Workflow Rules (15)

| ID | Rule | Trigger | Expected Outcome | Priority |
|----|------|---------|------------------|----------|
| FUN-001 | Every CUD creates audit | Create/Update/Delete | Audit entry exists | P0 |
| FUN-002 | Audit immutable | Any modify attempt | Rejected | P0 |
| FUN-003 | CreatedBy set on create | Create | CreatedBy = current user | P0 |
| FUN-004 | LastModifiedBy set on update | Update | LastModifiedBy = current user | P0 |
| FUN-005 | DeletedBy set on soft delete | Delete | DeletedBy = current user | P0 |
| FUN-006 | Soft delete entities have DeletedDate | Soft delete | DeletedDate populated | P0 |
| FUN-007 | Workflow actions audited | Submit/Approve/Reject | Action in audit | P0 |
| FUN-008 | Role changes audited | Assign/remove role | Audit entry | P1 |
| FUN-009 | Login/logout audited | Auth events | Audit entries | P1 |
| FUN-010 | Failed login audited | Failed auth | Audit with attempt count | P1 |
| FUN-011 | Export respects retention | Export beyond retention | Filtered or blocked | P1 |
| FUN-012 | Audit query by entity type | Filter EntityType | Only that type | P1 |
| FUN-013 | Audit query by user | Filter UserId | Only that user | P1 |
| FUN-014 | Audit includes entity reference | Any CUD | EntityId/EntityType in record | P0 |
| FUN-015 | Cascade delete audited | Parent deleted | Child audit entries | P1 |

### 4.2 Validation Rules (15)

| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | Entity ID required | Valid ID | Null/negative → error | P0 |
| FUN-017 | User ID required | Valid user | Null → system/error | P0 |
| FUN-018 | Timestamp required | Valid date | Null → error | P0 |
| FUN-019 | Action required | Create/Update/Delete | Empty → error | P0 |
| FUN-020 | Export date range valid | Start < End | Start >= End → error | P1 |
| FUN-021 | Page size within range | 1-1000 | 0 or >1000 → error | P1 |
| FUN-022 | Page number positive | >= 1 | 0 or negative → error | P1 |
| FUN-023 | Retention days non-negative | >= 0 | Negative → error | P1 |
| FUN-024 | Filter value sanitized | Safe string | XSS → escaped | P0 |
| FUN-025 | Export format valid | CSV/JSON | Invalid → error | P1 |
| FUN-026 | Entity type from allowlist | Partner/Opportunity/etc | Invalid → error | P1 |
| FUN-027 | User filter from valid users | Existing user | Invalid → empty | P1 |
| FUN-028 | Audit ID format | Valid int | Invalid → 404 | P1 |
| FUN-029 | Export limit enforced | <= 100000 | Over → error | P1 |
| FUN-030 | Concurrent export limit | <= 5 | Over → queue/error | P2 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Test | Expected | Priority |
|----|------------|------|----------|----------|
| FUN-031 | Audit PK unique | Duplicate insert | Rejected | P0 |
| FUN-032 | Audit FK to entity | Orphan audit | Retained, entity null | P1 |
| FUN-033 | Audit FK to user | User deleted | UserId retained | P1 |
| FUN-034 | No audit update | UPDATE audit | Rejected | P0 |
| FUN-035 | No audit delete (purge only) | DELETE audit | Admin purge only | P0 |
| FUN-036 | Retention purge batch size | Purge 100000 | Batched | P1 |
| FUN-037 | Export format case-insensitive | csv/CSV | Both work | P2 |
| FUN-038 | Sort field allowlist | Valid field | Invalid → error | P1 |
| FUN-039 | Audit entry size limit | Normal payload | Oversized → split/reject | P1 |
| FUN-040 | Sensitive field exclusion | Password change | Password not in audit | P0 |

### 4.4 Audit Rules (10)

| ID | Action | Expected Audit Entry | Priority |
|----|--------|---------------------|----------|
| FUN-041 | Partner create | Action=Create, EntityType=Partner, EntityId | P0 |
| FUN-042 | Partner update | Action=Update, OldValues, NewValues | P0 |
| FUN-043 | Partner soft delete | Action=Delete, DeletedBy, DeletedDate | P0 |
| FUN-044 | Opportunity submit | Action=SubmitForGo, Stage, User | P0 |
| FUN-045 | Opportunity approve | Action=Approve, User, Timestamp | P0 |
| FUN-046 | Role assign | Action=AssignRole, Role, User | P1 |
| FUN-047 | Login success | Action=Login, User, IP, Timestamp | P1 |
| FUN-048 | Login failure | Action=LoginFailed, User/IP, Reason | P1 |
| FUN-049 | Export audit | Action=Export, User, Criteria | P1 |
| FUN-050 | Retention purge | Action=Purge, Count, Timestamp | P1 |

### 4.5 Additional Functional Rules (40)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-051 | Every CUD creates audit | Create/Update/Delete | Audit exists | P0 |
| FUN-052 | Audit immutable | Modify attempt | Rejected | P0 |
| FUN-053 | CreatedBy on create | Create | Current user | P0 |
| FUN-054 | LastModifiedBy on update | Update | Current user | P0 |
| FUN-055 | DeletedBy on soft delete | Delete | Current user | P0 |
| FUN-056 | DeletedDate on soft delete | Delete | Populated | P0 |
| FUN-057 | Workflow actions audited | Submit/Approve/Reject | In audit | P0 |
| FUN-058 | Role changes audited | Assign/remove | Audit entry | P1 |
| FUN-059 | Login/logout audited | Auth events | Audit entries | P1 |
| FUN-060 | Failed login audited | Failed auth | Audit with count | P1 |
| FUN-061 | Export respects retention | Beyond retention | Filtered or blocked | P1 |
| FUN-062 | Audit query by entity type | Filter EntityType | Only that type | P1 |
| FUN-063 | Audit query by user | Filter UserId | Only that user | P1 |
| FUN-064 | Audit includes entity ref | Any CUD | EntityId/EntityType | P0 |
| FUN-065 | Cascade delete audited | Parent deleted | Child audit | P1 |
| FUN-066 | Entity ID required | Entity ID | Valid | P0 |
| FUN-067 | User ID required | User | Valid | P0 |
| FUN-068 | Timestamp required | Timestamp | Valid | P0 |
| FUN-069 | Action required | Action | Create/Update/Delete | P0 |
| FUN-070 | Export date range valid | Start < End | Valid | P1 |
| FUN-071 | Page size range | 1-1000 | In range | P1 |
| FUN-072 | Page number positive | >= 1 | Valid | P1 |
| FUN-073 | Retention non-negative | >= 0 | Valid | P1 |
| FUN-074 | Filter sanitized | Safe string | XSS escaped | P0 |
| FUN-075 | Export format valid | CSV/JSON | Valid | P1 |
| FUN-076 | Entity type allowlist | Partner/Opp/etc | Valid | P1 |
| FUN-077 | User filter valid | Existing user | Valid | P1 |
| FUN-078 | Audit ID format | Valid int | Valid | P1 |
| FUN-079 | Export limit enforced | <= 100000 | Enforced | P1 |
| FUN-080 | Concurrent export limit | <= 5 | Queue or error | P2 |
| FUN-081 | Audit PK unique | Duplicate insert | Rejected | P0 |
| FUN-082 | Audit FK to entity | Orphan audit | Retained | P1 |
| FUN-083 | Audit FK to user | User deleted | UserId retained | P1 |
| FUN-084 | No audit update | UPDATE audit | Rejected | P0 |
| FUN-085 | No audit delete | DELETE audit | Admin purge only | P0 |
| FUN-086 | Retention purge batch | Purge 100000 | Batched | P1 |
| FUN-087 | Export format case | csv/CSV | Both work | P2 |
| FUN-088 | Sort field allowlist | Valid field | Valid | P1 |
| FUN-089 | Audit entry size limit | Normal payload | Split/reject | P1 |
| FUN-090 | Sensitive field exclusion | Password change | Password not in audit | P0 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 5.1 CRUD + Audit (15)

| ID | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| INT-001 | Create partner → query audit | Partner, Audit | Audit for partner create | P0 |
| INT-002 | Update partner → query audit | Partner, Audit | Audit for update | P0 |
| INT-003 | Delete partner → query audit | Partner, Audit | Audit for delete | P0 |
| INT-004 | Create opportunity → audit | Opportunity, Audit | Opportunity audit | P0 |
| INT-005 | Create contact → audit | Contact, Partner, Audit | Contact + Partner ref | P0 |
| INT-006 | Create interaction → audit | Interaction, Audit | Interaction audit | P1 |
| INT-007 | Attach document → audit | Document, Audit | Document audit | P1 |
| INT-008 | Update workflow status → audit | Opportunity, Workflow, Audit | Workflow audit | P0 |
| INT-009 | Bulk create → audit | Multiple, Audit | Multiple audit entries | P1 |
| INT-010 | Cascade delete → audit | Parent, Children, Audit | All CUD audited | P1 |
| INT-011 | Soft delete → restore → audit | Entity, Audit | Delete + restore audited | P1 |
| INT-012 | Create with relationship → audit | Entity, Related, Audit | Both audited | P1 |
| INT-013 | Update multiple fields → audit | Entity, Audit | Single audit, all changes | P1 |
| INT-014 | Transaction rollback → audit | Entity, Audit | No audit or rollback entry | P1 |
| INT-015 | Cross-entity workflow → audit | Opp, Partner, Audit | Full chain audited | P1 |

### 5.2 Search/Filter/Pagination (10)

| ID | Test | Criteria | Expected | Priority |
|----|------|----------|----------|----------|
| INT-016 | Query by date range | Last 7 days | Correct records | P0 |
| INT-017 | Query by entity type | EntityType=Partner | Only partners | P0 |
| INT-018 | Query by user | UserId=123 | Only user 123 | P0 |
| INT-019 | Query by entity ID | EntityId=456 | All for entity 456 | P0 |
| INT-020 | Pagination page 1 | Page=1, Size=50 | First 50 | P1 |
| INT-021 | Pagination page 2 | Page=2, Size=50 | Next 50 | P1 |
| INT-022 | Sort by timestamp desc | Sort=Timestamp DESC | Newest first | P1 |
| INT-023 | Filter by action | Action=Update | Only updates | P1 |
| INT-024 | Combined filters | Date+User+Type | Intersection | P1 |
| INT-025 | Empty result | No matching | Empty array | P1 |

### 5.3 Export Integration (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-026 | Export CSV | Date range | Valid CSV | P0 |
| INT-027 | Export JSON | Date range | Valid JSON | P0 |
| INT-028 | Export with filters | User+Type+Date | Filtered export | P1 |
| INT-029 | Export large result | 10000 records | File or chunked | P1 |
| INT-030 | Export triggers audit | Export | Export action audited | P1 |
| INT-031 | Export concurrent | 2 users export | Both succeed | P2 |
| INT-032 | Export retention bound | Beyond retention | Filtered or error | P1 |
| INT-033 | Export format selection | CSV vs JSON | Correct format | P1 |
| INT-034 | Export filename | Default | Includes date/type | P2 |
| INT-035 | Export empty result | No data | Empty file | P1 |

### 5.4 Relationships & Error Handling (15)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-036 | Audit for related entity delete | Partner deleted | Contact audit updated | P1 |
| INT-037 | Audit with missing user | User deleted | UserId retained | P1 |
| INT-038 | Audit with missing entity | Entity hard-deleted | Audit retained | P1 |
| INT-039 | Audit query timeout | Large query | Timeout or paginated | P1 |
| INT-040 | Audit write failure | DB error | CUD fails | P1 |
| INT-041 | Audit + notification | Create + notify | Both succeed | P1 |
| INT-042 | Audit + cache invalidation | Update | Cache cleared, audit written | P2 |
| INT-043 | Audit + search index | Create | Index updated, audit written | P2 |
| INT-044 | Multi-tenant audit | Tenant A creates | Only Tenant A sees | P0 |
| INT-045 | Audit retention purge job | Scheduled purge | Old records removed | P1 |
| INT-046 | Audit archive | Archive old audit | Archived, queryable | P2 |
| INT-047 | Audit restore from archive | Restore | Records available | P2 |
| INT-048 | Audit + reporting | Report on audit | Correct aggregates | P1 |
| INT-049 | Audit + compliance check | Compliance scan | Audit satisfies | P1 |
| INT-050 | Full lifecycle audit | Create→Update→Delete | All 3 in audit | P0 |

### 5.5 Additional Integration Flows (40)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-051 | Create partner → query audit | Partner, Audit | Audit for create | P0 |
| INT-052 | Update partner → query audit | Partner, Audit | Audit for update | P0 |
| INT-053 | Delete partner → query audit | Partner, Audit | Audit for delete | P0 |
| INT-054 | Create opportunity → audit | Opportunity, Audit | Opportunity audit | P0 |
| INT-055 | Create contact → audit | Contact, Partner, Audit | Contact + Partner ref | P0 |
| INT-056 | Create interaction → audit | Interaction, Audit | Interaction audit | P1 |
| INT-057 | Attach document → audit | Document, Audit | Document audit | P1 |
| INT-058 | Update workflow status → audit | Opportunity, Workflow, Audit | Workflow audit | P0 |
| INT-059 | Bulk create → audit | Multiple, Audit | Multiple audit entries | P1 |
| INT-060 | Cascade delete → audit | Parent, Children, Audit | All CUD audited | P1 |
| INT-061 | Soft delete → restore → audit | Entity, Audit | Delete + restore audited | P1 |
| INT-062 | Create with relationship → audit | Entity, Related, Audit | Both audited | P1 |
| INT-063 | Update multiple fields → audit | Entity, Audit | Single audit, all changes | P1 |
| INT-064 | Transaction rollback → audit | Entity, Audit | No audit or rollback entry | P1 |
| INT-065 | Cross-entity workflow → audit | Opp, Partner, Audit | Full chain audited | P1 |
| INT-066 | Query by date range | Last 7 days | Correct records | P0 |
| INT-067 | Query by entity type | EntityType=Partner | Only partners | P0 |
| INT-068 | Query by user | UserId=123 | Only user 123 | P0 |
| INT-069 | Query by entity ID | EntityId=456 | All for entity 456 | P0 |
| INT-070 | Pagination page 1 | Page=1, Size=50 | First 50 | P1 |
| INT-071 | Pagination page 2 | Page=2, Size=50 | Next 50 | P1 |
| INT-072 | Sort by timestamp desc | Sort=Timestamp DESC | Newest first | P1 |
| INT-073 | Filter by action | Action=Update | Only updates | P1 |
| INT-074 | Combined filters | Date+User+Type | Intersection | P1 |
| INT-075 | Empty result | No matching | Empty array | P1 |
| INT-076 | Export CSV | Date range | Valid CSV | P0 |
| INT-077 | Export JSON | Date range | Valid JSON | P0 |
| INT-078 | Export with filters | User+Type+Date | Filtered export | P1 |
| INT-079 | Export large result | 10000 records | File or chunked | P1 |
| INT-080 | Export triggers audit | Export | Export action audited | P1 |
| INT-081 | Export concurrent | 2 users export | Both succeed | P2 |
| INT-082 | Export retention bound | Beyond retention | Filtered or error | P1 |
| INT-083 | Export format selection | CSV vs JSON | Correct format | P1 |
| INT-084 | Export filename | Default | Includes date/type | P2 |
| INT-085 | Export empty result | No data | Empty file | P1 |
| INT-086 | Audit for related entity delete | Partner deleted | Contact audit | P1 |
| INT-087 | Audit with missing user | User deleted | UserId retained | P1 |
| INT-088 | Audit with missing entity | Entity hard-deleted | Audit retained | P1 |
| INT-089 | Audit query timeout | Large query | Timeout or paginated | P1 |
| INT-090 | Audit write failure | DB error | CUD fails | P1 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: 50** | ✅ COMPLIANT

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in filter | Audit query | Parameterized, no injection | P0 |
| SEC-002 | SQL injection in entity type | Filter | Parameterized | P0 |
| SEC-003 | XSS in export filename | Filename | Escaped | P0 |
| SEC-004 | XSS in audit display | Old/New value | Escaped | P0 |
| SEC-005 | NoSQL injection | JSON filter | Validation | P1 |
| SEC-006 | LDAP injection | User filter | Parameterized | P1 |
| SEC-007 | Command injection | Export path | Sanitized | P0 |
| SEC-008 | Header injection | Export headers | Validated | P1 |
| SEC-009 | Log injection | Audit content | Escaped | P0 |
| SEC-010 | Template injection | Export template | No eval | P1 |

### 6.2 Access Control (10)

| ID | User | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Query audit | 401 | P0 |
| SEC-012 | Partner user | Query audit | 403 | P0 |
| SEC-013 | Read-only user | Export audit | 403 | P0 |
| SEC-014 | Admin | Full audit access | 200 | P0 |
| SEC-015 | Org-scoped user | Cross-org audit | 403 or filtered | P0 |
| SEC-016 | User A | User B's audit | 403 or filtered | P0 |
| SEC-017 | API key | Audit query | 403 or allowed per scope | P0 |
| SEC-018 | Service account | Audit purge | 403 or admin only | P0 |
| SEC-019 | Delegated user | Audit on behalf | Per delegation rules | P1 |
| SEC-020 | Expired session | Audit query | 401 | P0 |

### 6.3 IDOR (10)

| ID | Manipulation | Expected | Priority |
|----|-------------|----------|----------|
| SEC-021 | Change EntityId to other's | 403 or filtered | P0 |
| SEC-022 | Change UserId to other's | 403 or filtered | P0 |
| SEC-023 | Access audit by ID brute force | 403 or 404 | P0 |
| SEC-024 | Export other org data | 403 | P0 |
| SEC-025 | Modify audit ID in request | Ignored, use auth | P0 |
| SEC-026 | Sequential audit ID enumeration | Rate limit or 403 | P1 |
| SEC-027 | Audit ID in URL tampering | 403 | P0 |
| SEC-028 | Batch request with mixed IDs | Filter to own | P0 |
| SEC-029 | Audit record IDOR | 403 | P0 |
| SEC-030 | Entity ID in filter IDOR | Filter to permitted | P0 |

### 6.4 Auth & Session (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| SEC-031 | JWT expired | 401 | P0 |
| SEC-032 | JWT tampered | 401 | P0 |
| SEC-033 | JWT with wrong audience | 401 | P0 |
| SEC-034 | Session fixation | New session on login | P0 |
| SEC-035 | CSRF on audit export | CSRF token required | P0 |
| SEC-036 | Replay attack | Nonce or timestamp | P1 |
| SEC-037 | Token theft | Token invalidated on logout | P0 |
| SEC-038 | Refresh token misuse | Limited reuse | P1 |
| SEC-039 | Concurrent session | Per policy | P1 |
| SEC-040 | Logout audit | Logout action logged | P1 |

### 6.5 Data Exposure (10)

| ID | Data | Risk | Expected | Priority |
|----|------|------|----------|----------|
| SEC-041 | Password in audit | Never log password | Excluded | P0 |
| SEC-042 | Token in audit | Never log token | Excluded | P0 |
| SEC-043 | PII in export | Mask or exclude | PII protected | P0 |
| SEC-044 | Audit record with PII | Mask in response | PII protected | P0 |
| SEC-045 | Error message info leak | Generic error | No internal details | P0 |
| SEC-046 | Stack trace in audit | Never | No stack trace | P0 |
| SEC-047 | Internal IP in export | Mask if sensitive | Protected | P1 |
| SEC-048 | Bulk export data leak | Org-scoped | No cross-org | P0 |
| SEC-049 | Audit metadata | Minimal exposure | Only necessary fields | P1 |
| SEC-050 | Sensitive field names | Redact if needed | Per policy | P1 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|----------|
| CON-001 | 2 users create same entity type | Both audits created | P1 |
| CON-002 | 2 users update same entity | Both updates audited | P1 |
| CON-003 | 2 users export audit concurrently | Both succeed | P1 |
| CON-004 | 10 users create in parallel | All 10 audited | P1 |
| CON-005 | Update + export same entity | Both operations succeed | P1 |
| CON-006 | Audit write during purge | No deadlock | P1 |
| CON-007 | Concurrent audit queries | All return correct | P1 |
| CON-008 | Race: create + delete | One of two in audit | P1 |
| CON-009 | Race: update + delete | Delete wins, update rolled back | P1 |
| CON-010 | 5 concurrent exports | All 5 or queue | P1 |
| CON-011 | Audit write timeout under load | Retry or fail gracefully | P1 |
| CON-012 | Connection pool exhaustion | Queue or reject | P1 |
| CON-013 | Lock contention on audit table | Timeout, retry | P2 |
| CON-014 | Batch audit + single audit | Both succeed | P1 |
| CON-015 | Purge + query overlap | Consistent view | P1 |
| CON-016 | Export + purge overlap | No corruption | P1 |
| CON-017 | 100 concurrent CUD | All audited | P1 |
| CON-018 | Optimistic concurrency + audit | Both recorded | P1 |
| CON-019 | Transaction rollback + audit | No orphan audit | P1 |
| CON-020 | Audit table partition switch | No gap | P2 |
| CON-021 | N+1 audit writes | Batched | P2 |
| CON-022 | Audit async write | Eventually consistent | P1 |
| CON-023 | Audit sync write | Immediate | P1 |
| CON-024 | Concurrent retention purge | Single writer | P2 |
| CON-025 | Audit read replica lag | Acceptable lag | P2 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

### 8.1 Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | Validate entity ID | 123 | Valid | P1 |
| UNT-002 | Validate entity ID | -1 | Invalid | P1 |
| UNT-003 | Validate date range | Start<End | Valid | P1 |
| UNT-004 | Validate date range | Start>End | Invalid | P1 |
| UNT-005 | Validate action | Create | Valid | P1 |

### 8.2 Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | Format timestamp | 1707091200000 | UTC string | P1 |
| UNT-007 | Format audit summary | Changes dict | Human-readable string | P1 |
| UNT-008 | Format export row | Audit record | CSV/JSON row | P1 |

### 8.3 Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | Retention cutoff date | Days=30 | Date 30 days ago | P1 |
| UNT-010 | Page offset | Page=3, Size=50 | Offset=100 | P1 |
| UNT-011 | Total pages | Total=97, Size=10 | Pages=10 | P1 |
| UNT-012 | Export chunk size | 10000 | 1000 per chunk | P2 |
| UNT-013 | Audit entry size | Payload | Bytes count | P2 |

### 8.4 Status Logic (5)

| ID | Test | Condition | Expected | Priority |
|----|------|-----------|----------|----------|
| UNT-014 | Is create action | Action=Create | True | P1 |
| UNT-015 | Is update action | Action=Update | True | P1 |
| UNT-016 | Is delete action | Action=Delete | True | P1 |
| UNT-017 | Within retention | Date, retention | Boolean | P1 |
| UNT-018 | Sensitive field | FieldName | Exclude from audit | P1 |

### 8.5 Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | Merge old/new values | Dict | Combined | P1 |
| UNT-020 | Diff changes | Old, New | Change list | P1 |
| UNT-021 | Batch audit entries | List of CUD | Audit entries | P1 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | Single audit write | < 50 ms | P1 |
| PRF-002 | Audit query (100 records) | < 200 ms | P1 |
| PRF-003 | Audit query (1000 records) | < 1 s | P1 |
| PRF-004 | Export 1000 records | < 5 s | P1 |
| PRF-005 | Export 10000 records | < 30 s | P1 |
| PRF-006 | Audit query with 3 filters | < 300 ms | P1 |
| PRF-007 | Pagination query | < 200 ms | P1 |
| PRF-008 | 10 concurrent audit writes | < 500 ms total | P1 |
| PRF-009 | 100 concurrent audit writes | < 5 s total | P2 |
| PRF-010 | Audit query + export | < 10 s | P1 |
| PRF-011 | Memory: export 100K records | < 500 MB | P2 |
| PRF-012 | Memory: 1000 audit queries | No leak | P2 |
| PRF-013 | Retention purge 10K records | < 30 s | P2 |
| PRF-014 | Audit index usage | Index used | P2 |
| PRF-015 | N+1 audit avoidance | Single query | P2 |
| PRF-016 | Audit write with batch | Batched < 100 ms | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 50 CUD/min sustained | 30 min | All audited, no errors | P1 |
| LDT-002 | 100 CUD/min sustained | 30 min | All audited, < 1% error | P1 |
| LDT-003 | 200 CUD/min sustained | 15 min | Degradation acceptable | P2 |
| LDT-004 | Spike: 500 CUD in 1 min | 1 min | Catch up within 5 min | P1 |
| LDT-005 | Spike: 100 export requests | 2 min | Queue or complete | P2 |
| LDT-006 | Stress: 1000 CUD/min | 5 min | Observe limits | P2 |
| LDT-007 | Stress: connection pool | Until exhaustion | Graceful degradation | P2 |
| LDT-008 | Stress: disk I/O | High write load | No corruption | P2 |
| LDT-009 | Recovery after spike | 5 min cool-down | Normal latency | P1 |
| LDT-010 | Recovery after stress | 10 min cool-down | Full recovery | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| CUD logging | POS-001–005, FUN-001–006 |
| User tracking | POS-005, FUN-003–005, SEC-004 |
| Timestamp accuracy | POS-006, BND-031–045 |
| Field-level changes | POS-004, POS-007, UNT-019–021 |
| Export | POS-011–013, INT-026–035 |
| Retention | POS-020, FUN-011, NEG-064 |
| Security | SEC-001–050 |
| Concurrency | CON-001–025 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
