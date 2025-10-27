using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models.Search;

/// <summary>
/// Represents a single search criterion for advanced search
/// </summary>
public class SearchCriteria
{
    /// <summary>
    /// The field name to search in (e.g., "firstName", "email", "partner.name")
    /// </summary>
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// The value to search for
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    /// <summary>
    /// The display label for the field (used by the UI)
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// The comparison operator to use (is, is not, like, not like, >, <, >=, <=, after, before, between)
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "like";
    
    /// <summary>
    /// The logical operator to combine this criterion with the previous one (AND, OR)
    /// </summary>
    [JsonPropertyName("logicalOperator")]
    public string? LogicalOperator { get; set; } = "AND";
    
    /// <summary>
    /// The second value for range operators like "between" (end date for date ranges)
    /// </summary>
    [JsonPropertyName("secondValue")]
    public string? SecondValue { get; set; }
    
    /// <summary>
    /// The field type to determine input method (text, date, number, etc.)
    /// </summary>
    [JsonPropertyName("fieldType")]
    public string? FieldType { get; set; }
} 