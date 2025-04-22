namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by assistant phone
/// </summary>
public class ContactByAssistantPhoneSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by assistant phone
    /// </summary>
    /// <param name="assistantPhone">The assistant phone to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByAssistantPhoneSpecification(string assistantPhone, bool exactMatch = false)
        : base(BuildSearchExpression(assistantPhone, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided assistant phone
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string assistantPhone, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(assistantPhone))
        {
            return c => true; // Match all if no assistant phone provided
        }
        
        // Normalize phone number by removing non-numeric characters for comparison
        string normalizedPhone = new string(assistantPhone.Where(char.IsDigit).ToArray());
        
        if (exactMatch)
        {
            return c => c.AssistantPhone != null && 
                        new string(c.AssistantPhone.Where(char.IsDigit).ToArray()) == normalizedPhone;
        }
        else
        {
            return c => c.AssistantPhone != null && c.AssistantPhone.Contains(assistantPhone);
        }
    }
} 