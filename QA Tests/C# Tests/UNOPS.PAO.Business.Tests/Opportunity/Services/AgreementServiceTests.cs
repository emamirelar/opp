using Moq;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSBusiness.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Services
{
    public class AgreementServiceTests
    {
        private readonly Mock<IAgreementManager> _mockAgreementManager;
        private readonly Mock<IAIService> _mockAIService;
        private readonly AgreementService _service;

        public AgreementServiceTests()
        {
            _mockAgreementManager = new Mock<IAgreementManager>();
            _mockAIService = new Mock<IAIService>();
            _service = new AgreementService(_mockAgreementManager.Object, _mockAIService.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-001")]
        public async Task ProcessAgreement_ExtractTerms_Success()
        {
            var documentContent = "Test agreement content";
            var terms = new AgreementTerms { FeePercentage = 8m };
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(documentContent)).ReturnsAsync(terms);

            var result = await _service.ProcessAndExtractAsync(documentContent);

            Assert.NotNull(result);
            Assert.Equal(8m, result.FeePercentage);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-002")]
        public async Task ValidateOpportunityAgainstAgreement_WithinTerms_Valid()
        {
            // Arrange
            var opportunityId = 1;
            var agreementId = 1;
            
            _mockAgreementManager.Setup(m => m.ValidateOpportunityAsync(opportunityId, agreementId))
                .ReturnsAsync(true);

            // Act
            var isValid = await _service.ValidateOpportunityAgainstAgreementAsync(opportunityId, agreementId);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-003")]
        public async Task ValidateOpportunity_GeographyMismatch_ReturnsInvalid()
        {
            // Arrange
            var opportunityId = 1;
            var agreementId = 1;
            
            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new System.Collections.Generic.List<string>
                {
                    "Opportunity geography (Kenya) not covered by agreement (Bangladesh, Nepal)"
                }
            };

            _mockAgreementManager.Setup(m => m.ValidateOpportunityDetailedAsync(opportunityId, agreementId))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _service.ValidateOpportunityDetailedAsync(opportunityId, agreementId);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("geography", result.Errors[0], System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-004")]
        public async Task ValidateOpportunity_BudgetExceedsCeiling_ReturnsWarning()
        {
            // Arrange
            var opportunityId = 1;
            var agreementId = 1;
            
            var validationResult = new ValidationResult
            {
                IsValid = true,
                Warnings = new System.Collections.Generic.List<string>
                {
                    "Opportunity budget ($5M) exceeds annual ceiling ($3M) - requires approval"
                }
            };

            _mockAgreementManager.Setup(m => m.ValidateOpportunityDetailedAsync(opportunityId, agreementId))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _service.ValidateOpportunityDetailedAsync(opportunityId, agreementId);

            // Assert
            Assert.True(result.IsValid);
            Assert.Single(result.Warnings);
            Assert.Contains("ceiling", result.Warnings[0], System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-005")]
        public async Task ExtractAndCompareTerms_MultipleAgreements_IdentifiesBestMatch()
        {
            // Arrange
            var opportunityId = 1;
            var agreementIds = new[] { 1, 2, 3 };

            var matchScores = new System.Collections.Generic.List<AgreementMatchScore>
            {
                new AgreementMatchScore { AgreementId = 1, MatchScore = 0.92m, MatchReason = "Geography and scope fully aligned" },
                new AgreementMatchScore { AgreementId = 2, MatchScore = 0.75m, MatchReason = "Geography matches, scope partial" },
                new AgreementMatchScore { AgreementId = 3, MatchScore = 0.58m, MatchReason = "Geographic overlap only" }
            };

            _mockAIService.Setup(ai => ai.CompareAgreementsForOpportunityAsync(opportunityId, It.IsAny<object>()))
                .ReturnsAsync(matchScores);

            // Act
            var result = await _service.FindBestMatchingAgreementAsync(opportunityId, agreementIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BestMatchAgreementId);
            Assert.True(result.MatchScore > 0.9m);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-006")]
        public async Task AutoLinkAgreement_BasedOnPartner_Success()
        {
            // Arrange
            var opportunityId = 1;
            var partnerId = 5;

            var suggestedAgreements = new System.Collections.Generic.List<AgreementSuggestion>
            {
                new AgreementSuggestion
                {
                    AgreementId = 10,
                    AgreementName = "Framework Agreement with Partner X",
                    ConfidenceScore = 0.95m,
                    Reason = "Partner match + geographic alignment"
                }
            };

            _mockAgreementManager.Setup(m => m.SuggestAgreementsAsync(opportunityId))
                .ReturnsAsync(suggestedAgreements);

            // Act
            var result = await _service.AutoLinkBestAgreementAsync(opportunityId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.LinkedAgreementId);
            Assert.True(result.ConfidenceScore > 0.9m);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGMT-SVC-F-007")]
        public async Task ExtractTermsFromScannedPDF_OCRAndParse_Success()
        {
            // Arrange
            var scannedPDFContent = "[Binary image data representing scanned agreement]";
            
            var extractedTerms = new AgreementTerms
            {
                FeePercentage = 8m,
                Geography = new[] { "Bangladesh", "Nepal" },
                Validity = 3,
                Partner = "UNDP"
            };

            _mockAIService.Setup(ai => ai.ExtractFromScannedDocumentAsync(scannedPDFContent))
                .ReturnsAsync(extractedTerms);

            // Act
            var result = await _service.ProcessScannedAgreementAsync(scannedPDFContent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8m, result.FeePercentage);
            Assert.Equal(2, result.Geography.Length);
            Assert.Equal("UNDP", result.Partner);
        }

        public class AgreementTerms
        {
            public decimal? FeePercentage { get; set; }
            public string[] Geography { get; set; }
            public int? Validity { get; set; }
            public string Partner { get; set; }
        }

        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public System.Collections.Generic.List<string> Errors { get; set; } = new System.Collections.Generic.List<string>();
            public System.Collections.Generic.List<string> Warnings { get; set; } = new System.Collections.Generic.List<string>();
        }

        public class AgreementMatchScore
        {
            public int AgreementId { get; set; }
            public decimal MatchScore { get; set; }
            public string MatchReason { get; set; }
        }

        public class BestMatchResult
        {
            public int BestMatchAgreementId { get; set; }
            public decimal MatchScore { get; set; }
        }

        public class AgreementSuggestion
        {
            public int AgreementId { get; set; }
            public string AgreementName { get; set; }
            public decimal ConfidenceScore { get; set; }
            public string Reason { get; set; }
        }

        public class AutoLinkResult
        {
            public bool Success { get; set; }
            public int LinkedAgreementId { get; set; }
            public decimal ConfidenceScore { get; set; }
        }
    }
}
