/**
 * @fileoverview Unit tests for DocumentTypeController
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Documents;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class DocumentTypeControllerTests : ControllerTestBase
{
    private readonly Mock<IDocumentTypeManager> _mockDocumentTypeManager;
    private readonly Mock<ILogger<DocumentTypeController>> _mockLogger;
    private readonly DocumentTypeController _controller;

    public DocumentTypeControllerTests()
    {
        _mockDocumentTypeManager = new Mock<IDocumentTypeManager>();
        _mockLogger = new Mock<ILogger<DocumentTypeController>>();

        MockManager.Setup(m => m.DocumentTypeManager).Returns(_mockDocumentTypeManager.Object);

        var userResolverService = new UserResolverService<int>(null!);

        _controller = new DocumentTypeController(
            MockManager.Object,
            _mockLogger.Object,
            MockAuthorizationService.Object,
            userResolverService
        );

        SetupControllerContext(_controller);
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesController()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task GetAll_WithValidEntityName_ReturnsDocumentTypes()
    {
        // Arrange
        var documentTypes = new List<DocumentTypeModel>
        {
            new DocumentTypeModel { Id = 1, Name = "Type1" }
        };

        var paginationResponse = new PaginationResponse<DocumentTypeModel>
        {
            Records = documentTypes,
            TotalCount = documentTypes.Count
        };

        _mockDocumentTypeManager
            .Setup(m => m.GetDocumentTypesAsync(It.IsAny<DocumentTypeRequestParameters>()))
            .Returns(paginationResponse);

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetAll("partner");

        // Assert
        var okResult = AssertOkResult(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetAll_WithInvalidEntityName_ReturnsError()
    {
        // Arrange
        _mockDocumentTypeManager
            .Setup(m => m.GetDocumentTypesAsync(It.IsAny<DocumentTypeRequestParameters>()))
            .Throws(new ArgumentException("Invalid entity name"));

        // Act
        var result = await _controller.GetAll("invalid");

        // Assert
        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.True(statusResult.StatusCode == 400 || statusResult.StatusCode == 500);
    }
}
