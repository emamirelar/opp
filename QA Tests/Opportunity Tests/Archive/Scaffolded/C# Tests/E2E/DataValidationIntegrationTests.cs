using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.BusinessLogic;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class DataValidationIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly DataValidationLogic _validationLogic;

        public DataValidationIntegrationTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"DataValidationTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _validationLogic = new DataValidationLogic(_context);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-NEG-011")]
        public async Task BudgetDSTMisalignment_SevereUnderbudget_AlertGenerated()
        {
            // Arrange - Opportunity with severe budget-complexity misalignment
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Healthcare Infrastructure - 50 Facilities",
                EstimatedValue = 500000, // $500K - severely under-budgeted
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            var dstProfile = new DSTProfile
            {
                OpportunityId = 1,
                ComplexityScore = 8.5m, // Very high complexity
                BenchmarkBudget = 2500000m // Historical similar projects: $2-3M
            };

            _context.Opportunities.Add(opportunity);
            _context.DSTProfiles.Add(dstProfile);
            await _context.SaveChangesAsync();

            // Act - Validate budget-complexity alignment
            var validation = await _validationLogic.ValidateBudgetComplexityAlignmentAsync(1);

            // Assert
            Assert.False(validation.IsAligned);
            Assert.True(validation.PercentageBelow < -70m); // -75% below benchmark
            Assert.Equal("Severe Misalignment", validation.Severity);
            Assert.Contains("review budget", validation.Recommendation, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-NEG-012")]
        public async Task GeographyCountryMismatch_DocumentsSayKenya_OpportunitySaysTanzania_Detected()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                PrimaryCountryId = 1, // Tanzania
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Countries.AddRange(
                new Country { Id = 1, Name = "Tanzania", Code = "TZ" },
                new Country { Id = 2, Name = "Kenya", Code = "KE" }
            );

            // Document mentions different country
            var documentAnalysis = new DocumentCountryAnalysis
            {
                OpportunityId = 1,
                DocumentCountryMentions = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "Kenya", 45 }, // Mentioned 45 times
                    { "Tanzania", 0 } // Not mentioned
                }
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var mismatchDetection = _validationLogic.DetectGeographicMismatch(opportunity, documentAnalysis);

            // Assert
            Assert.True(mismatchDetection.HasMismatch);
            Assert.Equal("Tanzania", mismatchDetection.OpportunityCountry);
            Assert.Equal("Kenya", mismatchDetection.DocumentCountry); // Most mentioned
            Assert.Equal(45, mismatchDetection.MentionCount);
        }

        public class DSTProfile { public int OpportunityId { get; set; } public decimal ComplexityScore { get; set; } public decimal BenchmarkBudget { get; set; } }
        public class Country { public int Id { get; set; } public string Name { get; set; } public string Code { get; set; } }
        public class BudgetComplexityValidation { public bool IsAligned { get; set; } public decimal PercentageBelow { get; set; } public string Severity { get; set; } public string Recommendation { get; set; } }
        public class DocumentCountryAnalysis { public int OpportunityId { get; set; } public System.Collections.Generic.Dictionary<string, int> DocumentCountryMentions { get; set; } }
        public class GeographicMismatchResult { public bool HasMismatch { get; set; } public string OpportunityCountry { get; set; } public string DocumentCountry { get; set; } public int MentionCount { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
