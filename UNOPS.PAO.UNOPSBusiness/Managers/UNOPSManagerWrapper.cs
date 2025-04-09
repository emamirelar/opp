using Microsoft.Extensions.Configuration;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

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
                               UserManager<PAOIdentityUser> userManager) : base(mapper, context, userManager)
    {
        systemAdminManager = new UNOPSSystemAdminManager(opsContext);
        contactManager = new UNOPSContactManager(mapper, opsContext, configuration);
        interactionManager = new UNOPSInteractionManager(mapper, opsContext, configuration);
        partnerTreeManager = new UNOPSPartnerTreeManager(mapper, opsContext, configuration);
        partnerManager = new UNOPSPartnerManager(mapper,opsContext, configuration);
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