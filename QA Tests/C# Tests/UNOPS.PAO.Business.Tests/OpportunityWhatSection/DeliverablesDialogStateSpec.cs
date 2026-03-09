// PNO-964: Specification model for Add Products and Services dialog state management.
// Mirrors the Angular opportunity-what-section.component.ts reset logic.
// Used by C# tests to validate the dialog state reset contract.

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

/// <summary>
/// Specification model for the Add Products and Services dialog state.
/// REQ-2 through REQ-10: When opening the dialog, all search/selection state must be reset.
/// </summary>
public sealed class DeliverablesDialogStateSpec
{
    // Edit mode (reset on open)
    public bool IsEditingDeliverable { get; set; }
    public int? EditingDeliverableIndex { get; set; }

    // Multi-selection (REQ-10)
    public IList<OutputSpec> SelectedOutputsForDialog { get; set; } = new List<OutputSpec>();

    // Quick search (REQ-3, REQ-4)
    public string SearchQuery { get; set; } = string.Empty;
    public IList<OutputSpec> SearchResults { get; set; } = new List<OutputSpec>();

    // Tree browse search (REQ-5)
    public string TreeSearchQuery { get; set; } = string.Empty;

    // AI semantic search (REQ-6, REQ-7, REQ-8, REQ-9)
    public string AiSearchQuery { get; set; } = string.Empty;
    public IList<AiSearchMatchSpec> AiSearchResults { get; set; } = new List<AiSearchMatchSpec>();
    public string? AiSearchError { get; set; }
    public bool IsAiSearching { get; set; }

    // Dialog visibility
    public bool ShowDeliverablesDialog { get; set; }

    /// <summary>
    /// Opens the dialog and resets ALL search/selection state (PNO-964 fix).
    /// Mirrors openDeliverablesDialog() in opportunity-what-section.component.ts.
    /// </summary>
    public void OpenDialog()
    {
        IsEditingDeliverable = false;
        EditingDeliverableIndex = null;
        SelectedOutputsForDialog = new List<OutputSpec>();

        // REQ-2 through REQ-10: Clear all search state
        SearchQuery = string.Empty;
        SearchResults = new List<OutputSpec>();
        TreeSearchQuery = string.Empty;
        AiSearchQuery = string.Empty;
        AiSearchResults = new List<AiSearchMatchSpec>();
        AiSearchError = null;
        IsAiSearching = false;

        ShowDeliverablesDialog = true;
    }

    /// <summary>
    /// Closes the dialog and clears search/selection state.
    /// Mirrors closeDeliverablesDialog() in opportunity-what-section.component.ts.
    /// </summary>
    public void CloseDialog()
    {
        ShowDeliverablesDialog = false;
        SearchQuery = string.Empty;
        SearchResults = new List<OutputSpec>();
        SelectedOutputsForDialog = new List<OutputSpec>();
    }

    /// <summary>
    /// Simulates populating state as if user had searched (for Open→Search→Close→Reopen tests).
    /// </summary>
    public void SimulateSearchPerformed(string query, int resultCount = 3)
    {
        SearchQuery = query;
        SearchResults = Enumerable.Range(1, resultCount).Select(i => new OutputSpec { Id = i, Name = $"Output {i}" }).ToList();
    }

    /// <summary>
    /// Simulates tree search state.
    /// </summary>
    public void SimulateTreeSearch(string query)
    {
        TreeSearchQuery = query;
    }

    /// <summary>
    /// Simulates AI search state (query, results, loading, error).
    /// </summary>
    public void SimulateAiSearch(string query, int resultCount = 2, bool isSearching = false, string? error = null)
    {
        AiSearchQuery = query;
        AiSearchResults = Enumerable.Range(1, resultCount).Select(i => new AiSearchMatchSpec { OutputId = i, Score = 0.9 }).ToList();
        IsAiSearching = isSearching;
        AiSearchError = error;
    }

    /// <summary>
    /// Simulates user selecting outputs in the dialog.
    /// </summary>
    public void SimulateOutputsSelected(params OutputSpec[] outputs)
    {
        SelectedOutputsForDialog = outputs.ToList();
    }

    /// <summary>
    /// Returns true if all search-related state is empty (clean state).
    /// </summary>
    public bool IsSearchStateClean()
    {
        return string.IsNullOrEmpty(SearchQuery)
            && (SearchResults == null || SearchResults.Count == 0)
            && string.IsNullOrEmpty(TreeSearchQuery)
            && string.IsNullOrEmpty(AiSearchQuery)
            && (AiSearchResults == null || AiSearchResults.Count == 0)
            && AiSearchError == null
            && !IsAiSearching
            && (SelectedOutputsForDialog == null || SelectedOutputsForDialog.Count == 0);
    }
}

/// <summary>
/// Minimal output representation for dialog state tests.
/// </summary>
public sealed class OutputSpec
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Minimal AI search match representation.
/// </summary>
public sealed class AiSearchMatchSpec
{
    public int OutputId { get; set; }
    public double Score { get; set; }
}
