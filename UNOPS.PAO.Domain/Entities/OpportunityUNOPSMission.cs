using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Junction table linking Opportunities to UNOPS Strategic Missions
/// </summary>
public class OpportunityUNOPSMission : ModifiableDeletableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
    
    public new string? Name { get; set; }
    
    /// <summary>
    /// Foreign key to the Opportunity
    /// </summary>
    public int OpportunityId { get; set; }

    /// <summary>
    /// Navigation property to the Opportunity
    /// </summary>
    public virtual Opportunity? Opportunity { get; set; }

    /// <summary>
    /// Foreign key to the UNOPS Mission
    /// </summary>
    public int UNOPSMissionId { get; set; }

    /// <summary>
    /// Navigation property to the UNOPS Mission
    /// </summary>
    public virtual UNOPSMission? UNOPSMission { get; set; }
}

