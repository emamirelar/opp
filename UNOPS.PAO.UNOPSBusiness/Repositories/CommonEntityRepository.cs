namespace UNOPS.PAO.UNOPSBusiness.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;

public class CommonEntityRepository
{
    protected readonly UNOPSAppDbContext context;

    public CommonEntityRepository(UNOPSAppDbContext context)
    {
        this.context = context;
    }

    public IEnumerable<Country> GetCountries() => context.Countries.Where(x => x.Status == EntityStatus.Active);
    public IEnumerable<EligibleEntity> GetEligibleEntities() => context.EligibleEntities.Where(x => x.Status == EntityStatus.Active);

    public async Task<Currency?> GetCurrencyByCodeAsync(string currencyCode) =>
        await context.Currencies.SingleOrDefaultAsync(x => x.Code == currencyCode);

}