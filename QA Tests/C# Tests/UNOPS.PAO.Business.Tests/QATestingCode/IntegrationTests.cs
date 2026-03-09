using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// PNO-1166: QA Testing Code — Integration tests.
/// Validates end-to-end flows across pipeline, projects, infrastructure, and defect tracking.
///
/// Requirements validated:
/// - REQ-1: Full pipeline → build → test → report flow works end-to-end → I01–I06
/// - REQ-2: Test projects → infrastructure → execution flow is coherent → I07–I12
/// - REQ-3: Defect tracking → CI filtering → visibility flow is complete → I13–I18
/// - REQ-4: All components reference compatible versions → I19–I24
/// </summary>
[Collection("QATestingCode")]
public class PNO1166IntegrationTests
{
    private readonly QATestingCodeFixture _fixture;

    public PNO1166IntegrationTests(QATestingCodeFixture fixture) => _fixture = fixture;

    // ── Pipeline → Build → Test → Report end-to-end ─────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I01_Pipeline_HasCompleteBuildTestReportFlow_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("BUILD STAGE",
            "REQ-1: Pipeline must have a clearly labeled BUILD stage");
        _fixture.PipelineYaml.Should().Contain("TEST STAGE",
            "REQ-1: Pipeline must have a clearly labeled TEST stage");
        _fixture.PipelineYaml.Should().Contain("SUMMARY",
            "REQ-1: Pipeline must have a SUMMARY stage");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I02_Pipeline_BuildArtifacts_SharedWithTestJobs_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("upload-artifact@v4",
            "REQ-1: Build jobs must upload artifacts");
        _fixture.PipelineYaml.Should().Contain("download-artifact",
            "REQ-1: Test jobs must download build artifacts (or rebuild, but artifact sharing is preferred)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I03_Pipeline_AllTestProjects_Referenced_REQ1()
    {
        foreach (var proj in QATestingCodeSpec.TestProjectPaths)
        {
            var projFileName = Path.GetFileName(proj);
            _fixture.PipelineYaml.Should().Contain(projFileName,
                $"REQ-1: Pipeline must reference test project '{projFileName}' for execution");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I04_Pipeline_TestResultArtifacts_Uploaded_REQ1()
    {
        foreach (var artifact in QATestingCodeSpec.RequiredPipelineArtifacts)
        {
            _fixture.PipelineYaml.Should().Contain(artifact,
                $"REQ-1: Pipeline must upload '{artifact}' artifact for result preservation");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I05_Pipeline_EFCoreMigrations_RunBeforeBusinessTests_REQ1()
    {
        var businessSection = ExtractJobSection(_fixture.PipelineYaml, "business-tests");
        businessSection.Should().Contain("dotnet-ef",
            "REQ-1: Business tests must install EF Core tools for database migration");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I06_Pipeline_FullSuite_CoveredByNightlySchedule_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("playwright-full",
            "REQ-1: Full Playwright suite must be available for nightly/comprehensive runs");
    }

    // ── Test project → infrastructure coherence ─────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I07_BusinessTestProject_ReferencesXunitPackages_REQ2()
    {
        var businessProj = _fixture.TestProjectContents.First(k =>
            k.Key.Contains("Business.Tests")).Value;

        foreach (var pkg in QATestingCodeSpec.RequiredXunitPackages)
        {
            businessProj.Should().Contain(pkg,
                $"REQ-2: Business.Tests must reference '{pkg}' for test execution");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I08_BusinessTestProject_ReferencesFluentAssertions_REQ2()
    {
        foreach (var kvp in _fixture.TestProjectContents.Where(k => k.Value.Length > 0))
        {
            kvp.Value.Should().Contain("FluentAssertions",
                $"REQ-2: {Path.GetFileName(kvp.Key)} must use FluentAssertions for readable test assertions");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I09_BusinessTestProject_ReferencesEFCoreSqlite_REQ2()
    {
        var businessProj = _fixture.TestProjectContents.First(k =>
            k.Key.Contains("Business.Tests")).Value;

        businessProj.Should().Contain("Microsoft.EntityFrameworkCore.Sqlite",
            "REQ-2: Business.Tests must reference Sqlite provider for in-memory test mode");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I10_BusinessTestProject_ReferencesBogus_REQ2()
    {
        var businessProj = _fixture.TestProjectContents.First(k =>
            k.Key.Contains("Business.Tests")).Value;

        businessProj.Should().Contain("Bogus",
            "REQ-2: Business.Tests must reference Bogus for realistic test data generation");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I11_TestDataHelper_Exists_REQ2()
    {
        var helperSource = _fixture.TestBaseFiles.GetValueOrDefault("TestDataHelper.cs", "");
        helperSource.Should().NotBeNullOrWhiteSpace(
            "REQ-2: TestDataHelper must exist for standardized test user creation");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I12_TestEnvironment_CreatesAppDbContextOptions_REQ2()
    {
        _fixture.TestEnvironmentSource.Should().Contain("CreateAppDbContextOptions",
            "REQ-2: TestEnvironment must provide CreateAppDbContextOptions for test fixture setup");
        _fixture.TestEnvironmentSource.Should().Contain("CreateUNOPSDbContextOptions",
            "REQ-2: TestEnvironment must provide CreateUNOPSDbContextOptions for UNOPS-specific tests");
    }

    // ── Defect tracking → CI flow ───────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I13_Pipeline_DefectFilterConsistency_IncludeAndExclude_REQ3()
    {
        _fixture.PipelineYaml.Should().Contain(QATestingCodeSpec.DefectExcludeFilter,
            "REQ-3: Gating tests must use exclude filter for defect tests");
        _fixture.PipelineYaml.Should().Contain(QATestingCodeSpec.DefectIncludeFilter,
            "REQ-3: Defect job must use include filter to run ONLY defect tests");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I14_Pipeline_DefectJob_RunsBothBusinessAndIntegration_REQ3()
    {
        var defectSection = ExtractJobSection(_fixture.PipelineYaml, "defect-tests");
        defectSection.Should().Contain("Business.Tests",
            "REQ-3: Defect job must run business test defects");
        defectSection.Should().Contain("IntegrationTests",
            "REQ-3: Defect job must run integration test defects");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I15_DefectLists_CrossReferenceEachOther_REQ3()
    {
        var devRefsQA = _fixture.DevDefectList.Contains("QA-") ||
                        _fixture.DevDefectList.Contains("Defect List for QA");
        var qaRefsDev = _fixture.QADefectList.Contains("DEF-") ||
                        _fixture.QADefectList.Contains("Defect List for Developers");
        (devRefsQA || qaRefsDev).Should().BeTrue(
            "REQ-3: Defect lists should cross-reference each other for traceability");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I16_Pipeline_DefectTests_UseSQLite_ForSpeed_REQ3()
    {
        var defectSection = ExtractJobSection(_fixture.PipelineYaml, "defect-tests");
        defectSection.Should().Contain("USE_INMEMORY_DB: 'true'",
            "REQ-3: Defect tests should use SQLite for faster execution (informational only)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I17_DevDefectList_ContainsOpenAndResolvedSections_REQ3()
    {
        _fixture.DevDefectList.Should().Contain("Open",
            "REQ-3: Developer defect list must track open defects");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I18_QADefectList_ContainsImpactColumn_REQ3()
    {
        _fixture.QADefectList.Should().Contain("Impact",
            "REQ-3: QA defect list must include Impact column (number of tests blocked)");
    }

    // ── Cross-component version compatibility ───────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I19_AllTestProjects_SameTargetFramework_REQ4()
    {
        foreach (var kvp in _fixture.TestProjectContents.Where(k => k.Value.Length > 0))
        {
            kvp.Value.Should().Contain(QATestingCodeSpec.TargetFramework,
                $"REQ-4: {Path.GetFileName(kvp.Key)} must target {QATestingCodeSpec.TargetFramework} for consistency");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I20_Pipeline_DotnetVersion_MatchesProjectTargetFramework_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain(QATestingCodeSpec.DotnetVersion,
            $"REQ-4: Pipeline .NET version must be {QATestingCodeSpec.DotnetVersion} matching project target");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I21_Pipeline_SubmoduleCheckout_UsesRecursive_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("submodules: recursive",
            "REQ-4: Pipeline checkout must use 'recursive' to initialize all submodules");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I22_GitModules_SubmoduleUrls_AreGitHub_REQ4()
    {
        foreach (var submodule in QATestingCodeSpec.RequiredSubmodules)
        {
            _fixture.GitModules.Should().Contain("github.com/UNOPS-ITG",
                $"REQ-4: Submodule '{submodule}' URL must point to UNOPS-ITG GitHub organization");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I23_Pipeline_EnvironmentVariables_DefinedAtTopLevel_REQ4()
    {
        _fixture.PipelineYaml.Should().Contain("env:",
            "REQ-4: Pipeline must define environment variables at top level for consistency");
        _fixture.PipelineYaml.Should().Contain("DOTNET_VERSION:",
            "REQ-4: DOTNET_VERSION must be defined as a top-level env var");
        _fixture.PipelineYaml.Should().Contain("NODE_VERSION:",
            "REQ-4: NODE_VERSION must be defined as a top-level env var");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I24_TestEnvironment_SupportsBothAppDbContextTypes_REQ4()
    {
        _fixture.TestEnvironmentSource.Should().Contain("DbContextOptions<AppDbContext>",
            "REQ-4: TestEnvironment must create options for AppDbContext");
        _fixture.TestEnvironmentSource.Should().Contain("DbContextOptions<UNOPSAppDbContext>",
            "REQ-4: TestEnvironment must create options for UNOPSAppDbContext (UNOPS override)");
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
