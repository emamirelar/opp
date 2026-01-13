using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.BusinessLogic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.BusinessLogic
{
    /// <summary>
    /// Tests for Go/No-Go decision workflow logic
    /// Based on GoNoGoDecision_TestCases.md (25+ tests)
    /// </summary>
    public class GoNoGoDecisionTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly GoNoGoDecisionLogic _decisionLogic;

        public GoNoGoDecisionTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"GoNoGoTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockNotificationService = new Mock<INotificationService>();
            _mockPermissionService = new Mock<IPermissionService>();

            _decisionLogic = new GoNoGoDecisionLogic(
                _context,
                _mockNotificationService.Object,
                _mockPermissionService.Object
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed opportunity
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Decision Test Opportunity",
                EstimatedValue = 2000000,
                Status = "Ready for Decision",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            // Seed DOA holders
            _context.Users.AddRange(new[]
            {
                new User { Id = 1, Name = "DOA3", DOALevel = 3, DOALimit = 5000000 },
                new User { Id = 2, Name = "DOA2", DOALevel = 2, DOALimit = 10000000 }
            });

            _context.SaveChanges();
        }

        #region TC-OPP-GONOGO-F-001: Initiate Go/No-Go Process

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-F-001")]
        public async Task InitiateGoNoGoProcess_CompletePackage_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var initiationResult = await _decisionLogic.InitiateGoNoGoProcessAsync(opportunityId, initiatedBy: 1);

            // Assert
            Assert.True(initiationResult.Success);
            Assert.NotNull(initiationResult.ProcessId);
            
            // Process record created
            var process = await _context.GoNoGoProcesses.FirstOrDefaultAsync(p => p.OpportunityId == opportunityId);
            Assert.NotNull(process);
            Assert.Equal("Initiated", process.Status);
            Assert.NotNull(process.InitiatedDate);
        }

        #endregion

        #region TC-OPP-GONOGO-F-002: Route to Appropriate DOA Level

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "BusinessRule")]
        [Trait("TestId", "TC-OPP-GONOGO-F-002")]
        public async Task RouteToDetermineDOALevel_BasedOnBudget_CorrectRouting()
        {
            // Arrange
            var smallOpportunity = new Domain.Entities.Opportunity
            {
                Id = 10,
                EstimatedValue = 800000, // $800K - DOA4 can approve
                Status = "Ready for Decision"
            };

            var largeOpportunity = new Domain.Entities.Opportunity
            {
                Id = 11,
                EstimatedValue = 8000000, // $8M - requires DOA1
                Status = "Ready for Decision"
            };

            _context.Opportunities.AddRange(smallOpportunity, largeOpportunity);
            await _context.SaveChangesAsync();

            // Act
            var smallRoute = await _decisionLogic.DetermineDOALevelAsync(10);
            var largeRoute = await _decisionLogic.DetermineDOALevelAsync(11);

            // Assert
            // Small opportunity: DOA4 sufficient
            Assert.Equal(4, smallRoute.RequiredDOALevel);
            Assert.Equal("DOA4", smallRoute.AuthorityLevel);

            // Large opportunity: DOA1 required
            Assert.Equal(1, largeRoute.RequiredDOALevel);
            Assert.Equal("DOA1", largeRoute.AuthorityLevel);
        }

        #endregion

        #region TC-OPP-GONOGO-F-003: Sequential Review Stages

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Workflow")]
        [Trait("TestId", "TC-OPP-GONOGO-F-003")]
        public async Task SequentialReviewStages_AllStages_CompletesInOrder()
        {
            // Arrange
            var opportunityId = 1;
            await _decisionLogic.InitiateGoNoGoProcessAsync(opportunityId, initiatedBy: 1);

            // Define sequential stages
            var stages = new[] { "Technical Review", "Financial Review", "Legal Review", "DOA Decision" };

            // Act - Progress through stages
            foreach (var stage in stages)
            {
                var result = await _decisionLogic.CompleteStageAsync(opportunityId, stage, approvedBy: 1);
                Assert.True(result.Success);
            }

            // Assert - All stages completed
            var process = await _context.GoNoGoProcesses
                .Include(p => p.CompletedStages)
                .FirstAsync(p => p.OpportunityId == opportunityId);

            Assert.Equal(4, process.CompletedStages.Count);
            Assert.True(process.IsComplete);
        }

        #endregion

        #region TC-OPP-GONOGO-F-004: Parallel Review Stages

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Workflow")]
        [Trait("TestId", "TC-OPP-GONOGO-F-004")]
        public async Task ParallelReviewStages_AllReviewers_CompletesWhenAllDone()
        {
            // Arrange
            var opportunityId = 1;
            await _decisionLogic.InitiateGoNoGoProcessAsync(opportunityId, initiatedBy: 1);

            // Parallel reviewers: Technical, Financial, Legal (all must approve)
            // Act - Reviews in any order
            await _decisionLogic.CompleteParallelStageAsync(opportunityId, "Financial Review", reviewerId: 3);
            await _decisionLogic.CompleteParallelStageAsync(opportunityId, "Technical Review", reviewerId: 2);
            await _decisionLogic.CompleteParallelStageAsync(opportunityId, "Legal Review", reviewerId: 4);

            // Assert - All parallel reviews complete
            var parallelStatus = await _decisionLogic.GetParallelStageStatusAsync(opportunityId);
            Assert.Equal(3, parallelStatus.CompletedReviews);
            Assert.Equal(3, parallelStatus.RequiredReviews);
            Assert.True(parallelStatus.AllComplete);
        }

        #endregion

        #region TC-OPP-GONOGO-F-005: Deadline Management and Escalation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Workflow")]
        [Trait("TestId", "TC-OPP-GONOGO-F-005")]
        public async Task DeadlineManagement_Overdue_TriggersEscalation()
        {
            // Arrange
            var opportunityId = 1;
            var deadline = DateTime.UtcNow.AddDays(-2); // Overdue by 2 days
            
            var process = await _context.GoNoGoProcesses.FirstAsync(p => p.OpportunityId == opportunityId);
            process.DecisionDeadline = deadline;
            await _context.SaveChangesAsync();

            // Act - Check for overdue decisions
            var overdueItems = await _decisionLogic.GetOverdueDecisionsAsync();

            // Assert
            Assert.Contains(overdueItems, item => item.OpportunityId == opportunityId);
            
            var overdueItem = overdueItems.First(item => item.OpportunityId == opportunityId);
            Assert.True(overdueItem.DaysOverdue >= 2);
            Assert.True(overdueItem.RequiresEscalation);
            
            // Escalation notification sent
            _mockNotificationService.Verify(
                n => n.SendEscalationNotificationAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region TC-OPP-GONOGO-F-006: Conditional Go Decision

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-F-006")]
        public async Task RecordConditionalGo_WithConditions_TracksConditions()
        {
            // Arrange
            var opportunityId = 1;
            var conditions = new List<string>
            {
                "Infrastructure advisor sign-off required",
                "Monthly progress reports to HQ mandatory",
                "Environmental assessment must be completed before start"
            };

            // Act
            var decision = await _decisionLogic.RecordConditionalGoAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Approved with conditions to mitigate identified risks",
                conditions: conditions);

            // Assert
            Assert.NotNull(decision);
            Assert.Equal("Go with Conditions", decision.Decision);
            Assert.Equal(3, decision.Conditions.Count);
            Assert.All(decision.Conditions, c => Assert.Equal("Pending", c.Status));
        }

        #endregion

        #region TC-OPP-GONOGO-F-007: Condition Fulfillment Tracking

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-F-007")]
        public async Task TrackConditionFulfillment_MarkComplete_UpdatesStatus()
        {
            // Arrange
            var opportunityId = 1;
            
            // Create decision with conditions
            var decision = new OpportunityDecision
            {
                Id = 1,
                OpportunityId = opportunityId,
                Decision = "Go with Conditions"
            };
            _context.OpportunityDecisions.Add(decision);

            var condition1 = new DecisionCondition
            {
                Id = 1,
                DecisionId = 1,
                Condition = "Test condition 1",
                Status = "Pending"
            };
            _context.DecisionConditions.Add(condition1);
            await _context.SaveChangesAsync();

            // Act - Mark condition as fulfilled
            await _decisionLogic.MarkConditionFulfilledAsync(1, fulfilledBy: 1, evidence: "Advisor signed off");

            // Assert
            var updatedCondition = await _context.DecisionConditions.FindAsync(1);
            Assert.Equal("Fulfilled", updatedCondition.Status);
            Assert.NotNull(updatedCondition.FulfilledDate);
            Assert.Equal(1, updatedCondition.FulfilledBy);
            Assert.Contains("Advisor signed off", updatedCondition.Evidence);
        }

        #endregion

        #region TC-OPP-GONOGO-WF-008: Decision Withdrawal

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Workflow")]
        [Trait("TestId", "TC-OPP-GONOGO-WF-008")]
        public async Task WithdrawDecision_BeforeDecisionMade_AllowsWithdrawal()
        {
            // Arrange
            var opportunityId = 1;
            await _decisionLogic.InitiateGoNoGoProcessAsync(opportunityId, initiatedBy: 1);
            
            // Act - Withdraw before decision
            var result = await _decisionLogic.WithdrawDecisionRequestAsync(opportunityId, withdrawnBy: 1, reason: "Scope changed");
            
            // Assert
            Assert.True(result.Success);
            
            var process = await _context.GoNoGoProcesses.FirstAsync(p => p.OpportunityId == opportunityId);
            Assert.Equal("Withdrawn", process.Status);
            Assert.Contains("Scope changed", process.WithdrawalReason);
            
            // DOA holder notified
            _mockNotificationService.Verify(
                n => n.SendWithdrawalNotificationAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-001: Validate Decision Package Completeness

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-001")]
        public async Task ValidateDecisionPackage_MissingComponents_BlocksSubmission()
        {
            // Arrange
            var opportunityId = 1;
            
            // Incomplete package - missing DST, Budget, Risk Register
            // Act
            var validationResult = await _decisionLogic.ValidateDecisionPackageAsync(opportunityId);
            
            // Assert
            Assert.False(validationResult.IsComplete);
            Assert.Contains(validationResult.MissingComponents, c => c == "DST Profile");
            Assert.Contains(validationResult.MissingComponents, c => c == "Budget");
            Assert.Contains(validationResult.MissingComponents, c => c == "Risk Register");
            Assert.Contains(validationResult.MissingComponents, c => c == "Opportunity Statement");
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-002: Validate DST Profile Exists

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-002")]
        public async Task ValidateDSTProfile_NotGenerated_BlocksDecision()
        {
            // Arrange
            var opportunityId = 1;
            // No DST profile exists
            
            // Act
            var canProceed = await _decisionLogic.CanProceedWithDecisionAsync(opportunityId);
            
            // Assert
            Assert.False(canProceed.Allowed);
            Assert.Contains("DST profile", canProceed.BlockingReason);
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-003: Validate Budget Alignment

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-003")]
        public async Task ValidateBudgetAlignment_Mismatch_FlagsDiscrepancy()
        {
            // Arrange
            var opportunityId = 1;
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            opportunity.EstimatedValue = 2000000; // $2M
            
            // But budget details sum to $2.5M
            var budgetDetails = new { TotalBudget = 2500000 };
            
            // Act
            var alignmentCheck = await _decisionLogic.ValidateBudgetAlignmentAsync(opportunityId, budgetDetails);
            
            // Assert
            Assert.False(alignmentCheck.IsAligned);
            Assert.Equal(500000, alignmentCheck.Discrepancy); // $500K difference
            Assert.True(alignmentCheck.RequiresReconciliation);
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-004: Validate Risk Assessment Complete

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-004")]
        public async Task ValidateRiskAssessment_CriticalRisksWithoutMitigation_BlocksSubmission()
        {
            // Arrange
            var opportunityId = 1;
            
            // Add critical risk without mitigation
            var criticalRisk = new Risk
            {
                OpportunityId = opportunityId,
                Description = "Political instability",
                Probability = "High",
                Impact = "High",
                MitigationPlan = null // No mitigation!
            };
            _context.Risks.Add(criticalRisk);
            await _context.SaveChangesAsync();
            
            // Act
            var riskValidation = await _decisionLogic.ValidateRiskAssessmentAsync(opportunityId);
            
            // Assert
            Assert.False(riskValidation.IsComplete);
            Assert.Contains(riskValidation.Issues, i => i.Contains("mitigation"));
            Assert.True(riskValidation.BlocksDecision);
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-005: Validate Due Diligence Status

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-005")]
        public async Task ValidateDueDiligence_PendingStatus_AllowsWithWarning()
        {
            // Arrange
            var opportunityId = 1;
            
            // Partner due diligence pending
            var dueDiligence = new DueDiligenceCheck
            {
                OpportunityId = opportunityId,
                Status = "Pending",
                Type = "Partner Financial"
            };
            _context.DueDiligenceChecks.Add(dueDiligence);
            await _context.SaveChangesAsync();
            
            // Act
            var ddValidation = await _decisionLogic.ValidateDueDiligenceAsync(opportunityId);
            
            // Assert
            Assert.True(ddValidation.CanProceed); // Not a blocker
            Assert.True(ddValidation.HasWarnings);
            Assert.Contains(ddValidation.Warnings, w => w.Contains("pending"));
        }

        #endregion

        #region TC-OPP-GONOGO-VAL-006: Validate Approvals Obtained

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-GONOGO-VAL-006")]
        public async Task ValidateApprovals_MissingTechnicalApproval_BlocksDecision()
        {
            // Arrange
            var opportunityId = 1;
            
            // Required approvals: Technical, Financial, Legal
            // Only Financial approved
            var approvals = new[]
            {
                new { Type = "Financial", Status = "Approved" }
            };
            
            // Act
            var approvalValidation = await _decisionLogic.ValidateRequiredApprovalsAsync(opportunityId, approvals);
            
            // Assert
            Assert.False(approvalValidation.AllApprovalsObtained);
            Assert.Contains(approvalValidation.MissingApprovals, a => a == "Technical");
            Assert.Contains(approvalValidation.MissingApprovals, a => a == "Legal");
        }

        #endregion

        #region TC-OPP-GONOGO-NOT-001: Notify DOA Holder

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NOT-001")]
        public async Task NotifyDOAHolder_PackageSubmitted_SendsNotification()
        {
            // Arrange
            var opportunityId = 1;
            var doaHolderId = 1;
            
            // Act
            await _decisionLogic.SubmitForDecisionAsync(opportunityId, doaHolderId, submittedBy: 1);
            
            // Assert - Notification sent
            _mockNotificationService.Verify(
                n => n.SendDOANotificationAsync(
                    doaHolderId,
                    It.Is<string>(s => s.Contains("decision package")),
                    It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-GONOGO-NOT-002: Notify Development Team of Decision

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NOT-002")]
        public async Task NotifyDevelopmentTeam_GoDecisionMade_TeamNotified()
        {
            // Arrange
            var opportunityId = 1;
            var developmentTeam = new[] { 1, 2, 3 }; // User IDs
            
            // Act - DOA makes Go decision
            await _decisionLogic.RecordGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Approved - proceed with implementation");
            
            // Assert - Team notified
            _mockNotificationService.Verify(
                n => n.SendTeamNotificationAsync(
                    It.IsAny<int[]>(),
                    It.Is<string>(s => s.Contains("Go") || s.Contains("Approved")),
                    It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region TC-OPP-GONOGO-NOT-003: Notify on Decision Delays

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NOT-003")]
        public async Task NotifyOnDelays_SevenDaysPending_SendsReminder()
        {
            // Arrange
            var opportunityId = 1;
            var process = await _context.GoNoGoProcesses.FirstAsync(p => p.OpportunityId == opportunityId);
            process.InitiatedDate = DateTime.UtcNow.AddDays(-7); // 7 days ago
            process.Status = "Pending";
            await _context.SaveChangesAsync();
            
            // Act - Run delay check
            await _decisionLogic.CheckForDelaysAndNotifyAsync();
            
            // Assert - Reminder sent
            _mockNotificationService.Verify(
                n => n.SendReminderNotificationAsync(
                    It.IsAny<int>(),
                    It.Is<string>(s => s.Contains("pending") || s.Contains("days")),
                    It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region TC-OPP-GONOGO-NOT-004: Notify Finance on Approval

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NOT-004")]
        public async Task NotifyFinance_GoDecisionWithBudget_FinanceNotified()
        {
            // Arrange
            var opportunityId = 1;
            var budgetAmount = 2000000m;
            
            // Act - Record Go decision with budget
            await _decisionLogic.RecordGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Approved",
                budgetAuthorized: budgetAmount);
            
            // Assert - Finance system notified
            _mockNotificationService.Verify(
                n => n.SendFinanceNotificationAsync(
                    It.IsAny<int>(),
                    It.Is<decimal>(amt => amt == budgetAmount),
                    It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-GONOGO-NOT-005: Notify Partners on Go Decision

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NOT-005")]
        public async Task NotifyPartners_GoDecision_PartnersInformed()
        {
            // Arrange
            var opportunityId = 1;
            var partnerIds = new[] { 1, 2 };
            
            // Act - Record Go decision
            await _decisionLogic.RecordGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Approved",
                notifyPartners: true);
            
            // Assert - Partners notified
            _mockNotificationService.Verify(
                n => n.SendPartnerNotificationAsync(
                    It.IsAny<int[]>(),
                    It.Is<string>(s => s.Contains("approved") || s.Contains("Go")),
                    It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-GONOGO-COND-001: Record Conditional Go with Tracking

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-COND-001")]
        public async Task RecordConditionalGo_MultipleConditions_TracksAllConditions()
        {
            // Arrange
            var opportunityId = 1;
            var conditions = new List<string>
            {
                "Complete environmental assessment",
                "Obtain local government approval",
                "Secure additional $500K funding",
                "Partner capacity assessment"
            };
            
            // Act
            var decision = await _decisionLogic.RecordConditionalGoAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Approved subject to conditions",
                conditions: conditions);
            
            // Assert
            Assert.NotNull(decision);
            Assert.Equal("Go with Conditions", decision.Decision);
            Assert.Equal(4, decision.Conditions.Count);
            
            // All conditions tracked
            Assert.All(decision.Conditions, c =>
            {
                Assert.Equal("Pending", c.Status);
                Assert.NotNull(c.Condition);
                Assert.Null(c.FulfilledDate);
            });
        }

        #endregion

        #region TC-OPP-GONOGO-COND-002 to COND-006: Condition Management

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-COND-002")]
        public async Task TrackConditionProgress_PartialCompletion_UpdatesProgress()
        {
            // Arrange
            var opportunityId = 1;
            var decision = await _decisionLogic.RecordConditionalGoAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Conditional approval",
                conditions: new[] { "Condition 1", "Condition 2", "Condition 3" }.ToList());
            
            // Act - Fulfill one condition
            await _decisionLogic.MarkConditionFulfilledAsync(decision.Conditions[0].Id, 1, "Evidence provided");
            
            // Get progress
            var progress = await _decisionLogic.GetConditionProgressAsync(opportunityId);
            
            // Assert
            Assert.Equal(33.33m, progress.PercentComplete, 2); // 1/3 = 33.33%
            Assert.Equal(1, progress.FulfilledCount);
            Assert.Equal(3, progress.TotalCount);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-COND-003")]
        public async Task AllConditionsFulfilled_AllComplete_UpdatesOpportunityStatus()
        {
            // Arrange
            var opportunityId = 1;
            var decision = await _decisionLogic.RecordConditionalGoAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Conditional",
                conditions: new[] { "Condition 1", "Condition 2" }.ToList());
            
            // Act - Fulfill all conditions
            foreach (var condition in decision.Conditions)
            {
                await _decisionLogic.MarkConditionFulfilledAsync(condition.Id, 1, "Complete");
            }
            
            // Assert - Opportunity status updated
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Approved - All Conditions Met", opportunity.Status);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-COND-004")]
        public async Task ConditionWithDeadline_Overdue_FlagsEscalation()
        {
            // Arrange
            var opportunityId = 1;
            var decision = await _decisionLogic.RecordConditionalGoAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Conditional",
                conditions: new[] { "Time-sensitive condition" }.ToList());
            
            // Set deadline in the past
            var condition = decision.Conditions[0];
            condition.Deadline = DateTime.UtcNow.AddDays(-2);
            await _context.SaveChangesAsync();
            
            // Act
            var overdueConditions = await _decisionLogic.GetOverdueConditionsAsync();
            
            // Assert
            Assert.Contains(overdueConditions, c => c.Id == condition.Id);
            Assert.True(overdueConditions.First().RequiresEscalation);
        }

        #endregion

        #region TC-OPP-GONOGO-NO-001 to NO-003: No-Go Decision Handling

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-NO-001")]
        public async Task RecordNoGoDecision_WithRationale_UpdatesStatusAndRecords()
        {
            // Arrange
            var opportunityId = 1;
            var rationale = "Risk profile too high, budget insufficient for scope";
            
            // Act
            var decision = await _decisionLogic.RecordNoGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: rationale);
            
            // Assert
            Assert.NotNull(decision);
            Assert.Equal("No-Go", decision.Decision);
            Assert.Contains("Risk profile", decision.Rationale);
            
            // Opportunity status updated
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Rejected", opportunity.Status);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-GONOGO-NO-002")]
        public async Task NoGoDecision_CapturesLessonsLearned_StoresForFuture()
        {
            // Arrange
            var opportunityId = 1;
            var lessonsLearned = "Need stronger technical capacity assessment in early stages";
            
            // Act
            await _decisionLogic.RecordNoGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Technical capacity insufficient",
                lessonsLearned: lessonsLearned);
            
            // Assert - Lessons learned captured
            var lessons = await _decisionLogic.GetLessonsLearnedAsync(opportunityId);
            Assert.Contains(lessons, l => l.Contains("technical capacity"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-GONOGO-NO-003")]
        public async Task NoGoDecision_NotifiesStakeholders_AllPartiesInformed()
        {
            // Arrange
            var opportunityId = 1;
            
            // Act
            await _decisionLogic.RecordNoGoDecisionAsync(
                opportunityId,
                decisionMakerId: 1,
                rationale: "Not aligned with strategic priorities");
            
            // Assert - Notifications sent
            _mockNotificationService.Verify(
                n => n.SendNoGoNotificationAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Helper Classes

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DOALevel { get; set; }
            public decimal DOALimit { get; set; }
        }

        public class GoNoGoProcess
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Status { get; set; }
            public DateTime? InitiatedDate { get; set; }
            public DateTime? DecisionDeadline { get; set; }
            public bool IsComplete { get; set; }
            public List<CompletedStage> CompletedStages { get; set; }
        }

        public class CompletedStage
        {
            public string StageName { get; set; }
            public DateTime CompletedDate { get; set; }
        }

        public class DOARoutingResult
        {
            public int RequiredDOALevel { get; set; }
            public string AuthorityLevel { get; set; }
        }

        public class ParallelStageStatus
        {
            public int CompletedReviews { get; set; }
            public int RequiredReviews { get; set; }
            public bool AllComplete { get; set; }
        }

        public class OverdueDecisionItem
        {
            public int OpportunityId { get; set; }
            public int DaysOverdue { get; set; }
            public bool RequiresEscalation { get; set; }
        }

        public class DecisionCondition
        {
            public int Id { get; set; }
            public int DecisionId { get; set; }
            public string Condition { get; set; }
            public string Status { get; set; } // Pending, Fulfilled
            public DateTime? FulfilledDate { get; set; }
            public int? FulfilledBy { get; set; }
            public string Evidence { get; set; }
            public DateTime? Deadline { get; set; }
            public bool RequiresEscalation { get; set; }
        }

        public class ProcessInitiationResult
        {
            public bool Success { get; set; }
            public string ProcessId { get; set; }
        }

        public class StageCompletionResult
        {
            public bool Success { get; set; }
        }

        public class WithdrawalResult
        {
            public bool Success { get; set; }
        }

        public class DecisionPackageValidation
        {
            public bool IsComplete { get; set; }
            public List<string> MissingComponents { get; set; } = new List<string>();
        }

        public class ProceedCheckResult
        {
            public bool Allowed { get; set; }
            public string BlockingReason { get; set; }
        }

        public class BudgetAlignmentCheck
        {
            public bool IsAligned { get; set; }
            public decimal Discrepancy { get; set; }
            public bool RequiresReconciliation { get; set; }
        }

        public class RiskValidationResult
        {
            public bool IsComplete { get; set; }
            public List<string> Issues { get; set; } = new List<string>();
            public bool BlocksDecision { get; set; }
        }

        public class DueDiligenceValidation
        {
            public bool CanProceed { get; set; }
            public bool HasWarnings { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public class ApprovalValidationResult
        {
            public bool AllApprovalsObtained { get; set; }
            public List<string> MissingApprovals { get; set; } = new List<string>();
        }

        public class ConditionProgressResult
        {
            public decimal PercentComplete { get; set; }
            public int FulfilledCount { get; set; }
            public int TotalCount { get; set; }
        }

        public class Risk
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Description { get; set; }
            public string Probability { get; set; }
            public string Impact { get; set; }
            public string MitigationPlan { get; set; }
        }

        public class DueDiligenceCheck
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
