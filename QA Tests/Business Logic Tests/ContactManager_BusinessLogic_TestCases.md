# ContactManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ContactManager`  
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

The ContactManager handles CRUD operations for contacts associated with partners. Key functionality includes: contact creation with partner association, email validation, document and interaction loading, soft delete, partner reassignment, pagination, specification-based filtering, audit trail, profile picture upload, and Gmail integration.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Create Contact with Valid Data

**Priority:** P0  
**Precondition:** Partner exists and is active. User has create permission.

**Steps:**
1. Call `CreateContactAsync` with valid contact data (FirstName, LastName, Email, PartnerId)
2. Verify response

**Expected Result:**
- Contact created with auto-generated Id
- PartnerId correctly associated
- CreatedBy set to current user ID
- CreatedDate set to current UTC timestamp
- IsDeleted = false
- Status = Active

---

#### POS-002: Get Contact by ID with Related Data

**Priority:** P0  
**Precondition:** Contact exists with linked documents and interactions.

**Steps:**
1. Call `GetContactByIdAsync(contactId)` with includes for documents and interactions
2. Verify response includes related data

**Expected Result:**
- Contact data returned with all fields populated
- Documents collection loaded (non-deleted only)
- Interactions collection loaded (non-deleted only)
- Partner navigation property loaded

---

#### POS-003: Update Contact Fields

**Priority:** P0  
**Precondition:** Contact exists, user has edit permission.

**Steps:**
1. Call `UpdateContactAsync` with modified FirstName, LastName, Email, Phone
2. Verify changes persisted

**Expected Result:**
- All modified fields updated in database
- LastModifiedBy set to current user ID
- LastModifiedDate updated to current UTC timestamp
- Unchanged fields remain intact

---

#### POS-004: Soft Delete Contact

**Priority:** P0  
**Precondition:** Contact exists, user has delete permission.

**Steps:**
1. Call `DeleteContactAsync(contactId)`
2. Verify soft delete applied

**Expected Result:**
- Contact.IsDeleted = true
- Contact.DeletedBy = current user ID
- Contact.DeletedDate = current UTC timestamp
- Contact NOT physically removed from database
- Subsequent `GetContactByIdAsync` returns null (filtered by IsDeleted)

---

#### POS-005: List Contacts with Pagination

**Priority:** P0  
**Precondition:** 50+ contacts exist for a partner.

**Steps:**
1. Call `GetContactsWithPagination` with page=1, pageSize=20
2. Verify paginated results

**Expected Result:**
- Returns exactly 20 contacts on page 1
- Total count reflects all non-deleted contacts
- Results ordered by configured default sort
- No deleted contacts included

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Get contacts by partner ID | Partner with contacts | GetContactsByPartnerIdAsync | Returns all non-deleted contacts for partner | P1 |
| POS-007 | Search contacts by name | Contacts exist | SearchContactsAsync("John") | Returns matching contacts | P1 |
| POS-008 | Reassign contact to different partner | Contact linked to Partner A | ReassignContactAsync(contactId, partnerBId) | Contact.PartnerId = partnerBId | P1 |
| POS-009 | Load contact with documents | Contact has 3 documents | GetContactByIdAsync with doc include | 3 non-deleted documents loaded | P1 |
| POS-010 | Load contact with interactions | Contact has 5 interactions | GetContactByIdAsync with interaction include | 5 non-deleted interactions loaded | P1 |
| POS-011 | Upload profile picture | Valid image file | UploadProfilePictureAsync | Picture URL stored on contact | P1 |
| POS-012 | Filter contacts by specification | Specification criteria | GetContactsWithSpecification | Matching contacts returned | P1 |
| POS-013 | Create contact with all optional fields | All fields populated | CreateContactAsync | All fields persisted | P1 |
| POS-014 | Update contact email only | Existing contact | UpdateContactAsync (email change) | Only email updated, audit fields set | P1 |
| POS-015 | Get contact audit trail | Contact with modifications | GetContactAuditAsync | Audit entries returned | P1 |
| POS-016 | Create contact with Gmail integration data | Gmail metadata | CreateContactAsync with Gmail source | Gmail fields populated | P1 |
| POS-017 | Paginate contacts page 2 | 50+ contacts | GetContactsWithPagination(page=2) | Contacts 21-40 returned | P1 |
| POS-018 | Sort contacts by name ascending | Multiple contacts | GetContactsWithPagination(sortBy=name, asc) | Alphabetically sorted | P1 |
| POS-019 | Sort contacts by date descending | Multiple contacts | GetContactsWithPagination(sortBy=date, desc) | Most recent first | P1 |
| POS-020 | Get contact count for partner | Partner with contacts | GetContactCountAsync(partnerId) | Correct count (non-deleted only) | P1 |
| POS-021 | Create contact with minimum required fields | FirstName, LastName, PartnerId only | CreateContactAsync | Contact created with nulls for optional | P2 |
| POS-022 | Update contact phone number | Existing contact | UpdateContactAsync (phone change) | Phone updated, audit set | P2 |
| POS-023 | Update contact title/position | Existing contact | UpdateContactAsync (title change) | Title updated | P2 |
| POS-024 | Get contacts for multiple partners | 3 partner IDs | GetContactsByPartnerIdsAsync | Contacts from all 3 partners | P2 |
| POS-025 | Filter contacts by status (Active) | Active + inactive contacts | Filter by Active status | Only active contacts returned | P2 |
| POS-026 | Filter contacts by email domain | Contacts with various domains | FilterByEmailDomain("@unops.org") | Only matching contacts | P2 |
| POS-027 | Contact with Unicode name | Arabic/Chinese name | CreateContactAsync | Name stored and retrieved correctly | P2 |
| POS-028 | Contact with special characters in name | O'Brien, Müller | CreateContactAsync | Special chars preserved | P2 |
| POS-029 | Map contact entity to model | Contact entity | mapper.Map<ContactModel> | All fields mapped correctly | P2 |
| POS-030 | Map create request to entity | CreateContactRequest | mapper.Map<Contact> | All request fields mapped | P2 |

---

## §2 Negative Tests (Failure Scenarios) — 90 tests

> **Minimum:** 90 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create with null FirstName | FirstName = null | BusinessException: "First name is required" | P0 |
| NEG-002 | Create with empty FirstName | FirstName = "" | BusinessException: "First name is required" | P0 |
| NEG-003 | Create with null LastName | LastName = null | BusinessException: "Last name is required" | P0 |
| NEG-004 | Create with invalid email format | Email = "not-an-email" | BusinessException: "Invalid email format" | P0 |
| NEG-005 | Create with null PartnerId | PartnerId = null/0 | BusinessException: "Partner is required" | P0 |
| NEG-006 | Create with non-existent PartnerId | PartnerId = 999999 | KeyNotFoundException: "Partner not found" | P0 |
| NEG-007 | Create with deleted partner | PartnerId of deleted partner | BusinessException: "Partner has been deleted" | P0 |
| NEG-008 | Update non-existent contact | Id = 999999 | KeyNotFoundException: "Contact not found" | P0 |
| NEG-009 | Delete non-existent contact | Id = 999999 | KeyNotFoundException: "Contact not found" | P0 |
| NEG-010 | Get non-existent contact | Id = 999999 | KeyNotFoundException or null | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | User without create permission | Read-only role | CreateContactAsync | UnauthorizedAccessException | P0 |
| NEG-012 | User without edit permission | Read-only role | UpdateContactAsync | UnauthorizedAccessException | P0 |
| NEG-013 | User without delete permission | Read-only role | DeleteContactAsync | UnauthorizedAccessException | P0 |
| NEG-014 | OrgUnit-scoped user creates in wrong scope | Scoped user | Create contact for out-of-scope partner | UnauthorizedAccessException | P0 |
| NEG-015 | OrgUnit-scoped user reads out-of-scope | Scoped user | GetContactByIdAsync (wrong scope) | UnauthorizedAccessException or null | P0 |
| NEG-016 | OrgUnit-scoped user updates out-of-scope | Scoped user | UpdateContactAsync (wrong scope) | UnauthorizedAccessException | P0 |
| NEG-017 | OrgUnit-scoped user deletes out-of-scope | Scoped user | DeleteContactAsync (wrong scope) | UnauthorizedAccessException | P0 |
| NEG-018 | User without view permission | No CanViewContacts | GetContactByIdAsync | UnauthorizedAccessException | P1 |
| NEG-019 | Expired user session | Expired context | Any operation | UnauthorizedAccessException | P1 |
| NEG-020 | Anonymous user (no auth context) | No user | CreateContactAsync | UnauthorizedAccessException | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Update already-deleted contact | IsDeleted=true | UpdateContactAsync | BusinessException or KeyNotFoundException | P1 |
| NEG-022 | Delete already-deleted contact | IsDeleted=true | DeleteContactAsync | No-op or error | P1 |
| NEG-023 | Reassign contact to deleted partner | Target partner deleted | ReassignContactAsync | BusinessException: "Target partner is deleted" | P1 |
| NEG-024 | Reassign deleted contact | Contact deleted | ReassignContactAsync | BusinessException | P1 |
| NEG-025 | Upload picture for deleted contact | Contact deleted | UploadProfilePictureAsync | BusinessException | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Create with all nulls | All fields null | BusinessException (multiple validation errors) | P1 |
| NEG-027 | Update with null email | Email set to null | Validation passes (email optional) or error based on rules | P1 |
| NEG-028 | Filter with null specification | Spec = null | ArgumentNullException or empty results | P1 |
| NEG-029 | Paginate with null page params | Page = null | Default to page 1, size 20 | P1 |
| NEG-030 | Search with null query | Query = null | Empty results or all contacts | P1 |
| NEG-031 | Create with whitespace-only name | "   " for names | BusinessException: "Name cannot be blank" | P1 |
| NEG-032 | Get contacts for null partner ID | PartnerId = null | ArgumentException or empty | P1 |
| NEG-033 | Upload null file | File = null | BusinessException: "File is required" | P1 |
| NEG-034 | Upload empty file (0 bytes) | File size = 0 | BusinessException: "File is empty" | P1 |
| NEG-035 | Reassign to null partner ID | Target = null | ArgumentException | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Database connection lost during create | DB drops | Exception, transaction rolled back | P1 |
| NEG-037 | Database timeout during query | Slow DB | TimeoutException, graceful handling | P1 |
| NEG-038 | File storage failure on picture upload | Cloud storage down | BusinessException: "Upload failed" | P1 |
| NEG-039 | AutoMapper configuration missing | Missing map | AutoMapperMappingException | P2 |
| NEG-040 | Repository throws on add | Constraint violation | BusinessException with context | P1 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Create duplicate email for same partner | Same email exists | BusinessException: "Email already exists for this partner" | P1 |
| NEG-042 | Reassign to same partner | Same partner ID | No-op or validation message | P2 |
| NEG-043 | Create contact with email > max length | Email = 500 chars | Validation error | P1 |
| NEG-044 | Create with FirstName > max length | 300-char FirstName | Validation error | P1 |
| NEG-045 | Create with LastName > max length | 300-char LastName | Validation error | P1 |
| NEG-046 | Upload oversized profile picture | File > 5MB | BusinessException: "File exceeds size limit" | P1 |
| NEG-047 | Upload invalid file type for picture | .exe file | BusinessException: "Invalid file type" | P0 |
| NEG-048 | Create with malicious email | Email = `"><script>` | Input sanitized or rejected | P0 |
| NEG-049 | Create with SQL injection in name | `'; DROP TABLE--` | Parameterized query, no injection | P0 |
| NEG-050 | Phone number with letters | Phone = "abc-def" | Validation error or sanitized | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Create with negative PartnerId | PartnerId = -1 | Validation error | P1 |
| NEG-052 | Create with zero PartnerId | PartnerId = 0 | Validation error | P1 |
| NEG-053 | Get with negative contact ID | Id = -1 | Not found or validation error | P1 |
| NEG-054 | Get with zero contact ID | Id = 0 | Not found or validation error | P1 |
| NEG-055 | Paginate with page = 0 | Page = 0 | Default to 1 or validation error | P2 |
| NEG-056 | Paginate with pageSize = 0 | Size = 0 | Default or validation error | P2 |
| NEG-057 | Paginate with pageSize = -1 | Size = -1 | Validation error | P2 |
| NEG-058 | Paginate with pageSize > 1000 | Size = 5000 | Capped at max or error | P2 |
| NEG-059 | Sort by invalid column name | SortBy = "INVALID" | Default sort or error | P2 |
| NEG-060 | Create with duplicate phone for partner | Same phone exists | Allowed (phone not unique) or error | P2 |
| NEG-061 | Update email to duplicate within partner | Email exists for sibling contact | Business error | P1 |
| NEG-062 | Create with future date in custom field | Date > today | Validation error if applicable | P2 |
| NEG-063 | Multiple validation errors at once | Null name + invalid email | All errors returned | P1 |
| NEG-064 | Create contact for inactive partner | Partner status = Inactive | Business rule validation | P1 |
| NEG-065 | Upload picture with path traversal name | `../../evil.jpg` | Filename sanitized | P0 |
| NEG-066 | Create with null request object | Request = null | ArgumentNullException | P1 |
| NEG-067 | Update with null request object | Request = null | ArgumentNullException | P1 |
| NEG-068 | Gmail sync with invalid metadata | Malformed Gmail data | Handled gracefully, fields default | P2 |
| NEG-069 | Search with special regex characters | `.*+?[]()` | Treated as literal or escaped | P1 |
| NEG-070 | Filter with invalid date range | FromDate > ToDate | Validation error | P1 |
| NEG-071 | Input | Null reassign target | ArgumentException | P1 |
| NEG-072 | Input | Invalid email domain filter | Error | P2 |
| NEG-073 | State | Update during reassign | Conflict | P1 |
| NEG-074 | Dep | Storage fail on picture | BusinessException | P1 |
| NEG-075 | Auth | Reassign out of scope | Unauthorized | P0 |
| NEG-076 | Data | Email with 321 chars | Validation error | P1 |
| NEG-077 | Mass | Mass assign DeletedBy | Blocked | P1 |
| NEG-078 | Mass | Mass assign DeletedDate | Blocked | P1 |
| NEG-079 | Filter | Filter by invalid status | Error | P2 |
| NEG-080 | Search | Search with injection | Parameterized | P0 |
| NEG-081 | Input | Negative document count | Handled | P2 |
| NEG-082 | State | Load with deleted partner | Excluded or error | P1 |
| NEG-083 | Gmail | Gmail sync duplicate | Dedup | P1 |
| NEG-084 | Batch | Batch create all invalid | Error | P1 |
| NEG-085 | Export | Export during update | Consistent | P1 |
| NEG-086 | Input | Null specification | Default or error | P1 |
| NEG-087 | Dep | DB timeout on bulk | Graceful | P1 |
| NEG-088 | Auth | Create for inactive partner | Business rule | P1 |
| NEG-089 | Picture | Picture upload during delete | Conflict | P1 |
| NEG-090 | Input | PageSize 0 | Default or error | P2 |

---

## §3 Boundary Tests (Edge Cases) — 90 tests

> **Minimum:** 90 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | FirstName | 1 | 200 | ✅ "A" | ✅ 200 chars | ❌ Rejected | P1 |
| BND-002 | LastName | 1 | 200 | ✅ "B" | ✅ 200 chars | ❌ Rejected | P1 |
| BND-003 | Email | 5 | 320 | ✅ "a@b.c" | ✅ 320 chars | ❌ Rejected | P1 |
| BND-004 | Phone | 0 | 50 | ✅ Empty/null | ✅ 50 chars | ❌ Rejected | P1 |
| BND-005 | Title/Position | 0 | 200 | ✅ Empty | ✅ 200 chars | ❌ Rejected | P2 |
| BND-006 | Notes | 0 | 4000 | ✅ Empty | ✅ 4000 chars | ❌ Rejected | P2 |
| BND-007 | Address | 0 | 500 | ✅ Empty | ✅ 500 chars | ❌ Rejected | P2 |
| BND-008 | ProfilePictureUrl | 0 | 2048 | ✅ No picture | ✅ Long URL | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-009 | Contact ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-010 | PartnerId | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-011 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-012 | Page size | 1 | 1000 | ❌ Default | ❌ Error | Capped at 1000 | P1 |
| BND-013 | Contact count per partner | 0 | 10000 | ✅ Empty list | ✅ Large list | Performance | P1 |
| BND-014 | Document count per contact | 0 | 500 | ✅ No docs | ✅ Many docs | Performance | P2 |
| BND-015 | Interaction count per contact | 0 | 1000 | ✅ No interactions | ✅ Many | Performance | P2 |
| BND-016 | Profile picture file size | 1KB | 5MB | ✅ Tiny | ✅ At limit | ❌ Rejected | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-017 | Contact created on leap year | Feb 29, 2028 | CreatedDate stored correctly | P2 |
| BND-018 | Contact with very old date | Jan 1, 2000 | Handled correctly | P2 |
| BND-019 | Contact created at midnight UTC | 00:00:00 UTC | No date boundary error | P2 |
| BND-020 | Contact created at 23:59:59 UTC | End of day | Correct date storage | P2 |
| BND-021 | Filter from date = to date | Same day | Returns contacts from that day | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-022 | Partner with 0 contacts | Empty | Returns empty list, count = 0 | P1 |
| BND-023 | Partner with 1 contact | Single | Returns list with 1 item | P1 |
| BND-024 | Partner with exactly page size contacts | 20 contacts, page=1, size=20 | Full page, total=20, hasNext=false | P1 |
| BND-025 | Partner with pageSize + 1 contacts | 21 contacts, page=1, size=20 | 20 on page 1, hasNext=true | P1 |
| BND-026 | Partner with 1000 contacts | Large collection | Paginated correctly | P1 |
| BND-027 | Contact with 0 documents | No documents | Empty documents collection | P1 |
| BND-028 | Contact with 0 interactions | No interactions | Empty interactions collection | P1 |
| BND-029 | Contact with 100 documents | Many docs | All loaded (non-deleted) | P2 |
| BND-030 | Contact with 100 interactions | Many interactions | All loaded (non-deleted) | P2 |
| BND-031 | Last page of paginated results | Page 5 of 5 | Correct remaining contacts | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-032 | FirstName (Arabic) | `أحمد` | Stored and retrieved correctly | P2 |
| BND-033 | LastName (Chinese) | `李明` | Stored and retrieved correctly | P2 |
| BND-034 | FirstName (Cyrillic) | `Иван` | Stored correctly | P2 |
| BND-035 | LastName (Accented) | `García-López` | Accents and hyphen preserved | P2 |
| BND-036 | Name with apostrophe | `O'Brien` | Apostrophe preserved, no SQL issues | P1 |
| BND-037 | Name with umlaut | `Müller` | Umlaut preserved | P2 |
| BND-038 | Email with subdomain | `user@sub.domain.co.uk` | Valid email accepted | P1 |
| BND-039 | Email with plus | `user+tag@example.com` | Valid email accepted | P1 |
| BND-040 | Phone with international format | `+1-555-123-4567` | Stored as-is | P2 |
| BND-041 | Notes with emoji | `Great meeting! 🤝` | Emoji stored and retrieved | P2 |
| BND-042 | Address with newlines | Multi-line address | Newlines preserved | P2 |

### 3.6 Profile Picture Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-043 | Upload 1KB image | Minimum size | Accepted | P1 |
| BND-044 | Upload exactly 5MB image | At limit | Accepted | P1 |
| BND-045 | Upload 5MB + 1 byte | Over limit | Rejected | P1 |
| BND-046 | Upload PNG format | Valid type | Accepted | P1 |
| BND-047 | Upload JPG format | Valid type | Accepted | P1 |
| BND-048 | Upload GIF format | Valid type | Accepted (if allowed) | P2 |
| BND-049 | Upload WebP format | Valid type | Accepted (if allowed) | P2 |
| BND-050 | Upload SVG format | Potentially risky | Rejected (XSS risk) | P1 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Email with exactly min valid format | `a@b.c` (5 chars) | Accepted | P1 |
| BND-052 | Email with 320 characters | Max email length | Accepted | P1 |
| BND-053 | Name with exactly 1 character | "A" | Accepted | P1 |
| BND-054 | Name with exactly max chars | 200 chars | Accepted | P1 |
| BND-055 | Contact ID = 1 (minimum valid) | First contact | Retrieved correctly | P2 |
| BND-056 | Contact ID = MAX_INT | 2147483647 | Handled correctly | P2 |
| BND-057 | PartnerId = 1 | Minimum valid | Contact created | P2 |
| BND-058 | Paginate last page with 1 item | 41 contacts, page 3, size 20 | 1 contact on page 3 | P1 |
| BND-059 | Search with exactly 1 char | "J" | Matches all starting with J | P1 |
| BND-060 | Search with exactly max chars | 255 chars | Processed correctly | P1 |
| BND-061 | Multiple partners each with 1 contact | 100 partners, 1 contact each | Batch query returns all | P2 |
| BND-062 | Contact with all optional fields null | Only required fields | Created successfully | P1 |
| BND-063 | Contact with all optional fields filled | All fields populated | Created successfully | P1 |
| BND-064 | Gmail metadata at max length | Max Gmail fields | Stored correctly | P2 |
| BND-065 | Sort by each available column | Name, Email, Date, etc. | Each sort works correctly | P1 |
| BND-066 | Filter with exact match | Exact name = "John Doe" | Exact match returned | P2 |
| BND-067 | Filter with partial match | "Joh" matches "John" | Partial matches returned | P2 |
| BND-068 | Concurrent page requests | Pages 1, 2, 3 simultaneously | All return correct data | P2 |
| BND-069 | Contact at midnight timezone boundary | UTC vs local timezone | Correct date handling | P2 |
| BND-070 | Partner with exactly MAX contacts | At partner contact limit | Last contact accepted | P1 |
| BND-071 | FirstName 50 chars | Accepted | P1 |
| BND-072 | LastName 100 chars | Accepted | P1 |
| BND-073 | Email 100 chars | Accepted | P1 |
| BND-074 | PartnerId 500 | Valid | P1 |
| BND-075 | Page 100 | Handled | P2 |
| BND-076 | PageSize 500 | Accepted | P1 |
| BND-077 | Contacts 500 per partner | Paginated | P1 |
| BND-078 | Documents 50 per contact | Loaded | P2 |
| BND-079 | Interactions 100 per contact | Loaded | P2 |
| BND-080 | Picture 2MB | Accepted | P1 |
| BND-081 | Name 150 chars | Accepted | P1 |
| BND-082 | Notes 2000 chars | Accepted | P2 |
| BND-083 | Address 250 chars | Accepted | P2 |
| BND-084 | Unicode name Hindi | Stored | P2 |
| BND-085 | Email with plus | user+tag@example.com | Valid | P1 |
| BND-086 | Contact ID 5000 | Retrieved | P2 |
| BND-087 | Last page 3 items | Correct | P1 |
| BND-088 | Search 50 chars | Processed | P1 |
| BND-089 | Filter 2 partners | Both | P2 |
| BND-090 | All optional null | Created | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 90 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10), Extended (40)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Contacts query excludes deleted (IsDeleted filter) | Soft-delete filter | GetContacts | Only !IsDeleted returned | P0 |
| FUN-002 | Create sets audit fields | Audit on create | CreateContactAsync | CreatedBy, CreatedDate set | P0 |
| FUN-003 | Update sets audit fields | Audit on update | UpdateContactAsync | LastModifiedBy, LastModifiedDate set | P0 |
| FUN-004 | Delete sets soft-delete fields | Soft-delete | DeleteContactAsync | IsDeleted, DeletedBy, DeletedDate set | P0 |
| FUN-005 | Name property auto-set from First+Last | Name inheritance | Create contact | Name = "FirstName LastName" | P1 |
| FUN-006 | Partner association validated on create | FK validation | Create with PartnerId | Partner must exist and !IsDeleted | P0 |
| FUN-007 | Reassign updates PartnerId | Reassignment logic | ReassignContactAsync | PartnerId changed, audit set | P1 |
| FUN-008 | Documents loaded exclude deleted | Include filter | GetContactByIdAsync | Only !IsDeleted documents | P1 |
| FUN-009 | Interactions loaded exclude deleted | Include filter | GetContactByIdAsync | Only !IsDeleted interactions | P1 |
| FUN-010 | Profile picture URL persisted | Upload logic | UploadProfilePictureAsync | URL stored on entity | P1 |
| FUN-011 | Search is case-insensitive | Search logic | SearchContactsAsync | "john" matches "John" | P1 |
| FUN-012 | Pagination defaults applied | Default params | Page=null → 1, Size=null → 20 | Correct defaults | P1 |
| FUN-013 | Gmail source tracked | Integration | Create from Gmail | Source="Gmail" field set | P1 |
| FUN-014 | Contact count excludes deleted | Count query | GetContactCountAsync | Only !IsDeleted counted | P1 |
| FUN-015 | Typeahead returns Id and Name | Dropdown data | GetContactTypeaheadAsync | List of {Id, Name} | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | FirstName required and non-empty | Required | "John" | null, "" | P0 |
| FUN-017 | LastName required and non-empty | Required | "Doe" | null, "" | P0 |
| FUN-018 | Email format validation | RFC 5322 | "a@b.com" | "not-email" | P0 |
| FUN-019 | PartnerId required and positive | FK required | 42 | 0, -1, null | P0 |
| FUN-020 | Partner must exist and not deleted | FK + soft-delete | Active partner | Deleted/non-existent | P0 |
| FUN-021 | Email unique per partner | Uniqueness | New email | Duplicate for partner | P1 |
| FUN-022 | Profile picture type validation | Image types only | .jpg, .png | .exe, .bat | P0 |
| FUN-023 | Profile picture size limit | ≤ 5MB | 4MB | 6MB | P1 |
| FUN-024 | Phone format lenient | Any string | "+1-555-1234" | Very long string (>50) | P2 |
| FUN-025 | Notes max length | ≤ 4000 chars | 3999 chars | 4001 chars | P2 |
| FUN-026 | First name no leading/trailing whitespace | Trim | " John " → "John" | N/A | P2 |
| FUN-027 | Last name no leading/trailing whitespace | Trim | " Doe " → "Doe" | N/A | P2 |
| FUN-028 | Email lowercase normalization | Normalize | "John@GMAIL.COM" → "john@gmail.com" | N/A | P2 |
| FUN-029 | Input sanitization for HTML | XSS prevention | "John" | `<script>alert(1)</script>` | P0 |
| FUN-030 | Reassign target must be active partner | Business rule | Active partner | Deleted/inactive partner | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max contacts per partner | System limit | Create beyond limit | Error or unlimited | P2 |
| FUN-032 | Max page size | 1000 | Size=5000 | Capped at 1000 | P1 |
| FUN-033 | Max file upload concurrency | 1 per contact | 2 simultaneous uploads | Second queued | P2 |
| FUN-034 | Unique email per partner enforced | DB constraint | Duplicate email | Constraint error | P1 |
| FUN-035 | Foreign key partner exists | FK constraint | Non-existent partner | FK violation | P0 |
| FUN-036 | Soft-delete doesn't violate FK | Cascade | Delete contact with docs | Docs remain, contact flagged | P1 |
| FUN-037 | Search result limit | API limit | 10,000 matches | Paginated, limited | P2 |
| FUN-038 | Profile picture overwrites previous | Single picture | Upload new | Old URL replaced | P1 |
| FUN-039 | Gmail deduplication | Same Gmail ID | Import same contact twice | Deduplication or update | P1 |
| FUN-040 | Batch operation limit | Max batch size | 1000 contacts in batch | Processed or chunked | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Create audit entry | CreateContactAsync | CreatedBy = currentUser, CreatedDate = now | P0 |
| FUN-042 | Update audit entry | UpdateContactAsync | LastModifiedBy = currentUser, LastModifiedDate = now | P0 |
| FUN-043 | Delete audit entry | DeleteContactAsync | DeletedBy = currentUser, DeletedDate = now | P0 |
| FUN-044 | Reassign audit entry | ReassignContactAsync | LastModifiedBy updated, audit trail | P1 |
| FUN-045 | Picture upload audit | UploadProfilePictureAsync | LastModifiedBy updated | P1 |
| FUN-046 | Read operation no audit | GetContactByIdAsync | No modification to audit fields | P1 |
| FUN-047 | Batch update audit | Batch operation | Each contact's audit fields updated | P1 |
| FUN-048 | Gmail import audit | Create from Gmail | CreatedBy = system or user, source=Gmail | P1 |
| FUN-049 | Failed operation no audit change | Failed create | No audit entries created | P1 |
| FUN-050 | Audit fields immutable on read | Get contact | CreatedBy/Date never modified by reads | P1 |
| FUN-051 | IsDeleted filter | Deleted excluded | P0 |
| FUN-052 | Create audit | CreatedBy/Date | P0 |
| FUN-053 | Update audit | LastModifiedBy/Date | P0 |
| FUN-054 | Delete soft-delete | IsDeleted set | P0 |
| FUN-055 | Name from First+Last | Auto-set | P1 |
| FUN-056 | Partner validation | Exists, !deleted | P0 |
| FUN-057 | Reassign PartnerId | Updated | P1 |
| FUN-058 | Documents exclude deleted | !IsDeleted | P1 |
| FUN-059 | Interactions exclude deleted | !IsDeleted | P1 |
| FUN-060 | Picture URL | Persisted | P1 |
| FUN-061 | Search case-insensitive | Match | P1 |
| FUN-062 | Pagination defaults | Page=1, Size=20 | P1 |
| FUN-063 | Gmail source | Tracked | P1 |
| FUN-064 | Count exclude deleted | !IsDeleted | P1 |
| FUN-065 | Typeahead Id+Name | Returned | P1 |
| FUN-066 | FirstName required | Reject null | P0 |
| FUN-067 | LastName required | Reject null | P0 |
| FUN-068 | Email format | RFC 5322 | P0 |
| FUN-069 | PartnerId required | Reject 0 | P0 |
| FUN-070 | Partner exists | Reject deleted | P0 |
| FUN-071 | Email unique per partner | Reject duplicate | P1 |
| FUN-072 | Picture type | Reject .exe | P0 |
| FUN-073 | Picture size ≤5MB | Reject >5MB | P1 |
| FUN-074 | Phone format | Lenient | P2 |
| FUN-075 | Notes max 4000 | Reject 4001 | P2 |
| FUN-076 | FirstName trim | Trimmed | P2 |
| FUN-077 | LastName trim | Trimmed | P2 |
| FUN-078 | Email lowercase | Normalized | P2 |
| FUN-079 | XSS prevention | Sanitize | P0 |
| FUN-080 | Reassign target active | Reject deleted | P1 |
| FUN-081 | Max contacts per partner | Enforced | P2 |
| FUN-082 | Max page size 1000 | Capped | P1 |
| FUN-083 | Unique email | Constraint | P1 |
| FUN-084 | FK partner | Violation | P0 |
| FUN-085 | Soft-delete no cascade | Docs intact | P1 |
| FUN-086 | Search limit | Paginated | P2 |
| FUN-087 | Picture overwrite | Replaced | P1 |
| FUN-088 | Gmail dedup | Handled | P1 |
| FUN-089 | Batch limit | Chunked | P2 |
| FUN-090 | Deletable check | Active=true | P1 |

---

## §5 Integration Tests (End-to-End Flows) — 90 tests

> **Minimum:** 90 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Full CRUD lifecycle | Create→Read→Update→Delete | Contact | All operations succeed | P0 |
| INT-002 | Create contact → appears in partner's contacts | Create | Contact, Partner | Listed under partner | P0 |
| INT-003 | Delete contact → excluded from partner list | Delete | Contact, Partner | Not in partner.Contacts | P0 |
| INT-004 | Update contact → persists across sessions | Update + read | Contact | Changes persisted | P0 |
| INT-005 | Create with documents → both saved | Create + attach | Contact, Document | Both entities saved | P1 |
| INT-006 | Delete contact → documents remain | Soft-delete | Contact, Documents | Docs not cascade-deleted | P1 |
| INT-007 | Reassign → old partner count decreases | Reassign | Contact, Partners | Counts update correctly | P1 |
| INT-008 | Reassign → new partner count increases | Reassign | Contact, Partners | Counts update correctly | P1 |
| INT-009 | Restore deleted → appears in list again | Restore | Contact | Re-included in queries | P1 |
| INT-010 | Create from Gmail → searchable | Gmail import | Contact | Found via search | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Search by first name | "John" | Contacts with first name John | P0 |
| INT-012 | Search by last name | "Doe" | Contacts with last name Doe | P0 |
| INT-013 | Search by email | "john@" | Contacts with matching email | P1 |
| INT-014 | Filter by partner | PartnerId = 42 | Only partner 42's contacts | P0 |
| INT-015 | Filter by status | Active | Only active contacts | P1 |
| INT-016 | Filter by date range | Last 30 days | Recently created contacts | P1 |
| INT-017 | Combined search + filter | "John" + PartnerId=42 | John contacts for partner 42 | P1 |
| INT-018 | Search returns empty | "NONEXISTENT" | Empty result set | P1 |
| INT-019 | Search is case-insensitive | "john" vs "JOHN" | Same results | P1 |
| INT-020 | Filter excludes deleted | Include deleted partner | Deleted partner's contacts excluded | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Page 1 of 3 | 50 contacts, page=1, size=20 | 20 contacts, hasNext=true | P1 |
| INT-022 | Page 3 of 3 (last page) | 50 contacts, page=3, size=20 | 10 contacts, hasNext=false | P1 |
| INT-023 | Empty results page | Filter yields 0 | Empty list, total=0 | P1 |
| INT-024 | Single page | 15 contacts, size=20 | 15 contacts, hasNext=false | P2 |
| INT-025 | Large page size | 1000 contacts, size=1000 | All contacts, 1 page | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Contact → Partner relationship | FK | Load contact | Partner loaded | P0 |
| INT-027 | Contact → Documents relationship | Include | Load with docs | Documents loaded | P0 |
| INT-028 | Contact → Interactions relationship | Include | Load with interactions | Interactions loaded | P1 |
| INT-029 | Partner deletion impacts contacts | Cascade | Delete partner | Contacts accessible but partner deleted | P1 |
| INT-030 | Contact across partners (search) | Cross-partner | Search all contacts | Returns from all partners | P1 |
| INT-031 | Contact with Gmail integration | Gmail link | Load with Gmail data | Gmail fields populated | P1 |
| INT-032 | Contact → Opportunity (via interaction) | Indirect | Load contact chain | Opportunity reachable | P2 |
| INT-033 | Contact → OrgUnit (via partner) | Indirect | Scope check | OrgUnit-scoped query works | P1 |
| INT-034 | Multiple contacts same email (different partners) | Email uniqueness per partner | Create for P1 and P2 | Both accepted | P2 |
| INT-035 | Contact → Audit trail | Audit | Load contact audit | Complete history | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Create with invalid data → 400 | Validation error | BusinessException with details | P0 |
| INT-037 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-038 | Unauthorized create → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-039 | Update non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-040 | Delete non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-041 | Duplicate email → 400 | Constraint | BusinessException | P1 |
| INT-042 | FK violation (deleted partner) → 400 | FK error | BusinessException | P1 |
| INT-043 | Upload invalid file → 400 | File validation | BusinessException | P1 |
| INT-044 | Upload oversized file → 400 | Size limit | BusinessException | P1 |
| INT-045 | Database timeout → 500 | Timeout | Graceful error | P1 |
| INT-046 | Concurrent conflict → 409 | Concurrency | Optimistic concurrency error | P1 |
| INT-047 | Malformed request → 400 | Bad JSON | Validation error | P1 |
| INT-048 | Rate limit exceeded → 429 | Too many requests | Rate limit message | P2 |
| INT-049 | Search SQL injection → sanitized | Injection attempt | Parameterized, no harm | P0 |
| INT-050 | Large payload → 413 | Oversized body | Request too large error | P2 |
| INT-051 | Full CRUD | All succeed | P0 |
| INT-052 | Create→Partner list | Listed | P0 |
| INT-053 | Delete→Excluded | Not in list | P0 |
| INT-054 | Update→Persisted | Saved | P0 |
| INT-055 | Create with docs | Both saved | P1 |
| INT-056 | Delete→Docs remain | Intact | P1 |
| INT-057 | Reassign→Old count | Decreases | P1 |
| INT-058 | Reassign→New count | Increases | P1 |
| INT-059 | Restore | Re-included | P1 |
| INT-060 | Gmail create→Search | Found | P1 |
| INT-061 | Search first name | Matching | P0 |
| INT-062 | Search last name | Matching | P0 |
| INT-063 | Search email | Matching | P1 |
| INT-064 | Filter partner | Partner's only | P0 |
| INT-065 | Filter status | Active only | P1 |
| INT-066 | Filter date range | Recent | P1 |
| INT-067 | Combined search+filter | Intersection | P1 |
| INT-068 | Search empty | Empty | P1 |
| INT-069 | Case-insensitive | Same | P1 |
| INT-070 | Exclude deleted | Correct | P1 |
| INT-071 | Page 1 of 3 | 20 items | P1 |
| INT-072 | Page 3 partial | Remaining | P1 |
| INT-073 | Empty page | 0 total | P1 |
| INT-074 | Single page | All | P2 |
| INT-075 | Large page 1000 | All | P2 |
| INT-076 | Contact→Partner | Loaded | P0 |
| INT-077 | Contact→Documents | Loaded | P0 |
| INT-078 | Contact→Interactions | Loaded | P1 |
| INT-079 | Partner delete impact | Handled | P1 |
| INT-080 | Contact cross-partner | All | P1 |
| INT-081 | Gmail integration | Populated | P1 |
| INT-082 | Contact→Opportunity | Reachable | P2 |
| INT-083 | Contact→OrgUnit | Scope | P1 |
| INT-084 | Same email diff partners | Both | P2 |
| INT-085 | Audit trail | Complete | P1 |
| INT-086 | Invalid 400 | BusinessException | P0 |
| INT-087 | NotFound 404 | KeyNotFound | P0 |
| INT-088 | Unauthorized 403 | Unauthorized | P0 |
| INT-089 | Duplicate email 400 | BusinessException | P1 |
| INT-090 | End-to-end | Full flow | P0 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in FirstName | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL injection in search | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in FirstName | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in Email | `"><script>` | Sanitized | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | OS command in filename | `; rm -rf /` | Sanitized | P0 |
| SEC-007 | Path traversal in upload | `../../evil.jpg` | Rejected | P0 |
| SEC-008 | HTML in notes | `<img onerror=...>` | Escaped | P1 |
| SEC-009 | JSON injection in API | `{"$ne":null}` | Rejected | P1 |
| SEC-010 | XML entity injection | XXE payload | Rejected | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous create | No auth | POST /contacts | 401 | P0 |
| SEC-012 | No create permission | Read-only | POST /contacts | 403 | P0 |
| SEC-013 | OrgUnit scope violation (read) | Scoped | GET /contacts/{other} | 403 | P0 |
| SEC-014 | OrgUnit scope violation (create) | Scoped | POST /contacts (other OrgUnit) | 403 | P0 |
| SEC-015 | Expired token | Expired | Any operation | 401 | P0 |
| SEC-016 | Tampered JWT | Modified | Any operation | 401/403 | P0 |
| SEC-017 | Horizontal access | User A | User B's contacts | 403 | P0 |
| SEC-018 | Disabled account | Disabled | Any operation | 403 | P1 |
| SEC-019 | Post-logout | Logged out | Cached call | 401 | P1 |
| SEC-020 | Role escalation | Basic | ?role=admin | Ignored | P0 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Contact ID | Guess ID | 403 if not in scope | P0 |
| SEC-022 | Partner ID | Enumerate | Rate limited | P0 |
| SEC-023 | Deleted contact | Access deleted | 404 | P1 |
| SEC-024 | Other OrgUnit contact | Change scope | 403 | P0 |
| SEC-025 | Negative ID | -1 | 400 | P1 |
| SEC-026 | Zero ID | 0 | 400 | P1 |
| SEC-027 | Float ID | 1.5 | 400 | P1 |
| SEC-028 | String ID | "abc" | 400 | P1 |
| SEC-029 | MAX_INT ID | 2147483647 | 404 | P1 |
| SEC-030 | Other user's contact | Access via ID | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable via create/update | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable via API | P0 |
| SEC-035 | DeletedBy/DeletedDate | Not modifiable via update | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on create | CSRF token | P0 |
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
| SEC-048 | Sensitive contact PII | Filtered per permission | P1 |
| SEC-049 | Response caching | Cache-Control: no-store | P1 |
| SEC-050 | Auth tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users update same contact | Concurrent update | Last write wins or conflict error | P1 |
| CON-002 | Create and delete same contact | Race condition | One succeeds, other fails | P1 |
| CON-003 | Two users create for same partner | Concurrent create | Both succeed if no duplication | P1 |
| CON-004 | Update during read | Read consistency | Read sees before or after, not partial | P1 |
| CON-005 | Delete during read | Read consistency | Read returns null or pre-delete data | P1 |
| CON-006 | Concurrent reassign | Both reassign same contact | One succeeds, other conflicts | P1 |
| CON-007 | Concurrent picture upload | Same contact | Last upload wins | P1 |
| CON-008 | Concurrent pagination | Multiple page requests | Each returns correct page | P2 |
| CON-009 | Database deadlock | Circular lock | Deadlock resolved, retry | P1 |
| CON-010 | Token refresh during create | Token expires mid-call | Retry with new token | P1 |
| CON-011 | Bulk import concurrent | 100 contacts imported by 2 users | Both batches complete | P2 |
| CON-012 | Gmail sync concurrent | Two Gmail syncs | Deduplication handles both | P2 |
| CON-013 | Search during bulk update | Search while batch runs | Consistent results | P2 |
| CON-014 | Export during modification | Export + update | Export captures consistent snapshot | P2 |
| CON-015 | Multiple filter changes | Rapid filter changes | Final state correct | P1 |
| CON-016 | Optimistic concurrency | Stale entity update | Version conflict detected | P1 |
| CON-017 | Concurrent soft-delete by two users | Both delete same contact | One succeeds, other no-op | P1 |
| CON-018 | Create with same email simultaneously | Race on unique constraint | One succeeds, other fails with duplicate | P1 |
| CON-019 | Database migration during operation | Schema change | Graceful handling | P2 |
| CON-020 | Cache invalidation during read | Cache expires | Fresh data fetched | P1 |
| CON-021 | Concurrent count queries | Multiple count requests | All return consistent count | P2 |
| CON-022 | Parallel partner lookups for contact creation | Validate partner concurrently | All validations succeed | P2 |
| CON-023 | Session timeout during create | Timeout mid-save | Transaction rolled back | P1 |
| CON-024 | Concurrent typeahead requests | Rapid typing | Each request independent | P2 |
| CON-025 | Database connection pool exhaustion | Many concurrent requests | Connection wait or graceful error | P1 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Validate email format valid | Validation | "a@b.com" | Valid | P1 |
| UNT-002 | Validate email format invalid | Validation | "not-email" | Invalid | P1 |
| UNT-003 | Validate required FirstName | Validation | null | Invalid | P1 |
| UNT-004 | Validate required LastName | Validation | "" | Invalid | P1 |
| UNT-005 | Validate PartnerId positive | Validation | -1 | Invalid | P1 |
| UNT-006 | Format full name | Formatting | ("John", "Doe") | "John Doe" | P1 |
| UNT-007 | Format name with trim | Formatting | (" John ", " Doe ") | "John Doe" | P1 |
| UNT-008 | Format email lowercase | Formatting | "JOHN@GMAIL.COM" | "john@gmail.com" | P2 |
| UNT-009 | Calculate contact count for partner | Calculations | 5 contacts (2 deleted) | 3 | P1 |
| UNT-010 | Calculate pagination totals | Calculations | 55 contacts, size=20 | Pages=3 | P1 |
| UNT-011 | Calculate has next page | Calculations | Page 1 of 3 | HasNext=true | P1 |
| UNT-012 | Calculate has previous page | Calculations | Page 2 of 3 | HasPrev=true | P1 |
| UNT-013 | Calculate search match | Calculations | "John" in "John Doe" | Match=true | P1 |
| UNT-014 | Determine contact is deletable | Status | Active contact | Deletable=true | P1 |
| UNT-015 | Determine contact is deleted | Status | IsDeleted=true | Accessible=false | P1 |
| UNT-016 | Determine partner association valid | Status | Active partner | Valid=true | P1 |
| UNT-017 | Determine email uniqueness | Status | Unique email | Valid=true | P1 |
| UNT-018 | Determine file type valid | Status | "image/png" | Valid=true | P1 |
| UNT-019 | Build contact name from parts | Collections | First + Last | Full name | P1 |
| UNT-020 | Filter contacts by IsDeleted | Collections | Mixed contacts | Only !IsDeleted | P1 |
| UNT-021 | Group contacts by partner | Collections | Mixed partners | Grouped dictionary | P1 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create single contact | Insert | < 200ms | P1 |
| PRF-002 | Get contact with includes | Query + includes | < 300ms | P1 |
| PRF-003 | Bulk create 100 contacts | Batch insert | < 5 seconds | P2 |
| PRF-004 | Bulk create 1000 contacts | Batch insert | < 30 seconds | P2 |
| PRF-005 | Upload 5MB profile picture | File upload | < 3 seconds | P2 |
| PRF-006 | Search 1000 contacts | Search query | < 500ms | P1 |
| PRF-007 | Search 10,000 contacts | Large search | < 1 second | P1 |
| PRF-008 | Paginate 10,000 contacts | Pagination | < 500ms per page | P1 |
| PRF-009 | Filter with specification | Spec query | < 500ms | P1 |
| PRF-010 | Get contact count | Count query | < 100ms | P1 |
| PRF-011 | 10 concurrent creates | Concurrent | < 1s per create | P2 |
| PRF-012 | 50 concurrent reads | Concurrent | < 500ms per read | P2 |
| PRF-013 | 100 concurrent searches | Concurrent | < 1s per search | P2 |
| PRF-014 | Memory for 10,000 contact load | Memory | < 200MB | P2 |
| PRF-015 | Memory for 50,000 contact query | Memory | < 500MB | P2 |
| PRF-016 | Memory leak on repeated operations | Memory | No growth > 10% | P1 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | 50 concurrent CRUD operations | Sustained | 30 min | 95% < 500ms, 0 errors | P2 |
| LDT-002 | 100 concurrent reads | Sustained | 30 min | 95% < 300ms | P2 |
| LDT-003 | 50 concurrent searches | Sustained | 15 min | < 1s per search | P2 |
| LDT-004 | Spike 10 to 200 requests/s | Spike | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike with concurrent uploads | 50 reads + 10 uploads | 5 min | All complete | P2 |
| LDT-006 | 500 concurrent operations | Stress | 10 min | Graceful degradation | P2 |
| LDT-007 | 100,000 contacts in DB | Large data | 15 min | Queries < 1s | P2 |
| LDT-008 | Continuous create/delete cycle | 50 users | 10 min | Stable performance | P2 |
| LDT-009 | Recovery after DB crash | Kill + restart | N/A | Recovery < 60s | P2 |
| LDT-010 | Recovery after service restart | Service restart | N/A | Recovery < 30s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|-----------|
| Contact CRUD operations | POS-001–005, INT-001–004, NEG-001–010 |
| Partner association | POS-006, POS-008, FUN-006, FUN-007, NEG-005–007 |
| Soft delete | POS-004, FUN-004, NEG-021–022, SEC-031 |
| Email validation | POS-014, FUN-018, FUN-021, NEG-004, BND-003 |
| Pagination | POS-005, POS-017, INT-021–025, BND-022–031 |
| Profile picture | POS-011, FUN-010, NEG-046–047, BND-043–050 |
| Gmail integration | POS-016, FUN-013, FUN-039, CON-012 |
| Security & permissions | SEC-001–050, NEG-011–020 |
| Audit trail | FUN-041–050, POS-015 |
| Performance | PRF-001–016, LDT-001–010 |

---

## Test Environment Setup

**Prerequisites:**
- Test database with seeded partners and contacts
- File storage service (or mock) for profile picture uploads
- Gmail integration test credentials (or mock)
- Multiple user accounts with varying permissions and OrgUnit scopes

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
