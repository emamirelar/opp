using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Integration tests for PNO-964: Full Open→Search→Close→Reopen cycles, multi-step workflows.
/// </summary>
public class PNO964IntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_OpenSearchCloseReopen_ShowsCleanState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("infrastructure", 5);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_OpenTreeSearchCloseReopen_ShowsCleanState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("project management");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_OpenAiSearchCloseReopen_ShowsCleanState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("procurement consulting", 4);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_OpenSelectOutputsCloseReopen_ShowsCleanState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "A" }, new OutputSpec { Id = 2, Name = "B" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_AllModesPopulatedCloseReopen_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick", 3);
        spec.SimulateTreeSearch("browse");
        spec.SimulateAiSearch("ai", 2, true, "error");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void MultipleOpenCloseCycles_StateConsistentlyCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        for (int i = 0; i < 5; i++)
        {
            spec.OpenDialog();
            spec.SimulateSearchPerformed($"query-{i}", i + 1);
            spec.CloseDialog();
        }
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenSearchAddToSelectionCloseReopen_SelectionCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("infrastructure", 5);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Infra 1" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenTreeBrowseSelectCloseReopen_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("project");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 10, Name = "Project Mgmt" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenAiSearchSelectCloseReopen_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("consulting", 3);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 20, Name = "Consulting" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenWithEditModeSearchCloseReopen_EditModeAndSearchCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 2;
        spec.SimulateSearchPerformed("edit search");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenWithAiErrorSearchCloseReopen_ErrorCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("failing query", 0, false, "API timeout");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenWithAiLoadingSearchCloseReopen_LoadingReset()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("loading query", 0, isSearching: true);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SwitchBetweenModesThenOpen_AllModesCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateTreeSearch("browse");
        spec.SimulateAiSearch("ai");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenCloseOpenCloseOpen_StateCleanOnFinalOpen()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("first");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("second");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenWithMultipleSelectedOutputsCloseReopen_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateOutputsSelected(
            new OutputSpec { Id = 1, Name = "A" },
            new OutputSpec { Id = 2, Name = "B" },
            new OutputSpec { Id = 3, Name = "C" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenSearchSwitchToTreeCloseReopen_BothCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateTreeSearch("tree");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenSearchSwitchToAiCloseReopen_BothCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateAiSearch("ai", 2);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenTreeSwitchToAiCloseReopen_BothCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("tree");
        spec.SimulateAiSearch("ai", 1);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullUserWorkflow_AddNewOpensCleanDialog()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("previous search");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Prev" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullUserWorkflow_ReopenAfterError_ErrorCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("bad query", 0, false, "Network error");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullUserWorkflow_ReopenAfterLoading_LoadingReset()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("slow query", 0, isSearching: true);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenDialog_AfterCloseWithStaleData_ReopenClearsAll()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("stale");
        spec.SimulateTreeSearch("stale tree");
        spec.SimulateAiSearch("stale ai", 2);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Stale" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenClose_WithoutReopen_CloseClearsSearchAndSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("test");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.CloseDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TripleOpenCycle_StateCleanEachTime()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("1");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
        spec.SimulateSearchPerformed("2");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
        spec.SimulateSearchPerformed("3");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenWithAllSignalsPopulated_CloseReopen_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 1;
        spec.SimulateSearchPerformed("q", 5);
        spec.SimulateTreeSearch("t");
        spec.SimulateAiSearch("a", 3, true, "e");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "O" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenDialog_ShowDeliverablesDialogTrue_AfterOpen()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void CloseDialog_ShowDeliverablesDialogFalse_AfterClose()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.ShowDeliverablesDialog.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenReopen_WithoutClose_SecondOpenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("first");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SimulateMethods_DoNotCallOpen_StatePersists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("persist");
        spec.SimulateTreeSearch("persist");
        spec.SimulateAiSearch("persist", 2);
        spec.SearchQuery.Should().Be("persist");
        spec.TreeSearchQuery.Should().Be("persist");
        spec.AiSearchQuery.Should().Be("persist");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenDialog_ResetContract_AllSignalsClearedInSingleCall()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("a", 1);
        spec.SimulateTreeSearch("b");
        spec.SimulateAiSearch("c", 1);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "d" });
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 0;

        spec.OpenDialog();

        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_QuickSearchOnly_ReopenShowsEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick search term", 10);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_TreeSearchOnly_ReopenShowsEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("tree filter text");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_AiSearchOnly_ReopenShowsEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("semantic query", 5, false, null);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_SelectionOnly_ReopenShowsEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Only" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void OpenDialog_Contract_MatchesPNO964FixSpecification()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("prev", 3);
        spec.SimulateTreeSearch("prev");
        spec.SimulateAiSearch("prev", 2, true, "prev err");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Prev" });
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 1;

        spec.OpenDialog();

        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_WithEditMode_ReopenClearsEditMode()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 3;
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_SequentialSearches_EachReopenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("search1");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SimulateSearchPerformed("search2");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_OutputSelectionWorkflow_ReopenClearsSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("infra", 5);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Infra A" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_QuickThenTreeSearch_ReopenClearsBoth()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateTreeSearch("tree");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_QuickThenAiSearch_ReopenClearsBoth()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateAiSearch("ai", 2);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_TreeThenAiSearch_ReopenClearsBoth()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("tree");
        spec.SimulateAiSearch("ai", 1);
        spec.CloseDialog();
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullCycle_AllThreeModesPlusSelection_ReopenClearsAll()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("q", 1);
        spec.SimulateTreeSearch("t");
        spec.SimulateAiSearch("a", 1);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "O" });
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }
}
