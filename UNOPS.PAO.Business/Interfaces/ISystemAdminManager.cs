namespace UNOPS.PAO.Business.Interfaces;

using System.Threading.Tasks;


public interface ISystemAdminManager
{
    public Task RunMigrations();
}