namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters interactions by contact
/// </summary>
public class InteractionByContactSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a specification that filters interactions by contact ID
    /// </summary>
    /// <param name="contactId">The contact ID to filter by</param>
    public InteractionByContactSpecification(int contactId)
        : base(i => i.InteractionContacts != null && i.InteractionContacts.Any(ic => ic.ContactId == contactId))
    {
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
        
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
    }
} 