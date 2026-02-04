# Opportunity Sections Test Cases

## Overview
Test cases for Opportunity section tabs: WHY, WHERE, WHAT, and WHO sections within the UNOPS Opportunity+ system.

**JIRA Stories:** PNO-692, PNO-697, PNO-700, PNO-6701  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 43

---

## 1. WHY Section - Impact & Strategic Alignment (PNO-692)

### POS_001 - Validate Structural Elements and Context Input
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify "WHY" section exists with "Context" subsection and accepts text.

**Preconditions:**
- User is Opportunity Manager
- Opportunity exists in Draft state

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to "Opportunities" list and click an existing Opportunity | Opportunity details page opens |
| 3 | Click the "WHY - Impact & Strategic Alignment" tab | WHY section is displayed |
| 4 | Verify "Context and challenge(s)" subsection | Subsection is labeled with prompt: "Describe challenge(s) that the initiative will address" |
| 5 | Enter text into Context field and Save | Text is saved successfully |

---

### POS_002 - Partner Results Framework Upload and Link
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify upload of Partner Results Framework and linking logic.

**Preconditions:**
- Opportunity has multiple Funding Partners

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to "Partner Results Framework" subsection | Option to upload or select document exists |
| 2 | Upload a document | System prompts to indicate which partner(s) this relates to |
| 3 | Select a specific partner and Save | Document is linked to the selected partner |
| 4 | Verify "No Partner Results Framework available" option | User can select this option to satisfy requirement without uploading |

---

### POS_003 - SDG Selection and Opt-Out Logic
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify Primary/Secondary SDG selection and target opt-out.

**Preconditions:**
- WHY section is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add SDG". Select "Goal 16" and mark as "Primary" | Goal 16 is added with "Primary" badge |
| 2 | Add another SDG (e.g., Goal 5) as "Secondary" | Goal 5 is added as "Secondary" |
| 3 | Expand Primary SDG card | Dropdowns for Targets/Indicators are visible |
| 4 | Select "I want to opt out..." | Target/Indicator selection is disabled/skipped |

---

### NEG_004 - Single Primary SDG Validation
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify only one Primary SDG is allowed.

**Preconditions:**
- One Primary SDG exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to add a second SDG with "Primary" status | System prevents selection or displays warning: "Only one main SDG allowed" |

---

### NEG_005 - UNCF Availability and Missing Country Alert
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify UNCF section behavior when Country is missing.

**Preconditions:**
- Country of Implementation is BLANK

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to "UN Cooperation Framework" subsection | Section is unavailable/locked |
| 2 | Verify alert message | Alert: "This section will only become available once countries... identified" |

---

### POS_006 - UNCF Outcome Selection and SDG Highlighting
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify active UNCF Outcomes and SDG alignment suggestions.

**Preconditions:**
- Country = "Somalia" (Active Framework exists)
- Primary SDG = Goal 16

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open UNCF subsection | Active Framework Outcomes for Somalia are listed |
| 2 | Verify highlighting | Outcomes aligning with Goal 16 are highlighted as "Compelling" |
| 3 | Select an Outcome and Save | Outcome is saved |
| 4 | (Variation) Remove SDG and check prompts | Prompt appears: "SDG alignment could help refine Outcome possibilities" |

---

### POS_007 - Inactive UNCF Handling
**Priority:** Low  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify handling of replaced/inactive frameworks.

**Preconditions:**
- Previously selected UNCF is now inactive

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View WHY section | System indicates the Framework is no longer active |
| 2 | Verify user options | User can "Change Outcome Alignment" or "Retain original selection" |

---

### POS_008 - Humanitarian and Strategy Alignment
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify Humanitarian and UNOPS Strategy selection/opt-out.

**Preconditions:**
- Country has active Humanitarian Framework

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Scroll to Humanitarian Framework | Active framework shown. User can Align or select "Not provided at this time" |
| 2 | Scroll to UNOPS Strategy | List of UNOPS missions (Climate, Digital, etc.) is displayed |
| 3 | Select "Just Digital Transformation" | Selection is saved |

---

### POS_009 - NDC and NAP First Release Stub
**Priority:** Low  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify "Not Found" message for NDCs and NAPs.

**Preconditions:**
- Country is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Scroll to NDC subsection | Text: "No NDC found... Alignment not currently possible" |
| 2 | Scroll to NAP subsection | Text: "No NAP found... Alignment not currently possible" |

---

### POS_010 - Org Unit Strategy Fallback
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify Strategy Fallback logic.

**Preconditions:**
- Country has NO local strategy, but Region has one

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Scroll to "Organization Unit Strategy" | System displays Regional Strategy (fallback from Country) |
| 2 | Select priority or "Alignment will not be provided" | Selection is saved |

---

### POS_011 - Beneficiaries Input and Breakdown
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify Direct/Indirect beneficiary input and optional breakdown.

**Preconditions:**
- User is on Beneficiaries section

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter Direct and Indirect Beneficiary counts | Positive integers accepted |
| 2 | Select option to provide disaggregated estimates | Fields for Gender/Age/Status appear |
| 3 | Enter breakdown details and additional text specifics | Breakdown is saved. Additional text is saved |

**Test Data:**
- Direct: 1000
- Indirect: 5000

---

### NEG_012 - Go Decision Blocking Validation
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify "Go Decision" is blocked if ANY section is incomplete.

**Preconditions:**
- SDGs selected
- Humanitarian & Strategy sections EMPTY (no "opt-out" selected)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to submit for "Go Decision" | System blocks submission. Error: Must respond to all subsections |
| 2 | Update Humanitarian to "Alignment not provided" | Selection Recorded |
| 3 | Update UNOPS Strategy to "Not Applicable" | Selection Recorded |
| 4 | Retry "Go Decision" | Submission allowed (Assuming Primary SDG and UNCF Outcome present) |

---

### NEG_013 - Validate Go Decision Blocked by Missing SDG
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify submission is blocked if no Primary SDG is selected.

**Preconditions:**
- Opportunity is in Draft
- Context and UNCF sections are valid
- NO SDG is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to WHY section and ensure "SDG Alignment" list is empty | List shows "No SDGs selected" |
| 2 | Attempt to submit Opportunity for "Go Decision" | System blocks action. Error/Alert: "Primary SDG is required" |

---

### NEG_014 - Validate Invalid Beneficiary Inputs
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify system rejects non-numeric or negative values in beneficiary fields.

**Preconditions:**
- User is on Beneficiaries subsection

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter a negative value in "Direct Beneficiaries" (e.g., -100) | System validates input: Field turns red or prevents typing |
| 2 | Enter a decimal value (e.g., 50.5) or text | System validates input: Only positive integers allowed |

---

### POS_015 - Validate Beneficiary Breakdown Calculation
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify disaggregated breakdown sums up to or updates the total direct beneficiaries.

**Preconditions:**
- Total Direct Beneficiaries = 100

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter "100" in Total Direct Beneficiaries field | Value accepted |
| 2 | Open disaggregation breakdown. Enter "60" for Female | "Male" or remaining fields may auto-calc, or validation waits for submit |
| 3 | Enter "30" for Male (Total = 90). Attempt to Save | System warns: Breakdown total (90) does not match Total (100) |
| 4 | Correct Male to "40". Save | System saves successfully |

---

### NEG_016 - Validate Go Decision Blocked by Missing Context
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify submission is blocked if Context is empty.

**Preconditions:**
- All other mandatory fields (SDG, UNCF) are filled
- Context field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to "Context and challenge(s)" and clear the text field | Field is empty |
| 2 | Attempt to submit Opportunity for "Go Decision" | System blocks action. Error highlights Context field |

---

### POS_017 - Missing Country Alerts for All Sections
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify missing country alerts for NDC, NAP, and Strategy sections.

**Preconditions:**
- Opportunity "Country of Implementation" is BLANK

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Scroll to NDC, NAP, and Org Unit Strategy sections | Verify all display "Missing Country" or "Section unavailable" alerts |
| 2 | Verify user cannot attempt alignment in these sections | Controls are disabled or hidden |

---

### POS_018 - Verify Read-Only Status of NDC/NAP Statements
**Priority:** Low  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify "Statement of Fact" text for NDC/NAP cannot be edited.

**Preconditions:**
- Country is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate the text "No NDC found..." | Text is visible |
| 2 | Attempt to click into or edit the text | Text is read-only/label, not an input field |

---

### POS_019 - Verify Retain Selection for Inactive Frameworks
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHY, Section

**Objective:** Verify user can save "Retain original selection" for replaced frameworks.

**Preconditions:**
- User views an Opportunity with an inactive UNCF Outcome selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Observe alert regarding Inactive Framework | Alert options: "Change" or "Retain" |
| 2 | Select "Retain original selection" and click Save | System saves the choice. Original Outcome remains selected |

---

## 2. WHERE Section - Geographic Implementation (PNO-697)

### POS_001 - Validate WHERE Section Existence and Placement
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, UI

**Objective:** Validate that the "WHERE - Geographic Implementation" section exists and is accessible.

**Preconditions:**
- User is Partner User
- Opportunity record exists and is editable

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity record's main tab | Opportunity detail page loads |
| 3 | Locate the "WHERE - Geographic Implementation" section/header | Section is visible and navigable |

---

### POS_002 - Validate Single Country Selection and Deletion
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, Selection

**Objective:** Validate that a user can select a single country, save the selection, and subsequently remove it.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and select a single country | Country is added to geographic implementations list |
| 3 | Save the Opportunity record | Selected country persists on the record |
| 4 | Refresh the page and verify saved values are retained | Saved values are retained |
| 5 | Remove the selected country using the 'remove' option | Country is successfully removed from the list |

**Test Data:**
- Country: Kenya

---

### POS_003 - Validate Geographic Region Selection
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, Region

**Objective:** Validate that the user can select a geographic region and the system correctly displays included countries.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and select a region | Region is selected, list of included countries is displayed |
| 3 | Select to exclude a specific country from the list | Excluded country is removed/grayed out from final list |
| 4 | Verify saving reflects the region minus excluded country | Geographic Implementation list accurately reflects selection/exclusion |

**Test Data:**
- Region: Sub-Saharan Africa
- Exclude Country: South Africa

---

### POS_004 - Validate Fragile State/SIDS Highlighting
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, Intelligence

**Objective:** Validate that countries flagged as Fragile States or SIDS are visibly highlighted when selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section | Section is displayed |
| 3 | Select a country flagged as a Fragile State or SIDS | Country is added to implementation list |
| 4 | Check appearance of selected country in the list | Country name/row is visibly highlighted (bold, colored, or tagging icon) |

**Test Data:**
- Country: Haiti (Flagged as Fragile State/SIDS)

---

### POS_005 - Validate UNOPS Org Unit Identification
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, OrgUnit

**Objective:** Validate that the system correctly identifies and displays the normally responsible UNOPS Org Unit for the selected country.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and select a country | Country is added to implementation list |
| 3 | Check the Org Unit column/field associated with the selected country | Relevant UNOPS Org Unit (e.g., "MCO Mexico") is displayed |

**Test Data:**
- Country: Mexico

---

### POS_006 - Validate Host Country Agreement (HCA) Status
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, HCA

**Objective:** Validate that the system displays the correct status regarding an active Host Country Agreement.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and select two countries: one with HCA and one without | Both countries are added to the list |
| 3 | Check the HCA status indicator for both countries | Country 1 shows "Yes" indicator ✓. Country 2 shows "No" indicator ✗ |

**Test Data:**
- Country 1: Portugal (HCA Status: Yes)
- Country 2: Brazil (HCA Status: No)

---

### POS_007 - Validate UNSDCF/Strategic Alignment Notification
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, UNSDCF

**Objective:** Validate that the user is notified about the requirement to complete Strategic Alignment section when a country with active UNSDCF is selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and select a country with active UNSDCF | Country is added to implementation list |
| 3 | Confirm notification/prompt appears related to Strategic Alignment section | Notification displayed stating Strategic Alignment must be completed |

**Test Data:**
- Country: Viet Nam (Active UNSDCF)

---

### NEG_008 - Validate Saving with Invalid Geographic Input
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, Negative

**Objective:** Validate that the system handles an attempt to save without specifying any geographic location.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an existing Opportunity and access WHERE section and leave all geographic selections empty | Section remains blank |
| 3 | Attempt to save the Opportunity record | If mandatory: validation error prevents save. If not mandatory: saves successfully with no geographic location |
| 4 | Verify system's defined behavior for missing geographic input | System behaves per requirement for mandatory geographic fields |

---

### NEG_009 - Validate No UNSDCF Notification for Non-Aligned Country
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Geographic, Negative, UNSDCF

**Objective:** Validate that no notification is triggered when a country without an active UNSDCF is selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to WHERE section of an Opportunity record | Section loads |
| 3 | Select a country known to not have an active UNSDCF | Country is added to implementation list |
| 4 | Confirm that no notification/prompt appears for Strategic Alignment | No notification, pop-up, or banner displayed |

**Test Data:**
- Country: Chile (No active UNSDCF)

---

## 3. WHAT Section - Products & Services (PNO-700)

### POS_001 - Validate Section Exists and AI Prioritizes Partner Results Framework
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify "WHAT" tab exists and AI extraction prioritizes tagged documents.

**Preconditions:**
- Opportunity has 2 docs uploaded: Doc A (Tagged as Framework), Doc B (Untagged)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to the Opportunity "What" tab | "What - Products & Services" section is displayed |
| 3 | Trigger AI extraction (if not auto-triggered) | System analyzes documents |
| 4 | Verify the extraction source priority | Products/Services proposed based on Doc A (Tagged Framework) content |

---

### NEG_002 - Verify Warning Appears if No Partner Framework is Tagged
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify warning banner appears when no document is tagged.

**Preconditions:**
- Opportunity has uploaded documents, but NONE are tagged "Partner Results Framework"

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to Opportunity "What" tab | "What" section loads |
| 3 | Observe the top notification area | Yellow warning banner displayed: "Partner Results Framework Not Defined" |

---

### POS_003 - Verify OM Can Review and Translate Partner Terminology
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify extracted partner terms are presented for OM verification.

**Preconditions:**
- AI analysis has completed

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to Opportunity "What" tab. Review AI-proposed Products/Services | AI displays "Partner Terminology" alongside proposed UNOPS matches |
| 3 | Verify OM can edit/verify the proposal | OM able to modify description to "translate" partner language into UNOPS terms |

---

### POS_004 - Validate All Grant Support Modality Options are Selectable
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify all 4 specific modality options are present.

**Preconditions:**
- User is on "What" tab

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to Opportunity "What" tab. Click "Delivery Modality" dropdown | Dropdown displays 4 options |
| 3 | Select each option sequentially | System accepts each selection without error |

**Expected Dropdown Options:**
1. UNOPS will be delivering all directly
2. All... via Grant Support
3. Some... via Grant Support
4. Not yet known

---

### POS_005 - Verify AI Proposes Matches to Lowest Possible Level
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify AI mapping attempts to find the lowest hierarchy level.

**Preconditions:**
- Source document contains specific service details

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to Opportunity "What" tab. Trigger AI extraction | System analyzes content |
| 3 | Review the matched UNOPS Service Lines | Matches proposed at Level 1 or lowest available level |

---

### POS_006 - Validate OM Can Manually Reject AI Match and Search
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify user can reject AI proposal and manually search.

**Preconditions:**
- AI has proposed a match

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate and log in as Partner User | User lands on the homepage |
| 2 | Navigate to Opportunity "What" tab. Review AI-proposed Products/Services | AI displays proposals |
| 3 | Reject an AI proposed match | Match is removed or flagged for replacement |
| 4 | Click to search for a replacement | Search modal opens for keyword search across "UNOPS Products and Services List" |

---

### EDGE_007 - Verify Manual Selection Allows Level 0/1 When No Docs Exist
**Priority:** Low  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify manual entry fallback when no source material exists.

**Preconditions:**
- No documents uploaded

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add New" (Product/Service) | Modal opens |
| 2 | Select a broad "Level 0" service line | System allows selection, recognizing details will be pursued later |

---

### POS_008 - Validate Hierarchy Visualization During Manual Selection
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify user can see hierarchy levels (Level 0 vs Level 1) in search.

**Preconditions:**
- "Add Product" modal is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Type a generic keyword (e.g., "Infrastructure") | Search results display |
| 2 | Inspect the results list | User can visually distinguish between Level 0 (Category) and Level 1 (Specific Service) items |

---

### POS_009 - Verify Procurement Expert Flag is Displayed for Relevant Services
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify "Procurement Expert Required" warning triggers for specific services.

**Preconditions:**
- "What" tab is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add service: "Technical advisory services - infrastructure" | Service is added |
| 2 | Verify warnings | "Procurement Expert Required" banner appears at top AND yellow badge appears on service card |

---

### POS_010 - Validate Selected Products Appear in Timeline
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify integration with Timeline/Work Breakdown Structure.

**Preconditions:**
- A Product/Service is added in "What" tab

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to the "When" tab | "When" section opens |
| 2 | Verify Milestones list | Product/Service added earlier is listed as a placeholder milestone |

---

### NEG_011 - Validate User Cannot Save Without Selecting Delivery Modality
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify "Delivery Modality" is a mandatory field.

**Preconditions:**
- "Delivery Modality" is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to move Opportunity to next stage or Save section | System prevents action or highlights "Delivery Modality" as required field |

---

### POS_012 - Verify 'Not Yet Known' Modality Option is Accepted
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify specific handling of "Not yet known" option.

**Preconditions:**
- Modality dropdown open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "Not yet known whether UNOPS will deliver directly or via Grant support modality" | Selection is accepted and saved without validation error |

---

### POS_013 - Validate Selecting 'Some' Grant Support Forces Identification
**Priority:** High  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHAT, Section

**Objective:** Verify "Some... via Grant Support" requires identifying which services.

**Preconditions:**
- Multiple products added

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "Some Products & Services will be delivered via Grant Support Modality" | System prompts user to identify which specific products/services apply to this modality |

---

## 4. WHO Section - Partners & External Stakeholders (PNO-6701)

### POS_007 - Validate Org Unit Delivery Value Warning
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHO, Risk

**Objective:** Validate that a warning is displayed if the Responsible Org Unit has not previously delivered an engagement of the calculated Total Budget (USD) value.

**Preconditions:**
- User is Partner User or Partner Global Admin
- Opportunity Total Budget > historical delivery value of Responsible Org Unit

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Identify a Responsible Org Unit whose historical delivery data is lower than calculated Total Budget | Org Unit is set as responsible |
| 3 | Check the WHO section or Opportunity Risk/Insights section | Visible warning displayed indicating responsible org unit has not previously delivered engagement of this value |

**Test Data:**
- Total Budget (USD): $150,000
- Org Unit Max History: $100,000

---

### NEG_010 - Validate that partners flagged as pooled funding cannot be selected as a Funding Partner
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, PNO, Opportunity, WHO, Negative

**Objective:** Validate the strict restriction that partners flagged as pooled funding cannot be selected as a Funding Partner for a new opportunity.

**Preconditions:**
- User is Partner User or Partner Global Admin
- Partner record exists with pooled funding flag = YES

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to the WHO section of an Opportunity | Section loads |
| 3 | Attempt to select the specific partner entity type as a Funding Partner | System prevents selection or greys out partner name in lookup list |
| 4 | Confirm partner cannot be added as Funding Partner | Error message confirms the entity is not eligible as a new opportunity funding partner |

**Test Data:**
- Partner: EU Programme Fund (pooled funding flag = YES)

---

## Summary

| Section | Total Tests | High Priority | Normal Priority | Low Priority |
|---------|-------------|---------------|-----------------|--------------|
| WHY Section (PNO-692) | 19 | 7 | 9 | 3 |
| WHERE Section (PNO-697) | 9 | 5 | 4 | 0 |
| WHAT Section (PNO-700) | 13 | 5 | 7 | 1 |
| WHO Section (PNO-6701) | 2 | 0 | 2 | 0 |
| **TOTAL** | **43** | **17** | **22** | **4** |
