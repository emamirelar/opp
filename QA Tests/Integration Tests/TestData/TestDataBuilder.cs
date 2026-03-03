using Bogus;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Authorization;
using DomainLink = UNOPS.PAO.Domain.Entities.Link;

namespace UNOPS.PAO.IntegrationTests.TestData;

/// <summary>
/// Comprehensive Bogus-based test data builder for all core domain entities.
/// Provides configurable Faker instances that generate realistic, valid test data.
/// </summary>
public static class TestDataBuilder
{
    private static int _idSequence = 1000;
    private static int NextId() => Interlocked.Increment(ref _idSequence);

    // ═══════════════════════════════════════════════════════════════════════
    //  CORE ENTITIES: Partner, Contact, Interaction
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<UNOPSPartner> GetPartnerFaker()
    {
        return new Faker<UNOPSPartner>()
            .RuleFor(p => p.Name, f => f.Company.CompanyName())
            .RuleFor(p => p.PartnerShortDescription, f => f.Company.CompanySuffix())
            .RuleFor(p => p.PartnerLongDescription, f => f.Lorem.Paragraph())
            .RuleFor(p => p.PartnerCategoryId, f => f.Random.Number(1, 10))
            .RuleFor(p => p.LiaisonOfficeId, f => f.Random.Int(1, 3))
            .RuleFor(p => p.UNAndStateEntity, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(p => p.KeyGlobalPartner, f => f.Random.Bool(0.2f))
            .RuleFor(p => p.UNSecretariatPartner, f => f.Random.Bool(0.1f))
            .RuleFor(p => p.DueDiligenceRequired, f => f.PickRandom<DueDiligenceRequired>())
            .RuleFor(p => p.DueDiligenceApproval, f => f.PickRandom<DueDiligenceApproval>())
            .RuleFor(p => p.DueDiligenceApprovalDate, (f, p) => p.DueDiligenceApproval == DueDiligenceApproval.Approved ? f.Date.Past(1) : null)
            .RuleFor(p => p.DueDiligenceExpiryDate, (f, p) => p.DueDiligenceApproval == DueDiligenceApproval.Approved ? f.Date.Future(1) : null)
            .RuleFor(p => p.PartnerApprovalStatus, f => f.PickRandom<PartnerApprovalStatus>())
            .RuleFor(p => p.PartnerLevyStatus, f => f.PickRandom<PartnerLevyStatus>())
            .RuleFor(p => p.ReasonForLevy, (f, p) => p.PartnerLevyStatus != PartnerLevyStatus.DoesNotApply ? f.Lorem.Sentence() : null)
            .RuleFor(p => p.LevyTreatment, (f, p) => p.PartnerLevyStatus == PartnerLevyStatus.PotentiallyApplied ? f.PickRandom(new[] { "Direct", "Indirect", "Exempt" }) : null)
            .RuleFor(p => p.PooledFund, f => f.Random.Bool(0.15f))
            .RuleFor(p => p.CanCreateNewOpportunities, f => f.Random.Bool(0.8f))
            .RuleFor(p => p.ReasonForNoNewOpportunity, (f, p) => !p.CanCreateNewOpportunities ? f.Lorem.Sentence() : null)
            .RuleFor(p => p.PartnerGroupId, f => f.Random.Int(1, 10))
            .RuleFor(p => p.IsDeleted, false)
            .RuleFor(p => p.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(p => p.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(p => p.CreatedDate, f => f.Date.Past(2))
            .RuleFor(p => p.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<Contact> GetContactFaker()
    {
        return new Faker<Contact>()
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Title, f => f.Name.JobTitle())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Salutation, f => f.PickRandom(new[] { "Mr.", "Ms.", "Mrs.", "Dr.", "Prof." }))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Mobile, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Department, f => f.Commerce.Department())
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.MailingStreet, f => f.Address.StreetAddress())
            .RuleFor(c => c.MailingCity, f => f.Address.City())
            .RuleFor(c => c.MailingStateProvince, f => f.Address.State())
            .RuleFor(c => c.MailingPostalCode, f => f.Address.ZipCode())
            .RuleFor(c => c.MailingCountry, f => f.Address.Country())
            .RuleFor(c => c.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(c => c.PartnerId, f => f.Random.Int(1, 100))
            .RuleFor(c => c.IsDeleted, false)
            .RuleFor(c => c.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.CreatedDate, f => f.Date.Past(2))
            .RuleFor(c => c.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<UNOPSContact> GetUNOPSContactFaker()
    {
        return new Faker<UNOPSContact>()
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Title, f => f.Name.JobTitle())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Salutation, f => f.PickRandom(new[] { "Mr.", "Ms.", "Mrs.", "Dr.", "Prof." }))
            .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Mobile, f => f.Phone.PhoneNumber())
            .RuleFor(c => c.Department, f => f.Commerce.Department())
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.MailingStreet, f => f.Address.StreetAddress())
            .RuleFor(c => c.MailingCity, f => f.Address.City())
            .RuleFor(c => c.MailingStateProvince, f => f.Address.State())
            .RuleFor(c => c.MailingPostalCode, f => f.Address.ZipCode())
            .RuleFor(c => c.MailingCountry, f => f.Address.Country())
            .RuleFor(c => c.ContactNumber, f => $"CN-{f.Random.AlphaNumeric(8).ToUpper()}")
            .RuleFor(c => c.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(c => c.PartnerId, f => f.Random.Int(1, 100))
            .RuleFor(c => c.IsDeleted, false)
            .RuleFor(c => c.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.CreatedDate, f => f.Date.Past(2))
            .RuleFor(c => c.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<Interaction> GetInteractionFaker()
    {
        return new Faker<Interaction>()
            .RuleFor(i => i.Name, f => f.Lorem.Sentence(4))
            .RuleFor(i => i.Subject, f => f.Lorem.Sentence(6))
            .RuleFor(i => i.Description, f => f.Lorem.Paragraph())
            .RuleFor(i => i.Type, f => f.PickRandom<InteractionType>())
            .RuleFor(i => i.Date, f => f.Date.Recent(30))
            .RuleFor(i => i.Location, f => f.Address.City())
            .RuleFor(i => i.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(i => i.IsDeleted, false)
            .RuleFor(i => i.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(i => i.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(i => i.CreatedDate, f => f.Date.Past(1))
            .RuleFor(i => i.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<UNOPSInteraction> GetUNOPSInteractionFaker()
    {
        return new Faker<UNOPSInteraction>()
            .RuleFor(i => i.Name, f => f.Lorem.Sentence(4))
            .RuleFor(i => i.Subject, f => f.Lorem.Sentence(6))
            .RuleFor(i => i.Description, f => f.Lorem.Paragraph())
            .RuleFor(i => i.Type, f => f.PickRandom<InteractionType>())
            .RuleFor(i => i.Date, f => f.Date.Recent(30))
            .RuleFor(i => i.Location, f => f.Address.City())
            .RuleFor(i => i.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(i => i.IsDeleted, false)
            .RuleFor(i => i.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(i => i.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(i => i.CreatedDate, f => f.Date.Past(1))
            .RuleFor(i => i.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  OPPORTUNITY & SUB-ENTITIES
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<Opportunity> GetOpportunityFaker()
    {
        return new Faker<Opportunity>()
            .RuleFor(o => o.Name, f => f.Commerce.ProductName() + " " + f.Company.CompanySuffix())
            .RuleFor(o => o.Description, f => f.Lorem.Paragraphs(2))
            .RuleFor(o => o.PartnerReference, f => f.Random.Bool(0.5f) ? $"REF-{f.Random.AlphaNumeric(8).ToUpper()}" : null)
            .RuleFor(o => o.Stage, f => f.PickRandom(new[] { "IDENTIFY & PROFILE", "ENGAGE", "FORMALIZE", "DELIVER & CLOSE" }))
            .RuleFor(o => o.ResponsibleOrgUnitId, f => f.Random.Int(1, 5))
            .RuleFor(o => o.InitiativeBudgetUSD, f => f.Finance.Amount(10000, 5000000))
            .RuleFor(o => o.TargetSigningDate, f => f.Date.Future(1))
            .RuleFor(o => o.ImplementationStartDate, f => f.Date.Future(1))
            .RuleFor(o => o.TargetDeliveryDate, f => f.Date.Future(3))
            .RuleFor(o => o.IsTargetSigningDateFirm, f => f.Random.Bool())
            .RuleFor(o => o.SigningDateNotes, f => f.Random.Bool(0.3f) ? f.Lorem.Sentence() : null)
            .RuleFor(o => o.SubmissionDeadline, f => f.Random.Bool(0.5f) ? f.Date.Soon(180) : null)
            .RuleFor(o => o.ProposedInitiativeTypeId, f => f.Random.Int(1, 5))
            .RuleFor(o => o.ResultsFocus, f => f.Lorem.Sentences(2))
            .RuleFor(o => o.ExpectedImpact, f => f.Lorem.Sentence())
            .RuleFor(o => o.ExpectedOutcomes, f => f.Lorem.Sentence())
            .RuleFor(o => o.ExpectedBeneficiaries, f => f.Lorem.Sentence())
            .RuleFor(o => o.EstimatedDirectBeneficiaries, f => f.Random.Int(100, 100000))
            .RuleFor(o => o.EstimatedIndirectBeneficiaries, f => f.Random.Int(500, 500000))
            .RuleFor(o => o.BeneficiariesToBeDetermined, false)
            .RuleFor(o => o.Challenges, f => f.Lorem.Sentences(2))
            .RuleFor(o => o.IsPooledFunding, f => f.Random.Bool(0.2f))
            .RuleFor(o => o.HighRisksAcknowledged, f => f.Random.Bool(0.3f))
            .RuleFor(o => o.DeliveryModality, f => f.PickRandom<DeliveryModality>())
            .RuleFor(o => o.UNOPSMissionsNotApplicable, f => f.Random.Bool(0.1f))
            .RuleFor(o => o.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(o => o.IsDeleted, false)
            .RuleFor(o => o.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.CreatedDate, f => f.Date.Past(1))
            .RuleFor(o => o.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityFundingPartner> GetOpportunityFundingPartnerFaker()
    {
        return new Faker<OpportunityFundingPartner>()
            .RuleFor(fp => fp.Name, f => f.Company.CompanyName() + " Funding")
            .RuleFor(fp => fp.PartnerId, f => f.Random.Int(1, 50))
            .RuleFor(fp => fp.Amount, f => f.Finance.Amount(10000, 2000000))
            .RuleFor(fp => fp.AmountUSD, (f, fp) => fp.Amount)
            .RuleFor(fp => fp.CurrencyId, 1)
            .RuleFor(fp => fp.Percentage, f => f.Random.Decimal(5, 100))
            .RuleFor(fp => fp.FeePercentage, f => f.Random.Decimal(1, 15))
            .RuleFor(fp => fp.FeeAmount, f => f.Finance.Amount(500, 50000))
            .RuleFor(fp => fp.FeeAmountUSD, (f, fp) => fp.FeeAmount)
            .RuleFor(fp => fp.IsAmountBasedFee, f => f.Random.Bool())
            .RuleFor(fp => fp.PartnershipAgreementReference, f => f.Random.Bool(0.5f) ? $"AGR-{f.Random.AlphaNumeric(6).ToUpper()}" : null)
            .RuleFor(fp => fp.CommitmentStatus, f => f.PickRandom(new[] { "Committed", "Tentative", "Pledged", "Confirmed" }))
            .RuleFor(fp => fp.IsPooledContribution, f => f.Random.Bool(0.2f))
            .RuleFor(fp => fp.Status, EntityStatus.Active)
            .RuleFor(fp => fp.IsDeleted, false)
            .RuleFor(fp => fp.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(fp => fp.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(fp => fp.CreatedDate, f => f.Date.Past(1))
            .RuleFor(fp => fp.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityClientPartner> GetOpportunityClientPartnerFaker()
    {
        return new Faker<OpportunityClientPartner>()
            .RuleFor(cp => cp.Name, f => f.Company.CompanyName() + " Client")
            .RuleFor(cp => cp.PartnerId, f => f.Random.Int(1, 50))
            .RuleFor(cp => cp.SelectedPartnerAgreementNumber, f => f.Random.Bool(0.5f) ? $"PA-{f.Random.Number(1000, 9999)}" : null)
            .RuleFor(cp => cp.Status, EntityStatus.Active)
            .RuleFor(cp => cp.IsDeleted, false)
            .RuleFor(cp => cp.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(cp => cp.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(cp => cp.CreatedDate, f => f.Date.Past(1))
            .RuleFor(cp => cp.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityStakeholder> GetOpportunityStakeholderFaker()
    {
        return new Faker<OpportunityStakeholder>()
            .RuleFor(s => s.Name, f => f.Name.FullName())
            .RuleFor(s => s.EntityRoleId, f => f.Random.Int(1, 10))
            .RuleFor(s => s.IsInternal, f => f.Random.Bool(0.7f))
            .RuleFor(s => s.StakeholderType, f => f.PickRandom(new[] { "Individual", "Organization", "Government", "NGO" }))
            .RuleFor(s => s.UserId, (f, s) => s.IsInternal ? f.Random.Int(1, 20) : null)
            .RuleFor(s => s.OrganizationHierarchyId, (f, s) => s.IsInternal ? f.Random.Int(1, 5) : null)
            .RuleFor(s => s.Notes, f => f.Random.Bool(0.4f) ? f.Lorem.Sentence() : null)
            .RuleFor(s => s.Status, EntityStatus.Active)
            .RuleFor(s => s.IsDeleted, false)
            .RuleFor(s => s.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(s => s.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(s => s.CreatedDate, f => f.Date.Past(1))
            .RuleFor(s => s.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityExternalStakeholder> GetOpportunityExternalStakeholderFaker()
    {
        return new Faker<OpportunityExternalStakeholder>()
            .RuleFor(es => es.Name, f => f.Name.FullName())
            .RuleFor(es => es.ContactId, f => f.Random.Int(1, 50))
            .RuleFor(es => es.Status, EntityStatus.Active)
            .RuleFor(es => es.IsDeleted, false)
            .RuleFor(es => es.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(es => es.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(es => es.CreatedDate, f => f.Date.Past(1))
            .RuleFor(es => es.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityDeliverable> GetOpportunityDeliverableFaker()
    {
        return new Faker<OpportunityDeliverable>()
            .RuleFor(d => d.Name, f => f.Commerce.ProductName())
            .RuleFor(d => d.OutputId, f => f.Random.Int(1, 20))
            .RuleFor(d => d.Quantity, f => f.Random.Decimal(1, 100))
            .RuleFor(d => d.Notes, f => f.Random.Bool(0.5f) ? f.Lorem.Sentence() : null)
            .RuleFor(d => d.SequenceOrder, f => f.Random.Int(1, 10))
            .RuleFor(d => d.PlannedStartDate, f => f.Date.Soon(180))
            .RuleFor(d => d.PlannedEndDate, f => f.Date.Future(2))
            .RuleFor(d => d.Status, EntityStatus.Active)
            .RuleFor(d => d.IsDeleted, false)
            .RuleFor(d => d.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.CreatedDate, f => f.Date.Past(1))
            .RuleFor(d => d.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityCountry> GetOpportunityCountryFaker()
    {
        return new Faker<OpportunityCountry>()
            .RuleFor(oc => oc.Name, f => f.Address.Country())
            .RuleFor(oc => oc.CountryId, f => f.Random.Int(1, 50))
            .RuleFor(oc => oc.SpecificAreas, f => f.Random.Bool(0.4f) ? f.Address.State() : null)
            .RuleFor(oc => oc.RiskScore, f => f.Random.Bool(0.5f) ? f.Random.Decimal(0, 10) : null)
            .RuleFor(oc => oc.HumanitarianFrameworkAlignment, f => f.Random.Bool(0.3f))
            .RuleFor(oc => oc.NdcAlignment, f => f.Random.Bool(0.3f))
            .RuleFor(oc => oc.NapAlignment, f => f.Random.Bool(0.3f))
            .RuleFor(oc => oc.OrgUnitStrategyAlignment, f => f.Random.Bool(0.3f))
            .RuleFor(oc => oc.Status, EntityStatus.Active)
            .RuleFor(oc => oc.IsDeleted, false)
            .RuleFor(oc => oc.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(oc => oc.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(oc => oc.CreatedDate, f => f.Date.Past(1))
            .RuleFor(oc => oc.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunitySDG> GetOpportunitySDGFaker()
    {
        return new Faker<OpportunitySDG>()
            .RuleFor(s => s.Name, f => $"SDG {f.Random.Int(1, 17)}")
            .RuleFor(s => s.SDGId, f => f.Random.Int(1, 17))
            .RuleFor(s => s.IsPrimary, f => f.Random.Bool(0.2f))
            .RuleFor(s => s.SkipTargetsAndIndicators, f => f.Random.Bool(0.1f))
            .RuleFor(s => s.Notes, f => f.Random.Bool(0.3f) ? f.Lorem.Sentence() : null)
            .RuleFor(s => s.Status, EntityStatus.Active)
            .RuleFor(s => s.IsDeleted, false)
            .RuleFor(s => s.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(s => s.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(s => s.CreatedDate, f => f.Date.Past(1))
            .RuleFor(s => s.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityCollaborator> GetOpportunityCollaboratorFaker()
    {
        return new Faker<OpportunityCollaborator>()
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.UserId, f => f.Random.Int(1, 20))
            .RuleFor(c => c.AddedDate, f => f.Date.Recent(30))
            .RuleFor(c => c.AddedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.Status, EntityStatus.Active)
            .RuleFor(c => c.IsDeleted, false)
            .RuleFor(c => c.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.CreatedDate, f => f.Date.Past(1))
            .RuleFor(c => c.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityInteraction> GetOpportunityInteractionFaker()
    {
        return new Faker<OpportunityInteraction>()
            .RuleFor(oi => oi.Name, f => f.Lorem.Sentence(3))
            .RuleFor(oi => oi.InteractionId, f => f.Random.Int(1, 50))
            .RuleFor(oi => oi.Status, EntityStatus.Active)
            .RuleFor(oi => oi.IsDeleted, false)
            .RuleFor(oi => oi.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(oi => oi.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(oi => oi.CreatedDate, f => f.Date.Past(1))
            .RuleFor(oi => oi.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OpportunityUNOPSMission> GetOpportunityUNOPSMissionFaker()
    {
        return new Faker<OpportunityUNOPSMission>()
            .RuleFor(m => m.Name, f => f.Lorem.Sentence(3))
            .RuleFor(m => m.UNOPSMissionId, f => f.Random.Int(1, 5))
            .RuleFor(m => m.Status, EntityStatus.Active)
            .RuleFor(m => m.IsDeleted, false)
            .RuleFor(m => m.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(m => m.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(m => m.CreatedDate, f => f.Date.Past(1))
            .RuleFor(m => m.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DOCUMENT & AUDIT
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<Document> GetDocumentFaker()
    {
        return new Faker<Document>()
            .RuleFor(d => d.Name, f => f.System.FileName())
            .RuleFor(d => d.Link, f => f.Internet.Url())
            .RuleFor(d => d.StoragePath, f => $"/documents/{f.Random.AlphaNumeric(12)}/{f.System.FileName()}")
            .RuleFor(d => d.Type, f => f.PickRandom(new[] { "application/pdf", "image/png", "application/vnd.ms-excel", "application/msword", "text/plain" }))
            .RuleFor(d => d.AITranscribed, f => f.Random.Bool(0.1f))
            .RuleFor(d => d.DocumentTypeId, f => f.Random.Int(1, 10))
            .RuleFor(d => d.InteractionId, f => f.Random.Bool(0.3f) ? f.Random.Int(1, 50) : null)
            .RuleFor(d => d.Status, EntityStatus.Active)
            .RuleFor(d => d.IsDeleted, false)
            .RuleFor(d => d.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.CreatedDate, f => f.Date.Past(1))
            .RuleFor(d => d.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<UNOPSDocument> GetUNOPSDocumentFaker()
    {
        return new Faker<UNOPSDocument>()
            .RuleFor(d => d.Name, f => f.System.FileName())
            .RuleFor(d => d.Link, f => f.Internet.Url())
            .RuleFor(d => d.StoragePath, f => $"/documents/{f.Random.AlphaNumeric(12)}/{f.System.FileName()}")
            .RuleFor(d => d.Type, f => f.PickRandom(new[] { "application/pdf", "image/png", "application/vnd.ms-excel", "application/msword" }))
            .RuleFor(d => d.AITranscribed, f => f.Random.Bool(0.1f))
            .RuleFor(d => d.LinkedFile, f => f.Random.Bool(0.3f))
            .RuleFor(d => d.GoogleId, f => f.Random.AlphaNumeric(20))
            .RuleFor(d => d.DocumentTypeId, f => f.Random.Int(1, 10))
            .RuleFor(d => d.Status, EntityStatus.Active)
            .RuleFor(d => d.IsDeleted, false)
            .RuleFor(d => d.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(d => d.CreatedDate, f => f.Date.Past(1))
            .RuleFor(d => d.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<DocumentType> GetDocumentTypeFaker()
    {
        return new Faker<DocumentType>()
            .RuleFor(dt => dt.Name, f => f.PickRandom(new[] { "Proposal", "Contract", "Report", "Agreement", "Letter", "MOU", "Budget", "Invoice" }))
            .RuleFor(dt => dt.EntityType, f => f.PickRandom(new[] { "Partner", "Contact", "Opportunity", "Interaction" }))
            .RuleFor(dt => dt.Status, EntityStatus.Active)
            .RuleFor(dt => dt.IsDeleted, false)
            .RuleFor(dt => dt.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(dt => dt.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(dt => dt.CreatedDate, f => f.Date.Past(2))
            .RuleFor(dt => dt.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<DocumentRelationship> GetDocumentRelationshipFaker()
    {
        return new Faker<DocumentRelationship>()
            .RuleFor(dr => dr.Name, f => f.Lorem.Sentence(3))
            .RuleFor(dr => dr.DocumentId, f => f.Random.Int(1, 50))
            .RuleFor(dr => dr.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(dr => dr.EntityType, f => f.PickRandom(new[] { "Partner", "Contact", "Opportunity", "Interaction" }))
            .RuleFor(dr => dr.Description, f => f.Random.Bool(0.5f) ? f.Lorem.Sentence() : null)
            .RuleFor(dr => dr.Status, EntityStatus.Active)
            .RuleFor(dr => dr.IsDeleted, false)
            .RuleFor(dr => dr.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(dr => dr.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(dr => dr.CreatedDate, f => f.Date.Past(1))
            .RuleFor(dr => dr.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<AuditLog> GetAuditLogFaker()
    {
        return new Faker<AuditLog>()
            .RuleFor(a => a.Name, f => f.Lorem.Sentence(3))
            .RuleFor(a => a.EntityType, f => f.PickRandom(new[] { "Partner", "Contact", "Opportunity", "Interaction", "Document" }))
            .RuleFor(a => a.EntityId, f => f.Random.Int(1, 100))
            .RuleFor(a => a.Action, f => f.PickRandom(new[] { "Create", "Update", "Delete", "StatusChange", "StageChange", "RoleAssignment" }))
            .RuleFor(a => a.Timestamp, f => f.Date.Recent(30))
            .RuleFor(a => a.UserId, f => f.Random.Int(1, 20))
            .RuleFor(a => a.Description, f => f.Lorem.Sentence())
            .RuleFor(a => a.JsonData, (f, a) => $"{{\"action\":\"{a.Action}\",\"entityType\":\"{a.EntityType}\",\"entityId\":{a.EntityId}}}")
            .RuleFor(a => a.Status, EntityStatus.Active)
            .RuleFor(a => a.IsDeleted, false)
            .RuleFor(a => a.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(a => a.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(a => a.CreatedDate, f => f.Date.Past(1))
            .RuleFor(a => a.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  RISK & RISK LOOKUPS
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<Risk> GetRiskFaker()
    {
        return new Faker<Risk>()
            .RuleFor(r => r.Name, f => f.Lorem.Sentence(4))
            .RuleFor(r => r.Title, f => f.Lorem.Sentence(6))
            .RuleFor(r => r.EntityType, f => f.PickRandom(new[] { "Opportunity", "Partner" }))
            .RuleFor(r => r.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(r => r.RiskTypeId, f => f.Random.Int(1, 5))
            .RuleFor(r => r.RiskCategoryId, f => f.Random.Int(1, 10))
            .RuleFor(r => r.RiskProbabilityId, f => f.Random.Int(1, 5))
            .RuleFor(r => r.RiskProximityId, f => f.Random.Int(1, 5))
            .RuleFor(r => r.RiskImpactLevelId, f => f.Random.Int(1, 5))
            .RuleFor(r => r.RiskResponseTypeId, f => f.Random.Bool(0.7f) ? f.Random.Int(1, 5) : null)
            .RuleFor(r => r.Description, f => f.Lorem.Paragraph())
            .RuleFor(r => r.Recommendation, f => f.Lorem.Paragraph())
            .RuleFor(r => r.Impact, f => f.PickRandom<RiskImpact>())
            .RuleFor(r => r.RiskStatus, f => f.PickRandom<RiskStatus>())
            .RuleFor(r => r.IdentifiedDate, f => f.Date.Past(1))
            .RuleFor(r => r.IdentifiedBy, f => f.Random.Int(1, 10))
            .RuleFor(r => r.Status, EntityStatus.Active)
            .RuleFor(r => r.IsDeleted, false)
            .RuleFor(r => r.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(r => r.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(r => r.CreatedDate, f => f.Date.Past(1))
            .RuleFor(r => r.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<RiskType> GetRiskTypeFaker()
    {
        var index = 0;
        return new Faker<RiskType>()
            .RuleFor(rt => rt.Id, _ => ++index)
            .RuleFor(rt => rt.Name, f => f.PickRandom(new[] { "Strategic", "Operational", "Financial", "Compliance", "Reputational" }))
            .RuleFor(rt => rt.Code, (f, rt) => rt.Name[..3].ToUpper())
            .RuleFor(rt => rt.Description, f => f.Lorem.Sentence())
            .RuleFor(rt => rt.DisplayOrder, f => f.Random.Int(1, 10))
            .RuleFor(rt => rt.IsResponseTypeMandatory, f => f.Random.Bool(0.5f))
            .RuleFor(rt => rt.Status, EntityStatus.Active)
            .RuleFor(rt => rt.IsDeleted, false);
    }

    public static Faker<RiskCategory> GetRiskCategoryFaker()
    {
        return new Faker<RiskCategory>()
            .RuleFor(rc => rc.Name, f => f.PickRandom(new[] { "Safety", "Environmental", "Security", "Social", "Governance", "Technical", "Political" }))
            .RuleFor(rc => rc.Code, (f, rc) => $"RC-{rc.Name[..3].ToUpper()}")
            .RuleFor(rc => rc.ShortCode, (f, rc) => rc.Name[..3].ToUpper())
            .RuleFor(rc => rc.Level, f => f.Random.Int(1, 3))
            .RuleFor(rc => rc.DisplayOrder, f => f.Random.Int(1, 20))
            .RuleFor(rc => rc.Status, EntityStatus.Active)
            .RuleFor(rc => rc.IsDeleted, false)
            .RuleFor(rc => rc.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(rc => rc.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(rc => rc.CreatedDate, f => f.Date.Past(2))
            .RuleFor(rc => rc.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<RiskProbability> GetRiskProbabilityFaker()
    {
        var index = 0;
        return new Faker<RiskProbability>()
            .RuleFor(rp => rp.Id, _ => ++index)
            .RuleFor(rp => rp.Name, f => f.PickRandom(new[] { "Rare", "Unlikely", "Possible", "Likely", "Almost Certain" }))
            .RuleFor(rp => rp.Code, (f, rp) => rp.Name[..3].ToUpper())
            .RuleFor(rp => rp.DisplayLabel, (f, rp) => $"{rp.Name} ({rp.NumericValue})")
            .RuleFor(rp => rp.NumericValue, f => f.Random.Int(1, 5))
            .RuleFor(rp => rp.DisplayOrder, f => f.Random.Int(1, 5))
            .RuleFor(rp => rp.Status, EntityStatus.Active)
            .RuleFor(rp => rp.IsDeleted, false);
    }

    public static Faker<RiskProximity> GetRiskProximityFaker()
    {
        var index = 0;
        return new Faker<RiskProximity>()
            .RuleFor(rp => rp.Id, _ => ++index)
            .RuleFor(rp => rp.Name, f => f.PickRandom(new[] { "Immediate", "Short-term", "Medium-term", "Long-term" }))
            .RuleFor(rp => rp.Code, (f, rp) => rp.Name.Replace("-", "")[..4].ToUpper())
            .RuleFor(rp => rp.MonthsValue, f => f.PickRandom(new int?[] { 1, 3, 6, 12, 24 }))
            .RuleFor(rp => rp.DisplayOrder, f => f.Random.Int(1, 5))
            .RuleFor(rp => rp.Status, EntityStatus.Active)
            .RuleFor(rp => rp.IsDeleted, false);
    }

    public static Faker<RiskImpactLevel> GetRiskImpactLevelFaker()
    {
        var index = 0;
        return new Faker<RiskImpactLevel>()
            .RuleFor(ri => ri.Id, _ => ++index)
            .RuleFor(ri => ri.Name, f => f.PickRandom(new[] { "Negligible", "Minor", "Moderate", "Major", "Severe" }))
            .RuleFor(ri => ri.Code, (f, ri) => ri.Name[..3].ToUpper())
            .RuleFor(ri => ri.DisplayLabel, (f, ri) => $"{ri.Name} ({ri.NumericValue})")
            .RuleFor(ri => ri.NumericValue, f => f.Random.Int(1, 5))
            .RuleFor(ri => ri.DisplayOrder, f => f.Random.Int(1, 5))
            .RuleFor(ri => ri.Status, EntityStatus.Active)
            .RuleFor(ri => ri.IsDeleted, false);
    }

    public static Faker<RiskResponseType> GetRiskResponseTypeFaker()
    {
        var index = 0;
        return new Faker<RiskResponseType>()
            .RuleFor(rr => rr.Id, _ => ++index)
            .RuleFor(rr => rr.Name, f => f.PickRandom(new[] { "Avoid", "Mitigate", "Transfer", "Accept", "Exploit", "Share" }))
            .RuleFor(rr => rr.Code, (f, rr) => rr.Name[..3].ToUpper())
            .RuleFor(rr => rr.Description, f => f.Lorem.Sentence())
            .RuleFor(rr => rr.ValidForThreat, f => f.Random.Bool())
            .RuleFor(rr => rr.ValidForOpportunity, f => f.Random.Bool())
            .RuleFor(rr => rr.DisplayOrder, f => f.Random.Int(1, 10))
            .RuleFor(rr => rr.Status, EntityStatus.Active)
            .RuleFor(rr => rr.IsDeleted, false);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ORGANIZATION, COUNTRY, REFERENCE DATA
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<OrganizationHierarchy> GetOrganizationHierarchyFaker()
    {
        return new Faker<OrganizationHierarchy>()
            .RuleFor(o => o.Code, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(o => o.Name, f => f.Company.CompanyName() + " Office")
            .RuleFor(o => o.Description, f => f.Lorem.Sentence())
            .RuleFor(o => o.Type, f => f.PickRandom<OrganizationUnitType>())
            .RuleFor(o => o.IsSelfManagementEnabled, f => f.Random.Bool(0.2f))
            .RuleFor(o => o.Status, EntityStatus.Active)
            .RuleFor(o => o.IsDeleted, false)
            .RuleFor(o => o.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.CreatedDate, f => f.Date.Past(2))
            .RuleFor(o => o.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<OrganizationUnitRelationship> GetOrganizationUnitRelationshipFaker()
    {
        return new Faker<OrganizationUnitRelationship>()
            .RuleFor(r => r.Name, (f, r) => $"{r.EntityType}-{r.EntityId}-OrgUnit-{r.OrganizationHierarchyId}")
            .RuleFor(r => r.OrganizationHierarchyId, f => f.Random.Int(1, 100))
            .RuleFor(r => r.EntityId, f => f.Random.Int(1, 100))
            .RuleFor(r => r.EntityType, f => f.PickRandom(new[] { "Partner", "UNOPSPartner", "Contact", "Interaction", "Opportunity" }))
            .RuleFor(r => r.Status, f => f.PickRandom<EntityStatus>())
            .RuleFor(r => r.IsDeleted, false)
            .RuleFor(r => r.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(r => r.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(r => r.CreatedDate, f => f.Date.Past(2))
            .RuleFor(r => r.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<Country> GetCountryFaker()
    {
        return new Faker<Country>()
            .RuleFor(c => c.Name, f => f.Address.Country())
            .RuleFor(c => c.Iso2Code, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha2))
            .RuleFor(c => c.Iso3Code, f => f.Address.CountryCode(Bogus.DataSets.Iso3166Format.Alpha3))
            .RuleFor(c => c.RegionDescription, f => f.PickRandom(new[] { "Eastern Europe", "Western Africa", "South-East Asia", "Latin America", "Central Asia", "Middle East" }))
            .RuleFor(c => c.ContinentDescription, f => f.PickRandom(new[] { "Africa", "Asia", "Europe", "North America", "South America", "Oceania" }))
            .RuleFor(c => c.Status, EntityStatus.Active)
            .RuleFor(c => c.IsDeleted, false);
    }

    public static Faker<Currency> GetCurrencyFaker()
    {
        var index = 0;
        var currencies = new[] { ("USD", "US Dollar", "$"), ("EUR", "Euro", "€"), ("GBP", "British Pound", "£"), ("CHF", "Swiss Franc", "CHF"), ("JPY", "Japanese Yen", "¥"), ("CAD", "Canadian Dollar", "C$"), ("AUD", "Australian Dollar", "A$"), ("SEK", "Swedish Krona", "kr"), ("NOK", "Norwegian Krone", "kr"), ("DKK", "Danish Krone", "kr") };
        return new Faker<Currency>()
            .RuleFor(c => c.Id, _ => ++index)
            .RuleFor(c => c.Code, f => { var cur = f.PickRandom(currencies); return cur.Item1; })
            .RuleFor(c => c.Name, (f, c) => currencies.FirstOrDefault(x => x.Item1 == c.Code).Item2 ?? c.Code)
            .RuleFor(c => c.Symbol, (f, c) => currencies.FirstOrDefault(x => x.Item1 == c.Code).Item3)
            .RuleFor(c => c.DecimalPlaces, f => f.PickRandom(new int?[] { 0, 2, 3 }))
            .RuleFor(c => c.Status, EntityStatus.Active)
            .RuleFor(c => c.IsDeleted, false);
    }

    public static Faker<ExchangeRate> GetExchangeRateFaker()
    {
        var index = 0;
        return new Faker<ExchangeRate>()
            .RuleFor(e => e.Id, _ => ++index)
            .RuleFor(e => e.Name, f => $"USD/{f.PickRandom(new[] { "EUR", "GBP", "CHF", "JPY" })}")
            .RuleFor(e => e.Currency, (f, e) => e.Name.Split('/').Last())
            .RuleFor(e => e.Effective_Date, f => f.Date.Recent(90))
            .RuleFor(e => e.Exchange_Rate_Sequence_No, f => f.Random.Int(1, 1000))
            .RuleFor(e => e.Exchange_Rate, f => f.Random.Decimal(0.5m, 150m))
            .RuleFor(e => e.Status, EntityStatus.Active)
            .RuleFor(e => e.IsDeleted, false);
    }

    public static Faker<LiaisonOffice> GetLiaisonOfficeFaker()
    {
        return new Faker<LiaisonOffice>()
            .RuleFor(lo => lo.Code, f => f.Random.AlphaNumeric(6).ToUpper())
            .RuleFor(lo => lo.Name, f => f.Address.City() + " Liaison Office")
            .RuleFor(lo => lo.Description, f => f.Lorem.Sentence())
            .RuleFor(lo => lo.Region, f => f.PickRandom(new[] { "Asia Pacific", "Africa", "Europe", "Americas", "Middle East" }))
            .RuleFor(lo => lo.Country, f => f.Address.Country())
            .RuleFor(lo => lo.IsActive, true)
            .RuleFor(lo => lo.Status, EntityStatus.Active)
            .RuleFor(lo => lo.IsDeleted, false)
            .RuleFor(lo => lo.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(lo => lo.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(lo => lo.CreatedDate, f => f.Date.Past(2))
            .RuleFor(lo => lo.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<ProposedInitiativeType> GetProposedInitiativeTypeFaker()
    {
        var index = 0;
        return new Faker<ProposedInitiativeType>()
            .RuleFor(p => p.Id, _ => ++index)
            .RuleFor(p => p.Name, f => f.PickRandom(new[] { "Grant", "Loan", "Technical Assistance", "Capacity Building", "Emergency Response", "Infrastructure" }))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Order, f => f.Random.Int(1, 10))
            .RuleFor(p => p.Status, EntityStatus.Active)
            .RuleFor(p => p.IsDeleted, false)
            .RuleFor(p => p.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(p => p.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(p => p.CreatedDate, f => f.Date.Past(2))
            .RuleFor(p => p.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<PartnerAgreement> GetPartnerAgreementFaker()
    {
        return new Faker<PartnerAgreement>()
            .RuleFor(pa => pa.Name, f => f.Lorem.Sentence(4))
            .RuleFor(pa => pa.BasePartnerAgreementNumber, f => $"BPA-{f.Random.Number(10000, 99999)}")
            .RuleFor(pa => pa.PartnerAgreementNumber, f => $"PA-{f.Random.Number(10000, 99999)}")
            .RuleFor(pa => pa.PartnerAgreementDescriptionLong, f => f.Lorem.Paragraph())
            .RuleFor(pa => pa.PartnerAgreementType, f => f.PickRandom(new[] { "Framework", "Specific", "Amendment" }))
            .RuleFor(pa => pa.PartnerAgreementTypeDescription, f => f.Lorem.Sentence())
            .RuleFor(pa => pa.PartnerAgreementScope, f => f.PickRandom(new[] { "Global", "Regional", "Country" }))
            .RuleFor(pa => pa.PartnerAgreementScopeDescription, f => f.Lorem.Sentence())
            .RuleFor(pa => pa.PartnerAgreementPartner, f => f.Company.CompanyName())
            .RuleFor(pa => pa.PartnerAgreementPartnerDescription, f => f.Lorem.Sentence())
            .RuleFor(pa => pa.PartnerAgreementStartDate, f => f.Date.Past(2))
            .RuleFor(pa => pa.PartnerAgreementEndDate, f => f.Date.Future(3))
            .RuleFor(pa => pa.PartnerAgreementSignedDate, f => f.Date.Past(2))
            .RuleFor(pa => pa.PartnerAgreementResponsibleOrgUnit, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(pa => pa.PartnerAgreementResponsibleOrgUnitDescription, f => f.Company.CompanyName() + " Office")
            .RuleFor(pa => pa.PartnerAgreementServiceLineInfrastructureFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementServiceLineProcurementFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementServiceLineProjectManagementFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementServiceLineFundManagementFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementServiceLineHumanResourcesFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementServiceLineOtherFlag, f => f.Random.Bool())
            .RuleFor(pa => pa.PartnerAgreementCountries, f => string.Join(", ", f.Make(3, () => f.Address.Country())))
            .RuleFor(pa => pa.Status, EntityStatus.Active)
            .RuleFor(pa => pa.IsDeleted, false);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SDG, UNCF, MISSIONS REFERENCE DATA
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<SDG> GetSDGFaker()
    {
        var index = 0;
        var sdgNames = new[] { "No Poverty", "Zero Hunger", "Good Health and Well-being", "Quality Education", "Gender Equality", "Clean Water and Sanitation", "Affordable and Clean Energy", "Decent Work and Economic Growth", "Industry, Innovation and Infrastructure", "Reduced Inequalities", "Sustainable Cities and Communities", "Responsible Consumption and Production", "Climate Action", "Life Below Water", "Life on Land", "Peace, Justice and Strong Institutions", "Partnerships for the Goals" };
        return new Faker<SDG>()
            .RuleFor(s => s.Id, _ => ++index)
            .RuleFor(s => s.SDGNumber, (f, s) => s.Id.ToString())
            .RuleFor(s => s.SDGId, (f, s) => $"SDG-{s.Id}")
            .RuleFor(s => s.Name, (f, s) => s.Id <= sdgNames.Length ? sdgNames[s.Id - 1] : f.Lorem.Sentence(3))
            .RuleFor(s => s.SDGDescription, f => f.Lorem.Paragraph())
            .RuleFor(s => s.SDGLongDescription, f => f.Lorem.Paragraphs(2))
            .RuleFor(s => s.Status, EntityStatus.Active)
            .RuleFor(s => s.IsDeleted, false);
    }

    public static Faker<SDGTarget> GetSDGTargetFaker()
    {
        var index = 0;
        return new Faker<SDGTarget>()
            .RuleFor(t => t.Id, _ => ++index)
            .RuleFor(t => t.SDGId, f => f.Random.Int(1, 17).ToString())
            .RuleFor(t => t.SDGTargetId, (f, t) => $"{t.SDGId}.{f.Random.Int(1, 10)}")
            .RuleFor(t => t.Name, f => f.Lorem.Sentence(6))
            .RuleFor(t => t.TargetDescription, f => f.Lorem.Paragraph())
            .RuleFor(t => t.TargetType, f => f.PickRandom(new[] { "Outcome", "Means of Implementation" }))
            .RuleFor(t => t.Status, EntityStatus.Active)
            .RuleFor(t => t.IsDeleted, false);
    }

    public static Faker<SDGIndicator> GetSDGIndicatorFaker()
    {
        var index = 0;
        return new Faker<SDGIndicator>()
            .RuleFor(i => i.Id, _ => ++index)
            .RuleFor(i => i.SDGTargetId, f => $"{f.Random.Int(1, 17)}.{f.Random.Int(1, 10)}")
            .RuleFor(i => i.SDGIndicatorId, (f, i) => $"{i.SDGTargetId}.{f.Random.Int(1, 5)}")
            .RuleFor(i => i.Name, f => f.Lorem.Sentence(8))
            .RuleFor(i => i.SDGIndicatorLongDescription, f => f.Lorem.Paragraphs(2))
            .RuleFor(i => i.Status, EntityStatus.Active)
            .RuleFor(i => i.IsDeleted, false);
    }

    public static Faker<UNOPSMission> GetUNOPSMissionFaker()
    {
        var index = 0;
        var missions = new[] { "Infrastructure", "Procurement", "Project Management", "Fund Management", "Human Resources" };
        return new Faker<UNOPSMission>()
            .RuleFor(m => m.Id, _ => ++index)
            .RuleFor(m => m.Name, f => f.PickRandom(missions))
            .RuleFor(m => m.Code, (f, m) => m.Name[..3].ToUpper())
            .RuleFor(m => m.Description, f => f.Lorem.Sentence())
            .RuleFor(m => m.DisplayOrder, f => f.Random.Int(1, 10))
            .RuleFor(m => m.IconClass, f => f.PickRandom(new[] { "pi pi-building", "pi pi-shopping-cart", "pi pi-chart-bar", "pi pi-wallet", "pi pi-users" }))
            .RuleFor(m => m.Status, EntityStatus.Active)
            .RuleFor(m => m.IsDeleted, false);
    }

    public static Faker<Output> GetOutputFaker()
    {
        return new Faker<Output>()
            .RuleFor(o => o.Name, f => f.Lorem.Sentence(4))
            .RuleFor(o => o.Level0, f => f.Lorem.Sentence(3))
            .RuleFor(o => o.Level1, f => f.Lorem.Sentence(3))
            .RuleFor(o => o.DefinitionLevel1, f => f.Lorem.Paragraph())
            .RuleFor(o => o.Level2, f => f.Random.Bool(0.5f) ? f.Lorem.Sentence(3) : null)
            .RuleFor(o => o.DefinitionLevel2, f => f.Random.Bool(0.5f) ? f.Lorem.Paragraph() : null)
            .RuleFor(o => o.ServiceLine, f => f.PickRandom(new[] { "Infrastructure", "Procurement", "Project Management", "Fund Management", "HR" }))
            .RuleFor(o => o.GrantSupportImplementingModality, f => f.Random.Bool(0.3f))
            .RuleFor(o => o.GrantSupportComponent, f => f.Random.Bool(0.3f))
            .RuleFor(o => o.ProcurementComponent, f => f.Random.Bool(0.3f))
            .RuleFor(o => o.InfrastructureComponent, f => f.Random.Bool(0.3f))
            .RuleFor(o => o.Status, EntityStatus.Active)
            .RuleFor(o => o.IsDeleted, false)
            .RuleFor(o => o.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(o => o.CreatedDate, f => f.Date.Past(2))
            .RuleFor(o => o.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  USER, ROLES, COMMENTS, NOTIFICATIONS
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<PAOUser> GetPAOUserFaker()
    {
        var index = 0;
        return new Faker<PAOUser>()
            .RuleFor(u => u.Id, _ => ++index)
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.IsInternal, f => f.Random.Bool(0.8f))
            .RuleFor(u => u.ActiveUser, true);
    }

    public static Faker<UserProfile> GetUserProfileFaker()
    {
        return new Faker<UserProfile>()
            .RuleFor(up => up.FirstName, f => f.Name.FirstName())
            .RuleFor(up => up.LastName, f => f.Name.LastName())
            .RuleFor(up => up.UserEmail, f => f.Internet.Email())
            .RuleFor(up => up.OrgUnit, f => f.Random.AlphaNumeric(5).ToUpper())
            .RuleFor(up => up.DutyStation, f => f.Address.City())
            .RuleFor(up => up.Position, f => f.Name.JobTitle())
            .RuleFor(up => up.UserId, f => f.Random.Int(1, 20))
            .RuleFor(up => up.Status, EntityStatus.Active)
            .RuleFor(up => up.IsDeleted, false)
            .RuleFor(up => up.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(up => up.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(up => up.CreatedDate, f => f.Date.Past(2))
            .RuleFor(up => up.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<EntityRole> GetEntityRoleFaker()
    {
        return new Faker<EntityRole>()
            .RuleFor(er => er.EntityType, f => f.PickRandom(new[] { "Opportunity", "Partner", "Contact" }))
            .RuleFor(er => er.Name, f => f.PickRandom(new[] { "Opportunity Manager", "Business Developer", "Project Executive", "Focal Point", "Reviewer", "Approver" }))
            .RuleFor(er => er.Description, f => f.Lorem.Sentence())
            .RuleFor(er => er.IsInternal, f => f.Random.Bool(0.8f))
            .RuleFor(er => er.AllowsMultiple, f => f.Random.Bool(0.3f))
            .RuleFor(er => er.Type, f => f.PickRandom(new[] { "Primary", "Secondary", "Support" }))
            .RuleFor(er => er.Code, f => f.Random.AlphaNumeric(6).ToUpper())
            .RuleFor(er => er.Status, EntityStatus.Active)
            .RuleFor(er => er.IsDeleted, false)
            .RuleFor(er => er.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(er => er.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(er => er.CreatedDate, f => f.Date.Past(2))
            .RuleFor(er => er.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<EntityRolePerson> GetEntityRolePersonFaker()
    {
        return new Faker<EntityRolePerson>()
            .RuleFor(erp => erp.Name, f => f.Name.FullName())
            .RuleFor(erp => erp.EntityType, f => f.PickRandom(new[] { "Opportunity", "Partner" }))
            .RuleFor(erp => erp.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(erp => erp.EntityRoleId, f => f.Random.Int(1, 10))
            .RuleFor(erp => erp.UserId, f => f.Random.Int(1, 20))
            .RuleFor(erp => erp.EffectiveDate, f => f.Date.Past(1))
            .RuleFor(erp => erp.EndDate, f => f.Random.Bool(0.3f) ? f.Date.Future(1) : null)
            .RuleFor(erp => erp.Status, EntityStatus.Active)
            .RuleFor(erp => erp.IsDeleted, false)
            .RuleFor(erp => erp.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(erp => erp.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(erp => erp.CreatedDate, f => f.Date.Past(1))
            .RuleFor(erp => erp.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<EntityUserRole> GetEntityUserRoleFaker()
    {
        return new Faker<EntityUserRole>()
            .RuleFor(eur => eur.Name, f => f.Name.FullName() + " - " + f.Lorem.Word())
            .RuleFor(eur => eur.UserId, f => f.Random.Int(1, 20))
            .RuleFor(eur => eur.EntityRoleId, f => f.Random.Int(1, 10))
            .RuleFor(eur => eur.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(eur => eur.EntityType, f => f.PickRandom(new[] { "Opportunity", "Partner", "Contact" }))
            .RuleFor(eur => eur.RoleSource, f => f.PickRandom(new[] { "Manual", "Auto", "Inherited" }))
            .RuleFor(eur => eur.Status, EntityStatus.Active)
            .RuleFor(eur => eur.IsDeleted, false)
            .RuleFor(eur => eur.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(eur => eur.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(eur => eur.CreatedDate, f => f.Date.Past(1))
            .RuleFor(eur => eur.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<Comment> GetCommentFaker()
    {
        return new Faker<Comment>()
            .RuleFor(c => c.Name, f => f.Lorem.Sentence(3))
            .RuleFor(c => c.EntityType, f => f.PickRandom(new[] { "Opportunity", "Partner", "Contact", "Interaction" }))
            .RuleFor(c => c.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
            .RuleFor(c => c.IsEdited, false)
            .RuleFor(c => c.IsPinned, f => f.Random.Bool(0.1f))
            .RuleFor(c => c.Status, EntityStatus.Active)
            .RuleFor(c => c.IsDeleted, false)
            .RuleFor(c => c.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(c => c.CreatedDate, f => f.Date.Recent(30))
            .RuleFor(c => c.LastModifiedDate, f => f.Date.Recent());
    }

    public static Faker<Notification> GetNotificationFaker()
    {
        return new Faker<Notification>()
            .RuleFor(n => n.UserId, f => f.Random.Int(1, 20))
            .RuleFor(n => n.Message, f => f.Lorem.Sentence())
            .RuleFor(n => n.Category, f => f.PickRandom(new[] { "RoleAssignment", "StageChange", "Comment", "Mention", "Deadline" }))
            .RuleFor(n => n.ResponseType, f => f.PickRandom(new[] { "Info", "Action", "Warning" }))
            .RuleFor(n => n.RecordData, f => $"{{\"id\":{f.Random.Int(1, 100)}}}")
            .RuleFor(n => n.Entity, f => f.PickRandom(new[] { "Opportunity", "Partner", "Contact" }))
            .RuleFor(n => n.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(n => n.IsRead, f => f.Random.Bool(0.3f))
            .RuleFor(n => n.Status, f => f.PickRandom<NotificationStatus>())
            .RuleFor(n => n.CreatedAt, f => f.Date.Recent(14));
    }

    public static Faker<DomainLink> GetLinkFaker()
    {
        return new Faker<DomainLink>()
            .RuleFor(l => l.Name, f => f.Internet.DomainName())
            .RuleFor(l => l.Entity, f => f.PickRandom<LinkEntityType>())
            .RuleFor(l => l.EntityId, f => f.Random.Int(1, 50))
            .RuleFor(l => l.Url, f => f.Internet.Url())
            .RuleFor(l => l.Status, EntityStatus.Active)
            .RuleFor(l => l.IsDeleted, false)
            .RuleFor(l => l.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(l => l.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(l => l.CreatedDate, f => f.Date.Past(1))
            .RuleFor(l => l.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  JUNCTION / RELATIONSHIP ENTITIES
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<InteractionContact> GetInteractionContactFaker()
    {
        return new Faker<InteractionContact>()
            .RuleFor(ic => ic.InteractionId, f => f.Random.Int(1, 50))
            .RuleFor(ic => ic.ContactId, f => f.Random.Int(1, 50));
    }

    public static Faker<InteractionPartner> GetInteractionPartnerFaker()
    {
        return new Faker<InteractionPartner>()
            .RuleFor(ip => ip.InteractionId, f => f.Random.Int(1, 50))
            .RuleFor(ip => ip.PartnerId, f => f.Random.Int(1, 50));
    }

    public static Faker<InteractionUser> GetInteractionUserFaker()
    {
        return new Faker<InteractionUser>()
            .RuleFor(iu => iu.InteractionId, f => f.Random.Int(1, 50))
            .RuleFor(iu => iu.UserId, f => f.Random.Int(1, 20));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PARTNER TREE / GROUP ENTITIES
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<PartnerTree> GetPartnerTreeFaker()
    {
        return new Faker<PartnerTree>()
            .RuleFor(pt => pt.Name, f => f.Company.CompanyName())
            .RuleFor(pt => pt.Description, f => f.Lorem.Sentence())
            .RuleFor(pt => pt.Code, f => f.Random.AlphaNumeric(6).ToUpper())
            .RuleFor(pt => pt.Type, f => f.PickRandom(new[] { "Category", "Group" }))
            .RuleFor(pt => pt.Parent, f => f.Random.Bool(0.3f) ? f.Random.AlphaNumeric(6).ToUpper() : null)
            .RuleFor(pt => pt.PartnerCategoryCode, f => f.Random.AlphaNumeric(4).ToUpper())
            .RuleFor(pt => pt.PartnerGroupCode, f => f.Random.AlphaNumeric(4).ToUpper())
            .RuleFor(pt => pt.Status, EntityStatus.Active)
            .RuleFor(pt => pt.IsDeleted, false)
            .RuleFor(pt => pt.CreatedBy, f => f.Random.Int(1, 5))
            .RuleFor(pt => pt.LastModifiedBy, f => f.Random.Int(1, 5))
            .RuleFor(pt => pt.CreatedDate, f => f.Date.Past(2))
            .RuleFor(pt => pt.LastModifiedDate, f => f.Date.Recent());
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ENTITY PERMISSION (for seeding)
    // ═══════════════════════════════════════════════════════════════════════

    public static Faker<EntityPermission> GetEntityPermissionFaker()
    {
        return new Faker<EntityPermission>()
            .RuleFor(ep => ep.Entity, f => f.PickRandom(new[] { "Partner", "Contact", "Opportunity", "Interaction", "Document" }))
            .RuleFor(ep => ep.Role, f => f.PickRandom(new[] { "User", "Admin", "ReadOnly", "Manager" }))
            .RuleFor(ep => ep.CanRead, true)
            .RuleFor(ep => ep.CanCreate, f => f.Random.Bool(0.8f))
            .RuleFor(ep => ep.CanUpdate, f => f.Random.Bool(0.7f))
            .RuleFor(ep => ep.CanDelete, f => f.Random.Bool(0.5f));
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CONVENIENCE: Filter Request Builders
    // ═══════════════════════════════════════════════════════════════════════

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
