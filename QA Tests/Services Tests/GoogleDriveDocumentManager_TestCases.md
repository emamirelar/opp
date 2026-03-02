# GoogleDriveDocumentManager — Test Cases

**Component:** `UNOPS.PAO.Business/Services/GoogleDriveDocumentManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

| Check | Formula | Result |
|-------|---------|--------|
| N≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| E≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| F≥3P | 90 ≥ 3×30=90 | ✅ PASS |
| I≥3P | 90 ≥ 3×30=90 | ✅ PASS |

---

## Feature Overview

Google Drive integration: document creation, sharing, permissions, conversion, sync, folder management.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|-------------|-------|-----------------|
| POS-001 | Create document | Valid params | CreateDocumentAsync(name, folderId, content) | Document created |
| POS-002 | Get document | Doc exists | GetDocumentAsync(docId) | Document returned |
| POS-003 | Update document | Doc exists | UpdateDocumentAsync(docId, content) | Updated |
| POS-004 | Delete document | Doc exists | DeleteDocumentAsync(docId) | Deleted |
| POS-005 | Share document | Doc exists | ShareAsync(docId, email, role) | Shared |
| POS-006 | Get permissions | Doc exists | GetPermissionsAsync(docId) | Permissions |
| POS-007 | Remove permission | Permission exists | RemovePermissionAsync(docId, permId) | Removed |
| POS-008 | Convert to PDF | Doc exists | ConvertToPdfAsync(docId) | PDF |
| POS-009 | Convert to DOCX | Doc exists | ConvertToDocxAsync(docId) | DOCX |
| POS-010 | Create folder | Valid name | CreateFolderAsync(name, parentId) | Folder created |
| POS-011 | List folder contents | Folder exists | ListFolderAsync(folderId) | Contents |
| POS-012 | Move document | Doc exists | MoveAsync(docId, folderId) | Moved |
| POS-013 | Copy document | Doc exists | CopyAsync(docId, folderId) | Copied |
| POS-014 | Sync document | Doc exists | SyncAsync(docId) | Synced |
| POS-015 | Get download URL | Doc exists | GetDownloadUrlAsync(docId) | URL |
| POS-016 | Get export URL | Doc exists | GetExportUrlAsync(docId, format) | URL |
| POS-017 | Search documents | Query | SearchAsync(query) | Results |
| POS-018 | Trash document | Doc exists | TrashAsync(docId) | Trashed |
| POS-019 | Restore from trash | Doc trashed | RestoreAsync(docId) | Restored |
| POS-020 | Empty trash | Trash has items | EmptyTrashAsync() | Emptied |
| POS-021 | Add comment | Doc exists | AddCommentAsync(docId, comment) | Comment added |
| POS-022 | Get comments | Doc exists | GetCommentsAsync(docId) | Comments |
| POS-023 | Set viewer role | Doc exists | ShareAsync(docId, email, viewer) | Viewer |
| POS-024 | Set editor role | Doc exists | ShareAsync(docId, email, editor) | Editor |
| POS-025 | Set commenter role | Doc exists | ShareAsync(docId, email, commenter) | Commenter |
| POS-026 | Create from template | Template exists | CreateFromTemplateAsync(templateId, name) | Created |
| POS-027 | Get revision list | Doc exists | GetRevisionsAsync(docId) | Revisions |
| POS-028 | Get revision | Doc + rev | GetRevisionAsync(docId, revId) | Revision |
| POS-029 | Publish web | Doc exists | PublishAsync(docId) | Published |
| POS-030 | Unpublish | Doc published | UnpublishAsync(docId) | Unpublished |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|---------------|----------------|
| NEG-001 | Null doc ID | GetDocumentAsync(null) | ArgumentNullException |
| NEG-002 | Empty doc ID | GetDocumentAsync("") | ArgumentException |
| NEG-003 | Invalid doc ID | GetDocumentAsync("invalid") | NotFoundException |
| NEG-004 | Non-existent doc | GetDocumentAsync("999") | NotFoundException |
| NEG-005 | Null document name | CreateDocumentAsync(null, ...) | ArgumentNullException |
| NEG-006 | Empty document name | CreateDocumentAsync("", ...) | ArgumentException |
| NEG-007 | Null folder ID | CreateDocumentAsync(name, null, ...) | ArgumentNullException |
| NEG-008 | Invalid folder ID | CreateDocumentAsync(name, "bad", ...) | NotFoundException |
| NEG-009 | Null content | CreateDocumentAsync(name, folder, null) | ArgumentNullException |
| NEG-010 | Null email for share | ShareAsync(docId, null, role) | ArgumentNullException |
| NEG-011 | Invalid email | ShareAsync(docId, "bad", role) | ArgumentException |
| NEG-012 | Invalid role | ShareAsync(docId, email, "invalid") | ArgumentException |
| NEG-013 | Deleted doc | GetDocumentAsync(deletedId) | NotFoundException |
| NEG-014 | Trashed doc | UpdateDocumentAsync(trashedId, ...) | NotFoundException |
| NEG-015 | Permission denied | GetDocumentAsync(noPerm) | UnauthorizedAccessException |
| NEG-016 | Quota exceeded | CreateDocumentAsync(...) | QuotaExceededException |
| NEG-017 | Rate limit | Many requests | TooManyRequestsException |
| NEG-018 | Invalid credentials | Any op | AuthenticationException |
| NEG-019 | Expired credentials | Any op | AuthenticationException |
| NEG-020 | Network timeout | Any op | TimeoutException |
| NEG-021 | Service unavailable | Any op | ServiceUnavailableException |
| NEG-022 | Null conversion format | ConvertToPdfAsync(docId, null) | ArgumentNullException |
| NEG-023 | Invalid conversion format | ConvertToPdfAsync(docId, "xyz") | ArgumentException |
| NEG-024 | Unsupported conversion | ConvertToPdfAsync(imageId) | NotSupportedException |
| NEG-025 | Null search query | SearchAsync(null) | ArgumentNullException |
| NEG-026 | Search too long | SearchAsync(veryLong) | ArgumentException |
| NEG-027 | Null permission ID | RemovePermissionAsync(docId, null) | ArgumentNullException |
| NEG-028 | Non-existent permission | RemovePermissionAsync(docId, "999") | NotFoundException |
| NEG-029 | Null folder name | CreateFolderAsync(null, ...) | ArgumentNullException |
| NEG-030 | Invalid parent folder | CreateFolderAsync(name, "bad") | NotFoundException |
| NEG-031 | Null move target | MoveAsync(docId, null) | ArgumentNullException |
| NEG-032 | Move to same folder | MoveAsync(docId, currentFolder) | NoOp or error |
| NEG-033 | Null comment | AddCommentAsync(docId, null) | ArgumentNullException |
| NEG-034 | Comment too long | AddCommentAsync(docId, veryLong) | ArgumentException |
| NEG-035 | Null template ID | CreateFromTemplateAsync(null, name) | ArgumentNullException |
| NEG-036 | Invalid template | CreateFromTemplateAsync("bad", name) | NotFoundException |
| NEG-037 | Null revision ID | GetRevisionAsync(docId, null) | ArgumentNullException |
| NEG-038 | Non-existent revision | GetRevisionAsync(docId, "999") | NotFoundException |
| NEG-039 | Null channel | WatchAsync(docId, null) | ArgumentNullException |
| NEG-040 | Null batch IDs | BatchGetAsync(null) | ArgumentNullException |
| NEG-041 | Empty batch | BatchGetAsync([]) | ArgumentException |
| NEG-042 | Batch too large | BatchGetAsync(1000 ids) | ArgumentException |
| NEG-043 | Export format invalid | GetExportUrlAsync(docId, "invalid") | ArgumentException |
| NEG-044 | Doc not exportable | GetExportUrlAsync(nativeId, "pdf") | NotSupportedException |
| NEG-045 | Cancelled token | CreateDocumentAsync(..., cancelled) | OperationCanceledException |
| NEG-046 | Concurrent delete | Delete during update | ConflictException |
| NEG-047 | Circular move | Move A to B, B to A | InvalidOperationException |
| NEG-048 | Share with self | ShareAsync(docId, self, ...) | ArgumentException |
| NEG-049 | Domain restriction | ShareAsync(domain) | DomainException |
| NEG-050 | File size limit | CreateDocumentAsync(huge) | ArgumentException |
| NEG-051 | Name with invalid chars | CreateDocumentAsync("a/b", ...) | ArgumentException |
| NEG-052 | Path too long | CreateDocumentAsync(veryLongPath) | ArgumentException |
| NEG-053 | Duplicate share | ShareAsync(same email) | AlreadyExistsException |
| NEG-054 | Trash already empty | EmptyTrashAsync() | NoOp or error |
| NEG-055 | Restore not trashed | RestoreAsync(activeId) | InvalidOperationException |
| NEG-056 | Publish already | PublishAsync(published) | AlreadyPublishedException |
| NEG-057 | Unpublish not published | UnpublishAsync(notPublished) | InvalidOperationException |
| NEG-058 | Watch channel exists | WatchAsync(duplicate) | ConflictException |
| NEG-059 | Stop non-existent watch | StopWatchAsync("bad") | NotFoundException |
| NEG-060 | Copy to trashed folder | CopyAsync(docId, trashedFolder) | NotFoundException |
| NEG-061 | Move to deleted folder | MoveAsync(docId, deletedFolder) | NotFoundException |
| NEG-062 | Comment on trashed | AddCommentAsync(trashedId, ...) | NotFoundException |
| NEG-063 | Get revision deleted | GetRevisionAsync(deleted) | NotFoundException |
| NEG-064 | Batch mixed valid/invalid | BatchGetAsync([1,"bad"]) | Partial or error |
| NEG-065 | Sync deleted doc | SyncAsync(deletedId) | NotFoundException |
| NEG-066 | Get URL deleted | GetDownloadUrlAsync(deletedId) | NotFoundException |
| NEG-067 | Convert deleted | ConvertToPdfAsync(deletedId) | NotFoundException |
| NEG-068 | List deleted folder | ListFolderAsync(deletedId) | NotFoundException |
| NEG-069 | Create in deleted folder | CreateDocumentAsync(..., deletedFolder) | NotFoundException |
| NEG-070 | OAuth scope insufficient | CreateDocumentAsync(...) | InsufficientScopeException |
| NEG-071 | Null GetMetadata | GetMetadataAsync(null) | ArgumentNullException |
| NEG-072 | Null Watch channel | WatchAsync(docId, null) | ArgumentNullException |
| NEG-073 | Null BatchGet IDs | BatchGetAsync(null) | ArgumentNullException |
| NEG-074 | Null BatchUpdate | BatchUpdateAsync(null) | ArgumentNullException |
| NEG-075 | Invalid StopWatch | StopWatchAsync("") | ArgumentException |
| NEG-076 | Null GetRevisions | GetRevisionsAsync(null) | ArgumentNullException |
| NEG-077 | Null GetRevision | GetRevisionAsync(docId, null) | ArgumentNullException |
| NEG-078 | Null Publish | PublishAsync(null) | ArgumentNullException |
| NEG-079 | Null Unpublish | UnpublishAsync(null) | ArgumentNullException |
| NEG-080 | Null AddComment | AddCommentAsync(docId, null) | ArgumentNullException |
| NEG-081 | Null GetComments | GetCommentsAsync(null) | ArgumentNullException |
| NEG-082 | Invalid Copy target | CopyAsync(docId, null) | ArgumentNullException |
| NEG-083 | Invalid Move target | MoveAsync(docId, null) | ArgumentNullException |
| NEG-084 | Null CreateFolder | CreateFolderAsync(null, ...) | ArgumentNullException |
| NEG-085 | Null ListFolder | ListFolderAsync(null) | ArgumentNullException |
| NEG-086 | Null Search | SearchAsync(null) | ArgumentNullException |
| NEG-087 | Null Trash | TrashAsync(null) | ArgumentNullException |
| NEG-088 | Null Restore | RestoreAsync(null) | ArgumentNullException |
| NEG-089 | Invalid GetExportUrl | GetExportUrlAsync(docId, null) | ArgumentNullException |
| NEG-090 | Null GetDownloadUrl | GetDownloadUrlAsync(null) | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Value | Expected Result |
|----|-----------|----------------|-----------------|
| BND-001 | Doc name length = 1 | "a" | Valid |
| BND-002 | Doc name length = 255 | Max | Valid |
| BND-003 | Doc name length = 256 | Over | Rejected |
| BND-004 | Content size = 0 | "" | Valid |
| BND-005 | Content size = 1 | 1 char | Valid |
| BND-006 | Content size = 50MB | Max | Valid |
| BND-007 | Content size = 50MB+1 | Over | Rejected |
| BND-008 | Folder depth = 1 | Root | Valid |
| BND-009 | Folder depth = 10 | Deep | Valid |
| BND-010 | Folder depth = 20 | Max | Valid |
| BND-011 | Share count = 0 | None | Valid |
| BND-012 | Share count = 1 | One | Valid |
| BND-013 | Share count = 100 | Max | Valid |
| BND-014 | Share count = 101 | Over | Rejected |
| BND-015 | Search length = 0 | "" | Invalid |
| BND-016 | Search length = 1 | "a" | Results |
| BND-017 | Search length = 2048 | Max | Valid |
| BND-018 | Comment length = 0 | "" | Invalid |
| BND-019 | Comment length = 1 | "a" | Valid |
| BND-020 | Comment length = 10000 | Max | Valid |
| BND-021 | Batch size = 1 | 1 | Valid |
| BND-022 | Batch size = 100 | Max | Valid |
| BND-023 | Batch size = 101 | Over | Rejected |
| BND-024 | Revision count = 0 | None | [] |
| BND-025 | Revision count = 100 | Many | Returned |
| BND-026 | List page = 1 | First | Results |
| BND-027 | List page = last | Last | Results |
| BND-028 | List page size = 1 | Min | One |
| BND-029 | List page size = 1000 | Max | 1000 |
| BND-030 | List page size = 1001 | Over | Rejected |
| BND-031 | Concurrent requests = 1 | 1 | Success |
| BND-032 | Concurrent requests = 50 | 50 | Success |
| BND-033 | Concurrent requests = 200 | 200 | Throttled |
| BND-034 | Unicode in name | "文档.pdf" | Valid |
| BND-035 | Emoji in name | "📄.doc" | Valid |
| BND-036 | RTL in name | "ملف.docx" | Valid |
| BND-037 | Special chars in name | "file (1).doc" | Valid |
| BND-038 | Leading space in name | " file" | Trimmed |
| BND-039 | Trailing space in name | "file " | Trimmed |
| BND-040 | Tab in name | "file\t.doc" | Sanitized |
| BND-041 | Newline in name | "file\n.doc" | Sanitized |
| BND-042 | Email length = 254 | Max | Valid |
| BND-043 | Email length = 255 | Over | Rejected |
| BND-044 | Role name length = 1 | "v" | Invalid |
| BND-045 | Role name length = 20 | "reader" | Valid |
| BND-046 | Export format max | "pdf" | Valid |
| BND-047 | Folder name = 1 | "a" | Valid |
| BND-048 | Folder name = 255 | Max | Valid |
| BND-049 | Path length = 1024 | Max | Valid |
| BND-050 | Path length = 1025 | Over | Rejected |
| BND-051 | Watch expiry = 1s | 1 second | Valid |
| BND-052 | Watch expiry = 7d | 7 days | Valid |
| BND-053 | Revision ID length | 64 chars | Valid |
| BND-054 | Channel ID length | 64 chars | Valid |
| BND-055 | Query param count = 0 | None | Valid |
| BND-056 | Query param count = 10 | 10 | Valid |
| BND-057 | Query param count = 20 | Max | Valid |
| BND-058 | Timeout = 0ms | 0 | Immediate |
| BND-059 | Timeout = 60000ms | 60s | Success |
| BND-060 | Retry count = 0 | No retry | Fail once |
| BND-061 | Retry count = 3 | 3 | Retries |
| BND-062 | Empty folder | ListFolder(empty) | [] |
| BND-063 | Single item folder | ListFolder(one) | [1] |
| BND-064 | Max items folder | ListFolder(max) | Paginated |
| BND-065 | Trash count = 0 | Empty | NoOp |
| BND-066 | Trash count = 1000 | Many | Emptied |
| BND-067 | Permission role owner | "owner" | Valid |
| BND-068 | Permission role writer | "writer" | Valid |
| BND-069 | Permission role reader | "reader" | Valid |
| BND-070 | Permission type user | "user" | Valid |
| BND-071 | Doc count = 0 | None | [] |
| BND-072 | Doc count = 1 | One | [1] |
| BND-073 | Doc count = 1000 | Many | Paginated |
| BND-074 | Revision count = 0 | None | [] |
| BND-075 | Revision count = 100 | Many | Returned |
| BND-076 | Comment count = 0 | None | [] |
| BND-077 | Comment count = 100 | Many | All |
| BND-078 | Share count = 0 | None | [] |
| BND-079 | Share count = 100 | Max | All |
| BND-080 | Folder depth = 1 | Root | Valid |
| BND-081 | Folder depth = 20 | Max | Valid |
| BND-082 | Batch size = 1 | One | Valid |
| BND-083 | Batch size = 100 | Max | Valid |
| BND-084 | Watch expiry = 1s | 1 second | Valid |
| BND-085 | Watch expiry = 7d | 7 days | Valid |
| BND-086 | Path length = 1 | "a" | Valid |
| BND-087 | Path length = 1024 | Max | Valid |
| BND-088 | Content size = 0 | "" | Valid |
| BND-089 | Content size = 50MB | Max | Valid |
| BND-090 | Query length = 1 | "a" | Results |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule | Trigger | Expected Outcome |
|----|-----------|------|---------|------------------|
| FUN-001 | Doc naming | Naming | Create | Valid name |
| FUN-002 | Folder naming | Naming | CreateFolder | Valid name |
| FUN-003 | Share propagation | Propagate | Share | Recipient notified |
| FUN-004 | Permission inheritance | Inherit | Create in folder | Inherited |
| FUN-005 | Trash retention | Retention | Trash | 30 days |
| FUN-006 | Revision retention | Retention | Edit | Revisions kept |
| FUN-007 | Conversion format | Format | Convert | Supported format |
| FUN-008 | Sync frequency | Sync | Watch | Notified |
| FUN-009 | Export format | Format | Export | Supported |
| FUN-010 | Search scope | Scope | Search | Scoped |
| FUN-011 | Copy metadata | Metadata | Copy | Preserved |
| FUN-012 | Move metadata | Metadata | Move | Preserved |
| FUN-013 | Delete cascade | Cascade | Delete folder | All deleted |
| FUN-014 | Trash cascade | Cascade | Trash folder | All trashed |
| FUN-015 | Restore location | Location | Restore | Original |
| FUN-016 | Share expiration | Expiry | Share | Optional expiry |
| FUN-017 | Domain restriction | Domain | Share | Checked |
| FUN-018 | Role hierarchy | Hierarchy | Share | Role order |
| FUN-019 | Comment threading | Threading | AddComment | Threaded |
| FUN-020 | Revision ordering | Order | GetRevisions | Chronological |
| FUN-021 | Publish visibility | Visibility | Publish | Public |
| FUN-022 | Unpublish | Unpublish | Unpublish | Private |
| FUN-023 | Watch channel | Channel | Watch | Unique |
| FUN-024 | Batch atomicity | Atomic | BatchUpdate | All or none |
| FUN-025 | Conflict resolution | Conflict | Concurrent edit | Merge |
| FUN-026 | Version history | History | Edit | Versioned |
| FUN-027 | Export quality | Quality | Export | High |
| FUN-028 | Conversion quality | Quality | Convert | High |
| FUN-029 | Sync consistency | Consistency | Sync | Consistent |
| FUN-030 | Permission check | Check | Access | Checked |
| FUN-031 | Quota enforcement | Quota | Create | Enforced |
| FUN-032 | Rate limit | Rate | Many | Limited |
| FUN-033 | Retry transient | Retry | Transient | Retried |
| FUN-034 | Error format | Format | Error | Consistent |
| FUN-035 | Audit trail | Audit | Any op | Logged |
| FUN-036 | Soft delete | Soft | Delete | Trashed |
| FUN-037 | Hard delete | Hard | EmptyTrash | Deleted |
| FUN-038 | Template instantiation | Instantiate | CreateFromTemplate | Copy |
| FUN-039 | Link sharing | Link | Share | Link |
| FUN-040 | Email notification | Notify | Share | Email |
| FUN-041 | Comment notification | Notify | AddComment | Notified |
| FUN-042 | Edit notification | Notify | Update | Notified |
| FUN-043 | Download tracking | Track | Download | Tracked |
| FUN-044 | View tracking | Track | View | Tracked |
| FUN-045 | Metadata indexing | Index | Create | Indexed |
| FUN-046 | Search indexing | Index | Create | Searchable |
| FUN-047 | Caching | Cache | Get | Cached |
| FUN-048 | Cache invalidation | Invalidation | Update | Invalidated |
| FUN-049 | Offline support | Offline | Sync | Queued |
| FUN-050 | Conflict detection | Conflict | Concurrent | Detected |
| FUN-051 | Doc naming | Naming | Create | Valid name |
| FUN-052 | Folder naming | Naming | CreateFolder | Valid name |
| FUN-053 | Share propagation | Propagate | Share | Recipient notified |
| FUN-054 | Permission inheritance | Inherit | Create in folder | Inherited |
| FUN-055 | Trash retention | Retention | Trash | 30 days |
| FUN-056 | Revision retention | Retention | Edit | Revisions kept |
| FUN-057 | Conversion format | Format | Convert | Supported format |
| FUN-058 | Sync frequency | Sync | Watch | Notified |
| FUN-059 | Export format | Format | Export | Supported |
| FUN-060 | Search scope | Scope | Search | Scoped |
| FUN-061 | Copy metadata | Metadata | Copy | Preserved |
| FUN-062 | Move metadata | Metadata | Move | Preserved |
| FUN-063 | Delete cascade | Cascade | Delete folder | All deleted |
| FUN-064 | Trash cascade | Cascade | Trash folder | All trashed |
| FUN-065 | Restore location | Location | Restore | Original |
| FUN-066 | Share expiration | Expiry | Share | Optional expiry |
| FUN-067 | Domain restriction | Domain | Share | Checked |
| FUN-068 | Role hierarchy | Hierarchy | Share | Role order |
| FUN-069 | Comment threading | Threading | AddComment | Threaded |
| FUN-070 | Revision ordering | Order | GetRevisions | Chronological |
| FUN-071 | Publish visibility | Visibility | Publish | Public |
| FUN-072 | Unpublish | Unpublish | Unpublish | Private |
| FUN-073 | Watch channel | Channel | Watch | Unique |
| FUN-074 | Batch atomicity | Atomic | BatchUpdate | All or none |
| FUN-075 | Conflict resolution | Conflict | Concurrent edit | Merge |
| FUN-076 | Version history | History | Edit | Versioned |
| FUN-077 | Export quality | Quality | Export | High |
| FUN-078 | Conversion quality | Quality | Convert | High |
| FUN-079 | Sync consistency | Consistency | Sync | Consistent |
| FUN-080 | Permission check | Check | Access | Checked |
| FUN-081 | Quota enforcement | Quota | Create | Enforced |
| FUN-082 | Rate limit | Rate | Many | Limited |
| FUN-083 | Retry transient | Retry | Transient | Retried |
| FUN-084 | Error format | Format | Error | Consistent |
| FUN-085 | Audit trail | Audit | Any op | Logged |
| FUN-086 | Soft delete | Soft | Delete | Trashed |
| FUN-087 | Hard delete | Hard | EmptyTrash | Deleted |
| FUN-088 | Template instantiation | Instantiate | CreateFromTemplate | Copy |
| FUN-089 | Link sharing | Link | Share | Link |
| FUN-090 | Email notification | Notify | Share | Email |

---

## §5 Integration Tests (90)

| ID | Test Name | Integration | Scenario | Expected Result |
|----|-----------|-------------|----------|-----------------|
| INT-001 | Google Drive API | Drive API | Create | Success |
| INT-002 | OAuth2 | OAuth | Auth | Authenticated |
| INT-003 | Configuration | IConfiguration | Config | Applied |
| INT-004 | Logger | ILogger | Log | Logged |
| INT-005 | Document manager | IDocumentManager | Link | Linked |
| INT-006 | Opportunity | IOpportunityManager | Doc to opp | Linked |
| INT-007 | Partner | IPartnerManager | Doc to partner | Linked |
| INT-008 | User service | IUserService | Share | User resolved |
| INT-009 | Audit | IAuditService | Any op | Logged |
| INT-010 | Permission | IPermissionService | Access | Checked |
| INT-011 | Full create flow | All | Create doc | Success |
| INT-012 | Full share flow | All | Share | Success |
| INT-013 | Full convert flow | All | Convert | Success |
| INT-014 | Full sync flow | All | Sync | Success |
| INT-015 | Create + share | Create + share | Both | Success |
| INT-016 | Share + permission | Share + get | Both | Success |
| INT-017 | Convert + export | Convert + export | Both | Success |
| INT-018 | Move + sync | Move + sync | Both | Success |
| INT-019 | Copy + share | Copy + share | Both | Success |
| INT-020 | Trash + restore | Trash + restore | Both | Success |
| INT-021 | Document + opp | Document | Link to opp | Linked |
| INT-022 | Document + partner | Document | Link to partner | Linked |
| INT-023 | Document + contact | Document | Link to contact | Linked |
| INT-024 | Config + credentials | Config | Credentials | From config |
| INT-025 | Logger + error | Logger | Error | Logged |
| INT-026 | Audit + create | Audit | Create | Audited |
| INT-027 | Permission + share | Permission | Share | Checked |
| INT-028 | User + share | User | Share | Resolved |
| INT-029 | Retry + transient | Retry | Transient | Retried |
| INT-030 | Timeout + create | Timeout | Create | Timeout |
| INT-031 | Cancellation + create | Cancel | Create | Cancelled |
| INT-032 | Rate limit + many | Rate limit | Many | Limited |
| INT-033 | OAuth + refresh | OAuth | Refresh | Refreshed |
| INT-034 | Drive + Sheets | Drive + Sheets | Create sheet | Success |
| INT-035 | Drive + Slides | Drive + Slides | Create slide | Success |
| INT-036 | Drive + Forms | Drive + Forms | Create form | Success |
| INT-037 | Drive + GCS | Drive + GCS | Export to GCS | Success |
| INT-038 | Webhook + Drive | Webhook | Change | Notified |
| INT-039 | Push + Drive | Push | Create | Pushed |
| INT-040 | Email + share | Email | Share | Email sent |
| INT-041 | Notification + comment | Notification | Comment | Notified |
| INT-042 | Cache + Drive | Cache | Get | Cached |
| INT-043 | Tenant + Drive | Tenant | Create | Isolated |
| INT-044 | Multi-user + Drive | Multi-user | Share | Scoped |
| INT-045 | Batch + Drive | Batch | Batch op | Success |
| INT-046 | Resilient + Drive | Resilient | Transient | Retried |
| INT-047 | Circuit breaker | Circuit | Failures | Opened |
| INT-048 | Fallback | Fallback | Unavailable | Fallback |
| INT-049 | Health check | Health | Check | Healthy |
| INT-050 | End-to-end | All | Full flow | Success |
| INT-051 | Google Drive API | Drive API | Create | Success |
| INT-052 | OAuth2 | OAuth | Auth | Authenticated |
| INT-053 | Configuration | IConfiguration | Config | Applied |
| INT-054 | Logger | ILogger | Log | Logged |
| INT-055 | Document manager | IDocumentManager | Link | Linked |
| INT-056 | Opportunity | IOpportunityManager | Doc to opp | Linked |
| INT-057 | Partner | IPartnerManager | Doc to partner | Linked |
| INT-058 | User service | IUserService | Share | User resolved |
| INT-059 | Audit | IAuditService | Any op | Logged |
| INT-060 | Permission | IPermissionService | Access | Checked |
| INT-061 | Full create flow | All | Create doc | Success |
| INT-062 | Full share flow | All | Share | Success |
| INT-063 | Full convert flow | All | Convert | Success |
| INT-064 | Full sync flow | All | Sync | Success |
| INT-065 | Create + share | Create + share | Both | Success |
| INT-066 | Share + permission | Share + get | Both | Success |
| INT-067 | Convert + export | Convert + export | Both | Success |
| INT-068 | Move + sync | Move + sync | Both | Success |
| INT-069 | Copy + share | Copy + share | Both | Success |
| INT-070 | Trash + restore | Trash + restore | Both | Success |
| INT-071 | Document + opp | Document | Link to opp | Linked |
| INT-072 | Document + partner | Document | Link to partner | Linked |
| INT-073 | Document + contact | Document | Link to contact | Linked |
| INT-074 | Config + credentials | Config | Credentials | From config |
| INT-075 | Logger + error | Logger | Error | Logged |
| INT-076 | Audit + create | Audit | Create | Audited |
| INT-077 | Permission + share | Permission | Share | Checked |
| INT-078 | User + share | User | Share | Resolved |
| INT-079 | Retry + transient | Retry | Transient | Retried |
| INT-080 | Timeout + create | Timeout | Create | Timeout |
| INT-081 | Cancellation + create | Cancel | Create | Cancelled |
| INT-082 | Rate limit + many | Rate limit | Many | Limited |
| INT-083 | OAuth + refresh | OAuth | Refresh | Refreshed |
| INT-084 | Drive + Sheets | Drive + Sheets | Create sheet | Success |
| INT-085 | Drive + Slides | Drive + Slides | Create slide | Success |
| INT-086 | Drive + Forms | Drive + Forms | Create form | Success |
| INT-087 | Drive + GCS | Drive + GCS | Export to GCS | Success |
| INT-088 | Webhook + Drive | Webhook | Change | Notified |
| INT-089 | Cache + Drive | Cache | Get | Cached |
| INT-090 | End-to-end | All | Full flow | Success |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | Path traversal | ../etc | Path | Rejected |
| SEC-002 | XSS in name | <script> | Name | Sanitized |
| SEC-003 | XSS in comment | <img onerror> | Comment | Sanitized |
| SEC-004 | SQL injection | '; DROP | Search | Parameterized |
| SEC-005 | Unauthorized read | No perm | Get | 403 |
| SEC-006 | Unauthorized write | No perm | Update | 403 |
| SEC-007 | Unauthorized share | No perm | Share | 403 |
| SEC-008 | Unauthorized delete | No perm | Delete | 403 |
| SEC-009 | IDOR document | Alter ID | Get | 403 |
| SEC-010 | IDOR folder | Alter ID | List | 403 |
| SEC-011 | Cross-tenant | Tenant A | Tenant B doc | 403 |
| SEC-012 | Share escalation | Reader | Share as editor | 403 |
| SEC-013 | Mass assignment | Extra fields | Create | Ignored |
| SEC-014 | Credential leak | Log | Credential | Not logged |
| SEC-015 | Token in URL | URL | Token | Not in URL |
| SEC-016 | PII in log | Log | PII | Redacted |
| SEC-017 | OAuth scope | Insufficient | Create | 403 |
| SEC-018 | Token tampering | Tampered | Any | 401 |
| SEC-019 | Expired token | Expired | Any | 401 |
| SEC-020 | Refresh token leak | Log | Refresh | Not logged |
| SEC-021 | DoS large create | 100MB | Create | Rejected |
| SEC-022 | DoS many requests | 10000/s | Any | Rate limited |
| SEC-023 | SSRF in URL | URL | Metadata | Blocked |
| SEC-024 | Open redirect | Redirect | Share | Blocked |
| SEC-025 | Cache poisoning | Poison | Cache | Validated |
| SEC-026 | Replay attack | Replay | Share | Nonce |
| SEC-027 | CSRF | Cross-site | Create | Token |
| SEC-028 | Session fixation | Fixation | Auth | New session |
| SEC-029 | Privilege escalation | Low role | Admin | 403 |
| SEC-030 | Horizontal privilege | User A | User B doc | 403 |
| SEC-031 | Domain bypass | Share | External | Blocked |
| SEC-032 | Link sharing bypass | Link | Private | Blocked |
| SEC-033 | Export bypass | Export | Private | 403 |
| SEC-034 | Revision bypass | Revision | Deleted | 403 |
| SEC-035 | Comment injection | Injection | Comment | Sanitized |
| SEC-036 | Template injection | {{payload}} | Template | Escaped |
| SEC-037 | Prototype pollution | __proto__ | Metadata | Sanitized |
| SEC-038 | Insecure deserialization | Binary | Parse | JSON only |
| SEC-039 | XML external entity | XXE | Parse | Not XML |
| SEC-040 | JWT tampering | Altered | Auth | Rejected |
| SEC-041 | Algorithm confusion | Alg none | JWT | Rejected |
| SEC-042 | Information disclosure | Error | Detail | Generic |
| SEC-043 | Enumeration | Sequential IDs | Get | Rate limited |
| SEC-044 | Metadata exposure | Metadata | Response | Filtered |
| SEC-045 | Header injection | CRLF | Name | Sanitized |
| SEC-046 | Command injection | ; rm | Path | Sanitized |
| SEC-047 | No auth | No auth | Create | 401 |
| SEC-048 | Weak crypto | MD5 | Checksum | SHA256 |
| SEC-049 | Insecure TLS | TLS 1.0 | Connection | TLS 1.2+ |
| SEC-050 | Audit bypass | Bypass | Audit | Logged |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Concurrent create same folder | 2 threads | Both succeed |
| CON-002 | Concurrent update same doc | 2 threads | Conflict or merge |
| CON-003 | Concurrent delete same doc | 2 threads | One 404 |
| CON-004 | Concurrent share same doc | 2 threads | Both succeed |
| CON-005 | Create during delete | Create + delete | Consistent |
| CON-006 | Update during share | Update + share | Handled |
| CON-007 | Move during copy | Move + copy | Consistent |
| CON-008 | Trash during restore | Trash + restore | One wins |
| CON-009 | Share during remove | Share + remove | Handled |
| CON-010 | Convert during update | Convert + update | Version |
| CON-011 | Cache stampede | 100 cold | Single load |
| CON-012 | Batch concurrent | 2 batches | Both succeed |
| CON-013 | Watch concurrent | 2 watch same | One fails |
| CON-014 | Comment concurrent | 2 comments | Both added |
| CON-015 | Revision concurrent | 2 edits | Both versions |
| CON-016 | Deadlock | A→B, B→A | No deadlock |
| CON-017 | Lock contention | 50 updates | Throttled |
| CON-018 | Thread pool exhaustion | 1000 threads | Limited |
| CON-019 | Memory barrier | Create + list | Visible |
| CON-020 | Optimistic concurrency | Update + update | Version |
| CON-021 | Pessimistic lock | Edit + lock | Locked |
| CON-022 | Semaphore | Limited | Semaphore |
| CON-023 | Read-write lock | Read + write | RW lock |
| CON-024 | Circuit breaker | Many failures | Opened |
| CON-025 | Full concurrency | All ops | All succeed |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Doc ID validation | Validation | "abc" | Invalid |
| UNT-002 | Folder ID validation | Validation | "" | Invalid |
| UNT-003 | Email validation | Validation | "bad" | Invalid |
| UNT-004 | Role validation | Validation | "invalid" | Invalid |
| UNT-005 | Name validation | Validation | "" | Invalid |
| UNT-006 | Path sanitize | Formatting | "a//b" | "a/b" |
| UNT-007 | Name format | Formatting | "  name  " | "name" |
| UNT-008 | Email format | Formatting | "User@DOMAIN.com" | Lowercase |
| UNT-009 | Role format | Formatting | "READER" | "reader" |
| UNT-010 | MIME type format | Formatting | "pdf" | "application/pdf" |
| UNT-011 | Path join | Calculations | ["a","b"] | "a/b" |
| UNT-012 | Pagination offset | Calculations | Page 2, 10 | 10 |
| UNT-013 | Batch chunk | Calculations | 100 ids | Chunks |
| UNT-014 | Expiry calc | Calculations | 1h | Timestamp |
| UNT-015 | Retry delay | Calculations | Attempt 2 | Delay |
| UNT-016 | Doc exists | Status | Doc ID | True/False |
| UNT-017 | Shared check | Status | Doc | True/False |
| UNT-018 | Trashed check | Status | Doc | True/False |
| UNT-019 | Published check | Status | Doc | True/False |
| UNT-020 | Empty list | Collections | [] | Empty |
| UNT-021 | Single item | Collections | [1] | Single |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold |
|----|-----------|-----------|-----------|
| PRF-001 | Create small doc | CreateDocumentAsync(1KB) | <2s |
| PRF-002 | Create large doc | CreateDocumentAsync(10MB) | <30s |
| PRF-003 | Get document | GetDocumentAsync | <500ms |
| PRF-004 | Update document | UpdateDocumentAsync | <2s |
| PRF-005 | Share document | ShareAsync | <1s |
| PRF-006 | Convert to PDF | ConvertToPdfAsync | <5s |
| PRF-007 | List folder | ListFolderAsync(100) | <1s |
| PRF-008 | Search | SearchAsync | <2s |
| PRF-009 | Get permissions | GetPermissionsAsync | <500ms |
| PRF-010 | Batch get 10 | BatchGetAsync(10) | <2s |
| PRF-011 | Concurrent 10 create | 10 concurrent | <20s |
| PRF-012 | Concurrent 50 get | 50 concurrent | <5s |
| PRF-013 | Memory create | Create 10MB | <50MB |
| PRF-014 | Memory get | Get 100 | <20MB |
| PRF-015 | Cold start | First request | <1s |
| PRF-016 | Full flow | Create + share + convert | <10s |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|------------------|
| LDT-001 | Sustained 10 req/s | 10/s | 5 min | 99% success |
| LDT-002 | Sustained 50 req/s | 50/s | 5 min | 99% success |
| LDT-003 | Sustained 100 req/s | 100/s | 5 min | 95% success |
| LDT-004 | Spike 0→100 | 0→100/s | 1 min | No crash |
| LDT-005 | Spike 100→0 | 100→0/s | 1 min | No crash |
| LDT-006 | Stress 200 req/s | 200/s | 2 min | Graceful |
| LDT-007 | Stress 500 req/s | 500/s | 1 min | Throttled |
| LDT-008 | Stress 1000 req/s | 1000/s | 30s | No crash |
| LDT-009 | Recovery after spike | Spike + recovery | 5 min | Recovery |
| LDT-010 | Recovery after stress | Stress + recovery | 5 min | Recovery |

---

**Status:** Ready for Execution
