namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by name
/// </summary>
public class ContactByNameSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by name
    /// </summary>
    /// <param name="searchText">The text to search for in first or last name</param>
    public ContactByNameSpecification(string searchText)
        : base(BuildSearchExpression(searchText))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided text
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return c => true; // Match all if no search text provided
        }
        
        // Always perform case-insensitive search
        string lowerSearchText = searchText.ToLower();
        return c => 
            (c.FirstName != null && c.FirstName.ToLower().Contains(lowerSearchText)) ||
            (c.LastName != null && c.LastName.ToLower().Contains(lowerSearchText));
    }
} 