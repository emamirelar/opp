# Take a Tour Feature Test Cases

## Overview
Test cases for the "Take a Tour" onboarding feature in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-446  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 12

---

## Test Cases

### POS_001 - Validate Take a Tour Button Visibility
**Priority:** High  
**Labels:** Onboarding, Tour, UI

**Objective:** Validate that the "Take a Tour" button is visible for new users.

**Preconditions:**
- User is newly created or has not completed the tour
- User is logged in

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as a new user | Dashboard/homepage loads |
| 2 | Locate "Take a Tour" button or prompt | Button/prompt is visible in header or modal |
| 3 | Verify button is clickable | Button has hover state and is interactive |

---

### POS_002 - Start Tour Successfully
**Priority:** High  
**Labels:** Onboarding, Tour

**Objective:** Validate that clicking "Take a Tour" starts the guided tour.

**Preconditions:**
- "Take a Tour" button is visible

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Take a Tour" button | Tour overlay/modal appears |
| 2 | Verify first tour step is displayed | Step indicator shows "Step 1 of X" |
| 3 | Verify highlighted area | First feature is highlighted with explanation |

---

### POS_003 - Navigate Tour Steps Forward
**Priority:** High  
**Labels:** Onboarding, Tour, Navigation

**Objective:** Validate navigating forward through tour steps.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View current tour step | Step content displayed |
| 2 | Click "Next" button | Next step is displayed |
| 3 | Verify step counter updates | Counter shows "Step 2 of X" |
| 4 | Continue clicking Next through all steps | Each step displays correctly |

---

### POS_004 - Navigate Tour Steps Backward
**Priority:** Normal  
**Labels:** Onboarding, Tour, Navigation

**Objective:** Validate navigating backward through tour steps.

**Preconditions:**
- Tour is past first step

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Step 3 or later | Step displayed |
| 2 | Click "Previous" or "Back" button | Previous step displayed |
| 3 | Verify step counter updates | Counter decrements |
| 4 | Verify content matches previous step | Correct step content shown |

---

### POS_005 - Skip Tour Functionality
**Priority:** High  
**Labels:** Onboarding, Tour

**Objective:** Validate that users can skip the tour.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View any tour step | Step content displayed |
| 2 | Locate "Skip Tour" or "X" close button | Button is visible |
| 3 | Click "Skip Tour" | Confirmation dialog may appear |
| 4 | Confirm skip | Tour closes |
| 5 | Verify user returns to normal view | Application is usable without tour overlay |

---

### POS_006 - Complete Tour Successfully
**Priority:** High  
**Labels:** Onboarding, Tour

**Objective:** Validate completing all tour steps.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate through all tour steps | Each step displays |
| 2 | Click "Next" on final step | "Finish" or "Complete" button appears |
| 3 | Click "Finish" | Tour closes with success message |
| 4 | Verify tour completion is recorded | "Take a Tour" button may be hidden or changed to "Restart Tour" |

---

### POS_007 - Tour Highlights Correct Elements
**Priority:** Normal  
**Labels:** Onboarding, Tour, UI

**Objective:** Validate that each tour step highlights the correct UI element.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Start tour | First element highlighted |
| 2 | Verify highlight matches step description | Highlighted element is what the text describes |
| 3 | Navigate to next step | New element highlighted |
| 4 | Repeat verification for each step | All highlights match descriptions |

---

### POS_008 - Tour Tooltip Positioning
**Priority:** Normal  
**Labels:** Onboarding, Tour, UI

**Objective:** Validate that tour tooltips are positioned correctly and visible.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View each tour step | Tooltip appears |
| 2 | Verify tooltip doesn't overlap with highlighted element | Content is readable |
| 3 | Verify tooltip is fully visible on screen | No clipping at screen edges |
| 4 | Resize window and verify responsiveness | Tooltip adjusts position as needed |

---

### POS_009 - Restart Tour Option
**Priority:** Normal  
**Labels:** Onboarding, Tour

**Objective:** Validate that users can restart the tour after completion.

**Preconditions:**
- User has previously completed the tour

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as user who completed tour | Application loads |
| 2 | Locate "Help" menu or settings | Menu accessible |
| 3 | Find "Restart Tour" or "Take a Tour" option | Option is available |
| 4 | Click to restart tour | Tour begins from Step 1 |

---

### NEG_010 - Tour on Different Screen Sizes
**Priority:** Normal  
**Labels:** Onboarding, Tour, Responsive

**Objective:** Validate tour functionality on tablet and mobile screen sizes.

**Preconditions:**
- Responsive testing capability

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set viewport to tablet size (768px) | Application adapts |
| 2 | Start tour | Tour displays correctly |
| 3 | Navigate through steps | All steps accessible |
| 4 | Set viewport to mobile size (375px) | Application adapts |
| 5 | Verify tour remains functional | Tour steps display and navigation works |

---

### NEG_011 - Tour Persists After Page Refresh
**Priority:** Normal  
**Labels:** Onboarding, Tour, State

**Objective:** Validate tour state after page refresh.

**Preconditions:**
- Tour is in progress (e.g., Step 3)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Step 3 of tour | Step 3 displayed |
| 2 | Refresh the browser page | Page reloads |
| 3 | Check tour state | Tour resumes from Step 3 OR restarts from beginning (document expected behavior) |

---

### POS_012 - Tour Keyboard Navigation
**Priority:** Low  
**Labels:** Onboarding, Tour, Accessibility

**Objective:** Validate tour can be navigated using keyboard.

**Preconditions:**
- Tour is started

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Start tour | Tour overlay appears |
| 2 | Press Tab key | Focus moves to Next button |
| 3 | Press Enter | Next step displayed |
| 4 | Press Escape | Tour closes or skip dialog appears |
| 5 | Verify all interactions keyboard accessible | Tour fully navigable via keyboard |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 5 |
| Normal | 6 |
| Low | 1 |
| **TOTAL** | **12** |
