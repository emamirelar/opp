using System.Security.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Server.Infrastructure;
using System.Text.Json;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Tests for GlobalExceptionHandler to verify correct HTTP status codes,
/// ProblemDetails structure, and environment-specific behavior.
/// Designed to find bugs in exception-to-status-code mapping.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public GlobalExceptionHandlerTests()
    {
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        _mockServiceProvider = new Mock<IServiceProvider>();
    }

    private GlobalExceptionHandler CreateHandler(bool isDevelopment = false)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(isDevelopment ? Environments.Development : Environments.Production);
        return new GlobalExceptionHandler(env.Object, _mockServiceProvider.Object);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails?> GetProblemDetails(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        return await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    #region Positive Tests (P=2)

    [Fact]
    public async Task P1_BusinessException_Returns400()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var result = await handler.TryHandleAsync(httpContext, new BusinessException("Bad request"), CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task P2_KeyNotFoundException_Returns404()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var result = await handler.TryHandleAsync(httpContext, new KeyNotFoundException("Not found"), CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(404);
    }

    #endregion

    #region Negative Tests (N≥6)

    [Fact]
    public async Task N1_UnauthorizedAccessException_Returns403()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new UnauthorizedAccessException("Forbidden"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task N2_AuthenticationException_Returns401()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new AuthenticationException("Not authenticated"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task N3_GenericException_Returns500()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new InvalidOperationException("Server error"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task N4_NullReferenceException_Returns500_DoesNotExposeMessage()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new NullReferenceException("Object reference null"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(500);
        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be("Server error occurred", "500 errors should not expose internal messages");
    }

    [Fact]
    public async Task N5_ApplicationException_Returns400()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new ApplicationException("App error"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task N6_ExceptionWithNullMessage_DoesNotThrow()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var act = async () => await handler.TryHandleAsync(httpContext, new Exception(null), CancellationToken.None);

        await act.Should().NotThrowAsync();
        httpContext.Response.StatusCode.Should().Be(500);
    }

    #endregion

    #region Edge/Boundary Tests (E≥6)

    [Fact]
    public async Task E1_BusinessExceptionMessage_PreservedInResponse()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();
        var message = "Partner name is required";

        await handler.TryHandleAsync(httpContext, new BusinessException(message), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be(message);
    }

    [Fact]
    public async Task E2_500Error_HidesInternalMessage()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new InvalidOperationException("SQL injection in column 'users'"), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be("Server error occurred")
            .And.NotContain("SQL", "internal error details should never be exposed");
    }

    [Fact]
    public async Task E3_DevelopmentMode_IncludesStackTrace()
    {
        var handler = CreateHandler(isDevelopment: true);
        var httpContext = CreateHttpContext();

        try { throw new InvalidOperationException("test error"); }
        catch (Exception ex)
        {
            await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);
        }

        var body = await ReadResponseBody(httpContext);
        body.Should().Contain("StackTrace", "development mode should include stack trace");
    }

    [Fact]
    public async Task E4_ProductionMode_ExcludesStackTrace()
    {
        var handler = CreateHandler(isDevelopment: false);
        var httpContext = CreateHttpContext();

        try { throw new InvalidOperationException("test error"); }
        catch (Exception ex)
        {
            await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);
        }

        var body = await ReadResponseBody(httpContext);
        body.Should().NotContain("StackTrace", "production should not include stack traces");
    }

    [Fact]
    public async Task E5_VeryLongExceptionMessage_DoesNotTruncate()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();
        var longMessage = new string('x', 10000);

        await handler.TryHandleAsync(httpContext, new BusinessException(longMessage), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().HaveLength(10000);
    }

    [Fact]
    public async Task E6_AggregateException_Returns500()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();
        var aggregateEx = new AggregateException(
            new InvalidOperationException("inner1"),
            new ArgumentException("inner2"));

        await handler.TryHandleAsync(httpContext, aggregateEx, CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(500);
    }

    #endregion

    #region Functional Tests (F≥6)

    [Fact]
    public async Task F1_ProblemDetailsType_CorrectRFC7231Url()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new BusinessException("test"), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public async Task F2_404_CorrectRFCUrl()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new KeyNotFoundException("not found"), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4");
    }

    [Fact]
    public async Task F3_403_CorrectRFCUrl()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new UnauthorizedAccessException("forbidden"), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3");
    }

    [Fact]
    public async Task F4_401_CorrectRFCUrl()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new AuthenticationException("unauthorized"), CancellationToken.None);

        var details = await GetProblemDetails(httpContext);
        details!.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7235#section-3.1");
    }

    [Fact]
    public async Task F5_AlwaysReturnsTrue()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var result = await handler.TryHandleAsync(httpContext, new Exception("any"), CancellationToken.None);

        result.Should().BeTrue("handler claims to always handle exceptions");
    }

    [Fact]
    public async Task F6_ResponseContentType_IsJson()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new BusinessException("test"), CancellationToken.None);

        httpContext.Response.ContentType.Should().Contain("json");
    }

    [Fact]

    [Trait("Defect", "DEF-067")]
    public async Task F7_500Errors_ShouldBeLoggedToDatabase()
    {
        var handler = CreateHandler(isDevelopment: false);
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new InvalidOperationException("server crash"), CancellationToken.None);

        // The commented-out code in production means errors are NEVER logged:
        // if (!hostEnvironment.IsDevelopment() && exception is not BusinessException)
        // {
        //     var errorLog = new ErrorLog(...);
        //     baseDbContext.ErrorLogs.Add(errorLog);
        //     await baseDbContext.SaveChangesAsync();
        // }
        Assert.Fail("Error logging is commented out in GlobalExceptionHandler — errors are silently lost");
    }

    #endregion

    #region Integration Tests (I≥6)

    [Fact]
    public async Task I1_FullFlow_BusinessException()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var result = await handler.TryHandleAsync(httpContext, new BusinessException("Validation failed"), CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(400);
        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be("Validation failed");
        details.Status.Should().Be(400);
    }

    [Fact]
    public async Task I2_FullFlow_KeyNotFoundException()
    {
        var handler = CreateHandler();
        var httpContext = CreateHttpContext();

        var result = await handler.TryHandleAsync(httpContext, new KeyNotFoundException("Partner 123 not found"), CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(404);
        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be("Partner 123 not found");
    }

    [Fact]
    public async Task I3_FullFlow_GenericException_Production()
    {
        var handler = CreateHandler(isDevelopment: false);
        var httpContext = CreateHttpContext();

        await handler.TryHandleAsync(httpContext, new Exception("internal error"), CancellationToken.None);

        httpContext.Response.StatusCode.Should().Be(500);
        var details = await GetProblemDetails(httpContext);
        details!.Title.Should().Be("Server error occurred");
    }

    [Fact]
    public async Task I4_FullFlow_GenericException_Development()
    {
        var handler = CreateHandler(isDevelopment: true);
        var httpContext = CreateHttpContext();

        try { throw new Exception("dev error"); }
        catch (Exception ex)
        {
            await handler.TryHandleAsync(httpContext, ex, CancellationToken.None);
        }

        httpContext.Response.StatusCode.Should().Be(500);
        var body = await ReadResponseBody(httpContext);
        body.Should().Contain("StackTrace");
    }

    [Fact]
    public async Task I5_ExceptionTypeHierarchy_CorrectMapping()
    {
        var handler = CreateHandler();
        var testCases = new (Exception, int)[]
        {
            (new BusinessException("biz"), 400),
            (new ApplicationException("app"), 400),
            (new UnauthorizedAccessException("unauth"), 403),
            (new AuthenticationException("auth"), 401),
            (new KeyNotFoundException("key"), 404),
            (new InvalidOperationException("invalid"), 500),
            (new ArgumentException("arg"), 500),
            (new NullReferenceException("null"), 500),
        };

        foreach (var (exception, expectedStatus) in testCases)
        {
            var ctx = CreateHttpContext();
            await handler.TryHandleAsync(ctx, exception, CancellationToken.None);
            ctx.Response.StatusCode.Should().Be(expectedStatus, $"{exception.GetType().Name} should return {expectedStatus}");
        }
    }

    [Fact]
    public async Task I6_ConcurrentExceptions_AllHandledCorrectly()
    {
        var handler = CreateHandler();
        var tasks = Enumerable.Range(0, 10).Select(i =>
        {
            var ctx = CreateHttpContext();
            var ex = i % 2 == 0 ? (Exception)new BusinessException("biz") : new KeyNotFoundException("key");
            var expectedCode = i % 2 == 0 ? 400 : 404;
            return handler.TryHandleAsync(ctx, ex, CancellationToken.None)
                .AsTask()
                .ContinueWith(t => (ctx.Response.StatusCode, expectedCode));
        });
        var results = await Task.WhenAll(tasks);
        foreach (var (actual, expected) in results)
        {
            actual.Should().Be(expected);
        }
    }

    #endregion

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
