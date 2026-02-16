using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Direct database-level tests for EntityConfiguration entities (Entities, EntityManager, EntityFieldManager).
/// Validates CRUD operations, parent-child relationships, field configuration,
/// and soft delete behavior for the entity configuration system.
/// </summary>
public class UNOPSEntityConfigurationManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"ECFG_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<int> SeedEntityManagerAsync(string entityName, string? tableName = null)
    {
        var em = new EntityManager
        {
            Name = $"EM {entityName} {_testMarker}",
            EntityName = entityName,
            TableName = tableName ?? $"tbl_{entityName.ToLower()}",
            Description = $"Test entity config for {entityName}",
            IsActive = true,
            EnableChangeLog = false,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityManagers.AddAsync(em);
        await SaveChangesAsync();
        return em.Id;
    }

    private async Task<int> SeedEntityFieldAsync(int entityManagerId, string fieldName, string dataType = "text")
    {
        var field = new EntityFieldManager
        {
            Name = $"Field {fieldName} {_testMarker}",
            FieldName = fieldName,
            DataType = dataType,
            Description = $"Test field {fieldName}",
            IsRequired = false,
            IsActive = true,
            DisplayOrder = 0,
            EntityManagerId = entityManagerId,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.EntityFieldManagers.AddAsync(field);
        await SaveChangesAsync();
        return field.Id;
    }

    #endregion

    #region P0 - EntityManager CRUD

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-001")]
    public async Task CreateEntityManager_WithValidData_ShouldPersist()
    {
        // Arrange
        var entityName = $"TestEntity_{_testMarker[..8]}";

        // Act
        var emId = await SeedEntityManagerAsync(entityName);
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(em => em.Id == emId);
        saved.Should().NotBeNull();
        saved!.EntityName.Should().Be(entityName);
        saved.TableName.Should().Be($"tbl_{entityName.ToLower()}");
        saved.IsActive.Should().BeTrue();
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-002")]
    public async Task ReadEntityManager_ById_ShouldReturnCorrectRecord()
    {
        // Arrange
        var entityName = $"ReadEntity_{_testMarker[..8]}";
        var emId = await SeedEntityManagerAsync(entityName);
        Context.ChangeTracker.Clear();

        // Act
        var em = await Context.EntityManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == emId && !e.IsDeleted);

        // Assert
        em.Should().NotBeNull();
        em!.EntityName.Should().Be(entityName);
        em.Description.Should().Contain(entityName);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-003")]
    public async Task UpdateEntityManager_ChangeDescription_ShouldPersist()
    {
        // Arrange
        var entityName = $"UpdateEntity_{_testMarker[..8]}";
        var emId = await SeedEntityManagerAsync(entityName);

        // Act
        var em = await Context.EntityManagers.FirstAsync(e => e.Id == emId);
        em.Description = "Updated description";
        em.EnableChangeLog = true;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == emId);
        saved.Should().NotBeNull();
        saved!.Description.Should().Be("Updated description");
        saved.EnableChangeLog.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-004")]
    public async Task SoftDeleteEntityManager_ShouldSetIsDeleted()
    {
        // Arrange
        var entityName = $"DeleteEntity_{_testMarker[..8]}";
        var emId = await SeedEntityManagerAsync(entityName);

        // Act
        var em = await Context.EntityManagers.FirstAsync(e => e.Id == emId);
        em.IsDeleted = true;
        em.DeletedDate = DateTime.UtcNow;
        em.DeletedBy = TestUserId;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.EntityManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == emId);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedDate.Should().NotBeNull();
    }

    #endregion

    #region P0 - EntityFieldManager CRUD

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-005")]
    public async Task CreateEntityField_WithValidData_ShouldPersist()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"FieldTest_{_testMarker[..8]}");

        // Act
        var fieldId = await SeedEntityFieldAsync(emId, "TestField", "text");
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityFieldManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fieldId);
        saved.Should().NotBeNull();
        saved!.FieldName.Should().Be("TestField");
        saved.DataType.Should().Be("text");
        saved.EntityManagerId.Should().Be(emId);
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-006")]
    public async Task CreateEntityField_MultipleTypes_ShouldPersistAllTypes()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"MultiField_{_testMarker[..8]}");

        // Act
        var textFieldId = await SeedEntityFieldAsync(emId, "TextField", "text");
        var numberFieldId = await SeedEntityFieldAsync(emId, "NumberField", "number");
        var dateFieldId = await SeedEntityFieldAsync(emId, "DateField", "date");
        var boolFieldId = await SeedEntityFieldAsync(emId, "BoolField", "boolean");
        Context.ChangeTracker.Clear();

        // Assert
        var fields = await Context.EntityFieldManagers
            .AsNoTracking()
            .Where(f => f.EntityManagerId == emId && !f.IsDeleted)
            .ToListAsync();
        fields.Should().HaveCount(4);
        fields.Select(f => f.DataType).Should().Contain(new[] { "text", "number", "date", "boolean" });
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-007")]
    public async Task UpdateEntityField_ChangeProperties_ShouldPersist()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"UpdateField_{_testMarker[..8]}");
        var fieldId = await SeedEntityFieldAsync(emId, "EditableField", "text");

        // Act
        var field = await Context.EntityFieldManagers.FirstAsync(f => f.Id == fieldId);
        field.IsRequired = true;
        field.MaxLength = 255;
        field.ShowInListView = true;
        field.ListViewOrder = 1;
        field.HelperText = "This field is important";
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.EntityFieldManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fieldId);
        saved.Should().NotBeNull();
        saved!.IsRequired.Should().BeTrue();
        saved.MaxLength.Should().Be(255);
        saved.ShowInListView.Should().BeTrue();
        saved.ListViewOrder.Should().Be(1);
        saved.HelperText.Should().Be("This field is important");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-008")]
    public async Task SoftDeleteEntityField_ShouldSetIsDeleted()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"DeleteField_{_testMarker[..8]}");
        var fieldId = await SeedEntityFieldAsync(emId, "DeleteMe", "text");

        // Act
        var field = await Context.EntityFieldManagers.FirstAsync(f => f.Id == fieldId);
        field.IsDeleted = true;
        field.DeletedDate = DateTime.UtcNow;
        field.DeletedBy = TestUserId;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var activeFields = await Context.EntityFieldManagers
            .AsNoTracking()
            .Where(f => f.EntityManagerId == emId && !f.IsDeleted)
            .ToListAsync();
        activeFields.Should().BeEmpty();
    }

    #endregion

    #region P1 - Parent-Child Relationships

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-009")]
    public async Task EntityManager_WithFields_ShouldLoadNavigationProperty()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"NavTest_{_testMarker[..8]}");
        await SeedEntityFieldAsync(emId, "Field1", "text");
        await SeedEntityFieldAsync(emId, "Field2", "number");
        await SeedEntityFieldAsync(emId, "Field3", "date");
        Context.ChangeTracker.Clear();

        // Act
        var em = await Context.EntityManagers
            .AsNoTracking()
            .Include(e => e.EntityFields.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(e => e.Id == emId);

        // Assert
        em.Should().NotBeNull();
        em!.EntityFields.Should().HaveCount(3);
        em.EntityFields.Select(f => f.FieldName).Should().Contain(new[] { "Field1", "Field2", "Field3" });
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("Category", "P1")]
    [Trait("Type", "DataIntegrity")]
    [Trait("TestId", "TC-ECFG-010")]
    public async Task EntityField_WithInvalidEntityManagerId_ShouldBeRejectedByFK()
    {
        // Arrange
        var field = new EntityFieldManager
        {
            Name = $"Orphan Field {_testMarker}",
            FieldName = "OrphanField",
            DataType = "text",
            EntityManagerId = int.MaxValue,
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        // Act
        await Context.EntityFieldManagers.AddAsync(field);
        Func<Task> act = async () => await SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region P1 - List View Configuration

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-011")]
    public async Task EntityFields_ShowInListView_ShouldFilterCorrectly()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"ListView_{_testMarker[..8]}");
        var f1Id = await SeedEntityFieldAsync(emId, "VisibleField", "text");
        var f2Id = await SeedEntityFieldAsync(emId, "HiddenField", "text");

        var f1 = await Context.EntityFieldManagers.FirstAsync(f => f.Id == f1Id);
        f1.ShowInListView = true;
        f1.ListViewOrder = 1;
        f1.ListViewLabel = "Visible";

        var f2 = await Context.EntityFieldManagers.FirstAsync(f => f.Id == f2Id);
        f2.ShowInListView = false;

        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var listViewFields = await Context.EntityFieldManagers
            .AsNoTracking()
            .Where(f => f.EntityManagerId == emId && f.ShowInListView && !f.IsDeleted)
            .OrderBy(f => f.ListViewOrder)
            .ToListAsync();

        // Assert
        listViewFields.Should().HaveCount(1);
        listViewFields.Single().FieldName.Should().Be("VisibleField");
        listViewFields.Single().ListViewLabel.Should().Be("Visible");
    }

    #endregion

    #region P2 - Multiple EntityManagers

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-012")]
    public async Task MultipleEntityManagers_ShouldAllPersistIndependently()
    {
        // Arrange & Act
        var em1Id = await SeedEntityManagerAsync($"Entity1_{_testMarker[..8]}");
        var em2Id = await SeedEntityManagerAsync($"Entity2_{_testMarker[..8]}");
        var em3Id = await SeedEntityManagerAsync($"Entity3_{_testMarker[..8]}");

        await SeedEntityFieldAsync(em1Id, "Field_A", "text");
        await SeedEntityFieldAsync(em1Id, "Field_B", "number");
        await SeedEntityFieldAsync(em2Id, "Field_C", "date");
        Context.ChangeTracker.Clear();

        // Assert
        var managers = await Context.EntityManagers
            .AsNoTracking()
            .Include(em => em.EntityFields.Where(f => !f.IsDeleted))
            .Where(em => em.Name.Contains(_testMarker) && !em.IsDeleted)
            .ToListAsync();

        managers.Should().HaveCount(3);

        var m1 = managers.First(m => m.Id == em1Id);
        m1.EntityFields.Should().HaveCount(2);

        var m2 = managers.First(m => m.Id == em2Id);
        m2.EntityFields.Should().HaveCount(1);

        var m3 = managers.First(m => m.Id == em3Id);
        m3.EntityFields.Should().BeEmpty();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-013")]
    public async Task SoftDeletedEntityManager_ShouldExcludeFromActiveQueries()
    {
        // Arrange
        var activeId = await SeedEntityManagerAsync($"Active_{_testMarker[..8]}");
        var deletedId = await SeedEntityManagerAsync($"Deleted_{_testMarker[..8]}");

        var deleted = await Context.EntityManagers.FirstAsync(e => e.Id == deletedId);
        deleted.IsDeleted = true;
        deleted.DeletedDate = DateTime.UtcNow;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var activeManagers = await Context.EntityManagers
            .AsNoTracking()
            .Where(em => em.Name.Contains(_testMarker) && !em.IsDeleted)
            .ToListAsync();

        // Assert
        activeManagers.Should().HaveCount(1);
        activeManagers.Single().Id.Should().Be(activeId);
    }

    #endregion

    #region P2 - Display Order and Sorting

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-ECFG-014")]
    public async Task EntityFields_DisplayOrder_ShouldSortCorrectly()
    {
        // Arrange
        var emId = await SeedEntityManagerAsync($"Sort_{_testMarker[..8]}");
        var f3 = await SeedEntityFieldAsync(emId, "Third", "text");
        var f1 = await SeedEntityFieldAsync(emId, "First", "text");
        var f2 = await SeedEntityFieldAsync(emId, "Second", "text");

        var field3 = await Context.EntityFieldManagers.FirstAsync(f => f.Id == f3);
        field3.DisplayOrder = 3;
        var field1 = await Context.EntityFieldManagers.FirstAsync(f => f.Id == f1);
        field1.DisplayOrder = 1;
        var field2 = await Context.EntityFieldManagers.FirstAsync(f => f.Id == f2);
        field2.DisplayOrder = 2;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var sorted = await Context.EntityFieldManagers
            .AsNoTracking()
            .Where(f => f.EntityManagerId == emId && !f.IsDeleted)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        // Assert
        sorted.Should().HaveCount(3);
        sorted[0].FieldName.Should().Be("First");
        sorted[1].FieldName.Should().Be("Second");
        sorted[2].FieldName.Should().Be("Third");
    }

    #endregion
}
