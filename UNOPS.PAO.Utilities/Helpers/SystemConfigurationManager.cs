namespace UNOPS.PAO.Utilities.Helpers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class SystemConfigurationManager
{
    private readonly IWebHostEnvironment environment;

    public SystemConfigurationManager(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    public SystemConfigurationManager()
    {
    }

    public IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment?.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .Build();
    }
}