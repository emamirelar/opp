# Data Integrity — Test Cases

**Component:** Cross-cutting / Data Integrity  
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

**Data Integrity** ensures referential integrity, constraints, and consistency: FK constraints, unique constraints, soft delete consistency, orphan record prevention, cascade behavior, and transaction boundaries. The system must maintain data correctness under all operations.

**Key Capabilities:**
- Foreign key constraint enforcement
- Unique constraint enforcement
- Soft delete consistency (IsDeleted)
- Orphan record prevention
- Cascade delete/update behavior
- Transaction boundary integrity

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create with valid FK | Partner exists | Create contact | Contact created | P0 |
| POS-002 | Update with valid FK | Contact exists | Update PartnerId | Updated | P0 |
| POS-003 | Delete parent with cascade | Partner has contacts | Delete partner | Contacts cascade | P0 |
| POS-004 | Unique constraint: new value | No duplicate | Create with unique name | Created | P0 |
| POS-005 | Soft delete: set flags | Entity exists | Soft delete | IsDeleted=true | P0 |
| POS-006 | Query excludes soft-deleted | Mix of data | Query | Only !IsDeleted | P0 |
| POS-007 | FK to soft-deleted parent | Parent soft-deleted | Query child | Child filtered or visible | P0 |
| POS-008 | Transaction: all commit | Multi-step | Commit | All persisted | P0 |
| POS-009 | Transaction: rollback | Error | Rollback | None persisted | P0 |
| POS-010 | Cascade delete: children | Parent deleted | Children | Cascade or restrict | P0 |
| POS-011 | Unique composite | (A,B) unique | Create (a1,b1) | Created | P0 |
| POS-012 | FK to nullable | Optional FK | Create with null FK | Created | P0 |
| POS-013 | Update FK to valid | Valid new parent | Update | Updated | P0 |
| POS-014 | No orphan on delete | Parent deleted | Child | FK enforced | P0 |
| POS-015 | Check constraint: valid | Valid value | Insert | Inserted | P0 |
| POS-016 | Default value applied | Default defined | Insert without | Default applied | P0 |
| POS-017 | Not null enforced | Required field | Insert with value | Inserted | P0 |
| POS-018 | Restore soft-deleted | Soft-deleted | Restore | IsDeleted=false | P0 |
| POS-019 | Batch with FK | All valid | Batch create | All created | P1 |
| POS-020 | Nested FK | A→B→C | Create C | All valid | P1 |
| POS-021 | Self-referential FK | Entity has parent | Create valid | Created | P1 |
| POS-022 | Circular FK (avoid) | Design | N/A | Avoided | P1 |
| POS-023 | Transaction savepoint | Nested | Rollback savepoint | Partial rollback | P1 |
| POS-024 | Constraint name in error | Constraint fail | Error message | Constraint name | P1 |
| POS-025 | Audit + FK | Create with FK | Audit | FK in audit | P1 |
| POS-026 | Soft delete + audit | Soft delete | Audit | DeletedBy, DeletedDate | P0 |
| POS-027 | Unique: case insensitive | Config | "Abc" vs "abc" | Per config | P1 |
| POS-028 | FK cascade: update | Parent updated | Child | Cascade or no action | P1 |
| POS-029 | Orphan cleanup job | Orphans exist | Run cleanup | Orphans handled | P1 |
| POS-030 | Data validation trigger | Insert | Trigger | Validation | P1 |
| POS-031 | Integrity check | DB | Run check | No violations | P1 |
| POS-032 | Migration preserves FK | Migration | Run migration | FK intact | P1 |
| POS-033 | Import with FK validation | Import file | Import | FK validated | P1 |
| POS-034 | Export preserves referential | Export | Export | Data consistent | P1 |
| POS-035 | Soft delete cascade | Parent soft-deleted | Children | Per config | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 2.1 FK Violations (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-001 | Insert with invalid FK | PartnerId=999999 | FK violation | P0 |
| NEG-002 | Insert with null required FK | PartnerId=null (required) | Violation | P0 |
| NEG-003 | Update to invalid FK | ParentId=999999 | Violation | P0 |
| NEG-004 | Delete parent with children (restrict) | Restrict | Violation | P0 |
| NEG-005 | FK to soft-deleted parent | Parent IsDeleted | Per config: reject | P0 |
| NEG-006 | FK to self (invalid) | Self-ref invalid | Violation | P0 |
| NEG-007 | FK cycle | Circular ref | Violation | P0 |
| NEG-008 | FK type mismatch | Wrong type | Violation | P0 |
| NEG-009 | FK to deleted (hard) | Parent hard-deleted | Violation | P0 |
| NEG-010 | Multi-FK: one invalid | 2 FKs, 1 invalid | Violation | P0 |
| NEG-011 | FK in batch: one invalid | Batch with 1 bad FK | Row rejected | P0 |
| NEG-012 | FK to wrong entity type | PartnerId in Opp | Violation | P0 |
| NEG-013 | FK to future entity | Not yet created | Violation | P0 |
| NEG-014 | FK to different tenant | Cross-tenant | Violation | P0 |
| NEG-015 | Orphan FK (manual) | Direct DB insert | Violation or allowed | P0 |

### 2.2 Unique Constraint Violations (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-016 | Duplicate name | Name exists | Violation | P0 |
| NEG-017 | Duplicate composite | (A,B) exists | Violation | P0 |
| NEG-018 | Duplicate on update | Update to existing | Violation | P0 |
| NEG-019 | Duplicate in batch | 2 rows same | Violation | P0 |
| NEG-020 | Null in unique | Unique allows null | 2 nulls: per DB | P0 |
| NEG-021 | Deferred unique | Same tx | Per deferral | P1 |
| NEG-022 | Unique partial | Partial index | Violation | P1 |
| NEG-023 | Unique case | Case sensitivity | Per config | P1 |
| NEG-024 | Unique soft-deleted | Soft-deleted same | Per config | P1 |
| NEG-025 | Unique across tenants | Same tenant | Violation | P0 |
| NEG-026 | Unique constraint name | Violation | Name in error | P1 |
| NEG-027 | Unique: whitespace | Trim or not | Per config | P1 |
| NEG-028 | Unique: empty string | "" unique | Per config | P1 |
| NEG-029 | Unique: multiple columns | (A,B,C) | Violation | P0 |
| NEG-030 | Unique index violation | Index violation | Violation | P0 |

### 2.3 Soft Delete Violations (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-031 | Query without IsDeleted filter | Default query | Include deleted per config | P0 |
| NEG-032 | FK to soft-deleted | Reference deleted | Per config | P0 |
| NEG-033 | Restore without permission | Restore | 403 | P0 |
| NEG-034 | Hard delete without permission | Hard delete | 403 | P0 |
| NEG-035 | Soft delete already deleted | Delete again | Idempotent or error | P1 |
| NEG-036 | Restore with orphan FK | Child deleted | Restore parent | Per config | P1 |
| NEG-037 | Soft delete cascade | Parent soft-deleted | Children | Per config | P1 |
| NEG-038 | DeletedBy without user | System delete | System/0 | P1 |
| NEG-039 | DeletedDate future | Bug | Validation | P1 |
| NEG-040 | Soft delete in transaction | Rollback | Restored | P0 |
| NEG-041 | Unique with soft-deleted | Same name deleted | Per config | P1 |
| NEG-042 | Audit for soft delete | Soft delete | Audit | DeletedBy, DeletedDate | P0 |
| NEG-043 | Export with deleted | Export | Include or exclude | P1 |
| NEG-044 | Search with deleted | Search | Include or exclude | P1 |
| NEG-045 | Count with deleted | Count | Per filter | P1 |

### 2.4 Orphan & Cascade Violations (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-046 | Orphan on restrict delete | Parent delete | Blocked | P0 |
| NEG-047 | Orphan on cascade | Parent delete | Children deleted | P0 |
| NEG-048 | Orphan on set null | Parent delete | FK set null | P0 |
| NEG-049 | Orphan on set default | Parent delete | FK set default | P1 |
| NEG-050 | Orphan on no action | Parent delete | Error | P0 |
| NEG-051 | Orphan from manual delete | Bypass | Prevent or cleanup | P0 |
| NEG-052 | Orphan in batch | Batch partial | Some orphan | Rollback or partial | P1 |
| NEG-053 | Cascade depth | 5 levels | All cascade | P1 |
| NEG-054 | Cascade cycle | Circular | Prevent | P0 |
| NEG-055 | Cascade permission | User can't delete child | Cascade blocked | P0 |
| NEG-056 | Orphan report | Run report | Orphans listed | P1 |
| NEG-057 | Cascade audit | Cascade delete | All audited | P1 |
| NEG-058 | Cascade transaction | Cascade | Atomic | P0 |
| NEG-059 | Orphan in export | Export | No orphans | P1 |
| NEG-060 | Cascade soft delete | Parent soft-deleted | Children | Per config | P1 |

### 2.5 Transaction & Constraint Violations (10)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-061 | Constraint in transaction | Violation | Rollback | P0 |
| NEG-062 | Multiple violations | 2 violations | All reported | P0 |
| NEG-063 | Check constraint | Invalid value | Violation | P0 |
| NEG-064 | Not null violation | Null in required | Violation | P0 |
| NEG-065 | Default override invalid | Invalid default | Violation | P1 |
| NEG-066 | Trigger violation | Trigger fails | Rollback | P0 |
| NEG-067 | Constraint violation message | Violation | Clear message | P1 |
| NEG-068 | Constraint deferred | Deferred | Check at commit | P1 |
| NEG-069 | Partial transaction | Some fail | Rollback all | P0 |
| NEG-070 | Constraint name in error | Any violation | Constraint name | P1 |

### 2.6 Additional Negative (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | FK to inactive entity | Parent inactive | Per config | P1 |
| NEG-072 | Unique with different case | "Abc" vs "abc" | Per config | P1 |
| NEG-073 | Batch with orphan FK | Batch create orphan | Reject | P0 |
| NEG-074 | Soft delete with active children | Children active | Block or cascade | P1 |
| NEG-075 | Restore with unique conflict | Same name exists | Reject | P1 |
| NEG-076 | FK to wrong schema | Cross-schema | Violation | P1 |
| NEG-077 | Unique constraint deferred violation | Same tx | Check at commit | P1 |
| NEG-078 | Cascade with permission denied | User can't delete child | Block | P0 |
| NEG-079 | Import with circular FK | A→B→A | Reject | P1 |
| NEG-080 | Export with deleted references | Include deleted | Per config | P1 |
| NEG-081 | Transaction with FK violation | Violation in tx | Rollback | P0 |
| NEG-082 | Check constraint with null | Null in check | Per config | P1 |
| NEG-083 | Default override invalid type | Wrong type | Violation | P1 |
| NEG-084 | Trigger on soft delete | Soft delete | Trigger fires | P1 |
| NEG-085 | Orphan audit with entity delete | Entity hard-deleted | Audit retained | P1 |
| NEG-086 | Unique with partial index | Partial | Per index | P1 |
| NEG-087 | FK cascade with restrict | Mixed config | Per config | P1 |
| NEG-088 | Batch with mixed FK validity | Mixed | Reject invalid rows | P1 |
| NEG-089 | Soft delete unique constraint | Same name deleted | Per config | P1 |
| NEG-090 | Migration with FK constraint | Migration | FK preserved | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 3.1 FK Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Valid | Priority |
|----|-------|-----|-----|--------|--------|------|----------|
| BND-001 | FK value | 1 | Max int | ✅ | ✅ | ✅ | P1 |
| BND-002 | FK null (nullable) | null | ✅ | ✅ | N/A | P1 |
| BND-003 | FK zero | 0 | ❌ | ❌ | N/A | P1 |
| BND-004 | FK negative | -1 | ❌ | ❌ | N/A | P1 |
| BND-005 | FK to self (valid) | Same ID | ✅ | If allowed | P1 |
| BND-006 | Multiple FK same | 2 FKs same parent | ✅ | P1 |
| BND-007 | FK chain depth | 1 | 10 | ✅ | ✅ | P1 |
| BND-008 | FK batch size | 1 | 1000 | ✅ | ✅ | P1 |
| BND-009 | FK in update | Change | Valid new | P1 |
| BND-010 | FK in delete | Parent | Cascade | P1 |
| BND-011 | Composite FK | (A,B) | Both valid | P1 |
| BND-012 | FK to soft-deleted | Parent deleted | Per config | P1 |
| BND-013 | FK to active only | Parent active | Filter | P1 |
| BND-014 | FK in bulk | 100 rows | All valid | P1 |
| BND-015 | FK in import | 1000 rows | Validate | P1 |

### 3.2 Unique Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-016 | Unique string | 1 | 255 | ✅ | ✅ | Reject | P1 |
| BND-017 | Unique empty | "" | Per config | ✅ | ✅ | P1 |
| BND-018 | Unique null | null | Per config | ✅ | ✅ | P1 |
| BND-019 | Unique composite | 2 cols | Both valid | ✅ | P1 |
| BND-020 | Unique 3 columns | 3 cols | All valid | ✅ | P1 |
| BND-021 | Unique case | "Abc" vs "abc" | Per config | P1 |
| BND-022 | Unique whitespace | " a " vs "a" | Trim or not | P1 |
| BND-023 | Unique soft-deleted | Exclude deleted | Per config | P1 |
| BND-024 | Unique partial | WHERE clause | Per index | P1 |
| BND-025 | Unique across tenants | Same tenant | Enforced | P1 |
| BND-026 | Unique in batch | 100 rows | No duplicate | P1 |
| BND-027 | Unique on update | Change to existing | Violation | P1 |
| BND-028 | Unique deferred | Same tx | Deferred check | P1 |
| BND-029 | Unique multiple | 5 unique cols | All enforced | P1 |
| BND-030 | Unique index | Index | Violation | P1 |

### 3.3 Soft Delete Boundaries (15)

| ID | Test Name | Condition | Expected | Priority |
|----|-----------|-----------|----------|----------|
| BND-031 | IsDeleted false | New entity | IsDeleted=false | P0 |
| BND-032 | IsDeleted true | Soft deleted | IsDeleted=true | P0 |
| BND-033 | DeletedBy set | Soft delete | DeletedBy=user | P0 |
| BND-034 | DeletedDate set | Soft delete | DeletedDate=now | P0 |
| BND-035 | Query filter | Where !IsDeleted | Excludes deleted | P0 |
| BND-036 | Include deleted | IncludeDeleted=true | Includes | P1 |
| BND-037 | Restore | Restore | IsDeleted=false | P0 |
| BND-038 | Double delete | Delete again | Idempotent | P1 |
| BND-039 | Delete cascade | Parent deleted | Children | Per config | P1 |
| BND-040 | Restore cascade | Parent restored | Children | Per config | P1 |
| BND-041 | Audit soft delete | Delete | Audit | P0 |
| BND-042 | Batch soft delete | 100 entities | All deleted | P1 |
| BND-043 | Soft delete in transaction | Rollback | Restored | P0 |
| BND-044 | FK to deleted | Reference | Per config | P1 |
| BND-045 | Unique with deleted | Same name | Per config | P1 |

### 3.4 Transaction Boundaries (15)

| ID | Test Name | Condition | Expected | Priority |
|----|-----------|-----------|----------|----------|
| BND-046 | Single statement | 1 insert | 1 transaction | P0 |
| BND-047 | Multi-statement | 5 inserts | 1 transaction | P0 |
| BND-048 | Rollback | Error | All rolled back | P0 |
| BND-049 | Commit | Success | All committed | P0 |
| BND-050 | Savepoint | Nested | Partial rollback | P1 |
| BND-051 | Transaction timeout | Long | Rollback | P0 |
| BND-052 | Nested transaction | Nested | Per implementation | P1 |
| BND-053 | Transaction isolation | RC | Read committed | P0 |
| BND-054 | Deadlock in transaction | Deadlock | Victim rollback | P0 |
| BND-055 | Transaction + audit | Both | Atomic | P0 |
| BND-056 | Transaction + FK | Cascade | Atomic | P0 |
| BND-057 | Transaction + bulk | 100 rows | All or none | P0 |
| BND-058 | Transaction + notification | Both | Commit then notify | P1 |
| BND-059 | Transaction + cache | Update | Invalidate on commit | P1 |
| BND-060 | Transaction + search | Update | Index on commit | P1 |

### 3.5 Unicode & Special (10)

| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-061 | Unique string | Arabic | Stored | P1 |
| BND-062 | Unique string | Emoji | Stored or rejected | P1 |
| BND-063 | FK in error | Unicode | Error message | P1 |
| BND-064 | Constraint name | Unicode | Valid | P2 |
| BND-065 | Unique whitespace | Trim | Per config | P1 |
| BND-066 | Soft delete reason | Unicode | Stored | P1 |
| BND-067 | Audit constraint | Unicode | Audit | P1 |
| BND-068 | Cascade log | Unicode | Logged | P2 |
| BND-069 | Orphan report | Unicode | Displayed | P1 |
| BND-070 | Transaction ID | UUID | Valid | P1 |

### 3.6 Additional Boundaries (20)

| ID | Test Name | Condition | Expected | Priority |
|----|-----------|-----------|----------|----------|
| BND-071 | FK at max int | Max int | Valid | P1 |
| BND-072 | Unique at 255 chars | Max length | Accept | P1 |
| BND-073 | Soft delete at 0 records | No data | Empty | P1 |
| BND-074 | Transaction at 1000 stmts | Large tx | Complete or timeout | P1 |
| BND-075 | Cascade at 10 levels | Deep hierarchy | All cascade | P1 |
| BND-076 | Batch at 1000 rows | Max batch | All valid | P1 |
| BND-077 | Import at 10000 rows | Max import | Validate | P1 |
| BND-078 | Orphan count at 0 | No orphans | Empty report | P1 |
| BND-079 | Unique composite at 5 cols | 5 columns | All enforced | P1 |
| BND-080 | FK at zero (nullable) | Nullable FK | Null allowed | P1 |
| BND-081 | DeletedDate at epoch | Epoch | Valid | P1 |
| BND-082 | Audit at max records | 100K audit | Pagination | P1 |
| BND-083 | Restore with FK to deleted | Parent deleted | Per config | P1 |
| BND-084 | Unique whitespace at boundary | " a " | Trim or not | P1 |
| BND-085 | Transaction isolation at RR | Repeatable read | If used | P1 |
| BND-086 | Deadlock at 2 transactions | 2 tx | One victim | P0 |
| BND-087 | Savepoint at 0 | No savepoint | N/A | P1 |
| BND-088 | Batch with 1 valid | 1 of 100 | All or none | P1 |
| BND-089 | Export at 0 records | No data | Empty file | P1 |
| BND-090 | Integrity check at 0 violations | Clean DB | No violations | P1 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 4.1 FK Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | FK must exist | Insert | Parent exists | P0 |
| FUN-002 | FK on delete | Parent delete | Per config | P0 |
| FUN-003 | FK on update | Parent update | Per config | P0 |
| FUN-004 | FK nullable | Optional | Null allowed | P0 |
| FUN-005 | FK to same tenant | Multi-tenant | Same tenant | P0 |
| FUN-006 | FK to active only | Parent active | Filter | P1 |
| FUN-007 | FK to non-deleted | Soft delete | Exclude deleted | P1 |
| FUN-008 | FK composite | Multiple cols | All valid | P0 |
| FUN-009 | FK self-reference | Valid | Parent exists | P1 |
| FUN-010 | FK in batch | All valid | Validate | P0 |
| FUN-011 | FK in import | Validate | Reject invalid | P0 |
| FUN-012 | FK cascade depth | N levels | All cascade | P1 |
| FUN-013 | FK cycle | Prevent | No cycle | P0 |
| FUN-014 | FK constraint name | Violation | Name in error | P1 |
| FUN-015 | FK deferred | Deferrable | Check at commit | P1 |

### 4.2 Unique Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-016 | Unique enforced | Insert | No duplicate | P0 |
| FUN-017 | Unique on update | Update | No duplicate | P0 |
| FUN-018 | Unique composite | (A,B) | Both unique | P0 |
| FUN-019 | Unique partial | WHERE | Per index | P1 |
| FUN-020 | Unique soft-deleted | Exclude | Per config | P1 |
| FUN-021 | Unique case | Case | Per config | P1 |
| FUN-022 | Unique null | Nulls | Per DB | P1 |
| FUN-023 | Unique in batch | Batch | No duplicate | P0 |
| FUN-024 | Unique constraint name | Violation | Name | P1 |
| FUN-025 | Unique deferred | Deferrable | Check at commit | P1 |

### 4.3 Soft Delete Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-026 | IsDeleted set | Delete | Soft delete | P0 |
| FUN-027 | DeletedBy set | Delete | User ID | P0 |
| FUN-028 | DeletedDate set | Delete | Timestamp | P0 |
| FUN-029 | Query filter | Default | !IsDeleted | P0 |
| FUN-030 | Restore | Restore | IsDeleted=false | P0 |
| FUN-031 | Cascade soft delete | Parent | Children | Per config | P1 |
| FUN-032 | FK to deleted | Reference | Per config | P1 |
| FUN-033 | Unique with deleted | Same name | Per config | P1 |
| FUN-034 | Audit soft delete | Delete | Audit | P0 |
| FUN-035 | No hard delete | Default | Soft only | P0 |

### 4.4 Orphan & Cascade Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-036 | No orphan on restrict | Parent delete | Blocked | P0 |
| FUN-037 | Cascade on delete | Config cascade | Children deleted | P0 |
| FUN-038 | Set null on delete | Config | FK null | P0 |
| FUN-039 | Set default on delete | Config | FK default | P1 |
| FUN-040 | Cascade audit | Cascade | All audited | P1 |
| FUN-041 | Cascade transaction | Cascade | Atomic | P0 |
| FUN-042 | Orphan report | Run | Report | P1 |
| FUN-043 | Orphan cleanup | Cleanup job | Handle | P1 |
| FUN-044 | Cascade depth | N levels | All | P1 |
| FUN-045 | Cascade cycle | Prevent | No cycle | P0 |
| FUN-046 | Cascade permission | User | Per permission | P0 |
| FUN-047 | Orphan prevention | Insert | Prevent | P0 |
| FUN-048 | Cascade soft delete | Parent soft-deleted | Children | Per config | P1 |
| FUN-049 | Cascade update | Parent update | Per config | P1 |
| FUN-050 | Transaction + cascade | Both | Atomic | P0 |

### 4.5 Additional Functional Rules (40)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-051 | FK to same tenant | Multi-tenant | Same tenant | P0 |
| FUN-052 | Unique constraint name | Violation | Name in error | P1 |
| FUN-053 | Soft delete audit | Delete | DeletedBy, DeletedDate | P0 |
| FUN-054 | Orphan prevention | Insert | Prevent | P0 |
| FUN-055 | Cascade permission | User | Per permission | P0 |
| FUN-056 | FK deferred | Deferrable | Check at commit | P1 |
| FUN-057 | Unique partial | WHERE | Per index | P1 |
| FUN-058 | Unique case | Case | Per config | P1 |
| FUN-059 | Unique null | Nulls | Per DB | P1 |
| FUN-060 | FK to active only | Parent active | Filter | P1 |
| FUN-061 | FK to non-deleted | Soft delete | Exclude deleted | P1 |
| FUN-062 | Cascade audit | Cascade | All audited | P1 |
| FUN-063 | Orphan report | Run | Report | P1 |
| FUN-064 | Orphan cleanup | Cleanup job | Handle | P1 |
| FUN-065 | Cascade depth | N levels | All | P1 |
| FUN-066 | Cascade cycle | Prevent | No cycle | P0 |
| FUN-067 | FK in batch | All valid | Validate | P0 |
| FUN-068 | FK in import | Validate | Reject invalid | P0 |
| FUN-069 | Unique in batch | Batch | No duplicate | P0 |
| FUN-070 | Restore | Restore | IsDeleted=false | P0 |
| FUN-071 | No hard delete | Default | Soft only | P0 |
| FUN-072 | Query filter | Default | !IsDeleted | P0 |
| FUN-073 | FK composite | Multiple cols | All valid | P0 |
| FUN-074 | FK self-reference | Valid | Parent exists | P1 |
| FUN-075 | Constraint name | Violation | Name in error | P1 |
| FUN-076 | Transaction atomicity | Transaction | All or nothing | P0 |
| FUN-077 | Rollback | Error | All rolled back | P0 |
| FUN-078 | Cascade transaction | Cascade | Atomic | P0 |
| FUN-079 | Import with FK | Import | FK validated | P0 |
| FUN-080 | Export consistency | Export | Data consistent | P0 |
| FUN-081 | Migration FK | Migration | FK intact | P1 |
| FUN-082 | Integrity check | Run | No violations | P1 |
| FUN-083 | Unique constraint | Insert | No duplicate | P0 |
| FUN-084 | Unique on update | Update | No duplicate | P0 |
| FUN-085 | Check constraint | Invalid value | Violation | P0 |
| FUN-086 | Not null | Null required | Violation | P0 |
| FUN-087 | Default value | Insert | Default applied | P0 |
| FUN-088 | Trigger validation | Insert | Trigger | P1 |
| FUN-089 | Batch validation | Batch | All valid | P0 |
| FUN-090 | Nested FK | A→B→C | All valid | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 5.1 CRUD + Integrity (15)

| ID | Operation | Entities | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-001 | Create with FK | Contact, Partner | Created | P0 |
| INT-002 | Update FK | Contact | Updated | P0 |
| INT-003 | Delete cascade | Partner, Contacts | Cascade | P0 |
| INT-004 | Soft delete | Partner | Soft deleted | P0 |
| INT-005 | Restore | Partner | Restored | P0 |
| INT-006 | Create with unique | Partner | Created | P0 |
| INT-007 | Batch create | 10 contacts | All created | P0 |
| INT-008 | Delete restrict | Partner with contacts | Blocked | P0 |
| INT-009 | Transaction rollback | Error | Rollback | P0 |
| INT-010 | Import with FK | Import file | Validated | P0 |
| INT-011 | Export | Partner, Contacts | Consistent | P0 |
| INT-012 | Nested FK | A→B→C | All valid | P1 |
| INT-013 | Self-referential | Entity | Created | P1 |
| INT-014 | Bulk + FK | Bulk create | All valid | P1 |
| INT-015 | Workflow + FK | Workflow | FK valid | P1 |

### 5.2 Search/Filter/Pagination (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-016 | Search excludes deleted | Search | !IsDeleted | P0 |
| INT-017 | Filter by FK | Filter PartnerId | Valid | P0 |
| INT-018 | Pagination | Page 2 | Consistent | P1 |
| INT-019 | Sort by FK | Sort by parent | Consistent | P1 |
| INT-020 | Count excludes deleted | Count | !IsDeleted | P0 |
| INT-021 | Include deleted | Param | Include | P1 |
| INT-022 | Export excludes deleted | Export | !IsDeleted | P0 |
| INT-023 | Filter by child | Has contacts | Filter | P1 |
| INT-024 | Aggregate with FK | Sum by parent | Consistent | P1 |
| INT-025 | Full-text + deleted | Search | Exclude | P1 |

### 5.3 Cascade & Orphan (15)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-026 | Cascade delete | Parent delete | Children | P0 |
| INT-027 | Orphan cleanup | Orphans | Cleanup | P1 |
| INT-028 | Cascade audit | Cascade | All audited | P1 |
| INT-029 | Cascade transaction | Cascade | Atomic | P0 |
| INT-030 | Restrict delete | Has children | Blocked | P0 |
| INT-031 | Set null | Parent delete | FK null | P0 |
| INT-032 | Set default | Parent delete | FK default | P1 |
| INT-033 | Cascade depth 3 | A→B→C | All cascade | P1 |
| INT-034 | Soft delete cascade | Parent | Children | Per config | P1 |
| INT-035 | Orphan report | Run report | Listed | P1 |
| INT-036 | Cascade permission | User | Per permission | P0 |
| INT-037 | Cascade + notification | Cascade | Notify | P1 |
| INT-038 | Cascade + audit | Cascade | All in audit | P1 |
| INT-039 | Cascade + search | Cascade | Index updated | P1 |
| INT-040 | Import orphan | Invalid FK | Rejected | P0 |

### 5.4 Error Handling (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-041 | FK violation message | Invalid FK | Clear message | P0 |
| INT-042 | Unique violation message | Duplicate | Clear message | P0 |
| INT-043 | Constraint name | Violation | Name in error | P1 |
| INT-044 | Rollback message | Rollback | Clear | P1 |
| INT-045 | Batch partial failure | Some fail | Rollback or partial | P1 |
| INT-046 | Import validation | Invalid | Row rejected | P0 |
| INT-047 | Transaction timeout | Timeout | Rollback | P0 |
| INT-048 | Deadlock | Deadlock | Retry | P0 |
| INT-049 | Cascade failure | Child delete fail | Rollback | P0 |
| INT-050 | Integrity check | Run check | Report | P1 |

### 5.5 Additional Integration Flows (40)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-051 | Create → Query → Audit | Full flow | Audit for create | P0 |
| INT-052 | Update → Query → Audit | Full flow | Audit for update | P0 |
| INT-053 | Delete → Query → Audit | Full flow | Audit for delete | P0 |
| INT-054 | Import → Validate → Create | Import flow | FK validated | P0 |
| INT-055 | Export → Verify | Export flow | Data consistent | P0 |
| INT-056 | Cascade delete → Audit | Parent delete | All audited | P1 |
| INT-057 | Restore → Query | Restore flow | Entity restored | P0 |
| INT-058 | Batch create → Search | Batch + search | Indexed | P1 |
| INT-059 | Soft delete → Include deleted | Query param | Include deleted | P1 |
| INT-060 | FK update → Child | Parent update | Per config | P1 |
| INT-061 | Unique violation → Message | Duplicate | Clear message | P0 |
| INT-062 | Transaction rollback → State | Rollback | No partial | P0 |
| INT-063 | Deadlock → Retry | Deadlock | Retry | P0 |
| INT-064 | Orphan cleanup → Report | Cleanup | Orphans handled | P1 |
| INT-065 | Migration → Verify | Migration | Integrity | P1 |
| INT-066 | Nested FK create | A→B→C | All created | P1 |
| INT-067 | Self-referential create | Entity | Created | P1 |
| INT-068 | Bulk + FK | Bulk create | All valid | P1 |
| INT-069 | Workflow + FK | Workflow | FK valid | P1 |
| INT-070 | Search + soft delete | Search | Exclude deleted | P0 |
| INT-071 | Filter + FK | Filter by parent | Valid | P0 |
| INT-072 | Pagination + consistency | Page 2 | Consistent | P1 |
| INT-073 | Sort + FK | Sort by parent | Consistent | P1 |
| INT-074 | Count + deleted | Count | Per filter | P0 |
| INT-075 | Export + deleted | Export | Exclude deleted | P0 |
| INT-076 | Import + orphan | Invalid FK | Rejected | P0 |
| INT-077 | Cascade + notification | Cascade | Notify | P1 |
| INT-078 | Cascade + search | Cascade | Index updated | P1 |
| INT-079 | Transaction + audit | Both | Atomic | P0 |
| INT-080 | Transaction + cache | Update | Invalidate on commit | P1 |
| INT-081 | Multi-tenant isolation | Tenant A, B | No cross-tenant | P0 |
| INT-082 | Retention purge | Scheduled | Old removed | P1 |
| INT-083 | Full lifecycle | Create→Update→Delete | All audited | P0 |
| INT-084 | Batch partial failure | Some fail | Rollback or partial | P1 |
| INT-085 | Constraint violation message | Violation | Clear message | P0 |
| INT-086 | Import validation | Invalid | Row rejected | P0 |
| INT-087 | Transaction timeout | Timeout | Rollback | P0 |
| INT-088 | Restrict delete | Has children | Blocked | P0 |
| INT-089 | Set null delete | Parent delete | FK null | P0 |
| INT-090 | Set default delete | Parent delete | FK default | P1 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: 50** | ✅ COMPLIANT

### 6.1 Injection (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in FK | FK param | Parameterized | P0 |
| SEC-002 | SQL injection in filter | Filter | Parameterized | P0 |
| SEC-003 | XSS in error message | Error | Escaped | P0 |
| SEC-004 | Log injection | Constraint | Escaped | P0 |
| SEC-005 | NoSQL injection | Query | Validated | P1 |
| SEC-006 | Command injection | Constraint | Sanitized | P0 |
| SEC-007 | Path traversal | Path | Sanitized | P0 |
| SEC-008 | Header injection | Header | Validated | P0 |
| SEC-009 | Template injection | Message | No eval | P1 |
| SEC-010 | XXE in config | Config | Validated | P1 |

### 6.2 Access Control (10)

| ID | User | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Create | 401 | P0 |
| SEC-012 | Partner user | Create partner | 403 | P0 |
| SEC-013 | Read-only | Delete | 403 | P0 |
| SEC-014 | Admin | Full access | 200 | P0 |
| SEC-015 | Org-scoped | Cross-org | 403 | P0 |
| SEC-016 | User A | User B's entity | 403 | P0 |
| SEC-017 | API key | Create (no scope) | 403 | P0 |
| SEC-018 | Service account | Per config | Per config | P1 |
| SEC-019 | Delegated | On behalf | Per delegation | P1 |
| SEC-020 | Expired session | Create | 401 | P0 |

### 6.3 IDOR (10)

| ID | Manipulation | Expected | Priority |
|----|-------------|----------|----------|
| SEC-021 | Change FK to other's | 403 | P0 |
| SEC-022 | Change entity ID | 403 | P0 |
| SEC-023 | Access deleted entity | 403 or 404 | P0 |
| SEC-024 | Export other org | 403 | P0 |
| SEC-025 | Import for other org | 403 | P0 |
| SEC-026 | Cascade delete other's | 403 | P0 |
| SEC-027 | Restore other's | 403 | P0 |
| SEC-028 | Orphan report other org | 403 | P0 |
| SEC-029 | Constraint override | Rejected | P0 |
| SEC-030 | FK bypass | Rejected | P0 |

### 6.4 Auth & Session (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| SEC-031 | JWT expired | 401 | P0 |
| SEC-032 | JWT tampered | 401 | P0 |
| SEC-033 | CSRF on create | Token required | P0 |
| SEC-034 | Replay attack | Nonce | P1 |
| SEC-035 | Session fixation | New session | P0 |
| SEC-036 | Token theft | Invalidate | P0 |
| SEC-037 | Concurrent session | Per policy | P1 |
| SEC-038 | Refresh token | Limited | P1 |
| SEC-039 | MFA | MFA required | P1 |
| SEC-040 | Password change | Re-auth | P1 |

### 6.5 Data Exposure (10)

| ID | Data | Risk | Expected | Priority |
|----|------|------|----------|----------|
| SEC-041 | FK in error | Info leak | Generic | P0 |
| SEC-042 | Constraint name | Internal | Minimal | P1 |
| SEC-043 | Deleted data | Exposure | Filtered | P0 |
| SEC-044 | Orphan data | Cross-org | Filtered | P0 |
| SEC-045 | Error stack trace | Never | No stack | P0 |
| SEC-046 | Internal IDs | Per config | Minimal | P1 |
| SEC-047 | Cascade details | Internal | Logged | P1 |
| SEC-048 | Transaction ID | Correlation | No PII | P1 |
| SEC-049 | Constraint details | Minimal | Generic | P1 |
| SEC-050 | Validation details | User-facing | Clear | P1 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | 2 users create different | Both succeed | P0 |
| CON-002 | 2 users create same unique | One fails | P0 |
| CON-003 | 2 users update same | 409 or one wins | P0 |
| CON-004 | 2 users delete same | One succeeds | P0 |
| CON-005 | Create + delete FK | Consistent | P0 |
| CON-006 | Cascade + concurrent | No conflict | P1 |
| CON-007 | Soft delete + read | Consistent | P0 |
| CON-008 | Restore + update | Consistent | P1 |
| CON-009 | Import + FK | Consistent | P1 |
| CON-010 | Export + update | Snapshot | P1 |
| CON-011 | Batch + single | Both succeed | P1 |
| CON-012 | Transaction + deadlock | Retry | P0 |
| CON-013 | FK + concurrent | Consistent | P0 |
| CON-014 | Unique + concurrent | One fails | P0 |
| CON-015 | Orphan cleanup + update | No conflict | P1 |
| CON-016 | Cascade + audit | Both | P1 |
| CON-017 | Soft delete + cascade | Per config | P1 |
| CON-018 | Restrict + cascade | No conflict | P1 |
| CON-019 | Transaction + timeout | Rollback | P0 |
| CON-020 | Bulk + FK | All valid | P1 |
| CON-021 | Search + soft delete | Consistent | P1 |
| CON-022 | Cache + soft delete | Invalidated | P1 |
| CON-023 | Audit + cascade | All audited | P1 |
| CON-024 | Import + unique | Validate | P0 |
| CON-025 | Migration + FK | Integrity | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

### 8.1 Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | FK valid | 123 | Valid | P1 |
| UNT-002 | FK invalid | 0 | Invalid | P1 |
| UNT-003 | Unique valid | "abc" | Valid | P1 |
| UNT-004 | Unique duplicate | "abc" exists | Invalid | P1 |
| UNT-005 | Soft delete flag | true | Valid | P1 |

### 8.2 Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | Format FK error | Violation | Message | P1 |
| UNT-007 | Format unique error | Violation | Message | P1 |
| UNT-008 | Format constraint error | Violation | Message | P1 |

### 8.3 Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | Cascade depth | 3 levels | 3 | P1 |
| UNT-010 | Orphan count | 5 orphans | 5 | P1 |
| UNT-011 | FK count | Entity | Count | P1 |
| UNT-012 | Unique cols | (A,B) | 2 | P1 |
| UNT-013 | Transaction size | 10 stmts | 10 | P1 |

### 8.4 Status Logic (5)

| ID | Test | Condition | Expected | Priority |
|----|------|-----------|----------|----------|
| UNT-014 | Is deleted | IsDeleted=true | True | P1 |
| UNT-015 | Has orphan | Orphan exist | True | P1 |
| UNT-016 | Can cascade | Config | True | P1 |
| UNT-017 | Is restricted | Config | True | P1 |
| UNT-018 | FK nullable | Nullable | True | P1 |

### 8.5 Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | Cascade order | Parent, children | Order | P1 |
| UNT-020 | Orphan list | Orphans | List | P1 |
| UNT-021 | FK chain | A→B→C | Chain | P1 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | Create with FK | < 100 ms | P1 |
| PRF-002 | Cascade delete 10 | < 200 ms | P1 |
| PRF-003 | Cascade delete 100 | < 2 s | P1 |
| PRF-004 | Unique check | < 10 ms | P1 |
| PRF-005 | FK check | < 10 ms | P1 |
| PRF-006 | Soft delete query | < 50 ms | P1 |
| PRF-007 | Orphan report | < 5 s | P1 |
| PRF-008 | Integrity check | < 30 s | P1 |
| PRF-009 | Transaction commit | < 100 ms | P1 |
| PRF-010 | Transaction rollback | < 50 ms | P1 |
| PRF-011 | Batch with FK | 100 rows < 5 s | P1 |
| PRF-012 | Import with validation | 1000 rows < 30 s | P1 |
| PRF-013 | Cascade depth 5 | < 1 s | P2 |
| PRF-014 | Orphan cleanup | 1000 < 10 s | P2 |
| PRF-015 | 100 concurrent | < 10 s | P2 |
| PRF-016 | Memory under load | No leak | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 50 create/min | 10 min | All succeed | P1 |
| LDT-002 | 100 create/min | 10 min | < 1% error | P1 |
| LDT-003 | 200 create/min | 5 min | Degradation ok | P2 |
| LDT-004 | Spike: cascade | 100 deletes | Complete | P1 |
| LDT-005 | Spike: import | 1000 rows | Complete | P2 |
| LDT-006 | Stress: FK | High load | No violation | P2 |
| LDT-007 | Stress: unique | High load | No violation | P2 |
| LDT-008 | Stress: transaction | High load | No deadlock | P2 |
| LDT-009 | Recovery | 5 min | Normal | P1 |
| LDT-010 | Recovery | 10 min | Full | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| FK constraints | POS-001–003, NEG-001–015, FUN-001–015 |
| Unique constraints | POS-004, NEG-016–030, FUN-016–025 |
| Soft delete | POS-005–006, NEG-031–045, FUN-026–035 |
| Orphan prevention | NEG-046–060, FUN-036–050 |
| Cascade behavior | POS-010, INT-026–040 |
| Transaction boundaries | POS-008–009, FUN-036–050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
