# ContactManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ContactManager`  
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

**ContactManager** manages contact CRUD operations, email validation, partner linking, profile pictures, and Gmail integration. Key responsibilities: contact lifecycle management, partner-contact associations, document management, interaction tracking, profile picture upload, Gmail Add-on integration, specification-pattern queries, and pagination.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create contact with valid data | User has permissions, partner exists | CreateContactAsync with name, email, partner | Contact created with ID | P0 |
| POS-002 | Create contact without partner | User has permissions | CreateContactAsync with PartnerId=null | Contact created | P1 |
| POS-003 | Create contact with all optional fields | None | Full ContactRequest | All fields persisted | P1 |
| POS-004 | Get contact by ID — exists | Contact 789 exists | GetContact(userId, 789) | ContactModel returned | P0 |
| POS-005 | Get contact with documents | Contact has 5 documents | GetContactAsync(789) | Documents collection loaded | P1 |
| POS-006 | Get contact with interactions | Contact has 10 interactions | GetContactWithInteractionsAsync(789) | Interactions loaded | P1 |
| POS-007 | Update contact basic fields | Contact exists | UpdateContactAsync with new name, email | Changes persisted | P0 |
| POS-008 | Update contact — change partner | Contact linked to partner 123 | Update to partner 456 | PartnerId updated | P1 |
| POS-009 | Update contact — remove partner | Contact linked to partner | Set PartnerId=null | Association removed | P1 |
| POS-010 | Soft delete contact | Contact exists | DeleteContactAsync(userId, 789) | IsDeleted=true | P0 |
| POS-011 | Get partner contacts — list all | Partner 123 has 15 contacts | GetPartnerContacts(123) | 15 contacts returned | P1 |
| POS-012 | Get partner contacts — empty | Partner has no contacts | GetPartnerContacts(999) | Empty list | P1 |
| POS-013 | Get posted contacts | 50 posted contacts exist | GetPostedContacts() | Active contacts only | P1 |
| POS-014 | Get posted contact by ID | Contact has eligible entities | GetPostedContact(789) | EligibleEntities loaded | P1 |
| POS-015 | Update profile picture | Contact exists, valid JPG | UpdateContactProfilePictureAsync(789, file) | Image saved | P1 |
| POS-016 | Get contacts — specification | Multiple contacts | GetContactsWithSpecificationAsync | Filtered results | P1 |
| POS-017 | Get contacts — pagination | 100 contacts | GetContacts with PageSize=20 | 20 returned, TotalCount=100 | P0 |
| POS-018 | Get contact by email — exists | Contact with john@example.com | GetContactByEmailAsync("john@example.com") | Contact returned | P1 |
| POS-019 | Create contact — duplicate email different partner | john@example.com for partner 123 | Create for partner 456 | Contact created | P1 |
| POS-020 | Get unmatched emails with suggestions | Email list provided | GetUnmatchedEmailsWithPartnerSuggestionsAsync | Partner suggestions returned | P1 |
| POS-021 | Get contacts for Gmail Add-on | Gmail request with emails | GetContactsForGmailAddon(request) | Matching contacts returned | P1 |
| POS-022 | Get contact search fields | None | GetContactSearchFields() | SearchFieldInfo list | P2 |
| POS-023 | Delete contact with interactions | Contact has 10 interactions | DeleteContactAsync | Contact soft-deleted, interactions preserved | P1 |
| POS-024 | Get contact — not found returns null | ID 99999 invalid | GetContact(99999) | Returns null | P1 |
| POS-025 | Update contact — non-existent | ID 99999 invalid | UpdateContactAsync | Returns null, no error | P1 |
| POS-026 | Delete contact — non-existent | ID 99999 invalid | DeleteContactAsync | Graceful completion | P1 |
| POS-027 | Specification with email domain filter | Contacts with varied domains | Specification filter | Correct subset | P2 |
| POS-028 | Pagination — multiple pages | 100 contacts | Query pages 1, 2, 3 | Correct records per page | P1 |
| POS-029 | Contact with special characters in name | Unicode name | Create "François O'Neill" | Stored and retrieved correctly | P2 |
| POS-030 | Get contact by email — multiple matches | 2 contacts same email | GetContactByEmailAsync | Single contact returned | P2 |
---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create — missing first name | FirstName null/empty | Validation error | P0 |
| NEG-002 | Create — missing last name | LastName null/empty | Validation error | P0 |
| NEG-003 | Create — missing email | Email null/empty | Validation error | P0 |
| NEG-004 | Create — invalid email format | "not-an-email" | Validation error | P0 |
| NEG-005 | Create — invalid partner ID | PartnerId=99999 (non-existent) | BusinessException/KeyNotFoundException | P0 |
| NEG-006 | Create — duplicate email same partner | john@example.com exists for partner 123 | Error or business rule handling | P1 |
| NEG-007 | Get contact — ID zero | GetContact(userId, 0) | Returns null or error | P1 |
| NEG-008 | Get contact — ID negative | GetContact(userId, -1) | Returns null or error | P1 |
| NEG-009 | Update — non-existent ID | UpdateContactAsync with ID 99999 | Returns null | P1 |
| NEG-010 | Update — missing required fields | UpdateRequest with null FirstName | Validation error | P0 |
| NEG-011 | Update — invalid email | UpdateRequest with malformed email | Validation error | P0 |
| NEG-012 | Delete — non-existent ID | DeleteContactAsync(99999) | Graceful handling | P1 |
| NEG-013 | Delete — already deleted contact | Contact IsDeleted=true | Idempotent or error | P1 |
| NEG-014 | Get partner contacts — invalid partner ID | GetPartnerContacts(99999) | Empty list or error | P1 |
| NEG-015 | Get contact by email — empty string | GetContactByEmailAsync("") | Returns null | P1 |
| NEG-016 | Get contact by email — null | GetContactByEmailAsync(null) | ArgumentNullException or null | P1 |
| NEG-017 | Profile picture — invalid file type | IFormFile with .exe | Rejected | P0 |
| NEG-018 | Profile picture — file too large | IFormFile > 5MB | Rejected | P1 |
| NEG-019 | Profile picture — corrupted image | Invalid image bytes | Error handling | P1 |
| NEG-020 | Pagination — invalid page index | PageIndex=-1 | Error or default to 0 | P1 |
| NEG-021 | Pagination — page size zero | PageSize=0 | Error or default | P1 |
| NEG-022 | Pagination — page size excessive | PageSize=10000 | Capped or error | P1 |
| NEG-023 | Specification — null specification | GetContactsWithSpecificationAsync(null) | ArgumentNullException | P1 |
| NEG-024 | Create — user without permissions | User lacks CanCreateContacts | 403 Forbidden | P0 |
| NEG-025 | Update — user without permissions | User lacks CanEditContacts | 403 Forbidden | P0 |
| NEG-026 | Delete — user without permissions | User lacks CanDeleteContacts | 403 Forbidden | P0 |
| NEG-027 | Get — user without view permissions | User lacks view access | 403 or empty | P0 |
| NEG-028 | Create — malformed JSON request | Invalid JSON body | 400 Bad Request | P1 |
| NEG-029 | Update — concurrent modification | Optimistic concurrency conflict | Concurrency exception | P1 |
| NEG-030 | Gmail Add-on — null request | GetContactsForGmailAddon(null) | ArgumentNullException | P1 |
| NEG-031 | Unmatched emails — empty list | GetUnmatchedEmailsWithPartnerSuggestionsAsync([]) | Empty result | P1 |
| NEG-032 | Create — whitespace-only name | FirstName="   " | Validation error | P1 |
| NEG-033 | Create — email with spaces | Email=" john@example.com " | Trimmed or rejected | P1 |
| NEG-034 | Update — partner deleted | PartnerId points to deleted partner | Error or handled | P1 |
| NEG-035 | Get contact — deleted contact | Contact IsDeleted=true | Not returned or marked | P1 |
| NEG-036 | Database timeout during create | Simulate DB timeout | Timeout exception, no partial save | P1 |
| NEG-037 | Database timeout during update | Simulate DB timeout | Rollback | P1 |
| NEG-038 | Network error during get | Simulate network failure | Appropriate exception | P2 |
| NEG-039 | Create — SQL injection in name | FirstName="'; DROP TABLE--" | Sanitized or rejected | P0 |
| NEG-040 | Create — XSS in notes | Notes="<script>alert(1)</script>" | Sanitized or rejected | P0 |
| NEG-041 | Get — expired auth token | Expired JWT | 401 Unauthorized | P1 |
| NEG-042 | Create — null ContactRequest | CreateContactAsync(null) | ArgumentNullException | P0 |
| NEG-043 | Update — null UpdateRequest | UpdateContactAsync(null) | ArgumentNullException | P0 |
| NEG-044 | Get partner contacts — null userId | GetPartnerContacts with null user | Error | P1 |
| NEG-045 | Create — partner soft-deleted | PartnerId=456 (IsDeleted=true) | Error | P1 |
| NEG-046 | Specification — invalid filter expression | Malformed specification | Error | P2 |
| NEG-047 | Create — email domain blocked | Email from blocked domain | Rejected | P2 |
| NEG-048 | Update — read-only contact | Contact in read-only state | Error | P1 |
| NEG-049 | Delete — contact with active references | Contact referenced elsewhere | Business rule handling | P1 |
| NEG-050 | Get contact — wrong org unit scope | User from different org unit | Empty or 403 | P0 |
| NEG-051 | Create — phone invalid format | Mobile="invalid" | Validation error | P2 |
| NEG-052 | Create — URL in notes (phishing) | Notes with suspicious URL | Sanitized | P2 |
| NEG-053 | Update — remove required field | UpdateRequest with Email=null | Validation error | P0 |
| NEG-054 | GetContacts — negative userId | userId=-1 | Error or empty | P1 |
| NEG-055 | Bulk create — partial failure | One invalid in batch | Transaction rollback or partial | P2 |
| NEG-056 | Create — salutation invalid | Salutation="INVALID" | Validation error | P2 |
| NEG-057 | Update — position too long | Position > 255 chars | Validation error | P1 |
| NEG-058 | Get contact by email — SQL injection | Email="' OR 1=1--" | Sanitized | P0 |
| NEG-059 | Create — address XSS | Address with script tag | Sanitized | P1 |
| NEG-060 | Profile picture — path traversal | Filename="../../../etc/passwd" | Rejected | P0 |
| NEG-061 | Get — ID max int overflow | ID=2147483647 | Handled | P2 |
| NEG-062 | Create — circular partner reference | Complex scenario | Error | P2 |
| NEG-063 | Update — stale LastModifiedDate | Optimistic concurrency | Concurrency error | P1 |
| NEG-064 | Delete — user different org | User from OrgUnit B deletes OrgUnit A contact | 403 | P0 |
| NEG-065 | Create — rate limit exceeded | Too many creates | 429 | P2 |
| NEG-066 | Get — specification with invalid property | Filter on non-existent property | Error | P2 |
| NEG-067 | Gmail Add-on — malformed email list | Invalid email format in request | Error | P1 |
| NEG-068 | Create — external ID collision | Duplicate external system ID | Error | P2 |
| NEG-069 | Update — contact locked | Contact locked by another user | Lock error | P2 |
| NEG-070 | Get contact — audit log failure | Audit service down | Get succeeds, audit queued | P2 |

| NEG-071 | Create — missing Title | Title null/empty | Validation error (Title required) | P0 |
| NEG-072 | Create — PartnerId zero | PartnerId=0 | Validation error (PartnerId required) | P0 |
| NEG-073 | Scan contact data — null file | ScanContactData with File=null | BusinessException | P1 |
| NEG-074 | Scan contact data — empty file | File.Length=0 | BusinessException | P1 |
| NEG-075 | Scan contact data — incompatible file type | File type not supported | BusinessException | P1 |
| NEG-076 | Bulk upload — null request | BulkUploadContacts(null) | BusinessException | P1 |
| NEG-077 | Bulk upload — invalid Type | Type!="Contact" | BusinessException | P1 |
| NEG-078 | Profile picture — null file | UploadProfilePicture with file=null | BusinessException | P1 |
| NEG-079 | Profile picture — empty file | File.Length=0 | BusinessException | P1 |
| NEG-080 | Profile picture — wrong content type | ContentType not image/jpeg|png|webp | BusinessException | P1 |
| NEG-081 | Advanced search — empty filters | filters="" or null | 400 Bad Request | P1 |
| NEG-082 | Advanced search — malformed JSON filters | Invalid JSON in filters | 400 Bad Request | P1 |
| NEG-083 | Search contacts — empty query | query="" or whitespace | BusinessException | P1 |
| NEG-084 | Detect duplicates — null request | DetectDuplicatesForContact(null) | 400 Bad Request | P1 |
| NEG-085 | Analyse contact file — null request | AnalyseContactData(null) | BusinessException | P1 |
| NEG-086 | Create — invalid OrganizationHierarchyIds | Non-existent org unit IDs | Error or handled | P2 |
| NEG-087 | Update — invalid OrganizationHierarchyIds | Non-existent org unit IDs | Error or handled | P2 |
| NEG-088 | Get contact — base manager throws NotSupported | GetContactSearchFields on base ContactManager | NotSupportedException | P1 |
| NEG-089 | Get contacts — base manager throws NotImplemented | GetContactsForGmailAddon on base ContactManager | NotImplementedException | P1 |
| NEG-090 | Get contact by email — base manager throws NotImplemented | GetContactByEmailAsync on base ContactManager | NotImplementedException | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | FirstName | 1 | 255 | "A" | 255 chars | 256 chars | P1 |
| BND-002 | LastName | 1 | 255 | "B" | 255 chars | 256 chars | P1 |
| BND-003 | Email | 5 | 320 | "a@b.c" | 320 chars | 321 chars | P1 |
| BND-004 | Mobile | 0 | 50 | "" | 50 chars | 51 chars | P1 |
| BND-005 | Phone | 0 | 50 | "" | 50 chars | 51 chars | P1 |
| BND-006 | Position | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-007 | Address | 0 | 500 | "" | 500 chars | 501 chars | P1 |
| BND-008 | Notes | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-009 | PartnerId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-010 | ContactId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-011 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-012 | PageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-013 | Pagination TotalCount | 0 | Max | 0 | Large | — | P2 |
| BND-014 | Email local part | 1 | 64 | "a" | 64 chars | 65 chars | P1 |
| BND-015 | Email domain | 1 | 253 | "a.b" | 253 chars | 254 chars | P1 |
| BND-016 | Salutation | 0 | 50 | "" | 50 chars | 51 chars | P2 |
| BND-017 | Profile picture size | 0 | 5MB | 0 bytes | 5MB | 5MB+1 | P1 |
| BND-018 | Contact count per partner | 0 | 10000 | 0 | 10000 | — | P2 |
| BND-019 | Interaction count per contact | 0 | 10000 | 0 | 10000 | — | P2 |
| BND-020 | Document count per contact | 0 | 1000 | 0 | 1000 | — | P2 |
| BND-021 | Unicode first name | — | — | "François" | "日本語" | Emoji | P2 |
| BND-022 | Unicode last name | — | — | "O'Brien" | "Müller" | — | P2 |
| BND-023 | Unicode email | — | — | IDN format | — | — | P2 |
| BND-024 | Special chars in notes | — | — | "O'Brien & Co." | — | — | P2 |
| BND-025 | Date Created | — | — | Min DateTime | Max DateTime | — | P2 |
| BND-026 | Empty specification result | 0 | — | 0 matches | — | — | P1 |
| BND-027 | Single contact in list | 1 | — | 1 contact | — | — | P1 |
| BND-028 | Max contacts in Gmail request | 1 | 100 | 1 | 100 | 101 | P1 |
| BND-029 | Unmatched emails batch | 0 | 500 | 0 | 500 | 501 | P2 |
| BND-030 | Zero PartnerId | — | — | PartnerId=0 | — | — | P1 |
| BND-031 | Null optional fields | — | — | All optional null | — | — | P1 |
| BND-032 | Empty string optional fields | — | — | "" for optional | — | — | P1 |
| BND-033 | Whitespace-trimmed names | — | — | "  John  " | — | — | P1 |
| BND-034 | Case-insensitive email match | — | — | "John@Example.COM" | — | — | P1 |
| BND-035 | Newline in notes | — | — | "Line1\nLine2" | — | — | P2 |
| BND-036 | Tab in address | — | — | "Street\tCity" | — | — | P2 |
| BND-037 | Very long single word | — | — | 255 char word | — | — | P2 |
| BND-038 | Negative ID in filter | — | — | ID=-1 in spec | — | — | P1 |
| BND-039 | Zero ID | — | — | GetContact(0) | — | — | P1 |
| BND-040 | Float ID (if applicable) | — | — | ID=1.5 | — | — | P2 |
| BND-041 | Timestamp precision | — | — | Millisecond precision | — | — | P2 |
| BND-042 | Timezone in dates | — | — | UTC stored | — | — | P2 |
| BND-043 | Pagination last page partial | — | — | 95 total, PageSize=20 | — | — | P1 |
| BND-044 | Pagination beyond last page | — | — | PageIndex=100, 10 pages | — | — | P1 |
| BND-045 | Sort by empty column | — | — | OrderBy="" | — | — | P2 |
| BND-046 | Filter by null value | — | — | PartnerId=null in spec | — | — | P1 |
| BND-047 | Collection empty vs null | — | — | Empty list vs null | — | — | P1 |
| BND-048 | Profile picture dimensions | — | — | 1x1 px | 4096x4096 | — | P2 |
| BND-049 | File extension case | — | — | .JPG vs .jpg | — | — | P1 |
| BND-050 | Concurrent create same email | — | — | 2 threads same email | — | — | P1 |
| BND-051 | Date boundary — leap year | — | — | 2024-02-29 | — | — | P2 |
| BND-052 | Date boundary — year 1 | — | — | 0001-01-01 | — | — | P2 |
| BND-053 | Date boundary — year 9999 | — | — | 9999-12-31 | — | — | P2 |
| BND-054 | Numeric boundary — Decimal | — | — | Amount fields | — | — | P2 |
| BND-055 | Empty collections in response | — | — | Documents=[] | — | — | P1 |
| BND-056 | Null vs empty Interactions | — | — | Both handled | — | — | P1 |
| BND-057 | Email + character | — | — | "test+tag@example.com" | — | — | P1 |
| BND-058 | Email subdomain | — | — | "a@mail.example.com" | — | — | P1 |
| BND-059 | International phone | — | — | +1-234-567-8900 | — | — | P2 |
| BND-060 | Multiple spaces in name | — | — | "John  Doe" | — | — | P2 |
| BND-061 | Leading/trailing spaces | — | — | " John " | — | — | P1 |
| BND-062 | Control characters | — | — | \x00 in name | — | — | P1 |
| BND-063 | RTL text | — | — | Arabic name | — | — | P2 |
| BND-064 | Combining characters | — | — | é as e+combining | — | — | P2 |
| BND-065 | Surrogate pairs | — | — | Emoji in name | — | — | P2 |
| BND-066 | HTML entities | — | — | &amp; &lt; | — | — | P1 |
| BND-067 | URL in notes | — | — | https://example.com | — | — | P2 |
| BND-068 | Very long search term | — | — | 255 char search | — | — | P2 |
| BND-069 | Empty filter criteria | — | — | Specification with no criteria | — | — | P1 |
| BND-070 | Max nested includes | — | — | Contact→Partner→OrgUnit | — | — | P2 |

| BND-071 | MailingStreet | 0 | 500 | "" | 500 chars | 501 chars | P2 |
| BND-072 | MailingStreet2 | 0 | 255 | "" | 255 chars | 256 chars | P2 |
| BND-073 | MailingCity | 0 | 100 | "" | 100 chars | 101 chars | P2 |
| BND-074 | MailingStateProvince | 0 | 100 | "" | 100 chars | 101 chars | P2 |
| BND-075 | MailingPostalCode | 0 | 20 | "" | 20 chars | 21 chars | P2 |
| BND-076 | MailingCountry | 0 | 100 | "" | 100 chars | 101 chars | P2 |
| BND-077 | Assistant | 0 | 255 | "" | 255 chars | 256 chars | P2 |
| BND-078 | AssistantPhone | 0 | 50 | "" | 50 chars | 51 chars | P2 |
| BND-079 | AssistantEmail | 0 | 320 | "" | 320 chars | 321 chars | P2 |
| BND-080 | Department | 0 | 255 | "" | 255 chars | 256 chars | P2 |
| BND-081 | Description | 0 | 4000 | "" | 4000 chars | 4001 chars | P2 |
| BND-082 | MiddleName | 0 | 255 | "" | 255 chars | 256 chars | P2 |
| BND-083 | Suffix | 0 | 50 | "" | 50 chars | 51 chars | P2 |
| BND-084 | OrganizationHierarchyIds | 0 | 100 | [] | 100 IDs | 101 IDs | P2 |
| BND-085 | Profile picture 1MB | 0 | 1MB | 0 bytes | 1MB | 1MB+1 | P1 |
| BND-086 | ListAllContacts pageIndex | 1 | Max | 1 | Valid | 0 | P1 |
| BND-087 | ListAllContacts pageSize | 1 | int.MaxValue (export) | 1 | 20 default | — | P1 |
| BND-088 | Search filters array | 1 | 50 | 1 filter | 50 filters | — | P2 |
| BND-089 | Gmail email addresses | 1 | 100 | 1 email | 100 emails | 101 | P1 |
| BND-090 | GetByIdsAsync ids array | 1 | 500 | 1 ID | 500 IDs | — | P2 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete sets IsDeleted | Delete operation | DeleteContactAsync | IsDeleted=true, DeletedBy, DeletedDate set | P0 |
| FUN-002 | Soft-deleted contacts excluded from lists | Query active contacts | GetPartnerContacts, GetPostedContacts | Deleted contacts not returned | P0 |
| FUN-003 | CreatedBy/CreatedDate on create | Create contact | CreateContactAsync | Audit fields set | P0 |
| FUN-004 | LastModifiedBy/LastModifiedDate on update | Update contact | UpdateContactAsync | Audit fields updated | P0 |
| FUN-005 | Name required per ModifiableDeletableEntity | Create without Name | CreateContactAsync | Name set (e.g., FirstName + LastName) | P0 |
| FUN-006 | Partner association optional | Create with null PartnerId | CreateContactAsync | Contact created | P1 |
| FUN-007 | Same email different partners allowed | Business rule | Create contact same email for partner 456 | Created | P1 |
| FUN-008 | Duplicate email same partner | Business rule | Create duplicate for same partner | Per business rule (error or allow) | P1 |
| FUN-009 | Interactions preserved on delete | Cascade rule | Delete contact with interactions | Interactions not deleted | P0 |
| FUN-010 | Documents preserved on delete | Cascade rule | Delete contact with documents | Documents handled per rule | P1 |
| FUN-011 | Partner change updates relationship | Update partner | UpdateContactAsync with new PartnerId | Relationship updated | P1 |
| FUN-012 | Remove partner clears association | Update with null | UpdateContactAsync PartnerId=null | PartnerId=null | P1 |
| FUN-013 | Get by ID returns null if not found | Get non-existent | GetContact(99999) | Null | P0 |
| FUN-014 | Pagination returns correct slice | Pagination | GetContacts PageIndex=2, PageSize=10 | Records 11-20 | P0 |
| FUN-015 | TotalCount accurate | Pagination | GetContacts | TotalCount matches total | P0 |
| FUN-016 | Specification filter applied | Specification | GetContactsWithSpecificationAsync | Only matching contacts | P0 |
| FUN-017 | Email lookup case handling | Email search | GetContactByEmailAsync | Case-insensitive or as designed | P1 |
| FUN-018 | Gmail Add-on returns matching contacts | Gmail integration | GetContactsForGmailAddon | Contacts for provided emails | P1 |
| FUN-019 | Unmatched emails suggestions | Partner matching | GetUnmatchedEmailsWithPartnerSuggestionsAsync | Suggestions returned | P1 |
| FUN-020 | Search fields metadata | GetSearchFields | GetContactSearchFields | Correct SearchFieldInfo | P1 |
| FUN-021 | Eligible entities on posted contact | Posted contact | GetPostedContact | EligibleEntities loaded | P1 |
| FUN-022 | Profile picture updates URL | Upload picture | UpdateContactProfilePictureAsync | LogoUrl/ProfilePictureUrl updated | P1 |
| FUN-023 | Update non-existent returns null | Update missing | UpdateContactAsync(99999, model) | Null, no exception | P1 |
| FUN-024 | Delete non-existent graceful | Delete missing | DeleteContactAsync(99999) | No exception | P1 |
| FUN-025 | Org unit scope filtering | Permission scope | GetContacts for user | Only contacts in user's org scope | P0 |
| FUN-026 | Creator has full access | Permission | Creator updates own contact | Allowed | P0 |
| FUN-027 | Non-creator view access | Permission | Other user views contact | Per role | P1 |
| FUN-028 | Non-creator edit denied | Permission | Other user edits contact | 403 or blocked | P0 |
| FUN-029 | Specification pagination | Specification + pagination | Both applied | Correct paginated filtered results | P1 |
| FUN-030 | Sort order applied | Sort parameter | GetContacts with OrderBy | Results sorted | P1 |
| FUN-031 | Multiple sort columns | Multi-sort | OrderBy Name, Email | Correct order | P2 |
| FUN-032 | Date filtering | Date range in spec | Filter by CreatedDate | Correct subset | P1 |
| FUN-033 | Partner filter in specification | Partner filter | Filter by PartnerId | Correct subset | P1 |
| FUN-034 | Email domain filter | Domain filter | Filter by @example.com | Correct subset | P1 |
| FUN-035 | Status filter | Status filter | Filter by Status | Correct subset | P1 |
| FUN-036 | Bulk operations atomicity | Bulk create | Create 10, one fails | Per transaction design | P2 |
| FUN-037 | Idempotent delete | Delete twice | DeleteContactAsync twice | Second call graceful | P1 |
| FUN-038 | Concurrent update conflict | Two users update | Optimistic concurrency | One succeeds, one gets conflict | P1 |
| FUN-039 | Contact with no interactions | Empty interactions | GetContactWithInteractionsAsync | Empty list | P1 |
| FUN-040 | Contact with no documents | Empty documents | GetContact | Documents=[] | P1 |
| FUN-041 | Change partner to deleted | Update to deleted partner | UpdateContactAsync | Error or handled | P1 |
| FUN-042 | Audit trail on create | Create | CreateContactAsync | Audit log entry | P1 |
| FUN-043 | Audit trail on update | Update | UpdateContactAsync | Audit log entry | P1 |
| FUN-044 | Audit trail on delete | Delete | DeleteContactAsync | Audit log entry | P1 |
| FUN-045 | WorkflowStatus if applicable | Contact workflow | Check WorkflowStatus | Per entity design | P2 |
| FUN-046 | Status transitions | Status changes | Update Status | Valid transitions only | P1 |
| FUN-047 | Required field validation | Missing required | Create/Update | Validation errors | P0 |
| FUN-048 | Email format validation | Invalid email | Create/Update | Validation error | P0 |
| FUN-049 | Name length validation | Name > 255 | Create/Update | Validation error | P1 |
| FUN-050 | Optional vs required fields | All optional null | Create with required only | Created | P1 |

| FUN-051 | Name concatenation | Name = Salutation + FirstName + MiddleName + LastName | CreateContactAsync, UpdateContactAsync | Name field populated correctly | P0 |
| FUN-052 | OrganizationUnitRelationships on create | Create with OrganizationHierarchyIds | CreateContactAsync | OrganizationUnitRelationship records created | P1 |
| FUN-053 | OrganizationUnitRelationships on update | Update with new OrganizationHierarchyIds | UpdateContactAsync | Old removed, new added | P1 |
| FUN-054 | Soft delete OrganizationUnitRelationships on delete | Delete contact | DeleteContactAsync | OrgUnitRelationships soft-deleted | P1 |
| FUN-055 | ContactSpecificationAdapter routing | Specification is ContactSpecificationAdapter | GetContactsWithSpecificationAsync | Routes to GetUNOPSContactsWithSpecificationAsync | P1 |
| FUN-056 | ApplyAccessControlFilters on GetContact | User has access | GetContactAsync(ClaimsPrincipal, id) | Contact returned if access granted | P0 |
| FUN-057 | ApplyAccessControlFilters on GetContact — denied | User lacks access | GetContactAsync(ClaimsPrincipal, id) | Null returned | P0 |
| FUN-058 | GlobalFilterService applied | FilterActive=true | GetContactsWithSpecification | Global filters applied | P1 |
| FUN-059 | GetContactByEmailAsync case-insensitive | Email="John@Example.COM" | GetContactByEmailAsync | Matches john@example.com | P1 |
| FUN-060 | GetContactsForGmailAddon lowercase | EmailAddresses with mixed case | GetContactsForGmailAddon | Contacts matched case-insensitively | P1 |
| FUN-061 | GetContactWithInteractionsAsync structure | Contact has interactions | GetContactWithInteractionsAsync | JSON with interactions, documents, summary | P1 |
| FUN-062 | GetContactWithInteractionsAsync — no interactions | Contact has 0 interactions | GetContactWithInteractionsAsync | totalInteractions=0 | P1 |
| FUN-063 | Profile picture signed URL | Upload profile picture | UpdateContactProfilePictureAsync | Signed URL returned | P1 |
| FUN-064 | ProfilePictureUrl in model | Get contact with profile picture | GetContactAsync | ProfilePictureUrl as signed URL | P1 |
| FUN-065 | CreatedByName/CreatedByOfficeName | Contact created by user | GetContactAsync | CreatedByName, CreatedByOfficeName populated | P1 |
| FUN-066 | PatchNonNullProperties on update | Update with partial fields | UpdateContactAsync | Only non-null fields updated | P1 |
| FUN-067 | GetPartnerContacts — Partner.PartnerGroup | Partner has PartnerGroup | GetPartnerContacts | PartnerGroup loaded | P1 |
| FUN-068 | GetContacts — LoadOrganizationUnitRelationships | Load contacts | GetContacts | OrgUnitRelationships loaded for each | P1 |
| FUN-069 | GetPostedContacts — all contacts | Posted contacts | GetPostedContacts | All contacts returned (no IsPosted filter) | P1 |
| FUN-070 | GetUnmatchedEmailsWithPartnerSuggestions — domain match | Email domain exists in contacts | GetUnmatchedEmailsWithPartnerSuggestionsAsync | PartnerId/PartnerName from most common partner | P1 |
| FUN-071 | GetUnmatchedEmailsWithPartnerSuggestions — Gemini fallback | Domain not in DB | GetUnmatchedEmailsWithPartnerSuggestionsAsync | Gemini lookup for unknown domains | P2 |
| FUN-072 | GetByIdsAsync — access control | User with limited scope | GetByIdsAsync(ids, user) | Only accessible contacts returned | P1 |
| FUN-073 | GetBasicEntityAsync — contact | GetBasicEntityAsync | BaseUNOPSManager | ContactModel returned | P1 |
| FUN-074 | GetBasicEntityDataAsync — contact | GetBasicEntityDataAsync | BaseUNOPSManager | ContactModel returned | P1 |
| FUN-075 | GetPartnerNamesFromGeminiAsync — empty domains | Contact has no email domain | GetPartnerNamesFromGeminiAsync | Empty result with fallbacks | P2 |
| FUN-076 | GetPartnerNamesForAIAsync — prompt not found | No AiPrompt for domain_organization_lookup | GetPartnerNamesForAIAsync | Fallback values, error in response | P2 |
| FUN-077 | Search fields — fullName | GetContactSearchFields | GetContactSearchFields | fullName field with operators | P1 |
| FUN-078 | Search fields — partner.name | GetContactSearchFields | GetContactSearchFields | partner.name IsNavigationProperty=true | P1 |
| FUN-079 | Search fields — status enum | GetContactSearchFields | GetContactSearchFields | status with DropdownOptions | P1 |
| FUN-080 | Search fields — createdDate | GetContactSearchFields | GetContactSearchFields | createdDate with date operators | P1 |
| FUN-081 | Gmail Add-on — permissions per contact | GetContactsForGmailAddon | GetContactsForGmailAddon | Contacts include Permissions | P1 |
| FUN-082 | Gmail Add-on — interactions per contact | GetContactsForGmailAddon | GetContactsForGmailAddon | Interactions include Permissions | P1 |
| FUN-083 | PageIndex < 1 default | GetContacts PageIndex=0 | GetContacts | PageIndex defaults to 1 | P1 |
| FUN-084 | GetContacts — user info batch lookup | GetContacts with multiple creators | GetContacts | Single userInfo query | P1 |
| FUN-085 | GetContactsWithSpecification — filteredData is list | ApplyAccessControlFilters returns list | GetContactsWithSpecificationAsync | Pagination on filtered list | P1 |
| FUN-086 | GetContactsWithSpecification — filteredData is not list | ApplyAccessControlFilters returns other type | GetContactsWithSpecificationAsync | Empty result fallback | P1 |
| FUN-087 | GetContactWithInteractionsAsync — hasCV | Document link contains "cv" | GetContactWithInteractionsAsync | hasCV=true in summary | P1 |
| FUN-088 | GetContactWithInteractionsAsync — mailingAddress | Contact has MailingStreet | GetContactWithInteractionsAsync | mailingAddress object in response | P1 |
| FUN-089 | GetContactWithInteractionsAsync — assistant | Contact has Assistant | GetContactWithInteractionsAsync | assistant object in response | P1 |
| FUN-090 | GetContactWithInteractionsAsync — userProfile | GetContactWithInteractionsAsync | GetContactWithInteractionsAsync | userProfile included | P1 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD workflow | Create→Get→Update→Get→Delete | Contact | All succeed | P0 |
| INT-002 | Contact with Partner | Create contact for partner | Contact, Partner | Relationship established | P0 |
| INT-003 | Contact with Documents | Link documents to contact | Contact, Document | Documents accessible | P1 |
| INT-004 | Contact with Interactions | Create interactions for contact | Contact, Interaction | Interactions loaded | P1 |
| INT-005 | Contact→Partner→OrgUnit | Load contact with hierarchy | Contact, Partner, OrgUnit | Full hierarchy | P1 |
| INT-006 | Partner contacts pagination | Get paginated partner contacts | Partner, Contact | Correct page | P1 |
| INT-007 | Search across contacts | Specification search | Contact | Filtered results | P1 |
| INT-008 | Contact sync from Gmail | Gmail Add-on sync | Contact, GmailAddonManager | Contacts matched | P1 |
| INT-009 | Contact import batch | Bulk import | Contact | All created | P1 |
| INT-010 | Contact export | Export contacts | Contact | Export file | P2 |
| INT-011 | DocumentManager upload for contact | Upload document to contact | Contact, DocumentManager | Document linked | P1 |
| INT-012 | InteractionManager create for contact | Create interaction | Contact, InteractionManager | Interaction linked | P1 |
| INT-013 | PartnerManager get partner for contact | Get contact's partner | Contact, Partner | Partner loaded | P1 |
| INT-014 | Permission check for contact | Authorize contact action | Contact, PermissionService | Correct allow/deny | P0 |
| INT-015 | Audit log on contact actions | Audit contact CRUD | Contact, AuditLog | Entries created | P1 |
| INT-016 | UserContext in contact operations | Current user in request | Contact, UserResolver | UserId applied | P0 |
| INT-017 | Contact in opportunity context | Contact as stakeholder | Contact, Opportunity | Link works | P1 |
| INT-018 | Contact in interaction context | Interaction with contact | Contact, Interaction | Both linked | P1 |
| INT-019 | Contact search with filter | Multi-criteria search | Contact | Results correct | P1 |
| INT-020 | Pagination consistency | Sequential pages | Contact | No duplicates, no gaps | P0 |
| INT-021 | Contact cascading delete | Delete partner with contacts | Partner, Contact | Per cascade rule | P1 |
| INT-022 | Contact merge scenario | Merge duplicates | Contact | Per merge design | P2 |
| INT-023 | Contact deduplication | Gmail dedup | Contact, GmailAddon | Dedup applied | P1 |
| INT-024 | Contact in list view | List view with contacts | Contact, ListView | Display correct | P1 |
| INT-025 | Contact in detail view | Detail view | Contact | All sections load | P0 |
| INT-026 | Contact typeahead | Typeahead for contact | Contact | Suggestions returned | P1 |
| INT-027 | Contact autocomplete | Autocomplete by name | Contact | Matches returned | P1 |
| INT-028 | Contact link to opportunity | Link contact to opportunity | Contact, Opportunity | Link created | P1 |
| INT-029 | Contact unlink from opportunity | Remove link | Contact, Opportunity | Link removed | P1 |
| INT-030 | Contact preference storage | Save user preference | Contact, UserDataManager | Preference saved | P2 |
| INT-031 | Contact notification | Notify on contact create | Contact, NotificationManager | Notification sent | P2 |
| INT-032 | Contact in Report | Report with contacts | Contact, Report | Data correct | P2 |
| INT-033 | Contact API → Controller | API call Create | Contact, Controller | 201 Created | P0 |
| INT-034 | Contact API Get | API call Get | Contact, Controller | 200 OK | P0 |
| INT-035 | Contact API Update | API call Update | Contact, Controller | 200 OK | P0 |
| INT-036 | Contact API Delete | API call Delete | Contact, Controller | 204 No Content | P0 |
| INT-037 | Contact API 404 | Get non-existent | Contact, Controller | 404 Not Found | P0 |
| INT-038 | Contact API 400 | Invalid request | Contact, Controller | 400 Bad Request | P0 |
| INT-039 | Contact API 403 | Unauthorized | Contact, Controller | 403 Forbidden | P0 |
| INT-040 | Contact AutoMapper | Entity to Model | Contact, AutoMapper | Correct mapping | P1 |
| INT-041 | Contact Request to Entity | Request mapping | CreateRequest, AutoMapper | Entity populated | P1 |
| INT-042 | Contact DbContext | Save to database | Contact, DbContext | Persisted | P0 |
| INT-043 | Contact repository | Repository pattern | Contact, DataRepository | CRUD via repository | P1 |
| INT-044 | Contact ManagerWrapper | Manager resolution | ManagerWrapper.ContactManager | Correct manager | P1 |
| INT-045 | Contact validation service | Validation | Contact, Validator | Errors returned | P1 |
| INT-046 | Contact error handling | Global exception handler | Contact, ExceptionHandler | Consistent error response | P1 |
| INT-047 | Contact logging | Log contact operations | Contact, ILogger | Logs written | P2 |
| INT-048 | Contact configuration | Config-driven behavior | Contact, IConfiguration | Config applied | P2 |
| INT-049 | Contact feature flag | Feature flag for contact | Contact, FeatureFlags | Flag respected | P2 |
| INT-050 | Contact multi-tenant | Org scope isolation | Contact, Tenant | Data isolated | P0 |

| INT-051 | Contact→OrganizationUnitRelationship | Create contact with OrganizationHierarchyIds | Contact, OrganizationUnitRelationship | Relationships persisted | P1 |
| INT-052 | Contact→Partner→PartnerGroup | Get contact | Contact, Partner, PartnerGroup | Full hierarchy loaded | P1 |
| INT-053 | Contact→Partner→LiaisonOffice | GetContactWithInteractionsAsync | Contact, Partner, LiaisonOffice | LiaisonOffice in response | P1 |
| INT-054 | Contact→Documents→DocumentType | Get contact with documents | Contact, Document, DocumentType | DocumentType loaded | P1 |
| INT-055 | Contact→InteractionContacts→Interaction | GetContactsForGmailAddon | Contact, InteractionContact, Interaction | Interactions per contact | P1 |
| INT-056 | Contact→InteractionUsers→User | GetContactWithInteractionsAsync | Contact, Interaction, User | Users in interaction | P1 |
| INT-057 | ContactController→AdvancedSearchService | SearchContacts with query | Controller, AdvancedSearchService, Contact | SearchWithQueryAndMetadataAsync | P1 |
| INT-058 | ContactController→AdvancedSearchService filters | AdvancedSearchContacts | Controller, AdvancedSearchService | SearchWithFiltersAsync | P1 |
| INT-059 | ContactController→AiContextualService | Create with duplicate detection | Controller, AiContextualService | DetectDuplicateForSingleRecordAsync | P1 |
| INT-060 | ContactController→GeminiManager | ScanContactData | Controller, GeminiManager | ScanFileForGeminiProcessing | P1 |
| INT-061 | ContactController→GeminiManager | AnalyseContactData | Controller, GeminiManager | ExtractDataAfterAnalysis | P1 |
| INT-062 | ContactController→GeminiManager | BulkUploadContacts | Controller, GeminiManager | BulkInsertRecordsAsync | P1 |
| INT-063 | ContactController→EntityConfigurationManager | GetMetadataInfo | Controller, EntityConfigurationManager | GetEntityConfigurationDetailsAsync | P1 |
| INT-064 | UNOPSContactManager→GoogleCloudStorageService | UpdateContactProfilePictureAsync | ContactManager, GoogleCloudStorageService | UploadFileAsync, signed URL | P1 |
| INT-065 | UNOPSContactManager→PermissionService | GetContactsForGmailAddon | ContactManager, PermissionService | HasInstanceAccessAsync per contact | P1 |
| INT-066 | UNOPSContactManager→AiContextualService | GetPartnerNamesForAIAsync | ContactManager, AiContextualService | FetchResultFromGemini | P2 |
| INT-067 | UNOPSContactManager→GlobalFilterService | GetContactsWithSpecification | ContactManager, GlobalFilterService | ApplyGlobalFiltersAsync | P1 |
| INT-068 | UNOPSContactManager→UserProfile | MapEntityToModel | ContactManager, UserProfile | CreatedByName from UserProfile | P1 |
| INT-069 | UNOPSContactManager→OrganizationHierarchy | MapEntityToModel | ContactManager, OrganizationHierarchy | CreatedByOfficeName from OrgUnit | P1 |
| INT-070 | Contact→InteractionContacts junction | GetContactsForGmailAddon | Contact, InteractionContact | Junction table queried | P1 |
| INT-071 | ListAllContacts→ContactCompositeSpecification | ListAllContacts with partnerId | Controller, ContactCompositeSpecification | Filtered results | P1 |
| INT-072 | SearchContacts→UnifiedSearchRequest | Search with partnerId filter | AdvancedSearchService, SearchFilter | PartnerId filter applied | P1 |
| INT-073 | Contact API permissions endpoint | GET /contact/{id}/permissions | Controller, AuthorizationService | Permissions returned | P0 |
| INT-074 | Contact API profile picture | POST /contact/{id}/profile-picture | Controller, ContactManager | 200 OK with imageUrl | P1 |
| INT-075 | Contact API detect-duplicates | POST /contact/detect-duplicates | Controller, AiContextualService | Duplicate info returned | P1 |
| INT-076 | Contact API metadata-info | GET /contact/metadata-info | Controller, EntityConfigurationManager | Entity config returned | P1 |
| INT-077 | Contact API scan-data | POST /contact/scan-data | Controller, GeminiManager | Extracted data returned | P1 |
| INT-078 | Contact API analyse-file | POST /contact/analyse-file | Controller, GeminiManager | Structured data returned | P1 |
| INT-079 | Contact API bulk-upload | POST /contact/bulk-upload | Controller, GeminiManager | Bulk result returned | P1 |
| INT-080 | Contact API search-fields | GET /contact/search-fields | Controller, ContactManager | SearchFieldInfo list | P1 |
| INT-081 | BaseRepository→UNOPSAppDbContext | Contact CRUD | BaseRepository, DbContext | Persisted via context | P0 |
| INT-082 | Contact→Partner FK | Create contact for partner | Contact, Partner | PartnerId FK valid | P0 |
| INT-083 | Contact LoadOrganizationUnitRelationshipsAsync | GetPartnerContacts | Contact, Extension method | OrgUnitRelationships loaded | P1 |
| INT-084 | Partner LoadOrganizationUnitRelationshipsAsync | GetPartnerContacts | Partner, Extension method | Partner OrgUnitRelationships loaded | P1 |
| INT-085 | ContactFilterRequest→ContactCompositeSpecification | ListAllContacts | ContactFilterRequest, Specification | Specification built | P1 |
| INT-086 | ContactSpecificationAdapter→GetUNOPSContactsWithSpecification | Specification with adapter | ContactSpecificationAdapter, Manager | UNOPS-specific path | P1 |
| INT-087 | UNOPSContactManager→AiPrompt | GetPartnerNamesFromGeminiAsync | ContactManager, AiPrompt | domain_organization_lookup prompt | P2 |
| INT-088 | UNOPSContactManager→CommonEntityRepository | Base entity operations | ContactManager, CommonEntityRepository | Shared repository | P2 |
| INT-089 | Contact→Interaction many-to-many | Interaction with contact | Contact, Interaction, InteractionContact | Many-to-many via junction | P1 |
| INT-090 | UNOPSManagerWrapper→UNOPSContactManager | Resolve ContactManager | ManagerWrapper, UNOPSContactManager | UNOPS implementation used | P0 |

---

## §6 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent create same partner | 10 threads create for partner 123 | All 10 created, unique IDs | P0 |
| CON-002 | Concurrent update same contact | 5 threads update contact 789 | No corruption, last-write or optimistic | P0 |
| CON-003 | Concurrent delete and read | Thread1 delete, Thread2 read contact 789 | Consistent result | P0 |
| CON-004 | Concurrent email lookups | 20 threads GetContactByEmailAsync | All correct results | P1 |
| CON-005 | Concurrent partner contact queries | 15 threads GetPartnerContacts(123) | Same results | P1 |
| CON-006 | Create during partner update | Thread1 update partner, Thread2 create contact | Contact created | P1 |
| CON-007 | Concurrent profile picture uploads | 3 threads upload for contact 789 | One wins | P1 |
| CON-008 | Concurrent specification queries | 25 threads different specs | All succeed | P1 |
| CON-009 | Bulk create with concurrent reads | Thread1 create 100, Thread2 query | Queries consistent | P1 |
| CON-010 | Concurrent Gmail Add-on requests | 10 threads GetContactsForGmailAddon | All correct | P1 |
| CON-011 | Optimistic concurrency conflict | 2 users update same contact | One gets conflict | P0 |
| CON-012 | Concurrent pagination | 100 threads different pages | Correct pages | P1 |
| CON-013 | Concurrent delete same contact | 2 threads DeleteContactAsync(789) | One succeeds, one graceful | P1 |
| CON-014 | Create and update same contact | Thread1 create, Thread2 update new | Handled | P1 |
| CON-015 | Concurrent permission checks | 50 threads HasPermission | All correct | P1 |
| CON-016 | Deadlock — circular dependency | Contact A→Partner, Partner→Contact | No deadlock | P1 |
| CON-017 | Connection pool exhaustion | 100 concurrent operations | Pool holds, no exhaustion | P1 |
| CON-018 | Transaction isolation | Read uncommitted | Thread1 create, Thread2 read | Per isolation level | P1 |
| CON-019 | Lost update | 2 users update different fields | Per design | P1 |
| CON-020 | Phantom read | Thread1 insert, Thread2 paginate | Per isolation | P2 |
| CON-021 | Non-repeatable read | Thread1 read, Thread2 update, Thread1 read | Per isolation | P2 |
| CON-022 | Cache poisoning | Concurrent updates to shared cache | Cache consistent | P1 |
| CON-023 | Double submit | User double-clicks Create | One contact created | P0 |
| CON-024 | Race on duplicate email | 2 threads create same email same partner | One succeeds | P1 |
| CON-025 | Bulk delete concurrency | 2 threads bulk delete overlapping | Consistent state | P1 |

---

## §7 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Email validation — valid | Validation | "user@example.com" | Valid | P0 |
| UNT-002 | Email validation — invalid | Validation | "invalid" | Invalid | P0 |
| UNT-003 | Email validation — empty | Validation | "" | Invalid | P0 |
| UNT-004 | Name validation — required | Validation | FirstName null | Error | P0 |
| UNT-005 | Name trim | Formatting | "  John  " | "John" | P1 |
| UNT-006 | Email trim | Formatting | " user@example.com " | "user@example.com" | P1 |
| UNT-007 | Phone format | Formatting | "+1-234-567-8900" | Accepted or formatted | P1 |
| UNT-008 | Full name composition | Calculation | First="John", Last="Doe" | "John Doe" | P1 |
| UNT-009 | Contact display name | Calculation | Salutation, First, Last | "Mr. John Doe" | P1 |
| UNT-010 | Status — Active | Status logic | Status=Active | IsActive true | P1 |
| UNT-011 | Status — Inactive | Status logic | Status=Inactive | IsActive false | P1 |
| UNT-012 | Status — Deleted | Status logic | IsDeleted=true | Excluded from active | P0 |
| UNT-013 | Collection count | Collections | Contacts.Count | Correct count | P1 |
| UNT-014 | Empty collection | Collections | No contacts | Count=0 | P1 |
| UNT-015 | Null to empty handling | Collections | Null list | Return [] | P1 |
| UNT-016 | Map Contact to Model | Mapping | Contact entity | ContactModel | P0 |
| UNT-017 | Map Request to Entity | Mapping | CreateContactRequest | Contact entity | P0 |
| UNT-018 | Pagination slice | Calculation | PageIndex=1, Size=10 | Skip 10, Take 10 | P1 |
| UNT-019 | Specification combine | Validation | Spec1 AND Spec2 | Combined filter | P1 |
| UNT-020 | PartnerId null check | Status logic | PartnerId=null | No partner link | P1 |
| UNT-021 | Audit fields default | Status logic | New contact | CreatedBy, CreatedDate set | P1 |

---

## §8 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create contact response time | CreateContactAsync | < 300ms | P0 |
| PRF-002 | Get partner contacts — 500 | GetPartnerContacts(500 contacts) | < 1000ms | P0 |
| PRF-003 | Get contact with 200 interactions | GetContactWithInteractionsAsync | < 1500ms | P0 |
| PRF-004 | Search 50K contacts | GetContacts filtered | < 2000ms | P0 |
| PRF-005 | Bulk create throughput | 1000 contacts | > 30/sec | P1 |
| PRF-006 | Update with profile picture 2MB | UpdateContactProfilePictureAsync | < 2000ms | P1 |
| PRF-007 | Pagination — 10K contacts | GetContacts page 1 | < 500ms | P1 |
| PRF-008 | Specification complex filter | Multi-criteria query | < 1500ms | P1 |
| PRF-009 | Delete with 70 relationships | DeleteContactAsync | < 1000ms | P1 |
| PRF-010 | Unmatched emails 100 batch | GetUnmatchedEmailsWithPartnerSuggestions | < 5000ms | P1 |
| PRF-011 | Gmail Add-on 50 emails | GetContactsForGmailAddon | < 1000ms | P1 |
| PRF-012 | Memory — pagination 50 records | GetContacts PageSize=50 | Memory < 50MB | P1 |
| PRF-013 | Single GetContact | GetContact by ID | < 100ms | P0 |
| PRF-014 | Get by email | GetContactByEmailAsync | < 200ms | P1 |
| PRF-015 | Full contact load (docs+interactions) | GetContact with includes | < 500ms | P1 |
| PRF-016 | Search fields metadata | GetContactSearchFields | < 50ms | P2 |

---

## §9 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained load — 10 req/s create | 10 concurrent creates/sec | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-002 | Sustained load — 50 req/s get | 50 GetContact/sec | 5 min | 95% < 200ms | P0 |
| LDT-003 | Sustained load — 20 req/s search | 20 search/sec | 5 min | 95% < 1000ms | P0 |
| LDT-004 | Spike — 100 req/s for 1 min | 100 req/s burst | 1 min | No crash, recover | P0 |
| LDT-005 | Spike — 200 req/s for 30 sec | 200 req/s burst | 30 sec | Degrade gracefully | P1 |
| LDT-006 | Stress — increase to failure | Ramp 1→500 req/s | Until failure | Identify breaking point | P1 |
| LDT-007 | Stress — connection pool | 200 concurrent connections | 2 min | No exhaustion | P1 |
| LDT-008 | Stress — memory | 10K contacts loaded | 5 min | No leak | P1 |
| LDT-009 | Recovery — after spike | Spike then normal load | 5 min normal | Return to baseline | P0 |
| LDT-010 | Recovery — after stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
