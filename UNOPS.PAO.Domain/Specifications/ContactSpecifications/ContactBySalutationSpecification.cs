namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by salutation
/// </summary>
public class ContactBySalutationSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by salutation
    /// </summary>
    /// <param name="salutation">The salutation to filter by</param>
    public ContactBySalutationSpecification(string salutation)
        : base(c => c.Salutation == salutation)
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
} 