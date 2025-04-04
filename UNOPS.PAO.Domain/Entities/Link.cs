using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Domain.Entities;

public class Link : ModifiableDeletableEntity
{
    public int Id { get; set; }
    
    [Column(TypeName = "text")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkEntityType Entity { get; set; }
    
    public int EntityId { get; set; }
    public string Url { get; set; }
} 