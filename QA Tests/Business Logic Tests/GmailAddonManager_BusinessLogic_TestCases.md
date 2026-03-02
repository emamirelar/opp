# GmailAddonManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGmailAddonManager`  
**Created:** 2026-02-18  
**Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | Max(50, 3×30)=90 | ✅ |
| Boundary Tests | §3 | 90 | Max(50, 3×30)=90 | ✅ |
| Functional Tests | §4 | 90 | ≥90 | ✅ |
| Integration Tests | §5 | 90 | ≥90 | ✅ |
| Security Tests | §6 | 50 | OUT OF SCOPE | — |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

GmailAddonManager handles **email sync** and **record creation from Gmail**. Key business rules: **FindRelatedRecordsAsync** (match email addresses to contacts, partners, users; unmatched emails with partner suggestions), **CreateRecordsFromEmailsAsync** (create contacts/partners from selected emails; partner name from domain or user-provided; contact name extraction from email prefix; interaction update with GmailThreadId/GmailMessageId; notification triggers), **partner matching** (domain to partner records), **contact matching** (email to contact records), **selected contact handling** (user selects which contacts to link), **duplicate interaction prevention** (threadId), **interaction update** when email thread continues, **error handling** (missing partner, missing contact, invalid email), **notification triggers** (notify when interaction/contact/partner created), **organization unit association** (UserProfile OrgUnit), **name extraction** from email (ExtractNameFromEmail: split by . _ -).

---

## §1 Positive Tests (Happy Path) — 30 tests

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | FindRelatedRecords — matching contacts | Contact john@acme.com exists | FindRelatedRecordsAsync(EmailAddresses=["john@acme.com"]) | Contacts list includes john, unmatched empty | P0 |
| POS-002 | FindRelatedRecords — matching partners | Partner "ACME" has contact john@acme.com | FindRelatedRecordsAsync(EmailAddresses=["john@acme.com"]) | Partners list includes ACME | P0 |
| POS-003 | FindRelatedRecords — matching users | User jane@unops.org exists | FindRelatedRecordsAsync(EmailAddresses=["jane@unops.org"]) | Users list includes jane | P0 |
| POS-004 | FindRelatedRecords — unmatched emails | Email x@unknown.com not in system | FindRelatedRecordsAsync(EmailAddresses=["x@unknown.com"]) | UnmatchedEmails with partner suggestions | P0 |
| POS-005 | CreateRecordsFromEmails — new contact | Email new@acme.com, Partner "ACME" exists | CreateRecordsFromEmailsAsync(SelectedContacts=[{EmailAddress, PartnerId}]) | Contact created | P0 |
| POS-006 | CreateRecordsFromEmails — new partner | Email new@newcorp.com, no partner | CreateRecordsFromEmailsAsync(SelectedContacts=[{EmailAddress}]) | Partner "Newcorp" created, contact created | P0 |
| POS-007 | CreateRecordsFromEmails — existing contact | Contact new@acme.com exists | CreateRecordsFromEmailsAsync | Existing contact found, skipped | P0 |
| POS-008 | CreateRecordsFromEmails — existing partner | Partner "ACME" exists by name | CreateRecordsFromEmailsAsync | Partner found, contact linked | P0 |
| POS-009 | Partner name from domain | Email john@acme.com | GetPartnerNameFromEmail | "Acme" (domain capitalized) | P0 |
| POS-010 | Partner name from user | SelectedContact.PartnerName="ACME Corp" | GetPartnerNameFromEmail | "ACME Corp" | P0 |
| POS-011 | Name extraction from email | Email john.doe@acme.com | ExtractNameFromEmail("john.doe") | FirstName="John", LastName="Doe" | P0 |
| POS-012 | Name extraction single part | Email johndoe@acme.com | ExtractNameFromEmail("johndoe") | FirstName="Johndoe", LastName="" | P1 |
| POS-013 | Update existing interaction | GmailThreadId, GmailMessageId provided, interaction exists | CreateRecordsFromEmailsAsync | Interaction updated with new contact/partner IDs | P0 |
| POS-014 | Notification on contact creation | Contact created | CreateRecordsFromEmailsAsync | Notification sent | P1 |
| POS-015 | Notification on partner creation | Partner created | CreateRecordsFromEmailsAsync | Notification sent | P1 |
| POS-016 | Permissions initialized | FindRelatedRecords | CanCreateContacts, CanCreatePartners, CanCreateInteractions set | P0 |
| POS-017 | Contact with Partner | Contact has Partner | MapContactToGmailContact | PartnerName in response | P1 |
| POS-018 | Partner with Contacts | Partner has contacts | MapPartnerToGmailPartner | Contacts in response | P1 |
| POS-019 | User with OrgUnit | UserProfile has OrgUnit | MapUserToGmailUser | OrgUnit in response | P1 |
| POS-020 | Case-insensitive email match | Contact JOHN@acme.com | FindRelatedRecords(["john@acme.com"]) | Matched | P1 |
| POS-021 | Multiple emails | 5 emails, 3 match | FindRelatedRecords | 3 in Contacts/Partners/Users, 2 in UnmatchedEmails | P1 |
| POS-022 | SelectedContacts with PartnerId | PartnerId=123 provided | CreateRecordsFromEmailsAsync | Contact linked to partner 123 | P0 |
| POS-023 | SelectedContacts with PartnerName | PartnerName="ACME" | CreateRecordsFromEmailsAsync | Partner found or created | P0 |
| POS-024 | CreateContactRequest LastName fallback | No FirstName/LastName | CreateContactRequest | LastName from email prefix | P0 |
| POS-025 | CreateContactRequest FirstName as LastName | FirstName only | CreateContactRequest | LastName=FirstName | P1 |
| POS-026 | GetPartnersToCreate — unique by name | 3 emails same domain | GetPartnersToCreate | One partner to create | P1 |
| POS-027 | ProcessSinglePartnerAsync — existing | Partner "ACME" exists | ProcessSinglePartnerAsync | state.CreatedPartners["ACME"]=id | P0 |
| POS-028 | ProcessSingleContactAsync — skip failed | Email in state.FailedEmails | ProcessSingleContactAsync | Skipped | P1 |
| POS-029 | BuildCreateRecordsResult | state with created contacts | BuildCreateRecordsResult | CreatedContacts, CreatedPartners, FailedEmails, Message | P0 |
| POS-030 | GetContactsForGmailAddon | ContactManager | GetContactsForGmailAddon | Contact list | P0 |

---

## §2 Negative Tests — 90 tests

### 2.1 Invalid Input (20)
| ID | Invalid Input | Expected | Priority |
|----|--------------|----------|----------|
| NEG-001 | FindRelatedRecords — null input | FindRelatedRecordsAsync(null, user) | ArgumentNullException | P0 |
| NEG-002 | FindRelatedRecords — null user | FindRelatedRecordsAsync(input, null) | NullReference or error | P1 |
| NEG-003 | CreateRecordsFromEmails — null SelectedContacts | SelectedContacts=null | ArgumentException: No emails selected | P0 |
| NEG-004 | CreateRecordsFromEmails — empty SelectedContacts | SelectedContacts=[] | ArgumentException: No emails selected | P0 |
| NEG-005 | CreateRecordsFromEmails — null user | CreateRecordsFromEmailsAsync(request, null) | NullReference or error | P1 |
| NEG-006 | Invalid email format | EmailAddress="not-email" | Validation or error | P0 |
| NEG-007 | Empty email | EmailAddress="" | Validation error | P0 |
| NEG-008 | Null email in list | EmailAddresses=[null] | Handled | P1 |
| NEG-009 | Missing contact create permission | User lacks permission | CreateRecordsFromEmailsAsync | UnauthorizedAccessException | P0 |
| NEG-010 | Missing partner create permission | User lacks permission | CreateRecordsFromEmailsAsync | UnauthorizedAccessException | P0 |
| NEG-011 | Partner not created (exception) | ProcessSinglePartnerAsync throws | FailedEmails populated | P1 |
| NEG-012 | Contact not created (exception) | ProcessSingleContactAsync throws | FailedEmails populated | P1 |
| NEG-013 | GetPartnerIdForContact — partner not in state | Partner creation failed | Exception: Partner not created | P0 |
| NEG-014 | ExtractNameFromEmail — empty | "" | (FirstName="", LastName="") | P1 |
| NEG-015 | ExtractNameFromEmail — single char | "x" | (FirstName="X", LastName="") | P1 |
| NEG-016 | GetPartnerNameFromEmail — no @ | Malformed email | IndexOutOfRange or handled | P1 |
| NEG-017 | CreateContactRequest — null selectedEmail | null | ArgumentNullException | P0 |
| NEG-018 | ValidateCreateRecordsRequest — no permission | User lacks both | UnauthorizedAccessException | P0 |
| NEG-019 | GmailThreadId empty, GmailMessageId empty | Both null/empty | UpdateExistingInteractionAsync skipped | P1 |
| NEG-020 | FindGmailInteractionAsync — not found | No interaction for thread | Update skipped | P1 |

### 2.2 Unauthorized Access (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-021 | No auth token | FindRelatedRecordsAsync | 401 | P0 |
| NEG-022 | No auth token | CreateRecordsFromEmailsAsync | 401 | P0 |
| NEG-023 | Expired JWT | Any operation | 401 | P0 |
| NEG-024 | Tampered JWT | Any operation | 401 | P0 |
| NEG-025 | User A creates for User B scope | Org scope violation | 403 or filtered | P0 |
| NEG-026 | Disabled account | Any operation | 401/403 | P1 |
| NEG-027 | Post-logout | Any operation | 401 | P1 |
| NEG-028 | No contact create | CreateRecordsFromEmails | UnauthorizedAccessException | P0 |
| NEG-029 | No partner create | CreateRecordsFromEmails | UnauthorizedAccessException | P0 |
| NEG-030 | Permission check contact create | CanPerformActionAsync("Contact","create") | Required | P0 |
| NEG-031 | Permission check partner create | CanPerformActionAsync("Partner","create") | Required | P0 |
| NEG-032 | GetContactsForGmailAddon — no read | Contact Permissions.CanRead=false | CanRead=false in response | P1 |
| NEG-033 | GetPartnersForGmailAddon — no read | Partner Permissions.CanRead=false | CanRead=false in response | P1 |
| NEG-034 | IDOR — other user's contacts | Request other org contacts | 403 or filtered | P0 |
| NEG-035 | IDOR — update other user's interaction | GmailThreadId from other user | 403 or filtered | P0 |

### 2.3 Missing Data & State (20)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-036 | Missing partner | Email new@newcorp.com, partner creation fails | FailedEmails includes email | P0 |
| NEG-037 | Missing contact | Contact not found, create fails | FailedEmails includes email | P1 |
| NEG-038 | Partner by name not found | GetPartnerByNameAsync returns null | New partner created | P0 |
| NEG-039 | Contact by email exists | GetContactByEmailAsync returns contact | Skip creation, add to ExistingContacts | P0 |
| NEG-040 | FindRelatedRecords — empty EmailAddresses | EmailAddresses=[] | Empty response | P1 |
| NEG-041 | FindRelatedRecords — null EmailAddresses | EmailAddresses=null | NullReference or handled | P1 |
| NEG-042 | ProcessUsersAsync — exception | GetUsersByEmailsAsync throws | Users empty, continue | P1 |
| NEG-043 | ProcessUsersAsync — UserProfile null | userProfileLookup empty | MapUserToGmailUser with fallback | P1 |
| NEG-044 | GetUserIdFromClaims — no claim | User has no NameIdentifier | 0, notifications skipped | P1 |
| NEG-045 | UpdateInteractionWithRecordsAsync — null ContactIds | existingInteraction.ContactIds=null | Initialized to new List | P1 |
| NEG-046 | UpdateInteractionWithRecordsAsync — null PartnerIds | existingInteraction.PartnerIds=null | Initialized to new List | P1 |
| NEG-047 | CreateContactRequest — empty FirstName/LastName | Both null | Extract from email prefix | P0 |
| NEG-048 | CreateContactRequest — LastName empty | FirstName only | LastName=FirstName | P1 |
| NEG-049 | CreateContactRequest — all empty | No name | LastName from email prefix | P0 |
| NEG-050 | GetFailedEmailsForPartner — no match | PartnerName not in SelectedContacts | Empty | P1 |
| NEG-051 | GetFailedEmailsForPartner — PartnerId provided | email.PartnerId has value | Excluded (partner exists) | P1 |
| NEG-052 | ProcessPartnersForCreationAsync — exception | ProcessSinglePartnerAsync throws | FailedEmails for that partner's emails | P1 |
| NEG-053 | ProcessContactsForCreationAsync — exception | ProcessSingleContactAsync throws | FailedEmails includes email | P1 |
| NEG-054 | UpdateExistingInteractionAsync — exception | UpdateInteractionAsync throws | Logged, not rethrown | P1 |
| NEG-055 | SendCreationNotificationsAsync — userId 0 | No user ID | Skipped | P1 |

### 2.4 Injection & Sanitization (10)
| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| NEG-056 | SQL injection in email | EmailAddress="'; DROP TABLE--" | Parameterized | P0 |
| NEG-057 | XSS in PartnerName | PartnerName="<script>alert(1)</script>" | Sanitized | P0 |
| NEG-058 | XSS in FirstName | FirstName="<script>" | Sanitized | P0 |
| NEG-059 | XSS in EmailAddress | EmailAddress with script | Sanitized | P0 |
| NEG-060 | LDAP injection in email | Email with LDAP chars | Escaped | P1 |
| NEG-061 | Control chars in email | Email with \0 | Rejected | P1 |
| NEG-062 | Path traversal in PartnerName | PartnerName="../../../etc" | Sanitized | P0 |
| NEG-063 | HTML injection in LastName | LastName="<b>Bold</b>" | Escaped | P1 |
| NEG-064 | Unicode homograph in email | IDN homograph | Handled | P2 |
| NEG-065 | Log injection | Malicious string in email | Sanitized in log | P1 |

### 2.5 Additional (25)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-066 | FindRelatedRecords — exception | ProcessContactsAsync throws | Exception propagated | P0 |
| NEG-067 | CreateRecordsFromEmails — exception | ProcessPartnersForCreationAsync throws | Exception propagated | P0 |
| NEG-068 | GetValidAudienceForCurrentHost — null HttpContext | HttpContext=null | ApplicationException | P1 |
| NEG-069 | MapContactToGmailContact — null contact | contact=null | Handled in loop | P1 |
| NEG-070 | MapPartnerToGmailPartner — null partner | partner=null | Handled in loop | P1 |
| NEG-071 | MapUserToGmailUser — null userProfile | userProfile=null | Fallback to user.Email | P1 |
| NEG-072 | Interactions null in contact | contact.Interactions=null | No Interactions in GmailContact | P1 |
| NEG-073 | Interactions null in partner | partner.Interactions=null | No Interactions in GmailPartner | P1 |
| NEG-074 | Contacts null in partner | partner.Contacts=null | No Contacts in GmailPartner | P1 |
| NEG-075 | Permissions null in contact | contact.Permissions=null | NullReference or default | P1 |
| NEG-076 | Permissions null in interaction | i.Permissions=null | Filtered out | P1 |
| NEG-077 | GmailThreadId null | GmailThreadId=null | UpdateExistingInteraction skipped | P1 |
| NEG-078 | GmailMessageId null | GmailMessageId=null | FindGmailInteraction may use thread only | P1 |
| NEG-079 | Duplicate contact in SelectedContacts | Same email twice | Handled (create once or skip) | P1 |
| NEG-080 | Duplicate partner name | Two emails same domain | One partner created | P1 |
| NEG-081 | ContactManager.GetContactsForGmailAddon null | Returns null | Handled | P1 |
| NEG-082 | PartnerManager.GetPartnersForGmailAddon null | Returns null | Handled | P1 |
| NEG-083 | UserDataManager.GetUsersByEmailsAsync empty | Returns empty | Users empty | P1 |
| NEG-084 | ContactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync | Unmatched emails | Partner suggestions | P1 |
| NEG-085 | InteractionManager.FindGmailInteractionAsync null | No interaction | Update skipped | P1 |
| NEG-086 | InteractionManager.UpdateInteractionAsync failure | Update fails | Logged, not rethrown | P1 |
| NEG-087 | NotificationManager.CreateNotification failure | Notification fails | Logged, not rethrown | P1 |
| NEG-088 | Partner creation — PartnerManager throws | CreatePartnerAsync throws | FailedEmails populated | P1 |
| NEG-089 | Contact creation — ContactManager throws | CreateContactAsync throws | FailedEmails populated | P1 |
| NEG-090 | BuildResultMessage — all failed | state.FailedEmails only | Message includes failed count | P1 |

---

## §3 Boundary Tests — 90 tests

### String Lengths (15)
| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | EmailAddress | 5 | 320 | ✅ "a@b.c" | ✅ 320 chars | ❌ 321 | P1 |
| BND-002 | PartnerName | 0 | 200 | ✅ null | ✅ 200 chars | ❌ 201 | P2 |
| BND-003 | FirstName | 0 | 100 | ✅ null | ✅ 100 chars | ❌ 101 | P2 |
| BND-004 | LastName | 0 | 100 | ✅ null | ✅ 100 chars | ❌ 101 | P2 |
| BND-005 | GmailThreadId | 0 | 500 | ✅ null | ✅ 500 chars | — | P2 |
| BND-006 | GmailMessageId | 0 | 500 | ✅ null | ✅ 500 chars | — | P2 |
| BND-007 | Email prefix 1 char | "x" | — | ExtractNameFromEmail | — | — | P1 |
| BND-008 | Email prefix 2 parts | "john.doe" | — | FirstName, LastName | — | — | P1 |
| BND-009 | Email prefix 3+ parts | "john.middle.doe" | — | First 2 parts | — | — | P1 |
| BND-010 | Domain 1 char | "x@a.co" | — | GetPartnerNameFromEmail | "A" | — | P1 |
| BND-011 | Domain long | 200 chars | — | — | — | — | P2 |
| BND-012 | PartnerName provided | — | — | Used over domain | — | — | P0 |
| BND-013 | Notification message length | — | — | Combined message | — | — | P2 |
| BND-014 | ExtractNameFromEmail delimiter . | "a.b" | — | FirstName=a, LastName=b | — | — | P1 |
| BND-015 | ExtractNameFromEmail delimiter _ | "a_b" | — | FirstName=a, LastName=b | — | — | P1 |

### Numeric (15)
| ID | Field | Min | Max | Zero | Negative | Priority |
|----|-------|-----|-----|------|----------|----------|
| BND-016 | PartnerId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-017 | Contact Id | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-018 | User Id | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-019 | PartnerId=0 | — | — | Treated as null | — | P1 |
| BND-020 | PartnerId negative | — | — | Invalid | — | P1 |
| BND-021 | EmailAddresses count 0 | — | — | Empty response | — | P1 |
| BND-022 | EmailAddresses count 1 | — | — | Single match | — | P1 |
| BND-023 | EmailAddresses count 100 | — | — | All processed | — | P1 |
| BND-024 | SelectedContacts count 0 | — | — | ArgumentException | — | P0 |
| BND-025 | SelectedContacts count 1 | — | — | Single create | — | P0 |
| BND-026 | SelectedContacts count 50 | — | — | Batch processed | — | P1 |
| BND-027 | CreatedContacts count | — | — | Correct | — | P1 |
| BND-028 | FailedEmails count | — | — | Correct | — | P1 |
| BND-029 | NewPartnersCreated count | — | — | Correct | — | P1 |
| BND-030 | partnerIds count | — | — | From contacts | — | P1 |

### Collections (20)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-031 | 0 contacts found | FindRelatedRecords | Contacts=[], UnmatchedEmails | P1 |
| BND-032 | 0 partners found | FindRelatedRecords | Partners=[] | P1 |
| BND-033 | 0 users found | FindRelatedRecords | Users=[] | P1 |
| BND-034 | All emails matched | FindRelatedRecords | UnmatchedEmails=[] | P1 |
| BND-035 | No emails matched | FindRelatedRecords | All in UnmatchedEmails | P1 |
| BND-036 | 1 contact, 1 partner | FindRelatedRecords | Both in response | P0 |
| BND-037 | 5 contacts, 3 partners | FindRelatedRecords | Correct mapping | P1 |
| BND-038 | contactPartnerIds from contacts | ProcessContactsAsync | Partner IDs collected | P0 |
| BND-039 | partnerIds passed to ProcessPartners | input.partnerIds | Partners filtered | P0 |
| BND-040 | UnmatchedEmails case-insensitive remove | Matched contact | Original case removed | P1 |
| BND-041 | GetPartnersToCreate — GroupBy PartnerName | Same domain | One per unique name | P1 |
| BND-042 | GetPartnersToCreate — PartnerId provided | email.PartnerId has value | Excluded from create | P0 |
| BND-043 | state.CreatedPartners dictionary | Partner name → Id | Lookup for contact | P0 |
| BND-044 | state.FailedEmails distinct | Duplicate emails | Distinct in result | P1 |
| BND-045 | state.ExistingContacts | Skip create | Added to state | P0 |
| BND-046 | UpdateInteraction ContactIds add | New contact | needsUpdate=true | P0 |
| BND-047 | UpdateInteraction PartnerIds add | New partner | needsUpdate=true | P0 |
| BND-048 | UpdateInteraction no duplicate | Contact already in list | needsUpdate=false | P1 |
| BND-049 | userProfileLookup GroupBy email | Duplicate emails | First taken | P1 |
| BND-050 | MapInteractionsToGmailInteractions filter | Permissions.CanRead=false | Excluded | P1 |

### Unicode & Special (15)
| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-051 | Email (IDN) | user@münchen.de | Handled | P2 |
| BND-052 | PartnerName (Arabic) | `شركة` | Stored | P2 |
| BND-053 | FirstName (Chinese) | `明` | Stored | P2 |
| BND-054 | LastName (Cyrillic) | `Иванов` | Stored | P2 |
| BND-055 | Email prefix (French) | "jean-françois" | ExtractNameFromEmail | P2 |
| BND-056 | Email prefix (underscore) | "john_doe" | FirstName=John, LastName=Doe | P1 |
| BND-057 | Email prefix (dash) | "john-doe" | FirstName=John, LastName=Doe | P1 |
| BND-058 | Email prefix (multiple delimiters) | "j.o.h.n" | Parts split | P1 |
| BND-059 | Domain (subdomain) | user@mail.acme.com | GetPartnerNameFromEmail | "Mail" or "Acme" per logic | P1 |
| BND-060 | PartnerName with apostrophe | "O'Brien" | Stored | P1 |
| BND-061 | PartnerName with ampersand | "Smith & Co" | Stored | P1 |
| BND-062 | Email plus addressing | user+tag@acme.com | Handled | P2 |
| BND-063 | Name with RTL | Arabic FirstName | Stored | P2 |
| BND-064 | ExtractNameFromEmail empty parts | ".." | Split RemoveEmptyEntries | P1 |
| BND-065 | GetPartnerNameFromEmail domain uppercase | ACME.COM | Capitalized | P1 |

### Interaction & Thread (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-066 | GmailThreadId provided | UpdateExistingInteraction | FindGmailInteraction called | P0 |
| BND-067 | GmailMessageId provided | UpdateExistingInteraction | FindGmailInteraction called | P0 |
| BND-068 | Both provided | UpdateExistingInteraction | Both in request | P0 |
| BND-069 | Interaction found | UpdateInteractionWithRecordsAsync | Interaction updated | P0 |
| BND-070 | Interaction not found | FindGmailInteraction null | Update skipped | P1 |
| BND-071 | needsUpdate true | New contacts/partners | UpdateInteractionAsync called | P0 |
| BND-072 | needsUpdate false | No new | UpdateInteractionAsync not called | P1 |
| BND-073 | Duplicate interaction prevention | Same threadId | One interaction | P0 |
| BND-074 | Interaction update when thread continues | New email in thread | Interaction updated with new contacts | P0 |
| BND-075 | GmailInteractionRequest GmailThreadId | Empty string | Handled | P1 |
| BND-076 | GmailInteractionRequest GmailMessageId | Empty string | Handled | P1 |
| BND-077 | UpdateInteractionRequest from existing | All fields mapped | Correct update | P1 |
| BND-078 | ContactIds initialized | null | New List | P1 |
| BND-079 | PartnerIds initialized | null | New List | P1 |
| BND-080 | Users in UpdateInteractionRequest | UserIds from interaction | Preserved | P1 |

### Additional (10)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-081 | Notification combined | Contacts + Partners created | Single notification | P1 |
| BND-082 | Notification contacts only | Contacts only | Contact message | P1 |
| BND-083 | Notification partners only | Partners only | Partner message | P1 |
| BND-084 | Notification metadata | ContactNames, PartnerNames | In payload | P1 |
| BND-085 | GetUserIdFromClaims | Valid claim | User ID | P0 |
| BND-086 | GetUserIdFromClaims | No claim | 0 | P1 |
| BND-087 | OrgUnit from UserProfile | userProfile.OrgUnit | In GmailRelatedUser | P1 |
| BND-088 | OrgUnit fallback | userProfile=null | "Unknown" | P1 |
| BND-089 | GmailRelatedUser Name | userProfile.Name ?? user.Email ?? user.Id | Fallback chain | P1 |
| BND-090 | Success in BuildCreateRecordsResult | CreatedContacts or ExistingContacts | Success=true | P0 |

---

## §4 Functional Tests — 90 tests

### 4.1 Workflow (15)
| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | FindRelatedRecords process order | ProcessContacts→ProcessPartners→ProcessUsers→ProcessUnmatched | Correct sequence | P0 |
| FUN-002 | Contact match removes from unmatched | ProcessContactsAsync | unmatchedEmailStrings updated | P0 |
| FUN-003 | User match removes from unmatched | ProcessUsersAsync | unmatchedEmailStrings updated | P0 |
| FUN-004 | partnerIds from contacts | ProcessContactsAsync | contactPartnerIds to input.partnerIds | P0 |
| FUN-005 | CreateRecords process order | Validate→Partners→Contacts→UpdateInteraction→Notifications | Correct sequence | P0 |
| FUN-006 | Partner before contact | ProcessPartnersForCreationAsync first | Contact gets PartnerId | P0 |
| FUN-007 | Skip failed emails in contact | state.FailedEmails | ProcessSingleContactAsync skipped | P0 |
| FUN-008 | GetPartnerIdForContact — PartnerId | selectedEmail.PartnerId | Used | P0 |
| FUN-009 | GetPartnerIdForContact — state | state.CreatedPartners | Lookup by partner name | P0 |
| FUN-010 | CreateContactRequest — name from email | No FirstName/LastName | ExtractNameFromEmail | P0 |
| FUN-011 | CreateContactRequest — LastName required | Empty LastName | Fallback to FirstName or email prefix | P0 |
| FUN-012 | GetPartnerNameFromEmail — PartnerName | Provided | Used | P0 |
| FUN-013 | GetPartnerNameFromEmail — domain | No PartnerName | Domain capitalized | P0 |
| FUN-014 | UpdateExistingInteraction — thread/message | Both provided | FindGmailInteraction | P0 |
| FUN-015 | SendCreationNotifications — userId | userId>0 | Notification sent | P0 |

### 4.2 Validation (15)
| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | SelectedContacts required | Non-empty list | null, [] | P0 |
| FUN-017 | Contact create permission | CanPerformActionAsync true | false | P0 |
| FUN-018 | Partner create permission | CanPerformActionAsync true | false | P0 |
| FUN-019 | Email format | user@domain.com | "not-email" | P0 |
| FUN-020 | PartnerId optional | null or valid | Invalid (0, -1) | P1 |
| FUN-021 | PartnerName optional | null or string | — | P1 |
| FUN-022 | FirstName optional | null or string | — | P1 |
| FUN-023 | LastName optional | null or string | — | P1 |
| FUN-024 | GmailThreadId optional | null or string | — | P1 |
| FUN-025 | GmailMessageId optional | null or string | — | P1 |
| FUN-026 | ExtractNameFromEmail — 2+ parts | "john.doe" | FirstName, LastName | P0 |
| FUN-027 | ExtractNameFromEmail — 1 part | "john" | FirstName, LastName="" | P1 |
| FUN-028 | ExtractNameFromEmail — capitalization | "john" | "John" | P0 |
| FUN-029 | GetPartnerNameFromEmail — domain | "user@acme.com" | "Acme" | P0 |
| FUN-030 | GetPartnerNameFromEmail — subdomain | "user@mail.acme.com" | "Mail" (first part) | P1 |

### 4.3 Constraints (10)
| ID | Constraint | Expected | Priority |
|----|-----------|----------|----------|
| FUN-031 | One partner per domain (create) | Same domain emails | One partner created | P0 |
| FUN-032 | Contact unique by email | GetContactByEmailAsync | Skip if exists | P0 |
| FUN-033 | Partner unique by name | GetPartnerByNameAsync | Use existing | P0 |
| FUN-034 | Interaction unique by thread | FindGmailInteraction | One per thread | P0 |
| FUN-035 | FailedEmails distinct | BuildCreateRecordsResult | Distinct | P1 |
| FUN-036 | GetPartnersToCreate — no PartnerId | Exclude emails with PartnerId | P0 |
| FUN-037 | GetFailedEmailsForPartner — match | PartnerName match | Emails for that partner | P1 |
| FUN-038 | UpdateInteraction — no duplicate IDs | Contains check | Add only if not present | P0 |
| FUN-039 | userProfileLookup — case-insensitive | StringComparer.OrdinalIgnoreCase | P1 |
| FUN-040 | unmatchedEmailStrings — copy | New List | Original not mutated | P1 |

### 4.4 Audit (10)
| ID | Action | Expected | Priority |
|----|--------|----------|----------|
| FUN-041 | Create contact | ContactManager.CreateContactAsync | Audit fields set | P1 |
| FUN-042 | Create partner | PartnerManager.CreatePartnerAsync | Audit fields set | P1 |
| FUN-043 | Update interaction | InteractionManager.UpdateInteractionAsync | Audit fields set | P1 |
| FUN-044 | Create notification | NotificationManager.CreateNotification | Logged | P1 |
| FUN-045 | FindRelatedRecords — no audit | Read-only | No audit | P1 |
| FUN-046 | Logging on exception | ProcessPartnersForCreationAsync | _logger.LogError | P1 |
| FUN-047 | Logging on success | ProcessSinglePartnerAsync | _logger.LogInformation | P1 |
| FUN-048 | Logging on skip | ProcessSingleContactAsync existing | _logger.LogInformation | P1 |
| FUN-049 | Logging on update interaction | UpdateInteractionWithRecordsAsync | _logger.LogInformation | P1 |
| FUN-050 | Logging on notification skip | userId=0 | _logger.LogWarning | P1 |

### 4.5 Extended Functional (40)
| ID | Rule | Expected | Priority |
|----|------|----------|----------|
| FUN-051 | InitializeResponsePermissions | CanCreateContacts, CanCreatePartners, CanCreateInteractions | P0 |
| FUN-052 | MapContactToGmailContact — CanRead false | contact.Permissions.CanRead=false | CanRead=false, minimal data | P0 |
| FUN-053 | MapPartnerToGmailPartner — CanRead false | partner.Permissions.CanRead=false | CanRead=false, Name only | P0 |
| FUN-054 | MapUserToGmailUser — userProfile | OrgUnit, Name from profile | P1 |
| FUN-055 | MapInteractionsToGmailInteractions — filter | Permissions.CanRead | Only readable | P1 |
| FUN-056 | MapContactsToGmailContacts — filter | Permissions.CanRead | Only readable | P1 |
| FUN-057 | ProcessContactsAsync — contactPartnerIds | From contact.Partner | P0 |
| FUN-058 | ProcessContactsAsync — unmatched remove | Case-insensitive match | P0 |
| FUN-059 | ProcessPartnersAsync — input.partnerIds | Filter partners | P0 |
| FUN-060 | ProcessUsersAsync — bulk lookup | GetUsersByEmailsAsync | P0 |
| FUN-061 | ProcessUsersAsync — UserProfile lookup | GetUserInfosByEmailsAsync | P1 |
| FUN-062 | ProcessUnmatchedEmailsAsync | GetUnmatchedEmailsWithPartnerSuggestionsAsync | P1 |
| FUN-063 | GetPartnersToCreate — GroupBy case-insensitive | StringComparer.OrdinalIgnoreCase | P1 |
| FUN-064 | ProcessSinglePartnerAsync — existing | GetPartnerByNameAsync | state.CreatedPartners | P0 |
| FUN-065 | ProcessSinglePartnerAsync — create | CreatePartnerAsync | state.NewPartnersCreated++ | P0 |
| FUN-066 | ProcessSingleContactAsync — existing | GetContactByEmailAsync | state.ExistingContacts | P0 |
| FUN-067 | ProcessSingleContactAsync — create | CreateContactAsync | state.CreatedContacts | P0 |
| FUN-068 | CreateContactRequest — Salutation | "" default | P1 |
| FUN-069 | CreateContactRequest — Title | "" default | P1 |
| FUN-070 | CreateContactRequest — Status | EntityStatus.Active | P1 |
| FUN-071 | CreatePartnerRequest — Status | Draft | P1 |
| FUN-072 | CreatePartnerRequest — CanCreateNewOpportunities | false | P1 |
| FUN-073 | CreatePartnerRequest — PooledFund | false | P1 |
| FUN-074 | BuildCreateRecordsResult — Success | CreatedContacts or ExistingContacts any | P0 |
| FUN-075 | BuildResultMessage — stats | Created, existing, failed, partners | P0 |
| FUN-076 | UpdateInteractionWithRecordsAsync — ContactIds | Add new | P0 |
| FUN-077 | UpdateInteractionWithRecordsAsync — PartnerIds | Add new | P0 |
| FUN-078 | UpdateInteractionWithRecordsAsync — needsUpdate | Any add | P0 |
| FUN-079 | SendCreationNotificationsAsync — combined | Both contacts and partners | Single notification | P1 |
| FUN-080 | SendCreationNotificationsAsync — contacts only | Contacts only | Contact notification | P1 |
| FUN-081 | SendCreationNotificationsAsync — partners only | Partners only | Partner notification | P1 |
| FUN-082 | Notification type | "gmail_records_creation" | P1 |
| FUN-083 | Notification category | "GmailCreation" | P1 |
| FUN-084 | Organization unit association | UserProfile.OrgUnit | In GmailRelatedUser | P0 |
| FUN-085 | Name extraction delimiter . | Split | P0 |
| FUN-086 | Name extraction delimiter _ | Split | P0 |
| FUN-087 | Name extraction delimiter - | Split | P0 |
| FUN-088 | Partner fuzzy matching | Domain similarity (future) | P2 |
| FUN-089 | Duplicate interaction prevention | threadId | One interaction | P0 |
| FUN-090 | Error handling missing partner | Partner creation fails | FailedEmails | P0 |

---

## §5 Integration Tests — 90 tests

### 5.1 CRUD (10)
| ID | Operation | Entities | Expected | Priority |
|----|----------|----------|----------|----------|
| INT-001 | FindRelatedRecords full flow | Contact, Partner, User | All in response | P0 |
| INT-002 | CreateRecordsFromEmails full flow | Partner, Contact | Both created | P0 |
| INT-003 | Create partner → create contact | Partner created first | Contact linked | P0 |
| INT-004 | Create contact → update interaction | GmailThreadId provided | Interaction updated | P0 |
| INT-005 | Create → notification | Contact/Partner created | Notification sent | P1 |
| INT-006 | FindRelatedRecords → CreateRecords | Find then Create | Flow works | P0 |
| INT-007 | Existing contact → skip | Contact exists | No duplicate | P0 |
| INT-008 | Existing partner → use | Partner exists | Contact linked | P0 |
| INT-009 | Batch create 5 contacts | 5 emails | 5 created | P1 |
| INT-010 | Batch create 3 partners | 3 domains | 3 created | P1 |

### 5.2 Search & Filter (10)
| ID | Criteria | Expected | Priority |
|----|----------|----------|----------|
| INT-011 | FindRelatedRecords by email | Email list | Matching contacts, partners, users | P0 |
| INT-012 | GetContactsForGmailAddon | ContactManager | Contact list | P0 |
| INT-013 | GetPartnersForGmailAddon | PartnerManager | Partner list | P0 |
| INT-014 | GetUsersByEmailsAsync | UserDataManager | User list | P0 |
| INT-015 | GetUnmatchedEmailsWithPartnerSuggestions | Unmatched | Partner suggestions | P0 |
| INT-016 | GetContactByEmailAsync | Contact exists | Contact | P0 |
| INT-017 | GetPartnerByNameAsync | Partner exists | Partner | P0 |
| INT-018 | FindGmailInteractionAsync | Interaction exists | Interaction | P0 |
| INT-019 | Case-insensitive match | JOHN@acme.com | Matched | P1 |
| INT-020 | partnerIds filter | From contacts | Partners filtered | P0 |

### 5.3 Pagination (5)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-021 | Large EmailAddresses list | 100 emails | All processed | P1 |
| INT-022 | Large SelectedContacts list | 50 contacts | All processed | P1 |
| INT-023 | Empty results | No matches | Empty lists | P1 |
| INT-024 | Single result | 1 match | 1 in list | P1 |
| INT-025 | Mixed results | Some match, some not | Correct split | P1 |

### 5.4 Relationships (10)
| ID | Relationship | Expected | Priority |
|----|-------------|----------|----------|
| INT-026 | Contact → Partner | contact.Partner | In GmailRelatedContact | P0 |
| INT-027 | Partner → Contacts | partner.Contacts | In GmailRelatedPartner | P0 |
| INT-028 | Partner → Interactions | partner.Interactions | In GmailRelatedPartner | P1 |
| INT-029 | Contact → Interactions | contact.Interactions | In GmailRelatedContact | P1 |
| INT-030 | User → UserProfile | OrgUnit, Name | In GmailRelatedUser | P1 |
| INT-031 | Interaction → Contacts | ContactIds | Updated | P0 |
| INT-032 | Interaction → Partners | PartnerIds | Updated | P0 |
| INT-033 | Create contact → PartnerId | ContactRequest.PartnerId | Set | P0 |
| INT-034 | Create partner → Contact | Contact linked | P0 |
| INT-035 | Notification → User | userId | Recipient | P1 |

### 5.5 Error Handling (15)
| ID | Error | Expected | Priority |
|----|------|----------|----------|
| INT-036 | Invalid data → 400 | ArgumentException | P0 |
| INT-037 | Unauthorized → 403 | UnauthorizedAccessException | P0 |
| INT-038 | Null input → 400 | ArgumentNullException | P0 |
| INT-039 | Empty SelectedContacts → 400 | ArgumentException | P0 |
| INT-040 | Permission denied → 403 | UnauthorizedAccessException | P0 |
| INT-041 | Partner creation fails | FailedEmails | P1 |
| INT-042 | Contact creation fails | FailedEmails | P1 |
| INT-043 | ProcessUsersAsync exception | Logged, continue | P1 |
| INT-044 | UpdateInteraction exception | Logged, not rethrown | P1 |
| INT-045 | Notification exception | Logged, not rethrown | P1 |
| INT-046 | GetValidAudienceForCurrentHost exception | ApplicationException | P1 |
| INT-047 | ContactManager exception | Propagated | P1 |
| INT-048 | PartnerManager exception | Propagated | P1 |
| INT-049 | InteractionManager exception | Logged | P1 |
| INT-050 | UserDataManager exception | ProcessUsersAsync catch | P1 |

### 5.6 Extended Integration (40)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-051 | API POST FindRelatedRecords | 200 with response | P0 |
| INT-052 | API POST CreateRecordsFromEmails | 200 with result | P0 |
| INT-053 | GmailAddonController → UNOPSGmailAddonManager | Correct resolution | P1 |
| INT-054 | UNOPSGmailAddonManager → ContactManager | GetContactsForGmailAddon | P0 |
| INT-055 | UNOPSGmailAddonManager → PartnerManager | GetPartnersForGmailAddon, CreatePartnerAsync | P0 |
| INT-056 | UNOPSGmailAddonManager → UserDataManager | GetUsersByEmailsAsync | P0 |
| INT-057 | UNOPSGmailAddonManager → InteractionManager | FindGmailInteractionAsync, UpdateInteractionAsync | P0 |
| INT-058 | UNOPSGmailAddonManager → NotificationManager | CreateNotification | P0 |
| INT-059 | UNOPSGmailAddonManager → PermissionService | CanPerformActionAsync | P0 |
| INT-060 | UNOPSGmailAddonManager → UserInfoService | GetUserInfosByEmailsAsync | P1 |
| INT-061 | ContactManager.GetContactsForGmailAddon | Input emails | Contact list | P0 |
| INT-062 | ContactManager.GetUnmatchedEmailsWithPartnerSuggestions | Unmatched | Suggestions | P0 |
| INT-063 | PartnerManager.GetPartnersForGmailAddon | input.partnerIds | Partner list | P0 |
| INT-064 | PartnerManager.GetPartnerByNameAsync | Name | Partner or null | P0 |
| INT-065 | PartnerManager.CreatePartnerAsync | Request | Partner created | P0 |
| INT-066 | ContactManager.GetContactByEmailAsync | Email | Contact or null | P0 |
| INT-067 | ContactManager.CreateContactAsync | Request | Contact created | P0 |
| INT-068 | InteractionManager.FindGmailInteractionAsync | GmailInteractionRequest | Interaction or null | P0 |
| INT-069 | InteractionManager.UpdateInteractionAsync | UpdateInteractionRequest | Updated | P0 |
| INT-070 | UserDataManager.GetUsersByEmailsAsync | Emails | User list | P0 |
| INT-071 | UserInfoService.GetUserInfosByEmailsAsync | Emails | UserProfile list | P1 |
| INT-072 | PermissionService.CanPerformActionAsync | Entity, action | bool | P0 |
| INT-073 | NotificationManager.CreateNotification | userId, message, etc. | Notification created | P0 |
| INT-074 | GmailRelatedRecordsResponse structure | Contacts, Partners, Users, UnmatchedEmails | P0 |
| INT-075 | GmailCreateRecordsResult structure | CreatedContacts, CreatedPartners, FailedEmails, Success, Message | P0 |
| INT-076 | GmailRelatedRecordsRequest structure | EmailAddresses, partnerIds | P0 |
| INT-077 | GmailCreateRecordsRequest structure | SelectedContacts, GmailThreadId, GmailMessageId | P0 |
| INT-078 | GmailSelectedEmailModel structure | EmailAddress, PartnerName, PartnerId, FirstName, etc. | P0 |
| INT-079 | GmailRelatedContact structure | EmailAddress, Name, PartnerName, Id, CanRead | P0 |
| INT-080 | GmailRelatedPartner structure | Id, Name, Contacts, Interactions, CanRead | P0 |
| INT-081 | GmailRelatedUser structure | Id, Name, Email, OrgUnit, CanRead | P0 |
| INT-082 | GmailRelatedInteraction structure | Id, Type, Description, Date, CanRead | P1 |
| INT-083 | ManagerWrapper IGmailAddonManager | UNOPSGmailAddonManager | P1 |
| INT-084 | DI — all dependencies | Constructor | Resolved | P1 |
| INT-085 | Logger injection | _logger | Logging works | P1 |
| INT-086 | Configuration injection | _configuration | Available | P1 |
| INT-087 | HttpContextAccessor | GetValidAudienceForCurrentHost | Host URL | P1 |
| INT-088 | BaseUNOPSManager | Inherits | Base behavior | P1 |
| INT-089 | End-to-end FindRelatedRecords | Request → Response | P0 |
| INT-090 | End-to-end CreateRecordsFromEmails | Request → Result | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE)

Security tests are covered in a separate Security test suite. Categories: Injection (10), Access Control (10), IDOR (10), Mass Assignment (5), Auth & Session (10), Data Exposure (5).

---

## §7 Concurrency Tests — 25 tests

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two users FindRelatedRecords same emails | Both succeed | P1 |
| CON-002 | Two users CreateRecords same email | One creates, one gets existing | P1 |
| CON-003 | CreateRecords during FindRelatedRecords | No conflict | P1 |
| CON-004 | Concurrent CreateRecords different emails | Both succeed | P1 |
| CON-005 | Concurrent partner create same name | One creates, one gets existing | P0 |
| CON-006 | Concurrent contact create same email | One creates, one skips (existing) | P0 |
| CON-007 | Update interaction during CreateRecords | Consistent | P1 |
| CON-008 | Partner creation + contact creation same domain | Partner created once | P0 |
| CON-009 | DB deadlock | Resolved | P1 |
| CON-010 | Token refresh during CreateRecords | Retry | P1 |
| CON-011 | Bulk CreateRecords concurrent | All complete | P2 |
| CON-012 | FindRelatedRecords concurrent | No interference | P1 |
| CON-013 | ProcessUsersAsync concurrent | No interference | P1 |
| CON-014 | Notification concurrent | Both sent | P1 |
| CON-015 | GetContactByEmailAsync race | First create wins | P1 |
| CON-016 | GetPartnerByNameAsync race | First create wins | P1 |
| CON-017 | UpdateInteraction concurrent | Last write wins | P1 |
| CON-018 | state.CreatedPartners concurrent | Thread-safe | P1 |
| CON-019 | state.FailedEmails concurrent | Thread-safe | P1 |
| CON-020 | Session timeout during CreateRecords | Rolled back | P1 |
| CON-021 | Multiple users creating same partner | One succeeds | P1 |
| CON-022 | Multiple users creating same contact | One succeeds | P1 |
| CON-023 | FindGmailInteraction during Update | Consistent | P1 |
| CON-024 | UserProfile lookup concurrent | No interference | P2 |
| CON-025 | Real-time update propagation | Eventually consistent | P2 |

---

## §8 Unit Tests — 21 tests

| ID | Category | Input | Expected | Priority |
|----|----------|-------|----------|----------|
| UNT-001 | Validation | Null SelectedContacts | ArgumentException | P0 |
| UNT-002 | Validation | Empty SelectedContacts | ArgumentException | P0 |
| UNT-003 | Validation | Invalid email | Error | P0 |
| UNT-004 | Validation | No contact permission | UnauthorizedAccessException | P0 |
| UNT-005 | Validation | No partner permission | UnauthorizedAccessException | P0 |
| UNT-006 | Formatting | ExtractNameFromEmail "john.doe" | FirstName=John, LastName=Doe | P0 |
| UNT-007 | Formatting | ExtractNameFromEmail "johndoe" | FirstName=Johndoe, LastName="" | P1 |
| UNT-008 | Formatting | GetPartnerNameFromEmail domain | "Acme" from acme.com | P0 |
| UNT-009 | Calculation | GetPartnersToCreate count | Unique by PartnerName | P0 |
| UNT-010 | Calculation | GetFailedEmailsForPartner | Matching emails | P1 |
| UNT-011 | Calculation | BuildResultMessage | Correct stats | P1 |
| UNT-012 | Calculation | BuildCreateRecordsResult Success | true when any created/existing | P0 |
| UNT-013 | Calculation | needsUpdate | true when any add | P0 |
| UNT-014 | Status | Existing contact | Skip create | P0 |
| UNT-015 | Status | Existing partner | Use existing | P0 |
| UNT-016 | Status | Partner creation failed | FailedEmails | P0 |
| UNT-017 | Status | Contact creation failed | FailedEmails | P0 |
| UNT-018 | Status | Interaction not found | Update skipped | P1 |
| UNT-019 | Collections | MapContactToGmailContact | All fields | P1 |
| UNT-020 | Collections | MapPartnerToGmailPartner | All fields | P1 |
| UNT-021 | Collections | MapUserToGmailUser | All fields | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Operation | Threshold | Priority |
|----|----------|----------|----------|
| PRF-001 | FindRelatedRecords 10 emails | < 1s | P1 |
| PRF-002 | FindRelatedRecords 50 emails | < 3s | P1 |
| PRF-003 | CreateRecordsFromEmails 5 contacts | < 2s | P1 |
| PRF-004 | CreateRecordsFromEmails 10 contacts | < 4s | P1 |
| PRF-005 | ProcessContactsAsync | < 500ms | P1 |
| PRF-006 | ProcessPartnersAsync | < 500ms | P1 |
| PRF-007 | ProcessUsersAsync | < 500ms | P1 |
| PRF-008 | ProcessUnmatchedEmailsAsync | < 500ms | P1 |
| PRF-009 | GetContactsForGmailAddon | < 300ms | P1 |
| PRF-010 | GetPartnersForGmailAddon | < 300ms | P1 |
| PRF-011 | 10 concurrent FindRelatedRecords | < 2s each | P2 |
| PRF-012 | 5 concurrent CreateRecordsFromEmails | < 5s each | P2 |
| PRF-013 | UpdateExistingInteraction | < 500ms | P1 |
| PRF-014 | Memory 100 emails | < 50MB | P2 |
| PRF-015 | Memory 500 emails | < 200MB | P2 |
| PRF-016 | Memory leak check | No growth > 10% | P1 |

---

## §10 Load Tests — 10 tests

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 30 concurrent FindRelatedRecords | 15 min | 95% < 1s | P2 |
| LDT-002 | 20 concurrent CreateRecordsFromEmails | 15 min | 95% < 3s | P2 |
| LDT-003 | 50 concurrent FindRelatedRecords | 10 min | < 2s | P2 |
| LDT-004 | Spike 5→50 req/s | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike + CreateRecords | 5 min | All correct | P2 |
| LDT-006 | 100 concurrent FindRelatedRecords | 10 min | Graceful degradation | P2 |
| LDT-007 | Continuous FindRelatedRecords | 10 min | Stable | P2 |
| LDT-008 | Continuous CreateRecordsFromEmails | 10 min | Stable | P2 |
| LDT-009 | Recovery after service restart | N/A | < 30s | P2 |
| LDT-010 | Recovery after DB restart | N/A | < 60s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|------------|
| Email sync / FindRelatedRecords | POS-001–004, FUN-001–004, INT-001, INT-011 |
| Partner matching | POS-002, POS-008–010, FUN-012–013, NEG-038 |
| Contact matching | POS-001, POS-005–007, FUN-066–067 |
| Selected contact handling | POS-022–023, FUN-008–009, BND-042 |
| Name extraction | POS-011–012, FUN-010, FUN-026–028, UNT-006–007 |
| Partner name from domain | POS-009–010, FUN-012–013, BND-010 |
| Interaction update | POS-013, FUN-014, BND-066–072, INT-004 |
| Gmail thread tracking | BND-066–068, FUN-073, NEG-077–078 |
| Duplicate prevention | FUN-031–034, BND-073, CON-005–006 |
| Error handling | NEG-036–039, FUN-090, INT-041–042 |
| Notification triggers | POS-014–015, FUN-041–044, BND-081–084 |
| Organization unit | POS-019, BND-087–088, FUN-084 |
| CreateRecordsFromEmails flow | POS-005–008, FUN-005–007, INT-002–010 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
