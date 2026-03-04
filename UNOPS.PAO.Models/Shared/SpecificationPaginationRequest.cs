namespace UNOPS.PAO.Models.Shared;

using UNOPS.PAO.Domain.Specifications;

/// <summary>
/// A pagination request that supports using a specification to filter results
/// </summary>
/// <typeparam name="TEntity">The entity type the specification targets</typeparam>
public class SpecificationPaginationRequest<TEntity> : PaginationRequest
{
    /// <summary>
    /// The specification to apply when querying entities
    /// </summary>
    public ISpecification<TEntity> Specification { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public SpecificationPaginationRequest() : base()
    {
    }
    
    /// <summary>
    /// Constructor with specification and pagination parameters
    /// </summary>
    /// <param name="specification">The specification to apply</param>
    /// <param name="pageIndex">Page index (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Property to order by</param>
    /// <param name="ascending">Direction to order by</param>
    public SpecificationPaginationRequest(ISpecification<TEntity> specification, int pageIndex, int pageSize,
        string? orderBy = null, bool? ascending = null) 
        : base(pageIndex, pageSize, orderBy, ascending)
    {
        Specification = specification;
    }
} 