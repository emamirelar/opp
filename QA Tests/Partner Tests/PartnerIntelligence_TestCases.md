# PNO-108: Partner Intelligence — Test Cases

**JIRA Reference:** [PNO-108](https://unops.atlassian.net/browse/PNO-108)  
**Created:** 2026-02-04  
**Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 35 | 30-50 | ✅ |
| Negative Tests | §2 | 70 | Max(50, 2×35)=70 | ✅ |
| Boundary Tests | §3 | 70 | Max(50, 2×35)=70 | ✅ |
| Functional Tests | §4 | 50 | ≥50 | ✅ |
| Integration Tests | §5 | 50 | ≥50 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **397** | **≥347** | ✅ |

**3:1 Ratio Check:** (N + B) = 140 ≥ 3 × P = 105 → ✅ PASS

---

## Feature Overview

Partner Intelligence provides contextual insights on the partner detail page — engagement history, opportunity pipeline summary, risk indicators, AI-generated recommendations, and time-period filtering. Data is scoped to the user's role and OrgUnit permissions. The section supports manual refresh and displays last-updated timestamps.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Intelligence Section Visible on Partner Detail

**Priority:** P0  
**Precondition:** User authenticated with partner view permission. Partner exists with intelligence data.

**Steps:**
1. Navigate to partner detail page (/partners/{id})
2. Scroll to Intelligence section

**Expected Result:**
- "Partner Intelligence" section visible on detail page
- Section displays engagement history, opportunity pipeline, risk indicators, AI insights
- Data loads within 3 seconds

---

#### POS-002: User Context Personalizes Intelligence

**Priority:** P0  
**Precondition:** User has specific role and OrgUnit. Partner has multi-OrgUnit data.

**Steps:**
1. Log in as user with specific OrgUnit
2. Navigate to partner detail page
3. Observe intelligence data

**Expected Result:**
- Intelligence data scoped to user's role and OrgUnit
- Only interactions/opportunities visible to user's permission scope are shown
- Context indicator shows user's OrgUnit scope

---

#### POS-003: Engagement History Displays Interactions

**Priority:** P0  
**Precondition:** Partner has at least 5 interactions logged.

**Steps:**
1. Navigate to partner detail page
2. Locate Engagement History sub-section

**Expected Result:**
- Interaction list shows type, date, participants, and summary
- Most recent interaction first (chronological descending)
- Engagement metrics calculated (total count, frequency)

---

#### POS-004: Opportunity Pipeline Summary

**Priority:** P0  
**Precondition:** Partner linked to at least 3 opportunities in various stages.

**Steps:**
1. Navigate to partner detail page
2. Locate Opportunity Pipeline sub-section

**Expected Result:**
- Pipeline shows opportunities grouped by stage (Identify & Profile, GO, NO GO)
- Total pipeline value calculated and displayed
- Count per stage shown

---

#### POS-005: Filter Intelligence by Time Period — Last 6 Months

**Priority:** P0  
**Precondition:** Partner has interactions/data spanning 2+ years.

**Steps:**
1. Navigate to partner intelligence section
2. Select "Last 6 Months" from time period filter

**Expected Result:**
- All data filtered to show only last 6 months
- Engagement history, pipeline, risk indicators all reflect filter
- Filter indicator shows "Last 6 Months"

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Filter by Last Year | Data > 1 year | Select "Last Year" | Data from last 12 months | P1 |
| POS-007 | Filter by All Time | Data > 2 years | Select "All Time" | All historical data | P1 |
| POS-008 | Risk indicators display | Partner has risk data | View risk section | Risk categories with severity levels | P1 |
| POS-009 | Expand risk detail | Risk indicators shown | Click expand on risk | Detailed risk description visible | P1 |
| POS-010 | AI-generated recommendations | AI insights available | View AI section | Recommendations with action items | P1 |
| POS-011 | AI insights last-updated timestamp | AI data exists | View AI section | "Last updated: YYYY-MM-DD HH:MM" | P1 |
| POS-012 | Manual refresh intelligence | Intelligence loaded | Click Refresh button | Data reloads, spinner shown, fresh data | P1 |
| POS-013 | Loading indicator during refresh | Click refresh | Observe loading state | Spinner/skeleton visible during load | P1 |
| POS-014 | Engagement metric — total interactions | 10 interactions | View metrics | "Total: 10" | P1 |
| POS-015 | Engagement metric — interaction frequency | Monthly interactions | View metrics | "Average: X per month" | P1 |
| POS-016 | Pipeline value formatting | Total value = $1,234,567 | View pipeline | "$1,234,567" formatted with commas | P1 |
| POS-017 | Pipeline count by stage | 2 in I&P, 1 in GO | View pipeline | Stage counts match | P1 |
| POS-018 | Risk severity color coding | High/Med/Low risks | View risks | Red/Yellow/Green indicators | P1 |
| POS-019 | AI recommendation action click | Recommendation with link | Click action item | Navigates to relevant page | P2 |
| POS-020 | Multiple risk categories | 3+ risk types | View risks | All categories listed | P1 |
| POS-021 | Engagement history with meeting type | Meeting interactions | View engagement | Meeting icon and details | P2 |
| POS-022 | Engagement history with email type | Email interactions | View engagement | Email icon and details | P2 |
| POS-023 | Engagement history with call type | Call interactions | View engagement | Call icon and details | P2 |
| POS-024 | Time period filter affects metrics | Filter to 6 months | View metrics | Metrics recalculated for period | P1 |
| POS-025 | Opportunity pipeline links to detail | Pipeline item shown | Click opportunity in pipeline | Navigate to opportunity detail | P2 |
| POS-026 | Data scoped to user OrgUnit | OrgUnit-scoped user | View intelligence | Only in-scope data shown | P1 |
| POS-027 | Intelligence loads for new partner | New partner, no history | View intelligence | "No data yet" for each section | P1 |
| POS-028 | Interaction dates formatted correctly | Various dates | View engagement | "Jan 15, 2026" format | P2 |
| POS-029 | Risk indicator with trend arrow | Risk trending up | View risk | Upward trend arrow | P2 |
| POS-030 | AI insights with confidence score | AI data includes confidence | View AI | "Confidence: 85%" | P2 |
| POS-031 | Intelligence section collapsed by default | Fresh page load | Load detail page | Section collapsed, expand on click | P2 |
| POS-032 | Intelligence remembers collapse state | User expanded section | Navigate away, return | Section remains expanded | P2 |
| POS-033 | Print intelligence section | Intelligence loaded | Ctrl+P | Section prints cleanly | P2 |
| POS-034 | Export intelligence as PDF | Intelligence loaded | Click Export PDF | PDF with intelligence data | P2 |
| POS-035 | Intelligence accessible via screen reader | Section loaded | Use screen reader | All data points announced | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Invalid time period filter value | "InvalidPeriod" | Default to "All Time" | P1 |
| NEG-002 | Future date in custom filter | Start date = 2030 | Error: "Date cannot be in the future" | P1 |
| NEG-003 | End date before start date | From > To | Error: "End date must be after start date" | P1 |
| NEG-004 | SQL injection in filter parameter | `'; DROP TABLE--` | Sanitized | P0 |
| NEG-005 | XSS in partner intelligence context | `<script>alert(1)</script>` | Escaped | P0 |
| NEG-006 | Invalid partner ID in URL | /partners/abc/intelligence | 400 Bad Request | P1 |
| NEG-007 | Negative partner ID | /partners/-1/intelligence | 400 Bad Request | P1 |
| NEG-008 | Zero partner ID | /partners/0/intelligence | 400 Bad Request | P1 |
| NEG-009 | Extremely large partner ID | /partners/99999999999 | 404 Not Found | P1 |
| NEG-010 | Malformed date in custom filter | "2026-13-45" | Validation error | P1 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | Unauthenticated access | No auth | View intelligence | Redirect to login | P0 |
| NEG-012 | No partner view permission | Restricted role | View partner intelligence | 403 | P0 |
| NEG-013 | OrgUnit-scoped user views out-of-scope partner | Scoped user | Navigate to out-of-scope partner | 403 | P0 |
| NEG-014 | Expired session | Expired JWT | View intelligence | Redirect to login | P0 |
| NEG-015 | Revoked token | Revoked JWT | API call | 401 | P0 |
| NEG-016 | Cross-tenant partner intelligence | Tenant A user | Access Tenant B partner | 403 | P0 |
| NEG-017 | User sees only permitted opportunity data | Limited opportunity access | View pipeline | Only visible opportunities shown | P1 |
| NEG-018 | User without AI feature access | No AI permission | View AI insights | AI section hidden or "Feature unavailable" | P1 |
| NEG-019 | Disabled account | Disabled user | View intelligence | 403 | P1 |
| NEG-020 | API access after logout | Logged out | Intelligence API call | 401 | P1 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | View intelligence for deleted partner | Partner IsDeleted=true | Navigate via URL | 404 Not Found | P1 |
| NEG-022 | Refresh during active refresh | Already refreshing | Click refresh again | Second click ignored or queued | P1 |
| NEG-023 | Filter change during data load | Data loading | Change filter | Cancels current load, starts new | P1 |
| NEG-024 | Export during refresh | Data refreshing | Click export | Export waits or uses cached data | P2 |
| NEG-025 | Navigate away during AI generation | AI processing | Leave page | Generation cancelled cleanly | P2 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Partner with no interactions | 0 interactions | View engagement | "No interactions recorded" | P1 |
| NEG-027 | Partner with no opportunities | 0 opportunities | View pipeline | "No opportunities linked" | P1 |
| NEG-028 | Partner with no risk data | No risks | View risks | "No risks identified" | P1 |
| NEG-029 | Partner with no AI insights | No AI data | View AI | "AI insights not yet generated" | P1 |
| NEG-030 | All intelligence sections empty | New partner | View intelligence | Appropriate empty states for each section | P1 |
| NEG-031 | Interaction with null description | Description = null | View engagement | Row displays without description | P2 |
| NEG-032 | Opportunity with null value | Value = null | View pipeline | "$0" or "N/A" displayed | P2 |
| NEG-033 | Risk with null severity | Severity = null | View risks | Default severity indicator | P2 |
| NEG-034 | AI insight with null recommendation | Text = null | View AI | Skipped or placeholder | P2 |
| NEG-035 | Engagement metric with null dates | Some dates null | Calculate frequency | Null dates excluded from calculation | P2 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Intelligence API timeout | > 30s response | Loading spinner → timeout message | P1 |
| NEG-037 | Intelligence API 500 | Server error | "Unable to load intelligence. Try again." | P1 |
| NEG-038 | AI service unavailable | AI endpoint down | AI section: "Service temporarily unavailable" | P1 |
| NEG-039 | Network disconnection | Network drops | Error with retry option | P1 |
| NEG-040 | Partial section failure | Engagement API fails, others succeed | Failed section shows error, others display | P1 |
| NEG-041 | Opportunity API failure | Pipeline endpoint 500 | Pipeline: "Unable to load" | P2 |
| NEG-042 | Risk calculation service failure | Risk engine down | "Risk data unavailable" | P2 |
| NEG-043 | Database connection lost | DB drops | Cached data if available, error otherwise | P2 |
| NEG-044 | Export service failure | Export endpoint fails | "Export unavailable" | P2 |
| NEG-045 | Metrics calculation failure | Aggregation fails | Show raw data without metrics | P2 |

### 2.6 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-046 | Double-click refresh | Rapid double-click | Single refresh, no duplicate calls | P1 |
| NEG-047 | Filter then immediate refresh | Change filter + refresh | Filter applied, then fresh data for filter | P2 |
| NEG-048 | Intelligence for partner being edited | Another user editing partner | Intelligence shows current data | P2 |
| NEG-049 | Browser back from intelligence link | Click pipeline item, press back | Returns to partner detail | P2 |
| NEG-050 | Page refresh during intelligence load | F5 during load | Clean reload | P2 |
| NEG-051 | Multiple rapid filter changes | Toggle periods quickly | Final period applied | P1 |
| NEG-052 | Intelligence with extremely old data (10+ years) | Very old interactions | Displayed correctly | P2 |
| NEG-053 | Interaction with future date | Date = 2030 | Displayed but flagged | P2 |
| NEG-054 | Pipeline with negative value | Opportunity value = -1000 | Handled, displayed as negative or flagged | P2 |
| NEG-055 | Risk with invalid severity level | Severity = "ULTRA" | Default indicator or ignored | P2 |
| NEG-056 | AI insight with very long text (10,000 chars) | Long recommendation | Truncated with "Read more" | P2 |
| NEG-057 | Intelligence data with HTML in text fields | `<b>Bold</b>` in description | Escaped, not rendered as HTML | P1 |
| NEG-058 | Concurrent partners viewed | Open 5 partner tabs | Each loads independently | P2 |
| NEG-059 | Intelligence for partner with 0 contacts | No contacts | Engagement shows "No contacts linked" | P1 |
| NEG-060 | Print intelligence with no data | All empty sections | Print shows empty state | P2 |
| NEG-061 | Keyboard shortcut during filter | Tab/Enter during filter | Correct behavior, no crash | P2 |
| NEG-062 | Intelligence API returns empty array | [] for all sections | Empty state messages | P1 |
| NEG-063 | AI timeout during generation | AI takes > 60s | "AI generation timed out" | P1 |
| NEG-064 | Filter param manipulation in URL | ?period=MALICIOUS | Sanitized, default applied | P1 |
| NEG-065 | Intelligence section with JavaScript disabled | JS off | Graceful degradation | P2 |
| NEG-066 | Export intelligence as CSV | Click export CSV | Valid CSV file | P2 |
| NEG-067 | Intelligence with mixed time zones | Data from UTC and local | All normalized to display timezone | P2 |
| NEG-068 | Refresh button during error state | Error shown, click refresh | Retry attempt | P1 |
| NEG-069 | Intelligence for archived partner | Partner status = Archived | Intelligence still viewable (read-only) | P1 |
| NEG-070 | Intelligence with 10,000+ interactions | Huge dataset | Paginated or summarized | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** Max(50, 2×35)=70 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Interaction description | 0 | 4000 | ✅ Empty | ✅ Full text | Truncated | P1 |
| BND-002 | AI recommendation text | 1 | 10000 | ✅ Short | ✅ Long text | Truncated | P1 |
| BND-003 | Risk description | 0 | 2000 | ✅ No desc | ✅ Full | Truncated | P2 |
| BND-004 | Opportunity name in pipeline | 1 | 200 | ✅ Short | ✅ Full | Ellipsis | P2 |
| BND-005 | Time period label | 3 | 50 | ✅ "All" | ✅ "Custom: Jan 1 - Dec 31, 2025" | Truncated | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-006 | Interaction count | 0 | 100000 | ✅ Empty state | ✅ Paginated | N/A | P1 |
| BND-007 | Opportunity count in pipeline | 0 | 10000 | ✅ Empty state | ✅ Summarized | N/A | P1 |
| BND-008 | Pipeline total value | 0 | 999999999 | "$0" | Flagged | Formatted | P1 |
| BND-009 | Risk count | 0 | 100 | ✅ "No risks" | ✅ Listed | Paginated | P1 |
| BND-010 | AI insight count | 0 | 50 | ✅ "No insights" | ✅ Listed | N/A | P1 |
| BND-011 | AI confidence score | 0% | 100% | ✅ "0%" | ✅ "100%" | Capped at 100% | P2 |
| BND-012 | Engagement frequency (per month) | 0 | 999 | "0/month" | "999/month" | Display issue | P2 |
| BND-013 | Time period in months | 1 | 120 | "Last 1 month" | "Last 10 years" | Capped | P2 |
| BND-014 | Opportunity value decimal places | 0 | 2 | "$1,000" | "$1,000.99" | Rounded | P2 |
| BND-015 | Risk severity level | 1 | 5 | ✅ Low | ✅ Critical | Capped | P2 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-016 | Interaction on leap year | Feb 29, 2028 | Correctly displayed | P2 |
| BND-017 | Very old interaction (2000-01-01) | Ancient date | Formatted correctly | P2 |
| BND-018 | Today's interaction | Today's date | Shows "Today" or today's date | P2 |
| BND-019 | Filter from Jan 1 to Dec 31 same year | Full year | Correct range | P2 |
| BND-020 | Filter at midnight UTC boundary | 00:00:00 UTC | No off-by-one error | P2 |
| BND-021 | Last 6 months from Jan 15 | Jul 16 – Jan 15 | Correct range handling | P2 |
| BND-022 | Last Year from March 1 | Mar 1 prev year – Mar 1 | Correct calculation | P2 |
| BND-023 | Custom filter: single day | Jan 1 – Jan 1 | Shows data for that day only | P2 |
| BND-024 | Interaction at end of month | Jan 31 | Correctly included in filter | P2 |
| BND-025 | Year boundary (Dec 31 – Jan 1) | Cross-year filter | Both years included | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-026 | 0 interactions, 0 opportunities | Empty partner | All sections show empty state | P1 |
| BND-027 | 1 interaction only | Single interaction | Displayed without frequency calc | P1 |
| BND-028 | 1 opportunity only | Single opportunity | Pipeline shows 1 item | P1 |
| BND-029 | 100 interactions | Medium dataset | Loaded within 2 seconds | P1 |
| BND-030 | 1000 interactions | Large dataset | Paginated, < 3 seconds | P1 |
| BND-031 | 10,000 interactions | Very large | Summarized metrics, paginated history | P1 |
| BND-032 | 50 opportunities in pipeline | Medium pipeline | All stages represented | P1 |
| BND-033 | 500 opportunities | Large pipeline | Summarized by stage | P1 |
| BND-034 | 1 risk indicator | Single risk | Displayed without category grouping | P2 |
| BND-035 | 50 risk indicators | Many risks | Grouped by category, scrollable | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-036 | Interaction description (Arabic) | `اجتماع مع الشريك` | RTL text displayed correctly | P2 |
| BND-037 | AI recommendation (Chinese) | `建议加强合作` | Correctly rendered | P2 |
| BND-038 | Risk description (French) | `Risque de défaut` | Accents displayed correctly | P2 |
| BND-039 | Opportunity name (Cyrillic) | `Проект развития` | Correctly displayed | P2 |
| BND-040 | Interaction with emoji | `🤝 Partnership meeting` | Emoji renders | P2 |
| BND-041 | AI text with HTML entities | `Revenue &gt; $1M` | Rendered as "Revenue > $1M" | P2 |
| BND-042 | Risk with special characters | `Risk: 50% chance (±5%)` | Special chars display | P2 |
| BND-043 | Pipeline with currency symbols | `€1,000,000` | Euro symbol displays | P2 |
| BND-044 | Interaction participant with long Unicode name | 100-char Arabic name | Truncated with ellipsis | P2 |
| BND-045 | AI insight with mixed scripts | English + Arabic + Chinese | All scripts render correctly | P2 |

### 3.6 Value & Precision Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-046 | Pipeline value = $0 | All opportunities $0 | Total: "$0" | P1 |
| BND-047 | Pipeline value = $999,999,999 | Very large total | Formatted with commas | P1 |
| BND-048 | Pipeline with mixed currencies | USD + EUR + GBP | Converted or listed separately | P2 |
| BND-049 | Engagement frequency = 0/month | No interactions in period | "0 interactions per month" | P1 |
| BND-050 | Engagement frequency = 100/month | Very active | "100 per month" | P2 |
| BND-051 | AI confidence = 0% | Very uncertain | Displayed with warning indicator | P2 |
| BND-052 | AI confidence = 100% | Fully confident | Displayed with high-confidence badge | P2 |
| BND-053 | Risk trending up for 12 months | Consistent uptrend | Strong upward trend arrow | P2 |
| BND-054 | Risk trending down | Improving risk | Downward trend arrow (positive) | P2 |
| BND-055 | Risk flat (no change) | Stable risk | Flat trend indicator | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-056 | Intelligence for partner with 1 contact | Minimal data | Basic intelligence shown | P1 |
| BND-057 | Intelligence for partner with 100 contacts | Many contacts | Summarized engagement | P1 |
| BND-058 | Filter period exactly 1 day | Custom: today only | Only today's data | P2 |
| BND-059 | Filter period exactly 1 year | Custom: full year | Full year data | P2 |
| BND-060 | Refresh immediately after page load | Refresh < 1s after load | No duplicate load | P1 |
| BND-061 | Intelligence for partner created today | Brand new partner | All sections empty | P1 |
| BND-062 | Intelligence for partner active 10+ years | Long history | Full historical data available | P2 |
| BND-063 | Pipeline with opportunity at $0.01 | Minimum value | Displayed as "$0.01" | P2 |
| BND-064 | Interaction lasting 0 minutes | Zero-duration meeting | Displayed without duration | P2 |
| BND-065 | Interaction lasting 24+ hours | Multi-day event | Duration displayed correctly | P2 |
| BND-066 | Risk with exactly 1 data point | Single observation | Displayed without trend | P2 |
| BND-067 | AI insight generated exactly now | Timestamp = now | "Last updated: Just now" | P2 |
| BND-068 | AI insight from 365 days ago | Old AI data | Shows age warning | P2 |
| BND-069 | Intelligence section at minimum viewport | 320px width | Responsive, no overflow | P2 |
| BND-070 | Intelligence section at 4K viewport | 3840×2160 | Uses space effectively | P2 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow rules (15), Validation rules (15), Constraint rules (10), Audit rules (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule Description | Trigger | Expected Outcome | Priority |
|----|-----------|-----------------|---------|-----------------|----------|
| FUN-001 | Intelligence data scoped to user permissions | Permission-based filtering | Load intelligence | Only permitted data shown | P0 |
| FUN-002 | OrgUnit scope applies to all sections | OrgUnit filter | Load as scoped user | All sections scoped to OrgUnit | P0 |
| FUN-003 | Soft-deleted interactions excluded | IsDeleted filter | Load engagement | Deleted interactions not shown | P0 |
| FUN-004 | Soft-deleted opportunities excluded | IsDeleted filter | Load pipeline | Deleted opportunities not shown | P0 |
| FUN-005 | Time filter applies to engagement | Date range | Select "Last 6 Months" | Only 6-month interactions shown | P0 |
| FUN-006 | Time filter applies to pipeline | Date range | Select "Last 6 Months" | Only 6-month opportunities shown | P1 |
| FUN-007 | Time filter applies to risk | Date range | Select filter | Risk data scoped to period | P1 |
| FUN-008 | Refresh reloads all sections | Manual refresh | Click Refresh | All sections reload with fresh data | P1 |
| FUN-009 | Metrics recalculate on filter change | Filter trigger | Change time period | Totals, averages, counts update | P1 |
| FUN-010 | AI insights independent of time filter | AI is cumulative | Change filter | AI insights remain (not date-scoped) | P1 |
| FUN-011 | Engagement sorted by date descending | Sort order | Load engagement | Most recent first | P1 |
| FUN-012 | Pipeline grouped by stage | Stage grouping | Load pipeline | Opportunities grouped by stage | P1 |
| FUN-013 | Risk categories grouped | Category grouping | Load risks | Risks grouped by category | P1 |
| FUN-014 | New interaction appears after refresh | Data change | Add interaction, refresh | New interaction visible | P1 |
| FUN-015 | Deleted interaction disappears after refresh | Soft-delete | Delete interaction, refresh | Interaction removed | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Validation Rule | Valid Input | Invalid Input | Priority |
|----|-----------|----------------|------------|--------------|----------|
| FUN-016 | Time period must be valid enum | Valid periods | "Last 6 Months" | "InvalidPeriod" | P1 |
| FUN-017 | Custom date range: start ≤ end | Date order | Jan 1 – Dec 31 | Dec 31 – Jan 1 | P1 |
| FUN-018 | Custom date range: not future | Date validity | Today or past | Tomorrow | P1 |
| FUN-019 | Partner ID must be positive | ID validation | 42 | -1, "abc" | P1 |
| FUN-020 | Partner must not be deleted | Status check | Active partner | Deleted partner | P1 |
| FUN-021 | Input sanitized for XSS | XSS prevention | Clean text | `<script>` | P0 |
| FUN-022 | API content-type validation | Response type | application/json | text/html | P1 |
| FUN-023 | Metrics handle null values | Null safety | Mix of null/valid | All null | P1 |
| FUN-024 | Pipeline value must be non-negative | Value check | 0, 1000 | -500 | P1 |
| FUN-025 | Risk severity must be valid level | Enum check | High/Med/Low | "ULTRA" | P1 |
| FUN-026 | AI confidence 0-100% | Range check | 50% | -5%, 105% | P1 |
| FUN-027 | Interaction date not in future | Date check | Past/today | Tomorrow | P2 |
| FUN-028 | Engagement frequency calculation handles divide-by-zero | Period = 0 months | 0 → "N/A" | N/A | P1 |
| FUN-029 | Currency formatting respects locale | Locale setting | US: $1,000 | EU: €1.000 | P2 |
| FUN-030 | Sort parameter validation | Valid fields | "date", "type" | "DROP_TABLE" | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max engagement history displayed | 100 per page | 200 interactions | Paginated, 100 per page | P1 |
| FUN-032 | Max pipeline items displayed | 50 per stage | 100 per stage | Summarized or paginated | P1 |
| FUN-033 | Max risk indicators | 50 | 60 risks | Paginated | P2 |
| FUN-034 | Max AI recommendations | 20 | 25 insights | Top 20 shown, "View all" link | P2 |
| FUN-035 | API rate limit on refresh | 1 per 5 seconds | 3 refreshes in 5s | Rate limited | P1 |
| FUN-036 | Session timeout during intelligence view | 30 min idle | 31 min | Session expired | P1 |
| FUN-037 | Max custom date range | 10 years | 15 years | Capped at 10 or warning | P2 |
| FUN-038 | Intelligence API response size | 5MB max | Large partner | Paginated response | P2 |
| FUN-039 | Concurrent intelligence loads | 5 partners simultaneously | Load 6 | 6th queued | P2 |
| FUN-040 | Export size limit | 10MB | Large intelligence export | Chunked or limited | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Intelligence view logged | Page load | User ID, partner ID, timestamp | P1 |
| FUN-042 | Time filter change logged | Filter change | User ID, old period, new period | P2 |
| FUN-043 | Manual refresh logged | Refresh click | User ID, partner ID, timestamp | P2 |
| FUN-044 | Export action logged | Export intelligence | User ID, export format, timestamp | P1 |
| FUN-045 | AI insight view logged | View AI section | User ID, partner ID | P2 |
| FUN-046 | Failed access logged | Unauthorized view | User ID, partner ID, denied reason | P0 |
| FUN-047 | Error event logged | API error | Error code, context | P1 |
| FUN-048 | Pipeline click-through logged | Click opportunity | User ID, opportunity ID | P2 |
| FUN-049 | Risk detail expansion logged | Expand risk | User ID, risk category | P2 |
| FUN-050 | Intelligence data refresh duration logged | Refresh complete | Duration, data size | P2 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests | **Breakdown:** CRUD workflow (10), Search/filter (10), Pagination (5), Relationships (10), Error handling (15)

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|------------------|-----------------|----------|
| INT-001 | Add interaction → engagement updates | Create | Interaction, Intelligence | New interaction in engagement history | P0 |
| INT-002 | Delete interaction → engagement updates | Soft-delete | Interaction, Intelligence | Interaction removed after refresh | P0 |
| INT-003 | Link opportunity → pipeline updates | Create link | Opportunity, Intelligence | Opportunity appears in pipeline | P0 |
| INT-004 | Unlink opportunity → pipeline updates | Remove link | Opportunity, Intelligence | Opportunity removed from pipeline | P0 |
| INT-005 | Update risk data → indicators refresh | Update | Risk, Intelligence | Risk indicators reflect changes | P1 |
| INT-006 | Generate new AI insights → section updates | Generate | AI, Intelligence | New recommendations displayed | P1 |
| INT-007 | Partner status change → intelligence accessibility | Update | Partner status | Intelligence viewable if partner active/archived | P1 |
| INT-008 | Partner OrgUnit change → data scope changes | Update | OrgUnit | Intelligence rescoped to new OrgUnit | P1 |
| INT-009 | Contact added to partner → engagement scope expands | Create | Contact, Intelligence | Contact's interactions included | P1 |
| INT-010 | Contact removed → engagement scope narrows | Delete contact link | Contact, Intelligence | Contact's interactions excluded | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Search/Filter Criteria | Expected Results | Priority |
|----|-----------|----------------------|-----------------|----------|
| INT-011 | Filter "Last 6 Months" + verify metrics | 6-month filter | Metrics match 6-month data | P0 |
| INT-012 | Filter "Last Year" + verify pipeline | 12-month filter | Pipeline shows 12-month opportunities | P1 |
| INT-013 | Filter "All Time" shows complete data | All time | All historical data visible | P1 |
| INT-014 | Custom filter with specific date range | Custom dates | Exact range applied | P1 |
| INT-015 | Filter change recalculates engagement frequency | Period change | Frequency = count / months | P1 |
| INT-016 | Filter change updates pipeline value | Period change | Total recalculated for period | P1 |
| INT-017 | Filter preserves on section expand/collapse | Toggle section | Filter remains applied | P1 |
| INT-018 | Filter resets on partner navigation | Navigate to new partner | Filter resets to default | P2 |
| INT-019 | Filter persists within same partner | Change filter, scroll | Filter stays applied | P1 |
| INT-020 | Clear custom filter returns to default | Clear custom dates | "All Time" or default applied | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| INT-021 | Engagement history page 1 | First 20 interactions | Most recent 20 shown | P1 |
| INT-022 | Load more engagement history | Page 2 | Next 20 interactions loaded | P1 |
| INT-023 | Pipeline pagination by stage | Large pipeline | Stages paginated independently | P2 |
| INT-024 | Risk indicators pagination | 50+ risks | Paginated by category | P2 |
| INT-025 | AI recommendations show all | < 20 insights | All displayed, no pagination needed | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Test Scenario | Expected Result | Priority |
|----|-----------|-------------|--------------|-----------------|----------|
| INT-026 | Partner → Interactions → Intelligence | Interaction link | Interactions appear in engagement | P0 |
| INT-027 | Partner → Opportunities → Pipeline | Opportunity link | Opportunities appear in pipeline | P0 |
| INT-028 | Partner → Contacts → Interaction scope | Contact link | Contact interactions included | P1 |
| INT-029 | Partner type affects risk profile | Type relationship | Funding partners have financial risks | P1 |
| INT-030 | OrgUnit → User scope → Data visibility | Permission chain | User sees only permitted data | P1 |
| INT-031 | Interaction type → engagement categorization | Type grouping | Meetings grouped separately from emails | P2 |
| INT-032 | Opportunity stage → pipeline categorization | Stage mapping | Correct stage buckets | P1 |
| INT-033 | AI model → recommendation generation | AI relationship | Recommendations based on partner data | P2 |
| INT-034 | Risk data source → indicator calculation | Risk engine | Indicators calculated from source data | P2 |
| INT-035 | Time filter → all data relationships | Cross-section filter | All sections honor same filter | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error Condition | Expected Response | Priority |
|----|-----------|----------------|------------------|----------|
| INT-036 | Partner API 404 | Deleted partner | 404 page | P0 |
| INT-037 | Intelligence API 500 | Server error | Error with retry | P0 |
| INT-038 | Intelligence API 403 | No permission | Access denied | P0 |
| INT-039 | Engagement section 500 | Partial failure | Other sections load, engagement shows error | P1 |
| INT-040 | Pipeline section 500 | Partial failure | Other sections load, pipeline shows error | P1 |
| INT-041 | AI section timeout | Slow AI | "AI insights loading..." then timeout message | P1 |
| INT-042 | Risk section failure | Risk engine down | Other sections load, risk shows unavailable | P1 |
| INT-043 | Network timeout on filter change | Slow network | Timeout message, retry option | P1 |
| INT-044 | JWT refresh during load | Token refresh | Seamless reload | P1 |
| INT-045 | Malformed API response | Invalid JSON | Error handled, no crash | P1 |
| INT-046 | API rate limit on refresh | Too many refreshes | 429, "Please wait" message | P1 |
| INT-047 | CORS error | Misconfigured | Error in console, user message | P2 |
| INT-048 | Export fails | Export endpoint down | "Export unavailable" toast | P2 |
| INT-049 | Session expired during export | Token expired | Auth prompt | P1 |
| INT-050 | Concurrent section failures | 2+ sections fail | Each shows independent error | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests | **Coverage:** OWASP Top 10, injection, authorization, IDOR, mass assignment

### 6.1 Injection Prevention (10)

| ID | Test Name | Attack Vector | Target Field | Expected Block | Priority |
|----|-----------|--------------|-------------|---------------|----------|
| SEC-001 | SQL injection in filter | `'; DROP TABLE--` | Date filter API | Sanitized | P0 |
| SEC-002 | SQL injection in partner ID | `1 OR 1=1` | URL parameter | Parameterized query | P0 |
| SEC-003 | XSS via intelligence data | `<script>alert(1)</script>` | Display fields | Escaped | P0 |
| SEC-004 | XSS in interaction description | Script in description | Engagement history | Escaped in DOM | P0 |
| SEC-005 | XSS in AI recommendation | Script in AI text | AI section | Escaped | P0 |
| SEC-006 | Path traversal | `../../etc/passwd` | Export API | Rejected | P1 |
| SEC-007 | HTML injection in risk text | `<img onerror=...>` | Risk display | Escaped | P1 |
| SEC-008 | JSON injection | `{"$ne":null}` | Filter API body | Rejected | P1 |
| SEC-009 | LDAP injection | `*)(cn=*` | Search/filter | Sanitized | P1 |
| SEC-010 | Template injection | `{{constructor}}` | Display template | Escaped | P1 |

### 6.2 Broken Access Control (10)

| ID | Test Name | User Role | Unauthorized Action | Expected Result | Priority |
|----|-----------|-----------|-------------------|-----------------|----------|
| SEC-011 | Anonymous access | No auth | GET /api/partners/{id}/intelligence | 401 | P0 |
| SEC-012 | No partner view permission | Restricted | Intelligence API | 403 | P0 |
| SEC-013 | OrgUnit-scoped user | Scoped | Out-of-scope partner intelligence | 403 | P0 |
| SEC-014 | Expired token | Expired JWT | Intelligence API | 401 | P0 |
| SEC-015 | Tampered JWT | Modified token | API call | 401/403 | P0 |
| SEC-016 | Vertical escalation | Basic user | Admin intelligence features | 403 | P0 |
| SEC-017 | Horizontal access | User A | User B's scoped partner data | 403 | P0 |
| SEC-018 | Disabled account | Disabled | Intelligence API | 403 | P1 |
| SEC-019 | Post-logout access | Logged out | Cached API call | 401 | P1 |
| SEC-020 | Role escalation via parameter | Basic | ?role=admin | Ignored | P0 |

### 6.3 IDOR (10)

| ID | Test Name | Object | Manipulation | Expected Result | Priority |
|----|-----------|--------|-------------|-----------------|----------|
| SEC-021 | Access other partner intelligence | Partner ID | Change URL to other partner | 403 if not in scope | P0 |
| SEC-022 | Enumerate partner intelligence | Sequential IDs | /partners/1/intelligence, /2/... | Rate limited, scoped | P0 |
| SEC-023 | Access deleted partner intelligence | Deleted ID | URL with deleted partner | 404 | P1 |
| SEC-024 | Access other OrgUnit's intelligence | OrgUnit scope | Change scope parameter | 403 | P0 |
| SEC-025 | View interactions of other partner | Interaction data | Manipulate API params | Only own partner's data | P0 |
| SEC-026 | Negative partner ID | -1 | /partners/-1/intelligence | 400 | P1 |
| SEC-027 | Zero partner ID | 0 | /partners/0/intelligence | 400 | P1 |
| SEC-028 | Float partner ID | 1.5 | API call | 400 | P1 |
| SEC-029 | String partner ID | "abc" | API call | 400 | P1 |
| SEC-030 | Access AI insights for restricted partner | Restricted | AI API with other partner | 403 | P1 |

### 6.4 Mass Assignment (5)

| ID | Test Name | Protected Field | Manipulation | Expected Result | Priority |
|----|-----------|----------------|-------------|-----------------|----------|
| SEC-031 | Modify intelligence cache | Cache data | Tamper with cached response | Validated against server | P1 |
| SEC-032 | Modify risk severity | Risk data | Include in request body | Not modifiable via intelligence API | P0 |
| SEC-033 | Modify pipeline value | Opportunity value | Include in request | Not modifiable | P0 |
| SEC-034 | Modify AI recommendation | AI text | Include in request | Not modifiable | P1 |
| SEC-035 | Modify engagement metrics | Calculated metrics | Include in request | Recalculated server-side | P1 |

### 6.5 Authentication & Session (10)

| ID | Test Name | Attack Scenario | Expected Protection | Priority |
|----|-----------|----------------|-------------------|----------|
| SEC-036 | Brute-force partner intelligence | Repeated attempts | Rate limiting | P0 |
| SEC-037 | Session fixation | Pre-set session | New session on login | P0 |
| SEC-038 | Session hijacking | Stolen JWT | Token bound to context | P1 |
| SEC-039 | CSRF on intelligence export | Forged request | CSRF token | P0 |
| SEC-040 | Clickjacking | Iframe embedding | X-Frame-Options: DENY | P1 |
| SEC-041 | Token storage | Storage check | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Multiple logins | Policy enforced | P1 |
| SEC-043 | Token refresh | Near expiry | Refresh works | P1 |
| SEC-044 | Logout clears data | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS enforcement | HTTP attempt | Redirect to HTTPS | P0 |

### 6.6 Data Exposure (5)

| ID | Test Name | Sensitive Data | Exposure Risk | Expected Protection | Priority |
|----|-----------|---------------|--------------|-------------------|----------|
| SEC-046 | Internal fields excluded | Audit, internal IDs | Over-exposure | DTO filtering | P1 |
| SEC-047 | No stack traces | Exception details | Info disclosure | Generic messages | P0 |
| SEC-048 | Sensitive partner data not in intelligence | Financial details | Leakage | Filtered response | P1 |
| SEC-049 | No response caching | Intelligence API | Cache extraction | Cache-Control: no-store | P1 |
| SEC-050 | Auth tokens not in URL | JWT | URL leakage | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests | **Coverage:** Race conditions, deadlocks, double submit, transaction isolation, cache poisoning

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users view same partner intelligence | Concurrent read | Both see consistent data | P1 |
| CON-002 | Interaction added during intelligence load | Create during read | Either shows or refreshable | P1 |
| CON-003 | Partner deleted during intelligence view | Delete during view | Error on refresh, handled gracefully | P1 |
| CON-004 | Two users refresh simultaneously | Concurrent refresh | Both get fresh data | P1 |
| CON-005 | Filter change during load | Cancel + new request | Latest filter applied | P1 |
| CON-006 | AI generation during refresh | AI processing | AI section shows "Generating..." | P2 |
| CON-007 | Double-click refresh | Rapid clicks | Single refresh executed | P1 |
| CON-008 | Export during data update | Export + update | Export uses consistent snapshot | P2 |
| CON-009 | Multiple tab intelligence views | Same partner, 2 tabs | Independent loads, consistent data | P2 |
| CON-010 | Cache invalidation during read | Cache expires | Fresh data fetched | P1 |
| CON-011 | Opportunity status change during pipeline view | Stage change | Pipeline updates on refresh | P1 |
| CON-012 | Risk recalculation during view | Risk engine running | Risk data eventually updates | P2 |
| CON-013 | Token refresh during intelligence load | Token expires | Retry with new token | P1 |
| CON-014 | Database lock during aggregation | DB contention | Retry, eventual success | P1 |
| CON-015 | WebSocket disconnect during view | Connection drops | Reconnect, poll for updates | P2 |
| CON-016 | Multiple API calls from filter change | Rapid filter changes | Only latest response used | P1 |
| CON-017 | Intelligence cache poisoning | Modified cache | Validated against server | P1 |
| CON-018 | Concurrent section loads | All 4 sections loading | All complete independently | P1 |
| CON-019 | Partner type change during intelligence view | Partner update | Intelligence rescoped on refresh | P2 |
| CON-020 | OrgUnit change during view | OrgUnit reassignment | Data scope changes on refresh | P1 |
| CON-021 | Bulk interaction import during view | Many new interactions | Metrics update on refresh | P2 |
| CON-022 | Concurrent AI requests | Two users request AI for same partner | Single generation, both see result | P2 |
| CON-023 | Database migration during intelligence | Schema change | Graceful degradation | P2 |
| CON-024 | Session timeout during export | Token expires | Auth prompt | P1 |
| CON-025 | Optimistic concurrency on partner update | Stale version | Conflict detected | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests | **Breakdown:** Validation (5), Formatting (3), Calculations (5), Status logic (5), Collections (3)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Validate time period enum | Validation | "InvalidPeriod" | Invalid | P1 |
| UNT-002 | Validate date range order | Validation | Start > End | Invalid | P1 |
| UNT-003 | Validate partner ID positive | Validation | -1 | Invalid | P1 |
| UNT-004 | Validate confidence range | Validation | 105% | Invalid | P1 |
| UNT-005 | Validate risk severity level | Validation | "ULTRA" | Invalid | P1 |
| UNT-006 | Format currency value | Formatting | 1234567 | "$1,234,567" | P1 |
| UNT-007 | Format engagement frequency | Formatting | 24 interactions / 6 months | "4.0/month" | P1 |
| UNT-008 | Format date for display | Formatting | 2026-02-11 | "Feb 11, 2026" | P2 |
| UNT-009 | Calculate pipeline total | Calculations | [100K, 200K, 50K] | 350K | P1 |
| UNT-010 | Calculate engagement frequency | Calculations | 12 interactions / 3 months | 4.0/month | P1 |
| UNT-011 | Calculate frequency with 0 months | Calculations | 5 interactions / 0 months | 0 or N/A | P1 |
| UNT-012 | Calculate risk trend | Calculations | Rising severity over 3 months | "Increasing" | P1 |
| UNT-013 | Calculate pipeline by stage | Calculations | Mixed stages | Correct stage counts | P1 |
| UNT-014 | Determine section visibility | Status | AI disabled | AI section hidden | P1 |
| UNT-015 | Determine empty state | Status | 0 interactions | "No interactions" | P1 |
| UNT-016 | Determine filter active | Status | "Last 6 Months" selected | Filter active indicator | P1 |
| UNT-017 | Determine refresh cooldown | Status | Refreshed < 5s ago | Refresh disabled | P1 |
| UNT-018 | Determine data staleness | Status | Last update > 1 hour | "Data may be stale" | P2 |
| UNT-019 | Group interactions by type | Collections | Mixed interactions | Grouped dictionary | P1 |
| UNT-020 | Group opportunities by stage | Collections | Mixed opportunities | Grouped by stage | P1 |
| UNT-021 | Filter collection by date range | Collections | Date range + interactions | Filtered list | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests | **Breakdown:** Single ops (2), Bulk ops (3), Search (5), Concurrent access (3), Memory (3)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Intelligence load (50 interactions) | Initial load | < 2 seconds | P1 |
| PRF-002 | Intelligence load (500 interactions) | Initial load | < 5 seconds | P1 |
| PRF-003 | Pipeline calculation (100 opportunities) | Aggregate | < 1 second | P2 |
| PRF-004 | Risk calculation (50 indicators) | Calculate | < 1 second | P2 |
| PRF-005 | AI insight retrieval | Fetch | < 3 seconds | P2 |
| PRF-006 | Filter change response (6-month) | Filter | < 500ms | P1 |
| PRF-007 | Filter change response (All Time, large data) | Filter | < 2 seconds | P1 |
| PRF-008 | Refresh all sections | Manual refresh | < 3 seconds | P1 |
| PRF-009 | Engagement pagination (page 2) | Load more | < 500ms | P1 |
| PRF-010 | Export intelligence PDF | Export | < 5 seconds | P2 |
| PRF-011 | 10 concurrent partner intelligence loads | Concurrent | < 5s per user | P2 |
| PRF-012 | 50 concurrent intelligence loads | Concurrent | < 10s per user | P2 |
| PRF-013 | 20 concurrent AI requests | Concurrent AI | < 10s per request | P2 |
| PRF-014 | Memory with 1000 interactions | Memory | < 200MB heap | P2 |
| PRF-015 | Memory with 5000 interactions | Memory | < 500MB heap | P2 |
| PRF-016 | Memory leak (30 min browsing) | Memory | No growth > 10% | P1 |

---

## §10 Load Tests

> **Minimum:** 10 tests | **Breakdown:** Sustained load (3), Spike load (2), Stress limits (3), Recovery (2)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | 50 users viewing intelligence | Sustained | 30 min | 95% < 3s, 0 errors | P2 |
| LDT-002 | 100 users viewing intelligence | Sustained | 30 min | 95% < 5s, < 1% errors | P2 |
| LDT-003 | 50 users with active filtering | Sustained filter | 15 min | Filter < 1s | P2 |
| LDT-004 | Spike 10 to 200 users | Sudden spike | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike with concurrent AI requests | 50 users + 20 AI requests | 5 min | All AI complete, no crash | P2 |
| LDT-006 | 500 concurrent users | Stress | 10 min | Graceful degradation | P2 |
| LDT-007 | Partner with 10,000 interactions | Large data, 50 users | 15 min | Paginated, loads complete | P2 |
| LDT-008 | Continuous refresh (100 users) | Sustained refresh | 10 min | API handles load | P2 |
| LDT-009 | Recovery after API crash | Kill + restart | N/A | Recovers < 60s | P2 |
| LDT-010 | Recovery after DB restart | DB restart | N/A | Reconnects and loads | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Intelligence section visible on partner detail | POS-001, NEG-030, INT-001 |
| AC-2: User context personalizes data | POS-002, FUN-001, FUN-002, SEC-013, SEC-017 |
| AC-3: Engagement history with metrics | POS-003, POS-014, POS-015, INT-001, PRF-001 |
| AC-4: Opportunity pipeline summary | POS-004, POS-016, POS-017, INT-003, INT-027 |
| AC-5: Time period filtering | POS-005, POS-006, POS-007, FUN-005–007, INT-011–020 |
| AC-6: Risk indicators | POS-008, POS-009, POS-018, POS-020, INT-005 |
| AC-7: AI-generated recommendations | POS-010, POS-011, POS-019, INT-006, NEG-029 |
| AC-8: Manual refresh | POS-012, POS-013, FUN-008, NEG-046, CON-007 |
| AC-9: Security and scoping | SEC-001–050, NEG-011–020 |
| AC-10: Performance | PRF-001–016, LDT-001–010 |

---

## Test Environment Setup

**Prerequisites:**
- Authenticated user with Partner View permissions
- Partner with at least 20 interactions, 5 opportunities, risk data, and AI insights
- AI insight generation service available (or mocked)
- Multiple time periods of data (6 months, 1 year, 2+ years)
- OrgUnit-scoped test user for permission testing

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
