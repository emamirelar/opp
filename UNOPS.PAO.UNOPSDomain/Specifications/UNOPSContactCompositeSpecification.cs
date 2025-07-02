namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification that allows filtering UNOPS contacts by multiple criteria
/// </summary>
public class UNOPSContactCompositeSpecification : GenericCompositeSpecification<UNOPSContact, IContactSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for UNOPS contacts
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public UNOPSContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        AddInclude(c => c.Partner);
        
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
    }
}