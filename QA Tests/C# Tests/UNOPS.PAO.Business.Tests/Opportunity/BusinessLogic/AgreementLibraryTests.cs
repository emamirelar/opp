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
    public class AgreementLibraryTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IAIService> _mockAIService;
        private readonly AgreementLibraryLogic _logic;

        public AgreementLibraryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"AgreementTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockAIService = new Mock<IAIService>();
            _logic = new AgreementLibraryLogic(_context, _mockAIService.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-AGMT-F-001")]
        public async Task ExtractAgreementTerms_ValidPDF_ExtractsKeyTerms()
        {
            var documentContent = "Partnership MOU - Geography: Bangladesh - Fee: 8% - Validity: 3 years";
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms { Geography = new[] { "Bangladesh" }, FeePercentage = 8m, ValidityYears = 3 });

            var terms = await _logic.ExtractTermsFromDocumentAsync(documentContent);

            Assert.Contains("Bangladesh", terms.Geography);
            Assert.Equal(8m, terms.FeePercentage);
            Assert.Equal(3, terms.ValidityYears);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-AGMT-F-002")]
        public async Task LinkAgreementToOpportunity_ValidAgreement_Success()
        {
            var agreement = new PartnershipAgreement { Id = 1, IsActive = true };
            _context.PartnershipAgreements.Add(agreement);
            var opportunity = new Domain.Entities.Opportunity { Id = 1, Name = "Test", CreatedBy = 1, CreatedDate = DateTime.UtcNow };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var link = await _logic.LinkAgreementToOpportunityAsync(1, 1);

            Assert.NotNull(link);
            var linkRecord = await _context.OpportunityAgreements.FirstOrDefaultAsync(oa => oa.OpportunityId == 1);
            Assert.NotNull(linkRecord);
        }

        #region TC-OPP-AGR-ST-001: Upload Partnership Agreement

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-ST-001")]
        public async Task UploadPartnershipAgreement_ValidPDF_StoresWithMetadata()
        {
            // Arrange
            var documentContent = @"PARTNERSHIP AGREEMENT
                Partner: World Bank
                Type: Framework Agreement
                Start Date: January 1, 2025
                End Date: December 31, 2027
                Value: $50,000,000
                Geography: Bangladesh, Nepal, Sri Lanka";
            
            // Mock AI extraction
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms
                {
                    PartnerName = "World Bank",
                    AgreementType = "Framework Agreement",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2027, 12, 31),
                    Value = 50000000,
                    Geography = new[] { "Bangladesh", "Nepal", "Sri Lanka" }
                });
            
            // Act
            var agreement = await _logic.UploadAgreementAsync(documentContent, uploadedBy: 1);
            
            // Assert
            Assert.NotNull(agreement);
            Assert.Equal("World Bank", agreement.PartnerName);
            Assert.Equal("Framework Agreement", agreement.Type);
            Assert.Equal(50000000, agreement.Value);
            Assert.True(agreement.IsFullTextSearchable);
        }

        #endregion

        #region TC-OPP-AGR-ST-002: Link Agreement to Partner

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-ST-002")]
        public async Task LinkAgreementToPartner_AutoIdentified_LinksCorrectly()
        {
            // Arrange
            var partner = new Partner { Id = 1, Name = "World Bank", IsActive = true };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            
            var agreementContent = "Partnership with World Bank...";
            
            // Act
            var agreement = await _logic.UploadAgreementAsync(agreementContent, uploadedBy: 1);
            var partnerLink = await _logic.IdentifyAndLinkPartnerAsync(agreement.Id);
            
            // Assert
            Assert.NotNull(partnerLink);
            Assert.Equal(1, partnerLink.PartnerId);
            
            // Agreement visible in partner profile
            var partnerAgreements = await _logic.GetAgreementsForPartnerAsync(1);
            Assert.Contains(partnerAgreements, a => a.Id == agreement.Id);
        }

        #endregion

        #region TC-OPP-AGR-ST-003: Agreement Versioning

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-ST-003")]
        public async Task UploadAmendment_CreatesNewVersion_TracksHistory()
        {
            // Arrange
            var originalAgreement = new PartnershipAgreement
            {
                Id = 1,
                PartnerName = "UNDP",
                Type = "Framework Agreement",
                Version = 1,
                IsActive = true
            };
            _context.PartnershipAgreements.Add(originalAgreement);
            await _context.SaveChangesAsync();
            
            var amendmentContent = "Amendment 1 to Framework Agreement with UNDP...";
            
            // Act
            var amendment = await _logic.UploadAmendmentAsync(originalAgreement.Id, amendmentContent, uploadedBy: 1);
            
            // Assert
            Assert.NotNull(amendment);
            Assert.Equal(2, amendment.Version);
            Assert.True(amendment.IsCurrentVersion);
            
            // Original version retained
            var versions = await _logic.GetAgreementVersionsAsync(originalAgreement.Id);
            Assert.Equal(2, versions.Count);
        }

        #endregion

        #region TC-OPP-AGR-ST-004: Agreement Expiration Tracking

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-ST-004")]
        public async Task TrackAgreementExpiration_Approaching_TriggersWarnings()
        {
            // Arrange
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                PartnerName = "UNICEF",
                StartDate = DateTime.UtcNow.AddYears(-2),
                EndDate = DateTime.UtcNow.AddDays(60), // Expires in 60 days
                IsActive = true
            };
            _context.PartnershipAgreements.Add(agreement);
            await _context.SaveChangesAsync();
            
            // Act
            var expiringAgreements = await _logic.GetExpiringAgreementsAsync(daysThreshold: 90);
            
            // Assert
            Assert.Contains(expiringAgreements, a => a.Id == 1);
            Assert.True(expiringAgreements.First().RequiresRenewal);
            Assert.Equal(60, expiringAgreements.First().DaysUntilExpiration);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGR-ST-004-NEG")]
        public async Task LinkExpiredAgreement_ToNewOpportunity_BlocksLinkage()
        {
            // Arrange
            var expiredAgreement = new PartnershipAgreement
            {
                Id = 1,
                PartnerName = "WHO",
                EndDate = DateTime.UtcNow.AddDays(-10), // Expired
                IsActive = false
            };
            _context.PartnershipAgreements.Add(expiredAgreement);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "New Opportunity",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => _logic.LinkAgreementToOpportunityAsync(1, 1));
        }

        #endregion

        #region TC-OPP-AGR-ST-005: Agreement Search and Retrieval

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-ST-005")]
        public async Task SearchAgreements_ByMultipleCriteria_ReturnsFiltered()
        {
            // Arrange
            _context.PartnershipAgreements.AddRange(
                new PartnershipAgreement { Id = 1, PartnerName = "UNDP", Geography = "Bangladesh", Type = "Framework", Value = 10000000 },
                new PartnershipAgreement { Id = 2, PartnerName = "World Bank", Geography = "Nepal", Type = "Service Level", Value = 5000000 },
                new PartnershipAgreement { Id = 3, PartnerName = "UNDP", Geography = "Sri Lanka", Type = "Framework", Value = 8000000 }
            );
            await _context.SaveChangesAsync();
            
            // Act - Search by partner and type
            var results = await _logic.SearchAgreementsAsync(
                partner: "UNDP",
                type: "Framework");
            
            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, a => Assert.Equal("UNDP", a.PartnerName));
            Assert.All(results, a => Assert.Equal("Framework", a.Type));
        }

        #endregion

        #region TC-OPP-AGR-EXT-001 to EXT-006: Terms Extraction

        [Theory]
        [InlineData("Valid for Bangladesh and Nepal", new[] { "Bangladesh", "Nepal" })]
        [InlineData("Geographic scope: Kenya, Tanzania, Uganda", new[] { "Kenya", "Tanzania", "Uganda" })]
        [InlineData("Applicable in all SIDS", new[] { "SIDS" })]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-001")]
        public async Task ExtractGeographicScope_VariousFormats_ExtractsCountries(string text, string[] expectedCountries)
        {
            // Arrange
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms { Geography = expectedCountries });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(text);
            
            // Assert
            Assert.Equal(expectedCountries.Length, terms.Geography.Length);
            foreach (var country in expectedCountries)
            {
                Assert.Contains(country, terms.Geography);
            }
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-002")]
        public async Task ExtractScopeOfWork_EligibleActivities_ExtractsCorrectly()
        {
            // Arrange
            var agreementText = @"Eligible activities include:
                - Infrastructure development
                - Capacity building and training
                - Procurement services
                Excluded: Political activities, religious activities";
            
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms
                {
                    EligibleActivities = new[] { "Infrastructure development", "Capacity building", "Procurement services" },
                    ExcludedActivities = new[] { "Political activities", "Religious activities" }
                });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(agreementText);
            
            // Assert
            Assert.Equal(3, terms.EligibleActivities.Length);
            Assert.Contains("Infrastructure development", terms.EligibleActivities);
            Assert.Equal(2, terms.ExcludedActivities.Length);
            Assert.Contains("Political activities", terms.ExcludedActivities);
        }

        [Theory]
        [InlineData("Management fee of 8%", 8.0)]
        [InlineData("Cost recovery at 7.5% of eligible costs", 7.5)]
        [InlineData("Fee: 10 percent", 10.0)]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-003")]
        public async Task ExtractPricingTerms_VariousFormats_ExtractsFeePercentage(string text, decimal expectedFee)
        {
            // Arrange
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms { FeePercentage = expectedFee });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(text);
            
            // Assert
            Assert.Equal(expectedFee, terms.FeePercentage);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-004")]
        public async Task ExtractDurationAndValidity_MultipleFormats_ExtractsDates()
        {
            // Arrange
            var agreementText = "Agreement valid from January 1, 2025 to December 31, 2027";
            
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms
                {
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2027, 12, 31),
                    ValidityYears = 3
                });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(agreementText);
            
            // Assert
            Assert.Equal(new DateTime(2025, 1, 1), terms.StartDate);
            Assert.Equal(new DateTime(2027, 12, 31), terms.EndDate);
            Assert.Equal(3, terms.ValidityYears);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-005")]
        public async Task ExtractAuthorizedSignatories_MultipleSignatures_ExtractsAll()
        {
            // Arrange
            var agreementText = @"Signed by:
                John Smith, Director, UNOPS
                Jane Doe, Country Director, Partner Organization";
            
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms
                {
                    Signatories = new[]
                    {
                        new Signatory { Name = "John Smith", Title = "Director", Organization = "UNOPS" },
                        new Signatory { Name = "Jane Doe", Title = "Country Director", Organization = "Partner Organization" }
                    }
                });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(agreementText);
            
            // Assert
            Assert.Equal(2, terms.Signatories.Length);
            Assert.Contains(terms.Signatories, s => s.Name == "John Smith" && s.Organization == "UNOPS");
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "AI")]
        [Trait("TestId", "TC-OPP-AGR-EXT-006")]
        public async Task ExtractFinancialCeilings_VariousCaps_ExtractsCorrectly()
        {
            // Arrange
            var agreementText = "Maximum aggregate value: $100 million. Annual cap: $25 million per year.";
            
            _mockAIService.Setup(ai => ai.ExtractAgreementTermsAsync(It.IsAny<string>()))
                .ReturnsAsync(new AgreementTerms
                {
                    MaxAggregatValue = 100000000,
                    AnnualCap = 25000000
                });
            
            // Act
            var terms = await _logic.ExtractTermsFromDocumentAsync(agreementText);
            
            // Assert
            Assert.Equal(100000000, terms.MaxAggregateValue);
            Assert.Equal(25000000, terms.AnnualCap);
        }

        #endregion

        #region TC-OPP-AGR-LINK-001 to LINK-004: Linkage Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGR-LINK-001")]
        public async Task ValidateOpportunityAgainstAgreement_GeographyMismatch_FlagsError()
        {
            // Arrange
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                Geography = "Bangladesh,Nepal",
                IsActive = true
            };
            _context.PartnershipAgreements.Add(agreement);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Kenya Project", // Wrong geography!
                PrimaryCountryId = 110, // Kenya
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // Act
            var validation = await _logic.ValidateOpportunityAgainstAgreementAsync(1, 1);
            
            // Assert
            Assert.False(validation.IsValid);
            Assert.Contains(validation.Errors, e => e.Contains("geography") || e.Contains("Kenya"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-AGR-LINK-002")]
        public async Task ValidateOpportunityValue_ExceedsCeiling_FlagsWarning()
        {
            // Arrange
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                AnnualCap = 10000000, // $10M annual cap
                IsActive = true
            };
            _context.PartnershipAgreements.Add(agreement);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Large Project",
                EstimatedValue = 15000000, // $15M - exceeds cap!
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // Act
            var validation = await _logic.ValidateOpportunityAgainstAgreementAsync(1, 1);
            
            // Assert
            Assert.True(validation.HasWarnings);
            Assert.Contains(validation.Warnings, w => w.Contains("ceiling") || w.Contains("cap"));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-LINK-003")]
        public async Task AutosuggestAgreements_BasedOnOpportunityCharacteristics_SuggestsRelevant()
        {
            // Arrange
            _context.PartnershipAgreements.AddRange(
                new PartnershipAgreement { Id = 1, PartnerName = "UNDP", Geography = "Bangladesh", Type = "Infrastructure", IsActive = true },
                new PartnershipAgreement { Id = 2, PartnerName = "World Bank", Geography = "Nepal", Type = "Health", IsActive = true },
                new PartnershipAgreement { Id = 3, PartnerName = "UNDP", Geography = "Bangladesh", Type = "Education", IsActive = true }
            );
            await _context.SaveChangesAsync();
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Bangladesh Infrastructure Project",
                PrimaryCountryId = 18, // Bangladesh
                Sector = "Infrastructure",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            
            // Act
            var suggestions = await _logic.AutosuggestAgreementsAsync(opportunity);
            
            // Assert
            Assert.NotEmpty(suggestions);
            Assert.Contains(suggestions, s => s.Id == 1); // UNDP Bangladesh Infrastructure
            Assert.All(suggestions, s => Assert.Contains("Bangladesh", s.Geography));
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-AGR-LINK-004")]
        public async Task LinkMultipleAgreements_ToSingleOpportunity_AllowsMultiple()
        {
            // Arrange
            _context.PartnershipAgreements.AddRange(
                new PartnershipAgreement { Id = 1, PartnerName = "UNDP", IsActive = true },
                new PartnershipAgreement { Id = 2, PartnerName = "World Bank", IsActive = true }
            );
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Multi-Partner Project",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            // Act - Link both agreements
            await _logic.LinkAgreementToOpportunityAsync(1, 1);
            await _logic.LinkAgreementToOpportunityAsync(1, 2);
            
            // Assert
            var linkedAgreements = await _logic.GetLinkedAgreementsAsync(1);
            Assert.Equal(2, linkedAgreements.Count);
        }

        #endregion

        #region TC-OPP-AGR-PREP-001 to PREP-005: Pre-Population Tests

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-AGR-PREP-001")]
        public async Task PrePopulateBudget_FromAgreementTerms_PopulatesFeeStructure()
        {
            // Arrange
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                FeePercentage = 8.0m,
                IsActive = true
            };
            _context.PartnershipAgreements.Add(agreement);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Project",
                EstimatedValue = 10000000, // $10M
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            await _logic.LinkAgreementToOpportunityAsync(1, 1);
            
            // Act
            var budgetData = await _logic.PrePopulateBudgetFromAgreementAsync(1);
            
            // Assert
            Assert.NotNull(budgetData);
            Assert.Equal(8.0m, budgetData.FeePercentage);
            Assert.Equal(800000m, budgetData.CalculatedFee); // 8% of $10M
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Integration")]
        [Trait("TestId", "TC-OPP-AGR-PREP-002")]
        public async Task PrePopulatePartners_FromAgreement_AddsPartnerAutomatically()
        {
            // Arrange
            var partner = new Partner { Id = 1, Name = "UNDP", IsActive = true };
            _context.Partners.Add(partner);
            
            var agreement = new PartnershipAgreement
            {
                Id = 1,
                PartnerName = "UNDP",
                PartnerId = 1,
                IsActive = true
            };
            _context.PartnershipAgreements.Add(agreement);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "UNDP Partnership Project",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();
            
            await _logic.LinkAgreementToOpportunityAsync(1, 1);
            
            // Act
            await _logic.PrePopulatePartnersFromAgreementAsync(1);
            
            // Assert
            var opportunityPartners = await _context.OpportunityPartners
                .Where(op => op.OpportunityId == 1)
                .ToListAsync();
            
            Assert.Single(opportunityPartners);
            Assert.Equal(1, opportunityPartners[0].PartnerId);
        }

        #endregion

        public class AgreementTerms
        {
            public string[] Geography { get; set; }
            public decimal? FeePercentage { get; set; }
            public int? ValidityYears { get; set; }
            public string PartnerName { get; set; }
            public string AgreementType { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Value { get; set; }
            public string[] EligibleActivities { get; set; }
            public string[] ExcludedActivities { get; set; }
            public Signatory[] Signatories { get; set; }
            public decimal? MaxAggregateValue { get; set; }
            public decimal? AnnualCap { get; set; }
        }

        public class Signatory
        {
            public string Name { get; set; }
            public string Title { get; set; }
            public string Organization { get; set; }
        }

        public class PartnershipAgreement
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
            public string PartnerName { get; set; }
            public string Type { get; set; }
            public string Geography { get; set; }
            public decimal? Value { get; set; }
            public int Version { get; set; }
            public bool IsCurrentVersion { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsFullTextSearchable { get; set; }
            public decimal? FeePercentage { get; set; }
            public decimal? AnnualCap { get; set; }
            public int? PartnerId { get; set; }
        }

        public class OpportunityAgreement
        {
            public int OpportunityId { get; set; }
            public int AgreementId { get; set; }
        }

        public class ExpiringAgreement
        {
            public int Id { get; set; }
            public bool RequiresRenewal { get; set; }
            public int DaysUntilExpiration { get; set; }
        }

        public class AgreementValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public bool HasWarnings { get; set; }
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public class BudgetPrePopulationData
        {
            public decimal FeePercentage { get; set; }
            public decimal CalculatedFee { get; set; }
        }

        public class Partner
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }

        public class OpportunityPartner
        {
            public int OpportunityId { get; set; }
            public int PartnerId { get; set; }
        }

        public class BusinessException : Exception
        {
            public BusinessException(string message) : base(message) { }
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
