# DocumentManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DocumentManager`  
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

**DocumentManager** manages document upload/download, file type validation, virus scan integration, metadata management, and entity linking. Key responsibilities: document CRUD, entity-document relationships, document type assignment, filtering (exclude deleted, exclude folders), metadata updates, and parent entity resolution.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | List documents for entity | Partner 123 has 10 docs | ListDocuments(entityType, entityId) | 10 documents returned | P0 |
| POS-002 | List documents — empty | Entity has no docs | ListDocuments | Empty list | P1 |
| POS-003 | Filter deleted documents | Mix of active/deleted | ListDocuments | Deleted excluded | P0 |
| POS-004 | Filter folders from list | Entity has folders+docs | ListDocuments | Only files (not folders) | P0 |
| POS-005 | Get document by ID — exists | Document 789 exists | GetDocument(789) | Document returned | P0 |
| POS-006 | Get document with DocumentType | Doc has type | GetDocument | DocumentType loaded | P1 |
| POS-007 | Get document parent entity | Doc linked to Partner | GetDocument | Parent entity info | P1 |
| POS-008 | Update document metadata | Document exists | UpdateDocument(name, description) | Metadata updated | P0 |
| POS-009 | Update document type | Document exists | UpdateDocumentType(docId, typeId) | Type updated | P1 |
| POS-010 | Upload document — valid type | Valid file, entity exists | UploadDocument(file, entityType, entityId) | Document created | P0 |
| POS-011 | Download document | Document exists | DownloadDocument(docId) | File stream returned | P0 |
| POS-012 | List documents — pagination | 100 documents | ListDocuments with pagination | Paginated results | P1 |
| POS-013 | List documents — ordered by date | Documents exist | ListDocuments OrderBy=CreatedDate | Sorted by date | P1 |
| POS-014 | List documents — ordered by name | Documents exist | ListDocuments OrderBy=Name | Sorted by name | P1 |
| POS-015 | Document with multiple relationships | Doc linked to Partner+Contact | GetDocument | All relationships | P1 |
| POS-016 | Delete document — soft delete | Document exists | DeleteDocument(docId) | IsDeleted=true | P0 |
| POS-017 | Get document — not found | ID 99999 invalid | GetDocument(99999) | Null | P1 |
| POS-018 | Update non-existent — graceful | ID 99999 invalid | UpdateDocument(99999) | Null | P1 |
| POS-019 | List by entity type Partner | Partner docs | ListDocuments(Partner, 123) | Partner docs only | P0 |
| POS-020 | List by entity type Contact | Contact docs | ListDocuments(Contact, 456) | Contact docs only | P0 |
| POS-021 | List by entity type Interaction | Interaction docs | ListDocuments(Interaction, 789) | Interaction docs only | P0 |
| POS-022 | Document type validation | Valid MIME type | UploadDocument(PDF) | Accepted | P0 |
| POS-023 | Metadata — name, description | Document exists | Update with name/desc | Saved | P1 |
| POS-024 | Virus scan — clean file | Clean file | UploadDocument | Document created | P0 |
| POS-025 | Entity linking — create relationship | Doc + entity | LinkDocumentToEntity | Relationship created | P1 |
| POS-026 | Entity linking — remove | Doc linked | UnlinkDocumentFromEntity | Relationship removed | P1 |
| POS-027 | Get parent entity — Partner | Doc linked to Partner | GetParentEntity | Partner returned | P1 |
| POS-028 | Get parent entity — Contact | Doc linked to Contact | GetParentEntity | Contact returned | P1 |
| POS-029 | Document without DocumentType | Doc has null type | GetDocument | Handled | P1 |
| POS-030 | Search documents by name | Documents exist | SearchDocuments(name) | Matching docs | P1 |
| POS-031 | Filter by document type | Multiple types | ListDocuments filter by type | Filtered | P1 |
| POS-032 | Full CRUD cycle | None | Create→Get→Update→Get→Delete | All succeed | P0 |
| POS-033 | Upload — different file types | PDF, DOCX, XLSX | UploadDocument each | All accepted | P1 |
| POS-034 | Download — verify file integrity | Document uploaded | Download, compare hash | Match | P1 |
| POS-035 | Document relationship validation | Valid entity IDs | LinkDocumentToEntity | Relationship valid | P1 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Upload — invalid file type | .exe file | Rejected | P0 |
| NEG-002 | Upload — blocked extension | .bat, .cmd | Rejected | P0 |
| NEG-003 | Upload — virus detected | Infected file | Rejected | P0 |
| NEG-004 | Upload — file too large | File > max size | Rejected | P0 |
| NEG-005 | Upload — null file | IFormFile null | ArgumentNullException | P0 |
| NEG-006 | Upload — invalid entity ID | EntityId=99999 | Error | P0 |
| NEG-007 | Upload — invalid entity type | EntityType="Invalid" | Error | P0 |
| NEG-008 | Get document — ID zero | GetDocument(0) | Null or error | P1 |
| NEG-009 | Get document — ID negative | GetDocument(-1) | Null or error | P1 |
| NEG-010 | Update — non-existent ID | UpdateDocument(99999) | Null | P1 |
| NEG-011 | Delete — non-existent ID | DeleteDocument(99999) | Graceful | P1 |
| NEG-012 | Delete — already deleted | Doc IsDeleted=true | Idempotent or error | P1 |
| NEG-013 | List — invalid entity type | EntityType="Xyz" | Empty or error | P1 |
| NEG-014 | List — invalid entity ID | EntityId=0 | Empty or error | P1 |
| NEG-015 | Download — non-existent | DownloadDocument(99999) | 404 or error | P0 |
| NEG-016 | Download — deleted document | Doc IsDeleted | Error or blocked | P0 |
| NEG-017 | Link — invalid document ID | LinkDocument(99999, entityId) | Error | P1 |
| NEG-018 | Link — invalid entity ID | LinkDocument(docId, 99999) | Error | P1 |
| NEG-019 | Path traversal — filename | ../../../etc/passwd | Rejected | P0 |
| NEG-020 | Path traversal — in content | Malicious content | Sanitized | P0 |
| NEG-021 | XSS in document name | <script>alert(1)</script> | Sanitized | P0 |
| NEG-022 | SQL injection in search | ' OR 1=1-- | Sanitized | P0 |
| NEG-023 | Unauthorized upload | User lacks permission | 403 | P0 |
| NEG-024 | Unauthorized download | User lacks permission | 403 | P0 |
| NEG-025 | Unauthorized delete | User lacks permission | 403 | P0 |
| NEG-026 | IDOR — access other org document | GetDocument(otherOrgDocId) | 403 | P0 |
| NEG-027 | IDOR — download other org | DownloadDocument(otherOrgDocId) | 403 | P0 |
| NEG-028 | Null document type ID | DocumentTypeId=null on create | Error or default | P1 |
| NEG-029 | Invalid document type ID | DocumentTypeId=99999 | Error | P1 |
| NEG-030 | Pagination — invalid page | PageIndex=-1 | Error | P1 |
| NEG-031 | Pagination — zero page size | PageSize=0 | Error | P1 |
| NEG-032 | Empty file upload | 0-byte file | Rejected | P1 |
| NEG-033 | Corrupted file upload | Invalid PDF | Rejected | P1 |
| NEG-034 | Duplicate filename same entity | Same name upload | Handled (rename or error) | P1 |
| NEG-035 | Link to deleted entity | EntityId deleted | Error | P1 |
| NEG-036 | Get parent — doc has no relationship | Doc unlinked | Null | P1 |
| NEG-037 | Database timeout on upload | Simulate timeout | Rollback | P1 |
| NEG-038 | Storage full on upload | Disk full | Error | P1 |
| NEG-039 | Virus scan service down | Scan unavailable | Queued or error | P1 |
| NEG-040 | Malformed MIME type | Invalid content-type | Rejected | P1 |
| NEG-041 | Double extension | file.pdf.exe | Rejected | P0 |
| NEG-042 | Null byte in filename | file.pdf%00.exe | Rejected | P0 |
| NEG-043 | Excessive metadata length | Name > 255 | Validation error | P1 |
| NEG-044 | Invalid character in name | Name with \0 | Rejected | P1 |
| NEG-045 | Expired auth token | Expired JWT | 401 | P0 |
| NEG-046 | Tampered file hash | Modified after upload | Integrity check fail | P1 |
| NEG-047 | Concurrent update conflict | 2 users update same doc | Concurrency error | P1 |
| NEG-048 | Document type mismatch | Upload PDF, assign DOC type | Validation or allowed | P1 |
| NEG-049 | Entity type mismatch | Link Partner doc to Contact entity | Error or validated | P1 |
| NEG-050 | Orphaned document | Entity deleted, doc remains | Handled | P1 |
| NEG-051 | Mass assignment — set Id | Include Id in UploadRequest | Ignored | P0 |
| NEG-052 | Mass assignment — set CreatedBy | Include in request | Ignored | P0 |
| NEG-053 | Rate limit on upload | Too many uploads | 429 | P1 |
| NEG-054 | Rate limit on download | Too many downloads | 429 | P1 |
| NEG-055 | Unicode filename — invalid | Problematic chars | Sanitized | P1 |
| NEG-056 | Link — circular reference | Complex scenario | Error | P2 |
| NEG-057 | Update — read-only document | Doc in read-only state | Error | P1 |
| NEG-058 | Delete — document in use | Doc referenced | Business rule | P1 |
| NEG-059 | List — deleted entity | EntityId deleted | Empty or error | P1 |
| NEG-060 | GetParentEntity — wrong entity type | Request wrong type | Null or error | P1 |
| NEG-061 | Upload — quota exceeded | User/org quota exceeded | Error | P1 |
| NEG-062 | Download — range request invalid | Invalid byte range | 416 or error | P2 |
| NEG-063 | Checksum mismatch | Stored vs computed | Error | P1 |
| NEG-064 | Document type deleted | DocTypeId soft-deleted | Handled | P1 |
| NEG-065 | List — specification invalid | Malformed filter | Error | P2 |
| NEG-066 | Unlink — non-existent link | UnlinkDocument(no link) | Graceful | P1 |
| NEG-067 | Batch upload — partial failure | One invalid in batch | Per design | P2 |
| NEG-068 | Storage path injection | Custom path in request | Rejected | P0 |
| NEG-069 | Symbolic link attack | Symlink in path | Rejected | P0 |
| NEG-070 | Metadata injection | Metadata with script | Sanitized | P0 |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Document name | 1 | 255 | "a" | 255 chars | 256 chars | P1 |
| BND-002 | Description | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-003 | File size | 1 | MaxAllowed | 1 byte | Max (e.g. 50MB) | Max+1 | P1 |
| BND-004 | DocumentId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-005 | EntityId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-006 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-007 | PageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-008 | Filename length | 1 | 255 | "a.pdf" | 255 chars | 256 chars | P1 |
| BND-009 | MIME type length | 1 | 255 | "a/b" | 255 chars | 256 chars | P1 |
| BND-010 | Document count per entity | 0 | 10000 | 0 | 10000 | — | P2 |
| BND-011 | Empty file | 0 | — | 0 bytes | — | — | P1 |
| BND-012 | Single byte file | 1 | — | 1 byte | — | — | P1 |
| BND-013 | Unicode filename | — | — | "文档.pdf" | — | — | P1 |
| BND-014 | Special chars filename | — | — | "file (1).pdf" | — | — | P1 |
| BND-015 | Very long description | — | — | 4000 chars | — | — | P1 |
| BND-016 | Pagination last page partial | — | — | 95 total, PageSize=20 | — | — | P1 |
| BND-017 | Date boundaries | — | — | Min/Max DateTime | — | — | P2 |
| BND-018 | Multiple extensions | — | — | file.tar.gz | — | — | P1 |
| BND-019 | Case sensitivity extension | — | — | .PDF vs .pdf | — | — | P1 |
| BND-020 | Zero EntityId | — | — | EntityId=0 | — | — | P1 |
| BND-021 | Null optional metadata | — | — | Description=null | — | — | P1 |
| BND-022 | Empty search term | — | — | Search("") | — | — | P1 |
| BND-023 | Max search term length | — | — | 255 char search | — | — | P2 |
| BND-024 | Concurrent upload same name | — | — | 2 threads same filename | — | — | P1 |
| BND-025 | Document type ID zero | — | — | DocumentTypeId=0 | — | — | P1 |
| BND-026 | Batch size | 1 | 100 | 1 | 100 | 101 | P2 |
| BND-027 | Path length | — | — | Max path 260/4096 | — | — | P1 |
| BND-028 | Content-Type boundary | — | — | multipart boundaries | — | — | P2 |
| BND-029 | Unicode in description | — | — | 日本語 | — | — | P2 |
| BND-030 | Control characters in name | — | — | \x00 in name | — | — | P1 |
| BND-031 | RTL in filename | — | — | Arabic filename | — | — | P2 |
| BND-032 | Emoji in name | — | — | 📄doc.pdf | — | — | P2 |
| BND-033 | HTML in description | — | — | <b>bold</b> | — | — | P1 |
| BND-034 | Newline in description | — | — | "Line1\nLine2" | — | — | P2 |
| BND-035 | Tab in name | — | — | "Doc\tument" | — | — | P1 |
| BND-036 | Leading/trailing spaces | — | — | "  file.pdf  " | — | — | P1 |
| BND-037 | Multiple spaces | — | — | "file  name.pdf" | — | — | P2 |
| BND-038 | Dot at start | — | — | ".hidden" | — | — | P1 |
| BND-039 | Multiple dots | — | — | "file..pdf" | — | — | P1 |
| BND-040 | No extension | — | — | "filename" | — | — | P1 |
| BND-041 | Very long extension | — | — | .xxxxxxxx | — | — | P2 |
| BND-042 | File size exactly at limit | — | — | Exactly max bytes | — | — | P1 |
| BND-043 | Pagination beyond last | — | — | PageIndex=100, 10 pages | — | — | P1 |
| BND-044 | Sort empty result | — | — | OrderBy on empty | — | — | P1 |
| BND-045 | Filter by empty type | — | — | DocumentTypeId=null | — | — | P1 |
| BND-046 | Collection empty vs null | — | — | Empty list vs null | — | — | P1 |
| BND-047 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-048 | Checksum algorithms | — | — | MD5/SHA256 | — | — | P2 |
| BND-049 | MIME type variations | — | — | application/pdf vs image/pdf | — | — | P1 |
| BND-050 | Case entity type | — | — | "partner" vs "Partner" | — | — | P1 |
| BND-051 | Zero DocumentTypeId | — | — | DocumentTypeId=0 | — | — | P1 |
| BND-052 | Max relationships per doc | — | — | Doc linked to N entities | — | — | P2 |
| BND-053 | Leap year date | — | — | 2024-02-29 | — | — | P2 |
| BND-054 | Epoch date | — | — | 1970-01-01 | — | — | P2 |
| BND-055 | Future date | — | — | 2030-01-01 | — | — | P2 |
| BND-056 | Stream position | — | — | Read from position 0 | — | — | P2 |
| BND-057 | Large metadata JSON | — | — | Metadata blob | — | — | P2 |
| BND-058 | Concurrent download | — | — | 2 threads same doc | — | — | P1 |
| BND-059 | Download range partial | — | — | bytes=0-999 | — | — | P2 |
| BND-060 | Content-Disposition | — | — | attachment; filename= | — | — | P2 |
| BND-061 | Filename with quotes | — | — | "file name".pdf | — | — | P1 |
| BND-062 | Reserved Windows names | — | — | CON, NUL, etc. | — | — | P1 |
| BND-063 | Reserved chars | — | — | * ? : < > | | — | — | P1 |
| BND-064 | Very long path | — | — | Deep directory | — | — | P2 |
| BND-065 | Empty entity type | — | — | EntityType="" | — | — | P1 |
| BND-066 | Whitespace entity type | — | — | "  Partner  " | — | — | P1 |
| BND-067 | Decimal ID | — | — | ID=1.5 | — | — | P2 |
| BND-068 | Negative ID | — | — | ID=-1 | — | — | P1 |
| BND-069 | Null ID | — | — | GetDocument(null) | — | — | P1 |
| BND-070 | Max nested includes | — | — | Doc→Type→Entity | — | — | P2 |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete sets IsDeleted | Delete document | DeleteDocument | IsDeleted=true | P0 |
| FUN-002 | Deleted docs excluded from list | List documents | ListDocuments | Deleted excluded | P0 |
| FUN-003 | Folders excluded from document list | List documents | ListDocuments | Folders excluded | P0 |
| FUN-004 | CreatedBy/CreatedDate on upload | Upload | UploadDocument | Audit fields set | P0 |
| FUN-005 | LastModified on update | Update | UpdateDocument | LastModified updated | P0 |
| FUN-006 | Document type required/optional | Create | UploadDocument | Per schema | P1 |
| FUN-007 | Entity relationship required | Link | LinkDocumentToEntity | Relationship created | P1 |
| FUN-008 | Parent entity resolution | Get parent | GetParentEntity | Correct entity | P0 |
| FUN-009 | File type validation | Upload | UploadDocument | Valid types only | P0 |
| FUN-010 | Virus scan before save | Upload | UploadDocument | Scan runs | P0 |
| FUN-011 | Metadata persistence | Update | UpdateDocument | All metadata saved | P1 |
| FUN-012 | Pagination TotalCount | List | ListDocuments | Accurate count | P0 |
| FUN-013 | Sort order applied | List | ListDocuments OrderBy | Sorted | P1 |
| FUN-014 | Filter by entity type | List | ListDocuments(Partner) | Partner docs only | P0 |
| FUN-015 | Filter by document type | List | ListDocuments filter | Type filtered | P1 |
| FUN-016 | Name uniqueness per entity | Upload | Same name | Per rule | P1 |
| FUN-017 | Download returns correct file | Download | DownloadDocument | File matches | P0 |
| FUN-018 | Checksum verification | Upload/Download | Verify hash | Match | P1 |
| FUN-019 | MIME type stored | Upload | UploadDocument | ContentType saved | P1 |
| FUN-020 | File size stored | Upload | UploadDocument | Size saved | P1 |
| FUN-021 | Unlink clears relationship | Unlink | UnlinkDocumentFromEntity | Relationship removed | P1 |
| FUN-022 | Cascade on entity delete | Delete entity | Entity deleted | Docs handled per rule | P1 |
| FUN-023 | Org scope filtering | List | User from OrgA | Only OrgA docs | P0 |
| FUN-024 | Permission on upload | Upload | User permission | 403 if denied | P0 |
| FUN-025 | Permission on download | Download | User permission | 403 if denied | P0 |
| FUN-026 | Permission on delete | Delete | User permission | 403 if denied | P0 |
| FUN-027 | Specification filter | List | Specification | Filter applied | P1 |
| FUN-028 | Multiple entity types in list | List | Mixed entities | Correct filtering | P1 |
| FUN-029 | Document without type | Get | DocTypeId null | Handled | P1 |
| FUN-030 | Audit trail on create | Create | UploadDocument | Audit entry | P1 |
| FUN-031 | Audit trail on update | Update | UpdateDocument | Audit entry | P1 |
| FUN-032 | Audit trail on delete | Delete | DeleteDocument | Audit entry | P1 |
| FUN-033 | Idempotent delete | Delete twice | DeleteDocument twice | Graceful | P1 |
| FUN-034 | Update non-existent | Update | UpdateDocument(99999) | Null | P1 |
| FUN-035 | Get non-existent | Get | GetDocument(99999) | Null | P1 |
| FUN-036 | Storage path generation | Upload | UploadDocument | Correct path | P1 |
| FUN-037 | Filename sanitization | Upload | Malicious filename | Sanitized | P0 |
| FUN-038 | Quota enforcement | Upload | At quota | Rejected | P1 |
| FUN-039 | Content-Type validation | Upload | Mismatch extension | Rejected or flagged | P1 |
| FUN-040 | Relationship integrity | Link | Valid IDs | FK valid | P1 |
| FUN-041 | Orphan prevention | Delete entity | Entity with docs | Per cascade | P1 |
| FUN-042 | Status filter | List | Status=Active | Active only | P1 |
| FUN-043 | Date range filter | List | CreatedDate range | Filtered | P1 |
| FUN-044 | Search by name | Search | Name contains | Matches | P1 |
| FUN-045 | Search by description | Search | Description contains | Matches | P1 |
| FUN-046 | Bulk operations atomicity | Bulk | Upload batch | Per transaction | P2 |
| FUN-047 | Optimistic concurrency | Update | Concurrent | Conflict handling | P1 |
| FUN-048 | Document versioning (if any) | Update | Version exists | Per design | P2 |
| FUN-049 | Retention policy (if any) | Delete | Retention | Per policy | P2 |
| FUN-050 | Archive/restore (if any) | Archive | Document | Archived state | P2 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD workflow | Create→Get→Update→Delete | Document | All succeed | P0 |
| INT-002 | Document with Partner | Upload doc to Partner | Document, Partner | Link established | P0 |
| INT-003 | Document with Contact | Upload doc to Contact | Document, Contact | Link established | P0 |
| INT-004 | Document with Interaction | Upload doc to Interaction | Document, Interaction | Link established | P0 |
| INT-005 | DocumentTypeManager get type | Get document type for doc | Document, DocumentType | Type loaded | P0 |
| INT-006 | EntityArtifactManager | Document as artifact | Document, EntityArtifact | Link works | P1 |
| INT-007 | Permission check | Authorize document action | Document, PermissionService | Correct allow/deny | P0 |
| INT-008 | Audit log | Audit document CRUD | Document, AuditLog | Entries created | P1 |
| INT-009 | UserContext | Current user in request | Document, UserResolver | UserId applied | P0 |
| INT-010 | Storage service | Save to storage | Document, IStorageService | File stored | P0 |
| INT-011 | Virus scan service | Scan on upload | Document, IVirusScanService | Scan result | P0 |
| INT-012 | DbContext save | Persist to DB | Document, DbContext | Persisted | P0 |
| INT-013 | AutoMapper | Entity to Model | Document, AutoMapper | Correct mapping | P1 |
| INT-014 | Controller upload | API upload | Document, Controller | 201 Created | P0 |
| INT-015 | Controller download | API download | Document, Controller | 200 + stream | P0 |
| INT-016 | Controller list | API list | Document, Controller | 200 + list | P0 |
| INT-017 | Controller delete | API delete | Document, Controller | 204 | P0 |
| INT-018 | Error handling | Global handler | Document, ExceptionHandler | Consistent response | P1 |
| INT-019 | Logging | Log operations | Document, ILogger | Logs written | P2 |
| INT-020 | Configuration | Config for max size | Document, IConfiguration | Config applied | P2 |
| INT-021 | Opportunity document link | Doc for opportunity | Document, Opportunity | Link works | P1 |
| INT-022 | Partner document list | List partner docs | Document, PartnerManager | Correct list | P0 |
| INT-023 | Contact document list | List contact docs | Document, ContactManager | Correct list | P0 |
| INT-024 | Interaction document list | List interaction docs | Document, InteractionManager | Correct list | P0 |
| INT-025 | Document in list view | List view with docs | Document, ListView | Display correct | P1 |
| INT-026 | Document in detail view | Detail with docs | Document | All sections load | P0 |
| INT-027 | Document preview | Generate preview | Document, PreviewService | Preview generated | P2 |
| INT-028 | Document thumbnail | Generate thumbnail | Document, ThumbnailService | Thumbnail created | P2 |
| INT-029 | Notification on upload | Notify on upload | Document, NotificationManager | Notification sent | P2 |
| INT-030 | Document in Report | Report with docs | Document, Report | Data correct | P2 |
| INT-031 | API 404 | Get non-existent | Controller | 404 | P0 |
| INT-032 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-033 | API 403 | Unauthorized | Controller | 403 | P0 |
| INT-034 | API 413 | Payload too large | Controller | 413 | P0 |
| INT-035 | Repository pattern | CRUD via repository | Document, DataRepository | CRUD works | P1 |
| INT-036 | ManagerWrapper | Manager resolution | ManagerWrapper.DocumentManager | Correct manager | P1 |
| INT-037 | Validation service | Validate request | Document, Validator | Errors returned | P1 |
| INT-038 | Multi-tenant | Org scope | Document, Tenant | Data isolated | P0 |
| INT-039 | Feature flag | Feature for docs | Document, FeatureFlags | Flag respected | P2 |
| INT-040 | Blob storage | Cloud storage | Document, IBlobStorage | Stored in cloud | P1 |
| INT-041 | CDN integration | CDN for download | Document, CDN | Served from CDN | P2 |
| INT-042 | Metadata extraction | Extract metadata | Document, MetadataExtractor | Extracted | P2 |
| INT-043 | Full-text search | Search docs | Document, SearchService | Results returned | P1 |
| INT-044 | Version history | Document versions | Document, VersionService | Versions tracked | P2 |
| INT-045 | Share/link | Share document | Document, ShareService | Link created | P2 |
| INT-046 | Expiring link | Time-limited link | Document | Expires | P2 |
| INT-047 | Download tracking | Track downloads | Document, Analytics | Count updated | P2 |
| INT-048 | Storage quota | Quota check | Document, QuotaService | Enforced | P1 |
| INT-049 | Backup/restore | Backup docs | Document, BackupService | Backup created | P2 |
| INT-050 | Migration | Migrate documents | Document | Migration succeeds | P2 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | Path traversal filename | ../../../etc/passwd | Upload | Rejected | P0 |
| SEC-002 | Path traversal content | Malicious content | Upload | Sanitized | P0 |
| SEC-003 | SQL injection in search | ' OR 1=1-- | SearchDocuments | Sanitized | P0 |
| SEC-004 | XSS in document name | <script>alert(1)</script> | UpdateDocument | Sanitized | P0 |
| SEC-005 | XSS in description | <img src=x onerror=alert(1)> | UpdateDocument | Sanitized | P0 |
| SEC-006 | IDOR — get other org doc | GetDocument(otherId) | GetDocument | 403 | P0 |
| SEC-007 | IDOR — download other org | DownloadDocument(otherId) | Download | 403 | P0 |
| SEC-008 | IDOR — delete other org | DeleteDocument(otherId) | Delete | 403 | P0 |
| SEC-009 | IDOR — update other org | UpdateDocument(otherId) | Update | 403 | P0 |
| SEC-010 | Mass assignment Id | Include Id in request | Upload | Ignored | P0 |
| SEC-011 | Mass assignment CreatedBy | Include in request | Upload | Ignored | P0 |
| SEC-012 | Mass assignment StoragePath | Include in request | Upload | Ignored | P0 |
| SEC-013 | Unauthenticated upload | No auth | Upload | 401 | P0 |
| SEC-014 | Unauthenticated download | No auth | Download | 401 | P0 |
| SEC-015 | Expired token | Expired JWT | Any | 401 | P0 |
| SEC-016 | Wrong role upload | No permission | Upload | 403 | P0 |
| SEC-017 | Wrong role download | No permission | Download | 403 | P0 |
| SEC-018 | Wrong role delete | No permission | Delete | 403 | P0 |
| SEC-019 | Org scope bypass | Cross-org access | GetDocument | 403 | P0 |
| SEC-020 | Null byte injection | file.pdf%00.exe | Filename | Rejected | P0 |
| SEC-021 | Command injection | ; rm -rf / | Filename | Rejected | P0 |
| SEC-022 | XXE in Office doc | XML entity | Upload | Rejected | P0 |
| SEC-023 | Zip slip | Malicious zip | Extract | Rejected | P0 |
| SEC-024 | Polyglot file | PDF+HTML | Upload | Rejected | P0 |
| SEC-025 | Sensitive data in error | Connection string | Exception | Not exposed | P0 |
| SEC-026 | Sensitive data in response | Internal path | GetDocument | Not exposed | P0 |
| SEC-027 | Rate limit upload | Too many uploads | API | 429 | P1 |
| SEC-028 | Rate limit download | Too many downloads | API | 429 | P1 |
| SEC-029 | CSRF upload | Cross-site request | Upload | Token validated | P0 |
| SEC-030 | CSRF delete | Cross-site request | Delete | Token validated | P0 |
| SEC-031 | Header injection | Malicious headers | Request | Sanitized | P1 |
| SEC-032 | Brute force doc IDs | Enumerate IDs | GetDocument | Rate limited | P1 |
| SEC-033 | Insecure direct reference | EntityId manipulation | LinkDocument | Validated | P0 |
| SEC-034 | Timing attack | Response time | GetDocument | Constant time | P2 |
| SEC-035 | Replay attack | Replay request | Upload | Nonce/timestamp | P1 |
| SEC-036 | JWT alg none | alg=none | Request | Rejected | P0 |
| SEC-037 | Storage path injection | Custom path | Upload | Rejected | P0 |
| SEC-038 | Symbolic link | Symlink in path | Upload | Rejected | P0 |
| SEC-039 | Log injection | Malicious log | Log field | Sanitized | P1 |
| SEC-040 | Metadata injection | Script in metadata | Metadata | Sanitized | P0 |
| SEC-041 | Content-Disposition injection | Malicious header | Download | Sanitized | P1 |
| SEC-042 | MIME sniffing | Wrong content-type | Upload | Validated | P1 |
| SEC-043 | Open redirect | Redirect in callback | OAuth | Validated | P1 |
| SEC-044 | Parameter pollution | id=1&id=2 | GetDocument | Handled | P1 |
| SEC-045 | HTTP verb tampering | PUT instead of POST | Upload | 405 | P1 |
| SEC-046 | Cookie manipulation | Modify auth cookie | Request | Rejected | P0 |
| SEC-047 | Session fixation | Fixate session | Login | New session | P1 |
| SEC-048 | Excessive data | Huge PageSize | ListDocuments | Capped | P1 |
| SEC-049 | Information disclosure | Probe errors | Invalid input | Generic message | P1 |
| SEC-050 | Double extension | file.pdf.exe | Upload | Rejected | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent list same entity | 20 threads ListDocuments(Partner, 123) | All correct | P0 |
| CON-002 | Concurrent update same document | 5 threads UpdateDocument(789) | No corruption | P0 |
| CON-003 | Concurrent upload same entity | 10 threads Upload to Partner 123 | All created | P0 |
| CON-004 | Concurrent download same doc | 20 threads DownloadDocument(789) | All succeed | P0 |
| CON-005 | Concurrent delete same doc | 2 threads DeleteDocument(789) | One succeeds | P0 |
| CON-006 | Upload and download same | Thread1 upload, Thread2 download | Consistent | P1 |
| CON-007 | Update and get | Thread1 update, Thread2 get | Consistent | P1 |
| CON-008 | Delete and get | Thread1 delete, Thread2 get | Null or 404 | P0 |
| CON-009 | Concurrent link | 2 threads LinkDocument same | One succeeds | P1 |
| CON-010 | Concurrent unlink | 2 threads Unlink same | One succeeds | P1 |
| CON-011 | Optimistic concurrency | 2 users update same doc | Conflict handling | P0 |
| CON-012 | Connection pool | 100 concurrent ops | No exhaustion | P1 |
| CON-013 | Deadlock | Circular dependency | No deadlock | P1 |
| CON-014 | Transaction isolation | Read uncommitted | Per isolation | P1 |
| CON-015 | Lost update | 2 users update different fields | Per design | P1 |
| CON-016 | Phantom read | Insert during paginate | Per isolation | P2 |
| CON-017 | Cache poisoning | Concurrent cache updates | Consistent | P1 |
| CON-018 | Double submit | User double-clicks Upload | One doc | P0 |
| CON-019 | Race on duplicate name | 2 threads upload same name | Handled | P1 |
| CON-020 | Bulk upload concurrency | 2 threads bulk upload | Consistent | P1 |
| CON-021 | Virus scan during concurrent | Scan + upload | No race | P1 |
| CON-022 | Storage write conflict | Same path | Handled | P1 |
| CON-023 | Metadata update conflict | 2 users update metadata | One wins | P1 |
| CON-024 | Document type assignment | 2 threads assign type | One wins | P1 |
| CON-025 | List during bulk upload | Thread1 upload 100, Thread2 list | Consistent | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | File extension validation | Validation | .pdf | Valid | P0 |
| UNT-002 | File extension invalid | Validation | .exe | Invalid | P0 |
| UNT-003 | MIME type validation | Validation | application/pdf | Valid | P0 |
| UNT-004 | File size validation | Validation | 5MB | Valid | P0 |
| UNT-005 | Filename sanitization | Formatting | "file<>name.pdf" | Sanitized | P0 |
| UNT-006 | Path combination | Formatting | base + relative | Correct path | P1 |
| UNT-007 | Content-Disposition header | Formatting | filename with spaces | Encoded | P1 |
| UNT-008 | Hash calculation | Calculation | File bytes | SHA256 | P1 |
| UNT-009 | Metadata merge | Calculation | Existing + new | Merged | P1 |
| UNT-010 | Status — Active | Status logic | IsDeleted=false | Active | P1 |
| UNT-011 | Status — Deleted | Status logic | IsDeleted=true | Excluded | P0 |
| UNT-012 | IsFolder check | Status logic | IsFolder=true | Excluded from doc list | P0 |
| UNT-013 | Collection filter | Collections | List with deleted | Deleted excluded | P1 |
| UNT-014 | Empty collection | Collections | No docs | Count=0 | P1 |
| UNT-015 | Null to empty | Collections | Null list | Return [] | P1 |
| UNT-016 | Map Document to Model | Mapping | Document entity | DocumentModel | P0 |
| UNT-017 | Map Request to Entity | Mapping | UploadRequest | Document entity | P0 |
| UNT-018 | Pagination slice | Calculation | PageIndex=1, Size=10 | Skip 10, Take 10 | P1 |
| UNT-019 | Entity type enum | Validation | "Partner" | Partner | P1 |
| UNT-020 | Audit fields default | Status logic | New document | CreatedBy, CreatedDate | P1 |
| UNT-021 | Relationship count | Calculation | Doc with 3 links | Count=3 | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | List 1000 documents | ListDocuments | < 1000ms | P0 |
| PRF-002 | Get document by ID | GetDocument | < 200ms | P0 |
| PRF-003 | Upload 5MB file | UploadDocument | < 3000ms | P0 |
| PRF-004 | Download 10MB file | DownloadDocument | < 2000ms | P0 |
| PRF-005 | Batch update 100 docs | Batch UpdateDocument | < 2000ms | P1 |
| PRF-006 | Document relationship query | GetDocument with includes | < 500ms | P1 |
| PRF-007 | List with DocumentType join | ListDocuments | < 300ms | P1 |
| PRF-008 | Search 10K documents | SearchDocuments | < 1000ms | P0 |
| PRF-009 | Pagination — 10K docs | ListDocuments page 1 | < 500ms | P1 |
| PRF-010 | Get parent entity | GetParentEntity | < 300ms | P1 |
| PRF-011 | Virus scan overhead | Upload with scan | < 5000ms | P1 |
| PRF-012 | Memory — list 50 docs | ListDocuments PageSize=50 | < 50MB | P1 |
| PRF-013 | Metadata update | UpdateDocument | < 500ms | P1 |
| PRF-014 | Multiple entities list | List across 10 entities | < 1500ms | P1 |
| PRF-015 | Cold start first query | First ListDocuments | < 500ms | P2 |
| PRF-016 | Cached query | Subsequent ListDocuments | < 100ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained upload — 5 req/s | 5 uploads/sec | 5 min | 95% < 3000ms | P0 |
| LDT-002 | Sustained download — 20 req/s | 20 downloads/sec | 5 min | 95% < 1000ms | P0 |
| LDT-003 | Sustained list — 50 req/s | 50 ListDocuments/sec | 5 min | 95% < 500ms | P0 |
| LDT-004 | Spike — 50 uploads for 1 min | 50 uploads/sec burst | 1 min | No crash | P0 |
| LDT-005 | Spike — 100 downloads for 30 sec | 100 downloads/sec | 30 sec | Degrade gracefully | P1 |
| LDT-006 | Stress — ramp to failure | 1→200 req/s | Until failure | Identify limit | P1 |
| LDT-007 | Stress — connection pool | 150 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Stress — disk I/O | Many large uploads | 5 min | No hang | P1 |
| LDT-009 | Recovery — after spike | Spike then normal | 5 min | Return to baseline | P0 |
| LDT-010 | Recovery — after stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
