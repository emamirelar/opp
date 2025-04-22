namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by ID
/// </summary>
public class ContactByIdSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by ID
    /// </summary>
    /// <param name="id">The contact ID to filter by</param>
    public ContactByIdSpecification(int id)
        : base(c => c.Id == id)
    {
        // Include the related partner
        AddInclude(c => c.Partner);
    }
} 