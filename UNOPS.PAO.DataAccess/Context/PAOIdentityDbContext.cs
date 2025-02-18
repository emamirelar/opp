namespace UNOPS.PAO.DataAccess.Context;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Identity.Entities;

public class PAOIdentityDbContext : IdentityDbContext<PAOIdentityUser, PAOIdentityRole, int>
{
    public PAOIdentityDbContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<PAOIdentityUser>().Ignore(x => x.GoogleSignIn);
    }
}
