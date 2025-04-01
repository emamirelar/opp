using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models;

public class LinkRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkEntityType Entity { get; set; }
    public int EntityId { get; set; }
    public string Url { get; set; }
    public string? Description { get; set; }
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
    public string Url { get; set; }
    public string? Description { get; set; }
} 