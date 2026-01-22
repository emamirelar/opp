# Security & Authorization Test Cases

**Priority**: P0 - Critical  
**Total Test Cases**: 40  

---

## Overview

Security tests covering:
- Authentication bypass attempts
- Authorization/permission violations
- Token manipulation
- Role escalation
- Data access controls
- Input sanitization

---

## Test Categories

| Category | Count | Priority |
|----------|-------|----------|
| Authentication | 10 | P0 |
| Authorization | 12 | P0 |
| Token Security | 6 | P0 |
| Role Escalation | 6 | P0 |
| Input Sanitization | 6 | P0 |

---

## Authentication Tests

### TC-SEC-001: Access without token
**Description**: API call without auth header  
**Test Steps**:
1. Call protected endpoint without token
**Expected Result**: 401 Unauthorized

### TC-SEC-002: Expired token rejected
**Description**: Use expired JWT  
**Test Steps**:
1. Create token with past expiry
2. Call API with token
**Expected Result**: 401 Unauthorized

### TC-SEC-003: Invalid signature rejected
**Description**: Tampered token  
**Test Steps**:
1. Modify token payload
2. Keep original signature
**Expected Result**: 401 Unauthorized

### TC-SEC-004: Invalid issuer rejected
**Description**: Wrong token issuer  
**Test Steps**:
1. Create token with different issuer
**Expected Result**: 401 Unauthorized

### TC-SEC-005: Invalid audience rejected
**Description**: Wrong token audience  
**Test Steps**:
1. Create token for different app
**Expected Result**: 401 Unauthorized

### TC-SEC-006: Malformed token rejected
**Description**: Invalid JWT format  
**Test Steps**:
1. Send garbage as token
**Expected Result**: 401 Unauthorized

### TC-SEC-007: None algorithm rejected
**Description**: alg=none attack  
**Test Steps**:
1. Create token with alg: none
**Expected Result**: 401 Unauthorized

### TC-SEC-008: Weak key rejected
**Description**: Brute-forceable key  
**Test Steps**:
1. Try common weak keys
**Expected Result**: Token invalid

### TC-SEC-009: Token reuse after logout
**Description**: Token invalidated on logout  
**Test Steps**:
1. Get token
2. Logout
3. Use old token
**Expected Result**: 401 Unauthorized

### TC-SEC-010: Concurrent session limit
**Description**: Too many sessions  
**Test Steps**:
1. Login on many devices
2. Verify limit enforced
**Expected Result**: Old sessions invalidated

---

## Authorization Tests

### TC-SEC-011: Access denied without permission
**Description**: Missing required permission  
**Test Steps**:
1. User without CanEdit permission
2. Try edit operation
**Expected Result**: 403 Forbidden

### TC-SEC-012: Org unit filter enforced
**Description**: Can't access other org's data  
**Test Steps**:
1. User in Org A
2. Try access Org B data
**Expected Result**: 404 or 403

### TC-SEC-013: Own data access allowed
**Description**: Can access own records  
**Test Steps**:
1. Access own created records
**Expected Result**: 200 OK

### TC-SEC-014: Admin override works
**Description**: Admin bypasses restrictions  
**Test Steps**:
1. Admin accesses restricted data
**Expected Result**: 200 OK

### TC-SEC-015: Field-level security
**Description**: Hidden fields not returned  
**Test Steps**:
1. Access entity without field permission
**Expected Result**: Sensitive fields null

### TC-SEC-016: DELETE requires permission
**Description**: Delete authorization  
**Test Steps**:
1. User without delete permission
2. Try delete
**Expected Result**: 403 Forbidden

### TC-SEC-017: Bulk delete authorization
**Description**: Each item checked  
**Test Steps**:
1. Bulk delete with mixed permissions
**Expected Result**: Partial or denied

### TC-SEC-018: Export authorization
**Description**: Export requires permission  
**Test Steps**:
1. Try export without permission
**Expected Result**: 403 Forbidden

### TC-SEC-019: Import authorization
**Description**: Import requires permission  
**Test Steps**:
1. Try import without permission
**Expected Result**: 403 Forbidden

### TC-SEC-020: Workflow action authorization
**Description**: Only allowed users can transition  
**Test Steps**:
1. Non-approver tries approve
**Expected Result**: 403 Forbidden

### TC-SEC-021: API rate limiting
**Description**: Too many requests blocked  
**Test Steps**:
1. Make 1000 rapid requests
**Expected Result**: 429 Too Many Requests

### TC-SEC-022: Resource ownership check
**Description**: Can't modify others' resources  
**Test Steps**:
1. Try modify another user's saved filter
**Expected Result**: 403 Forbidden

---

## Token Security Tests

### TC-SEC-023: Token refresh works
**Description**: Refresh before expiry  
**Test Steps**:
1. Call refresh endpoint
**Expected Result**: New valid token

### TC-SEC-024: Refresh token rotation
**Description**: Refresh token changes  
**Test Steps**:
1. Use refresh token
**Expected Result**: New refresh token

### TC-SEC-025: Old refresh token invalid
**Description**: Can't reuse refresh token  
**Test Steps**:
1. Use refresh token twice
**Expected Result**: Second use fails

### TC-SEC-026: Refresh token expiry
**Description**: Long-lived token expiry  
**Test Steps**:
1. Use expired refresh token
**Expected Result**: 401 - Must re-login

### TC-SEC-027: Token claims accurate
**Description**: Claims match user  
**Test Steps**:
1. Decode token
2. Verify claims
**Expected Result**: Correct user/roles

### TC-SEC-028: Token scope limited
**Description**: Minimal permissions in token  
**Test Steps**:
1. Check token doesn't over-grant
**Expected Result**: Only needed permissions

---

## Role Escalation Tests

### TC-SEC-029: Can't self-assign admin
**Description**: User promotes self  
**Test Steps**:
1. User tries add admin role
**Expected Result**: 403 Forbidden

### TC-SEC-030: Can't create higher role
**Description**: Create user with higher privileges  
**Test Steps**:
1. Non-admin creates admin user
**Expected Result**: 403 Forbidden

### TC-SEC-031: Can't modify higher role user
**Description**: Edit admin user  
**Test Steps**:
1. Non-admin edits admin
**Expected Result**: 403 Forbidden

### TC-SEC-032: Role change audit logged
**Description**: Track role changes  
**Test Steps**:
1. Admin changes role
2. Check audit log
**Expected Result**: Logged with details

### TC-SEC-033: Permission addition audit
**Description**: Track permission grants  
**Test Steps**:
1. Add permission
2. Check audit
**Expected Result**: Logged

### TC-SEC-034: Permission removal audit
**Description**: Track permission revokes  
**Test Steps**:
1. Remove permission
2. Check audit
**Expected Result**: Logged

---

## Input Sanitization Tests

### TC-SEC-035: SQL injection prevented
**Description**: SQL in input  
**Test Steps**:
1. Send `'; DROP TABLE partners; --`
**Expected Result**: Sanitized, no SQL execution

### TC-SEC-036: XSS prevented
**Description**: Script in input  
**Test Steps**:
1. Send `<script>alert('xss')</script>`
**Expected Result**: Sanitized in output

### TC-SEC-037: Path traversal prevented
**Description**: File path manipulation  
**Test Steps**:
1. Send `../../../etc/passwd`
**Expected Result**: Path validated

### TC-SEC-038: Command injection prevented
**Description**: OS command in input  
**Test Steps**:
1. Send `; rm -rf /`
**Expected Result**: Sanitized

### TC-SEC-039: LDAP injection prevented
**Description**: LDAP special chars  
**Test Steps**:
1. Send LDAP injection payload
**Expected Result**: Sanitized

### TC-SEC-040: XML injection prevented
**Description**: XXE attack  
**Test Steps**:
1. Send malicious XML
**Expected Result**: Parsed safely

---

**Last Updated**: December 18, 2025

