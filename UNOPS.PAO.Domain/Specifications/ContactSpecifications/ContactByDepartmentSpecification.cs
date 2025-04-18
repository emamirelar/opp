namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by department
/// </summary>
public class ContactByDepartmentSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by department
    /// </summary>
    /// <param name="department">The department to filter by</param>
    /// <param name="exactMatch">If true, requires an exact match. If false, uses contains</param>
    public ContactByDepartmentSpecification(string department, bool exactMatch = false)
        : base(BuildSearchExpression(department, exactMatch))
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
    
    /// <summary>
    /// Builds the search expression based on the provided department
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildSearchExpression(string department, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(department))
        {
            return c => true; // Match all if no department provided
        }
        
        // Always perform case-insensitive search
        string lowerDepartment = department.ToLower();
        
        if (exactMatch)
        {
            return c => c.Department != null && c.Department.ToLower() == lowerDepartment;
        }
        else
        {
            return c => c.Department != null && c.Department.ToLower().Contains(lowerDepartment);
        }
    }
} 