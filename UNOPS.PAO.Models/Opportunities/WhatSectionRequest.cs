namespace UNOPS.PAO.Models.Opportunities;

/// <summary>
/// Request model for updating the WHAT section of an opportunity
/// Includes name, description, org unit, initiative type, delivery modality, and deliverables
/// </summary>
public class WhatSectionRequest
{
    /// <summary>
    /// Opportunity name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Opportunity description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Responsible organization unit ID
    /// </summary>
    public int? ResponsibleOrgUnitId { get; set; }

    /// <summary>
    /// Proposed initiative type ID
    /// </summary>
    public int? ProposedInitiativeTypeId { get; set; }
    
    /// <summary>
    /// Delivery modality for products and services
    /// 1 = NotYetKnown, 2 = AllDirect, 3 = AllGrantSupport, 4 = Mixed
    /// </summary>
    public int? DeliveryModality { get; set; }

    /// <summary>
    /// List of deliverables for the opportunity
    /// </summary>
    public List<OpportunityDeliverableRequest>? Deliverables { get; set; }
}


