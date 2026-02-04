# Opportunity Creation Test Cases

## Overview
Test cases for creating Opportunities from different entry points within the UNOPS Opportunity+ system.

**JIRA Stories:** PNO-687, PNO-688, PNO-689  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 19

---

## 1. Opportunity Creation from Partners Page (PNO-687)

### POS_001 - Validate Opportunity Creation Button on Active Partner
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that the "Create a New Opportunity" button is present and clickable on an active Partner record.

**Preconditions:**
- User Role: Partner User
- Active Partner Account exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an Active Partner Account record | The "Create a New Opportunity" button/link is clearly visible |
| 3 | Click the "Create a New Opportunity" button/link | System navigates to New Opportunity creation screen/form |
| 4 | Verify screen displays fields for Opportunity Name and Partner Role | Opportunity creation form loads successfully |

---

### NEG_002 - Validate that Opportunity cannot be created on Closed Partner
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that a warning message is displayed when attempting to create an opportunity on a Closed Partner account.

**Preconditions:**
- User Role: Partner User
- A Partner Account with 'Closed' status exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to a Closed Partner Account record | The "Create a New Opportunity" button is hidden/disabled or displays warning on click |
| 3 | Attempt to create a new opportunity | Warning displayed: "Creation of opportunities is not possible if a partner account is closed or archived" |
| 4 | Verify no new opportunity is created | User remains on Partner Account page, no opportunity record logged |

---

### POS_003 - Create New Opportunity from Partner Page
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate successful creation of an opportunity, ensuring mandatory fields (Name, Partner Role) are provided and auto-populated fields are correct.

**Preconditions:**
- User Role: Partner User
- Active Partner Account exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to an Active Partner Account and initiate New Opportunity creation | Opportunity creation form opens |
| 3 | Enter Opportunity Name, Description, Partner Type | User can input data into the fields |
| 4 | Click Save/Create Record | Record saved successfully, navigates to Opportunity detail page |
| 5 | Check Opportunity detail page for auto-populated fields | Partner Name, Partner ID, Opportunity Manager, Creator fields correctly populated |

**Test Data:**
- Opportunity Name: "Funding Project 2025"
- Description: Text of length 255 chars
- Partner Type: Funding/Client/Both

---

### NEG_004 - Validate that Opportunity cannot be created if mandatory field is not filled
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that saving is prevented if the Opportunity Name is missing.

**Preconditions:**
- User Role: Partner User
- Active Partner Account exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to New Opportunity creation screen | Opportunity creation form opens |
| 3 | Select a Partner Type but leave Opportunity Name blank | Field remains blank |
| 4 | Attempt to Save/Create Record | Validation error displayed indicating Opportunity Name is mandatory |

---

### B&L_005 - Validate Max Length for Opportunity Name (255 chars)
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that an Opportunity Name entered exactly at the maximum length (255 characters) is accepted.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to New Opportunity creation screen | Opportunity creation form opens |
| 3 | Enter Opportunity Name exactly 255 characters long | Input accepted without error |
| 4 | Save the record | Record saves successfully, full 255-character name visible on detail page |
| 5 | Verify record visible in Opportunities module | Opportunity successfully retrieved in search/list views |

---

### B&L_006 - Validate Name Length Exceeded (256 chars)
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that the system rejects an Opportunity Name exceeding 255-character limit.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to New Opportunity creation screen | Opportunity creation form opens |
| 3 | Enter Opportunity Name exactly 256 characters long | Input limited or error displayed on 256th character |
| 4 | Attempt to Save the record | Validation error displayed, record NOT saved |

---

### POS_007 - Validate Visibility from Partner Account and Module
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Visibility

**Objective:** Validate that a newly created opportunity is visible in both the main Opportunities module and the associated Partner Account record.

**Preconditions:**
- Opportunity from TC #3 exists
- User is Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Opportunities Module list view | Newly created opportunity visible in the list |
| 2 | Navigate back to Partner Account record used for creation | Partner Account detail page loads |
| 3 | Check related list/tab for Opportunities | Newly created opportunity listed under related Opportunities section |

---

### POS_008 - Validate AI Assistant Retrieval by Name Keyword
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, AI_Assistant

**Objective:** Validate that the AI Assistant can retrieve the opportunity using keywords from its name.

**Preconditions:**
- Opportunity from TC #3 exists
- User has access to AI Assistant

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Access the AI Assistant interface | Assistant is ready for input |
| 3 | Ask assistant to find opportunity using a keyword from the name | AI Assistant returns the Opportunity record in results |
| 4 | Verify opportunity record is correct | Returned record matches created opportunity |

**Test Data:**
- AI Query: "Show me opportunities with 'Funding' in the name."

---

### POS_009 - Validate AI Assistant Retrieval by Description Keyword
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, AI_Assistant

**Objective:** Validate that the AI Assistant can retrieve the opportunity using keywords from its description field.

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Create new Opportunity with unique description keyword | Record successfully created |
| 3 | Access AI Assistant and query using description keyword | AI Assistant returns the Opportunity record |
| 4 | Verify returned record is correct | Returned record matches "Test AI Search 1" |

**Test Data:**
- Opportunity Name: "Test AI Search 1"
- Description: "Mandate: Infrastructure improvement project."
- AI Query: "Find opportunities mentioning 'Infrastructure'."

---

### NEG_010 - Validate user not having 'Partner User' role cannot create a new Opportunity
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Permission

**Objective:** Validate that users with roles apart from 'Partner User' are not allowed to create a new Opportunity.

**Preconditions:**
- User Role: Not a Partner User (GENUSER, ORG UNIT ADMIN)

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to a Partner Account record | "Create a New Opportunity" button hidden/disabled or shows warning on click |
| 3 | Verify no opportunity can be created | No new opportunity record logged in system |

---

## 2. Opportunity Creation from Interactions (PNO-688)

### POS_001 - Validate Creation from Single Interaction
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Interaction

**Objective:** Validate that a user can select a single interaction related to a partner and successfully initiate the "Create new Opportunity" process.

**Preconditions:**
- User is Partner User
- Partner Account exists with at least one linked Interaction

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to Partner record and select one Interaction | Interaction is highlighted/selected |
| 3 | Click "Create new Opportunity" option | New Opportunity form loads with auto-populated fields based on interaction content |
| 4 | Verify proposed content highlights text from selected interaction | Relevant content from interaction visible and highlighted |

**Test Data:**
- Interaction ID: Int_A
- Content: "Meeting discussion on renewable energy project."

---

### POS_002 - Validate Creation from Multiple Interactions
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Interaction

**Objective:** Validate that a user can select multiple interactions and proceed to create a new Opportunity, combining proposed content from all sources.

**Preconditions:**
- User is Partner User
- Partner Account exists with multiple linked Interactions

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to Partner record and select two or more Interactions | Interactions are highlighted/selected |
| 3 | Click "Create new Opportunity" option | New Opportunity form loads |
| 4 | Verify proposed content highlights relevant text from both selected interactions | Content from both interactions visible and highlighted |

**Test Data:**
- Interaction ID: Int_B (focus on water sanitation)
- Interaction ID: Int_C (focus on funding)

---

### NEG_003 - Validate Mandatory Name/Description Check
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Mandatory

**Objective:** Validate that the system requires a Name and Description for the new Opportunity even when created from Interactions.

**Preconditions:**
- User is Partner User
- One Interaction is selected

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Select an Interaction and proceed to New Opportunity form | Proposed content is visible |
| 3 | Attempt to save without providing required Name and Description | Validation error displayed indicating mandatory fields |
| 4 | Verify record is not saved | Record ID field remains empty |

---

### POS_004 - Validate Document Upload and Content Proposal
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Documents

**Objective:** Validate that a user can upload a new document during opportunity creation and the system attempts to derive proposed content.

**Preconditions:**
- User is Partner User
- One Interaction selected
- Sample document file available

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Initiate New Opportunity creation from Interaction | Form loads |
| 3 | Upload an additional document via dedicated upload feature | Document successfully uploaded and processed |
| 4 | Verify proposed content includes relevant text from uploaded document | Content from document visible alongside interaction content |

**Test Data:**
- Document: "Proposal.pdf" (contains keyword: "Solar Array")

---

### POS_005 - Accept/Reject All Proposed Content (Bulk Accept/Reject)
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Proposal

**Objective:** Validate that the user can accept or reject all proposed content in bulk to save the record.

**Preconditions:**
- User is Partner User
- Form contains multiple items of proposed content/properties

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Initiate New Opportunity creation from Interaction | Form loads with proposed content and bulk options |
| 3 | Click "Accept All" or "Reject All" button | All proposed fields moved to final accepted/rejected state |
| 4 | Fill mandatory Name/Description and Save | Record saved successfully with accepted/rejected content |

---

### POS_006 - Accept/Reject Content Per Property (Selective)
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Proposal

**Objective:** Validate that the user can selectively reject/accept proposed content for individual properties.

**Preconditions:**
- User is Partner User
- Form contains proposed content for multiple properties

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Initiate New Opportunity creation from Interaction | Form loads with proposed content |
| 3 | Accept/Reject Target Market proposal and/or Funding Source proposal | Fields moved to accepted/rejected state individually |
| 4 | Fill mandatory fields and Save | Record saved successfully reflecting accepted values |

**Test Data:**
- Proposed Properties: Target Market (Proposed: Asia), Funding Source (Proposed: UN)

---

### NEG_008 - Attempt Save Without Accepting/Rejecting All Proposals
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation, Negative

**Objective:** Validate that the user is prevented from saving if there is unaddressed proposed content.

**Preconditions:**
- User is Partner User
- Form contains at least one item of proposed content not acted upon

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Initiate New Opportunity creation from Interaction | Form loads with proposed content |
| 3 | Fill mandatory Name/Description but do not accept/reject for one proposed property | Proposed property remains highlighted/uncommitted |
| 4 | Attempt to Save | Validation error displayed indicating all proposed content must be accepted/rejected |

---

## 3. Opportunity Creation from Opportunity Page (PNO-689)

### POS_001 - Validate Create New Opportunity button visibility on Opportunity Page
**Priority:** Normal  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate that the "Create a New Opportunity" button is present on Opportunity Page and clickable for authorized personnel.

**Preconditions:**
- User is Partner User

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to 'Opportunity' module/tab | Opportunities Module page/list view loads |
| 3 | Verify presence of "Create a new Opportunity" button/link | Button or link clearly visible |
| 4 | Click "Create a new Opportunity" button | System navigates to Opportunity Creation screen/form |

---

### POS_002 - Create New Opportunity from Opportunity Page
**Priority:** High  
**Labels:** Partners_&_Opportunities, Opportunity, Creation

**Objective:** Validate successful creation of an opportunity from Opportunity page, ensuring mandatory field (Name) is provided and auto-populated fields are correct.

**Preconditions:**
- User is Partner User
- Active Partner Account exists

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to Partners & Opportunities URL | User lands on the homepage |
| 2 | Navigate to 'Opportunity' module/tab | Opportunities Module page/list view loads |
| 3 | Click "Create a new Opportunity" button | System navigates to Opportunity Creation screen/form |
| 4 | Enter Opportunity Name, Description, Partner Type | User can input data |
| 5 | Click Save/Create Record | Record saved successfully, navigates to Opportunity detail page |
| 6 | Check Opportunity detail page for auto-populated fields | Partner Name, Partner ID, Opportunity Manager, Creator correctly populated |

**Test Data:**
- Opportunity Name: "Funding Project 2025"
- Description: Text of length 255 chars
- Partner Type: Funding/Client/Both

---

## Summary

| Category | Total Tests | High Priority | Normal Priority |
|----------|-------------|---------------|-----------------|
| From Partners Page (PNO-687) | 10 | 4 | 6 |
| From Interactions (PNO-688) | 7 | 4 | 3 |
| From Opportunity Page (PNO-689) | 2 | 1 | 1 |
| **TOTAL** | **19** | **9** | **10** |
