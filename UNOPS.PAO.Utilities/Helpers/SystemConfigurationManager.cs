namespace UNOPS.PAO.Utilities.Helpers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class SystemConfigurationManager
{
    private readonly IWebHostEnvironment? environment;

    public SystemConfigurationManager(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    public SystemConfigurationManager()
    {
        environment = null;
    }

    public IConfigurationRoot GetConfiguration()
    {
        // Get environment name first
        var environmentName = environment?.EnvironmentName;
        
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);

        // For "Development" environment, also load appsettings.Local.json (only on local development - appsettings.Local.json is not present in the repo)
        if (!string.IsNullOrEmpty(environmentName) && environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"==============[SystemConfigurationManager] Loading appsettings.Local.json for Development environment==============");
            configBuilder.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
        }

        return configBuilder.Build();
    }
}