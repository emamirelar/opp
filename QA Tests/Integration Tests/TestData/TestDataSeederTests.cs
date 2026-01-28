using Xunit;
using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.IntegrationTests.TestData;

/// <summary>
/// Unit tests for TestDataSeeder to validate test data generation
/// </summary>
public class TestDataSeederTests
{
    [Fact]
    public void CreateContactWithValidRelations_SetsAllRequiredProperties()
    {
        // Act
        var contact = TestDataSeeder.CreateContactWithValidRelations();
        
        // Assert - Verify all required properties are set
        contact.Should().NotBeNull();
        contact.Name.Should().NotBeNullOrEmpty("Name is required by ModifiableDeletableEntity");
        contact.LastName.Should().NotBeNullOrEmpty("LastName is required by Contact entity");
        contact.Title.Should().NotBeNullOrEmpty("Title is required by Contact entity");
        contact.Email.Should().NotBeNullOrEmpty("Email is required by Contact entity");
    }
    
    [Fact]
    public void CreateContactWithValidRelations_WithPartnerId_LinksToPartner()
    {
        // Arrange
        var partnerId = 123;
        
        // Act
        var contact = TestDataSeeder.CreateContactWithValidRelations(partnerId);
        
        // Assert
        contact.PartnerId.Should().Be(partnerId);
    }
    
    [Fact]
    public void CreateContactWithValidRelations_WithStatus_SetsCorrectEnum()
    {
        // Act
        var activeContact = TestDataSeeder.CreateContactWithValidRelations(status: "Active");
        var inactiveContact = TestDataSeeder.CreateContactWithValidRelations(status: "Inactive");
        var draftContact = TestDataSeeder.CreateContactWithValidRelations(status: "Draft");
        
        // Assert
        activeContact.Status.Should().Be(EntityStatus.Active);
        inactiveContact.Status.Should().Be(EntityStatus.Closed);
        draftContact.Status.Should().Be(EntityStatus.Draft);
    }
    
    [Fact]
    public void CreateContactsForPartner_CreatesCorrectCount()
    {
        // Arrange
        var partnerId = 456;
        var count = 5;
        
        // Act
        var contacts = TestDataSeeder.CreateContactsForPartner(partnerId, count);
        
        // Assert
        contacts.Should().HaveCount(count);
        contacts.Should().AllSatisfy(c =>
        {
            c.PartnerId.Should().Be(partnerId);
            c.Name.Should().NotBeNullOrEmpty();
            c.LastName.Should().NotBeNullOrEmpty();
            c.Title.Should().NotBeNullOrEmpty();
            c.Email.Should().NotBeNullOrEmpty();
        });
    }
    
    [Fact]
    public void GetContactFaker_GeneratesValidContacts()
    {
        // Act
        var faker = TestDataBuilder.GetContactFaker();
        var contact = faker.Generate();
        
        // Assert
        contact.Should().NotBeNull();
        contact.Name.Should().NotBeNullOrEmpty();
        contact.FirstName.Should().NotBeNullOrEmpty();
        contact.LastName.Should().NotBeNullOrEmpty();
        contact.Title.Should().NotBeNullOrEmpty();
        contact.Email.Should().NotBeNullOrEmpty();
        
        // Verify email format is valid
        contact.Email.Should().Contain("@");
    }
}
