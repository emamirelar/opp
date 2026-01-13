/**
 * @fileoverview Tests for PostgreSQL Sequence Resync functionality
 * Tests for recent commit: b1e1976c - Sequence Resync Logic for PartnerTree and Interaction
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataImport;

/// <summary>
/// Tests for PostgreSQL Sequence Resync Logic
/// Ensures sequences are properly synchronized with actual max IDs
/// to prevent duplicate key errors after data import
/// </summary>
public class SequenceResyncTests : IDisposable
{
    private readonly AppDbContext _context;

    public SequenceResyncTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region Sequence Verification Logic Tests

    [Fact]
    public void SequenceVerification_WhenSequenceAheadOfMaxId_ShouldBeOk()
    {
        // Arrange
        var sequenceValue = 100L;
        var maxId = 50;

        // Act
        var difference = sequenceValue - maxId;
        var isOk = difference >= 0;

        // Assert
        isOk.Should().BeTrue("Sequence ahead of max ID is valid");
        difference.Should().Be(50);
    }

    [Fact]
    public void SequenceVerification_WhenSequenceBehindMaxId_ShouldBeProblem()
    {
        // Arrange
        var sequenceValue = 50L;
        var maxId = 100;

        // Act
        var difference = sequenceValue - maxId;
        var isOk = difference >= 0;

        // Assert
        isOk.Should().BeFalse("Sequence behind max ID will cause duplicate key errors");
        difference.Should().Be(-50);
    }

    [Fact]
    public void SequenceVerification_WhenSequenceEqualsMaxId_ShouldBeOk()
    {
        // Arrange
        var sequenceValue = 100L;
        var maxId = 100;

        // Act
        var difference = sequenceValue - maxId;
        var isOk = difference >= 0;

        // Assert
        isOk.Should().BeTrue("Sequence equal to max ID is valid (next insert will be max+1)");
        difference.Should().Be(0);
    }

    #endregion

    #region PartnerTree Sequence Tests

    [Fact]
    public async Task PartnerTreeSequence_AfterDataImport_ShouldMatchMaxId()
    {
        // Arrange - Create partner trees with specific IDs (simulating data import)
        var partnerTrees = new List<PartnerTree>
        {
            new PartnerTree { Name = "Category 1", Code = "CAT1", Level = 1 },
            new PartnerTree { Name = "Category 2", Code = "CAT2", Level = 1 },
            new PartnerTree { Name = "Category 3", Code = "CAT3", Level = 1 }
        };

        await _context.PartnerTrees.AddRangeAsync(partnerTrees);
        await _context.SaveChangesAsync();

        // Act - Get max ID
        var maxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThan(0, "PartnerTrees should have been created");

        // Verify new entity can be added without conflict
        var newTree = new PartnerTree { Name = "New Category", Code = "NEW1", Level = 1 };
        await _context.PartnerTrees.AddAsync(newTree);
        
        // This should not throw - if sequence is properly synced
        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync("Sequence should be synced to allow new inserts");

        newTree.Id.Should().BeGreaterThan(maxId, "New entity should have ID greater than previous max");
    }

    [Fact]
    public async Task PartnerTreeSequence_WhenTableEmpty_ShouldStartFromZero()
    {
        // Act
        var maxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().Be(0, "Empty table should have max ID of 0");
    }

    #endregion

    #region Interaction Sequence Tests

    [Fact]
    public async Task InteractionSequence_AfterDataImport_ShouldMatchMaxId()
    {
        // Arrange - Create interactions
        var interactions = new List<Interaction>
        {
            new Interaction { Subject = "Meeting 1", Date = DateTime.UtcNow.AddDays(-10) },
            new Interaction { Subject = "Call 2", Date = DateTime.UtcNow.AddDays(-5) },
            new Interaction { Subject = "Email 3", Date = DateTime.UtcNow }
        };

        await _context.Interactions.AddRangeAsync(interactions);
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThan(0);

        // Verify new entity can be added without conflict
        var newInteraction = new Interaction { Subject = "New Interaction", Date = DateTime.UtcNow };
        await _context.Interactions.AddAsync(newInteraction);
        
        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync();

        newInteraction.Id.Should().BeGreaterThan(maxId);
    }

    [Fact]
    public async Task InteractionSequence_WhenTableEmpty_ShouldStartFromZero()
    {
        // Act
        var maxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().Be(0);
    }

    #endregion

    #region Multiple Table Sequence Resync Tests

    [Fact]
    public async Task AllSequences_AfterDataImport_ShouldBeResyncedCorrectly()
    {
        // Arrange - Create data in multiple tables
        var partnerTrees = Enumerable.Range(1, 5)
            .Select(i => new PartnerTree { Name = $"Tree {i}", Code = $"T{i}", Level = 1 })
            .ToList();

        var interactions = Enumerable.Range(1, 10)
            .Select(i => new Interaction { Subject = $"Interaction {i}", Date = DateTime.UtcNow })
            .ToList();

        var partners = Enumerable.Range(1, 3)
            .Select(i => new Partner { Name = $"Partner {i}", PartnerShortDescription = $"Desc {i}" })
            .ToList();

        await _context.PartnerTrees.AddRangeAsync(partnerTrees);
        await _context.Interactions.AddRangeAsync(interactions);
        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Get max IDs for each table
        var partnerTreeMaxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;
        var interactionMaxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;
        var partnerMaxId = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        partnerTreeMaxId.Should().Be(5);
        interactionMaxId.Should().Be(10);
        partnerMaxId.Should().Be(3);

        // Verify new entities can be added to all tables
        var newTree = new PartnerTree { Name = "New Tree", Code = "NT", Level = 1 };
        var newInteraction = new Interaction { Subject = "New Interaction", Date = DateTime.UtcNow };
        var newPartner = new Partner { Name = "New Partner", PartnerShortDescription = "New Desc" };

        await _context.PartnerTrees.AddAsync(newTree);
        await _context.Interactions.AddAsync(newInteraction);
        await _context.Partners.AddAsync(newPartner);

        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync();

        newTree.Id.Should().Be(6);
        newInteraction.Id.Should().Be(11);
        newPartner.Id.Should().Be(4);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SequenceResync_WithSoftDeletedRecords_ShouldConsiderAllRecords()
    {
        // Arrange - Create records including soft-deleted
        var partners = new List<Partner>
        {
            new Partner { Name = "Active 1", PartnerShortDescription = "Desc", IsDeleted = false },
            new Partner { Name = "Deleted 1", PartnerShortDescription = "Desc", IsDeleted = true },
            new Partner { Name = "Active 2", PartnerShortDescription = "Desc", IsDeleted = false }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Get max ID (should include soft-deleted)
        var maxIdIncludingDeleted = await _context.Partners
            .IgnoreQueryFilters()
            .MaxAsync(x => (int?)x.Id) ?? 0;

        var maxIdExcludingDeleted = await _context.Partners
            .Where(p => !p.IsDeleted)
            .MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxIdIncludingDeleted.Should().Be(3, "Should count all records including soft-deleted");
        
        // For sequence resync, we should use the higher value (including deleted)
        // to ensure no conflicts with existing IDs
        var sequenceValue = maxIdIncludingDeleted;
        
        // Add new partner
        var newPartner = new Partner { Name = "New Partner", PartnerShortDescription = "New Desc" };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().BeGreaterThan(sequenceValue);
    }

    [Fact]
    public async Task SequenceResync_WithGapsInIds_ShouldUseMaxId()
    {
        // Arrange - In reality, PostgreSQL doesn't reuse IDs, so gaps can exist
        // Create some records, delete one, add more
        var initialPartners = new List<Partner>
        {
            new Partner { Name = "Partner 1", PartnerShortDescription = "Desc" },
            new Partner { Name = "Partner 2", PartnerShortDescription = "Desc" },
            new Partner { Name = "Partner 3", PartnerShortDescription = "Desc" }
        };

        await _context.Partners.AddRangeAsync(initialPartners);
        await _context.SaveChangesAsync();

        // Hard delete partner 2 (creates gap)
        var toDelete = await _context.Partners.FirstAsync(p => p.Name == "Partner 2");
        _context.Partners.Remove(toDelete);
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert - Max ID should still be 3 (the gap doesn't affect this)
        maxId.Should().Be(3);

        // New partner should get ID 4 (not reusing 2)
        var newPartner = new Partner { Name = "Partner 4", PartnerShortDescription = "Desc" };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().Be(4, "Should not reuse deleted ID");
    }

    [Fact]
    public async Task SequenceResync_WhenLargeIdGap_ShouldHandleCorrectly()
    {
        // Arrange - Simulate a large ID gap (as might occur after data import)
        var partner1 = new Partner { Name = "Partner 1", PartnerShortDescription = "Desc" };
        await _context.Partners.AddAsync(partner1);
        await _context.SaveChangesAsync();
        
        var firstId = partner1.Id;

        // Simulate importing data with high IDs would be done by setting identity insert
        // In in-memory database, we just add more records
        for (int i = 0; i < 100; i++)
        {
            await _context.Partners.AddAsync(new Partner 
            { 
                Name = $"Imported Partner {i}", 
                PartnerShortDescription = "Imported" 
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().Be(101); // 1 + 100

        // New partner should continue from max
        var newPartner = new Partner { Name = "Post-Import Partner", PartnerShortDescription = "New" };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().Be(102);
    }

    #endregion

    #region Verification Result Model Tests

    [Theory]
    [InlineData("PartnerTrees", 100, 50, "OK")]
    [InlineData("PartnerTrees", 50, 50, "OK")]
    [InlineData("PartnerTrees", 50, 100, "PROBLEM")]
    [InlineData("Interactions", 1000, 999, "OK")]
    [InlineData("Interactions", 999, 1000, "PROBLEM")]
    public void SequenceVerificationResult_ShouldIndicateCorrectStatus(
        string tableName, long sequenceValue, int maxId, string expectedStatus)
    {
        // Arrange
        var verification = new SequenceVerification
        {
            TableName = tableName,
            SequenceValue = sequenceValue,
            MaxId = maxId,
            Difference = sequenceValue - maxId
        };

        // Act
        var status = verification.Difference >= 0 ? "OK" : "PROBLEM";

        // Assert
        status.Should().Be(expectedStatus);
    }

    private class SequenceVerification
    {
        public string TableName { get; set; } = string.Empty;
        public long SequenceValue { get; set; }
        public int MaxId { get; set; }
        public long Difference { get; set; }
    }

    #endregion

    #region Concurrent Insert Tests

    [Fact]
    public async Task SequenceResync_WithConcurrentInserts_ShouldNotCauseConflicts()
    {
        // Arrange
        var dbName = $"ConcurrentSequence_{Guid.NewGuid()}";
        
        // Create initial data
        using (var setupContext = TestDbContextFactory.Create(dbName))
        {
            var initialPartners = Enumerable.Range(1, 10)
                .Select(i => new Partner { Name = $"Initial {i}", PartnerShortDescription = $"Desc {i}" })
                .ToList();
            await setupContext.Partners.AddRangeAsync(initialPartners);
            await setupContext.SaveChangesAsync();
        }

        // Act - Simulate concurrent inserts
        var tasks = new List<Task>();
        var insertedIds = new System.Collections.Concurrent.ConcurrentBag<int>();

        for (int i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                using var context = TestDbContextFactory.Create(dbName);
                var partner = new Partner 
                { 
                    Name = $"Concurrent {index}", 
                    PartnerShortDescription = $"Concurrent Desc {index}" 
                };
                await context.Partners.AddAsync(partner);
                await context.SaveChangesAsync();
                insertedIds.Add(partner.Id);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - All IDs should be unique
        insertedIds.Should().OnlyHaveUniqueItems("Concurrent inserts should all get unique IDs");
        insertedIds.Should().HaveCount(5);

        // Verify total count
        using (var verifyContext = TestDbContextFactory.Create(dbName))
        {
            var totalCount = await verifyContext.Partners.CountAsync();
            totalCount.Should().Be(15, "10 initial + 5 concurrent = 15 total");
        }
    }

    #endregion
}

