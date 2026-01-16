using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    /// <summary>
    /// Tests for DecisionController API endpoints
    /// Based on DecisionController_TestCases.md (10+ tests)
    /// </summary>
    public class DecisionControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DecisionController _controller;

        public DecisionControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _mockMapper = new Mock<IMapper>();

            _controller = new DecisionController(
                _mockManagerWrapper.Object,
                _mockMapper.Object
            );
        }

        #region TC-OPP-DEC-CTRL-F-001: POST - Assemble Decision Package

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DEC-CTRL-F-001")]
        public async Task AssembleDecisionPackage_CompleteOpportunity_ReturnsOkWithPackage()
        {
            // Arrange
            var opportunityId = 1;
            var decisionPackage = new DecisionPackageModel
            {
                OpportunityId = opportunityId,
                IsComplete = true,
                Components = new List<string>
                {
                    "Opportunity Statement",
                    "DST Profile",
                    "Budget",
                    "Schedule",
                    "Risk Register"
                }
            };

            _mockManagerWrapper.Setup(m => m.DecisionManager.AssembleDecisionPackageAsync(opportunityId))
                .ReturnsAsync(decisionPackage);

            // Act
            var result = await _controller.AssemblePackage(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPackage = Assert.IsType<DecisionPackageModel>(okResult.Value);
            Assert.True(returnedPackage.IsComplete);
            Assert.Equal(5, returnedPackage.Components.Count);
        }

        #endregion

        #region TC-OPP-DEC-CTRL-F-002: POST - Record Go Decision

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DEC-CTRL-F-002")]
        public async Task RecordDecision_GoDecision_ReturnsCreated()
        {
            // Arrange
            var decisionRequest = new DecisionRecordRequest
            {
                OpportunityId = 1,
                Decision = "Go",
                Rationale = "Strong alignment with strategy, manageable risks",
                DecisionMakerId = 5
            };

            var recordedDecision = new DecisionModel
            {
                Id = 1,
                OpportunityId = decisionRequest.OpportunityId,
                Decision = decisionRequest.Decision,
                DecisionDate = DateTime.UtcNow
            };

            _mockManagerWrapper.Setup(m => m.DecisionManager.RecordDecisionAsync(
                decisionRequest.OpportunityId,
                decisionRequest.Decision,
                decisionRequest.Rationale,
                decisionRequest.DecisionMakerId))
                .ReturnsAsync(recordedDecision);

            // Act
            var result = await _controller.RecordDecision(decisionRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedDecision = Assert.IsType<DecisionModel>(createdResult.Value);
            Assert.Equal("Go", returnedDecision.Decision);
        }

        #endregion

        #region TC-OPP-DEC-CTRL-F-003: POST - Record No-Go Decision

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DEC-CTRL-F-003")]
        public async Task RecordDecision_NoGoDecision_ReturnsCreated()
        {
            // Arrange
            var decisionRequest = new DecisionRecordRequest
            {
                OpportunityId = 1,
                Decision = "No-Go",
                Rationale = "Insufficient budget, high risk context not suitable",
                DecisionMakerId = 5
            };

            var recordedDecision = new DecisionModel
            {
                Id = 1,
                OpportunityId = decisionRequest.OpportunityId,
                Decision = decisionRequest.Decision,
                DecisionDate = DateTime.UtcNow
            };

            _mockManagerWrapper.Setup(m => m.DecisionManager.RecordDecisionAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
                .ReturnsAsync(recordedDecision);

            // Act
            var result = await _controller.RecordDecision(decisionRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedDecision = Assert.IsType<DecisionModel>(createdResult.Value);
            Assert.Equal("No-Go", returnedDecision.Decision);
        }

        #endregion

        #region TC-OPP-DEC-CTRL-F-004: POST - Authorize Budget

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DEC-CTRL-F-004")]
        public async Task AuthorizeBudget_ValidAuthority_ReturnsOk()
        {
            // Arrange
            var opportunityId = 1;
            var authRequest = new BudgetAuthorizationRequest
            {
                BudgetId = 1,
                AuthorizerId = 5,
                Amount = 2500000m
            };

            _mockManagerWrapper.Setup(m => m.DecisionManager.AuthorizeBudgetAsync(
                opportunityId,
                authRequest.Amount,
                authRequest.AuthorizerId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AuthorizeBudget(opportunityId, authRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        #endregion

        #region TC-OPP-DEC-CTRL-F-005: GET - Get Decision Audit Trail

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-DEC-CTRL-F-005")]
        public async Task GetDecisionAuditTrail_ValidOpportunity_ReturnsCompleteHistory()
        {
            // Arrange
            var opportunityId = 1;
            var auditTrail = new DecisionAuditTrailModel
            {
                OpportunityId = opportunityId,
                Events = new List<AuditEvent>
                {
                    new AuditEvent { Action = "Package Assembled", Timestamp = DateTime.UtcNow.AddDays(-5) },
                    new AuditEvent { Action = "Submitted for Review", Timestamp = DateTime.UtcNow.AddDays(-4) },
                    new AuditEvent { Action = "Technical Review Complete", Timestamp = DateTime.UtcNow.AddDays(-2) },
                    new AuditEvent { Action = "Go Decision Recorded", Timestamp = DateTime.UtcNow.AddDays(-1) },
                    new AuditEvent { Action = "Budget Authorized", Timestamp = DateTime.UtcNow }
                }
            };

            _mockManagerWrapper.Setup(m => m.DecisionManager.GetAuditTrailAsync(opportunityId))
                .ReturnsAsync(auditTrail);

            // Act
            var result = await _controller.GetAuditTrail(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAudit = Assert.IsType<DecisionAuditTrailModel>(okResult.Value);
            Assert.Equal(5, returnedAudit.Events.Count);
            
            // Events in chronological order
            for (int i = 0; i < returnedAudit.Events.Count - 1; i++)
            {
                Assert.True(returnedAudit.Events[i].Timestamp <= returnedAudit.Events[i + 1].Timestamp);
            }
        }

        #endregion

        #region Helper Classes

        public class DecisionPackageModel
        {
            public int OpportunityId { get; set; }
            public bool IsComplete { get; set; }
            public List<string> Components { get; set; }
        }

        public class DecisionRecordRequest
        {
            public int OpportunityId { get; set; }
            public string Decision { get; set; } // Go, No-Go, Go with Conditions
            public string Rationale { get; set; }
            public int DecisionMakerId { get; set; }
        }

        public class DecisionModel
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Decision { get; set; }
            public DateTime DecisionDate { get; set; }
        }

        public class BudgetAuthorizationRequest
        {
            public int BudgetId { get; set; }
            public int AuthorizerId { get; set; }
            public decimal Amount { get; set; }
        }

        public class DecisionAuditTrailModel
        {
            public int OpportunityId { get; set; }
            public List<AuditEvent> Events { get; set; }
        }

        public class AuditEvent
        {
            public string Action { get; set; }
            public DateTime Timestamp { get; set; }
            public int? UserId { get; set; }
            public string Details { get; set; }
        }

        #endregion
    }
}
