namespace UNOPS.PAO.Presentation.Controllers;

using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Utilities.Helpers;

[Route("/")]
[ApiController]
public class ValuesController : ControllerBase
{
    private ValuesManager manager;

    public ValuesController(ValuesManager manager)
    {
        this.manager = manager;
    }

    [HttpGet(APIDictionary.Currency)]
    public ActionResult GetCurrencies()
    {
        return Ok(manager.GetCurrencies());
    }

    [HttpGet(APIDictionary.SelectionMethodology)]
    public ActionResult GetSelectionMethodologies()
    {
        return Ok(manager.GetSelectionMethodologies());
    }

    [HttpGet(APIDictionary.EligibleEntity)]
    public ActionResult GetEligibleEntities()
    {
        return Ok(manager.GetEligibleEntities());
    }

    [HttpGet(APIDictionary.SDG)]
    public ActionResult GetSDGs()
    {
        return Ok(manager.GetSDGs());
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
}
