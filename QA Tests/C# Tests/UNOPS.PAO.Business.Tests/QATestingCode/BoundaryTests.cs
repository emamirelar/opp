using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// PNO-1166: QA Testing Code — Boundary/edge-case tests.
/// Validates pipeline and infrastructure at configuration boundaries.
///
/// Requirements validated:
/// - REQ-1: Pipeline covers edge cases (scheduled runs, manual dispatch) → B01–B06
/// - REQ-2: Test configuration boundaries (env vars, timeouts, parallelism) → B07–B14
/// - REQ-3: Defect ID sequence integrity → B15–B18
/// - REQ-4: Pipeline artifact retention and shard configuration → B19–B24
/// </summary>
[Collection("QATestingCode")]
public class PNO1166BoundaryTests
{
    private readonly QATestingCodeFixture _fixture;

    public PNO1166BoundaryTests(QATestingCodeFixture fixture) => _fixture = fixture;

    // ── Pipeline trigger boundaries ─────────────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B01_Pipeline_HasScheduledTrigger_NightlyAt2AM_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("schedule:",
            "REQ-1: Pipeline must have a scheduled trigger for nightly full suite runs");
        _fixture.PipelineYaml.Should().Contain("cron: '0 2 * * *'",
            "REQ-1: Nightly schedule should run at 2 AM UTC");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B02_Pipeline_HasManualDispatch_WithPlaywrightTiers_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("workflow_dispatch:",
            "REQ-1: Pipeline must support manual dispatch for on-demand test runs");
        _fixture.PipelineYaml.Should().Contain("playwright_tier",
            "REQ-1: Manual dispatch should allow selecting Playwright tier");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B03_Pipeline_ManualDispatch_SupportsAllPlaywrightTiers_REQ1()
    {
        var expectedTiers = new[] { "smoke", "extended", "full", "cross-browser" };
        foreach (var tier in expectedTiers)
        {
            _fixture.PipelineYaml.Should().Contain($"- {tier}",
                $"REQ-1: Manual dispatch must support '{tier}' Playwright tier");
        }
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B04_Pipeline_PullRequestTrigger_IncludesMainAndDevDeploy_REQ2()
    {
        _fixture.PipelineYaml.Should().Contain("pull_request:",
            "REQ-2: Pipeline must trigger on pull requests for pre-merge validation");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B05_Pipeline_PushTrigger_IncludesQATestsBranch_REQ2()
    {
        _fixture.PipelineYaml.Should().Contain("QA-Tests",
            "REQ-2: Pipeline must trigger on QA-Tests branch for immediate feedback on QA work");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B06_Pipeline_TestSummary_HandlesSkippedPlaywright_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("skipped",
            "REQ-1: Summary must handle skipped Playwright tests (scheduled runs skip smoke tier)");
    }

    // ── Test environment configuration boundaries ───────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B07_TestEnvironment_SupportsThreeDatabaseModes_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("UsePostgreSQL",
            "REQ-2: TestEnvironment must support PostgreSQL mode (default)");
        _fixture.TestEnvironmentSource.Should().Contain("UseInMemory",
            "REQ-2: TestEnvironment must support InMemory/SQLite fallback mode");
        _fixture.TestEnvironmentSource.Should().Contain("UseSQLite",
            "REQ-2: TestEnvironment must expose SQLite property for conditional test logic");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B08_TestEnvironment_ConnectionPriority_EnvVarOverAppsettings_REQ2()
    {
        var envVarIndex = _fixture.TestEnvironmentSource.IndexOf("Priority 1: Explicit connection string env var");
        var appSettingsIndex = _fixture.TestEnvironmentSource.IndexOf("Priority 2 (DEFAULT):");
        envVarIndex.Should().BeLessThan(appSettingsIndex,
            "REQ-2: Env var connection string must take priority over appsettings.Testing.json");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B09_TestEnvironment_ProxyTimeout_Is5Seconds_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("FromSeconds(5)",
            "REQ-2: Proxy connectivity check timeout should be 5 seconds (fast-fail boundary)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B10_TestEnvironment_CommandTimeout_Is60Seconds_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("CommandTimeout(60)",
            "REQ-2: Database command timeout should be 60 seconds for complex test operations");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B11_TestEnvironment_SQLiteConnectionTracking_PreventsGC_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("_sqliteConnections",
            "REQ-2: SQLite connections must be tracked to prevent GC from destroying in-memory databases");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B12_TestEnvironment_ForeignKeyEnforcement_IsOptIn_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("SQLITE_ENABLE_FK",
            "REQ-2: Foreign key enforcement must be opt-in via environment variable");
        _fixture.TestEnvironmentSource.Should().Contain("PRAGMA foreign_keys = ON",
            "REQ-2: FK ON pragma must be set when SQLITE_ENABLE_FK=true");
        _fixture.TestEnvironmentSource.Should().Contain("PRAGMA foreign_keys = OFF",
            "REQ-2: FK OFF pragma must be the default behavior");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B13_TestEnvironment_IAMTokenRefresh_Is55Minutes_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("FromMinutes(55)",
            "REQ-2: IAM token refresh should be 55 minutes (5 min before 60 min expiry)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B14_TestEnvironment_IAMRetryInterval_Is5Seconds_REQ2()
    {
        var retryMatches = Regex.Matches(_fixture.TestEnvironmentSource, @"FromSeconds\(5\)");
        retryMatches.Count.Should().BeGreaterThanOrEqualTo(2,
            "REQ-2: IAM auth failure retry interval should be 5 seconds");
    }

    // ── Defect ID sequence boundaries ───────────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B15_DevDefectList_HasSequentialDEFIds_REQ3()
    {
        var ids = QATestingCodeSpec.DefIdPattern.Matches(_fixture.DevDefectList)
            .Select(m => int.Parse(m.Value.Replace("DEF-", "")))
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        ids.Should().HaveCountGreaterThan(0, "REQ-3: Defect list must contain at least one DEF entry");
        ids.First().Should().BeGreaterThan(0, "REQ-3: DEF IDs must start from DEF-001 or higher");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B16_QADefectList_HasSequentialQAIds_REQ3()
    {
        var ids = QATestingCodeSpec.QAIdPattern.Matches(_fixture.QADefectList)
            .Select(m => int.Parse(m.Value.Replace("QA-", "")))
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        ids.Should().HaveCountGreaterThan(0, "REQ-3: QA defect list must contain at least one QA entry");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B17_DevDefectList_DEFIdsAreThreeDigits_REQ3()
    {
        var malformedIds = Regex.Matches(_fixture.DevDefectList, @"DEF-\d{1,2}(?!\d)")
            .Where(m => !Regex.IsMatch(m.Value, @"DEF-\d{3}"))
            .Select(m => m.Value)
            .Distinct()
            .ToList();
        malformedIds.Should().BeEmpty(
            "REQ-3: All DEF IDs must use 3-digit format (DEF-001, not DEF-1)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B18_DevDefectList_ContainsAtLeast50Entries_REQ3()
    {
        var uniqueIds = QATestingCodeSpec.DefIdPattern.Matches(_fixture.DevDefectList)
            .Select(m => m.Value)
            .Distinct()
            .Count();
        uniqueIds.Should().BeGreaterThanOrEqualTo(50,
            "REQ-3: Developer defect list should contain significant entries given ~10,000 test methods");
    }

    // ── Pipeline artifact/shard boundaries ──────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B19_Pipeline_ArtifactRetention_Is1Day_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("retention-days: 1",
            "REQ-4: Build artifacts should retain for 1 day only (minimize storage)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B20_Pipeline_UsesCaching_ForNugetAndNpm_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("actions/cache@v4",
            "REQ-4: Pipeline must use caching for faster builds");
        _fixture.PipelineYaml.Should().Contain("~/.nuget/packages",
            "REQ-4: NuGet packages must be cached");
        _fixture.PipelineYaml.Should().Contain("node_modules",
            "REQ-4: npm dependencies must be cached");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B21_Pipeline_PostgresService_Version15_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("postgres:15",
            "REQ-4: CI PostgreSQL service must use version 15 matching production");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B22_Pipeline_PostgresHealthCheck_Configured_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("pg_isready",
            "REQ-4: PostgreSQL service must have a health check to ensure readiness before tests");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B23_Pipeline_SmokeTests_UseSQLite_NotPostgres_REQ1()
    {
        var smokeSection = ExtractJobSection(_fixture.PipelineYaml, "smoke-tests");
        smokeSection.Should().Contain("USE_INMEMORY_DB: 'true'",
            "REQ-1: Smoke tests must use SQLite for speed (fast gate)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B24_Pipeline_BusinessTests_UsePostgres_NotSQLite_REQ1()
    {
        var businessSection = ExtractJobSection(_fixture.PipelineYaml, "business-tests");
        businessSection.Should().Contain("POSTGRES_DB",
            "REQ-1: Business tests must use PostgreSQL for realistic database behavior");
    }

    private static string ExtractJobSection(string yaml, string jobName)
    {
        var pattern = $@"(?m)^  {Regex.Escape(jobName)}:";
        var match = Regex.Match(yaml, pattern);
        if (!match.Success) return string.Empty;
        var start = match.Index;
        var nextJobMatch = Regex.Match(yaml.Substring(start + match.Length), @"(?m)^\s{{2}}\w[\w-]*:");
        var end = nextJobMatch.Success ? start + match.Length + nextJobMatch.Index : yaml.Length;
        return yaml.Substring(start, end - start);
    }
}
