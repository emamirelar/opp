namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter UNOPS partners by current user's organizational unit
/// </summary>
public class UNOPSPartnerByMyOfficeSpecification : BaseSpecification<UNOPSPartner>
{
    public UNOPSPartnerByMyOfficeSpecification(string userOrgUnit) 
        : base(BuildCriteria(userOrgUnit))
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
    }

    private static Expression<Func<UNOPSPartner, bool>> BuildCriteria(string userOrgUnit)
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