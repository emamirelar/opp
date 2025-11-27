using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Models;

public class OpportunityClientPartnerModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public string? PartnerLogoUrl { get; set; }
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public List<DocumentDetailModel>? AssociatedDocuments { get; set; }
    
    /// <summary>
    /// Partner's current status (Draft/Active/Closed/Archived)
    /// </summary>
    public string? PartnerStatus { get; set; }
    
    /// <summary>
    /// Partner's approval status (Approved/NotApproved)
    /// </summary>
    public string? PartnerApprovalStatus { get; set; }
    
    /// <summary>
    /// Due Diligence approval status (NotRequired/Required/NotApproved/Approved)
    /// </summary>
    public string? DDApproval { get; set; }
    
    /// <summary>
    /// Due Diligence approval date
    /// </summary>
    public DateTime? DDApprovalDate { get; set; }
    
    /// <summary>
    /// Due Diligence expiry date
    /// </summary>
    public DateTime? DDExpiryDate { get; set; }
    
    /// <summary>
    /// Computed: DD status based on expiry date
    /// </summary>
    public string? DDStatus { get; set; }
    
    /// <summary>
    /// Computed: Whether DD expires before opportunity end
    /// </summary>
    public bool? DDExpiresBeforeOpportunityEnd { get; set; }
    
    /// <summary>
    /// Selected Partner Agreement Number
    /// </summary>
    public string? SelectedPartnerAgreementNumber { get; set; }
    
    /// <summary>
    /// Available partner agreements for this partner
    /// </summary>
    public List<PartnerAgreementInfo>? AvailableAgreements { get; set; }
}
