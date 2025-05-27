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
    public OrganizationHierarchy? PartnerOffice { get; set; }
    public int? PartnerOfficeId { get; set; }
    
    [ForeignKey("PartnerGroupCode")]
    public PartnerTree? PartnerGroup { get; set; }
    
    public string? PartnerGroupCode { get; set; }

    
    // Collection of all contacts for this partner
    public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    
    // Computed property to get the first 5 contacts ordered by creation date (newest first)
    [NotMapped]
    public IEnumerable<Contact> First5ContactsByDate => 
        Contacts?.OrderByDescending(c => c.CreatedDate).Take(5) ?? Enumerable.Empty<Contact>();
}

