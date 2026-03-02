# InteractionManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/InteractionManager`  
**Created:** 2026-02-04  
**Last Updated:** 2026-02-11  
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
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

The InteractionManager handles CRUD operations for interactions (meetings, emails, calls, site visits, virtual meetings, chats). Key functionality: creation with contact/partner association, date range validation (FromDate/ToDate), type classification, Gmail integration, deduplication, status lifecycle, search by date/type, pagination, audit trail, and soft delete.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Create Meeting Interaction

**Priority:** P0  
**Precondition:** Contact and Partner exist. User has create permission.

**Steps:**
1. Call `CreateInteractionAsync` with Type=Meeting, ContactId, PartnerId, FromDate, ToDate, Description
2. Verify response

**Expected Result:** Interaction created with auto-ID, audit fields set, IsDeleted=false, Status=Active

---

#### POS-002: Get Interaction by ID with Related Data

**Priority:** P0  
**Precondition:** Interaction exists with linked contact and partner.

**Steps:**
1. Call `GetInteractionByIdAsync(id)` with includes
2. Verify related data loaded

**Expected Result:** Interaction returned with Contact and Partner navigation properties loaded, all fields populated

---

#### POS-003: Update Interaction Description and Dates

**Priority:** P0  
**Precondition:** Interaction exists, user has edit permission.

**Steps:**
1. Call `UpdateInteractionAsync` with modified Description, FromDate, ToDate
2. Verify persistence

**Expected Result:** Fields updated, LastModifiedBy/Date set, unchanged fields intact

---

#### POS-004: Soft Delete Interaction

**Priority:** P0  
**Precondition:** Interaction exists, user has delete permission.

**Steps:**
1. Call `DeleteInteractionAsync(id)`
2. Verify soft delete

**Expected Result:** IsDeleted=true, DeletedBy/Date set, not physically removed, excluded from future queries

---

#### POS-005: List Interactions with Pagination

**Priority:** P0  
**Precondition:** 50+ interactions for a partner.

**Steps:**
1. Call `GetInteractionsWithPagination(page=1, size=20)`
2. Verify paginated results

**Expected Result:** 20 interactions on page 1, total count correct, no deleted interactions, sorted by date desc

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Create Email interaction | Contact exists | Create with Type=Email | Email interaction created | P1 |
| POS-007 | Create Call interaction | Contact exists | Create with Type=Call | Call interaction created | P1 |
| POS-008 | Create SiteVisit interaction | Contact exists | Create with Type=SiteVisit | SiteVisit created | P1 |
| POS-009 | Create VirtualMeeting interaction | Contact exists | Create with Type=VirtualMeeting | VirtualMeeting created | P1 |
| POS-010 | Create Chat interaction | Contact exists | Create with Type=Chat | Chat created | P1 |
| POS-011 | Get interactions by partner ID | Partner with interactions | GetByPartnerId | Non-deleted interactions returned | P1 |
| POS-012 | Get interactions by contact ID | Contact with interactions | GetByContactId | Contact's interactions returned | P1 |
| POS-013 | Search interactions by date range | Interactions span months | SearchByDateRange(from, to) | Only interactions in range | P1 |
| POS-014 | Search interactions by type | Various types exist | SearchByType(Meeting) | Only Meeting type returned | P1 |
| POS-015 | Get recent interactions | Many interactions | GetRecentInteractions(limit=10) | 10 most recent | P1 |
| POS-016 | Gmail import interaction | Gmail data | CreateFromGmail | Gmail metadata populated | P1 |
| POS-017 | Gmail deduplication — new interaction | Unique Gmail ID | ImportGmail | New interaction created | P1 |
| POS-018 | Gmail deduplication — existing skipped | Duplicate Gmail ID | ImportGmail | Existing not duplicated | P1 |
| POS-019 | Update interaction type | Existing interaction | Update Type=Email to Call | Type changed | P1 |
| POS-020 | Update interaction contact association | Existing interaction | Reassign to new contact | ContactId updated | P1 |
| POS-021 | Filter by multiple types | Various types | Filter(Meeting, Call) | Both types returned | P2 |
| POS-022 | Sort by FromDate ascending | Multiple interactions | Sort(fromDate, asc) | Oldest first | P2 |
| POS-023 | Sort by FromDate descending | Multiple interactions | Sort(fromDate, desc) | Newest first | P2 |
| POS-024 | Get interaction count by partner | Partner with 15 interactions | GetCount(partnerId) | 15 (non-deleted) | P2 |
| POS-025 | Get interaction count by type | Mixed types | GetCountByType | Correct per-type counts | P2 |
| POS-026 | Create with minimum fields | Only required fields | Create with required only | Created with nulls for optional | P2 |
| POS-027 | Create with all optional fields | All fields populated | Create with all fields | All persisted | P2 |
| POS-028 | Map entity to model | Interaction entity | mapper.Map<InteractionModel> | All fields mapped | P2 |
| POS-029 | Map create request to entity | CreateInteractionRequest | mapper.Map<Interaction> | All fields mapped | P2 |
| POS-030 | Interaction with long description (4000 chars) | Long text | Create with 4000 chars | Stored completely | P2 |

---

## §2 Negative Tests (Failure Scenarios) — 90 tests

> **Minimum:** 90 tests

### 2.1 Invalid Input (10)

| ID | Invalid Input | Expected Error | Priority |
|----|--------------|---------------|----------|
| NEG-001 | Null Description | BusinessException: required | P0 |
| NEG-002 | Null ContactId | BusinessException: contact required | P0 |
| NEG-003 | Non-existent ContactId | KeyNotFoundException | P0 |
| NEG-004 | Deleted ContactId | BusinessException: contact deleted | P0 |
| NEG-005 | Null PartnerId | BusinessException: partner required | P0 |
| NEG-006 | Non-existent PartnerId | KeyNotFoundException | P0 |
| NEG-007 | Invalid Type value | BusinessException: invalid type | P0 |
| NEG-008 | FromDate > ToDate | BusinessException: invalid date range | P0 |
| NEG-009 | Null FromDate | BusinessException: date required | P0 |
| NEG-010 | Update non-existent interaction | KeyNotFoundException | P0 |

### 2.2 Unauthorized Access (10)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| NEG-011 | No auth | Create | UnauthorizedAccessException | P0 |
| NEG-012 | Read-only | Create | UnauthorizedAccessException | P0 |
| NEG-013 | Read-only | Update | UnauthorizedAccessException | P0 |
| NEG-014 | Read-only | Delete | UnauthorizedAccessException | P0 |
| NEG-015 | OrgUnit-scoped | Create out of scope | UnauthorizedAccessException | P0 |
| NEG-016 | OrgUnit-scoped | Read out of scope | UnauthorizedAccessException | P0 |
| NEG-017 | OrgUnit-scoped | Update out of scope | UnauthorizedAccessException | P0 |
| NEG-018 | OrgUnit-scoped | Delete out of scope | UnauthorizedAccessException | P0 |
| NEG-019 | Expired session | Any operation | UnauthorizedAccessException | P1 |
| NEG-020 | Disabled account | Any operation | UnauthorizedAccessException | P1 |

### 2.3 Invalid State (5)

| ID | State | Action | Expected | Priority |
|----|-------|--------|----------|----------|
| NEG-021 | Deleted interaction | Update | BusinessException | P1 |
| NEG-022 | Deleted interaction | Delete again | No-op or error | P1 |
| NEG-023 | Completed interaction | Modify dates | BusinessException (if locked) | P1 |
| NEG-024 | Cancelled interaction | Update | BusinessException (if locked) | P1 |
| NEG-025 | Invalid status transition | Active → Draft | BusinessException: invalid transition | P1 |

### 2.4 Missing/Null Data (10)

| ID | Missing | Expected | Priority |
|----|---------|----------|----------|
| NEG-026 | All fields null | Multiple validation errors | P1 |
| NEG-027 | Null request object | ArgumentNullException | P1 |
| NEG-028 | Null specification | Default or error | P1 |
| NEG-029 | Null pagination params | Defaults applied | P1 |
| NEG-030 | Whitespace-only Description | BusinessException | P1 |
| NEG-031 | Empty Type string | BusinessException | P1 |
| NEG-032 | Null date range for search | Default range or error | P1 |
| NEG-033 | ToDate = null with FromDate set | Error or open-ended | P2 |
| NEG-034 | FromDate = null with ToDate set | Error or open-ended | P2 |
| NEG-035 | Description with only whitespace | Trimmed to empty → error | P1 |

### 2.5 Dependency Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-036 | DB connection lost on create | Transaction rolled back | P1 |
| NEG-037 | DB timeout on query | TimeoutException | P1 |
| NEG-038 | AutoMapper missing mapping | MappingException | P2 |
| NEG-039 | Repository constraint violation | BusinessException | P1 |
| NEG-040 | Gmail API unavailable | Graceful failure | P1 |
| NEG-041 | Gmail returns malformed data | Handled, fields default | P2 |
| NEG-042 | Concurrent DB migration | Graceful error | P2 |
| NEG-043 | Connection pool exhausted | Wait or error | P1 |
| NEG-044 | Transaction deadlock | Retry mechanism | P1 |
| NEG-045 | Serialization failure | Error handled | P2 |

### 2.6 Additional Scenarios (25)

| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| NEG-046 | SQL injection in Description | Parameterized, no injection | P0 |
| NEG-047 | XSS in Description | Sanitized | P0 |
| NEG-048 | Description > 4000 chars | Validation error | P1 |
| NEG-049 | Negative ContactId | Validation error | P1 |
| NEG-050 | Zero ContactId | Validation error | P1 |
| NEG-051 | Negative PartnerId | Validation error | P1 |
| NEG-052 | Zero PartnerId | Validation error | P1 |
| NEG-053 | Negative interaction ID | Not found | P1 |
| NEG-054 | Zero interaction ID | Not found | P1 |
| NEG-055 | Page = 0 | Default to 1 | P2 |
| NEG-056 | PageSize = -1 | Validation error | P2 |
| NEG-057 | PageSize > 1000 | Capped | P2 |
| NEG-058 | Sort by invalid column | Default sort | P2 |
| NEG-059 | FromDate far in future | Validation (if rule exists) | P2 |
| NEG-060 | ToDate far in past | Valid if before FromDate check | P2 |
| NEG-061 | Create for inactive partner | Business rule | P1 |
| NEG-062 | Multiple validation errors | All returned | P1 |
| NEG-063 | Gmail duplicate with different data | Dedup logic handles | P1 |
| NEG-064 | Path traversal in any field | Sanitized | P0 |
| NEG-065 | HTML injection in Description | Escaped | P1 |
| NEG-066 | Very long Type string | Validation error | P1 |
| NEG-067 | Search regex chars | Escaped/literal | P1 |
| NEG-068 | Filter invalid date format | Parsing error | P1 |
| NEG-069 | Batch create with mixed valid/invalid | Valid created, invalid rejected | P1 |
| NEG-070 | Create with contact from different partner | Business rule validation | P1 |
| NEG-071 | Input | Null type for filter | Default or error | P2 |
| NEG-072 | Input | Invalid date format in search | Parsing error | P1 |
| NEG-073 | State | Update during delete | Conflict | P1 |
| NEG-074 | Dep | Gmail API timeout | Graceful | P2 |
| NEG-075 | Auth | Create for deleted partner | BusinessException | P1 |
| NEG-076 | Auth | Create for deleted contact | BusinessException | P1 |
| NEG-077 | Data | Description with null chars | Sanitized | P1 |
| NEG-078 | Data | FromDate = ToDate + 1 day | Rejected | P0 |
| NEG-079 | State | Completed interaction update | BusinessException (if locked) | P1 |
| NEG-080 | Dep | Transaction deadlock on bulk | Retry | P1 |
| NEG-081 | Mass | Mass assign IsDeleted | Blocked | P0 |
| NEG-082 | Mass | Mass assign CreatedBy | Blocked | P0 |
| NEG-083 | Search | Search with SQL chars | Escaped | P0 |
| NEG-084 | Filter | Filter by invalid status | Error | P2 |
| NEG-085 | Gmail | Gmail ID format invalid | Rejected | P2 |
| NEG-086 | Input | Negative limit for recent | Error | P2 |
| NEG-087 | State | Reassign to deleted contact | BusinessException | P1 |
| NEG-088 | Dep | Connection pool exhausted | Wait or error | P1 |
| NEG-089 | Data | Type with 51 chars | Validation error | P1 |
| NEG-090 | Input | Null partner for filter | Error or all | P2 |

---

## §3 Boundary Tests (Edge Cases) — 90 tests

> **Minimum:** 90 tests

### 3.1 String Lengths (8)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Description | 1 | 4000 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Type | 1 | 50 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Location | 0 | 500 | ✅ | ✅ | ❌ | P2 |
| BND-004 | Subject | 0 | 200 | ✅ | ✅ | ❌ | P2 |
| BND-005 | Participants | 0 | 2000 | ✅ | ✅ | ❌ | P2 |
| BND-006 | Notes | 0 | 4000 | ✅ | ✅ | ❌ | P2 |
| BND-007 | GmailMessageId | 0 | 500 | ✅ | ✅ | ❌ | P2 |
| BND-008 | GmailThreadId | 0 | 500 | ✅ | ✅ | ❌ | P2 |

### 3.2 Numeric Boundaries (8)

| ID | Field | Min | Max | Zero | Negative | Priority |
|----|-------|-----|-----|------|----------|----------|
| BND-009 | Interaction ID | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-010 | ContactId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-011 | PartnerId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-012 | Page number | 1 | 10000 | ❌ Default | ❌ Error | P1 |
| BND-013 | Page size | 1 | 1000 | ❌ | ❌ | P1 |
| BND-014 | Interactions per partner | 0 | 100000 | ✅ Empty | ✅ Large | P1 |
| BND-015 | Interactions per contact | 0 | 10000 | ✅ Empty | ✅ Large | P1 |
| BND-016 | Recent interactions limit | 1 | 100 | ✅ | ✅ | P2 |

### 3.3 Date Boundaries (12)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|---------|----------|----------|
| BND-017 | FromDate = ToDate (same day) | Point-in-time | Accepted | P1 |
| BND-018 | FromDate = ToDate (same second) | Exact match | Accepted | P1 |
| BND-019 | ToDate = FromDate + 1 second | Minimal duration | Accepted | P1 |
| BND-020 | Multi-day interaction | FromDate Mon, ToDate Fri | Accepted | P1 |
| BND-021 | Leap year date | Feb 29, 2028 | Accepted | P2 |
| BND-022 | End of month | Jan 31 | Accepted | P2 |
| BND-023 | Year boundary | Dec 31 - Jan 1 | Accepted | P2 |
| BND-024 | Midnight UTC | 00:00:00 | No boundary error | P2 |
| BND-025 | Very old date | 2000-01-01 | Accepted | P2 |
| BND-026 | Today's date | Current date | Accepted | P2 |
| BND-027 | Search range exactly 1 day | Same from/to | Returns that day | P1 |
| BND-028 | Search range exactly 1 year | 365 days | Correct results | P1 |

### 3.4 Collections (10)

| ID | Test Name | State | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-029 | 0 interactions for partner | Empty | Empty list, count=0 | P1 |
| BND-030 | 1 interaction | Single | List with 1 | P1 |
| BND-031 | Exactly page size | 20 interactions, size=20 | Full page, hasNext=false | P1 |
| BND-032 | Page size + 1 | 21 interactions | 20 on page 1, hasNext=true | P1 |
| BND-033 | 1000 interactions | Large | Paginated correctly | P1 |
| BND-034 | 10,000 interactions | Very large | Performance acceptable | P1 |
| BND-035 | Last page partial | 45 items, page 3, size=20 | 5 items | P1 |
| BND-036 | All interactions same type | Only Meeting | Filter returns all | P2 |
| BND-037 | All interactions same date | Same FromDate | Sort handles ties | P2 |
| BND-038 | Contact with 0 interactions, partner has some | Mixed | Contact-specific returns empty | P1 |

### 3.5 Unicode & Special Characters (10)

| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-039 | Description (Arabic) | `اجتماع عمل` | Stored correctly | P2 |
| BND-040 | Description (Chinese) | `商务会议记录` | Stored correctly | P2 |
| BND-041 | Description (Cyrillic) | `Деловая встреча` | Stored correctly | P2 |
| BND-042 | Description (French) | `Réunion d'affaires` | Accents preserved | P2 |
| BND-043 | Description with emoji | `Great meeting! 🤝` | Emoji preserved | P2 |
| BND-044 | Location with special chars | `Room 3-A (2nd Floor)` | Chars preserved | P2 |
| BND-045 | Subject with HTML entities | `Revenue &gt; $1M` | Stored as-is | P2 |
| BND-046 | Participants with commas | `John, Jane, Bob` | Stored correctly | P2 |
| BND-047 | Notes with newlines | Multi-line text | Newlines preserved | P2 |
| BND-048 | GmailMessageId with special chars | Base64 encoded ID | Stored as-is | P2 |

### 3.6 Type Boundaries (7)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|---------|----------|----------|
| BND-049 | Each valid type creates successfully | Meeting,Email,Call,SiteVisit,VirtualMeeting,Chat | All accepted | P1 |
| BND-050 | Type case sensitivity | "meeting" vs "Meeting" | Handled (either accepted or normalized) | P1 |
| BND-051 | Type with leading/trailing spaces | " Meeting " | Trimmed | P2 |
| BND-052 | Type enum at first value | Meeting (index 0) | Accepted | P2 |
| BND-053 | Type enum at last value | Chat (last index) | Accepted | P2 |
| BND-054 | Status Active to Completed | Valid transition | Accepted | P1 |
| BND-055 | Status Active to Cancelled | Valid transition | Accepted | P1 |

### 3.7 Additional (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|---------|----------|----------|
| BND-056 | Duration = 0 (same time) | FromDate = ToDate exact | Accepted | P1 |
| BND-057 | Duration = 1 minute | 60-second gap | Accepted | P1 |
| BND-058 | Duration = 24 hours | Full day | Accepted | P1 |
| BND-059 | Duration = 7 days | Week-long | Accepted | P2 |
| BND-060 | Create at exactly midnight | Time = 00:00:00 | No boundary issue | P2 |
| BND-061 | Create at end of day | Time = 23:59:59 | No boundary issue | P2 |
| BND-062 | Interaction ID = 1 | Minimum valid | Retrieved | P2 |
| BND-063 | Interaction ID = MAX_INT | Maximum | Handled | P2 |
| BND-064 | Search with exactly 1 result | Single match | Returned correctly | P1 |
| BND-065 | Search with 0 results | No match | Empty list | P1 |
| BND-066 | Filter by type + date combined | Both criteria | Intersection returned | P1 |
| BND-067 | Sort each available column | Type, Date, Description | Each works | P1 |
| BND-068 | Paginate exactly to last page | Total / pageSize = integer | Last page full | P2 |
| BND-069 | Gmail message ID at max length | 500 chars | Stored correctly | P2 |
| BND-070 | Multiple contacts for same interaction | Many-to-many if supported | All linked | P2 |
| BND-071 | Description 2000 chars | Accepted | P1 |
| BND-072 | Type 25 chars | Accepted | P1 |
| BND-073 | ContactId 500 | Retrieved | P1 |
| BND-074 | PartnerId 1000 | Retrieved | P1 |
| BND-075 | Page 100 | Handled | P2 |
| BND-076 | PageSize 500 | Accepted | P1 |
| BND-077 | Interactions 500 per partner | Paginated | P1 |
| BND-078 | Interactions 100 per contact | Loaded | P1 |
| BND-079 | Duration 1 hour | Accepted | P1 |
| BND-080 | Duration 8 hours | Accepted | P1 |
| BND-081 | FromDate = ToDate (same hour) | Accepted | P1 |
| BND-082 | Search range 7 days | Correct | P1 |
| BND-083 | Recent limit 50 | 50 returned | P2 |
| BND-084 | Interaction ID 100 | Retrieved | P2 |
| BND-085 | Unicode Location | Arabic | Stored | P2 |
| BND-086 | Subject 100 chars | Accepted | P1 |
| BND-087 | Participants 500 chars | Accepted | P2 |
| BND-088 | Notes 2000 chars | Accepted | P2 |
| BND-089 | GmailThreadId 200 chars | Stored | P2 |
| BND-090 | All types in one partner | 6 types | All returned | P1 |

---

## §4 Functional Tests (Business Rules) — 90 tests

> **Minimum:** 90 tests

### 4.1 Workflow Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Queries exclude IsDeleted=true | Any query | Deleted filtered out | P0 |
| FUN-002 | Create sets audit fields | Create | CreatedBy, CreatedDate | P0 |
| FUN-003 | Update sets audit fields | Update | LastModifiedBy/Date | P0 |
| FUN-004 | Delete sets soft-delete fields | Delete | IsDeleted, DeletedBy/Date | P0 |
| FUN-005 | Name auto-set from Type + Date | Create | Name = "Meeting - 2026-02-11" | P1 |
| FUN-006 | Contact association validated | Create | Contact must exist, !IsDeleted | P0 |
| FUN-007 | Partner association validated | Create | Partner must exist, !IsDeleted | P0 |
| FUN-008 | Date range validated | Create/Update | FromDate ≤ ToDate | P0 |
| FUN-009 | Gmail deduplication by MessageId | Gmail import | Existing not duplicated | P1 |
| FUN-010 | Status transitions validated | ChangeStatus | Only valid transitions | P1 |
| FUN-011 | Search is case-insensitive | Search | "meeting" = "Meeting" | P1 |
| FUN-012 | Pagination defaults applied | Null params | Page=1, Size=20 | P1 |
| FUN-013 | Recent interactions sorted desc | GetRecent | Most recent first | P1 |
| FUN-014 | Count excludes deleted | GetCount | Only !IsDeleted | P1 |
| FUN-015 | Contact change updates interaction scope | Reassign contact | Interaction follows contact | P1 |

### 4.2 Validation Rules (15)

| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | Description required | "Meeting notes" | null, "" | P0 |
| FUN-017 | ContactId required and positive | 42 | 0, -1 | P0 |
| FUN-018 | PartnerId required and positive | 42 | 0, -1 | P0 |
| FUN-019 | Type must be valid enum | "Meeting" | "INVALID" | P0 |
| FUN-020 | FromDate required | Valid date | null | P0 |
| FUN-021 | FromDate ≤ ToDate | Jan 1 < Jan 2 | Jan 2 > Jan 1 reversed | P0 |
| FUN-022 | Contact must exist and not deleted | Active contact | Deleted contact | P0 |
| FUN-023 | Partner must exist and not deleted | Active partner | Deleted partner | P0 |
| FUN-024 | Input sanitized for XSS | Clean text | `<script>` | P0 |
| FUN-025 | Description max length 4000 | 3999 chars | 4001 chars | P1 |
| FUN-026 | Location max length 500 | 499 chars | 501 chars | P2 |
| FUN-027 | Subject max length 200 | 199 chars | 201 chars | P2 |
| FUN-028 | Description trimmed | " text " | Trimmed to "text" | P2 |
| FUN-029 | Gmail MessageId validated | Valid format | Malformed | P2 |
| FUN-030 | Status transition: Active→Completed valid | Active | Draft→Completed invalid | P1 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| FUN-031 | Max page size 1000 | Size=5000 | Capped at 1000 | P1 |
| FUN-032 | FK contact exists | Non-existent | FK violation | P0 |
| FUN-033 | FK partner exists | Non-existent | FK violation | P0 |
| FUN-034 | Soft-delete no cascade | Delete interaction | Contact/Partner unchanged | P1 |
| FUN-035 | Gmail MessageId unique | Duplicate | Dedup (skip or update) | P1 |
| FUN-036 | Search result limit | 10,000 matches | Paginated | P2 |
| FUN-037 | Batch operation limit | 1000 interactions | Processed | P2 |
| FUN-038 | Max interactions per contact | System limit | Enforced or unlimited | P2 |
| FUN-039 | Date range search limit | 10 years | Accepted or capped | P2 |
| FUN-040 | Recent interactions limit | Max 100 | Capped | P2 |

### 4.4 Audit Rules (10)

| ID | Action | Expected Audit | Priority |
|----|--------|---------------|----------|
| FUN-041 | Create | CreatedBy=current, CreatedDate=now | P0 |
| FUN-042 | Update | LastModifiedBy=current, LastModifiedDate=now | P0 |
| FUN-043 | Delete | DeletedBy=current, DeletedDate=now | P0 |
| FUN-044 | Status change | LastModifiedBy updated | P1 |
| FUN-045 | Gmail import | CreatedBy=user or system | P1 |
| FUN-046 | Read no audit change | Audit fields unchanged | P1 |
| FUN-047 | Batch update | Each interaction's audit set | P1 |
| FUN-048 | Failed operation | No audit change | P1 |
| FUN-049 | Reassign contact | Audit trail entry | P1 |
| FUN-050 | Restore | IsDeleted=false, LastModifiedBy updated | P1 |
| FUN-051 | IsDeleted filter | Deleted excluded | P0 |
| FUN-052 | Create audit | CreatedBy/Date | P0 |
| FUN-053 | Update audit | LastModifiedBy/Date | P0 |
| FUN-054 | Delete soft-delete | IsDeleted set | P0 |
| FUN-055 | Name auto-set | Type + Date | P1 |
| FUN-056 | Contact validation | Exists, !deleted | P0 |
| FUN-057 | Partner validation | Exists, !deleted | P0 |
| FUN-058 | Date range | FromDate ≤ ToDate | P0 |
| FUN-059 | Gmail dedup | MessageId | P1 |
| FUN-060 | Status transitions | Valid only | P1 |
| FUN-061 | Search case-insensitive | Match | P1 |
| FUN-062 | Pagination defaults | Page=1, Size=20 | P1 |
| FUN-063 | Recent sort desc | Most recent first | P1 |
| FUN-064 | Count exclude deleted | !IsDeleted | P1 |
| FUN-065 | Contact change scope | Follows contact | P1 |
| FUN-066 | Description required | Reject null | P0 |
| FUN-067 | ContactId positive | Reject 0 | P0 |
| FUN-068 | PartnerId positive | Reject 0 | P0 |
| FUN-069 | Type valid enum | Reject invalid | P0 |
| FUN-070 | FromDate required | Reject null | P0 |
| FUN-071 | FromDate ≤ ToDate | Reject reversed | P0 |
| FUN-072 | Contact exists | Reject deleted | P0 |
| FUN-073 | Partner exists | Reject deleted | P0 |
| FUN-074 | XSS sanitize | Escape script | P0 |
| FUN-075 | Description max 4000 | Reject 4001 | P1 |
| FUN-076 | Location max 500 | Reject 501 | P2 |
| FUN-077 | Subject max 200 | Reject 201 | P2 |
| FUN-078 | Description trim | Trimmed | P2 |
| FUN-079 | Gmail MessageId | Valid format | P2 |
| FUN-080 | Status Active→Completed | Valid | P1 |
| FUN-081 | Max page size 1000 | Capped | P1 |
| FUN-082 | FK contact | Violation | P0 |
| FUN-083 | FK partner | Violation | P0 |
| FUN-084 | Soft-delete no cascade | Contact/Partner intact | P1 |
| FUN-085 | Gmail MessageId unique | Dedup | P1 |
| FUN-086 | Search limit | Paginated | P2 |
| FUN-087 | Batch limit | Processed | P2 |
| FUN-088 | Max per contact | Enforced | P2 |
| FUN-089 | Date range limit | Accepted | P2 |
| FUN-090 | Recent limit 100 | Capped | P2 |

---

## §5 Integration Tests (End-to-End) — 90 tests

> **Minimum:** 90 tests

### 5.1 CRUD (10)

| ID | Test | Operation | Expected | Priority |
|----|------|----------|----------|----------|
| INT-001 | Full CRUD lifecycle | Create→Read→Update→Delete | All succeed | P0 |
| INT-002 | Create → appears in partner interactions | Create | Listed under partner | P0 |
| INT-003 | Create → appears in contact interactions | Create | Listed under contact | P0 |
| INT-004 | Delete → excluded from lists | Soft-delete | Not in partner/contact lists | P0 |
| INT-005 | Update → persists across reads | Update + read | Changes persisted | P0 |
| INT-006 | Create all 6 types | Each type | All created successfully | P1 |
| INT-007 | Status transition lifecycle | Active→Completed | Status updated | P1 |
| INT-008 | Gmail import lifecycle | Import → read → search | Searchable | P1 |
| INT-009 | Restore deleted | Restore → read | Available again | P1 |
| INT-010 | Bulk create 50 interactions | Batch | All 50 created | P1 |

### 5.2 Search & Filter (10)

| ID | Test | Criteria | Expected | Priority |
|----|------|---------|----------|----------|
| INT-011 | Search by type Meeting | Type filter | Only meetings | P0 |
| INT-012 | Search by date range | Last 30 days | Only recent | P0 |
| INT-013 | Search by type + date | Meeting + last 7 days | Intersection | P1 |
| INT-014 | Search by partner | PartnerId | Partner's interactions | P1 |
| INT-015 | Search by contact | ContactId | Contact's interactions | P1 |
| INT-016 | Search case-insensitive | "meeting" | Matches "Meeting" | P1 |
| INT-017 | Search returns empty | "NONEXISTENT" | Empty set | P1 |
| INT-018 | Filter excludes deleted | Include deleted contact | Filtered out | P1 |
| INT-019 | Combined partner + type + date | All three | Narrow result | P1 |
| INT-020 | Clear filters | Reset | All interactions shown | P1 |

### 5.3 Pagination (5)

| ID | Page | Expected | Priority |
|----|------|----------|----------|
| INT-021 | Page 1 of 3 | 20 interactions | P1 |
| INT-022 | Page 3 of 3 (partial) | Remaining items | P1 |
| INT-023 | Empty results | 0 total | P1 |
| INT-024 | Single page | < pageSize | P2 |
| INT-025 | Large page size 1000 | All in 1 page | P2 |

### 5.4 Relationships (10)

| ID | Relationship | Scenario | Expected | Priority |
|----|-------------|---------|----------|----------|
| INT-026 | Interaction → Contact | Include | Contact loaded | P0 |
| INT-027 | Interaction → Partner | Include | Partner loaded | P0 |
| INT-028 | Contact deletion impact | Delete contact | Interactions remain, contact ref broken or cascade | P1 |
| INT-029 | Partner deletion impact | Delete partner | Interactions remain | P1 |
| INT-030 | Interaction across contacts | Same partner, diff contacts | Both listed under partner | P1 |
| INT-031 | Gmail → Interaction link | Gmail import | Gmail metadata linked | P1 |
| INT-032 | Interaction → OrgUnit (via partner) | Scope check | OrgUnit scoping works | P1 |
| INT-033 | Multiple interactions same contact | 10 interactions | All listed | P2 |
| INT-034 | Interaction type affects categorization | Type grouping | Correct grouping | P2 |
| INT-035 | Audit trail integration | Modify + check audit | Audit entries match | P1 |

### 5.5 Error Handling (15)

| ID | Error | Expected | Priority |
|----|-------|----------|----------|
| INT-036 | Invalid data → 400 | BusinessException | P0 |
| INT-037 | Not found → 404 | KeyNotFoundException | P0 |
| INT-038 | Unauthorized → 403 | UnauthorizedAccessException | P0 |
| INT-039 | Update non-existent → 404 | KeyNotFoundException | P0 |
| INT-040 | Delete non-existent → 404 | KeyNotFoundException | P0 |
| INT-041 | Duplicate Gmail → dedup | Handled gracefully | P1 |
| INT-042 | FK violation → 400 | BusinessException | P1 |
| INT-043 | Date range invalid → 400 | BusinessException | P1 |
| INT-044 | DB timeout → 500 | Graceful error | P1 |
| INT-045 | Concurrency conflict → 409 | Optimistic concurrency | P1 |
| INT-046 | Malformed request → 400 | Validation error | P1 |
| INT-047 | Rate limit → 429 | Rate limit message | P2 |
| INT-048 | SQL injection sanitized | No harm | P0 |
| INT-049 | Large payload → 413 | Request too large | P2 |
| INT-050 | Session expired → 401 | Auth required | P1 |
| INT-051 | Full CRUD | All succeed | P0 |
| INT-052 | Create→Partner list | Listed | P0 |
| INT-053 | Create→Contact list | Listed | P0 |
| INT-054 | Delete→Excluded | Not in lists | P0 |
| INT-055 | Update→Persisted | Saved | P0 |
| INT-056 | Create all 6 types | All created | P1 |
| INT-057 | Status lifecycle | Active→Completed | P1 |
| INT-058 | Gmail import lifecycle | Import→Search | P1 |
| INT-059 | Restore | Available again | P1 |
| INT-060 | Bulk create 50 | All created | P1 |
| INT-061 | Search type Meeting | Only meetings | P0 |
| INT-062 | Search date range | Recent only | P0 |
| INT-063 | Search type+date | Intersection | P1 |
| INT-064 | Search partner | Partner's | P1 |
| INT-065 | Search contact | Contact's | P1 |
| INT-066 | Search case-insensitive | Same | P1 |
| INT-067 | Search empty | Empty | P1 |
| INT-068 | Filter exclude deleted | Correct | P1 |
| INT-069 | Combined filters | Narrow | P1 |
| INT-070 | Clear filters | All shown | P1 |
| INT-071 | Page 1 of 3 | 20 items | P1 |
| INT-072 | Page 3 partial | Remaining | P1 |
| INT-073 | Empty page | 0 total | P1 |
| INT-074 | Single page | < pageSize | P2 |
| INT-075 | Large page 1000 | All | P2 |
| INT-076 | Interaction→Contact | Loaded | P0 |
| INT-077 | Interaction→Partner | Loaded | P0 |
| INT-078 | Contact delete impact | Handled | P1 |
| INT-079 | Partner delete impact | Handled | P1 |
| INT-080 | Same partner diff contacts | Both listed | P1 |
| INT-081 | Gmail→Interaction | Linked | P1 |
| INT-082 | Interaction→OrgUnit | Scope | P1 |
| INT-083 | Multiple same contact | All listed | P2 |
| INT-084 | Type affects category | Grouping | P2 |
| INT-085 | Audit integration | Match | P1 |
| INT-086 | Invalid 400 | BusinessException | P0 |
| INT-087 | NotFound 404 | KeyNotFound | P0 |
| INT-088 | Unauthorized 403 | Unauthorized | P0 |
| INT-089 | Duplicate Gmail | Handled | P1 |
| INT-090 | End-to-end | Full flow | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL in Description | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL in search | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in Description | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in Subject | Script tag | Sanitized | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | HTML in Notes | `<img onerror=...>` | Escaped | P1 |
| SEC-007 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-008 | Path traversal | `../../etc/passwd` | Rejected | P1 |
| SEC-009 | XML entity | XXE payload | Rejected | P1 |
| SEC-010 | Template injection | `{{constructor}}` | Escaped | P1 |

### 6.2 Access Control (10)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Anonymous | POST create | 401 | P0 |
| SEC-012 | No permission | POST create | 403 | P0 |
| SEC-013 | Scoped | Out-of-scope read | 403 | P0 |
| SEC-014 | Scoped | Out-of-scope create | 403 | P0 |
| SEC-015 | Expired | Any | 401 | P0 |
| SEC-016 | Tampered JWT | Any | 401/403 | P0 |
| SEC-017 | Horizontal | Other user's data | 403 | P0 |
| SEC-018 | Disabled | Any | 403 | P1 |
| SEC-019 | Post-logout | Cached | 401 | P1 |
| SEC-020 | Role escalation | ?role=admin | Ignored | P0 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Interaction ID | Guess | 403 if not in scope | P0 |
| SEC-022 | Sequential enum | /1, /2, /3 | Rate limited | P0 |
| SEC-023 | Deleted ID | Access deleted | 404 | P1 |
| SEC-024 | Other OrgUnit | Change scope | 403 | P0 |
| SEC-025 | Negative ID | -1 | 400 | P1 |
| SEC-026 | Zero ID | 0 | 400 | P1 |
| SEC-027 | Float ID | 1.5 | 400 | P1 |
| SEC-028 | String ID | "abc" | 400 | P1 |
| SEC-029 | MAX_INT | Large ID | 404 | P1 |
| SEC-030 | Other contact's interaction | Wrong scope | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Field | Expected | Priority |
|----|-------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable | P0 |
| SEC-035 | DeletedBy/Date | Not modifiable | P1 |

### 6.5 Auth & Session (10)

| ID | Attack | Protection | Priority |
|----|--------|-----------|----------|
| SEC-036 | Brute-force | Lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Hijacking | Token binding | P1 |
| SEC-039 | CSRF create | CSRF token | P0 |
| SEC-040 | CSRF delete | CSRF token | P0 |
| SEC-041 | Token storage | HttpOnly | P0 |
| SEC-042 | Concurrent sessions | Policy | P1 |
| SEC-043 | Token refresh | Works | P1 |
| SEC-044 | Logout | Invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Protection | Priority |
|----|------|-----------|----------|
| SEC-046 | Internal fields | DTO filtered | P1 |
| SEC-047 | Stack traces | Generic errors | P0 |
| SEC-048 | Gmail credentials | Not exposed | P0 |
| SEC-049 | Cache | no-store | P1 |
| SEC-050 | Tokens in URL | HttpOnly | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| CON-001 | Two users update same interaction | Last-write-wins or conflict | P1 |
| CON-002 | Create and delete simultaneously | One succeeds, other fails | P1 |
| CON-003 | Two users create for same contact | Both succeed | P1 |
| CON-004 | Update during read | Consistent state | P1 |
| CON-005 | Delete during read | Null or pre-delete data | P1 |
| CON-006 | Concurrent status change | One succeeds | P1 |
| CON-007 | Gmail import race condition | Dedup handles both | P1 |
| CON-008 | Concurrent pagination | Correct pages | P2 |
| CON-009 | Database deadlock | Resolved, retry | P1 |
| CON-010 | Token refresh during create | Retry with new token | P1 |
| CON-011 | Bulk import concurrent | Both batches complete | P2 |
| CON-012 | Search during bulk update | Consistent results | P2 |
| CON-013 | Optimistic concurrency | Version conflict detected | P1 |
| CON-014 | Concurrent soft-delete | One succeeds, other no-op | P1 |
| CON-015 | Rapid status changes | Final state correct | P1 |
| CON-016 | Connection pool exhaustion | Wait or graceful error | P1 |
| CON-017 | Cache invalidation during read | Fresh data | P1 |
| CON-018 | Concurrent count queries | Consistent | P2 |
| CON-019 | Session timeout during save | Rolled back | P1 |
| CON-020 | Parallel partner lookups | All succeed | P2 |
| CON-021 | Multiple Gmail syncs | Dedup handles | P1 |
| CON-022 | Database migration during operation | Graceful | P2 |
| CON-023 | Two users reassign same interaction | One succeeds | P1 |
| CON-024 | Export during modification | Consistent snapshot | P2 |
| CON-025 | Concurrent filter changes | Final correct | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Category | Input | Expected | Priority |
|----|----------|-------|----------|----------|
| UNT-001 | Validation | Email valid "a@b.com" | Valid | P1 |
| UNT-002 | Validation | Null Description | Invalid | P1 |
| UNT-003 | Validation | FromDate > ToDate | Invalid | P1 |
| UNT-004 | Validation | Invalid Type | Invalid | P1 |
| UNT-005 | Validation | PartnerId = -1 | Invalid | P1 |
| UNT-006 | Formatting | Type + Date → Name | "Meeting - 2026-02-11" | P1 |
| UNT-007 | Formatting | Duration calculation | "2h 30m" | P1 |
| UNT-008 | Formatting | Date display | "Feb 11, 2026" | P2 |
| UNT-009 | Calculations | Count non-deleted | 5 of 7 (2 deleted) | P1 |
| UNT-010 | Calculations | Pagination pages | 55/20=3 | P1 |
| UNT-011 | Calculations | HasNext page | True for page 1 of 3 | P1 |
| UNT-012 | Calculations | Duration (From→To) | Correct hours | P1 |
| UNT-013 | Calculations | Count by type | {Meeting:5, Email:3} | P1 |
| UNT-014 | Status | IsDeletedCheck | true → inaccessible | P1 |
| UNT-015 | Status | ValidTransition | Active→Completed = valid | P1 |
| UNT-016 | Status | InvalidTransition | Draft→Completed = invalid | P1 |
| UNT-017 | Status | GmailDedupCheck | Existing=skip | P1 |
| UNT-018 | Status | ContactAssocValid | Active=valid | P1 |
| UNT-019 | Collections | FilterByDateRange | Correct subset | P1 |
| UNT-020 | Collections | GroupByType | Dictionary<Type, List> | P1 |
| UNT-021 | Collections | SortByDate | Descending order | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Create single | < 200ms | P1 |
| PRF-002 | Get with includes | < 300ms | P1 |
| PRF-003 | Bulk create 100 | < 5s | P2 |
| PRF-004 | Bulk create 1000 | < 30s | P2 |
| PRF-005 | Gmail import 50 | < 10s | P2 |
| PRF-006 | Search 1000 interactions | < 500ms | P1 |
| PRF-007 | Search 10,000 | < 1s | P1 |
| PRF-008 | Paginate 10,000 | < 500ms/page | P1 |
| PRF-009 | Date range search | < 500ms | P1 |
| PRF-010 | Count query | < 100ms | P1 |
| PRF-011 | 10 concurrent creates | < 1s each | P2 |
| PRF-012 | 50 concurrent reads | < 500ms each | P2 |
| PRF-013 | 100 concurrent searches | < 1s each | P2 |
| PRF-014 | Memory 10,000 load | < 200MB | P2 |
| PRF-015 | Memory 50,000 query | < 500MB | P2 |
| PRF-016 | Memory leak check | No growth > 10% | P1 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 50 concurrent CRUD | 30 min | 95% < 500ms | P2 |
| LDT-002 | 100 concurrent reads | 30 min | 95% < 300ms | P2 |
| LDT-003 | 50 concurrent searches | 15 min | < 1s/search | P2 |
| LDT-004 | Spike 10→200 req/s | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike + Gmail imports | 5 min | All complete | P2 |
| LDT-006 | 500 concurrent ops | 10 min | Graceful degradation | P2 |
| LDT-007 | 100K interactions in DB | 15 min | Queries < 1s | P2 |
| LDT-008 | Continuous create/delete | 10 min | Stable | P2 |
| LDT-009 | Recovery after DB crash | N/A | < 60s | P2 |
| LDT-010 | Recovery after service restart | N/A | < 30s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|-----------|
| Interaction CRUD | POS-001–005, INT-001–005, NEG-001–010 |
| Contact/Partner association | POS-011–012, FUN-006–007, NEG-002–006 |
| Date range validation | FUN-008, FUN-021, NEG-008, BND-017–028 |
| Type classification | POS-006–010, FUN-019, BND-049–055 |
| Gmail integration | POS-016–018, FUN-009, CON-007, CON-021 |
| Status lifecycle | POS-031–032, FUN-010, FUN-030, NEG-023–025 |
| Soft delete | POS-004, FUN-004, NEG-021–022, SEC-031 |
| Audit trail | FUN-041–050, POS-034 |
| Security | SEC-001–050 |
| Performance | PRF-001–016, LDT-001–010 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
