namespace UNOPS.PAO.Presentation.Controllers;

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.DataAccess.Services;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class ValuesController : ControllerBase
{
    private ValuesManager manager;
    private UserResolverService<int> userResolverService;
    private IAuthorizationService authorizationService;
    private int currentUserId => userResolverService.GetCurrentUserId();

    public ValuesController(ValuesManager manager, UserResolverService<int> userResolverService, IAuthorizationService authorizationService)
    {
        this.manager = manager;
        this.userResolverService = userResolverService;
        this.authorizationService = authorizationService;
    }

    [HttpGet(APIDictionary.Currency)]
    public ActionResult GetCurrencies()
    {
        return Ok(manager.GetCurrencies());
    }

    [HttpGet(APIDictionary.EligibleEntity)]
    public ActionResult GetEligibleEntities()
    {
        return Ok(manager.GetEligibleEntities());
    }

    [HttpGet(APIDictionary.Country)]
    public ActionResult GetCountries()
    {
        return Ok(manager.GetCountries());
    }

    [HttpGet(APIDictionary.ApplicationType)]
    public ActionResult GetApplicationTypes()
    {
        var types = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new { Id = x.value.Name, DisplayName = x.attr?.Value});

        return Ok(types);
    }

    [HttpGet(APIDictionary.Partners)]
    public ActionResult GetPartners()
    {
        return Ok(manager.GetPartners());
    }

    [HttpGet(APIDictionary.OrganizationUnits)]
    public ActionResult GetOrganizationUnits()
    {
        return Ok(manager.GetOrganizationUnits());
    }

    [HttpGet(APIDictionary.PartnerCategories)]
    public ActionResult GetPartnerCategories()
    {
        return Ok(manager.GetPartnerCategories());
    }

    [HttpGet(APIDictionary.Contacts)]
    public ActionResult GetContacts()
    {
        return Ok(manager.GetContacts());
    }

    [HttpGet(APIDictionary.Users)]
    public ActionResult GetUsers()
    {
        return Ok(manager.GetUsers());
    }
}
