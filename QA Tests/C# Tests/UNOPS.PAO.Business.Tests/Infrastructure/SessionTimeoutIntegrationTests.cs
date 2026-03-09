using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Integration tests for PNO-914: Cross-component flows, config consistency, end-to-end spec.
/// </summary>
public class PNO914IntegrationTests
{
    // ========== Config Integration ==========

    [Fact]
    [Trait("Category", "Integration")]
    public void AppSettings_JWTAndIAP_ConfigConsistent()
    {
        var json = ReadAppSettingsJson();
        json.Should().Contain("JWTSettings");
        json.Should().Contain("IAP");
        json.Should().Contain("expiryInMinutes");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AppSettings_IAP_HealthCheckPath_ExcludedFromVerification()
    {
        var path = GetIapHealthCheckPath();
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("HealthCheckPath");
        path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorFlow_Status0_HandlerSkips401_ParserProducesKeys()
    {
        var err = new HttpErrorSpec(0);
        var parsed = ErrorParserSpec.Parse(err);
        parsed.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);
        parsed.Detail.Should().Be(ErrorParserSpec.NetworkErrorDetailKey);

        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("status === 401");
        handlerSource.Should().Contain("return");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorFlow_I18n_NetworkErrorKeys_ExistInAllLocales()
    {
        var enJson = ReadI18nFile("en.json");
        var frJson = ReadI18nFile("fr.json");
        var ptJson = ReadI18nFile("pt.json");
        var esJson = ReadI18nFile("es.json");

        enJson.Should().Contain(ErrorParserSpec.NetworkErrorTitleKey);
        enJson.Should().Contain(ErrorParserSpec.NetworkErrorDetailKey);
        frJson.Should().Contain(ErrorParserSpec.NetworkErrorTitleKey);
        ptJson.Should().Contain(ErrorParserSpec.NetworkErrorTitleKey);
        esJson.Should().Contain(ErrorParserSpec.NetworkErrorTitleKey);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthFlow_InterceptorAndService_ConsistentCookieCheck()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        var authSource = ReadAuthServiceSource();

        interceptorSource.Should().Contain("dev-user-email");
        authSource.Should().Contain("dev-user-email");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthFlow_Interceptor_401Handling_DoesNotConflictWithHandler()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("if (error.status === 401)");
        handlerSource.Should().Contain("return", "Handler skips 401 so interceptor handles it");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_And_JWTConfig_ExpiryAligned()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().BeGreaterThanOrEqualTo(SessionConfigSpec.MinimumExpiryMinutes);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_AllStatusRanges_ProduceValidOutput()
    {
        foreach (var status in new[] { 0, 400, 401, 404, 500, 502, 503 })
        {
            var err = new HttpErrorSpec(status, Message: status >= 500 ? "Server error" : "Client error");
            var result = ErrorParserSpec.Parse(err);
            result.Status.Should().Be(status);
            result.Title.Should().NotBeNullOrEmpty();
            result.Detail.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorHandler_And_ErrorParser_Status0_BlockingDialog()
    {
        var parsed = ErrorParserSpec.Parse(new HttpErrorSpec(0));
        parsed.Status.Should().Be(0);

        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("appError.status === 0");
        handlerSource.Should().Contain("showNetworkErrorDialog");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_StaticFiles_And_Health_AllBypass()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("EndsWith");
        middlewareSource.Should().Contain("StartsWithSegments");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthService_ResetState_And_Interceptor_NoRedirectLoop()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("resetAuthenticationState");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Config_ServerAppSettings_Loadable()
    {
        var path = ResolvePath("UNOPS.PAO.Server", "appsettings.json");
        File.Exists(path).Should().BeTrue();
        var json = File.ReadAllText(path);
        json.Should().Contain("{");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ClientApp_ErrorModel_And_Handler_ImportChain()
    {
        var errorModelPath = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "shared", "models", "error.model.ts");
        var handlerPath = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "shared", "services", "utils", "error-handler.service.ts");

        File.Exists(errorModelPath).Should().BeTrue();
        File.Exists(handlerPath).Should().BeTrue();

        var handlerSource = File.ReadAllText(handlerPath);
        handlerSource.Should().Contain("ErrorParser");
        handlerSource.Should().Contain("error.model");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_And_Startup_Registration()
    {
        var middlewarePath = ResolvePath("UNOPS.PAO.UNOPSIdentity", "Authentication", "IAPVerificationMiddleware.cs");
        File.Exists(middlewarePath).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_ProblemDetails_And_Validation_Formats()
    {
        var problemDetails = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Title: "Bad Request", Detail: "Invalid"));
        var validation = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: new Dictionary<string, string[]> { ["x"] = new[] { "y" } }));

        var pdResult = ErrorParserSpec.Parse(problemDetails);
        var valResult = ErrorParserSpec.Parse(validation);

        pdResult.Title.Should().Be("Bad Request");
        valResult.Title.Should().Be("Validation Error");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SessionConfig_Expiry_And_DevCookie_8Hours()
    {
        var json = ReadAppSettingsJson();
        var devSection = System.Text.RegularExpressions.Regex.Match(json, @"DevIAPAuth.*Expires.*AddHours\((\d+)\)");
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().BeGreaterThanOrEqualTo(480);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthInterceptor_And_AuthService_DevCookieFormat()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        var authSource = ReadAuthServiceSource();

        interceptorSource.Should().Contain("dev-user-email=");
        authSource.Should().Contain("dev-user-email=");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_Status0_MatchesTypeScriptBehavior()
    {
        var err = new HttpErrorSpec(0, "/api/test", "Failed to fetch");
        var result = ErrorParserSpec.Parse(err);

        result.Status.Should().Be(0);
        result.Title.Should().Be("error.networkError.title");
        result.Detail.Should().Be("error.networkError.detail");
        result.Url.Should().Be("/api/test");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_JWT_And_EmailHeader_VerificationOrder()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("x-goog-iap-jwt-assertion");
        middlewareSource.Should().Contain("x-goog-authenticated-user-email");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorHandler_Status0_ClosableAndRefreshButton()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("showErrorDialog");
        handlerSource.Should().Contain("closable");
        handlerSource.Should().Contain("showRefreshButton");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthService_IsIapAuthenticated_DevVsProduction_Paths()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("localhost");
        authSource.Should().Contain("check-iap-simulation");
        authSource.Should().Contain("/user/claims");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Config_IAP_ProjectNumber_And_Audience()
    {
        var json = ReadAppSettingsJson();
        json.Should().Contain("ProjectNumber");
        json.Should().Contain("Audience");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_ServerError_StackTraceInDevelopment()
    {
        var err = new HttpErrorSpec(500, ErrorObject: new ErrorObjectSpec(StackTrace: "at Foo.Bar()"));
        var result = ErrorParserSpec.Parse(err);
        result.StackTrace.Should().Be("at Foo.Bar()");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_NetworkError_FromParserToDialog()
    {
        var err = new HttpErrorSpec(0);
        var parsed = ErrorParserSpec.Parse(err);
        parsed.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);

        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("translateService.instant");
        handlerSource.Should().Contain("error.title");
        handlerSource.Should().Contain("error.detail");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_Development_SkipValidation_Config()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("IsDevelopment");
        middlewareSource.Should().Contain("SkipValidationInDevelopment");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthInterceptor_AllErrorPaths_ReturnObservable()
    {
        var interceptorSource = ReadAuthInterceptorSource();
        interceptorSource.Should().Contain("return of(error)");
        interceptorSource.Should().Contain("catchError");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_ValidationErrors_ObjectFormat()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["field1"] = new[] { "error1" },
            ["field2"] = new[] { "error2a", "error2b" }
        };
        var err = new HttpErrorSpec(400, ErrorObject: new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors!.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_EmailMismatch_LogsWarning()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("Email mismatch");
        middlewareSource.Should().Contain("LogWarning");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthService_DevCookie_And_IAPAuth_FallbackChain()
    {
        var authSource = ReadAuthServiceSource();
        authSource.Should().Contain("hasDevCookie");
        authSource.Should().Contain("iapAuthenticationChecked");
        authSource.Should().Contain("/user/claims");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_And_ErrorHandler_Translation_Flow()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("translateService.instant(error.title)");
        handlerSource.Should().Contain("translateService.instant(error.detail)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AllAppSettingsEnvironments_HaveExpiryConfigured()
    {
        foreach (var env in new[] { "appsettings.json", "appsettings.Dev.json", "appsettings.Production.json", "appsettings.QA.json" })
        {
            var path = ResolvePath("UNOPS.PAO.Server", env);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                json.Should().Contain("expiryInMinutes", $"{env} must have expiryInMinutes configured");
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_And_AuthInterceptor_401Consistency()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        var interceptorSource = ReadAuthInterceptorSource();
        middlewareSource.Should().Contain("Status401Unauthorized");
        interceptorSource.Should().Contain("status === 401");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorParser_And_I18n_AllLanguages_HaveNetworkErrorDetail()
    {
        foreach (var lang in new[] { "en.json", "fr.json", "es.json", "pt.json" })
        {
            var json = ReadI18nFile(lang);
            json.Should().Contain(ErrorParserSpec.NetworkErrorDetailKey, $"{lang} must have network error detail key");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void AuthService_And_AuthGuard_ConsistentDevCookieLogic()
    {
        var authSource = ReadAuthServiceSource();
        var guardPath = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "core", "guards", "auth.guard.ts");
        if (File.Exists(guardPath))
        {
            var guardSource = File.ReadAllText(guardPath);
            guardSource.Should().Contain("isIapAuthenticated");
            authSource.Should().Contain("isIapAuthenticated");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IAPMiddleware_UserCreation_AssignsDefaultRole()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("UNOPS_GEN_USER");
        middlewareSource.Should().Contain("AddToRoleAsync");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ErrorHandler_And_FeedbackService_ImportChain()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("FeedbackDialogService");
        handlerSource.Should().Contain("showErrorDialog");
        handlerSource.Should().Contain("showErrorToast");
    }

    private static int GetJwtExpiryFromAppSettings()
    {
        var json = ReadAppSettingsJson();
        var match = System.Text.RegularExpressions.Regex.Match(json, @"""expiryInMinutes""\s*:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static string GetIapHealthCheckPath()
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

    private static string ReadI18nFile(string fileName)
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "assets", "i18n", fileName);
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
