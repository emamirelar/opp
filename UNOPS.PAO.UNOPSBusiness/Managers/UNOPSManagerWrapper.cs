using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;

public class UNOPSManagerWrapper : ManagerWrapper
{
    private readonly UNOPSSystemAdminManager systemAdminManager;
    private readonly UNOPSContactManager contactManager;
    private readonly UNOPSInteractionManager interactionManager;
    private readonly UNOPSPartnerTreeManager partnerTreeManager;
    private readonly UNOPSPartnerManager partnerManager;
    private readonly UNOPSGeminiManager geminiManager;
    private readonly LinkManager linkManager;

    public UNOPSManagerWrapper(IMapper mapper, AppDbContext context, UNOPSAppDbContext opsContext, IConfiguration configuration,
                               UserManager<PAOIdentityUser> userManager, IHttpContextAccessor httpContextAccessor, 
                               IBusinessSecurityService securityService = null) : base(mapper, context, userManager, httpContextAccessor)
    {
        // Create a MemoryCache instance for services that need it
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        // Create a PartnerTreeService instance
        var partnerTreeRepository = new DataRepository<PartnerTree>(opsContext);
        var partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);
        
        systemAdminManager = new UNOPSSystemAdminManager(opsContext);
        contactManager = new UNOPSContactManager(mapper, opsContext, configuration, securityService);
        interactionManager = new UNOPSInteractionManager(mapper, opsContext, configuration, securityService);
        partnerTreeManager = new UNOPSPartnerTreeManager(mapper, opsContext, partnerTreeService);
        partnerManager = new UNOPSPartnerManager(mapper, opsContext, configuration, partnerTreeService, securityService);
        geminiManager = new UNOPSGeminiManager(mapper, opsContext, configuration);
        linkManager = new LinkManager(mapper, opsContext);
    }

    public override ISystemAdminManager SystemAdminManager => systemAdminManager;
    public override IContactManager ContactManager => contactManager;
    public override IInteractionManager InteractionManager => interactionManager;
    public override IPartnerTreeManager PartnerTreeManager => partnerTreeManager;
    public override IPartnerManager PartnerManager => partnerManager;
    public override IGeminiManager GeminiManager => geminiManager;
    public override ILinkManager LinkManager => linkManager;
}