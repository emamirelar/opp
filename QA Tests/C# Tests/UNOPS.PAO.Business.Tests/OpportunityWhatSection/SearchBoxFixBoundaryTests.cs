using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Boundary tests for PNO-964: Edge values, min/max, empty vs null, concurrent-like scenarios.
/// </summary>
public class PNO964BoundaryTests
{
    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WhenAlreadyEmpty_StateRemainsEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithSingleCharSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "x";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithSingleSearchResult_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 1);
        spec.OpenDialog();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithSingleAiSearchResult_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 1);
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithSingleSelectedOutput_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithZeroSearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 0);
        spec.OpenDialog();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithZeroAiSearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 0);
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEditingDeliverableIndexZero_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.EditingDeliverableIndex = 0;
        spec.OpenDialog();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEditingDeliverableIndexMaxInt_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.EditingDeliverableIndex = int.MaxValue;
        spec.OpenDialog();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithWhitespaceOnlyTreeSearch_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.TreeSearchQuery = "\t\n ";
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithWhitespaceOnlyAiSearch_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchQuery = " \r\n ";
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithNewlineOnlySearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "\n";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithTabOnlySearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "\t";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyStringSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = string.Empty;
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyStringTreeSearch_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.TreeSearchQuery = string.Empty;
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyStringAiSearch_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchQuery = string.Empty;
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyListSearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchResults = new List<OutputSpec>();
        spec.OpenDialog();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyListAiSearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchResults = new List<AiSearchMatchSpec>();
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithEmptyListSelectedOutputs_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SelectedOutputsForDialog = new List<OutputSpec>();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithNullSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = null!;
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithNullTreeSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.TreeSearchQuery = null!;
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithNullAiSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchQuery = null!;
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_MultipleTimesInSequence_StateRemainsClean()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.OpenDialog();
        spec.OpenDialog();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenCloseOpen_WithSearchBetweenOpens_SecondOpenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.SimulateSearchPerformed("between");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenCloseOpen_WithTreeSearchBetweenOpens_SecondOpenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.SimulateTreeSearch("between");
        spec.OpenDialog();
        spec.TreeSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenCloseOpen_WithAiSearchBetweenOpens_SecondOpenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.SimulateAiSearch("between", 3);
        spec.OpenDialog();
        spec.AiSearchQuery.Should().BeEmpty();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenCloseOpen_WithSelectionBetweenOpens_SecondOpenClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsSearchStateClean_WhenAllEmpty_ReturnsTrue()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IsSearchStateClean_AfterOpenDialog_ReturnsTrue()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithMixedPopulatedAndEmpty_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "only this";
        spec.TreeSearchQuery = string.Empty;
        spec.AiSearchQuery = string.Empty;
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlyEditModePopulated_EditModeCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 5;
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlySearchQueryPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "solo";
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlyTreeSearchPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.TreeSearchQuery = "solo";
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlyAiSearchPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("solo", 1);
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlyAiSearchErrorPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchError = "solo error";
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlyIsAiSearchingTrue_ResetsToFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsAiSearching = true;
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOnlySelectedOutputsPopulated_AllCleared()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "Solo" });
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_ShowDeliverablesDialog_SetToTrue()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.ShowDeliverablesDialog = false;
        spec.OpenDialog();
        spec.ShowDeliverablesDialog.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void CloseDialog_ShowDeliverablesDialog_SetToFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.ShowDeliverablesDialog.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOutputSpecWithEmptyName_ResetsSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "" });
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithOutputSpecWithZeroId_ResetsSelection()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 0, Name = "X" });
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void OpenDialog_WithAiSearchMatchZeroScore_ResetsResults()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchResults = new List<AiSearchMatchSpec> { new() { OutputId = 1, Score = 0 } };
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }
}
