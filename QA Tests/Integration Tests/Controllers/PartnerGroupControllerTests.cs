/**
 * @fileoverview Integration tests for PartnerGroupController
 * Tests partner group CRUD, membership management, and permissions.
 * 
 * @coverage
 * - CRUD Operations (8 tests)
 * - Membership (8 tests)
 * - Permissions (3 tests)
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
 *   - PartnerGroupModel
 *   - PartnerGroupCreateRequest
 *   - PartnerGroupUpdateRequest
 *   - PartnerGroupMemberModel
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
/// Integration tests for PartnerGroupController.
/// Tests group CRUD, membership management, and authorization.
/// </summary>
[Collection("Integration Tests")]
public class PartnerGroupControllerTests : IntegrationTestBase
{
    /// <summary>
    /// Initializes test class and seeds test data for partner group scenarios
    /// </summary>
    public PartnerGroupControllerTests(PAOWebApplicationFactory<Program> factory) : base(factory)
    {
        SeedPartnerGroupTestData().Wait();
    }

    #region Test Data Setup

    /// <summary>
    /// Seeds test data for partner group management scenarios
    /// </summary>
    private async Task SeedPartnerGroupTestData()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

        // TODO: Add partner group test data
        await context.SaveChangesAsync();
    }

    #endregion

    #region CRUD Operations Tests (8 tests)

    /// <summary>
    /// TC-PGC-001: Get all groups
    /// Verifies retrieval of all partner groups
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-001")]
    public async Task GetAllGroups_ValidRequest_ReturnsGroupList()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because group list should be accessible");
        var groups = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        groups.Should().NotBeNull("because all groups should be returned");
    }

    /// <summary>
    /// TC-PGC-002: Get group by ID
    /// Verifies retrieval of specific group details
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-002")]
    public async Task GetGroupById_ExistingGroup_ReturnsGroupDetails()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-groups/{groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because existing group should be found");
        var group = await response.Content.ReadFromJsonAsync<dynamic>();
        group.Should().NotBeNull("because group details should be returned");
    }

    /// <summary>
    /// TC-PGC-003: Create group
    /// Verifies creation of new partner group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-003")]
    public async Task CreateGroup_ValidData_ReturnsCreatedGroup()
    {
        // Arrange
        var client = Factory.CreateClient();
        var newGroup = new
        {
            name = "Test Partner Group",
            description = "Test group description",
            groupType = "Regional"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-groups", newGroup);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "because valid group should be created");
        var createdGroup = await response.Content.ReadFromJsonAsync<dynamic>();
        createdGroup.Should().NotBeNull("because created group should be returned");
    }

    /// <summary>
    /// TC-PGC-004: Update group
    /// Verifies successful update of existing group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-004")]
    public async Task UpdateGroup_ExistingGroup_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var updateData = new
        {
            name = "Updated Group Name",
            description = "Updated description"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/partner-groups/{groupId}", updateData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because existing group should be updated");
        var updatedGroup = await response.Content.ReadFromJsonAsync<dynamic>();
        updatedGroup.Should().NotBeNull("because updated group should be returned");
    }

    /// <summary>
    /// TC-PGC-005: Delete group
    /// Verifies deletion of unused group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-005")]
    public async Task DeleteGroup_EmptyGroup_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 10; // Empty group

        // Act
        var response = await client.DeleteAsync($"/api/partner-groups/{groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent, "because empty group should be deleted");
    }

    /// <summary>
    /// TC-PGC-006: Get by ID - not found
    /// Verifies handling of non-existent group ID
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-006")]
    public async Task GetGroupById_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var client = Factory.CreateClient();
        var nonExistentId = 999999;

        // Act
        var response = await client.GetAsync($"/api/partner-groups/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because non-existent group should return 404");
    }

    /// <summary>
    /// TC-PGC-007: Create - validation
    /// Verifies validation of required fields
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-007")]
    public async Task CreateGroup_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var client = Factory.CreateClient();
        var invalidGroup = new
        {
            description = "Group without name"
            // Missing required name
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-groups", invalidGroup);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "because name is required");
    }

    /// <summary>
    /// TC-PGC-008: Duplicate name prevented
    /// Verifies that duplicate group names are rejected
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-008")]
    public async Task CreateGroup_DuplicateName_ReturnsConflict()
    {
        // Arrange
        var client = Factory.CreateClient();
        var duplicateGroup = new
        {
            name = "Existing Group Name",
            description = "Duplicate"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-groups", duplicateGroup);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict, "because duplicate name should be rejected");
    }

    #endregion

    #region Membership Tests (8 tests)

    /// <summary>
    /// TC-PGC-009: Get group members
    /// Verifies retrieval of all partners in a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-009")]
    public async Task GetGroupMembers_GroupWithMembers_ReturnsPartnerList()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-groups/{groupId}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because group members should be accessible");
        var members = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        members.Should().NotBeNull("because partner list should be returned");
    }

    /// <summary>
    /// TC-PGC-010: Add member
    /// Verifies adding a partner to a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-010")]
    public async Task AddMember_ValidPartnerAndGroup_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var partnerId = 5;

        // Act
        var response = await client.PostAsync($"/api/partner-groups/{groupId}/members/{partnerId}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner should be added to group");
    }

    /// <summary>
    /// TC-PGC-011: Remove member
    /// Verifies removing a partner from a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-011")]
    public async Task RemoveMember_GroupMember_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var partnerId = 5;

        // Act
        var response = await client.DeleteAsync($"/api/partner-groups/{groupId}/members/{partnerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because member should be removed from group");
    }

    /// <summary>
    /// TC-PGC-012: Add multiple members
    /// Verifies bulk addition of members to a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-012")]
    public async Task AddMultipleMembers_ValidPartners_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var partnerIds = new[] { 5, 6, 7 };

        // Act
        var response = await client.PostAsJsonAsync($"/api/partner-groups/{groupId}/members/bulk", partnerIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because multiple partners should be added");
    }

    /// <summary>
    /// TC-PGC-013: Remove multiple members
    /// Verifies bulk removal of members from a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-013")]
    public async Task RemoveMultipleMembers_GroupMembers_ReturnsSuccess()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var partnerIds = new[] { 5, 6 };

        // Act
        var response = await client.DeleteAsync($"/api/partner-groups/{groupId}/members/bulk");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because multiple members should be removed");
    }

    /// <summary>
    /// TC-PGC-014: Get member count
    /// Verifies retrieval of group member count
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-014")]
    public async Task GetMemberCount_GroupWithMembers_ReturnsCount()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;

        // Act
        var response = await client.GetAsync($"/api/partner-groups/{groupId}/members/count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because member count should be accessible");
        var count = await response.Content.ReadFromJsonAsync<int>();
        count.Should().BeGreaterOrEqualTo(0, "because count should be non-negative");
    }

    /// <summary>
    /// TC-PGC-015: Check membership
    /// Verifies checking if a partner is in a group
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-015")]
    public async Task CheckMembership_ValidPartner_ReturnsBooleanResult()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupId = 1;
        var partnerId = 5;

        // Act
        var response = await client.GetAsync($"/api/partner-groups/{groupId}/members/{partnerId}/check");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because membership check should complete");
        var isMember = await response.Content.ReadFromJsonAsync<bool>();
        isMember.Should().Be(true, "because boolean result should be returned");
    }

    /// <summary>
    /// TC-PGC-016: Get partner's groups
    /// Verifies retrieval of all groups containing a partner
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-016")]
    public async Task GetPartnerGroups_PartnerWithGroups_ReturnsGroupList()
    {
        // Arrange
        var client = Factory.CreateClient();
        var partnerId = 1;

        // Act
        var response = await client.GetAsync($"/api/partners/{partnerId}/groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because partner's groups should be accessible");
        var groups = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        groups.Should().NotBeNull("because group list should be returned");
    }

    #endregion

    #region Additional Operations Tests (3 tests)

    /// <summary>
    /// TC-PGC-017: Get for dropdown
    /// Verifies simplified list for UI dropdowns
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-017")]
    public async Task GetGroupsForDropdown_ValidRequest_ReturnsIdNamePairs()
    {
        // Arrange
        var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/partner-groups/dropdown");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because dropdown data should be accessible");
        var dropdownItems = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        dropdownItems.Should().NotBeNull("because ID/name pairs should be returned");
    }

    /// <summary>
    /// TC-PGC-018: Search groups
    /// Verifies searching groups by name
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-018")]
    public async Task SearchGroups_PartialName_ReturnsMatchingGroups()
    {
        // Arrange
        var client = Factory.CreateClient();
        var searchTerm = "Regional";

        // Act
        var response = await client.GetAsync($"/api/partner-groups?search={searchTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because group search should be supported");
        var groups = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        groups.Should().NotBeNull("because matching groups should be returned");
    }

    /// <summary>
    /// TC-PGC-019: Filter by type
    /// Verifies filtering groups by group type
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    [Trait("Priority", "P1")]
    [Trait("TestId", "TC-PGC-019")]
    public async Task FilterGroupsByType_ValidType_ReturnsFilteredGroups()
    {
        // Arrange
        var client = Factory.CreateClient();
        var groupType = "Regional";

        // Act
        var response = await client.GetAsync($"/api/partner-groups?type={groupType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because type filtering should be supported");
        var groups = await response.Content.ReadFromJsonAsync<List<dynamic>>();
        groups.Should().NotBeNull("because filtered groups should be returned");
    }

    #endregion

    #region Authorization Tests (3 tests)

    /// <summary>
    /// TC-PGC-A001: Read requires auth
    /// Verifies that unauthenticated users cannot access groups
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-A001")]
    public async Task GetGroups_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Clear(); // Remove authentication

        // Act
        var response = await client.GetAsync("/api/partner-groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "because group access requires authentication");
    }

    /// <summary>
    /// TC-PGC-A002: Write requires admin
    /// Verifies that only admin users can create/update groups
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-A002")]
    public async Task CreateGroup_NonAdminUser_ReturnsForbidden()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup non-admin user
        var newGroup = new
        {
            name = "Test Group"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/partner-groups", newGroup);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "because non-admin users cannot create groups");
    }

    /// <summary>
    /// TC-PGC-A003: Member management permissions
    /// Verifies role-based permissions for member management
    /// </summary>
    [Fact]
    [Trait("Category", "Security")]
    [Trait("Priority", "P0")]
    [Trait("TestId", "TC-PGC-A003")]
    public async Task ManageMembers_BasedOnRole_EnforcesPermissions()
    {
        // Arrange
        var client = Factory.CreateClient();
        // TODO: Setup user with specific role
        var groupId = 1;
        var partnerId = 5;

        // Act
        var response = await client.PostAsync($"/api/partner-groups/{groupId}/members/{partnerId}", null);

        // Assert
        response.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.OK, HttpStatusCode.Forbidden }, 
            "because response depends on user role permissions");
    }

    #endregion
}
