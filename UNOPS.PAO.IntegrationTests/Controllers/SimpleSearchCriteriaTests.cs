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

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Tests simples des critères de recherche sans WebApplicationFactory
/// Teste directement la logique de filtrage avec base de données en mémoire
/// </summary>
public class SimpleSearchCriteriaTests
{
    [Fact]
    public async Task PartnerCompositeSpecification_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active"
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(3);
        partners.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithNameFilter_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            Name = "ACME"
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(2);
        partners.Should().OnlyContain(p => p.Name.Contains("ACME"));
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithSearchText_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            SearchText = "Global"
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(2);
        partners.Should().OnlyContain(p => 
            p.Name.Contains("Global") || 
            p.ShortName.Contains("Global"));
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithMultipleFilters_ReturnsIntersection()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active",
            SearchText = "Global"
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(1);
        partners.Should().OnlyContain(p => 
            p.Status == "Active" && 
            (p.Name.Contains("Global") || p.ShortName.Contains("Global")));
    }

    [Fact]
    public async Task PartnerQuery_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedManyPartnersAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            PageIndex = 2,
            PageSize = 2,
            OrderBy = "Name",
            Ascending = true
        };

        // Act
        var query = context.Partners.AsQueryable();
        
        // Apply ordering
        if (filterRequest.OrderBy == "Name")
        {
            query = filterRequest.Ascending == true
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name);
        }

        // Apply pagination
        var totalCount = await query.CountAsync();
        var partners = await query
            .Skip((filterRequest.PageIndex - 1) * filterRequest.PageSize)
            .Take(filterRequest.PageSize)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(2);
        totalCount.Should().Be(10);
        
        // Verify sorting
        partners.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task PartnerQuery_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        // Act - Ascending
        var partnersAsc = await context.Partners
            .OrderBy(p => p.Name)
            .ToListAsync();

        // Act - Descending
        var partnersDesc = await context.Partners
            .OrderByDescending(p => p.Name)
            .ToListAsync();

        // Assert
        partnersAsc.Should().BeInAscendingOrder(p => p.Name);
        partnersDesc.Should().BeInDescendingOrder(p => p.Name);
        
        partnersAsc.First().Name.Should().Be("ACME Corporation");
        partnersDesc.First().Name.Should().Be("Global Tech Solutions");
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithInvalidStatus_ReturnsEmpty()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest
        {
            Status = "NonExistentStatus"
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().BeEmpty();
    }

    [Fact]
    public async Task PartnerCompositeSpecification_WithEmptyFilter_ReturnsAll()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestDataAsync(context);

        var filterRequest = new PartnerFilterRequest();
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var partners = await context.Partners
            .Where(specification.Criteria)
            .ToListAsync();

        // Assert
        partners.Should().HaveCount(5);
    }

    [Fact]
    public void TestDataBuilder_GeneratesConsistentPartners()
    {
        // Arrange & Act
        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(10);

        // Assert
        partners.Should().HaveCount(10);
        partners.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Name));
        partners.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Status));
        partners.Should().OnlyContain(p => new[] { "Active", "Inactive", "Prospect" }.Contains(p.Status));
        partners.Should().OnlyContain(p => new[] { "NGO", "GOV", "PRI", "UN" }.Contains(p.PartnerGroupCode!));
    }

    [Fact]
    public async Task PartnerQuery_Performance_WithManyRecords()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedManyPartnersAsync(context, 1000);

        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active",
            SearchText = "Test",
            PageSize = 10,
            PageIndex = 1
        };

        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var partners = await context.Partners
            .Where(specification.Criteria)
            .OrderBy(p => p.Name)
            .Skip((filterRequest.PageIndex - 1) * filterRequest.PageSize)
            .Take(filterRequest.PageSize)
            .ToListAsync();
            
        stopwatch.Stop();

        // Assert
        partners.Should().HaveCountLessOrEqualTo(10);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should be fast with InMemory
    }

    // Helper methods
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

    private static async Task SeedManyPartnersAsync(UNOPSAppDbContext context, int count = 10)
    {
        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);

        // Ensure some variety in status
        for (int i = 0; i < count; i++)
        {
            if (i % 3 == 0) partners[i].Status = "Active";
            else if (i % 3 == 1) partners[i].Status = "Inactive";
            else partners[i].Status = "Prospect";

            // Add some with "Test" in name for search testing
            if (i % 5 == 0)
            {
                partners[i].Name = $"Test Company {i}";
            }
        }

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
}