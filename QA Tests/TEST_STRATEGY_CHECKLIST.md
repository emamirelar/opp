# Test Strategy Quick Checklist

**USE THIS BEFORE CREATING ANY TEST SUITE**

---

## ✅ Comprehensive Test Strategy Checklist

### Before Implementation

- [ ] **Test strategy document created** (not just diving into code)
- [ ] **All 5 categories included** in strategy:
  - [ ] 1. Positive Tests (40-50% of total)
  - [ ] 2. Negative Tests (20-25% of total)
  - [ ] 3. Edge Cases (15-20% of total)
  - [ ] 4. Security/Validation (10-15% of total)
  - [ ] 5. Concurrency (5-10% of total)
- [ ] **Test count targets** defined for each category
- [ ] **Test IDs** follow consistent naming (TC-[FEATURE]-[TYPE]-###)
- [ ] **Priority** assigned (Critical/High/Medium)
- [ ] **OWASP coverage** planned (if user-facing or data-handling)
- [ ] **User/stakeholder review** of strategy completed

---

## During Implementation

### 1. Positive Tests (Happy Path)
- [ ] Valid CRUD operations
- [ ] Expected user workflows
- [ ] Successful authentication/authorization
- [ ] Standard business logic execution
- [ ] Component integration

### 2. Negative Tests (Failure Scenarios)
- [ ] Invalid/non-existent IDs → KeyNotFoundException
- [ ] Null/empty required parameters → ArgumentNullException
- [ ] Missing required fields → ValidationException
- [ ] Invalid foreign keys → FK constraint violations
- [ ] Unauthorized access → UnauthorizedAccessException
- [ ] Insufficient permissions → 403 Forbidden
- [ ] Operations on deleted entities → Appropriate error
- [ ] Duplicate prevention (if applicable)

### 3. Edge Cases (Boundary Conditions)
- [ ] Boundary values (0, 1, -1, MAX_INT)
- [ ] Empty strings/collections
- [ ] Very long strings (10,000+ chars)
- [ ] Special characters (`'`, `"`, `<`, `>`, `;`, `-`)
- [ ] Unicode (Chinese, Arabic, Emoji)
- [ ] Whitespace-only inputs
- [ ] Extreme numeric values (zero, negative, billions)
- [ ] Timing edge cases (immediate operations, rapid requests)
- [ ] Mass operations (dismiss all, delete all)

### 4. Security/Validation Tests
- [ ] **SQL Injection**: `'; DROP TABLE Users; --`
- [ ] **XSS**: `<script>alert('XSS')</script>`
- [ ] **NoSQL Injection**: `$where: '1==1'`
- [ ] **Command Injection**: `; rm -rf /`
- [ ] **IDOR**: User A accessing User B's data
- [ ] **Privilege Escalation**: Guest attempting admin operations
- [ ] **Mass Assignment**: Modifying read-only properties
- [ ] **CSRF**: Requests without valid token
- [ ] **Token Manipulation**: Changing JWT claims
- [ ] Required field validation
- [ ] Length limit validation
- [ ] Range validation (min/max)

### 5. Concurrency Tests
- [ ] Concurrent updates to same entity
- [ ] Concurrent deletes
- [ ] Double submit (rapid button clicks)
- [ ] Read during write
- [ ] Delete during update (deadlock scenarios)
- [ ] Cache invalidation conflicts
- [ ] Race conditions in counters
- [ ] High-frequency requests (rate limiting)

---

## After Implementation

- [ ] **All 5 categories implemented** with actual tests
- [ ] **Test execution successful** in dev environment
- [ ] **Code coverage** ≥ 80%
- [ ] **Documentation complete** (JSDoc comments)
- [ ] **No hardcoded test data** (use factories/fixtures)
- [ ] **Tests are independent** (no order dependencies)
- [ ] **Cleanup after tests** (no database pollution)
- [ ] **Performance acceptable** (test suite < 10 min)
- [ ] **CI/CD integration** configured

---

## Red Flags (DO NOT PROCEED if any apply)

- ❌ Any of the 5 categories missing entirely
- ❌ Negative tests < 15% of total
- ❌ No security tests for user-facing features
- ❌ No edge cases for numeric/text inputs
- ❌ No concurrency tests for shared resources
- ❌ Test strategy skipped (went straight to coding)
- ❌ Only positive/happy path tests created

---

## OWASP Top 10 Quick Check

If feature handles user input or database operations:

- [ ] A01: Broken Access Control (IDOR tests)
- [ ] A02: Cryptographic Failures (token security)
- [ ] A03: Injection (SQL, XSS, NoSQL)
- [ ] A04: Insecure Design (business logic validation)
- [ ] A05: Security Misconfiguration (mass assignment)
- [ ] A07: Authentication Failures (session management)
- [ ] A08: Data Integrity (audit trails)
- [ ] A09: Logging Failures (security events logged)

---

## Quick Test Distribution Formula

**For 100 tests total:**
- Positive: ~45 tests
- Negative: ~22 tests
- Edge Cases: ~18 tests
- Security: ~12 tests
- Concurrency: ~3 tests

**Scale up/down proportionally based on feature complexity**

---

## Example: DST Feature

**Initial (Incomplete) ❌**:
- Positive: 85 tests (100%)
- Negative: 0 tests
- Edge: 0 tests
- Security: 0 tests
- Concurrency: 0 tests
- **Total: 85 tests - MISSING 4 CATEGORIES**

**Final (Complete) ✅**:
- Positive: 85 tests (51%)
- Negative: 20 tests (12%)
- Edge: 20 tests (12%)
- Security: 20 tests (12%)
- Concurrency: 20 tests (12%)
- **Total: 165 tests - ALL CATEGORIES COVERED**

---

## Remember

**Comprehensive testing is MANDATORY, not optional.**

**All 5 categories. Every feature. No exceptions.**

**If you're only writing positive tests, you're only 50% done.**
