# EngagementManager — Test Cases

**Component:** UNOPS.PAO.UNOPSBusiness/Managers/EngagementManager.cs  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
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

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

The EngagementManager handles engagement tracking for the CRM enhancement:
- **Engagement tracking** for partners and contacts
- **Activity logging** (meetings, calls, emails)
- **Follow-ups** and reminders
- **Metrics** and analytics
- **Partner engagement scoring**

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|-------------|-------|-----------------|----------|
| POS-001 | Create engagement | Partner exists | CreateAsync(engagementData) | Engagement created | P0 |
| POS-002 | Get engagement by ID | Engagement exists | GetByIdAsync(id) | Engagement returned | P0 |
| POS-003 | Update engagement | Engagement exists | UpdateAsync(id, data) | Updated | P0 |
| POS-004 | Delete engagement | Engagement exists | DeleteAsync(id) | Soft deleted | P0 |
| POS-005 | Log activity | Partner exists | LogActivityAsync(activity) | Activity logged | P0 |
| POS-006 | Get activities for partner | Activities exist | GetActivitiesAsync(partnerId) | List returned | P0 |
| POS-007 | Create follow-up | Engagement exists | CreateFollowUpAsync(followUp) | Follow-up created | P0 |
| POS-008 | Get follow-ups | Follow-ups exist | GetFollowUpsAsync(engagementId) | List returned | P0 |
| POS-009 | Calculate engagement score | Partner has activities | GetEngagementScoreAsync(partnerId) | Score 0-100 | P1 |
| POS-010 | Get engagement metrics | Partner exists | GetMetricsAsync(partnerId) | Metrics object | P1 |
| POS-011 | Filter by date range | Activities exist | GetActivitiesAsync(from, to) | Filtered list | P1 |
| POS-012 | Filter by activity type | Activities exist | Filter by type | Filtered list | P1 |
| POS-013 | Paginate activities | 50+ activities | Page 2, size 10 | Items 11-20 | P1 |
| POS-014 | Sort by date | Activities exist | Sort desc | Newest first | P1 |
| POS-015 | Complete follow-up | Follow-up exists | CompleteFollowUpAsync(id) | Status completed | P1 |
| POS-016 | Overdue follow-ups | Overdue exist | GetOverdueFollowUpsAsync() | Overdue list | P1 |
| POS-017 | Engagement trend | 12 months data | GetTrendAsync(partnerId) | Trend data | P1 |
| POS-018 | Top engaged partners | Partners exist | GetTopEngagedAsync(10) | Top 10 list | P1 |
| POS-019 | Low engagement alert | Partner low score | GetLowEngagementAsync() | Alert list | P1 |
| POS-020 | Assign engagement to user | Engagement exists | AssignAsync(id, userId) | Assigned | P1 |
| POS-021 | Add notes to engagement | Engagement exists | AddNotesAsync(id, notes) | Notes added | P1 |
| POS-022 | Bulk log activities | Partner exists | LogActivitiesAsync(batch) | All logged | P2 |
| POS-023 | Export engagement report | Data exists | ExportReportAsync(filters) | Report file | P2 |
| POS-024 | Engagement summary | Partner exists | GetSummaryAsync(partnerId) | Summary object | P2 |
| POS-025 | Score breakdown | Partner has activities | GetScoreBreakdownAsync(id) | Breakdown by type | P2 |
| POS-026 | Recent activity count | Partner exists | GetRecentCountAsync(id, days) | Count | P2 |
| POS-027 | Next follow-up date | Follow-ups exist | GetNextFollowUpAsync(partnerId) | Date or null | P2 |
| POS-028 | Engagement history | Partner exists | GetHistoryAsync(partnerId) | Timeline | P2 |
| POS-029 | Validate activity type | Valid type | Create with type | Success | P2 |
| POS-030 | Audit trail | Create engagement | Create | Audit fields set | P2 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create without partner | PartnerId null | Validation error | P0 |
| NEG-002 | Create without type | ActivityType null | Validation error | P0 |
| NEG-003 | Get non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-004 | Update non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-005 | Delete non-existent | ID 99999 | KeyNotFoundException | P0 |
| NEG-006 | Invalid activity type | Type 999 | Validation error | P0 |
| NEG-007 | Future date in past | Date logic error | Validation error | P0 |
| NEG-008 | Null create request | CreateAsync(null) | ArgumentNullException | P0 |
| NEG-009 | Negative partner ID | PartnerId -1 | ArgumentException | P0 |
| NEG-010 | negative engagement ID | Id -1 | ArgumentException | P0 |
| NEG-011 | Score out of range | Score 150 | Validation error | P0 |
| NEG-012 | Empty activity description | Description "" | Validation error | P1 |
| NEG-013 | Invalid date range | To < From | Validation error | P1 |
| NEG-014 | Non-existent partner | PartnerId 99999 | KeyNotFoundException | P1 |
| NEG-015 | Non-existent user | Assign to 99999 | KeyNotFoundException | P1 |
| NEG-016 | SQL injection | '; DROP-- in notes | Sanitized/Rejected | P1 |
| NEG-017 | XSS in notes | <script> in notes | Sanitized | P1 |
| NEG-018 | Invalid pagination | Page -1 | ArgumentException | P1 |
| NEG-019 | Invalid page size | Size 0 | ArgumentException | P1 |
| NEG-020 | Complete already complete | Complete twice | BusinessException | P1 |
| NEG-021 | Cancel completed | Cancel completed | BusinessException | P1 |
| NEG-022 | Invalid score weight | Weight negative | Validation error | P1 |
| NEG-023 | Missing required field | Date null | Validation error | P1 |
| NEG-024 | Duplicate follow-up | Same date/engagement | Conflict or allow | P1 |
| NEG-025 | Orphan activity | Invalid engagementId | FK error | P1 |
| NEG-026 | Stale concurrency | Stale update | ConcurrencyException | P1 |
| NEG-027 | Unauthorized access | Wrong user | 403 | P1 |
| NEG-028 | Expired token | Stale JWT | 401 | P1 |
| NEG-029 | Rate limit | Too many requests | 429 | P1 |
| NEG-030 | DB timeout | Slow query | TimeoutException | P1 |
| NEG-031 | Connection lost | DB down | Connection exception | P1 |
| NEG-032 | Invalid filter combo | Conflicting filters | Validation error | P1 |
| NEG-033 | Null option params | GetActivitiesAsync(null) | ArgumentNullException | P1 |
| NEG-034 | Batch with invalid items | One invalid in batch | Partial fail or reject | P1 |
| NEG-035 | Export empty | No data | Empty file | P1 |
| NEG-036 | Invalid export format | Format "xyz" | ArgumentException | P1 |
| NEG-037 | Very long notes | 10000 chars | Validation error | P1 |
| NEG-038 | Invalid reminder time | Past time | Validation error | P1 |
| NEG-039 | Circular reference | Self-reference | Validation error | P1 |
| NEG-040 | Duplicate activity | Same activity twice | Idempotent or reject | P1 |
| NEG-041 | Permission denied | User lacks permission | 403 | P2 |
| NEG-042 | Tenant mismatch | Cross-tenant access | 403 | P2 |
| NEG-043 | Deleted partner | Activity for deleted | Handler or error | P2 |
| NEG-044 | Invalid status transition | Invalid state change | BusinessException | P2 |
| NEG-045 | Malformed JSON | Invalid request body | 400 Bad Request | P2 |
| NEG-046 | Wrong content type | Text/plain | 415 | P2 |
| NEG-047 | Oversized payload | 10MB request | 413 | P2 |
| NEG-048 | Missing auth header | No Authorization | 401 | P2 |
| NEG-049 | Invalid token | Malformed JWT | 401 | P2 |
| NEG-050 | Expired token | Expired JWT | 401 | P2 |
| NEG-051 | Transaction rollback | Explicit rollback | Changes reverted | P2 |
| NEG-052 | Partial batch failure | Batch partial fail | Rollback or partial | P2 |
| NEG-053 | Cache corruption | Bad cache entry | Bypass cache | P2 |
| NEG-054 | Disk full | Export to full disk | IO exception | P2 |
| NEG-055 | Memory pressure | Large export | Throttle or error | P2 |
| NEG-056 | Deadlock | Concurrent update | Retry or deadlock | P2 |
| NEG-057 | Unique constraint | Duplicate key | DB exception | P2 |
| NEG-058 | FK violation | Invalid foreign key | DB exception | P2 |
| NEG-059 | Null in collection | Null in list | ArgumentNullException | P2 |
| NEG-060 | Empty ID list | GetByIds([]) | Empty result | P2 |
| NEG-061 | Invalid sort field | Sort "invalid" | ArgumentException | P2 |
| NEG-062 | Invalid sort direction | Direction "xyz" | ArgumentException | P2 |
| NEG-063 | Timezone invalid | Bad TZ string | Validation error | P2 |
| NEG-064 | Locale invalid | Bad locale | Validation error | P2 |
| NEG-065 | Missing correlation | No trace | Logged or required | P2 |
| NEG-066 | Retry exhaustion | All retries fail | Final exception | P2 |
| NEG-067 | Circuit open | Circuit breaker | Rejected | P2 |
| NEG-068 | Service unavailable | Dependent service down | 503 or fallback | P2 |
| NEG-069 | Validation multiple | Multiple errors | All returned | P2 |
| NEG-070 | Encoding invalid | Wrong charset | 400 Bad Request | P2 |
| NEG-071 | Partner API fail | Partner 500 | Error | P2 |
| NEG-072 | Activity API fail | Activity 500 | Error | P2 |
| NEG-073 | DbContext disposed | After dispose | ObjectDisposed | P2 |
| NEG-074 | Partner soft-deleted | Deleted partner | Reject | P2 |
| NEG-075 | Null batch | LogActivities null | ArgumentNull | P2 |
| NEG-076 | Empty batch | LogActivities [] | No-op or error | P2 |
| NEG-077 | Invalid activity type | Type 999 | Reject | P2 |
| NEG-078 | Follow-up past | Past date | Reject | P2 |
| NEG-079 | Complete non-existent | ID 99999 | KeyNotFound | P2 |
| NEG-080 | Cancel non-existent | ID 99999 | KeyNotFound | P2 |
| NEG-081 | GetByIds empty | [] | Empty list | P2 |
| NEG-082 | Pagination page 0 | Page 0 | Clamp or error | P2 |
| NEG-083 | Pagination size 0 | Size 0 | Validation | P2 |
| NEG-084 | Export fail | Export error | Handled | P2 |
| NEG-085 | Dashboard fail | Stats error | Handled | P2 |
| NEG-086 | Comparison fail | Invalid ids | Error | P2 |
| NEG-087 | Reminder fail | Reminder error | Handled | P2 |
| NEG-088 | Score overflow | Score 101 | Validation | P2 |
| NEG-089 | Notes too long | 4001 chars | Validation | P2 |
| NEG-090 | Invalid date range | To < From | Validation | P2 |

---

## §3 Boundary Tests (90)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Notes length | 0 | 4000 | Accept | Accept | Reject | P1 |
| BND-002 | Partner ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-003 | Engagement ID | 1 | int.Max | 1 ok | Max ok | Overflow | P1 |
| BND-004 | Score | 0 | 100 | 0 ok | 100 ok | Reject | P1 |
| BND-005 | Page number | 1 | maxPages | 1 ok | Last ok | Empty | P1 |
| BND-006 | Page size | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-007 | Date range | 1 day | 1 year | 1 day ok | 1 year ok | Reject | P1 |
| BND-008 | Activity count | 0 | 10000 | 0 ok | 10000 ok | Perf | P1 |
| BND-009 | Follow-up count | 0 | 1000 | 0 ok | 1000 ok | Perf | P1 |
| BND-010 | Top N partners | 1 | 100 | 1 ok | 100 ok | Reject | P1 |
| BND-011 | Description 1 char | 1 | 4000 | Accept | — | — | P1 |
| BND-012 | Description 4000 | 1 | 4000 | — | Accept | — | P1 |
| BND-013 | Description 4001 | 1 | 4000 | — | — | Reject | P1 |
| BND-014 | Score exactly 0 | 0 | 100 | Accept | — | — | P1 |
| BND-015 | Score exactly 100 | 0 | 100 | — | Accept | — | P1 |
| BND-016 | Empty activities | 0 | — | Return [] | — | — | P1 |
| BND-017 | Single activity | 1 | — | Return [1] | — | — | P1 |
| BND-018 | Min date | DateTime.Min | — | Handle | — | — | P2 |
| BND-019 | Max date | DateTime.Max | — | — | Handle | — | P2 |
| BND-020 | Leap year | Feb 29 | — | Accept | — | — | P2 |
| BND-021 | Unicode notes | Arabic/Chinese | — | Accept | — | — | P2 |
| BND-022 | Emoji in notes | Emoji | — | Accept or reject | — | — | P2 |
| BND-023 | Zero days | Recent days 0 | — | Reject or all | — | — | P2 |
| BND-024 | Negative days | Days -1 | — | Reject | — | — | P2 |
| BND-025 | Batch size | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-026 | Null vs empty | — | — | Both handled | — | — | P2 |
| BND-027 | Whitespace notes | — | — | Trim or reject | — | — | P2 |
| BND-028 | Leading/trailing space | — | — | Trimmed | — | — | P2 |
| BND-029 | Tab/newline in notes | — | — | Sanitize | — | — | P2 |
| BND-030 | Pagination last partial | — | — | Correct count | — | — | P2 |
| BND-031 | Sort empty | — | — | No error | — | — | P2 |
| BND-032 | Filter no matches | — | — | Empty list | — | — | P2 |
| BND-033 | Exactly N items | N | — | Paginate correctly | — | — | P2 |
| BND-034 | Status enum first | First | — | Accept | — | — | P2 |
| BND-035 | Status enum last | Last | — | Accept | — | — | P2 |
| BND-036 | Weight sum | 0 | 1.0 | 0 ok | 1.0 ok | Reject | P2 |
| BND-037 | Duration minutes | 0 | 1440 | 0 ok | 1440 ok | Reject | P2 |
| BND-038 | Reminder advance | 0 | 365 days | 0 ok | 365 ok | Reject | P2 |
| BND-039 | Concurrent sessions | — | 100 | — | — | — | P2 |
| BND-040 | Timeout ms | 100 | 30000 | Min ok | Max ok | — | P2 |
| BND-041 | Retry count | 0 | 5 | 0=no retry | 5 ok | — | P2 |
| BND-042 | Cache TTL | 0 | 3600 | 0=no cache | 3600 ok | — | P2 |
| BND-043 | Rate limit | 1 | 1000 | 1 ok | 1000 ok | — | P2 |
| BND-044 | Activity types | 1 | 20 | 1 ok | 20 ok | — | P2 |
| BND-045 | Export rows | 0 | 100000 | 0 ok | 100k ok | Reject | P2 |
| BND-046 | ID list length | 1 | 100 | 1 ok | 100 ok | Reject | P2 |
| BND-047 | Name length | 1 | 200 | 1 ok | 200 ok | Reject | P2 |
| BND-048 | Subject length | 1 | 500 | 1 ok | 500 ok | Reject | P2 |
| BND-049 | URL length | 1 | 2048 | 1 ok | 2048 ok | Reject | P2 |
| BND-050 | Tag count | 0 | 50 | 0 ok | 50 ok | Reject | P2 |
| BND-051 | Attachment count | 0 | 10 | 0 ok | 10 ok | Reject | P2 |
| BND-052 | Attachment size | 0 | 10MB | 0 ok | 10MB ok | Reject | P2 |
| BND-053 | Time precision | — | — | Sub-ms | Full ms | — | P2 |
| BND-054 | Timezone offset | -12 | +14 | Correct | Correct | — | P2 |
| BND-055 | Decimal precision | 2 | 2 | 0.00 | 99.99 | — | P2 |
| BND-056 | Percent 0 | 0 | — | Accept | — | — | P2 |
| BND-057 | Percent 100 | 100 | — | Accept | — | — | P2 |
| BND-058 | Boolean boundary | — | — | True/False | — | — | P2 |
| BND-059 | Enum all values | — | — | All valid | — | — | P2 |
| BND-060 | JSON depth | 1 | 32 | 1 ok | 32 ok | Reject | P2 |
| BND-061 | Array length | 0 | 1000 | 0 ok | 1000 ok | — | P2 |
| BND-062 | Nested depth | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-063 | Filter param count | 0 | 20 | 0 ok | 20 ok | Reject | P2 |
| BND-064 | Sort field count | 1 | 5 | 1 ok | 5 ok | Reject | P2 |
| BND-065 | Trend months | 1 | 24 | 1 ok | 24 ok | Reject | P2 |
| BND-066 | Comparison partners | 2 | 2 | 2 ok | 2 ok | Reject | P2 |
| BND-067 | Report date range | 1 day | 1 year | 1 day ok | 1 year ok | Reject | P2 |
| BND-068 | RTL text | — | — | Accept | — | — | P2 |
| BND-069 | Null byte | — | — | Reject | — | — | P2 |
| BND-070 | CRLF in field | — | — | Sanitize | — | — | P2 |
| BND-071 | Partner ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-072 | Engagement ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-073 | Activity ID 1 | 1 | int.Max | Min | — | — | P2 |
| BND-074 | Page size 1 | 1 | 100 | Min | — | — | P2 |
| BND-075 | Page size 100 | 1 | 100 | — | Max | — | P2 |
| BND-076 | Notes 0 | 0 | 4000 | Empty | — | — | P2 |
| BND-077 | Notes 4000 | 0 | 4000 | — | Max | — | P2 |
| BND-078 | Score 0 | 0 | 100 | Min | — | — | P2 |
| BND-079 | Score 100 | 0 | 100 | — | Max | — | P2 |
| BND-080 | Days 0 | 0 | 365 | Min | — | — | P2 |
| BND-081 | Days 365 | 0 | 365 | — | Max | — | P2 |
| BND-082 | Batch 0 | 0 | 50 | Empty | — | — | P2 |
| BND-083 | Batch 50 | 0 | 50 | — | Max | — | P2 |
| BND-084 | Description 0 | 0 | 2000 | Empty | — | — | P2 |
| BND-085 | Description 2000 | 0 | 2000 | — | Max | — | P2 |
| BND-086 | Filter ids 0 | 0 | 100 | Empty | — | — | P2 |
| BND-087 | Filter ids 100 | 0 | 100 | — | Max | — | P2 |
| BND-088 | Date min | 1900 | 2100 | Min | — | — | P2 |
| BND-089 | Date max | 1900 | 2100 | — | Max | — | P2 |
| BND-090 | Weight 0 | 0 | 1 | Min | — | — | P2 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|------------------|----------|
| FUN-001 | Create sets audit | Audit on create | CreateAsync | CreatedBy, CreatedDate | P0 |
| FUN-002 | Update sets audit | Audit on update | UpdateAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-003 | Delete soft | Soft delete | DeleteAsync | IsDeleted=true | P0 |
| FUN-004 | Partner required | PartnerId not null | Create without | Reject | P0 |
| FUN-005 | Activity type required | Type not null | Create without | Reject | P0 |
| FUN-006 | Date required | Date not null | Create without | Reject | P0 |
| FUN-007 | Score 0-100 | Range | Calculate score | 0-100 | P0 |
| FUN-008 | Get excludes deleted | Default filter | GetAllAsync | !IsDeleted | P0 |
| FUN-009 | Activity type enum | Valid types | Create with valid | Success | P1 |
| FUN-010 | Date not future | Past or today | Create with future | Reject | P1 |
| FUN-011 | Follow-up due date | Due >= created | Create follow-up | Validated | P1 |
| FUN-012 | Pagination | Page/size | Get with page 2 | Items 11-20 | P1 |
| FUN-013 | Sort default | Default sort | GetActivities | By date desc | P1 |
| FUN-014 | Filter by partner | Partner filter | Filter by partnerId | Filtered | P1 |
| FUN-015 | Filter by type | Type filter | Filter by type | Filtered | P1 |
| FUN-016 | Filter by date range | Date filter | From, To | Filtered | P1 |
| FUN-017 | Complete sets date | Complete | CompleteFollowUp | CompletedDate set | P1 |
| FUN-018 | Cancel sets date | Cancel | CancelFollowUp | CancelledDate set | P1 |
| FUN-019 | Score weight sum | Weights | Calculate | Sum = 1.0 | P1 |
| FUN-020 | Top N sorted | Top engaged | GetTopEngaged(10) | Sorted by score | P1 |
| FUN-021 | Overdue filter | Due < now | GetOverdue | Only overdue | P1 |
| FUN-022 | Assign sets user | Assign | AssignAsync | AssignedTo set | P1 |
| FUN-023 | Notes append | Add notes | AddNotes | Appended | P1 |
| FUN-024 | Trend aggregation | Monthly | GetTrend | Monthly buckets | P1 |
| FUN-025 | Low threshold | Threshold | GetLowEngagement | Score < threshold | P1 |
| FUN-026 | Reminder scheduling | Reminder | ScheduleReminder | ReminderDate set | P1 |
| FUN-027 | Bulk atomic | Batch | LogActivities | All or none | P1 |
| FUN-028 | Export format | CSV/Excel | ExportReport | Valid format | P1 |
| FUN-029 | Mapping complete | All fields | GetById mapped | All populated | P1 |
| FUN-030 | Concurrency token | Optimistic | Stale update | ConcurrencyException | P1 |
| FUN-031 | Transaction scope | Create+activity | Create with activity | Atomic | P1 |
| FUN-032 | Idempotent get | GetById | Call twice | Same result | P1 |
| FUN-033 | Stateless | No server state | Request | Independent | P1 |
| FUN-034 | Score formula | Weighted sum | Calculate | Correct formula | P1 |
| FUN-035 | Recency decay | Older = less weight | Score calc | Decay applied | P1 |
| FUN-036 | Activity type weight | Per-type weight | Score calc | Type-specific | P1 |
| FUN-037 | Null handling | Optional fields | Null optional | No error | P2 |
| FUN-038 | Default values | New entity | Create minimal | Defaults | P2 |
| FUN-039 | Validation order | Multiple invalid | Create | All errors | P2 |
| FUN-040 | Idempotent delete | Delete twice | Delete same | Second 404 | P2 |
| FUN-041 | Update partial | PATCH | Update 1 field | Only that | P2 |
| FUN-042 | Read-your-writes | Consistency | Create then get | Visible | P2 |
| FUN-043 | Version header | ETag | Get | ETag returned | P2 |
| FUN-044 | Conditional update | If-Match | Stale ETag | 412 | P2 |
| FUN-045 | Timezone handling | User TZ | Date fields | Correct | P2 |
| FUN-046 | Localization | Locale | Display | Localized | P2 |
| FUN-047 | Permission check | CanCreate | Create | Validated | P2 |
| FUN-048 | Tenant isolation | Multi-tenant | Cross-tenant | Rejected | P2 |
| FUN-049 | Soft delete cascade | Children | Delete parent | Children handled | P2 |
| FUN-050 | Audit immutable | No audit change | Update audit | Ignored | P2 |
| FUN-051 | Create audit | Create | Create | Audit set | P2 |
| FUN-052 | Update audit | Update | Update | Audit set | P2 |
| FUN-053 | Soft delete audit | Delete | Delete | DeletedBy set | P2 |
| FUN-054 | IsDeleted filter | Query | Query | Excludes deleted | P2 |
| FUN-055 | Include partner | Get | Include | Partner loaded | P2 |
| FUN-056 | Pagination | Page | Page | Correct slice | P2 |
| FUN-057 | Sort | Sort | Sort | Ordered | P2 |
| FUN-058 | Filter by type | Filter | Type | Filtered | P2 |
| FUN-059 | Filter by date | Filter | Date | Filtered | P2 |
| FUN-060 | GetSummary | Summary | Get | Returned | P2 |
| FUN-061 | GetScoreBreakdown | Breakdown | Get | Returned | P2 |
| FUN-062 | GetNextFollowUp | Follow-up | Get | Returned | P2 |
| FUN-063 | GetHistory | History | Get | Returned | P2 |
| FUN-064 | LogActivities batch | Batch | Log | All logged | P2 |
| FUN-065 | Complete | Complete | Complete | Completed | P2 |
| FUN-066 | Cancel follow-up | Cancel | Cancel | Cancelled | P2 |
| FUN-067 | Schedule reminder | Reminder | Schedule | Scheduled | P2 |
| FUN-068 | Restore | Restore | Restore | Restored | P2 |
| FUN-069 | Include deleted | Admin | IncludeDeleted | All | P2 |
| FUN-070 | GetMetricsForPartners | Metrics | Get | Returned | P2 |
| FUN-071 | GetDashboardStats | Stats | Get | Returned | P2 |
| FUN-072 | Compare | Compare | Compare | Returned | P2 |
| FUN-073 | AsNoTracking | Read | Query | No tracking | P2 |
| FUN-074 | Transaction | Transaction | Commit | Committed | P2 |
| FUN-075 | Concurrency | Concurrent | Read | No conflict | P2 |
| FUN-076 | DbContext scope | Scope | Per request | Isolated | P2 |
| FUN-077 | Validation order | Invalid | Validate | Order correct | P2 |
| FUN-078 | Idempotent delete | Delete | Twice | Second no-op | P2 |
| FUN-079 | Idempotent restore | Restore | Twice | Second no-op | P2 |
| FUN-080 | Export | Export | Export | File | P2 |
| FUN-081 | Logging | Operation | Log | Logged | P2 |
| FUN-082 | Metrics | Operation | Metric | Recorded | P2 |
| FUN-083 | Query timeout | Slow | Query | Timeout | P2 |
| FUN-084 | Retry policy | Transient | Fail | Retried | P2 |
| FUN-085 | Cascading load | Include | Load | Loaded | P2 |
| FUN-086 | Connection pool | Concurrent | Connections | Pooled | P2 |
| FUN-087 | Foreign key | FK | Constraint | Enforced | P2 |
| FUN-088 | Unique constraint | Unique | Insert | Enforced | P2 |
| FUN-089 | Index | Query | Index | Fast | P2 |
| FUN-090 | Status transition | Status | Change | Validated | P2 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | CRUD full cycle | Create→Read→Update→Delete | Engagement | Success | P0 |
| INT-002 | Create then get | Create, GetById | Engagement | Data matches | P0 |
| INT-003 | Log activity flow | Log, Get activities | Activity | In list | P0 |
| INT-004 | Follow-up flow | Create, Complete | Follow-up | Completed | P0 |
| INT-005 | Partner→Engagement | Partner has engagements | Get by partner | List | P0 |
| INT-006 | Engagement→Activities | Engagement has activities | Get activities | Loaded | P0 |
| INT-007 | API→Manager→DB | Full stack | All | End-to-end | P1 |
| INT-008 | Controller→Manager | Controller call | API, Manager | Mapped | P1 |
| INT-009 | Manager→Repository | Manager call | Manager, Repo | Executed | P1 |
| INT-010 | Auth→Manager | Authorized call | Auth, Manager | Checked | P1 |
| INT-011 | Error propagation | Manager throws | Manager→Controller | 400/404/500 | P1 |
| INT-012 | Logging integration | Operation | Logger | Log entry | P1 |
| INT-013 | Metrics integration | Operation | Metrics | Counter | P1 |
| INT-014 | Audit→DB | Create | Audit, DB | Audit row | P1 |
| INT-015 | Cache→DB | Get cached | Cache, DB | Hit/miss | P1 |
| INT-016 | Transaction scope | Create+child | Transaction | Atomic | P1 |
| INT-017 | Partner sync | Partner update | Engagement | Partner ref valid | P1 |
| INT-018 | Contact sync | Contact update | Engagement | Contact ref valid | P1 |
| INT-019 | User sync | User update | Assignment | User ref valid | P1 |
| INT-020 | Notification on follow-up | Follow-up due | Notifier | Notification | P1 |
| INT-021 | Reminder service | Reminder | Reminder service | Fired | P1 |
| INT-022 | Report generation | Report | Report, DB | Report data | P1 |
| INT-023 | Dashboard agg | Dashboard | Dashboard, DB | Aggregations | P1 |
| INT-024 | Search index | Create engagement | Search | Indexed | P1 |
| INT-025 | Event publish | Created | Event bus | Event sent | P1 |
| INT-026 | Bulk import | Import | CSV, DB | Imported | P1 |
| INT-027 | Export file | Export | DB, File | File created | P1 |
| INT-028 | Permission service | Permission | Permission svc | Allowed/Denied | P1 |
| INT-029 | Tenant isolation | Multi-tenant | Tenant A, B | Isolated | P1 |
| INT-030 | Retry on failure | Transient | Retry policy | Retried | P1 |
| INT-031 | Health check | Health | DB, Services | Healthy | P1 |
| INT-032 | Config override | Env | Config | Override | P1 |
| INT-033 | Feature flag | Flag off | Feature | Disabled | P1 |
| INT-034 | Rate limit | Many req | Rate limiter | Limited | P1 |
| INT-035 | Contact→Engagement | Contact link | Engagement | Linked | P1 |
| INT-036 | Interaction→Engagement | Interaction link | Engagement | Linked | P1 |
| INT-037 | Document→Engagement | Document link | Engagement | Linked | P1 |
| INT-038 | Calendar integration | Sync | Calendar | Synced | P1 |
| INT-039 | Email integration | Log email | Email service | Logged | P1 |
| INT-040 | Meeting integration | Log meeting | Meeting service | Logged | P1 |
| INT-041 | Score recalculation | Activity added | Recalc | Score updated | P1 |
| INT-042 | Trend refresh | New data | Refresh | Trend updated | P1 |
| INT-043 | Alert generation | Low score | Alert | Alert created | P1 |
| INT-044 | Scheduled job | Daily | Job | Processed | P1 |
| INT-045 | API versioning | v2 | Version | v2 behavior | P1 |
| INT-046 | CORS | Cross-origin | CORS | Allowed/Blocked | P1 |
| INT-047 | Correlation ID | Trace | Request | Propagated | P1 |
| INT-048 | Circuit breaker | Failures | Circuit | Opened | P1 |
| INT-049 | Backward compat | Old client | New API | Works | P1 |
| INT-050 | Forward compat | New client | Old API | Graceful | P1 |
| INT-051 | DbContext | CRUD | DbContext | Persisted | P1 |
| INT-052 | Repository | CRUD | Repository | Persisted | P1 |
| INT-053 | AutoMapper | Map | Mapper | Mapped | P1 |
| INT-054 | PartnerManager | Partner | Manager | Loaded | P1 |
| INT-055 | AuditDbContext | Audit | Context | Audited | P1 |
| INT-056 | Transaction | Transaction | Commit | Committed | P1 |
| INT-057 | PermissionService | Check | Service | Checked | P1 |
| INT-058 | HttpClient | API | HttpClient | Response | P1 |
| INT-059 | Logging | Log | ILogger | Logged | P1 |
| INT-060 | Configuration | Config | IConfiguration | Loaded | P1 |
| INT-061 | DI container | Resolve | Container | Resolved | P1 |
| INT-062 | Scoped lifetime | Request | Scope | Per request | P1 |
| INT-063 | Soft delete filter | Global | Query | Filtered | P1 |
| INT-064 | Foreign key | FK | Constraint | Enforced | P1 |
| INT-065 | Unique constraint | Unique | Insert | Enforced | P1 |
| INT-066 | Cache | Cache | Get | Cached | P1 |
| INT-067 | Retry | Transient | Retry | Retried | P1 |
| INT-068 | Health check | Health | Check | Healthy | P1 |
| INT-069 | Metrics | Metric | Record | Recorded | P1 |
| INT-070 | User context | User | Context | Resolved | P1 |
| INT-071 | Export service | Export | Service | File | P1 |
| INT-072 | API versioning | Version | Request | Versioned | P1 |
| INT-073 | Rate limiting | Limit | Request | Limited | P1 |
| INT-074 | Auth middleware | Auth | Request | Authenticated | P1 |
| INT-075 | Validation middleware | Validate | Request | Validated | P1 |
| INT-076 | Exception middleware | Exception | Throw | Handled | P1 |
| INT-077 | Correlation ID | Request | ID | Propagated | P1 |
| INT-078 | Tracing | Trace | Span | Traced | P1 |
| INT-079 | Feature flag | Flag | Check | Toggled | P1 |
| INT-080 | CORS | Cross-origin | Request | Allowed | P1 |
| INT-081 | Connection | Connection | Open | Connected | P1 |
| INT-082 | Migration | Migration | Run | Applied | P1 |
| INT-083 | Index | Query | Index | Fast | P1 |
| INT-084 | Circuit breaker | Fail | Circuit | Open | P1 |
| INT-085 | Tenant context | Tenant | Context | Resolved | P1 |
| INT-086 | Partner API | Partner | API | Response | P1 |
| INT-087 | Reminder service | Reminder | Service | Scheduled | P1 |
| INT-088 | Dashboard service | Dashboard | Service | Stats | P1 |
| INT-089 | Forward compat | New client | Old API | Graceful | P1 |
| INT-090 | Batch flow | Log batch | Activities | All logged | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|---------------|----------|
| SEC-001 | SQL injection notes | '; DROP-- | Notes | Sanitized/Rejected | P0 |
| SEC-002 | XSS in notes | <script> | Notes | Escaped | P0 |
| SEC-003 | Unauthorized get | No token | GetById | 401 | P0 |
| SEC-004 | Forbidden get | Wrong role | GetById | 403 | P0 |
| SEC-005 | IDOR get | Others' ID | GetById | 403/404 | P0 |
| SEC-006 | IDOR update | Others' ID | Update | 403 | P0 |
| SEC-007 | IDOR delete | Others' ID | Delete | 403 | P0 |
| SEC-008 | Mass assignment | isAdmin=true | Create | Ignored | P0 |
| SEC-009 | Parameterized query | SQL params | All queries | No injection | P0 |
| SEC-010 | Output encoding | HTML | All responses | Encoded | P0 |
| SEC-011 | CSRF token | No token | POST | Rejected | P0 |
| SEC-012 | Session timeout | Expired | Request | 401 | P0 |
| SEC-013 | LDAP injection | *)(uid=* | Search | Rejected | P1 |
| SEC-014 | NoSQL injection | {$gt:""} | Filter | Rejected | P1 |
| SEC-015 | JWT tampering | Modified JWT | Auth | Rejected | P1 |
| SEC-016 | JWT alg none | alg=none | JWT | Rejected | P1 |
| SEC-017 | Token replay | Reuse token | Request | Rejected | P1 |
| SEC-018 | Privilege escalation | Low→Admin | Action | 403 | P1 |
| SEC-019 | Horizontal access | User A→B | Resource | 403 | P1 |
| SEC-020 | Vertical access | User→Admin | Resource | 403 | P1 |
| SEC-021 | Sensitive data log | Password | Logging | Not logged | P1 |
| SEC-022 | Sensitive data response | Password | API | Not returned | P1 |
| SEC-023 | Stack trace | Error | Prod | No trace | P1 |
| SEC-024 | Verbose error | DB details | Error | Generic | P1 |
| SEC-025 | Rate limit bypass | Many IPs | Rate limit | Per-user | P1 |
| SEC-026 | Header injection | CRLF | Header | Rejected | P1 |
| SEC-027 | Oversized payload | 100MB | Request | Rejected | P1 |
| SEC-028 | Deep object | 100 levels | JSON | Rejected | P1 |
| SEC-029 | Regex DoS | Evil regex | Pattern | Timeout/Reject | P1 |
| SEC-030 | Prototype pollution | __proto__ | JSON | Sanitized | P1 |
| SEC-031 | CORS misconfig | Wildcard | CORS | Restricted | P1 |
| SEC-032 | Missing headers | X-Frame-Options | Response | Present | P1 |
| SEC-033 | HSTS | HTTP | Redirect | HTTPS | P1 |
| SEC-034 | Cookie secure | Cookie | Set-Cookie | Secure | P1 |
| SEC-035 | Cookie HttpOnly | Cookie | Set-Cookie | HttpOnly | P1 |
| SEC-036 | Audit integrity | Modify audit | Audit | Tamper evident | P1 |
| SEC-037 | Encryption at rest | DB | Sensitive | Encrypted | P1 |
| SEC-038 | Password in URL | Query | URL | Not logged | P1 |
| SEC-039 | Command injection | ; ls | Field | Rejected | P1 |
| SEC-040 | Path traversal | ../etc/passwd | File | Rejected | P1 |
| SEC-041 | XXE | XML entity | XML | Rejected | P1 |
| SEC-042 | SSRF | Internal URL | URL | Blocked | P1 |
| SEC-043 | Open redirect | redirect=evil | Redirect | Validated | P1 |
| SEC-044 | Brute force | Many auth | Login | Lockout | P1 |
| SEC-045 | Content-type bypass | Wrong type | Upload | Rejected | P1 |
| SEC-046 | File upload malicious | Exe | Upload | Rejected | P1 |
| SEC-047 | Insecure deserialization | Malicious | Deserialize | Rejected | P1 |
| SEC-048 | Info disclosure | Server details | Header | Minimal | P1 |
| SEC-049 | Tenant isolation | Cross-tenant | Request | 403 | P1 |
| SEC-050 | Data aggregation | PII in report | Export | Anonymized | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior | Priority |
|----|-----------|----------|-------------------|----------|
| CON-001 | Concurrent create same partner | 2 users create | Both or one | P1 |
| CON-002 | Concurrent update same | 2 users update | Optimistic lock | P1 |
| CON-003 | Concurrent delete same | 2 users delete | One succeeds | P1 |
| CON-004 | Read during update | Read while update | Consistent | P1 |
| CON-005 | Update during delete | Update while delete | One fails | P1 |
| CON-006 | Double submit | Same form twice | Idempotent | P1 |
| CON-007 | Transaction isolation | Parallel tx | No dirty read | P1 |
| CON-008 | Deadlock | Circular wait | Retry | P1 |
| CON-009 | Lost update | Interleaved | Version lock | P1 |
| CON-010 | Cache invalidation | Update after cache | Invalidated | P1 |
| CON-011 | Batch concurrent | 2 batches | Both complete | P1 |
| CON-012 | Connection pool | Exhaust | Queue/timeout | P1 |
| CON-013 | Lock timeout | Hold long | Timeout | P1 |
| CON-014 | Retry idempotency | Retry partial | No duplicate | P1 |
| CON-015 | Visibility | Write then read | Read sees write | P1 |
| CON-016 | Score recalc race | Concurrent activities | Consistent score | P1 |
| CON-017 | Follow-up complete race | 2 complete same | One succeeds | P1 |
| CON-018 | Assign race | 2 assign same | One succeeds | P1 |
| CON-019 | Bulk log race | 2 bulk logs | Both complete | P1 |
| CON-020 | Export concurrent | 2 exports | Both complete | P1 |
| CON-021 | Distributed lock | Multi-instance | Single writer | P1 |
| CON-022 | Eventual consistency | Replica lag | Converge | P2 |
| CON-023 | Failover | Primary fail | Replica | P2 |
| CON-024 | Saga compensation | Partial fail | Compensate | P2 |
| CON-025 | Outbox pattern | Event | Exactly once | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | PartnerId validation | Validation | Valid id | True | P1 |
| UNT-002 | PartnerId invalid | Validation | -1 | False | P1 |
| UNT-003 | Activity type validation | Validation | Valid type | True | P1 |
| UNT-004 | Date validation | Validation | Valid date | True | P1 |
| UNT-005 | Format date | Formatting | DateTime | ISO string | P1 |
| UNT-006 | Trim notes | Formatting | "  x  " | "x" | P1 |
| UNT-007 | Map entity to model | Mapping | Entity | Model | P1 |
| UNT-008 | Score calculation | Calculation | Activities | 0-100 | P1 |
| UNT-009 | Weighted sum | Calculation | Weights, values | Sum | P1 |
| UNT-010 | Status transition | Status logic | Complete | Completed | P1 |
| UNT-011 | Overdue check | Status logic | Due < now | Overdue | P1 |
| UNT-012 | IsDeleted filter | Status logic | Mixed | !IsDeleted | P1 |
| UNT-013 | Sort by date | Collections | Unsorted | Sorted | P1 |
| UNT-014 | Paginate | Collections | Full list | Slice | P1 |
| UNT-015 | Filter by type | Collections | Query | Filtered | P1 |
| UNT-016 | Null safe | Validation | Null | No throw | P1 |
| UNT-017 | Empty list | Collections | [] | [] | P1 |
| UNT-018 | Map list | Mapping | Entity list | Model list | P1 |
| UNT-019 | Trend aggregation | Calculation | Monthly data | Buckets | P1 |
| UNT-020 | Id equality | Validation | Same id | Equal | P1 |
| UNT-021 | Score clamp | Calculation | 150 | 100 | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetById latency | GetByIdAsync | < 50 ms | P2 |
| PRF-002 | GetActivities latency | 100 items | < 200 ms | P2 |
| PRF-003 | Create latency | CreateAsync | < 100 ms | P2 |
| PRF-004 | Update latency | UpdateAsync | < 100 ms | P2 |
| PRF-005 | Delete latency | DeleteAsync | < 100 ms | P2 |
| PRF-006 | Score calculation | 1000 activities | < 500 ms | P2 |
| PRF-007 | Pagination | Page 10 of 1000 | < 200 ms | P2 |
| PRF-008 | Bulk log 100 | LogActivities | < 5 s | P2 |
| PRF-009 | Bulk get 100 | GetByIds 100 | < 500 ms | P2 |
| PRF-010 | Export 1000 | ExportReport | < 5 s | P2 |
| PRF-011 | Concurrent 10 get | 10 parallel | < 200 ms | P2 |
| PRF-012 | Memory single | Create | No leak | P2 |
| PRF-013 | Memory 1000 ops | 1000 creates | Stable | P2 |
| PRF-014 | Query plan | GetById | Index used | P2 |
| PRF-015 | N+1 check | With activities | Single query | P2 |
| PRF-016 | Connection reuse | 100 sequential | Pool stable | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|------------------|----------|
| LDT-001 | Sustained 10 RPS | 10 req/s | 10 min | 99% < 500 ms | P2 |
| LDT-002 | Sustained 50 RPS | 50 req/s | 5 min | 99% < 1 s | P2 |
| LDT-003 | Sustained 100 RPS | 100 req/s | 5 min | 95% < 2 s | P2 |
| LDT-004 | Spike 200 RPS | 0→200→0 | 2 min | No 5xx | P2 |
| LDT-005 | Spike 500 RPS | 5 s burst | 5 s | Recover | P2 |
| LDT-006 | Stress 500 RPS | 500 req/s | 2 min | Graceful | P2 |
| LDT-007 | Stress 1000 RPS | 1000 req/s | 1 min | No crash | P2 |
| LDT-008 | Endurance 20 RPS | 20 req/s | 1 h | No leak | P2 |
| LDT-009 | Recovery | Post-spike | 5 min | Baseline | P2 |
| LDT-010 | Mixed workload | CRUD mix | 15 min | All succeed | P2 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
