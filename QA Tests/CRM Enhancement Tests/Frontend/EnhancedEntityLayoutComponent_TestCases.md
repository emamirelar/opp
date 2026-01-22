# EnhancedEntityLayoutComponent Test Cases

**Component**: `UNOPS.PAO.ClientApp/src/app/shared/components/enhanced-entity-layout/`  
**Priority**: P1 - High  
**Total Test Cases**: 45  

---

## Overview

The EnhancedEntityLayoutComponent provides the master layout for entity views:
- Main content area with entity details
- Side panel with related info
- Responsive layout switching
- Panel management integration
- Consistent UX across entities

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Layout Rendering | 10 | P0 |
| Side Panel | 10 | P1 |
| Responsive | 8 | P1 |
| Panel Management | 8 | P1 |
| Interactions | 6 | P1 |
| Accessibility | 3 | P1 |

---

## P0 - Critical Tests

### TC-EEL-001: Renders layout
**Description**: Basic rendering  
**Test Steps**:
1. Provide config
2. Check rendering
**Expected Result**: Layout rendered

### TC-EEL-002: Main content area
**Description**: Primary content  
**Test Steps**:
1. Project main content
2. Verify display
**Expected Result**: Content shown

### TC-EEL-003: Side panel area
**Description**: Related info panels  
**Test Steps**:
1. Set showSidePanel = true
2. Verify side area
**Expected Result**: Side panel shown

### TC-EEL-004: Header integration
**Description**: Entity header  
**Test Steps**:
1. Provide entity data
2. Verify header
**Expected Result**: Header rendered

### TC-EEL-005: Tab integration
**Description**: Tab navigation  
**Test Steps**:
1. Provide tabs config
2. Verify tabs
**Expected Result**: Tabs functional

### TC-EEL-006: Loading state
**Description**: Full page loading  
**Test Steps**:
1. Set loading = true
2. Verify UI
**Expected Result**: Loading overlay

### TC-EEL-007: Error state
**Description**: Full page error  
**Test Steps**:
1. Set error state
2. Verify UI
**Expected Result**: Error message

### TC-EEL-008: Entity type styling
**Description**: Type-specific theme  
**Test Steps**:
1. Set entityType = "Partner"
2. Verify styling
**Expected Result**: Partner theme

### TC-EEL-009: Breadcrumb integration
**Description**: Navigation breadcrumb  
**Test Steps**:
1. Provide breadcrumb data
2. Verify display
**Expected Result**: Breadcrumb shown

### TC-EEL-010: Action bar integration
**Description**: Entity actions  
**Test Steps**:
1. Provide actions
2. Verify bar
**Expected Result**: Actions shown

---

## P1 - High Priority Tests

### TC-EEL-011: Toggle side panel
**Description**: Show/hide side  
**Test Steps**:
1. Click toggle button
2. Verify state
**Expected Result**: Panel toggles

### TC-EEL-012: Side panel collapsed state
**Description**: Minimized view  
**Test Steps**:
1. Collapse side panel
2. Verify width
**Expected Result**: Narrow column

### TC-EEL-013: Multiple panels
**Description**: Stacked panels  
**Test Steps**:
1. Configure multiple panels
2. Verify display
**Expected Result**: All panels shown

### TC-EEL-014: Panel lazy loading
**Description**: Load on visibility  
**Test Steps**:
1. Configure lazy panels
2. Scroll into view
**Expected Result**: Panel loads

### TC-EEL-015: Panel order from service
**Description**: Use saved order  
**Test Steps**:
1. Set custom order
2. Reload
**Expected Result**: Order preserved

### TC-EEL-016: Panel visibility from service
**Description**: Use saved visibility  
**Test Steps**:
1. Hide panel
2. Reload
**Expected Result**: Still hidden

### TC-EEL-017: Panel config customization
**Description**: Panel settings button  
**Test Steps**:
1. Click panel settings
2. Verify dialog
**Expected Result**: Settings dialog

### TC-EEL-018: Panel drag reorder
**Description**: Drag and drop  
**Test Steps**:
1. Drag panel
2. Drop in new position
**Expected Result**: Reordered

### TC-EEL-019: Mobile layout switch
**Description**: Responsive switch  
**Test Steps**:
1. Resize to mobile
2. Verify layout
**Expected Result**: Stacked layout

### TC-EEL-020: Tablet layout switch
**Description**: Responsive switch  
**Test Steps**:
1. Resize to tablet
2. Verify layout
**Expected Result**: Adjusted layout

### TC-EEL-021: Desktop layout
**Description**: Full layout  
**Test Steps**:
1. Desktop viewport
2. Verify layout
**Expected Result**: Side-by-side

### TC-EEL-022: Panel width resize
**Description**: Drag to resize  
**Test Steps**:
1. Drag resize handle
2. Verify width
**Expected Result**: Width changed

### TC-EEL-023: Min/max panel width
**Description**: Width limits  
**Test Steps**:
1. Drag to extreme
2. Verify limits
**Expected Result**: Respects min/max

### TC-EEL-024: Print layout
**Description**: Print-friendly  
**Test Steps**:
1. Trigger print
2. Verify layout
**Expected Result**: Print optimized

### TC-EEL-025: Full screen mode
**Description**: Maximize main  
**Test Steps**:
1. Click full screen
2. Verify layout
**Expected Result**: Main maximized

### TC-EEL-026: Exit full screen
**Description**: Restore layout  
**Test Steps**:
1. Exit full screen
2. Verify layout
**Expected Result**: Normal layout

---

## Panel Management Tests

### TC-EEL-027: Show panel manager
**Description**: Open manager  
**Test Steps**:
1. Click manage panels
2. Verify dialog
**Expected Result**: Manager shown

### TC-EEL-028: Toggle panel in manager
**Description**: Show/hide panel  
**Test Steps**:
1. Toggle panel checkbox
2. Apply
**Expected Result**: Visibility changed

### TC-EEL-029: Reorder in manager
**Description**: Drag in manager  
**Test Steps**:
1. Reorder in manager
2. Apply
**Expected Result**: Order changed

### TC-EEL-030: Reset panels
**Description**: Reset to default  
**Test Steps**:
1. Click reset
2. Verify layout
**Expected Result**: Default restored

### TC-EEL-031: Apply preset
**Description**: Use preset  
**Test Steps**:
1. Select preset
2. Apply
**Expected Result**: Preset applied

### TC-EEL-032: Cancel manager changes
**Description**: Discard changes  
**Test Steps**:
1. Make changes
2. Cancel
**Expected Result**: No changes

### TC-EEL-033: Save manager changes
**Description**: Apply changes  
**Test Steps**:
1. Make changes
2. Save
**Expected Result**: Changes saved

### TC-EEL-034: Manager preference persist
**Description**: Save to service  
**Test Steps**:
1. Change and save
2. Reload page
**Expected Result**: Changes persist

---

## Interaction Tests

### TC-EEL-035: Tab change handling
**Description**: Tab switch  
**Test Steps**:
1. Click tab
2. Verify output
**Expected Result**: tabChange emitted

### TC-EEL-036: Action handling
**Description**: Action click  
**Test Steps**:
1. Click action
2. Verify output
**Expected Result**: actionClick emitted

### TC-EEL-037: Back navigation
**Description**: Back button  
**Test Steps**:
1. Click back
2. Verify navigation
**Expected Result**: Navigate back

### TC-EEL-038: Panel item click
**Description**: Related item click  
**Test Steps**:
1. Click related item
2. Verify navigation
**Expected Result**: Navigate to item

### TC-EEL-039: Panel add action
**Description**: Add from panel  
**Test Steps**:
1. Click add in panel
2. Verify dialog
**Expected Result**: Add dialog shown

### TC-EEL-040: Panel refresh
**Description**: Refresh panel data  
**Test Steps**:
1. Click refresh
2. Verify reload
**Expected Result**: Data reloaded

---

## Accessibility Tests

### TC-EEL-A001: Skip to main content
**Description**: Skip link  
**Test Steps**:
1. Tab to skip link
2. Activate
**Expected Result**: Focus to main

### TC-EEL-A002: Landmark regions
**Description**: ARIA landmarks  
**Test Steps**:
1. Check landmarks
**Expected Result**: Proper landmarks

### TC-EEL-A003: Panel resize keyboard
**Description**: Keyboard resize  
**Test Steps**:
1. Focus resize handle
2. Use arrow keys
**Expected Result**: Width changes

---

**Last Updated**: December 18, 2025

