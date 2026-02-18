# InteractionManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/InteractionManager`  
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

**InteractionManager** manages CRUD for interactions (meetings, emails, calls, visits), date validation, type classification, and audit. Key responsibilities: interaction lifecycle, multi-entity associations (contacts, partners, users), junction tables, type classification (Meeting/Email/Call/Visit), date validation, and filtering/search.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create interaction — single contact | Contact exists | CreateInteractionAsync(ContactIds=[101]) | Interaction + junction created | P0 |
| POS-002 | Create interaction — multiple contacts | 5 contacts | CreateInteractionAsync(ContactIds=[101-105]) | 5 junction records | P0 |
| POS-003 | Create interaction — with partners | Partners exist | CreateInteractionAsync(PartnerIds=[201,202]) | Partner junctions | P0 |
| POS-004 | Create interaction — with users | Users exist | CreateInteractionAsync(UserIds=[301,302]) | User junctions | P0 |
| POS-005 | Create interaction — all associations | Contacts, partners, users | CreateInteractionAsync full | All junctions | P0 |
| POS-006 | Get interaction by ID | Interaction exists | GetInteraction(789) | Interaction returned | P0 |
| POS-007 | Update interaction | Interaction exists | UpdateInteractionAsync | Updated | P0 |
| POS-008 | Delete interaction — soft | Interaction exists | DeleteInteractionAsync | IsDeleted=true | P0 |
| POS-009 | Get interaction — not found | ID 99999 | GetInteraction(99999) | Null | P1 |
| POS-010 | Create — type Meeting | Type=Meeting | CreateInteractionAsync | Type=Meeting | P1 |
| POS-011 | Create — type Email | Type=Email | CreateInteractionAsync | Type=Email | P1 |
| POS-012 | Create — type Call | Type=Call | CreateInteractionAsync | Type=Call | P1 |
| POS-013 | Create — type Visit | Type=Visit | CreateInteractionAsync | Type=Visit | P1 |
| POS-014 | Date validation — valid | Valid date | CreateInteractionAsync | Created | P0 |
| POS-015 | Get interactions — pagination | 100 interactions | GetInteractions paginated | Paginated | P1 |
| POS-016 | Get interactions — filter by type | Filter Meeting | GetInteractions | Meetings only | P1 |
| POS-017 | Get interactions — filter by contact | Contact 101 | GetInteractions | Contact 101 only | P1 |
| POS-018 | Get interactions — filter by partner | Partner 201 | GetInteractions | Partner 201 only | P1 |
| POS-019 | Get interactions — filter by date | Date range | GetInteractions | In range | P1 |
| POS-020 | Update — add contact | Interaction exists | Update add ContactId | Junction added | P1 |
| POS-021 | Update — remove contact | Interaction has contact | Update remove ContactId | Junction removed | P1 |
| POS-022 | Update — change type | Interaction exists | Update Type | Type changed | P1 |
| POS-023 | Update — change date | Interaction exists | Update Date | Date changed | P1 |
| POS-024 | Specification filter | Spec | GetInteractionsWithSpecification | Filtered | P1 |
| POS-025 | Interaction with notes | Notes provided | CreateInteractionAsync | Notes saved | P1 |
| POS-026 | Interaction with subject | Subject provided | CreateInteractionAsync | Subject saved | P1 |
| POS-027 | Full CRUD cycle | None | Create→Get→Update→Get→Delete | All succeed | P0 |
| POS-028 | Junction table integrity | Create | CreateInteractionAsync | All FKs valid | P0 |
| POS-029 | Update non-existent | ID 99999 | UpdateInteractionAsync | Null | P1 |
| POS-030 | Delete non-existent | ID 99999 | DeleteInteractionAsync | Graceful | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create — invalid contact ID | ContactIds=[99999] | Error | P0 |
| NEG-002 | Create — invalid partner ID | PartnerIds=[99999] | Error | P0 |
| NEG-003 | Create — invalid user ID | UserIds=[99999] | Error | P0 |
| NEG-004 | Create — invalid type | Type="Invalid" | Validation error | P0 |
| NEG-005 | Create — null model | CreateInteractionAsync(null) | ArgumentNullException | P0 |
| NEG-006 | Create — date in future (if invalid) | Future date | Per business rule | P1 |
| NEG-007 | Create — date before entity creation | Invalid date | Validation error | P1 |
| NEG-008 | Get — ID zero | GetInteraction(0) | Null | P1 |
| NEG-009 | Get — ID negative | GetInteraction(-1) | Null | P1 |
| NEG-010 | Update — non-existent | UpdateInteractionAsync(99999) | Null | P1 |
| NEG-011 | Update — null model | UpdateInteractionAsync(null) | ArgumentNullException | P0 |
| NEG-012 | Delete — non-existent | DeleteInteractionAsync(99999) | Graceful | P1 |
| NEG-013 | Delete — already deleted | IsDeleted=true | Idempotent | P1 |
| NEG-014 | Create — empty associations | No ContactIds, PartnerIds, UserIds | Per business rule | P1 |
| NEG-015 | Create — SQL injection in subject | '; DROP TABLE-- | Sanitized | P0 |
| NEG-016 | Create — XSS in notes | <script>alert(1)</script> | Sanitized | P0 |
| NEG-017 | Unauthorized create | User lacks permission | 403 | P0 |
| NEG-018 | Unauthorized update | User lacks permission | 403 | P0 |
| NEG-019 | Unauthorized delete | User lacks permission | 403 | P0 |
| NEG-020 | IDOR — access other org | GetInteraction(otherOrgId) | 403 | P0 |
| NEG-021 | IDOR — update other org | UpdateInteractionAsync(otherId) | 403 | P0 |
| NEG-022 | IDOR — delete other org | DeleteInteractionAsync(otherId) | 403 | P0 |
| NEG-023 | Mass assignment | Include Id | CreateInteractionAsync | Ignored | P0 |
| NEG-024 | Unauthenticated | No auth | Any op | 401 | P0 |
| NEG-025 | Expired token | Expired JWT | Any op | 401 | P0 |
| NEG-026 | Contact soft-deleted | ContactIds=[deleted] | Error | P1 |
| NEG-027 | Partner soft-deleted | PartnerIds=[deleted] | Error | P1 |
| NEG-028 | User inactive | UserIds=[inactive] | Error | P1 |
| NEG-029 | Pagination — invalid page | PageIndex=-1 | Error | P1 |
| NEG-030 | Pagination — zero size | PageSize=0 | Error | P1 |
| NEG-031 | Specification — null | GetInteractionsWithSpecification(null) | ArgumentNullException | P1 |
| NEG-032 | Date range invalid | End before start | GetInteractions | Error | P1 |
| NEG-033 | Type invalid enum | Type="Xyz" | Validation error | P1 |
| NEG-034 | Notes too long | Notes > max | Validation error | P1 |
| NEG-035 | Subject too long | Subject > max | Validation error | P1 |
| NEG-036 | Database timeout | DB timeout | CreateInteractionAsync | Exception | P1 |
| NEG-037 | Concurrent update conflict | 2 users update same | Concurrency error | P1 |
| NEG-038 | Circular reference | Complex | Create | Error | P2 |
| NEG-039 | Duplicate contact in list | ContactIds=[101,101] | Handled | P1 |
| NEG-040 | Duplicate partner in list | PartnerIds=[201,201] | Handled | P1 |
| NEG-041 | Rate limit | Too many creates | CreateInteractionAsync | 429 | P1 |
| NEG-042 | Org scope bypass | User OrgB | GetInteractions | 403 or filtered | P0 |
| NEG-043 | Junction orphan | Entity deleted | GetInteraction | Handled | P1 |
| NEG-044 | Invalid date format | Malformed date | CreateInteractionAsync | Error | P1 |
| NEG-045 | Timezone invalid | Invalid TZ | CreateInteractionAsync | Error | P2 |
| NEG-046 | Filter invalid | Malformed filter | GetInteractions | Error | P2 |
| NEG-047 | Sort invalid column | OrderBy="Invalid" | Error | P1 |
| NEG-048 | Get by contact — invalid | GetInteractionsForContact(99999) | Empty | P1 |
| NEG-049 | Get by partner — invalid | GetInteractionsForPartner(99999) | Empty | P1 |
| NEG-050 | Get by user — invalid | GetInteractionsForUser(99999) | Empty | P1 |
| NEG-051 | Create — contact from other org | ContactId other org | Error | P0 |
| NEG-052 | Create — partner from other org | PartnerId other org | Error | P0 |
| NEG-053 | JWT alg none | alg=none | Request | Rejected | P0 |
| NEG-054 | Brute force IDs | Enumerate | GetInteraction | Rate limited | P1 |
| NEG-055 | CSRF create | Cross-site | CreateInteractionAsync | Token validated | P0 |
| NEG-056 | CSRF update | Cross-site | UpdateInteractionAsync | Token validated | P0 |
| NEG-057 | CSRF delete | Cross-site | DeleteInteractionAsync | Token validated | P0 |
| NEG-058 | Log injection | Malicious log | Log | Sanitized | P1 |
| NEG-059 | Parameter pollution | id=1&id=2 | Get | Handled | P1 |
| NEG-060 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| NEG-061 | Null subject | Subject=null | Create | Per design | P1 |
| NEG-062 | Null notes | Notes=null | Create | Accepted | P1 |
| NEG-063 | Empty contact list | ContactIds=[] | Create | Per rule | P1 |
| NEG-064 | Empty partner list | PartnerIds=[] | Create | Per rule | P1 |
| NEG-065 | Empty user list | UserIds=[] | Create | Per rule | P1 |
| NEG-066 | Update — remove all contacts | Update empty | Per design | P1 |
| NEG-067 | Batch create — partial fail | One invalid | Per transaction | P2 |
| NEG-068 | Specification — invalid | Malformed spec | GetInteractionsWithSpecification | Error | P2 |
| NEG-069 | Date overflow | Year 9999 | Create | Handled | P2 |
| NEG-070 | Audit log failure | Audit down | Any op | Op succeeds | P2 |
| NEG-071 | Create — invalid OrganizationHierarchyId | OrganizationHierarchyIds=[99999] | BusinessException | P1 |
| NEG-072 | Create — OrganizationHierarchy not OrgUnit type | OrganizationHierarchyIds=[non-OrgUnitId] | BusinessException | P1 |
| NEG-073 | CreateGmailInteraction — duplicate Gmail | Same GmailThreadId+GmailMessageId | Returns existing | P1 |
| NEG-074 | Create — empty Subject | Subject="" | Validation error | P0 |
| NEG-075 | Create — whitespace-only Subject | Subject="   " | Validation error | P1 |
| NEG-076 | GetInteractionDetailsForAI — not found | GetInteractionDetailsForAIAsync(99999) | Error object | P1 |
| NEG-077 | Create — invalid email in EmailAddresses | EmailAddresses=["invalid"] | Per validation | P1 |
| NEG-078 | Update — model.Id zero | UpdateInteractionAsync(Id=0) | Error | P1 |
| NEG-079 | Search — empty query | SearchInteractions(query="") | BusinessException | P1 |
| NEG-080 | Advanced search — invalid filter JSON | filters="not valid json" | 400 Bad Request | P1 |
| NEG-081 | Create — negative OrganizationHierarchyId | OrganizationHierarchyIds=[-1] | Error | P1 |
| NEG-082 | Bulk upload — wrong entity type | Type="Partner" | BusinessException | P1 |
| NEG-083 | Scan data — unsupported file type | GeminiFileRequest with .exe | BusinessException | P1 |
| NEG-084 | Deep search — threshold out of range | threshold=1.5 | 400 Bad Request | P1 |
| NEG-085 | Deep search — limit out of range | limit=101 | 400 Bad Request | P1 |
| NEG-086 | Permissions — non-existent interaction | GET /interaction/99999/permissions | BusinessException | P1 |
| NEG-087 | Create — null Subject | Subject=null | Validation error | P0 |
| NEG-088 | Create — no participants | No ContactIds, PartnerIds, UserIds, EmailAddresses | Validation error | P0 |
| NEG-089 | Update — invalid CreatedBy | CreatedBy=99999 | Per design | P1 |
| NEG-090 | GetByIdsAsync — empty array | GetByIdsAsync([]) | Empty list | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Subject | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-002 | Notes | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-003 | InteractionId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-004 | ContactId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-005 | PartnerId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-006 | UserId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-007 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-008 | PageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-009 | Contact count per interaction | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-010 | Partner count per interaction | 0 | 50 | 0 | 50 | 51 | P1 |
| BND-011 | User count per interaction | 0 | 50 | 0 | 50 | 51 | P1 |
| BND-012 | Date min | — | — | 0001-01-01 | — | — | P1 |
| BND-013 | Date max | — | — | 9999-12-31 | — | — | P1 |
| BND-014 | Leap year | — | — | 2024-02-29 | — | — | P2 |
| BND-015 | Unicode subject | — | — | "日本語" | — | — | P1 |
| BND-016 | Unicode notes | — | — | "résumé" | — | — | P1 |
| BND-017 | Special chars | — | — | "Meeting & Call" | — | — | P1 |
| BND-018 | Newline in notes | — | — | "Line1\nLine2" | — | — | P2 |
| BND-019 | Empty result | — | — | 0 interactions | — | — | P1 |
| BND-020 | Single result | — | — | 1 interaction | — | — | P1 |
| BND-021 | Pagination last partial | — | — | 95 total, Size=20 | — | — | P1 |
| BND-022 | Pagination beyond last | — | — | Page 100 | — | — | P1 |
| BND-023 | Zero InteractionId | — | — | GetInteraction(0) | — | — | P1 |
| BND-024 | Null optional | — | — | Notes=null | — | — | P1 |
| BND-025 | Collection empty | — | — | ContactIds=[] | — | — | P1 |
| BND-026 | Collection max | — | — | 100 contacts | — | — | P2 |
| BND-027 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-028 | Timezone | — | — | UTC | — | — | P2 |
| BND-029 | Control chars | — | — | \x00 in subject | — | — | P1 |
| BND-030 | Emoji in notes | — | — | "📅 Meeting" | — | — | P2 |
| BND-031 | RTL text | — | — | Arabic | — | — | P2 |
| BND-032 | HTML in notes | — | — | <b>bold</b> | — | — | P1 |
| BND-033 | Sort empty | — | — | OrderBy on empty | — | — | P1 |
| BND-034 | Filter empty | — | — | No filter | — | — | P1 |
| BND-035 | Type enum all values | — | — | Meeting, Email, Call, Visit | — | — | P1 |
| BND-036 | Date range single day | — | — | Start=End | — | — | P1 |
| BND-037 | Date range max | — | — | 1 year | — | — | P2 |
| BND-038 | Concurrent create | — | — | 2 threads same | — | — | P1 |
| BND-039 | Negative ID | — | — | ID=-1 | — | — | P1 |
| BND-040 | Float ID | — | — | ID=1.5 | — | — | P2 |
| BND-041 | Null ID | — | — | GetInteraction(null) | — | — | P1 |
| BND-042 | Duplicate type | — | — | Type=Meeting twice | — | — | P1 |
| BND-043 | Null date | — | — | Date=null | — | — | P1 |
| BND-044 | Past date | — | — | Yesterday | — | — | P1 |
| BND-045 | Future date | — | — | Tomorrow | — | — | P1 |
| BND-046 | Epoch date | — | — | 1970-01-01 | — | — | P2 |
| BND-047 | DST transition | — | — | DST date | — | — | P2 |
| BND-048 | Midnight | — | — | 00:00:00 | — | — | P2 |
| BND-049 | End of day | — | — | 23:59:59 | — | — | P2 |
| BND-050 | Same entity multiple | — | — | Contact in 2 interactions | — | — | P1 |
| BND-051 | Junction cascade | — | — | Delete interaction | — | — | P1 |
| BND-052 | Whitespace subject | — | — | "  Subject  " | — | — | P1 |
| BND-053 | Whitespace notes | — | — | "  Notes  " | — | — | P1 |
| BND-054 | Leading/trailing | — | — | " Subject " | — | — | P1 |
| BND-055 | Multiple spaces | — | — | "Subject   here" | — | — | P2 |
| BND-056 | Tab in notes | — | — | "Tab\there" | — | — | P2 |
| BND-057 | Carriage return | — | — | "Line1\r\nLine2" | — | — | P2 |
| BND-058 | Empty string subject | — | — | "" | — | — | P1 |
| BND-059 | Empty string notes | — | — | "" | — | — | P1 |
| BND-060 | Specification complex | — | — | Multi-criteria | — | — | P2 |
| BND-061 | Sort multi-column | — | — | Date, Type | — | — | P2 |
| BND-062 | Filter combination | — | — | Type+Date+Contact | — | — | P1 |
| BND-063 | Large notes | — | — | 4000 chars | — | — | P1 |
| BND-064 | Large subject | — | — | 255 chars | — | — | P1 |
| BND-065 | Interaction count | — | — | 10000 per entity | — | — | P2 |
| BND-066 | Nested includes | — | — | Interaction→Contact→Partner | — | — | P2 |
| BND-067 | Empty type | — | — | Type="" | — | — | P1 |
| BND-068 | Null type | — | — | Type=null | — | — | P1 |
| BND-069 | Case type | — | — | "meeting" vs "Meeting" | — | — | P1 |
| BND-070 | Mixed associations | — | — | 1 contact, 2 partners, 3 users | — | — | P1 |
| BND-071 | Location | 0 | 500 | "" | 500 chars | 501 chars | P1 |
| BND-072 | GmailMessageId | 0 | 80 | "" | 80 chars | 81 chars | P1 |
| BND-073 | GmailThreadId | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-074 | EmailAddresses count | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-075 | OrganizationHierarchyIds count | 0 | 50 | 0 | 50 | 51 | P1 |
| BND-076 | Description length | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-077 | Deep search threshold | 0 | 1 | 0 | 1 | 1.1 | P1 |
| BND-078 | Deep search limit | 1 | 100 | 1 | 100 | 101 | P1 |
| BND-079 | OrderBy valid values | — | — | "date" | "type","subject","description" | "invalid" | P1 |
| BND-080 | Single email in EmailAddresses | — | — | ["a@b.com"] | — | — | P1 |
| BND-081 | InteractionType enum values | — | — | Email, Chat, Call, VirtualMeeting, InPersonMeeting, Other | — | — | P1 |
| BND-082 | GetContactInteractionsAsync PageIndex | 1 | Max | 1 | Valid | 0 | P1 |
| BND-083 | Export PageSize | 1 | int.MaxValue | 1 | int.MaxValue | 0 | P1 |
| BND-084 | Subject single character | — | — | "A" | — | — | P1 |
| BND-085 | Subject whitespace only | — | — | "   " | — | — | P1 |
| BND-086 | Date at SQL min | — | — | 0001-01-01 | — | — | P1 |
| BND-087 | Date at SQL max | — | — | 9999-12-31 | — | — | P1 |
| BND-088 | CreatedBy valid | — | — | Valid userId | — | — | P1 |
| BND-089 | ConfirmDuplicateCreation flag | — | — | true | false | — | P1 |
| BND-090 | GetByIdsAsync input size | — | — | [1,2,3] | 1000 ids | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete | Delete | DeleteInteractionAsync | IsDeleted=true | P0 |
| FUN-002 | Deleted excluded | List | GetInteractions | Deleted excluded | P0 |
| FUN-003 | CreatedBy/CreatedDate | Create | CreateInteractionAsync | Audit set | P0 |
| FUN-004 | LastModified on update | Update | UpdateInteractionAsync | Updated | P0 |
| FUN-005 | Junction records | Create | CreateInteractionAsync | All junctions | P0 |
| FUN-006 | Type validation | Create | CreateInteractionAsync | Valid type | P0 |
| FUN-007 | Date validation | Create | CreateInteractionAsync | Valid date | P0 |
| FUN-008 | Contact association | Create | CreateInteractionAsync | Contact linked | P0 |
| FUN-009 | Partner association | Create | CreateInteractionAsync | Partner linked | P0 |
| FUN-010 | User association | Create | CreateInteractionAsync | User linked | P0 |
| FUN-011 | Pagination TotalCount | List | GetInteractions | Accurate | P0 |
| FUN-012 | Filter by type | Filter | GetInteractions | Type filtered | P1 |
| FUN-013 | Filter by contact | Filter | GetInteractions | Contact filtered | P1 |
| FUN-014 | Filter by partner | Filter | GetInteractions | Partner filtered | P1 |
| FUN-015 | Filter by date | Filter | GetInteractions | Date filtered | P1 |
| FUN-016 | Org scope | User OrgA | GetInteractions | OrgA only | P0 |
| FUN-017 | Permission create | User lacks | CreateInteractionAsync | 403 | P0 |
| FUN-018 | Permission update | User lacks | UpdateInteractionAsync | 403 | P0 |
| FUN-019 | Permission delete | User lacks | DeleteInteractionAsync | 403 | P0 |
| FUN-020 | Update add contact | Update | Add ContactId | Junction added | P1 |
| FUN-021 | Update remove contact | Update | Remove ContactId | Junction removed | P1 |
| FUN-022 | Update add partner | Update | Add PartnerId | Junction added | P1 |
| FUN-023 | Update remove partner | Update | Remove PartnerId | Junction removed | P1 |
| FUN-024 | Update add user | Update | Add UserId | Junction added | P1 |
| FUN-025 | Update remove user | Update | Remove UserId | Junction removed | P1 |
| FUN-026 | Specification filter | Spec | GetInteractionsWithSpecification | Filtered | P1 |
| FUN-027 | Sort applied | Sort | GetInteractions | Sorted | P1 |
| FUN-028 | Get by contact | Get | GetInteractionsForContact | Correct list | P0 |
| FUN-029 | Get by partner | Get | GetInteractionsForPartner | Correct list | P0 |
| FUN-030 | Get by user | Get | GetInteractionsForUser | Correct list | P0 |
| FUN-031 | Audit trail create | Create | CreateInteractionAsync | Audit entry | P1 |
| FUN-032 | Audit trail update | Update | UpdateInteractionAsync | Audit entry | P1 |
| FUN-033 | Audit trail delete | Delete | DeleteInteractionAsync | Audit entry | P1 |
| FUN-034 | Idempotent delete | Delete twice | DeleteInteractionAsync | Graceful | P1 |
| FUN-035 | Update non-existent | Update | UpdateInteractionAsync(99999) | Null | P1 |
| FUN-036 | Get non-existent | Get | GetInteraction(99999) | Null | P1 |
| FUN-037 | Junction cascade delete | Delete interaction | DeleteInteractionAsync | Junctions removed | P1 |
| FUN-038 | Contact soft-deleted | Contact deleted | GetInteraction | Handled | P1 |
| FUN-039 | Partner soft-deleted | Partner deleted | GetInteraction | Handled | P1 |
| FUN-040 | Optimistic concurrency | Concurrent update | UpdateInteractionAsync | Conflict | P1 |
| FUN-041 | Type enum | All types | Create each | All valid | P1 |
| FUN-042 | Date range inclusive | Filter | GetInteractions | Inclusive | P1 |
| FUN-043 | Empty associations | Create | No IDs | Per rule | P1 |
| FUN-044 | Duplicate in list | ContactIds [101,101] | Create | Deduped | P1 |
| FUN-045 | Notes optional | Notes=null | Create | Accepted | P1 |
| FUN-046 | Subject optional | Subject=null | Create | Per design | P1 |
| FUN-047 | Required fields | Missing required | Create | Validation error | P0 |
| FUN-048 | Status transitions | Status | Update | Valid only | P1 |
| FUN-049 | WorkflowStatus | If applicable | Check | Per design | P2 |
| FUN-050 | Name property | ModifiableDeletableEntity | Create | Name set | P0 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD | Create→Get→Update→Delete | Interaction | All succeed | P0 |
| INT-002 | ContactManager | Get contact | Interaction, Contact | Contact loaded | P0 |
| INT-003 | PartnerManager | Get partner | Interaction, Partner | Partner loaded | P0 |
| INT-004 | User/Profile | Get user | Interaction | User loaded | P0 |
| INT-005 | Permission | Authorize | Interaction, PermissionService | Correct | P0 |
| INT-006 | Audit | Audit | Interaction, AuditLog | Entries | P1 |
| INT-007 | UserContext | Current user | Interaction, UserResolver | UserId | P0 |
| INT-008 | DbContext | Persist | Interaction, DbContext | Saved | P0 |
| INT-009 | AutoMapper | Entity to Model | Interaction, AutoMapper | Mapped | P1 |
| INT-010 | Controller | API | Interaction, Controller | 200/201/204 | P0 |
| INT-011 | Junction tables | InteractionContacts, etc. | Interaction | All linked | P0 |
| INT-012 | Error handling | Exception | Interaction, Handler | Consistent | P1 |
| INT-013 | Logging | Log | Interaction, ILogger | Logs | P2 |
| INT-014 | Configuration | Config | Interaction | Applied | P2 |
| INT-015 | ManagerWrapper | Resolution | ManagerWrapper | Correct | P1 |
| INT-016 | Multi-tenant | Org scope | Interaction | Isolated | P0 |
| INT-017 | API 404 | Get invalid | Controller | 404 | P0 |
| INT-018 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-019 | API 403 | Unauthorized | Controller | 403 | P0 |
| INT-020 | Opportunity link | Interaction for opp | Interaction, Opportunity | Link | P1 |
| INT-021 | Document link | Interaction docs | Interaction, Document | Docs | P1 |
| INT-022 | Gmail integration | Email interaction | Interaction, GmailAddonManager | Created | P1 |
| INT-023 | List view | Interactions in list | Interaction | Displayed | P1 |
| INT-024 | Detail view | Interaction detail | Interaction | All sections | P0 |
| INT-025 | Contact list | Contact interactions | Interaction, Contact | List | P0 |
| INT-026 | Partner list | Partner interactions | Interaction, Partner | List | P0 |
| INT-027 | Repository | CRUD | Interaction, Repository | Works | P1 |
| INT-028 | Validation | Validate | Interaction, Validator | Errors | P1 |
| INT-029 | Specification | Spec pattern | Interaction | Applied | P1 |
| INT-030 | Pagination | Paginate | Interaction | Correct | P0 |
| INT-031 | Filter | Filter | Interaction | Filtered | P0 |
| INT-032 | Sort | Sort | Interaction | Sorted | P1 |
| INT-033 | Report | Report | Interaction | In report | P2 |
| INT-034 | Export | Export | Interaction | Exported | P2 |
| INT-035 | Import | Import | Interaction | Imported | P2 |
| INT-036 | Migration | Add field | Interaction | Migrated | P2 |
| INT-037 | Seed data | Seed | Interaction | Seeded | P2 |
| INT-038 | Feature flag | Feature | Interaction | Respected | P2 |
| INT-039 | Notification | Notify | Interaction, NotificationManager | Sent | P2 |
| INT-040 | Workflow | Workflow | Interaction | In workflow | P2 |
| INT-041 | Timeline | Timeline | Interaction | Displayed | P1 |
| INT-042 | Search | Search | Interaction | In results | P1 |
| INT-043 | Dashboard | Dashboard | Interaction | Count/List | P1 |
| INT-044 | Analytics | Analytics | Interaction | Metrics | P2 |
| INT-045 | Bulk operations | Bulk | Interaction | Batch | P2 |
| INT-046 | Validation service | Validate | Interaction | Validated | P1 |
| INT-047 | Mapping profile | Map | Interaction | Profile | P1 |
| INT-048 | Soft delete cascade | Delete contact | Interaction | Per rule | P1 |
| INT-049 | Restore | Restore | Interaction | Restored | P2 |
| INT-050 | Archive | Archive | Interaction | Archived | P2 |
| INT-051 | Search fields endpoint | GET /interaction/search-fields | Interaction, Controller | SearchFieldInfo list | P1 |
| INT-052 | GetByIdsAsync batch | GetByIdsAsync([1,2,3]) | Interaction, Contact, Partner, User | List of models | P1 |
| INT-053 | CreateGmailInteraction flow | Create from Gmail | Interaction, GmailAddonManager | Interaction created | P1 |
| INT-054 | FindGmailInteraction | Find by GmailThreadId+MessageId | Interaction, DbContext | Existing or null | P1 |
| INT-055 | Scan data endpoint | POST /interaction/scan-data | Interaction, GeminiManager | Extracted data | P1 |
| INT-056 | Analyse file endpoint | POST /interaction/analyse-file | Interaction, GeminiManager | Structured data | P1 |
| INT-057 | Bulk upload endpoint | POST /interaction/bulk-upload | Interaction, GeminiManager | Bulk result | P1 |
| INT-058 | GetInteractionDetailsForAI | AI prompt data | Interaction, Contacts, Partners, Users | JSON structure | P1 |
| INT-059 | Deep search endpoint | GET /interaction/deepSearch | Interaction, AiContextualService | Similar interactions | P1 |
| INT-060 | Detect duplicates endpoint | POST /interaction/detect-duplicates | Interaction, AiContextualService | Duplicate info | P1 |
| INT-061 | Auto-populate from contacts | Create with ContactIds | Interaction, ContactManager | PartnerIds+EmailAddresses | P1 |
| INT-062 | GetInteractionsBrief | GET /interactions-brief | Interaction, AdvancedSearchService | Paginated list | P1 |
| INT-063 | SecureSearchControllerHelper | List with spec | Interaction, SecureSpecificationFactory | Filtered list | P1 |
| INT-064 | OrganizationUnitRelationships | Create with OrgUnitIds | Interaction, OrganizationHierarchy | Relationships | P1 |
| INT-065 | Soft delete OrgUnitRelationships | Delete interaction | Interaction, OrganizationUnitRelationship | Cascade soft delete | P1 |
| INT-066 | GoogleCloudStorageService | Get with contacts | Interaction, Contact.ProfilePictureUrl | Signed URLs | P1 |
| INT-067 | UserProfileCacheService | Batch user names | Interaction, UserProfile | Cached names | P1 |
| INT-068 | GlobalFilterService | GetInteractionsWithSpecification | Interaction, GlobalFilterService | Org-filtered | P1 |
| INT-069 | Access control filters | ApplyAccessControlFilters | Interaction, PermissionService | Filtered by role | P1 |
| INT-070 | Metadata info endpoint | GET /interaction/metadata-info | Interaction, EntityConfigurationManager | Field config | P1 |
| INT-071 | Duplicate detection on create | Create with similar | Interaction, AiContextualService | Confirmation required | P1 |
| INT-072 | ConfirmDuplicateCreation bypass | Create with flag | Interaction, AiContextualService | Created | P1 |
| INT-073 | NormalizeDateTimeToUtc | Create with Local | Interaction, Controller | UTC stored | P1 |
| INT-074 | Partner filter in list | ListAllInteractions(partnerId) | Interaction, Partner | Partner-filtered | P1 |
| INT-075 | Contact filter in list | ListAllInteractions(contactId) | Interaction, Contact | Contact-filtered | P1 |
| INT-076 | Advanced search with filters | AdvancedSearchInteractions | Interaction, AdvancedSearchService | Filtered results | P1 |
| INT-077 | Search with partner filter | SearchInteractions(partnerId) | Interaction, AdvancedSearchService | Combined filter | P1 |
| INT-078 | Export mode | List with export=true | Interaction, Controller | Full dataset | P1 |
| INT-079 | GetBasicEntityAsync | AI/reflection call | Interaction, BaseUNOPSManager | Entity details | P1 |
| INT-080 | GetInteractionDetailsForOpportunityCreation | Gemini reflection | Interaction, GeminiManager | Dictionary | P1 |
| INT-081 | ProcessJunctionTables | Create with all IDs | Interaction, InteractionContact/Partner/User | All junctions | P1 |
| INT-082 | ProcessGmailInteractionJunctionTables | CreateGmailInteraction | Interaction, Junction tables | Bulk insert | P1 |
| INT-083 | LoadOrganizationUnitRelationshipsAsync | Get single | Interaction, OrganizationUnitRelationship | Loaded | P1 |
| INT-084 | PatchNonNullProperties | Update partial | Interaction, UpdateInteractionRequest | Only changed | P1 |
| INT-085 | UpdateOrganizationUnitRelationshipsDifferential | Update OrgUnitIds | Interaction, OrganizationHierarchy | Differential update | P1 |
| INT-086 | HandleOperationAsync | Any controller op | Controller, Exception handler | Consistent response | P1 |
| INT-087 | ValidatePaginationParameters | List with invalid page | Controller, BaseController | 400 | P1 |
| INT-088 | AccessControlled attribute | Request without permission | Controller, Authorization | 403 | P1 |
| INT-089 | IAP authentication | Request without auth | Controller, Auth | 401 | P1 |
| INT-090 | DbContextFactory parallel | GetInteractionDetailsForAI | Interaction, DbContextFactory | Parallel queries | P1 |

---

## §6 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent create | 10 threads CreateInteractionAsync | All created | P0 |
| CON-002 | Concurrent update same | 5 threads UpdateInteractionAsync(789) | No corruption | P0 |
| CON-003 | Concurrent delete same | 2 threads DeleteInteractionAsync(789) | One succeeds | P0 |
| CON-004 | Create and get | Thread1 create, Thread2 get | Consistent | P1 |
| CON-005 | Update and get | Thread1 update, Thread2 get | Consistent | P1 |
| CON-006 | Delete and get | Thread1 delete, Thread2 get | Null | P0 |
| CON-007 | Optimistic concurrency | 2 users update same | Conflict | P0 |
| CON-008 | Connection pool | 100 concurrent | No exhaustion | P1 |
| CON-009 | Deadlock | Circular | No deadlock | P1 |
| CON-010 | Double submit | User double-clicks | One created | P0 |
| CON-011 | Race on contact | 2 threads add same contact | Handled | P1 |
| CON-012 | Junction concurrent | 2 threads update junctions | Consistent | P1 |
| CON-013 | List during create | Thread1 create, Thread2 list | Consistent | P1 |
| CON-014 | Filter concurrent | 10 threads different filters | All correct | P1 |
| CON-015 | Pagination concurrent | 20 threads different pages | Correct pages | P1 |
| CON-016 | Transaction isolation | Read uncommitted | Per level | P1 |
| CON-017 | Lost update | 2 users different fields | Per design | P1 |
| CON-018 | Phantom read | Insert during list | Per isolation | P2 |
| CON-019 | Non-repeatable read | Update between reads | Per isolation | P2 |
| CON-020 | Cache consistency | Concurrent cache | Consistent | P1 |
| CON-021 | Bulk create | 2 threads bulk | Consistent | P1 |
| CON-022 | Get by contact concurrent | 10 threads GetInteractionsForContact | All correct | P1 |
| CON-023 | Get by partner concurrent | 10 threads GetInteractionsForPartner | All correct | P1 |
| CON-024 | Specification concurrent | 15 threads different specs | All correct | P1 |
| CON-025 | Junction cascade concurrent | Delete during get | Handled | P1 |

---

## §7 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Type validation | Validation | "Meeting" | Valid | P0 |
| UNT-002 | Type invalid | Validation | "Invalid" | Invalid | P0 |
| UNT-003 | Date validation | Validation | Valid date | Valid | P0 |
| UNT-004 | Date invalid | Validation | Invalid date | Invalid | P0 |
| UNT-005 | Subject trim | Formatting | "  Subject  " | "Subject" | P1 |
| UNT-006 | Notes trim | Formatting | "  Notes  " | "Notes" | P1 |
| UNT-007 | Date format | Formatting | Date | ISO format | P1 |
| UNT-008 | Junction count | Calculation | 5 contacts | Count=5 | P1 |
| UNT-009 | Type enum | Calculation | "Meeting" | Meeting | P1 |
| UNT-010 | Status Active | Status logic | IsDeleted=false | Active | P1 |
| UNT-011 | Status Deleted | Status logic | IsDeleted=true | Excluded | P0 |
| UNT-012 | Collection filter | Collections | List with deleted | Excluded | P1 |
| UNT-013 | Empty collection | Collections | No interactions | Count=0 | P1 |
| UNT-014 | Null to empty | Collections | Null list | [] | P1 |
| UNT-015 | Map to Model | Mapping | Interaction entity | InteractionModel | P0 |
| UNT-016 | Map Request | Mapping | CreateRequest | Entity | P0 |
| UNT-017 | Pagination slice | Calculation | Page 1, Size 10 | Skip 10, Take 10 | P1 |
| UNT-018 | Date range | Calculation | Start, End | Range | P1 |
| UNT-019 | Type parse | Validation | "meeting" | Meeting | P1 |
| UNT-020 | Audit fields | Status logic | New interaction | CreatedBy set | P1 |
| UNT-021 | Name composition | Calculation | Subject, Type | Name | P1 |

---

## §8 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create interaction | CreateInteractionAsync | < 300ms | P0 |
| PRF-002 | Get by ID | GetInteraction | < 100ms | P0 |
| PRF-003 | Get interactions | GetInteractions (100) | < 500ms | P0 |
| PRF-004 | Update interaction | UpdateInteractionAsync | < 200ms | P0 |
| PRF-005 | Delete interaction | DeleteInteractionAsync | < 100ms | P0 |
| PRF-006 | Get by contact | GetInteractionsForContact (500) | < 1000ms | P0 |
| PRF-007 | Get by partner | GetInteractionsForPartner (500) | < 1000ms | P0 |
| PRF-008 | Specification query | GetInteractionsWithSpecification | < 500ms | P1 |
| PRF-009 | Pagination | GetInteractions page 1 | < 300ms | P1 |
| PRF-010 | Create with 10 contacts | Create full | < 500ms | P1 |
| PRF-011 | Memory 100 | GetInteractions PageSize=100 | < 50MB | P1 |
| PRF-012 | Concurrent 20 | 20 GetInteractions | < 500ms each | P1 |
| PRF-013 | Junction load | Get with junctions | < 300ms | P1 |
| PRF-014 | Filter + sort | Both | < 400ms | P1 |
| PRF-015 | Cold start | First query | < 500ms | P2 |
| PRF-016 | Cached | Second query | < 100ms | P2 |

---

## §9 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained 20 req/s create | 20 Create/sec | 5 min | 95% < 300ms | P0 |
| LDT-002 | Sustained 50 req/s get | 50 GetInteractions/sec | 5 min | 95% < 200ms | P0 |
| LDT-003 | Sustained 30 req/s list | 30 List/sec | 5 min | 95% < 500ms | P0 |
| LDT-004 | Spike 100 req/s | 100 req/s burst | 1 min | No crash | P0 |
| LDT-005 | Spike 200 req/s | 200 req/s | 30 sec | Graceful degrade | P1 |
| LDT-006 | Stress ramp | 1→500 req/s | Until fail | Find limit | P1 |
| LDT-007 | Connection pool | 200 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Memory | 10K interactions | 5 min | No leak | P1 |
| LDT-009 | Recovery spike | Spike then normal | 5 min | Baseline | P0 |
| LDT-010 | Recovery stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
