using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class OpportunityCountry : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Parent opportunity
    /// </summary>
    public int OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }
    
    /// <summary>
    /// Country of implementation
    /// </summary>
    public int CountryId { get; set; }
    public virtual Country? Country { get; set; }
    
    /// <summary>
    /// Specific districts/municipalities/areas within the country (comma-separated or JSON)
    /// </summary>
    public string? SpecificAreas { get; set; }
}

