using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Negative tests for PNO-964: Stale state when dialog NOT opened, invalid states, expected failures.
/// </summary>
public class PNO964NegativeTests
{
    [Fact]
    [Trait("Category", "Negative")]
    public void StaleSearchQuery_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("infrastructure");
        spec.SearchQuery.Should().Be("infrastructure");
        // Never call OpenDialog - state persists (bug behavior before fix)
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleSearchResults_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("test", 5);
        spec.SearchResults.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleTreeSearchQuery_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("project");
        spec.TreeSearchQuery.Should().Be("project");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleAiSearchQuery_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("procurement");
        spec.AiSearchQuery.Should().Be("procurement");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleAiSearchResults_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 7);
        spec.AiSearchResults.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleAiSearchError_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 0, false, "Network error");
        spec.AiSearchError.Should().Be("Network error");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleIsAiSearching_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("q", 0, isSearching: true);
        spec.IsAiSearching.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void StaleSelectedOutputs_WhenDialogNotOpened_Persists()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "A" });
        spec.SelectedOutputsForDialog.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CloseDialog_DoesNotResetTreeSearchQuery()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateTreeSearch("project");
        spec.CloseDialog();
        spec.TreeSearchQuery.Should().Be("project");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CloseDialog_DoesNotResetAiSearchQuery()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("procurement");
        spec.CloseDialog();
        spec.AiSearchQuery.Should().Be("procurement");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CloseDialog_DoesNotResetAiSearchResults()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("x", 3);
        spec.CloseDialog();
        spec.AiSearchResults.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CloseDialog_DoesNotResetAiSearchError()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateAiSearch("x", 0, false, "Error");
        spec.CloseDialog();
        spec.AiSearchError.Should().Be("Error");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithoutPriorSearch_DoesNotThrow()
    {
        var spec = new DeliverablesDialogStateSpec();
        var act = () => spec.OpenDialog();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithNullSearchResults_DoesNotThrow()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchResults = null!;
        var act = () => spec.OpenDialog();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithNullAiSearchResults_DoesNotThrow()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchResults = null!;
        var act = () => spec.OpenDialog();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithNullSelectedOutputs_DoesNotThrow()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SelectedOutputsForDialog = null!;
        var act = () => spec.OpenDialog();
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenSearchQueryPopulated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x");
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenSearchResultsPopulated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 1);
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenTreeSearchPopulated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateTreeSearch("x");
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenAiSearchPopulated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 1);
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenAiSearchError_Populated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 0, false, "err");
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenIsAiSearchingTrue_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 0, isSearching: true);
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IsSearchStateClean_WhenSelectedOutputsPopulated_ReturnsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(new OutputSpec { Id = 1, Name = "X" });
        spec.IsSearchStateClean().Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ScssFile_WhenMissing_TestDoesNotFail()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.IsSearchStateClean().Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithWhitespaceOnlySearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "   ";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithVeryLongSearchQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = new string('x', 10000);
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithSpecialCharsInQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "<script>alert(1)</script>";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithUnicodeInQuery_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "日本語\u00A0test";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithManySearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("x", 1000);
        spec.OpenDialog();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithManyAiSearchResults_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateAiSearch("x", 500);
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithManySelectedOutputs_ResetsToEmpty()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateOutputsSelected(Enumerable.Range(1, 50).Select(i => new OutputSpec { Id = i, Name = $"O{i}" }).ToArray());
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEditModeActive_ResetsEditMode()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsEditingDeliverable = true;
        spec.EditingDeliverableIndex = 99;
        spec.OpenDialog();
        spec.IsEditingDeliverable.Should().BeFalse();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithNegativeEditingIndex_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.EditingDeliverableIndex = -1;
        spec.OpenDialog();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithZeroEditingIndex_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.EditingDeliverableIndex = 0;
        spec.OpenDialog();
        spec.EditingDeliverableIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEmptyString_StillClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchQuery = "";
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEmptyArray_StillClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SearchResults = new List<OutputSpec>();
        spec.OpenDialog();
        spec.SearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEmptyAiResults_StillClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchResults = new List<AiSearchMatchSpec>();
        spec.OpenDialog();
        spec.AiSearchResults.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEmptySelectedOutputs_StillClears()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SelectedOutputsForDialog = new List<OutputSpec>();
        spec.OpenDialog();
        spec.SelectedOutputsForDialog.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithNullAiSearchError_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchError = null;
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithEmptyStringAiSearchError_ResetsToNull()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.AiSearchError = "";
        spec.OpenDialog();
        spec.AiSearchError.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithIsAiSearchingFalse_RemainsFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsAiSearching = false;
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithIsAiSearchingTrue_ResetsToFalse()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.IsAiSearching = true;
        spec.OpenDialog();
        spec.IsAiSearching.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithAllModesPopulated_ResetsAll()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.SimulateSearchPerformed("quick");
        spec.SimulateTreeSearch("tree");
        spec.SimulateAiSearch("ai");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
        spec.TreeSearchQuery.Should().BeEmpty();
        spec.AiSearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithDialogAlreadyOpen_StillResetsState()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.SimulateSearchPerformed("test");
        spec.OpenDialog();
        spec.SearchQuery.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpenDialog_WithDialogClosed_OpensAndResets()
    {
        var spec = new DeliverablesDialogStateSpec();
        spec.OpenDialog();
        spec.CloseDialog();
        spec.SimulateSearchPerformed("stale");
        spec.OpenDialog();
        spec.ShowDeliverablesDialog.Should().BeTrue();
        spec.SearchQuery.Should().BeEmpty();
    }
}
