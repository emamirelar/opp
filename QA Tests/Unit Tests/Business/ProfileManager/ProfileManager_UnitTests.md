# ProfileManager — Unit Test Cases

**Component:** `UNOPS.PAO.Business/Managers/ProfileManager` (Unit Tests)  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
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

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Profile manager unit tests cover user profile CRUD, avatar handling, preferences, and org unit association. Tests include: profile CRUD, avatar upload/delete, preference management, org unit assignment, privacy settings, profile display, validation, and audit trails.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result |
|----|-----------|--------------|-------|-----------------|
| POS-001 | Get user profile | Profile exists | GetProfile | Profile returned |
| POS-002 | Create profile | Valid data | Create | Profile created |
| POS-003 | Update profile | Profile exists | Update | Updated |
| POS-004 | Delete profile | Profile exists | Delete | Soft deleted |
| POS-005 | Upload avatar | Valid image | UploadAvatar | Avatar stored |
| POS-006 | Delete avatar | Avatar exists | DeleteAvatar | Removed |
| POS-007 | Get preferences | User has prefs | GetPreferences | Preferences |
| POS-008 | Update preferences | User exists | UpdatePreferences | Updated |
| POS-009 | Get org unit | User assigned | GetOrgUnit | Org unit |
| POS-010 | Assign org unit | Org unit exists | AssignOrgUnit | Assigned |
| POS-011 | Get public profile | Profile exists | GetPublicProfile | Public data |
| POS-012 | Get full profile | Own profile | GetFullProfile | Full data |
| POS-013 | Update privacy settings | User exists | UpdatePrivacy | Updated |
| POS-014 | Get privacy settings | User exists | GetPrivacy | Settings |
| POS-015 | Audit CreatedBy | Create | Check audit | Set |
| POS-016 | Audit CreatedDate | Create | Check audit | UTC |
| POS-017 | Audit LastModifiedBy | Update | Check audit | Set |
| POS-018 | Audit LastModifiedDate | Update | Check audit | UTC |
| POS-019 | Soft delete DeletedBy | Delete | Check audit | Set |
| POS-020 | Soft delete DeletedDate | Delete | Check audit | UTC |
| POS-021 | Validate avatar format | Valid format | Validate | Valid |
| POS-022 | Default preferences | New user | GetPreferences | Defaults |
| POS-023 | Search profiles | Profiles exist | Search | Matching |
| POS-024 | Pagination | Many profiles | List page | Page |
| POS-025 | Sort by name | Profiles exist | Sort | Ordered |
| POS-026 | Filter by org unit | Profiles exist | Filter | Filtered |
| POS-027 | Avatar resize | Large image | UploadAvatar | Resized |
| POS-028 | Profile merge | Partial update | Update | Merged |
| POS-029 | Preference default | New pref | GetPreference | Default |
| POS-030 | Org unit hierarchy | Org unit has parent | GetOrgUnit | Hierarchy |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Action | Expected Result |
|----|-----------|---------------------|-----------------|
| NEG-001 | Get profile zero user | UserId=0 | ArgumentException |
| NEG-002 | Get profile negative user | UserId=-1 | ArgumentException |
| NEG-003 | Create with null user | User=null | ArgumentNullException |
| NEG-004 | Update non-existent | UserId=99999 | KeyNotFoundException |
| NEG-005 | Delete non-existent | UserId=99999 | KeyNotFoundException |
| NEG-006 | Upload avatar null | File=null | ArgumentNullException |
| NEG-007 | Upload avatar invalid format | Format=exe | ValidationException |
| NEG-008 | Upload avatar too large | Size>limit | ValidationException |
| NEG-009 | Delete avatar no avatar | No avatar | BusinessException |
| NEG-010 | GetById without permission | Unauthorized | Forbidden |
| NEG-011 | Update without permission | Unauthorized | Forbidden |
| NEG-012 | Delete without permission | Unauthorized | Forbidden |
| NEG-013 | Upload avatar unauthorized | Unauthorized | Forbidden |
| NEG-014 | Update preferences unauthorized | Unauthorized | Forbidden |
| NEG-015 | Get other user full profile | Other user | Forbidden |
| NEG-016 | SQL injection in search | '; DROP | Rejected |
| NEG-017 | XSS in display name | <script> | Escaped |
| NEG-018 | Path traversal in avatar | ../../../etc | Rejected |
| NEG-019 | Invalid org unit | OrgUnitId=-1 | ArgumentException |
| NEG-020 | Deleted org unit | Org unit deleted | KeyNotFoundException |
| NEG-021 | Invalid preference key | Key invalid | ArgumentException |
| NEG-022 | Null preference value | Value=null | ArgumentNullException |
| NEG-023 | DbContext disposed | After dispose | ObjectDisposedException |
| NEG-024 | Concurrent update conflict | Stale entity | ConcurrencyException |
| NEG-025 | Connection timeout | DB unavailable | TimeoutException |
| NEG-026 | Invalid privacy setting | Setting invalid | ArgumentException |
| NEG-027 | Export unauthorized | Unauthorized | Forbidden |
| NEG-028 | Import invalid data | Malformed | ValidationException |
| NEG-029 | Expired session | Expired token | Unauthorized |
| NEG-030 | Null user context | User=null | InvalidOperationException |
| NEG-031 | Invalid page number | Page=0 | ArgumentException |
| NEG-032 | Invalid page size | PageSize=0 | ArgumentException |
| NEG-033 | Search null term | Term=null | ArgumentNullException |
| NEG-034 | Filter malformed | Malformed filter | ArgumentException |
| NEG-035 | Avatar malicious | Executable | ValidationException |
| NEG-036 | Update deleted | Profile deleted | KeyNotFoundException |
| NEG-037 | GetById deleted | Profile deleted | KeyNotFoundException |
| NEG-038 | Child override throws | Child throws | Propagated |
| NEG-039 | Assign org unit invalid | Org unit invalid | ArgumentException |
| NEG-040 | Privacy override | Invalid override | BusinessException |
| NEG-041 | Duplicate profile | Profile exists | BusinessException |
| NEG-042 | Audit missing user | User=0 | InvalidOperationException |
| NEG-043 | Permission null resource | Resource=null | ArgumentNullException |
| NEG-044 | Avatar path traversal | Path=../../../ | Rejected |
| NEG-045 | Pagination overflow | Page too large | Empty or error |
| NEG-046 | Sort invalid field | Sort invalid | ArgumentException |
| NEG-047 | Import overwrite | Overwrite flag | Config |
| NEG-048 | Export invalid format | Format invalid | ArgumentException |
| NEG-049 | Profile incomplete | Missing required | ValidationException |
| NEG-050 | Timezone invalid | Timezone invalid | ArgumentException |
| NEG-051 | Locale invalid | Locale invalid | ArgumentException |
| NEG-052 | Cross-tenant profile | Other tenant | Forbidden |
| NEG-053 | GetPublicProfile deleted | Profile deleted | KeyNotFoundException |
| NEG-054 | UpdatePreferences deleted | Profile deleted | KeyNotFoundException |
| NEG-055 | AssignOrgUnit deleted | Profile deleted | KeyNotFoundException |
| NEG-056 | Invalid include path | Invalid include | ArgumentException |
| NEG-057 | MIME type spoofing | Wrong MIME | ValidationException |
| NEG-058 | Avatar dimensions max | 10000px | ValidationException |
| NEG-059 | Preference value max | Value too long | ValidationException |
| NEG-060 | Org unit not found | OrgUnitId=99999 | KeyNotFoundException |
| NEG-061 | Null display name | DisplayName=null | ArgumentNullException |
| NEG-062 | Empty display name | DisplayName="" | ValidationException |
| NEG-063 | Bio too long | Bio 10k chars | ValidationException |
| NEG-064 | Phone invalid format | Bad phone | ValidationException |
| NEG-065 | Email invalid format | Bad email | ValidationException |
| NEG-066 | Duplicate email | Email exists | BusinessException |
| NEG-067 | Null navigation | Unloaded nav | NullReferenceException |
| NEG-068 | Invalid enum value | Pref invalid | ArgumentException |
| NEG-069 | Storage unavailable | Storage down | StorageException |
| NEG-070 | Avatar corrupt | Corrupt image | ValidationException |
| NEG-071 | GetProfile null user | User=null | ArgumentNullException |
| NEG-072 | Create profile null request | Request=null | ArgumentNullException |
| NEG-073 | Update profile null request | Request=null | ArgumentNullException |
| NEG-074 | GetCompleteness invalid user | UserId=0 | ArgumentException |
| NEG-075 | GetTimezone invalid user | UserId=-1 | ArgumentException |
| NEG-076 | GetLocale invalid user | UserId=99999 | KeyNotFoundException |
| NEG-077 | UpdatePrivacy null settings | Settings=null | ArgumentNullException |
| NEG-078 | GetPrivacy deleted profile | Profile deleted | KeyNotFoundException |
| NEG-079 | AssignOrgUnit null org unit | OrgUnit=null | ArgumentNullException |
| NEG-080 | Search empty term | Term="" | Config |
| NEG-081 | Filter invalid org unit | OrgUnitId=0 | ArgumentException |
| NEG-082 | Export deleted profiles | Include deleted | Config |
| NEG-083 | Import null file | File=null | ArgumentNullException |
| NEG-084 | GetPublicProfile null user | User=null | ArgumentNullException |
| NEG-085 | GetFullProfile other user | Other user | Forbidden |
| NEG-086 | UpdatePreferences invalid key | Key invalid | ArgumentException |
| NEG-087 | Avatar format unsupported | Format=webp | ValidationException |
| NEG-088 | Profile merge null fields | Fields=null | ArgumentNullException |
| NEG-089 | GetOrgUnit deleted profile | Profile deleted | KeyNotFoundException |
| NEG-090 | Preference value null | Value=null | ArgumentNullException |

---

## §3 Boundary Tests (90)

| ID | Test Name | Boundary Condition | Expected Result |
|----|-----------|-------------------|-----------------|
| BND-001 | Display name at min | Length=1 | Valid |
| BND-002 | Display name at max | Length=255 | Valid |
| BND-003 | Display name exceeds max | Length=256 | Reject |
| BND-004 | Avatar size at limit | Size=limit | Valid |
| BND-005 | Avatar size over limit | Size=limit+1 | Reject |
| BND-006 | Avatar size zero | Size=0 | Reject |
| BND-007 | User ID at Int32.MaxValue | UserId=2147483647 | Handle |
| BND-008 | User ID at zero | UserId=0 | Reject |
| BND-009 | Page size at min | PageSize=1 | Valid |
| BND-010 | Page size at max | PageSize=100 | Valid |
| BND-011 | Page size over max | PageSize=101 | Reject |
| BND-012 | Bio at max | Length=4000 | Valid |
| BND-013 | Bio over max | Length=4001 | Reject |
| BND-014 | Preference key max | Key length | Valid or reject |
| BND-015 | Preference value max | Value length | Truncate or reject |
| BND-016 | Unicode in name | Arabic/Chinese | Stored |
| BND-017 | Special chars in bio | <>&"' | Escaped |
| BND-018 | Leading/trailing spaces | Name="  x  " | Trimmed |
| BND-019 | Avatar min dimensions | 1x1 | Valid or reject |
| BND-020 | Avatar max dimensions | 4096x4096 | Valid |
| BND-021 | Date at min | Date=MinValue | Handle |
| BND-022 | Date at max | Date=MaxValue | Handle |
| BND-023 | DateTime UTC | UTC input | Stored |
| BND-024 | Empty search term | Term="" | Return all |
| BND-025 | Search term max | Term=500 | Valid |
| BND-026 | Search term over max | Term=501 | Reject |
| BND-027 | Collection empty | [] | No exception |
| BND-028 | Collection single | 1 item | Valid |
| BND-029 | Preferences empty | No prefs | Defaults |
| BND-030 | Preferences all set | All set | Valid |
| BND-031 | Pagination last partial | Partial page | Correct |
| BND-032 | Pagination total | Total count | Accurate |
| BND-033 | Sort null handling | Nulls in data | Deterministic |
| BND-034 | Filter combination all | All filters | Correct |
| BND-035 | Org unit null | Not assigned | Null |
| BND-036 | Org unit max int | OrgUnitId=2147483647 | Handle |
| BND-037 | Soft delete boundary | DeletedDate set | Excluded |
| BND-038 | Include depth | Deep include | No explosion |
| BND-039 | Query timeout | Slow query | Timeout |
| BND-040 | Audit timestamp precision | Millisecond | Stored |
| BND-041 | Avatar format boundary | JPEG, PNG | Valid |
| BND-042 | Avatar format invalid | EXE | Reject |
| BND-043 | Async cancellation | Cancel token | OperationCanceledException |
| BND-044 | Task timeout | Timeout | TimeoutException |
| BND-045 | Concurrent same second | Same timestamp | Deterministic |
| BND-046 | Privacy all on | All on | Valid |
| BND-047 | Privacy all off | All off | Valid |
| BND-048 | Timezone boundary | UTC | Valid |
| BND-049 | Locale boundary | en-US | Valid |
| BND-050 | Completeness 0 | Empty profile | 0 |
| BND-051 | Completeness 100 | Full profile | 100 |
| BND-052 | Filter empty result | No match | Empty list |
| BND-053 | Sort empty | Empty list | No exception |
| BND-054 | Pagination empty | No data | Empty |
| BND-055 | Export large result | 10k rows | Stream |
| BND-056 | Import large | 1000 records | Valid |
| BND-057 | Avatar aspect ratio | 16:9 | Valid |
| BND-058 | Avatar aspect ratio extreme | 100:1 | Reject or crop |
| BND-059 | Profile merge partial | Partial fields | Merged |
| BND-060 | Profile merge full | All fields | Replaced |
| BND-061 | Org unit depth | Deep hierarchy | Valid |
| BND-062 | Multiple org units | Multi-org | Config |
| BND-063 | Preference count max | Many prefs | Valid |
| BND-064 | Preference count over | Too many | Reject |
| BND-065 | Phone max length | 20 chars | Valid |
| BND-066 | Email max length | 254 chars | Valid |
| BND-067 | Bio empty | Bio="" | Valid |
| BND-068 | GetPublicProfile fields | Limited fields | Correct |
| BND-069 | GetFullProfile fields | All fields | Correct |
| BND-070 | Concurrent avatar upload | Two upload | One wins |
| BND-071 | Display name whitespace | Name="   " | Reject |
| BND-072 | OrgUnitId at zero | OrgUnitId=0 | Reject |
| BND-073 | GetCompleteness empty | Empty profile | 0 |
| BND-074 | GetCompleteness full | Full profile | 100 |
| BND-075 | Timezone IANA format | IANA zone | Valid |
| BND-076 | Locale BCP47 format | BCP47 | Valid |
| BND-077 | Preference key empty | Key="" | Reject |
| BND-078 | Privacy setting partial | Partial | Valid |
| BND-079 | Avatar dimensions min | 1x1 | Config |
| BND-080 | Export format boundary | Each format | Valid |
| BND-081 | Import encoding | UTF-8 | Valid |
| BND-082 | Profile merge null | Null merge | No-op |
| BND-083 | GetOrgUnit unassigned | Not assigned | Null |
| BND-084 | Search max results | Max results | Paginate |
| BND-085 | Filter by status | Status filter | Filtered |
| BND-086 | Sort multi-column | 3 columns | Correct |
| BND-087 | GetPreferences empty | No prefs | Defaults |
| BND-088 | UpdatePreferences partial | Partial | Merged |
| BND-089 | AssignOrgUnit same | Same org | No-op |
| BND-090 | DeleteAvatar no-op | No avatar | No-op |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Workflow | Trigger | Expected Outcome |
|----|-----------|---------------|---------|------------------|
| FUN-001 | User required | Validation | GetProfile | Reject if zero |
| FUN-002 | Display name required | Validation | Create | Reject if empty |
| FUN-003 | Avatar format whitelist | Constraint | UploadAvatar | Only allowed |
| FUN-004 | Soft delete excludes | Constraint | List | Excludes IsDeleted |
| FUN-005 | GetById excludes deleted | Constraint | GetById | 404 if deleted |
| FUN-006 | Update excludes deleted | Constraint | Update | Reject if deleted |
| FUN-007 | Avatar size limit | Constraint | UploadAvatar | Reject over |
| FUN-008 | Org unit must exist | Constraint | AssignOrgUnit | Reject invalid |
| FUN-009 | Audit CreatedBy | Audit | Create | Set user |
| FUN-010 | Audit CreatedDate | Audit | Create | Set UTC |
| FUN-011 | Audit LastModifiedBy | Audit | Update | Set user |
| FUN-012 | Audit LastModifiedDate | Audit | Update | Set UTC |
| FUN-013 | Soft delete DeletedBy | Audit | Delete | Set user |
| FUN-014 | Soft delete DeletedDate | Audit | Delete | Set UTC |
| FUN-015 | Permission before action | Authorization | Any | Check first |
| FUN-016 | Own profile full access | Constraint | GetFullProfile | Own only |
| FUN-017 | Other profile public only | Constraint | GetProfile | Public fields |
| FUN-018 | List respects IsDeleted | Constraint | List | Excludes deleted |
| FUN-019 | Search excludes deleted | Constraint | Search | Excludes deleted |
| FUN-020 | Preference merge | Logic | UpdatePreferences | Merged |
| FUN-021 | Avatar resize on upload | Logic | UploadAvatar | Resized |
| FUN-022 | Default preferences | Logic | GetPreferences | Defaults |
| FUN-023 | Privacy filter | Logic | GetPublicProfile | Filtered |
| FUN-024 | Completeness calculation | Logic | GetCompleteness | Calculated |
| FUN-025 | Org unit validation | Logic | AssignOrgUnit | Valid |
| FUN-026 | Pagination offset | Calculation | Page | Skip correct |
| FUN-027 | Total count accurate | Calculation | Count | Matches |
| FUN-028 | Sort applies | Calculation | Sort | Ordered |
| FUN-029 | Filter AND logic | Filter | Multi-filter | All match |
| FUN-030 | Transaction on create | Transaction | Create | Atomic |
| FUN-031 | Transaction on update | Transaction | Update | Atomic |
| FUN-032 | Async all operations | Concurrency | All | Async |
| FUN-033 | Include loads org unit | Data load | GetById include | Org unit loaded |
| FUN-034 | No Cartesian on includes | Data load | Multiple includes | Split queries |
| FUN-035 | Profile create on first | Logic | GetProfile | Create if missing |
| FUN-036 | Avatar path unique | Logic | UploadAvatar | Unique path |
| FUN-037 | Export format | Logic | Export | Format |
| FUN-038 | Import validation | Logic | Import | Validated |
| FUN-039 | Timezone default | Logic | GetTimezone | Default |
| FUN-040 | Locale default | Logic | GetLocale | Default |
| FUN-041 | Privacy default | Logic | GetPrivacy | Default |
| FUN-042 | Export excludes deleted | Constraint | Export | Excludes deleted |
| FUN-043 | Import creates profile | Logic | Import | Created |
| FUN-044 | Config avatar size | Config | UploadAvatar | Config |
| FUN-045 | Config avatar formats | Config | UploadAvatar | Config |
| FUN-046 | Localized display | i18n | GetDisplay | Localized |
| FUN-047 | Status transition | Workflow | ChangeStatus | Valid only |
| FUN-048 | Permission cached | Performance | Repeated check | Cached |
| FUN-049 | AsNoTracking read-only | Performance | List | No tracking |
| FUN-050 | Avatar caching | Performance | Repeated get | Cached |
| FUN-051 | GetPublicProfile fields | Logic | GetPublicProfile | Limited |
| FUN-052 | GetFullProfile fields | Logic | GetFullProfile | All |
| FUN-053 | Completeness weights | Logic | GetCompleteness | Weighted |
| FUN-054 | Preference default keys | Logic | GetPreferences | Default keys |
| FUN-055 | Privacy setting validation | Validation | UpdatePrivacy | Valid |
| FUN-056 | Org unit hierarchy load | Logic | GetOrgUnit | Hierarchy |
| FUN-057 | Profile merge strategy | Logic | Update | Merge |
| FUN-058 | Avatar format validation | Validation | UploadAvatar | Format |
| FUN-059 | Search index | Logic | Search | Index |
| FUN-060 | Filter by org unit | Logic | Filter | Org unit |
| FUN-061 | Sort by completeness | Logic | Sort | Completeness |
| FUN-062 | Export user filter | Logic | Export | User filter |
| FUN-063 | Import overwrite | Logic | Import | Overwrite |
| FUN-064 | Timezone validation | Validation | GetTimezone | Valid |
| FUN-065 | Locale validation | Validation | GetLocale | Valid |
| FUN-066 | AssignOrgUnit replace | Logic | AssignOrgUnit | Replace |
| FUN-067 | DeleteAvatar cleanup | Logic | DeleteAvatar | Cleanup |
| FUN-068 | UpdatePreferences validate | Validation | UpdatePreferences | Valid |
| FUN-069 | GetProfile lazy create | Logic | GetProfile | Create |
| FUN-070 | Profile merge audit | Audit | Update | Audit |
| FUN-071 | Avatar upload audit | Audit | UploadAvatar | Audit |
| FUN-072 | Preference update audit | Audit | UpdatePreferences | Audit |
| FUN-073 | Org unit assign audit | Audit | AssignOrgUnit | Audit |
| FUN-074 | Privacy update audit | Audit | UpdatePrivacy | Audit |
| FUN-075 | Search relevance | Logic | Search | Relevance |
| FUN-076 | Filter combination | Logic | Filter | Combined |
| FUN-077 | Pagination total | Calculation | Paginate | Total |
| FUN-078 | Sort multi-field | Logic | Sort | Multi-field |
| FUN-079 | Export encoding | Logic | Export | Encoding |
| FUN-080 | Import encoding | Logic | Import | Encoding |
| FUN-081 | Completeness threshold | Logic | GetCompleteness | Threshold |
| FUN-082 | Avatar dimension limit | Constraint | UploadAvatar | Limit |
| FUN-083 | Preference key whitelist | Constraint | UpdatePreferences | Whitelist |
| FUN-084 | Privacy setting whitelist | Constraint | UpdatePrivacy | Whitelist |
| FUN-085 | Org unit type check | Constraint | AssignOrgUnit | Type |
| FUN-086 | Profile required fields | Validation | Create | Required |
| FUN-087 | Bio length limit | Constraint | Update | Limit |
| FUN-088 | Email format | Validation | Create | Format |
| FUN-089 | Phone format | Validation | Create | Format |
| FUN-090 | Display name format | Logic | Create | Format |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities | Expected Result |
|----|-----------|----------|----------|-----------------|
| INT-001 | Get profile full flow | GetProfile | Profile | Returned |
| INT-002 | Create profile full flow | Create | Profile | Created |
| INT-003 | Update profile full flow | Update | Profile | Updated |
| INT-004 | Delete profile full flow | Delete | Profile | Soft deleted |
| INT-005 | Upload avatar full flow | UploadAvatar | Profile, Avatar | Uploaded |
| INT-006 | Get with org unit | GetById | Profile, OrgUnit | Org unit loaded |
| INT-007 | List with filter and sort | List | Profile | Filtered, sorted |
| INT-008 | Update preferences | UpdatePreferences | Profile | Updated |
| INT-009 | Assign org unit | AssignOrgUnit | Profile, OrgUnit | Assigned |
| INT-010 | Get public profile | GetPublicProfile | Profile | Public data |
| INT-011 | Profile-User relationship | Relationship | Profile, User | FK valid |
| INT-012 | Profile-OrgUnit relationship | Relationship | Profile, OrgUnit | Valid |
| INT-013 | Cascade soft delete | Relationship | User deleted | Config |
| INT-014 | Orphan handling | Relationship | User deleted | Retained |
| INT-015 | Storage integration | Integration | Storage | Avatar stored |
| INT-016 | DB error handling | Error | DB down | Graceful |
| INT-017 | Storage error handling | Error | Storage down | Graceful |
| INT-018 | Timeout handling | Error | Slow | Timeout |
| INT-019 | Constraint violation | Error | FK violation | Clear error |
| INT-020 | Permission service integration | Integration | Permission | Check |
| INT-021 | User resolver integration | Integration | User | Resolved |
| INT-022 | Audit context integration | Integration | Audit | Context |
| INT-023 | Logger integration | Integration | Log | Logged |
| INT-024 | OrganizationHierarchyManager integration | Integration | OrgUnit | Org unit |
| INT-025 | Mapper integration | Integration | Map | Correct |
| INT-026 | Repository integration | Integration | Repository | CRUD |
| INT-027 | DbContext integration | Integration | DbContext | Scoped |
| INT-028 | Transaction scope | Integration | Transaction | Atomic |
| INT-029 | Avatar storage | Scenario | UploadAvatar | Stored |
| INT-030 | Preferences flow | Scenario | Get, Update | Complete |
| INT-031 | Org unit flow | Scenario | Assign, Get | Complete |
| INT-032 | Concurrent update | Scenario | Parallel | All succeed |
| INT-033 | Search with filter | Scenario | Search | Filtered |
| INT-034 | Pagination with sort | Scenario | Paginate | Sorted |
| INT-035 | Export with filter | Scenario | Export | Filtered |
| INT-036 | Import with validation | Scenario | Import | Validated |
| INT-037 | Privacy flow | Scenario | Update, Get | Complete |
| INT-038 | Avatar delete flow | Scenario | DeleteAvatar | Deleted |
| INT-039 | Profile completeness | Scenario | GetCompleteness | Score |
| INT-040 | Timezone flow | Scenario | Get, Update | Complete |
| INT-041 | Locale flow | Scenario | Get, Update | Complete |
| INT-042 | Public vs full profile | Scenario | GetPublic, GetFull | Correct |
| INT-043 | Default preferences | Scenario | New user | Defaults |
| INT-044 | Avatar resize | Scenario | Large upload | Resized |
| INT-045 | Profile merge | Scenario | Partial update | Merged |
| INT-046 | Org unit hierarchy | Scenario | Assign | Hierarchy |
| INT-047 | Export format | Scenario | Export | Format |
| INT-048 | Import overwrite | Scenario | Import | Config |
| INT-049 | Audit trail | Scenario | Create, Update | Trail |
| INT-050 | E2E create-update-delete | Scenario | Full cycle | Complete |
| INT-051 | GetProfile then Update | Scenario | Get, Update | Complete |
| INT-052 | Create then GetProfile | Scenario | Create, Get | Complete |
| INT-053 | UploadAvatar then DeleteAvatar | Scenario | Upload, Delete | Complete |
| INT-054 | GetPreferences then Update | Scenario | Get, Update | Complete |
| INT-055 | AssignOrgUnit then GetOrgUnit | Scenario | Assign, Get | Complete |
| INT-056 | GetPublicProfile vs GetFullProfile | Scenario | Both | Correct |
| INT-057 | UpdatePrivacy then GetPrivacy | Scenario | Update, Get | Complete |
| INT-058 | Search then GetProfile | Scenario | Search, Get | Complete |
| INT-059 | Filter then Paginate | Scenario | Filter, Paginate | Complete |
| INT-060 | Export then Import | Scenario | Export, Import | Complete |
| INT-061 | GetCompleteness after Update | Scenario | Update, Completeness | Complete |
| INT-062 | GetTimezone after Update | Scenario | Update, Get | Complete |
| INT-063 | GetLocale after Update | Scenario | Update, Get | Complete |
| INT-064 | Profile merge then Get | Scenario | Merge, Get | Complete |
| INT-065 | Org unit assign then hierarchy | Scenario | Assign, Hierarchy | Complete |
| INT-066 | Avatar upload then Get | Scenario | Upload, Get | Complete |
| INT-067 | Preferences merge | Scenario | Merge | Complete |
| INT-068 | Privacy update then GetPublic | Scenario | Update, GetPublic | Complete |
| INT-069 | Sort then Filter | Scenario | Sort, Filter | Complete |
| INT-070 | Paginate then Sort | Scenario | Paginate, Sort | Complete |
| INT-071 | Import then Export | Scenario | Import, Export | Complete |
| INT-072 | Create profile with org unit | Scenario | Create | Org unit |
| INT-073 | Update with preferences | Scenario | Update | Preferences |
| INT-074 | Delete with avatar | Scenario | Delete | Avatar |
| INT-075 | GetFullProfile with org | Scenario | GetFull | Org |
| INT-076 | Search with pagination | Scenario | Search | Paginated |
| INT-077 | Filter by completeness | Scenario | Filter | Completeness |
| INT-078 | Export with sort | Scenario | Export | Sorted |
| INT-079 | Import with validation | Scenario | Import | Validated |
| INT-080 | GetCompleteness threshold | Scenario | Completeness | Threshold |
| INT-081 | Timezone with locale | Scenario | Timezone, Locale | Complete |
| INT-082 | Privacy with export | Scenario | Privacy, Export | Complete |
| INT-083 | Org unit with profile | Scenario | Org unit | Profile |
| INT-084 | Avatar with profile | Scenario | Avatar | Profile |
| INT-085 | Preferences with default | Scenario | Preferences | Default |
| INT-086 | Profile with audit | Scenario | Profile | Audit |
| INT-087 | Search with relevance | Scenario | Search | Relevance |
| INT-088 | Filter with sort | Scenario | Filter | Sorted |
| INT-089 | Export with encoding | Scenario | Export | Encoding |
| INT-090 | E2E full profile lifecycle | Scenario | Full cycle | Complete |

---

## §6 Security Tests (50)

| ID | Test Name | Vector | Target | Expected Block |
|----|-----------|--------|--------|----------------|
| SEC-001 | SQL injection in search | '; DROP TABLE-- | Search | Sanitized |
| SEC-002 | SQL injection in filter | 1; DELETE | Filter | Rejected |
| SEC-003 | Path traversal in avatar | ../../../etc/passwd | Avatar | Rejected |
| SEC-004 | XSS in display name | <script>alert(1)</script> | DisplayName | Escaped |
| SEC-005 | XSS in bio | <img onerror=...> | Bio | Escaped |
| SEC-006 | LDAP injection | *)(uid=* | Search | Rejected |
| SEC-007 | NoSQL injection | {$gt: ""} | Filter | Rejected |
| SEC-008 | Command injection | ; ls -la | Any | Rejected |
| SEC-009 | Unauthorized list | No permission | List | 403 |
| SEC-010 | Unauthorized get | No permission | GetById | 403 |
| SEC-011 | Unauthorized create | No permission | Create | 403 |
| SEC-012 | Unauthorized update | No permission | Update | 403 |
| SEC-013 | Unauthorized delete | No permission | Delete | 403 |
| SEC-014 | Unauthorized avatar upload | No permission | UploadAvatar | 403 |
| SEC-015 | Role escalation | Low role | Admin | 403 |
| SEC-016 | Cross-tenant access | User A | User B profile | 403 |
| SEC-017 | IDOR get other full | Id=other | GetFullProfile | 403 |
| SEC-018 | IDOR update other | Id=other | Update | 403 |
| SEC-019 | IDOR delete other | Id=other | Delete | 403 |
| SEC-020 | IDOR in filter | UserId=other | List | Filtered |
| SEC-021 | Mass assign Id | Id=999 | Request | Ignored |
| SEC-022 | Mass assign CreatedBy | CreatedBy=1 | Request | Ignored |
| SEC-023 | Mass assign IsDeleted | IsDeleted=false | Request | Ignored |
| SEC-024 | Mass assign UserId | UserId=manipulated | Request | Ignored |
| SEC-025 | Malicious avatar | Executable | UploadAvatar | Rejected |
| SEC-026 | Session hijack | Stolen token | Any | Detected |
| SEC-027 | Token expiration | Expired | Any | 401 |
| SEC-028 | Invalid token | Malformed | Any | 401 |
| SEC-029 | CSRF on update | No token | Update | Rejected |
| SEC-030 | CSRF on delete | No token | Delete | Rejected |
| SEC-031 | Sensitive data in log | Log request | Log | PII redacted |
| SEC-032 | Sensitive data in error | Error | Stack | Sanitized |
| SEC-033 | Avatar tampering | Tamper avatar | Access | Rejected |
| SEC-034 | Replay old request | Replay | Access | Rejected |
| SEC-035 | Rate limit upload | Many uploads | UploadAvatar | Throttled |
| SEC-036 | Rate limit update | Many updates | Update | Throttled |
| SEC-037 | Rate limit list | Many lists | List | Throttled |
| SEC-038 | Oversized request | 10MB payload | Update | Rejected |
| SEC-039 | Deep nesting | Nested object | Request | Rejected |
| SEC-040 | Header injection | \r\n in header | Header | Rejected |
| SEC-041 | Null byte injection | %00 in name | DisplayName | Rejected |
| SEC-042 | Unicode normalization | Homoglyphs | Compare | Normalized |
| SEC-043 | Integer overflow | Id=overflow | Parse | Rejected |
| SEC-044 | Denial of service | Huge avatar | UploadAvatar | Rejected |
| SEC-045 | MIME type spoofing | Wrong MIME | Avatar | Rejected |
| SEC-046 | Extension bypass | .exe as .jpg | Avatar | Rejected |
| SEC-047 | Double extension | file.jpg.exe | Avatar | Rejected |
| SEC-048 | Audit log integrity | Tamper audit | Audit | Detected |
| SEC-049 | Permission cached | Repeated check | Permission | Cached |
| SEC-050 | Storage ACL | Direct access | Storage | Denied |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected Behavior |
|----|-----------|----------|-------------------|
| CON-001 | Two users update same | A, B update | Optimistic lock |
| CON-002 | Update and delete same | Update, delete | Deterministic |
| CON-003 | Double avatar upload | Two upload | One wins |
| CON-004 | Concurrent update | Two update | One wins |
| CON-005 | Read during write | Read while update | Consistent |
| CON-006 | Transaction isolation | Parallel transactions | Serializable |
| CON-007 | Stale entity update | Old version | Concurrency handled |
| CON-008 | Race on preferences | Two update | One wins |
| CON-009 | Race on org unit | Two assign | One wins |
| CON-010 | DbContext concurrency | Share context | Not shared |
| CON-011 | Async parallel gets | 10 parallel GetById | All succeed |
| CON-012 | Async parallel updates | 10 parallel Update | All succeed |
| CON-013 | Batch vs single | Batch vs loop | Same result |
| CON-014 | Pagination concurrent | Two paginate | Both correct |
| CON-015 | Avatar upload concurrent | Two upload | One wins |
| CON-016 | Preferences concurrent | Two update | One wins |
| CON-017 | Org unit concurrent | Two assign | One wins |
| CON-018 | Soft delete concurrent | Delete while update | Deterministic |
| CON-019 | Create concurrent | Two create | Both or one |
| CON-020 | Update concurrent | Two update | One wins |
| CON-021 | Idempotency | Same request twice | Same result |
| CON-022 | Lock escalation | Many locks | No escalation |
| CON-023 | Connection pool | Many concurrent | Pool limit |
| CON-024 | Storage connection limit | Many concurrent | Limit |
| CON-025 | Deadlock | Circular lock | Timeout or avoid |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output |
|----|-----------|----------|-------|-----------------|
| UNT-001 | Validate user not null | Validation | null | Exception |
| UNT-002 | Validate display name | Validation | Valid name | Pass |
| UNT-003 | Validate avatar format | Validation | Valid format | Pass |
| UNT-004 | Validate org unit | Validation | Valid org unit | Pass |
| UNT-005 | Validate preference | Validation | Valid pref | Pass |
| UNT-006 | Format display name | Formatting | Name | Formatted |
| UNT-007 | Format bio | Formatting | Bio | Formatted |
| UNT-008 | Format audit entry | Formatting | Audit | Formatted |
| UNT-009 | Calculate pagination offset | Calculation | Page, Size | Offset |
| UNT-010 | Calculate total pages | Calculation | Total, Size | Pages |
| UNT-011 | Calculate skip count | Calculation | Page, Size | Skip |
| UNT-012 | Completeness score | Calculation | Profile | Score |
| UNT-013 | Privacy filter | Calculation | Settings | Filtered |
| UNT-014 | Avatar allows upload | Status logic | Format | true |
| UNT-015 | Org unit allows assign | Status logic | Org unit | true |
| UNT-016 | Preference allows update | Status logic | Key | true |
| UNT-017 | Profile complete check | Status logic | Profile | Boolean |
| UNT-018 | Name check | Status logic | Name | Valid |
| UNT-019 | Collection distinct | Collections | Duplicates | Distinct |
| UNT-020 | Collection order | Collections | Unordered | Ordered |
| UNT-021 | Collection empty | Collections | [] | No exception |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Single get by ID | GetById | <100ms | P1 |
| PRF-002 | Single create | Create | <200ms | P1 |
| PRF-003 | Single update | Update | <200ms | P1 |
| PRF-004 | Upload avatar | UploadAvatar | <1s | P0 |
| PRF-005 | Get preferences | GetPreferences | <100ms | P0 |
| PRF-006 | Search profiles | Search | <500ms | P1 |
| PRF-007 | List with pagination | List | <300ms | P1 |
| PRF-008 | List with sort | List | <300ms | P1 |
| PRF-009 | Get public profile | GetPublicProfile | <100ms | P1 |
| PRF-010 | Concurrent 10 reads | 10 parallel GetById | <2s total | P1 |
| PRF-011 | Concurrent 5 updates | 5 parallel Update | <3s total | P1 |
| PRF-012 | Concurrent mixed | 5 read, 5 update | <5s total | P2 |
| PRF-013 | Memory list 1000 | List 1000 | <50MB | P2 |
| PRF-014 | Memory avatar | UploadAvatar | <50MB | P2 |
| PRF-015 | Memory export | Export | <100MB | P2 |
| PRF-016 | Query no N+1 | Get with includes | Single query | P0 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria |
|----|-----------|-------------|----------|-------------------|
| LDT-001 | Sustained 5 RPS create | 5 req/s | 5 min | 99% success |
| LDT-002 | Sustained 20 RPS read | 20 req/s | 5 min | 99% success |
| LDT-003 | Sustained 5 RPS mixed | 5 req/s mixed | 5 min | 99% success |
| LDT-004 | Spike 30 RPS update | 0→30→0 | 1 min | No errors |
| LDT-005 | Spike 50 RPS get | 0→50→0 | 30s | Graceful deg |
| LDT-006 | Stress find limit | Ramp to fail | Until fail | Document limit |
| LDT-007 | Stress avatar upload | Many uploads | Until limit | Holds |
| LDT-008 | Stress memory | Large avatars | Until OOM | Document limit |
| LDT-009 | Recovery after spike | Spike then normal | 2 min | Return normal |
| LDT-010 | Recovery after stress | Stress then stop | 5 min | Recovery |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
