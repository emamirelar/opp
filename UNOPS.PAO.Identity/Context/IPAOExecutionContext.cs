namespace UNOPS.PAO.Identity.Context;

using UNOPS.PAO.Identity.Security;

public interface IPAOExecutionContext
{
    public IEnumerable<Permission> UserPermissions { get; }
}
