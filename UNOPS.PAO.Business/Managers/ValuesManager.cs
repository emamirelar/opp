namespace UNOPS.PAO.Business.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Utilities.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models.Users;
using UNOPS.PAO.Models.LiaisonOffices;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Locations;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Values;
using UNOPS.PAO.Models.SDG;
using UNOPS.PAO.Models.UNCF;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.OrganizationUnits;

public class ValuesManager : IApplicationService
{
    private IMapper mapper;

    ValuesRepository repository;

    public ValuesManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        repository = new ValuesRepository(context);
    }

    public IEnumerable<CurrencyModel> GetCurrencies() => repository.GetCurrencies().Select(mapper.Map<CurrencyModel>);

    public IEnumerable<EligibleEntityModel> GetEligibleEntities() => repository.GetEligibleEntities().Select(mapper.Map<EligibleEntityModel>);
    public IEnumerable<SimpleValueModel> GetCountries() => repository.GetCountries();

    public IQueryable<PartnerValueModel> GetPartners()
         => (IQueryable<PartnerValueModel>)repository.GetPartners().Select(mapper.Map<PartnerValueModel>);

    public IQueryable<Partner> GetPartnersForFiltering()
         => repository.GetPartners();

    public IEnumerable<OrganizationHierarchyModel> GetOrganizationUnits()
        => repository.GetOrganizationsByType(OrganizationUnitType.OrgUnit).Select(mapper.Map<OrganizationHierarchyModel>);

    /// <summary>
    /// Gets organization units for Opportunity dropdown (includes OrgUnit, Hub, and Region types)
    /// </summary>
    public IEnumerable<OrganizationHierarchyModel> GetOpportunityOrganizationUnits()
        => repository.GetOrganizationsByTypes(OrganizationUnitType.OrgUnit, OrganizationUnitType.Hub, OrganizationUnitType.Region)
            .Select(mapper.Map<OrganizationHierarchyModel>);

    public IEnumerable<ContactValueModel> GetContacts()
         => repository.GetContacts().Select(mapper.Map<ContactValueModel>);

    public IEnumerable<UserValueModel> GetUsers()
         => repository.GetUsers().Select(mapper.Map<UserValueModel>);

    // Optimized paginated user retrieval
    public async Task<PaginationResponse<UserValueModel>> GetUsersPagedAsync(UsersPagedRequest request)
    {
        var (users, totalCount) = await repository.GetUsersPagedAsync(
            request.PageIndex,
            request.PageSize,
            request.SearchTerm,
            request.ActiveOnly,
            request.SelectedUserIds
        );

        var userModels = users.Select(mapper.Map<UserValueModel>).ToList();

        return new PaginationResponse<UserValueModel>
        {
            Records = userModels,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    // Quick user search for autocomplete
    public async Task<IEnumerable<UserValueModel>> SearchUsersAsync(string? searchTerm, int maxResults = 20, int[]? selectedUserIds = null)
    {
        var users = await repository.SearchUsersAsync(searchTerm, maxResults, selectedUserIds);
        return users.Select(mapper.Map<UserValueModel>);
    }

    public IEnumerable<LiaisonOfficeModel> GetLiaisonOffices()
         => repository.GetLiaisonOffices().Select(mapper.Map<LiaisonOfficeModel>);

    public IEnumerable<SimpleValueModel> GetProposedInitiativeTypes()
         => repository.GetProposedInitiativeTypes().Select(mapper.Map<SimpleValueModel>);

    public IEnumerable<OutputModel> GetOutputs()
         => repository.GetOutputs().Select(mapper.Map<OutputModel>);

    public IEnumerable<SDGModel> GetSDGs()
         => repository.GetSDGs().Select(mapper.Map<SDGModel>);

    public IEnumerable<SDGTargetModel> GetSDGTargets()
         => repository.GetSDGTargets().Select(mapper.Map<SDGTargetModel>);

    public IEnumerable<SDGTargetModel> GetSDGTargetsBySDGId(string sdgId)
         => repository.GetSDGTargetsBySDGId(sdgId).Select(mapper.Map<SDGTargetModel>);

    public IEnumerable<SDGIndicatorModel> GetSDGIndicators()
         => repository.GetSDGIndicators().Select(mapper.Map<SDGIndicatorModel>);

    public IEnumerable<SDGIndicatorModel> GetSDGIndicatorsByTargetId(string targetId)
         => repository.GetSDGIndicatorsByTargetId(targetId).Select(mapper.Map<SDGIndicatorModel>);

    public IEnumerable<UNCFOutcomeModel> GetUNCFOutcomes()
         => repository.GetUNCFOutcomes().Select(mapper.Map<UNCFOutcomeModel>);

    public IEnumerable<UNCFOutcomeModel> GetUNCFOutcomesByCountry(string countryCode)
         => repository.GetUNCFOutcomesByCountry(countryCode).Select(mapper.Map<UNCFOutcomeModel>);

    public IEnumerable<UNCFIndicatorModel> GetUNCFIndicators()
         => repository.GetUNCFIndicators().Select(mapper.Map<UNCFIndicatorModel>);

    public IEnumerable<UNCFIndicatorModel> GetUNCFIndicatorsByOutcomeId(int outcomeId)
         => repository.GetUNCFIndicatorsByOutcomeId(outcomeId).Select(mapper.Map<UNCFIndicatorModel>);

    public IEnumerable<UNOPSMissionModel> GetUNOPSMissions()
         => repository.GetUNOPSMissions().Select(mapper.Map<UNOPSMissionModel>);

    public async Task<IEnumerable<SimpleValueModel>> GetEntityRolesAsync(string entityType)
         => await repository.GetEntityRolesAsync(entityType);

    public async Task<IEnumerable<SimpleValueModel>> GetInternalUsersAsync()
         => await repository.GetInternalUsersAsync();

    public async Task<SuggestedOrgUnitsResponse> GetSuggestedOrgUnitsForCountriesAsync(int[] countryIds)
         => await repository.GetSuggestedOrgUnitsForCountriesAsync(countryIds);
}