namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
[ApiController]
public class ConfigurationController : ControllerBase
{
    private readonly IConfiguration configuration;
    public ConfigurationController(SystemConfigurationManager manager)
    {
        configuration = manager.GetConfiguration();
    }

    [HttpGet(APIDictionary.Configuration)]
    public ConfigurationResponse Get()
    {
        var googleSettings = configuration.GetSection("GoogleAuthSettings");
        return new ConfigurationResponse()
        {
            GoogleClientId = googleSettings.GetSection("clientId").Value
            , GoogleApiKey = googleSettings.GetSection("apiKey").Value
        };
    }
}
