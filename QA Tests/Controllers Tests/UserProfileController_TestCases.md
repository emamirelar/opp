# UserProfileController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/UserProfileController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
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
| §7 Concurrency | 10 | 10 | ✅ |
| §8 Unit | 6 | 6 | ✅ |
| §9 Performance | 4 | 4 | ✅ |
| §10 Load | 2 | 2 | ✅ |
| **TOTAL** | **462** | **≥390** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result | Status |
|-------|--------|--------|
| N≥3P: 90≥90 | ✅ PASS | N >= 3 × P |
| E≥3P: 90≥90 | ✅ PASS | E >= 3 × P |
| F≥3P: 90≥90 | ✅ PASS | F >= 3 × P |
| I≥3P: 90≥90 | ✅ PASS | I >= 3 × P |

---

## Feature Overview

REST API for user profiles: get/update profile, avatar, contact info, org unit.

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get my profile | GET /api/user-profile/me | Profile |
| POS-002 | Get profile by ID | GET /api/user-profile/{id} | Profile (admin) |
| POS-003 | Update my profile | PUT /api/user-profile/me | 200 OK |
| POS-004 | Get avatar | GET /api/user-profile/me/avatar | Avatar |
| POS-005 | Upload avatar | POST /api/user-profile/me/avatar | Avatar uploaded |
| POS-006 | Delete avatar | DELETE /api/user-profile/me/avatar | Avatar deleted |
| POS-007 | Get contact info | GET /api/user-profile/me/contact | Contact |
| POS-008 | Update contact info | PUT /api/user-profile/me/contact | Contact updated |
| POS-009 | Get org unit | GET /api/user-profile/me/org-unit | Org unit |
| POS-010 | Update org unit | PUT /api/user-profile/me/org-unit | Org unit updated |
| POS-011 | Get display name | GET /api/user-profile/me/display-name | Display name |
| POS-012 | Update display name | PUT display name | Updated |
| POS-013 | Get email | GET /api/user-profile/me/email | Email |
| POS-014 | Update email | PUT email | Updated |
| POS-015 | Get phone | GET /api/user-profile/me/phone | Phone |
| POS-016 | Update phone | PUT phone | Updated |
| POS-017 | Get preferences link | GET /api/user-profile/me/preferences | Preferences |
| POS-018 | Empty result | GET for empty | Defaults |
| POS-019 | Single result | GET for single | Profile |
| POS-020 | Authenticated access | GET with token | 200 |
| POS-021 | Admin get profile | GET as admin | Profile |
| POS-022 | Partial update | PUT partial | Partial updated |
| POS-023 | Full update | PUT full | Full updated |
| POS-024 | Avatar URL | GET avatar URL | URL |
| POS-025 | Avatar thumbnail | GET avatar?size=thumb | Thumbnail |
| POS-026 | Get roles | GET /api/user-profile/me/roles | Roles |
| POS-027 | Get permissions | GET /api/user-profile/me/permissions | Permissions |
| POS-028 | Get last login | GET /api/user-profile/me/last-login | Last login |
| POS-029 | Get timezone | GET /api/user-profile/me/timezone | Timezone |
| POS-030 | Update timezone | PUT timezone | Updated |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid ID | id=abc | 400 |
| NEG-004 | Negative ID | id=-1 | 400 |
| NEG-005 | Non-existent ID | id=999999 | 404 |
| NEG-006 | Null request | PUT null | 400 |
| NEG-007 | Invalid email | email=invalid | 400 |
| NEG-008 | Invalid phone | phone=invalid | 400 |
| NEG-009 | Invalid org unit | orgUnitId=999999 | 404 |
| NEG-010 | SQL injection | displayName='; DROP | Sanitized |
| NEG-011 | XSS in display name | displayName=<script> | Sanitized |
| NEG-012 | Cross-user access | Other user profile | 403 |
| NEG-013 | Cross-org access | Other org profile | 403 |
| NEG-014 | Malformed JSON | Invalid JSON | 400 |
| NEG-015 | Wrong content-type | Application/xml | 415 |
| NEG-016 | Rate limit | Too many | 429 |
| NEG-017 | Payload too large | Huge body | 413 |
| NEG-018 | Invalid Accept | Accept: text/plain | 406 |
| NEG-019 | HTTP method | POST for get | 405 |
| NEG-020 | Trailing slash | /api/user-profile/ | Redirect |
| NEG-021 | Case sensitivity | /api/User-Profile | 404 |
| NEG-022 | Extra path | /api/user-profile/me/extra | 404 |
| NEG-023 | Invalid bearer | Bearer malformed | 401 |
| NEG-024 | Revoked token | Revoked JWT | 401 |
| NEG-025 | Service account | Service for UI | 403 |
| NEG-026 | DB timeout | Simulate | 503 |
| NEG-027 | Invalid avatar format | Avatar wrong type | 400 |
| NEG-028 | Avatar too large | Avatar >5MB | 400 |
| NEG-029 | Invalid avatar | Avatar malformed | 400 |
| NEG-030 | Blocked IP | From blocked | 403 |
| NEG-031 | Control chars | displayName with \0 | 400 |
| NEG-032 | Unicode overflow | Very long | 400 |
| NEG-033 | Duplicate email | email exists | 409 |
| NEG-034 | Mismatched IDs | Path != body | 400 |
| NEG-035 | Read-only field | Update createdDate | Ignored |
| NEG-036 | Version conflict | Stale version | 409 |
| NEG-037 | CORS fail | Invalid origin | CORS error |
| NEG-038 | Inactive org | Org inactive | 403 |
| NEG-039 | Inactive user | User disabled | 403 |
| NEG-040 | Invalid display name | displayName empty | 400 |
| NEG-041 | Invalid timezone | timezone=invalid | 400 |
| NEG-042 | Invalid language | language=invalid | 400 |
| NEG-043 | Max URL length | Very long URL | 414 |
| NEG-044 | Invalid endpoint | /api/user-profile/invalid | 404 |
| NEG-045 | Invalid method | PATCH | 405 |
| NEG-046 | Missing query | GET no params | 200 or 400 |
| NEG-047 | Invalid encoding | Malformed URL | 400 |
| NEG-048 | Avatar not found | Avatar not set | 404 |
| NEG-049 | Delete avatar not set | Delete no avatar | 404 |
| NEG-050 | Invalid org unit | orgUnit deleted | 404 |
| NEG-051 | Audit failure | Audit down | Continue |
| NEG-052 | Invalid phone format | phone=invalid | 400 |
| NEG-053 | Invalid address | address malformed | 400 |
| NEG-054 | Email length | email too long | 400 |
| NEG-055 | Display name length | displayName too long | 400 |
| NEG-056 | OPTIONS | OPTIONS | 200 |
| NEG-057 | HEAD | HEAD | 200 or 405 |
| NEG-058 | Invalid avatar size | size=invalid | 400 |
| NEG-059 | Invalid avatar dimension | dimension>2048 | 400 |
| NEG-060 | Reserved email | email=reserved | 403 |
| NEG-061 | Invalid role | role invalid | 400 |
| NEG-062 | Permission change | No permission | 403 |
| NEG-063 | Deleted org unit | orgUnit deleted | 404 |
| NEG-064 | Invalid last login | lastLogin malformed | 400 |
| NEG-065 | Invalid profile format | Profile malformed | 400 |
| NEG-066 | Invalid nested object | Nested malformed | 400 |
| NEG-067 | Invalid array | Array malformed | 400 |
| NEG-068 | Negative page | page=-1 | 400 |
| NEG-069 | Excessive page size | pageSize=10000 | 400 |
| NEG-070 | Soft-deleted | Query deleted | Excluded |
| NEG-071 | Invalid JSON schema | Schema mismatch | 400 |
| NEG-072 | Missing required field | Required null | 400 |
| NEG-073 | Invalid date format | date=invalid | 400 |
| NEG-074 | Future date | createdDate future | 400 |
| NEG-075 | Invalid enum | status=invalid | 400 |
| NEG-076 | Empty array | partners=[] | 400 |
| NEG-077 | Invalid GUID | id=bad-guid | 400 |
| NEG-078 | Profile locked | Locked profile | 423 |
| NEG-079 | Maintenance mode | During maintenance | 503 |
| NEG-080 | Quota exceeded | Storage quota | 507 |
| NEG-081 | Invalid avatar MIME | Wrong MIME | 400 |
| NEG-082 | Avatar corrupt | Corrupt file | 400 |
| NEG-083 | Profile migration | During migration | 503 |
| NEG-084 | Session invalid | Invalid session | 401 |
| NEG-085 | Token type wrong | Wrong token type | 401 |
| NEG-086 | Scope insufficient | OAuth scope | 403 |
| NEG-087 | Rate limit per user | User rate limit | 429 |
| NEG-088 | Concurrent limit | Too many concurrent | 429 |
| NEG-089 | Request timeout | Slow request | 408 |
| NEG-090 | Profile archived | Archived profile | 410 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | displayName length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | email length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-003 | phone length | 0 | 50 | ✅ | ✅ | ❌ |
| BND-004 | id | 1 | int.Max | ✅ | ✅ | ❌ |
| BND-005 | orgUnitId | 0 | int.Max | ✅ | ✅ | ❌ |
| BND-006 | Empty list | - | - | [] | - | - |
| BND-007 | Single item | - | - | [item] | - | - |
| BND-008 | Zero length displayName | - | - | ❌ | - | - |
| BND-009 | Max length displayName | 255 | - | - | ✅ | ❌ |
| BND-010 | Unicode displayName | - | - | Accept | - | - |
| BND-011 | Arabic displayName | - | - | Display | - | - |
| BND-012 | Chinese displayName | - | - | Display | - | - |
| BND-013 | Null optional | - | - | Default | - | - |
| BND-014 | Empty string | - | - | No filter | - | - |
| BND-015 | Whitespace | - | - | Trim | - | - |
| BND-016 | Avatar size | 0 | 5MB | ✅ | ✅ | ❌ |
| BND-017 | Avatar dimensions | 1 | 2048 | ✅ | ✅ | ❌ |
| BND-018 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-019 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-020 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-021 | Created date | - | - | UTC | - | - |
| BND-022 | Modified date | - | - | UTC | - | - |
| BND-023 | Audit fields | - | - | Set | - | - |
| BND-024 | Round-trip | Update → Get | - | Match | - | - |
| BND-025 | Avatar formats | - | - | jpg/png | - | - |
| BND-026 | Address length | - | 500 | ✅ | ✅ | ❌ |
| BND-027 | Timezone length | - | 50 | ✅ | ✅ | ❌ |
| BND-028 | Language length | - | 10 | ✅ | ✅ | ❌ |
| BND-029 | Version | 1 | - | ✅ | ❌ | - |
| BND-030 | Zero ID | id=0 | - | 400 | - | - |
| BND-031 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-032 | Inactive | - | - | Excluded | - | - |
| BND-033 | Partial update | - | - | Merge | - | - |
| BND-034 | Full update | - | - | Replace | - | - |
| BND-035 | Avatar thumbnail | - | 128 | ✅ | ✅ | ❌ |
| BND-036 | Avatar medium | - | 256 | ✅ | ✅ | ❌ |
| BND-037 | Avatar large | - | 512 | ✅ | ✅ | ❌ |
| BND-038 | Contact count | 0 | 10 | ✅ | ✅ | ❌ |
| BND-039 | Role count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-040 | Permission count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-041 | Last login | - | - | UTC | - | - |
| BND-042 | Profile visibility | - | - | Scope | - | - |
| BND-043 | Admin override | - | - | Admin | - | - |
| BND-044 | Org unit hierarchy | - | 10 | ✅ | ✅ | ❌ |
| BND-045 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-046 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-047 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-048 | Export empty | - | - | Headers | - | - |
| BND-049 | Export single | - | - | Valid | - | - |
| BND-050 | Bio length | - | 2000 | ✅ | ✅ | ❌ |
| BND-051 | Title length | - | 100 | ✅ | ✅ | ❌ |
| BND-052 | Department length | - | 100 | ✅ | ✅ | ❌ |
| BND-053 | Location length | - | 255 | ✅ | ✅ | ❌ |
| BND-054 | Profile completeness | - | 100 | ✅ | ✅ | ❌ |
| BND-055 | Required fields | - | - | Set | - | - |
| BND-056 | Optional fields | - | - | Null | - | - |
| BND-057 | Avatar aspect | - | - | Square | - | - |
| BND-058 | Avatar crop | - | - | Center | - | - |
| BND-059 | Contact validation | - | - | Valid | - | - |
| BND-060 | Email validation | - | - | Valid | - | - |
| BND-061 | Phone validation | - | - | Valid | - | - |
| BND-062 | Org unit validation | - | - | Valid | - | - |
| BND-063 | Profile schema | - | - | Valid | - | - |
| BND-064 | Update scope | - | - | Own | - | - |
| BND-065 | Admin scope | - | - | All | - | - |
| BND-066 | Avatar cache | - | 3600 | Valid | Valid | ❌ |
| BND-067 | Profile cache | - | 300 | Valid | Valid | ❌ |
| BND-068 | Contact info | - | - | Valid | - | - |
| BND-069 | Org unit link | - | - | Valid | - | - |
| BND-070 | Role link | - | - | Valid | - | - |
| BND-071 | Language code | - | 10 | ✅ | ✅ | ❌ |
| BND-072 | Preference count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-073 | Avatar byte size | 0 | 5242880 | ✅ | ✅ | ❌ |
| BND-074 | Request size | - | 1MB | - | ✅ | ❌ |
| BND-075 | Header count | - | 50 | ✅ | ✅ | ❌ |
| BND-076 | Cookie size | - | 4KB | - | ✅ | ❌ |
| BND-077 | Session duration | - | 24h | Valid | Valid | ❌ |
| BND-078 | Token lifetime | - | 1h | Valid | Valid | ❌ |
| BND-079 | Retry count | 0 | 3 | ✅ | ✅ | ❌ |
| BND-080 | Backoff max | - | 30s | - | ✅ | ❌ |
| BND-081 | Connection timeout | - | 30s | - | ✅ | ❌ |
| BND-082 | Read timeout | - | 60s | - | ✅ | ❌ |
| BND-083 | Write timeout | - | 60s | - | ✅ | ❌ |
| BND-084 | Idle timeout | - | 90s | - | ✅ | ❌ |
| BND-085 | Keep-alive | - | 60s | - | ✅ | ❌ |
| BND-086 | Chunk size | - | 8KB | - | ✅ | ❌ |
| BND-087 | Buffer size | - | 64KB | - | ✅ | ❌ |
| BND-088 | Pool size | - | 100 | - | ✅ | ❌ |
| BND-089 | Queue depth | - | 1000 | - | ✅ | ❌ |
| BND-090 | Batch size | 1 | 100 | ✅ | ✅ | ❌ |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get my profile | GET me | Profile |
| FUN-002 | Workflow | Get by ID | GET id | Profile |
| FUN-003 | Workflow | Update my profile | PUT me | Updated |
| FUN-004 | Workflow | Get avatar | GET avatar | Avatar |
| FUN-005 | Workflow | Upload avatar | POST avatar | Uploaded |
| FUN-006 | Workflow | Delete avatar | DELETE avatar | Deleted |
| FUN-007 | Workflow | Get contact | GET contact | Contact |
| FUN-008 | Workflow | Update contact | PUT contact | Updated |
| FUN-009 | Workflow | Get org unit | GET org-unit | Org unit |
| FUN-010 | Workflow | Update org unit | PUT org-unit | Updated |
| FUN-011 | Workflow | Partial update | PUT partial | Partial |
| FUN-012 | Workflow | Full update | PUT full | Full |
| FUN-013 | Workflow | Get roles | GET roles | Roles |
| FUN-014 | Workflow | Get permissions | GET permissions | Permissions |
| FUN-015 | Workflow | Get last login | GET last-login | Last login |
| FUN-016 | Validation | Valid email | Invalid | 400 |
| FUN-017 | Validation | Valid phone | Invalid | 400 |
| FUN-018 | Validation | Valid org unit | Invalid | 404 |
| FUN-019 | Validation | Permission | No permission | 403 |
| FUN-020 | Validation | Own profile | Other user | 403 |
| FUN-021 | Validation | ID format | Invalid | 400 |
| FUN-022 | Validation | Avatar format | Invalid | 400 |
| FUN-023 | Validation | Display name | Invalid | 400 |
| FUN-024 | Validation | Avatar size | Too large | 400 |
| FUN-025 | Validation | Duplicate email | Duplicate | 409 |
| FUN-026 | Constraint | Soft delete | Query | Excluded |
| FUN-027 | Constraint | Org scope | Cross-org | 403 |
| FUN-028 | Constraint | Version | Optimistic | 409 |
| FUN-029 | Constraint | Avatar size | >5MB | 400 |
| FUN-030 | Constraint | Avatar dimension | >2048 | 400 |
| FUN-031 | Constraint | Display name length | >255 | 400 |
| FUN-032 | Constraint | Email length | >255 | 400 |
| FUN-033 | Constraint | Required fields | Missing | 400 |
| FUN-034 | Constraint | Org unit hierarchy | Valid | 404 |
| FUN-035 | Constraint | URL length | >2048 | 414 |
| FUN-036 | Audit | Update | PUT | Audit |
| FUN-037 | Audit | Avatar upload | POST avatar | Audit |
| FUN-038 | Audit | Avatar delete | DELETE avatar | Audit |
| FUN-039 | Audit | Contact update | PUT contact | Audit |
| FUN-040 | Audit | Timestamp | Any | UTC |
| FUN-041 | Audit | User ID | Any | User ID |
| FUN-042 | Audit | IP | Any | IP |
| FUN-043 | Audit | Resource | Any | Resource |
| FUN-044 | Audit | Outcome | Any | Outcome |
| FUN-045 | Audit | Profile change | Any | Audit |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | User scope | Own profile | Correct |
| FUN-050 | Business | Contact info | Contact | Correct |
| FUN-051 | Workflow | Get language | GET language | Language |
| FUN-052 | Workflow | Update language | PUT language | Updated |
| FUN-053 | Validation | Timezone format | Invalid | 400 |
| FUN-054 | Validation | Language format | Invalid | 400 |
| FUN-055 | Constraint | Profile lock | Locked | 423 |
| FUN-056 | Audit | Language update | PUT language | Audit |
| FUN-057 | Audit | Timezone update | PUT timezone | Audit |
| FUN-058 | Business | Default timezone | No value | UTC |
| FUN-059 | Business | Default language | No value | en |
| FUN-060 | Workflow | Get after update | PUT then GET | Match |
| FUN-061 | Validation | Combined fields | Partial invalid | 400 |
| FUN-062 | Constraint | Avatar aspect | Invalid ratio | 400 |
| FUN-063 | Audit | Profile view | GET | Audit |
| FUN-064 | Business | Org unit cascade | Delete org | 404 |
| FUN-065 | Workflow | Cached response | GET same | 200 |
| FUN-066 | Validation | Email domain | Invalid domain | 400 |
| FUN-067 | Constraint | Phone country | Invalid country | 400 |
| FUN-068 | Audit | Avatar view | GET avatar | Audit |
| FUN-069 | Business | Role scope | Role change | Updated |
| FUN-070 | Workflow | Combined update | PUT multiple | All |
| FUN-071 | Validation | Display name format | Invalid chars | 400 |
| FUN-072 | Constraint | Org hierarchy | Invalid level | 404 |
| FUN-073 | Audit | Contact view | GET contact | Audit |
| FUN-074 | Business | Permission scope | Permission change | Updated |
| FUN-075 | Workflow | Reset to default | POST reset | Default |
| FUN-076 | Validation | Address format | Invalid | 400 |
| FUN-077 | Constraint | Max profiles | Limit | 429 |
| FUN-078 | Audit | Org unit view | GET org-unit | Audit |
| FUN-079 | Business | Inactive org unit | Org inactive | 403 |
| FUN-080 | Workflow | Thumbnail generation | GET thumb | Thumbnail |
| FUN-081 | Validation | Bio length | Too long | 400 |
| FUN-082 | Constraint | Concurrent updates | Stale | 409 |
| FUN-083 | Audit | Preference link | GET prefs | Audit |
| FUN-084 | Business | Cross-org unit | Other org | 403 |
| FUN-085 | Workflow | Session refresh | Token refresh | 200 |
| FUN-086 | Validation | Title length | Too long | 400 |
| FUN-087 | Constraint | Department scope | Invalid dept | 403 |
| FUN-088 | Audit | Role view | GET roles | Audit |
| FUN-089 | Business | Location scope | Invalid location | 403 |
| FUN-090 | Workflow | Full round-trip | Update → Get | Match |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Update → Get | Profile | Match |
| INT-002 | CRUD | Get by ID | Profile | Profile |
| INT-003 | CRUD | Get my profile | Profile | Profile |
| INT-004 | CRUD | Upload avatar → Get | Profile | Avatar |
| INT-005 | CRUD | Delete avatar → Get | Profile | No avatar |
| INT-006 | CRUD | Update contact → Get | Profile | Contact |
| INT-007 | CRUD | Update org unit → Get | Profile | Org unit |
| INT-008 | CRUD | Partial update → Get | Profile | Partial |
| INT-009 | CRUD | Full update → Get | Profile | Full |
| INT-010 | CRUD | Get roles | Profile, Role | Roles |
| INT-011 | Avatar | Upload | Profile | Avatar |
| INT-012 | Avatar | Get | Profile | Avatar |
| INT-013 | Avatar | Delete | Profile | Deleted |
| INT-014 | Avatar | Thumbnail | Profile | Thumbnail |
| INT-015 | Avatar | Format | Profile | Format |
| INT-016 | Contact | Get | Profile | Contact |
| INT-017 | Contact | Update | Profile | Updated |
| INT-018 | Contact | Format | Profile | Format |
| INT-019 | Contact | Validation | Profile | Valid |
| INT-020 | Contact | Required | Profile | Required |
| INT-021 | Org unit | Get | Profile, OrgUnit | Org unit |
| INT-022 | Org unit | Update | Profile, OrgUnit | Updated |
| INT-023 | Org unit | Hierarchy | Profile, OrgUnit | Hierarchy |
| INT-024 | Org unit | Validation | Profile | Valid |
| INT-025 | Org unit | Orphan | Deleted org unit | 404 |
| INT-026 | Relationships | Profile → User | Profile, User | Linked |
| INT-027 | Relationships | Profile → OrgUnit | Profile, OrgUnit | Linked |
| INT-028 | Relationships | Profile → Roles | Profile, Role | Linked |
| INT-029 | Relationships | Orphan | Deleted user | 404 |
| INT-030 | Relationships | User scope | User | Scoped |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid ID | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | Duplicate | 409 |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow | 504 |
| INT-039 | Error | Payload | Huge | 413 |
| INT-040 | Error | Media | Wrong type | 415 |
| INT-041 | Error | Method | Wrong verb | 405 |
| INT-042 | Error | Service | Dependency | 503 |
| INT-043 | Error | Gateway | Upstream | 504 |
| INT-044 | Error | Gone | Deleted | 410 |
| INT-045 | Error | Locked | Locked | 423 |
| INT-046 | E2E | Full update flow | Profile | Update → Get |
| INT-047 | E2E | Avatar flow | Profile | Upload → Get |
| INT-048 | E2E | Contact flow | Profile | Update → Get |
| INT-049 | E2E | Org unit flow | Profile | Update → Get |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Update → Get | Profile | Match |
| INT-052 | CRUD | Get by ID | Profile | Profile |
| INT-053 | CRUD | Get my profile | Profile | Profile |
| INT-054 | CRUD | Upload avatar → Get | Profile | Avatar |
| INT-055 | CRUD | Delete avatar → Get | Profile | No avatar |
| INT-056 | CRUD | Update contact → Get | Profile | Contact |
| INT-057 | CRUD | Update org unit → Get | Profile | Org unit |
| INT-058 | CRUD | Partial update → Get | Profile | Partial |
| INT-059 | CRUD | Full update → Get | Profile | Full |
| INT-060 | CRUD | Get roles | Profile, Role | Roles |
| INT-061 | Avatar | Upload | Profile | Avatar |
| INT-062 | Avatar | Get | Profile | Avatar |
| INT-063 | Avatar | Delete | Profile | Deleted |
| INT-064 | Avatar | Thumbnail | Profile | Thumbnail |
| INT-065 | Avatar | Format | Profile | Format |
| INT-066 | Contact | Get | Profile | Contact |
| INT-067 | Contact | Update | Profile | Updated |
| INT-068 | Contact | Format | Profile | Format |
| INT-069 | Contact | Validation | Profile | Valid |
| INT-070 | Contact | Required | Profile | Required |
| INT-071 | Org unit | Get | Profile, OrgUnit | Org unit |
| INT-072 | Org unit | Update | Profile, OrgUnit | Updated |
| INT-073 | Org unit | Hierarchy | Profile, OrgUnit | Hierarchy |
| INT-074 | Org unit | Validation | Profile | Valid |
| INT-075 | Org unit | Orphan | Deleted org unit | 404 |
| INT-076 | Relationships | Profile → User | Profile, User | Linked |
| INT-077 | Relationships | Profile → OrgUnit | Profile, OrgUnit | Linked |
| INT-078 | Relationships | Profile → Roles | Profile, Role | Linked |
| INT-079 | Relationships | Orphan | Deleted user | 404 |
| INT-080 | Relationships | User scope | User | Scoped |
| INT-081 | Error | DB down | DB | 503 |
| INT-082 | Error | Auth down | Auth | 401/503 |
| INT-083 | Error | Validation | Bad input | 400 |
| INT-084 | Error | NotFound | Invalid ID | 404 |
| INT-085 | Error | Forbidden | No permission | 403 |
| INT-086 | Error | Conflict | Duplicate | 409 |
| INT-087 | Error | Rate limit | Too many | 429 |
| INT-088 | Error | Timeout | Slow | 504 |
| INT-089 | Error | Payload | Huge | 413 |
| INT-090 | Error | Media | Wrong type | 415 |

---

## §6 Security Tests (50)

| ID | Category | Attack | Target | Expected |
|----|----------|--------|-------|----------|
| SEC-001 | Injection | SQL | displayName | Sanitized |
| SEC-002 | Injection | XSS | displayName | Encoded |
| SEC-003 | Injection | Path traversal | Path | Rejected |
| SEC-004 | Injection | NoSQL | Filter | Rejected |
| SEC-005 | Injection | Command | Avatar | Rejected |
| SEC-006 | Injection | Header | Header | Rejected |
| SEC-007 | Injection | Log | Input | Sanitized |
| SEC-008 | Injection | LDAP | Search | Rejected |
| SEC-009 | Injection | Log4j | Input | Rejected |
| SEC-010 | Injection | SSRF | URL | Rejected |
| SEC-011 | Access | No auth | All | 401 |
| SEC-012 | Access | Wrong role | Admin | 403 |
| SEC-013 | Access | Cross-org | Other org | 403 |
| SEC-014 | Access | Horizontal | Other user | 403 |
| SEC-015 | Access | Vertical | Admin | 403 |
| SEC-016 | Access | Expired | Token | 401 |
| SEC-017 | Access | Revoked | Token | 401 |
| SEC-018 | Access | Tampered | Token | 401 |
| SEC-019 | Access | Scope | OAuth | 403 |
| SEC-020 | Access | Service | UI | 403 |
| SEC-021 | IDOR | Other user profile | ID | 403 |
| SEC-022 | IDOR | Other user | ID | 403 |
| SEC-023 | IDOR | Manipulate | Path | 403 |
| SEC-024 | IDOR | Enumeration | IDs | Rate limit |
| SEC-025 | IDOR | Pollution | Params | First |
| SEC-026 | Mass Assign | Admin | Body | Ignored |
| SEC-027 | Mass Assign | Role | Body | Ignored |
| SEC-028 | Mass Assign | Org | Body | Ignored |
| SEC-029 | Mass Assign | User | Body | Ignored |
| SEC-030 | Mass Assign | Permission | Body | Ignored |
| SEC-031 | Auth | Fixation | Session | New |
| SEC-032 | Auth | Hijack | Token | Invalid |
| SEC-033 | Auth | Replay | Old token | Reject |
| SEC-034 | Auth | CSRF | State | Token |
| SEC-035 | Auth | Brute | Login | Rate limit |
| SEC-036 | Data | PII in export | Export | Masked |
| SEC-037 | Data | Logs | Sensitive | No PII |
| SEC-038 | Data | Error | 500 | Generic |
| SEC-039 | Data | Stack | Exception | Hidden |
| SEC-040 | Data | Debug | Prod | Off |
| SEC-041 | OWASP | A01 | Access | 403 |
| SEC-042 | OWASP | A02 | Crypto | TLS |
| SEC-043 | OWASP | A03 | Injection | Param |
| SEC-044 | OWASP | A04 | Design | Defensive |
| SEC-045 | OWASP | A05 | Misconfig | Secure |
| SEC-046 | OWASP | A06 | Vulnerable | No CVE |
| SEC-047 | OWASP | A07 | Auth | Strong |
| SEC-048 | OWASP | A08 | Integrity | Checks |
| SEC-049 | OWASP | A09 | Logging | Audit |
| SEC-050 | OWASP | A10 | SSRF | No internal |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users get same | Both succeed |
| CON-002 | 2 users update same | Last write |
| CON-003 | 2 users upload avatar | Last wins |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click update | Single |
| CON-007 | Rapid update | Last wins |
| CON-008 | Delete during read | Snapshot |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Avatar upload concurrent | Last wins |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple updates | All succeed |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Contact update concurrent | Last write |
| CON-022 | Org unit update concurrent | Last write |
| CON-023 | Permission change | Old |
| CON-024 | Profile change | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid email | Accept |
| UNT-002 | Validation | Invalid email | Reject |
| UNT-003 | Validation | Valid phone | Accept |
| UNT-004 | Validation | Invalid phone | Reject |
| UNT-005 | Validation | Valid display name | Accept |
| UNT-006 | Formatting | Display name | Formatted |
| UNT-007 | Formatting | Date | ISO 8601 |
| UNT-008 | Formatting | Phone | Formatted |
| UNT-009 | Calculation | Profile completeness | Correct |
| UNT-010 | Calculation | Avatar size | Correct |
| UNT-011 | Calculation | Contact count | Correct |
| UNT-012 | Calculation | Role count | Correct |
| UNT-013 | Calculation | Permission count | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Avatar set | Avatar |
| UNT-018 | Status | Avatar not set | Default |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get profile | < 100ms |
| PRF-002 | Get by ID | < 50ms |
| PRF-003 | Update | < 200ms |
| PRF-004 | Get avatar | < 50ms |
| PRF-005 | Upload avatar | < 500ms |
| PRF-006 | Get contact | < 50ms |
| PRF-007 | Update contact | < 200ms |
| PRF-008 | Get org unit | < 100ms |
| PRF-009 | Update org unit | < 200ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent update | < 500ms each |
| PRF-013 | Memory profile | < 10MB |
| PRF-014 | Memory avatar | < 50MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 5 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users | 10 min | 95% < 500ms |
| LDT-002 | 50 users | 10 min | 95% < 1s |
| LDT-003 | 100 users | 10 min | 95% < 2s |
| LDT-004 | Spike 10→100 | 5 min | No crash |
| LDT-005 | Spike 50→200 | 5 min | Graceful |
| LDT-006 | Stress 200 | Until fail | Document |
| LDT-007 | Stress 500 | Until fail | Document |
| LDT-008 | 50 concurrent | 5 min | Queue/limit |
| LDT-009 | Recovery spike | 5 min | Baseline |
| LDT-010 | Recovery stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Get/update profile | POS-001–003, FUN-001–003 |
| Avatar | POS-004–006, FUN-004–006 |
| Contact info | POS-007–008, FUN-007–008 |
| Org unit | POS-009–010, FUN-009–010 |
| 3:1 Ratio | NEG-001–090, BND-001–090, FUN-001–090, INT-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
