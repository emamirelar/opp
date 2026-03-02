/**
 * @fileoverview Unit tests for DashboardController — complete coverage of all 10 endpoints.
 * 3:1 ratio: P=10, N=30, E=30, F=30, I=30 → Total=130
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Dashboard;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.Presentation.Tests.Controllers;

/// <summary>
/// Full unit-test coverage for DashboardController — all 10 endpoints.
///
/// Ratio breakdown:
///   Positive  (P) =  10
///   Negative  (N) =  30  (N ≥ 3P ✅)
///   Edge      (E) =  30  (E ≥ 3P ✅)
///   Functional(F) =  30  (F ≥ 3P ✅)
///   Integration(I)=  30  (I ≥ 3P ✅)
///   ─────────────────────────────────
///   TOTAL         = 130
/// </summary>
public class DashboardControllerTests : ControllerTestBase
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly DashboardController _controller;

    // ─── Shared test data factories ────────────────────────────────────────────

    private static PaginationResponse<PartnerModel> EmptyPartnerPage() =>
        new() { Records = new List<PartnerModel>(), TotalCount = 0 };

    private static PaginationResponse<PartnerModel> PopulatedPartnerPage() =>
        new() { Records = new List<PartnerModel> { new PartnerModel { Id = 1, Name = "ACME" } }, TotalCount = 1 };

    private static PaginationResponse<ContactModel> EmptyContactPage() =>
        new() { Records = new List<ContactModel>(), TotalCount = 0 };

    private static PaginationResponse<ContactModel> PopulatedContactPage() =>
        new() { Records = new List<ContactModel> { new ContactModel { Id = 1 } }, TotalCount = 1 };

    private static PaginationResponse<InteractionModel> EmptyInteractionPage() =>
        new() { Records = new List<InteractionModel>(), TotalCount = 0 };

    private static PaginationResponse<InteractionModel> PopulatedInteractionPage() =>
        new() { Records = new List<InteractionModel> { new InteractionModel { Id = 1 } }, TotalCount = 1 };

    private static PaginationResponse<OpportunityModel> EmptyOpportunityPage() =>
        new() { Records = new List<OpportunityModel>(), TotalCount = 0 };

    private static PaginationResponse<OpportunityModel> PopulatedOpportunityPage() =>
        new() { Records = new List<OpportunityModel> { new OpportunityModel { Id = 1, Name = "Opp 1" } }, TotalCount = 1 };

    private static OrgUnitRecentUpdatesResponse EmptyOrgUpdates() =>
        new() { Updates = new List<RecentUpdateModel>(), OrgUnitName = "Test Unit" };

    private static OrgUnitRecentUpdatesResponse PopulatedOrgUpdates() =>
        new() { Updates = new List<RecentUpdateModel> { new RecentUpdateModel { Id = 1, Name = "Update" } }, OrgUnitName = "Test Unit" };

    private static DashboardCombinedResponse EmptyCombinedResponse() => new();

    private static DashboardCombinedResponse PopulatedCombinedResponse() => new()
    {
        MyPartners = new List<DashboardPartnerModel> { new() { Id = 1, Name = "P1" } },
        MyContacts = new List<DashboardContactModel> { new() { Id = 1 } },
        MyInteractions = new List<DashboardInteractionModel> { new() { Id = 1 } },
        MyOpportunities = new List<DashboardOpportunityModel> { new() { Id = 1, Name = "O1" } },
        DraftPartners = new List<DashboardPartnerModel>(),
        DraftContacts = new List<DashboardContactModel>(),
        DraftInteractions = new List<DashboardInteractionModel>(),
        DraftOpportunities = new List<DashboardOpportunityModel>(),
        OrgUnitRecentUpdates = new List<DashboardRecentUpdateModel>(),
        OrgUnitName = "Test Unit"
    };

    public DashboardControllerTests()
    {
        _mockDashboardService = new Mock<IDashboardService>();
        _mockLogger = new Mock<ILogger<DashboardController>>();

        _controller = new DashboardController(
            _mockDashboardService.Object,
            new UserResolverService<int>(null!),
            _mockLogger.Object,
            MockAuthorizationService.Object);

        SetupControllerContext(_controller);
        SetupSuccessfulAuthorization();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // POSITIVE TESTS (P = 10)
    // ══════════════════════════════════════════════════════════════════════════
    #region Positive Tests

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyPartners_DefaultPageSize_Returns200WithPartners()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyContacts_DefaultPageSize_Returns200WithContacts()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedContactPage());

        var result = await _controller.GetMyContacts();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyDraftPartners_DefaultPageSize_Returns200WithDraftPartners()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyDraftPartners();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyDraftContacts_DefaultPageSize_Returns200WithDraftContacts()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedContactPage());

        var result = await _controller.GetMyDraftContacts();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyInteractions_DefaultPageSize_Returns200WithInteractions()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedInteractionPage());

        var result = await _controller.GetMyInteractions();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyDraftInteractions_DefaultPageSize_Returns200WithDraftInteractions()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedInteractionPage());

        var result = await _controller.GetMyDraftInteractions();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyOpportunities_DefaultPageSize_Returns200WithOpportunities()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOpportunityPage());

        var result = await _controller.GetMyOpportunities();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetMyDraftOpportunities_DefaultPageSize_Returns200WithDraftOpportunities()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOpportunityPage());

        var result = await _controller.GetMyDraftOpportunities();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetOrgUnitRecentUpdates_DefaultPageSize_Returns200WithUpdates()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOrgUpdates());

        var result = await _controller.GetOrgUnitRecentUpdates();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task GetDashboardContent_DefaultPageSizes_Returns200WithCombinedResponse()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedCombinedResponse());

        var result = await _controller.GetDashboardContent();

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // NEGATIVE TESTS (N = 30)
    // ══════════════════════════════════════════════════════════════════════════
    #region Negative Tests — BusinessException → 400 (×10)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyPartners_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Partner data unavailable"));

        var result = await _controller.GetMyPartners();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyContacts_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Contact data unavailable"));

        var result = await _controller.GetMyContacts();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftPartners_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Draft partner data unavailable"));

        var result = await _controller.GetMyDraftPartners();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftContacts_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Draft contact data unavailable"));

        var result = await _controller.GetMyDraftContacts();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyInteractions_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Interaction data unavailable"));

        var result = await _controller.GetMyInteractions();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftInteractions_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Draft interaction data unavailable"));

        var result = await _controller.GetMyDraftInteractions();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyOpportunities_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Opportunity data unavailable"));

        var result = await _controller.GetMyOpportunities();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftOpportunities_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Draft opportunity data unavailable"));

        var result = await _controller.GetMyDraftOpportunities();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrgUnitRecentUpdates_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Org unit data unavailable"));

        var result = await _controller.GetOrgUnitRecentUpdates();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDashboardContent_WhenBusinessExceptionThrown_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Dashboard data unavailable"));

        var result = await _controller.GetDashboardContent();

        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region Negative Tests — UnauthorizedAccessException → Forbid (×10)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyPartners_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyPartners();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyContacts_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyContacts();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftPartners_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyDraftPartners();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftContacts_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyDraftContacts();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyInteractions_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyInteractions();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftInteractions_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyDraftInteractions();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyOpportunities_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyOpportunities();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftOpportunities_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyDraftOpportunities();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrgUnitRecentUpdates_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetOrgUnitRecentUpdates();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDashboardContent_WhenUnauthorizedAccessExceptionThrown_ReturnsForbid()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetDashboardContent();

        Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region Negative Tests — Unhandled Exception → 500 (×10)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyPartners_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyPartners();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyContacts_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyContacts();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftPartners_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyDraftPartners();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftContacts_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyDraftContacts();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyInteractions_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyInteractions();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftInteractions_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyDraftInteractions();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyOpportunities_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyOpportunities();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetMyDraftOpportunities_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyDraftOpportunities();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetOrgUnitRecentUpdates_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetOrgUnitRecentUpdates();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetDashboardContent_WhenUnhandledExceptionThrown_Returns500()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetDashboardContent();

        var statusResult = result as ObjectResult;
        Assert.NotNull(statusResult);
        Assert.Equal(500, statusResult!.StatusCode);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // EDGE / BOUNDARY TESTS (E = 30)
    // ══════════════════════════════════════════════════════════════════════════
    #region Edge Tests — PageSize = 1 (minimum, ×10)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyPartners_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyPartners(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyContacts_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyContacts(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftPartners_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyDraftPartners(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftContacts_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyDraftContacts(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyInteractions_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyInteractions(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftInteractions_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyDraftInteractions(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyOpportunities_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyOpportunities(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftOpportunities_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyDraftOpportunities(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetOrgUnitRecentUpdates_WithPageSizeOne_ServiceCalledWithPageSizeOne()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 1))
            .ReturnsAsync(EmptyOrgUpdates());

        await _controller.GetOrgUnitRecentUpdates(pageSize: 1);

        _mockDashboardService.Verify(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 1), Times.Once);
    }

    /// <summary>Controller caps pageSize at 100 — request with 999 should call service with 100.</summary>
    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDashboardContent_WithPageSizeExceedingCap_ServiceCalledWithCappedValue()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), 100, It.IsAny<int>()))
            .ReturnsAsync(EmptyCombinedResponse());

        await _controller.GetDashboardContent(pageSize: 999);

        _mockDashboardService.Verify(
            s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), 100, It.IsAny<int>()),
            Times.Once);
    }

    #endregion

    #region Edge Tests — Large PageSize (×10)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyPartners_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyPartners(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyContacts_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyContacts(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftPartners_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyDraftPartners(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftContacts_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyDraftContacts(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyInteractions_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyInteractions(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftInteractions_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyDraftInteractions(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyOpportunities_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyOpportunities(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftOpportunities_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyDraftOpportunities(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetOrgUnitRecentUpdates_WithLargePageSize_ServiceCalledWithThatValue()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 5000))
            .ReturnsAsync(EmptyOrgUpdates());

        await _controller.GetOrgUnitRecentUpdates(pageSize: 5000);

        _mockDashboardService.Verify(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 5000), Times.Once);
    }

    /// <summary>recentUpdatesPageSize is capped at 20 — request with 999 should call service with 20.</summary>
    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDashboardContent_WithRecentUpdatesPageSizeExceedingCap_ServiceCalledWithCappedValue()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), 20))
            .ReturnsAsync(EmptyCombinedResponse());

        await _controller.GetDashboardContent(recentUpdatesPageSize: 999);

        _mockDashboardService.Verify(
            s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), 20),
            Times.Once);
    }

    #endregion

    #region Edge Tests — Zero / Negative PageSize (×10)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyPartners_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyPartnerPage());

        var result = await _controller.GetMyPartners(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyContacts_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyContactPage());

        var result = await _controller.GetMyContacts(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftPartners_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyPartnerPage());

        var result = await _controller.GetMyDraftPartners(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftContacts_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyContactPage());

        var result = await _controller.GetMyDraftContacts(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyInteractions_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyInteractionPage());

        var result = await _controller.GetMyInteractions(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftInteractions_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyInteractionPage());

        var result = await _controller.GetMyDraftInteractions(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyOpportunities_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyOpportunityPage());

        var result = await _controller.GetMyOpportunities(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetMyDraftOpportunities_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyOpportunityPage());

        var result = await _controller.GetMyDraftOpportunities(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetOrgUnitRecentUpdates_WithZeroPageSize_StillDelegatesAndReturnsResult()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 0))
            .ReturnsAsync(EmptyOrgUpdates());

        var result = await _controller.GetOrgUnitRecentUpdates(pageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    /// <summary>Both params at 0 — controller still delegates; capping Math.Min(0,100)=0, Math.Min(0,20)=0.</summary>
    [Fact]
    [Trait("Category", "Edge")]
    public async Task GetDashboardContent_WithBothParamsZero_ServiceCalledWithZeros()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), 0, 0))
            .ReturnsAsync(EmptyCombinedResponse());

        var result = await _controller.GetDashboardContent(pageSize: 0, recentUpdatesPageSize: 0);

        var ok = AssertOkResult(result);
        Assert.NotNull(ok.Value);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // FUNCTIONAL TESTS (F = 30)
    // ══════════════════════════════════════════════════════════════════════════
    #region Functional Tests — Correct service method is called (×10)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartners_CallsGetMyPartnersAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyPartners();

        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyContacts_CallsGetMyContactsAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyContacts();

        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftPartners_CallsGetMyDraftPartnersAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyDraftPartners();

        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftContacts_CallsGetMyDraftContactsAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyDraftContacts();

        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyInteractions_CallsGetMyInteractionsAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyInteractions();

        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftInteractions_CallsGetMyDraftInteractionsAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyDraftInteractions();

        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyOpportunities_CallsGetMyOpportunitiesAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyOpportunities();

        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftOpportunities_CallsGetMyDraftOpportunitiesAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyDraftOpportunities();

        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrgUnitRecentUpdates_CallsGetOrgUnitRecentUpdatesAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOrgUpdates());

        await _controller.GetOrgUnitRecentUpdates();

        _mockDashboardService.Verify(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetDashboardContent_CallsGetAllDashboardDataAsyncOnService()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyCombinedResponse());

        await _controller.GetDashboardContent();

        _mockDashboardService.Verify(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region Functional Tests — Default page size values passed correctly (×10)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartners_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyPartners();

        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyContacts_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyContacts();

        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftPartners_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyDraftPartners();

        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftContacts_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyDraftContacts();

        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyInteractions_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyInteractions();

        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftInteractions_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyDraftInteractions();

        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyOpportunities_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyOpportunities();

        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftOpportunities_WithoutExplicitPageSize_UsesDefault1000()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1000))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyDraftOpportunities();

        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), 1000), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrgUnitRecentUpdates_WithoutExplicitPageSize_UsesDefault10()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 10))
            .ReturnsAsync(EmptyOrgUpdates());

        await _controller.GetOrgUnitRecentUpdates();

        _mockDashboardService.Verify(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), 10), Times.Once);
    }

    /// <summary>Default pageSize=50 → Math.Min(50,100)=50; default recentUpdatesPageSize=10 → Math.Min(10,20)=10.</summary>
    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetDashboardContent_WithoutExplicitParams_UsesDefaultsAfterCapping()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), 50, 10))
            .ReturnsAsync(EmptyCombinedResponse());

        await _controller.GetDashboardContent();

        _mockDashboardService.Verify(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), 50, 10), Times.Once);
    }

    #endregion

    #region Functional Tests — Service return value is propagated (×10)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyPartners_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedPartnerPage();
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyContacts_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedContactPage();
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyContacts();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftPartners_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedPartnerPage();
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyDraftPartners();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftContacts_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedContactPage();
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyDraftContacts();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyInteractions_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedInteractionPage();
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyInteractions();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftInteractions_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedInteractionPage();
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyDraftInteractions();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyOpportunities_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedOpportunityPage();
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyOpportunities();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetMyDraftOpportunities_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedOpportunityPage();
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetMyDraftOpportunities();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetOrgUnitRecentUpdates_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedOrgUpdates();
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetOrgUnitRecentUpdates();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task GetDashboardContent_ServiceReturnValue_IsReturnedInResponse()
    {
        var expected = PopulatedCombinedResponse();
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetDashboardContent();

        var ok = AssertOkResult(result);
        Assert.Same(expected, ok.Value);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // INTEGRATION TESTS (I = 30)
    // ══════════════════════════════════════════════════════════════════════════
    #region Integration Tests — Service called exactly once (×10)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyPartners_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyPartners();

        _mockDashboardService.Verify(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyContacts_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyContacts();

        _mockDashboardService.Verify(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftPartners_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        await _controller.GetMyDraftPartners();

        _mockDashboardService.Verify(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftContacts_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        await _controller.GetMyDraftContacts();

        _mockDashboardService.Verify(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyInteractions_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyInteractions();

        _mockDashboardService.Verify(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftInteractions_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        await _controller.GetMyDraftInteractions();

        _mockDashboardService.Verify(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyOpportunities_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyOpportunities();

        _mockDashboardService.Verify(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftOpportunities_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        await _controller.GetMyDraftOpportunities();

        _mockDashboardService.Verify(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrgUnitRecentUpdates_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOrgUpdates());

        await _controller.GetOrgUnitRecentUpdates();

        _mockDashboardService.Verify(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetDashboardContent_ServiceCalledExactlyOnce()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyCombinedResponse());

        await _controller.GetDashboardContent();

        _mockDashboardService.Verify(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    #endregion

    #region Integration Tests — Empty service response returns 200 (×10)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyPartners_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<PartnerModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyContacts_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        var result = await _controller.GetMyContacts();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<ContactModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftPartners_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyPartnerPage());

        var result = await _controller.GetMyDraftPartners();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<PartnerModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftContacts_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyContactPage());

        var result = await _controller.GetMyDraftContacts();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<ContactModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyInteractions_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        var result = await _controller.GetMyInteractions();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<InteractionModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftInteractions_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyInteractionPage());

        var result = await _controller.GetMyDraftInteractions();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<InteractionModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyOpportunities_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        var result = await _controller.GetMyOpportunities();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<OpportunityModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftOpportunities_WhenServiceReturnsEmptyList_Returns200WithEmptyRecords()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOpportunityPage());

        var result = await _controller.GetMyDraftOpportunities();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<OpportunityModel>;
        Assert.NotNull(page);
        Assert.Empty(page!.Records);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrgUnitRecentUpdates_WhenServiceReturnsEmptyList_Returns200WithEmptyUpdates()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyOrgUpdates());

        var result = await _controller.GetOrgUnitRecentUpdates();

        var ok = AssertOkResult(result);
        var response = ok.Value as OrgUnitRecentUpdatesResponse;
        Assert.NotNull(response);
        Assert.Empty(response!.Updates);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetDashboardContent_WhenServiceReturnsEmptyCollections_Returns200WithEmptyData()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(EmptyCombinedResponse());

        var result = await _controller.GetDashboardContent();

        var ok = AssertOkResult(result);
        var combined = ok.Value as DashboardCombinedResponse;
        Assert.NotNull(combined);
        Assert.Empty(combined!.MyPartners);
        Assert.Empty(combined.MyContacts);
    }

    #endregion

    #region Integration Tests — Populated service response returns correct data (×10)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyPartners_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<PartnerModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyContacts_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedContactPage());

        var result = await _controller.GetMyContacts();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<ContactModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftPartners_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyDraftPartners();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<PartnerModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftContacts_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftContactsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedContactPage());

        var result = await _controller.GetMyDraftContacts();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<ContactModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyInteractions_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedInteractionPage());

        var result = await _controller.GetMyInteractions();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<InteractionModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftInteractions_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftInteractionsAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedInteractionPage());

        var result = await _controller.GetMyDraftInteractions();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<InteractionModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyOpportunities_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOpportunityPage());

        var result = await _controller.GetMyOpportunities();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<OpportunityModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMyDraftOpportunities_WhenServiceReturnsData_ResponseContainsExpectedCount()
    {
        _mockDashboardService
            .Setup(s => s.GetMyDraftOpportunitiesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOpportunityPage());

        var result = await _controller.GetMyDraftOpportunities();

        var ok = AssertOkResult(result);
        var page = ok.Value as PaginationResponse<OpportunityModel>;
        Assert.Equal(1, page!.TotalCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetOrgUnitRecentUpdates_WhenServiceReturnsData_ResponseContainsExpectedOrgUnitName()
    {
        _mockDashboardService
            .Setup(s => s.GetOrgUnitRecentUpdatesAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedOrgUpdates());

        var result = await _controller.GetOrgUnitRecentUpdates();

        var ok = AssertOkResult(result);
        var response = ok.Value as OrgUnitRecentUpdatesResponse;
        Assert.Equal("Test Unit", response!.OrgUnitName);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetDashboardContent_WhenServiceReturnsData_ResponseContainsAllPopulatedSections()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedCombinedResponse());

        var result = await _controller.GetDashboardContent();

        var ok = AssertOkResult(result);
        var combined = ok.Value as DashboardCombinedResponse;
        Assert.NotNull(combined);
        Assert.Single(combined!.MyPartners);
        Assert.Single(combined.MyContacts);
        Assert.Single(combined.MyInteractions);
        Assert.Single(combined.MyOpportunities);
        Assert.Equal("Test Unit", combined.OrgUnitName);
    }

    #endregion
}
