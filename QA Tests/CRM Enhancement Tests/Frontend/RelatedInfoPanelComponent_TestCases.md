# RelatedInfoPanelComponent Test Cases

**Component**: `UNOPS.PAO.ClientApp/src/app/shared/components/related-info-panel/`  
**Priority**: P1 - High  
**Total Test Cases**: 40  

---

## Overview

The RelatedInfoPanelComponent displays related entity information:
- Configurable panels for different entity types
- Expandable/collapsible sections
- Quick actions on related items
- Load-on-demand for performance
- Multiple layout options

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Rendering | 10 | P0 |
| Configuration | 8 | P1 |
| Interactions | 8 | P1 |
| Loading | 6 | P0 |
| Responsive | 5 | P2 |
| Accessibility | 3 | P1 |

---

## P0 - Critical Tests

### TC-RIP-001: Renders panel
**Description**: Basic rendering  
**Test Steps**:
1. Provide config and data
2. Check rendering
**Expected Result**: Panel rendered

### TC-RIP-002: Displays panel title
**Description**: Title in header  
**Test Steps**:
1. Set title in config
2. Verify header
**Expected Result**: Title shown

### TC-RIP-003: Displays item count
**Description**: Count badge  
**Test Steps**:
1. Provide items array
2. Verify count
**Expected Result**: Count shown

### TC-RIP-004: Shows loading state
**Description**: Loading indicator  
**Test Steps**:
1. Set loading = true
2. Verify UI
**Expected Result**: Skeleton shown

### TC-RIP-005: Shows empty state
**Description**: No items message  
**Test Steps**:
1. Provide empty array
2. Verify message
**Expected Result**: Empty message shown

### TC-RIP-006: Shows error state
**Description**: Error display  
**Test Steps**:
1. Set error state
2. Verify message
**Expected Result**: Error message shown

### TC-RIP-007: Renders item list
**Description**: Items displayed  
**Test Steps**:
1. Provide items
2. Verify list
**Expected Result**: Items rendered

### TC-RIP-008: Item click navigation
**Description**: Click item  
**Test Steps**:
1. Click item row
2. Verify output
**Expected Result**: itemClick emitted

### TC-RIP-009: Collapse/expand panel
**Description**: Toggle visibility  
**Test Steps**:
1. Click collapse button
2. Verify state
**Expected Result**: Panel collapsed

### TC-RIP-010: Persist collapsed state
**Description**: Remember state  
**Test Steps**:
1. Collapse
2. Re-render
**Expected Result**: Still collapsed

---

## P1 - High Priority Tests

### TC-RIP-011: Config-driven layout
**Description**: Use RelatedInfoConfig  
**Test Steps**:
1. Provide config object
2. Verify layout
**Expected Result**: Config applied

### TC-RIP-012: Column configuration
**Description**: Specify columns  
**Test Steps**:
1. Set columns in config
2. Verify display
**Expected Result**: Columns shown

### TC-RIP-013: Sort configuration
**Description**: Default sort  
**Test Steps**:
1. Set sortBy in config
2. Verify order
**Expected Result**: Sorted correctly

### TC-RIP-014: Max items limit
**Description**: Show top N  
**Test Steps**:
1. Set maxItems = 5
2. Provide 10 items
**Expected Result**: Only 5 shown

### TC-RIP-015: "See all" link
**Description**: View all link  
**Test Steps**:
1. Set showSeeAll = true
2. Verify link
**Expected Result**: Link shown

### TC-RIP-016: See all click
**Description**: Navigate to list  
**Test Steps**:
1. Click "See all"
2. Verify output
**Expected Result**: seeAllClick emitted

### TC-RIP-017: Quick action button
**Description**: Add button  
**Test Steps**:
1. Set showAddButton = true
2. Verify button
**Expected Result**: Add button shown

### TC-RIP-018: Quick action click
**Description**: Add action  
**Test Steps**:
1. Click add button
2. Verify output
**Expected Result**: addClick emitted

### TC-RIP-019: Item hover actions
**Description**: Row actions  
**Test Steps**:
1. Hover item row
2. Verify actions
**Expected Result**: Actions shown

### TC-RIP-020: Item action click
**Description**: Action on item  
**Test Steps**:
1. Click item action
2. Verify output
**Expected Result**: actionClick emitted

### TC-RIP-021: Refresh panel
**Description**: Reload data  
**Test Steps**:
1. Click refresh
2. Verify reload
**Expected Result**: refreshClick emitted

### TC-RIP-022: Filter items
**Description**: Search within panel  
**Test Steps**:
1. Set showFilter = true
2. Enter filter text
**Expected Result**: Items filtered

---

## Loading Tests

### TC-RIP-023: Lazy load data
**Description**: Load on expand  
**Test Steps**:
1. Set lazyLoad = true
2. Expand panel
**Expected Result**: Data loaded

### TC-RIP-024: Skeleton loading
**Description**: Skeleton items  
**Test Steps**:
1. Show loading
2. Verify skeleton
**Expected Result**: Skeleton shown

### TC-RIP-025: Error retry
**Description**: Retry on error  
**Test Steps**:
1. Show error
2. Click retry
**Expected Result**: Reload triggered

### TC-RIP-026: Partial refresh
**Description**: Refresh single panel  
**Test Steps**:
1. Refresh one panel
2. Verify others unchanged
**Expected Result**: Only one reloaded

---

## Responsive Tests

### TC-RIP-027: Mobile layout
**Description**: Stacked on mobile  
**Test Steps**:
1. Set mobile viewport
2. Verify layout
**Expected Result**: Full width

### TC-RIP-028: Tablet layout
**Description**: Two columns  
**Test Steps**:
1. Set tablet viewport
2. Verify layout
**Expected Result**: 2 columns

### TC-RIP-029: Desktop layout
**Description**: Side panel  
**Test Steps**:
1. Set desktop viewport
2. Verify layout
**Expected Result**: Side panel

### TC-RIP-030: Panel reorder
**Description**: Drag to reorder  
**Test Steps**:
1. Drag panel
2. Verify order
**Expected Result**: Reordered

### TC-RIP-031: Save layout preference
**Description**: Remember order  
**Test Steps**:
1. Reorder
2. Reload
**Expected Result**: Order saved

---

## Accessibility Tests

### TC-RIP-A001: Keyboard expand
**Description**: Enter to expand  
**Test Steps**:
1. Focus header
2. Press Enter
**Expected Result**: Panel toggles

### TC-RIP-A002: ARIA expanded
**Description**: State announced  
**Test Steps**:
1. Check aria-expanded
**Expected Result**: Correct state

### TC-RIP-A003: Focus management
**Description**: Focus on expand  
**Test Steps**:
1. Expand panel
2. Check focus
**Expected Result**: Focus in panel

---

**Last Updated**: December 18, 2025

