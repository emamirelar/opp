using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models;

public class PartnerValueModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? OrganizationHierarchyId { get; set; }
}