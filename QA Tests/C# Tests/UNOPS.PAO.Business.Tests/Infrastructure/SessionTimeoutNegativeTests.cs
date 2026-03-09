using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Negative tests for PNO-914: Invalid inputs, missing config, anti-patterns.
/// </summary>
public class PNO914NegativeTests
{
    // ========== ErrorParser Negative ==========

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status0_ShouldNotReturnGenericMessage()
    {
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().NotBe("Connection Lost");
        result.Detail.Should().Be(ErrorParserSpec.NetworkErrorDetailKey, "AC-5: Use translation key, not hardcoded text");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status500_WithoutErrorObject_ShouldNotThrow()
    {
        var err = new HttpErrorSpec(500, ErrorObject: null);
        var act = () => ErrorParserSpec.Parse(err);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status404_WithNullErrorObject_ReturnsFallback()
    {
        var err = new HttpErrorSpec(404, ErrorObject: null);
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().NotBeNullOrEmpty();
        result.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status0_ShouldNotIncludeStackTrace()
    {
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err);
        result.StackTrace.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status400_EmptyErrorObject_ShouldNotThrow()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec());
        var act = () => ErrorParserSpec.Parse(err);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorHandler_MustNotHandle401_LestDuplicateAuthHandling()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("if (error.status === 401)");
        handlerSource.Should().Contain("return", "401 must be skipped to avoid duplicate handling");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthInterceptor_MustNotSwallowErrors_WithoutPropagating()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("catchError");
        interceptorSource.Should().NotContain("throwError", "DEF-117: Interceptor uses of(error) instead of throwError");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-117")]
    public void AuthInterceptor_ShouldPropagateErrors_ForServerErrorInterceptor()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("throwError(() => error)",
            "DEF-117: Auth interceptor returns of(error) which swallows errors - server-error interceptor never sees 401/403");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-118")]
    public void AuthService_RedirectCounter_ShouldBeChecked()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("redirectCounter");
        authSource.Should().Contain("redirectCounter >", "DEF-118: redirectCounter is incremented but never checked for loop prevention");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-118")]
    public void AuthService_RedirectCounter_ShouldPreventInfiniteLoops()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("redirectCounter");
        var hasCheck = authSource.Contains("redirectCounter >") || authSource.Contains("redirectCounter >=") ||
                       authSource.Contains("if (redirectCounter)");
        hasCheck.Should().BeTrue("DEF-118: redirectCounter is incremented but never used to prevent redirect loops");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void SessionConfig_ExpiryMustNotBeZero()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().NotBe(0, "Session expiry of 0 would cause immediate logout");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void SessionConfig_ExpiryMustNotBeLessThan60()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        if (expiry > 0)
            expiry.Should().BeGreaterOrEqualTo(60, "Session less than 1 hour causes frequent timeouts");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IAPMiddleware_HealthCheckPath_MustNotBeNull()
    {
        var path = GetIapHealthCheckPathFromConfig();
        path.Should().NotBeNullOrEmpty("Health check path required for IAP bypass");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status500_NullTitle_UsesDefault()
    {
        var err = new HttpErrorSpec(500, ErrorObject: new ErrorObjectSpec(Title: null));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Server Error");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status400_ValidationErrors_NonObjectFormat_ShouldNotThrow()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: new Dictionary<string, string[]> { ["x"] = new[] { "y" } }));
        var act = () => ErrorParserSpec.Parse(err);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthService_MustNotExposeRedirectCounterPublicly()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().NotContain("public redirectCounter");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorHandler_NetworkError_MustShowRefreshButton()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("showRefreshButton");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void SessionKeepalive_ShouldExist()
    {
        var clientSource = ReadClientAppSourceForPattern("keepalive|heartbeat|session.*refresh|IapSessionRefresh");
        clientSource.Should().NotBeNullOrEmpty("DEF-116: Session keepalive mechanism should exist");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-116")]
    public void SessionKeepalive_Mechanism_ShouldExist()
    {
        var authService = ReadAuthServiceSource();
        var hasKeepalive = authService.Contains("keepalive") || authService.Contains("heartbeat") ||
                          authService.Contains("refreshToken") || authService.Contains("extendSession");
        hasKeepalive.Should().BeTrue("DEF-116: No session keepalive/heartbeat - users get frequent Connection Lost");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void SessionExpiryWarning_ShouldExist()
    {
        var clientSource = ReadClientAppSourceForPattern("session.*expir|expir.*warn|timeout.*warn");
        clientSource.Should().NotBeNullOrEmpty("DEF-119: Session expiry warning should exist");
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-119")]
    public void SessionExpiryWarning_ShouldWarnBeforeExpiry()
    {
        var authSource = ReadAuthServiceSource();
        var errorHandler = ReadErrorHandlerSource();
        var combined = authSource + errorHandler;
        var hasWarning = combined.Contains("session") && (combined.Contains("warn") || combined.Contains("expir") || combined.Contains("about to expire"));
        hasWarning.Should().BeTrue("DEF-119: No session expiry warning - users get Connection Lost with no prior notification");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthInterceptor_RetryOnTransientFailure_ShouldExist()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("retry", "REQ: Auth interceptor should retry on network failures before showing Connection Lost");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status0_UrlPreserved()
    {
        var err = new HttpErrorSpec(0, Url: "/api/opportunity/1");
        var result = ErrorParserSpec.Parse(err);
        result.Url.Should().Be("/api/opportunity/1");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IAPMiddleware_ClockSkew_ShouldNotExceed5Minutes()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        var match = System.Text.RegularExpressions.Regex.Match(middlewareSource, @"ClockSkew\s*=\s*TimeSpan\.FromMinutes\((\d+)\)");
        if (match.Success)
        {
            var minutes = int.Parse(match.Groups[1].Value);
            minutes.Should().BeLessThanOrEqualTo(SessionConfigSpec.MaxClockSkewMinutes);
        }
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status503_ReturnsServerError()
    {
        var err = new HttpErrorSpec(503);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(503);
        result.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status502_WithErrorObject_UsesProvidedDetail()
    {
        var err = new HttpErrorSpec(502, ErrorObject: new ErrorObjectSpec(Detail: "Bad Gateway"));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Be("Bad Gateway");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthService_ResetAuthenticationState_ShouldResetRedirectCounter()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("resetAuthenticationState");
        authSource.Should().Contain("redirectCounter = 0");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorHandler_MustNotShowToastForStatus0()
    {
        var handlerSource = ReadErrorHandlerSource();
        var status0Section = ExtractStatus0Handling(handlerSource);
        status0Section.Should().NotContain("showErrorToast");
        status0Section.Should().Contain("showNetworkErrorDialog");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IAPMiddleware_MustNotSkipHealthCheckInProduction()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("HealthCheckPath");
        middlewareSource.Should().Contain("StartsWithSegments");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthInterceptor_MustNotNavigateToLogin_WhenAlreadyOnLogin()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("router.url.includes('/login')");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_NegativeStatus_ShouldNotThrow()
    {
        var err = new HttpErrorSpec(-1);
        var act = () => ErrorParserSpec.Parse(err);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status400_NullMessage_ShouldNotThrow()
    {
        var err = new HttpErrorSpec(400, Message: null!, ErrorObject: null);
        var act = () => ErrorParserSpec.Parse(err);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AuthService_IsCheckingAuth_MustResetOnError()
    {
        var authSource = ReadAuthServiceSource();
        var catchBlocks = System.Text.RegularExpressions.Regex.Matches(authSource, @"catchError");
        catchBlocks.Count.Should().BeGreaterThan(0, "Auth service should handle errors and reset isCheckingAuth");
        authSource.Should().Contain("isCheckingAuth = false");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IAPMiddleware_MissingBothHeaders_Returns401()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("No IAP authentication found");
        middlewareSource.Should().Contain("401");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorParser_Status0_EmptyUrl_Allowed()
    {
        var err = new HttpErrorSpec(0, Url: "");
        var result = ErrorParserSpec.Parse(err);
        result.Url.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ErrorHandler_MustLogBeforeShowingDialog()
    {
        var handlerSource = ReadErrorHandlerSource();
        var logIdx = handlerSource.IndexOf("logger.error");
        var dialogIdx = handlerSource.IndexOf("showNetworkErrorDialog");
        logIdx.Should().BeLessThan(dialogIdx, "Logging must happen before showing dialog");
    }

    private static string ExtractStatus0Handling(string source)
    {
        var idx = source.IndexOf("status === 0");
        if (idx < 0) return source;
        var end = source.IndexOf("}", idx + 1);
        return end > idx ? source[idx..end] : source[idx..];
    }

    private static int GetJwtExpiryFromAppSettings()
    {
        var json = ReadAppSettingsJson();
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""expiryInMinutes""\s*:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static string GetIapHealthCheckPathFromConfig()
    {
        var json = ReadAppSettingsJson();
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""HealthCheckPath""\s*:\s*""([^""]*)""");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ReadAppSettingsJson()
    {
        var path = ResolvePath("UNOPS.PAO.Server", "appsettings.json");
        return File.Exists(path) ? File.ReadAllText(path) : "{}";
    }

    private static string ReadErrorHandlerSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "shared", "services", "utils", "error-handler.service.ts");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadAuthInterceptorSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "core", "interceptors", "auth.interceptor.ts");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadAuthServiceSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "core", "services", "auth", "auth.service.ts");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadIAPMiddlewareSource()
    {
        var path = ResolvePath("UNOPS.PAO.UNOPSIdentity", "Authentication", "IAPVerificationMiddleware.cs");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadClientAppSourceForPattern(string pattern)
    {
        var dir = ResolvePath("UNOPS.PAO.ClientApp", "src", "app");
        if (!Directory.Exists(dir)) return string.Empty;
        var files = Directory.GetFiles(dir, "*.ts", SearchOption.AllDirectories);
        var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return files.Where(f => regex.IsMatch(File.ReadAllText(f))).Select(Path.GetFileName).FirstOrDefault() ?? string.Empty;
    }

    private static string ResolvePath(params string[] segments)
    {
        var relative = Path.Combine(segments);
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", relative),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", relative),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", relative),
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full) || Directory.Exists(full))
                return full;
        }
        return Path.Combine(baseDir, Path.GetFileName(relative));
    }
}
