/// <summary>
/// Base fixture for Offices Feature tests (PNO-1213, PNO-1214).
/// Provides OrganizationHierarchyManager (office hierarchy), seeded org units, opportunities, partners.
/// </summary>

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.OfficesFeature;

public abstract class OfficesFeatureFixtureBase : IDisposable
{
    protected readonly DbContextOptions<UNOPSAppDbContext> DbContextOptions;
    protected readonly UNOPSAppDbContext Context;
    protected IDbContextTransaction? Transaction;
    protected readonly string TestMarker = $"OF_{Guid.NewGuid():N}";
    protected int RootOrgId;
    protected int ChildOrgId1;
    protected int ChildOrgId2;
    protected int OpportunityId;
    protected int PartnerId;
    protected int PaoUserId;
    protected readonly IMapper Mapper;
    protected readonly IOrganizationHierarchyManager OrgHierarchyManager;

    protected OfficesFeatureFixtureBase()
    {
        DbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OfficesFeature_{Guid.NewGuid()}");
        var mockDbSchema = new Moq.Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        var tempAccessor = CreateMockHttpContextAccessor("0");
        var tempResolver = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(tempAccessor.Object, null);
        using var tempCtx = TestDbContextFactory.CreateUNOPS(DbContextOptions, tempResolver, mockDbSchema.Object);
        PaoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "offices-test@unops.org");

        var mainAccessor = CreateMockHttpContextAccessor(PaoUserId.ToString());
        var userResolverService = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(mainAccessor.Object, null);
        Context = TestDbContextFactory.CreateUNOPS(DbContextOptions, userResolverService, mockDbSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            Transaction = Context.Database.BeginTransaction();
        }

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()));
        Mapper = mapperConfig.CreateMapper();

        var valuesRepository = new ValuesRepository(Context);
        OrgHierarchyManager = new OrganizationHierarchyManager(valuesRepository, Mapper);

        SeedTestData();
    }

    protected static Moq.Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor> CreateMockHttpContextAccessor(string userId = "0")
    {
        var accessor = new Moq.Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var identity = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
        }, "Test");
        accessor.Setup(a => a.HttpContext).Returns(new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new System.Security.Claims.ClaimsPrincipal(identity)
        });
        return accessor;
    }

    protected void SeedTestData()
    {
        var root = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "OF_ROOT" && !o.IsDeleted);
        if (root == null)
        {
            root = new OrganizationHierarchy
            {
                Name = $"Office Root {TestMarker}",
                Code = "OF_ROOT",
                Description = "Root office for tests",
                Type = OrganizationUnitType.Office,
                ParentId = null,
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.OrganizationHierarchies.Add(root);
            Context.SaveChanges();
        }
        RootOrgId = root.Id;

        var child1 = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "OF_CH1" && !o.IsDeleted);
        if (child1 == null)
        {
            child1 = new OrganizationHierarchy
            {
                Name = $"Office Child 1 {TestMarker}",
                Code = "OF_CH1",
                Description = "Child office 1",
                Type = OrganizationUnitType.Office,
                ParentId = RootOrgId,
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.OrganizationHierarchies.Add(child1);
            Context.SaveChanges();
        }
        ChildOrgId1 = child1.Id;

        var child2 = Context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "OF_CH2" && !o.IsDeleted);
        if (child2 == null)
        {
            child2 = new OrganizationHierarchy
            {
                Name = $"Office Child 2 {TestMarker}",
                Code = "OF_CH2",
                Description = "Child office 2",
                Type = OrganizationUnitType.Office,
                ParentId = RootOrgId,
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.OrganizationHierarchies.Add(child2);
            Context.SaveChanges();
        }
        ChildOrgId2 = child2.Id;

        var opportunity = Context.Opportunities.FirstOrDefault(o => o.Name != null && o.Name.Contains(TestMarker) && !o.IsDeleted);
        if (opportunity == null)
        {
            opportunity = new UNOPS.PAO.Domain.Entities.Opportunity
            {
                Name = $"Opp for Office {TestMarker}",
                Description = "Test",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Draft,
                ResponsibleOrgUnitId = ChildOrgId1,
                CreatedBy = PaoUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = PaoUserId,
                LastModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };
            Context.Opportunities.Add(opportunity);
            Context.SaveChanges();
        }
        OpportunityId = opportunity.Id;

        var partner = Context.Partners.FirstOrDefault(p => p.Name != null && p.Name.Contains(TestMarker) && !p.IsDeleted);
        if (partner == null)
        {
            partner = new UNOPSPartner
            {
                Name = $"Partner for Office {TestMarker}",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.Partners.Add(partner);
            Context.SaveChanges();

            var orgRel = new OrganizationUnitRelationship
            {
                EntityId = partner.Id,
                EntityType = nameof(Partner),
                OrganizationHierarchyId = ChildOrgId1,
                Name = $"Partner-{partner.Id}-OrgUnit-{ChildOrgId1}",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            Context.OrganizationUnitRelationships.Add(orgRel);
            Context.SaveChanges();
        }
        PartnerId = partner.Id;

        Context.ChangeTracker.Clear();
    }

    public virtual void Dispose() => Transaction?.Dispose();
}
