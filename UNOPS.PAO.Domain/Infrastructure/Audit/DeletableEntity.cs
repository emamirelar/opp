namespace UNOPS.PAO.Domain.Infrastructure;

public class DeletableEntity<TUserId> : IDeletableEntity<TUserId>
{
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

public abstract class DeletableEntity: DeletableEntity<int> 
{

}