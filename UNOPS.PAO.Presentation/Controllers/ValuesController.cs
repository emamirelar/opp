namespace UNOPS.PAO.Presentation.Controllers;

using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class ValuesController : BaseController
{
    private readonly ValuesManager _manager;
    private int currentUserId => _userResolverService.GetCurrentUserId();

    public ValuesController(
        ValuesManager manager,
        ILogger<ValuesController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager;
    }

    [HttpGet(APIDictionary.Currency)]
    public async Task<ActionResult> GetCurrencies()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetCurrencies()));
    }

    [HttpGet(APIDictionary.EligibleEntity)]
    public async Task<ActionResult> GetEligibleEntities()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetEligibleEntities()));
    }

    [HttpGet(APIDictionary.Country)]
    public async Task<ActionResult> GetCountries()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetCountries()));
    }

    [HttpGet(APIDictionary.ApplicationType)]
    public async Task<ActionResult> GetApplicationTypes()
    {
        return await HandleOperationAsync(async () => 
        {
            var types = typeof(ApplicationType)
                .GetMembers()
                .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
                .Where(x => x.attr != null)
                .Select(x => new { Id = x.value.Name, DisplayName = x.attr?.Value});

            return await Task.FromResult(types);
        });
    }

    [HttpGet(APIDictionary.Partners)]
    public async Task<ActionResult> GetPartners()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetPartners()));
    }

    [HttpGet(APIDictionary.OrganizationUnits)]
    public async Task<ActionResult> GetOrganizationUnits()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetOrganizationUnits()));
    }

    [HttpGet(APIDictionary.PartnerCategories)]
    public async Task<ActionResult> GetPartnerCategories()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetPartnerCategories()));
    }

    [HttpGet(APIDictionary.Contacts)]
    public async Task<ActionResult> GetContacts()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetContacts()));
    }

    [HttpGet(APIDictionary.Users)]
    public async Task<ActionResult> GetUsers()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetUsers()));
    }
}
