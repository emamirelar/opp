# Homepage / Dashboard Test Cases

## Overview
Test cases for the Homepage and Dashboard functionality in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-261  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 16

---

## Test Cases

### POS_001 - Dashboard Loads Successfully
**Priority:** High  
**Labels:** Dashboard, UI, Home

**Objective:** Validate that the dashboard loads successfully after login.

**Preconditions:**
- Valid user credentials

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in with valid credentials | Login successful |
| 2 | Verify dashboard/homepage loads | Dashboard displayed |
| 3 | Verify all widgets load | No loading spinners stuck |
| 4 | Verify page title | Correct page title displayed |

---

### POS_002 - Dashboard Displays Welcome Message
**Priority:** Normal  
**Labels:** Dashboard, UI

**Objective:** Validate personalized welcome message on dashboard.

**Preconditions:**
- User is logged in

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate welcome message | "Welcome, [User Name]" displayed |
| 3 | Verify name is correct | User's actual name shown |

---

### POS_003 - Dashboard Shows Recent Activities
**Priority:** High  
**Labels:** Dashboard, Activities

**Objective:** Validate that recent activities are displayed on dashboard.

**Preconditions:**
- User has recent activity in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate Recent Activities widget | Widget visible |
| 3 | Verify activities listed | Recent actions displayed with timestamps |
| 4 | Click on an activity | Navigates to related record |

---

### POS_004 - Dashboard Opportunity Summary Widget
**Priority:** High  
**Labels:** Dashboard, Opportunities, Widget

**Objective:** Validate opportunity summary widget on dashboard.

**Preconditions:**
- Opportunities exist in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate Opportunity Summary widget | Widget visible |
| 3 | Verify opportunity counts by stage | Counts displayed (Draft, Active, etc.) |
| 4 | Click on a stage | Navigates to filtered opportunity list |

---

### POS_005 - Dashboard Partner Summary Widget
**Priority:** Normal  
**Labels:** Dashboard, Partners, Widget

**Objective:** Validate partner summary widget on dashboard.

**Preconditions:**
- Partners exist in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate Partner Summary widget | Widget visible |
| 3 | Verify partner statistics | Active partners count, by type |
| 4 | Click on widget | Navigates to partners list |

---

### POS_006 - Dashboard My Tasks Widget
**Priority:** High  
**Labels:** Dashboard, Tasks, Widget

**Objective:** Validate My Tasks or Action Items widget.

**Preconditions:**
- User has pending tasks or action items

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate My Tasks widget | Widget visible |
| 3 | Verify pending tasks listed | Tasks with due dates shown |
| 4 | Click on a task | Navigates to related record |

---

### POS_007 - Dashboard Refresh Data
**Priority:** Normal  
**Labels:** Dashboard, Refresh

**Objective:** Validate refreshing dashboard data.

**Preconditions:**
- Dashboard is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Click refresh button or reload page | Data refreshes |
| 3 | Verify data is current | Updated data displayed |
| 4 | Verify no errors | Refresh completes successfully |

---

### POS_008 - Dashboard Date Range Selector
**Priority:** Normal  
**Labels:** Dashboard, Filter, Date

**Objective:** Validate filtering dashboard data by date range.

**Preconditions:**
- Dashboard has date-sensitive widgets

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate date range selector | Selector visible |
| 2 | Change to "Last 30 Days" | Data filters |
| 3 | Verify widgets update | Data reflects selected period |
| 4 | Change to "This Year" | Data expands |

---

### POS_009 - Dashboard Quick Links
**Priority:** Normal  
**Labels:** Dashboard, Navigation

**Objective:** Validate quick access links on dashboard.

**Preconditions:**
- Dashboard is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate quick links section | Links visible |
| 2 | Click "Create Opportunity" link | Create form opens |
| 3 | Click "View Partners" link | Partners list opens |
| 4 | Click "Log Interaction" link | Interaction form opens |

---

### POS_010 - Dashboard Responsive Layout
**Priority:** Normal  
**Labels:** Dashboard, Responsive

**Objective:** Validate dashboard layout on different screen sizes.

**Preconditions:**
- Responsive testing capability

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard on desktop | Full layout displayed |
| 2 | Resize to tablet | Widgets reflow/stack |
| 3 | Resize to mobile | Mobile-optimized layout |
| 4 | Verify all widgets accessible | All content reachable |

---

### NEG_011 - Dashboard with No Data
**Priority:** Normal  
**Labels:** Dashboard, Empty, Negative

**Objective:** Validate dashboard display for new user with no data.

**Preconditions:**
- New user with no associated data

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as new user | Dashboard loads |
| 2 | Verify empty state widgets | Appropriate empty messages |
| 3 | Verify getting started guidance | Tips or links to get started |
| 4 | Verify no errors | Page displays gracefully |

---

### POS_012 - Dashboard Widget Customization
**Priority:** Low  
**Labels:** Dashboard, Customization

**Objective:** Validate customizing dashboard widgets (if feature exists).

**Preconditions:**
- Widget customization is supported

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate customize option | Settings gear visible |
| 2 | Open widget settings | Customization panel opens |
| 3 | Hide a widget | Widget removed from view |
| 4 | Show hidden widget | Widget restored |
| 5 | Verify settings persist | Preferences saved |

---

### POS_013 - Dashboard Announcements Widget
**Priority:** Normal  
**Labels:** Dashboard, Announcements

**Objective:** Validate system announcements display on dashboard.

**Preconditions:**
- System announcements have been created

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate announcements section | Announcements visible |
| 3 | Verify announcement content | Message text displayed |
| 4 | Click to read more (if applicable) | Full announcement opens |

---

### POS_014 - Dashboard Performance Charts
**Priority:** Normal  
**Labels:** Dashboard, Charts

**Objective:** Validate performance charts on dashboard.

**Preconditions:**
- Historical data exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View dashboard | Dashboard loads |
| 2 | Locate performance charts | Charts visible |
| 3 | Hover over chart elements | Tooltips with details |
| 4 | Verify chart accuracy | Data matches underlying records |

---

### POS_015 - Dashboard Navigate to Module
**Priority:** High  
**Labels:** Dashboard, Navigation

**Objective:** Validate navigation from dashboard to main modules.

**Preconditions:**
- Dashboard is displayed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Partners in navigation | Partners list loads |
| 2 | Return to dashboard | Dashboard loads |
| 3 | Click Opportunities in navigation | Opportunities list loads |
| 4 | Return to dashboard | Dashboard loads |
| 5 | Click Contacts in navigation | Contacts list loads |

---

### POS_016 - Dashboard Session Timeout Handling
**Priority:** Normal  
**Labels:** Dashboard, Session, Security

**Objective:** Validate dashboard behavior on session timeout.

**Preconditions:**
- Session timeout configured

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Load dashboard and wait for timeout | Session expires |
| 2 | Attempt to interact with widget | Redirect to login |
| 3 | Re-login | Dashboard reloads |
| 4 | Verify fresh data loaded | Current data displayed |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 5 |
| Normal | 10 |
| Low | 1 |
| **TOTAL** | **16** |
