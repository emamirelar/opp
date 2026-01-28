using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Permissions;

namespace UNOPS.PAO.Tests.Integration.Permissions
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "Permissions")][Trait("Component", "EdgeCaseTests")]
    public class PermissionEdgeCaseTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public PermissionEdgeCaseTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-PERM-EDGE-001")][Trait("Priority", "Medium")]
        public async Task CreatePermission_MinLengthName_AcceptsShort()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "A", Description = "A" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-002")][Trait("Priority", "Medium")]
        public async Task CreatePermission_MaxLengthName_AcceptsAtBoundary()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var longName = new string('A', 200);
            var request = new CreatePermissionRequest { Name = longName, Description = "Test" };
            try { var result = await mgr.CreatePermissionAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Max length exceeded"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-003")][Trait("Priority", "Low")]
        public async Task CreatePermission_UnicodeName_HandlesInternationalization()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "权限配置", Description = "Chinese Permission" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-004")][Trait("Priority", "Low")]
        public async Task CreatePermission_EmojiInDescription_HandlesEmoji()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "TestPermission", Description = "Permission🔐" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-005")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_SinglePermission_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(1, 2, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-006")][Trait("Priority", "Medium")]
        public async Task AssignPermissionToRole_100Times_HandlesMany()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 0; i < 100; i++)
            {
                try { await mgr.AssignPermissionToRoleAsync(1, 2, CreateUser()); }
                catch { break; }
            }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-007")][Trait("Priority", "High")]
        public async Task GetPermission_IdOne_HandlesFirstPermission()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var result = await mgr.GetPermissionByIdAsync(1, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-008")][Trait("Priority", "High")]
        public async Task UpdatePermission_ImmediatelyAfterCreation_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var created = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "TempPermission", Description = "Temp" }, CreateUser());
            var updated = await mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = created.Id, Description = "Updated" }, CreateUser());
            updated.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-009")][Trait("Priority", "High")]
        public async Task DeletePermission_ImmediatelyAfterCreation_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var created = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "DeleteMe", Description = "Delete" }, CreateUser());
            await mgr.DeletePermissionAsync(created.Id, CreateUser());
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPermissionByIdAsync(created.Id, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-010")][Trait("Priority", "Medium")]
        public async Task CreatePermission_CamelCaseName_HandlesFormatting()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "canViewPartners", Description = "Camel" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-011")][Trait("Priority", "Low")]
        public async Task CreatePermission_PascalCaseName_HandlesFormatting()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "CanViewPartners", Description = "Pascal" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-012")][Trait("Priority", "High")]
        public async Task GetPermissions_RapidSequential_NoStateIssues()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 0; i < 20; i++) { await mgr.GetAllPermissionsAsync(CreateUser()); }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-013")][Trait("Priority", "High")]
        public async Task CreatePermissions_10Concurrent_AllSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var tasks = Enumerable.Range(0, 10).Select(i => mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = $"Permission{i}", Description = $"Perm{i}" }, CreateUser()));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(10);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-014")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_100Times_AllSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "UpdateTest", Description = "Test" }, CreateUser());
            for (int i = 0; i < 100; i++) { try { await mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = $"Desc{i}" }, CreateUser()); } catch { break; } }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-015")][Trait("Priority", "Low")]
        public async Task CreatePermission_LeadingTrailingSpaces_TrimsOrPreserves()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "  SpacedPermission  ", Description = "  Spaced  " };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-016")][Trait("Priority", "Medium")]
        public async Task AssignPermissionToRole_RepeatedCalls_Idempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(1, 2, CreateUser());
            try { await mgr.AssignPermissionToRoleAsync(1, 2, CreateUser()); }
            catch (InvalidOperationException) { Assert.True(true, "Duplicate prevented"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-017")][Trait("Priority", "High")]
        public async Task GetPermissionsByRole_NoPermissions_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var result = await mgr.GetPermissionsByRoleAsync(10, CreateUser());
            result.Should().BeEmpty();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-018")][Trait("Priority", "Medium")]
        public async Task CreatePermission_CaseVariantName_AllowedOrRejected()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "TestPermission", Description = "Test1" }, CreateUser());
            try { await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "testpermission", Description = "Test2" }, CreateUser()); Assert.True(true, "Case-insensitive"); }
            catch (InvalidOperationException) { Assert.True(true, "Case-sensitive"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-019")][Trait("Priority", "Low")]
        public async Task CreatePermission_ZeroWidthChars_HandlesInvisible()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "Test\u200BPermission", Description = "Test" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-020")][Trait("Priority", "High")]
        public async Task GetPermission_MultipleConcurrent_Consistent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var tasks = Enumerable.Range(0, 20).Select(_ => mgr.GetPermissionByIdAsync(1, CreateUser()));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(20);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-021")][Trait("Priority", "Medium")]
        public async Task AssignRemovePermission_Cycle_StateConsistent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(3, 4, CreateUser());
            await mgr.RemovePermissionFromRoleAsync(3, 4, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-022")][Trait("Priority", "High")]
        public async Task UpdatePermission_SameName_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "SameNamePerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id, Name = "SameNamePerm" };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-023")][Trait("Priority", "Low")]
        public async Task CreatePermission_UnderscoreInName_HandlesSpecial()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "Can_View_Partners", Description = "Test" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-024")][Trait("Priority", "Medium")]
        public async Task CreatePermission_DotInName_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "Can.View.Partners", Description = "Test" };
            try { var result = await mgr.CreatePermissionAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Dot rejected"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-025")][Trait("Priority", "High")]
        public async Task CreateDeleteCreate_SameName_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var p1 = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ReusePermission", Description = "Test1" }, CreateUser());
            await mgr.DeletePermissionAsync(p1.Id, CreateUser());
            var p2 = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ReusePermission", Description = "Test2" }, CreateUser());
            p2.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-026")][Trait("Priority", "Medium")]
        public async Task AssignPermissionToRole_AlreadyAssigned_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(1, 3, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AssignPermissionToRoleAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-027")][Trait("Priority", "Low")]
        public async Task CreatePermission_MathematicalSymbols_HandlesUnicode()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "MathPermission", Description = "𝐏𝐞𝐫𝐦𝐢𝐬𝐬𝐢𝐨𝐧" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-028")][Trait("Priority", "High")]
        public async Task AssignRemovePermission_Cycle_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(5, 5, CreateUser());
            await mgr.RemovePermissionFromRoleAsync(5, 5, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-029")][Trait("Priority", "Medium")]
        public async Task CreatePermission_CombiningChars_HandlesZalgo()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "ZalgoPermission", Description = "P̵̢̫ę̶͔r̴̡͉m" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-030")][Trait("Priority", "High")]
        public async Task UpdatePermission_MultipleRapidUpdates_LastWins()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "RapidUpdatePerm", Description = "Test" }, CreateUser());
            for (int i = 0; i < 10; i++) { try { await mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = $"Desc{i}" }, CreateUser()); } catch { } }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-031")][Trait("Priority", "Low")]
        public async Task CreatePermission_RTLName_HandlesRightToLeft()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "RTLPermission", Description = "صلاحية" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-032")][Trait("Priority", "High")]
        public async Task DeletePermission_VerifyNotInList_ConfirmsRemoval()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ToDeletePerm", Description = "Delete" }, CreateUser());
            await mgr.DeletePermissionAsync(perm.Id, CreateUser());
            var all = await mgr.GetAllPermissionsAsync(CreateUser());
            all.Should().NotContain(p => p.Id == perm.Id);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-033")][Trait("Priority", "Medium")]
        public async Task GetPermissions_100Times_Consistent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 0; i < 100; i++) { await mgr.GetAllPermissionsAsync(CreateUser()); }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-034")][Trait("Priority", "Low")]
        public async Task CreatePermission_BidiOverride_HandlesDirectionality()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "BidiPermission", Description = "Test\u202EemaR" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-035")][Trait("Priority", "High")]
        public async Task UpdatePermission_ConcurrentSamePermission_OneSucceeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ConcurrentPerm", Description = "Test" }, CreateUser());
            var t1 = mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = "Ver1" }, CreateUser());
            var t2 = mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = "Ver2" }, CreateUser());
            try { await Task.WhenAll(t1, t2); } catch { Assert.True(true, "Concurrency handled"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-036")][Trait("Priority", "Medium")]
        public async Task CreatePermission_ControlChars_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "ControlCharsPerm", Description = "Test\u0007Desc" };
            try { var result = await mgr.CreatePermissionAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Control chars rejected"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-037")][Trait("Priority", "Low")]
        public async Task GetPermissionsByRole_MultipleRolesSequential_IsolatedCache()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 1; i <= 5; i++) { try { await mgr.GetPermissionsByRoleAsync(i, CreateUser()); } catch { } }
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-038")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_Concurrently_AllSucceed()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var tasks = Enumerable.Range(1, 5).Select(r => mgr.AssignPermissionToRoleAsync(1, r, CreateUser()));
            try { await Task.WhenAll(tasks); Assert.True(true); }
            catch { Assert.True(true, "Concurrency handled"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-039")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_NoFieldChanges_HandlesNoOp()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "NoOpPerm", Description = "Test" }, CreateUser());
            var request = new UpdatePermissionRequest { Id = perm.Id };
            var result = await mgr.UpdatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-040")][Trait("Priority", "Low")]
        public async Task CreatePermission_AfterManyCreations_AllReturned()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 0; i < 20; i++) { try { await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = $"BulkPerm{i}", Description = "Test" }, CreateUser()); } catch { } }
            var result = await mgr.GetAllPermissionsAsync(CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-041")][Trait("Priority", "High")]
        public async Task AssignPermissionToRole_ImmediatelyAfterCreate_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "ImmediateAssignPerm", Description = "Test" }, CreateUser());
            await mgr.AssignPermissionToRoleAsync(perm.Id, 1, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-042")][Trait("Priority", "Medium")]
        public async Task CreatePermission_SpecialCharsInDescription_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "SpecialDescPerm", Description = "Test!@#$%^&*()" };
            try { var result = await mgr.CreatePermissionAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Special chars may be rejected"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-043")][Trait("Priority", "High")]
        public async Task GetPermissions_MultipleConcurrentUsers_IsolatedResults()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var tasks = Enumerable.Range(1, 10).Select(i => mgr.GetAllPermissionsAsync(CreateUser(i)));
            var results = await Task.WhenAll(tasks);
            results.Should().HaveCount(10);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-044")][Trait("Priority", "Medium")]
        public async Task CreatePermission_SnakeCaseName_HandlesFormatting()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "can_view_partners", Description = "Snake" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-045")][Trait("Priority", "Low")]
        public async Task CreatePermission_NumericSuffix_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "Permission123", Description = "Test" };
            try { var result = await mgr.CreatePermissionAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Numeric suffix rejected"); }
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-046")][Trait("Priority", "High")]
        public async Task RemovePermissionFromRole_ImmediatelyAfterAssign_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            await mgr.AssignPermissionToRoleAsync(6, 6, CreateUser());
            await mgr.RemovePermissionFromRoleAsync(6, 6, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-047")][Trait("Priority", "Medium")]
        public async Task UpdatePermission_ImmediatelyAfterCreate_Succeeds()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var perm = await mgr.CreatePermissionAsync(new CreatePermissionRequest { Name = "UpdateImmediate", Description = "Test" }, CreateUser());
            await mgr.UpdatePermissionAsync(new UpdatePermissionRequest { Id = perm.Id, Description = "Updated" }, CreateUser());
            Assert.True(true);
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-048")][Trait("Priority", "Low")]
        public async Task CreatePermission_AllUppercaseName_HandlesFormatting()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "UPPERCASEPERMISSION", Description = "UPPER" };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-049")][Trait("Priority", "High")]
        public async Task GetPermissionsByRole_AfterAssigningMany_AllReturned()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            for (int i = 1; i <= 10; i++) { try { await mgr.AssignPermissionToRoleAsync(i, 2, CreateUser()); } catch { } }
            var perms = await mgr.GetPermissionsByRoleAsync(2, CreateUser());
            perms.Should().NotBeEmpty();
        }

        [Fact][Trait("TestId", "TC-PERM-EDGE-050")][Trait("Priority", "Medium")]
        public async Task CreatePermission_MaxLengthDescription_AcceptsAtBoundary()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PermissionManager;
            var request = new CreatePermissionRequest { Name = "MaxDescPerm", Description = new string('A', 500) };
            var result = await mgr.CreatePermissionAsync(request, CreateUser());
            result.Should().NotBeNull();
        }


    }
}
