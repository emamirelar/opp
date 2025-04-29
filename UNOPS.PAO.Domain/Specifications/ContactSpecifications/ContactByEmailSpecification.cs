namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by email
/// </summary>
public class ContactByEmailSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by email
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByEmailSpecification(string email, bool exactMatch = false)
        : base(BuildSearchExpression(email, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided email
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string email, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return c => true; // Match all if no email provided
        }
        
        // Always perform case-insensitive search
        string lowerEmail = email.ToLower();
        
        if (exactMatch)
        {
            return c => c.Email != null && c.Email.ToLower() == lowerEmail;
        }
        else
        {
            return c => c.Email != null && c.Email.ToLower().Contains(lowerEmail);
        }
    }
} 