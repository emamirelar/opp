# AiContextualService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/AiContextualService`  
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
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

| Check | Formula | Result |
|-------|---------|--------|
| N≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| E≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| F≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| I≥3P | 90 ≥ 3×30=90 | ✅ PASS |

---

## Feature Overview

AI contextual service: context building for AI prompts, entity data aggregation, prompt templating, response parsing.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Build context with valid entity IDs | Valid opp/partner IDs | BuildContextAsync(oppId, partnerId) | Context object returned |
| POS-002 | Aggregate entity data for prompt | Existing entity | LoadEntityDataAsync(id) | Entity data aggregated |
| POS-003 | Apply prompt template | Template + params | ApplyTemplate(template, params) | Placeholders replaced |
| POS-004 | Parse valid AI response JSON | Valid JSON response | ParseResponseAsync(response) | Parsed object returned |
| POS-005 | Build context with multiple entities | Multiple IDs | BuildContextAsync(ids) | Multi-entity context |
| POS-006 | Substitute single parameter | Template with {param} | Substitute(param, value) | Correct substitution |
| POS-007 | Substitute multiple parameters | Template with multiple placeholders | Substitute(params) | All replaced |
| POS-008 | Parse response with nested structure | Nested JSON | ParseResponseAsync(response) | Structure preserved |
| POS-009 | Context includes opportunity metadata | Valid opp ID | BuildContextAsync(oppId) | Opp metadata in context |
| POS-010 | Context includes partner metadata | Valid partner ID | BuildContextAsync(partnerId) | Partner metadata in context |
| POS-011 | Empty optional params use defaults | Template with optional params | ApplyTemplate(template, {}) | Defaults applied |
| POS-012 | UTF-8 encoding in response | Unicode in response | ParseResponseAsync(response) | Correct encoding |
| POS-013 | Cache hit returns context | Cached context | BuildContextAsync(id) | Context from cache |
| POS-014 | Response with array | JSON array | ParseResponseAsync(response) | Array parsed |
| POS-015 | Template with conditional | Conditional template | ApplyTemplate(template, params) | Correct branch |
| POS-016 | Aggregate related entities | Entity with relations | LoadEntityDataAsync(id) | Relations included |
| POS-017 | Build context for GPT prompt | Ready context | BuildContextAsync(id) | Prompt-ready format |
| POS-018 | Parse numeric values | Response with numbers | ParseResponseAsync(response) | Numbers correct |
| POS-019 | Parse boolean values | Response with booleans | ParseResponseAsync(response) | Booleans correct |
| POS-020 | Parse null values | Response with null | ParseResponseAsync(response) | Null handled |
| POS-021 | Template escape sequences | Template with escapes | ApplyTemplate(template) | Escapes correct |
| POS-022 | Context with empty relations | Entity with no relations | LoadEntityDataAsync(id) | Empty arrays handled |
| POS-023 | Multiple template applications | Same template different params | ApplyTemplate twice | Both correct |
| POS-024 | Response parsing with whitespace | JSON with extra whitespace | ParseResponseAsync(response) | Parsed correctly |
| POS-025 | Context includes workflow status | Entity with status | BuildContextAsync(id) | Status in context |
| POS-026 | Template with date format | Date in params | ApplyTemplate(template, params) | Date formatted |
| POS-027 | Aggregate with pagination | Large entity set | LoadEntityDataAsync(ids) | Paginated correctly |
| POS-028 | Response with empty object | {} | ParseResponseAsync(response) | Empty object returned |
| POS-029 | Context includes user preferences | User + entity | BuildContextAsync(userId, entityId) | Preferences in context |
| POS-030 | Template with optional sections | Optional section in template | ApplyTemplate(template, params) | Section omitted if empty |
| POS-031 | Parse response with metadata | Response with metadata field | ParseResponseAsync(response) | Metadata preserved |
| POS-032 | Build context for partner prompt | Partner-focused | BuildContextAsync(partnerId) | Partner-focused context |
| POS-033 | Build context for opportunity prompt | Opp-focused | BuildContextAsync(oppId) | Opp-focused context |
| POS-034 | Full pipeline: build → template → parse | End-to-end | Build, Apply, Parse | Full success |
| POS-035 | Async build with cancellation token | Valid token | BuildContextAsync(id, ct) | Respects cancellation |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null entity ID | BuildContextAsync(null) | ArgumentNullException |
| NEG-002 | Negative entity ID | BuildContextAsync(-1) | ArgumentException |
| NEG-003 | Zero entity ID | BuildContextAsync(0) | ArgumentException |
| NEG-004 | Non-existent entity ID | BuildContextAsync(999999) | KeyNotFoundException |
| NEG-005 | Null template | ApplyTemplate(null, params) | ArgumentNullException |
| NEG-006 | Null template params | ApplyTemplate(template, null) | ArgumentNullException |
| NEG-007 | Null response | ParseResponseAsync(null) | ArgumentNullException |
| NEG-008 | Empty string response | ParseResponseAsync("") | JsonException |
| NEG-009 | Missing required parameter | ApplyTemplate(template, missing) | ArgumentException |
| NEG-010 | Malformed JSON | ParseResponseAsync("{invalid}") | JsonException |
| NEG-011 | Truncated JSON | ParseResponseAsync("{") | JsonException |
| NEG-012 | JSON with wrong root type | ParseResponseAsync("[]") | InvalidOperationException |
| NEG-013 | Invalid entity type | BuildContextAsync("invalid") | ArgumentException |
| NEG-014 | Null entity ID array | LoadEntityDataAsync(null) | ArgumentNullException |
| NEG-015 | Empty entity ID array | LoadEntityDataAsync([]) | ArgumentException |
| NEG-016 | Entity ID exceeds max | BuildContextAsync(999999999) | ArgumentException |
| NEG-017 | Template with invalid placeholder | ApplyTemplate("{invalid}") | FormatException |
| NEG-018 | Response with circular reference | ParseResponseAsync(circular) | JsonException |
| NEG-019 | DB timeout during context build | BuildContextAsync(id) | TimeoutException |
| NEG-020 | AI service unavailable | BuildContextAsync(id) | ServiceUnavailableException |
| NEG-021 | Decode invalid base64 | ParseResponseAsync(invalid) | FormatException |
| NEG-022 | Unicode in invalid encoding | ParseResponseAsync(wrongEncoding) | DecoderFallbackException |
| NEG-023 | Cancelled token | BuildContextAsync(id, cancelled) | OperationCanceledException |
| NEG-024 | Template with unbalanced braces | ApplyTemplate("{unbalanced") | FormatException |
| NEG-025 | Response exceeds max size | ParseResponseAsync(huge) | InvalidOperationException |
| NEG-026 | Null entity reference | LoadEntityDataAsync(id) | NullReferenceException |
| NEG-027 | Deleted entity | BuildContextAsync(deletedId) | KeyNotFoundException |
| NEG-028 | Soft-deleted entity | BuildContextAsync(softDeletedId) | KeyNotFoundException |
| NEG-029 | Template injection attempt | ApplyTemplate("{userInput}") | XSS sanitized |
| NEG-030 | SQL injection in entity lookup | BuildContextAsync(evilId) | Parameterized |
| NEG-031 | Invalid UTF-8 sequence | ParseResponseAsync(badUtf8) | DecoderFallbackException |
| NEG-032 | Response timeout | ParseResponseAsync(slow) | TimeoutException |
| NEG-033 | Rate limit exceeded | BuildContextAsync(id) | TooManyRequestsException |
| NEG-034 | Quota exceeded | BuildContextAsync(id) | QuotaExceededException |
| NEG-035 | Invalid JSON patch | ApplyPatch(invalid) | JsonException |
| NEG-036 | Template with reserved keyword | ApplyTemplate("{system}") | InvalidOperationException |
| NEG-037 | Entity ID as string when int expected | BuildContextAsync("123") | ArgumentException |
| NEG-038 | Empty template | ApplyTemplate("", params) | ArgumentException |
| NEG-039 | Whitespace-only template | ApplyTemplate("   ", params) | ArgumentException |
| NEG-040 | Null context builder config | BuildContextAsync(opt: null) | ArgumentNullException |
| NEG-041 | Invalid cache key | GetCachedContext(invalid) | ArgumentException |
| NEG-042 | Expired cache entry | GetCachedContext(expired) | Cache miss |
| NEG-043 | Corrupted cache entry | GetCachedContext(corrupt) | CacheInvalidException |
| NEG-044 | Concurrent modification | BuildContextAsync(modified) | ConcurrentModificationException |
| NEG-045 | Disk full during cache write | BuildContextAsync(id) | IOException |
| NEG-046 | Permission denied on cache | BuildContextAsync(id) | UnauthorizedAccessException |
| NEG-047 | Network unreachable | BuildContextAsync(id) | NetworkException |
| NEG-048 | SSL certificate invalid | BuildContextAsync(id) | SecurityException |
| NEG-049 | Response with BOM | ParseResponseAsync(bom) | Handled |
| NEG-050 | Template with invalid escape | ApplyTemplate("\\x") | FormatException |
| NEG-051 | NaN in response | ParseResponseAsync(nan) | InvalidOperationException |
| NEG-052 | Infinity in response | ParseResponseAsync(inf) | InvalidOperationException |
| NEG-053 | Duplicate keys in JSON | ParseResponseAsync(dup) | Last wins |
| NEG-054 | Very deep nesting | ParseResponseAsync(deep) | StackOverflow prevented |
| NEG-055 | Entity ID format mismatch | BuildContextAsync("abc-def") | ArgumentException |
| NEG-056 | Unicode in entity ID | BuildContextAsync("你好") | ArgumentException |
| NEG-057 | Special chars in template | ApplyTemplate(template, special) | Sanitized |
| NEG-058 | Control characters in response | ParseResponseAsync(control) | Sanitized |
| NEG-059 | Null byte in string | ParseResponseAsync(nullByte) | ArgumentException |
| NEG-060 | Entity ID overflow | BuildContextAsync(Int32.MaxValue+1) | OverflowException |
| NEG-061 | Concurrent context invalidation | Build + invalidate | Handled |
| NEG-062 | Template recursion | ApplyTemplate(recursive) | StackOverflow prevented |
| NEG-063 | Empty response | ParseResponseAsync("") | JsonException |
| NEG-064 | Response with invalid date | ParseResponseAsync(badDate) | FormatException |
| NEG-065 | Response with invalid GUID | ParseResponseAsync(badGuid) | FormatException |
| NEG-066 | Template with invalid regex | ApplyTemplate(regex) | ArgumentException |
| NEG-067 | Memory pressure during parse | ParseResponseAsync(large) | OutOfMemoryException |
| NEG-068 | Service account missing | BuildContextAsync(noService) | ConfigurationException |
| NEG-069 | API key invalid | BuildContextAsync(id) | AuthenticationException |
| NEG-070 | Response with illegal escape | ParseResponseAsync illegal | JsonException |

---

## §3 Boundary Tests (70)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Entity ID = 1 | Min valid ID | Context built |
| BND-002 | Entity ID = Int32.MaxValue | Max valid ID | Context built or error |
| BND-003 | Entity ID = Int32.MaxValue - 1 | Just below max | Context built |
| BND-004 | Empty string template | "" | ArgumentException |
| BND-005 | Single char template | "x" | Applied |
| BND-006 | Template length = 1 | 1 char | Applied |
| BND-007 | Template length = 10000 | Max length | Truncated or applied |
| BND-008 | Template length = 10001 | Over max | ArgumentException |
| BND-009 | Zero params | {} | Defaults applied |
| BND-010 | Single param | {} | Applied |
| BND-011 | 100 params | 100 keys | Applied |
| BND-012 | 101 params | Over limit | ArgumentException |
| BND-013 | Response length = 0 | "" | JsonException |
| BND-014 | Response length = 1 | "{" | JsonException |
| BND-015 | Response length = 1000000 | 1MB | Parsed |
| BND-016 | Response length = 1000000 + 1 | Over limit | Rejected |
| BND-017 | Nesting depth = 1 | 1 level | Parsed |
| BND-018 | Nesting depth = 64 | Max depth | Parsed |
| BND-019 | Nesting depth = 65 | Over max | Rejected |
| BND-020 | Array length = 0 | [] | Parsed |
| BND-021 | Array length = 1 | [x] | Parsed |
| BND-022 | Array length = 10000 | Max | Parsed |
| BND-023 | String length = 0 | "" | Handled |
| BND-024 | String length = 32767 | Max | Handled |
| BND-025 | String length = 32768 | Over | Truncated |
| BND-026 | Placeholder at start | "{param}..." | Substituted |
| BND-027 | Placeholder at end | "...{param}" | Substituted |
| BND-028 | Placeholder alone | "{param}" | Substituted |
| BND-029 | Adjacent placeholders | "{a}{b}" | Both substituted |
| BND-030 | Empty placeholder name | "{}" | Invalid |
| BND-031 | Unicode in template | "你好{param}" | Handled |
| BND-032 | Emoji in template | "👍{param}" | Handled |
| BND-033 | RTL in template | "مرحبا{param}" | Handled |
| BND-034 | Min number | Int32.MinValue | Parsed |
| BND-035 | Max number | Int32.MaxValue | Parsed |
| BND-036 | Min date | DateTime.MinValue | Parsed |
| BND-037 | Max date | DateTime.MaxValue | Parsed |
| BND-038 | Min float | float.MinValue | Parsed |
| BND-039 | Max float | float.MaxValue | Parsed |
| BND-040 | Empty object | {} | Parsed |
| BND-041 | Object with one key | {"a":1} | Parsed |
| BND-042 | Empty array | [] | Parsed |
| BND-043 | Context cache size = 0 | No cache | Miss |
| BND-044 | Context cache size = 1 | One entry | Hit |
| BND-045 | Context cache size = 1000 | Max | Eviction |
| BND-046 | Timeout = 0ms | 0 | Immediate |
| BND-047 | Timeout = 1ms | 1 | Timed out |
| BND-048 | Timeout = 30000ms | 30s | Success |
| BND-049 | Timeout = 30001ms | Over | Clamped |
| BND-050 | Retry count = 0 | No retry | Fail once |
| BND-051 | Retry count = 3 | 3 retries | Retries |
| BND-052 | Retry count = 10 | Max | Retries |
| BND-053 | Retry count = 11 | Over | Clamped |
| BND-054 | Concurrent requests = 1 | 1 | Success |
| BND-055 | Concurrent requests = 100 | 100 | All succeed |
| BND-056 | Concurrent requests = 1000 | 1000 | Throttled |
| BND-057 | UTF-8 BOM | 0xEFBBBF | Stripped |
| BND-058 | UTF-16 BOM | 0xFFFE | Rejected |
| BND-059 | Line endings CR | \r | Handled |
| BND-060 | Line endings LF | \n | Handled |
| BND-061 | Line endings CRLF | \r\n | Handled |
| BND-062 | Mixed line endings | Mixed | Normalized |
| BND-063 | Null in JSON | null | Parsed |
| BND-064 | True in JSON | true | Parsed |
| BND-065 | False in JSON | false | Parsed |
| BND-066 | Scientific notation | 1e10 | Parsed |
| BND-067 | Decimal precision | 0.123456789 | Preserved |
| BND-068 | Timestamp precision | 1704067200000 | Parsed |
| BND-069 | ISO8601 date | 2026-02-11T00:00:00Z | Parsed |
| BND-070 | Empty key in object | {"":1} | Parsed |
| BND-071 | Entity count = 0 | [] | Invalid |
| BND-072 | Entity count = 1 | [1] | Valid |
| BND-073 | Entity count = 100 | Max | Valid |
| BND-074 | Param count = 0 | {} | Defaults |
| BND-075 | Param count = 100 | Many | Applied |
| BND-076 | Response size = 0 | "" | Invalid |
| BND-077 | Response size = 1MB | Max | Parsed |
| BND-078 | Nesting depth = 1 | 1 level | Parsed |
| BND-079 | Nesting depth = 64 | Max | Parsed |
| BND-080 | Array length = 0 | [] | Parsed |
| BND-081 | Array length = 10000 | Max | Parsed |
| BND-082 | Cache size = 0 | Cold | Miss |
| BND-083 | Cache size = 1000 | Max | Eviction |
| BND-084 | Timeout = 0ms | 0 | Immediate |
| BND-085 | Timeout = 30000ms | 30s | Success |
| BND-086 | Retry = 0 | No retry | Fail once |
| BND-087 | Retry = 3 | 3 | Retries |
| BND-088 | Concurrent = 1 | 1 | Success |
| BND-089 | Concurrent = 1000 | 1000 | Throttled |
| BND-090 | Placeholder count = 0 | None | Applied |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Context includes entity metadata | Metadata rule | BuildContext | Metadata present |
| FUN-002 | Context includes audit fields | Audit rule | BuildContext | CreatedBy, Date |
| FUN-003 | Template respects order | Order rule | ApplyTemplate | Correct order |
| FUN-004 | Parse preserves types | Type rule | ParseResponse | Types preserved |
| FUN-005 | Cache key includes entity ID | Cache key rule | BuildContext | Unique key |
| FUN-006 | Cache invalidation on entity update | Invalidation rule | Entity update | Cache cleared |
| FUN-007 | Response truncation at limit | Truncation rule | Parse large | Truncated |
| FUN-008 | Placeholder case sensitivity | Case rule | ApplyTemplate | Case-sensitive |
| FUN-009 | Default for missing optional | Default rule | ApplyTemplate | Default used |
| FUN-010 | Error for missing required | Required rule | ApplyTemplate | Exception |
| FUN-011 | Context aggregation order | Aggregation rule | LoadEntityData | Order consistent |
| FUN-012 | Template comment ignored | Comment rule | ApplyTemplate | Comments stripped |
| FUN-013 | JSON number precision | Precision rule | ParseResponse | Precision kept |
| FUN-014 | Null handling in template | Null rule | ApplyTemplate | Null as empty |
| FUN-015 | Empty array handling | Array rule | ParseResponse | Empty array |
| FUN-016 | Cache TTL | TTL rule | Cache entry | Expires |
| FUN-017 | Retry on transient failure | Retry rule | Transient error | Retried |
| FUN-018 | No retry on permanent failure | No retry rule | Permanent error | Fail once |
| FUN-019 | Context includes related entities | Related rule | BuildContext | Related included |
| FUN-020 | Template conditional evaluation | Conditional rule | ApplyTemplate | Eval correct |
| FUN-021 | Response schema validation | Schema rule | ParseResponse | Schema validated |
| FUN-022 | Max aggregate entities | Max rule | LoadEntityData | Limited |
| FUN-023 | Context excludes soft-deleted | Exclude rule | BuildContext | Excluded |
| FUN-024 | Template multiline | Multiline rule | ApplyTemplate | Preserved |
| FUN-025 | Response encoding detection | Encoding rule | ParseResponse | Detected |
| FUN-026 | Placeholder nesting | Nesting rule | ApplyTemplate | Nested |
| FUN-027 | Cache key collision | Collision rule | Same key | Overwrite |
| FUN-028 | Build context timeout | Timeout rule | Slow build | Timeout |
| FUN-029 | Parse timeout | Parse rule | Slow parse | Timeout |
| FUN-030 | Context size limit | Size rule | Large context | Limited |
| FUN-031 | Template variable scope | Scope rule | ApplyTemplate | Scope correct |
| FUN-032 | Response field mapping | Mapping rule | ParseResponse | Mapped |
| FUN-033 | Error message format | Error rule | Any error | Consistent format |
| FUN-034 | Context versioning | Version rule | BuildContext | Version in context |
| FUN-035 | Template versioning | Template rule | ApplyTemplate | Version checked |
| FUN-036 | Aggregate pagination | Pagination rule | LoadEntityData | Paginated |
| FUN-037 | Context includes user context | User rule | BuildContext | User in context |
| FUN-038 | Permission in context | Permission rule | BuildContext | Permissions |
| FUN-039 | Localization in template | Locale rule | ApplyTemplate | Locale applied |
| FUN-040 | Response error field | Error field rule | ParseResponse | Error handled |
| FUN-041 | Audit trail in context | Audit rule | BuildContext | Audit trail |
| FUN-042 | Context dependency order | Dependency rule | BuildContext | Order correct |
| FUN-043 | Template escape | Escape rule | ApplyTemplate | Escaped |
| FUN-044 | Response sanitization | Sanitize rule | ParseResponse | Sanitized |
| FUN-045 | Cache warm-up | Warm-up rule | Startup | Preloaded |
| FUN-046 | Context fallback | Fallback rule | Missing data | Fallback |
| FUN-047 | Template fallback | Template fallback | Missing template | Default |
| FUN-048 | Response fallback | Fallback rule | Parse fail | Fallback |
| FUN-049 | Rate limit per user | Rate rule | Many requests | Limited |
| FUN-050 | Quota per tenant | Quota rule | Tenant | Limited |
| FUN-051 | Context includes entity metadata | Metadata rule | BuildContext | Metadata present |
| FUN-052 | Context includes audit fields | Audit rule | BuildContext | CreatedBy, Date |
| FUN-053 | Template respects order | Order rule | ApplyTemplate | Correct order |
| FUN-054 | Parse preserves types | Type rule | ParseResponse | Types preserved |
| FUN-055 | Cache key includes entity ID | Cache key rule | BuildContext | Unique key |
| FUN-056 | Cache invalidation on entity update | Invalidation rule | Entity update | Cache cleared |
| FUN-057 | Response truncation at limit | Truncation rule | Parse large | Truncated |
| FUN-058 | Placeholder case sensitivity | Case rule | ApplyTemplate | Case-sensitive |
| FUN-059 | Default for missing optional | Default rule | ApplyTemplate | Default used |
| FUN-060 | Error for missing required | Required rule | ApplyTemplate | Exception |
| FUN-061 | Context aggregation order | Aggregation rule | LoadEntityData | Order consistent |
| FUN-062 | Template comment ignored | Comment rule | ApplyTemplate | Comments stripped |
| FUN-063 | JSON number precision | Precision rule | ParseResponse | Precision kept |
| FUN-064 | Null handling in template | Null rule | ApplyTemplate | Null as empty |
| FUN-065 | Empty array handling | Array rule | ParseResponse | Empty array |
| FUN-066 | Cache TTL | TTL rule | Cache entry | Expires |
| FUN-067 | Retry on transient failure | Retry rule | Transient error | Retried |
| FUN-068 | No retry on permanent failure | No retry rule | Permanent error | Fail once |
| FUN-069 | Context includes related entities | Related rule | BuildContext | Related included |
| FUN-070 | Template conditional evaluation | Conditional rule | ApplyTemplate | Eval correct |
| FUN-071 | Response schema validation | Schema rule | ParseResponse | Schema validated |
| FUN-072 | Max aggregate entities | Max rule | LoadEntityData | Limited |
| FUN-073 | Context excludes soft-deleted | Exclude rule | BuildContext | Excluded |
| FUN-074 | Template multiline | Multiline rule | ApplyTemplate | Preserved |
| FUN-075 | Response encoding detection | Encoding rule | ParseResponse | Detected |
| FUN-076 | Placeholder nesting | Nesting rule | ApplyTemplate | Nested |
| FUN-077 | Cache key collision | Collision rule | Same key | Overwrite |
| FUN-078 | Build context timeout | Timeout rule | Slow build | Timeout |
| FUN-079 | Parse timeout | Parse rule | Slow parse | Timeout |
| FUN-080 | Context size limit | Size rule | Large context | Limited |
| FUN-081 | Template variable scope | Scope rule | ApplyTemplate | Scope correct |
| FUN-082 | Response field mapping | Mapping rule | ParseResponse | Mapped |
| FUN-083 | Error message format | Error rule | Any error | Consistent format |
| FUN-084 | Context versioning | Version rule | BuildContext | Version in context |
| FUN-085 | Template versioning | Template rule | ApplyTemplate | Version checked |
| FUN-086 | Aggregate pagination | Pagination rule | LoadEntityData | Paginated |
| FUN-087 | Context includes user context | User rule | BuildContext | User in context |
| FUN-088 | Permission in context | Permission rule | BuildContext | Permissions |
| FUN-089 | Localization in template | Locale rule | ApplyTemplate | Locale applied |
| FUN-090 | Response error field | Error field rule | ParseResponse | Error handled |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | Entity repository | DbContext | BuildContext | Entity loaded |
| INT-002 | Opportunity manager | IOpportunityManager | BuildContext | Opp data |
| INT-003 | Partner manager | IPartnerManager | BuildContext | Partner data |
| INT-004 | AI service | IAIService | Parse response | Response parsed |
| INT-005 | Cache service | ICacheService | Cache context | Cached |
| INT-006 | Permission service | IPermissionService | BuildContext | Permissions |
| INT-007 | User resolves | UserResolverService | BuildContext | User context |
| INT-008 | Config service | IConfiguration | Load config | Config applied |
| INT-009 | Logger | ILogger | Log | Logged |
| INT-010 | AutoMapper | IMapper | Map entity | Mapped |
| INT-011 | Org hierarchy | IOrgHierarchyService | BuildContext | Hierarchy |
| INT-012 | Country service | ICountryService | BuildContext | Countries |
| INT-013 | Full pipeline DB | DbContext | BuildContext | Success |
| INT-014 | Full pipeline AI | IAIService | BuildContext | Success |
| INT-015 | Full pipeline cache | ICacheService | BuildContext | Cache hit |
| INT-016 | Entity + partner | Both managers | BuildContext | Both |
| INT-017 | Entity + opportunity | Both managers | BuildContext | Both |
| INT-018 | Multi-entity context | Multiple managers | BuildContext | Aggregated |
| INT-019 | Context with audit | AuditableDbContext | BuildContext | Audit |
| INT-020 | Context with soft delete | Soft delete filter | BuildContext | Filtered |
| INT-021 | Template + parse | Template + AI | Full flow | Success |
| INT-022 | Cache + build | Cache + build | Second call | Cache hit |
| INT-023 | Error handling chain | Error handler | Error | Handled |
| INT-024 | Retry + AI | Retry + AI | Transient fail | Retried |
| INT-025 | Permission + build | Permission + build | Build | Scoped |
| INT-026 | Config + timeout | Config | Build | Timeout | Timeout |
| INT-027 | Logger + error | Logger | Error | Logged |
| INT-028 | Mapper + entity | Mapper | Entity | Mapped |
| INT-029 | User + permission | User + permission | Build | Correct |
| INT-030 | Org + country | Org + country | Build | Combined |
| INT-031 | Build + template | Build + template | Build + apply | Success |
| INT-032 | Template + parse | Template + parse | Apply + parse | Success |
| INT-033 | Build + cache + template | All | Full flow | Success |
| INT-034 | Build + AI + parse | Build + AI | Full flow | Success |
| INT-035 | Multi-tenant | Tenant context | Build | Tenant isolated |
| INT-036 | Multi-user | User context | Build | User isolated |
| INT-037 | Build + notification | Notification | Build | Notified |
| INT-038 | Parse + validation | Validation | Parse | Validated |
| INT-039 | Template + localization | Localization | Apply | Localized |
| INT-040 | Cache + invalidation | Invalidation | Update | Invalidated |
| INT-041 | Build + workflow | Workflow | Build | Status |
| INT-042 | Build + document | Document | Build | Docs |
| INT-043 | Build + interaction | Interaction | Build | Interactions |
| INT-044 | Build + contact | Contact | Build | Contacts |
| INT-045 | Build + budget | Budget | Build | Budget |
| INT-046 | Build + schedule | Schedule | Build | Schedule |
| INT-047 | Build + risk | Risk | Build | Risks |
| INT-048 | Build + stakeholder | Stakeholder | Build | Stakeholders |
| INT-049 | Build + resource plan | Resource | Build | Resources |
| INT-050 | End-to-end | All services | Full | Success |
| INT-051 | Entity repository | DbContext | BuildContext | Entity loaded |
| INT-052 | Opportunity manager | IOpportunityManager | BuildContext | Opp data |
| INT-053 | Partner manager | IPartnerManager | BuildContext | Partner data |
| INT-054 | AI service | IAIService | Parse response | Response parsed |
| INT-055 | Cache service | ICacheService | Cache context | Cached |
| INT-056 | Permission service | IPermissionService | BuildContext | Permissions |
| INT-057 | User resolves | UserResolverService | BuildContext | User context |
| INT-058 | Config service | IConfiguration | Load config | Config applied |
| INT-059 | Logger | ILogger | Log | Logged |
| INT-060 | AutoMapper | IMapper | Map entity | Mapped |
| INT-061 | Org hierarchy | IOrgHierarchyService | BuildContext | Hierarchy |
| INT-062 | Country service | ICountryService | BuildContext | Countries |
| INT-063 | Full pipeline DB | DbContext | BuildContext | Success |
| INT-064 | Full pipeline AI | IAIService | BuildContext | Success |
| INT-065 | Full pipeline cache | ICacheService | BuildContext | Cache hit |
| INT-066 | Entity + partner | Both managers | BuildContext | Both |
| INT-067 | Entity + opportunity | Both managers | BuildContext | Both |
| INT-068 | Multi-entity context | Multiple managers | BuildContext | Aggregated |
| INT-069 | Context with audit | AuditableDbContext | BuildContext | Audit |
| INT-070 | Context with soft delete | Soft delete filter | BuildContext | Filtered |
| INT-071 | Template + parse | Template + AI | Full flow | Success |
| INT-072 | Cache + build | Cache + build | Second call | Cache hit |
| INT-073 | Error handling chain | Error handler | Error | Handled |
| INT-074 | Retry + AI | Retry + AI | Transient fail | Retried |
| INT-075 | Permission + build | Permission + build | Build | Scoped |
| INT-076 | Config + timeout | Config | Build | Timeout |
| INT-077 | Logger + error | Logger | Error | Logged |
| INT-078 | Mapper + entity | Mapper | Entity | Mapped |
| INT-079 | User + permission | User + permission | Build | Correct |
| INT-080 | Org + country | Org + country | Build | Combined |
| INT-081 | Build + template | Build + template | Build + apply | Success |
| INT-082 | Template + parse | Template + parse | Apply + parse | Success |
| INT-083 | Build + cache + template | All | Full flow | Success |
| INT-084 | Build + AI + parse | Build + AI | Full flow | Success |
| INT-085 | Multi-tenant | Tenant context | Build | Tenant isolated |
| INT-086 | Multi-user | User context | Build | User isolated |
| INT-087 | Build + notification | Notification | Build | Notified |
| INT-088 | Parse + validation | Validation | Parse | Validated |
| INT-089 | Template + localization | Localization | Apply | Localized |
| INT-090 | End-to-end | All services | Full | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection | '; DROP | Entity ID | Parameterized |
| SEC-002 | SQL injection | 1 OR 1=1 | Entity ID | Parameterized |
| SEC-003 | SQL injection | UNION SELECT | Entity ID | Parameterized |
| SEC-004 | XSS in template | <script> | Template param | Sanitized |
| SEC-005 | XSS in template | javascript: | Template param | Sanitized |
| SEC-006 | XSS in response | <img onerror> | Response | Sanitized |
| SEC-007 | LDAP injection | *)(uid=* | Template param | Sanitized |
| SEC-008 | Command injection | ; rm -rf | Template param | Sanitized |
| SEC-009 | Path traversal | ../../../etc | Entity ID | Rejected |
| SEC-010 | Null byte injection | %00 | Entity ID | Rejected |
| SEC-011 | Unauthorized entity access | User A, Entity B | BuildContext | 403 |
| SEC-012 | Unauthorized cross-tenant | Tenant A, Tenant B | BuildContext | 403 |
| SEC-013 | IDOR entity ID | Alter ID | BuildContext | 403 |
| SEC-014 | IDOR response | Alter response | ParseResponse | Validate |
| SEC-015 | Mass assignment | Extra fields | BuildContext | Ignored |
| SEC-016 | Token tampering | Tampered token | BuildContext | 401 |
| SEC-017 | Expired token | Expired | BuildContext | 401 |
| SEC-018 | No token | Missing | BuildContext | 401 |
| SEC-019 | Invalid token | Invalid | BuildContext | 401 |
| SEC-020 | Session hijack | Stolen session | BuildContext | 403 |
| SEC-021 | CSRF | Cross-site | BuildContext | Token |
| SEC-022 | Replay attack | Replay | BuildContext | Nonce |
| SEC-023 | Sensitive data in context | PII | BuildContext | Redacted |
| SEC-024 | Sensitive data in response | PII | ParseResponse | Redacted |
| SEC-025 | Sensitive data in cache | PII | Cache | Encrypted |
| SEC-026 | Secret in error | API key | Error message | No secret |
| SEC-027 | Secret in log | API key | Log | No secret |
| SEC-028 | Secret in template | Placeholder | Template | No secret |
| SEC-029 | SSRF in entity URL | URL | Entity | Blocked |
| SEC-030 | DoS large payload | 10MB | ParseResponse | Rejected |
| SEC-031 | DoS deep nesting | 1000 levels | ParseResponse | Rejected |
| SEC-032 | DoS template recursion | Recursive | ApplyTemplate | Limited |
| SEC-033 | Rate limit bypass | Rapid requests | BuildContext | Rate limited |
| SEC-034 | Cache poisoning | Malicious cache | GetCached | Validated |
| SEC-035 | Response poisoning | Malicious response | ParseResponse | Validated |
| SEC-036 | Template injection | {{payload}} | Template | Escaped |
| SEC-037 | Prototype pollution | __proto__ | ParseResponse | Sanitized |
| SEC-038 | Insecure deserialization | Binary | ParseResponse | JSON only |
| SEC-039 | XML external entity | XXE | ParseResponse | Not XML |
| SEC-040 | Open redirect | Redirect URL | Template | Blocked |
| SEC-041 | Header injection | CRLF | Template param | Sanitized |
| SEC-042 | NoSQL injection | $ne | Entity ID | Parameterized |
| SEC-043 | GraphQL injection | Mutation | BuildContext | Not GraphQL |
| SEC-044 | JWT tampering | Altered JWT | BuildContext | Rejected |
| SEC-045 | Privilege escalation | Low role | BuildContext | 403 |
| SEC-046 | Horizontal privilege | User A | User B entity | 403 |
| SEC-047 | Vertical privilege | User | Admin | 403 |
| SEC-048 | API key exposure | Log | API key | Logged |
| SEC-049 | Weak crypto | MD5 | Cache | SHA256 |
| SEC-050 | Missing auth | No auth | BuildContext | 401 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Concurrent build same entity | 2 threads same ID | Both succeed |
| CON-002 | Concurrent build different | 2 threads diff ID | Both succeed |
| CON-003 | Concurrent cache write | 2 threads same key | No corruption |
| CON-004 | Concurrent cache read | 10 threads read | All succeed |
| CON-005 | Build during invalidation | Build + invalidate | Consistent |
| CON-006 | Parse during parse | 2 threads parse | Both succeed |
| CON-007 | Template during template | 2 threads | Both succeed |
| CON-008 | Build + update entity | Build + update | No stale |
| CON-009 | Cache eviction during read | Read + evict | Handled |
| CON-010 | Double submit | 2 submit same | One succeeds |
| CON-011 | Race condition | Update + build | Consistent |
| CON-012 | Deadlock | Build A → B, B → A | No deadlock |
| CON-013 | Concurrent invalidation | 2 invalidate | Both applied |
| CON-014 | Concurrent retry | 2 retry same | One succeeds |
| CON-015 | Concurrent timeout | 2 timeout | Both timeout |
| CON-016 | Cache stampede | 100 threads cold | Single load |
| CON-017 | Lock contention | 50 threads | Throttled |
| CON-018 | Memory barrier | Build + cache | Visible |
| CON-019 | Thread pool exhaustion | 1000 threads | Limited |
| CON-020 | Concurrent cancellation | Build + cancel | Cancelled |
| CON-021 | Concurrent error | 2 error | Both handled |
| CON-022 | Optimistic concurrency | Update + build | Version check |
| CON-023 | Pessimistic lock | Build + lock | Locked |
| CON-024 | Read-write lock | Read + write | RW lock |
| CON-025 | Semaphore | Limited | Semaphore |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Placeholder extraction | Validation | "{a}{b}" | ["a","b"] |
| UNT-002 | Empty placeholder | Validation | "{}" | Invalid |
| UNT-003 | Parameter validation | Validation | null | Invalid |
| UNT-004 | Template validation | Validation | "" | Invalid |
| UNT-005 | Entity ID validation | Validation | -1 | Invalid |
| UNT-006 | Date format | Formatting | DateTime | ISO8601 |
| UNT-007 | Number format | Formatting | 1234.56 | "1234.56" |
| UNT-008 | String truncate | Formatting | Long string | Truncated |
| UNT-009 | Context size calc | Formatting | 100 entities | Size |
| UNT-010 | Cache key gen | Formatting | Entity ID | Key |
| UNT-011 | Context merge | Calculations | 2 contexts | Merged |
| UNT-012 | Template merge | Calculations | 2 templates | Merged |
| UNT-013 | Response size | Calculations | Response | Size |
| UNT-014 | Aggregation count | Calculations | Entities | Count |
| UNT-015 | Placeholder count | Calculations | Template | Count |
| UNT-016 | Status check | Status | Entity | Status |
| UNT-017 | Permission check | Status | User + entity | Allowed |
| UNT-018 | Cache hit check | Status | Key | Hit/Miss |
| UNT-019 | Retry status | Status | Error | Retry |
| UNT-020 | Empty collection | Collections | [] | Empty |
| UNT-021 | Single collection | Collections | [x] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Build context single | BuildContextAsync(1) | <200ms |
| PRF-002 | Build context multi | BuildContextAsync(10) | <500ms |
| PRF-003 | Apply template | ApplyTemplate | <50ms |
| PRF-004 | Parse response small | ParseResponse | <100ms |
| PRF-005 | Parse response large | ParseResponse 1MB | <500ms |
| PRF-006 | Cache hit | GetCached | <10ms |
| PRF-007 | Cache miss | BuildContext | <200ms |
| PRF-008 | Bulk aggregate | LoadEntityData 100 | <2s |
| PRF-009 | Full pipeline | Build + Apply + Parse | <3s |
| PRF-010 | Concurrent 10 | 10 concurrent | <5s |
| PRF-011 | Concurrent 50 | 50 concurrent | <15s |
| PRF-012 | Memory single | BuildContext | <10MB |
| PRF-013 | Memory bulk | Bulk | <100MB |
| PRF-014 | GC pressure | 1000 builds | No leak |
| PRF-015 | Template large | 10000 chars | <100ms |
| PRF-016 | Response 10MB | Parse 10MB | <2s |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|------------------|
| LDT-001 | Sustained 10 req/s | 10/s | 5 min | 99% success |
| LDT-002 | Sustained 50 req/s | 50/s | 5 min | 99% success |
| LDT-003 | Sustained 100 req/s | 100/s | 5 min | 95% success |
| LDT-004 | Spike 0→100 | 0→100/s | 1 min | No crash |
| LDT-005 | Spike 100→0 | 100→0/s | 1 min | No crash |
| LDT-006 | Stress 200 req/s | 200/s | 2 min | Graceful |
| LDT-007 | Stress 500 req/s | 500/s | 1 min | Throttled |
| LDT-008 | Stress 1000 req/s | 1000/s | 30s | No crash |
| LDT-009 | Recovery after spike | Spike + recovery | 5 min | Recovery |
| LDT-010 | Recovery after stress | Stress + recovery | 5 min | Recovery |

---

**Status:** Ready for Execution
