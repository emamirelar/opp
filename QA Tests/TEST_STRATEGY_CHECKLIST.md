# Test Strategy Quick Checklist

**USE THIS BEFORE CREATING ANY TEST SUITE**

---

## 🚨 CRITICAL: 3:1 RATIO REQUIREMENT 🚨

**ABSOLUTE MANDATE**: Create **THREE times as many** negative and edge tests as positive tests.

**Minimum Per Category**:
- Negative: ≥50 tests
- Edge Cases: ≥50 tests
- Security: ≥50 tests (independent requirement, not part of 3:1 ratio)
- Concurrency: ≥25 tests (independent requirement, not part of 3:1 ratio)

**Formula**: `Negative + Edge ≥ 3 × Positive`

**REJECT any test strategy that doesn't meet this requirement.**

---

## ✅ Comprehensive Test Strategy Checklist

### Before Implementation

- [ ] **Test strategy document created** (not just diving into code)
- [ ] **3:1 ratio calculated and documented explicitly**:
  - [ ] Positive tests counted: P = ___
  - [ ] Minimum negative/edge: 3P = ___
  - [ ] Each category meets minimum (≥50 for neg/edge/sec, ≥25 for concurrency)
- [ ] **All 5 categories included** in strategy:
  - [ ] 1. Positive Tests (baseline)
  - [ ] 2. Negative Tests (≥50, ≥P for 3:1 ratio)
  - [ ] 3. Edge Cases (≥50, ≥P for 3:1 ratio)
  - [ ] 4. Security/Validation (≥50, independent requirement)
  - [ ] 5. Concurrency (≥25, independent requirement)
- [ ] **Test count targets** meet ALL minimums:
  - [ ] Negative ≥ 50 ✅
  - [ ] Edge ≥ 50 ✅
  - [ ] Security ≥ 50 ✅ (independent, not part of 3:1)
  - [ ] Concurrency ≥ 25 ✅ (independent, not part of 3:1)
  - [ ] Total negative + edge ≥ 3 × positive ✅
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
- ❌ **Negative tests < 50 tests**
- ❌ **Edge case tests < 50 tests**
- ❌ **Security tests < 50 tests** (independent minimum, not part of 3:1)
- ❌ **Concurrency tests < 25 tests** (independent minimum, not part of 3:1)
- ❌ **Total negative + edge < 3 × positive tests**
- ❌ 3:1 ratio not calculated or documented in strategy
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

**For 50 positive tests (minimum viable):**
- Positive: 50 tests (baseline)
- Negative: 75 tests (3:1 ratio = 150 ÷ 2 = 75 each for neg/edge)
- Edge Cases: 75 tests (3:1 ratio = 150 ÷ 2 = 75 each for neg/edge)
- Security: 50 tests (independent minimum ≥50, not part of 3:1)
- Concurrency: 25 tests (independent minimum ≥25, not part of 3:1)
- **Total: 275 tests**
- **Ratio: 150:50 = 3:1 ✅ MEETS 3:1 RULE**

**For 85 positive tests (like DST):**
- Positive: 85 tests (baseline)
- Negative: 128 tests (3:1 ratio = 255 ÷ 2 ≈ 128 each for neg/edge)
- Edge Cases: 127 tests (3:1 ratio = 255 ÷ 2 ≈ 127 each for neg/edge)
- Security: 50 tests (independent minimum ≥50, not part of 3:1)
- Concurrency: 50 tests (2x minimum for larger features, not part of 3:1)
- **Total: 440 tests**
- **Ratio: 255:85 = 3:1 ✅ MEETS 3:1 RULE**

---

## Example: DST Feature Evolution

**Phase 1 (Incomplete) ❌ REJECT**:
- Positive: 85 tests
- Negative: 0 tests (< 50 minimum) ❌
- Edge: 0 tests (< 50 minimum) ❌
- Security: 0 tests (< 50 minimum) ❌
- Concurrency: 0 tests (< 25 minimum) ❌
- **Total: 85 tests**
- **Ratio: 0:85 = 0:1 ❌ FAR BELOW 3:1 RULE - REJECT**

**Phase 2 (Marginal) ⚠️ BELOW TARGET**:
- Positive: 85 tests
- Negative: 20 tests (< 50 minimum) ⚠️
- Edge: 20 tests (< 50 minimum) ⚠️
- Security: 50 tests (≥50 ✅, independent minimum met)
- Concurrency: 20 tests (< 25 minimum) ⚠️
- **Total: 195 tests**
- **Ratio: 40:85 = 0.47:1 ⚠️ FAR BELOW 3:1 RULE**
- **Action Required: Add 215 more negative/edge tests to reach 3:1 (255 total)**

**Phase 3 (Compliant) ✅ MEETS REQUIREMENTS**:
- Positive: 85 tests
- Negative: 128 tests (≥50 ✅, meets 3:1 ✅)
- Edge: 127 tests (≥50 ✅, meets 3:1 ✅)
- Security: 50 tests (≥50 ✅, independent minimum)
- Concurrency: 50 tests (≥25 ✅, independent minimum)
- **Total: 440 tests**
- **Ratio: 255:85 = 3:1 ✅ MEETS 3:1 RULE**

---

## Remember

**🚨 THE 3:1 RULE IS NON-NEGOTIABLE 🚨**

**For every positive test, create THREE times as many negative and edge tests combined.**

**Minimum per category:**
- Negative: ≥50 tests (part of 3:1 ratio)
- Edge Cases: ≥50 tests (part of 3:1 ratio)
- Security: ≥50 tests (independent requirement, keep existing tests)
- Concurrency: ≥25 tests (independent requirement)

**All 5 categories. Every feature. No exceptions. No compromises.**

**If you're only writing positive tests, you're less than 25% done.**

**Formula: Total Negative + Edge ≥ 3 × Positive Tests**

**Note: Security tests remain required (≥50 minimum) but are not part of the 3:1 ratio calculation going forward. Keep all existing security tests.**
