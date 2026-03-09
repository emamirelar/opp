using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Functional tests for PNO-964: Business rules, state transitions, reset contract enforcement.
/// </summary>
public class PNO964FunctionalTests
{
    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ResetSequence_MatchesSpecificationOrder()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.SimulateTreeSearch("y");
        spec.SimulateAiSearch("z", 2);
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "A" });
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 1;

        spec.OpenDialog();

        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_StateTransition_FromPopulatedToClean()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("infrastructure", 5);
        var wasPopulated = !spec.IsSearchStateClean();

        spec.OpenDialog();

        wasPopulated.Should().BeTrue();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CloseDialog_StateTransition_ClearsSearchAndSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("test");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });

        spec.CloseDialog();

        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.ShowDeliverablesDialog.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IsSearchStateClean_ComputedValue_ReflectsAllSearchFields()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsSearchStateClean().Should().BeTrue();

        spec.SearchQuery = "x";
        spec.IsSearchStateClean().Should().BeFalse();

        spec.SearchQuery = string.Empty;
        spec.SearchResults = new List<OutputSpec> { new() { Id = 1, Name = "X" } };
        spec.IsSearchStateClean().Should().BeFalse();

        spec.SearchResults = new List<OutputSpec>();
        spec.TreeSearchQuery = "y";
        spec.IsSearchStateClean().Should().BeFalse();

        spec.TreeSearchQuery = string.Empty;
        spec.AiSearchQuery = "z";
        spec.IsSearchStateClean().Should().BeFalse();

        spec.AiSearchQuery = string.Empty;
        spec.AiSearchResults = new List<AiSearchMatchSpec> { new() { OutputId = 1, Score = 0.9 } };
        spec.IsSearchStateClean().Should().BeFalse();

        spec.AiSearchResults = new List<AiSearchMatchSpec>();
        spec.AiSearchError = "err";
        spec.IsSearchStateClean().Should().BeFalse();

        spec.AiSearchError = null;
        spec.IsAiSearching = true;
        spec.IsSearchStateClean().Should().BeFalse();

        spec.IsAiSearching = false;
        spec.SelectedOutputsForDialog = new List<OutputSpec> { new() { Id = 1, Name = "X" } };
        spec.IsSearchStateClean().Should().BeFalse();

        spec.SelectedOutputsForDialog = new List<OutputSpec>();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ResetContract_AllTenSignalsCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("q", 1);
        spec.SimulateTreeSearch("t");
        spec.SimulateAiSearch("a", 1, true, "e");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "O" });
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
    [Trait("Category", "Functional")]
    public void OpenDialog_QuickSearchMode_StateCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("quick search term");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_BrowseMode_StateCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("tree filter");
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AiMode_StateCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("semantic query", 5, true, "loading");
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AllThreeSearchModesPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateTreeSearch("browse");
        spec.SimulateAiSearch("ai", 3);
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_OutputSelectionState_Cleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(
            new OutputSpec { Id = 1, Name = "A" },
            new OutputSpec { Id = 2, Name = "B" },
            new OutputSpec { Id = 3, Name = "C" });
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ErrorState_Cleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchError = "API timeout";
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_LoadingState_Reset()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsAiSearching = true;
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_EditModeState_Reset()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 42;
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SimulateSearchPerformed_SetsQueryAndResults()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("test", 4);
        spec.SearchQuery.Should().Be("test");
        spec.SearchResults.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SimulateTreeSearch_SetsTreeSearchQuery()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("filter");
        spec.TreeSearchQuery.Should().Be("filter");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SimulateAiSearch_SetsAllAiState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("q", 2, true, "err");
        spec.AiSearchQuery.Should().Be("q");
        spec.AiSearchResults.Should().HaveCount(2);
        spec.IsAiSearching.Should().BeTrue();
        spec.AiSearchError.Should().Be("err");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void SimulateOutputsSelected_SetsSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        var o1 = new OutputSpec { Id = 1, Name = "A" };
        var o2 = new OutputSpec { Id = 2, Name = "B" };
        spec.SimulateOutputsSelected(o1, o2);
        spec.SelectedOutputsForDialog.Should().Contain(o1);
        spec.SelectedOutputsForDialog.Should().Contain(o2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ReplacesCollections_NotMutates()
    {
        var spec = new DeliverablesDialogStateSpec();
        var originalList = new List<OutputSpec> { new() { Id = 1, Name = "X" } };
        spec.SelectedOutputsForDialog = originalList;
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().NotBeSameAs(originalList);
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_SearchResultsReplaced_NewInstance()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 3);
        var oldResults = spec.SearchResults;
        spec.OpenDialog();
        spec.SearchResults.Should().NotBeSameAs(oldResults);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AiSearchResultsReplaced_NewInstance()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 2);
        var oldResults = spec.AiSearchResults;
        spec.OpenDialog();
        spec.AiSearchResults.Should().NotBeSameAs(oldResults);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CloseDialog_DoesNotResetTreeSearch_ByDesign()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("persist");
        spec.CloseDialog();
        spec.TreeSearchQuery.Should().Be("persist");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CloseDialog_DoesNotResetAiSearch_ByDesign()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("persist", 2);
        spec.CloseDialog();
        spec.AiSearchQuery.Should().Be("persist");
        spec.AiSearchResults.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AfterClose_ReopensWithCleanState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("stale");
        spec.CloseDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ShowDeliverablesDialog_AlwaysTrueAfterOpen()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CloseDialog_ShowDeliverablesDialog_AlwaysFalseAfterClose()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.ShowDeliverablesDialog.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_IdempotentWhenCalledMultipleTimes_StateRemainsClean()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.OpenDialog();
        spec.OpenDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ResetOrder_EditModeBeforeSearchState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.SimulateSearchPerformed("x");
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ResetOrder_SelectionBeforeSearchState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.SimulateSearchPerformed("y");
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OutputSpec_CanBeUsedInSelection()
    {
        var output = new OutputSpec { Id = 42, Name = "Test Output" };
        output.Id.Should().Be(42);
        output.Name.Should().Be("Test Output");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AiSearchMatchSpec_CanBeUsedInResults()
    {
        var match = new AiSearchMatchSpec { OutputId = 10, Score = 0.95 };
        match.OutputId.Should().Be(10);
        match.Score.Should().Be(0.95);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_WithAllStatesPopulated_IsSearchStateCleanTrue()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("a", 10);
        spec.SimulateTreeSearch("b");
        spec.SimulateAiSearch("c", 5, true, "err");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ResetContract_MatchesAngularImplementation()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 2;
        spec.SelectedOutputsForDialog = new List<OutputSpec> { new() { Id = 1, Name = "A" } };
        spec.SearchQuery = "q";
        spec.SearchResults = new List<OutputSpec> { new() { Id = 2, Name = "B" } };
        spec.TreeSearchQuery = "t";
        spec.AiSearchQuery = "a";
        spec.AiSearchResults = new List<AiSearchMatchSpec> { new() { OutputId = 3, Score = 0.9 } };
        spec.AiSearchError = "e";
        spec.IsAiSearching = true;

        spec.OpenDialog();

        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CloseDialog_ResetContract_MatchesAngularImplementation()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("q");
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });

        spec.CloseDialog();

        spec.ShowDeliverablesDialog.Should().BeFalse();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_QuickSearchThenOpen_ClearsQuickSearchOnly()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("quick only");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_TreeBrowseThenOpen_ClearsTreeSearch()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("browse only");
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AiSearchThenOpen_ClearsAllAiState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("ai only", 3);
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
        spec.AiSearchError.Should().BeNull();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ReplacesSearchResults_WithNewList()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 2);
        var oldRef = spec.SearchResults;
        spec.OpenDialog();
        spec.SearchResults.Should().NotBeSameAs(oldRef);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ReplacesSelectedOutputs_WithNewList()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        var oldRef = spec.SelectedOutputsForDialog;
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().NotBeSameAs(oldRef);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_ReplacesAiSearchResults_WithNewList()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 1);
        var oldRef = spec.AiSearchResults;
        spec.OpenDialog();
        spec.AiSearchResults.Should().NotBeSameAs(oldRef);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_SearchQuerySetToEmptyString_NotNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.OpenDialog();
        spec.SearchQuery.Should().Be(string.Empty);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_AiSearchErrorSetToNull_NotEmptyString()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchError = "err";
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpenDialog_EditingDeliverableIndexSetToNull_NotZero()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.EditingDeliverableIndex = 5;
        spec.OpenDialog();
        spec.EditingDeliverableIndex.Should().BeNull();
    }
}
