namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by assistant name
/// </summary>
public class ContactByAssistantSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by assistant name
    /// </summary>
    /// <param name="assistant">The assistant name to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByAssistantSpecification(string assistant, bool exactMatch = false)
        : base(BuildSearchExpression(assistant, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided assistant name
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string assistant, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(assistant))
        {
            return c => true; // Match all if no assistant provided
        }
        
        // Always perform case-insensitive search
        string lowerAssistant = assistant.ToLower();
        
        if (exactMatch)
        {
            return c => c.Assistant != null && c.Assistant.ToLower() == lowerAssistant;
        }
        else
        {
            return c => c.Assistant != null && c.Assistant.ToLower().Contains(lowerAssistant);
        }
    }
} 