# Enhanced Contact View Test Cases

**Component**: `UNOPS.PAO.ClientApp/src/app/features/partnerships/contacts/components/contact/view/`  
**Priority**: P1 - High  
**Total Test Cases**: 38  

---

## Overview

The Enhanced Contact View integrates:
- EnhancedEntityLayoutComponent as base
- Contact-specific related info panels
- Partner associations panel
- Interactions panel
- Documents panel
- Communication history panel (new)

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Layout Integration | 8 | P0 |
| Partner Panel | 6 | P1 |
| Interactions Panel | 6 | P1 |
| Documents Panel | 6 | P1 |
| Communication Panel | 6 | P1 |
| Quick Actions | 4 | P1 |
| Navigation | 2 | P0 |

---

## P0 - Critical Tests

### TC-CVE-001: Renders enhanced layout
**Description**: Uses EnhancedEntityLayoutComponent  
**Test Steps**:
1. Navigate to contact view
2. Verify layout
**Expected Result**: Enhanced layout shown

### TC-CVE-002: Contact header displayed
**Description**: Name, role, partner  
**Test Steps**:
1. Load contact
2. Verify header
**Expected Result**: Contact info in header

### TC-CVE-003: Contact tabs displayed
**Description**: Overview, Details, etc.  
**Test Steps**:
1. Load contact
2. Verify tabs
**Expected Result**: Tabs rendered

### TC-CVE-004: Side panel displayed
**Description**: Related panels  
**Test Steps**:
1. Load contact
2. Verify side panel
**Expected Result**: Panels shown

### TC-CVE-005: Partner panel visible
**Description**: Shows associated partner  
**Test Steps**:
1. Load contact with partner
2. Verify panel
**Expected Result**: Partner shown

### TC-CVE-006: Interactions panel visible
**Description**: Shows interactions  
**Test Steps**:
1. Load contact with interactions
2. Verify panel
**Expected Result**: Interactions listed

### TC-CVE-007: Documents panel visible
**Description**: Shows documents  
**Test Steps**:
1. Load contact with documents
2. Verify panel
**Expected Result**: Documents listed

### TC-CVE-008: Navigate to contact detail
**Description**: Deep link works  
**Test Steps**:
1. Navigate to /contacts/{id}
2. Verify load
**Expected Result**: Contact loaded

---

## P1 - High Priority Tests

### Partner Panel Tests

### TC-CVE-009: Partner info display
**Description**: Partner name, type  
**Test Steps**:
1. Verify partner row
**Expected Result**: Info displayed

### TC-CVE-010: Click partner navigates
**Description**: Navigate to partner  
**Test Steps**:
1. Click partner
**Expected Result**: Partner view

### TC-CVE-011: Primary partner indicator
**Description**: Primary badge  
**Test Steps**:
1. Load with primary partner
**Expected Result**: Badge shown

### TC-CVE-012: Multiple partners
**Description**: Associated with multiple  
**Test Steps**:
1. Load with 3 partners
**Expected Result**: All listed

### TC-CVE-013: Change primary partner
**Description**: Quick action  
**Test Steps**:
1. Click set primary
**Expected Result**: Primary changed

### TC-CVE-014: Associate partner button
**Description**: Add association  
**Test Steps**:
1. Click associate
**Expected Result**: Partner picker

---

### Interactions Panel Tests

### TC-CVE-015: Interactions count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with interactions
**Expected Result**: Count shown

### TC-CVE-016: Interaction item display
**Description**: Type, date, summary  
**Test Steps**:
1. Verify interaction row
**Expected Result**: Info displayed

### TC-CVE-017: Click interaction navigates
**Description**: Navigate to detail  
**Test Steps**:
1. Click interaction
**Expected Result**: Interaction view

### TC-CVE-018: Add interaction button
**Description**: Quick add  
**Test Steps**:
1. Click add
**Expected Result**: Add dialog

### TC-CVE-019: Log call quick action
**Description**: Quick call log  
**Test Steps**:
1. Click log call
**Expected Result**: Call form

### TC-CVE-020: Log email quick action
**Description**: Quick email log  
**Test Steps**:
1. Click log email
**Expected Result**: Email form

---

### Documents Panel Tests

### TC-CVE-021: Documents count shown
**Description**: Panel header count  
**Test Steps**:
1. Load with documents
**Expected Result**: Count shown

### TC-CVE-022: Document item display
**Description**: Name, type, date  
**Test Steps**:
1. Verify document row
**Expected Result**: Info displayed

### TC-CVE-023: Download document
**Description**: Quick download  
**Test Steps**:
1. Click download icon
**Expected Result**: Download starts

### TC-CVE-024: Upload document button
**Description**: Quick upload  
**Test Steps**:
1. Click upload
**Expected Result**: Upload dialog

### TC-CVE-025: Document preview
**Description**: Inline preview  
**Test Steps**:
1. Click preview
**Expected Result**: Preview opens

### TC-CVE-026: Document type icon
**Description**: File type indicator  
**Test Steps**:
1. Verify icons
**Expected Result**: Correct icons

---

### Communication Panel Tests (New)

### TC-CVE-027: Communication panel present
**Description**: New panel  
**Test Steps**:
1. Verify panel exists
**Expected Result**: Panel shown

### TC-CVE-028: Email activity shown
**Description**: Email history  
**Test Steps**:
1. Load with emails
**Expected Result**: Emails listed

### TC-CVE-029: Call activity shown
**Description**: Call history  
**Test Steps**:
1. Load with calls
**Expected Result**: Calls listed

### TC-CVE-030: Meeting activity shown
**Description**: Meeting history  
**Test Steps**:
1. Load with meetings
**Expected Result**: Meetings listed

### TC-CVE-031: Timeline view
**Description**: Chronological  
**Test Steps**:
1. Verify order
**Expected Result**: Most recent first

### TC-CVE-032: Filter by type
**Description**: Type filter  
**Test Steps**:
1. Filter by "Email"
**Expected Result**: Only emails shown

---

### Quick Actions Tests

### TC-CVE-033: Edit contact action
**Description**: Quick edit  
**Test Steps**:
1. Click edit
**Expected Result**: Edit mode/dialog

### TC-CVE-034: Email contact action
**Description**: Compose email  
**Test Steps**:
1. Click email
**Expected Result**: Email opens

### TC-CVE-035: Call contact action
**Description**: Initiate call  
**Test Steps**:
1. Click call
**Expected Result**: Phone opens

### TC-CVE-036: Delete contact action
**Description**: Delete with confirm  
**Test Steps**:
1. Click delete
**Expected Result**: Confirmation dialog

---

### Navigation Tests

### TC-CVE-037: Back to list
**Description**: Navigate back  
**Test Steps**:
1. Click back
**Expected Result**: Contact list

### TC-CVE-038: Breadcrumb navigation
**Description**: Click breadcrumb  
**Test Steps**:
1. Click breadcrumb item
**Expected Result**: Navigate

---

**Last Updated**: December 18, 2025

