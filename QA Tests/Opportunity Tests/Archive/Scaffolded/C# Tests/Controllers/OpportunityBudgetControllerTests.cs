using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    /// <summary>
    /// Tests for OpportunityBudgetController API endpoints
    /// Based on OpportunityBudgetController_TestCases.md (8+ tests)
    /// </summary>
    public class OpportunityBudgetControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly OpportunityBudgetController _controller;

        public OpportunityBudgetControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _mockMapper = new Mock<IMapper>();

            _controller = new OpportunityBudgetController(
                _mockManagerWrapper.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUD-CTRL-F-001")]
        public async Task GenerateBudget_ValidOpportunity_ReturnsOkWithBudget()
        {
            // Arrange
            var opportunityId = 1;
            var budgetModel = new BudgetModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                TotalBudget = 2750000m,
                BaseCost = 2500000m,
                FeeAmount = 250000m,
                FeePercentage = 10m
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.GenerateBudgetAsync(opportunityId, 10m))
                .ReturnsAsync(budgetModel);

            // Act
            var result = await _controller.GenerateBudget(opportunityId, feePercentage: 10m);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBudget = Assert.IsType<BudgetModel>(okResult.Value);
            Assert.Equal(2750000m, returnedBudget.TotalBudget);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUD-CTRL-F-002")]
        public async Task GetBudget_ValidOpportunity_ReturnsOkWithBudget()
        {
            // Arrange
            var opportunityId = 1;
            var budgetModel = new BudgetModel { Id = 1, OpportunityId = opportunityId };

            _mockManagerWrapper.Setup(m => m.BudgetManager.GetBudgetAsync(opportunityId))
                .ReturnsAsync(budgetModel);

            // Act
            var result = await _controller.GetBudget(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBudget = Assert.IsType<BudgetModel>(okResult.Value);
            Assert.Equal(opportunityId, returnedBudget.OpportunityId);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-003")]
        public async Task UpdateBudget_ValidRequest_ReturnsOkWithUpdatedBudget()
        {
            // Arrange
            var budgetId = 1;
            var updateRequest = new BudgetUpdateRequest
            {
                FeePercentage = 12m,
                Notes = "Updated fee structure per new agreement"
            };

            var updatedBudget = new BudgetModel
            {
                Id = budgetId,
                FeePercentage = 12m,
                TotalBudget = 2800000m
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.UpdateBudgetAsync(budgetId, updateRequest))
                .ReturnsAsync(updatedBudget);

            // Act
            var result = await _controller.UpdateBudget(budgetId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBudget = Assert.IsType<BudgetModel>(okResult.Value);
            Assert.Equal(12m, returnedBudget.FeePercentage);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-004")]
        public async Task GetSpendRate_ValidBudget_ReturnsOkWithChartData()
        {
            // Arrange
            var budgetId = 1;
            var spendRateData = new SpendRateResponse
            {
                MonthlyData = new List<MonthlySpend>
                {
                    new MonthlySpend { Month = 1, Amount = 50000m },
                    new MonthlySpend { Month = 2, Amount = 75000m },
                    new MonthlySpend { Month = 3, Amount = 100000m }
                }
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.GetSpendRateAsync(budgetId))
                .ReturnsAsync(spendRateData);

            // Act
            var result = await _controller.GetSpendRate(budgetId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<SpendRateResponse>(okResult.Value);
            Assert.Equal(3, returnedData.MonthlyData.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-005")]
        public async Task GetCostCategories_ValidBudget_ReturnsOkWithBreakdown()
        {
            // Arrange
            var budgetId = 1;
            var costCategories = new CostCategoriesResponse
            {
                Personnel = 1500000m,
                NonPersonnel = 1000000m,
                Fee = 250000m,
                Total = 2750000m
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.GetCostCategoriesAsync(budgetId))
                .ReturnsAsync(costCategories);

            // Act
            var result = await _controller.GetCostCategories(budgetId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategories = Assert.IsType<CostCategoriesResponse>(okResult.Value);
            Assert.Equal(1500000m, returnedCategories.Personnel);
            Assert.Equal(1000000m, returnedCategories.NonPersonnel);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-006")]
        public async Task UpdateFeeStructure_ValidRequest_ReturnsOkWithUpdatedBudget()
        {
            // Arrange
            var budgetId = 1;
            var feeUpdateRequest = new FeeStructureUpdateRequest
            {
                FeePercentage = 8m,
                Source = "Partnership Agreement PA-2026-001",
                Justification = "Agreement specifies 8% management fee"
            };

            var updatedBudget = new BudgetModel
            {
                Id = budgetId,
                FeePercentage = 8m,
                FeeSource = "Partnership Agreement PA-2026-001"
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.UpdateFeeStructureAsync(budgetId, feeUpdateRequest))
                .ReturnsAsync(updatedBudget);

            // Act
            var result = await _controller.UpdateFeeStructure(budgetId, feeUpdateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBudget = Assert.IsType<BudgetModel>(okResult.Value);
            Assert.Equal(8m, returnedBudget.FeePercentage);
            Assert.Contains("PA-2026-001", returnedBudget.FeeSource);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-007")]
        public async Task GenerateBudgetReport_ValidBudget_ReturnsFileResult()
        {
            // Arrange
            var budgetId = 1;
            var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF signature

            _mockManagerWrapper.Setup(m => m.BudgetManager.GenerateBudgetReportAsync(budgetId))
                .ReturnsAsync(pdfBytes);

            // Act
            var result = await _controller.GenerateBudgetReport(budgetId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-BUDCTRL-008")]
        public async Task CompareBudgets_MultipleIds_ReturnsOkWithComparison()
        {
            // Arrange
            var budgetIds = new[] { 1, 2, 3 };
            var comparison = new BudgetComparisonResponse
            {
                Budgets = new List<BudgetModel>
                {
                    new BudgetModel { Id = 1, TotalBudget = 2500000m },
                    new BudgetModel { Id = 2, TotalBudget = 3000000m },
                    new BudgetModel { Id = 3, TotalBudget = 2800000m }
                },
                AverageBudget = 2766667m,
                HighestBudget = 3000000m,
                LowestBudget = 2500000m
            };

            _mockManagerWrapper.Setup(m => m.BudgetManager.CompareBudgetsAsync(budgetIds))
                .ReturnsAsync(comparison);

            // Act
            var result = await _controller.CompareBudgets(budgetIds);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedComparison = Assert.IsType<BudgetComparisonResponse>(okResult.Value);
            Assert.Equal(3, returnedComparison.Budgets.Count);
            Assert.Equal(3000000m, returnedComparison.HighestBudget);
        }

        public class BudgetModel
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal BaseCost { get; set; }
            public decimal FeeAmount { get; set; }
            public decimal FeePercentage { get; set; }
            public string FeeSource { get; set; }
        }

        public class BudgetUpdateRequest
        {
            public decimal FeePercentage { get; set; }
            public string Notes { get; set; }
        }

        public class SpendRateResponse
        {
            public List<MonthlySpend> MonthlyData { get; set; }
        }

        public class MonthlySpend
        {
            public int Month { get; set; }
            public decimal Amount { get; set; }
        }

        public class CostCategoriesResponse
        {
            public decimal Personnel { get; set; }
            public decimal NonPersonnel { get; set; }
            public decimal Fee { get; set; }
            public decimal Total { get; set; }
        }

        public class FeeStructureUpdateRequest
        {
            public decimal FeePercentage { get; set; }
            public string Source { get; set; }
            public string Justification { get; set; }
        }

        public class BudgetComparisonResponse
        {
            public List<BudgetModel> Budgets { get; set; }
            public decimal AverageBudget { get; set; }
            public decimal HighestBudget { get; set; }
            public decimal LowestBudget { get; set; }
        }
    }
}
