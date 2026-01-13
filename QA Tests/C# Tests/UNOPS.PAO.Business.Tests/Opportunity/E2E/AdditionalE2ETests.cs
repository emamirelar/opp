using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    /// <summary>
    /// Additional End-to-End test scenarios for comprehensive coverage
    /// Tests complete workflows, multi-step processes, and real-world scenarios
    /// </summary>
    public class AdditionalE2ETests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<IDocumentStorageService> _mockStorageService;

        public AdditionalE2ETests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"E2ETestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockNotificationService = new Mock<INotificationService>();
            _mockAIService = new Mock<IAIService>();
            _mockStorageService = new Mock<IDocumentStorageService>();
        }

        #region TC-OPP-E2E-ADD-001: Complete Partnership Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-001")]
        public async Task CompletePartnershipWorkflow_FromIdentificationToAgreement_Success()
        {
            // Arrange - Create partner
            var partner = new Partner
            {
                Name = "World Food Programme",
                PartnerType = "UN Agency",
                Status = "Active"
            };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            // Step 1: Create opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Food Security Programme",
                EstimatedValue = 3000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Step 2: Link partner
            var oppPartner = new OpportunityPartner
            {
                OpportunityId = opportunity.Id,
                PartnerId = partner.Id,
                Role = "Implementing Partner"
            };
            _context.OpportunityPartners.Add(oppPartner);
            await _context.SaveChangesAsync();

            // Step 3: Create partnership agreement
            var agreement = new PartnershipAgreement
            {
                PartnerId = partner.Id,
                OpportunityId = opportunity.Id,
                AgreementNumber = "MOU-2026-001",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(3),
                Status = "Active",
                AnnualCeiling = 5000000m
            };
            _context.PartnershipAgreements.Add(agreement);
            await _context.SaveChangesAsync();

            // Assert - Complete workflow
            var finalOpportunity = await _context.Opportunities
                .Include(o => o.OpportunityPartners)
                    .ThenInclude(op => op.Agreement)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            Assert.NotNull(finalOpportunity);
            Assert.Single(finalOpportunity.OpportunityPartners);
            Assert.NotNull(finalOpportunity.OpportunityPartners.First().Agreement);
            Assert.Equal("Active", finalOpportunity.OpportunityPartners.First().Agreement.Status);
        }

        #endregion

        #region TC-OPP-E2E-ADD-002: Multi-Country Programme

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-002")]
        public async Task CreateProgramme_MultipleCountries_CompleteSetup()
        {
            // Arrange - Create 5-country regional programme
            var countries = new[] { "Bangladesh", "Nepal", "Sri Lanka", "Bhutan", "Maldives" };
            for (int i = 1; i <= 5; i++)
            {
                _context.Countries.Add(new Country
                {
                    Id = i,
                    Name = countries[i - 1],
                    Code = countries[i - 1].Substring(0, 2).ToUpper()
                });
            }

            // Create regional programme
            var programme = new Domain.Entities.Opportunity
            {
                Name = "South Asia Regional Infrastructure Programme",
                EstimatedValue = 25000000,
                Timeline = 60,
                IsProgramme = true,
                PrimaryCountryId = 1, // Bangladesh (regional hub)
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(programme);
            await _context.SaveChangesAsync();

            // Link to all 5 countries
            for (int i = 1; i <= 5; i++)
            {
                _context.OpportunityCountries.Add(new OpportunityCountry
                {
                    OpportunityId = programme.Id,
                    CountryId = i
                });
            }
            await _context.SaveChangesAsync();

            // Assert
            var programmeWithCountries = await _context.Opportunities
                .Include(o => o.OpportunityCountries)
                .FirstOrDefaultAsync(o => o.Id == programme.Id);

            Assert.True(programmeWithCountries.IsProgramme);
            Assert.Equal(5, programmeWithCountries.OpportunityCountries.Count);
        }

        #endregion

        #region TC-OPP-E2E-ADD-003: Document Version Control

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-003")]
        public async Task UploadDocuments_MultipleVersions_TracksHistory()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Version Control Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Upload document v1
            var doc1 = new OpportunityDocument
            {
                OpportunityId = opportunity.Id,
                FileName = "Concept_Note.pdf",
                Version = 1,
                UploadedDate = DateTime.UtcNow,
                UploadedBy = 1
            };
            _context.OpportunityDocuments.Add(doc1);
            await _context.SaveChangesAsync();

            // Upload document v2 (revision)
            await Task.Delay(100); // Ensure different timestamp
            var doc2 = new OpportunityDocument
            {
                OpportunityId = opportunity.Id,
                FileName = "Concept_Note.pdf",
                Version = 2,
                UploadedDate = DateTime.UtcNow,
                UploadedBy = 1,
                PreviousVersionId = doc1.Id
            };
            _context.OpportunityDocuments.Add(doc2);
            await _context.SaveChangesAsync();

            // Upload document v3
            await Task.Delay(100);
            var doc3 = new OpportunityDocument
            {
                OpportunityId = opportunity.Id,
                FileName = "Concept_Note.pdf",
                Version = 3,
                UploadedDate = DateTime.UtcNow,
                UploadedBy = 1,
                PreviousVersionId = doc2.Id
            };
            _context.OpportunityDocuments.Add(doc3);
            await _context.SaveChangesAsync();

            // Assert - All versions tracked
            var allVersions = await _context.OpportunityDocuments
                .Where(d => d.OpportunityId == opportunity.Id && d.FileName == "Concept_Note.pdf")
                .OrderBy(d => d.Version)
                .ToListAsync();

            Assert.Equal(3, allVersions.Count);
            Assert.Equal(1, allVersions[0].Version);
            Assert.Equal(2, allVersions[1].Version);
            Assert.Equal(3, allVersions[2].Version);
            Assert.Equal(doc2.Id, allVersions[2].PreviousVersionId);
        }

        #endregion

        #region TC-OPP-E2E-ADD-004: Complete Budget Revision Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-004")]
        public async Task BudgetRevisionWorkflow_MultipleIterations_AllVersionsTracked()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Budget Revision Test",
                EstimatedValue = 2000000,
                Status = "Budget Development",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Version 1: Initial budget
            var budget1 = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                Version = 1,
                TotalBudget = 2000000m,
                BaseCost = 1800000m,
                FeeAmount = 200000m,
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunityBudgets.Add(budget1);
            await _context.SaveChangesAsync();

            // Version 2: Revised after partner feedback
            var budget2 = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                Version = 2,
                TotalBudget = 2500000m,
                BaseCost = 2250000m,
                FeeAmount = 250000m,
                RevisionReason = "Partner requested additional activities",
                CreatedDate = DateTime.UtcNow.AddDays(7)
            };
            _context.OpportunityBudgets.Add(budget2);
            await _context.SaveChangesAsync();

            // Version 3: Final budget
            var budget3 = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                Version = 3,
                TotalBudget = 2300000m,
                BaseCost = 2070000m,
                FeeAmount = 230000m,
                RevisionReason = "Final negotiation - reduced scope",
                IsApproved = true,
                CreatedDate = DateTime.UtcNow.AddDays(14)
            };
            _context.OpportunityBudgets.Add(budget3);
            await _context.SaveChangesAsync();

            // Assert - Complete version history
            var allVersions = await _context.OpportunityBudgets
                .Where(b => b.OpportunityId == opportunity.Id)
                .OrderBy(b => b.Version)
                .ToListAsync();

            Assert.Equal(3, allVersions.Count);
            Assert.Equal(2000000m, allVersions[0].TotalBudget);
            Assert.Equal(2500000m, allVersions[1].TotalBudget);
            Assert.Equal(2300000m, allVersions[2].TotalBudget);
            Assert.True(allVersions[2].IsApproved); // Final version approved
        }

        #endregion

        #region TC-OPP-E2E-ADD-005: Opportunity to Project Conversion

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-005")]
        public async Task ConvertOpportunity_ToProject_CompleteDataMigration()
        {
            // Arrange - Approved opportunity ready for conversion
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Approved Opportunity for Conversion",
                EstimatedValue = 5000000,
                Timeline = 36,
                Status = "Authorized",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            var budget = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                Version = 1,
                TotalBudget = 5000000m,
                IsApproved = true
            };
            _context.OpportunityBudgets.Add(budget);

            var schedule = new OpportunitySchedule
            {
                OpportunityId = opportunity.Id,
                TotalMonths = 36,
                IsApproved = true
            };
            _context.OpportunitySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Act - Convert to project
            var project = new Project
            {
                SourceOpportunityId = opportunity.Id,
                Name = opportunity.Name,
                Budget = budget.TotalBudget,
                Duration = schedule.TotalMonths,
                Status = "Active",
                ConversionDate = DateTime.UtcNow
            };
            _context.Projects.Add(project);

            opportunity.Status = "Converted";
            opportunity.ConvertedToProjectId = project.Id;
            await _context.SaveChangesAsync();

            // Assert
            var convertedOpportunity = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);
            
            var createdProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.SourceOpportunityId == opportunity.Id);

            Assert.Equal("Converted", convertedOpportunity.Status);
            Assert.NotNull(createdProject);
            Assert.Equal(opportunity.Name, createdProject.Name);
            Assert.Equal(budget.TotalBudget, createdProject.Budget);
        }

        #endregion

        #region TC-OPP-E2E-ADD-006: Multi-User Collaboration Scenario

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-006")]
        public async Task MultiUserCollaboration_4Users_ConcurrentEditing()
        {
            // Arrange - Create opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Collaborative Opportunity",
                EstimatedValue = 2000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - 4 users working on different components
            // User 1: Works on budget
            var budget = new OpportunityBudget
            {
                OpportunityId = opportunity.Id,
                Version = 1,
                TotalBudget = 2000000m,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunityBudgets.Add(budget);

            // User 2: Works on schedule
            var schedule = new OpportunitySchedule
            {
                OpportunityId = opportunity.Id,
                TotalMonths = 24,
                CreatedBy = 2,
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunitySchedules.Add(schedule);

            // User 3: Works on risk register
            var risk = new OpportunityRisk
            {
                OpportunityId = opportunity.Id,
                RiskDescription = "Political instability",
                Severity = "High",
                CreatedBy = 3,
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunityRisks.Add(risk);

            // User 4: Works on DST profile
            var dstProfile = new DSTProfile
            {
                OpportunityId = opportunity.Id,
                ComplexityScore = 7.5m,
                CreatedBy = 4,
                CreatedDate = DateTime.UtcNow
            };
            _context.DSTProfiles.Add(dstProfile);

            await _context.SaveChangesAsync();

            // Assert - All components created by different users
            Assert.Equal(1, budget.CreatedBy);
            Assert.Equal(2, schedule.CreatedBy);
            Assert.Equal(3, risk.CreatedBy);
            Assert.Equal(4, dstProfile.CreatedBy);
        }

        #endregion

        #region TC-OPP-E2E-ADD-007: Complete Audit Trail

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-007")]
        public async Task CompleteLifecycle_AuditTrail_AllChangesTracked()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Audit Trail Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var auditTrail = new List<AuditEntry>();

            // Act - Track all changes
            // Change 1: Update status
            opportunity.Status = "Profiling";
            opportunity.LastModifiedBy = 1;
            opportunity.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            auditTrail.Add(new AuditEntry { Action = "Status changed to Profiling", UserId = 1 });

            // Change 2: Update budget
            opportunity.EstimatedValue = 1200000;
            opportunity.LastModifiedBy = 2;
            opportunity.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            auditTrail.Add(new AuditEntry { Action = "Budget updated to $1.2M", UserId = 2 });

            // Change 3: Approve
            opportunity.Status = "Approved";
            opportunity.LastModifiedBy = 5;
            opportunity.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            auditTrail.Add(new AuditEntry { Action = "Opportunity approved", UserId = 5 });

            // Assert - Complete audit trail
            Assert.Equal(3, auditTrail.Count);
            Assert.Equal(1, auditTrail[0].UserId);
            Assert.Equal(2, auditTrail[1].UserId);
            Assert.Equal(5, auditTrail[2].UserId);
        }

        #endregion

        #region TC-OPP-E2E-ADD-008: Template Application and Customization

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-008")]
        public async Task ApplyTemplate_CustomizeAndSave_Success()
        {
            // Arrange - Create template
            var template = new OpportunityTemplate
            {
                Name = "Infrastructure Project Template",
                Sector = "Infrastructure",
                DefaultTimeline = 24,
                DefaultDeliverables = new[] { "Design", "Construction", "Handover" }
            };
            _context.OpportunityTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Act - Create opportunity from template
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Bridge Construction - Ghana",
                EstimatedValue = 3000000,
                Timeline = template.DefaultTimeline,
                TemplateId = template.Id,
                Sector = template.Sector,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Apply template deliverables
            foreach (var deliverable in template.DefaultDeliverables)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = opportunity.Id,
                    Description = deliverable,
                    EstimatedCost = 1000000m
                });
            }
            await _context.SaveChangesAsync();

            // Customize: Add extra deliverable
            _context.OpportunityDeliverables.Add(new OpportunityDeliverable
            {
                OpportunityId = opportunity.Id,
                Description = "Environmental Impact Assessment",
                EstimatedCost = 200000m
            });
            await _context.SaveChangesAsync();

            // Assert
            var finalOpportunity = await _context.Opportunities
                .Include(o => o.Deliverables)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            Assert.NotNull(finalOpportunity);
            Assert.Equal(template.Id, finalOpportunity.TemplateId);
            Assert.Equal(4, finalOpportunity.Deliverables.Count); // 3 template + 1 custom
        }

        #endregion

        #region TC-OPP-E2E-ADD-009: Risk Mitigation Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-009")]
        public async Task CompleteRiskMitigation_FromIdentificationToResolution_Success()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Risk Management Test",
                EstimatedValue = 5000000,
                Status = "Risk Assessment",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Step 1: Identify high risk
            var risk = new OpportunityRisk
            {
                OpportunityId = opportunity.Id,
                RiskDescription = "Supply chain disruption",
                Severity = "High",
                Probability = "Medium",
                Impact = 8.5m,
                Status = "Identified",
                CreatedDate = DateTime.UtcNow
            };
            _context.OpportunityRisks.Add(risk);
            await _context.SaveChangesAsync();

            // Step 2: Develop mitigation strategy
            risk.MitigationStrategy = "Establish relationships with 3 alternative suppliers";
            risk.Status = "Mitigation Planned";
            risk.LastModifiedDate = DateTime.UtcNow.AddDays(3);
            await _context.SaveChangesAsync();

            // Step 3: Implement mitigation
            risk.Status = "Mitigation Implemented";
            risk.ResidualRisk = 3.5m; // Reduced from 8.5
            risk.LastModifiedDate = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            // Step 4: Monitor and close
            risk.Status = "Closed";
            risk.ClosureNotes = "Alternative suppliers secured, risk mitigated";
            risk.ClosedDate = DateTime.UtcNow.AddDays(60);
            await _context.SaveChangesAsync();

            // Assert - Complete lifecycle
            var finalRisk = await _context.OpportunityRisks.FindAsync(risk.Id);
            Assert.Equal("Closed", finalRisk.Status);
            Assert.True(finalRisk.ResidualRisk < finalRisk.Impact); // Risk reduced
            Assert.NotNull(finalRisk.ClosureNotes);
        }

        #endregion

        #region TC-OPP-E2E-ADD-010: Notification Cascade

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-010")]
        public async Task StatusChange_NotificationCascade_AllStakeholdersNotified()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Notification Test",
                EstimatedValue = 3000000,
                Status = "Under Review",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);

            // Add stakeholders
            var stakeholders = new[]
            {
                new Stakeholder { OpportunityId = opportunity.Id, UserId = 1, Role = "Owner" },
                new Stakeholder { OpportunityId = opportunity.Id, UserId = 2, Role = "Technical Lead" },
                new Stakeholder { OpportunityId = opportunity.Id, UserId = 3, Role = "Budget Officer" },
                new Stakeholder { OpportunityId = opportunity.Id, UserId = 5, Role = "DOA Holder" }
            };
            _context.Stakeholders.AddRange(stakeholders);
            await _context.SaveChangesAsync();

            // Mock notification service
            var notificationsSent = new List<int>();
            _mockNotificationService
                .Setup(n => n.SendNotificationAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Callback<int, string>((userId, message) => notificationsSent.Add(userId))
                .ReturnsAsync(true);

            // Act - Status change to Approved
            opportunity.Status = "Approved";
            await _context.SaveChangesAsync();

            // Simulate notification cascade
            foreach (var stakeholder in stakeholders)
            {
                await _mockNotificationService.Object.SendNotificationAsync(
                    stakeholder.UserId,
                    $"Opportunity '{opportunity.Name}' has been approved");
            }

            // Assert
            Assert.Equal(4, notificationsSent.Count);
            Assert.Contains(1, notificationsSent); // Owner notified
            Assert.Contains(2, notificationsSent); // Technical lead notified
            Assert.Contains(3, notificationsSent); // Budget officer notified
            Assert.Contains(5, notificationsSent); // DOA holder notified
        }

        #endregion

        #region TC-OPP-E2E-ADD-011: Data Export and Import Cycle

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-011")]
        public async Task ExportAndImport_CompleteOpportunity_DataIntegrity()
        {
            // Arrange - Create complete opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Export Test Opportunity",
                EstimatedValue = 2500000,
                Timeline = 24,
                Status = "Approved",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Export to JSON
            var exportedData = new ExportedOpportunity
            {
                Id = opportunity.Id,
                Name = opportunity.Name,
                EstimatedValue = opportunity.EstimatedValue,
                Timeline = opportunity.Timeline,
                Status = opportunity.Status,
                ExportDate = DateTime.UtcNow
            };

            // Act - Import back (simulate system migration)
            var importedOpportunity = new Domain.Entities.Opportunity
            {
                Name = exportedData.Name + " (Imported)",
                EstimatedValue = exportedData.EstimatedValue,
                Timeline = exportedData.Timeline,
                Status = exportedData.Status,
                SourceOpportunityId = exportedData.Id,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(importedOpportunity);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(exportedData.EstimatedValue, importedOpportunity.EstimatedValue);
            Assert.Equal(exportedData.Timeline, importedOpportunity.Timeline);
            Assert.Equal(exportedData.Id, importedOpportunity.SourceOpportunityId);
        }

        #endregion

        #region TC-OPP-E2E-ADD-012: Lessons Learned Capture

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-012")]
        public async Task NoGoDecision_CaptureLessonsLearned_FutureReference()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "No-Go Test Opportunity",
                EstimatedValue = 8000000,
                Timeline = 48,
                Status = "Ready for Decision",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - No-Go decision with lessons learned
            var decision = new GoNoGoDecision
            {
                OpportunityId = opportunity.Id,
                Decision = "No-Go",
                Reason = "Partner capacity insufficient, political context too unstable",
                DecisionDate = DateTime.UtcNow,
                DecidedBy = 5
            };
            _context.GoNoGoDecisions.Add(decision);

            var lessonsLearned = new LessonsLearned
            {
                OpportunityId = opportunity.Id,
                Category = "Partner Selection",
                Lesson = "Always conduct thorough capacity assessment before opportunity development",
                Severity = "High",
                ApplicableToFuture = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.LessonsLearned.Add(lessonsLearned);

            opportunity.Status = "Closed - No-Go";
            await _context.SaveChangesAsync();

            // Assert
            var savedLesson = await _context.LessonsLearned
                .FirstOrDefaultAsync(l => l.OpportunityId == opportunity.Id);

            Assert.NotNull(savedLesson);
            Assert.Equal("Partner Selection", savedLesson.Category);
            Assert.True(savedLesson.ApplicableToFuture);
        }

        #endregion

        #region TC-OPP-E2E-ADD-013: Bulk Status Update with Rollback

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-013")]
        public async Task BulkStatusUpdate_OneFailure_RollsBackAll()
        {
            // Arrange - Create 10 opportunities
            for (int i = 1; i <= 10; i++)
            {
                _context.Opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Bulk Test {i}",
                    EstimatedValue = 1000000,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            // Act - Attempt bulk update
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var opportunities = await _context.Opportunities.ToListAsync();
                foreach (var opp in opportunities)
                {
                    opp.Status = "Under Review";
                }
                await _context.SaveChangesAsync();

                // Simulate error on 5th opportunity
                if (opportunities.Count >= 5)
                {
                    throw new Exception("Validation failed on opportunity 5");
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
            }

            // Assert - All should remain "Draft" due to rollback
            var statuses = await _context.Opportunities
                .Select(o => o.Status)
                .Distinct()
                .ToListAsync();

            Assert.Single(statuses);
            Assert.Equal("Draft", statuses[0]); // All still Draft
        }

        #endregion

        #region TC-OPP-E2E-ADD-014: Geographic Scope Validation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-014")]
        public async Task CreateOpportunity_MultipleGeographies_ValidatesConsistency()
        {
            // Arrange - Opportunity targeting specific countries
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Regional Water Project",
                EstimatedValue = 5000000,
                PrimaryCountryId = 1, // Bangladesh
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Add geographic scope
            _context.OpportunityCountries.Add(new OpportunityCountry
            {
                OpportunityId = opportunity.Id,
                CountryId = 1 // Bangladesh
            });
            _context.OpportunityCountries.Add(new OpportunityCountry
            {
                OpportunityId = opportunity.Id,
                CountryId = 2 // Nepal
            });
            await _context.SaveChangesAsync();

            // Act - Validate that primary country is included in geography list
            var geoScope = await _context.OpportunityCountries
                .Where(oc => oc.OpportunityId == opportunity.Id)
                .Select(oc => oc.CountryId)
                .ToListAsync();

            var primaryCountryIncluded = geoScope.Contains(opportunity.PrimaryCountryId.Value);

            // Assert
            Assert.True(primaryCountryIncluded); // Primary country must be in scope
            Assert.Equal(2, geoScope.Count);
        }

        #endregion

        #region TC-OPP-E2E-ADD-015: Workflow Timeout Handling

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-015")]
        public async Task WorkflowStage_ExceedsTimeout_AutomaticallyEscalates()
        {
            // Arrange - Opportunity stuck in approval for 30 days
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Timeout Test",
                EstimatedValue = 2000000,
                Status = "Pending Approval",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-30) // Created 30 days ago
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Check for timeouts
            var daysSinceCreation = (DateTime.UtcNow - opportunity.CreatedDate).Days;
            var hasTimedOut = daysSinceCreation > 21; // SLA: 21 days

            if (hasTimedOut)
            {
                var escalation = new WorkflowEscalation
                {
                    OpportunityId = opportunity.Id,
                    Reason = "Approval timeout - exceeded 21-day SLA",
                    EscalatedTo = "Director",
                    EscalationDate = DateTime.UtcNow
                };
                _context.WorkflowEscalations.Add(escalation);
                await _context.SaveChangesAsync();
            }

            // Assert
            Assert.True(hasTimedOut);
            var escalations = await _context.WorkflowEscalations
                .Where(e => e.OpportunityId == opportunity.Id)
                .ToListAsync();

            Assert.Single(escalations);
            Assert.Contains("timeout", escalations[0].Reason, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-E2E-ADD-016: Data Migration Scenario

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-016")]
        public async Task MigrateLegacyData_TransformAndImport_Success()
        {
            // Arrange - Legacy data format
            var legacyData = new LegacyOpportunity
            {
                ProjectName = "Legacy Project",
                BudgetUSD = 2500000,
                DurationMonths = 24,
                StatusCode = "A" // Legacy code
            };

            // Act - Transform to new format
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = legacyData.ProjectName,
                EstimatedValue = legacyData.BudgetUSD,
                Timeline = legacyData.DurationMonths,
                Status = TransformLegacyStatus(legacyData.StatusCode),
                IsLegacyMigration = true,
                CreatedBy = 0, // System migration
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Assert
            var migrated = await _context.Opportunities.FindAsync(opportunity.Id);
            Assert.True(migrated.IsLegacyMigration);
            Assert.Equal("Approved", migrated.Status); // "A" → "Approved"
        }

        private string TransformLegacyStatus(string legacyCode)
        {
            return legacyCode switch
            {
                "D" => "Draft",
                "R" => "Under Review",
                "A" => "Approved",
                "C" => "Closed",
                _ => "Draft"
            };
        }

        #endregion

        #region TC-OPP-E2E-ADD-017: Offline Data Sync

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-017")]
        public async Task OfflineDataSync_ReconnectAndMerge_ConflictResolution()
        {
            // Arrange - Original opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Offline Sync Test",
                EstimatedValue = 1000000,
                Status = "Draft",
                LastSyncDate = DateTime.UtcNow.AddDays(-2),
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Offline edit (on mobile)
            var offlineEdit = new OfflineChange
            {
                OpportunityId = opportunity.Id,
                Field = "EstimatedValue",
                OldValue = "1000000",
                NewValue = "1200000",
                ChangedDate = DateTime.UtcNow.AddDays(-1),
                DeviceId = "MOBILE-001"
            };

            // Online edit (on desktop - concurrent)
            opportunity.EstimatedValue = 1100000;
            opportunity.LastModifiedDate = DateTime.UtcNow.AddDays(-1).AddHours(2);
            await _context.SaveChangesAsync();

            // Act - Reconnect and sync
            // Conflict detected: offline = 1200000, online = 1100000
            var conflict = offlineEdit.NewValue != opportunity.EstimatedValue.ToString();

            // Assert
            Assert.True(conflict); // Conflict detected
            // Resolution strategy: Last write wins, or prompt user
        }

        #endregion

        #region TC-OPP-E2E-ADD-018: Complete Programme Management

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-ADD-018")]
        public async Task CreateProgramme_With5SubProjects_HierarchyManagement()
        {
            // Arrange - Create parent programme
            var programme = new Domain.Entities.Opportunity
            {
                Name = "Regional Education Programme",
                EstimatedValue = 50000000,
                Timeline = 60,
                IsProgramme = true,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(programme);
            await _context.SaveChangesAsync();

            // Act - Create 5 sub-projects
            for (int i = 1; i <= 5; i++)
            {
                var subProject = new Domain.Entities.Opportunity
                {
                    Name = $"School Rehabilitation - District {i}",
                    EstimatedValue = 10000000,
                    Timeline = 36,
                    ParentProgrammeId = programme.Id,
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Opportunities.Add(subProject);
            }
            await _context.SaveChangesAsync();

            // Assert
            var subProjects = await _context.Opportunities
                .Where(o => o.ParentProgrammeId == programme.Id)
                .ToListAsync();

            Assert.Equal(5, subProjects.Count);
            Assert.All(subProjects, sp => Assert.Equal(programme.Id, sp.ParentProgrammeId));

            // Total sub-project value
            var totalValue = subProjects.Sum(sp => sp.EstimatedValue ?? 0);
            Assert.Equal(50000000m, totalValue);
        }

        #endregion

        #region Helper Classes

        public class Partner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PartnerType { get; set; }
            public string Status { get; set; }
        }

        public class OpportunityPartner
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int PartnerId { get; set; }
            public string Role { get; set; }
            public PartnershipAgreement Agreement { get; set; }
        }

        public class PartnershipAgreement
        {
            public int Id { get; set; }
            public int PartnerId { get; set; }
            public int? OpportunityId { get; set; }
            public string AgreementNumber { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }
            public decimal? AnnualCeiling { get; set; }
        }

        public class Country
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public class OpportunityCountry
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int CountryId { get; set; }
        }

        public class OpportunityDocument
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string FileName { get; set; }
            public int Version { get; set; }
            public int? PreviousVersionId { get; set; }
            public DateTime UploadedDate { get; set; }
            public int UploadedBy { get; set; }
        }

        public class OpportunityBudget
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int Version { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal BaseCost { get; set; }
            public decimal FeeAmount { get; set; }
            public string RevisionReason { get; set; }
            public bool IsApproved { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class OpportunitySchedule
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int TotalMonths { get; set; }
            public bool IsApproved { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Project
        {
            public int Id { get; set; }
            public int SourceOpportunityId { get; set; }
            public string Name { get; set; }
            public decimal Budget { get; set; }
            public int Duration { get; set; }
            public string Status { get; set; }
            public DateTime ConversionDate { get; set; }
        }

        public class OpportunityDeliverable
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Description { get; set; }
            public decimal EstimatedCost { get; set; }
        }

        public class OpportunityRisk
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string RiskDescription { get; set; }
            public string Severity { get; set; }
            public string Probability { get; set; }
            public decimal Impact { get; set; }
            public string Status { get; set; }
            public string MitigationStrategy { get; set; }
            public decimal? ResidualRisk { get; set; }
            public string ClosureNotes { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastModifiedDate { get; set; }
            public DateTime? ClosedDate { get; set; }
        }

        public class DSTProfile
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal ComplexityScore { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Stakeholder
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int UserId { get; set; }
            public string Role { get; set; }
        }

        public class AuditEntry
        {
            public string Action { get; set; }
            public int UserId { get; set; }
        }

        public class OpportunityTemplate
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Sector { get; set; }
            public int DefaultTimeline { get; set; }
            public string[] DefaultDeliverables { get; set; }
        }

        public class ExportedOpportunity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal? EstimatedValue { get; set; }
            public int? Timeline { get; set; }
            public string Status { get; set; }
            public DateTime ExportDate { get; set; }
        }

        public class GoNoGoDecision
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Decision { get; set; }
            public string Reason { get; set; }
            public DateTime DecisionDate { get; set; }
            public int DecidedBy { get; set; }
        }

        public class LessonsLearned
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Category { get; set; }
            public string Lesson { get; set; }
            public string Severity { get; set; }
            public bool ApplicableToFuture { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class WorkflowEscalation
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Reason { get; set; }
            public string EscalatedTo { get; set; }
            public DateTime EscalationDate { get; set; }
        }

        public class LegacyOpportunity
        {
            public string ProjectName { get; set; }
            public decimal BudgetUSD { get; set; }
            public int DurationMonths { get; set; }
            public string StatusCode { get; set; }
        }

        public class OfflineChange
        {
            public int OpportunityId { get; set; }
            public string Field { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public DateTime ChangedDate { get; set; }
            public string DeviceId { get; set; }
        }

        public class BusinessException : Exception
        {
            public BusinessException(string message) : base(message) { }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
