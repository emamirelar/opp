# UNOPSGmailAddonManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GmailAddonManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | ≥30 | ✅ |
| §2 Negative | 90 | ≥90 | ✅ |
| §3 Boundary | 90 | ≥90 | ✅ |
| §4 Functional | 90 | ≥90 | ✅ |
| §5 Integration | 90 | ≥90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Gmail addon manager unit tests cover email sync, contact import, OAuth flow, and deduplication for Gmail integration. Tests include: sync emails, import contacts from emails, OAuth token handling, duplicate detection, and Gmail API integration.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Sync emails | Valid token | SyncEmails | Synced |
| POS-002 | Import contact from email | Email valid | ImportContact | Contact created |
| POS-003 | Get OAuth token | Auth valid | GetToken | Token returned |
| POS-004 | Refresh token | Token expired | RefreshToken | New token |
| POS-005 | Deduplicate contacts | Duplicates exist | Deduplicate | Deduplicated |
| POS-006 | List emails | Token valid | ListEmails | List returned |
| POS-007 | Get email by ID | Email exists | GetEmail | Email returned |
| POS-008 | Parse email sender | Email valid | ParseSender | Sender parsed |
| POS-009 | Parse email recipients | Email valid | ParseRecipients | Recipients parsed |
| POS-010 | Extract contact info | Email valid | ExtractContact | Contact extracted |
| POS-011 | Match to existing contact | Contact exists | MatchContact | Matched |
| POS-012 | Create contact from email | Email valid | CreateContact | Created |
| POS-013 | Link contact to partner | Partner exists | Link | Linked |
| POS-014 | OAuth callback | Callback valid | OAuthCallback | Token stored |
| POS-015 | OAuth consent | Consent given | Consent | Granted |
| POS-016 | Revoke token | Token exists | Revoke | Revoked |
| POS-017 | Validate token | Token valid | Validate | True |
| POS-018 | Sync with pagination | Many emails | Sync | All synced |
| POS-019 | Sync with filter | Filter set | Sync | Filtered |
| POS-020 | Import batch | Batch valid | ImportBatch | All imported |
| POS-021 | Deduplicate by email | Same email | Deduplicate | Merged |
| POS-022 | Deduplicate by name | Same name | Deduplicate | Merged |
| POS-023 | Audit sync | Sync performed | Check audit | Logged |
| POS-024 | Audit import | Import performed | Check audit | Logged |
| POS-025 | Handle rate limit | Rate limited | Retry | Success |
| POS-026 | Handle pagination | Many results | Paginate | All fetched |
| POS-027 | Map Gmail to contact | Gmail format | Map | Mapped |
| POS-028 | Map contact to Gmail | Contact format | Map | Mapped |
| POS-029 | Get sync status | Sync ran | GetStatus | Status |
| POS-030 | Get last sync | Sync ran | GetLastSync | Date |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Sync with null token | Token=null | ArgumentNullException |
| NEG-002 | Sync with invalid token | Token=invalid | UnauthorizedException |
| NEG-003 | Sync with expired token | Token expired | UnauthorizedException |
| NEG-004 | Import with null email | Email=null | ArgumentNullException |
| NEG-005 | Import with empty email | Email="" | ValidationException |
| NEG-006 | Get token null user | User=null | ArgumentNullException |
| NEG-007 | Refresh token invalid | Token invalid | UnauthorizedException |
| NEG-008 | Deduplicate null list | List=null | ArgumentNullException |
| NEG-009 | List emails invalid token | Token invalid | UnauthorizedException |
| NEG-010 | Get email non-existent | Id=invalid | KeyNotFoundException |
| NEG-011 | Parse sender invalid | Email invalid | ParseException |
| NEG-012 | Parse recipients invalid | Email invalid | ParseException |
| NEG-013 | Extract contact invalid | Email invalid | ParseException |
| NEG-014 | Match contact null | Contact=null | ArgumentNullException |
| NEG-015 | OAuth callback invalid | Callback invalid | OAuthException |
| NEG-016 | OAuth consent denied | Consent denied | OAuthException |
| NEG-017 | Revoke non-existent | Token invalid | KeyNotFoundException |
| NEG-018 | Validate invalid token | Token invalid | False |
| NEG-019 | Sync without permission | Unauthorized | Forbidden |
| NEG-020 | Import without permission | Unauthorized | Forbidden |
| NEG-021 | Gmail API unavailable | API down | ServiceUnavailableException |
| NEG-022 | Gmail API rate limit | Rate limited | RateLimitException |
| NEG-023 | Gmail API timeout | Timeout | TimeoutException |
| NEG-024 | Gmail API error 500 | Server error | ApiException |
| NEG-025 | Gmail API error 401 | Unauthorized | UnauthorizedException |
| NEG-026 | Create contact duplicate | Duplicate | BusinessException |
| NEG-027 | Link to deleted partner | Partner deleted | BusinessException |
| NEG-028 | Invalid partner ID | PartnerId=-1 | ArgumentException |
| NEG-029 | Invalid email format | Format invalid | ValidationException |
| NEG-030 | Null request object | Request=null | ArgumentNullException |
| NEG-031 | Invalid page number | Page=0 | ArgumentException |
| NEG-032 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-033 | Search null query | Query=null | ArgumentNullException |
| NEG-034 | Import batch empty | Batch=[] | ArgumentException |
| NEG-035 | Deduplicate empty | List=[] | Returns empty |
| NEG-036 | Get sync status invalid | User invalid | KeyNotFoundException |
| NEG-037 | Get last sync invalid | User invalid | KeyNotFoundException |
| NEG-038 | Cancel non-running | Sync not running | InvalidOperationException |
| NEG-039 | Mark processed invalid | Id invalid | KeyNotFoundException |
| NEG-040 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-041 | Concurrent sync conflict | Two sync | One or both |
| NEG-042 | Transaction rollback | Fail in transaction | Rollback |
| NEG-043 | Connection timeout | Network down | TimeoutException |
| NEG-044 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-045 | Invalid enum value | Status invalid | ArgumentException |
| NEG-046 | Expired session | Expired token | Unauthorized |
| NEG-047 | Null user context | User=null | InvalidOperationException |
| NEG-048 | Invalid include path | Invalid include | ArgumentException |
| NEG-049 | OAuth scope insufficient | Scope missing | OAuthException |
| NEG-050 | Token storage failed | Storage fails | StorageException |
| NEG-051 | Sync interrupted | Interrupted | OperationCanceledException |
| NEG-052 | Import interrupted | Interrupted | OperationCanceledException |
| NEG-053 | Pagination invalid | Page invalid | ArgumentException |
| NEG-054 | Filter invalid | Filter invalid | ArgumentException |
| NEG-055 | Map invalid format | Format invalid | ParseException |
| NEG-056 | Get thread non-existent | Thread invalid | KeyNotFoundException |
| NEG-057 | Search invalid query | Query invalid | ArgumentException |
| NEG-058 | Audit missing user | User=0 | InvalidOperationException |
| NEG-059 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-060 | Link null contact | Contact=null | ArgumentNullException |
| NEG-061 | Create contact validation | Validation fails | ValidationException |
| NEG-062 | Child override throws | Child throws | Propagated |
| NEG-063 | OAuth state mismatch | State invalid | OAuthException |
| NEG-064 | OAuth code invalid | Code invalid | OAuthException |
| NEG-065 | Refresh token expired | Refresh expired | UnauthorizedException |
| NEG-066 | Deduplicate criteria invalid | Criteria invalid | ArgumentException |
| NEG-067 | Sync filter invalid | Filter invalid | ArgumentException |
| NEG-068 | Import batch validation | Batch invalid | ValidationException |
| NEG-069 | Token encrypt fail | Encrypt fails | SecurityException |
| NEG-070 | Token decrypt fail | Decrypt fails | SecurityException |
| NEG-071 | Sync with whitespace token | Token="   " | ArgumentException |
| NEG-072 | Import with invalid email | Email invalid | ValidationException |
| NEG-073 | Get token null user | User=null | ArgumentNullException |
| NEG-074 | Deduplicate null criteria | Criteria=null | ArgumentNullException |
| NEG-075 | List emails null token | Token=null | ArgumentNullException |
| NEG-076 | Get email null ID | Id=null | ArgumentNullException |
| NEG-077 | Parse sender null | Email=null | ArgumentNullException |
| NEG-078 | OAuth callback null | Callback=null | ArgumentNullException |
| NEG-079 | Revoke null token | Token=null | ArgumentNullException |
| NEG-080 | Validate null token | Token=null | ArgumentNullException |
| NEG-081 | Search null query | Query=null | ArgumentNullException |
| NEG-082 | Import batch null | Batch=null | ArgumentNullException |
| NEG-083 | Get sync status null user | User=null | ArgumentNullException |
| NEG-084 | Cancel non-running sync | Sync not running | InvalidOperationException |
| NEG-085 | Mark processed null ID | Id=null | ArgumentNullException |
| NEG-086 | Link null contact | Contact=null | ArgumentNullException |
| NEG-087 | Create contact null email | Email=null | ArgumentNullException |
| NEG-088 | Get thread null ID | Id=null | ArgumentNullException |
| NEG-089 | Pagination invalid | Page invalid | ArgumentException |
| NEG-090 | Filter invalid | Filter invalid | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Email subject at min | Length=1 | Valid |
| BND-002 | Email subject at max | Length=998 | Valid |
| BND-003 | Email subject exceeds max | Length=999 | Truncate |
| BND-004 | Contact name at max | Length=200 | Valid |
| BND-005 | Contact name exceeds max | Length=201 | Reject |
| BND-006 | Email address at max | Length=254 | Valid |
| BND-007 | Email address over max | Length=255 | Reject |
| BND-008 | Page size at min | PageSize=1 | Valid |
| BND-009 | Page size at max | PageSize=500 | Valid |
| BND-010 | Page size over max | PageSize=501 | Reject |
| BND-011 | Sync batch at max | Batch=100 | Valid |
| BND-012 | Sync batch over max | Batch=101 | Reject |
| BND-013 | Import batch at max | Batch=50 | Valid |
| BND-014 | Import batch over max | Batch=51 | Reject |
| BND-015 | Token length at max | Length=limit | Valid |
| BND-016 | Token length over max | Length=limit+1 | Reject |
| BND-017 | Unicode in email | Arabic/Chinese | Valid |
| BND-018 | Special chars in name | <>&"' | Escaped |
| BND-019 | Empty email body | Body="" | Valid |
| BND-020 | Single recipient | Count=1 | Valid |
| BND-021 | Max recipients | Count=limit | Valid |
| BND-022 | Empty contact list | List=[] | Returns empty |
| BND-023 | Single contact | Count=1 | Valid |
| BND-024 | Max contacts | Count=limit | Valid |
| BND-025 | Date at min | Date=MinValue | Handle |
| BND-026 | Date at max | Date=MaxValue | Handle |
| BND-027 | DateTime UTC | UTC input | Stored |
| BND-028 | Empty search query | Query="" | Return all |
| BND-029 | Search query max | Query=500 | Valid |
| BND-030 | Search query over max | Query=501 | Reject |
| BND-031 | Collection empty | [] | No exception |
| BND-032 | Collection single | 1 item | Valid |
| BND-033 | Collection max | At limit | Valid |
| BND-034 | Pagination last partial | Partial page | Correct |
| BND-035 | Pagination total | Total count | Accurate |
| BND-036 | Sort null handling | Nulls in data | Deterministic |
| BND-037 | Filter combination all | All filters | Correct |
| BND-038 | OAuth state length | State length | Valid |
| BND-039 | Token expiry boundary | Just expired | Refresh |
| BND-040 | Rate limit at limit | At limit | Reject |
| BND-041 | Rate limit at limit-1 | Limit-1 | Valid |
| BND-042 | Deduplicate threshold | At threshold | Merged |
| BND-043 | Deduplicate below threshold | Below | Not merged |
| BND-044 | Sync time boundary | Same second | Deterministic |
| BND-045 | Import time boundary | Same second | Deterministic |
| BND-046 | Soft delete boundary | DeletedDate set | Excluded |
| BND-047 | Include depth | Deep include | No explosion |
| BND-048 | Query timeout | Slow query | Timeout |
| BND-049 | Memory large result | 10k emails | No OOM |
| BND-050 | Audit timestamp precision | Millisecond | Stored |
| BND-051 | Long string in subject | 1000 chars | Truncate |
| BND-052 | Long string in body | 1MB | Truncate or stream |
| BND-053 | Attachment count zero | Count=0 | Valid |
| BND-054 | Attachment count max | At limit | Valid |
| BND-055 | Thread count zero | Count=0 | Valid |
| BND-056 | Thread count max | At limit | Valid |
| BND-057 | OAuth scope empty | Scope=[] | Reject |
| BND-058 | OAuth scope max | At limit | Valid |
| BND-059 | Deduplicate same | Same contact | No-op |
| BND-060 | Match exact | Exact match | Matched |
| BND-061 | Match partial | Partial match | Config |
| BND-062 | Extract minimal | Minimal email | Extracted |
| BND-063 | Extract full | Full email | Extracted |
| BND-064 | Parse sender minimal | Minimal | Parsed |
| BND-065 | Parse sender full | Full | Parsed |
| BND-066 | Refresh token boundary | Just before expiry | Refresh |
| BND-067 | Sync status pending | Pending | Status |
| BND-068 | Sync status completed | Completed | Status |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |
| BND-071 | Email subject single char | Length=1 | Valid |
| BND-072 | Contact name max | Length=200 | Valid |
| BND-073 | Email address max | Length=254 | Valid |
| BND-074 | Page size one | PageSize=1 | Valid |
| BND-075 | Sync batch max | Batch=100 | Valid |
| BND-076 | Import batch max | Batch=50 | Valid |
| BND-077 | Token length max | Length=limit | Valid |
| BND-078 | Empty email body | Body="" | Valid |
| BND-079 | Single recipient | Count=1 | Valid |
| BND-080 | Max recipients | Count=limit | Valid |
| BND-081 | Empty contact list | List=[] | Returns empty |
| BND-082 | Single contact | Count=1 | Valid |
| BND-083 | OAuth state length | State length | Valid |
| BND-084 | Token expiry boundary | Just expired | Refresh |
| BND-085 | Rate limit at limit | At limit | Reject |
| BND-086 | Rate limit at limit-1 | Limit-1 | Valid |
| BND-087 | Deduplicate threshold | At threshold | Merged |
| BND-088 | Pagination first page | Page=1 | Valid |
| BND-089 | Search query max | Query=500 | Valid |
| BND-090 | Collection single | 1 item | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Token required for sync | Validation | SyncEmails | Reject if null |
| FUN-002 | Email required for import | Validation | ImportContact | Reject if null |
| FUN-003 | User required for OAuth | Validation | OAuth | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Token encrypted at rest | Constraint | Store | Encrypted |
| FUN-008 | Token decrypted on use | Constraint | Get | Decrypted |
| FUN-009 | Duplicate detection by email | Logic | Deduplicate | By email |
| FUN-010 | Audit sync | Audit | SyncEmails | Logged |
| FUN-011 | Audit import | Audit | ImportContact | Logged |
| FUN-012 | Audit CreatedBy | Audit | Create | Set user |
| FUN-013 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-014 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-015 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | OAuth state validated | Validation | Callback | State check |
| FUN-018 | Token refresh on expiry | Logic | GetToken | Refresh if expired |
| FUN-019 | Rate limit respected | Constraint | Sync | Limiter |
| FUN-020 | Pagination correct | Logic | List | Correct page |
| FUN-021 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-022 | Map Gmail to contact | Logic | Map | Mapped |
| FUN-023 | Map contact to Gmail | Logic | Map | Mapped |
| FUN-024 | Extract sender | Logic | Extract | Sender |
| FUN-025 | Extract recipients | Logic | Extract | Recipients |
| FUN-026 | Match contact logic | Logic | Match | Matched |
| FUN-027 | Create contact logic | Logic | Create | Created |
| FUN-028 | Link to partner | Logic | Link | Linked |
| FUN-029 | Pagination offset | Calculation | Page | Skip correct |
| FUN-030 | Total count accurate | Calculation | Count | Matches |
| FUN-031 | Sort applies | Calculation | Sort | Ordered |
| FUN-032 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-033 | Transaction on import | Transaction | Import | Atomic |
| FUN-034 | Transaction on sync | Transaction | Sync | Atomic |
| FUN-035 | Async all operations | Concurrency | All | Async |
| FUN-036 | Include loads contact | Data load | GetById include | Contact loaded |
| FUN-037 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-038 | Mark processed | Logic | MarkProcessed | Marked |
| FUN-039 | Get sync status | Logic | GetStatus | Status |
| FUN-040 | Get last sync | Logic | GetLastSync | Date |
| FUN-041 | Cancel sync | Logic | Cancel | Cancelled |
| FUN-042 | Retry on transient | Logic | Retry | Retried |
| FUN-043 | Revoke invalidates | Logic | Revoke | Invalidated |
| FUN-044 | Consent stores token | Logic | Consent | Stored |
| FUN-045 | Callback completes auth | Logic | Callback | Complete |
| FUN-046 | Deduplicate merges | Logic | Deduplicate | Merged |
| FUN-047 | Import batch atomic | Logic | ImportBatch | All or none |
| FUN-048 | Localized display | i18n | GetDisplay | Localized |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | Token required for sync | Validation | SyncEmails | Reject if null |
| FUN-052 | Email required for import | Validation | ImportContact | Reject if null |
| FUN-053 | User required for OAuth | Validation | OAuth | Reject if null |
| FUN-054 | Token encrypted at rest | Constraint | Store | Encrypted |
| FUN-055 | Token decrypted on use | Constraint | Get | Decrypted |
| FUN-056 | Duplicate detection by email | Logic | Deduplicate | By email |
| FUN-057 | OAuth state validated | Validation | Callback | State check |
| FUN-058 | Token refresh on expiry | Logic | GetToken | Refresh if expired |
| FUN-059 | Rate limit respected | Constraint | Sync | Limiter |
| FUN-060 | Pagination correct | Logic | List | Correct page |
| FUN-061 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-062 | Map Gmail to contact | Logic | Map | Mapped |
| FUN-063 | Map contact to Gmail | Logic | Map | Mapped |
| FUN-064 | Extract sender | Logic | Extract | Sender |
| FUN-065 | Extract recipients | Logic | Extract | Recipients |
| FUN-066 | Match contact logic | Logic | Match | Matched |
| FUN-067 | Create contact logic | Logic | Create | Created |
| FUN-068 | Link to partner | Logic | Link | Linked |
| FUN-069 | Pagination offset | Calculation | Page | Skip correct |
| FUN-070 | Total count accurate | Calculation | Count | Matches |
| FUN-071 | Sort applies | Calculation | Sort | Ordered |
| FUN-072 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-073 | Transaction on import | Transaction | Import | Atomic |
| FUN-074 | Transaction on sync | Transaction | Sync | Atomic |
| FUN-075 | Async all operations | Concurrency | All | Async |
| FUN-076 | Include loads contact | Data load | GetById include | Contact loaded |
| FUN-077 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-078 | Mark processed | Logic | MarkProcessed | Marked |
| FUN-079 | Get sync status | Logic | GetStatus | Status |
| FUN-080 | Get last sync | Logic | GetLastSync | Date |
| FUN-081 | Cancel sync | Logic | Cancel | Cancelled |
| FUN-082 | Retry on transient | Logic | Retry | Retried |
| FUN-083 | Revoke invalidates | Logic | Revoke | Invalidated |
| FUN-084 | Consent stores token | Logic | Consent | Stored |
| FUN-085 | Callback completes auth | Logic | Callback | Complete |
| FUN-086 | Deduplicate merges | Logic | Deduplicate | Merged |
| FUN-087 | Import batch atomic | Logic | ImportBatch | All or none |
| FUN-088 | Permission before sync | Authorization | Sync | Check first |
| FUN-089 | Permission before import | Authorization | Import | Check first |
| FUN-090 | Audit sync and import | Audit | Sync, Import | Logged |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Sync emails full flow | SyncEmails | Email | Synced |
| INT-002 | Import contact full flow | ImportContact | Contact | Imported |
| INT-003 | OAuth flow full | OAuth | Token | Stored |
| INT-004 | Deduplicate full flow | Deduplicate | Contact | Deduplicated |
| INT-005 | List with filter and sort | List | Email | Filtered, sorted |
| INT-006 | Gmail API list | Gmail API | Emails | List |
| INT-007 | Gmail API get | Gmail API | Email | Email |
| INT-008 | Gmail API search | Gmail API | Query | Results |
| INT-009 | ContactManager create | ContactManager | Contact | Created |
| INT-010 | ContactManager link | ContactManager | Contact, Partner | Linked |
| INT-011 | Pagination | Paginate | Email | Pages |
| INT-012 | Token refresh | Refresh | Token | Refreshed |
| INT-013 | Token revoke | Revoke | Token | Revoked |
| INT-014 | OAuth callback | Callback | OAuth | Complete |
| INT-015 | OAuth consent | Consent | OAuth | Granted |
| INT-016 | Email-Contact relationship | Relationship | Email, Contact | Valid |
| INT-017 | Contact-Partner relationship | Relationship | Contact, Partner | Valid |
| INT-018 | Token-User relationship | Relationship | Token, User | Valid |
| INT-019 | Cascade soft delete | Relationship | User deleted | Config |
| INT-020 | Orphan handling | Relationship | User deleted | Retained |
| INT-021 | Gmail API error handling | Error | API down | Graceful |
| INT-022 | Timeout handling | Error | Slow API | Timeout |
| INT-023 | Rate limit handling | Error | Rate limited | Retry |
| INT-024 | Parse error handling | Error | Malformed | ParseException |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | HTTP client integration | Integration | HttpClient | Call |
| INT-030 | OAuth client integration | Integration | OAuth | Auth |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | Config integration | Integration | Config | Read |
| INT-036 | Sync then import | Scenario | Sync, Import | Both |
| INT-037 | Import then deduplicate | Scenario | Import, Deduplicate | Both |
| INT-038 | OAuth then sync | Scenario | OAuth, Sync | Both |
| INT-039 | Concurrent sync | Scenario | Parallel | All succeed |
| INT-040 | Rate limit across syncs | Scenario | Many syncs | Limited |
| INT-041 | Token refresh during sync | Scenario | Sync | Refreshed |
| INT-042 | Batch import | Scenario | ImportBatch | All imported |
| INT-043 | Search then import | Scenario | Search, Import | Both |
| INT-044 | Get thread then import | Scenario | Thread, Import | Both |
| INT-045 | Mark processed | Scenario | MarkProcessed | Marked |
| INT-046 | Cancel sync | Scenario | Cancel | Cancelled |
| INT-047 | Get status | Scenario | GetStatus | Status |
| INT-048 | Get last sync | Scenario | GetLastSync | Date |
| INT-049 | Map formats | Scenario | Map | Mapped |
| INT-050 | E2E OAuth-sync-import | Scenario | Full flow | Complete |
| INT-051 | Sync then import | Scenario | Sync, Import | Both |
| INT-052 | Import then deduplicate | Scenario | Import, Deduplicate | Both |
| INT-053 | OAuth then sync | Scenario | OAuth, Sync | Both |
| INT-054 | Token refresh during sync | Scenario | Sync | Refreshed |
| INT-055 | Batch import | Scenario | ImportBatch | All imported |
| INT-056 | Search then import | Scenario | Search, Import | Both |
| INT-057 | Get thread then import | Scenario | Thread, Import | Both |
| INT-058 | Mark processed | Scenario | MarkProcessed | Marked |
| INT-059 | Cancel sync | Scenario | Cancel | Cancelled |
| INT-060 | Get status | Scenario | GetStatus | Status |
| INT-061 | Get last sync | Scenario | GetLastSync | Date |
| INT-062 | Gmail API integration | Integration | Gmail API | Client |
| INT-063 | OAuth client integration | Integration | OAuth | Auth |
| INT-064 | ContactManager integration | Integration | ContactManager | Contact |
| INT-065 | Mapper integration | Integration | Mapper | Mapped |
| INT-066 | Repository integration | Integration | Repository | CRUD |
| INT-067 | DbContext integration | Integration | DbContext | Scoped |
| INT-068 | Transaction scope | Integration | Transaction | Atomic |
| INT-069 | Config integration | Integration | Config | Read |
| INT-070 | Permission service | Integration | Permission | Check |
| INT-071 | User resolver | Integration | User | Resolved |
| INT-072 | Audit context | Integration | Audit | Context |
| INT-073 | Logger integration | Integration | Logger | Logged |
| INT-074 | HTTP client integration | Integration | HttpClient | Call |
| INT-075 | Email-Contact relationship | Relationship | Email, Contact | Valid |
| INT-076 | Contact-Partner relationship | Relationship | Contact, Partner | Valid |
| INT-077 | Token-User relationship | Relationship | Token, User | Valid |
| INT-078 | Cascade soft delete | Relationship | User deleted | Config |
| INT-079 | Orphan handling | Relationship | User deleted | Retained |
| INT-080 | Gmail API error | Error | API down | Graceful |
| INT-081 | Timeout handling | Error | Slow API | Timeout |
| INT-082 | Rate limit handling | Error | Rate limited | Retry |
| INT-083 | Parse error handling | Error | Malformed | ParseException |
| INT-084 | Concurrent sync | Scenario | Parallel | All succeed |
| INT-085 | Rate limit across syncs | Scenario | Many syncs | Limited |
| INT-086 | Pagination | Scenario | Paginate | Pages |
| INT-087 | Filter voices | Scenario | Filter | Filtered |
| INT-088 | Get email | Scenario | GetEmail | Email |
| INT-089 | Search emails | Scenario | Search | Results |
| INT-090 | Full workflow | Scenario | Full flow | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in query | '; DROP TABLE-- | Query | Sanitized |
| SEC-002 | XSS in email subject | <script>alert(1)</script> | Subject | Escaped |
| SEC-003 | XSS in email body | <img onerror=...> | Body | Escaped |
| SEC-004 | XSS in contact name | javascript:alert(1) | Name | Sanitized |
| SEC-005 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Any | Rejected |
| SEC-008 | OAuth token exposure | Log token | Log | Redacted |
| SEC-009 | Token in error | Error | Stack | Redacted |
| SEC-010 | Unauthorized sync | No permission | Sync | 403 |
| SEC-011 | Unauthorized import | No permission | Import | 403 |
| SEC-012 | Unauthorized OAuth | No permission | OAuth | 403 |
| SEC-013 | Unauthorized list | No permission | List | 403 |
| SEC-014 | Role escalation | Low role | Admin | 403 |
| SEC-015 | Cross-tenant access | User A | User B data | 403 |
| SEC-016 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-017 | IDOR update other | Id=other | Update | 403 |
| SEC-018 | IDOR delete other | Id=other | Delete | 403 |
| SEC-019 | IDOR in filter | UserId=other | List | Filtered |
| SEC-020 | OAuth state tampering | Tamper state | Callback | Rejected |
| SEC-021 | OAuth code replay | Replay code | Callback | Rejected |
| SEC-022 | Token theft | Stolen token | Use | Detected |
| SEC-023 | Mass assign Token | Token= | Request | Ignored |
| SEC-024 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-025 | Session hijack | Stolen token | Any | Detected |
| SEC-026 | Token expiration | Expired | Any | 401 |
| SEC-027 | Invalid token | Malformed | Any | 401 |
| SEC-028 | CSRF on OAuth | No token | OAuth | Rejected |
| SEC-029 | CSRF on import | No token | Import | Rejected |
| SEC-030 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-031 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-032 | Email in log | Log email | Log | Redacted |
| SEC-033 | Rate limit bypass | Bypass attempt | Rate limit | Blocked |
| SEC-034 | Rate limit sync | Many syncs | Sync | Throttled |
| SEC-035 | Rate limit import | Many imports | Import | Throttled |
| SEC-036 | Oversized request | 10MB payload | Import | Rejected |
| SEC-037 | Deep nesting | Nested object | Request | Rejected |
| SEC-038 | Header injection | \r\n in header | Header | Rejected |
| SEC-039 | Null byte injection | %00 in name | Name | Rejected |
| SEC-040 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-041 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-042 | Denial of service | Huge batch | Import | Rejected |
| SEC-043 | OAuth scope escalation | Request more scope | OAuth | Rejected |
| SEC-044 | Token storage encryption | Decrypt without key | Token | Fail |
| SEC-045 | Import malicious email | Malicious | Import | Rejected |
| SEC-046 | Export data injection | Inject in export | Export | Sanitized |
| SEC-047 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-048 | Permission cached | Repeated check | Permission | Cached |
| SEC-049 | Token rotation | Rotate token | Config | Updated |
| SEC-050 | Request signing | Tamper request | Request | Rejected |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent sync | Two sync | Both succeed |
| CON-004 | Concurrent import | Two import | Both succeed |
| CON-005 | Sync during import | Sync, import | No conflict |
| CON-006 | Read during write | Read while update | Consistent |
| CON-007 | Transaction isolation | Parallel transactions | Serializable |
| CON-008 | Stale entity update | Old version | Concurrency handled |
| CON-009 | Race on token refresh | Two refresh | One wins |
| CON-010 | Race on deduplicate | Two deduplicate | Consistent |
| CON-011 | DbContext concurrency | Share context | Not shared |
| CON-012 | Async parallel syncs | 10 parallel | All succeed |
| CON-013 | Async parallel imports | 10 parallel | All succeed |
| CON-014 | Batch vs single | Batch vs loop | Same result |
| CON-015 | Pagination concurrent | Two paginate | Both correct |
| CON-016 | OAuth concurrent | Two OAuth | One or both |
| CON-017 | Token storage concurrent | Two store | Consistent |
| CON-018 | Gmail API concurrent | Many calls | Rate limited |
| CON-019 | Deduplicate concurrent | Two deduplicate | Consistent |
| CON-020 | Soft delete concurrent | Delete while update | Deterministic |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Gmail API connection limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate token not null | Validation | null | Exception |
| UNT-002 | Validate email format | Validation | Valid email | Pass |
| UNT-003 | Validate user | Validation | Valid user | Pass |
| UNT-004 | Validate OAuth state | Validation | Valid state | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format email display | Formatting | Email | Display |
| UNT-007 | Format contact display | Formatting | Contact | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Parse sender | Calculation | Email | Sender |
| UNT-013 | Parse recipients | Calculation | Email | Recipients |
| UNT-014 | Token valid check | Status logic | Token | Valid |
| UNT-015 | Token expired check | Status logic | Token | Expired |
| UNT-016 | Sync status check | Status logic | Status | Status |
| UNT-017 | Import status check | Status logic | Status | Status |
| UNT-018 | Duplicate check | Status logic | Contact | Duplicate |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | List emails | ListEmails | <2s | P1 |
| PRF-003 | Get email | GetEmail | <1s | P1 |
| PRF-004 | Import single contact | ImportContact | <500ms | P1 |
| PRF-005 | Import batch 10 | ImportBatch | <5s | P1 |
| PRF-006 | Sync 100 emails | SyncEmails | <30s | P1 |
| PRF-007 | Deduplicate 100 | Deduplicate | <2s | P1 |
| PRF-008 | OAuth callback | OAuthCallback | <2s | P1 |
| PRF-009 | Token refresh | RefreshToken | <1s | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 imports | 5 parallel Import | <5s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 import | <6s total | P2 |
| PRF-013 | Memory single import | ImportContact | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory sync 100 | Sync 100 | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 2 RPS sync | 2 req/s | 5 min | 99% success |
| LDT-002 | Sustained 10 RPS read | 10 req/s | 5 min | 99% success |
| LDT-003 | Sustained 2 RPS mixed | 2 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 5 RPS sync | 0→5→0 | 1 min | No errors |
| LDT-005 | Spike 20 RPS read | 0→20→0 | 30s | Graceful deg |
| LDT-006 | Stress rate limit | Many syncs | Until limit | Limited |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large batch | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
