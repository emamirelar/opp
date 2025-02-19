using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Domain.Infrastructure;

public interface IModifiableEntity<TId, TUserId>
{
    TUserId CreatedBy { get; set; }
    DateTime CreatedDate { get; set; }
    TUserId? LastModifiedBy { get; set; }
    DateTime? LastModifiedDate { get; set; }
    void SetCreateAuditData(TUserId userId);
    void SetUpdateAuditData(TUserId userId);
}

public interface IModifiableEntity : IModifiableEntity<int, int>
{
    
}

