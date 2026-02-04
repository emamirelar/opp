# Partner Intelligence - User Context Test Cases

## Overview
Test cases for the Partner Intelligence feature and user context functionality in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-108  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 10

---

## Test Cases

### POS_001 - Validate Partner Intelligence Section Visibility
**Priority:** High  
**Labels:** Partners, Intelligence, UI

**Objective:** Validate that Partner Intelligence section is visible on partner details page.

**Preconditions:**
- User is logged in with Partner User role
- Partner record exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to a Partner detail page | Partner page loads |
| 2 | Locate "Partner Intelligence" section | Section/tab is visible |
| 3 | Click to expand or navigate to section | Intelligence content displays |

---

### POS_002 - Display User Context Information
**Priority:** High  
**Labels:** Partners, Intelligence, Context

**Objective:** Validate that user context is displayed based on current user's role and org unit.

**Preconditions:**
- User is logged in
- Partner has relevant data

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Intelligence section | Section loads |
| 2 | Verify context indicators | User's org unit relationship to partner shown |
| 3 | Verify relevant insights displayed | Insights personalized to user's context |

---

### POS_003 - View Partner Engagement History
**Priority:** Normal  
**Labels:** Partners, Intelligence, History

**Objective:** Validate that engagement history is displayed in intelligence section.

**Preconditions:**
- Partner has interaction history

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Intelligence section | Section loads |
| 2 | Locate engagement history panel | History panel visible |
| 3 | Verify historical interactions listed | Interactions displayed with dates |
| 4 | Verify engagement metrics | Count, frequency, or timeline shown |

---

### POS_004 - View Partner Opportunity Pipeline
**Priority:** High  
**Labels:** Partners, Intelligence, Opportunities

**Objective:** Validate that partner's opportunity pipeline is displayed.

**Preconditions:**
- Partner has associated opportunities

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Intelligence section | Section loads |
| 2 | Locate opportunity pipeline widget | Pipeline visualization visible |
| 3 | Verify opportunities by stage | Opportunities grouped by stage |
| 4 | Verify total value displayed | Total pipeline value shown |

---

### POS_005 - Filter Intelligence by Time Period
**Priority:** Normal  
**Labels:** Partners, Intelligence, Filter

**Objective:** Validate filtering intelligence data by time period.

**Preconditions:**
- Partner Intelligence section is open
- Historical data exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate time period filter | Filter dropdown visible |
| 2 | Select "Last 6 Months" | Data filters to 6 months |
| 3 | Verify data reflects filter | Only recent 6 months data shown |
| 4 | Select "Last Year" | Data expands to year |

---

### POS_006 - View Partner Risk Indicators
**Priority:** Normal  
**Labels:** Partners, Intelligence, Risk

**Objective:** Validate that risk indicators are displayed in intelligence section.

**Preconditions:**
- Partner has risk-relevant data

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Intelligence section | Section loads |
| 2 | Locate risk indicators | Risk panel or badges visible |
| 3 | Verify risk categories shown | Categories like financial, compliance displayed |
| 4 | Click on risk indicator | Risk details expand |

---

### POS_007 - View AI-Generated Insights
**Priority:** High  
**Labels:** Partners, Intelligence, AI

**Objective:** Validate that AI-generated insights are displayed.

**Preconditions:**
- AI analysis has been run on partner

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partner Intelligence section | Section loads |
| 2 | Locate AI Insights panel | Insights displayed |
| 3 | Verify insight content | AI-generated recommendations visible |
| 4 | Verify insight timestamp | Last updated date shown |

---

### NEG_008 - Intelligence Section with No Data
**Priority:** Normal  
**Labels:** Partners, Intelligence, Negative

**Objective:** Validate empty state when partner has no intelligence data.

**Preconditions:**
- Partner is newly created with no history

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to new Partner Intelligence section | Section loads |
| 2 | Verify empty state messaging | "No intelligence data available" message |
| 3 | Verify call to action | Suggestion to add interactions or opportunities |

---

### POS_009 - Refresh Partner Intelligence
**Priority:** Normal  
**Labels:** Partners, Intelligence, Refresh

**Objective:** Validate refreshing intelligence data manually.

**Preconditions:**
- Partner Intelligence section is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate refresh button | Refresh icon visible |
| 2 | Click refresh | Loading indicator appears |
| 3 | Wait for refresh complete | Data refreshes |
| 4 | Verify updated timestamp | Last updated time changes |

---

### POS_010 - Intelligence Respects User Permissions
**Priority:** High  
**Labels:** Partners, Intelligence, Security

**Objective:** Validate that intelligence only shows data user is authorized to see.

**Preconditions:**
- User has limited access (e.g., Org Unit specific)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as user with limited access | Login successful |
| 2 | Navigate to Partner Intelligence | Section loads |
| 3 | Verify data scope | Only authorized data displayed |
| 4 | Verify no unauthorized data exposed | Restricted opportunities/interactions hidden |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 5 |
| Normal | 5 |
| **TOTAL** | **10** |
