using System.Text.Json.Serialization;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models.OrganizationUnits;

public class OrganizationUnitRelationshipModel
{
    public int OrganizationHierarchyId { get; set; }
    public OrganizationHierarchyModel? OrganizationHierarchy { get; set; }
    public int EntityId { get; set; }
    public string EntityType { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public EntityStatus Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsDeleted { get; set; }
} 