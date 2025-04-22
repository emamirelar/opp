namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by description
/// </summary>
public class ContactByDescriptionSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by description
    /// </summary>
    /// <param name="description">The description text to filter by</param>
    public ContactByDescriptionSpecification(string description)
        : base(BuildSearchExpression(description))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided description
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return c => true; // Match all if no description provided
        }
        
        // Always perform case-insensitive search
        string lowerDescription = description.ToLower();
        return c => c.Description != null && c.Description.ToLower().Contains(lowerDescription);
    }
} 