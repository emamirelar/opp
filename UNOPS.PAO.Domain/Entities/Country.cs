using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class Country : BaseBusinessEntity
{
    public required string Iso2Code { get; set; }
    
    // Computed properties for list/search operations
    [NotMapped]
    public int PartnerCount { get; set; } // Will be populated by service
    
    [NotMapped]
    public int LiaisonOfficeCount { get; set; } // Will be populated by service
}
