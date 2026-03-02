namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters interactions by text contained in the Description property
/// </summary>
public class InteractionByTextSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a specification that filters interactions by text search
    /// </summary>
    /// <param name="searchText">The text to search for in the interaction description</param>
    public InteractionByTextSpecification(string searchText)
        : base(BuildSearchExpression(searchText))
    {
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
        
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
    }
    
    /// <summary>
    /// Builds the search expression based on the provided text
    /// </summary>
    private static Expression<Func<Interaction, bool>> BuildSearchExpression(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return i => true; // Match all if no search text provided
        }
        
        // Always perform case-insensitive search
        string lowerSearchText = searchText.ToLower();
        return i => i.Description != null && i.Description.ToLower().Contains(lowerSearchText);
    }
} 