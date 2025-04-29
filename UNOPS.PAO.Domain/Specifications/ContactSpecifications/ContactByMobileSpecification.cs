namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by mobile number
/// </summary>
public class ContactByMobileSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by mobile number
    /// </summary>
    /// <param name="mobile">The mobile number to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByMobileSpecification(string mobile, bool exactMatch = false)
        : base(BuildSearchExpression(mobile, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided mobile number
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string mobile, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(mobile))
        {
            return c => true; // Match all if no mobile provided
        }
        
        // Normalize mobile number by removing non-numeric characters for comparison
        string normalizedMobile = new string(mobile.Where(char.IsDigit).ToArray());
        
        if (exactMatch)
        {
            return c => c.Mobile != null && 
                        new string(c.Mobile.Where(char.IsDigit).ToArray()) == normalizedMobile;
        }
        else
        {
            return c => c.Mobile != null && c.Mobile.Contains(mobile);
        }
    }
} 