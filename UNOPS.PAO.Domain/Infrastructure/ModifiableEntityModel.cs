using System.Security.Cryptography;

namespace UNOPS.PAO.Domain.Infrastructure;

public class ModifiableEntityModel<TId, TUserId>
{
    public ModifiableEntityModel(ModifiableEntity<TId, TUserId> b)
    {
        Id = b.Id;
        CreatedBy = b.CreatedBy;
        CreatedDate = b.CreatedDate;
        LastModifiedBy = b.LastModifiedBy;
        LastModifiedDate = b.LastModifiedDate;
    }

    public TId Id { get; set; }
    public TUserId CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public TUserId? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
