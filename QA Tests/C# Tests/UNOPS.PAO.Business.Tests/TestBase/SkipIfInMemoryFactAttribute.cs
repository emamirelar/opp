using Xunit;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Custom xUnit [Fact] attribute that skips tests when NOT using a real PostgreSQL database.
/// 
/// Z.EntityFramework.Extensions operations (SingleUpdateAsync, BulkUpdate, etc.) call
/// GetRelationalModel() internally, which fails on SQLite in-memory databases due to
/// model finalization differences. Tests using these operations MUST run against PostgreSQL.
/// 
/// Usage: [SkipIfInMemoryFact] — runs only on PostgreSQL, skips on SQLite/InMemory.
/// 
/// QA-009: Z.EntityFramework.Extensions requires a real relational database (PostgreSQL).
/// SQLite in-memory does NOT fully support bulk operations used by UNOPSOpportunityManager.
/// </summary>
public sealed class SkipIfInMemoryFactAttribute : FactAttribute
{
    public SkipIfInMemoryFactAttribute()
    {
        if (!TestEnvironment.UsePostgreSQL)
        {
            Skip = "QA-009: Z.EntityFramework.Extensions requires PostgreSQL. " +
                   "SingleUpdateAsync/BulkUpdate call GetRelationalModel() which fails on SQLite in-memory. " +
                   "Set TEST_DB_CONNECTION_STRING or configure appsettings.Testing.json to run these tests.";
        }
    }
}

/// <summary>
/// Custom xUnit [Theory] attribute — same behavior as SkipIfInMemoryFact.
/// Skips when not using PostgreSQL (SQLite/InMemory environments).
/// </summary>
public sealed class SkipIfInMemoryTheoryAttribute : TheoryAttribute
{
    public SkipIfInMemoryTheoryAttribute()
    {
        if (!TestEnvironment.UsePostgreSQL)
        {
            Skip = "QA-009: Z.EntityFramework.Extensions requires PostgreSQL. " +
                   "SingleUpdateAsync/BulkUpdate call GetRelationalModel() which fails on SQLite in-memory. " +
                   "Set TEST_DB_CONNECTION_STRING or configure appsettings.Testing.json to run these tests.";
        }
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

/// <summary>
/// Custom xUnit [Fact] attribute that skips when using PostgreSQL (shared database).
/// Use for tests that require an isolated database and make assertions about exact counts
/// or absence of data — with PostgreSQL, tests share the same DB and see each other's data.
/// Run with USE_INMEMORY_DB=true for full suite execution.
/// </summary>
public sealed class SkipIfPostgreSQLFactAttribute : FactAttribute
{
    public SkipIfPostgreSQLFactAttribute()
    {
        if (TestEnvironment.UsePostgreSQL)
        {
            Skip = "Requires isolated database (USE_INMEMORY_DB=true). PostgreSQL uses shared DB; test data from other tests affects assertions.";
        }
    }
}
