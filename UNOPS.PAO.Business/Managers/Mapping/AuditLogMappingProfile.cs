using AutoMapper;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.AuditLogs;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Managers.Mapping;

public class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<AuditLog, AuditLogModel>();
        CreateMap<UNOPSAuditLog, AuditLogModel>();
        CreateMap<AuditLogCreateRequest, AuditLog>();
        CreateMap<AuditLogCreateRequest, UNOPSAuditLog>();
    }
}

