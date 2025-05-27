namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
public class ConfigurationController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    
    public ConfigurationController(
        SystemConfigurationManager manager,
        ILogger<ConfigurationController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService,
        IWebHostEnvironment environment)
        : base(logger, authorizationService, userResolverService)
    {
        _configuration = manager.GetConfiguration();
        _environment = environment;
    }

    [HttpGet(APIDictionary.Configuration)]
    public ActionResult Get()
    {
        return HandleOperationAsync(async () => 
        {
            var googleSettings = _configuration.GetSection("GoogleAuthSettings");
            var appConfig = _configuration.GetSection("AppConfig");
            return await Task.FromResult(new ConfigurationResponse()
            {
                GoogleClientId = googleSettings.GetSection("clientId").Value,
                GoogleApiKey = googleSettings.GetSection("apiKey").Value,
                Environment = appConfig.GetSection("Environment").Value ?? _environment.EnvironmentName
            });
        }).Result;
    }
}
