# Enhanced Partner View Test Cases

**Component**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/partners/components/partner/view/`  
**Priority**: P1 - High  
**Total Test Cases**: 40  

---

## Overview

The Enhanced Partner View integrates:
- EnhancedEntityLayoutComponent as base
- Partner-specific related info panels
- Contacts panel
- Interactions panel
- Documents panel
- Engagements panel (new)

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Layout Integration | 8 | P0 |
| Contacts Panel | 8 | P1 |
| Interactions Panel | 6 | P1 |
| Documents Panel | 6 | P1 |
| Engagements Panel | 6 | P1 |
| Quick Actions | 4 | P1 |
| Navigation | 2 | P0 |

---

## P0 - Critical Tests

### TC-PVE-001: Renders enhanced layout
**Description**: Uses EnhancedEntityLayoutComponent  
**Test Steps**:
1. Navigate to partner view
2. Verify layout
**Expected Result**: Enhanced layout shown

### TC-PVE-002: Partner header displayed
**Description**: Name, status, type  
**Test Steps**:
1. Load partner
2. Verify header
**Expected Result**: Partner info in header

### TC-PVE-003: Partner tabs displayed
**Description**: Overview, Details, etc.  
**Test Steps**:
1. Load partner
2. Verify tabs
**Expected Result**: Tabs rendered

### TC-PVE-004: Side panel displayed
**Description**: Related panels  
**Test Steps**:
1. Load partner
2. Verify side panel
**Expected Result**: Panels shown

### TC-PVE-005: Contacts panel visible
**Description**: Shows related contacts  
**Test Steps**:
1. Load partner with contacts
2. Verify panel
**Expected Result**: Contacts listed

### TC-PVE-006: Interactions panel visible
**Description**: Shows interactions  
**Test Steps**:
1. Load partner with interactions
2. Verify panel
**Expected Result**: Interactions listed

### TC-PVE-007: Documents panel visible
**Description**: Shows documents  
**Test Steps**:
1. Load partner with documents
2. Verify panel
**Expected Result**: Documents listed

### TC-PVE-008: Navigate to partner detail
**Description**: Deep link works  
**Test Steps**:
1. Navigate to /partners/{id}
2. Verify load
**Expected Result**: Partner loaded

---

## P1 - High Priority Tests

### Contacts Panel Tests

### TC-PVE-009: Contacts count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with 5 contacts
2. Verify count
**Expected Result**: "Contacts (5)"

### TC-PVE-010: Contact item display
**Description**: Name, role, email  
**Test Steps**:
1. Verify contact row
**Expected Result**: Info displayed

### TC-PVE-011: Click contact navigates
**Description**: Navigate to contact  
**Test Steps**:
1. Click contact
2. Verify navigation
**Expected Result**: Contact view

### TC-PVE-012: Add contact button
**Description**: Quick add  
**Test Steps**:
1. Click add contact
2. Verify dialog
**Expected Result**: Add dialog

### TC-PVE-013: Primary contact indicator
**Description**: Primary badge  
**Test Steps**:
1. Load with primary contact
**Expected Result**: Badge shown

### TC-PVE-014: See all contacts link
**Description**: Full list  
**Test Steps**:
1. Click "See all"
**Expected Result**: Navigate to list

### TC-PVE-015: Contact email action
**Description**: Quick email  
**Test Steps**:
1. Click email icon
**Expected Result**: Email opens

### TC-PVE-016: Contact phone action
**Description**: Quick call  
**Test Steps**:
1. Click phone icon
**Expected Result**: Phone opens

---

### Interactions Panel Tests

### TC-PVE-017: Interactions count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with interactions
2. Verify count
**Expected Result**: Count shown

### TC-PVE-018: Interaction item display
**Description**: Type, date, summary  
**Test Steps**:
1. Verify interaction row
**Expected Result**: Info displayed

### TC-PVE-019: Click interaction navigates
**Description**: Navigate to detail  
**Test Steps**:
1. Click interaction
**Expected Result**: Interaction view

### TC-PVE-020: Add interaction button
**Description**: Quick add  
**Test Steps**:
1. Click add interaction
**Expected Result**: Add dialog

### TC-PVE-021: Interaction type icon
**Description**: Type indicator  
**Test Steps**:
1. Verify type icons
**Expected Result**: Correct icons

### TC-PVE-022: Recent interactions first
**Description**: Sort by date  
**Test Steps**:
1. Verify order
**Expected Result**: Most recent first

---

### Documents Panel Tests

### TC-PVE-023: Documents count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with documents
**Expected Result**: Count shown

### TC-PVE-024: Document item display
**Description**: Name, type, date  
**Test Steps**:
1. Verify document row
**Expected Result**: Info displayed

### TC-PVE-025: Download document
**Description**: Quick download  
**Test Steps**:
1. Click download icon
**Expected Result**: Download starts

### TC-PVE-026: Upload document button
**Description**: Quick upload  
**Test Steps**:
1. Click upload button
**Expected Result**: Upload dialog

### TC-PVE-027: Document preview
**Description**: Inline preview  
**Test Steps**:
1. Click preview icon
**Expected Result**: Preview opens

### TC-PVE-028: Document type icon
**Description**: File type indicator  
**Test Steps**:
1. Verify file icons
**Expected Result**: Correct icons

---

### Engagements Panel Tests (New)

### TC-PVE-029: Engagements panel present
**Description**: New panel  
**Test Steps**:
1. Verify panel exists
**Expected Result**: Panel shown

### TC-PVE-030: Engagements count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with engagements
**Expected Result**: Count shown

### TC-PVE-031: Engagement item display
**Description**: Title, status, date  
**Test Steps**:
1. Verify engagement row
**Expected Result**: Info displayed

### TC-PVE-032: Click engagement navigates
**Description**: Navigate to detail  
**Test Steps**:
1. Click engagement
**Expected Result**: Engagement view

### TC-PVE-033: Add engagement button
**Description**: Quick add  
**Test Steps**:
1. Click add engagement
**Expected Result**: Add dialog

### TC-PVE-034: Engagement status badge
**Description**: Status indicator  
**Test Steps**:
1. Verify status badge
**Expected Result**: Correct status

---

### Quick Actions Tests

### TC-PVE-035: Edit partner action
**Description**: Quick edit  
**Test Steps**:
1. Click edit
**Expected Result**: Edit mode/dialog

### TC-PVE-036: Share partner action
**Description**: Share link  
**Test Steps**:
1. Click share
**Expected Result**: Share dialog

### TC-PVE-037: Export partner action
**Description**: Export to PDF  
**Test Steps**:
1. Click export
**Expected Result**: Export generated

### TC-PVE-038: Delete partner action
**Description**: Delete with confirm  
**Test Steps**:
1. Click delete
**Expected Result**: Confirmation dialog

---

### Navigation Tests

### TC-PVE-039: Back to list
**Description**: Navigate back  
**Test Steps**:
1. Click back
**Expected Result**: Partner list

### TC-PVE-040: Breadcrumb navigation
**Description**: Click breadcrumb  
**Test Steps**:
1. Click breadcrumb item
**Expected Result**: Navigate

---

**Last Updated**: December 18, 2025

