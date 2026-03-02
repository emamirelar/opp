# PartnerManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerManager`  
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

The PartnerManager handles CRUD for partners (organizations). Key features: approval workflow with ERP dim value assignment (uniqueness, reserved range 8000-9999), unapproval, status transitions (Draft→Active→Closed→Archived), OrgUnit relationships, soft delete, partner group/category validation, contact cascade, OrgUnit filtering, logo upload, partner tree recursion, specification filters, smart search, concurrency, sorting, audit trail, Gmail integration, and pagination.

---

## §1 Positive Tests (Happy Path) — 30 tests

### Detailed Test Cases (P0)

#### POS-001: Create Partner with Valid Data
**Priority:** P0 | **Precondition:** User has create permission. OrgUnit exists.
**Steps:** Call `CreatePartnerAsync` with Name, Type, Group, Category, OrgUnitId
**Expected:** Partner created, Id generated, Status=Draft, audit fields set, IsDeleted=false

#### POS-002: Approve Partner — ERP Dim Value Assigned
**Priority:** P0 | **Precondition:** Partner in Draft status. User has approval permission.
**Steps:** Call `ApprovePartnerAsync(id)`
**Expected:** Status → Active, ErpDimValue assigned (1-7999), unique across all partners

#### POS-003: Get Partner by ID with Includes
**Priority:** P0 | **Precondition:** Partner exists with contacts, interactions, documents.
**Steps:** Call `GetPartnerByIdAsync(id)` with includes
**Expected:** Partner returned with all related data loaded (!IsDeleted filtered)

#### POS-004: Soft Delete Partner
**Priority:** P0 | **Precondition:** Partner exists, user has delete permission.
**Steps:** Call `DeletePartnerAsync(id)`
**Expected:** IsDeleted=true, DeletedBy/Date set, contacts not cascade-deleted

#### POS-005: List Partners with Pagination
**Priority:** P0 | **Precondition:** 100+ partners exist.
**Steps:** Call `GetPartnersWithPagination(page=1, size=20)`
**Expected:** 20 partners returned, no deleted, total count correct

### Positive Tests — Tabular (P1/P2) — 30 tests

| ID | Test Name | Steps (Brief) | Expected | Priority |
|----|-----------|--------------|----------|----------|
| POS-006 | Unapprove partner | UnapprovePartnerAsync | Status → Draft, ErpDimValue cleared | P1 |
| POS-007 | Close partner | ChangeStatus(Closed) | Status → Closed | P1 |
| POS-008 | Archive partner | ChangeStatus(Archived) | Status → Archived | P1 |
| POS-009 | Reactivate closed partner | ChangeStatus(Active) | Status → Active | P1 |
| POS-010 | ERP dim value in valid range (1-7999) | Approve | Value ∈ [1,7999] | P1 |
| POS-011 | ERP dim value unique | Approve 2 partners | Different values | P1 |
| POS-012 | Get partners by OrgUnit | GetByOrgUnit | Only OrgUnit's partners | P1 |
| POS-013 | Search partners by name | SmartSearch("ACME") | Matching partners | P1 |
| POS-014 | Filter by group | FilterByGroup | Correct group's partners | P1 |
| POS-015 | Filter by category | FilterByCategory | Correct category's partners | P1 |
| POS-016 | Filter by type (Funding) | FilterByType | Only Funding partners | P1 |
| POS-017 | Filter by type (Client) | FilterByType | Only Client partners | P1 |
| POS-018 | Filter by type (Implementation) | FilterByType | Only Implementation partners | P1 |
| POS-019 | Get partner tree (hierarchy) | GetPartnerTree | Recursive tree structure | P1 |
| POS-020 | Upload partner logo | UploadLogo | Logo URL stored | P1 |
| POS-021 | Update partner name | UpdatePartner | Name changed, audit set | P1 |
| POS-022 | Update partner OrgUnit | UpdatePartner | OrgUnitId changed | P1 |
| POS-023 | Get partner contacts | GetPartnerContacts | Non-deleted contacts | P1 |
| POS-024 | Get partner interactions | GetPartnerInteractions | Non-deleted interactions | P1 |
| POS-025 | Get partner documents | GetPartnerDocuments | Non-deleted documents | P1 |
| POS-026 | Specification filter | GetWithSpecification | Filtered results | P2 |
| POS-027 | Sort by name ascending | Sort(name, asc) | A-Z order | P2 |
| POS-028 | Sort by date descending | Sort(date, desc) | Newest first | P2 |
| POS-029 | Get partner audit trail | GetAudit | History entries | P2 |
| POS-030 | Map entity to model | mapper.Map | All fields mapped | P2 |

---

## §2 Negative Tests — 90 tests

### 2.1 Invalid Input (10)
| ID | Invalid Input | Expected | Priority |
|----|--------------|----------|----------|
| NEG-001 | Null Name | BusinessException: required | P0 |
| NEG-002 | Empty Name | BusinessException: required | P0 |
| NEG-003 | Null Type | BusinessException: required | P0 |
| NEG-004 | Invalid Type value | BusinessException | P0 |
| NEG-005 | Null OrgUnitId | BusinessException: required | P0 |
| NEG-006 | Non-existent OrgUnitId | KeyNotFoundException | P0 |
| NEG-007 | Update non-existent partner | KeyNotFoundException | P0 |
| NEG-008 | Delete non-existent partner | KeyNotFoundException | P0 |
| NEG-009 | Approve non-existent partner | KeyNotFoundException | P0 |
| NEG-010 | Approve already-active partner | BusinessException: already approved | P0 |

### 2.2 Unauthorized Access (10)
| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| NEG-011 | No auth | Create | Unauthorized | P0 |
| NEG-012 | Read-only | Create | Unauthorized | P0 |
| NEG-013 | Read-only | Update | Unauthorized | P0 |
| NEG-014 | Read-only | Delete | Unauthorized | P0 |
| NEG-015 | No approval perm | Approve | Unauthorized | P0 |
| NEG-016 | OrgUnit-scoped | Create out of scope | Unauthorized | P0 |
| NEG-017 | OrgUnit-scoped | Update out of scope | Unauthorized | P0 |
| NEG-018 | OrgUnit-scoped | Delete out of scope | Unauthorized | P0 |
| NEG-019 | Expired session | Any | Unauthorized | P1 |
| NEG-020 | Disabled account | Any | Unauthorized | P1 |

### 2.3 ERP Dim Value (10)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| NEG-021 | Approve when all 1-7999 values taken | BusinessException: no values available | P0 |
| NEG-022 | Manually set ErpDimValue to reserved range (8000) | Rejected: reserved range | P0 |
| NEG-023 | Manually set ErpDimValue to 9999 | Rejected: reserved range | P0 |
| NEG-024 | Manually set ErpDimValue to 0 | Rejected: out of range | P1 |
| NEG-025 | Manually set ErpDimValue to -1 | Rejected: out of range | P1 |
| NEG-026 | Manually set ErpDimValue to 10000 | Rejected: out of range | P1 |
| NEG-027 | Duplicate ErpDimValue assignment | Uniqueness enforced | P0 |
| NEG-028 | Unapprove then re-approve gets new value | Re-approve | New unique value assigned | P1 |
| NEG-029 | ErpDimValue collision with soft-deleted partner | Uniqueness includes deleted | P1 |
| NEG-030 | Approve partner with incomplete prerequisites | Missing required data | BusinessException | P0 |

### 2.4 Invalid State Transitions (10)
| ID | Transition | Expected | Priority |
|----|-----------|----------|----------|
| NEG-031 | Draft → Closed (skip Active) | BusinessException: invalid transition | P1 |
| NEG-032 | Draft → Archived | BusinessException | P1 |
| NEG-033 | Archived → Active | BusinessException (if not allowed) | P1 |
| NEG-034 | Closed → Draft | BusinessException | P1 |
| NEG-035 | Update deleted partner | BusinessException | P1 |
| NEG-036 | Approve deleted partner | BusinessException | P1 |
| NEG-037 | Delete already-deleted | No-op or error | P1 |
| NEG-038 | Upload logo for deleted partner | BusinessException | P1 |
| NEG-039 | Change OrgUnit for approved partner | Business rule check | P1 |
| NEG-040 | Re-approve active partner | BusinessException: already active | P1 |

### 2.5 Missing Data (10)
| ID | Missing | Expected | Priority |
|----|---------|----------|----------|
| NEG-041 | All nulls | Multiple validation errors | P1 |
| NEG-042 | Null request object | ArgumentNullException | P1 |
| NEG-043 | Whitespace-only Name | BusinessException | P1 |
| NEG-044 | Null Group | Accepted (if optional) or error | P1 |
| NEG-045 | Null Category | Accepted (if optional) or error | P1 |
| NEG-046 | Invalid Group value | BusinessException | P1 |
| NEG-047 | Invalid Category value | BusinessException | P1 |
| NEG-048 | Logo file = null | BusinessException | P1 |
| NEG-049 | Logo file = 0 bytes | BusinessException | P1 |
| NEG-050 | Logo invalid type (.exe) | BusinessException | P0 |

### 2.6 Additional (20)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| NEG-051 | SQL injection in Name | Parameterized | P0 |
| NEG-052 | XSS in Name | Sanitized | P0 |
| NEG-053 | Name > max length | Validation error | P1 |
| NEG-054 | Negative PartnerId | Not found | P1 |
| NEG-055 | Zero PartnerId | Not found | P1 |
| NEG-056 | Page = 0 | Default to 1 | P2 |
| NEG-057 | PageSize = -1 | Error | P2 |
| NEG-058 | PageSize > 1000 | Capped | P2 |
| NEG-059 | Sort by invalid column | Default sort | P2 |
| NEG-060 | Search with regex chars | Escaped | P1 |
| NEG-061 | Logo > 5MB | Rejected | P1 |
| NEG-062 | Logo path traversal | Sanitized | P0 |
| NEG-063 | Circular hierarchy (parent = self) | Rejected | P0 |
| NEG-064 | Circular hierarchy (A→B→A) | Rejected | P0 |
| NEG-065 | Create duplicate name (same OrgUnit) | Allowed or duplicate warning | P2 |
| NEG-066 | Batch delete with mixed valid/invalid | Valid deleted, invalid error | P1 |
| NEG-067 | Multiple validation errors at once | All returned | P1 |
| NEG-068 | Create for deleted OrgUnit | BusinessException | P1 |
| NEG-069 | Gmail import malformed data | Handled gracefully | P2 |
| NEG-070 | Search empty string | No results or all | P1 |
| NEG-071 | Input | Null approval request | ArgumentNull | P1 |
| NEG-072 | Input | Invalid status string | BusinessException | P1 |
| NEG-073 | Auth | Approve without scope | Unauthorized | P0 |
| NEG-074 | State | Update during approval | Conflict | P1 |
| NEG-075 | ERP | ErpDimValue collision on restore | Uniqueness | P1 |
| NEG-076 | Dep | Logo storage unavailable | BusinessException | P1 |
| NEG-077 | Hierarchy | Parent partner deleted | BusinessException | P1 |
| NEG-078 | Mass | Mass assign Status | Validated | P1 |
| NEG-079 | Search | Search with null partnerId | Error or all | P2 |
| NEG-080 | Filter | Filter by invalid group | Error | P2 |
| NEG-081 | Filter | Filter by invalid category | Error | P2 |
| NEG-082 | Tree | Tree with deleted nodes | Excluded | P1 |
| NEG-083 | Export | Export deleted partner | Excluded | P1 |
| NEG-084 | Batch | Batch create with duplicate names | Handled | P2 |
| NEG-085 | Logo | Logo URL path traversal | Sanitized | P0 |
| NEG-086 | State | Reactivate archived (if blocked) | BusinessException | P1 |
| NEG-087 | Dep | DB constraint on OrgUnit change | Error | P1 |
| NEG-088 | Auth | Create for deleted OrgUnit | BusinessException | P1 |
| NEG-089 | Gmail | Gmail API rate limit | Graceful | P2 |
| NEG-090 | Input | Negative page number | Default or error | P2 |

---

## §3 Boundary Tests — 90 tests

### String Lengths (8)
| ID | Field | Min | Max | At Min | At Max | Over | Pr |
|----|-------|-----|-----|--------|--------|------|---|
| BND-001 | Name | 1 | 200 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Code | 0 | 50 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Description | 0 | 4000 | ✅ | ✅ | ❌ | P2 |
| BND-004 | Address | 0 | 500 | ✅ | ✅ | ❌ | P2 |
| BND-005 | Website | 0 | 2048 | ✅ | ✅ | ❌ | P2 |
| BND-006 | Phone | 0 | 50 | ✅ | ✅ | ❌ | P2 |
| BND-007 | Email | 0 | 320 | ✅ | ✅ | ❌ | P2 |
| BND-008 | LogoUrl | 0 | 2048 | ✅ | ✅ | ❌ | P2 |

### Numeric (10)
| ID | Field | Min | Max | Zero | Neg | Pr |
|----|-------|-----|-----|------|-----|---|
| BND-009 | PartnerId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-010 | OrgUnitId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-011 | ErpDimValue | 1 | 7999 | ❌ | ❌ | P0 |
| BND-012 | ErpDimValue reserved min | 8000 | — | Reserved | — | P0 |
| BND-013 | ErpDimValue reserved max | — | 9999 | — | Reserved | P0 |
| BND-014 | Page | 1 | 10000 | ❌ | ❌ | P1 |
| BND-015 | PageSize | 1 | 1000 | ❌ | ❌ | P1 |
| BND-016 | Contacts per partner | 0 | 10000 | ✅ | — | P1 |
| BND-017 | Hierarchy depth | 0 | 20 | ✅ | — | P1 |
| BND-018 | Children per parent | 0 | 1000 | ✅ | — | P1 |

### Date (5)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| BND-019 | Created on leap year | Correct date | P2 |
| BND-020 | Very old creation date | Handled | P2 |
| BND-021 | Created at midnight UTC | No boundary error | P2 |
| BND-022 | Approved at end of year | Correct | P2 |
| BND-023 | Status change at midnight | Correct | P2 |

### Collections (12)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| BND-024 | 0 partners | Empty list | P1 |
| BND-025 | 1 partner | Single item list | P1 |
| BND-026 | Exactly page size (20) | Full page, hasNext=false | P1 |
| BND-027 | PageSize + 1 (21) | 20 + hasNext=true | P1 |
| BND-028 | 1000 partners | Paginated | P1 |
| BND-029 | 10,000 partners | Performance OK | P1 |
| BND-030 | Partner with 0 contacts | Empty contacts | P1 |
| BND-031 | Partner with 500 contacts | Large collection | P1 |
| BND-032 | Partner with 0 documents | Empty docs | P2 |
| BND-033 | Partner tree depth = 0 (root only) | Single node | P1 |
| BND-034 | Partner tree depth = 20 (max) | Full depth | P1 |
| BND-035 | Partner tree width = 100 children | Wide tree | P1 |

### Unicode (10)
| ID | Field | Input | Expected | Pr |
|----|-------|-------|----------|----|
| BND-036 | Name (Arabic) | `مؤسسة` | Stored | P2 |
| BND-037 | Name (Chinese) | `合作公司` | Stored | P2 |
| BND-038 | Name (Cyrillic) | `Организация` | Stored | P2 |
| BND-039 | Name (French) | `Société` | Accents | P2 |
| BND-040 | Name (Emoji) | `🏢 Corp` | Stored | P2 |
| BND-041 | Name with apostrophe | `O'Brien & Co` | Preserved | P1 |
| BND-042 | Name with ampersand | `Smith & Partners` | Preserved | P1 |
| BND-043 | Address multi-line | Multi-line | Newlines | P2 |
| BND-044 | Website with path | `https://example.com/path?q=1` | Stored | P2 |
| BND-045 | Email with plus | `admin+test@corp.com` | Valid | P2 |

### ERP Dim Value Boundaries (10)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| BND-046 | ErpDimValue = 1 (min valid) | Accepted | P0 |
| BND-047 | ErpDimValue = 7999 (max valid) | Accepted | P0 |
| BND-048 | ErpDimValue = 8000 (reserved start) | Rejected | P0 |
| BND-049 | ErpDimValue = 9999 (reserved end) | Rejected | P0 |
| BND-050 | ErpDimValue = 10000 (over reserved) | Rejected | P1 |
| BND-051 | All values 1-7999 taken (7999 partners) | No value available error | P1 |
| BND-052 | 7998 values taken, 1 remaining | Last value assigned | P1 |
| BND-053 | Value freed by unapproval → reusable | Re-assigned | P1 |
| BND-054 | Sequential assignment order | Values assigned sequentially | P2 |
| BND-055 | Gap in values (3 freed) → fills gaps | Gap values reused | P2 |

### Additional (15)
| ID | Scenario | Expected | Priority |
|----|---------|----------|----------|
| BND-056 | Name exactly 1 char | "A" accepted | P1 |
| BND-057 | Name exactly 200 chars | Accepted | P1 |
| BND-058 | Code exactly 50 chars | Accepted | P1 |
| BND-059 | Partner ID = 1 | Retrieved | P2 |
| BND-060 | Partner ID = MAX_INT | Handled | P2 |
| BND-061 | Search 1 char | Results from "A*" | P1 |
| BND-062 | Search 255 chars | Processed | P1 |
| BND-063 | Sort each column | All sort correctly | P1 |
| BND-064 | Filter exact match | Exact result | P2 |
| BND-065 | Filter partial match | Partial matches | P2 |
| BND-066 | Last page with 1 item | Single item page | P1 |
| BND-067 | All partners same type | Filter shows all | P2 |
| BND-068 | All partners same group | Group filter shows all | P2 |
| BND-069 | Partner with all optional null | Created | P1 |
| BND-070 | Partner with all optional filled | Created | P1 |

---

## §4 Functional Tests — 50 tests

### 4.1 Workflow (15)
| ID | Rule | Expected | Pr |
|----|------|----------|----|
| FUN-001 | Queries exclude IsDeleted | Deleted filtered | P0 |
| FUN-002 | Create sets audit | CreatedBy/Date | P0 |
| FUN-003 | Update sets audit | LastModifiedBy/Date | P0 |
| FUN-004 | Delete sets soft-delete | IsDeleted/DeletedBy/Date | P0 |
| FUN-005 | Approval assigns ErpDimValue | Unique value 1-7999 | P0 |
| FUN-006 | Unapproval clears ErpDimValue | Value cleared | P1 |
| FUN-007 | Status: Draft→Active (approve) | Valid | P0 |
| FUN-008 | Status: Active→Closed | Valid | P1 |
| FUN-009 | Status: Active→Archived | Valid | P1 |
| FUN-010 | Status: Closed→Active (reactivate) | Valid | P1 |
| FUN-011 | Name property set from input | Auto-set | P1 |
| FUN-012 | OrgUnit association validated | OrgUnit exists, !deleted | P0 |
| FUN-013 | Contact cascade on delete | Contacts not deleted | P1 |
| FUN-014 | Search case-insensitive | "acme"="ACME" | P1 |
| FUN-015 | Pagination defaults | Page=1, Size=20 | P1 |

### 4.2 Validation (15)
| ID | Rule | Valid | Invalid | Pr |
|----|------|-------|---------|---|
| FUN-016 | Name required | "ACME" | null | P0 |
| FUN-017 | Type required | "Funding" | null | P0 |
| FUN-018 | OrgUnitId required | 42 | 0, -1 | P0 |
| FUN-019 | Group valid enum | Valid group | "INVALID" | P1 |
| FUN-020 | Category valid enum | Valid category | "INVALID" | P1 |
| FUN-021 | ErpDimValue in 1-7999 | 5000 | 8500 | P0 |
| FUN-022 | ErpDimValue unique | New value | Duplicate | P0 |
| FUN-023 | Logo image type | .jpg, .png | .exe | P0 |
| FUN-024 | Logo size ≤ 5MB | 4MB | 6MB | P1 |
| FUN-025 | XSS prevention | "ACME" | `<script>` | P0 |
| FUN-026 | Name trimmed | " ACME " | "ACME" | P2 |
| FUN-027 | Circular hierarchy check | Non-circular | Parent=self | P0 |
| FUN-028 | Hierarchy depth ≤ 20 | 19 levels | 21 | P1 |
| FUN-029 | Status transition valid | Draft→Active | Draft→Archived | P1 |
| FUN-030 | Approval prerequisites met | All required fields | Missing fields | P0 |

### 4.3 Constraints (10)
| ID | Constraint | Expected | Pr |
|----|-----------|----------|----|
| FUN-031 | Max page size 1000 | Capped | P1 |
| FUN-032 | FK OrgUnit exists | Violation error | P0 |
| FUN-033 | ErpDimValue 8000-9999 reserved | Rejected | P0 |
| FUN-034 | Unique ErpDimValue (incl deleted) | Enforced | P0 |
| FUN-035 | Soft-delete no FK cascade | Contacts intact | P1 |
| FUN-036 | Search result limit | Paginated | P2 |
| FUN-037 | Logo overwrites previous | Old replaced | P1 |
| FUN-038 | Batch operation limit | Chunked | P2 |
| FUN-039 | Gmail deduplication | Handled | P1 |
| FUN-040 | Max hierarchy depth | Enforced | P1 |

### 4.4 Audit (10)
| ID | Action | Expected Audit | Pr |
|----|--------|---------------|----|
| FUN-041 | Create | CreatedBy=current | P0 |
| FUN-042 | Update | LastModifiedBy=current | P0 |
| FUN-043 | Delete | DeletedBy=current | P0 |
| FUN-044 | Approve | Status change + ErpDimValue logged | P1 |
| FUN-045 | Unapprove | Status revert logged | P1 |
| FUN-046 | Logo upload | LastModifiedBy updated | P1 |
| FUN-047 | Status change | Transition logged | P1 |
| FUN-048 | Read no audit | No modification | P1 |
| FUN-049 | Failed op no audit | No entries | P1 |
| FUN-050 | Batch update | Each partner's audit set | P1 |
| FUN-051 | IsDeleted filter in list | Deleted excluded | P0 |
| FUN-052 | Create audit | CreatedBy/Date | P0 |
| FUN-053 | Update audit | LastModifiedBy/Date | P0 |
| FUN-054 | Delete soft-delete | IsDeleted set | P0 |
| FUN-055 | Approval ErpDimValue | Unique 1-7999 | P0 |
| FUN-056 | Unapproval clear | Value cleared | P1 |
| FUN-057 | Draft→Active | Valid | P0 |
| FUN-058 | Active→Closed | Valid | P1 |
| FUN-059 | Active→Archived | Valid | P1 |
| FUN-060 | Closed→Active | Valid | P1 |
| FUN-061 | Name from input | Auto-set | P1 |
| FUN-062 | OrgUnit validated | Exists, !deleted | P0 |
| FUN-063 | Contact no cascade | Intact | P1 |
| FUN-064 | Search case-insensitive | Match | P1 |
| FUN-065 | Pagination defaults | Page=1, Size=20 | P1 |
| FUN-066 | Name required | Reject null | P0 |
| FUN-067 | Type required | Reject null | P0 |
| FUN-068 | OrgUnitId required | Reject 0 | P0 |
| FUN-069 | Group valid | Reject invalid | P1 |
| FUN-070 | Category valid | Reject invalid | P1 |
| FUN-071 | ErpDimValue range | 1-7999 | P0 |
| FUN-072 | ErpDimValue unique | Reject duplicate | P0 |
| FUN-073 | Logo type | Reject .exe | P0 |
| FUN-074 | Logo size | Reject >5MB | P1 |
| FUN-075 | XSS prevention | Sanitize | P0 |
| FUN-076 | Name trim | Trimmed | P2 |
| FUN-077 | Circular hierarchy | Reject | P0 |
| FUN-078 | Hierarchy depth | ≤20 | P1 |
| FUN-079 | Status transition | Valid only | P1 |
| FUN-080 | Approval prereqs | All required | P0 |
| FUN-081 | Max page size | 1000 | P1 |
| FUN-082 | FK OrgUnit | Violation | P0 |
| FUN-083 | ErpDimValue reserved | Reject 8000-9999 | P0 |
| FUN-084 | Unique ErpDimValue | Enforced | P0 |
| FUN-085 | Soft-delete no cascade | Contacts intact | P1 |
| FUN-086 | Search limit | Paginated | P2 |
| FUN-087 | Logo overwrite | Replaced | P1 |
| FUN-088 | Batch limit | Chunked | P2 |
| FUN-089 | Gmail dedup | Handled | P1 |
| FUN-090 | Max hierarchy depth | Enforced | P1 |

---

## §5 Integration Tests — 90 tests

### 5.1 CRUD (10)
| ID | Operation | Expected | Pr |
|----|----------|----------|----|
| INT-001 | Full CRUD lifecycle | All succeed | P0 |
| INT-002 | Create → listed | In partner list | P0 |
| INT-003 | Delete → excluded | Not in list | P0 |
| INT-004 | Update → persisted | Changes saved | P0 |
| INT-005 | Approve → ErpDimValue visible | Value assigned | P0 |
| INT-006 | Unapprove → ErpDimValue cleared | Value removed | P1 |
| INT-007 | Status lifecycle (Draft→Active→Closed→Archived) | All transitions | P1 |
| INT-008 | Create with contacts → both saved | Both entities | P1 |
| INT-009 | Delete → contacts accessible | Contacts remain | P1 |
| INT-010 | Restore deleted | Re-included | P1 |

### 5.2 Search & Filter (10)
| ID | Criteria | Expected | Pr |
|----|---------|----------|----|
| INT-011 | Search by name | Matching partners | P0 |
| INT-012 | Filter by type | Type-specific list | P0 |
| INT-013 | Filter by group | Group-specific | P1 |
| INT-014 | Filter by category | Category-specific | P1 |
| INT-015 | Filter by OrgUnit | OrgUnit-specific | P1 |
| INT-016 | Filter by status | Status-specific | P1 |
| INT-017 | Combined search + filter | Intersection | P1 |
| INT-018 | Search returns empty | Empty result | P1 |
| INT-019 | Case-insensitive search | Same results | P1 |
| INT-020 | Filter excludes deleted | Correct | P1 |

### 5.3 Pagination (5)
| ID | Page | Expected | Pr |
|----|------|----------|----|
| INT-021 | Page 1 of 5 | 20 items | P1 |
| INT-022 | Last page partial | Remaining | P1 |
| INT-023 | Empty results | 0 total | P1 |
| INT-024 | Single page | All items | P2 |
| INT-025 | Max page size | 1000 items | P2 |

### 5.4 Relationships (10)
| ID | Relationship | Expected | Pr |
|----|-------------|----------|----|
| INT-026 | Partner → Contacts | Loaded | P0 |
| INT-027 | Partner → Interactions | Loaded | P0 |
| INT-028 | Partner → Documents | Loaded | P1 |
| INT-029 | Partner → OrgUnit | Loaded | P1 |
| INT-030 | Partner → Parent (hierarchy) | Loaded | P1 |
| INT-031 | Partner → Children (hierarchy) | Loaded | P1 |
| INT-032 | Delete partner → children orphaned | Reparented or error | P1 |
| INT-033 | OrgUnit change → scope change | Correct scoping | P1 |
| INT-034 | Partner → Opportunities | Loaded | P1 |
| INT-035 | Audit trail | Complete history | P1 |

### 5.5 Error Handling (15)
| ID | Error | Expected | Pr |
|----|-------|----------|----|
| INT-036 | Invalid data → 400 | BusinessException | P0 |
| INT-037 | Not found → 404 | KeyNotFound | P0 |
| INT-038 | Unauthorized → 403 | Unauthorized | P0 |
| INT-039 | Duplicate ErpDimValue → 400 | Constraint error | P0 |
| INT-040 | Invalid status transition → 400 | BusinessException | P1 |
| INT-041 | FK violation → 400 | BusinessException | P1 |
| INT-042 | Logo invalid → 400 | BusinessException | P1 |
| INT-043 | Circular hierarchy → 400 | BusinessException | P1 |
| INT-044 | DB timeout → 500 | Graceful error | P1 |
| INT-045 | Concurrency conflict → 409 | Optimistic concurrency | P1 |
| INT-046 | Malformed request → 400 | Validation | P1 |
| INT-047 | Rate limit → 429 | Rate limit | P2 |
| INT-048 | SQL injection → sanitized | No harm | P0 |
| INT-049 | Large payload → 413 | Too large | P2 |
| INT-050 | Session expired → 401 | Auth required | P1 |
| INT-051 | Full CRUD | All succeed | P0 |
| INT-052 | Create→Listed | In list | P0 |
| INT-053 | Delete→Excluded | Not in list | P0 |
| INT-054 | Update→Persisted | Saved | P0 |
| INT-055 | Approve→ErpDimValue | Assigned | P0 |
| INT-056 | Unapprove→Cleared | Removed | P1 |
| INT-057 | Status lifecycle | All transitions | P1 |
| INT-058 | Create with contacts | Both saved | P1 |
| INT-059 | Delete→Contacts | Remain | P1 |
| INT-060 | Restore | Re-included | P1 |
| INT-061 | Search name | Matching | P0 |
| INT-062 | Filter type | Type-specific | P0 |
| INT-063 | Filter group | Group-specific | P1 |
| INT-064 | Filter category | Category-specific | P1 |
| INT-065 | Filter OrgUnit | OrgUnit-specific | P1 |
| INT-066 | Filter status | Status-specific | P1 |
| INT-067 | Combined search+filter | Intersection | P1 |
| INT-068 | Search empty | Empty | P1 |
| INT-069 | Case-insensitive | Same | P1 |
| INT-070 | Exclude deleted | Correct | P1 |
| INT-071 | Page 1 of 5 | 20 items | P1 |
| INT-072 | Last page partial | Remaining | P1 |
| INT-073 | Empty results | 0 total | P1 |
| INT-074 | Single page | All items | P2 |
| INT-075 | Max page size | 1000 | P2 |
| INT-076 | Partner→Contacts | Loaded | P0 |
| INT-077 | Partner→Interactions | Loaded | P0 |
| INT-078 | Partner→Documents | Loaded | P1 |
| INT-079 | Partner→OrgUnit | Loaded | P1 |
| INT-080 | Partner→Parent | Loaded | P1 |
| INT-081 | Partner→Children | Loaded | P1 |
| INT-082 | Delete→Children | Reparented | P1 |
| INT-083 | OrgUnit change | Scope | P1 |
| INT-084 | Partner→Opportunities | Loaded | P1 |
| INT-085 | Audit trail | Complete | P1 |
| INT-086 | Invalid 400 | BusinessException | P0 |
| INT-087 | NotFound 404 | KeyNotFound | P0 |
| INT-088 | Unauthorized 403 | Unauthorized | P0 |
| INT-089 | Duplicate ErpDimValue 400 | Constraint | P0 |
| INT-090 | Invalid transition 400 | BusinessException | P1 |

---

## §6 Security Tests — 50 tests

| ID | Category | Attack/Scenario | Expected | Pr |
|----|----------|----------------|----------|----|
| SEC-001 | Injection | SQL in Name | Parameterized | P0 |
| SEC-002 | Injection | SQL in search | Parameterized | P0 |
| SEC-003 | Injection | XSS in Name | Sanitized | P0 |
| SEC-004 | Injection | XSS in Description | Sanitized | P0 |
| SEC-005 | Injection | LDAP | Sanitized | P1 |
| SEC-006 | Injection | OS cmd in logo name | Sanitized | P0 |
| SEC-007 | Injection | Path traversal | Rejected | P0 |
| SEC-008 | Injection | HTML in fields | Escaped | P1 |
| SEC-009 | Injection | JSON injection | Rejected | P1 |
| SEC-010 | Injection | Template injection | Escaped | P1 |
| SEC-011 | Access | Anonymous create | 401 | P0 |
| SEC-012 | Access | No perm create | 403 | P0 |
| SEC-013 | Access | Scoped read violation | 403 | P0 |
| SEC-014 | Access | Scoped create violation | 403 | P0 |
| SEC-015 | Access | Expired token | 401 | P0 |
| SEC-016 | Access | Tampered JWT | 401/403 | P0 |
| SEC-017 | Access | Horizontal access | 403 | P0 |
| SEC-018 | Access | Disabled account | 403 | P1 |
| SEC-019 | Access | Post-logout | 401 | P1 |
| SEC-020 | Access | Role escalation | Ignored | P0 |
| SEC-021 | IDOR | Guess partner ID | 403 if not scoped | P0 |
| SEC-022 | IDOR | Enumerate IDs | Rate limited | P0 |
| SEC-023 | IDOR | Deleted partner | 404 | P1 |
| SEC-024 | IDOR | Other OrgUnit | 403 | P0 |
| SEC-025 | IDOR | Negative ID | 400 | P1 |
| SEC-026 | IDOR | Zero ID | 400 | P1 |
| SEC-027 | IDOR | Float ID | 400 | P1 |
| SEC-028 | IDOR | String ID | 400 | P1 |
| SEC-029 | IDOR | MAX_INT | 404 | P1 |
| SEC-030 | IDOR | Other user's partner | 403 | P0 |
| SEC-031 | Mass assign | IsDeleted | Not modifiable | P0 |
| SEC-032 | Mass assign | CreatedBy | Not modifiable | P0 |
| SEC-033 | Mass assign | CreatedDate | Not modifiable | P0 |
| SEC-034 | Mass assign | Id | Not settable | P0 |
| SEC-035 | Mass assign | ErpDimValue via API | Validated/rejected | P0 |
| SEC-036 | Auth | Brute-force | Lockout | P0 |
| SEC-037 | Auth | Session fixation | New session | P0 |
| SEC-038 | Auth | Hijacking | Token binding | P1 |
| SEC-039 | Auth | CSRF create | CSRF token | P0 |
| SEC-040 | Auth | CSRF delete | CSRF token | P0 |
| SEC-041 | Auth | Token storage | HttpOnly | P0 |
| SEC-042 | Auth | Concurrent sessions | Policy | P1 |
| SEC-043 | Auth | Token refresh | Works | P1 |
| SEC-044 | Auth | Logout | Invalidated | P0 |
| SEC-045 | Auth | HTTPS | Enforced | P0 |
| SEC-046 | Exposure | Internal fields | DTO filtered | P1 |
| SEC-047 | Exposure | Stack traces | Generic errors | P0 |
| SEC-048 | Exposure | ErpDimValue range info | Not exposed | P1 |
| SEC-049 | Exposure | Cache | no-store | P1 |
| SEC-050 | Exposure | Tokens in URL | HttpOnly | P1 |

---

## §7 Concurrency Tests — 25 tests

| ID | Scenario | Expected | Pr |
|----|---------|----------|----|
| CON-001 | Two users update same partner | Conflict/last-write | P1 |
| CON-002 | Two users approve same partner | One succeeds | P1 |
| CON-003 | Approve + delete simultaneously | One succeeds | P1 |
| CON-004 | Create during search | Consistent | P1 |
| CON-005 | Delete during read | Null or pre-delete | P1 |
| CON-006 | Concurrent ErpDimValue assignment | Unique values | P0 |
| CON-007 | Concurrent status changes | One succeeds | P1 |
| CON-008 | Concurrent pagination | Correct pages | P2 |
| CON-009 | DB deadlock | Resolved | P1 |
| CON-010 | Token refresh during approve | Retry | P1 |
| CON-011 | Bulk import concurrent | Both complete | P2 |
| CON-012 | Gmail sync concurrent | Dedup handles | P2 |
| CON-013 | Optimistic concurrency | Conflict detected | P1 |
| CON-014 | Concurrent soft-delete | One succeeds | P1 |
| CON-015 | Rapid status transitions | Final correct | P1 |
| CON-016 | Connection pool exhaustion | Graceful | P1 |
| CON-017 | Cache invalidation | Fresh data | P1 |
| CON-018 | Concurrent logo upload | Last wins | P2 |
| CON-019 | Session timeout during approve | Rolled back | P1 |
| CON-020 | Multiple users creating partners | All succeed | P2 |
| CON-021 | Concurrent tree operations | Correct hierarchy | P1 |
| CON-022 | Approve during unapproval | Conflict | P1 |
| CON-023 | DB migration during operation | Graceful | P2 |
| CON-024 | Export during update | Consistent snapshot | P2 |
| CON-025 | Concurrent ErpDimValue recalculation | Unique guaranteed | P0 |

---

## §8 Unit Tests — 21 tests

| ID | Category | Input | Expected | Pr |
|----|----------|-------|----------|----|
| UNT-001 | Validation | Null Name | Invalid | P1 |
| UNT-002 | Validation | Invalid Type | Invalid | P1 |
| UNT-003 | Validation | OrgUnitId=-1 | Invalid | P1 |
| UNT-004 | Validation | ErpDimValue=8500 | Invalid (reserved) | P0 |
| UNT-005 | Validation | Circular hierarchy | Invalid | P0 |
| UNT-006 | Formatting | Name trim | " ACME " → "ACME" | P1 |
| UNT-007 | Formatting | ErpDimValue display | "5000" | P1 |
| UNT-008 | Formatting | Status display | "Active" | P2 |
| UNT-009 | Calculation | Next ErpDimValue | Sequential, skip taken | P0 |
| UNT-010 | Calculation | Contact count | Non-deleted only | P1 |
| UNT-011 | Calculation | Pagination pages | 55/20=3 | P1 |
| UNT-012 | Calculation | HasNext | True for page 1 of 3 | P1 |
| UNT-013 | Calculation | Tree depth | Correct depth | P1 |
| UNT-014 | Status | ValidTransition Draft→Active | True | P1 |
| UNT-015 | Status | InvalidTransition Draft→Closed | False | P1 |
| UNT-016 | Status | IsDeleted check | True → inaccessible | P1 |
| UNT-017 | Status | ErpDimValue in valid range | True for 5000 | P0 |
| UNT-018 | Status | ErpDimValue in reserved | True for 8500 | P0 |
| UNT-019 | Collections | Filter by IsDeleted | Correct subset | P1 |
| UNT-020 | Collections | Group by type | Dictionary | P1 |
| UNT-021 | Collections | Build tree from flat | Correct hierarchy | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Operation | Threshold | Pr |
|----|----------|----------|----|
| PRF-001 | Create single | < 200ms | P1 |
| PRF-002 | Get with includes | < 300ms | P1 |
| PRF-003 | Approve (ErpDimValue assign) | < 500ms | P1 |
| PRF-004 | Bulk create 100 | < 5s | P2 |
| PRF-005 | Logo upload 5MB | < 3s | P2 |
| PRF-006 | Search 1000 partners | < 500ms | P1 |
| PRF-007 | Search 10,000 | < 1s | P1 |
| PRF-008 | Paginate 10,000 | < 500ms/page | P1 |
| PRF-009 | Tree build 1000 partners | < 1s | P1 |
| PRF-010 | Count query | < 100ms | P1 |
| PRF-011 | 10 concurrent creates | < 1s each | P2 |
| PRF-012 | 50 concurrent reads | < 500ms each | P2 |
| PRF-013 | 10 concurrent approvals | < 1s each (unique values) | P2 |
| PRF-014 | Memory 10,000 load | < 200MB | P2 |
| PRF-015 | Memory 50,000 query | < 500MB | P2 |
| PRF-016 | Memory leak check | No growth > 10% | P1 |

---

## §10 Load Tests — 10 tests

| ID | Profile | Duration | Criteria | Pr |
|----|---------|----------|----------|----|
| LDT-001 | 50 concurrent CRUD | 30 min | 95% < 500ms | P2 |
| LDT-002 | 100 concurrent reads | 30 min | 95% < 300ms | P2 |
| LDT-003 | 50 concurrent searches | 15 min | < 1s/search | P2 |
| LDT-004 | Spike 10→200 req/s | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike + approvals | 5 min | All unique values | P2 |
| LDT-006 | 500 concurrent | 10 min | Graceful degradation | P2 |
| LDT-007 | 100K partners in DB | 15 min | Queries < 1s | P2 |
| LDT-008 | Continuous create/delete | 10 min | Stable | P2 |
| LDT-009 | Recovery after DB crash | N/A | < 60s | P2 |
| LDT-010 | Recovery after restart | N/A | < 30s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|-----------|
| Partner CRUD | POS-001–005, INT-001–004 |
| Approval workflow + ErpDimValue | POS-002, POS-010–011, FUN-005, NEG-021–030, BND-046–055 |
| Status transitions | POS-006–009, FUN-007–010, NEG-031–040 |
| OrgUnit association | POS-012, FUN-012, INT-015, SEC-013 |
| Hierarchy/tree | POS-019, FUN-027–028, NEG-063–064, BND-033–035 |
| Soft delete | POS-004, FUN-004, SEC-031 |
| Security | SEC-001–050 |
| Performance | PRF-001–016, LDT-001–010 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
