using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests pour valider le filtre "My Office" basé sur l'unité organisationnelle de l'utilisateur
/// </summary>
public class MyOfficeFilterTests
{
    [Fact]
    public void PartnerMyOfficeFilter_WithUserOrgUnit_ShouldFilterCorrectly()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var partners = GetTestPartnersWithOffices();

        // Act - Utiliser la spécification MyOffice directement
        var specification = new UNOPSPartnerByMyOfficeSpecification(userOrgUnit);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(2, "Should find partners from NYC office");
        filteredPartners.Should().OnlyContain(p => p.PartnerOffice != null && p.PartnerOffice.Code == userOrgUnit);
        
        var expectedNames = new[] { "NYC Tech Solutions", "NYC Finance Corp" };
        filteredPartners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void ContactMyOfficeFilter_WithUserOrgUnit_ShouldFilterCorrectly()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var contacts = GetTestContactsWithPartnerOffices();

        // Act - Utiliser la spécification MyOffice directement
        var specification = new UNOPSContactByMyOfficeSpecification(userOrgUnit);
        var filteredContacts = contacts
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert
        filteredContacts.Should().HaveCount(2, "Should find contacts from partners in NYC office");
        filteredContacts.Should().OnlyContain(c => c.Partner != null && c.Partner.PartnerOffice != null && c.Partner.PartnerOffice.Code == userOrgUnit);
        
        var expectedFirstNames = new[] { "John", "Jane" };
        filteredContacts.Select(c => c.FirstName).Should().BeEquivalentTo(expectedFirstNames);
    }

    [Fact]
    public void InteractionMyOfficeFilter_WithUserOrgUnit_ShouldFilterCorrectly()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var interactions = GetTestInteractionsWithContactPartnerOffices();

        // Act - Utiliser la spécification MyOffice directement
        var specification = new InteractionByMyOfficeSpecification(userOrgUnit);
        var filteredInteractions = interactions
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert
        filteredInteractions.Should().HaveCount(2, "Should find interactions from contacts in NYC office");
        filteredInteractions.Should().OnlyContain(i => 
            i.InteractionContacts != null && 
            i.InteractionContacts.Any(ic => 
                ic.Contact != null && 
                ic.Contact.Partner != null && 
                ic.Contact.Partner.PartnerOffice != null && 
                ic.Contact.Partner.PartnerOffice.Code == userOrgUnit));
        
        var expectedSubjects = new[] { "NYC Meeting", "NYC Follow-up" };
        filteredInteractions.Select(i => i.Subject).Should().BeEquivalentTo(expectedSubjects);
    }

    [Fact]
    public void PartnerCompositeWithMyOffice_MyOfficeOnlyTrue_ShouldCombineFilters()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var partners = GetTestPartnersWithOffices();
        
        var filter = new PartnerFilterRequest
        {
            Status = "Active",
            MyOfficeOnly = true
        };

        // Act - Utiliser la spécification composite avec MyOffice
        var specification = new UNOPSPartnerCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert - Doit être Active ET de NYC
        filteredPartners.Should().HaveCount(1, "Should find active partners from NYC office only");
        filteredPartners.Single().Name.Should().Be("NYC Tech Solutions");
        filteredPartners.Single().Status.Should().Be("Active");
        filteredPartners.Single().PartnerOffice!.Code.Should().Be(userOrgUnit);
    }

    [Fact]
    public void PartnerCompositeWithMyOffice_MyOfficeOnlyFalse_ShouldIgnoreOfficeFilter()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var partners = GetTestPartnersWithOffices();
        
        var filter = new PartnerFilterRequest
        {
            Status = "Active",
            MyOfficeOnly = false // Important: MyOffice désactivé
        };

        // Act - Utiliser la spécification composite sans MyOffice
        var specification = new UNOPSPartnerCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert - Doit trouver tous les Active, pas seulement NYC
        filteredPartners.Should().HaveCount(3, "Should find all active partners regardless of office");
        filteredPartners.Should().OnlyContain(p => p.Status == "Active");
        
        var expectedNames = new[] { "NYC Tech Solutions", "London Corp", "Tokyo Industries" };
        filteredPartners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void ContactCompositeWithMyOffice_MyOfficeOnlyTrue_ShouldCombineFilters()
    {
        // Arrange
        var userOrgUnit = "NYC";
        var contacts = GetTestContactsWithPartnerOffices();
        
        var filter = new ContactFilterRequest
        {
            Title = "Manager",
            MyOfficeOnly = true
        };

        // Act
        var specification = new UNOPSContactCompositeWithMyOfficeSpecification(filter, userOrgUnit);
        var filteredContacts = contacts
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert - Doit être Manager ET de NYC
        filteredContacts.Should().HaveCount(1, "Should find managers from NYC office only");
        filteredContacts.Single().FirstName.Should().Be("John");
        filteredContacts.Single().Title.Should().Be("Manager");
        filteredContacts.Single().Partner!.PartnerOffice!.Code.Should().Be(userOrgUnit);
    }

    [Fact]
    public void MyOfficeFilter_WithEmptyOrgUnit_ShouldReturnNoResults()
    {
        // Arrange
        var emptyOrgUnit = "";
        var partners = GetTestPartnersWithOffices();

        // Act - Tester avec OrgUnit vide pour sécurité
        var specification = new UNOPSPartnerByMyOfficeSpecification(emptyOrgUnit);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert - Pour sécurité, retourner aucun résultat si OrgUnit vide
        filteredPartners.Should().BeEmpty("Should return no results for empty org unit for security");
    }

    [Fact]
    public void MyOfficeFilter_WithNullOrgUnit_ShouldReturnNoResults()
    {
        // Arrange
        string? nullOrgUnit = null;
        var partners = GetTestPartnersWithOffices();

        // Act - Tester avec OrgUnit null pour sécurité
        var specification = new UNOPSPartnerByMyOfficeSpecification(nullOrgUnit!);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert - Pour sécurité, retourner aucun résultat si OrgUnit null
        filteredPartners.Should().BeEmpty("Should return no results for null org unit for security");
    }

    [Theory]
    [InlineData("NYC", 2)] // NYC office has 2 partners
    [InlineData("LON", 1)] // London office has 1 partner  
    [InlineData("TKY", 1)] // Tokyo office has 1 partner
    [InlineData("PAR", 0)] // Paris office has no partners
    [InlineData("INVALID", 0)] // Invalid office code
    public void PartnerMyOfficeFilter_WithDifferentOrgUnits_ShouldReturnCorrectCounts(string orgUnit, int expectedCount)
    {
        // Arrange
        var partners = GetTestPartnersWithOffices();

        // Act
        var specification = new UNOPSPartnerByMyOfficeSpecification(orgUnit);
        var filteredPartners = partners
            .Where(specification.Criteria.Compile())
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(expectedCount, 
            $"Should find {expectedCount} partners for org unit {orgUnit}");
    }

    #region Helper Methods

    private static List<UNOPSPartner> GetTestPartnersWithOffices()
    {
        return new List<UNOPSPartner>
        {
            CreatePartnerWithOffice("NYC Tech Solutions", "Active", "NYC"),
            CreatePartnerWithOffice("NYC Finance Corp", "Inactive", "NYC"),
            CreatePartnerWithOffice("London Corp", "Active", "LON"),
            CreatePartnerWithOffice("Tokyo Industries", "Active", "TKY"),
            CreatePartnerWithOffice("Zurich Bank", "Prospect", "ZUR")
        };
    }

    private static List<UNOPSContact> GetTestContactsWithPartnerOffices()
    {
        var partners = GetTestPartnersWithOffices();
        
        return new List<UNOPSContact>
        {
            CreateContactWithPartner("John", "Smith", "Manager", partners[0]), // NYC
            CreateContactWithPartner("Jane", "Doe", "Director", partners[1]), // NYC
            CreateContactWithPartner("Bob", "Wilson", "Manager", partners[2]), // London
            CreateContactWithPartner("Alice", "Brown", "Analyst", partners[3]) // Tokyo
        };
    }

    private static List<UNOPSInteraction> GetTestInteractionsWithContactPartnerOffices()
    {
        var contacts = GetTestContactsWithPartnerOffices();
        
        return new List<UNOPSInteraction>
        {
            CreateInteractionWithContact("NYC Meeting", contacts[0]), // NYC via John
            CreateInteractionWithContact("NYC Follow-up", contacts[1]), // NYC via Jane
            CreateInteractionWithContact("London Call", contacts[2]), // London via Bob
            CreateInteractionWithContact("Tokyo Review", contacts[3]) // Tokyo via Alice
        };
    }

    private static UNOPSPartner CreatePartnerWithOffice(string name, string status, string officeCode)
    {
        var office = new OrganizationHierarchy
        {
            Id = Random.Shared.Next(1, 1000),
            Code = officeCode,
            Name = $"{officeCode} Office",
            Type = Domain.Enums.OrganizationUnitType.Office
        };

        return new UNOPSPartner
        {
            Id = Random.Shared.Next(1, 1000),
            Name = name,
            Status = status,
            ShortName = name.Length > 10 ? name.Substring(0, 10) : name,
            PartnerOffice = office,
            PartnerOfficeId = office.Id,
            NewEngagement = "true",
            PooledFund = "false",
            DDRequired = "false",
            DDEACDone = "false",
            LevyPotentiallyApplies = "false",
            PartnerCode = $"P{Random.Shared.Next(1000, 9999)}",
            PartnerGroupCode = "NGO",
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 100)),
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private static UNOPSContact CreateContactWithPartner(string firstName, string lastName, string title, UNOPSPartner partner)
    {
        return new UNOPSContact
        {
            Id = Random.Shared.Next(1, 1000),
            FirstName = firstName,
            LastName = lastName,
            Title = title,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@{partner.Name.Replace(" ", "").ToLower()}.com",
            Partner = partner,
            PartnerId = partner.Id,
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 50)),
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private static UNOPSInteraction CreateInteractionWithContact(string subject, UNOPSContact contact)
    {
        var interaction = new UNOPSInteraction
        {
            Id = Random.Shared.Next(1, 1000),
            Subject = subject,
            Description = $"Interaction with {contact.FirstName} {contact.LastName}",
            Date = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
            Type = InteractionType.InPersonMeeting,
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
            LastModifiedDate = DateTime.UtcNow,
            InteractionContacts = new List<InteractionContact>()
        };

        // Create the many-to-many relationship
        var interactionContact = new InteractionContact
        {
            InteractionId = interaction.Id,
            ContactId = contact.Id,
            Interaction = interaction,
            Contact = contact
        };

        interaction.InteractionContacts.Add(interactionContact);

        return interaction;
    }

    #endregion
}