# Test Templates

This folder contains standardized test templates following the **3:1 Test Strategy** defined in `.cursor/rules/comprehensive-test-strategy.mdc` and `QA Tests/Documentation/QA_TESTER_PLAYBOOK.md`.

**MANDATORY:** All test suites — both C# test files AND Markdown test case documents — must follow the 10-category standard with the minimum test counts below.

## Quick Reference

### Core 5 Categories (3:1 Ratio Participants)

| Template | Required Minimum | Formula |
|----------|------------------|---------|
| `PositiveTests.cs.template` / `§1 Positive Tests` | 30-50 tests | Baseline (P) |
| `NegativeTests.cs.template` / `§2 Negative Tests` | ≥50 AND ≥2×P | Max(50, 2×P) |
| `BoundaryTests.cs.template` / `§3 Boundary Tests` | ≥50 AND ≥2×P | Max(50, 2×P) |
| `FunctionalTests.cs.template` / `§4 Functional Tests` | ≥50 (FIXED) | Workflow (15), Validation (15), Constraint (10), Audit (10) |
| `IntegrationTests.cs.template` / `§5 Integration Tests` | ≥50 (FIXED) | CRUD (10), Search/Filter (10), Pagination (5), Relationships (10), Error Handling (15) |

### Additional 5 Mandatory Categories

| Template | Minimum | Coverage Breakdown |
|----------|---------|-------------------|
| `SecurityTests.cs.template` / `§6 Security Tests` | ≥50 (FIXED) | OWASP Top 10, injection prevention, authorization, IDOR, mass assignment |
| `ConcurrencyTests.cs.template` / `§7 Concurrency Tests` | ≥25 (FIXED) | race conditions, deadlocks, double submit, transaction isolation, cache poisoning |
| `UnitTests.cs.template` / `§8 Unit Tests` | ≥21 | Validation (5), Formatting (3), Calculations (5), Status Logic (5), Collections (3) |
| `PerformanceTests.cs.template` / `§9 Performance Tests` | ≥16 | Single Ops (2), Bulk Ops (3), Search (5), Concurrent Access (3), Memory (3) |
| `LoadTests.cs.template` / `§10 Load Tests` | ≥10 | Sustained Load (3), Spike Testing (2), Stress Limits (3), Recovery (2) |

### Markdown Test Case Template

| Template | Purpose |
|----------|---------|
| `TestCases_Template.md` | **Markdown test case specification document.** All 10 categories in one file with compliance summary, 3:1 ratio check, and traceability matrix. Use for all `*_TestCases.md` files. |

## 3:1 Ratio Requirement

```
Negative ≥ 3 × Positive
Edge/Boundary ≥ 3 × Positive
Functional ≥ 3 × Positive
Integration ≥ 3 × Positive
```

### Example: 50 Positive Tests

| Category | Calculation | Required |
|----------|-------------|----------|
| Positive | Baseline | 50 |
| Negative | Max(50, 2×50) = 100 | 100 |
| Boundary | Max(50, 2×50) = 100 | 100 |
| Security | FIXED | 50 |
| Concurrency | FIXED | 25 |
| **Total** | | **325** |
| **Ratio Check** | N≥3P, E≥3P, F≥3P, I≥3P (each individually) | ✅ |

## How to Use Templates

1. **Copy the template** to your test directory:
   ```bash
   cp BoundaryTests.cs.template ../C#\ Tests/UNOPS.PAO.Business.Tests/YourModule/EntityBoundaryTests.cs
   ```

2. **Replace placeholders**:
   - `[ENTITY]` → Your entity name (e.g., `Partner`, `Opportunity`)
   - `[MODULE]` → Your module name (e.g., `Partners`, `Opportunities`)

3. **Implement test methods** based on your entity's specific behavior.

4. **Run validation script**:
   ```powershell
   .\Scripts\Validate-TestRatios.ps1 -Path "C# Tests/UNOPS.PAO.Business.Tests/YourModule"
   ```

## Template Contents

### PositiveTests.cs.template
- Valid input scenarios
- Standard CRUD operations
- Successful workflows
- Filter and sort operations

### NegativeTests.cs.template
- Null/empty input validation
- Non-existent entity operations
- Invalid state transitions
- SQL/XSS injection prevention
- Invalid foreign key references

### BoundaryTests.cs.template
- String length boundaries (min, max, max+1)
- Numeric boundaries (0, negative, max)
- Date boundaries (leap years, end-of-month)
- Collection boundaries (empty, single, large)
- Unicode and special characters
- Precision boundaries

### SecurityTests.cs.template
- OWASP Top 10 coverage
- Broken Access Control (A01)
- Cryptographic Failures (A02)
- Injection Prevention (A03)
- Insecure Design (A04)
- Security Misconfiguration (A05)
- Authentication Failures (A07)
- Data Integrity (A08)
- Security Logging (A09)
- SSRF Prevention (A10)

### ConcurrencyTests.cs.template
- Optimistic concurrency (RowVersion)
- Race condition prevention
- Parallel read performance
- Lock acquisition and release
- Deadlock prevention
- Transaction isolation
- Bulk operation atomicity

## Validation Scripts

### Validate-TestRatios.ps1
Validates a single test suite against the 3:1 ratio requirements.

```powershell
.\Scripts\Validate-TestRatios.ps1 -Path "C# Tests/UNOPS.PAO.Business.Tests/Partners" -Detailed
```

### Validate-AllTestSuites.ps1
Validates ALL test suites in the project.

```powershell
.\Scripts\Validate-AllTestSuites.ps1 -FailOnWarning
.\Scripts\Validate-AllTestSuites.ps1 -OutputFormat Markdown > compliance-report.md
```

## File Naming Convention

Test files MUST follow this naming convention for validation scripts to work:

| Category | File Pattern | Example |
|----------|--------------|---------|
| Positive | `PositiveTests.cs` | `PartnerPositiveTests.cs` |
| Negative | `NegativeTests.cs` | `PartnerNegativeTests.cs` |
| Boundary/Edge | `BoundaryTests.cs` or `EdgeTests.cs` | `PartnerBoundaryTests.cs` |
| Security | `SecurityTests.cs` | `PartnerSecurityTests.cs` |
| Concurrency | `ConcurrencyTests.cs` | `PartnerConcurrencyTests.cs` |

## Updates

- **2026-02-11**: Added `TestCases_Template.md` — Markdown test case template enforcing 10-category standard for all `.md` test case docs
- **2026-02-11**: Updated README minimums to match `QA_TESTER_PLAYBOOK.md` v1.4 (Functional ≥50, Integration ≥50)
- **2026-02-11**: Clarified that BOTH C# and Markdown test case documents must follow the 10-category standard
- **2026-02-02**: Updated ratio from 1.5×P to **2×P** for Negative and Boundary tests
- **2026-02-02**: Added comprehensive templates for all 5 required test categories
- **2026-01-28**: Initial template creation with 3:1 ratio strategy

## Reference

- `.cursor/rules/comprehensive-test-strategy.mdc` — Complete test strategy (AI rules)
- `QA Tests/Documentation/QA_TESTER_PLAYBOOK.md` — Human-readable QA guide (v1.4)
- `QA Tests/TEST_CASE_COMPLIANCE_TRACKER.md` — Compliance tracking for all test case documents
