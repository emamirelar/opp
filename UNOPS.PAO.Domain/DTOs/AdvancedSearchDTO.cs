using System.Collections.Generic;

namespace UNOPS.PAO.Domain.DTOs;

public class AdvancedSearchDTO
{
    public string? GeneralSearch { get; set; }
    public List<SearchCriterionDTO>? Criteria { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
}

public class SearchCriterionDTO
{
    public string Field { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Operator { get; set; } = null!;  // is, like, >, <, etc.
    public string? LogicalOperator { get; set; }  // AND, OR
} 