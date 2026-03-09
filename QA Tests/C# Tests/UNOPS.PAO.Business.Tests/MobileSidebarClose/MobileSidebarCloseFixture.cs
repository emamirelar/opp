namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// Shared fixture for PNO-669 tests.
/// Pre-loads sidebar, layout, and translation file contents.
/// </summary>
public sealed class MobileSidebarCloseFixture
{
    public string SidebarTemplate { get; }
    public string SidebarTypescript { get; }
    public string SidebarScss { get; }
    public string LayoutTemplate { get; }
    public string LayoutTypescript { get; }
    public string LayoutService { get; }
    public string ResponsiveScss { get; }
    public string MenuScss { get; }
    public string TopbarTemplate { get; }
    public Dictionary<string, string> TranslationContents { get; } = new();

    public MobileSidebarCloseFixture()
    {
        SidebarTemplate = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.SidebarTemplatePath));
        SidebarTypescript = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.SidebarTypescriptPath));
        SidebarScss = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.SidebarScssPath));
        LayoutTemplate = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.LayoutTemplatePath));
        LayoutTypescript = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.LayoutTypescriptPath));
        LayoutService = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.LayoutServicePath));
        ResponsiveScss = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.ResponsiveScssPath));
        MenuScss = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.MenuScssPath));
        TopbarTemplate = MobileSidebarCloseSpec.ReadFileOrEmpty(
            MobileSidebarCloseSpec.ResolvePath(MobileSidebarCloseSpec.TopbarTemplatePath));

        foreach (var file in MobileSidebarCloseSpec.TranslationFiles)
        {
            var content = MobileSidebarCloseSpec.ReadFileOrEmpty(
                MobileSidebarCloseSpec.ResolvePath(file));
            TranslationContents[file] = content;
        }
    }
}

[CollectionDefinition("MobileSidebarClose")]
public class MobileSidebarCloseCollection : ICollectionFixture<MobileSidebarCloseFixture> { }
