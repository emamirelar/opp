namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that includes the Contact navigation property
/// </summary>
public class InteractionWithContactSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a specification that includes the Contact navigation property
    /// </summary>
    public InteractionWithContactSpecification()
        : base(i => true) // Match all interactions
    {
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
        
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Creates a specification that retrieves a specific interaction by ID and includes its Contact
    /// </summary>
    /// <param name="interactionId">The interaction ID to retrieve</param>
    public InteractionWithContactSpecification(int interactionId)
        : base(i => i.Id == interactionId)
    {
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
    }
} 