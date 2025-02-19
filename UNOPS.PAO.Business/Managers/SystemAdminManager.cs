namespace UNOPS.PAO.Business.Managers;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;


public class SystemAdminManager : ISystemAdminManager
{
    private readonly AppDbContext appDbContext;

    public SystemAdminManager(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task RunMigrations()
    {
        await appDbContext.Database.MigrateAsync();
    }
}