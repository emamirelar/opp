using FluentAssertions;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using Xunit;
using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Models.Interactions;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests for InteractionFilterRequest to validate DateOnly properties and interface implementation
/// </summary>
public class InteractionFilterRequestTests
{
    [Fact]
    public void InteractionFilterRequest_DateOnlyProperties_ShouldWork()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var testDate = new DateOnly(2024, 6, 15);
        
        // Act
        request.FromDate = testDate;
        request.ToDate = testDate.AddDays(10);
        request.Date = testDate.AddDays(5);
        
        // Assert
        request.FromDate.Should().Be(testDate);
        request.ToDate.Should().Be(testDate.AddDays(10));
        request.Date.Should().Be(testDate.AddDays(5));
    }
    
    [Fact]
    public void InteractionFilterRequest_InterfaceImplementation_ShouldConvertDates()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        var testDate = new DateOnly(2024, 6, 15);
        var expectedDateTime = new DateTime(2024, 6, 15);
        
        // Act - Set DateOnly property
        request.FromDate = testDate;
        
        // Assert - Interface should return DateTime
        interface_ref.FromDate.Should().Be(expectedDateTime);
    }
    
    [Fact]
    public void InteractionFilterRequest_InterfaceSetter_ShouldConvertFromDateTime()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        var testDateTime = new DateTime(2024, 6, 15, 14, 30, 45);
        var expectedDate = new DateOnly(2024, 6, 15);
        
        // Act - Set via interface (DateTime)
        interface_ref.FromDate = testDateTime;
        
        // Assert - DateOnly property should be set correctly
        request.FromDate.Should().Be(expectedDate);
    }
    
    [Fact]
    public void InteractionFilterRequest_NullDates_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        
        // Act - Set null values
        request.FromDate = null;
        request.ToDate = null;
        request.Date = null;
        
        // Assert - Interface should return null
        interface_ref.FromDate.Should().BeNull();
        interface_ref.ToDate.Should().BeNull();
        interface_ref.Date.Should().BeNull();
    }
    
    [Fact]
    public void InteractionFilterRequest_InterfaceNullSetter_ShouldSetNull()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            FromDate = new DateOnly(2024, 6, 15),
            ToDate = new DateOnly(2024, 6, 20),
            Date = new DateOnly(2024, 6, 18)
        };
        var interface_ref = (IInteractionSearchFilter)request;
        
        // Act - Set null via interface
        interface_ref.FromDate = null;
        interface_ref.ToDate = null;
        interface_ref.Date = null;
        
        // Assert - DateOnly properties should be null
        request.FromDate.Should().BeNull();
        request.ToDate.Should().BeNull();
        request.Date.Should().BeNull();
    }
    
    [Fact]
    public void InteractionFilterRequest_TypeEnumConversion_ShouldWork()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        
        // Act
        request.Type = InteractionType.Email;
        
        // Assert
        interface_ref.Type.Should().Be("Email");
    }
    
    [Fact]
    public void InteractionFilterRequest_TypeStringConversion_ShouldWork()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        
        // Act
        interface_ref.Type = "VirtualMeeting";
        
        // Assert
        request.Type.Should().Be(InteractionType.VirtualMeeting);
    }
    
    [Fact]
    public void InteractionFilterRequest_InvalidTypeString_ShouldSetNull()
    {
        // Arrange
        var request = new InteractionFilterRequest();
        var interface_ref = (IInteractionSearchFilter)request;
        
        // Act
        interface_ref.Type = "InvalidType";
        
        // Assert
        request.Type.Should().BeNull();
    }
    
    [Fact]
    public void InteractionFilterRequest_AllProperties_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new InteractionFilterRequest
        {
            Id = 123,
            ContactId = 456,
            ContactName = "John Doe",
            PartnerId = 789,
            Type = InteractionType.Call,
            FromDate = new DateOnly(2024, 1, 1),
            ToDate = new DateOnly(2024, 12, 31),
            Date = new DateOnly(2024, 6, 15),
            Description = "Test description",
            Subject = "Test subject",
            SearchText = "Search term",
            OrgUnitId = 10,
            AdvancedSearch = true,
            SearchCriteria = "Advanced criteria"
        };
        
        // Assert
        request.Id.Should().Be(123);
        request.ContactId.Should().Be(456);
        request.ContactName.Should().Be("John Doe");
        request.PartnerId.Should().Be(789);
        request.Type.Should().Be(InteractionType.Call);
        request.FromDate.Should().Be(new DateOnly(2024, 1, 1));
        request.ToDate.Should().Be(new DateOnly(2024, 12, 31));
        request.Date.Should().Be(new DateOnly(2024, 6, 15));
        request.Description.Should().Be("Test description");
        request.Subject.Should().Be("Test subject");
        request.SearchText.Should().Be("Search term");
        request.OrgUnitId.Should().Be(10);
        request.AdvancedSearch.Should().BeTrue();
        request.SearchCriteria.Should().Be("Advanced criteria");
    }

    [Fact]
    public void InteractionFilterRequest_ValidateInvalidDateRange_ShouldReturnError()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            FromDate = new DateOnly(2024, 6, 20),
            ToDate = new DateOnly(2024, 6, 15) // Earlier than FromDate
        };
        var context = new ValidationContext(request);
        
        // Act
        var results = request.Validate(context).ToList();
        
        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("FromDate cannot be later than ToDate");
        results[0].MemberNames.Should().Contain(new[] { nameof(request.FromDate), nameof(request.ToDate) });
    }
    
    [Fact]
    public void InteractionFilterRequest_ValidateFutureDate_ShouldReturnError()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) // Tomorrow
        };
        var context = new ValidationContext(request);
        
        // Act
        var results = request.Validate(context).ToList();
        
        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("Date cannot be in the future");
        results[0].MemberNames.Should().Contain(nameof(request.Date));
    }
    
    [Fact]
    public void InteractionFilterRequest_ValidateAdvancedSearchWithoutCriteria_ShouldReturnError()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            AdvancedSearch = true,
            SearchCriteria = "" // Empty criteria
        };
        var context = new ValidationContext(request);
        
        // Act
        var results = request.Validate(context).ToList();
        
        // Assert
        results.Should().HaveCount(1);
        results[0].ErrorMessage.Should().Be("SearchCriteria is required when AdvancedSearch is enabled");
        results[0].MemberNames.Should().Contain(nameof(request.SearchCriteria));
    }
    
    [Fact]
    public void InteractionFilterRequest_ValidateNegativeIds_ShouldReturnErrors()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            Id = -1,
            ContactId = 0,
            PartnerId = -5
        };
        var context = new ValidationContext(request);
        
        // Act
        var results = request.Validate(context).ToList();
        
        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(r => r.ErrorMessage == "Id must be a positive number");
        results.Should().Contain(r => r.ErrorMessage == "ContactId must be a positive number");
        results.Should().Contain(r => r.ErrorMessage == "PartnerId must be a positive number");
    }
    
    [Fact]
    public void InteractionFilterRequest_ValidateValidRequest_ShouldReturnNoErrors()
    {
        // Arrange
        var request = new InteractionFilterRequest
        {
            Id = 1,
            ContactId = 2,
            PartnerId = 3,
            FromDate = new DateOnly(2024, 1, 1),
            ToDate = new DateOnly(2024, 6, 15),
            Date = new DateOnly(2024, 3, 15),
            AdvancedSearch = true,
            SearchCriteria = "valid criteria"
        };
        var context = new ValidationContext(request);
        
        // Act
        var results = request.Validate(context).ToList();
        
        // Assert
        results.Should().BeEmpty();
    }
}