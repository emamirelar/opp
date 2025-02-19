using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Domain.Infrastructure;

public interface IDeletableEntity<TUserId>
{
    bool IsDeleted { get; set; }
    TUserId? DeletedBy { get; set; }
    DateTime? DeletedDate { get; set; }
    void SetDeleteAuditData(TUserId deletedBy);
}
