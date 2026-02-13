using Xunit;

namespace UNOPS.PAO.Business.Tests.TestBase;

/// <summary>
/// Custom xUnit [Fact] attribute that automatically skips when using InMemory database.
/// Tests decorated with this attribute will run by default (PostgreSQL mode)
/// and only skip when USE_INMEMORY_DB=true is set.
/// 
/// Usage: Replace [Fact(Skip = "QA-009: ...")] with [SkipIfInMemoryFact]
/// 
/// Default behavior: Tests RUN (PostgreSQL is the default).
/// InMemory fallback: Tests SKIP (when USE_INMEMORY_DB=true).
/// </summary>
public sealed class SkipIfInMemoryFactAttribute : FactAttribute
{
    public SkipIfInMemoryFactAttribute()
    {
        if (TestEnvironment.UseInMemory)
        {
            Skip = "QA-009: Requires relational database (PostgreSQL). Currently in InMemory mode (USE_INMEMORY_DB=true).";
        }
        // Default: PostgreSQL is active, Skip remains null, test runs normally
    }
}

/// <summary>
/// Custom xUnit [Theory] attribute that automatically skips when using InMemory database.
/// Same behavior as SkipIfInMemoryFact but for parameterized tests.
/// </summary>
public sealed class SkipIfInMemoryTheoryAttribute : TheoryAttribute
{
    public SkipIfInMemoryTheoryAttribute()
    {
        if (TestEnvironment.UseInMemory)
        {
            Skip = "QA-009: Requires relational database (PostgreSQL). Currently in InMemory mode (USE_INMEMORY_DB=true).";
        }
    }
}
