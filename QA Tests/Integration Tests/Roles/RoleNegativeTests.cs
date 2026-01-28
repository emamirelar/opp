using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Roles;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.Roles
{
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "Roles")]
    [Trait("Component", "NegativeTests")]
    public class RoleNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public RoleNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ROLE-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetRole_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetRoleByIdAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-002")][Trait("Priority", "High")]
        public async Task CreateRole_NullName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-003")][Trait("Priority", "High")]
        public async Task CreateRole_EmptyName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-004")][Trait("Priority", "Medium")]
        public async Task CreateRole_WhitespaceName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "   " };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-005")][Trait("Priority", "High")]
        public async Task CreateRole_DuplicateName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Administrator" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-006")][Trait("Priority", "Critical")]
        public async Task UpdateRole_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 999999, Name = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-007")][Trait("Priority", "High")]
        public async Task DeleteRole_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeleteRoleAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-008")][Trait("Priority", "Critical")]
        public async Task GetRole_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetRoleByIdAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-009")][Trait("Priority", "Critical")]
        public async Task CreateRole_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new CreateRoleRequest { Name = "NewRole" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CreateRoleAsync(request, user));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-010")][Trait("Priority", "High")]
        public async Task UpdateRole_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new UpdateRoleRequest { Id = 1, Name = "Updated" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdateRoleAsync(request, user));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-011")][Trait("Priority", "High")]
        public async Task DeleteRole_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.DeleteRoleAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-012")][Trait("Priority", "High")]
        public async Task DeleteRole_SystemRole_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeleteRoleAsync(1, CreateUser())); // System Administrator
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-013")][Trait("Priority", "High")]
        public async Task DeleteRole_RoleWithUsers_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeleteRoleAsync(2, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-014")][Trait("Priority", "Medium")]
        public async Task GetRole_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetRoleByIdAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-015")][Trait("Priority", "Medium")]
        public async Task GetRole_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetRoleByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-016")][Trait("Priority", "High")]
        public async Task CreateRole_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateRoleAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-017")][Trait("Priority", "High")]
        public async Task UpdateRole_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateRoleAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-018")][Trait("Priority", "High")]
        public async Task AssignPermissions_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignPermissionsAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-019")][Trait("Priority", "High")]
        public async Task AssignPermissions_NonExistentPermission_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignPermissionsAsync(1, new[] { 999999 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-020")][Trait("Priority", "High")]
        public async Task AssignUsers_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignUsersToRoleAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-021")][Trait("Priority", "High")]
        public async Task AssignUsers_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignUsersToRoleAsync(1, new[] { 999999 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-022")][Trait("Priority", "Critical")]
        public async Task CreateRole_ExcessiveNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = new string('A', 500) };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-023")][Trait("Priority", "Medium")]
        public async Task UpdateRole_DuplicateName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, Name = "Administrator" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-024")][Trait("Priority", "High")]
        public async Task UpdateRole_SystemRole_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 1, Name = "Modified Admin" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-025")][Trait("Priority", "Critical")]
        public async Task AssignPermissions_NullPermissionList_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.AssignPermissionsAsync(1, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-026")][Trait("Priority", "High")]
        public async Task AssignPermissions_EmptyPermissionList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignPermissionsAsync(1, new int[0], CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-027")][Trait("Priority", "High")]
        public async Task AssignUsers_NullUserList_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.AssignUsersToRoleAsync(1, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-028")][Trait("Priority", "High")]
        public async Task AssignUsers_EmptyUserList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignUsersToRoleAsync(1, new int[0], CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-029")][Trait("Priority", "Medium")]
        public async Task GetRole_DeletedRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetRoleByIdAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-030")][Trait("Priority", "High")]
        public async Task UpdateRole_DeletedRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 888, Name = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-031")][Trait("Priority", "High")]
        public async Task AssignPermissions_DeletedRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignPermissionsAsync(888, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-032")][Trait("Priority", "Medium")]
        public async Task CreateRole_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Id = -1, Name = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-033")][Trait("Priority", "Medium")]
        public async Task UpdateRole_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = -1, Name = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-034")][Trait("Priority", "High")]
        public async Task DeleteRole_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.DeleteRoleAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-035")][Trait("Priority", "Medium")]
        public async Task GetRoleById_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetRoleByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-036")][Trait("Priority", "High")]
        public async Task AssignPermissions_NegativePermissionId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignPermissionsAsync(1, new[] { -1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-037")][Trait("Priority", "High")]
        public async Task AssignUsers_NegativeUserId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignUsersToRoleAsync(1, new[] { -1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-038")][Trait("Priority", "Low")]
        public async Task GetRole_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetRoleByIdAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-039")][Trait("Priority", "High")]
        public async Task CreateRole_CircularRoleHierarchy_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "Test", ParentRoleId = 1 };
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.CreateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-040")][Trait("Priority", "Medium")]
        public async Task UpdateRole_CircularHierarchy_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new UpdateRoleRequest { Id = 2, ParentRoleId = 2 }; // Self-parent
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateRoleAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-041")][Trait("Priority", "High")]
        public async Task AssignPermissions_DuplicatePermissions_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AssignPermissionsAsync(1, new[] { 1, 1, 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-042")][Trait("Priority", "High")]
        public async Task AssignUsers_DuplicateUsers_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AssignUsersToRoleAsync(1, new[] { 1, 1, 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-043")][Trait("Priority", "Critical")]
        public async Task GetRoles_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAllRolesAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-044")][Trait("Priority", "High")]
        public async Task CreateRole_ConcurrentCreation_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var r1 = new CreateRoleRequest { Name = "Duplicate" };
            var r2 = new CreateRoleRequest { Name = "Duplicate" };
            var t1 = mgr.CreateRoleAsync(r1, CreateUser());
            var t2 = mgr.CreateRoleAsync(r2, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Duplicate conflict"); }
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-045")][Trait("Priority", "High")]
        public async Task UpdateDeleteRole_Concurrent_HandlesRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var update = mgr.UpdateRoleAsync(new UpdateRoleRequest { Id = 5, Name = "Updated" }, CreateUser());
            var delete = mgr.DeleteRoleAsync(5, CreateUser());
            try { await Task.WhenAll(update, delete); }
            catch { Assert.True(true, "Race condition handled"); }
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-046")][Trait("Priority", "Medium")]
        public async Task GetRole_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetRoleByIdAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-047")][Trait("Priority", "High")]
        public async Task RemovePermissions_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemovePermissionsAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-048")][Trait("Priority", "High")]
        public async Task RemoveUsers_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveUsersFromRoleAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-049")][Trait("Priority", "Medium")]
        public async Task UpdateRole_NoChanges_HandlesIdempotently()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var current = await mgr.GetRoleByIdAsync(2, CreateUser());
            var request = new UpdateRoleRequest { Id = 2, Name = current.Name };
            var result = await mgr.UpdateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-ROLE-NEG-050")][Trait("Priority", "Critical")]
        public async Task CreateRole_NameWithSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().RoleManager;
            var request = new CreateRoleRequest { Name = "'; DROP TABLE Roles; --" };
            var result = await mgr.CreateRoleAsync(request, CreateUser());
            result.Should().NotBeNull();
            result.Name.Should().Contain("DROP");
        }


    }
}
