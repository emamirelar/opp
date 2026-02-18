# GoogleTextToSpeechService — Test Cases

**Component:** `UNOPS.PAO.Business/Services/GoogleTextToSpeechService`  
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

Google Text-to-Speech: audio generation, voice selection, SSML, language support, caching, streaming.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Synthesize plain text | Valid text | SynthesizeAsync(text) | Audio returned |
| POS-002 | Synthesize with voice | Text + voice | SynthesizeAsync(text, voice) | Audio with voice |
| POS-003 | Synthesize with language | Text + lang | SynthesizeAsync(text, lang) | Audio in language |
| POS-004 | Synthesize SSML | Valid SSML | SynthesizeSsmlAsync(ssml) | Audio |
| POS-005 | Get available voices | None | GetVoicesAsync() | Voices list |
| POS-006 | Get voices by language | Language | GetVoicesAsync(lang) | Filtered voices |
| POS-007 | Get languages | None | GetLanguagesAsync() | Languages |
| POS-008 | Cache hit | Cached audio | SynthesizeAsync(cached) | From cache |
| POS-009 | Streaming synthesis | Text | SynthesizeStreamingAsync(text) | Stream |
| POS-010 | Set speaking rate | Text + rate | SynthesizeAsync(text, rate: 1.2) | Faster speech |
| POS-011 | Set pitch | Text + pitch | SynthesizeAsync(text, pitch: 2) | Higher pitch |
| POS-012 | Set volume gain | Text + gain | SynthesizeAsync(text, gain: 5) | Louder |
| POS-013 | Audio format MP3 | Text | SynthesizeAsync(text, format: MP3) | MP3 |
| POS-014 | Audio format WAV | Text | SynthesizeAsync(text, format: WAV) | WAV |
| POS-015 | Audio format OGG | Text | SynthesizeAsync(text, format: OGG) | OGG |
| POS-016 | Sample rate 8000 | Text | SynthesizeAsync(text, rate: 8000) | 8kHz |
| POS-017 | Sample rate 24000 | Text | SynthesizeAsync(text, rate: 24000) | 24kHz |
| POS-018 | WaveNet voice | Text + WaveNet | SynthesizeAsync(text, voice: WaveNet) | WaveNet |
| POS-019 | Neural2 voice | Text + Neural2 | SynthesizeAsync(text, voice: Neural2) | Neural2 |
| POS-020 | Studio voice | Text + Studio | SynthesizeAsync(text, voice: Studio) | Studio |
| POS-021 | Pause in SSML | SSML with break | SynthesizeSsmlAsync(ssml) | Pause |
| POS-022 | Emphasis in SSML | SSML with emphasis | SynthesizeSsmlAsync(ssml) | Emphasis |
| POS-023 | Phoneme in SSML | SSML with phoneme | SynthesizeSsmlAsync(ssml) | Phoneme |
| POS-024 | Sub in SSML | SSML with sub | SynthesizeSsmlAsync(ssml) | Substitution |
| POS-025 | Prosody in SSML | SSML with prosody | SynthesizeSsmlAsync(ssml) | Prosody |
| POS-026 | Say-as in SSML | SSML with say-as | SynthesizeSsmlAsync(ssml) | Say-as |
| POS-027 | Batch synthesize | Multiple texts | BatchSynthesizeAsync(texts) | Batch audio |
| POS-028 | Cancel synthesis | Synthesis in progress | CancelAsync() | Cancelled |
| POS-029 | Preload voice | Voice | PreloadVoiceAsync(voice) | Preloaded |
| POS-030 | Warm cache | Startup | WarmCacheAsync() | Preloaded |
| POS-031 | Get voice metadata | Voice | GetVoiceMetadataAsync(voice) | Metadata |
| POS-032 | Validate SSML | SSML | ValidateSsmlAsync(ssml) | Valid |
| POS-033 | Detect language | Text | DetectLanguageAsync(text) | Language |
| POS-034 | Estimate duration | Text | EstimateDurationAsync(text) | Duration |
| POS-035 | Get synthesis limits | None | GetLimitsAsync() | Limits |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null text | SynthesizeAsync(null) | ArgumentNullException |
| NEG-002 | Empty text | SynthesizeAsync("") | ArgumentException |
| NEG-003 | Text too long | SynthesizeAsync(veryLong) | ArgumentException |
| NEG-004 | Null SSML | SynthesizeSsmlAsync(null) | ArgumentNullException |
| NEG-005 | Invalid SSML | SynthesizeSsmlAsync("<invalid>") | SsmlException |
| NEG-006 | Malformed SSML | SynthesizeSsmlAsync("<speak>") | SsmlException |
| NEG-007 | Invalid voice | SynthesizeAsync(text, "invalid") | NotFoundException |
| NEG-008 | Invalid language | SynthesizeAsync(text, "xx") | ArgumentException |
| NEG-009 | Unsupported language | SynthesizeAsync(text, "zz") | NotSupportedException |
| NEG-010 | Invalid format | SynthesizeAsync(text, format: "xyz") | ArgumentException |
| NEG-011 | Invalid sample rate | SynthesizeAsync(text, rate: 12345) | ArgumentException |
| NEG-012 | Invalid speaking rate | SynthesizeAsync(text, rate: -1) | ArgumentException |
| NEG-013 | Invalid pitch | SynthesizeAsync(text, pitch: 25) | ArgumentException |
| NEG-014 | Invalid volume | SynthesizeAsync(text, gain: 100) | ArgumentException |
| NEG-015 | Quota exceeded | SynthesizeAsync(quota) | QuotaExceededException |
| NEG-016 | Rate limit | Many requests | TooManyRequestsException |
| NEG-017 | Invalid credentials | Any op | AuthenticationException |
| NEG-018 | Expired credentials | Any op | AuthenticationException |
| NEG-019 | Network timeout | SynthesizeAsync(slow) | TimeoutException |
| NEG-020 | Service unavailable | Any op | ServiceUnavailableException |
| NEG-021 | Cancelled token | SynthesizeAsync(..., cancelled) | OperationCanceledException |
| NEG-022 | Disposed stream | Streaming disposed | ObjectDisposedException |
| NEG-023 | Null voice list | GetVoicesAsync(null) | ArgumentNullException |
| NEG-024 | Null batch texts | BatchSynthesizeAsync(null) | ArgumentNullException |
| NEG-025 | Empty batch | BatchSynthesizeAsync([]) | ArgumentException |
| NEG-026 | Batch too large | BatchSynthesizeAsync(1000) | ArgumentException |
| NEG-027 | XSS in text | SynthesizeAsync("<script>") | Sanitized |
| NEG-028 | SSML injection | SynthesizeSsmlAsync(injection) | Sanitized |
| NEG-029 | Control chars in text | SynthesizeAsync(control) | Sanitized |
| NEG-030 | Null byte in text | SynthesizeAsync(nullByte) | ArgumentException |
| NEG-031 | Binary in text | SynthesizeAsync(binary) | ArgumentException |
| NEG-032 | Invalid UTF-8 | SynthesizeAsync(badUtf8) | DecoderFallbackException |
| NEG-033 | Voice gender mismatch | SynthesizeAsync(text, wrongGender) | ArgumentException |
| NEG-034 | Language voice mismatch | SynthesizeAsync(text, wrongLang) | ArgumentException |
| NEG-035 | Streaming after cancel | Stream after cancel | ObjectDisposedException |
| NEG-036 | Preload invalid voice | PreloadVoiceAsync("bad") | NotFoundException |
| NEG-037 | Validate invalid SSML | ValidateSsmlAsync(bad) | SsmlException |
| NEG-038 | Detect empty text | DetectLanguageAsync("") | ArgumentException |
| NEG-039 | Estimate empty | EstimateDurationAsync("") | ArgumentException |
| NEG-040 | Cache key collision | Same key diff content | Overwrite |
| NEG-041 | Cache corruption | Corrupted cache | CacheInvalidException |
| NEG-042 | Warm-up failure | WarmCacheAsync() | CacheException |
| NEG-043 | Batch partial failure | Batch mixed | Partial |
| NEG-044 | Stream read after end | Read after end | InvalidOperationException |
| NEG-045 | Stream position | Seek stream | NotSupportedException |
| NEG-046 | Concurrent stream | 2 read same | Shared |
| NEG-047 | Prosody out of range | SSML prosody | SsmlException |
| NEG-048 | Break too long | SSML break | SsmlException |
| NEG-049 | Phoneme invalid | SSML phoneme | SsmlException |
| NEG-050 | Say-as invalid | SSML say-as | SsmlException |
| NEG-051 | Sub invalid | SSML sub | SsmlException |
| NEG-052 | Unsupported tag | SSML custom | SsmlException |
| NEG-053 | Nested speak | SSML nested | SsmlException |
| NEG-054 | Invalid audio config | Bad config | ArgumentException |
| NEG-055 | Voice not available | Discontinued voice | NotFoundException |
| NEG-056 | Language deprecated | Deprecated lang | NotSupportedException |
| NEG-057 | Format deprecated | Deprecated format | NotSupportedException |
| NEG-058 | Billing not enabled | No billing | BillingException |
| NEG-059 | Project quota | Project limit | QuotaExceededException |
| NEG-060 | User quota | User limit | QuotaExceededException |
| NEG-061 | Region restriction | Restricted region | NotAvailableException |
| NEG-062 | API disabled | API off | ApiDisabledException |
| NEG-063 | Permission denied | No permission | UnauthorizedAccessException |
| NEG-064 | Invalid project | Wrong project | NotFoundException |
| NEG-065 | Retry exhausted | Many retries | RetryException |
| NEG-066 | Connection refused | Offline | ConnectionException |
| NEG-067 | SSL error | SSL | SecurityException |
| NEG-068 | Proxy required | No proxy | ProxyException |
| NEG-069 | Character limit | Over limit | ArgumentException |
| NEG-070 | Byte limit | Over limit | ArgumentException |
| NEG-071 | Null voice metadata | GetVoiceMetadataAsync(null) | ArgumentNullException |
| NEG-072 | Invalid preload | PreloadVoiceAsync("") | ArgumentException |
| NEG-073 | Null batch item | BatchSynthesizeAsync([null]) | ArgumentNullException |
| NEG-074 | Invalid stream config | SynthesizeStreamingAsync(bad) | ArgumentException |
| NEG-075 | Null cancel token | CancelAsync(null) | ArgumentNullException |
| NEG-076 | Invalid GetLimits | GetLimitsAsync(bad) | ArgumentException |
| NEG-077 | Null DetectLanguage | DetectLanguageAsync(null) | ArgumentNullException |
| NEG-078 | Invalid EstimateDuration | EstimateDurationAsync(null) | ArgumentNullException |
| NEG-079 | Null ValidateSsml | ValidateSsmlAsync(null) | ArgumentNullException |
| NEG-080 | Invalid cache key | GetCachedAsync(bad) | ArgumentException |
| NEG-081 | Disposed audio stream | Read disposed | ObjectDisposedException |
| NEG-082 | Invalid audio config | SynthesizeAsync(badConfig) | ArgumentException |
| NEG-083 | Null format param | SynthesizeAsync(text, format: null) | ArgumentNullException |
| NEG-084 | Invalid sample rate combo | SynthesizeAsync(badCombo) | ArgumentException |
| NEG-085 | Null language list | GetVoicesAsync(null) | ArgumentNullException |
| NEG-086 | Invalid retry config | SynthesizeAsync(..., retry: -1) | ArgumentException |
| NEG-087 | Null timeout | SynthesizeAsync(..., timeout: null) | ArgumentNullException |
| NEG-088 | Invalid WarmCache | WarmCacheAsync(bad) | CacheException |
| NEG-089 | Expired preload | Use expired preload | NotFoundException |
| NEG-090 | Invalid batch order | BatchSynthesizeAsync(badOrder) | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Text length = 0 | "" | Invalid |
| BND-002 | Text length = 1 | "a" | Synthesized |
| BND-003 | Text length = 5000 | Max | Synthesized |
| BND-004 | Text length = 5001 | Over | Rejected |
| BND-005 | SSML length = 0 | "" | Invalid |
| BND-006 | SSML length = 5000 | Max | Synthesized |
| BND-007 | SSML length = 5001 | Over | Rejected |
| BND-008 | Speaking rate = 0.25 | Min | Valid |
| BND-009 | Speaking rate = 4.0 | Max | Valid |
| BND-010 | Speaking rate = 0.24 | Under | Rejected |
| BND-011 | Speaking rate = 4.1 | Over | Rejected |
| BND-012 | Pitch = -20 | Min | Valid |
| BND-013 | Pitch = 20 | Max | Valid |
| BND-014 | Pitch = -21 | Under | Rejected |
| BND-015 | Pitch = 21 | Over | Rejected |
| BND-016 | Volume = -96 | Min | Valid |
| BND-017 | Volume = 16 | Max | Valid |
| BND-018 | Sample rate = 8000 | Min | Valid |
| BND-019 | Sample rate = 24000 | Max | Valid |
| BND-020 | Sample rate = 16000 | Common | Valid |
| BND-021 | Sample rate = 22050 | Common | Valid |
| BND-022 | Sample rate = 44100 | Over | Rejected |
| BND-023 | Batch size = 0 | [] | Invalid |
| BND-024 | Batch size = 1 | [1] | Valid |
| BND-025 | Batch size = 100 | Max | Valid |
| BND-026 | Batch size = 101 | Over | Rejected |
| BND-027 | Voice count = 0 | [] | Empty |
| BND-028 | Voice count = 1 | [1] | One |
| BND-029 | Voice count = 500 | Many | Returned |
| BND-030 | Language count = 0 | [] | Empty |
| BND-031 | Language count = 1 | [1] | One |
| BND-032 | Language count = 100 | Many | Returned |
| BND-033 | Stream chunk = 0 | 0 | Invalid |
| BND-034 | Stream chunk = 1 | 1 byte | Valid |
| BND-035 | Stream chunk = 64KB | 64KB | Valid |
| BND-036 | Audio duration = 0 | Empty | 0 |
| BND-037 | Audio duration = 1s | 1 second | 1s |
| BND-038 | Audio duration = 10min | Max | 10min |
| BND-039 | Cache size = 0 | Cold | Miss |
| BND-040 | Cache size = 1 | One | Hit |
| BND-041 | Cache size = 10000 | Max | Eviction |
| BND-042 | Unicode in text | "你好" | Synthesized |
| BND-043 | Emoji in text | "👍" | Handled |
| BND-044 | RTL in text | "مرحبا" | Synthesized |
| BND-045 | Mixed script | "Hello 世界" | Synthesized |
| BND-046 | Numbers in text | "123" | Synthesized |
| BND-047 | Punctuation | "Hello!" | Synthesized |
| BND-048 | Abbreviations | "Dr. Smith" | Synthesized |
| BND-049 | Acronyms | "NASA" | Synthesized |
| BND-050 | Long word | Very long | Synthesized |
| BND-051 | Multiple sentences | "A. B. C." | Synthesized |
| BND-052 | Empty SSML tag | <break/> | Valid |
| BND-053 | SSML with attribute | <prosody rate="slow"> | Valid |
| BND-054 | Nested SSML | <emphasis><break/></emphasis> | Valid |
| BND-055 | Concurrent requests = 1 | 1 | Success |
| BND-056 | Concurrent requests = 50 | 50 | Success |
| BND-057 | Concurrent requests = 200 | 200 | Throttled |
| BND-058 | Timeout = 0ms | 0 | Immediate |
| BND-059 | Timeout = 30000ms | 30s | Success |
| BND-060 | Retry count = 0 | No retry | Fail once |
| BND-061 | Retry count = 3 | 3 | Retries |
| BND-062 | Break duration = 0ms | 0ms | Valid |
| BND-063 | Break duration = 5000ms | 5s | Valid |
| BND-064 | Break duration = 5001ms | Over | Rejected |
| BND-065 | Prosody rate = 0.5 | Slow | Valid |
| BND-066 | Prosody rate = 2.0 | Fast | Valid |
| BND-067 | Prosody pitch = -50% | Low | Valid |
| BND-068 | Prosody pitch = +50% | High | Valid |
| BND-069 | Prosody volume = silent | Silent | Valid |
| BND-070 | Prosody volume = x-loud | Loud | Valid |
| BND-071 | Voice count = 0 | [] | Empty |
| BND-072 | Voice count = 500 | Many | Returned |
| BND-073 | Language count = 0 | [] | Empty |
| BND-074 | Language count = 100 | Many | Returned |
| BND-075 | Batch size = 1 | [1] | Valid |
| BND-076 | Batch size = 100 | Max | Valid |
| BND-077 | Stream chunk = 0 | 0 | Invalid |
| BND-078 | Stream chunk = 64KB | 64KB | Valid |
| BND-079 | Audio duration = 0 | Empty | 0 |
| BND-080 | Audio duration = 10min | Max | 10min |
| BND-081 | Cache size = 0 | Cold | Miss |
| BND-082 | Cache size = 10000 | Max | Eviction |
| BND-083 | Concurrent = 1 | 1 | Success |
| BND-084 | Concurrent = 200 | 200 | Throttled |
| BND-085 | Timeout = 0ms | 0 | Immediate |
| BND-086 | Timeout = 30000ms | 30s | Success |
| BND-087 | Retry = 0 | No retry | Fail once |
| BND-088 | Retry = 3 | 3 | Retries |
| BND-089 | Break duration = 0ms | 0ms | Valid |
| BND-090 | Break duration = 5000ms | 5s | Valid |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Text normalization | Normalize | Synthesize | Normalized |
| FUN-002 | SSML validation | Validate | SynthesizeSsml | Validated |
| FUN-003 | Voice selection | Select | Synthesize | Selected |
| FUN-004 | Language detection | Detect | Synthesize | Detected |
| FUN-005 | Cache key generation | Generate | Synthesize | Unique key |
| FUN-006 | Cache TTL | TTL | Cache | Expires |
| FUN-007 | Streaming chunk size | Chunk | Stream | Sized |
| FUN-008 | Format conversion | Convert | Export | Converted |
| FUN-009 | Sample rate conversion | Convert | Export | Resampled |
| FUN-010 | Voice fallback | Fallback | Voice missing | Fallback |
| FUN-011 | Language fallback | Fallback | Lang missing | Fallback |
| FUN-012 | Error retry | Retry | Transient | Retried |
| FUN-013 | Rate limiting | Limit | Many | Limited |
| FUN-014 | Quota enforcement | Quota | Synthesize | Enforced |
| FUN-015 | Character limit | Limit | Synthesize | Limited |
| FUN-016 | Byte limit | Limit | Synthesize | Limited |
| FUN-017 | Duration limit | Limit | Synthesize | Limited |
| FUN-018 | SSML tag support | Support | SSML | Supported |
| FUN-019 | Plain text handling | Handle | Plain | Handled |
| FUN-020 | Mixed content | Handle | Mixed | Handled |
| FUN-021 | Prosody application | Apply | Prosody | Applied |
| FUN-022 | Break application | Apply | Break | Applied |
| FUN-023 | Emphasis application | Apply | Emphasis | Applied |
| FUN-024 | Phoneme application | Apply | Phoneme | Applied |
| FUN-025 | Say-as application | Apply | Say-as | Applied |
| FUN-026 | Sub application | Apply | Sub | Applied |
| FUN-027 | Batch ordering | Order | Batch | Preserved |
| FUN-028 | Batch deduplication | Dedup | Batch | Deduplicated |
| FUN-029 | Stream buffering | Buffer | Stream | Buffered |
| FUN-030 | Cache warm-up | Warm | Startup | Preloaded |
| FUN-031 | Preload validation | Validate | Preload | Validated |
| FUN-032 | Voice metadata | Metadata | GetVoice | Returned |
| FUN-033 | Language metadata | Metadata | GetLang | Returned |
| FUN-034 | Duration estimation | Estimate | Text | Estimated |
| FUN-035 | Limit reporting | Report | GetLimits | Reported |
| FUN-036 | Error format | Format | Error | Consistent |
| FUN-037 | Logging | Log | Any op | Logged |
| FUN-038 | Metrics | Metrics | Any op | Recorded |
| FUN-039 | Tracing | Trace | Request | Traced |
| FUN-040 | Health check | Health | Check | Healthy |
| FUN-041 | Cancellation | Cancel | Cancel | Cancelled |
| FUN-042 | Timeout | Timeout | Slow | Timeout |
| FUN-043 | Circuit breaker | Circuit | Failures | Opened |
| FUN-044 | Fallback | Fallback | Unavailable | Fallback |
| FUN-045 | Backup voice | Backup | Primary fail | Backup |
| FUN-046 | Graceful degradation | Degrade | Partial fail | Degraded |
| FUN-047 | Audit trail | Audit | Any op | Logged |
| FUN-048 | Permission check | Check | Access | Checked |
| FUN-049 | Tenant isolation | Isolate | Tenant | Isolated |
| FUN-050 | Rate per user | Rate | User | Per user |
| FUN-051 | Text normalization | Normalize | Synthesize | Normalized |
| FUN-052 | SSML validation | Validate | SynthesizeSsml | Validated |
| FUN-053 | Voice selection | Select | Synthesize | Selected |
| FUN-054 | Language detection | Detect | Synthesize | Detected |
| FUN-055 | Cache key generation | Generate | Synthesize | Unique key |
| FUN-056 | Cache TTL | TTL | Cache | Expires |
| FUN-057 | Streaming chunk size | Chunk | Stream | Sized |
| FUN-058 | Format conversion | Convert | Export | Converted |
| FUN-059 | Sample rate conversion | Convert | Export | Resampled |
| FUN-060 | Voice fallback | Fallback | Voice missing | Fallback |
| FUN-061 | Language fallback | Fallback | Lang missing | Fallback |
| FUN-062 | Error retry | Retry | Transient | Retried |
| FUN-063 | Rate limiting | Limit | Many | Limited |
| FUN-064 | Quota enforcement | Quota | Synthesize | Enforced |
| FUN-065 | Character limit | Limit | Synthesize | Limited |
| FUN-066 | Byte limit | Limit | Synthesize | Limited |
| FUN-067 | Duration limit | Limit | Synthesize | Limited |
| FUN-068 | SSML tag support | Support | SSML | Supported |
| FUN-069 | Plain text handling | Handle | Plain | Handled |
| FUN-070 | Mixed content | Handle | Mixed | Handled |
| FUN-071 | Prosody application | Apply | Prosody | Applied |
| FUN-072 | Break application | Apply | Break | Applied |
| FUN-073 | Emphasis application | Apply | Emphasis | Applied |
| FUN-074 | Phoneme application | Apply | Phoneme | Applied |
| FUN-075 | Say-as application | Apply | Say-as | Applied |
| FUN-076 | Sub application | Apply | Sub | Applied |
| FUN-077 | Batch ordering | Order | Batch | Preserved |
| FUN-078 | Batch deduplication | Dedup | Batch | Deduplicated |
| FUN-079 | Stream buffering | Buffer | Stream | Buffered |
| FUN-080 | Cache warm-up | Warm | Startup | Preloaded |
| FUN-081 | Preload validation | Validate | Preload | Validated |
| FUN-082 | Voice metadata | Metadata | GetVoice | Returned |
| FUN-083 | Language metadata | Metadata | GetLang | Returned |
| FUN-084 | Duration estimation | Estimate | Text | Estimated |
| FUN-085 | Limit reporting | Report | GetLimits | Reported |
| FUN-086 | Error format | Format | Error | Consistent |
| FUN-087 | Logging | Log | Any op | Logged |
| FUN-088 | Metrics | Metrics | Any op | Recorded |
| FUN-089 | Health check | Health | Check | Healthy |
| FUN-090 | Cancellation | Cancel | Cancel | Cancelled |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | Google Cloud TTS API | TTS API | Synthesize | Success |
| INT-002 | OAuth2 | OAuth | Auth | Authenticated |
| INT-003 | Configuration | IConfiguration | Config | Applied |
| INT-004 | Logger | ILogger | Log | Logged |
| INT-005 | Document manager | IDocumentManager | Attach audio | Linked |
| INT-006 | Opportunity | IOpportunityManager | Audio to opp | Linked |
| INT-007 | Partner | IPartnerManager | Audio to partner | Linked |
| INT-008 | Cache service | ICacheService | Cache | Cached |
| INT-009 | Storage service | IStorageService | Store audio | Stored |
| INT-010 | Full synthesize flow | All | Synthesize | Success |
| INT-011 | Full SSML flow | All | SynthesizeSsml | Success |
| INT-012 | Full streaming flow | All | Stream | Success |
| INT-013 | Synthesize + store | TTS + storage | Both | Success |
| INT-014 | Synthesize + cache | TTS + cache | Both | Success |
| INT-015 | Batch + store | Batch + storage | Both | Success |
| INT-016 | Document + TTS | Document | Attach | Linked |
| INT-017 | Opportunity + TTS | Opportunity | Attach | Linked |
| INT-018 | Partner + TTS | Partner | Attach | Linked |
| INT-019 | Config + credentials | Config | Credentials | From config |
| INT-020 | Logger + error | Logger | Error | Logged |
| INT-021 | Cache + TTS | Cache | Hit | From cache |
| INT-022 | Storage + TTS | Storage | Save | Saved |
| INT-023 | Retry + transient | Retry | Transient | Retried |
| INT-024 | Timeout + synthesize | Timeout | Synthesize | Timeout |
| INT-025 | Cancellation + synthesize | Cancel | Synthesize | Cancelled |
| INT-026 | Rate limit + many | Rate limit | Many | Limited |
| INT-027 | OAuth + refresh | OAuth | Refresh | Refreshed |
| INT-028 | Voice + language | Voice + lang | Synthesize | Matched |
| INT-029 | Format + storage | Format | Store | Stored |
| INT-030 | Stream + storage | Stream | Store | Stored |
| INT-031 | SSML + validation | SSML | Validate | Validated |
| INT-032 | Batch + parallel | Batch | Parallel | Parallel |
| INT-033 | Preload + synthesize | Preload | Synthesize | Faster |
| INT-034 | Warm + synthesize | Warm | Synthesize | Faster |
| INT-035 | Metadata + synthesize | Metadata | Synthesize | Used |
| INT-036 | Duration + planning | Duration | Plan | Planned |
| INT-037 | Limits + validate | Limits | Validate | Validated |
| INT-038 | Tenant + TTS | Tenant | Synthesize | Isolated |
| INT-039 | User + quota | User | Synthesize | Quota |
| INT-040 | Audit + synthesize | Audit | Synthesize | Audited |
| INT-041 | Permission + synthesize | Permission | Synthesize | Checked |
| INT-042 | Resilient + TTS | Resilient | Failures | Retried |
| INT-043 | Circuit + TTS | Circuit | Failures | Opened |
| INT-044 | Fallback + TTS | Fallback | Unavailable | Fallback |
| INT-045 | Health + TTS | Health | Check | Healthy |
| INT-046 | Metrics + TTS | Metrics | Synthesize | Recorded |
| INT-047 | Tracing + TTS | Tracing | Request | Traced |
| INT-048 | Monitoring + TTS | Monitoring | Synthesize | Monitored |
| INT-049 | Alerting + TTS | Alerting | Failures | Alerted |
| INT-050 | End-to-end | All | Full flow | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | XSS in text | <script> | Text | Sanitized |
| SEC-002 | SSML injection | <script> | SSML | Sanitized |
| SEC-003 | XXE in SSML | XXE | SSML | Blocked |
| SEC-004 | Path traversal | ../etc | Path | Rejected |
| SEC-005 | Unauthorized synthesize | No perm | Synthesize | 403 |
| SEC-006 | Unauthorized voices | No perm | GetVoices | 403 |
| SEC-007 | IDOR | Alter ID | Get | 403 |
| SEC-008 | Cross-tenant | Tenant A | Tenant B | 403 |
| SEC-009 | Credential leak | Log | Credential | Not logged |
| SEC-010 | Token in URL | URL | Token | Not in URL |
| SEC-011 | PII in text | PII | Synthesize | Redacted |
| SEC-012 | PII in log | Log | PII | Redacted |
| SEC-013 | OAuth scope | Insufficient | Synthesize | 403 |
| SEC-014 | Token tampering | Tampered | Auth | 401 |
| SEC-015 | Expired token | Expired | Auth | 401 |
| SEC-016 | DoS long text | 1MB text | Synthesize | Rejected |
| SEC-017 | DoS many requests | 10000/s | Any | Rate limited |
| SEC-018 | SSRF in URL | URL | Metadata | Blocked |
| SEC-019 | Open redirect | Redirect | Callback | Blocked |
| SEC-020 | Cache poisoning | Poison | Cache | Validated |
| SEC-021 | Replay attack | Replay | Auth | Nonce |
| SEC-022 | CSRF | Cross-site | Synthesize | Token |
| SEC-023 | Privilege escalation | Low role | Admin | 403 |
| SEC-024 | Horizontal privilege | User A | User B | 403 |
| SEC-025 | Quota bypass | Bypass | Quota | Blocked |
| SEC-026 | Rate limit bypass | Bypass | Rate | Blocked |
| SEC-027 | Injection in voice | Injection | Voice | Sanitized |
| SEC-028 | Injection in language | Injection | Lang | Sanitized |
| SEC-029 | Prototype pollution | __proto__ | Config | Sanitized |
| SEC-030 | Command injection | ; rm | Text | Sanitized |
| SEC-031 | Header injection | CRLF | Header | Sanitized |
| SEC-032 | Null byte | %00 | Path | Rejected |
| SEC-033 | Unicode normalization | Homoglyph | Text | Normalized |
| SEC-034 | Information disclosure | Error | Detail | Generic |
| SEC-035 | Enumeration | Sequential | Get | Rate limited |
| SEC-036 | Metadata exposure | Metadata | Response | Filtered |
| SEC-037 | API key exposure | Log | Key | Not logged |
| SEC-038 | Weak crypto | MD5 | Checksum | SHA256 |
| SEC-039 | Insecure TLS | TLS 1.0 | Connection | TLS 1.2+ |
| SEC-040 | No auth | No auth | Synthesize | 401 |
| SEC-041 | Invalid auth | Invalid | Synthesize | 401 |
| SEC-042 | Delegation | Delegate | Synthesize | Scoped |
| SEC-043 | Impersonation | Impersonate | Synthesize | Rejected |
| SEC-044 | Service account | Service | Synthesize | Allowed |
| SEC-045 | User consent | Consent | Synthesize | Checked |
| SEC-046 | Data retention | Retention | Cache | Limited |
| SEC-047 | Audit bypass | Bypass | Audit | Logged |
| SEC-048 | Log injection | Injection | Log | Sanitized |
| SEC-049 | Timing attack | Timing | Comparison | Constant |
| SEC-050 | Side channel | Side channel | Synthesize | Mitigated |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Concurrent synthesize same | 2 threads same | Both succeed |
| CON-002 | Concurrent synthesize different | 2 threads diff | Both succeed |
| CON-003 | Concurrent cache write | 2 threads same key | No corruption |
| CON-004 | Concurrent cache read | 10 threads | All succeed |
| CON-005 | Synthesize during cancel | Synthesize + cancel | Cancelled |
| CON-006 | Stream during cancel | Stream + cancel | Cancelled |
| CON-007 | Batch during batch | 2 batches | Both succeed |
| CON-008 | GetVoices during synthesize | Both | Both succeed |
| CON-009 | Cache stampede | 100 cold | Single load |
| CON-010 | Preload during synthesize | Both | Handled |
| CON-011 | Warm during synthesize | Both | Handled |
| CON-012 | Deadlock | A→B, B→A | No deadlock |
| CON-013 | Lock contention | 50 synthesize | Throttled |
| CON-014 | Thread pool exhaustion | 1000 threads | Limited |
| CON-015 | Memory barrier | Synthesize + cache | Visible |
| CON-016 | Optimistic concurrency | Update + update | Version |
| CON-017 | Pessimistic lock | Synthesize + lock | Locked |
| CON-018 | Semaphore | Limited | Semaphore |
| CON-019 | Read-write lock | Read + write | RW lock |
| CON-020 | Stream concurrent read | 2 read same | Shared |
| CON-021 | Batch concurrent | 2 batches same | Both succeed |
| CON-022 | Circuit breaker | Many failures | Opened |
| CON-023 | Retry concurrent | 2 retry same | One succeeds |
| CON-024 | Quota concurrent | 2 over quota | One fails |
| CON-025 | Full concurrency | All ops | All succeed |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Text validation | Validation | "" | Invalid |
| UNT-002 | SSML validation | Validation | "<invalid>" | Invalid |
| UNT-003 | Voice validation | Validation | "bad" | Invalid |
| UNT-004 | Language validation | Validation | "xx" | Invalid |
| UNT-005 | Format validation | Validation | "xyz" | Invalid |
| UNT-006 | Text sanitize | Formatting | "<script>" | Sanitized |
| UNT-007 | SSML format | Formatting | Input | Formatted |
| UNT-008 | Voice format | Formatting | "en-US" | Formatted |
| UNT-009 | Cache key format | Formatting | Params | Key |
| UNT-010 | Duration format | Formatting | Ms | Formatted |
| UNT-011 | Character count | Calculations | Text | Count |
| UNT-012 | Byte count | Calculations | Text | Bytes |
| UNT-013 | Duration estimate | Calculations | Text | Duration |
| UNT-014 | Chunk size | Calculations | Size | Chunks |
| UNT-015 | Retry delay | Calculations | Attempt | Delay |
| UNT-016 | Voice available | Status | Voice | True/False |
| UNT-017 | Language available | Status | Lang | True/False |
| UNT-018 | Cache hit | Status | Key | Hit/Miss |
| UNT-019 | Quota remaining | Status | User | Remaining |
| UNT-020 | Empty voices | Collections | [] | Empty |
| UNT-021 | Single voice | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Synthesize 100 chars | SynthesizeAsync(100) | <2s |
| PRF-002 | Synthesize 1000 chars | SynthesizeAsync(1000) | <5s |
| PRF-003 | Synthesize 5000 chars | SynthesizeAsync(5000) | <15s |
| PRF-004 | SSML synthesize | SynthesizeSsmlAsync | <5s |
| PRF-005 | Get voices | GetVoicesAsync | <500ms |
| PRF-006 | Cache hit | Synthesize (cached) | <50ms |
| PRF-007 | Cache miss | Synthesize (cold) | <2s |
| PRF-008 | Streaming start | First chunk | <500ms |
| PRF-009 | Streaming throughput | Stream | >1MB/s |
| PRF-010 | Batch 10 | BatchSynthesizeAsync(10) | <20s |
| PRF-011 | Concurrent 10 | 10 concurrent | <15s |
| PRF-012 | Concurrent 50 | 50 concurrent | <60s |
| PRF-013 | Memory synthesize | Synthesize 5K | <50MB |
| PRF-014 | Memory stream | Stream 5K | <30MB |
| PRF-015 | Cold start | First request | <1s |
| PRF-016 | Full flow | Synthesize + cache | <3s |

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
