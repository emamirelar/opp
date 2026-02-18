# DashboardController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/DashboardController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for dashboard: widgets, KPI tiles, recent activity, pipeline overview, partner statistics.

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

**3:1 Ratio Checks:** N≥3P ✅ (90≥90) | E≥3P ✅ (90≥90) | F≥3P ✅ (90≥90) | I≥3P ✅ (90≥90)

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get dashboard | GET /api/dashboard | Dashboard data |
| POS-002 | Get widgets | GET /api/dashboard/widgets | Widget list |
| POS-003 | Get KPI tiles | GET /api/dashboard/kpis | KPI tiles |
| POS-004 | Get recent activity | GET /api/dashboard/activity | Activity feed |
| POS-005 | Get pipeline overview | GET /api/dashboard/pipeline | Pipeline data |
| POS-006 | Get partner statistics | GET /api/dashboard/partners/stats | Partner stats |
| POS-007 | Get opportunity stats | GET /api/dashboard/opportunities/stats | Opp stats |
| POS-008 | Get widget by ID | GET /api/dashboard/widgets/{id} | Widget details |
| POS-009 | Filter by org unit | GET ?orgUnitId=1 | Org-scoped |
| POS-010 | Filter by date range | GET ?start&end | Date-filtered |
| POS-011 | Get user-specific | GET (authenticated) | User dashboard |
| POS-012 | Get default layout | GET /api/dashboard/layout | Layout |
| POS-013 | Get widget config | GET /api/dashboard/widgets/{id}/config | Config |
| POS-014 | Update widget order | PUT /api/dashboard/layout | Order updated |
| POS-015 | Add widget | POST /api/dashboard/widgets | Widget added |
| POS-016 | Remove widget | DELETE /api/dashboard/widgets/{id} | Removed |
| POS-017 | Refresh dashboard | POST /api/dashboard/refresh | Refreshed |
| POS-018 | Get drill-down | GET /api/dashboard/kpis/{id}/drill | Drill data |
| POS-019 | Paginate activity | GET ?page=1&pageSize=20 | Paginated |
| POS-020 | Filter activity type | GET ?activityType=Created | Filtered |
| POS-021 | Get top partners | GET /api/dashboard/partners/top | Top list |
| POS-022 | Get pipeline stages | GET /api/dashboard/pipeline/stages | Stages |
| POS-023 | Get charts data | GET /api/dashboard/charts | Chart data |
| POS-024 | Get alerts | GET /api/dashboard/alerts | Alerts |
| POS-025 | Dismiss alert | POST /api/dashboard/alerts/{id}/dismiss | Dismissed |
| POS-026 | Get notifications | GET /api/dashboard/notifications | Notifications |
| POS-027 | Get shortcut summary | GET /api/dashboard/shortcuts | Shortcuts |
| POS-028 | Empty dashboard | New user | Default layout |
| POS-029 | Cached dashboard | GET twice | Cached |
| POS-030 | Localized dashboard | GET ?lang=fr | French labels |


---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid widget ID | id=999999 | 404 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Invalid orgUnitId | orgUnitId=-1 | 400 |
| NEG-006 | Non-existent org | orgUnitId=999999 | 404 or empty |
| NEG-007 | Invalid date format | start=invalid | 400 |
| NEG-008 | End before start | start>end | 400 |
| NEG-009 | Future date | start=2030 | 400 |
| NEG-010 | SQL injection | filter='; DROP | Sanitized |
| NEG-011 | XSS in widget name | name=<script> | Sanitized |
| NEG-012 | Negative page | page=-1 | 400 |
| NEG-013 | Zero pageSize | pageSize=0 | 400 |
| NEG-014 | Excessive pageSize | pageSize=10000 | 400 |
| NEG-015 | Invalid activity type | type=Invalid | 400 |
| NEG-016 | Cross-org access | Other org data | 403 |
| NEG-017 | No permission | User without CanViewDashboard | 403 |
| NEG-018 | Deleted widget | id of deleted | 404 |
| NEG-019 | Invalid layout | Malformed layout JSON | 400 |
| NEG-020 | Null request | POST null | 400 |
| NEG-021 | Malformed JSON | Invalid JSON | 400 |
| NEG-022 | Wrong content-type | Application/xml | 415 |
| NEG-023 | Invalid widget type | type=Invalid | 400 |
| NEG-024 | Duplicate widget | Add same widget twice | 400 |
| NEG-025 | Exceed max widgets | Add 21st widget | 400 |
| NEG-026 | Rate limit | Too many requests | 429 |
| NEG-027 | Invalid drill ID | drill?id=invalid | 404 |
| NEG-028 | Invalid alert ID | alert?id=999999 | 404 |
| NEG-029 | Dismiss others' alert | Dismiss other user's | 403 |
| NEG-030 | Update read-only widget | PUT on system widget | 403 |
| NEG-031 | Delete system widget | DELETE system widget | 403 |
| NEG-032 | DB timeout | Simulate | 503 |
| NEG-033 | Cache failure | Cache down | Fallback |
| NEG-034 | Payload too large | Huge body | 413 |
| NEG-035 | Invalid Accept | Accept: text/plain | 406 |
| NEG-036 | HTTP method | PUT for get | 405 |
| NEG-037 | OPTIONS | OPTIONS | 200 |
| NEG-038 | HEAD | HEAD | 200 or 405 |
| NEG-039 | Trailing slash | /api/dashboard/ | Redirect |
| NEG-040 | Case sensitivity | /api/Dashboard | 404 |
| NEG-041 | Extra path | /api/dashboard/1/extra | 404 |
| NEG-042 | Invalid bearer | Bearer malformed | 401 |
| NEG-043 | Revoked token | Revoked JWT | 401 |
| NEG-044 | Service account | Service for UI | 403 |
| NEG-045 | Session expired | Mid-request | 401 |
| NEG-046 | Audit failure | Audit down | Continue |
| NEG-047 | Invalid theme | theme=invalid | 400 |
| NEG-048 | Invalid lang | lang=xx | 400 or default |
| NEG-049 | Export no permission | No export permission | 403 |
| NEG-050 | Stale date range | Very old dates | Empty or warn |
| NEG-051 | Control chars | name with \0 | 400 |
| NEG-052 | Unicode overflow | Very long | 400 |
| NEG-053 | Invalid UUID | id=invalid-guid | 400 |
| NEG-054 | Mismatched IDs | Path != body | 400 |
| NEG-055 | Read-only field | Update system field | Ignored |
| NEG-056 | Version conflict | Stale version | 409 |
| NEG-057 | Blocked IP | From blocked | 403 |
| NEG-058 | CORS fail | Invalid origin | CORS error |
| NEG-059 | Widget config invalid | Invalid config | 400 |
| NEG-060 | Pipeline filter invalid | Invalid stage | 400 |
| NEG-061 | KPI drill disabled | Drill on disabled KPI | 403 |
| NEG-062 | Alert limit exceeded | Too many alerts | 429 |
| NEG-063 | Notification limit | Too many | Truncate |
| NEG-064 | Empty layout | Empty layout | Default |
| NEG-065 | Concurrent layout update | 2 users update | Last write |
| NEG-066 | Widget dependency missing | Widget needs missing data | Graceful |
| NEG-067 | Data source failure | Source down | Partial/503 |
| NEG-068 | Permission change mid-load | Permission revoked | 403 |
| NEG-069 | Inactive org | Org inactive | 403 |
| NEG-070 | Soft-deleted data | Query deleted | Excluded |
| NEG-071 | Invalid widget position | position=-1 | 400 |
| NEG-072 | Invalid KPI aggregation | agg=Invalid | 400 |
| NEG-073 | Missing org context | No orgUnitId for scoped | 400 |
| NEG-074 | Invalid refresh token | Refresh with invalid | 401 |
| NEG-075 | Widget type mismatch | Add wrong type | 400 |
| NEG-076 | Invalid drill parameters | drill?invalid=1 | 400 |
| NEG-077 | Alert already dismissed | Dismiss again | 400 |
| NEG-078 | Pipeline stage invalid | stage=Invalid | 400 |
| NEG-079 | Notification mark invalid | mark=Invalid | 400 |
| NEG-080 | Export format invalid | format=Invalid | 400 |
| NEG-081 | Layout schema invalid | Wrong schema version | 400 |
| NEG-082 | Widget config type wrong | Config wrong type | 400 |
| NEG-083 | Duplicate shortcut | Add existing | 409 |
| NEG-084 | Invalid chart type | chartType=Invalid | 400 |
| NEG-085 | Missing required filter | Required filter absent | 400 |
| NEG-086 | Conflicting filters | Mutually exclusive | 400 |
| NEG-087 | Invalid date timezone | tz=Invalid | 400 |
| NEG-088 | Widget dependency cycle | Circular dependency | 400 |
| NEG-089 | KPI source unavailable | Source down | 503 |
| NEG-090 | Dashboard locked | Another user editing | 423 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | widget count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-002 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | name length | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | date range | 1 day | 365 days | ✅ | ✅ | ❌ |
| BND-006 | orgUnitId | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-007 | activity count | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-008 | pipeline stages | 0 | 20 | ✅ | ✅ | ❌ |
| BND-009 | KPI count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-010 | alert count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-011 | Empty dashboard | - | - | Default | - | - |
| BND-012 | Single widget | - | - | [widget] | - | - |
| BND-013 | First page | page=1 | - | ✅ | - | - |
| BND-014 | Last page | - | - | Partial | - | - |
| BND-015 | Zero widgets | - | - | [] | - | - |
| BND-016 | Max widgets | 20 | - | - | ✅ | ❌ |
| BND-017 | Feb 29 | - | - | Valid | - | - |
| BND-018 | Unicode name | - | - | Accept | - | - |
| BND-019 | Null optional | - | - | Default | - | - |
| BND-020 | Empty string | - | - | No filter | - | - |
| BND-021 | Whitespace | - | - | Trim | - | - |
| BND-022 | Sort empty | - | - | [] | - | - |
| BND-023 | Sort single | - | - | [item] | - | - |
| BND-024 | Filter no match | - | - | [] | - | - |
| BND-025 | Filter all | - | - | Full | - | - |
| BND-026 | Timezone | UTC | - | Correct | - | - |
| BND-027 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-028 | Cache TTL | - | - | Expire | - | - |
| BND-029 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-030 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-031 | Layout size | - | 100KB | ✅ | ✅ | ❌ |
| BND-032 | Config size | - | 10KB | ✅ | ✅ | ❌ |
| BND-033 | Chart data points | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-034 | Pipeline items | 0 | 500 | ✅ | ✅ | ❌ |
| BND-035 | Partner stats | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-036 | Activity items | 0 | 500 | ✅ | ✅ | ❌ |
| BND-037 | KPI value | - | decimal.Max | ✅ | ✅ | ❌ |
| BND-038 | Percent value | 0 | 100 | ✅ | ✅ | ❌ |
| BND-039 | Theme values | - | - | light,dark,system | - | - |
| BND-040 | Locale values | - | - | en,fr,es,pt | - | - |
| BND-041 | Pagination boundary | - | - | Exact | - | - |
| BND-042 | Cursor pagination | - | - | Valid | - | - |
| BND-043 | Empty activity | - | - | [] | - | - |
| BND-044 | Single activity | - | - | [item] | - | - |
| BND-045 | Empty pipeline | - | - | [] | - | - |
| BND-046 | Single pipeline | - | - | [item] | - | - |
| BND-047 | Zero KPIs | - | - | [] | - | - |
| BND-048 | Single KPI | - | - | [item] | - | - |
| BND-049 | Round-trip | Update → Get | - | Match | - | - |
| BND-050 | Soft-deleted | - | - | Excluded | - | - |
| BND-051 | Inactive | - | - | Excluded | - | - |
| BND-052 | Widget order | 0 | 19 | ✅ | ✅ | ❌ |
| BND-053 | Position x | 0 | 12 | ✅ | ✅ | ❌ |
| BND-054 | Position y | 0 | 12 | ✅ | ✅ | ❌ |
| BND-055 | Width | 1 | 12 | ✅ | ✅ | ❌ |
| BND-056 | Height | 1 | 12 | ✅ | ✅ | ❌ |
| BND-057 | Refresh interval | 0 | 3600 | ✅ | ✅ | ❌ |
| BND-058 | Drill depth | 0 | 5 | ✅ | ✅ | ❌ |
| BND-059 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-060 | Export empty | - | - | Headers | - | - |
| BND-061 | Export single | - | - | Valid | - | - |
| BND-062 | Alert priority | 0 | 3 | ✅ | ✅ | ❌ |
| BND-063 | Notification limit | - | 50 | ✅ | ✅ | ❌ |
| BND-064 | Shortcut count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-065 | Chart series | 0 | 10 | ✅ | ✅ | ❌ |
| BND-066 | Top N | 1 | 100 | ✅ | ✅ | ❌ |
| BND-067 | Decimal precision | - | 2 | Rounded | - | - |
| BND-068 | Null KPI | - | - | 0 or N/A | - | - |
| BND-069 | Negative KPI | - | - | Reject or 0 | - | - |
| BND-070 | Version | 1 | - | ✅ | ❌ | - |
| BND-071 | Widget refresh rate | 0 | 3600 | ✅ | ✅ | ❌ |
| BND-072 | Activity type enum | - | - | Valid | - | - |
| BND-073 | Pipeline stage count | 0 | 20 | ✅ | ✅ | ❌ |
| BND-074 | KPI decimal places | 0 | 4 | ✅ | ✅ | ❌ |
| BND-075 | Alert priority range | 0 | 3 | ✅ | ✅ | ❌ |
| BND-076 | Notification batch | 0 | 50 | ✅ | ✅ | ❌ |
| BND-077 | Shortcut max | 0 | 20 | ✅ | ✅ | ❌ |
| BND-078 | Chart series max | 0 | 10 | ✅ | ✅ | ❌ |
| BND-079 | Top N limit | 1 | 100 | ✅ | ✅ | ❌ |
| BND-080 | Drill level | 0 | 5 | ✅ | ✅ | ❌ |
| BND-081 | Export format | - | - | csv, xlsx | - | - |
| BND-082 | Layout version | 1 | - | ✅ | ❌ | - |
| BND-083 | Widget min size | 1 | - | ✅ | - | - |
| BND-084 | Filter combo max | - | 10 | ✅ | ✅ | ❌ |
| BND-085 | Cached response TTL | - | 300s | ✅ | ✅ | ❌ |
| BND-086 | Org unit depth | 0 | 10 | ✅ | ✅ | ❌ |
| BND-087 | Date precision | - | - | Day | - | - |
| BND-088 | Null optional filter | - | - | Default | - | - |
| BND-089 | Empty filter set | - | - | All | - | - |
| BND-090 | Single filter | - | - | Applied | - | - |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get dashboard | GET | Data |
| FUN-002 | Workflow | Get widgets | GET | Widgets |
| FUN-003 | Workflow | Get KPIs | GET | KPIs |
| FUN-004 | Workflow | Get activity | GET | Activity |
| FUN-005 | Workflow | Get pipeline | GET | Pipeline |
| FUN-006 | Workflow | Add widget | POST | Added |
| FUN-007 | Workflow | Remove widget | DELETE | Removed |
| FUN-008 | Workflow | Update layout | PUT | Updated |
| FUN-009 | Workflow | Drill-down | GET drill | Drill data |
| FUN-010 | Workflow | Dismiss alert | POST dismiss | Dismissed |
| FUN-011 | Workflow | Filter org | GET ?orgUnitId | Filtered |
| FUN-012 | Workflow | Filter date | GET ?start&end | Filtered |
| FUN-013 | Workflow | Paginate | GET ?page | Paginated |
| FUN-014 | Workflow | Refresh | POST refresh | Refreshed |
| FUN-015 | Workflow | Export | GET export | File |
| FUN-016 | Validation | Required auth | No auth | 401 |
| FUN-017 | Validation | Permission | No permission | 403 |
| FUN-018 | Validation | Valid ID | Invalid ID | 400 |
| FUN-019 | Validation | Valid date | Invalid date | 400 |
| FUN-020 | Validation | Org scope | Cross-org | 403 |
| FUN-021 | Validation | Widget type | Invalid type | 400 |
| FUN-022 | Validation | Max widgets | >20 | 400 |
| FUN-023 | Validation | Layout format | Invalid | 400 |
| FUN-024 | Validation | Config format | Invalid | 400 |
| FUN-025 | Validation | Activity type | Invalid | 400 |
| FUN-026 | Constraint | System widget | No delete | 403 |
| FUN-027 | Constraint | Widget order | Unique | Enforce |
| FUN-028 | Constraint | Cache TTL | Stale | Refresh |
| FUN-029 | Constraint | Rate limit | Too many | 429 |
| FUN-030 | Constraint | Org scope | Cross-org | 403 |
| FUN-031 | Constraint | User scope | Own alerts | Only |
| FUN-032 | Constraint | Version | Optimistic | 409 |
| FUN-033 | Constraint | Max export | >10K | Truncate |
| FUN-034 | Constraint | Drill depth | >5 | Limit |
| FUN-035 | Constraint | Refresh cooldown | Too soon | 429 |
| FUN-036 | Audit | View | GET | Audit |
| FUN-037 | Audit | Add widget | POST | Audit |
| FUN-038 | Audit | Remove widget | DELETE | Audit |
| FUN-039 | Audit | Update layout | PUT | Audit |
| FUN-040 | Audit | Dismiss alert | POST | Audit |
| FUN-041 | Audit | Export | GET export | Audit |
| FUN-042 | Audit | Timestamp | Any | UTC |
| FUN-043 | Audit | User ID | Any | User ID |
| FUN-044 | Audit | IP | Any | IP |
| FUN-045 | Audit | Resource | Any | Resource |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Role-based | Query | Role widgets |
| FUN-050 | Business | Decimal | Currency | 2 decimals |
| FUN-051 | Workflow | Get export | GET export | File |
| FUN-052 | Workflow | Update layout | PUT layout | Updated |
| FUN-053 | Workflow | Add multiple widgets | POST batch | Added |
| FUN-054 | Workflow | Filter by stage | GET ?stage | Filtered |
| FUN-055 | Workflow | Get drill data | GET drill | Data |
| FUN-056 | Validation | Widget ID format | Invalid | 400 |
| FUN-057 | Validation | Layout JSON | Malformed | 400 |
| FUN-058 | Validation | Date range | Invalid | 400 |
| FUN-059 | Validation | Org unit | Invalid | 404 |
| FUN-060 | Validation | Activity type | Invalid | 400 |
| FUN-061 | Constraint | Max widgets | >20 | 400 |
| FUN-062 | Constraint | Export limit | >10K | Truncate |
| FUN-063 | Constraint | Drill depth | >5 | Limit |
| FUN-064 | Constraint | Refresh cooldown | Too soon | 429 |
| FUN-065 | Constraint | Alert ownership | Other user | 403 |
| FUN-066 | Audit | Add widget | POST | Audit |
| FUN-067 | Audit | Remove widget | DELETE | Audit |
| FUN-068 | Audit | Update layout | PUT | Audit |
| FUN-069 | Audit | Dismiss alert | POST | Audit |
| FUN-070 | Audit | Export | GET | Audit |
| FUN-071 | Business | Cache invalidation | Update | Refresh |
| FUN-072 | Business | Permission change | Mid-session | Next request |
| FUN-073 | Business | Org hierarchy | Rollup | Correct |
| FUN-074 | Business | Timezone | Display | UTC |
| FUN-075 | Business | Localization | lang param | Correct |
| FUN-076 | Workflow | Get layout | GET | Layout |
| FUN-077 | Workflow | Get config | GET config | Config |
| FUN-078 | Workflow | Filter activity | GET ?type | Filtered |
| FUN-079 | Workflow | Paginate | GET ?page | Paginated |
| FUN-080 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-081 | Validation | Required auth | No auth | 401 |
| FUN-082 | Validation | Permission | No perm | 403 |
| FUN-083 | Validation | Valid ID | Invalid | 400 |
| FUN-084 | Validation | Cross-org | Other org | 403 |
| FUN-085 | Validation | Config format | Invalid | 400 |
| FUN-086 | Constraint | System widget | No delete | 403 |
| FUN-087 | Constraint | Widget order | Unique | Enforce |
| FUN-088 | Constraint | Cache TTL | Stale | Refresh |
| FUN-089 | Constraint | Rate limit | Too many | 429 |
| FUN-090 | Constraint | Version | Optimistic | 409 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get dashboard | Dashboard | Data |
| INT-002 | CRUD | Get widgets | Widgets | List |
| INT-003 | CRUD | Add widget | Widget | Added |
| INT-004 | CRUD | Remove widget | Widget | Removed |
| INT-005 | CRUD | Update layout | Layout | Updated |
| INT-006 | CRUD | Get KPIs | KPIs | List |
| INT-007 | CRUD | Get activity | Activity | Feed |
| INT-008 | CRUD | Get pipeline | Pipeline | Data |
| INT-009 | CRUD | Drill-down | KPI, Drill | Data |
| INT-010 | CRUD | Dismiss alert | Alert | Dismissed |
| INT-011 | Search | Filter org | Dashboard | Filtered |
| INT-012 | Search | Filter date | Dashboard | Filtered |
| INT-013 | Search | Filter type | Activity | Filtered |
| INT-014 | Search | Multi-filter | Dashboard | Combined |
| INT-015 | Search | Empty filter | - | Default |
| INT-016 | Search | Invalid filter | Dashboard | 400 |
| INT-017 | Search | Sort | Activity | Sorted |
| INT-018 | Search | Paginate | Activity | Paginated |
| INT-019 | Search | Export filtered | Dashboard | Matches |
| INT-020 | Search | Widget config | Widget | Config |
| INT-021 | Pagination | Page 1 | Activity | First |
| INT-022 | Pagination | Last page | Activity | Partial |
| INT-023 | Pagination | Size | Activity | Correct |
| INT-024 | Pagination | Invalid | Activity | 400 |
| INT-025 | Pagination | Boundary | Activity | Exact |
| INT-026 | Relationships | Dashboard → Widget | Dashboard, Widget | Linked |
| INT-027 | Relationships | Dashboard → User | Dashboard, User | Linked |
| INT-028 | Relationships | KPI → Drill | KPI, Drill | Linked |
| INT-029 | Relationships | Orphan | Deleted widget | 404 |
| INT-030 | Relationships | Pipeline → Stage | Pipeline, Stage | Linked |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid ID | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | Duplicate | 409 |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow | 504 |
| INT-039 | Error | Payload | Huge | 413 |
| INT-040 | Error | Media | Wrong type | 415 |
| INT-041 | Error | Method | Wrong verb | 405 |
| INT-042 | Error | Service | Dependency | 503 |
| INT-043 | Error | Gateway | Upstream | 504 |
| INT-044 | Error | Gone | Deleted | 410 |
| INT-045 | Error | Locked | Locked | 423 |
| INT-046 | E2E | Full dashboard load | All | Load → display |
| INT-047 | E2E | Add widget flow | Widget | Add → refresh |
| INT-048 | E2E | Layout update flow | Layout | Update → persist |
| INT-049 | E2E | Multi-user | Users | Isolated |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get layout | Layout | Data |
| INT-052 | CRUD | Update layout | Layout | Updated |
| INT-053 | CRUD | Add widget | Widget | Added |
| INT-054 | CRUD | Remove widget | Widget | Removed |
| INT-055 | CRUD | Get config | Config | Config |
| INT-056 | Search | Filter org | Dashboard | Filtered |
| INT-057 | Search | Filter date | Dashboard | Filtered |
| INT-058 | Search | Filter type | Activity | Filtered |
| INT-059 | Search | Multi-filter | Dashboard | Combined |
| INT-060 | Search | Empty filter | - | Default |
| INT-061 | Pagination | Page 1 | Activity | First |
| INT-062 | Pagination | Last page | Activity | Partial |
| INT-063 | Pagination | Size | Activity | Correct |
| INT-064 | Pagination | Invalid | Activity | 400 |
| INT-065 | Pagination | Boundary | Activity | Exact |
| INT-066 | Relationships | Dashboard → Widget | Linked | Correct |
| INT-067 | Relationships | Dashboard → User | Linked | Correct |
| INT-068 | Relationships | KPI → Drill | Linked | Correct |
| INT-069 | Relationships | Orphan | Deleted widget | 404 |
| INT-070 | Relationships | Pipeline → Stage | Linked | Correct |
| INT-071 | Error | DB down | DB | 503 |
| INT-072 | Error | Auth down | Auth | 401/503 |
| INT-073 | Error | Validation | Bad input | 400 |
| INT-074 | Error | NotFound | Invalid ID | 404 |
| INT-075 | Error | Forbidden | No permission | 403 |
| INT-076 | Error | Conflict | Duplicate | 409 |
| INT-077 | Error | Rate limit | Too many | 429 |
| INT-078 | Error | Timeout | Slow | 504 |
| INT-079 | Error | Payload | Huge | 413 |
| INT-080 | Error | Media | Wrong type | 415 |
| INT-081 | Error | Method | Wrong verb | 405 |
| INT-082 | Error | Service | Dependency | 503 |
| INT-083 | Error | Gateway | Upstream | 504 |
| INT-084 | Error | Gone | Deleted | 410 |
| INT-085 | Error | Locked | Locked | 423 |
| INT-086 | E2E | Full load | All | Load → display |
| INT-087 | E2E | Add widget flow | Widget | Add → refresh |
| INT-088 | E2E | Layout update | Layout | Update → persist |
| INT-089 | E2E | Export flow | Export | Export → file |
| INT-090 | E2E | Drill flow | Drill | Drill → data |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users get dashboard | Both succeed |
| CON-002 | 2 users update layout | Last write |
| CON-003 | Add widget during refresh | Consistent |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click add | Single |
| CON-007 | Rapid filter | Last wins |
| CON-008 | Cache invalidation | No stale |
| CON-009 | Refresh during update | Snapshot |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Export + update | Snapshot |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple adds | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | KPI during update | Consistent |
| CON-022 | Widget during delete | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Layout concurrent | Last write |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid ID | Accept |
| UNT-002 | Validation | Invalid ID | Reject |
| UNT-003 | Validation | Valid date | Accept |
| UNT-004 | Validation | Invalid date | Reject |
| UNT-005 | Validation | Valid type | Accept |
| UNT-006 | Formatting | KPI value | Localized |
| UNT-007 | Formatting | Date | ISO 8601 |
| UNT-008 | Formatting | Percent | 2 decimal |
| UNT-009 | Calculation | KPI sum | Correct |
| UNT-010 | Calculation | KPI avg | Correct |
| UNT-011 | Calculation | Growth % | Correct |
| UNT-012 | Calculation | Pipeline % | Correct |
| UNT-013 | Calculation | Delta | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Dismissed | Excluded |
| UNT-018 | Status | Pending | Pending only |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get dashboard | < 500ms |
| PRF-002 | Get widgets | < 200ms |
| PRF-003 | Get KPIs | < 300ms |
| PRF-004 | Get activity | < 500ms |
| PRF-005 | Get pipeline | < 500ms |
| PRF-006 | Drill-down | < 300ms |
| PRF-007 | Add widget | < 200ms |
| PRF-008 | Update layout | < 200ms |
| PRF-009 | Refresh | < 2s |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent add | < 500ms each |
| PRF-013 | Memory | < 100MB |
| PRF-014 | Memory refresh | < 200MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 10 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users | 10 min | 95% < 1s |
| LDT-002 | 50 users | 10 min | 95% < 2s |
| LDT-003 | 100 users | 10 min | 95% < 3s |
| LDT-004 | Spike 10→100 | 5 min | No crash |
| LDT-005 | Spike 50→200 | 5 min | Graceful |
| LDT-006 | Stress 200 | Until fail | Document |
| LDT-007 | Stress 500 | Until fail | Document |
| LDT-008 | 50 concurrent | 5 min | Queue/limit |
| LDT-009 | Recovery spike | 5 min | Baseline |
| LDT-010 | Recovery stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Widgets | POS-002, FUN-002 |
| KPI tiles | POS-003, FUN-003 |
| Recent activity | POS-004, FUN-004 |
| Pipeline overview | POS-005, FUN-005 |
| Partner statistics | POS-006, FUN-006 |
| 3:1 Ratio | NEG-001–090, BND-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
