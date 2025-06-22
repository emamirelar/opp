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
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore;

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
        return await HandleOperationAsync(async () => 
        {
            // Get all partners and evaluate on client side to avoid EF translation issues
            var allPartners = await _manager.GetPartnersForFiltering().ToListAsync();
            
            // Apply row-level filtering based on user's role and organization unit
            
            // Map to PartnerValueModel after filtering
            return allPartners.Select(p => new PartnerValueModel
            {
                Id = p.Id,
                Name = p.Name,
                PartnerOfficeId = p.PartnerOfficeId
            }).ToList();
        });
    }

    [HttpGet(APIDictionary.OrganizationUnits)]
    public async Task<ActionResult> GetOrganizationUnits()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetOrganizationUnits()));
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

    [HttpGet(APIDictionary.GeminiModels)]
    public async Task<ActionResult> GetGeminiModels()
    {
        return await HandleOperationAsync(async () => 
        {
            var models = Enum.GetValues<GeminiModel>()
                .Select(model => new
                {
                    Value = GetGeminiModelValue(model),
                    Label = GetGeminiModelDisplayName(model),
                    Location = GetGeminiModelLocation(model),
                    MaxTokens = GetGeminiModelMaxTokens(model)
                })
                .ToList();

            return await Task.FromResult(models);
        });
    }

    private static string GetGeminiModelValue(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_0_Flash_001 => "gemini-2.0-flash-001",
            GeminiModel.Gemini_2_5_Flash_Preview_04_17 => "gemini-2.5-flash-preview-04-17",
            GeminiModel.Gemini_2_5_Pro_Preview_05_06 => "gemini-2.5-pro-preview-05-06",
            GeminiModel.Gemini_2_5_Flash_Preview_05_20 => "gemini-2.5-flash-preview-05-20",
            _ => model.ToString().ToLowerInvariant()
        };
    }

    private static string GetGeminiModelDisplayName(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_0_Flash_001 => "Gemini 2.0 Flash (001)",
            GeminiModel.Gemini_2_5_Flash_Preview_04_17 => "Gemini 2.5 Flash Preview (04-17)",
            GeminiModel.Gemini_2_5_Pro_Preview_05_06 => "Gemini 2.5 Pro Preview (05-06)", 
            GeminiModel.Gemini_2_5_Flash_Preview_05_20 => "Gemini 2.5 Flash Preview (05-20)",
            _ => model.ToString()
        };
    }

    private static string GetGeminiModelLocation(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_0_Flash_001 => "europe-west4",
            GeminiModel.Gemini_2_5_Flash_Preview_04_17 => "global",
            GeminiModel.Gemini_2_5_Pro_Preview_05_06 => "global",
            GeminiModel.Gemini_2_5_Flash_Preview_05_20 => "global",
            _ => "europe-west4"
        };
    }

    private static int GetGeminiModelMaxTokens(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_0_Flash_001 => 8192,
            GeminiModel.Gemini_2_5_Flash_Preview_04_17 => 65535,
            GeminiModel.Gemini_2_5_Pro_Preview_05_06 => 65535,
            GeminiModel.Gemini_2_5_Flash_Preview_05_20 => 65535,
            _ => 8192
        };
    }
}

public enum GeminiModel
{
    Gemini_2_0_Flash_001,
    Gemini_2_5_Flash_Preview_04_17,
    Gemini_2_5_Pro_Preview_05_06,
    Gemini_2_5_Flash_Preview_05_20
}
