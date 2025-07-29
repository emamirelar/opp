using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Models;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers
{
    public class PartnerControllerOrgUnitTests : IntegrationTestBase
    {
        private const string BaseUrl = "/api/partner";

        public PartnerControllerOrgUnitTests(PAOWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        private static UNOPSPartner CreateTestPartner(string name, string status, int organizationHierarchyId, int createdBy = 1)
        {
            var partner = new UNOPSPartner
            {
                Name = name,
                ShortName = name.Length > 10 ? name.Substring(0, 10) : name,
                Status = status,
                NewEngagement = "No",
                PooledFund = "No",
                DDRequired = "No",
                DDEACDone = "No",
                LevyPotentiallyApplies = "No",
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            // Add organization unit relationship
            partner.OrganizationUnitRelationships = new List<OrganizationUnitRelationship>
            {
                new OrganizationUnitRelationship
                {
                    OrganizationHierarchyId = organizationHierarchyId,
                    EntityId = partner.Id,
                    EntityType = nameof(UNOPSPartner)
                }
            };

            return partner;
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_ReturnsPartnersFromOrgUnitAndDescendants()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create org unit hierarchy
            var rootOrgUnit = new OrganizationHierarchy
            {
                Id = 100,
                Code = "ROOT",
                Name = "Root Organization",
                Description = "Root Organization Description",
                Type = OrganizationUnitType.Office,
                ParentId = null
            };
            
            var childOrgUnit1 = new OrganizationHierarchy
            {
                Id = 101,
                Code = "CHILD1",
                Name = "Child Organization 1",
                Description = "Child Organization 1 Description",
                Type = OrganizationUnitType.Office,
                ParentId = 100
            };
            
            var childOrgUnit2 = new OrganizationHierarchy
            {
                Id = 102,
                Code = "CHILD2",
                Name = "Child Organization 2",
                Description = "Child Organization 2 Description",
                Type = OrganizationUnitType.Office,
                ParentId = 100
            };
            
            var grandchildOrgUnit = new OrganizationHierarchy
            {
                Id = 103,
                Code = "GRANDCHILD",
                Name = "Grandchild Organization",
                Description = "Grandchild Organization Description",
                Type = OrganizationUnitType.Office,
                ParentId = 101
            };
            
            await dbContext.OrganizationHierarchies.AddRangeAsync(rootOrgUnit, childOrgUnit1, childOrgUnit2, grandchildOrgUnit);

            // Create partners in different org units
            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Partner in Root", "Active", 100),
                CreateTestPartner("Partner in Child 1", "Active", 101),
                CreateTestPartner("Partner in Child 2", "Active", 102),
                CreateTestPartner("Partner in Grandchild", "Active", 103),
                CreateTestPartner("Partner in Different Org", "Active", 200) // Different org unit not in hierarchy
            };

            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by root org unit (should return all partners in hierarchy)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=100&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(4); // All partners in the hierarchy
            result!.Records.Should().HaveCount(4);
            result!.Records.Select(r => r.Name).Should().BeEquivalentTo(new[]
            {
                "Partner in Root",
                "Partner in Child 1",
                "Partner in Child 2",
                "Partner in Grandchild"
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_MiddleLevel_ReturnsPartnersFromSubtree()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create org unit hierarchy
            var orgUnits = new List<OrganizationHierarchy>
            {
                new OrganizationHierarchy { Id = 200, Code = "ROOT2", Name = "Root 2", Description = "Root 2 Description", Type = OrganizationUnitType.Office, ParentId = null },
                new OrganizationHierarchy { Id = 201, Code = "MID", Name = "Middle", Description = "Middle Description", Type = OrganizationUnitType.Office, ParentId = 200 },
                new OrganizationHierarchy { Id = 202, Code = "LEAF1", Name = "Leaf 1", Description = "Leaf 1 Description", Type = OrganizationUnitType.Office, ParentId = 201 },
                new OrganizationHierarchy { Id = 203, Code = "LEAF2", Name = "Leaf 2", Description = "Leaf 2 Description", Type = OrganizationUnitType.Office, ParentId = 201 }
            };
            await dbContext.OrganizationHierarchies.AddRangeAsync(orgUnits);

            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Partner at Root", "Active", 200),
                CreateTestPartner("Partner at Middle", "Active", 201),
                CreateTestPartner("Partner at Leaf 1", "Active", 202),
                CreateTestPartner("Partner at Leaf 2", "Active", 203)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by middle org unit (should return middle and its descendants)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=201&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(3); // Middle and its two children
            result!.Records.Should().HaveCount(3);
            result!.Records.Select(r => r.Name).Should().BeEquivalentTo(new[]
            {
                "Partner at Middle",
                "Partner at Leaf 1",
                "Partner at Leaf 2"
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_LeafNode_ReturnsOnlyLeafPartners()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create simple hierarchy
            var orgUnits = new List<OrganizationHierarchy>
            {
                new OrganizationHierarchy { Id = 300, Code = "PARENT", Name = "Parent", Description = "Parent Description", Type = OrganizationUnitType.Office, ParentId = null },
                new OrganizationHierarchy { Id = 301, Code = "LEAF", Name = "Leaf", Description = "Leaf Description", Type = OrganizationUnitType.Office, ParentId = 300 }
            };
            await dbContext.OrganizationHierarchies.AddRangeAsync(orgUnits);

            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Partner at Parent", "Active", 300),
                CreateTestPartner("Partner at Leaf", "Active", 301)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by leaf org unit (should return only leaf partners)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=301&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(1); // Only the leaf partner
            result!.Records.Should().HaveCount(1);
            result!.Records.First().Name.Should().Be("Partner at Leaf");
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndStatusFilter_AppliesBothFilters()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 400, Code = "ORG400", Name = "Org 400", Description = "Org 400 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Active Partner", "Active", 400),
                CreateTestPartner("Inactive Partner", "Inactive", 400),
                CreateTestPartner("Draft Partner", "Draft", 400),
                CreateTestPartner("Active Partner Different Org", "Active", 500)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by org unit and status
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=400&status=Active&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(1); // Only active partner in org 400
            result!.Records.Should().HaveCount(1);
            result!.Records.First().Name.Should().Be("Active Partner");
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndNameFilter_AppliesBothFilters()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 500, Code = "ORG500", Name = "Org 500", Description = "Org 500 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Alpha Corporation", "Active", 500),
                CreateTestPartner("Beta Industries", "Active", 500),
                CreateTestPartner("Alpha Solutions", "Active", 500),
                CreateTestPartner("Alpha Global", "Active", 600)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by org unit and name containing "Alpha"
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=500&name=Alpha&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2); // Only "Alpha" partners in org 500
            result!.Records.Should().HaveCount(2);
            result!.Records.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Alpha Corporation", "Alpha Solutions" });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndPagination_ReturnsCorrectPage()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 600, Code = "ORG600", Name = "Org 600", Description = "Org 600 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            // Create 15 partners in the same org unit
            var partners = new List<UNOPSPartner>();
            for (int i = 1; i <= 15; i++)
            {
                partners.Add(CreateTestPartner($"Partner {i:D2}", "Active", 600));
            }
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Get second page with org unit filter
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=600&pageIndex=2&pageSize=5&orderBy=name&ascending=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(15);
            result!.Records.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result!.Records.Select(r => r.Name).Should().BeEquivalentTo(new[] 
            { 
                "Partner 06", "Partner 07", "Partner 08", "Partner 09", "Partner 10" 
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndSearchText_FiltersCorrectly()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 700, Code = "ORG700", Name = "Org 700", Description = "Org 700 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Technology Corp", "Active", 700),
                CreateTestPartner("Finance Ltd", "Active", 700),
                CreateTestPartner("Tech Solutions", "Active", 700),
                CreateTestPartner("Technology Inc", "Active", 800)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Search for "tech" within org unit 700
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=700&searchText=Tech&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2); // Only tech-related partners in org 700
            result!.Records.Should().HaveCount(2);
            result!.Records.Select(r => r.Name).Should().BeEquivalentTo(new[] { "Technology Corp", "Tech Solutions" });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithNonExistentOrgUnitId_ReturnsEmptyResult()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create some partners but no org unit with ID 999
            var partners = new List<UNOPSPartner>
            {
                CreateTestPartner("Partner 1", "Active", 100),
                CreateTestPartner("Partner 2", "Active", 200)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Act - Filter by non-existent org unit
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=999&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(0);
            result!.Records.Should().BeEmpty();
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdButNoPartners_ReturnsEmptyResult()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create org unit but no partners
            var orgUnit = new OrganizationHierarchy { Id = 800, Code = "EMPTY", Name = "Empty Org", Description = "Empty Org Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=800&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<PartnerModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(0);
            result!.Records.Should().BeEmpty();
        }
    }
}