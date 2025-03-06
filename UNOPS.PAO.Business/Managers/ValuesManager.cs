namespace UNOPS.PAO.Business.Managers;

using AutoMapper;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
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

    public IEnumerable<SelectonMethodologyModel> GetSelectionMethodologies()
         => repository.GetSelectionMethodologies().Select(mapper.Map<SelectonMethodologyModel>);

    public IEnumerable<EligibleEntityModel> GetEligibleEntities() => repository.GetEligibleEntities().Select(mapper.Map<EligibleEntityModel>);
    public IEnumerable<SDGModel> GetSDGs() => repository.GetSDGs().Select(mapper.Map<SDGModel>);
    public IEnumerable<CountryModel> GetCountries() => repository.GetCountries().Select(mapper.Map<CountryModel>);

    public IEnumerable<PartnerModel> GetPartners()
         => repository.GetPartners().Select(mapper.Map<PartnerModel>);

}