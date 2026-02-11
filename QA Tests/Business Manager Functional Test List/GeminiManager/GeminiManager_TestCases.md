# GeminiManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GeminiManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

**GeminiManager** manages AI prompts, content generation, summarization, context management, and rate limiting for Gemini AI integration. Key responsibilities: prompt data retrieval by type, chat sessions, content generation, session management (create/end/star/archive), context handling, and API rate limit enforcement.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Get prompt data by type | Type exists | GetPromptDataByType("Summary") | Prompt data returned | P0 |
| POS-002 | Get prompt data — non-existent | Type missing | GetPromptDataByType("Unknown") | Empty | P1 |
| POS-003 | Map model to entity | Valid request | MapModelToEntity(request) | Entity mapped | P0 |
| POS-004 | Get user sessions | User has sessions | GetUserSessions(userId) | Sessions returned | P0 |
| POS-005 | Get user sessions — empty | User no sessions | GetUserSessions(userId) | Empty list | P1 |
| POS-006 | Create session | Valid user | CreateSession(userId) | Session created | P0 |
| POS-007 | End session | Session exists | EndSession(sessionId) | Session ended | P0 |
| POS-008 | Update session star | Session exists | UpdateSessionStar(sessionId, true) | Starred | P1 |
| POS-009 | Update session archive | Session exists | UpdateSessionArchive(sessionId, true) | Archived | P1 |
| POS-010 | Update session title | Session exists | UpdateSessionTitle(sessionId, "Title") | Title updated | P1 |
| POS-011 | Get session with chats | Session has chats | GetSessionDataWithChats(sessionId) | Chats loaded | P0 |
| POS-012 | Content generation | Valid prompt | GenerateContent(prompt) | Content returned | P0 |
| POS-013 | Summarization | Long text | Summarize(text) | Summary returned | P0 |
| POS-014 | Context management | Context provided | SetContext(context) | Context set | P1 |
| POS-015 | Rate limit check | Under limit | CheckRateLimit(userId) | Allowed | P0 |
| POS-016 | End session — non-existent | Session 99999 | EndSession(99999) | Graceful | P1 |
| POS-017 | Get prompt — null type | Type=null | GetPromptDataByType(null) | Empty or handled | P1 |
| POS-018 | Get prompt — empty type | Type="" | GetPromptDataByType("") | Empty | P1 |
| POS-019 | Map model — null request | Request=null | MapModelToEntity(null) | Handled | P1 |
| POS-020 | Update star remove | Session starred | UpdateSessionStar(sessionId, false) | Unstarred | P1 |
| POS-021 | Update archive unarchive | Session archived | UpdateSessionArchive(sessionId, false) | Unarchived | P1 |
| POS-022 | Update title empty | Session exists | UpdateSessionTitle(sessionId, "") | Per design | P1 |
| POS-023 | Session inactive update | Inactive session | UpdateCurrentSessionIfInactive | Updated | P1 |
| POS-024 | Multiple prompts | Several types | GetPromptDataByType each | All returned | P1 |
| POS-025 | Chat history | Session with messages | GetSessionDataWithChats | Messages loaded | P0 |
| POS-026 | Full CRUD session | None | Create→Get→Update→End | All succeed | P0 |
| POS-027 | Content with context | Context set | GenerateContent | Context used | P1 |
| POS-028 | Summarize short text | Short input | Summarize | Summary returned | P1 |
| POS-029 | Rate limit reset | After window | CheckRateLimit | Reset | P1 |
| POS-030 | Session list pagination | 100 sessions | GetUserSessions paginated | Paginated | P1 |
| POS-031 | Session filter by starred | Starred sessions | GetUserSessions filter | Starred only | P1 |
| POS-032 | Session filter by archived | Archived | GetUserSessions filter | Archived only | P1 |
| POS-033 | Prompt type case | "summary" vs "Summary" | GetPromptDataByType | Per design | P1 |
| POS-034 | Content generation — long | Long prompt | GenerateContent | Handled | P1 |
| POS-035 | Session cleanup | Old sessions | CleanupInactiveSessions | Cleaned | P2 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create session — null userId | userId=null | ArgumentNullException | P0 |
| NEG-002 | Get prompt — invalid type | Type "Xyz!!" | Empty or error | P1 |
| NEG-003 | Content generation — empty prompt | Prompt="" | Error | P0 |
| NEG-004 | Content generation — null prompt | Prompt=null | ArgumentNullException | P0 |
| NEG-005 | Rate limit exceeded | Over limit | 429 or blocked | P0 |
| NEG-006 | End session — invalid ID | sessionId=99999 | Graceful | P1 |
| NEG-007 | Get session — invalid ID | sessionId=99999 | Null | P1 |
| NEG-008 | Update session — invalid ID | sessionId=99999 | Null | P1 |
| NEG-009 | Map model — invalid request | Malformed request | Error | P1 |
| NEG-010 | Unauthenticated get prompt | No auth | 401 | P0 |
| NEG-011 | Unauthenticated create session | No auth | 401 | P0 |
| NEG-012 | Unauthorized user | User lacks permission | 403 | P0 |
| NEG-013 | IDOR — access other user session | GetSession(otherUserId) | 403 | P0 |
| NEG-014 | SQL injection in prompt type | ' OR 1=1-- | Sanitized | P0 |
| NEG-015 | XSS in session title | <script>alert(1)</script> | Sanitized | P0 |
| NEG-016 | Prompt injection | Malicious prompt | Sanitized | P0 |
| NEG-017 | API key invalid | Invalid Gemini key | Error | P0 |
| NEG-018 | API timeout | Gemini timeout | Timeout exception | P1 |
| NEG-019 | API rate limit | Gemini 429 | Handled | P1 |
| NEG-020 | Context too large | Context > max | Error | P0 |
| NEG-021 | Session already ended | End ended session | Idempotent | P1 |
| NEG-022 | Expired session | Old session | Handled | P1 |
| NEG-023 | Null context | SetContext(null) | Handled | P1 |
| NEG-024 | Empty context | SetContext("") | Handled | P1 |
| NEG-025 | Summarize empty | Summarize("") | Error | P1 |
| NEG-026 | Summarize null | Summarize(null) | ArgumentNullException | P0 |
| NEG-027 | Content — API error | Gemini 500 | Error | P1 |
| NEG-028 | Content — quota exceeded | User quota | Error | P1 |
| NEG-029 | Session count limit | Max sessions | Error | P1 |
| NEG-030 | Chat message limit | Max messages | Error | P1 |
| NEG-031 | Prompt type SQL injection | '; DROP TABLE-- | Sanitized | P0 |
| NEG-032 | Mass assignment | Include Id in CreateSession | Ignored | P0 |
| NEG-033 | Expired token | Expired JWT | 401 | P0 |
| NEG-034 | Wrong org scope | User OrgB get OrgA session | 403 | P0 |
| NEG-035 | Rate limit bypass | Rapid requests | 429 | P0 |
| NEG-036 | Session hijack | Use other session ID | 403 | P0 |
| NEG-037 | Content — unsafe output | Harmful content | Filtered | P0 |
| NEG-038 | Prompt — PII in prompt | PII in prompt | Sanitized | P0 |
| NEG-039 | Database timeout | DB timeout | Exception | P1 |
| NEG-040 | Cache poisoning | Malicious cache | Sanitized | P1 |
| NEG-041 | Concurrent session conflict | 2 users same session | Handled | P1 |
| NEG-042 | Update ended session | Update ended | Error | P1 |
| NEG-043 | Get prompt — whitespace type | "  Summary  " | Trimmed | P1 |
| NEG-044 | Session title too long | Title 1000 chars | Validation error | P1 |
| NEG-045 | Content — model not found | Invalid model | Error | P1 |
| NEG-046 | Content — temperature invalid | Temp=2.0 | Validation error | P1 |
| NEG-047 | Content — max tokens zero | MaxTokens=0 | Error | P1 |
| NEG-048 | Session — negative userId | userId=-1 | Error | P1 |
| NEG-049 | Session — zero sessionId | sessionId=0 | Error | P1 |
| NEG-050 | Replay attack | Replay content request | Rejected | P0 |
| NEG-051 | JWT alg none | alg=none | Rejected | P0 |
| NEG-052 | Brute force session IDs | Enumerate | Rate limited | P1 |
| NEG-053 | Log injection | Malicious log | Sanitized | P1 |
| NEG-054 | Header injection | Malicious header | Sanitized | P1 |
| NEG-055 | CSRF on create session | Cross-site | Token validated | P0 |
| NEG-056 | Parameter pollution | sessionId=1&sessionId=2 | Handled | P1 |
| NEG-057 | Content — token limit | Huge input | Error | P0 |
| NEG-058 | Session — duplicate create | Create same | Handled | P1 |
| NEG-059 | Prompt — reserved type | System type | Error | P1 |
| NEG-060 | Context — injection | Malicious context | Sanitized | P0 |
| NEG-061 | Summarize — XSS output | XSS in summary | Sanitized | P0 |
| NEG-062 | Rate limit — bypass | Manipulate header | Rejected | P0 |
| NEG-063 | Session — orphaned | User deleted | Handled | P1 |
| NEG-064 | Content — blocked output | Blocked category | Filtered | P0 |
| NEG-065 | Prompt — empty after trim | "   " | Empty | P1 |
| NEG-066 | Update — stale session | Concurrent update | Conflict | P1 |
| NEG-067 | Get sessions — invalid filter | Malformed filter | Error | P2 |
| NEG-068 | Content — network error | Network fail | Exception | P1 |
| NEG-069 | Session — corrupt data | Corrupt session | Handled | P1 |
| NEG-070 | Audit log failure | Audit down | Op succeeds, audit queued | P2 |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Prompt type length | 1 | 100 | "A" | 100 chars | 101 chars | P1 |
| BND-002 | Session title | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-003 | Context length | 0 | MaxToken | "" | Max | Max+1 | P1 |
| BND-004 | Content prompt length | 1 | MaxToken | "A" | Max | Max+1 | P1 |
| BND-005 | Summarize input | 1 | Max | "A" | Max | Max+1 | P1 |
| BND-006 | SessionId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-007 | UserId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-008 | Rate limit count | 0 | Max | 0 | Max | Max+1 | P1 |
| BND-009 | Chat message count | 0 | 10000 | 0 | 10000 | 10001 | P2 |
| BND-010 | Session count per user | 0 | 1000 | 0 | 1000 | 1001 | P1 |
| BND-011 | Temperature | 0 | 2 | 0 | 2 | 2.1 | P1 |
| BND-012 | MaxTokens | 1 | 8192 | 1 | 8192 | 8193 | P1 |
| BND-013 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-014 | PageSize | 1 | 100 | 1 | 100 | 101 | P1 |
| BND-015 | Empty prompt type | — | — | "" | — | — | P1 |
| BND-016 | Null prompt type | — | — | null | — | — | P1 |
| BND-017 | Unicode in title | — | — | "日本語" | — | — | P1 |
| BND-018 | Unicode in prompt | — | — | "résumé" | — | — | P1 |
| BND-019 | Special chars prompt | — | — | "Test & Co." | — | — | P1 |
| BND-020 | Newline in context | — | — | "Line1\nLine2" | — | — | P2 |
| BND-021 | Control chars | — | — | \x00 in prompt | — | — | P1 |
| BND-022 | Emoji in title | — | — | "📝 Session" | — | — | P2 |
| BND-023 | RTL in prompt | — | — | Arabic | — | — | P2 |
| BND-024 | Rate limit window | — | — | Exactly at limit | — | — | P1 |
| BND-025 | Session list empty | — | — | 0 sessions | — | — | P1 |
| BND-026 | Session list single | — | — | 1 session | — | — | P1 |
| BND-027 | Pagination last partial | — | — | 95 total, Size=20 | — | — | P1 |
| BND-028 | Pagination beyond last | — | — | Page 100 | — | — | P1 |
| BND-029 | Zero SessionId | — | — | sessionId=0 | — | — | P1 |
| BND-030 | Zero UserId | — | — | userId=0 | — | — | P1 |
| BND-031 | Negative SessionId | — | — | sessionId=-1 | — | — | P1 |
| BND-032 | Float temperature | — | — | 0.5 | — | — | P1 |
| BND-033 | Float max tokens | — | — | 100.5 | — | — | P2 |
| BND-034 | Date boundaries | — | — | Min/Max DateTime | — | — | P2 |
| BND-035 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-036 | Timezone | — | — | UTC | — | — | P2 |
| BND-037 | Rate limit reset time | — | — | Window boundary | — | — | P1 |
| BND-038 | Content response empty | — | — | Empty response | — | — | P1 |
| BND-039 | Content response max | — | — | Max tokens | — | — | P1 |
| BND-040 | Summarize min length | — | — | 1 char | — | — | P1 |
| BND-041 | Summarize max length | — | — | Max chars | — | — | P1 |
| BND-042 | Context null | — | — | null | — | — | P1 |
| BND-043 | Context empty | — | — | "" | — | — | P1 |
| BND-044 | Starred filter | — | — | starred=true | — | — | P1 |
| BND-045 | Archived filter | — | — | archived=true | — | — | P1 |
| BND-046 | Combined filters | — | — | Starred + archived | — | — | P2 |
| BND-047 | Sort by date | — | — | OrderBy CreatedDate | — | — | P1 |
| BND-048 | Sort empty | — | — | OrderBy on empty | — | — | P1 |
| BND-049 | Prompt type case | — | — | "summary" vs "Summary" | — | — | P1 |
| BND-050 | Concurrent rate limit | — | — | 2 threads at limit | — | — | P1 |
| BND-051 | Session expiry | — | — | Expired session | — | — | P1 |
| BND-052 | Token count boundary | — | — | Exactly max tokens | — | — | P1 |
| BND-053 | Model name length | — | — | Model string | — | — | P2 |
| BND-054 | Temperature zero | — | — | 0 | — | — | P1 |
| BND-055 | Temperature one | — | — | 1 | — | — | P1 |
| BND-056 | Content format | — | — | JSON/Text | — | — | P2 |
| BND-057 | Chat message length | — | — | Max per message | — | — | P2 |
| BND-058 | Batch content | — | — | Multiple prompts | — | — | P2 |
| BND-059 | Stream response | — | — | Streaming | — | — | P2 |
| BND-060 | Retry count | — | — | Max retries | — | — | P2 |
| BND-061 | Timeout value | — | — | 0, max | — | — | P2 |
| BND-062 | Pagination edge | — | — | First/last page | — | — | P1 |
| BND-063 | Collection null | — | — | Null list | — | — | P1 |
| BND-064 | Empty collection | — | — | [] | — | — | P1 |
| BND-065 | Prompt type reserved | — | — | System types | — | — | P1 |
| BND-066 | Multiple context | — | — | Append context | — | — | P2 |
| BND-067 | Session cleanup threshold | — | — | Inactive days | — | — | P2 |
| BND-068 | Rate limit per user | — | — | User limit | — | — | P1 |
| BND-069 | Rate limit per IP | — | — | IP limit | — | — | P2 |
| BND-070 | API version | — | — | v1 vs v2 | — | — | P2 |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Prompt type case handling | Get by type | GetPromptDataByType | Per design | P1 |
| FUN-002 | Deleted prompts excluded | Soft delete | GetPromptDataByType | Deleted excluded | P0 |
| FUN-003 | Session created with user | Create | CreateSession | UserId set | P0 |
| FUN-004 | Session ended sets EndDate | End | EndSession | EndDate set | P0 |
| FUN-005 | Starred filter | Update star | UpdateSessionStar | Starred persisted | P1 |
| FUN-006 | Archived filter | Update archive | UpdateSessionArchive | Archived persisted | P1 |
| FUN-007 | Title update | Update title | UpdateSessionTitle | Title persisted | P1 |
| FUN-008 | User sessions only | Get sessions | GetUserSessions | Own sessions only | P0 |
| FUN-009 | Rate limit enforced | Over limit | CheckRateLimit | Blocked | P0 |
| FUN-010 | Context in generation | Set context | GenerateContent | Context used | P1 |
| FUN-011 | Content sanitization | Harmful content | GenerateContent | Filtered | P0 |
| FUN-012 | Summarization length | Long text | Summarize | Within limit | P1 |
| FUN-013 | Session pagination | 100 sessions | GetUserSessions | Paginated | P1 |
| FUN-014 | Audit on create | Create session | CreateSession | Audit entry | P1 |
| FUN-015 | Audit on end | End session | EndSession | Audit entry | P1 |
| FUN-016 | Audit on update | Update session | UpdateSession | Audit entry | P1 |
| FUN-017 | Idempotent end | End twice | EndSession twice | Graceful | P1 |
| FUN-018 | Update ended session | Update ended | UpdateSession | Error | P1 |
| FUN-019 | Session with chats | Get session | GetSessionDataWithChats | Chats loaded | P0 |
| FUN-020 | Prompt type mapping | Get prompt | GetPromptDataByType | Correct mapping | P1 |
| FUN-021 | Model mapping | Map request | MapModelToEntity | Correct entity | P0 |
| FUN-022 | Null prompt type | Type=null | GetPromptDataByType | Handled | P1 |
| FUN-023 | Empty prompt type | Type="" | GetPromptDataByType | Empty | P1 |
| FUN-024 | Org scope | User OrgA | GetUserSessions | OrgA only | P0 |
| FUN-025 | Permission on create | Create | CreateSession | 403 if denied | P0 |
| FUN-026 | Permission on content | Generate | GenerateContent | 403 if denied | P0 |
| FUN-027 | Rate limit reset | After window | CheckRateLimit | Reset | P1 |
| FUN-028 | Token count | Input length | GenerateContent | Within limit | P1 |
| FUN-029 | Session cleanup | Old sessions | CleanupInactiveSessions | Removed | P2 |
| FUN-030 | Prompt versioning | Multiple versions | GetPromptDataByType | Correct version | P2 |
| FUN-031 | Content retry | API failure | GenerateContent | Retry | P1 |
| FUN-032 | Session list sort | Sort by date | GetUserSessions | Sorted | P1 |
| FUN-033 | Session filter | Starred/archived | GetUserSessions | Filtered | P1 |
| FUN-034 | Inactive session update | Inactive | UpdateCurrentSessionIfInactive | Updated | P1 |
| FUN-035 | Content timeout | Long generation | GenerateContent | Timeout | P1 |
| FUN-036 | Summarize empty | Summarize("") | Summarize | Error | P1 |
| FUN-037 | Content model validation | Invalid model | GenerateContent | Error | P1 |
| FUN-038 | Temperature validation | Invalid temp | GenerateContent | Error | P1 |
| FUN-039 | MaxTokens validation | Invalid | GenerateContent | Error | P1 |
| FUN-040 | Context size validation | Too large | SetContext | Error | P0 |
| FUN-041 | Session count limit | Max sessions | CreateSession | Error | P1 |
| FUN-042 | Chat message limit | Max messages | AddChat | Error | P1 |
| FUN-043 | Prompt injection prevention | Malicious prompt | GenerateContent | Sanitized | P0 |
| FUN-044 | PII in prompt | PII | GenerateContent | Sanitized | P0 |
| FUN-045 | Output filtering | Blocked category | GenerateContent | Filtered | P0 |
| FUN-046 | Map model null | Request=null | MapModelToEntity | Handled | P1 |
| FUN-047 | Session orphan | User deleted | GetUserSessions | Handled | P1 |
| FUN-048 | Optimistic concurrency | Concurrent update | UpdateSession | Conflict | P1 |
| FUN-049 | Rate limit per user | User limit | CheckRateLimit | Enforced | P0 |
| FUN-050 | Session expiry | Expired | GetSession | Handled | P1 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full session cycle | Create→Get→Update→End | GeminiManager | All succeed | P0 |
| INT-002 | Content generation flow | Prompt→Generate→Response | GeminiManager, Gemini API | Content returned | P0 |
| INT-003 | UserContext | Current user | GeminiManager, UserResolver | UserId applied | P0 |
| INT-004 | Permission check | Authorize | GeminiManager, PermissionService | Correct | P0 |
| INT-005 | Audit log | Audit CRUD | GeminiManager, AuditLog | Entries | P1 |
| INT-006 | DbContext | Persist | GeminiManager, DbContext | Saved | P0 |
| INT-007 | AutoMapper | Entity to Model | GeminiManager, AutoMapper | Mapped | P1 |
| INT-008 | Controller | API | GeminiManager, Controller | 200/201 | P0 |
| INT-009 | Gemini API client | API call | GeminiManager, IGeminiClient | Response | P0 |
| INT-010 | Rate limit service | Check limit | GeminiManager, IRateLimitService | Enforced | P0 |
| INT-011 | Error handling | Exception | GeminiManager, Handler | Consistent | P1 |
| INT-012 | Logging | Log | GeminiManager, ILogger | Logs | P2 |
| INT-013 | Configuration | Config | GeminiManager, IConfiguration | Applied | P2 |
| INT-014 | AI assistant UI | Session in UI | GeminiManager, AIAssistant | Displayed | P1 |
| INT-015 | Opportunity summarization | Summarize opportunity | GeminiManager, OpportunityManager | Summary | P1 |
| INT-016 | Prompt from DB | Get prompt | GeminiManager, DocumentType | Prompt loaded | P0 |
| INT-017 | Session in list | List sessions | GeminiManager, ListView | Displayed | P1 |
| INT-018 | ManagerWrapper | Resolution | ManagerWrapper | Correct | P1 |
| INT-019 | Multi-tenant | Org scope | GeminiManager | Isolated | P0 |
| INT-020 | API 404 | Get invalid | Controller | 404 | P0 |
| INT-021 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-022 | API 401 | Unauthorized | Controller | 401 | P0 |
| INT-023 | API 429 | Rate limit | Controller | 429 | P0 |
| INT-024 | Repository | CRUD | GeminiManager, Repository | Works | P1 |
| INT-025 | Cache | Session cache | GeminiManager, ICache | Cached | P2 |
| INT-026 | Retry policy | API retry | GeminiManager | Retries | P1 |
| INT-027 | Timeout config | Timeout | GeminiManager | Applied | P1 |
| INT-028 | Feature flag | AI feature | GeminiManager | Respected | P2 |
| INT-029 | Content in opportunity | Generate for opp | GeminiManager, Opportunity | Content used | P1 |
| INT-030 | Contact import | Generate from email | GeminiManager, ContactManager | Generated | P2 |
| INT-031 | Notification | Notify on error | GeminiManager, NotificationManager | Sent | P2 |
| INT-032 | Metrics | Track usage | GeminiManager, IMetrics | Recorded | P2 |
| INT-033 | Health check | API health | GeminiManager | Status | P2 |
| INT-034 | Migration | Add prompt type | GeminiManager | Migrated | P2 |
| INT-035 | Seed prompts | Initial prompts | GeminiManager | Seeded | P2 |
| INT-036 | Export sessions | Export | GeminiManager | Export file | P2 |
| INT-037 | Import sessions | Import | GeminiManager | Imported | P2 |
| INT-038 | Session sharing | Share session | GeminiManager | Per design | P2 |
| INT-039 | Session export | Export session | GeminiManager | Exported | P2 |
| INT-040 | Content streaming | Stream response | GeminiManager | Streamed | P2 |
| INT-041 | Batch generation | Multiple prompts | GeminiManager | Batch | P2 |
| INT-042 | Model fallback | Primary fail | GeminiManager | Fallback | P2 |
| INT-043 | Region routing | Region | GeminiManager | Routed | P2 |
| INT-044 | Cost tracking | Track cost | GeminiManager | Tracked | P2 |
| INT-045 | Usage quota | User quota | GeminiManager | Enforced | P1 |
| INT-046 | Prompt version | Version | GeminiManager | Correct | P2 |
| INT-047 | A/B test prompt | Variant | GeminiManager | Variant | P2 |
| INT-048 | Feedback loop | User feedback | GeminiManager | Stored | P2 |
| INT-049 | Compliance check | Content compliance | GeminiManager | Checked | P1 |
| INT-050 | PII detection | PII in content | GeminiManager | Detected | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection prompt type | ' OR 1=1-- | GetPromptDataByType | Sanitized | P0 |
| SEC-002 | XSS in session title | <script>alert(1)</script> | UpdateSessionTitle | Sanitized | P0 |
| SEC-003 | Prompt injection | Ignore previous | GenerateContent | Sanitized | P0 |
| SEC-004 | IDOR get session | GetSession(otherId) | GetSessionDataWithChats | 403 | P0 |
| SEC-005 | IDOR update session | UpdateSession(otherId) | UpdateSession | 403 | P0 |
| SEC-006 | IDOR end session | EndSession(otherId) | EndSession | 403 | P0 |
| SEC-007 | Mass assignment | Include Id | CreateSession | Ignored | P0 |
| SEC-008 | Unauthenticated | No auth | Any op | 401 | P0 |
| SEC-009 | Expired token | Expired JWT | Any | 401 | P0 |
| SEC-010 | Wrong role | No permission | CreateSession | 403 | P0 |
| SEC-011 | Org scope bypass | Cross-org | GetUserSessions | 403 | P0 |
| SEC-012 | API key exposure | Key in response | GenerateContent | Not exposed | P0 |
| SEC-013 | PII in prompt | PII | GenerateContent | Sanitized | P0 |
| SEC-014 | Content exfiltration | Extract data | GenerateContent | Blocked | P0 |
| SEC-015 | Rate limit bypass | Manipulate | CheckRateLimit | Rejected | P0 |
| SEC-016 | Session hijack | Other session | GetSession | 403 | P0 |
| SEC-017 | Replay attack | Replay | GenerateContent | Rejected | P0 |
| SEC-018 | JWT alg none | alg=none | Request | Rejected | P0 |
| SEC-019 | Brute force | Enumerate | GetSession | Rate limited | P1 |
| SEC-020 | CSRF create | Cross-site | CreateSession | Token validated | P0 |
| SEC-021 | CSRF update | Cross-site | UpdateSession | Token validated | P0 |
| SEC-022 | CSRF end | Cross-site | EndSession | Token validated | P0 |
| SEC-023 | Log injection | Malicious log | Log | Sanitized | P1 |
| SEC-024 | Header injection | Malicious header | Request | Sanitized | P1 |
| SEC-025 | Parameter pollution | id=1&id=2 | Get | Handled | P1 |
| SEC-026 | Info disclosure | Probe errors | Invalid | Generic | P1 |
| SEC-027 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| SEC-028 | Token in URL | Token in query | Request | Avoided | P1 |
| SEC-029 | Cookie manipulation | Modify cookie | Request | Rejected | P0 |
| SEC-030 | Substitution attack | Replace JWT | Request | 403 | P0 |
| SEC-031 | Timing attack | Response time | GetSession | Constant | P2 |
| SEC-032 | Cache poisoning | Malicious cache | Cache | Sanitized | P1 |
| SEC-033 | Content — harmful output | Harmful | GenerateContent | Filtered | P0 |
| SEC-034 | Summarize — PII | PII in text | Summarize | Sanitized | P0 |
| SEC-035 | Context injection | Malicious context | SetContext | Sanitized | P0 |
| SEC-036 | Model injection | Invalid model | GenerateContent | Rejected | P0 |
| SEC-037 | LDAP injection | *)(uid=* | Filter | Sanitized | P0 |
| SEC-038 | Path traversal | ../../../ | Any | Rejected | P0 |
| SEC-039 | Null byte | %00 | Filename | Rejected | P0 |
| SEC-040 | Open redirect | Redirect | Callback | Validated | P1 |
| SEC-041 | Session fixation | Fixate | Login | New session | P1 |
| SEC-042 | Excessive data | Huge request | GenerateContent | Rejected | P1 |
| SEC-043 | DoS rate limit | Many requests | API | 429 | P0 |
| SEC-044 | DoS large prompt | Huge prompt | GenerateContent | Rejected | P0 |
| SEC-045 | Privilege escalation | Admin action | User | 403 | P0 |
| SEC-046 | Insecure reference | SessionId manip | Get | Validated | P0 |
| SEC-047 | Token expiry | Expired | Request | 401 | P0 |
| SEC-048 | Audit bypass | Skip audit | Any op | Audit required | P0 |
| SEC-049 | Content policy bypass | Policy bypass | GenerateContent | Blocked | P0 |
| SEC-050 | API key leak | Key in log | Log | Not logged | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent create sessions | 10 threads CreateSession | All created | P0 |
| CON-002 | Concurrent get prompts | 20 threads GetPromptDataByType | All correct | P0 |
| CON-003 | Concurrent generate content | 5 threads GenerateContent | All succeed | P0 |
| CON-004 | Concurrent update same session | 5 threads UpdateSession(123) | No corruption | P0 |
| CON-005 | Concurrent end same session | 2 threads EndSession(123) | One succeeds | P0 |
| CON-006 | Create and get | Thread1 create, Thread2 get | Consistent | P1 |
| CON-007 | Update and get | Thread1 update, Thread2 get | Consistent | P1 |
| CON-008 | End and get | Thread1 end, Thread2 get | Null or ended | P0 |
| CON-009 | Rate limit concurrent | 50 threads at limit | Enforced | P0 |
| CON-010 | Optimistic concurrency | 2 users update session | Conflict | P0 |
| CON-011 | Connection pool | 100 concurrent | No exhaustion | P1 |
| CON-012 | Deadlock | Circular | No deadlock | P1 |
| CON-013 | Transaction isolation | Read uncommitted | Per level | P1 |
| CON-014 | Double submit create | User double-clicks | One session | P0 |
| CON-015 | Race on session list | Thread1 create, Thread2 list | Consistent | P1 |
| CON-016 | Concurrent star update | 2 threads star | One wins | P1 |
| CON-017 | Concurrent archive | 2 threads archive | One wins | P1 |
| CON-018 | Content generation parallel | 10 threads generate | All succeed | P1 |
| CON-019 | Summarize parallel | 10 threads summarize | All succeed | P1 |
| CON-020 | Context update parallel | 2 threads set context | One wins | P1 |
| CON-021 | Cache update | Concurrent cache | Consistent | P1 |
| CON-022 | Lost update | 2 users different fields | Per design | P1 |
| CON-023 | Phantom read | Insert during list | Per isolation | P2 |
| CON-024 | Non-repeatable read | Update between reads | Per isolation | P2 |
| CON-025 | API rate limit shared | Shared limit | Enforced | P0 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Prompt type validation | Validation | "Summary" | Valid | P0 |
| UNT-002 | Prompt type invalid | Validation | "" | Invalid | P0 |
| UNT-003 | Session title validation | Validation | "Title" | Valid | P0 |
| UNT-004 | Context validation | Validation | Valid context | Valid | P0 |
| UNT-005 | Token count | Calculation | Text | Count | P1 |
| UNT-006 | Title trim | Formatting | "  Title  " | "Title" | P1 |
| UNT-007 | Context trim | Formatting | "  context  " | Trimmed | P1 |
| UNT-008 | Rate limit check | Calculation | Count, limit | Allowed/Blocked | P1 |
| UNT-009 | Session active check | Status logic | Session | IsActive | P1 |
| UNT-010 | Session starred | Status logic | Starred=true | IsStarred | P1 |
| UNT-011 | Session archived | Status logic | Archived=true | IsArchived | P1 |
| UNT-012 | Collection filter | Collections | List with ended | Ended excluded | P1 |
| UNT-013 | Empty collection | Collections | No sessions | Count=0 | P1 |
| UNT-014 | Null to empty | Collections | Null list | [] | P1 |
| UNT-015 | Map Session to Model | Mapping | Session entity | SessionModel | P0 |
| UNT-016 | Map Request to Entity | Mapping | CreateRequest | Entity | P0 |
| UNT-017 | Pagination slice | Calculation | Page 1, Size 10 | Skip 10, Take 10 | P1 |
| UNT-018 | Temperature clamp | Validation | 2.5 | 2.0 | P1 |
| UNT-019 | MaxTokens clamp | Validation | 10000 | 8192 | P1 |
| UNT-020 | Prompt type parse | Validation | "summary" | Summary | P1 |
| UNT-021 | Audit fields | Status logic | New session | CreatedBy set | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Get prompt data | GetPromptDataByType | < 100ms | P0 |
| PRF-002 | Create session | CreateSession | < 200ms | P0 |
| PRF-003 | Get user sessions | GetUserSessions (100) | < 500ms | P0 |
| PRF-004 | Session update | UpdateSession | < 150ms | P0 |
| PRF-005 | Get session with chats | GetSessionDataWithChats (500 msgs) | < 1000ms | P0 |
| PRF-006 | Content generation | GenerateContent | < 5000ms | P0 |
| PRF-007 | Summarization | Summarize (10K chars) | < 3000ms | P0 |
| PRF-008 | Rate limit check | CheckRateLimit | < 50ms | P0 |
| PRF-009 | Concurrent 20 reads | 20 GetUserSessions | < 300ms each | P1 |
| PRF-010 | Bulk 50 updates | 50 session updates | < 2000ms | P1 |
| PRF-011 | Prompt with filter | GetPromptDataByType | < 50ms | P1 |
| PRF-012 | Memory 100 sessions | GetUserSessions | < 50MB | P1 |
| PRF-013 | End session | EndSession | < 100ms | P1 |
| PRF-014 | Map model | MapModelToEntity | < 10ms | P1 |
| PRF-015 | Cold start | First GetPromptDataByType | < 200ms | P2 |
| PRF-016 | Cached prompt | Second GetPromptDataByType | < 20ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained 20 req/s get prompt | 20 GetPromptDataByType/sec | 5 min | 95% < 100ms | P0 |
| LDT-002 | Sustained 10 req/s create session | 10 CreateSession/sec | 5 min | 95% < 300ms | P0 |
| LDT-003 | Sustained 5 req/s generate | 5 GenerateContent/sec | 5 min | 95% < 5000ms | P0 |
| LDT-004 | Spike 50 req/s | 50 req/s burst | 1 min | No crash | P0 |
| LDT-005 | Spike 100 req/s | 100 req/s | 30 sec | Graceful degrade | P1 |
| LDT-006 | Stress ramp | 1→200 req/s | Until fail | Find limit | P1 |
| LDT-007 | Connection pool | 100 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Memory | 1K sessions | 5 min | No leak | P1 |
| LDT-009 | Recovery spike | Spike then normal | 5 min | Baseline | P0 |
| LDT-010 | Recovery stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
