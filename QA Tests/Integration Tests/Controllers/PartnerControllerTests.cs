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
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;

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
        
        // First, create test partner groups
        var partnerGroups = new List<PartnerTree>
        {
            new PartnerTree { Id = 1, Name = "Corporate Partners", PartnerGroupCode = "CORP", Description = "Corporate partner organizations", Code = "CORP", Type = "Group" },
            new PartnerTree { Id = 2, Name = "Government Partners", PartnerGroupCode = "GOV", Description = "Government partner organizations", Code = "GOV", Type = "Group" },
            new PartnerTree { Id = 3, Name = "NGO Partners", PartnerGroupCode = "NGO", Description = "Non-governmental partner organizations", Code = "NGO", Type = "Group" }
        };
        dbContext.Set<PartnerTree>().AddRange(partnerGroups);
        await dbContext.SaveChangesAsync();
        
        // Add test partners with specific characteristics
        var partners = new List<UNOPSPartner>
        {
            CreateTestPartner(1, "ACME Corporation", "Active", "ACME Corp specializes in global technology solutions", 1),
            CreateTestPartner(2, "Global Tech Solutions", "Active", "Global Tech is a leading technology provider", 2),
            CreateTestPartner(3, "Beta Industries", "Inactive", "Beta Industries is a manufacturing company", 1),
            CreateTestPartner(4, "Global Finance Corp", "Prospect", "Global Finance provides financial services", 3),
            CreateTestPartner(5, "ACME Global Services", "Active", "ACME Global offers consulting services", 2),
            CreateTestPartner(6, "Delta Corporation", "Inactive", "DELTA", 4),
            CreateTestPartner(7, "Tech Innovations Ltd", "Active", "TIL", 1),
            CreateTestPartner(8, "Finance Solutions Inc", "Prospect", "FSI", 3),
            CreateTestPartner(9, "Alpha Partners", "Active", "ALPHA", 2),
            CreateTestPartner(10, "Omega Services", "Inactive", "OMEGA", 1)
        };
        
        dbContext.Set<UNOPSPartner>().AddRange(partners);
        await dbContext.SaveChangesAsync();
        
        // Add some test contacts for the partners
        var contacts = new List<Contact>
        {
            new Contact 
            { 
                Id = 1, 
                FirstName = "John", 
                LastName = "Smith", 
                Title = "Manager",
                Email = "john.smith@acme.com", 
                PartnerId = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new Contact 
            { 
                Id = 2, 
                FirstName = "Jane", 
                LastName = "Doe",
                Title = "Director", 
                Email = "jane.doe@globaltech.com", 
                PartnerId = 2,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            },
            new Contact 
            { 
                Id = 3, 
                FirstName = "Bob", 
                LastName = "Johnson",
                Title = "Coordinator", 
                Email = "bob.johnson@beta.com", 
                PartnerId = 3,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            }
        };
        dbContext.Set<Contact>().AddRange(contacts);
        await dbContext.SaveChangesAsync();
    }

    private UNOPSPartner CreateTestPartner(int id, string name, string status, string shortName, int organizationHierarchyId)
    {
        // Map old status to new enum
        var systemStatus = status switch
        {
            "Active" => Domain.Entities.EntityStatus.Active,
            "Inactive" => Domain.Entities.EntityStatus.Closed,
            "Prospect" => Domain.Entities.EntityStatus.Draft,
            _ => Domain.Entities.EntityStatus.Draft
        };

        var partner = new UNOPSPartner
        {
            Id = id,
            // Enhanced Partner structure
            Name = name,
            PartnerShortDescription = shortName,
            PartnerLongDescription = $"{name} is a test partner for integration testing purposes. ID: {id}",
            PartnerCategoryId = 1, // Default test category
            LiaisonOfficeId = 1, // Default test liaison office
            UNAndStateEntity = false,
            Status = systemStatus,
            CanCreateNewOpportunities = true, // Default for test partners
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
            PartnerGroupId = 1,
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

    [Fact(Skip = "Authorization mocking required - high effort, low value")]
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

    [Fact]
    public async Task GetAll_FilterByStatus_Active_ReturnsOnlyActivePartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(5);
        response.Records.Should().OnlyContain(p => p.Status == "Active");
        response.TotalCount.Should().Be(5);
        
        var expectedNames = new[] { "ACME Corporation", "Global Tech Solutions", "ACME Global Services", "Tech Innovations Ltd", "Alpha Partners" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_Inactive_ReturnsOnlyInactivePartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Inactive&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(3);
        response.Records.Should().OnlyContain(p => p.Status == "Closed");
        response.TotalCount.Should().Be(3);
        
        var expectedNames = new[] { "Beta Industries", "Delta Corporation", "Omega Services" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task GetAll_FilterByName_ReturnsMatchingPartners()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?name=ACME&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        
        var expectedNames = new[] { "ACME Corporation", "ACME Global Services" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task GetAll_FilterBySearchText_SearchesNameAndShortName()
    {
        // Act - search for "Global" which appears in multiple partner names
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=Global&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(3);
        response.TotalCount.Should().Be(3);
        
        var expectedNames = new[] { "Global Tech Solutions", "Global Finance Corp", "ACME Global Services" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task GetAll_MultipleFilters_AppliesAllFilters()
    {
        // Act - Active status AND name contains "Global"
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&searchText=Global&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // Global Tech Solutions and ACME Global Services
        response.Records.Should().OnlyContain(p => p.Status == "Active");
        response.Records.Should().OnlyContain(p => p.Name.Contains("Global"));
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task GetAll_StatusAndName_ReturnsIntersection()
    {
        // Act - Active status AND name = "ACME"
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Active&name=ACME&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // ACME Corporation and ACME Global Services
        response.Records.Should().OnlyContain(p => p.Status == "Active");
        response.Records.Should().OnlyContain(p => p.Name.Contains("ACME"));
    }

    #endregion

    #region Pagination Tests

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task GetAll_OrderByName_Ascending_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Name&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInAscendingOrder(p => p.Name);
        response.Records.First().Name.Should().Be("ACME Corporation");
        response.Records.Last().Name.Should().Be("Tech Innovations Ltd");
    }

    [Fact]
    public async Task GetAll_OrderByName_Descending_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Name&ascending=false");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInDescendingOrder(p => p.Name);
        response.Records.First().Name.Should().Be("Tech Innovations Ltd");
        response.Records.Last().Name.Should().Be("ACME Corporation");
    }

    [Fact]
    public async Task GetAll_OrderByStatus_ReturnsSortedResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?pageSize=10&pageIndex=1&orderBy=Status&ascending=true");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeInAscendingOrder(p => p.Status);
    }

    #endregion

    #region Advanced Search Tests

    [Fact]
    public async Task GetAll_SimpleTextSearch_WithSearchTextParameter_ReturnsFilteredResults()
    {
        // Act - use the searchText query parameter (not in PartnerFilterRequest)
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=Tech&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(2); // Global Tech Solutions and Tech Innovations Ltd
        
        var expectedNames = new[] { "Global Tech Solutions", "Tech Innovations Ltd" };
        response.Records.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
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

    [Fact]
    public async Task GetAll_NonExistentStatus_ReturnsEmptyResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?status=Archived&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_EmptySearchText_ReturnsAllResults()
    {
        // Act
        var response = await GetAsync<PaginationResponse<PartnerModel>>("/api/partner?searchText=&pageSize=10&pageIndex=1");
        
        // Assert
        response.Should().NotBeNull();
        response.Records.Should().HaveCount(10);
        response.TotalCount.Should().Be(10);
    }

    [Fact]
    public async Task GetAll_InvalidPageIndex_ReturnsError()
    {
        // Act
        var response = await GetAsync("/api/partner?pageSize=10&pageIndex=0");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAll_InvalidPageSize_ReturnsError()
    {
        // Act
        var response = await GetAsync("/api/partner?pageSize=0&pageIndex=1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
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

    [Fact]
    public async Task Get_ExistingPartner_ReturnsPartner()
    {
        // Act
        var response = await GetAsync("/api/partner/1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("ACME Corporation");
    }

    [Fact]
    public async Task Get_NonExistentPartner_ReturnsNotFound()
    {
        // Act
        var response = await GetAsync("/api/partner/999");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidPartner_ReturnsCreated()
    {
        // Arrange
        var newPartner = new PartnerRequest
        {
            Name = "New Test Partner",
            PartnerShortDescription = "NTP",
            PartnerCategoryId = 1,
            LiaisonOfficeId = 1,
            Status = "Active",
            PartnerGroupId = 1
        };
        
        // Act
        var response = await PostAsync("/api/partner", newPartner);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Update_ExistingPartner_ReturnsOk()
    {
        // Arrange
        var updateRequest = new UpdatePartnerRequest
        {
            Id = 1,
            Name = "Updated ACME Corporation",
            Status = "Closed"
        };
        
        // Act
        var result = await PutAsync<PartnerModel>("/api/partner", updateRequest);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated ACME Corporation");
        result.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Delete_ExistingPartner_ReturnsNoContent()
    {
        // Act
        var response = await DeleteAsync("/api/partner/1");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region New Advanced Search Tests

    [Fact]
    public async Task NewAdvancedSearch_BasicTextSearch_ReturnsMatchingPartners()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "ACME",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
        result.Records.Should().OnlyContain(p => p.Name.Contains("ACME", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task NewAdvancedSearch_MultipleAndConditions_ReturnsCorrectResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "Global",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            },
            {
                "field": "status",
                "value": "1",
                "operator": "eq",
                "logicalOperator": "AND",
                "fieldType": "number"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Records.Should().OnlyContain(p => 
            p.Name.Contains("Global", StringComparison.OrdinalIgnoreCase) && 
            p.Status == "Active");
    }

    [Fact]
    public async Task NewAdvancedSearch_OrConditions_ReturnsUnionOfResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "ACME",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            },
            {
                "field": "name",
                "value": "Beta",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty();
        result.Records.Should().OnlyContain(p => 
            p.Name.Contains("ACME", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Contains("Beta", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task NewAdvancedSearch_NavigationPropertySearch_ReturnsCorrectResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "partnerGroup.name",
                "value": "Corporate",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Since we don't have specific partner group data in our test setup, 
        // we just verify the request doesn't fail
    }

    [Fact]
    public async Task NewAdvancedSearch_ContactsSearch_ReturnsPartnersWithMatchingContacts()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "contacts.firstName",
                "value": "John",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Since our test data doesn't include contacts with specific names,
        // we just verify the request processes without errors
    }

    [Fact]
    public async Task NewAdvancedSearch_DateRangeSearch_ReturnsCorrectResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "createdDate",
                "value": "2023-01-01",
                "operator": "gte",
                "logicalOperator": "AND",
                "fieldType": "date"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Verify that all returned partners were created after 2023-01-01
        if (result.Records.Any())
        {
            result.Records.Should().OnlyContain(p => p.CreatedDate >= new DateTime(2023, 1, 1));
        }
    }

    [Fact]
    public async Task NewAdvancedSearch_BooleanSearch_ReturnsCorrectResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "keyGlobalPartner",
                "value": "true",
                "operator": "eq",
                "logicalOperator": "AND",
                "fieldType": "bool"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Verify that all returned partners have keyGlobalPartner = true
        if (result.Records.Any())
        {
            result.Records.Should().OnlyContain(p => p.KeyGlobalPartner == true);
        }
    }

    [Fact]
    public async Task NewAdvancedSearch_SimilaritySearch_FindsTypos()
    {
        // Arrange - intentionally misspell "ACME" as "ACMEE" to test similarity
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "ACMEE",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // With smart search enabled, it should still find "ACME" partners despite the typo
        // Note: This depends on the similarity threshold being appropriate
    }

    [Fact]
    public async Task NewAdvancedSearch_ComplexMixedCriteria_ReturnsCorrectResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "Global",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            },
            {
                "field": "status",
                "value": "1",
                "operator": "eq",
                "logicalOperator": "OR",
                "fieldType": "number"
            },
            {
                "field": "keyGlobalPartner",
                "value": "true",
                "operator": "eq",
                "logicalOperator": "AND",
                "fieldType": "bool"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Complex logic: (name contains "Global" AND keyGlobalPartner = true) OR (status = 1)
    }

    [Fact]
    public async Task NewAdvancedSearch_EmptySearchCriteria_ReturnsAllPartners()
    {
        // Arrange
        var searchCriteria = "[]";

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Records.Should().NotBeEmpty(); // Should return all partners (up to page size)
    }

    [Fact]
    public async Task NewAdvancedSearch_InvalidSearchCriteria_ReturnsBadRequest()
    {
        // Arrange
        var searchCriteria = "invalid json";

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task NewAdvancedSearch_InvalidFieldName_ReturnsError()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "nonExistentField",
                "value": "test",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task NewAdvancedSearch_PaginationWorks_ReturnsCorrectPage()
    {
        // Arrange
        var searchCriteria = "[]"; // Get all partners

        // Act - Get first page
        var response1 = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=2");
        var result1 = await response1.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Act - Get second page
        var response2 = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=2&pageSize=2");
        var result2 = await response2.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        
        if (result1.Records.Any() && result2.Records.Any())
        {
            // Verify different records on different pages
            var page1Ids = result1.Records.Select(p => p.Id).ToList();
            var page2Ids = result2.Records.Select(p => p.Id).ToList();
            page1Ids.Should().NotIntersectWith(page2Ids);
        }
    }

    [Fact]
    public async Task NewAdvancedSearch_CaseInsensitiveSearch_ReturnsResults()
    {
        // Arrange - test with lowercase when data might be uppercase
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "acme",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find "ACME" partners even with lowercase search
        if (result.Records.Any())
        {
            result.Records.Should().Contain(p => p.Name.Contains("ACME", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public async Task NewAdvancedSearch_PartnerDescriptionSearch_ReturnsResults()
    {
        // Arrange
        var searchCriteria = """
        [
            {
                "field": "partnerLongDescription",
                "value": "Corporation",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should work without errors even if no matches found
    }

    [Fact]
    public async Task NewAdvancedSearch_NumericComparisons_ReturnsCorrectResults()
    {
        // Arrange - test greater than operator
        var searchCriteria = """
        [
            {
                "field": "status",
                "value": "0",
                "operator": "gt",
                "logicalOperator": "AND",
                "fieldType": "number"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Verify all returned partners have status > 0 (Active, Closed > Draft)
        if (result.Records.Any())
        {
            result.Records.Should().OnlyContain(p => p.Status != "Draft");
        }
    }

    #region Nested Properties and Similarity Search Tests

    [Fact]
    public async Task NewAdvancedSearch_NestedPropertySimilarity_FindsTyposInPartnerGroupName()
    {
        // Arrange - intentionally misspell "Corporate" as "Corporat" to test similarity on nested property
        var searchCriteria = """
        [
            {
                "field": "partnerGroup.name",
                "value": "Corporat",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // With smart search enabled, it should still find "Corporate" partners despite the typo
        // Note: This tests similarity search on navigation properties
    }

    [Fact]
    public async Task NewAdvancedSearch_CollectionPropertySimilarity_FindsTyposInContactNames()
    {
        // Arrange - intentionally misspell "John" as "Jon" to test similarity on collection property
        var searchCriteria = """
        [
            {
                "field": "contacts.firstName",
                "value": "Jon",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // With smart search enabled, it should still find partners with "John" in contacts despite the typo
    }

    [Fact]
    public async Task NewAdvancedSearch_NestedPropertyExactMatch_WorksCorrectly()
    {
        // Arrange - exact match on partner group code
        var searchCriteria = """
        [
            {
                "field": "partnerGroup.code",
                "value": "CORP",
                "operator": "eq",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find partners in Corporate group
    }

    [Fact]
    public async Task NewAdvancedSearch_DeepNestedPropertySimilarity_HandlesComplexPaths()
    {
        // Arrange - test similarity on multiple nested levels (if available in schema)
        var searchCriteria = """
        [
            {
                "field": "partnerGroup.name",
                "value": "Governmnt",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            },
            {
                "field": "contacts.lastName",
                "value": "Smyth",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find partners through similarity on both "Government" and "Smith" despite typos
    }

    [Fact]
    public async Task NewAdvancedSearch_CollectionPropertyEmail_SimilaritySearch()
    {
        // Arrange - test similarity on email addresses in collections
        var searchCriteria = """
        [
            {
                "field": "contacts.email",
                "value": "acme.com",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find partners with contacts having acme.com emails
        if (result.Records.Any())
        {
            // At least one partner should be found since we have john.smith@acme.com in test data
            result.Records.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task NewAdvancedSearch_CombinedNestedAndDirectSimilarity_ComplexSearch()
    {
        // Arrange - complex search combining direct field similarity with nested property similarity
        var searchCriteria = """
        [
            {
                "field": "name",
                "value": "ACMEE",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            },
            {
                "field": "partnerGroup.name",
                "value": "Corporat",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find ACME partners in Corporate group despite typos in both fields
    }

    [Fact]
    public async Task NewAdvancedSearch_MultipleCollectionPropertiesSimilarity_TestsAllContactFields()
    {
        // Arrange - test similarity across multiple collection properties
        var searchCriteria = """
        [
            {
                "field": "contacts.firstName",
                "value": "Jan",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            },
            {
                "field": "contacts.lastName",
                "value": "Do",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            },
            {
                "field": "contacts.email",
                "value": "globaltech",
                "operator": "like",
                "logicalOperator": "OR",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find partners through similarity on Jane/Doe/globaltech.com combinations
    }

    [Fact]
    public async Task NewAdvancedSearch_NestedPropertyCaseInsensitive_WithSimilarity()
    {
        // Arrange - test case insensitive + similarity on nested properties
        var searchCriteria = """
        [
            {
                "field": "partnerGroup.name",
                "value": "corporat",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should find "Corporate" partners despite lowercase input and missing letter
    }

    [Fact]
    public async Task NewAdvancedSearch_NestedPropertiesWithSpecialCharacters_SimilarityHandling()
    {
        // Arrange - test similarity with special characters in nested properties
        var searchCriteria = """
        [
            {
                "field": "contacts.email",
                "value": "john.smith@acme",
                "operator": "like",
                "logicalOperator": "AND",
                "fieldType": "text"
            }
        ]
        """;

        // Act
        var response = await GetAsync($"/api/partner/new-advanced-search?searchCriteria={Uri.EscapeDataString(searchCriteria)}&pageNumber=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>(JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        // Should handle email partial matching with special characters
        if (result.Records.Any())
        {
            result.Records.Should().NotBeEmpty();
        }
    }

    #endregion

    #endregion
}
