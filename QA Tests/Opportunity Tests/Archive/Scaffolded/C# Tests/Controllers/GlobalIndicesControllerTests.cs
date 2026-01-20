using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    public class GlobalIndicesControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly GlobalIndicesController _controller;

        public GlobalIndicesControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _controller = new GlobalIndicesController(_mockManagerWrapper.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-GI-CTRL-F-001")]
        public async Task UploadIndices_ValidData_ReturnsOk()
        {
            var uploadRequest = new List<GlobalIndexUploadModel> { new GlobalIndexUploadModel { CountryId = 1, IndexType = "MVI", Value = 35m, Year = 2026 } };
            var result = new UploadResultModel { Success = true, RecordsProcessed = 1 };
            _mockManagerWrapper.Setup(m => m.GlobalIndicesManager.UploadGlobalIndicesAsync(uploadRequest, 1)).ReturnsAsync(result);

            var response = await _controller.UploadIndices(uploadRequest);

            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-GI-CTRL-F-002")]
        public async Task GetCurrentIndices_ValidCountry_ReturnsIndices()
        {
            // Arrange
            var countryId = 1;
            var indices = new List<GlobalIndexModel>
            {
                new GlobalIndexModel { IndexType = "MVI", Value = 35m, IsCurrent = true, Year = 2026 },
                new GlobalIndexModel { IndexType = "HDI", Value = 0.68m, IsCurrent = true, Year = 2026 },
                new GlobalIndexModel { IndexType = "GDI", Value = 0.95m, IsCurrent = true, Year = 2026 }
            };

            _mockManagerWrapper.Setup(m => m.GlobalIndicesManager.GetCurrentIndicesAsync(countryId))
                .ReturnsAsync(indices);

            // Act
            var result = await _controller.GetCurrentIndices(countryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIndices = Assert.IsAssignableFrom<List<GlobalIndexModel>>(okResult.Value);
            Assert.Equal(3, returnedIndices.Count);
            Assert.All(returnedIndices, idx => Assert.True(idx.IsCurrent));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-GI-CTRL-F-003")]
        public async Task GetHistoricalIndices_AsAtDate_ReturnsHistoricalValues()
        {
            // Arrange
            var countryId = 1;
            var asAtDate = new System.DateTime(2024, 1, 1);
            var historicalIndices = new List<GlobalIndexModel>
            {
                new GlobalIndexModel { IndexType = "MVI", Value = 33m, IsCurrent = false, Year = 2024 },
                new GlobalIndexModel { IndexType = "HDI", Value = 0.65m, IsCurrent = false, Year = 2024 }
            };

            _mockManagerWrapper.Setup(m => m.GlobalIndicesManager.GetHistoricalIndicesAsync(countryId, asAtDate))
                .ReturnsAsync(historicalIndices);

            // Act
            var result = await _controller.GetHistoricalIndices(countryId, asAtDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedIndices = Assert.IsAssignableFrom<List<GlobalIndexModel>>(okResult.Value);
            Assert.Equal(2, returnedIndices.Count);
            Assert.All(returnedIndices, idx => Assert.Equal(2024, idx.Year));
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-GI-CTRL-F-004")]
        public async Task BulkUploadIndices_193Countries_ReturnsSuccessWithCounts()
        {
            // Arrange
            var uploadData = new List<GlobalIndexUploadModel>();
            for (int i = 1; i <= 193; i++) // All UN member states
            {
                uploadData.Add(new GlobalIndexUploadModel
                {
                    CountryId = i,
                    IndexType = "MVI",
                    Value = 30m + (i % 40), // Vary values 30-70
                    Year = 2026
                });
            }

            var uploadResult = new UploadResultModel
            {
                Success = true,
                RecordsProcessed = 193,
                RecordsUpdated = 193,
                RecordsInserted = 0,
                Errors = new List<string>()
            };

            _mockManagerWrapper.Setup(m => m.GlobalIndicesManager.UploadGlobalIndicesAsync(uploadData, It.IsAny<int>()))
                .ReturnsAsync(uploadResult);

            // Act
            var response = await _controller.UploadIndices(uploadData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var result = Assert.IsType<UploadResultModel>(okResult.Value);
            Assert.True(result.Success);
            Assert.Equal(193, result.RecordsProcessed);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-GI-CTRL-F-005")]
        public async Task GetIndicesTrend_MultipleYears_ReturnsTimeSeriesData()
        {
            // Arrange
            var countryId = 1;
            var indexType = "MVI";
            var startYear = 2020;
            var endYear = 2026;

            var trendData = new IndexTrendResponse
            {
                CountryId = countryId,
                IndexType = indexType,
                DataPoints = new List<TrendDataPoint>
                {
                    new TrendDataPoint { Year = 2020, Value = 28m },
                    new TrendDataPoint { Year = 2021, Value = 30m },
                    new TrendDataPoint { Year = 2022, Value = 31m },
                    new TrendDataPoint { Year = 2023, Value = 32m },
                    new TrendDataPoint { Year = 2024, Value = 33m },
                    new TrendDataPoint { Year = 2025, Value = 34m },
                    new TrendDataPoint { Year = 2026, Value = 35m }
                },
                AverageAnnualChange = 1.17m,
                Trend = "Improving"
            };

            _mockManagerWrapper.Setup(m => m.GlobalIndicesManager.GetIndicesTrendAsync(countryId, indexType, startYear, endYear))
                .ReturnsAsync(trendData);

            // Act
            var result = await _controller.GetIndicesTrend(countryId, indexType, startYear, endYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTrend = Assert.IsType<IndexTrendResponse>(okResult.Value);
            Assert.Equal(7, returnedTrend.DataPoints.Count);
            Assert.Equal("Improving", returnedTrend.Trend);
        }

        public class GlobalIndexUploadModel
        {
            public int CountryId { get; set; }
            public string IndexType { get; set; }
            public decimal Value { get; set; }
            public int Year { get; set; }
        }

        public class UploadResultModel
        {
            public bool Success { get; set; }
            public int RecordsProcessed { get; set; }
            public int RecordsUpdated { get; set; }
            public int RecordsInserted { get; set; }
            public List<string> Errors { get; set; }
        }

        public class GlobalIndexModel
        {
            public string IndexType { get; set; }
            public decimal Value { get; set; }
            public bool IsCurrent { get; set; }
            public int Year { get; set; }
        }

        public class IndexTrendResponse
        {
            public int CountryId { get; set; }
            public string IndexType { get; set; }
            public List<TrendDataPoint> DataPoints { get; set; }
            public decimal AverageAnnualChange { get; set; }
            public string Trend { get; set; }
        }

        public class TrendDataPoint
        {
            public int Year { get; set; }
            public decimal Value { get; set; }
        }
    }
}
