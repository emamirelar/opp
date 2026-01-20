# UNOPS Opportunity+ - Defect Report for Developers

**Generated:** December 19, 2025  
**Test Run:** Full Suite Execution  
**Total Tests:** 380  
**Failed Tests:** 41  
**Pass Rate:** 89.2%

---

## Executive Summary

This report documents all failing tests from the latest test execution. Defects are categorized by root cause to help developers prioritize and address issues efficiently.

---

## Defect Categories Overview

| Category | Count | Severity | Priority |
|----------|-------|----------|----------|
| **gRPC Authentication** | 17 | High | P1 |
| **Parameter Mismatch** | 4 | High | P1 |
| **Specification Logic** | 2 | Medium | P2 |
| **Date Parsing** | 1 | Low | P3 |
| **Other/Miscellaneous** | 17 | Medium | P2 |

---

## Category 1: gRPC Authentication Failures (17 tests)

### Root Cause
Tests are receiving `Grpc.Core.RpcException` with `StatusCode="PermissionDenied"` and message "Request had insufficient authentication scopes."

### Affected Tests

| Test Name | Location |
|-----------|----------|
| `NewAdvancedSearch_OrConditions_ReturnsUnionOfResults` | PartnerControllerTests |
| `NewAdvancedSearch_NumericComparisons_ReturnsCorrectResults` | PartnerControllerTests |
| `NewAdvancedSearch_NestedPropertySimilarity_FindsTyposInPartnerGroupName` | PartnerControllerTests |
| `NewAdvancedSearch_CombinedNestedAndDirectSimilarity_ComplexSearch` | PartnerControllerTests |
| `NewAdvancedSearch_PartnerDescriptionSearch_ReturnsResults` | PartnerControllerTests |
| `NewAdvancedSearch_CollectionPropertyEmail_SimilaritySearch` | PartnerControllerTests |
| `NewAdvancedSearch_NestedPropertyCaseInsensitive_WithSimilarity` | PartnerControllerTests |
| `NewAdvancedSearch_NestedPropertiesWithSpecialCharacters_SimilarityHandling` | PartnerControllerTests |
| `NewAdvancedSearch_InvalidFieldName_ReturnsError` | PartnerControllerTests |
| `NewAdvancedSearch_BasicTextSearch_ReturnsMatchingPartners` | PartnerControllerTests |
| `NewAdvancedSearch_DeepNestedPropertySimilarity_HandlesComplexPaths` | PartnerControllerTests |
| `NewAdvancedSearch_MultipleAndConditions_ReturnsCorrectResults` | PartnerControllerTests |
| `NewAdvancedSearch_MultipleCollectionPropertiesSimilarity_TestsAllContactFields` | PartnerControllerTests |
| `NewAdvancedSearch_ComplexMixedCriteria_ReturnsCorrectResults` | PartnerControllerTests |
| `NewAdvancedSearch_DateRangeSearch_ReturnsCorrectResults` | PartnerControllerTests |

### Error Message
```
Grpc.Core.RpcException : Status(StatusCode="PermissionDenied", Detail="Request had insufficient authentication scopes.")
```

### Recommended Fix
1. **Check Test Authentication Setup**: Verify that the test's mock authentication includes required gRPC scopes
2. **Review PAOWebApplicationFactory**: Ensure proper authentication middleware configuration for tests
3. **Add Required Scopes**: Add missing authentication scopes to the test identity claims

### File to Investigate
- `QA Tests/Integration Tests/Infrastructure/PAOWebApplicationFactory.cs`
- `QA Tests/Integration Tests/Infrastructure/TestAuthHandler.cs`

---

## Category 2: Parameter Count Mismatch (4 tests)

### Root Cause
`System.Reflection.TargetParameterCountException: Parameter count mismatch` indicates that the mock setup for `UNOPSPartnerManager` is outdated - the actual constructor signature has changed.

### Affected Tests

| Test Name | Location |
|-----------|----------|
| `GetPartnersWithSpecificationAsync_WhenHierarchyServiceNotAvailable_LogsWarningAndSkipsOrgUnitFilter` | UNOPSPartnerManagerTests |
| `TestDataPersistence_VerifyPartnersAreSavedCorrectly` | UNOPSPartnerManagerTests |
| `TestSimpleGetPartnersWithSpecification_ReturnsData` | UNOPSPartnerManagerTests |
| `GetPartnersWithSpecificationAsync_WithOrgUnitIdAndOtherFilters_AppliesSpecificationOnly` | UNOPSPartnerManagerTests |

### Error Message
```
System.Reflection.TargetParameterCountException : Parameter count mismatch.
```

### Recommended Fix
1. **Review UNOPSPartnerManager Constructor**: Check the current constructor signature
2. **Update Test Mocks**: Update the mock setup to match the current constructor parameters
3. **Check Dependency Injection**: Verify all required dependencies are being mocked

### File to Investigate
- `QA Tests/Integration Tests/UnitTests/Managers/UNOPSPartnerManagerTests.cs`
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`

---

## Category 3: Specification Logic Issues (2 tests)

### Root Cause
The specification classes have been modified after tests were written, causing assertion mismatches.

### Affected Tests

#### Test 1: `Criteria_WithMultipleOrgUnitIds_FiltersCorrectly`
**Location:** `PartnerByOrgUnitWithRelationsSpecificationTests`

**Error:**
```
Expected results to contain 3 item(s), but found 4
```

**Analysis:** The specification is returning more results than expected. Either:
- The specification logic was changed to include additional partners
- The test data setup creates an additional partner that matches the criteria

#### Test 2: `Constructor_AddsRequiredIncludes`
**Location:** `PartnerByOrgUnitWithRelationsSpecificationTests`

**Error:**
```
Expected specification.Includes to contain 2 item(s), but found 1: {p.Contacts}
```

**Analysis:** The specification was modified to only include `Contacts` instead of 2 related entities.

### Recommended Fix

**Option A: Update Test Assertions**
If the current specification behavior is correct:
1. Update `Criteria_WithMultipleOrgUnitIds_FiltersCorrectly` to expect 4 results
2. Update `Constructor_AddsRequiredIncludes` to expect 1 include

**Option B: Fix Specification Logic**
If the original test assertions represent correct behavior:
1. Review and fix `PartnerByOrgUnitWithRelationsSpecification.cs`

### Files to Investigate
- `UNOPS.PAO.Domain/Specifications/PartnerSpecifications/PartnerByOrgUnitWithRelationsSpecification.cs`
- `QA Tests/Integration Tests/UnitTests/Specifications/PartnerByOrgUnitWithRelationsSpecificationTests.cs`

---

## Category 4: Date Parsing Issue (1 test)

### Affected Test
`DateParsing_MultipleFormats_ShouldParseCorrectly(dateInput: "hier", description: "Should parse 'hier' as yesterday")`

**Location:** `DateSearchTests`

### Error Message
```
Expected result to have a value because Should parse 'hier' as yesterday, but found <null>.
```

### Analysis
The French word "hier" (yesterday) is not being parsed correctly by the date parsing logic.

### Recommended Fix
1. Add support for French date terms in the date parsing utility
2. Or update test to skip non-English date terms if multi-language support is not required

### File to Investigate
- Date parsing utility/service in the project
- `QA Tests/Integration Tests/UnitTests/DateSearchTests.cs`

---

## Quick Reference: All 41 Failed Tests

| # | Test Name | Category | Priority |
|---|-----------|----------|----------|
| 1 | `NewAdvancedSearch_OrConditions_ReturnsUnionOfResults` | gRPC Auth | P1 |
| 2 | `NewAdvancedSearch_NumericComparisons_ReturnsCorrectResults` | gRPC Auth | P1 |
| 3 | `NewAdvancedSearch_NestedPropertySimilarity_FindsTyposInPartnerGroupName` | gRPC Auth | P1 |
| 4 | `NewAdvancedSearch_CombinedNestedAndDirectSimilarity_ComplexSearch` | gRPC Auth | P1 |
| 5 | `NewAdvancedSearch_PartnerDescriptionSearch_ReturnsResults` | gRPC Auth | P1 |
| 6 | `NewAdvancedSearch_CollectionPropertyEmail_SimilaritySearch` | gRPC Auth | P1 |
| 7 | `NewAdvancedSearch_NestedPropertyCaseInsensitive_WithSimilarity` | gRPC Auth | P1 |
| 8 | `NewAdvancedSearch_NestedPropertiesWithSpecialCharacters_SimilarityHandling` | gRPC Auth | P1 |
| 9 | `NewAdvancedSearch_InvalidFieldName_ReturnsError` | gRPC Auth | P1 |
| 10 | `NewAdvancedSearch_BasicTextSearch_ReturnsMatchingPartners` | gRPC Auth | P1 |
| 11 | `NewAdvancedSearch_DeepNestedPropertySimilarity_HandlesComplexPaths` | gRPC Auth | P1 |
| 12 | `NewAdvancedSearch_MultipleAndConditions_ReturnsCorrectResults` | gRPC Auth | P1 |
| 13 | `NewAdvancedSearch_MultipleCollectionPropertiesSimilarity_TestsAllContactFields` | gRPC Auth | P1 |
| 14 | `NewAdvancedSearch_ComplexMixedCriteria_ReturnsCorrectResults` | gRPC Auth | P1 |
| 15 | `NewAdvancedSearch_DateRangeSearch_ReturnsCorrectResults` | gRPC Auth | P1 |
| 16 | `GetPartnersWithSpecificationAsync_WhenHierarchyServiceNotAvailable_LogsWarningAndSkipsOrgUnitFilter` | Param Mismatch | P1 |
| 17 | `TestDataPersistence_VerifyPartnersAreSavedCorrectly` | Param Mismatch | P1 |
| 18 | `TestSimpleGetPartnersWithSpecification_ReturnsData` | Param Mismatch | P1 |
| 19 | `GetPartnersWithSpecificationAsync_WithOrgUnitIdAndOtherFilters_AppliesSpecificationOnly` | Param Mismatch | P1 |
| 20 | `Criteria_WithMultipleOrgUnitIds_FiltersCorrectly` | Spec Logic | P2 |
| 21 | `Constructor_AddsRequiredIncludes` | Spec Logic | P2 |
| 22 | `DateParsing_MultipleFormats_ShouldParseCorrectly("hier")` | Date Parsing | P3 |
| 23-41 | Additional specification/filter tests | Mixed | P2 |

---

## Recommended Action Plan

### Immediate (P1 - This Sprint)

1. **Fix gRPC Authentication in Tests** (Fixes 17 tests)
   - Update `PAOWebApplicationFactory` to include proper authentication scopes
   - Estimated effort: 2-4 hours

2. **Update UNOPSPartnerManager Test Mocks** (Fixes 4 tests)
   - Sync test mocks with current constructor signature
   - Estimated effort: 1-2 hours

### Short-term (P2 - Next Sprint)

3. **Review Specification Logic** (Fixes 2 tests)
   - Decide if specifications or test assertions need updating
   - Estimated effort: 2-3 hours

### Low Priority (P3)

4. **Date Parsing Enhancement** (Fixes 1 test)
   - Add multi-language date term support or skip non-English terms
   - Estimated effort: 1 hour

---

## Contact

For questions about this defect report, contact the QA team.

**Report Location:** `QA Tests/Test Execution Results/DEFECTS_FOR_DEVELOPERS.md`

