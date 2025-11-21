using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models.Partners;

public class PartnerValueModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? OrganizationHierarchyId { get; set; }
    public string? LogoUrl { get; set; }
    public bool PooledFund { get; set; }
}