using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// PNO-1166: QA Testing Code — Positive (happy-path) tests.
///
/// Requirements validated:
/// - REQ-1: Pipeline YAML exists and contains all required jobs → P01–P03
/// - REQ-2: Test projects are correctly configured → P04–P05
/// - REQ-3: Defect lists exist for tracking → P06
/// - REQ-4: Pipeline triggers on correct branches → P07
/// - REQ-6: Submodule configuration present → P08
/// </summary>
[Collection("QATestingCode")]
public class PNO1166PositiveTests
{
    private readonly QATestingCodeFixture _fixture;

    public PNO1166PositiveTests(QATestingCodeFixture fixture) => _fixture = fixture;

    [Fact]
    [Trait("Category", "Positive")]
    public void P01_PipelineYaml_Exists_AndIsNotEmpty_REQ1()
    {
        _fixture.PipelineYaml.Should().NotBeNullOrWhiteSpace(
            "REQ-1: qa-tests.yml pipeline must exist to know within minutes if functionality is broken");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P02_PipelineYaml_ContainsAllRequiredJobs_REQ1()
    {
        foreach (var job in QATestingCodeSpec.RequiredPipelineJobs)
        {
            _fixture.PipelineYaml.Should().Contain(job + ":",
                $"REQ-1: Pipeline must include '{job}' job for comprehensive test execution");
        }
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P03_PipelineYaml_HasTestSummaryJob_REQ1()
    {
        _fixture.PipelineYaml.Should().Contain("test-summary:",
            "REQ-1: Pipeline must aggregate results in a test-summary job");
        _fixture.PipelineYaml.Should().Contain("QA Tests Summary",
            "REQ-1: Summary job should produce a human-readable summary");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P04_AllTestProjectFiles_Exist_REQ2()
    {
        foreach (var proj in QATestingCodeSpec.TestProjectPaths)
        {
            _fixture.TestProjectContents[proj].Should().NotBeNullOrWhiteSpace(
                $"REQ-2: Test project '{proj}' must exist for daily workflow integration");
        }
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P05_TestProjects_TargetCorrectFramework_REQ2()
    {
        foreach (var kvp in _fixture.TestProjectContents)
        {
            kvp.Value.Should().Contain($"<TargetFramework>{QATestingCodeSpec.TargetFramework}</TargetFramework>",
                $"REQ-2: {kvp.Key} must target {QATestingCodeSpec.TargetFramework}");
        }
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P06_DefectLists_Exist_REQ3()
    {
        _fixture.DevDefectList.Should().NotBeNullOrWhiteSpace(
            "REQ-3: Developer defect list must exist to track high-priority bugs");
        _fixture.QADefectList.Should().NotBeNullOrWhiteSpace(
            "REQ-3: QA defect list must exist to track test infrastructure issues");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P07_PipelineYaml_TriggersOnRequiredBranches_REQ2()
    {
        foreach (var branch in QATestingCodeSpec.RequiredTriggerBranches)
        {
            _fixture.PipelineYaml.Should().Contain(branch,
                $"REQ-2: Pipeline must trigger on '{branch}' branch for daily workflow integration");
        }
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P08_SubmoduleConfig_Exists_WithRequiredModules_REQ6()
    {
        _fixture.GitModules.Should().NotBeNullOrWhiteSpace(
            "REQ-6: .gitmodules must exist for submodule checkout");
        foreach (var submodule in QATestingCodeSpec.RequiredSubmodules)
        {
            _fixture.GitModules.Should().Contain(submodule,
                $"REQ-6: .gitmodules must reference '{submodule}' submodule");
        }
    }
}
