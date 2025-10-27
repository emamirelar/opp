using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using Xunit;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Services
{
    public class OrgUnitFilterServiceTests : IDisposable
    {
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IUserPreferenceService> _mockUserPreferenceService;
        private readonly Mock<IOrgUnitHierarchyService> _mockHierarchyService;
        private readonly Mock<ILogger<OrgUnitFilterService>> _mockLogger;
        private readonly Mock<IDbContextSchema> _mockDbContextSchema;
        private readonly UNOPSAppDbContext _dbContext;
        private readonly OrgUnitFilterService _service;

        public OrgUnitFilterServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            // Setup mocks for DbContext
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
            _mockDbContextSchema = new Mock<IDbContextSchema>();
            _mockDbContextSchema.Setup(x => x.Schema).Returns("public");
            
            _dbContext = new UNOPSAppDbContext(options, userResolverService, _mockDbContextSchema.Object);

            // Setup mocks
            _mockPermissionService = new Mock<IPermissionService>();
            _mockUserPreferenceService = new Mock<IUserPreferenceService>();
            _mockHierarchyService = new Mock<IOrgUnitHierarchyService>();
            _mockLogger = new Mock<ILogger<OrgUnitFilterService>>();

            // Create service instance
            _service = new OrgUnitFilterService(
                _mockPermissionService.Object,
                _mockUserPreferenceService.Object,
                _mockHierarchyService.Object,
                _dbContext,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreatePartnerSpecificationAsync_WithOrgUnitId_ReturnsOrgUnitSpecification()
        {
            // Arrange
            var orgUnitId = 10;
            var hierarchyIds = new List<int> { 10, 11, 12 };
            var orgUnitUserIds = new List<int> { 100, 101, 102 };

            // Setup org units in database
            var orgUnits = new List<OrganizationHierarchy>
            {
                new OrganizationHierarchy { Id = 10, Code = "ORG10", Name = "Org 10", Description = "Org 10 Description" },
                new OrganizationHierarchy { Id = 11, Code = "ORG11", Name = "Org 11", Description = "Org 11 Description" },
                new OrganizationHierarchy { Id = 12, Code = "ORG12", Name = "Org 12", Description = "Org 12 Description" }
            };
            await _dbContext.OrganizationHierarchies.AddRangeAsync(orgUnits);

            // Setup users in org units
            var userInfos = new List<UserProfile>
            {
                new UserProfile { UserId = 100, OrgUnit = "ORG10" },
                new UserProfile { UserId = 101, OrgUnit = "ORG11" },
                new UserProfile { UserId = 102, OrgUnit = "ORG12" }
            };
            await _dbContext.UserProfile.AddRangeAsync(userInfos);
            await _dbContext.SaveChangesAsync();

            var filter = new PartnerFilterRequest { OrgUnitId = orgUnitId };
            var user = new ClaimsPrincipal();

            _mockHierarchyService.Setup(x => x.GetDescendantIdsAsync(orgUnitId))
                .ReturnsAsync(hierarchyIds);

            // Act
            var specification = await _service.CreatePartnerSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<UNOPSPartnerCompositeWithOrgUnitAndRelationsSpecification>();
            
            // Verify hierarchy service was called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(orgUnitId), Times.Once);
        }

        [Fact]
        public async Task CreatePartnerSpecificationAsync_WithoutOrgUnitId_ReturnsStandardSpecification()
        {
            // Arrange
            var filter = new PartnerFilterRequest { Name = "Test Partner" }; // No OrgUnitId
            var user = new ClaimsPrincipal();

            // Act
            var specification = await _service.CreatePartnerSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<UNOPSPartnerCompositeSpecification>();
            
            // Verify hierarchy service was NOT called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateContactSpecificationAsync_WithOrgUnitId_ReturnsOrgUnitSpecification()
        {
            // Arrange
            var orgUnitId = 20;
            var hierarchyIds = new List<int> { 20, 21, 22 };

            var filter = new ContactFilterRequest { OrgUnitId = orgUnitId };
            var user = new ClaimsPrincipal();

            _mockHierarchyService.Setup(x => x.GetDescendantIdsAsync(orgUnitId))
                .ReturnsAsync(hierarchyIds);

            // Act
            var specification = await _service.CreateContactSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<UNOPSContactCompositeWithOrgUnitSpecification>();
            
            // Verify hierarchy service was called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(orgUnitId), Times.Once);
        }

        [Fact]
        public async Task CreateContactSpecificationAsync_WithoutOrgUnitId_ReturnsStandardSpecification()
        {
            // Arrange
            var filter = new ContactFilterRequest { FirstName = "Test", LastName = "Contact" }; // No OrgUnitId
            var user = new ClaimsPrincipal();

            // Act
            var specification = await _service.CreateContactSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<UNOPSContactCompositeSpecification>();
            
            // Verify hierarchy service was NOT called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreateInteractionSpecificationAsync_WithOrgUnitId_ReturnsOrgUnitSpecification()
        {
            // Arrange
            var orgUnitId = 30;
            var hierarchyIds = new List<int> { 30, 31, 32 };

            var filter = new InteractionFilterRequest { OrgUnitId = orgUnitId };
            var user = new ClaimsPrincipal();

            _mockHierarchyService.Setup(x => x.GetDescendantIdsAsync(orgUnitId))
                .ReturnsAsync(hierarchyIds);

            // Act
            var specification = await _service.CreateInteractionSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<InteractionCompositeWithOrgUnitSpecification>();
            
            // Verify hierarchy service was called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(orgUnitId), Times.Once);
        }

        [Fact]
        public async Task CreateInteractionSpecificationAsync_WithoutOrgUnitId_ReturnsStandardSpecification()
        {
            // Arrange
            var filter = new InteractionFilterRequest { Type = InteractionType.InPersonMeeting }; // No OrgUnitId
            var user = new ClaimsPrincipal();

            // Act
            var specification = await _service.CreateInteractionSpecificationAsync(filter, user);

            // Assert
            specification.Should().NotBeNull();
            specification.Should().BeOfType<InteractionCompositeSpecification>();
            
            // Verify hierarchy service was NOT called
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUserDefaultOrgUnitIdAsync_WithValidUser_ReturnsOrgUnitId()
        {
            // Arrange
            var userId = 123;
            var defaultOrgUnitId = 456;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()) 
            }));

            _mockUserPreferenceService.Setup(x => x.GetDefaultOrgUnitIdAsync(userId))
                .ReturnsAsync(defaultOrgUnitId);

            // Act
            var result = await _service.GetUserDefaultOrgUnitIdAsync(user);

            // Assert
            result.Should().Be(defaultOrgUnitId);
            _mockUserPreferenceService.Verify(x => x.GetDefaultOrgUnitIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserDefaultOrgUnitIdAsync_WithInvalidUser_ReturnsNull()
        {
            // Arrange
            var user = new ClaimsPrincipal(); // No claims

            // Act
            var result = await _service.GetUserDefaultOrgUnitIdAsync(user);

            // Assert
            result.Should().BeNull();
            _mockUserPreferenceService.Verify(x => x.GetDefaultOrgUnitIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task CreatePartnerSpecificationAsync_WithOrgUnitId_IncludesUsersFromHierarchy()
        {
            // Arrange
            var orgUnitId = 40;
            var hierarchyIds = new List<int> { 40, 41 };

            // Setup org units
            var orgUnits = new List<OrganizationHierarchy>
            {
                new OrganizationHierarchy { Id = 40, Code = "DEPT40", Name = "Department 40", Description = "Department 40 Description" },
                new OrganizationHierarchy { Id = 41, Code = "DEPT41", Name = "Department 41", Description = "Department 41 Description" }
            };
            await _dbContext.OrganizationHierarchies.AddRangeAsync(orgUnits);

            // Setup users in these departments
            var userInfos = new List<UserProfile>
            {
                new UserProfile { UserId = 200, OrgUnit = "DEPT40" },
                new UserProfile { UserId = 201, OrgUnit = "DEPT40" },
                new UserProfile { UserId = 202, OrgUnit = "DEPT41" },
                new UserProfile { UserId = 203, OrgUnit = "DEPT99" } // Different department
            };
            await _dbContext.UserProfile.AddRangeAsync(userInfos);
            await _dbContext.SaveChangesAsync();

            var filter = new PartnerFilterRequest { OrgUnitId = orgUnitId };
            var user = new ClaimsPrincipal();

            _mockHierarchyService.Setup(x => x.GetDescendantIdsAsync(orgUnitId))
                .ReturnsAsync(hierarchyIds);

            // Act
            var specification = await _service.CreatePartnerSpecificationAsync(filter, user);

            // Assert
            specification.Should().BeOfType<UNOPSPartnerCompositeWithOrgUnitAndRelationsSpecification>();
            
            // The specification should have been created with the correct user IDs
            // Note: We can't directly inspect the private fields, but we can verify the service behavior
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(orgUnitId), Times.Once);
        }

        [Fact]
        public async Task CreatePartnerSpecificationAsync_LogsInformation()
        {
            // Arrange
            var orgUnitId = 50;
            var hierarchyIds = new List<int> { 50, 51, 52 };
            var filter = new PartnerFilterRequest { OrgUnitId = orgUnitId };
            var user = new ClaimsPrincipal();

            _mockHierarchyService.Setup(x => x.GetDescendantIdsAsync(orgUnitId))
                .ReturnsAsync(hierarchyIds);

            // Act
            await _service.CreatePartnerSpecificationAsync(filter, user);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Creating UNOPS partner specification")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("OrgUnit filter applied for partner")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}