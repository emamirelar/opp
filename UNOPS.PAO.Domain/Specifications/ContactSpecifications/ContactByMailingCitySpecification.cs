namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by mailing city
/// </summary>
public class ContactByMailingCitySpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by mailing city
    /// </summary>
    /// <param name="city">The city to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByMailingCitySpecification(string city, bool exactMatch = false)
        : base(BuildSearchExpression(city, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided city
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string city, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return c => true; // Match all if no city provided
        }
        
        // Always perform case-insensitive search
        string lowerCity = city.ToLower();
        
        if (exactMatch)
        {
            return c => c.MailingCity != null && c.MailingCity.ToLower() == lowerCity;
        }
        else
        {
            return c => c.MailingCity != null && c.MailingCity.ToLower().Contains(lowerCity);
        }
    }
} 