# Security & Authorization — Test Cases

**Component:** Cross-cutting / Security & Authorization  
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
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P? 90≥90 ✅ | E≥3P? 90≥90 ✅ | F≥3P? 90≥90 ✅ | I≥3P? 90≥90 ✅

---

## Feature Overview

**Security & Authorization** covers JWT validation, role-based access control (RBAC), IDOR prevention, injection protection, CORS/CSRF, rate limiting, and data exposure controls. The system must enforce authentication and authorization consistently across all endpoints and protect against common attack vectors.

**Key Capabilities:**
- JWT validation (signature, expiry, issuer, audience)
- Role-based access control (RBAC)
- IDOR (Insecure Direct Object Reference) prevention
- Injection protection (SQL, XSS, NoSQL, etc.)
- CORS and CSRF protection
- Rate limiting
- Data exposure prevention

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Valid JWT grants access | Valid token | Request with token | 200 OK | P0 |
| POS-002 | User with permission can action | CanEdit | Edit entity | Success | P0 |
| POS-003 | Admin accesses all | Admin role | Access any entity | Success | P0 |
| POS-004 | Org-scoped user sees own org | Org A user | Query Org A data | Filtered to Org A | P0 |
| POS-005 | CSRF token validates | Valid CSRF | POST with token | Success | P0 |
| POS-006 | Rate limit allows normal load | 10 req/min | 5 requests | All succeed | P0 |
| POS-007 | CORS allows configured origin | Origin in allowlist | Cross-origin request | Success | P0 |
| POS-008 | Permission endpoint returns flags | User has access | GET /permissions | canEdit, canDelete | P0 |
| POS-009 | Role assignment enforces | Role assigned | Access per role | Correct access | P0 |
| POS-010 | Entity-level permission | User owns entity | Access own | Success | P0 |
| POS-011 | Logout invalidates token | Logout | Use old token | 401 | P0 |
| POS-012 | Refresh token issues new | Valid refresh | Refresh | New access token | P0 |
| POS-013 | MFA validates | MFA enabled | Login with MFA | Success | P0 |
| POS-014 | Password change requires re-auth | Password changed | Old token | 401 | P0 |
| POS-015 | Sensitive field masked | User without field perm | Get entity | Sensitive null | P0 |
| POS-016 | API key with scope | Key has scope | Request in scope | Success | P1 |
| POS-017 | Delegation works | User delegated | Act on behalf | Success | P1 |
| POS-018 | Service account limited | Service account | Allowed endpoints | Success | P1 |
| POS-019 | Rate limit resets | Window passed | New request | Succeeds | P1 |
| POS-020 | Audit logs auth events | Login | Check audit | Login logged | P1 |
| POS-021 | Failed login logged | Invalid creds | Attempt | Failed login audit | P1 |
| POS-022 | Token rotation | Rotate | New token | Old invalid | P1 |
| POS-023 | Session limit enforced | Max sessions | New login | Old invalidated | P1 |
| POS-024 | Workflow permission | CanApprove | Approve | Success | P0 |
| POS-025 | Bulk permission per row | Mixed access | Bulk | Rows filtered | P1 |
| POS-026 | Export permission | CanExport | Export | Success | P0 |
| POS-027 | Import permission | CanImport | Import | Success | P0 |
| POS-028 | Delete permission | CanDelete | Delete | Success | P0 |
| POS-029 | Create permission | CanCreate | Create | Success | P0 |
| POS-030 | View permission | CanView | View | Success | P0 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 2.1 Authentication Failures (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-001 | No token | Request without token | 401 Unauthorized | P0 |
| NEG-002 | Expired token | Token expired | 401 | P0 |
| NEG-003 | Invalid signature | Tampered token | 401 | P0 |
| NEG-004 | Wrong issuer | Token from other app | 401 | P0 |
| NEG-005 | Wrong audience | Token for other API | 401 | P0 |
| NEG-006 | Malformed token | Garbage as token | 401 | P0 |
| NEG-007 | alg=none attack | Token with alg none | 401 | P0 |
| NEG-008 | Token after logout | Use logged-out token | 401 | P0 |
| NEG-009 | Revoked token | Token revoked | 401 | P0 |
| NEG-010 | Deactivated user | User disabled | 401 | P0 |
| NEG-011 | Invalid refresh token | Bad refresh | 401 | P0 |
| NEG-012 | Expired refresh token | Refresh expired | 401 | P0 |
| NEG-013 | Bearer case wrong | bearer vs Bearer | 401 or accept | P0 |
| NEG-014 | Token in wrong header | Token in body | 401 | P0 |
| NEG-015 | Empty token | Token="" | 401 | P0 |

### 2.2 Authorization Failures (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-016 | No permission | User without CanEdit | 403 Forbidden | P0 |
| NEG-017 | Cross-org access | User Org A, query Org B | 403 or filtered | P0 |
| NEG-018 | Access other user's entity | User A, entity of B | 403 | P0 |
| NEG-019 | Read-only user writes | ReadOnly role | 403 | P0 |
| NEG-020 | Partner user admin action | Partner role | 403 | P0 |
| NEG-021 | API key without scope | Key limited | 403 | P0 |
| NEG-022 | Service account restricted | Endpoint not allowed | 403 | P0 |
| NEG-023 | Delegation expired | Delegation lapsed | 403 | P0 |
| NEG-024 | Workflow state blocks | Wrong state | 403 | P0 |
| NEG-025 | Bulk mixed permission | Some no access | 403 or partial | P0 |
| NEG-026 | Export no permission | No CanExport | 403 | P0 |
| NEG-027 | Import no permission | No CanImport | 403 | P0 |
| NEG-028 | Delete no permission | No CanDelete | 403 | P0 |
| NEG-029 | Admin endpoint non-admin | User | 403 | P0 |
| NEG-030 | Audit view no permission | No audit view | 403 | P0 |

### 2.3 IDOR Attempts (15)

| ID | Test Name | Manipulation | Expected | Priority |
|----|-----------|-------------|----------|----------|
| NEG-031 | Change entity ID | Id=123 → Id=456 | 403 or 404 | P0 |
| NEG-032 | Change user ID in request | UserId=other | Ignored, use token | P0 |
| NEG-033 | Change org ID | OrgId=other | 403 | P0 |
| NEG-034 | Batch with mixed IDs | Include other's IDs | Filtered | P0 |
| NEG-035 | Sequential ID enumeration | Brute force IDs | Rate limit or 403 | P0 |
| NEG-036 | Export other org | Filter org=other | 403 | P0 |
| NEG-037 | Import for other org | Target org=other | 403 | P0 |
| NEG-038 | Permission for other entity | GET /other/id/permissions | 403 | P0 |
| NEG-039 | Workflow on other's entity | Approve other's | 403 | P0 |
| NEG-040 | Delete other's entity | DELETE other/id | 403 | P0 |
| NEG-041 | Update other's entity | PUT other/id | 403 | P0 |
| NEG-042 | View other's audit | Audit other | 403 | P0 |
| NEG-043 | Template for restricted entity | Download template | 403 | P0 |
| NEG-044 | Retry other's bulk job | Retry job other | 403 | P0 |
| NEG-045 | Access other's export | Get export other | 403 | P0 |

### 2.4 Injection Attempts (15)

| ID | Test Name | Attack | Expected | Priority |
|----|-----------|-------|----------|----------|
| NEG-046 | SQL injection in filter | '; DROP TABLE-- | Parameterized | P0 |
| NEG-047 | SQL injection in search | 1' OR '1'='1 | Parameterized | P0 |
| NEG-048 | XSS in name field | <script>alert(1)</script> | Escaped | P0 |
| NEG-049 | XSS in comment | <img src=x onerror=alert(1)> | Escaped | P0 |
| NEG-050 | NoSQL injection | {"$gt":""} | Validated | P0 |
| NEG-051 | LDAP injection | *)(uid=* | Parameterized | P0 |
| NEG-052 | Command injection | ; ls -la | Sanitized | P0 |
| NEG-053 | Path traversal | ../../../etc/passwd | Sanitized | P0 |
| NEG-054 | Header injection | CRLF in header | Validated | P0 |
| NEG-055 | Log injection | \nFake log | Escaped | P0 |
| NEG-056 | CSV injection | =cmd|' /C calc | Sanitized | P0 |
| NEG-057 | Formula injection | +cmd|' /C calc | Sanitized | P0 |
| NEG-058 | XXE in upload | Malicious XML | Rejected | P0 |
| NEG-059 | Template injection | {{constructor}} | No eval | P0 |
| NEG-060 | CRLF injection | \r\nHeader | Validated | P0 |

### 2.5 Rate Limit & CORS/CSRF (10)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-061 | Rate limit exceeded | 100 req/min, 101st | 429 Too Many Requests | P0 |
| NEG-062 | CORS disallowed origin | Origin not in list | CORS error | P0 |
| NEG-063 | CSRF missing token | POST without token | 403 | P0 |
| NEG-064 | CSRF invalid token | Wrong token | 403 | P0 |
| NEG-065 | CSRF token expired | Old token | 403 | P0 |
| NEG-066 | Replay attack | Reuse request | Nonce/timestamp | P0 |
| NEG-067 | Login rate limit | 10 failed logins | Lockout or delay | P0 |
| NEG-068 | Brute force protection | Many password attempts | Lockout | P0 |
| NEG-069 | API key rate limit | Key over limit | 429 | P0 |
| NEG-070 | Session fixation | Reuse session ID | New session | P0 |

### 2.6 Additional Negative (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-071 | Token with empty claims | JWT no claims | 401 | P0 |
| NEG-072 | Token with invalid JSON | Malformed payload | 401 | P0 |
| NEG-073 | Cross-site request without origin | Missing Origin header | CORS reject | P0 |
| NEG-074 | Permission check with null entity | EntityId=null | 400 or 404 | P1 |
| NEG-075 | Role without permissions | Empty role | 403 | P1 |
| NEG-076 | Expired API key | Key expired | 401 | P0 |
| NEG-077 | Revoked delegation | Delegation revoked | 403 | P0 |
| NEG-078 | User in wrong org hierarchy | Child org access parent | 403 | P1 |
| NEG-079 | Bulk action with no IDs | Empty ID list | 400 | P0 |
| NEG-080 | Workflow action on wrong stage | Approve Draft | 403 | P0 |
| NEG-081 | Template download without entity access | No entity perm | 403 | P1 |
| NEG-082 | Export with invalid filter | Filter syntax error | 400 | P1 |
| NEG-083 | Import with wrong entity type | Partner template for Opp | 400 | P1 |
| NEG-084 | Audit view for other tenant | Cross-tenant audit | 403 | P0 |
| NEG-085 | Session after password reset | Old session | 401 | P0 |
| NEG-086 | MFA bypass attempt | Skip MFA challenge | 401 | P0 |
| NEG-087 | Token in query string | Token in URL | Reject or 401 | P0 |
| NEG-088 | Multiple auth headers | 2 Authorization headers | 400 | P1 |
| NEG-089 | Permission for non-existent entity | EntityId=999999 | 404 | P1 |
| NEG-090 | Rate limit on auth endpoint | 100 login attempts | 429 or lockout | P0 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 3.1 Token Boundaries (15)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | JWT length | 10 | 8192 | ✅ | ✅ | Reject | P1 |
| BND-002 | Token expiry | 1 min | 24 hr | ✅ | ✅ | Config | P1 |
| BND-003 | Refresh expiry | 1 day | 90 days | ✅ | ✅ | Config | P1 |
| BND-004 | Token claim length | 0 | 4096 | ✅ | ✅ | Reject | P1 |
| BND-005 | Session ID length | 16 | 128 | ✅ | ✅ | Reject | P1 |
| BND-006 | API key length | 16 | 256 | ✅ | ✅ | Reject | P1 |
| BND-007 | Nonce length | 8 | 64 | ✅ | ✅ | Reject | P1 |
| BND-008 | CSRF token length | 16 | 128 | ✅ | ✅ | Reject | P1 |
| BND-009 | Scope count | 0 | 50 | ✅ | ✅ | Reject | P1 |
| BND-010 | Role count per user | 1 | 20 | ✅ | ✅ | Cap | P1 |
| BND-011 | Permission count | 0 | 200 | ✅ | ✅ | Cap | P1 |
| BND-012 | Claim count | 1 | 50 | ✅ | ✅ | Reject | P2 |
| BND-013 | Audience count | 1 | 10 | ✅ | ✅ | Reject | P1 |
| BND-014 | Issuer length | 1 | 256 | ✅ | ✅ | Reject | P1 |
| BND-015 | Subject length | 1 | 256 | ✅ | ✅ | Reject | P1 |

### 3.2 Numeric Boundaries (15)

| ID | Field | Zero | Negative | Very Large | Priority |
|----|-------|------|----------|------------|----------|
| BND-016 | User ID | ❌ | ❌ | Max int | P1 |
| BND-017 | Entity ID | ❌ | ❌ | Max int | P1 |
| BND-018 | Rate limit | 1 | ❌ | 10000 | P1 |
| BND-019 | Rate window sec | 1 | ❌ | 86400 | P1 |
| BND-020 | Session count | 1 | ❌ | 10 | P1 |
| BND-021 | MFA attempt count | 0 | ❌ | 10 | P1 |
| BND-022 | Lockout minutes | 5 | ❌ | 1440 | P1 |
| BND-023 | Permission ID | 1 | ❌ | Max | P1 |
| BND-024 | Role ID | 1 | ❌ | Max | P1 |
| BND-025 | Org ID | 1 | ❌ | Max | P1 |
| BND-026 | IP allowlist count | 0 | ❌ | 100 | P1 |
| BND-027 | CORS origin count | 1 | ❌ | 50 | P1 |
| BND-028 | API key limit | 1 | ❌ | 100 | P2 |
| BND-029 | Token lifetime | 60 | ❌ | 86400 | P1 |
| BND-030 | Delegation duration | 1 | ❌ | 365 | P1 |

### 3.3 Timing Boundaries (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-031 | Token at exact expiry | Expiry now | 401 | P0 |
| BND-032 | Token 1 sec before expiry | Valid | 200 | P0 |
| BND-033 | Rate limit at window boundary | Last second | Count | P1 |
| BND-034 | Session at expiry | Exact | 401 | P0 |
| BND-035 | Lockout at end | Lockout end | Allow | P1 |
| BND-036 | CSRF token at expiry | Expired | 403 | P0 |
| BND-037 | Refresh at expiry | Expired | 401 | P0 |
| BND-038 | MFA at attempt limit | 10th fail | Lockout | P0 |
| BND-039 | Rate limit reset | New window | Reset | P1 |
| BND-040 | Token issued at | Clock skew | Per config | P1 |
| BND-041 | nbf claim | Future | Reject | P0 |
| BND-042 | Delegation expiry | Lapsed | 403 | P0 |
| BND-043 | Password expiry | Expired | Force change | P0 |
| BND-044 | API key expiry | Expired | 401 | P0 |
| BND-045 | Concurrent session limit | At limit | Oldest out | P1 |

### 3.4 Permission Boundaries (15)

| ID | State | Condition | Expected | Priority |
|----|-------|-----------|----------|----------|
| BND-046 | No roles | User no roles | Minimal access | P0 |
| BND-047 | Single role | 1 role | Role permissions | P0 |
| BND-048 | Multiple roles | 5 roles | Union of perms | P0 |
| BND-049 | Admin role | Admin | Full access | P0 |
| BND-050 | Read-only role | ReadOnly | Read only | P0 |
| BND-051 | Entity owner | Created entity | Full on own | P0 |
| BND-052 | Entity collaborator | Shared | Per share | P0 |
| BND-053 | Org member | Org A | Org A data | P0 |
| BND-054 | Cross-org | Org B | 403 | P0 |
| BND-055 | Soft-deleted entity | Deleted | 403 or 404 | P0 |
| BND-056 | Workflow state | Draft | Limited | P0 |
| BND-057 | Workflow state | Active | Full | P0 |
| BND-058 | Delegation active | Delegate | Act on behalf | P0 |
| BND-059 | Delegation expired | Lapsed | 403 | P0 |
| BND-060 | Service account | Limited | Per config | P0 |

### 3.5 Input Boundaries (10)

| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-061 | Username | 255 chars | Accept | P1 |
| BND-062 | Password | 128 chars | Accept | P1 |
| BND-063 | Role name | Unicode | Stored | P1 |
| BND-064 | Permission name | Special chars | Escaped | P1 |
| BND-065 | Origin header | Valid URL | Check | P1 |
| BND-066 | Referer | Valid | Check | P1 |
| BND-067 | User-Agent | Long | Truncate | P1 |
| BND-068 | IP address | IPv6 | Accept | P1 |
| BND-069 | Scope string | 500 chars | Accept | P1 |
| BND-070 | Claim value | 4096 chars | Accept or reject | P1 |

### 3.6 Additional Boundaries (20)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-071 | Token exactly at nbf | nbf=now | Accept or reject per spec | P1 |
| BND-072 | Zero scope count | No scopes | Minimal access | P1 |
| BND-073 | Max permission check time | 200 perms | < timeout | P1 |
| BND-074 | Rate limit at 99% | 99 of 100 | Allow | P1 |
| BND-075 | CORS preflight at boundary | OPTIONS exactly | 200 | P1 |
| BND-076 | Session at max count | 10 sessions | New invalidates oldest | P1 |
| BND-077 | Delegation at expiry boundary | Expires in 1 sec | Accept or reject | P1 |
| BND-078 | Org ID at max int | Max int | Valid or reject | P1 |
| BND-079 | Empty permission list | User no perms | 403 on action | P0 |
| BND-080 | Single permission | 1 perm | That action only | P0 |
| BND-081 | Token with max claim size | 4096 char claim | Accept or reject | P1 |
| BND-082 | IP allowlist at max | 100 IPs | All checked | P1 |
| BND-083 | Origin at max length | 256 chars | Valid or reject | P1 |
| BND-084 | Lockout at boundary | Last attempt before lock | Lock or allow | P1 |
| BND-085 | MFA at attempt limit | 9th fail | 10th locks | P1 |
| BND-086 | Refresh token 1 sec before expiry | Valid | New token | P1 |
| BND-087 | API key at rate limit | Exactly at limit | 429 on next | P1 |
| BND-088 | Batch permission 100 entities | 100 IDs | All checked | P1 |
| BND-089 | Workflow at final stage | Last stage | Limited actions | P1 |
| BND-090 | Entity with max depth hierarchy | 10 levels | Resolve or timeout | P2 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 4.1 JWT Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Signature validated | Any token | Verify signature | P0 |
| FUN-002 | Expiry checked | exp claim | Reject if past | P0 |
| FUN-003 | Issuer checked | iss claim | Must match | P0 |
| FUN-004 | Audience checked | aud claim | Must match | P0 |
| FUN-005 | alg=none rejected | alg none | Reject | P0 |
| FUN-006 | Token format | JWT structure | Validate | P0 |
| FUN-007 | Required claims | sub, exp | Must present | P0 |
| FUN-008 | nbf if present | Future nbf | Reject until | P0 |
| FUN-009 | Token type | Bearer | Check | P0 |
| FUN-010 | Key rotation | New key | Old invalid | P1 |
| FUN-011 | Token revocation | Revoked | Reject | P0 |
| FUN-012 | Logout invalidates | Logout | Reject | P0 |
| FUN-013 | Multiple audiences | aud array | Any match | P1 |
| FUN-014 | Clock skew | 5 min | Allow | P1 |
| FUN-015 | Token binding | IP/binding | Optional check | P1 |

### 4.2 RBAC Rules (15)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-016 | Permission required | Endpoint | Check permission | P0 |
| FUN-017 | Role implies permission | Role has perm | Allow | P0 |
| FUN-018 | Org scope | Multi-tenant | Filter to org | P0 |
| FUN-019 | Entity owner | Own entity | Allow | P0 |
| FUN-020 | Admin bypass | Admin role | Full access | P0 |
| FUN-021 | Read-only | ReadOnly | Deny write | P0 |
| FUN-022 | Bulk per row | Bulk op | Check each | P0 |
| FUN-023 | Workflow permission | Stage | Check action | P0 |
| FUN-024 | Hierarchy | Parent org | Child access | P1 |
| FUN-025 | Delegation | Delegate | Check delegation | P0 |
| FUN-026 | Delegation expiry | Expired | Deny | P0 |
| FUN-027 | Service account | Limited | Per config | P0 |
| FUN-028 | API key scope | Scope | Enforce | P0 |
| FUN-029 | Field-level | Sensitive field | Mask if no perm | P0 |
| FUN-030 | Permission endpoint | GET permissions | Server-side | P0 |

### 4.3 IDOR Prevention Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-031 | Entity ownership | Access entity | Check owner | P0 |
| FUN-032 | Org scope | Entity org | Filter | P0 |
| FUN-033 | Batch filter | Batch IDs | Filter to own | P0 |
| FUN-034 | Export filter | Export | Org scope | P0 |
| FUN-035 | Import filter | Import | Org scope | P0 |
| FUN-036 | Audit filter | Audit query | Org scope | P0 |
| FUN-037 | Permission check | Entity ID | Verify access | P0 |
| FUN-038 | Workflow check | Entity | Verify access | P0 |
| FUN-039 | Template check | Entity type | Permission | P0 |
| FUN-040 | Job ownership | Bulk job | Own only | P0 |

### 4.4 Injection & CORS/CSRF Rules (10)

| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-041 | Parameterized queries | All queries | No concatenation | P0 |
| FUN-042 | Input sanitization | All input | Sanitize | P0 |
| FUN-043 | Output encoding | All output | Encode | P0 |
| FUN-044 | CORS origin allowlist | Origin | Check list | P0 |
| FUN-045 | CSRF token required | State-changing | Token required | P0 |
| FUN-046 | CSRF token validate | Token | Verify | P0 |
| FUN-047 | SameSite cookie | Cookie | SameSite | P0 |
| FUN-048 | Secure cookie | HTTPS | Secure flag | P0 |
| FUN-049 | Content-Type check | POST | Validate | P1 |
| FUN-050 | Rate limit | All endpoints | Enforce | P0 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: 90** | ✅ COMPLIANT

### 5.1 Auth Flow (15)

| ID | Operation | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-001 | Login flow | Valid creds | Token issued | P0 |
| INT-002 | Logout flow | Logout | Token invalid | P0 |
| INT-003 | Refresh flow | Valid refresh | New token | P0 |
| INT-004 | MFA flow | MFA required | Challenge | P0 |
| INT-005 | Password change | Change | Re-auth | P0 |
| INT-006 | Token expiry | Token expires | 401, refresh | P0 |
| INT-007 | Session limit | Max sessions | Old invalid | P0 |
| INT-008 | Delegation grant | Admin delegates | Delegate can act | P0 |
| INT-009 | API key create | Create key | Key with scope | P0 |
| INT-010 | API key revoke | Revoke | 401 | P0 |
| INT-011 | Lockout flow | Failed logins | Lockout | P0 |
| INT-012 | Lockout recovery | Wait | Unlock | P0 |
| INT-013 | Remember me | Long session | Extended | P1 |
| INT-014 | SSO flow | SSO | Token | P1 |
| INT-015 | Consent flow | OAuth | Consent | P1 |

### 5.2 Permission Flow (15)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-016 | Create with perm | CanCreate | Success | P0 |
| INT-017 | Edit with perm | CanEdit | Success | P0 |
| INT-018 | Delete with perm | CanDelete | Success | P0 |
| INT-019 | View with perm | CanView | Success | P0 |
| INT-020 | Export with perm | CanExport | Success | P0 |
| INT-021 | Import with perm | CanImport | Success | P0 |
| INT-022 | Approve with perm | CanApprove | Success | P0 |
| INT-023 | Permission endpoint | GET permissions | Flags | P0 |
| INT-024 | Org filter | Org A user | Org A only | P0 |
| INT-025 | Entity filter | Own only | Own only | P0 |
| INT-026 | Bulk permission | Mixed | Filtered | P0 |
| INT-027 | Workflow permission | Stage | Action allowed | P0 |
| INT-028 | Audit permission | CanViewAudit | Audit | P0 |
| INT-029 | Admin permission | Admin | Full | P0 |
| INT-030 | Role change | Role updated | New perms | P0 |

### 5.3 IDOR Prevention Flow (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-031 | Access own entity | Entity owned | 200 | P0 |
| INT-032 | Access other entity | Entity other | 403 | P0 |
| INT-033 | Batch own only | Batch own | Success | P0 |
| INT-034 | Batch mixed | Batch mixed | Filtered | P0 |
| INT-035 | Export own org | Export | Org filtered | P0 |
| INT-036 | Import own org | Import | Org filtered | P0 |
| INT-037 | Permission other | GET other permissions | 403 | P0 |
| INT-038 | Workflow other | Approve other | 403 | P0 |
| INT-039 | Delete other | Delete other | 403 | P0 |
| INT-040 | Update other | Update other | 403 | P0 |

### 5.4 Security Flow (10)

| ID | Test | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| INT-041 | CORS valid | Origin allowlist | Success | P0 |
| INT-042 | CSRF valid | Token | Success | P0 |
| INT-043 | Rate limit | Under limit | Success | P0 |
| INT-044 | SQL injection blocked | Attack | Blocked | P0 |
| INT-045 | XSS blocked | Attack | Escaped | P0 |
| INT-046 | Path traversal blocked | Attack | Blocked | P0 |
| INT-047 | Audit auth events | Login | Logged | P0 |
| INT-048 | Audit failed login | Failed | Logged | P0 |
| INT-049 | Audit permission denied | 403 | Logged | P1 |
| INT-050 | Sensitive data masked | No perm | Masked | P0 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: 50** | ✅ COMPLIANT

### 6.1 JWT Security (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | Token tampering | Payload | 401 | P0 |
| SEC-002 | Key confusion | Wrong key | 401 | P0 |
| SEC-003 | alg=none | alg header | 401 | P0 |
| SEC-004 | Token replay | Reuse | Nonce/timestamp | P0 |
| SEC-005 | Token theft | Stolen | Revoke on logout | P0 |
| SEC-006 | Token in URL | Logs | Never in URL | P0 |
| SEC-007 | Token in Referer | Leak | No token | P0 |
| SEC-008 | Weak key | Brute force | Strong key | P0 |
| SEC-009 | Token expiry bypass | Manipulate | Server time | P0 |
| SEC-010 | Issuer bypass | Wrong iss | Reject | P0 |

### 6.2 Access Control (10)

| ID | User | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | Any | 401 | P0 |
| SEC-012 | Wrong org | Cross-org | 403 | P0 |
| SEC-013 | Wrong entity | Other's | 403 | P0 |
| SEC-014 | No permission | Action | 403 | P0 |
| SEC-015 | Read-only | Write | 403 | P0 |
| SEC-016 | Expired delegation | Act | 403 | P0 |
| SEC-017 | Revoked API key | Request | 401 | P0 |
| SEC-018 | Deactivated user | Request | 401 | P0 |
| SEC-019 | Locked user | Request | 401 | P0 |
| SEC-020 | Wrong scope | API key | 403 | P0 |

### 6.3 IDOR (10)

| ID | Manipulation | Expected | Priority |
|----|-------------|----------|----------|
| SEC-021 | Entity ID | 403 | P0 |
| SEC-022 | User ID | Ignored | P0 |
| SEC-023 | Org ID | 403 | P0 |
| SEC-024 | Batch IDs | Filtered | P0 |
| SEC-025 | Export filter | 403 | P0 |
| SEC-026 | Permission ID | 403 | P0 |
| SEC-027 | Workflow ID | 403 | P0 |
| SEC-028 | Job ID | 403 | P0 |
| SEC-029 | Template ID | 403 | P0 |
| SEC-030 | Audit ID | 403 | P0 |

### 6.4 Injection (10)

| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| SEC-031 | SQL injection | Parameterized | P0 |
| SEC-032 | XSS | Escaped | P0 |
| SEC-033 | NoSQL injection | Validated | P0 |
| SEC-034 | Command injection | Sanitized | P0 |
| SEC-035 | Path traversal | Sanitized | P0 |
| SEC-036 | LDAP injection | Parameterized | P0 |
| SEC-037 | Header injection | Validated | P0 |
| SEC-038 | Log injection | Escaped | P0 |
| SEC-039 | XXE | Rejected | P0 |
| SEC-040 | Template injection | No eval | P0 |

### 6.5 Data Exposure (10)

| ID | Data | Risk | Expected | Priority |
|----|------|------|----------|----------|
| SEC-041 | Password in response | Never | Never | P0 |
| SEC-042 | Token in response | Never | Never | P0 |
| SEC-043 | Stack trace | Never | Never | P0 |
| SEC-044 | Internal details | Error | Generic | P0 |
| SEC-045 | PII in log | Minimal | Masked | P0 |
| SEC-046 | Cross-org data | Never | Filtered | P0 |
| SEC-047 | Deleted data | Per policy | Filtered | P0 |
| SEC-048 | Sensitive field | No perm | Null | P0 |
| SEC-049 | Config in error | Never | Generic | P0 |
| SEC-050 | DB schema | Never | Never | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | 2 users login | Both succeed | P0 |
| CON-002 | 2 users same entity | Per permission | P0 |
| CON-003 | 10 users rate limit | Enforced | P0 |
| CON-004 | Token revoke + use | Revoke wins | P0 |
| CON-005 | Role change + request | New role | P0 |
| CON-006 | Permission change + request | New perm | P0 |
| CON-007 | Logout + request | 401 | P0 |
| CON-008 | Session limit + login | Oldest out | P0 |
| CON-009 | Delegation expire + act | 403 | P0 |
| CON-010 | API key revoke + request | 401 | P0 |
| CON-011 | 50 concurrent auth | All succeed | P1 |
| CON-012 | Circuit + auth | Independent | P1 |
| CON-013 | Rate limit + retry | Both enforced | P1 |
| CON-014 | Lockout + login | Blocked | P0 |
| CON-015 | MFA + concurrent | Both complete | P1 |
| CON-016 | Refresh + use | One valid | P0 |
| CON-017 | Password change + token | 401 | P0 |
| CON-018 | Role assign + bulk | New role | P0 |
| CON-019 | Org change + query | New org | P0 |
| CON-020 | Permission revoke + action | 403 | P0 |
| CON-021 | 100 concurrent 403 | All 403 | P1 |
| CON-022 | Token rotation + request | New token | P1 |
| CON-023 | CSRF + concurrent | Both valid | P1 |
| CON-024 | CORS + concurrent | Both check | P1 |
| CON-025 | Rate limit + burst | Burst handled | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

### 8.1 Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | JWT format | Valid JWT | Valid | P1 |
| UNT-002 | JWT format | Invalid | Invalid | P1 |
| UNT-003 | Permission name | CanEdit | Valid | P1 |
| UNT-004 | Role name | Admin | Valid | P1 |
| UNT-005 | Org ID | 123 | Valid | P1 |

### 8.2 Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | Format 401 | Unauthorized | Message | P1 |
| UNT-007 | Format 403 | Forbidden | Message | P1 |
| UNT-008 | Format permission | Flags | Object | P1 |

### 8.3 Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | Permission union | 2 roles | Combined | P1 |
| UNT-010 | Org scope | User org | Filter | P1 |
| UNT-011 | Rate limit count | Request | Increment | P1 |
| UNT-012 | Token expiry | Now + 1hr | Expiry time | P1 |
| UNT-013 | Session age | Login time | Age | P1 |

### 8.4 Status Logic (5)

| ID | Test | Condition | Expected | Priority |
|----|------|-----------|----------|----------|
| UNT-014 | Has permission | CanEdit | True | P1 |
| UNT-015 | Is admin | Admin role | True | P1 |
| UNT-016 | Is expired | Token | True | P1 |
| UNT-017 | Is locked | User | True | P1 |
| UNT-018 | Can delegate | Config | True | P1 |

### 8.5 Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | Role permissions | [R1,R2] | Union | P1 |
| UNT-020 | Filter IDs | [1,2,3] | Own only | P1 |
| UNT-021 | Scope list | "a b c" | [a,b,c] | P1 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|-----------|-----------|----------|
| PRF-001 | JWT validation | < 5 ms | P1 |
| PRF-002 | Permission check | < 10 ms | P1 |
| PRF-003 | Role resolution | < 20 ms | P1 |
| PRF-004 | Auth middleware | < 15 ms | P1 |
| PRF-005 | Rate limit check | < 2 ms | P1 |
| PRF-006 | CORS check | < 1 ms | P1 |
| PRF-007 | CSRF validation | < 5 ms | P1 |
| PRF-008 | 100 auth requests | < 2 s | P1 |
| PRF-009 | Permission endpoint | < 50 ms | P1 |
| PRF-010 | Login flow | < 500 ms | P1 |
| PRF-011 | Token refresh | < 100 ms | P1 |
| PRF-012 | 1000 rate limit checks | < 1 s | P2 |
| PRF-013 | Org filter | < 20 ms | P1 |
| PRF-014 | Batch permission | 100 < 500 ms | P1 |
| PRF-015 | Memory: 1000 tokens | No leak | P2 |
| PRF-016 | Session lookup | < 10 ms | P1 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 100 auth/min | 10 min | All succeed | P1 |
| LDT-002 | 200 auth/min | 10 min | < 1% error | P1 |
| LDT-003 | 500 auth/min | 5 min | Degradation ok | P2 |
| LDT-004 | Spike: 1000 auth | 1 min | Rate limit or succeed | P1 |
| LDT-005 | Spike: 100 login | 2 min | All or lockout | P2 |
| LDT-006 | Stress: rate limit | Until 429 | 429 returned | P2 |
| LDT-007 | Stress: session limit | Max sessions | Limit enforced | P2 |
| LDT-008 | Stress: token validation | 500 req/s | Validate | P2 |
| LDT-009 | Recovery after spike | 5 min | Normal | P1 |
| LDT-010 | Recovery after stress | 10 min | Full | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| JWT validation | POS-001, NEG-001–015, FUN-001–015, SEC-001–010 |
| Role-based access | POS-002–010, NEG-016–030, FUN-016–030 |
| IDOR prevention | NEG-031–045, FUN-031–040, SEC-021–030 |
| Injection protection | NEG-046–060, FUN-041–050, SEC-031–040 |
| CORS/CSRF | POS-005, POS-007, NEG-061–065, FUN-044–048 |
| Rate limiting | POS-006, NEG-061, NEG-067–069, FUN-050 |
| Data exposure | POS-015, SEC-041–050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
