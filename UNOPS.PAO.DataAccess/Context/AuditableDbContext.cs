using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.DataAccess.Context;

public class AuditableDbContext<TId, TUserId> : DbContext, IDbContextSchema
{
    private readonly TUserId? _currentUserId;
    private readonly UserResolverService<TUserId> _userResolverService;
    public string Schema { get; set; }
    public AuditableDbContext(DbContextOptions options, UserResolverService<TUserId> userResolverService, IDbContextSchema schema) : base(options)
    {
        _userResolverService = userResolverService;
        _currentUserId = _userResolverService.GetCurrentUserId();
        Schema = schema.Schema;
    }
    
    public AuditableDbContext(string connectionString)
        : base(new DbContextOptionsBuilder().UseNpgsql(connectionString).Options)
    {
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry is { Entity: IModifiableEntity<TId, TUserId> created, State: EntityState.Added })
            {
                created.SetCreateAuditData(_currentUserId);
                created.SetUpdateAuditData(_currentUserId);
            }
            
            if (entry is { Entity: IModifiableEntity<TId, TUserId> modifiable, State: EntityState.Modified})
            {
                modifiable.SetUpdateAuditData(_currentUserId);
            }

            if (entry is { Entity: IDeletableEntity<TUserId> deletable, State: EntityState.Deleted })
            {
                // Perform soft delete
                entry.State = EntityState.Modified;
                deletable.SetDeleteAuditData(_currentUserId);
            }
        }
    }
}
