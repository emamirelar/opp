# DocumentManager - Unit Test Cases

**Manager**: `DocumentManager`  
**File**: `UNOPS.PAO.Business/Managers/DocumentManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `DocumentManager` with focus on:
- Document CRUD operations
- Document listing and filtering
- Document-entity relationships
- Document type handling
- Soft delete behavior

**Total Test Cases**: 25+

---

## 1. Document Listing Tests

### TC-DM-001: List Documents for Entity
**Test**: `ListDocuments_Should_ReturnDocuments_When_ValidEntityProvided`

**Arrange**:
```csharp
var partner = new Partner { Id = 1, Name = "Test Partner" };
var documents = new List<Document>
{
    new() { Id = 1, Name = "Doc1", Type = "file", IsDeleted = false,
           DocumentRelationships = new List<DocumentRelationship>
           {
               new() { EntityType = "Partner", EntityId = 1 }
           }},
    new() { Id = 2, Name = "Doc2", Type = "file", IsDeleted = false,
           DocumentRelationships = new List<DocumentRelationship>
           {
               new() { EntityType = "Partner", EntityId = 1 }
           }}
};
await Context.Documents.AddRangeAsync(documents);
await SaveChangesAsync();
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().AllSatisfy(d => d.Type.Should().NotBe("folder"));
```

---

### TC-DM-002: Exclude Folders from List
**Test**: `ListDocuments_Should_ExcludeFolders_When_TypeIsFolder`

**Arrange**:
```csharp
var documents = new List<Document>
{
    new() { Id = 1, Name = "Doc1", Type = "file",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}},
    new() { Id = 2, Name = "Folder1", Type = "folder", // Should be excluded
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.Should().HaveCount(1);
result.Should().NotContain(d => d.Type == "folder");
```

---

### TC-DM-003: Exclude Deleted Documents
**Test**: `ListDocuments_Should_ExcludeDeleted_When_IsDeletedTrue`

**Arrange**:
```csharp
var documents = new List<Document>
{
    new() { Id = 1, Name = "Active", IsDeleted = false,
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}},
    new() { Id = 2, Name = "Deleted", IsDeleted = true, // Should be excluded
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.Should().HaveCount(1);
result.First().Name.Should().Be("Active");
```

---

### TC-DM-004: Filter by Entity Type
**Test**: `ListDocuments_Should_FilterByEntityType_When_DifferentEntitiesExist`

**Arrange**:
```csharp
var documents = new List<Document>
{
    new() { Id = 1, Name = "PartnerDoc",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}},
    new() { Id = 2, Name = "ContactDoc",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Contact", EntityId = 1 }}} // Different entity type
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.Should().HaveCount(1);
result.First().Name.Should().Be("PartnerDoc");
```

---

### TC-DM-005: Include Document Type
**Test**: `ListDocuments_Should_IncludeDocumentType_When_DocumentTypeExists`

**Arrange**:
```csharp
var docType = new DocumentType { Id = 1, Name = "PDF" };
var document = new Document
{
    Id = 1,
    Name = "Test",
    DocumentTypeId = 1,
    DocumentType = docType,
    DocumentRelationships = new List<DocumentRelationship>
    { new() { EntityType = "Partner", EntityId = 1 }}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.First().DocumentType.Should().NotBeNull();
result.First().DocumentType.Name.Should().Be("PDF");
```

---

## 2. Get Document By ID Tests

### TC-DM-006: Get Existing Document
**Test**: `GetDocumentById_Should_ReturnDocument_When_DocumentExists`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    Name = "Test Document",
    FilePath = "/path/to/file.pdf"
};
await Context.Documents.AddAsync(document);
```

**Act**:
```csharp
var result = await manager.GetDocumentByIdAsync(1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Id.Should().Be(1);
result.Name.Should().Be("Test Document");
```

---

### TC-DM-007: Non-Existent Document Returns Null
**Test**: `GetDocumentById_Should_ReturnNull_When_DocumentDoesNotExist`

**Arrange**:
```csharp
// No document with ID 999
```

**Act**:
```csharp
var result = await manager.GetDocumentByIdAsync(999);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

## 3. Get Document Parent Entity Tests

### TC-DM-008: Get Parent Entity
**Test**: `GetDocumentParentEntity_Should_ReturnEntityInfo_When_RelationshipExists`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    Name = "Test",
    DocumentRelationships = new List<DocumentRelationship>
    {
        new() { EntityType = "Partner", EntityId = 100 }
    }
};
await Context.Documents.AddAsync(document);
```

**Act**:
```csharp
var result = await manager.GetDocumentParentEntityByIdAsync(1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Value.EntityId.Should().Be(100);
result.Value.EntityType.Should().Be("Partner");
```

---

### TC-DM-009: No Parent Entity Returns Null
**Test**: `GetDocumentParentEntity_Should_ReturnNull_When_NoRelationshipExists`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    Name = "Test",
    DocumentRelationships = new List<DocumentRelationship>() // Empty
};
```

**Act**:
```csharp
var result = await manager.GetDocumentParentEntityByIdAsync(1);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-DM-010: Document Not Found Returns Null
**Test**: `GetDocumentParentEntity_Should_ReturnNull_When_DocumentDoesNotExist`

**Arrange**:
```csharp
// No document
```

**Act**:
```csharp
var result = await manager.GetDocumentParentEntityByIdAsync(999);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

## 4. Update Document Tests

### TC-DM-011: Update Document Successfully
**Test**: `UpdateDocument_Should_UpdateFields_When_ValidRequestProvided`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    Name = "Original Name",
    Description = "Original Description"
};
await Context.Documents.AddAsync(document);

var request = new UpdateDocumentRequest
{
    Id = 1,
    Name = "Updated Name",
    Description = "Updated Description"
};
```

**Act**:
```csharp
var result = await manager.UpdateDocumentAsync(request);
```

**Assert**:
```csharp
result.Name.Should().Be("Updated Name");
result.Description.Should().Be("Updated Description");
```

---

### TC-DM-012: Update Non-Existent Document Returns Null
**Test**: `UpdateDocument_Should_ReturnNull_When_DocumentDoesNotExist`

**Arrange**:
```csharp
var request = new UpdateDocumentRequest
{
    Id = 999,
    Name = "Updated"
};
```

**Act**:
```csharp
var result = await manager.UpdateDocumentAsync(request);
```

**Assert**:
```csharp
result.Should().BeNull();
```

---

### TC-DM-013: Update Document Name
**Test**: `UpdateDocument_Should_UpdateName_When_NameProvided`

**Arrange**:
```csharp
var document = new Document { Id = 1, Name = "Old Name" };
var request = new UpdateDocumentRequest { Id = 1, Name = "New Name" };
```

**Act**:
```csharp
var result = await manager.UpdateDocumentAsync(request);
```

**Assert**:
```csharp
result.Name.Should().Be("New Name");
```

---

### TC-DM-014: Update Document Type
**Test**: `UpdateDocument_Should_UpdateDocumentType_When_DocumentTypeIdProvided`

**Arrange**:
```csharp
var docType1 = new DocumentType { Id = 1, Name = "PDF" };
var docType2 = new DocumentType { Id = 2, Name = "Word" };
await Context.DocumentTypes.AddRangeAsync(docType1, docType2);

var document = new Document { Id = 1, DocumentTypeId = 1 };
var request = new UpdateDocumentRequest { Id = 1, DocumentTypeId = 2 };
```

**Act**:
```csharp
var result = await manager.UpdateDocumentAsync(request);
```

**Assert**:
```csharp
result.DocumentTypeId.Should().Be(2);
```

---

## 5. Document Relationship Tests

### TC-DM-015: List Documents for Multiple Entities
**Test**: `ListDocuments_Should_ReturnOnlyMatchingEntity_When_MultipleEntitiesExist`

**Arrange**:
```csharp
var documents = new List<Document>
{
    new() { Id = 1, Name = "Doc1",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}},
    new() { Id = 2, Name = "Doc2",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 2 }}}, // Different entity ID
    new() { Id = 3, Name = "Doc3",
           DocumentRelationships = new List<DocumentRelationship>
           { new() { EntityType = "Partner", EntityId = 1 }}}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().Contain(d => d.Name == "Doc1");
result.Should().Contain(d => d.Name == "Doc3");
```

---

### TC-DM-016: Document with Multiple Relationships
**Test**: `GetDocumentParentEntity_Should_ReturnFirst_When_MultipleRelationshipsExist`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    DocumentRelationships = new List<DocumentRelationship>
    {
        new() { EntityType = "Partner", EntityId = 1 },
        new() { EntityType = "Contact", EntityId = 2 }
    }
};
```

**Act**:
```csharp
var result = await manager.GetDocumentParentEntityByIdAsync(1);
```

**Assert**:
```csharp
result.Should().NotBeNull();
// Should return first relationship
```

---

## 6. Mapping Tests

### TC-DM-017: Map Document to Model
**Test**: `ListDocuments_Should_MapToModel_When_DocumentsExist`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    Name = "Test",
    FilePath = "/path/file.pdf",
    FileSize = 1024,
    DocumentRelationships = new List<DocumentRelationship>
    { new() { EntityType = "Partner", EntityId = 1 }}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
var doc = result.First();
doc.Id.Should().Be(1);
doc.Name.Should().Be("Test");
doc.FilePath.Should().Be("/path/file.pdf");
```

---

## 7. Edge Case Tests

### TC-DM-018: List Documents with No Results
**Test**: `ListDocuments_Should_ReturnEmpty_When_NoDocumentsMatch`

**Arrange**:
```csharp
// No documents for this entity
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 999);
```

**Assert**:
```csharp
result.Should().BeEmpty();
```

---

### TC-DM-019: Case Sensitivity in Entity Type
**Test**: `ListDocuments_Should_MatchEntityType_CaseSensitive`

**Arrange**:
```csharp
var document = new Document
{
    Id = 1,
    DocumentRelationships = new List<DocumentRelationship>
    { new() { EntityType = "Partner", EntityId = 1 }}
};
```

**Act**:
```csharp
var result1 = manager.ListDocumentsAsync("Partner", 1);
var result2 = manager.ListDocumentsAsync("partner", 1); // lowercase
```

**Assert**:
```csharp
result1.Should().HaveCount(1);
result2.Should().BeEmpty(); // Case-sensitive
```

---

## 8. Document Type Tests

### TC-DM-020: List Documents with Document Type
**Test**: `ListDocuments_Should_IncludeDocumentType_WhenIncludeSpecified`

**Arrange**:
```csharp
var docType = new DocumentType { Id = 1, Name = "PDF", Extension = ".pdf" };
var document = new Document
{
    Id = 1,
    DocumentTypeId = 1,
    DocumentType = docType,
    DocumentRelationships = new List<DocumentRelationship>
    { new() { EntityType = "Partner", EntityId = 1 }}
};
```

**Act**:
```csharp
var result = manager.ListDocumentsAsync("Partner", 1);
```

**Assert**:
```csharp
var doc = result.First();
doc.DocumentType.Should().NotBeNull();
doc.DocumentType.Name.Should().Be("PDF");
```

---

## Test Data Factory

```csharp
public class DocumentTestDataFactory
{
    private int _sequenceNumber = 1;

    public Document CreateDocument(Action<Document>? customize = null)
    {
        var document = new Document
        {
            Id = _sequenceNumber++,
            Name = $"Document {_sequenceNumber}",
            FilePath = $"/path/doc{_sequenceNumber}.pdf",
            Type = "file",
            FileSize = 1024 * _sequenceNumber,
            IsDeleted = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "TestUser",
            DocumentRelationships = new List<DocumentRelationship>()
        };

        customize?.Invoke(document);
        return document;
    }

    public Document CreateDocumentForEntity(string entityType, int entityId)
    {
        return CreateDocument(d =>
        {
            d.DocumentRelationships.Add(new DocumentRelationship
            {
                EntityType = entityType,
                EntityId = entityId
            });
        });
    }

    public Document CreateFolder()
    {
        return CreateDocument(d =>
        {
            d.Type = "folder";
            d.FilePath = null;
            d.FileSize = 0;
        });
    }

    public Document CreateDeletedDocument()
    {
        return CreateDocument(d =>
        {
            d.IsDeleted = true;
            d.DeletedDate = DateTime.UtcNow;
            d.DeletedBy = "TestUser";
        });
    }
}
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **Document Listing** | 5 tests | 90% |
| **Get Document** | 2 tests | 85% |
| **Parent Entity** | 3 tests | 85% |
| **Update Document** | 4 tests | 85% |
| **Relationships** | 2 tests | 80% |
| **Mapping** | 1 test | 85% |
| **Edge Cases** | 2 tests | 80% |
| **Document Types** | 1 test | 85% |
| **Overall** | **25+ tests** | **85%+** |

---

**End of DocumentManager Unit Test Cases**

