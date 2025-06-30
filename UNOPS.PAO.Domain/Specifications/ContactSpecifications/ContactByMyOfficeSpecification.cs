namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter contacts by current user's organizational unit (via partner office)
/// </summary>
public class ContactByMyOfficeSpecification : BaseSpecification<Contact>
{
    public ContactByMyOfficeSpecification(string userOrgUnit) 
        : base(BuildCriteria(userOrgUnit))
    {
        // Include related entities
        AddInclude(c => c.Partner);
        AddInclude("Partner.PartnerOffice");
    }

    private static Expression<Func<Contact, bool>> BuildCriteria(string userOrgUnit)
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