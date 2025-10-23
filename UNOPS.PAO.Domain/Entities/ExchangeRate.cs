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
    /// Exchange Rate Name (computed or descriptive)
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
    /// Currency (FROM currency code - e.g., "USD", "EUR")
    /// </summary>
    [MaxLength(10)]
    public string? Currency { get; set; }
    
    /// <summary>
    /// Currency_Description (FROM currency description)
    /// </summary>
    [MaxLength(500)]
    public string? Currency_Description { get; set; }
    
    /// <summary>
    /// Registered_Rate (official registered exchange rate)
    /// </summary>
    [Column(TypeName = "decimal(18, 8)")]
    public decimal? Registered_Rate { get; set; }
    
    /// <summary>
    /// Exchange_Rate (actual exchange rate to be used)
    /// </summary>
    [Column(TypeName = "decimal(18, 8)")]
    public decimal? Exchange_Rate { get; set; }
    
    /// <summary>
    /// Currency_Type (type of currency or exchange rate)
    /// </summary>
    [MaxLength(100)]
    public string? Currency_Type { get; set; }
    
    /// <summary>
    /// Effective_Date (date when this exchange rate becomes effective)
    /// </summary>
    public DateTime? Effective_Date { get; set; }
    
    /// <summary>
    /// Exchange_Rate_Start_Date (start date for this exchange rate)
    /// </summary>
    public DateTime? Exchange_Rate_Start_Date { get; set; }
    
    /// <summary>
    /// Exchange_Rate_End_Date (end date for this exchange rate)
    /// </summary>
    public DateTime? Exchange_Rate_End_Date { get; set; }
    
    /// <summary>
    /// Is_Current_Flag (indicates if this is the current/active exchange rate)
    /// 0 = Not Current, 1 = Current
    /// </summary>
    public int? Is_Current_Flag { get; set; }
    
    /// <summary>
    /// Exchange_Rate_Sequence_No (sequence number for ordering exchange rates)
    /// </summary>
    public int? Exchange_Rate_Sequence_No { get; set; }
    
    /// <summary>
    /// Exchange_Rate_Line_Source (source system or origin of this exchange rate)
    /// </summary>
    [MaxLength(500)]
    public string? Exchange_Rate_Line_Source { get; set; }
    
    /// <summary>
    /// Rate_Expiration (expiration date for this exchange rate)
    /// </summary>
    public DateTime? Rate_Expiration { get; set; }
}

