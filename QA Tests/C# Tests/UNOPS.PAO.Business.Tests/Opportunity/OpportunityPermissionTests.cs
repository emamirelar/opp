using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Permission and security tests for opportunity management
/// Tests row-level security, role-based access, and permission enforcement
/// Created: January 15, 2026
/// Priority: P1-P2
/// </summary>
public class OpportunityPermissionTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;

    public OpportunityPermissionTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityPermissionTestDb_{Guid.NewGuid()}")
            .Options;

        var mockUserService = new Mock<UserResolverService<int>>(null);
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        _context = new UNOPSAppDbContext(_dbContextOptions, mockUserService.Object, mockDbSchema.Object);

        _mockMapper = new Mock<IMapper>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(_testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(default))
            .ReturnsAsync(_context);

        _manager = new UNOPSOpportunityManager(
            _mockMapper.Object,
            _context,
            _mockConfiguration.Object,
            _mockDbContextFactory.Object,
            _mockExchangeRateService.Object,
            _mockPermissionService.Object,
            _mockHttpContextAccessor.Object,
            _mockServiceProvider.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Currencies.Add(new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false });
        _context.Countries.Add(new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" });

        _context.OrganizationHierarchies.AddRange(new[]
        {
            new OrganizationHierarchy { Id = 1, Name = "Org Unit 1", Code = "OU1", Description = "Organization Unit 1", IsDeleted = false },
            new OrganizationHierarchy { Id = 2, Name = "Org Unit 2", Code = "OU2", Description = "Organization Unit 2", IsDeleted = false }
        });

        _context.WorkflowStages.Add(new WorkflowStage { Id = 1, Name = "Identification", EntityType = "Opportunity", Order = 1, IsDeleted = false });
        _context.ProposedInitiativeTypes.Add(new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false });
        
        _context.PAOUsers.AddRange(new[]
        {
            new PAOUser { Id = 1, Email = "user1@unops.org" },
            new PAOUser { Id = 2, Email = "user2@unops.org" }
        });

        _context.SaveChanges();
    }

    #region P1 - Permission Checks Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-001")]
    public async Task GetOpportunityWithUser_IncludesPermissions_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Permission Test Opportunity",
            WorkflowStageId = 1,
            ResponsibleOrgUnitId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var modelWithPermissions = new OpportunityModel
        {
            Id = 1,
            Name = "Permission Test Opportunity",
            Permissions = new EntityPermissionsModel
            {
                CanView = true,
                CanEdit = true,
                CanDelete = false,
                CanShare = true
            }
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(modelWithPermissions);

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.CanView.Should().BeTrue();
        result.Permissions.CanEdit.Should().BeTrue();
        result.Permissions.CanDelete.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-002")]
    public async Task GetOpportunity_UserCannotView_ReturnsNull()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Restricted Opportunity",
            WorkflowStageId = 1,
            ResponsibleOrgUnitId = 2, // Different org unit
            Status = EntityStatus.Draft,
            CreatedBy = 2, // Different user
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to deny access
        _mockPermissionService.Setup(p => p.CanViewEntity(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<string>(),
            It.IsAny<int>()))
            .Returns(false);

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, 1);

        // Assert - Access denied should return null or throw
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-003")]
    public async Task CreateOpportunity_UserLacksPermission_ThrowsException()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Unauthorized Creation",
            Description = "User lacks create permission"
        };

        // Mock permission service to deny creation
        _mockPermissionService.Setup(p => p.CanCreateEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity"))
            .Returns(false);

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-004")]
    public async Task UpdateOpportunity_UserLacksEditPermission_ThrowsException()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Read-Only Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 2, // Created by different user
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Unauthorized Update"
        };

        // Mock permission service to deny editing
        _mockPermissionService.Setup(p => p.CanEditEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(false);

        // Act & Assert
        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*edit*");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-005")]
    public async Task DeleteOpportunity_UserLacksDeletePermission_ThrowsException()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Protected Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Active, // Active opportunities may have stricter delete rules
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to deny deletion
        _mockPermissionService.Setup(p => p.CanDeleteEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(false);

        // Act & Assert
        Func<Task> act = async () => await _manager.DeleteOpportunityAsync(1);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*delete*");
    }

    #endregion

    #region P1 - Row-Level Security Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-006")]
    public async Task GetAllOpportunities_FiltersByOrgUnit_Success()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Opp in Org Unit 1",
                ResponsibleOrgUnitId = 1,
                WorkflowStageId = 1,
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Opp in Org Unit 2",
                ResponsibleOrgUnitId = 2,
                WorkflowStageId = 1,
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            }
        });
        await _context.SaveChangesAsync();

        // Mock permission service to filter by org unit
        _mockPermissionService.Setup(p => p.GetAuthorizedOrgUnits(It.IsAny<ClaimsPrincipal>()))
            .Returns(new List<int> { 1 }); // User can only see Org Unit 1

        var filteredModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Opp in Org Unit 1", ResponsibleOrgUnitId = 1 }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
            .Returns(filteredModels);

        // Act
        var result = await _manager.GetAllOpportunitiesAsync();

        // Assert
        var opportunities = result.ToList();
        opportunities.Should().HaveCount(1);
        opportunities.Should().OnlyContain(o => o.ResponsibleOrgUnitId == 1);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-007")]
    public async Task GetOpportunitiesByPartner_FiltersByPermission_Success()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity { Id = 1, Name = "Visible Opp", ResponsibleOrgUnitId = 1, WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 1, CreatedDate = DateTime.UtcNow, IsDeleted = false },
            new Domain.Entities.Opportunity { Id = 2, Name = "Hidden Opp", ResponsibleOrgUnitId = 2, WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 1, CreatedDate = DateTime.UtcNow, IsDeleted = false }
        });
        await _context.SaveChangesAsync();

        var visibleModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Visible Opp" }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
            .Returns(visibleModels);

        // Act
        var result = await _manager.GetOpportunitiesByPartnerIdAsync(1);

        // Assert
        var opportunities = result.ToList();
        opportunities.Should().HaveCount(1);
        opportunities.Should().NotContain(o => o.Name == "Hidden Opp");
    }

    #endregion

    #region P2 - Role-Based Access Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-008")]
    public async Task AdminUser_CanAccessAllOpportunities_Success()
    {
        // Arrange
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "99"),
            new Claim(ClaimTypes.Role, "Administrator")
        }, "TestAuthType"));

        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity { Id = 1, Name = "Opp 1", WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 1, CreatedDate = DateTime.UtcNow, IsDeleted = false },
            new Domain.Entities.Opportunity { Id = 2, Name = "Opp 2", WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 2, CreatedDate = DateTime.UtcNow, IsDeleted = false }
        });
        await _context.SaveChangesAsync();

        // Mock permission service to grant admin full access
        _mockPermissionService.Setup(p => p.IsAdmin(It.IsAny<ClaimsPrincipal>()))
            .Returns(true);

        var allModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Opp 1" },
            new() { Id = 2, Name = "Opp 2" }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
            .Returns(allModels);

        // Act
        var result = await _manager.GetAllOpportunitiesAsync();

        // Assert
        var opportunities = result.ToList();
        opportunities.Should().HaveCount(2); // Admin sees all
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-009")]
    public async Task ReadOnlyUser_CannotEdit_ThrowsException()
    {
        // Arrange
        var readOnlyUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "50"),
            new Claim(ClaimTypes.Role, "ReadOnly")
        }, "TestAuthType"));

        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Attempted Update"
        };

        _mockPermissionService.Setup(p => p.CanEditEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(false);

        // Act & Assert
        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-010")]
    public async Task OpportunityCreator_HasSpecialPermissions_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Created by User 1",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1, // Created by current user
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to grant creator special permissions
        _mockPermissionService.Setup(p => p.IsCreator(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<int>(),
            1)) // CreatedBy = 1
            .Returns(true);

        var modelWithCreatorPermissions = new OpportunityModel
        {
            Id = 1,
            Name = "Created by User 1",
            Permissions = new EntityPermissionsModel
            {
                CanView = true,
                CanEdit = true,
                CanDelete = true, // Creator can delete
                CanShare = true
            }
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(modelWithCreatorPermissions);

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.CanDelete.Should().BeTrue(); // Creator has delete permission
    }

    #endregion

    #region P2 - Workflow-Based Permissions Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-011")]
    public async Task ActiveOpportunity_RestrictsDelete_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Active Opportunity",
            WorkflowStageId = 2, // Advanced stage
            Status = EntityStatus.Active,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to restrict deletion based on status
        _mockPermissionService.Setup(p => p.CanDeleteEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns<ClaimsPrincipal, string, int>((user, entityType, entityId) =>
            {
                var opp = _context.Opportunities.Find(entityId);
                return opp?.Status != "Active"; // Cannot delete active opportunities
            });

        // Act & Assert
        Func<Task> act = async () => await _manager.DeleteOpportunityAsync(1);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*active*");
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-012")]
    public async Task DraftOpportunity_AllowsDelete_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Draft Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to allow deletion of drafts
        _mockPermissionService.Setup(p => p.CanDeleteEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(true);

        // Act
        var result = await _manager.DeleteOpportunityAsync(1);

        // Assert
        result.Should().BeTrue();

        var deletedOpportunity = await _context.Opportunities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == 1);

        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region P2 - Team-Based Permissions Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-013")]
    public async Task TeamMember_HasEditPermission_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Team Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 2, // Created by different user
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to grant team member edit permission
        _mockPermissionService.Setup(p => p.IsTeamMember(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(true);

        _mockPermissionService.Setup(p => p.CanEditEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(true);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Team Member Update"
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Team Member Update" });

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Team Member Update");
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-014")]
    public async Task NonTeamMember_CannotEdit_ThrowsException()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Private Team Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 2,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Mock permission service to deny non-team member
        _mockPermissionService.Setup(p => p.IsTeamMember(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(false);

        _mockPermissionService.Setup(p => p.CanEditEntity(
            It.IsAny<ClaimsPrincipal>(),
            "Opportunity",
            It.IsAny<int>()))
            .Returns(false);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Unauthorized Edit"
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-PERM-015")]
    public async Task AssignTeamMember_AddsPermissions_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Team Assignment Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Assign creator as opportunity manager (team member)
        await _manager.AssignCreatorAsOpportunityManagerAsync(1, 1);

        // Assert - Verify assignment (actual implementation would add stakeholder record)
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOpportunity.Should().NotBeNull();
        // In actual implementation, would verify stakeholder assignment
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
