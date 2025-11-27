namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Represents the delivery modality for products and services in an opportunity.
/// The Opportunity Manager must indicate whether UNOPS will be directly delivering
/// any/all Product or Service(s) or if they will be delivered via a Grant Support modality.
/// </summary>
public enum DeliveryModality
{
    /// <summary>
    /// Not yet known whether UNOPS will deliver directly or via Grant Support modality
    /// </summary>
    NotYetKnown = 1,
    
    /// <summary>
    /// UNOPS will be delivering all Products & Services directly
    /// </summary>
    AllDirect = 2,
    
    /// <summary>
    /// All Products & Services will be delivered via Grant Support
    /// </summary>
    AllGrantSupport = 3,
    
    /// <summary>
    /// Some of the Products and Services will be delivered via Grant Support Modality
    /// </summary>
    Mixed = 4
}

