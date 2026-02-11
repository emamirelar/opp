# UNOPSInteractionManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/InteractionManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

Interaction manager unit tests cover CRUD interactions, date validation, and type classification for interactions. Tests include: create/update/delete interactions, date range validation, interaction type classification, partner/contact linking, and audit trail.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create interaction | Valid data | Create | Interaction created |
| POS-002 | Get interaction by ID | Interaction exists | GetById | Interaction returned |
| POS-003 | Update interaction | Interaction exists | Update | Updated |
| POS-004 | Soft delete interaction | Interaction exists | Delete | IsDeleted=true |
| POS-005 | List interactions by partner | Partner has interactions | List | List returned |
| POS-006 | List interactions by contact | Contact has interactions | List | List returned |
| POS-007 | Valid interaction type | Type valid | Create | Accepted |
| POS-008 | Valid date range | Dates valid | Create | Accepted |
| POS-009 | Link to partner | Partner exists | Link | Linked |
| POS-010 | Link to contact | Contact exists | Link | Linked |
| POS-011 | Get with partner | Interaction exists | GetById include | Partner loaded |
| POS-012 | Get with contact | Interaction exists | GetById include | Contact loaded |
| POS-013 | Search by subject | Interactions exist | Search | Matching |
| POS-014 | Filter by type | Interactions exist | Filter | Filtered |
| POS-015 | Filter by date range | Interactions exist | Filter | Filtered |
| POS-016 | Pagination | Many interactions | List page | Page |
| POS-017 | Sort by date | Interactions exist | Sort | Ordered |
| POS-018 | Sort by type | Interactions exist | Sort | Ordered |
| POS-019 | Audit CreatedBy | Create | Check audit | Set |
| POS-020 | Audit CreatedDate | Create | Check audit | UTC |
| POS-021 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-022 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-023 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-024 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-025 | Classify type | Type input | Classify | Classified |
| POS-026 | Get interaction types | Types exist | GetTypes | Types |
| POS-027 | Count by partner | Partner has interactions | Count | Count |
| POS-028 | Count by contact | Contact has interactions | Count | Count |
| POS-029 | Export interactions | Interactions exist | Export | Exported |
| POS-030 | Import interactions | CSV valid | Import | Imported |
| POS-031 | Get interaction summary | Interactions exist | GetSummary | Summary |
| POS-032 | Validate date range | Dates valid | Validate | True |
| POS-033 | Get by opportunity | Opportunity has interactions | GetByOpportunity | List |
| POS-034 | Bulk create | Valid data | BulkCreate | All created |
| POS-035 | Permission check | User has permission | Check | True |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null subject | Subject=null | ValidationException |
| NEG-002 | Create with empty subject | Subject="" | ValidationException |
| NEG-003 | Create with invalid type | Type=invalid | ValidationException |
| NEG-004 | Create with invalid date | End before start | ValidationException |
| NEG-005 | Get by zero ID | Id=0 | KeyNotFoundException |
| NEG-006 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-007 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-009 | Link to deleted partner | Partner deleted | BusinessException |
| NEG-010 | Link to deleted contact | Contact deleted | BusinessException |
| NEG-011 | Invalid partner ID | PartnerId=-1 | ArgumentException |
| NEG-012 | Invalid contact ID | ContactId=-1 | ArgumentException |
| NEG-013 | Invalid opportunity ID | OpportunityId=-1 | ArgumentException |
| NEG-014 | Null request object | Request=null | ArgumentNullException |
| NEG-015 | GetById without permission | Unauthorized | Forbidden |
| NEG-016 | Create without permission | Unauthorized | Forbidden |
| NEG-017 | Update without permission | Unauthorized | Forbidden |
| NEG-018 | Delete without permission | Unauthorized | Forbidden |
| NEG-019 | Date in future | Date future | ValidationException |
| NEG-020 | Date range invalid | End<Start | ValidationException |
| NEG-021 | Type classification invalid | Type invalid | ArgumentException |
| NEG-022 | Subject exceeds max | Subject length | ValidationException |
| NEG-023 | Description exceeds max | Description length | ValidationException |
| NEG-024 | Import invalid format | Bad CSV | ValidationException |
| NEG-025 | Import duplicate | CSV duplicate | BusinessException |
| NEG-026 | Export invalid format | Format invalid | ArgumentException |
| NEG-027 | List with invalid filter | Malformed filter | ArgumentException |
| NEG-028 | Invalid page number | Page=0 | ArgumentException |
| NEG-029 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-030 | Search null term | Term=null | ArgumentNullException |
| NEG-031 | Bulk create null list | List=null | ArgumentNullException |
| NEG-032 | GetSummary invalid | Params invalid | ArgumentException |
| NEG-033 | Validate null date | Date=null | ArgumentNullException |
| NEG-034 | GetByOpportunity invalid | Id invalid | ArgumentException |
| NEG-035 | Update deleted interaction | Interaction deleted | KeyNotFoundException |
| NEG-036 | GetById deleted | Interaction deleted | KeyNotFoundException |
| NEG-037 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-038 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-039 | Transaction rollback | Fail in transaction | Rollback |
| NEG-040 | Connection timeout | DB unavailable | TimeoutException |
| NEG-041 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-042 | Invalid enum value | Type invalid | ArgumentException |
| NEG-043 | Expired session | Expired token | Unauthorized |
| NEG-044 | Null user context | User=null | InvalidOperationException |
| NEG-045 | Invalid include path | Invalid include | ArgumentException |
| NEG-046 | Count invalid partner | PartnerId=-1 | ArgumentException |
| NEG-047 | Count invalid contact | ContactId=-1 | ArgumentException |
| NEG-048 | Classify null type | Type=null | ArgumentNullException |
| NEG-049 | GetTypes invalid | Params invalid | ArgumentException |
| NEG-050 | Bulk create empty | List=[] | ArgumentException |
| NEG-051 | Export empty | No interactions | Empty or error |
| NEG-052 | Filter invalid type | Type invalid | ArgumentException |
| NEG-053 | Filter invalid date | Date invalid | ArgumentException |
| NEG-054 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-055 | Pagination overflow | Page too large | Empty or error |
| NEG-056 | GetByPartner deleted partner | Partner deleted | Empty list |
| NEG-057 | GetByContact deleted contact | Contact deleted | Empty list |
| NEG-058 | Audit missing user | User=0 | InvalidOperationException |
| NEG-059 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-060 | Validate null range | Range=null | ArgumentNullException |
| NEG-061 | GetSummary null params | Params=null | ArgumentNullException |
| NEG-062 | Child override throws | Child throws | Propagated |
| NEG-063 | Link to null entity | Entity=null | ArgumentNullException |
| NEG-064 | Import missing column | CSV missing | ValidationException |
| NEG-065 | GetByOpportunity deleted | Opportunity deleted | Empty list |
| NEG-066 | Bulk create one invalid | One invalid | Partial or fail |
| NEG-067 | Classify empty type | Type="" | ArgumentException |
| NEG-068 | GetTypes empty | No types | Empty list |
| NEG-069 | Date format invalid | Format invalid | FormatException |
| NEG-070 | Duplicate interaction | Same exists | BusinessException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Subject at min length | Length=1 | Valid |
| BND-002 | Subject at max length | Length=500 | Valid |
| BND-003 | Subject exceeds max | Length=501 | Reject |
| BND-004 | Description at max | Length=4000 | Valid |
| BND-005 | Description exceeds max | Length=4001 | Reject |
| BND-006 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Date at min | Date=MinValue | Handle |
| BND-011 | Date at max | Date=MaxValue | Handle |
| BND-012 | Date range same | Start=End | Valid |
| BND-013 | Date range one day | 1 day | Valid |
| BND-014 | Date range max | Max range | Valid |
| BND-015 | Unicode in subject | Arabic/Chinese | Stored |
| BND-016 | Special chars in subject | <>&"' | Escaped |
| BND-017 | Leading/trailing spaces | Subject="  x  " | Trimmed |
| BND-018 | Empty description | Description="" | Valid |
| BND-019 | Type enum first | First | Valid |
| BND-020 | Type enum last | Last | Valid |
| BND-021 | Empty partner list | Partner=[] | Valid |
| BND-022 | Single partner | Count=1 | Valid |
| BND-023 | Single contact | Count=1 | Valid |
| BND-024 | Max participants | At limit | Valid |
| BND-025 | Empty search term | Term="" | Return all |
| BND-026 | Search term max | Term=500 | Valid |
| BND-027 | Search term over max | Term=501 | Reject |
| BND-028 | Collection empty | [] | No exception |
| BND-029 | Collection single | 1 item | Valid |
| BND-030 | Collection max | At limit | Valid |
| BND-031 | Pagination last partial | Partial page | Correct |
| BND-032 | Pagination total | Total count | Accurate |
| BND-033 | Sort null handling | Nulls in data | Deterministic |
| BND-034 | Filter combination all | All filters | Correct |
| BND-035 | DateTime UTC | UTC input | Stored |
| BND-036 | Timezone edge | DST transition | Correct |
| BND-037 | Zero partner ID | PartnerId=0 | Reject |
| BND-038 | Zero contact ID | ContactId=0 | Reject |
| BND-039 | Max int for ID | Id=2147483647 | Handle |
| BND-040 | Bulk create max | 100 interactions | Valid |
| BND-041 | Bulk create over max | 101 interactions | Reject |
| BND-042 | Import row max | Max rows | Valid or reject |
| BND-043 | Import empty file | 0 rows | Empty or error |
| BND-044 | Export large result | 10k rows | Stream |
| BND-045 | GetSummary empty | No interactions | Empty |
| BND-046 | GetSummary max | Many | Valid |
| BND-047 | Soft delete boundary | DeletedDate set | Excluded |
| BND-048 | Include depth | Deep include | No explosion |
| BND-049 | Query timeout | Slow query | Timeout |
| BND-050 | Memory large result | 10k rows | No OOM |
| BND-051 | Audit timestamp precision | Millisecond | Stored |
| BND-052 | Long string in notes | 4000 chars | Truncate |
| BND-053 | Classify boundary type | Boundary | Classified |
| BND-054 | GetTypes empty | No types | Empty list |
| BND-055 | GetTypes max | Many types | Valid |
| BND-056 | Count zero | No interactions | 0 |
| BND-057 | Count max | Many | Valid |
| BND-058 | Validate date edge | Edge date | Valid |
| BND-059 | GetByOpportunity empty | No interactions | Empty list |
| BND-060 | GetByPartner empty | No interactions | Empty list |
| BND-061 | GetByContact empty | No interactions | Empty list |
| BND-062 | Filter date edge | Edge date | Correct |
| BND-063 | Filter type edge | Edge type | Correct |
| BND-064 | Sort multi-column | 3 columns | Correct order |
| BND-065 | Export format boundary | CSV | Valid |
| BND-066 | Import format boundary | CSV | Valid |
| BND-067 | Bulk create single | Count=1 | Valid |
| BND-068 | Classify unknown | Unknown type | Default or error |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Subject required | Validation | Create | Reject if empty |
| FUN-002 | Type required | Validation | Create | Reject if invalid |
| FUN-003 | Date required | Validation | Create | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Date range valid | Constraint | Create | Reject if invalid |
| FUN-008 | Partner or contact required | Constraint | Create | Reject if both null |
| FUN-009 | Type in allowed list | Constraint | Create | Reject if invalid |
| FUN-010 | Audit CreatedBy | Audit | Create | Set user |
| FUN-011 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-012 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-013 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-014 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-015 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | Partner must exist | Constraint | Create | Reject invalid |
| FUN-018 | Contact must exist | Constraint | Create | Reject invalid |
| FUN-019 | Opportunity must exist | Constraint | Create | Reject invalid |
| FUN-020 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-021 | GetByPartner excludes deleted | Constraint | GetByPartner | Excludes deleted |
| FUN-022 | GetByContact excludes deleted | Constraint | GetByContact | Excludes deleted |
| FUN-023 | GetByOpportunity excludes deleted | Constraint | GetByOpportunity | Excludes deleted |
| FUN-024 | Pagination offset | Calculation | Page | Skip correct |
| FUN-025 | Total count accurate | Calculation | Count | Matches |
| FUN-026 | Sort applies | Calculation | Sort | Ordered |
| FUN-027 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-028 | Classify type logic | Logic | Classify | Classified |
| FUN-029 | Validate date logic | Logic | Validate | Validated |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on update | Transaction | Update | Atomic |
| FUN-032 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-033 | Async all operations | Concurrency | All | Async |
| FUN-034 | Include loads partner | Data load | GetById include | Partner loaded |
| FUN-035 | Include loads contact | Data load | GetById include | Contact loaded |
| FUN-036 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-037 | Link creates association | Data | Link | Association |
| FUN-038 | Unlink removes association | Data | Unlink | Removed |
| FUN-039 | Bulk create atomic | Logic | BulkCreate | All or none |
| FUN-040 | Import validates | Validation | Import | Validated |
| FUN-041 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-042 | GetSummary aggregates | Logic | GetSummary | Aggregated |
| FUN-043 | Count excludes deleted | Constraint | Count | Excludes deleted |
| FUN-044 | GetTypes filtered | Logic | GetTypes | Filtered |
| FUN-045 | Localized display | i18n | GetDisplay | Localized |
| FUN-046 | Date format | Logic | Format | Formatted |
| FUN-047 | Type display | Logic | GetTypeDisplay | Display |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Date range validation | Validation | Validate | Range check |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create interaction full flow | Create | Interaction, Partner | Created |
| INT-002 | Update interaction full flow | Update | Interaction | Updated |
| INT-003 | Delete interaction full flow | Delete | Interaction | Soft deleted |
| INT-004 | Get with partner | GetById | Interaction, Partner | Partner loaded |
| INT-005 | Get with contact | GetById | Interaction, Contact | Contact loaded |
| INT-006 | List with filter and sort | List | Interaction | Filtered, sorted |
| INT-007 | Link to partner | Link | Interaction, Partner | Linked |
| INT-008 | Link to contact | Link | Interaction, Contact | Linked |
| INT-009 | Search by subject | Search | Interaction | Matching |
| INT-010 | Filter by type | Filter | Interaction | Filtered |
| INT-011 | Filter by date | Filter | Interaction | Filtered |
| INT-012 | Pagination | Paginate | Interaction | Pages |
| INT-013 | Import CSV | Import | Interaction | Imported |
| INT-014 | Export CSV | Export | Interaction | Exported |
| INT-015 | Bulk create | BulkCreate | Interaction | All created |
| INT-016 | Interaction-Partner relationship | Relationship | Interaction, Partner | FK valid |
| INT-017 | Interaction-Contact relationship | Relationship | Interaction, Contact | FK valid |
| INT-018 | Interaction-Opportunity relationship | Relationship | Interaction, Opportunity | FK valid |
| INT-019 | Cascade soft delete | Relationship | Partner deleted | Config |
| INT-020 | Orphan handling | Relationship | Partner deleted | Retained |
| INT-021 | DB error handling | Error | DB down | Graceful |
| INT-022 | Timeout handling | Error | Slow DB | Timeout |
| INT-023 | Constraint violation | Error | FK violation | Clear error |
| INT-024 | Unique violation | Error | Duplicate | Clear error |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | PartnerManager integration | Integration | PartnerManager | Partner |
| INT-030 | ContactManager integration | Integration | ContactManager | Contact |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | OpportunityManager integration | Integration | OpportunityManager | Opportunity |
| INT-036 | Multiple interactions per partner | Scenario | Interaction, Partner | All linked |
| INT-037 | Multiple interactions per contact | Scenario | Interaction, Contact | All linked |
| INT-038 | Concurrent create | Scenario | Parallel | All created |
| INT-039 | Import with validation | Scenario | Import | Validated |
| INT-040 | Export with filter | Scenario | Export | Filtered |
| INT-041 | GetSummary by partner | Scenario | GetSummary | Summary |
| INT-042 | GetSummary by contact | Scenario | GetSummary | Summary |
| INT-043 | GetByOpportunity | Scenario | GetByOpportunity | List |
| INT-044 | Classify then create | Scenario | Classify, Create | Both |
| INT-045 | Validate then create | Scenario | Validate, Create | Both |
| INT-046 | Count by partner | Scenario | Count | Count |
| INT-047 | Count by contact | Scenario | Count | Count |
| INT-048 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-049 | Bulk create with validation | Scenario | BulkCreate | Validated |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in subject | '; DROP TABLE-- | Subject | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | XSS in subject | <script>alert(1)</script> | Subject | Escaped |
| SEC-004 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-005 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Any | Rejected |
| SEC-008 | Path traversal | ../../../etc/passwd | File ref | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized create | No permission | Create | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized import | No permission | Import | 403 |
| SEC-015 | Unauthorized export | No permission | Export | 403 |
| SEC-016 | Role escalation | Low role | Admin | 403 |
| SEC-017 | Cross-tenant access | User A | User B data | 403 |
| SEC-018 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-019 | IDOR update other | Id=other | Update | 403 |
| SEC-020 | IDOR delete other | Id=other | Delete | 403 |
| SEC-021 | IDOR in filter | PartnerId=other | List | Filtered |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-025 | Mass assign PartnerId | PartnerId=other | Request | Validated |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on update | No token | Update | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Rate limit create | Many creates | Create | Throttled |
| SEC-034 | Rate limit list | Many lists | List | Throttled |
| SEC-035 | Rate limit search | Many searches | Search | Throttled |
| SEC-036 | Oversized request | 10MB payload | Create | Rejected |
| SEC-037 | Deep nesting | Nested object | Request | Rejected |
| SEC-038 | Header injection | \r\n in header | Header | Rejected |
| SEC-039 | Null byte injection | %00 in subject | Subject | Rejected |
| SEC-040 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-041 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-042 | Denial of service | Huge page size | List | Capped |
| SEC-043 | Import malicious CSV | Malicious | Import | Rejected |
| SEC-044 | Export data injection | Inject in export | Export | Sanitized |
| SEC-045 | Date injection | Invalid date | Create | Rejected |
| SEC-046 | Type injection | Invalid type | Create | Rejected |
| SEC-047 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-048 | Permission cached | Repeated check | Permission | Cached |
| SEC-049 | Partner IDOR | PartnerId=other | Create | Validated |
| SEC-050 | Contact IDOR | ContactId=other | Create | Validated |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent create | Two create | Both succeed |
| CON-004 | Concurrent update same | Two update | One wins |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on link | Two link | One or both |
| CON-009 | Race on count | Two count | Both correct |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Import concurrent | Two import | One or both |
| CON-016 | Export concurrent | Two export | Both succeed |
| CON-017 | Search concurrent | Two search | Both correct |
| CON-018 | GetSummary concurrent | Two get | Both correct |
| CON-019 | Soft delete concurrent | Delete while update | Deterministic |
| CON-020 | Bulk create | Concurrent bulk | All succeed |
| CON-021 | Filter concurrent update | Filter while update | Consistent |
| CON-022 | Idempotency | Same request twice | Same result |
| CON-023 | Lock escalation | Many locks | No escalation |
| CON-024 | Connection pool | Many concurrent | Pool limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate subject not null | Validation | null | Exception |
| UNT-002 | Validate type | Validation | Valid type | Pass |
| UNT-003 | Validate date range | Validation | Valid range | Pass |
| UNT-004 | Validate partner | Validation | Valid partner | Pass |
| UNT-005 | Validate date range invalid | Validation | End<Start | Exception |
| UNT-006 | Format date display | Formatting | Date | Display |
| UNT-007 | Format type display | Formatting | Type | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Date range duration | Calculation | Start, End | Duration |
| UNT-013 | Classify type | Calculation | Input | Type |
| UNT-014 | Type allows create | Status logic | Type | true |
| UNT-015 | Type allows update | Status logic | Type | true |
| UNT-016 | Type allows delete | Status logic | Type | true |
| UNT-017 | Date valid check | Status logic | Date | Valid |
| UNT-018 | Date range valid check | Status logic | Range | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Bulk create 100 | Create 100 | <5s | P0 |
| PRF-004 | Bulk create 1000 | Create 1000 | <30s | P0 |
| PRF-005 | Search by subject | Search | <500ms | P1 |
| PRF-006 | List with pagination | List | <300ms | P1 |
| PRF-007 | List with sort | List | <300ms | P1 |
| PRF-008 | Filter by type | Filter | <300ms | P1 |
| PRF-009 | Filter by date | Filter | <300ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 writes | 5 parallel Create | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 write | <4s total | P2 |
| PRF-013 | Memory single create | Create | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory bulk 100 | Bulk create | <20MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 10 RPS create | 10 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 50 RPS | 0→50→0 | 1 min | No errors |
| LDT-005 | Spike 100 RPS | 0→100→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large bulk | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
