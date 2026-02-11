# GmailAddonManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GmailAddonManager`  
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

**GmailAddonManager** manages email sync, contact import, interaction creation from email, OAuth, and deduplication. Key responsibilities: Gmail Add-on integration, email-to-contact matching, interaction creation from emails, OAuth token management, sync/dedup logic, and related records retrieval.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Get contacts for Gmail Add-on | Emails provided | GetContactsForGmailAddon(request) | Matching contacts | P0 |
| POS-002 | Get related records | Gmail request | GetRelatedRecords(request) | Records returned | P0 |
| POS-003 | OAuth token exchange | Auth code | ExchangeToken(code) | Token stored | P0 |
| POS-004 | OAuth token refresh | Refresh token | RefreshToken() | New token | P0 |
| POS-005 | Import contact from email | Email provided | ImportContactFromEmail(email) | Contact created | P0 |
| POS-006 | Create interaction from email | Email + contact | CreateInteractionFromEmail(email, contactId) | Interaction created | P0 |
| POS-007 | Sync emails | User sync | SyncEmails(userId) | Emails synced | P0 |
| POS-008 | Dedup contacts | Duplicate emails | DeduplicateContacts(emails) | Deduped | P0 |
| POS-009 | Get unmatched emails | Email list | GetUnmatchedEmails(emails) | Unmatched returned | P1 |
| POS-010 | Match email to contact | Email exists | MatchEmailToContact(email) | Contact returned | P1 |
| POS-011 | Get email thread | Thread ID | GetEmailThread(threadId) | Thread returned | P1 |
| POS-012 | Link email to interaction | Email + interaction | LinkEmailToInteraction(emailId, interactionId) | Linked | P1 |
| POS-013 | OAuth disconnect | User connected | DisconnectOAuth(userId) | Disconnected | P1 |
| POS-014 | Get sync status | User syncing | GetSyncStatus(userId) | Status returned | P1 |
| POS-015 | Batch import contacts | Email list | BatchImportContacts(emails) | Contacts created | P1 |
| POS-016 | Get contacts — empty emails | Empty list | GetContactsForGmailAddon([]) | Empty | P1 |
| POS-017 | Token valid | Valid token | ValidateToken(token) | Valid | P1 |
| POS-018 | Create interaction — no contact | Email only | CreateInteractionFromEmail(email, null) | Interaction created | P1 |
| POS-019 | Dedup — no duplicates | Unique emails | DeduplicateContacts | All returned | P1 |
| POS-020 | Sync — no new emails | No new | SyncEmails | Empty sync | P1 |
| POS-021 | OAuth scope check | Required scope | CheckOAuthScope | Scope valid | P1 |
| POS-022 | Get related — partner | Request with partner | GetRelatedRecords | Partner data | P1 |
| POS-023 | Get related — opportunity | Request with opp | GetRelatedRecords | Opp data | P1 |
| POS-024 | Full sync cycle | User connected | SyncEmails→GetSyncStatus | Synced | P0 |
| POS-025 | Full import cycle | Emails | ImportContactFromEmail→MatchEmailToContact | Imported | P0 |
| POS-026 | OAuth full flow | User | ExchangeToken→RefreshToken→Disconnect | All succeed | P0 |
| POS-027 | Contact already exists | Email matches | ImportContactFromEmail | Existing returned | P1 |
| POS-028 | Interaction type from email | Email | CreateInteractionFromEmail | Type=Email | P1 |
| POS-029 | Email metadata extraction | Email | ExtractMetadata(email) | Metadata | P1 |
| POS-030 | Get contacts by thread | Thread ID | GetContactsForThread(threadId) | Contacts | P1 |
| POS-031 | Sync incremental | Previous sync | SyncEmails | Delta only | P1 |
| POS-032 | Dedup with suggestions | Unmatched | GetUnmatchedEmailsWithSuggestions | Suggestions | P1 |
| POS-033 | OAuth state validation | State param | ValidateOAuthState(state) | Valid | P1 |
| POS-034 | Email attachment handling | Email with attachments | CreateInteractionFromEmail | Attachments handled | P1 |
| POS-035 | Multiple recipients | Email to multiple | MatchEmailToContact | All matched | P1 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Get contacts — null request | Request=null | ArgumentNullException | P0 |
| NEG-002 | OAuth — invalid code | Invalid auth code | Error | P0 |
| NEG-003 | OAuth — expired code | Expired code | Error | P0 |
| NEG-004 | OAuth — invalid refresh | Invalid refresh token | Error | P0 |
| NEG-005 | Import — invalid email | Malformed email | Error | P0 |
| NEG-006 | Import — null email | Email=null | ArgumentNullException | P0 |
| NEG-007 | Create interaction — invalid contact | contactId=99999 | Error | P0 |
| NEG-008 | Sync — unauthenticated | No OAuth | Error | P0 |
| NEG-009 | Sync — invalid userId | userId=0 | Error | P1 |
| NEG-010 | Dedup — null list | Emails=null | ArgumentNullException | P0 |
| NEG-011 | Get unmatched — null | Emails=null | ArgumentNullException | P0 |
| NEG-012 | SQL injection in email | ' OR 1=1-- | Sanitized | P0 |
| NEG-013 | XSS in email subject | <script>alert(1)</script> | Sanitized | P0 |
| NEG-014 | IDOR — access other user sync | SyncEmails(otherUserId) | 403 | P0 |
| NEG-015 | IDOR — other user tokens | GetToken(otherUserId) | 403 | P0 |
| NEG-016 | Unauthenticated | No auth | Any op | 401 | P0 |
| NEG-017 | Unauthorized | No permission | SyncEmails | 403 | P0 |
| NEG-018 | OAuth token theft | Stolen token | Use token | Detected | P0 |
| NEG-019 | Token replay | Replay token | Exchange | Rejected | P0 |
| NEG-020 | State validation fail | Invalid state | OAuth callback | Error | P0 |
| NEG-021 | Email format invalid | "not-email" | ImportContactFromEmail | Error | P0 |
| NEG-022 | Email domain blocked | Blocked domain | ImportContactFromEmail | Rejected | P1 |
| NEG-023 | Rate limit exceeded | Too many syncs | SyncEmails | 429 | P0 |
| NEG-024 | Gmail API error | API 500 | SyncEmails | Error | P1 |
| NEG-025 | Gmail API timeout | Timeout | SyncEmails | Timeout exception | P1 |
| NEG-026 | Gmail quota exceeded | Quota | SyncEmails | Error | P1 |
| NEG-027 | Token expired | Expired token | RefreshToken | Refresh | P1 |
| NEG-028 | Refresh token revoked | Revoked | RefreshToken | Error | P1 |
| NEG-029 | Scope insufficient | Missing scope | SyncEmails | Error | P0 |
| NEG-030 | Contact create conflict | Duplicate | ImportContactFromEmail | Handled | P1 |
| NEG-031 | Interaction create conflict | Duplicate | CreateInteractionFromEmail | Handled | P1 |
| NEG-032 | Thread not found | Invalid threadId | GetEmailThread | Null | P1 |
| NEG-033 | Link — invalid email ID | emailId=99999 | Error | P1 |
| NEG-034 | Link — invalid interaction ID | interactionId=99999 | Error | P1 |
| NEG-035 | Get sync status — no sync | Never synced | GetSyncStatus | No status | P1 |
| NEG-036 | Disconnect — not connected | Not connected | DisconnectOAuth | Graceful | P1 |
| NEG-037 | Batch import — partial failure | One invalid | Per design | P1 |
| NEG-038 | Mass assignment | Include Id | ImportContactFromEmail | Ignored | P0 |
| NEG-039 | Expired JWT | Expired | Request | 401 | P0 |
| NEG-040 | Org scope bypass | OrgB access OrgA | GetContactsForGmailAddon | 403 | P0 |
| NEG-041 | Email injection | Malicious email | CreateInteractionFromEmail | Sanitized | P0 |
| NEG-042 | Attachment virus | Infected attachment | CreateInteractionFromEmail | Rejected | P0 |
| NEG-043 | Attachment too large | 10MB attachment | CreateInteractionFromEmail | Rejected | P1 |
| NEG-044 | Concurrent sync | 2 threads sync same user | One succeeds | P1 |
| NEG-045 | OAuth redirect invalid | Invalid redirect_uri | ExchangeToken | Error | P0 |
| NEG-046 | OAuth client invalid | Invalid client_id | ExchangeToken | Error | P0 |
| NEG-047 | OAuth secret invalid | Invalid client_secret | ExchangeToken | Error | P0 |
| NEG-048 | Email header injection | \r\nBcc: attacker@ | Email | Sanitized | P0 |
| NEG-049 | Thread ID injection | Malicious threadId | GetEmailThread | Sanitized | P0 |
| NEG-050 | Rate limit bypass | Manipulate | SyncEmails | Rejected | P0 |
| NEG-051 | JWT alg none | alg=none | Request | Rejected | P0 |
| NEG-052 | Brute force | Enumerate | GetRelatedRecords | Rate limited | P1 |
| NEG-053 | Log injection | Malicious log | Log | Sanitized | P1 |
| NEG-054 | CSRF OAuth | Cross-site | OAuth callback | State validated | P0 |
| NEG-055 | Parameter pollution | userId=1&userId=2 | Request | Handled | P1 |
| NEG-056 | Open redirect | Redirect | OAuth callback | Validated | P0 |
| NEG-057 | Session fixation | Fixate | OAuth | New session | P1 |
| NEG-058 | Token in URL | Token in query | Request | Avoided | P1 |
| NEG-059 | Database timeout | DB timeout | SyncEmails | Exception | P1 |
| NEG-060 | Gmail API change | API breaking | SyncEmails | Handled | P1 |
| NEG-061 | Email parsing error | Malformed | ParseEmail | Error | P1 |
| NEG-062 | Metadata extraction fail | Unparseable | ExtractMetadata | Default | P1 |
| NEG-063 | Dedup conflict | Same email multiple | DeduplicateContacts | Resolved | P1 |
| NEG-064 | Sync conflict | Concurrent sync | SyncEmails | Handled | P1 |
| NEG-065 | Token storage full | Storage full | ExchangeToken | Error | P1 |
| NEG-066 | Contact limit | Max contacts | ImportContactFromEmail | Error | P1 |
| NEG-067 | Interaction limit | Max interactions | CreateInteractionFromEmail | Error | P1 |
| NEG-068 | Sync backlog | Too many emails | SyncEmails | Throttled | P1 |
| NEG-069 | Email size limit | Too large | CreateInteractionFromEmail | Rejected | P1 |
| NEG-070 | Audit log failure | Audit down | Any op | Op succeeds | P2 |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Email length | 5 | 320 | "a@b.c" | 320 chars | 321 chars | P1 |
| BND-002 | Email list count | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-003 | Thread ID | 1 | Max | 1 | Valid | — | P1 |
| BND-004 | ContactId | 0 | 2147483647 | 0 | Max int | Overflow | P1 |
| BND-005 | InteractionId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-006 | UserId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-007 | Sync batch size | 1 | 500 | 1 | 500 | 501 | P1 |
| BND-008 | Token length | — | — | JWT | — | — | P1 |
| BND-009 | OAuth state length | 10 | 255 | 10 | 255 | 256 | P1 |
| BND-010 | Email subject | 0 | 1000 | "" | 1000 chars | 1001 chars | P1 |
| BND-011 | Email body | 0 | Max | "" | Max | Max+1 | P1 |
| BND-012 | Attachment count | 0 | 50 | 0 | 50 | 51 | P1 |
| BND-013 | Attachment size | 0 | 25MB | 0 | 25MB | 25MB+1 | P1 |
| BND-014 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-015 | PageSize | 1 | 100 | 1 | 100 | 101 | P1 |
| BND-016 | Empty email list | — | — | [] | — | — | P1 |
| BND-017 | Single email | — | — | 1 email | — | — | P1 |
| BND-018 | Unicode in email | — | — | IDN format | — | — | P1 |
| BND-019 | Unicode in subject | — | — | "日本語" | — | — | P1 |
| BND-020 | Special chars email | — | — | "user+tag@example.com" | — | — | P1 |
| BND-021 | Newline in body | — | — | "Line1\nLine2" | — | — | P2 |
| BND-022 | Control chars | — | — | \x00 in email | — | — | P1 |
| BND-023 | Emoji in subject | — | — | "📧 Subject" | — | — | P2 |
| BND-024 | RTL in body | — | — | Arabic | — | — | P2 |
| BND-025 | Zero UserId | — | — | userId=0 | — | — | P1 |
| BND-026 | Zero ContactId | — | — | contactId=0 | — | — | P1 |
| BND-027 | Negative ContactId | — | — | contactId=-1 | — | — | P1 |
| BND-028 | Null optional | — | — | contactId=null | — | — | P1 |
| BND-029 | Date boundaries | — | — | Min/Max DateTime | — | — | P2 |
| BND-030 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-031 | Timezone | — | — | UTC | — | — | P2 |
| BND-032 | Token expiry | — | — | Expired | — | — | P1 |
| BND-033 | Sync window | — | — | 7 days | — | — | P1 |
| BND-034 | Pagination last partial | — | — | 95 total, Size=20 | — | — | P1 |
| BND-035 | Pagination beyond last | — | — | Page 100 | — | — | P1 |
| BND-036 | Concurrent sync | — | — | 2 threads | — | — | P1 |
| BND-037 | Dedup empty | — | — | [] | — | — | P1 |
| BND-038 | Dedup single | — | — | 1 email | — | — | P1 |
| BND-039 | OAuth scope list | — | — | Multiple scopes | — | — | P1 |
| BND-040 | Email header count | — | — | Many headers | — | — | P2 |
| BND-041 | Recipient count | — | — | 100 recipients | — | — | P2 |
| BND-042 | Thread message count | — | — | 100 messages | — | — | P2 |
| BND-043 | Sync interval | — | — | Min interval | — | — | P1 |
| BND-044 | Rate limit window | — | — | At limit | — | — | P1 |
| BND-045 | Token refresh window | — | — | Before expiry | — | — | P1 |
| BND-046 | Collection null | — | — | Null list | — | — | P1 |
| BND-047 | Collection empty | — | — | [] | — | — | P1 |
| BND-048 | Whitespace email | — | — | "  user@example.com  " | — | — | P1 |
| BND-049 | Case email | — | — | "User@Example.COM" | — | — | P1 |
| BND-050 | Subdomain | — | — | "a@mail.example.com" | — | — | P1 |
| BND-051 | International domain | — | — | IDN | — | — | P1 |
| BND-052 | Long subject | — | — | 1000 chars | — | — | P1 |
| BND-053 | Empty subject | — | — | "" | — | — | P1 |
| BND-054 | Empty body | — | — | "" | — | — | P1 |
| BND-055 | HTML body | — | — | <html> | — | — | P1 |
| BND-056 | Plain text body | — | — | Plain | — | — | P1 |
| BND-057 | Multipart | — | — | multipart/alternative | — | — | P2 |
| BND-058 | Attachment types | — | — | PDF, DOCX | — | — | P1 |
| BND-059 | Inline image | — | — | inline image | — | — | P2 |
| BND-060 | Encoding | — | — | UTF-8, Base64 | — | — | P1 |
| BND-061 | MIME type | — | — | text/plain | — | — | P1 |
| BND-062 | Date header | — | — | Invalid date | — | — | P2 |
| BND-063 | Message ID | — | — | Long ID | — | — | P2 |
| BND-064 | References header | — | — | Thread refs | — | — | P2 |
| BND-065 | In-Reply-To | — | — | Reply ref | — | — | P2 |
| BND-066 | Label count | — | — | Many labels | — | — | P2 |
| BND-067 | Snippet length | — | — | 255 chars | — | — | P2 |
| BND-068 | Sync cursor | — | — | Pagination token | — | — | P2 |
| BND-069 | Batch size | — | — | 100 emails | — | — | P1 |
| BND-070 | Retry count | — | — | Max retries | — | — | P2 |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete excluded | Deleted contacts | GetContactsForGmailAddon | Excluded | P0 |
| FUN-002 | OAuth token encrypted | Store token | ExchangeToken | Encrypted | P0 |
| FUN-003 | CreatedBy on import | Import contact | ImportContactFromEmail | CreatedBy set | P0 |
| FUN-004 | Interaction type Email | Create from email | CreateInteractionFromEmail | Type=Email | P0 |
| FUN-005 | Dedup exact match | Same email | DeduplicateContacts | One | P0 |
| FUN-006 | Sync incremental | Previous sync | SyncEmails | Delta only | P1 |
| FUN-007 | Token refresh | Before expiry | RefreshToken | Refreshed | P0 |
| FUN-008 | User sessions only | Get status | GetSyncStatus | Own only | P0 |
| FUN-009 | Org scope | User OrgA | GetRelatedRecords | OrgA only | P0 |
| FUN-010 | Permission sync | User lacks | SyncEmails | 403 | P0 |
| FUN-011 | Permission import | User lacks | ImportContactFromEmail | 403 | P0 |
| FUN-012 | Contact match | Email exists | MatchEmailToContact | Contact | P1 |
| FUN-013 | No match | Email new | MatchEmailToContact | Null | P1 |
| FUN-014 | Audit on import | Import | ImportContactFromEmail | Audit entry | P1 |
| FUN-015 | Audit on interaction | Create interaction | CreateInteractionFromEmail | Audit entry | P1 |
| FUN-016 | Audit on sync | Sync | SyncEmails | Audit entry | P1 |
| FUN-017 | Idempotent sync | Sync twice | SyncEmails | Same result | P1 |
| FUN-018 | Disconnect clears token | Disconnect | DisconnectOAuth | Token removed | P1 |
| FUN-019 | Unmatched emails | New emails | GetUnmatchedEmails | Returned | P1 |
| FUN-020 | Suggestions | Unmatched | GetUnmatchedEmailsWithSuggestions | Suggestions | P1 |
| FUN-021 | Thread linking | Get thread | GetEmailThread | Linked | P1 |
| FUN-022 | Link email interaction | Link | LinkEmailToInteraction | Linked | P1 |
| FUN-023 | Batch import | Multiple | BatchImportContacts | All created | P1 |
| FUN-024 | Token validation | Validate | ValidateToken | Valid/Invalid | P1 |
| FUN-025 | Scope validation | Check scope | CheckOAuthScope | Allowed/Denied | P1 |
| FUN-026 | State validation | OAuth | ValidateOAuthState | Valid | P0 |
| FUN-027 | Metadata extraction | Email | ExtractMetadata | Extracted | P1 |
| FUN-028 | Attachment handling | With attachments | CreateInteractionFromEmail | Attachments | P1 |
| FUN-029 | Multiple recipients | To multiple | MatchEmailToContact | All matched | P1 |
| FUN-030 | CC/BCC handling | CC/BCC | CreateInteractionFromEmail | Handled | P1 |
| FUN-031 | Reply chain | In reply | GetEmailThread | Chain | P1 |
| FUN-032 | Label filter | Filter | SyncEmails | Filtered | P1 |
| FUN-033 | Date range | Sync | SyncEmails | Range | P1 |
| FUN-034 | Contact update | Existing | ImportContactFromEmail | Updated | P1 |
| FUN-035 | Interaction update | Existing | CreateInteractionFromEmail | Per design | P1 |
| FUN-036 | Dedup rule | Same person | DeduplicateContacts | Per rule | P1 |
| FUN-037 | Sync conflict | Concurrent | SyncEmails | Handled | P1 |
| FUN-038 | Token expiry | Expired | RefreshToken | Refreshed | P0 |
| FUN-039 | Optimistic concurrency | Concurrent update | Update | Conflict | P1 |
| FUN-040 | Rate limit | Over limit | SyncEmails | 429 | P0 |
| FUN-041 | Gmail API version | API version | SyncEmails | Correct version | P1 |
| FUN-042 | Error retry | API fail | SyncEmails | Retry | P1 |
| FUN-043 | Partial sync | Partial fail | SyncEmails | Per design | P1 |
| FUN-044 | Sync cursor | Pagination | SyncEmails | Cursor | P1 |
| FUN-045 | Contact merge | Duplicate | ImportContactFromEmail | Merged | P2 |
| FUN-046 | Interaction dedup | Duplicate | CreateInteractionFromEmail | Deduped | P2 |
| FUN-047 | Email template | Template | CreateInteractionFromEmail | Template | P2 |
| FUN-048 | Notification on sync | Sync complete | SyncEmails | Notification | P2 |
| FUN-049 | Metrics | Track | SyncEmails | Metrics | P2 |
| FUN-050 | Health check | Health | Gmail API | Status | P2 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full OAuth flow | Exchange→Refresh→Disconnect | GmailAddonManager | All succeed | P0 |
| INT-002 | Full sync flow | Sync→GetStatus | GmailAddonManager | Synced | P0 |
| INT-003 | ContactManager | Get contacts | GmailAddonManager, ContactManager | Contacts | P0 |
| INT-004 | InteractionManager | Create interaction | GmailAddonManager, InteractionManager | Interaction | P0 |
| INT-005 | UserContext | Current user | GmailAddonManager, UserResolver | UserId | P0 |
| INT-006 | Permission | Authorize | GmailAddonManager, PermissionService | Correct | P0 |
| INT-007 | Audit | Audit | GmailAddonManager, AuditLog | Entries | P1 |
| INT-008 | DbContext | Persist | GmailAddonManager, DbContext | Saved | P0 |
| INT-009 | Gmail API | API call | GmailAddonManager, Gmail API | Response | P0 |
| INT-010 | OAuth provider | Google OAuth | GmailAddonManager | Token | P0 |
| INT-011 | Controller | API | GmailAddonManager, Controller | 200/201 | P0 |
| INT-012 | Error handling | Exception | GmailAddonManager, Handler | Consistent | P1 |
| INT-013 | Logging | Log | GmailAddonManager, ILogger | Logs | P2 |
| INT-014 | Configuration | Config | GmailAddonManager | Applied | P2 |
| INT-015 | PartnerManager | Partner data | GmailAddonManager | Partner | P1 |
| INT-016 | OpportunityManager | Opp data | GmailAddonManager | Opportunity | P1 |
| INT-017 | Multi-tenant | Org scope | GmailAddonManager | Isolated | P0 |
| INT-018 | ManagerWrapper | Resolution | ManagerWrapper | Correct | P1 |
| INT-019 | API 404 | Get invalid | Controller | 404 | P0 |
| INT-020 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-021 | API 401 | Unauthorized | Controller | 401 | P0 |
| INT-022 | API 429 | Rate limit | Controller | 429 | P0 |
| INT-023 | DocumentManager | Attachments | GmailAddonManager | Documents | P1 |
| INT-024 | NotificationManager | Notify | GmailAddonManager | Notification | P2 |
| INT-025 | Token storage | Store token | GmailAddonManager | Encrypted | P0 |
| INT-026 | Add-on UI | Add-on | GmailAddonManager | Displayed | P1 |
| INT-027 | Contextual trigger | Email open | GmailAddonManager | Triggered | P1 |
| INT-028 | Compose trigger | Compose | GmailAddonManager | Triggered | P1 |
| INT-029 | Universal action | Action | GmailAddonManager | Action | P1 |
| INT-030 | Card builder | Card | GmailAddonManager | Card | P1 |
| INT-031 | Gmail API v1 | API version | GmailAddonManager | v1 | P1 |
| INT-032 | Batch request | Batch | GmailAddonManager | Batched | P1 |
| INT-033 | Retry policy | Retry | GmailAddonManager | Retries | P1 |
| INT-034 | Circuit breaker | API fail | GmailAddonManager | Open | P2 |
| INT-035 | Timeout | Timeout | GmailAddonManager | Timeout | P1 |
| INT-036 | Health check | Health | GmailAddonManager | Status | P2 |
| INT-037 | Metrics | Metrics | GmailAddonManager | Recorded | P2 |
| INT-038 | Feature flag | Feature | GmailAddonManager | Respected | P2 |
| INT-039 | Migration | Add OAuth | GmailAddonManager | Migrated | P2 |
| INT-040 | Token storage migration | Migrate tokens | GmailAddonManager | Migrated | P2 |
| INT-041 | Email parser | Parse | GmailAddonManager | Parsed | P1 |
| INT-042 | Metadata extraction | Extract | GmailAddonManager | Extracted | P1 |
| INT-043 | Validation rules | Validate | GmailAddonManager | Validated | P1 |
| INT-044 | Rate limit service | Rate limit | GmailAddonManager | Enforced | P0 |
| INT-045 | Cache | Cache | GmailAddonManager | Cached | P2 |
| INT-046 | Queue | Queue sync | GmailAddonManager | Queued | P2 |
| INT-047 | Background job | Background sync | GmailAddonManager | Job | P2 |
| INT-048 | Webhook | Webhook | GmailAddonManager | Received | P2 |
| INT-049 | Push notification | Push | GmailAddonManager | Pushed | P2 |
| INT-050 | Consent | Consent | GmailAddonManager | Recorded | P1 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection email | ' OR 1=1-- | GetContactsForGmailAddon | Sanitized | P0 |
| SEC-002 | XSS in subject | <script>alert(1)</script> | CreateInteractionFromEmail | Sanitized | P0 |
| SEC-003 | IDOR sync | SyncEmails(otherUserId) | SyncEmails | 403 | P0 |
| SEC-004 | IDOR tokens | GetToken(otherUserId) | GetToken | 403 | P0 |
| SEC-005 | IDOR import | Import for other | ImportContactFromEmail | 403 | P0 |
| SEC-006 | Mass assignment | Include Id | ImportContactFromEmail | Ignored | P0 |
| SEC-007 | Unauthenticated | No auth | Any op | 401 | P0 |
| SEC-008 | Expired token | Expired JWT | Request | 401 | P0 |
| SEC-009 | Wrong role | No permission | SyncEmails | 403 | P0 |
| SEC-010 | Org scope bypass | Cross-org | GetRelatedRecords | 403 | P0 |
| SEC-011 | OAuth token theft | Stolen token | Use | Detected | P0 |
| SEC-012 | OAuth state bypass | No state | Callback | Rejected | P0 |
| SEC-013 | OAuth redirect hijack | Redirect manip | Callback | Validated | P0 |
| SEC-014 | Email header injection | \r\nBcc: | Email | Sanitized | P0 |
| SEC-015 | Attachment virus | Infected | CreateInteractionFromEmail | Rejected | P0 |
| SEC-016 | Rate limit bypass | Bypass | SyncEmails | Rejected | P0 |
| SEC-017 | Replay attack | Replay | ExchangeToken | Rejected | P0 |
| SEC-018 | JWT alg none | alg=none | Request | Rejected | P0 |
| SEC-019 | Brute force | Enumerate | GetRelatedRecords | Rate limited | P1 |
| SEC-020 | CSRF OAuth | Cross-site | OAuth callback | State validated | P0 |
| SEC-021 | CSRF sync | Cross-site | SyncEmails | Token validated | P0 |
| SEC-022 | Log injection | Malicious log | Log | Sanitized | P1 |
| SEC-023 | Header injection | Malicious header | Request | Sanitized | P1 |
| SEC-024 | Parameter pollution | userId=1&userId=2 | Request | Handled | P1 |
| SEC-025 | Open redirect | Redirect | OAuth callback | Validated | P0 |
| SEC-026 | Session fixation | Fixate | OAuth | New session | P1 |
| SEC-027 | Token in URL | Token in query | Request | Avoided | P1 |
| SEC-028 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| SEC-029 | Info disclosure | Probe | Invalid | Generic | P1 |
| SEC-030 | Token storage plain | Store | ExchangeToken | Encrypted | P0 |
| SEC-031 | Refresh token exposure | Log | RefreshToken | Not logged | P0 |
| SEC-032 | Scope escalation | Add scope | Request | Rejected | P0 |
| SEC-033 | Timing attack | Response time | GetToken | Constant | P2 |
| SEC-034 | Cache poisoning | Malicious cache | Cache | Sanitized | P1 |
| SEC-035 | Substitution attack | Replace JWT | Request | 403 | P0 |
| SEC-036 | Cookie manipulation | Modify auth | Request | Rejected | P0 |
| SEC-037 | Path traversal | ../../../ | Attachment | Rejected | P0 |
| SEC-038 | Null byte | %00 | Filename | Rejected | P0 |
| SEC-039 | LDAP injection | *)(uid=* | Filter | Sanitized | P0 |
| SEC-040 | Privilege escalation | Admin action | User | 403 | P0 |
| SEC-041 | Excessive data | Huge request | SyncEmails | Rejected | P1 |
| SEC-042 | DoS sync | Many syncs | SyncEmails | 429 | P0 |
| SEC-043 | DoS import | Many imports | ImportContactFromEmail | Rate limited | P0 |
| SEC-044 | Insecure reference | EmailId manip | Get | Validated | P0 |
| SEC-045 | Token expiry | Expired | Request | 401 | P0 |
| SEC-046 | Consent bypass | No consent | SyncEmails | Blocked | P0 |
| SEC-047 | Audit bypass | Skip audit | Any op | Audit required | P0 |
| SEC-048 | PII in log | PII | Log | Not logged | P0 |
| SEC-049 | API key leak | Key in response | GenerateContent | Not exposed | P0 |
| SEC-050 | Redirect URI validation | Invalid URI | OAuth | Rejected | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent get contacts | 20 threads GetContactsForGmailAddon | All correct | P0 |
| CON-002 | Concurrent sync same user | 5 threads SyncEmails(userId) | One succeeds | P0 |
| CON-003 | Concurrent import | 10 threads ImportContactFromEmail | All created | P0 |
| CON-004 | Concurrent OAuth | 2 threads ExchangeToken | One succeeds | P0 |
| CON-005 | Create and get | Thread1 create, Thread2 get | Consistent | P1 |
| CON-006 | Sync and get status | Thread1 sync, Thread2 status | Consistent | P1 |
| CON-007 | Token refresh concurrent | 2 threads RefreshToken | One succeeds | P0 |
| CON-008 | Optimistic concurrency | 2 users update | Conflict | P0 |
| CON-009 | Connection pool | 100 concurrent | No exhaustion | P1 |
| CON-010 | Deadlock | Circular | No deadlock | P1 |
| CON-011 | Double submit | User double-clicks | One created | P0 |
| CON-012 | Race on contact | 2 threads same email | One created | P1 |
| CON-013 | Race on interaction | 2 threads same email | Handled | P1 |
| CON-014 | Dedup concurrent | 2 threads dedup | Consistent | P1 |
| CON-015 | Token update concurrent | 2 threads refresh | One wins | P0 |
| CON-016 | Disconnect concurrent | 2 threads disconnect | Handled | P1 |
| CON-017 | List during sync | Thread1 sync, Thread2 list | Consistent | P1 |
| CON-018 | Batch import concurrent | 2 threads batch | Consistent | P1 |
| CON-019 | Transaction isolation | Read uncommitted | Per level | P1 |
| CON-020 | Lost update | 2 users different | Per design | P1 |
| CON-021 | Phantom read | Insert during list | Per isolation | P2 |
| CON-022 | Non-repeatable read | Update between reads | Per isolation | P2 |
| CON-023 | Gmail API rate limit | Shared limit | Enforced | P0 |
| CON-024 | Cache consistency | Concurrent cache | Consistent | P1 |
| CON-025 | Sync queue | Concurrent syncs | Queued | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Email validation | Validation | "user@example.com" | Valid | P0 |
| UNT-002 | Email invalid | Validation | "invalid" | Invalid | P0 |
| UNT-003 | Email empty | Validation | "" | Invalid | P0 |
| UNT-004 | Token validation | Validation | Valid token | Valid | P0 |
| UNT-005 | Token invalid | Validation | Invalid token | Invalid | P0 |
| UNT-006 | Email trim | Formatting | "  user@example.com  " | Trimmed | P1 |
| UNT-007 | Subject trim | Formatting | "  Subject  " | Trimmed | P1 |
| UNT-008 | Dedup logic | Calculation | Duplicate emails | Deduped | P1 |
| UNT-009 | Match logic | Calculation | Email match | Match | P1 |
| UNT-010 | Status active | Status logic | Sync running | Active | P1 |
| UNT-011 | Status complete | Status logic | Sync done | Complete | P1 |
| UNT-012 | Token expired | Status logic | Expired | Expired | P0 |
| UNT-013 | Collection filter | Collections | List with deleted | Excluded | P1 |
| UNT-014 | Empty collection | Collections | No contacts | Count=0 | P1 |
| UNT-015 | Null to empty | Collections | Null list | [] | P1 |
| UNT-016 | Map to Model | Mapping | Entity | Model | P0 |
| UNT-017 | Map Request | Mapping | Request | Entity | P0 |
| UNT-018 | Pagination slice | Calculation | Page 1, Size 10 | Skip 10, Take 10 | P1 |
| UNT-019 | OAuth state generate | Calculation | Generate | Random | P1 |
| UNT-020 | Token encrypt | Calculation | Token | Encrypted | P0 |
| UNT-021 | Audit fields | Status logic | New record | CreatedBy set | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Get contacts | GetContactsForGmailAddon | < 500ms | P0 |
| PRF-002 | Get related records | GetRelatedRecords | < 500ms | P0 |
| PRF-003 | OAuth exchange | ExchangeToken | < 1000ms | P0 |
| PRF-004 | Token refresh | RefreshToken | < 500ms | P0 |
| PRF-005 | Import contact | ImportContactFromEmail | < 300ms | P0 |
| PRF-006 | Create interaction | CreateInteractionFromEmail | < 500ms | P0 |
| PRF-007 | Sync emails | SyncEmails (100) | < 5000ms | P0 |
| PRF-008 | Dedup | DeduplicateContacts (100) | < 500ms | P0 |
| PRF-009 | Get unmatched | GetUnmatchedEmails (50) | < 500ms | P1 |
| PRF-010 | Get sync status | GetSyncStatus | < 100ms | P1 |
| PRF-011 | Get email thread | GetEmailThread | < 500ms | P1 |
| PRF-012 | Batch import | BatchImportContacts (50) | < 5000ms | P1 |
| PRF-013 | Memory 100 emails | GetContactsForGmailAddon | < 50MB | P1 |
| PRF-014 | Concurrent 20 | 20 GetContactsForGmailAddon | < 1000ms each | P1 |
| PRF-015 | Cold start | First GetRelatedRecords | < 500ms | P2 |
| PRF-016 | Cached | Second GetRelatedRecords | < 100ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained 20 req/s get | 20 GetContactsForGmailAddon/sec | 5 min | 95% < 500ms | P0 |
| LDT-002 | Sustained 10 req/s import | 10 Import/sec | 5 min | 95% < 500ms | P0 |
| LDT-003 | Sustained 5 req/s sync | 5 SyncEmails/sec | 5 min | 95% < 5000ms | P0 |
| LDT-004 | Spike 50 req/s | 50 req/s burst | 1 min | No crash | P0 |
| LDT-005 | Spike 100 req/s | 100 req/s | 30 sec | Graceful degrade | P1 |
| LDT-006 | Stress ramp | 1→200 req/s | Until fail | Find limit | P1 |
| LDT-007 | Connection pool | 100 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Memory | 1K syncs | 5 min | No leak | P1 |
| LDT-009 | Recovery spike | Spike then normal | 5 min | Baseline | P0 |
| LDT-010 | Recovery stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
