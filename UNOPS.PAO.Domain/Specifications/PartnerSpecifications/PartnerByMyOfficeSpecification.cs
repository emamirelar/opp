namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter partners by current user's organizational unit
/// </summary>
public class PartnerByMyOfficeSpecification : BaseSpecification<Partner>
{
    public PartnerByMyOfficeSpecification(string userOrgUnit) 
        : base(BuildCriteria(userOrgUnit))
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
    }

    private static Expression<Func<Partner, bool>> BuildCriteria(string userOrgUnit)
    {
        if (string.IsNullOrEmpty(userOrgUnit))
        {
            // If no org unit specified, return no results for security
            return p => false;
        }

        // Filter by PartnerOffice.Code matching user's OrgUnit
        return p => p.PartnerOffice != null && p.PartnerOffice.Code == userOrgUnit;
    }
}