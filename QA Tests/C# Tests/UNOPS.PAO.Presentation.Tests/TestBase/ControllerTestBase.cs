/**
 * @fileoverview Base class for controller unit tests with common test infrastructure
 * @author UNOPS Opportunity+ QA Team
 */

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UNOPS.PAO.Presentation.Tests.TestBase;

/// <summary>
/// Base class for controller unit tests providing common mocking and setup functionality.
/// Follows AAA (Arrange-Act-Assert) testing pattern with pre-configured mocks.
/// </summary>
/// <remarks>
/// All controller tests should inherit from this class to ensure:
/// - Consistent mock setup across all controller tests
/// - Standardized HTTP context configuration
/// - Common assertion helpers for HTTP responses
/// - Reusable test data factories
/// </remarks>
public abstract class ControllerTestBase : IDisposable
{
    /// <summary>
    /// Mock manager wrapper providing access to all business managers
    /// </summary>
    protected readonly Mock<IManagerWrapper> MockManager;

    /// <summary>
    /// Mock authorization service for testing permission checks
    /// </summary>
    protected readonly Mock<IAuthorizationService> MockAuthorizationService;

    /// <summary>
    /// Mock AutoMapper for DTO/Entity conversions
    /// </summary>
    protected readonly Mock<IMapper> MockMapper;

    /// <summary>
    /// Test user ID for simulating authenticated user
    /// </summary>
    protected const int TestUserId = 1;

    /// <summary>
    /// Test user email for simulating authenticated user
    /// </summary>
    protected const string TestUserEmail = "test.user@unops.org";

    /// <summary>
    /// Test user name for simulating authenticated user
    /// </summary>
    protected const string TestUserName = "Test User";

    /// <summary>
    /// Initializes a new instance of the ControllerTestBase class with mock dependencies
    /// </summary>
    protected ControllerTestBase()
    {
        MockManager = new Mock<IManagerWrapper>();
        MockAuthorizationService = new Mock<IAuthorizationService>();
        MockMapper = new Mock<IMapper>();
    }

    /// <summary>
    /// Creates a mock HTTP context with authenticated user claims
    /// </summary>
    /// <param name="userId">User ID to include in claims (default: TestUserId)</param>
    /// <param name="email">User email to include in claims (default: TestUserEmail)</param>
    /// <param name="name">User name to include in claims (default: TestUserName)</param>
    /// <returns>Configured HttpContext with user claims</returns>
    protected HttpContext CreateMockHttpContext(
        int userId = TestUserId,
        string email = TestUserEmail,
        string name = TestUserName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        return httpContext;
    }

    /// <summary>
    /// Sets up the controller's ControllerContext with a mock HTTP context
    /// </summary>
    /// <param name="controller">Controller to configure</param>
    /// <param name="httpContext">Optional custom HttpContext (default: authenticated test user)</param>
    protected void SetupControllerContext(ControllerBase controller, HttpContext? httpContext = null)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext ?? CreateMockHttpContext()
        };
    }

    /// <summary>
    /// Sets up a successful authorization result for the mock authorization service
    /// </summary>
    protected void SetupSuccessfulAuthorization()
    {
        MockAuthorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        MockAuthorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
    }

    /// <summary>
    /// Sets up a failed authorization result for the mock authorization service
    /// </summary>
    /// <param name="failureReason">Optional reason for authorization failure</param>
    protected void SetupFailedAuthorization(string failureReason = "Unauthorized")
    {
        MockAuthorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        MockAuthorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Failed());
    }

    /// <summary>
    /// Asserts that an action result is an OkObjectResult or ObjectResult with 200 status code.
    /// Controllers using HandleOperationAsync may return ObjectResult instead of OkObjectResult.
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>ObjectResult for further assertions</returns>
    protected ObjectResult AssertOkResult(IActionResult result)
    {
        // HandleOperationAsync returns StatusCode(200, result) which is ObjectResult, not OkObjectResult
        var objectResult = result as ObjectResult;
        Assert.NotNull(objectResult);
        Assert.True(objectResult.StatusCode == 200 || objectResult.StatusCode == null, 
            $"Expected status code 200, but got {objectResult.StatusCode}");
        return objectResult;
    }

    /// <summary>
    /// Asserts that an action result is a CreatedAtActionResult with the expected status code
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>CreatedAtActionResult for further assertions</returns>
    protected CreatedAtActionResult AssertCreatedResult(IActionResult result)
    {
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        return createdResult;
    }

    /// <summary>
    /// Asserts that an action result is a NotFoundObjectResult with the expected status code
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>NotFoundObjectResult for further assertions</returns>
    protected NotFoundObjectResult AssertNotFoundResult(IActionResult result)
    {
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        return notFoundResult;
    }

    /// <summary>
    /// Asserts that an action result is a BadRequestObjectResult with the expected status code
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>BadRequestObjectResult for further assertions</returns>
    protected BadRequestObjectResult AssertBadRequestResult(IActionResult result)
    {
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        return badRequestResult;
    }

    /// <summary>
    /// Asserts that an action result is a ForbidResult
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>ForbidResult for further assertions</returns>
    protected ForbidResult AssertForbidResult(IActionResult result)
    {
        return Assert.IsType<ForbidResult>(result);
    }

    /// <summary>
    /// Asserts that an action result is an UnauthorizedResult with status code 401
    /// </summary>
    /// <param name="result">Action result to verify</param>
    /// <returns>UnauthorizedResult for further assertions</returns>
    protected UnauthorizedResult AssertUnauthorizedResult(IActionResult result)
    {
        return Assert.IsType<UnauthorizedResult>(result);
    }

    /// <summary>
    /// Cleans up resources after test execution
    /// </summary>
    public virtual void Dispose()
    {
        // Override in derived classes if needed
        GC.SuppressFinalize(this);
    }
}
