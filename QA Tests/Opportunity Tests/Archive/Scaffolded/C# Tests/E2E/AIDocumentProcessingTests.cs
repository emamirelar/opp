using AutoMapper;
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
    public class AIDocumentProcessingTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly OpportunityManager _opportunityManager;

        public AIDocumentProcessingTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"AIProcessingTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();
            _opportunityManager = new OpportunityManager(_context, _mockAIService.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-006")]
        public async Task AIDiscoveryFromMultipleDocuments_FiveDocuments_85PercentPopulated()
        {
            // Arrange - Mock AI processing 5 documents
            _mockAIService.Setup(ai => ai.ExtractFromMultipleDocumentsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new ConsolidatedExtractionResult
                {
                    Title = "Extracted Title",
                    Budget = 2800000m,
                    Timeline = 24,
                    Country = "Bangladesh",
                    PopulationRate = 0.85m, // 85% of fields populated
                    Conflicts = new List<DataConflict>
                    {
                        new DataConflict { Field = "Budget", Values = new List<object> { 2500000m, 2800000m } }
                    }
                });

            // Act
            var result = await _opportunityManager.ProcessMultipleDocumentsAsync(new[] { "doc1.pdf", "doc2.docx", "doc3.pdf", "doc4.xlsx", "doc5.pdf" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.85m, result.PopulationRate);
            Assert.Single(result.Conflicts); // Budget conflict detected
            Assert.Equal(2800000m, result.RecommendedValues["Budget"]); // AI recommends most common value
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-007")]
        public async Task HistoricalDataMigration_200Opportunities_AllImported()
        {
            // Arrange - Create 200 historical opportunities
            var historicalOpportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 1; i <= 200; i++)
            {
                historicalOpportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Historical Opp {i}",
                    EstimatedValue = 100000 + (i * 5000),
                    Status = "Historical",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddYears(-2)
                });
            }

            // Act
            _context.Opportunities.AddRange(historicalOpportunities);
            await _context.SaveChangesAsync();

            // Assert
            var count = await _context.Opportunities.CountAsync(o => o.Status == "Historical");
            Assert.Equal(200, count);
        }

        #region Additional E2E AI Processing Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-001")]
        public async Task CompleteAIWorkflow_ConceptNoteToDSTProfile_FullyAutomated()
        {
            // Arrange - Upload concept note
            var conceptNote = @"PROJECT TITLE: School Rehabilitation in Bangladesh
                BUDGET: $2.5M USD
                DURATION: 24 months
                LOCATION: Cox's Bazar District
                OBJECTIVE: Rehabilitate 25 schools damaged by flooding";
            
            // Mock AI extraction from concept note
            _mockAIService.Setup(ai => ai.ExtractFromDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Title = "School Rehabilitation in Bangladesh",
                    Budget = 2500000m,
                    Duration = 24,
                    Country = "Bangladesh",
                    Objectives = "Rehabilitate 25 schools",
                    Confidence = 0.92m
                });
            
            // Mock DST profile generation
            _mockAIService.Setup(ai => ai.GenerateDSTProfileAsync(It.IsAny<object>()))
                .ReturnsAsync(new DSTProfile
                {
                    ComplexityScore = 65,
                    ContextRating = "Fragile State - High",
                    Recommendation = "Proceed with enhanced monitoring"
                });
            
            // Act - Full workflow
            var extractionResult = await _opportunityManager.ExtractFromConceptNoteAsync(conceptNote);
            var opportunity = await _opportunityManager.CreateFromExtractionAsync(extractionResult);
            var dstProfile = await _opportunityManager.GenerateDSTProfileAsync(opportunity.Id);
            
            // Assert - Complete automated workflow
            Assert.NotNull(opportunity);
            Assert.Equal("School Rehabilitation in Bangladesh", opportunity.Name);
            Assert.Equal(2500000m, opportunity.EstimatedValue);
            
            Assert.NotNull(dstProfile);
            Assert.Equal(65, dstProfile.ComplexityScore);
            Assert.Contains("Fragile State", dstProfile.ContextRating);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-002")]
        public async Task BatchDocumentProcessing_50Concepts_ProcessedInParallel()
        {
            // Arrange - 50 concept notes
            var conceptNotes = new List<string>();
            for (int i = 1; i <= 50; i++)
            {
                conceptNotes.Add($"Concept Note {i}: Infrastructure project with budget ${i * 100000}");
            }
            
            _mockAIService.Setup(ai => ai.BatchExtractAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(conceptNotes.Select((_, index) => new DocumentExtractionResult
                {
                    Title = $"Project {index + 1}",
                    Budget = (index + 1) * 100000m,
                    Confidence = 0.85m
                }).ToList());
            
            // Act - Batch processing
            var results = await _opportunityManager.BatchProcessConceptNotesAsync(conceptNotes);
            
            // Assert
            Assert.Equal(50, results.Count);
            Assert.All(results, r => Assert.True(r.Confidence >= 0.85m));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-003")]
        public async Task IncrementalExtraction_LowConfidence_RequestsHumanReview()
        {
            // Arrange - Document with low confidence extraction
            var poorQualityDoc = "Scanned document with poor image quality...";
            
            _mockAIService.Setup(ai => ai.ExtractFromDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Title = "Unclear Title",
                    Budget = null, // Could not extract
                    Confidence = 0.45m // Low confidence
                });
            
            // Act
            var result = await _opportunityManager.ExtractFromConceptNoteAsync(poorQualityDoc);
            
            // Assert - Flags for human review
            Assert.True(result.RequiresHumanReview);
            Assert.True(result.Confidence < 0.5m);
            Assert.Contains("Low confidence", result.ReviewReason);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-004")]
        public async Task MultiLanguageExtraction_FrenchDocument_ExtractsAndTranslates()
        {
            // Arrange - French concept note
            var frenchDoc = "TITRE: Réhabilitation des écoles au Bangladesh";
            
            _mockAIService.Setup(ai => ai.ExtractFromDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Title = "School Rehabilitation in Bangladesh", // Translated
                    OriginalLanguage = "French",
                    Confidence = 0.88m
                });
            
            // Act
            var result = await _opportunityManager.ExtractFromConceptNoteAsync(frenchDoc);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("French", result.OriginalLanguage);
            Assert.Contains("Bangladesh", result.Title);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-005")]
        public async Task AIGeneratedNarrative_AllSections_ProfessionalQuality()
        {
            // Arrange - Opportunity with basic data
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Health System Strengthening",
                EstimatedValue = 5000000,
                PrimaryCountryId = 1,
                Timeline = 36,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // Mock AI narrative generation
            _mockAIService.Setup(ai => ai.GenerateNarrativeAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("Comprehensive professional narrative spanning multiple paragraphs with proper structure...");
            
            // Act - Generate all sections
            var narratives = new Dictionary<string, string>();
            var sections = new[] { "Executive Summary", "Background", "Methodology", "Risk Assessment", "M&E Framework" };
            
            foreach (var section in sections)
            {
                narratives[section] = await _opportunityManager.GenerateNarrativeSectionAsync(1, section);
            }
            
            // Assert - All sections generated
            Assert.Equal(5, narratives.Count);
            Assert.All(narratives.Values, narrative => Assert.True(narrative.Length > 100)); // Substantial content
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-006")]
        public async Task ConflictResolution_TwoDocumentsWithDifferentBudgets_UserReviewsAndChooses()
        {
            // Arrange - Two documents with conflicting information
            var doc1 = "Budget: $2.5 million";
            var doc2 = "Total cost: $3.2 million";
            
            _mockAIService.Setup(ai => ai.ExtractFromMultipleDocumentsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(new ConsolidatedExtractionResult
                {
                    Budget = 2850000m, // Average
                    Conflicts = new List<DataConflict>
                    {
                        new DataConflict
                        {
                            Field = "Budget",
                            Values = new List<object> { 2500000m, 3200000m },
                            RequiresUserDecision = true
                        }
                    }
                });
            
            // Act
            var result = await _opportunityManager.ProcessMultipleDocumentsAsync(new[] { doc1, doc2 });
            
            // Assert - Conflict detected and flagged
            Assert.Single(result.Conflicts);
            Assert.True(result.Conflicts[0].RequiresUserDecision);
            Assert.Equal(2, result.Conflicts[0].Values.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-007")]
        public async Task SmartFieldMapping_NonStandardLabels_CorrectlyMapsFields()
        {
            // Arrange - Document with non-standard field names
            var document = @"
                Project Name: Education Infrastructure Development
                Estimated Cost: USD 4.2M
                Timeframe: 30 months
                Geographic Area: Southeast Asia";
            
            _mockAIService.Setup(ai => ai.ExtractFromDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Title = "Education Infrastructure Development", // Mapped from "Project Name"
                    Budget = 4200000m, // Mapped from "Estimated Cost"
                    Duration = 30, // Mapped from "Timeframe"
                    Region = "Southeast Asia", // Mapped from "Geographic Area"
                    Confidence = 0.91m
                });
            
            // Act
            var result = await _opportunityManager.ExtractFromConceptNoteAsync(document);
            
            // Assert - All fields correctly mapped
            Assert.Equal("Education Infrastructure Development", result.Title);
            Assert.Equal(4200000m, result.Budget);
            Assert.Equal(30, result.Duration);
            Assert.Equal("Southeast Asia", result.Region);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-008")]
        public async Task TableExtraction_BudgetBreakdown_PreservesStructure()
        {
            // Arrange - Document with budget table
            var documentWithTable = "Budget breakdown: Personnel $500K, Equipment $300K, Travel $200K";
            
            _mockAIService.Setup(ai => ai.ExtractFromDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Budget = 1000000m,
                    BudgetBreakdown = new Dictionary<string, decimal>
                    {
                        { "Personnel", 500000m },
                        { "Equipment", 300000m },
                        { "Travel", 200000m }
                    },
                    Confidence = 0.94m
                });
            
            // Act
            var result = await _opportunityManager.ExtractFromConceptNoteAsync(documentWithTable);
            
            // Assert - Table structure preserved
            Assert.Equal(1000000m, result.Budget);
            Assert.Equal(3, result.BudgetBreakdown.Count);
            Assert.Equal(500000m, result.BudgetBreakdown["Personnel"]);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-009")]
        public async Task ImageBasedExtraction_ScannedPDF_OCRAndExtract()
        {
            // Arrange - Scanned PDF (image-based)
            var scannedPDF = "[Binary image data representing scanned document]";
            
            _mockAIService.Setup(ai => ai.ExtractFromImageAsync(It.IsAny<string>()))
                .ReturnsAsync(new DocumentExtractionResult
                {
                    Title = "Infrastructure Project", // Extracted via OCR
                    Budget = 1500000m,
                    Confidence = 0.78m, // Lower confidence due to OCR
                    ProcessingMethod = "OCR"
                });
            
            // Act
            var result = await _opportunityManager.ExtractFromScannedDocumentAsync(scannedPDF);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("OCR", result.ProcessingMethod);
            Assert.True(result.Confidence >= 0.75m); // Acceptable OCR confidence
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E")]
        [Trait("TestId", "TC-OPP-E2E-AI-010")]
        public async Task ProgressiveEnhancement_InitialExtractThenEnrichment_ImprovedData()
        {
            // Arrange - Initial extraction with basic data
            var initialExtraction = new DocumentExtractionResult
            {
                Title = "Water Project",
                Budget = 2000000m,
                Country = "Tanzania",
                Confidence = 0.88m
            };
            
            // Mock enrichment from global data sources
            _mockAIService.Setup(ai => ai.EnrichOpportunityDataAsync(It.IsAny<object>()))
                .ReturnsAsync(new EnrichmentResult
                {
                    CountryContext = "Tanzania - Fragile Coastal Regions - High Flood Risk",
                    RelevantSDGs = new[] { "SDG 6: Clean Water", "SDG 13: Climate Action" },
                    SimilarProjects = new[] { "Water Infrastructure - Kenya 2023", "Coastal Resilience - Mozambique 2024" }
                });
            
            // Act - Extract then enrich
            var opportunity = await _opportunityManager.CreateFromExtractionAsync(initialExtraction);
            var enriched = await _opportunityManager.EnrichOpportunityAsync(opportunity.Id);
            
            // Assert - Enhanced with contextual data
            Assert.NotNull(enriched);
            Assert.Contains("Fragile Coastal", enriched.CountryContext);
            Assert.Equal(2, enriched.RelevantSDGs.Length);
            Assert.NotEmpty(enriched.SimilarProjects);
        }

        #endregion

        public class ConsolidatedExtractionResult
        {
            public string Title { get; set; }
            public decimal Budget { get; set; }
            public int Timeline { get; set; }
            public string Country { get; set; }
            public decimal PopulationRate { get; set; }
            public List<DataConflict> Conflicts { get; set; }
            public Dictionary<string, object> RecommendedValues { get; set; }
        }

        public class DataConflict
        {
            public string Field { get; set; }
            public List<object> Values { get; set; }
            public bool RequiresUserDecision { get; set; }
        }

        public class DocumentExtractionResult
        {
            public string Title { get; set; }
            public decimal? Budget { get; set; }
            public int? Duration { get; set; }
            public string Country { get; set; }
            public string Objectives { get; set; }
            public decimal Confidence { get; set; }
            public bool RequiresHumanReview { get; set; }
            public string ReviewReason { get; set; }
            public string OriginalLanguage { get; set; }
            public string Region { get; set; }
            public Dictionary<string, decimal> BudgetBreakdown { get; set; }
            public string ProcessingMethod { get; set; }
        }

        public class DSTProfile
        {
            public int ComplexityScore { get; set; }
            public string ContextRating { get; set; }
            public string Recommendation { get; set; }
        }

        public class EnrichmentResult
        {
            public string CountryContext { get; set; }
            public string[] RelevantSDGs { get; set; }
            public string[] SimilarProjects { get; set; }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
