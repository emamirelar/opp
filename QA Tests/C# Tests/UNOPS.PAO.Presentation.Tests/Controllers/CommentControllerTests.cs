/**
 * @fileoverview Unit tests for CommentController
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class CommentControllerTests : ControllerTestBase
{
    private readonly Mock<ICommentManager> _mockCommentManager;
    private readonly Mock<ILogger<CommentController>> _mockLogger;
    private readonly CommentController _controller;

    public CommentControllerTests()
    {
        _mockCommentManager = new Mock<ICommentManager>();
        _mockLogger = new Mock<ILogger<CommentController>>();

        MockManager.Setup(m => m.CommentManager).Returns(_mockCommentManager.Object);

        _controller = new CommentController(
            MockManager.Object,
            _mockLogger.Object
        );

        SetupControllerContext(_controller);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_CreatesController()
    {
        Assert.NotNull(_controller);
    }

    #endregion

    #region GetCommentsByEntity Tests

    [Fact]
    public async Task GetCommentsByEntity_WithValidParameters_ReturnsComments()
    {
        // Arrange
        var comments = new List<CommentModel>
        {
            new CommentModel { Id = 1, EntityType = "Partner", EntityId = 1, Content = "Test comment" }
        };
        
        _mockCommentManager
            .Setup(m => m.GetCommentsByEntityAsync("Partner", 1, true))
            .ReturnsAsync(comments);

        // Act
        var result = await _controller.GetCommentsByEntity("Partner", 1, true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    #endregion

    #region GetCommentById Tests

    [Fact]
    public async Task GetCommentById_WithValidId_ReturnsComment()
    {
        // Arrange
        var comment = new CommentModel { Id = 1, EntityType = "Partner", EntityId = 1, Content = "Test" };
        
        _mockCommentManager
            .Setup(m => m.GetCommentByIdAsync(1))
            .ReturnsAsync(comment);

        // Act
        var result = await _controller.GetCommentById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetCommentById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.GetCommentByIdAsync(999))
            .ReturnsAsync((CommentModel)null!);

        // Act
        var result = await _controller.GetCommentById(999);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFound.StatusCode);
    }

    #endregion

    #region Create Comment Tests

    [Fact]
    public async Task CreateComment_WithValidData_Returns201()
    {
        // Arrange
        var request = new CommentRequest 
        { 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = "Test comment" 
        };
        var createdComment = new CommentModel 
        { 
            Id = 1, 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = "Test comment" 
        };

        _mockCommentManager
            .Setup(m => m.CreateCommentAsync(It.IsAny<CommentRequest>()))
            .ReturnsAsync(createdComment);

        // Act
        var result = await _controller.CreateComment(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(CommentController.GetCommentById), createdResult.ActionName);
    }

    [Fact]
    public async Task CreateComment_WithEmptyContent_ReturnsError()
    {
        // Arrange
        var request = new CommentRequest 
        { 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = "" 
        };

        _mockCommentManager
            .Setup(m => m.CreateCommentAsync(It.IsAny<CommentRequest>()))
            .ThrowsAsync(new ArgumentException("Content is required"));

        // Act
        var result = await _controller.CreateComment(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateComment_WithLongContent_Succeeds()
    {
        // Arrange
        var longContent = new string('A', 5000);
        var request = new CommentRequest 
        { 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = longContent 
        };
        var createdComment = new CommentModel 
        { 
            Id = 1, 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = longContent 
        };

        _mockCommentManager
            .Setup(m => m.CreateCommentAsync(It.IsAny<CommentRequest>()))
            .ReturnsAsync(createdComment);

        // Act
        var result = await _controller.CreateComment(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    #endregion

    #region Update Comment Tests

    [Fact]
    public async Task UpdateComment_WithValidData_Returns200()
    {
        // Arrange
        var request = new UpdateCommentRequest 
        { 
            Id = 1, 
            Content = "Updated comment" 
        };
        var updatedComment = new CommentModel 
        { 
            Id = 1, 
            EntityType = "Partner", 
            EntityId = 1, 
            Content = "Updated comment" 
        };

        _mockCommentManager
            .Setup(m => m.UpdateCommentAsync(It.IsAny<UpdateCommentRequest>()))
            .ReturnsAsync(updatedComment);

        // Act
        var result = await _controller.UpdateComment(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateComment_WithInvalidId_Returns404()
    {
        // Arrange
        var request = new UpdateCommentRequest 
        { 
            Id = 999, 
            Content = "Updated comment" 
        };

        _mockCommentManager
            .Setup(m => m.UpdateCommentAsync(It.IsAny<UpdateCommentRequest>()))
            .ReturnsAsync((CommentModel)null!);

        // Act
        var result = await _controller.UpdateComment(999, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region Delete Comment Tests

    [Fact]
    public async Task DeleteComment_WithValidId_Returns204()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.DeleteCommentAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteComment(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteComment_WithInvalidId_Returns404()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.DeleteCommentAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteComment(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region Toggle Pin Tests

    [Fact]
    public async Task TogglePin_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.TogglePinAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TogglePin(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task TogglePin_WithInvalidId_Returns404()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.TogglePinAsync(999))
            .ThrowsAsync(new KeyNotFoundException("Comment not found"));

        // Act
        var result = await _controller.TogglePin(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region Get Comment Count Tests

    [Fact]
    public async Task GetCommentCount_WithValidEntity_ReturnsCount()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.GetCommentCountAsync("Partner", 1))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.GetCommentCount("Partner", 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetCommentCount_WithZeroComments_ReturnsZero()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.GetCommentCountAsync("Partner", 1))
            .ReturnsAsync(0);

        // Act
        var result = await _controller.GetCommentCount("Partner", 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    #endregion

    #region GetCommentsByEntity Additional Tests

    [Fact]
    public async Task GetCommentsByEntity_WithNoComments_ReturnsEmptyList()
    {
        // Arrange
        _mockCommentManager
            .Setup(m => m.GetCommentsByEntityAsync("Partner", 1, true))
            .ReturnsAsync(new List<CommentModel>());

        // Act
        var result = await _controller.GetCommentsByEntity("Partner", 1, true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var comments = Assert.IsAssignableFrom<IEnumerable<CommentModel>>(okResult.Value);
        Assert.Empty(comments);
    }

    [Fact]
    public async Task GetCommentsByEntity_WithIncludeRepliesFalse_ExcludesReplies()
    {
        // Arrange
        var comments = new List<CommentModel>
        {
            new CommentModel { Id = 1, EntityType = "Partner", EntityId = 1, Content = "Parent comment" }
        };

        _mockCommentManager
            .Setup(m => m.GetCommentsByEntityAsync("Partner", 1, false))
            .ReturnsAsync(comments);

        // Act
        var result = await _controller.GetCommentsByEntity("Partner", 1, false);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedComments = Assert.IsAssignableFrom<IEnumerable<CommentModel>>(okResult.Value);
        Assert.Single(returnedComments);
    }

    #endregion
}
