using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

public class UNOPSManagerWrapper : ManagerWrapper
{
    private readonly UNOPSFundingOpportunityManager fundingOpportunityManager;
    private readonly DocumentManager documentManager;
    private readonly UNOPSSystemAdminManager systemAdminManager;
    private readonly UNOPSContactManager contactManager;
    private readonly UNOPSInteractionManager interactionManager;
    private readonly UNOPSPartnerTreeManager partnerTreeManager;

    public UNOPSManagerWrapper(IMapper mapper, AppDbContext context, UNOPSAppDbContext opsContext, IGoogleDriveDocumentManager driveManager, IConfiguration configuration) : base(mapper, context)
    {
        fundingOpportunityManager = new UNOPSFundingOpportunityManager(mapper, opsContext);
        documentManager = new DocumentManager(driveManager, configuration, mapper, opsContext);
        systemAdminManager = new UNOPSSystemAdminManager(opsContext);
        contactManager = new UNOPSContactManager(mapper, opsContext);
        interactionManager = new UNOPSInteractionManager(mapper, opsContext);
        partnerTreeManager = new UNOPSPartnerTreeManager(mapper, opsContext);
    }

    public override IFundingOpportunityManager FundingOpportunityManager => fundingOpportunityManager;
    public override IDocumentManager DocumentManager => documentManager;
    public override ISystemAdminManager SystemAdminManager => systemAdminManager;
    public override IContactManager ContactManager => contactManager;
    public override IInteractionManager InteractionManager => interactionManager;
    public override IPartnerTreeManager PartnerTreeManager => partnerTreeManager;
}
