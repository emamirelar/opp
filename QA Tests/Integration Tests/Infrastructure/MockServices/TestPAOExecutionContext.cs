using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Identity.Security;

namespace UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;

/// <summary>
/// Test implementation of IPAOExecutionContext that returns all permissions.
/// 
/// In production, PAOExecutionContext resolves user permissions from
/// Identity roles/claims via UserManager. In tests, the identity store
/// has no role-permission mappings, so UserPermissions returns empty,
/// causing PermissionHandler to call context.Fail() → 403 Forbidden.
/// 
/// This implementation returns all known permissions so that all
/// permission-based authorization checks succeed in tests.
/// </summary>
public sealed class TestPAOExecutionContext : IPAOExecutionContext
{
    public IEnumerable<Permission> UserPermissions { get; }

    public TestPAOExecutionContext()
    {
        // Return all static Permission fields via reflection so tests
        // automatically pick up any new permissions added to the Permission class
        var permissions = typeof(Permission)
            .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Where(f => f.FieldType == typeof(Permission))
            .Select(f => (Permission)f.GetValue(null)!)
            .ToList();

        UserPermissions = permissions;
    }
}
