using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;

public class PartnerTree : ModifiableDeletableEntity
{
    public static readonly string[] specialCategoryCodes = { "MULTILATERAL", "GOVERNMENT" };
    public string Description { get; set; }
    
    [Key]
    public string Code { get; set; }
    
    public string Type { get; set; }
    public string? Parent { get; set; }
    public string? PartnerCategoryCode { get; set; }
    public string? PartnerGroupCode { get; set; }
    
    [InverseProperty("PartnerGroup")]
    [JsonIgnore]
    public virtual ICollection<Partner> Partners { get; set; } = new List<Partner>();
}