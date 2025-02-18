using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSBusiness.Interfaces;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;

public class UNOPSManagerWrapper : ManagerWrapper
{
    private readonly UNOPSFundingOpportunityManager fundingOpportunityManager;
    private readonly DocumentManager documentManager;
    private readonly UNOPSSystemAdminManager systemAdminManager;
    private readonly UNOPSContactManager contactManager;

    public UNOPSManagerWrapper(IMapper mapper, AppDbContext context, UNOPSAppDbContext opsContext, IGoogleDriveDocumentManager driveManager, IConfiguration configuration) : base(mapper, context)
    {
        fundingOpportunityManager = new UNOPSFundingOpportunityManager(mapper, opsContext);
        documentManager = new DocumentManager(driveManager, configuration, mapper, opsContext);
        systemAdminManager = new UNOPSSystemAdminManager(opsContext);
        contactManager = new UNOPSContactManager(mapper, opsContext);
    }

    public override IFundingOpportunityManager FundingOpportunityManager => fundingOpportunityManager;
    public override IDocumentManager DocumentManager => documentManager;
    public override ISystemAdminManager SystemAdminManager => systemAdminManager;
    public override IContactManager ContactManager => contactManager;
}
