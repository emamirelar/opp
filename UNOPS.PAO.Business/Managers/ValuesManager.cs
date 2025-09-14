namespace UNOPS.PAO.Business.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

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
    public IEnumerable<CountryModel> GetCountries() => repository.GetCountries().Select(mapper.Map<CountryModel>);

    public IQueryable<PartnerValueModel> GetPartners()
         => (IQueryable<PartnerValueModel>)repository.GetPartners().Select(mapper.Map<PartnerValueModel>);

    public IQueryable<Partner> GetPartnersForFiltering()
         => repository.GetPartners();

    public IEnumerable<OrganizationHierarchyModel> GetOrganizationUnits()
        => repository.GetOrganizationsByType(OrganizationUnitType.OrgUnit).Select(mapper.Map<OrganizationHierarchyModel>);

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
}