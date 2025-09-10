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
            // Enhanced Partner structure
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .RuleFor(p => p.PartnerShortDescription, f => f.Company.CompanySuffix())
            .RuleFor(p => p.PartnerLongDescription, f => f.Lorem.Paragraph())
            .RuleFor(p => p.PartnerCategoryId, f => f.Random.Number(1, 10))
            .RuleFor(p => p.LiaisonOfficeId, f => f.Random.Int(1, 3)) // Reference to seeded liaison offices
            .RuleFor(p => p.UNAndStateEntity, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.Status, f => f.PickRandom<Domain.Entities.EntityStatus>())
            .RuleFor(p => p.KeyGlobalPartner, f => f.Random.Bool(0.2f))
            .RuleFor(p => p.UNSecretariatPartner, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.DueDiligenceRequired, f => f.PickRandom<Domain.Enums.DueDiligenceRequired>())
            .RuleFor(p => p.DueDiligenceApproval, f => f.PickRandom<Domain.Enums.DueDiligenceApproval>())
            .RuleFor(p => p.DueDiligenceApprovalDate, (f, p) => p.DueDiligenceApproval == Domain.Enums.DueDiligenceApproval.Approved ? f.Date.Past(1) : null)
            .RuleFor(p => p.DueDiligenceExpiryDate, (f, p) => p.DueDiligenceApproval == Domain.Enums.DueDiligenceApproval.Approved ? f.Date.Future(1) : null)
            .RuleFor(p => p.PartnerApprovalStatus, f => f.PickRandom<Domain.Enums.PartnerApprovalStatus>())
            .RuleFor(p => p.PartnerLevyStatus, f => f.PickRandom<Domain.Enums.PartnerLevyStatus>())
            .RuleFor(p => p.ReasonForLevy, (f, p) => p.PartnerLevyStatus != Domain.Enums.PartnerLevyStatus.DoesNotApply ? f.Lorem.Sentence() : null)
            .RuleFor(p => p.LevyTreatment, (f, p) => p.PartnerLevyStatus == Domain.Enums.PartnerLevyStatus.PotentiallyApplied ? f.PickRandom(new[] { "Direct", "Indirect", "Exempt" }) : null)
            .RuleFor(p => p.PooledFund, f => f.Random.Bool(0.15f))
            .RuleFor(p => p.CanCreateNewOpportunities, f => f.Random.Bool(0.8f))
            .RuleFor(p => p.ReasonForNoNewOpportunity, (f, p) => !p.CanCreateNewOpportunities ? f.Lorem.Sentence() : null)
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