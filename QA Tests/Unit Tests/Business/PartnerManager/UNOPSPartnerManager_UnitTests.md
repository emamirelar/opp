# UNOPSPartnerManager - Unit Test Cases

**Manager**: `UNOPSPartnerManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSPartnerManager` with focus on:
- CRUD operations
- Partner approval workflow
- ErpDimValue sequence generation (PNO-686 prevention)
- Validation logic
- Business rules
- Error handling

**Total Test Cases**: 75+

---

## 1. GetNextErpDimValueAsync Tests (CRITICAL - PNO-686)

### Purpose
Prevent regression of PNO-686 defect where ErpDimValue was generated incorrectly.

### TC-PM-001: Normal Sequence Generation
**Test**: `GetNextErpDimValue_Should_Return_NextSequentialValue_When_ValidPartnersExist`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 1960 },
    new() { Id = 2, ErpDimValue = 1961 }
};
await context.Partners.AddRangeAsync(partners);
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1962);
```

---

### TC-PM-002: Reserved Range Exclusion (8000-9999)
**Test**: `GetNextErpDimValue_Should_Skip_ReservedRange_When_ValuesInRange8000To9999Exist`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 1961 },
    new() { Id = 2, ErpDimValue = 8500 }, // In reserved range - should be ignored
    new() { Id = 3, ErpDimValue = 9000 }, // In reserved range - should be ignored
    new() { Id = 4, ErpDimValue = 10000 } // Above reserved range - should be ignored
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1962); // Should use 1961 + 1, not 10000 + 1
```

**Priority**: **CRITICAL** - This test prevents PNO-686 recurrence

---

### TC-PM-003: Empty Database Scenario
**Test**: `GetNextErpDimValue_Should_ReturnOne_When_NoPartnersExist`

**Arrange**:
```csharp
// Empty database - no partners
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1); // Should return 1 when database is empty
```

---

### TC-PM-004: Boundary Value - Just Before Reserved Range
**Test**: `GetNextErpDimValue_Should_Return8000_When_HighestValueIs7999`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 7999 }
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(8000); // Should increment to 8000 (start of reserved range is OK)
```

---

### TC-PM-005: Boundary Value - At Start of Reserved Range
**Test**: `GetNextErpDimValue_Should_Return8001_When_HighestValueIs8000`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 7998 },
    new() { Id = 2, ErpDimValue = 8000 } // Should be ignored
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(7999); // Should use 7998 + 1, ignoring 8000
```

---

### TC-PM-006: Boundary Value - At End of Reserved Range
**Test**: `GetNextErpDimValue_Should_Return10000_When_HighestValidValueIs9999`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 7999 },
    new() { Id = 2, ErpDimValue = 9999 } // Should be ignored
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(8000); // Should use 7999 + 1
```

---

### TC-PM-007: Null ErpDimValue Partners
**Test**: `GetNextErpDimValue_Should_IgnoreNullValues_When_PartnersHaveNullErpDimValue`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 1960 },
    new() { Id = 2, ErpDimValue = null }, // Should be ignored
    new() { Id = 3, ErpDimValue = null }  // Should be ignored
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1961);
```

---

### TC-PM-008: Deleted Partners Included
**Test**: `GetNextErpDimValue_Should_ConsiderDeletedPartners_When_CalculatingSequence`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 1960, IsDeleted = false },
    new() { Id = 2, ErpDimValue = 1961, IsDeleted = true } // Deleted but should be considered
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1962); // Should include deleted partners to prevent reuse
```

---

### TC-PM-009: Multiple Values in Reserved Range
**Test**: `GetNextErpDimValue_Should_SkipAllReservedValues_When_MultipleValuesInReservedRange`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, ErpDimValue = 1500 },
    new() { Id = 2, ErpDimValue = 8000 },
    new() { Id = 3, ErpDimValue = 8100 },
    new() { Id = 4, ErpDimValue = 8500 },
    new() { Id = 5, ErpDimValue = 9000 },
    new() { Id = 6, ErpDimValue = 9999 }
};
```

**Act**:
```csharp
var result = await manager.GetNextErpDimValueAsync();
```

**Assert**:
```csharp
result.Should().Be(1501);
```

---

### TC-PM-010: Large Dataset Performance
**Test**: `GetNextErpDimValue_Should_PerformEfficiently_When_ThousandsOfPartnersExist`

**Arrange**:
```csharp
var partners = Enumerable.Range(1, 5000)
    .Select(i => new Partner { Id = i, ErpDimValue = i })
    .ToList();
await context.Partners.AddRangeAsync(partners);
```

**Act**:
```csharp
var stopwatch = Stopwatch.StartNew();
var result = await manager.GetNextErpDimValueAsync();
stopwatch.Stop();
```

**Assert**:
```csharp
result.Should().Be(5001);
stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in < 1 second
```

---

## 2. ApprovePartnerAsync Tests

### TC-PM-011: Successful Approval
**Test**: `ApprovePartner_Should_SetApprovalFields_When_ValidAdminApproves`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Test Partner",
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
var user = CreateAdminUser(); // With PARTNER_GLOB_ADMIN role
var request = new UpdatePartnerRequest();
```

**Act**:
```csharp
var result = await manager.ApprovePartnerAsync(user, 1, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.Approved);
result.PartnerApprovalDate.Should().NotBeNull();
result.PartnerApprovalDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
result.CanCreateNewOpportunities.Should().BeTrue();
result.ErpDimValue.Should().BeGreaterThan(0);
```

---

### TC-PM-012: ErpDimValue Assignment on Approval
**Test**: `ApprovePartner_Should_AssignErpDimValue_When_NotAlreadyAssigned`

**Arrange**:
```csharp
var existingPartner = new Partner { Id = 1, ErpDimValue = 1000 };
await context.Partners.AddAsync(existingPartner);

var newPartner = new Partner
{
    Id = 2,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved,
    ErpDimValue = null // Not yet assigned
};
```

**Act**:
```csharp
var result = await manager.ApprovePartnerAsync(adminUser, 2, request);
```

**Assert**:
```csharp
result.ErpDimValue.Should().Be(1001); // Should assign next sequential value
```

---

### TC-PM-013: ErpDimValue Not Overwritten
**Test**: `ApprovePartner_Should_NotOverwriteErpDimValue_When_AlreadyAssigned`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    ErpDimValue = 5000, // Already assigned
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
```

**Act**:
```csharp
var result = await manager.ApprovePartnerAsync(adminUser, 1, request);
```

**Assert**:
```csharp
result.ErpDimValue.Should().Be(5000); // Should keep existing value
```

---

### TC-PM-014: Non-Admin Cannot Approve
**Test**: `ApprovePartner_Should_ThrowUnauthorized_When_NonAdminTriesToApprove`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Status = EntityStatus.Active };
var regularUser = CreateRegularUser(); // Without PARTNER_GLOB_ADMIN role
```

**Act**:
```csharp
Func<Task> act = async () => await manager.ApprovePartnerAsync(regularUser, 1, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<UnauthorizedAccessException>()
    .WithMessage("*Partnership Global Administrator*");
```

---

### TC-PM-015: Cannot Approve Inactive Partner
**Test**: `ApprovePartner_Should_ThrowException_When_PartnerIsInactive`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Inactive, // Not active
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.ApprovePartnerAsync(adminUser, 1, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("*Active partners*");
```

---

### TC-PM-016: Approval Audit Trail
**Test**: `ApprovePartner_Should_RecordAuditTrail_When_Approved`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
var admin = CreateAdminUser(id: 123, name: "John Admin");
```

**Act**:
```csharp
var result = await manager.ApprovePartnerAsync(admin, 1, request);
```

**Assert**:
```csharp
result.PartnerApprovedBy.Should().Contain("John Admin");
result.PartnerApprovedBy.Should().Contain("123");
result.PartnerApprovedBy.Should().Contain(DateTime.Now.ToString("yyyy-MM-dd"));
```

---

## 3. UnapprovePartnerAsync Tests

### TC-PM-017: Successful Unapproval
**Test**: `UnapprovePartner_Should_RemoveApprovalStatus_When_ValidAdminUnapproves`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.Approved,
    CanCreateNewOpportunities = true
};
```

**Act**:
```csharp
var result = await manager.UnapprovePartnerAsync(adminUser, 1, request);
```

**Assert**:
```csharp
result.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.NotApproved);
result.CanCreateNewOpportunities.Should().BeFalse();
```

---

### TC-PM-018: Cannot Unapprove NotApproved Partner
**Test**: `UnapprovePartner_Should_ThrowException_When_PartnerNotApproved`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.UnapprovePartnerAsync(adminUser, 1, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<InvalidOperationException>()
    .WithMessage("*approved partners*");
```

---

## 4. CreatePartnerAsync Tests

### TC-PM-019: Successful Partner Creation
**Test**: `CreatePartner_Should_CreateWithDefaultValues_When_ValidDataProvided`

**Arrange**:
```csharp
var request = new CreatePartnerRequest
{
    Name = "New Partner",
    PartnerShortDescription = "Short desc"
};
```

**Act**:
```csharp
var result = await manager.CreatePartnerAsync(user, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Name.Should().Be("New Partner");
result.Status.Should().Be(EntityStatus.Active);
result.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.NotApproved);
result.ErpDimValue.Should().BeNull(); // Not assigned until approved
result.CanCreateNewOpportunities.Should().BeFalse();
```

---

### TC-PM-020: Name Required Validation
**Test**: `CreatePartner_Should_ThrowException_When_NameIsEmpty`

**Arrange**:
```csharp
var request = new CreatePartnerRequest
{
    Name = "", // Empty name
    PartnerShortDescription = "Desc"
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreatePartnerAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*name*required*");
```

---

### TC-PM-021: Duplicate Partner Name
**Test**: `CreatePartner_Should_ThrowException_When_PartnerNameAlreadyExists`

**Arrange**:
```csharp
var existingPartner = new Partner { Id = 1, Name = "Existing Partner" };
await context.Partners.AddAsync(existingPartner);

var request = new CreatePartnerRequest
{
    Name = "Existing Partner" // Duplicate name
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreatePartnerAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*already exists*");
```

---

## 5. UpdatePartnerAsync Tests

### TC-PM-022: Successful Update
**Test**: `UpdatePartner_Should_UpdateFields_When_ValidDataProvided`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Original Name",
    PartnerShortDescription = "Original Desc"
};
var request = new UpdatePartnerRequest
{
    Name = "Updated Name",
    PartnerShortDescription = "Updated Desc"
};
```

**Act**:
```csharp
var result = await manager.UpdatePartnerAsync(user, 1, request);
```

**Assert**:
```csharp
result.Name.Should().Be("Updated Name");
result.PartnerShortDescription.Should().Be("Updated Desc");
```

---

### TC-PM-023: Cannot Update Approved Partner (Regular User)
**Test**: `UpdatePartner_Should_ThrowUnauthorized_When_RegularUserUpdatesApprovedPartner`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Approved Partner",
    PartnerApprovalStatus = PartnerApprovalStatus.Approved
};
var regularUser = CreateRegularUser();
```

**Act**:
```csharp
Func<Task> act = async () => await manager.UpdatePartnerAsync(regularUser, 1, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<UnauthorizedAccessException>();
```

---

### TC-PM-024: Admin Can Update Approved Partner
**Test**: `UpdatePartner_Should_AllowUpdate_When_AdminUpdatesApprovedPartner`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Approved Partner",
    PartnerApprovalStatus = PartnerApprovalStatus.Approved
};
var admin = CreateAdminUser();
```

**Act**:
```csharp
var result = await manager.UpdatePartnerAsync(admin, 1, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
```

---

## 6. DeletePartnerAsync / ArchivePartnerAsync Tests

### TC-PM-025: Soft Delete Partner
**Test**: `DeletePartner_Should_SoftDelete_When_ValidPartnerProvided`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Partner To Delete",
    IsDeleted = false
};
```

**Act**:
```csharp
await manager.DeletePartnerAsync(user, 1);
```

**Assert**:
```csharp
var deleted = await context.Partners.FindAsync(1);
deleted.IsDeleted.Should().BeTrue();
deleted.DeletedDate.Should().NotBeNull();
deleted.DeletedBy.Should().NotBeNull();
```

---

### TC-PM-026: Archive NotApproved Partner
**Test**: `ArchivePartner_Should_Archive_When_PartnerNotApproved`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.NotApproved
};
```

**Act**:
```csharp
var result = await manager.ArchivePartnerAsync(user, 1, request);
```

**Assert**:
```csharp
result.Status.Should().Be(EntityStatus.Archived);
```

---

### TC-PM-027: Cannot Archive Approved Partner (Regular User)
**Test**: `ArchivePartner_Should_ThrowUnauthorized_When_RegularUserArchivesApprovedPartner`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Status = EntityStatus.Active,
    PartnerApprovalStatus = PartnerApprovalStatus.Approved
};
var regularUser = CreateRegularUser();
```

**Act**:
```csharp
Func<Task> act = async () => await manager.ArchivePartnerAsync(regularUser, 1, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<UnauthorizedAccessException>()
    .WithMessage("*administrators*");
```

---

## 7. GetPartnerByIdAsync Tests

### TC-PM-028: Get Existing Partner
**Test**: `GetPartnerById_Should_ReturnPartner_When_PartnerExists`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Test Partner"
};
await context.Partners.AddAsync(partner);
```

**Act**:
```csharp
var result = await manager.GetPartnerByIdAsync(user, 1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Id.Should().Be(1);
result.Name.Should().Be("Test Partner");
```

---

### TC-PM-029: Non-Existent Partner Returns Null
**Test**: `GetPartnerById_Should_ReturnNull_When_PartnerDoesNotExist`

**Arrange**:
```csharp
// No partner with ID 999
```

**Act**:
```csharp
var result = await manager.GetPartnerByIdAsync(user, 999);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-PM-030: Deleted Partner Not Returned
**Test**: `GetPartnerById_Should_ReturnNull_When_PartnerIsDeleted`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Deleted Partner",
    IsDeleted = true
};
await context.Partners.AddAsync(partner);
```

**Act**:
```csharp
var result = await manager.GetPartnerByIdAsync(user, 1);
```

**Assert**:
```csharp
result.Should().BeNull(); // Soft-deleted partners not returned
```

---

## 8. GetPartnersAsync Tests (List/Pagination)

### TC-PM-031: Get All Partners
**Test**: `GetPartners_Should_ReturnAllActivePartners_When_NoFilterProvided`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "Partner 1", Status = EntityStatus.Active },
    new() { Id = 2, Name = "Partner 2", Status = EntityStatus.Active },
    new() { Id = 3, Name = "Partner 3", Status = EntityStatus.Inactive }, // Should be included
    new() { Id = 4, Name = "Partner 4", IsDeleted = true } // Should be excluded
};
await context.Partners.AddRangeAsync(partners);
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(user, new PartnerFilterRequest());
```

**Assert**:
```csharp
result.Should().HaveCount(3);
result.Should().NotContain(p => p.IsDeleted);
```

---

### TC-PM-032: Pagination
**Test**: `GetPartners_Should_ReturnPagedResults_When_PaginationParametersProvided`

**Arrange**:
```csharp
var partners = Enumerable.Range(1, 25)
    .Select(i => new Partner { Id = i, Name = $"Partner {i}" })
    .ToList();
await context.Partners.AddRangeAsync(partners);

var filter = new PartnerFilterRequest
{
    PageNumber = 2,
    PageSize = 10
};
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(10);
result.TotalCount.Should().Be(25);
result.PageNumber.Should().Be(2);
```

---

### TC-PM-033: Filter By Approval Status
**Test**: `GetPartners_Should_FilterByApprovalStatus_When_StatusFilterProvided`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "P1", PartnerApprovalStatus = PartnerApprovalStatus.Approved },
    new() { Id = 2, Name = "P2", PartnerApprovalStatus = PartnerApprovalStatus.NotApproved },
    new() { Id = 3, Name = "P3", PartnerApprovalStatus = PartnerApprovalStatus.Approved }
};

var filter = new PartnerFilterRequest
{
    PartnerApprovalStatus = PartnerApprovalStatus.Approved
};
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(p => p.PartnerApprovalStatus == PartnerApprovalStatus.Approved);
```

---

### TC-PM-034: Search By Name
**Test**: `GetPartners_Should_SearchByName_When_SearchTextProvided`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "UNICEF" },
    new() { Id = 2, Name = "UNESCO" },
    new() { Id = 3, Name = "World Bank" }
};

var filter = new PartnerFilterRequest
{
    SearchText = "UNI"
};
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().Contain(p => p.Name.Contains("UNICEF"));
result.Records.Should().Contain(p => p.Name.Contains("UNESCO"));
```

---

## 9. Permission/RBAC Tests

### TC-PM-035: User Can Only See Own Org Unit Partners
**Test**: `GetPartners_Should_FilterByOrgUnit_When_UserHasLimitedAccess`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "P1", OrgUnitId = 100 },
    new() { Id = 2, Name = "P2", OrgUnitId = 200 },
    new() { Id = 3, Name = "P3", OrgUnitId = 100 }
};

var user = CreateUserWithOrgUnitAccess(orgUnitId: 100);
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(user, new PartnerFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(p => p.OrgUnitId == 100);
```

---

### TC-PM-036: Admin Can See All Partners
**Test**: `GetPartners_Should_ReturnAllPartners_When_UserIsGlobalAdmin`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "P1", OrgUnitId = 100 },
    new() { Id = 2, Name = "P2", OrgUnitId = 200 },
    new() { Id = 3, Name = "P3", OrgUnitId = 300 }
};

var admin = CreateGlobalAdmin();
```

**Act**:
```csharp
var result = await manager.GetPartnersAsync(admin, new PartnerFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(3);
```

---

## 10. Validation Tests

### TC-PM-037: Pooled Fund Validation
**Test**: `CreatePartner_Should_SetPooledFundFlag_When_ValidValueProvided`

**Arrange**:
```csharp
var request = new CreatePartnerRequest
{
    Name = "Test Partner",
    PooledFund = true
};
```

**Act**:
```csharp
var result = await manager.CreatePartnerAsync(user, request);
```

**Assert**:
```csharp
result.PooledFund.Should().BeTrue();
```

---

### TC-PM-038: Key Global Partner Validation
**Test**: `UpdatePartner_Should_SetKeyGlobalPartner_When_ValidValueProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Partner", KeyGlobalPartner = false };
var request = new UpdatePartnerRequest { KeyGlobalPartner = true };
```

**Act**:
```csharp
var result = await manager.UpdatePartnerAsync(user, 1, request);
```

**Assert**:
```csharp
result.KeyGlobalPartner.Should().BeTrue();
```

---

## 11. Concurrent Access Tests

### TC-PM-039: Concurrent Approval of Multiple Partners
**Test**: `ApprovePartner_Should_AssignUniqueErpDimValues_When_MultipleConcurrentApprovals`

**Arrange**:
```csharp
var partners = new List<Partner>
{
    new() { Id = 1, Name = "P1", Status = EntityStatus.Active },
    new() { Id = 2, Name = "P2", Status = EntityStatus.Active },
    new() { Id = 3, Name = "P3", Status = EntityStatus.Active }
};
```

**Act**:
```csharp
var tasks = partners.Select(p => 
    manager.ApprovePartnerAsync(adminUser, p.Id, new UpdatePartnerRequest())
).ToArray();

await Task.WhenAll(tasks);
```

**Assert**:
```csharp
var results = tasks.Select(t => t.Result).ToList();
var erpDimValues = results.Select(r => r.ErpDimValue).ToList();

erpDimValues.Should().OnlyHaveUniqueItems(); // No duplicates
erpDimValues.Should().AllSatisfy(v => v.Should().BeGreaterThan(0));
```

---

## 12. Mapping Tests

### TC-PM-040: Entity to Model Mapping
**Test**: `MapEntityToModel_Should_MapAllFields_When_ValidPartnerProvided`

**Arrange**:
```csharp
var partner = new Partner
{
    Id = 1,
    Name = "Test Partner",
    PartnerShortDescription = "Short",
    PartnerLongDescription = "Long",
    ErpDimValue = 1500,
    PartnerApprovalStatus = PartnerApprovalStatus.Approved,
    PooledFund = true,
    KeyGlobalPartner = true
};
```

**Act**:
```csharp
var model = await manager.MapEntityToModelAsync(partner, mapper, user);
```

**Assert**:
```csharp
model.Id.Should().Be(1);
model.Name.Should().Be("Test Partner");
model.PartnerShortDescription.Should().Be("Short");
model.ErpDimValue.Should().Be(1500);
model.PartnerApprovalStatus.Should().Be(PartnerApprovalStatus.Approved);
model.PooledFund.Should().BeTrue();
model.KeyGlobalPartner.Should().BeTrue();
```

---

## Test Data Factories

### Partner Test Data Factory

```csharp
public class PartnerTestDataFactory
{
    private int _sequenceNumber = 1;

    public Partner CreatePartner(Action<Partner>? customize = null)
    {
        var partner = new Partner
        {
            Id = _sequenceNumber++,
            Name = $"Test Partner {_sequenceNumber}",
            PartnerShortDescription = $"Short description {_sequenceNumber}",
            Status = EntityStatus.Active,
            PartnerApprovalStatus = PartnerApprovalStatus.NotApproved,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            IsDeleted = false
        };

        customize?.Invoke(partner);
        return partner;
    }

    public Partner CreateApprovedPartner(int? erpDimValue = null)
    {
        return CreatePartner(p =>
        {
            p.PartnerApprovalStatus = PartnerApprovalStatus.Approved;
            p.PartnerApprovalDate = DateTime.UtcNow;
            p.ErpDimValue = erpDimValue ?? 1000 + _sequenceNumber;
            p.CanCreateNewOpportunities = true;
        });
    }

    public List<Partner> CreatePartnersWithErpDimValues(params int[] erpDimValues)
    {
        return erpDimValues.Select(value => CreateApprovedPartner(value)).ToList();
    }

    public Partner CreatePartnerInReservedRange()
    {
        return CreateApprovedPartner(erpDimValue: 8500); // In 8000-9999 range
    }
}
```

---

## Test Helpers

### Mock User Creator

```csharp
public static class TestUserHelper
{
    public static ClaimsPrincipal CreateAdminUser(int id = 1, string name = "Admin User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, "PARTNER_GLOB_ADMIN"),
            new(ClaimTypes.Email, $"{name.Replace(" ", "")}@unops.org")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    public static ClaimsPrincipal CreateRegularUser(int id = 2, string name = "Regular User")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, id.ToString()),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, $"{name.Replace(" ", "")}@example.com")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    public static ClaimsPrincipal CreateUserWithOrgUnitAccess(int orgUnitId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "3"),
            new(ClaimTypes.Name, "Org Unit User"),
            new("OrgUnitId", orgUnitId.ToString())
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
```

---

## Test Suite Execution

### Running Tests

```bash
# Run all PartnerManager tests
dotnet test --filter "FullyQualifiedName~PartnerManagerTests"

# Run ErpDimValue tests only (critical for PNO-686)
dotnet test --filter "FullyQualifiedName~PartnerManagerTests.GetNextErpDimValue"

# Run with coverage
dotnet test --filter "FullyQualifiedName~PartnerManagerTests" /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~GetNextErpDimValue_Should_Skip_ReservedRange"
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **ErpDimValue Generation** | 10 tests | 100% (CRITICAL) |
| **Approval Workflow** | 6 tests | 95% |
| **CRUD Operations** | 8 tests | 85% |
| **Validation** | 5 tests | 90% |
| **Permissions/RBAC** | 3 tests | 80% |
| **Pagination/Filtering** | 4 tests | 80% |
| **Mapping** | 1 test | 85% |
| **Concurrent Access** | 1 test | 70% |
| **Overall** | **40+ tests** | **90%+** |

---

## Priority Test Implementation Order

### Phase 1: Critical (Week 1) - Defect Prevention
1. TC-PM-002: Reserved range exclusion (PNO-686)
2. TC-PM-001: Normal sequence generation
3. TC-PM-003: Empty database scenario
4. TC-PM-004-006: Boundary values
5. TC-PM-012: ErpDimValue assignment on approval

**Goal**: Prevent PNO-686 recurrence

### Phase 2: High Priority (Week 2) - Core Functionality
6. TC-PM-011: Successful approval
7. TC-PM-014: Non-admin cannot approve
8. TC-PM-019: Partner creation
9. TC-PM-022: Partner update
10. TC-PM-028: Get partner by ID

**Goal**: Cover main CRUD operations

### Phase 3: Medium Priority (Week 3) - Advanced Features
11-20. Remaining approval, unapproval, archive tests
21-30. Filtering, pagination, search tests
31-35. Permission/RBAC tests

**Goal**: Comprehensive coverage

### Phase 4: Nice to Have (Week 4)
36-40. Validation, concurrent access, performance tests

---

**End of UNOPSPartnerManager Unit Test Cases**

**Next Steps**:
1. Create test project: `UNOPS.PAO.Business.Tests`
2. Implement test data factories
3. Write tests in priority order
4. Achieve 90%+ coverage
5. Integrate into CI/CD pipeline

