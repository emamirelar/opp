using System;
using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Partner Agreement master data entity
/// Data synced from External Data Service - Read Only
/// Tracks MoUs and Partnership Agreements between UNOPS and partners
/// </summary>
public class PartnerAgreement : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    
    /// <summary>
    /// Partner Agreement Description (short)
    /// Maps to IBaseBusinessEntity.Name requirement
    /// </summary>
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Entity status (read-only entities default to Active)
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Base Partner Agreement Number (e.g., "10039")
    /// </summary>
    [MaxLength(50)]
    public required string BasePartnerAgreementNumber { get; set; }
    
    /// <summary>
    /// Full Partner Agreement Number including amendments (e.g., "10039-00")
    /// </summary>
    [MaxLength(50)]
    public required string PartnerAgreementNumber { get; set; }
    
    /// <summary>
    /// Partner Agreement Description (long version)
    /// </summary>
    [MaxLength(4000)]
    public string? PartnerAgreementDescriptionLong { get; set; }
    
    /// <summary>
    /// Agreement Type (e.g., "FRAMEWORK", "PROJECT")
    /// </summary>
    [MaxLength(50)]
    public string? PartnerAgreementType { get; set; }
    
    /// <summary>
    /// Agreement Type Description
    /// </summary>
    [MaxLength(255)]
    public string? PartnerAgreementTypeDescription { get; set; }
    
    /// <summary>
    /// Agreement Scope (e.g., "GLOBAL", "REGIONAL", "COUNTRY")
    /// </summary>
    [MaxLength(50)]
    public string? PartnerAgreementScope { get; set; }
    
    /// <summary>
    /// Agreement Scope Description
    /// </summary>
    [MaxLength(255)]
    public string? PartnerAgreementScopeDescription { get; set; }
    
    /// <summary>
    /// Partner Number from ERP (links to Partner entity)
    /// </summary>
    [MaxLength(50)]
    public string? PartnerAgreementPartner { get; set; }
    
    /// <summary>
    /// Partner Name/Description
    /// </summary>
    [MaxLength(500)]
    public string? PartnerAgreementPartnerDescription { get; set; }
    
    /// <summary>
    /// Agreement Start Date
    /// </summary>
    public DateTime? PartnerAgreementStartDate { get; set; }
    
    /// <summary>
    /// Agreement End Date
    /// </summary>
    public DateTime? PartnerAgreementEndDate { get; set; }
    
    /// <summary>
    /// Agreement Signed Date
    /// </summary>
    public DateTime? PartnerAgreementSignedDate { get; set; }
    
    /// <summary>
    /// Responsible Organization Unit Code
    /// </summary>
    [MaxLength(50)]
    public string? PartnerAgreementResponsibleOrgUnit { get; set; }
    
    /// <summary>
    /// Responsible Organization Unit Description
    /// </summary>
    [MaxLength(500)]
    public string? PartnerAgreementResponsibleOrgUnitDescription { get; set; }
    
    // Service Line Flags (for determining what services the agreement covers)
    public bool PartnerAgreementServiceLineInfrastructureFlag { get; set; }
    public bool PartnerAgreementServiceLineProcurementFlag { get; set; }
    public bool PartnerAgreementServiceLineProjectManagementFlag { get; set; }
    public bool PartnerAgreementServiceLineFundManagementFlag { get; set; }
    public bool PartnerAgreementServiceLineHumanResourcesFlag { get; set; }
    public bool PartnerAgreementServiceLineOtherFlag { get; set; }
    
    /// <summary>
    /// Comma-separated list of countries this agreement applies to
    /// Empty if GLOBAL scope
    /// </summary>
    [MaxLength(4000)]
    public string? PartnerAgreementCountries { get; set; }
}

