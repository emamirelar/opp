using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Direct database-level tests for the AuditLog entity.
/// Validates CRUD operations, query patterns (by entity type/id, latest log),
/// soft delete, and JSON data storage for the audit trail system.
/// These tests exercise the data layer used by AuditLogController.
/// </summary>
public class AuditLogManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"AUDIT_{Guid.NewGuid():N}";

    #region P0 - AuditLog CRUD

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-001")]
    public async Task CreateAuditLog_WithValidData_ShouldPersist()
    {
        // Arrange
        var auditLog = new AuditLog
        {
            Name = $"Audit {_testMarker}",
            EntityType = "Opportunity",
            EntityId = 1,
            Action = "create",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Description = $"Created opportunity {_testMarker}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.AuditLogs.AddAsync(auditLog);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLog.Id);
        saved.Should().NotBeNull();
        saved!.EntityType.Should().Be("Opportunity");
        saved.EntityId.Should().Be(1);
        saved.Action.Should().Be("create");
        saved.UserId.Should().Be(TestUserId);
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-002")]
    public async Task CreateAuditLog_WithJsonData_ShouldPersist()
    {
        // Arrange
        var jsonData = "{\"field\":\"status\",\"old\":\"Draft\",\"new\":\"Active\"}";
        var auditLog = new AuditLog
        {
            Name = $"JSON Audit {_testMarker}",
            EntityType = "Partner",
            EntityId = 2,
            Action = "update",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            JsonData = jsonData,
            Description = "Status changed from Draft to Active",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.AuditLogs.AddAsync(auditLog);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == auditLog.Id);
        saved.Should().NotBeNull();
        saved!.JsonData.Should().NotBeNullOrEmpty();
        saved.JsonData.Should().Contain("status");
        saved.JsonData.Should().Contain("Draft");
        saved.JsonData.Should().Contain("Active");
        saved.Action.Should().Be("update");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-003")]
    public async Task ReadAuditLog_ByEntityTypeAndId_ShouldReturnMatches()
    {
        // Arrange
        var entityId = 100;
        for (int i = 0; i < 3; i++)
        {
            await Context.AuditLogs.AddAsync(new AuditLog
            {
                Name = $"Read Audit {i} {_testMarker}",
                EntityType = "Opportunity",
                EntityId = entityId,
                Action = i == 0 ? "create" : "update",
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                UserId = TestUserId,
                Description = $"Action {i} on entity {_testMarker}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var logs = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Opportunity"
                        && a.EntityId == entityId
                        && a.Name.Contains(_testMarker)
                        && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(3);
        logs.Should().OnlyContain(a => a.EntityId == entityId);
    }

    #endregion

    #region P1 - Latest Audit Log Query

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-004")]
    public async Task GetLatestAuditLog_ShouldReturnMostRecent()
    {
        // Arrange
        var entityId = 200;
        var baseTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Oldest {_testMarker}",
            EntityType = "Partner",
            EntityId = entityId,
            Action = "create",
            Timestamp = baseTime,
            UserId = TestUserId,
            Description = "First entry",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Middle {_testMarker}",
            EntityType = "Partner",
            EntityId = entityId,
            Action = "update",
            Timestamp = baseTime.AddHours(1),
            UserId = TestUserId,
            Description = "Second entry",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Latest {_testMarker}",
            EntityType = "Partner",
            EntityId = entityId,
            Action = "source_update",
            Timestamp = baseTime.AddHours(2),
            UserId = TestUserId,
            Description = "Third entry (latest)",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act - mimic the controller's "latest" query
        var latest = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Partner"
                        && a.EntityId == entityId
                        && a.Name.Contains(_testMarker)
                        && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        // Assert
        latest.Should().NotBeNull();
        latest!.Action.Should().Be("source_update");
        latest.Description.Should().Be("Third entry (latest)");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-005")]
    public async Task GetLatestAuditLog_NoEntries_ShouldReturnNull()
    {
        // Act
        var latest = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "NonExistent"
                        && a.EntityId == int.MaxValue
                        && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        // Assert
        latest.Should().BeNull();
    }

    #endregion

    #region P1 - Multiple Action Types

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-006")]
    public async Task AuditLog_DifferentActionTypes_ShouldAllPersist()
    {
        // Arrange
        var entityId = 300;
        var actions = new[] { "create", "update", "delete", "source_update", "status_change" };

        foreach (var action in actions)
        {
            await Context.AuditLogs.AddAsync(new AuditLog
            {
                Name = $"Action {action} {_testMarker}",
                EntityType = "Opportunity",
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                UserId = TestUserId,
                Description = $"Performed {action}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var logs = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityId == entityId && a.Name.Contains(_testMarker) && !a.IsDeleted)
            .ToListAsync();

        // Assert
        logs.Should().HaveCount(5);
        logs.Select(a => a.Action).Should().BeEquivalentTo(actions);
    }

    #endregion

    #region P2 - Soft Delete

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-007")]
    public async Task SoftDeletedAuditLog_ShouldBeExcludedFromActiveQuery()
    {
        // Arrange
        var entityId = 400;

        var active = new AuditLog
        {
            Name = $"Active Audit {_testMarker}",
            EntityType = "Opportunity",
            EntityId = entityId,
            Action = "create",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Description = "Active entry",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deleted = new AuditLog
        {
            Name = $"Deleted Audit {_testMarker}",
            EntityType = "Opportunity",
            EntityId = entityId,
            Action = "update",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Description = "Deleted entry",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            DeletedBy = TestUserId
        };

        await Context.AuditLogs.AddRangeAsync(active, deleted);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var activeOnly = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityId == entityId && a.Name.Contains(_testMarker) && !a.IsDeleted)
            .ToListAsync();

        // Assert
        activeOnly.Should().HaveCount(1);
        activeOnly.Single().Action.Should().Be("create");
    }

    #endregion

    #region P2 - Cross-Entity Audit Queries

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-AUDIT-008")]
    public async Task AuditLog_DifferentEntityTypes_ShouldFilterCorrectly()
    {
        // Arrange
        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Opp Audit {_testMarker}",
            EntityType = "Opportunity",
            EntityId = 500,
            Action = "create",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Partner Audit {_testMarker}",
            EntityType = "Partner",
            EntityId = 501,
            Action = "create",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await Context.AuditLogs.AddAsync(new AuditLog
        {
            Name = $"Contact Audit {_testMarker}",
            EntityType = "Contact",
            EntityId = 502,
            Action = "create",
            Timestamp = DateTime.UtcNow,
            UserId = TestUserId,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act - filter by Opportunity only
        var oppLogs = await Context.AuditLogs
            .AsNoTracking()
            .Where(a => a.EntityType == "Opportunity" && a.Name.Contains(_testMarker) && !a.IsDeleted)
            .ToListAsync();

        // Assert
        oppLogs.Should().HaveCount(1);
        oppLogs.Single().EntityId.Should().Be(500);
    }

    #endregion
}
