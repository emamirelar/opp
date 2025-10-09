namespace UNOPS.PAO.Business.Interfaces;

using System.Threading.Tasks;


public interface ISystemAdminManager
{
    public Task RunMigrations();
    public Task RunSeeding();
    public Task RunSpecificSeeder(string seederName);
    public Task TruncateSeedScripts();
    public Task DeleteSeedScript(string scriptName);
}