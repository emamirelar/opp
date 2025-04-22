namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification for paginated lists of contacts
/// </summary>
public class PagedContactSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a paged specification for contacts
    /// </summary>
    /// <param name="pageIndex">The page index (starting at 1)</param>
    /// <param name="pageSize">The page size</param>
    public PagedContactSpecification(int pageIndex, int pageSize) 
        : base(c => true) // Match all contacts
    {
        if (pageIndex < 1)
            pageIndex = 1;
            
        if (pageSize < 1)
            pageSize = 10;
            
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
        ApplyOrderBy(c => c.LastName);
        AddInclude(c => c.Partner);
    }
} 