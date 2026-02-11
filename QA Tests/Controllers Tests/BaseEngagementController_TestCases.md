# BaseEngagementController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/BaseEngagementController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API base engagement controller: shared engagement CRUD, notes, activities, follow-ups, and common engagement patterns.

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

## §1 Positive Tests (35)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Create engagement | POST /api/engagement | 201 Created |
| POS-002 | Get engagement by ID | GET /api/engagement/{id} | Engagement details |
| POS-003 | Update engagement | PUT /api/engagement/{id} | 200 OK |
| POS-004 | Delete engagement | DELETE /api/engagement/{id} | 204 No Content |
| POS-005 | List engagements | GET /api/engagement | Paginated list |
| POS-006 | Add note | POST /api/engagement/{id}/notes | Note added |
| POS-007 | Get notes | GET /api/engagement/{id}/notes | Notes list |
| POS-008 | Get activities | GET /api/engagement/{id}/activities | Activities list |
| POS-009 | Add activity | POST /api/engagement/{id}/activities | Activity added |
| POS-010 | Create follow-up | POST /api/engagement/{id}/follow-ups | Follow-up created |
| POS-011 | Get follow-ups | GET /api/engagement/{id}/follow-ups | Follow-ups list |
| POS-012 | Filter by entity | GET /api/engagement?entityId=1 | Filtered list |
| POS-013 | Filter by type | GET /api/engagement?type=Meeting | Filtered list |
| POS-014 | Filter by date range | GET with start/end | Date-filtered |
| POS-015 | Sort by date | GET ?sortBy=date | Sorted by date |
| POS-016 | Pagination | GET ?page=1&pageSize=20 | Paginated |
| POS-017 | Update note | PUT /api/engagement/{id}/notes/{noteId} | Note updated |
| POS-018 | Delete note | DELETE /api/engagement/{id}/notes/{noteId} | Note removed |
| POS-019 | Complete follow-up | PUT /api/engagement/{id}/follow-ups/{fid} | Marked complete |
| POS-020 | Search engagements | GET ?search=text | Filtered by search |
| POS-021 | Get engagement by type | GET /api/engagement?type=Call | Type-filtered |
| POS-022 | Get engagement history | GET /api/engagement/{id}/history | History list |
| POS-023 | Soft delete engagement | DELETE (soft) | IsDeleted=true |
| POS-024 | Restore engagement | POST /api/engagement/{id}/restore | Restored |
| POS-025 | Attach document | POST /api/engagement/{id}/documents | Document attached |
| POS-026 | Get documents | GET /api/engagement/{id}/documents | Documents list |
| POS-027 | Assign participant | POST /api/engagement/{id}/participants | Participant added |
| POS-028 | Get participants | GET /api/engagement/{id}/participants | Participants list |
| POS-029 | Add reminder | POST /api/engagement/{id}/reminders | Reminder set |
| POS-030 | Get reminders | GET /api/engagement/{id}/reminders | Reminders list |
| POS-031 | Bulk create | POST /api/engagement/bulk | Bulk created |
| POS-032 | Export engagements | GET /api/engagement/export | Export file |
| POS-033 | Get engagement summary | GET /api/engagement/{id}/summary | Summary |
| POS-034 | Get engagement timeline | GET /api/engagement/{id}/timeline | Timeline |
| POS-035 | Clone engagement | POST /api/engagement/{id}/clone | Clone created |

---

## §2 Negative Tests (70)

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
| NEG-009 | Past date for future | Follow-up date past | 400 |
| NEG-010 | Invalid entity type | type=Invalid | 400 |
| NEG-011 | Orphaned entityId | entityId=999999 | 404 |
| NEG-012 | Deleted entity | entityId deleted | 404 |
| NEG-013 | SQL injection in search | search='; DROP | Sanitized |
| NEG-014 | XSS in note | note=<script> | Sanitized |
| NEG-015 | Negative page | page=-1 | 400 |
| NEG-016 | Zero pageSize | pageSize=0 | 400 |
| NEG-017 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-018 | Invalid sort | sortBy=invalid | 400 |
| NEG-019 | No permission | User without CanView | 403 |
| NEG-020 | Cross-org access | Other org engagement | 403 |
| NEG-021 | Create on deleted entity | POST to deleted | 404 |
| NEG-022 | Update deleted engagement | PUT on deleted | 404 |
| NEG-023 | Delete already deleted | DELETE on deleted | 404 |
| NEG-024 | Empty note content | Note "" | 400 |
| NEG-025 | Whitespace-only note | Note "   " | 400/reject |
| NEG-026 | Invalid note ID | noteId=999999 | 404 |
| NEG-027 | Invalid follow-up ID | followUpId invalid | 404 |
| NEG-028 | Activity on wrong engagement | Wrong engagementId | 404 |
| NEG-029 | Malformed JSON | Invalid JSON body | 400 |
| NEG-030 | Wrong content-type | Application/xml | 415 |
| NEG-031 | Duplicate follow-up date | Same date | 400 or allow |
| NEG-032 | Circular reference | Self-reference | 400 |
| NEG-033 | Rate limit | Too many requests | 429 |
| NEG-034 | Payload too large | Huge body | 413 |
| NEG-035 | Invalid document type | Unsupported file | 400 |
| NEG-036 | Oversized document | File > 10MB | 413 |
| NEG-037 | Invalid participant | participantId invalid | 404 |
| NEG-038 | Duplicate participant | Same user twice | 400 |
| NEG-039 | Invalid reminder date | Past reminder | 400 |
| NEG-040 | Clone deleted | Clone deleted engagement | 404 |
| NEG-041 | Bulk with invalid items | One invalid in bulk | 400/partial |
| NEG-042 | Export no permission | No export permission | 403 |
| NEG-043 | Invalid date range | end < start | 400 |
| NEG-044 | Future date limit | Date > 1 year | 400 |
| NEG-045 | Invalid enum | status=Invalid | 400 |
| NEG-046 | Null optional field | Optional null | Use default |
| NEG-047 | Wrong HTTP method | PUT for create | 405 |
| NEG-048 | HEAD request | HEAD /api/engagement | 200 or 405 |
| NEG-049 | OPTIONS | OPTIONS | 200 CORS |
| NEG-050 | Trailing slash | /api/engagement/ | Redirect or 404 |
| NEG-051 | Case sensitivity | /api/Engagement | 404 or 200 |
| NEG-052 | Extra path segments | /api/engagement/1/extra | 404 |
| NEG-053 | DB timeout | Simulate timeout | 503 |
| NEG-054 | Storage unavailable | Document storage down | 503 |
| NEG-055 | Validation cascade | Multiple invalid fields | All errors |
| NEG-056 | Concurrent delete | Delete while updating | 409 or 404 |
| NEG-057 | Session expired | Mid-request | 401 |
| NEG-058 | Permission revoked | Mid-session | 403 on next |
| NEG-059 | Entity type mismatch | Wrong entity type | 400 |
| NEG-060 | Maximum depth exceeded | Nested too deep | 400 |
| NEG-061 | Reserved character | Name with \0 | 400 |
| NEG-062 | Control characters | Note with \n\r\t | Sanitize |
| NEG-063 | Invalid bulk count | Bulk 1000 items | 400 |
| NEG-064 | Empty bulk | Bulk [] | 400 |
| NEG-065 | Invalid UUID | GUID format wrong | 400 |
| NEG-066 | Mismatched IDs | Path id != body id | 400 |
| NEG-067 | Read-only field update | Update createdBy | Ignored |
| NEG-068 | Version conflict | Stale version | 409 |
| NEG-069 | Soft-delete filter | Query deleted | Excluded |
| NEG-070 | Audit failure | Audit service down | Log, continue |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | note length | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | search length | 0 | 200 | ✅ | ✅ | ❌ |
| BND-006 | entityId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | date range | 1 day | 365 days | ✅ | ✅ | ❌ |
| BND-008 | participants | 0 | 50 | ✅ | ✅ | ❌ |
| BND-009 | notes count | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-010 | activities count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-011 | follow-ups | 0 | 100 | ✅ | ✅ | ❌ |
| BND-012 | bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-013 | Empty list | - | - | Returns [] | - | - |
| BND-014 | Single item | - | - | Returns 1 | - | - |
| BND-015 | First page | page=1 | - | ✅ | - | - |
| BND-016 | Last page | - | - | Partial OK | - | - |
| BND-017 | Zero length name | - | - | ❌ | - | - |
| BND-018 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-019 | Feb 29 | - | - | Valid | - | - |
| BND-020 | Dec 31 | - | - | Inclusive | - | - |
| BND-021 | Jan 1 | - | - | Inclusive | - | - |
| BND-022 | Midnight | 00:00:00 | - | Valid | - | - |
| BND-023 | End of day | 23:59:59 | - | Valid | - | - |
| BND-024 | Same start/end | - | - | Valid | - | - |
| BND-025 | Unicode name | - | - | Accept | - | - |
| BND-026 | Emoji in note | - | - | Accept/sanitize | - | - |
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
| BND-044 | Nested notes | - | 5 | ✅ | ✅ | ❌ |
| BND-045 | Hierarchy depth | - | 10 | ✅ | ✅ | ❌ |
| BND-046 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-047 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-048 | Attachment count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-049 | Reminder count | 0 | 10 | ✅ | ✅ | ❌ |
| BND-050 | Tag count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-051 | History entries | - | 1000 | ✅ | ✅ | ❌ |
| BND-052 | Clone depth | - | 1 | - | ✅ | ❌ |
| BND-053 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-054 | Empty export | - | - | Headers only | - | - |
| BND-055 | Single export row | - | - | Valid | - | - |
| BND-056 | Pagination boundary | - | - | Exact count | - | - |
| BND-057 | Cursor pagination | - | - | Valid cursor | - | - |
| BND-058 | Empty filter result | - | - | [] | - | - |
| BND-059 | All nulls optional | - | - | Defaults | - | - |
| BND-060 | Mixed nulls | - | - | Skip nulls | - | - |
| BND-061 | Soft-deleted entity | - | - | Excluded | - | - |
| BND-062 | Inactive org | - | - | 403/excluded | - | - |
| BND-063 | Inactive user | - | - | Excluded | - | - |
| BND-064 | Duplicate detection | - | - | Reject or idempotent | - | - |
| BND-065 | Idempotent create | - | - | Same result | - | - |
| BND-066 | Version number | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-067 | Sequence number | 1 | - | ✅ | ❌ | - |
| BND-068 | Empty bulk success | - | - | 200 | - | - |
| BND-069 | Partial bulk success | - | - | 207 | - | - |
| BND-070 | Round-trip | Create → Get | - | Match | - | - |

---

## §4 Functional Tests (50)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Create engagement | POST valid | 201 |
| FUN-002 | Workflow | Update engagement | PUT valid | 200 |
| FUN-003 | Workflow | Soft delete | DELETE | IsDeleted |
| FUN-004 | Workflow | Add note | POST note | Note added |
| FUN-005 | Workflow | Add activity | POST activity | Activity added |
| FUN-006 | Workflow | Create follow-up | POST follow-up | Created |
| FUN-007 | Workflow | Complete follow-up | PUT complete | Status updated |
| FUN-008 | Workflow | Restore | POST restore | Restored |
| FUN-009 | Workflow | Clone | POST clone | Clone created |
| FUN-010 | Workflow | Bulk create | POST bulk | All created |
| FUN-011 | Workflow | Filter by entity | GET ?entityId | Filtered |
| FUN-012 | Workflow | Filter by date | GET ?start&end | Filtered |
| FUN-013 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-014 | Workflow | Paginate | GET ?page | Paginated |
| FUN-015 | Workflow | Search | GET ?search | Searched |
| FUN-016 | Validation | Required name | Missing name | 400 |
| FUN-017 | Validation | Required entityId | Missing | 400 |
| FUN-018 | Validation | Valid date | Invalid date | 400 |
| FUN-019 | Validation | Valid type | Invalid type | 400 |
| FUN-020 | Validation | ID format | Invalid ID | 400 |
| FUN-021 | Validation | Note length | Too long | 400 |
| FUN-022 | Validation | Participant exists | Invalid | 404 |
| FUN-023 | Validation | Entity exists | Invalid | 404 |
| FUN-024 | Validation | No duplicate | Duplicate | 400 |
| FUN-025 | Validation | Permission | No permission | 403 |
| FUN-026 | Constraint | Unique constraint | Duplicate key | 409 |
| FUN-027 | Constraint | FK constraint | Orphan | 400 |
| FUN-028 | Constraint | Max participants | >50 | 400 |
| FUN-029 | Constraint | Max notes | >1000 | 400 |
| FUN-030 | Constraint | Max bulk | >100 | 400 |
| FUN-031 | Constraint | Org scope | Cross-org | 403 |
| FUN-032 | Constraint | Soft delete | Query deleted | Excluded |
| FUN-033 | Constraint | Date range | End < start | 400 |
| FUN-034 | Constraint | Future limit | >1 year | 400 |
| FUN-035 | Constraint | File size | >10MB | 413 |
| FUN-036 | Audit | Create logged | POST | Audit entry |
| FUN-037 | Audit | Update logged | PUT | Audit entry |
| FUN-038 | Audit | Delete logged | DELETE | Audit entry |
| FUN-039 | Audit | Note added | POST note | Audit entry |
| FUN-040 | Audit | Activity added | POST activity | Audit entry |
| FUN-041 | Audit | Timestamp | Any action | UTC |
| FUN-042 | Audit | User ID | Any action | User ID |
| FUN-043 | Audit | IP | Any action | IP |
| FUN-044 | Audit | Resource | Any action | Resource ID |
| FUN-045 | Audit | Outcome | Success/fail | Outcome |
| FUN-046 | Business | Soft-deleted excluded | Query | Excluded |
| FUN-047 | Business | Permission-based | Query | Scoped |
| FUN-048 | Business | Timezone | Dates | UTC |
| FUN-049 | Business | Hierarchy | Org filter | Rollup |
| FUN-050 | Business | Decimal | Currency | 2 decimals |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create → Get | Engagement | Match |
| INT-002 | CRUD | Update → Get | Engagement | Updated |
| INT-003 | CRUD | Delete → Get | Engagement | 404 |
| INT-004 | CRUD | Restore → Get | Engagement | Restored |
| INT-005 | CRUD | Create note → list | Notes | Note in list |
| INT-006 | CRUD | Delete note → list | Notes | Removed |
| INT-007 | CRUD | Add activity → timeline | Activity | In timeline |
| INT-008 | CRUD | Create follow-up | Follow-up | In list |
| INT-009 | CRUD | Complete follow-up | Follow-up | Status updated |
| INT-010 | CRUD | Clone → new | Engagement | New entity |
| INT-011 | Search | Search by text | Engagement | Matches |
| INT-012 | Search | Filter entity | Engagement | Filtered |
| INT-013 | Search | Filter by type | Engagement | Filtered |
| INT-014 | Search | Filter date | Engagement | Filtered |
| INT-015 | Search | Multi-filter | Engagement | Combined |
| INT-016 | Search | Sort + filter | Engagement | Both applied |
| INT-017 | Search | Filter + pagination | Engagement | Both applied |
| INT-018 | Search | Empty search | - | [] |
| INT-019 | Search | Partial match | Engagement | Fuzzy |
| INT-020 | Search | Export filtered | Engagement | Matches |
| INT-021 | Pagination | Page 1 | Engagement | First page |
| INT-022 | Pagination | Last page | Engagement | Partial OK |
| INT-023 | Pagination | Page size | Engagement | Correct size |
| INT-024 | Pagination | Invalid page | Engagement | 400 |
| INT-025 | Pagination | Boundary | Engagement | Correct |
| INT-026 | Relationships | Engagement → Entity | Engagement, Entity | Linked |
| INT-027 | Relationships | Engagement → Notes | Engagement, Notes | Linked |
| INT-028 | Relationships | Engagement → Activities | Engagement, Activities | Linked |
| INT-029 | Relationships | Engagement → Participants | Engagement, User | Linked |
| INT-030 | Relationships | Orphan handling | Deleted parent | Graceful |
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
| INT-049 | E2E | Multi-user | Users | No conflict |
| INT-050 | E2E | Session expiry | Auth | Clean fail |

---

## §6 Security Tests (50)

| ID | Category | Attack/Scenario | Target | Expected |
|----|----------|-----------------|-------|----------|
| SEC-001 | Injection | SQL injection | Filter | Sanitized |
| SEC-002 | Injection | XSS in note | Note | Encoded |
| SEC-003 | Injection | XSS in search | Search | Encoded |
| SEC-004 | Injection | Path traversal | File path | Rejected |
| SEC-005 | Injection | NoSQL injection | Filter | Rejected |
| SEC-006 | Injection | Command injection | Export | Rejected |
| SEC-007 | Injection | Header injection | Header | Rejected |
| SEC-008 | Injection | Log injection | Input | Sanitized |
| SEC-009 | Injection | LDAP injection | Search | Rejected |
| SEC-010 | Injection | Log4j-style | Input | Rejected |
| SEC-011 | Access | No auth | All | 401 |
| SEC-012 | Access | Wrong role | Create | 403 |
| SEC-013 | Access | Cross-org | Other org | 403 |
| SEC-014 | Access | Horizontal | Other user | 403 |
| SEC-015 | Access | Vertical | Admin | 403 |
| SEC-016 | Access | Expired token | All | 401 |
| SEC-017 | Access | Revoked token | All | 401 |
| SEC-018 | Access | Tampered token | All | 401 |
| SEC-019 | Access | Missing scope | OAuth | 403 |
| SEC-020 | Access | Service account | UI | 403 |
| SEC-021 | IDOR | Other org engagement | ID | 403 |
| SEC-022 | IDOR | Other user note | Note ID | 403 |
| SEC-023 | IDOR | Manipulate ID | Path ID | 403 |
| SEC-024 | IDOR | Enumeration | Sequential | Rate limit |
| SEC-025 | IDOR | Parameter pollution | Params | First wins |
| SEC-026 | Mass Assign | Add admin | Body | Ignored |
| SEC-027 | Mass Assign | Add role | Body | Ignored |
| SEC-028 | Mass Assign | Override org | Body | Ignored |
| SEC-029 | Mass Assign | Override user | Body | Ignored |
| SEC-030 | Mass Assign | Override permission | Body | Ignored |
| SEC-031 | Auth | Session fixation | Session | New session |
| SEC-032 | Auth | Session hijack | Token | Invalidated |
| SEC-033 | Auth | Replay | Old token | Rejected |
| SEC-034 | Auth | CSRF | State change | Token required |
| SEC-035 | Auth | Brute force | Login | Rate limit |
| SEC-036 | Data | PII in export | Export | Masked |
| SEC-037 | Data | Sensitive in logs | Logs | No PII |
| SEC-038 | Data | Error message | 500 | Generic |
| SEC-039 | Data | Stack trace | Exception | Not exposed |
| SEC-040 | Data | Debug prod | Config | Disabled |
| SEC-041 | OWASP | A01 Access | - | 403 |
| SEC-042 | OWASP | A02 Crypto | - | TLS |
| SEC-043 | OWASP | A03 Injection | - | Parametrized |
| SEC-044 | OWASP | A04 Design | - | Defensive |
| SEC-045 | OWASP | A05 Misconfig | - | Secure |
| SEC-046 | OWASP | A06 Vulnerable | - | No CVE |
| SEC-047 | OWASP | A07 Auth | - | Strong auth |
| SEC-048 | OWASP | A08 Integrity | - | Checks |
| SEC-049 | OWASP | A09 Logging | - | Audit |
| SEC-050 | OWASP | A10 SSRF | - | No internal |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users create same | Both succeed or unique |
| CON-002 | 2 users update same | Last write wins |
| CON-003 | Update during delete | 409 or 404 |
| CON-004 | 10 concurrent reads | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click create | Single create |
| CON-007 | Rapid filter changes | Last wins |
| CON-008 | Concurrent note add | No race |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue or 503 |
| CON-011 | Transaction isolation | No dirty read |
| CON-012 | Optimistic concurrency | Last write |
| CON-013 | Deadlock | Timeout retry |
| CON-014 | Export + update | Snapshot |
| CON-015 | Rate limit concurrent | Fair |
| CON-016 | Session expiry | Clean fail |
| CON-017 | Multiple creates same user | All succeed |
| CON-018 | Cache stampede | Single recompute |
| CON-019 | Lock contention | Timeout |
| CON-020 | Memory pressure | Graceful |
| CON-021 | Bulk during insert | Consistent |
| CON-022 | Hierarchy change | Snapshot |
| CON-023 | Permission change | Old for request |
| CON-024 | Bulk concurrent | Queue or limit |
| CON-025 | Read replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid date | Accepted |
| UNT-002 | Validation | Invalid date | Rejected |
| UNT-003 | Validation | Negative ID | Rejected |
| UNT-004 | Validation | Empty required | Rejected |
| UNT-005 | Validation | Invalid enum | Rejected |
| UNT-006 | Formatting | Date to string | ISO 8601 |
| UNT-007 | Formatting | Number | Localized |
| UNT-008 | Formatting | Percent | 2 decimal |
| UNT-009 | Calculation | Duration | Correct |
| UNT-010 | Calculation | Sum | Correct |
| UNT-011 | Calculation | Count | Correct |
| UNT-012 | Calculation | Average | Correct |
| UNT-013 | Calculation | Delta | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Draft | Draft only |
| UNT-018 | Status | Completed | Completed only |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No duplicates |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get single | < 100ms |
| PRF-002 | List 20 | < 500ms |
| PRF-003 | Create | < 200ms |
| PRF-004 | Update | < 200ms |
| PRF-005 | Delete | < 100ms |
| PRF-006 | Search | < 1s |
| PRF-007 | Add note | < 100ms |
| PRF-008 | Get notes | < 500ms |
| PRF-009 | List activities | < 500ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent create | < 500ms each |
| PRF-013 | Memory list | < 50MB |
| PRF-014 | Memory bulk | < 200MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 5 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users sustained | 10 min | 95% < 1s |
| LDT-002 | 50 users sustained | 10 min | 95% < 2s |
| LDT-003 | 100 users sustained | 10 min | 95% < 3s |
| LDT-004 | Spike 10→100 | 5 min | No crash |
| LDT-005 | Spike 50→200 | 5 min | Graceful |
| LDT-006 | Stress 200 | Until fail | Document |
| LDT-007 | Stress 500 | Until fail | Document |
| LDT-008 | 50 concurrent create | 5 min | Queue/limit |
| LDT-009 | Recovery after spike | 5 min | Baseline |
| LDT-010 | Recovery after stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Engagement CRUD | POS-001–005, FUN-001–003 |
| Notes | POS-006–007, NEG-024–026 |
| Activities | POS-008–009, INT-007 |
| Follow-ups | POS-010–011, FUN-006–007 |
| 3:1 Ratio | NEG-001–070, BND-001–070 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
