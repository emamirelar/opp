namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by mailing country
/// </summary>
public class ContactByMailingCountrySpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by mailing country
    /// </summary>
    /// <param name="country">The country to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByMailingCountrySpecification(string country, bool exactMatch = false)
        : base(BuildSearchExpression(country, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided country
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string country, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return c => true; // Match all if no country provided
        }
        
        // Always perform case-insensitive search
        string lowerCountry = country.ToLower();
        
        if (exactMatch)
        {
            return c => c.MailingCountry != null && c.MailingCountry.ToLower() == lowerCountry;
        }
        else
        {
            return c => c.MailingCountry != null && c.MailingCountry.ToLower().Contains(lowerCountry);
        }
    }
} 