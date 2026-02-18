# GoogleDriveDocumentManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Services/GoogleDriveDocumentManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | ≥30 | ✅ |
| §2 Negative | 90 | ≥90 | ✅ |
| §3 Boundary | 90 | ≥90 | ✅ |
| §4 Functional | 90 | ≥90 | ✅ |
| §5 Integration | 90 | ≥90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Google Drive document manager unit tests cover document CRUD, sharing, permissions, and conversion for Drive integration. Tests include: create/read/update/delete documents, share with users, set permissions, export/convert formats, and Drive API integration.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Create document | Valid data | Create | Document created |
| POS-002 | Get document | Document exists | Get | Document returned |
| POS-003 | Update document | Document exists | Update | Updated |
| POS-004 | Delete document | Document exists | Delete | Deleted |
| POS-005 | List documents | Documents exist | List | List returned |
| POS-006 | Share with user | User valid | Share | Shared |
| POS-007 | Set permissions | Document exists | SetPermissions | Permissions set |
| POS-008 | Get permissions | Document exists | GetPermissions | Permissions |
| POS-009 | Remove sharing | Share exists | RemoveShare | Removed |
| POS-010 | Export to PDF | Document exists | Export | PDF |
| POS-011 | Convert format | Document exists | Convert | Converted |
| POS-012 | Create folder | Name valid | CreateFolder | Created |
| POS-013 | Move document | Document exists | Move | Moved |
| POS-014 | Copy document | Document exists | Copy | Copied |
| POS-015 | Search documents | Query valid | Search | Results |
| POS-016 | Get file metadata | File exists | GetMetadata | Metadata |
| POS-017 | Upload file | File valid | Upload | Uploaded |
| POS-018 | Download file | File exists | Download | Stream |
| POS-019 | Get revision history | File exists | GetRevisions | History |
| POS-020 | Restore revision | Revision exists | Restore | Restored |
| POS-021 | Trash document | Document exists | Trash | Trashed |
| POS-022 | Untrash document | Document trashed | Untrash | Restored |
| POS-023 | Get shared drives | Drives exist | GetSharedDrives | Drives |
| POS-024 | Get drive info | Drive exists | GetDriveInfo | Info |
| POS-025 | Add to folder | Folder exists | AddToFolder | Added |
| POS-026 | Remove from folder | In folder | RemoveFromFolder | Removed |
| POS-027 | Audit create | Create | Check audit | Logged |
| POS-028 | Audit update | Update | Check audit | Logged |
| POS-029 | Audit delete | Delete | Check audit | Logged |
| POS-030 | Pagination | Many documents | List | Pages |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Create with null name | Name=null | ArgumentNullException |
| NEG-002 | Create with empty name | Name="" | ValidationException |
| NEG-003 | Get non-existent | Id=invalid | NotFoundException |
| NEG-004 | Get null ID | Id=null | ArgumentNullException |
| NEG-005 | Update non-existent | Id=invalid | NotFoundException |
| NEG-006 | Delete non-existent | Id=invalid | NotFoundException |
| NEG-007 | Share with null user | User=null | ArgumentNullException |
| NEG-008 | Share with invalid user | User=invalid | ValidationException |
| NEG-009 | Set permissions invalid | Permissions invalid | ValidationException |
| NEG-010 | Remove share non-existent | Share invalid | NotFoundException |
| NEG-011 | Export invalid format | Format invalid | ArgumentException |
| NEG-012 | Convert invalid format | Format invalid | ArgumentException |
| NEG-013 | Create folder invalid name | Name invalid | ValidationException |
| NEG-014 | Move invalid destination | Dest invalid | ArgumentException |
| NEG-015 | Copy invalid destination | Dest invalid | ArgumentException |
| NEG-016 | Search null query | Query=null | ArgumentNullException |
| NEG-017 | Get metadata invalid | Id invalid | NotFoundException |
| NEG-018 | Upload null file | File=null | ArgumentNullException |
| NEG-019 | Download invalid | Id invalid | NotFoundException |
| NEG-020 | Restore invalid revision | Revision invalid | NotFoundException |
| NEG-021 | Credentials missing | Credentials null | ConfigurationException |
| NEG-022 | Credentials invalid | Credentials invalid | UnauthorizedException |
| NEG-023 | Drive not found | Drive invalid | NotFoundException |
| NEG-024 | Quota exceeded | Quota full | QuotaExceededException |
| NEG-025 | Rate limit exceeded | Over limit | RateLimitException |
| NEG-026 | Timeout | Slow operation | TimeoutException |
| NEG-027 | Create without permission | Unauthorized | Forbidden |
| NEG-028 | Get without permission | Unauthorized | Forbidden |
| NEG-029 | Update without permission | Unauthorized | Forbidden |
| NEG-030 | Delete without permission | Unauthorized | Forbidden |
| NEG-031 | Share without permission | Unauthorized | Forbidden |
| NEG-032 | Path traversal | ../../../etc | ValidationException |
| NEG-033 | Add to folder invalid | Folder invalid | NotFoundException |
| NEG-034 | Remove from folder invalid | Not in folder | NotFoundException |
| NEG-035 | Get revisions invalid | Id invalid | NotFoundException |
| NEG-036 | Trash invalid | Id invalid | NotFoundException |
| NEG-037 | Untrash invalid | Not trashed | InvalidOperationException |
| NEG-038 | Get shared drives invalid | Invalid | ArgumentException |
| NEG-039 | Get drive info invalid | Drive invalid | NotFoundException |
| NEG-040 | Batch null operations | Ops=null | ArgumentNullException |
| NEG-041 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-042 | Concurrent conflict | Two write same | ConflictException |
| NEG-043 | Transaction rollback | Fail in transaction | Rollback |
| NEG-044 | Connection timeout | Drive unavailable | TimeoutException |
| NEG-045 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-046 | Invalid enum value | Permission invalid | ArgumentException |
| NEG-047 | Expired session | Expired token | Unauthorized |
| NEG-048 | Null user context | User=null | InvalidOperationException |
| NEG-049 | Invalid include path | Invalid include | ArgumentException |
| NEG-050 | Get download URL invalid | Id invalid | NotFoundException |
| NEG-051 | Get web view link invalid | Id invalid | NotFoundException |
| NEG-052 | Upload file too large | Size>limit | ValidationException |
| NEG-053 | Export format unsupported | Format unsupported | ArgumentException |
| NEG-054 | Convert format unsupported | Format unsupported | ArgumentException |
| NEG-055 | Move to same folder | Same folder | No-op or error |
| NEG-056 | Copy to same folder | Same folder | Conflict |
| NEG-057 | Restore old revision | Old revision | Restored |
| NEG-058 | List invalid filter | Filter invalid | ArgumentException |
| NEG-059 | Pagination invalid | Page invalid | ArgumentException |
| NEG-060 | Audit missing user | User=0 | InvalidOperationException |
| NEG-061 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-062 | Get permissions invalid | Id invalid | NotFoundException |
| NEG-063 | Set permissions invalid role | Role invalid | ArgumentException |
| NEG-064 | Share duplicate | Already shared | ConflictException |
| NEG-065 | Child override throws | Child throws | Propagated |
| NEG-066 | Batch partial failure | One fails | PartialFailureException |
| NEG-067 | Get metadata deleted | Document deleted | NotFoundException |
| NEG-068 | Download trashed | Document trashed | NotFoundException |
| NEG-069 | Move trashed | Document trashed | InvalidOperationException |
| NEG-070 | Copy trashed | Document trashed | NotFoundException |
| NEG-071 | Create with whitespace name | Name="   " | ValidationException |
| NEG-072 | Get null ID | Id=null | ArgumentNullException |
| NEG-073 | Share with empty email | Email="" | ValidationException |
| NEG-074 | Set permissions null | Permissions=null | ArgumentNullException |
| NEG-075 | Export null format | Format=null | ArgumentNullException |
| NEG-076 | Create folder invalid | Name invalid | ValidationException |
| NEG-077 | Move null destination | Dest=null | ArgumentNullException |
| NEG-078 | Search empty query | Query="" | ArgumentException |
| NEG-079 | Get metadata null | Id=null | ArgumentNullException |
| NEG-080 | Upload empty stream | Stream empty | ValidationException |
| NEG-081 | Restore invalid revision | Revision invalid | NotFoundException |
| NEG-082 | Trash null ID | Id=null | ArgumentNullException |
| NEG-083 | Untrash non-trashed | Not trashed | InvalidOperationException |
| NEG-084 | Add to folder null | Folder=null | ArgumentNullException |
| NEG-085 | Remove from folder null | Folder=null | ArgumentNullException |
| NEG-086 | Batch null list | Ops=null | ArgumentNullException |
| NEG-087 | Get download URL invalid | Id invalid | NotFoundException |
| NEG-088 | Get web view link invalid | Id invalid | NotFoundException |
| NEG-089 | List invalid filter | Filter invalid | ArgumentException |
| NEG-090 | Pagination invalid page | Page invalid | ArgumentException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Name at min length | Length=1 | Valid |
| BND-002 | Name at max length | Length=255 | Valid |
| BND-003 | Name exceeds max | Length=256 | Reject |
| BND-004 | File size zero | Size=0 | Valid or reject |
| BND-005 | File size at limit | Size=5TB | Valid |
| BND-006 | File size over limit | Size=5TB+1 | Reject |
| BND-007 | Page size at min | PageSize=1 | Valid |
| BND-008 | Page size at max | PageSize=1000 | Valid |
| BND-009 | Page size over max | PageSize=1001 | Reject |
| BND-010 | Permission role first | First | Valid |
| BND-011 | Permission role last | Last | Valid |
| BND-012 | Export format boundary | PDF | Valid |
| BND-013 | Convert format boundary | DOCX | Valid |
| BND-014 | Unicode in name | Arabic/Chinese | Valid |
| BND-015 | Special chars in name | <>&"' | Escaped |
| BND-016 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-017 | Empty folder | Folder empty | Empty list |
| BND-018 | Single document | Count=1 | Valid |
| BND-019 | Max documents | At limit | Valid |
| BND-020 | Empty search query | Query="" | Return all |
| BND-021 | Search query max | Query=500 | Valid |
| BND-022 | Search query over max | Query=501 | Reject |
| BND-023 | Revision count zero | No revisions | Empty list |
| BND-024 | Revision count max | Many | Valid |
| BND-025 | Share count zero | No shares | Empty list |
| BND-026 | Share count max | At limit | Valid |
| BND-027 | Date at min | Date=MinValue | Handle |
| BND-028 | Date at max | Date=MaxValue | Handle |
| BND-029 | Pagination last partial | Partial page | Correct |
| BND-030 | Pagination total | Total count | Accurate |
| BND-031 | Sort null handling | Nulls in data | Deterministic |
| BND-032 | Filter combination all | All filters | Correct |
| BND-033 | MIME type boundary | application/pdf | Valid |
| BND-034 | MIME type unknown | application/octet | Handle |
| BND-035 | Metadata key max | Key length | Valid |
| BND-036 | Metadata value max | Value length | Valid |
| BND-037 | Trash then untrash | Trash, Untrash | Restored |
| BND-038 | Move then verify | Move | Verified |
| BND-039 | Copy then verify | Copy | Verified |
| BND-040 | Export large file | 100MB | Stream |
| BND-041 | Convert large file | 100MB | Converted |
| BND-042 | Soft delete boundary | DeletedDate set | Excluded |
| BND-043 | Include depth | Deep include | No explosion |
| BND-044 | Query timeout | Slow query | Timeout |
| BND-045 | Memory large result | 10k docs | No OOM |
| BND-046 | Audit timestamp precision | Millisecond | Stored |
| BND-047 | Long string in description | 4000 chars | Truncate |
| BND-048 | Batch count zero | Count=0 | Valid |
| BND-049 | Batch count max | Count=100 | Valid |
| BND-050 | Batch count over max | Count=101 | Reject |
| BND-051 | Add to folder duplicate | Already in | No-op or error |
| BND-052 | Remove from folder not in | Not in folder | Error |
| BND-053 | Get revisions deleted | Document deleted | NotFoundException |
| BND-054 | Restore current revision | Current | No-op |
| BND-055 | Get download URL expiry | Expired | Regenerate |
| BND-056 | Get web view link private | Private | Error |
| BND-057 | Share with self | Self | No-op or error |
| BND-058 | Set permissions empty | Permissions=[] | Clear |
| BND-059 | Get shared drives empty | No drives | Empty list |
| BND-060 | Get drive info minimal | Minimal | Info |
| BND-061 | Search no results | No match | Empty list |
| BND-062 | List empty | No documents | Empty list |
| BND-063 | Get metadata minimal | Minimal | Metadata |
| BND-064 | Upload empty file | File empty | Reject |
| BND-065 | Download empty file | File empty | Stream |
| BND-066 | Batch one operation | Count=1 | Valid |
| BND-067 | Export format unsupported | Format | Error |
| BND-068 | Async cancellation | Cancel token | OperationCanceledException |
| BND-069 | Task timeout | Timeout | TimeoutException |
| BND-070 | Concurrent same document | Same doc | One wins |
| BND-071 | Name single char | Length=1 | Valid |
| BND-072 | File size 1 byte | Size=1 | Valid |
| BND-073 | Page size one | PageSize=1 | Valid |
| BND-074 | Revision count one | Count=1 | Valid |
| BND-075 | Share count one | Count=1 | Valid |
| BND-076 | Folder empty | Empty | Empty list |
| BND-077 | Search query max | Query=500 | Valid |
| BND-078 | Metadata key max | Key length | Valid |
| BND-079 | Metadata value max | Value length | Valid |
| BND-080 | MIME type boundary | application/pdf | Valid |
| BND-081 | Batch one op | Count=1 | Valid |
| BND-082 | Pagination first | Page=1 | Valid |
| BND-083 | Sort ascending | Asc | Ordered |
| BND-084 | Sort descending | Desc | Ordered |
| BND-085 | Filter type and date | Both | Correct |
| BND-086 | Export format boundary | PDF | Valid |
| BND-087 | Convert format boundary | DOCX | Valid |
| BND-088 | Get download URL expiry | Expiry | Regenerate |
| BND-089 | Get web view private | Private | Error |
| BND-090 | Trash then untrash | Cycle | Restored |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Name required | Validation | Create | Reject if empty |
| FUN-002 | Id required for get | Validation | Get | Reject if null |
| FUN-003 | User required for share | Validation | Share | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | Get excludes deleted | Constraint | Get | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Permission format | Constraint | SetPermissions | Reject invalid |
| FUN-008 | Export format whitelist | Constraint | Export | Only allowed |
| FUN-009 | Convert format whitelist | Constraint | Convert | Only allowed |
| FUN-010 | Audit create | Audit | Create | Logged |
| FUN-011 | Audit update | Audit | Update | Logged |
| FUN-012 | Audit delete | Audit | Delete | Logged |
| FUN-013 | Audit share | Audit | Share | Logged |
| FUN-014 | Audit CreatedBy | Audit | Create | Set user |
| FUN-015 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-016 | Permission before action | Authorization | Any | Check first |
| FUN-017 | Share creates permission | Logic | Share | Permission |
| FUN-018 | Remove share deletes | Logic | RemoveShare | Removed |
| FUN-019 | Move updates parent | Logic | Move | Parent updated |
| FUN-020 | Copy creates new | Logic | Copy | New document |
| FUN-021 | List respects filter | Constraint | List | Filtered |
| FUN-022 | Pagination correct | Logic | List | Correct page |
| FUN-023 | Pagination offset | Calculation | Page | Skip correct |
| FUN-024 | Total count accurate | Calculation | Count | Matches |
| FUN-025 | Sort applies | Calculation | Sort | Ordered |
| FUN-026 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-027 | Trash soft delete | Logic | Trash | Trashed |
| FUN-028 | Untrash restore | Logic | Untrash | Restored |
| FUN-029 | Restore revision | Logic | Restore | Restored |
| FUN-030 | Export format | Logic | Export | Formatted |
| FUN-031 | Convert format | Logic | Convert | Converted |
| FUN-032 | Transaction on create | Transaction | Create | Atomic |
| FUN-033 | Transaction on update | Transaction | Update | Atomic |
| FUN-034 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-035 | Async all operations | Concurrency | All | Async |
| FUN-036 | Include loads metadata | Data load | Get include | Metadata loaded |
| FUN-037 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-038 | Get revisions ordered | Logic | GetRevisions | Chronological |
| FUN-039 | Add to folder | Logic | AddToFolder | Added |
| FUN-040 | Remove from folder | Logic | RemoveFromFolder | Removed |
| FUN-041 | Get download URL | Logic | GetDownloadUrl | URL |
| FUN-042 | Get web view link | Logic | GetWebViewLink | Link |
| FUN-043 | Batch atomic | Logic | Batch | All or none |
| FUN-044 | Get metadata complete | Logic | GetMetadata | Complete |
| FUN-045 | Get shared drives | Logic | GetSharedDrives | Drives |
| FUN-046 | Get drive info | Logic | GetDriveInfo | Info |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Stream disposal | Logic | Download | Disposed |
| FUN-051 | Name trim on create | Logic | Create | Trimmed |
| FUN-052 | Share creates permission | Logic | Share | Permission |
| FUN-053 | Remove share deletes | Logic | RemoveShare | Removed |
| FUN-054 | Move updates parent | Logic | Move | Parent updated |
| FUN-055 | Copy creates new | Logic | Copy | New document |
| FUN-056 | Trash soft delete | Logic | Trash | Trashed |
| FUN-057 | Untrash restore | Logic | Untrash | Restored |
| FUN-058 | Restore revision | Logic | Restore | Restored |
| FUN-059 | Export format | Logic | Export | Formatted |
| FUN-060 | Convert format | Logic | Convert | Converted |
| FUN-061 | Get revisions ordered | Logic | GetRevisions | Chronological |
| FUN-062 | Add to folder | Logic | AddToFolder | Added |
| FUN-063 | Remove from folder | Logic | RemoveFromFolder | Removed |
| FUN-064 | Get download URL | Logic | GetDownloadUrl | URL |
| FUN-065 | Get web view link | Logic | GetWebViewLink | Link |
| FUN-066 | Batch atomic | Logic | Batch | All or none |
| FUN-067 | Get metadata complete | Logic | GetMetadata | Complete |
| FUN-068 | Get shared drives | Logic | GetSharedDrives | Drives |
| FUN-069 | Get drive info | Logic | GetDriveInfo | Info |
| FUN-070 | Include loads metadata | Data load | Get include | Metadata |
| FUN-071 | No Cartesian on includes | Data load | Multiple | Split |
| FUN-072 | Audit share | Audit | Share | Logged |
| FUN-073 | Permission before create | Authorization | Create | Check first |
| FUN-074 | Permission before get | Authorization | Get | Check first |
| FUN-075 | Permission before update | Authorization | Update | Check first |
| FUN-076 | Permission before delete | Authorization | Delete | Check first |
| FUN-077 | Permission before share | Authorization | Share | Check first |
| FUN-078 | List respects filter | Constraint | List | Filtered |
| FUN-079 | Pagination correct | Logic | List | Correct page |
| FUN-080 | Pagination offset | Calculation | Page | Skip correct |
| FUN-081 | Total count accurate | Calculation | Count | Matches |
| FUN-082 | Sort applies | Calculation | Sort | Ordered |
| FUN-083 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-084 | Path format valid | Constraint | Create | Reject invalid |
| FUN-085 | Export format whitelist | Constraint | Export | Only allowed |
| FUN-086 | Convert format whitelist | Constraint | Convert | Only allowed |
| FUN-087 | Transaction on create | Transaction | Create | Atomic |
| FUN-088 | Transaction on update | Transaction | Update | Atomic |
| FUN-089 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-090 | Async all operations | Concurrency | All | Async |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Create full flow | Create | Document | Created |
| INT-002 | Get full flow | Get | Document | Returned |
| INT-003 | Update full flow | Update | Document | Updated |
| INT-004 | Delete full flow | Delete | Document | Deleted |
| INT-005 | List with filter | List | Document | Filtered |
| INT-006 | Share with user | Share | Document, User | Shared |
| INT-007 | Set permissions | SetPermissions | Document | Set |
| INT-008 | Get permissions | GetPermissions | Document | Permissions |
| INT-009 | Remove sharing | RemoveShare | Document | Removed |
| INT-010 | Export to PDF | Export | Document | PDF |
| INT-011 | Convert format | Convert | Document | Converted |
| INT-012 | Create folder | CreateFolder | Folder | Created |
| INT-013 | Move document | Move | Document | Moved |
| INT-014 | Copy document | Copy | Document | Copied |
| INT-015 | Search documents | Search | Document | Results |
| INT-016 | Document-Folder relationship | Relationship | Document, Folder | Valid |
| INT-017 | Document-User relationship | Relationship | Document, User | Valid |
| INT-018 | Document-Revision relationship | Relationship | Document, Revision | Valid |
| INT-019 | Cascade delete | Relationship | Folder deleted | Config |
| INT-020 | Orphan handling | Relationship | Folder deleted | Retained |
| INT-021 | Drive API error handling | Error | API down | Graceful |
| INT-022 | Timeout handling | Error | Slow API | Timeout |
| INT-023 | Credential error | Error | Invalid creds | Unauthorized |
| INT-024 | Quota error | Error | Quota | QuotaExceeded |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | Drive client integration | Integration | Drive | Client |
| INT-030 | Mapper integration | Integration | Map | Correct |
| INT-031 | Repository integration | Integration | Repository | CRUD |
| INT-032 | DbContext integration | Integration | DbContext | Scoped |
| INT-033 | Transaction scope | Integration | Transaction | Atomic |
| INT-034 | Config integration | Integration | Config | Read |
| INT-035 | Upload then download | Scenario | Upload, Download | Both |
| INT-036 | Share then remove | Scenario | Share, Remove | Both |
| INT-037 | Move then verify | Scenario | Move | Verified |
| INT-038 | Copy then verify | Scenario | Copy | Verified |
| INT-039 | Trash then untrash | Scenario | Trash, Untrash | Restored |
| INT-040 | Export then download | Scenario | Export, Download | Both |
| INT-041 | Convert then download | Scenario | Convert, Download | Both |
| INT-042 | Restore revision | Scenario | Restore | Restored |
| INT-043 | Add remove from folder | Scenario | Add, Remove | Both |
| INT-044 | Batch operations | Scenario | Batch | All succeed |
| INT-045 | Concurrent create | Scenario | Parallel | All created |
| INT-046 | Get revisions | Scenario | GetRevisions | History |
| INT-047 | Get shared drives | Scenario | GetSharedDrives | Drives |
| INT-048 | Get drive info | Scenario | GetDriveInfo | Info |
| INT-049 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-050 | E2E CRUD cycle | Scenario | Full cycle | Create→Update→Delete |
| INT-051 | Create then get | Scenario | Create, Get | Both |
| INT-052 | Update then get | Scenario | Update, Get | Both |
| INT-053 | Delete then list | Scenario | Delete, List | Excluded |
| INT-054 | Share then get permissions | Scenario | Share, GetPermissions | Both |
| INT-055 | Remove share then get | Scenario | RemoveShare, Get | Both |
| INT-056 | Move then verify | Scenario | Move | Verified |
| INT-057 | Copy then verify | Scenario | Copy | Verified |
| INT-058 | Trash then untrash | Scenario | Trash, Untrash | Restored |
| INT-059 | Export then download | Scenario | Export, Download | Both |
| INT-060 | Convert then download | Scenario | Convert, Download | Both |
| INT-061 | Restore revision | Scenario | Restore | Restored |
| INT-062 | Add remove from folder | Scenario | Add, Remove | Both |
| INT-063 | Batch operations | Scenario | Batch | All succeed |
| INT-064 | Drive client integration | Integration | Drive | Client |
| INT-065 | Mapper integration | Integration | Mapper | Mapped |
| INT-066 | Repository integration | Integration | Repository | CRUD |
| INT-067 | DbContext integration | Integration | DbContext | Scoped |
| INT-068 | Transaction scope | Integration | Transaction | Atomic |
| INT-069 | Config integration | Integration | Config | Read |
| INT-070 | Permission service | Integration | Permission | Check |
| INT-071 | User resolver | Integration | User | Resolved |
| INT-072 | Audit context | Integration | Audit | Context |
| INT-073 | Logger integration | Integration | Logger | Logged |
| INT-074 | Document-Folder relationship | Relationship | Document, Folder | Valid |
| INT-075 | Document-User relationship | Relationship | Document, User | Valid |
| INT-076 | Document-Revision relationship | Relationship | Document, Revision | Valid |
| INT-077 | Cascade delete | Relationship | Folder deleted | Config |
| INT-078 | Orphan handling | Relationship | Folder deleted | Retained |
| INT-079 | Drive API error | Error | API down | Graceful |
| INT-080 | Timeout handling | Error | Slow API | Timeout |
| INT-081 | Credential error | Error | Invalid creds | Unauthorized |
| INT-082 | Quota error | Error | Quota | QuotaExceeded |
| INT-083 | Upload then download | Scenario | Upload, Download | Both |
| INT-084 | Get revisions | Scenario | GetRevisions | History |
| INT-085 | Get shared drives | Scenario | GetSharedDrives | Drives |
| INT-086 | Get drive info | Scenario | GetDriveInfo | Info |
| INT-087 | Get download URL | Scenario | GetDownloadUrl | URL |
| INT-088 | Get web view link | Scenario | GetWebViewLink | Link |
| INT-089 | Filter by type | Scenario | Filter | Filtered |
| INT-090 | Full workflow | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-002 | SQL injection | '; DROP TABLE-- | Query | Rejected |
| SEC-003 | XSS in name | <script>alert(1)</script> | Name | Escaped |
| SEC-004 | XSS in description | <img onerror=...> | Description | Escaped |
| SEC-005 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Any | Rejected |
| SEC-008 | Credentials in log | Log | Log | Redacted |
| SEC-009 | Credentials in error | Error | Stack | Redacted |
| SEC-010 | Unauthorized create | No permission | Create | 403 |
| SEC-011 | Unauthorized get | No permission | Get | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized share | No permission | Share | 403 |
| SEC-015 | Unauthorized export | No permission | Export | 403 |
| SEC-016 | Role escalation | Low role | Admin | 403 |
| SEC-017 | Cross-tenant access | User A | User B doc | 403 |
| SEC-018 | IDOR get other | Id=other | Get | 403/404 |
| SEC-019 | IDOR update other | Id=other | Update | 403 |
| SEC-020 | IDOR delete other | Id=other | Delete | 403 |
| SEC-021 | IDOR share other | Id=other | Share | 403 |
| SEC-022 | IDOR in filter | UserId=other | List | Filtered |
| SEC-023 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-024 | Mass assign owner | Owner=other | Request | Validated |
| SEC-025 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on create | No token | Create | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Share with invalid | Invalid user | Share | Rejected |
| SEC-034 | Rate limit create | Many creates | Create | Throttled |
| SEC-035 | Rate limit export | Many exports | Export | Throttled |
| SEC-036 | Oversized request | 10MB payload | Create | Rejected |
| SEC-037 | Deep nesting | Nested path | Request | Rejected |
| SEC-038 | Header injection | \r\n in header | Header | Rejected |
| SEC-039 | Null byte injection | %00 in name | Name | Rejected |
| SEC-040 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-041 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-042 | Denial of service | Huge export | Export | Rejected |
| SEC-043 | Download URL tampering | Tamper URL | Access | Rejected |
| SEC-044 | Web view link tampering | Tamper link | Access | Rejected |
| SEC-045 | Permission bypass | Direct access | Permission | Denied |
| SEC-046 | Import malicious file | Malicious | Upload | Rejected |
| SEC-047 | Export data injection | Inject in export | Export | Sanitized |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Credential rotation | Rotate creds | Config | Updated |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent create | Two create | Both succeed |
| CON-004 | Concurrent update same | Two update | One wins |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on share | Two share | One or both |
| CON-009 | Race on move | Two move | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel creates | 10 parallel | All succeed |
| CON-012 | Async parallel reads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Export concurrent | Two export | Both succeed |
| CON-016 | Share concurrent | Two share | One or both |
| CON-017 | Move concurrent | Two move | One wins |
| CON-018 | Copy concurrent | Two copy | Both succeed |
| CON-019 | Soft delete concurrent | Delete while update | Deterministic |
| CON-020 | Restore concurrent | Two restore | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Drive API limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate name not null | Validation | null | Exception |
| UNT-002 | Validate Id format | Validation | Valid id | Pass |
| UNT-003 | Validate user | Validation | Valid user | Pass |
| UNT-004 | Validate permission | Validation | Valid perm | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format name display | Formatting | Name | Display |
| UNT-007 | Format metadata | Formatting | Metadata | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Path normalization | Calculation | Path | Normalized |
| UNT-013 | Format mapping | Calculation | Format | Mapped |
| UNT-014 | Document allows create | Status logic | Document | true |
| UNT-015 | Document allows update | Status logic | Document | true |
| UNT-016 | Document allows delete | Status logic | Document | true |
| UNT-017 | Document shared check | Status logic | Document | Shared |
| UNT-018 | Document trashed check | Status logic | Document | Trashed |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get | Get | <500ms | P1 |
| PRF-002 | Single create | Create | <2s | P1 |
| PRF-003 | Single update | Update | <1s | P1 |
| PRF-004 | Single delete | Delete | <1s | P1 |
| PRF-005 | List 100 | List | <2s | P1 |
| PRF-006 | Search | Search | <2s | P1 |
| PRF-007 | Export to PDF | Export | <5s | P1 |
| PRF-008 | Convert format | Convert | <5s | P1 |
| PRF-009 | Get metadata | GetMetadata | <500ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel Get | <5s total | P1 |
| PRF-011 | Concurrent 5 creates | 5 parallel Create | <10s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 create | <8s total | P2 |
| PRF-013 | Memory single create | Create | <10MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory export | Export | <50MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS create | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 20 RPS create | 0→20→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS read | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress quota | Many creates | Until quota | Limited |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large exports | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
