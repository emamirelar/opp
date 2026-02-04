# AI Assistant Test Cases

## Overview
Test cases for the AI Assistant feature in the UNOPS Opportunity+ system.

**JIRA Story:** PNO-374  
**Last Updated:** 2026-02-04  
**Total Test Cases:** 15

---

## Test Cases

### POS_001 - Validate AI Assistant Accessibility
**Priority:** High  
**Labels:** AI, Assistant, UI

**Objective:** Validate that the AI Assistant is accessible from the main application interface.

**Preconditions:**
- User is logged in with Partner User role

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Navigate to the Opportunity+ homepage | Homepage loads |
| 2 | Locate the AI Assistant icon/button | AI Assistant icon is visible in toolbar/sidebar |
| 3 | Click the AI Assistant icon | AI Assistant panel/dialog opens |
| 4 | Verify AI Assistant is ready for input | Input field is visible and active |

---

### POS_002 - Ask AI About Partners
**Priority:** High  
**Labels:** AI, Assistant, Partners

**Objective:** Validate that the AI Assistant can answer questions about partners.

**Preconditions:**
- Partner records exist in the system
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open AI Assistant | Assistant panel opens |
| 2 | Enter query: "Show me all Funding Partners" | AI processes the query |
| 3 | Review AI response | AI returns a list of Funding Partners |
| 4 | Verify response accuracy | Partners match actual Funding Partners in system |

**Test Data:**
- Query: "Show me all Funding Partners"

---

### POS_003 - Ask AI About Opportunities
**Priority:** High  
**Labels:** AI, Assistant, Opportunities

**Objective:** Validate that the AI Assistant can answer questions about opportunities.

**Preconditions:**
- Opportunity records exist in the system
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open AI Assistant | Assistant panel opens |
| 2 | Enter query: "What opportunities are in Draft status?" | AI processes the query |
| 3 | Review AI response | AI returns opportunities in Draft status |
| 4 | Verify response accuracy | Listed opportunities are actually in Draft status |

**Test Data:**
- Query: "What opportunities are in Draft status?"

---

### POS_004 - AI Provides Navigation Assistance
**Priority:** Normal  
**Labels:** AI, Assistant, Navigation

**Objective:** Validate that the AI Assistant can help users navigate to specific pages.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter query: "Take me to the Partners list" | AI processes the query |
| 2 | Review AI response | AI provides navigation link or auto-navigates |
| 3 | Click provided link (if applicable) | User is navigated to Partners list |

**Test Data:**
- Query: "Take me to the Partners list"

---

### POS_005 - AI Keyword Search Across Entities
**Priority:** High  
**Labels:** AI, Assistant, Search

**Objective:** Validate that the AI Assistant can search across multiple entity types.

**Preconditions:**
- Data exists across Partners, Opportunities, Contacts
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter query: "Find everything related to 'Climate'" | AI processes the query |
| 2 | Review AI response | AI returns results from Partners, Opportunities, and other entities mentioning "Climate" |
| 3 | Verify cross-entity search | Results include matches from different entity types |

**Test Data:**
- Query: "Find everything related to 'Climate'"

---

### POS_006 - AI Conversation History
**Priority:** Normal  
**Labels:** AI, Assistant, History

**Objective:** Validate that the AI Assistant maintains conversation context.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Ask initial question: "Show me Opportunities in Africa" | AI responds with list |
| 2 | Ask follow-up: "Which of these are Active?" | AI filters previous results to Active only |
| 3 | Verify context is maintained | AI correctly references previous query |

**Test Data:**
- Query 1: "Show me Opportunities in Africa"
- Query 2: "Which of these are Active?"

---

### POS_007 - AI Provides Help Information
**Priority:** Normal  
**Labels:** AI, Assistant, Help

**Objective:** Validate that the AI Assistant can provide help on using the system.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter query: "How do I create a new Opportunity?" | AI processes the query |
| 2 | Review AI response | AI provides step-by-step instructions |
| 3 | Verify instructions are accurate | Steps align with actual system workflow |

**Test Data:**
- Query: "How do I create a new Opportunity?"

---

### NEG_008 - AI Handles Unknown Queries
**Priority:** Normal  
**Labels:** AI, Assistant, Negative

**Objective:** Validate that the AI Assistant gracefully handles queries it cannot answer.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter an unrelated or nonsensical query | AI processes the query |
| 2 | Review AI response | AI indicates it cannot answer or asks for clarification |
| 3 | Verify no error is thrown | System remains stable |

**Test Data:**
- Query: "What is the airspeed velocity of an unladen swallow?"

---

### NEG_009 - AI Respects User Permissions
**Priority:** High  
**Labels:** AI, Assistant, Security

**Objective:** Validate that the AI Assistant only returns data the user is authorized to see.

**Preconditions:**
- User has limited permissions (not admin)
- Restricted data exists in system

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Log in as user with restricted access | Login successful |
| 2 | Open AI Assistant | Assistant opens |
| 3 | Query for data user shouldn't see | AI processes query |
| 4 | Verify response | AI only returns data user is authorized to view |

---

### POS_010 - AI Voice Input (if available)
**Priority:** Low  
**Labels:** AI, Assistant, Voice

**Objective:** Validate voice input functionality for the AI Assistant.

**Preconditions:**
- Voice input is enabled
- Microphone access granted

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open AI Assistant | Assistant opens |
| 2 | Click voice input button | Microphone activates |
| 3 | Speak query: "Show me active opportunities" | Voice is transcribed |
| 4 | Verify transcription | Query is accurately transcribed |
| 5 | Review AI response | AI responds to the spoken query |

---

### POS_011 - AI Response Copy Functionality
**Priority:** Normal  
**Labels:** AI, Assistant, UI

**Objective:** Validate that AI responses can be copied to clipboard.

**Preconditions:**
- AI has returned a response

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Ask AI a question | AI responds |
| 2 | Locate copy button on response | Copy icon is visible |
| 3 | Click copy button | Response is copied to clipboard |
| 4 | Paste in external application | Content matches AI response |

---

### POS_012 - AI Response Feedback
**Priority:** Normal  
**Labels:** AI, Assistant, Feedback

**Objective:** Validate that users can provide feedback on AI responses.

**Preconditions:**
- AI has returned a response

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Ask AI a question | AI responds |
| 2 | Locate feedback buttons (thumbs up/down) | Feedback icons are visible |
| 3 | Click thumbs up | Positive feedback recorded, visual confirmation |
| 4 | Ask another question and click thumbs down | Negative feedback recorded, optional feedback form appears |

---

### NEG_013 - AI Handles Long Queries
**Priority:** Low  
**Labels:** AI, Assistant, Edge

**Objective:** Validate that the AI Assistant handles very long queries appropriately.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter a very long query (500+ characters) | Query is entered |
| 2 | Submit query | AI processes or indicates query is too long |
| 3 | Verify system stability | No errors or crashes |

---

### POS_014 - AI Minimizes and Restores
**Priority:** Normal  
**Labels:** AI, Assistant, UI

**Objective:** Validate that the AI Assistant panel can be minimized and restored.

**Preconditions:**
- AI Assistant is open

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open AI Assistant | Assistant panel opens |
| 2 | Click minimize button | Panel minimizes to icon |
| 3 | Continue using application | Application functions normally with minimized assistant |
| 4 | Click to restore AI Assistant | Panel opens with previous conversation preserved |

---

### POS_015 - AI Clear Conversation
**Priority:** Normal  
**Labels:** AI, Assistant, UI

**Objective:** Validate that the conversation history can be cleared.

**Preconditions:**
- AI Assistant has existing conversation

**Test Steps:**
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open AI Assistant with existing conversation | Conversation history visible |
| 2 | Click "Clear" or "New Conversation" button | Confirmation prompt appears |
| 3 | Confirm clear action | Conversation is cleared |
| 4 | Verify clean state | AI Assistant shows empty conversation state |

---

## Summary

| Priority | Count |
|----------|-------|
| High | 5 |
| Normal | 8 |
| Low | 2 |
| **TOTAL** | **15** |
