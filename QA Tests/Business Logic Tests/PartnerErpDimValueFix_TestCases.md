# Partner ErpDimValue Fix - Business Logic Test Cases

## Change Overview
**PR**: #477 (partner-erpdimvalue-fix-from-development)  
**Commit**: 9eb7a7b3  
**Date**: November 5, 2025  
**Author**: Tafazzul Mohammed

## Summary of Changes

### ErpDimValue Range Fix
The system had partners with ErpDimValue > 9999 which is outside the valid range. This seeder fixes those values by:

1. Finding partners with ErpDimValue > 9999
2. Calculating the highest valid ErpDimValue (excluding 8000-9999 reserved range)
3. Reassigning values sequentially starting from next available
4. Skipping the 8000-9999 reserved range

**File**: `tools/DataImport/Archives/erpdimvaluefix/PartnerErpDimValueFixSeeder.cs`

### Business Rules for ErpDimValue

1. **Valid Range**: 1-7999 for regular partners
2. **Reserved Range**: 8000-9999 for special/reserved partners
3. **Invalid Range**: > 9999 must be corrected
4. **Uniqueness**: All ErpDimValues must be unique across ALL partners (including soft-deleted)

---

## P0 - Critical Test Cases

### TC-EPF-BL-P0-001: Fix Partners with ErpDimValue > 9999
**Priority**: P0 - Critical  
**Description**: Verify partners with values > 9999 are reassigned valid values  
**Business Rule**: Partners must have ErpDimValue in valid range (1-7999 or reserved)  
**Preconditions**: 
- Partners exist with ErpDimValue = 10001, 10002, 10003
- Partners exist with valid values (e.g., 1000-1500)

**Test Steps**:
1. Create partners with ErpDimValue = 10001, 10002, 10003
2. Create partners with valid values (1000, 1001, 1002)
3. Run FixPartnerErpDimValuesAsync
4. Verify partners 10001, 10002, 10003 now have values starting from 1003
5. Verify no partner has ErpDimValue > 9999

**Expected Result**: All invalid ErpDimValues corrected to valid range  
**Business Impact**: ERP integration requires valid dimension values

---

### TC-EPF-BL-P0-002: Skip Reserved Range (8000-9999)
**Priority**: P0 - Critical  
**Description**: Verify reassigned values skip the 8000-9999 reserved range  
**Business Rule**: New values must not be assigned in 8000-9999 range  
**Preconditions**: 
- Partners exist with ErpDimValue approaching 8000
- Partners exist with values > 9999 needing fix

**Test Steps**:
1. Create partners with ErpDimValue = 7998, 7999
2. Create partner with ErpDimValue = 10001
3. Run FixPartnerErpDimValuesAsync
4. Verify reassigned value skips 8000-9999
5. Verify new value is 10000 or higher (jumping over reserved range)

**Expected Result**: Reserved range not used for reassignment  
**Business Impact**: Reserved partners retain their special range

---

### TC-EPF-BL-P0-003: Include Soft-Deleted Partners in Uniqueness Check
**Priority**: P0 - Critical  
**Description**: Verify soft-deleted partners considered when checking uniqueness  
**Business Rule**: ErpDimValue must be unique across ALL partners including soft-deleted  
**Preconditions**: 
- Soft-deleted partner with ErpDimValue = 1500
- Partner to fix with ErpDimValue = 10001
- Highest active partner value = 1499

**Test Steps**:
1. Create partner with ErpDimValue = 1500 and IsDeleted = true
2. Create partner with ErpDimValue = 1499 and IsDeleted = false
3. Create partner with ErpDimValue = 10001 (to fix)
4. Run FixPartnerErpDimValuesAsync
5. Verify new value is 1501 (not 1500 which is used by deleted partner)

**Expected Result**: Soft-deleted partner values respected  
**Business Impact**: Prevents ERP dimension conflicts if partner restored

---

### TC-EPF-BL-P0-004: Sequential Assignment Order
**Priority**: P0 - Critical  
**Description**: Verify partners are fixed in order by current ErpDimValue  
**Business Rule**: Partners sorted by ErpDimValue before reassignment  
**Preconditions**: Partners with ErpDimValue = 10003, 10001, 10002

**Test Steps**:
1. Create partners with values 10003, 10001, 10002
2. Run FixPartnerErpDimValuesAsync
3. Verify partner with 10001 gets lowest new value
4. Verify partner with 10002 gets next value
5. Verify partner with 10003 gets highest new value

**Expected Result**: Sequential assignment maintains relative order  
**Business Impact**: Predictable reassignment behavior

---

### TC-EPF-BL-P0-005: Uniqueness After Fix
**Priority**: P0 - Critical  
**Description**: Verify no duplicate ErpDimValues after fix  
**Business Rule**: All ErpDimValues must be unique  
**Preconditions**: Multiple partners needing fix

**Test Steps**:
1. Create 10 partners with ErpDimValue > 9999
2. Run FixPartnerErpDimValuesAsync
3. Query all partners
4. Verify Count(DISTINCT ErpDimValue) = Count(ErpDimValue WHERE HasValue)

**Expected Result**: All values unique  
**Business Impact**: ERP integrity

---

### TC-EPF-BL-P0-006: No Partners to Fix
**Priority**: P0 - Critical  
**Description**: Verify graceful handling when no partners need fixing  
**Business Rule**: Should complete without error or changes  
**Preconditions**: All partners have ErpDimValue < 8000

**Test Steps**:
1. Ensure no partners have ErpDimValue > 9999
2. Run FixPartnerErpDimValuesAsync
3. Verify "No partners found" message
4. Verify no database changes

**Expected Result**: Clean completion, no changes  
**Business Impact**: Safe to run when not needed

---

### TC-EPF-BL-P0-007: LastModifiedBy Set to System User
**Priority**: P0 - Critical  
**Description**: Verify audit trail updated correctly  
**Business Rule**: LastModifiedBy = 0 (system user) and LastModifiedDate updated  
**Preconditions**: Partner needing fix

**Test Steps**:
1. Create partner with ErpDimValue = 10001, LastModifiedBy = 123
2. Run FixPartnerErpDimValuesAsync
3. Verify LastModifiedBy = 0 (system user)
4. Verify LastModifiedDate is recent (within last minute)

**Expected Result**: Audit fields updated to system user  
**Business Impact**: Clear audit trail for automated changes

---

## P1 - High Priority Test Cases

### TC-EPF-BL-P1-001: Large Number of Partners to Fix
**Priority**: P1 - High  
**Description**: Verify performance with many partners needing fix  
**Business Rule**: Should complete in reasonable time  
**Preconditions**: 100 partners with ErpDimValue > 9999

**Test Steps**:
1. Create 100 partners with ErpDimValue = 10001 to 10100
2. Measure time to run FixPartnerErpDimValuesAsync
3. Verify completes < 30 seconds
4. Verify all 100 partners fixed correctly

**Expected Result**: Efficient bulk update  
**Business Impact**: Migration performance

---

### TC-EPF-BL-P1-002: Highest Valid Value Calculation
**Priority**: P1 - High  
**Description**: Verify correct calculation of highest valid value  
**Business Rule**: Only values < 8000 considered for highest  
**Preconditions**: 
- Partners with ErpDimValue = 7000, 7500, 8500, 9500

**Test Steps**:
1. Create partners with values 7000, 7500, 8500, 9500
2. Create partner with ErpDimValue = 10001
3. Run fix and verify new value starts from 7501
4. Verify 8500, 9500 (reserved range) not considered

**Expected Result**: Highest = 7500, next = 7501  
**Business Impact**: Correct range boundary handling

---

### TC-EPF-BL-P1-003: Empty Database - No Valid Partners
**Priority**: P1 - High  
**Description**: Verify handling when no valid partners exist  
**Business Rule**: Start from 1 if no valid values exist  
**Preconditions**: 
- Only partners with ErpDimValue > 9999 exist
- No partners with ErpDimValue < 8000

**Test Steps**:
1. Create partners only with ErpDimValue = 10001, 10002
2. Run FixPartnerErpDimValuesAsync
3. Verify new values start from 1

**Expected Result**: Start from 1 when no valid base exists  
**Business Impact**: Fresh database handling

---

### TC-EPF-BL-P1-004: Partners in Reserved Range Unchanged
**Priority**: P1 - High  
**Description**: Verify partners in 8000-9999 range are not modified  
**Business Rule**: Reserved range partners should not be touched  
**Preconditions**: Partners with ErpDimValue = 8500, 9000

**Test Steps**:
1. Create partners with values 8500, 9000
2. Create partner with 10001
3. Run fix
4. Verify 8500 and 9000 unchanged
5. Verify only 10001 was modified

**Expected Result**: Reserved partners untouched  
**Business Impact**: Special partner protection

---

### TC-EPF-BL-P1-005: Idempotency
**Priority**: P1 - High  
**Description**: Verify running fix multiple times is safe  
**Business Rule**: Second run should find nothing to fix  
**Preconditions**: Partners already fixed

**Test Steps**:
1. Create partners with ErpDimValue > 9999
2. Run FixPartnerErpDimValuesAsync
3. Note the new values
4. Run FixPartnerErpDimValuesAsync again
5. Verify "No partners found" message
6. Verify values unchanged

**Expected Result**: Safe to run multiple times  
**Business Impact**: Migration reliability

---

### TC-EPF-BL-P1-006: Console Output Accuracy
**Priority**: P1 - High  
**Description**: Verify console output shows correct information  
**Business Rule**: Output should show old value → new value for each partner  
**Preconditions**: Partners to fix

**Test Steps**:
1. Create partner ID=123 with ErpDimValue = 10001
2. Capture console output during fix
3. Verify output includes "Partner ID 123 ('{name}'): 10001 → {new_value}"
4. Verify summary count is correct

**Expected Result**: Accurate console logging  
**Business Impact**: Migration monitoring

---

## P2 - Medium Priority Test Cases

### TC-EPF-BL-P2-001: Gap in Existing Values
**Priority**: P2 - Medium  
**Description**: Verify fix doesn't reuse gaps in existing values  
**Test Steps**:
1. Create partners with values 1000, 1002, 1003 (gap at 1001)
2. Create partner with 10001
3. Run fix
4. Verify new value = 1004 (not 1001)

**Expected Result**: Gaps preserved, sequential from max

---

### TC-EPF-BL-P2-002: Partner Name in Output
**Priority**: P2 - Medium  
**Description**: Verify partner name shown in console output  
**Test Steps**:
1. Create partner "ABC Corporation" with ErpDimValue = 10001
2. Run fix
3. Verify output includes "ABC Corporation"

**Expected Result**: Partner name logged for identification

---

### TC-EPF-BL-P2-003: Null ErpDimValue Partners Unchanged
**Priority**: P2 - Medium  
**Description**: Verify partners without ErpDimValue are not affected  
**Test Steps**:
1. Create partner with null ErpDimValue
2. Create partner with ErpDimValue = 10001
3. Run fix
4. Verify null partner still has null

**Expected Result**: Null values unchanged

---

## P3 - Low Priority Edge Cases

### TC-EPF-BL-P3-001: ErpDimValue Exactly 10000
**Priority**: P3 - Low  
**Description**: Verify boundary at 10000 handled correctly  
**Test Steps**:
1. Create partner with ErpDimValue = 10000
2. Run fix
3. Verify 10000 is also fixed (> 9999 means > 9999)

**Expected Result**: 10000 is fixed

---

### TC-EPF-BL-P3-002: Very Large ErpDimValue
**Priority**: P3 - Low  
**Description**: Verify handling of very large values  
**Test Steps**:
1. Create partner with ErpDimValue = 999999
2. Run fix
3. Verify corrected to valid value

**Expected Result**: Large values handled

---

### TC-EPF-BL-P3-003: Concurrent Fix Attempts
**Priority**: P3 - Low  
**Description**: Verify behavior with concurrent execution  
**Test Steps**:
1. Start fix in two parallel threads
2. Verify no duplicate assignments
3. Verify data consistent after both complete

**Expected Result**: No conflicts, consistent data

---

## Test Implementation Guidance

### Unit Test Example

```csharp
[Fact]
public async Task FixPartnerErpDimValuesAsync_WithInvalidValues_CorrectsThem()
{
    // Arrange
    var context = CreateTestContext();
    var partner1 = new Partner { Name = "Test1", ErpDimValue = 10001 };
    var partner2 = new Partner { Name = "Test2", ErpDimValue = 1000 }; // Valid
    context.Partners.AddRange(partner1, partner2);
    await context.SaveChangesAsync();
    
    // Act
    await PartnerErpDimValueFixSeeder.FixPartnerErpDimValuesAsync(context);
    
    // Assert
    var fixed = await context.Partners.FindAsync(partner1.Id);
    fixed.ErpDimValue.Should().Be(1001); // Next after 1000
    fixed.ErpDimValue.Should().BeLessThan(8000);
}

[Fact]
public async Task FixPartnerErpDimValuesAsync_SkipsReservedRange()
{
    // Arrange
    var context = CreateTestContext();
    var partnerAtBoundary = new Partner { Name = "Boundary", ErpDimValue = 7999 };
    var partnerToFix = new Partner { Name = "ToFix", ErpDimValue = 10001 };
    context.Partners.AddRange(partnerAtBoundary, partnerToFix);
    await context.SaveChangesAsync();
    
    // Act
    await PartnerErpDimValueFixSeeder.FixPartnerErpDimValuesAsync(context);
    
    // Assert
    var fixed = await context.Partners.FindAsync(partnerToFix.Id);
    fixed.ErpDimValue.Should().BeGreaterThanOrEqualTo(10000); // Skipped 8000-9999
    fixed.ErpDimValue.Should().NotBeInRange(8000, 9999);
}
```

---

## Related Documentation

- [PartnerManager Business Logic Tests](./PartnerManager_BusinessLogic_TestCases.md) - TC-PM-BL-P0-002, TC-PM-BL-P0-003
- [Partner Approval Workflow](../../UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs)

---

## Test Summary

| Priority | Test Cases | Coverage Area |
|----------|------------|---------------|
| P0 | 7 | Core fix logic, uniqueness, reserved range |
| P1 | 6 | Performance, edge cases, idempotency |
| P2 | 3 | Gaps, logging, null handling |
| P3 | 3 | Boundaries, large values, concurrency |
| **Total** | **19** | |

---

**Created**: December 8, 2025  
**PR Reference**: #477 (partner-erpdimvalue-fix-from-development)

