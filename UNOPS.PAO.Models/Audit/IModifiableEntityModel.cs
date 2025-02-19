using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Models.Audit;

public interface IModifiableEntityModel<TId, TUserId>
{
    public TId Id { get; set; }
    public TUserId CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public TUserId? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}
