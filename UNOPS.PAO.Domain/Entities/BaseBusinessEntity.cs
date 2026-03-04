namespace UNOPS.PAO.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Infrastructure;

public class BaseBusinessEntity<TId> : IBaseBusinessEntity<TId>
{
    public TId Id { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public EntityStatus Status { get; set; }
}
public class BaseBusinessEntity : BaseBusinessEntity<int>
{
}

public interface IBaseBusinessEntity<TId> 
{
    TId Id { get; set; }
    string Name { get; set; }
    EntityStatus Status { get; set; }
}