# Data Import Fixes - Business Logic Test Cases

## Change Overview
**PR**: #479 (dataimport-fixes-v3)  
**Commit**: 0080a355  
**Date**: November 5, 2025  
**Author**: Tafazzul Mohammed

## Summary of Changes

### 1. UserId -1 Support (Opportunity+ User)
The system now supports a special "Opportunity+ User" with UserId = -1 for data imported via automated processes.

**Files Changed**:
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSContactManager.cs`
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSInteractionManager.cs`
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`

**Logic Change**:
- **Before**: `if (entity.CreatedBy > 0)` - Only looked up user info for positive user IDs
- **After**: `if (entity.CreatedBy != 0)` - Now includes user ID -1 (Opportunity+ User)

### 2. Audit Data Migration
Seeder scripts to migrate audit data from legacy user (larsj@unops.org) and 0 values to -1 (system user).

**Files Changed**:
- `UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/Partner_Audit_Data_Fixes_v3.cs`
- `UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/Interaction_Audit_Data_Fixes_v3.cs`

---

## P0 - Critical Test Cases

### TC-DIF-BL-P0-001: Contact Query - UserId -1 User Info Lookup
**Priority**: P0 - Critical  
**Description**: Verify contacts created by Opportunity+ User (ID=-1) display correct user info  
**Business Rule**: Entities with CreatedBy = -1 should have their user info looked up and displayed  
**Preconditions**: 
- UserInfo record exists for UserId = -1 (Opportunity+ User)
- Contact exists with CreatedBy = -1

**Test Steps**:
1. Create/seed a Contact with CreatedBy = -1
2. Ensure UserInfo record exists for UserId = -1
3. Call GetContactAsync to retrieve the contact
4. Verify CreatedByName is populated from UserInfo
5. Verify CreatedByOffice is populated correctly

**Expected Result**: Contact displays "Opportunity+ User" as creator  
**Business Impact**: Data import records must show correct attribution

---

### TC-DIF-BL-P0-002: Contact Query - UserId 0 Excluded from Lookup
**Priority**: P0 - Critical  
**Description**: Verify contacts with CreatedBy = 0 do not attempt user info lookup  
**Business Rule**: CreatedBy = 0 is invalid and should be skipped (not cause errors)  
**Preconditions**: Contact exists with CreatedBy = 0

**Test Steps**:
1. Create Contact with CreatedBy = 0 (legacy data)
2. Call GetContactAsync to retrieve the contact
3. Verify no error occurs
4. Verify CreatedByName is null or empty (not looked up)

**Expected Result**: No error, graceful handling of zero user ID  
**Business Impact**: Legacy data with missing creator should not break queries

---

### TC-DIF-BL-P0-003: Contact List - Negative UserId in Bulk Query
**Priority**: P0 - Critical  
**Description**: Verify bulk contact queries include UserId -1 in user lookup  
**Business Rule**: GetContactsAsync must include -1 in userIds for UserInfo lookup  
**Preconditions**: Multiple contacts with various CreatedBy values including -1

**Test Steps**:
1. Create contacts with CreatedBy = [1, 2, -1, 5, -1]
2. Ensure UserInfo exists for all userIds including -1
3. Call GetContactsAsync with pagination
4. Verify all contacts have CreatedByName populated
5. Verify contacts with CreatedBy=-1 show correct user info

**Expected Result**: All contacts including those created by Opportunity+ User show user info  
**Business Impact**: Contact lists must display creator info for all records

---

### TC-DIF-BL-P0-004: Interaction Query - UserId -1 Support
**Priority**: P0 - Critical  
**Description**: Verify interactions with CreatedBy = -1 display correct user info  
**Business Rule**: Same as contacts - negative user IDs should be looked up  
**Preconditions**: 
- Interaction exists with CreatedBy = -1
- UserInfo exists for UserId = -1

**Test Steps**:
1. Create Interaction with CreatedBy = -1
2. Call GetInteractionAsync to retrieve it
3. Verify CreatedByName populated correctly

**Expected Result**: Interaction shows Opportunity+ User as creator  
**Business Impact**: Interaction history must show correct attribution

---

### TC-DIF-BL-P0-005: Partner Query - UserId -1 Support
**Priority**: P0 - Critical  
**Description**: Verify partners with CreatedBy = -1 display correct user info  
**Business Rule**: Partners created via data import should show system user  
**Preconditions**: 
- Partner exists with CreatedBy = -1
- UserInfo exists for UserId = -1

**Test Steps**:
1. Create Partner with CreatedBy = -1
2. Call GetPartnerAsync to retrieve it
3. Verify CreatedByName populated correctly
4. Verify CreatedByOffice populated if available

**Expected Result**: Partner shows Opportunity+ User as creator  
**Business Impact**: Partner records from data import must have proper attribution

---

### TC-DIF-BL-P0-006: User Info Lookup - Include Negative IDs
**Priority**: P0 - Critical  
**Description**: Verify user info repository query includes negative user IDs  
**Business Rule**: Query `userInfoRepository.GetAll().Where(u => userIds.Contains(u.UserId))` must work with -1  
**Preconditions**: UserInfo record for UserId = -1 exists

**Test Steps**:
1. Create list of userIds including -1: [1, 2, -1, 3]
2. Query UserInfo repository with Contains filter
3. Verify UserId = -1 record is returned
4. Verify no SQL/LINQ errors with negative values

**Expected Result**: Repository correctly returns user info for ID = -1  
**Business Impact**: Core user lookup functionality

---

### TC-DIF-BL-P0-007: Audit Data Fix - Partner CreatedBy Migration
**Priority**: P0 - Critical  
**Description**: Verify Partner_Audit_Data_Fixes_v3 updates CreatedBy correctly  
**Business Rule**: Partners with CreatedBy = larsjUserId or 0 should be updated to -1  
**Preconditions**: 
- User larsj@unops.org exists
- Partners exist with CreatedBy = larsjUserId or CreatedBy = 0

**Test Steps**:
1. Get larsj@unops.org user ID
2. Create partners with CreatedBy = larsjUserId and CreatedBy = 0
3. Run UpdatePartnerAuditDataAsync
4. Verify all affected partners now have CreatedBy = -1
5. Verify partners with other CreatedBy values unchanged

**Expected Result**: Legacy audit data migrated to system user  
**Business Impact**: Data consistency for imported records

---

### TC-DIF-BL-P0-008: Audit Data Fix - Partner LastModifiedBy Migration
**Priority**: P0 - Critical  
**Description**: Verify Partner_Audit_Data_Fixes_v3 updates LastModifiedBy correctly  
**Business Rule**: Partners with LastModifiedBy = larsjUserId or 0 should be updated to -1  
**Preconditions**: Similar to P0-007

**Test Steps**:
1. Create partners with LastModifiedBy = larsjUserId and LastModifiedBy = 0
2. Run UpdatePartnerAuditDataAsync
3. Verify all affected partners now have LastModifiedBy = -1

**Expected Result**: LastModifiedBy migrated to system user  
**Business Impact**: Consistent audit trail

---

### TC-DIF-BL-P0-009: Audit Data Fix - Interaction CreatedBy Migration
**Priority**: P0 - Critical  
**Description**: Verify Interaction_Audit_Data_Fixes_v3 updates CreatedBy correctly  
**Business Rule**: Interactions with CreatedBy = larsjUserId or 0 should be updated to -1  
**Preconditions**: 
- User larsj@unops.org exists
- Interactions exist with target CreatedBy values

**Test Steps**:
1. Get larsj@unops.org user ID
2. Create interactions with CreatedBy = larsjUserId and CreatedBy = 0
3. Run UpdateInteractionAuditDataAsync
4. Verify all affected interactions now have CreatedBy = -1

**Expected Result**: Interaction audit data migrated  
**Business Impact**: Interaction history data consistency

---

### TC-DIF-BL-P0-010: Audit Data Fix - Interaction LastModifiedBy Migration
**Priority**: P0 - Critical  
**Description**: Verify Interaction_Audit_Data_Fixes_v3 updates LastModifiedBy correctly  
**Business Rule**: Interactions with LastModifiedBy = larsjUserId or 0 should be updated to -1  
**Preconditions**: Similar to P0-009

**Test Steps**:
1. Create interactions with LastModifiedBy = larsjUserId and LastModifiedBy = 0
2. Run UpdateInteractionAuditDataAsync
3. Verify all affected interactions now have LastModifiedBy = -1

**Expected Result**: LastModifiedBy migrated to system user  
**Business Impact**: Consistent interaction audit trail

---

## P1 - High Priority Test Cases

### TC-DIF-BL-P1-001: Audit Fix - Transaction Rollback on Error
**Priority**: P1 - High  
**Description**: Verify transaction rolls back if error occurs during audit fix  
**Business Rule**: Atomicity - all or nothing update  
**Preconditions**: Database setup with partners

**Test Steps**:
1. Setup condition that will cause error during ExecuteUpdateAsync
2. Run UpdatePartnerAuditDataAsync
3. Verify transaction rolled back
4. Verify no partial updates applied

**Expected Result**: Complete rollback on failure  
**Business Impact**: Data integrity

---

### TC-DIF-BL-P1-002: Audit Fix - Missing larsj User Handling
**Priority**: P1 - High  
**Description**: Verify graceful handling when larsj@unops.org user not found  
**Business Rule**: If legacy user doesn't exist, skip updates without error  
**Preconditions**: User larsj@unops.org does NOT exist

**Test Steps**:
1. Ensure larsj@unops.org user is not in database
2. Run UpdatePartnerAuditDataAsync
3. Verify no exception thrown
4. Verify warning message logged
5. Verify no updates performed

**Expected Result**: Graceful skip with warning  
**Business Impact**: Migration robustness

---

### TC-DIF-BL-P1-003: Contact Specification Query - UserId -1 Support
**Priority**: P1 - High  
**Description**: Verify specification-based contact queries include UserId -1  
**Business Rule**: GetContactsWithSpecificationAsync must handle -1 in user lookup  
**Preconditions**: Contacts with CreatedBy = -1 matching specification

**Test Steps**:
1. Create contacts with various CreatedBy values including -1
2. Create specification filter
3. Call GetContactsWithSpecificationAsync
4. Verify contacts with CreatedBy=-1 have user info populated

**Expected Result**: Specification queries work with system user  
**Business Impact**: Advanced search functionality

---

### TC-DIF-BL-P1-004: Gmail Addon Contact Query - UserId -1 Support
**Priority**: P1 - High  
**Description**: Verify Gmail addon contact queries support UserId -1  
**Business Rule**: Contacts for Gmail addon should show correct creator info  
**Preconditions**: Contacts with CreatedBy = -1

**Test Steps**:
1. Create contacts for Gmail addon query with CreatedBy = -1
2. Call GetContactsForGmailAddon
3. Verify user info populated correctly

**Expected Result**: Gmail addon shows correct creator  
**Business Impact**: Gmail integration data quality

---

### TC-DIF-BL-P1-005: Audit Fix - Count Verification
**Priority**: P1 - High  
**Description**: Verify audit fix reports correct update counts  
**Business Rule**: Console output should show accurate counts  
**Preconditions**: Known number of records to update

**Test Steps**:
1. Create 5 partners with CreatedBy = larsjUserId
2. Create 3 partners with CreatedBy = 0
3. Run UpdatePartnerAuditDataAsync
4. Verify console shows "Updated CreatedBy for 8 partners"

**Expected Result**: Accurate count reporting  
**Business Impact**: Migration verification

---

### TC-DIF-BL-P1-006: Audit Fix - No Updates Needed
**Priority**: P1 - High  
**Description**: Verify handling when no records need updating  
**Business Rule**: Should complete successfully with zero updates message  
**Preconditions**: No partners with larsjUserId or 0 as CreatedBy

**Test Steps**:
1. Ensure no partners have CreatedBy = larsjUserId or 0
2. Run UpdatePartnerAuditDataAsync
3. Verify "No partners found" message
4. Verify transaction completes successfully

**Expected Result**: Clean completion with informative message  
**Business Impact**: Migration idempotency

---

### TC-DIF-BL-P1-007: Partner List - Mixed UserIds Performance
**Priority**: P1 - High  
**Description**: Verify performance with mix of positive and negative user IDs  
**Business Rule**: User lookup should be efficient regardless of ID sign  
**Preconditions**: 1000 partners with mixed CreatedBy values

**Test Steps**:
1. Create 1000 partners with CreatedBy values [1, -1, 2, -1, ...] alternating
2. Query partners with pagination
3. Measure response time
4. Verify < 2000ms for 100 results

**Expected Result**: Performance acceptable with mixed IDs  
**Business Impact**: System responsiveness

---

### TC-DIF-BL-P1-008: UserInfo Repository - UserId -1 Record
**Priority**: P1 - High  
**Description**: Verify UserInfo record exists for Opportunity+ User  
**Business Rule**: System must have UserInfo for UserId = -1  
**Preconditions**: Database seeded correctly

**Test Steps**:
1. Query UserInfo for UserId = -1
2. Verify record exists
3. Verify Name = "Opportunity+ User" or similar
4. Verify OrgUnit populated if applicable

**Expected Result**: System user record exists  
**Business Impact**: Foundation for data import attribution

---

## P2 - Medium Priority Test Cases

### TC-DIF-BL-P2-001: Audit Fix - Idempotency
**Priority**: P2 - Medium  
**Description**: Verify running audit fix multiple times is safe  
**Test Steps**:
1. Run UpdatePartnerAuditDataAsync
2. Run it again
3. Verify second run updates 0 records
4. Verify data unchanged

**Expected Result**: Safe to run multiple times

---

### TC-DIF-BL-P2-002: Contact with Null CreatedBy
**Priority**: P2 - Medium  
**Description**: Verify handling of null CreatedBy (if possible)  
**Test Steps**:
1. If nullable, create contact with null CreatedBy
2. Query contact
3. Verify no error, graceful handling

**Expected Result**: Null handled gracefully

---

### TC-DIF-BL-P2-003: Bulk User Info Query - Large ID Set
**Priority**: P2 - Medium  
**Description**: Verify user info lookup with many unique user IDs  
**Test Steps**:
1. Create contacts with 500 unique user IDs (including -1)
2. Query contacts
3. Verify all user info populated
4. Verify performance acceptable

**Expected Result**: Large user ID sets handled efficiently

---

### TC-DIF-BL-P2-004: Audit Fix - Case Insensitive Email Match
**Priority**: P2 - Medium  
**Description**: Verify larsj@unops.org match is case-insensitive  
**Test Steps**:
1. Ensure user exists with email "LarsJ@UNOPS.org" (different case)
2. Run audit fix
3. Verify user found via ToLower() comparison

**Expected Result**: Case-insensitive email matching

---

## P3 - Low Priority Edge Cases

### TC-DIF-BL-P3-001: Extreme Negative UserId
**Priority**: P3 - Low  
**Description**: Verify handling of very negative user IDs  
**Test Steps**:
1. Create entity with CreatedBy = -999999
2. Query entity
3. Verify no overflow or error

**Expected Result**: Large negative numbers handled

---

### TC-DIF-BL-P3-002: Mixed Audit Fix - Same Record Both Fields
**Priority**: P3 - Low  
**Description**: Verify record with both CreatedBy and LastModifiedBy needing update  
**Test Steps**:
1. Create partner with CreatedBy = larsjUserId AND LastModifiedBy = larsjUserId
2. Run audit fix
3. Verify BOTH fields updated to -1

**Expected Result**: Both fields updated in same transaction

---

### TC-DIF-BL-P3-003: Concurrent Audit Fixes
**Priority**: P3 - Low  
**Description**: Verify behavior if audit fix runs concurrently  
**Test Steps**:
1. Start audit fix in two parallel threads
2. Verify both complete without deadlock
3. Verify data correct after both complete

**Expected Result**: No deadlocks, consistent data

---

## Test Implementation Guidance

### Unit Test Implementation

```csharp
// tests/UNOPS.PAO.Business.Tests/DataImport/UserIdNegativeOneTests.cs

[Fact]
public async Task GetContactAsync_WithCreatedByMinusOne_ReturnsUserInfo()
{
    // Arrange
    var contact = new Contact { Id = 1, CreatedBy = -1, Name = "Test" };
    var userInfo = new UserInfo { UserId = -1, Name = "Opportunity+ User" };
    
    // Setup mocks and context
    // ...
    
    // Act
    var result = await manager.GetContactAsync(user, 1);
    
    // Assert
    result.CreatedByName.Should().Be("Opportunity+ User");
}

[Fact]
public async Task GetContactAsync_WithCreatedByZero_DoesNotLookupUserInfo()
{
    // Arrange
    var contact = new Contact { Id = 1, CreatedBy = 0, Name = "Test" };
    
    // Act
    var result = await manager.GetContactAsync(user, 1);
    
    // Assert
    result.CreatedByName.Should().BeNullOrEmpty();
}
```

### Integration Test for Audit Fix

```csharp
// tests/UNOPS.PAO.Business.Tests/DataImport/AuditDataFixTests.cs

[Fact]
public async Task UpdatePartnerAuditDataAsync_UpdatesLegacyCreatedBy()
{
    // Arrange
    // Create larsj user and partners with that CreatedBy
    
    // Act
    await Partner_Audit_Data_Fixes_v3.UpdatePartnerAuditDataAsync(context);
    
    // Assert
    var updatedPartner = await context.Partners.FirstAsync();
    updatedPartner.CreatedBy.Should().Be(-1);
}
```

---

## Related Files

- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSContactManager.cs`
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSInteractionManager.cs`
- `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`
- `UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/Partner_Audit_Data_Fixes_v3.cs`
- `UNOPS.PAO.UNOPSDataAccess/Seed/Seeders/Interaction_Audit_Data_Fixes_v3.cs`

---

## Test Summary

| Priority | Test Cases | Coverage Area |
|----------|------------|---------------|
| P0 | 10 | Core UserId -1 support, Audit data migration |
| P1 | 8 | Error handling, Performance, Specification queries |
| P2 | 4 | Idempotency, Edge cases |
| P3 | 3 | Extreme values, Concurrency |
| **Total** | **25** | |

---

**Created**: December 8, 2025  
**PR Reference**: #479 (dataimport-fixes-v3)  
**Related PRs**: #476, #477 (partner-erpdimvalue-fix)

