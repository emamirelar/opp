namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification for paginated lists of partners
/// </summary>
public class PagedPartnerSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a paged specification for partners
    /// </summary>
    /// <param name="pageIndex">The page index (starting at 1)</param>
    /// <param name="pageSize">The page size</param>
    public PagedPartnerSpecification(int pageIndex, int pageSize) 
        : base(p => true) // Match all partners
    {
        if (pageIndex < 1)
            pageIndex = 1;
            
        if (pageSize < 1)
            pageSize = 10;
            
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);
        ApplyOrderBy(p => p.Name);
    }
} 