using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;
using System.Text;

namespace UNOPS.PAO.Models;

public class InteractionModel
{
    public int Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InteractionType Type { get; set; }
    public DateTime Date { get; set; }

    [JsonIgnore]
    public byte[]? Data { get; set; }
    
    [JsonPropertyName("data")]
    public string? TextData 
    { 
        get => Data != null ? Encoding.UTF8.GetString(Data) : null;
        set => Data = value != null ? Encoding.UTF8.GetBytes(value) : null;
    }
    
    public int ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
} 