namespace UNOPS.PAO.Presentation.Controllers;

using Microsoft.AspNetCore.Authorization;
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
    
    public ConfigurationController(
        SystemConfigurationManager manager,
        ILogger<ConfigurationController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _configuration = manager.GetConfiguration();
    }

    [HttpGet(APIDictionary.Configuration)]
    public ActionResult Get()
    {
        return HandleOperationAsync(async () => 
        {
            var googleSettings = _configuration.GetSection("GoogleAuthSettings");
            return await Task.FromResult(new ConfigurationResponse()
            {
                GoogleClientId = googleSettings.GetSection("clientId").Value,
                GoogleApiKey = googleSettings.GetSection("apiKey").Value
            });
        }).Result;
    }
}
