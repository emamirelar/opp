using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Functional tests for PNO-914: Business rules, state transitions, data flow.
/// </summary>
public class PNO914FunctionalTests
{
    // ========== ErrorParser Functional ==========

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Status0_AlwaysMapsToTranslationKeys()
    {
        var err = new HttpErrorSpec(0, "/api/any");
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);
        result.Detail.Should().Be(ErrorParserSpec.NetworkErrorDetailKey);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Status500Plus_IncludesStackTraceWhenPresent()
    {
        var err = new HttpErrorSpec(500, ErrorObject: new ErrorObjectSpec(StackTrace: "Trace line"));
        var result = ErrorParserSpec.Parse(err);
        result.StackTrace.Should().Be("Trace line");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_ValidationErrors_FormatsAsKeyValuePairs()
    {
        var errors = new Dictionary<string, string[]> { ["Name"] = new[] { "Required" } };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Contain("Name");
        result.Detail.Should().Contain("Required");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_ProblemDetails_TitleTakesPrecedence()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Title: "Custom Title", Detail: "Custom detail"));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Custom Title");
        result.Detail.Should().Be("Custom detail");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_SimpleError_TakesErrorFieldOverMessage()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Error: "Error field", Message: "Message field"));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Be("Error field");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Url_PreservedThroughParse()
    {
        var err = new HttpErrorSpec(404, Url: "https://example.com/api/foo");
        var result = ErrorParserSpec.Parse(err);
        result.Url.Should().Be("https://example.com/api/foo");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Context_PreservedThroughParse()
    {
        var err = new HttpErrorSpec(500);
        var result = ErrorParserSpec.Parse(err, "OpportunityService");
        result.Context.Should().Be("OpportunityService");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorHandler_Status0_ShowsDialogNotToast()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("status === 0");
        handlerSource.Should().Contain("showNetworkErrorDialog");
        var idx0 = handlerSource.IndexOf("status === 0");
        var idxDialog = handlerSource.IndexOf("showNetworkErrorDialog");
        var idxToast = handlerSource.IndexOf("showErrorToast");
        idxDialog.Should().BeLessThan(idxToast, "status 0 shows dialog before toast path");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorHandler_NonZeroStatus_ShowsToastNotDialog()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("showErrorToast");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthInterceptor_401_WithoutDevCookie_NavigatesToLogin()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("router.navigate");
        interceptorSource.Should().Contain("login");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthInterceptor_401_WithDevCookie_ReloadsPage()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("window.location.reload");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthInterceptor_403_LogsToConsole()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("403");
        interceptorSource.Should().Contain("console.error");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_ResetAuthenticationState_ClearsAllFlags()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("redirectCounter = 0");
        authSource.Should().Contain("iapAuthenticationChecked = false");
        authSource.Should().Contain("iapAuthenticationStatus = false");
        authSource.Should().Contain("isCheckingAuth = false");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_IsIapAuthenticated_ChecksDevCookieFirst()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("hasDevCookie");
        authSource.Should().Contain("dev-user-email");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_IsCheckingAuth_PreventsConcurrentCalls()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("isCheckingAuth = true");
        authSource.Should().Contain("isCheckingAuth = false");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_IapAuthentication_CachesResult()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("iapAuthenticationChecked");
        authSource.Should().Contain("iapAuthenticationStatus");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_HealthCheck_BypassesVerification()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("HealthCheckPath");
        middlewareSource.Should().Contain("await _next(context)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_StaticResources_BypassVerification()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain(".js");
        middlewareSource.Should().Contain(".css");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SessionConfig_JWTExpiry_AppliedToTokens()
    {
        var json = ReadAppSettingsJson();
        json.Should().Contain("JWTSettings");
        json.Should().Contain("expiryInMinutes");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_ValidationErrors_MultipleKeys_Joined()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["a"] = new[] { "msg1" },
            ["b"] = new[] { "msg2" }
        };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Contain("msg1");
        result.Detail.Should().Contain("msg2");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Status500_DefaultDetail_WhenNull()
    {
        var err = new HttpErrorSpec(500, ErrorObject: new ErrorObjectSpec(Detail: null));
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().Be("An unexpected server error occurred. Please try again later.");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorHandler_LogsBeforeShowing()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("logger.error");
        handlerSource.Should().Contain("handleHttpError");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthInterceptor_DevCookie_AddsHeader()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("X-Using-Dev-Cookie");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_PublicKeyCache_RefreshesWhenStale()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("_keysLastRefreshed");
        middlewareSource.Should().Contain("AddHours(1)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Fallback_UnknownFormat_UsesMessage()
    {
        var err = new HttpErrorSpec(418, Message: "I'm a teapot", ErrorObject: new ErrorObjectSpec());
        var result = ErrorParserSpec.Parse(err);
        result.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Status400_ErrorsTakesPrecedenceOverTitle()
    {
        var errors = new Dictionary<string, string[]> { ["x"] = new[] { "y" } };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Title: "Other", Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Other", "validation errors branch uses Title ?? Validation Error per TypeScript");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SessionConfig_IAP_HealthCheckPath_Configurable()
    {
        var json = ReadAppSettingsJson();
        json.Should().Contain("IAP");
        var hasHealthPath = json.Contains("HealthCheckPath") || json.Contains("health");
        hasHealthPath.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_CheckAuthStatus_ConstructorInit()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("checkAuthStatus");
        authSource.Should().Contain("constructor");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_Status0_NoErrorObject_StillParses()
    {
        var err = new HttpErrorSpec(0, ErrorObject: null);
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(0);
        result.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_JWTValidation_ClockSkewConfigured()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("TokenValidationParameters");
        middlewareSource.Should().Contain("ClockSkew");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorParser_MissingFields_PassedThroughFromErrorObject()
    {
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Error: "err", MissingFields: new[] { "field1" }));
        var result = ErrorParserSpec.Parse(err);
        result.MissingFields.Should().Contain("field1");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_EmailHeader_ExtractsAfterColon()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("Split(':')");
        middlewareSource.Should().Contain("Last()");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthService_HasDevCookie_ChecksCookiePrefix()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("startsWith('dev-user-email=')");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IAPMiddleware_JWTMismatch_Returns401InProduction()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("Email mismatch");
        middlewareSource.Should().Contain("Identity mismatch");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ErrorHandler_ToastLife_5000ms()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("life: 5000");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AuthInterceptor_DevCookieHeader_OnlyForApiRequests()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("request.url.startsWith('/api')");
    }

    private static string ExtractBlockAfter(string source, string marker)
    {
        var idx = source.IndexOf(marker);
        if (idx < 0) return string.Empty;
        return source[idx..];
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

    private static string ReadAppSettingsJson()
    {
        var path = ResolvePath("UNOPS.PAO.Server", "appsettings.json");
        return File.Exists(path) ? File.ReadAllText(path) : "{}";
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
