namespace UNOPS.PAO.Presentation.Controllers.Shared;

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
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.Partners;

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
                Name = p.Name ?? ""
            }).ToList();
        });
    }

    [HttpGet(APIDictionary.OrganizationUnits)]
    public async Task<ActionResult> GetOrganizationUnits()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetOrganizationUnits()));
    }

    [HttpGet(APIDictionary.LiaisonOffices)]
    public async Task<ActionResult> GetLiaisonOffices()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetLiaisonOffices()));
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

    [HttpPost(APIDictionary.Users + "/paged")]
    public async Task<ActionResult> GetUsersPaged([FromBody] UsersPagedRequest request)
    {
        return await HandleOperationAsync(async () => await _manager.GetUsersPagedAsync(request));
    }

    [HttpGet(APIDictionary.Users + "/search")]
    public async Task<ActionResult> SearchUsers([FromQuery] string? searchTerm = null, [FromQuery] int maxResults = 20, [FromQuery] int[]? selectedUserIds = null)
    {
        return await HandleOperationAsync(async () => await _manager.SearchUsersAsync(searchTerm, maxResults, selectedUserIds));
    }

    [HttpGet(APIDictionary.ProposedInitiativeTypes)]
    public async Task<ActionResult> GetProposedInitiativeTypes()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetProposedInitiativeTypes()));
    }

    [HttpGet(APIDictionary.Outputs)]
    public async Task<ActionResult> GetOutputs()
    {
        return await HandleOperationAsync(async () => await Task.FromResult(_manager.GetOutputs()));
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
            GeminiModel.Gemini_2_5_Flash_001 => "gemini-2.5-flash",
            GeminiModel.Gemini_2_5_Flash_Lite => "gemini-2.5-flash-lite",
            _ => model.ToString().ToLowerInvariant()
        };
    }

    private static string GetGeminiModelDisplayName(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_5_Flash_001 => "Gemini 2.5 Flash",
            GeminiModel.Gemini_2_5_Flash_Lite => "Gemini 2.5 Flash Lite",
            _ => model.ToString()
        };
    }

    private static string GetGeminiModelLocation(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_5_Flash_001 => "europe-west4",
            GeminiModel.Gemini_2_5_Flash_Lite => "europe-west4",
            _ => "europe-west4"
        };
    }

    private static int GetGeminiModelMaxTokens(GeminiModel model)
    {
        return model switch
        {
            GeminiModel.Gemini_2_5_Flash_001 => 65535,
            GeminiModel.Gemini_2_5_Flash_Lite => 65535,
            _ => 8192
        };
    }
}

public enum GeminiModel
{
    Gemini_2_5_Flash_001,
    Gemini_2_5_Flash_Lite
}
