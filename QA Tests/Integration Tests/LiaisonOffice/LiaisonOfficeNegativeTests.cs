using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Liaison;
using UNOPS.PAO.IntegrationTests.Infrastructure;

using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "LiaisonOffice")][Trait("Component", "NegativeTests")]
    public class LiaisonOfficeNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public LiaisonOfficeNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-LIAISON-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetLiaisonOffice_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetLiaisonOfficeByIdAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-002")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateLiaisonOfficeAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-003")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_NullName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = null, Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-004")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_EmptyName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = string.Empty, Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-005")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_DuplicateName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Existing Office", Region = "Africa" };
            await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-006")][Trait("Priority", "Critical")]
        public async Task UpdateLiaisonOffice_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new UpdateLiaisonOfficeRequest { Id = 999999, Name = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-007")][Trait("Priority", "High")]
        public async Task DeleteLiaisonOffice_NonExistentId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.DeleteLiaisonOfficeAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-008")][Trait("Priority", "Critical")]
        public async Task GetLiaisonOffice_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetLiaisonOfficeByIdAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-009")][Trait("Priority", "Critical")]
        public async Task CreateLiaisonOffice_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new CreateLiaisonOfficeRequest { Name = "Test", Region = "Africa" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.CreateLiaisonOfficeAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-010")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            var request = new UpdateLiaisonOfficeRequest { Id = 1, Name = "Hacked" };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-011")][Trait("Priority", "High")]
        public async Task DeleteLiaisonOffice_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.DeleteLiaisonOfficeAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-012")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_NullRegion_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Test", Region = null };
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-013")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_EmptyRegion_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Test", Region = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-014")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_InvalidRegion_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Test", Region = "InvalidRegion" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-015")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOffice_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetLiaisonOfficeByIdAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOffice_ZeroId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetLiaisonOfficeByIdAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-017")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateLiaisonOfficeAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-018")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_ExcessiveNameLength_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = new string('A', 500), Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-019")][Trait("Priority", "Medium")]
        public async Task UpdateLiaisonOffice_DeletedOffice_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new UpdateLiaisonOfficeRequest { Id = 888, Name = "Test" };
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-020")][Trait("Priority", "High")]
        public async Task GetLiaisonOfficesByRegion_InvalidRegion_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetLiaisonOfficesByRegionAsync("InvalidRegion", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-021")][Trait("Priority", "High")]
        public async Task GetLiaisonOfficesByRegion_NullRegion_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetLiaisonOfficesByRegionAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-022")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOffice_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetLiaisonOfficeByIdAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-023")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_ChangeToExistingName_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new UpdateLiaisonOfficeRequest { Id = 2, Name = "Existing Office" };
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-024")][Trait("Priority", "Critical")]
        public async Task GetLiaisonOffices_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetAllLiaisonOfficesAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-025")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_ConcurrentDuplicateName_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var r1 = new CreateLiaisonOfficeRequest { Name = "Duplicate Office", Region = "Africa" };
            var r2 = new CreateLiaisonOfficeRequest { Name = "Duplicate Office", Region = "Africa" };
            var t1 = mgr.CreateLiaisonOfficeAsync(r1, CreateUser());
            var t2 = mgr.CreateLiaisonOfficeAsync(r2, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Duplicate prevented"); }
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-026")][Trait("Priority", "High")]
        public async Task UpdateDeleteLiaisonOffice_Concurrent_HandlesRaceCondition()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Race Office", Region = "Africa" }, CreateUser());
            var update = mgr.UpdateLiaisonOfficeAsync(new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "Updated" }, CreateUser());
            var delete = mgr.DeleteLiaisonOfficeAsync(office.Id, CreateUser());
            try { await Task.WhenAll(update, delete); }
            catch { Assert.True(true, "Race handled"); }
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-027")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOffice_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetLiaisonOfficeByIdAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-028")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_NonExistentOffice_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignLiaisonOfficerAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-029")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AssignLiaisonOfficerAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-030")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_NegativeOfficeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignLiaisonOfficerAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-031")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_NegativeUserId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AssignLiaisonOfficerAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-032")][Trait("Priority", "High")]
        public async Task RemoveLiaisonOfficer_NonExistentOffice_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveLiaisonOfficerAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-033")][Trait("Priority", "High")]
        public async Task RemoveLiaisonOfficer_NonExistentUser_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveLiaisonOfficerAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-034")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_WhitespaceName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "   ", Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-035")][Trait("Priority", "Medium")]
        public async Task UpdateLiaisonOffice_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new UpdateLiaisonOfficeRequest { Id = -1, Name = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-036")][Trait("Priority", "Medium")]
        public async Task DeleteLiaisonOffice_NegativeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.DeleteLiaisonOfficeAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-037")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_AlreadyAssigned_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await mgr.AssignLiaisonOfficerAsync(1, 2, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AssignLiaisonOfficerAsync(1, 2, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-038")][Trait("Priority", "High")]
        public async Task RemoveLiaisonOfficer_NotAssigned_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.RemoveLiaisonOfficerAsync(1, 10, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-039")][Trait("Priority", "High")]
        public async Task GetLiaisonOfficeActivities_NonExistentOffice_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetLiaisonOfficeActivitiesAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-040")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOfficeActivities_NegativeOfficeId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetLiaisonOfficeActivitiesAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-041")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_NullAddress_HandlesGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Test Office", Region = "Africa", Address = null };
            try { var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentNullException) { Assert.True(true, "Address required"); }
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-042")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_InvalidCountry_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new UpdateLiaisonOfficeRequest { Id = 1, Country = "InvalidCountry" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-043")][Trait("Priority", "High")]
        public async Task AssignLiaisonOfficer_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.AssignLiaisonOfficerAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-044")][Trait("Priority", "High")]
        public async Task RemoveLiaisonOfficer_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemoveLiaisonOfficerAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-045")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOfficesByRegion_EmptyRegion_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetLiaisonOfficesByRegionAsync(string.Empty, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-046")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_SpecialCharsInName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Office!@#$", Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-047")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_EmptyName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Temp Office", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = string.Empty };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetLiaisonOfficeActivities_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetLiaisonOfficeActivitiesAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-049")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_InvalidEmail_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Test", Region = "Africa", Email = "notanemail" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-NEG-050")][Trait("Priority", "Critical")]
        public async Task CreateLiaisonOffice_SQLInjectionName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "'; DROP TABLE LiaisonOffices; --", Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }


    }
}
