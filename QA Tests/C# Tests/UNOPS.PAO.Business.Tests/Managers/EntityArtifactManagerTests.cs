using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Direct database-level tests for EntityArtifact and ArtifactType entities.
/// Validates CRUD operations, multi-value types (text, number, boolean, date, JSON),
/// FK constraints, and soft delete behavior for the entity artifact system.
/// These tests exercise the data layer used by EntityArtifactController.
/// </summary>
public class EntityArtifactManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"EART_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<int> SeedArtifactDataTypeAsync(string name = "text")
    {
        var existing = await Context.ArtifactDataTypes
            .FirstOrDefaultAsync(adt => adt.Name == name && !adt.IsDeleted);
        if (existing != null) return existing.Id;

        var dataType = new ArtifactDataType
        {
            Name = name,
            Description = $"Test data type: {name}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.ArtifactDataTypes.AddAsync(dataType);
        await SaveChangesAsync();
        return dataType.Id;
    }

    private async Task<int> SeedArtifactTypeAsync(string code, string name, string dataTypeName = "text")
    {
        var existing = await Context.ArtifactTypes
            .FirstOrDefaultAsync(at => at.ArtifactTypeCode == code && !at.IsDeleted);
        if (existing != null) return existing.Id;

        var dataTypeId = await SeedArtifactDataTypeAsync(dataTypeName);

        var artifactType = new ArtifactType
        {
            Name = $"AT {name} {_testMarker}",
            ArtifactTypeCode = code,
            ArtifactDataTypeId = dataTypeId,
            Description = $"Test artifact type: {name}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.ArtifactTypes.AddAsync(artifactType);
        await SaveChangesAsync();
        return artifactType.Id;
    }

    #endregion

    #region P0 - EntityArtifact CRUD

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-001")]
    public async Task CreateEntityArtifact_TextValue_ShouldPersist()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"TEXT_{_testMarker[..8]}", "Text Artifact", "text");
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 1,
            ArtifactTypeId = typeId,
            Name = $"Text Artifact {_testMarker}",
            ValueText = "Sample text value",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        saved.Should().NotBeNull();
        saved!.EntityType.Should().Be("Opportunity");
        saved.EntityId.Should().Be(1);
        saved.ValueText.Should().Be("Sample text value");
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-002")]
    public async Task CreateEntityArtifact_NumberValue_ShouldPersist()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"NUM_{_testMarker[..8]}", "Number Artifact", "number");
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 2,
            ArtifactTypeId = typeId,
            Name = $"Number Artifact {_testMarker}",
            ValueNumber = 42.5m,
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        saved.Should().NotBeNull();
        saved!.ValueNumber.Should().Be(42.5m);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-003")]
    public async Task CreateEntityArtifact_BooleanValue_ShouldPersist()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"BOOL_{_testMarker[..8]}", "Bool Artifact", "boolean");
        var artifact = new EntityArtifact
        {
            EntityType = "Partner",
            EntityId = 3,
            ArtifactTypeId = typeId,
            Name = $"Bool Artifact {_testMarker}",
            ValueBoolean = true,
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        saved.Should().NotBeNull();
        saved!.ValueBoolean.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-004")]
    public async Task CreateEntityArtifact_DateValue_ShouldPersist()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"DATE_{_testMarker[..8]}", "Date Artifact", "date");
        var targetDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 4,
            ArtifactTypeId = typeId,
            Name = $"Date Artifact {_testMarker}",
            ValueDate = targetDate,
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        saved.Should().NotBeNull();
        saved!.ValueDate.Should().BeCloseTo(targetDate, TimeSpan.FromSeconds(1));
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-005")]
    public async Task UpdateEntityArtifact_ChangeValue_ShouldPersist()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"UPD_{_testMarker[..8]}", "Update Test", "text");
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 5,
            ArtifactTypeId = typeId,
            Name = $"Update Artifact {_testMarker}",
            ValueText = "Original",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();

        // Act
        artifact.ValueText = "Updated value";
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        saved.Should().NotBeNull();
        saved!.ValueText.Should().Be("Updated value");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-006")]
    public async Task SoftDeleteEntityArtifact_ShouldSetIsDeleted()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"DEL_{_testMarker[..8]}", "Delete Test", "text");
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 6,
            ArtifactTypeId = typeId,
            Name = $"Delete Artifact {_testMarker}",
            ValueText = "To be deleted",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();

        // Act
        artifact.IsDeleted = true;
        artifact.DeletedDate = DateTime.UtcNow;
        artifact.DeletedBy = TestUserId;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.EntityArtifacts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedDate.Should().NotBeNull();

        var activeArtifacts = await Context.EntityArtifacts
            .AsNoTracking()
            .Where(a => a.Name.Contains(_testMarker) && !a.IsDeleted)
            .ToListAsync();
        activeArtifacts.Should().BeEmpty();
    }

    #endregion

    #region P1 - ArtifactType Lookups

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-007")]
    public async Task ArtifactType_SeedData_ShouldHaveExpectedFields()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"LOOKUP_{_testMarker[..8]}", "Lookup Test", "text");
        Context.ChangeTracker.Clear();

        // Act
        var type = await Context.ArtifactTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(at => at.Id == typeId);

        // Assert
        type.Should().NotBeNull();
        type!.ArtifactTypeCode.Should().Contain(_testMarker[..8]);
        type.Description.Should().Contain("Lookup Test");
        type.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-008")]
    public async Task EntityArtifact_NavigationToArtifactType_ShouldLoad()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"NAV_{_testMarker[..8]}", "Nav Test", "text");
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 10,
            ArtifactTypeId = typeId,
            Name = $"Nav Artifact {_testMarker}",
            ValueText = "Navigate me",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityArtifacts.AddAsync(artifact);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var loaded = await Context.EntityArtifacts
            .AsNoTracking()
            .Include(a => a.ArtifactType)
            .FirstOrDefaultAsync(a => a.Id == artifact.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.ArtifactType.Should().NotBeNull();
        loaded.ArtifactType!.ArtifactTypeCode.Should().Contain(_testMarker[..8]);
    }

    #endregion

    #region P1 - FK Constraints

    [SkipIfNotPostgreSQLFact]
    [Trait("Category", "P1")]
    [Trait("Type", "DataIntegrity")]
    [Trait("TestId", "TC-EART-009")]
    public async Task CreateEntityArtifact_WithInvalidArtifactTypeId_ShouldBeRejectedByFK()
    {
        // Arrange
        var artifact = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = 1,
            ArtifactTypeId = int.MaxValue,
            Name = $"Bad FK {_testMarker}",
            ValueText = "Should fail",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityArtifacts.AddAsync(artifact);
        Func<Task> act = async () => await SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region P1 - Query by Entity

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-010")]
    public async Task QueryArtifacts_ByEntityTypeAndId_ShouldReturnCorrectSet()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"QUERY_{_testMarker[..8]}", "Query Test", "text");
        var entityId = 50;

        for (int i = 0; i < 3; i++)
        {
            await Context.EntityArtifacts.AddAsync(new EntityArtifact
            {
                EntityType = "Opportunity",
                EntityId = entityId,
                ArtifactTypeId = typeId,
                Name = $"Query Artifact {i} {_testMarker}",
                ValueText = $"Value {i}",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        // Add one for a different entity to verify filtering
        await Context.EntityArtifacts.AddAsync(new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = entityId + 1,
            ArtifactTypeId = typeId,
            Name = $"Other Entity {_testMarker}",
            ValueText = "Different entity",
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var results = await Context.EntityArtifacts
            .AsNoTracking()
            .Where(a => a.EntityType == "Opportunity"
                        && a.EntityId == entityId
                        && a.Name.Contains(_testMarker)
                        && !a.IsDeleted)
            .ToListAsync();

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(a => a.EntityId == entityId);
    }

    #endregion

    #region P2 - Soft Delete Isolation

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-EART-011")]
    public async Task SoftDeletedArtifacts_ShouldBeExcludedFromActiveQuery()
    {
        // Arrange
        var typeId = await SeedArtifactTypeAsync($"ISO_{_testMarker[..8]}", "Isolation Test", "text");
        var entityId = 60;

        var active = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = entityId,
            ArtifactTypeId = typeId,
            Name = $"Active {_testMarker}",
            ValueText = "Still here",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deleted = new EntityArtifact
        {
            EntityType = "Opportunity",
            EntityId = entityId,
            ArtifactTypeId = typeId,
            Name = $"Deleted {_testMarker}",
            ValueText = "Gone",
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            DeletedBy = TestUserId
        };

        await Context.EntityArtifacts.AddRangeAsync(active, deleted);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var activeOnly = await Context.EntityArtifacts
            .AsNoTracking()
            .Where(a => a.EntityId == entityId && a.Name.Contains(_testMarker) && !a.IsDeleted)
            .ToListAsync();

        // Assert
        activeOnly.Should().HaveCount(1);
        activeOnly.Single().ValueText.Should().Be("Still here");
    }

    #endregion
}
