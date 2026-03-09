using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Users;
using AutoMapper;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Comprehensive functional tests for CommentManager CRUD operations.
/// </summary>
public class CommentManagerFullTests : ManagerTestBase
{
    private readonly CommentManager _manager;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IManagerWrapper> _mockManagerWrapper;

    public CommentManagerFullTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockManagerWrapper = new Mock<IManagerWrapper>();

        var mockUserDataManager = new Mock<IUserDataManager>();
        mockUserDataManager.Setup(u => u.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PAOUserModel { Id = 1, Email = "test@unops.org" });
        _mockManagerWrapper.Setup(m => m.UserDataManager).Returns(mockUserDataManager.Object);

        _mockMapper.Setup(m => m.Map<CommentModel>(It.IsAny<Comment>()))
            .Returns((Comment c) => new CommentModel
            {
                Id = c.Id, Content = c.Content, EntityType = c.EntityType,
                EntityId = c.EntityId, IsPinned = c.IsPinned, IsEdited = c.IsEdited
            });

        _manager = new CommentManager(_mockMapper.Object, Context, _mockManagerWrapper.Object);
    }

    private async Task<Comment> SeedComment(string entityType = "Partner", int entityId = 1,
        string content = "Test comment", bool pinned = false, bool deleted = false, int? parentId = null)
    {
        var comment = new Comment
        {
            Name = $"Comment-{Guid.NewGuid():N}",
            EntityType = entityType, EntityId = entityId, Content = content,
            IsPinned = pinned, IsDeleted = deleted, IsEdited = false,
            ParentCommentId = parentId, Status = EntityStatus.Active,
            CreatedBy = TestUserId, LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();
        return comment;
    }

    #region Positive Tests

    [Fact]
    public async Task P1_CreateComment_ReturnsCreatedComment()
    {
        var request = new CommentRequest { EntityType = "Partner", EntityId = 1, Content = "Hello" };
        var result = await _manager.CreateCommentAsync(request);
        result.Should().NotBeNull();
        result.Content.Should().Be("Hello");
    }

    [Fact]
    public async Task P2_GetCommentsByEntity_ReturnsComments()
    {
        await SeedComment("Partner", 1, "Comment 1");
        await SeedComment("Partner", 1, "Comment 2");
        var results = await _manager.GetCommentsByEntityAsync("Partner", 1);
        results.Should().HaveCountGreaterOrEqualTo(2);
    }

    #endregion

    #region Negative Tests

    [Fact]
    public async Task N1_GetCommentById_NonexistentId_ReturnsNull()
    {
        var result = await _manager.GetCommentByIdAsync(99999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task N2_GetCommentById_DeletedComment_ReturnsNull()
    {
        var comment = await SeedComment(deleted: true);
        var result = await _manager.GetCommentByIdAsync(comment.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task N3_UpdateComment_NonexistentId_Throws()
    {
        var request = new UpdateCommentRequest { Id = 99999, Content = "Updated" };
        Func<Task> act = () => _manager.UpdateCommentAsync(request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task N4_UpdateComment_DeletedComment_Throws()
    {
        var comment = await SeedComment(deleted: true);
        var request = new UpdateCommentRequest { Id = comment.Id, Content = "Updated" };
        Func<Task> act = () => _manager.UpdateCommentAsync(request);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task N5_DeleteComment_NonexistentId_Throws()
    {
        Func<Task> act = () => _manager.DeleteCommentAsync(99999);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task N6_TogglePin_DeletedComment_Throws()
    {
        var comment = await SeedComment(deleted: true);
        Func<Task> act = () => _manager.TogglePinAsync(comment.Id);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Edge/Boundary Tests

    [Fact]
    public async Task E1_GetCommentsByEntity_NoComments_ReturnsEmpty()
    {
        var results = await _manager.GetCommentsByEntityAsync("Partner", 99999);
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task E2_GetCommentCount_NoComments_ReturnsZero()
    {
        var count = await _manager.GetCommentCountAsync("Partner", 99999);
        count.Should().Be(0);
    }

    [Fact]
    public async Task E3_CreateComment_NullMentionedUserIds()
    {
        var request = new CommentRequest
        {
            EntityType = "Partner", EntityId = 1,
            Content = "No mentions", MentionedUserIds = null
        };
        var result = await _manager.CreateCommentAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task E4_CreateComment_EmptyMentionedUserIds()
    {
        var request = new CommentRequest
        {
            EntityType = "Partner", EntityId = 1,
            Content = "Empty mentions", MentionedUserIds = new List<int>()
        };
        var result = await _manager.CreateCommentAsync(request);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task E5_TogglePin_FalseToTrue()
    {
        var comment = await SeedComment(pinned: false);
        var result = await _manager.TogglePinAsync(comment.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task E6_TogglePin_TrueToFalse()
    {
        var comment = await SeedComment(pinned: true);
        var result = await _manager.TogglePinAsync(comment.Id);
        result.Should().BeFalse();
    }

    #endregion

    #region Functional Tests

    [Fact]
    public async Task F1_CreateComment_SetsIsEditedFalse()
    {
        var request = new CommentRequest { EntityType = "Partner", EntityId = 1, Content = "Test" };
        var result = await _manager.CreateCommentAsync(request);
        result.IsEdited.Should().BeFalse();
    }

    [Fact]
    public async Task F2_UpdateComment_SetsIsEditedTrue()
    {
        var comment = await SeedComment(content: "Original");
        var request = new UpdateCommentRequest { Id = comment.Id, Content = "Updated" };
        var result = await _manager.UpdateCommentAsync(request);
        result.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task F3_DeleteComment_SetsIsDeletedTrue()
    {
        var comment = await SeedComment();
        await _manager.DeleteCommentAsync(comment.Id);
        var dbComment = await Context.Comments.FindAsync(comment.Id);
        dbComment!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task F4_GetCommentsByEntity_OrdersPinnedFirst()
    {
        await SeedComment("Opportunity", 5, "Unpinned", pinned: false);
        await SeedComment("Opportunity", 5, "Pinned", pinned: true);
        var results = (await _manager.GetCommentsByEntityAsync("Opportunity", 5)).ToList();
        results.Should().HaveCountGreaterOrEqualTo(2);
        results.First().IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task F5_GetCommentsByEntity_FiltersOutDeleted()
    {
        await SeedComment("Contact", 3, "Visible");
        await SeedComment("Contact", 3, "Deleted", deleted: true);
        var results = await _manager.GetCommentsByEntityAsync("Contact", 3);
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task F6_GetCommentCount_CountsOnlyNonDeleted()
    {
        await SeedComment("Interaction", 7, "Active");
        await SeedComment("Interaction", 7, "Deleted", deleted: true);
        var count = await _manager.GetCommentCountAsync("Interaction", 7);
        count.Should().Be(1);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task I1_CreateRetrieveFlow()
    {
        var request = new CommentRequest { EntityType = "Partner", EntityId = 10, Content = "Integration test" };
        var created = await _manager.CreateCommentAsync(request);
        var retrieved = await _manager.GetCommentByIdAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Content.Should().Be("Integration test");
    }

    [Fact]
    public async Task I2_CreateUpdateFlow()
    {
        var request = new CommentRequest { EntityType = "Partner", EntityId = 10, Content = "Original" };
        var created = await _manager.CreateCommentAsync(request);
        var updateReq = new UpdateCommentRequest { Id = created.Id, Content = "Modified" };
        var updated = await _manager.UpdateCommentAsync(updateReq);
        updated.Content.Should().Be("Modified");
        updated.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task I3_CreateDeleteFlow()
    {
        var request = new CommentRequest { EntityType = "Partner", EntityId = 10, Content = "To delete" };
        var created = await _manager.CreateCommentAsync(request);
        var result = await _manager.DeleteCommentAsync(created.Id);
        result.Should().BeTrue();
        var retrieved = await _manager.GetCommentByIdAsync(created.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task I4_CreatePinVerifyOrdering()
    {
        var req1 = new CommentRequest { EntityType = "Partner", EntityId = 20, Content = "First" };
        var c1 = await _manager.CreateCommentAsync(req1);
        var req2 = new CommentRequest { EntityType = "Partner", EntityId = 20, Content = "Second" };
        await _manager.CreateCommentAsync(req2);
        await _manager.TogglePinAsync(c1.Id);
        var all = (await _manager.GetCommentsByEntityAsync("Partner", 20)).ToList();
        all.First().IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task I5_CreateMultiple_VerifyCount()
    {
        var entityId = 30;
        for (int i = 0; i < 5; i++)
        {
            var req = new CommentRequest { EntityType = "Partner", EntityId = entityId, Content = $"Comment {i}" };
            await _manager.CreateCommentAsync(req);
        }
        var count = await _manager.GetCommentCountAsync("Partner", entityId);
        count.Should().Be(5);
    }

    [Fact]
    public async Task I6_FullLifecycle_CreateUpdatePinDelete()
    {
        var req = new CommentRequest { EntityType = "Partner", EntityId = 40, Content = "Lifecycle" };
        var created = await _manager.CreateCommentAsync(req);
        await _manager.UpdateCommentAsync(new UpdateCommentRequest { Id = created.Id, Content = "Updated" });
        await _manager.TogglePinAsync(created.Id);
        await _manager.DeleteCommentAsync(created.Id);
        var retrieved = await _manager.GetCommentByIdAsync(created.Id);
        retrieved.Should().BeNull();
    }

    #endregion
}

// 3:1 Ratio Compliance Check
// | Category        | Count |
// |-----------------|-------|
// | Positive (P)    | 2     |
// | Negative (N)    | 6     | ✅ 6 >= 6 |
// | Edge/Boundary   | 6     | ✅ 6 >= 6 |
// | Functional (F)  | 6     | ✅ 6 >= 6 |
// | Integration (I) | 6     | ✅ 6 >= 6 |
