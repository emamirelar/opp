namespace UNOPS.PAO.Models.Search;

/// <summary>
/// Information about a search field for dynamic search functionality
/// </summary>
public class SearchFieldInfo
{
    /// <summary>
    /// Field name (e.g., "name", "contacts.firstName")
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Field data type (text, number, date, bool)
    /// </summary>
    public string FieldType { get; set; } = "text";

    /// <summary>
    /// Whether this field is a navigation property
    /// </summary>
    public bool IsNavigationProperty { get; set; } = false;

    /// <summary>
    /// Navigation entity name if this is a navigation property
    /// </summary>
    public string? NavigationEntity { get; set; }

    /// <summary>
    /// List of allowed operators for this field
    /// </summary>
    public List<string> AllowedOperators { get; set; } = new List<string> { "eq", "neq", "like" };

    /// <summary>
    /// Available options for dropdown fields (enums, lookups, etc.)
    /// </summary>
    public List<DropdownOption>? DropdownOptions { get; set; }
}

/// <summary>
/// Option for dropdown fields
/// </summary>
public class DropdownOption
{
    /// <summary>
    /// Value to send to the backend
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Display text for the user (can be a translation key)
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
