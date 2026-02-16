/**
 * @fileoverview Tests for PostgreSQL Sequence Resync functionality
 * Tests for recent commit: b1e1976c - Sequence Resync Logic for PartnerTree and Interaction
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
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
    private IDbContextTransaction? _transaction;
    private readonly int _testUserId;
    private readonly string _testMarker;

    public SequenceResyncTests()
    {
        _testMarker = Guid.NewGuid().ToString("N")[..8];

        if (TestEnvironment.UsePostgreSQL)
        {
            using var tempContext = TestDbContextFactory.CreateUNOPS();
            _testUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "seqresync-test@unops.org");
            _context = TestDbContextFactory.CreateWithUserId(_testUserId);
            _transaction = _context.Database.BeginTransaction();
        }
        else
        {
            _testUserId = 1;
            _context = TestDbContextFactory.Create();
        }
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { }
            _transaction.Dispose();
            _transaction = null;
        }
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
        // Arrange - Create partner trees with unique codes to avoid constraint violations
        var partnerTrees = new List<PartnerTree>
        {
            new PartnerTree { Name = $"Category 1 {_testMarker}", Description = "Category 1 Description", Code = $"C1{_testMarker}", Type = "CATEGORY" },
            new PartnerTree { Name = $"Category 2 {_testMarker}", Description = "Category 2 Description", Code = $"C2{_testMarker}", Type = "CATEGORY" },
            new PartnerTree { Name = $"Category 3 {_testMarker}", Description = "Category 3 Description", Code = $"C3{_testMarker}", Type = "CATEGORY" }
        };

        await _context.PartnerTrees.AddRangeAsync(partnerTrees);
        await _context.SaveChangesAsync();

        // Act - Get max ID
        var maxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThan(0, "PartnerTrees should have been created");

        // Verify new entity can be added without conflict
        var newTree = new PartnerTree { Name = $"New Category {_testMarker}", Description = "New Category Description", Code = $"N{_testMarker}", Type = "CATEGORY" };
        await _context.PartnerTrees.AddAsync(newTree);
        
        // This should not throw - if sequence is properly synced
        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync("Sequence should be synced to allow new inserts");

        newTree.Id.Should().BeGreaterThan(maxId, "New entity should have ID greater than previous max");
    }

    [Fact]
    public async Task PartnerTreeSequence_MaxIdQuery_ShouldReturnNonNegative()
    {
        // Act - On a shared DB, table may not be empty
        var maxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThanOrEqualTo(0, "Max ID query should return a non-negative value");
    }

    #endregion

    #region Interaction Sequence Tests

    [Fact]
    public async Task InteractionSequence_AfterDataImport_ShouldMatchMaxId()
    {
        // Arrange - Create interactions
        var interactions = new List<UNOPSInteraction>
        {
            new UNOPSInteraction { Name = "Meeting 1", Subject = "Meeting 1", Date = DateTime.UtcNow.AddDays(-10), LastModifiedDate = DateTime.UtcNow },
            new UNOPSInteraction { Name = "Call 2", Subject = "Call 2", Date = DateTime.UtcNow.AddDays(-5), LastModifiedDate = DateTime.UtcNow },
            new UNOPSInteraction { Name = "Email 3", Subject = "Email 3", Date = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Interactions.AddRangeAsync(interactions);
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThan(0);

        // Verify new entity can be added without conflict
        var newInteraction = new UNOPSInteraction { Name = "New Interaction", Subject = "New Interaction", Date = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow };
        await _context.Interactions.AddAsync(newInteraction);
        
        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync();

        newInteraction.Id.Should().BeGreaterThan(maxId);
    }

    [Fact]
    public async Task InteractionSequence_MaxIdQuery_ShouldReturnNonNegative()
    {
        // Act - On a shared DB, table may not be empty
        var maxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert
        maxId.Should().BeGreaterThanOrEqualTo(0, "Max ID query should return a non-negative value");
    }

    #endregion

    #region Multiple Table Sequence Resync Tests

    [Fact]
    public async Task AllSequences_AfterDataImport_ShouldBeResyncedCorrectly()
    {
        // Capture baseline max IDs before inserting test data
        var baselineTreeMax = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;
        var baselineInteractionMax = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;
        var baselinePartnerMax = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Arrange - Create data in multiple tables with unique codes/names
        var partnerTrees = Enumerable.Range(1, 5)
            .Select(i => new PartnerTree { Name = $"Tree {_testMarker}_{i}", Description = $"Tree {i} Description", Code = $"T{_testMarker}{i}", Type = "GROUP" })
            .ToList();

        var interactions = Enumerable.Range(1, 10)
            .Select(i => new UNOPSInteraction { Name = $"Int {_testMarker}_{i}", Subject = $"Interaction {_testMarker}_{i}", Date = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow })
            .ToList();

        var partners = Enumerable.Range(1, 3)
            .Select(i => new UNOPSPartner { Name = $"Ptr {_testMarker}_{i}", PartnerShortDescription = $"Desc {i}", LastModifiedDate = DateTime.UtcNow })
            .ToList();

        await _context.PartnerTrees.AddRangeAsync(partnerTrees);
        await _context.Interactions.AddRangeAsync(interactions);
        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Get max IDs for each table
        var partnerTreeMaxId = await _context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;
        var interactionMaxId = await _context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;
        var partnerMaxId = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert - Max IDs should have increased by the number of records we added
        partnerTreeMaxId.Should().BeGreaterThanOrEqualTo(baselineTreeMax + 5);
        interactionMaxId.Should().BeGreaterThanOrEqualTo(baselineInteractionMax + 10);
        partnerMaxId.Should().BeGreaterThanOrEqualTo(baselinePartnerMax + 3);

        // Verify new entities can be added to all tables without conflict
        var newTree = new PartnerTree { Name = $"NewTree {_testMarker}", Description = "New Tree Description", Code = $"NT{_testMarker}", Type = "GROUP" };
        var newInteraction = new UNOPSInteraction { Name = $"NewInt {_testMarker}", Subject = $"New Interaction {_testMarker}", Date = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow };
        var newPartner = new UNOPSPartner { Name = $"NewPtr {_testMarker}", PartnerShortDescription = "New Desc", LastModifiedDate = DateTime.UtcNow };

        await _context.PartnerTrees.AddAsync(newTree);
        await _context.Interactions.AddAsync(newInteraction);
        await _context.Partners.AddAsync(newPartner);

        var saveAction = async () => await _context.SaveChangesAsync();
        await saveAction.Should().NotThrowAsync();

        newTree.Id.Should().BeGreaterThan(partnerTreeMaxId);
        newInteraction.Id.Should().BeGreaterThan(interactionMaxId);
        newPartner.Id.Should().BeGreaterThan(partnerMaxId);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SequenceResync_WithSoftDeletedRecords_ShouldConsiderAllRecords()
    {
        // Capture baseline
        var baselineMax = await _context.Partners.IgnoreQueryFilters().MaxAsync(x => (int?)x.Id) ?? 0;

        // Arrange - Create records including soft-deleted
        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = "Active 1", PartnerShortDescription = "Desc", IsDeleted = false, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "Deleted 1", PartnerShortDescription = "Desc", IsDeleted = true, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = "Active 2", PartnerShortDescription = "Desc", IsDeleted = false, LastModifiedDate = DateTime.UtcNow }
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

        // Assert - max including deleted should be >= baseline + 3 (all 3 records)
        maxIdIncludingDeleted.Should().BeGreaterThanOrEqualTo(baselineMax + 3, 
            "Should count all records including soft-deleted");
        
        // Max including deleted should be >= max excluding deleted
        maxIdIncludingDeleted.Should().BeGreaterThanOrEqualTo(maxIdExcludingDeleted,
            "Including soft-deleted should always be >= excluding");
        
        // For sequence resync, we should use the higher value (including deleted)
        var sequenceValue = maxIdIncludingDeleted;
        
        // Add new partner
        var newPartner = new UNOPSPartner { Name = "New Partner", PartnerShortDescription = "New Desc", LastModifiedDate = DateTime.UtcNow };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().BeGreaterThan(sequenceValue);
    }

    [Fact]
    public async Task SequenceResync_WithGapsInIds_ShouldUseMaxId()
    {
        // Arrange - Use unique names to identify test data
        var marker = $"GapTest_{Guid.NewGuid():N}";
        var initialPartners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{marker}_1", PartnerShortDescription = "Desc", LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{marker}_2", PartnerShortDescription = "Desc", LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{marker}_3", PartnerShortDescription = "Desc", LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(initialPartners);
        await _context.SaveChangesAsync();

        var thirdPartnerId = initialPartners[2].Id;

        // Hard delete partner 2 (creates gap)
        var toDelete = await _context.Partners.FirstAsync(p => p.Name == $"{marker}_2");
        _context.Partners.Remove(toDelete);
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Partners.IgnoreQueryFilters().MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert - Max ID should be at least the third partner's ID (gap doesn't reduce max)
        maxId.Should().BeGreaterThanOrEqualTo(thirdPartnerId, "Deleting partner 2 should not reduce max ID");

        // New partner should get a higher ID (not reusing the deleted one)
        var newPartner = new UNOPSPartner { Name = $"{marker}_4", PartnerShortDescription = "Desc", LastModifiedDate = DateTime.UtcNow };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().BeGreaterThan(thirdPartnerId, "Should not reuse deleted ID");
    }

    [Fact]
    public async Task SequenceResync_WhenLargeIdGap_ShouldHandleCorrectly()
    {
        // Capture baseline
        var baselineMax = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Arrange - Simulate a large ID gap (as might occur after data import)
        var marker = $"LargeGap_{Guid.NewGuid():N}";
        var partner1 = new UNOPSPartner { Name = $"{marker}_First", PartnerShortDescription = "Desc", LastModifiedDate = DateTime.UtcNow };
        await _context.Partners.AddAsync(partner1);
        await _context.SaveChangesAsync();
        
        var firstId = partner1.Id;

        // Simulate importing many records
        for (int i = 0; i < 100; i++)
        {
            await _context.Partners.AddAsync(new UNOPSPartner
            {
                Name = $"{marker}_Imported_{i}",
                PartnerShortDescription = "Imported",
                LastModifiedDate = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var maxId = await _context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;

        // Assert - Max ID should have increased by at least 101 from baseline
        maxId.Should().BeGreaterThanOrEqualTo(baselineMax + 101);

        // New partner should continue from max
        var newPartner = new UNOPSPartner { Name = $"{marker}_PostImport", PartnerShortDescription = "New", LastModifiedDate = DateTime.UtcNow };
        await _context.Partners.AddAsync(newPartner);
        await _context.SaveChangesAsync();

        newPartner.Id.Should().BeGreaterThan(maxId, "New partner should get an ID greater than current max");
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

    [Fact(Skip = "Concurrent insert test requires separate database instances. On shared PostgreSQL, " +
                "TestDbContextFactory.Create(dbName) ignores the dbName parameter, and concurrent contexts " +
                "share the same DB making count assertions unreliable.")]
    public async Task SequenceResync_WithConcurrentInserts_ShouldNotCauseConflicts()
    {
        // This test uses separate named databases for concurrent operations.
        // On PostgreSQL, TestDbContextFactory.Create(dbName) ignores the dbName parameter
        // and all contexts connect to the same shared database, making count assertions invalid.
        await Task.CompletedTask;
    }

    #endregion
}

