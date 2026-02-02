# Send Opportunity for Go Decision - PRD-Aligned Test Cases

**PRD Reference:** Product Requirements Document: Send Opportunity for Go Decision  
**Component:** Opportunity Workflow - Go Decision Submission & Approval  
**Priority:** P0-P2 (Critical to Medium)  
**Created:** February 2, 2026  
**Last Updated:** February 2, 2026  
**Test Count:** 102 test cases  
**Coverage:** 12 User Stories, 18 Functional Requirements, Additional Acceptance Criteria

---

## Overview

These test cases are aligned with the PRD requirements and additional acceptance criteria. They cover PAO-specific implementation details including:

- **DoA Level 2 approver lookup** from EntityUserRole
- **Mandatory field validation** (20+ fields including UNCooperation Framework, Beneficiaries, High Risk)
- **Roles & permissions** (OM, Collaborator, role transfer)
- **Custom rejection handling** (Rejection → NO GO, not previous stage)
- **CANCELLED stage** with cancel/reopen workflows
- **OIC notifications** and DoA pathway display
- **Visibility rules** (read-only in workflow, inactive OM handling)
- **Exact email wording** and acknowledgment statements

---

## Test Categories

| # | Category | Test Count | Priority |
|---|----------|------------|----------|
| 1 | DoA Level 2 Approver Lookup (FR-1) | 6 | P0 |
| 2 | Mandatory Field Validation (FR-2, US-3) | 12 | P0 |
| 3 | Non-OM Submitter Warning (FR-6, US-2) | 4 | P1 |
| 4 | Country-Org Unit Warning (FR-7, US-4) | 5 | P1 |
| 5 | OM Recall Capability (FR-8, US-9) | 5 | P0 |
| 6 | Opportunity Statement Regeneration (FR-5) | 3 | P1 |
| 7 | Email Notifications (FR-9, FR-10, US-5) | 6 | P1 |
| 8 | Custom Rejection → NO GO (FR-14, US-7) | 5 | P0 |
| 9 | Reopen from NO GO (FR-15, US-8) | 4 | P1 |
| 10 | Cancel Opportunity (FR-17, US-11) | 5 | P0 |
| 11 | Reopen from CANCELLED (FR-18, US-12) | 4 | P1 |
| 12 | Stage Stepper Display (FR-16) | 4 | P2 |
| 13 | Acknowledgment Statement (FR-13) | 3 | P1 |
| 14 | Internal Stakeholder Notifications (FR-11) | 4 | P1 |
| 15 | Workflow View & Status (US-10) | 3 | P2 |
| 16 | **Roles, Responsibilities & Permissions** | **9** | P0-P1 |
| 17 | **Visibility & Workflow Lock** | **5** | P0-P1 |
| 18 | **Additional Mandatory Validations** | **5** | P0-P1 |
| 19 | **DoA Pathway Display** | **2** | P1-P2 |
| 20 | **OIC Notifications** | **2** | P1 |
| 21 | **Cancellation/Archiving Restrictions** | **2** | P0 |
| 22 | **Email Notification Content** | **3** | P1 |
| 23 | **Additional Remarks Field** | **1** | P2 |
| | **TOTAL** | **102** | - |

---

## 1. DoA Level 2 Approver Lookup (FR-1)

### TC-GO-DOA-001: DoA2 Holder Found for Responsible Org Unit
**Priority:** P0  
**User Story:** US-1  
**Precondition:** Opportunity has ResponsibleOrgUnitId = 100  
**Test Data:** EntityUserRole exists with EntityType="OrganizationHierarchy", EntityId=100, EntityRole.Code="DoA2_OrganizationHierarchy"

**Test Steps:**
1. Navigate to opportunity in IDENTIFY & PROFILE stage
2. Click "Send Opportunity for Go Decision"
3. Complete all mandatory fields
4. Submit for approval

**Expected Results:**
- Submission succeeds
- DoA2 holder(s) are identified from EntityUserRole table
- DoA2 holder(s) receive email notification
- Workflow shows correct approver(s) in Approvers tab

---

### TC-GO-DOA-002: No DoA2 Holder Found - Submission Blocked
**Priority:** P0  
**User Story:** US-1, US-3  
**Precondition:** Opportunity has ResponsibleOrgUnitId = 999 (org unit with no DoA2 assigned)

**Test Steps:**
1. Navigate to opportunity in IDENTIFY & PROFILE stage
2. Complete all mandatory fields except DoA2 is not available
3. Attempt to submit for Go decision

**Expected Results:**
- Submission is BLOCKED
- Requirements panel shows: "✗ DoA Level 2 holder exists for responsible org unit"
- Error message: "No DoA Level 2 holder found for the responsible org unit"
- Submit button remains disabled

---

### TC-GO-DOA-003: Multiple DoA2 Holders for Org Unit
**Priority:** P1  
**User Story:** US-1  
**Precondition:** Two DoA2 holders assigned to same org unit

**Test Steps:**
1. Navigate to opportunity with org unit having 2 DoA2 holders
2. Submit for Go decision

**Expected Results:**
- Both DoA2 holders receive email notification
- Either DoA2 holder can approve/reject
- Approvers tab shows both users with "DoA Level 2" role
- Only one approval needed (not both)

---

### TC-GO-DOA-004: DoA2 Lookup Uses Correct Entity Type
**Priority:** P0  
**Test Steps:**
1. Create EntityUserRole with EntityType="Partner" and DoA2 role (incorrect)
2. Attempt to submit opportunity for that org unit

**Expected Results:**
- System does NOT find the DoA2 from Partner entity type
- Only searches EntityType="OrganizationHierarchy"
- Submission blocked with "No DoA Level 2 holder found"

---

### TC-GO-DOA-005: Inactive DoA2 User Not Selected
**Priority:** P1  
**Test Steps:**
1. Set DoA2 user status to Inactive
2. Submit opportunity for their org unit

**Expected Results:**
- Inactive user NOT included in approver list
- If no other active DoA2 exists, submission blocked

---

### TC-GO-DOA-006: DoA2 Role with Correct Code
**Priority:** P0  
**Test Steps:**
1. Create EntityRole with Code="DoA2" (missing "_OrganizationHierarchy")
2. Assign user to org unit with this role
3. Submit opportunity

**Expected Results:**
- User NOT recognized as valid DoA2 approver
- Only Code="DoA2_OrganizationHierarchy" is valid
- Submission blocked if no correct role exists

---

## 2. Mandatory Field Validation (FR-2, US-3)

### TC-GO-VAL-001: All 18+ Required Fields Validated
**Priority:** P0  
**Test Steps:**
1. Create new opportunity with minimal data
2. Navigate to Stage Requirements panel
3. Verify each mandatory field is checked

**Expected Results:**
All fields validated with ✓/✗ indicators:
- [ ] Opportunity Name (required)
- [ ] Description (required)
- [ ] Proposed Budget for Initiative (required)
- [ ] Context & Challenges (required)
- [ ] UNOPS Strategic Mission(s) (minLength=1)
- [ ] Expected Impact (required)
- [ ] Expected Outcomes (required)
- [ ] SDG Alignment (minLength=1)
- [ ] Funding Partner with amount/currency (minLength=1)
- [ ] Client Partner (minLength=1)
- [ ] Products & Services (minLength=1)
- [ ] Countries of Implementation (minLength=1)
- [ ] Target Signing Date (required)
- [ ] Implementation Start Date (required)
- [ ] Implementation End Date (required)
- [ ] Opportunity Manager (role required)
- [ ] Responsible Org Unit (required)
- [ ] Proposed Initiative Type (required)
- [ ] DoA Level 2 holder (server-side only)
- [ ] Opportunity Statement generated (required)

---

### TC-GO-VAL-002: Real-Time Validation Updates
**Priority:** P1  
**Test Steps:**
1. View opportunity with 5 missing fields (✗ shown)
2. Populate one missing field (e.g., add SDG)
3. Observe requirements panel

**Expected Results:**
- ✗ changes to ✓ immediately without page refresh
- Counter updates: "4 of 20 requirements incomplete"
- Submit button enables when all requirements met

---

### TC-GO-VAL-003: Funding Partner Requires Amount AND Currency
**Priority:** P0  
**Test Steps:**
1. Add Funding Partner with name only
2. Check requirements panel

**Expected Results:**
- Requirement NOT met
- Must have: Partner name + Amount + Currency
- Error: "At least one funding partner with amount and currency required"

---

### TC-GO-VAL-004: Opportunity Manager Role Validated
**Priority:** P0  
**Test Steps:**
1. Create opportunity without Opportunity Manager stakeholder
2. Check requirements

**Expected Results:**
- ✗ Opportunity Manager
- Error: "Opportunity Manager role must be assigned"

---

### TC-GO-VAL-005: Opportunity Statement Must Be Generated
**Priority:** P0  
**Test Steps:**
1. Create opportunity with all fields except OpportunityStatementMarkdown is null
2. Check requirements

**Expected Results:**
- ✗ Opportunity Statement
- Error: "Please generate Opportunity Statement"
- Cannot submit until statement exists

---

### TC-GO-VAL-006: DoA2 Server-Side Validation
**Priority:** P0  
**Test Steps:**
1. Complete all client-visible requirements (all show ✓)
2. Responsible org unit has no DoA2 assigned
3. Attempt submission

**Expected Results:**
- Client shows all ✓ (DoA2 check not visible client-side)
- Server rejects with: "No DoA Level 2 holder found for the responsible org unit"
- User sees validation error popup

---

### TC-GO-VAL-007: Minimum Array Length Validation
**Priority:** P1  
**Test Steps:**
1. Add 0 SDGs
2. Add 1 SDG
3. Observe requirements

**Expected Results:**
- 0 SDGs: ✗ SDG Alignment
- 1 SDG: ✓ SDG Alignment
- minLength=1 enforced

---

### TC-GO-VAL-008: Date Fields Required
**Priority:** P1  
**Test Steps:**
1. Leave targetSigningDate, implementationStartDate, targetDeliveryDate null
2. Check requirements

**Expected Results:**
- ✗ Target Signing Date
- ✗ Implementation Start Date
- ✗ Implementation End Date
- All three required

---

### TC-GO-VAL-009: Select Fields Required
**Priority:** P1  
**Test Steps:**
1. Leave responsibleOrgUnitId and proposedInitiativeTypeId null
2. Check requirements

**Expected Results:**
- ✗ Responsible Org Unit
- ✗ Proposed Initiative Type

---

### TC-GO-VAL-010: Text Fields Required
**Priority:** P1  
**Test Steps:**
1. Leave name, description, challenges, expectedImpact, expectedOutcomes empty
2. Check requirements

**Expected Results:**
- All show ✗
- Must have non-empty string values

---

### TC-GO-VAL-011: Budget Must Be Greater Than Zero
**Priority:** P1  
**Test Steps:**
1. Set initiativeBudgetUSD = 0
2. Set initiativeBudgetUSD = -100
3. Set initiativeBudgetUSD = 1000000

**Expected Results:**
- 0: ✗ (may be treated as not set)
- -100: ✗ Invalid
- 1000000: ✓ Valid

---

### TC-GO-VAL-012: Requirements Panel Shows Count
**Priority:** P2  
**Test Steps:**
1. View opportunity with 8 missing fields

**Expected Results:**
- Panel shows: "[8 of 20 requirements incomplete]"
- Updates as fields are populated

---

## 3. Non-OM Submitter Warning (FR-6, US-2)

### TC-GO-NONOM-001: Non-OM User Sees Warning
**Priority:** P1  
**User Story:** US-2  
**Precondition:** Current user is Internal Stakeholder, NOT Opportunity Manager

**Test Steps:**
1. Log in as non-OM user with workflow trigger permission
2. Navigate to opportunity they're stakeholder on
3. Click "Send Opportunity for Go Decision"

**Expected Results:**
- Warning dialog appears BEFORE requirements check
- Text: "You currently hold a [Internal Stakeholder] role for this Opportunity. It is normally expected that the UNOPS personnel listed as the Opportunity Manager will perform the action of sending the Opportunity for a Go decision. Please confirm that you wish to proceed."
- Buttons: [Cancel] and [Proceed]

---

### TC-GO-NONOM-002: Non-OM User Cancels
**Priority:** P1  
**Test Steps:**
1. Trigger non-OM warning dialog
2. Click [Cancel]

**Expected Results:**
- Dialog closes
- Submission NOT initiated
- Opportunity remains in IDENTIFY & PROFILE

---

### TC-GO-NONOM-003: Non-OM User Proceeds
**Priority:** P1  
**Test Steps:**
1. Trigger non-OM warning dialog
2. Click [Proceed]

**Expected Results:**
- Dialog closes
- Submission flow continues (requirements check, country warning, acknowledgment)
- ConfirmedNonOMSubmission = true sent to backend

---

### TC-GO-NONOM-004: OM User Sees No Warning
**Priority:** P1  
**Precondition:** Current user IS the Opportunity Manager

**Test Steps:**
1. Log in as OM for the opportunity
2. Click "Send Opportunity for Go Decision"

**Expected Results:**
- No warning dialog appears
- Proceeds directly to requirements check / submission flow

---

## 4. Country-Org Unit Relationship Warning (FR-7, US-4)

### TC-GO-ORGCOUNTRY-001: Mismatch Detected
**Priority:** P1  
**User Story:** US-4  
**Test Data:**
- Opportunity: ResponsibleOrgUnit = AFRO, Countries = [South Sudan, Nepal, Bangladesh]
- OrganizationUnitRelationship: AFRO → South Sudan exists
- Nepal, Bangladesh NOT related to AFRO

**Test Steps:**
1. Submit opportunity for Go decision
2. Pass requirements validation

**Expected Results:**
- Warning dialog appears
- Text: "The org unit selected (AFRO - Africa Regional Office) is not normally responsible for the following countries of implementation: Nepal, Bangladesh"
- Unrelated countries listed
- Buttons: [Cancel] and [Proceed]

---

### TC-GO-ORGCOUNTRY-002: All Countries Match - No Warning
**Priority:** P1  
**Test Data:**
- Opportunity: ResponsibleOrgUnit = AFRO, Countries = [South Sudan, Kenya]
- OrganizationUnitRelationship: AFRO → South Sudan, AFRO → Kenya both exist

**Test Steps:**
1. Submit opportunity

**Expected Results:**
- No country-org unit warning dialog
- Proceeds to acknowledgment dialog

---

### TC-GO-ORGCOUNTRY-003: User Cancels Mismatch Warning
**Priority:** P1  
**Test Steps:**
1. Trigger country mismatch warning
2. Click [Cancel]

**Expected Results:**
- Submission NOT initiated
- User can change org unit or countries

---

### TC-GO-ORGCOUNTRY-004: User Proceeds Despite Mismatch
**Priority:** P1  
**Test Steps:**
1. Trigger country mismatch warning
2. Click [Proceed]

**Expected Results:**
- ConfirmedOrgUnitMismatch = true sent
- Submission continues
- Internal stakeholders from normally responsible org units will be notified on Go decision

---

### TC-GO-ORGCOUNTRY-005: Query Uses Correct Relationship Table
**Priority:** P0  
**Test Steps:**
1. Verify OrganizationUnitRelationship query uses:
   - EntityType = "Country"
   - OrganizationHierarchyId = ResponsibleOrgUnitId
   - IsDeleted = false

**Expected Results:**
- Only active country relationships checked
- Deleted relationships ignored

---

## 5. OM Recall Capability (FR-8, US-9)

### TC-GO-RECALL-001: OM Can Recall Even If Not Submitter
**Priority:** P0  
**User Story:** US-9  
**Precondition:**
- User A (Internal Stakeholder) submitted opportunity
- User B is Opportunity Manager

**Test Steps:**
1. Log in as User B (OM)
2. Navigate to opportunity in workflow
3. Click [Recall]
4. Enter mandatory justification

**Expected Results:**
- Recall succeeds
- Workflow cancelled
- Stage remains IDENTIFY & PROFILE
- Opportunity unlocked for editing
- DoA holder(s) notified of recall

---

### TC-GO-RECALL-002: Submitter Can Recall
**Priority:** P0  
**Test Steps:**
1. Log in as original submitter
2. Recall their own submission

**Expected Results:**
- Recall succeeds
- Same behavior as OM recall

---

### TC-GO-RECALL-003: Mandatory Justification Required
**Priority:** P0  
**Test Steps:**
1. Initiate recall
2. Leave justification empty
3. Click [Recall]

**Expected Results:**
- Error: "Justification is required when recalling"
- Recall blocked until justification provided

---

### TC-GO-RECALL-004: Non-OM Non-Submitter Cannot Recall
**Priority:** P1  
**Test Steps:**
1. Log in as Internal Stakeholder who didn't submit
2. View opportunity in workflow

**Expected Results:**
- [Recall] button NOT visible
- Or if visible, returns 403 Forbidden

---

### TC-GO-RECALL-005: Recall Logged in Workflow History
**Priority:** P1  
**Test Steps:**
1. Complete recall with justification "Need to update budget figures"

**Expected Results:**
- Workflow history shows:
  - Action: "Recall"
  - User: [User name]
  - Comment: "Need to update budget figures"
  - Timestamp

---

## 6. Opportunity Statement Regeneration (FR-5)

### TC-GO-STMT-001: Statement Regenerated on Submit
**Priority:** P1  
**Test Steps:**
1. Generate Opportunity Statement
2. Update opportunity description
3. Submit for Go decision

**Expected Results:**
- Statement regenerated with new description
- Email link goes to #statement section
- Statement reflects current data

---

### TC-GO-STMT-002: Email Link Scrolls to Statement
**Priority:** P1  
**Test Steps:**
1. Click link in DoA notification email

**Expected Results:**
- Opens opportunity page
- Auto-scrolls to Opportunity Statement section
- URL contains #statement anchor

---

### TC-GO-STMT-003: No PDF Generated
**Priority:** P2  
**Test Steps:**
1. Submit for Go decision

**Expected Results:**
- No PDF attachment in email
- Only web link provided
- Markdown statement viewable in browser

---

## 7. Email Notifications (FR-9, FR-10, US-5)

### TC-GO-EMAIL-001: Approval Request Email Content
**Priority:** P1  
**User Story:** US-5  
**Test Steps:**
1. Submit opportunity for Go decision
2. Verify DoA2 holder receives email

**Expected Results:**
- Subject: "PAO: [Opportunity Name] - Action Required"
- Body contains:
  - Greeting with DoA holder name(s)
  - "You are the current DoA Level 2 holder(s) for [Org Unit ID & Description]"
  - Opportunity name with link
  - Submitter name
  - Link to statement section
  - Note about Internal Stakeholder notifications

---

### TC-GO-EMAIL-002: Approval Email Sent to All DoA2 Holders
**Priority:** P1  
**Test Data:** 2 DoA2 holders for org unit

**Test Steps:**
1. Submit opportunity

**Expected Results:**
- Both DoA2 holders receive email
- Each email personalized with recipient name

---

### TC-GO-EMAIL-003: Rejection Email (NO GO)
**Priority:** P1  
**Test Steps:**
1. DoA2 rejects opportunity with reason "Not aligned with strategy"

**Expected Results:**
- Email to submitter
- Subject: "PAO: [Opportunity Name] - Set to NO GO"
- Contains rejection reason
- Explains OM can reopen if circumstances change

---

### TC-GO-EMAIL-004: Recall Email
**Priority:** P1  
**Test Steps:**
1. OM recalls submission with justification

**Expected Results:**
- Email to DoA2 holder(s)
- Subject: "PAO: [Opportunity Name] - Submission Recalled"
- Contains recall justification

---

### TC-GO-EMAIL-005: Approval Email (GO)
**Priority:** P1  
**Test Steps:**
1. DoA2 approves opportunity

**Expected Results:**
- Email to submitter
- Subject: "PAO: [Opportunity Name] - Go Decision Approved"
- Confirms authorization to proceed

---

### TC-GO-EMAIL-006: Additional Remarks Included in Email
**Priority:** P2  
**Test Steps:**
1. Submit with additional remarks "Please prioritize this urgent request"

**Expected Results:**
- DoA email includes remarks section
- "Additional Remarks from Submitter: Please prioritize this urgent request"

---

## 8. Custom Rejection → NO GO (FR-14, US-7)

### TC-GO-REJECT-001: Rejection Sets Stage to NO GO (Not Previous)
**Priority:** P0  
**User Story:** US-7  
**Important:** This is CUSTOM behavior - standard workflow returns to previous stage

**Test Steps:**
1. Submit opportunity from IDENTIFY & PROFILE
2. DoA2 clicks [Reject]
3. Enters mandatory reason
4. Confirms rejection

**Expected Results:**
- Stage changes to "NO GO" (NOT back to IDENTIFY & PROFILE)
- Opportunity unlocked for editing
- WorkflowStatus = None
- Status remains Active

---

### TC-GO-REJECT-002: Rejection Requires Comment
**Priority:** P0  
**Test Steps:**
1. DoA2 clicks [Reject]
2. Leave comment empty
3. Attempt to confirm

**Expected Results:**
- Error: "Rejection reason is required"
- Rejection blocked

---

### TC-GO-REJECT-003: Rejection Confirmation Dialog
**Priority:** P1  
**Test Steps:**
1. DoA2 clicks [Reject]

**Expected Results:**
- Confirmation dialog appears
- Text: "Rejecting this opportunity will set its stage to NO GO. The Opportunity Manager can reopen it later if circumstances change. Are you sure you want to proceed?"

---

### TC-GO-REJECT-004: Workflow History Shows NO GO Outcome
**Priority:** P1  
**Test Steps:**
1. Complete rejection

**Expected Results:**
- Workflow history shows:
  - Action: "Rejected → NO GO"
  - From Stage: "IDENTIFY & PROFILE"
  - To Stage: "NO GO"
  - Reason: [DoA comment]

---

### TC-GO-REJECT-005: Submitter Notified of NO GO
**Priority:** P1  
**Test Steps:**
1. DoA2 rejects with reason

**Expected Results:**
- Submitter receives email
- Clearly states opportunity is NO GO
- Includes rejection reason
- Notes OM can reopen

---

## 9. Reopen from NO GO (FR-15, US-8)

### TC-GO-REOPEN-NOGO-001: OM Can Reopen NO GO Opportunity
**Priority:** P1  
**User Story:** US-8  
**Precondition:** Opportunity in NO GO stage

**Test Steps:**
1. Log in as Opportunity Manager
2. Navigate to NO GO opportunity
3. Click [Reopen]

**Expected Results:**
- Stage changes to IDENTIFY & PROFILE
- Opportunity fully editable
- Can make updates and re-submit
- No approval required for reopen

---

### TC-GO-REOPEN-NOGO-002: Non-OM Cannot Reopen
**Priority:** P1  
**Test Steps:**
1. Log in as non-OM user
2. View NO GO opportunity

**Expected Results:**
- [Reopen] button NOT visible
- Or returns 403 if attempted

---

### TC-GO-REOPEN-NOGO-003: Workflow History Shows Reopen
**Priority:** P2  
**Test Steps:**
1. OM reopens NO GO opportunity

**Expected Results:**
- History shows:
  - Action: "Reopen"
  - From Stage: "NO GO"
  - To Stage: "IDENTIFY & PROFILE"

---

### TC-GO-REOPEN-NOGO-004: Can Re-Submit After Reopen
**Priority:** P1  
**Test Steps:**
1. Reopen NO GO opportunity
2. Make updates
3. Submit for Go decision again

**Expected Results:**
- Second submission works
- DoA holders notified again
- Full workflow repeats

---

## 10. Cancel Opportunity (FR-17, US-11)

### TC-GO-CANCEL-001: OM Can Cancel from IDENTIFY & PROFILE
**Priority:** P0  
**User Story:** US-11  
**Test Steps:**
1. Log in as OM
2. Navigate to IDENTIFY & PROFILE opportunity
3. Click [Cancel]
4. Enter mandatory justification
5. Confirm

**Expected Results:**
- Stage changes to CANCELLED
- Status changes to Closed
- Opportunity becomes read-only
- WorkflowStatus = None
- No approval required

---

### TC-GO-CANCEL-002: Non-OM Cannot Cancel
**Priority:** P0  
**Test Steps:**
1. Log in as non-OM user
2. View IDENTIFY & PROFILE opportunity

**Expected Results:**
- [Cancel] button NOT visible to non-OM

---

### TC-GO-CANCEL-003: Cannot Cancel from Other Stages
**Priority:** P0  
**Test Steps:**
1. Opportunity in GO stage
2. Attempt to cancel

**Expected Results:**
- Error: "Can only cancel opportunities in IDENTIFY & PROFILE stage"
- Or [Cancel] button not shown for GO/NO GO stages

---

### TC-GO-CANCEL-004: Mandatory Justification for Cancel
**Priority:** P0  
**Test Steps:**
1. Click [Cancel]
2. Leave justification empty
3. Attempt confirm

**Expected Results:**
- Error: "Justification is required for cancellation"

---

### TC-GO-CANCEL-005: Workflow History Shows Cancellation
**Priority:** P1  
**Test Steps:**
1. Cancel with justification "Funding partner withdrew"

**Expected Results:**
- History shows:
  - Action: "Cancel"
  - From Stage: "IDENTIFY & PROFILE"
  - To Stage: "CANCELLED"
  - Reason: "Funding partner withdrew"

---

## 11. Reopen from CANCELLED (FR-18, US-12)

### TC-GO-REOPEN-CANCEL-001: OM Can Reopen Cancelled Opportunity
**Priority:** P1  
**User Story:** US-12  
**Precondition:** Opportunity in CANCELLED stage

**Test Steps:**
1. Log in as OM
2. Click [Reopen]
3. Enter mandatory reason

**Expected Results:**
- Stage changes to IDENTIFY & PROFILE
- Status changes to Active
- Opportunity fully editable

---

### TC-GO-REOPEN-CANCEL-002: Mandatory Reason for Reopen
**Priority:** P1  
**Test Steps:**
1. Attempt reopen from CANCELLED without reason

**Expected Results:**
- Error: "Reason is required when reopening a cancelled opportunity"

---

### TC-GO-REOPEN-CANCEL-003: Non-OM Cannot Reopen Cancelled
**Priority:** P1  
**Test Steps:**
1. Log in as non-OM
2. View CANCELLED opportunity

**Expected Results:**
- [Reopen] button NOT visible

---

### TC-GO-REOPEN-CANCEL-004: Workflow History Shows Reopen
**Priority:** P2  
**Test Steps:**
1. Reopen with reason "New funding secured"

**Expected Results:**
- History shows:
  - Action: "Reopen"
  - From Stage: "CANCELLED"
  - To Stage: "IDENTIFY & PROFILE"
  - Reason: "New funding secured"

---

## 12. Stage Stepper Display (FR-16)

### TC-GO-STEPPER-001: Happy Path Shows Only IDENTIFY & GO
**Priority:** P2  
**Precondition:** Opportunity in IDENTIFY & PROFILE or GO stage

**Test Steps:**
1. View stage stepper component

**Expected Results:**
- Shows only: IDENTIFY & PROFILE → GO
- NO GO and CANCELLED NOT displayed

---

### TC-GO-STEPPER-002: NO GO Path When in NO GO
**Priority:** P2  
**Precondition:** Opportunity in NO GO stage

**Test Steps:**
1. View stage stepper

**Expected Results:**
- Shows: IDENTIFY & PROFILE → NO GO
- GO stage hidden (it was skipped)

---

### TC-GO-STEPPER-003: CANCELLED Path When Cancelled
**Priority:** P2  
**Precondition:** Opportunity in CANCELLED stage

**Test Steps:**
1. View stage stepper

**Expected Results:**
- Shows: IDENTIFY & PROFILE → CANCELLED
- GO stage hidden

---

### TC-GO-STEPPER-004: Stepper Updates After Stage Change
**Priority:** P2  
**Test Steps:**
1. View opportunity in IDENTIFY & PROFILE (stepper shows happy path)
2. DoA rejects (stage becomes NO GO)
3. Observe stepper

**Expected Results:**
- Stepper updates to show NO GO path
- Reflects actual stage reached

---

## 13. Acknowledgment Statement (FR-13)

### TC-GO-ACK-001: Acknowledgment Checkbox Required
**Priority:** P1  
**Test Steps:**
1. Pass all requirements and warnings
2. Acknowledgment dialog appears
3. Attempt to submit without checking acknowledgment

**Expected Results:**
- [Submit] button disabled
- Must check acknowledgment checkbox first

---

### TC-GO-ACK-002: Acknowledgment Text Correct
**Priority:** P1  
**Test Steps:**
1. View acknowledgment dialog

**Expected Results:**
- Text: "All known information and materials relevant to this Opportunity have been provided and are summarized in the Opportunity Statement for your review. Please confirm whether UNOPS org unit [Org Unit ID & Name] is authorised to assign resources to continue development based on this information"

---

### TC-GO-ACK-003: Optional Additional Remarks
**Priority:** P2  
**Test Steps:**
1. Enter additional remarks in text field
2. Submit

**Expected Results:**
- Remarks included in DoA notification email
- Stored in workflow history/log

---

## 14. Internal Stakeholder Notifications (FR-11)

### TC-GO-INTERNAL-001: Other Org Units Notified on GO
**Priority:** P1  
**Test Data:**
- Opportunity countries: South Sudan, Nepal
- South Sudan → AFRO (responsible)
- Nepal → APRO (NOT responsible but normally responsible for Nepal)

**Test Steps:**
1. DoA2 approves opportunity

**Expected Results:**
- Internal stakeholders from APRO notified
- AFRO not notified (already the responsible org unit)

---

### TC-GO-INTERNAL-002: No Notification on NO GO
**Priority:** P1  
**Test Steps:**
1. DoA2 rejects opportunity (NO GO)

**Expected Results:**
- Internal stakeholders from other org units NOT notified
- Only submitter notified

---

### TC-GO-INTERNAL-003: No Duplicate Org Unit Notifications
**Priority:** P2  
**Test Data:** All countries belong to responsible org unit

**Test Steps:**
1. Approve opportunity

**Expected Results:**
- No redundant notifications to responsible org unit

---

### TC-GO-INTERNAL-004: Position Titles in Notifications
**Priority:** P2  
**Test Steps:**
1. View approvers/stakeholders in emails or UI

**Expected Results:**
- Personnel position titles displayed from UserProfile.PositionTitle

---

## 15. Workflow View & Status (US-10)

### TC-GO-VIEW-001: Approval Pending Tag Visible
**Priority:** P2  
**User Story:** US-10  
**Precondition:** Opportunity in workflow

**Test Steps:**
1. View opportunity detail page

**Expected Results:**
- "Approval Pending" tag visible
- Current stage and pending stage displayed

---

### TC-GO-VIEW-002: Opportunity Read-Only in Workflow
**Priority:** P0  
**Test Steps:**
1. Navigate to opportunity in workflow
2. Attempt to edit any field

**Expected Results:**
- All content is read-only
- Edit buttons disabled or hidden
- Clear indication opportunity is locked

---

### TC-GO-VIEW-003: Approvers Tab Shows DoA Holders
**Priority:** P1  
**Test Steps:**
1. View Approvers tab on opportunity in workflow

**Expected Results:**
- DoA holder(s) listed with names
- Role: "DoA Level 2"
- Position titles from UserProfile

---

## 16. Roles, Responsibilities & Permissions (Additional AC)

### TC-GO-ROLE-001: Opportunity Manager Field Can Never Be Blank
**Priority:** P0  
**Acceptance Criteria:** "The Opportunity Manager (OM) is the primary caretaker responsible for the record and its contents. This field can never be left blank."

**Test Steps:**
1. Create new opportunity
2. Attempt to save without assigning Opportunity Manager
3. Attempt to remove existing OM without assigning replacement

**Expected Results:**
- Cannot save opportunity without OM assigned
- Validation error: "Opportunity Manager is required"
- Cannot remove last OM without assigning replacement

---

### TC-GO-ROLE-002: Collaborator Can Edit All Opportunity Content
**Priority:** P0  
**Acceptance Criteria:** "A user with a Collaborator role may edit all content of the Opportunity record."

**Test Steps:**
1. Log in as Collaborator (not OM) for an opportunity
2. Navigate to opportunity in IDENTIFY & PROFILE (not in workflow)
3. Edit various fields (name, description, partners, etc.)
4. Save changes

**Expected Results:**
- All fields editable by Collaborator
- Changes saved successfully
- Same editing capabilities as OM

---

### TC-GO-ROLE-003: OM Can Transfer Role to Another User
**Priority:** P0  
**Acceptance Criteria:** "An opportunity manager (OM) can assign another active UNOPS personnel to be listed as the opportunity manager and in doing so, their own role and permissions should transfer to Collaborator for the Opportunity."

**Test Steps:**
1. Log in as current OM (User A)
2. Navigate to stakeholders section
3. Assign User B as new Opportunity Manager
4. Save changes
5. Verify User A's role

**Expected Results:**
- User B becomes Opportunity Manager
- User A automatically becomes Collaborator
- User A retains edit access (as Collaborator)
- Only one OM at a time

---

### TC-GO-ROLE-004: Collaborator Can Initiate Submission
**Priority:** P0  
**Acceptance Criteria:** "The Opportunity Manager (OM) or any personnel with Collaborator role can initiate the 'Send Opportunity for Go Decision' process."

**Test Steps:**
1. Log in as Collaborator
2. Navigate to opportunity with all requirements met
3. Click "Send Opportunity for Go Decision"

**Expected Results:**
- Collaborator CAN initiate submission
- Warning dialog appears (see TC-GO-ROLE-005)
- Submission proceeds if confirmed

---

### TC-GO-ROLE-005: Collaborator Sees Specific Warning Text
**Priority:** P0  
**Acceptance Criteria:** Specific warning text for Collaborator role

**Test Steps:**
1. Log in as Collaborator
2. Initiate submission

**Expected Results:**
- Warning text EXACTLY: "You currently hold a Collaborator role for this Opportunity. It is normally expected that the UNOPS personnel listed as the Opportunity Manager will perform the action of sending the Opportunity for a Go decision. Please confirm that you wish to proceed."

---

### TC-GO-ROLE-006: Standardized Position Titles Displayed
**Priority:** P1  
**Acceptance Criteria:** "The system must display the standardized position titles for all personnel on the opportunity - the OM's and Collaborators', the personnel holding Organisational roles and DoAs, drawn from their personnel records."

**Test Steps:**
1. View opportunity stakeholders section
2. View DoA holders in workflow
3. View approvers tab

**Expected Results:**
- Position titles shown for: OM, Collaborators, DoA holders
- Titles pulled from personnel records (UserProfile.PositionTitle)
- Consistent display across all views

---

### TC-GO-ROLE-007: Original Decision Makers Visible in Workflow History
**Priority:** P1  
**Acceptance Criteria:** "The personnel listed as Decision Makers as at the time the Opportunity was successfully Send Opportunity for Go Decision (and who received email notifications) should be visible in the workflow"

**Test Steps:**
1. Submit opportunity (DoA2 = User X)
2. User X changed to User Y for the org unit
3. View workflow history

**Expected Results:**
- Workflow history shows User X as original approver
- Email notification log shows User X received notification

---

### TC-GO-ROLE-008: New DoA Holder Can Approve After Personnel Change
**Priority:** P0  
**Acceptance Criteria:** "if the personnel holding those roles subsequently has changed the new role holder(s) must be able to perform the approval action."

**Test Steps:**
1. Submit opportunity (DoA2 = User X)
2. Admin changes DoA2 for org unit to User Y
3. User Y logs in
4. Navigate to pending opportunity

**Expected Results:**
- User Y (new DoA2) CAN approve/reject
- User X (former DoA2) cannot approve
- System recognizes current role holder

---

### TC-GO-ROLE-009: Decision Maker is Lowest Level DoA Starting at 2
**Priority:** P0  
**Acceptance Criteria:** "The Decision Maker is the personnel identified as the first (lowest level, starting at 2) DoA holder(s) related to the responsible Org Unit."

**Test Steps:**
1. Org unit has DoA2, DoA3, DoA4 holders assigned
2. Submit opportunity

**Expected Results:**
- Only DoA2 (Level 2) identified as Decision Maker
- DoA3, DoA4 NOT notified for approval
- Lowest level (2) is first approver

---

## 17. Visibility & Workflow Lock (Additional AC)

### TC-GO-VIS-001: Read-Only for Both OM and Collaborators in Workflow
**Priority:** P0  
**Acceptance Criteria:** "Once the record is successfully Send Opportunity for Go Decision, all content including the Opportunity Statement must become read-only for the OM and Collaborators."

**Test Steps:**
1. Submit opportunity for Go decision
2. Log in as OM, attempt to edit
3. Log in as Collaborator, attempt to edit

**Expected Results:**
- BOTH OM and Collaborator see read-only content
- Opportunity Statement also read-only
- No edit buttons visible for either role

---

### TC-GO-VIS-002: Workflow Status Clearly Visible and Locked
**Priority:** P0  
**Acceptance Criteria:** "When the record is in workflow, this must be clearly visible and it is locked for editing."

**Test Steps:**
1. View opportunity in workflow from list page
2. View opportunity detail page

**Expected Results:**
- Visual indicator: "In Workflow" or "Approval Pending"
- Lock icon or similar indicator
- Status visible from opportunity card AND detail page

---

### TC-GO-VIS-003: Workflow History Shows Recalls and Remarks
**Priority:** P1  
**Acceptance Criteria:** "Workflow history including recalls and associated remarks should be visible."

**Test Steps:**
1. Submit opportunity
2. Recall with reason "Need to update budget"
3. Re-submit
4. View workflow history

**Expected Results:**
- History shows all actions: Submit, Recall, Re-submit
- Recall entry includes remarks/reason
- Complete audit trail visible

---

### TC-GO-VIS-004: Inactive OM Visible on Active Opportunity
**Priority:** P0  
**Acceptance Criteria:** "If the user listed as Opportunity Manager is no longer an active personnel, for an active Opportunity in Identify & Profile stage this must be visible and on editing the record this must be populated to be able to save it"

**Test Steps:**
1. Set OM user to Inactive status
2. View opportunity in IDENTIFY & PROFILE
3. Attempt to edit and save

**Expected Results:**
- Visual indication that OM is inactive (warning icon, strikethrough, or message)
- Cannot save without assigning new active OM
- Error: "Current Opportunity Manager is inactive. Please assign an active user."

---

### TC-GO-VIS-005: In-Workflow Indicator on Opportunity Card
**Priority:** P1  
**Acceptance Criteria:** "The system must provide on-screen confirmation of a successful submission and clearly indicate across the application (including the opportunity card from the main Opportunities screen) that the record is 'in workflow'."

**Test Steps:**
1. Submit opportunity successfully
2. Navigate to main Opportunities list
3. Find the opportunity card

**Expected Results:**
- Success message on submission
- Opportunity card shows "In Workflow" badge/indicator
- Visible from list view without opening detail

---

## 18. Additional Mandatory Field Validations (Additional AC)

### TC-GO-ADDVAL-001: UNCooperation Framework Outcome Required
**Priority:** P0  
**Acceptance Criteria:** "UNCooperation Framework Outcome(s) - selection of at least one outcome from any UNSDCFs which exist for the country/ies of implementation"

**Test Steps:**
1. Create opportunity with countries that have UNSDCFs
2. Do not select any UNCooperation Framework Outcome
3. Check requirements panel

**Expected Results:**
- ✗ UNCooperation Framework Outcome(s)
- Must select at least one outcome from relevant UNSDCFs
- Blocked from submission

---

### TC-GO-ADDVAL-002: Beneficiaries Required OR Acknowledgement
**Priority:** P0  
**Acceptance Criteria:** "Estimated Direct Beneficiaries and Estimated Indirect Beneficiaries OR have indicated that these will be sought if further development pursued"

**Test Steps:**
1. Leave both direct and indirect beneficiaries blank
2. Do not check acknowledgement
3. Check requirements

**Expected Results:**
- ✗ Estimated Beneficiaries
- Either: populate both fields, OR check acknowledgement box
- Error: "Enter estimated beneficiaries or indicate these will be sought later"

---

### TC-GO-ADDVAL-003: Beneficiaries Acknowledgement Option
**Priority:** P1  
**Test Steps:**
1. Leave beneficiaries blank
2. Check "Will be sought if further development pursued" option
3. Check requirements

**Expected Results:**
- ✓ Requirement met with acknowledgement
- Can proceed to submission

---

### TC-GO-ADDVAL-004: High Risk Acknowledgement Required
**Priority:** P0  
**Acceptance Criteria:** "High Risk Acknowledgement"

**Test Steps:**
1. Complete all fields except High Risk Acknowledgement
2. Check requirements

**Expected Results:**
- ✗ High Risk Acknowledgement
- Must acknowledge high risks before submission

---

### TC-GO-ADDVAL-005: Opportunity Statement Warning Before Submission
**Priority:** P0  
**Acceptance Criteria:** "On initiating the submission to Send Opportunity for Go Decision a warning should be triggered if the user has not yet generated an Opportunity Statement"

**Test Steps:**
1. Complete all fields except Statement not generated
2. Click "Send Opportunity for Go Decision"

**Expected Results:**
- Warning: "An Opportunity Statement has not yet been generated for this Opportunity. Please generate and review the Opportunity Statement before proceeding."
- User must generate statement before continuing

---

## 19. DoA Pathway Display (Additional AC)

### TC-GO-PATHWAY-001: DoA2 and DoA3 Pathway Displayed Read-Only
**Priority:** P1  
**Acceptance Criteria:** "The system should display the Opportunity Decision-Making Pathway as read-only, showing the current DOA 2 and DOA 3 holders for the responsible unit."

**Test Steps:**
1. View opportunity (in or out of workflow)
2. Find Decision-Making Pathway section

**Expected Results:**
- Shows current DoA2 holder(s) for responsible org unit
- Shows current DoA3 holder(s) for responsible org unit
- Displayed as read-only (informational)
- Position titles included

---

### TC-GO-PATHWAY-002: Pathway Updates When DoA Holders Change
**Priority:** P2  
**Test Steps:**
1. View pathway (shows User X as DoA2)
2. Admin changes DoA2 to User Y
3. Refresh opportunity page

**Expected Results:**
- Pathway now shows User Y as DoA2
- Reflects current role holders

---

## 20. OIC (Officer-in-Charge) Notifications (Additional AC)

### TC-GO-OIC-001: OIC Notified on Submission
**Priority:** P1  
**Acceptance Criteria:** "The system should send a notification to the identified Decision Maker(s) and any Officer-in-Charge (OIC) alerting them to the required action after successful submission"

**Test Steps:**
1. Org unit has DoA2 and OIC assigned
2. Submit opportunity

**Expected Results:**
- DoA2 holder receives notification
- OIC also receives notification
- Both alerted to required action

---

### TC-GO-OIC-002: OIC Can Act on Behalf of DoA
**Priority:** P1  
**Test Steps:**
1. Submit opportunity
2. Log in as OIC
3. Attempt to approve/reject

**Expected Results:**
- OIC can perform approval actions
- Authority delegated from DoA holder
- Workflow history shows OIC performed action

---

## 21. Cancellation/Archiving Restrictions (Additional AC)

### TC-GO-ARCHIVE-001: Cannot Cancel While In Workflow
**Priority:** P0  
**Acceptance Criteria:** "Cancellation or Archiving is only permitted when the record is in the 'Identify and Profile' stage (not in workflow), and the user must provide a reason for not pursuing the opportunity"

**Test Steps:**
1. Submit opportunity (now in workflow)
2. Attempt to cancel/archive

**Expected Results:**
- [Cancel] button NOT available while in workflow
- Must recall from workflow first before cancelling
- Error if attempted: "Cannot cancel opportunity while in workflow"

---

### TC-GO-ARCHIVE-002: Reason Required for Cancellation
**Priority:** P0  
**Test Steps:**
1. Navigate to IDENTIFY & PROFILE opportunity (not in workflow)
2. Click [Cancel]
3. Leave reason blank

**Expected Results:**
- Error: "Please provide a reason for not pursuing this opportunity"
- Free text field for reason
- Cannot proceed without reason

---

## 22. Email Notification Content (Additional AC)

### TC-GO-EMAIL-CONTENT-001: Exact Email Wording Verified
**Priority:** P1  
**Acceptance Criteria:** Exact notification text provided

**Test Steps:**
1. Submit opportunity
2. Verify DoA email content

**Expected Results:**
Email contains:
- "Dear [names],"
- "You are the current DoA[x] holder for [org unit ID & Description]."
- "Opportunity [Opportunity name] has been submitted by [user submitting opportunity] for your review and to request confirmation that [org unit ID & Description] may proceed with further development."
- "Please review the Opportunity Statement carefully and indicate your decision [here] [Link to Opportunity+]."
- "Please note that, where applicable, Internal Stakeholders from any other UNOPS Org Units normally responsible for any of the countries of Implementation will be notified of any decision to continue development of this Opportunity."

---

### TC-GO-EMAIL-CONTENT-002: Email Contains Org Unit ID AND Description
**Priority:** P1  
**Test Steps:**
1. Submit opportunity with ResponsibleOrgUnit = "AFRO - Africa Regional Office"

**Expected Results:**
- Email includes full org unit identifier: "AFRO - Africa Regional Office"
- NOT just ID, includes description

---

### TC-GO-EMAIL-CONTENT-003: Email Link Goes to Opportunity+
**Priority:** P1  
**Test Steps:**
1. Click link in DoA notification email

**Expected Results:**
- Opens Opportunity+ application
- Navigates to the specific opportunity
- Scrolls to statement section

---

## 23. Additional Remarks Field (Additional AC)

### TC-GO-REMARKS-001: Explanatory Text for Remarks Field
**Priority:** P2  
**Acceptance Criteria:** "The system should allow the user to enter optional Additional remarks for the attention of the decision maker (free text), with the explanatory text 'Add any comments or remarks for the attention of the decision maker'."

**Test Steps:**
1. View acknowledgment dialog with remarks field

**Expected Results:**
- Explanatory text visible: "Add any comments or remarks for the attention of the decision maker"
- Field is optional (can be left blank)
- Free text input

---

---

## Summary

| Category | Test Count |
|----------|------------|
| DoA Level 2 Approver Lookup | 6 |
| Mandatory Field Validation | 12 |
| Non-OM Submitter Warning | 4 |
| Country-Org Unit Warning | 5 |
| OM Recall Capability | 5 |
| Opportunity Statement Regeneration | 3 |
| Email Notifications | 6 |
| Custom Rejection → NO GO | 5 |
| Reopen from NO GO | 4 |
| Cancel Opportunity | 5 |
| Reopen from CANCELLED | 4 |
| Stage Stepper Display | 4 |
| Acknowledgment Statement | 3 |
| Internal Stakeholder Notifications | 4 |
| Workflow View & Status | 3 |
| **Roles, Responsibilities & Permissions** | **9** |
| **Visibility & Workflow Lock** | **5** |
| **Additional Mandatory Validations** | **5** |
| **DoA Pathway Display** | **2** |
| **OIC Notifications** | **2** |
| **Cancellation/Archiving Restrictions** | **2** |
| **Email Notification Content** | **3** |
| **Additional Remarks Field** | **1** |
| **TOTAL** | **102** |

**Priority Distribution:**
- P0 (Critical): 41 tests
- P1 (High): 46 tests
- P2 (Medium): 15 tests

---

**Last Updated:** February 2, 2026  
**Status:** ✅ Ready for Implementation  
**PRD Alignment:** Complete with Additional Acceptance Criteria
