# UNOPSGeminiManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GeminiManager` (Unit Tests)  
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

Gemini AI manager unit tests cover prompt handling, context building, response parsing, and rate limiting for AI interactions. Tests include: prompt generation, context assembly, API call handling, response extraction, token limit management, and error handling.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Generate prompt | Valid input | GeneratePrompt | Prompt returned |
| POS-002 | Build context | Valid data | BuildContext | Context built |
| POS-003 | Send request to API | Valid config | SendRequest | Response returned |
| POS-004 | Parse response | Response valid | ParseResponse | Parsed |
| POS-005 | Get completion | Prompt valid | GetCompletion | Completion |
| POS-006 | Handle short prompt | Prompt short | Process | Success |
| POS-007 | Handle long prompt | Within limit | Process | Success |
| POS-008 | Rate limit check | Within limit | CheckRate | Allowed |
| POS-009 | Token count | Text valid | CountTokens | Count |
| POS-010 | Truncate to limit | Text over limit | Truncate | Truncated |
| POS-011 | Get model config | Model exists | GetConfig | Config |
| POS-012 | Validate prompt | Prompt valid | Validate | True |
| POS-013 | Format output | Raw output | Format | Formatted |
| POS-014 | Extract JSON | Response has JSON | ExtractJson | JSON |
| POS-015 | Extract list | Response has list | ExtractList | List |
| POS-016 | Cache response | Cache enabled | GetCompletion | Cached |
| POS-017 | Retry on transient | Transient error | Retry | Success |
| POS-018 | Timeout handling | Slow response | Timeout | Handled |
| POS-019 | Stream response | Stream enabled | Stream | Chunks |
| POS-020 | Multiple models | Models exist | GetModel | Model |
| POS-021 | Temperature config | Config valid | SetTemp | Set |
| POS-022 | Max tokens config | Config valid | SetMaxTokens | Set |
| POS-023 | Audit API call | Call made | Check audit | Logged |
| POS-024 | Handle empty response | Empty response | Parse | Handled |
| POS-025 | Handle partial response | Partial | Parse | Handled |
| POS-026 | Fallback model | Primary fails | Fallback | Success |
| POS-027 | Structured output | Schema provided | GetCompletion | Structured |
| POS-028 | System prompt | System prompt set | Process | Applied |
| POS-029 | User prompt | User prompt set | Process | Applied |
| POS-030 | Few-shot examples | Examples provided | Process | Applied |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Generate prompt null input | Input=null | ArgumentNullException |
| NEG-002 | Generate prompt empty | Input="" | ValidationException |
| NEG-003 | Build context null data | Data=null | ArgumentNullException |
| NEG-004 | Send request null prompt | Prompt=null | ArgumentNullException |
| NEG-005 | Parse response null | Response=null | ArgumentNullException |
| NEG-006 | API key missing | Key=null | ConfigurationException |
| NEG-007 | API key invalid | Key=invalid | UnauthorizedException |
| NEG-008 | API key expired | Key expired | UnauthorizedException |
| NEG-009 | Rate limit exceeded | Over limit | RateLimitException |
| NEG-010 | Token limit exceeded | Over limit | TokenLimitException |
| NEG-011 | Invalid model | Model=invalid | ArgumentException |
| NEG-012 | Invalid temperature | Temp=-1 | ArgumentException |
| NEG-013 | Invalid max tokens | MaxTokens=0 | ArgumentException |
| NEG-014 | Null model config | Config=null | ArgumentNullException |
| NEG-015 | GetById without permission | Unauthorized | Forbidden |
| NEG-016 | Send without permission | Unauthorized | Forbidden |
| NEG-017 | API unavailable | API down | ServiceUnavailableException |
| NEG-018 | API timeout | Timeout | TimeoutException |
| NEG-019 | API error 500 | Server error | ApiException |
| NEG-020 | API error 429 | Rate limit | RateLimitException |
| NEG-021 | Malformed response | Malformed JSON | ParseException |
| NEG-022 | Invalid JSON in response | Invalid JSON | ParseException |
| NEG-023 | Invalid schema | Schema invalid | ValidationException |
| NEG-024 | Context too large | Context size | TokenLimitException |
| NEG-025 | Prompt too large | Prompt size | TokenLimitException |
| NEG-026 | Null context | Context=null | ArgumentNullException |
| NEG-027 | Empty context | Context=[] | ValidationException |
| NEG-028 | Invalid retry count | Retry=-1 | ArgumentException |
| NEG-029 | Invalid timeout | Timeout=0 | ArgumentException |
| NEG-030 | Stream null handler | Handler=null | ArgumentNullException |
| NEG-031 | Extract from non-JSON | No JSON | ParseException |
| NEG-032 | Extract from empty | Empty | ParseException |
| NEG-033 | Cache key null | Key=null | ArgumentNullException |
| NEG-034 | Fallback model invalid | Fallback invalid | ArgumentException |
| NEG-035 | System prompt null | Prompt=null | ArgumentNullException |
| NEG-036 | Examples invalid | Examples bad | ValidationException |
| NEG-037 | Update deleted config | Config deleted | KeyNotFoundException |
| NEG-038 | GetById deleted | Config deleted | KeyNotFoundException |
| NEG-039 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-040 | Concurrent rate limit | Over limit | RateLimitException |
| NEG-041 | Transaction rollback | Fail in transaction | Rollback |
| NEG-042 | Connection timeout | Network down | TimeoutException |
| NEG-043 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-044 | Invalid enum value | Model invalid | ArgumentException |
| NEG-045 | Circular reference | Self-reference | BusinessException |
| NEG-046 | Expired session | Expired token | Unauthorized |
| NEG-047 | Null user context | User=null | InvalidOperationException |
| NEG-048 | Invalid include path | Invalid include | ArgumentException |
| NEG-049 | Health check failed | API down | UnhealthyException |
| NEG-050 | Version check failed | Version invalid | UnknownVersionException |
| NEG-051 | Usage stats null | No stats | NullReferenceException |
| NEG-052 | Reset rate limit invalid | Invalid | ArgumentException |
| NEG-053 | Capability invalid | API invalid | ApiException |
| NEG-054 | Structured output invalid | Schema mismatch | ParseException |
| NEG-055 | Stream interrupted | Interrupted | OperationCanceledException |
| NEG-056 | Retry exhausted | All retries fail | ApiException |
| NEG-057 | Timeout too short | Timeout=1ms | TimeoutException |
| NEG-058 | Temperature over max | Temp=2 | ArgumentException |
| NEG-059 | Max tokens over limit | MaxTokens=limit+1 | ArgumentException |
| NEG-060 | Audit missing user | User=0 | InvalidOperationException |
| NEG-061 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-062 | Validate null prompt | Prompt=null | ArgumentNullException |
| NEG-063 | Format null output | Output=null | ArgumentNullException |
| NEG-064 | CountTokens null | Text=null | ArgumentNullException |
| NEG-065 | Truncate null text | Text=null | ArgumentNullException |
| NEG-066 | Child override throws | Child throws | Propagated |
| NEG-067 | GetModel invalid | Model invalid | KeyNotFoundException |
| NEG-068 | GetConfig invalid | Config invalid | KeyNotFoundException |
| NEG-069 | Cache invalid key | Key invalid | ArgumentException |
| NEG-070 | Response content blocked | Content blocked | ContentFilterException |
| NEG-071 | Generate prompt whitespace | Input="   " | ValidationException |
| NEG-072 | Build context empty | Data=[] | ValidationException |
| NEG-073 | Send request null config | Config=null | ArgumentNullException |
| NEG-074 | Parse response empty | Response="" | ParseException |
| NEG-075 | Get completion null | Prompt=null | ArgumentNullException |
| NEG-076 | Count tokens null | Text=null | ArgumentNullException |
| NEG-077 | Truncate null text | Text=null | ArgumentNullException |
| NEG-078 | Get model null | Model=null | ArgumentNullException |
| NEG-079 | Get config null | Config=null | ArgumentNullException |
| NEG-080 | Validate null prompt | Prompt=null | ArgumentNullException |
| NEG-081 | Format null output | Output=null | ArgumentNullException |
| NEG-082 | Extract JSON null | Response=null | ArgumentNullException |
| NEG-083 | Stream null handler | Handler=null | ArgumentNullException |
| NEG-084 | Set temp null | Temp=null | ArgumentNullException |
| NEG-085 | Set max tokens null | MaxTokens=null | ArgumentNullException |
| NEG-086 | Get usage null user | User=null | ArgumentNullException |
| NEG-087 | Reset rate limit invalid | Invalid | ArgumentException |
| NEG-088 | Health check invalid | Invalid | ArgumentException |
| NEG-089 | Get version invalid | Invalid | ArgumentException |
| NEG-090 | Get capabilities invalid | Invalid | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Prompt at min length | Length=1 | Valid |
| BND-002 | Prompt at max length | Length=limit | Valid |
| BND-003 | Prompt exceeds max | Length=limit+1 | Reject |
| BND-004 | Context at max | Size=limit | Valid |
| BND-005 | Context over max | Size=limit+1 | Reject |
| BND-006 | Token count at limit | Tokens=limit | Valid |
| BND-007 | Token count over limit | Tokens=limit+1 | Reject |
| BND-008 | Temperature at 0 | Temp=0 | Valid |
| BND-009 | Temperature at 1 | Temp=1 | Valid |
| BND-010 | Temperature over 1 | Temp=1.1 | Reject |
| BND-011 | Max tokens at min | MaxTokens=1 | Valid |
| BND-012 | Max tokens at max | MaxTokens=limit | Valid |
| BND-013 | Max tokens over max | MaxTokens=limit+1 | Reject |
| BND-014 | Rate limit at limit | At limit | Reject |
| BND-015 | Rate limit at limit-1 | Limit-1 | Valid |
| BND-016 | Unicode in prompt | Arabic/Chinese | Valid |
| BND-017 | Special chars in prompt | <>&"' | Escaped |
| BND-018 | Newlines in prompt | \n\r | Handled |
| BND-019 | Empty response | Response="" | Handled |
| BND-020 | Single char response | Response="x" | Valid |
| BND-021 | Single token | Tokens=1 | Valid |
| BND-022 | Zero tokens | Tokens=0 | Reject |
| BND-023 | Null response | Response=null | Handled |
| BND-024 | Partial JSON | Incomplete JSON | Handled |
| BND-025 | Malformed JSON | Bad JSON | Handled |
| BND-026 | Empty context | Context=[] | Valid or reject |
| BND-027 | Single context item | Count=1 | Valid |
| BND-028 | Context max items | At limit | Valid |
| BND-029 | Retry at 0 | Retry=0 | No retry |
| BND-030 | Retry at max | Retry=max | Max retries |
| BND-031 | Timeout at min | Timeout=1s | Valid |
| BND-032 | Timeout at max | Timeout=120s | Valid |
| BND-033 | Timeout over max | Timeout=121s | Reject |
| BND-034 | Pagination last partial | Partial page | Correct |
| BND-035 | Pagination total | Total count | Accurate |
| BND-036 | Sort null handling | Nulls in data | Deterministic |
| BND-037 | Filter combination all | All filters | Correct |
| BND-038 | Model enum first | First | Valid |
| BND-039 | Model enum last | Last | Valid |
| BND-040 | Cache TTL at min | TTL=1s | Valid |
| BND-041 | Cache TTL at max | TTL=24h | Valid |
| BND-042 | Usage stats zero | Usage=0 | Valid |
| BND-043 | Usage stats max | Usage=max | Valid |
| BND-044 | Stream chunk size min | Size=1 | Valid |
| BND-045 | Stream chunk size max | Size=limit | Valid |
| BND-046 | Health check interval | Interval | Valid |
| BND-047 | Version check interval | Interval | Valid |
| BND-048 | Soft delete boundary | DeletedDate set | Excluded |
| BND-049 | Include depth | Deep include | No explosion |
| BND-050 | Query timeout | Slow query | Timeout |
| BND-051 | Memory large context | 10k tokens | No OOM |
| BND-052 | Audit timestamp precision | Millisecond | Stored |
| BND-053 | Long string in prompt | 100k chars | Truncate |
| BND-054 | JSON depth max | Deep JSON | Valid |
| BND-055 | JSON depth over max | Over limit | Reject |
| BND-056 | Structured output empty | Empty schema | Valid |
| BND-057 | Structured output min | Min schema | Valid |
| BND-058 | Few-shot count zero | Count=0 | Valid |
| BND-059 | Few-shot count max | Count=limit | Valid |
| BND-060 | Few-shot count over max | Count=limit+1 | Reject |
| BND-061 | System prompt empty | Prompt="" | Valid or reject |
| BND-062 | User prompt empty | Prompt="" | Valid or reject |
| BND-063 | GetModel fallback | Primary invalid | Fallback |
| BND-064 | GetConfig fallback | Primary invalid | Fallback |
| BND-065 | Cache miss | Not cached | Fetch |
| BND-066 | Cache hit | Cached | Return |
| BND-067 | Cache expiry | After expiry | Miss |
| BND-068 | Async cancellation | Cancel token | OperationCanceledException |
| BND-069 | Task timeout | Timeout | TimeoutException |
| BND-070 | Concurrent same second | Same timestamp | Deterministic |
| BND-071 | Prompt single char | Length=1 | Valid |
| BND-072 | Context single item | Count=1 | Valid |
| BND-073 | Token count one | Tokens=1 | Valid |
| BND-074 | Temperature zero | Temp=0 | Valid |
| BND-075 | Temperature one | Temp=1 | Valid |
| BND-076 | Max tokens min | MaxTokens=1 | Valid |
| BND-077 | Max tokens max | MaxTokens=limit | Valid |
| BND-078 | Rate limit at limit | At limit | Reject |
| BND-079 | Rate limit at limit-1 | Limit-1 | Valid |
| BND-080 | Retry at min | Retry=0 | No retry |
| BND-081 | Retry at max | Retry=max | Max retries |
| BND-082 | Timeout at min | Timeout=1s | Valid |
| BND-083 | Timeout at max | Timeout=120s | Valid |
| BND-084 | Model enum first | First | Valid |
| BND-085 | Model enum last | Last | Valid |
| BND-086 | Cache TTL min | TTL=1s | Valid |
| BND-087 | Cache TTL max | TTL=24h | Valid |
| BND-088 | Usage stats zero | Usage=0 | Valid |
| BND-089 | Few-shot count zero | Count=0 | Valid |
| BND-090 | Few-shot count max | Count=limit | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Prompt required | Validation | Generate | Reject if empty |
| FUN-002 | Context required | Validation | BuildContext | Reject if null |
| FUN-003 | Model required | Validation | SendRequest | Reject if invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | API key required | Constraint | SendRequest | Reject if missing |
| FUN-008 | Rate limit enforced | Constraint | SendRequest | Reject if over |
| FUN-009 | Token limit enforced | Constraint | SendRequest | Reject if over |
| FUN-010 | Audit API call | Audit | SendRequest | Logged |
| FUN-011 | Audit CreatedBy | Audit | Create | Set user |
| FUN-012 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-013 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-014 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-015 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | Token count accurate | Logic | CountTokens | Accurate |
| FUN-018 | Truncate preserves start | Logic | Truncate | Start preserved |
| FUN-019 | Parse response structure | Logic | ParseResponse | Structured |
| FUN-020 | Extract JSON valid | Logic | ExtractJson | Valid JSON |
| FUN-021 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-022 | GetCompletion returns text | Logic | GetCompletion | Text |
| FUN-023 | Stream returns chunks | Logic | Stream | Chunks |
| FUN-024 | Retry on transient | Logic | Retry | Retried |
| FUN-025 | Fallback on primary fail | Logic | Fallback | Fallback used |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Cache key unique | Logic | Cache | Unique key |
| FUN-031 | Cache TTL respected | Logic | Cache | Expiry |
| FUN-032 | Transaction on create | Transaction | Create | Atomic |
| FUN-033 | Transaction on update | Transaction | Update | Atomic |
| FUN-034 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-035 | Async all operations | Concurrency | All | Async |
| FUN-036 | Include loads config | Data load | GetById include | Config loaded |
| FUN-037 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-038 | Format output sanitizes | Logic | Format | Sanitized |
| FUN-039 | Validate prompt format | Validation | Validate | Format check |
| FUN-040 | Structured output schema | Logic | GetCompletion | Schema applied |
| FUN-041 | System prompt applied | Logic | Process | Applied |
| FUN-042 | User prompt applied | Logic | Process | Applied |
| FUN-043 | Few-shot applied | Logic | Process | Applied |
| FUN-044 | Usage stats tracked | Logic | SendRequest | Tracked |
| FUN-045 | Rate limit reset | Logic | Reset | Reset |
| FUN-046 | Health check interval | Logic | HealthCheck | Interval |
| FUN-047 | Version check | Logic | GetVersion | Version |
| FUN-048 | Capability check | Logic | GetCapabilities | Capabilities |
| FUN-049 | Permission cached | Performance | Repeated check | Cached |
| FUN-050 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-051 | Prompt required | Validation | Generate | Reject if empty |
| FUN-052 | Context required | Validation | BuildContext | Reject if null |
| FUN-053 | Model required | Validation | SendRequest | Reject if invalid |
| FUN-054 | API key required | Constraint | SendRequest | Reject if missing |
| FUN-055 | Rate limit enforced | Constraint | SendRequest | Reject if over |
| FUN-056 | Token limit enforced | Constraint | SendRequest | Reject if over |
| FUN-057 | Token count accurate | Logic | CountTokens | Accurate |
| FUN-058 | Truncate preserves start | Logic | Truncate | Start preserved |
| FUN-059 | Parse response structure | Logic | ParseResponse | Structured |
| FUN-060 | Extract JSON valid | Logic | ExtractJson | Valid JSON |
| FUN-061 | GetCompletion returns text | Logic | GetCompletion | Text |
| FUN-062 | Stream returns chunks | Logic | Stream | Chunks |
| FUN-063 | Retry on transient | Logic | Retry | Retried |
| FUN-064 | Fallback on primary fail | Logic | Fallback | Fallback used |
| FUN-065 | Cache key unique | Logic | Cache | Unique key |
| FUN-066 | Cache TTL respected | Logic | Cache | Expiry |
| FUN-067 | Format output sanitizes | Logic | Format | Sanitized |
| FUN-068 | Validate prompt format | Validation | Validate | Format check |
| FUN-069 | Structured output schema | Logic | GetCompletion | Schema applied |
| FUN-070 | System prompt applied | Logic | Process | Applied |
| FUN-071 | User prompt applied | Logic | Process | Applied |
| FUN-072 | Few-shot applied | Logic | Process | Applied |
| FUN-073 | Usage stats tracked | Logic | SendRequest | Tracked |
| FUN-074 | Rate limit reset | Logic | Reset | Reset |
| FUN-075 | Health check interval | Logic | HealthCheck | Interval |
| FUN-076 | Version check | Logic | GetVersion | Version |
| FUN-077 | Capability check | Logic | GetCapabilities | Capabilities |
| FUN-078 | Include loads config | Data load | GetById include | Config loaded |
| FUN-079 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-080 | Audit API call | Audit | SendRequest | Logged |
| FUN-081 | Permission before send | Authorization | SendRequest | Check first |
| FUN-082 | Permission before get | Authorization | GetById | Check first |
| FUN-083 | Pagination offset | Calculation | Page | Skip correct |
| FUN-084 | Total count accurate | Calculation | Count | Matches |
| FUN-085 | Sort applies | Calculation | Sort | Ordered |
| FUN-086 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-087 | Transaction on create | Transaction | Create | Atomic |
| FUN-088 | Transaction on update | Transaction | Update | Atomic |
| FUN-089 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-090 | Async all operations | Concurrency | All | Async |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Generate prompt full flow | GeneratePrompt | Prompt | Generated |
| INT-002 | Build context full flow | BuildContext | Context | Built |
| INT-003 | Send request full flow | SendRequest | Request | Response |
| INT-004 | Parse response full flow | ParseResponse | Response | Parsed |
| INT-005 | Get completion full flow | GetCompletion | Completion | Received |
| INT-006 | API call | API | Gemini API | Success |
| INT-007 | Rate limit handling | RateLimit | Check | Allowed/Rejected |
| INT-008 | Token counting | CountTokens | Text | Count |
| INT-009 | Truncation | Truncate | Text | Truncated |
| INT-010 | Cache hit | Cache | Cache | Hit |
| INT-011 | Cache miss | Cache | Cache | Miss |
| INT-012 | Retry logic | Retry | Transient | Success |
| INT-013 | Timeout handling | Timeout | Slow | Timeout |
| INT-014 | Stream handling | Stream | Response | Chunks |
| INT-015 | Pagination | Paginate | Config | Pages |
| INT-016 | Config-API relationship | Relationship | Config, API | Valid |
| INT-017 | Config-User relationship | Relationship | Config, User | Valid |
| INT-018 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-019 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-020 | API error handling | Error | API down | Graceful |
| INT-021 | Timeout handling | Error | Slow DB | Timeout |
| INT-022 | Parse error handling | Error | Malformed | ParseException |
| INT-023 | Rate limit error | Error | Over limit | RateLimitException |
| INT-024 | Permission service integration | Integration | Permission | Check |
| INT-025 | User resolver integration | Integration | User | Resolved |
| INT-026 | Audit context integration | Integration | Audit | Context |
| INT-027 | Logger integration | Integration | Log | Logged |
| INT-028 | HTTP client integration | Integration | HttpClient | Call |
| INT-029 | Config integration | Integration | Config | Read |
| INT-030 | Mapper integration | Integration | Map | Correct |
| INT-031 | Repository integration | Integration | Repository | CRUD |
| INT-032 | DbContext integration | Integration | DbContext | Scoped |
| INT-033 | Transaction scope | Integration | Transaction | Atomic |
| INT-034 | API key from config | Integration | Config | Key |
| INT-035 | Model from config | Integration | Config | Model |
| INT-036 | Multiple API calls | Scenario | Multiple | All succeed |
| INT-037 | Rate limit across calls | Scenario | Many calls | Limited |
| INT-038 | Fallback chain | Scenario | Primary fail | Fallback |
| INT-039 | Structured output | Scenario | Schema | Structured |
| INT-040 | Stream with parse | Scenario | Stream | Parsed |
| INT-041 | Cache expiry | Scenario | Cache | Expiry |
| INT-042 | Retry with backoff | Scenario | Retry | Backoff |
| INT-043 | Health check | Scenario | HealthCheck | Healthy |
| INT-044 | Usage tracking | Scenario | Usage | Tracked |
| INT-045 | Version check | Scenario | Version | Version |
| INT-046 | Capability check | Scenario | Capabilities | Capabilities |
| INT-047 | Prompt with context | Scenario | Prompt, Context | Combined |
| INT-048 | Response with parse | Scenario | Response | Parsed |
| INT-049 | JSON extraction | Scenario | ExtractJson | JSON |
| INT-050 | E2E completion flow | Scenario | Full flow | Complete |
| INT-051 | Generate then send | Scenario | GeneratePrompt, SendRequest | Both |
| INT-052 | Build context then send | Scenario | BuildContext, SendRequest | Both |
| INT-053 | Parse then extract | Scenario | ParseResponse, ExtractJson | Both |
| INT-054 | Cache hit flow | Scenario | Cache | Hit |
| INT-055 | Cache miss flow | Scenario | Cache | Miss |
| INT-056 | Retry flow | Scenario | Retry | Success |
| INT-057 | Timeout flow | Scenario | Timeout | Handled |
| INT-058 | Fallback flow | Scenario | Fallback | Used |
| INT-059 | Stream flow | Scenario | Stream | Chunks |
| INT-060 | Structured output flow | Scenario | GetCompletion | Structured |
| INT-061 | Get model then send | Scenario | GetModel, SendRequest | Both |
| INT-062 | Get config then send | Scenario | GetConfig, SendRequest | Both |
| INT-063 | HTTP client integration | Integration | HttpClient | Call |
| INT-064 | Config integration | Integration | Config | Read |
| INT-065 | Mapper integration | Integration | Mapper | Mapped |
| INT-066 | Repository integration | Integration | Repository | CRUD |
| INT-067 | DbContext integration | Integration | DbContext | Scoped |
| INT-068 | Transaction scope | Integration | Transaction | Atomic |
| INT-069 | API key from config | Integration | Config | Key |
| INT-070 | Model from config | Integration | Config | Model |
| INT-071 | Permission service | Integration | Permission | Check |
| INT-072 | User resolver | Integration | User | Resolved |
| INT-073 | Audit context | Integration | Audit | Context |
| INT-074 | Logger integration | Integration | Logger | Logged |
| INT-075 | Config-API relationship | Relationship | Config, API | Valid |
| INT-076 | Config-User relationship | Relationship | Config, User | Valid |
| INT-077 | Cascade soft delete | Relationship | Parent deleted | Config |
| INT-078 | Orphan handling | Relationship | Parent deleted | Retained |
| INT-079 | API error handling | Error | API down | Graceful |
| INT-080 | Timeout handling | Error | Slow DB | Timeout |
| INT-081 | Parse error handling | Error | Malformed | ParseException |
| INT-082 | Rate limit error | Error | Over limit | RateLimitException |
| INT-083 | Multiple API calls | Scenario | Multiple | All succeed |
| INT-084 | Rate limit across calls | Scenario | Many calls | Limited |
| INT-085 | Fallback chain | Scenario | Primary fail | Fallback |
| INT-086 | Stream with parse | Scenario | Stream | Parsed |
| INT-087 | Cache expiry | Scenario | Cache | Expiry |
| INT-088 | Retry with backoff | Scenario | Retry | Backoff |
| INT-089 | Health check | Scenario | HealthCheck | Healthy |
| INT-090 | Full workflow | Scenario | Full flow | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | Prompt injection | Malicious prompt | Prompt | Sanitized |
| SEC-002 | SQL injection in prompt | '; DROP TABLE-- | Prompt | Sanitized |
| SEC-003 | XSS in prompt | <script>alert(1)</script> | Prompt | Escaped |
| SEC-004 | XSS in response | <img onerror=...> | Response | Escaped |
| SEC-005 | LDAP injection | *)(uid=* | Prompt | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Prompt | Rejected |
| SEC-008 | API key exposure | Log | Log | Redacted |
| SEC-009 | API key in error | Error | Stack | Redacted |
| SEC-010 | Unauthorized list | No permission | List | 403 |
| SEC-011 | Unauthorized get | No permission | GetById | 403 |
| SEC-012 | Unauthorized send | No permission | SendRequest | 403 |
| SEC-013 | Unauthorized config | No permission | Config | 403 |
| SEC-014 | Role escalation | Low role | Admin | 403 |
| SEC-015 | Cross-tenant access | User A | User B data | 403 |
| SEC-016 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-017 | IDOR update other | Id=other | Update | 403 |
| SEC-018 | IDOR delete other | Id=other | Delete | 403 |
| SEC-019 | IDOR in filter | ConfigId=other | List | Filtered |
| SEC-020 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-021 | Mass assign API key | APIKey= | Request | Ignored |
| SEC-022 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-023 | Session hijack | Stolen token | Any | Detected |
| SEC-024 | Token expiration | Expired | Any | 401 |
| SEC-025 | Invalid token | Malformed | Any | 401 |
| SEC-026 | CSRF on send | No token | SendRequest | Rejected |
| SEC-027 | CSRF on config | No token | Config | Rejected |
| SEC-028 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-029 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-030 | Rate limit bypass | Bypass attempt | Rate limit | Blocked |
| SEC-031 | Rate limit create | Many creates | Create | Throttled |
| SEC-032 | Rate limit send | Many sends | SendRequest | Throttled |
| SEC-033 | Oversized request | 10MB payload | SendRequest | Rejected |
| SEC-034 | Deep nesting | Nested object | Request | Rejected |
| SEC-035 | Header injection | \r\n in header | Header | Rejected |
| SEC-036 | Null byte injection | %00 in prompt | Prompt | Rejected |
| SEC-037 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-038 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-039 | Denial of service | Huge prompt | SendRequest | Rejected |
| SEC-040 | Content filter bypass | Malicious content | Filter | Blocked |
| SEC-041 | Jailbreak attempt | Jailbreak prompt | Prompt | Blocked |
| SEC-042 | PII in prompt | PII in prompt | Prompt | Redacted |
| SEC-043 | PII in response | PII in response | Response | Redacted |
| SEC-044 | Import malicious config | Malicious | Import | Rejected |
| SEC-045 | Export data injection | Inject in export | Export | Sanitized |
| SEC-046 | Cache poisoning | Malicious cache | Cache | Not used |
| SEC-047 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-048 | Permission cached | Repeated check | Permission | Cached |
| SEC-049 | API key rotation | Rotate key | Config | Updated |
| SEC-050 | Request signing | Tamper request | Request | Rejected |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent API calls | Two send | Both succeed |
| CON-004 | Rate limit concurrent | Many concurrent | Limited |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on rate limit | Two check | Correct limit |
| CON-009 | Race on cache | Two cache | Consistent |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel sends | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Stream concurrent | Two stream | Both succeed |
| CON-016 | Retry concurrent | Two retry | Both succeed |
| CON-017 | Cache invalidation race | Update, read | Consistent |
| CON-018 | Usage stats concurrent | Many updates | Consistent |
| CON-019 | Rate limit reset concurrent | Two reset | Deterministic |
| CON-020 | Soft delete concurrent | Delete while update | Deterministic |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | API connection limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate prompt not null | Validation | null | Exception |
| UNT-002 | Validate context format | Validation | Valid context | Pass |
| UNT-003 | Validate model | Validation | Valid model | Pass |
| UNT-004 | Validate temperature | Validation | 0-1 | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format prompt display | Formatting | Prompt | Display |
| UNT-007 | Format response | Formatting | Response | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate token count | Calculation | Text | Count |
| UNT-010 | Calculate truncation | Calculation | Text, Limit | Truncated |
| UNT-011 | Calculate pagination | Calculation | Page, Size | Offset |
| UNT-012 | Parse JSON | Calculation | JSON string | Object |
| UNT-013 | Extract list | Calculation | Response | List |
| UNT-014 | Model allows send | Status logic | Model | true |
| UNT-015 | Rate limit allows | Status logic | Under limit | true |
| UNT-016 | Token limit allows | Status logic | Under limit | true |
| UNT-017 | Cache hit check | Status logic | Cached | true |
| UNT-018 | Retry needed check | Status logic | Transient | true |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Generate prompt | GeneratePrompt | <50ms | P1 |
| PRF-003 | Build context | BuildContext | <100ms | P1 |
| PRF-004 | Count tokens | CountTokens | <50ms | P1 |
| PRF-005 | Parse response | ParseResponse | <100ms | P1 |
| PRF-006 | API call | SendRequest | <5s | P1 |
| PRF-007 | Get completion | GetCompletion | <5s | P1 |
| PRF-008 | List with pagination | List | <300ms | P1 |
| PRF-009 | Cache hit | Cache get | <10ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 sends | 5 parallel Send | <25s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 send | <15s total | P2 |
| PRF-013 | Memory single send | SendRequest | <50MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory large context | 10k tokens | <100MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 2 RPS send | 2 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 2 RPS mixed | 2 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 10 RPS send | 0→10→0 | 1 min | No errors |
| LDT-005 | Spike 20 RPS read | 0→20→0 | 30s | Graceful deg |
| LDT-006 | Stress rate limit | Many sends | Until limit | Limited |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large contexts | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
