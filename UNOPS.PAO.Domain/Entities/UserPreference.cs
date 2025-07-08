using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class UserPreference : ModifiableDeletableEntity
{
    public int UserId { get; set; }
    public virtual UserInfo User { get; set; }
    
    // Filtre OrgUnit par défaut (inclut toujours la hiérarchie)
    public int? DefaultOrgUnitId { get; set; }
    public virtual OrganizationHierarchy? DefaultOrgUnit { get; set; }
    
    // Pour futures préférences
    public string? PreferencesJson { get; set; }
}