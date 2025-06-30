using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests unitaires simples pour les critères de recherche de partenaires
/// Ces tests valident la logique de filtrage sans avoir besoin d'infrastructure complexe
/// </summary>
public class PartnerSearchCriteriaUnitTests
{
    [Fact]
    public void TestDataBuilder_GeneratesValidPartners()
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
    public void PartnerCompositeSpecification_WithStatusFilter_CreatesCorrectSpecification()
    {
        // Arrange
        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active"
        };

        // Act
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Assert
        specification.Should().NotBeNull();
        specification.Criteria.Should().NotBeNull();
    }

    [Fact]
    public void PartnerFilter_WithStatus_FiltersInMemoryCollection()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().HaveCount(3);
        filteredPartners.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public void PartnerFilter_WithName_FiltersInMemoryCollection()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest
        {
            Name = "ACME"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().HaveCount(2);
        filteredPartners.Should().OnlyContain(p => p.Name.Contains("ACME"));
    }

    [Fact]
    public void PartnerFilter_WithSearchText_FiltersInMemoryCollection()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest
        {
            SearchText = "Global"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().HaveCount(2);
        filteredPartners.Should().OnlyContain(p => 
            p.Name.Contains("Global") || 
            (p.ShortName != null && p.ShortName.Contains("Global")));
    }

    [Fact]
    public void PartnerFilter_WithMultipleCriteria_ReturnsIntersection()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active",
            SearchText = "Global"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().HaveCount(1);
        filteredPartners.Should().OnlyContain(p => 
            p.Status == "Active" && 
            (p.Name.Contains("Global") || (p.ShortName != null && p.ShortName.Contains("Global"))));
    }

    [Fact]
    public void PartnerFilter_WithNonExistentStatus_ReturnsEmpty()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest
        {
            Status = "NonExistent"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().BeEmpty();
    }

    [Fact]
    public void PartnerFilter_WithEmptyFilter_ReturnsAllPartners()
    {
        // Arrange
        var partners = GetTestPartners();
        var filterRequest = new PartnerFilterRequest(); // Aucun filtre
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var filteredPartners = partners.Where(specification.Criteria.Compile()).ToList();

        // Assert
        filteredPartners.Should().HaveCount(5);
    }

    [Fact]
    public void PartnerCollection_CanBeSorted()
    {
        // Arrange
        var partners = GetTestPartners();

        // Act
        var sortedAsc = partners.OrderBy(p => p.Name).ToList();
        var sortedDesc = partners.OrderByDescending(p => p.Name).ToList();

        // Assert
        sortedAsc.Should().BeInAscendingOrder(p => p.Name);
        sortedDesc.Should().BeInDescendingOrder(p => p.Name);
        
        sortedAsc.First().Name.Should().Be("ACME Corporation");
        sortedDesc.First().Name.Should().Be("Global Tech Solutions");
    }

    [Fact]
    public void PartnerCollection_SupportsPagination()
    {
        // Arrange
        var partners = GetTestPartners().OrderBy(p => p.Name).ToList();
        var pageSize = 2;
        var pageIndex = 2; // Deuxième page (index 2 = skip 2, take 2)

        // Act
        var pagedPartners = partners
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Assert
        pagedPartners.Should().HaveCount(2);
        pagedPartners[0].Name.Should().Be("ACME Global Services");
        pagedPartners[1].Name.Should().Be("Beta Industries");
    }

    [Fact]
    public void PartnerFilter_Performance_WithLargeDataset()
    {
        // Arrange
        var faker = TestDataBuilder.GetPartnerFaker();
        var largeDataset = faker.Generate(1000);
        
        // Ensure variety in data
        for (int i = 0; i < 1000; i++)
        {
            if (i % 3 == 0) largeDataset[i].Status = "Active";
            else if (i % 3 == 1) largeDataset[i].Status = "Inactive";
            else largeDataset[i].Status = "Prospect";

            if (i % 10 == 0) largeDataset[i].Name = $"Test Company {i}";
        }

        var filterRequest = new PartnerFilterRequest
        {
            Status = "Active",
            SearchText = "Test"
        };
        var specification = new PartnerCompositeSpecification(filterRequest);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var filteredPartners = largeDataset
            .Where(specification.Criteria.Compile())
            .Take(10)
            .ToList();
            
        stopwatch.Stop();

        // Assert
        filteredPartners.Should().HaveCountLessOrEqualTo(10);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should be very fast with in-memory
        
        filteredPartners.Should().OnlyContain(p => p.Status == "Active");
        filteredPartners.Should().OnlyContain(p => p.Name.Contains("Test"));
    }

    #region Helper Methods

    private static List<UNOPSPartner> GetTestPartners()
    {
        return new List<UNOPSPartner>
        {
            CreatePartner("ACME Corporation", "Active", "ACME"),
            CreatePartner("Global Tech Solutions", "Active", "GTS"),
            CreatePartner("Beta Industries", "Inactive", "BETA"),
            CreatePartner("Global Finance Corp", "Prospect", "GFC"),
            CreatePartner("ACME Global Services", "Active", "AGS")
        };
    }

    private static UNOPSPartner CreatePartner(string name, string status, string shortName)
    {
        return new UNOPSPartner
        {
            Id = Random.Shared.Next(1, 1000),
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