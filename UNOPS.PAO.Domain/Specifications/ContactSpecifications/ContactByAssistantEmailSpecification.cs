namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by assistant email
/// </summary>
public class ContactByAssistantEmailSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by assistant email
    /// </summary>
    /// <param name="assistantEmail">The assistant email to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByAssistantEmailSpecification(string assistantEmail, bool exactMatch = false)
        : base(BuildSearchExpression(assistantEmail, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided assistant email
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string assistantEmail, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(assistantEmail))
        {
            return c => true; // Match all if no assistant email provided
        }
        
        // Always perform case-insensitive search
        string lowerAssistantEmail = assistantEmail.ToLower();
        
        if (exactMatch)
        {
            return c => c.AssistantEmail != null && c.AssistantEmail.ToLower() == lowerAssistantEmail;
        }
        else
        {
            return c => c.AssistantEmail != null && c.AssistantEmail.ToLower().Contains(lowerAssistantEmail);
        }
    }
} 