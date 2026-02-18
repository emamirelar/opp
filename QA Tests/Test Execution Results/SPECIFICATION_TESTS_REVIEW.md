# Specification Filtering Tests - Business Logic Review

**Generated:** December 2024  
**Status:** Requires Developer Review  
**Priority:** Medium

---

## Executive Summary

9 integration tests are currently skipped because their assertions don't match the actual behavior of the specification filtering logic. This document provides detailed analysis for each test to help developers determine whether to:

1. **Update the test assertions** (if current behavior is correct)
2. **Fix the specification logic** (if tests represent correct business requirements)

---

## Tests Requiring Review

### Category 1: UNOPSPartnerManagerTests

#### Test 1.1: `GetPartnersWithSpecificationAsync_WithOrgUnitIdAndOtherFilters_AppliesSpecificationOnly`

**File:** `QA Tests/Integration Tests/UnitTests/Managers/UNOPSPartnerManagerTests.cs`  
**Line:** 265-326

**Test Setup:**
```csharp
var partners = new List<UNOPSPartner>
{
    CreatePartnerWithOrgUnit(1, "Active Partner 1", 10),  // Org unit 10
    CreatePartnerWithOrgUnit(2, "Active Partner 2", 11),  // Org unit 11
    CreatePartnerWithoutOrgUnit(3, "Active Partner 2"),   // No org unit
    CreatePartnerWithoutOrgUnit(4, "Active Partner 3")    // No org unit
};
```

**Specification Applied:**
```csharp
var hierarchyIds = new List<int> { 10, 11 };
var userIds = new List<string> { "1" };
var specification = new PartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
```

**Expected (Line 320):**
```csharp
response!.TotalCount.Should().Be(3);
```

**Actual:** Different count (likely 2 - only partners with org units 10 and 11)

**Business Question:**  
Should partners WITHOUT org unit assignments be included when filtering by org unit? Currently the test expects 3 partners but only 2 have matching org units.

---

#### Test 1.2: `GetPartnersWithSpecificationAsync_WithOrgUnitIdButNoHierarchy_IncludesIndirectRelations`

**File:** `QA Tests/Integration Tests/UnitTests/Managers/UNOPSPartnerManagerTests.cs`  
**Line:** 328-381

**Test Setup:**
```csharp
var partners = new List<UNOPSPartner>
{
    CreatePartnerWithoutOrgUnit(1, "Partner 1"),  // No org unit
    CreatePartnerWithoutOrgUnit(2, "Partner 2")   // No org unit, has contact
};
// Partner 2 has a contact added
```

**Specification Applied:**
```csharp
var hierarchyIds = new List<int> { 10 };
var userIds = new List<string> { "1" };
```

**Expected (Line 376):**
```csharp
response!.TotalCount.Should().Be(1);
```

**Actual:** 2 partners returned

**Business Question:**  
Neither partner has org unit 10 assigned. Should the specification return 0 partners, or should indirect relations through contacts include partners even without direct org unit assignment?

---

### Category 2: PartnerByOrgUnitWithRelationsSpecificationTests

**File:** `QA Tests/Integration Tests/UnitTests/Specifications/PartnerByOrgUnitWithRelationsSpecificationTests.cs`

#### Test 2.1: `Constructor_AddsRequiredIncludes`

**Line:** 58-73

**Expected (Line 69):**
```csharp
specification.Includes.Should().HaveCount(2);
specification.IncludeStrings.Should().Contain("Contacts.Interactions");
specification.IncludeStrings.Should().Contain("Contacts.Interactions.InteractionContacts");
specification.IncludeStrings.Should().Contain("Contacts.Interactions.InteractionUsers");
```

**Actual:** Only 1 include found (`p.Contacts`)

**Business Question:**  
What entity relationships should be eagerly loaded for org unit filtering?
- Just `Contacts`?
- `Contacts.Interactions`?
- Full chain including `InteractionContacts` and `InteractionUsers`?

---

#### Test 2.2: `Criteria_FiltersPartnersByDirectOrgUnitLink`

**Line:** 76-101

**Test Setup:**
```csharp
var partner1 = CreatePartnerWithOrgUnit(1, "Partner 1", orgUnitId);  // Has org unit 5
var partner2 = CreatePartnerWithOrgUnit(2, "Partner 2", 999);        // Different org unit
var partner3 = CreatePartnerWithoutOrgUnit(3, "Partner 3");          // No org unit
```

**Expected (Line 97-100):**
```csharp
results.Should().HaveCount(1);
results.Should().Contain(p => p.Id == partner1.Id);
```

**Actual:** Different count

**Business Question:**  
Should partners with no org unit assignment be excluded or included when filtering by org unit ID?

---

#### Test 2.3: `Criteria_WithMultipleOrgUnitIds_FiltersCorrectly`

**Line:** 381-406

**Test Setup:**
```csharp
var partner1 = CreatePartnerWithOrgUnit(1, "Partner 1", 5);   // Org unit 5
var partner2 = CreatePartnerWithOrgUnit(2, "Partner 2", 6);   // Org unit 6
var partner3 = CreatePartnerWithOrgUnit(3, "Partner 3", 7);   // Org unit 7
var partner4 = CreatePartnerWithOrgUnit(4, "Partner 4", 999); // Different org unit
```

**Specification:**
```csharp
var orgUnitIds = new List<int> { 5, 6, 7 };
```

**Expected (Line 404):**
```csharp
results.Should().HaveCount(3);
```

**Actual:** Different count

---

### Category 3: ContactByOrgUnitHierarchySpecificationTests

**File:** `QA Tests/Integration Tests/UnitTests/Specifications/ContactByOrgUnitHierarchySpecificationTests.cs`

#### Test 3.1: `Criteria_FiltersContactsByPartnerOrgUnit`

**Line:** 72-101

**Expected:** Contacts filtered by their partner's org unit assignment

**Business Question:**  
How should contacts be filtered when their parent partner has a specific org unit?

---

#### Test 3.2: `Criteria_WithMultipleOrgUnitIds_FiltersCorrectly`

**Line:** 109-140

**Similar issue to partner specification tests.**

---

#### Test 3.3: `Criteria_ExcludesContactsWherePartnerHasNullOfficeId`

**Line:** 222-260

**Expected (Line ~250):**
```csharp
results.Should().HaveCount(1);
```

**Actual:** 2 contacts returned

**Business Question:**  
Should contacts whose partners have NULL org unit (LiaisonOfficeId) be excluded from filtered results?

---

## Specification Classes to Review

### 1. PartnerByOrgUnitWithRelationsSpecification

**File:** `UNOPS.PAO.Domain/Specifications/PartnerSpecifications/PartnerByOrgUnitWithRelationsSpecification.cs`

**Key Questions:**
1. What criteria should be applied for org unit filtering?
2. Should indirect relations (via contacts/interactions) be included?
3. What entity relationships should be eagerly loaded?

### 2. ContactByOrgUnitHierarchySpecification

**File:** `UNOPS.PAO.Domain/Specifications/ContactSpecifications/ContactByOrgUnitHierarchySpecification.cs`

**Key Questions:**
1. Should contacts be filtered by their partner's org unit?
2. How to handle contacts whose partner has no org unit assignment?

---

## Recommended Actions

### For Developer:

1. **Review the specification implementations** in the files listed above
2. **Clarify business requirements** with stakeholders:
   - How should org unit filtering work hierarchically?
   - Should indirect relations be included?
   - How to handle NULL org unit assignments?
3. **Choose a fix approach:**
   - Update test assertions if current behavior is correct
   - Fix specification logic if tests represent correct requirements

### Quick Fixes (if current behavior is correct):

```csharp
// Example: Update assertion to match actual behavior
// Before:
response!.TotalCount.Should().Be(3);

// After (example - use actual expected value):
response!.TotalCount.Should().BeGreaterThanOrEqualTo(1);
// OR
response!.TotalCount.Should().Be(actualCorrectValue);
```

---

## Test File Locations Summary

| Test Class | File Path |
|------------|-----------|
| UNOPSPartnerManagerTests | `QA Tests/Integration Tests/UnitTests/Managers/UNOPSPartnerManagerTests.cs` |
| PartnerByOrgUnitWithRelationsSpecificationTests | `QA Tests/Integration Tests/UnitTests/Specifications/PartnerByOrgUnitWithRelationsSpecificationTests.cs` |
| ContactByOrgUnitHierarchySpecificationTests | `QA Tests/Integration Tests/UnitTests/Specifications/ContactByOrgUnitHierarchySpecificationTests.cs` |

---

## Specification Class Locations Summary

| Specification Class | File Path |
|---------------------|-----------|
| PartnerByOrgUnitWithRelationsSpecification | `UNOPS.PAO.Domain/Specifications/PartnerSpecifications/` |
| ContactByOrgUnitHierarchySpecification | `UNOPS.PAO.Domain/Specifications/ContactSpecifications/` |

---

## Contact

For questions about this review, please contact the QA team or the original test authors.

