namespace UNOPS.PAO.Business.Tests.QATestingCode;

/// <summary>
/// Shared fixture for PNO-1166 tests.
/// Pre-loads pipeline YAML, .csproj contents, defect lists, and .gitmodules
/// so individual tests don't re-read files.
/// </summary>
public sealed class QATestingCodeFixture
{
    public string PipelineYaml { get; }
    public string GitModules { get; }
    public string DevDefectList { get; }
    public string QADefectList { get; }
    public Dictionary<string, string> TestProjectContents { get; } = new();
    public Dictionary<string, string> TestBaseFiles { get; } = new();
    public string TestEnvironmentSource { get; }

    public QATestingCodeFixture()
    {
        PipelineYaml = QATestingCodeSpec.ReadFileOrEmpty(
            QATestingCodeSpec.ResolvePath(QATestingCodeSpec.PipelineFile));

        GitModules = QATestingCodeSpec.ReadFileOrEmpty(
            QATestingCodeSpec.ResolvePath(".gitmodules"));

        DevDefectList = QATestingCodeSpec.ReadFileOrEmpty(
            QATestingCodeSpec.ResolvePath(QATestingCodeSpec.DevDefectListFile));

        QADefectList = QATestingCodeSpec.ReadFileOrEmpty(
            QATestingCodeSpec.ResolvePath(QATestingCodeSpec.QADefectListFile));

        foreach (var proj in QATestingCodeSpec.TestProjectPaths)
        {
            var path = QATestingCodeSpec.ResolvePath(proj);
            TestProjectContents[proj] = QATestingCodeSpec.ReadFileOrEmpty(path);
        }

        var testBasePath = QATestingCodeSpec.ResolvePath(
            "QA Tests", "C# Tests", "UNOPS.PAO.Business.Tests", "TestBase");
        foreach (var file in QATestingCodeSpec.RequiredTestBaseFiles)
        {
            var path = Path.Combine(testBasePath, file);
            TestBaseFiles[file] = QATestingCodeSpec.ReadFileOrEmpty(path);
        }

        TestEnvironmentSource = TestBaseFiles.GetValueOrDefault("TestEnvironment.cs", string.Empty);
    }
}

[CollectionDefinition("QATestingCode")]
public class QATestingCodeCollection : ICollectionFixture<QATestingCodeFixture> { }
