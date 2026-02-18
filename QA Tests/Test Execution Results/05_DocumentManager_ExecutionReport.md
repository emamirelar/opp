# DocumentManager - Test Execution Report

**Manager**: `DocumentManager`  
**Location**: `UNOPS.PAO.Business/Managers/DocumentManager.cs`  
**Test Specification**: `Test Cases/Business/DocumentManager/DocumentManager_TestCases.md`  
**Execution Date**: November 11, 2025  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Executive Summary

**Total Test Cases**: 45  
**Test Categories**: Functional (20), Performance (10), Concurrency (10), Edge Cases (5)  
**Implementation Status**: ⚠️ Awaiting Implementation  
**Priority**: 🟡 **MEDIUM** (Document Management & Relationships)

---

## Test Categories Breakdown

### 1. Functional Tests (20 cases)

#### TC-DM-F001-F009: Document Retrieval and Relationships
- **F001**: List documents for entity (partner, contact, interaction) ✅ Spec Complete
- **F002**: List documents with empty result ✅ Spec Complete
- **F003**: Filter out deleted documents ✅ Spec Complete
- **F004**: Filter out folders from document list ✅ Spec Complete
- **F005**: Get document by ID - exists ✅ Spec Complete
- **F006**: Get document by ID - not found ✅ Spec Complete
- **F007**: Get document with DocumentType relationship ✅ Spec Complete
- **F008**: Get document parent entity information ✅ Spec Complete
- **F009**: Get parent entity - document has no relationship ✅ Spec Complete

**Purpose**: Validate basic document operations and entity relationship queries.

**Implementation Notes**:
```csharp
// Mock DbContext and Document DbSet
// Test document retrieval by entity (Partner, Contact, Interaction)
// Verify soft delete filtering (IsDeleted = false)
// Test parent entity lookup via relationships
```

#### TC-DM-F010-F020: Document Updates and Advanced Operations
- **F010**: Update document metadata (name, description) ✅ Spec Complete
- **F011**: Update document type assignment ✅ Spec Complete
- **F012**: Update non-existent document ✅ Spec Complete
- **F013**: List documents with multiple entity types ✅ Spec Complete
- **F014**: Document with multiple relationships (many-to-many) ✅ Spec Complete
- **F015**: Get document with null DocumentType ✅ Spec Complete
- **F016**: List documents ordered by creation date ✅ Spec Complete
- **F017**: List documents ordered by name ✅ Spec Complete
- **F018**: Document relationship validation ✅ Spec Complete
- **F019**: Get document with corrupt relationship data ✅ Spec Complete
- **F020**: List documents with pagination ✅ Spec Complete

---

### 2. Performance Tests (10 cases)

| Test ID | Scenario | Target | Priority | Status |
|---------|----------|--------|----------|--------|
| **P001** | List Documents - 1000 documents | < 1000ms | 🔴 Critical | ⏳ Pending |
| **P002** | Get Document By ID | < 200ms | 🔴 Critical | ⏳ Pending |
| **P003** | Update Document - Batch 100 | < 2000ms | 🟡 High | ⏳ Pending |
| **P004** | Document Relationship Query | < 500ms with join | 🔴 Critical | ⏳ Pending |
| **P005** | List Documents - 10 entities | < 1500ms | 🟡 High | ⏳ Pending |
| **P006** | Document Type Join Performance | < 300ms | 🟡 High | ⏳ Pending |
| **P007** | Large Document Metadata Update | < 500ms | 🟢 Medium | ⏳ Pending |
| **P008** | Concurrent Document Listing (50 users) | < 2s each | 🔴 Critical | ⏳ Pending |
| **P009** | Get Parent Entity Performance | < 300ms | 🟡 High | ⏳ Pending |
| **P010** | Document Search Performance (10K docs) | < 1000ms | 🔴 Critical | ⏳ Pending |

**Performance Baseline**: DocumentManager handles document retrieval and metadata management. Must maintain sub-second response times for document lists and lookups.

**Critical Performance Paths**:
1. List documents for entity (most frequent operation)
2. Get document by ID (viewing document details)
3. Update document metadata (editing documents)

---

### 3. Concurrency Tests (10 cases)

#### TC-DM-C001-C003: Read Operations Concurrency
- **C001**: Concurrent Document Listing - Same Entity ⏳
  - **Scenario**: 20 threads list documents for same entity simultaneously
  - **Expected**: All return consistent document list
  - **Risk**: Caching issues or query contention
  
- **C002**: Concurrent Updates - Same Document ⏳
  - **Scenario**: 5 threads update same document metadata
  - **Expected**: Consistent final state, optimistic concurrency
  - **Risk**: Lost updates or version conflicts
  
- **C003**: Concurrent Document Type Assignment ⏳
  - **Scenario**: 3 threads assign different types to same document
  - **Expected**: One type assignment wins, consistent state
  - **Risk**: Race conditions in type assignment

#### TC-DM-C004-C007: Write and Query Concurrency
- **C004**: List During Document Creation ⏳
  - **Scenario**: Querying list while documents being added
  - **Expected**: Consistent query results
  - **Risk**: Dirty reads or missing documents
  
- **C005**: Get Document During Update ⏳
  - **Scenario**: Reading document during metadata update
  - **Expected**: Consistent document state
  - **Risk**: Partial updates visible
  
- **C006**: Concurrent Relationship Queries ⏳
  - **Scenario**: 15 threads query document relationships
  - **Expected**: Consistent relationship data
  
- **C007**: Concurrent Parent Entity Lookups ⏳
  - **Scenario**: 10 threads lookup parent entities
  - **Expected**: Correct parent entities returned

#### TC-DM-C008-C010: Complex Concurrency Scenarios
- **C008**: Update During Relationship Modification ⏳
  - **Scenario**: Update metadata while relationship changes
  - **Expected**: Both operations complete successfully
  
- **C009**: Concurrent Soft Deletes ⏳
  - **Scenario**: Multiple threads soft-deleting documents
  - **Expected**: All documents marked as deleted
  
- **C010**: Bulk List Requests ⏳
  - **Scenario**: 30 threads listing different entity documents
  - **Expected**: All lists returned correctly

---

### 4. Edge Cases (5 cases)

| Test ID | Scenario | Expected Behavior | Risk Level | Status |
|---------|----------|-------------------|------------|--------|
| **E001** | Document With Null Entity Relationship | Handle gracefully, return null parent | 🟡 Medium | ⏳ Pending |
| **E002** | Entity With 1000+ Documents | Query completes successfully | 🔴 High | ⏳ Pending |
| **E003** | Document Relationship to Deleted Entity | Detect and handle orphaned relationship | 🔴 High | ⏳ Pending |
| **E004** | Circular Document Relationships | Detect circular references | 🟡 Medium | ⏳ Pending |
| **E005** | Document With Special Characters in Name | Store and retrieve correctly | 🟢 Low | ⏳ Pending |

---

## Risk Assessment

### Critical Risks 🔴

1. **Document-Entity Relationship Integrity**
   - Impact: Orphaned documents, incorrect parent entity lookups
   - Test Coverage: F008-F009, F018-F019, E001, E003
   - Mitigation: Relationship validation tests, orphan detection

2. **Performance with Large Document Collections**
   - Impact: Slow document loading, timeouts
   - Test Coverage: P001, P005, P008, P010, E002
   - Mitigation: Performance benchmarks, pagination, indexing

3. **Soft Delete Filtering**
   - Impact: Deleted documents shown to users
   - Test Coverage: F003, C009
   - Mitigation: Soft delete filter tests, query validation

### Medium Risks 🟡

1. **Document Type Management**
   - Impact: Incorrect document categorization
   - Test Coverage: F007, F011, F015, P006, C003
   - Mitigation: Document type assignment tests

2. **Concurrent Metadata Updates**
   - Impact: Lost document metadata, version conflicts
   - Test Coverage: C002, C005, C008
   - Mitigation: Optimistic concurrency tests

---

## Implementation Checklist

### Setup (30 minutes)
- [ ] Create `DocumentManagerTests.cs` in unit test project
- [ ] Set up test fixtures and mock data
- [ ] Mock `AppDbContext`, `Document` DbSet, `DocumentType` DbSet
- [ ] Create sample documents with various entity relationships

### Functional Tests (2 hours)
- [ ] Implement F001-F009: Document retrieval and relationships
- [ ] Implement F010-F020: Document updates and advanced operations

### Performance Tests (1 hour)
- [ ] Implement P001-P003: Core operation benchmarks
- [ ] Implement P004-P007: Relationship and join benchmarks
- [ ] Implement P008-P010: Concurrent and search benchmarks

### Concurrency Tests (1 hour)
- [ ] Implement C001-C003: Read operations concurrency
- [ ] Implement C004-C007: Write and query concurrency
- [ ] Implement C008-C010: Complex concurrency scenarios

### Edge Cases (30 minutes)
- [ ] Implement E001-E005: All edge case scenarios

---

## Code Coverage Goals

**Target Coverage**: 85%+

### Critical Paths (Must Cover)
✅ ListDocumentsAsync(entityId, entityType)  
✅ GetDocumentByIdAsync(documentId)  
✅ UpdateDocumentAsync(documentId, metadata)  
✅ GetParentEntityAsync(documentId)  
✅ AssignDocumentTypeAsync(documentId, typeId)

### Secondary Paths (Should Cover)
✅ Soft delete filtering (IsDeleted = false)  
✅ Folder filtering  
✅ Ordering by date/name  
✅ Pagination  
✅ Document type relationships  
✅ Multi-entity document queries

---

## Sample Test Implementation

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class DocumentManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<DbSet<Document>> _mockDocumentSet;
    private readonly Mock<DbSet<DocumentType>> _mockDocumentTypeSet;
    private readonly Mock<IMapper> _mockMapper;
    private readonly DocumentManager _sut;

    public DocumentManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockDocumentSet = new Mock<DbSet<Document>>();
        _mockDocumentTypeSet = new Mock<DbSet<DocumentType>>();
        _mockMapper = new Mock<IMapper>();
        
        _mockDbContext.Setup(x => x.Documents).Returns(_mockDocumentSet.Object);
        _mockDbContext.Setup(x => x.DocumentTypes).Returns(_mockDocumentTypeSet.Object);
        
        _sut = new DocumentManager(_mockDbContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task ListDocuments_Should_Filter_Out_Deleted_Documents()
    {
        // Arrange
        var partnerId = 100;
        var documents = new List<Document>
        {
            new() { Id = 1, PartnerId = partnerId, IsDeleted = false, Name = "Doc1" },
            new() { Id = 2, PartnerId = partnerId, IsDeleted = true, Name = "Doc2 (Deleted)" },
            new() { Id = 3, PartnerId = partnerId, IsDeleted = false, Name = "Doc3" }
        }.AsQueryable();

        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Provider).Returns(documents.Provider);
        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Expression).Returns(documents.Expression);

        // Act
        var result = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => !d.IsDeleted);
        result.Should().NotContain(d => d.Id == 2);
    }

    [Fact]
    public async Task ListDocuments_Should_Filter_Out_Folders()
    {
        // Arrange
        var partnerId = 100;
        var documents = new List<Document>
        {
            new() { Id = 1, PartnerId = partnerId, IsFolder = false, Name = "Document.pdf" },
            new() { Id = 2, PartnerId = partnerId, IsFolder = true, Name = "Folder" },
            new() { Id = 3, PartnerId = partnerId, IsFolder = false, Name = "Report.docx" }
        }.AsQueryable();

        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Provider).Returns(documents.Provider);
        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Expression).Returns(documents.Expression);

        // Act
        var result = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => !d.IsFolder);
        result.Should().NotContain(d => d.Id == 2);
    }

    [Fact]
    public async Task ListDocuments_Should_Order_By_Creation_Date_Descending()
    {
        // Arrange
        var partnerId = 100;
        var documents = new List<Document>
        {
            new() { Id = 1, PartnerId = partnerId, CreatedDate = DateTime.UtcNow.AddHours(-2) },
            new() { Id = 2, PartnerId = partnerId, CreatedDate = DateTime.UtcNow },
            new() { Id = 3, PartnerId = partnerId, CreatedDate = DateTime.UtcNow.AddHours(-1) }
        }.AsQueryable();

        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Provider).Returns(documents.Provider);
        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Expression).Returns(documents.Expression);

        // Act
        var result = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner, orderBy: "date_desc");

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(d => d.CreatedDate);
        result.First().Id.Should().Be(2); // Most recent first
    }

    [Fact]
    public async Task GetDocumentById_Should_Include_DocumentType()
    {
        // Arrange
        var documentId = 1;
        var document = new Document
        {
            Id = documentId,
            Name = "Contract.pdf",
            DocumentTypeId = 10,
            DocumentType = new DocumentType { Id = 10, Name = "Contract" }
        };

        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId, includeType: true);

        // Assert
        result.Should().NotBeNull();
        result.DocumentType.Should().NotBeNull();
        result.DocumentType.Name.Should().Be("Contract");
    }

    [Fact]
    public async Task GetDocumentById_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var documentId = 999;
        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync((Document)null);

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateDocument_Should_Update_Metadata()
    {
        // Arrange
        var documentId = 1;
        var document = new Document
        {
            Id = documentId,
            Name = "Old Name",
            Description = "Old Description"
        };

        var updateRequest = new DocumentUpdateRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync(document);

        // Act
        await _sut.UpdateDocumentAsync(documentId, updateRequest);

        // Assert
        document.Name.Should().Be("New Name");
        document.Description.Should().Be("New Description");
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateDocument_Should_Return_False_When_Not_Found()
    {
        // Arrange
        var documentId = 999;
        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync((Document)null);

        var updateRequest = new DocumentUpdateRequest { Name = "New Name" };

        // Act
        var result = await _sut.UpdateDocumentAsync(documentId, updateRequest);

        // Assert
        result.Should().BeFalse();
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task AssignDocumentType_Should_Update_Document_Type()
    {
        // Arrange
        var documentId = 1;
        var newTypeId = 20;
        var document = new Document
        {
            Id = documentId,
            DocumentTypeId = 10
        };

        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync(document);

        // Act
        await _sut.AssignDocumentTypeAsync(documentId, newTypeId);

        // Assert
        document.DocumentTypeId.Should().Be(newTypeId);
        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetParentEntity_Should_Return_Partner()
    {
        // Arrange
        var documentId = 1;
        var partnerId = 100;
        var document = new Document
        {
            Id = documentId,
            PartnerId = partnerId,
            Partner = new Partner { Id = partnerId, Name = "Acme Corp" }
        };

        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _sut.GetParentEntityAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.EntityType.Should().Be(EntityType.Partner);
        result.EntityId.Should().Be(partnerId);
        result.EntityName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetParentEntity_Should_Return_Null_When_No_Relationship()
    {
        // Arrange
        var documentId = 1;
        var document = new Document
        {
            Id = documentId,
            PartnerId = null,
            ContactId = null,
            InteractionId = null
        };

        _mockDocumentSet.Setup(x => x.FindAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _sut.GetParentEntityAsync(documentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ListDocuments_Should_Handle_Multiple_Entity_Types()
    {
        // Arrange
        var partnerId = 100;
        var contactId = 200;
        
        var documents = new List<Document>
        {
            new() { Id = 1, PartnerId = partnerId, ContactId = null },
            new() { Id = 2, PartnerId = null, ContactId = contactId },
            new() { Id = 3, PartnerId = partnerId, ContactId = null }
        }.AsQueryable();

        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Provider).Returns(documents.Provider);
        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Expression).Returns(documents.Expression);

        // Act
        var partnerDocs = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner);
        var contactDocs = await _sut.ListDocumentsAsync(contactId, EntityType.Contact);

        // Assert
        partnerDocs.Should().HaveCount(2);
        partnerDocs.Should().OnlyContain(d => d.PartnerId == partnerId);
        
        contactDocs.Should().HaveCount(1);
        contactDocs.Should().OnlyContain(d => d.ContactId == contactId);
    }

    [Fact]
    public async Task ListDocuments_Should_Support_Pagination()
    {
        // Arrange
        var partnerId = 100;
        var documents = Enumerable.Range(1, 100)
            .Select(i => new Document { Id = i, PartnerId = partnerId, Name = $"Doc{i}" })
            .AsQueryable();

        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Provider).Returns(documents.Provider);
        _mockDocumentSet.As<IQueryable<Document>>()
            .Setup(m => m.Expression).Returns(documents.Expression);

        // Act
        var page1 = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner, pageSize: 20, pageNumber: 1);
        var page2 = await _sut.ListDocumentsAsync(partnerId, EntityType.Partner, pageSize: 20, pageNumber: 2);

        // Assert
        page1.Should().HaveCount(20);
        page2.Should().HaveCount(20);
        page1.First().Id.Should().NotBe(page2.First().Id); // Different pages
    }
}
```

---

## Execution Timeline

### Week 1: Implementation
- **Day 1**: Functional tests F001-F010
- **Day 2**: Functional tests F011-F020
- **Day 3**: Performance tests P001-P010
- **Day 4**: Concurrency tests C001-C010
- **Day 5**: Edge cases E001-E005 + Review

### Week 2: Validation
- **Day 1**: Execute all tests, fix failures
- **Day 2**: Performance validation with large document sets
- **Day 3**: Relationship integrity testing
- **Day 4**: Code coverage analysis (target: 85%+)
- **Day 5**: Documentation and CI/CD integration

---

## Success Criteria

✅ All 45 test cases implemented  
✅ All tests passing (100% pass rate)  
✅ Code coverage ≥ 85%  
✅ Performance targets met:
  - List 1000 documents < 1000ms
  - Get document by ID < 200ms
  - Update batch 100 < 2000ms  
✅ Soft delete filtering working correctly  
✅ Document-entity relationships validated  
✅ No orphaned documents  
✅ No concurrency issues  
✅ CI/CD integration complete  

---

## Dependencies

### Required Packages
- xUnit (test framework)
- Moq (mocking)
- FluentAssertions (readable assertions)
- Microsoft.EntityFrameworkCore.InMemory (EF Core testing)
- AutoMapper (mapping tests)

### Test Data Requirements
- Sample documents with various entity relationships
- Documents with different types (Contract, Report, Invoice, etc.)
- Large document collections for performance testing (10K+ documents)
- Deleted documents for soft delete filtering tests
- Folders for filtering tests

---

## Related Components

This test suite validates:
- Document CRUD operations
- Document-entity relationship management
- Document type categorization
- Soft delete functionality
- Document metadata management

Impacts:
- Partner document management
- Contact document management
- Interaction document tracking
- Document type system
- Document repository

---

## Next Steps

1. **Create test project infrastructure** (if not exists)
2. **Implement Priority 1: Document retrieval and filtering** (F001-F009)
3. **Implement document updates** (F010-F020)
4. **Run relationship validation tests**
5. **Performance test with large document sets**
6. **Test soft delete filtering thoroughly**
7. **Generate coverage reports**
8. **Document findings**

---

**Report Status**: Specification Complete ✅ | Implementation Pending ⚠️  
**Estimated Implementation Time**: 2.5 hours  
**Priority**: 🟡 MEDIUM (Important for document management, supporting role to main business functions)






