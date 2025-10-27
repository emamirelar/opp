namespace UNOPS.PAO.Models.Search;

/// <summary>
/// Represents a single search filter criterion for dynamic search
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// The field to search on (supports navigation properties like "contacts.firstName")
    /// </summary>
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// The search operator (eq, like, neq, gt, lt, gte, lte, in)
    /// </summary>
    public string Operator { get; set; } = "eq";
    
    /// <summary>
    /// The value to search for
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Logical operator to combine with the next filter (AND, OR)
    /// </summary>
    public string LogicalOperator { get; set; } = "AND";
    
    /// <summary>
    /// The data type of the field (text, number, date, bool)
    /// </summary>
    public string FieldType { get; set; } = "text";
}
