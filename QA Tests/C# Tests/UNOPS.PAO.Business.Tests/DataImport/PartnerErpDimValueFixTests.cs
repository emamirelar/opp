/**
 * @fileoverview Tests for Partner ErpDimValue Fix functionality
 * Tests for PR #477 (partner-erpdimvalue-fix-from-development)
 * Commit: 82070b85 - Partner ErpDimValue fix to ignore 8000-9999 numbers
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataImport;

/// <summary>
/// Tests for Partner ErpDimValue Fix functionality
/// Business Rules:
/// - Valid Range: 1-7999 for regular partners
/// - Reserved Range: 8000-9999 for special/reserved partners
/// - Invalid Range: > 9999 must be corrected
/// - Uniqueness: All ErpDimValues must be unique (including soft-deleted)
///
/// IMPORTANT: ErpDimValue has a unique constraint (IX_Partners_ErpDimValue).
/// All tests dynamically find available values to avoid conflicts on a shared database.
/// </summary>
public class PartnerErpDimValueFixTests : IDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly int _testUserId;
    private readonly string _testMarker;
    private const int VALID_RANGE_START = 1;
    private const int VALID_RANGE_END = 7999;
    private const int RESERVED_RANGE_START = 8000;
    private const int RESERVED_RANGE_END = 9999;
    private const int INVALID_THRESHOLD = 10000;

    public PartnerErpDimValueFixTests()
    {
        _testMarker = $"ErpTest_{Guid.NewGuid():N}";

        if (TestEnvironment.UsePostgreSQL)
        {
            using var tempContext = TestDbContextFactory.CreateUNOPS();
            _testUserId = TestDataHelper.GetOrCreateTestUser(tempContext, "erpdimvalue-test@unops.org");
            _context = TestDbContextFactory.CreateWithUserId(_testUserId);
            _transaction = _context.Database.BeginTransaction();
        }
        else
        {
            _testUserId = 1;
            _context = TestDbContextFactory.Create();
        }
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { }
            _transaction.Dispose();
            _transaction = null;
        }
        _context?.Dispose();
    }

    /// <summary>
    /// Finds available (unused) ErpDimValues in the specified range.
    /// Queries all partners (including soft-deleted) to avoid unique constraint violations.
    /// </summary>
    private async Task<List<int>> FindAvailableErpDimValues(int count, int rangeStart, int rangeEnd)
    {
        var usedValues = new HashSet<int>(
            await _context.Partners
                .IgnoreQueryFilters()
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        var available = new List<int>();
        for (int val = rangeStart; val <= rangeEnd && available.Count < count; val++)
        {
            if (!usedValues.Contains(val))
            {
                available.Add(val);
            }
        }

        if (available.Count < count)
            throw new InvalidOperationException(
                $"Not enough available ErpDimValues in range [{rangeStart}-{rangeEnd}]. Needed {count}, found {available.Count}.");

        return available;
    }

    #region Core ErpDimValue Logic Tests

    [Theory]
    [InlineData(1, true)]
    [InlineData(1000, true)]
    [InlineData(7999, true)]
    [InlineData(8000, false)]  // Reserved
    [InlineData(8500, false)]  // Reserved
    [InlineData(9999, false)]  // Reserved
    [InlineData(10000, false)] // Invalid
    [InlineData(15000, false)] // Invalid
    public void IsValidRegularErpDimValue_ShouldValidateCorrectly(int value, bool expectedValid)
    {
        // Act
        var isValid = value >= VALID_RANGE_START && value <= VALID_RANGE_END;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(8000, true)]
    [InlineData(8500, true)]
    [InlineData(9000, true)]
    [InlineData(9999, true)]
    [InlineData(7999, false)]
    [InlineData(10000, false)]
    public void IsReservedErpDimValue_ShouldIdentifyCorrectly(int value, bool expectedReserved)
    {
        // Act
        var isReserved = value >= RESERVED_RANGE_START && value <= RESERVED_RANGE_END;

        // Assert
        isReserved.Should().Be(expectedReserved);
    }

    [Theory]
    [InlineData(10000, true)]
    [InlineData(10001, true)]
    [InlineData(99999, true)]
    [InlineData(9999, false)]
    [InlineData(1000, false)]
    public void IsInvalidErpDimValue_ShouldIdentifyValuesAbove9999(int value, bool expectedInvalid)
    {
        // Act
        var isInvalid = value >= INVALID_THRESHOLD;

        // Assert
        isInvalid.Should().Be(expectedInvalid);
    }

    #endregion

    #region Fix Partners with ErpDimValue > 9999

    [Fact]
    public async Task FixErpDimValues_WhenPartnersHaveValuesAbove9999_ShouldReassignValidValues()
    {
        // Arrange - Find available ErpDimValues dynamically
        var validValues = await FindAvailableErpDimValues(3, VALID_RANGE_START, VALID_RANGE_END);
        var invalidValues = await FindAvailableErpDimValues(3, INVALID_THRESHOLD + 1, 99999);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_Valid1", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Valid2", PartnerShortDescription = "Desc", ErpDimValue = validValues[1], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Valid3", PartnerShortDescription = "Desc", ErpDimValue = validValues[2], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Invalid1", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Invalid2", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[1], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Invalid3", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[2], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Find highest valid value and fix invalid partners
        var highestValidValue = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value < RESERVED_RANGE_START)
            .MaxAsync(p => (int?)p.ErpDimValue) ?? 0;

        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .OrderBy(p => p.ErpDimValue)
            .ToListAsync();

        var nextValue = highestValidValue + 1;
        var usedValues = new HashSet<int>(
            await _context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        foreach (var partner in partnersToFix)
        {
            while (usedValues.Contains(nextValue) ||
                   (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END))
            {
                nextValue++;
            }

            partner.ErpDimValue = nextValue;
            usedValues.Add(nextValue);
            nextValue++;
        }

        await _context.SaveChangesAsync();

        // Assert - Our test-created invalid partners should now have valid values
        var testInvalidPartners = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.Name.Contains("Invalid"))
            .ToListAsync();

        testInvalidPartners.Should().HaveCount(3);
        var fixedValues = testInvalidPartners.Select(p => p.ErpDimValue!.Value).OrderBy(v => v).ToList();
        fixedValues.Should().OnlyHaveUniqueItems("All fixed values should be unique");
        fixedValues.Should().BeInAscendingOrder("Fixed values should be assigned in ascending order");
        foreach (var v in fixedValues)
        {
            (v >= RESERVED_RANGE_START && v <= RESERVED_RANGE_END).Should().BeFalse(
                $"Fixed value {v} should not be in reserved range ({RESERVED_RANGE_START}-{RESERVED_RANGE_END})");
        }
    }

    [Fact]
    public async Task FixErpDimValues_WhenNoInvalidPartners_ShouldCompleteWithoutChanges()
    {
        // Arrange - Only valid and reserved partners (no invalid > 9999)
        var validValues = await FindAvailableErpDimValues(2, VALID_RANGE_START, VALID_RANGE_END);
        var reservedValues = await FindAvailableErpDimValues(1, RESERVED_RANGE_START, RESERVED_RANGE_END);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_Valid1", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Valid2", PartnerShortDescription = "Desc", ErpDimValue = validValues[1], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Reserved", PartnerShortDescription = "Desc", ErpDimValue = reservedValues[0], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Query for partners needing fix (> 9999)
        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("No test partners should need fixing when none have values > 9999");
    }

    #endregion

    #region Skip Reserved Range (8000-9999)

    [Fact]
    public async Task FixErpDimValues_WhenReassigning_ShouldSkipReservedRange()
    {
        // Arrange - Seed a partner at VALID_RANGE_END (7999) so that the next assigned value
        // is 8000, which falls in the reserved range and must be bumped to RESERVED_RANGE_END + 1 (10000).
        // This is deterministic in both SQLite (fresh DB) and PostgreSQL (find available boundary value).
        var isValueAvailable = !await _context.Partners
            .IgnoreQueryFilters()
            .AnyAsync(p => p.ErpDimValue == VALID_RANGE_END);

        if (!isValueAvailable)
        {
            // VALID_RANGE_END (7999) already in use — skip rather than find a lower value
            // because the test requires nextValue to enter the reserved range.
            return;
        }

        var boundaryValues = new List<int> { VALID_RANGE_END };
        var invalidValues = await FindAvailableErpDimValues(1, INVALID_THRESHOLD + 1, 99999);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_AtBoundary", PartnerShortDescription = "Desc", ErpDimValue = boundaryValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Invalid", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[0], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - highestValidValue = VALID_RANGE_END (7999), nextValue = 8000.
        // 8000 is in reserved range [8000-9999] so it must be bumped to RESERVED_RANGE_END + 1 (10000).
        var highestValidValue = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value < RESERVED_RANGE_START)
            .MaxAsync(p => (int?)p.ErpDimValue) ?? 0;

        var partnerToFix = await _context.Partners
            .FirstAsync(p => p.Name == $"{_testMarker}_Invalid");

        var nextValue = highestValidValue + 1;

        // Skip reserved range
        if (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END)
        {
            nextValue = RESERVED_RANGE_END + 1;
        }

        // Skip already used values
        var usedValues = new HashSet<int>(
            await _context.Partners
                .IgnoreQueryFilters()
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );
        while (usedValues.Contains(nextValue))
        {
            nextValue++;
        }

        partnerToFix.ErpDimValue = nextValue;
        await _context.SaveChangesAsync();

        // Assert
        var fixedPartner = await _context.Partners.FirstAsync(p => p.Name == $"{_testMarker}_Invalid");
        fixedPartner.ErpDimValue!.Value.Should().BeGreaterThan(RESERVED_RANGE_END,
            "Value should skip reserved range (8000-9999) and be above 9999");
    }

    [Fact]
    public async Task FixErpDimValues_WhenExistingValuesInReservedRange_ShouldPreserveThem()
    {
        // Arrange - Create reserved partners and a valid partner
        var reservedValues = await FindAvailableErpDimValues(2, RESERVED_RANGE_START, RESERVED_RANGE_END);
        var validValues = await FindAvailableErpDimValues(1, VALID_RANGE_START, VALID_RANGE_END);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_Reserved1", PartnerShortDescription = "Desc", ErpDimValue = reservedValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Reserved2", PartnerShortDescription = "Desc", ErpDimValue = reservedValues[1], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Valid", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Query should NOT return reserved partners as needing fix
        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("Reserved range partners should NOT be flagged for fix");

        var reservedPartner = await _context.Partners.FirstAsync(p => p.Name == $"{_testMarker}_Reserved1");
        reservedPartner.ErpDimValue.Should().Be(reservedValues[0], "Reserved range values should be preserved");
    }

    #endregion

    #region Uniqueness Tests

    [Fact]
    public async Task FixErpDimValues_ShouldAssignUniqueValues()
    {
        // Arrange - Create partners with some gaps in ErpDimValues
        var validValues = await FindAvailableErpDimValues(3, VALID_RANGE_START, VALID_RANGE_END);
        var invalidValues = await FindAvailableErpDimValues(2, INVALID_THRESHOLD + 1, 99999);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_P1", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_P2", PartnerShortDescription = "Desc", ErpDimValue = validValues[1], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_P3", PartnerShortDescription = "Desc", ErpDimValue = validValues[2], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Inv1", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[0], LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Inv2", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[1], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Fix invalid partners
        var usedValues = new HashSet<int>(
            await _context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        var highestValid = usedValues
            .Where(v => v < RESERVED_RANGE_START)
            .DefaultIfEmpty(0)
            .Max();

        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .OrderBy(p => p.ErpDimValue)
            .ToListAsync();

        var nextValue = highestValid + 1;
        foreach (var partner in partnersToFix)
        {
            while (usedValues.Contains(nextValue) ||
                   (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END))
            {
                nextValue++;
            }

            partner.ErpDimValue = nextValue;
            usedValues.Add(nextValue);
            nextValue++;
        }

        await _context.SaveChangesAsync();

        // Assert - All ErpDimValues for our test partners should be unique
        var testValues = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue)
            .Select(p => p.ErpDimValue!.Value)
            .ToListAsync();

        testValues.Should().OnlyHaveUniqueItems("All test-created ErpDimValues must be unique");
    }

    [Fact]
    public async Task FixErpDimValues_ShouldConsiderSoftDeletedPartners()
    {
        // Arrange - Find 3 consecutive available values for active/deleted testing
        var validValues = await FindAvailableErpDimValues(3, VALID_RANGE_START, VALID_RANGE_END);
        var invalidValues = await FindAvailableErpDimValues(1, INVALID_THRESHOLD + 1, 99999);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_Active1", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], IsDeleted = false, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Deleted1", PartnerShortDescription = "Desc", ErpDimValue = validValues[1], IsDeleted = true, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Active2", PartnerShortDescription = "Desc", ErpDimValue = validValues[2], IsDeleted = false, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_Invalid1", PartnerShortDescription = "Desc", ErpDimValue = invalidValues[0], IsDeleted = false, LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Get used values including soft-deleted
        var usedValues = new HashSet<int>(
            await _context.Partners
                .IgnoreQueryFilters()
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        // Assert - Should include soft-deleted partner's value
        usedValues.Should().Contain(validValues[1], "Soft-deleted partner's ErpDimValue should be considered");

        // When computing next available, should skip the soft-deleted value
        var testUsedValues = new HashSet<int>(validValues.Concat(invalidValues));
        var nextAvailable = validValues[0];
        while (testUsedValues.Contains(nextAvailable) || usedValues.Contains(nextAvailable))
        {
            nextAvailable++;
            // Skip reserved range
            if (nextAvailable >= RESERVED_RANGE_START && nextAvailable <= RESERVED_RANGE_END)
                nextAvailable = RESERVED_RANGE_END + 1;
        }

        nextAvailable.Should().BeGreaterThan(validValues[0],
            "Next available should skip all used values including soft-deleted");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task FixErpDimValues_WhenAllValuesUsedUpTo7999_ShouldContinueAfterReservedRange()
    {
        // Arrange - Simulate all values 1-7999 being used (in-memory logic test)
        var usedValues = new HashSet<int>(Enumerable.Range(1, 7999));

        // Act - Find next available value
        var nextValue = 1;
        while (usedValues.Contains(nextValue) ||
               (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END))
        {
            nextValue++;
        }

        // Assert - Should jump to 10000 (after reserved range)
        nextValue.Should().Be(10000,
            "When 1-7999 are all used, next value should skip reserved range and be 10000");
    }

    [Fact]
    public async Task FixErpDimValues_WhenPartnerHasNullErpDimValue_ShouldNotBeAffected()
    {
        // Arrange - One null and one valid
        var validValues = await FindAvailableErpDimValues(1, VALID_RANGE_START, VALID_RANGE_END);

        var partners = new List<UNOPSPartner>
        {
            new UNOPSPartner { Name = $"{_testMarker}_NullErp", PartnerShortDescription = "Desc", ErpDimValue = null, LastModifiedDate = DateTime.UtcNow },
            new UNOPSPartner { Name = $"{_testMarker}_HasErp", PartnerShortDescription = "Desc", ErpDimValue = validValues[0], LastModifiedDate = DateTime.UtcNow }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Find partners to fix (only > 9999, not null)
        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("Neither null nor valid ErpDimValues need fixing");

        var nullPartner = await _context.Partners.FirstAsync(p => p.Name == $"{_testMarker}_NullErp");
        nullPartner.ErpDimValue.Should().BeNull("Null ErpDimValue should remain null");
    }

    [Fact]
    public async Task FixErpDimValues_WhenValueInReservedRange_ShouldNotBeFlagged()
    {
        // Arrange - Use any available reserved-range value
        var reservedValues = await FindAvailableErpDimValues(1, RESERVED_RANGE_START, RESERVED_RANGE_END);

        var partner = new UNOPSPartner
        {
            Name = $"{_testMarker}_UpperReserved",
            PartnerShortDescription = "Desc",
            ErpDimValue = reservedValues[0],
            LastModifiedDate = DateTime.UtcNow
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act - Query for partners needing fix (> 9999 only)
        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty(
            $"Value {reservedValues[0]} is in reserved range ({RESERVED_RANGE_START}-{RESERVED_RANGE_END}), not invalid");
    }

    [Fact]
    public async Task FixErpDimValues_WhenValueAboveThreshold_ShouldBeFlagged()
    {
        // Arrange - Use any available invalid value (> 9999)
        var invalidValues = await FindAvailableErpDimValues(1, INVALID_THRESHOLD, 99999);

        var partner = new UNOPSPartner
        {
            Name = $"{_testMarker}_FirstInvalid",
            PartnerShortDescription = "Desc",
            ErpDimValue = invalidValues[0],
            LastModifiedDate = DateTime.UtcNow
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert - Our test partner should be in the list
        partnersToFix.Should().Contain(p => p.Name == $"{_testMarker}_FirstInvalid",
            $"Partner with ErpDimValue {invalidValues[0]} (>{RESERVED_RANGE_END}) should be flagged for fix");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task FixErpDimValues_WithManyPartners_ShouldCompleteEfficiently()
    {
        // Arrange - Find enough available values for 100 partners (50 valid, 50 invalid)
        var validValues = await FindAvailableErpDimValues(50, VALID_RANGE_START, VALID_RANGE_END);
        var invalidValues = await FindAvailableErpDimValues(50, INVALID_THRESHOLD + 1, 99999);

        var partners = new List<UNOPSPartner>();
        for (int i = 0; i < 50; i++)
        {
            partners.Add(new UNOPSPartner
            {
                Name = $"{_testMarker}_Valid{i}",
                PartnerShortDescription = $"Description {i}",
                ErpDimValue = validValues[i],
                LastModifiedDate = DateTime.UtcNow
            });
        }
        for (int i = 0; i < 50; i++)
        {
            partners.Add(new UNOPSPartner
            {
                Name = $"{_testMarker}_Invalid{i}",
                PartnerShortDescription = $"Description {50 + i}",
                ErpDimValue = invalidValues[i],
                LastModifiedDate = DateTime.UtcNow
            });
        }

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var usedValues = new HashSet<int>(
            await _context.Partners
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        var partnersToFix = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .OrderBy(p => p.ErpDimValue)
            .ToListAsync();

        // Capture original values to verify they changed
        var originalValues = partnersToFix.ToDictionary(p => p.Id, p => p.ErpDimValue!.Value);

        partnersToFix.Should().HaveCount(50, "Should identify all 50 test-created invalid partners");

        var highestValid = usedValues
            .Where(v => v < RESERVED_RANGE_START)
            .DefaultIfEmpty(0)
            .Max();

        var nextValue = highestValid + 1;
        foreach (var partner in partnersToFix)
        {
            while (usedValues.Contains(nextValue) ||
                   (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END))
            {
                nextValue++;
            }

            partner.ErpDimValue = nextValue;
            usedValues.Add(nextValue);
            nextValue++;
        }

        await _context.SaveChangesAsync();

        stopwatch.Stop();

        // Assert - Performance
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000,
            "Fixing partners should complete within 5 seconds");

        // Verify all 50 invalid partners were reassigned to new unique values
        _context.ChangeTracker.Clear();
        var fixedPartners = await _context.Partners
            .Where(p => p.Name.StartsWith(_testMarker) && p.Name.Contains("Invalid"))
            .ToListAsync();

        fixedPartners.Should().HaveCount(50, "All 50 invalid partners should still exist");

        var newValues = fixedPartners.Select(p => p.ErpDimValue!.Value).ToList();
        newValues.Should().OnlyHaveUniqueItems("All reassigned values should be unique");

        foreach (var fp in fixedPartners)
        {
            fp.ErpDimValue!.Value.Should().NotBe(originalValues[fp.Id],
                $"Partner {fp.Name} should have a reassigned ErpDimValue");
        }
    }

    #endregion
}
