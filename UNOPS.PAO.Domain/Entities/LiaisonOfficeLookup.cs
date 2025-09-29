using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Lookup entity for LiaisonOffice - simplified version for dropdown/lookup scenarios
/// </summary>
public class LiaisonOfficeLookup : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string Code { get; set; }
    
    [Required]
    [MaxLength(250)]
    public required string Name { get; set; }
    
    [MaxLength(100)]
    public string? Region { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // For dropdown display
    public string DisplayName => $"{Code} - {Name}";
}