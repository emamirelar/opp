# WHY Section Test Cases

## Overview
Test cases for the WHY Section in Opportunities, covering SDG alignment, implementation context, beneficiaries, and alignment frameworks.

**JIRA Story:** PNO-692, PNO-938  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 42

---

## Test Case Summary

| Category | Count | Priority |
|----------|-------|----------|
| Positive Tests | 20 | High/Normal |
| Negative Tests | 12 | High |
| Boundary/Validation Tests | 6 | Normal |
| Integration Tests | 4 | Normal |
| **TOTAL** | **42** | |

---

## 1. SDG Alignment Tests

### POS_001 - Verify SDG Selection UI Displays All 17 Goals
**Priority:** High  
**JIRA ID:** PNO-1200  
**Labels:** Opportunity, WHY_Section, SDG

**Objective:** Verify that the SDG selection interface displays all 17 Sustainable Development Goals.

**Preconditions:**
- User is logged in with Editor permissions
- Opportunity exists in Draft or Active status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to WHY Section | Section loads |
| 2 | Click SDG selection field | SDG picker opens |
| 3 | Count available SDG options | All 17 SDGs listed |
| 4 | Verify each SDG has icon and name | Icons and names correct |

**Expected SDGs:**
1. No Poverty
2. Zero Hunger
3. Good Health and Well-being
4. Quality Education
5. Gender Equality
6. Clean Water and Sanitation
7. Affordable and Clean Energy
8. Decent Work and Economic Growth
9. Industry, Innovation and Infrastructure
10. Reduced Inequalities
11. Sustainable Cities and Communities
12. Responsible Consumption and Production
13. Climate Action
14. Life Below Water
15. Life on Land
16. Peace, Justice and Strong Institutions
17. Partnerships for the Goals

---

### POS_002 - Verify Multiple SDG Selection
**Priority:** High  
**JIRA ID:** PNO-1201  
**Labels:** Opportunity, WHY_Section, SDG

**Objective:** Verify that multiple SDGs can be selected for an opportunity.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open SDG selector | Selector displays |
| 2 | Select SDG 1 (No Poverty) | SDG 1 added |
| 3 | Select SDG 4 (Quality Education) | SDG 4 added |
| 4 | Select SDG 13 (Climate Action) | SDG 13 added |
| 5 | Save the opportunity | All 3 SDGs saved |
| 6 | Reload page | All 3 SDGs displayed |

---

### POS_003 - Verify Primary SDG Designation
**Priority:** Normal  
**JIRA ID:** PNO-1202  
**Labels:** Opportunity, WHY_Section, SDG

**Objective:** Verify that one SDG can be marked as the primary goal.

**Preconditions:**
- Multiple SDGs are selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select 3 SDGs | SDGs selected |
| 2 | Click "Set as Primary" on SDG 4 | SDG 4 marked primary |
| 3 | Verify visual indicator | Primary badge shown |
| 4 | Save and reload | Primary designation preserved |

---

### POS_004 - Verify SDG Removal
**Priority:** Normal  
**JIRA ID:** PNO-1203  
**Labels:** Opportunity, WHY_Section, SDG

**Objective:** Verify that selected SDGs can be removed.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View selected SDGs | SDGs displayed |
| 2 | Click remove on SDG | SDG removed from selection |
| 3 | Save changes | Removal saved |
| 4 | Verify in list | SDG no longer associated |

---

### NEG_005 - Verify Minimum SDG Selection Required
**Priority:** High  
**JIRA ID:** PNO-1204  
**Labels:** Opportunity, WHY_Section, SDG, Negative

**Objective:** Verify that at least one SDG is required for submission.

**Preconditions:**
- No SDGs selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Clear all SDG selections | No SDGs selected |
| 2 | Attempt to submit for decision | Validation error |
| 3 | Verify error message | "At least one SDG is required" |

---

## 2. Beneficiary Tests

### POS_006 - Verify Beneficiary Count Entry
**Priority:** High  
**JIRA ID:** PNO-1210  
**Labels:** Opportunity, WHY_Section, Beneficiaries

**Objective:** Verify that expected beneficiary count can be entered.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to WHY Section | Section loads |
| 2 | Locate beneficiary count field | Field visible |
| 3 | Enter count (e.g., 50000) | Value accepted |
| 4 | Save opportunity | Value saved |

---

### POS_007 - Verify Beneficiary Type Selection
**Priority:** High  
**JIRA ID:** PNO-1211  
**Labels:** Opportunity, WHY_Section, Beneficiaries

**Objective:** Verify beneficiary types can be selected from predefined list.

**Preconditions:**
- User is in Edit mode

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open beneficiary type dropdown | Options display |
| 2 | Select "Direct Beneficiaries" | Type selected |
| 3 | Select "Indirect Beneficiaries" | Additional type added |
| 4 | Save | Selections saved |

---

### POS_008 - Verify Beneficiary Breakdown Entry
**Priority:** Normal  
**JIRA ID:** PNO-1212  
**Labels:** Opportunity, WHY_Section, Beneficiaries

**Objective:** Verify demographic breakdown can be entered for beneficiaries.

**Preconditions:**
- Beneficiary count is entered

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter total beneficiaries: 10000 | Total entered |
| 2 | Enter women: 5200 | Women count entered |
| 3 | Enter men: 4800 | Men count entered |
| 4 | Enter youth: 3500 | Youth count entered |
| 5 | Save | All values saved |

---

### NEG_009 - Verify Beneficiary Breakdown Validation
**Priority:** High  
**JIRA ID:** PNO-1213  
**Labels:** Opportunity, WHY_Section, Beneficiaries, Negative

**Objective:** Verify breakdown cannot exceed total beneficiaries.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter total beneficiaries: 1000 | Total entered |
| 2 | Enter women: 600 | Accepted |
| 3 | Enter men: 600 | Validation warning |
| 4 | Verify message | "Gender breakdown exceeds total" |

---

### NEG_010 - Verify Negative Beneficiary Count Rejected
**Priority:** High  
**JIRA ID:** PNO-1214  
**Labels:** Opportunity, WHY_Section, Beneficiaries, Negative

**Objective:** Verify negative numbers are rejected for beneficiary counts.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter beneficiary count: -500 | Value entered |
| 2 | Attempt to save | Validation error |
| 3 | Verify message | "Beneficiary count must be positive" |

---

## 3. Implementation Context Tests

### POS_011 - Verify Implementation Context Narrative Entry
**Priority:** High  
**JIRA ID:** PNO-1220  
**Labels:** Opportunity, WHY_Section, Implementation

**Objective:** Verify implementation context narrative field accepts text.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Implementation Context | Field visible |
| 2 | Enter detailed narrative (500 chars) | Text accepted |
| 3 | Save opportunity | Text saved |
| 4 | Reload page | Text displayed correctly |

---

### POS_012 - Verify AI-Assisted Context Generation
**Priority:** Normal  
**JIRA ID:** PNO-1221  
**Labels:** Opportunity, WHY_Section, Implementation, AI

**Objective:** Verify AI can generate implementation context suggestions.

**Preconditions:**
- AI features enabled
- Basic opportunity info entered

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Generate with AI" button | AI processing starts |
| 2 | Wait for generation | Content generated |
| 3 | Verify content appears | Suggested text displayed |
| 4 | Option to accept/edit/reject | All actions available |

---

### NEG_013 - Verify AI Content Character Limit
**Priority:** Normal  
**JIRA ID:** PNO-1222  
**Labels:** Opportunity, WHY_Section, Implementation, AI, Negative

**Objective:** Verify AI-generated content respects character limits.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Generate AI content | Content created |
| 2 | Check character count | Within allowed limit |
| 3 | Verify truncation if exceeded | Content truncated properly |

---

### POS_014 - Verify Geographic Context Selection
**Priority:** High  
**JIRA ID:** PNO-1223  
**Labels:** Opportunity, WHY_Section, Implementation, Geographic

**Objective:** Verify geographic implementation context can be specified.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select implementation country | Country selected |
| 2 | Select region/state | Region selected |
| 3 | Specify locality if applicable | Locality entered |
| 4 | Save | Geographic context saved |

---

## 4. UN Cooperation Framework Tests

### POS_015 - Verify UN Cooperation Framework Selection
**Priority:** High  
**JIRA ID:** PNO-1230  
**Labels:** Opportunity, WHY_Section, UNFramework

**Objective:** Verify UN Cooperation Framework can be selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to UN Framework section | Section visible |
| 2 | Open framework dropdown | Options display |
| 3 | Select applicable framework | Selection made |
| 4 | Save | Framework saved |

---

### POS_016 - Verify Framework Outcome Alignment
**Priority:** Normal  
**JIRA ID:** PNO-1231  
**Labels:** Opportunity, WHY_Section, UNFramework

**Objective:** Verify specific outcomes can be aligned within a framework.

**Preconditions:**
- UN Framework is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select UN Framework | Framework selected |
| 2 | View outcome options | Outcomes for framework display |
| 3 | Select specific outcome(s) | Outcomes selected |
| 4 | Save | Alignments saved |

---

### NEG_017 - Verify Framework Required for Submission
**Priority:** High  
**JIRA ID:** PNO-1232  
**Labels:** Opportunity, WHY_Section, UNFramework, Negative

**Objective:** Verify UN Framework is mandatory for go decision.

**Preconditions:**
- UN Framework field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave framework unselected | Field empty |
| 2 | Attempt send for decision | Blocked |
| 3 | Verify validation | "UN Cooperation Framework required" |

---

## 5. Alignment Framework Tests

### POS_018 - Verify Corporate Strategy Alignment
**Priority:** Normal  
**JIRA ID:** PNO-1240  
**Labels:** Opportunity, WHY_Section, Alignment

**Objective:** Verify alignment with corporate strategic objectives.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to alignment section | Section loads |
| 2 | Select strategic objective(s) | Objectives available |
| 3 | Link opportunity to objective | Link created |
| 4 | Save | Alignment saved |

---

### POS_019 - Verify Country Programme Alignment
**Priority:** Normal  
**JIRA ID:** PNO-1241  
**Labels:** Opportunity, WHY_Section, Alignment

**Objective:** Verify alignment with country programme can be specified.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select implementation country | Country set |
| 2 | View country programme options | Programmes for country display |
| 3 | Select applicable programme | Selection made |
| 4 | Save | Alignment recorded |

---

### POS_020 - Verify Multi-Framework Alignment
**Priority:** Normal  
**JIRA ID:** PNO-1242  
**Labels:** Opportunity, WHY_Section, Alignment

**Objective:** Verify multiple alignment frameworks can be selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select corporate strategy | Selected |
| 2 | Select UN Framework | Selected |
| 3 | Select regional priority | Selected |
| 4 | Save | All alignments saved |
| 5 | Verify display | All shown in summary |

---

## 6. High-Risk Checklist Tests

### POS_021 - Verify High-Risk Checklist Display
**Priority:** High  
**JIRA ID:** PNO-1250  
**Labels:** Opportunity, WHY_Section, HighRisk

**Objective:** Verify high-risk checklist questions are displayed.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to High-Risk section | Section loads |
| 2 | View checklist questions | All questions visible |
| 3 | Verify each has Yes/No options | Options available |

---

### POS_022 - Verify High-Risk Flag Triggers
**Priority:** High  
**JIRA ID:** PNO-1251  
**Labels:** Opportunity, WHY_Section, HighRisk

**Objective:** Verify that "Yes" answers trigger high-risk flag.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Answer all questions "No" | No flag |
| 2 | Change one answer to "Yes" | High-risk flag appears |
| 3 | Verify visual indicator | Warning/badge shown |
| 4 | Verify in opportunity summary | Marked as high-risk |

---

### POS_023 - Verify High-Risk Additional Information
**Priority:** Normal  
**JIRA ID:** PNO-1252  
**Labels:** Opportunity, WHY_Section, HighRisk

**Objective:** Verify additional information can be provided for high-risk items.

**Preconditions:**
- At least one high-risk item answered "Yes"

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Mark item as high-risk (Yes) | Item flagged |
| 2 | Additional info field appears | Field visible |
| 3 | Enter mitigation details | Text accepted |
| 4 | Save | Information saved |

---

### NEG_024 - Verify High-Risk Checklist Required
**Priority:** High  
**JIRA ID:** PNO-1253  
**Labels:** Opportunity, WHY_Section, HighRisk, Negative

**Objective:** Verify high-risk checklist must be completed for submission.

**Preconditions:**
- Checklist questions unanswered

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave checklist incomplete | Questions unanswered |
| 2 | Attempt submit for decision | Blocked |
| 3 | Verify validation | "High-risk checklist required" |

---

## 7. Rationale Tests

### POS_025 - Verify Rationale Narrative Entry
**Priority:** High  
**JIRA ID:** PNO-1260  
**Labels:** Opportunity, WHY_Section, Rationale

**Objective:** Verify the rationale/justification narrative can be entered.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Rationale field | Field visible |
| 2 | Enter rationale text | Text accepted |
| 3 | Verify rich text formatting | Formatting works |
| 4 | Save | Content saved |

---

### POS_026 - Verify AI Rationale Suggestion
**Priority:** Normal  
**JIRA ID:** PNO-1261  
**Labels:** Opportunity, WHY_Section, Rationale, AI

**Objective:** Verify AI can suggest rationale content.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Suggest with AI" | AI processes |
| 2 | View suggestion | Suggested text appears |
| 3 | Edit as needed | Editing works |
| 4 | Accept | Content saved |

---

### NEG_027 - Verify Rationale Character Minimum
**Priority:** Normal  
**JIRA ID:** PNO-1262  
**Labels:** Opportunity, WHY_Section, Rationale, Negative

**Objective:** Verify minimum character requirement for rationale.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter very short rationale (10 chars) | Text entered |
| 2 | Attempt to save/submit | Warning displayed |
| 3 | Verify message | "Rationale should be at least X characters" |

---

## 8. Validation Tests

### NEG_028 - Verify Required Fields Validation
**Priority:** High  
**JIRA ID:** PNO-1270  
**Labels:** Opportunity, WHY_Section, Validation, Negative

**Objective:** Verify all required WHY Section fields are validated.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave all required fields empty | Fields blank |
| 2 | Click Save | Validation runs |
| 3 | Verify all errors shown | Each missing field highlighted |
| 4 | Fill fields one by one | Errors clear as fixed |

---

### NEG_029 - Verify Numeric Field Validation
**Priority:** Normal  
**JIRA ID:** PNO-1271  
**Labels:** Opportunity, WHY_Section, Validation, Negative

**Objective:** Verify numeric fields reject non-numeric input.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter "abc" in beneficiary count | Input rejected/cleared |
| 2 | Enter "12.34.56" | Invalid format rejected |
| 3 | Enter valid number | Accepted |

---

### NEG_030 - Verify Date Field Validation
**Priority:** Normal  
**JIRA ID:** PNO-1272  
**Labels:** Opportunity, WHY_Section, Validation, Negative

**Objective:** Verify date fields validate format and range.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter invalid date format | Rejected |
| 2 | Enter date in past (if not allowed) | Warning/error |
| 3 | Enter valid date | Accepted |

---

## 9. Section Navigation Tests

### POS_031 - Verify Navigation to WHY Section
**Priority:** Normal  
**JIRA ID:** PNO-1280  
**Labels:** Opportunity, WHY_Section, Navigation

**Objective:** Verify navigation to WHY section works correctly.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open opportunity | Opportunity loads |
| 2 | Click WHY tab | WHY section displays |
| 3 | Verify all subsections | All visible and accessible |

---

### POS_032 - Verify Section Completion Indicator
**Priority:** Normal  
**JIRA ID:** PNO-1281  
**Labels:** Opportunity, WHY_Section, Navigation

**Objective:** Verify WHY section shows completion status.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View opportunity tabs | Tabs display |
| 2 | Check WHY tab indicator | Shows completion % |
| 3 | Complete more fields | Indicator updates |
| 4 | Complete all fields | Shows as complete |

---

## 10. Permission Tests

### NEG_033 - Verify View-Only User Cannot Edit WHY Section
**Priority:** High  
**JIRA ID:** PNO-1290  
**Labels:** Opportunity, WHY_Section, Permission, Negative

**Objective:** Verify read-only users cannot modify WHY section.

**Preconditions:**
- User has View-only access

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as View-only user | Login successful |
| 2 | Navigate to WHY section | Section loads |
| 3 | Verify all fields read-only | No edit controls |
| 4 | Attempt edit via API | 403 Forbidden |

---

### POS_034 - Verify OM Can Edit WHY Section
**Priority:** High  
**JIRA ID:** PNO-1291  
**Labels:** Opportunity, WHY_Section, Permission

**Objective:** Verify Opportunity Manager can edit WHY section.

**Preconditions:**
- User is Opportunity Manager

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as OM | Login successful |
| 2 | Navigate to WHY section | Section loads |
| 3 | Verify edit controls available | Can edit |
| 4 | Make changes and save | Changes saved |

---

## 11. Integration Tests

### INT_035 - Verify WHY Data in Opportunity Summary
**Priority:** Normal  
**JIRA ID:** PNO-1300  
**Labels:** Opportunity, WHY_Section, Integration

**Objective:** Verify WHY section data appears in opportunity summary.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHY section fields | Fields saved |
| 2 | View opportunity summary | Summary displays |
| 3 | Verify SDGs appear | SDG icons shown |
| 4 | Verify beneficiary count | Count displayed |

---

### INT_036 - Verify WHY Data in Decision Package
**Priority:** High  
**JIRA ID:** PNO-1301  
**Labels:** Opportunity, WHY_Section, Integration

**Objective:** Verify WHY section data included in go decision package.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHY section | All data saved |
| 2 | Submit for decision | Package generated |
| 3 | View decision package | WHY data included |
| 4 | Verify all fields present | Complete information |

---

### INT_037 - Verify WHY Data Export
**Priority:** Normal  
**JIRA ID:** PNO-1302  
**Labels:** Opportunity, WHY_Section, Integration

**Objective:** Verify WHY section data can be exported.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHY section | Data saved |
| 2 | Export opportunity data | Export generated |
| 3 | Verify export contains WHY data | All fields included |

---

### INT_038 - Verify WHY Data in oUP Integration
**Priority:** High  
**JIRA ID:** PNO-1303  
**Labels:** Opportunity, WHY_Section, Integration, oUP

**Objective:** Verify WHY section data syncs to oUP system.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHY section | Data saved |
| 2 | Trigger oUP sync | Sync runs |
| 3 | Verify in oUP system | Data appears correctly |
| 4 | Check field mapping | All fields mapped |

---

## 12. Additional Tests

### POS_039 - Verify Draft Save Without All Fields
**Priority:** Normal  
**JIRA ID:** PNO-1310  
**Labels:** Opportunity, WHY_Section

**Objective:** Verify partial WHY data can be saved as draft.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter only SDGs | SDGs set |
| 2 | Save as draft | Draft saved |
| 3 | Leave other fields empty | No error |
| 4 | Reload | SDGs preserved |

---

### POS_040 - Verify Field Tooltips/Help Text
**Priority:** Low  
**JIRA ID:** PNO-1311  
**Labels:** Opportunity, WHY_Section, UI

**Objective:** Verify help text is available for complex fields.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Hover over field label | Tooltip appears |
| 2 | Click help icon (if present) | Help text displays |
| 3 | Verify content is helpful | Clear explanation |

---

### NEG_041 - Verify XSS Prevention in Text Fields
**Priority:** High  
**JIRA ID:** PNO-1312  
**Labels:** Opportunity, WHY_Section, Security, Negative

**Objective:** Verify script injection is prevented.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter `<script>alert('xss')</script>` in narrative | Text entered |
| 2 | Save field | Saves successfully |
| 3 | Reload and view | Script NOT executed |
| 4 | Verify encoding | HTML entities escaped |

---

### POS_042 - Verify Autosave Functionality
**Priority:** Normal  
**JIRA ID:** PNO-1313  
**Labels:** Opportunity, WHY_Section

**Objective:** Verify WHY section autosaves work.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Start editing WHY fields | Edit mode |
| 2 | Wait for autosave interval | Autosave triggers |
| 3 | Verify save indicator | "Draft saved" shown |
| 4 | Close browser unexpectedly | Data preserved |
| 5 | Reopen opportunity | Recent edits present |

---

## Summary

| Category | Count |
|----------|-------|
| SDG Alignment | 5 |
| Beneficiaries | 5 |
| Implementation Context | 4 |
| UN Framework | 3 |
| Alignment Frameworks | 3 |
| High-Risk Checklist | 4 |
| Rationale | 3 |
| Validation | 3 |
| Navigation | 2 |
| Permissions | 2 |
| Integration | 4 |
| Additional | 4 |
| **TOTAL** | **42** |

---

**C# Test Class:** `WHYSectionTests.cs`  
**Playwright Test File:** `why-section.spec.ts`  
**Status:** ✅ Aligned with JIRA PNO-692, PNO-938
