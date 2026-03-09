using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.ExternalDataAndIntegration;

/// <summary>
/// Base fixture for External Data & Gmail Integration tests.
/// Provides DbContext, Mapper, and helpers for SDG, UNCF, Country, Gmail models.
/// No shared transaction is used to prevent cascading 25P02 errors when
/// individual tests trigger PostgreSQL errors (defect-exposing tests).
/// </summary>
public abstract class ExternalDataAndIntegrationFixtureBase : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected readonly IMapper Mapper;
    protected readonly IConfiguration Configuration;
    protected readonly string TestMarker = $"EDI_{Guid.NewGuid():N}";

    protected ExternalDataAndIntegrationFixtureBase()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"ExtDataInt_{Guid.NewGuid():N}");
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        var mockAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(Context);

        if (TestEnvironment.UsePostgreSQL)
        {
            try
            {
                Context.Database.ExecuteSqlRaw(
                    @"TRUNCATE TABLE ""InteractionContacts"", ""InteractionPartners"", ""InteractionUsers"", ""Interactions"", ""Contacts"", ""Partners"", ""SDGs"", ""Countries"", ""UNCFIndicators"", ""UNCFOutcomes"", ""UNCFMetadatas"" CASCADE");
            }
            catch
            {
                Context.Set<InteractionContact>().RemoveRange(Context.Set<InteractionContact>());
                Context.Set<InteractionPartner>().RemoveRange(Context.Set<InteractionPartner>());
                Context.Set<InteractionUser>().RemoveRange(Context.Set<InteractionUser>());
                Context.Set<UNOPSInteraction>().RemoveRange(Context.Set<UNOPSInteraction>());
                Context.Set<UNOPSContact>().RemoveRange(Context.Set<UNOPSContact>());
                Context.Set<UNOPSPartner>().RemoveRange(Context.Set<UNOPSPartner>());
                Context.SDGs.RemoveRange(Context.SDGs);
                Context.Countries.RemoveRange(Context.Countries);
                Context.UNCFIndicators.RemoveRange(Context.UNCFIndicators);
                Context.UNCFOutcomes.RemoveRange(Context.UNCFOutcomes);
                Context.UNCFMetadatas.RemoveRange(Context.UNCFMetadatas);
                Context.SaveChanges();
            }
        }

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["IsUNOPSOverride"] = "true"
            })
            .Build();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            cfg.ConstructServicesUsing(serviceType =>
            {
                if (serviceType == typeof(UNOPS.PAO.Business.Mapping.EntityArtifactValueResolver))
                    return Activator.CreateInstance(serviceType, Configuration)!;
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        Mapper = config.CreateMapper();
    }

    protected async Task<int> SeedSDGAsync(string sdgId, string name, EntityStatus status = EntityStatus.Active)
    {
        var sdg = Context.SDGs.FirstOrDefault(s => s.SDGId == sdgId && !s.IsDeleted);
        if (sdg != null) return sdg.Id;
        sdg = new SDG
        {
            SDGId = sdgId,
            SDGNumber = sdgId,
            Name = name,
            Status = status,
            IsDeleted = false
        };
        Context.SDGs.Add(sdg);
        await Context.SaveChangesAsync();
        return sdg.Id;
    }

    protected async Task<int> SeedCountryAsync(string name, string iso2Code, string? iso3Code = null)
    {
        var country = Context.Countries.FirstOrDefault(c => c.Iso2Code == iso2Code && !c.IsDeleted);
        if (country != null) return country.Id;
        country = new Country
        {
            Name = name,
            Iso2Code = iso2Code,
            Iso3Code = iso3Code ?? iso2Code,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.Countries.Add(country);
        await Context.SaveChangesAsync();
        return country.Id;
    }

    protected async Task<int> SeedUNCFOutcomeAsync(string country, int versionNo, string outcomeId)
    {
        // UNCFOutcomes join with UNCFMetadata - must seed both
        var metadata = new UNCFMetadata
        {
            Name = $"{country} v{versionNo}",
            Country = country,
            UNCooperationFrameworkVersionNo = versionNo,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.UNCFMetadatas.Add(metadata);
        await Context.SaveChangesAsync();

        var outcome = new UNCFOutcome
        {
            Country = country,
            UNCooperationFrameworkVersionNo = versionNo,
            UNCFOutcomeId = outcomeId,
            Name = $"Outcome {outcomeId}",
            Status = EntityStatus.Active,
            IsDeleted = false
        };
        Context.UNCFOutcomes.Add(outcome);
        await Context.SaveChangesAsync();
        return outcome.Id;
    }

    protected async Task<int> SeedPartnerAsync(string name)
    {
        var partner = new UNOPSPartner
        {
            Name = name,
            PartnerShortDescription = name,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Partners.Add(partner);
        await Context.SaveChangesAsync();
        return partner.Id;
    }

    protected async Task<int> SeedContactAsync(int partnerId, string email, string firstName, string lastName)
    {
        var partner = await Context.Partners.FindAsync(partnerId);
        var contact = new UNOPSContact
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Title = "Mr",
            Name = $"{firstName} {lastName}",
            PartnerId = partnerId,
            Partner = partner!,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Contacts.Add(contact);
        await Context.SaveChangesAsync();
        return contact.Id;
    }

    protected async Task<int> SeedInteractionAsync(string? gmailThreadId = null, string? gmailMessageId = null)
    {
        var interaction = new UNOPSInteraction
        {
            Name = $"Interaction {TestMarker}",
            Subject = $"Subject {TestMarker}",
            Date = DateTime.UtcNow,
            Type = InteractionType.Email,
            Status = EntityStatus.Active,
            IsDeleted = false,
            GmailThreadId = gmailThreadId,
            GmailMessageId = gmailMessageId,
            CreatedBy = 1,
            LastModifiedBy = 1,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        Context.Interactions.Add(interaction);
        await Context.SaveChangesAsync();
        return interaction.Id;
    }

    protected static ClaimsPrincipal CreatePrincipal(int userId = 1)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, "test@unops.org")
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    public virtual void Dispose()
    {
        Context.Dispose();
    }
}
