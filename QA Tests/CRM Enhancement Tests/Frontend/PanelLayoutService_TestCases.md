# PanelLayoutService Test Cases

**Service**: `UNOPS.PAO.ClientApp/src/app/shared/services/panel-layout.service.ts`  
**Priority**: P1 - High  
**Total Test Cases**: 25  

---

## Overview

The PanelLayoutService manages panel layout configuration:
- Panel visibility and order
- User preferences storage
- Layout presets
- Responsive breakpoints

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Layout Management | 8 | P0 |
| User Preferences | 6 | P1 |
| Presets | 5 | P1 |
| Responsive | 4 | P2 |
| Events | 2 | P1 |

---

## P0 - Critical Tests

### TC-PLS-001: Get default layout
**Description**: Initial layout config  
**Test Steps**:
1. Call `getLayout(entityType)`
**Expected Result**: Default layout returned

### TC-PLS-002: Set panel visibility
**Description**: Show/hide panel  
**Test Steps**:
1. Call `setPanelVisibility(panelId, false)`
**Expected Result**: Panel hidden

### TC-PLS-003: Get panel visibility
**Description**: Check if visible  
**Test Steps**:
1. Hide panel
2. Call `isPanelVisible(panelId)`
**Expected Result**: false

### TC-PLS-004: Set panel order
**Description**: Reorder panels  
**Test Steps**:
1. Call `setPanelOrder(newOrder)`
**Expected Result**: Order updated

### TC-PLS-005: Get panel order
**Description**: Current order  
**Test Steps**:
1. Call `getPanelOrder(entityType)`
**Expected Result**: Order array

### TC-PLS-006: Reset to default
**Description**: Reset layout  
**Test Steps**:
1. Customize layout
2. Call `resetToDefault(entityType)`
**Expected Result**: Default restored

### TC-PLS-007: Move panel up
**Description**: Reorder single panel  
**Test Steps**:
1. Call `movePanelUp(panelId)`
**Expected Result**: Panel moved up

### TC-PLS-008: Move panel down
**Description**: Reorder single panel  
**Test Steps**:
1. Call `movePanelDown(panelId)`
**Expected Result**: Panel moved down

---

## P1 - High Priority Tests

### TC-PLS-009: Save preferences
**Description**: Persist to storage  
**Test Steps**:
1. Customize layout
2. Call `savePreferences()`
**Expected Result**: Saved to storage

### TC-PLS-010: Load preferences
**Description**: Load from storage  
**Test Steps**:
1. Save preferences
2. Call `loadPreferences()`
**Expected Result**: Loaded correctly

### TC-PLS-011: Clear preferences
**Description**: Remove saved  
**Test Steps**:
1. Save preferences
2. Call `clearPreferences()`
**Expected Result**: Preferences cleared

### TC-PLS-012: Per-entity preferences
**Description**: Different per entity  
**Test Steps**:
1. Set Partner layout
2. Set Contact layout
3. Verify separate
**Expected Result**: Separate configs

### TC-PLS-013: Per-user preferences
**Description**: User-specific  
**Test Steps**:
1. User A sets layout
2. User B has different
**Expected Result**: Separate by user

### TC-PLS-014: Merge with defaults
**Description**: Handle new panels  
**Test Steps**:
1. Load old preferences
2. Add new panel to default
**Expected Result**: New panel included

### TC-PLS-015: Apply preset
**Description**: Use layout preset  
**Test Steps**:
1. Call `applyPreset(presetName)`
**Expected Result**: Preset applied

### TC-PLS-016: Get available presets
**Description**: List presets  
**Test Steps**:
1. Call `getPresets(entityType)`
**Expected Result**: Preset list

### TC-PLS-017: Create custom preset
**Description**: Save as preset  
**Test Steps**:
1. Customize layout
2. Call `saveAsPreset(name)`
**Expected Result**: Preset saved

### TC-PLS-018: Delete preset
**Description**: Remove preset  
**Test Steps**:
1. Create preset
2. Call `deletePreset(name)`
**Expected Result**: Preset deleted

### TC-PLS-019: Share preset
**Description**: Make preset public  
**Test Steps**:
1. Create preset
2. Call `sharePreset(name)`
**Expected Result**: Preset shared

---

## Responsive Tests

### TC-PLS-020: Get mobile layout
**Description**: Mobile-specific  
**Test Steps**:
1. Call `getLayout(type, 'mobile')`
**Expected Result**: Mobile layout

### TC-PLS-021: Get tablet layout
**Description**: Tablet-specific  
**Test Steps**:
1. Call `getLayout(type, 'tablet')`
**Expected Result**: Tablet layout

### TC-PLS-022: Get desktop layout
**Description**: Desktop-specific  
**Test Steps**:
1. Call `getLayout(type, 'desktop')`
**Expected Result**: Desktop layout

### TC-PLS-023: Auto-detect breakpoint
**Description**: Use current screen  
**Test Steps**:
1. Call `getLayout(type)`
**Expected Result**: Appropriate layout

---

## Event Tests

### TC-PLS-024: Layout change event
**Description**: Emit on change  
**Test Steps**:
1. Subscribe to layoutChange$
2. Update layout
**Expected Result**: Event emitted

### TC-PLS-025: Preference save event
**Description**: Emit on save  
**Test Steps**:
1. Subscribe to preferencesSaved$
2. Save preferences
**Expected Result**: Event emitted

---

**Last Updated**: December 18, 2025

