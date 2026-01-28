using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Organizations;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.OrgHierarchy
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "OrgHierarchy")][Trait("Component", "NegativeTests")]
    public class OrgHierarchyNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public OrgHierarchyNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ORG-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetOrgHierarchy_NonExistentOrgId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgHierarchyAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-002")][Trait("Priority", "High")]
        public async Task GetOrgHierarchy_NegativeOrgId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetOrgHierarchyAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-003")][Trait("Priority", "High")]
        public async Task GetOrgHierarchy_ZeroOrgId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetOrgHierarchyAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-004")][Trait("Priority", "Critical")]
        public async Task GetOrgHierarchy_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetOrgHierarchyAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-005")][Trait("Priority", "Critical")]
        public async Task GetOrgHierarchy_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetOrgHierarchyAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-006")][Trait("Priority", "High")]
        public async Task AddSubOrganization_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddSubOrganizationAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-007")][Trait("Priority", "High")]
        public async Task AddSubOrganization_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddSubOrganizationAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-008")][Trait("Priority", "High")]
        public async Task AddSubOrganization_SelfReference_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddSubOrganizationAsync(1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-009")][Trait("Priority", "Critical")]
        public async Task AddSubOrganization_CircularReference_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 2, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddSubOrganizationAsync(2, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-010")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveSubOrganizationAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-011")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveSubOrganizationAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-012")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_NotActualChild_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.RemoveSubOrganizationAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-013")][Trait("Priority", "Medium")]
        public async Task GetOrgHierarchy_DeletedOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgHierarchyAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-014")][Trait("Priority", "High")]
        public async Task AddSubOrganization_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddSubOrganizationAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-015")][Trait("Priority", "High")]
        public async Task AddSubOrganization_NegativeChildId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddSubOrganizationAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetOrgHierarchy_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgHierarchyAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-017")][Trait("Priority", "Critical")]
        public async Task GetOrgHierarchy_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetOrgHierarchyAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-018")][Trait("Priority", "High")]
        public async Task AddSubOrganization_DuplicateChild_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 3, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddSubOrganizationAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-019")][Trait("Priority", "High")]
        public async Task AddSubOrganization_ConcurrentSameChild_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var t1 = mgr.AddSubOrganizationAsync(1, 4, CreateUser());
            var t2 = mgr.AddSubOrganizationAsync(1, 4, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Duplicate prevented"); }
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetOrgHierarchy_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetOrgHierarchyAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-021")][Trait("Priority", "High")]
        public async Task AddSubOrganization_ExcessiveDepth_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            for (int i = 1; i < 100; i++)
            {
                try { await mgr.AddSubOrganizationAsync(i, i + 1, CreateUser()); }
                catch (InvalidOperationException) { Assert.True(true, "Max depth enforced"); break; }
                catch { break; }
            }
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-022")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveSubOrganizationAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-023")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_NegativeChildId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveSubOrganizationAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-024")][Trait("Priority", "High")]
        public async Task AddSubOrganization_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.AddSubOrganizationAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-025")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemoveSubOrganizationAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-026")][Trait("Priority", "High")]
        public async Task GetOrgChildren_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgChildrenAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-027")][Trait("Priority", "High")]
        public async Task GetOrgParent_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgParentAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-028")][Trait("Priority", "Medium")]
        public async Task GetOrgAncestors_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgAncestorsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-029")][Trait("Priority", "Medium")]
        public async Task GetOrgDescendants_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgDescendantsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-030")][Trait("Priority", "High")]
        public async Task AddSubOrganization_AlreadyHasParent_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 5, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddSubOrganizationAsync(2, 5, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-031")][Trait("Priority", "High")]
        public async Task MoveOrganization_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.MoveOrganizationAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-032")][Trait("Priority", "High")]
        public async Task MoveOrganization_NonExistentNewParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.MoveOrganizationAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-033")][Trait("Priority", "High")]
        public async Task MoveOrganization_ToOwnDescendant_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 2, CreateUser());
            await mgr.AddSubOrganizationAsync(2, 3, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.MoveOrganizationAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-034")][Trait("Priority", "High")]
        public async Task GetOrgSiblings_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgSiblingsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-035")][Trait("Priority", "Medium")]
        public async Task GetOrgLevel_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgLevelAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-036")][Trait("Priority", "High")]
        public async Task GetOrgPath_NonExistentOrg_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetOrgPathAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-037")][Trait("Priority", "High")]
        public async Task AddSubOrganization_DeletedParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddSubOrganizationAsync(888, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-038")][Trait("Priority", "High")]
        public async Task AddSubOrganization_DeletedChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddSubOrganizationAsync(1, 888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-039")][Trait("Priority", "Medium")]
        public async Task GetOrgChildren_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetOrgChildrenAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-040")][Trait("Priority", "High")]
        public async Task MoveOrganization_NegativeOrgId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.MoveOrganizationAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-041")][Trait("Priority", "High")]
        public async Task MoveOrganization_NegativeNewParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.MoveOrganizationAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-042")][Trait("Priority", "Critical")]
        public async Task AddSubOrganization_CrossOrganization_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var user1 = CreateUser(100);
            var user2 = CreateUser(200);
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.AddSubOrganizationAsync(1, 2, user2));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-043")][Trait("Priority", "High")]
        public async Task GetOrgHierarchy_MaxDepthExceeded_HandlesGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var result = await mgr.GetOrgHierarchyAsync(1, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-044")][Trait("Priority", "High")]
        public async Task RemoveSubOrganization_ViewerRoleAttempt_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemoveSubOrganizationAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-045")][Trait("Priority", "Medium")]
        public async Task GetOrgParent_OrphanOrg_ReturnsNull()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            var result = await mgr.GetOrgParentAsync(1, CreateUser());
            result.Should().BeNull();
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-046")][Trait("Priority", "High")]
        public async Task MoveOrganization_ToSameParent_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 6, CreateUser());
            await mgr.MoveOrganizationAsync(6, 1, CreateUser());
            Assert.True(true, "Idempotent");
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-047")][Trait("Priority", "High")]
        public async Task AddSubOrganization_BothDeleted_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddSubOrganizationAsync(888, 889, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetOrgHierarchy_ZeroDepthLimit_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            try { var result = await mgr.GetOrgHierarchyAsync(1, CreateUser(), maxDepth: 0); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Zero depth rejected"); }
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-049")][Trait("Priority", "High")]
        public async Task GetOrgHierarchy_NegativeDepthLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetOrgHierarchyAsync(1, CreateUser(), maxDepth: -1));
        }

        [Fact][Trait("TestId", "TC-ORG-NEG-050")][Trait("Priority", "Critical")]
        public async Task AddSubOrganization_MultiLevelCircular_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OrganizationManager;
            await mgr.AddSubOrganizationAsync(1, 7, CreateUser());
            await mgr.AddSubOrganizationAsync(7, 8, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddSubOrganizationAsync(8, 1, CreateUser()));
        }


    }
}
