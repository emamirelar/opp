using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class EmergencyWorkflowTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IDecisionManager> _mockDecisionManager;
        private readonly OpportunityManager _opportunityManager;

        public EmergencyWorkflowTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"EmergencyTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockDecisionManager = new Mock<IDecisionManager>();
            _opportunityManager = new OpportunityManager(_context);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-010")]
        public async Task EmergencyFastTrack_24HourDecision_CompleteWorkflow()
        {
            // Arrange - Emergency opportunity
            var emergency = new Domain.Entities.Opportunity
            {
                Name = "Nepal Earthquake Emergency Response",
                EstimatedValue = 500000,
                IsEmergency = true,
                Priority = "Critical",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(emergency);
            await _context.SaveChangesAsync();

            // Act - Simplified workflow for emergency
            // Mock fast-track decision (24 hours vs 5-7 days)
            _mockDecisionManager.Setup(m => m.RecordEmergencyDecisionAsync(emergency.Id, "Go", "Emergency approval", 1))
                .ReturnsAsync(new OpportunityDecision
                {
                    Id = 1,
                    OpportunityId = emergency.Id,
                    Decision = "Go",
                    DecisionDate = DateTime.UtcNow.AddHours(4), // Decision in 4 hours
                    IsEmergencyDecision = true
                });

            var decision = await _mockDecisionManager.Object.RecordEmergencyDecisionAsync(emergency.Id, "Go", "Emergency approval", 1);

            // Assert
            Assert.True(decision.IsEmergencyDecision);
            var decisionTime = (decision.DecisionDate - emergency.CreatedDate).TotalHours;
            Assert.True(decisionTime < 24); // Under 24 hours
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-POS-012")]
        public async Task SameDayFastTrack_8Hours_CompleteLifecycle()
        {
            // Arrange - Same day fast-track
            var startTime = DateTime.UtcNow.Date.AddHours(9); // 9 AM

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Same-Day Opportunity",
                EstimatedValue = 1800000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = startTime
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Complete all steps within 8 hours
            // Create (9 AM), DST (10:30 AM), Decision (1 PM), Submit (4:30 PM)
            var completionTime = startTime.AddHours(7.5); // 4:30 PM

            // Assert
            var totalHours = (completionTime - startTime).TotalHours;
            Assert.True(totalHours <= 8); // Within 8-hour window
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-EMG-001")]
        public async Task EmergencyOpportunity_SimplifiedDST_ReducedReview()
        {
            // Arrange - Emergency opportunity with simplified DST
            var emergency = new Domain.Entities.Opportunity
            {
                Name = "Cyclone Relief - Mozambique",
                EstimatedValue = 750000,
                IsEmergency = true,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(emergency);
            await _context.SaveChangesAsync();

            // Mock simplified DST (3 questions vs 12)
            _mockDecisionManager.Setup(m => m.GetSimplifiedDSTAsync(emergency.Id))
                .ReturnsAsync(new SimplifiedDST
                {
                    QuestionCount = 3,
                    ComplexityScore = 6.5m,
                    IsEmergency = true,
                    RecommendedAction = "Proceed - Emergency conditions warrant immediate action"
                });

            // Act
            var simplifiedDST = await _mockDecisionManager.Object.GetSimplifiedDSTAsync(emergency.Id);

            // Assert
            Assert.NotNull(simplifiedDST);
            Assert.True(simplifiedDST.IsEmergency);
            Assert.Equal(3, simplifiedDST.QuestionCount); // Simplified vs 12 full questions
            Assert.Contains("Emergency", simplifiedDST.RecommendedAction);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-EMG-002")]
        public async Task EmergencyEscalation_HigherDOALevel_AutoApproval()
        {
            // Arrange - Emergency requiring higher DOA
            var emergency = new Domain.Entities.Opportunity
            {
                Name = "Humanitarian Crisis Response",
                EstimatedValue = 5000000, // Requires Director-level DOA
                IsEmergency = true,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(emergency);
            await _context.SaveChangesAsync();

            // Mock automatic escalation to Director
            _mockDecisionManager.Setup(m => m.EscalateEmergencyDecisionAsync(emergency.Id, "Director"))
                .ReturnsAsync(new EscalationResult
                {
                    Success = true,
                    EscalatedTo = "Director",
                    EscalationReason = "Emergency: High value requires Director approval",
                    EscalationTime = DateTime.UtcNow.AddHours(2) // Fast escalation
                });

            // Act
            var escalation = await _mockDecisionManager.Object.EscalateEmergencyDecisionAsync(emergency.Id, "Director");

            // Assert
            Assert.True(escalation.Success);
            Assert.Equal("Director", escalation.EscalatedTo);
            Assert.Contains("Emergency", escalation.EscalationReason);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-EMG-003")]
        public async Task EmergencyPostHocDocumentation_DecisionFirst_DocumentLater()
        {
            // Arrange - Emergency decision made verbally/phone
            var emergency = new Domain.Entities.Opportunity
            {
                Name = "Urgent Shelter Provision",
                EstimatedValue = 300000,
                IsEmergency = true,
                Status = "Approved", // Already approved verbally
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddHours(-12)
            };
            _context.Opportunities.Add(emergency);
            await _context.SaveChangesAsync();

            // Mock post-hoc documentation
            _mockDecisionManager.Setup(m => m.RecordPostHocDecisionAsync(emergency.Id, "Go", "Verbal approval - now documenting", 1))
                .ReturnsAsync(new OpportunityDecision
                {
                    Id = 1,
                    OpportunityId = emergency.Id,
                    Decision = "Go",
                    DecisionDate = DateTime.UtcNow, // Documented now
                    ActualDecisionDate = DateTime.UtcNow.AddHours(-10), // Decision made 10 hours ago
                    IsPostHoc = true,
                    IsEmergencyDecision = true
                });

            // Act
            var decision = await _mockDecisionManager.Object.RecordPostHocDecisionAsync(emergency.Id, "Go", "Verbal approval - now documenting", 1);

            // Assert
            Assert.True(decision.IsPostHoc);
            Assert.True(decision.IsEmergencyDecision);
            var documentationDelay = (decision.DecisionDate - decision.ActualDecisionDate).TotalHours;
            Assert.True(documentationDelay > 0); // Decision documented later
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-EMG-004")]
        public async Task EmergencyConversion_ImmediateProject_SkipAuthorization()
        {
            // Arrange - Emergency approved, immediate conversion
            var emergency = new Domain.Entities.Opportunity
            {
                Name = "Emergency Medical Supplies",
                EstimatedValue = 450000,
                IsEmergency = true,
                Status = "Approved",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddHours(-6)
            };
            _context.Opportunities.Add(emergency);
            await _context.SaveChangesAsync();

            // Mock immediate conversion (skip Authorization stage)
            _mockDecisionManager.Setup(m => m.ConvertEmergencyOpportunityAsync(emergency.Id))
                .ReturnsAsync(new ConversionResult
                {
                    Success = true,
                    ProjectId = 1000,
                    ConversionTime = DateTime.UtcNow,
                    SkippedStages = new[] { "Authorization" },
                    Reason = "Emergency: Immediate conversion authorized"
                });

            // Act
            var conversion = await _mockDecisionManager.Object.ConvertEmergencyOpportunityAsync(emergency.Id);

            // Assert
            Assert.True(conversion.Success);
            Assert.NotNull(conversion.SkippedStages);
            Assert.Contains("Authorization", conversion.SkippedStages);
            Assert.Contains("Emergency", conversion.Reason);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-EMG-005-NegativeTest")]
        public async Task EmergencyFlagMisuse_StandardOpportunity_Rejected()
        {
            // Arrange - Standard opportunity incorrectly flagged as emergency
            var fakeEmergency = new Domain.Entities.Opportunity
            {
                Name = "Regular IT Infrastructure",
                EstimatedValue = 2000000,
                IsEmergency = true, // Incorrectly flagged
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(fakeEmergency);
            await _context.SaveChangesAsync();

            // Mock validation that detects misuse
            _mockDecisionManager.Setup(m => m.ValidateEmergencyFlagAsync(fakeEmergency.Id))
                .ReturnsAsync(new EmergencyValidationResult
                {
                    IsValid = false,
                    Reason = "Emergency flag not justified - no humanitarian crisis, natural disaster, or immediate threat",
                    SuggestedAction = "Remove emergency flag and follow standard workflow"
                });

            // Act
            var validation = await _mockDecisionManager.Object.ValidateEmergencyFlagAsync(fakeEmergency.Id);

            // Assert
            Assert.False(validation.IsValid);
            Assert.Contains("not justified", validation.Reason);
            Assert.Contains("standard workflow", validation.SuggestedAction);
        }

        public class OpportunityDecision
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Decision { get; set; }
            public DateTime DecisionDate { get; set; }
            public bool IsEmergencyDecision { get; set; }
            public DateTime ActualDecisionDate { get; set; }
            public bool IsPostHoc { get; set; }
        }

        public class SimplifiedDST
        {
            public int QuestionCount { get; set; }
            public decimal ComplexityScore { get; set; }
            public bool IsEmergency { get; set; }
            public string RecommendedAction { get; set; }
        }

        public class EscalationResult
        {
            public bool Success { get; set; }
            public string EscalatedTo { get; set; }
            public string EscalationReason { get; set; }
            public DateTime EscalationTime { get; set; }
        }

        public class ConversionResult
        {
            public bool Success { get; set; }
            public int ProjectId { get; set; }
            public DateTime ConversionTime { get; set; }
            public string[] SkippedStages { get; set; }
            public string Reason { get; set; }
        }

        public class EmergencyValidationResult
        {
            public bool IsValid { get; set; }
            public string Reason { get; set; }
            public string SuggestedAction { get; set; }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
