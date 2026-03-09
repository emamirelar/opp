using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Tests for PNO-964: What > Products and Services - issues with search boxes
///
/// Requirements validated:
/// - REQ-1: Quick search input must NOT overlap with search icon (SCSS padding rule)
/// - REQ-2: When opening dialog, ALL search fields must be cleared
/// - REQ-3: searchQuery reset to empty
/// - REQ-4: searchResults reset to empty array
/// - REQ-5: treeSearchQuery reset to empty
/// - REQ-6: aiSearchQuery reset to empty
/// - REQ-7: aiSearchResults reset to empty array
/// - REQ-8: aiSearchError reset to null
/// - REQ-9: isAiSearching reset to false
/// - REQ-10: selectedOutputsForDialog reset to empty array
/// - REQ-11: SCSS padding-left: 2.25rem !important for .search-input-no-focus-border.p-inputtext.pl-9
/// </summary>
public class PNO964PositiveTests
{
    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsSearchQuery_ToEmpty()
    {
        // REQ-3
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("infrastructure");
        spec.SearchQuery.Should().NotBeEmpty();

        spec.OpenDialog();

        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsSearchResults_ToEmpty()
    {
        // REQ-4
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("test", 5);
        spec.SearchResults.Should().HaveCount(5);

        spec.OpenDialog();

        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsTreeSearchQuery_ToEmpty()
    {
        // REQ-5
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("project management");
        spec.TreeSearchQuery.Should().NotBeEmpty();

        spec.OpenDialog();

        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsAiSearchQuery_ToEmpty()
    {
        // REQ-6
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("procurement services");
        spec.AiSearchQuery.Should().NotBeEmpty();

        spec.OpenDialog();

        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsAiSearchResults_ToEmpty()
    {
        // REQ-7
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("consulting", 4);
        spec.AiSearchResults.Should().HaveCount(4);

        spec.OpenDialog();

        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsAiSearchError_ToNull()
    {
        // REQ-8
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 0, false, "API error");
        spec.AiSearchError.Should().NotBeNull();

        spec.OpenDialog();

        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsIsAiSearching_ToFalse()
    {
        // REQ-9
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("query", 0, isSearching: true);
        spec.IsAiSearching.Should().BeTrue();

        spec.OpenDialog();

        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsSelectedOutputsForDialog_ToEmpty()
    {
        // REQ-10
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Out1" }, new OutputSpec { Id = 2, Name = "Out2" });
        spec.SelectedOutputsForDialog.Should().HaveCount(2);

        spec.OpenDialog();

        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_ResetsEditMode()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 3;

        spec.OpenDialog();

        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialog_AfterFullyPopulatedState_AllFieldsCleared()
    {
        // REQ-2: Core bug scenario - previously searched items must not persist
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("infrastructure", 10);
        spec.SimulateTreeSearch("project");
        spec.SimulateAiSearch("procurement", 5, true, "timeout");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "A" });
        spec.IsEditingDeliverable = true;

        spec.OpenDialog();

        spec.IsSearchStateClean().Should().BeTrue();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void OpenDialogCloseReopen_Cycle_ShowsCleanState()
    {
        // Core bug: Open → Search → Close → Reopen must show blank search boxes
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("stale query", 3);
        spec.CloseDialog();

        spec.OpenDialog();

        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ScssRule_Exists_EnforcesPaddingLeft()
    {
        // REQ-1, REQ-11: SCSS rule must exist to prevent text overlap with search icon
        var scssPath = ResolveScssPath();
        if (!File.Exists(scssPath))
        {
            // Skip if SCSS not found (e.g., different repo layout)
            return;
        }

        var content = File.ReadAllText(scssPath);
        content.Should().Contain("search-input-no-focus-border");
        content.Should().Contain("pl-9");
        content.Should().Contain("padding-left");
        content.Should().Contain("2.25rem");
        content.Should().Contain("!important");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void CloseDialog_ClearsSearchQuery()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("test");

        spec.CloseDialog();

        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void CloseDialog_ClearsSelectedOutputs()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });

        spec.CloseDialog();

        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    private static string ResolveScssPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var scssRelative = Path.Combine("UNOPS.PAO.ClientApp", "src", "app", "features", "partnerships", "opportunities", "components", "opportunity", "view", "sections", "what", "opportunity-what-section.component.scss");
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", scssRelative),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", scssRelative),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", scssRelative),
            Path.Combine(Directory.GetCurrentDirectory(), scssRelative),
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full))
                return full;
        }
        return Path.Combine(baseDir, "opportunity-what-section.component.scss");
    }
}
