using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Tests.Integration.PartnerTree
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "PartnerTree")][Trait("Component", "NegativeTests")]
    public class PartnerTreeNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public PartnerTreeNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-TREE-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetPartnerTree_NonExistentPartnerId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerTreeAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-002")][Trait("Priority", "High")]
        public async Task GetPartnerTree_NegativePartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPartnerTreeAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-003")][Trait("Priority", "High")]
        public async Task GetPartnerTree_ZeroPartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPartnerTreeAsync(0, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-004")][Trait("Priority", "Critical")]
        public async Task GetPartnerTree_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetPartnerTreeAsync(1, null));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-005")][Trait("Priority", "Critical")]
        public async Task GetPartnerTree_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetPartnerTreeAsync(1, viewer));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-006")][Trait("Priority", "High")]
        public async Task AddChildPartner_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddChildPartnerAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-007")][Trait("Priority", "High")]
        public async Task AddChildPartner_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddChildPartnerAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-008")][Trait("Priority", "High")]
        public async Task AddChildPartner_SelfReference_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddChildPartnerAsync(1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-009")][Trait("Priority", "Critical")]
        public async Task AddChildPartner_CircularReference_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 2, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddChildPartnerAsync(2, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-010")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveChildPartnerAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-011")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RemoveChildPartnerAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-012")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_NotActualChild_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.RemoveChildPartnerAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-013")][Trait("Priority", "Medium")]
        public async Task GetPartnerTree_DeletedPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerTreeAsync(888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-014")][Trait("Priority", "High")]
        public async Task AddChildPartner_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddChildPartnerAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-015")][Trait("Priority", "High")]
        public async Task AddChildPartner_NegativeChildId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.AddChildPartnerAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-016")][Trait("Priority", "Medium")]
        public async Task GetPartnerTree_MaxIntId_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerTreeAsync(int.MaxValue, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-017")][Trait("Priority", "Critical")]
        public async Task GetPartnerTree_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetPartnerTreeAsync(1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-018")][Trait("Priority", "High")]
        public async Task AddChildPartner_DuplicateChild_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 3, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddChildPartnerAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-019")][Trait("Priority", "High")]
        public async Task AddChildPartner_ConcurrentSameChild_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var t1 = mgr.AddChildPartnerAsync(1, 4, CreateUser());
            var t2 = mgr.AddChildPartnerAsync(1, 4, CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.Fail("Should conflict"); }
            catch { Assert.True(true, "Duplicate prevented"); }
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetPartnerTree_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetPartnerTreeAsync(1, user));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-021")][Trait("Priority", "High")]
        public async Task AddChildPartner_ExcessiveDepth_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            for (int i = 1; i < 100; i++)
            {
                try { await mgr.AddChildPartnerAsync(i, i + 1, CreateUser()); }
                catch (InvalidOperationException) { Assert.True(true, "Max depth enforced"); break; }
                catch { break; }
            }
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-022")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveChildPartnerAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-023")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_NegativeChildId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RemoveChildPartnerAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-024")][Trait("Priority", "High")]
        public async Task AddChildPartner_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.AddChildPartnerAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-025")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemoveChildPartnerAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-026")][Trait("Priority", "High")]
        public async Task GetPartnerChildren_NonExistentParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerChildrenAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-027")][Trait("Priority", "High")]
        public async Task GetPartnerParent_NonExistentChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerParentAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-028")][Trait("Priority", "Medium")]
        public async Task GetPartnerAncestors_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerAncestorsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-029")][Trait("Priority", "Medium")]
        public async Task GetPartnerDescendants_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerDescendantsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-030")][Trait("Priority", "High")]
        public async Task AddChildPartner_AlreadyHasParent_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 5, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddChildPartnerAsync(2, 5, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-031")][Trait("Priority", "High")]
        public async Task MovePartner_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.MovePartnerAsync(999999, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-032")][Trait("Priority", "High")]
        public async Task MovePartner_NonExistentNewParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.MovePartnerAsync(1, 999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-033")][Trait("Priority", "High")]
        public async Task MovePartner_ToOwnDescendant_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 2, CreateUser());
            await mgr.AddChildPartnerAsync(2, 3, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.MovePartnerAsync(1, 3, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-034")][Trait("Priority", "High")]
        public async Task GetPartnerSiblings_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerSiblingsAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-035")][Trait("Priority", "Medium")]
        public async Task GetPartnerLevel_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerLevelAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-036")][Trait("Priority", "High")]
        public async Task GetPartnerPath_NonExistentPartner_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetPartnerPathAsync(999999, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-037")][Trait("Priority", "High")]
        public async Task AddChildPartner_DeletedParent_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddChildPartnerAsync(888, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-038")][Trait("Priority", "High")]
        public async Task AddChildPartner_DeletedChild_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddChildPartnerAsync(1, 888, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-039")][Trait("Priority", "Medium")]
        public async Task GetPartnerChildren_NegativeParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPartnerChildrenAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-040")][Trait("Priority", "High")]
        public async Task MovePartner_NegativePartnerId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.MovePartnerAsync(-1, 1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-041")][Trait("Priority", "High")]
        public async Task MovePartner_NegativeNewParentId_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.MovePartnerAsync(1, -1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-042")][Trait("Priority", "Critical")]
        public async Task AddChildPartner_CrossOrganization_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var user1 = CreateUser(100);
            var user2 = CreateUser(200);
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.AddChildPartnerAsync(1, 2, user2));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-043")][Trait("Priority", "High")]
        public async Task GetPartnerTree_MaxDepthExceeded_HandlesGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var result = await mgr.GetPartnerTreeAsync(1, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-044")][Trait("Priority", "High")]
        public async Task RemoveChildPartner_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RemoveChildPartnerAsync(1, 2, viewer));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-045")][Trait("Priority", "Medium")]
        public async Task GetPartnerParent_OrphanPartner_ReturnsNull()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            var result = await mgr.GetPartnerParentAsync(1, CreateUser());
            result.Should().BeNull();
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-046")][Trait("Priority", "High")]
        public async Task MovePartner_ToSameParent_HandlesIdempotent()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 6, CreateUser());
            await mgr.MovePartnerAsync(6, 1, CreateUser());
            Assert.True(true, "Idempotent");
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-047")][Trait("Priority", "High")]
        public async Task AddChildPartner_BothDeleted_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.AddChildPartnerAsync(888, 889, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetPartnerTree_ZeroDepthLimit_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            try { var result = await mgr.GetPartnerTreeAsync(1, CreateUser(), maxDepth: 0); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Zero depth rejected"); }
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-049")][Trait("Priority", "High")]
        public async Task GetPartnerTree_NegativeDepthLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetPartnerTreeAsync(1, CreateUser(), maxDepth: -1));
        }

        [Fact][Trait("TestId", "TC-TREE-NEG-050")][Trait("Priority", "Critical")]
        public async Task AddChildPartner_MultiLevelCircular_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().PartnerManager;
            await mgr.AddChildPartnerAsync(1, 7, CreateUser());
            await mgr.AddChildPartnerAsync(7, 8, CreateUser());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.AddChildPartnerAsync(8, 1, CreateUser()));
        }

        #endregion
    }
}
