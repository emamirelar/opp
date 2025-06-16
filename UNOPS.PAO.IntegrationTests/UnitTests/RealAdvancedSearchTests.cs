using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.Models;
using Xunit;
using Moq;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using System.Text.Json;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests pour valider la logique OR/AND dans PartnerCompositeSpecification avec des vraies données
/// Ces tests utilisent la spécification réelle avec une base de données en mémoire
/// </summary>
public class RealAdvancedSearchTests
{
    [Fact]
    public async Task PartnerCompositeSpecification_WithAdvancedSearchOR_ReturnsCorrectResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Créer une recherche avancée avec logique OR: Status = "Inactive" OR Name contains "ACME"
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Inactive", Operator = "is", LogicalOperator = "OR" },
            new SearchCriteria { Field = "Name", Value = "ACME", Operator = "like", LogicalOperator = null }
        };

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = JsonSerializer.Serialize(searchCriteria)
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(3, "Should find Beta Industries (Inactive) + ACME Corporation + ACME Global Services");
        
        var expectedNames = new[] { "Beta Industries", "ACME Corporation", "ACME Global Services" };
        partners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
        
        // Vérifier que les critères OR sont bien appliqués
        partners.Should().Contain(p => p.Status == "Inactive");
        partners.Should().Contain(p => p.Name.Contains("ACME"));
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithAdvancedSearchAND_ReturnsIntersection()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Créer une recherche avancée avec logique AND: Status = "Active" AND Name contains "Global"
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Active", Operator = "is", LogicalOperator = "AND" },
            new SearchCriteria { Field = "Name", Value = "Global", Operator = "like", LogicalOperator = null }
        };

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = JsonSerializer.Serialize(searchCriteria)
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(2, "Should find Global Tech Solutions + ACME Global Services (both Active + contain Global)");
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services" };
        partners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
        
        // Vérifier que les critères AND sont bien appliqués
        partners.Should().OnlyContain(p => p.Status == "Active");
        partners.Should().OnlyContain(p => p.Name.Contains("Global"));
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithMixedANDOR_ReturnsComplexResult()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Recherche complexe: (Status = "Active" AND Name contains "Tech") OR Status = "Prospect"
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Active", Operator = "is", LogicalOperator = "AND" },
            new SearchCriteria { Field = "Name", Value = "Tech", Operator = "like", LogicalOperator = "OR" },
            new SearchCriteria { Field = "Status", Value = "Prospect", Operator = "is", LogicalOperator = null }
        };

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = JsonSerializer.Serialize(searchCriteria)
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert - Devrait trouver: Global Tech Solutions (Active+Tech) + Global Finance Corp (Prospect)
        partners.Should().HaveCount(2);
        
        var expectedNames = new[] { "Global Tech Solutions", "Global Finance Corp" };
        partners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithDifferentOperators_ReturnsCorrectResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Test de différents opérateurs: Status = "Active" OR Name is not "Beta Industries"
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Active", Operator = "is", LogicalOperator = "OR" },
            new SearchCriteria { Field = "Name", Value = "Beta Industries", Operator = "is not", LogicalOperator = null }
        };

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = JsonSerializer.Serialize(searchCriteria)
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert - Devrait trouver tous sauf peut-être Beta Industries (selon l'implémentation)
        partners.Should().HaveCountGreaterThan(3);
        
        // Les partenaires Active devraient être inclus
        partners.Should().Contain(p => p.Name == "ACME Corporation");
        partners.Should().Contain(p => p.Name == "Global Tech Solutions");
        partners.Should().Contain(p => p.Name == "ACME Global Services");
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithRegularAndAdvancedFilters_CombinesCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Combiner filtres réguliers ET recherche avancée
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Name", Value = "Global", Operator = "like", LogicalOperator = null }
        };

        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active", // Filtre régulier
            AdvancedSearch = true,
            SearchCriteria = JsonSerializer.Serialize(searchCriteria)
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert - Devrait appliquer AND entre filtre régulier et recherche avancée
        partners.Should().HaveCount(2, "Should find partners that are Active AND contain Global");
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services" };
        partners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
        
        partners.Should().OnlyContain(p => p.Status == "Active");
        partners.Should().OnlyContain(p => p.Name.Contains("Global"));
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithInvalidJSON_HandlesGracefully()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = "invalid json{}" // JSON invalide
        };

        // Act & Assert - Ne devrait pas lever d'exception
        var specification = new PartnerCompositeSpecification(filterRequest);
        
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Devrait retourner tous les partenaires (pas de critères valides)
        partners.Should().HaveCount(5);
    }

    [Fact]
    public async Task PartnerCompositeSpecification_EmptyAdvancedSearch_ReturnsAll()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = "[]" // Tableau vide
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(5, "Empty advanced search should return all partners");
    }

    #region Helper Methods

    private static UNOPSAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Mock required dependencies
        var mockUserService = new Mock<UserResolverService<int>>(Mock.Of<IUserInfoService>());
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(x => x.Schema).Returns("test");

        return new UNOPSAppDbContext(options, mockUserService.Object, mockDbSchema.Object);
    }

    private static async Task SeedTestDataAsync(UNOPSAppDbContext context)
    {
        var partners = new List<UNOPSPartner>
        {
            CreatePartner("ACME Corporation", "Active", "ACME"),
            CreatePartner("Global Tech Solutions", "Active", "GTS"),
            CreatePartner("Beta Industries", "Inactive", "BETA"),
            CreatePartner("Global Finance Corp", "Prospect", "GFC"),
            CreatePartner("ACME Global Services", "Active", "AGS")
        };

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();
    }

    private static UNOPSPartner CreatePartner(string name, string status, string shortName)
    {
        return new UNOPSPartner
        {
            Name = name,
            Status = status,
            ShortName = shortName,
            NewEngagement = "true",
            PooledFund = "false",
            DDRequired = "false",
            DDEACDone = "false",
            LevyPotentiallyApplies = "false",
            PartnerCode = $"P{Random.Shared.Next(1000, 9999)}",
            PartnerGroupCode = "NGO",
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 100)),
            LastModifiedDate = DateTime.UtcNow
        };
    }

    #endregion
}