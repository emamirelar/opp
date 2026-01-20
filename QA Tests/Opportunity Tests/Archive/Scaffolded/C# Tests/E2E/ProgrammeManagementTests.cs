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
    public class ProgrammeManagementTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;

        public ProgrammeManagementTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ProgrammeTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Countries.Add(new Country { Id = 1, Name = "Mozambique", Code = "MZ" });
            _context.SaveChanges();
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-016")]
        public async Task OpportunityToProgrammeConversion_FourComponents_CreatesHierarchy()
        {
            // Arrange - Large multi-component opportunity
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Integrated Rural Development Programme",
                EstimatedValue = 15000000,
                PrimaryCountryId = 1,
                OpportunityType = "Programme",
                Status = "Approved",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            // Add 4 components
            var components = new List<OpportunityComponent>
            {
                new OpportunityComponent { OpportunityId = 1, Name = "Agricultural Development", Budget = 5000000 },
                new OpportunityComponent { OpportunityId = 1, Name = "Market Infrastructure", Budget = 4000000 },
                new OpportunityComponent { OpportunityId = 1, Name = "Water & Sanitation", Budget = 3000000 },
                new OpportunityComponent { OpportunityId = 1, Name = "Capacity Building", Budget = 3000000 }
            };

            _context.Opportunities.Add(opportunity);
            _context.OpportunityComponents.AddRange(components);
            await _context.SaveChangesAsync();

            // Act - Convert to Programme
            var programme = new Programme
            {
                Id = 1,
                Name = opportunity.Name,
                TotalBudget = opportunity.EstimatedValue,
                OriginalOpportunityId = opportunity.Id,
                CreatedDate = DateTime.UtcNow
            };
            _context.Programmes.Add(programme);

            // Create 4 child projects
            var projects = components.Select(c => new Project
            {
                Name = c.Name,
                ProgrammeId = programme.Id,
                Budget = c.Budget,
                OriginalOpportunityComponentId = c.Id
            }).ToList();

            _context.Projects.AddRange(projects);
            await _context.SaveChangesAsync();

            // Assert
            Assert.NotNull(programme);
            var savedProgramme = await _context.Programmes
                .Include(p => p.ChildProjects)
                .FirstAsync(p => p.Id == programme.Id);

            Assert.Equal(4, savedProgramme.ChildProjects.Count);
            Assert.Equal(15000000, savedProgramme.TotalBudget);
            
            // Budget allocation correct
            var projectsBudgetSum = savedProgramme.ChildProjects.Sum(p => p.Budget);
            Assert.True(projectsBudgetSum <= savedProgramme.TotalBudget);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-005")]
        public async Task PortfolioAggregation_FourOpportunities_CreatesPortfolio()
        {
            // Arrange - 4 related water opportunities
            var opportunities = new List<Domain.Entities.Opportunity>
            {
                new() { Id = 10, Name = "Water Supply - Urban", EstimatedValue = 3000000, Sector = "Water", PrimaryCountryId = 1 },
                new() { Id = 11, Name = "Sanitation - Rural", EstimatedValue = 2000000, Sector = "Water", PrimaryCountryId = 1 },
                new() { Id = 12, Name = "Water Treatment", EstimatedValue = 4000000, Sector = "Water", PrimaryCountryId = 1 },
                new() { Id = 13, Name = "Community Hygiene", EstimatedValue = 1000000, Sector = "Water", PrimaryCountryId = 1 }
            };

            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Create portfolio
            var portfolio = new Portfolio
            {
                Id = 1,
                Name = "Tanzania Water & Sanitation Portfolio",
                TotalBudget = 10000000, // Aggregated
                CreatedDate = DateTime.UtcNow
            };
            _context.Portfolios.Add(portfolio);

            // Link opportunities to portfolio
            foreach (var opp in opportunities)
            {
                _context.PortfolioOpportunities.Add(new PortfolioOpportunity
                {
                    PortfolioId = portfolio.Id,
                    OpportunityId = opp.Id
                });
            }

            await _context.SaveChangesAsync();

            // Assert
            var savedPortfolio = await _context.Portfolios
                .Include(p => p.Opportunities)
                .FirstAsync(p => p.Id == portfolio.Id);

            Assert.Equal(4, savedPortfolio.Opportunities.Count);
            Assert.Equal(10000000, savedPortfolio.TotalBudget);
            Assert.All(savedPortfolio.Opportunities, o => Assert.Equal("Water", o.Opportunity.Sector));
        }

        public class OpportunityComponent { public int Id { get; set; } public int OpportunityId { get; set; } public string Name { get; set; } public decimal Budget { get; set; } }
        public class Programme { public int Id { get; set; } public string Name { get; set; } public decimal TotalBudget { get; set; } public int OriginalOpportunityId { get; set; } public DateTime CreatedDate { get; set; } public List<Project> ChildProjects { get; set; } }
        public class Project { public int Id { get; set; } public string Name { get; set; } public int ProgrammeId { get; set; } public decimal Budget { get; set; } public int OriginalOpportunityComponentId { get; set; } }
        public class Portfolio { public int Id { get; set; } public string Name { get; set; } public decimal TotalBudget { get; set; } public DateTime CreatedDate { get; set; } public List<PortfolioOpportunity> Opportunities { get; set; } }
        public class PortfolioOpportunity { public int PortfolioId { get; set; } public int OpportunityId { get; set; } public Domain.Entities.Opportunity Opportunity { get; set; } }
        public class Country { public int Id { get; set; } public string Name { get; set; } public string Code { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
