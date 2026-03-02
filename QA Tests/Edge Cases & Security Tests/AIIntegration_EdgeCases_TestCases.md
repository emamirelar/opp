# AI Integration — Edge Cases & Test Cases

**Component:** AI Integration (GeminiManager, AiContextualService, DST, Embeddings, Statement Generation)  
**Created:** 2026-02-18  
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
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P? 90≥90 ✅ | E≥3P? 90≥90 ✅ | F≥3P? 90≥90 ✅ | I≥3P? 90≥90 ✅

---

## Feature Overview

**AI Integration** covers DST (Decision Support Tool) recommendations, similar project search, statement generation, embedding services, prompt management, cache behavior, and vector store interactions. Edge cases include timeouts, rate limits, malformed responses, token limits, and cross-service failures.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| POS-001 | DST recommendation with valid opportunity | Opportunity with complete data | Call GetDSTRecommendation | Valid recommendations | P0 |
| POS-002 | DST with standard risk keywords | Opportunity with risk terms | Analyze risks | Risk keywords identified | P0 |
| POS-003 | Similar projects with good embedding match | Vector store populated | Search similar | Relevant projects returned | P0 |
| POS-004 | Statement generation with complete opportunity | Full opportunity data | Generate statement | Coherent statement | P0 |
| POS-005 | AI prompt resolution with all placeholders | Prompt with {opportunity}, {partner} | Resolve | All replaced | P0 |
| POS-006 | Embedding generation for opportunity | Valid opportunity text | Generate embedding | Vector returned | P0 |
| POS-007 | Cache hit for repeated DST request | Same opportunity, cached | Request DST | Cache hit, fast response | P0 |
| POS-008 | GeminiManager valid API response | API available | Call Gemini | Valid response | P0 |
| POS-009 | AiContextualService context building | Opportunity + partner | Build context | Complete context | P0 |
| POS-010 | Risk recommendation to RiskManager | DST recommends risk | Create risk | Risk created | P0 |
| POS-011 | Statement generation to Opportunity update | Generated statement | Update opportunity | Statement saved | P0 |
| POS-012 | Embedding storage in EntityEmbeddings | New embedding | Store | Stored correctly | P0 |
| POS-013 | Prompt template with valid variables | Template {var} | Resolve | Correct output | P0 |
| POS-014 | DST confidence score in range | Recommendation | Get score | 0.0–1.0 | P0 |
| POS-015 | Similar projects sorted by similarity | Multiple matches | Sort | Descending similarity | P0 |
| POS-016 | AI response JSON parsing | Valid JSON response | Parse | Object parsed | P0 |
| POS-017 | Rate limit within quota | Under limit | Multiple requests | All succeed | P0 |
| POS-018 | Vector store connection | Store available | Connect | Connected | P0 |
| POS-019 | Prompt fallback when primary fails | Primary prompt error | Fallback | Secondary used | P1 |
| POS-020 | Retry on transient AI failure | Transient 503 | Retry | Success | P1 |
| POS-021 | Batch embedding generation | Multiple texts | Batch | All embedded | P1 |
| POS-022 | DST deduplication of recommendations | Duplicate risks | Deduplicate | Unique list | P1 |
| POS-023 | Statement with special characters | Opportunity with &, <, > | Generate | Escaped correctly | P1 |
| POS-024 | Embedding search with threshold 0.7 | Similarity 0.8 | Search | Match returned | P1 |
| POS-025 | Cache TTL respected | Cached item | Wait TTL | Cache miss | P1 |
| POS-026 | AI prompt localization | Locale fr | Resolve | French text | P1 |
| POS-027 | OpportunityController → GeminiManager flow | Valid request | Full flow | Success | P0 |
| POS-028 | Risk recommendation CRUD via RiskManager | DST risk | Create/Update | CRUD works | P0 |
| POS-029 | Statement generation → Opportunity update | Statement | Save | Persisted | P0 |
| POS-030 | Embedding → EntityEmbeddings storage | Embedding | Store | Queryable | P0 |

---

## §2 Negative Tests (90)

### 2.1 AI Service Failures (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-001 | AI service timeout | Request exceeds timeout | TimeoutException, graceful handling | P0 |
| NEG-002 | AI rate limit exceeded | Too many requests | 429, retry-after or queue | P0 |
| NEG-003 | AI quota exhausted | Quota exceeded | QuotaExceededException | P0 |
| NEG-004 | AI service unavailable | 503 from API | Retry or fallback | P0 |
| NEG-005 | AI invalid API key | Bad key | 401 Unauthorized | P0 |
| NEG-006 | AI connection refused | Network down | ConnectionException | P0 |
| NEG-007 | AI SSL/TLS error | Certificate invalid | SecurityException | P0 |
| NEG-008 | AI malformed request | Invalid payload | 400 Bad Request | P0 |
| NEG-009 | AI content filtered | Policy violation | FilteredResponseException | P0 |
| NEG-010 | AI model not found | Wrong model ID | 404 | P0 |
| NEG-011 | Embedding service unavailable | Embedding API down | ServiceUnavailableException | P0 |
| NEG-012 | Vector store connection failure | DB/store down | ConnectionException | P0 |
| NEG-013 | Vector store auth failure | Bad credentials | AuthException | P0 |
| NEG-014 | AI response timeout | Slow response | TimeoutException | P0 |
| NEG-015 | AI circuit breaker open | Repeated failures | CircuitOpenException | P1 |
| NEG-016 | AI retry exhausted | All retries fail | RetryExhaustedException | P1 |
| NEG-017 | AI payload too large | Oversized request | 413 or reject | P1 |
| NEG-018 | AI invalid embedding dimensions | Wrong dim count | DimensionMismatchException | P0 |
| NEG-019 | AI corrupted vector data | Invalid vector format | ParseException | P1 |
| NEG-020 | AI concurrent limit exceeded | Too many concurrent | Throttled | P1 |

### 2.2 Invalid Input / Context (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-021 | Empty opportunity context | Opportunity null/empty | ValidationException | P0 |
| NEG-022 | Missing required fields for DST | Opportunity incomplete | ValidationException | P0 |
| NEG-023 | Invalid opportunity ID | Id=0 or -1 | ArgumentException | P0 |
| NEG-024 | Deleted opportunity | Opportunity IsDeleted | KeyNotFoundException | P0 |
| NEG-025 | Prompt template with unresolvable placeholders | {unknown} in template | UnresolvedPlaceholderException | P0 |
| NEG-026 | AI response invalid JSON | Malformed JSON | JsonException | P0 |
| NEG-027 | AI response empty | Empty string | EmptyResponseException | P0 |
| NEG-028 | AI response null | Null response | NullReferenceException handled | P0 |
| NEG-029 | Invalid embedding dimensions | Dim mismatch | DimensionException | P0 |
| NEG-030 | Null text for embedding | Text=null | ArgumentNullException | P0 |
| NEG-031 | Empty text for embedding | Text="" | ValidationException or skip | P1 |
| NEG-032 | Invalid prompt ID | PromptId=99999 | KeyNotFoundException | P0 |
| NEG-033 | Deleted prompt | Prompt IsDeleted | KeyNotFoundException | P0 |
| NEG-034 | Invalid similarity threshold | Threshold=-1 or 2 | ArgumentException | P0 |
| NEG-035 | Null search query | Query=null | ArgumentNullException | P0 |
| NEG-036 | Invalid entity type for embedding | Wrong EntityType | ArgumentException | P1 |
| NEG-037 | Statement generation missing partner | No partner | ValidationException | P0 |
| NEG-038 | DST with no countries | Opportunity no countries | ValidationException | P0 |
| NEG-039 | Risk recommendation invalid type | Invalid risk type | ValidationException | P1 |
| NEG-040 | Cache key null | Key=null | ArgumentNullException | P1 |

### 2.3 Security & Integrity (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-041 | Cache poisoning attempt | Malicious cache write | Validation, reject | P0 |
| NEG-042 | Stale cache data | Expired cache used | Cache miss, refresh | P0 |
| NEG-043 | Prompt injection in opportunity | Malicious text in opportunity | Sanitized or rejected | P0 |
| NEG-044 | XSS in AI response | Response with script | Escaped on display | P0 |
| NEG-045 | Unauthorized DST request | No permission | 403 Forbidden | P0 |
| NEG-046 | Cross-org opportunity for AI | User org A, opp org B | 403 | P0 |
| NEG-047 | AI API key exposure in log | Log request | Key redacted | P0 |
| NEG-048 | Embedding data tampering | Tampered vector | Validation fail | P1 |
| NEG-049 | Rate limit bypass attempt | Manipulate headers | Enforced | P0 |
| NEG-050 | Oversized prompt injection | Huge input | Rejected or truncated | P0 |
| NEG-051 | Invalid cache TTL | Negative TTL | ArgumentException | P1 |
| NEG-052 | SQL injection in vector search | Malicious query | Parameterized | P0 |
| NEG-053 | IDOR in embedding fetch | Other user's entity | 403 | P0 |
| NEG-054 | Statement generation no permission | No CanEdit | 403 | P0 |
| NEG-055 | Risk create no permission | No CanCreate risk | 403 | P0 |

### 2.4 Data & State (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-056 | Opportunity modified during DST | Concurrent update | Stale or conflict | P1 |
| NEG-057 | Partner deleted during context build | Partner soft-deleted | Handled | P1 |
| NEG-058 | Embedding for deleted entity | Entity IsDeleted | Rejected | P0 |
| NEG-059 | Vector store out of space | Storage full | StorageException | P1 |
| NEG-060 | Duplicate embedding for same entity | Re-embed | Update or reject | P1 |
| NEG-061 | Orphaned embedding after entity delete | Entity deleted | Cleanup or ignore | P1 |
| NEG-062 | Invalid RecordData for AI context | Malformed RecordData | ParseException | P1 |
| NEG-063 | Missing EntityConfiguration for AI | No config | Default or error | P1 |
| NEG-064 | Statement overwrite conflict | Concurrent update | Conflict handled | P1 |
| NEG-065 | Risk recommendation duplicate | Same risk exists | Deduplicate or reject | P1 |
| NEG-066 | Embedding version mismatch | Old format | Migration or error | P1 |
| NEG-067 | Prompt version changed mid-request | Prompt updated | Use version or error | P1 |
| NEG-068 | Cache key collision | Same key different data | Unique keys | P1 |
| NEG-069 | AI response schema change | New schema | Backward compat or error | P1 |
| NEG-070 | Vector store index corrupted | Index error | Rebuild or error | P1 |

### 2.5 Integration Failures (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | GeminiManager → AI API failure | API down | Propagate or fallback | P0 |
| NEG-072 | AiContextualService → vector store failure | Store down | Graceful degradation | P0 |
| NEG-073 | DST → RiskManager create failure | RiskManager error | Rollback or retry | P0 |
| NEG-074 | Statement → Opportunity update failure | Update fails | Transaction rollback | P0 |
| NEG-075 | Embedding → EntityEmbeddings save failure | DB error | Retry or error | P0 |
| NEG-076 | OpportunityController → GeminiManager timeout | Slow Gemini | Timeout to client | P0 |
| NEG-077 | Risk recommendation → RiskManager validation | Invalid risk | ValidationException | P0 |
| NEG-078 | Statement generation → mapping failure | Map error | Exception | P1 |
| NEG-079 | Gmail addon → AI context failure | Context build fail | Fallback | P1 |
| NEG-080 | Search → embedding service down | Embedding unavailable | Fallback to keyword | P1 |
| NEG-081 | Prompt resolution → missing dependency | Manager null | NullReferenceException | P1 |
| NEG-082 | Cache → distributed cache failure | Redis down | Fallback to no cache | P1 |
| NEG-083 | AI → audit logging failure | Audit error | Log, continue | P1 |
| NEG-084 | Batch embedding → partial failure | Some fail | Partial success or all fail | P1 |
| NEG-085 | DST → notification failure | Notification error | Continue, log | P1 |
| NEG-086 | Vector search → pagination error | Invalid page | ArgumentException | P1 |
| NEG-087 | AI response → model mapping null | Null in response | Default or error | P1 |
| NEG-088 | Embedding → entity link failure | Entity not found | KeyNotFoundException | P1 |
| NEG-089 | Statement → workflow state invalid | Wrong state | BusinessException | P1 |
| NEG-090 | Full pipeline → DB connection lost | Mid-pipeline | Transaction rollback | P0 |

---

## §3 Boundary Tests (90)

### 3.1 Token & Length (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-001 | Maximum prompt length (token limit) | Prompt at limit | Accepted or truncated | P0 |
| BND-002 | Minimum context for DST | Minimal valid context | Success | P0 |
| BND-003 | Prompt 1 char under limit | Limit-1 | Success | P1 |
| BND-004 | Prompt 1 char over limit | Limit+1 | Truncated or rejected | P0 |
| BND-005 | Opportunity description 10,000 chars | Very long | Handled | P0 |
| BND-006 | Opportunity description 10,001 chars | Over typical max | Truncated or rejected | P1 |
| BND-007 | Empty opportunity description | "" | Handled | P1 |
| BND-008 | Single character context | "A" | Minimal context | P1 |
| BND-009 | Embedding input max length | Max chars | Success | P1 |
| BND-010 | Embedding input over max | Over max | Truncated or error | P1 |
| BND-011 | Search query max length | Max | Success | P1 |
| BND-012 | Search query empty | "" | No results or error | P1 |
| BND-013 | Batch embedding max batch size | Max batch | Success | P1 |
| BND-014 | Batch embedding over max | Over max | Split or reject | P1 |
| BND-015 | Cache key max length | Max key | Success | P1 |

### 3.2 Confidence & Similarity (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-016 | Confidence score 0.00 | Min confidence | Edge case | P0 |
| BND-017 | Confidence score 0.01 | Just above zero | Low confidence | P0 |
| BND-018 | Confidence score 0.99 | Just below one | High confidence | P0 |
| BND-019 | Confidence score 1.00 | Max confidence | Exact match | P0 |
| BND-020 | Confidence score -0.01 | Invalid | Rejected | P0 |
| BND-021 | Confidence score 1.01 | Invalid | Clamped or rejected | P0 |
| BND-022 | Similarity threshold 0.0 | All match | All results | P1 |
| BND-023 | Similarity threshold 1.0 | Exact only | Exact matches | P1 |
| BND-024 | Similarity threshold 0.5 | Mid | Filtered | P1 |
| BND-025 | Zero search results | No matches | Empty list | P0 |
| BND-026 | Maximum results (100+) | Many matches | Capped or paginated | P0 |
| BND-027 | Exactly 1 result | Single match | Single item | P1 |
| BND-028 | Similarity at threshold | Score = threshold | Included | P1 |
| BND-029 | Similarity just below threshold | Score = threshold - ε | Excluded | P1 |
| BND-030 | Float precision confidence | 0.9999999 | Handled | P1 |

### 3.3 Risk Keywords (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-031 | Empty risk keywords | [] | No risks identified | P0 |
| BND-032 | Single risk keyword | ["risk"] | One risk | P0 |
| BND-033 | Maximum risk keywords | 100 keywords | All processed | P1 |
| BND-034 | Risk keyword max length | Very long keyword | Truncated or reject | P1 |
| BND-035 | Risk keyword special chars | "risk & co" | Escaped | P1 |
| BND-036 | Risk keyword unicode | "risque" | Handled | P1 |
| BND-037 | Duplicate risk keywords | ["r","r"] | Deduplicated | P1 |
| BND-038 | Risk keyword null | null | Handled | P1 |
| BND-039 | Risk keyword empty string | "" | Ignored | P1 |
| BND-040 | Risk keyword whitespace only | "   " | Ignored | P1 |

### 3.4 Timeouts & Timing (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-041 | Response timeout at exactly threshold | Response at limit | Success or timeout | P0 |
| BND-042 | Response 1ms before timeout | Just under | Success | P1 |
| BND-043 | Response 1ms after timeout | Just over | Timeout | P1 |
| BND-044 | Retry delay minimum | Min delay | Applied | P1 |
| BND-045 | Retry delay maximum | Max delay | Capped | P1 |
| BND-046 | Cache TTL at expiry | Exactly TTL | Cache miss | P1 |
| BND-047 | Cache TTL 1s before expiry | TTL-1 | Cache hit | P1 |
| BND-048 | Rate limit window boundary | At window end | Reset | P1 |
| BND-049 | Concurrent request at limit | At concurrency limit | All succeed | P1 |
| BND-050 | Concurrent request 1 over limit | Over limit | One queued or rejected | P1 |

### 3.5 Unicode & Encoding (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-051 | Unicode in AI prompts | 中文, émoji | Handled | P0 |
| BND-052 | Unicode in AI responses | Response with 日本語 | Parsed | P0 |
| BND-053 | Emoji in opportunity | 🚀 | Preserved | P1 |
| BND-054 | RTL text in prompt | Arabic | Handled | P1 |
| BND-055 | Mixed script | Latin + Cyrillic | Handled | P1 |
| BND-056 | Zero-width characters | Unicode ZWJ | Sanitized or preserved | P1 |
| BND-057 | Null byte in text | \0 | Rejected | P0 |
| BND-058 | UTF-8 BOM | BOM in input | Handled | P1 |
| BND-059 | Invalid UTF-8 sequence | Malformed | Rejected or repaired | P1 |
| BND-060 | Homoglyph in search | lookalike chars | Normalized | P1 |

### 3.6 Numeric & Collection (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-061 | Page 0 for similar projects | page=0 | First page | P1 |
| BND-062 | Page 1 for similar projects | page=1 | Second page | P1 |
| BND-063 | PageSize 0 | size=0 | Default or error | P1 |
| BND-064 | PageSize 1 | size=1 | One result | P1 |
| BND-065 | PageSize max | size=100 | Capped | P1 |
| BND-066 | Total results 0 | No results | Empty, total=0 | P1 |
| BND-067 | Total results 1 | One result | total=1 | P1 |
| BND-068 | Embedding dimension 0 | dim=0 | Invalid | P0 |
| BND-069 | Embedding dimension min | dim=64 | Valid | P1 |
| BND-070 | Embedding dimension max | dim=4096 | Valid | P1 |
| BND-071 | Retry count 0 | No retries | Single attempt | P1 |
| BND-072 | Retry count max | Max retries | All attempted | P1 |
| BND-073 | Cache size at limit | Cache full | Eviction or reject | P1 |
| BND-074 | Vector store at capacity | Store full | Error or eviction | P1 |
| BND-075 | Batch size 0 | batch=0 | Error or skip | P1 |

### 3.7 Null & Empty (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-076 | Null opportunity | opportunity=null | ArgumentNullException | P0 |
| BND-077 | Null partner in context | partner=null | Handled | P1 |
| BND-078 | Null prompt template | template=null | ArgumentNullException | P0 |
| BND-079 | Empty prompt template | template="" | Handled | P1 |
| BND-080 | Null embedding vector | vector=null | Rejected | P0 |
| BND-081 | Empty embedding vector | vector=[] | Rejected | P0 |
| BND-082 | Null recommendation list | recommendations=null | Handled | P1 |
| BND-083 | Empty recommendation list | recommendations=[] | Empty list | P1 |
| BND-084 | Null similar projects | results=null | Empty | P1 |
| BND-085 | Null statement | statement=null | Handled | P1 |
| BND-086 | Empty statement | statement="" | Handled | P1 |
| BND-087 | Null cache value | value=null | Cache miss | P1 |
| BND-088 | Null entity type | EntityType=null | Error | P1 |
| BND-089 | Null entity ID | EntityId=0 | Error | P0 |
| BND-090 | Partial null in batch | Some null | Handled | P1 |

---

## §4 Functional Tests (90)

### 4.1 DST Pipeline (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-001 | Full DST pipeline end-to-end | Opportunity → DST | Recommendations | P0 |
| FUN-002 | DST deduplication of recommendations | Duplicate risks | Unique list | P0 |
| FUN-003 | DST with all 21 GO requirements | Complete opportunity | All validated | P0 |
| FUN-004 | DST risk keyword matching | Risk terms in text | Matched | P0 |
| FUN-005 | DST confidence threshold filter | Low confidence | Filtered | P0 |
| FUN-006 | DST recommendation to Risk | Create risk | Risk created | P0 |
| FUN-007 | DST skip existing risks | Risk exists | Not duplicated | P0 |
| FUN-008 | DST with partial opportunity | Incomplete | Missing requirements | P0 |
| FUN-009 | DST pipeline with retry | Transient fail | Retry succeeds | P1 |
| FUN-010 | DST pipeline with fallback | Primary fail | Fallback used | P1 |
| FUN-011 | DST audit trail | DST run | Audit logged | P1 |
| FUN-012 | DST recommendation ordering | Multiple | By confidence | P1 |
| FUN-013 | DST with soft-deleted partner | Partner deleted | Handled | P1 |
| FUN-014 | DST with inactive country | Country inactive | Handled | P1 |
| FUN-015 | DST pipeline transaction | Fail mid-pipeline | Rollback | P0 |

### 4.2 Cache Behavior (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-016 | Cache hit vs cache miss | Same request twice | Hit on second | P0 |
| FUN-017 | Cache miss populates | Miss | Fetch and store | P0 |
| FUN-018 | Cache TTL expiry | After TTL | Miss | P0 |
| FUN-019 | Cache invalidation on update | Opportunity updated | Invalidated | P0 |
| FUN-020 | Cache key uniqueness | Different params | Different keys | P0 |
| FUN-021 | Cache scope per user | User A vs B | Separate cache | P0 |
| FUN-022 | Cache scope per opportunity | Opp 1 vs 2 | Separate cache | P0 |
| FUN-023 | Cache stale data | Stale | Refresh | P1 |
| FUN-024 | Cache size limit | At limit | Eviction | P1 |
| FUN-025 | Cache distributed | Multi-instance | Consistent | P1 |
| FUN-026 | Cache fallback on failure | Cache down | No cache | P1 |
| FUN-027 | Cache compression | Large value | Compressed | P1 |
| FUN-028 | Cache serialization | Complex object | Round-trip | P1 |
| FUN-029 | Cache key collision | Same key | Overwrite | P1 |
| FUN-030 | Cache metrics | Cache ops | Metrics recorded | P1 |

### 4.3 Embedding Search (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-031 | Embedding search with threshold 0.5 | Similarity 0.6 | Returned | P0 |
| FUN-032 | Embedding search with threshold 0.9 | Similarity 0.8 | Not returned | P0 |
| FUN-033 | Embedding search pagination | Page 2 | Correct page | P0 |
| FUN-034 | Embedding search sort by similarity | Multiple | Descending | P0 |
| FUN-035 | Embedding search with filter | Entity type filter | Filtered | P0 |
| FUN-036 | Embedding search empty | No matches | Empty list | P0 |
| FUN-037 | Embedding search with limit | Limit 10 | Max 10 | P0 |
| FUN-038 | Embedding search exclude self | Same entity | Excluded | P1 |
| FUN-039 | Embedding search exclude deleted | Deleted entities | Excluded | P0 |
| FUN-040 | Embedding search with org filter | Org scope | Filtered | P0 |
| FUN-041 | Embedding search with date range | Date filter | Filtered | P1 |
| FUN-042 | Embedding search approximate | ANN | Approximate match | P1 |
| FUN-043 | Embedding search exact | Exact match | Exact | P1 |
| FUN-044 | Embedding search batch | Multiple queries | Batch result | P1 |
| FUN-045 | Embedding search fallback | Vector store down | Keyword fallback | P1 |

### 4.4 Prompt & Context (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-046 | AI prompt placeholder resolution | {opportunity} | Replaced | P0 |
| FUN-047 | AI prompt placeholder with missing data | {unknown} | Default or error | P0 |
| FUN-048 | AI prompt multiple placeholders | {a}, {b}, {c} | All replaced | P0 |
| FUN-049 | AI prompt nested placeholder | {opportunity.name} | Resolved | P1 |
| FUN-050 | AI prompt conditional section | {#if x} | Conditional output | P1 |
| FUN-051 | AI prompt localization | Locale | Localized | P1 |
| FUN-052 | AI context building | Opportunity + partner | Build | P0 |
| FUN-053 | AI context with null optional | Optional null | Handled | P1 |
| FUN-054 | AI context truncation | Too long | Truncated | P1 |
| FUN-055 | AI context sanitization | XSS | Sanitized | P0 |
| FUN-056 | AI prompt version | Version | Correct version | P1 |
| FUN-057 | AI prompt fallback | Primary fail | Fallback | P1 |
| FUN-058 | AI prompt validation | Invalid template | Error | P1 |
| FUN-059 | AI prompt caching | Same prompt | Cached | P1 |
| FUN-060 | AI prompt audit | Prompt used | Logged | P1 |

### 4.5 Concurrency & Retry (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-061 | Concurrent AI requests for same opportunity | 2 requests | Both succeed or dedupe | P0 |
| FUN-062 | Rate limit recovery | After limit | Retry succeeds | P0 |
| FUN-063 | Retry behavior on 503 | 503 | Retry with backoff | P0 |
| FUN-064 | Retry behavior on timeout | Timeout | Retry | P0 |
| FUN-065 | Retry exhausted | All fail | Final error | P0 |
| FUN-066 | Circuit breaker open | Failures | Circuit open | P1 |
| FUN-067 | Circuit breaker half-open | After timeout | Test request | P1 |
| FUN-068 | Circuit breaker close | Success | Circuit closed | P1 |
| FUN-069 | Concurrent embedding generation | 10 parallel | All succeed | P1 |
| FUN-070 | Concurrent cache access | 10 parallel | Consistent | P1 |
| FUN-071 | Concurrent DST requests | 5 parallel | All succeed | P1 |
| FUN-072 | Concurrent statement generation | 2 parallel | One or both | P1 |
| FUN-073 | Idempotency | Same request twice | Same result | P1 |
| FUN-074 | Request deduplication | Duplicate in flight | Dedupe | P1 |
| FUN-075 | Backoff jitter | Retry | Jitter applied | P1 |

### 4.6 Business Rules (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-076 | DST only for valid workflow stage | Draft | Allowed | P0 |
| FUN-077 | Statement generation permission | CanEdit | Required | P0 |
| FUN-078 | Risk recommendation permission | CanCreate | Required | P0 |
| FUN-079 | Embedding permission | CanView | Required | P0 |
| FUN-080 | Similar projects org scope | Org filter | Enforced | P0 |
| FUN-081 | DST recommendation limit | Max 50 | Capped | P1 |
| FUN-082 | Statement max length | Very long | Truncated | P1 |
| FUN-083 | Risk recommendation validation | Invalid risk | Rejected | P0 |
| FUN-084 | AI response validation | Invalid structure | Rejected | P0 |
| FUN-085 | Embedding entity association | Entity must exist | Validated | P0 |
| FUN-086 | Prompt entity type match | Wrong entity | Rejected | P1 |
| FUN-087 | DST opportunity lock | During DST | Optional lock | P1 |
| FUN-088 | Statement overwrite permission | Overwrite | CanEdit | P1 |
| FUN-089 | Similar projects exclusion | Same opportunity | Excluded | P1 |
| FUN-090 | AI audit logging | All AI calls | Logged | P0 |

---

## §5 Integration Tests (90)

### 5.1 GeminiManager → AiContextualService → Vector Store (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-001 | GeminiManager calls AiContextualService | Full flow | Context built | P0 |
| INT-002 | AiContextualService calls vector store | Search | Results | P0 |
| INT-003 | Vector store returns to AiContextualService | Response | Parsed | P0 |
| INT-004 | AiContextualService returns to GeminiManager | Response | Used | P0 |
| INT-005 | Vector store failure propagates | Store down | Error to caller | P0 |
| INT-006 | AiContextualService timeout | Slow | Timeout | P0 |
| INT-007 | GeminiManager retry | Transient fail | Retry | P0 |
| INT-008 | Embedding storage in vector store | Store | Stored | P0 |
| INT-009 | Vector search with filters | Filter | Filtered results | P0 |
| INT-010 | Context building with multiple sources | Multi | Combined | P1 |
| INT-011 | Vector store connection pooling | Concurrent | Pooled | P1 |
| INT-012 | AiContextualService cache | Cache | Hit/miss | P1 |
| INT-013 | GeminiManager fallback | Primary fail | Fallback | P1 |
| INT-014 | End-to-end similar projects | Full | Results | P0 |
| INT-015 | End-to-end DST | Full | Recommendations | P0 |

### 5.2 OpportunityController → GeminiManager → AI API (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-016 | Controller receives request | Request | Validated | P0 |
| INT-017 | Controller calls GeminiManager | Call | Success | P0 |
| INT-018 | GeminiManager calls AI API | API | Response | P0 |
| INT-019 | AI API response to GeminiManager | Response | Parsed | P0 |
| INT-020 | GeminiManager response to Controller | Response | Returned | P0 |
| INT-021 | Controller returns to client | Response | 200 OK | P0 |
| INT-022 | Controller timeout | Slow | 504 or timeout | P0 |
| INT-023 | Controller permission check | No perm | 403 | P0 |
| INT-024 | Controller validation | Invalid | 400 | P0 |
| INT-025 | Controller error handling | Error | 500 or appropriate | P0 |
| INT-026 | Controller audit | Request | Logged | P1 |
| INT-027 | Controller rate limit | Over limit | 429 | P1 |
| INT-028 | Controller async | Async | Non-blocking | P0 |
| INT-029 | Controller cancellation | Cancel | Cancelled | P1 |
| INT-030 | Full pipeline DST | End-to-end | Success | P0 |

### 5.3 Risk Recommendation → RiskManager CRUD (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-031 | DST recommendation to RiskManager.Create | Create | Risk created | P0 |
| INT-032 | RiskManager.Create validation | Invalid | Rejected | P0 |
| INT-033 | RiskManager.Create permission | No perm | 403 | P0 |
| INT-034 | RiskManager.Update from DST | Update | Updated | P0 |
| INT-035 | RiskManager.Delete | Delete | Soft deleted | P0 |
| INT-036 | RiskManager.GetById | Get | Risk returned | P0 |
| INT-037 | RiskManager list by opportunity | List | Filtered | P0 |
| INT-038 | DST deduplication before create | Duplicate | Not created | P0 |
| INT-039 | Risk recommendation transaction | Fail | Rollback | P0 |
| INT-040 | Risk recommendation audit | Create | Audit | P0 |
| INT-041 | Risk recommendation mapping | DST → Risk | Mapped | P0 |
| INT-042 | Risk recommendation batch | Multiple | All created | P1 |
| INT-043 | Risk recommendation conflict | Concurrent | Handled | P1 |
| INT-044 | Risk recommendation orphan | Opportunity deleted | Handled | P1 |
| INT-045 | Risk recommendation workflow | Workflow state | Validated | P1 |

### 5.4 Statement Generation → Opportunity Update (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-046 | Statement generation to Opportunity | Generate | Statement | P0 |
| INT-047 | Opportunity update with statement | Update | Saved | P0 |
| INT-048 | Opportunity update validation | Invalid | Rejected | P0 |
| INT-049 | Opportunity update permission | No perm | 403 | P0 |
| INT-050 | Opportunity update workflow | Wrong state | Rejected | P0 |
| INT-051 | Statement generation mapping | AI → Model | Mapped | P0 |
| INT-052 | Statement generation sanitization | XSS | Sanitized | P0 |
| INT-053 | Statement generation length | Long | Truncated or rejected | P0 |
| INT-054 | Transaction rollback on failure | Update fail | Rollback | P0 |
| INT-055 | Statement generation audit | Update | Audit | P0 |
| INT-056 | Statement overwrite | Overwrite | Updated | P1 |
| INT-057 | Statement partial update | Partial | Merged | P1 |
| INT-058 | Statement generation concurrent | Concurrent | One or both | P1 |
| INT-059 | Statement generation notification | Update | Notification | P1 |
| INT-060 | Statement generation cache | Cached | Invalidated | P1 |

### 5.5 Embedding Generation → EntityEmbeddings Storage (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-061 | Embedding generation | Generate | Vector | P0 |
| INT-062 | EntityEmbeddings save | Save | Stored | P0 |
| INT-063 | EntityEmbeddings get by entity | Get | Retrieved | P0 |
| INT-064 | EntityEmbeddings update | Update | Updated | P0 |
| INT-065 | EntityEmbeddings delete | Delete | Soft deleted | P0 |
| INT-066 | Embedding entity association | EntityId | Linked | P0 |
| INT-067 | Embedding entity type | EntityType | Stored | P0 |
| INT-068 | Embedding version | Version | Stored | P1 |
| INT-069 | Embedding on entity create | Create | Generated | P0 |
| INT-070 | Embedding on entity update | Update | Regenerated | P0 |
| INT-071 | Embedding on entity delete | Delete | Cleanup or keep | P1 |
| INT-072 | Embedding batch save | Batch | All saved | P1 |
| INT-073 | Embedding search integration | Search | From store | P0 |
| INT-074 | Embedding dimension validation | Wrong dim | Rejected | P0 |
| INT-075 | Embedding storage failure | DB fail | Error | P0 |

### 5.6 Cross-Service Integration (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-076 | Gmail → AI context | Email | Context | P1 |
| INT-077 | Document → AI extraction | Document | Extracted | P1 |
| INT-078 | Partner → AI context | Partner | In context | P0 |
| INT-079 | Contact → AI context | Contact | In context | P1 |
| INT-080 | Workflow → AI notification | Transition | Notification | P1 |
| INT-081 | Search → AI embedding | Search | Embedding used | P0 |
| INT-082 | Dashboard → AI summary | Dashboard | Summary | P1 |
| INT-083 | Export → AI formatting | Export | Formatted | P1 |
| INT-084 | Import → AI validation | Import | Validated | P1 |
| INT-085 | User preference → AI | Preference | Applied | P1 |
| INT-086 | Permission → AI scope | Permission | Filtered | P0 |
| INT-087 | Org hierarchy → AI | Org | In context | P1 |
| INT-088 | Country → AI context | Country | In context | P1 |
| INT-089 | Config → AI prompt | Config | Prompt | P1 |
| INT-090 | Audit → AI calls | Audit | Logged | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| CON-001 | 2 users DST same opportunity | Concurrent | Both succeed or one | P0 |
| CON-002 | 10 users AI requests | Concurrent | All succeed or rate limit | P0 |
| CON-003 | 2 users statement generation same opp | Concurrent | One or both | P0 |
| CON-004 | Concurrent embedding generation | 5 parallel | All succeed | P0 |
| CON-005 | Concurrent cache read/write | Read during write | Consistent | P0 |
| CON-006 | Concurrent vector search | 10 parallel | All succeed | P0 |
| CON-007 | DST during opportunity update | Concurrent | Handled | P0 |
| CON-008 | Statement gen during workflow transition | Concurrent | Handled | P0 |
| CON-009 | Risk create during DST | Concurrent | Handled | P0 |
| CON-010 | Embedding save during entity update | Concurrent | Handled | P0 |
| CON-011 | 50 concurrent AI requests | 50 parallel | Rate limit or succeed | P1 |
| CON-012 | Cache invalidation during read | Invalidate | Consistent | P1 |
| CON-013 | Circuit breaker with concurrent | Multiple | Circuit open | P1 |
| CON-014 | Retry with concurrent | Concurrent retries | No duplicate | P1 |
| CON-015 | Vector store connection pool | Many concurrent | Pooled | P1 |
| CON-016 | Prompt resolution concurrent | 5 parallel | All succeed | P1 |
| CON-017 | Similar projects concurrent | 5 parallel | All succeed | P1 |
| CON-018 | Batch embedding concurrent | 2 batches | Both succeed | P1 |
| CON-019 | Cache key collision concurrent | Same key | One wins | P1 |
| CON-020 | Transaction isolation | Parallel transactions | Isolated | P0 |
| CON-021 | Deadlock | Circular wait | Timeout or avoid | P1 |
| CON-022 | Connection exhaustion | Many connections | Limit | P1 |
| CON-023 | Memory under concurrent load | 100 concurrent | No OOM | P1 |
| CON-024 | Idempotency | Same request 5x | Same result | P1 |
| CON-025 | Request deduplication | Duplicate in flight | Dedupe | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected | Priority |
|----|-----------|----------|-------|----------|----------|
| UNT-001 | Prompt placeholder regex | Parsing | "{var}" | Match | P1 |
| UNT-002 | Confidence score clamp | Calculation | 1.5 | 1.0 | P1 |
| UNT-003 | Similarity threshold | Calculation | 0.7 | Filter | P1 |
| UNT-004 | Cache key generation | Hash | Params | Key | P1 |
| UNT-005 | Embedding dimension | Validation | 768 | Valid | P1 |
| UNT-006 | JSON response parse | Parsing | Valid JSON | Object | P1 |
| UNT-007 | Risk keyword match | Matching | "risk" in text | True | P1 |
| UNT-008 | Statement truncate | Formatting | Long text | Truncated | P1 |
| UNT-009 | Context sanitization | Sanitization | XSS | Sanitized | P1 |
| UNT-010 | Retry count | Calculation | Retries | Count | P1 |
| UNT-011 | Backoff delay | Calculation | Attempt 3 | Delay | P1 |
| UNT-012 | Token count estimate | Estimation | Text | Count | P1 |
| UNT-013 | Vector normalize | Math | Vector | Normalized | P1 |
| UNT-014 | Similarity cosine | Math | 2 vectors | Score | P1 |
| UNT-015 | Placeholder default | Resolution | Missing | Default | P1 |
| UNT-016 | Entity type mapping | Mapping | EntityType | String | P1 |
| UNT-017 | Error message format | Formatting | Error | Message | P1 |
| UNT-018 | Cache TTL calculation | Calculation | TTL | Seconds | P1 |
| UNT-019 | Batch size | Validation | 10 | Valid | P1 |
| UNT-020 | Pagination | Calculation | Page, size | Skip, take | P1 |
| UNT-021 | Deduplication | Logic | Duplicates | Unique | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|-----------|-----------|----------|
| PRF-001 | DST single request | DST | < 5 s | P0 |
| PRF-002 | Similar projects search | Search | < 2 s | P0 |
| PRF-003 | Statement generation | Generate | < 10 s | P0 |
| PRF-004 | Embedding generation | Generate | < 1 s | P0 |
| PRF-005 | Vector search | Search | < 500 ms | P0 |
| PRF-006 | Cache hit | Cache | < 10 ms | P1 |
| PRF-007 | Cache miss | Cache | < 100 ms | P1 |
| PRF-008 | Prompt resolution | Resolve | < 50 ms | P1 |
| PRF-009 | Context building | Build | < 200 ms | P1 |
| PRF-010 | 10 DST requests | 10 parallel | < 30 s | P1 |
| PRF-011 | 100 embedding generations | Batch | < 60 s | P1 |
| PRF-012 | Vector search 1000 | Search | < 2 s | P1 |
| PRF-013 | Memory DST | Single | < 50 MB | P2 |
| PRF-014 | Memory embedding batch | 100 | < 200 MB | P2 |
| PRF-015 | No N+1 in embedding | Query | Single query | P0 |
| PRF-016 | Connection pool | 20 concurrent | No exhaustion | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | 10 DST requests/min | 10/min | 10 min | 99% success | P1 |
| LDT-002 | 50 AI requests/min | 50/min | 10 min | 99% success | P1 |
| LDT-003 | 100 embedding/min | 100/min | 5 min | 99% success | P1 |
| LDT-004 | Spike 50 DST | 0→50→0 | 2 min | Graceful | P1 |
| LDT-005 | Spike 100 AI | 0→100→0 | 1 min | Rate limit or succeed | P2 |
| LDT-006 | Stress DST | Ramp to fail | Until fail | Document limit | P2 |
| LDT-007 | Stress embedding | Ramp | Until fail | Document limit | P2 |
| LDT-008 | Stress vector search | 50 concurrent | 5 min | No errors | P2 |
| LDT-009 | Recovery after spike | Spike then normal | 5 min | Recovery | P1 |
| LDT-010 | Recovery after stress | Stress then stop | 10 min | Full recovery | P2 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
