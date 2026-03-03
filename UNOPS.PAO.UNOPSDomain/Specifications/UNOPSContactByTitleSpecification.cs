namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// Specification that filters UNOPS contacts by title
/// </summary>
public class UNOPSContactByTitleSpecification : BaseSpecification<UNOPSContact>
{
    /// <summary>
    /// Creates a specification that filters UNOPS contacts by title
    /// </summary>
    /// <param name="title">The title to filter by</param>
    public UNOPSContactByTitleSpecification(string? title)
        : base(string.IsNullOrEmpty(title) ? c => true : c => c.Title == title)
    {
        // Include related entities
        AddInclude(c => c.Partner!);
    }
}