# AiContextualService — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Services/AiContextualService` (Unit Tests)  
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

AI contextual service unit tests cover context building, entity aggregation, and prompt templating. Tests include: context assembly, entity data aggregation, similarity search, embedding generation, prompt template rendering, variable substitution, cache management, and vector operations.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Build context | Entity exists | BuildContext | Context built |
| POS-002 | Aggregate entity data | Entity exists | AggregateEntity | Aggregated |
| POS-003 | Generate embedding | Valid text | GenerateEmbedding | Vector |
| POS-004 | Find similar | Query exists | FindSimilar | Matches |
| POS-005 | Render prompt template | Template exists | RenderTemplate | Rendered |
| POS-006 | Substitute variables | Vars exist | SubstituteVariables | Substituted |
| POS-007 | Get cached context | Context cached | GetContext | From cache |
| POS-008 | Invalidate cache | Cache has data | InvalidateCache | Invalidated |
| POS-009 | Get embedding dimension | Service up | GetDimension | Dimension |
| POS-010 | Batch embeddings | Multiple texts | GenerateBatch | Vectors |
| POS-011 | Filter by threshold | Matches exist | FindSimilar | Filtered |
| POS-012 | Limit results | Many matches | FindSimilar | Limited |
| POS-013 | Sort by similarity | Matches exist | FindSimilar | Sorted |
| POS-014 | Get entity summary | Entity exists | GetEntitySummary | Summary |
| POS-015 | Get related entities | Entity exists | GetRelatedEntities | Related |
| POS-016 | Audit CreatedBy | Create | Check audit | Set |
| POS-017 | Audit CreatedDate | Create | Check audit | UTC |
| POS-018 | Merge context | Multiple contexts | MergeContext | Merged |
| POS-019 | Truncate context | Long context | Truncate | Truncated |
| POS-020 | Encode context | Context exists | Encode | Encoded |
| POS-021 | Decode context | Encoded exists | Decode | Decoded |
| POS-022 | Get context size | Context exists | GetSize | Size |
| POS-023 | Validate template | Template exists | ValidateTemplate | Valid |
| POS-024 | Get required variables | Template exists | GetRequiredVars | Vars |
| POS-025 | Apply default values | Vars missing | ApplyDefaults | Applied |
| POS-026 | Escape special chars | Special chars | Escape | Escaped |
| POS-027 | Get entity type | Entity exists | GetEntityType | Type |
| POS-028 | Get entity fields | Entity exists | GetEntityFields | Fields |
| POS-029 | Format for AI | Data exists | FormatForAI | Formatted |
| POS-030 | Get token count | Text exists | GetTokenCount | Count |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Build context null entity | Entity=null | ArgumentNullException |
| NEG-002 | Build context invalid entity | EntityId=-1 | ArgumentException |
| NEG-003 | Generate embedding null text | Text=null | ArgumentNullException |
| NEG-004 | Generate embedding empty | Text="" | ValidationException |
| NEG-005 | Find similar null query | Query=null | ArgumentNullException |
| NEG-006 | Render template null | Template=null | ArgumentNullException |
| NEG-007 | Substitute variables null | Vars=null | ArgumentNullException |
| NEG-008 | Aggregate entity null | Entity=null | ArgumentNullException |
| NEG-009 | Invalid entity type | Type invalid | ArgumentException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | Build context unauthorized | Unauthorized | Forbidden |
| NEG-012 | Find similar unauthorized | Unauthorized | Forbidden |
| NEG-013 | Get embedding unauthorized | Unauthorized | Forbidden |
| NEG-014 | Invalidate cache unauthorized | Unauthorized | Forbidden |
| NEG-015 | Aggregate unauthorized | Unauthorized | Forbidden |
| NEG-016 | SQL injection in query | '; DROP | Rejected |
| NEG-017 | XSS in context | <script> | Escaped |
| NEG-018 | Path traversal | ../../../etc | Rejected |
| NEG-019 | Template injection | {{malicious}} | Sanitized |
| NEG-020 | Variable injection | Malicious var | Sanitized |
| NEG-021 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-022 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-023 | Connection timeout | DB unavailable | TimeoutException |
| NEG-024 | Embedding API error | API down | EmbeddingException |
| NEG-025 | Cache corrupted | Corrupt cache | Handle |
| NEG-026 | Template syntax invalid | Invalid syntax | ValidationException |
| NEG-027 | Missing required variable | Var missing | ValidationException |
| NEG-028 | Expired session | Expired token | Unauthorized |
| NEG-029 | Null user context | User=null | InvalidOperationException |
| NEG-030 | Entity not found | EntityId=99999 | KeyNotFoundException |
| NEG-031 | Invalid threshold | Threshold=-1 | ArgumentException |
| NEG-032 | Invalid limit | Limit=0 | ArgumentException |
| NEG-033 | Invalid page number | Page=0 | ArgumentException |
| NEG-034 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-035 | Filter malformed | Malformed filter | ArgumentException |
| NEG-036 | Child override throws | Child throws | Propagated |
| NEG-037 | Context too large | 100k tokens | ValidationException |
| NEG-038 | Embedding dimension mismatch | Wrong dim | ArgumentException |
| NEG-039 | Audit missing user | User=0 | InvalidOperationException |
| NEG-040 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-041 | Pagination overflow | Page too large | Empty or error |
| NEG-042 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-043 | Merge invalid contexts | Incompatible | ArgumentException |
| NEG-044 | Truncate invalid length | Length=-1 | ArgumentException |
| NEG-045 | Encode invalid | Encoding invalid | ArgumentException |
| NEG-046 | Decode invalid | Decoded invalid | ArgumentException |
| NEG-047 | GetEntitySummary invalid | Entity invalid | ArgumentException |
| NEG-048 | GetRelatedEntities invalid | Entity invalid | ArgumentException |
| NEG-049 | Chunk invalid size | Size=0 | ArgumentException |
| NEG-050 | Combine invalid chunks | Chunks invalid | ArgumentException |
| NEG-051 | ClearExpired invalid | Date invalid | ArgumentException |
| NEG-052 | GetMetadata invalid | Context invalid | ArgumentException |
| NEG-053 | Batch embeddings null | Texts=null | ArgumentNullException |
| NEG-054 | Batch embeddings empty | Texts=[] | ArgumentException |
| NEG-055 | Batch embeddings one invalid | One invalid | Partial or fail |
| NEG-056 | Cross-tenant access | Other tenant | Forbidden |
| NEG-057 | Invalid include path | Invalid include | ArgumentException |
| NEG-058 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-059 | Invalid enum value | Type invalid | ArgumentException |
| NEG-060 | Embedding API rate limit | Rate limit | RateLimitException |
| NEG-061 | Embedding API timeout | Timeout | TimeoutException |
| NEG-062 | Vector dimension wrong | Wrong dim | ArgumentException |
| NEG-063 | Similarity score invalid | Score invalid | ArgumentException |
| NEG-064 | Context format invalid | Format invalid | ArgumentException |
| NEG-065 | Token count overflow | Huge text | OverflowException |
| NEG-066 | GetTokenCount null | Text=null | ArgumentNullException |
| NEG-067 | FormatForAI null | Data=null | ArgumentNullException |
| NEG-068 | GetEntityFields invalid | Entity invalid | ArgumentException |
| NEG-069 | GetCacheStats null | Cache null | ArgumentNullException |
| NEG-070 | Merge context empty | Contexts=[] | ArgumentException |
| NEG-071 | BuildContext null entity type | EntityType=null | ArgumentNullException |
| NEG-072 | FindSimilar negative threshold | Threshold=-0.1 | ArgumentException |
| NEG-073 | FindSimilar threshold over 1 | Threshold=1.1 | ArgumentException |
| NEG-074 | RenderTemplate empty vars | Vars={} | ValidationException |
| NEG-075 | Truncate zero length | Length=0 | ArgumentException |
| NEG-076 | Encode null context | Context=null | ArgumentNullException |
| NEG-077 | Decode null input | Input=null | ArgumentNullException |
| NEG-078 | GetSize null context | Context=null | ArgumentNullException |
| NEG-079 | ValidateTemplate null | Template=null | ArgumentNullException |
| NEG-080 | GetRequiredVars invalid template | Template invalid | ArgumentException |
| NEG-081 | ApplyDefaults null template | Template=null | ArgumentNullException |
| NEG-082 | Escape null string | String=null | ArgumentNullException |
| NEG-083 | GetEntityType null entity | Entity=null | ArgumentNullException |
| NEG-084 | GetEntityFields null entity | Entity=null | ArgumentNullException |
| NEG-085 | Chunk null context | Context=null | ArgumentNullException |
| NEG-086 | CombineChunks null | Chunks=null | ArgumentNullException |
| NEG-087 | GetCacheStats invalid | Cache invalid | ArgumentException |
| NEG-088 | ClearExpired null date | Date=null | ArgumentNullException |
| NEG-089 | GetMetadata null context | Context=null | ArgumentNullException |
| NEG-090 | Entity ID zero | EntityId=0 | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Context at min | 1 token | Valid |
| BND-002 | Context at max | Max tokens | Valid |
| BND-003 | Context over max | Max+1 | Truncate or reject |
| BND-004 | Entity ID at Int32.MaxValue | Id=2147483647 | Handle |
| BND-005 | Entity ID at zero | Id=0 | Reject |
| BND-006 | Page size at min | PageSize=1 | Valid |
| BND-007 | Page size at max | PageSize=100 | Valid |
| BND-008 | Page size over max | PageSize=101 | Reject |
| BND-009 | Threshold at 0 | Threshold=0 | Valid |
| BND-010 | Threshold at 1 | Threshold=1 | Valid |
| BND-011 | Limit at 1 | Limit=1 | Valid |
| BND-012 | Limit at max | Limit=100 | Valid |
| BND-013 | Limit over max | Limit=101 | Reject |
| BND-014 | Embedding dimension | 768 or 1536 | Valid |
| BND-015 | Text at min | 1 char | Valid |
| BND-016 | Text at max | 8k chars | Valid |
| BND-017 | Text over max | 8k+1 | Reject |
| BND-018 | Unicode in context | Arabic/Chinese | Stored |
| BND-019 | Special chars in template | <>&"' | Escaped |
| BND-020 | Leading/trailing spaces | Text="  x  " | Trimmed |
| BND-021 | Empty matches | No matches | [] |
| BND-022 | Single match | 1 match | Valid |
| BND-023 | Many matches | 1000 matches | Valid |
| BND-024 | Date at min | Date=MinValue | Handle |
| BND-025 | Date at max | Date=MaxValue | Handle |
| BND-026 | DateTime UTC | UTC input | Stored |
| BND-027 | Template variables empty | No vars | Valid |
| BND-028 | Template variables max | 50 vars | Valid |
| BND-029 | Pagination last partial | Partial page | Correct |
| BND-030 | Pagination total | Total count | Accurate |
| BND-031 | Sort null handling | Nulls in data | Deterministic |
| BND-032 | Filter combination all | All filters | Correct |
| BND-033 | Type enum boundary | Last enum | Valid |
| BND-034 | Parent null for root | ParentId=null | Valid |
| BND-035 | Parent max int | ParentId=2147483647 | Handle |
| BND-036 | Soft delete boundary | DeletedDate set | Excluded |
| BND-037 | Include depth | Deep include | No explosion |
| BND-038 | Query timeout | Slow query | Timeout |
| BND-039 | Audit timestamp precision | Millisecond | Stored |
| BND-040 | Async cancellation | Cancel token | OperationCanceledException |
| BND-041 | Task timeout | Timeout | TimeoutException |
| BND-042 | Concurrent same second | Same timestamp | Deterministic |
| BND-043 | Cache TTL at min | 1 second | Valid |
| BND-044 | Cache TTL at max | 24 hours | Valid |
| BND-045 | Chunk size at min | 100 tokens | Valid |
| BND-046 | Chunk size at max | 4k tokens | Valid |
| BND-047 | Similarity score 0 | No similarity | 0 |
| BND-048 | Similarity score 1 | Exact match | 1 |
| BND-049 | Batch size at max | 100 texts | Valid |
| BND-050 | Batch size over max | 101 texts | Reject |
| BND-051 | Filter empty result | No match | Empty list |
| BND-052 | Sort empty | Empty list | No exception |
| BND-053 | Pagination empty | No data | Empty |
| BND-054 | GetEntitySummary empty | No data | Empty |
| BND-055 | GetRelatedEntities empty | No related | [] |
| BND-056 | Truncate at boundary | At boundary | Truncated |
| BND-057 | Merge at boundary | At boundary | Merged |
| BND-058 | GetTokenCount boundary | Boundary | Count |
| BND-059 | Chunk boundary | At boundary | Chunked |
| BND-060 | Combine boundary | At boundary | Combined |
| BND-061 | Cache hit boundary | At TTL | Hit or miss |
| BND-062 | Cache miss boundary | After TTL | Miss |
| BND-063 | Embedding empty batch | Batch=[] | [] |
| BND-064 | FindSimilar empty | No matches | [] |
| BND-065 | Variable substitution empty | Empty value | Replaced |
| BND-066 | Variable substitution long | Long value | Truncate |
| BND-067 | Default value boundary | Empty default | Applied |
| BND-068 | Escape boundary | All special | Escaped |
| BND-069 | Encode decode roundtrip | Encode, Decode | Same |
| BND-070 | Concurrent context build | Two build | Both valid |
| BND-071 | Context size exactly max | At max | Valid |
| BND-072 | Embedding vector length | 768 dim | Valid |
| BND-073 | Token count zero | Empty text | 0 |
| BND-074 | Token count max | Max text | Count |
| BND-075 | Merge two contexts | Two | Merged |
| BND-076 | Merge many contexts | Many | Merged |
| BND-077 | Chunk overlap zero | Overlap=0 | Valid |
| BND-078 | Chunk overlap max | Overlap=max | Valid |
| BND-079 | Similarity threshold exact | At threshold | Include |
| BND-080 | Similarity threshold below | Below | Exclude |
| BND-081 | Batch single text | 1 text | Valid |
| BND-082 | FormatForAI empty | Empty data | Formatted |
| BND-083 | GetEntityType boundary | Type | Valid |
| BND-084 | GetEntityFields empty | No fields | [] |
| BND-085 | GetEntityFields many | Many fields | All |
| BND-086 | Cache key collision | Collision | Handle |
| BND-087 | Invalidate partial | Partial | Invalidated |
| BND-088 | ClearExpired at boundary | At TTL | Cleared |
| BND-089 | GetMetadata empty | No metadata | Empty |
| BND-090 | Vector norm zero | Zero vector | Handle |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Entity required | Validation | BuildContext | Reject if null |
| FUN-002 | Text required | Validation | GenerateEmbedding | Reject if empty |
| FUN-003 | Query required | Validation | FindSimilar | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Template syntax | Constraint | RenderTemplate | Valid syntax |
| FUN-008 | Variable required | Constraint | SubstituteVariables | Reject missing |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Context size limit | Constraint | BuildContext | Reject over |
| FUN-017 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-018 | FindSimilar excludes deleted | Constraint | FindSimilar | Excludes deleted |
| FUN-019 | Cache key unique | Logic | Cache | Unique |
| FUN-020 | Cache TTL | Logic | Cache | TTL |
| FUN-021 | Context aggregation | Logic | AggregateEntity | Aggregated |
| FUN-022 | Similarity calculation | Logic | FindSimilar | Cosine |
| FUN-023 | Template variable replace | Logic | RenderTemplate | Replaced |
| FUN-024 | Default value apply | Logic | ApplyDefaults | Applied |
| FUN-025 | Truncate logic | Logic | Truncate | Truncated |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on build | Transaction | BuildContext | Atomic |
| FUN-031 | Transaction on cache | Transaction | Cache | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads entity | Data load | GetById include | Entity loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | Embedding dimension | Logic | GenerateEmbedding | Fixed |
| FUN-036 | Batch embedding | Logic | GenerateBatch | All |
| FUN-037 | Chunk overlap | Logic | Chunk | Overlap |
| FUN-038 | Combine order | Logic | CombineChunks | Ordered |
| FUN-039 | Token count | Logic | GetTokenCount | Count |
| FUN-040 | Format for AI | Logic | FormatForAI | Formatted |
| FUN-041 | Encode decode | Logic | Encode, Decode | Same |
| FUN-042 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-043 | Invalidate on update | Logic | Update | Invalidated |
| FUN-044 | Config context size | Config | BuildContext | Config |
| FUN-045 | Config embeddings | Config | GenerateEmbedding | Config |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Context caching | Performance | BuildContext | Cached |
| FUN-051 | Similarity threshold filter | Logic | FindSimilar | Filtered |
| FUN-052 | Result limit | Logic | FindSimilar | Limited |
| FUN-053 | Sort by score | Logic | FindSimilar | Sorted |
| FUN-054 | Entity summary format | Logic | GetEntitySummary | Formatted |
| FUN-055 | Related entities filter | Logic | GetRelatedEntities | Filtered |
| FUN-056 | Merge order | Logic | MergeContext | Order |
| FUN-057 | Truncate preserve | Logic | Truncate | Preserved |
| FUN-058 | Encode format | Logic | Encode | Format |
| FUN-059 | Decode format | Logic | Decode | Format |
| FUN-060 | Token count accuracy | Logic | GetTokenCount | Accurate |
| FUN-061 | Template validation | Logic | ValidateTemplate | Validated |
| FUN-062 | Required vars extraction | Logic | GetRequiredVars | Extracted |
| FUN-063 | Default precedence | Logic | ApplyDefaults | Precedence |
| FUN-064 | Escape all special | Logic | Escape | All |
| FUN-065 | Entity type resolution | Logic | GetEntityType | Resolved |
| FUN-066 | Entity fields filter | Logic | GetEntityFields | Filtered |
| FUN-067 | Format structure | Logic | FormatForAI | Structure |
| FUN-068 | Chunk boundary | Logic | Chunk | Boundary |
| FUN-069 | Combine boundary | Logic | CombineChunks | Boundary |
| FUN-070 | Cache eviction | Logic | Cache | Evicted |
| FUN-071 | ClearExpired logic | Logic | ClearExpired | Cleared |
| FUN-072 | GetMetadata structure | Logic | GetMetadata | Structure |
| FUN-073 | Batch order | Logic | GenerateBatch | Order |
| FUN-074 | Context scope | Logic | BuildContext | Scoped |
| FUN-075 | Entity scope | Logic | AggregateEntity | Scoped |
| FUN-076 | Permission scope | Authorization | Per entity | Check |
| FUN-077 | User context scope | Audit | Per user | Set |
| FUN-078 | Timestamp UTC | Audit | All | UTC |
| FUN-079 | Deleted exclude FindSimilar | Constraint | FindSimilar | Excluded |
| FUN-080 | Deleted exclude Aggregate | Constraint | Aggregate | Excluded |
| FUN-081 | Pagination consistency | Calculation | Page | Consistent |
| FUN-082 | Sort multi-column | Calculation | Sort | Multi |
| FUN-083 | Filter OR logic | Filter | OR filter | Match |
| FUN-084 | Transaction on aggregate | Transaction | Aggregate | Atomic |
| FUN-085 | Include selective | Data load | Include | Selective |
| FUN-086 | Config batch size | Config | GenerateBatch | Config |
| FUN-087 | Config chunk size | Config | Chunk | Config |
| FUN-088 | Config similarity | Config | FindSimilar | Config |
| FUN-089 | Context lifecycle | Workflow | Build to invalidate | Complete |
| FUN-090 | Embedding lifecycle | Workflow | Generate to use | Complete |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Build context full flow | BuildContext | Entity | Context |
| INT-002 | Generate embedding full flow | GenerateEmbedding | Text | Vector |
| INT-003 | Find similar full flow | FindSimilar | Query | Matches |
| INT-004 | Render template full flow | RenderTemplate | Template | Rendered |
| INT-005 | Aggregate entity full flow | AggregateEntity | Entity | Aggregated |
| INT-006 | Get with entity | GetById | Context, Entity | Entity loaded |
| INT-007 | List with filter and sort | List | Context | Filtered, sorted |
| INT-008 | Cache then get | Cache, Get | Context | From cache |
| INT-009 | Invalidate then get | Invalidate, Get | Context | From DB |
| INT-010 | Context-Entity relationship | Relationship | Context, Entity | FK valid |
| INT-011 | Cascade soft delete | Relationship | Entity deleted | Config |
| INT-012 | Orphan handling | Relationship | Entity deleted | Retained |
| INT-013 | Embedding API integration | Integration | API | Embeddings |
| INT-014 | DB error handling | Error | DB down | Graceful |
| INT-015 | API error handling | Error | API down | Graceful |
| INT-016 | Timeout handling | Error | Slow | Timeout |
| INT-017 | Constraint violation | Error | FK violation | Clear error |
| INT-018 | Permission service integration | Integration | Permission | Check |
| INT-019 | User resolver integration | Integration | User | Resolved |
| INT-020 | Audit context integration | Integration | Audit | Context |
| INT-021 | Logger integration | Integration | Log | Logged |
| INT-022 | Cache service integration | Integration | Cache | Cache |
| INT-023 | Mapper integration | Integration | Map | Correct |
| INT-024 | Repository integration | Integration | Repository | CRUD |
| INT-025 | DbContext integration | Integration | DbContext | Scoped |
| INT-026 | Transaction scope | Integration | Transaction | Atomic |
| INT-027 | Full context build | Scenario | BuildContext | Complete |
| INT-028 | Similarity search | Scenario | FindSimilar | Matches |
| INT-029 | Template rendering | Scenario | RenderTemplate | Rendered |
| INT-030 | Concurrent build | Scenario | Parallel | All succeed |
| INT-031 | Cache invalidation | Scenario | Update, Get | Fresh |
| INT-032 | Entity aggregation | Scenario | AggregateEntity | Aggregated |
| INT-033 | Chunk combine | Scenario | Chunk, Combine | Complete |
| INT-034 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-035 | Filter by type | Scenario | Filter | Filtered |
| INT-036 | Batch embeddings | Scenario | GenerateBatch | All |
| INT-037 | Variable substitution | Scenario | SubstituteVariables | Substituted |
| INT-038 | Truncate context | Scenario | Truncate | Truncated |
| INT-039 | Merge context | Scenario | MergeContext | Merged |
| INT-040 | Encode decode | Scenario | Encode, Decode | Same |
| INT-041 | Get entity summary | Scenario | GetEntitySummary | Summary |
| INT-042 | Get related entities | Scenario | GetRelatedEntities | Related |
| INT-043 | Get token count | Scenario | GetTokenCount | Count |
| INT-044 | Format for AI | Scenario | FormatForAI | Formatted |
| INT-045 | Default values | Scenario | ApplyDefaults | Applied |
| INT-046 | Escape special | Scenario | Escape | Escaped |
| INT-047 | Validate template | Scenario | ValidateTemplate | Valid |
| INT-048 | Get required vars | Scenario | GetRequiredVars | Vars |
| INT-049 | Audit trail | Scenario | Operations | Trail |
| INT-050 | E2E build-cache-find | Scenario | Full cycle | Complete |
| INT-051 | Build then aggregate | Scenario | Build, Aggregate | Complete |
| INT-052 | Embed then find | Scenario | Embed, Find | Complete |
| INT-053 | Template then substitute | Scenario | Template, Substitute | Complete |
| INT-054 | Chunk then combine | Scenario | Chunk, Combine | Complete |
| INT-055 | Cache then invalidate | Scenario | Cache, Invalidate | Complete |
| INT-056 | Merge then truncate | Scenario | Merge, Truncate | Complete |
| INT-057 | Encode then decode | Scenario | Encode, Decode | Same |
| INT-058 | Get summary then format | Scenario | Summary, Format | Complete |
| INT-059 | Get related then aggregate | Scenario | Related, Aggregate | Complete |
| INT-060 | Token count then truncate | Scenario | Count, Truncate | Complete |
| INT-061 | DbContext scope | Integration | Request | Scoped |
| INT-062 | Permission cascade | Integration | Role | Cascade |
| INT-063 | User context propagation | Integration | Request | Propagated |
| INT-064 | Audit chain | Integration | Operations | Chained |
| INT-065 | Config service | Integration | Config | Service |
| INT-066 | Error handling chain | Integration | Error | Handled |
| INT-067 | Validation chain | Integration | Build | Validated |
| INT-068 | Mapping chain | Integration | Entity | Mapped |
| INT-069 | Repository CRUD | Integration | Repository | CRUD |
| INT-070 | DbContext save | Integration | SaveChanges | Saved |
| INT-071 | Transaction rollback | Integration | Error | Rollback |
| INT-072 | Embedding API flow | Integration | API | Flow |
| INT-073 | Cache flow | Integration | Cache | Flow |
| INT-074 | Concurrent build | Scenario | Parallel build | All succeed |
| INT-075 | Concurrent find | Scenario | Parallel find | All succeed |
| INT-076 | Build aggregate format | Scenario | Full cycle | Complete |
| INT-077 | Embed find format | Scenario | Full cycle | Complete |
| INT-078 | Template substitute format | Scenario | Full cycle | Complete |
| INT-079 | Chunk combine format | Scenario | Full cycle | Complete |
| INT-080 | Cache invalidate get | Scenario | Full cycle | Complete |
| INT-081 | Merge truncate encode | Scenario | Full cycle | Complete |
| INT-082 | Entity summary format | Scenario | Full cycle | Complete |
| INT-083 | Related aggregate format | Scenario | Full cycle | Complete |
| INT-084 | Token truncate format | Scenario | Full cycle | Complete |
| INT-085 | Permission check flow | Integration | Auth | Check |
| INT-086 | User resolution flow | Integration | User | Resolved |
| INT-087 | Audit flow | Integration | Audit | Logged |
| INT-088 | Logging flow | Integration | Log | Logged |
| INT-089 | Config flow | Integration | Config | Config |
| INT-090 | E2E full lifecycle | Scenario | All operations | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in query | '; DROP TABLE-- | Query | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-004 | XSS in context | <script>alert(1)</script> | Context | Escaped |
| SEC-005 | XSS in template | <img onerror=...> | Template | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized build | No permission | BuildContext | 403 |
| SEC-012 | Unauthorized find | No permission | FindSimilar | 403 |
| SEC-013 | Unauthorized embed | No permission | GenerateEmbedding | 403 |
| SEC-014 | Unauthorized aggregate | No permission | AggregateEntity | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B context | 403 |
| SEC-017 | IDOR get other | Id=other | GetById | 403/404 |
| SEC-018 | IDOR build other | Id=other | BuildContext | 403 |
| SEC-019 | IDOR find other | Id=other | FindSimilar | 403 |
| SEC-020 | IDOR in filter | EntityId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign Context | Context=manipulated | Request | Validated |
| SEC-025 | Template injection | {{malicious}} | Template | Sanitized |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on build | No token | BuildContext | Rejected |
| SEC-030 | CSRF on find | No token | FindSimilar | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Context tampering | Tamper context | Access | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit embed | Many embeds | GenerateEmbedding | Throttled |
| SEC-036 | Rate limit find | Many finds | FindSimilar | Throttled |
| SEC-037 | Rate limit build | Many builds | BuildContext | Throttled |
| SEC-038 | Oversized request | 10MB payload | BuildContext | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in text | Text | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge context | BuildContext | Rejected |
| SEC-045 | Variable injection | Malicious var | SubstituteVariables | Sanitized |
| SEC-046 | Embedding injection | Malicious embed | GenerateEmbedding | Rejected |
| SEC-047 | Query injection | Malicious query | FindSimilar | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Cache ACL | Direct access | Cache | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users build same | A, B build | Optimistic lock |
| CON-002 | Build and invalidate | Build, invalidate | Deterministic |
| CON-003 | Double find similar | Two find | Both succeed |
| CON-004 | Concurrent build | Two build | Both succeed |
| CON-005 | Read during write | Read while build | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on cache | Two cache | One or both |
| CON-009 | Race on invalidate | Two invalidate | Both |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel builds | 10 parallel | All succeed |
| CON-012 | Async parallel finds | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Embed concurrent | Two embed | Both succeed |
| CON-016 | Build concurrent | Two build | Both succeed |
| CON-017 | Find concurrent | Two find | Both succeed |
| CON-018 | Soft delete concurrent | Delete while build | Deterministic |
| CON-019 | Cache concurrent | Two cache | Both |
| CON-020 | Render concurrent | Two render | Both succeed |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Embedding API limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate entity not null | Validation | null | Exception |
| UNT-002 | Validate text | Validation | Valid text | Pass |
| UNT-003 | Validate query | Validation | Valid query | Pass |
| UNT-004 | Validate template | Validation | Valid template | Pass |
| UNT-005 | Validate variables | Validation | Valid vars | Pass |
| UNT-006 | Format context | Formatting | Context | Formatted |
| UNT-007 | Format template | Formatting | Template | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Similarity score | Calculation | Vectors | Score |
| UNT-013 | Token count | Calculation | Text | Count |
| UNT-014 | Template allows render | Status logic | Template | true |
| UNT-015 | Entity allows aggregate | Status logic | Entity | true |
| UNT-016 | Context allows cache | Status logic | Context | true |
| UNT-017 | Query allows find | Status logic | Query | true |
| UNT-018 | Text check | Status logic | Text | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single build context | BuildContext | <500ms | P1 |
| PRF-002 | Single generate embedding | GenerateEmbedding | <1s | P1 |
| PRF-003 | Single find similar | FindSimilar | <500ms | P1 |
| PRF-004 | Render template | RenderTemplate | <100ms | P0 |
| PRF-005 | Aggregate entity | AggregateEntity | <300ms | P0 |
| PRF-006 | Cache hit | GetContext cached | <10ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Batch 10 embeddings | GenerateBatch | <5s | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel | <2s total | P1 |
| PRF-011 | Concurrent 5 builds | 5 parallel | <5s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 build | <5s total | P2 |
| PRF-013 | Memory context 10k | BuildContext | <100MB | P2 |
| PRF-014 | Memory batch 10 | GenerateBatch | <50MB | P2 |
| PRF-015 | Memory find 1000 | FindSimilar | <100MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 2 RPS build | 2 req/s | 5 min | 99% success |
| LDT-002 | Sustained 10 RPS find | 10 req/s | 5 min | 99% success |
| LDT-003 | Sustained 2 RPS mixed | 2 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 10 RPS build | 0→10→0 | 1 min | No errors |
| LDT-005 | Spike 20 RPS find | 0→20→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress embedding | Many embeds | Until limit | Holds |
| LDT-008 | Stress memory | Large context | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
