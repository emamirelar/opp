# DocumentManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DocumentManager`  
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

**DocumentManager** manages document upload/download, file type validation, virus scan integration, metadata management, and entity linking. Key responsibilities: document CRUD, entity-document relationships, document type assignment, filtering (exclude deleted, exclude folders), metadata updates, and parent entity resolution.

---

## §1 Positive Tests (30)

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
---

## §2 Negative Tests (90)

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

| NEG-071 | GetFileContentById — document has no blob | Doc has Link only, no Blob | Exception "Document has no blob content" | P0 |
| NEG-072 | GetFileContentById — document has GCS path only | Doc has StoragePath, no Blob | Exception "Document has no blob content" | P0 |
| NEG-073 | UpdateDocument — request Id zero | UpdateDocumentAsync(Id=0) | Null or error | P1 |
| NEG-074 | UpdateDocument — request Id negative | UpdateDocumentAsync(Id=-1) | Null | P1 |
| NEG-075 | ListDocuments — entityName unknown | EntityNames.ByName("Invalid") | Empty list (no match) | P1 |
| NEG-076 | ListDocuments — entityName empty string | EntityName="" | Empty list | P1 |
| NEG-077 | GetDocumentParentEntity — doc has multiple relationships | Doc with 2+ DocumentRelationships | SingleOrDefault may return first or InvalidOp | P1 |
| NEG-078 | UpdateDocument — DocumentTypeId non-existent | DocumentTypeId=99999 | FK error or handled | P1 |
| NEG-079 | GetDocumentById — document soft-deleted | Doc IsDeleted=true | May return (manager has no filter) | P1 |
| NEG-080 | ListDocuments — entity has no relationships | EntityId with no DocumentRelationships | Empty list | P1 |
| NEG-081 | Document entity validation — all null | Link, Blob, StoragePath all null | ValidationResult | P1 |
| NEG-082 | UpdateDocument — null request | UpdateDocumentAsync(null) | NullRef or ArgumentNull | P1 |
| NEG-083 | GetDocumentViewUrl — doc has no content | Doc with null Link, Blob, StoragePath | BusinessException | P0 |
| NEG-084 | Download — doc has Link only | Doc stored as Link, no Blob | GetFileContentById throws | P0 |
| NEG-085 | GenerateGoogleDoc — empty data | Request.Data empty | ArgumentException | P0 |
| NEG-086 | GenerateGoogleDoc — null data | Request.Data null | ArgumentException | P0 |
| NEG-087 | GetDocumentViewUrl — non-existent doc | GetDocumentViewUrl(99999) | BusinessException "Document not found" | P0 |
| NEG-088 | Update — document not found | UpdateDocumentAsync(Id=99999) | Returns default (null) | P1 |
| NEG-089 | GetDocumentParentEntity — doc not found | GetDocumentParentEntityByIdAsync(99999) | Null | P1 |
| NEG-090 | ListDocuments — entity type PartnerTree | EntityName="partnerTree" | Empty or per EntityNames mapping | P1 |

---

## §3 Boundary Tests (90)

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

| BND-071 | UpdateDocumentRequest DocumentTypeId | null | Valid ID | null (optional) | Valid | Invalid ID | P1 |
| BND-072 | Document Type field | "folder" | "application/pdf" | "folder" excluded | Valid MIME | — | P1 |
| BND-073 | DocumentRelationship EntityType | "Contact" | "Opportunity" | Exact match | All supported | Unknown | P1 |
| BND-074 | Blob array length | 0 | Max | Empty blob | Large file | — | P1 |
| BND-075 | StoragePath gs:// prefix | — | — | gs://bucket/key | Signed URL | — | P1 |
| BND-076 | Document Id in UpdateRequest | 1 | 2147483647 | 1 | Max | 0, -1 | P1 |
| BND-077 | EntityId in ListDocuments | 1 | 2147483647 | 1 | Max | 0 | P1 |
| BND-078 | DocumentRelationship EntityId | 1 | 2147483647 | 1 | Max | 0 | P1 |
| BND-079 | Link URL length | 1 | 2048 | Short URL | Long URL | — | P2 |
| BND-080 | GoogleId length | — | — | Valid ID | — | — | P2 |
| BND-081 | EntityNames.ByName case | "partner" | "Partner" | Both map to Partner | — | — | P1 |
| BND-082 | AITranscribed flag | false | true | false | true | — | P2 |
| BND-083 | DocumentTypeId nullable | null | Valid | null allowed | Valid | 0 | P1 |
| BND-084 | InteractionId on Document | null | Valid | null | Valid | — | P2 |
| BND-085 | GetDocumentViewUrl signed URL expiry | 1 min | 60 min | — | 60 min | — | P2 |
| BND-086 | GenerateGoogleDoc filename | 1 char | 255 | "a" | Long name | — | P1 |
| BND-087 | Content-Type in download | application/octet-stream | Specific | Fallback | document.Type | — | P1 |
| BND-088 | DocumentRelationships count | 0 | N | 0 (no parent) | Multiple | — | P1 |
| BND-089 | Include chains depth | DocumentType | DocumentRelationships | 1 level | 2 levels | — | P1 |
| BND-090 | ModifiableDeletableEntity Name | 1 | 255 | "a" | 255 chars | 256 | P1 |

---

## §4 Functional Tests (90)

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

| FUN-051 | ListDocuments filters Type != folder | Exclude folders | ListDocumentsAsync | Only files returned | P0 |
| FUN-052 | ListDocuments filters !IsDeleted | Exclude soft-deleted | ListDocumentsAsync | Deleted excluded | P0 |
| FUN-053 | ListDocuments requires DocumentRelationship match | EntityType + EntityId | ListDocumentsAsync | Only docs linked to entity | P0 |
| FUN-054 | GetDocumentById includes DocumentType | Load type | GetDocumentByIdAsync | DocumentType populated | P1 |
| FUN-055 | GetDocumentParentEntity SingleOrDefault | One relationship | GetDocumentParentEntityByIdAsync | (EntityId, EntityType) | P1 |
| FUN-056 | GetDocumentParentEntity null when no relationship | Doc unlinked | GetDocumentParentEntityByIdAsync | Null | P1 |
| FUN-057 | UpdateDocument maps Id and DocumentTypeId | Update request | UpdateDocumentAsync | Entity updated | P0 |
| FUN-058 | UpdateDocument returns default when not found | Non-existent ID | UpdateDocumentAsync | default (null) | P1 |
| FUN-059 | GetFileContentById returns Blob | Doc has Blob | GetFileContentByIdAsync | byte[] returned | P0 |
| FUN-060 | GetFileContentById throws when no blob | Link/StoragePath only | GetFileContentByIdAsync | Exception | P0 |
| FUN-061 | GetFileContentById throws when doc not found | Invalid ID | GetFileContentByIdAsync | "Document not found" | P0 |
| FUN-062 | Document entity validation | Link, Blob, StoragePath | Validate | At least one required | P1 |
| FUN-063 | EntityNames.ByName maps contact/Contact | Case variants | ListDocuments | Same result | P1 |
| FUN-064 | EntityNames.ByName maps partner/Partner | Case variants | ListDocuments | Same result | P1 |
| FUN-065 | EntityNames.ByName maps interaction/Interaction | Case variants | ListDocuments | Same result | P1 |
| FUN-066 | EntityNames.ByName returns empty for unknown | Invalid entity | ListDocuments | No match, empty | P1 |
| FUN-067 | GetDocumentViewUrl GCS path | StoragePath gs:// | GetDocumentViewUrl | Signed URL | P0 |
| FUN-068 | GetDocumentViewUrl Link | Document has Link | GetDocumentViewUrl | Link returned | P0 |
| FUN-069 | GetDocumentViewUrl Blob fallback | Blob storage | GetDocumentViewUrl | /api/document/{id}/download | P1 |
| FUN-070 | Download returns FileResult | Doc has Blob | DownloadDocument | File(content, type, name) | P0 |
| FUN-071 | Update calls GetDocumentParentEntity first | Permission check | DocumentController.Update | Parent resolved | P1 |
| FUN-072 | GenerateGoogleDoc uses markdown directly | Skip AI | GenerateGoogleDoc | ConvertMarkdownToGoogleDoc | P1 |
| FUN-073 | GenerateGoogleDoc default filename | No filename | GenerateGoogleDoc | "Generated_Document" | P1 |
| FUN-074 | GetAll uses EntityNames.ByName | Route entityName | DocumentController.GetAll | Normalized entity type | P0 |
| FUN-075 | Get throws BusinessException when null | Doc not found | DocumentController.Get | BusinessException | P0 |
| FUN-076 | Download throws when doc null | After GetFileContent | DocumentController.Download | BusinessException | P0 |
| FUN-077 | DocumentType optional on Document | DocumentTypeId null | GetDocumentByIdAsync | Handled | P1 |
| FUN-078 | DocumentRelationships eager loaded | ListDocuments | GetAll includes | Relationships loaded | P1 |
| FUN-079 | AutoMapper Document to DocumentModel | Entity mapping | GetDocumentByIdAsync | DocumentModel | P0 |
| FUN-080 | AutoMapper UpdateDocumentRequest to entity | Update mapping | UpdateDocumentAsync | Entity updated | P0 |
| FUN-081 | DataRepository GetAll with includes | DocumentRelationships, DocumentType | ListDocuments | Includes applied | P1 |
| FUN-082 | GetByIdAsync with includes array | DocumentType | GetDocumentByIdAsync | Include applied | P1 |
| FUN-083 | HandleOperationAsync wraps manager calls | Controller pattern | All endpoints | Consistent handling | P1 |
| FUN-084 | GetDocumentViewUrl type field | Response shape | GetDocumentViewUrl | url, type, mimeType | P1 |
| FUN-085 | Document Name from ModifiableDeletableEntity | Required field | Document entity | Name set | P1 |
| FUN-086 | DocumentRelationship EntityType required | required string | DocumentRelationship | Non-null | P1 |
| FUN-087 | Document Blob nullable | Optional storage | Document entity | Blob can be null | P1 |
| FUN-088 | Document Link nullable | Optional external link | Document entity | Link can be null | P1 |
| FUN-089 | Document StoragePath nullable | Optional GCS path | Document entity | StoragePath can be null | P1 |
| FUN-090 | GetDocumentsByEntityAsync same filter as ListDocuments | Consistency | GetDocumentsByEntityAsync | Same results | P1 |

---

## §5 Integration Tests (90)

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

| INT-051 | DocumentController GetAll → ListDocumentsAsync | GET /document/{entityName}/{entityId} | Document, DocumentManager | 200 + list | P0 |
| INT-052 | DocumentController Get → GetDocumentByIdAsync | GET /document/{id} | Document, DocumentManager | 200 + document | P0 |
| INT-053 | DocumentController Update → UpdateDocumentAsync | PUT /document | Document, UpdateDocumentRequest | 200 | P0 |
| INT-054 | DocumentController Download → GetFileContentByIdAsync | GET /document/download/{id} | Document, DocumentManager | 200 + file | P0 |
| INT-055 | DocumentController GetDocumentViewUrl → GetDocumentByIdAsync | GET /document/view-url/{id} | Document, GCS | 200 + url | P0 |
| INT-056 | DocumentController GenerateGoogleDoc → CloudRunHelper | POST /document/generate | Document, CloudRun API | 200 + result | P1 |
| INT-057 | ManagerWrapper.DocumentManager resolution | DI | ManagerWrapper, DocumentManager | Correct manager | P1 |
| INT-058 | Document → DocumentRelationship FK | Load doc with relationships | Document, DocumentRelationship | Relationships loaded | P0 |
| INT-059 | Document → DocumentType FK | Load doc with type | Document, DocumentType | DocumentType loaded | P0 |
| INT-060 | DocumentRelationship → EntityId, EntityType | Link to Partner | DocumentRelationship, Partner | EntityId, EntityType set | P0 |
| INT-061 | DocumentController HasPermission → ContactManager | Update with Contact parent | Document, Contact, Authorization | Permission checked | P1 |
| INT-062 | DocumentController HasPermission → PartnerManager | Update with Partner parent | Document, Partner, Authorization | Permission checked | P1 |
| INT-063 | GetDocumentViewUrl → GoogleCloudStorageService | GCS signed URL | Document, GCS, StoragePath | Signed URL generated | P0 |
| INT-064 | Download → File(content, type, name) | Blob download | Document, Controller | Correct Content-Type | P0 |
| INT-065 | UpdateDocumentRequest → AutoMapper → Document | Update flow | UpdateDocumentRequest, Document | DocumentTypeId mapped | P0 |
| INT-066 | Document entity → AutoMapper → DocumentModel | Get flow | Document, DocumentModel | All fields mapped | P0 |
| INT-067 | DataRepository<Document> CRUD | Manager uses repository | DocumentManager, DataRepository | CRUD works | P0 |
| INT-068 | DocumentController HandleOperationAsync | All endpoints | Controller, BaseController | Consistent response | P1 |
| INT-069 | GenerateGoogleDoc → ConvertMarkdownToGoogleDoc | External API call | DocumentController, CloudRun | Markdown converted | P1 |
| INT-070 | EntityNames.ByName in GetAll | Route param | EntityNames, DocumentController | Entity type normalized | P0 |
| INT-071 | Document with Blob storage | Legacy blob | Document, GetFileContentByIdAsync | Blob returned | P0 |
| INT-072 | Document with StoragePath (GCS) | Cloud storage | Document, GetDocumentViewUrl | Signed URL | P0 |
| INT-073 | Document with Link (Google Drive) | External link | Document, GetDocumentViewUrl | Link returned | P0 |
| INT-074 | DocumentController constructor deps | DI resolution | Controller, IManagerWrapper, IConfiguration | All resolved | P1 |
| INT-075 | GoogleCloudStorageService in DocumentController | GCS init | DocumentController, GCS | Service created | P1 |
| INT-076 | CloudRunHelper in DocumentController | GenerateGoogleDoc | DocumentController, CloudRunHelper | Helper created | P1 |
| INT-077 | GetCredentials from AISettings | Config | DocumentController, IConfiguration | Credentials loaded | P1 |
| INT-078 | Document BaseController inheritance | Authorization | DocumentController, BaseController | Base behavior | P1 |
| INT-079 | APIDictionary.Document routes | Routing | DocumentController, APIDictionary | Correct routes | P1 |
| INT-080 | DocumentController UserResolverService | Current user | BaseController, UserResolver | User context | P1 |
| INT-081 | DocumentController IAuthorizationService | Auth | BaseController, Authorization | Auth available | P1 |
| INT-082 | DocumentController ILogger | Logging | DocumentController, ILogger | Logs written | P2 |
| INT-083 | GenerateGoogleDoc timeout | 60 sec | HttpClient, CloudRun | Timeout applied | P2 |
| INT-084 | MultipartFormDataContent in GenerateGoogleDoc | Request format | GenerateGoogleDoc | Form data correct | P1 |
| INT-085 | Document Model IModifiableEntityModel | Audit fields | DocumentModel | CreatedBy, LastModifiedBy | P1 |
| INT-086 | DocumentModel DocumentTypeModel | Nested model | DocumentModel, DocumentTypeModel | Type included | P1 |
| INT-087 | UpdateDocumentRequest Id required | Request validation | UpdateDocumentRequest | Id required | P1 |
| INT-088 | DocumentRelationship ModifiableDeletableEntity | Audit | DocumentRelationship | Inherits audit | P1 |
| INT-089 | Document ModifiableDeletableEntity | Audit, soft delete | Document | Name, IsDeleted | P0 |
| INT-090 | Document IValidatableObject | Entity validation | Document.Validate | ValidationResult | P1 |

---

## §6 Concurrency Tests (25)

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

## §7 Unit Tests (21)

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

## §8 Performance Tests (16)

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

## §9 Load Tests (10)

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
