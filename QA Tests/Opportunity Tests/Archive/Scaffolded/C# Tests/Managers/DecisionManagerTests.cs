using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
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
    /// Test suite for DecisionManager - Go/No-Go Decision Process
    /// Tests decision package assembly, approval workflows, authorization
    /// </summary>
    public class DecisionManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly DecisionManager _manager;

        public DecisionManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"DecisionTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();
            _mockNotificationService = new Mock<INotificationService>();

            _manager = new DecisionManager(_mockMapper.Object, _context, _mockNotificationService.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed DOA users
            _context.Users.AddRange(new[]
            {
                new User { Id = 1, Name = "John Doe", DOALevel = "DOA4", DOALimit = 100000 },
                new User { Id = 2, Name = "Jane Smith", DOALevel = "DOA3", DOALimit = 1000000 },
                new User { Id = 3, Name = "Bob Johnson", DOALevel = "DOA2", DOALimit = 5000000 }
            });

            // Seed test opportunities
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity
                {
                    Id = 1,
                    Name = "Small Project",
                    EstimatedValue = 50000,
                    Status = "Approved",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new Domain.Entities.Opportunity
                {
                    Id = 2,
                    Name = "Medium Project",
                    EstimatedValue = 500000,
                    Status = "Approved",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                },
                new Domain.Entities.Opportunity
                {
                    Id = 3,
                    Name = "Large Project",
                    EstimatedValue = 2500000,
                    Status = "Approved",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                }
            });

            _context.SaveChanges();
        }

        #region Decision Package Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DEC-F-001")]
        public async Task AssembleDecisionPackage_CompleteOpportunity_Success()
        {
            // Arrange
            var opportunityId = 2;
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            
            // Add required components
            _context.DSTProfiles.Add(new DSTProfile
            {
                OpportunityId = opportunityId,
                ComplexityScore = 6.5m,
                IsCurrent = true
            });
            _context.OpportunityStatements.Add(new OpportunityStatement
            {
                OpportunityId = opportunityId,
                Version = 1,
                Status = "Final"
            });
            _context.RiskRegisters.Add(new RiskRegister
            {
                OpportunityId = opportunityId
            });
            await _context.SaveChangesAsync();

            // Act
            var package = await _manager.AssembleDecisionPackageAsync(opportunityId);

            // Assert
            Assert.NotNull(package);
            Assert.Equal(opportunityId, package.OpportunityId);
            Assert.NotNull(package.DSTProfile);
            Assert.NotNull(package.OpportunityStatement);
            Assert.NotNull(package.RiskRegister);
            Assert.True(package.IsComplete);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DEC-F-002")]
        public async Task AssembleDecisionPackage_MissingDST_ValidationError()
        {
            // Arrange
            var opportunityId = 1;
            // No DST profile exists

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.AssembleDecisionPackageAsync(opportunityId));

            Assert.Contains("DST profile", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Decision Making Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DEC-D-001")]
        public async Task RecordGoDecision_ValidAuthority_Success()
        {
            // Arrange
            var opportunityId = 1; // $50K - within DOA4
            var userId = 1; // DOA4 ($100K limit)
            var rationale = "Strong strategic alignment, manageable risks, team available";

            // Act
            var decision = await _manager.RecordDecisionAsync(
                opportunityId,
                "Go",
                rationale,
                userId);

            // Assert
            Assert.NotNull(decision);
            Assert.Equal("Go", decision.Decision);
            Assert.Equal(rationale, decision.Rationale);
            Assert.Equal(userId, decision.DecisionMakerId);
            Assert.True(decision.DecisionDate <= DateTime.UtcNow);

            // Verify opportunity updated
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Approved", opportunity.Status);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DEC-D-002")]
        public async Task RecordNoGoDecision_CapturesRationale()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1;
            var rationale = "Risk too high for current context, partner capacity insufficient";

            // Act
            var decision = await _manager.RecordDecisionAsync(
                opportunityId,
                "No-Go",
                rationale,
                userId);

            // Assert
            Assert.Equal("No-Go", decision.Decision);
            Assert.Equal(rationale, decision.Rationale);

            // Verify opportunity updated
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Declined", opportunity.Status);

            // Verify notification sent
            _mockNotificationService.Verify(n =>
                n.SendAsync(It.IsAny<NotificationRequest>()), Times.AtLeastOnce);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DEC-D-003")]
        public async Task RecordConditionalGo_TracksConditions()
        {
            // Arrange
            var opportunityId = 2;
            var userId = 2;
            var rationale = "Approved subject to conditions";
            var conditions = new[]
            {
                "Complete environmental assessment before Q2 2026",
                "Secure partner co-financing commitment",
                "Recruit project manager with bridge engineering experience"
            };

            // Act
            var decision = await _manager.RecordDecisionAsync(
                opportunityId,
                "Go with Conditions",
                rationale,
                userId,
                conditions);

            // Assert
            Assert.Equal("Go with Conditions", decision.Decision);
            Assert.Equal(3, decision.Conditions.Count);
            Assert.All(decision.Conditions, c => Assert.Equal("Pending", c.Status));

            // Verify opportunity status
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Approved - Pending Conditions", opportunity.Status);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-DEC-D-006")]
        public async Task RecordDecision_InsufficientAuthority_ThrowsException()
        {
            // Arrange
            var opportunityId = 3; // $2.5M
            var userId = 2; // DOA3 ($1M limit) - insufficient

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _manager.RecordDecisionAsync(opportunityId, "Go", "Rationale", userId));

            Assert.Contains("authority", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("$1,000,000", ex.Message); // Shows user's limit
            Assert.Contains("$2,500,000", ex.Message); // Shows required amount
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DEC-D-007")]
        public async Task RecordDecision_MissingRationale_ThrowsException()
        {
            // Arrange
            var opportunityId = 1;
            var userId = 1;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.RecordDecisionAsync(opportunityId, "Go", null, userId));

            Assert.Contains("rationale required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Authorization Tests

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-DEC-A-001")]
        public async Task AuthorizeBudget_GoDecision_BudgetReleased()
        {
            // Arrange
            var decision = new OpportunityDecision
            {
                Id = 1,
                OpportunityId = 1,
                Decision = "Go",
                DecisionMakerId = 1,
                DecisionDate = DateTime.UtcNow
            };
            _context.OpportunityDecisions.Add(decision);

            var budget = new OpportunityBudget
            {
                Id = 1,
                OpportunityId = 1,
                TotalAmount = 50000,
                Status = "Draft"
            };
            _context.OpportunityBudgets.Add(budget);
            await _context.SaveChangesAsync();

            // Act
            await _manager.AuthorizeBudgetAsync(1);

            // Assert
            var authorizedBudget = await _context.OpportunityBudgets.FindAsync(1);
            Assert.Equal("Authorized", authorizedBudget.Status);
            Assert.True(authorizedBudget.AuthorizedDate.HasValue);
            Assert.Equal(1, authorizedBudget.AuthorizedBy);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-DEC-A-002")]
        public async Task AuthorizePersonnel_GoDecision_AssignmentsCreated()
        {
            // Arrange
            var decision = new OpportunityDecision
            {
                Id = 2,
                OpportunityId = 2,
                Decision = "Go",
                DecisionMakerId = 2
            };
            _context.OpportunityDecisions.Add(decision);

            var resourcePlan = new ResourcePlan
            {
                Id = 1,
                OpportunityId = 2,
                Roles = new List<ResourceRequirement>
                {
                    new ResourceRequirement { RoleName = "Project Manager", FTE = 1.0m },
                    new ResourceRequirement { RoleName = "Engineer", FTE = 2.0m }
                }
            };
            _context.ResourcePlans.Add(resourcePlan);
            await _context.SaveChangesAsync();

            // Act
            await _manager.AuthorizePersonnelAsync(2);

            // Assert
            var assignments = await _context.PersonnelAssignments
                .Where(a => a.OpportunityId == 2)
                .ToListAsync();
            Assert.Equal(2, assignments.Count); // PM + Engineer
            Assert.All(assignments, a => Assert.Equal("Authorized", a.Status));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-DEC-A-004")]
        public async Task RevokeAuthorization_ValidReason_Success()
        {
            // Arrange
            var budget = new OpportunityBudget
            {
                Id = 2,
                OpportunityId = 1,
                TotalAmount = 50000,
                Status = "Authorized",
                AuthorizedDate = DateTime.UtcNow.AddDays(-7)
            };
            _context.OpportunityBudgets.Add(budget);
            await _context.SaveChangesAsync();

            var reason = "Major risk discovered - requires reassessment";

            // Act
            await _manager.RevokeAuthorizationAsync(1, reason);

            // Assert
            var revokedBudget = await _context.OpportunityBudgets.FindAsync(2);
            Assert.Equal("Revoked", revokedBudget.Status);
            Assert.Equal(reason, revokedBudget.RevocationReason);
            Assert.True(revokedBudget.RevokedDate.HasValue);

            // Verify team notified
            _mockNotificationService.Verify(n =>
                n.SendAsync(It.Is<NotificationRequest>(r =>
                    r.Subject.Contains("Authorization Revoked"))),
                Times.Once);
        }

        #endregion

        #region Delegation Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Delegation")]
        [Trait("TestId", "TC-OPP-DEC-DEL-001")]
        public async Task DelegateDecisionAuthority_ValidDelegate_Success()
        {
            // Arrange
            var fromUserId = 2; // DOA3
            var toUserId = 3; // DOA2 (higher authority - valid)
            var opportunityId = 2;
            var reason = "Out of office next two weeks";

            // Act
            var delegation = await _manager.DelegateDecisionAsync(
                opportunityId,
                fromUserId,
                toUserId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(14),
                reason);

            // Assert
            Assert.NotNull(delegation);
            Assert.Equal(fromUserId, delegation.FromUserId);
            Assert.Equal(toUserId, delegation.ToUserId);
            Assert.Equal(opportunityId, delegation.OpportunityId);
            Assert.True(delegation.IsActive);

            // Verify delegate notified
            _mockNotificationService.Verify(n =>
                n.SendAsync(It.Is<NotificationRequest>(r =>
                    r.RecipientId == toUserId)),
                Times.Once);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DEC-DEL-002")]
        public async Task DelegateDecision_NoAuthority_ThrowsException()
        {
            // Arrange
            var fromUserId = 2;
            var toUserId = 1; // DOA4 - lower authority, invalid

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.DelegateDecisionAsync(1, fromUserId, toUserId,
                    DateTime.UtcNow, DateTime.UtcNow.AddDays(7), "Reason"));

            Assert.Contains("insufficient authority", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Delegation")]
        [Trait("TestId", "TC-OPP-DEC-DEL-004")]
        public async Task EscalateToHigherAuthority_Success()
        {
            // Arrange
            var opportunityId = 3; // $2.5M
            var currentUserId = 2; // DOA3 ($1M limit)
            var higherUserId = 3; // DOA2 ($5M limit)
            var reason = "Exceeds my authority limit";

            // Act
            var escalation = await _manager.EscalateDecisionAsync(
                opportunityId,
                currentUserId,
                higherUserId,
                reason);

            // Assert
            Assert.NotNull(escalation);
            Assert.Equal("Escalated", escalation.Status);
            Assert.Equal(reason, escalation.Reason);

            // Verify higher authority notified
            _mockNotificationService.Verify(n =>
                n.SendAsync(It.Is<NotificationRequest>(r =>
                    r.RecipientId == higherUserId &&
                    r.Subject.Contains("Escalation"))),
                Times.Once);
        }

        #endregion

        #region Audit Trail Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Audit")]
        [Trait("TestId", "TC-OPP-DEC-AUD-001")]
        public async Task DecisionProcess_MaintainsCompleteAuditTrail()
        {
            // Arrange & Act - Complete decision flow
            var opportunityId = 1;
            await _manager.AssembleDecisionPackageAsync(opportunityId);
            await _manager.RecordDecisionAsync(opportunityId, "Go", "Rationale", 1);
            await _manager.AuthorizeBudgetAsync(opportunityId);

            // Assert - Query audit trail
            var auditTrail = await _manager.GetDecisionAuditTrailAsync(opportunityId);

            Assert.NotNull(auditTrail);
            Assert.True(auditTrail.Events.Count >= 3); // Package + Decision + Authorization
            Assert.All(auditTrail.Events, e =>
            {
                Assert.NotNull(e.Timestamp);
                Assert.True(e.UserId > 0);
                Assert.NotEmpty(e.Action);
            });

            // Verify chronological order
            var timestamps = auditTrail.Events.Select(e => e.Timestamp).ToList();
            Assert.Equal(timestamps.OrderBy(t => t).ToList(), timestamps);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Reporting")]
        [Trait("TestId", "TC-OPP-DEC-AUD-003")]
        public async Task ExportDecisionAuditReport_GeneratesPDF()
        {
            // Arrange
            var decisionId = 1;

            // Act
            var pdfBytes = await _manager.GenerateDecisionAuditReportAsync(decisionId);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 1000);
            // Verify PDF signature
            Assert.Equal(0x25, pdfBytes[0]); // %PDF
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
