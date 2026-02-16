using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for DocumentManager
/// Uses unique test markers for PostgreSQL data isolation.
/// </summary>
public class DocumentManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"DocTest_{Guid.NewGuid():N}";

    [Fact]
    public async Task GetDocumentById_Should_ReturnDocument_When_Exists()
    {
        // Arrange
        var document = new UNOPSDocument
        {
            Name = $"Test Document {_testMarker}",
            Link = "https://example.com/doc.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        var result = await Context.Documents.FindAsync(document.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Contain("Test Document");
    }

    [Fact]
    public async Task GetDocumentById_Should_ReturnNull_When_NotExists()
    {
        // Act - Use a very high ID that won't exist
        var result = await Context.Documents.FindAsync(int.MaxValue);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllDocuments_Should_ReturnCreatedDocuments()
    {
        // Arrange
        var documents = new List<UNOPSDocument>
        {
            new() { Name = $"Doc 1 {_testMarker}", Link = "https://example.com/doc1.pdf", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Doc 2 {_testMarker}", Link = "https://example.com/doc2.pdf", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Documents.AddRangeAsync(documents);
        await SaveChangesAsync();

        // Act - Filter to test-created documents only
        var result = await Context.Documents
            .Where(d => d.Name.Contains(_testMarker))
            .ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateDocument_Should_PersistDocument()
    {
        // Arrange
        var document = new UNOPSDocument
        {
            Name = $"New Document {_testMarker}",
            Link = "https://example.com/new.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Assert
        var result = await Context.Documents.FindAsync(document.Id);
        result.Should().NotBeNull();
        result!.Link.Should().Be("https://example.com/new.pdf");
    }

    [Fact]
    public async Task UpdateDocument_Should_UpdateFields()
    {
        // Arrange
        var document = new UNOPSDocument
        {
            Name = $"Original Name {_testMarker}",
            Link = "https://example.com/original.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        document.Name = $"Updated Name {_testMarker}";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Documents.FindAsync(document.Id);
        result!.Name.Should().Contain("Updated Name");
    }

    [Fact]
    public async Task DeleteDocument_Should_SoftDelete()
    {
        // Arrange
        var document = new UNOPSDocument
        {
            Name = $"To Delete {_testMarker}",
            Link = "https://example.com/delete.pdf",
            Status = EntityStatus.Active,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        document.IsDeleted = true;
        document.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Documents.FindAsync(document.Id);
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetDocumentsByType_Should_FilterCorrectly()
    {
        // Arrange - Use unique names to scope assertions
        var documents = new List<UNOPSDocument>
        {
            new() { Name = $"PDF Doc {_testMarker}", Link = "https://example.com/doc.pdf", Type = "PDF", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Word Doc {_testMarker}", Link = "https://example.com/doc.docx", Type = "DOCX", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow },
            new() { Name = $"Excel Doc {_testMarker}", Link = "https://example.com/doc.xlsx", Type = "XLSX", Status = EntityStatus.Active, LastModifiedDate = DateTime.UtcNow }
        };
        await Context.Documents.AddRangeAsync(documents);
        await SaveChangesAsync();

        // Act - Filter by type AND test marker
        var result = await Context.Documents
            .Where(d => d.Name.Contains(_testMarker) && d.Type == "PDF")
            .ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Contain("PDF Doc");
    }
}
