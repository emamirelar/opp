using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Managers
{
    /// <summary>
    /// Test suite for OpportunityManager
    /// Tests CRUD operations, lifecycle management, AI suggestions, and conversions
    /// </summary>
    public class OpportunityManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly OpportunityManager _manager;

        public OpportunityManagerTests()
        {
            // Setup in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase(databaseName: $"OpportunityTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);

            // Setup mocks
            _mockMapper = new Mock<IMapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Setup HttpContext with user
            var mockHttpContext = new Mock<HttpContext>();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Test User")
            }));
            mockHttpContext.Setup(m => m.User).Returns(mockUser);
            _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

            // Initialize manager
            _manager = new OpportunityManager(
                _mockMapper.Object,
                _context,
                _mockConfiguration.Object,
                _mockPermissionService.Object,
                _mockHttpContextAccessor.Object
            );

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed Countries
            _context.Countries.AddRange(new[]
            {
                new Country { Id = 1, Name = "Bangladesh", Code = "BD" },
                new Country { Id = 2, Name = "Nepal", Code = "NP" },
                new Country { Id = 3, Name = "Myanmar", Code = "MM" }
            });

            // Seed Organizational Units
            _context.OrganizationUnits.AddRange(new[]
            {
                new OrganizationUnit { Id = 1, Name = "South Asia Hub", Code = "SAH" },
                new OrganizationUnit { Id = 2, Name = "Bangladesh Office", Code = "BDO", ParentId = 1 }
            });

            // Seed Currencies
            _context.Currencies.AddRange(new[]
            {
                new Currency { Id = 1, Code = "USD", Name = "US Dollar" },
                new Currency { Id = 2, Code = "EUR", Name = "Euro" }
            });

            _context.SaveChanges();
        }

        #region CRUD Operations Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-001")]
        public async Task CreateOpportunity_WithRequiredFields_Success()
        {
            // Arrange
            var request = new OpportunityCreateRequest
            {
                Name = "Water Infrastructure Initiative - South Asia",
                Description = "Multi-country water infrastructure project",
                OpportunityType = "Project",
                EstimatedValue = 2500000.00m,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = request.Name,
                Description = request.Description,
                OpportunityType = request.OpportunityType,
                EstimatedValue = request.EstimatedValue,
                CurrencyId = request.CurrencyId,
                PrimaryCountryId = request.PrimaryCountryId,
                ResponsibleOrgUnitId = request.ResponsibleOrgUnitId,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityCreateRequest>()))
                .Returns(opportunity);
            _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
                .Returns(new OpportunityModel { Id = 1, Name = request.Name });

            // Act
            var result = await _manager.CreateOpportunityAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal("Draft", opportunity.Status);
            Assert.Equal(1, opportunity.CreatedBy);
            Assert.True(opportunity.CreatedDate <= DateTime.UtcNow);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-F-002")]
        public async Task CreateOpportunity_MissingRequiredFields_ThrowsException()
        {
            // Arrange
            var request = new OpportunityCreateRequest
            {
                // Missing Name
                Description = "Test opportunity",
                OpportunityType = "Project"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.CreateOpportunityAsync(request));
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-003")]
        public async Task GetOpportunityById_ValidId_ReturnsOpportunity()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Description = "Test Description",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
                .Returns(new OpportunityModel { Id = 1, Name = "Test Opportunity" });

            // Act
            var result = await _manager.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Opportunity", result.Name);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-F-004")]
        public async Task GetOpportunityById_NonExistentId_ReturnsNull()
        {
            // Act
            var result = await _manager.GetByIdAsync(999999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-005")]
        public async Task UpdateOpportunity_BasicFields_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Original Name",
                Description = "Original Description",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var updateRequest = new OpportunityUpdateRequest
            {
                Id = 1,
                Name = "Updated Name",
                Description = "Updated Description",
                EstimatedValue = 1500000
            };

            _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
                .Returns(new OpportunityModel { Id = 1, Name = "Updated Name" });

            // Act
            var result = await _manager.UpdateAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            var updatedEntity = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Updated Name", updatedEntity.Name);
            Assert.Equal("Updated Description", updatedEntity.Description);
            Assert.Equal(1500000, updatedEntity.EstimatedValue);
            Assert.Equal(1, updatedEntity.LastModifiedBy);
            Assert.True(updatedEntity.LastModifiedDate.HasValue);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Concurrency")]
        [Trait("TestId", "TC-OPP-OM-F-006")]
        public async Task UpdateOpportunity_ConcurrencyConflict_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft",
                RowVersion = new byte[] { 1, 2, 3, 4 },
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Simulate concurrent update
            opportunity.Name = "Updated by User A";
            opportunity.RowVersion = new byte[] { 5, 6, 7, 8 };
            await _context.SaveChangesAsync();

            // User B tries to update with old row version
            var updateRequest = new OpportunityUpdateRequest
            {
                Id = 1,
                Name = "Updated by User B",
                RowVersion = new byte[] { 1, 2, 3, 4 } // Old version
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
                await _manager.UpdateAsync(updateRequest));
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-007")]
        public async Task DeleteOpportunity_SoftDelete_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            await _manager.DeleteAsync(1);

            // Assert
            var deletedEntity = await _context.Opportunities.FindAsync(1);
            Assert.True(deletedEntity.IsDeleted);
            Assert.Equal(1, deletedEntity.DeletedBy);
            Assert.True(deletedEntity.DeletedDate.HasValue);

            // Verify not returned in normal queries
            var activeOpportunities = await _context.Opportunities
                .Where(o => !o.IsDeleted)
                .ToListAsync();
            Assert.DoesNotContain(activeOpportunities, o => o.Id == 1);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-008")]
        public async Task GetAllOpportunities_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                _context.Opportunities.Add(new Domain.Entities.Opportunity
                {
                    Id = i,
                    Name = $"Opportunity {i}",
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                });
            }
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<List<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
                .Returns<List<Domain.Entities.Opportunity>>(opps =>
                    opps.Select(o => new OpportunityModel { Id = o.Id, Name = o.Name }).ToList());

            // Act
            var result = await _manager.GetAllAsync(page: 1, pageSize: 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Items.Count);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages); // 25 / 10 = 3 pages
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-009")]
        public async Task GetOpportunitiesByStatus_Active_ReturnsOnlyActive()
        {
            // Arrange
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity { Id = 1, Name = "Opp 1", Status = "Active", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 2, Name = "Opp 2", Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 3, Name = "Opp 3", Status = "Active", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 4, Name = "Opp 4", Status = "Closed", CreatedBy = 1, CreatedDate = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<List<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
                .Returns<List<Domain.Entities.Opportunity>>(opps =>
                    opps.Select(o => new OpportunityModel { Id = o.Id, Name = o.Name, Status = o.Status }).ToList());

            // Act
            var result = await _manager.GetByStatusAsync("Active");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, opp => Assert.Equal("Active", opp.Status));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-OM-F-010")]
        public async Task GetOpportunitiesByOrgUnit_FiltersCorrectly()
        {
            // Arrange
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity { Id = 1, Name = "Opp 1", ResponsibleOrgUnitId = 1, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 2, Name = "Opp 2", ResponsibleOrgUnitId = 2, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 3, Name = "Opp 3", ResponsibleOrgUnitId = 1, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<List<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
                .Returns<List<Domain.Entities.Opportunity>>(opps =>
                    opps.Select(o => new OpportunityModel { Id = o.Id, Name = o.Name }).ToList());

            // Act
            var result = await _manager.GetByOrgUnitAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, o => o.Id == 1);
            Assert.Contains(result, o => o.Id == 3);
            Assert.DoesNotContain(result, o => o.Id == 2);
        }

        #endregion

        #region Status Lifecycle Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Lifecycle")]
        [Trait("TestId", "TC-OPP-OM-L-001")]
        public async Task TransitionFromDraftToActive_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            await _manager.UpdateStatusAsync(1, "Active");

            // Assert
            var updated = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Active", updated.Status);
            Assert.True(updated.ActivatedDate.HasValue);
            Assert.Equal(1, updated.ActivatedBy);

            // Verify status history created
            var history = await _context.OpportunityStatusHistory
                .Where(h => h.OpportunityId == 1)
                .OrderBy(h => h.ChangedDate)
                .ToListAsync();
            Assert.Equal(2, history.Count); // Draft (initial) + Active
            Assert.Equal("Draft", history[0].Status);
            Assert.Equal("Active", history[1].Status);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Lifecycle")]
        [Trait("TestId", "TC-OPP-OM-L-002")]
        public async Task TransitionToOnHold_WithReason_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Active",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var reason = "Partner requested delay due to budget review";

            // Act
            await _manager.UpdateStatusAsync(1, "OnHold", reason);

            // Assert
            var updated = await _context.Opportunities.FindAsync(1);
            Assert.Equal("OnHold", updated.Status);
            Assert.Equal(reason, updated.OnHoldReason);
            Assert.True(updated.OnHoldDate.HasValue);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Lifecycle")]
        [Trait("TestId", "TC-OPP-OM-L-003")]
        public async Task TransitionToClosed_WithReason_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Active",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var reason = "Partner decided not to proceed";

            // Act
            await _manager.UpdateStatusAsync(1, "Closed", reason);

            // Assert
            var updated = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Closed", updated.Status);
            Assert.Equal(reason, updated.ClosedReason);
            Assert.True(updated.ClosedDate.HasValue);
            Assert.Equal(1, updated.ClosedBy);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Lifecycle")]
        [Trait("TestId", "TC-OPP-OM-L-004")]
        public async Task RecoverClosedOpportunity_WithJustification_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Closed",
                ClosedReason = "Original closure reason",
                ClosedDate = DateTime.UtcNow.AddDays(-30),
                ClosedBy = 1,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddMonths(-6)
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var justification = "Partner re-engaged with renewed interest and additional funding";

            // Act
            await _manager.RecoverOpportunityAsync(1, justification);

            // Assert
            var updated = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Active", updated.Status); // Or previous status
            Assert.True(updated.RecoveryDate.HasValue);
            Assert.Equal(1, updated.RecoveredBy);
            Assert.Equal(justification, updated.RecoveryJustification);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-L-005")]
        public async Task InvalidStatusTransition_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Closed",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert - Cannot go from Closed back to Draft directly
            await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.UpdateStatusAsync(1, "Draft"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-OM-L-006")]
        public async Task StatusTransitionWithoutPermission_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            _mockPermissionService.Setup(p => p.HasPermission(It.IsAny<ClaimsPrincipal>(), "Opportunity", "UpdateStatus"))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _manager.UpdateStatusAsync(1, "Active"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-L-007")]
        public async Task StatusTransitionWithoutJustification_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Active",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act & Assert - Closing requires reason
            await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.UpdateStatusAsync(1, "Closed", reason: null));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Audit")]
        [Trait("TestId", "TC-OPP-OM-L-008")]
        public async Task StatusHistory_TracksAllChanges()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Make multiple status changes
            await _manager.UpdateStatusAsync(1, "Active");
            await _manager.UpdateStatusAsync(1, "OnHold", "Waiting for partner");
            await _manager.UpdateStatusAsync(1, "Active", "Partner ready");

            // Assert
            var history = await _context.OpportunityStatusHistory
                .Where(h => h.OpportunityId == 1)
                .OrderBy(h => h.ChangedDate)
                .ToListAsync();

            Assert.Equal(4, history.Count); // Draft, Active, OnHold, Active
            Assert.Equal("Draft", history[0].Status);
            Assert.Equal("Active", history[1].Status);
            Assert.Equal("OnHold", history[2].Status);
            Assert.Equal("Active", history[3].Status);
            Assert.All(history, h => Assert.Equal(1, h.ChangedBy));
        }

        #endregion

        #region AI Suggestions Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-OM-AI-001")]
        public async Task SuggestSDGs_BasedOnDeliverables_ReturnsRelevant()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Clean Water Project",
                Description = "Providing clean water and sanitation systems",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            _context.Deliverables.AddRange(new[]
            {
                new Deliverable { Id = 1, OpportunityId = 1, Name = "Clean water infrastructure" },
                new Deliverable { Id = 2, OpportunityId = 1, Name = "Sanitation systems" }
            });
            await _context.SaveChangesAsync();

            // Act
            var suggestions = await _manager.SuggestSDGsAsync(1);

            // Assert
            Assert.NotNull(suggestions);
            Assert.NotEmpty(suggestions);
            // Should suggest SDG 6 (Clean Water and Sanitation)
            Assert.Contains(suggestions, s => s.SDGNumber == 6);
            Assert.Contains(suggestions, s => s.ConfidenceScore > 0.7);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-OM-AI-002")]
        public async Task SuggestUNCFOutcomes_ByCountry_ReturnsRelevant()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Bangladesh Development Project",
                PrimaryCountryId = 1, // Bangladesh
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var suggestions = await _manager.SuggestUNCFOutcomesAsync(1);

            // Assert
            Assert.NotNull(suggestions);
            Assert.NotEmpty(suggestions);
            Assert.All(suggestions, s =>
            {
                Assert.Equal("BD", s.CountryCode);
                Assert.True(s.ConfidenceScore > 0);
                Assert.NotEmpty(s.Justification);
            });
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-OM-AI-003")]
        public async Task AcceptAISuggestion_UpdatesOpportunity()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var suggestion = new AISuggestion
            {
                Id = 1,
                OpportunityId = 1,
                SuggestionType = "SDG",
                SuggestedValue = "6", // SDG 6
                Status = "Pending"
            };
            _context.AISuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            // Act
            await _manager.AcceptSuggestionAsync(1, 1);

            // Assert
            var updatedSuggestion = await _context.AISuggestions.FindAsync(1);
            Assert.Equal("Accepted", updatedSuggestion.Status);
            Assert.True(updatedSuggestion.AcceptedDate.HasValue);
            Assert.Equal(1, updatedSuggestion.AcceptedBy);

            // Verify opportunity updated with SDG
            var updatedOpportunity = await _context.Opportunities
                .Include(o => o.SDGs)
                .FirstOrDefaultAsync(o => o.Id == 1);
            Assert.Contains(updatedOpportunity.SDGs, sdg => sdg.SDGNumber == 6);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-OM-AI-004")]
        public async Task RejectAISuggestion_RecordsRejection()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var suggestion = new AISuggestion
            {
                Id = 1,
                OpportunityId = 1,
                SuggestionType = "SDG",
                SuggestedValue = "7",
                Status = "Pending"
            };
            _context.AISuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            var reason = "Not relevant to this opportunity's scope";

            // Act
            await _manager.RejectSuggestionAsync(1, 1, reason);

            // Assert
            var updatedSuggestion = await _context.AISuggestions.FindAsync(1);
            Assert.Equal("Rejected", updatedSuggestion.Status);
            Assert.Equal(reason, updatedSuggestion.RejectionReason);
            Assert.True(updatedSuggestion.RejectedDate.HasValue);
            Assert.Equal(1, updatedSuggestion.RejectedBy);

            // Verify opportunity NOT updated
            var opportunity = await _context.Opportunities
                .Include(o => o.SDGs)
                .FirstOrDefaultAsync(o => o.Id == 1);
            Assert.DoesNotContain(opportunity.SDGs, sdg => sdg.SDGNumber == 7);
        }

        #endregion

        #region Conversion Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Conversion")]
        [Trait("TestId", "TC-OPP-OM-C-001")]
        public async Task ConvertOpportunityToProject_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Infrastructure Development",
                Status = "Approved",
                EstimatedValue = 2500000,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var projectDetails = new ProjectConversionDetails
            {
                ProjectCode = "PROJ-2026-001",
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(19)
            };

            // Act
            var project = await _manager.ConvertToProjectAsync(1, projectDetails);

            // Assert
            Assert.NotNull(project);
            Assert.Equal(opportunity.Name, project.Name);
            Assert.Equal("PROJ-2026-001", project.ProjectCode);

            // Verify opportunity updated
            var updatedOpportunity = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Converted", updatedOpportunity.Status);
            Assert.True(updatedOpportunity.ConvertedDate.HasValue);
            Assert.Equal(project.Id, updatedOpportunity.ConvertedToProjectId);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-C-004")]
        public async Task ConvertNonApprovedOpportunity_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Draft", // Not approved
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var projectDetails = new ProjectConversionDetails
            {
                ProjectCode = "PROJ-2026-001"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.ConvertToProjectAsync(1, projectDetails));

            Assert.Contains("must be approved", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-C-005")]
        public async Task ConvertAlreadyConvertedOpportunity_ThrowsException()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                Status = "Converted",
                ConvertedDate = DateTime.UtcNow.AddDays(-7),
                ConvertedToProjectId = 123,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddMonths(-6)
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var projectDetails = new ProjectConversionDetails
            {
                ProjectCode = "PROJ-2026-002"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.ConvertToProjectAsync(1, projectDetails));

            Assert.Contains("already converted", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("123", ex.Message); // Should reference existing project
        }

        #endregion

        #region Validation Tests

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-V-001")]
        public async Task CreateOpportunity_InvalidEstimatedValue_ThrowsException(decimal invalidValue)
        {
            // Arrange
            var request = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                EstimatedValue = invalidValue,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.CreateOpportunityAsync(request));

            Assert.Contains("value must be positive", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-V-002")]
        public async Task CreateOpportunity_InvalidCountryId_ThrowsException()
        {
            // Arrange
            var request = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                CurrencyId = 1,
                PrimaryCountryId = 999, // Non-existent
                ResponsibleOrgUnitId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.CreateOpportunityAsync(request));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-OM-V-004")]
        public async Task CreateOpportunity_InvalidTimeline_ThrowsException()
        {
            // Arrange
            var request = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1,
                StartDate = new DateTime(2025, 12, 01),
                EndDate = new DateTime(2025, 01, 01) // Before start date
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.CreateOpportunityAsync(request));

            Assert.Contains("end date must be after start date", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Permission Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-OM-P-001")]
        public async Task CreateOpportunity_WithoutPermission_ThrowsException()
        {
            // Arrange
            _mockPermissionService.Setup(p => p.HasPermission(It.IsAny<ClaimsPrincipal>(), "Opportunity", "Create"))
                .Returns(false);

            var request = new OpportunityCreateRequest
            {
                Name = "Test Opportunity",
                EstimatedValue = 1000000
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _manager.CreateOpportunityAsync(request));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-OM-P-004")]
        public async Task GetAll_WithRowLevelSecurity_FiltersCorrectly()
        {
            // Arrange - Create opportunities in different org units
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity { Id = 1, Name = "Opp 1", ResponsibleOrgUnitId = 1, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 2, Name = "Opp 2", ResponsibleOrgUnitId = 2, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow },
                new Domain.Entities.Opportunity { Id = 3, Name = "Opp 3", ResponsibleOrgUnitId = 1, Status = "Draft", CreatedBy = 1, CreatedDate = DateTime.UtcNow }
            });
            await _context.SaveChangesAsync();

            // Mock permission service to only allow access to OrgUnit 1
            _mockPermissionService.Setup(p => p.FilterByOrgUnit(It.IsAny<ClaimsPrincipal>(), It.IsAny<IQueryable<Domain.Entities.Opportunity>>()))
                .Returns<ClaimsPrincipal, IQueryable<Domain.Entities.Opportunity>>((user, query) =>
                    query.Where(o => o.ResponsibleOrgUnitId == 1));

            _mockMapper.Setup(m => m.Map<List<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
                .Returns<List<Domain.Entities.Opportunity>>(opps =>
                    opps.Select(o => new OpportunityModel { Id = o.Id, Name = o.Name }).ToList());

            // Act
            var result = await _manager.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Items.Count); // Only opportunities from OrgUnit 1
            Assert.Contains(result.Items, o => o.Id == 1);
            Assert.Contains(result.Items, o => o.Id == 3);
            Assert.DoesNotContain(result.Items, o => o.Id == 2);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
