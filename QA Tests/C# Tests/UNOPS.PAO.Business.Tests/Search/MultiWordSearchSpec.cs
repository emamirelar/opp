/**
 * @fileoverview Spec that models the multi-word search logic from UNOPSUserManagementManager.GetUsersAsync (PNO-1211).
 * Replicates the exact algorithm: split search term by spaces, each word must match FirstName OR LastName OR Email,
 * words joined with AND. Used for testing without requiring PostgreSQL.
 * @author QA Team
 */

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Models the search term splitting and WHERE clause generation logic from UNOPSUserManagementManager.
/// PNO-1211 fix: "John Doe" splits into ["john","doe"], each term matches (FirstName OR LastName OR Email),
/// terms joined with AND.
/// </summary>
public sealed class MultiWordSearchSpec
{
    /// <summary>
    /// Split terms produced by the production logic: ToLower().Split(' ', RemoveEmptyEntries).
    /// </summary>
    public IReadOnlyList<string> SplitTerms { get; }

    /// <summary>
    /// SQL parameters (e.g. "%john%", "%doe%") in order.
    /// </summary>
    public IReadOnlyList<string> Parameters { get; }

    /// <summary>
    /// Whether any search filter was added (false for null/empty/whitespace).
    /// </summary>
    public bool HasSearchFilter { get; }

    public MultiWordSearchSpec(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            SplitTerms = Array.Empty<string>();
            Parameters = Array.Empty<string>();
            HasSearchFilter = false;
            return;
        }

        var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            SplitTerms = Array.Empty<string>();
            Parameters = Array.Empty<string>();
            HasSearchFilter = false;
            return;
        }

        SplitTerms = terms;
        Parameters = terms.Select(t => $"%{t}%").ToList();
        HasSearchFilter = true;
    }

    /// <summary>
    /// Applies the same matching logic as the SQL: each term must match at least one of FirstName, LastName, or Email (case-insensitive).
    /// REQ-7: Multi-word uses AND — each word must match.
    /// </summary>
    public bool Matches(string? firstName, string? lastName, string? email)
    {
        if (!HasSearchFilter) return true;

        var fn = (firstName ?? "").ToLower();
        var ln = (lastName ?? "").ToLower();
        var em = (email ?? "").ToLower();

        foreach (var term in SplitTerms)
        {
            var termMatches = fn.Contains(term) || ln.Contains(term) || em.Contains(term);
            if (!termMatches) return false;
        }
        return true;
    }

    /// <summary>
    /// Builds the WHERE clause fragment (for verification) — same structure as production.
    /// </summary>
    public string BuildWhereClauseFragment(int paramIndexStart)
    {
        if (!HasSearchFilter) return "";

        var termConditions = new List<string>();
        var idx = paramIndexStart;
        foreach (var _ in SplitTerms)
        {
            termConditions.Add($@"(
                LOWER(up.""FirstName"") LIKE @p{idx} OR 
                LOWER(up.""LastName"") LIKE @p{idx} OR 
                LOWER(up.""UserEmail"") LIKE @p{idx}
            )");
            idx++;
        }
        return $"({string.Join(" AND ", termConditions)})";
    }
}
