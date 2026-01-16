using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    /// <summary>
    /// Tests for OpportunityController API endpoints
    /// Based on OpportunityController_TestCases.md (12+ tests)
    /// </summary>
    public class OpportunityControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuthorizationService> _mockAuthService;
        private readonly OpportunityController _controller;

        public OpportunityControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockAuthService = new Mock<IAuthorizationService>();

            _controller = new OpportunityController(
                _mockManagerWrapper.Object,
                _mockAuthService.Object,
                _mockMapper.Object
            );
        }

        #region TC-OPP-CTRL-F-001: GET - Get All Opportunities

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-001")]
        public async Task GetAllOpportunities_ValidRequest_ReturnsOkWithList()
        {
            // Arrange
            var opportunities = new List<OpportunityModel>
            {
                new OpportunityModel { Id = 1, Name = "Opportunity 1", EstimatedValue = 1000000 },
                new OpportunityModel { Id = 2, Name = "Opportunity 2", EstimatedValue = 2000000 }
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.GetAllAsync())
                .ReturnsAsync(opportunities);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOpportunities = Assert.IsAssignableFrom<List<OpportunityModel>>(okResult.Value);
            Assert.Equal(2, returnedOpportunities.Count);
        }

        #endregion

        #region TC-OPP-CTRL-F-002: GET - Get Opportunity by ID

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-002")]
        public async Task GetOpportunityById_ValidId_ReturnsOkWithOpportunity()
        {
            // Arrange
            var opportunityId = 1;
            var opportunity = new OpportunityModel 
            { 
                Id = opportunityId, 
                Name = "Test Opportunity",
                EstimatedValue = 1500000
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.GetByIdAsync(opportunityId))
                .ReturnsAsync(opportunity);

            // Act
            var result = await _controller.GetById(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOpportunity = Assert.IsType<OpportunityModel>(okResult.Value);
            Assert.Equal(opportunityId, returnedOpportunity.Id);
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-002-NotFound")]
        public async Task GetOpportunityById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = 99999;
            _mockManagerWrapper.Setup(m => m.OpportunityManager.GetByIdAsync(invalidId))
                .ThrowsAsync(new KeyNotFoundException($"Opportunity {invalidId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _controller.GetById(invalidId));
        }

        #endregion

        #region TC-OPP-CTRL-F-003: POST - Create Opportunity

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-003")]
        public async Task CreateOpportunity_ValidRequest_ReturnsCreatedWithLocation()
        {
            // Arrange
            var createRequest = new OpportunityCreateRequest
            {
                Name = "New Opportunity",
                Description = "Test Description",
                EstimatedValue = 2500000,
                CurrencyId = 1,
                PrimaryCountryId = 1,
                ResponsibleOrgUnitId = 1
            };

            var createdOpportunity = new OpportunityModel
            {
                Id = 1,
                Name = createRequest.Name,
                EstimatedValue = createRequest.EstimatedValue,
                Status = "Draft"
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.CreateOpportunityAsync(createRequest))
                .ReturnsAsync(createdOpportunity);

            // Act
            var result = await _controller.Create(createRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            var returnedOpportunity = Assert.IsType<OpportunityModel>(createdResult.Value);
            Assert.Equal(1, returnedOpportunity.Id);
            Assert.Equal("Draft", returnedOpportunity.Status);
        }

        #endregion

        #region TC-OPP-CTRL-F-004: PUT - Update Opportunity

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-004")]
        public async Task UpdateOpportunity_ValidRequest_ReturnsOk()
        {
            // Arrange
            var updateRequest = new OpportunityUpdateRequest
            {
                Id = 1,
                Name = "Updated Name",
                EstimatedValue = 3000000
            };

            var updatedOpportunity = new OpportunityModel
            {
                Id = 1,
                Name = updateRequest.Name,
                EstimatedValue = updateRequest.EstimatedValue.Value
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.UpdateAsync(updateRequest))
                .ReturnsAsync(updatedOpportunity);

            // Act
            var result = await _controller.Update(1, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOpportunity = Assert.IsType<OpportunityModel>(okResult.Value);
            Assert.Equal("Updated Name", returnedOpportunity.Name);
        }

        #endregion

        #region TC-OPP-CTRL-F-005: DELETE - Soft Delete Opportunity

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-005")]
        public async Task DeleteOpportunity_ValidId_ReturnsNoContent()
        {
            // Arrange
            var opportunityId = 1;
            _mockManagerWrapper.Setup(m => m.OpportunityManager.DeleteAsync(opportunityId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(opportunityId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify soft delete called
            _mockManagerWrapper.Verify(
                m => m.OpportunityManager.DeleteAsync(opportunityId),
                Times.Once);
        }

        #endregion

        #region TC-OPP-CTRL-F-006: POST - Update Opportunity Status

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-006")]
        public async Task UpdateOpportunityStatus_ValidTransition_ReturnsOk()
        {
            // Arrange
            var opportunityId = 1;
            var statusRequest = new StatusUpdateRequest
            {
                Status = "Profiling",
                Reason = "Moving to profiling stage"
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.UpdateStatusAsync(opportunityId, statusRequest.Status))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateStatus(opportunityId, statusRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
        }

        #endregion

        #region TC-OPP-CTRL-F-007: GET - Filter Opportunities by Status

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-007")]
        public async Task FilterOpportunities_ByStatus_ReturnsFilteredList()
        {
            // Arrange
            var filterRequest = new OpportunityFilterRequest
            {
                Status = "Profiling",
                PageNumber = 1,
                PageSize = 10
            };

            var filteredOpportunities = new PaginatedResult<OpportunityModel>
            {
                Data = new List<OpportunityModel>
                {
                    new OpportunityModel { Id = 1, Name = "Opp 1", Status = "Profiling" },
                    new OpportunityModel { Id = 2, Name = "Opp 2", Status = "Profiling" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.FilterAsync(filterRequest))
                .ReturnsAsync(filteredOpportunities);

            // Act
            var result = await _controller.Filter(filterRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var paginatedResult = Assert.IsType<PaginatedResult<OpportunityModel>>(okResult.Value);
            Assert.Equal(2, paginatedResult.Data.Count);
            Assert.All(paginatedResult.Data, o => Assert.Equal("Profiling", o.Status));
        }

        #endregion

        #region TC-OPP-CTRL-F-008: POST - Convert Opportunity to Project

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-CTRL-F-008")]
        public async Task ConvertToProject_ApprovedOpportunity_ReturnsOk()
        {
            // Arrange
            var opportunityId = 1;
            var conversionRequest = new ConversionRequest
            {
                TargetType = "Project",
                ProjectManagerId = 5
            };

            var projectResult = new ProjectModel
            {
                Id = 1,
                Name = "Converted Project",
                OriginalOpportunityId = opportunityId
            };

            _mockManagerWrapper.Setup(m => m.OpportunityManager.ConvertToProjectAsync(opportunityId, conversionRequest))
                .ReturnsAsync(projectResult);

            // Act
            var result = await _controller.Convert(opportunityId, conversionRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProject = Assert.IsType<ProjectModel>(okResult.Value);
            Assert.Equal(opportunityId, returnedProject.OriginalOpportunityId);
        }

        #endregion

        #region TC-OPP-CTRL-AUTH-001: Authorization - Insufficient Permissions

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Authorization")]
        [Trait("TestId", "TC-OPP-CTRL-AUTH-001")]
        public async Task CreateOpportunity_InsufficientPermissions_ReturnsUnauthorized()
        {
            // Arrange
            var createRequest = new OpportunityCreateRequest
            {
                Name = "Unauthorized Test",
                EstimatedValue = 1000000
            };

            // Mock authorization failure
            _mockAuthService.Setup(a => a.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _controller.Create(createRequest));
        }

        #endregion

        #region Helper Classes

        public class StatusUpdateRequest
        {
            public string Status { get; set; }
            public string Reason { get; set; }
        }

        public class ConversionRequest
        {
            public string TargetType { get; set; } // Project, Programme, Portfolio
            public int ProjectManagerId { get; set; }
        }

        public class ProjectModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int OriginalOpportunityId { get; set; }
        }

        public class PaginatedResult<T>
        {
            public List<T> Data { get; set; }
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }

        #endregion
    }
}
