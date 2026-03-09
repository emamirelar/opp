using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Fluent test data builders for common entities.
/// Consolidates duplicated seed logic from fixture classes into a single reusable API.
///
/// Usage:
///   var partnerId = await TestEntityBuilder.Partner()
///       .WithName("UNICEF")
///       .WithStatus(EntityStatus.Active)
///       .BuildAsync(context);
///
///   var oppId = await TestEntityBuilder.Opportunity()
///       .WithName("Test Opp")
///       .WithStage("IDENTIFY &amp; PROFILE")
///       .WithCreatedBy(userId)
///       .BuildAsync(context);
/// </summary>
public static class TestEntityBuilder
{
    public static UserBuilder User() => new();
    public static PartnerBuilder Partner() => new();
    public static OpportunityBuilder Opportunity() => new();
    public static CurrencyBuilder Currency() => new();
    public static CountryBuilder Country() => new();
    public static SdgBuilder SDG() => new();
    public static OrgHierarchyBuilder OrgHierarchy() => new();
    public static ContactBuilder Contact() => new();
    public static InteractionBuilder Interaction() => new();
    public static EntityRoleBuilder EntityRole() => new();
    public static InitiativeTypeBuilder InitiativeType() => new();
    public static OutputBuilder Output() => new();
}

#region User

public sealed class UserBuilder
{
    private string _email = "testuser@unops.org";

    public UserBuilder WithEmail(string email) { _email = email; return this; }

    /// <summary>
    /// Creates or retrieves a test user via <see cref="TestDataHelper"/>.
    /// Delegates to the async version.
    /// </summary>
    public int Build(AppDbContext context) =>
        TestDataHelper.GetOrCreateTestUser(context, _email);

    public Task<int> BuildAsync(AppDbContext context) =>
        TestDataHelper.GetOrCreateTestUserAsync(context, _email);
}

#endregion

#region Partner

public sealed class PartnerBuilder
{
    private string _name = "Test Partner";
    private EntityStatus _status = EntityStatus.Active;
    private bool _isDeleted;
    private int? _createdBy;
    private string? _shortDescription;

    public PartnerBuilder WithName(string name) { _name = name; return this; }
    public PartnerBuilder WithStatus(EntityStatus status) { _status = status; return this; }
    public PartnerBuilder Deleted() { _isDeleted = true; return this; }
    public PartnerBuilder WithCreatedBy(int userId) { _createdBy = userId; return this; }
    public PartnerBuilder WithShortDescription(string desc) { _shortDescription = desc; return this; }

    public async Task<int> BuildAsync(AppDbContext context)
    {
        var partner = new UNOPSPartner
        {
            Name = _name,
            PartnerShortDescription = _shortDescription ?? _name,
            Status = _status,
            IsDeleted = _isDeleted,
            CreatedBy = _createdBy ?? 0,
            LastModifiedBy = _createdBy ?? 0,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        context.Partners.Add(partner);
        await context.SaveChangesAsync();
        return partner.Id;
    }
}

#endregion

#region Opportunity

public sealed class OpportunityBuilder
{
    private string _name = "Test Opportunity";
    private string _description = "Test description";
    private string _stage = "IDENTIFY & PROFILE";
    private EntityStatus _status = EntityStatus.Draft;
    private bool _isDeleted;
    private int? _createdBy;
    private decimal? _budgetUsd;
    private int? _responsibleOrgUnitId;
    private string? _statementMarkdown;
    private bool _highRisksAcknowledged;

    public OpportunityBuilder WithName(string name) { _name = name; return this; }
    public OpportunityBuilder WithDescription(string desc) { _description = desc; return this; }
    public OpportunityBuilder WithStage(string stage) { _stage = stage; return this; }
    public OpportunityBuilder WithStatus(EntityStatus status) { _status = status; return this; }
    public OpportunityBuilder Deleted() { _isDeleted = true; return this; }
    public OpportunityBuilder WithCreatedBy(int userId) { _createdBy = userId; return this; }
    public OpportunityBuilder WithBudgetUSD(decimal budget) { _budgetUsd = budget; return this; }
    public OpportunityBuilder WithResponsibleOrgUnit(int orgUnitId) { _responsibleOrgUnitId = orgUnitId; return this; }
    public OpportunityBuilder WithStatement(string markdown) { _statementMarkdown = markdown; return this; }
    public OpportunityBuilder WithHighRisksAcknowledged(bool value = true) { _highRisksAcknowledged = value; return this; }

    public async Task<int> BuildAsync(AppDbContext context)
    {
        var now = DateTime.UtcNow;
        var userId = _createdBy ?? 0;
        var opp = new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Name = _name,
            Description = _description,
            Stage = _stage,
            Status = _status,
            IsDeleted = _isDeleted,
            CreatedBy = userId,
            CreatedDate = now,
            LastModifiedBy = userId,
            LastModifiedDate = now,
            InitiativeBudgetUSD = _budgetUsd,
            ResponsibleOrgUnitId = _responsibleOrgUnitId,
            OpportunityStatementMarkdown = _statementMarkdown,
            HighRisksAcknowledged = _highRisksAcknowledged
        };
        context.Opportunities.Add(opp);
        await context.SaveChangesAsync();
        return opp.Id;
    }
}

#endregion

#region Currency

public sealed class CurrencyBuilder
{
    private string _code = "USD";
    private string? _name;

    public CurrencyBuilder WithCode(string code) { _code = code; return this; }
    public CurrencyBuilder WithName(string name) { _name = name; return this; }

    /// <summary>
    /// Creates or retrieves a currency by code (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.Set<Currency>()
            .FirstOrDefaultAsync(c => c.Code == _code && !c.IsDeleted);
        if (existing != null) return existing.Id;

        var currency = new Currency
        {
            Code = _code,
            Name = _name ?? _code,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.Set<Currency>().Add(currency);
        await context.SaveChangesAsync();
        return currency.Id;
    }
}

#endregion

#region Country

public sealed class CountryBuilder
{
    private string _iso2 = "XX";
    private string _name = "Test Country";
    private string? _iso3;

    public CountryBuilder WithIso2(string iso2) { _iso2 = iso2; return this; }
    public CountryBuilder WithName(string name) { _name = name; return this; }
    public CountryBuilder WithIso3(string iso3) { _iso3 = iso3; return this; }

    /// <summary>
    /// Creates or retrieves a country by ISO-2 code (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.Countries
            .FirstOrDefaultAsync(c => c.Iso2Code == _iso2 && !c.IsDeleted);
        if (existing != null) return existing.Id;

        var country = new Country
        {
            Name = _name,
            Iso2Code = _iso2,
            Iso3Code = _iso3 ?? _iso2 + "X",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        return country.Id;
    }
}

#endregion

#region SDG

public sealed class SdgBuilder
{
    private string _sdgId = "1";
    private string _name = "No Poverty";

    public SdgBuilder WithSdgId(string sdgId) { _sdgId = sdgId; return this; }
    public SdgBuilder WithName(string name) { _name = name; return this; }

    /// <summary>
    /// Creates or retrieves an SDG by SDGId (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.SDGs
            .FirstOrDefaultAsync(s => s.SDGId == _sdgId && !s.IsDeleted);
        if (existing != null) return existing.Id;

        var sdg = new SDG
        {
            SDGId = _sdgId,
            SDGNumber = _sdgId,
            Name = _name,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.SDGs.Add(sdg);
        await context.SaveChangesAsync();
        return sdg.Id;
    }
}

#endregion

#region OrganizationHierarchy

public sealed class OrgHierarchyBuilder
{
    private string _name = "Test Org Unit";
    private string _code = "TOU";
    private string _description = "Test org unit";
    private OrganizationUnitType _type = OrganizationUnitType.OrgUnit;
    private int? _parentId;
    private bool _isDeleted;

    public OrgHierarchyBuilder WithName(string name) { _name = name; return this; }
    public OrgHierarchyBuilder WithCode(string code) { _code = code; return this; }
    public OrgHierarchyBuilder WithDescription(string desc) { _description = desc; return this; }
    public OrgHierarchyBuilder WithType(OrganizationUnitType type) { _type = type; return this; }
    public OrgHierarchyBuilder WithParent(int parentId) { _parentId = parentId; return this; }
    public OrgHierarchyBuilder Deleted() { _isDeleted = true; return this; }

    /// <summary>
    /// Creates or retrieves an org hierarchy node by code (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.OrganizationHierarchies
            .FirstOrDefaultAsync(o => o.Code == _code && !o.IsDeleted);
        if (existing != null) return existing.Id;

        var org = new OrganizationHierarchy
        {
            Name = _name,
            Code = _code,
            Description = _description,
            Type = _type,
            ParentId = _parentId,
            Status = EntityStatus.Active,
            IsDeleted = _isDeleted
        };
        context.OrganizationHierarchies.Add(org);
        await context.SaveChangesAsync();
        return org.Id;
    }

    /// <summary>
    /// Always creates a new org hierarchy node (ignores existing records with same code).
    /// Use when you need multiple nodes with unique codes.
    /// </summary>
    public async Task<int> CreateAsync(AppDbContext context)
    {
        var org = new OrganizationHierarchy
        {
            Name = _name,
            Code = _code,
            Description = _description,
            Type = _type,
            ParentId = _parentId,
            Status = EntityStatus.Active,
            IsDeleted = _isDeleted
        };
        context.OrganizationHierarchies.Add(org);
        await context.SaveChangesAsync();
        return org.Id;
    }
}

#endregion

#region Contact

public sealed class ContactBuilder
{
    private string _firstName = "Test";
    private string _lastName = "Contact";
    private string _email = "test.contact@example.org";
    private string _title = "Mr";
    private int _partnerId;
    private int? _createdBy;
    private bool _isDeleted;

    public ContactBuilder WithFirstName(string name) { _firstName = name; return this; }
    public ContactBuilder WithLastName(string name) { _lastName = name; return this; }
    public ContactBuilder WithEmail(string email) { _email = email; return this; }
    public ContactBuilder WithTitle(string title) { _title = title; return this; }
    public ContactBuilder WithPartner(int partnerId) { _partnerId = partnerId; return this; }
    public ContactBuilder WithCreatedBy(int userId) { _createdBy = userId; return this; }
    public ContactBuilder Deleted() { _isDeleted = true; return this; }

    public async Task<int> BuildAsync(AppDbContext context)
    {
        var userId = _createdBy ?? 0;
        var contact = new UNOPSContact
        {
            FirstName = _firstName,
            LastName = _lastName,
            Email = _email,
            Title = _title,
            Name = $"{_firstName} {_lastName}",
            PartnerId = _partnerId,
            Status = EntityStatus.Active,
            IsDeleted = _isDeleted,
            CreatedBy = userId,
            LastModifiedBy = userId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();
        return contact.Id;
    }
}

#endregion

#region Interaction

public sealed class InteractionBuilder
{
    private string _name = "Test Interaction";
    private string _subject = "Test Subject";
    private InteractionType _type = InteractionType.Email;
    private DateTime _date = DateTime.UtcNow;
    private int? _createdBy;
    private string? _gmailThreadId;
    private string? _gmailMessageId;
    private bool _isDeleted;

    public InteractionBuilder WithName(string name) { _name = name; return this; }
    public InteractionBuilder WithSubject(string subject) { _subject = subject; return this; }
    public InteractionBuilder WithType(InteractionType type) { _type = type; return this; }
    public InteractionBuilder WithDate(DateTime date) { _date = date; return this; }
    public InteractionBuilder WithCreatedBy(int userId) { _createdBy = userId; return this; }
    public InteractionBuilder WithGmailThread(string threadId) { _gmailThreadId = threadId; return this; }
    public InteractionBuilder WithGmailMessage(string messageId) { _gmailMessageId = messageId; return this; }
    public InteractionBuilder Deleted() { _isDeleted = true; return this; }

    public async Task<int> BuildAsync(AppDbContext context)
    {
        var userId = _createdBy ?? 0;
        var interaction = new UNOPSInteraction
        {
            Name = _name,
            Subject = _subject,
            Type = _type,
            Date = _date,
            Status = EntityStatus.Active,
            IsDeleted = _isDeleted,
            GmailThreadId = _gmailThreadId,
            GmailMessageId = _gmailMessageId,
            CreatedBy = userId,
            LastModifiedBy = userId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        context.Interactions.Add(interaction);
        await context.SaveChangesAsync();
        return interaction.Id;
    }
}

#endregion

#region EntityRole

public sealed class EntityRoleBuilder
{
    private string _name = "Opportunity Manager";
    private string _code = "Opportunity_Manager_Opportunity";
    private string _entityType = "Opportunity";
    private string _description = "Manages the opportunity";
    private bool _isInternal = true;
    private bool _allowsMultiple;

    public EntityRoleBuilder WithName(string name) { _name = name; return this; }
    public EntityRoleBuilder WithCode(string code) { _code = code; return this; }
    public EntityRoleBuilder WithEntityType(string type) { _entityType = type; return this; }
    public EntityRoleBuilder WithDescription(string desc) { _description = desc; return this; }
    public EntityRoleBuilder Internal(bool isInternal = true) { _isInternal = isInternal; return this; }
    public EntityRoleBuilder AllowsMultiple(bool value = true) { _allowsMultiple = value; return this; }

    /// <summary>
    /// Creates or retrieves an entity role by code (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.EntityRoles
            .FirstOrDefaultAsync(r => r.Code == _code && !r.IsDeleted);
        if (existing != null) return existing.Id;

        var role = new EntityRole
        {
            Name = _name,
            Code = _code,
            EntityType = _entityType,
            Description = _description,
            IsInternal = _isInternal,
            AllowsMultiple = _allowsMultiple,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.EntityRoles.Add(role);
        await context.SaveChangesAsync();
        return role.Id;
    }
}

#endregion

#region ProposedInitiativeType

public sealed class InitiativeTypeBuilder
{
    private string _name = "Project";

    public InitiativeTypeBuilder WithName(string name) { _name = name; return this; }

    /// <summary>
    /// Creates or retrieves a proposed initiative type by name (get-or-create pattern).
    /// </summary>
    public async Task<int> BuildAsync(AppDbContext context)
    {
        var existing = await context.Set<ProposedInitiativeType>()
            .FirstOrDefaultAsync(p => p.Name == _name && !p.IsDeleted);
        if (existing != null) return existing.Id;

        var type = new ProposedInitiativeType
        {
            Name = _name,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.Set<ProposedInitiativeType>().Add(type);
        await context.SaveChangesAsync();
        return type.Id;
    }
}

#endregion

#region Output

public sealed class OutputBuilder
{
    private string _name = "Test Output";
    private string _level0 = "Test Level";

    public OutputBuilder WithName(string name) { _name = name; return this; }
    public OutputBuilder WithLevel0(string level) { _level0 = level; return this; }

    public async Task<int> BuildAsync(AppDbContext context)
    {
        var output = new Output
        {
            Name = _name,
            Level0 = _level0,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        context.Set<Output>().Add(output);
        await context.SaveChangesAsync();
        return output.Id;
    }
}

#endregion
