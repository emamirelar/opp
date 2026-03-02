/**
 * @fileoverview xUnit collection definition for all integration tests.
 *
 * PURPOSE: This collection ensures ALL integration tests that use PAOWebApplicationFactory
 * share a SINGLE factory instance (via ICollectionFixture), instead of each test class
 * creating its own factory (via IClassFixture).
 *
 * PROBLEM SOLVED: Using IClassFixture caused 75+ factory instances to be created
 * sequentially during the full test suite run. Each factory startup took ~16 seconds
 * (PostgreSQL probe timeout + host build + Identity seeding), consuming thread pool
 * threads and causing:
 *   - PNO926/PNO729/PNO1197 tests to fail due to thread pool starvation
 *   - Total test suite duration of 25+ minutes
 *   - Sporadic failures in timing-sensitive tests
 *
 * SOLUTION: By using ICollectionFixture, all 75+ test classes in the "Integration Tests"
 * collection share ONE factory instance, reducing factory startups from 75+ to 1.
 * This eliminates thread pool contention and prevents PNO test failures.
 *
 * NOTE: The InMemory database is shared across all test classes in this collection.
 * All tests use resilient assertions (e.g., Returns200Or404) that tolerate existing
 * database state, so sharing is safe.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using UNOPS.PAO.IntegrationTests.Infrastructure;
using UNOPS.PAO.Server;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

/// <summary>
/// Defines the "Integration Tests" xUnit collection and binds the shared
/// PAOWebApplicationFactory fixture to it.  All test classes annotated with
/// [Collection("Integration Tests")] will share a single factory instance,
/// eliminating the ~16-second-per-class startup overhead.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<PAOWebApplicationFactory<Program>>
{
    // This class has no code - it is simply the marker that defines the collection.
    // ICollectionFixture<T> tells xUnit to create ONE instance of T for the entire
    // collection and inject it into every test class constructor that requests it.
}
