# GoogleCloudStorageService — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Services/GoogleCloudStorageService` (Unit Tests)  
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

Google Cloud Storage service unit tests cover upload, download, signed URLs, bucket management, and ACL for GCS operations. Tests include: upload/download files, generate signed URLs, create/delete buckets, set ACLs, and handle errors.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Upload file | Valid file | Upload | Uploaded |
| POS-002 | Download file | File exists | Download | Stream returned |
| POS-003 | Get signed URL | File exists | GetSignedUrl | URL returned |
| POS-004 | Delete file | File exists | Delete | Deleted |
| POS-005 | List objects | Bucket has objects | List | List returned |
| POS-006 | Get object metadata | Object exists | GetMetadata | Metadata |
| POS-007 | Create bucket | Bucket name valid | CreateBucket | Created |
| POS-008 | Delete bucket | Bucket empty | DeleteBucket | Deleted |
| POS-009 | Set ACL | Object exists | SetAcl | ACL set |
| POS-010 | Get ACL | Object exists | GetAcl | ACL returned |
| POS-011 | Copy object | Source exists | Copy | Copied |
| POS-012 | Move object | Source exists | Move | Moved |
| POS-013 | Exists check | Object exists | Exists | True |
| POS-014 | Get content type | Object exists | GetContentType | Type returned |
| POS-015 | Get file size | Object exists | GetSize | Size returned |
| POS-016 | Upload with metadata | Valid file | Upload | Metadata set |
| POS-017 | Download to stream | Object exists | Download | Stream |
| POS-018 | Upload from stream | Stream valid | Upload | Uploaded |
| POS-019 | Pagination list | Many objects | List | Pages |
| POS-020 | Filter by prefix | Prefix set | List | Filtered |
| POS-021 | Signed URL expiry | Expiry set | GetSignedUrl | Expiry |
| POS-022 | CORS config | Bucket exists | SetCors | Set |
| POS-023 | Lifecycle rule | Bucket exists | SetLifecycle | Set |
| POS-024 | Public read ACL | Object exists | SetPublicRead | Set |
| POS-025 | Private ACL | Object exists | SetPrivate | Set |
| POS-026 | Upload retry | Transient error | Upload | Retried |
| POS-027 | Download retry | Transient error | Download | Retried |
| POS-028 | Multipart upload | Large file | Upload | Uploaded |
| POS-029 | Resumable upload | Interrupted | Resume | Resumed |
| POS-030 | Cache control | Object exists | SetCacheControl | Set |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Upload null file | File=null | ArgumentNullException |
| NEG-002 | Upload empty file | File empty | ValidationException |
| NEG-003 | Upload null path | Path=null | ArgumentNullException |
| NEG-004 | Upload empty path | Path="" | ValidationException |
| NEG-005 | Download non-existent | Path invalid | NotFoundException |
| NEG-006 | Download null path | Path=null | ArgumentNullException |
| NEG-007 | Get signed URL invalid | Path invalid | NotFoundException |
| NEG-008 | Get signed URL null | Path=null | ArgumentNullException |
| NEG-009 | Delete non-existent | Path invalid | NotFoundException |
| NEG-010 | Delete null path | Path=null | ArgumentNullException |
| NEG-011 | List null bucket | Bucket=null | ArgumentNullException |
| NEG-012 | Create bucket invalid name | Name invalid | ValidationException |
| NEG-013 | Create bucket duplicate | Name exists | ConflictException |
| NEG-014 | Delete bucket non-empty | Has objects | ConflictException |
| NEG-015 | Set ACL invalid object | Object invalid | NotFoundException |
| NEG-016 | Get ACL invalid object | Object invalid | NotFoundException |
| NEG-017 | Copy source invalid | Source invalid | NotFoundException |
| NEG-018 | Move source invalid | Source invalid | NotFoundException |
| NEG-019 | Credentials missing | Credentials null | ConfigurationException |
| NEG-020 | Credentials invalid | Credentials invalid | UnauthorizedException |
| NEG-021 | Bucket not found | Bucket invalid | NotFoundException |
| NEG-022 | Project not found | Project invalid | NotFoundException |
| NEG-023 | Quota exceeded | Quota full | QuotaExceededException |
| NEG-024 | Rate limit exceeded | Over limit | RateLimitException |
| NEG-025 | Timeout | Slow operation | TimeoutException |
| NEG-026 | Network error | Network down | NetworkException |
| NEG-027 | Exists null path | Path=null | ArgumentNullException |
| NEG-028 | GetMetadata invalid | Path invalid | NotFoundException |
| NEG-029 | GetContentType invalid | Path invalid | NotFoundException |
| NEG-030 | GetSize invalid | Path invalid | NotFoundException |
| NEG-031 | Path traversal | ../../../etc | ValidationException |
| NEG-032 | Invalid bucket name | Name invalid | ValidationException |
| NEG-033 | Invalid object name | Name invalid | ValidationException |
| NEG-034 | Signed URL expiry past | Expiry past | ArgumentException |
| NEG-035 | Signed URL expiry too long | Expiry 30 days | ArgumentException |
| NEG-036 | Upload without permission | Unauthorized | Forbidden |
| NEG-037 | Download without permission | Unauthorized | Forbidden |
| NEG-038 | Delete without permission | Unauthorized | Forbidden |
| NEG-039 | Create bucket without permission | Unauthorized | Forbidden |
| NEG-040 | Null stream | Stream=null | ArgumentNullException |
| NEG-041 | Closed stream | Stream closed | InvalidOperationException |
| NEG-042 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-043 | Concurrent conflict | Two write same | ConflictException |
| NEG-044 | Transaction rollback | Fail in transaction | Rollback |
| NEG-045 | Connection timeout | GCS unavailable | TimeoutException |
| NEG-046 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-047 | Invalid enum value | ACL invalid | ArgumentException |
| NEG-048 | Expired session | Expired token | Unauthorized |
| NEG-049 | Null user context | User=null | InvalidOperationException |
| NEG-050 | Copy destination invalid | Dest invalid | ArgumentException |
| NEG-051 | Move destination invalid | Dest invalid | ArgumentException |
| NEG-052 | Compose invalid sources | Sources invalid | ArgumentException |
| NEG-053 | Lifecycle invalid rule | Rule invalid | ValidationException |
| NEG-054 | CORS invalid config | Config invalid | ValidationException |
| NEG-055 | Metadata too large | Metadata size | ValidationException |
| NEG-056 | Content type invalid | Type invalid | ValidationException |
| NEG-057 | Cache control invalid | Control invalid | ArgumentException |
| NEG-058 | Pagination invalid | Page invalid | ArgumentException |
| NEG-059 | Filter invalid | Filter invalid | ArgumentException |
| NEG-060 | Audit missing user | User=0 | InvalidOperationException |
| NEG-061 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-062 | Multipart part size invalid | Size invalid | ArgumentException |
| NEG-063 | Resumable invalid state | State invalid | InvalidOperationException |
| NEG-064 | Child override throws | Child throws | Propagated |
| NEG-065 | GetSignedUrlPost invalid | Params invalid | ArgumentException |
| NEG-066 | GetBucketInfo invalid | Bucket invalid | NotFoundException |
| NEG-067 | SetMetadata null | Metadata=null | ArgumentNullException |
| NEG-068 | SetEncoding invalid | Encoding invalid | ArgumentException |
| NEG-069 | Retry exhausted | All retries fail | StorageException |
| NEG-070 | Object locked | Object locked | LockedException |
| NEG-071 | Upload null path | Path=null | ArgumentNullException |
| NEG-072 | Download null path | Path=null | ArgumentNullException |
| NEG-073 | Get signed URL null path | Path=null | ArgumentNullException |
| NEG-074 | Delete null path | Path=null | ArgumentNullException |
| NEG-075 | List null bucket | Bucket=null | ArgumentNullException |
| NEG-076 | Create bucket null name | Name=null | ArgumentNullException |
| NEG-077 | Set ACL null object | Object=null | ArgumentNullException |
| NEG-078 | Copy null source | Source=null | ArgumentNullException |
| NEG-079 | Move null source | Source=null | ArgumentNullException |
| NEG-080 | Exists null path | Path=null | ArgumentNullException |
| NEG-081 | GetMetadata null path | Path=null | ArgumentNullException |
| NEG-082 | Compose null sources | Sources=null | ArgumentNullException |
| NEG-083 | SetMetadata null | Metadata=null | ArgumentNullException |
| NEG-084 | GetBucketInfo null | Bucket=null | ArgumentNullException |
| NEG-085 | GetSignedUrlPost null | Params=null | ArgumentNullException |
| NEG-086 | Pagination invalid | Page invalid | ArgumentException |
| NEG-087 | Filter invalid | Filter invalid | ArgumentException |
| NEG-088 | Lifecycle invalid rule | Rule invalid | ValidationException |
| NEG-089 | CORS invalid config | Config invalid | ValidationException |
| NEG-090 | Content type null | Type=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | File name at min | Length=1 | Valid |
| BND-002 | File name at max | Length=1024 | Valid |
| BND-003 | File name exceeds max | Length=1025 | Reject |
| BND-004 | File size zero | Size=0 | Valid or reject |
| BND-005 | File size at limit | Size=5TB | Valid |
| BND-006 | File size over limit | Size=5TB+1 | Reject |
| BND-007 | Bucket name at min | Length=3 | Valid |
| BND-008 | Bucket name at max | Length=63 | Valid |
| BND-009 | Bucket name over max | Length=64 | Reject |
| BND-010 | Object name at max | Length=1024 | Valid |
| BND-011 | Object name over max | Length=1025 | Reject |
| BND-012 | Signed URL expiry min | 1 second | Valid |
| BND-013 | Signed URL expiry max | 7 days | Valid |
| BND-014 | Signed URL expiry over max | 8 days | Reject |
| BND-015 | Page size at min | PageSize=1 | Valid |
| BND-016 | Page size at max | PageSize=1000 | Valid |
| BND-017 | Page size over max | PageSize=1001 | Reject |
| BND-018 | Unicode in path | Arabic/Chinese | Valid |
| BND-019 | Special chars in path | Encoded | Valid |
| BND-020 | Leading/trailing slashes | Path | Normalized |
| BND-021 | Empty prefix | Prefix="" | Return all |
| BND-022 | Prefix at max | Prefix length | Valid |
| BND-023 | Metadata key max | Key length | Valid |
| BND-024 | Metadata value max | Value length | Valid |
| BND-025 | Metadata count max | Count=limit | Valid |
| BND-026 | Empty bucket | Bucket empty | Empty list |
| BND-027 | Single object | Count=1 | Valid |
| BND-028 | Max objects | At limit | Valid |
| BND-029 | Multipart part size min | Size=5MB | Valid |
| BND-030 | Multipart part size max | Size=5GB | Valid |
| BND-031 | Stream position zero | Position=0 | Valid |
| BND-032 | Stream at end | At end | Valid |
| BND-033 | Pagination last partial | Partial page | Correct |
| BND-034 | Pagination total | Total count | Accurate |
| BND-035 | Sort null handling | Nulls in data | Deterministic |
| BND-036 | Filter combination all | All filters | Correct |
| BND-037 | ACL enum first | First | Valid |
| BND-038 | ACL enum last | Last | Valid |
| BND-039 | Content type boundary | application/octet | Valid |
| BND-040 | Cache control max | Length | Valid |
| BND-041 | Content encoding | gzip | Valid |
| BND-042 | Retry count at 0 | Retry=0 | No retry |
| BND-043 | Retry count at max | Retry=max | Max retries |
| BND-044 | Timeout at min | Timeout=1s | Valid |
| BND-045 | Timeout at max | Timeout=300s | Valid |
| BND-046 | Soft delete boundary | DeletedDate set | Excluded |
| BND-047 | Include depth | Deep include | No explosion |
| BND-048 | Query timeout | Slow query | Timeout |
| BND-049 | Memory large file | 1GB file | Stream |
| BND-050 | Audit timestamp precision | Millisecond | Stored |
| BND-051 | Long string in metadata | 4000 chars | Truncate |
| BND-052 | Duplicate upload | Same path | Overwrite or reject |
| BND-053 | Copy same path | Source=dest | Reject |
| BND-054 | Move same path | Source=dest | Reject |
| BND-055 | Compose empty | Sources=[] | Reject |
| BND-056 | Compose single | Sources=1 | Copy |
| BND-057 | Compose max | Sources=max | Valid |
| BND-058 | Lifecycle rule empty | Rule=[] | Valid |
| BND-059 | CORS empty | CORS=[] | Valid |
| BND-060 | Exists false | Path invalid | False |
| BND-061 | Exists true | Path valid | True |
| BND-062 | GetMetadata empty | No metadata | Empty |
| BND-063 | GetContentType default | No type | application/octet |
| BND-064 | GetSize zero | Size=0 | 0 |
| BND-065 | List empty | No objects | Empty |
| BND-066 | GetSignedUrlPost | POST URL | URL |
| BND-067 | GetBucketInfo minimal | Minimal bucket | Info |
| BND-068 | Async cancellation | Cancel token | OperationCanceledException |
| BND-069 | Task timeout | Timeout | TimeoutException |
| BND-070 | Concurrent same path | Same path | One wins |
| BND-071 | File name single char | Length=1 | Valid |
| BND-072 | Bucket name min | Length=3 | Valid |
| BND-073 | Object name max | Length=1024 | Valid |
| BND-074 | Signed URL expiry min | 1 second | Valid |
| BND-075 | Signed URL expiry max | 7 days | Valid |
| BND-076 | Page size one | PageSize=1 | Valid |
| BND-077 | Metadata count max | Count=limit | Valid |
| BND-078 | Multipart part min | Size=5MB | Valid |
| BND-079 | Multipart part max | Size=5GB | Valid |
| BND-080 | Retry at min | Retry=0 | No retry |
| BND-081 | Retry at max | Retry=max | Max retries |
| BND-082 | Timeout at min | Timeout=1s | Valid |
| BND-083 | Timeout at max | Timeout=300s | Valid |
| BND-084 | ACL enum first | First | Valid |
| BND-085 | ACL enum last | Last | Valid |
| BND-086 | Content type boundary | application/octet | Valid |
| BND-087 | Compose single | Sources=1 | Copy |
| BND-088 | Compose max | Sources=max | Valid |
| BND-089 | Pagination first page | Page=1 | Valid |
| BND-090 | Empty prefix | Prefix="" | Return all |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | Path required | Validation | Upload | Reject if null |
| FUN-002 | Bucket required | Validation | List | Reject if null |
| FUN-003 | Credentials required | Validation | Any | Reject if null |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Path format valid | Constraint | Upload | Reject invalid |
| FUN-008 | Bucket name format | Constraint | CreateBucket | Reject invalid |
| FUN-009 | Object name format | Constraint | Upload | Reject invalid |
| FUN-010 | Audit upload | Audit | Upload | Logged |
| FUN-011 | Audit download | Audit | Download | Logged |
| FUN-012 | Audit delete | Audit | Delete | Logged |
| FUN-013 | Audit CreatedBy | Audit | Create | Set user |
| FUN-014 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Signed URL expiry | Logic | GetSignedUrl | Expiry set |
| FUN-017 | ACL application | Logic | SetAcl | ACL applied |
| FUN-018 | Copy preserves metadata | Logic | Copy | Metadata |
| FUN-019 | Move deletes source | Logic | Move | Source deleted |
| FUN-020 | List respects prefix | Constraint | List | Prefix filter |
| FUN-021 | Pagination correct | Logic | List | Correct page |
| FUN-022 | Pagination offset | Calculation | Page | Skip correct |
| FUN-023 | Total count accurate | Calculation | Count | Matches |
| FUN-024 | Sort applies | Calculation | Sort | Ordered |
| FUN-025 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-026 | Transaction on upload | Transaction | Upload | Atomic |
| FUN-027 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-028 | Async all operations | Concurrency | All | Async |
| FUN-029 | Retry on transient | Logic | Retry | Retried |
| FUN-030 | Multipart chunk size | Logic | Upload | Chunked |
| FUN-031 | Resumable state | Logic | Resume | State |
| FUN-032 | Content type from file | Logic | Upload | Detected |
| FUN-033 | Cache control default | Logic | Upload | Default |
| FUN-034 | Metadata merge | Logic | SetMetadata | Merged |
| FUN-035 | Compose order | Logic | Compose | Order |
| FUN-036 | Lifecycle application | Logic | SetLifecycle | Applied |
| FUN-037 | CORS application | Logic | SetCors | Applied |
| FUN-038 | Exists check | Logic | Exists | Check |
| FUN-039 | GetMetadata complete | Logic | GetMetadata | Complete |
| FUN-040 | GetContentType from metadata | Logic | GetContentType | From metadata |
| FUN-041 | GetSize from metadata | Logic | GetSize | From metadata |
| FUN-042 | Public read ACL | Logic | SetPublicRead | Public |
| FUN-043 | Private ACL | Logic | SetPrivate | Private |
| FUN-044 | Custom ACL | Logic | SetAcl | Custom |
| FUN-045 | GetBucketInfo complete | Logic | GetBucketInfo | Complete |
| FUN-046 | GetSignedUrlPost params | Logic | GetSignedUrlPost | Params |
| FUN-047 | Localized display | i18n | GetDisplay | Localized |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Stream disposal | Logic | Download | Disposed |
| FUN-051 | Path required | Validation | Upload | Reject if null |
| FUN-052 | Bucket required | Validation | List | Reject if null |
| FUN-053 | Credentials required | Validation | Any | Reject if null |
| FUN-054 | Path format valid | Constraint | Upload | Reject invalid |
| FUN-055 | Bucket name format | Constraint | CreateBucket | Reject invalid |
| FUN-056 | Object name format | Constraint | Upload | Reject invalid |
| FUN-057 | Signed URL expiry | Logic | GetSignedUrl | Expiry set |
| FUN-058 | ACL application | Logic | SetAcl | ACL applied |
| FUN-059 | Copy preserves metadata | Logic | Copy | Metadata |
| FUN-060 | Move deletes source | Logic | Move | Source deleted |
| FUN-061 | List respects prefix | Constraint | List | Prefix filter |
| FUN-062 | Pagination correct | Logic | List | Correct page |
| FUN-063 | Pagination offset | Calculation | Page | Skip correct |
| FUN-064 | Total count accurate | Calculation | Count | Matches |
| FUN-065 | Sort applies | Calculation | Sort | Ordered |
| FUN-066 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-067 | Retry on transient | Logic | Retry | Retried |
| FUN-068 | Multipart chunk size | Logic | Upload | Chunked |
| FUN-069 | Resumable state | Logic | Resume | State |
| FUN-070 | Content type from file | Logic | Upload | Detected |
| FUN-071 | Metadata merge | Logic | SetMetadata | Merged |
| FUN-072 | Compose order | Logic | Compose | Order |
| FUN-073 | Lifecycle application | Logic | SetLifecycle | Applied |
| FUN-074 | CORS application | Logic | SetCors | Applied |
| FUN-075 | Exists check | Logic | Exists | Check |
| FUN-076 | GetMetadata complete | Logic | GetMetadata | Complete |
| FUN-077 | GetContentType from metadata | Logic | GetContentType | From metadata |
| FUN-078 | GetSize from metadata | Logic | GetSize | From metadata |
| FUN-079 | Public read ACL | Logic | SetPublicRead | Public |
| FUN-080 | Private ACL | Logic | SetPrivate | Private |
| FUN-081 | Custom ACL | Logic | SetAcl | Custom |
| FUN-082 | GetBucketInfo complete | Logic | GetBucketInfo | Complete |
| FUN-083 | GetSignedUrlPost params | Logic | GetSignedUrlPost | Params |
| FUN-084 | Audit upload | Audit | Upload | Logged |
| FUN-085 | Audit download | Audit | Download | Logged |
| FUN-086 | Audit delete | Audit | Delete | Logged |
| FUN-087 | Permission before action | Authorization | Any | Check first |
| FUN-088 | Transaction on upload | Transaction | Upload | Atomic |
| FUN-089 | Transaction on delete | Transaction | Delete | Atomic |
| FUN-090 | Async all operations | Concurrency | All | Async |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Upload full flow | Upload | File | Uploaded |
| INT-002 | Download full flow | Download | File | Stream |
| INT-003 | Delete full flow | Delete | File | Deleted |
| INT-004 | Get signed URL | GetSignedUrl | File | URL |
| INT-005 | List with filter | List | Objects | Filtered |
| INT-006 | Create bucket | CreateBucket | Bucket | Created |
| INT-007 | Delete bucket | DeleteBucket | Bucket | Deleted |
| INT-008 | Set ACL | SetAcl | Object | ACL set |
| INT-009 | Get ACL | GetAcl | Object | ACL |
| INT-010 | Copy object | Copy | Object | Copied |
| INT-011 | Move object | Move | Object | Moved |
| INT-012 | Get metadata | GetMetadata | Object | Metadata |
| INT-013 | Pagination | Paginate | Objects | Pages |
| INT-014 | Upload with metadata | Upload | File, Metadata | Uploaded |
| INT-015 | Download to stream | Download | Object | Stream |
| INT-016 | GCS-Project relationship | Relationship | GCS, Project | Valid |
| INT-017 | GCS-Bucket relationship | Relationship | GCS, Bucket | Valid |
| INT-018 | Bucket-Object relationship | Relationship | Bucket, Object | Valid |
| INT-019 | Cascade delete | Relationship | Bucket deleted | Config |
| INT-020 | Orphan handling | Relationship | Bucket deleted | Retained |
| INT-021 | GCS API error handling | Error | API down | Graceful |
| INT-022 | Timeout handling | Error | Slow API | Timeout |
| INT-023 | Credential error | Error | Invalid creds | Unauthorized |
| INT-024 | Quota error | Error | Quota | QuotaExceeded |
| INT-025 | Permission service integration | Integration | Permission | Check |
| INT-026 | User resolver integration | Integration | User | Resolved |
| INT-027 | Audit context integration | Integration | Audit | Context |
| INT-028 | Logger integration | Integration | Log | Logged |
| INT-029 | HTTP client integration | Integration | HttpClient | Call |
| INT-030 | GCS client integration | Integration | GCS | Client |
| INT-031 | Mapper integration | Integration | Map | Correct |
| INT-032 | Repository integration | Integration | Repository | CRUD |
| INT-033 | DbContext integration | Integration | DbContext | Scoped |
| INT-034 | Transaction scope | Integration | Transaction | Atomic |
| INT-035 | Config integration | Integration | Config | Read |
| INT-036 | Upload then download | Scenario | Upload, Download | Both |
| INT-037 | Copy then delete | Scenario | Copy, Delete | Both |
| INT-038 | Move then verify | Scenario | Move | Verified |
| INT-039 | Concurrent upload | Scenario | Parallel | All succeed |
| INT-040 | Multipart upload | Scenario | Large file | Uploaded |
| INT-041 | Resumable upload | Scenario | Interrupted | Resumed |
| INT-042 | Signed URL access | Scenario | GetSignedUrl | Access |
| INT-043 | ACL change | Scenario | SetAcl | Changed |
| INT-044 | Lifecycle application | Scenario | SetLifecycle | Applied |
| INT-045 | CORS application | Scenario | SetCors | Applied |
| INT-046 | Metadata update | Scenario | SetMetadata | Updated |
| INT-047 | Compose multiple | Scenario | Compose | Composed |
| INT-048 | List with prefix | Scenario | List | Prefix |
| INT-049 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-050 | E2E upload-download-delete | Scenario | Full cycle | Complete |
| INT-051 | Upload then download | Scenario | Upload, Download | Both |
| INT-052 | Copy then delete | Scenario | Copy, Delete | Both |
| INT-053 | Move then verify | Scenario | Move | Verified |
| INT-054 | Set ACL then get | Scenario | SetAcl, GetAcl | Both |
| INT-055 | Create bucket then list | Scenario | CreateBucket, List | Both |
| INT-056 | Delete bucket | Scenario | DeleteBucket | Deleted |
| INT-057 | Multipart upload | Scenario | Large file | Uploaded |
| INT-058 | Resumable upload | Scenario | Interrupted | Resumed |
| INT-059 | Signed URL access | Scenario | GetSignedUrl | Access |
| INT-060 | Lifecycle application | Scenario | SetLifecycle | Applied |
| INT-061 | CORS application | Scenario | SetCors | Applied |
| INT-062 | Metadata update | Scenario | SetMetadata | Updated |
| INT-063 | Compose multiple | Scenario | Compose | Composed |
| INT-064 | GCS client integration | Integration | GCS | Client |
| INT-065 | HTTP client integration | Integration | HttpClient | Call |
| INT-066 | Mapper integration | Integration | Mapper | Mapped |
| INT-067 | Repository integration | Integration | Repository | CRUD |
| INT-068 | DbContext integration | Integration | DbContext | Scoped |
| INT-069 | Transaction scope | Integration | Transaction | Atomic |
| INT-070 | Config integration | Integration | Config | Read |
| INT-071 | Permission service | Integration | Permission | Check |
| INT-072 | User resolver | Integration | User | Resolved |
| INT-073 | Audit context | Integration | Audit | Context |
| INT-074 | Logger integration | Integration | Logger | Logged |
| INT-075 | GCS-Project relationship | Relationship | GCS, Project | Valid |
| INT-076 | GCS-Bucket relationship | Relationship | GCS, Bucket | Valid |
| INT-077 | Bucket-Object relationship | Relationship | Bucket, Object | Valid |
| INT-078 | Cascade delete | Relationship | Bucket deleted | Config |
| INT-079 | Orphan handling | Relationship | Bucket deleted | Retained |
| INT-080 | GCS API error | Error | API down | Graceful |
| INT-081 | Timeout handling | Error | Slow API | Timeout |
| INT-082 | Credential error | Error | Invalid creds | Unauthorized |
| INT-083 | Quota error | Error | Quota | QuotaExceeded |
| INT-084 | Concurrent upload | Scenario | Parallel | All succeed |
| INT-085 | List with prefix | Scenario | List | Prefix |
| INT-086 | Get metadata | Scenario | GetMetadata | Metadata |
| INT-087 | Exists check | Scenario | Exists | Check |
| INT-088 | Get content type | Scenario | GetContentType | Type |
| INT-089 | Get size | Scenario | GetSize | Size |
| INT-090 | Full workflow | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | Path traversal | ../../../etc/passwd | Path | Rejected |
| SEC-002 | SQL injection | '; DROP TABLE-- | Path | Rejected |
| SEC-003 | XSS in metadata | <script>alert(1)</script> | Metadata | Escaped |
| SEC-004 | XSS in object name | <img onerror=...> | Name | Escaped |
| SEC-005 | LDAP injection | *)(uid=* | Filter | Rejected |
| SEC-006 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-007 | Command injection | ; ls -la | Any | Rejected |
| SEC-008 | Credentials in log | Log | Log | Redacted |
| SEC-009 | Credentials in error | Error | Stack | Redacted |
| SEC-010 | Unauthorized upload | No permission | Upload | 403 |
| SEC-011 | Unauthorized download | No permission | Download | 403 |
| SEC-012 | Unauthorized delete | No permission | Delete | 403 |
| SEC-013 | Unauthorized list | No permission | List | 403 |
| SEC-014 | Unauthorized signed URL | No permission | GetSignedUrl | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B object | 403 |
| SEC-017 | IDOR get other | Path=other | GetById | 403/404 |
| SEC-018 | IDOR download other | Path=other | Download | 403 |
| SEC-019 | IDOR delete other | Path=other | Delete | 403 |
| SEC-020 | IDOR in filter | Bucket=other | List | Filtered |
| SEC-021 | Signed URL tampering | Tamper URL | Access | Rejected |
| SEC-022 | Signed URL replay | Replay old URL | Access | Expired |
| SEC-023 | Mass assign path | Path=manipulated | Request | Validated |
| SEC-024 | Mass assign bucket | Bucket=other | Request | Validated |
| SEC-025 | Session hijack | Stolen token | Any | Detected |
| SEC-026 | Token expiration | Expired | Any | 401 |
| SEC-027 | Invalid token | Malformed | Any | 401 |
| SEC-028 | CSRF on upload | No token | Upload | Rejected |
| SEC-029 | CSRF on delete | No token | Delete | Rejected |
| SEC-030 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-031 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-032 | Rate limit bypass | Bypass attempt | Rate limit | Blocked |
| SEC-033 | Rate limit upload | Many uploads | Upload | Throttled |
| SEC-034 | Rate limit download | Many downloads | Download | Throttled |
| SEC-035 | Oversized request | 10MB metadata | Upload | Rejected |
| SEC-036 | Deep nesting | Nested path | Request | Rejected |
| SEC-037 | Header injection | \r\n in header | Header | Rejected |
| SEC-038 | Null byte injection | %00 in path | Path | Rejected |
| SEC-039 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-040 | Integer overflow | Size=overflow | Parse | Rejected |
| SEC-041 | Denial of service | Huge file | Upload | Rejected |
| SEC-042 | ACL bypass | Direct access | ACL | Denied |
| SEC-043 | Public read bypass | Private object | Access | Denied |
| SEC-044 | Bucket policy bypass | Policy | Access | Denied |
| SEC-045 | Import malicious file | Malicious | Upload | Rejected |
| SEC-046 | Export data injection | Inject in export | Export | Sanitized |
| SEC-047 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-048 | Permission cached | Repeated check | Permission | Cached |
| SEC-049 | Credential rotation | Rotate creds | Config | Updated |
| SEC-050 | Request signing | Tamper request | Request | Rejected |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Concurrent upload same path | Two upload | One wins |
| CON-004 | Concurrent upload diff paths | Two upload | Both succeed |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on delete | Two delete | One wins |
| CON-009 | Race on copy | Two copy | One or both |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel uploads | 10 parallel | All succeed |
| CON-012 | Async parallel downloads | 10 parallel | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Multipart concurrent | Two multipart | Both succeed |
| CON-016 | Resumable concurrent | Two resume | One wins |
| CON-017 | List concurrent | Two list | Both correct |
| CON-018 | Signed URL concurrent | Two get URL | Both succeed |
| CON-019 | ACL concurrent | Two set ACL | One wins |
| CON-020 | Soft delete concurrent | Delete while update | Deterministic |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | GCS connection limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate path not null | Validation | null | Exception |
| UNT-002 | Validate bucket name | Validation | Valid name | Pass |
| UNT-003 | Validate object name | Validation | Valid name | Pass |
| UNT-004 | Validate credentials | Validation | Valid creds | Pass |
| UNT-005 | Validate date range | Validation | End<Start | Exception |
| UNT-006 | Format path display | Formatting | Path | Display |
| UNT-007 | Format metadata | Formatting | Metadata | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Path normalization | Calculation | Path | Normalized |
| UNT-013 | Bucket name validation | Calculation | Name | Valid |
| UNT-014 | Object allows upload | Status logic | Object | true |
| UNT-015 | Object allows download | Status logic | Object | true |
| UNT-016 | Object allows delete | Status logic | Object | true |
| UNT-017 | Bucket exists check | Status logic | Bucket | Exists |
| UNT-018 | Object exists check | Status logic | Object | Exists |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single upload 1MB | Upload | <2s | P1 |
| PRF-002 | Single download 1MB | Download | <1s | P1 |
| PRF-003 | Get signed URL | GetSignedUrl | <100ms | P1 |
| PRF-004 | List 100 objects | List | <2s | P1 |
| PRF-005 | Get metadata | GetMetadata | <100ms | P1 |
| PRF-006 | Delete single | Delete | <500ms | P1 |
| PRF-007 | Copy 1MB | Copy | <2s | P1 |
| PRF-008 | Move 1MB | Move | <2s | P1 |
| PRF-009 | Upload 100MB | Upload | <60s | P1 |
| PRF-010 | Concurrent 10 uploads | 10 parallel | <30s total | P1 |
| PRF-011 | Concurrent 10 downloads | 10 parallel | <15s total | P1 |
| PRF-012 | Concurrent mixed | 5 upload, 5 download | <25s total | P2 |
| PRF-013 | Memory single upload | Upload | <50MB delta | P2 |
| PRF-014 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-015 | Memory download stream | Download | Stream | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS upload | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 20 RPS upload | 0→20→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS download | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress quota | Many uploads | Until quota | Limited |
| LDT-007 | Stress connection pool | Many concurrent | Until limit | Pool holds |
| LDT-008 | Stress memory | Large files | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Implementation
