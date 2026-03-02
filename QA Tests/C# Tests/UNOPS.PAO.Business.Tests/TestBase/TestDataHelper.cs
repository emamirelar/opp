using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Shared helper methods for creating test data that works correctly
/// with both SQLite and PostgreSQL databases.
/// </summary>
public static class TestDataHelper
{
    /// <summary>
    /// Creates or retrieves a test user in the AspNetUsers table.
    /// Uses raw SQL for PostgreSQL because PAOUser entity does not map all
    /// required AspNetUsers columns (EmailConfirmed, PhoneNumberConfirmed, etc.).
    /// For SQLite/InMemory, uses EF Core entity insertion.
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="email">The user's email address</param>
    /// <returns>The user's ID</returns>
    public static int GetOrCreateTestUser(AppDbContext context, string email)
    {
        var existing = context.PAOUsers.FirstOrDefault(u => u.Email == email);
        if (existing != null)
            return existing.Id;

        if (TestEnvironment.UsePostgreSQL)
        {
            var normalized = email.ToUpperInvariant();
            var result = context.Database.SqlQueryRaw<int>(
                "INSERT INTO \"AspNetUsers\" (\"IsInternal\", \"Email\", \"NormalizedEmail\", \"EmailConfirmed\", \"UserName\", \"NormalizedUserName\", " +
                "\"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\") " +
                "VALUES ({0}, {1}, {2}, true, {3}, {4}, false, false, false, 0) RETURNING \"Id\"",
                false, email, normalized, email, normalized).ToList();
            return result[0];
        }
        else
        {
            var user = new UNOPS.PAO.Domain.Entities.PAOUser { Email = email };
            context.PAOUsers.Add(user);
            context.SaveChanges();
            return user.Id;
        }
    }

    /// <summary>
    /// Async version of GetOrCreateTestUser.
    /// </summary>
    public static async Task<int> GetOrCreateTestUserAsync(AppDbContext context, string email)
    {
        var existing = await context.PAOUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (existing != null)
            return existing.Id;

        if (TestEnvironment.UsePostgreSQL)
        {
            var normalized = email.ToUpperInvariant();
            var result = await context.Database.SqlQueryRaw<int>(
                "INSERT INTO \"AspNetUsers\" (\"IsInternal\", \"Email\", \"NormalizedEmail\", \"EmailConfirmed\", \"UserName\", \"NormalizedUserName\", " +
                "\"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\") " +
                "VALUES ({0}, {1}, {2}, true, {3}, {4}, false, false, false, 0) RETURNING \"Id\"",
                false, email, normalized, email, normalized).ToListAsync();
            return result[0];
        }
        else
        {
            var user = new UNOPS.PAO.Domain.Entities.PAOUser { Email = email };
            context.PAOUsers.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }
    }
}
