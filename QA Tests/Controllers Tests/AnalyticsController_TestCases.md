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
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## §1 Positive Tests (35)

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
| POS-031 | Median calculations | GET /api/analytics/median | Median metrics |
| POS-032 | Percentile analytics | GET /api/analytics/percentile?p=90 | 90th percentile |
| POS-033 | Filter by status | GET /api/analytics?status=Active | Status-filtered |
| POS-034 | Pipeline overview | GET /api/analytics/pipeline | Pipeline funnel |
| POS-035 | Recent activity feed | GET /api/analytics/activity | Activity feed |

---

## §2 Negative Tests (70)

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

---

## §3 Boundary Tests (70)

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

---

## §4 Functional Tests (50)

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

---

## §5 Integration Tests (50)

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

---

## §6 Security Tests (50)

| ID | Category | Attack/Scenario | Target | Expected |
|----|----------|-----------------|-------|----------|
| SEC-001 | Injection | SQL injection in filter | Filter param | Sanitized/rejected |
| SEC-002 | Injection | SQL injection in sort | Sort param | Sanitized |
| SEC-003 | Injection | XSS in export filename | Filename | Encoded |
| SEC-004 | Injection | XSS in search | Search param | Encoded |
| SEC-005 | Injection | NoSQL injection | Filter | Rejected |
| SEC-006 | Injection | LDAP injection | Search | Rejected |
| SEC-007 | Injection | Command injection | Export path | Rejected |
| SEC-008 | Injection | Path traversal | File path | Rejected |
| SEC-009 | Injection | Header injection | Custom header | Rejected |
| SEC-010 | Injection | Log injection | User input | Sanitized |
| SEC-011 | Access | No auth | All endpoints | 401 |
| SEC-012 | Access | Wrong role | Export | 403 |
| SEC-013 | Access | Cross-org | Other org data | 403 |
| SEC-014 | Access | Horizontal privilege | Other user's scope | 403 |
| SEC-015 | Access | Vertical privilege | Admin endpoint | 403 |
| SEC-016 | Access | Expired token | All | 401 |
| SEC-017 | Access | Revoked token | All | 401 |
| SEC-018 | Access | Tampered token | All | 401 |
| SEC-019 | Access | Missing scope | OAuth scope | 403 |
| SEC-020 | Access | Service account UI | UI endpoint | 403 |
| SEC-021 | IDOR | Access other org analytics | orgUnitId | 403 |
| SEC-022 | IDOR | Access other user export | Export ID | 403 |
| SEC-023 | IDOR | Manipulate entityId | entityId | 403 |
| SEC-024 | IDOR | ID enumeration | Sequential IDs | Rate limit |
| SEC-025 | IDOR | Parameter pollution | Duplicate params | First wins |
| SEC-026 | Mass Assign | Add admin flag | Request body | Ignored |
| SEC-027 | Mass Assign | Add role | Request body | Ignored |
| SEC-028 | Mass Assign | Override org scope | Request body | Ignored |
| SEC-029 | Mass Assign | Override user ID | Request body | Ignored |
| SEC-030 | Mass Assign | Override permission | Request body | Ignored |
| SEC-031 | Auth | Session fixation | Session | New session |
| SEC-032 | Auth | Session hijack | Token | Invalidated |
| SEC-033 | Auth | Replay attack | Old token | Rejected |
| SEC-034 | Auth | CSRF | State-changing | Token required |
| SEC-035 | Auth | Brute force | Login | Rate limit |
| SEC-036 | Data | PII in export | Export | Masked/redacted |
| SEC-037 | Data | Sensitive in logs | Logs | No PII |
| SEC-038 | Data | Error message info | 500 response | Generic message |
| SEC-039 | Data | Stack trace | Exception | Not exposed |
| SEC-040 | Data | Debug mode prod | Config | Disabled |
| SEC-041 | OWASP | A01 Broken Access | - | 403 tests |
| SEC-042 | OWASP | A02 Cryptographic | - | TLS, hashing |
| SEC-043 | OWASP | A03 Injection | - | Parametrized |
| SEC-044 | OWASP | A04 Insecure Design | - | Defensive |
| SEC-045 | OWASP | A05 Misconfig | - | Secure defaults |
| SEC-046 | OWASP | A06 Vulnerable Components | - | No known CVEs |
| SEC-047 | OWASP | A07 Auth Failures | - | Strong auth |
| SEC-048 | OWASP | A08 Data Integrity | - | Integrity checks |
| SEC-049 | OWASP | A09 Logging | - | Audit logs |
| SEC-050 | OWASP | A10 SSRF | - | No internal calls |

---

## §7 Concurrency Tests (25)

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

## §8 Unit Tests (21)

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

## §9 Performance Tests (16)

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

## §10 Load Tests (10)

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
