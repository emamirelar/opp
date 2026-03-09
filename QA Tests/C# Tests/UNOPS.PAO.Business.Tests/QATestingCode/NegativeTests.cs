using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// PNO-1166: QA Testing Code — Negative tests.
/// Validates that infrastructure correctly handles missing, invalid, or incomplete configurations.
///
/// Requirements validated:
/// - REQ-1: Pipeline detects failures and reports them clearly → N01–N06
/// - REQ-3: Defect tracking enforces conventions → N07–N12
/// - REQ-4: Pipeline errors surfaced, not silently swallowed → N13–N18
/// - REQ-5: Build warnings tracked → N19–N21
/// - REQ-6: Missing GH_PAT detected → N22–N24
/// </summary>
[Collection("QATestingCode")]
public class PNO1166NegativeTests
{
    private readonly QATestingCodeFixture _fixture;

    public PNO1166NegativeTests(QATestingCodeFixture fixture) => _fixture = fixture;

    // ── Pipeline error handling ─────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N01_Pipeline_SmokeTests_FailOnError_IsTrue_REQ1()
    {
        var smokeSection = ExtractJobSection(_fixture.PipelineYaml, "smoke-tests");
        smokeSection.Should().Contain("fail-on-error: true",
            "REQ-1: Smoke test failures must block the pipeline to catch broken functionality immediately");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N02_Pipeline_DefectTests_ContinueOnError_IsTrue_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("defect-tests:");
        var defectSection = ExtractJobSection(_fixture.PipelineYaml, "defect-tests");
        defectSection.Should().Contain("continue-on-error: true",
            "REQ-1: Defect tests must NOT block PRs — they are informational");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N03_Pipeline_BusinessTests_ExcludesDefectTraitedTests_REQ1()
    {
        var businessSection = ExtractJobSection(_fixture.PipelineYaml, "business-tests");
        businessSection.Should().Contain(QATestingCodeSpec.DefectExcludeFilter,
            "REQ-1: Business tests must exclude DEF-tagged tests to avoid false failures");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N04_Pipeline_IntegrationTests_ExcludesDefectTraitedTests_REQ1()
    {
        var integrationSection = ExtractJobSection(_fixture.PipelineYaml, "integration-tests");
        integrationSection.Should().Contain(QATestingCodeSpec.DefectExcludeFilter,
            "REQ-1: Integration tests must exclude DEF-tagged tests to avoid false failures");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N05_Pipeline_TestSummary_ReportsFailed_Jobs_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("One or more test suites failed",
            "REQ-4: Pipeline summary must report when jobs fail, not silently pass");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N06_Pipeline_TestSummary_FailsOnCriticalTestFailure_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("exit 1",
            "REQ-4: Pipeline must exit with non-zero code when critical tests fail");
    }

    // ── Defect list conventions ─────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N07_DevDefectList_ContainsDEFPrefixedIds_REQ3()
    {
        QATestingCodeSpec.DefIdPattern.IsMatch(_fixture.DevDefectList).Should().BeTrue(
            "REQ-3: Developer defect list must contain DEF-XXX prefixed entries");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N08_QADefectList_ContainsQAPrefixedIds_REQ3()
    {
        QATestingCodeSpec.QAIdPattern.IsMatch(_fixture.QADefectList).Should().BeTrue(
            "REQ-3: QA defect list must contain QA-XXX prefixed entries");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N09_DevDefectList_DoesNotContain_QAPrefixed_Entries_REQ3()
    {
        var lines = _fixture.DevDefectList.Split('\n')
            .Where(l => l.TrimStart().StartsWith("| QA-"))
            .ToList();
        lines.Should().BeEmpty(
            "REQ-3: Developer defect list must NOT contain QA-XXX entries (those belong in QA defect list)");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N10_QADefectList_DoesNotContain_DEFPrefixed_TableRows_REQ3()
    {
        var tableRows = _fixture.QADefectList.Split('\n')
            .Where(l => l.TrimStart().StartsWith("| DEF-"))
            .ToList();
        tableRows.Should().BeEmpty(
            "REQ-3: QA defect list must NOT contain DEF-XXX table rows (those belong in developer defect list)");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N11_DevDefectList_ContainsSeverityIndicators_REQ3()
    {
        var hasSeverity = _fixture.DevDefectList.Contains("🔴") ||
                          _fixture.DevDefectList.Contains("🟠") ||
                          _fixture.DevDefectList.Contains("🟡") ||
                          _fixture.DevDefectList.Contains("🟢");
        hasSeverity.Should().BeTrue(
            "REQ-3: Developer defect list must use severity indicators (🔴🟠🟡🟢) for prioritization");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N12_DefectLists_ContainStatusColumn_REQ3()
    {
        _fixture.DevDefectList.Should().Contain("Status",
            "REQ-3: Developer defect list must include a Status column for tracking");
        _fixture.QADefectList.Should().Contain("Status",
            "REQ-3: QA defect list must include a Status column for tracking");
    }

    // ── Pipeline infrastructure validation ──────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N13_Pipeline_DoesNotUse_WindowsRunners_REQ4()
    {
        _fixture.PipelineYaml.Should().NotContain("windows-latest",
            "REQ-4: All CI jobs should use ubuntu-latest for faster execution, not Windows runners");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N14_Pipeline_DoesNotHardcode_DotnetVersion_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain($"DOTNET_VERSION: '{QATestingCodeSpec.DotnetVersion}'",
            "REQ-4: .NET version should be defined as an environment variable, not hardcoded per job");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N15_Pipeline_DoesNotHardcode_NodeVersion_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain($"NODE_VERSION: '{QATestingCodeSpec.NodeVersion}'",
            "REQ-4: Node version should be defined as an environment variable, not hardcoded per job");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N16_TestEnvironment_HandlesEmptyInMemoryEnvVar_REQ4()
    {
        _fixture.TestEnvironmentSource.Should().Contain("StringComparison.OrdinalIgnoreCase",
            "REQ-4: USE_INMEMORY_DB comparison must be case-insensitive to handle varied input");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N17_TestEnvironment_HandlesNullConnectionString_REQ4()
    {
        _fixture.TestEnvironmentSource.Should().Contain("IsNullOrWhiteSpace",
            "REQ-4: Connection string handling must check for null/whitespace gracefully");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N18_TestEnvironment_ThrowsClearErrorOnProxyFailure_REQ4()
    {
        _fixture.TestEnvironmentSource.Should().Contain("DATABASE PROXY NOT RUNNING",
            "REQ-4: Missing Cloud SQL Proxy must produce a clear, actionable error message");
    }

    // ── Build warning tracking ──────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N19_Pipeline_DoesNotSuppress_BuildWarnings_REQ5()
    {
        _fixture.PipelineYaml.Should().NotContain("--nowarn",
            "REQ-5: Pipeline should not suppress build warnings — they indicate issues to address");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N20_Pipeline_DoesNotUse_TreatWarningsAsErrors_False_REQ5()
    {
        _fixture.PipelineYaml.Should().NotContain("TreatWarningsAsErrors=false",
            "REQ-5: Pipeline should not explicitly disable TreatWarningsAsErrors");
    }

    [Fact(Skip = "QA-106: InMemory provider cannot be removed until 6+ test files are migrated to SQLite")]
    [Trait("Category", "Negative")]
    public void N21_TestProjects_DoNotReference_DeprecatedInMemoryProvider_REQ5()
    {
        foreach (var kvp in _fixture.TestProjectContents.Where(k => k.Value.Length > 0))
        {
            kvp.Value.Should().NotContain("Microsoft.EntityFrameworkCore.InMemory",
                $"REQ-5: {Path.GetFileName(kvp.Key)} should not reference deprecated InMemory provider; use SQLite instead");
        }
    }

    // ── Submodule / GH_PAT validation ───────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N22_Pipeline_Uses_GH_PAT_ForSubmoduleCheckout_REQ6()
    {
        _fixture.PipelineYaml.Should().Contain("secrets.GH_PAT",
            "REQ-6: Pipeline must use GH_PAT secret for submodule checkout");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N23_Pipeline_InitializesWorkflowSubmodule_REQ6()
    {
        _fixture.PipelineYaml.Should().Contain("git submodule update --init --recursive UNOPS.Workflow",
            "REQ-6: Pipeline must explicitly initialize the UNOPS.Workflow submodule");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N24_GitModules_UsesHttpsUrls_NotSsh_REQ6()
    {
        _fixture.GitModules.Should().NotContain("git@github.com",
            "REQ-6: Submodule URLs must use HTTPS (not SSH) for CI compatibility with GH_PAT");
    }

    private static string ExtractJobSection(string yaml, string jobName)
    {
        var pattern = $@"(?m)^  {Regex.Escape(jobName)}:";
        var match = Regex.Match(yaml, pattern);
        if (!match.Success) return string.Empty;

        var start = match.Index;
        var nextJobMatch = Regex.Match(yaml.Substring(start + match.Length), @"(?m)^\s{2}\w[\w-]*:");
        var end = nextJobMatch.Success ? start + match.Length + nextJobMatch.Index : yaml.Length;
        return yaml.Substring(start, end - start);
    }
}
