using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Server.Middleware;
using UNOPS.PAO.Server.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Middleware;

/// <summary>
/// Tests for middleware components focusing on security, correctness, and edge cases.
/// </summary>
public class MiddlewareSecurityTests
{
    #region ValidationMiddleware Tests

    // ── Positive (P=1) ──

    [Fact]
    public async Task Validation_P1_ValidRequest_PassesThrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    // ── Negative (N≥3) ──

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task Validation_N1_InvalidModelState_ShouldReturn400()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        // ValidationMiddleware.InvokeAsync just calls: await _next(context);
        // It performs ZERO validation. The CustomValidationProblemDetails class exists but is never used.
        Assert.Fail("ValidationMiddleware does not perform any validation — it is a complete no-op");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task Validation_N2_MissingRequiredFields_ShouldReturn400()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400, "middleware should validate and reject bad requests");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task Validation_N3_NullRequestBody_ShouldReturn400()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(400);
    }

    // ── Edge (E≥3) ──

    [Fact]
    public async Task Validation_E1_NoOp_AlwaysCallsNext()
    {
        var callCount = 0;
        RequestDelegate next = ctx => { callCount++; return Task.CompletedTask; };
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());

        await middleware.InvokeAsync(new DefaultHttpContext());
        await middleware.InvokeAsync(new DefaultHttpContext());

        callCount.Should().Be(2, "middleware unconditionally forwards all requests");
    }

    [Fact]
    public async Task Validation_E2_NextThrows_ExceptionPropagates()
    {
        RequestDelegate next = ctx => throw new InvalidOperationException("next failed");
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());

        var act = async () => await middleware.InvokeAsync(new DefaultHttpContext());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Validation_E3_NullLogger_ConstructorAccepts()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var act = () => new ValidationMiddleware(next, null!);

        // Middleware stores logger but never uses it (since it's a no-op)
        act.Should().NotThrow();
    }

    // ── Functional (F≥3) ──

    [Fact]
    public async Task Validation_F1_CustomValidationProblemDetails_FieldConstructor()
    {
        var details = new CustomValidationProblemDetails("email", "Email is required");

        details.Title.Should().Be("Validation failed");
        details.Status.Should().Be(400);
        details.Errors.Should().ContainKey("email");
        details.Errors["email"].Should().Contain("Email is required");
    }

    [Fact]
    public async Task Validation_F2_CustomValidationProblemDetails_DictionaryConstructor()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "name", new[] { "Name is required" } },
            { "email", new[] { "Invalid email format" } },
        };
        var details = new CustomValidationProblemDetails(errors);

        details.Errors.Should().HaveCount(2);
        details.Type.Should().Be("https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1");
    }

    [Fact]

    [Trait("Defect", "DEF-066")]
    public async Task Validation_F3_CustomProblemDetails_ShouldBeUsedByMiddleware()
    {
        // CustomValidationProblemDetails exists but is NEVER instantiated by the middleware
        // The middleware InvokeAsync method is just: await _next(context);
        Assert.Fail("CustomValidationProblemDetails is dead code — never used by ValidationMiddleware");
    }

    // ── Integration (I≥3) ──

    [Fact]
    public async Task Validation_I1_MiddlewareInPipeline_TransparentPassthrough()
    {
        var finalResponse = 0;
        RequestDelegate next = ctx => { finalResponse = 200; ctx.Response.StatusCode = 200; return Task.CompletedTask; };
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        finalResponse.Should().Be(200);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Validation_I2_ConcurrentRequests_AllPassThrough()
    {
        var callCount = 0;
        RequestDelegate next = ctx => { Interlocked.Increment(ref callCount); return Task.CompletedTask; };
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());

        var tasks = Enumerable.Range(0, 10).Select(_ =>
            middleware.InvokeAsync(new DefaultHttpContext()));
        await Task.WhenAll(tasks);

        callCount.Should().Be(10);
    }

    [Fact]
    public async Task Validation_I3_ResponseStatusNotModified()
    {
        RequestDelegate next = ctx => { ctx.Response.StatusCode = 201; return Task.CompletedTask; };
        var middleware = new ValidationMiddleware(next, Mock.Of<ILogger<ValidationMiddleware>>());
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(201, "middleware should not modify response status");
    }

    #endregion

    #region AuthenticationLoggingMiddleware Tests

    [Fact]
    public async Task AuthLogging_P1_ApiRequest_LogsAndPassesThrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/partners";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task AuthLogging_N1_NonApiRequest_SkipsLogging()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/static/file.js";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue("non-API requests should still pass through");
    }

    [Fact]
    public async Task AuthLogging_N2_NextThrows_ExceptionPropagates()
    {
        RequestDelegate next = ctx => throw new InvalidOperationException("downstream error");
        var middleware = new AuthenticationLoggingMiddleware(next, Mock.Of<ILogger<AuthenticationLoggingMiddleware>>());
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        var act = async () => await middleware.InvokeAsync(context);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AuthLogging_N3_MissingIAPHeaders_LogsWarning()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/partners";

        await middleware.InvokeAsync(context);

        // Verify warning was logged for missing headers
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Missing")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AuthLogging_E1_IAPEmailHeader_ExtractsEmail()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Goog-Authenticated-User-Email"] = "accounts.google.com:user@unops.org";

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("user@unops.org")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AuthLogging_E2_JWTHeader_RedactsTokenValue()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Goog-IAP-JWT-Assertion"] = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.secret-payload";

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("REDACTED")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AuthLogging_E3_EmptyPath_DoesNotThrow()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthenticationLoggingMiddleware(next, Mock.Of<ILogger<AuthenticationLoggingMiddleware>>());
        var context = new DefaultHttpContext();
        context.Request.Path = "";

        var act = async () => await middleware.InvokeAsync(context);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AuthLogging_F1_LogsBeforeAndAfterAuthentication()
    {
        var logMessages = new List<string>();
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((level, id, state, ex, formatter) =>
                logMessages.Add(state.ToString()!));

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/partners";

        await middleware.InvokeAsync(context);

        logMessages.Should().Contain(m => m.Contains("Before authentication"));
        logMessages.Should().Contain(m => m.Contains("After authentication"));
    }

    [Fact]
    public async Task AuthLogging_F2_ApiPathDetection_CaseSensitive()
    {
        var logCalled = false;
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback(() => logCalled = true);

        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/API/partners"; // uppercase

        await middleware.InvokeAsync(context);

        // StartsWithSegments is case-insensitive by default for path matching
    }

    [Fact]
    public async Task AuthLogging_F3_DevSimulationHeader_Logged()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Request.Headers["X-Dev-IAP-Simulation"] = "true";

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dev IAP Simulation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AuthLogging_I1_FullApiRequestFlow()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var middleware = new AuthenticationLoggingMiddleware(next, Mock.Of<ILogger<AuthenticationLoggingMiddleware>>());
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/partners/123";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task AuthLogging_I2_ConcurrentRequests_NoInterference()
    {
        var callCount = 0;
        RequestDelegate next = ctx => { Interlocked.Increment(ref callCount); return Task.CompletedTask; };
        var middleware = new AuthenticationLoggingMiddleware(next, Mock.Of<ILogger<AuthenticationLoggingMiddleware>>());

        var tasks = Enumerable.Range(0, 10).Select(_ =>
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = "/api/test";
            return middleware.InvokeAsync(ctx);
        });
        await Task.WhenAll(tasks);

        callCount.Should().Be(10);
    }

    [Fact]
    public async Task AuthLogging_I3_NonApiRequest_NoLogs()
    {
        var mockLogger = new Mock<ILogger<AuthenticationLoggingMiddleware>>();
        RequestDelegate next = ctx => Task.CompletedTask;
        var middleware = new AuthenticationLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/static/file.js";

        await middleware.InvokeAsync(context);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Before authentication")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    #endregion

    #region DevelopmentLoginPageMiddleware Tests

    [Fact]
    public async Task DevLogin_P1_NonDevEnvironment_PassesThrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue("production env should skip dev login page");
    }

    [Fact]
    public async Task DevLogin_N1_NonDevLoginPath_PassesThrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/partners";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue("non dev-login paths should pass through");
    }

    [Fact]
    public async Task DevLogin_N2_DevEnvironment_DevLoginPath_ServesPage()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("text/html");
    }

    [Fact]
    public async Task DevLogin_N3_ProductionEnvironment_NeverServesLoginPage()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().NotBe("text/html", "production should never serve dev login page");
    }

    [Fact]
    public async Task DevLogin_E1_StagingEnvironment_SkipsDevLogin()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Staging);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue("staging should not serve dev login page");
    }

    [Fact]
    public async Task DevLogin_E2_UserQueryParam_DevEnvironment_SetsLoginCookie()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Request.QueryString = new QueryString("?user=test@unops.org");
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task DevLogin_E3_EmptyPath_PassesThrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        await middleware.InvokeAsync(context);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task DevLogin_F1_OnlyDevEnvironment_ServesPage()
    {
        var environments = new[] { Environments.Staging, Environments.Production, "Testing" };
        foreach (var env in environments)
        {
            var nextCalled = false;
            RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.EnvironmentName).Returns(env);
            var middleware = new DevelopmentLoginPageMiddleware(
                next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
                new ConfigurationBuilder().Build());
            var context = new DefaultHttpContext();
            context.Request.Path = "/dev-login";

            await middleware.InvokeAsync(context);

            nextCalled.Should().BeTrue($"environment '{env}' should not serve dev login page");
        }
    }

    [Fact]
    public async Task DevLogin_F2_LoginPageContainsForm()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("loginWithEmail", "login page should contain email login function");
    }

    [Fact]
    public async Task DevLogin_F3_ConfiguredUserEmail_Available()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Development:IAPSimulation:UserEmail", "configured@unops.org" }
            })
            .Build();
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(), config);
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("configured@unops.org");
    }

    [Fact]
    public async Task DevLogin_I1_FullDevLoginFlow()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("text/html");
        context.Response.Body.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DevLogin_I2_ProductionFullFlow_Passthrough()
    {
        var nextCalled = false;
        RequestDelegate next = ctx => { nextCalled = true; return Task.CompletedTask; };
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());
        var context = new DefaultHttpContext();
        context.Request.Path = "/dev-login";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task DevLogin_I3_ConcurrentDevLoginRequests()
    {
        RequestDelegate next = ctx => Task.CompletedTask;
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var middleware = new DevelopmentLoginPageMiddleware(
            next, mockEnv.Object, Mock.Of<ILogger<DevelopmentLoginPageMiddleware>>(),
            new ConfigurationBuilder().Build());

        var tasks = Enumerable.Range(0, 5).Select(_ =>
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = "/dev-login";
            ctx.Response.Body = new MemoryStream();
            return middleware.InvokeAsync(ctx);
        });

        var act = async () => await Task.WhenAll(tasks);
        await act.Should().NotThrowAsync();
    }

    #endregion
}
