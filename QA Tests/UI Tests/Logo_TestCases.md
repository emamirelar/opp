# Application Logo Test Cases

## Overview
Test cases for the Application Logo display across different devices and screen sizes in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-232  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 10

---

## Test Cases

### POS_001 - Validate Logo Display on Desktop
**Priority:** High  
**Labels:** UI, Logo, Desktop

**Objective:** Validate that the application logo displays correctly on desktop.

**Preconditions:**
- Desktop browser (1920x1080 or similar)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to application URL | Homepage loads |
| 2 | Locate application logo | Logo visible in header area |
| 3 | Verify logo quality | Logo is clear, not pixelated |
| 4 | Verify logo positioning | Logo properly aligned in header |

---

### POS_002 - Validate Logo Display on Tablet
**Priority:** Normal  
**Labels:** UI, Logo, Tablet, Responsive

**Objective:** Validate logo display on tablet-sized screens.

**Preconditions:**
- Tablet device or responsive mode (768px width)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set viewport to tablet size | Application adapts |
| 2 | Locate application logo | Logo visible |
| 3 | Verify logo scales appropriately | Logo sized for tablet view |
| 4 | Verify no clipping or overflow | Logo fully visible |

---

### POS_003 - Validate Logo Display on Mobile
**Priority:** Normal  
**Labels:** UI, Logo, Mobile, Responsive

**Objective:** Validate logo display on mobile-sized screens.

**Preconditions:**
- Mobile device or responsive mode (375px width)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set viewport to mobile size | Application adapts |
| 2 | Locate application logo | Logo visible (may be in collapsed menu) |
| 3 | Verify logo scales or adapts for mobile | Logo appropriately sized |
| 4 | Verify usability not impacted | Logo doesn't obstruct navigation |

---

### POS_004 - Logo Click Navigates to Home
**Priority:** High  
**Labels:** UI, Logo, Navigation

**Objective:** Validate that clicking the logo navigates to homepage.

**Preconditions:**
- User is on any page other than home

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to a partner detail page | Page loads |
| 2 | Click on application logo | Navigation occurs |
| 3 | Verify homepage loads | Dashboard/home page displayed |

---

### POS_005 - Logo Has Appropriate Alt Text
**Priority:** Normal  
**Labels:** UI, Logo, Accessibility

**Objective:** Validate logo has proper alt text for accessibility.

**Preconditions:**
- Access to browser developer tools

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to homepage | Page loads |
| 2 | Inspect logo element | Element details visible |
| 3 | Verify alt attribute | Alt text describes logo appropriately |
| 4 | Verify screen reader compatibility | Logo announced correctly |

---

### POS_006 - Logo Loads Without Delay
**Priority:** Normal  
**Labels:** UI, Logo, Performance

**Objective:** Validate logo loads promptly without flicker.

**Preconditions:**
- Clear browser cache

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Clear cache and hard refresh | Page reloads |
| 2 | Observe logo load behavior | Logo appears quickly |
| 3 | Verify no placeholder or broken image | No broken image icon |
| 4 | Verify no layout shift | Page doesn't jump when logo loads |

---

### POS_007 - Logo Display in Sidebar (Collapsed)
**Priority:** Normal  
**Labels:** UI, Logo, Sidebar

**Objective:** Validate logo display when sidebar is collapsed.

**Preconditions:**
- Sidebar can be collapsed
- Desktop view

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open application with sidebar expanded | Full sidebar visible |
| 2 | Collapse sidebar | Sidebar minimizes |
| 3 | Verify logo behavior | Logo shows abbreviated version or icon |
| 4 | Expand sidebar | Full logo returns |

---

### NEG_008 - Handle Logo Image Load Failure
**Priority:** Low  
**Labels:** UI, Logo, Error, Negative

**Objective:** Validate graceful fallback when logo image fails to load.

**Preconditions:**
- Network dev tools to block image

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Block logo image request | Image blocked |
| 2 | Refresh page | Page loads |
| 3 | Verify fallback display | Text fallback or placeholder shown |
| 4 | Verify navigation still works | Clicking logo area still navigates home |

---

### POS_009 - Logo Display in Print View
**Priority:** Low  
**Labels:** UI, Logo, Print

**Objective:** Validate logo display when printing or in print preview.

**Preconditions:**
- Print functionality available

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to a page with content | Page loads |
| 2 | Open print preview (Ctrl+P) | Print preview opens |
| 3 | Verify logo in print layout | Logo included or excluded per design |
| 4 | Verify logo quality in print | Logo clear in print format |

---

### POS_010 - Logo Correct Colors (Light/Dark Theme)
**Priority:** Normal  
**Labels:** UI, Logo, Theme

**Objective:** Validate logo adapts to light and dark themes if supported.

**Preconditions:**
- Theme switching is available

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View application in light theme | Light theme applied |
| 2 | Verify logo visibility | Logo contrasts with light background |
| 3 | Switch to dark theme | Dark theme applied |
| 4 | Verify logo adapts | Logo contrasts with dark background |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 2 |
| Normal | 6 |
| Low | 2 |
| **TOTAL** | **10** |
