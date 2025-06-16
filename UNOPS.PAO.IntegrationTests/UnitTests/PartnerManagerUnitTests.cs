using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests unitaires simples pour démontrer les tests de base de données en mémoire
/// Ces tests fonctionnent immédiatement sans configuration DI complexe
/// </summary>
public class PartnerManagerUnitTests
{
    [Fact]
    public async Task InMemoryDatabase_CanStoreAndRetrievePartners()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var faker = TestDataBuilder.GetPartnerFaker();
        var testPartners = faker.Generate(3);

        // Act
        context.Partners.AddRange(testPartners);
        await context.SaveChangesAsync();

        // Assert
        var retrievedPartners = await context.Partners.ToListAsync();
        retrievedPartners.Should().HaveCount(3);
        retrievedPartners.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Name));
    }

    [Fact]
    public async Task InMemoryDatabase_CanFilterPartnersByStatus()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(5);
        
        // Set specific statuses
        partners[0].Status = "Active";
        partners[1].Status = "Active";
        partners[2].Status = "Inactive";
        partners[3].Status = "Active";
        partners[4].Status = "Prospect";

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();

        // Act
        var activePartners = await context.Partners
            .Where(p => p.Status == "Active")
            .ToListAsync();

        // Assert
        activePartners.Should().HaveCount(3);
        activePartners.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public async Task InMemoryDatabase_CanSearchPartnersByName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(3);
        
        // Set specific names for testing
        partners[0].Name = "ACME Corporation";
        partners[1].Name = "Global Tech Solutions";
        partners[2].Name = "ACME Industries";

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();

        // Act
        var acmePartners = await context.Partners
            .Where(p => p.Name.Contains("ACME"))
            .ToListAsync();

        // Assert
        acmePartners.Should().HaveCount(2);
        acmePartners.Should().OnlyContain(p => p.Name.Contains("ACME"));
    }

    [Fact]
    public async Task InMemoryDatabase_CanOrderPartnersByName()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var partners = new List<UNOPSPartner>
        {
            new() { Name = "Zebra Corp", Status = "Active", NewEngagement = "true", ShortName = "ZC", PooledFund = "false", DDRequired = "false", DDEACDone = "false", LevyPotentiallyApplies = "false" },
            new() { Name = "Alpha Inc", Status = "Active", NewEngagement = "true", ShortName = "AI", PooledFund = "false", DDRequired = "false", DDEACDone = "false", LevyPotentiallyApplies = "false" },
            new() { Name = "Beta LLC", Status = "Active", NewEngagement = "true", ShortName = "BL", PooledFund = "false", DDRequired = "false", DDEACDone = "false", LevyPotentiallyApplies = "false" }
        };

        context.Partners.AddRange(partners);
        await context.SaveChangesAsync();

        // Act
        var orderedPartners = await context.Partners
            .OrderBy(p => p.Name)
            .ToListAsync();

        // Assert
        orderedPartners.Should().HaveCount(3);
        orderedPartners[0].Name.Should().Be("Alpha Inc");
        orderedPartners[1].Name.Should().Be("Beta LLC");
        orderedPartners[2].Name.Should().Be("Zebra Corp");
    }

    [Fact]
    public void TestDataBuilder_GeneratesValidPartners()
    {
        // Arrange & Act
        var faker = TestDataBuilder.GetPartnerFaker();
        var partner = faker.Generate();

        // Assert
        partner.Should().NotBeNull();
        partner.Name.Should().NotBeNullOrEmpty();
        partner.ShortName.Should().NotBeNullOrEmpty();
        partner.Status.Should().BeOneOf("Active", "Inactive", "Prospect");
        partner.PartnerCode.Should().StartWith("P");
        partner.PartnerGroupCode.Should().BeOneOf("NGO", "GOV", "PRI", "UN");
    }

    // Helper method to create in-memory database context
    private static UNOPSAppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Mock the required dependencies
        var mockUserService = new Mock<UserResolverService<int>>(Mock.Of<IUserInfoService>());
        var mockDbSchema = new Mock<IDbContextSchema>();

        return new UNOPSAppDbContext(options, mockUserService.Object, mockDbSchema.Object);
    }
}