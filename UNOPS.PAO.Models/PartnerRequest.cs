namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;

public class PartnerRequest : ExtensibleModel
{
    public string? Name { get; set; }
    public string Status { get; set; } = "Active";
    public string? NewEngagement { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address1Street { get; set; }
    public string? Address1Street2 { get; set; }
    public string? Address1City { get; set; }
    public string? Address1StateProvince { get; set; }
    public string? Address1PostalCode { get; set; }
    public string? Address1Country { get; set; }
    public string? ShortName { get; set; }
    //Level
    //Group
    //LiaisonOffice
    public string? PooledFund { get; set; }
    public string? DDRequired { get; set; }
    public string? DDEACDone { get; set; }
    public string? EACReference { get; set; }
    public bool? GlobalKeyAccount { get; set; } = false;
    public bool? UNSecretariatEntity { get; set; } = false;
    public string? LevyPotentiallyApplies { get; set; }
    public string? ReasonForLevyNotApplying { get; set; }
    public string? LevyTreatment { get; set; }
    //public List<int>? ContactIds { get; set; }
    
    // Replace OrganizationHierarchyId with OrganizationUnitRelationships
    public List<OrganizationUnitRelationshipRequest>? OrganizationUnitRelationships { get; set; }
    public string? PartnerGroupCode { get; set; }
}

/// <summary>
/// Request model for organization unit relationships
/// </summary>
public class OrganizationUnitRelationshipRequest
{
    public int OrganizationHierarchyId { get; set; }
    public string EntityType { get; set; } = "Partner";
}