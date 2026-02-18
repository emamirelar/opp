/**
 * @fileoverview Unit tests for SavedFilterController
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class SavedFilterControllerTests : ControllerTestBase
{
    private readonly Mock<ISavedFilterService> _mockSavedFilterService;
    private readonly Mock<ILogger<SavedFilterController>> _mockLogger;
    private readonly SavedFilterController _controller;

    public SavedFilterControllerTests()
    {
        _mockSavedFilterService = new Mock<ISavedFilterService>();
        _mockLogger = new Mock<ILogger<SavedFilterController>>();

        var userResolverService = new UserResolverService<int>(null!);

        _controller = new SavedFilterController(
            _mockSavedFilterService.Object,
            userResolverService,
            MockAuthorizationService.Object,
            _mockLogger.Object
        );

        SetupControllerContext(_controller);
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesController()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task GetSavedFilter_ReturnsResult()
    {
        // Arrange
        _mockSavedFilterService.Setup(s => s.GetSavedFilterAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync((UNOPS.PAO.Models.Filters.SavedFilterModel?)null);

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetSavedFilter(1);

        // Assert
        Assert.NotNull(result);
    }
}
