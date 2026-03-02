namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification for paginated lists of interactions
/// </summary>
public class PagedInteractionSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a paged specification for interactions
    /// </summary>
    /// <param name="pageIndex">The page index (starting at 1)</param>
    /// <param name="pageSize">The page size</param>
    public PagedInteractionSpecification(int pageIndex, int pageSize) 
        : base(i => true) // Match all interactions
    {
        if (pageIndex < 1)
            pageIndex = 1;
            
        if (pageSize < 1)
            pageSize = 10;
            
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
        ApplyOrderByDescending(i => i.Date);
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
    }
} 