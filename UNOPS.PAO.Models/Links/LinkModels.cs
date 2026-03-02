using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Models.Links;

public class LinkRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkEntityType Entity { get; set; }
    public int EntityId { get; set; }
    [StringLength(2000)]
    public string Url { get; set; } = null!;
    [StringLength(2000)]
    public string? Name { get; set; }
}

public class UpdateLinkRequest : LinkRequest
{
    public int Id { get; set; }
}

public class LinkModel
{
    public int Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkEntityType Entity { get; set; }
    public int EntityId { get; set; }
    public string Url { get; set; } = null!;
    public string? Name { get; set; }
} 