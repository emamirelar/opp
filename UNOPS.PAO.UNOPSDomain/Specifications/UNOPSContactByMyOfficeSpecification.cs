namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter UNOPS contacts by current user's organizational unit (via partner office)
/// </summary>
public class UNOPSContactByMyOfficeSpecification : BaseSpecification<UNOPSContact>
{
    public UNOPSContactByMyOfficeSpecification(string userOrgUnit) 
        : base(BuildCriteria(userOrgUnit))
    {
        // Include related entities
        AddInclude(c => c.Partner);
        AddInclude("Partner.PartnerOffice");
    }

    private static Expression<Func<UNOPSContact, bool>> BuildCriteria(string userOrgUnit)
    {
        if (string.IsNullOrEmpty(userOrgUnit))
        {
            // If no org unit specified, return no results for security
            return c => false;
        }

        // Filter by Partner.PartnerOffice.Code matching user's OrgUnit
        return c => c.Partner != null && 
                   c.Partner.PartnerOffice != null && 
                   c.Partner.PartnerOffice.Code == userOrgUnit;
    }
}