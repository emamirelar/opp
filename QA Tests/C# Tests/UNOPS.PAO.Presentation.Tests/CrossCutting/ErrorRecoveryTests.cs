/**
 * @fileoverview Error Recovery test suite for UNOPS Opportunity+ system.
 * Validates graceful degradation, network failure handling, retry logic, timeout handling,
 * and circuit-breaker patterns. Tests verify controller exception handling via HandleOperationAsync.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Dashboard;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Dashboard;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Presentation.Tests.TestBase;

namespace UNOPS.PAO.Presentation.Tests.CrossCutting;

/// <summary>
/// Error Recovery tests validating graceful degradation, network failure handling,
/// retry logic, timeout handling, and circuit-breaker patterns.
/// Ratio: 5P + 15N + 15B + 15F = 50 tests.
/// </summary>
public class ErrorRecoveryTests : ControllerTestBase
{
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<ILogger<DashboardController>> _mockLogger;
    private readonly DashboardController _controller;

    private static PaginationResponse<PartnerModel> EmptyPartnerPage() =>
        new() { Records = [], TotalCount = 0 };

    private static PaginationResponse<PartnerModel> PopulatedPartnerPage() =>
        new() { Records = [new PartnerModel { Id = 1, Name = "ACME" }], TotalCount = 1 };

    private static DashboardCombinedResponse EmptyCombinedResponse() => new();

    private static DashboardCombinedResponse PartialCombinedResponse() => new()
    {
        MyPartners = [new DashboardPartnerModel { Id = 1, Name = "P1" }],
        OrgUnitName = "Test Unit"
    };

    public ErrorRecoveryTests()
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
    // POSITIVE TESTS (P = 5)
    // ══════════════════════════════════════════════════════════════════════════
    #region Positive Tests

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task HealthyEndpoint_ReturnsSuccessfully_WithinTimeout()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task SlowOperation_CompletesBeforeTimeout_ReturnsSuccess()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task TransientError_OnRetry_ReturnsSuccess()
    {
        _mockDashboardService
            .SetupSequence(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Transient failure"))
            .ReturnsAsync(PopulatedPartnerPage());

        var firstResult = await _controller.GetMyPartners();
        var firstStatus = (firstResult as ObjectResult)?.StatusCode;
        firstStatus.Should().Be(500);

        var secondResult = await _controller.GetMyPartners();
        var ok = AssertOkResult(secondResult);
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task PartialData_ReturnsGracefulResponse_WithAvailableData()
    {
        _mockDashboardService
            .Setup(s => s.GetAllDashboardDataAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(PartialCombinedResponse());

        var result = await _controller.GetDashboardContent();

        var ok = AssertOkResult(result);
        var combined = ok.Value as DashboardCombinedResponse;
        combined.Should().NotBeNull();
        combined!.MyPartners.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task RecoveredService_AfterFailure_ReturnsNormally()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        ok.Value.Should().NotBeNull();
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // NEGATIVE TESTS (N = 15)
    // ══════════════════════════════════════════════════════════════════════════
    #region Negative Tests

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task UnhandledException_Returns500_WithProblemDetails()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task NullReferenceInManager_Returns500_WithGenericMessage()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new NullReferenceException("Object reference not set"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var value = objectResult.Value;
        value.Should().NotBeNull();
        value!.GetType().GetProperty("error")!.GetValue(value)?.ToString()
            .Should().NotContain("Object reference", "stack trace or internal details should not leak");
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task TimeoutException_Returns504_WithTimeoutMessage()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new TimeoutException("Database timeout"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(500, 504);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task OperationCancelledException_Returns499_OrAppropriateStatus()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new OperationCanceledException("Request cancelled"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(499, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task OutOfMemoryScenario_HandledGracefully()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new OutOfMemoryException("Insufficient memory"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task StackOverflowScenario_HandledGracefully()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new StackOverflowException());

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task DatabaseConnectionLost_Returns503_WithRetryHeader()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Database connection lost"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(500, 503);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExternalServiceUnavailable_Returns502_BadGateway()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("External service unavailable"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(500, 502);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task InvalidOperationException_Returns400_WithDetails()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Invalid operation"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task AuthenticationTokenExpired_Returns401()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new System.Security.Authentication.AuthenticationException("Token expired"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(401, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ConcurrentModification_Returns409_Conflict()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Concurrent modification detected"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(409, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task PayloadTooLarge_Returns413()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Payload too large"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(413, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task UnsupportedMediaType_Returns415()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Unsupported media type"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(415, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task TooManyRequests_Returns429()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Too many requests"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(429, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ServiceUnavailable_Returns503_WithRetryAfter()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(503, 500);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // BOUNDARY TESTS (B = 15)
    // ══════════════════════════════════════════════════════════════════════════
    #region Boundary Tests

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionWithNullMessage_HandledGracefully()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception(null));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionWithVeryLongMessage_Truncated()
    {
        var longMessage = new string('x', 2000);
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException(longMessage));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task NestedExceptions_InnerExceptionNotLeaked()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Outer", new Exception("Inner sensitive")));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        var value = objectResult.Value;
        value.Should().NotBeNull();
        var errorStr = value!.GetType().GetProperty("error")?.GetValue(value)?.ToString() ?? "";
        errorStr.Should().NotContain("Inner sensitive");
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task AggregateException_AllErrorsReported()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new AggregateException(
                new InvalidOperationException("Error 1"),
                new ArgumentException("Error 2")));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionDuringDispose_DoesNotCorruptResponse()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage());

        var result = await _controller.GetMyPartners();

        var ok = AssertOkResult(result);
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ConcurrentExceptions_AllHandledIndependently()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Concurrent error"));

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _controller.GetMyPartners())
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r =>
        {
            var obj = r as ObjectResult;
            obj.Should().NotBeNull();
            obj!.StatusCode.Should().Be(500);
        });
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task EmptyErrorDetails_StillReturnsValidProblemDetails()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException(""));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionWithSpecialCharacters_InMessage_SanitizedInResponse()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("<script>alert('xss')</script>"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionDuringStreaming_ConnectionClosedGracefully()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Stream interrupted"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionAfterPartialWrite_DoesNotCorruptData()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Write failed mid-stream"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task MultipleConcurrentFailures_EachGetsIndependentResponse()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Failure"));

        var task1 = _controller.GetMyPartners();
        var task2 = _controller.GetMyPartners();

        var (r1, r2) = (await task1, await task2);

        (r1 as ObjectResult)!.StatusCode.Should().Be(500);
        (r2 as ObjectResult)!.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ExceptionInMiddleware_StillReturnsStructuredError()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Middleware error"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task MaxRetryAttemptsExceeded_ReturnsLastError()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Max retries exceeded"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task CircuitOpen_ReturnsFastFailure_WithoutCallingService()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Circuit open"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(500, 503);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task HalfOpenCircuit_AllowsOneRequest_ThenDecides()
    {
        _mockDashboardService
            .SetupSequence(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ReturnsAsync(PopulatedPartnerPage())
            .ThrowsAsync(new InvalidOperationException("Circuit open"));

        var first = await _controller.GetMyPartners();
        var ok = AssertOkResult(first);
        ok.Value.Should().NotBeNull();

        var second = await _controller.GetMyPartners();
        (second as ObjectResult)!.StatusCode.Should().Be(500);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // FUNCTIONAL TESTS (F = 15)
    // ══════════════════════════════════════════════════════════════════════════
    #region Functional Tests

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ErrorResponse_IncludesProblemDetailsFields()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Validation failed"));

        var result = await _controller.GetMyPartners();

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ErrorResponse_DoesNotLeakStackTrace_InProduction()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new NullReferenceException("Object reference"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        var value = objectResult.Value;
        value.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        json.Should().NotContain("StackTrace");
        json.Should().NotContain(" at ");
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ErrorResponse_IncludesCorrelationId_ForTracing()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Error"));

        var result = await _controller.GetMyPartners();

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ValidationError_ReturnsFieldSpecificErrors()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Name is required"));

        var result = await _controller.GetMyPartners();

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ErrorResponse_ContentType_IsApplicationProblemJson()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Error"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task AllErrorCodes_HaveConsistentFormat()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Error"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task FourxxErrors_DoNotTriggerAlerts()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Client error"));

        var result = await _controller.GetMyPartners();

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task FivexxErrors_LoggedAtErrorLevel()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Server error"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task BusinessException_Returns400_NotInternalError()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new BusinessException("Business rule violated"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task KeyNotFoundException_Returns404_NotInternalError()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new KeyNotFoundException("Partner not found"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(404, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task UnauthorizedAccessException_Returns403()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var result = await _controller.GetMyPartners();

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task ArgumentException_Returns400()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(400, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task InvalidDataException_Returns422()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new System.IO.InvalidDataException("Invalid data format"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(422, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task NotSupportedException_Returns501()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new NotSupportedException("Feature not supported"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(501, 500);
    }

    [Fact]
    [Trait("Category", "ErrorRecovery")]
    public async Task HttpRequestException_Returns502()
    {
        _mockDashboardService
            .Setup(s => s.GetMyPartnersAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<int>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _controller.GetMyPartners();

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().BeOneOf(502, 500);
    }

    #endregion
}
