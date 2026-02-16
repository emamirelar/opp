using Xunit;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Custom xUnit [Fact] attribute that was previously used to skip tests when the
/// InMemory provider was active. Now that the fallback uses SQLite in-memory
/// (which supports relational model features, raw SQL, and Z.EntityFramework.Extensions),
/// this attribute NO LONGER SKIPS tests automatically.
/// 
/// Tests decorated with this attribute now run in BOTH PostgreSQL and SQLite modes.
/// 
/// For tests that truly require PostgreSQL-specific features (similarity(), pg_trgm, etc.),
/// use [SkipIfNotPostgreSQLFact] instead.
/// 
/// Usage: [SkipIfInMemoryFact] — runs on both PostgreSQL and SQLite.
/// 
/// Default behavior: Tests RUN (PostgreSQL is the default).
/// SQLite fallback: Tests RUN (SQLite supports relational features).
/// </summary>
public sealed class SkipIfInMemoryFactAttribute : FactAttribute
{
    public SkipIfInMemoryFactAttribute()
    {
        // Previously skipped when USE_INMEMORY_DB=true. Now that we use SQLite
        // in-memory (which supports relational features), tests run in all modes.
        // Skip is always null — tests always run.
    }
}

/// <summary>
/// Custom xUnit [Theory] attribute — same behavior as SkipIfInMemoryFact.
/// Now runs in BOTH PostgreSQL and SQLite modes (SQLite supports relational features).
/// </summary>
public sealed class SkipIfInMemoryTheoryAttribute : TheoryAttribute
{
    public SkipIfInMemoryTheoryAttribute()
    {
        // Previously skipped when USE_INMEMORY_DB=true. Now always runs.
    }
}

/// <summary>
/// Custom xUnit [Fact] attribute that skips when NOT using PostgreSQL.
/// Use this for tests that require PostgreSQL-specific features like
/// similarity(), pg_trgm, stored procedures, or other PostgreSQL extensions.
/// 
/// Tests decorated with this attribute SKIP in SQLite mode and RUN in PostgreSQL mode.
/// </summary>
public sealed class SkipIfNotPostgreSQLFactAttribute : FactAttribute
{
    public SkipIfNotPostgreSQLFactAttribute()
    {
        if (!TestEnvironment.UsePostgreSQL)
        {
            Skip = "Requires PostgreSQL-specific features (similarity, pg_trgm, etc.). Currently in SQLite mode.";
        }
    }
}

/// <summary>
/// Custom xUnit [Theory] attribute that skips when NOT using PostgreSQL.
/// Same behavior as SkipIfNotPostgreSQLFact but for parameterized tests.
/// </summary>
public sealed class SkipIfNotPostgreSQLTheoryAttribute : TheoryAttribute
{
    public SkipIfNotPostgreSQLTheoryAttribute()
    {
        if (!TestEnvironment.UsePostgreSQL)
        {
            Skip = "Requires PostgreSQL-specific features (similarity, pg_trgm, etc.). Currently in SQLite mode.";
        }
    }
}
