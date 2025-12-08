namespace UNOPS.PAO.Models.OrganizationUnits;

/// <summary>
/// Response model for suggested organization units based on countries of implementation
/// </summary>
public class SuggestedOrgUnitsResponse
{
    /// <summary>
    /// List of suggested organization unit IDs
    /// </summary>
    public List<int> SuggestedOrgUnitIds { get; set; } = new();

    /// <summary>
    /// The primary suggested org unit ID (the most recommended one)
    /// </summary>
    public int? PrimarySuggestionId { get; set; }

    /// <summary>
    /// Reason for the suggestion:
    /// - "responsible_for_all_countries" - single org unit responsible for all countries
    /// - "common_parent_for_multiple_countries" - common parent when multiple org units are responsible
    /// - "multiple_responsible_units" - multiple org units with no clear parent
    /// </summary>
    public string? SuggestionReason { get; set; }
}

