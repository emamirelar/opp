# DocumentTypeManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DocumentTypeManager`  
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

**DocumentTypeManager** manages CRUD of document types, MIME type mapping, validation rules, and categorization. Key responsibilities: document type definitions, entity type filtering (Partner/Contact/Interaction), MIME type associations, validation rules, pagination, and excluded-deleted filtering.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Get document types — all | Types exist | GetDocumentTypes() | All non-deleted | P0 |
| POS-002 | Get document types — exclude deleted | Mix active/deleted | GetDocumentTypes | Deleted excluded | P0 |
| POS-003 | Get document types — filter by Partner | EntityType=Partner | GetDocumentTypes(Partner) | Partner types only | P0 |
| POS-004 | Get document types — filter by Contact | EntityType=Contact | GetDocumentTypes(Contact) | Contact types only | P0 |
| POS-005 | Get document types — filter by Interaction | EntityType=Interaction | GetDocumentTypes(Interaction) | Interaction types only | P0 |
| POS-006 | Get document types — pagination | 100 types | GetDocumentTypes pagination | Paginated | P0 |
| POS-007 | Get document type by ID | Type 123 exists | GetDocumentType(123) | Type returned | P0 |
| POS-008 | Create document type | Valid data | CreateDocumentType(model) | Type created | P0 |
| POS-009 | Update document type | Type exists | UpdateDocumentType(model) | Type updated | P0 |
| POS-010 | Delete document type — soft | Type exists | DeleteDocumentType(id) | IsDeleted=true | P0 |
| POS-011 | MIME type mapping | Type has MIME | GetDocumentType | MimeType returned | P1 |
| POS-012 | Validation rules | Type has rules | GetDocumentType | Rules returned | P1 |
| POS-013 | Entity type mapping | Type has EntityType | GetDocumentType | EntityType returned | P1 |
| POS-014 | Empty entity type returns all | EntityType=null | GetDocumentTypes | All types | P1 |
| POS-015 | Single result | One type matches | GetDocumentTypes filter | 1 result | P1 |
| POS-016 | Large result set | 100+ types | GetDocumentTypes | All returned | P1 |
| POS-017 | Name property mapped | Get type | DocumentTypeModel | Name correct | P1 |
| POS-018 | Id property mapped | Get type | DocumentTypeModel | Id correct | P1 |
| POS-019 | Case sensitivity entity filter | "partner" vs "Partner" | GetDocumentTypes | Per design | P1 |
| POS-020 | Whitespace in entity filter | "  Partner  " | GetDocumentTypes | Trimmed | P1 |
| POS-021 | Create with MIME types | Create with MIMEs | CreateDocumentType | MIMEs saved | P1 |
| POS-022 | Update MIME mapping | Type exists | Update MIMEs | Updated | P1 |
| POS-023 | Categorization | Type has category | GetDocumentType | Category returned | P1 |
| POS-024 | Get by ID — not found | ID 99999 | GetDocumentType(99999) | Null | P1 |
| POS-025 | Update non-existent | ID 99999 | UpdateDocumentType | Null | P1 |
| POS-026 | Delete non-existent | ID 99999 | DeleteDocumentType | Graceful | P1 |
| POS-027 | Empty database | No types | GetDocumentTypes | Empty list | P1 |
| POS-028 | Full CRUD cycle | None | Create→Get→Update→Get→Delete | All succeed | P0 |
| POS-029 | Multiple MIME types per type | Type has pdf, application/pdf | GetDocumentType | Both | P1 |
| POS-030 | Validation rule — max size | Type has maxSize rule | GetDocumentType | Rule returned | P1 |
| POS-031 | Validation rule — extensions | Type has extensions | GetDocumentType | Extensions returned | P1 |
| POS-032 | Sort by name | Types exist | GetDocumentTypes OrderBy=Name | Sorted | P1 |
| POS-033 | Sort by entity type | Types exist | GetDocumentTypes OrderBy=EntityType | Sorted | P2 |
| POS-034 | Filter by category | Types have categories | GetDocumentTypes category filter | Filtered | P1 |
| POS-035 | Document type in use | Docs reference type | GetDocumentType | Type returned | P1 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create — missing name | Name null/empty | Validation error | P0 |
| NEG-002 | Create — invalid entity type | EntityType="Invalid" | Validation error | P0 |
| NEG-003 | Create — duplicate name | Name exists | Error | P0 |
| NEG-004 | Create — invalid MIME | MimeType="invalid" | Validation error | P0 |
| NEG-005 | Create — null model | CreateDocumentType(null) | ArgumentNullException | P0 |
| NEG-006 | Update — non-existent ID | UpdateDocumentType(99999) | Null | P1 |
| NEG-007 | Update — missing name | Name null | Validation error | P0 |
| NEG-008 | Update — null model | UpdateDocumentType(null) | ArgumentNullException | P0 |
| NEG-009 | Delete — non-existent | DeleteDocumentType(99999) | Graceful | P1 |
| NEG-010 | Delete — already deleted | Type IsDeleted | Idempotent | P1 |
| NEG-011 | Get — ID zero | GetDocumentType(0) | Null | P1 |
| NEG-012 | Get — ID negative | GetDocumentType(-1) | Null | P1 |
| NEG-013 | Get document types — invalid entity | EntityType="Xyz" | Empty or error | P1 |
| NEG-014 | Pagination — invalid page | PageIndex=-1 | Error | P1 |
| NEG-015 | Pagination — zero size | PageSize=0 | Error | P1 |
| NEG-016 | Create — SQL injection in name | '; DROP TABLE-- | Sanitized | P0 |
| NEG-017 | Create — XSS in name | <script>alert(1)</script> | Sanitized | P0 |
| NEG-018 | Unauthorized create | User lacks permission | 403 | P0 |
| NEG-019 | Unauthorized update | User lacks permission | 403 | P0 |
| NEG-020 | Unauthorized delete | User lacks permission | 403 | P0 |
| NEG-021 | IDOR — access other org type | GetDocumentType(otherId) | 403 | P0 |
| NEG-022 | Mass assignment — set Id | Include Id in CreateRequest | Ignored | P0 |
| NEG-023 | Mass assignment — IsDeleted | Include in request | Ignored | P0 |
| NEG-024 | Unauthenticated get | No auth | GetDocumentTypes | 401 | P0 |
| NEG-025 | Expired token | Expired JWT | Any op | 401 | P0 |
| NEG-026 | MIME type format invalid | MimeType="application" | Error | P1 |
| NEG-027 | MIME type too long | MimeType 256 chars | Validation error | P1 |
| NEG-028 | Extension format invalid | Extensions="exe" | Validation or rejected | P1 |
| NEG-029 | Duplicate MIME for type | Same MIME twice | Error | P1 |
| NEG-030 | Entity type null on create | EntityType=null | Error or default | P1 |
| NEG-031 | Validation rule invalid | Rule malformed | Error | P1 |
| NEG-032 | Category invalid | Category="Invalid" | Error | P1 |
| NEG-033 | Delete — type in use | Docs reference type | Business rule | P1 |
| NEG-034 | Update — type in use | Change type with docs | Per design | P1 |
| NEG-035 | Database timeout | Simulate timeout | Exception | P1 |
| NEG-036 | Concurrent update conflict | 2 users update same | Concurrency error | P1 |
| NEG-037 | Name whitespace only | Name="   " | Validation error | P1 |
| NEG-038 | Name too long | Name 256 chars | Validation error | P1 |
| NEG-039 | Description XSS | Description with script | Sanitized | P0 |
| NEG-040 | Sort invalid column | OrderBy="Invalid" | Error | P1 |
| NEG-041 | Filter invalid | Malformed filter | Error | P2 |
| NEG-042 | Rate limit | Too many requests | 429 | P1 |
| NEG-043 | Create — duplicate EntityType+Name | Same combo | Error | P1 |
| NEG-044 | MIME wildcard invalid | MimeType="*/*" | Per design | P2 |
| NEG-045 | Empty MIME list | MimeTypes=[] | Error or default | P1 |
| NEG-046 | Empty extensions list | Extensions=[] | Per design | P1 |
| NEG-047 | Negative max file size | MaxFileSize=-1 | Validation error | P1 |
| NEG-048 | Zero max file size | MaxFileSize=0 | Per design | P1 |
| NEG-049 | Invalid category ID | CategoryId=99999 | Error | P1 |
| NEG-050 | Org scope bypass | User from OrgB get OrgA type | 403 or filtered | P0 |
| NEG-051 | Create — reserved name | Name="System" | Error | P1 |
| NEG-052 | Update — to reserved name | Update to "System" | Error | P1 |
| NEG-053 | Delete — system type | Delete system type | Blocked | P0 |
| NEG-054 | Get — deleted type | Type IsDeleted | Not returned | P1 |
| NEG-055 | Specification invalid | Malformed spec | Error | P2 |
| NEG-056 | Enum parse failure | EntityType invalid enum | Error | P1 |
| NEG-057 | Circular category | Category parent=self | Error | P2 |
| NEG-058 | Audit log failure | Audit service down | Op succeeds, audit queued | P2 |
| NEG-059 | Cache corruption | Stale cache | Refresh or invalidate | P2 |
| NEG-060 | Unicode in name | Name with 日本語 | Stored correctly | P1 |
| NEG-061 | Special chars in name | Name with & < > | Sanitized | P1 |
| NEG-062 | Null optional fields | Description=null | Accepted | P1 |
| NEG-063 | Empty string optional | Description="" | Accepted | P1 |
| NEG-064 | MIME case sensitivity | application/PDF | Per design | P1 |
| NEG-065 | Extension case | .PDF vs .pdf | Per design | P1 |
| NEG-066 | Update — stale version | Optimistic concurrency | Conflict | P1 |
| NEG-067 | List — wrong org | User OrgB list OrgA types | Empty or 403 | P0 |
| NEG-068 | Create — quota exceeded | Max types reached | Error | P1 |
| NEG-069 | Batch create — partial fail | One invalid | Per transaction | P2 |
| NEG-070 | Get with invalid include | Include invalid nav | Error | P2 |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Name | 1 | 255 | "A" | 255 chars | 256 chars | P1 |
| BND-002 | Description | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-003 | MimeType | 1 | 255 | "a/b" | 255 chars | 256 chars | P1 |
| BND-004 | DocumentTypeId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-005 | EntityType length | 1 | 100 | "A" | 100 chars | 101 chars | P1 |
| BND-006 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-007 | PageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-008 | MaxFileSize | 0 | Max | 0 | Max | -1 | P1 |
| BND-009 | Extensions count | 0 | 50 | 0 | 50 | 51 | P2 |
| BND-010 | MIME types count | 1 | 20 | 1 | 20 | 21 | P2 |
| BND-011 | Empty result | 0 | — | 0 types | — | — | P1 |
| BND-012 | Single result | 1 | — | 1 type | — | — | P1 |
| BND-013 | Unicode name | — | — | "类型" | — | — | P1 |
| BND-014 | Special chars | — | — | "Type & Class" | — | — | P1 |
| BND-015 | Pagination last partial | — | — | 95 total, Size=20 | — | — | P1 |
| BND-016 | Pagination beyond last | — | — | Page 100, 10 pages | — | — | P1 |
| BND-017 | Zero ID | — | — | GetDocumentType(0) | — | — | P1 |
| BND-018 | Null entity type | — | — | EntityType=null | — | — | P1 |
| BND-019 | Empty entity type | — | — | EntityType="" | — | — | P1 |
| BND-020 | Whitespace name | — | — | "  Type  " | — | — | P1 |
| BND-021 | Control chars | — | — | \x00 in name | — | — | P1 |
| BND-022 | RTL name | — | — | Arabic | — | — | P2 |
| BND-023 | Emoji in name | — | — | 📄Type | — | — | P2 |
| BND-024 | MIME format | — | — | type/subtype | — | — | P1 |
| BND-025 | Extension format | — | — | .pdf | — | — | P1 |
| BND-026 | Multiple extensions | — | — | .pdf,.doc,.docx | — | — | P1 |
| BND-027 | Very long extension | — | — | .xxxxxxxx | — | — | P2 |
| BND-028 | No dot extension | — | — | pdf | — | — | P1 |
| BND-029 | Date boundaries | — | — | Min/Max DateTime | — | — | P2 |
| BND-030 | Collection empty vs null | — | — | [] vs null | — | — | P1 |
| BND-031 | Sort empty | — | — | OrderBy on empty | — | — | P1 |
| BND-032 | Filter empty | — | — | No criteria | — | — | P1 |
| BND-033 | Category hierarchy | — | — | Parent/child | — | — | P2 |
| BND-034 | Validation rule max | — | — | Max rules | — | — | P2 |
| BND-035 | Concurrent create same name | — | — | 2 threads | — | — | P1 |
| BND-036 | Case entity type | — | — | partner vs Partner | — | — | P1 |
| BND-037 | Decimal ID | — | — | ID=1.5 | — | — | P2 |
| BND-038 | Negative ID | — | — | ID=-1 | — | — | P1 |
| BND-039 | Float max file size | — | — | 5.5MB | — | — | P2 |
| BND-040 | Zero max file size | — | — | 0 | — | — | P1 |
| BND-041 | HTML in description | — | — | <b>bold</b> | — | — | P1 |
| BND-042 | Newline in description | — | — | "Line1\nLine2" | — | — | P2 |
| BND-043 | Tab in name | — | — | "Type\t1" | — | — | P1 |
| BND-044 | Leading/trailing | — | — | "  Type  " | — | — | P1 |
| BND-045 | Multiple spaces | — | — | "Type  1" | — | — | P2 |
| BND-046 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-047 | Cached query | — | — | Second GetDocumentTypes | — | — | P2 |
| BND-048 | Cold start | — | — | First query | — | — | P2 |
| BND-049 | Large result set | — | — | 1000 types | — | — | P1 |
| BND-050 | Single page full | — | — | PageSize=total | — | — | P1 |
| BND-051 | Category ID zero | — | — | CategoryId=0 | — | — | P1 |
| BND-052 | Category ID max | — | — | Max int | — | — | P2 |
| BND-053 | Reserved names list | — | — | Check all reserved | — | — | P1 |
| BND-054 | Enum boundary | — | — | All EntityType values | — | — | P1 |
| BND-055 | Empty MIME list | — | — | [] | — | — | P1 |
| BND-056 | Single MIME | — | — | [application/pdf] | — | — | P1 |
| BND-057 | Extension list max | — | — | 50 extensions | — | — | P2 |
| BND-058 | Validation rule min | — | — | No rules | — | — | P1 |
| BND-059 | Validation rule max | — | — | Max rules | — | — | P2 |
| BND-060 | Category depth | — | — | 5 levels | — | — | P2 |
| BND-061 | Leap year date | — | — | 2024-02-29 | — | — | P2 |
| BND-062 | Epoch date | — | — | 1970-01-01 | — | — | P2 |
| BND-063 | Future date | — | — | 2030-01-01 | — | — | P2 |
| BND-064 | Multiple entity types | — | — | Type for Partner+Contact | — | — | P2 |
| BND-065 | Overlapping MIME | — | — | Two types same MIME | — | — | P2 |
| BND-066 | Overlapping extension | — | — | Two types .pdf | — | — | P2 |
| BND-067 | Sort by multiple | — | — | EntityType, Name | — | — | P2 |
| BND-068 | Filter combination | — | — | Entity+Category | — | — | P1 |
| BND-069 | Null optional | — | — | All optional null | — | — | P1 |
| BND-070 | Max nested includes | — | — | Type→Category→Parent | — | — | P2 |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete sets IsDeleted | Delete type | DeleteDocumentType | IsDeleted=true | P0 |
| FUN-002 | Deleted excluded from list | Get types | GetDocumentTypes | Deleted excluded | P0 |
| FUN-003 | Entity type filter | Filter by Partner | GetDocumentTypes(Partner) | Partner only | P0 |
| FUN-004 | CreatedBy/CreatedDate | Create | CreateDocumentType | Audit set | P0 |
| FUN-005 | LastModified on update | Update | UpdateDocumentType | Updated | P0 |
| FUN-006 | Name required | Create without name | CreateDocumentType | Validation error | P0 |
| FUN-007 | MIME mapping | Type with MIME | GetDocumentType | MIME returned | P1 |
| FUN-008 | Validation rules | Type with rules | GetDocumentType | Rules returned | P1 |
| FUN-009 | Pagination TotalCount | Get types | GetDocumentTypes | Accurate | P0 |
| FUN-010 | Sort applied | OrderBy | GetDocumentTypes | Sorted | P1 |
| FUN-011 | Null entity returns all | EntityType=null | GetDocumentTypes | All | P1 |
| FUN-012 | Invalid entity empty | EntityType invalid | GetDocumentTypes | Empty | P1 |
| FUN-013 | Case sensitivity | partner vs Partner | GetDocumentTypes | Per design | P1 |
| FUN-014 | Whitespace trim | "  Partner  " | GetDocumentTypes | Trimmed | P1 |
| FUN-015 | Get by ID null | ID 99999 | GetDocumentType | Null | P1 |
| FUN-016 | Update non-existent | Update 99999 | UpdateDocumentType | Null | P1 |
| FUN-017 | Delete non-existent | Delete 99999 | DeleteDocumentType | Graceful | P1 |
| FUN-018 | Delete in use | Docs reference | DeleteDocumentType | Per rule | P1 |
| FUN-019 | Org scope | User OrgA | GetDocumentTypes | OrgA types | P0 |
| FUN-020 | Permission create | User lacks | CreateDocumentType | 403 | P0 |
| FUN-021 | Permission update | User lacks | UpdateDocumentType | 403 | P0 |
| FUN-022 | Permission delete | User lacks | DeleteDocumentType | 403 | P0 |
| FUN-023 | Categorization | Category filter | GetDocumentTypes | Filtered | P1 |
| FUN-024 | MIME multiple | Multiple MIMEs | GetDocumentType | All returned | P1 |
| FUN-025 | Extensions multiple | Multiple ext | GetDocumentType | All returned | P1 |
| FUN-026 | MaxFileSize rule | Rule set | GetDocumentType | Returned | P1 |
| FUN-027 | Duplicate name | Create duplicate | CreateDocumentType | Error | P1 |
| FUN-028 | Audit trail create | Create | CreateDocumentType | Audit entry | P1 |
| FUN-029 | Audit trail update | Update | UpdateDocumentType | Audit entry | P1 |
| FUN-030 | Audit trail delete | Delete | DeleteDocumentType | Audit entry | P1 |
| FUN-031 | Idempotent delete | Delete twice | DeleteDocumentType | Graceful | P1 |
| FUN-032 | Specification filter | Spec | GetDocumentTypes | Filtered | P1 |
| FUN-033 | Mapping to Model | Get type | DocumentTypeModel | Correct mapping | P1 |
| FUN-034 | Name length validation | Name > 255 | Create/Update | Error | P1 |
| FUN-035 | MIME format validation | Invalid MIME | Create/Update | Error | P1 |
| FUN-036 | Entity type validation | Invalid enum | Create/Update | Error | P1 |
| FUN-037 | Category validation | Invalid category | Create/Update | Error | P1 |
| FUN-038 | Optional fields | Null optional | Create | Accepted | P1 |
| FUN-039 | Caching (if any) | Second query | GetDocumentTypes | Cached or fresh | P2 |
| FUN-040 | Invalidate cache on update | Update type | Cache | Invalidated | P2 |
| FUN-041 | Document count per type | Type with docs | GetDocumentType | Count or N/A | P2 |
| FUN-042 | Reserved type protection | System type | Delete | Blocked | P0 |
| FUN-043 | Default type | No type specified | Document create | Default applied | P2 |
| FUN-044 | Type ordering | Display order | GetDocumentTypes | Ordered | P2 |
| FUN-045 | Status filter | Active only | GetDocumentTypes | Active only | P1 |
| FUN-046 | Optimistic concurrency | Concurrent update | UpdateDocumentType | Conflict | P1 |
| FUN-047 | Bulk operations | Batch create | CreateDocumentTypes | Per design | P2 |
| FUN-048 | Validation rule apply | Validate file | Per rules | Pass/Fail | P1 |
| FUN-049 | MIME match | File MIME vs type | Match | Correct type | P1 |
| FUN-050 | Extension match | File ext vs type | Match | Correct type | P1 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD | Create→Get→Update→Delete | DocumentType | All succeed | P0 |
| INT-002 | DocumentManager uses type | Document with type | Document, DocumentType | Type loaded | P0 |
| INT-003 | Document upload type validation | Upload with type | Document, DocumentType | Validated | P0 |
| INT-004 | Permission check | Authorize | DocumentType, PermissionService | Correct | P0 |
| INT-005 | Audit log | Audit CRUD | DocumentType, AuditLog | Entries | P1 |
| INT-006 | UserContext | Current user | DocumentType, UserResolver | Applied | P0 |
| INT-007 | DbContext | Persist | DocumentType, DbContext | Saved | P0 |
| INT-008 | AutoMapper | Entity to Model | DocumentType, AutoMapper | Mapped | P1 |
| INT-009 | Controller | API CRUD | DocumentType, Controller | 200/201/204 | P0 |
| INT-010 | Error handling | Exception | DocumentType, Handler | Consistent | P1 |
| INT-011 | Document list by type | List docs | Document, DocumentType | Filtered | P0 |
| INT-012 | Entity type enum | Partner/Contact/Interaction | DocumentType | Correct | P0 |
| INT-013 | Repository | CRUD | DocumentType, Repository | Works | P1 |
| INT-014 | ManagerWrapper | Resolution | ManagerWrapper | Correct | P1 |
| INT-015 | Validation service | Validate | DocumentType, Validator | Errors | P1 |
| INT-016 | Multi-tenant | Org scope | DocumentType | Isolated | P0 |
| INT-017 | Configuration | Config | DocumentType, IConfiguration | Applied | P2 |
| INT-018 | Logging | Log | DocumentType, ILogger | Logs | P2 |
| INT-019 | List view | Types in dropdown | DocumentType, ListView | Displayed | P1 |
| INT-020 | Detail view | Type in doc detail | DocumentType | Displayed | P0 |
| INT-021 | Create document with type | Create doc | Document, DocumentType | Type assigned | P0 |
| INT-022 | Update document type | Change type | Document, DocumentType | Updated | P0 |
| INT-023 | Document type dropdown | Get types for entity | DocumentType | List for dropdown | P0 |
| INT-024 | MIME validation on upload | Upload file | Document, DocumentType | MIME checked | P0 |
| INT-025 | Extension validation | Upload file | Document, DocumentType | Ext checked | P0 |
| INT-026 | Category hierarchy | Type in category | DocumentType, Category | Hierarchy | P1 |
| INT-027 | Cached types | Second request | DocumentType | Fast | P2 |
| INT-028 | API 404 | Get 99999 | Controller | 404 | P0 |
| INT-029 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-030 | API 403 | Unauthorized | Controller | 403 | P0 |
| INT-031 | Feature flag | Feature for types | DocumentType | Respected | P2 |
| INT-032 | Migration | Add type | DocumentType | Migrated | P2 |
| INT-033 | Seed data | Initial types | DocumentType | Seeded | P2 |
| INT-034 | Export types | Export | DocumentType | Export file | P2 |
| INT-035 | Import types | Import | DocumentType | Imported | P2 |
| INT-036 | Type in report | Report | DocumentType | In report | P2 |
| INT-037 | Type in search | Search | DocumentType | In results | P1 |
| INT-038 | Type ordering | Display order | DocumentType | Ordered | P2 |
| INT-039 | Active/inactive | Status | DocumentType | Filtered | P1 |
| INT-040 | Cascading delete | Delete type with docs | DocumentType | Per rule | P1 |
| INT-041 | Default type assignment | New doc | DocumentType | Default | P2 |
| INT-042 | Type change audit | Change type | DocumentType | Audited | P1 |
| INT-043 | Validation rules apply | Upload | DocumentType | Rules applied | P0 |
| INT-044 | MIME to type lookup | File MIME | DocumentType | Type found | P0 |
| INT-045 | Extension to type lookup | File ext | DocumentType | Type found | P0 |
| INT-046 | Multiple types match | Same MIME | DocumentType | Per design | P1 |
| INT-047 | No type match | Unknown MIME | DocumentType | Default or error | P1 |
| INT-048 | Type in workflow | Document workflow | DocumentType | In workflow | P2 |
| INT-049 | Type permissions | Per-type permission | DocumentType | Enforced | P1 |
| INT-050 | Type quota | Per-type quota | DocumentType | Enforced | P2 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection name | '; DROP TABLE-- | CreateDocumentType | Sanitized | P0 |
| SEC-002 | SQL injection filter | ' OR 1=1-- | GetDocumentTypes | Sanitized | P0 |
| SEC-003 | XSS in name | <script>alert(1)</script> | CreateDocumentType | Sanitized | P0 |
| SEC-004 | XSS in description | <img src=x> | CreateDocumentType | Sanitized | P0 |
| SEC-005 | IDOR get | GetDocumentType(otherId) | GetDocumentType | 403 | P0 |
| SEC-006 | IDOR update | UpdateDocumentType(otherId) | Update | 403 | P0 |
| SEC-007 | IDOR delete | DeleteDocumentType(otherId) | Delete | 403 | P0 |
| SEC-008 | Mass assignment Id | Include Id | Create | Ignored | P0 |
| SEC-009 | Mass assignment IsDeleted | Include IsDeleted | Update | Ignored | P0 |
| SEC-010 | Unauthenticated get | No auth | GetDocumentTypes | 401 | P0 |
| SEC-011 | Unauthenticated create | No auth | CreateDocumentType | 401 | P0 |
| SEC-012 | Expired token | Expired JWT | Any | 401 | P0 |
| SEC-013 | Wrong role create | No permission | Create | 403 | P0 |
| SEC-014 | Wrong role update | No permission | Update | 403 | P0 |
| SEC-015 | Wrong role delete | No permission | Delete | 403 | P0 |
| SEC-016 | Org scope bypass | Cross-org | GetDocumentType | 403 | P0 |
| SEC-017 | LDAP injection | *)(uid=* | Filter | Sanitized | P0 |
| SEC-018 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| SEC-019 | Rate limit | Too many | API | 429 | P1 |
| SEC-020 | CSRF create | Cross-site | Create | Token validated | P0 |
| SEC-021 | CSRF update | Cross-site | Update | Token validated | P0 |
| SEC-022 | CSRF delete | Cross-site | Delete | Token validated | P0 |
| SEC-023 | Parameter pollution | id=1&id=2 | Get | Handled | P1 |
| SEC-024 | Header injection | Malicious header | Request | Sanitized | P1 |
| SEC-025 | Brute force IDs | Enumerate | GetDocumentType | Rate limited | P1 |
| SEC-026 | JWT alg none | alg=none | Request | Rejected | P0 |
| SEC-027 | Log injection | Malicious log | Log | Sanitized | P1 |
| SEC-028 | Info disclosure | Probe errors | Invalid | Generic | P1 |
| SEC-029 | Excessive data | Huge PageSize | List | Capped | P1 |
| SEC-030 | Cookie manipulation | Modify cookie | Request | Rejected | P0 |
| SEC-031 | Replay attack | Replay | Create | Nonce | P1 |
| SEC-032 | MIME injection | Malicious MIME | Create | Sanitized | P0 |
| SEC-033 | Extension injection | Path in extension | Create | Sanitized | P0 |
| SEC-034 | Validation rule injection | Malicious rule | Create | Sanitized | P0 |
| SEC-035 | Category injection | Malicious category | Create | Sanitized | P0 |
| SEC-036 | HTTP verb tampering | PUT vs POST | Create | 405 | P1 |
| SEC-037 | Path traversal | ../../../ | Any | Rejected | P0 |
| SEC-038 | Null byte | %00 | Filename | Rejected | P0 |
| SEC-039 | Unicode homograph | Cyrillic 'a' | Name | Detected | P1 |
| SEC-040 | Open redirect | Redirect | Callback | Validated | P1 |
| SEC-041 | Session fixation | Fixate | Login | New session | P1 |
| SEC-042 | Substitution attack | Replace JWT | Request | 403 | P0 |
| SEC-043 | Timing attack | Response time | Get | Constant | P2 |
| SEC-044 | Insecure reference | EntityType manip | Filter | Validated | P0 |
| SEC-045 | Privilege escalation | Admin action | User | 403 | P0 |
| SEC-046 | Concurrent auth | Two users | Request | Isolated | P0 |
| SEC-047 | Token in URL | Token in query | Request | Avoided | P1 |
| SEC-048 | Cache poisoning | Malicious cache | Cache | Sanitized | P1 |
| SEC-049 | DoS large request | Huge body | Create | Rejected | P1 |
| SEC-050 | DoS deep recursion | Deep category | Get | Limited | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent get types | 20 threads GetDocumentTypes | All correct | P0 |
| CON-002 | Concurrent create | 10 threads CreateDocumentType | All created | P0 |
| CON-003 | Concurrent update same | 5 threads UpdateDocumentType(123) | No corruption | P0 |
| CON-004 | Concurrent delete same | 2 threads DeleteDocumentType(123) | One succeeds | P0 |
| CON-005 | Create and get | Thread1 create, Thread2 get | Consistent | P1 |
| CON-006 | Update and get | Thread1 update, Thread2 get | Consistent | P1 |
| CON-007 | Delete and get | Thread1 delete, Thread2 get | Null | P0 |
| CON-008 | Optimistic concurrency | 2 users update same | Conflict | P0 |
| CON-009 | Connection pool | 100 concurrent | No exhaustion | P1 |
| CON-010 | Deadlock | Circular | No deadlock | P1 |
| CON-011 | Transaction isolation | Read uncommitted | Per level | P1 |
| CON-012 | Cache consistency | Concurrent updates | Cache fresh | P1 |
| CON-013 | Double submit create | User double-clicks | One created | P0 |
| CON-014 | Race duplicate name | 2 threads same name | One succeeds | P1 |
| CON-015 | List during create | Thread1 create, Thread2 list | Consistent | P1 |
| CON-016 | Filtered get concurrent | 10 threads different filters | All correct | P1 |
| CON-017 | Pagination concurrent | 20 threads different pages | Correct pages | P1 |
| CON-018 | Bulk create | 2 threads bulk | Consistent | P1 |
| CON-019 | Delete in use | Thread1 delete, Thread2 doc create | Handled | P1 |
| CON-020 | Update type with docs | Thread1 update type, Thread2 get doc | Consistent | P1 |
| CON-021 | MIME update concurrent | 2 threads update MIMEs | One wins | P1 |
| CON-022 | Category update | 2 threads update category | One wins | P1 |
| CON-023 | Lost update | 2 users different fields | Per design | P1 |
| CON-024 | Phantom read | Insert during paginate | Per isolation | P2 |
| CON-025 | Non-repeatable read | Update between reads | Per isolation | P2 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Entity type validation | Validation | "Partner" | Valid | P0 |
| UNT-002 | Entity type invalid | Validation | "Invalid" | Invalid | P0 |
| UNT-003 | Name validation | Validation | "Type1" | Valid | P0 |
| UNT-004 | Name empty | Validation | "" | Invalid | P0 |
| UNT-005 | MIME format | Formatting | "application/pdf" | Valid | P0 |
| UNT-006 | MIME invalid | Formatting | "application" | Invalid | P0 |
| UNT-007 | Extension format | Formatting | ".pdf" | Valid | P1 |
| UNT-008 | Name trim | Calculation | "  Type  " | "Type" | P1 |
| UNT-009 | Entity type parse | Calculation | "partner" | Partner | P1 |
| UNT-010 | Status Active | Status logic | IsDeleted=false | Active | P1 |
| UNT-011 | Status Deleted | Status logic | IsDeleted=true | Excluded | P0 |
| UNT-012 | Collection filter | Collections | List with deleted | Deleted excluded | P1 |
| UNT-013 | Empty collection | Collections | No types | Count=0 | P1 |
| UNT-014 | Null to empty | Collections | Null list | [] | P1 |
| UNT-015 | Map to Model | Mapping | DocumentType entity | DocumentTypeModel | P0 |
| UNT-016 | Map Request | Mapping | CreateRequest | Entity | P0 |
| UNT-017 | Pagination slice | Calculation | Page 1, Size 10 | Skip 10, Take 10 | P1 |
| UNT-018 | MIME match | Calculation | application/pdf vs type | Match | P1 |
| UNT-019 | Extension match | Calculation | .pdf vs type | Match | P1 |
| UNT-020 | Audit fields | Status logic | New type | CreatedBy set | P1 |
| UNT-021 | Validation rule apply | Validation | File vs rules | Pass/Fail | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Get document types | GetDocumentTypes | < 200ms | P0 |
| PRF-002 | Get 1000 types | GetDocumentTypes | < 500ms | P0 |
| PRF-003 | Get with filter | GetDocumentTypes(Partner) | < 150ms | P0 |
| PRF-004 | Pagination | GetDocumentTypes page 1 | < 100ms | P0 |
| PRF-005 | Get by ID | GetDocumentType | < 50ms | P0 |
| PRF-006 | Create type | CreateDocumentType | < 200ms | P1 |
| PRF-007 | Update type | UpdateDocumentType | < 200ms | P1 |
| PRF-008 | Delete type | DeleteDocumentType | < 100ms | P1 |
| PRF-009 | Concurrent 20 requests | 20 GetDocumentTypes | < 300ms each | P1 |
| PRF-010 | Cold start | First query | < 500ms | P1 |
| PRF-011 | Cached query | Second query | < 50ms | P1 |
| PRF-012 | Memory 1000 types | Get 1000 | < 50MB | P1 |
| PRF-013 | Mapping 100 | 100 entity mappings | < 10ms | P2 |
| PRF-014 | Index usage | Filter by EntityType | Uses index | P2 |
| PRF-015 | Sort large set | Sort 1000 | < 300ms | P1 |
| PRF-016 | Filter + sort | Both | < 200ms | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained 50 req/s get | 50 GetDocumentTypes/sec | 5 min | 95% < 200ms | P0 |
| LDT-002 | Sustained 20 req/s create | 20 Create/sec | 5 min | 95% < 300ms | P0 |
| LDT-003 | Sustained 30 req/s filter | 30 filtered/sec | 5 min | 95% < 150ms | P0 |
| LDT-004 | Spike 100 req/s | 100 req/s burst | 1 min | No crash | P0 |
| LDT-005 | Spike 200 req/s | 200 req/s | 30 sec | Graceful degrade | P1 |
| LDT-006 | Stress ramp | 1→500 req/s | Until fail | Find limit | P1 |
| LDT-007 | Connection pool | 200 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Memory | 10K types | 5 min | No leak | P1 |
| LDT-009 | Recovery spike | Spike then normal | 5 min | Baseline | P0 |
| LDT-010 | Recovery stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
