# AnalyticsController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/AnalyticsController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for analytics/reporting: dashboard data, charts, KPIs, export, date ranges, aggregation.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30-50 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Concurrency (CON) | 25 | 25 | ✅ |
| §7 Unit (UNT) | 21 | 21 | ✅ |
| §8 Performance (PRF) | 16 | 16 | ✅ |
| §9 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Formula |
|-------|--------|---------|
| N≥3P? | ✅ | 90 ≥ 90 |
| E≥3P? | ✅ | 90 ≥ 90 |
| F≥3P? | ✅ | 90 ≥ 90 |
| I≥3P? | ✅ | 90 ≥ 90 |

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get dashboard KPIs | GET /api/analytics/dashboard/kpis | Returns KPI aggregations |
| POS-002 | Get chart data | GET /api/analytics/charts?type=trend | Returns chart series |
| POS-003 | Get date range analytics | GET /api/analytics?start=2026-01-01&end=2026-01-31 | Returns filtered data |
| POS-004 | Export to CSV | GET /api/analytics/export?format=csv | CSV download |
| POS-005 | Export to Excel | GET /api/analytics/export?format=xlsx | XLSX download |
| POS-006 | Aggregate by region | GET /api/analytics/by-region | Region breakdown |
| POS-007 | Aggregate by partner type | GET /api/analytics/by-type | Type breakdown |
| POS-008 | Get trend data (monthly) | GET /api/analytics/trends?period=monthly | Monthly trend series |
| POS-009 | Get trend data (quarterly) | GET /api/analytics/trends?period=quarterly | Quarterly trend series |
| POS-010 | Get top N by metric | GET /api/analytics/top?metric=value&limit=10 | Top 10 results |
| POS-011 | Filter by org unit | GET /api/analytics?orgUnitId=123 | Org-scoped data |
| POS-012 | Filter by partner | GET /api/analytics?partnerId=456 | Partner-scoped data |
| POS-013 | Compare periods | GET /api/analytics/compare?p1=2026-Q1&p2=2026-Q2 | Period comparison |
| POS-014 | Get summary statistics | GET /api/analytics/summary | Summary counts |
| POS-015 | Activity heatmap data | GET /api/analytics/heatmap | Heatmap matrix |
| POS-016 | Conversion rates | GET /api/analytics/conversion-rates | Conversion metrics |
| POS-017 | Growth metrics | GET /api/analytics/growth | Growth percentages |
| POS-018 | Paginated analytics | GET /api/analytics?page=1&pageSize=20 | Paginated response |
| POS-019 | Sorted analytics | GET /api/analytics?sortBy=date&order=desc | Sorted results |
| POS-020 | Multiple filters combined | GET with start, end, orgUnit, type | Combined filter result |
| POS-021 | Empty result set | GET for org with no data | Zero counts, no error |
| POS-022 | Default date range | GET without dates | Uses system default range |
| POS-023 | Authenticated user access | GET with valid token | 200 OK |
| POS-024 | Export PDF | GET /api/analytics/export?format=pdf | PDF download |
| POS-025 | Drill-down by entity | GET /api/analytics/drill/{entityId} | Drill-down details |
| POS-026 | Time-series breakdown | GET /api/analytics/timeseries | Time-series data |
| POS-027 | Benchmark comparison | GET /api/analytics/benchmark | Benchmark metrics |
| POS-028 | Year-over-year | GET /api/analytics/yoy | YoY comparison |
| POS-029 | Cumulative totals | GET /api/analytics/cumulative | Cumulative series |
| POS-030 | Average calculations | GET /api/analytics/avg | Average metrics |
---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | Unauthenticated dashboard | No token | 401 Unauthorized |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid date format | start=invalid | 400 Bad Request |
| NEG-004 | End before start | start=2026-02-01&end=2026-01-01 | 400 |
| NEG-005 | Future date range | start=2030-01-01 | 400 or validation |
| NEG-006 | Negative orgUnitId | orgUnitId=-1 | 400 |
| NEG-007 | Non-existent orgUnitId | orgUnitId=999999 | 404 or empty |
| NEG-008 | Invalid format param | format=invalid | 400 |
| NEG-009 | Negative limit | limit=-5 | 400 |
| NEG-010 | Zero limit | limit=0 | 400 |
| NEG-011 | Excessive limit | limit=100000 | 400 or capped |
| NEG-012 | Invalid sortBy | sortBy=invalidField | 400 |
| NEG-013 | Invalid period | period=invalid | 400 |
| NEG-014 | Malformed JSON body | Invalid JSON | 400 |
| NEG-015 | SQL injection in filter | filter='; DROP | Sanitized/rejected |
| NEG-016 | XSS in format | format=<script> | Sanitized |
| NEG-017 | Path traversal | path=../../../etc | 400 |
| NEG-018 | Negative page | page=-1 | 400 |
| NEG-019 | Non-numeric page | page=abc | 400 |
| NEG-020 | Oversized pageSize | pageSize=10000 | 400 |
| NEG-021 | Null required param | start=null | 400 |
| NEG-022 | Empty string ID | orgUnitId= | 400 |
| NEG-023 | Wrong content-type | Application/xml | 415 |
| NEG-024 | Missing permission | User without CanViewAnalytics | 403 |
| NEG-025 | Cross-org unit access | Org unit user accesses other org | 403 |
| NEG-026 | Invalid entityId for drill | entityId=999999 | 404 |
| NEG-027 | Invalid percentile | percentile=150 | 400 |
| NEG-028 | Invalid compare periods | p1=invalid | 400 |
| NEG-029 | Database timeout | Simulate DB timeout | 503 or retry |
| NEG-030 | Export service unavailable | Export service down | 503 |
| NEG-031 | Rate limit exceeded | Too many requests | 429 |
| NEG-032 | Disabled feature flag | Analytics disabled | 403 |
| NEG-033 | Insufficient role | Viewer role for export | 403 |
| NEG-034 | Deleted org unit | orgUnitId of deleted entity | 404 |
| NEG-035 | Invalid timezone | timezone=invalid | 400 |
| NEG-036 | Orphaned partner reference | partnerId deleted | 404 |
| NEG-037 | Circular org hierarchy | Malformed hierarchy | 500 with message |
| NEG-038 | Null optional filter | All optional null | Use defaults |
| NEG-039 | Special chars in search | search=\%'" | Sanitized |
| NEG-040 | Unicode overflow | Very long Unicode query | 400 or truncated |
| NEG-041 | Duplicate filters | Same filter twice | Deduplicated |
| NEG-042 | Conflicting filters | Mutually exclusive filters | 400 |
| NEG-043 | Invalid date range span | 10-year range where max 1 year | 400 |
| NEG-044 | Concurrent export cancellation | Cancel mid-export | Graceful |
| NEG-045 | Session expired mid-export | Token expires during export | 401 |
| NEG-046 | Missing export permission | No CanExportAnalytics | 403 |
| NEG-047 | Blocked IP | From blocked IP | 403 |
| NEG-048 | CORS preflight failure | Invalid origin | CORS error |
| NEG-049 | Invalid Accept header | Accept: text/plain | 406 |
| NEG-050 | HTTP method not allowed | PUT /api/analytics | 405 |
| NEG-051 | OPTIONS without auth | OPTIONS request | 200 (CORS) |
| NEG-052 | Head request | HEAD /api/analytics | 200 or 405 |
| NEG-053 | Invalid bearer format | Bearer malformed | 401 |
| NEG-054 | Revoked token | Revoked JWT | 401 |
| NEG-055 | Service account restriction | Service account for UI endpoint | 403 |
| NEG-056 | Audit log failure | Audit service down | Analytics still returns |
| NEG-057 | Cache corruption | Corrupted cache | Fallback to DB |
| NEG-058 | Decimal overflow | Very large aggregation | Handle or 400 |
| NEG-059 | Division by zero | Metric with zero denominator | 0 or N/A |
| NEG-060 | Null in aggregation | Null values in series | Handled |
| NEG-061 | Export format mismatch | Request PDF, send CSV | Correct format |
| NEG-062 | Pagination beyond data | page=99999 | Empty page |
| NEG-063 | Invalid drill entity type | Drill on wrong entity | 400 |
| NEG-064 | Orphaned hierarchy node | Missing parent | Fallback |
| NEG-065 | Stale cache | Cache TTL exceeded | Refresh |
| NEG-066 | Concurrent modify during read | Data changed during export | Consistent snapshot |
| NEG-067 | Memory pressure | Low memory during bulk export | Graceful degradation |
| NEG-068 | Disk full export | No disk for temp file | 507 |
| NEG-069 | Encoding error | Invalid charset in export | UTF-8 fallback |
| NEG-070 | Empty filter result | All filters result in empty | Empty array, 200 |

| NEG-071 | Invalid timeframe for mostActive | timeframe=invalid | 400 Bad Request |
| NEG-072 | Invalid metric for mostActive | metric=invalid | 400 Bad Request |
| NEG-073 | Negative userId for byUser | /api/partner/analytics/byUser/-1 | 400 |
| NEG-074 | Zero months for engagementTrends | months=0 | 400 Bad Request |
| NEG-075 | Months over max for engagementTrends | months=61 | 400 Bad Request |
| NEG-076 | Invalid limit for byCountry | limit=0 | 400 Bad Request |
| NEG-077 | Limit over max for byCountry | limit=251 | 400 Bad Request |
| NEG-078 | Invalid minCount for byCountry | minCount=0 | 400 Bad Request |
| NEG-079 | Invalid period for engagementTrends | period=invalid | 400 Bad Request |
| NEG-080 | Non-existent userId for byUser | /api/partner/analytics/byUser/999999 | 200 with empty partners |
| NEG-081 | Invalid timeframe for GetMostActiveContacts | timeframe=invalid | 200 (uses default 30d) |
| NEG-082 | Invalid period for GetContactsByGeographicRegion | period=invalid | 200 (uses default) |
| NEG-083 | Months over max for GetContactEngagementTrends | months=61 | 500 or validation |
| NEG-084 | End date before start for GetContactsByInteractionType | endDate < startDate | Empty or 400 |
| NEG-085 | Negative days for GetRecentlyActiveContacts | days=-1 | 500 or validation |
| NEG-086 | Invalid sortBy for GetRecentlyActiveContacts | sortBy=invalid | 200 (default lastInteraction) |
| NEG-087 | Zero limit for GetMostActiveContacts | limit=0 | 200 with empty data |
| NEG-088 | Negative minContacts for GetContactsByPartner | minContacts=-1 | 200 or validation |
| NEG-089 | Invalid InteractionType enum | type=999 | 400 Bad Request |
| NEG-090 | Non-existent partnerId for engagementTrends | partnerId=999999 | 200 with empty trends |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | date range (days) | 1 | 365 | ✅ | ✅ | ❌ | P1 |
| BND-002 | limit | 1 | 1000 | ✅ | ✅ | ❌ | P1 |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ | P2 |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ | P1 |
| BND-005 | search string | 0 | 200 | ✅ | ✅ | ❌ | P1 |
| BND-006 | orgUnitId | 1 | int.Max | ✅ | ✅ | ❌ | P2 |
| BND-007 | partnerId | 1 | int.Max | ✅ | ✅ | ❌ | P2 |
| BND-008 | percentile | 0 | 100 | ✅ | ✅ | ❌ | P1 |
| BND-009 | year | 2000 | 2100 | ✅ | ✅ | ❌ | P2 |
| BND-010 | month | 1 | 12 | ✅ | ✅ | ❌ | P2 |
| BND-011 | start date epoch | - | now | - | ✅ | ❌ | P1 |
| BND-012 | end date epoch | - | now+1 | ✅ | ✅ | ❌ | P1 |
| BND-013 | concurrent requests | 0 | 100 | ✅ | ✅ | ❌ | P2 |
| BND-014 | export file size (MB) | 0 | 50 | ✅ | ✅ | ❌ | P2 |
| BND-015 | filter count | 0 | 10 | ✅ | ✅ | ❌ | P2 |
| BND-016 | Empty result set | - | - | Returns [] | - | - | P1 |
| BND-017 | Single record | - | - | Returns 1 item | - | - | P1 |
| BND-018 | Feb 29 leap year | - | - | Valid date | - | - | P2 |
| BND-019 | Dec 31 boundary | - | - | Inclusive | - | - | P1 |
| BND-020 | Jan 1 boundary | - | - | Inclusive | - | - | P1 |
| BND-021 | Zero aggregation | - | - | Returns 0 | - | - | P1 |
| BND-022 | Max decimal precision | - | 2 | Rounded | - | - | P2 |
| BND-023 | Unicode name (max) | - | 255 | ✅ | ✅ | ❌ | P2 |
| BND-024 | Arabic chars | - | - | Display correctly | - | - | P2 |
| BND-025 | Chinese chars | - | - | Display correctly | - | - | P2 |
| BND-026 | Emoji in filter | - | - | Reject or sanitize | - | - | P2 |
| BND-027 | Null-safe aggregation | - | - | Skip nulls | - | - | P1 |
| BND-028 | Empty string filter | - | - | Treat as no filter | - | - | P2 |
| BND-029 | Whitespace-only | - | - | Trim/reject | - | - | P2 |
| BND-030 | First page | page=1 | - | ✅ | - | - | P1 |
| BND-031 | Last page | - | - | Partial results OK | - | - | P1 |
| BND-032 | Exact page boundary | - | - | Correct count | - | - | P1 |
| BND-033 | Sort empty | - | - | Returns [] | - | - | P2 |
| BND-034 | Sort single | - | - | Returns 1 | - | - | P2 |
| BND-035 | Timezone UTC | - | - | Correct conversion | - | - | P1 |
| BND-036 | Timezone offset | - | +/-14 | Correct | - | - | P2 |
| BND-037 | DST transition date | - | - | Handle correctly | - | - | P2 |
| BND-038 | Midnight boundary | 00:00:00 | - | Inclusive | - | - | P2 |
| BND-039 | End of day | 23:59:59 | - | Inclusive | - | - | P2 |
| BND-040 | Same start/end | start=end | - | Single day | - | - | P1 |
| BND-041 | Max filters combined | 10 | - | All applied | - | - | P2 |
| BND-042 | Zero limit special | limit=0 | - | 400 or default | - | - | P2 |
| BND-043 | Integer overflow risk | - | - | Use long | - | - | P2 |
| BND-044 | Float precision | - | - | Round consistently | - | - | P2 |
| BND-045 | Empty CSV export | - | - | Headers only | - | - | P1 |
| BND-046 | Single row export | - | - | Valid file | - | - | P1 |
| BND-047 | Max rows export | - | 100000 | Truncate or stream | - | - | P2 |
| BND-048 | Chart series empty | - | - | Empty arrays | - | - | P1 |
| BND-049 | Chart series single | - | - | One point | - | - | P1 |
| BND-050 | Heatmap sparse | - | - | Zeros for missing | - | - | P2 |
| BND-051 | Hierarchy depth max | - | 10 | Full depth | - | - | P2 |
| BND-052 | Drill levels | - | 5 | All levels | - | - | P2 |
| BND-053 | Period alignment | - | - | Month boundaries | - | - | P1 |
| BND-054 | Quarter boundaries | Q1-Q4 | - | Correct ranges | - | - | P1 |
| BND-055 | Year boundary | - | - | Full year | - | - | P1 |
| BND-056 | Comparison same period | p1=p2 | - | Same data | - | - | P2 |
| BND-057 | Adjacent periods | - | - | No overlap | - | - | P1 |
| BND-058 | Overlapping periods | - | - | Warn or reject | - | - | P2 |
| BND-059 | Very old data | 2000 | - | Included if valid | - | - | P2 |
| BND-060 | Future data | - | - | Excluded | - | - | P1 |
| BND-061 | Soft-deleted filter | - | - | Excluded | - | - | P1 |
| BND-062 | Inactive org filter | - | - | Excluded or 403 | - | - | P1 |
| BND-063 | Large aggregation set | - | 10000 | Paginate | - | - | P2 |
| BND-064 | Nested aggregation | - | - | Correct rollup | - | - | P1 |
| BND-065 | Empty group by | - | - | Single bucket | - | - | P2 |
| BND-066 | Single group | - | - | One group | - | - | P2 |
| BND-067 | All nulls | - | - | N/A or 0 | - | - | P2 |
| BND-068 | Mixed nulls | - | - | Skip nulls | - | - | P1 |
| BND-069 | Extreme values | - | - | No overflow | - | - | P2 |
| BND-070 | Round-trip export/import | Export then import | - | Data preserved | - | - | P2 |

| BND-071 | mostActive limit | 1 | 100 | ✅ | ✅ | ❌ 400 | P1 |
| BND-072 | engagementTrends months | 1 | 60 | ✅ | ✅ | ❌ 400 | P1 |
| BND-073 | byCountry limit | 1 | 250 | ✅ | ✅ | ❌ 400 | P1 |
| BND-074 | byCountry minCount | 1 | - | ✅ | - | ❌ 400 | P1 |
| BND-075 | timeframe daily | - | - | Cutoff 1 day | - | - | P1 |
| BND-076 | timeframe yearly | - | - | Cutoff 1 year | - | - | P1 |
| BND-077 | period daily/weekly/monthly | - | - | Correct grouping | - | - | P1 |
| BND-078 | GetMostActiveContacts limit | 1 | - | ✅ | - | - | P1 |
| BND-079 | GetRecentlyActiveContacts days | 1 | 365 | ✅ | ✅ | - | P2 |
| BND-080 | GetContactEngagementTrends months | 1 | - | ✅ | - | - | P1 |
| BND-081 | GetContactGrowthTrends months | 1 | - | ✅ | - | - | P1 |
| BND-082 | GetContactsByInteractionType limit | 1 | - | ✅ | - | - | P1 |
| BND-083 | GetContactsWithMostDocuments limit | 1 | - | ✅ | - | - | P1 |
| BND-084 | GetContactsByPartner minContacts | 1 | - | ✅ | - | - | P1 |
| BND-085 | byUser userId | 1 | int.Max | ✅ | ✅ | - | P2 |
| BND-086 | engagementTrends partnerId | 1 | int.Max | ✅ | ✅ | - | P2 |
| BND-087 | Contact timeframe 7d | - | - | 7-day window | - | - | P2 |
| BND-088 | Contact timeframe 1y | - | - | 1-year window | - | - | P2 |
| BND-089 | period all (geographic) | - | - | DateTime.MinValue | - | - | P2 |
| BND-090 | Empty engagement result | - | - | Returns [] | - | - | P1 |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|---------|
| FUN-001 | Workflow | KPI refresh on data change | New record created | KPI updated |
| FUN-002 | Workflow | Export respects filters | Export with filters | Filtered export |
| FUN-003 | Workflow | Drill-down respects permissions | Drill as viewer | Limited fields |
| FUN-004 | Workflow | Aggregation rollup | Child org data | Rolled to parent |
| FUN-005 | Workflow | Date range validation | Invalid range | Reject |
| FUN-006 | Workflow | Export format selection | CSV/Excel/PDF | Correct format |
| FUN-007 | Workflow | Period comparison | Two periods | Side-by-side |
| FUN-008 | Workflow | Trend calculation | Multiple periods | Correct slope |
| FUN-009 | Workflow | Conversion rate formula | Submissions/Approvals | Correct % |
| FUN-010 | Workflow | Growth rate formula | Period over period | Correct % |
| FUN-011 | Workflow | Pipeline stage progression | Stage data | Correct funnel |
| FUN-012 | Workflow | Time-series alignment | Multiple series | Aligned dates |
| FUN-013 | Workflow | Benchmark comparison | Vs target | Delta shown |
| FUN-014 | Workflow | YoY calculation | Same period Y-1 | Correct delta |
| FUN-015 | Workflow | Cumulative sum | Period series | Running total |
| FUN-016 | Validation | Required date range | Missing dates | 400 |
| FUN-017 | Validation | Org unit membership | Non-member org | 403 |
| FUN-018 | Validation | Export permission | No permission | 403 |
| FUN-019 | Validation | Numeric range | Out of range | 400 |
| FUN-020 | Validation | Enum values | Invalid period | 400 |
| FUN-021 | Validation | ID format | Non-numeric ID | 400 |
| FUN-022 | Validation | Pagination bounds | Invalid page | 400 |
| FUN-023 | Validation | Sort field whitelist | Invalid sort | 400 |
| FUN-024 | Validation | Filter field whitelist | Invalid filter | 400 |
| FUN-025 | Validation | Percentile 0-100 | 150 | 400 |
| FUN-026 | Constraint | Max export rows | 100K+ | Truncate/stream |
| FUN-027 | Constraint | Max date range | 1 year | Reject |
| FUN-028 | Constraint | Max concurrent exports | 5/user | 429 |
| FUN-029 | Constraint | Cache TTL | Stale data | Refresh |
| FUN-030 | Constraint | Rate limit | 100 req/min | 429 |
| FUN-031 | Constraint | Hierarchy depth | >10 levels | Limit |
| FUN-032 | Constraint | Filter combinatory | 10+ filters | Reject |
| FUN-033 | Constraint | Drill level | >5 levels | Limit |
| FUN-034 | Constraint | Aggregation size | >10K groups | Paginate |
| FUN-035 | Constraint | Export file size | >50MB | Reject |
| FUN-036 | Audit | View logged | User views analytics | Audit entry |
| FUN-037 | Audit | Export logged | User exports | Audit entry |
| FUN-038 | Audit | Failed access logged | 403 attempt | Audit entry |
| FUN-039 | Audit | Drill-down logged | Drill action | Audit entry |
| FUN-040 | Audit | Filter change logged | Filter applied | Audit (if sensitive) |
| FUN-041 | Audit | Timestamp in audit | Any action | UTC timestamp |
| FUN-042 | Audit | User ID in audit | Any action | User ID |
| FUN-043 | Audit | IP in audit | Any action | IP address |
| FUN-044 | Audit | Resource in audit | Export | Resource ID |
| FUN-045 | Audit | Outcome in audit | Success/fail | Outcome |
| FUN-046 | Business | Soft-deleted excluded | Query | No deleted |
| FUN-047 | Business | Inactive org excluded | Query | No inactive |
| FUN-048 | Business | Permission-based filter | Query | Auto-scoped |
| FUN-049 | Business | Timezone consistency | All dates | UTC stored |
| FUN-050 | Business | Decimal precision | Currency | 2 decimals |

| FUN-051 | Partner | mostActive metric engagements | GET with metric=engagements | Partners by engagement count |
| FUN-052 | Partner | mostActive metric interactions | GET with metric=interactions | Partners by interaction count |
| FUN-053 | Partner | mostActive metric lastActivity | GET with metric=lastActivity | Partners by last modified |
| FUN-054 | Partner | byUser includeCreated filter | includeCreated=false | Excludes created-by partners |
| FUN-055 | Partner | byUser includeModified filter | includeModified=false | Excludes modified-by partners |
| FUN-056 | Partner | byUser includeFocalPoint filter | includeFocalPoint=false | Excludes focal-point partners |
| FUN-057 | Partner | engagementTrends period grouping | period=daily | Daily buckets in trends |
| FUN-058 | Partner | engagementTrends period quarterly | period=quarterly | Quarterly buckets |
| FUN-059 | Partner | engagementTrends partnerId filter | partnerId=123 | Single-partner trends |
| FUN-060 | Partner | byCountry minCount filter | minCount=5 | Only countries with 5+ partners |
| FUN-061 | Partner | byCountry KeyGlobalPartners count | - | Counts key global per country |
| FUN-062 | Partner | byCountry ApprovedPartners count | - | Counts approved per country |
| FUN-063 | Partner | Soft-deleted partners excluded | Query | IsDeleted filter applied |
| FUN-064 | Partner | Active status filter byCountry | - | Status=Active only |
| FUN-065 | Contact | GetMostActiveContacts timeframe 7d | timeframe=7d | 7-day window |
| FUN-066 | Contact | GetMostActiveContacts timeframe 1y | timeframe=1y | 1-year window |
| FUN-067 | Contact | GetContactsByGeographicRegion groupBy | groupBy=country | Groups by country |
| FUN-068 | Contact | GetContactsByInteractionType type filter | type=Meeting | Filters by interaction type |
| FUN-069 | Contact | GetContactsByPartner includeInactive | includeInactive=true | Includes inactive contacts |
| FUN-070 | Contact | GetRecentlyActiveContacts sortBy | sortBy=interactionCount | Sorts by count |
| FUN-071 | Contact | GetContactsByJobTitle minContacts | minContacts=2 | Only titles with 2+ contacts |
| FUN-072 | Contact | GetContactGrowthTrends cumulative | - | CumulativeContacts in response |
| FUN-073 | Contact | GetContactsWithMostDocuments date filter | startDate, endDate | Date-scoped document count |
| FUN-074 | Contact | Soft-deleted contacts excluded | Query | IsDeleted filter applied |
| FUN-075 | Contact | Soft-deleted interactions excluded | Query | IsDeleted filter applied |
| FUN-076 | Partner | BaseEngagements IsDeleted filter | engagementTrends | Excludes deleted engagements |
| FUN-077 | Partner | LiaisonOffice country for byCountry | - | Uses liaison office country |
| FUN-078 | Partner | EngagementSignedDate for trends | - | Groups by signed date |
| FUN-079 | Partner | BaseEngagementPartners join | engagementTrends | Correct partner linkage |
| FUN-080 | Contact | InteractionContacts join | GetMostActiveContacts | Correct contact-interaction link |
| FUN-081 | Contact | DocumentRelationships EntityType | getContactsWithMostDocuments | EntityType=Contact filter |
| FUN-082 | Partner | metadata in response | Any partner analytics | timeframe, generatedAt present |
| FUN-083 | Contact | success flag in response | Any contact analytics | success=true in response |
| FUN-084 | Partner | AccessControlled EntityTypes.Partner | Any partner endpoint | Permission checked |
| FUN-085 | Contact | AccessControlled EntityTypes.Contact | Any contact endpoint | Permission checked |
| FUN-086 | Partner | ArgumentException to BadRequest | Invalid timeframe/metric | 400 with error message |
| FUN-087 | Partner | Exception to 500 | Unhandled exception | 500 with error |
| FUN-088 | Contact | Exception to 500 | Unhandled exception | 500 with message |
| FUN-089 | Partner | Empty result metadata | No data in range | totalPartners=0, partners=[] |
| FUN-090 | Contact | Empty result structure | No matching contacts | data=[], total=0 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Create partner → analytics | Partner, Analytics | New partner in analytics |
| INT-002 | CRUD | Update partner → analytics | Partner, Analytics | Updated in analytics |
| INT-003 | CRUD | Delete partner → analytics | Partner, Analytics | Excluded |
| INT-004 | CRUD | Create opportunity → pipeline | Opportunity, Analytics | Pipeline updated |
| INT-005 | CRUD | Status change → KPI | Entity, KPI | KPI reflects |
| INT-006 | CRUD | Org unit change → scope | Org, Analytics | Scope updated |
| INT-007 | CRUD | Bulk import → aggregation | Batch, Analytics | Aggregation updated |
| INT-008 | CRUD | Soft delete → exclusion | Entity, Analytics | Excluded |
| INT-009 | CRUD | Restore → inclusion | Entity, Analytics | Re-included |
| INT-010 | CRUD | Hierarchy change → rollup | Org, Analytics | Rollup correct |
| INT-011 | Search | Search by name | Partner, Analytics | Match in results |
| INT-012 | Search | Filter by type | Partner, Analytics | Filtered |
| INT-013 | Search | Filter by status | Entity, Analytics | Filtered |
| INT-014 | Search | Filter by date | All, Analytics | Date range applied |
| INT-015 | Search | Multi-filter | Partner, Org, Analytics | Combined |
| INT-016 | Search | Empty search | - | Empty array |
| INT-017 | Search | Partial match | Partner, Analytics | Fuzzy match |
| INT-018 | Search | Sort + filter | Analytics | Both applied |
| INT-019 | Search | Filter + pagination | Analytics | Both applied |
| INT-020 | Search | Export filtered | Analytics | Export matches filter |
| INT-021 | Pagination | Page 1 | Analytics | First page |
| INT-022 | Pagination | Last page | Analytics | Partial page OK |
| INT-023 | Pagination | Page size 10 | Analytics | 10 items |
| INT-024 | Pagination | Page size 100 | Analytics | 100 items |
| INT-025 | Pagination | Invalid page | Analytics | 400 |
| INT-026 | Relationships | Partner → Contacts | Partner, Contact, Analytics | Joined |
| INT-027 | Relationships | Org → Partners | Org, Partner, Analytics | Hierarchy |
| INT-028 | Relationships | Opportunity → Partners | Opp, Partner, Analytics | Linked |
| INT-029 | Relationships | Drill-down | Entity, Child | Drill works |
| INT-030 | Relationships | Orphan handling | Deleted parent | Graceful |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth service down | Auth | 401/503 |
| INT-033 | Error | Export service down | Export | 503 |
| INT-034 | Error | Cache miss | Cache | Fallback DB |
| INT-035 | Error | Timeout | Slow query | 504 or retry |
| INT-036 | Error | Validation error | Bad input | 400 |
| INT-037 | Error | NotFound | Invalid ID | 404 |
| INT-038 | Error | Forbidden | No permission | 403 |
| INT-039 | Error | Conflict | Concurrent update | 409 |
| INT-040 | Error | Rate limit | Too many | 429 |
| INT-041 | Error | Payload too large | Huge request | 413 |
| INT-042 | Error | Unsupported media | Wrong content-type | 415 |
| INT-043 | Error | Method not allowed | Wrong verb | 405 |
| INT-044 | Error | Service unavailable | Dependency down | 503 |
| INT-045 | Error | Gateway timeout | Upstream timeout | 504 |
| INT-046 | E2E | Full analytics flow | All | Dashboard → Export |
| INT-047 | E2E | Filter → Drill → Export | All | Consistent data |
| INT-048 | E2E | Multi-user concurrent | Users | No corruption |
| INT-049 | E2E | Session expiry during flow | Auth | Clean failure |
| INT-050 | E2E | Permission change mid-session | Auth | Re-check on next |

| INT-051 | Partner | mostActive → Partner list | Partner, BaseEngagement | Partners with metrics |
| INT-052 | Partner | byUser → Partner list | Partner, User | Filtered by user |
| INT-053 | Partner | engagementTrends → time series | BaseEngagement, Partner | Grouped by period |
| INT-054 | Partner | byCountry → geographic | Partner, LiaisonOffice | Country breakdown |
| INT-055 | Contact | GetMostActiveContacts → list | Contact, Interaction | Contacts with interaction count |
| INT-056 | Contact | GetContactsByGeographicRegion | Contact | Region breakdown |
| INT-057 | Contact | GetContactEngagementTrends | Contact, Interaction | Time-series by period |
| INT-058 | Contact | GetContactsByInteractionType | Contact, Interaction | Filtered by type |
| INT-059 | Contact | GetContactsByPartner | Contact, Partner | Partner distribution |
| INT-060 | Contact | GetRecentlyActiveContacts | Contact, Interaction | Recent activity list |
| INT-061 | Contact | GetContactsByJobTitle | Contact | Job title grouping |
| INT-062 | Contact | GetContactGrowthTrends | Contact | Growth over time |
| INT-063 | Contact | GetContactsWithMostDocuments | Contact, Document, DocumentRelationship | Document count per contact |
| INT-064 | Partner | Create partner → mostActive | Partner | New partner in results |
| INT-065 | Partner | Create engagement → engagementTrends | BaseEngagement, Partner | New engagement in trends |
| INT-066 | Contact | Create interaction → GetMostActiveContacts | Interaction, Contact | New interaction counted |
| INT-067 | Contact | Create contact → GetContactsByPartner | Contact, Partner | New contact in partner |
| INT-068 | Partner | Soft delete partner → byCountry | Partner | Excluded from results |
| INT-069 | Contact | Soft delete contact → analytics | Contact | Excluded from results |
| INT-070 | Partner | Update partner → byUser | Partner | Updated data in results |
| INT-071 | Partner | DbContext Partners query | UNOPSAppDbContext, Partner | Correct data source |
| INT-072 | Partner | DbContext BaseEngagements | UNOPSAppDbContext, BaseEngagement | Engagement data |
| INT-073 | Contact | DbContext InteractionContacts | UNOPSAppDbContext, InteractionContact | Junction table join |
| INT-074 | Contact | DbContext DocumentRelationships | UNOPSAppDbContext, DocumentRelationship | Document linkage |
| INT-075 | Partner | Auth IAP scheme | PartnerAnalyticsController | 401 if no token |
| INT-076 | Contact | Auth Authorize | ContactAnalyticsController | 401 if no token |
| INT-077 | Partner | AccessControlled attribute | Partner read | 403 if no permission |
| INT-078 | Contact | AccessControlled attribute | Contact read | 403 if no permission |
| INT-079 | Partner | Full flow mostActive | Partner, DB, Auth | End-to-end 200 |
| INT-080 | Contact | Full flow GetMostActiveContacts | Contact, DB, Auth | End-to-end 200 |
| INT-081 | Partner | Partner + LiaisonOffice join | Partner, LiaisonOffice | byCountry uses country |
| INT-082 | Partner | BaseEngagement + BaseEngagementPartners | BaseEngagement, Partner | engagementTrends join |
| INT-083 | Contact | Contact + InteractionContacts + Interaction | Contact, Interaction | GetMostActiveContacts join |
| INT-084 | Contact | Contact + Partner join | Contact, Partner | GetContactsByPartner |
| INT-085 | Partner | Empty DB mostActive | No partners | Empty partners array |
| INT-086 | Contact | Empty DB GetMostActiveContacts | No contacts | Empty data array |
| INT-087 | Partner | Single partner in DB | 1 Partner | Single result |
| INT-088 | Contact | Single contact in DB | 1 Contact | Single result |
| INT-089 | Partner | Multiple org units | Partner, Hierarchy | Scoped by org |
| INT-090 | Contact | Date range filter | Interaction.Date | Correct date filtering |

---

## §6 Concurrency Tests (25)

| ID | Scenario | Expected Behavior |
|----|----------|-------------------|
| CON-001 | 2 users export same data | Both succeed, both get file |
| CON-002 | 2 users update filter cache | No corruption |
| CON-003 | Export during data update | Consistent snapshot |
| CON-004 | 10 concurrent dashboard requests | All succeed |
| CON-005 | 50 concurrent analytics requests | All succeed or 429 |
| CON-006 | Double-click export | Single export, no duplicate |
| CON-007 | Rapid filter changes | Last filter wins |
| CON-008 | Concurrent drill-down | No race |
| CON-009 | Cache invalidation during read | No stale read |
| CON-010 | DB connection pool exhaustion | Queue or 503 |
| CON-011 | Transaction isolation | No dirty read |
| CON-012 | Optimistic concurrency | Last write wins |
| CON-013 | Deadlock scenario | Timeout and retry |
| CON-014 | Export + delete data | Export completes or partial |
| CON-015 | Rate limit + concurrent | Fair throttling |
| CON-016 | Session expiry during export | Clean fail |
| CON-017 | Multiple exports same user | Serialized or queued |
| CON-018 | Cache stampede | Single recompute |
| CON-019 | Lock contention | Timeout |
| CON-020 | Memory pressure concurrent | Graceful degradation |
| CON-021 | Aggregation during insert | Consistent or eventual |
| CON-022 | Hierarchy change during query | Snapshot |
| CON-023 | Permission change during request | Request uses old |
| CON-024 | Bulk export concurrent | Queue or limit |
| CON-025 | Read replica lag | Eventual consistency |

---

## §7 Unit Tests (21)

| ID | Category | Input | Expected Output |
|----|----------|-------|-----------------|
| UNT-001 | Validation | Valid date range | Accepted |
| UNT-002 | Validation | Invalid date format | Rejected |
| UNT-003 | Validation | Negative limit | Rejected |
| UNT-004 | Validation | Empty required | Rejected |
| UNT-005 | Validation | Invalid enum | Rejected |
| UNT-006 | Formatting | Date to string | ISO 8601 |
| UNT-007 | Formatting | Number to string | Localized |
| UNT-008 | Formatting | Percent to string | 2 decimal |
| UNT-009 | Calculation | Sum aggregation | Correct sum |
| UNT-010 | Calculation | Average aggregation | Correct avg |
| UNT-011 | Calculation | Growth rate | Correct % |
| UNT-012 | Calculation | Percentile | Correct value |
| UNT-013 | Calculation | YoY delta | Correct delta |
| UNT-014 | Status | Active filter | Active only |
| UNT-015 | Status | Inactive filter | Inactive only |
| UNT-016 | Status | All statuses | All |
| UNT-017 | Status | Draft filter | Draft only |
| UNT-018 | Status | Closed filter | Closed only |
| UNT-019 | Collections | Empty list | [] |
| UNT-020 | Collections | Single item | [item] |
| UNT-021 | Collections | Deduplication | No duplicates |

---

## §8 Performance Tests (16)

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | Dashboard KPIs | < 500ms | P0 |
| PRF-002 | Chart data | < 1s | P0 |
| PRF-003 | Export 1K rows CSV | < 2s | P1 |
| PRF-004 | Export 10K rows CSV | < 10s | P1 |
| PRF-005 | Export 100K rows | < 60s or stream | P2 |
| PRF-006 | Search with filter | < 1s | P1 |
| PRF-007 | Drill-down | < 500ms | P1 |
| PRF-008 | Aggregation | < 2s | P1 |
| PRF-009 | Trend calculation | < 2s | P1 |
| PRF-010 | 10 concurrent dashboard | < 2s each | P1 |
| PRF-011 | 50 concurrent read | < 3s each | P2 |
| PRF-012 | 5 concurrent exports | < 15s each | P2 |
| PRF-013 | Memory during 100K export | < 500MB | P2 |
| PRF-014 | Memory dashboard | < 100MB | P2 |
| PRF-015 | Cache hit ratio | > 80% | P2 |
| PRF-016 | DB query count per request | < 10 | P2 |

---

## §9 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users sustained | 10 min | 95% < 2s, 0 errors |
| LDT-002 | 50 users sustained | 10 min | 95% < 3s, <1% errors |
| LDT-003 | 100 users sustained | 10 min | 95% < 5s, <2% errors |
| LDT-004 | Spike 10→100 in 1 min | 5 min | No crash |
| LDT-005 | Spike 50→200 in 30s | 5 min | Degrade gracefully |
| LDT-006 | Stress to 200 users | Until failure | Document limit |
| LDT-007 | Stress to 500 users | Until failure | Document limit |
| LDT-008 | Stress export 50 concurrent | 5 min | Queue or limit |
| LDT-009 | Recovery after spike | 5 min | Return to baseline |
| LDT-010 | Recovery after stress | 10 min | Full recovery |

---

## Traceability Matrix

| Requirement / AC | Test Cases |
|-----------------|------------|
| Dashboard KPIs | POS-001, FUN-001, PRF-001 |
| Chart data | POS-002, BND-048, INT-046 |
| Export CSV/Excel | POS-004, POS-005, NEG-008, SEC-036 |
| Date range filter | POS-003, NEG-003, BND-001 |
| Aggregation | POS-006, POS-007, FUN-009, UNT-009 |
| 3:1 Ratio | NEG-001–070, BND-001–070 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
