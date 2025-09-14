namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// Specification that filters UNOPS partners by status
/// </summary>
public class UNOPSPartnerByStatusSpecification : BaseSpecification<UNOPSPartner>
{
    /// <summary>
    /// Creates a specification that filters UNOPS partners by status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    public UNOPSPartnerByStatusSpecification(string? status)
        : base(BuildPredicate(status))
    {
        // Include related entities
    }
    
    private static System.Linq.Expressions.Expression<System.Func<UNOPSPartner, bool>> BuildPredicate(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return p => true;
        }
        
                        return p => p.Status.ToString() == status;
    }
}