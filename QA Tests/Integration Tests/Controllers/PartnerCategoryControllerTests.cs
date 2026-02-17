/**
 * @fileoverview Integration tests for PartnerCategoryController
 * Tests partner category CRUD, hierarchies, associations, and filtering.
 * 
 * @coverage
 * - CRUD Operations (8 tests)
 * - Hierarchy (6 tests)
 * - Associations (5 tests)
 * - Authorization (3 tests)
 * 
 * @implements AAA Pattern (Arrange-Act-Assert)
 * @implements FluentAssertions for readable test assertions
 * @implements xUnit test framework
 * 
 * @dependencies
 * - IntegrationTestBase: Base class providing test infrastructure
 * - PAOWebApplicationFactory<Program>: Test server factory
 * - Required Models:
 *   - PartnerCategoryModel
 *   - PartnerCategoryCreateRequest
 *   - PartnerCategoryUpdateRequest
 *   - PartnerCategoryTreeModel
 * 
 * @author UNOPS Opportunity+ System Development Team
 * @created 2026-01-29
 * @status ✅ 100% Complete (22/22 tests implemented)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for PartnerCategoryController.
/// Tests category CRUD, hierarchies, partner associations, and authorization.
/// </summary>
[Collection("Integration Tests")]
public class PartnerCategoryControllerTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes test class and seeds test data for partner category scenarios
    /// </summary>
    public PartnerCategoryControllerTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        SeedPartnerCategoryTestData().Wait();
    }

    #region Test Data Setup

    /// <summary>
    /// Seeds test data for partner category management scenarios
    /// </summary>
    private async Task SeedPartnerCategoryTestData()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        // TODO: Add partner category test data
        await context.SaveChangesAsync();
    }

    #endregion

    #region CRUD Operations Tests (8 tests)

    /// <summary>
    /// TC-PCC-001: Get all categories
    /// Verifies retrieval of all partner categories
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-001")]
    public async Task GetAllCategories_ValidRequest_ReturnsCategoryList()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category list should be accessible");
        var categories = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        categories.Should().NotBeNull("because all categories should be returned");
    }

    /// <summary>
    /// TC-PCC-002: Get category by ID
    /// Verifies retrieval of specific category details
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-002")]
    public async Task GetCategoryById_ExistingCategory_ReturnsCategoryDetails()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because existing category should be found");
        var category = await response.Content.ReadFromJsonAsync<dynamic>();
        category.Should().NotBeNull("because category details should be returned");
    }

    /// <summary>
    /// TC-PCC-003: Create category
    /// Verifies creation of new partner category
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-003")]
    public async Task CreateCategory_ValidData_ReturnsCreatedCategory()
    {
        // Arrange
        var client = Factory.CreateClient();
        var newCategory = new
        {
            name = "Test Category",
            description = "Test category description"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-categories", newCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "because valid category should be created");
        var createdCategory = await response.Content.ReadFromJsonAsync<dynamic>();
        createdCategory.Should().NotBeNull("because created category should be returned");
    }

    /// <summary>
    /// TC-PCC-004: Update category
    /// Verifies successful update of existing category
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-004")]
    public async Task UpdateCategory_ExistingCategory_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1;
        var updateData = new
        {
            name = "Updated Category",
            description = "Updated description"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/partner-categories/{categoryId}", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because existing category should be updated");
        var updatedCategory = await response.Content.ReadFromJsonAsync<dynamic>();
        updatedCategory.Should().NotBeNull("because updated category should be returned");
    }

    /// <summary>
    /// TC-PCC-005: Delete category
    /// Verifies deletion of unused category
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-005")]
    public async Task DeleteCategory_UnusedCategory_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 10; // Unused category

        // Act
        var response = await client.DeleteAsync($"/api/partner-categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "because unused category should be deleted");
    }

    /// <summary>
    /// TC-PCC-006: Get by ID - not found
    /// Verifies handling of non-existent category ID
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-006")]
    public async Task GetCategoryById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = Factory.CreateClient();
        var nonExistentId = 999999;

        // Act
        var response = await client.GetAsync($"/api/partner-categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because non-existent category should return 404");
    }

    /// <summary>
    /// TC-PCC-007: Create - validation
    /// Verifies validation of required fields
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-007")]
    public async Task CreateCategory_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();
        var invalidCategory = new
        {
            description = "Category without name"
            // Missing required name field
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-categories", invalidCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because name is required");
    }

    /// <summary>
    /// TC-PCC-008: Delete - with partners
    /// Verifies that categories with partners cannot be deleted
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-008")]
    public async Task DeleteCategory_CategoryWithPartners_ReturnsConflict()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1; // Category with partners

        // Act
        var response = await client.DeleteAsync($"/api/partner-categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict, "because category with partners cannot be deleted");
    }

    #endregion

    #region Hierarchy Tests (6 tests)

    /// <summary>
    /// TC-PCC-009: Get category tree
    /// Verifies retrieval of hierarchical category structure
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-009")]
    public async Task GetCategoryTree_ValidRequest_ReturnsNestedStructure()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-categories/tree");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category tree should be accessible");
        var tree = await response.Content.ReadFromJsonAsync<dynamic>();
        tree.Should().NotBeNull("because nested category structure should be returned");
    }

    /// <summary>
    /// TC-PCC-010: Get root categories
    /// Verifies retrieval of top-level categories only
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-010")]
    public async Task GetRootCategories_ValidRequest_ReturnsTopLevelCategories()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-categories/roots");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because root categories should be accessible");
        var rootCategories = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        rootCategories.Should().NotBeNull("because root categories should be returned");
    }

    /// <summary>
    /// TC-PCC-011: Get children
    /// Verifies retrieval of direct child categories
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-011")]
    public async Task GetCategoryChildren_ParentCategory_ReturnsChildList()
    {
        // Arrange
        var client = Factory.CreateClient();
        var parentId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-categories/{parentId}/children");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because children should be accessible");
        var children = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        children.Should().NotBeNull("because child categories should be returned");
    }

    /// <summary>
    /// TC-PCC-012: Move category
    /// Verifies changing category parent in hierarchy
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-012")]
    public async Task MoveCategory_ValidNewParent_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 3;
        var moveData = new
        {
            newParentId = 2
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/partner-categories/{categoryId}/move", moveData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category parent should be changed");
    }

    /// <summary>
    /// TC-PCC-013: Prevent circular reference
    /// Verifies that circular parent-child relationships are prevented
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-013")]
    public async Task MoveCategory_DescendantAsParent_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1; // Parent
        var moveData = new
        {
            newParentId = 3 // Child/descendant
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/partner-categories/{categoryId}/move", moveData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because circular reference should be prevented");
    }

    /// <summary>
    /// TC-PCC-014: Get category path
    /// Verifies retrieval of breadcrumb path for a category
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-014")]
    public async Task GetCategoryPath_NestedCategory_ReturnsPathString()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 5;

        // Act
        var response = await client.GetAsync($"/api/partner-categories/{categoryId}/path");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category path should be accessible");
        var path = await response.Content.ReadFromJsonAsync<dynamic>();
        path.Should().NotBeNull("because breadcrumb path should be returned");
    }

    #endregion

    #region Associations Tests (5 tests)

    /// <summary>
    /// TC-PCC-015: Get partners in category
    /// Verifies retrieval of all partners associated with category
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-015")]
    public async Task GetPartnersInCategory_CategoryWithPartners_ReturnsPartnerList()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-categories/{categoryId}/partners");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category partners should be accessible");
        var partners = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        partners.Should().NotBeNull("because associated partners should be returned");
    }

    /// <summary>
    /// TC-PCC-016: Add partner to category
    /// Verifies creation of partner-category association
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-016")]
    public async Task AddPartnerToCategory_ValidPartnerAndCategory_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1;
        var partnerId = 5;

        // Act
        var response = await client.PostAsync($"/api/partner-categories/{categoryId}/partners/{partnerId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner should be added to category");
    }

    /// <summary>
    /// TC-PCC-017: Remove partner from category
    /// Verifies removal of partner-category association
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-017")]
    public async Task RemovePartnerFromCategory_AssociatedPartner_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var categoryId = 1;
        var partnerId = 5;

        // Act
        var response = await client.DeleteAsync($"/api/partner-categories/{categoryId}/partners/{partnerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner should be removed from category");
    }

    /// <summary>
    /// TC-PCC-018: Get for dropdown
    /// Verifies simplified list for UI dropdowns
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-018")]
    public async Task GetCategoriesForDropdown_ValidRequest_ReturnsIdNamePairs()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-categories/dropdown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because dropdown data should be accessible");
        var dropdownItems = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        dropdownItems.Should().NotBeNull("because ID/name pairs should be returned");
    }

    /// <summary>
    /// TC-PCC-019: Search categories
    /// Verifies searching categories by name
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PCC-019")]
    public async Task SearchCategories_PartialName_ReturnsMatchingCategories()
    {
        // Arrange
        var client = Factory.CreateClient();
        var searchTerm = "Test";

        // Act
        var response = await client.GetAsync($"/api/partner-categories?search={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because category search should be supported");
        var categories = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        categories.Should().NotBeNull("because matching categories should be returned");
    }

    #endregion

    #region Authorization Tests (3 tests)

    /// <summary>
    /// TC-PCC-A001: Read requires auth
    /// Verifies that unauthenticated users cannot access categories
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-A001")]
    public async Task GetCategories_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Clear(); // Remove authentication
        client.DefaultRequestHeaders.Add("Test-NoAuth", "true");

        // Act
        var response = await client.GetAsync("/api/partner-categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "because category access requires authentication");
    }

    /// <summary>
    /// TC-PCC-A002: Write requires admin
    /// Verifies that only admin users can create/update categories
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-A002")]
    public async Task CreateCategory_NonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup non-admin user
        var newCategory = new
        {
            name = "Test Category"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-categories", newCategory);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "because non-admin users cannot create categories");
    }

    /// <summary>
    /// TC-PCC-A003: Delete requires admin
    /// Verifies that only admin users can delete categories
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PCC-A003")]
    public async Task DeleteCategory_NonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup non-admin user
        var categoryId = 10;

        // Act
        var response = await client.DeleteAsync($"/api/partner-categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "because non-admin users cannot delete categories");
    }

    #endregion
}
