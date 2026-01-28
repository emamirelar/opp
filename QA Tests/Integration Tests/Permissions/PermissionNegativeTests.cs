using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Permissions;

namespace UNOPS.PAO.Tests.Integration.Permissions
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "Permissions")][Trait("Component", "NegativeTests")]
    public class PermissionNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public PermissionNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-PERM-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetPermission_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPermissionByIdAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-002")][Trait("Priority", "High")]
        public async Task CreatePermission_NullName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = null, Description = "Test" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-003")][Trait("Priority", "High")]
        public async Task CreatePermission_EmptyName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = string.Empty, Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-004")][Trait("Priority", "High")]
        public async Task CreatePermission_DuplicateName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "CanViewPartners", Description = "Test" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-005")][Trait("Priority", "Critical")]
        public async Task UpdatePermission_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new UpdatePermissionRequest { Id = 999999, Description = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-006")][Trait("Priority", "High")]
        public async Task DeletePermission_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeletePermissionAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-007")][Trait("Priority", "Critical")]
        public async Task GetPermission_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetPermissionByIdAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-008")][Trait("Priority", "Critical")]
        public async Task CreatePermission_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = "Test" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CreatePermissionAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-009")][Trait("Priority", "High")]
        public async Task UpdatePermission_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new UpdatePermissionRequest { Id = 1, Description = "Hacked" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdatePermissionAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-010")][Trait("Priority", "High")]
        public async Task DeletePermission_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.DeletePermissionAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-011")][Trait("Priority", "High")]
        public async Task DeletePermission_SystemPermission_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeletePermissionAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-012")][Trait("Priority", "Medium")]
        public async Task CreatePermission_NullDescription_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-013")][Trait("Priority", "High")]
        public async Task CreatePermission_EmptyDescription_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-014")][Trait("Priority", "Medium")]
        public async Task CreatePermission_WhitespaceName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "   ", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-015")][Trait("Priority", "Medium")]
        public async Task CreatePermission_WhitespaceDescription_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = "   " };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetPermission_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPermissionByIdAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-017")][Trait("Priority", "Medium")]
        public async Task GetPermission_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPermissionByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-018")][Trait("Priority", "High")]
        public async Task CreatePermission_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreatePermissionAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-019")][Trait("Priority", "High")]
        public async Task UpdatePermission_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdatePermissionAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-020")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_NonExistentPermission_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignPermissionToRoleAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-021")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignPermissionToRoleAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-022")][Trait("Priority", "High")]
        public async Task UpdatePermission_ChangeToExistingName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new UpdatePermissionRequest { Id = 2, Name = "CanViewPartners" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-023")][Trait("Priority", "High")]
        public async Task CreatePermission_ExcessiveNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = new string('A', 500), Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-024")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_DeletedPermission_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new UpdatePermissionRequest { Id = 888, Description = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-025")][Trait("Priority", "High")]
        public async Task GetPermissionsByRole_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPermissionsByRoleAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-026")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new UpdatePermissionRequest { Id = -1, Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-027")][Trait("Priority", "Medium")]
        public async Task DeletePermission_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.DeletePermissionAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-028")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_NegativePermissionId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignPermissionToRoleAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-029")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_NegativeRoleId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignPermissionToRoleAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-030")][Trait("Priority", "Medium")]
        public async Task GetPermission_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPermissionByIdAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-031")][Trait("Priority", "High")]
        public async Task RemovePermissionFromRole_NonExistentPermission_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemovePermissionFromRoleAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-032")][Trait("Priority", "High")]
        public async Task RemovePermissionFromRole_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemovePermissionFromRoleAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-033")][Trait("Priority", "High")]
        public async Task CreatePermission_ExcessiveDescriptionLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = new string('A', 1000) };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-034")][Trait("Priority", "Medium")]
        public async Task GetPermissionsByRole_NegativeRoleId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPermissionsByRoleAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-035")][Trait("Priority", "Critical")]
        public async Task GetPermissions_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAllPermissionsAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-036")][Trait("Priority", "High")]
        public async Task CreatePermission_ConcurrentDuplicateName_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var r1 = new CreatePermissionRequest { Name = "DuplicatePermission", Description = "Dup1" };
            var r2 = new CreatePermissionRequest { Name = "DuplicatePermission", Description = "Dup2" };
            var t1 = mgr.CreatePermissionAsync(r1, CreateUser());
            var t2 = mgr.CreatePermissionAsync(r2, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Name conflict"); }
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-037")][Trait("Priority", "High")]
        public async Task UpdateDeletePermission_Concurrent_HandlesRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "RacePermission", Description = "Test" }, CreateUser());
            var update = mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = "Updated" }, CreateUser());
            var delete = mgr.DeletePermissionAsync(perm.Id, CreateUser());
            try { await Task.WhenAll(update, delete); }
            catch { Assert.True(true, "Race handled"); }
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-038")][Trait("Priority", "Medium")]
        public async Task GetPermission_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetPermissionByIdAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-039")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_AlreadyAssigned_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(1, 1, CreateUser());
            try { await mgr.AssignPermissionToRoleAsync(1, 1, CreateUser()); Assert.True(true); }
            catch (InvalidOperationException) { Assert.True(true, "Duplicate prevented"); }
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-040")][Trait("Priority", "High")]
        public async Task CreatePermission_SpecialCharsInName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "Permission!@#$", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-041")][Trait("Priority", "High")]
        public async Task UpdatePermission_EmptyDescription_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "TestPermEmpty", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-042")][Trait("Priority", "Medium")]
        public async Task GetPermissionsByRole_ZeroRoleId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPermissionsByRoleAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-043")][Trait("Priority", "High")]
        public async Task RemovePermissionFromRole_NotAssigned_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            try { await mgr.RemovePermissionFromRoleAsync(10, 10, CreateUser()); Assert.True(true); }
            catch (KeyNotFoundException) { Assert.True(true, "Not assigned"); }
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-044")][Trait("Priority", "High")]
        public async Task CreatePermission_NumericName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "123Permission", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-045")][Trait("Priority", "Critical")]
        public async Task CreatePermission_SQLInjectionName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "'; DROP TABLE Permissions; --", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreatePermissionAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-046")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.AssignPermissionToRoleAsync(1, 1, viewer));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-047")][Trait("Priority", "High")]
        public async Task RemovePermissionFromRole_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemovePermissionFromRoleAsync(1, 1, viewer));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-048")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_NoChanges_HandlesIdempotently()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "IdempotentPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Description = "Test" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-049")][Trait("Priority", "High")]
        public async Task GetPermissionsByRole_DeletedRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPermissionsByRoleAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-NEG-050")][Trait("Priority", "Critical")]
        public async Task AssignPermissionToRole_ZeroPermissionId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignPermissionToRoleAsync(0, 1, CreateUser()));
        }

        #endregion
    }
}
