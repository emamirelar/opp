namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A composite specification that allows filtering contacts by multiple criteria
/// </summary>
public class ContactCompositeSpecification : GenericCompositeSpecification<Contact, IContactSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for contacts
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public ContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include the related partner
        AddInclude(c => c.Partner);
        
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
    }
} 