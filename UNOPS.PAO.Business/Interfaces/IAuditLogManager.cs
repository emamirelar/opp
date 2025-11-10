using UNOPS.PAO.Models.AuditLogs;

namespace UNOPS.PAO.Business.Interfaces;

public interface IAuditLogManager
{
    /// <summary>
    /// Creates an audit log entry
    /// </summary>
    Task<AuditLogModel> CreateAuditLogAsync(AuditLogCreateRequest request);
    
    /// <summary>
    /// Gets the latest audit log entry for a specific entity
    /// </summary>
    Task<AuditLogModel?> GetLatestAuditLogAsync(string entityType, int entityId);
    
    /// <summary>
    /// Gets all audit log entries for a specific entity
    /// </summary>
    Task<IEnumerable<AuditLogModel>> GetAuditLogsAsync(string entityType, int entityId);
}

