/**
 * @fileoverview Unit tests for DashboardController
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Dashboard;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

public class DashboardControllerTests : ControllerTestBase
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _mockLogger = new Mock<ILogger<DashboardController>>();

        var userResolverService = new UserResolverService<int>(null!);

        _controller = new DashboardController(
            _mockDashboardService.Object,
            userResolverService,
            _mockLogger.Object,
            MockAuthorizationService.Object
        );

        SetupControllerContext(_controller);
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesController()
    {
        Assert.NotNull(_controller);
    }

    [Fact]
    public async Task GetMyPartners_ReturnsPartners()
    {
        // Arrange
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 });

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetMyPartners();

        // Assert
        var okResult = AssertOkResult(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMyContacts_ReturnsContacts()
    {
        // Arrange
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 });

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetMyContacts();

        // Assert
        var okResult = AssertOkResult(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMyDraftPartners_ReturnsDrafts()
    {
        // Arrange
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginationResponse<PartnerModel> { Records = new List<PartnerModel>(), TotalCount = 0 });

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetMyDraftPartners();

        // Assert
        var okResult = AssertOkResult(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMyDraftContacts_ReturnsDrafts()
    {
        // Arrange
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(new PaginationResponse<ContactModel> { Records = new List<ContactModel>(), TotalCount = 0 });

        SetupSuccessfulAuthorization();

        // Act
        var result = await _controller.GetMyDraftContacts();

        // Assert
        var okResult = AssertOkResult(result);
        Assert.NotNull(okResult.Value);
    }
}
