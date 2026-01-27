using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.BusinessLogic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.BusinessLogic
{
    /// <summary>
    /// Tests for AI-powered document extraction logic
    /// Based on DocumentExtraction_TestCases.md (30+ tests)
    /// </summary>
    public class DocumentExtractionTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly Mock<IDocumentStorageService> _mockStorageService;
        private readonly DocumentExtractionLogic _extractionLogic;

        public DocumentExtractionTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"DocExtractionTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();
            _mockStorageService = new Mock<IDocumentStorageService>();

            _extractionLogic = new DocumentExtractionLogic(
                _context,
                _mockAIService.Object,
                _mockStorageService.Object
            );

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Document Test Opportunity",
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        #region TC-OPP-DOC-F-001: Upload and Extract from Concept Note

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-001")]
        public async Task ExtractFromConceptNote_ValidPDF_ExtractsStructuredData()
        {
            // Arrange
            var opportunityId = 1;
            var documentContent = @"
                Project Title: Water Infrastructure Development
                Location: Bangladesh, Dhaka District
                Budget: USD 2,500,000
                Duration: 24 months
                Objectives: Improve water access for 50,000 beneficiaries
                Partners: Government of Bangladesh, Local NGO
                SDGs: SDG 6 (Clean Water), SDG 11 (Sustainable Cities)
            ";

            // Mock AI extraction response
            _mockAIService.Setup(ai => ai.ExtractStructuredDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExtractedData
                {
                    Title = "Water Infrastructure Development",
                    Country = "Bangladesh",
                    Budget = 2500000m,
                    Currency = "USD",
                    Timeline = 24,
                    Objectives = new List<string> { "Improve water access for 50,000 beneficiaries" },
                    Partners = new List<string> { "Government of Bangladesh", "Local NGO" },
                    SDGs = new List<int> { 6, 11 },
                    ConfidenceScore = 0.92m // 92% confidence
                });

            // Act
            var extraction = await _extractionLogic.ExtractFromDocumentAsync(
                opportunityId,
                documentContent,
                "concept-note.pdf");

            // Assert
            Assert.NotNull(extraction);
            Assert.Equal(0.92m, extraction.ConfidenceScore);
            Assert.Equal("Water Infrastructure Development", extraction.ExtractedFields["Title"]);
            Assert.Equal("Bangladesh", extraction.ExtractedFields["Country"]);
            Assert.Equal(2500000m, extraction.ExtractedFields["Budget"]);
            Assert.Equal(2, extraction.ExtractedSDGs.Count);
            Assert.Contains(6, extraction.ExtractedSDGs);
            Assert.Contains(11, extraction.ExtractedSDGs);
        }

        #endregion

        #region TC-OPP-DOC-F-002: Field Mapping and Confidence Scores

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-002")]
        public async Task ExtractWithConfidenceScores_VariousFields_ReturnsScores()
        {
            // Arrange
            var documentContent = "Mixed quality content...";

            // Mock AI with varying confidence
            _mockAIService.Setup(ai => ai.ExtractStructuredDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExtractedData
                {
                    FieldConfidences = new Dictionary<string, decimal>
                    {
                        { "Title", 0.98m }, // Very high confidence
                        { "Budget", 0.95m }, // High
                        { "Timeline", 0.88m }, // Good
                        { "Partners", 0.72m }, // Moderate
                        { "Deliverables", 0.55m } // Low - needs review
                    }
                });

            // Act
            var extraction = await _extractionLogic.ExtractFromDocumentAsync(1, documentContent, "test.pdf");

            // Assert
            Assert.NotNull(extraction.FieldConfidences);
            
            // High confidence fields
            Assert.True(extraction.FieldConfidences["Title"] > 0.90m);
            Assert.True(extraction.FieldConfidences["Budget"] > 0.90m);
            
            // Low confidence fields flagged for review
            var lowConfidenceFields = extraction.FieldConfidences.Where(kv => kv.Value < 0.75m).ToList();
            Assert.Contains(lowConfidenceFields, kv => kv.Key == "Deliverables");
            
            // User should be prompted to review low-confidence fields
            Assert.Contains(extraction.ReviewRequired, "Deliverables");
        }

        #endregion

        #region TC-OPP-DOC-F-003: Multi-Document Processing

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-003")]
        public async Task ExtractFromMultipleDocuments_CrossValidation_ConsolidatesData()
        {
            // Arrange
            var opportunityId = 1;
            
            // Document 1: Concept Note
            var doc1Extraction = new ExtractedData
            {
                Title = "Water Project",
                Budget = 2500000m,
                Timeline = 24
            };

            // Document 2: Partner MOU
            var doc2Extraction = new ExtractedData
            {
                Title = "Water Infrastructure Initiative", // Slightly different
                Budget = 2800000m, // Different budget!
                Partners = new List<string> { "Government Partner" }
            };

            // Document 3: Budget Estimate
            var doc3Extraction = new ExtractedData
            {
                Budget = 2800000m, // Confirms doc2 budget
                Currency = "USD"
            };

            // Act - Process all 3 documents
            var consolidation = await _extractionLogic.ConsolidateMultipleExtractionsAsync(
                opportunityId,
                new[] { doc1Extraction, doc2Extraction, doc3Extraction });

            // Assert
            Assert.NotNull(consolidation);
            
            // Budget conflict detected
            Assert.True(consolidation.HasConflicts);
            Assert.Contains(consolidation.Conflicts, c => c.Field == "Budget");
            Assert.Equal(2, consolidation.Conflicts.First(c => c.Field == "Budget").Values.Count);
            Assert.Contains(2500000m, consolidation.Conflicts.First(c => c.Field == "Budget").Values);
            Assert.Contains(2800000m, consolidation.Conflicts.First(c => c.Field == "Budget").Values);
            
            // Most common/confident value recommended
            Assert.Equal(2800000m, consolidation.RecommendedValues["Budget"]); // 2 docs say $2.8M
        }

        #endregion

        #region TC-OPP-DOC-F-004: Verification Workflow

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-004")]
        public async Task VerifyExtractedData_UserReviewAndAccept_UpdatesOpportunity()
        {
            // Arrange
            var opportunityId = 1;
            var extraction = new ExtractedData
            {
                Title = "Extracted Title",
                Budget = 2500000m,
                Country = "Bangladesh",
                Timeline = 24,
                Objectives = new List<string> { "Objective 1", "Objective 2" }
            };

            // User reviews and accepts
            var verificationRequest = new ExtractionVerificationRequest
            {
                OpportunityId = opportunityId,
                ExtractionId = 1,
                AcceptedFields = new Dictionary<string, bool>
                {
                    { "Title", true },
                    { "Budget", true },
                    { "Country", true },
                    { "Timeline", true },
                    { "Objectives", false } // User rejects objectives - will manually enter
                },
                Corrections = new Dictionary<string, object>
                {
                    { "Objectives", new List<string> { "Corrected Objective 1", "Corrected Objective 2", "New Objective 3" } }
                }
            };

            // Act
            var result = await _extractionLogic.VerifyAndApplyExtractionAsync(verificationRequest);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(4, result.FieldsAccepted); // 4 of 5 accepted
            Assert.Equal(1, result.FieldsCorrected); // 1 corrected
            
            // Opportunity updated with verified data
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            Assert.Equal("Extracted Title", opportunity.Name);
            Assert.Equal(2500000m, opportunity.EstimatedValue);
            Assert.Equal(24, opportunity.Timeline);
            // Objectives would be updated with corrected version
        }

        #endregion

        #region TC-OPP-DOC-F-005: OCR for Scanned Documents

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-005")]
        public async Task ExtractFromScannedPDF_OCRProcessing_ExtractsText()
        {
            // Arrange
            var scannedDocument = new byte[] { /* PDF bytes */ };
            
            // Mock OCR service
            _mockAIService.Setup(ai => ai.PerformOCRAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(new OCRResult
                {
                    ExtractedText = "Project Title: School Rehabilitation\nBudget: $1,200,000",
                    Confidence = 0.87m,
                    Language = "en"
                });

            // Act
            var ocrResult = await _extractionLogic.ProcessScannedDocumentAsync(1, scannedDocument, "scanned.pdf");

            // Assert
            Assert.NotNull(ocrResult);
            Assert.Equal(0.87m, ocrResult.Confidence);
            Assert.Contains("School Rehabilitation", ocrResult.ExtractedText);
            Assert.Contains("1,200,000", ocrResult.ExtractedText);
            
            // Extracted text can now be processed for structured data
            Assert.True(ocrResult.ReadyForExtraction);
        }

        #endregion

        #region TC-OPP-DOC-V-001: Validate Supported File Types

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DOC-V-001")]
        public async Task UploadDocument_UnsupportedFileType_ThrowsException()
        {
            // Arrange
            var unsupportedFile = new DocumentUpload
            {
                FileName = "document.exe", // Executable - not allowed
                FileSize = 1024000,
                ContentType = "application/x-msdownload"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _extractionLogic.ValidateAndProcessUploadAsync(1, unsupportedFile));

            Assert.Contains("file type", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("supported formats", ex.Message, StringComparison.OrdinalIgnoreCase);
            
            // Supported formats listed: PDF, DOCX, XLSX, etc.
        }

        #endregion

        #region TC-OPP-DOC-V-002: Validate File Size Limits

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-DOC-V-002")]
        public async Task UploadDocument_ExceedsSizeLimit_ThrowsException()
        {
            // Arrange
            var oversizedFile = new DocumentUpload
            {
                FileName = "large-document.pdf",
                FileSize = 11 * 1024 * 1024, // 11 MB (limit is 10 MB)
                ContentType = "application/pdf"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _extractionLogic.ValidateAndProcessUploadAsync(1, oversizedFile));

            Assert.Contains("size limit", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("10 MB", ex.Message);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Boundary")]
        [Trait("TestId", "TC-OPP-DOC-V-002-Boundary")]
        public async Task UploadDocument_ExactlySizeLimit_Accepted()
        {
            // Arrange
            var exactSizeFile = new DocumentUpload
            {
                FileName = "document.pdf",
                FileSize = 10 * 1024 * 1024, // Exactly 10 MB
                ContentType = "application/pdf",
                Content = new byte[10 * 1024 * 1024]
            };

            // Mock storage
            _mockStorageService.Setup(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("documents/document.pdf");

            // Act
            var result = await _extractionLogic.ValidateAndProcessUploadAsync(1, exactSizeFile);

            // Assert
            Assert.True(result.Success); // Exactly at limit - accepted
        }

        #endregion

        #region TC-OPP-DOC-F-006: Handle Multiple Languages

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-DOC-F-006")]
        public async Task ExtractFromDocument_MultipleLanguages_DetectsAndExtracts()
        {
            // Arrange
            var multiLangContent = @"
                Project Title: Multilingual Project / مشروع متعدد اللغات
                Location: Middle East Region
                Budget: $3,000,000
            ";

            // Mock AI with language detection
            _mockAIService.Setup(ai => ai.DetectLanguageAsync(It.IsAny<string>()))
                .ReturnsAsync(new[] { "en", "ar" }); // English and Arabic

            _mockAIService.Setup(ai => ai.ExtractStructuredDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExtractedData
                {
                    Title = "Multilingual Project",
                    Budget = 3000000m,
                    DetectedLanguages = new[] { "en", "ar" }
                });

            // Act
            var extraction = await _extractionLogic.ExtractFromDocumentAsync(1, multiLangContent, "multilang.pdf");

            // Assert
            Assert.NotNull(extraction);
            Assert.Contains("en", extraction.DetectedLanguages);
            Assert.Contains("ar", extraction.DetectedLanguages);
            Assert.Equal("Multilingual Project", extraction.ExtractedFields["Title"]);
        }

        #endregion

        #region TC-OPP-DOC-F-007: Malicious Content Detection

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Security")]
        [Trait("TestId", "TC-OPP-DOC-F-007")]
        public async Task ValidateDocument_MaliciousContent_Rejected()
        {
            // Arrange
            var maliciousFile = new DocumentUpload
            {
                FileName = "malicious.pdf",
                FileSize = 1024000,
                ContentType = "application/pdf",
                Content = new byte[] { /* malicious content */ },
                ContainsMalware = true // Would be detected by security scan
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<SecurityException>(async () =>
                await _extractionLogic.ValidateAndProcessUploadAsync(1, maliciousFile));

            Assert.Contains("security", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("rejected", ex.Message, StringComparison.OrdinalIgnoreCase);
            
            // Security incident logged
            // Admin notified
        }

        #endregion

        #region TC-OPP-DOC-F-008: Corrupted PDF Handling

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "ErrorHandling")]
        [Trait("TestId", "TC-OPP-DOC-F-008")]
        public async Task ExtractFromDocument_CorruptedPDF_GracefulFailure()
        {
            // Arrange
            var corruptedContent = "CORRUPTED DATA %@#$%";

            // Mock AI service failing on corrupted content
            _mockAIService.Setup(ai => ai.ExtractStructuredDataAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("PDF parsing error - corrupted file"));

            // Act
            var result = await _extractionLogic.ExtractFromDocumentAsync(1, corruptedContent, "corrupted.pdf");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("corrupted", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(result.ErrorMessage); // Clear error to user
            
            // User can re-upload
            Assert.True(result.CanRetry);
        }

        #endregion

        #region TC-OPP-DOC-F-009: AI Service Timeout

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "ErrorHandling")]
        [Trait("TestId", "TC-OPP-DOC-F-009")]
        public async Task ExtractFromDocument_AITimeout_FallbackToManual()
        {
            // Arrange
            var largeDocument = new string('A', 100000); // Large document

            // Mock AI timeout
            _mockAIService.Setup(ai => ai.ExtractStructuredDataAsync(It.IsAny<string>()))
                .ThrowsAsync(new TimeoutException("AI service timeout after 30 seconds"));

            // Act
            var result = await _extractionLogic.ExtractFromDocumentAsync(
                1, 
                largeDocument, 
                "large-doc.pdf",
                timeoutSeconds: 30);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("timeout", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            
            // Fallback option provided
            Assert.True(result.ManualEntryRequired);
            Assert.NotNull(result.ManualEntryGuidance);
            Assert.Contains("manually enter", result.ManualEntryGuidance, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Helper Classes

        public class ExtractedData
        {
            public string Title { get; set; }
            public string Country { get; set; }
            public decimal? Budget { get; set; }
            public string Currency { get; set; }
            public int? Timeline { get; set; }
            public List<string> Objectives { get; set; }
            public List<string> Partners { get; set; }
            public List<int> SDGs { get; set; }
            public decimal ConfidenceScore { get; set; }
            public Dictionary<string, decimal> FieldConfidences { get; set; }
            public string[] DetectedLanguages { get; set; }
        }

        public class DocumentUpload
        {
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string ContentType { get; set; }
            public byte[] Content { get; set; }
            public bool ContainsMalware { get; set; }
        }

        public class ExtractionResult
        {
            public bool Success { get; set; }
            public decimal ConfidenceScore { get; set; }
            public Dictionary<string, object> ExtractedFields { get; set; }
            public Dictionary<string, decimal> FieldConfidences { get; set; }
            public List<string> ReviewRequired { get; set; }
            public string ErrorMessage { get; set; }
            public bool CanRetry { get; set; }
            public bool ManualEntryRequired { get; set; }
            public string ManualEntryGuidance { get; set; }
            public List<int> ExtractedSDGs { get; set; }
            public string[] DetectedLanguages { get; set; }
        }

        public class ConsolidatedExtraction
        {
            public bool HasConflicts { get; set; }
            public List<FieldConflict> Conflicts { get; set; }
            public Dictionary<string, object> RecommendedValues { get; set; }
        }

        public class FieldConflict
        {
            public string Field { get; set; }
            public List<object> Values { get; set; }
        }

        public class ExtractionVerificationRequest
        {
            public int OpportunityId { get; set; }
            public int ExtractionId { get; set; }
            public Dictionary<string, bool> AcceptedFields { get; set; }
            public Dictionary<string, object> Corrections { get; set; }
        }

        public class VerificationResult
        {
            public bool Success { get; set; }
            public int FieldsAccepted { get; set; }
            public int FieldsCorrected { get; set; }
        }

        public class OCRResult
        {
            public string ExtractedText { get; set; }
            public decimal Confidence { get; set; }
            public string Language { get; set; }
            public bool ReadyForExtraction { get; set; }
        }

        public class SecurityException : Exception
        {
            public SecurityException(string message) : base(message) { }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
