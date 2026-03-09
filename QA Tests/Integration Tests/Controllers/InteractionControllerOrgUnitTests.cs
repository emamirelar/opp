using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Server;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for InteractionController OrgUnit filtering functionality
    /// </summary>
    [Collection("Integration Tests")]
    public class InteractionControllerOrgUnitTests : IntegrationTestBase
    {
        private const string BaseUrl = "/api/interaction";

        public InteractionControllerOrgUnitTests(PAOWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]

        [Trait("Defect", "DEF-106")]
        public async Task GetInteractions_WithOrgUnitIdFilter_ShouldAcceptParameter()
        {
            // Act
            var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=10&orgUnitId=100");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
            result.Should().NotBeNull();
            result.PageIndex.Should().Be(1);
            result.PageSize.Should().Be(10);
            // Note: Actual filtering will be handled by OrgUnitFilterService
        }

        [Fact]

        [Trait("Defect", "DEF-106")]
        public async Task GetInteractions_WithOrgUnitIdAndSearchText_ShouldAcceptBothParameters()
        {
            // Act
            var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=10&orgUnitId=100&searchText=meeting");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
            result.Should().NotBeNull();
            // The service will combine both filters
        }

        [Fact]

        [Trait("Defect", "DEF-106")]
        public async Task GetInteractions_WithOrgUnitIdInAdvancedSearch_ShouldWork()
        {
            // Arrange
            var searchCriteria = System.Text.Json.JsonSerializer.Serialize(new[]
            {
                new
                {
                    field = "subject",
                    value = "meeting",
                    @operator = "like",
                    logicalOperator = "AND"
                }
            });
            
            // Act
            var response = await Client.GetAsync($"{BaseUrl}?pageIndex=1&pageSize=10&orgUnitId=100&advancedSearch=true&searchCriteria={Uri.EscapeDataString(searchCriteria)}");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PaginationResponse<InteractionModel>>();
            result.Should().NotBeNull();
        }
    }
}