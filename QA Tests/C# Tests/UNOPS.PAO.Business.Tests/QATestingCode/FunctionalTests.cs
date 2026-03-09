using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// PNO-1166: QA Testing Code — Functional tests.
/// Validates business rules, pipeline logic, and infrastructure conventions.
///
/// Requirements validated:
/// - REQ-1: Pipeline correctly orchestrates build-then-test flow → F01–F06
/// - REQ-2: Test infrastructure supports daily developer workflow → F07–F12
/// - REQ-3: Defect management integrates with CI filtering → F13–F18
/// - REQ-4: Pipeline optimizations applied correctly → F19–F24
/// </summary>
[Collection("QATestingCode")]
public class PNO1166FunctionalTests
{
    private readonly QATestingCodeFixture _fixture;

    public PNO1166FunctionalTests(QATestingCodeFixture fixture) => _fixture = fixture;

    // ── Pipeline orchestration rules ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F01_Pipeline_TestJobs_DependOnBuildJobs_REQ1()
    {
        foreach (var (job, deps) in QATestingCodeSpec.ExpectedJobDependencies)
        {
            var section = ExtractJobSection(_fixture.PipelineYaml, job);
            foreach (var dep in deps)
            {
                section.Should().Contain(dep,
                    $"REQ-1: Job '{job}' must depend on '{dep}' to ensure build completes first");
            }
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F02_Pipeline_BuildStage_RunsBeforeTestStage_REQ1()
    {
        var buildIndex = _fixture.PipelineYaml.IndexOf("dotnet-build:");
        var testIndex = _fixture.PipelineYaml.IndexOf("smoke-tests:");
        buildIndex.Should().BeLessThan(testIndex,
            "REQ-1: Build stage must be defined before test stage in pipeline");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F03_Pipeline_AllTestJobs_PublishResults_REQ1()
    {
        var resultPublishers = Regex.Matches(_fixture.PipelineYaml, @"dorny/test-reporter@v1").Count;
        resultPublishers.Should().BeGreaterThanOrEqualTo(4,
            "REQ-1: At least 4 test jobs must publish results via test-reporter for visibility");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F04_Pipeline_AllTestJobs_GenerateTrxFiles_REQ1()
    {
        var trxLoggers = Regex.Matches(_fixture.PipelineYaml, @"--logger ""trx").Count;
        trxLoggers.Should().BeGreaterThanOrEqualTo(5,
            "REQ-1: All dotnet test jobs must generate .trx result files for reporting");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F05_Pipeline_TestSummary_DependsOnAllCriticalJobs_REQ1()
    {
        var summarySection = ExtractJobSection(_fixture.PipelineYaml, "test-summary");
        var criticalJobs = new[] { "smoke-tests", "fast-tests", "business-tests", "presentation-tests" };
        foreach (var job in criticalJobs)
        {
            summarySection.Should().Contain(job,
                $"REQ-1: Test summary must depend on '{job}' to aggregate all results");
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F06_Pipeline_TestSummary_RunsAlways_REQ1()
    {
        var summarySection = ExtractJobSection(_fixture.PipelineYaml, "test-summary");
        summarySection.Should().Contain("if: always()",
            "REQ-1: Test summary must run even when some jobs fail (always() condition)");
    }

    // ── Test infrastructure workflow support ─────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F07_TestEnvironment_SupportsISUNOPSOverride_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("IsUNOPSOverride",
            "REQ-2: Test configuration must include IsUNOPSOverride for UNOPS-specific testing");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F08_TestEnvironment_DisablesExternalCalls_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("DisableExternalCalls",
            "REQ-2: Test configuration must disable external API calls for isolated testing");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F09_TestEnvironment_IncludesAISettings_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("AISettings:ModelName",
            "REQ-2: Test configuration must include AI settings for AI feature tests");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F10_TestEnvironment_IncludesGoogleCloudSettings_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("GoogleCloud:UseMockServices",
            "REQ-2: Test config must mock Google Cloud services to avoid external dependencies");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F11_TestEnvironment_CreatesAspNetUserStubs_ForSQLite_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("AspNetUsers",
            "REQ-2: SQLite mode must create AspNetUsers table stub for identity-related tests");
        _fixture.TestEnvironmentSource.Should().Contain("AspNetUserRoles",
            "REQ-2: SQLite mode must create AspNetUserRoles table stub for role-based tests");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F12_TestEntityBuilder_Exists_WithBuilders_REQ2()
    {
        var builderSource = _fixture.TestBaseFiles.GetValueOrDefault("TestEntityBuilder.cs", "");
        builderSource.Should().NotBeNullOrWhiteSpace(
            "REQ-2: TestEntityBuilder must exist for fluent test data creation");
        var expectedBuilders = new[] { "UserBuilder", "PartnerBuilder", "OpportunityBuilder" };
        foreach (var builder in expectedBuilders)
        {
            builderSource.Should().Contain(builder,
                $"REQ-2: TestEntityBuilder must include {builder} for standardized data creation");
        }
    }

    // ── Defect/CI filter integration ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F13_Pipeline_DefectJob_UsesCorrectIncludeFilter_REQ3()
    {
        var defectSection = ExtractJobSection(_fixture.PipelineYaml, "defect-tests");
        defectSection.Should().Contain(QATestingCodeSpec.DefectIncludeFilter,
            "REQ-3: Defect test job must use 'Defect~DEF' filter to run ONLY defect-tagged tests");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F14_Pipeline_DefectJob_ProducesSummary_REQ3()
    {
        var defectSection = ExtractJobSection(_fixture.PipelineYaml, "defect-tests");
        defectSection.Should().Contain("Known Defect Tests Report",
            "REQ-3: Defect test job must produce a summary report for visibility");
        defectSection.Should().Contain("informational",
            "REQ-3: Defect summary must clarify results are informational, not blocking");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F15_Pipeline_DefectJob_ExplainsMeaning_REQ3()
    {
        _fixture.PipelineYaml.Should().Contain("expected to FAIL",
            "REQ-3: Pipeline must explain that defect tests are expected to fail");
        _fixture.PipelineYaml.Should().Contain("developer has fixed the bug",
            "REQ-3: Pipeline must explain that passing defect tests mean the bug is fixed");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F16_DefectList_ContainsTableWithRequiredColumns_REQ3()
    {
        var requiredColumns = new[] { "ID", "Severity", "Title", "Description" };
        foreach (var col in requiredColumns)
        {
            _fixture.DevDefectList.Should().Contain(col,
                $"REQ-3: Developer defect list must have '{col}' column for standardized tracking");
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F17_Pipeline_BusinessTests_UsePostgreSQLService_REQ3()
    {
        var businessSection = ExtractJobSection(_fixture.PipelineYaml, "business-tests");
        businessSection.Should().Contain("services:",
            "REQ-3: Business tests must use a PostgreSQL service for realistic testing");
        businessSection.Should().Contain("postgres:",
            "REQ-3: Service must be PostgreSQL");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F18_Pipeline_IntegrationTests_DisableExternalCalls_REQ3()
    {
        var integrationSection = ExtractJobSection(_fixture.PipelineYaml, "integration-tests");
        integrationSection.Should().Contain("DisableExternalCalls",
            "REQ-3: Integration tests must disable external calls to prevent flaky failures");
    }

    // ── Pipeline optimizations ──────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F19_Pipeline_Uses_UbuntuLatest_ForAllJobs_REQ4()
    {
        var ubuntuCount = Regex.Matches(_fixture.PipelineYaml, @"runs-on:\s*ubuntu-latest").Count;
        ubuntuCount.Should().BeGreaterThanOrEqualTo(8,
            "REQ-4: All CI jobs should use ubuntu-latest runners for faster execution");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F20_Pipeline_AngularBuild_IncludesESLintCheck_REQ4()
    {
        var angularSection = ExtractJobSection(_fixture.PipelineYaml, "angular-build");
        angularSection.Should().Contain("eslint",
            "REQ-4: Angular build must include ESLint check for code quality");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F21_Pipeline_AngularBuild_IncludesSecurityAudit_REQ4()
    {
        var angularSection = ExtractJobSection(_fixture.PipelineYaml, "angular-build");
        angularSection.Should().Contain("npm audit",
            "REQ-4: Angular build must include npm security audit for vulnerability detection");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F22_Pipeline_FastTests_CollectCodeCoverage_REQ4()
    {
        var fastSection = ExtractJobSection(_fixture.PipelineYaml, "fast-tests");
        fastSection.Should().Contain("XPlat Code Coverage",
            "REQ-4: Fast tests should collect code coverage data for quality metrics");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F23_Pipeline_Summary_IncludesTroubleshootingGuide_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("Troubleshooting",
            "REQ-4: Pipeline summary must include troubleshooting steps when failures occur");
        _fixture.PipelineYaml.Should().Contain("dotnet test --configuration Release",
            "REQ-4: Troubleshooting must include local test run command");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F24_Pipeline_Summary_ListsOptimizationsApplied_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("Pipeline Optimizations Applied",
            "REQ-4: Summary must list pipeline optimizations for transparency");
        _fixture.PipelineYaml.Should().Contain("NuGet package caching",
            "REQ-4: NuGet caching must be documented as an optimization");
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
