using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.BusinessLogic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.BusinessLogic
{
    /// <summary>
    /// Tests for Opportunity Statement generation and management
    /// Based on OpportunityStatement_TestCases.md (20+ tests)
    /// </summary>
    public class OpportunityStatementTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly OpportunityStatementLogic _statementLogic;

        public OpportunityStatementTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"StatementTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();

            _statementLogic = new OpportunityStatementLogic(_context, _mockAIService.Object);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Education Infrastructure",
                Description = "School construction and rehabilitation",
                EstimatedValue = 2500000,
                PrimaryCountryId = 1,
                Timeline = 24,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        #region TC-OPP-STMT-F-001: Generate Statement from Template

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-F-001")]
        public async Task GenerateStatement_UsingTemplate_Success()
        {
            // Arrange
            var opportunityId = 1;
            var templateId = 1; // Standard opportunity statement template

            // Act
            var statement = await _statementLogic.GenerateFromTemplateAsync(opportunityId, templateId);

            // Assert
            Assert.NotNull(statement);
            Assert.Contains("Executive Summary", statement.Sections.Keys);
            Assert.Contains("Background", statement.Sections.Keys);
            Assert.Contains("Objectives", statement.Sections.Keys);
            Assert.Contains("Implementation Approach", statement.Sections.Keys);
            Assert.Contains("Risk Management", statement.Sections.Keys);
            Assert.Contains("Budget Summary", statement.Sections.Keys);
            
            // All sections have content (even if placeholder)
            Assert.All(statement.Sections.Values, content => Assert.NotNull(content));
        }

        #endregion

        #region TC-OPP-STMT-F-002: Pre-Populate from Opportunity Data

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-F-002")]
        public async Task PrePopulateStatement_FromOpportunityData_FieldsPopulated()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var statement = await _statementLogic.PrePopulateFromOpportunityAsync(opportunityId);

            // Assert
            Assert.NotNull(statement);
            
            // Verify pre-population from opportunity data
            Assert.Contains("Education Infrastructure", statement.Title);
            Assert.Contains("2,500,000", statement.BudgetSummary); // $2.5M formatted
            Assert.Contains("24 months", statement.TimelineSummary);
            
            // Some sections fully populated
            Assert.True(statement.PopulationPercentage > 60m); // > 60% populated
        }

        #endregion

        #region TC-OPP-STMT-F-003: AI-Generated Narrative

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-STMT-F-003")]
        public async Task GenerateNarrative_UsingAI_ComprehensiveText()
        {
            // Arrange
            var opportunityId = 1;
            var section = "Background";

            // Mock AI response
            _mockAIService.Setup(ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(@"The Education Infrastructure project aims to improve educational facilities 
                              in underserved regions. With an estimated budget of $2.5M over 24 months, this 
                              initiative will construct and rehabilitate school facilities to benefit thousands 
                              of students. The project aligns with SDG 4 (Quality Education) and national 
                              development priorities.");

            // Act
            var narrative = await _statementLogic.GenerateNarrativeForSectionAsync(opportunityId, section);

            // Assert
            Assert.NotNull(narrative);
            Assert.Contains("Education Infrastructure", narrative);
            Assert.Contains("$2.5M", narrative);
            Assert.Contains("24 months", narrative);
            Assert.True(narrative.Length > 100); // Substantial content
            
            // AI was called
            _mockAIService.Verify(ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        #endregion

        #region TC-OPP-STMT-F-004: Version Control

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-F-004")]
        public async Task SaveStatement_CreatesNewVersion_VersionTracking()
        {
            // Arrange
            var opportunityId = 1;
            var statement = new OpportunityStatement
            {
                OpportunityId = opportunityId,
                Title = "Education Infrastructure Statement",
                Version = 1,
                Sections = new Dictionary<string, string>
                {
                    { "Executive Summary", "Initial summary" }
                }
            };

            // Act - Save version 1
            await _statementLogic.SaveStatementAsync(statement);

            // Update and save version 2
            statement.Sections["Executive Summary"] = "Updated summary with more details";
            statement.Version = 2;
            await _statementLogic.SaveStatementAsync(statement);

            // Assert - Both versions exist
            var versions = await _statementLogic.GetStatementVersionsAsync(opportunityId);
            Assert.Equal(2, versions.Count);
            Assert.Equal(1, versions[0].Version);
            Assert.Equal(2, versions[1].Version);
            
            // Can retrieve any version
            var version1 = await _statementLogic.GetStatementVersionAsync(opportunityId, 1);
            Assert.Contains("Initial summary", version1.Sections["Executive Summary"]);
        }

        #endregion

        #region TC-OPP-STMT-F-005: Export Statement as PDF

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-F-005")]
        public async Task ExportStatement_AsPDF_GeneratesDocument()
        {
            // Arrange
            var opportunityId = 1;
            var statement = await _statementLogic.PrePopulateFromOpportunityAsync(opportunityId);

            // Act
            var pdfResult = await _statementLogic.ExportAsPDFAsync(statement.Id);

            // Assert
            Assert.NotNull(pdfResult);
            Assert.NotNull(pdfResult.FileBytes);
            Assert.True(pdfResult.FileBytes.Length > 0);
            Assert.Equal("application/pdf", pdfResult.ContentType);
            Assert.Contains(".pdf", pdfResult.FileName);
        }

        #endregion

        #region TC-OPP-STMT-TMP-002: Load Sector-Specific Template

        [Theory]
        [InlineData("Infrastructure", "Technical Specifications")]
        [InlineData("Capacity Building", "Training Approach")]
        [InlineData("Procurement", "Supply Chain Management")]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-TMP-002")]
        public async Task LoadSectorSpecificTemplate_VariousSectors_AppropriateTemplate(string sector, string expectedSection)
        {
            // Arrange
            var opportunityId = 1;
            
            // Act
            var statement = await _statementLogic.GenerateSectorSpecificTemplateAsync(opportunityId, sector);
            
            // Assert
            Assert.NotNull(statement);
            Assert.Contains(expectedSection, statement.Sections.Keys);
            
            // Template customized for sector
            Assert.True(statement.Sections.Count >= 6); // Minimum sections
        }

        #endregion

        #region TC-OPP-STMT-TMP-003: Create Custom Template

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-TMP-003")]
        public async Task CreateCustomTemplate_WithCustomSections_SavesTemplate()
        {
            // Arrange
            var customSections = new List<string>
            {
                "Strategic Alignment",
                "Partnership Value",
                "Innovation Approach",
                "Sustainability Plan"
            };
            
            // Act
            var template = await _statementLogic.CreateCustomTemplateAsync(
                name: "Custom Infrastructure Template",
                sections: customSections,
                createdBy: 1);
            
            // Assert
            Assert.NotNull(template);
            Assert.Equal(4, template.Sections.Count);
            Assert.Contains("Strategic Alignment", template.Sections);
            Assert.True(template.IsCustom);
        }

        #endregion

        #region TC-OPP-STMT-TMP-004: Template Versioning

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-TMP-004")]
        public async Task TemplateVersioning_UpdateTemplate_TracksVersions()
        {
            // Arrange
            var template = await _statementLogic.CreateCustomTemplateAsync(
                name: "Test Template",
                sections: new List<string> { "Section 1" },
                createdBy: 1);
            
            // Act - Update template
            template.Sections.Add("Section 2");
            await _statementLogic.UpdateTemplateAsync(template, modifiedBy: 1);
            
            // Act - Get versions
            var versions = await _statementLogic.GetTemplateVersionsAsync(template.Id);
            
            // Assert
            Assert.True(versions.Count >= 2);
            Assert.Equal(1, versions[0].Version);
            Assert.Equal(2, versions[1].Version);
        }

        #endregion

        #region TC-OPP-STMT-TMP-005: Template Validation

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-STMT-TMP-005")]
        public async Task ValidateTemplate_MissingRequiredSections_ReturnsErrors()
        {
            // Arrange
            var incompleteStatement = new OpportunityStatement
            {
                OpportunityId = 1,
                Title = "Test Statement",
                Sections = new Dictionary<string, string>
                {
                    { "Executive Summary", "" }, // Empty required section
                    { "Budget", "Some content" }
                    // Missing other required sections
                }
            };
            
            // Act
            var validationResult = await _statementLogic.ValidateStatementAsync(incompleteStatement);
            
            // Assert
            Assert.False(validationResult.IsValid);
            Assert.Contains(validationResult.Errors, e => e.Contains("Executive Summary"));
            Assert.Contains(validationResult.Errors, e => e.Contains("required"));
        }

        #endregion

        #region TC-OPP-STMT-POP-002: Import From DST Profile

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-STMT-POP-002")]
        public async Task ImportFromDSTProfile_ExistingProfile_PopulatesRelevantSections()
        {
            // Arrange
            var opportunityId = 1;
            
            // Mock DST profile data
            var dstProfile = new
            {
                ComplexityScore = 75,
                ContextParameter = "Fragile state with limited infrastructure",
                RiskAssessment = "High political risk, medium implementation risk",
                Recommendations = "Proceed with risk mitigation measures"
            };
            
            // Act
            var statement = await _statementLogic.ImportFromDSTProfileAsync(opportunityId, dstProfile);
            
            // Assert
            Assert.NotNull(statement);
            Assert.Contains("Fragile state", statement.Sections["Context"]);
            Assert.Contains("High political risk", statement.Sections["Risk Assessment"]);
            Assert.Contains("risk mitigation", statement.Sections["Recommendations"]);
        }

        #endregion

        #region TC-OPP-STMT-POP-003: Import From Document Extraction

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-STMT-POP-003")]
        public async Task ImportFromDocumentExtraction_ConceptNoteUploaded_ExtractsAndPopulates()
        {
            // Arrange
            var opportunityId = 1;
            var extractedData = new
            {
                Background = "Country has significant infrastructure gap...",
                Objectives = "Construct 50 schools in rural areas",
                Methodology = "Design-build approach with local contractors"
            };
            
            // Act
            var statement = await _statementLogic.ImportFromDocumentExtractionAsync(opportunityId, extractedData);
            
            // Assert
            Assert.NotNull(statement);
            Assert.Contains("infrastructure gap", statement.Sections["Background"]);
            Assert.Contains("50 schools", statement.Sections["Objectives"]);
            Assert.Contains("Design-build", statement.Sections["Methodology"]);
        }

        #endregion

        #region TC-OPP-STMT-POP-004: Import From Partnership Agreement

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-STMT-POP-004")]
        public async Task ImportFromPartnershipAgreement_ExistingAgreement_IncludesTerms()
        {
            // Arrange
            var opportunityId = 1;
            var agreementData = new
            {
                AgreementNumber = "PA-2026-001",
                ScopeOfWork = "Infrastructure development",
                PricingTerms = "Cost plus 7% management fee",
                Duration = "24 months"
            };
            
            // Act
            var statement = await _statementLogic.ImportFromPartnershipAgreementAsync(opportunityId, agreementData);
            
            // Assert
            Assert.NotNull(statement);
            Assert.Contains("PA-2026-001", statement.Sections["Agreement Reference"]);
            Assert.Contains("Cost plus 7%", statement.Sections["Pricing"]);
        }

        #endregion

        #region TC-OPP-STMT-POP-005: Merge Multiple Sources

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-STMT-POP-005")]
        public async Task MergeMultipleSources_AllDataSources_IntelligentMerge()
        {
            // Arrange
            var opportunityId = 1;
            var sources = new
            {
                OpportunityData = new { Name = "School Construction", Budget = 2500000 },
                DSTProfile = new { Complexity = "High", Risks = new[] { "Political", "Technical" } },
                DocumentExtraction = new { Background = "Extracted background text..." }
            };
            
            // Act
            var statement = await _statementLogic.MergeFromMultipleSourcesAsync(opportunityId, sources);
            
            // Assert
            Assert.NotNull(statement);
            
            // All sources contribute
            Assert.Contains("School Construction", statement.Title);
            Assert.Contains("2,500,000", statement.BudgetSummary);
            Assert.Contains("High", statement.Sections["Complexity Analysis"]);
            Assert.Contains("Extracted background", statement.Sections["Background"]);
            
            // No duplication
            var duplicateCount = statement.Sections.Values
                .SelectMany(v => v.Split(' '))
                .GroupBy(w => w)
                .Count(g => g.Count() > 3); // Check for excessive repetition
            Assert.True(duplicateCount < 5); // Minimal duplication
        }

        #endregion

        #region TC-OPP-STMT-POP-006: Handle Missing Data

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "ErrorHandling")]
        [Trait("TestId", "TC-OPP-STMT-POP-006")]
        public async Task HandleMissingData_IncompleteOpportunity_GeneratesPartialStatement()
        {
            // Arrange
            var incompleteOpportunity = new Domain.Entities.Opportunity
            {
                Id = 10,
                Name = "Incomplete Opportunity",
                // Missing: EstimatedValue, Timeline, Countries, etc.
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            
            _context.Opportunities.Add(incompleteOpportunity);
            await _context.SaveChangesAsync();
            
            // Act
            var statement = await _statementLogic.PrePopulateFromOpportunityAsync(10);
            
            // Assert
            Assert.NotNull(statement);
            
            // Missing sections have placeholders
            Assert.Contains("[TBD]", statement.BudgetSummary ?? "[TBD]");
            Assert.Contains("[TBD]", statement.TimelineSummary ?? "[TBD]");
            
            // Flags missing data
            Assert.NotEmpty(statement.MissingDataWarnings);
            Assert.Contains(statement.MissingDataWarnings, w => w.Contains("Budget"));
            Assert.Contains(statement.MissingDataWarnings, w => w.Contains("Timeline"));
        }

        #endregion

        #region TC-OPP-STMT-NAR-001 to NAR-005: Narrative Generation

        [Theory]
        [InlineData("Executive Summary", 250, "overview")]
        [InlineData("Background", 600, "context")]
        [InlineData("Methodology", 400, "approach")]
        [InlineData("Risk Assessment", 300, "risks")]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-STMT-NAR-001-005")]
        public async Task GenerateNarrativeSection_VariousSections_AIGeneratesAppropriateContent(
            string sectionName, int minWords, string expectedKeyword)
        {
            // Arrange
            var opportunityId = 1;
            
            // Mock AI service for each section
            _mockAIService.Setup(ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync($"Generated {sectionName} content with {expectedKeyword} and comprehensive details...");
            
            // Act
            var narrative = await _statementLogic.GenerateNarrativeForSectionAsync(opportunityId, sectionName);
            
            // Assert
            Assert.NotNull(narrative);
            Assert.Contains(expectedKeyword, narrative.ToLower());
            
            // Check word count
            var wordCount = narrative.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.True(wordCount >= minWords * 0.8m); // At least 80% of target
            
            // AI service was called
            _mockAIService.Verify(
                ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region TC-OPP-STMT-NAR-005: Maintain Consistency

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-STMT-NAR-005")]
        public async Task ValidateConsistency_EntireStatement_DetectsContradictions()
        {
            // Arrange
            var statement = new OpportunityStatement
            {
                OpportunityId = 1,
                Title = "Test Statement",
                BudgetSummary = "$2,500,000",
                Sections = new Dictionary<string, string>
                {
                    { "Executive Summary", "Budget of $2.5M over 24 months" },
                    { "Budget Detail", "Total cost $3,000,000 over 18 months" }, // Contradiction!
                    { "Timeline", "Duration: 24 months" }
                }
            };
            
            // Act
            var consistencyCheck = await _statementLogic.ValidateConsistencyAsync(statement);
            
            // Assert
            Assert.False(consistencyCheck.IsConsistent);
            Assert.Contains(consistencyCheck.Inconsistencies, i => i.Contains("Budget"));
            Assert.Contains(consistencyCheck.Inconsistencies, i => i.Contains("$2.5M") || i.Contains("$3.0M"));
        }

        #endregion

        #region TC-OPP-STMT-CN-001 to CN-004: Concept Note Creation

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-CN-001")]
        public async Task GeneratePartnerFacingConceptNote_SimplifiedLanguage_PartnerFriendly()
        {
            // Arrange
            var opportunityId = 1;
            
            // Act
            var conceptNote = await _statementLogic.GeneratePartnerFacingConceptNoteAsync(opportunityId);
            
            // Assert
            Assert.NotNull(conceptNote);
            
            // Simplified language (less jargon)
            Assert.DoesNotContain("fiduciary", conceptNote.Content.ToLower());
            Assert.DoesNotContain("m&e framework", conceptNote.Content.ToLower());
            
            // Partner-focused benefits
            Assert.Contains("benefit", conceptNote.Content.ToLower());
            Assert.Contains("outcome", conceptNote.Content.ToLower());
            
            // Professional formatting
            Assert.True(conceptNote.Content.Length > 500);
        }

        [Theory]
        [InlineData("World Bank", "Results Framework")]
        [InlineData("UN Agency", "SDG Alignment")]
        [InlineData("Government", "National Development Plan")]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-CN-002")]
        public async Task TailorToPartnerFormat_VariousFormats_AdaptsStructure(string partnerType, string expectedSection)
        {
            // Arrange
            var opportunityId = 1;
            
            // Act
            var conceptNote = await _statementLogic.GenerateConceptNoteForPartnerTypeAsync(opportunityId, partnerType);
            
            // Assert
            Assert.NotNull(conceptNote);
            Assert.Contains(expectedSection, conceptNote.Sections.Keys);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-CN-003")]
        public async Task IncludeVisuals_ChartGeneration_CreatesCharts()
        {
            // Arrange
            var opportunityId = 1;
            
            // Act
            var conceptNote = await _statementLogic.GenerateConceptNoteWithVisualsAsync(opportunityId);
            
            // Assert
            Assert.NotNull(conceptNote);
            Assert.NotNull(conceptNote.Visuals);
            Assert.Contains(conceptNote.Visuals, v => v.Type == "Timeline Chart");
            Assert.Contains(conceptNote.Visuals, v => v.Type == "Budget Pie Chart");
            Assert.All(conceptNote.Visuals, v => Assert.NotNull(v.ImageData));
        }

        [Theory]
        [InlineData("PDF", "application/pdf")]
        [InlineData("DOCX", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("HTML", "text/html")]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-STMT-CN-004")]
        public async Task ExportConceptNote_VariousFormats_GeneratesCorrectFormat(string format, string expectedMimeType)
        {
            // Arrange
            var opportunityId = 1;
            var conceptNote = await _statementLogic.GeneratePartnerFacingConceptNoteAsync(opportunityId);
            
            // Act
            var exportResult = await _statementLogic.ExportConceptNoteAsync(conceptNote.Id, format);
            
            // Assert
            Assert.NotNull(exportResult);
            Assert.Equal(expectedMimeType, exportResult.ContentType);
            Assert.True(exportResult.FileBytes.Length > 0);
        }

        #endregion

        #region Helper Classes

        public class OpportunityStatement
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Title { get; set; }
            public int Version { get; set; }
            public Dictionary<string, string> Sections { get; set; }
            public string BudgetSummary { get; set; }
            public string TimelineSummary { get; set; }
            public decimal PopulationPercentage { get; set; }
            public List<string> MissingDataWarnings { get; set; } = new List<string>();
            public bool IsCustom { get; set; }
            public string Content { get; set; }
            public List<Visual> Visuals { get; set; } = new List<Visual>();
        }

        public class PDFExportResult
        {
            public byte[] FileBytes { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        public class ConsistencyCheckResult
        {
            public bool IsConsistent { get; set; }
            public List<string> Inconsistencies { get; set; } = new List<string>();
        }

        public class ConceptNote
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Content { get; set; }
            public Dictionary<string, string> Sections { get; set; } = new Dictionary<string, string>();
            public List<Visual> Visuals { get; set; } = new List<Visual>();
        }

        public class Visual
        {
            public string Type { get; set; }
            public byte[] ImageData { get; set; }
        }

        public class StatementTemplate
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<string> Sections { get; set; } = new List<string>();
            public bool IsCustom { get; set; }
            public int Version { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
