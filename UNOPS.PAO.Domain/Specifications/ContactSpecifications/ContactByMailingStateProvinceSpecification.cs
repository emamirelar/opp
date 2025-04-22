namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by mailing state/province
/// </summary>
public class ContactByMailingStateProvinceSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by mailing state/province
    /// </summary>
    /// <param name="stateProvince">The state/province to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByMailingStateProvinceSpecification(string stateProvince, bool exactMatch = false)
        : base(BuildSearchExpression(stateProvince, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided state/province
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string stateProvince, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(stateProvince))
        {
            return c => true; // Match all if no state/province provided
        }
        
        // Always perform case-insensitive search
        string lowerStateProvince = stateProvince.ToLower();
        
        if (exactMatch)
        {
            return c => c.MailingStateProvince != null && c.MailingStateProvince.ToLower() == lowerStateProvince;
        }
        else
        {
            return c => c.MailingStateProvince != null && c.MailingStateProvince.ToLower().Contains(lowerStateProvince);
        }
    }
} 