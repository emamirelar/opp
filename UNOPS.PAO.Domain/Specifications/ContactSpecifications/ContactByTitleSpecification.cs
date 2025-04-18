namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by title
/// </summary>
public class ContactByTitleSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by title
    /// </summary>
    /// <param name="title">The title to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByTitleSpecification(string title, bool exactMatch = false)
        : base(BuildSearchExpression(title, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided title
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string title, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return c => true; // Match all if no title provided
        }
        
        // Always perform case-insensitive search
        string lowerTitle = title.ToLower();
        
        if (exactMatch)
        {
            return c => c.Title != null && c.Title.ToLower() == lowerTitle;
        }
        else
        {
            return c => c.Title != null && c.Title.ToLower().Contains(lowerTitle);
        }
    }
} 