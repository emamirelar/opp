using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Domain.Interfaces;

public interface IStatusEntity
{
    public EntityStatus Status { get; set; }
}
