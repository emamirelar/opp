namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by status
/// </summary>
public class ContactByStatusSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    public ContactByStatusSpecification(string status)
        : base(c => c.Status == status)
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
} 