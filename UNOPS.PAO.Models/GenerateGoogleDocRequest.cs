using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Models;

public class GenerateGoogleDocRequest
{
    [Required]
    public string Data { get; set; }
    
    public string? Filename { get; set; }
}

