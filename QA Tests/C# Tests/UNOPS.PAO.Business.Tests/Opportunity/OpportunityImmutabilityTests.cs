using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Helpers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Tests for opportunity immutability enforcement after Go/No-Go/Cancelled decisions.
/// Verifies that opportunities in terminal stages (GO, NO GO, CANCELLED) cannot be modified.
/// Also verifies that the Reopen workflow correctly makes opportunities editable again.
/// Created: February 2, 2026
/// Related PRD: The Go Decision PRD (US-6, FR-6)
/// </summary>
public class OpportunityImmutabilityTests : IDisposable
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

    public OpportunityImmutabilityTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityImmutabilityTestDb_{Guid.NewGuid()}")
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
        _mockExchangeRateService = new Mock<IExchangeRateService>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User")
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
        _context.Countries.Add(new Country { Id = 1, Name = "Test Country", Iso2Code = "TC" });
        _context.OrganizationHierarchies.Add(new OrganizationHierarchy { Id = 1, Name = "Test Org Unit", Code = "TOU", Description = "Test Organization Unit", IsDeleted = false });
        _context.ProposedInitiativeTypes.Add(new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false });
        _context.PAOUsers.Add(new PAOUser { Id = 1, Email = "test@unops.org" });
        _context.SaveChanges();
    }

    private async Task<int> CreateOpportunityWithStage(string stage)
    {
        var opportunity = new Domain.Entities.Opportunity
        {
            Name = $"Test Opportunity - {stage}",
            Description = $"Test opportunity in {stage} stage for immutability testing",
            Stage = stage,
            Status = EntityStatus.Active,
            ResponsibleOrgUnitId = 1,
            IsDeleted = false,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        return opportunity.Id;
    }

    #region GO Stage Immutability Tests

    [Fact]
    public async Task UpdateOpportunityAsync_ThrowsBusinessException_WhenOpportunityIsInGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("GO");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = opportunityId,
            Name = "Updated Name"
        };

        // Act
        var act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    [Fact]
    public async Task UpdateOverviewSectionAsync_ThrowsBusinessException_WhenOpportunityIsInGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("GO");

        var request = new OverviewSectionRequest
        {
            Name = "Updated Name"
        };

        // Act
        var act = async () => await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    [Fact]
    public async Task DeleteOpportunityAsync_ThrowsBusinessException_WhenOpportunityIsInGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("GO");

        // Act
        var act = async () => await _manager.DeleteOpportunityAsync(opportunityId);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    #endregion

    #region NO GO Stage Immutability Tests

    [Fact]
    public async Task UpdateOpportunityAsync_ThrowsBusinessException_WhenOpportunityIsInNoGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("NO GO");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = opportunityId,
            Name = "Updated Name"
        };

        // Act
        var act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    [Fact]
    public async Task UpdateWhatSectionAsync_ThrowsBusinessException_WhenOpportunityIsInNoGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("NO GO");

        var request = new WhatSectionRequest
        {
            Description = "Updated Description"
        };

        // Act
        var act = async () => await _manager.UpdateWhatSectionAsync(opportunityId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    #endregion

    #region CANCELLED Stage Immutability Tests

    [Fact]
    public async Task UpdateOpportunityAsync_ThrowsBusinessException_WhenOpportunityIsInCancelledStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("CANCELLED");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = opportunityId,
            Name = "Updated Name"
        };

        // Act
        var act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    [Fact]
    public async Task UpdateTeamSectionAsync_ThrowsBusinessException_WhenOpportunityIsInCancelledStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("CANCELLED");

        var request = new TeamSectionRequest
        {
            ResponsibleOrgUnitId = 1
        };

        // Act
        var act = async () => await _manager.UpdateTeamSectionAsync(opportunityId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    #endregion

    #region Non-Immutable Stage Tests (Should Allow Updates)

    [Fact]
    public async Task UpdateOverviewSectionAsync_Succeeds_WhenOpportunityIsInIdentifyAndProfileStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("IDENTIFY & PROFILE");

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = opportunityId, Name = "Updated Name" });

        var request = new OverviewSectionRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOverviewSectionAsync_Succeeds_WhenOpportunityIsInDraftStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("Draft");

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = opportunityId, Name = "Updated Name" });

        var request = new OverviewSectionRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOverviewSectionAsync_Succeeds_WhenOpportunityIsInSendForGoDecisionStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("SEND FOR GO DECISION");

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = opportunityId, Name = "Updated Name" });

        var request = new OverviewSectionRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Reopen Workflow Tests (Immutability Lifted)

    [Fact]
    public async Task UpdateOverviewSectionAsync_Succeeds_WhenReopenedFromNoGoToIdentifyAndProfile()
    {
        // Arrange - Create opportunity in NO GO stage, then simulate reopen
        var opportunityId = await CreateOpportunityWithStage("NO GO");
        
        // Simulate reopen by updating stage directly (this would be done by workflow)
        var opportunity = await _context.Opportunities.FindAsync(opportunityId);
        opportunity!.Stage = "IDENTIFY & PROFILE";
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = opportunityId, Name = "Reopened Opportunity" });

        var request = new OverviewSectionRequest
        {
            Name = "Reopened Opportunity"
        };

        // Act - Should succeed because opportunity is no longer in immutable stage
        var result = await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOverviewSectionAsync_Succeeds_WhenReopenedFromCancelledToIdentifyAndProfile()
    {
        // Arrange - Create opportunity in CANCELLED stage, then simulate reopen
        var opportunityId = await CreateOpportunityWithStage("CANCELLED");
        
        // Simulate reopen by updating stage directly (this would be done by workflow)
        var opportunity = await _context.Opportunities.FindAsync(opportunityId);
        opportunity!.Stage = "IDENTIFY & PROFILE";
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = opportunityId, Name = "Reopened Opportunity" });

        var request = new OverviewSectionRequest
        {
            Name = "Reopened Opportunity"
        };

        // Act - Should succeed because opportunity is no longer in immutable stage
        var result = await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Permission Endpoint Tests

    [Fact]
    public async Task GetOpportunityAsync_WithUser_ReturnsIsImmutableTrue_WhenOpportunityIsInGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("GO");
        
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel 
            { 
                Id = opportunityId, 
                Name = "GO Opportunity",
                Stage = "GO",
                Permissions = new EntityPermissionsModel
                {
                    CanRead = true,
                    CanUpdate = true, // Will be overridden by immutability check
                    CanDelete = true  // Will be overridden by immutability check
                }
            });

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, opportunityId);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.IsImmutable.Should().BeTrue();
        result.Permissions.CanUpdate.Should().BeFalse();
        result.Permissions.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GetOpportunityAsync_WithUser_ReturnsIsImmutableTrue_WhenOpportunityIsInNoGoStage()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("NO GO");
        
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel 
            { 
                Id = opportunityId, 
                Name = "NO GO Opportunity",
                Stage = "NO GO",
                Permissions = new EntityPermissionsModel
                {
                    CanRead = true,
                    CanUpdate = true,
                    CanDelete = true
                }
            });

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, opportunityId);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        result.Permissions!.IsImmutable.Should().BeTrue();
        result.Permissions.CanUpdate.Should().BeFalse();
        result.Permissions.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task GetOpportunityAsync_WithUser_ReturnsIsImmutableNullOrFalse_WhenOpportunityIsEditable()
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage("IDENTIFY & PROFILE");
        
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel 
            { 
                Id = opportunityId, 
                Name = "Editable Opportunity",
                Stage = "IDENTIFY & PROFILE",
                Permissions = new EntityPermissionsModel
                {
                    CanRead = true,
                    CanUpdate = true,
                    CanDelete = true
                }
            });

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, opportunityId);

        // Assert
        result.Should().NotBeNull();
        result!.Permissions.Should().NotBeNull();
        // IsImmutable should be null or false for editable opportunities
        (result.Permissions!.IsImmutable == null || result.Permissions.IsImmutable == false).Should().BeTrue();
    }

    #endregion

    #region Case Sensitivity Tests

    [Theory]
    [InlineData("GO")]
    [InlineData("go")]
    [InlineData("Go")]
    [InlineData("NO GO")]
    [InlineData("no go")]
    [InlineData("No Go")]
    [InlineData("CANCELLED")]
    [InlineData("cancelled")]
    [InlineData("Cancelled")]
    public async Task UpdateOverviewSectionAsync_ThrowsBusinessException_ForImmutableStagesCaseInsensitive(string stage)
    {
        // Arrange
        var opportunityId = await CreateOpportunityWithStage(stage);

        var request = new OverviewSectionRequest
        {
            Name = "Updated Name"
        };

        // Act
        var act = async () => await _manager.UpdateOverviewSectionAsync(opportunityId, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*decision*");
    }

    #endregion

    #region EntityPermissionsModel IsImmutable Property Tests

    [Fact]
    public void EntityPermissionsModel_IsImmutableProperty_DefaultsToNull()
    {
        // Arrange & Act
        var permissions = new EntityPermissionsModel();

        // Assert
        permissions.IsImmutable.Should().BeNull();
    }

    [Fact]
    public void EntityPermissionsModel_IsImmutableProperty_CanBeSetToTrue()
    {
        // Arrange
        var permissions = new EntityPermissionsModel();

        // Act
        permissions.IsImmutable = true;

        // Assert
        permissions.IsImmutable.Should().BeTrue();
    }

    [Fact]
    public void EntityPermissionsModel_IsImmutableProperty_CanBeSetToFalse()
    {
        // Arrange
        var permissions = new EntityPermissionsModel();

        // Act
        permissions.IsImmutable = false;

        // Assert
        permissions.IsImmutable.Should().BeFalse();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
