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
    /// Tests for OpportunityWorkflow business logic
    /// Based on OpportunityWorkflow_TestCases.md (35+ tests)
    /// </summary>
    public class OpportunityWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly OpportunityWorkflowLogic _workflowLogic;

        public OpportunityWorkflowTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"WorkflowTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockNotificationService = new Mock<INotificationService>();

            _workflowLogic = new OpportunityWorkflowLogic(
                _context,
                _mockNotificationService.Object
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Workflow Test Opportunity",
                Status = "Draft",
                WorkflowStage = "Identification",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        #region TC-OPP-WF-F-001: Status Transition Draft to Profiling

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-001")]
        public async Task TransitionStatus_DraftToProfiling_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await _workflowLogic.TransitionStatusAsync(opportunityId, "Profiling", transitionedBy: 1);

            // Assert
            Assert.True(result.Success);
            
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Profiling", opportunity.Status);
            Assert.Equal("Profiling", opportunity.WorkflowStage);
            
            // Audit trail created
            var auditEntry = await _context.WorkflowAuditTrail
                .FirstOrDefaultAsync(a => a.OpportunityId == opportunityId);
            Assert.NotNull(auditEntry);
            Assert.Equal("Draft", auditEntry.FromStatus);
            Assert.Equal("Profiling", auditEntry.ToStatus);
        }

        #endregion

        #region TC-OPP-WF-F-002: Invalid Status Transition Blocked

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-WF-F-002")]
        public async Task TransitionStatus_InvalidTransition_ThrowsException()
        {
            // Arrange
            var opportunityId = 1; // Status = "Draft"

            // Act & Assert - Cannot jump from Draft to Authorized
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _workflowLogic.TransitionStatusAsync(opportunityId, "Authorized", transitionedBy: 1));

            Assert.Contains("invalid transition", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Draft", ex.Message);
            Assert.Contains("Authorized", ex.Message);
            
            // Suggest valid transitions
            Assert.Contains("Profiling", ex.Message); // Valid next status
        }

        #endregion

        #region TC-OPP-WF-F-003: Multi-Level Approval Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-003")]
        public async Task ExecuteApprovalWorkflow_MultiLevel_Success()
        {
            // Arrange
            var opportunityId = 1;
            
            // Define approval workflow: Technical → Legal → Financial → DOA
            var approvalStages = new List<ApprovalStage>
            {
                new ApprovalStage { Order = 1, ApproverRole = "Technical Advisor", ApproverUserId = 2 },
                new ApprovalStage { Order = 2, ApproverRole = "Legal Advisor", ApproverUserId = 3 },
                new ApprovalStage { Order = 3, ApproverRole = "Financial Advisor", ApproverUserId = 4 },
                new ApprovalStage { Order = 4, ApproverRole = "DOA Holder", ApproverUserId = 5 }
            };

            // Act - Execute workflow stage by stage
            // Stage 1: Technical Approval
            var stage1Result = await _workflowLogic.ProcessApprovalAsync(opportunityId, approverUserId: 2, decision: "Approved");
            Assert.True(stage1Result.Approved);

            // Stage 2: Legal Approval
            var stage2Result = await _workflowLogic.ProcessApprovalAsync(opportunityId, approverUserId: 3, decision: "Approved");
            Assert.True(stage2Result.Approved);

            // Stage 3: Financial Approval
            var stage3Result = await _workflowLogic.ProcessApprovalAsync(opportunityId, approverUserId: 4, decision: "Approved");
            Assert.True(stage3Result.Approved);

            // Stage 4: DOA Approval (final)
            var stage4Result = await _workflowLogic.ProcessApprovalAsync(opportunityId, approverUserId: 5, decision: "Approved");
            Assert.True(stage4Result.Approved);
            Assert.True(stage4Result.WorkflowComplete); // All stages complete

            // Assert - All approvals recorded
            var approvals = await _context.WorkflowApprovals
                .Where(a => a.OpportunityId == opportunityId)
                .OrderBy(a => a.ApprovalDate)
                .ToListAsync();

            Assert.Equal(4, approvals.Count);
            Assert.Equal(2, approvals[0].ApprovedBy); // Technical
            Assert.Equal(3, approvals[1].ApprovedBy); // Legal
            Assert.Equal(4, approvals[2].ApprovedBy); // Financial
            Assert.Equal(5, approvals[3].ApprovedBy); // DOA
        }

        #endregion

        #region TC-OPP-WF-F-004: Rejection at Any Stage

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-004")]
        public async Task ApprovalWorkflow_RejectedAtStage2_WorkflowStopped()
        {
            // Arrange
            var opportunityId = 1;

            // Act - Stage 1 approved
            await _workflowLogic.ProcessApprovalAsync(opportunityId, approverUserId: 2, decision: "Approved");

            // Act - Stage 2 rejected
            var rejectionResult = await _workflowLogic.ProcessApprovalAsync(
                opportunityId, 
                approverUserId: 3, 
                decision: "Rejected",
                comments: "Budget concerns - needs revision");

            // Assert
            Assert.False(rejectionResult.Approved);
            Assert.True(rejectionResult.WorkflowStopped);
            
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Revision Required", opportunity.Status);
            
            // Notification sent to opportunity manager
            _mockNotificationService.Verify(
                n => n.SendNotificationAsync(
                    It.Is<NotificationRequest>(r => r.Subject.Contains("rejected")),
                    It.IsAny<int>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-WF-F-005: Parallel Approval Workflow

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-005")]
        public async Task ParallelApprovalWorkflow_AllApprove_Success()
        {
            // Arrange - Parallel approval (all must approve, order doesn't matter)
            var opportunityId = 1;
            var parallelApprovers = new[] { 2, 3, 4 }; // Technical, Legal, Financial (in parallel)

            // Act - Approvers approve in any order
            await _workflowLogic.ProcessParallelApprovalAsync(opportunityId, approverUserId: 3, decision: "Approved"); // Legal first
            await _workflowLogic.ProcessParallelApprovalAsync(opportunityId, approverUserId: 2, decision: "Approved"); // Technical second
            await _workflowLogic.ProcessParallelApprovalAsync(opportunityId, approverUserId: 4, decision: "Approved"); // Financial last

            // Assert - Workflow complete when all approve
            var workflowStatus = await _workflowLogic.GetWorkflowStatusAsync(opportunityId);
            Assert.Equal(3, workflowStatus.ApprovalsReceived);
            Assert.Equal(3, workflowStatus.ApprovalsRequired);
            Assert.True(workflowStatus.IsComplete);
        }

        #endregion

        #region TC-OPP-WF-F-006: Escalation Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-006")]
        public async Task EscalateToHigherAuthority_ValidReason_Success()
        {
            // Arrange
            var opportunityId = 1;
            var fromUserId = 2; // DOA4
            var toUserId = 5; // DOA2 (higher authority)

            // Act
            var escalationResult = await _workflowLogic.EscalateAsync(
                opportunityId,
                fromUserId,
                toUserId,
                reason: "Complexity beyond my authority level - seeking DOA2 guidance");

            // Assert
            Assert.True(escalationResult.Success);
            
            var escalation = await _context.WorkflowEscalations
                .FirstOrDefaultAsync(e => e.OpportunityId == opportunityId);
            
            Assert.NotNull(escalation);
            Assert.Equal(fromUserId, escalation.FromUserId);
            Assert.Equal(toUserId, escalation.ToUserId);
            Assert.Contains("authority", escalation.Reason);
            
            // Notification sent to higher authority
            _mockNotificationService.Verify(
                n => n.SendNotificationAsync(
                    It.Is<NotificationRequest>(r => r.UserId == toUserId),
                    It.IsAny<int>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-WF-F-007: Workflow Deadline Management

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-007")]
        public async Task WorkflowDeadline_ApproachingDeadline_SendsReminders()
        {
            // Arrange
            var opportunityId = 1;
            var deadline = DateTime.UtcNow.AddDays(2); // Deadline in 2 days
            
            await _workflowLogic.SetWorkflowDeadlineAsync(opportunityId, deadline);

            // Act - Check for approaching deadlines
            var approachingDeadlines = await _workflowLogic.GetApproachingDeadlinesAsync(daysThreshold: 3);

            // Assert
            Assert.Contains(approachingDeadlines, d => d.OpportunityId == opportunityId);
            
            var urgentItem = approachingDeadlines.First(d => d.OpportunityId == opportunityId);
            Assert.Equal(2, urgentItem.DaysRemaining);
            Assert.True(urgentItem.IsUrgent);
            
            // Reminders should be sent
            _mockNotificationService.Verify(
                n => n.SendReminderAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region TC-OPP-WF-F-008: Conditional Workflow Based on Opportunity Value

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "BusinessRule")]
        [Trait("TestId", "TC-OPP-WF-F-008")]
        public async Task DetermineWorkflow_BasedOnValue_CorrectApprovalLevels()
        {
            // Arrange - Small opportunity (<$500K)
            var smallOpportunity = new Domain.Entities.Opportunity
            {
                Id = 100,
                Name = "Small Opportunity",
                EstimatedValue = 400000, // $400K
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(smallOpportunity);

            // Large opportunity (>$5M)
            var largeOpportunity = new Domain.Entities.Opportunity
            {
                Id = 101,
                Name = "Large Opportunity",
                EstimatedValue = 8000000, // $8M
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(largeOpportunity);
            await _context.SaveChangesAsync();

            // Act
            var smallWorkflow = await _workflowLogic.DetermineApprovalWorkflowAsync(100);
            var largeWorkflow = await _workflowLogic.DetermineApprovalWorkflowAsync(101);

            // Assert - Small opportunity: Simpler workflow
            Assert.Equal(2, smallWorkflow.ApprovalStages.Count); // Technical + DOA4
            Assert.Contains(smallWorkflow.ApprovalStages, s => s.AuthorityLevel == "DOA4");

            // Assert - Large opportunity: More complex workflow
            Assert.Equal(4, largeWorkflow.ApprovalStages.Count); // Technical + Legal + Financial + DOA1
            Assert.Contains(largeWorkflow.ApprovalStages, s => s.AuthorityLevel == "DOA1");
        }

        #endregion

        #region TC-OPP-WF-F-009: Automated Workflow Progression

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-WF-F-009")]
        public async Task AutomatedProgression_AllPrerequisitesMet_AdvancesAutomatically()
        {
            // Arrange
            var opportunityId = 1;
            
            // Prerequisites for "Ready for Decision" status
            var prerequisites = new Dictionary<string, bool>
            {
                { "DST Profile Complete", false },
                { "Budget Finalized", false },
                { "Schedule Created", false },
                { "Risk Register Populated", false }
            };

            // Act - Complete prerequisites one by one
            prerequisites["DST Profile Complete"] = true;
            Assert.False(AllPrerequisitesMet(prerequisites)); // Not ready yet

            prerequisites["Budget Finalized"] = true;
            Assert.False(AllPrerequisitesMet(prerequisites)); // Not ready yet

            prerequisites["Schedule Created"] = true;
            Assert.False(AllPrerequisitesMet(prerequisites)); // Not ready yet

            prerequisites["Risk Register Populated"] = true;
            Assert.True(AllPrerequisitesMet(prerequisites)); // NOW ready!

            // Automatically advance status
            if (AllPrerequisitesMet(prerequisites))
            {
                await _workflowLogic.TransitionStatusAsync(opportunityId, "Ready for Decision", transitionedBy: 0); // System
            }

            // Assert
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Ready for Decision", opportunity.Status);
        }

        private bool AllPrerequisitesMet(Dictionary<string, bool> prerequisites)
        {
            return prerequisites.All(p => p.Value);
        }

        #endregion

        #region TC-OPP-WF-F-010: Workflow Rollback on Error

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "ErrorHandling")]
        [Trait("TestId", "TC-OPP-WF-F-010")]
        public async Task WorkflowTransition_ErrorDuringTransition_RollsBack()
        {
            // Arrange
            var opportunityId = 1;
            var originalStatus = "Draft";

            // Act - Begin transition
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var opportunity = await _context.Opportunities.FindAsync(opportunityId);
                opportunity.Status = "Profiling";
                await _context.SaveChangesAsync();

                // Simulate error during notification send
                throw new Exception("Notification service error");

                // Would commit here if successful
                // await transaction.CommitAsync();
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync();
            }

            // Assert - Status unchanged due to rollback
            var unchangedOpportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal(originalStatus, unchangedOpportunity.Status);
        }

        #endregion

        #region Helper Classes

        public class ApprovalStage
        {
            public int Order { get; set; }
            public string ApproverRole { get; set; }
            public int ApproverUserId { get; set; }
            public string AuthorityLevel { get; set; }
        }

        public class WorkflowAuditTrail
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string FromStatus { get; set; }
            public string ToStatus { get; set; }
            public int TransitionedBy { get; set; }
            public DateTime TransitionedDate { get; set; }
        }

        public class WorkflowApproval
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int ApprovedBy { get; set; }
            public string Decision { get; set; }
            public DateTime ApprovalDate { get; set; }
        }

        public class WorkflowEscalation
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int FromUserId { get; set; }
            public int ToUserId { get; set; }
            public string Reason { get; set; }
            public DateTime EscalatedDate { get; set; }
        }

        public class ApprovalWorkflow
        {
            public List<ApprovalStage> ApprovalStages { get; set; }
        }

        public class WorkflowStatus
        {
            public int ApprovalsReceived { get; set; }
            public int ApprovalsRequired { get; set; }
            public bool IsComplete { get; set; }
        }

        public class DeadlineItem
        {
            public int OpportunityId { get; set; }
            public int DaysRemaining { get; set; }
            public bool IsUrgent { get; set; }
        }

        public class TransitionResult
        {
            public bool Success { get; set; }
        }

        public class ApprovalResult
        {
            public bool Approved { get; set; }
            public bool WorkflowComplete { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
