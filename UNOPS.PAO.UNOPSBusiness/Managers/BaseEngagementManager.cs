using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

public class BaseEngagementManager : BaseUNOPSManager, IBaseEngagementManager
{
    private readonly DataRepository<BaseEngagement> _engagementRepository;
    private readonly DataRepository<BaseEngagementPartners> _engagementPartnersRepository;
    
    public BaseEngagementManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration,
        IPermissionService permissionService,
        IHttpContextAccessor httpContextAccessor) 
        : base(mapper, context, configuration, null, "BaseEngagement", permissionService, httpContextAccessor)
    {
        _engagementRepository = new DataRepository<BaseEngagement>(context);
        _engagementPartnersRepository = new DataRepository<BaseEngagementPartners>(context);
    }
    
    public async Task<IEnumerable<BaseEngagementModel>> GetAllAsync(ClaimsPrincipal user)
    {
        var query = _engagementRepository.GetAll()
            .Where(e => !e.IsDeleted)
            .Include(x => x.EngagementPartners)
            .ThenInclude(ep => ep.PartnerEntity);

        // Apply permission filtering (read-only check)
        var filteredResult = await _permissionService.ApplyAccessControlFiltersAsync(query, user, "read", _entityName);
        
        // Handle both cases: IQueryable<BaseEngagement> or List<BaseEngagement>
        List<BaseEngagement> engagements;
        if (filteredResult is IQueryable<BaseEngagement> queryable)
        {
            // If it's still a queryable, execute it
            engagements = await queryable.ToListAsync();
        }
        else if (filteredResult is List<BaseEngagement> list)
        {
            // If it's already materialized as a list, use it directly
            engagements = list;
        }
        else
        {
            // Try to cast it as IEnumerable<BaseEngagement> and convert to list
            engagements = ((IEnumerable<BaseEngagement>)filteredResult).ToList();
        }
        
        return _mapper.Map<IEnumerable<BaseEngagementModel>>(engagements);
    }
    
    public async Task<BaseEngagementModel?> GetByIdAsync(ClaimsPrincipal user, int id)
    {
        var engagement = await _engagementRepository.GetAll()
            .Where(e => e.Id == id && !e.IsDeleted)
            .Include(x => x.EngagementPartners)
            .ThenInclude(ep => ep.PartnerEntity)
            .FirstOrDefaultAsync();
        
        if (engagement == null) return null;
        
        // Check read permission for this specific instance
        var hasReadAccess = await _permissionService.HasInstanceAccessAsync(_entityName, engagement, user, "read");
        if (!hasReadAccess) return null;
        
        return _mapper.Map<BaseEngagementModel>(engagement);
    }
    
    public async Task<IEnumerable<BaseEngagementModel>> GetByPartnerIdAsync(ClaimsPrincipal user, int partnerId)
    {
        var query = _engagementRepository.GetAll()
            .Where(e => !e.IsDeleted && e.EngagementPartners.Any(ep => ep.PartnerId == partnerId))
            .Include(x => x.EngagementPartners)
            .ThenInclude(ep => ep.PartnerEntity);

        // Apply permission filtering
        var filteredResult = await _permissionService.ApplyAccessControlFiltersAsync(query, user, "read", _entityName);
        
        // Handle both cases: IQueryable<BaseEngagement> or List<BaseEngagement>
        List<BaseEngagement> engagements;
        if (filteredResult is IQueryable<BaseEngagement> queryable)
        {
            // If it's still a queryable, execute it
            engagements = await queryable.ToListAsync();
        }
        else if (filteredResult is List<BaseEngagement> list)
        {
            // If it's already materialized as a list, use it directly
            engagements = list;
        }
        else
        {
            // Try to cast it as IEnumerable<BaseEngagement> and convert to list
            engagements = ((IEnumerable<BaseEngagement>)filteredResult).ToList();
        }
        
        return _mapper.Map<IEnumerable<BaseEngagementModel>>(engagements);
    }
    
    public async Task<IEnumerable<BaseEngagementPartnerModel>> GetEngagementPartnersAsync(ClaimsPrincipal user, int engagementId)
    {
        var engagementPartners = await _engagementPartnersRepository.GetAll()
            .Where(ep => !ep.IsDeleted && ep.BaseEngagementId == engagementId)
            .Include(x => x.BaseEngagementEntity)
            .Include(x => x.PartnerEntity)
            .ToListAsync();
        
        // Note: You may want to apply partner-level permissions here as well
        return _mapper.Map<IEnumerable<BaseEngagementPartnerModel>>(engagementPartners);
    }

    /// <summary>
    /// Implementation of abstract method from BaseUNOPSManager
    /// Gets basic entity data for AI prompts and generic operations
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal? user = null)
    {
        if (user != null)
        {
            return await GetByIdAsync(user, entityId);
        }

        // Fallback for cases without user context - still check deletion status
        var engagement = await _engagementRepository.GetAll()
            .Where(e => e.Id == entityId && !e.IsDeleted)
            .Include(x => x.EngagementPartners)
            .ThenInclude(ep => ep.PartnerEntity)
            .FirstOrDefaultAsync();

        return engagement != null ? _mapper.Map<BaseEngagementModel>(engagement) : null;
    }
}
