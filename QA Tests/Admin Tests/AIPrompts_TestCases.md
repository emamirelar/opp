# AI Prompts Administration Test Cases

## Overview
Test cases for the Administration - AI Prompts management feature in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-120  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 14

---

## Test Cases

### POS_001 - Access AI Prompts Administration Page
**Priority:** High  
**Labels:** Administration, AI_Prompts, Access

**Objective:** Validate that administrators can access the AI Prompts administration page.

**Preconditions:**
- User has Administrator role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as Administrator | Dashboard loads |
| 2 | Navigate to Administration menu | Admin options displayed |
| 3 | Click on "AI Prompts" | AI Prompts management page loads |
| 4 | Verify page content | List of configured AI prompts displayed |

---

### POS_002 - View List of AI Prompts
**Priority:** High  
**Labels:** Administration, AI_Prompts

**Objective:** Validate that all configured AI prompts are displayed.

**Preconditions:**
- User is on AI Prompts management page

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | View the prompts list | List of AI prompts visible |
| 2 | Verify columns displayed | Name, Category, Status, Last Modified columns visible |
| 3 | Verify prompts are categorized | Prompts grouped by category or type |

---

### POS_003 - Create New AI Prompt
**Priority:** High  
**Labels:** Administration, AI_Prompts, Create

**Objective:** Validate creating a new AI prompt.

**Preconditions:**
- Administrator access

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add New Prompt" button | Create prompt form opens |
| 2 | Enter prompt name | Name field accepts input |
| 3 | Select prompt category | Category dropdown selection works |
| 4 | Enter prompt text/template | Text area accepts prompt content |
| 5 | Set prompt as Active | Status toggle works |
| 6 | Save the prompt | Prompt saved successfully |
| 7 | Verify prompt appears in list | New prompt visible in list |

**Test Data:**
- Name: "Partner Analysis Prompt"
- Category: "Partners"
- Prompt Text: "Analyze the following partner information and provide insights..."

---

### POS_004 - Edit Existing AI Prompt
**Priority:** High  
**Labels:** Administration, AI_Prompts, Edit

**Objective:** Validate editing an existing AI prompt.

**Preconditions:**
- At least one AI prompt exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate an existing prompt | Prompt displayed in list |
| 2 | Click edit button | Edit form opens with current values |
| 3 | Modify prompt text | Changes accepted in field |
| 4 | Save changes | Changes saved successfully |
| 5 | Verify updated content | Updated prompt text visible |

---

### POS_005 - Delete AI Prompt
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Delete

**Objective:** Validate deleting an AI prompt.

**Preconditions:**
- AI prompt exists that can be deleted

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate prompt to delete | Prompt in list |
| 2 | Click delete button | Confirmation dialog appears |
| 3 | Confirm deletion | Prompt deleted |
| 4 | Verify prompt removed from list | Prompt no longer visible |

---

### NEG_006 - Prevent Deletion of System Prompts
**Priority:** High  
**Labels:** Administration, AI_Prompts, Negative

**Objective:** Validate that system-defined prompts cannot be deleted.

**Preconditions:**
- System prompts exist (marked as non-deletable)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate a system prompt | Prompt displayed |
| 2 | Verify delete button is disabled or hidden | No delete option for system prompts |
| 3 | Attempt deletion via API (if testing API) | Request rejected |

---

### POS_007 - Activate/Deactivate AI Prompt
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Status

**Objective:** Validate toggling prompt active status.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate an active prompt | Prompt shows Active status |
| 2 | Click to deactivate | Status changes to Inactive |
| 3 | Verify prompt is inactive | Status indicator updated |
| 4 | Click to reactivate | Status changes to Active |
| 5 | Verify prompt is active again | Status indicator updated |

---

### POS_008 - Preview AI Prompt
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Preview

**Objective:** Validate previewing how a prompt will appear to users.

**Preconditions:**
- AI prompt exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate a prompt | Prompt displayed |
| 2 | Click preview button | Preview modal opens |
| 3 | Verify prompt content displayed | Full prompt text visible |
| 4 | Verify variable placeholders highlighted | Variables like {partnerName} are visible |
| 5 | Close preview | Modal closes |

---

### NEG_009 - Validate Required Fields on Create
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Validation

**Objective:** Validate that required fields are enforced when creating a prompt.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click "Add New Prompt" | Create form opens |
| 2 | Leave Name field empty | Field is blank |
| 3 | Attempt to save | Validation error on Name field |
| 4 | Leave Prompt Text empty | Field is blank |
| 5 | Attempt to save | Validation error on Prompt Text field |

---

### POS_010 - Search AI Prompts
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Search

**Objective:** Validate searching for prompts by name or category.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to AI Prompts page | List displayed |
| 2 | Enter search term in search field | Results filter |
| 3 | Verify matching prompts displayed | Only matching prompts shown |
| 4 | Clear search | All prompts displayed |

**Test Data:**
- Search Term: "Partner"

---

### POS_011 - Filter AI Prompts by Category
**Priority:** Normal  
**Labels:** Administration, AI_Prompts, Filter

**Objective:** Validate filtering prompts by category.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate category filter | Filter dropdown visible |
| 2 | Select "Partners" category | List filters |
| 3 | Verify only Partner-related prompts shown | Filter correctly applied |
| 4 | Select "All Categories" | All prompts displayed |

---

### POS_012 - Duplicate AI Prompt
**Priority:** Low  
**Labels:** Administration, AI_Prompts, Duplicate

**Objective:** Validate duplicating an existing prompt as a starting point.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Locate existing prompt | Prompt displayed |
| 2 | Click "Duplicate" option | Create form opens with copied content |
| 3 | Verify name is modified (e.g., "Copy of...") | New name populated |
| 4 | Verify prompt text copied | Same prompt content |
| 5 | Modify and save | New prompt created |

---

### NEG_013 - Non-Admin Cannot Access AI Prompts
**Priority:** High  
**Labels:** Administration, AI_Prompts, Security

**Objective:** Validate non-administrators cannot access AI Prompts management.

**Preconditions:**
- Logged in as non-Administrator

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Attempt to navigate to AI Prompts | Access denied |
| 2 | Try direct URL access | Redirected or 403 error |
| 3 | Verify menu item is hidden | AI Prompts not in menu |

---

### POS_014 - Version History for AI Prompts
**Priority:** Low  
**Labels:** Administration, AI_Prompts, History

**Objective:** Validate viewing version history of prompt changes.

**Preconditions:**
- Prompt has been edited multiple times

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open prompt details | Prompt displayed |
| 2 | Click "View History" | Version history modal opens |
| 3 | Verify previous versions listed | Each edit shown with timestamp |
| 4 | Click on previous version | Previous content displayed |
| 5 | Optionally restore previous version | If supported, version is restored |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 5 |
| Normal | 7 |
| Low | 2 |
| **TOTAL** | **14** |
