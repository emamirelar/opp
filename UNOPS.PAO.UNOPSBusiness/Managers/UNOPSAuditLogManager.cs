using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Models.AuditLogs;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Security.Claims;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class UNOPSAuditLogManager : BaseUNOPSManager, IAuditLogManager
{
    public UNOPSAuditLogManager(
        IMapper mapper,
        UNOPSAppDbContext context,
        IConfiguration configuration,
        UserManager<PAOIdentityUser>? userManager = null,
        IPermissionService? permissionService = null,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(mapper, context, configuration, userManager, "AuditLog", permissionService, httpContextAccessor)
    {
    }

    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        var auditLog = await _context.AuditLogs
            .AsNoTracking() // ✅ Read-only query - no updates needed
            .FirstOrDefaultAsync(a => a.Id == entityId && !a.IsDeleted);

        return auditLog != null ? _mapper.Map<AuditLogModel>(auditLog) : null;
    }

    public async Task<AuditLogModel> CreateAuditLogAsync(AuditLogCreateRequest request)
    {
        var auditLog = new UNOPSAuditLog
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            Timestamp = DateTime.UtcNow,
            UserId = request.UserId,
            JsonData = request.JsonData,
            Description = request.Description,
            Name = $"{request.EntityType}_{request.EntityId}_{request.Action}"
        };

        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<AuditLogModel>(auditLog);
    }

    public async Task<AuditLogModel?> GetLatestAuditLogAsync(string entityType, int entityId)
    {
        var auditLog = await _context.AuditLogs
            .AsNoTracking() // ✅ Read-only query - audit logs are immutable
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        return auditLog != null ? _mapper.Map<AuditLogModel>(auditLog) : null;
    }

    public async Task<IEnumerable<AuditLogModel>> GetAuditLogsAsync(string entityType, int entityId)
    {
        var auditLogs = await _context.AuditLogs
            .AsNoTracking() // ✅ Read-only query - audit logs used for display only
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AuditLogModel>>(auditLogs);
    }
}

