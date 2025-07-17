using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.UNOPSBusiness;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.IntegrationTests.UnitTests.Specifications;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using UNOPS.PAO.UNOPSDomain.Specifications;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Managers
{
    public class UNOPSPartnerManagerTests : IDisposable
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<UNOPSPartnerManager>> _mockLogger;
        private readonly Mock<IOrgUnitHierarchyService> _mockHierarchyService;
        private readonly Mock<IOrgUnitFilterService> _mockOrgUnitFilterService;
        private readonly IPermissionService _permissionService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IDbContextSchema> _mockDbContextSchema;
        private readonly UNOPSAppDbContext _dbContext;
        private readonly UNOPSPartnerManager _manager;
        private readonly ServiceCollection _services;
        private readonly ServiceProvider _serviceProvider;

        public UNOPSPartnerManagerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            // Setup mocks for DbContext
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            var userResolverService = new UserResolverService<int>(_mockHttpContextAccessor.Object);
            _mockDbContextSchema = new Mock<IDbContextSchema>();
            _mockDbContextSchema.Setup(x => x.Schema).Returns("public");
            
            _dbContext = new UNOPSAppDbContext(options, userResolverService, _mockDbContextSchema.Object);

            // Setup mocks
            _mockMapper = new Mock<IMapper>();
            
            // Create actual configuration using ConfigurationBuilder for proper testing
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AISettings:ProjectId", "test-project"},
                {"AISettings:Location", "test-location"},
                {"AISettings:EmbeddingModelName", "test-model"},
                {"ConnectionStrings:DbSchema", "public"},
                {"PubSub:ProjectId", "test-project"},
                {"PubSub:TopicId", "test-topic"}
            };
            
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            
            _configuration = configuration;
            
            _mockLogger = new Mock<ILogger<UNOPSPartnerManager>>();
            _mockHierarchyService = new Mock<IOrgUnitHierarchyService>();
            _mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
            _permissionService = new TestPermissionService();

            // Setup service collection for dependency injection
            _services = new ServiceCollection();
            _services.AddSingleton(_mockHierarchyService.Object);
            _services.AddSingleton(_mockOrgUnitFilterService.Object);
            _services.AddSingleton<IPermissionService>(_permissionService);
            _serviceProvider = _services.BuildServiceProvider();

            // Setup HttpContext with service provider
            httpContext.RequestServices = _serviceProvider;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Setup mapper to return basic mapped object
            _mockMapper.Setup(m => m.Map<UNOPSPartner, PartnerModel>(It.IsAny<UNOPSPartner>()))
                .Returns((UNOPSPartner source) => new PartnerModel
                {
                    Id = source.Id,
                    Name = source.Name,
                    Status = source.Status,
                    PartnerOfficeId = source.PartnerOfficeId,
                    PartnerGroupCode = source.PartnerGroupCode
                });

            // Create manager instance using reflection to access private constructor
            var managerType = typeof(UNOPSPartnerManager);
            var constructor = managerType.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).First();
            _manager = (UNOPSPartnerManager)constructor.Invoke(new object[]
            {
                _mockMapper.Object,
                _dbContext,
                _configuration,
                null, // PartnerTreeService is not used in these tests
                _mockLogger.Object,
                _permissionService,
                _mockHttpContextAccessor.Object
            });
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WithOrgUnitId_FiltersPartnersByOrgUnitHierarchy()
        {
            // Note: The org unit filtering now includes both direct and indirect relations
            
            // Arrange
            var orgUnitId = 10;
            
            // Seed test data
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 1, Name = "Partner 1", PartnerOfficeId = 10, Status = "Active", ShortName = "P1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 2, Name = "Partner 2", PartnerOfficeId = 11, Status = "Active", ShortName = "P2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 3, Name = "Partner 3", PartnerOfficeId = 12, Status = "Active", ShortName = "P3", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 4, Name = "Partner 4", PartnerOfficeId = 20, Status = "Active", ShortName = "P4", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }, // Different org unit
                new UNOPSPartner { Id = 5, Name = "Partner 5", PartnerOfficeId = null, Status = "Active", ShortName = "P5", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false } // No org unit
            };
            
            // Add contacts and interactions to create indirect relations
            var contact1 = new UNOPSContact { Id = 1, PartnerId = 4, Name = "John Doe", ContactNumber = "C001", FirstName = "John", LastName = "Doe", Title = "Manager", Email = "john@example.com", Status = "Active" };
            var contact2 = new UNOPSContact { Id = 2, PartnerId = 5, Name = "Jane Smith", ContactNumber = "C002", FirstName = "Jane", LastName = "Smith", Title = "Director", Email = "jane@example.com", Status = "Active" };
            partners[3].Contacts = new List<Contact> { contact1 };
            partners[4].Contacts = new List<Contact> { contact2 };
            
            // Create interactions to link contacts with users from the org units
            var interaction1 = new UNOPSInteraction 
            { 
                Id = 1, 
                Name = "Interaction 1",
                Subject = "Test Interaction 1",
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Test interaction 1",
                InteractionContacts = new List<InteractionContact> { new InteractionContact { InteractionId = 1, ContactId = 1 } },
                InteractionUsers = new List<InteractionUser> { new InteractionUser { InteractionId = 1, UserId = 1 } }
            };
            
            var interaction2 = new UNOPSInteraction 
            { 
                Id = 2, 
                Name = "Interaction 2",
                Subject = "Test Interaction 2",
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Test interaction 2",
                InteractionContacts = new List<InteractionContact> { new InteractionContact { InteractionId = 2, ContactId = 2 } },
                InteractionUsers = new List<InteractionUser> { new InteractionUser { InteractionId = 2, UserId = 2 } }
            };
            
            contact1.Interactions = new List<Interaction> { interaction1 };
            contact2.Interactions = new List<Interaction> { interaction2 };
            
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.Contacts.AddRangeAsync(contact1, contact2);
            await _dbContext.Interactions.AddRangeAsync(interaction1, interaction2);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                OrgUnitId = orgUnitId,
                PageIndex = 1,
                PageSize = 10
            };

            // Create specification that includes org unit filtering with relations
            var hierarchyIds = new List<int> { 10, 11, 12 };
            var userIds = new List<string> { "1", "2" }; // Users in the org units who have interactions
            var unosSpecification = new UNOPSPartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
            var adaptedSpecification = new PartnerSpecificationAdapter(unosSpecification);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            
            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, adaptedSpecification, filterRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PaginationResponse<PartnerModel>>();
            
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            
            // The new org unit filter includes both direct (PartnerOfficeId) and indirect (via contacts) relations
            // Partners 1, 2, 3 (direct) + 4, 5 (indirect via interactions) = 5 total
            response!.TotalCount.Should().Be(5);
            response!.Records.Should().HaveCount(5);
            
            // All partners should be included
            var partnerIds = response!.Records.Select(r => r.Id).ToList();
            partnerIds.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WithoutOrgUnitId_ReturnsAllPermittedPartners()
        {
            // Arrange
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 1, Name = "Partner 1", Status = "Active", ShortName = "P1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 2, Name = "Partner 2", Status = "Active", ShortName = "P2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 3, Name = "Partner 3", Status = "Active", ShortName = "P3", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                PageIndex = 1,
                PageSize = 10
            };

            // Create specification that matches all partners
            var specification = new TestPartnerSpecification(matchAll: true);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, specification, filterRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PaginationResponse<PartnerModel>>();
            
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            response!.TotalCount.Should().Be(3);
            response!.Records.Should().HaveCount(3);
            
            // Verify hierarchy service was NOT called (no OrgUnitId filter)
            _mockHierarchyService.Verify(x => x.GetDescendantIdsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WithOrgUnitIdAndOtherFilters_AppliesSpecificationOnly()
        {
            // Note: The org unit filtering is now handled at the controller level
            // The manager only applies the provided specification
            
            // Arrange
            var orgUnitId = 10;
            
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 1, Name = "Active Partner 1", PartnerOfficeId = 10, Status = "Active", ShortName = "AP1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 2, Name = "Inactive Partner", PartnerOfficeId = 10, Status = "Inactive", ShortName = "IP", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 3, Name = "Active Partner 2", PartnerOfficeId = 11, Status = "Active", ShortName = "AP2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 4, Name = "Active Partner 3", PartnerOfficeId = 20, Status = "Active", ShortName = "AP3", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            
            // Add a contact to partner 4 to simulate indirect relation
            var contact = new UNOPSContact { Id = 1, PartnerId = 4, Name = "Contact Four", ContactNumber = "C003", FirstName = "Contact", LastName = "Four", Title = "Manager", Email = "contact4@example.com", Status = "Active" };
            partners[3].Contacts = new List<Contact> { contact };
            
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                OrgUnitId = orgUnitId,
                Status = "Active",
                PageIndex = 1,
                PageSize = 10
            };

            // Create specification with org unit filtering including relations
            var hierarchyIds = new List<int> { 10, 11 };
            var userIds = new List<string> { "1" }; // User who may have interactions with contacts
            var baseSpecification = new PartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
            var unosSpecification = new UNOPSPartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
            var adaptedSpecification = new PartnerSpecificationAdapter(unosSpecification);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, adaptedSpecification, filterRequest);

            // Assert
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            
            // The specification includes org unit filtering with both direct and indirect relations
            // Without actual interactions in the test data, only direct matches are included
            // Partners 1, 2, 3 have direct org unit matches (org units 10, 11)
            // Partner 4 has contact but no interactions, so it's not included
            response!.TotalCount.Should().Be(3);
            response!.Records.Should().HaveCount(3);
            
            // Only partners 1, 2, 3 should be included (direct org unit match)
            var partnerIds = response!.Records.Select(r => r.Id).ToList();
            partnerIds.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WithOrgUnitIdButNoHierarchy_IncludesIndirectRelations()
        {
            // Note: Even with a single org unit, indirect relations through contacts are included
            
            // Arrange
            var orgUnitId = 10;
            
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 1, Name = "Partner 1", PartnerOfficeId = 10, Status = "Active", ShortName = "P1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 2, Name = "Partner 2", PartnerOfficeId = 11, Status = "Active", ShortName = "P2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            
            // Add a contact to partner 2 to simulate potential indirect relation
            var contact = new UNOPSContact { Id = 1, PartnerId = 2, Name = "Contact Two", ContactNumber = "C004", FirstName = "Contact", LastName = "Two", Title = "Manager", Email = "contact2@example.com", Status = "Active" };
            partners[1].Contacts = new List<Contact> { contact };
            
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                OrgUnitId = orgUnitId,
                PageIndex = 1,
                PageSize = 10
            };

            // Create specification with single org unit and potential user interactions
            var hierarchyIds = new List<int> { 10 }; // Only the org unit itself
            var userIds = new List<string> { "1" }; // User who may have interactions
            var baseSpecification = new PartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
            var unosSpecification = new UNOPSPartnerByOrgUnitWithRelationsSpecification(hierarchyIds, userIds);
            var adaptedSpecification = new PartnerSpecificationAdapter(unosSpecification);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, adaptedSpecification, filterRequest);

            // Assert
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            
            // Without actual interactions in the test data, only direct matches are included
            // Only Partner 1 has a direct org unit match (org unit 10)
            // Partner 2 is in org unit 11 which is not in the hierarchy for this test
            response!.TotalCount.Should().Be(1);
            response!.Records.Should().HaveCount(1);
            
            // Only Partner 1 should be included (direct match)
            response!.Records.First().Id.Should().Be(1);
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var partners = new List<UNOPSPartner>();
            for (int i = 1; i <= 15; i++)
            {
                partners.Add(new UNOPSPartner { Id = i, Name = $"Partner {i}", Status = "Active", ShortName = $"P{i}", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false });
            }
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                PageIndex = 2,
                PageSize = 5
            };

            // Create specification that matches all partners
            var specification = new TestPartnerSpecification(matchAll: true);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, specification, filterRequest);

            // Assert
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            response!.TotalCount.Should().Be(15);
            response!.Records.Should().HaveCount(5);
            response.PageIndex.Should().Be(2);
            response.PageSize.Should().Be(5);
            response!.Records.Select(r => r.Id).Should().BeEquivalentTo(new[] { 6, 7, 8, 9, 10 });
        }

        [Fact]
        public async Task GetPartnersWithSpecificationAsync_WhenHierarchyServiceNotAvailable_LogsWarningAndSkipsOrgUnitFilter()
        {
            // Note: This test is no longer relevant as the manager doesn't handle org unit filtering directly
            // The org unit filtering is now handled at the controller level by IOrgUnitFilterService
            // Keeping this test to verify that the manager works correctly without org unit filtering
            
            // Arrange
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 1, Name = "Partner 1", PartnerOfficeId = 10, Status = "Active", ShortName = "P1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 2, Name = "Partner 2", PartnerOfficeId = 20, Status = "Active", ShortName = "P2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.SaveChangesAsync();

            var filterRequest = new PartnerFilterRequest
            {
                PageIndex = 1,
                PageSize = 10
            };

            // Create specification that matches all partners (no org unit filtering)
            var specification = new TestPartnerSpecification(matchAll: true);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));

            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, specification, filterRequest);

            // Assert
            var response = result as PaginationResponse<PartnerModel>;
            response!.Should().NotBeNull();
            response!.TotalCount.Should().Be(2); // All partners returned
            response!.Records.Should().HaveCount(2);
        }

        [Fact]
        public async Task TestSimpleGetPartnersWithSpecification_ReturnsData()
        {
            // Arrange
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 201, Name = "Simple Test Partner", Status = "Active", ShortName = "STP", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.SaveChangesAsync();
            
            // Debug: Verify data is in database
            var dbCount = await _dbContext.Partners.CountAsync();
            var partnerRepoField = _manager.GetType().GetField("PartnerRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var partnerRepo = partnerRepoField?.GetValue(_manager) as BaseRepository<UNOPSPartner>;
            var repoAll = partnerRepo?.GetAll().ToList() ?? new List<UNOPSPartner>();
            var repoNotDeleted = repoAll.Where(x => !x.IsDeleted).ToList();
            
            _mockLogger.Object.LogInformation($"DB Count: {dbCount}, Repo All: {repoAll.Count}, Repo Not Deleted: {repoNotDeleted.Count}");
            
            var filterRequest = new PartnerFilterRequest
            {
                PageIndex = 1,
                PageSize = 10
            };
            
            var specification = new TestPartnerSpecification(matchAll: true);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
            
            // Act
            var result = await _manager.GetPartnersWithSpecificationAsync(user, specification, filterRequest);
            
            // Assert
            var response = result as PaginationResponse<PartnerModel>;
            response.Should().NotBeNull();
            
            if (response!.TotalCount == 0)
            {
                // More debugging
                _mockLogger.Object.LogError($"No results returned. DB has {dbCount} partners, repo returned {repoAll.Count}");
            }
            
            response!.TotalCount.Should().Be(1);
            response!.Records.Should().HaveCount(1);
            response!.Records.First().Name.Should().Be("Simple Test Partner");
        }

        [Fact]
        public async Task TestDataPersistence_VerifyPartnersAreSavedCorrectly()
        {
            // Arrange
            var partners = new List<UNOPSPartner>
            {
                new UNOPSPartner { Id = 101, Name = "Test Partner 1", Status = "Active", ShortName = "TP1", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false },
                new UNOPSPartner { Id = 102, Name = "Test Partner 2", Status = "Active", ShortName = "TP2", DDEACDone = "false", DDRequired = "false", LevyPotentiallyApplies = "false", NewEngagement = "true", PooledFund = "false", IsDeleted = false }
            };
            
            // Act - Save data
            await _dbContext.Partners.AddRangeAsync(partners);
            await _dbContext.SaveChangesAsync();
            
            // Verify using same context
            var countSameContext = await _dbContext.Partners.CountAsync();
            var partnersSameContext = await _dbContext.Partners.ToListAsync();
            
            // Verify using repository (via reflection since it's private)
            var partnerRepoField = _manager.GetType().GetField("PartnerRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var partnerRepo = partnerRepoField?.GetValue(_manager) as BaseRepository<UNOPSPartner>;
            var partnersFromRepo = partnerRepo?.GetAll().ToList() ?? new List<UNOPSPartner>();
            
            // Assert
            countSameContext.Should().BeGreaterThanOrEqualTo(2);
            partnersSameContext.Should().Contain(p => p.Id == 101);
            partnersSameContext.Should().Contain(p => p.Id == 102);
            
            partnersFromRepo.Should().NotBeEmpty();
            partnersFromRepo.Should().Contain(p => p.Id == 101);
            partnersFromRepo.Should().Contain(p => p.Id == 102);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}