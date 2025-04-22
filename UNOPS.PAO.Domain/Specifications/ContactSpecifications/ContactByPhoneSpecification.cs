namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by phone number
/// </summary>
public class ContactByPhoneSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by phone number
    /// </summary>
    /// <param name="phone">The phone number to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByPhoneSpecification(string phone, bool exactMatch = false)
        : base(BuildSearchExpression(phone, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided phone number
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string phone, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return c => true; // Match all if no phone provided
        }
        
        // Normalize phone number by removing non-numeric characters for comparison
        string normalizedPhone = new string(phone.Where(char.IsDigit).ToArray());
        
        if (exactMatch)
        {
            return c => c.Phone != null && 
                        new string(c.Phone.Where(char.IsDigit).ToArray()) == normalizedPhone;
        }
        else
        {
            return c => c.Phone != null && c.Phone.Contains(phone);
        }
    }
} 