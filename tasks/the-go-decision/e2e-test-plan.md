# The Go Decision Feature - E2E Test Plan

## Overview

This document provides comprehensive end-to-end test cases for validating "The Go Decision" feature. These tests should be executed in a staging/test environment with a fully deployed application.

## Prerequisites

Before executing these tests:
1. Application is deployed and running
2. Test user accounts are configured:
   - **User A**: Opportunity Manager (OM) role
   - **User B**: DoA2 (Decision Authority Level 2) role
   - **User C**: Director/Manager of the org unit
3. Test opportunity exists or can be created
4. Email service is configured and accessible
5. Database access is available for verification queries

---

## Test Suite 1: Go Decision Flow (10.1)

### TC-1.1: Submit Opportunity for Go Decision
**Precondition:** Opportunity exists in appropriate stage for submission

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User A (OM) | Dashboard displayed |
| 2 | Navigate to test opportunity | Opportunity detail view displayed |
| 3 | Click workflow action to submit for Go decision | Workflow dialog appears |
| 4 | Complete submission with acknowledgment | Success message shown |
| 5 | Verify stage changes to "SEND FOR GO DECISION" | Stage indicator updated |

### TC-1.2: Verify Email Notification to DoA2
**Precondition:** TC-1.1 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check DoA2 user's email inbox | Email received from system |
| 2 | Verify email subject | Contains opportunity name |
| 3 | Verify email body | Contains opportunity details, link to view |
| 4 | Verify CC recipients | Includes OM, initiator (if different), Director |
| 5 | Verify all recipients receive identical content | Content matches across recipients |

### TC-1.3: Verify In-System Notification
**Precondition:** TC-1.1 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User B (DoA2) | Dashboard displayed |
| 2 | Check notification bell in topbar | Unread notification count > 0 |
| 3 | Click notification bell | Notification dropdown appears |
| 4 | Locate workflow approval notification | Shows check-circle icon, blue color |
| 5 | Verify notification text | Shows "Go Decision Required: [Opportunity Name]" or similar |

### TC-1.4: Verify Actions Required Card
**Precondition:** TC-1.1 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User B (DoA2) | Dashboard displayed |
| 2 | Check Actions Required card | Workflow Approvals section visible |
| 3 | Locate the pending approval | Shows opportunity name, org unit, submitter, date |
| 4 | Verify "Review" badge displayed | Blue badge with "Review" text |
| 5 | Verify pulsing indicator | Blue dot animating |

### TC-1.5: Navigate to Opportunity from Notification
**Precondition:** TC-1.3 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click notification in dropdown | Navigation initiated |
| 2 | Wait for page load | Opportunity detail view displayed |
| 3 | Verify correct opportunity loaded | Opportunity name matches |

### TC-1.6: Verify Instructional Guidance
**Precondition:** Logged in as DoA2, viewing pending opportunity

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View opportunity page | Info message displayed at top |
| 2 | Verify message title | "Action Required: Go/No-Go Decision" |
| 3 | Verify message content | Instructions for reviewing and deciding |
| 4 | Verify info message styling | Blue informational styling |

### TC-1.7: Verify Decision Info Panel
**Precondition:** Logged in as DoA2, viewing pending opportunity

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate decision info panel | Panel visible below guidance |
| 2 | Verify Initiative Type displayed | Matches opportunity data |
| 3 | Verify Responsible Org Unit displayed | Matches opportunity data |
| 4 | Verify Proposed Budget displayed | Shows formatted currency |
| 5 | Verify Time to Signing displayed | Shows days with color indicator |
| 6 | Verify Submitter Remarks (if present) | Shows remarks from submission |
| 7 | Verify DD Concerns (if any) | Shows partner DD issues |
| 8 | Verify High Risks (if any) | Shows risk items |

### TC-1.8: Complete Go Decision
**Precondition:** Logged in as DoA2, viewing pending opportunity

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Approve" workflow action | Go Decision dialog opens |
| 2 | Verify dialog title | "Confirm Go Decision" |
| 3 | Verify confirmation statement | Contains org unit code and initiative type |
| 4 | Verify Executive dropdown | Pre-populated with suggested Director/Manager |
| 5 | Check the confirmation checkbox | Checkbox checked |
| 6 | Enter decision rationale | Text entered in textarea |
| 7 | Select Executive (if not pre-selected) | Executive selected |
| 8 | Click "Confirm Go Decision" | Loading spinner appears |
| 9 | Wait for completion | Success toast displayed |
| 10 | Verify dialog closes | Dialog no longer visible |

### TC-1.9: Verify Post-Go Decision State
**Precondition:** TC-1.8 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Verify stage changes to "GO" | Stage indicator shows GO |
| 2 | Refresh page | Page reloads |
| 3 | Verify Executive field populated | Shows selected Executive |
| 4 | Verify instructional guidance hidden | Info message no longer displayed |
| 5 | Verify decision info panel hidden | Panel no longer displayed |
| 6 | Check notification bell | Notification marked as done/read |
| 7 | Check Actions Required card | Approval task removed |

### TC-1.10: Verify Immutability After Go Decision
**Precondition:** TC-1.9 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to edit opportunity | Edit button disabled/hidden OR click shows error |
| 2 | Verify field controls | All fields read-only |
| 3 | Verify workflow actions | Limited actions available (no edit-related) |

---

## Test Suite 2: No-Go Decision Flow (10.2)

### TC-2.1: Submit Opportunity for Go Decision
**Precondition:** Fresh opportunity in appropriate stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User A (OM) | Dashboard displayed |
| 2 | Navigate to test opportunity | Opportunity detail view displayed |
| 3 | Submit for Go decision | Stage changes to "SEND FOR GO DECISION" |

### TC-2.2: Navigate to Opportunity as DoA2
**Precondition:** TC-2.1 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User B (DoA2) | Dashboard displayed |
| 2 | Navigate to pending opportunity | Opportunity detail view displayed |
| 3 | Verify instructional guidance displayed | Info message visible |

### TC-2.3: Complete No-Go Decision
**Precondition:** TC-2.2 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Reject" workflow action | No-Go Decision dialog opens |
| 2 | Verify dialog title | "Confirm No-Go Decision" |
| 3 | Verify warning message displayed | Red warning about consequences |
| 4 | Verify confirmation statement | Describes No-Go implications |
| 5 | Check the confirmation checkbox | Checkbox checked |
| 6 | Enter decision rationale | Text entered in textarea |
| 7 | Click "Confirm No-Go Decision" | Loading spinner appears |
| 8 | Wait for completion | Success toast displayed |

### TC-2.4: Verify Post-No-Go Decision State
**Precondition:** TC-2.3 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Verify stage changes to "NO GO" | Stage indicator shows NO GO |
| 2 | Verify notification marked as done | Notification bell updated |
| 3 | Verify record is immutable | Edit controls disabled |

---

## Test Suite 3: Reopen Flow (10.3)

### TC-3.1: Reopen After No-Go Decision
**Precondition:** Opportunity is in NO GO stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User A (OM) | Dashboard displayed |
| 2 | Navigate to NO GO opportunity | Opportunity detail view displayed |
| 3 | Locate "Reopen" workflow action | Action available in workflow panel |
| 4 | Click "Reopen" | Confirmation dialog appears |
| 5 | Complete reopen action | Success message displayed |

### TC-3.2: Verify Post-Reopen State
**Precondition:** TC-3.1 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Verify stage changes to "IDENTIFY & PROFILE" | Stage indicator updated |
| 2 | Verify record is editable | Edit button enabled |
| 3 | Attempt to edit a field | Field accepts input |
| 4 | Save changes | Changes saved successfully |

### TC-3.3: Verify Permissions After Reopen
**Precondition:** TC-3.2 completed successfully

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check permissions endpoint | `GET /api/opportunity/{id}/permissions` |
| 2 | Verify canUpdate | Returns `true` |
| 3 | Verify isImmutable | Returns `false` |

---

## Test Suite 4: Immutability Enforcement (10.4)

### TC-4.1: UI Immutability After Go Decision
**Precondition:** Opportunity is in GO stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as User A (OM) | Dashboard displayed |
| 2 | Navigate to GO opportunity | Opportunity detail view displayed |
| 3 | Locate edit controls | Edit button disabled or hidden |
| 4 | Attempt to click any editable field | Fields are read-only |
| 5 | Verify Add Document button | Disabled or hidden |

### TC-4.2: API Immutability - Edit Opportunity
**Precondition:** Opportunity is in GO stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Obtain valid auth token | Token available |
| 2 | Send PUT request to edit opportunity | `PUT /api/opportunity/{id}` |
| 3 | Include valid payload | JSON body with changes |
| 4 | Verify response status | 400 Bad Request |
| 5 | Verify error message | Contains immutability error |

**Sample Request:**
```bash
curl -X PUT "https://api/opportunity/{id}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name": "Updated Name"}'
```

**Expected Response:**
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "This opportunity is in an immutable state and cannot be modified."
}
```

### TC-4.3: API Immutability - Add Document
**Precondition:** Opportunity is in GO stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Obtain valid auth token | Token available |
| 2 | Send POST request to add document | `POST /api/opportunity/{id}/documents` |
| 3 | Include valid document payload | Form data with file |
| 4 | Verify response status | 400 Bad Request |
| 5 | Verify error message | Contains immutability error |

---

## Test Suite 5: Email CC Recipients (10.5)

### TC-5.1: Verify Email Recipients
**Precondition:** Opportunity submitted for Go decision

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Check DoA2 inbox | Email received |
| 2 | Check OM inbox | Email received in CC |
| 3 | Check Director inbox | Email received in CC |
| 4 | If initiator ≠ OM, check initiator inbox | Email received in CC |

### TC-5.2: Verify Email Content Consistency
**Precondition:** TC-5.1 completed, emails collected

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Compare email body from DoA2 inbox | Record body content |
| 2 | Compare with OM CC email | Content identical |
| 3 | Compare with Director CC email | Content identical |
| 4 | Compare with initiator CC email (if applicable) | Content identical |

---

## Test Suite 6: Regression Testing (10.6)

### TC-6.1: Submit Workflow Still Works
**Precondition:** Opportunity in IDENTIFY & PROFILE stage

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to draft opportunity | Opportunity displayed |
| 2 | Complete all required fields | Fields populated |
| 3 | Click Submit workflow action | Submit dialog appears |
| 4 | Complete submission | Success, stage changes |

### TC-6.2: Recall Workflow Still Works
**Precondition:** Opportunity submitted (in workflow)

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to submitted opportunity | Opportunity displayed |
| 2 | Locate Recall action | Action available |
| 3 | Click Recall | Recall dialog appears |
| 4 | Complete recall | Success, stage reverts |

### TC-6.3: Cancel Workflow Still Works
**Precondition:** Opportunity exists, user has cancel permission

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to opportunity | Opportunity displayed |
| 2 | Locate Cancel action | Action available |
| 3 | Click Cancel | Cancel dialog appears |
| 4 | Enter mandatory reason | Reason entered |
| 5 | Complete cancellation | Success, stage changes to CANCELLED |

### TC-6.4: Existing Notifications Unaffected
**Precondition:** System has existing notification types

| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Trigger a collaboration notification | Notification created |
| 2 | Check notification bell | Shows collaboration icon |
| 3 | Click notification | Navigates to correct entity |
| 4 | Trigger an import notification | Notification created |
| 5 | Verify import notification display | Shows correct icon, opens import dialog |

---

## Test Execution Checklist

| Test Suite | Test Case | Status | Notes |
|------------|-----------|--------|-------|
| 1. Go Decision | TC-1.1 Submit | ☐ | |
| 1. Go Decision | TC-1.2 Email | ☐ | |
| 1. Go Decision | TC-1.3 In-System Notification | ☐ | |
| 1. Go Decision | TC-1.4 Actions Required Card | ☐ | |
| 1. Go Decision | TC-1.5 Navigate from Notification | ☐ | |
| 1. Go Decision | TC-1.6 Instructional Guidance | ☐ | |
| 1. Go Decision | TC-1.7 Decision Info Panel | ☐ | |
| 1. Go Decision | TC-1.8 Complete Go Decision | ☐ | |
| 1. Go Decision | TC-1.9 Post-Go State | ☐ | |
| 1. Go Decision | TC-1.10 Immutability After Go | ☐ | |
| 2. No-Go Decision | TC-2.1 Submit | ☐ | |
| 2. No-Go Decision | TC-2.2 Navigate as DoA2 | ☐ | |
| 2. No-Go Decision | TC-2.3 Complete No-Go | ☐ | |
| 2. No-Go Decision | TC-2.4 Post-No-Go State | ☐ | |
| 3. Reopen Flow | TC-3.1 Reopen After No-Go | ☐ | |
| 3. Reopen Flow | TC-3.2 Post-Reopen State | ☐ | |
| 3. Reopen Flow | TC-3.3 Permissions After Reopen | ☐ | |
| 4. Immutability | TC-4.1 UI Immutability | ☐ | |
| 4. Immutability | TC-4.2 API Edit Immutability | ☐ | |
| 4. Immutability | TC-4.3 API Add Document | ☐ | |
| 5. Email CC | TC-5.1 Verify Recipients | ☐ | |
| 5. Email CC | TC-5.2 Content Consistency | ☐ | |
| 6. Regression | TC-6.1 Submit Works | ☐ | |
| 6. Regression | TC-6.2 Recall Works | ☐ | |
| 6. Regression | TC-6.3 Cancel Works | ☐ | |
| 6. Regression | TC-6.4 Existing Notifications | ☐ | |

---

## Bug Report Template

Use this template to document any bugs found during testing:

```
### Bug ID: [e.g., GO-BUG-001]

**Test Case:** [e.g., TC-1.8]
**Severity:** [Critical/High/Medium/Low]
**Status:** [Open/In Progress/Resolved]

**Description:**
[Clear description of the bug]

**Steps to Reproduce:**
1. [Step 1]
2. [Step 2]
3. [Step 3]

**Expected Result:**
[What should happen]

**Actual Result:**
[What actually happened]

**Environment:**
- Browser: 
- User Role: 
- Opportunity ID: 

**Screenshots/Logs:**
[Attach relevant evidence]

**Notes:**
[Any additional context]
```

---

## Test Completion Criteria

All tests are considered complete when:
1. All test cases executed with Pass/Fail status recorded
2. All Critical and High severity bugs resolved
3. Medium/Low bugs documented for future sprints
4. Regression tests confirm existing features work
5. Automated test suite passes (build verification)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-02 | AI Assistant | Initial test plan creation |
