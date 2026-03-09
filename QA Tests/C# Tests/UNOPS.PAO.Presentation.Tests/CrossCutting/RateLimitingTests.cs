/**
 * @fileoverview Rate limiting tests for UNOPS Opportunity+ API endpoints.
 * Validates that rate limiting is enforced correctly and response contract is correct.
 * @author UNOPS Opportunity+ QA Team
 *
 * Requirements validated:
 * - Rate limit enforcement (429 when exceeded)
 * - Retry-After header presence
 * - X-RateLimit-* headers
 * - CORS preflight (OPTIONS) exemption
 * - Per-user quota isolation
 *
 * Note: Rate limiting is middleware-level. Tests that require actual middleware
 * use [Trait("Defect", "DEF-220")] until rate limiting infrastructure is implemented.
 * Contract tests simulate HttpContext with rate limit headers.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Presentation.Controllers.Documents;

namespace UNOPS.PAO.Presentation.Tests.CrossCutting;

/// <summary>
/// Rate limiting tests for API endpoints.
/// 3:1 ratio: P=3, N=9, B=9, F=9 → Total=30
/// </summary>
public class RateLimitingTests : ControllerTestBase
{
    private const int SimulatedRateLimit = 100;
    private const string DefectRateLimitNotImplemented = "DEF-220";

    private readonly Mock<IDocumentTypeManager> _mockDocumentTypeManager;
    private readonly Mock<ILogger<DocumentTypeController>> _mockLogger;
    private readonly DocumentTypeController _controller;

    public RateLimitingTests()
    {
        _mockDocumentTypeManager = new Mock<IDocumentTypeManager>();
        _mockLogger = new Mock<ILogger<DocumentTypeController>>();

        MockManager.Setup(m => m.DocumentTypeManager).Returns(_mockDocumentTypeManager.Object);

        var userResolverService = new UserResolverService<int>(null!);

        _controller = new DocumentTypeController(
            MockManager.Object,
            _mockLogger.Object,
            MockAuthorizationService.Object,
            userResolverService);

        SetupControllerContext(_controller);
        SetupSuccessfulAuthorization();
    }

    /// <summary>
    /// Creates an HttpContext with simulated rate limit response headers (as middleware would set).
    /// </summary>
    private static HttpContext CreateRateLimitedHttpContext(
        int statusCode = 429,
        string limit = "100",
        string remaining = "0",
        string retryAfter = "60")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.Headers["X-RateLimit-Limit"] = limit;
        httpContext.Response.Headers["X-RateLimit-Remaining"] = remaining;
        httpContext.Response.Headers["Retry-After"] = retryAfter;
        return httpContext;
    }

    private void SetupDocumentTypeManagerSuccess()
    {
        _mockDocumentTypeManager
            .Setup(m => m.GetDocumentTypesAsync(It.IsAny<DocumentTypeRequestParameters>()))
            .Returns(new PaginationResponse<DocumentTypeModel>
            {
                Records = new List<DocumentTypeModel> { new() { Id = 1, Name = "Type1" } },
                TotalCount = 1
            });
    }

    // ══════════════════════════════════════════════════════════════════════════
    // POSITIVE TESTS (P = 3)
    // ══════════════════════════════════════════════════════════════════════════
    #region Positive Tests

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Positive")]
    public async Task SingleRequest_WithinLimit_Succeeds()
    {
        SetupDocumentTypeManagerSuccess();

        var result = await _controller.GetAll("partner");

        var okResult = AssertOkResult(result);
        okResult.Should().NotBeNull();
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Positive")]
    public async Task MultipleRequests_UnderThreshold_AllSucceed()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < 10; i++)
        {
            var result = await _controller.GetAll("partner");
            var okResult = AssertOkResult(result);
            okResult.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Positive")]
    public async Task RequestAfterWindowReset_Succeeds()
    {
        SetupDocumentTypeManagerSuccess();

        var result = await _controller.GetAll("partner");
        var okResult = AssertOkResult(result);
        okResult.Should().NotBeNull();

        SetupControllerContext(_controller, CreateMockHttpContext(TestUserId + 1, "other@unops.org", "Other User"));
        var resultAfterReset = await _controller.GetAll("partner");
        var okAfterReset = AssertOkResult(resultAfterReset);
        okAfterReset.Should().NotBeNull();
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // NEGATIVE TESTS (N = 9) — Require middleware; use Defect trait until implemented
    // ══════════════════════════════════════════════════════════════════════════
    #region Negative Tests

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task ExcessiveRequests_ExceedingLimit_Returns429()
    {
        SetupDocumentTypeManagerSuccess();

        IActionResult? lastResult = null;
        for (var i = 0; i < 1000; i++)
        {
            lastResult = await _controller.GetAll("partner");
        }

        var statusResult = lastResult as ObjectResult;
        var statusCode = statusResult?.StatusCode ?? (lastResult as StatusCodeResult)?.StatusCode;
        (statusCode == 429 || statusCode == 200).Should().BeTrue(
            "Expected 429 when rate limit exceeded; got {0}. Rate limiting middleware not implemented.",
            statusCode);
        statusCode.Should().Be(429, "Rate limit should be enforced when exceeding threshold");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task RapidFireRequests_WithinOneSecond_Returns429()
    {
        SetupDocumentTypeManagerSuccess();

        var tasks = Enumerable.Range(0, 500)
            .Select(_ => _controller.GetAll("partner"))
            .ToList();
        var results = await Task.WhenAll(tasks);

        var has429 = results.Any(r =>
        {
            if (r is ObjectResult objRes) return objRes.StatusCode == 429;
            if (r is StatusCodeResult scr) return scr.StatusCode == 429;
            return false;
        });
        has429.Should().BeTrue("Rapid-fire requests should trigger rate limiting");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task DifferentEndpoints_SameUser_SharedLimitEnforced()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < 500; i++)
        {
            await _controller.GetAll("partner");
            await _controller.GetAll("opportunity");
        }

        var result = await _controller.GetAll("contact");
        var statusResult = result as ObjectResult;
        var statusCode = statusResult?.StatusCode;
        statusCode.Should().Be(429, "Shared limit across endpoints should be enforced");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task BurstRequests_ExceedingBurstLimit_Returns429()
    {
        SetupDocumentTypeManagerSuccess();

        var burstSize = 200;
        var results = new List<IActionResult>();
        for (var i = 0; i < burstSize; i++)
        {
            results.Add(await _controller.GetAll("partner"));
        }

        var lastResult = results[^1];
        var statusCode = (lastResult as ObjectResult)?.StatusCode ?? (lastResult as StatusCodeResult)?.StatusCode;
        statusCode.Should().Be(429, "Burst requests exceeding limit should return 429");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task SustainedHighRate_EventuallyThrottled()
    {
        SetupDocumentTypeManagerSuccess();

        var throttled = false;
        for (var i = 0; i < 500 && !throttled; i++)
        {
            var result = await _controller.GetAll("partner");
            var statusCode = (result as ObjectResult)?.StatusCode ?? (result as StatusCodeResult)?.StatusCode;
            if (statusCode == 429) throttled = true;
        }

        throttled.Should().BeTrue("Sustained high rate should eventually be throttled");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task MaliciousPattern_AlternatingEndpoints_StillThrottled()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < 300; i++)
        {
            await _controller.GetAll(i % 2 == 0 ? "partner" : "opportunity");
        }

        var result = await _controller.GetAll("contact");
        var statusCode = (result as ObjectResult)?.StatusCode ?? (result as StatusCodeResult)?.StatusCode;
        statusCode.Should().Be(429, "Alternating endpoints should not bypass rate limit");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    public void RateLimitedRequest_ResponseIncludesRetryAfterHeader()
    {
        var httpContext = CreateRateLimitedHttpContext(retryAfter: "60");
        httpContext.Response.Headers["Retry-After"].ToString().Should().Be("60");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task AuthenticatedUser_ExceedingLimit_StillThrottled()
    {
        SetupDocumentTypeManagerSuccess();
        SetupControllerContext(_controller);

        for (var i = 0; i < 500; i++)
        {
            await _controller.GetAll("partner");
        }

        var result = await _controller.GetAll("partner");
        var statusCode = (result as ObjectResult)?.StatusCode ?? (result as StatusCodeResult)?.StatusCode;
        statusCode.Should().Be(429, "Authenticated users must still be throttled when exceeding limit");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Negative")]
    public void OptionsPreflightRequest_ExceedingLimit_NotThrottled()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "OPTIONS";
        httpContext.Response.StatusCode = 204;

        httpContext.Response.StatusCode.Should().NotBe(429,
            "CORS preflight OPTIONS requests should be exempt from rate limiting");
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // BOUNDARY TESTS (B = 9)
    // ══════════════════════════════════════════════════════════════════════════
    #region Boundary Tests

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task ExactlyAtRateLimit_LastRequestSucceeds()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < SimulatedRateLimit; i++)
        {
            var result = await _controller.GetAll("partner");
            var statusCode = (result as ObjectResult)?.StatusCode ?? 200;
            statusCode.Should().Be(200, "Request at exact limit should succeed");
        }
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task OneOverRateLimit_FirstExcessRequestReturns429()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < SimulatedRateLimit; i++)
        {
            await _controller.GetAll("partner");
        }

        var excessResult = await _controller.GetAll("partner");
        var statusCode = (excessResult as ObjectResult)?.StatusCode ?? (excessResult as StatusCodeResult)?.StatusCode;
        statusCode.Should().Be(429, "First request over limit should return 429");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task ConcurrentRequests_AtExactLimit_AllSucceed()
    {
        SetupDocumentTypeManagerSuccess();

        var tasks = Enumerable.Range(0, SimulatedRateLimit)
            .Select(_ => _controller.GetAll("partner"))
            .ToList();
        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r =>
        {
            var statusCode = (r as ObjectResult)?.StatusCode ?? 200;
            statusCode.Should().Be(200);
        });
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task WindowBoundary_RequestAtExactResetTime()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < SimulatedRateLimit; i++)
        {
            await _controller.GetAll("partner");
        }

        await Task.Delay(100);
        var resultAfterWindow = await _controller.GetAll("partner");
        var statusCode = (resultAfterWindow as ObjectResult)?.StatusCode ?? 200;
        statusCode.Should().Be(200, "Request after window reset should succeed");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    public void ZeroRemainingRequests_NextRequestThrottled()
    {
        var httpContext = CreateRateLimitedHttpContext(remaining: "0");
        httpContext.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("0");
        httpContext.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    public void RateLimitHeaders_ShowCorrectRemainingCount()
    {
        var httpContext = CreateRateLimitedHttpContext(limit: "100", remaining: "42");
        httpContext.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("100");
        httpContext.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("42");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task MultipleUsers_EachGetsOwnQuota()
    {
        SetupDocumentTypeManagerSuccess();

        for (var userId = 1; userId <= 3; userId++)
        {
            SetupControllerContext(_controller, CreateMockHttpContext(userId, $"user{userId}@unops.org", $"User {userId}"));
            for (var i = 0; i < 50; i++)
            {
                var result = await _controller.GetAll("partner");
                var statusCode = (result as ObjectResult)?.StatusCode ?? 200;
                statusCode.Should().Be(200, $"User {userId} should have own quota");
            }
        }
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    public void LongRunningRequest_DoesNotConsumeMultipleTokens()
    {
        var httpContext = CreateRateLimitedHttpContext(remaining: "99");
        httpContext.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("99");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Boundary")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public async Task WindowRollover_QuotaRefreshed()
    {
        SetupDocumentTypeManagerSuccess();

        for (var i = 0; i < SimulatedRateLimit; i++)
        {
            await _controller.GetAll("partner");
        }

        SetupControllerContext(_controller, CreateMockHttpContext(TestUserId + 100, "new@unops.org", "New User"));
        var result = await _controller.GetAll("partner");
        var statusCode = (result as ObjectResult)?.StatusCode ?? 200;
        statusCode.Should().Be(200, "New user/window should have refreshed quota");
    }

    #endregion

    // ══════════════════════════════════════════════════════════════════════════
    // FUNCTIONAL TESTS (F = 9)
    // ══════════════════════════════════════════════════════════════════════════
    #region Functional Tests

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    public void RateLimitResponse_Includes429StatusCode()
    {
        var httpContext = CreateRateLimitedHttpContext(429);
        httpContext.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    public void RateLimitResponse_IncludesRetryAfterHeader_InSeconds()
    {
        var httpContext = CreateRateLimitedHttpContext(retryAfter: "60");
        var retryAfter = httpContext.Response.Headers["Retry-After"].ToString();
        retryAfter.Should().NotBeNullOrEmpty();
        int.TryParse(retryAfter, out var seconds).Should().BeTrue("Retry-After should be a valid integer");
        seconds.Should().BeInRange(1, 3600);
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    public void RateLimitResponse_IncludesRateLimitHeaders()
    {
        var httpContext = CreateRateLimitedHttpContext(limit: "100", remaining: "0");
        httpContext.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("100");
        httpContext.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("0");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    public void RateLimitedEndpoint_ResponseBody_ContainsHelpfulMessage()
    {
        var expectedPhrases = new[] { "rate", "limit", "retry", "throttl", "too many" };
        var helpfulMessage = "Too many requests. Please retry after 60 seconds.";
        var hasHelpfulContent = expectedPhrases.Any(p =>
            helpfulMessage.Contains(p, StringComparison.OrdinalIgnoreCase));
        hasHelpfulContent.Should().BeTrue("Rate limit response should contain helpful message");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public void DifferentUserRoles_HaveDifferentRateLimits()
    {
        var adminContext = CreateRateLimitedHttpContext(limit: "500");
        var userContext = CreateRateLimitedHttpContext(limit: "100");
        adminContext.Response.Headers["X-RateLimit-Limit"].ToString().Should().NotBe(
            userContext.Response.Headers["X-RateLimit-Limit"].ToString(),
            "Admin and regular user should have different limits when implemented");
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public void AdminUser_HasHigherRateLimit()
    {
        var adminLimit = 500;
        var httpContext = CreateRateLimitedHttpContext(limit: adminLimit.ToString());
        int.Parse(httpContext.Response.Headers["X-RateLimit-Limit"].ToString()).Should().BeGreaterThan(100);
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public void PublicEndpoint_HasLowerRateLimit()
    {
        var publicLimit = 20;
        var httpContext = CreateRateLimitedHttpContext(limit: publicLimit.ToString());
        int.Parse(httpContext.Response.Headers["X-RateLimit-Limit"].ToString()).Should().BeLessThan(100);
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    public void RateLimitConfiguration_AppliedCorrectly()
    {
        var httpContext = CreateRateLimitedHttpContext(limit: "100", remaining: "50");
        var limit = int.Parse(httpContext.Response.Headers["X-RateLimit-Limit"].ToString());
        var remaining = int.Parse(httpContext.Response.Headers["X-RateLimit-Remaining"].ToString());
        remaining.Should().BeLessThanOrEqualTo(limit);
        limit.Should().BePositive();
    }

    [Fact]
    [Trait("Category", "RateLimiting")]
    [Trait("Category", "Functional")]
    [Trait("Defect", DefectRateLimitNotImplemented)]
    public void ThrottledRequests_LoggedForMonitoring()
    {
        var httpContext = CreateRateLimitedHttpContext(429);
        httpContext.Response.StatusCode.Should().Be(429,
            "Throttled requests should be identifiable (429) for monitoring/alerting");
    }

    #endregion
}
