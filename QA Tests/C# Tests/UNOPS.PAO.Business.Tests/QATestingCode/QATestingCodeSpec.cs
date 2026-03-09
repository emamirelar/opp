/// <summary>
/// PNO-1166: QA Testing Code — Specification helpers.
///
/// Business goal: "To know within minutes if any existing functionality is broken."
///
/// Requirements validated:
/// - REQ-1: CI/CD pipeline executes all test suites automatically on push/PR
/// - REQ-2: QA tests integrated into daily workflow (triggers on main, dev-deploy, QA-Tests)
/// - REQ-3: High-priority bugs tracked via DEF-XXX defects with CI-visible traits
/// - REQ-4: Pipeline passes without infrastructure errors (build, checkout, submodules)
/// - REQ-5: Build warnings addressed (submodule-related)
/// - REQ-6: Submodule checkout works with GH_PAT token in GitHub Actions
///
/// QA observations (Anusha, 2026-03-05):
/// - UNOPS.Workflow warning deferred to Rosnier
/// - Submodule checkout needs PAT token — waiting on DevOps
/// - CI/CD pipeline issues addressed as of March 3, 2026
///
/// Defects found:
/// - DEF-020: GH_PAT required for submodule checkout (credential management)
/// </summary>

using System.Text.RegularExpressions;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

public static class QATestingCodeSpec
{
    public static string ResolvePath(params string[] segments)
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

    public static string ReadFileOrEmpty(string path)
        => File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    // ── Pipeline constants ──────────────────────────────────────────────────
    public const string PipelineFile = ".github/workflows/qa-tests.yml";

    public static readonly string[] RequiredPipelineJobs = new[]
    {
        "dotnet-build", "angular-build",
        "smoke-tests", "fast-tests", "business-tests",
        "presentation-tests", "frontend-tests",
        "playwright-smoke", "defect-tests", "test-summary"
    };

    public static readonly string[] RequiredTriggerBranches = new[]
    {
        "main", "dev-deploy", "QA-Tests"
    };

    public static readonly string[] RequiredPipelineArtifacts = new[]
    {
        "dotnet-build", "angular-dist",
        "fast-tests-results", "business-tests-results",
        "presentation-tests-results", "defect-tests-results"
    };

    // ── Test project constants ──────────────────────────────────────────────
    public static readonly string[] TestProjectPaths = new[]
    {
        "QA Tests/C# Tests/UNOPS.PAO.Business.Tests/UNOPS.PAO.Business.Tests.csproj",
        "QA Tests/C# Tests/UNOPS.PAO.Presentation.Tests/UNOPS.PAO.Presentation.Tests.csproj",
        "QA Tests/C# Tests/UNOPS.PAO.FastTests/UNOPS.PAO.FastTests.csproj",
    };

    public static readonly string[] RequiredXunitPackages = new[]
    {
        "xunit", "xunit.runner.visualstudio", "FluentAssertions", "Microsoft.NET.Test.Sdk"
    };

    public const string TargetFramework = "net9.0";
    public const string DotnetVersion = "9.0.x";
    public const string NodeVersion = "20";

    // ── Submodule constants ─────────────────────────────────────────────────
    public static readonly string[] RequiredSubmodules = new[]
    {
        "UNOPS.PAO.ExternalDataService",
        "UNOPS.Workflow"
    };

    // ── Defect filter constants ─────────────────────────────────────────────
    public const string DefectExcludeFilter = "Defect!~DEF";
    public const string DefectIncludeFilter = "Defect~DEF";

    // ── Test infrastructure files ───────────────────────────────────────────
    public static readonly string[] RequiredTestBaseFiles = new[]
    {
        "TestEnvironment.cs",
        "TestEntityBuilder.cs",
        "TestDataHelper.cs",
    };

    public static readonly string[] RequiredEnvironmentVariables = new[]
    {
        "USE_INMEMORY_DB", "TEST_DB_CONNECTION_STRING", "SQLITE_ENABLE_FK"
    };

    // ── Defect list constants ───────────────────────────────────────────────
    public const string DevDefectListFile = "QA Tests/Defect List for Developers.md";
    public const string QADefectListFile = "QA Tests/Defect List for QA.md";
    public static readonly Regex DefIdPattern = new(@"DEF-\d{3}", RegexOptions.Compiled);
    public static readonly Regex QAIdPattern = new(@"QA-\d{3}", RegexOptions.Compiled);

    // ── xUnit trait constants ───────────────────────────────────────────────
    public static readonly string[] ValidCategoryTraitValues = new[]
    {
        "Positive", "Negative", "Boundary", "Functional", "Integration",
        "Unit", "Concurrency", "Performance", "Load", "DataEntryPermutation",
        "Security", "ApiContract", "Accessibility", "i18n", "ErrorRecovery",
        "RateLimiting", "Smoke"
    };

    // ── Pipeline job dependency validation ──────────────────────────────────
    public static Dictionary<string, string[]> ExpectedJobDependencies = new()
    {
        ["smoke-tests"] = new[] { "dotnet-build" },
        ["fast-tests"] = new[] { "dotnet-build" },
        ["business-tests"] = new[] { "dotnet-build" },
        ["presentation-tests"] = new[] { "dotnet-build" },
        ["defect-tests"] = new[] { "dotnet-build" },
        ["frontend-tests"] = new[] { "angular-build" },
        ["playwright-smoke"] = new[] { "angular-build" },
    };
}
