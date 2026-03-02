# UNOPSAiPromptManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/AiPromptManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
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

**Ratio Compliance:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

AI prompt manager unit tests cover CRUD prompts, template variables, versioning, and usage tracking. Tests include: prompt CRUD, template creation, variable substitution, syntax validation, version history, revert to version, usage logging, statistics, and prompt categorization.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create prompt | Valid data | Create | Prompt created |
| POS-002 | Get prompt by ID | Prompt exists | GetById | Prompt returned |
| POS-003 | Update prompt | Prompt exists | Update | Updated |
| POS-004 | Delete prompt | Prompt exists | Delete | Soft deleted |
| POS-005 | List prompts | Prompts exist | List | List returned |
| POS-006 | Replace variables | Template has vars | ReplaceVariables | Replaced |
| POS-007 | Validate template syntax | Valid template | ValidateSyntax | Valid |
| POS-008 | Handle missing variables | Vars missing | HandleMissing | Defaults |
| POS-009 | Escape special characters | Special chars | Escape | Escaped |
| POS-010 | Apply default values | Defaults defined | ApplyDefaults | Applied |
| POS-011 | Create new version | Prompt exists | CreateVersion | New version |
| POS-012 | Get version history | Versions exist | GetVersionHistory | History |
| POS-013 | Revert to previous version | Version exists | Revert | Reverted |
| POS-014 | Categorize prompt | Valid category | Categorize | Categorized |
| POS-015 | Filter by category | Prompts exist | FilterByCategory | Filtered |
| POS-016 | Log prompt usage | Prompt used | LogUsage | Logged |
| POS-017 | Get usage statistics | Usage exists | GetUsageStats | Stats |
| POS-018 | Track popular prompts | Usage exists | GetPopular | Popular |
| POS-019 | Audit CreatedBy | Create | Check audit | Set |
| POS-020 | Audit CreatedDate | Create | Check audit | UTC |
| POS-021 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-022 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-023 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-024 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-025 | Get required variables | Template exists | GetRequiredVars | Vars |
| POS-026 | Get optional variables | Template exists | GetOptionalVars | Vars |
| POS-027 | Get by name | Name exists | GetByName | Prompt |
| POS-028 | Get by category | Category exists | GetByCategory | Prompts |
| POS-029 | Pagination | Many prompts | List page | Page |
| POS-030 | Sort by name | Prompts exist | Sort | Ordered |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ArgumentNullException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Create with null template | Template=null | ArgumentNullException |
| NEG-004 | Get by zero ID | Id=0 | ArgumentException |
| NEG-005 | Get by negative ID | Id=-1 | ArgumentException |
| NEG-006 | Update non-existent | Id=99999 | KeyNotFoundException |
| NEG-007 | Delete non-existent | Id=99999 | KeyNotFoundException |
| NEG-008 | Replace variables null template | Template=null | ArgumentNullException |
| NEG-009 | Replace variables null vars | Vars=null | ArgumentNullException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | Create without permission | Unauthorized | Forbidden |
| NEG-012 | Update without permission | Unauthorized | Forbidden |
| NEG-013 | Delete without permission | Unauthorized | Forbidden |
| NEG-014 | Create version unauthorized | Unauthorized | Forbidden |
| NEG-015 | Revert unauthorized | Unauthorized | Forbidden |
| NEG-016 | SQL injection in search | '; DROP | Rejected |
| NEG-017 | XSS in template | <script> | Escaped |
| NEG-018 | Path traversal | ../../../etc | Rejected |
| NEG-019 | Template injection | {{malicious}} | Sanitized |
| NEG-020 | Variable injection | Malicious var | Sanitized |
| NEG-021 | Invalid template syntax | Syntax invalid | ValidationException |
| NEG-022 | Missing required variable | Required missing | ValidationException |
| NEG-023 | Duplicate name | Name exists | BusinessException |
| NEG-024 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-025 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-026 | Connection timeout | DB unavailable | TimeoutException |
| NEG-027 | Revert to invalid version | Version invalid | KeyNotFoundException |
| NEG-028 | Create version deleted prompt | Prompt deleted | KeyNotFoundException |
| NEG-029 | Expired session | Expired token | Unauthorized |
| NEG-030 | Null user context | User=null | InvalidOperationException |
| NEG-031 | Invalid category | Category invalid | ArgumentException |
| NEG-032 | Invalid page number | Page=0 | ArgumentException |
| NEG-033 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-034 | Search null term | Term=null | ArgumentNullException |
| NEG-035 | Filter malformed | Malformed filter | ArgumentException |
| NEG-036 | Child override throws | Child throws | Propagated |
| NEG-037 | Template too long | 100k chars | ValidationException |
| NEG-038 | Variable name invalid | Name invalid | ValidationException |
| NEG-039 | Audit missing user | User=0 | InvalidOperationException |
| NEG-040 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-041 | Pagination overflow | Page too large | Empty or error |
| NEG-042 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-043 | GetByName non-existent | Name invalid | Null or KeyNotFoundException |
| NEG-044 | GetByCategory invalid | Category invalid | ArgumentException |
| NEG-045 | SetActiveVersion invalid | Version invalid | KeyNotFoundException |
| NEG-046 | GetActiveVersion no versions | No versions | Null or exception |
| NEG-047 | LogUsage null prompt | Prompt=null | ArgumentNullException |
| NEG-048 | GetUsageStats invalid | Period invalid | ArgumentException |
| NEG-049 | Import invalid data | Malformed | ValidationException |
| NEG-050 | Export unauthorized | Unauthorized | Forbidden |
| NEG-051 | ApplyDefaults null defaults | Defaults=null | ArgumentNullException |
| NEG-052 | Escape null input | Input=null | ArgumentNullException |
| NEG-053 | ValidateSyntax invalid | Invalid | ValidationException |
| NEG-054 | HandleMissing invalid | Config invalid | ArgumentException |
| NEG-055 | Categorize invalid | Category invalid | ArgumentException |
| NEG-056 | GetVersionHistory deleted | Prompt deleted | KeyNotFoundException |
| NEG-057 | CreateVersion deleted | Prompt deleted | KeyNotFoundException |
| NEG-058 | Cross-tenant access | Other tenant | Forbidden |
| NEG-059 | Invalid include path | Invalid include | ArgumentException |
| NEG-060 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-061 | Invalid enum value | Category invalid | ArgumentException |
| NEG-062 | Revert same version | Current version | No-op or BusinessException |
| NEG-063 | Create version max | At max | BusinessException |
| NEG-064 | Variable circular reference | Circular | BusinessException |
| NEG-065 | Template recursive | Recursive | ValidationException |
| NEG-066 | GetPopular invalid period | Period invalid | ArgumentException |
| NEG-067 | Import duplicate name | Name exists | BusinessException or skip |
| NEG-068 | Export format invalid | Format invalid | ArgumentException |
| NEG-069 | GetRequiredVars invalid | Template invalid | ArgumentException |
| NEG-070 | GetOptionalVars invalid | Template invalid | ArgumentException |
| NEG-071 | Search prompts null filter | Filter=null | ArgumentNullException |
| NEG-072 | Export null format | Format=null | ArgumentNullException |
| NEG-073 | Import null stream | Stream=null | ArgumentNullException |
| NEG-074 | GetByName empty string | Name="" | ArgumentException |
| NEG-075 | ReplaceVariables empty template | Template="" | ValidationException |
| NEG-076 | CreateVersion null prompt | Prompt=null | ArgumentNullException |
| NEG-077 | Revert null version | Version=null | ArgumentNullException |
| NEG-078 | LogUsage invalid prompt ID | PromptId=0 | ArgumentException |
| NEG-079 | GetUsageStats null period | Period=null | ArgumentNullException |
| NEG-080 | Categorize null category | Category=null | ArgumentNullException |
| NEG-081 | FilterByCategory invalid | Category invalid | ArgumentException |
| NEG-082 | GetByCode non-existent | Code invalid | KeyNotFoundException |
| NEG-083 | Template variable reserved | Reserved name | ValidationException |
| NEG-084 | Version number negative | Version=-1 | ArgumentException |
| NEG-085 | Pagination negative page | Page=-1 | ArgumentException |
| NEG-086 | Sort field SQL injection | Field='; DROP | Rejected |
| NEG-087 | Cache key collision | Collision | Handle |
| NEG-088 | Template parse error | Parse error | ValidationException |
| NEG-089 | Variable type mismatch | Type mismatch | ValidationException |
| NEG-090 | Prompt name whitespace only | Name="   " | ValidationException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min | Length=1 | Valid |
| BND-002 | Name at max | Length=255 | Valid |
| BND-003 | Name over max | Length=256 | Reject |
| BND-004 | Template at min | Length=1 | Valid |
| BND-005 | Template at max | 50k chars | Valid |
| BND-006 | Template over max | 50k+1 | Reject |
| BND-007 | ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-008 | ID at zero | Id=0 | Reject |
| BND-009 | Page size at min | PageSize=1 | Valid |
| BND-010 | Page size at max | PageSize=100 | Valid |
| BND-011 | Page size over max | PageSize=101 | Reject |
| BND-012 | Version at 1 | Version=1 | Valid |
| BND-013 | Version at max | Version=max | Valid |
| BND-014 | Variables count zero | No vars | Valid |
| BND-015 | Variables count max | 50 vars | Valid |
| BND-016 | Variables count over max | 51 vars | Reject |
| BND-017 | Unicode in template | Arabic/Chinese | Stored |
| BND-018 | Special chars in name | <>&"' | Escaped |
| BND-019 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-020 | Empty category | Category="" | Valid or reject |
| BND-021 | Single variable | 1 var | Valid |
| BND-022 | Many variables | 50 vars | Valid |
| BND-023 | Date at min | Date=MinValue | Handle |
| BND-024 | Date at max | Date=MaxValue | Handle |
| BND-025 | DateTime UTC | UTC input | Stored |
| BND-026 | Empty search term | Term="" | Return all |
| BND-027 | Search term max | Term=500 | Valid |
| BND-028 | Search term over max | Term=501 | Reject |
| BND-029 | Pagination last partial | Partial page | Correct |
| BND-030 | Pagination total | Total count | Accurate |
| BND-031 | Sort null handling | Nulls in data | Deterministic |
| BND-032 | Filter combination all | All filters | Correct |
| BND-033 | Category enum boundary | Last enum | Valid |
| BND-034 | Version null | No version | Null |
| BND-035 | Version max int | Version=2147483647 | Handle |
| BND-036 | Soft delete boundary | DeletedDate set | Excluded |
| BND-037 | Include depth | Deep include | No explosion |
| BND-038 | Query timeout | Slow query | Timeout |
| BND-039 | Audit timestamp precision | Millisecond | Stored |
| BND-040 | Async cancellation | Cancel token | OperationCanceledException |
| BND-041 | Task timeout | Timeout | TimeoutException |
| BND-042 | Concurrent same second | Same timestamp | Deterministic |
| BND-043 | Usage count zero | No usage | 0 |
| BND-044 | Usage count max | Many usage | Count |
| BND-045 | Default value empty | Empty default | Applied |
| BND-046 | Default value long | Long default | Truncate |
| BND-047 | Variable name max | Length=100 | Valid |
| BND-048 | Variable name over max | Length=101 | Reject |
| BND-049 | Variable value max | Length=4000 | Valid |
| BND-050 | Variable value over max | Length=4001 | Reject |
| BND-051 | Export large result | 10k rows | Stream |
| BND-052 | Import large | 1000 records | Valid |
| BND-053 | Filter empty result | No match | Empty list |
| BND-054 | Sort empty | Empty list | No exception |
| BND-055 | Pagination empty | No data | Empty |
| BND-056 | GetVersionHistory empty | No versions | [] |
| BND-057 | GetUsageStats empty | No usage | Empty |
| BND-058 | GetPopular empty | No usage | [] |
| BND-059 | ReplaceVariables empty | No vars | Original |
| BND-060 | ApplyDefaults empty | No defaults | Original |
| BND-061 | HandleMissing empty | No missing | Original |
| BND-062 | Escape boundary | All special | Escaped |
| BND-063 | ValidateSyntax valid | Valid | True |
| BND-064 | ValidateSyntax invalid | Invalid | False |
| BND-065 | GetByName exact | Exact | Found |
| BND-066 | GetByCategory exact | Exact | Found |
| BND-067 | GetActiveVersion first | First | Valid |
| BND-068 | GetActiveVersion last | Last | Valid |
| BND-069 | Revert to first | First version | Reverted |
| BND-070 | Concurrent version create | Two create | Both or one |
| BND-071 | Page number at max | Page=max | Valid |
| BND-072 | Page number over max | Page=max+1 | Empty |
| BND-073 | Template single char var | {{x}} | Valid |
| BND-074 | Template nested braces | {{outer{{inner}}}} | Handle |
| BND-075 | Variable name underscore | _valid | Valid |
| BND-076 | Variable name hyphen | valid-name | Config |
| BND-077 | Category empty string | "" | Valid or reject |
| BND-078 | List zero results | No prompts | [] |
| BND-079 | List single result | 1 prompt | [prompt] |
| BND-080 | CreateVersion at limit | At max versions | Config |
| BND-081 | Revert to current | Same version | No-op |
| BND-082 | LogUsage zero count | First usage | 1 |
| BND-083 | GetPopular single | 1 prompt | [prompt] |
| BND-084 | Export empty list | No prompts | Empty |
| BND-085 | Import empty | Empty file | ValidationException |
| BND-086 | Search exact match | Exact term | Match |
| BND-087 | Search partial match | Partial | Match |
| BND-088 | Filter by multiple categories | Multi | Filtered |
| BND-089 | Sort ascending | Asc | Ordered |
| BND-090 | Sort descending | Desc | Ordered |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | Template required | Validation | Create | Reject if null |
| FUN-003 | Category valid | Validation | Categorize | Reject invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Name unique | Constraint | Create | Reject duplicate |
| FUN-008 | Template syntax valid | Constraint | Create | Reject invalid |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Version sequential | Constraint | CreateVersion | Sequential |
| FUN-017 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-018 | GetByCategory excludes deleted | Constraint | GetByCategory | Excludes deleted |
| FUN-019 | Replace syntax | Logic | ReplaceVariables | {{var}} |
| FUN-020 | Default apply | Logic | ApplyDefaults | Applied |
| FUN-021 | Missing handle | Logic | HandleMissing | Default or error |
| FUN-022 | Escape logic | Logic | Escape | Escaped |
| FUN-023 | Version history order | Logic | GetVersionHistory | Chronological |
| FUN-024 | Revert copies | Logic | Revert | Copied |
| FUN-025 | Usage increment | Logic | LogUsage | Incremented |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on version | Transaction | CreateVersion | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads category | Data load | GetById include | Category loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | GetRequiredVars parse | Logic | GetRequiredVars | Parsed |
| FUN-036 | GetOptionalVars parse | Logic | GetOptionalVars | Parsed |
| FUN-037 | ValidateSyntax parse | Logic | ValidateSyntax | Parsed |
| FUN-038 | GetUsageStats aggregate | Logic | GetUsageStats | Aggregated |
| FUN-039 | GetPopular order | Logic | GetPopular | By usage |
| FUN-040 | SetActiveVersion update | Logic | SetActiveVersion | Updated |
| FUN-041 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-042 | Import validation | Logic | Import | Validated |
| FUN-043 | Config max versions | Config | CreateVersion | Config |
| FUN-044 | Config variable format | Config | ReplaceVariables | Config |
| FUN-045 | Localized display | i18n | GetDisplay | Localized |
| FUN-046 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-047 | Permission cached | Performance | Repeated check | Cached |
| FUN-048 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-049 | Template caching | Performance | ReplaceVariables | Cached |
| FUN-050 | Usage caching | Performance | GetUsageStats | Cached |
| FUN-051 | GetByName case | Logic | GetByName | Config |
| FUN-052 | Search case insensitive | Logic | Search | Matching |
| FUN-053 | Filter by active only | Logic | List | Active |
| FUN-054 | Variable substitution order | Logic | ReplaceVariables | Order |
| FUN-055 | Default value precedence | Logic | ApplyDefaults | Precedence |
| FUN-056 | Version rollback integrity | Logic | Revert | Integrity |
| FUN-057 | Category filter cascade | Logic | FilterByCategory | Cascade |
| FUN-058 | Usage stats date range | Logic | GetUsageStats | Range |
| FUN-059 | GetPopular limit | Logic | GetPopular | Limited |
| FUN-060 | Export format selection | Logic | Export | Format |
| FUN-061 | Import conflict resolution | Logic | Import | Resolution |
| FUN-062 | Template variable scope | Logic | ReplaceVariables | Scope |
| FUN-063 | Audit trail completeness | Audit | All ops | Complete |
| FUN-064 | Soft delete cascade | Constraint | Delete | Cascade |
| FUN-065 | Pagination consistency | Calculation | List | Consistent |
| FUN-066 | Sort multi-column | Calculation | Sort | Multi |
| FUN-067 | Filter OR logic | Filter | OR filter | Match |
| FUN-068 | Transaction on update | Transaction | Update | Atomic |
| FUN-069 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-070 | Include selective | Data load | Include | Selective |
| FUN-071 | Cache invalidation | Logic | Update | Invalidated |
| FUN-072 | Variable validation | Logic | ReplaceVariables | Validated |
| FUN-073 | Template escape | Logic | Escape | Escaped |
| FUN-074 | Version comparison | Logic | Revert | Compare |
| FUN-075 | Category hierarchy | Logic | GetByCategory | Hierarchy |
| FUN-076 | Search ranking | Logic | Search | Ranked |
| FUN-077 | Export encoding | Logic | Export | Encoding |
| FUN-078 | Import encoding | Logic | Import | Encoding |
| FUN-079 | Config pagination | Config | List | Config |
| FUN-080 | Config search | Config | Search | Config |
| FUN-081 | Permission per action | Authorization | Per action | Check |
| FUN-082 | User context audit | Audit | Create | User |
| FUN-083 | Timestamp UTC | Audit | All | UTC |
| FUN-084 | Deleted exclude GetByName | Constraint | GetByName | Excluded |
| FUN-085 | Deleted exclude Search | Constraint | Search | Excluded |
| FUN-086 | Variable type coercion | Logic | ReplaceVariables | Coerce |
| FUN-087 | Template literal | Logic | ReplaceVariables | Literal |
| FUN-088 | Version diff | Logic | GetVersionHistory | Diff |
| FUN-089 | Usage aggregation | Logic | GetUsageStats | Aggregated |
| FUN-090 | Prompt lifecycle | Workflow | Full cycle | Complete |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create prompt full flow | Create | Prompt | Created |
| INT-002 | Get prompt full flow | GetById | Prompt | Returned |
| INT-003 | Update prompt full flow | Update | Prompt | Updated |
| INT-004 | Delete prompt full flow | Delete | Prompt | Soft deleted |
| INT-005 | Replace variables full flow | ReplaceVariables | Prompt | Replaced |
| INT-006 | Get with category | GetById | Prompt, Category | Category loaded |
| INT-007 | List with filter and sort | List | Prompt | Filtered, sorted |
| INT-008 | Create version | CreateVersion | Prompt | New version |
| INT-009 | Revert version | Revert | Prompt | Reverted |
| INT-010 | Prompt-Category relationship | Relationship | Prompt, Category | FK valid |
| INT-011 | Prompt-Version relationship | Relationship | Prompt, Version | Valid |
| INT-012 | Cascade soft delete | Relationship | Category deleted | Config |
| INT-013 | Orphan handling | Relationship | Category deleted | Retained |
| INT-014 | DB error handling | Error | DB down | Graceful |
| INT-015 | Timeout handling | Error | Slow | Timeout |
| INT-016 | Constraint violation | Error | FK violation | Clear error |
| INT-017 | Permission service integration | Integration | Permission | Check |
| INT-018 | User resolver integration | Integration | User | Resolved |
| INT-019 | Audit context integration | Integration | Audit | Context |
| INT-020 | Logger integration | Integration | Log | Logged |
| INT-021 | AiContextualService integration | Integration | Context | Context |
| INT-022 | Mapper integration | Integration | Map | Correct |
| INT-023 | Repository integration | Integration | Repository | CRUD |
| INT-024 | DbContext integration | Integration | DbContext | Scoped |
| INT-025 | Transaction scope | Integration | Transaction | Atomic |
| INT-026 | Full version cycle | Scenario | Create, Version, Revert | Complete |
| INT-027 | Full variable cycle | Scenario | Replace, Validate | Complete |
| INT-028 | Full usage cycle | Scenario | Log, GetStats | Complete |
| INT-029 | Concurrent create | Scenario | Parallel | All succeed |
| INT-030 | Export import cycle | Scenario | Export, Import | Complete |
| INT-031 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-032 | Filter by category | Scenario | FilterByCategory | Filtered |
| INT-033 | Search prompts | Scenario | Search | Matching |
| INT-034 | Get by name | Scenario | GetByName | Prompt |
| INT-035 | Get active version | Scenario | GetActiveVersion | Version |
| INT-036 | Set active version | Scenario | SetActiveVersion | Set |
| INT-037 | Get version history | Scenario | GetVersionHistory | History |
| INT-038 | Get usage stats | Scenario | GetUsageStats | Stats |
| INT-039 | Get popular | Scenario | GetPopular | Popular |
| INT-040 | Variable substitution | Scenario | ReplaceVariables | Substituted |
| INT-041 | Default values | Scenario | ApplyDefaults | Applied |
| INT-042 | Handle missing | Scenario | HandleMissing | Handled |
| INT-043 | Escape special | Scenario | Escape | Escaped |
| INT-044 | Validate syntax | Scenario | ValidateSyntax | Valid |
| INT-045 | Get required vars | Scenario | GetRequiredVars | Vars |
| INT-046 | Get optional vars | Scenario | GetOptionalVars | Vars |
| INT-047 | Categorize | Scenario | Categorize | Categorized |
| INT-048 | Audit trail | Scenario | Operations | Trail |
| INT-049 | Config override | Scenario | Config | Override |
| INT-050 | E2E create-version-revert | Scenario | Full cycle | Complete |
| INT-051 | Create with category | Scenario | Create | Category set |
| INT-052 | Update category | Scenario | Update | Category updated |
| INT-053 | List by category | Scenario | List | Filtered |
| INT-054 | Replace then validate | Scenario | Replace, Validate | Complete |
| INT-055 | Version then revert | Scenario | Version, Revert | Complete |
| INT-056 | Log then stats | Scenario | Log, GetStats | Complete |
| INT-057 | Search then get | Scenario | Search, GetById | Complete |
| INT-058 | Export then import | Scenario | Export, Import | Roundtrip |
| INT-059 | Pagination full | Scenario | Paginate all | Complete |
| INT-060 | Sort multi-column | Scenario | Sort | Ordered |
| INT-061 | Filter multi-category | Scenario | Filter | Filtered |
| INT-062 | DbContext scope | Integration | Request | Scoped |
| INT-063 | Permission cascade | Integration | Role | Cascade |
| INT-064 | User context propagation | Integration | Request | Propagated |
| INT-065 | Audit chain | Integration | Operations | Chained |
| INT-066 | Cache service | Integration | Cache | Service |
| INT-067 | Config service | Integration | Config | Service |
| INT-068 | Error handling chain | Integration | Error | Handled |
| INT-069 | Validation chain | Integration | Create | Validated |
| INT-070 | Mapping chain | Integration | Entity | Mapped |
| INT-071 | Repository CRUD | Integration | Repository | CRUD |
| INT-072 | DbContext save | Integration | SaveChanges | Saved |
| INT-073 | Transaction rollback | Integration | Error | Rollback |
| INT-074 | Concurrent list | Scenario | Parallel list | All succeed |
| INT-075 | Concurrent get | Scenario | Parallel get | All succeed |
| INT-076 | Create update delete | Scenario | CRUD | Complete |
| INT-077 | Version history full | Scenario | Multiple versions | Complete |
| INT-078 | Usage tracking full | Scenario | Multiple logs | Complete |
| INT-079 | Category migration | Scenario | Re-categorize | Complete |
| INT-080 | Template migration | Scenario | Update template | Complete |
| INT-081 | Variable migration | Scenario | Add variable | Complete |
| INT-082 | Search pagination | Scenario | Search, Page | Complete |
| INT-083 | Export filter | Scenario | Export filtered | Complete |
| INT-084 | Import validate | Scenario | Import validate | Complete |
| INT-085 | Permission check flow | Integration | Auth | Check |
| INT-086 | User resolution flow | Integration | User | Resolved |
| INT-087 | Audit flow | Integration | Audit | Logged |
| INT-088 | Logging flow | Integration | Log | Logged |
| INT-089 | Context flow | Integration | Context | Built |
| INT-090 | E2E full lifecycle | Scenario | All operations | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in template | <script>alert(1)</script> | Template | Escaped |
| SEC-005 | XSS in name | <img onerror=...> | Name | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized create | No permission | Create | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized create version | No permission | CreateVersion | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B prompt | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR update other | Id=other | Update | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | CategoryId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign Template | Template=manipulated | Request | Validated |
| SEC-025 | Template injection | {{malicious}} | Template | Sanitized |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Template tampering | Tamper template | Access | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit create | Many creates | Create | Throttled |
| SEC-036 | Rate limit update | Many updates | Update | Throttled |
| SEC-037 | Rate limit list | Many lists | List | Throttled |
| SEC-038 | Oversized request | 10MB payload | Create | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in name | Name | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge template | Create | Rejected |
| SEC-045 | Variable injection | Malicious var | ReplaceVariables | Sanitized |
| SEC-046 | Version injection | Invalid version | Revert | Rejected |
| SEC-047 | Category injection | Invalid category | Categorize | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Export ACL | Direct access | Export | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double create same name | Two create | One or both |
| CON-004 | Concurrent create | Two create | Both or one |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on create version | Two create | One or both |
| CON-009 | Race on revert | Two revert | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel gets | 10 parallel | All succeed |
| CON-012 | Async parallel updates | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Replace variables concurrent | Two replace | Both succeed |
| CON-016 | Create version concurrent | Two create | One or both |
| CON-017 | Revert concurrent | Two revert | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Log usage concurrent | Two log | Both |
| CON-020 | Update concurrent | Two update | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Version sequential | Concurrent create | Sequential |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate template | Validation | Valid template | Pass |
| UNT-003 | Validate variables | Validation | Valid vars | Pass |
| UNT-004 | Validate category | Validation | Valid category | Pass |
| UNT-005 | Validate version | Validation | Valid version | Pass |
| UNT-006 | Format template | Formatting | Template | Formatted |
| UNT-007 | Format variable | Formatting | Variable | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Variable replace | Calculation | Vars | Replaced |
| UNT-013 | Syntax parse | Calculation | Template | Parsed |
| UNT-014 | Template allows replace | Status logic | Template | true |
| UNT-015 | Version allows revert | Status logic | Version | true |
| UNT-016 | Category allows filter | Status logic | Category | true |
| UNT-017 | Name check | Status logic | Name | Valid |
| UNT-018 | Variable format check | Status logic | Var | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Single update | Update | <200ms | P1 |
| PRF-004 | Replace variables | ReplaceVariables | <50ms | P0 |
| PRF-005 | Validate syntax | ValidateSyntax | <100ms | P0 |
| PRF-006 | Get version history | GetVersionHistory | <200ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Get usage stats | GetUsageStats | <500ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel | <2s total | P1 |
| PRF-011 | Concurrent 5 updates | 5 parallel | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 update | <5s total | P2 |
| PRF-013 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-014 | Memory replace | ReplaceVariables | <20MB | P2 |
| PRF-015 | Memory version history | GetVersionHistory | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 10 RPS create | 10 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 10 RPS mixed | 10 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS create | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS replace | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress create | Many creates | Until limit | Holds |
| LDT-008 | Stress memory | Large templates | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
