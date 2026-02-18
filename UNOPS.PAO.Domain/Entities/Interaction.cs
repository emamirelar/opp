using System;
using System.Linq;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;


namespace UNOPS.PAO.Domain.Entities
{
    public class Interaction : ModifiableDeletableEntity
    {
        public new int Id { get; set; }

        /// <summary>
        /// Direct FK to Contact for Contact.Interactions navigation (legacy/simple relationship).
        /// Many-to-many is also supported via InteractionContacts.
        /// </summary>
        public int? ContactId { get; set; }

        public InteractionType Type { get; set; }
        
        public DateTime Date { get; set; }
        
        public string? Description { get; set; }

        // Note: Contact relationships are now handled through InteractionContacts many-to-many table

        [JsonIgnore]
        public virtual List<string>? EmailAddresses { get; set; } = new List<string>();

        // Many-to-many with Contacts
        [JsonIgnore]
        public virtual ICollection<InteractionContact>? InteractionContacts { get; set; }

        // Many-to-many with Partners
        [JsonIgnore]
        public virtual ICollection<InteractionPartner>? InteractionPartners { get; set; }

        // Many-to-many with Users
        [JsonIgnore]
        public virtual ICollection<InteractionUser>? InteractionUsers { get; set; }

        //can not make this a lookup / enum as there can be a lot of combinations for city/country
        public string? Location { get; set; }

        public required string Subject { get; set; }

        // Many-to-many relationship with OrganizationHierarchy through OrganizationUnitRelationships
        // This replaces the direct OrgUnitId/OrgUnit relationship to support multiple org unit associations
        [JsonIgnore]
        public virtual ICollection<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; } = new HashSet<OrganizationUnitRelationship>();

        public List<Document>? Documents { get; set; }
        public string? GmailThreadId { get; set; }
        [MaxLength(80)]
        public string? GmailMessageId { get; set; }

        // Computed properties for comma-separated lists
        [NotMapped]
        public string InteractionContactsList => 
            string.Join(", ", InteractionContacts?
                .Where(ic => ic?.Contact != null)
                .Select(ic => $"{ic.Contact.FirstName} {ic.Contact.LastName}".Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .OrderBy(name => name) ?? Enumerable.Empty<string>());

        [NotMapped]
        public string InteractionPartnersList => 
            string.Join(", ", InteractionPartners?
                .Where(ip => ip?.Partner != null && !string.IsNullOrWhiteSpace(ip.Partner.Name))
                .Select(ip => ip.Partner.Name)
                .OrderBy(name => name) ?? Enumerable.Empty<string>());

        [NotMapped]
        public string InteractionUsersList => 
            string.Join(", ", InteractionUsers?
                .Where(iu => iu?.User != null && !string.IsNullOrWhiteSpace(iu.User.Name))
                .Select(iu => iu.User.Name)
                .OrderBy(name => name) ?? Enumerable.Empty<string>());

        [NotMapped]
        public string InteractionOrgUnits => 
            string.Join(", ", OrganizationUnitRelationships?
                .Where(r => r?.OrganizationHierarchy != null && r.Status == EntityStatus.Active && !r.IsDeleted)
                .Select(r => r.OrganizationHierarchy.Name)
                .OrderBy(name => name) ?? Enumerable.Empty<string>());
    }
}