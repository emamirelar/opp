using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Models.Audit;

public class ModifiableEntityModel<TId, TUserId>
{
    public ModifiableEntityModel(){}
    public ModifiableEntityModel(ModifiableEntity<TId, TUserId> b)
    {
        Id = b.Id;
        CreatedBy = b.CreatedBy;
        CreatedDate = b.CreatedDate;
        LastModifiedBy = b.LastModifiedBy;
        LastModifiedDate = b.LastModifiedDate;
    }

    public required TId Id { get; set; }
    public required TUserId CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public TUserId? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
