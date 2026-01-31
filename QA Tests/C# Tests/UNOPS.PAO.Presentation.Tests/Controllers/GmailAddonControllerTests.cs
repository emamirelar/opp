/**
 * @fileoverview Unit tests for GmailAddonController
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Controllers.Integrations;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class GmailAddonControllerTests : ControllerTestBase
{
    private readonly Mock<IManagerWrapper> _mockManagerWrapper;
    private readonly Mock<ILogger<GmailAddonController>> _mockLogger;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly GmailAddonController _controller;

    public GmailAddonControllerTests()
    {
        _mockManagerWrapper = new Mock<IManagerWrapper>();
        _mockLogger = new Mock<ILogger<GmailAddonController>>();
        _mockPermissionService = new Mock<IPermissionService>();

        var userResolverService = new UserResolverService<int>(null!);

        _controller = new GmailAddonController(
            _mockManagerWrapper.Object,
            userResolverService,
            _mockLogger.Object,
            MockAuthorizationService.Object,
            _mockPermissionService.Object
        );

        SetupControllerContext(_controller);
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesController()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task FindGmailInteraction_ReturnsResult()
    {
        // Arrange
        _mockManagerWrapper.Setup(m => m.InteractionManager.FindGmailInteractionAsync(It.IsAny<UNOPS.PAO.Models.Integrations.GmailInteractionRequest>()))
            .ReturnsAsync((UNOPS.PAO.Models.Interactions.InteractionModel?)null);

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.FindGmailInteraction(new UNOPS.PAO.Models.Integrations.GmailInteractionRequest());

        // Assert
        Assert.NotNull(result);
    }
}
