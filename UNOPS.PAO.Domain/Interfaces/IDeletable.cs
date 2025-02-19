namespace UNOPS.PAO.Domain.Interfaces;

public interface IDeletable<TUserId>
{
    public bool IsDeleted { get; set; }
    public TUserId? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    public void SetDeletedAuditData(TUserId userId)
    {
        IsDeleted = true;
        DeletedBy = userId;
        DeletedOn = DateTime.UtcNow.ToUniversalTime();
    }
}

public interface IDeletable : IDeletable<int>
{ 
}