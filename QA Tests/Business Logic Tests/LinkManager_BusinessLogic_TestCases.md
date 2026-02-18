# LinkManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/LinkManager`  
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

LinkManager manages **link CRUD** (create, read, update, delete) with **multi-entity support**: Links can be attached to Partner, Contact, PartnerTree (Interaction, Opportunity if enum extended). Key business rules: **URL validation** (valid URL format), **EntityType + EntityId pairing** (correct association), **soft-delete** (IsDeleted flag), **pagination** (GetEntityLinks with page/pageSize), **ordering** (by CreatedDate descending), **audit fields** (CreatedBy, LastModifiedBy populated), **Name property** (required for ModifiableDeletableEntity, defaults to URL), **duplicate URL detection per entity** (optional), **link title/name** (optional description), **maximum URL length** handling (2000 chars).

---

## §1 Positive Tests (Happy Path) — 30 tests

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create link — Partner | Partner Id=123 exists | CreateLinkAsync(Entity=Partner, EntityId=123, Url="https://example.com") | Link created, Id generated | P0 |
| POS-002 | Create link — Contact | Contact Id=456 exists | CreateLinkAsync(Entity=Contact, EntityId=456, Url="https://example.com") | Link created | P0 |
| POS-003 | Create link — PartnerTree | PartnerTree Id=789 exists | CreateLinkAsync(Entity=PartnerTree, EntityId=789, Url="https://example.com") | Link created | P0 |
| POS-004 | Create — name defaults to URL | Name=null | CreateLinkAsync(Url="https://example.com", Name=null) | entity.Name=Url | P0 |
| POS-005 | Create — explicit name | Name="Company Website" | CreateLinkAsync(Url="...", Name="Company Website") | Name saved | P1 |
| POS-006 | Get link by ID | Link Id=10 exists, !IsDeleted | GetLink(10) | LinkModel returned | P0 |
| POS-007 | Update link URL | Link exists | UpdateLinkAsync(Id=10, Url="https://new.com") | URL updated | P0 |
| POS-008 | Update link name | Link exists | UpdateLinkAsync(Id=10, Name="Updated") | Name updated | P0 |
| POS-009 | Delete link | Link exists | DeleteLinkAsync(10) | IsDeleted=true (soft-delete) | P0 |
| POS-010 | Get entity links — Partner | Partner 123 has 5 links | GetEntityLinks(Partner, 123, request) | Paginated, 5 total, !IsDeleted | P0 |
| POS-011 | Get entity links — Contact | Contact 456 has 3 links | GetEntityLinks(Contact, 456, request) | 3 links returned | P0 |
| POS-012 | Get entity links — PartnerTree | PartnerTree 789 has 2 links | GetEntityLinks(PartnerTree, 789, request) | 2 links returned | P0 |
| POS-013 | Get all links | Links exist | GetLinks() | All non-deleted links | P1 |
| POS-014 | URL validation — https | https://example.com | CreateLinkAsync | Accepted | P0 |
| POS-015 | URL validation — http | http://example.com | CreateLinkAsync | Accepted | P1 |
| POS-016 | EntityType + EntityId pair | Partner 123 exists | CreateLinkAsync(Partner, 123, url) | Link.Entity=Partner, Link.EntityId=123 | P0 |
| POS-017 | Audit fields on create | Create | CreatedBy, CreatedDate set | P0 |
| POS-018 | Audit fields on update | Update | LastModifiedBy, LastModifiedDate set | P0 |
| POS-019 | Pagination page 1 | 100 links for entity | GetEntityLinks(page=1, size=20) | 20 records, TotalCount=100 | P1 |
| POS-020 | Ordering by CreatedDate desc | Links exist | GetEntityLinks | Newest first | P1 |
| POS-021 | Full CRUD cycle | None | Create→Get→Update→Get→Delete | All succeed | P0 |
| POS-022 | Name optional (title) | Name="Documentation" | CreateLinkAsync | Name stored | P1 |
| POS-023 | Multiple links per entity | Partner has 10 links | GetEntityLinks(Partner, id) | 10 returned | P1 |
| POS-024 | Empty entity links | Entity has no links | GetEntityLinks | Records=[], TotalCount=0 | P1 |
| POS-025 | Orphan handling — entity deleted | Partner deleted | GetLink(id) | Null, link auto-deleted | P1 |
| POS-026 | Update — name defaults to URL | Update with Name=null | UpdateLinkAsync | entity.Name=Url | P1 |
| POS-027 | GetEntityLinks — entity not found | Partner 99999 not exist | GetEntityLinks(Partner, 99999) | Records=[], TotalCount=0 | P1 |
| POS-028 | URL with path | https://example.com/docs | CreateLinkAsync | Accepted | P1 |
| POS-029 | URL with query | https://example.com?q=1 | CreateLinkAsync | Accepted | P2 |
| POS-030 | Map entity to model | GetLink | LinkModel with Id, Entity, EntityId, Url, Name | P2 |

---

## §2 Negative Tests — 90 tests

### 2.1 Invalid Input (20)
| ID | Invalid Input | Expected | Priority |
|----|--------------|----------|----------|
| NEG-001 | Entity not exist | CreateLinkAsync(Partner, 99999, url) | ArgumentException: Partner with id 99999 not found | P0 |
| NEG-002 | Null URL | CreateLinkAsync(Url=null) | Validation/ArgumentNull | P0 |
| NEG-003 | Empty URL | CreateLinkAsync(Url="") | Validation error | P0 |
| NEG-004 | Invalid URL format | Url="not-a-url" | Validation error | P0 |
| NEG-005 | Unsupported entity type | Entity=Interaction (if not in enum) | ArgumentException: Unsupported entity type | P0 |
| NEG-006 | Get — ID zero | GetLink(0) | Null | P1 |
| NEG-007 | Get — ID negative | GetLink(-1) | Null | P1 |
| NEG-008 | Update — non-existent | UpdateLinkAsync(Id=99999) | Null | P1 |
| NEG-009 | Update — null model | UpdateLinkAsync(null) | ArgumentNullException | P0 |
| NEG-010 | Delete — non-existent | DeleteLinkAsync(99999) | Graceful (no-op) | P1 |
| NEG-011 | EntityId zero | CreateLinkAsync(Partner, 0, url) | ArgumentException | P0 |
| NEG-012 | EntityId negative | CreateLinkAsync(Partner, -1, url) | ArgumentException | P0 |
| NEG-013 | Null request object | CreateLinkAsync(null) | ArgumentNullException | P0 |
| NEG-014 | Update — missing Id | UpdateLinkRequest with Id=0 | Null or error | P1 |
| NEG-015 | Malformed URL | Url="htxp://example" | Validation error | P0 |
| NEG-016 | Relative URL | Url="/path" | Rejected | P1 |
| NEG-017 | javascript: URL | Url="javascript:alert(1)" | Rejected | P0 |
| NEG-018 | data: URL | Url="data:text/html,<script>" | Rejected | P0 |
| NEG-019 | file: URL | Url="file:///etc/passwd" | Rejected | P0 |
| NEG-020 | SQL injection in URL | Url="'; DROP TABLE--" | Parameterized | P0 |

### 2.2 Unauthorized Access (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-021 | No auth token | CreateLinkAsync | 401 Unauthorized | P0 |
| NEG-022 | No create permission | CreateLinkAsync | 403 Forbidden | P0 |
| NEG-023 | No read permission | GetLink | 403 Forbidden | P0 |
| NEG-024 | No update permission | UpdateLinkAsync | 403 Forbidden | P0 |
| NEG-025 | No delete permission | DeleteLinkAsync | 403 Forbidden | P0 |
| NEG-026 | Expired JWT | Any operation | 401 | P0 |
| NEG-027 | Tampered JWT | Any operation | 401 | P0 |
| NEG-028 | IDOR GetLink | User A gets User B's link | 403 or filtered | P0 |
| NEG-029 | IDOR Update | User A updates User B's link | 403 | P0 |
| NEG-030 | IDOR Delete | User A deletes User B's link | 403 | P0 |
| NEG-031 | Org scope bypass | User OrgB gets Partner A's links | 403 or filtered | P0 |
| NEG-032 | Disabled account | Any operation | 401/403 | P1 |
| NEG-033 | Post-logout | Any operation | 401 | P1 |
| NEG-034 | Role escalation | Include admin in token | Ignored | P0 |
| NEG-035 | Anonymous GetEntityLinks | No auth | 401 | P0 |

### 2.3 Entity & State (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-036 | Contact not exist | CreateLinkAsync(Contact, 99999, url) | ArgumentException | P0 |
| NEG-037 | Partner not exist | CreateLinkAsync(Partner, 99999, url) | ArgumentException | P0 |
| NEG-038 | PartnerTree not exist | CreateLinkAsync(PartnerTree, 99999, url) | ArgumentException | P0 |
| NEG-039 | Entity soft-deleted | Partner IsDeleted=true | CreateLinkAsync | ArgumentException | P1 |
| NEG-040 | Get deleted link | Link IsDeleted=true | GetLink(id) | Null | P1 |
| NEG-041 | Update deleted link | Link IsDeleted=true | UpdateLinkAsync | Null or error | P1 |
| NEG-042 | Delete already deleted | Link IsDeleted=true | DeleteLinkAsync | Idempotent | P1 |
| NEG-043 | GetEntityLinks — deleted links excluded | Links IsDeleted=true | GetEntityLinks | Excluded | P0 |
| NEG-044 | ValidateEntityExists — Contact | Contact 99999 not in DB | ArgumentException | P0 |
| NEG-045 | ValidateEntityExists — Partner | Partner 99999 not in DB | ArgumentException | P0 |
| NEG-046 | ValidateEntityExists — PartnerTree | PartnerTree 99999 not in DB | ArgumentException | P0 |
| NEG-047 | Entity deleted after create | Create link, then delete Partner | GetLink → null, link deleted | P1 |
| NEG-048 | Entity deleted during update | Update link, Partner deleted | ArgumentException | P1 |
| NEG-049 | Wrong Entity+EntityId pair | Create for Partner 123, EntityId=456 | EntityId must match | P0 |
| NEG-050 | Duplicate URL same entity | Same URL for same Entity+EntityId | Per rule (allowed or rejected) | P1 |

### 2.4 Injection & Sanitization (10)
| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| NEG-051 | XSS in Name | Name="<script>alert(1)</script>" | Sanitized | P0 |
| NEG-052 | XSS in URL | URL with script | Rejected | P0 |
| NEG-053 | SQL injection in Name | Name="'; DROP TABLE--" | Parameterized | P0 |
| NEG-054 | SQL injection in Url | Url with SQL | Parameterized | P0 |
| NEG-055 | Path traversal in URL | Url="file://../../../etc/passwd" | Rejected | P0 |
| NEG-056 | HTML injection in Name | Name="<b>Bold</b>" | Escaped | P1 |
| NEG-057 | Open redirect | Url to phishing | Validation | P1 |
| NEG-058 | SSRF — internal URL | Url="http://localhost/admin" | Per rule | P1 |
| NEG-059 | Control chars in URL | Url with \0 | Rejected | P1 |
| NEG-060 | URL encoding bypass | %00 in URL | Rejected | P0 |

### 2.5 Additional (30)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-061 | URL too long | Url 2001 chars | Validation error | P1 |
| NEG-062 | Name too long | Name 2001 chars | Validation error | P1 |
| NEG-063 | Pagination page=0 | PageIndex=0 | Default to 1 | P2 |
| NEG-064 | Pagination pageSize=-1 | PageSize=-1 | Error or default | P2 |
| NEG-065 | Pagination pageSize>1000 | PageSize=2000 | Capped | P2 |
| NEG-066 | Mass assignment Id on create | Include Id in POST | Ignored | P0 |
| NEG-067 | Mass assignment CreatedBy | Include in request | Ignored | P0 |
| NEG-068 | Mass assignment IsDeleted | Include in request | Ignored | P0 |
| NEG-069 | Null PaginationRequest | GetEntityLinks(request=null) | ArgumentNull or default | P1 |
| NEG-070 | Invalid OrderBy column | OrderBy="InvalidColumn" | Fallback or error | P2 |
| NEG-071 | DB connection lost | CreateLinkAsync | Exception | P1 |
| NEG-072 | DB timeout | CreateLinkAsync | Timeout | P1 |
| NEG-073 | Constraint violation | Duplicate constraint | BusinessException | P1 |
| NEG-074 | AutoMapper missing mapping | Map failure | MappingException | P2 |
| NEG-075 | Multiple validation errors | Url=null, EntityId=0 | All returned | P1 |
| NEG-076 | Update — entity not exist | Update with EntityId=99999 | ArgumentException | P1 |
| NEG-077 | GetEntityLinks — exception path | ValidateEntityExists throws | Records=[], TotalCount=0 | P1 |
| NEG-078 | Link entity FK violation | Invalid EntityId | ArgumentException | P0 |
| NEG-079 | Name null on create | Name=null | Defaults to Url | P1 |
| NEG-080 | Name empty on create | Name="" | Defaults to Url | P1 |
| NEG-081 | Url with credentials | http://user:pass@host.com | Sanitized or accepted | P1 |
| NEG-082 | IPv6 URL | http://[::1] | Accepted or rejected per rule | P2 |
| NEG-083 | Unicode URL (IDN) | https://münchen.de | Handled | P2 |
| NEG-084 | Protocol-relative URL | //example.com | Per rule | P2 |
| NEG-085 | Localhost URL | http://localhost | Per rule | P2 |
| NEG-086 | Private IP URL | http://192.168.1.1 | Per rule | P2 |
| NEG-087 | Rate limit | Too many creates | 429 | P2 |
| NEG-088 | Malformed JSON request | Invalid body | 400 | P0 |
| NEG-089 | Missing required fields | No Url in request | 400 | P0 |
| NEG-090 | Invalid enum value | Entity=99 | ArgumentException | P1 |

---

## §3 Boundary Tests — 90 tests

### String Lengths (20)
| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Url | 10 | 2000 | ✅ min valid | ✅ 2000 chars | ❌ 2001 chars | P1 |
| BND-002 | Name | 0 | 2000 | ✅ null | ✅ 2000 chars | ❌ 2001 chars | P1 |
| BND-003 | Url 1 char | — | — | "a" (invalid) | — | — | P1 |
| BND-004 | Url 10 chars | — | — | "https://a.co" | — | — | P1 |
| BND-005 | Url 2000 chars | — | — | — | 2000 chars | — | P1 |
| BND-006 | Url 2001 chars | — | — | — | — | Rejected | P1 |
| BND-007 | Name 0 (null) | — | — | null | — | — | P1 |
| BND-008 | Name 1 char | — | — | "A" | — | — | P1 |
| BND-009 | Name 2000 chars | — | — | — | 2000 chars | — | P1 |
| BND-010 | Name 2001 chars | — | — | — | — | Rejected | P1 |
| BND-011 | Url with long path | — | 2000 | — | — | — | P2 |
| BND-012 | Url with long query | — | 2000 | — | — | — | P2 |
| BND-013 | Name with unicode | — | — | 2000 chars | — | — | P2 |
| BND-014 | Url exact max | — | — | 2000 | — | — | P1 |
| BND-015 | Name exact max | — | — | 2000 | — | — | P1 |
| BND-016 | Empty string Url | — | — | "" | — | — | P1 |
| BND-017 | Whitespace Url | — | — | "   " | — | — | P1 |
| BND-018 | Whitespace Name | — | — | "   " | — | — | P2 |
| BND-019 | Url with fragment | #section | — | — | — | — | P2 |
| BND-020 | Url with port | :8080 | — | — | — | — | P2 |

### Numeric (15)
| ID | Field | Min | Max | Zero | Negative | Priority |
|----|-------|-----|-----|------|----------|----------|
| BND-021 | Link Id | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-022 | EntityId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-023 | Page | 1 | 10000 | ❌ | ❌ | P1 |
| BND-024 | PageSize | 1 | 1000 | ❌ | ❌ | P1 |
| BND-025 | Id = 1 | — | — | — | — | P2 |
| BND-026 | Id = MAX_INT | — | — | — | — | P2 |
| BND-027 | EntityId = 1 | — | — | — | — | P1 |
| BND-028 | EntityId = MAX_INT | — | — | — | — | P2 |
| BND-029 | Page 1 | — | — | — | — | P1 |
| BND-030 | Page 10000 | — | — | — | — | P2 |
| BND-031 | PageSize 1 | — | — | — | — | P1 |
| BND-032 | PageSize 1000 | — | — | — | — | P1 |
| BND-033 | Links per entity 0 | — | — | 0 | — | P1 |
| BND-034 | Links per entity 500 | — | — | 500 | — | P1 |
| BND-035 | Pagination skip | 0 | (total-1)*pageSize | — | — | P1 |

### Collections (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-036 | 0 links | GetEntityLinks | Records=[], TotalCount=0 | P1 |
| BND-037 | 1 link | GetEntityLinks | 1 record | P1 |
| BND-038 | Exactly page size (20) | GetEntityLinks page=1, size=20 | 20 records | P1 |
| BND-039 | PageSize + 1 (21) | 21 links | 20 on page 1, 1 on page 2 | P1 |
| BND-040 | 100 links | GetEntityLinks | Paginated | P1 |
| BND-041 | 1000 links | GetEntityLinks | Paginated | P1 |
| BND-042 | Last page 1 item | 21 total, size=20 | 1 on page 2 | P1 |
| BND-043 | Empty GetLinks | No links | Empty list | P1 |
| BND-044 | GetLinks 1 item | 1 link | 1 item | P1 |
| BND-045 | GetLinks 1000 | 1000 links | All returned | P1 |
| BND-046 | Multiple entities same URL | Partner 1, Partner 2, same URL | Both created | P2 |
| BND-047 | Same entity multiple URLs | Partner 1, 10 URLs | 10 links | P1 |
| BND-048 | Pagination total count | 55 links, size=20 | TotalCount=55 | P1 |
| BND-049 | Pagination last page | 55 links, page=3 | 15 records | P1 |
| BND-050 | Pagination beyond last | page=100, 55 total | Empty | P2 |

### Unicode & Special (15)
| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-051 | Name (Arabic) | `رابط` | Stored | P2 |
| BND-052 | Name (Chinese) | `链接` | Stored | P2 |
| BND-053 | Name (Cyrillic) | `Ссылка` | Stored | P2 |
| BND-054 | Name (French) | `Lien & Société` | Preserved | P2 |
| BND-055 | Name (Emoji) | `🔗 Link` | Stored | P2 |
| BND-056 | Url with unicode path | https://example.com/路径 | Handled | P2 |
| BND-057 | Name with apostrophe | "O'Brien's Link" | Preserved | P1 |
| BND-058 | Name with ampersand | "Smith & Co" | Preserved | P1 |
| BND-059 | Url with encoded chars | %20 %2F | Decoded/stored | P2 |
| BND-060 | Url with plus | https://example.com?q=a+b | Stored | P2 |
| BND-061 | Name with newline | "Line1\nLine2" | Per rule | P2 |
| BND-062 | Url IDN | https://münchen.de | Handled | P2 |
| BND-063 | Name 100 chars | 100 chars | Accepted | P1 |
| BND-064 | Url 500 chars | 500 chars | Accepted | P1 |
| BND-065 | Name with RTL | Arabic | Correct display | P2 |

### Entity Type & Date (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-066 | LinkEntityType Contact (0) | CreateLinkAsync(Contact, id, url) | Accepted | P1 |
| BND-067 | LinkEntityType Partner (1) | CreateLinkAsync(Partner, id, url) | Accepted | P1 |
| BND-068 | LinkEntityType PartnerTree (2) | CreateLinkAsync(PartnerTree, id, url) | Accepted | P1 |
| BND-069 | EntityType enum first | Contact | Accepted | P1 |
| BND-070 | EntityType enum last | PartnerTree | Accepted | P1 |
| BND-071 | CreatedDate at midnight UTC | Create at 00:00 UTC | Correct | P2 |
| BND-072 | LastModifiedDate on update | Update | Updated | P1 |
| BND-073 | DeletedDate on delete | Delete | Set | P1 |
| BND-074 | Leap year date | Create on Feb 29 | Correct | P2 |
| BND-075 | DST transition | Create during DST | Correct | P2 |
| BND-076 | Ordering CreatedDate desc | Multiple links | Newest first | P1 |
| BND-077 | Ordering Name asc | GetEntityLinks OrderBy=Name | A-Z | P1 |
| BND-078 | Ordering Url asc | GetEntityLinks OrderBy=Url | A-Z | P1 |
| BND-079 | Entity+EntityId pair valid | Partner 123 exists | Create succeeds | P0 |
| BND-080 | Entity+EntityId pair invalid | Partner 99999 | ArgumentException | P0 |

### Additional (10)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-081 | Url https | https://example.com | Accepted | P0 |
| BND-082 | Url http | http://example.com | Accepted | P1 |
| BND-083 | Url with subdomain | https://api.example.com | Accepted | P1 |
| BND-084 | Url with port | https://example.com:8443 | Accepted | P2 |
| BND-085 | Url with path | https://example.com/docs | Accepted | P1 |
| BND-086 | Url with query | https://example.com?q=1 | Accepted | P1 |
| BND-087 | All optional null | Name=null | Name=Url | P1 |
| BND-088 | All optional filled | Name="Title" | Stored | P1 |
| BND-089 | Update partial | Update only Url | Name unchanged | P1 |
| BND-090 | Update full | Update Url and Name | Both updated | P1 |

---

## §4 Functional Tests — 90 tests

### 4.1 Workflow (15)
| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Queries exclude IsDeleted | GetLink, GetEntityLinks | Deleted filtered | P0 |
| FUN-002 | Create sets audit | CreateLinkAsync | CreatedBy, CreatedDate | P0 |
| FUN-003 | Update sets audit | UpdateLinkAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-004 | Delete sets soft-delete | DeleteLinkAsync | IsDeleted, DeletedBy, DeletedDate | P0 |
| FUN-005 | Name defaults to URL | Create with Name=null | entity.Name=Url | P0 |
| FUN-006 | Name defaults on update | Update with Name=null | entity.Name=Url | P0 |
| FUN-007 | EntityType + EntityId validated | Create | ValidateEntityExists | P0 |
| FUN-008 | Pagination applied | GetEntityLinks | Skip, Take | P0 |
| FUN-009 | Ordering applied | GetEntityLinks | OrderBy CreatedDate desc | P0 |
| FUN-010 | Name required (ModifiableDeletableEntity) | Create | Name set (Url or explicit) | P0 |
| FUN-011 | Orphan cleanup | Entity deleted | GetLink deletes link | P1 |
| FUN-012 | ValidateEntityExists Contact | Create Contact link | context.Contacts.AnyAsync | P0 |
| FUN-013 | ValidateEntityExists Partner | Create Partner link | context.Partners.AnyAsync | P0 |
| FUN-014 | ValidateEntityExists PartnerTree | Create PartnerTree link | context.PartnerTrees.AnyAsync | P0 |
| FUN-015 | GetEntityLinks empty on invalid entity | Entity 99999 not exist | Records=[], TotalCount=0 | P0 |

### 4.2 Validation (15)
| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | Url required | "https://example.com" | null, "" | P0 |
| FUN-017 | Url format | Valid URL | "not-a-url" | P0 |
| FUN-018 | EntityId required | 123 | 0, -1 | P0 |
| FUN-019 | Entity valid enum | Contact, Partner, PartnerTree | Invalid | P0 |
| FUN-020 | Entity exists | Partner 123 in DB | Partner 99999 | P0 |
| FUN-021 | Url max length | 2000 | 2001 | P1 |
| FUN-022 | Name max length | 2000 | 2001 | P1 |
| FUN-023 | Url scheme | http, https | javascript, file, data | P0 |
| FUN-024 | Id required on update | 10 | 0 | P1 |
| FUN-025 | XSS prevention | "ACME" | "<script>" | P0 |
| FUN-026 | SQL injection prevention | "https://example.com" | "'; DROP" | P0 |
| FUN-027 | Url trim | " https://x.com " | "https://x.com" | P2 |
| FUN-028 | Name trim | " Title " | "Title" | P2 |
| FUN-029 | Pagination PageIndex | 1+ | 0, -1 | P2 |
| FUN-030 | Pagination PageSize | 1-1000 | -1, 2000 | P2 |

### 4.3 Constraints (10)
| ID | Constraint | Expected | Priority |
|----|-----------|----------|----------|
| FUN-031 | Max page size 1000 | Capped | P1 |
| FUN-032 | Url StringLength 2000 | Enforced | P0 |
| FUN-033 | Name StringLength 2000 | Enforced | P1 |
| FUN-034 | Soft-delete no physical delete | Record remains | P0 |
| FUN-035 | Entity FK (logical) | Entity exists | ValidateEntityExists | P0 |
| FUN-036 | Duplicate URL per entity | Per rule | Allowed or rejected | P2 |
| FUN-037 | Paginate extension | GetEntityLinks | Skip, Take applied | P0 |
| FUN-038 | AutoMapper Link→LinkModel | All fields | P1 |
| FUN-039 | AutoMapper LinkRequest→Link | All fields | P1 |
| FUN-040 | ModifiableDeletableEntity base | Name, IsDeleted, audit | P0 |

### 4.4 Audit (10)
| ID | Action | Expected Audit | Priority |
|----|--------|---------------|----------|
| FUN-041 | Create | CreatedBy=current, CreatedDate | P0 |
| FUN-042 | Update | LastModifiedBy=current, LastModifiedDate | P0 |
| FUN-043 | Delete | DeletedBy=current, DeletedDate | P0 |
| FUN-044 | Read | No audit change | P1 |
| FUN-045 | Failed create | No audit entry | P1 |
| FUN-046 | Orphan delete | DeletedBy/Date set | P1 |
| FUN-047 | Update Name | LastModifiedBy updated | P1 |
| FUN-048 | Update Url | LastModifiedBy updated | P1 |
| FUN-049 | Update EntityId | LastModifiedBy updated | P1 |
| FUN-050 | Batch operations | Each link audit | P1 |

### 4.5 Extended Functional (40)
| ID | Rule | Expected | Priority |
|----|------|----------|----------|
| FUN-051 | IsDeleted filter GetLink | null if deleted | P0 |
| FUN-052 | IsDeleted filter GetEntityLinks | Excluded | P0 |
| FUN-053 | IsDeleted filter GetLinks | Excluded | P0 |
| FUN-054 | Create Partner link | Partner exists | Created | P0 |
| FUN-055 | Create Contact link | Contact exists | Created | P0 |
| FUN-056 | Create PartnerTree link | PartnerTree exists | Created | P0 |
| FUN-057 | GetEntityLinks Partner | Partner id | Partner's links | P0 |
| FUN-058 | GetEntityLinks Contact | Contact id | Contact's links | P0 |
| FUN-059 | GetEntityLinks PartnerTree | PartnerTree id | PartnerTree's links | P0 |
| FUN-060 | Pagination default | PageIndex=1, PageSize=20 | P1 |
| FUN-061 | Pagination TotalCount | Correct | P0 |
| FUN-062 | Pagination Records | Correct page | P0 |
| FUN-063 | Ordering default | CreatedDate desc | P0 |
| FUN-064 | Ordering configurable | OrderBy param | Applied | P1 |
| FUN-065 | Name optional | null allowed | Defaults to Url | P0 |
| FUN-066 | Url required | Must provide | Validation | P0 |
| FUN-067 | Entity required | Must provide | Validation | P0 |
| FUN-068 | EntityId required | Must provide | Validation | P0 |
| FUN-069 | Update Id required | Must provide | Validation | P1 |
| FUN-070 | Delete idempotent | Delete non-existent | No error | P1 |
| FUN-071 | Get null for non-existent | GetLink(99999) | null | P0 |
| FUN-072 | Update null for non-existent | UpdateLink(99999) | null | P1 |
| FUN-073 | GetEntityLinks exception handling | ValidateEntityExists throws | Records=[], TotalCount=0 | P0 |
| FUN-074 | Orphan ValidateEntityExists | Entity deleted | ArgumentException on Update | P1 |
| FUN-075 | Link entity Name | From ModifiableDeletableEntity | Required, set | P0 |
| FUN-076 | Link entity Url | Required | Stored | P0 |
| FUN-077 | Link entity Entity | Enum | Stored | P0 |
| FUN-078 | Link entity EntityId | Int | Stored | P0 |
| FUN-079 | Duplicate URL detection | Same Entity+EntityId+Url | Per rule | P2 |
| FUN-080 | Maximum URL length | 2000 | Enforced | P0 |
| FUN-081 | Link title/name | Optional description | Stored | P1 |
| FUN-082 | Controller Create | POST /api/links | 201 | P0 |
| FUN-083 | Controller Update | PUT /api/links | 200 | P0 |
| FUN-084 | Controller Delete | DELETE /api/links/{id} | 204 | P0 |
| FUN-085 | Controller Get | GET /api/links/{id} | 200 | P0 |
| FUN-086 | Controller GetEntityLinks | GET with entity, entityId | 200 paginated | P0 |
| FUN-087 | ManagerWrapper resolution | ILinkManager | P1 |
| FUN-088 | DbContext scope | Per request | P1 |
| FUN-089 | Repository Delete | Soft delete | P0 |
| FUN-090 | Paginate helper | Skip, Take, Count | P0 |

---

## §5 Integration Tests — 90 tests

### 5.1 CRUD (10)
| ID | Operation | Entities | Expected | Priority |
|----|----------|----------|----------|----------|
| INT-001 | Full CRUD lifecycle | Link | Create→Get→Update→Get→Delete | P0 |
| INT-002 | Create → GetLink | Link | In GetLink | P0 |
| INT-003 | Delete → excluded | Link | GetLink returns null | P0 |
| INT-004 | Update → persisted | Link | Changes in GetLink | P0 |
| INT-005 | Create Partner link | Partner, Link | Both saved | P0 |
| INT-006 | Create Contact link | Contact, Link | Both saved | P0 |
| INT-007 | Create PartnerTree link | PartnerTree, Link | Both saved | P0 |
| INT-008 | Delete → GetEntityLinks | Link | Excluded | P0 |
| INT-009 | Create with Name null | Link | Name=Url | P0 |
| INT-010 | Create with Name | Link | Name stored | P1 |

### 5.2 Search & Filter (10)
| ID | Criteria | Expected | Priority |
|----|----------|----------|----------|
| INT-011 | GetEntityLinks Partner | Partner's links | P0 |
| INT-012 | GetEntityLinks Contact | Contact's links | P0 |
| INT-013 | GetEntityLinks PartnerTree | PartnerTree's links | P0 |
| INT-014 | GetLinks all | All non-deleted | P1 |
| INT-015 | Pagination page 1 | 20 items | P1 |
| INT-016 | Pagination last page | Remaining | P1 |
| INT-017 | Ordering CreatedDate desc | Newest first | P1 |
| INT-018 | Empty entity | No links | Records=[], TotalCount=0 | P1 |
| INT-019 | Multiple links | 10 links | 10 returned | P1 |
| INT-020 | Entity not exist | 99999 | Records=[], TotalCount=0 | P1 |

### 5.3 Pagination (5)
| ID | Page/Size | Expected | Priority |
|----|-----------|----------|----------|
| INT-021 | Page 1, Size 20 | 20 records | P1 |
| INT-022 | Last page partial | Remaining | P1 |
| INT-023 | Empty | 0 total | P1 |
| INT-024 | Single page | All items | P2 |
| INT-025 | Max size 1000 | 1000 items | P2 |

### 5.4 Relationships (10)
| ID | Relationship | Expected | Priority |
|----|-------------|----------|----------|
| INT-026 | Link → Partner | Entity=Partner, EntityId | P0 |
| INT-027 | Link → Contact | Entity=Contact, EntityId | P0 |
| INT-028 | Link → PartnerTree | Entity=PartnerTree, EntityId | P0 |
| INT-029 | Partner → Links | GetEntityLinks(Partner, id) | P0 |
| INT-030 | Contact → Links | GetEntityLinks(Contact, id) | P0 |
| INT-031 | PartnerTree → Links | GetEntityLinks(PartnerTree, id) | P0 |
| INT-032 | Delete Partner → Link orphan | GetLink deletes link | P1 |
| INT-033 | Delete Contact → Link orphan | GetLink deletes link | P1 |
| INT-034 | Delete PartnerTree → Link orphan | GetLink deletes link | P1 |
| INT-035 | Audit trail | Create/Update/Delete logged | P1 |

### 5.5 Error Handling (15)
| ID | Error | Expected | Priority |
|----|------|----------|----------|
| INT-036 | Invalid data → 400 | Validation | P0 |
| INT-037 | Not found → 404 | GetLink null | P0 |
| INT-038 | Unauthorized → 403 | Forbidden | P0 |
| INT-039 | Entity not exist → 400 | ArgumentException | P0 |
| INT-040 | Invalid URL → 400 | Validation | P0 |
| INT-041 | Null request → 400 | ArgumentNull | P0 |
| INT-042 | DB timeout → 500 | Exception | P1 |
| INT-043 | Concurrency → 409 | Conflict | P1 |
| INT-044 | Malformed request → 400 | Validation | P0 |
| INT-045 | SQL injection → sanitized | No harm | P0 |
| INT-046 | Rate limit → 429 | 429 | P2 |
| INT-047 | Session expired → 401 | Auth required | P1 |
| INT-048 | Unsupported entity type → 400 | ArgumentException | P0 |
| INT-049 | Constraint violation → 400 | BusinessException | P1 |
| INT-050 | Orphan GetLink → null | Link deleted | P1 |

### 5.6 Extended Integration (40)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-051 | API POST create | 201 Created | P0 |
| INT-052 | API GET by ID | 200 with LinkModel | P0 |
| INT-053 | API PUT update | 200 updated | P0 |
| INT-054 | API DELETE | 204 No Content | P0 |
| INT-055 | API GET entity links | 200 paginated | P0 |
| INT-056 | Controller → LinkManager | Correct resolution | P1 |
| INT-057 | LinkManager → Repository | DataRepository<Link> | P1 |
| INT-058 | AutoMapper LinkRequest→Link | Mapped | P1 |
| INT-059 | AutoMapper Link→LinkModel | Mapped | P1 |
| INT-060 | AutoMapper UpdateLinkRequest→Link | Mapped | P1 |
| INT-061 | ValidateEntityExists Contact | context.Contacts.AnyAsync | P0 |
| INT-062 | ValidateEntityExists Partner | context.Partners.AnyAsync | P0 |
| INT-063 | ValidateEntityExists PartnerTree | context.PartnerTrees.AnyAsync | P0 |
| INT-064 | Paginate extension | Skip, Take | P0 |
| INT-065 | GetEntityLinks catch ArgumentException | Records=[], TotalCount=0 | P0 |
| INT-066 | GetEntityLinks catch Exception | Records=[], TotalCount=0 | P1 |
| INT-067 | Update orphan delete | Entity deleted, link deleted | P1 |
| INT-068 | GetLink orphan delete | Entity deleted, link deleted, null | P1 |
| INT-069 | Create full flow | Request→Validate→Map→Add→Return | P0 |
| INT-070 | Update full flow | Get→Validate→Map→Update→Return | P0 |
| INT-071 | Delete full flow | Get→Delete (soft) | P0 |
| INT-072 | GetEntityLinks full flow | Validate→Query→Paginate→Map | P0 |
| INT-073 | Name default create | Name=null → Url | P0 |
| INT-074 | Name default update | Name=null → Url | P0 |
| INT-075 | Multi-entity Partner | Partner links only | P0 |
| INT-076 | Multi-entity Contact | Contact links only | P0 |
| INT-077 | Multi-entity PartnerTree | PartnerTree links only | P0 |
| INT-078 | PaginationResponse structure | Records, TotalCount | P0 |
| INT-079 | LinkModel structure | Id, Entity, EntityId, Url, Name | P0 |
| INT-080 | LinkRequest structure | Entity, EntityId, Url, Name | P0 |
| INT-081 | UpdateLinkRequest structure | Id, Entity, EntityId, Url, Name | P0 |
| INT-082 | Link entity Name | ModifiableDeletableEntity | P0 |
| INT-083 | Link entity IsDeleted | Soft delete | P0 |
| INT-084 | Link entity audit | CreatedBy, etc. | P0 |
| INT-085 | DbContext Contacts | AnyAsync | P0 |
| INT-086 | DbContext Partners | AnyAsync | P0 |
| INT-087 | DbContext PartnerTrees | AnyAsync | P0 |
| INT-088 | LinkController Create | FromBody LinkRequest | P0 |
| INT-089 | LinkController GetEntityLinks | FromQuery entity, entityId | P0 |
| INT-090 | End-to-end Create→GetEntityLinks→Delete | Full flow | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE)

Security tests are covered in a separate Security test suite. Categories: Injection (10), Access Control (10), IDOR (10), Mass Assignment (5), Auth & Session (10), Data Exposure (5).

---

## §7 Concurrency Tests — 25 tests

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two users update same link | Conflict or last-write | P1 |
| CON-002 | Two users create for same entity | Both succeed | P1 |
| CON-003 | Create during GetEntityLinks | Consistent view | P1 |
| CON-004 | Delete during GetLink | Null or pre-delete | P1 |
| CON-005 | Update during read | Consistent snapshot | P1 |
| CON-006 | Concurrent GetEntityLinks | Both succeed | P1 |
| CON-007 | Concurrent CreateLinkAsync | Both succeed | P1 |
| CON-008 | Delete during Update | Conflict or error | P1 |
| CON-009 | DB deadlock | Resolved | P1 |
| CON-010 | Token refresh during create | Retry | P1 |
| CON-011 | Bulk create concurrent | All complete | P2 |
| CON-012 | Optimistic concurrency | Conflict detected | P1 |
| CON-013 | Concurrent soft-delete | One succeeds | P1 |
| CON-014 | Rapid create/delete | Final state correct | P1 |
| CON-015 | Connection pool exhaustion | Graceful | P1 |
| CON-016 | Multiple users creating links | All succeed | P2 |
| CON-017 | Concurrent GetLink | No interference | P1 |
| CON-018 | Update during delete | Conflict | P1 |
| CON-019 | Entity deleted during create | ArgumentException | P1 |
| CON-020 | Session timeout during update | Rolled back | P1 |
| CON-021 | Concurrent pagination | Correct pages | P2 |
| CON-022 | Create for same entity+url | Per duplicate rule | P1 |
| CON-023 | Parallel GetEntityLinks | No interference | P1 |
| CON-024 | Orphan cleanup concurrent | Handled | P1 |
| CON-025 | Real-time update propagation | Eventually consistent | P2 |

---

## §8 Unit Tests — 21 tests

| ID | Category | Input | Expected | Priority |
|----|----------|-------|----------|----------|
| UNT-001 | Validation | Null Url | Invalid | P1 |
| UNT-002 | Validation | Empty Url | Invalid | P1 |
| UNT-003 | Validation | Invalid URL format | Invalid | P0 |
| UNT-004 | Validation | EntityId=0 | Invalid | P0 |
| UNT-005 | Validation | Unsupported Entity | Invalid | P0 |
| UNT-006 | Formatting | Name trim | " Link " → "Link" | P1 |
| UNT-007 | Formatting | Url trim | " https://x.com " | "https://x.com" | P1 |
| UNT-008 | Formatting | Name default | null → Url | P1 |
| UNT-009 | Calculation | Pagination pages | 55/20=3 | P1 |
| UNT-010 | Calculation | HasNext | True for page 1 of 3 | P1 |
| UNT-011 | Calculation | TotalCount | Correct | P1 |
| UNT-012 | Calculation | Skip | (page-1)*size | P1 |
| UNT-013 | Calculation | Take | pageSize | P1 |
| UNT-014 | Status | IsDeleted check | True → GetLink null | P1 |
| UNT-015 | Status | Entity exists | ValidateEntityExists | P0 |
| UNT-016 | Status | Orphan detection | Entity deleted | P1 |
| UNT-017 | Status | Url scheme valid | http, https | P0 |
| UNT-018 | Status | Url scheme invalid | javascript | P0 |
| UNT-019 | Collections | Filter IsDeleted | Excluded | P1 |
| UNT-020 | Collections | Paginate | Skip, Take | P1 |
| UNT-021 | Collections | Map to LinkModel | All fields | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Operation | Threshold | Priority |
|----|----------|----------|----------|
| PRF-001 | Create single | < 200ms | P1 |
| PRF-002 | GetLink | < 100ms | P1 |
| PRF-003 | GetEntityLinks 100 links | < 500ms | P1 |
| PRF-004 | GetEntityLinks 1000 links | < 1s | P1 |
| PRF-005 | Update single | < 200ms | P1 |
| PRF-006 | Delete single | < 100ms | P1 |
| PRF-007 | GetLinks 1000 | < 1s | P1 |
| PRF-008 | Paginate 10,000 | < 500ms/page | P1 |
| PRF-009 | ValidateEntityExists | < 50ms | P1 |
| PRF-010 | Count query | < 100ms | P1 |
| PRF-011 | 10 concurrent creates | < 1s each | P2 |
| PRF-012 | 50 concurrent reads | < 500ms each | P2 |
| PRF-013 | 10 concurrent GetEntityLinks | < 1s each | P2 |
| PRF-014 | Memory 10,000 links | < 200MB | P2 |
| PRF-015 | Memory 50,000 links | < 500MB | P2 |
| PRF-016 | Memory leak check | No growth > 10% | P1 |

---

## §10 Load Tests — 10 tests

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 50 concurrent CRUD | 30 min | 95% < 500ms | P2 |
| LDT-002 | 100 concurrent reads | 30 min | 95% < 300ms | P2 |
| LDT-003 | 50 concurrent GetEntityLinks | 15 min | < 1s | P2 |
| LDT-004 | Spike 10→200 req/s | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike + creates | 5 min | All succeed | P2 |
| LDT-006 | 500 concurrent | 10 min | Graceful degradation | P2 |
| LDT-007 | 100K links in DB | 15 min | Queries < 1s | P2 |
| LDT-008 | Continuous create/delete | 10 min | Stable | P2 |
| LDT-009 | Recovery after DB crash | N/A | < 60s | P2 |
| LDT-010 | Recovery after restart | N/A | < 30s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|------------|
| Link CRUD | POS-001–009, INT-001–010 |
| Multi-entity (Partner, Contact, PartnerTree) | POS-001–003, POS-010–012, FUN-054–059 |
| URL validation | POS-014–015, NEG-002–004, FUN-016–017 |
| EntityType + EntityId pairing | POS-016, FUN-007, NEG-049 |
| Soft-delete | POS-009, FUN-004, NEG-040–043 |
| Pagination | POS-019, FUN-008, BND-036–050 |
| Ordering CreatedDate desc | POS-020, FUN-009, BND-076 |
| Audit fields | POS-017–018, FUN-002–003, FUN-041–043 |
| Name property | POS-004–005, FUN-005–006, FUN-010 |
| Duplicate URL | NEG-050, FUN-036, BND-046 |
| Maximum URL length | NEG-061, FUN-021, BND-001–006 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
