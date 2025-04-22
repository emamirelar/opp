namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters contacts by partner
/// </summary>
public class ContactByPartnerSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a specification that filters contacts by partner ID
    /// </summary>
    /// <param name="partnerId">The partner ID to filter by</param>
    public ContactByPartnerSpecification(int partnerId)
        : base(c => c.PartnerId == partnerId)
    {
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
        
        // Include the related partner
        AddInclude(c => c.Partner);
    }
} 