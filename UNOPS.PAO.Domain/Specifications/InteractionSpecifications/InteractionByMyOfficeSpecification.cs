namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter interactions by current user's organizational unit (via contact's partner office)
/// </summary>
public class InteractionByMyOfficeSpecification : BaseSpecification<Interaction>
{
    public InteractionByMyOfficeSpecification(string userOrgUnit) 
        : base(BuildCriteria(userOrgUnit))
    {
        // Include related entities for many-to-many relationships
        AddInclude(i => i.InteractionContacts);
        AddInclude("InteractionContacts.Contact");
        AddInclude("InteractionContacts.Contact.Partner");
        AddInclude("InteractionContacts.Contact.Partner.PartnerOffice");
    }

    private static Expression<Func<Interaction, bool>> BuildCriteria(string userOrgUnit)
    {
        if (string.IsNullOrEmpty(userOrgUnit))
        {
            // If no org unit specified, return no results for security
            return i => false;
        }

        // Filter by any related Contact's Partner.PartnerOffice.Code matching user's OrgUnit
        return i => i.InteractionContacts != null && 
                   i.InteractionContacts.Any(ic => 
                       ic.Contact != null && 
                       ic.Contact.Partner != null && 
                       ic.Contact.Partner.PartnerOffice != null && 
                       ic.Contact.Partner.PartnerOffice.Code == userOrgUnit);
    }
}