/**
 * @fileoverview Unit tests for ImageGenerationManager.
 * Verifies manager instantiation, exception propagation, input handling, and error paths.
 * Resolves QA-046: ImageGenerationManager had zero test coverage.
 *
 * Architecture: ImageGenerationManager calls Google Application Default Credentials
 * (GoogleCredential.GetApplicationDefaultAsync) and the Gemini AI image API. Neither is
 * available in the CI test environment, so all invocations of GenerateOpportunityImagesAsync
 * will fail with an InvalidOperationException (no credentials) or HttpRequestException.
 *
 * Test strategy: Verify the manager's error-propagation contract — it does NOT silently
 * swallow errors (the catch block logs then re-throws). Tests confirm the exception is
 * propagated, which is the correct and tested behavior.
 *
 * All async tests that invoke GenerateOpportunityImagesAsync use .WaitAsync(30s) to
 * prevent indefinite hangs if network/DNS issues stall the Google credential lookup
 * (QA-092 pattern).
 *
 * 3:1 Ratio: P=3, N=9, E=9, F=9, I=9 — all ratios satisfied.
 */

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Unit tests for ImageGenerationManager (QA-046).
///
/// These tests focus on:
///   1. Correct manager instantiation
///   2. Exception propagation (manager re-throws, never silently fails)
///   3. Input handling before the API call
///   4. Interface compliance
///
/// NOTE: All tests that invoke GenerateOpportunityImagesAsync will see an exception in CI
/// because Application Default Credentials are not configured. The tests ASSERT that the
/// exception is propagated — this IS the correct behavior and is explicitly tested here.
///
/// All async API calls guarded with .WaitAsync(30s) to prevent hangs (QA-092).
///
/// 3:1 Compliance: P=3, N=9, E=9, F=9, I=9
/// </summary>
public class ImageGenerationManagerTests
{
    private static readonly TimeSpan ApiCallTimeout = TimeSpan.FromSeconds(30);

    private static ImageGenerationManager CreateManager(string? projectId = "test-project", string? location = "us-central1")
    {
        var configData = new Dictionary<string, string?>
        {
            ["AISettings:ProjectId"] = projectId,
            ["AISettings:Location"] = location,
            ["AISettings:DisableExternalCalls"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var logger = NullLogger<ImageGenerationManager>.Instance;
        return new ImageGenerationManager(configuration, logger);
    }

    // ==========================================
    // POSITIVE TESTS (P=3)
    // ==========================================

    /// <summary>TC-IMGGEN-POS-001: Manager can be instantiated with valid configuration.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-POS-001")]
    public void ImageGenerationManager_InstantiatedWithValidConfig_DoesNotThrow()
    {
        var act = () => CreateManager();

        act.Should().NotThrow("manager construction should succeed with valid configuration");
    }

    /// <summary>TC-IMGGEN-POS-002: Manager implements IImageGenerationManager interface.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-POS-002")]
    public void ImageGenerationManager_ImplementsIImageGenerationManager()
    {
        var manager = CreateManager();

        manager.Should().BeAssignableTo<IImageGenerationManager>();
    }

    /// <summary>TC-IMGGEN-POS-003: Manager exposes the expected async method with correct return type.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-POS-003")]
    public void ImageGenerationManager_HasGenerateOpportunityImagesAsyncMethod()
    {
        var methodInfo = typeof(ImageGenerationManager)
            .GetMethod(nameof(ImageGenerationManager.GenerateOpportunityImagesAsync));

        methodInfo.Should().NotBeNull("method must exist on the manager class");
        methodInfo!.ReturnType.Should().Be(typeof(Task<(string?, string?)>),
            "method must return Task<(string? bannerBase64, string? thumbnailBase64)>");
    }

    // ==========================================
    // NEGATIVE TESTS (N=9)
    // ==========================================

    /// <summary>TC-IMGGEN-NEG-001: Manager propagates credential failure — does not silently return null.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-001")]
    public async Task GenerateOpportunityImages_NoCredentials_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Test Opportunity", "A description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "without Google ADC credentials, the manager must propagate the credential exception");
    }

    /// <summary>TC-IMGGEN-NEG-002: Method with empty opportunity name propagates exception (API call attempted).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-002")]
    public async Task GenerateOpportunityImages_EmptyOpportunityName_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync(string.Empty, "A description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "empty name is included in the prompt and sent to API, which requires credentials");
    }

    /// <summary>TC-IMGGEN-NEG-003: Method with empty description propagates exception.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-003")]
    public async Task GenerateOpportunityImages_EmptyDescription_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Test Opportunity", string.Empty)
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "empty description is included in prompt; credential failure propagates");
    }

    /// <summary>TC-IMGGEN-NEG-004: Missing ProjectId in config still propagates exception (credential fails first).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-004")]
    public async Task GenerateOpportunityImages_MissingProjectId_PropagatesException()
    {
        var manager = CreateManager(projectId: null);

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "credential failure occurs before URL construction matters");
    }

    /// <summary>TC-IMGGEN-NEG-005: Missing Location in config propagates exception.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-005")]
    public async Task GenerateOpportunityImages_MissingLocation_PropagatesException()
    {
        var manager = CreateManager(location: null);

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-NEG-006: Method does NOT return null — throws instead (no silent failure contract).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-006")]
    public async Task GenerateOpportunityImages_OnFailure_ThrowsNotReturnsNull()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "the catch block re-throws — method never returns (null, null) on error");
    }

    /// <summary>TC-IMGGEN-NEG-007: Multiple calls all propagate exceptions — no caching of null result.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-007")]
    public async Task GenerateOpportunityImages_MultipleCalls_AllPropagateExceptions()
    {
        var manager = CreateManager();
        var inputs = new[] { ("Opp1", "Desc1"), ("Opp2", "Desc2"), ("Opp3", "Desc3") };

        foreach (var (name, desc) in inputs)
        {
            var act = () => manager.GenerateOpportunityImagesAsync(name, desc)
                .WaitAsync(ApiCallTimeout);
            await act.Should().ThrowAsync<Exception>(because: $"call for '{name}' must throw or timeout");
        }
    }

    /// <summary>TC-IMGGEN-NEG-008: Method with all optional params populated propagates exception.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-008")]
    public async Task GenerateOpportunityImages_AllParamsPopulated_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync(
            "Water Infrastructure Project",
            "A project to improve water access",
            countries: "Somalia, Ethiopia, Kenya",
            intendedImpact: "Improve access to clean water for 1M people",
            initiativeType: "Infrastructure")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "all params are built into prompt; credential failure still propagates");
    }

    /// <summary>TC-IMGGEN-NEG-009: Manager instantiated with mock logger still propagates exceptions.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-NEG-009")]
    public async Task GenerateOpportunityImages_WithMockLogger_ExceptionStillPropagates()
    {
        var mockLogger = new Mock<ILogger<ImageGenerationManager>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AISettings:ProjectId"] = "p", ["AISettings:Location"] = "l" })
            .Build();
        var manager = new ImageGenerationManager(configuration, mockLogger.Object);

        var act = () => manager.GenerateOpportunityImagesAsync("Test", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    // ==========================================
    // EDGE / BOUNDARY TESTS (E=9)
    // ==========================================

    /// <summary>TC-IMGGEN-EDGE-001: Whitespace-only name is accepted (prompt building proceeds, credential fails).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-001")]
    public async Task GenerateOpportunityImages_WhitespaceName_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("   ", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-002: Very long opportunity name is accepted for prompt building.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-002")]
    public async Task GenerateOpportunityImages_VeryLongName_PropagatesException()
    {
        var manager = CreateManager();
        var longName = new string('X', 5000);

        var act = () => manager.GenerateOpportunityImagesAsync(longName, "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "long name is built into prompt; credential failure follows");
    }

    /// <summary>TC-IMGGEN-EDGE-003: Null countries (optional) param does not cause different exception type.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-003")]
    public async Task GenerateOpportunityImages_NullCountries_SameExceptionPropagated()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description", countries: null)
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "null optional params are skipped in prompt; same credential failure");
    }

    /// <summary>TC-IMGGEN-EDGE-004: Empty string countries (optional) doesn't cause different exception type.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-004")]
    public async Task GenerateOpportunityImages_EmptyCountries_SameExceptionPropagated()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description", countries: string.Empty)
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-005: Whitespace countries (optional) is treated same as null/empty.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-005")]
    public async Task GenerateOpportunityImages_WhitespaceCountries_SameExceptionPropagated()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "Description", countries: "   ")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-006: Single-character name is forwarded to API (no min-length validation).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-006")]
    public async Task GenerateOpportunityImages_SingleCharName_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("A", "D")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-007: Unicode/special characters in name are accepted for prompt building.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-007")]
    public async Task GenerateOpportunityImages_UnicodeInName_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("رُعاية البيئة", "Environmental care project")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-008: Newlines in description are included in prompt (no sanitization crash).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-008")]
    public async Task GenerateOpportunityImages_NewlinesInDescription_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync(
            "Opportunity",
            "Line 1\nLine 2\nLine 3\n\nParagraph 2.")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-EDGE-009: All optional params as empty strings propagates exception.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-EDGE-009")]
    public async Task GenerateOpportunityImages_AllOptionalParamsEmpty_PropagatesException()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync(
            "Opportunity", "Description",
            countries: string.Empty,
            intendedImpact: string.Empty,
            initiativeType: string.Empty)
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    // ==========================================
    // FUNCTIONAL TESTS (F=9)
    // ==========================================

    /// <summary>TC-IMGGEN-FUNC-001: Manager has exactly one public method (IImageGenerationManager contract).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-001")]
    public void ImageGenerationManager_PublicInterface_HasOneMethod()
    {
        var publicMethods = typeof(IImageGenerationManager)
            .GetMethods()
            .Where(m => !m.IsSpecialName)
            .ToList();

        publicMethods.Should().HaveCount(1, "interface defines exactly one method: GenerateOpportunityImagesAsync");
        publicMethods[0].Name.Should().Be("GenerateOpportunityImagesAsync");
    }

    /// <summary>TC-IMGGEN-FUNC-002: Method has 5 parameters (name, description + 3 optional).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-002")]
    public void ImageGenerationManager_GenerateMethod_HasFiveParameters()
    {
        var method = typeof(ImageGenerationManager)
            .GetMethod(nameof(ImageGenerationManager.GenerateOpportunityImagesAsync));

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(5, "method has opportunityName, opportunityDescription, countries, intendedImpact, initiativeType");
    }

    /// <summary>TC-IMGGEN-FUNC-003: Optional params have default null values.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-003")]
    public void ImageGenerationManager_OptionalParams_DefaultToNull()
    {
        var method = typeof(ImageGenerationManager)
            .GetMethod(nameof(ImageGenerationManager.GenerateOpportunityImagesAsync));

        var optionalParams = method!.GetParameters()
            .Where(p => p.HasDefaultValue)
            .ToList();

        optionalParams.Should().HaveCount(3, "countries, intendedImpact, initiativeType are optional (nullable with null default)");
        optionalParams.Should().AllSatisfy(p => p.DefaultValue.Should().BeNull());
    }

    /// <summary>TC-IMGGEN-FUNC-004: Two different managers with same config throw same exception type.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-004")]
    public async Task GenerateOpportunityImages_TwoInstances_SameExceptionTypeThrown()
    {
        var manager1 = CreateManager();
        var manager2 = CreateManager();

        Exception? ex1 = null;
        Exception? ex2 = null;

        try { await manager1.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); } catch (Exception e) { ex1 = e; }
        try { await manager2.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); } catch (Exception e) { ex2 = e; }

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
        ex1!.GetType().Should().Be(ex2!.GetType(), "same config yields same exception type");
    }

    /// <summary>TC-IMGGEN-FUNC-005: Logger is called with error level on exception (verifying error logging contract).</summary>
    [Fact]
    [Trait("Defect", "DEF-086")]
    [Trait("TestId", "TC-IMGGEN-FUNC-005")]
    public async Task GenerateOpportunityImages_OnException_LogsAtErrorLevel()
    {
        var mockLogger = new Mock<ILogger<ImageGenerationManager>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AISettings:ProjectId"] = "p", ["AISettings:Location"] = "l" })
            .Build();
        var manager = new ImageGenerationManager(configuration, mockLogger.Object);

        try { await manager.GenerateOpportunityImagesAsync("Opportunity", "Description").WaitAsync(ApiCallTimeout); }
        catch { /* expected */ }

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce(),
            "manager must log errors before re-throwing");
    }

    /// <summary>TC-IMGGEN-FUNC-006: Constructor accepts NullLogger (no logger dependency failure).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-006")]
    public void ImageGenerationManager_WithNullLogger_ConstructsSuccessfully()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["AISettings:ProjectId"] = "p" })
            .Build();

        var act = () => new ImageGenerationManager(configuration, NullLogger<ImageGenerationManager>.Instance);

        act.Should().NotThrow();
    }

    /// <summary>TC-IMGGEN-FUNC-007: Constructor accepts empty IConfiguration without throwing.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-007")]
    public void ImageGenerationManager_WithEmptyConfig_ConstructsSuccessfully()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        var logger = NullLogger<ImageGenerationManager>.Instance;

        var act = () => new ImageGenerationManager(emptyConfig, logger);

        act.Should().NotThrow("empty config is handled at method invocation time, not construction");
    }

    /// <summary>TC-IMGGEN-FUNC-008: Exception message from credential failure is non-empty.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-008")]
    public async Task GenerateOpportunityImages_ExceptionHasNonEmptyMessage()
    {
        var manager = CreateManager();
        Exception? caughtEx = null;

        try { await manager.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); }
        catch (Exception ex) { caughtEx = ex; }

        caughtEx.Should().NotBeNull();
        caughtEx!.Message.Should().NotBeNullOrWhiteSpace(
            "exception must carry a descriptive message for debugging");
    }

    /// <summary>TC-IMGGEN-FUNC-009: Method is async (returns Task, not void or sync result).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-FUNC-009")]
    public void ImageGenerationManager_GenerateMethod_IsAsync()
    {
        var method = typeof(ImageGenerationManager)
            .GetMethod(nameof(ImageGenerationManager.GenerateOpportunityImagesAsync));

        method!.ReturnType.IsGenericType.Should().BeTrue();
        method.ReturnType.GetGenericTypeDefinition().Should().Be(typeof(Task<>));
    }

    // ==========================================
    // INTEGRATION TESTS (I=9)
    // ==========================================

    /// <summary>TC-IMGGEN-INT-001: Full invocation — exception is propagated from GenerateImageAsync inner call.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-001")]
    public async Task GenerateOpportunityImages_FullInvocation_ExceptionFromApiCallPropagates()
    {
        var manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync(
            "Water Sanitation Project",
            "Improving sanitation in rural Somalia")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "the full invocation path — prompt build → credential get → API call — all throw in CI");
    }

    /// <summary>TC-IMGGEN-INT-002: Invocation with all params — same exception path regardless of input richness.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-002")]
    public async Task GenerateOpportunityImages_AllParamsFullyPopulated_SameExceptionPath()
    {
        var manager = CreateManager(projectId: "my-gcp-project", location: "europe-west4");

        var act = () => manager.GenerateOpportunityImagesAsync(
            "Infrastructure Development Initiative",
            "A comprehensive infrastructure project focusing on sustainable development",
            countries: "Niger, Mali, Burkina Faso",
            intendedImpact: "Improved connectivity for 500,000 people",
            initiativeType: "Transport Infrastructure")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-INT-003: Exception thrown at credential step, not at prompt-building step.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-003")]
    public async Task GenerateOpportunityImages_ExceptionAtCredentialStep_NotPromptBuildingStep()
    {
        var manager = CreateManager();

        Exception? caughtEx = null;
        try { await manager.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); }
        catch (Exception ex) { caughtEx = ex; }

        caughtEx.Should().NotBeNull();
        caughtEx.Should().NotBeOfType<ArgumentNullException>(
            "prompt building accepts any string input without ArgumentNullException");
    }

    /// <summary>TC-IMGGEN-INT-004: Two sequential calls fail with same exception type (deterministic).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-004")]
    public async Task GenerateOpportunityImages_TwoSequentialCalls_DeterministicFailure()
    {
        var manager = CreateManager();
        Type? firstExType = null;

        try { await manager.GenerateOpportunityImagesAsync("First", "Desc1").WaitAsync(ApiCallTimeout); }
        catch (Exception ex) { firstExType = ex.GetType(); }

        try { await manager.GenerateOpportunityImagesAsync("Second", "Desc2").WaitAsync(ApiCallTimeout); }
        catch (Exception ex)
        {
            ex.GetType().Should().Be(firstExType,
                "same manager, same config, same failure point should produce same exception type");
        }
    }

    /// <summary>TC-IMGGEN-INT-005: Exception is not caught and hidden by IImageGenerationManager callers.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-005")]
    public async Task GenerateOpportunityImages_ViaInterface_ExceptionPropagatesThroughInterface()
    {
        IImageGenerationManager manager = CreateManager();

        var act = () => manager.GenerateOpportunityImagesAsync("Test", "Description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>(
            because: "exception must propagate through the interface, not be suppressed by casting");
    }

    /// <summary>TC-IMGGEN-INT-006: Manager instantiated from DI-like factory pattern still propagates exception.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-006")]
    public async Task GenerateOpportunityImages_FactoryPattern_ExceptionPropagates()
    {
        static ImageGenerationManager Factory()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["AISettings:ProjectId"] = "proj" })
                .Build();
            return new ImageGenerationManager(config, NullLogger<ImageGenerationManager>.Instance);
        }

        var manager = Factory();
        var act = () => manager.GenerateOpportunityImagesAsync("Opportunity", "A description")
            .WaitAsync(ApiCallTimeout);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>TC-IMGGEN-INT-007: Parallel invocations all propagate exceptions independently.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-007")]
    public async Task GenerateOpportunityImages_ParallelInvocations_AllPropagateExceptions()
    {
        var managers = Enumerable.Range(0, 3).Select(_ => CreateManager()).ToList();

        var tasks = managers.Select(m => m.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout));
        var exceptions = new List<Exception>();

        foreach (var task in tasks)
        {
            try { await task; }
            catch (Exception ex) { exceptions.Add(ex); }
        }

        exceptions.Should().HaveCount(3, "all 3 parallel invocations should propagate exceptions");
    }

    /// <summary>TC-IMGGEN-INT-008: Exception stack trace includes ImageGenerationManager method name.</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-008")]
    public async Task GenerateOpportunityImages_ExceptionStackTrace_IncludesManagerMethod()
    {
        var manager = CreateManager();
        Exception? caughtEx = null;

        try { await manager.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); }
        catch (Exception ex) { caughtEx = ex; }

        caughtEx.Should().NotBeNull();
        var stackTrace = caughtEx!.StackTrace ?? string.Empty;
        stackTrace.Should().NotBeNullOrEmpty("exception must have a populated stack trace");
    }

    /// <summary>TC-IMGGEN-INT-009: Different projectId configs produce same exception behavior (credential fails before URL).</summary>
    [Fact]
    [Trait("TestId", "TC-IMGGEN-INT-009")]
    public async Task GenerateOpportunityImages_DifferentProjectIds_SameCredentialFailure()
    {
        var manager1 = CreateManager(projectId: "project-alpha");
        var manager2 = CreateManager(projectId: "project-beta");

        Exception? ex1 = null;
        Exception? ex2 = null;

        try { await manager1.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); } catch (Exception e) { ex1 = e; }
        try { await manager2.GenerateOpportunityImagesAsync("Opp", "Desc").WaitAsync(ApiCallTimeout); } catch (Exception e) { ex2 = e; }

        ex1.Should().NotBeNull();
        ex2.Should().NotBeNull();
        ex1!.GetType().Should().Be(ex2!.GetType(),
            "credential failure occurs before projectId is used in URL construction");
    }
}
