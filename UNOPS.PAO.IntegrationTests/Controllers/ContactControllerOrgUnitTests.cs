using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers
{
    public class ContactControllerOrgUnitTests : IntegrationTestBase
    {
        private const string BaseUrl = "/api/contact";

        public ContactControllerOrgUnitTests(PAOWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        private static UNOPSPartner CreateTestPartner(string name, int organizationHierarchyId, int createdBy = 1)
        {
            var partner = new UNOPSPartner
            {
                // Enhanced Partner structure
                Name = name,
                PartnerShortDescription = name.Length > 10 ? name.Substring(0, 10) : name,
                PartnerCategoryId = 1, // Default test category
                LiaisonOfficeId = 1, // Default test liaison office
                UNAndStateEntity = false,
                Status = Domain.Entities.EntityStatus.Active,
                CanCreateNewOpportunities = false,
                PooledFund = false,
                DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
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

        private static UNOPSContact CreateTestContact(string firstName, string lastName, int partnerId, int createdBy = 1)
        {
            return new UNOPSContact
            {
                FirstName = firstName,
                LastName = lastName,
                Name = $"{firstName} {lastName}",
                Title = "Manager",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                Status = EntityStatus.Active,
                PartnerId = partnerId,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                ContactNumber = $"CON-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_ReturnsContactsFromOrgUnitAndDescendants()
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
                CreateTestPartner("Partner in Root", 100),
                CreateTestPartner("Partner in Child 1", 101),
                CreateTestPartner("Partner in Child 2", 102),
                CreateTestPartner("Partner in Grandchild", 103),
                CreateTestPartner("Partner in Different Org", 200)
            };

            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            // Create contacts for each partner
            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("John", "Root", partners[0].Id),
                CreateTestContact("Jane", "Child1", partners[1].Id),
                CreateTestContact("Bob", "Child2", partners[2].Id),
                CreateTestContact("Alice", "Grandchild", partners[3].Id),
                CreateTestContact("Eve", "Different", partners[4].Id)
            };

            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Filter by root org unit (should return all contacts in hierarchy)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=100&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(4); // All contacts in the hierarchy
            result!.Records.Should().HaveCount(4);
            result!.Records.Select(r => r.FirstName).Should().BeEquivalentTo(new[]
            {
                "John", "Jane", "Bob", "Alice"
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_MiddleLevel_ReturnsContactsFromSubtree()
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
                CreateTestPartner("Partner at Root", 200),
                CreateTestPartner("Partner at Middle", 201),
                CreateTestPartner("Partner at Leaf 1", 202),
                CreateTestPartner("Partner at Leaf 2", 203)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("Root", "Contact", partners[0].Id),
                CreateTestContact("Middle", "Contact", partners[1].Id),
                CreateTestContact("Leaf1", "Contact", partners[2].Id),
                CreateTestContact("Leaf2", "Contact", partners[3].Id)
            };
            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Filter by middle org unit (should return middle and its descendants)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=201&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(3); // Middle and its two children
            result!.Records.Should().HaveCount(3);
            result!.Records.Select(r => r.FirstName).Should().BeEquivalentTo(new[]
            {
                "Middle", "Leaf1", "Leaf2"
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdFilter_LeafNode_ReturnsOnlyLeafContacts()
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
                CreateTestPartner("Partner at Parent", 300),
                CreateTestPartner("Partner at Leaf", 301)
            };
            await dbContext.Partners.AddRangeAsync(partners);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("Parent", "Contact", partners[0].Id),
                CreateTestContact("Leaf", "Contact", partners[1].Id)
            };
            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Filter by leaf org unit (should return only leaf contacts)
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=301&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(1); // Only the leaf contact
            result!.Records.Should().HaveCount(1);
            result!.Records.First().FirstName.Should().Be("Leaf");
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndStatusFilter_AppliesBothFilters()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 400, Code = "ORG400", Name = "Org 400", Description = "Org 400 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partner = CreateTestPartner("Test Partner", 400);
            await dbContext.Partners.AddAsync(partner);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("Active", "Contact1", partner.Id),
                CreateTestContact("Inactive", "Contact2", partner.Id),
                CreateTestContact("Active", "Contact3", partner.Id),
            };
            
            // Set status for contacts
            contacts[1].Status = EntityStatus.Inactive;
            
            await dbContext.Contacts.AddRangeAsync(contacts);
            
            // Add contact in different org
            var differentOrgPartner = CreateTestPartner("Different Org Partner", 500);
            await dbContext.Partners.AddAsync(differentOrgPartner);
            await dbContext.SaveChangesAsync();
            
            var differentOrgContact = CreateTestContact("Active", "DifferentOrg", differentOrgPartner.Id);
            await dbContext.Contacts.AddAsync(differentOrgContact);
            await dbContext.SaveChangesAsync();

            // Act - Filter by org unit and status
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=400&searchText=Active&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2); // Only active contacts in org 400
            result!.Records.Should().HaveCount(2);
            result!.Records.All(r => r.FirstName == "Active").Should().BeTrue();
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndNameFilter_AppliesBothFilters()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 500, Code = "ORG500", Name = "Org 500", Description = "Org 500 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partner = CreateTestPartner("Test Partner", 500);
            await dbContext.Partners.AddAsync(partner);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("John", "Smith", partner.Id),
                CreateTestContact("Jane", "Smith", partner.Id),
                CreateTestContact("John", "Doe", partner.Id),
            };
            await dbContext.Contacts.AddRangeAsync(contacts);
            
            // Add contact in different org
            var differentOrgPartner = CreateTestPartner("Different Org Partner", 600);
            await dbContext.Partners.AddAsync(differentOrgPartner);
            await dbContext.SaveChangesAsync();
            
            var differentOrgContact = CreateTestContact("John", "Different", differentOrgPartner.Id);
            await dbContext.Contacts.AddAsync(differentOrgContact);
            await dbContext.SaveChangesAsync();

            // Act - Filter by org unit and name containing "John"
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=500&searchText=John&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2); // Only "John" contacts in org 500
            result!.Records.Should().HaveCount(2);
            result!.Records.All(r => r.FirstName == "John").Should().BeTrue();
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndPagination_ReturnsCorrectPage()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 600, Code = "ORG600", Name = "Org 600", Description = "Org 600 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partner = CreateTestPartner("Test Partner", 600);
            await dbContext.Partners.AddAsync(partner);
            await dbContext.SaveChangesAsync();

            // Create 15 contacts in the same org unit
            var contacts = new List<UNOPSContact>();
            for (int i = 1; i <= 15; i++)
            {
                contacts.Add(CreateTestContact($"Contact{i:D2}", "Test", partner.Id));
            }
            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Get second page with org unit filter
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=600&pageIndex=2&pageSize=5&orderBy=firstName&ascending=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(15);
            result!.Records.Should().HaveCount(5);
            result.PageIndex.Should().Be(2);
            result.PageSize.Should().Be(5);
            result!.Records.Select(r => r.FirstName).Should().BeEquivalentTo(new[] 
            { 
                "Contact06", "Contact07", "Contact08", "Contact09", "Contact10" 
            });
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdAndAdvancedSearch_FiltersCorrectly()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            var orgUnit = new OrganizationHierarchy { Id = 700, Code = "ORG700", Name = "Org 700", Description = "Org 700 Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);

            var partner = CreateTestPartner("Test Partner", 700);
            await dbContext.Partners.AddAsync(partner);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                new UNOPSContact { FirstName = "John", LastName = "Manager", Name = "John Manager", Title = "Senior Manager", Department = "Sales", Email = "john.manager@example.com", Status = EntityStatus.Active, PartnerId = partner.Id, CreatedBy = 1, CreatedDate = DateTime.UtcNow, ContactNumber = "CON-001" },
                new UNOPSContact { FirstName = "Jane", LastName = "Developer", Name = "Jane Developer", Title = "Software Developer", Department = "IT", Email = "jane.developer@example.com", Status = EntityStatus.Active, PartnerId = partner.Id, CreatedBy = 1, CreatedDate = DateTime.UtcNow, ContactNumber = "CON-002" },
                new UNOPSContact { FirstName = "Bob", LastName = "Manager", Name = "Bob Manager", Title = "Junior Manager", Department = "Sales", Email = "bob.manager@example.com", Status = EntityStatus.Active, PartnerId = partner.Id, CreatedBy = 1, CreatedDate = DateTime.UtcNow, ContactNumber = "CON-003" }
            };
            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Advanced search for managers in org unit 700
            var searchCriteria = new ContactFilterRequest 
            { 
                OrgUnitId = 700,
                Title = "Manager"
            };
            var response = await Client.GetAsync($"{BaseUrl}?advancedSearch=true&searchCriteria={Uri.EscapeDataString(System.Text.Json.JsonSerializer.Serialize(searchCriteria))}&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(2); // Only managers in org 700
            result!.Records.Should().HaveCount(2);
            result!.Records.All(r => r.Title!.Contains("Manager")).Should().BeTrue();
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithNonExistentOrgUnitId_ReturnsEmptyResult()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create some contacts but no org unit with ID 999
            var partner = CreateTestPartner("Test Partner", 100);
            await dbContext.Partners.AddAsync(partner);
            await dbContext.SaveChangesAsync();

            var contacts = new List<UNOPSContact>
            {
                CreateTestContact("Contact", "One", partner.Id),
                CreateTestContact("Contact", "Two", partner.Id)
            };
            await dbContext.Contacts.AddRangeAsync(contacts);
            await dbContext.SaveChangesAsync();

            // Act - Filter by non-existent org unit
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=999&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(0);
            result!.Records.Should().BeEmpty();
        }

        [Fact(Skip = "Skipping due to authorization issues in test environment")]
        public async Task GetAll_WithOrgUnitIdButNoContacts_ReturnsEmptyResult()
        {
            // Arrange
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
            
            // Create org unit but no partners/contacts
            var orgUnit = new OrganizationHierarchy { Id = 800, Code = "EMPTY", Name = "Empty Org", Description = "Empty Org Description", Type = OrganizationUnitType.Office };
            await dbContext.OrganizationHierarchies.AddAsync(orgUnit);
            await dbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"{BaseUrl}?orgUnitId=800&pageIndex=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<ContactModel>>();
            
            result.Should().NotBeNull();
            result!.TotalCount.Should().Be(0);
            result!.Records.Should().BeEmpty();
        }
    }
}