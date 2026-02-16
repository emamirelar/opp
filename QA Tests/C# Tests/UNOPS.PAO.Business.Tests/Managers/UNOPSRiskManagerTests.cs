using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Direct database-level tests for the Risk entity and its related lookup tables.
/// Validates CRUD operations, FK constraints, soft delete, and lookup data integrity.
/// These tests exercise the data layer that UNOPSRiskManager depends on.
/// </summary>
public class UNOPSRiskManagerTests : ManagerTestBase
{
    private readonly string _testMarker = $"RISK_{Guid.NewGuid():N}";

    #region Seed Helpers

    private async Task<int> SeedRiskTypeAsync(string code = "THREAT", string name = "Threat")
    {
        var existing = await Context.RiskTypes
            .FirstOrDefaultAsync(rt => rt.Code == code && !rt.IsDeleted);
        if (existing != null) return existing.Id;

        var riskType = new RiskType
        {
            Name = name,
            Code = code,
            Description = $"Test {name}",
            DisplayOrder = 1,
            IsResponseTypeMandatory = code == "OPPORTUNITY",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskTypes.AddAsync(riskType);
        await SaveChangesAsync();
        return riskType.Id;
    }

    private async Task<int> SeedRiskProbabilityAsync(string code = "LOW", string name = "Low")
    {
        var existing = await Context.RiskProbabilities
            .FirstOrDefaultAsync(rp => rp.Code == code);
        if (existing != null) return existing.Id;

        var prob = new RiskProbability
        {
            Name = name,
            Code = code,
            DisplayLabel = name,
            NumericValue = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskProbabilities.AddAsync(prob);
        await SaveChangesAsync();
        return prob.Id;
    }

    private async Task<int> SeedRiskProximityAsync(string code = "WITHIN_SIX_MONTHS", string name = "Within six months")
    {
        var existing = await Context.RiskProximities
            .FirstOrDefaultAsync(rp => rp.Code == code);
        if (existing != null) return existing.Id;

        var prox = new RiskProximity
        {
            Name = name,
            Code = code,
            MonthsValue = 6,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskProximities.AddAsync(prox);
        await SaveChangesAsync();
        return prox.Id;
    }

    private async Task<int> SeedRiskImpactLevelAsync(string code = "LOW", string name = "Low")
    {
        var existing = await Context.RiskImpactLevels
            .FirstOrDefaultAsync(ri => ri.Code == code);
        if (existing != null) return existing.Id;

        var impact = new RiskImpactLevel
        {
            Name = name,
            Code = code,
            DisplayLabel = name,
            NumericValue = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        await Context.RiskImpactLevels.AddAsync(impact);
        await SaveChangesAsync();
        return impact.Id;
    }

    private async Task<int> SeedRiskCategoryLevel3Async()
    {
        var existing = await Context.RiskCategories
            .FirstOrDefaultAsync(rc => rc.Level == 3 && !rc.IsDeleted);
        if (existing != null) return existing.Id;

        var level1 = new RiskCategory
        {
            Name = $"Finance {_testMarker}",
            Code = $"UPC1_FINANCE_{_testMarker[..8]}",
            ShortCode = "FINANCE",
            Level = 1,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level1);
        await SaveChangesAsync();

        var level2 = new RiskCategory
        {
            Name = $"Contributions {_testMarker}",
            Code = $"UPC2_CONTRIB_{_testMarker[..8]}",
            ShortCode = "CONTRIBUTIONS",
            Level = 2,
            ParentCategoryId = level1.Id,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level2);
        await SaveChangesAsync();

        var level3 = new RiskCategory
        {
            Name = $"Engagement Costing {_testMarker}",
            Code = $"UPC3_ENGCOST_{_testMarker[..8]}",
            ShortCode = "ENG_COST_PRICE",
            Level = 3,
            ParentCategoryId = level2.Id,
            DisplayOrder = 1,
            Status = EntityStatus.Active
        };
        await Context.RiskCategories.AddAsync(level3);
        await SaveChangesAsync();
        return level3.Id;
    }

    private async Task<(int TypeId, int ProbId, int ProxId, int ImpactId, int CategoryId)> SeedAllLookupsAsync()
    {
        var typeId = await SeedRiskTypeAsync();
        var probId = await SeedRiskProbabilityAsync();
        var proxId = await SeedRiskProximityAsync();
        var impactId = await SeedRiskImpactLevelAsync();
        var categoryId = await SeedRiskCategoryLevel3Async();
        return (typeId, probId, proxId, impactId, categoryId);
    }

    #endregion

    #region P0 - Risk CRUD (Create, Read, Update, Delete)

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-001")]
    public async Task CreateRisk_WithValidData_ShouldPersist()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Risk {_testMarker}",
            Title = $"Test Risk {_testMarker}",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Description = "Test risk description",
            Recommendation = "Mitigate by testing",
            Impact = RiskImpact.Medium,
            RiskStatus = RiskStatus.Open,
            IdentifiedDate = DateTime.UtcNow,
            Status = EntityStatus.Active
        };

        // Act
        await Context.Risks.AddAsync(risk);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Risks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == risk.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be($"Test Risk {_testMarker}");
        saved.EntityType.Should().Be("Opportunity");
        saved.RiskTypeId.Should().Be(lookups.TypeId);
        saved.RiskCategoryId.Should().Be(lookups.CategoryId);
        saved.RiskStatus.Should().Be(RiskStatus.Open);
        saved.IsDeleted.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-002")]
    public async Task ReadRisk_ByEntityTypeAndId_ShouldReturnMatchingRisks()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var entityId = 42;
        for (int i = 0; i < 3; i++)
        {
            await Context.Risks.AddAsync(new Risk
            {
                Name = $"Risk {i} {_testMarker}",
                Title = $"Risk {i} for Entity {_testMarker}",
                EntityType = "Opportunity",
                EntityId = entityId,
                RiskTypeId = lookups.TypeId,
                RiskCategoryId = lookups.CategoryId,
                RiskProbabilityId = lookups.ProbId,
                RiskProximityId = lookups.ProxId,
                RiskImpactLevelId = lookups.ImpactId,
                Impact = RiskImpact.Low,
                RiskStatus = RiskStatus.Open,
                Status = EntityStatus.Active
            });
        }
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var risks = await Context.Risks
            .AsNoTracking()
            .Where(r => r.EntityType == "Opportunity"
                        && r.EntityId == entityId
                        && r.Name.Contains(_testMarker)
                        && !r.IsDeleted)
            .ToListAsync();

        // Assert
        risks.Should().HaveCount(3);
        risks.Should().OnlyContain(r => r.EntityId == entityId);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-003")]
    public async Task UpdateRisk_ChangeFields_ShouldPersist()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Update Risk {_testMarker}",
            Title = $"Original Title {_testMarker}",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Low,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };
        await Context.Risks.AddAsync(risk);
        await SaveChangesAsync();

        // Act
        risk.Title = $"Updated Title {_testMarker}";
        risk.Description = "Updated description";
        risk.RiskStatus = RiskStatus.Mitigated;
        risk.Impact = RiskImpact.High;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Risks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == risk.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be($"Updated Title {_testMarker}");
        saved.Description.Should().Be("Updated description");
        saved.RiskStatus.Should().Be(RiskStatus.Mitigated);
        saved.Impact.Should().Be(RiskImpact.High);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-004")]
    public async Task SoftDeleteRisk_ShouldSetIsDeletedFlag()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Delete Risk {_testMarker}",
            Title = $"To Be Deleted {_testMarker}",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Medium,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };
        await Context.Risks.AddAsync(risk);
        await SaveChangesAsync();

        // Act - soft delete
        risk.IsDeleted = true;
        risk.DeletedDate = DateTime.UtcNow;
        risk.DeletedBy = TestUserId;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.Risks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == risk.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedDate.Should().NotBeNull();
        deleted.DeletedBy.Should().Be(TestUserId);

        var activeRisks = await Context.Risks
            .AsNoTracking()
            .Where(r => r.Name.Contains(_testMarker) && !r.IsDeleted)
            .ToListAsync();
        activeRisks.Should().BeEmpty();
    }

    #endregion

    #region P1 - FK Constraints and Data Integrity

    [SkipIfNotPostgreSQLFact]
    [Trait("Category", "P1")]
    [Trait("Type", "DataIntegrity")]
    [Trait("TestId", "TC-RISK-005")]
    public async Task CreateRisk_WithInvalidRiskTypeId_ShouldBeRejectedByFK()
    {
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Bad Type {_testMarker}",
            Title = "Invalid RiskType",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = int.MaxValue,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Low,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };

        await Context.Risks.AddAsync(risk);
        Func<Task> act = async () => await SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("Category", "P1")]
    [Trait("Type", "DataIntegrity")]
    [Trait("TestId", "TC-RISK-006")]
    public async Task CreateRisk_WithInvalidCategoryId_ShouldBeRejectedByFK()
    {
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Bad Category {_testMarker}",
            Title = "Invalid Category",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = int.MaxValue,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Low,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };

        await Context.Risks.AddAsync(risk);
        Func<Task> act = async () => await SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region P1 - Risk Lookups

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-007")]
    public async Task RiskType_SeedData_ShouldHaveExpectedStructure()
    {
        // Arrange
        await SeedRiskTypeAsync("THREAT", "Threat");
        await SeedRiskTypeAsync("OPPORTUNITY", "Opportunity");
        Context.ChangeTracker.Clear();

        // Act
        var types = await Context.RiskTypes
            .AsNoTracking()
            .Where(rt => !rt.IsDeleted)
            .ToListAsync();

        // Assert
        types.Should().NotBeEmpty();
        types.Should().Contain(rt => rt.Code == "THREAT");
        types.Should().Contain(rt => rt.Code == "OPPORTUNITY");

        var opp = types.First(rt => rt.Code == "OPPORTUNITY");
        opp.IsResponseTypeMandatory.Should().BeTrue();

        var threat = types.First(rt => rt.Code == "THREAT");
        threat.IsResponseTypeMandatory.Should().BeFalse();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-008")]
    public async Task RiskCategory_Hierarchy_ShouldHaveThreeLevels()
    {
        // Arrange
        await SeedRiskCategoryLevel3Async();
        Context.ChangeTracker.Clear();

        // Act
        var categories = await Context.RiskCategories
            .AsNoTracking()
            .Where(rc => rc.Name.Contains(_testMarker) && !rc.IsDeleted)
            .OrderBy(rc => rc.Level)
            .ToListAsync();

        // Assert
        categories.Should().HaveCount(3);

        var l1 = categories.First(c => c.Level == 1);
        l1.ParentCategoryId.Should().BeNull();

        var l2 = categories.First(c => c.Level == 2);
        l2.ParentCategoryId.Should().Be(l1.Id);

        var l3 = categories.First(c => c.Level == 3);
        l3.ParentCategoryId.Should().Be(l2.Id);
    }

    #endregion

    #region P1 - Risk Status Transitions

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-009")]
    public async Task RiskStatusTransition_OpenToMitigated_ShouldPersist()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Status Transition {_testMarker}",
            Title = "Status test",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Medium,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };
        await Context.Risks.AddAsync(risk);
        await SaveChangesAsync();

        // Act - transition through statuses
        risk.RiskStatus = RiskStatus.UnderReview;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var afterReview = await Context.Risks
            .AsNoTracking()
            .FirstAsync(r => r.Id == risk.Id);
        afterReview.RiskStatus.Should().Be(RiskStatus.UnderReview);

        var tracked = await Context.Risks.FirstAsync(r => r.Id == risk.Id);
        tracked.RiskStatus = RiskStatus.Mitigated;
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var afterMitigated = await Context.Risks
            .AsNoTracking()
            .FirstAsync(r => r.Id == risk.Id);
        afterMitigated.RiskStatus.Should().Be(RiskStatus.Mitigated);
    }

    #endregion

    #region P1 - Navigation Properties

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-010")]
    public async Task Risk_NavigationProperties_ShouldLoadRelatedLookups()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var risk = new Risk
        {
            Name = $"Nav Props {_testMarker}",
            Title = "Navigation test",
            EntityType = "Opportunity",
            EntityId = 1,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Low,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        };
        await Context.Risks.AddAsync(risk);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act - load with includes
        var loaded = await Context.Risks
            .AsNoTracking()
            .Include(r => r.RiskTypeEntity)
            .Include(r => r.RiskCategory)
            .Include(r => r.RiskProbabilityEntity)
            .Include(r => r.RiskProximityEntity)
            .Include(r => r.RiskImpactLevelEntity)
            .FirstOrDefaultAsync(r => r.Id == risk.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.RiskTypeEntity.Should().NotBeNull();
        loaded.RiskTypeEntity!.Code.Should().Be("THREAT");
        loaded.RiskCategory.Should().NotBeNull();
        loaded.RiskProbabilityEntity.Should().NotBeNull();
        loaded.RiskProximityEntity.Should().NotBeNull();
        loaded.RiskImpactLevelEntity.Should().NotBeNull();
    }

    #endregion

    #region P2 - Multiple Risks per Entity

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-011")]
    public async Task MultipleRisks_ForSameEntity_ShouldAllPersist()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var entityId = 99;
        var risks = Enumerable.Range(1, 5).Select(i => new Risk
        {
            Name = $"Multi {i} {_testMarker}",
            Title = $"Risk {i}",
            EntityType = "Opportunity",
            EntityId = entityId,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = (RiskImpact)(i % 3 + 1),
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active
        }).ToList();

        // Act
        await Context.Risks.AddRangeAsync(risks);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Risks
            .AsNoTracking()
            .Where(r => r.EntityId == entityId && r.Name.Contains(_testMarker) && !r.IsDeleted)
            .ToListAsync();
        saved.Should().HaveCount(5);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-RISK-012")]
    public async Task SoftDeletedRisks_ShouldBeExcludedFromActiveQuery()
    {
        // Arrange
        var lookups = await SeedAllLookupsAsync();
        var entityId = 100;

        var activeRisk = new Risk
        {
            Name = $"Active {_testMarker}",
            Title = "Still active",
            EntityType = "Opportunity",
            EntityId = entityId,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.Low,
            RiskStatus = RiskStatus.Open,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        var deletedRisk = new Risk
        {
            Name = $"Deleted {_testMarker}",
            Title = "Soft deleted",
            EntityType = "Opportunity",
            EntityId = entityId,
            RiskTypeId = lookups.TypeId,
            RiskCategoryId = lookups.CategoryId,
            RiskProbabilityId = lookups.ProbId,
            RiskProximityId = lookups.ProxId,
            RiskImpactLevelId = lookups.ImpactId,
            Impact = RiskImpact.High,
            RiskStatus = RiskStatus.Closed,
            Status = EntityStatus.Active,
            IsDeleted = true,
            DeletedDate = DateTime.UtcNow,
            DeletedBy = TestUserId
        };

        await Context.Risks.AddRangeAsync(activeRisk, deletedRisk);
        await SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var activeOnly = await Context.Risks
            .AsNoTracking()
            .Where(r => r.EntityId == entityId && r.Name.Contains(_testMarker) && !r.IsDeleted)
            .ToListAsync();

        // Assert
        activeOnly.Should().HaveCount(1);
        activeOnly.Single().Title.Should().Be("Still active");
    }

    #endregion
}
