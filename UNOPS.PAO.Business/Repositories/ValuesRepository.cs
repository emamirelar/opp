using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Interfaces;

namespace UNOPS.PAO.Business.Repositories;
public class ValuesRepository
{
    AppDbContext context;

    public ValuesRepository(AppDbContext context)
    {
        this.context = context;
    }

    public IEnumerable<Currency> GetCurrencies() => context.Currencies.Where(x => x.Status == EntityStatus.Active);
    public IEnumerable<SelectionMethodology> GetSelectionMethodologies() 
        => context.SelectionMethodologies.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<EligibleEntity> GetEligibleEntities() => context.EligibleEntities.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<SDG> GetSDGs() => context.SDGs.Where(x => x.Status == EntityStatus.Active);
    public IEnumerable<Country> GetCountries() => context.Countries.Where(x => x.Status == EntityStatus.Active);

    public IEnumerable<Partner> GetPartners()
        => context.Partners.Where(x => x.Status.Equals("Active"));
}