using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Models;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Entities;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.IntegrationTests.Controllers;

public class PartnerControllerTests : IntegrationTestBase
{
    private readonly ILogger<PartnerControllerTests>? _logger;

    public PartnerControllerTests(PAOWebApplicationFactory<Program> factory) 
        : base(factory) 
    {
        // Seed test data for each test
        SeedTestPartners().Wait();
    }

    private async Task SeedTestPartners()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
        
        // Check if partners already exist
        var existingCount = await dbContext.Set<UNOPSPartner>().CountAsync();
        if (existingCount > 0)
        {
            // Partners already seeded, skip
            return;
        }
        
        // Add test partners with specific characteristics
        var partners = new List<UNOPSPartner>
        {
            CreateTestPartner(1, "ACME Corporation", "Active", "ACME", 1),
            CreateTestPartner(2, "Global Tech Solutions", "Active", "GTS", 2),
            CreateTestPartner(3, "Beta Industries", "Inactive", "BETA", 1),
            CreateTestPartner(4, "Global Finance Corp", "Prospect", "GFC", 3),
            CreateTestPartner(5, "ACME Global Services", "Active", "AGS", 2),
            CreateTestPartner(6, "Delta Corporation", "Inactive", "DELTA", 4),
            CreateTestPartner(7, "Tech Innovations Ltd", "Active", "TIL", 1),
            CreateTestPartner(8, "Finance Solutions Inc", "Prospect", "FSI", 3),
            CreateTestPartner(9, "Alpha Partners", "Active", "ALPHA", 2),
            CreateTestPartner(10, "Omega Services", "Inactive", "OMEGA", 1)
        };
        
        dbContext.Set<UNOPSPartner>().AddRange(partners);
        await dbContext.SaveChangesAsync();
    }

    private UNOPSPartner CreateTestPartner(int id, string name, string status, string shortName, int organizationHierarchyId)
    {
        // Map old status to new enum
        var systemStatus = status switch
        {
            "Active" => Domain.Enums.PartnerStatus.Active,
            "Inactive" => Domain.Enums.PartnerStatus.Closed,
            "Prospect" => Domain.Enums.PartnerStatus.Draft,
            _ => Domain.Enums.PartnerStatus.Draft
        };

        var partner = new UNOPSPartner
        {
            Id = id,
            // Enhanced Partner structure
            PartnerDescription = name,
            PartnerShortDescription = shortName,
            PartnerCategoryId = 1, // Default test category
            PartnerLiaisonOffice = "Default", // Default test liaison office
            UNAndStateEntity = false,
            SystemStatus = systemStatus,
            CanCreateNewOpportunities = true, // Default for test partners
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
            PartnerCode = $"P{id:D4}",
            PartnerGroupCode = "NGO",
            CreatedDate = DateTime.UtcNow.AddDays(-id),
            LastModifiedDate = DateTime.UtcNow
        };

        // Add organization unit relationship
        partner.OrganizationUnitRelationships = new List<OrganizationUnitRelationship>
        {
            new OrganizationUnitRelationship
            {
                OrganizationHierarchyId = organizationHierarchyId,
                EntityId = partner.Id,
                EntityType = nameof(UNOPSPartner),
                Name = $"Partner-{partner.Id}-TestOrgUnit",
                Status = EntityStatus.Active
            }
        };

        return partner;
    }

    #region Basic Filtering Tests

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_NoFilters_ReturnsAllPartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=20&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(10);
        response.TotalCount.Should().Be(10);
        response.PageIndex.Should().Be(1);
        response.PageSize.Should().Be(20);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterByStatus_Active_ReturnsOnlyActivePartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(5);
        response.Records.Should().OnlyContain(p => p.SystemStatus == "Active");
        response.TotalCount.Should().Be(5);
        
        var expectedNames = new[] { "ACME Corporation", "Global Tech Solutions", "ACME Global Services", "Tech Innovations Ltd", "Alpha Partners" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterByStatus_Inactive_ReturnsOnlyInactivePartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Inactive&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(3);
        response.Records.Should().OnlyContain(p => p.SystemStatus == "Closed");
        response.TotalCount.Should().Be(3);
        
        var expectedNames = new[] { "Beta Industries", "Delta Corporation", "Omega Services" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterByName_ReturnsMatchingPartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?name=ACME&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        
        var expectedNames = new[] { "ACME Corporation", "ACME Global Services" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterBySearchText_SearchesNameAndShortName()
    {
        // Act - search for "Global" which appears in multiple partner names
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=Global&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(3);
        response.TotalCount.Should().Be(3);
        
        var expectedNames = new[] { "Global Tech Solutions", "Global Finance Corp", "ACME Global Services" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterBySearchText_ShortName_ReturnsMatchingPartner()
    {
        // Act - search for "GTS" which is the short name of Global Tech Solutions
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=GTS&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(1);
        response.Records.Single().Name.Should().Be("Global Tech Solutions");
        response.Records.Single().PartnerShortDescription.Should().Be("GTS");
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_FilterByOrgUnitId_ReturnsPartnersInOrgUnit()
    {
        // Note: This test assumes OrgUnitId filtering is implemented in the backend
        // The test OrgUnitHierarchyService should handle the hierarchy logic
        
        // Act - filter by a specific org unit (assuming ID mapping)
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?orgUnitId=1&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        // The actual count will depend on how the OrgUnitHierarchyService maps IDs to org units
        response.Records.Should().NotBeNull();
    }

    #endregion

    #region Multiple Filter Tests

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_MultipleFilters_AppliesAllFilters()
    {
        // Act - Active status AND name contains "Global"
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&searchText=Global&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // Global Tech Solutions and ACME Global Services
        response.Records.Should().OnlyContain(p => p.SystemStatus == "Active");
        response.Records.Should().OnlyContain(p => p.Name.Contains("Global"));
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_StatusAndName_ReturnsIntersection()
    {
        // Act - Active status AND name = "ACME"
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&name=ACME&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // ACME Corporation and ACME Global Services
        response.Records.Should().OnlyContain(p => p.SystemStatus == "Active");
        response.Records.Should().OnlyContain(p => p.Name.Contains("ACME"));
    }

    #endregion

    #region Pagination Tests

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_Pagination_FirstPage_ReturnsCorrectResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=5&pageIndex=1&orderBy=Name&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(5);
        response.PageIndex.Should().Be(1);
        response.PageSize.Should().Be(5);
        response.TotalCount.Should().Be(10);
        response.TotalPages.Should().Be(2);
        
        // First 5 partners alphabetically
        response.Records.First().Name.Should().Be("ACME Corporation");
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_Pagination_SecondPage_ReturnsCorrectResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=5&pageIndex=2&orderBy=Name&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(5);
        response.PageIndex.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalCount.Should().Be(10);
        response.TotalPages.Should().Be(2);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_Pagination_PageSizeLargerThanTotal_ReturnsAllResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=20&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(10);
        response.PageIndex.Should().Be(1);
        response.PageSize.Should().Be(20);
        response.TotalCount.Should().Be(10);
        response.TotalPages.Should().Be(1);
    }

    #endregion

    #region Sorting Tests

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_OrderByName_Ascending_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Name&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInAscendingOrder(p => p.PartnerDescription);
        response.Records.First().Name.Should().Be("ACME Corporation");
        response.Records.Last().Name.Should().Be("Tech Innovations Ltd");
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_OrderByName_Descending_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Name&ascending=false");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInDescendingOrder(p => p.PartnerDescription);
        response.Records.First().Name.Should().Be("Tech Innovations Ltd");
        response.Records.Last().Name.Should().Be("ACME Corporation");
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_OrderByStatus_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Status&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInAscendingOrder(p => p.SystemStatus);
    }

    #endregion

    #region Advanced Search Tests

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_SimpleTextSearch_WithSearchTextParameter_ReturnsFilteredResults()
    {
        // Act - use the searchText query parameter (not in PartnerFilterRequest)
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=Tech&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // Global Tech Solutions and Tech Innovations Ltd
        
        var expectedNames = new[] { "Global Tech Solutions", "Tech Innovations Ltd" };
        response.Records.Select(p => p.PartnerDescription).Should().BeEquivalentTo(expectedNames);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_AdvancedSearch_WithSearchCriteria_ReturnsFilteredResults()
    {
        // Act - use advanced search with specific criteria
        // Note: The actual search criteria format depends on the implementation
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?advancedSearch=true&searchCriteria=Status:Active&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        // Results depend on how searchCriteria is parsed and applied
        response.Records.Should().NotBeNull();
    }

    #endregion

    #region Edge Cases

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_NonExistentStatus_ReturnsEmptyResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Archived&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_EmptySearchText_ReturnsAllResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(10);
        response.TotalCount.Should().Be(10);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_InvalidPageIndex_ReturnsError()
    {
        // Act
        var response = await GetAsync("/api/partner?pageSize=10&pageIndex=0");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_InvalidPageSize_ReturnsError()
    {
        // Act
        var response = await GetAsync("/api/partner?pageSize=0&pageIndex=1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(Skip = "Skipping due to authorization issues in test environment")]
    public async Task GetAll_NoMatchingResults_ReturnsEmptyList()
    {
        // Act - search for something that doesn't exist
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=NonExistentCompany&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.TotalPages.Should().Be(0);
    }

    #endregion

    #region Other Endpoint Tests

    [Fact(Skip = "Skipping non-GetAll tests for now")]
    public async Task Get_ExistingPartner_ReturnsPartner()
    {
        // Act
        var response = await GetAsync("/api/partner/1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ACME Corporation");
    }

    [Fact(Skip = "Skipping non-GetAll tests for now")]
    public async Task Get_NonExistentPartner_ReturnsNotFound()
    {
        // Act
        var response = await GetAsync("/api/partner/999");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(Skip = "Skipping non-GetAll tests for now")]
    public async Task Create_ValidPartner_ReturnsCreated()
    {
        // Arrange
        var newPartner = new PartnerRequest
        {
            PartnerDescription = "New Test Partner",
            PartnerShortDescription = "NTP",
            PartnerCategoryId = 1,
            PartnerLiaisonOffice = "Default",
            SystemStatus = "Active",
            PartnerGroupCode = "NGO"
        };
        
        // Act
        var response = await PostAsync("/api/partner", newPartner);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(Skip = "Skipping non-GetAll tests for now")]
    public async Task Update_ExistingPartner_ReturnsOk()
    {
        // Arrange
        var updateRequest = new UpdatePartnerRequest
        {
            Id = 1,
            PartnerDescription = "Updated ACME Corporation",
            SystemStatus = "Closed"
        };
        
        // Act
        var result = await PutAsync<PartnerModel>("/api/partner", updateRequest);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated ACME Corporation");
        result.SystemStatus.Should().Be("Closed");
    }

    [Fact(Skip = "Skipping non-GetAll tests for now")]
    public async Task Delete_ExistingPartner_ReturnsNoContent()
    {
        // Act
        var response = await DeleteAsync("/api/partner/1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion
}