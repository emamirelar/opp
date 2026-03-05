/**
 * @fileoverview Mock-based tests for AiPromptCacheService.
 * Tests GetCachedResultAsync, GetCachedEntryAsync, SetCachedResultAsync,
 * InvalidateCache, and InvalidateAllForPrompt using real IMemoryCache.
 *
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.UNOPSBusiness.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based tests for AiPromptCacheService.
/// Uses real IMemoryCache for cache behavior verification.
/// </summary>
public class AiPromptCacheServiceTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<AiPromptCacheService>> _mockLogger;
    private readonly AiPromptCacheService _service;

    public AiPromptCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<AiPromptCacheService>>();
        _service = new AiPromptCacheService(_memoryCache, _mockLogger.Object);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    #region Positive (1)

    [Fact]
    public async Task GetCachedResultAsync_AfterSet_ReturnsCachedResult()
    {
        // Arrange
        const string promptType = "Summary";
        const string entityId = "123";
        const string result = "Cached AI result";
        await _service.SetCachedResultAsync(promptType, entityId, "sys", "user", result, 60);

        // Act
        var cached = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        cached.Should().Be(result);
    }

    #endregion

    #region Negative (3+)

    [Fact]
    public async Task GetCachedResultAsync_EmptyKey_ReturnsNull()
    {
        // Arrange
        await _service.SetCachedResultAsync("T", "1", "s", "u", "r", 60);

        // Act
        var cached = await _service.GetCachedResultAsync("", "1");

        // Assert
        cached.Should().BeNull();
    }

    [Fact]
    public async Task GetCachedResultAsync_NonExistentKey_ReturnsNull()
    {
        // Act
        var cached = await _service.GetCachedResultAsync("Nonexistent", "999");

        // Assert
        cached.Should().BeNull();
    }

    [Fact]
    public async Task GetCachedEntryAsync_AfterInvalidate_ReturnsNull()
    {
        // Arrange
        const string promptType = "Summary";
        const string entityId = "456";
        await _service.SetCachedResultAsync(promptType, entityId, "sys", "user", "result", 60);
        await _service.InvalidateCache(promptType, entityId);

        // Act
        var entry = await _service.GetCachedEntryAsync(promptType, entityId);

        // Assert
        entry.Should().BeNull();
    }

    [Fact]
    public async Task GetCachedResultAsync_NullEntityId_ReturnsNull()
    {
        // Act - null entityId produces different cache key
        var cached = await _service.GetCachedResultAsync("T", null!);

        // Assert
        cached.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary (3+)

    [Fact]
    public async Task GetCachedEntryAsync_AfterSet_ReturnsEntryWithCorrectProperties()
    {
        // Arrange
        const string promptType = "Insight";
        const string entityId = "789";
        const string sys = "System instructions";
        const string user = "User prompt";
        const string result = "Gemini result";
        await _service.SetCachedResultAsync(promptType, entityId, sys, user, result, 30);

        // Act
        var entry = await _service.GetCachedEntryAsync(promptType, entityId);

        // Assert
        entry.Should().NotBeNull();
        entry!.PromptType.Should().Be(promptType);
        entry.EntityId.Should().Be(entityId);
        entry.GeminiResult.Should().Be(result);
    }

    [Fact]

    [Trait("Defect", "DEF-074")]
    public async Task SetCachedResultAsync_ZeroMinutes_StillCaches()
    {
        // Arrange
        const string promptType = "Zero";
        const string entityId = "0";

        // Act
        await _service.SetCachedResultAsync(promptType, entityId, "s", "u", "r", 0);
        var cached = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        cached.Should().Be("r");
    }

    [Fact]
    public void InvalidateAllForPrompt_DoesNotThrow()
    {
        // Act
        var act = () => _service.InvalidateAllForPrompt("AnyPrompt");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task InvalidateCache_NonExistentKey_DoesNotThrow()
    {
        // Act
        await _service.InvalidateCache("NonExistent", "999");

        // Assert - no exception
    }

    #endregion

    #region Functional (3+)

    [Fact]
    public async Task GetCachedResultAsync_CacheKeyFormat_IsPromptTypeAndEntityId()
    {
        // Arrange
        await _service.SetCachedResultAsync("TypeA", "Id1", "s", "u", "r1", 60);
        await _service.SetCachedResultAsync("TypeB", "Id1", "s", "u", "r2", 60);

        // Act
        var a = await _service.GetCachedResultAsync("TypeA", "Id1");
        var b = await _service.GetCachedResultAsync("TypeB", "Id1");

        // Assert
        a.Should().Be("r1");
        b.Should().Be("r2");
    }

    [Fact]
    public async Task SetCachedResultAsync_OverwritesExisting()
    {
        // Arrange
        const string promptType = "Overwrite";
        const string entityId = "1";
        await _service.SetCachedResultAsync(promptType, entityId, "s", "u", "old", 60);

        // Act
        await _service.SetCachedResultAsync(promptType, entityId, "s", "u", "new", 60);
        var cached = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        cached.Should().Be("new");
    }

    [Fact]
    public async Task InvalidateCache_RemovesOnlySpecifiedEntry()
    {
        // Arrange
        await _service.SetCachedResultAsync("T1", "1", "s", "u", "r1", 60);
        await _service.SetCachedResultAsync("T1", "2", "s", "u", "r2", 60);
        await _service.InvalidateCache("T1", "1");

        // Act
        var r1 = await _service.GetCachedResultAsync("T1", "1");
        var r2 = await _service.GetCachedResultAsync("T1", "2");

        // Assert
        r1.Should().BeNull();
        r2.Should().Be("r2");
    }

    [Fact]
    public async Task GetCachedEntryAsync_ContainsCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;
        await _service.SetCachedResultAsync("T", "1", "s", "u", "r", 60);
        var after = DateTime.UtcNow;

        // Act
        var entry = await _service.GetCachedEntryAsync("T", "1");

        // Assert
        entry.Should().NotBeNull();
        entry!.CreatedAt.Should().BeOnOrAfter(before.AddSeconds(-1));
        entry.CreatedAt.Should().BeOnOrBefore(after.AddSeconds(1));
    }

    #endregion

    #region Integration (3+)

    [Fact]
    public async Task FullFlow_SetGetInvalidateGet_VerifiesCacheLifecycle()
    {
        // Arrange
        const string promptType = "FullFlow";
        const string entityId = "100";

        // Act - Set
        await _service.SetCachedResultAsync(promptType, entityId, "sys", "user", "result", 60);
        var first = await _service.GetCachedResultAsync(promptType, entityId);

        // Invalidate
        await _service.InvalidateCache(promptType, entityId);
        var afterInvalidate = await _service.GetCachedResultAsync(promptType, entityId);

        // Re-set and get
        await _service.SetCachedResultAsync(promptType, entityId, "sys2", "user2", "result2", 60);
        var second = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        first.Should().Be("result");
        afterInvalidate.Should().BeNull();
        second.Should().Be("result2");
    }

    [Fact]
    public async Task MultipleEntries_SetAndRetrieveIndependently()
    {
        // Arrange
        await _service.SetCachedResultAsync("A", "1", "s", "u", "r1", 60);
        await _service.SetCachedResultAsync("A", "2", "s", "u", "r2", 60);
        await _service.SetCachedResultAsync("B", "1", "s", "u", "r3", 60);

        // Act
        var a1 = await _service.GetCachedResultAsync("A", "1");
        var a2 = await _service.GetCachedResultAsync("A", "2");
        var b1 = await _service.GetCachedResultAsync("B", "1");

        // Assert
        a1.Should().Be("r1");
        a2.Should().Be("r2");
        b1.Should().Be("r3");
    }

    [Fact]
    public async Task GetCachedEntryAsync_AndGetCachedResultAsync_ReturnConsistentData()
    {
        // Arrange
        const string promptType = "Consistent";
        const string entityId = "42";
        const string result = "Consistent result";
        await _service.SetCachedResultAsync(promptType, entityId, "sys", "user", result, 60);

        // Act
        var entry = await _service.GetCachedEntryAsync(promptType, entityId);
        var cachedResult = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        entry!.GeminiResult.Should().Be(cachedResult);
        cachedResult.Should().Be(result);
    }

    [Fact]
    public async Task InvalidateCache_ThenSet_NewEntryStored()
    {
        // Arrange
        const string promptType = "Reuse";
        const string entityId = "1";
        await _service.SetCachedResultAsync(promptType, entityId, "s", "u", "old", 60);
        await _service.InvalidateCache(promptType, entityId);

        // Act
        await _service.SetCachedResultAsync(promptType, entityId, "s", "u", "new", 60);
        var cached = await _service.GetCachedResultAsync(promptType, entityId);

        // Assert
        cached.Should().Be("new");
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 1 | GetCachedResultAsync_AfterSet_ReturnsCachedResult |
| Negative (N) | 4 | GetCachedResultAsync_EmptyKey_ReturnsNull, GetCachedResultAsync_NonExistentKey_ReturnsNull, GetCachedEntryAsync_AfterInvalidate_ReturnsNull, GetCachedResultAsync_NullEntityId_ReturnsNull |
| Edge/Boundary (E) | 4 | GetCachedEntryAsync_AfterSet_ReturnsEntryWithCorrectProperties, SetCachedResultAsync_ZeroMinutes_StillCaches, InvalidateAllForPrompt_DoesNotThrow, InvalidateCache_NonExistentKey_DoesNotThrow |
| Functional (F) | 4 | GetCachedResultAsync_CacheKeyFormat_IsPromptTypeAndEntityId, SetCachedResultAsync_OverwritesExisting, InvalidateCache_RemovesOnlySpecifiedEntry, GetCachedEntryAsync_ContainsCreatedAt |
| Integration (I) | 4 | FullFlow_SetGetInvalidateGet_VerifiesCacheLifecycle, MultipleEntries_SetAndRetrieveIndependently, GetCachedEntryAsync_AndGetCachedResultAsync_ReturnConsistentData, InvalidateCache_ThenSet_NewEntryStored |
| **N ≥ 3P?** | ✅ | 4 >= 3 |
| **E ≥ 3P?** | ✅ | 4 >= 3 |
| **F ≥ 3P?** | ✅ | 4 >= 3 |
| **I ≥ 3P?** | ✅ | 4 >= 3 |
*/
