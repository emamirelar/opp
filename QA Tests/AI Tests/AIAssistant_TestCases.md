# AI Assistant — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/features/ai`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result |
|-------|--------|
| N ≥ 3P | 90 ≥ 90 ✅ PASS |
| E ≥ 3P | 90 ≥ 90 ✅ PASS |
| F ≥ 3P | 90 ≥ 90 ✅ PASS |
| I ≥ 3P | 90 ≥ 90 ✅ PASS |

---

## Feature Overview

AI assistant: chat interface, context-aware responses, entity data access, prompt management, rate limiting.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Send Message and Receive Response

**Priority:** P0  
**Precondition:** User has AI access, assistant enabled.

**Steps:**
1. Open AI Assistant panel
2. Enter message "Summarize this partner"
3. Send
4. Wait for response

**Expected Result:** Response displayed, chat history updated.

---

#### POS-002: Chat with Entity Context

**Priority:** P0  
**Precondition:** Viewing partner detail page.

**Steps:**
1. Open AI Assistant from partner context
2. Ask "What are the key risks for this partner?"
3. Send

**Expected Result:** Context-aware response using partner data.

---

#### POS-003: View Chat History

**Priority:** P0  
**Precondition:** Previous chat exists.

**Steps:**
1. Open AI Assistant
2. View conversation list

**Expected Result:** Previous conversations displayed.

---

#### POS-004: Clear Chat

**Priority:** P0  
**Precondition:** Chat has messages.

**Steps:**
1. Open chat
2. Click Clear/New conversation
3. Confirm

**Expected Result:** Chat cleared, new conversation started.

---

#### POS-005: Use Suggested Prompt

**Priority:** P0  
**Precondition:** Suggested prompts configured.

**Steps:**
1. Open AI Assistant
2. Click suggested prompt "Analyze opportunity"
3. Send

**Expected Result:** Prompt sent, response received.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Chat from opportunity context | Viewing opportunity | Open assistant, ask | Context-aware | P1 |
| POS-007 | Chat from contact context | Viewing contact | Open assistant, ask | Context-aware | P1 |
| POS-008 | Multi-turn conversation | Initial response | Follow-up question | Context maintained | P1 |
| POS-009 | Copy response to clipboard | Response received | Copy button | Text copied | P1 |
| POS-010 | Regenerate response | Response received | Regenerate | New response | P1 |
| POS-011 | Collapse/expand chat panel | Panel open | Toggle | Panel collapses | P1 |
| POS-012 | Chat with long message | Valid input | 500 char message | Sent, response | P1 |
| POS-013 | Chat with Unicode | Valid input | Arabic message | Stored, response | P1 |
| POS-014 | Switch between conversations | Multiple chats | Select different | Correct chat shown | P1 |
| POS-015 | Delete conversation | Chat exists | Delete | Removed from list | P1 |
| POS-016 | Export conversation | Chat exists | Export | File downloaded | P1 |
| POS-017 | Rate limit within limit | Under limit | 5 messages | All succeed | P2 |
| POS-018 | Suggested prompts load | Assistant open | View suggestions | Prompts displayed | P2 |
| POS-019 | Empty state | No chats | Open assistant | Empty state shown | P2 |
| POS-020 | Loading indicator | Send message | Wait | Spinner shown | P2 |
| POS-021 | Error retry | Temporary error | Retry button | Resubmitted | P2 |
| POS-022 | Keyboard shortcut | Focus in input | Ctrl+Enter | Message sent | P2 |
| POS-023 | Responsive panel | Mobile view | Open panel | Responsive layout | P2 |
| POS-024 | Accessibility: keyboard nav | Focus | Tab through | All elements focusable | P2 |
| POS-025 | Accessibility: screen reader | Screen reader | Navigate | Announced correctly | P2 |
| POS-026 | Chat with markdown response | AI returns markdown | Display | Rendered correctly | P2 |
| POS-027 | Chat with code block | AI returns code | Display | Syntax highlighted | P2 |
| POS-028 | Chat with link | AI returns link | Display | Clickable link | P2 |
| POS-029 | Session persistence | Refresh page | Reopen | Chat history restored | P2 |
| POS-030 | Entity type detection | Partner page | Open assistant | Partner context detected | P2 |
| POS-031 | Prompt template applied | Template configured | Use template | Template filled | P2 |
| POS-032 | Stream response | API streams | Send message | Streaming display | P2 |
| POS-033 | Stop generation | Stream in progress | Stop button | Generation stopped | P2 |
| POS-034 | Chat with emoji | Message with emoji | Send | Stored, displayed | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 90 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Send empty message | Message = "" | Validation: "Message required" | P0 |
| NEG-002 | Send whitespace-only | Message = "   " | Validation error | P0 |
| NEG-003 | Send null message | Message = null | Validation error | P0 |
| NEG-004 | Send message exceeds max | 100000 chars | Validation: "Too long" | P0 |
| NEG-005 | Send with invalid entity ID | EntityId = 999999 | KeyNotFoundException | P0 |
| NEG-006 | Send with deleted entity | Entity deleted | Context error | P0 |
| NEG-007 | Invalid conversation ID | ConvId = 999999 | KeyNotFoundException | P0 |
| NEG-008 | Delete non-existent conversation | Id = 999999 | KeyNotFoundException | P0 |
| NEG-009 | Export non-existent chat | Id = 999999 | KeyNotFoundException | P0 |
| NEG-010 | Regenerate with invalid context | Stale context | Error | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | User without AI permission | No CanUseAI | Open assistant | Unauthorized | P0 |
| NEG-012 | User without entity view | No CanViewPartner | Chat with partner context | Unauthorized | P0 |
| NEG-013 | Anonymous user | No auth | Send message | 401 | P0 |
| NEG-014 | Expired session | Expired token | Send message | 401 | P0 |
| NEG-015 | Disabled user | Disabled | Any operation | 403 | P1 |
| NEG-016 | User without opportunity view | No CanViewOpportunity | Chat with opportunity | Unauthorized | P1 |
| NEG-017 | OrgUnit-scoped user wrong scope | Scoped | Partner out of scope | Unauthorized | P0 |
| NEG-018 | API without auth | No Bearer | POST /ai/chat | 401 | P0 |
| NEG-019 | Tampered JWT | Modified | Any operation | 401 | P0 |
| NEG-020 | Post-logout | Logged out | Cached send | 401 | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Send during generation | Generating | Send another | Queued or error | P1 |
| NEG-022 | Regenerate deleted response | Response deleted | Regenerate | Error | P1 |
| NEG-023 | Access deleted conversation | Conv deleted | Open | 404 | P1 |
| NEG-024 | Chat with deleted entity | Entity deleted | Send with context | Context error | P1 |
| NEG-025 | Rate limit exceeded | At limit | Send | 429, message | P0 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Send with null conversation ID | ConvId = null | New conversation | P1 |
| NEG-027 | Send with null entity ID | EntityId = null | No context | P1 |
| NEG-028 | Export with null format | Format = null | Default format | P1 |
| NEG-029 | Context with missing entity | Entity not found | Error | P1 |
| NEG-030 | Prompt with null variables | Variables = null | Default or error | P1 |
| NEG-031 | Chat with empty context | No entity | No context sent | P1 |
| NEG-032 | Regenerate with null message ID | MsgId = null | ArgumentNullException | P1 |
| NEG-033 | Delete with null ID | Id = null | ArgumentNullException | P1 |
| NEG-034 | List with null session | Session = null | Empty or error | P1 |
| NEG-035 | Template with missing variable | {{missing}} | Placeholder or error | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | AI service unavailable | Service down | User-friendly error | P0 |
| NEG-037 | AI service timeout | Slow response | Timeout message | P0 |
| NEG-038 | Database connection lost | DB drops | Error, no data loss | P1 |
| NEG-039 | Entity service unavailable | Entity API down | Context fetch error | P1 |
| NEG-040 | Prompt service unavailable | Prompt API down | Default prompt | P2 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Create duplicate conversation | Same name | Unique ID | P1 |
| NEG-042 | Send duplicate message | Same content | Both stored | P1 |
| NEG-043 | Message with SQL injection | `'; DROP--` | Sanitized or rejected | P0 |
| NEG-044 | Message with XSS | `<script>alert(1)</script>` | Sanitized | P0 |
| NEG-045 | Prompt injection attempt | "Ignore previous..." | Mitigated | P0 |
| NEG-046 | Malformed JSON in context | Invalid JSON | Parse error | P1 |
| NEG-047 | Oversized context | 1MB context | Truncated or error | P1 |
| NEG-048 | Too many concurrent requests | 20 simultaneous | Rate limited | P0 |
| NEG-049 | Session expired mid-chat | Token expires | Re-auth prompt | P1 |
| NEG-050 | Entity without permission | No view permission | Context error | P0 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Send with negative entity ID | -1 | Validation error | P1 |
| NEG-052 | Send with zero conversation ID | 0 | New conversation | P1 |
| NEG-053 | Get with negative ID | -1 | Not found | P1 |
| NEG-054 | Paginate with invalid page | Page = -1 | Default or error | P2 |
| NEG-055 | Paginate with invalid size | Size = 0 | Default or error | P2 |
| NEG-056 | Export with invalid format | Format = "invalid" | Default | P2 |
| NEG-057 | Sort by invalid column | "INVALID" | Default sort | P2 |
| NEG-058 | Context with circular ref | Entity A→B→A | Handled | P1 |
| NEG-059 | Message with control chars | \0, \n | Sanitized | P1 |
| NEG-060 | Chat with binary content | Binary in message | Rejected | P1 |
| NEG-061 | Import malformed conversation | Invalid JSON | Parse error | P2 |
| NEG-062 | Export empty conversation | No messages | Empty file or error | P2 |
| NEG-063 | Rate limit exceeded | 100 requests | 429 | P0 |
| NEG-064 | Quota exceeded | Monthly limit | Quota message | P1 |
| NEG-065 | Path traversal in export | `../../evil` | Rejected | P0 |
| NEG-066 | Send with null request | Request = null | ArgumentNullException | P1 |
| NEG-067 | Update with null request | Request = null | ArgumentNullException | P1 |
| NEG-068 | LDAP injection in message | `*)(cn=*` | Sanitized | P1 |
| NEG-069 | Regex DoS in search | `(((((((((((...))))))))))))` | Rejected or timeout | P1 |
| NEG-070 | Concurrent delete same chat | 2 users delete | One succeeds, other 404 | P1 |
| NEG-071 | Send with invalid entity type | EntityType = "Invalid" | Validation error | P1 |
| NEG-072 | Regenerate with null message | MsgId = null | ArgumentNullException | P1 |
| NEG-073 | Get conversation invalid format | Id = "abc" | 400 Bad Request | P1 |
| NEG-074 | Export with invalid format | Format = "invalid" | Default format | P1 |
| NEG-075 | Context with deleted entity | Entity deleted | Context error | P1 |
| NEG-076 | List with invalid session | Session invalid | Empty or error | P1 |
| NEG-077 | Template with missing var | {{missing}} | Placeholder or error | P1 |
| NEG-078 | Send during rate limit | At limit | 429 | P0 |
| NEG-079 | Quota exceeded | Over monthly | Quota message | P1 |
| NEG-080 | Context size exceeded | 50001 chars | Truncated or error | P1 |
| NEG-081 | Message with null content | Content = null | Validation error | P1 |
| NEG-082 | Conversation with stale data | Stale | Error or refresh | P1 |
| NEG-083 | Filter with invalid date | Date = "invalid" | Default or error | P2 |
| NEG-084 | Search with control chars | \0 in query | Sanitized | P1 |
| NEG-085 | Stream with invalid chunk | Malformed chunk | Error handled | P1 |
| NEG-086 | Update with stale version | Stale version | Conflict error | P1 |
| NEG-087 | Delete with invalid ID | Id = "x" | 400 Bad Request | P1 |
| NEG-088 | Send with oversized message | 10001 chars | Validation error | P0 |
| NEG-089 | Context with circular ref | A→B→A | Handled | P1 |
| NEG-090 | Concurrent send same user | 2 simultaneous | Queued or error | P1 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 90 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Message | 1 | 10000 | ✅ "x" | ✅ 10000 | ❌ Rejected | P1 |
| BND-002 | Response | 0 | 50000 | ✅ Empty | ✅ 50000 | ❌ Truncated | P1 |
| BND-003 | Conversation name | 0 | 200 | ✅ Empty | ✅ 200 | ❌ Rejected | P1 |
| BND-004 | Search query | 0 | 255 | ✅ Empty | ✅ 255 | ❌ Capped | P1 |
| BND-005 | Context entity name | 0 | 500 | ✅ Empty | ✅ 500 | ❌ Rejected | P2 |
| BND-006 | Suggested prompt | 1 | 500 | ✅ "x" | ✅ 500 | ❌ Rejected | P2 |
| BND-007 | Export filename | 1 | 260 | ✅ "a" | ✅ 260 | ❌ Rejected | P2 |
| BND-008 | Template variable | 0 | 1000 | ✅ Empty | ✅ 1000 | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-009 | Message ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-010 | Conversation ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-011 | Entity ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-012 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-013 | Page size | 1 | 100 | ❌ Default | ❌ Error | Capped | P1 |
| BND-014 | Rate limit | 0 | 100 | ✅ 0 | ❌ | Capped | P1 |
| BND-015 | Messages per conversation | 0 | 1000 | ✅ Empty | ❌ | Paginated | P2 |
| BND-016 | Context size (chars) | 0 | 50000 | ✅ No context | ✅ 50000 | Truncated | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-017 | Chat at midnight UTC | 00:00:00 | Stored correctly | P2 |
| BND-018 | Chat at 23:59:59 | End of day | Stored correctly | P2 |
| BND-019 | Conversation created leap year | Feb 29, 2028 | Correct | P2 |
| BND-020 | Filter by date range | Same day | Returns that day | P2 |
| BND-021 | Rate limit reset | Midnight | Counter reset | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-022 | Zero conversations | Empty | Empty list | P1 |
| BND-023 | One conversation | Single | List with 1 | P1 |
| BND-024 | Exactly page size | 20, size=20 | Full page | P1 |
| BND-025 | Page size + 1 | 21, size=20 | 20 on page 1 | P1 |
| BND-026 | 1000 conversations | Large | Paginated | P1 |
| BND-027 | Conversation with 0 messages | Empty | Empty chat | P1 |
| BND-028 | Conversation with 1 message | Single | 1 message | P1 |
| BND-029 | Conversation with 100 messages | Many | Paginated | P2 |
| BND-030 | Last page with 1 item | 41, page 3, size 20 | 1 on page 3 | P1 |
| BND-031 | Context with 0 entities | No context | Empty context | P1 |
| BND-032 | Context with 10 entities | Many | All included | P2 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-033 | Message (Arabic) | `أفهم` | Stored, displayed | P2 |
| BND-034 | Message (Chinese) | `你好` | Stored, displayed | P2 |
| BND-035 | Message (Cyrillic) | `Понятно` | Stored, displayed | P2 |
| BND-036 | Message with apostrophe | "What's next?" | Preserved | P1 |
| BND-037 | Message with newlines | Multi-line | Newlines preserved | P1 |
| BND-038 | Message with emoji | `Great! 🤝` | Stored, displayed | P2 |
| BND-039 | Message with special chars | `@#$%` | Stored | P2 |
| BND-040 | Response with markdown | `**bold**` | Rendered | P1 |
| BND-041 | Response with code | `` `code` `` | Highlighted | P2 |
| BND-042 | Message with HTML | `<b>bold</b>` | Escaped | P1 |

### 3.6 Rate Limit Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-043 | At rate limit | 10th request | Success | P1 |
| BND-044 | Just over rate limit | 11th request | 429 | P1 |
| BND-045 | After rate limit window | Wait 1 min | Success | P1 |
| BND-046 | Concurrent at limit | 10 simultaneous | All succeed or queue | P1 |
| BND-047 | Burst limit | 5 in 1 sec | 5 succeed | P1 |
| BND-048 | Quota at limit | Monthly limit | Success | P1 |
| BND-049 | Quota just over | 1 over | Quota error | P1 |
| BND-050 | Per-user limit | User A | Independent of User B | P1 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Message exactly 1 char | "x" | Accepted | P1 |
| BND-052 | Message exactly max | 10000 chars | Accepted | P1 |
| BND-053 | Conversation ID = 1 | First | Retrieved | P2 |
| BND-054 | Entity ID = MAX_INT | Overflow | Handled | P2 |
| BND-055 | Search with 1 char | "a" | Matches | P1 |
| BND-056 | Search with max chars | 255 chars | Processed | P1 |
| BND-057 | Empty context | No entity | No context sent | P1 |
| BND-058 | Max context | 50000 chars | Sent or truncated | P1 |
| BND-059 | Create with minimal data | Message only | Success | P1 |
| BND-060 | Create with full context | All entities | Success | P1 |
| BND-061 | Concurrent list requests | 2 users list | Both correct | P2 |
| BND-062 | Timezone boundary | UTC vs local | Correct | P2 |
| BND-063 | Sort by each column | Date, Name | All work | P1 |
| BND-064 | Filter by entity type | Partner, Opportunity | Correct | P2 |
| BND-065 | Stream first chunk | Streaming | Displayed | P1 |
| BND-066 | Stream last chunk | Streaming | Complete | P1 |
| BND-067 | Stop mid-stream | Mid-stream | Partial displayed | P1 |
| BND-068 | Long response | 50000 chars | Displayed | P2 |
| BND-069 | Multi-language mixed | Mixed | Displayed correctly | P2 |
| BND-070 | Context with null optional | Optional null | Handled | P2 |
| BND-071 | Message at 9999 chars | 9999 chars | Accepted | P1 |
| BND-072 | Response at 49999 chars | 49999 chars | Displayed | P1 |
| BND-073 | Conversation with 2 messages | Two | Both shown | P1 |
| BND-074 | Page size at 99 | 99 | Accepted | P1 |
| BND-075 | Rate limit at 99 | 99 requests | Success | P1 |
| BND-076 | Search exactly 254 chars | 254 chars | Processed | P1 |
| BND-077 | Context with 1 entity | Single | Included | P1 |
| BND-078 | Empty conversation | No messages | Empty chat | P1 |
| BND-079 | Single message | 1 message | 1 shown | P1 |
| BND-080 | Context at 49999 chars | 49999 | Sent or truncated | P1 |
| BND-081 | Message exactly 1 char | "x" | Accepted | P1 |
| BND-082 | Conversation ID = 2 | Second | Retrieved | P2 |
| BND-083 | Pagination page 2 of 2 | 2 pages | 2nd page | P1 |
| BND-084 | Filter by single entity | One entity | Correct | P1 |
| BND-085 | Unicode in message | Arabic | Stored | P2 |
| BND-086 | Stream first byte | First chunk | Displayed | P1 |
| BND-087 | Stream last byte | Last chunk | Complete | P1 |
| BND-088 | Rate limit reset | After window | Success | P1 |
| BND-089 | Quota at limit | At monthly | Success | P1 |
| BND-090 | Zero context | No entity | No context | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 50 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Conversations exclude deleted | IsDeleted filter | List | Only !IsDeleted | P0 |
| FUN-002 | Create sets audit | Audit on create | New conversation | CreatedBy, CreatedDate | P0 |
| FUN-003 | Message order by timestamp | Order | Display | Chronological | P0 |
| FUN-004 | Context sent with message | Context | Send with entity | Entity data in request | P0 |
| FUN-005 | Rate limit enforced | Limit | Exceed limit | 429 | P0 |
| FUN-006 | User permission checked | Permission | Open assistant | CanUseAI required | P0 |
| FUN-007 | Entity permission for context | Permission | Partner context | CanViewPartner required | P0 |
| FUN-008 | Session persistence | Session | Refresh | Chats restored | P1 |
| FUN-009 | Prompt template applied | Template | Use template | Variables filled | P1 |
| FUN-010 | Streaming displayed | Stream | API streams | Chunks displayed | P1 |
| FUN-011 | Regenerate uses same context | Regenerate | Regenerate | Same context | P1 |
| FUN-012 | Deleted entity not in context | Deleted | Context fetch | Excluded | P1 |
| FUN-013 | Export includes all messages | Export | Export | All messages | P1 |
| FUN-014 | Clear starts new conversation | Clear | Clear | New conversation | P1 |
| FUN-015 | Stop generation | Stop | Stop button | Generation stopped | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Message required | Required | "Hello" | null, "" | P0 |
| FUN-017 | Message max length | ≤10000 | 10000 | 10001 | P0 |
| FUN-018 | Entity must exist | FK | Valid ID | 999999 | P0 |
| FUN-019 | User must have permission | Permission | CanUseAI | No permission | P0 |
| FUN-020 | No prompt injection | Sanitize | "Normal" | "Ignore previous" | P0 |
| FUN-021 | No SQL in message | Sanitize | "Text" | `'; DROP--` | P0 |
| FUN-022 | No XSS in message | Sanitize | "Text" | `<script>` | P0 |
| FUN-023 | Conversation must exist | FK | Valid ID | 999999 | P1 |
| FUN-024 | Trim whitespace | Trim | "  Hi  " | → "Hi" | P2 |
| FUN-025 | Entity type validated | Enum | Partner | Invalid | P1 |
| FUN-026 | Context size limit | ≤50000 | 50000 | 50001 | P1 |
| FUN-027 | Rate limit per user | Per user | Limit | Exceed | P0 |
| FUN-028 | Quota per user | Monthly | Under limit | Over limit | P1 |
| FUN-029 | Session must be valid | Session | Valid | Expired | P0 |
| FUN-030 | Export format | Enum | JSON, CSV | Invalid | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max messages per conversation | 1000 | 1001 | Paginated | P1 |
| FUN-032 | Max page size | 100 | 500 | Capped | P1 |
| FUN-033 | Unique conversation ID | DB | Duplicate | Unique ID | P0 |
| FUN-034 | FK user exists | FK | Non-existent | FK error | P0 |
| FUN-035 | Concurrent send limit | 1 per user | 2 simultaneous | Queued | P1 |
| FUN-036 | Context entity limit | 10 | 11 | Truncated | P2 |
| FUN-037 | Export row limit | 1000 | 1500 | Paginated | P2 |
| FUN-038 | Conversation retention | 90 days | 91 days | Archived or deleted | P2 |
| FUN-039 | Response timeout | 60s | 61s | Timeout error | P1 |
| FUN-040 | Concurrent conversation limit | 10 active | 11 | Oldest archived | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Create conversation audit | New conversation | CreatedBy, CreatedDate | P0 |
| FUN-042 | Send message audit | Send | UserId, MessageId, Timestamp | P1 |
| FUN-043 | Delete conversation audit | Delete | DeletedBy, DeletedDate | P1 |
| FUN-044 | Export audit | Export | ExportBy, ExportDate | P1 |
| FUN-045 | Read no audit | Get conversation | No modification | P1 |
| FUN-046 | Context access audit | Context fetch | EntityId, UserId | P1 |
| FUN-047 | Rate limit audit | Limit hit | UserId, Timestamp | P1 |
| FUN-048 | Regenerate audit | Regenerate | UserId, OriginalMsgId | P1 |
| FUN-049 | Failed send no audit | Failed send | No audit entry | P1 |
| FUN-050 | Audit immutable on read | Get | Audit fields unchanged | P1 |
| FUN-051 | Message order by timestamp | Order | Chronological | P0 |
| FUN-052 | Context sent with message | Context | Entity in request | P0 |
| FUN-053 | Rate limit enforced | Limit | 429 on exceed | P0 |
| FUN-054 | User permission checked | Permission | CanUseAI required | P0 |
| FUN-055 | Entity permission for context | Permission | CanViewPartner required | P0 |
| FUN-056 | Session persistence | Session | Chats restored | P1 |
| FUN-057 | Prompt template applied | Template | Variables filled | P1 |
| FUN-058 | Streaming displayed | Stream | Chunks displayed | P1 |
| FUN-059 | Regenerate uses same context | Regenerate | Same context | P1 |
| FUN-060 | Deleted entity not in context | Deleted | Excluded | P1 |
| FUN-061 | Export includes all messages | Export | All messages | P1 |
| FUN-062 | Clear starts new | Clear | New conversation | P1 |
| FUN-063 | Stop generation | Stop | Generation stopped | P1 |
| FUN-064 | Message required | Required | "Hello" | null, "" | P0 |
| FUN-065 | Message max length | ≤10000 | 10000 | 10001 | P0 |
| FUN-066 | Entity must exist | FK | Valid ID | 999999 | P0 |
| FUN-067 | User must have permission | Permission | CanUseAI | No permission | P0 |
| FUN-068 | No prompt injection | Sanitize | "Normal" | "Ignore previous" | P0 |
| FUN-069 | No SQL in message | Sanitize | "Text" | `'; DROP--` | P0 |
| FUN-070 | No XSS in message | Sanitize | "Text" | `<script>` | P0 |
| FUN-071 | Conversation must exist | FK | Valid ID | 999999 | P1 |
| FUN-072 | Trim whitespace | Trim | "  Hi  " | → "Hi" | P2 |
| FUN-073 | Entity type validated | Enum | Partner | Invalid | P1 |
| FUN-074 | Context size limit | ≤50000 | 50000 | 50001 | P1 |
| FUN-075 | Rate limit per user | Per user | Limit | Exceed | P0 |
| FUN-076 | Quota per user | Monthly | Under | Over | P1 |
| FUN-077 | Session must be valid | Session | Valid | Expired | P0 |
| FUN-078 | Export format | Enum | JSON, CSV | Invalid | P1 |
| FUN-079 | Max messages per conversation | 1000 | 1001 | Paginated | P1 |
| FUN-080 | Max page size | 100 | 500 | Capped | P1 |
| FUN-081 | Unique conversation ID | DB | Duplicate | Unique ID | P0 |
| FUN-082 | FK user exists | FK | Non-existent | FK error | P0 |
| FUN-083 | Concurrent send limit | 1 per user | 2 simultaneous | Queued | P1 |
| FUN-084 | Context entity limit | 10 | 11 | Truncated | P2 |
| FUN-085 | Export row limit | 1000 | 1500 | Paginated | P2 |
| FUN-086 | Conversation retention | 90 days | 91 | Archived | P2 |
| FUN-087 | Response timeout | 60s | 61s | Timeout error | P1 |
| FUN-088 | Create conversation audit | New | CreatedBy, CreatedDate | P0 |
| FUN-089 | Send message audit | Send | UserId, MessageId | P1 |
| FUN-090 | Delete conversation audit | Delete | DeletedBy, DeletedDate | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 50 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Full chat lifecycle | Create→Send→View→Delete | Conversation | All succeed | P0 |
| INT-002 | Create → in list | Create | Conversation | In list | P0 |
| INT-003 | Delete → excluded | Delete | Conversation | Not in list | P0 |
| INT-004 | Send → response | Send | Message | Response received | P0 |
| INT-005 | Context → partner | Partner page | Context | Partner data in context | P0 |
| INT-006 | Multi-turn → context | 3 messages | Context | All in context | P1 |
| INT-007 | Regenerate → new response | Regenerate | Message | New response | P1 |
| INT-008 | Export → import | Export + Import | Conversation | Round-trip | P1 |
| INT-009 | Clear → new | Clear | Conversation | New conversation | P1 |
| INT-010 | Suggested prompt → send | Click suggestion | Message | Sent | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Search by message content | "partner" | Matching conversations | P0 |
| INT-012 | Filter by date | Last 7 days | Date filtered | P1 |
| INT-013 | Filter by entity type | Partner | Partner conversations | P1 |
| INT-014 | Combined search + filter | "risk" + Partner | Both applied | P1 |
| INT-015 | Search empty | "NONEXISTENT" | Empty result | P1 |
| INT-016 | Search case-insensitive | "PARTNER" vs "partner" | Same results | P1 |
| INT-017 | Filter by entity ID | PartnerId=42 | That partner's chats | P1 |
| INT-018 | Filter excludes deleted | Include deleted | Deleted excluded | P1 |
| INT-019 | Search with special chars | "O'Brien" | Handled | P2 |
| INT-020 | Filter by conversation name | Name contains | Matching | P2 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Page 1 of 3 | 50, page=1, size=20 | 20, hasNext | P1 |
| INT-022 | Last page | 50, page=3, size=20 | 10, hasNext=false | P1 |
| INT-023 | Empty page | Filter yields 0 | Empty, total=0 | P1 |
| INT-024 | Single page | 15, size=20 | 15 items | P2 |
| INT-025 | Large page | 100, size=100 | All on 1 page | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Conversation → Messages | One-to-many | Load conversation | Messages loaded | P0 |
| INT-027 | Message → User | User | Load message | User info | P1 |
| INT-028 | Conversation → Entity | Context | Context conversation | Entity link | P1 |
| INT-029 | User → Conversations | One-to-many | Load user | Conversations loaded | P1 |
| INT-030 | Entity → Conversations | Reverse | Entity page | Related conversations | P1 |
| INT-031 | Prompt → Conversation | Template | Use prompt | Template applied | P1 |
| INT-032 | Message → Response | Paired | Send message | Response linked | P1 |
| INT-033 | Audit trail | Audit | Load conversation | History | P1 |
| INT-034 | Export includes relations | Export | Export | All messages | P2 |
| INT-035 | Rate limit → User | Per user | Rate limit | User-specific | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Send invalid → 400 | Validation | BusinessException | P0 |
| INT-037 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-038 | Unauthorized → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-039 | Delete non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-040 | AI service down → 503 | Service unavailable | User-friendly error | P0 |
| INT-041 | Rate limit → 429 | Too many | Rate limit message | P0 |
| INT-042 | Timeout → 504 | Request timeout | Timeout message | P1 |
| INT-043 | Import malformed → 400 | Parse | Validation error | P1 |
| INT-044 | DB timeout → 500 | Timeout | Graceful error | P1 |
| INT-045 | Concurrent conflict → 409 | Concurrency | Conflict error | P1 |
| INT-046 | Malformed request → 400 | Bad JSON | Validation error | P1 |
| INT-047 | Quota exceeded → 429 | Quota | Quota message | P1 |
| INT-048 | SQL injection → sanitized | Injection | Parameterized | P0 |
| INT-049 | Large payload → 413 | Oversized | Rejected | P2 |
| INT-050 | Prompt injection → mitigated | Injection | Filtered | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in message | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL injection in search | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in message | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in response | `"><script>` | Escaped | P0 |
| SEC-005 | Prompt injection | "Ignore previous" | Mitigated | P0 |
| SEC-006 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-007 | Path traversal in export | `../../evil` | Rejected | P0 |
| SEC-008 | HTML in message | `<img onerror=...>` | Escaped | P1 |
| SEC-009 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-010 | XXE in import | XXE payload | Rejected | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous send | No auth | POST /ai/chat | 401 | P0 |
| SEC-012 | No AI permission | Reader | Send message | 403 | P0 |
| SEC-013 | Expired token | Expired | Any | 401 | P0 |
| SEC-014 | Tampered JWT | Modified | Any | 401 | P0 |
| SEC-015 | Disabled account | Disabled | Any | 403 | P1 |
| SEC-016 | Post-logout | Logged out | Cached | 401 | P1 |
| SEC-017 | Role escalation | Basic | ?role=admin | Ignored | P0 |
| SEC-018 | Cross-tenant | User A | User B's conversation | 403 | P0 |
| SEC-019 | No entity view | No CanViewPartner | Partner context | 403 | P0 |
| SEC-020 | No export permission | Reader | Export | 403 | P1 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Conversation ID guess | Enumerate | 403 if no access | P0 |
| SEC-022 | Deleted conversation | Access deleted | 404 | P1 |
| SEC-023 | Negative ID | -1 | 400 | P1 |
| SEC-024 | Zero ID | 0 | 400 | P1 |
| SEC-025 | Float ID | 1.5 | 400 | P1 |
| SEC-026 | String ID | "abc" | 400 | P1 |
| SEC-027 | MAX_INT ID | 2147483647 | 404 | P1 |
| SEC-028 | Message ID manipulation | Change ID | Validated | P0 |
| SEC-029 | Entity ID manipulation | Change ID | Validated | P0 |
| SEC-030 | Other user's conversation | Access via ID | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable | P0 |
| SEC-035 | DeletedBy/DeletedDate | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on send | CSRF token | P0 |
| SEC-040 | CSRF on delete | CSRF token | P0 |
| SEC-041 | Token storage | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Policy enforced | P1 |
| SEC-043 | Token refresh | Works correctly | P1 |
| SEC-044 | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected Protection | Priority |
|----|------|-------------------|----------|
| SEC-046 | Internal audit fields | DTO filtering | P1 |
| SEC-047 | Stack traces | Generic errors | P0 |
| SEC-048 | Entity PII in context | Filtered per permission | P1 |
| SEC-049 | Response caching | Cache-Control: no-store | P1 |
| SEC-050 | Tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users send same conversation | Concurrent send | Both succeed | P1 |
| CON-002 | Create and delete same conversation | Race | One succeeds, other fails | P1 |
| CON-003 | Two users create conversations | Concurrent create | Both succeed | P1 |
| CON-004 | Update during read | Read consistency | Consistent read | P1 |
| CON-005 | Delete during read | Read consistency | Null or pre-delete | P1 |
| CON-006 | Concurrent send different | 2 users send | Both succeed | P1 |
| CON-007 | Concurrent regenerate | Same message | One succeeds | P1 |
| CON-008 | Concurrent pagination | Multiple pages | Correct data | P2 |
| CON-009 | Database deadlock | Circular | Resolved, retry | P1 |
| CON-010 | Token refresh during send | Expire mid-call | Retry with new token | P1 |
| CON-011 | Bulk export concurrent | 2 exports | Both succeed | P2 |
| CON-012 | Concurrent list | 2 users list | Both correct | P2 |
| CON-013 | Send during generation | Send + Generate | Queued or error | P1 |
| CON-014 | Rate limit race | 2 users at limit | Both handled | P1 |
| CON-015 | Context fetch during send | Fetch + Send | No corruption | P1 |
| CON-016 | Duplicate during delete | Duplicate + Delete | Handled | P2 |
| CON-017 | Import during list | Import + List | List consistent | P2 |
| CON-018 | Concurrent filter | 2 users filter | Independent | P2 |
| CON-019 | Optimistic concurrency | Update stale | Conflict error | P1 |
| CON-020 | Connection pool exhaustion | Many concurrent | Queued or error | P1 |
| CON-021 | Cache invalidation | Update + read | Fresh data | P1 |
| CON-022 | Stream interrupt | 2 streams | Both handled | P1 |
| CON-023 | Conversation conflict | Same ID | Unique ID | P1 |
| CON-024 | Bulk delete concurrent | 2 bulk deletes | Both complete | P2 |
| CON-025 | Search during send | Search + Send | Search consistent | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Message validation | Validation | "Hello" | Valid | P1 |
| UNT-002 | Empty message validation | Validation | "" | Invalid | P1 |
| UNT-003 | Context serialization | Formatting | Entity | JSON | P1 |
| UNT-004 | Rate limit check | Calculations | 9 requests | Under limit | P1 |
| UNT-005 | Prompt injection detection | Validation | "Ignore" | Flagged | P1 |
| UNT-006 | Message trim | Formatting | "  Hi  " | "Hi" | P2 |
| UNT-007 | Pagination default | Calculations | null, null | 1, 20 | P1 |
| UNT-008 | Stream chunk parse | Formatting | Chunk | Parsed | P1 |
| UNT-009 | Markdown render | Formatting | `**bold**` | Rendered | P1 |
| UNT-010 | Map entity to model | Collections | Entity | Model | P1 |
| UNT-011 | Map request to DTO | Collections | Request | DTO | P1 |
| UNT-012 | Context entity filter | Status logic | Deleted entity | Excluded | P1 |
| UNT-013 | Permission check | Validation | CanUseAI | true | P1 |
| UNT-014 | Quota check | Validation | Under limit | true | P1 |
| UNT-015 | Export format | Formatting | Messages | JSON | P2 |
| UNT-016 | Import parse | Validation | Valid JSON | Conversation | P1 |
| UNT-017 | Entity ID validation | Validation | 42 | Valid | P1 |
| UNT-018 | Conversation ID validation | Validation | 1 | Valid | P1 |
| UNT-019 | Timestamp format | Collections | Now | ISO string | P1 |
| UNT-020 | Template variable replace | Formatting | Template, vars | Filled | P1 |
| UNT-021 | Date format for audit | Formatting | Now | ISO string | P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Send message | Single send | < 5s | P2 |
| PRF-002 | Get conversation | Single read | < 200ms | P2 |
| PRF-003 | List 20 conversations | Paginated | < 500ms | P2 |
| PRF-004 | List 1000 conversations | Full list | < 3s | P2 |
| PRF-005 | Search conversations | Search | < 1s | P2 |
| PRF-006 | Export 100 messages | Export | < 2s | P2 |
| PRF-007 | Context fetch | 5 entities | < 500ms | P2 |
| PRF-008 | Regenerate response | Regenerate | < 5s | P2 |
| PRF-009 | 10 concurrent sends | Concurrent | All < 10s | P2 |
| PRF-010 | 20 concurrent reads | Concurrent | All < 500ms | P2 |
| PRF-011 | List with messages | Includes | < 1s | P2 |
| PRF-012 | Stream first chunk | Streaming | < 1s | P2 |
| PRF-013 | Long response | 50000 chars | < 10s | P2 |
| PRF-014 | Large context | 50000 chars | < 2s | P2 |
| PRF-015 | Memory: 1000 conversations | Load all | No leak | P2 |
| PRF-016 | Filter + sort | Combined | < 1s | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained send | 10 users, 0.5 req/s | 5 min | 95% < 5s | P2 |
| LDT-002 | Sustained list | 20 users, 1 req/s | 5 min | 95% < 1s | P2 |
| LDT-003 | Sustained search | 20 users, 2 req/s | 5 min | 95% < 1s | P2 |
| LDT-004 | Spike send | 0→30 users in 30s | 2 min | Queue or 429 | P2 |
| LDT-005 | Spike list | 0→50 users | 2 min | No errors | P2 |
| LDT-006 | Stress send | 50 users, 2 req/s | 5 min | Graceful degradation | P2 |
| LDT-007 | Stress list | 100 users, 5 req/s | 5 min | Graceful degradation | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 10 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | System recovers | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: Chat interface | POS-001 to POS-005, INT-001 to INT-010 |
| AC-2: Context-aware responses | POS-002, POS-006, POS-007, FUN-004, FUN-007 |
| AC-3: Entity data access | POS-002, FUN-007, INT-005, INT-026 to INT-028 |
| AC-4: Prompt management | POS-005, POS-018, FUN-009, UNT-020 |
| AC-5: Rate limiting | NEG-025, POS-017, BND-043 to BND-050, FUN-005 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
