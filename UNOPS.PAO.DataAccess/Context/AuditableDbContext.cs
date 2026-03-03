using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using System.Collections.Generic;

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
                // Check if CreatedBy has been explicitly set (not default value)
                var defaultUserId = default(TUserId);
                var currentCreatedBy = created.CreatedBy;
                
                // Only set CreatedBy if it hasn't been explicitly set or is default value
                if (EqualityComparer<TUserId>.Default.Equals(currentCreatedBy, defaultUserId))
                {
                    created.SetCreateAuditData(_currentUserId!);
                }
                else
                {
                    // CreatedBy was explicitly set, only set CreatedDate
                    created.CreatedDate = DateTime.UtcNow.ToUniversalTime();
                }

                created.SetUpdateAuditData(_currentUserId!);
            }
            
            if (entry is { Entity: IModifiableEntity<TId, TUserId> modifiable, State: EntityState.Modified})
            {
                modifiable.SetUpdateAuditData(_currentUserId!);
            }

            if (entry is { Entity: IDeletableEntity<TUserId> deletable, State: EntityState.Deleted })
            {
                // Perform soft delete
                entry.State = EntityState.Modified;
                deletable.SetDeleteAuditData(_currentUserId!);
            }
        }
    }
}
