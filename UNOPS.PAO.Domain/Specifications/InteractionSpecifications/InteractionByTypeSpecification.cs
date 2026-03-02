namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

/// <summary>
/// Specification that filters interactions by type
/// </summary>
public class InteractionByTypeSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a specification that filters interactions by their type
    /// </summary>
    /// <param name="type">The interaction type to filter by</param>
    public InteractionByTypeSpecification(InteractionType type)
        : base(i => i.Type == type)
    {
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
        
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
    }
} 