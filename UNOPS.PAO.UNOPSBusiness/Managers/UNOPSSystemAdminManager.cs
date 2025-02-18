namespace UNOPS.PAO.UNOPSBusiness.Managers;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Interfaces;


public class UNOPSSystemAdminManager : ISystemAdminManager
{
    private readonly UNOPSAppDbContext unopsAppDbContext;

    public UNOPSSystemAdminManager(UNOPSAppDbContext unopsAppDbContext)
    {
        this.unopsAppDbContext = unopsAppDbContext;
    }

    public async Task RunMigrations()
    {
        await unopsAppDbContext.Database.MigrateAsync();
    }
}