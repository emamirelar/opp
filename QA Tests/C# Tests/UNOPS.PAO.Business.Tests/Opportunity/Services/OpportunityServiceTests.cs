using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Services
{
    /// <summary>
    /// Tests for OpportunityService orchestration logic
    /// Based on OpportunityService_TestCases.md (10+ tests)
    /// </summary>
    public class OpportunityServiceTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IOpportunityManager> _mockOpportunityManager;
        private readonly Mock<IDSTManager> _mockDSTManager;
        private readonly Mock<IDecisionManager> _mockDecisionManager;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly OpportunityService _service;

        public OpportunityServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ServiceTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockOpportunityManager = new Mock<IOpportunityManager>();
            _mockDSTManager = new Mock<IDSTManager>();
            _mockDecisionManager = new Mock<IDecisionManager>();
            _mockCacheService = new Mock<ICacheService>();

            _service = new OpportunityService(
                _mockOpportunityManager.Object,
                _mockDSTManager.Object,
                _mockDecisionManager.Object,
                _mockCacheService.Object,
                _context
            );
        }

        #region TC-OPP-SVC-F-001: Orchestrate Opportunity Creation Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-001")]
        public async Task Orchestrateunicreation_CompletWorkflow_Success()
        {
            // Arrange
            var createRequest = new OpportunityCreateRequest
            {
                Name = "Orchestrated Opportunity",
                EstimatedValue = 2000000,
                CurrencyId = 1,
                PrimaryCountryId = 1
            };

            var createdOpportunity = new OpportunityModel { Id = 1, Name = createRequest.Name };

            _mockOpportunityManager.Setup(m => m.CreateOpportunityAsync(createRequest))
                .ReturnsAsync(createdOpportunity);

            // Act
            var result = await _service.OrchestratecreateOpportunityWorkflowAsync(createRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OpportunityId);
            
            // Verify orchestration steps
            _mockOpportunityManager.Verify(m => m.CreateOpportunityAsync(createRequest), Times.Once);
            
            // Additional orchestration: Cache invalidation, notifications, etc.
            _mockCacheService.Verify(c => c.InvalidateAsync("opportunities"), Times.Once);
        }

        #endregion

        #region TC-OPP-SVC-F-002: Coordinate Status Change Across Components

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-002")]
        public async Task CoordinateStatusChange_MultipleComponents_AllUpdated()
        {
            // Arrange
            var opportunityId = 1;
            var newStatus = "Approved";

            _mockOpportunityManager.Setup(m => m.UpdateStatusAsync(opportunityId, newStatus))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CoordinateStatusChangeAsync(opportunityId, newStatus);

            // Assert
            Assert.True(result.Success);
            
            // Verify coordination across components
            _mockOpportunityManager.Verify(m => m.UpdateStatusAsync(opportunityId, newStatus), Times.Once);
            
            // Cache invalidated
            _mockCacheService.Verify(c => c.InvalidateAsync($"opportunity_{opportunityId}"), Times.Once);
            
            // Related components notified (DST, Budget, Schedule might need updates)
        }

        #endregion

        #region TC-OPP-SVC-F-003: Cache Management for Frequently Accessed Data

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-SVC-F-003")]
        public async Task GetOpportunity_CachedData_ReturnFromCache()
        {
            // Arrange
            var opportunityId = 1;
            var cachedOpportunity = new OpportunityModel { Id = opportunityId, Name = "Cached" };

            // Mock cache hit
            _mockCacheService.Setup(c => c.GetAsync<OpportunityModel>($"opportunity_{opportunityId}"))
                .ReturnsAsync(cachedOpportunity);

            // Act
            var result = await _service.GetOpportunityAsync(opportunityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(opportunityId, result.Id);
            
            // Verify database was NOT called (used cache)
            _mockOpportunityManager.Verify(m => m.GetByIdAsync(opportunityId), Times.Never);
            
            // Cache was checked
            _mockCacheService.Verify(c => c.GetAsync<OpportunityModel>($"opportunity_{opportunityId}"), Times.Once);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-SVC-F-003-CacheMiss")]
        public async Task GetOpportunity_CacheMiss_LoadsAndCaches()
        {
            // Arrange
            var opportunityId = 1;
            var opportunity = new OpportunityModel { Id = opportunityId, Name = "Fresh Data" };

            // Mock cache miss
            _mockCacheService.Setup(c => c.GetAsync<OpportunityModel>($"opportunity_{opportunityId}"))
                .ReturnsAsync((OpportunityModel)null);

            // Mock database call
            _mockOpportunityManager.Setup(m => m.GetByIdAsync(opportunityId))
                .ReturnsAsync(opportunity);

            // Act
            var result = await _service.GetOpportunityAsync(opportunityId);

            // Assert
            Assert.NotNull(result);
            
            // Verify database was called
            _mockOpportunityManager.Verify(m => m.GetByIdAsync(opportunityId), Times.Once);
            
            // Verify data was cached for future requests
            _mockCacheService.Verify(c => c.SetAsync($"opportunity_{opportunityId}", opportunity, It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region TC-OPP-SVC-F-004: Assemble Complete Decision Package

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-004")]
        public async Task AssembleDecisionPackage_GathersAllComponents_CompletePackage()
        {
            // Arrange
            var opportunityId = 1;

            // Mock all components
            _mockOpportunityManager.Setup(m => m.GetByIdAsync(opportunityId))
                .ReturnsAsync(new OpportunityModel { Id = opportunityId, Name = "Test" });

            _mockDSTManager.Setup(m => m.GetCurrentProfileAsync(opportunityId))
                .ReturnsAsync(new DSTProfileModel { Id = 1, ComplexityScore = 6.5m });

            // Act
            var package = await _service.AssembleCompleteDecisionPackageAsync(opportunityId);

            // Assert
            Assert.NotNull(package);
            Assert.NotNull(package.OpportunityDetails);
            Assert.NotNull(package.DSTProfile);
            
            // All managers called
            _mockOpportunityManager.Verify(m => m.GetByIdAsync(opportunityId), Times.Once);
            _mockDSTManager.Verify(m => m.GetCurrentProfileAsync(opportunityId), Times.Once);
        }

        #endregion

        #region TC-OPP-SVC-F-005: Integration with External Systems

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-SVC-F-005")]
        public async Task SyncToExternalSystem_OpportunityConverted_Success()
        {
            // Arrange
            var opportunityId = 1;
            var projectId = 100;

            var syncRequest = new ExternalSyncRequest
            {
                OpportunityId = opportunityId,
                ProjectId = projectId,
                TargetSystems = new[] { "ERP", "ProjectManagementTool" }
            };

            // Act
            var result = await _service.SyncToExternalSystemsAsync(syncRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.SystemsSynced.Count);
            Assert.Contains("ERP", result.SystemsSynced);
            Assert.Contains("ProjectManagementTool", result.SystemsSynced);
            
            // No errors
            Assert.Empty(result.Errors);
        }

        #endregion

        #region TC-OPP-SVC-F-006: Bulk Status Update

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-006")]
        public async Task BulkUpdateStatus_MultipleOpportunities_AllUpdated()
        {
            // Arrange
            var opportunityIds = new[] { 1, 2, 3, 4, 5 };
            var newStatus = "Under Review";

            foreach (var id in opportunityIds)
            {
                _mockOpportunityManager.Setup(m => m.UpdateStatusAsync(id, newStatus))
                    .ReturnsAsync(true);
            }

            // Act
            var result = await _service.BulkUpdateStatusAsync(opportunityIds, newStatus);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.UpdatedCount);
            Assert.Empty(result.FailedIds);
            
            // Verify all opportunities updated
            foreach (var id in opportunityIds)
            {
                _mockOpportunityManager.Verify(m => m.UpdateStatusAsync(id, newStatus), Times.Once);
            }
        }

        #endregion

        #region TC-OPP-SVC-F-007: Opportunity Cloning

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-007")]
        public async Task CloneOpportunity_WithAllComponents_CompleteClone()
        {
            // Arrange
            var sourceOpportunityId = 1;
            var cloneRequest = new OpportunityCloneRequest
            {
                SourceOpportunityId = sourceOpportunityId,
                NewName = "Cloned Opportunity",
                IncludeDST = true,
                IncludeBudget = true,
                IncludeSchedule = true,
                IncludeRisks = true
            };

            var sourceOpportunity = new OpportunityModel
            {
                Id = sourceOpportunityId,
                Name = "Original Opportunity",
                EstimatedValue = 2000000
            };

            _mockOpportunityManager.Setup(m => m.GetByIdAsync(sourceOpportunityId))
                .ReturnsAsync(sourceOpportunity);

            _mockOpportunityManager.Setup(m => m.CloneOpportunityAsync(It.IsAny<OpportunityCloneRequest>()))
                .ReturnsAsync(new OpportunityModel { Id = 100, Name = "Cloned Opportunity" });

            // Act
            var clonedOpportunity = await _service.CloneOpportunityWithComponentsAsync(cloneRequest);

            // Assert
            Assert.NotNull(clonedOpportunity);
            Assert.Equal("Cloned Opportunity", clonedOpportunity.Name);
            Assert.NotEqual(sourceOpportunityId, clonedOpportunity.Id);
            
            // Verify cloning orchestration
            _mockOpportunityManager.Verify(m => m.CloneOpportunityAsync(It.IsAny<OpportunityCloneRequest>()), Times.Once);
        }

        #endregion

        #region TC-OPP-SVC-F-008: Validation Orchestration

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-SVC-F-008")]
        public async Task ValidateOpportunityForSubmission_AllChecks_ComprehensiveValidation()
        {
            // Arrange
            var opportunityId = 1;

            _mockOpportunityManager.Setup(m => m.GetByIdAsync(opportunityId))
                .ReturnsAsync(new OpportunityModel { Id = opportunityId, Name = "Test", EstimatedValue = 2000000 });

            _mockDSTManager.Setup(m => m.GetCurrentProfileAsync(opportunityId))
                .ReturnsAsync(new DSTProfileModel { Id = 1, ComplexityScore = 6.5m });

            // Act
            var validation = await _service.ValidateOpportunityForSubmissionAsync(opportunityId);

            // Assert
            Assert.NotNull(validation);
            Assert.True(validation.IsValid);
            Assert.NotEmpty(validation.ChecksPerformed);
            Assert.Contains("OpportunityDetails", validation.ChecksPerformed);
            Assert.Contains("DSTProfile", validation.ChecksPerformed);
            Assert.Contains("RequiredFields", validation.ChecksPerformed);
            
            // No blocking errors
            Assert.Empty(validation.BlockingErrors);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-SVC-F-008-NegativeTest")]
        public async Task ValidateOpportunityForSubmission_MissingDST_ValidationFails()
        {
            // Arrange
            var opportunityId = 1;

            _mockOpportunityManager.Setup(m => m.GetByIdAsync(opportunityId))
                .ReturnsAsync(new OpportunityModel { Id = opportunityId, Name = "Test" });

            // DST Profile missing
            _mockDSTManager.Setup(m => m.GetCurrentProfileAsync(opportunityId))
                .ReturnsAsync((DSTProfileModel)null);

            // Act
            var validation = await _service.ValidateOpportunityForSubmissionAsync(opportunityId);

            // Assert
            Assert.NotNull(validation);
            Assert.False(validation.IsValid);
            Assert.Contains(validation.BlockingErrors, e => e.Contains("DST"));
        }

        #endregion

        #region TC-OPP-SVC-F-009: Notification Orchestration

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-009")]
        public async Task SendOpportunityNotifications_StatusChange_AllStakeholdersNotified()
        {
            // Arrange
            var opportunityId = 1;
            var newStatus = "Approved";
            var notificationRequest = new OpportunityNotificationRequest
            {
                OpportunityId = opportunityId,
                EventType = "StatusChange",
                NewStatus = newStatus,
                NotifyOwner = true,
                NotifyTeam = true,
                NotifyDOAHolder = true
            };

            // Act
            var result = await _service.SendOpportunityNotificationsAsync(notificationRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.RecipientCount); // Owner + Team + DOA Holder
            Assert.Contains("Owner", result.NotifiedGroups);
            Assert.Contains("Team", result.NotifiedGroups);
            Assert.Contains("DOAHolder", result.NotifiedGroups);
        }

        #endregion

        #region TC-OPP-SVC-F-010: Archive and Reactivate

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-010")]
        public async Task ArchiveOpportunity_WithRelatedData_AllComponentsArchived()
        {
            // Arrange
            var opportunityId = 1;
            var archiveReason = "Not pursuing this year - budget constraints";

            _mockOpportunityManager.Setup(m => m.ArchiveOpportunityAsync(opportunityId, archiveReason))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ArchiveOpportunityWithComponentsAsync(opportunityId, archiveReason);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(archiveReason, result.ArchiveReason);
            Assert.NotNull(result.ArchivedDate);
            
            // Verify orchestration
            _mockOpportunityManager.Verify(m => m.ArchiveOpportunityAsync(opportunityId, archiveReason), Times.Once);
            _mockCacheService.Verify(c => c.InvalidateAsync($"opportunity_{opportunityId}"), Times.Once);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Orchestration")]
        [Trait("TestId", "TC-OPP-SVC-F-010-Reactivate")]
        public async Task ReactivateOpportunity_PreviouslyArchived_RestoredSuccessfully()
        {
            // Arrange
            var opportunityId = 1;
            var reactivationNotes = "Re-evaluating due to new funding availability";

            _mockOpportunityManager.Setup(m => m.ReactivateOpportunityAsync(opportunityId, reactivationNotes))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReactivateOpportunityAsync(opportunityId, reactivationNotes);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Active", result.NewStatus);
            Assert.Equal(reactivationNotes, result.ReactivationNotes);
            
            // Cache invalidated for fresh data
            _mockCacheService.Verify(c => c.InvalidateAsync($"opportunity_{opportunityId}"), Times.Once);
        }

        #endregion

        #region Helper Classes

        public class DSTProfileModel
        {
            public int Id { get; set; }
            public decimal ComplexityScore { get; set; }
        }

        public class CompleteDecisionPackage
        {
            public OpportunityModel OpportunityDetails { get; set; }
            public DSTProfileModel DSTProfile { get; set; }
        }

        public class ExternalSyncRequest
        {
            public int OpportunityId { get; set; }
            public int ProjectId { get; set; }
            public string[] TargetSystems { get; set; }
        }

        public class ExternalSyncResult
        {
            public bool Success { get; set; }
            public List<string> SystemsSynced { get; set; }
            public List<string> Errors { get; set; }
        }

        public class BulkUpdateResult
        {
            public bool Success { get; set; }
            public int UpdatedCount { get; set; }
            public List<int> FailedIds { get; set; } = new List<int>();
        }

        public class OpportunityCloneRequest
        {
            public int SourceOpportunityId { get; set; }
            public string NewName { get; set; }
            public bool IncludeDST { get; set; }
            public bool IncludeBudget { get; set; }
            public bool IncludeSchedule { get; set; }
            public bool IncludeRisks { get; set; }
        }

        public class OpportunityValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> ChecksPerformed { get; set; } = new List<string>();
            public List<string> BlockingErrors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public class OpportunityNotificationRequest
        {
            public int OpportunityId { get; set; }
            public string EventType { get; set; }
            public string NewStatus { get; set; }
            public bool NotifyOwner { get; set; }
            public bool NotifyTeam { get; set; }
            public bool NotifyDOAHolder { get; set; }
        }

        public class NotificationResult
        {
            public bool Success { get; set; }
            public int RecipientCount { get; set; }
            public List<string> NotifiedGroups { get; set; } = new List<string>();
        }

        public class ArchiveResult
        {
            public bool Success { get; set; }
            public string ArchiveReason { get; set; }
            public DateTime? ArchivedDate { get; set; }
        }

        public class ReactivationResult
        {
            public bool Success { get; set; }
            public string NewStatus { get; set; }
            public string ReactivationNotes { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
