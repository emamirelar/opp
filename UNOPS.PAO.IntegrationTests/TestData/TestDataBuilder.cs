using Bogus;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.IntegrationTests.TestData;

public static class TestDataBuilder
{
    public static Faker<UNOPSPartner> GetPartnerFaker()
    {
        return new Faker<UNOPSPartner>()
            .RuleFor(p => p.PartnerCode, f => $"P{f.Random.Number(1000, 9999)}")
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .RuleFor(p => p.ShortName, f => f.Company.CatchPhrase())
            .RuleFor(p => p.Status, f => f.PickRandom(new[] { "Active", "Inactive", "Prospect" }))
            .RuleFor(p => p.NewEngagement, f => f.Random.Bool().ToString())
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(p => p.Website, f => f.Internet.Url())
            .RuleFor(p => p.Address1Street, f => f.Address.StreetAddress())
            .RuleFor(p => p.Address1Street2, f => f.Random.Bool(0.3f) ? f.Address.SecondaryAddress() : null)
            .RuleFor(p => p.Address1City, f => f.Address.City())
            .RuleFor(p => p.Address1StateProvince, f => f.Address.State())
            .RuleFor(p => p.Address1PostalCode, f => f.Address.ZipCode())
            .RuleFor(p => p.Address1Country, f => f.Address.Country())
            .RuleFor(p => p.GlobalKeyAccount, f => f.Random.Bool(0.2f))
            .RuleFor(p => p.UNSecretariatEntity, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.DDRequired, f => f.Random.Bool(0.3f).ToString())
            .RuleFor(p => p.DDEACDone, (f, p) => (p.DDRequired == "True" && f.Random.Bool(0.7f)).ToString())
            .RuleFor(p => p.EACReference, (f, p) => p.DDEACDone == "True" ? $"EAC-{f.Random.Number(1000, 9999)}" : null)
            .RuleFor(p => p.LevyPotentiallyApplies, f => f.Random.Bool(0.4f).ToString())
            .RuleFor(p => p.ReasonForLevyNotApplying, (f, p) => p.LevyPotentiallyApplies == "False" ? f.Lorem.Sentence() : null)
            .RuleFor(p => p.LevyTreatment, (f, p) => p.LevyPotentiallyApplies == "True" ? f.PickRandom(new[] { "Direct", "Indirect", "Exempt" }) : null)
            .RuleFor(p => p.PooledFund, f => f.Random.Bool(0.15f).ToString())
            .RuleFor(p => p.PartnerGroupCode, f => f.PickRandom(new[] { "NGO", "GOV", "PRI", "UN" }))
            .RuleFor(p => p.CreatedDate, f => f.Date.Past(2))
            .RuleFor(p => p.LastModifiedDate, f => f.Date.Recent());
    }

    public static PartnerFilterRequest CreatePartnerFilterRequest(
        int pageIndex = 1,
        int pageSize = 10,
        string? searchText = null,
        string? status = null,
        string? orderBy = null,
        bool ascending = true)
    {
        return new PartnerFilterRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchText = searchText,
            Status = status,
            OrderBy = orderBy ?? "Name",
            Ascending = ascending
        };
    }
}