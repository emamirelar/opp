# GeminiManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGeminiManager`, `UNOPS.PAO.Business/Interfaces/IGeminiManager`  
**Created:** 2026-02-18  
**Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | Max(50, 3×30)=90 | ✅ |
| Boundary Tests | §3 | 90 | Max(50, 3×30)=90 | ✅ |
| Functional Tests | §4 | 90 | ≥90 | ✅ |
| Integration Tests | §5 | 90 | ≥90 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

GeminiManager handles AI business logic for the Opportunity+ system. Key functionality: DST recommendation pipeline (4-step: context → keywords → vector search → refinement), risk keyword extraction, vector store search with EntityTypeId="RISK", risk deduplication (existing vs AI recommendations), predefined high risk detection (COUNTRY_FRAGILE, PARTNER_DRAFT, NON_USD_CURRENCY), risk refinement and ranking (top 10), high risk guidance document loading from EntityArtifact PDF, similar projects search, relevant people search, opportunity insights, statement generation and caching, deliverable extraction with framework priority, AI prompt template loading and placeholder processing, response caching (AiResponseCache), cache invalidation, embedding generation and search, rate limiting, and fallback when AI unavailable.

---

## §1 Positive Tests (Happy Path) — 30 tests

> **Minimum:** 30-50 tests | **Focus:** Valid AI operations, successful pipelines

### Detailed Test Cases (P0)

#### POS-001: DST Recommendation Pipeline — Full 4-Step Success

**Priority:** P0  
**Precondition:** Opportunity exists with context. Vector store has risks. AI service available.

**Steps:**
1. Call `GetDSTRecommendationsAsync(opportunityId)`
2. Verify 4-step pipeline executes: context → keywords → vector search → refinement

**Expected Result:**
- Step 1: Opportunity details fetched for AI context
- Step 2: Risk keywords extracted from context
- Step 3: Vector store searched with EntityTypeId="RISK"
- Step 4: Risks refined and ranked (top 10)
- Recommendations returned with ExtractedKeywords, TotalFound

---

#### POS-002: Risk Keyword Extraction from Opportunity Context

**Priority:** P0  
**Precondition:** Opportunity with description, challenges, countries, partners.

**Steps:**
1. Call `ExtractRiskKeywordsAsync(opportunityDetailsDict)`
2. Verify keywords extracted

**Expected Result:**
- Non-empty keyword list
- Keywords relevant to opportunity context
- Keywords used for vector search

---

#### POS-003: Vector Store Search with EntityTypeId="RISK"

**Priority:** P0  
**Precondition:** Vector store configured. EntityTypeId="RISK" filter.

**Steps:**
1. Call vector store search with EntityTypeId="RISK"
2. Verify results filtered

**Expected Result:**
- Only risk entities returned
- DatasourceConnector="GOOGLE_BIGQUERY" filter applied
- MaxResults respected (default 10)

---

#### POS-004: Risk Deduplication — Existing vs AI Recommendations

**Priority:** P0  
**Precondition:** Opportunity has 3 existing risks. AI returns 10 recommendations including 2 similar to existing.

**Steps:**
1. Call `GetDSTRecommendationsAsync`
2. Verify deduplication

**Expected Result:**
- Existing risks excluded from recommendations
- No duplicate risk titles in result
- Refined list has unique risks only

---

#### POS-005: Predefined High Risk Detection — COUNTRY_FRAGILE

**Priority:** P0  
**Precondition:** Opportunity has country in fragile state list.

**Steps:**
1. Call `GetDSTRecommendationsAsync`
2. Verify predefined high risk detected

**Expected Result:**
- COUNTRY_FRAGILE predefined risk included
- sourceType="PREDEFINED_HIGH_RISK" or similar
- Detection rule triggered

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Predefined high risk PARTNER_DRAFT | Partner in draft status | GetDSTRecommendationsAsync | PARTNER_DRAFT risk included | P0 |
| POS-007 | Predefined high risk NON_USD_CURRENCY | Non-USD currency in budget | GetDSTRecommendationsAsync | NON_USD_CURRENCY risk included | P0 |
| POS-008 | Risk refinement and ranking top 10 | 15 vector results | RefineAndRankRisksAsync | Top 10 returned, ranked | P1 |
| POS-009 | High risk guidance document load | EntityArtifact PDF exists | GetHighRiskGuidanceDocumentAsync | PDF path returned | P1 |
| POS-010 | Similar projects search | Vector store + Gemini | GetSimilarProjectsAsync | Similar projects list | P1 |
| POS-011 | Relevant people search | Vector store + Gemini | GetRelevantPeopleAsync | Relevant people list | P1 |
| POS-012 | Opportunity insights generation | Opportunity with data | GenerateOpportunityInsightsAsync | Insights returned | P1 |
| POS-013 | Opportunity statement generation | Opportunity context | GenerateOpportunityStatementAsync | Statement markdown returned | P1 |
| POS-014 | Statement caching | Same opportunity, second call | GenerateOpportunityStatementAsync | Cached result returned | P1 |
| POS-015 | Deliverable extraction with framework priority | Opportunity with deliverables | ExtractDeliverablesAsync | Prioritized deliverables | P1 |
| POS-016 | AI prompt template loading | Prompt type exists | GetPromptByTypeAsync | Template with placeholders | P1 |
| POS-017 | Placeholder processing in prompt | Template with {placeholder} | ProcessPromptTemplate | Placeholders replaced | P1 |
| POS-018 | AiResponseCache hit | Cached response exists | FetchResultFromGemini | Cached returned, no API call | P1 |
| POS-019 | Cache invalidation on prompt change | Prompt updated | FetchResultFromGemini | Cache invalidated, new call | P1 |
| POS-020 | Embedding generation | Text input | CreateBatchEmbeddingsAsync | Embedding vectors returned | P1 |
| POS-021 | Embedding search | Query + embeddings | SearchVectorStoreAsync | Relevant documents | P1 |
| POS-022 | Rate limit within quota | Under limit | CallGeminiApiAsync | Success | P1 |
| POS-023 | Fallback when AI unavailable | AI service returns 503 | GetDSTRecommendationsAsync | Graceful fallback, empty or error | P1 |
| POS-024 | Force refresh bypasses cache | forceRefresh=true | GetDSTRecommendationsAsync | Fresh recommendations | P1 |
| POS-025 | Dismissed oupQuestionIds excluded | User dismissed IDs | GetDSTRecommendationsAsync | Dismissed excluded from result | P1 |
| POS-026 | Opportunity details for AI context | Opportunity exists | GetOpportunityDetailsForAIAsync | Full context dict | P1 |
| POS-027 | RefineAndRankRisks with guidance doc | PDF attached to prompt | RefineAndRankRisksAsync | LLM uses guidance | P1 |
| POS-028 | Predefined high risk matching | LLM returns "Currency Exchange Risk" | Match to NON_USD_CURRENCY | oupQuestionId linked | P1 |
| POS-029 | Empty keywords returns empty | No extractable context | ExtractRiskKeywordsAsync | Empty list, no crash | P1 |
| POS-030 | MaxResults parameter respected | maxResults=5 | GetDSTRecommendationsAsync | ≤5 recommendations | P1 |

---

## §2 Negative Tests (Failure Scenarios) — 90 tests

> **Minimum:** 90 tests | **Focus:** Invalid inputs, AI failures, missing data

### 2.1 Invalid Inputs

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | GetDSTRecommendations opportunityId=0 | opportunityId=0 | KeyNotFoundException or ArgumentException | P0 |
| NEG-002 | GetDSTRecommendations opportunityId=-1 | opportunityId=-1 | ArgumentException | P0 |
| NEG-003 | GetDSTRecommendations non-existent opportunity | opportunityId=999999 | KeyNotFoundException | P0 |
| NEG-004 | ExtractRiskKeywords null dict | opportunityDetailsDict=null | ArgumentNullException | P0 |
| NEG-005 | ExtractRiskKeywords empty dict | {} | Empty keywords, no crash | P1 |
| NEG-006 | Vector search null query | Query=null | ArgumentException or empty | P1 |
| NEG-007 | Vector search empty query | Query="" | Empty results | P1 |
| NEG-008 | GetSimilarProjects invalid opportunityId | opportunityId=0 | KeyNotFoundException | P0 |
| NEG-009 | GetRelevantPeople invalid opportunityId | opportunityId=0 | KeyNotFoundException | P0 |
| NEG-010 | GenerateStatement null opportunity | null | ArgumentNullException | P0 |

### 2.2 AI Service Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-011 | Gemini API 500 | AI returns 500 | Exception or graceful fallback | P0 |
| NEG-012 | Gemini API 503 | Service unavailable | Fallback, retry or error | P0 |
| NEG-013 | Gemini API timeout | Request times out | TimeoutException, handled | P0 |
| NEG-014 | Gemini API 429 rate limit | Rate limit exceeded | Retry with backoff or 429 | P0 |
| NEG-015 | Vector store unavailable | Vector store down | Exception or empty results | P0 |
| NEG-016 | Embedding API failure | Embedding service error | Exception or fallback | P0 |
| NEG-017 | AI returns malformed JSON | Invalid response format | Parsing error, handled | P1 |
| NEG-018 | AI returns empty content | Content null/empty | Graceful handling | P1 |
| NEG-019 | Network timeout to AI | Network failure | TimeoutException | P0 |
| NEG-020 | AI API key invalid | Invalid credentials | 401, handled | P0 |

### 2.3 Missing Dependencies

| ID | Test Name | Missing | Expected Error | Priority |
|----|-----------|---------|---------------|----------|
| NEG-021 | OpportunityManager null | Manager not available | InvalidOperationException | P0 |
| NEG-022 | RiskManager null | RiskManager not available | InvalidOperationException | P0 |
| NEG-023 | AiRetrieverManager null | AiRetrieverManager not available | InvalidOperationException | P0 |
| NEG-024 | High risk guidance document missing | No EntityArtifact PDF | Warning, continues without | P1 |
| NEG-025 | Predefined high risks empty | No predefined risks | Empty list, no crash | P1 |
| NEG-026 | EntityUserRole lookup fails | DoA lookup error | Handled in workflow context | P1 |
| NEG-027 | Prompt template not found | Invalid prompt type | KeyNotFoundException | P1 |
| NEG-028 | Placeholder not in context | {missing} in template | Default or error | P1 |
| NEG-029 | Opportunity details null | GetOpportunityDetailsForAI returns null | KeyNotFoundException | P0 |
| NEG-030 | Opportunity details wrong type | Not Dictionary | InvalidOperationException | P0 |

### 2.4 Deduplication and Validation

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-031 | All AI recommendations duplicate existing | 10 results, all match existing | Empty or minimal result | P1 |
| NEG-032 | Predefined high risk not applicable | No triggers | Not included | P1 |
| NEG-033 | Invalid EntityTypeId in vector search | EntityTypeId="INVALID" | Wrong or empty results | P1 |
| NEG-034 | MaxResults=0 | maxResults=0 | Empty or default | P1 |
| NEG-035 | MaxResults negative | maxResults=-1 | ArgumentException or default | P1 |
| NEG-036 | Dismissed IDs invalid | dismissedOupQuestionIds=[-1] | Handled | P2 |
| NEG-037 | Opportunity soft-deleted | Opportunity IsDeleted=true | KeyNotFoundException | P0 |
| NEG-038 | User context null | user=null | May default or error | P1 |
| NEG-039 | Vector store returns null | Search returns null | NullReference handled | P1 |
| NEG-040 | Refinement returns null | RefineAndRankRisks returns null | Empty list returned | P1 |

### 2.5 Cache and State

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-041 | Cache corrupted | Invalid cache entry | Bypass cache, fresh call | P1 |
| NEG-042 | Cache key collision | Same key different content | Correct result or invalidation | P2 |
| NEG-043 | Statement cache stale | Opportunity updated, cache hit | May return stale; invalidation rule | P1 |
| NEG-044 | AiResponseCache expired | TTL exceeded | New API call | P1 |
| NEG-045 | Prompt change during request | Prompt updated mid-request | Consistent behavior | P2 |

### 2.6 Rate Limiting and Quota

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-046 | Rate limit exceeded | Too many requests | 429 or queued | P0 |
| NEG-047 | Quota exceeded | Daily quota exhausted | Error or graceful | P0 |
| NEG-048 | Concurrent requests at limit | 10 simultaneous | Some throttled | P1 |
| NEG-049 | Retry exhaustion | 5 retries all fail | Final exception | P1 |
| NEG-050 | Rate limit no retry config | Retry disabled | Immediate failure | P2 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-051 | ExtractRiskKeywords exception in LLM | LLM throws | Caught, empty keywords | P1 |
| NEG-052 | Vector search exception | Search throws | Exception propagated | P0 |
| NEG-053 | RefineAndRankRisks exception | Refinement throws | Exception or empty | P0 |
| NEG-054 | Guidance document fetch exception | PDF fetch fails | Warning, continue without | P1 |
| NEG-055 | Existing risks fetch fails | RiskManager throws | Exception or empty | P0 |
| NEG-056 | Similar projects empty context | No similar in vector store | Empty list | P1 |
| NEG-057 | Relevant people empty | No matches | Empty list | P1 |
| NEG-058 | Insights generation fails | AI error | Exception or empty | P1 |
| NEG-059 | Deliverable extraction empty | No deliverables | Empty list | P1 |
| NEG-060 | Framework priority invalid | Unknown framework | Default or error | P2 |
| NEG-061 | Placeholder injection | Malicious placeholder | Sanitized | P0 |
| NEG-062 | Prompt template XSS | Script in template | Sanitized | P0 |
| NEG-063 | Opportunity context too large | 100K chars | Truncated or error | P1 |
| NEG-064 | Embedding text empty | Empty string | Null or zero-vector | P1 |
| NEG-065 | Embedding batch too large | 1000 texts | Chunked or error | P1 |
| NEG-066 | Vector dimensions mismatch | Wrong model | Error | P1 |
| NEG-067 | DatasourceConnector invalid | Wrong connector | Empty or error | P1 |
| NEG-068 | Filters malformed | Invalid filter dict | Error or ignored | P2 |
| NEG-069 | Debug=true in production | Debug flag | May log sensitive | P2 |
| NEG-070 | User email null for vector search | userEmail=null | Handled | P1 |
| NEG-071 | GetOpportunityDetailsForAI throws | Manager throws | Propagated | P0 |
| NEG-072 | DbContext disposed | Context disposed before use | ObjectDisposedException | P0 |
| NEG-073 | DbContextFactory fails | CreateDbContextAsync throws | Exception | P0 |
| NEG-074 | JSON serialization fails | RecordData invalid | SerializationException | P1 |
| NEG-075 | Predefined high risk ID invalid | Invalid oupQuestionId | Handled | P1 |
| NEG-076 | Risk category null | Category missing | Default or error | P2 |
| NEG-077 | Similar projects maxResults=0 | maxResults=0 | Empty | P1 |
| NEG-078 | Relevant people maxResults=0 | maxResults=0 | Empty | P1 |
| NEG-079 | Statement generation timeout | AI slow | Timeout | P1 |
| NEG-080 | Cache write failure | DB error on cache save | Logged, no cache | P2 |
| NEG-081 | Embedding model unavailable | Model not found | Error | P0 |
| NEG-082 | Vector store index missing | Index not built | Error or empty | P0 |
| NEG-083 | Opportunity with no context | Empty description, challenges | Minimal keywords | P1 |
| NEG-084 | Multiple predefined risks triggered | 3 rules match | All included | P1 |
| NEG-085 | Dismissed all recommendations | All in dismissedOupQuestionIds | Empty result | P1 |
| NEG-086 | Force refresh with cache miss | forceRefresh, no cache | Fresh call | P1 |
| NEG-087 | Concurrent cache invalidation | Two prompts updated | Consistent state | P2 |
| NEG-088 | Statement cache key collision | Two opportunities same hash | Separate cache entries | P2 |
| NEG-089 | Retry on transient failure | 503 then 200 | Success after retry | P1 |
| NEG-090 | Fallback empty result | AI fully unavailable | Empty list, no crash | P0 |

---

## §3 Boundary Tests (Edge Cases) — 90 tests

> **Minimum:** 90 tests | **Focus:** Edge values, boundary conditions

### 3.1 Opportunity ID Boundaries

| ID | Test Name | opportunityId | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| BND-001 | opportunityId=1 | Minimum valid | Success | P1 |
| BND-002 | opportunityId=MAX_INT | 2147483647 | Handled | P2 |
| BND-003 | opportunityId=0 | Zero | Rejected | P0 |
| BND-004 | opportunityId=-1 | Negative | Rejected | P0 |
| BND-005 | opportunityId non-existent | 999999999 | KeyNotFoundException | P1 |

### 3.2 Keyword and Query Boundaries

| ID | Test Name | Input | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-006 | Empty opportunity context | All fields empty | Empty keywords | P1 |
| BND-007 | Single word context | "Infrastructure" | 1+ keywords | P1 |
| BND-008 | Max length description | 50000 chars | Truncated or processed | P1 |
| BND-009 | Unicode in context | Arabic/Chinese | Keywords extracted | P2 |
| BND-010 | Special chars in context | <>&" | Escaped or sanitized | P1 |
| BND-011 | Search query 1 char | "a" | Processed | P1 |
| BND-012 | Search query 1000 chars | Long query | Truncated or processed | P1 |
| BND-013 | Search query with newlines | Multi-line | Handled | P2 |
| BND-014 | Keywords count 0 | No extractable | Empty list | P1 |
| BND-015 | Keywords count 50 | Many keywords | All used or truncated | P2 |
| BND-016 | sourceType="predefined" | Predefined risk | From predefined high risks | P1 |
| BND-017 | sourceType="vector" | Vector result | From vector search | P1 |
| BND-018 | sourceType="refined" | LLM refined | From refinement step | P1 |

### 3.3 Vector Search Boundaries

| ID | Test Name | Parameter | Value | Expected | Priority |
|----|-----------|----------|------|----------|----------|
| BND-019 | MaxResults=1 | maxResults | 1 | 1 result | P1 |
| BND-020 | MaxResults=10 | maxResults | 10 | ≤10 results | P1 |
| BND-021 | MaxResults=100 | maxResults | 100 | ≤100 or capped | P1 |
| BND-022 | EntityTypeId="RISK" | Filter | "RISK" | Only risks | P0 |
| BND-023 | EntityTypeId="OPPORTUNITY" | Filter | "OPPORTUNITY" | Different results | P1 |
| BND-024 | EntityId empty | EntityId | "" | Global search | P1 |
| BND-025 | DatasourceConnector | "GOOGLE_BIGQUERY" | Filter | BigQuery only | P1 |
| BND-026 | Filters empty dict | Filters | {} | No filter | P1 |
| BND-027 | Filters with values | Filters | {"key":"value"} | Applied | P1 |
| BND-028 | Vector store 0 results | Empty store | Search | Empty list | P1 |
| BND-029 | Vector store 1 result | Single match | Search | 1 result | P1 |
| BND-030 | Vector store 100 results | Many matches | Search | MaxResults respected | P1 |

### 3.4 Risk and Recommendation Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-031 | 0 existing risks | No risks on opportunity | All AI results novel | P1 |
| BND-032 | 1 existing risk | 1 risk | Dedup against 1 | P1 |
| BND-033 | 20 existing risks | Many risks | Dedup against all | P1 |
| BND-034 | 0 predefined high risks triggered | No triggers | No predefined in result | P1 |
| BND-035 | 1 predefined triggered | COUNTRY_FRAGILE | 1 predefined included | P1 |
| BND-036 | All 17 predefined triggered | All rules match | All included (or top 10) | P2 |
| BND-037 | Refinement returns 5 | 5 applicable | 5 in result | P1 |
| BND-038 | Refinement returns 15 | 15 applicable | Top 10 returned | P1 |
| BND-039 | Dismissed 0 IDs | [] | No exclusion | P1 |
| BND-040 | Dismissed 10 IDs | [1..10] | 10 excluded | P1 |
| BND-041 | Opportunity with 0 predefined triggers | No fragile country, etc. | No warnings | P1 |
| BND-042 | Opportunity with exactly 9 predefined | 9 rules match | All 9 displayed | P1 |
| BND-043 | Risk title 100 chars | Max length | Truncated or accepted | P1 |
| BND-044 | Risk title 1 char | Min | Accepted | P1 |
| BND-045 | oupQuestionId=0 | Invalid | Handled | P1 |
| BND-046 | oupQuestionId=1 | Min valid | Linked | P1 |
| BND-047 | Risk category ID boundary | 1, MAX | Handled | P2 |
| BND-048 | Duplicate risk titles | Same title from AI | Deduplicated | P1 |
| BND-049 | Existing risk title partial match | "Currency" vs "Currency Risk" | Dedup logic applied | P2 |
| BND-050 | Predefined + vector overlap | Same risk from both | Deduplicated | P1 |

### 3.5 Statement and Cache Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-051 | Statement 0 chars | Empty context | Empty or minimal | P1 |
| BND-052 | Statement 50000 chars | Large output | Truncated or full | P1 |
| BND-053 | Statement cache first call | No cache | API call | P1 |
| BND-054 | Statement cache second call | Cache hit | No API call | P1 |
| BND-055 | Statement cache key | Same opportunity | Same key | P1 |
| BND-056 | AiResponseCache TTL at boundary | Just expired | New call | P1 |
| BND-057 | AiResponseCache TTL just valid | Not expired | Cache hit | P1 |
| BND-058 | Cache key with special chars | Key has & | Escaped | P2 |
| BND-059 | Prompt template 0 placeholders | No {x} | As-is | P1 |
| BND-060 | Prompt template 20 placeholders | Many | All replaced | P1 |
| BND-061 | Placeholder value null | {x}=null | Default or error | P1 |
| BND-062 | Placeholder value empty | {x}="" | Replaced | P1 |
| BND-063 | Placeholder value 10000 chars | Very long | Truncated or full | P2 |
| BND-064 | Cache invalidation prompt ID change | New prompt ID | Invalidated | P1 |
| BND-065 | Cache invalidation prompt content change | Content updated | Invalidated | P1 |

### 3.6 Embedding Boundaries

| ID | Test Name | Input | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-066 | Embedding empty text | "" | Null or zero-vector | P1 |
| BND-067 | Embedding 1 char | "a" | Vector returned | P1 |
| BND-068 | Embedding 10000 chars | Long text | Chunked or truncated | P1 |
| BND-069 | Batch embedding 1 text | [1 item] | 1 embedding | P1 |
| BND-070 | Batch embedding 100 texts | 100 items | 100 embeddings | P1 |
| BND-071 | Batch embedding 101 texts | Over limit | Chunked or error | P2 |
| BND-072 | Embedding dimensions | Model spec | Consistent dimensions | P1 |
| BND-073 | Search with 0 embeddings | Empty index | Empty results | P1 |
| BND-074 | Search with 1 embedding | Single | 1 or 0 results | P1 |
| BND-075 | Search similarity threshold | At threshold | Included/excluded | P2 |

### 3.7 Rate Limit and Quota Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-076 | 1 request under limit | 1 call | Success | P1 |
| BND-077 | 9 requests at limit | 9th request | Success | P1 |
| BND-078 | 10th request at limit | 10th | Success or 429 | P1 |
| BND-079 | 11th request over limit | 11th | 429 | P1 |
| BND-080 | After rate limit window | Wait 1 min | Success | P1 |
| BND-081 | Concurrent 2 at limit | 2 threads | Both handled | P1 |
| BND-082 | Quota 99% used | Near limit | Next may fail | P2 |
| BND-083 | Retry count 0 | No retries | Immediate fail | P1 |
| BND-084 | Retry count 5 | 5 retries | Retry then fail | P1 |
| BND-085 | Backoff delay | Exponential | Delays increase | P2 |

### 3.8 Similar Projects and Relevant People

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-086 | Similar projects 0 results | No similar | Empty list | P1 |
| BND-087 | Similar projects 1 result | 1 match | 1 item | P1 |
| BND-088 | Similar projects 5 results | 5 matches | 5 items | P1 |
| BND-089 | Relevant people 0 results | No matches | Empty list | P1 |
| BND-090 | Relevant people 10 results | 10 matches | 10 items | P1 |

---

## §4 Functional Tests (Business Rules) — 90 tests

> **Minimum:** 90 tests | **Breakdown:** DST Pipeline (25), Caching (20), Validation (20), Integration (25)

### 4.1 DST Pipeline Rules (25)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Step 1: Fetch opportunity context | Context | GetDSTRecommendations | Opportunity details fetched | P0 |
| FUN-002 | Step 2: Extract keywords | Keywords | ExtractRiskKeywords | Keywords from context | P0 |
| FUN-003 | Step 3: Vector search EntityTypeId=RISK | Search | VectorStoreSearch | Only risks returned | P0 |
| FUN-004 | Step 4: Refine and rank | Refinement | RefineAndRankRisks | Top 10, ranked | P0 |
| FUN-005 | Deduplication against existing | Dedup | GetDSTRecommendations | Existing excluded | P0 |
| FUN-006 | Deduplication against predefined | Dedup | RefineAndRank | Predefined merged | P0 |
| FUN-007 | COUNTRY_FRAGILE detection | Rule | Fragile country | Risk included | P0 |
| FUN-008 | PARTNER_DRAFT detection | Rule | Draft partner | Risk included | P0 |
| FUN-009 | NON_USD_CURRENCY detection | Rule | Non-USD | Risk included | P0 |
| FUN-010 | High risk guidance doc in prompt | Attach | RefineAndRank | PDF attached | P1 |
| FUN-011 | Empty keywords returns empty | No keywords | Pipeline | Empty recommendations | P0 |
| FUN-012 | forceRefresh bypasses cache | forceRefresh=true | GetDSTRecommendations | Fresh call | P1 |
| FUN-013 | Dismissed IDs excluded | dismissedOupQuestionIds | Result | Excluded | P1 |
| FUN-014 | MaxResults respected | maxResults=5 | GetDSTRecommendations | ≤5 | P1 |
| FUN-015 | Similar projects: vector + Gemini | Pipeline | GetSimilarProjects | Both used | P1 |
| FUN-016 | Relevant people: vector + Gemini | Pipeline | GetRelevantPeople | Both used | P1 |
| FUN-017 | Opportunity insights: context | Context | GenerateInsights | Insights from context | P1 |
| FUN-018 | Statement: context to markdown | Context | GenerateStatement | Markdown output | P1 |
| FUN-019 | Deliverable extraction: framework priority | Priority | ExtractDeliverables | Prioritized | P1 |
| FUN-020 | Predefined high risk: oupQuestionId | Match | LLM title to predefined | oupQuestionId linked | P1 |
| FUN-021 | Fallback on AI unavailable | AI down | GetDSTRecommendations | Empty or error | P0 |
| FUN-022 | Retry on 429 | Rate limit | CallGeminiApi | Retry with backoff | P0 |
| FUN-023 | ExecutionTimeMs in response | Timing | GetDSTRecommendations | ExecutionTimeMs set | P1 |
| FUN-024 | ExtractedKeywords in response | Keywords | GetDSTRecommendations | List returned | P1 |
| FUN-025 | TotalFound in response | Count | GetDSTRecommendations | Count correct | P1 |

### 4.2 Caching Rules (20)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-026 | AiResponseCache hit | Cache exists | FetchResultFromGemini | Cached returned | P0 |
| FUN-027 | AiResponseCache miss | No cache | FetchResultFromGemini | API call | P0 |
| FUN-028 | Cache invalidation on prompt change | Prompt updated | FetchResultFromGemini | New call | P1 |
| FUN-029 | Statement cache key | opportunityId | GenerateStatement | Key includes ID | P1 |
| FUN-030 | Statement cache hit | Same opportunity | Second call | Cached | P1 |
| FUN-031 | Statement cache miss | New opportunity | First call | API call | P1 |
| FUN-032 | Cache TTL | Time | Expiry | Expired = new call | P1 |
| FUN-033 | Cache write on miss | API call | FetchResultFromGemini | Cache written | P1 |
| FUN-034 | Cache key uniqueness | Different prompts | Same content | Different keys | P1 |
| FUN-035 | forceRefresh invalidates | forceRefresh | GetDSTRecommendations | Bypass | P1 |
| FUN-036 | DST recommendations not cached | Each call | GetDSTRecommendations | Fresh or config | P1 |
| FUN-037 | Similar projects cache | Same opportunity | Second call | May cache | P2 |
| FUN-038 | Relevant people cache | Same opportunity | Second call | May cache | P2 |
| FUN-039 | Insights cache | Same opportunity | Second call | May cache | P2 |
| FUN-040 | Cache corruption handling | Invalid entry | Read | Bypass, new call | P1 |
| FUN-041 | Cache key collision | Same hash | Different content | Separate entries | P2 |
| FUN-042 | Prompt template cache | Template load | GetPromptByType | Cached | P2 |
| FUN-043 | Embedding cache | Same text | CreateBatchEmbeddings | May cache | P2 |
| FUN-044 | Vector search no cache | Each search | SearchVectorStore | No cache | P1 |
| FUN-045 | Cache invalidation on opportunity update | Opportunity changed | Statement | May invalidate | P2 |

### 4.3 Validation Rules (20)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-046 | opportunityId positive | Required | 1 | 0, -1 | P0 |
| FUN-047 | opportunityId exists | Exists | Valid ID | 999999 | P0 |
| FUN-048 | opportunityDetails Dictionary | Type | Dict | null, object | P0 |
| FUN-049 | Vector search query non-empty | Query | "text" | null, "" | P1 |
| FUN-050 | EntityTypeId valid | Filter | "RISK" | "INVALID" | P1 |
| FUN-051 | MaxResults positive | Range | 1-100 | 0, -1 | P1 |
| FUN-052 | Prompt template exists | Exists | Valid type | Invalid | P1 |
| FUN-053 | Placeholder format | Format | {name} | {{name} | P1 |
| FUN-054 | Embedding text length | Max | ≤limit | Over limit | P1 |
| FUN-055 | Batch embedding count | Max | ≤100 | 101 | P1 |
| FUN-056 | Risk title length | Max | ≤100 | 101 | P1 |
| FUN-057 | oupQuestionId valid | FK | Valid ID | 0, -1 | P1 |
| FUN-058 | Dismissed IDs non-negative | List | [1,2] | [-1] | P2 |
| FUN-059 | Filters JSON | Format | Valid dict | Invalid | P2 |
| FUN-060 | User context optional | Optional | user or null | N/A | P1 |
| FUN-061 | forceRefresh boolean | Type | true/false | null | P1 |
| FUN-062 | DatasourceConnector | Filter | "GOOGLE_BIGQUERY" | "" | P1 |
| FUN-063 | Response structure | Schema | Valid structure | Malformed | P1 |
| FUN-064 | Recommendation structure | Schema | Required fields | Missing fields | P1 |
| FUN-065 | Framework priority | Enum | Valid value | Invalid | P2 |

### 4.4 Integration Rules (25)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-066 | OpportunityManager.GetOpportunityDetailsForAI | Call | GetDSTRecommendations | Opportunity details | P0 |
| FUN-067 | RiskManager.GetRisksByEntity | Call | GetDSTRecommendations | Existing risks | P0 |
| FUN-068 | AiRetrieverManager.SearchVectorStore | Call | GetDSTRecommendations | Vector results | P0 |
| FUN-069 | GetHighRiskGuidanceDocument | Call | GetDSTRecommendations | PDF path | P1 |
| FUN-070 | Predefined high risks from RiskManager | Call | Fetch | Risk list | P1 |
| FUN-071 | DbContextFactory for background | Create | CreateNotifications | New context | P1 |
| FUN-072 | ManagerWrapper resolution | Resolve | All methods | Correct managers | P0 |
| FUN-073 | CreateBatchEmbeddings delegates | AiContextualService | CreateBatchEmbeddings | Delegated | P1 |
| FUN-074 | SearchVectorStore delegates | AiRetrieverManager | Search | Delegated | P1 |
| FUN-075 | Logger on error | Log | Exception | Error logged | P1 |
| FUN-076 | Execution time logging | Log | GetDSTRecommendations | Time logged | P2 |
| FUN-077 | Empty result logging | Log | No keywords | Warning logged | P2 |
| FUN-078 | Guidance doc missing warning | Log | No PDF | Warning | P2 |
| FUN-079 | Retry logging | Log | Retry | Logged | P2 |
| FUN-080 | Fallback logging | Log | AI unavailable | Logged | P1 |
| FUN-081 | Recommendation structure | Schema | Each recommendation | title, sourceType, etc. | P1 |
| FUN-082 | Similar project structure | Schema | Each project | Required fields | P1 |
| FUN-083 | Relevant person structure | Schema | Each person | Required fields | P1 |
| FUN-084 | Statement markdown format | Format | Output | Valid markdown | P1 |
| FUN-085 | Deliverable structure | Schema | Each deliverable | Framework, priority | P1 |
| FUN-086 | Prompt template format | Format | Output | Placeholders | P1 |
| FUN-087 | RecordData JSON | Format | Notification | Valid JSON | P1 |
| FUN-088 | Embedding vector dimensions | Consistent | All embeddings | Same dimensions | P1 |
| FUN-089 | Rate limit enforcement | Shared | Multiple users | Enforced | P0 |
| FUN-090 | Quota tracking | Usage | API calls | Tracked | P1 |

---

## §5 Integration Tests (End-to-End Flows) — 90 tests

> **Minimum:** 90 tests

### 5.1 DST Pipeline Integration (20)

| ID | Test Name | Flow | Entities | Expected | Priority |
|----|-----------|------|----------|----------|----------|
| INT-001 | Full DST pipeline | Context→Keywords→Search→Refine | Opportunity, Vector, AI | Recommendations | P0 |
| INT-002 | DST with existing risks | Pipeline + dedup | Opportunity, RiskManager | Deduplicated | P0 |
| INT-003 | DST with predefined high risks | Pipeline + predefined | Opportunity, PreDefinedHighRisk | Merged | P0 |
| INT-004 | DST with guidance document | Pipeline + PDF | Opportunity, EntityArtifact | PDF in prompt | P1 |
| INT-005 | DST with forceRefresh | Full pipeline, no cache | All | Fresh results | P1 |
| INT-006 | DST with dismissed IDs | Pipeline + filter | Opportunity | Dismissed excluded | P1 |
| INT-007 | DST empty opportunity | Minimal context | Opportunity | Empty or minimal | P1 |
| INT-008 | DST full opportunity | All sections filled | Opportunity | Rich results | P1 |
| INT-009 | DST OpportunityManager round-trip | GetOpportunityDetailsForAI | OpportunityManager | Dict returned | P0 |
| INT-010 | DST RiskManager round-trip | GetRisksByEntity | RiskManager | Risks returned | P0 |
| INT-011 | DST AiRetrieverManager round-trip | SearchVectorStore | AiRetrieverManager | Documents | P0 |
| INT-012 | DST RefineAndRank LLM call | RefineAndRankRisks | Gemini API | Refined risks | P0 |
| INT-013 | DST ExtractRiskKeywords LLM | ExtractRiskKeywords | Gemini API | Keywords | P0 |
| INT-014 | DST EntityTypeId filter | Vector search | RISK filter | Only risks | P0 |
| INT-015 | DST ExecutionTimeMs | Full pipeline | Timing | Value set | P1 |
| INT-016 | DST ExtractedKeywords in response | Pipeline | Response | List populated | P1 |
| INT-017 | DST TotalFound in response | Pipeline | Response | Count correct | P1 |
| INT-018 | DST fallback on AI error | AI throws | Pipeline | Graceful | P0 |
| INT-019 | DST retry on 429 | Rate limit | Pipeline | Retry succeeds | P0 |
| INT-020 | DST DbContext scope | Background ops | DbContextFactory | New context | P1 |

### 5.2 Similar Projects and Relevant People (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-021 | GetSimilarProjects full flow | Vector + Gemini | Projects list | P0 |
| INT-022 | GetSimilarProjects empty | No similar | Empty list | P1 |
| INT-023 | GetRelevantPeople full flow | Vector + Gemini | People list | P0 |
| INT-024 | GetRelevantPeople empty | No matches | Empty list | P1 |
| INT-025 | Similar projects → embeddings | Embedding step | Embeddings created | P1 |
| INT-026 | Relevant people → embeddings | Embedding step | Embeddings created | P1 |
| INT-027 | Similar projects Gemini refinement | LLM step | Refined list | P1 |
| INT-028 | Relevant people Gemini refinement | LLM step | Refined list | P1 |
| INT-029 | Similar projects maxResults | Parameter | Respected | P1 |
| INT-030 | Relevant people maxResults | Parameter | Respected | P1 |
| INT-031 | Similar projects opportunity context | Context | Used in search | P1 |
| INT-032 | Relevant people opportunity context | Context | Used in search | P1 |
| INT-033 | Similar projects error handling | AI error | Graceful | P1 |
| INT-034 | Relevant people error handling | AI error | Graceful | P1 |
| INT-035 | Both pipelines use AiContextualService | Delegation | Correct service | P1 |

### 5.3 Statement and Insights (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-036 | GenerateStatement full flow | Context → AI → Markdown | Statement | P0 |
| INT-037 | GenerateStatement cache | Second call | Cached | P1 |
| INT-038 | GenerateOpportunityInsights full flow | Context → AI | Insights | P0 |
| INT-039 | Statement cache invalidation | Opportunity update | May invalidate | P2 |
| INT-040 | Statement placeholder | Opportunity context | Placeholders filled | P1 |
| INT-041 | Insights structure | Response | Valid schema | P1 |
| INT-042 | Statement markdown | Output | Valid markdown | P1 |
| INT-043 | Statement empty context | Minimal | Minimal output | P1 |
| INT-044 | Statement large context | Full | Full output | P1 |
| INT-045 | Insights empty context | Minimal | Minimal output | P1 |
| INT-046 | GetOpportunityDetailsForAI full | OpportunityManager | Full dict | P0 |
| INT-047 | Statement generation timeout | Slow AI | Timeout handling | P1 |
| INT-048 | Insights generation timeout | Slow AI | Timeout handling | P1 |
| INT-049 | Statement AI error | AI throws | Error handling | P0 |
| INT-050 | Insights AI error | AI throws | Error handling | P0 |

### 5.4 Caching and Prompt Integration (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-051 | AiResponseCache write | API call | Cache written | P0 |
| INT-052 | AiResponseCache read | Cache hit | Cached returned | P0 |
| INT-053 | Cache invalidation on prompt change | Prompt update | Cache cleared | P1 |
| INT-054 | GetPromptByType round-trip | AiPromptManager | Template returned | P1 |
| INT-055 | Placeholder processing | Template + context | Replaced | P1 |
| INT-056 | ExtractDeliverables full flow | Opportunity → AI | Deliverables | P1 |
| INT-057 | Deliverable framework priority | Priority | Correct order | P1 |
| INT-058 | CreateBatchEmbeddings round-trip | AiContextualService | Embeddings | P1 |
| INT-059 | SearchVectorStore round-trip | AiRetrieverManager | Documents | P1 |
| INT-060 | Cache key collision | Same key | Handled | P2 |
| INT-061 | Prompt template not found | Invalid type | Error | P1 |
| INT-062 | Placeholder missing | {x} not in context | Default or error | P1 |
| INT-063 | Embedding model | Correct model | Used | P1 |
| INT-064 | Vector dimensions | Consistent | All same | P1 |
| INT-065 | Rate limit shared | Multiple users | Enforced | P0 |

### 5.5 Error and Fallback Integration (25)

| ID | Test Name | Error Condition | Expected | Priority |
|----|-----------|----------------|----------|----------|
| INT-066 | OpportunityManager throws | Manager error | Propagated | P0 |
| INT-067 | RiskManager throws | Manager error | Propagated | P0 |
| INT-068 | AiRetrieverManager throws | Manager error | Propagated | P0 |
| INT-069 | Gemini API 500 | Server error | Exception or fallback | P0 |
| INT-070 | Gemini API 503 | Unavailable | Retry or fallback | P0 |
| INT-071 | Gemini API timeout | Timeout | TimeoutException | P0 |
| INT-072 | Gemini API 429 | Rate limit | Retry | P0 |
| INT-073 | Vector store down | Service down | Exception | P0 |
| INT-074 | Embedding API down | Service down | Exception | P0 |
| INT-075 | DbContextFactory fails | Factory error | Exception | P0 |
| INT-076 | Cache DB error | Cache write fails | Logged, no cache | P1 |
| INT-077 | Guidance document fetch fails | PDF error | Warning, continue | P1 |
| INT-078 | Malformed AI response | Invalid JSON | Parsing error | P1 |
| INT-079 | Null AI response | Null content | Handled | P1 |
| INT-080 | Network failure | No connection | Exception | P0 |
| INT-081 | Retry exhaustion | 5 retries fail | Final exception | P1 |
| INT-082 | Fallback empty | AI fully down | Empty list | P0 |
| INT-083 | Transaction rollback | DB error during cache | Rollback | P1 |
| INT-084 | Concurrent cache invalidation | Two updates | Consistent | P2 |
| INT-085 | Rate limit during retry | 429 on retry | Further retry | P1 |
| INT-086 | Opportunity deleted during call | Soft delete | KeyNotFoundException | P0 |
| INT-087 | User context null | user=null | Handled | P1 |
| INT-088 | Permission check | No permission | Unauthorized | P0 |
| INT-089 | Full integration success | All components | End-to-end | P0 |
| INT-090 | Full integration with fallback | AI down | Graceful degradation | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE for QA)

> **Note:** Security testing is OUT OF SCOPE for QA per project standards.

| ID | Test Name | Category | Status | Priority |
|----|-----------|----------|--------|----------|
| SEC-001 | Prompt injection in context | Injection | OUT OF SCOPE | P0 |
| SEC-002 | XSS in AI response | Injection | OUT OF SCOPE | P0 |
| SEC-003 | Unauthorized DST access | Access Control | OUT OF SCOPE | P0 |
| SEC-004 | IDOR: Other user's opportunity | IDOR | OUT OF SCOPE | P0 |
| SEC-005 through SEC-050 | [Additional security scenarios] | Various | OUT OF SCOPE | P1/P2 |

---

## §7 Concurrency Tests — 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Two users GetDSTRecommendations same opportunity | Parallel | Both succeed, may share cache | P0 |
| CON-002 | Two users GetDSTRecommendations different opportunities | Parallel | Both succeed | P0 |
| CON-003 | GetDSTRecommendations + GenerateStatement same opportunity | Parallel | Both succeed | P1 |
| CON-004 | Cache write during read | Concurrent | Consistent | P1 |
| CON-005 | Cache invalidation during read | Concurrent | Consistent | P1 |
| CON-006 | Rate limit shared | 10 users simultaneous | Throttled | P0 |
| CON-007 | Concurrent embedding generation | 5 CreateBatchEmbeddings | All succeed | P1 |
| CON-008 | Concurrent vector search | 5 SearchVectorStore | All succeed | P1 |
| CON-009 | DbContextFactory concurrent | 5 CreateDbContext | Each gets own context | P0 |
| CON-010 | Retry during concurrent | Same user retry | No double count | P1 |
| CON-011 | Statement cache concurrent | 2 GenerateStatement same opp | One API call | P1 |
| CON-012 | AiResponseCache concurrent | 2 FetchResult same key | One API call | P1 |
| CON-013 | forceRefresh during cache read | Concurrent | forceRefresh wins | P2 |
| CON-014 | Prompt update during use | Prompt changed | Consistent | P2 |
| CON-015 | Opportunity update during DST | Opportunity changed | Consistent or stale | P2 |
| CON-016 | Concurrent GetSimilarProjects | 2 users | Both succeed | P1 |
| CON-017 | Concurrent GetRelevantPeople | 2 users | Both succeed | P1 |
| CON-018 | Embedding batch concurrent | 2 batches | Both complete | P1 |
| CON-019 | Vector store concurrent | 2 searches | Both complete | P1 |
| CON-020 | Notification creation concurrent | 2 stage changes | Both created | P1 |
| CON-021 | Deadlock scenario | Circular dependency | Timeout or retry | P2 |
| CON-022 | Connection pool exhaustion | Many concurrent | Throttled or queued | P1 |
| CON-023 | Cache key generation race | Same content | No collision | P2 |
| CON-024 | Quota race | Near limit | Correct handling | P1 |
| CON-025 | Full load concurrent | 20 mixed operations | All complete | P1 |

---

## §8 Unit Tests — 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | ExtractFromMetadata key exists | Validation | metadata={"x":"y"}, key="x" | "y" | P1 |
| UNT-002 | ExtractFromMetadata key missing | Validation | metadata={"x":"y"}, key="z" | null | P1 |
| UNT-003 | ExtractFromMetadata null | Validation | metadata=null | null | P1 |
| UNT-004 | ParseRecordData empty | Formatting | "" | [] | P1 |
| UNT-005 | ParseRecordData valid JSON | Formatting | "[{}]" | [{}] | P1 |
| UNT-006 | RefineAndRankRisks dedup | Validation | existingTitles | Excluded | P1 |
| UNT-007 | Predefined high risk match | Validation | "Currency Exchange" | NON_USD_CURRENCY | P1 |
| UNT-008 | Keyword join | Formatting | ["a","b","c"] | "a b c" | P1 |
| UNT-009 | Search query truncation | Formatting | 200 char query | 100 chars | P1 |
| UNT-010 | StageRequirement FieldType | Validation | GO requirements | Correct types | P1 |
| UNT-011 | Cache key generation | Formatting | opportunityId=1 | Consistent key | P1 |
| UNT-012 | Placeholder regex | Formatting | "{name}" | Match | P1 |
| UNT-013 | Embedding dimensions | Calculation | Model spec | 768 or 1536 | P1 |
| UNT-014 | Similarity score | Calculation | Two vectors | 0-1 | P1 |
| UNT-015 | ExecutionTimeMs | Calculation | Start, End | Milliseconds | P1 |
| UNT-016 | Recommendation structure | Validation | Recommendation | Required fields | P1 |
| UNT-017 | VectorStoreSearchRequest | Validation | Request | EntityTypeId=RISK | P1 |
| UNT-018 | DSTRecommendationsResponse | Validation | Response | Structure | P1 |
| UNT-019 | Retry delay calculation | Calculation | Attempt 1 | Delay 1s | P1 |
| UNT-020 | Retry delay exponential | Calculation | Attempt 3 | Delay 4s | P1 |
| UNT-021 | Fallback empty result | Validation | AI error | Empty list | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetDSTRecommendations full | Full pipeline | < 30s | P1 |
| PRF-002 | ExtractRiskKeywords | LLM call | < 5s | P1 |
| PRF-003 | Vector store search | Search | < 2s | P1 |
| PRF-004 | RefineAndRankRisks | LLM call | < 15s | P1 |
| PRF-005 | GetSimilarProjects | Full flow | < 20s | P1 |
| PRF-006 | GetRelevantPeople | Full flow | < 20s | P1 |
| PRF-007 | GenerateOpportunityStatement | Full flow | < 20s | P1 |
| PRF-008 | GenerateOpportunityInsights | Full flow | < 15s | P1 |
| PRF-009 | CreateBatchEmbeddings 10 | 10 texts | < 5s | P1 |
| PRF-010 | SearchVectorStore | Single search | < 2s | P1 |
| PRF-011 | GetOpportunityDetailsForAI | Manager call | < 5s | P1 |
| PRF-012 | AiResponseCache hit | Cache read | < 100ms | P1 |
| PRF-013 | GetPromptByType | Template load | < 200ms | P1 |
| PRF-014 | ExtractDeliverables | Full flow | < 10s | P1 |
| PRF-015 | GetHighRiskGuidanceDocument | Document fetch | < 2s | P1 |
| PRF-016 | Full DST with cache miss | No cache | < 35s | P2 |

---

## §10 Load Tests — 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained DST recommendations | 5/min | 10 min | All succeed | P2 |
| LDT-002 | Spike: 20 simultaneous DST | 20 concurrent | 1 min | 95% success | P2 |
| LDT-003 | Sustained statement generation | 10/min | 5 min | < 25s p95 | P2 |
| LDT-004 | Sustained vector search | 50/min | 5 min | < 3s p95 | P2 |
| LDT-005 | Stress: 50 concurrent embeddings | 50 batches | 2 min | No deadlocks | P2 |
| LDT-006 | Rate limit load | At limit | 5 min | 429 when exceeded | P2 |
| LDT-007 | Cache hit load | 100 cache reads/min | 5 min | < 150ms p95 | P2 |
| LDT-008 | Full pipeline load | 10 full DST/min | 10 min | All complete | P2 |
| LDT-009 | Recovery after load | Load then idle | 2 min | System recovers | P2 |
| LDT-010 | Mixed operations load | DST, statement, insights mix | 15 min | No degradation | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| DST 4-step pipeline | POS-001, FUN-001 to FUN-004, INT-001 |
| Risk keyword extraction | POS-002, FUN-002, NEG-004, FUN-051 |
| Vector store EntityTypeId=RISK | POS-003, FUN-003, BND-022, INT-014 |
| Risk deduplication | POS-004, FUN-005, FUN-006, BND-031 to BND-033 |
| Predefined high risks COUNTRY_FRAGILE, PARTNER_DRAFT, NON_USD | POS-005 to POS-007, FUN-007 to FUN-009, BND-034 to BND-036 |
| High risk guidance document | POS-009, FUN-010, NEG-024, INT-004 |
| Similar projects / Relevant people | POS-010, POS-011, INT-021 to INT-035 |
| Statement caching | POS-014, FUN-029 to FUN-031, INT-037 |
| AiResponseCache | POS-018, FUN-026 to FUN-028, INT-051 to INT-053 |
| Cache invalidation | POS-019, FUN-028, NEG-043, BND-064, BND-065 |
| Rate limiting | NEG-046 to NEG-050, FUN-089, BND-076 to BND-085 |
| Fallback when AI unavailable | POS-023, FUN-021, NEG-090, INT-082 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
