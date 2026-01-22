# BaseEntityViewComponent Test Cases

**Component**: `UNOPS.PAO.ClientApp/src/app/shared/components/base-entity-view/`  
**Priority**: P1 - High  
**Total Test Cases**: 35  

---

## Overview

The BaseEntityViewComponent provides a reusable base for entity views:
- Common layout structure
- Header with entity info
- Tab navigation
- Action buttons
- Loading states
- Error handling

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Rendering | 8 | P0 |
| Input Signals | 8 | P0 |
| Tab Navigation | 6 | P1 |
| Actions | 6 | P1 |
| Loading/Error States | 4 | P0 |
| Accessibility | 3 | P1 |

---

## P0 - Critical Tests

### TC-BEV-001: Renders with entity data
**Description**: Component renders correctly  
**Test Steps**:
1. Provide entity input
2. Check rendering
**Expected Result**: Entity displayed

### TC-BEV-002: Displays entity title
**Description**: Title in header  
**Test Steps**:
1. Set title input
2. Verify header
**Expected Result**: Title shown

### TC-BEV-003: Displays entity subtitle
**Description**: Subtitle in header  
**Test Steps**:
1. Set subtitle input
2. Verify header
**Expected Result**: Subtitle shown

### TC-BEV-004: Displays entity status
**Description**: Status badge  
**Test Steps**:
1. Set status input
2. Verify badge
**Expected Result**: Status badge shown

### TC-BEV-005: Shows loading state
**Description**: Loading indicator  
**Test Steps**:
1. Set loading = true
2. Verify UI
**Expected Result**: Loading shown

### TC-BEV-006: Shows error state
**Description**: Error message  
**Test Steps**:
1. Set error input
2. Verify UI
**Expected Result**: Error message shown

### TC-BEV-007: Shows empty state
**Description**: No data message  
**Test Steps**:
1. Provide null entity
2. Verify UI
**Expected Result**: Empty message shown

### TC-BEV-008: Handles entity type input
**Description**: Different entity types  
**Test Steps**:
1. Set entityType = "Partner"
2. Verify styling
**Expected Result**: Type-specific styling

### TC-BEV-009: Icon input displayed
**Description**: Entity icon  
**Test Steps**:
1. Set icon input
2. Verify header
**Expected Result**: Icon shown

### TC-BEV-010: Breadcrumb displayed
**Description**: Navigation breadcrumb  
**Test Steps**:
1. Set breadcrumb input
2. Verify UI
**Expected Result**: Breadcrumb shown

### TC-BEV-011: Entity ID displayed
**Description**: Reference number  
**Test Steps**:
1. Provide entity with ID
2. Verify display
**Expected Result**: ID/ref shown

### TC-BEV-012: Last modified shown
**Description**: Timestamp display  
**Test Steps**:
1. Provide lastModified
2. Verify display
**Expected Result**: Date shown

---

## P1 - High Priority Tests

### TC-BEV-013: Renders tabs
**Description**: Tab navigation  
**Test Steps**:
1. Set tabs input
2. Verify tabs
**Expected Result**: Tabs rendered

### TC-BEV-014: Tab selection
**Description**: Click tab  
**Test Steps**:
1. Click tab
2. Verify output
**Expected Result**: tabChange emitted

### TC-BEV-015: Active tab styling
**Description**: Active indicator  
**Test Steps**:
1. Set activeTab
2. Verify styling
**Expected Result**: Active styled

### TC-BEV-016: Tab content projection
**Description**: ng-content in tabs  
**Test Steps**:
1. Project content
2. Switch tabs
**Expected Result**: Correct content shown

### TC-BEV-017: Tab disabled state
**Description**: Disable specific tab  
**Test Steps**:
1. Set tab.disabled = true
2. Verify interaction
**Expected Result**: Tab not clickable

### TC-BEV-018: Tab badge count
**Description**: Item count badge  
**Test Steps**:
1. Set tab.count = 5
2. Verify badge
**Expected Result**: "5" badge shown

### TC-BEV-019: Action button display
**Description**: Primary action  
**Test Steps**:
1. Set actions input
2. Verify buttons
**Expected Result**: Buttons shown

### TC-BEV-020: Action click handling
**Description**: Button click  
**Test Steps**:
1. Click action button
2. Verify output
**Expected Result**: actionClick emitted

### TC-BEV-021: Action disabled state
**Description**: Disable action  
**Test Steps**:
1. Set action.disabled = true
2. Verify state
**Expected Result**: Button disabled

### TC-BEV-022: Action loading state
**Description**: Action in progress  
**Test Steps**:
1. Set action.loading = true
2. Verify UI
**Expected Result**: Loading indicator

### TC-BEV-023: More actions menu
**Description**: Overflow menu  
**Test Steps**:
1. Set many actions
2. Verify menu
**Expected Result**: Menu with overflow

### TC-BEV-024: Back button navigation
**Description**: Back to list  
**Test Steps**:
1. Click back button
2. Verify output
**Expected Result**: backClick emitted

---

## Accessibility Tests

### TC-BEV-A001: Keyboard navigation
**Description**: Tab through tabs  
**Test Steps**:
1. Focus component
2. Use arrow keys
**Expected Result**: Tabs navigable

### TC-BEV-A002: ARIA labels
**Description**: Screen reader support  
**Test Steps**:
1. Check ARIA attributes
**Expected Result**: Proper labels

### TC-BEV-A003: Focus management
**Description**: Focus trap in actions  
**Test Steps**:
1. Open action menu
2. Tab through
**Expected Result**: Focus managed

---

**Last Updated**: December 18, 2025

