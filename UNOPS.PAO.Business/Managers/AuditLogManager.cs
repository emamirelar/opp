using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.Business.Managers;

public class AuditLogManager : IAuditLogManager
{
    private readonly IMapper _mapper;
    private readonly DataRepository<AuditLog> _repository;
    private readonly AppDbContext _context;

    public AuditLogManager(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _context = context;
        _repository = new DataRepository<AuditLog>(context);
    }

    public async Task<AuditLogModel> CreateAuditLogAsync(AuditLogCreateRequest request)
    {
        var auditLog = new AuditLog
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

        await _repository.AddAsync(auditLog);
        
        return _mapper.Map<AuditLogModel>(auditLog);
    }

    public async Task<AuditLogModel?> GetLatestAuditLogAsync(string entityType, int entityId)
    {
        var auditLog = await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        return auditLog != null ? _mapper.Map<AuditLogModel>(auditLog) : null;
    }

    public async Task<IEnumerable<AuditLogModel>> GetAuditLogsAsync(string entityType, int entityId)
    {
        var auditLogs = await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AuditLogModel>>(auditLogs);
    }
}

