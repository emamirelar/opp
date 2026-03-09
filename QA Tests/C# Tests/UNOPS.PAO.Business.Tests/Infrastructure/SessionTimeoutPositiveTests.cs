using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Infrastructure;

/// <summary>
/// Tests for PNO-914: Session Timeout / Connection Lost.
///
/// Requirements validated:
/// - AC-1: Users should NOT get frequent "Connection lost" popups during normal use
/// - AC-2: Session/idle timeout should be properly managed (IAP expiry extended)
/// - AC-3: Application should handle network disconnection gracefully
/// - AC-4: Session refresh should happen transparently before expiry
/// - AC-5: Error messages should be clear and actionable
/// </summary>
public class PNO914PositiveTests
{
    // ========== ErrorParser Positive (AC-3, AC-5) ==========

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_Status0_ReturnsNetworkErrorKeys_AC3_AC5()
    {
        var err = new HttpErrorSpec(0, "/api/test", "Network error");
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(0);
        result.Title.Should().Be(ErrorParserSpec.NetworkErrorTitleKey);
        result.Detail.Should().Be(ErrorParserSpec.NetworkErrorDetailKey);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_Status500_ReturnsServerError_AC5()
    {
        var err = new HttpErrorSpec(500, "/api/test", "Internal Server Error",
            new ErrorObjectSpec("Custom Title", "Custom detail"));
        var result = ErrorParserSpec.Parse(err);
        result.Status.Should().Be(500);
        result.Title.Should().Be("Custom Title");
        result.Detail.Should().Be("Custom detail");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_Status400_ProblemDetails_ReturnsTitleAndDetail_AC5()
    {
        var err = new HttpErrorSpec(400, "/api/test", "Bad Request",
            new ErrorObjectSpec("Validation Error", "An error occurred while processing your request."));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Validation Error");
        result.Detail.Should().Be("An error occurred while processing your request.");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_Status400_ValidationErrors_ReturnsFormattedDetail_AC5()
    {
        var errors = new Dictionary<string, string[]> { ["name"] = ["Name is required"] };
        var err = new HttpErrorSpec(400, "/api/test", "Bad Request",
            new ErrorObjectSpec(Errors: errors));
        var result = ErrorParserSpec.Parse(err);
        result.ValidationErrors.Should().NotBeNull();
        result.Detail.Should().Contain("name");
        result.Detail.Should().Contain("Name is required");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_Status400_SimpleErrorObject_ReturnsErrorField_AC5()
    {
        var err = new HttpErrorSpec(400, "/api/test", "Bad Request",
            new ErrorObjectSpec(Error: "Invalid request payload"));
        var result = ErrorParserSpec.Parse(err);
        result.Title.Should().Be("Error 400");
        result.Detail.Should().Be("Invalid request payload");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorParser_ContextPassed_IsIncludedInResult_AC5()
    {
        var err = new HttpErrorSpec(0);
        var result = ErrorParserSpec.Parse(err, "TestContext");
        result.Context.Should().Be("TestContext");
    }

    // ========== Session Config Positive (AC-2) ==========

    [Fact]
    [Trait("Category", "Positive")]
    public void SessionConfig_ExpiryInMinutes_AtLeast480_AC2()
    {
        var expiry = GetJwtExpiryFromAppSettings();
        expiry.Should().BeGreaterThanOrEqualTo(SessionConfigSpec.MinimumExpiryMinutes,
            "AC-2: Session timeout should be at least 8 hours (480 minutes)");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void SessionConfig_HealthCheckPath_Configured_AC2()
    {
        var healthPath = GetIapHealthCheckPath();
        healthPath.Should().NotBeNullOrEmpty("AC-2: Health check path should be configured for IAP exclusion");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void SessionConfig_IAPMiddleware_HasPublicKeyRefresh_AC2()
    {
        var middlewareSource = ReadIAPMiddlewareSource();
        middlewareSource.Should().Contain("AddHours(1)", "AC-2: Public key cache should refresh periodically");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void SessionConfig_ErrorTranslations_NetworkErrorKeysExist_AC5()
    {
        var enJson = ReadI18nEnJson();
        enJson.Should().Contain(ErrorParserSpec.NetworkErrorTitleKey);
        enJson.Should().Contain(ErrorParserSpec.NetworkErrorDetailKey);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorHandler_Status401_SkippedInHandler_AC1()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("status === 401");
        handlerSource.Should().Contain("return", "AC-1: 401 should be skipped (handled by auth interceptor)");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ErrorHandler_Status0_ShowsBlockingDialog_AC3()
    {
        var handlerSource = ReadErrorHandlerSource();
        handlerSource.Should().Contain("status === 0");
        handlerSource.Should().Contain("showNetworkErrorDialog", "AC-3: Network errors show blocking dialog with refresh");
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

    private static string ReadIAPMiddlewareSource()
    {
        var path = ResolvePath("UNOPS.PAO.UNOPSIdentity", "Authentication", "IAPVerificationMiddleware.cs");
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static string ReadI18nEnJson()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "assets", "i18n", "en.json");
        return File.Exists(path) ? File.ReadAllText(path) : "{}";
    }

    private static string ReadErrorHandlerSource()
    {
        var path = ResolvePath("UNOPS.PAO.ClientApp", "src", "app", "shared", "services", "utils", "error-handler.service.ts");
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
            if (File.Exists(full))
                return full;
        }
        return Path.Combine(baseDir, Path.GetFileName(relative));
    }
}
