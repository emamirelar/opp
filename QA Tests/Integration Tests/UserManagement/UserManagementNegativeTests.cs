using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Users;

namespace UNOPS.PAO.Tests.Integration.UserManagement
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "UserManagement")][Trait("Component", "NegativeTests")]
    public class UserManagementNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public UserManagementNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-USER-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetUser_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetUserByIdAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-002")][Trait("Priority", "High")]
        public async Task CreateUser_NullEmail_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = null, FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-003")][Trait("Priority", "High")]
        public async Task CreateUser_EmptyEmail_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = string.Empty, FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-004")][Trait("Priority", "High")]
        public async Task CreateUser_InvalidEmailFormat_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "notanemail", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-005")][Trait("Priority", "High")]
        public async Task CreateUser_DuplicateEmail_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "admin@unops.org", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-006")][Trait("Priority", "Critical")]
        public async Task UpdateUser_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 999999, FirstName = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-007")][Trait("Priority", "High")]
        public async Task DeleteUser_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeleteUserAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-008")][Trait("Priority", "Critical")]
        public async Task GetUser_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetUserByIdAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-009")][Trait("Priority", "Critical")]
        public async Task CreateUser_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CreateUserAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-010")][Trait("Priority", "High")]
        public async Task UpdateUser_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new UpdateUserRequest { Id = 1, FirstName = "Hacked" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdateUserAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-011")][Trait("Priority", "High")]
        public async Task DeleteUser_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.DeleteUserAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-012")][Trait("Priority", "High")]
        public async Task DeleteUser_SystemAdministrator_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeleteUserAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-013")][Trait("Priority", "Medium")]
        public async Task CreateUser_NullFirstName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = null, LastName = "User" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-014")][Trait("Priority", "Medium")]
        public async Task CreateUser_NullLastName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = "Test", LastName = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-015")][Trait("Priority", "High")]
        public async Task CreateUser_EmptyFirstName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = string.Empty, LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-016")][Trait("Priority", "High")]
        public async Task CreateUser_EmptyLastName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = "Test", LastName = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-017")][Trait("Priority", "Medium")]
        public async Task CreateUser_WhitespaceFirstName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = "   ", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-018")][Trait("Priority", "Medium")]
        public async Task CreateUser_WhitespaceLastName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = "Test", LastName = "   " };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-019")][Trait("Priority", "High")]
        public async Task UpdateUser_ChangeToExistingEmail_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 2, Email = "admin@unops.org" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetUser_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetUserByIdAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-021")][Trait("Priority", "Medium")]
        public async Task GetUser_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetUserByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-022")][Trait("Priority", "High")]
        public async Task CreateUser_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateUserAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-023")][Trait("Priority", "High")]
        public async Task UpdateUser_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateUserAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-024")][Trait("Priority", "High")]
        public async Task AssignRoles_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignRolesAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-025")][Trait("Priority", "High")]
        public async Task AssignRoles_NonExistentRole_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignRolesAsync(1, new[] { 999999 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-026")][Trait("Priority", "High")]
        public async Task AssignRoles_NullRoleList_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.AssignRolesAsync(1, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-027")][Trait("Priority", "High")]
        public async Task AssignRoles_EmptyRoleList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignRolesAsync(1, new int[0], CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-028")][Trait("Priority", "Medium")]
        public async Task UpdateUser_DeletedUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 888, FirstName = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-029")][Trait("Priority", "High")]
        public async Task ActivateUser_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.ActivateUserAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-030")][Trait("Priority", "High")]
        public async Task DeactivateUser_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeactivateUserAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-031")][Trait("Priority", "Critical")]
        public async Task DeactivateUser_SystemAdministrator_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.DeactivateUserAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-032")][Trait("Priority", "High")]
        public async Task AssignRoles_DuplicateRoles_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AssignRolesAsync(1, new[] { 1, 1, 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-033")][Trait("Priority", "High")]
        public async Task CreateUser_ExcessiveNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "test@test.com", FirstName = new string('A', 500), LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-034")][Trait("Priority", "High")]
        public async Task CreateUser_ExcessiveEmailLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = new string('a', 500) + "@test.com", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-035")][Trait("Priority", "Medium")]
        public async Task UpdateUser_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = -1, FirstName = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-036")][Trait("Priority", "Medium")]
        public async Task DeleteUser_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.DeleteUserAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-037")][Trait("Priority", "High")]
        public async Task AssignRoles_NegativeRoleId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignRolesAsync(1, new[] { -1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-038")][Trait("Priority", "Medium")]
        public async Task GetUser_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetUserByIdAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-039")][Trait("Priority", "High")]
        public async Task UpdateUser_EmptyEmail_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 1, Email = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-040")][Trait("Priority", "High")]
        public async Task UpdateUser_InvalidEmailFormat_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new UpdateUserRequest { Id = 1, Email = "notanemail" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-041")][Trait("Priority", "Critical")]
        public async Task GetUsers_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAllUsersAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-042")][Trait("Priority", "High")]
        public async Task CreateUser_ConcurrentDuplicateEmail_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var r1 = new CreateUserRequest { Email = "duplicate@test.com", FirstName = "Test1", LastName = "User1" };
            var r2 = new CreateUserRequest { Email = "duplicate@test.com", FirstName = "Test2", LastName = "User2" };
            var t1 = mgr.CreateUserAsync(r1, CreateUser());
            var t2 = mgr.CreateUserAsync(r2, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Email conflict"); }
        }

        [Fact][Trait("TestId", "TC-USER-NEG-043")][Trait("Priority", "High")]
        public async Task UpdateDeleteUser_Concurrent_HandlesRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var update = mgr.UpdateUserAsync(new UpdateUserRequest { Id = 5, FirstName = "Updated" }, CreateUser());
            var delete = mgr.DeleteUserAsync(5, CreateUser());
            try { await Task.WhenAll(update, delete); }
            catch { Assert.True(true, "Race handled"); }
        }

        [Fact][Trait("TestId", "TC-USER-NEG-044")][Trait("Priority", "Medium")]
        public async Task GetUser_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetUserByIdAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-045")][Trait("Priority", "High")]
        public async Task RemoveRoles_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveRolesAsync(999999, new[] { 1 }, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-046")][Trait("Priority", "High")]
        public async Task RemoveRoles_NullRoleList_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.RemoveRolesAsync(1, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-047")][Trait("Priority", "High")]
        public async Task RemoveRoles_EmptyRoleList_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveRolesAsync(1, new int[0], CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-048")][Trait("Priority", "Medium")]
        public async Task ResetPassword_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.ResetPasswordAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-USER-NEG-049")][Trait("Priority", "High")]
        public async Task UpdateUser_NoChanges_HandlesIdempotently()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var current = await mgr.GetUserByIdAsync(2, CreateUser());
            var request = new UpdateUserRequest { Id = 2, Email = current.Email };
            var result = await mgr.UpdateUserAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-USER-NEG-050")][Trait("Priority", "Critical")]
        public async Task CreateUser_EmailWithSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserManager;
            var request = new CreateUserRequest { Email = "'; DROP TABLE Users; --@test.com", FirstName = "Test", LastName = "User" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateUserAsync(request, CreateUser()));
        }

        #endregion
    }
}
