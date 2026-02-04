# WHAT Section Test Cases

## Overview
Test cases for the WHAT Section in Opportunities, covering project scope, deliverables, outputs, and AI-assisted matching.

**JIRA Story:** PNO-700  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 38

---

## Test Case Summary

| Category | Count | Priority |
|----------|-------|----------|
| Positive Tests | 18 | High/Normal |
| Negative Tests | 10 | High |
| AI Matching Tests | 6 | Normal |
| Hierarchy/Structure Tests | 4 | Normal |
| **TOTAL** | **38** | |

---

## 1. Scope Definition Tests

### POS_001 - Verify Project Scope Narrative Entry
**Priority:** High  
**JIRA ID:** PNO-1400  
**Labels:** Opportunity, WHAT_Section, Scope

**Objective:** Verify that project scope narrative can be entered and saved.

**Preconditions:**
- User is logged in with Editor permissions
- Opportunity exists in Draft or Active status

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to WHAT Section | Section loads |
| 2 | Locate scope narrative field | Field visible |
| 3 | Enter detailed scope (1000+ chars) | Text accepted |
| 4 | Apply rich text formatting | Formatting works |
| 5 | Save opportunity | Scope saved |
| 6 | Reload page | Content preserved |

---

### POS_002 - Verify Scope AI Generation
**Priority:** Normal  
**JIRA ID:** PNO-1401  
**Labels:** Opportunity, WHAT_Section, Scope, AI

**Objective:** Verify AI can suggest project scope based on opportunity context.

**Preconditions:**
- AI features enabled
- Basic opportunity data entered

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Generate Scope with AI" | AI processing |
| 2 | Wait for generation | Content appears |
| 3 | Review suggested content | Relevant to opportunity |
| 4 | Edit as needed | Editing works |
| 5 | Accept | Content saved |

---

### NEG_003 - Verify Scope Required for Submission
**Priority:** High  
**JIRA ID:** PNO-1402  
**Labels:** Opportunity, WHAT_Section, Scope, Negative

**Objective:** Verify scope is mandatory for go decision submission.

**Preconditions:**
- Scope field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave scope empty | Field blank |
| 2 | Attempt submit for decision | Validation error |
| 3 | Verify error message | "Project scope is required" |

---

## 2. Deliverables Tests

### POS_004 - Verify Add Deliverable
**Priority:** High  
**JIRA ID:** PNO-1410  
**Labels:** Opportunity, WHAT_Section, Deliverables

**Objective:** Verify deliverables can be added to the opportunity.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add Deliverable" | Form appears |
| 2 | Enter deliverable name | Name accepted |
| 3 | Enter description | Description accepted |
| 4 | Set target date | Date selected |
| 5 | Save | Deliverable added to list |

---

### POS_005 - Verify Multiple Deliverables
**Priority:** Normal  
**JIRA ID:** PNO-1411  
**Labels:** Opportunity, WHAT_Section, Deliverables

**Objective:** Verify multiple deliverables can be added.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Add first deliverable | Deliverable 1 created |
| 2 | Add second deliverable | Deliverable 2 created |
| 3 | Add third deliverable | Deliverable 3 created |
| 4 | View deliverables list | All 3 displayed |
| 5 | Save opportunity | All saved |

---

### POS_006 - Verify Edit Deliverable
**Priority:** Normal  
**JIRA ID:** PNO-1412  
**Labels:** Opportunity, WHAT_Section, Deliverables

**Objective:** Verify existing deliverables can be edited.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View existing deliverable | Deliverable displayed |
| 2 | Click Edit | Edit form opens |
| 3 | Modify name | Name updated |
| 4 | Modify date | Date updated |
| 5 | Save | Changes saved |

---

### POS_007 - Verify Delete Deliverable
**Priority:** Normal  
**JIRA ID:** PNO-1413  
**Labels:** Opportunity, WHAT_Section, Deliverables

**Objective:** Verify deliverables can be removed.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View deliverables list | Deliverables shown |
| 2 | Click Delete on a deliverable | Confirmation dialog |
| 3 | Confirm deletion | Deliverable removed |
| 4 | Save opportunity | Removal saved |

---

### POS_008 - Verify Deliverable Reordering
**Priority:** Low  
**JIRA ID:** PNO-1414  
**Labels:** Opportunity, WHAT_Section, Deliverables

**Objective:** Verify deliverables can be reordered.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View multiple deliverables | List displays |
| 2 | Drag deliverable to new position | Order changes |
| 3 | Save | New order preserved |

---

### NEG_009 - Verify Deliverable Name Required
**Priority:** High  
**JIRA ID:** PNO-1415  
**Labels:** Opportunity, WHAT_Section, Deliverables, Negative

**Objective:** Verify deliverable name is mandatory.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Add Deliverable | Form opens |
| 2 | Leave name empty | Field blank |
| 3 | Attempt to save | Validation error |
| 4 | Verify message | "Deliverable name is required" |

---

## 3. Outputs Tests

### POS_010 - Verify Output Entry
**Priority:** High  
**JIRA ID:** PNO-1420  
**Labels:** Opportunity, WHAT_Section, Outputs

**Objective:** Verify project outputs can be defined.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Outputs section | Section visible |
| 2 | Click Add Output | Form appears |
| 3 | Enter output details | Details accepted |
| 4 | Link to deliverable | Link created |
| 5 | Save | Output saved |

---

### POS_011 - Verify Output Metrics
**Priority:** Normal  
**JIRA ID:** PNO-1421  
**Labels:** Opportunity, WHAT_Section, Outputs

**Objective:** Verify measurable metrics can be added to outputs.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View output | Output displayed |
| 2 | Add metric (e.g., "100 trained") | Metric added |
| 3 | Set target value | Value entered |
| 4 | Save | Metric saved |

---

### POS_012 - Verify Output-to-Outcome Mapping
**Priority:** Normal  
**JIRA ID:** PNO-1422  
**Labels:** Opportunity, WHAT_Section, Outputs

**Objective:** Verify outputs can be mapped to expected outcomes.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create output | Output exists |
| 2 | Open outcome mapping | Mapping interface |
| 3 | Select related outcome | Outcome linked |
| 4 | Save | Mapping preserved |

---

## 4. Initiative Type Tests

### POS_013 - Verify Initiative Type Selection
**Priority:** High  
**JIRA ID:** PNO-1430  
**Labels:** Opportunity, WHAT_Section, InitiativeType

**Objective:** Verify initiative type can be selected from predefined options.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Initiative Type | Field visible |
| 2 | Open dropdown | Options display |
| 3 | Select type (e.g., "Grant Support") | Type selected |
| 4 | Save | Selection saved |

---

### POS_014 - Verify Initiative Type Hierarchy
**Priority:** Normal  
**JIRA ID:** PNO-1431  
**Labels:** Opportunity, WHAT_Section, InitiativeType, Hierarchy

**Objective:** Verify initiative types display in hierarchical structure.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open initiative type selector | Selector opens |
| 2 | View type hierarchy | Parent-child structure visible |
| 3 | Expand parent category | Children display |
| 4 | Select child type | Selection made |

---

### NEG_015 - Verify Initiative Type Required
**Priority:** High  
**JIRA ID:** PNO-1432  
**Labels:** Opportunity, WHAT_Section, InitiativeType, Negative

**Objective:** Verify initiative type is mandatory.

**Preconditions:**
- Initiative type field is empty

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave initiative type empty | Field blank |
| 2 | Attempt submit for decision | Validation error |
| 3 | Verify message | "Initiative type is required" |

---

## 5. AI Matching Tests

### AI_016 - Verify AI Matching Service Options
**Priority:** High  
**JIRA ID:** PNO-1440  
**Labels:** Opportunity, WHAT_Section, AI, Matching

**Objective:** Verify AI can suggest relevant UNOPS service options.

**Preconditions:**
- Opportunity scope and context defined

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Match Services with AI" | AI processes |
| 2 | View suggestions | Service options displayed |
| 3 | See match confidence | Percentage shown |
| 4 | Accept or reject suggestions | Actions work |

---

### AI_017 - Verify AI Service Matching Accuracy
**Priority:** Normal  
**JIRA ID:** PNO-1441  
**Labels:** Opportunity, WHAT_Section, AI, Matching

**Objective:** Verify AI matching provides relevant service recommendations.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter scope: "Construction of health facilities" | Scope set |
| 2 | Run AI matching | Suggestions appear |
| 3 | Verify suggestions include infrastructure services | Relevant matches |
| 4 | Verify irrelevant services excluded | No unrelated options |

---

### AI_018 - Verify AI Matching History
**Priority:** Low  
**JIRA ID:** PNO-1442  
**Labels:** Opportunity, WHAT_Section, AI, Matching

**Objective:** Verify AI matching history is preserved.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run AI matching | Suggestions generated |
| 2 | Accept some suggestions | Selections saved |
| 3 | Run matching again | Previous selections visible |
| 4 | View matching history | History accessible |

---

### AI_019 - Verify AI Content Character Limits
**Priority:** Normal  
**JIRA ID:** PNO-1443  
**Labels:** Opportunity, WHAT_Section, AI, Negative

**Objective:** Verify AI-generated content respects field limits.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Generate AI content for scope | Content generated |
| 2 | Verify character count | Within field limit |
| 3 | If exceeded, verify truncation | Clean truncation |

---

### NEG_020 - Verify AI Matching Without Context
**Priority:** Normal  
**JIRA ID:** PNO-1444  
**Labels:** Opportunity, WHAT_Section, AI, Negative

**Objective:** Verify AI matching requires sufficient context.

**Preconditions:**
- Opportunity has minimal data

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create opportunity with only name | Minimal data |
| 2 | Attempt AI service matching | Warning displayed |
| 3 | Verify message | "Add more details for better matching" |

---

### AI_021 - Verify AI Matching Fallback
**Priority:** Low  
**JIRA ID:** PNO-1445  
**Labels:** Opportunity, WHAT_Section, AI, Negative

**Objective:** Verify system handles AI service unavailability.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | AI service is unavailable | Service down |
| 2 | Attempt AI matching | Graceful error |
| 3 | Verify error message | "AI service unavailable" |
| 4 | Manual selection still works | Can select manually |

---

## 6. Hierarchy Tests

### HIER_022 - Verify Service Hierarchy Display
**Priority:** Normal  
**JIRA ID:** PNO-1450  
**Labels:** Opportunity, WHAT_Section, Hierarchy

**Objective:** Verify services display in proper hierarchy.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open service selector | Selector opens |
| 2 | View service categories | Categories visible |
| 3 | Expand category | Subcategories display |
| 4 | Expand subcategory | Individual services show |

---

### HIER_023 - Verify Multi-Level Selection
**Priority:** Normal  
**JIRA ID:** PNO-1451  
**Labels:** Opportunity, WHAT_Section, Hierarchy

**Objective:** Verify services at different hierarchy levels can be selected.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select category-level service | Selected |
| 2 | Also select specific sub-service | Both selected |
| 3 | Save | Both saved |
| 4 | View selections | Hierarchy preserved |

---

### HIER_024 - Verify Parent-Child Relationship Display
**Priority:** Low  
**JIRA ID:** PNO-1452  
**Labels:** Opportunity, WHAT_Section, Hierarchy

**Objective:** Verify selected services show parent context.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select deep-level service | Service selected |
| 2 | View in summary | Shows parent path |
| 3 | Example display | "Infrastructure > Construction > Health Facilities" |

---

### NEG_025 - Verify Cannot Select Inactive Service
**Priority:** Normal  
**JIRA ID:** PNO-1453  
**Labels:** Opportunity, WHAT_Section, Hierarchy, Negative

**Objective:** Verify inactive services are not selectable.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open service selector | Selector opens |
| 2 | Look for inactive service | Not displayed or grayed |
| 3 | Attempt to select | Selection blocked |

---

## 7. Grant Support Tests

### POS_026 - Verify Grant Support Fields
**Priority:** High  
**JIRA ID:** PNO-1460  
**Labels:** Opportunity, WHAT_Section, GrantSupport

**Objective:** Verify grant-specific fields are available when grant type selected.

**Preconditions:**
- Initiative type is Grant Support

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select Grant Support initiative type | Type selected |
| 2 | View additional fields | Grant-specific fields appear |
| 3 | Enter grant value | Value accepted |
| 4 | Enter grant recipient | Recipient entered |
| 5 | Save | All fields saved |

---

### POS_027 - Verify Grant Recipient Search
**Priority:** Normal  
**JIRA ID:** PNO-1461  
**Labels:** Opportunity, WHAT_Section, GrantSupport

**Objective:** Verify grant recipients can be searched from partner database.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open grant recipient field | Search available |
| 2 | Search for partner | Results display |
| 3 | Select partner | Partner linked |
| 4 | Save | Link saved |

---

### NEG_028 - Verify Grant Value Validation
**Priority:** High  
**JIRA ID:** PNO-1462  
**Labels:** Opportunity, WHAT_Section, GrantSupport, Negative

**Objective:** Verify grant value validation.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter negative grant value | Value rejected |
| 2 | Enter zero value | Warning displayed |
| 3 | Enter valid value | Accepted |

---

## 8. Validation Tests

### NEG_029 - Verify Required Fields for WHAT Section
**Priority:** High  
**JIRA ID:** PNO-1470  
**Labels:** Opportunity, WHAT_Section, Validation, Negative

**Objective:** Verify all required WHAT fields are validated.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Leave required fields empty | Fields blank |
| 2 | Attempt save or submit | Validation runs |
| 3 | Verify all errors shown | Each field highlighted |

---

### NEG_030 - Verify Scope Character Limit
**Priority:** Normal  
**JIRA ID:** PNO-1471  
**Labels:** Opportunity, WHAT_Section, Validation, Negative

**Objective:** Verify scope field has character limit.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter scope exceeding limit | Text entered |
| 2 | Verify character counter | Shows limit exceeded |
| 3 | Attempt save | Warning or truncation |

---

### NEG_031 - Verify Date Validation for Deliverables
**Priority:** Normal  
**JIRA ID:** PNO-1472  
**Labels:** Opportunity, WHAT_Section, Validation, Negative

**Objective:** Verify deliverable dates must be logical.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set deliverable date in past | Date entered |
| 2 | Verify warning | "Date is in the past" |
| 3 | Set date before opportunity start | Date entered |
| 4 | Verify validation | Warning about sequence |

---

## 9. Permission Tests

### NEG_032 - Verify View-Only Cannot Edit WHAT Section
**Priority:** High  
**JIRA ID:** PNO-1480  
**Labels:** Opportunity, WHAT_Section, Permission, Negative

**Objective:** Verify read-only users cannot modify WHAT section.

**Preconditions:**
- User has View-only access

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as View-only user | Login successful |
| 2 | Navigate to WHAT section | Section loads |
| 3 | Verify edit controls hidden | No edit buttons |
| 4 | Attempt API edit | 403 Forbidden |

---

### POS_033 - Verify Collaborator Can Edit WHAT Section
**Priority:** High  
**JIRA ID:** PNO-1481  
**Labels:** Opportunity, WHAT_Section, Permission

**Objective:** Verify collaborators have edit access.

**Preconditions:**
- User is added as Collaborator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Collaborator | Login successful |
| 2 | Navigate to WHAT section | Section loads |
| 3 | Verify edit controls available | Can edit |
| 4 | Make changes and save | Changes saved |

---

## 10. Integration Tests

### INT_034 - Verify WHAT Data in Opportunity Statement
**Priority:** High  
**JIRA ID:** PNO-1490  
**Labels:** Opportunity, WHAT_Section, Integration

**Objective:** Verify WHAT section data appears in opportunity statement.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHAT section | Data saved |
| 2 | Generate opportunity statement | Statement created |
| 3 | Verify WHAT content included | Scope, deliverables present |

---

### INT_035 - Verify WHAT Data in oUP Sync
**Priority:** High  
**JIRA ID:** PNO-1491  
**Labels:** Opportunity, WHAT_Section, Integration, oUP

**Objective:** Verify WHAT section data syncs to oUP.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Complete WHAT section | Data saved |
| 2 | Trigger oUP sync | Sync runs |
| 3 | Verify in oUP | Data mapped correctly |

---

### INT_036 - Verify Services Linked to Budget
**Priority:** Normal  
**JIRA ID:** PNO-1492  
**Labels:** Opportunity, WHAT_Section, Integration

**Objective:** Verify selected services can link to budget items.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select services in WHAT | Services selected |
| 2 | Navigate to Budget section | Budget displays |
| 3 | Create budget line for service | Link available |
| 4 | Verify association | Service-budget linked |

---

## 11. Additional Tests

### POS_037 - Verify Section Completion Indicator
**Priority:** Normal  
**JIRA ID:** PNO-1500  
**Labels:** Opportunity, WHAT_Section, UI

**Objective:** Verify WHAT section shows completion status.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View opportunity tabs | Tabs display |
| 2 | Check WHAT tab indicator | Shows completion % |
| 3 | Complete fields | Indicator updates |
| 4 | Complete all | Shows as complete |

---

### POS_038 - Verify Autosave in WHAT Section
**Priority:** Normal  
**JIRA ID:** PNO-1501  
**Labels:** Opportunity, WHAT_Section

**Objective:** Verify autosave works for WHAT section.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Start editing WHAT | Edit mode |
| 2 | Wait for autosave | Autosave triggers |
| 3 | Verify save indicator | "Draft saved" shown |
| 4 | Navigate away and return | Data preserved |

---

## Summary

| Category | Count |
|----------|-------|
| Scope Definition | 3 |
| Deliverables | 6 |
| Outputs | 3 |
| Initiative Type | 3 |
| AI Matching | 6 |
| Hierarchy | 4 |
| Grant Support | 3 |
| Validation | 3 |
| Permission | 2 |
| Integration | 3 |
| Additional | 2 |
| **TOTAL** | **38** |

---

**C# Test Class:** `WHATSectionTests.cs`  
**Playwright Test File:** `what-section.spec.ts`  
**Status:** ✅ Aligned with JIRA PNO-700
