using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Boundary tests for PNO-914: Status boundaries, config limits, edge values.
/// </summary>
public class PNO914BoundaryTests
{
    // ========== ErrorParser Boundary ==========

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status0_ExactBoundary()
    {
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(0);
        result.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status499_ClientErrorNotServerError()
    {
        var err = new HttpErrorSpec(499, ErrorObject: new ErrorObjectSpec(Title: "Client Error"));
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(499);
        result.Title.Should().Be("Client Error");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status500_ServerErrorBoundary()
    {
        var err = new HttpErrorSpec(500);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(500);
        result.Title.Should().Be("Server Error");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status599_LastServerError()
    {
        var err = new HttpErrorSpec(599);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(599);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ValidationErrors_EmptyDictionary()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: new Dictionary<string, string[]>()));
        var result = ErrorParserSpec.Parse(err);
        result.ValidationErrors.Should().BeNull("empty Errors dict skips validation branch, falls through to fallback");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ValidationErrors_SingleKey()
    {
        var errors = new Dictionary<string, string[]> { ["field"] = new[] { "msg1" } };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.ValidationErrors!["field"].Should().Contain("msg1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ValidationErrors_MultipleValues()
    {
        var errors = new Dictionary<string, string[]> { ["name"] = new[] { "Required", "Too short" } };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Contain("Required");
        result.Detail.Should().Contain("Too short");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ErrorObject_OnlyErrorField()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Error: "Simple error"));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Error 400");
        result.Detail.Should().Be("Simple error");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_MissingFields_Array()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Error: "Invalid", MissingFields: new[] { "name", "email" }));
        var result = ErrorParserSpec.Parse(err);
        result.MissingFields.Should().NotBeNull();
        result.MissingFields!.Should().Contain("name");
        result.MissingFields.Should().Contain("email");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status401_NotTreatedAsNetworkError()
    {
        var err = new HttpErrorSpec(401);
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().NotBe(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void SessionConfig_Expiry480_ExactMinimum()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().BeGreaterThanOrEqualTo(SessionConfigSpec.MinimumExpiryMinutes);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void SessionConfig_Expiry_UpperBound_Reasonable()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().BeLessThanOrEqualTo(10080, "Session > 7 days may be excessive");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IAPMiddleware_PublicKeyRefresh_OneHourBoundary()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("AddHours(1)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IAPMiddleware_ClockSkew_FiveMinutesBoundary()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("ClockSkew");
        middlewareSource.Should().Contain("FromMinutes(5)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status0_NullUrl_Allowed()
    {
        var err = new HttpErrorSpec(0, Url: null);
        var result = ErrorParserSpec.Parse(err);
        result.Url.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status500_EmptyDetail_UsesDefault()
    {
        var err = new HttpErrorSpec(500, ErrorObject: new ErrorObjectSpec(Title: "Server Error", Detail: ""));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Be("An unexpected server error occurred. Please try again later.");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_MessageFallback()
    {
        var err = new HttpErrorSpec(400, Message: "Fallback message", ErrorObject: new ErrorObjectSpec());
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status404_ProblemDetails_NullDetail()
    {
        var err = new HttpErrorSpec(404, ErrorObject: new ErrorObjectSpec(Title: "Not Found", Detail: null));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Be("An error occurred while processing your request.");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void AuthService_IsCheckingAuth_ConcurrentGuard()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("isCheckingAuth");
        authSource.Should().Contain("isCheckingAuth = true");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void AuthService_IapAuthenticationChecked_CacheFlag()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("iapAuthenticationChecked");
        authSource.Should().Contain("iapAuthenticationStatus");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status0_TimestampSet()
    {
        var before = DateTime.UtcNow;
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err);
        var after = DateTime.UtcNow;
        result.Timestamp.Should().BeAfter(before.AddSeconds(-1));
        result.Timestamp.Should().BeBefore(after.AddSeconds(1));
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status502_StackTracePreserved()
    {
        var err = new HttpErrorSpec(502, ErrorObject: new ErrorObjectSpec(StackTrace: "at Foo.Bar()"));
        var result = ErrorParserSpec.Parse(err);
        result.StackTrace.Should().Be("at Foo.Bar()");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void HealthCheckPath_DefaultWhenMissing()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain(SessionConfigSpec.DefaultHealthCheckPath);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ErrorsObject_NonStandardFormat()
    {
        var errors = new Dictionary<string, string[]> { ["a"] = new[] { "b" }, ["c"] = new[] { "d" } };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Contain("a");
        result.Detail.Should().Contain("c");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void AuthInterceptor_401_DevCookie_ReloadPath()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("/dev-login");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status503_NoErrorObject_Fallback()
    {
        var err = new HttpErrorSpec(503, ErrorObject: null);
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Server Error");
        result.Detail.Should().Contain("unexpected");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void SessionConfig_JWTSettings_SectionExists()
    {
        var json = ReadAppSettingsJson();
        json.Should().Contain("JWTSettings");
        json.Should().Contain("expiryInMinutes");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IAPMiddleware_StaticKeyCache_RefreshLock()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("_refreshLock");
        middlewareSource.Should().Contain("SemaphoreSlim");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status0_ContextNull_Allowed()
    {
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err, null);
        result.Context.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IAPMiddleware_DevCookie_8HourExpiry()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("AddHours(8)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status1_NotNetworkError()
    {
        var err = new HttpErrorSpec(1);
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().NotBe(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status400_ExactClientBoundary()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Title: "Bad Request"));
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(400);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status200_NoErrors()
    {
        var err = new HttpErrorSpec(200);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(200);
        result.Title.Should().NotBe(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IAPMiddleware_PublicKeyCache_ThreadSafe()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("SemaphoreSlim(1, 1)");
        middlewareSource.Should().Contain("WaitAsync");
        middlewareSource.Should().Contain("Release");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void SessionConfig_DevCookie_7DayExpiry()
    {
        var devLoginSource = ReadDevLoginMiddlewareSource();
        if (!string.IsNullOrEmpty(devLoginSource))
            devLoginSource.Should().Contain("AddDays(7)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ErrorParser_Status0_MultipleParses_SameResult()
    {
        var err = new HttpErrorSpec(0, "/api/test");
        var r1 = ErrorParserSpec.Parse(err);
        var r2 = ErrorParserSpec.Parse(err);
        r1.Title.Should().Be(r2.Title);
        r1.Detail.Should().Be(r2.Detail);
    }

    private static string ReadDevLoginMiddlewareSource()
    {
        var path = ResolvePath("UNOPS.PAO.Server", "Infrastructure", "DevelopmentLoginPageMiddleware.cs");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static int GetJwtExpiryFromAppSettings()
    {
        var json = ReadAppSettingsJson();
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""expiryInMinutes""\s*:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static string ReadAppSettingsJson()
    {
        var path = ResolvePath("UNOPS.PAO.Server", "appsettings.json");
        return File.Exists(path) ? File.ReadAllText(path) : "{}";
    }

    private static string ReadIAPMiddlewareSource()
    {
        var path = ResolvePath("UNOPS.PAO.UNOPSIdentity", "Authentication", "IAPVerificationMiddleware.cs");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadAuthServiceSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "core", "services", "auth", "auth.service.ts");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadAuthInterceptorSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "core", "interceptors", "auth.interceptor.ts");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
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
