namespace UNOPS.PAO.Business.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Interfaces;

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

    public IEnumerable<PartnerValueModel> GetPartners()
         => repository.GetPartners().Select(mapper.Map<PartnerValueModel>);

    public IEnumerable<OrganizationHierarchyModel> GetOrganizationUnits()
        => repository.GetOrganizationsByType(OrganizationUnitType.OrgUnit).Select(mapper.Map<OrganizationHierarchyModel>);

    public IEnumerable<ContactValueModel> GetContacts()
         => repository.GetContacts().Select(mapper.Map<ContactValueModel>);

    public IEnumerable<UserValueModel> GetUsers()
         => repository.GetUsers().Select(mapper.Map<UserValueModel>);
}