# UNOPSContactManager - Unit Test Cases

**Manager**: `UNOPSContactManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSContactManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `UNOPSContactManager` with focus on:
- CRUD operations
- **Duplicate detection** (PNO-676 prevention)
- **Advanced search functionality** (PNO-677 prevention)
- Contact-Partner relationships
- Validation logic
- Error handling

**Total Test Cases**: 50+

---

## 1. Duplicate Detection Tests (CRITICAL - PNO-676)

### Purpose
Prevent regression of PNO-676 defect where duplicate detection failed after inline edits.

### TC-CM-001: Create Contact - Duplicate Detection
**Test**: `CreateContact_Should_CheckForDuplicates_When_ContactWithSameEmailExists`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};
await context.Contacts.AddAsync(existingContact);

var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com" // Duplicate email
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*duplicate*email*");
```

---

### TC-CM-002: Detect Duplicates - Exclude Own ID
**Test**: `DetectDuplicates_Should_ExcludeOwnRecord_When_IdProvided`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};
await context.Contacts.AddAsync(existingContact);

var request = new DetectDuplicateRequest
{
    Id = 1, // Exclude this ID
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com" // Same as existing
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeFalse(); // Should not detect self as duplicate
result.Duplicates.Should().BeEmpty();
```

**Priority**: **CRITICAL** - This test prevents PNO-676 recurrence

---

### TC-CM-003: Detect Duplicates - Similar Names
**Test**: `DetectDuplicates_Should_FindSimilarNames_When_NamesAreSimilar`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Smith",
    Email = "john.smith@example.com"
};
await context.Contacts.AddAsync(existingContact);

var request = new DetectDuplicateRequest
{
    FirstName = "Jon",  // Typo - similar
    LastName = "Smith",
    Email = "jon.smith@work.com" // Different email
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeTrue();
result.Duplicates.Should().HaveCount(1);
result.Duplicates.First().Score.Should().BeGreaterThan(0.7); // High similarity
result.Duplicates.First().MatchReason.Should().Contain("name");
```

---

### TC-CM-004: Detect Duplicates - Email Match
**Test**: `DetectDuplicates_Should_DetectEmailMatch_When_EmailIdentical`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};

var request = new DetectDuplicateRequest
{
    FirstName = "Jane", // Different name
    LastName = "Smith",
    Email = "john.doe@example.com" // Same email - high confidence duplicate
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeTrue();
result.HighConfidence.Should().Be(1); // Email match is high confidence
result.Duplicates.First().Score.Should().BeGreaterThan(0.9);
```

---

### TC-CM-005: Detect Duplicates - No Match
**Test**: `DetectDuplicates_Should_ReturnNoMatch_When_ContactIsUnique`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};

var request = new DetectDuplicateRequest
{
    FirstName = "Jane",
    LastName = "Smith",
    Email = "jane.smith@different.com"
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeFalse();
result.Duplicates.Should().BeEmpty();
```

---

### TC-CM-006: Detect Duplicates - After Inline Edit
**Test**: `DetectDuplicates_Should_ReturnUpdatedStatus_When_RecordEditedToBeUnique`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};

// First request - duplicate
var firstRequest = new DetectDuplicateRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com"
};

// Second request - edited to be unique (simulating inline edit)
var editedRequest = new DetectDuplicateRequest
{
    FirstName = "Jane",  // Changed
    LastName = "Smith",  // Changed
    Email = "jane.smith@different.com" // Changed
};
```

**Act**:
```csharp
var firstResult = await manager.DetectDuplicatesAsync(firstRequest);
var editedResult = await manager.DetectDuplicatesAsync(editedRequest);
```

**Assert**:
```csharp
firstResult.HasDuplicates.Should().BeTrue(); // Initially duplicate
editedResult.HasDuplicates.Should().BeFalse(); // After edit, unique
```

**Priority**: **CRITICAL** - This test prevents PNO-676 recurrence

---

### TC-CM-007: Detect Duplicates - Multiple Matches
**Test**: `DetectDuplicates_Should_ReturnAllMatches_When_MultipleContactsMatch`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Smith", Email = "john1@example.com" },
    new() { Id = 2, FirstName = "John", LastName = "Smith", Email = "john2@example.com" },
    new() { Id = 3, FirstName = "Jon", LastName = "Smith", Email = "jon@example.com" }
};
await context.Contacts.AddRangeAsync(contacts);

var request = new DetectDuplicateRequest
{
    FirstName = "John",
    LastName = "Smith",
    Email = "john.new@example.com"
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeTrue();
result.TotalDuplicates.Should().BeGreaterOrEqualTo(2);
result.Duplicates.Should().Contain(d => d.EntityId == 1);
result.Duplicates.Should().Contain(d => d.EntityId == 2);
```

---

### TC-CM-008: Detect Duplicates - Case Insensitive
**Test**: `DetectDuplicates_Should_MatchCaseInsensitive_When_EmailsDifferOnlyByCase`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    Email = "John.Doe@Example.COM"
};

var request = new DetectDuplicateRequest
{
    Email = "john.doe@example.com" // Different case, same email
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeTrue();
```

---

### TC-CM-009: Detect Duplicates - Confidence Scoring
**Test**: `DetectDuplicates_Should_CategorizeByConfidence_When_MultipleMatchesFound`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "exact@example.com" }, // Exact match
    new() { Id = 2, FirstName = "Jon", LastName = "Doe", Email = "similar@example.com" }, // Similar
    new() { Id = 3, FirstName = "John", LastName = "Do", Email = "other@example.com" } // Partial
};
await context.Contacts.AddRangeAsync(contacts);

var request = new DetectDuplicateRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "new@example.com"
};
```

**Act**:
```csharp
var result = await manager.DetectDuplicatesAsync(request);
```

**Assert**:
```csharp
result.HasDuplicates.Should().BeTrue();
result.HighConfidence.Should().BeGreaterThan(0);
result.MediumConfidence.Should().BeGreaterOrEqualTo(0);
result.TopDuplicate.Should().NotBeNull();
result.TopDuplicate.Score.Should().BeGreaterThan(0.8);
```

---

### TC-CM-010: Detect Duplicates - Import Workflow
**Test**: `DetectDuplicates_Should_WorkInBulk_When_ImportingMultipleContacts`

**Arrange**:
```csharp
var existingContacts = new List<Contact>
{
    new() { Id = 1, Email = "existing1@example.com" },
    new() { Id = 2, Email = "existing2@example.com" }
};
await context.Contacts.AddRangeAsync(existingContacts);

var importRequests = new List<DetectDuplicateRequest>
{
    new() { Email = "existing1@example.com" }, // Duplicate
    new() { Email = "new1@example.com" },      // Unique
    new() { Email = "existing2@example.com" }, // Duplicate
    new() { Email = "new2@example.com" }       // Unique
};
```

**Act**:
```csharp
var results = new List<DuplicateDetectionResult>();
foreach (var request in importRequests)
{
    results.Add(await manager.DetectDuplicatesAsync(request));
}
```

**Assert**:
```csharp
results.Count(r => r.HasDuplicates).Should().Be(2); // 2 duplicates
results.Count(r => !r.HasDuplicates).Should().Be(2); // 2 unique
```

---

## 2. Advanced Search Tests (CRITICAL - PNO-677)

### Purpose
Prevent regression of PNO-677 defect where certain fields didn't work in advanced search.

### TC-CM-011: Search by First Name - Equals
**Test**: `AdvancedSearch_Should_FindExactMatch_When_FirstNameEquals`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "Adam", LastName = "Smith" },
    new() { Id = 2, FirstName = "Adams", LastName = "Jones" },
    new() { Id = 3, FirstName = "Bob", LastName = "Brown" }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "firstName", @operator = "eq", value = "Adam", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(1);
result.Records.First().FirstName.Should().Be("Adam");
```

**Priority**: **CRITICAL** - This test prevents PNO-677 recurrence (equals operator)

---

### TC-CM-012: Search by First Name - Contains
**Test**: `AdvancedSearch_Should_FindPartialMatch_When_FirstNameContains`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "Adam", LastName = "Smith" },
    new() { Id = 2, FirstName = "Adams", LastName = "Jones" },
    new() { Id = 3, FirstName = "Bob", LastName = "Brown" }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "firstName", @operator = "contains", value = "Adam", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2); // Both "Adam" and "Adams"
result.Records.Should().Contain(c => c.FirstName == "Adam");
result.Records.Should().Contain(c => c.FirstName == "Adams");
```

---

### TC-CM-013: Search by Full Name
**Test**: `AdvancedSearch_Should_SearchFullName_When_FullNameFieldUsed`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Doe" },
    new() { Id = 2, FirstName = "Jane", LastName = "Doe" },
    new() { Id = 3, FirstName = "John", LastName = "Smith" }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "fullName", @operator = "contains", value = "John", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().AllSatisfy(c => 
    c.FirstName.Contains("John") || c.LastName.Contains("John"));
```

---

### TC-CM-014: Search by Partner Name (Related Entity)
**Test**: `AdvancedSearch_Should_FilterByPartnerName_When_RelatedEntitySearched`

**Arrange**:
```csharp
var partner1 = new Partner { Id = 1, Name = "UNICEF" };
var partner2 = new Partner { Id = 2, Name = "UNESCO" };

var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", PartnerId = 1, Partner = partner1 },
    new() { Id = 2, FirstName = "Jane", PartnerId = 2, Partner = partner2 },
    new() { Id = 3, FirstName = "Bob", PartnerId = 1, Partner = partner1 }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "partner.name", @operator = "eq", value = "UNICEF", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(c => c.Partner.Name == "UNICEF");
```

---

### TC-CM-015: Search by Email - Equals vs Contains
**Test**: `AdvancedSearch_Should_DifferentiateBetweenEqualsAndContains_When_EmailSearched`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, Email = "john@example.com" },
    new() { Id = 2, Email = "john.doe@example.com" },
    new() { Id = 3, Email = "contact@john.com" }
};

var equalsFilter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "email", @operator = "eq", value = "john@example.com", fieldType = "text" }
    }
};

var containsFilter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "email", @operator = "contains", value = "john", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var equalsResult = await manager.GetContactsAsync(user, equalsFilter);
var containsResult = await manager.GetContactsAsync(user, containsFilter);
```

**Assert**:
```csharp
equalsResult.Records.Should().HaveCount(1); // Exact match only
equalsResult.Records.First().Email.Should().Be("john@example.com");

containsResult.Records.Should().HaveCount(3); // All containing "john"
```

**Priority**: **CRITICAL** - This test prevents PNO-677 recurrence

---

### TC-CM-016: Search by Multiple Fields (AND)
**Test**: `AdvancedSearch_Should_CombineFiltersWithAND_When_MultipleFiltersProvided`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Doe", Title = "Manager" },
    new() { Id = 2, FirstName = "John", LastName = "Smith", Title = "Director" },
    new() { Id = 3, FirstName = "Jane", LastName = "Doe", Title = "Manager" }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "firstName", @operator = "eq", value = "John", logicalOperator = "AND", fieldType = "text" },
        new() { field = "title", @operator = "eq", value = "Manager", logicalOperator = "AND", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(1);
result.Records.First().FirstName.Should().Be("John");
result.Records.First().Title.Should().Be("Manager");
```

---

### TC-CM-017: Search by Multiple Fields (OR)
**Test**: `AdvancedSearch_Should_CombineFiltersWithOR_When_OROperatorSpecified`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Doe" },
    new() { Id = 2, FirstName = "Jane", LastName = "Smith" },
    new() { Id = 3, FirstName = "Bob", LastName = "Doe" }
};

var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "firstName", @operator = "eq", value = "John", logicalOperator = "OR", fieldType = "text" },
        new() { field = "lastName", @operator = "eq", value = "Doe", logicalOperator = "OR", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2); // John Doe and Bob Doe
```

---

### TC-CM-018: Search with Invalid Field
**Test**: `AdvancedSearch_Should_IgnoreInvalidFields_When_FieldNotInAllowedList`

**Arrange**:
```csharp
var filter = new ContactFilterRequest
{
    AdvancedSearchFilters = new List<SearchFilter>
    {
        new() { field = "invalidField", @operator = "eq", value = "test", fieldType = "text" }
    }
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
// Should not throw exception, just ignore invalid field
result.Should().NotBeNull();
```

---

## 3. CRUD Operation Tests

### TC-CM-019: Create Contact - Success
**Test**: `CreateContact_Should_ReturnCreatedContact_When_ValidDataProvided`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    Phone = "+1234567890",
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.FirstName.Should().Be("John");
result.LastName.Should().Be("Doe");
result.Email.Should().Be("john.doe@example.com");
result.Id.Should().BeGreaterThan(0);
result.ContactNumber.Should().NotBeNullOrEmpty(); // Auto-generated
```

---

### TC-CM-020: Create Contact - Name Required
**Test**: `CreateContact_Should_ThrowException_When_FirstNameMissing`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "", // Empty
    LastName = "Doe",
    Email = "test@example.com"
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*first name*required*");
```

---

### TC-CM-021: Create Contact - Email Validation
**Test**: `CreateContact_Should_ThrowException_When_EmailFormatInvalid`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "invalid-email-format" // Invalid format
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*email*invalid*");
```

---

### TC-CM-022: Update Contact - Success
**Test**: `UpdateContact_Should_UpdateFields_When_ValidDataProvided`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com"
};
await context.Contacts.AddAsync(contact);

var request = new UpdateContactRequest
{
    FirstName = "Jane",
    Phone = "+9876543210"
};
```

**Act**:
```csharp
var result = await manager.UpdateContactAsync(user, 1, request);
```

**Assert**:
```csharp
result.FirstName.Should().Be("Jane");
result.LastName.Should().Be("Doe"); // Unchanged
result.Phone.Should().Be("+9876543210");
```

---

### TC-CM-023: Update Contact - Not Found
**Test**: `UpdateContact_Should_ReturnNull_When_ContactDoesNotExist`

**Arrange**:
```csharp
var request = new UpdateContactRequest { FirstName = "Test" };
```

**Act**:
```csharp
var result = await manager.UpdateContactAsync(user, 999, request);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-CM-024: Get Contact By ID
**Test**: `GetContactById_Should_ReturnContact_When_ContactExists`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe"
};
await context.Contacts.AddAsync(contact);
```

**Act**:
```csharp
var result = await manager.GetContactByIdAsync(user, 1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Id.Should().Be(1);
result.FirstName.Should().Be("John");
```

---

### TC-CM-025: Get Contact - Deleted Not Returned
**Test**: `GetContactById_Should_ReturnNull_When_ContactIsDeleted`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    FirstName = "Deleted",
    IsDeleted = true
};
await context.Contacts.AddAsync(contact);
```

**Act**:
```csharp
var result = await manager.GetContactByIdAsync(user, 1);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-CM-026: Delete Contact - Soft Delete
**Test**: `DeleteContact_Should_SoftDelete_When_ValidContactProvided`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    FirstName = "John",
    IsDeleted = false
};
await context.Contacts.AddAsync(contact);
```

**Act**:
```csharp
await manager.DeleteContactAsync(user, 1);
```

**Assert**:
```csharp
var deleted = await context.Contacts.FindAsync(1);
deleted.IsDeleted.Should().BeTrue();
deleted.DeletedDate.Should().NotBeNull();
deleted.DeletedBy.Should().NotBeNull();
```

---

## 4. Contact-Partner Relationship Tests

### TC-CM-027: Create Contact with Partner
**Test**: `CreateContact_Should_AssociateWithPartner_When_PartnerIdProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Test Partner" };
await context.Partners.AddAsync(partner);

var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
result.PartnerId.Should().Be(1);
result.Partner.Should().NotBeNull();
result.Partner.Name.Should().Be("Test Partner");
```

---

### TC-CM-028: Create Contact - Invalid Partner ID
**Test**: `CreateContact_Should_ThrowException_When_PartnerIdDoesNotExist`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    PartnerId = 999 // Non-existent partner
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*partner*not found*");
```

---

### TC-CM-029: Update Contact - Change Partner
**Test**: `UpdateContact_Should_ChangePartner_When_NewPartnerIdProvided`

**Arrange**:
```csharp
var partner1 = new Partner { Id = 1, Name = "Partner 1" };
var partner2 = new Partner { Id = 2, Name = "Partner 2" };
await context.Partners.AddRangeAsync(partner1, partner2);

var contact = new Contact
{
    Id = 1,
    FirstName = "John",
    PartnerId = 1
};
await context.Contacts.AddAsync(contact);

var request = new UpdateContactRequest { PartnerId = 2 };
```

**Act**:
```csharp
var result = await manager.UpdateContactAsync(user, 1, request);
```

**Assert**:
```csharp
result.PartnerId.Should().Be(2);
result.Partner.Name.Should().Be("Partner 2");
```

---

## 5. Pagination and Filtering Tests

### TC-CM-030: Get All Contacts
**Test**: `GetContacts_Should_ReturnAllActiveContacts_When_NoFilterProvided`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "Contact1", IsDeleted = false },
    new() { Id = 2, FirstName = "Contact2", IsDeleted = false },
    new() { Id = 3, FirstName = "Contact3", IsDeleted = true } // Should be excluded
};
await context.Contacts.AddRangeAsync(contacts);
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, new ContactFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().NotContain(c => c.IsDeleted);
```

---

### TC-CM-031: Pagination
**Test**: `GetContacts_Should_ReturnPagedResults_When_PaginationParametersProvided`

**Arrange**:
```csharp
var contacts = Enumerable.Range(1, 25)
    .Select(i => new Contact { Id = i, FirstName = $"Contact{i}", Email = $"contact{i}@example.com" })
    .ToList();
await context.Contacts.AddRangeAsync(contacts);

var filter = new ContactFilterRequest
{
    PageNumber = 2,
    PageSize = 10
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(10);
result.TotalCount.Should().Be(25);
result.PageNumber.Should().Be(2);
result.PageSize.Should().Be(10);
```

---

### TC-CM-032: Filter By Partner
**Test**: `GetContacts_Should_FilterByPartner_When_PartnerIdProvided`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "C1", PartnerId = 1 },
    new() { Id = 2, FirstName = "C2", PartnerId = 2 },
    new() { Id = 3, FirstName = "C3", PartnerId = 1 }
};

var filter = new ContactFilterRequest
{
    PartnerId = 1
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(c => c.PartnerId == 1);
```

---

### TC-CM-033: Search By Text
**Test**: `GetContacts_Should_SearchMultipleFields_When_SearchTextProvided`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "John", LastName = "Smith", Email = "john@example.com" },
    new() { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com" },
    new() { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob@john.com" }
};

var filter = new ContactFilterRequest
{
    SearchText = "john"
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2); // John Smith and bob@john.com
result.Records.Should().Contain(c => c.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase) || 
                                      c.LastName.Contains("John", StringComparison.OrdinalIgnoreCase) ||
                                      c.Email.Contains("john", StringComparison.OrdinalIgnoreCase));
```

---

### TC-CM-034: Sorting
**Test**: `GetContacts_Should_SortByField_When_SortParametersProvided`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "Charlie", LastName = "Brown" },
    new() { Id = 2, FirstName = "Alice", LastName = "Smith" },
    new() { Id = 3, FirstName = "Bob", LastName = "Jones" }
};

var filter = new ContactFilterRequest
{
    OrderBy = "FirstName",
    Ascending = true
};
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, filter);
```

**Assert**:
```csharp
result.Records.Should().HaveCount(3);
result.Records[0].FirstName.Should().Be("Alice");
result.Records[1].FirstName.Should().Be("Bob");
result.Records[2].FirstName.Should().Be("Charlie");
```

---

## 6. Validation Tests

### TC-CM-035: Email Format Validation
**Test**: `CreateContact_Should_ValidateEmailFormat_When_EmailProvided`

**Arrange**:
```csharp
var invalidEmails = new[] { "invalid", "test@", "@example.com", "test@.com", "test..test@example.com" };
```

**Act & Assert**:
```csharp
foreach (var email in invalidEmails)
{
    var request = new CreateContactRequest
    {
        FirstName = "Test",
        LastName = "User",
        Email = email
    };
    
    Func<Task> act = async () => await manager.CreateContactAsync(user, request);
    await act.Should().ThrowAsync<BusinessException>()
        .WithMessage("*email*invalid*", $"Email '{email}' should be invalid");
}
```

---

### TC-CM-036: Phone Number Validation
**Test**: `UpdateContact_Should_ValidatePhoneFormat_When_PhoneProvided`

**Arrange**:
```csharp
var contact = new Contact { Id = 1, FirstName = "John", Email = "john@example.com" };

var invalidPhones = new[] { "abc", "123", "+", "123-456-789-0123-4567" };
```

**Act & Assert**:
```csharp
foreach (var phone in invalidPhones)
{
    var request = new UpdateContactRequest { Phone = phone };
    
    Func<Task> act = async () => await manager.UpdateContactAsync(user, 1, request);
    await act.Should().ThrowAsync<BusinessException>()
        .WithMessage("*phone*invalid*");
}
```

---

### TC-CM-037: Required Field Validation
**Test**: `CreateContact_Should_RequireEitherFirstOrLastName_When_CreatingContact`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "", // Empty
    LastName = "",  // Empty
    Email = "test@example.com"
};
```

**Act**:
```csharp
Func<Task> act = async () => await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<BusinessException>()
    .WithMessage("*name*required*");
```

---

## 7. Permission/RBAC Tests

### TC-CM-038: User Can Only See Own Org Unit Contacts
**Test**: `GetContacts_Should_FilterByOrgUnit_When_UserHasLimitedAccess`

**Arrange**:
```csharp
var partner1 = new Partner { Id = 1, OrgUnitId = 100 };
var partner2 = new Partner { Id = 2, OrgUnitId = 200 };

var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "C1", PartnerId = 1, Partner = partner1 },
    new() { Id = 2, FirstName = "C2", PartnerId = 2, Partner = partner2 },
    new() { Id = 3, FirstName = "C3", PartnerId = 1, Partner = partner1 }
};

var user = CreateUserWithOrgUnitAccess(orgUnitId: 100);
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(user, new ContactFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(2);
result.Records.Should().OnlyContain(c => c.Partner.OrgUnitId == 100);
```

---

### TC-CM-039: Global Admin Can See All Contacts
**Test**: `GetContacts_Should_ReturnAllContacts_When_UserIsGlobalAdmin`

**Arrange**:
```csharp
var contacts = new List<Contact>
{
    new() { Id = 1, FirstName = "C1", PartnerId = 1 },
    new() { Id = 2, FirstName = "C2", PartnerId = 2 },
    new() { Id = 3, FirstName = "C3", PartnerId = 3 }
};
await context.Contacts.AddRangeAsync(contacts);

var admin = CreateGlobalAdmin();
```

**Act**:
```csharp
var result = await manager.GetContactsAsync(admin, new ContactFilterRequest());
```

**Assert**:
```csharp
result.Records.Should().HaveCount(3);
```

---

### TC-CM-040: Cannot Delete Contact - Insufficient Permission
**Test**: `DeleteContact_Should_ThrowUnauthorized_When_UserLacksPermission`

**Arrange**:
```csharp
var contact = new Contact { Id = 1, FirstName = "John" };
var userWithoutPermission = CreateUserWithoutDeletePermission();
```

**Act**:
```csharp
Func<Task> act = async () => await manager.DeleteContactAsync(userWithoutPermission, 1);
```

**Assert**:
```csharp
await act.Should().ThrowAsync<UnauthorizedAccessException>();
```

---

## 8. Contact Number Generation Tests

### TC-CM-041: Auto-Generate Contact Number
**Test**: `CreateContact_Should_GenerateContactNumber_When_ContactCreated`

**Arrange**:
```csharp
var request = new CreateContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com"
};
```

**Act**:
```csharp
var result = await manager.CreateContactAsync(user, request);
```

**Assert**:
```csharp
result.ContactNumber.Should().NotBeNullOrEmpty();
result.ContactNumber.Should().MatchRegex(@"^CON-\d+$"); // Format: CON-001
```

---

### TC-CM-042: Contact Number Uniqueness
**Test**: `CreateContact_Should_GenerateUniqueContactNumbers_When_MultipleContactsCreated`

**Arrange**:
```csharp
var requests = Enumerable.Range(1, 10)
    .Select(i => new CreateContactRequest
    {
        FirstName = $"Contact{i}",
        LastName = "Test",
        Email = $"contact{i}@example.com"
    })
    .ToList();
```

**Act**:
```csharp
var results = new List<ContactModel>();
foreach (var request in requests)
{
    results.Add(await manager.CreateContactAsync(user, request));
}
```

**Assert**:
```csharp
var contactNumbers = results.Select(r => r.ContactNumber).ToList();
contactNumbers.Should().OnlyHaveUniqueItems();
contactNumbers.Should().AllSatisfy(cn => cn.Should().NotBeNullOrEmpty());
```

---

## 9. Mapping Tests

### TC-CM-043: Entity to Model Mapping
**Test**: `MapEntityToModel_Should_MapAllFields_When_ValidContactProvided`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    Salutation = "Mr.",
    FirstName = "John",
    MiddleName = "Q",
    LastName = "Doe",
    Suffix = "Jr.",
    Title = "CEO",
    Department = "Executive",
    Email = "john@example.com",
    Phone = "+1234567890",
    Mobile = "+0987654321"
};
```

**Act**:
```csharp
var model = mapper.Map<ContactModel>(contact);
```

**Assert**:
```csharp
model.Id.Should().Be(1);
model.Salutation.Should().Be("Mr.");
model.FirstName.Should().Be("John");
model.MiddleName.Should().Be("Q");
model.LastName.Should().Be("Doe");
model.Suffix.Should().Be("Jr.");
model.Title.Should().Be("CEO");
model.Email.Should().Be("john@example.com");
```

---

## 10. Business Logic Tests

### TC-CM-044: Full Name Composition
**Test**: `GetContact_Should_ComposeFullName_When_ContactRetrieved`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    Salutation = "Dr.",
    FirstName = "Jane",
    MiddleName = "Marie",
    LastName = "Smith",
    Suffix = "PhD"
};
await context.Contacts.AddAsync(contact);
```

**Act**:
```csharp
var result = await manager.GetContactByIdAsync(user, 1);
```

**Assert**:
```csharp
result.FullName.Should().Be("Dr. Jane Marie Smith, PhD");
// Or whatever the composition logic is
```

---

### TC-CM-045: Contact Status Management
**Test**: `UpdateContact_Should_ChangeStatus_When_StatusUpdateProvided`

**Arrange**:
```csharp
var contact = new Contact
{
    Id = 1,
    FirstName = "John",
    Status = EntityStatus.Active
};

var request = new UpdateContactRequest
{
    Status = EntityStatus.Inactive
};
```

**Act**:
```csharp
var result = await manager.UpdateContactAsync(user, 1, request);
```

**Assert**:
```csharp
result.Status.Should().Be(EntityStatus.Inactive);
```

---

## 11. Import Workflow Tests (PNO-676 Related)

### TC-CM-046: Import Single Contact
**Test**: `ImportContact_Should_CreateContact_When_ValidDataProvided`

**Arrange**:
```csharp
var importRequest = new ImportContactRequest
{
    FirstName = "Imported",
    LastName = "Contact",
    Email = "imported@example.com"
};
```

**Act**:
```csharp
var result = await manager.ImportContactAsync(user, importRequest);
```

**Assert**:
```csharp
result.Success.Should().BeTrue();
result.CreatedContact.Should().NotBeNull();
result.CreatedContact.FirstName.Should().Be("Imported");
```

---

### TC-CM-047: Import with Duplicate Check
**Test**: `ImportContact_Should_DetectDuplicate_When_SimilarContactExists`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com"
};
await context.Contacts.AddAsync(existingContact);

var importRequest = new ImportContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    ConfirmDuplicateCreation = false
};
```

**Act**:
```csharp
var result = await manager.ImportContactAsync(user, importRequest);
```

**Assert**:
```csharp
result.Success.Should().BeFalse();
result.Action.Should().Be("duplicateConfirmation");
result.DuplicateInfo.Should().NotBeNull();
result.DuplicateInfo.TotalDuplicates.Should().BeGreaterThan(0);
```

---

### TC-CM-048: Import with Duplicate Confirmation
**Test**: `ImportContact_Should_CreateDuplicate_When_ConfirmationProvided`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    FirstName = "John",
    Email = "john@example.com"
};

var importRequest = new ImportContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    ConfirmDuplicateCreation = true // User confirmed
};
```

**Act**:
```csharp
var result = await manager.ImportContactAsync(user, importRequest);
```

**Assert**:
```csharp
result.Success.Should().BeTrue();
result.CreatedContact.Should().NotBeNull();
```

---

### TC-CM-049: Bulk Import - Partial Success
**Test**: `ImportContacts_Should_ReturnPartialSuccess_When_SomeContactsInvalid`

**Arrange**:
```csharp
var importRequests = new List<ImportContactRequest>
{
    new() { FirstName = "John", LastName = "Doe", Email = "john@example.com" }, // Valid
    new() { FirstName = "", LastName = "Smith", Email = "invalid" }, // Invalid
    new() { FirstName = "Jane", LastName = "Doe", Email = "jane@example.com" }  // Valid
};
```

**Act**:
```csharp
var result = await manager.ImportContactsAsync(user, importRequests);
```

**Assert**:
```csharp
result.SuccessCount.Should().Be(2);
result.FailureCount.Should().Be(1);
result.Errors.Should().HaveCount(1);
```

---

### TC-CM-050: Import - Partner Assignment
**Test**: `ImportContact_Should_AssignToPartner_When_PartnerIdentifierProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Test Partner", ErpDimValue = 1500 };
await context.Partners.AddAsync(partner);

var importRequest = new ImportContactRequest
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    PartnerErpDimValue = 1500 // Identify partner by ErpDimValue
};
```

**Act**:
```csharp
var result = await manager.ImportContactAsync(user, importRequest);
```

**Assert**:
```csharp
result.CreatedContact.PartnerId.Should().Be(1);
result.CreatedContact.Partner.ErpDimValue.Should().Be(1500);
```

---

## Test Data Factories

### Contact Test Data Factory

```csharp
public class ContactTestDataFactory
{
    private int _sequenceNumber = 1;

    public Contact CreateContact(Action<Contact>? customize = null)
    {
        var contact = new Contact
        {
            Id = _sequenceNumber++,
            FirstName = $"FirstName{_sequenceNumber}",
            LastName = $"LastName{_sequenceNumber}",
            Email = $"contact{_sequenceNumber}@example.com",
            Phone = $"+123456789{_sequenceNumber:D2}",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            IsDeleted = false
        };

        customize?.Invoke(contact);
        return contact;
    }

    public Contact CreateContactWithPartner(int partnerId, string partnerName = "Test Partner")
    {
        return CreateContact(c =>
        {
            c.PartnerId = partnerId;
            c.Partner = new Partner { Id = partnerId, Name = partnerName };
        });
    }

    public Contact CreateDeletedContact()
    {
        return CreateContact(c =>
        {
            c.IsDeleted = true;
            c.DeletedDate = DateTime.UtcNow;
            c.DeletedBy = "TestUser";
        });
    }

    public List<Contact> CreateContactsWithSameEmail(string email, int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateContact(c => c.Email = email))
            .ToList();
    }

    public Contact CreateContactForDuplicateTesting(string firstName, string lastName, string email)
    {
        return CreateContact(c =>
        {
            c.FirstName = firstName;
            c.LastName = lastName;
            c.Email = email;
        });
    }
}
```

---

## Test Helpers

### Duplicate Detection Helper

```csharp
public static class DuplicateDetectionHelper
{
    public static bool IsDuplicateDetected(DuplicateDetectionResult result)
    {
        return result.HasDuplicates && result.TotalDuplicates > 0;
    }

    public static bool IsHighConfidenceDuplicate(DuplicateDetectionResult result)
    {
        return result.HighConfidence > 0;
    }

    public static DuplicateMatch GetTopDuplicate(DuplicateDetectionResult result)
    {
        return result.TopDuplicate ?? result.Duplicates.OrderByDescending(d => d.Score).First();
    }
}
```

---

## Test Suite Execution

### Running Tests

```bash
# Run all ContactManager tests
dotnet test --filter "FullyQualifiedName~ContactManagerTests"

# Run duplicate detection tests only (critical for PNO-676)
dotnet test --filter "FullyQualifiedName~ContactManagerTests.DetectDuplicates"

# Run advanced search tests only (critical for PNO-677)
dotnet test --filter "FullyQualifiedName~ContactManagerTests.AdvancedSearch"

# Run with coverage
dotnet test --filter "FullyQualifiedName~ContactManagerTests" /p:CollectCoverage=true
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **Duplicate Detection** | 10 tests | 100% (CRITICAL) |
| **Advanced Search** | 8 tests | 95% (CRITICAL) |
| **CRUD Operations** | 8 tests | 85% |
| **Validation** | 5 tests | 90% |
| **Permissions/RBAC** | 3 tests | 80% |
| **Import Workflow** | 5 tests | 85% |
| **Pagination/Filtering** | 5 tests | 80% |
| **Contact Number Generation** | 2 tests | 90% |
| **Mapping** | 1 test | 85% |
| **Business Logic** | 3 tests | 85% |
| **Overall** | **50+ tests** | **90%+** |

---

## Priority Test Implementation Order

### Phase 1: Critical (Week 2) - Defect Prevention
1. TC-CM-002: Duplicate detection with ID exclusion (PNO-676)
2. TC-CM-006: Duplicate detection after edit (PNO-676)
3. TC-CM-011: Search FirstName equals (PNO-677)
4. TC-CM-012: Search FirstName contains (PNO-677)
5. TC-CM-015: Search Email equals vs contains (PNO-677)

**Goal**: Prevent PNO-676 and PNO-677 recurrence

### Phase 2: High Priority (Week 2-3) - Core Functionality
6. TC-CM-001: Duplicate on create
7. TC-CM-019: Create contact success
8. TC-CM-022: Update contact
9. TC-CM-024: Get by ID
10. TC-CM-026: Delete contact

**Goal**: Cover main CRUD operations

### Phase 3: Medium Priority (Week 3-4) - Advanced Features
11-30. Remaining duplicate, search, and import tests
31-40. Validation, permissions, business logic tests

**Goal**: Comprehensive coverage

---

## Integration with Import Dialog

### Tests for Import Workflow State Management

These tests validate the integration between ContactManager and the import dialog component:

### TC-CM-051: Import Dialog - Initial Duplicate Detection
**Test**: `DetectDuplicatesForImport_Should_FlagDuplicates_When_ImportFileLoaded`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    Email = "existing@example.com"
};

var importRecords = new List<ImportContactRequest>
{
    new() { Email = "existing@example.com" }, // Duplicate
    new() { Email = "new@example.com" }        // Unique
};
```

**Act**:
```csharp
var results = new List<DuplicateDetectionResult>();
foreach (var record in importRecords)
{
    results.Add(await manager.DetectDuplicatesAsync(record));
}
```

**Assert**:
```csharp
results[0].HasDuplicates.Should().BeTrue();  // First is duplicate
results[1].HasDuplicates.Should().BeFalse(); // Second is unique
```

---

### TC-CM-052: Import Dialog - Re-check After Edit
**Test**: `DetectDuplicatesForImport_Should_ReCheck_When_RecordEditedInline`

**Arrange**:
```csharp
var existingContact = new Contact
{
    Id = 1,
    Email = "existing@example.com"
};

// Original import record (duplicate)
var originalRecord = new DetectDuplicateRequest
{
    Email = "existing@example.com"
};

// Edited record (now unique)
var editedRecord = new DetectDuplicateRequest
{
    Email = "edited@different.com"
};
```

**Act**:
```csharp
var originalResult = await manager.DetectDuplicatesAsync(originalRecord);
var editedResult = await manager.DetectDuplicatesAsync(editedRecord);
```

**Assert**:
```csharp
originalResult.HasDuplicates.Should().BeTrue();  // Before edit
editedResult.HasDuplicates.Should().BeFalse();   // After edit
```

**Priority**: **CRITICAL** - This prevents PNO-676 recurrence

---

## Mock Setup Examples

### DbContext Mock for Contact Tests

```csharp
public class ContactManagerTestBase
{
    protected Mock<AppDbContext> MockContext;
    protected Mock<DbSet<Contact>> MockContactSet;
    protected Mock<DbSet<Partner>> MockPartnerSet;
    protected Mock<IMapper> MockMapper;
    protected UNOPSContactManager Manager;

    public ContactManagerTestBase()
    {
        MockContext = new Mock<AppDbContext>();
        MockContactSet = new Mock<DbSet<Contact>>();
        MockPartnerSet = new Mock<DbSet<Partner>>();
        MockMapper = new Mock<IMapper>();

        MockContext.Setup(x => x.Contacts).Returns(MockContactSet.Object);
        MockContext.Setup(x => x.Partners).Returns(MockPartnerSet.Object);

        Manager = new UNOPSContactManager(
            MockContext.Object,
            MockMapper.Object,
            // ... other dependencies
        );
    }

    protected void SetupContactsQueryable(List<Contact> contacts)
    {
        var queryable = contacts.AsQueryable();
        MockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Provider).Returns(queryable.Provider);
        MockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.Expression).Returns(queryable.Expression);
        MockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.ElementType).Returns(queryable.ElementType);
        MockContactSet.As<IQueryable<Contact>>()
            .Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
    }
}
```

---

**End of UNOPSContactManager Unit Test Cases**

**Next Steps**:
1. Implement test base class
2. Create test data factory
3. Write tests in priority order
4. Achieve 90%+ coverage
5. Verify PNO-676 and PNO-677 prevention

