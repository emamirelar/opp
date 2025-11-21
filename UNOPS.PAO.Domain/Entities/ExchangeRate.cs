using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Exchange Rate entity for currency conversion
/// Data synced from External Data Service - Read Only
/// </summary>
public class ExchangeRate : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// Exchange Rate Name (computed: "Currency - Rate: X.XX (Effective: YYYY-MM-DD)")
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Currency code (e.g., "USD", "EUR", "GBP")
    /// </summary>
    [MaxLength(10)]
    public string? Currency { get; set; }
    
    /// <summary>
    /// Effective_Date - Date when this exchange rate becomes effective
    /// </summary>
    public DateTime? Effective_Date { get; set; }
    
    /// <summary>
    /// Exchange_Rate_Sequence_No - Sequence number for ordering exchange rates
    /// </summary>
    public int? Exchange_Rate_Sequence_No { get; set; }
    
    /// <summary>
    /// Exchange_Rate - Actual exchange rate value to be used for conversions
    /// </summary>
    [Column(TypeName = "decimal(18, 8)")]
    public decimal? Exchange_Rate { get; set; }
}


