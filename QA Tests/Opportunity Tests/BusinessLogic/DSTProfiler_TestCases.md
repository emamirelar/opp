# DST Profiler — Test Cases

**Component:** Opportunity DST (Decision Support Tool) Profiler  
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

**Ratio Checks:**
- N≥3P: 90≥90 ✅ PASS
- E≥3P: 90≥90 ✅ PASS
- F≥3P: 90≥90 ✅ PASS
- I≥3P: 90≥90 ✅ PASS

---

## Feature Overview

Decision Support Tool profiler for opportunities. Analyzes country/region indicators (Fragile States Index, HDI, Corruption Perceptions, Political Stability, Ease of Doing Business, Global Peace Index), generates risk profiles, calculates composite scores, visualizes data, compares regions, tracks historical trends, maps to opportunity risk categories, and produces PDF reports.

---

## §1 Positive — 30

| ID | Test | Expected | Pr |
|----|------|----------|----|
| POS-001 | Load DST profile for country | All indices loaded | P0 |
| POS-002 | Calculate composite score | Weighted score calculated | P0 |
| POS-003 | Generate risk profile | Risk categories assigned | P0 |
| POS-004 | Link profile to opportunity | Profile associated | P0 |
| POS-005 | Export PDF report | Valid PDF with charts | P0 |
| POS-006 | Load Fragile States Index | FSI data loaded | P1 |
| POS-007 | Load HDI | HDI data loaded | P1 |
| POS-008 | Load Corruption Index | CPI data loaded | P1 |
| POS-009 | Load Political Stability | Data loaded | P1 |
| POS-010 | Load Ease of Business | Data loaded | P1 |
| POS-011 | Load Global Peace Index | GPI data loaded | P1 |
| POS-012 | Compare two countries | Side-by-side comparison | P1 |
| POS-013 | Compare regions | Regional aggregation | P1 |
| POS-014 | Historical trend (5yr) | Trend data displayed | P1 |
| POS-015 | Risk category mapping | High/Medium/Low assigned | P1 |
| POS-016 | Update index data | Data refreshed | P1 |
| POS-017 | Custom weighting | User-defined weights | P1 |
| POS-018 | Visualization data | Chart-ready data | P1 |
| POS-019 | Search countries | Name search works | P1 |
| POS-020 | Filter by region | Regional filter | P1 |
| POS-021 | Filter by risk level | Risk filter | P1 |
| POS-022 | Paginate countries | Paginated results | P2 |
| POS-023 | Sort by score | Score-sorted list | P2 |
| POS-024 | Sort by name | Alpha sort | P2 |
| POS-025 | Get supported indices | List of all indices | P2 |
| POS-026 | Cache profile | Profile cached | P2 |
| POS-027 | Audit trail | Profile access logged | P2 |
| POS-028 | Multiple indices per opp | All indices linked | P1 |
| POS-029 | Refresh cached data | Cache invalidated | P2 |
| POS-030 | Get index metadata | Description, source, year | P2 |

---

## §2 Negative — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| NEG-001 | Null country code | 400, validation error | P0 |
| NEG-002 | Non-existent country | 404, not found | P0 |
| NEG-003 | Deleted country | 404, not found | P0 |
| NEG-004 | Invalid ISO code (2-char) | 400, invalid format | P0 |
| NEG-005 | Invalid ISO code (3-char) | 400, invalid format | P0 |
| NEG-006 | Null opportunity ID | 400, validation error | P0 |
| NEG-007 | Invalid indices list | 400, invalid indices | P1 |
| NEG-008 | Null weights object | 400, weights required | P1 |
| NEG-009 | Negative weight value | 400, weight must be ≥0 | P1 |
| NEG-010 | Weight > 100% | 400, weight must be ≤100 | P1 |
| NEG-011 | Weights sum ≠ 100% | 400, weights must sum to 100 | P1 |
| NEG-012 | Unauthenticated request | 401, unauthorized | P0 |
| NEG-013 | Expired token | 401, token expired | P0 |
| NEG-014 | Invalid token | 401, invalid token | P0 |
| NEG-015 | Missing permission | 403, forbidden | P0 |
| NEG-016 | Wrong role for DST | 403, insufficient role | P0 |
| NEG-017 | Cross-tenant access | 403, forbidden | P0 |
| NEG-018 | Deleted user access | 401, user invalid | P1 |
| NEG-019 | Disabled user access | 403, account disabled | P1 |
| NEG-020 | Session expired mid-request | 401, re-authenticate | P1 |
| NEG-021 | Missing FSI data | 404 or fallback, no FSI | P1 |
| NEG-022 | Missing HDI data | 404 or fallback, no HDI | P1 |
| NEG-023 | Stale index data | Warning or refresh prompt | P1 |
| NEG-024 | No historical data | 404, no history | P1 |
| NEG-025 | Corrupted index value | Error, invalid data | P1 |
| NEG-026 | Partial index data | Partial result or error | P1 |
| NEG-027 | Future year requested | 400, year not available | P1 |
| NEG-028 | Negative composite score | Error or validation | P1 |
| NEG-029 | NaN score in calculation | Error, invalid result | P1 |
| NEG-030 | Division by zero in weights | 500 or validation error | P1 |
| NEG-031 | SQL injection in country | 400, sanitized/rejected | P0 |
| NEG-032 | SQL injection in search | 400, sanitized/rejected | P0 |
| NEG-033 | XSS in country name | Escaped, no script exec | P0 |
| NEG-034 | XSS in report title | Escaped, no script exec | P0 |
| NEG-035 | Path traversal in export | 400, invalid path | P0 |
| NEG-036 | Command injection | 400, rejected | P0 |
| NEG-037 | Template injection | Sanitized, no exec | P1 |
| NEG-038 | LDAP injection | 400, rejected | P1 |
| NEG-039 | HTML injection | Escaped, safe output | P1 |
| NEG-040 | JSON injection | 400, invalid payload | P1 |
| NEG-041 | External index API down | Graceful degradation | P0 |
| NEG-042 | API timeout | Timeout error, retry option | P0 |
| NEG-043 | Rate limit exceeded | 429, retry-after | P1 |
| NEG-044 | Database error | 500, generic error | P0 |
| NEG-045 | Cache failure | Fallback to DB/API | P1 |
| NEG-046 | Memory OOM during batch | 500 or partial result | P1 |
| NEG-047 | Service unavailable | 503, retry later | P1 |
| NEG-048 | PDF generation failure | 500, retry option | P1 |
| NEG-049 | Invalid entity ID format | 400, invalid ID | P1 |
| NEG-050 | Negative pagination offset | 400, invalid offset | P1 |
| NEG-051 | Page size > max | 400, exceeds limit | P1 |
| NEG-052 | Invalid sort field | 400, unknown field | P1 |
| NEG-053 | Mass assignment | 400, extra fields ignored | P1 |
| NEG-054 | Empty country list | 400, at least one required | P1 |
| NEG-055 | Empty indices list | 400, indices required | P1 |
| NEG-056 | Duplicate country in compare | 400 or deduplicated | P2 |
| NEG-057 | Invalid region code | 400, unknown region | P2 |
| NEG-058 | Year before min supported | 400, year out of range | P2 |
| NEG-059 | Year after max supported | 400, year out of range | P2 |
| NEG-060 | Invalid weight key | 400, unknown index | P2 |
| NEG-061 | Malformed request body | 400, parse error | P0 |
| NEG-062 | Wrong content-type | 415, unsupported | P1 |
| NEG-063 | Oversized payload | 413, payload too large | P1 |
| NEG-064 | Missing required header | 400, header required | P2 |
| NEG-065 | Invalid Accept header | 406, not acceptable | P2 |
| NEG-066 | Empty search term | 400 or empty result | P2 |
| NEG-067 | Search term too long | 400, truncate or reject | P2 |
| NEG-068 | Invalid date range | 400, invalid range | P2 |
| NEG-069 | Null optional params | Handled, defaults used | P2 |
| NEG-070 | Concurrent conflicting update | 409, conflict | P1 |
| NEG-071 | Soft-deleted opportunity | 404, not found | P1 |
| NEG-072 | Soft-deleted country | 404, not found | P1 |
| NEG-073 | Invalid comparison count | 400, max N countries | P2 |
| NEG-074 | Circular dependency in weights | 400, invalid config | P2 |
| NEG-075 | Reserved country code | 400, reserved | P2 |
| NEG-076 | Invalid chart type | 400, unsupported type | P2 |
| NEG-077 | Export format unsupported | 400, format not supported | P2 |
| NEG-078 | Cache key collision | Error or fallback | P2 |
| NEG-079 | Audit log write failure | Degraded, logged | P2 |
| NEG-080 | Invalid locale in export | 400 or default locale | P2 |
| NEG-081 | Missing index source | 404, source not found | P2 |
| NEG-082 | Deprecated API version | 400 or 301, upgrade | P2 |
| NEG-083 | CORS preflight failure | CORS error | P2 |
| NEG-084 | Request ID replay | 400 or idempotency | P2 |
| NEG-085 | Invalid UTF-8 in input | 400, encoding error | P2 |
| NEG-086 | Null byte in string | 400, sanitized | P2 |
| NEG-087 | Unicode normalization attack | Handled safely | P2 |
| NEG-088 | Extremely long string | 400, length exceeded | P2 |
| NEG-089 | Negative page number | 400, invalid page | P2 |
| NEG-090 | Zero page size | 400, min 1 required | P2 |

---

## §3 Boundary — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| BND-001 | Country count = 1 | Single profile | P0 |
| BND-002 | Country count = 50 | Batch load | P1 |
| BND-003 | Country count = 100 | Batch load | P1 |
| BND-004 | Country count = 195 | All countries | P1 |
| BND-005 | Country count = 196 | At UN member limit | P1 |
| BND-006 | Country count = 197+ | Max limit or paginate | P2 |
| BND-007 | FSI value = 0.0 | Min boundary | P1 |
| BND-008 | FSI value = 0.5 | Mid-low | P1 |
| BND-009 | FSI value = 1.0 | Mid-high | P1 |
| BND-010 | FSI value = min (index) | Index minimum | P1 |
| BND-011 | FSI value = max (index) | Index maximum | P1 |
| BND-012 | HDI value = 0.0 | Min boundary | P1 |
| BND-013 | HDI value = 0.5 | Mid | P1 |
| BND-014 | HDI value = 1.0 | Max boundary | P1 |
| BND-015 | CPI value at boundaries | Min/max per index | P1 |
| BND-016 | Score = 0 | Min score | P0 |
| BND-017 | Score = 50 | Mid score | P1 |
| BND-018 | Score = 100 | Max score | P0 |
| BND-019 | Score = 0.001 | Decimal precision | P1 |
| BND-020 | Score = 99.999 | Decimal precision | P1 |
| BND-021 | Score = -0.001 | Just below zero | P1 |
| BND-022 | Score = 100.001 | Just above max | P1 |
| BND-023 | Weight = 0% | Min weight | P1 |
| BND-024 | Weight = 1% | Low weight | P1 |
| BND-025 | Weight = 50% | Mid weight | P1 |
| BND-026 | Weight = 100% | Max weight | P1 |
| BND-027 | Weights sum = 99.99% | Just under 100 | P1 |
| BND-028 | Weights sum = 100.01% | Just over 100 | P1 |
| BND-029 | Weights sum = 100.00% | Exact | P0 |
| BND-030 | Year = 1990 | Min historical | P1 |
| BND-031 | Year = 2000 | Early 2000s | P1 |
| BND-032 | Year = 2020 | Recent | P1 |
| BND-033 | Year = 2026 | Current/future | P1 |
| BND-034 | Year = current | Latest available | P0 |
| BND-035 | Historical period = 1yr | Min period | P1 |
| BND-036 | Historical period = 5yr | Standard | P0 |
| BND-037 | Historical period = 10yr | Extended | P1 |
| BND-038 | Historical period = 20yr | Long | P1 |
| BND-039 | Historical period = 20yr+ | Max period | P2 |
| BND-040 | Compare 2 countries | Min comparison | P0 |
| BND-041 | Compare 5 countries | Small batch | P1 |
| BND-042 | Compare 10 countries | Medium batch | P1 |
| BND-043 | Compare 50 countries | Large batch | P1 |
| BND-044 | Compare max allowed | At limit | P2 |
| BND-045 | Chart data points = 1 | Min points | P2 |
| BND-046 | Chart data points = 100 | Standard | P1 |
| BND-047 | Chart data points = 1000 | Large | P2 |
| BND-048 | Unicode country name (Côte d'Ivoire) | Renders correctly | P1 |
| BND-049 | Unicode country name (São Tomé) | Renders correctly | P1 |
| BND-050 | Search term = 1 char | Min search | P2 |
| BND-051 | Search term = 255 chars | Max search | P2 |
| BND-052 | Pagination page = 1 | First page | P0 |
| BND-053 | Pagination page = last | Last page | P1 |
| BND-054 | Page size = 1 | Min page size | P2 |
| BND-055 | Page size = max | Max page size | P1 |
| BND-056 | Concurrent requests = 2 | Low concurrency | P1 |
| BND-057 | Concurrent requests = 10 | Medium concurrency | P1 |
| BND-058 | Concurrent requests = 50 | High concurrency | P2 |
| BND-059 | Empty result set | Empty array returned | P1 |
| BND-060 | Single index only | One index in profile | P1 |
| BND-061 | All indices | Full index set | P0 |
| BND-062 | Timestamp at epoch | Epoch handling | P2 |
| BND-063 | Timestamp at max | Max date handling | P2 |
| BND-064 | Nullable field = null | Null handled | P1 |
| BND-065 | Nullable field = value | Value used | P1 |
| BND-066 | Region with 1 country | Minimal region | P2 |
| BND-067 | Region with all countries | Full region | P2 |
| BND-068 | Zero weights for one index | Edge weighting | P2 |
| BND-069 | All weights equal | Uniform weighting | P1 |
| BND-070 | Single decimal place | Precision boundary | P2 |
| BND-071 | Six decimal places | Precision boundary | P2 |
| BND-072 | Timezone boundary (UTC) | Correct conversion | P2 |
| BND-073 | Timezone boundary (DST) | DST handling | P2 |
| BND-074 | Locale en-US | Default locale | P1 |
| BND-075 | Locale fr-FR | French locale | P2 |
| BND-076 | Locale ar-SA (RTL) | RTL handling | P2 |
| BND-077 | Cache TTL = 0 | No cache | P2 |
| BND-078 | Cache TTL = max | Max cache | P2 |
| BND-079 | Batch size = 1 | Min batch | P2 |
| BND-080 | Batch size = max | Max batch | P2 |
| BND-081 | Retry count = 0 | No retry | P2 |
| BND-082 | Retry count = max | Max retries | P2 |
| BND-083 | Rate limit at threshold | At limit | P2 |
| BND-084 | Rate limit just under | Below limit | P2 |
| BND-085 | Connection pool at min | Min connections | P2 |
| BND-086 | Connection pool at max | Max connections | P2 |
| BND-087 | Memory at low watermark | Low memory | P2 |
| BND-088 | Disk space at limit | Low disk | P2 |
| BND-089 | File size = 0 bytes | Empty file | P2 |
| BND-090 | File size = max allowed | Max file size | P2 |

---

## §4 Functional — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| FUN-001 | Weighted score formula | Correct weighted sum | P0 |
| FUN-002 | Equal weights | Average of indices | P0 |
| FUN-003 | Single index weight 100% | Score = index value | P0 |
| FUN-004 | Zero weight index | Excluded from score | P1 |
| FUN-005 | Mixed weights | Correct partial sum | P1 |
| FUN-006 | Weight normalization | Weights normalized | P1 |
| FUN-007 | Score rounding | Consistent rounding | P1 |
| FUN-008 | Score scaling 0–100 | Output in range | P0 |
| FUN-009 | Score scaling per index | Per-index scaling | P1 |
| FUN-010 | Missing index fallback | Fallback or exclude | P1 |
| FUN-011 | Score with 1 index | Single-index score | P1 |
| FUN-012 | Score with all indices | Full composite | P0 |
| FUN-013 | Score with partial indices | Partial composite | P1 |
| FUN-014 | Score precision | 2 decimal places | P1 |
| FUN-015 | Score ordering | Higher score = lower risk | P0 |
| FUN-016 | Risk High threshold | Score < X → High | P0 |
| FUN-017 | Risk Medium threshold | X ≤ Score < Y → Medium | P0 |
| FUN-018 | Risk Low threshold | Score ≥ Y → Low | P0 |
| FUN-019 | Risk at boundary values | Correct at threshold | P0 |
| FUN-020 | Risk with custom thresholds | Custom mapping | P1 |
| FUN-021 | Risk with missing data | Fallback risk level | P1 |
| FUN-022 | Risk color mapping | Correct colors | P2 |
| FUN-023 | Risk label i18n | Translated labels | P2 |
| FUN-024 | Risk for edge scores | Boundary handling | P1 |
| FUN-025 | Risk for zero score | Handled | P1 |
| FUN-026 | Load FSI from API | FSI data correct | P0 |
| FUN-027 | Load HDI from API | HDI data correct | P0 |
| FUN-028 | Load CPI from API | CPI data correct | P0 |
| FUN-029 | Load all indices | All data loaded | P0 |
| FUN-030 | Load with cache hit | Cached data returned | P1 |
| FUN-031 | Load with cache miss | Fresh data loaded | P1 |
| FUN-032 | Load with partial cache | Hybrid load | P1 |
| FUN-033 | Load historical range | Range correct | P1 |
| FUN-034 | Load by year | Year filter applied | P1 |
| FUN-035 | Load with fallback source | Fallback used | P2 |
| FUN-036 | Chart data structure | Valid chart format | P0 |
| FUN-037 | Chart labels | Correct labels | P1 |
| FUN-038 | Chart tooltips | Tooltip data | P1 |
| FUN-039 | Chart legend | Legend correct | P1 |
| FUN-040 | Chart export format | Exportable format | P1 |
| FUN-041 | Multi-series chart | Series correct | P1 |
| FUN-042 | Time-series chart | Time axis correct | P1 |
| FUN-043 | Comparison chart | Comparison data | P1 |
| FUN-044 | Empty chart | Empty state | P2 |
| FUN-045 | Chart accessibility | A11y compliant | P2 |
| FUN-046 | Audit profile access | Access logged | P0 |
| FUN-047 | Audit score calculation | Calc logged | P1 |
| FUN-048 | Audit export | Export logged | P1 |
| FUN-049 | Audit user ID | User captured | P0 |
| FUN-050 | Audit timestamp | Timestamp correct | P0 |
| FUN-051 | Profile link to opportunity | Link persisted | P0 |
| FUN-052 | Unlink profile | Link removed | P1 |
| FUN-053 | Multiple profiles per opp | All linked | P1 |
| FUN-054 | Profile versioning | Version tracked | P2 |
| FUN-055 | Profile diff | Diff correct | P2 |
| FUN-056 | PDF structure | Valid PDF | P0 |
| FUN-057 | PDF charts embedded | Charts in PDF | P0 |
| FUN-058 | PDF metadata | Metadata correct | P1 |
| FUN-059 | PDF filename | Filename format | P1 |
| FUN-060 | PDF localization | Localized content | P2 |
| FUN-061 | Search exact match | Exact result | P0 |
| FUN-062 | Search partial match | Partial results | P0 |
| FUN-063 | Search case insensitive | Case ignored | P1 |
| FUN-064 | Search with accents | Accent handling | P1 |
| FUN-065 | Filter combined | AND logic | P1 |
| FUN-066 | Sort ascending | Asc order | P0 |
| FUN-067 | Sort descending | Desc order | P0 |
| FUN-068 | Sort multi-column | Multi-sort | P1 |
| FUN-069 | Pagination offset | Correct offset | P0 |
| FUN-070 | Pagination total count | Total correct | P0 |
| FUN-071 | Regional aggregation | Region avg correct | P1 |
| FUN-072 | Percentile calculation | Percentile correct | P1 |
| FUN-073 | Historical trend slope | Trend correct | P1 |
| FUN-074 | Year-over-year change | YoY correct | P1 |
| FUN-075 | Index metadata | Metadata complete | P1 |
| FUN-076 | Supported indices list | List correct | P1 |
| FUN-077 | Index source URL | URL valid | P2 |
| FUN-078 | Index year | Year correct | P1 |
| FUN-079 | Model mapping | All fields mapped | P0 |
| FUN-080 | DTO serialization | JSON correct | P1 |
| FUN-081 | Batch processing order | Order preserved | P1 |
| FUN-082 | Error partial batch | Partial success | P1 |
| FUN-083 | Idempotent create | No duplicate | P1 |
| FUN-084 | Cache invalidation | Cache cleared | P1 |
| FUN-085 | Cache key uniqueness | Keys unique | P1 |
| FUN-086 | Stale-while-revalidate | SWR behavior | P2 |
| FUN-087 | Fallback chain | Fallback order | P2 |
| FUN-088 | Retry on transient | Retry works | P1 |
| FUN-089 | Timeout handling | Timeout respected | P1 |
| FUN-090 | Circuit breaker | Circuit opens | P2 |

---

## §5 Integration — 90

| ID | Test | Expected | Pr |
|----|------|----------|----|
| INT-001 | FSI API integration | FSI data from API | P0 |
| INT-002 | HDI API integration | HDI data from API | P0 |
| INT-003 | CPI API integration | CPI data from API | P0 |
| INT-004 | Political Stability API | Data from API | P1 |
| INT-005 | EoDB API integration | Data from API | P1 |
| INT-006 | GPI API integration | Data from API | P1 |
| INT-007 | API auth | Auth header sent | P0 |
| INT-008 | API retry | Retry on failure | P1 |
| INT-009 | API timeout | Timeout applied | P1 |
| INT-010 | API rate limit | Rate limit respected | P1 |
| INT-011 | Link profile to opportunity | DB link created | P0 |
| INT-012 | Load opportunity with profile | Profile loaded | P0 |
| INT-013 | Update opportunity profile | Profile updated | P1 |
| INT-014 | Delete opportunity cascade | Profile handled | P1 |
| INT-015 | Opportunity permissions | Permissions checked | P0 |
| INT-016 | Opportunity workflow | Workflow respected | P1 |
| INT-017 | Opportunity soft delete | Excluded when deleted | P1 |
| INT-018 | Opportunity audit | Audit trail | P1 |
| INT-019 | Opportunity tenant | Tenant isolation | P0 |
| INT-020 | Opportunity multi-profile | Multiple profiles | P1 |
| INT-021 | PDF service call | PDF generated | P0 |
| INT-022 | Chart service call | Charts generated | P0 |
| INT-023 | PDF + chart combined | Combined output | P0 |
| INT-024 | PDF service error | Error propagated | P1 |
| INT-025 | Chart service error | Error propagated | P1 |
| INT-026 | PDF async | Async generation | P1 |
| INT-027 | Chart format options | Format passed | P1 |
| INT-028 | PDF storage | Stored correctly | P1 |
| INT-029 | Chart cache | Chart cached | P2 |
| INT-030 | PDF download URL | URL valid | P1 |
| INT-031 | Cache get | Cache hit | P0 |
| INT-032 | Cache set | Cache stored | P0 |
| INT-033 | Cache delete | Cache cleared | P1 |
| INT-034 | Cache TTL | TTL respected | P1 |
| INT-035 | Cache key format | Key format correct | P1 |
| INT-036 | Cache invalidation | Invalidated on update | P1 |
| INT-037 | Cache distributed | Distributed read | P2 |
| INT-038 | Cache fallback | Fallback on miss | P1 |
| INT-039 | Cache memory limit | Eviction works | P2 |
| INT-040 | Cache stats | Stats available | P2 |
| INT-041 | DB read profile | Profile from DB | P0 |
| INT-042 | DB write profile | Profile to DB | P0 |
| INT-043 | DB transaction | Transaction atomic | P0 |
| INT-044 | DB connection pool | Pool used | P1 |
| INT-045 | DB migration | Schema current | P1 |
| INT-046 | DB soft delete | Soft delete filter | P1 |
| INT-047 | DB audit columns | Audit populated | P1 |
| INT-048 | DB indexes | Indexes used | P2 |
| INT-049 | DB read replica | Replica read | P2 |
| INT-050 | DB connection timeout | Timeout handled | P1 |
| INT-051 | Auth service | User resolved | P0 |
| INT-052 | Permission service | Permissions checked | P0 |
| INT-053 | Config service | Config loaded | P1 |
| INT-054 | Logging service | Logs written | P1 |
| INT-055 | Notification service | Notifications sent | P2 |
| INT-056 | Message queue | Queue publish | P2 |
| INT-057 | Event bus | Events published | P2 |
| INT-058 | Health check | Health reported | P1 |
| INT-059 | Metrics export | Metrics exposed | P2 |
| INT-060 | Tracing | Traces captured | P2 |
| INT-061 | Country service | Country data | P0 |
| INT-062 | Region service | Region data | P1 |
| INT-063 | Translation service | i18n | P1 |
| INT-064 | File storage | File stored | P1 |
| INT-065 | Blob storage | Blob stored | P2 |
| INT-066 | CDN | CDN URL | P2 |
| INT-067 | Email service | Email sent | P2 |
| INT-068 | Scheduled job | Job runs | P2 |
| INT-069 | Webhook | Webhook called | P2 |
| INT-070 | API gateway | Gateway routing | P1 |
| INT-071 | Load balancer | LB routing | P2 |
| INT-072 | Reverse proxy | Proxy headers | P2 |
| INT-073 | CORS | CORS headers | P1 |
| INT-074 | Caching headers | Cache headers | P1 |
| INT-075 | Compression | Response compressed | P1 |
| INT-076 | Versioning | API version | P1 |
| INT-077 | Deprecation | Deprecation header | P2 |
| INT-078 | Rate limiting | Rate limit applied | P1 |
| INT-079 | Request ID | Request ID propagated | P1 |
| INT-080 | Correlation ID | Correlation ID | P1 |
| INT-081 | Tenant resolution | Tenant from context | P0 |
| INT-082 | User context | User from token | P0 |
| INT-083 | Locale resolution | Locale from header | P1 |
| INT-084 | Timezone resolution | TZ from header | P2 |
| INT-085 | Feature flags | Flags evaluated | P1 |
| INT-086 | A/B test | Variant assigned | P2 |
| INT-087 | Experimentation | Experiment tracked | P2 |
| INT-088 | Analytics | Event tracked | P2 |
| INT-089 | Error tracking | Error reported | P1 |
| INT-090 | Alerting | Alert triggered | P2 |

---

## §6 Security — 50

*(Injection, access control, IDOR, data exposure, API security — as previously defined.)*

---

## §7 Concurrency — 25

*(Concurrent profile loads, score calculations, cache updates, data refresh, comparisons, etc.)*

---

## §8 Unit — 21

*(Score calc (5), risk mapping (3), weighting (5), data validation (5), formatting (3).)*

---

## §9 Performance — 16

*(Single country (<200ms), 10 countries (<1s), comparison (<500ms), PDF (<3s), historical (<1s), memory tests.)*

---

## §10 Load — 10

*(50 concurrent profiles, 100 searches, spike, sustained, data refresh under load.)*

---

**Status:** Ready for Execution
