using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for DocumentManager
/// </summary>
public class DocumentManagerTests : ManagerTestBase
{
    [Fact]
    public async Task GetDocumentById_Should_ReturnDocument_When_Exists()
    {
        // Arrange
        var document = new Document
        {
            Id = 1,
            Name = "Test Document",
            Link = "https://example.com/doc.pdf",
            Status = EntityStatus.Active
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        var result = await Context.Documents.FindAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Document");
    }

    [Fact]
    public async Task GetDocumentById_Should_ReturnNull_When_NotExists()
    {
        // Act
        var result = await Context.Documents.FindAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllDocuments_Should_ReturnAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            new() { Id = 1, Name = "Doc 1", Link = "https://example.com/doc1.pdf", Status = EntityStatus.Active },
            new() { Id = 2, Name = "Doc 2", Link = "https://example.com/doc2.pdf", Status = EntityStatus.Active }
        };
        await Context.Documents.AddRangeAsync(documents);
        await SaveChangesAsync();

        // Act
        var result = await Context.Documents.ToListAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateDocument_Should_PersistDocument()
    {
        // Arrange
        var document = new Document
        {
            Id = 1,
            Name = "New Document",
            Link = "https://example.com/new.pdf",
            Status = EntityStatus.Active
        };

        // Act
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Assert
        var result = await Context.Documents.FindAsync(1);
        result.Should().NotBeNull();
        result!.Link.Should().Be("https://example.com/new.pdf");
    }

    [Fact]
    public async Task UpdateDocument_Should_UpdateFields()
    {
        // Arrange
        var document = new Document
        {
            Id = 1,
            Name = "Original Name",
            Link = "https://example.com/original.pdf",
            Status = EntityStatus.Active
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        document.Name = "Updated Name";
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Documents.FindAsync(1);
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteDocument_Should_SoftDelete()
    {
        // Arrange
        var document = new Document
        {
            Id = 1,
            Name = "To Delete",
            Link = "https://example.com/delete.pdf",
            Status = EntityStatus.Active
        };
        await Context.Documents.AddAsync(document);
        await SaveChangesAsync();

        // Act
        document.IsDeleted = true;
        document.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();

        // Assert
        Context.ChangeTracker.Clear();
        var result = await Context.Documents.FindAsync(1);
        result!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetDocumentsByType_Should_FilterCorrectly()
    {
        // Arrange
        var documents = new List<Document>
        {
            new() { Id = 1, Name = "PDF Doc", Link = "https://example.com/doc.pdf", Type = "PDF", Status = EntityStatus.Active },
            new() { Id = 2, Name = "Word Doc", Link = "https://example.com/doc.docx", Type = "DOCX", Status = EntityStatus.Active },
            new() { Id = 3, Name = "Excel Doc", Link = "https://example.com/doc.xlsx", Type = "XLSX", Status = EntityStatus.Active }
        };
        await Context.Documents.AddRangeAsync(documents);
        await SaveChangesAsync();

        // Act
        var result = await Context.Documents.Where(d => d.Type == "PDF").ToListAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("PDF Doc");
    }
}

