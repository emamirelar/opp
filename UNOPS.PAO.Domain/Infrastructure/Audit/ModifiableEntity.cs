using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Domain.Infrastructure;

public class ModifiableEntity<TId, TUserId>: BaseBusinessEntity<TId>, IModifiableEntity<TId, TUserId>
{
    public TUserId CreatedBy { get; set; } = default!;
    public DateTime CreatedDate { get; set; }
    public TUserId? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }

    public void SetCreateAuditData(TUserId userId)
    {
        CreatedDate = DateTime.UtcNow.ToUniversalTime();
        CreatedBy = userId;
    }

    public void SetUpdateAuditData(TUserId userId)
    {
        LastModifiedDate = DateTime.UtcNow.ToUniversalTime();
        LastModifiedBy = userId;
    }
}

public class ModifiableEntity : ModifiableEntity<int, int>
{
}

