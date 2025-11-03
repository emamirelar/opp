using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Currency master data entity
/// Data synced from External Data Service - Read Only
/// </summary>
public class Currency : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// Currency Name (e.g., "US Dollar", "Euro")
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// ISO 4217 currency code (e.g., "USD", "EUR", "KES")
    /// </summary>
    [MaxLength(3)]
    public required string Code { get; set; }
    
    /// <summary>
    /// Currency symbol (e.g., "$", "€", "KSh") - Optional
    /// </summary>
    [MaxLength(10)]
    public string? Symbol { get; set; }
    
    /// <summary>
    /// Number of decimal places (e.g., 2 for USD, 0 for JPY) - Optional
    /// </summary>
    public int? DecimalPlaces { get; set; }
}
