# UNOPSInteractionManager - Unit Test Cases

**Manager**: `UNOPSInteractionManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSInteractionManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSInteractionManager` with focus on:
- CRUD operations
- Partner-Interaction relationships
- Contact-Interaction relationships
- Interaction type handling
- Date validation
- Search and filtering

**Total Test Cases**: 35+

---

## 1. CRUD Operation Tests

### TC-IM-001: Create Interaction - Success
**Test**: `CreateInteraction_Should_ReturnCreatedInteraction_When_ValidDataProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Test Partner" };
await context.Partners.AddAsync(partner);

var request = new CreateInteractionRequest
{
    Subject = "Meeting Discussion",
    Description = "Quarterly review meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Subject.Should().Be("Meeting Discussion");
result.Type.Should().Be(InteractionType.Meeting);
result.PartnerId.Should().Be(1);
result.Id.Should().BeGreaterThan(0);
```

---

### TC-IM-002: Create Interaction - Subject Required
**Test**: `CreateInteraction_Should_ThrowException_When_SubjectIsEmpty`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "", // Empty subject
    Type = InteractionType.Email,
    Date = DateTime.UtcNow,
    PartnerId = 1
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*subject*required*");
```

---

### TC-IM-003: Create Interaction - Partner Required
**Test**: `CreateInteraction_Should_ThrowException_When_PartnerIdMissing`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Test Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = null // No partner
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*partner*required*");
```

---

### TC-IM-004: Create Interaction - Invalid Partner ID
**Test**: `CreateInteraction_Should_ThrowException_When_PartnerDoesNotExist`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Test Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 999 // Non-existent partner
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*partner*not found*");
```

---

### TC-IM-005: Update Interaction - Success
**Test**: `UpdateInteraction_Should_UpdateFields_When_ValidDataProvided`

**Arrange**:
```csharp
var interaction = new Interaction
{
    Id = 1,
    Subject = "Original Subject",
    Description = "Original Description",
    Type = InteractionType.Meeting
};
await context.Interactions.AddAsync(interaction);

var request = new UpdateInteractionRequest
{
    Subject = "Updated Subject",
    Description = "Updated Description"
};
```

**Act**:
```csharp
var result = await manager.UpdateInteractionAsync(user, 1, request);
```

**Assert**:
```csharp
result.Subject.Should().Be("Updated Subject");
result.Description.Should().Be("Updated Description");
result.Type.Should().Be(InteractionType.Meeting); // Unchanged
```

---

### TC-IM-006: Get Interaction By ID
**Test**: `GetInteractionById_Should_ReturnInteraction_When_InteractionExists`

**Arrange**:
```csharp
var interaction = new Interaction
{
    Id = 1,
    Subject = "Test Interaction",
    Type = InteractionType.Call
};
await context.Interactions.AddAsync(interaction);
```

**Act**:
```csharp
var result = await manager.GetInteractionByIdAsync(user, 1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Id.Should().Be(1);
result.Subject.Should().Be("Test Interaction");
result.Type.Should().Be(InteractionType.Call);
```

---

### TC-IM-007: Delete Interaction - Soft Delete
**Test**: `DeleteInteraction_Should_SoftDelete_When_ValidInteractionProvided`

**Arrange**:
```csharp
var interaction = new Interaction
{
    Id = 1,
    Subject = "To Delete",
    IsDeleted = false
};
await context.Interactions.AddAsync(interaction);
```

**Act**:
```csharp
await manager.DeleteInteractionAsync(user, 1);
```

**Assert**:
```csharp
var deleted = await context.Interactions.FindAsync(1);
deleted.IsDeleted.Should().BeTrue();
deleted.DeletedDate.Should().NotBeNull();
deleted.DeletedBy.Should().NotBeNull();
```

---

## 2. Interaction Type Tests

### TC-IM-008: Create Meeting Interaction
**Test**: `CreateInteraction_Should_AllowMeetingType_When_TypeIsMeeting`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Quarterly Review",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    FromDate = DateTime.UtcNow,
    ToDate = DateTime.UtcNow.AddHours(2),
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Type.Should().Be(InteractionType.Meeting);
result.FromDate.Should().NotBeNull();
result.ToDate.Should().NotBeNull();
```

---

### TC-IM-009: Create Email Interaction
**Test**: `CreateInteraction_Should_AllowEmailType_When_TypeIsEmail`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Follow-up Email",
    Type = InteractionType.Email,
    Date = DateTime.UtcNow,
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Type.Should().Be(InteractionType.Email);
```

---

### TC-IM-010: Create Call Interaction
**Test**: `CreateInteraction_Should_AllowCallType_When_TypeIsCall`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Phone Discussion",
    Type = InteractionType.Call,
    Date = DateTime.UtcNow,
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Type.Should().Be(InteractionType.Call);
```

---

### TC-IM-011: Invalid Interaction Type
**Test**: `CreateInteraction_Should_ThrowException_When_TypeIsInvalid`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Test",
    Type = (InteractionType)999, // Invalid type
    Date = DateTime.UtcNow,
    PartnerId = 1
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*interaction type*invalid*");
```

---

## 3. Contact Relationship Tests

### TC-IM-012: Create Interaction with Contacts
**Test**: `CreateInteraction_Should_AssociateContacts_When_ContactIdsProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Partner" };
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "Contact1", PartnerId = 1 },
    new() { Id = 2, FirstName = "Contact2", PartnerId = 1 }
};
await context.Partners.AddAsync(partner);
await context.Contacts.AddRangeAsync(contacts);

var request = new CreateInteractionRequest
{
    Subject = "Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 1,
    ContactIds = new List<int> { 1, 2 }
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Contacts.Should().HaveCount(2);
result.Contacts.Should().Contain(c => c.Id == 1);
result.Contacts.Should().Contain(c => c.Id == 2);
```

---

### TC-IM-013: Create Interaction - Contact Must Belong to Partner
**Test**: `CreateInteraction_Should_ThrowException_When_ContactDoesNotBelongToPartner`

**Arrange**:
```csharp
var partner1 = new Partner { Id = 1, Name = "Partner 1" };
var partner2 = new Partner { Id = 2, Name = "Partner 2" };
var contact = new Contact { Id = 1, FirstName = "Contact", PartnerId = 2 }; // Belongs to partner 2

var request = new CreateInteractionRequest
{
    Subject = "Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 1, // Partner 1
    ContactIds = new List<int> { 1 } // Contact belongs to Partner 2
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*contact*does not belong*partner*");
```

---

### TC-IM-014: Update Interaction - Add Contacts
**Test**: `UpdateInteraction_Should_AddContacts_When_NewContactIdsProvided`

**Arrange**:
```csharp
var interaction = new Interaction
{
    Id = 1,
    Subject = "Meeting",
    PartnerId = 1,
    Contacts = new List<Contact>()
};

var request = new UpdateInteractionRequest
{
    ContactIds = new List<int> { 1, 2 }
};
```

**Act**:
```csharp
var result = await manager.UpdateInteractionAsync(user, 1, request);
```

**Assert**:
```csharp
result.Contacts.Should().HaveCount(2);
```

---

## 4. Date Validation Tests

### TC-IM-015: Create Interaction - Future Date Allowed
**Test**: `CreateInteraction_Should_AllowFutureDate_When_DateInFuture`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Scheduled Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow.AddDays(7), // Future date
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Date.Should().BeAfter(DateTime.UtcNow);
```

---

### TC-IM-016: Create Interaction - Past Date Allowed
**Test**: `CreateInteraction_Should_AllowPastDate_When_DateInPast`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Past Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow.AddDays(-7), // Past date
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Date.Should().BeBefore(DateTime.UtcNow);
```

---

### TC-IM-017: Create Interaction - Date Range Validation
**Test**: `CreateInteraction_Should_ThrowException_When_ToDateBeforeFromDate`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    FromDate = DateTime.UtcNow.AddHours(2),
    ToDate = DateTime.UtcNow.AddHours(1), // Before FromDate - invalid
    PartnerId = 1
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*ToDate*must be after*FromDate*");
```

---

## 5. Filtering and Search Tests

### TC-IM-018: Get Interactions by Partner
**Test**: `GetInteractions_Should_FilterByPartner_When_PartnerIdProvided`

**Arrange**:
```csharp
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", PartnerId = 1 },
    new() { Id = 2, Subject = "I2", PartnerId = 2 },
    new() { Id = 3, Subject = "I3", PartnerId = 1 }
};

var filter = new InteractionFilterRequest
{
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(i => i.PartnerId == 1);
```

---

### TC-IM-019: Get Interactions by Type
**Test**: `GetInteractions_Should_FilterByType_When_TypeFilterProvided`

**Arrange**:
```csharp
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", Type = InteractionType.Meeting },
    new() { Id = 2, Subject = "I2", Type = InteractionType.Email },
    new() { Id = 3, Subject = "I3", Type = InteractionType.Meeting }
};

var filter = new InteractionFilterRequest
{
    Type = InteractionType.Meeting
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(i => i.Type == InteractionType.Meeting);
```

---

### TC-IM-020: Get Interactions by Date Range
**Test**: `GetInteractions_Should_FilterByDateRange_When_DateRangeProvided`

**Arrange**:
```csharp
var baseDate = DateTime.UtcNow;
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", Date = baseDate.AddDays(-10) },
    new() { Id = 2, Subject = "I2", Date = baseDate.AddDays(-5) },
    new() { Id = 3, Subject = "I3", Date = baseDate.AddDays(-2) },
    new() { Id = 4, Subject = "I4", Date = baseDate.AddDays(5) }
};

var filter = new InteractionFilterRequest
{
    FromDate = baseDate.AddDays(-7),
    ToDate = baseDate.AddDays(0)
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2); // I2 and I3 within range
result.Records.Should().OnlyContain(i => 
    i.Date >= baseDate.AddDays(-7) && i.Date <= baseDate);
```

---

### TC-IM-021: Search Interactions by Text
**Test**: `GetInteractions_Should_SearchSubjectAndDescription_When_SearchTextProvided`

**Arrange**:
```csharp
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "Quarterly Review Meeting", Description = "Discuss progress" },
    new() { Id = 2, Subject = "Email Update", Description = "Quarterly results sent" },
    new() { Id = 3, Subject = "Phone Call", Description = "Random discussion" }
};

var filter = new InteractionFilterRequest
{
    SearchText = "quarterly"
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().Contain(i => i.Subject.Contains("Quarterly", StringComparison.OrdinalIgnoreCase) ||
                                      i.Description.Contains("quarterly", StringComparison.OrdinalIgnoreCase));
```

---

## 6. Pagination Tests

### TC-IM-022: Pagination
**Test**: `GetInteractions_Should_ReturnPagedResults_When_PaginationParametersProvided`

**Arrange**:
```csharp
var interactions = Enumerable.Range(1, 30)
    .Select(i => new Interaction
    {
        Id = i,
        Subject = $"Interaction {i}",
        Type = InteractionType.Meeting,
        PartnerId = 1
    })
    .ToList();
await context.Interactions.AddRangeAsync(interactions);

var filter = new InteractionFilterRequest
{
    PageNumber = 2,
    PageSize = 10
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(10);
result.TotalCount.Should().Be(30);
result.PageNumber.Should().Be(2);
result.TotalPages.Should().Be(3);
```

---

### TC-IM-023: Sorting
**Test**: `GetInteractions_Should_SortByDate_When_SortParametersProvided`

**Arrange**:
```csharp
var baseDate = DateTime.UtcNow;
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", Date = baseDate.AddDays(-5) },
    new() { Id = 2, Subject = "I2", Date = baseDate.AddDays(-10) },
    new() { Id = 3, Subject = "I3", Date = baseDate.AddDays(-2) }
};

var filter = new InteractionFilterRequest
{
    OrderBy = "Date",
    Ascending = false // Most recent first
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(3);
result.Records[0].Id.Should().Be(3); // Most recent
result.Records[1].Id.Should().Be(1);
result.Records[2].Id.Should().Be(2); // Oldest
```

---

## 7. Permission/RBAC Tests

### TC-IM-024: User Can Only See Own Org Unit Interactions
**Test**: `GetInteractions_Should_FilterByOrgUnit_When_UserHasLimitedAccess`

**Arrange**:
```csharp
var partner1 = new Partner { Id = 1, OrgUnitId = 100 };
var partner2 = new Partner { Id = 2, OrgUnitId = 200 };

var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", PartnerId = 1, Partner = partner1 },
    new() { Id = 2, Subject = "I2", PartnerId = 2, Partner = partner2 },
    new() { Id = 3, Subject = "I3", PartnerId = 1, Partner = partner1 }
};

var user = CreateUserWithOrgUnitAccess(orgUnitId: 100);
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, new InteractionFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(i => i.Partner.OrgUnitId == 100);
```

---

### TC-IM-025: Delete Interaction - Permission Required
**Test**: `DeleteInteraction_Should_ThrowUnauthorized_When_UserLacksPermission`

**Arrange**:
```csharp
var interaction = new Interaction { Id = 1, Subject = "Test" };
var userWithoutPermission = CreateUserWithoutDeletePermission();
```

**Act**:
```csharp
Func<Task> act = async () => await manager.DeleteInteractionAsync(userWithoutPermission, 1);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<UnauthorizedAccessException>();
```

---

## 8. Business Logic Tests

### TC-IM-026: Interaction with Multiple Contacts
**Test**: `CreateInteraction_Should_LinkMultipleContacts_When_MultipleContactIdsProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1 };
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "C1", PartnerId = 1 },
    new() { Id = 2, FirstName = "C2", PartnerId = 1 },
    new() { Id = 3, FirstName = "C3", PartnerId = 1 }
};

var request = new CreateInteractionRequest
{
    Subject = "Group Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 1,
    ContactIds = new List<int> { 1, 2, 3 }
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Contacts.Should().HaveCount(3);
```

---

### TC-IM-027: Interaction Without Contacts Allowed
**Test**: `CreateInteraction_Should_AllowCreation_When_NoContactsProvided`

**Arrange**:
```csharp
var request = new CreateInteractionRequest
{
    Subject = "General Meeting",
    Type = InteractionType.Meeting,
    Date = DateTime.UtcNow,
    PartnerId = 1,
    ContactIds = new List<int>() // Empty - no specific contacts
};
```

**Act**:
```csharp
var result = await manager.CreateInteractionAsync(user, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Contacts.Should().BeEmpty();
```

---

## 9. Advanced Search Tests (PNO-677 Related)

### TC-IM-028: Search by Interaction Type
**Test**: `AdvancedSearch_Should_FilterByType_When_TypeFilterProvided`

**Arrange**:
```csharp
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", Type = InteractionType.Meeting },
    new() { Id = 2, Subject = "I2", Type = InteractionType.Email },
    new() { Id = 3, Subject = "I3", Type = InteractionType.Call }
};

var filter = new InteractionFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "type", @operator = "eq", value = "Meeting", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(1);
result.Records.First().Type.Should().Be(InteractionType.Meeting);
```

---

### TC-IM-029: Search by Date
**Test**: `AdvancedSearch_Should_FilterByDate_When_DateFilterProvided`

**Arrange**:
```csharp
var baseDate = new DateTime(2024, 1, 1);
var interactions = new List<Interaction>
{
    new() { Id = 1, Subject = "I1", Date = new DateTime(2024, 1, 15) },
    new() { Id = 2, Subject = "I2", Date = new DateTime(2024, 2, 15) },
    new() { Id = 3, Subject = "I3", Date = new DateTime(2024, 3, 15) }
};

var filter = new InteractionFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "date", @operator = "gte", value = "2024-02-01", fieldType = "date" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetInteractionsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(i => i.Date >= new DateTime(2024, 2, 1));
```

---

## Test Data Factory

```csharp
public class InteractionTestDataFactory
{
    private int _sequenceNumber = 1;

    public Interaction CreateInteraction(Action<Interaction>? customize = null)
    {
        var interaction = new Interaction
        {
            Id = _sequenceNumber++,
            Subject = $"Interaction {_sequenceNumber}",
            Description = $"Description {_sequenceNumber}",
            Type = InteractionType.Meeting,
            Date = DateTime.UtcNow,
            PartnerId = 1,
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            IsDeleted = false
        };

        customize?.Invoke(interaction);
        return interaction;
    }

    public Interaction CreateMeetingInteraction(DateTime? date = null)
    {
        return CreateInteraction(i =>
        {
            i.Type = InteractionType.Meeting;
            i.Date = date ?? DateTime.UtcNow;
            i.FromDate = date ?? DateTime.UtcNow;
            i.ToDate = (date ?? DateTime.UtcNow).AddHours(2);
        });
    }

    public Interaction CreateEmailInteraction()
    {
        return CreateInteraction(i =>
        {
            i.Type = InteractionType.Email;
        });
    }

    public Interaction CreateCallInteraction()
    {
        return CreateInteraction(i =>
        {
            i.Type = InteractionType.Call;
        });
    }

    public Interaction CreateInteractionWithContacts(List<int> contactIds)
    {
        return CreateInteraction(i =>
        {
            i.Contacts = contactIds.Select(id => new Contact { Id = id }).ToList();
        });
    }

    public List<Interaction> CreateInteractionsByType(InteractionType type, int count)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateInteraction(i => i.Type = type))
            .ToList();
    }
}
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **CRUD Operations** | 7 tests | 90% |
| **Interaction Types** | 4 tests | 95% |
| **Contact Relationships** | 3 tests | 85% |
| **Date Validation** | 3 tests | 85% |
| **Filtering/Search** | 4 tests | 85% |
| **Pagination** | 2 tests | 80% |
| **Permissions/RBAC** | 2 tests | 80% |
| **Business Logic** | 2 tests | 85% |
| **Advanced Search** | 2 tests | 85% |
| **Overall** | **35+ tests** | **85%+** |

---

## Priority Test Implementation Order

### Phase 1: Critical (Week 3)
1. TC-IM-001: Create interaction
2. TC-IM-002-004: Validation tests
3. TC-IM-005: Update interaction
4. TC-IM-006: Get by ID
5. TC-IM-007: Delete interaction

### Phase 2: High Priority (Week 3-4)
6-11. Interaction type tests
12-14. Contact relationship tests
15-17. Date validation tests

### Phase 3: Medium Priority (Week 4)
18-29. Filtering, search, pagination, RBAC tests

---

**End of UNOPSInteractionManager Unit Test Cases**

