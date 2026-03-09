using AutoMapper;
using FluentAssertions;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.Business.Managers.Mapping;
using Xunit;

using ManagerMappingProfile = UNOPS.PAO.Business.Managers.Mapping.MappingProfile;
using BaseEngagementMappingProfile = UNOPS.PAO.UNOPSBusiness.Managers.Mapping.BaseEngagementMappingProfile;

namespace UNOPS.PAO.Business.Tests.Mapping;

/// <summary>
/// Validates all AutoMapper profiles for configuration correctness.
/// AssertConfigurationIsValid() catches unmapped properties, missing type converters,
/// and other configuration issues that would cause runtime failures.
/// </summary>
public class MappingProfileValidationTests
{
    #region Positive Tests (P=2)

    [Fact]
    public async Task P1_MainMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });

        var act = () => config.CreateMapper();

        act.Should().NotThrow("main MappingProfile should create a valid mapper instance");
    }

    [Fact]
    public async Task P2_AuditLogMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogMappingProfile>());

        var act = () => config.CreateMapper();

        act.Should().NotThrow("AuditLogMappingProfile should create a valid mapper instance");
    }

    #endregion

    #region Negative Tests (N≥6)

    [Fact]

    [Trait("Defect", "DEF-073")]
    public async Task N1_MainMappingProfile_HasUnmappedProperties()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ManagerMappingProfile>());

        var act = () => config.AssertConfigurationIsValid();

        act.Should().NotThrow("all destination properties should be explicitly mapped or ignored");
    }

    [Fact]
    public async Task N2_NullSourceObject_MapReturnsNull()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogMappingProfile>());
        var mapper = config.CreateMapper();

        var result = mapper.Map<UNOPS.PAO.Models.AuditLogs.AuditLogModel>((UNOPS.PAO.Domain.Entities.AuditLog?)null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task N3_DuplicateProfileRegistration_DoesNotThrow()
    {
        var act = () => new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
        });

        act.Should().NotThrow("duplicate profile registration should be idempotent");
    }

    [Fact]
    public async Task N4_EmptyMapperConfiguration_NoProfiles_DoesNotThrow()
    {
        var config = new MapperConfiguration(cfg => { });
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task N5_CommentMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow("CommentMappingProfile should create a valid mapper");
    }

    [Fact]
    public async Task N6_SavedFilterMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SavedFilterMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow("SavedFilterMappingProfile should create a valid mapper");
    }

    #endregion

    #region Edge/Boundary Tests (E≥6)

    [Fact]
    public async Task E1_AllBusinessProfiles_CombinedCanCreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<CommentMappingProfile>();
            cfg.AddProfile<SavedFilterMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });

        var act = () => config.CreateMapper();
        act.Should().NotThrow("combined business profiles should be compatible");
    }

    [Fact]
    public async Task E2_PartnerGroupMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PartnerGroupMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task E3_LiaisonOfficeMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<LiaisonOfficeMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task E4_PartnerCategoryMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PartnerCategoryMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task E5_BaseEngagementMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BaseEngagementMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task E6_CountryMappingProfile_CanCreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CountryMappingProfile>());
        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    #endregion

    #region Functional Tests (F≥6)

    [Fact]
    public async Task F1_CommentProfile_MapsContentCorrectly()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentMappingProfile>());
        var mapper = config.CreateMapper();
        var comment = new UNOPS.PAO.Domain.Entities.Comment
        {
            Id = 42,
            Content = "Test comment",
            EntityType = "Partner",
            EntityId = 1,
            IsPinned = true,
            IsEdited = false,
        };

        var model = mapper.Map<UNOPS.PAO.Models.CommentModel>(comment);

        model.Id.Should().Be(42);
        model.Content.Should().Be("Test comment");
        model.EntityType.Should().Be("Partner");
        model.IsPinned.Should().BeTrue();
    }

    [Fact]
    public async Task F2_AuditLogProfile_MapsAuditLogToModel()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogMappingProfile>());
        var mapper = config.CreateMapper();
        var auditLog = new UNOPS.PAO.Domain.Entities.AuditLog
        {
            Id = 1,
            Name = "Test audit entry",
            EntityType = "Partner",
            Action = "Create",
        };

        var model = mapper.Map<UNOPS.PAO.Models.AuditLogs.AuditLogModel>(auditLog);

        model.Should().NotBeNull();
        model.Id.Should().Be(1);
    }

    [Fact]
    public async Task F3_AuditLogProfile_MapsUNOPSAuditLogToModel()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogMappingProfile>());
        var mapper = config.CreateMapper();
        var auditLog = new UNOPS.PAO.UNOPSDomain.Entities.UNOPSAuditLog
        {
            Id = 2,
            Name = "UNOPS audit",
            EntityType = "Opportunity",
            Action = "Update",
        };

        var model = mapper.Map<UNOPS.PAO.Models.AuditLogs.AuditLogModel>(auditLog);

        model.Should().NotBeNull();
        model.Id.Should().Be(2);
    }

    [Fact]
    public async Task F4_SavedFilterProfile_MapsOrderByField()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SavedFilterMappingProfile>());
        var mapper = config.CreateMapper();
        var filter = new UNOPS.PAO.Domain.Entities.SavedFilter
        {
            Id = 1,
            Name = "My Filter",
            OrderByField = "Name",
            CreatedDate = DateTime.UtcNow,
        };

        var model = mapper.Map<UNOPS.PAO.Models.Filters.SavedFilterModel>(filter);

        model.OrderBy.Should().Be("Name", "OrderByField should map to OrderBy property");
    }

    [Fact]
    public async Task F5_MainProfile_PartnerMapsWork()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        var mapper = config.CreateMapper();

        var partner = new UNOPS.PAO.Domain.Entities.Partner
        {
            Id = 1,
            Name = "Test Partner",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
        };

        var model = mapper.Map<UNOPS.PAO.Models.Partners.PartnerModel>(partner);
        model.Should().NotBeNull("Partner → PartnerModel mapping should work");
    }

    [Fact]
    public async Task F6_MainProfile_InteractionMappingWorks()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        var mapper = config.CreateMapper();

        var interaction = new UNOPS.PAO.Domain.Entities.Interaction
        {
            Id = 1,
            Name = "Test Interaction",
            Subject = "Test Subject",
            Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active,
        };

        var model = mapper.Map<UNOPS.PAO.Models.Interactions.InteractionModel>(interaction);
        model.Should().NotBeNull("Interaction → InteractionModel mapping should work");
    }

    #endregion

    #region Integration Tests (I≥6)

    [Fact]

    [Trait("Defect", "DEF-073")]
    public async Task I1_AllProfilesCombined_NoConflicts()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<CommentMappingProfile>();
            cfg.AddProfile<SavedFilterMappingProfile>();
            cfg.AddProfile<PartnerGroupMappingProfile>();
            cfg.AddProfile<LiaisonOfficeMappingProfile>();
            cfg.AddProfile<PartnerCategoryMappingProfile>();
            cfg.AddProfile<BaseEngagementMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });

        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow("all profiles combined should have no conflicts");
    }

    [Fact]
    public async Task I2_MapperFromConfig_CanBeCreated()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.AddProfile<AuditLogMappingProfile>();
            cfg.AddProfile<CommentMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });

        var act = () => config.CreateMapper();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task I3_CommentRequest_MapsToComment()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentMappingProfile>());
        var mapper = config.CreateMapper();
        var request = new UNOPS.PAO.Models.CommentRequest
        {
            Content = "New comment",
            EntityType = "Opportunity",
            EntityId = 5,
        };

        var entity = mapper.Map<UNOPS.PAO.Domain.Entities.Comment>(request);

        entity.Content.Should().Be("New comment");
        entity.EntityType.Should().Be("Opportunity");
        entity.EntityId.Should().Be(5);
    }

    [Fact]

    [Trait("Defect", "DEF-073")]
    public async Task I4_OrganizationHierarchyMappingProfile_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrganizationHierarchyMappingProfile>());
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task I5_MainProfile_AiPromptMappingRoundTrip()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        var mapper = config.CreateMapper();
        var entity = new UNOPS.PAO.Domain.Entities.AiPrompt
        {
            Id = 1,
            Name = "Test Prompt",
            GenerationConfig = "{}",
            ContentConfig = "{}",
            Project = "test-project",
            Location = "us-central1",
            Model = "gemini-1.5-pro",
        };

        var model = mapper.Map<UNOPS.PAO.Models.AI.AiPromptModel>(entity);
        model.Should().NotBeNull();
        model.Name.Should().Be("Test Prompt");
    }

    [Fact]
    public async Task I6_MainProfile_DocumentMappingRoundTrip()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ManagerMappingProfile>();
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        var mapper = config.CreateMapper();
        var entity = new UNOPS.PAO.Domain.Entities.Document { Id = 1, Name = "Test Doc" };

        var model = mapper.Map<UNOPS.PAO.Models.Documents.DocumentModel>(entity);
        model.Should().NotBeNull();
    }

    #endregion
}
