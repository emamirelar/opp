using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace UNOPS.PAO.Domain.Entities;

public class Partner : ModifiableDeletableEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string NewEngagement { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address1Street { get; set; }
    public string? Address1Street2 { get; set; }
    public string? Address1City { get; set; }
    public string? Address1StateProvince { get; set; }
    public string? Address1PostalCode { get; set; }
    public string? Address1Country { get; set; }
    public string ShortName { get; set; }
    //Level
    //Group
    //LiaisonOffice
    public string PooledFund { get; set; }
    public string DDRequired { get; set; }
    public string DDEACDone { get; set; }
    public string? EACReference { get; set; }
    public Boolean GlobalKeyAccount { get; set; }
    public Boolean UNSecretariatEntity { get; set; }
    public string LevyPotentiallyApplies { get; set; }
    public string? ReasonForLevyNotApplying { get; set; }
    public string? LevyTreatment { get; set; }
    public string? LogoUrl { get; set; }
    public List<Document>? Documents { get; set; }
    
    // Navigation property for organization unit relationships
    public virtual ICollection<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; } = new HashSet<OrganizationUnitRelationship>();
    
    [ForeignKey("PartnerGroupCode")]
    public PartnerTree? PartnerGroup { get; set; }
    
    public string? PartnerGroupCode { get; set; }

    
    // Collection of all contacts for this partner
    public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    
    // Computed property to get the first 5 contacts ordered by creation date (newest first)
    [NotMapped]
    public IEnumerable<Contact> First5ContactsByDate => 
        Contacts?.Where(c => c != null)
                 .OrderByDescending(c => c.CreatedDate == DateTime.MinValue ? DateTime.MinValue : c.CreatedDate)
                 .ThenByDescending(c => c.Id) // Fallback ordering by Id when CreatedDate is default
                 .Take(5) ?? Enumerable.Empty<Contact>();

    /// <summary>
    /// Gets all interactions related to this partner through its contacts
    /// </summary>
    public IEnumerable<Interaction> GetAllInteractions()
    {
        if (Contacts == null || !Contacts.Any())
            return Enumerable.Empty<Interaction>();

        return Contacts
            .Where(c => c.Interactions != null)
            .SelectMany(c => c.Interactions)
            .OrderByDescending(i => i.Date);
    }

    /// <summary>
    /// Gets recent interactions (last 10) for this partner
    /// </summary>
    public IEnumerable<Interaction> GetRecentInteractions(int count = 10)
    {
        return GetAllInteractions().Take(count);
    }

    /// <summary>
    /// Gets interactions grouped by contact for this partner
    /// </summary>
    public Dictionary<Contact, IEnumerable<Interaction>> GetInteractionsByContact()
    {
        if (Contacts == null || !Contacts.Any())
            return new Dictionary<Contact, IEnumerable<Interaction>>();

        return Contacts
            .Where(c => c.Interactions != null && c.Interactions.Any())
            .ToDictionary(
                contact => contact,
                contact => (IEnumerable<Interaction>)contact.Interactions.OrderByDescending(i => i.Date)
            );
    }

    /// <summary>
    /// Gets the count of all interactions for this partner
    /// </summary>
    public int GetTotalInteractionsCount()
    {
        return Contacts?.Sum(c => c.Interactions?.Count ?? 0) ?? 0;
    }

    /// <summary>
    /// Gets the date of the most recent interaction for this partner
    /// </summary>
    public DateTime? GetLastInteractionDate()
    {
        return GetAllInteractions().FirstOrDefault()?.Date;
    }

    /// <summary>
    /// Gets interactions by type for this partner
    /// </summary>
    public IEnumerable<Interaction> GetInteractionsByType(Domain.Enums.InteractionType type)
    {
        return GetAllInteractions().Where(i => i.Type == type);
    }

    /// <summary>
    /// Gets contact and interaction summary information
    /// </summary>
    public (int ContactsCount, int InteractionsCount, DateTime? LastInteractionDate) GetSummary()
    {
        var contactsCount = Contacts?.Count ?? 0;
        var interactionsCount = GetTotalInteractionsCount();
        var lastInteractionDate = GetLastInteractionDate();

        return (contactsCount, interactionsCount, lastInteractionDate);
    }

    
    /// <summary>
    /// Adds an organization unit relationship
    /// </summary>
    public void AddOrganizationUnitRelationship(OrganizationHierarchy organizationHierarchy)
    {
        if (organizationHierarchy == null) return;

        var relationship = new OrganizationUnitRelationship
        {
            OrganizationHierarchy = organizationHierarchy,
            EntityId = this.Id,
            EntityType = nameof(Partner),
            Name = $"Partner-{this.Id}-{organizationHierarchy.Code}",
            Status = EntityStatus.Active
        };

        OrganizationUnitRelationships.Add(relationship);
    }

    /// <summary>
    /// Removes an organization unit relationship
    /// </summary>
    public void RemoveOrganizationUnitRelationship(int organizationHierarchyId)
    {
        var relationshipsToRemove = OrganizationUnitRelationships
            .Where(r => r.OrganizationHierarchyId == organizationHierarchyId);

        foreach (var relationship in relationshipsToRemove.ToList())
        {
            OrganizationUnitRelationships.Remove(relationship);
        }
    }
}

