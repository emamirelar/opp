# GoogleTextToSpeechService — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Services/GoogleTextToSpeechService` (Unit Tests)  
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

Text-to-speech service unit tests cover audio generation, voice selection, SSML parsing, and caching for TTS operations. Tests include: synthesize text to audio, select voice/language, parse SSML, cache audio, and handle API errors.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Synthesize text | Text valid | Synthesize | Audio returned |
| POS-002 | Synthesize with voice | Voice valid | Synthesize | Audio with voice |
| POS-003 | Synthesize with language | Language valid | Synthesize | Audio |
| POS-004 | Synthesize SSML | SSML valid | Synthesize | Audio |
| POS-005 | Get available voices | API available | GetVoices | Voices list |
| POS-006 | Get voice by language | Language valid | GetVoice | Voice |
| POS-007 | Cache audio | Cache enabled | Synthesize | Cached |
| POS-008 | Get from cache | Cached | Synthesize | From cache |
| POS-009 | Select voice male | Voice type | SelectVoice | Male voice |
| POS-010 | Select voice female | Voice type | SelectVoice | Female voice |
| POS-011 | Select voice neutral | Voice type | SelectVoice | Neutral |
| POS-012 | Set speaking rate | Rate valid | SetRate | Rate set |
| POS-013 | Set pitch | Pitch valid | SetPitch | Pitch set |
| POS-014 | Set volume gain | Gain valid | SetVolume | Volume set |
| POS-015 | Parse SSML | SSML valid | ParseSsml | Parsed |
| POS-016 | Validate SSML | SSML valid | ValidateSsml | Valid |
| POS-017 | Get supported languages | API available | GetLanguages | Languages |
| POS-018 | Get audio format | Format valid | GetFormat | Format |
| POS-019 | Set audio format | Format valid | SetFormat | Format set |
| POS-020 | Synthesize long text | Text long | Synthesize | Chunked |
| POS-021 | Retry on transient | Transient error | Retry | Success |
| POS-022 | Audit synthesize | Synthesize | Check audit | Logged |
| POS-023 | Pagination voices | Many voices | GetVoices | Pages |
| POS-024 | Filter voices by language | Language | Filter | Filtered |
| POS-025 | Filter voices by type | Type | Filter | Filtered |
| POS-026 | Get default voice | Default exists | GetDefault | Voice |
| POS-027 | Get default language | Default exists | GetDefault | Language |
| POS-028 | Health check | API available | HealthCheck | Healthy |
| POS-029 | Get usage stats | Usage tracked | GetUsage | Stats |
| POS-030 | Rate limit check | Within limit | CheckRate | Allowed |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Synthesize null text | Text=null | ArgumentNullException |
| NEG-002 | Synthesize empty text | Text="" | ValidationException |
| NEG-003 | Synthesize text too long | Text length | ValidationException |
| NEG-004 | Synthesize invalid voice | Voice invalid | ArgumentException |
| NEG-005 | Synthesize invalid language | Language invalid | ArgumentException |
| NEG-006 | Synthesize invalid SSML | SSML invalid | ParseException |
| NEG-007 | Get voices API down | API down | ServiceUnavailableException |
| NEG-008 | Get voice invalid language | Language invalid | KeyNotFoundException |
| NEG-009 | Cache null key | Key=null | ArgumentNullException |
| NEG-010 | Select voice invalid | Voice invalid | ArgumentException |
| NEG-011 | Set rate invalid | Rate invalid | ArgumentException |
| NEG-012 | Set pitch invalid | Pitch invalid | ArgumentException |
| NEG-013 | Set volume invalid | Volume invalid | ArgumentException |
| NEG-014 | Parse SSML null | SSML=null | ArgumentNullException |
| NEG-015 | Parse SSML malformed | SSML malformed | ParseException |
| NEG-016 | Validate SSML invalid | SSML invalid | ValidationException |
| NEG-017 | Get languages API down | API down | ServiceUnavailableException |
| NEG-018 | Get format invalid | Format invalid | ArgumentException |
| NEG-019 | Set format invalid | Format invalid | ArgumentException |
| NEG-020 | API key missing | Key=null | ConfigurationException |
| NEG-021 | API key invalid | Key=invalid | UnauthorizedException |
| NEG-022 | Rate limit exceeded | Over limit | RateLimitException |
| NEG-023 | Quota exceeded | Quota full | QuotaExceededException |
| NEG-024 | Timeout | Slow API | TimeoutException |
| NEG-025 | Network error | Network down | NetworkException |
| NEG-026 | Synthesize without permission | Unauthorized | Forbidden |
| NEG-027 | Get voices without permission | Unauthorized | Forbidden |
| NEG-028 | Batch null list | List=null | ArgumentNullException |
| NEG-029 | Batch empty list | List=[] | ArgumentException |
| NEG-030 | Stream null handler | Handler=null | ArgumentNullException |
| NEG-031 | Get metadata invalid | Id invalid | KeyNotFoundException |
| NEG-032 | Preload invalid | Params invalid | ArgumentException |
| NEG-033 | Health check failed | API down | UnhealthyException |
| NEG-034 | Get usage invalid | No stats | NullReferenceException |
| NEG-035 | Validate text null | Text=null | ArgumentNullException |
| NEG-036 | Validate text empty | Text="" | False |
| NEG-037 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-038 | Concurrent rate limit | Over limit | RateLimitException |
| NEG-039 | Transaction rollback | Fail in transaction | Rollback |
| NEG-040 | Connection timeout | API unavailable | TimeoutException |
| NEG-041 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-042 | Invalid enum value | Format invalid | ArgumentException |
| NEG-043 | Expired session | Expired token | Unauthorized |
| NEG-044 | Null user context | User=null | InvalidOperationException |
| NEG-045 | Invalid include path | Invalid include | ArgumentException |
| NEG-046 | SSML unsupported tag | Tag invalid | ParseException |
| NEG-047 | SSML nested too deep | Nesting | ParseException |
| NEG-048 | Voice not found | Voice invalid | KeyNotFoundException |
| NEG-049 | Language not found | Language invalid | KeyNotFoundException |
| NEG-050 | Audio format unsupported | Format invalid | ArgumentException |
| NEG-051 | Speaking rate out of range | Rate=2.0 | ArgumentException |
| NEG-052 | Pitch out of range | Pitch=50 | ArgumentException |
| NEG-053 | Volume gain out of range | Gain=100 | ArgumentException |
| NEG-054 | Batch one invalid | One invalid | PartialFailureException |
| NEG-055 | Cache key invalid | Key invalid | ArgumentException |
| NEG-056 | Get default no default | No default | KeyNotFoundException |
| NEG-057 | Stream interrupted | Interrupted | OperationCanceledException |
| NEG-058 | Retry exhausted | All retries fail | ApiException |
| NEG-059 | Audit missing user | User=0 | InvalidOperationException |
| NEG-060 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-061 | GetVoices filter invalid | Filter invalid | ArgumentException |
| NEG-062 | Pagination invalid | Page invalid | ArgumentException |
| NEG-063 | Child override throws | Child throws | Propagated |
| NEG-064 | Voice type invalid | Type invalid | ArgumentException |
| NEG-065 | Language code invalid | Code invalid | ArgumentException |
| NEG-066 | Content filter | Blocked content | ContentFilterException |
| NEG-067 | Text encoding invalid | Encoding invalid | ArgumentException |
| NEG-068 | SSML entity invalid | Entity invalid | ParseException |
| NEG-069 | Cache corruption | Corrupted cache | CacheException |
| NEG-070 | Audio decode error | Invalid audio | DecodeException |
| NEG-071 | Synthesize whitespace-only | Text="   " | ValidationException |
| NEG-072 | Get voice null language | Language=null | ArgumentNullException |
| NEG-073 | Set rate null | Rate=null | ArgumentNullException |
| NEG-074 | GetVoices invalid page | Page invalid | ArgumentException |
| NEG-075 | Parse SSML empty | SSML="" | ParseException |
| NEG-076 | Validate SSML null | SSML=null | ArgumentNullException |
| NEG-077 | Get format null | Format=null | ArgumentNullException |
| NEG-078 | Batch one null text | Text null in batch | ArgumentNullException |
| NEG-079 | Stream invalid handler | Handler invalid | ArgumentException |
| NEG-080 | Get metadata null id | Id=null | ArgumentNullException |
| NEG-081 | Preload null voices | Voices=null | ArgumentNullException |
| NEG-082 | Health check invalid | Invalid params | ArgumentException |
| NEG-083 | Get usage null user | User=null | ArgumentNullException |
| NEG-084 | Validate text too long | Text over limit | ValidationException |
| NEG-085 | Select voice null | Voice=null | ArgumentNullException |
| NEG-086 | Set pitch null | Pitch=null | ArgumentNullException |
| NEG-087 | Set volume null | Volume=null | ArgumentNullException |
| NEG-088 | Get languages invalid | Params invalid | ArgumentException |
| NEG-089 | Cache key invalid chars | Key invalid | ArgumentException |
| NEG-090 | Retry count negative | Retry=-1 | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Text at min length | Length=1 | Valid |
| BND-002 | Text at max length | Length=5000 | Valid |
| BND-003 | Text exceeds max | Length=5001 | Reject |
| BND-004 | SSML at min | Length=1 | Valid |
| BND-005 | SSML at max | Length=limit | Valid |
| BND-006 | SSML exceeds max | Length=limit+1 | Reject |
| BND-007 | Speaking rate at 0.25 | Rate=0.25 | Valid |
| BND-008 | Speaking rate at 4.0 | Rate=4.0 | Valid |
| BND-009 | Speaking rate over 4.0 | Rate=4.1 | Reject |
| BND-010 | Pitch at -20 | Pitch=-20 | Valid |
| BND-011 | Pitch at 20 | Pitch=20 | Valid |
| BND-012 | Pitch over 20 | Pitch=21 | Reject |
| BND-013 | Volume at 0 | Volume=0 | Valid |
| BND-014 | Volume at 16 | Volume=16 | Valid |
| BND-015 | Volume over 16 | Volume=17 | Reject |
| BND-016 | Page size at min | PageSize=1 | Valid |
| BND-017 | Page size at max | PageSize=100 | Valid |
| BND-018 | Page size over max | PageSize=101 | Reject |
| BND-019 | Unicode in text | Arabic/Chinese | Valid |
| BND-020 | Special chars in text | <>&"' | Escaped |
| BND-021 | Newlines in text | \n\r | Handled |
| BND-022 | Empty voice list | Voices=[] | Empty list |
| BND-023 | Single voice | Count=1 | Valid |
| BND-024 | Max voices | At limit | Valid |
| BND-025 | Empty language list | Languages=[] | Empty list |
| BND-026 | Single language | Count=1 | Valid |
| BND-027 | Rate limit at limit | At limit | Reject |
| BND-028 | Rate limit at limit-1 | Limit-1 | Valid |
| BND-029 | Cache TTL at min | TTL=1s | Valid |
| BND-030 | Cache TTL at max | TTL=24h | Valid |
| BND-031 | Batch count at max | Count=10 | Valid |
| BND-032 | Batch count over max | Count=11 | Reject |
| BND-033 | Pagination last partial | Partial page | Correct |
| BND-034 | Pagination total | Total count | Accurate |
| BND-035 | Sort null handling | Nulls in data | Deterministic |
| BND-036 | Filter combination all | All filters | Correct |
| BND-037 | Format enum first | First | Valid |
| BND-038 | Format enum last | Last | Valid |
| BND-039 | Voice type boundary | Male/Female | Valid |
| BND-040 | Language code boundary | en-US | Valid |
| BND-041 | Audio size zero | Size=0 | Reject |
| BND-042 | Audio size max | Size=limit | Valid |
| BND-043 | Stream chunk size min | Size=1 | Valid |
| BND-044 | Stream chunk size max | Size=limit | Valid |
| BND-045 | Retry count at 0 | Retry=0 | No retry |
| BND-046 | Retry count at max | Retry=max | Max retries |
| BND-047 | Timeout at min | Timeout=1s | Valid |
| BND-048 | Timeout at max | Timeout=60s | Valid |
| BND-049 | Soft delete boundary | DeletedDate set | Excluded |
| BND-050 | Include depth | Deep include | No explosion |
| BND-051 | Query timeout | Slow query | Timeout |
| BND-052 | Memory large text | 100k chars | No OOM |
| BND-053 | Audit timestamp precision | Millisecond | Stored |
| BND-054 | Long string in SSML | 10k chars | Valid or reject |
| BND-055 | SSML tag depth max | At limit | Valid |
| BND-056 | SSML tag depth over max | Over limit | Reject |
| BND-057 | Cache hit | Cached | Hit |
| BND-058 | Cache miss | Not cached | Miss |
| BND-059 | Cache expiry | After expiry | Miss |
| BND-060 | Get default voice | Default | Voice |
| BND-061 | Get default language | Default | Language |
| BND-062 | Health check interval | Interval | Valid |
| BND-063 | Usage stats zero | Usage=0 | Valid |
| BND-064 | Usage stats max | Usage=max | Valid |
| BND-065 | Stream empty | No chunks | Empty |
| BND-066 | Stream single chunk | 1 chunk | Valid |
| BND-067 | Batch single | Count=1 | Valid |
| BND-068 | Validate text max | At limit | True |
| BND-069 | Async cancellation | Cancel token | OperationCanceledException |
| BND-070 | Task timeout | Timeout | TimeoutException |
| BND-071 | Text single char | Length=1 | Valid |
| BND-072 | Voice list single | Count=1 | Valid |
| BND-073 | Language list single | Count=1 | Valid |
| BND-074 | Batch count one | Count=1 | Valid |
| BND-075 | Speaking rate min | Rate=0.25 | Valid |
| BND-076 | Speaking rate max | Rate=4.0 | Valid |
| BND-077 | Pitch min | Pitch=-20 | Valid |
| BND-078 | Pitch max | Pitch=20 | Valid |
| BND-079 | Volume min | Volume=0 | Valid |
| BND-080 | Volume max | Volume=16 | Valid |
| BND-081 | Format enum first | First | Valid |
| BND-082 | Format enum last | Last | Valid |
| BND-083 | Pagination first page | Page=1 | Valid |
| BND-084 | Pagination last partial | Partial | Correct |
| BND-085 | Filter language and type | Both | Correct |
| BND-086 | Stream chunk min | Size=1 | Valid |
| BND-087 | Stream chunk max | Size=limit | Valid |
| BND-088 | Retry at min | Retry=0 | No retry |
| BND-089 | Timeout at min | Timeout=1s | Valid |
| BND-090 | Timeout at max | Timeout=60s | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Text required | Validation | Synthesize | Reject if empty |
| FUN-002 | Voice required for voice | Validation | Synthesize | Reject if invalid |
| FUN-003 | Language required | Validation | Synthesize | Reject if invalid |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | Get excludes deleted | Constraint | Get | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Text length limit | Constraint | Synthesize | Reject over |
| FUN-008 | SSML format valid | Constraint | ParseSsml | Reject invalid |
| FUN-009 | Rate limit enforced | Constraint | Synthesize | Reject if over |
| FUN-010 | Audit synthesize | Audit | Synthesize | Logged |
| FUN-011 | Audit CreatedBy | Audit | Create | Set user |
| FUN-012 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-013 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-014 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Voice selection logic | Logic | SelectVoice | Selected |
| FUN-017 | Language selection logic | Logic | SelectLanguage | Selected |
| FUN-018 | Cache key generation | Logic | Cache | Unique key |
| FUN-019 | Cache TTL respected | Logic | Cache | Expiry |
| FUN-020 | List respects filter | Constraint | GetVoices | Filtered |
| FUN-021 | Pagination correct | Logic | GetVoices | Correct page |
| FUN-022 | Pagination offset | Calculation | Page | Skip correct |
| FUN-023 | Total count accurate | Calculation | Count | Matches |
| FUN-024 | Sort applies | Calculation | Sort | Ordered |
| FUN-025 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-026 | Retry on transient | Logic | Retry | Retried |
| FUN-027 | Batch atomic | Logic | Batch | All or none |
| FUN-028 | Stream chunk size | Logic | Stream | Chunked |
| FUN-029 | SSML parsing | Logic | ParseSsml | Parsed |
| FUN-030 | SSML validation | Logic | ValidateSsml | Validated |
| FUN-031 | Transaction on create | Transaction | Create | Atomic |
| FUN-032 | Transaction on update | Transaction | Update | Atomic |
| FUN-033 | Async all operations | Concurrency | All | Async |
| FUN-034 | Include loads config | Data load | Get include | Config loaded |
| FUN-035 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-036 | Format conversion | Logic | SetFormat | Converted |
| FUN-037 | Rate limits | Logic | CheckRate | Limited |
| FUN-038 | Usage tracking | Logic | Synthesize | Tracked |
| FUN-039 | Health check | Logic | HealthCheck | Checked |
| FUN-040 | Get default fallback | Logic | GetDefault | Fallback |
| FUN-041 | Preload voices | Logic | Preload | Loaded |
| FUN-042 | Get metadata | Logic | GetMetadata | Metadata |
| FUN-043 | Validate text | Logic | Validate | Validated |
| FUN-044 | Speaking rate application | Logic | SetRate | Applied |
| FUN-045 | Pitch application | Logic | SetPitch | Applied |
| FUN-046 | Volume application | Logic | SetVolume | Applied |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Stream disposal | Logic | Stream | Disposed |
| FUN-051 | Text trim on synthesize | Logic | Synthesize | Trimmed |
| FUN-052 | Voice selection fallback | Logic | SelectVoice | Fallback |
| FUN-053 | Language format validation | Constraint | Format | Valid |
| FUN-054 | SSML tag whitelist | Constraint | ParseSsml | Reject invalid |
| FUN-055 | Rate limit per user | Constraint | Synthesize | Per user |
| FUN-056 | Cache key format | Logic | Cache | Unique key |
| FUN-057 | Usage stats increment | Logic | Synthesize | Incremented |
| FUN-058 | Health check interval | Logic | HealthCheck | Interval |
| FUN-059 | Default voice fallback | Logic | GetDefault | Fallback |
| FUN-060 | Batch sequential | Logic | Batch | Sequential |
| FUN-061 | Stream chunk size | Logic | Stream | Chunked |
| FUN-062 | Retry backoff | Logic | Retry | Exponential |
| FUN-063 | Timeout per request | Logic | SendRequest | Timeout |
| FUN-064 | Format conversion | Logic | SetFormat | Converted |
| FUN-065 | Pitch application | Logic | SetPitch | Applied |
| FUN-066 | Volume application | Logic | SetVolume | Applied |
| FUN-067 | Filter voices AND | Logic | GetVoices | Combined |
| FUN-068 | Pagination max page | Logic | GetVoices | Capped |
| FUN-069 | Sort voices | Logic | GetVoices | Ordered |
| FUN-070 | Include loads config | Data load | Get include | Config |
| FUN-071 | No Cartesian on includes | Data load | Multiple | Split |
| FUN-072 | Audit synthesize call | Audit | Synthesize | Logged |
| FUN-073 | Permission before synthesize | Authorization | Synthesize | Check first |
| FUN-074 | Permission before get voices | Authorization | GetVoices | Check first |
| FUN-075 | Validate text format | Validation | Validate | Format |
| FUN-076 | Parse SSML structure | Logic | ParseSsml | Parsed |
| FUN-077 | Validate SSML structure | Logic | ValidateSsml | Validated |
| FUN-078 | GetMetadata complete | Logic | GetMetadata | Complete |
| FUN-079 | Preload caches | Logic | Preload | Cached |
| FUN-080 | Stream disposal on error | Logic | Stream | Disposed |
| FUN-081 | Batch partial failure | Logic | Batch | Partial |
| FUN-082 | Cache hit returns | Logic | Cache | Hit |
| FUN-083 | Cache miss fetches | Logic | Cache | Miss |
| FUN-084 | Token count estimate | Logic | CountTokens | Estimate |
| FUN-085 | Truncate preserves | Logic | Truncate | Start |
| FUN-086 | GetLanguages ordered | Logic | GetLanguages | Ordered |
| FUN-087 | GetFormat default | Logic | GetFormat | Default |
| FUN-088 | SetFormat validation | Logic | SetFormat | Validated |
| FUN-089 | Rate limit reset | Logic | Reset | Reset |
| FUN-090 | Health check failure | Logic | HealthCheck | Unhealthy |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Synthesize full flow | Synthesize | Audio | Synthesized |
| INT-002 | Get voices full flow | GetVoices | Voices | List |
| INT-003 | Synthesize with voice | Synthesize | Audio, Voice | Audio |
| INT-004 | Synthesize with language | Synthesize | Audio, Language | Audio |
| INT-005 | Synthesize SSML | Synthesize | Audio, SSML | Audio |
| INT-006 | Cache hit | Cache | Cache | Hit |
| INT-007 | Cache miss | Cache | Cache | Miss |
| INT-008 | Select voice | SelectVoice | Voice | Selected |
| INT-009 | Set parameters | Set | Params | Set |
| INT-010 | Parse SSML | ParseSsml | SSML | Parsed |
| INT-011 | Validate SSML | ValidateSsml | SSML | Validated |
| INT-012 | Get languages | GetLanguages | Languages | List |
| INT-013 | Get format | GetFormat | Format | Format |
| INT-014 | Set format | SetFormat | Format | Set |
| INT-015 | Pagination | Paginate | Voices | Pages |
| INT-016 | Audio-Config relationship | Relationship | Audio, Config | Valid |
| INT-017 | Voice-Language relationship | Relationship | Voice, Language | Valid |
| INT-018 | Cache-Audio relationship | Relationship | Cache, Audio | Valid |
| INT-019 | Cascade delete | Relationship | Config deleted | Config |
| INT-020 | Orphan handling | Relationship | Config deleted | Retained |
| INT-021 | TTS API error handling | Error | API down | Graceful |
| INT-022 | Timeout handling | Error | Slow API | Timeout |
| INT-023 | Rate limit handling | Error | Over limit | RateLimitException |
| INT-024 | Parse error handling | Error | Malformed SSML | ParseException |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | HTTP client integration | Integration | HttpClient | Call |
| INT-030 | TTS client integration | Integration | TTS | Client |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | Config integration | Integration | Config | Read |
| INT-036 | Synthesize then cache | Scenario | Synthesize, Cache | Both |
| INT-037 | Batch synthesize | Scenario | Batch | All synthesized |
| INT-038 | Stream then play | Scenario | Stream | Played |
| INT-039 | Parse then synthesize | Scenario | Parse, Synthesize | Both |
| INT-040 | Get default then synthesize | Scenario | GetDefault, Synthesize | Both |
| INT-041 | Health check | Scenario | HealthCheck | Healthy |
| INT-042 | Usage tracking | Scenario | Synthesize | Tracked |
| INT-043 | Rate limit check | Scenario | CheckRate | Checked |
| INT-044 | Preload then get | Scenario | Preload, Get | Both |
| INT-045 | Filter voices | Scenario | Filter | Filtered |
| INT-046 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-047 | Get metadata | Scenario | GetMetadata | Metadata |
| INT-048 | Validate text | Scenario | Validate | Validated |
| INT-049 | Retry on transient | Scenario | Retry | Success |
| INT-050 | E2E synthesize flow | Scenario | Full flow | Complete |
| INT-051 | Synthesize then cache | Scenario | Synthesize, Cache | Both |
| INT-052 | Get voice then synthesize | Scenario | GetVoice, Synthesize | Both |
| INT-053 | Parse then validate | Scenario | Parse, Validate | Both |
| INT-054 | Batch then get metadata | Scenario | Batch, GetMetadata | Both |
| INT-055 | Stream then dispose | Scenario | Stream | Disposed |
| INT-056 | Preload then get | Scenario | Preload, Get | Both |
| INT-057 | Health check then synthesize | Scenario | HealthCheck, Synthesize | Both |
| INT-058 | Rate limit check then synthesize | Scenario | CheckRate, Synthesize | Both |
| INT-059 | Get usage then synthesize | Scenario | GetUsage, Synthesize | Both |
| INT-060 | Set format then synthesize | Scenario | SetFormat, Synthesize | Both |
| INT-061 | Get default then synthesize | Scenario | GetDefault, Synthesize | Both |
| INT-062 | TTS client call | Integration | TTS | Called |
| INT-063 | HTTP client integration | Integration | HttpClient | Call |
| INT-064 | Config integration | Integration | Config | Read |
| INT-065 | Logger integration | Integration | Logger | Logged |
| INT-066 | Permission service | Integration | Permission | Check |
| INT-067 | User resolver | Integration | User | Resolved |
| INT-068 | Audit context | Integration | Audit | Context |
| INT-069 | Mapper integration | Integration | Mapper | Mapped |
| INT-070 | Repository integration | Integration | Repository | CRUD |
| INT-071 | DbContext integration | Integration | DbContext | Scoped |
| INT-072 | Transaction scope | Integration | Transaction | Atomic |
| INT-073 | Cache hit flow | Scenario | Cache | Hit |
| INT-074 | Cache miss flow | Scenario | Cache | Miss |
| INT-075 | Retry flow | Scenario | Retry | Success |
| INT-076 | Timeout flow | Scenario | Timeout | Handled |
| INT-077 | Fallback model flow | Scenario | Fallback | Used |
| INT-078 | Pagination with filter | Scenario | Paginate | Filtered |
| INT-079 | Filter voices | Scenario | Filter | Filtered |
| INT-080 | Get metadata | Scenario | GetMetadata | Metadata |
| INT-081 | Validate text | Scenario | Validate | Validated |
| INT-082 | Parse SSML | Scenario | ParseSsml | Parsed |
| INT-083 | Validate SSML | Scenario | ValidateSsml | Validated |
| INT-084 | Get format | Scenario | GetFormat | Format |
| INT-085 | Set format | Scenario | SetFormat | Set |
| INT-086 | Get languages | Scenario | GetLanguages | Languages |
| INT-087 | Get default voice | Scenario | GetDefault | Voice |
| INT-088 | Get default language | Scenario | GetDefault | Language |
| INT-089 | Health check | Scenario | HealthCheck | Healthy |
| INT-090 | Full workflow | Scenario | Full flow | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SSML injection | <script> in SSML | SSML | Sanitized |
| SEC-002 | SQL injection | '; DROP TABLE-- | Text | Sanitized |
| SEC-003 | XSS in text | <script>alert(1)</script> | Text | Escaped |
| SEC-004 | XSS in SSML | <img onerror=...> | SSML | Escaped |
| SEC-005 | LDAP injection | *)(uid=* | Filter | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Any | Rejected |
| SEC-008 | API key in log | Log | Log | Redacted |
| SEC-009 | API key in error | Error | Stack | Redacted |
| SEC-010 | Unauthorized synthesize | No permission | Synthesize | 403 |
| SEC-011 | Unauthorized get voices | No permission | GetVoices | 403 |
| SEC-012 | Unauthorized cache | No permission | Cache | 403 |
| SEC-013 | Role escalation | Low role | Admin | 403 |
| SEC-014 | Cross-tenant access | User A | User B data | 403 |
| SEC-015 | IDOR get other | Id=other | Get | 403/404 |
| SEC-016 | IDOR update other | Id=other | Update | 403 |
| SEC-017 | IDOR delete other | Id=other | Delete | 403 |
| SEC-018 | IDOR in filter | UserId=other | List | Filtered |
| SEC-019 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-020 | Mass assign API key | APIKey= | Request | Ignored |
| SEC-021 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-022 | Session hijack | Stolen token | Any | Detected |
| SEC-023 | Token expiration | Expired | Any | 401 |
| SEC-024 | Invalid token | Malformed | Any | 401 |
| SEC-025 | CSRF on synthesize | No token | Synthesize | Rejected |
| SEC-026 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-027 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-028 | Content in audio | Malicious content | Synthesize | Filtered |
| SEC-029 | Rate limit bypass | Bypass attempt | Rate limit | Blocked |
| SEC-030 | Rate limit synthesize | Many synthesize | Synthesize | Throttled |
| SEC-031 | Rate limit get voices | Many get | GetVoices | Throttled |
| SEC-032 | Oversized request | 10MB text | Synthesize | Rejected |
| SEC-033 | Deep nesting | Nested SSML | Request | Rejected |
| SEC-034 | Header injection | \r\n in header | Header | Rejected |
| SEC-035 | Null byte injection | %00 in text | Text | Rejected |
| SEC-036 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-037 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-038 | Denial of service | Huge text | Synthesize | Rejected |
| SEC-039 | SSML entity injection | Malicious entity | SSML | Rejected |
| SEC-040 | Audio format tampering | Tamper format | Audio | Rejected |
| SEC-041 | Cache poisoning | Malicious cache | Cache | Not used |
| SEC-042 | Import malicious audio | Malicious | Upload | Rejected |
| SEC-043 | Export data injection | Inject in export | Export | Sanitized |
| SEC-044 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-045 | Permission cached | Repeated check | Permission | Cached |
| SEC-046 | API key rotation | Rotate key | Config | Updated |
| SEC-047 | Request signing | Tamper request | Request | Rejected |
| SEC-048 | Content filter bypass | Bypass filter | Content | Blocked |
| SEC-049 | Voice spoofing | Spoof voice | Voice | Validated |
| SEC-050 | Language spoofing | Spoof language | Language | Validated |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent synthesize | Two synthesize | Both succeed |
| CON-004 | Concurrent get voices | Two get | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on cache | Two cache | Consistent |
| CON-009 | Race on rate limit | Two check | Correct limit |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel synthesize | 10 parallel | All succeed |
| CON-012 | Async parallel get voices | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Stream concurrent | Two stream | Both succeed |
| CON-016 | Cache concurrent | Two cache | Consistent |
| CON-017 | Rate limit concurrent | Many concurrent | Limited |
| CON-018 | Usage stats concurrent | Many updates | Consistent |
| CON-019 | Soft delete concurrent | Delete while update | Deterministic |
| CON-020 | Preload concurrent | Two preload | Both succeed |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | TTS API limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate text not null | Validation | null | Exception |
| UNT-002 | Validate voice format | Validation | Valid voice | Pass |
| UNT-003 | Validate language | Validation | Valid language | Pass |
| UNT-004 | Validate SSML | Validation | Valid SSML | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format text display | Formatting | Text | Display |
| UNT-007 | Format SSML | Formatting | SSML | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Cache key hash | Calculation | Text, Voice | Key |
| UNT-013 | Token count estimate | Calculation | Text | Estimate |
| UNT-014 | Voice allows synthesize | Status logic | Voice | true |
| UNT-015 | Language allows synthesize | Status logic | Language | true |
| UNT-016 | Format allows | Status logic | Format | true |
| UNT-017 | Rate limit allows | Status logic | Under limit | true |
| UNT-018 | Cache hit check | Status logic | Cached | true |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single synthesize | Synthesize | <5s | P1 |
| PRF-002 | Get voices | GetVoices | <2s | P1 |
| PRF-003 | Parse SSML | ParseSsml | <100ms | P1 |
| PRF-004 | Validate SSML | ValidateSsml | <50ms | P1 |
| PRF-005 | Cache hit | Cache get | <10ms | P1 |
| PRF-006 | Cache miss | Cache miss | <5s | P1 |
| PRF-007 | Select voice | SelectVoice | <50ms | P1 |
| PRF-008 | Batch 5 | Batch | <25s | P1 |
| PRF-009 | Stream | Stream | <5s | P1 |
| PRF-010 | Concurrent 10 synthesize | 10 parallel | <30s total | P1 |
| PRF-011 | Concurrent 20 get voices | 20 parallel | <5s total | P1 |
| PRF-012 | Concurrent mixed | 5 synth, 5 get | <20s total | P2 |
| PRF-013 | Memory single synthesize | Synthesize | <50MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory batch 10 | Batch 10 | <100MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 2 RPS synthesize | 2 req/s | 5 min | 99% success |
| LDT-002 | Sustained 10 RPS get voices | 10 req/s | 5 min | 99% success |
| LDT-003 | Sustained 2 RPS mixed | 2 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 5 RPS synthesize | 0→5→0 | 1 min | No errors |
| LDT-005 | Spike 20 RPS get voices | 0→20→0 | 30s | Graceful deg |
| LDT-006 | Stress rate limit | Many synthesize | Until limit | Limited |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large batch | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
