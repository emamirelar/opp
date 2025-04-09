using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models;

public class PartnerModel
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
    //public List<ContactModel>? Contacts { get; set; }
    public List<DocumentModel>? Documents { get; set; }
    public OrganizationUnit? PartnerOffice { get; set; }
    public int? PartnerOfficeId { get; set; }
    public PartnerCategory? PartnerCategory { get; set; }
    public int? PartnerCategoryId { get; set; }
}