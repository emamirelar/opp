# Team Section Refinements Test Cases

## Overview
Test cases for the Team Section within Opportunities in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-979  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 39

---

## Test Case Summary

| Category | Count | Priority |
|----------|-------|----------|
| Positive Tests | 28 | High/Normal |
| Negative Tests | 8 | High |
| Boundary/Logic Tests | 3 | High |
| **TOTAL** | **39** | |

---

## 1. Team Section Layout and UI Tests

### POS_001 - Verify Team Section is Repositioned as the Last Section
**Priority:** Normal  
**JIRA ID:** PNO-1018  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify that the "Team" tab is visually located at the very end of the Opportunity workflow tabs, specifically positioned immediately after the "Statement" tab.

**Preconditions:**
- User is logged in
- An Opportunity record exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to an existing Opportunity | Opportunity detail page loads |
| 2 | View the tab navigation | Team tab is the last tab |
| 3 | Verify order: Statement → Team | Team is positioned after Statement |

---

### POS_002 - Verify Team Section Contains Exactly Three Specific Subsections
**Priority:** Normal  
**JIRA ID:** PNO-1019  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify the Team section structure has been updated to show only the three required subsections.

**Preconditions:**
- User is logged in

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Team section loads |
| 2 | Verify subsections present | Three subsections visible |
| 3 | Confirm subsection names | "Opportunity Development Team", "Other Internal Stakeholders", "Opportunity decision making pathway" |

---

## 2. Opportunity Manager Tests

### NEG_003 - Verify Save Action Cannot Proceed if Mandatory Opportunity Manager Field is Empty
**Priority:** High  
**JIRA ID:** PNO-1020  
**Labels:** Mandatory, Opportunity_Management, Team_Section

**Objective:** Verify that if the mandatory "Opportunity Manager" field is not populated, the system cannot save the changes and must trigger a validation error.

**Preconditions:**
- User is logged in as an Editor
- Opportunity is in Draft

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Team section loads |
| 2 | Clear the Opportunity Manager field | Field is empty |
| 3 | Click Save | Validation error displayed |
| 4 | Verify error message | "Opportunity Manager is required" |

---

### POS_004 - Verify Opportunity Manager Card Displays Standardized Position Title
**Priority:** Low  
**JIRA ID:** PNO-1021  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify that the Opportunity Manager's personnel card displays their Official Standardized Position Title alongside their name.

**Preconditions:**
- Assigned OM has a valid personnel record

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Team section loads |
| 2 | View Opportunity Manager card | Card displays name and title |
| 3 | Verify title matches personnel record | Standardized Position Title shown |

---

## 3. Collaborator Tests

### POS_005 - Verify Ability to Search and Add Active Personnel as Collaborators
**Priority:** High  
**JIRA ID:** PNO-1022  
**Labels:** Collaborators, Opportunity_Management, Team_Section

**Objective:** Verify that the user can search for any Active Personnel in the system and add them to the "Opportunity Collaborators" list.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add Collaborator" button | Search dialog opens |
| 2 | Search for active personnel | Results display active users |
| 3 | Select a user | User added to collaborators list |
| 4 | Save the opportunity | Collaborator saved successfully |

---

### NEG_006 - Verify Expertise Field is Mandatory When Adding Collaborator
**Priority:** High  
**JIRA ID:** PNO-1023  
**Labels:** Collaborators, Opportunity_Management, Team_Section

**Objective:** Verify that the "Expertise in" field is strictly mandatory; the system must not save if this field is empty.

**Preconditions:**
- User has added a draft Collaborator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add a new collaborator | Collaborator form appears |
| 2 | Leave "Expertise in" field empty | Field is empty |
| 3 | Attempt to save | Validation error displayed |
| 4 | Verify error message | "Expertise is required" |

---

### POS_007 - Verify Collaborator Expertise Dropdown Contains Specific Values
**Priority:** High  
**JIRA ID:** PNO-1024  
**Labels:** Collaborators, Opportunity_Management, Team_Section

**Objective:** Verify that the "Expertise in" dropdown displays exactly the 10 specific values defined in the Acceptance Criteria.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Expertise dropdown | Dropdown displays options |
| 2 | Verify all options present | Exactly 10 options listed |
| 3 | Confirm values match AC | Values match specification |

**Expected Values:**
1. Project Management
2. Technical Expertise
3. Financial Management
4. Legal
5. Procurement
6. Human Resources
7. Communications
8. Risk Management
9. Monitoring & Evaluation
10. Other

---

### POS_008 - Verify Expertise Dropdown Allows Multi-Selection
**Priority:** Normal  
**JIRA ID:** PNO-1025  
**Labels:** Collaborators, Opportunity_Management, Team_Section

**Objective:** Verify that a single Collaborator can be assigned multiple areas of expertise.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add a collaborator | Collaborator form appears |
| 2 | Open Expertise dropdown | Dropdown displays |
| 3 | Select multiple values | Multiple selections allowed |
| 4 | Save | Multiple expertise saved |

---

### POS_009 - Verify Collaborator Receives Edit Permissions
**Priority:** High  
**JIRA ID:** PNO-1026  
**Labels:** Opportunity_Management, Permissions, Team_Section

**Objective:** Verify that a user added as a "Collaborator" gains full Edit access to the Opportunity record.

**Preconditions:**
- User B is added as Collaborator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add User B as collaborator | User B added |
| 2 | Save the opportunity | Changes saved |
| 3 | Log in as User B | Login successful |
| 4 | Navigate to the Opportunity | Opportunity loads |
| 5 | Verify edit controls | Edit button is visible and enabled |

---

## 4. Responsible Org Unit Tests

### POS_010 - Verify Responsible Org Unit Search is Restricted to D&P Units
**Priority:** High  
**JIRA ID:** PNO-1027  
**Labels:** Opportunity_Management, Org_Unit, Team_Section

**Objective:** Verify that the "Responsible Organizational Unit" search filter strictly limits results to "Development and Partnerships" (D&P) related units.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Responsible Org Unit dropdown | Dropdown opens |
| 2 | Search for org units | Only D&P units appear |
| 3 | Verify non-D&P units excluded | No non-D&P results |

---

### POS_011 - Verify Org Unit Hierarchy Displays D&P Parent and Children
**Priority:** Normal  
**JIRA ID:** PNO-1028  
**Labels:** Opportunity_Management, Org_Unit, Team_Section

**Objective:** Verify the visual hierarchy in the Org Unit dropdown shows the "D&P" parent structure.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Org Unit dropdown | Dropdown displays |
| 2 | View hierarchy structure | Parent-child relationship visible |
| 3 | Verify D&P as root | D&P shown as parent |

---

### POS_012 - Verify Org Unit Type Auto-Populates Upon Selection
**Priority:** Normal  
**JIRA ID:** PNO-1029  
**Labels:** Fields, Opportunity_Management, Team_Section

**Objective:** Verify that the "Org Unit Type" field is automatically populated with the correct type based on the selected Org Unit.

**Preconditions:**
- No Org Unit selected initially

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a Responsible Org Unit | Org Unit selected |
| 2 | View Org Unit Type field | Field auto-populates |
| 3 | Verify type matches selected unit | Correct type displayed |

---

### POS_013 - Verify Org Unit Type Field Role Holders information is Read-Only
**Priority:** Normal  
**JIRA ID:** PNO-1030  
**Labels:** Fields, Opportunity_Management, Team_Section

**Objective:** Verify that the "Org Unit Type" field Role Holders information is strictly Read-Only.

**Preconditions:**
- Org Unit Type is populated

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View Org Unit Type field | Field displays |
| 2 | Attempt to edit | Field is read-only |
| 3 | Verify no edit controls | No input allowed |

---

## 5. Development Team Display Tests

### POS_014 - Verify Role Holders for Org Unit Display in Development Team Section
**Priority:** Normal  
**JIRA ID:** PNO-1031  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify that the "Role holders for responsible org unit" (Director, Manager) are displayed in the "Opportunity Development Team" subsection.

**Preconditions:**
- Org Unit is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a Responsible Org Unit | Org Unit selected |
| 2 | View Development Team subsection | Subsection displays |
| 3 | Verify role holders shown | Director/Manager cards visible |

---

## 6. Decision Making Pathway Tests

### POS_015 - Verify Decision Making Pathway Section Appears Dynamically
**Priority:** Normal  
**JIRA ID:** PNO-1032  
**Labels:** Fields, Opportunity_Management, Team_Section

**Objective:** Verify that the "Opportunity decision making pathway" section is hidden initially and becomes visible ONLY after a Responsible Org Unit is selected.

**Preconditions:**
- Org Unit field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View Team section with empty Org Unit | Pathway section hidden |
| 2 | Select a Responsible Org Unit | Pathway section appears |
| 3 | Clear Org Unit selection | Pathway section hides |

---

### POS_016 - Verify Decision Making Pathway Shows DoA 2 and DoA 3 Holders
**Priority:** Normal  
**JIRA ID:** PNO-1033  
**Labels:** Fields, Opportunity_Management, Team_Section

**Objective:** Verify that the Decision Making Pathway section correctly displays the personnel cards for DoA 2 and DoA 3 authorities.

**Preconditions:**
- Org Unit is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select Org Unit with DoA holders | Org Unit selected |
| 2 | View Decision Making Pathway | Section displays |
| 3 | Verify DoA 2 card visible | DoA 2 holder shown |
| 4 | Verify DoA 3 card visible | DoA 3 holder shown |

---

### NEG_017 - Verify DoA 1 Holders are Hidden from Pathway
**Priority:** Normal  
**JIRA ID:** PNO-1034  
**Labels:** Logic, Opportunity_Management, Team_Section

**Objective:** Verify that DoA 1 holders are filtered out and NOT displayed in the Opportunity Decision Making Pathway section.

**Preconditions:**
- Selected Org Unit is known to have a DoA 1 holder

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select Org Unit with DoA 1 holder | Org Unit selected |
| 2 | View Decision Making Pathway | Section displays |
| 3 | Verify DoA 1 is NOT shown | DoA 1 holder hidden |

---

## 7. Country/Org Unit Mismatch Tests

### B&L_018 - Verify Warning Popup Triggers on Country Mismatch
**Priority:** High  
**JIRA ID:** PNO-1035  
**Labels:** Boundary, Opportunity_Management, Team_Section

**Objective:** Verify that a Warning Popup is triggered if the user selects a Responsible Org Unit that is NOT normally responsible for the Implementation Country.

**Preconditions:**
- Opportunity Implementation Country is set (e.g., Country A)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set Implementation Country to Country A | Country set |
| 2 | Select Org Unit NOT responsible for Country A | Warning popup appears |
| 3 | Verify warning content | Message indicates mismatch |

---

### B&L_019 - Verify Normally Responsible Org Unit Auto-Populates on Mismatch
**Priority:** High  
**JIRA ID:** PNO-1036  
**Labels:** Logic, Opportunity_Management, Team_Section

**Objective:** Verify that when a mismatch occurs, the system automatically displays the *Normally* Responsible Org Unit as a read-only field.

**Preconditions:**
- User has triggered and acknowledged the Mismatch Warning

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Trigger country mismatch warning | Warning displayed |
| 2 | Acknowledge and proceed | Mismatch acknowledged |
| 3 | Verify "Normally Responsible Org Unit" field | Field auto-populates with correct unit |
| 4 | Verify field is read-only | Cannot modify field |

---

### B&L_020 - Verify Normal Role Holders Auto-Populate on Mismatch
**Priority:** High  
**JIRA ID:** PNO-1037  
**Labels:** Logic, Opportunity_Management, Team_Section

**Objective:** Verify that when a mismatch occurs, the Role Holders (Director/Manager) of the *Normally* Responsible Org Unit are automatically added as Internal Stakeholders.

**Preconditions:**
- Mismatch scenario active

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete mismatch acknowledgment | Mismatch processed |
| 2 | View Internal Stakeholders | Section displays |
| 3 | Verify role holders added | Director/Manager of normal Org Unit listed |

---

## 8. Internal Stakeholders Tests

### POS_021 - Verify Stakeholder Placeholder Text When Org Unit is Empty
**Priority:** Normal  
**JIRA ID:** PNO-1038  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify that informational placeholder text is displayed in the "Other Internal Stakeholders" section when no Responsible Org Unit is selected.

**Preconditions:**
- Responsible Org Unit field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View Team section with no Org Unit | Section displays |
| 2 | Check Internal Stakeholders section | Placeholder text visible |
| 3 | Verify placeholder content | Instructional text shown |

---

### POS_022 - Verify Functionality of Add Internal Stakeholder Button
**Priority:** Normal  
**JIRA ID:** PNO-1039  
**Labels:** Opportunity_Management, Stakeholders, Team_Section

**Objective:** Verify that the "Add Internal Stakeholder" button is available in the Stakeholders subsection and allows adding users.

**Preconditions:**
- User in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add Internal Stakeholder" | Dialog opens |
| 2 | Search for a user | Search results display |
| 3 | Select and add user | User added to stakeholders |
| 4 | Save changes | Stakeholder saved |

---

### NEG_023 - Verify Legacy Subject Matter Expertise Section is Removed
**Priority:** Normal  
**JIRA ID:** PNO-1040  
**Labels:** Legacy, Opportunity_Management, Team_Section

**Objective:** Verify that the "Subject Matter Expertise" section (legacy feature) is completely removed from the UI and no longer visible.

**Preconditions:**
- User views Team section

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Section loads |
| 2 | Search for "Subject Matter Expertise" | Section not found |
| 3 | Verify complete removal | No legacy section visible |

---

### POS_024 - Verify Standardized Position Titles Display on All Personnel Cards
**Priority:** Normal  
**JIRA ID:** PNO-1041  
**Labels:** Opportunity_Management, Team_Section, UI

**Objective:** Verify that ALL personnel cards (Stakeholders, Collaborators, Managers) consistently display the "Standardized Position Title".

**Preconditions:**
- Various users assigned

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View all personnel cards in Team section | Cards display |
| 2 | Verify each card shows position title | Title visible on all cards |
| 3 | Confirm consistency across card types | Same format used |

---

## 9. Workflow Validation Tests

### NEG_025 - Verify Workflow is Blocked if Mandatory Team Fields are Missing
**Priority:** High  
**JIRA ID:** PNO-1042  
**Labels:** Mandatory, Opportunity_Management, Team_Section

**Objective:** Verify that the "Send to Decision Maker" workflow action is blocked if mandatory Team fields are empty.

**Preconditions:**
- "Proposed Initiative Type" is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Clear mandatory Team fields | Fields empty |
| 2 | Attempt "Send to Decision Maker" | Action blocked |
| 3 | Verify validation error | Missing fields listed |

---

### POS_026 - Verify Warning Popup Cancellation Reverts Selection
**Priority:** Normal  
**JIRA ID:** PNO-1043  
**Labels:** Boundary, Opportunity_Management, Team_Section

**Objective:** Verify that clicking "Cancel" on the Country Mismatch Warning popup closes the popup and reverts the "Responsible Org Unit" selection to its previous state (or empty).

**Preconditions:**
- User is in Edit mode
- Implementation Country is set (e.g., Country A)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Note current Org Unit selection | Current value recorded |
| 2 | Select mismatched Org Unit | Warning popup appears |
| 3 | Click "Cancel" | Popup closes |
| 4 | Verify Org Unit reverted | Previous value restored |

---

### NEG_027 - Verify Send to Workflow Action Cannot Proceed if Mandatory Org Unit Responsible Field is Empty
**Priority:** High  
**JIRA ID:** PNO-1044  
**Labels:** Mandatory, Opportunity_Management, Team_Section

**Objective:** Verify that if the mandatory "Responsible Organizational Unit" field is empty, the system cannot do a "Send to Workflow" for Opportunity.

**Preconditions:**
- User is in Edit mode
- Org Unit field is cleared

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Clear Responsible Org Unit field | Field is empty |
| 2 | Complete all other mandatory fields | Other fields valid |
| 3 | Attempt "Send to Workflow" | Action blocked |
| 4 | Verify validation message | "Responsible Org Unit is required" |

---

### POS_028 - Verify Proposed Initiative Type Field Is Visible and Editable
**Priority:** Normal  
**JIRA ID:** PNO-1045  
**Labels:** Fields, Opportunity_Management, Team_Section

**Objective:** Verify the existence and editability of the "Proposed Initiative Type" field within the Team section.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Section loads |
| 2 | Locate "Proposed Initiative Type" field | Field visible |
| 3 | Click field to edit | Field is editable |
| 4 | Select a value and save | Value saved |

---

### NEG_029 - Verify Team Section Fields Cannot Be Modified by View Only Users
**Priority:** High  
**JIRA ID:** PNO-1046  
**Labels:** Opportunity_Management, Permissions, Team_Section

**Objective:** Verify that a user with "View Only" permissions cannot access Edit mode or modify any fields in the Team Section.

**Preconditions:**
- User is logged in as a Viewer (Not a Manager/Collaborator)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as View Only user | Login successful |
| 2 | Navigate to Team section | Section loads |
| 3 | Verify edit controls hidden | No edit buttons visible |
| 4 | Attempt to modify any field | Modification not possible |

---

## 10. Original Team Management Tests

### POS_030 - Validate Team Section UI Components
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that the Team Section displays all required UI components and fields.

**Preconditions:**
- User is Partner User
- Opportunity exists in Draft or Active state

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity | Opportunity detail page loads |
| 3 | Click on the "Team" section/tab | Team section is displayed |
| 4 | Verify UI components | All expected fields are visible |

---

### POS_031 - Add Team Member to Opportunity
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that a user can successfully add a team member to the Opportunity.

**Preconditions:**
- User has permission to modify Team
- Team Member to add exists in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity Team section | Team section loads |
| 2 | Click "Add Team Member" button | Member selection dialog opens |
| 3 | Search for and select a user | User is displayed in search results |
| 4 | Assign a role to the team member | Role is selected |
| 5 | Save the team member | Team member is added to the list |

---

### POS_032 - Remove Team Member from Opportunity
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that a user can successfully remove a team member from the Opportunity.

**Preconditions:**
- Team has at least one member besides the Team Lead

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity Team section | Team section loads |
| 2 | Locate a team member to remove | Team member visible |
| 3 | Click the remove/delete option | Confirmation dialog appears |
| 4 | Confirm removal | Team member removed |

---

### POS_033 - Change Team Member Role
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that a user can modify the role of an existing team member.

**Preconditions:**
- Team has at least one member with an assigned role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity Team section | Team section loads |
| 2 | Click edit on team member | Edit form opens |
| 3 | Change the role | New role selected |
| 4 | Save changes | Role updated |

---

### POS_034 - Designate Team Lead
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that a Team Lead can be designated for the Opportunity.

**Preconditions:**
- Team has multiple members

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunity Team section | Team section loads |
| 2 | Select different member as Team Lead | Option available |
| 3 | Confirm designation | New Team Lead assigned |
| 4 | Verify indicator updated | New Team Lead shows badge |

---

### NEG_035 - Validate Team Lead is Required
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team, Negative

**Objective:** Validate that an Opportunity cannot proceed without a designated Team Lead.

**Preconditions:**
- Opportunity is in Draft state
- Team Lead is not assigned

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section with no Team Lead | Section loads |
| 2 | Attempt to submit Opportunity for approval | System prevents submission |
| 3 | Verify validation error | "Team Lead is required" |

---

### NEG_036 - Validate Duplicate Team Member Prevention
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team, Negative

**Objective:** Validate that the same person cannot be added twice to the team.

**Preconditions:**
- Team has at least one member

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Team section loads |
| 2 | Click "Add Team Member" | Dialog opens |
| 3 | Attempt to add existing member | System prevents duplicate |
| 4 | Verify warning message | "Member already on team" |

---

### POS_037 - View Team Member Details
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team

**Objective:** Validate that a user can view details of a team member.

**Preconditions:**
- Team has at least one member

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Team section | Team section loads |
| 2 | Click on team member's details | Details displayed |
| 3 | Verify information | Name, email, role visible |

---

### POS_038 - Team Section Read-Only for Non-Authorized Users
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team, Permission

**Objective:** Validate that users without edit permissions can only view the team section in read-only mode.

**Preconditions:**
- User has view-only access to the Opportunity

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as view-only user | Login successful |
| 2 | Navigate to Team section | Section loads |
| 3 | Verify edit controls hidden | No Add/Edit/Remove buttons |

---

### POS_039 - Team Member Notification on Assignment
**Priority:** Low  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, Team, Notification

**Objective:** Validate that team members receive notification when added to an Opportunity.

**Preconditions:**
- Notification system is enabled
- User is added as a team member

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add new team member to Opportunity | Team member added |
| 2 | Log in as newly added team member | Login successful |
| 3 | Check notification inbox | Notification present |
| 4 | Verify notification content | Opportunity name and role shown |

---

## Summary

| Category | Count |
|----------|-------|
| Layout & UI | 2 |
| Opportunity Manager | 2 |
| Collaborators | 5 |
| Org Unit | 4 |
| Development Team | 1 |
| Decision Pathway | 3 |
| Country Mismatch | 3 |
| Internal Stakeholders | 4 |
| Workflow Validation | 5 |
| Original Team Tests | 10 |
| **TOTAL** | **39** |

---

**C# Test Class:** `TeamSectionTests.cs`  
**Playwright Test File:** `team-section.spec.ts`  
**Status:** ✅ Aligned with JIRA PNO-979
