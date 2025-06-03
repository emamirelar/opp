using System;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;


namespace UNOPS.PAO.Domain.Entities
{
    public class Interaction : ModifiableDeletableEntity
    {
        public int Id { get; set; }
        
        public InteractionType Type { get; set; }
        
        public DateTime Date { get; set; }
        
        public byte[]? Data { get; set; }

        public required int ContactId { get; set; }
        [JsonIgnore]  // Prevents circular reference in serialization
        public required virtual Contact Contact { get; set; } = null!;

        [JsonIgnore]
        public virtual List<string>? EmailAddresses { get; set; } = new List<string>();

        [JsonIgnore]
        public virtual List<string>? PhoneNumbers { get; set; } = new List<string>();

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

        public string Subject { get; set; }

        [JsonIgnore]  // Prevents circular reference in serialization
        public virtual OrganizationHierarchy? OrgUnit { get; set; }

        public int? OrgUnitId { get; set; }

        public List<Document>? Documents { get; set; }
        public string? GmailThreadId { get; set; }
    }
} 