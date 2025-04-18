namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by mailing postal code
/// </summary>
public class ContactByMailingPostalCodeSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by mailing postal code
    /// </summary>
    /// <param name="postalCode">The postal code to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByMailingPostalCodeSpecification(string postalCode, bool exactMatch = false)
        : base(BuildSearchExpression(postalCode, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided postal code
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string postalCode, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            return c => true; // Match all if no postal code provided
        }
        
        // Remove spaces from postal code for comparison
        string normalizedPostalCode = postalCode.Replace(" ", "");
        
        if (exactMatch)
        {
            return c => c.MailingPostalCode != null && 
                       c.MailingPostalCode.Replace(" ", "") == normalizedPostalCode;
        }
        else
        {
            return c => c.MailingPostalCode != null && c.MailingPostalCode.Contains(postalCode);
        }
    }
} 