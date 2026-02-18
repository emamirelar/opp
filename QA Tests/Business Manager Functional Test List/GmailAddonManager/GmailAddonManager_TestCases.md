# GmailAddonManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/GmailAddonManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30-50 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Concurrency (CON) | 25 | 25 | ✅ |
| §7 Unit (UNT) | 21 | 21 | ✅ |
| §8 Performance (PRF) | 16 | 16 | ✅ |
| §9 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Formula |
|-------|--------|---------|
| N≥3P? | ✅ | 90 ≥ 90 |
| E≥3P? | ✅ | 90 ≥ 90 |
| F≥3P? | ✅ | 90 ≥ 90 |
| I≥3P? | ✅ | 90 ≥ 90 |

---

## Feature Overview

**GmailAddonManager** manages email sync, contact import, interaction creation from email, OAuth, and deduplication. Key responsibilities: Gmail Add-on integration, email-to-contact matching, interaction creation from emails, OAuth token management, sync/dedup logic, and related records retrieval.

---

## §1 Positive Tests (30)

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
---

## §2 Negative Tests (90)

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

| NEG-071 | FindRelatedRecords — null request | Request=null | NullReferenceException or 500 | P1 |
| NEG-072 | FindRelatedRecords — null EmailAddresses | EmailAddresses=null | NullReferenceException or handled | P1 |
| NEG-073 | CreateRecordsFromEmails — null SelectedContacts | SelectedContacts=null | ArgumentException | P1 |
| NEG-074 | CreateRecordsFromEmails — empty SelectedContacts | SelectedContacts=[] | ArgumentException "No emails selected" | P1 |
| NEG-075 | CreateRecordsFromEmails — no contact permission | User lacks Contact create | UnauthorizedAccessException | P1 |
| NEG-076 | CreateRecordsFromEmails — no partner permission | User lacks Partner create | UnauthorizedAccessException | P1 |
| NEG-077 | CreateRecordsFromEmails — invalid email format | EmailAddress="not-an-email" | FailedEmails populated | P1 |
| NEG-078 | CreateRecordsFromEmails — null user | user=null | Exception or handled | P1 |
| NEG-079 | CreateRecordsFromEmails — empty EmailAddress | EmailAddress="" | Contact creation fails, FailedEmails | P1 |
| NEG-080 | CreateRecordsFromEmails — PartnerManager throws | CreatePartnerAsync throws | FailedEmails for that partner's emails | P1 |
| NEG-081 | CreateRecordsFromEmails — ContactManager throws | CreateContactAsync throws | FailedEmails populated | P1 |
| NEG-082 | CreateRecordsFromEmails — GetPartnerByNameAsync throws | Partner lookup throws | Exception propagated | P1 |
| NEG-083 | CreateRecordsFromEmails — GetContactByEmailAsync throws | Contact lookup throws | Exception propagated | P1 |
| NEG-084 | FindRelatedRecords — UserDataManager throws | GetUsersByEmailsAsync throws | Users empty, contacts/partners returned | P1 |
| NEG-085 | FindRelatedRecords — UserInfoService throws | GetUserInfosByEmailsAsync throws | Users with fallback data | P1 |
| NEG-086 | CreateRecordsFromEmails — invalid PartnerId | PartnerId=99999 | Partner creation fails | P1 |
| NEG-087 | CreateRecordsFromEmails — null user claims | GetUserIdFromClaims returns 0 | Notifications skipped | P1 |
| NEG-088 | FindRelatedRecords — ContactManager throws | GetContactsForGmailAddon throws | Exception propagated | P1 |
| NEG-089 | FindRelatedRecords — PartnerManager throws | GetPartnersForGmailAddon throws | Exception propagated | P1 |
| NEG-090 | CreateRecordsFromEmails — InteractionManager throws | UpdateInteractionAsync throws | Records created, notification sent | P1 |

---

## §3 Boundary Tests (90)

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

| BND-071 | SelectedContacts count | 1 | 100 | 1 | 100 | 101 | P1 |
| BND-072 | GmailThreadId length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-073 | GmailMessageId length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-074 | PartnerName length | 0 | 500 | "" | 500 chars | 501 chars | P1 |
| BND-075 | FirstName length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-076 | LastName length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-077 | EmailAddress domain | 1 | 255 | "a@b.c" | 320 chars | 321 chars | P1 |
| BND-078 | partnerIds count | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-079 | UnmatchedEmails count | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-080 | Contacts per partner | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-081 | Interactions per contact | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-082 | PartnerName from domain | 1 char | 255 | "a@b.c" | 320 chars | — | P1 |
| BND-083 | ExtractNameFromEmail | 1 part | 2+ parts | "user" | "first.last" | — | P1 |
| BND-084 | CreatedPartners dict | 0 | 100 | 0 | 100 | — | P1 |
| BND-085 | FailedEmails list | 0 | 100 | 0 | 100 | — | P1 |
| BND-086 | Email prefix for name | 0 | 100 | "" | 100 chars | — | P1 |
| BND-087 | MiddleName | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-088 | PartnerId nullable | 0 | Max int | null | Max int | — | P1 |
| BND-089 | EmailAddresses empty | — | — | [] | — | — | P1 |
| BND-090 | Single email in list | — | — | 1 email | — | — | P1 |

---

## §4 Functional Tests (90)

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

| FUN-051 | CanCreateContacts permission | Permission check | FindRelatedRecordsAsync | CanCreateContacts set | P1 |
| FUN-052 | CanCreatePartners permission | Permission check | FindRelatedRecordsAsync | CanCreatePartners set | P1 |
| FUN-053 | CanCreateInteractions permission | Permission check | FindRelatedRecordsAsync | CanCreateInteractions set | P1 |
| FUN-054 | Contact match removes from unmatched | Email match | ProcessContactsAsync | unmatchedEmailStrings.Remove | P1 |
| FUN-055 | Partner match by contactPartnerIds | Contact has partner | ProcessPartnersAsync | Partners returned | P1 |
| FUN-056 | User match removes from unmatched | User email match | ProcessUsersAsync | unmatchedEmailStrings.Remove | P1 |
| FUN-057 | Unmatched emails get suggestions | Unmatched list | GetUnmatchedEmailsWithPartnerSuggestionsAsync | UnmatchedEmails populated | P1 |
| FUN-058 | Partner name from domain | No PartnerName | GetPartnerNameFromEmail | Domain derived | P1 |
| FUN-059 | Partner name from provided | PartnerName set | GetPartnerNameFromEmail | PartnerName returned | P1 |
| FUN-060 | Partner dedup by name | Same partner name | ProcessPartnersForCreationAsync | One partner created | P1 |
| FUN-061 | Contact skip if exists | Email exists | ProcessSingleContactAsync | ExistingContacts | P1 |
| FUN-062 | Contact creation | New email | ProcessSingleContactAsync | CreatedContacts | P1 |
| FUN-063 | PartnerId from SelectedContact | PartnerId set | GetPartnerIdForContact | PartnerId used | P1 |
| FUN-064 | PartnerId from CreatedPartners | Partner created | GetPartnerIdForContact | PartnerId from state | P1 |
| FUN-065 | LastName fallback from FirstName | Empty LastName | CreateContactRequest | FirstName as LastName | P1 |
| FUN-066 | LastName from email prefix | No name | CreateContactRequest | Email prefix capitalized | P1 |
| FUN-067 | ExtractNameFromEmail | first.last format | ExtractNameFromEmail | FirstName, LastName | P1 |
| FUN-068 | Interaction update with contacts | GmailThreadId provided | UpdateExistingInteractionAsync | ContactIds added | P1 |
| FUN-069 | Interaction update with partners | GmailMessageId provided | UpdateExistingInteractionAsync | PartnerIds added | P1 |
| FUN-070 | Skip interaction update | No GmailThreadId/MessageId | UpdateExistingInteractionAsync | Skipped | P1 |
| FUN-071 | Notification on contact creation | Contacts created | SendCreationNotificationsAsync | Notification sent | P1 |
| FUN-072 | Notification on partner creation | Partners created | SendCreationNotificationsAsync | Notification sent | P1 |
| FUN-073 | Combined notification | Contacts + partners | SendCreationNotificationsAsync | Single combined | P1 |
| FUN-074 | No notification | Nothing created | SendCreationNotificationsAsync | No notification | P1 |
| FUN-075 | FailedEmails on partner failure | Partner creation throws | ProcessPartnersForCreationAsync | FailedEmails populated | P1 |
| FUN-076 | FailedEmails on contact failure | Contact creation throws | ProcessContactsForCreationAsync | FailedEmails populated | P1 |
| FUN-077 | Skip contact if failed in partner | Partner failed | ProcessSingleContactAsync | Skipped | P1 |
| FUN-078 | MapContactToGmailContact CanRead | CanRead=false | MapContactToGmailContact | Minimal fields | P1 |
| FUN-079 | MapPartnerToGmailPartner CanRead | CanRead=false | MapPartnerToGmailPartner | Minimal fields | P1 |
| FUN-080 | MapInteractionsToGmailInteractions | CanRead filter | MapInteractionsToGmailInteractions | Only CanRead | P1 |
| FUN-081 | BuildResultMessage | Success | BuildCreateRecordsResult | Message with stats | P1 |
| FUN-082 | BuildResultMessage | Failed emails | BuildCreateRecordsResult | Failed count in message | P1 |
| FUN-083 | Success true | Contacts or existing | BuildCreateRecordsResult | Success=true | P1 |
| FUN-084 | Success false | All failed | BuildCreateRecordsResult | Success=false | P1 |
| FUN-085 | GetPartnersToCreate | PartnerId set | GetPartnersToCreate | Excluded | P1 |
| FUN-086 | GetPartnersToCreate | Group by name | GetPartnersToCreate | One per group | P1 |
| FUN-087 | UserProfile fallback | No UserProfile | MapUserToGmailUser | user.Email | P1 |
| FUN-088 | UserProfile name | UserProfile exists | MapUserToGmailUser | userProfile.Name | P1 |
| FUN-089 | contactPartnerIds dedup | Multiple contacts same partner | ProcessContactsAsync | No duplicates | P1 |
| FUN-090 | Case-insensitive email match | Mixed case | FindRelatedRecordsAsync | Matched | P1 |

---

## §5 Integration Tests (90)

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

| INT-051 | FindRelatedRecords — ContactManager | GetContactsForGmailAddon | GmailAddonManager, ContactManager | Contacts in response | P1 |
| INT-052 | FindRelatedRecords — PartnerManager | GetPartnersForGmailAddon | GmailAddonManager, PartnerManager | Partners in response | P1 |
| INT-053 | FindRelatedRecords — UserDataManager | GetUsersByEmailsAsync | GmailAddonManager, UserDataManager | Users in response | P1 |
| INT-054 | FindRelatedRecords — UserInfoService | GetUserInfosByEmailsAsync | GmailAddonManager, UserInfoService | UserProfile data | P1 |
| INT-055 | FindRelatedRecords — ContactManager unmatched | GetUnmatchedEmailsWithPartnerSuggestionsAsync | GmailAddonManager, ContactManager | UnmatchedEmails | P1 |
| INT-056 | CreateRecordsFromEmails — PartnerManager | GetPartnerByNameAsync, CreatePartnerAsync | GmailAddonManager, PartnerManager | Partner created | P1 |
| INT-057 | CreateRecordsFromEmails — ContactManager | GetContactByEmailAsync, CreateContactAsync | GmailAddonManager, ContactManager | Contact created | P1 |
| INT-058 | CreateRecordsFromEmails — InteractionManager | FindGmailInteractionAsync, UpdateInteractionAsync | GmailAddonManager, InteractionManager | Interaction updated | P1 |
| INT-059 | CreateRecordsFromEmails — NotificationManager | CreateNotification | GmailAddonManager, NotificationManager | Notification created | P1 |
| INT-060 | FindRelatedRecords full flow | Contact→Partner→User→Unmatched | All managers | Complete response | P1 |
| INT-061 | CreateRecordsFromEmails full flow | Partner→Contact→Interaction→Notify | All managers | Records created | P1 |
| INT-062 | contactPartnerIds passed to PartnerManager | ProcessContactsAsync | ContactManager, PartnerManager | partnerIds in request | P1 |
| INT-063 | GmailContact mapping | ContactModel | MapContactToGmailContact | GmailRelatedContact | P1 |
| INT-064 | GmailPartner mapping | PartnerModel | MapPartnerToGmailPartner | GmailRelatedPartner | P1 |
| INT-065 | GmailUser mapping | PAOUserModel | MapUserToGmailUser | GmailRelatedUser | P1 |
| INT-066 | Contact interactions mapping | Contact.Interactions | MapInteractionsToGmailInteractions | GmailRelatedInteraction list | P1 |
| INT-067 | Partner contacts mapping | Partner.Contacts | MapContactsToGmailContacts | GmailRelatedContact list | P1 |
| INT-068 | PermissionService | CanPerformActionAsync | GmailAddonManager, PermissionService | CanCreate* flags | P1 |
| INT-069 | Controller FindRelatedRecords | POST find-related-records | GmailAddonController, GmailAddonManager | 200 OK | P1 |
| INT-070 | Controller CreateRecordsFromEmails | POST create-records | GmailAddonController, GmailAddonManager | 200 OK | P1 |
| INT-071 | Controller CreateRecordsFromEmails 400 | Bad request | GmailAddonController | 400 BadRequest | P1 |
| INT-072 | Controller CreateRecordsFromEmails 401 | Unauthorized | GmailAddonController | 401/403 | P1 |
| INT-073 | CreateRecordsFromEmails — PartnerRequest | CreatePartnerRequest | ContactRequest, PartnerRequest | Partner created | P1 |
| INT-074 | CreateRecordsFromEmails — ContactRequest | CreateContactRequest | ContactRequest, PartnerId | Contact created | P1 |
| INT-075 | UpdateInteractionRequest | UpdateInteractionAsync | InteractionModel, UpdateInteractionRequest | Interaction updated | P1 |
| INT-076 | GmailInteractionRequest | FindGmailInteractionAsync | GmailThreadId, GmailMessageId | Interaction found | P1 |
| INT-077 | EntityStatus in PartnerRequest | CreatePartnerRequest | Status=Draft | Partner created | P1 |
| INT-078 | EntityStatus in ContactRequest | CreateContactRequest | Status=Active | Contact created | P1 |
| INT-079 | DbContext | SaveChanges | GmailAddonManager, DbContext | Persisted | P1 |
| INT-080 | HttpContextAccessor | GetValidAudienceForCurrentHost | GmailAddonManager | Host URL | P1 |
| INT-081 | GetUserIdFromClaims | NameIdentifier | SendCreationNotificationsAsync | UserId | P1 |
| INT-082 | GetUserIdFromClaims fallback | sub, userId | SendCreationNotificationsAsync | UserId | P1 |
| INT-083 | RecordData in notification | ContactIds, PartnerNames | CreateNotification | RecordData JSON | P1 |
| INT-084 | Category gmail_records_creation | Combined creation | CreateNotification | Category set | P1 |
| INT-085 | Category gmail_contact_creation | Contact only | CreateNotification | Category set | P1 |
| INT-086 | Category gmail_partner_creation | Partner only | CreateNotification | Category set | P1 |
| INT-087 | ILogger | LogError, LogInformation | UNOPSGmailAddonManager | Logs written | P1 |
| INT-088 | AutoMapper | Map entity to model | GmailAddonManager | Mapped | P1 |
| INT-089 | Base GmailAddonManager | NotImplemented | ManagerWrapper | UNOPS override | P1 |
| INT-090 | ManagerWrapper resolution | IsUNOPSOverride | ManagerWrapper, GmailAddonManager | UNOPSGmailAddonManager | P1 |

---

## §6 Concurrency Tests (25)

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

## §7 Unit Tests (21)

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

## §8 Performance Tests (16)

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

## §9 Load Tests (10)

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
