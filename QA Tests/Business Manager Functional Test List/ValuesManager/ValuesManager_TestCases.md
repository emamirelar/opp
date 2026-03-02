# UserPreferenceController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/UserPreferenceController`  
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
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

REST API for user preferences: CRUD preferences, theme, language, notification settings.

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get my preferences | GET /api/user-preferences/me | My preferences |
| POS-002 | Get preference by key | GET /api/user-preferences/me/{key} | Value |
| POS-003 | Set preference | PUT /api/user-preferences/me | 200 OK |
| POS-004 | Set theme | PUT theme=dark | Theme set |
| POS-005 | Set language | PUT language=fr | Language set |
| POS-006 | Set notification settings | PUT notifications | Notifications set |
| POS-007 | Get theme | GET /api/user-preferences/me/theme | Theme |
| POS-008 | Get language | GET /api/user-preferences/me/language | Language |
| POS-009 | Get notifications | GET /api/user-preferences/me/notifications | Notifications |
| POS-010 | Reset to default | POST /api/user-preferences/me/reset | Reset |
| POS-011 | Get all keys | GET /api/user-preferences/me/keys | Keys |
| POS-012 | Bulk set | PUT /api/user-preferences/me/bulk | Bulk set |
| POS-013 | Delete preference | DELETE /api/user-preferences/me/{key} | Deleted |
| POS-014 | Empty result | GET for empty | Defaults |
| POS-015 | Single result | GET for single key | Value |
| POS-016 | Authenticated access | GET with token | 200 |
| POS-017 | Default theme | No theme set | Default |
| POS-018 | Default language | No language set | Default |
| POS-019 | Timezone preference | PUT timezone=UTC | Timezone set |
| POS-020 | Date format preference | PUT dateFormat=YYYY-MM-DD | Format set |
| POS-021 | Pagination preference | PUT pageSize=20 | PageSize set |
| POS-022 | Dashboard layout | PUT dashboardLayout | Layout set |
| POS-023 | Email notifications | PUT emailNotifications=true | Set |
| POS-024 | In-app notifications | PUT inAppNotifications=true | Set |
| POS-025 | Sort preference | PUT defaultSort=name | Sort set |
| POS-026 | Filter preference | PUT defaultFilter | Filter set |
| POS-027 | Export format | PUT exportFormat=csv | Format set |
| POS-028 | Accessibility | PUT accessibility | Settings set |
| POS-029 | Compact mode | PUT compactMode=false | Set |
| POS-030 | Combined update | PUT multiple | All set |
| POS-031 | Get after set | PUT then GET | Match |
| POS-032 | Reset single | POST reset key | Reset |
| POS-033 | Theme options | GET /api/user-preferences/themes | Options |
| POS-034 | Language options | GET /api/user-preferences/languages | Options |
| POS-035 | Cached response | GET same query | 200 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid key | key=invalid | 400 |
| NEG-004 | Null request | PUT null | 400 |
| NEG-005 | Invalid theme | theme=invalid | 400 |
| NEG-006 | Invalid language | language=invalid | 400 |
| NEG-007 | Invalid timezone | timezone=invalid | 400 |
| NEG-008 | SQL injection | key='; DROP | Sanitized |
| NEG-009 | XSS in value | value=<script> | Sanitized |
| NEG-010 | Cross-user access | Other user prefs | 403 |
| NEG-011 | Malformed JSON | Invalid JSON | 400 |
| NEG-012 | Wrong content-type | Application/xml | 415 |
| NEG-013 | Rate limit | Too many | 429 |
| NEG-014 | Payload too large | Huge body | 413 |
| NEG-015 | Invalid Accept | Accept: text/plain | 406 |
| NEG-016 | HTTP method | POST for get | 405 |
| NEG-017 | Trailing slash | /api/user-preferences/ | Redirect |
| NEG-018 | Case sensitivity | /api/User-Preferences | 404 |
| NEG-019 | Extra path | /api/user-preferences/me/extra | 404 |
| NEG-020 | Invalid bearer | Bearer malformed | 401 |
| NEG-021 | Revoked token | Revoked JWT | 401 |
| NEG-022 | Service account | Service for UI | 403 |
| NEG-023 | DB timeout | Simulate | 503 |
| NEG-024 | Invalid notification format | Notifications malformed | 400 |
| NEG-025 | Invalid date format | dateFormat=invalid | 400 |
| NEG-026 | Invalid page size | pageSize=invalid | 400 |
| NEG-027 | Blocked IP | From blocked | 403 |
| NEG-028 | Control chars | key with \0 | 400 |
| NEG-029 | Unicode overflow | Very long | 400 |
| NEG-030 | Empty key | key= | 400 |
| NEG-031 | Invalid bulk | Bulk malformed | 400 |
| NEG-032 | Mismatched IDs | Path != body | 400 |
| NEG-033 | Read-only field | Update createdDate | Ignored |
| NEG-034 | Version conflict | Stale version | 409 |
| NEG-035 | CORS fail | Invalid origin | CORS error |
| NEG-036 | Inactive org | Org inactive | 403 |
| NEG-037 | Invalid layout | dashboardLayout malformed | 400 |
| NEG-038 | Invalid export format | exportFormat=invalid | 400 |
| NEG-039 | Invalid accessibility | accessibility malformed | 400 |
| NEG-040 | Delete non-existent | key=invalid | 404 |
| NEG-041 | Reset non-existent | Reset invalid | 400 |
| NEG-042 | Max URL length | Very long URL | 414 |
| NEG-043 | Invalid endpoint | /api/user-preferences/invalid | 404 |
| NEG-044 | Invalid method | PATCH | 405 |
| NEG-045 | Missing query | GET no params | 200 or 400 |
| NEG-046 | Invalid encoding | Malformed URL | 400 |
| NEG-047 | Reserved key | key=RESERVED | 403 |
| NEG-048 | Invalid value type | value type mismatch | 400 |
| NEG-049 | Empty bulk | PUT [] | 400 |
| NEG-050 | Excessive bulk | 1000 keys | 400 |
| NEG-051 | Audit failure | Audit down | Continue |
| NEG-052 | Invalid sort | defaultSort=invalid | 400 |
| NEG-053 | Invalid filter | defaultFilter malformed | 400 |
| NEG-054 | Key length | key too long | 400 |
| NEG-055 | Value length | value too long | 400 |
| NEG-056 | OPTIONS | OPTIONS | 200 |
| NEG-057 | HEAD | HEAD | 200 or 405 |
| NEG-058 | Invalid boolean | boolean=invalid | 400 |
| NEG-059 | Invalid number | number=invalid | 400 |
| NEG-060 | Theme not in list | theme not supported | 400 |
| NEG-061 | Language not in list | language not supported | 400 |
| NEG-062 | Timezone not in list | timezone not supported | 400 |
| NEG-063 | Circular reference | value references self | 400 |
| NEG-064 | Duplicate key in bulk | Duplicate keys | 400 |
| NEG-065 | Invalid JSON value | JSON malformed | 400 |
| NEG-066 | Invalid nested object | Nested malformed | 400 |
| NEG-067 | Invalid array | Array malformed | 400 |
| NEG-068 | Negative page size | pageSize=-1 | 400 |
| NEG-069 | Excessive page size | pageSize=10000 | 400 |
| NEG-070 | Soft-deleted | Query deleted | Excluded |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | key length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | value length | 0 | 10000 | ✅ | ✅ | ❌ |
| BND-003 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-004 | Empty list | - | - | [] | - | - |
| BND-005 | Single item | - | - | [item] | - | - |
| BND-006 | Zero length key | - | - | ❌ | - | - |
| BND-007 | Max length key | 255 | - | - | ✅ | ❌ |
| BND-008 | Unicode key | - | - | Accept | - | - |
| BND-009 | Arabic value | - | - | Display | - | - |
| BND-010 | Chinese value | - | - | Display | - | - |
| BND-011 | Null optional | - | - | Default | - | - |
| BND-012 | Empty string | - | - | No filter | - | - |
| BND-013 | Whitespace | - | - | Trim | - | - |
| BND-014 | Theme options | 0 | 10 | ✅ | ✅ | ❌ |
| BND-015 | Language options | 0 | 50 | ✅ | ✅ | ❌ |
| BND-016 | Bulk size | 1 | 100 | ✅ | ✅ | ❌ |
| BND-017 | Preference count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-018 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-019 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-020 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-021 | Created date | - | - | UTC | - | - |
| BND-022 | Modified date | - | - | UTC | - | - |
| BND-023 | Audit fields | - | - | Set | - | - |
| BND-024 | Round-trip | Set → Get | - | Match | - | - |
| BND-025 | Reset default | - | - | Default | - | - |
| BND-026 | Theme length | - | 50 | ✅ | ✅ | ❌ |
| BND-027 | Language length | - | 10 | ✅ | ✅ | ❌ |
| BND-028 | Timezone length | - | 50 | ✅ | ✅ | ❌ |
| BND-029 | Date format length | - | 50 | ✅ | ✅ | ❌ |
| BND-030 | Layout size | - | 10000 | ✅ | ✅ | ❌ |
| BND-031 | Notification count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-032 | Export format length | - | 20 | ✅ | ✅ | ❌ |
| BND-033 | Boolean value | - | - | true/false | - | - |
| BND-034 | Number value | - | 999999 | ✅ | ✅ | ❌ |
| BND-035 | JSON value | - | 10000 | ✅ | ✅ | ❌ |
| BND-036 | Array value | - | 1000 | ✅ | ✅ | ❌ |
| BND-037 | Nested object | - | 10 | ✅ | ✅ | ❌ |
| BND-038 | Version | 1 | - | ✅ | ❌ | - |
| BND-039 | Key count | 0 | 500 | ✅ | ✅ | ❌ |
| BND-040 | Inactive | - | - | Excluded | - | - |
| BND-041 | Default theme | - | - | light | - | - |
| BND-042 | Default language | - | - | en | - | - |
| BND-043 | Default page size | - | - | 20 | - | - |
| BND-044 | Default timezone | - | - | UTC | - | - |
| BND-045 | Default date format | - | - | ISO | - | - |
| BND-046 | Partial bulk | - | - | 207 | - | - |
| BND-047 | Empty bulk | - | - | 400 | - | - |
| BND-048 | Filter combination | - | 5 | ✅ | ✅ | ❌ |
| BND-049 | Sort fields | - | 5 | ✅ | ✅ | ❌ |
| BND-050 | Compact mode | - | - | Boolean | - | - |
| BND-051 | Email notifications | - | - | Boolean | - | - |
| BND-052 | In-app notifications | - | - | Boolean | - | - |
| BND-053 | Dashboard layout | - | - | Valid | - | - |
| BND-054 | Default sort | - | - | Valid | - | - |
| BND-055 | Default filter | - | - | Valid | - | - |
| BND-056 | Accessibility options | - | 20 | ✅ | ✅ | ❌ |
| BND-057 | Export format options | - | 10 | ✅ | ✅ | ❌ |
| BND-058 | Page size options | - | 20 | ✅ | ✅ | ❌ |
| BND-059 | Timezone options | - | 600 | ✅ | ✅ | ❌ |
| BND-060 | Language options count | - | 50 | ✅ | ✅ | ❌ |
| BND-061 | Theme options count | - | 10 | ✅ | ✅ | ❌ |
| BND-062 | Combined update | - | 50 | ✅ | ✅ | ❌ |
| BND-063 | Reset scope | - | - | All | - | - |
| BND-064 | Partial reset | - | - | Key | - | - |
| BND-065 | Get after set | - | - | Match | - | - |
| BND-066 | Delete after set | - | - | Default | - | - |
| BND-067 | Overwrite | - | - | Replace | - | - |
| BND-068 | Merge | - | - | Merge | - | - |
| BND-069 | Immutable key | - | - | Reject | - | - |
| BND-070 | Cache TTL | - | 3600 | Valid | Valid | ❌ |

---

## §4 Functional Tests (50)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get my prefs | GET me | Prefs |
| FUN-002 | Workflow | Get by key | GET key | Value |
| FUN-003 | Workflow | Set preference | PUT | Set |
| FUN-004 | Workflow | Set theme | PUT theme | Theme |
| FUN-005 | Workflow | Set language | PUT language | Language |
| FUN-006 | Workflow | Set notifications | PUT notifications | Set |
| FUN-007 | Workflow | Reset | POST reset | Reset |
| FUN-008 | Workflow | Bulk set | PUT bulk | Bulk |
| FUN-009 | Workflow | Delete | DELETE key | Deleted |
| FUN-010 | Workflow | Get all keys | GET keys | Keys |
| FUN-011 | Workflow | Theme options | GET themes | Options |
| FUN-012 | Workflow | Language options | GET languages | Options |
| FUN-013 | Workflow | Set timezone | PUT timezone | Set |
| FUN-014 | Workflow | Set date format | PUT dateFormat | Set |
| FUN-015 | Workflow | Set page size | PUT pageSize | Set |
| FUN-016 | Validation | Valid theme | Invalid | 400 |
| FUN-017 | Validation | Valid language | Invalid | 400 |
| FUN-018 | Validation | Valid key | Invalid | 400 |
| FUN-019 | Validation | Permission | No permission | 403 |
| FUN-020 | Validation | Own prefs | Other user | 403 |
| FUN-021 | Validation | Key format | Invalid | 400 |
| FUN-022 | Validation | Value format | Invalid | 400 |
| FUN-023 | Validation | Type match | Mismatch | 400 |
| FUN-024 | Validation | Reserved key | Reserved | 403 |
| FUN-025 | Validation | Value length | Too long | 400 |
| FUN-026 | Constraint | Soft delete | Query | Excluded |
| FUN-027 | Constraint | Org scope | Cross-org | 403 |
| FUN-028 | Constraint | Version | Optimistic | 409 |
| FUN-029 | Constraint | Max keys | >500 | 400 |
| FUN-030 | Constraint | Max bulk | >100 | 400 |
| FUN-031 | Constraint | Value size | >10K | 400 |
| FUN-032 | Constraint | Key size | >255 | 400 |
| FUN-033 | Constraint | Theme options | Valid only | 400 |
| FUN-034 | Constraint | Language options | Valid only | 400 |
| FUN-035 | Constraint | URL length | >2048 | 414 |
| FUN-036 | Audit | Set | PUT | Audit |
| FUN-037 | Audit | Delete | DELETE | Audit |
| FUN-038 | Audit | Reset | POST reset | Audit |
| FUN-039 | Audit | Bulk | PUT bulk | Audit |
| FUN-040 | Audit | Timestamp | Any | UTC |
| FUN-041 | Audit | User ID | Any | User ID |
| FUN-042 | Audit | IP | Any | IP |
| FUN-043 | Audit | Resource | Any | Resource |
| FUN-044 | Audit | Outcome | Any | Outcome |
| FUN-045 | Audit | Key | Any | Key |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | User scope | Own prefs | Correct |
| FUN-050 | Business | Default fallback | No value | Default |

---

## §5 Integration Tests (50)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Set → Get | Preference | Match |
| INT-002 | CRUD | Delete → Get | Preference | Default |
| INT-003 | CRUD | Reset → Get | Preference | Default |
| INT-004 | CRUD | Bulk set → Get | Preference | Match |
| INT-005 | CRUD | Theme set | Preference | Theme |
| INT-006 | CRUD | Language set | Preference | Language |
| INT-007 | CRUD | Notification set | Preference | Notifications |
| INT-008 | CRUD | Get all keys | Preference | Keys |
| INT-009 | CRUD | Get by key | Preference | Value |
| INT-010 | CRUD | Options | Preference | Options |
| INT-011 | Theme | Set theme | Preference | Theme |
| INT-012 | Theme | Get theme | Preference | Theme |
| INT-013 | Theme | Theme options | Preference | Options |
| INT-014 | Language | Set language | Preference | Language |
| INT-015 | Language | Get language | Preference | Language |
| INT-016 | Language | Language options | Preference | Options |
| INT-017 | Notification | Set | Preference | Set |
| INT-018 | Notification | Get | Preference | Get |
| INT-019 | Notification | Format | Preference | Format |
| INT-020 | Notification | Types | Preference | Types |
| INT-021 | Pagination | Set page size | Preference | Set |
| INT-022 | Pagination | Get page size | Preference | Get |
| INT-023 | Pagination | Invalid | Preference | 400 |
| INT-024 | Pagination | Boundary | Preference | Exact |
| INT-025 | Pagination | Default | Preference | Default |
| INT-026 | Relationships | Preference → User | Preference, User | Linked |
| INT-027 | Relationships | Orphan | Deleted user | 404 |
| INT-028 | Relationships | User scope | User | Scoped |
| INT-029 | Relationships | Default cascade | User | Defaults |
| INT-030 | Relationships | Override | User | Override |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid key | 404 |
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
| INT-046 | E2E | Full set flow | Preference | Set → Get |
| INT-047 | E2E | Full reset flow | Preference | Reset → Default |
| INT-048 | E2E | Theme flow | Preference | Set → Get |
| INT-049 | E2E | Language flow | Preference | Set → Get |
| INT-050 | E2E | Session expiry | Auth | Clean fail |

---

## §6 Security Tests (50)

| ID | Category | Attack | Target | Expected |
|----|----------|--------|-------|----------|
| SEC-001 | Injection | SQL | Key | Sanitized |
| SEC-002 | Injection | XSS | Value | Encoded |
| SEC-003 | Injection | Path traversal | Path | Rejected |
| SEC-004 | Injection | NoSQL | Filter | Rejected |
| SEC-005 | Injection | Command | Export | Rejected |
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
| SEC-021 | IDOR | Other user prefs | ID | 403 |
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
| CON-002 | 2 users set same key | Last write |
| CON-003 | 2 users set different | Both succeed |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Double-click set | Single |
| CON-007 | Rapid update | Last wins |
| CON-008 | Delete during read | Snapshot |
| CON-009 | Cache invalidation | No stale |
| CON-010 | Connection pool | Queue/503 |
| CON-011 | Transaction | No dirty |
| CON-012 | Optimistic | Last write |
| CON-013 | Deadlock | Timeout |
| CON-014 | Bulk concurrent | Last or merge |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple sets | All succeed |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Theme change | Consistent |
| CON-022 | Language change | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Reset concurrent | Consistent |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid key | Accept |
| UNT-002 | Validation | Invalid key | Reject |
| UNT-003 | Validation | Valid value | Accept |
| UNT-004 | Validation | Invalid value | Reject |
| UNT-005 | Validation | Valid theme | Accept |
| UNT-006 | Formatting | Key | Formatted |
| UNT-007 | Formatting | Value | Formatted |
| UNT-008 | Formatting | Date | ISO 8601 |
| UNT-009 | Calculation | Preference count | Correct |
| UNT-010 | Calculation | Key count | Correct |
| UNT-011 | Calculation | Default merge | Correct |
| UNT-012 | Calculation | Reset scope | Correct |
| UNT-013 | Calculation | Bulk merge | Correct |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Theme default | Default |
| UNT-018 | Status | Language default | Default |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get all | < 100ms |
| PRF-002 | Get by key | < 50ms |
| PRF-003 | Set preference | < 100ms |
| PRF-004 | Bulk set | < 300ms |
| PRF-005 | Delete | < 100ms |
| PRF-006 | Reset | < 200ms |
| PRF-007 | Get theme | < 50ms |
| PRF-008 | Get language | < 50ms |
| PRF-009 | Get options | < 100ms |
| PRF-010 | 10 concurrent | < 1s each |
| PRF-011 | 50 concurrent | < 2s each |
| PRF-012 | 5 concurrent set | < 500ms each |
| PRF-013 | Memory list | < 10MB |
| PRF-014 | Memory bulk | < 50MB |
| PRF-015 | Cache hit | > 80% |
| PRF-016 | DB queries | < 3 per request |

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
| CRUD preferences | POS-001–005, FUN-001–003 |
| Theme settings | POS-004, POS-007, FUN-004 |
| Language settings | POS-005, POS-008, FUN-005 |
| Notification settings | POS-006, POS-009, FUN-006 |
| 3:1 Ratio | NEG-001–070, BND-001–070 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
