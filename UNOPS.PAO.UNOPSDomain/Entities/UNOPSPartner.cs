using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class UNOPSPartner : Domain.Entities.Partner
{
    public string? PartnerCode { get; set; }
    [JsonIgnore]  // Prevents circular reference in serialization
    public virtual ICollection<Project>? Projects { get; set; }
}