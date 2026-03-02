using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Interfaces;

namespace UNOPS.PAO.Domain.Infrastructure;

public class ModifiableDeletableEntity<TId, TUserId> : IModifiableEntity<TId, TUserId>, IDeletableEntity<TUserId>, IBaseBusinessEntity<TId>, IStatusEntity
{
    public TId Id { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public EntityStatus Status { get; set; }
    
    /// <summary>
    /// Indicates whether this entity is currently in an approval workflow
    /// </summary>
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.None;
    
    /// <summary>
    /// Computed property - returns true if entity is in an active approval workflow
    /// </summary>
    public bool IsInWorkflow => WorkflowStatus == WorkflowStatus.InWorkflow;
    
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

    public bool IsDeleted { get; set; }
    public TUserId? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public void SetDeleteAuditData(TUserId deletedBy)
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow.ToUniversalTime();
        DeletedBy = deletedBy;
    }
}

public class ModifiableDeletableEntity : ModifiableDeletableEntity<int, int>
{
}