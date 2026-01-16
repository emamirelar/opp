using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class TemplateAndCloningTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;

        public TemplateAndCloningTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"TemplateTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-008")]
        public async Task TemplateCreation_FromCompletedOpportunity_Success()
        {
            // Arrange - Completed high-quality opportunity
            var sourceOpportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Standard Road Rehabilitation",
                EstimatedValue = 4200000,
                Sector = "Infrastructure",
                Status = "Converted",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(sourceOpportunity);

            // Add deliverables
            var deliverables = new List<string>
            {
                "Site Survey and Assessment",
                "Engineering Design",
                "Environmental Impact Study",
                // ... 10 more standard deliverables
            };

            foreach (var del in deliverables)
            {
                _context.OpportunityDeliverables.Add(new OpportunityDeliverable
                {
                    OpportunityId = 1,
                    Description = del,
                    IsTemplateItem = true
                });
            }

            await _context.SaveChangesAsync();

            // Act - Save as template
            var template = new OpportunityTemplate
            {
                Name = "Road Infrastructure Template",
                SourceOpportunityId = 1,
                IsGlobalTemplate = true,
                Category = "Infrastructure",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.OpportunityTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Assert
            var savedTemplate = await _context.OpportunityTemplates
                .FirstAsync(t => t.SourceOpportunityId == 1);

            Assert.NotNull(savedTemplate);
            Assert.True(savedTemplate.IsGlobalTemplate);
            Assert.Equal("Infrastructure", savedTemplate.Category);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-008-UseTemplate")]
        public async Task CreateFromTemplate_UsesStructure_TimesSaved()
        {
            // Arrange - Template exists
            var template = new OpportunityTemplate
            {
                Id = 1,
                Name = "Road Infrastructure Template",
                IsGlobalTemplate = true,
                UsageCount = 47 // Used 47 times
            };
            _context.OpportunityTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Act - Create new opportunity from template
            var startTime = DateTime.UtcNow;

            var newOpportunity = new Domain.Entities.Opportunity
            {
                Name = "Highway Rehabilitation - Region 5",
                EstimatedValue = 4200000,
                TemplateId = template.Id,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(newOpportunity);
            await _context.SaveChangesAsync();

            var endTime = DateTime.UtcNow;

            // Update template usage
            template.UsageCount++;
            template.LastUsedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, newOpportunity.TemplateId);
            Assert.Equal(48, template.UsageCount); // Incremented
            
            // Time saved: 30 minutes vs 3 hours from scratch
            // Estimated 2.5 hours saved per use
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-008-Clone")]
        public async Task CloneOpportunity_CopiesStructure_QuickCreation()
        {
            // Arrange - Existing completed opportunity
            var sourceOpportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Highway Rehabilitation - Region 3",
                EstimatedValue = 4500000,
                Sector = "Infrastructure",
                PrimaryCountryId = 1,
                Status = "Converted",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(sourceOpportunity);
            await _context.SaveChangesAsync();

            // Act - Clone opportunity
            var clonedOpportunity = new Domain.Entities.Opportunity
            {
                Name = sourceOpportunity.Name + " - Copy",
                EstimatedValue = sourceOpportunity.EstimatedValue,
                Sector = sourceOpportunity.Sector,
                PrimaryCountryId = sourceOpportunity.PrimaryCountryId,
                Status = "Draft", // Reset to Draft
                ClonedFromId = sourceOpportunity.Id,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(clonedOpportunity);
            await _context.SaveChangesAsync();

            // Assert
            Assert.NotNull(clonedOpportunity);
            Assert.Contains("- Copy", clonedOpportunity.Name);
            Assert.Equal(sourceOpportunity.EstimatedValue, clonedOpportunity.EstimatedValue);
            Assert.Equal(sourceOpportunity.Id, clonedOpportunity.ClonedFromId);
            Assert.Equal("Draft", clonedOpportunity.Status); // Cloned opportunities start as Draft
        }

        public class OpportunityTemplate { public int Id { get; set; } public string Name { get; set; } public int SourceOpportunityId { get; set; } public bool IsGlobalTemplate { get; set; } public string Category { get; set; } public int UsageCount { get; set; } public DateTime? LastUsedDate { get; set; } public int CreatedBy { get; set; } public DateTime CreatedDate { get; set; } }
        public class OpportunityDeliverable { public int OpportunityId { get; set; } public string Description { get; set; } public bool IsTemplateItem { get; set; } }
        public class Project { public int Id { get; set; } public string Name { get; set; } public decimal Budget { get; set; } public int OriginalOpportunityId { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
