# Test Templates

This folder contains standardized test templates following the **3:1 Test Strategy** defined in `.cursor/rules/comprehensive-test-strategy.mdc`.

## Quick Reference

### Category 1: Required with Minimums (3:1 Ratio)

| Template | Required Minimum | Formula |
|----------|------------------|---------|
| `PositiveTests.cs.template` | 30-50 tests | Baseline (P) |
| `NegativeTests.cs.template` | ≥50 tests | Max(50, 2×P) |
| `BoundaryTests.cs.template` | ≥50 tests | Max(50, 2×P) |
| `SecurityTests.cs.template` | ≥50 tests | FIXED |
| `ConcurrencyTests.cs.template` | ≥25 tests | FIXED |

### Category 2: Mandatory Additional (Fixed Minimums)

| Template | Minimum | Coverage Breakdown |
|----------|---------|-------------------|
| `UnitTests.cs.template` | ≥21 | Validation (5), Formatting (3), Calculations (5), Status Logic (5), Collections (3) |
| `FunctionalTests.cs.template` | ≥26 | Workflow Rules (10), Validation Rules (10), Constraint Rules (3), Audit Rules (3) |
| `IntegrationTests.cs.template` | ≥25 | CRUD Workflow (5), Search/Filter (5), Pagination (2), Relationships (3), Error Handling (10) |
| `PerformanceTests.cs.template` | ≥16 | Single Ops (2), Bulk Ops (3), Search (5), Concurrent Access (3), Memory (3) |

## 3:1 Ratio Requirement

```
(Negative + Boundary) ≥ 3 × Positive
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
| **Ratio Check** | (100+100) = 200 ≥ 3×50 = 150 | ✅ |

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

- **2026-02-02**: Updated ratio from 1.5×P to **2×P** for Negative and Boundary tests
- **2026-02-02**: Added comprehensive templates for all 5 required test categories
- **2026-01-28**: Initial template creation with 3:1 ratio strategy

## Reference

See `.cursor/rules/comprehensive-test-strategy.mdc` for the complete test strategy documentation.
