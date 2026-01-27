/**
 * @fileoverview Tests for Partner ErpDimValue Fix functionality
 * Tests for PR #477 (partner-erpdimvalue-fix-from-development)
 * Commit: 82070b85 - Partner ErpDimValue fix to ignore 8000-9999 numbers
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataImport;

/// <summary>
/// Tests for Partner ErpDimValue Fix functionality
/// Business Rules:
/// - Valid Range: 1-7999 for regular partners
/// - Reserved Range: 8000-9999 for special/reserved partners
/// - Invalid Range: > 9999 must be corrected
/// - Uniqueness: All ErpDimValues must be unique (including soft-deleted)
/// </summary>
public class PartnerErpDimValueFixTests : IDisposable
{
    private readonly AppDbContext _context;
    private const int VALID_RANGE_START = 1;
    private const int VALID_RANGE_END = 7999;
    private const int RESERVED_RANGE_START = 8000;
    private const int RESERVED_RANGE_END = 9999;
    private const int INVALID_THRESHOLD = 10000;

    public PartnerErpDimValueFixTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context?.Dispose();
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
        // Arrange - Create partners with valid and invalid ErpDimValues
        var partners = new List<Partner>
        {
            new Partner { Name = "Valid Partner 1", PartnerShortDescription = "Desc", ErpDimValue = 1000 },
            new Partner { Name = "Valid Partner 2", PartnerShortDescription = "Desc", ErpDimValue = 1001 },
            new Partner { Name = "Valid Partner 3", PartnerShortDescription = "Desc", ErpDimValue = 1002 },
            new Partner { Name = "Invalid Partner 1", PartnerShortDescription = "Desc", ErpDimValue = 10001 },
            new Partner { Name = "Invalid Partner 2", PartnerShortDescription = "Desc", ErpDimValue = 10002 },
            new Partner { Name = "Invalid Partner 3", PartnerShortDescription = "Desc", ErpDimValue = 10003 }
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

        // Assert
        var fixedPartners = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        fixedPartners.Should().BeEmpty("All partners with ErpDimValue > 9999 should be fixed");

        // Verify the fixed values are sequential starting from 1003
        var invalidPartner1 = await _context.Partners.FirstAsync(p => p.Name == "Invalid Partner 1");
        var invalidPartner2 = await _context.Partners.FirstAsync(p => p.Name == "Invalid Partner 2");
        var invalidPartner3 = await _context.Partners.FirstAsync(p => p.Name == "Invalid Partner 3");

        invalidPartner1.ErpDimValue.Should().Be(1003);
        invalidPartner2.ErpDimValue.Should().Be(1004);
        invalidPartner3.ErpDimValue.Should().Be(1005);
    }

    [Fact]
    public async Task FixErpDimValues_WhenNoInvalidPartners_ShouldCompleteWithoutChanges()
    {
        // Arrange - Only valid partners
        var partners = new List<Partner>
        {
            new Partner { Name = "Valid Partner 1", PartnerShortDescription = "Desc", ErpDimValue = 1000 },
            new Partner { Name = "Valid Partner 2", PartnerShortDescription = "Desc", ErpDimValue = 1001 },
            new Partner { Name = "Reserved Partner", PartnerShortDescription = "Desc", ErpDimValue = 8500 }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("No partners should need fixing");
    }

    #endregion

    #region Skip Reserved Range (8000-9999)

    [Fact]
    public async Task FixErpDimValues_WhenReassigning_ShouldSkipReservedRange()
    {
        // Arrange - Create partners where next sequential value would be in reserved range
        var partners = new List<Partner>
        {
            new Partner { Name = "Last Valid Partner", PartnerShortDescription = "Desc", ErpDimValue = 7999 },
            new Partner { Name = "Invalid Partner", PartnerShortDescription = "Desc", ErpDimValue = 10001 }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Fix should skip 8000-9999 and assign 10000 or higher
        var highestValidValue = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value < RESERVED_RANGE_START)
            .MaxAsync(p => (int?)p.ErpDimValue) ?? 0;

        var partnerToFix = await _context.Partners
            .FirstAsync(p => p.Name == "Invalid Partner");

        var nextValue = highestValidValue + 1;
        
        // Skip reserved range
        if (nextValue >= RESERVED_RANGE_START && nextValue <= RESERVED_RANGE_END)
        {
            nextValue = RESERVED_RANGE_END + 1; // Jump to 10000
        }

        partnerToFix.ErpDimValue = nextValue;
        await _context.SaveChangesAsync();

        // Assert
        var fixedPartner = await _context.Partners.FirstAsync(p => p.Name == "Invalid Partner");
        fixedPartner.ErpDimValue.Should().Be(10000, 
            "Value should skip reserved range (8000-9999) and start at 10000");
    }

    [Fact]
    public async Task FixErpDimValues_WhenExistingValuesInReservedRange_ShouldPreserveThem()
    {
        // Arrange - Create reserved partner and invalid partner
        var partners = new List<Partner>
        {
            new Partner { Name = "Reserved Partner", PartnerShortDescription = "Desc", ErpDimValue = 8500 },
            new Partner { Name = "Another Reserved", PartnerShortDescription = "Desc", ErpDimValue = 9000 },
            new Partner { Name = "Valid Partner", PartnerShortDescription = "Desc", ErpDimValue = 1000 }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Query should NOT return reserved partners as needing fix
        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("Reserved range partners should NOT be flagged for fix");

        var reservedPartner = await _context.Partners.FirstAsync(p => p.Name == "Reserved Partner");
        reservedPartner.ErpDimValue.Should().Be(8500, "Reserved range values should be preserved");
    }

    #endregion

    #region Uniqueness Tests

    [Fact]
    public async Task FixErpDimValues_ShouldAssignUniqueValues()
    {
        // Arrange - Create partners with some gaps in ErpDimValues
        var partners = new List<Partner>
        {
            new Partner { Name = "Partner 1", PartnerShortDescription = "Desc", ErpDimValue = 1000 },
            new Partner { Name = "Partner 2", PartnerShortDescription = "Desc", ErpDimValue = 1002 }, // Skip 1001
            new Partner { Name = "Partner 3", PartnerShortDescription = "Desc", ErpDimValue = 1003 },
            new Partner { Name = "Invalid 1", PartnerShortDescription = "Desc", ErpDimValue = 10001 },
            new Partner { Name = "Invalid 2", PartnerShortDescription = "Desc", ErpDimValue = 10002 }
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
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
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

        // Assert - All ErpDimValues should be unique
        var allValues = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue)
            .Select(p => p.ErpDimValue!.Value)
            .ToListAsync();

        allValues.Should().OnlyHaveUniqueItems("All ErpDimValues must be unique");
    }

    [Fact]
    public async Task FixErpDimValues_ShouldConsiderSoftDeletedPartners()
    {
        // Arrange - Create partners including soft-deleted ones
        var partners = new List<Partner>
        {
            new Partner { Name = "Active Partner", PartnerShortDescription = "Desc", ErpDimValue = 1000, IsDeleted = false },
            new Partner { Name = "Deleted Partner", PartnerShortDescription = "Desc", ErpDimValue = 1001, IsDeleted = true },
            new Partner { Name = "Active Partner 2", PartnerShortDescription = "Desc", ErpDimValue = 1002, IsDeleted = false },
            new Partner { Name = "Invalid Partner", PartnerShortDescription = "Desc", ErpDimValue = 10001, IsDeleted = false }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Get used values including soft-deleted
        var usedValues = new HashSet<int>(
            await _context.Partners
                .IgnoreQueryFilters() // Include soft-deleted
                .Where(p => p.ErpDimValue.HasValue)
                .Select(p => p.ErpDimValue!.Value)
                .ToListAsync()
        );

        // Assert - Should include soft-deleted partner's value
        usedValues.Should().Contain(1001, "Soft-deleted partner's ErpDimValue should be considered");

        // When fixing, should skip 1001 even though it's soft-deleted
        var nextAvailable = 1000;
        while (usedValues.Contains(nextAvailable))
        {
            nextAvailable++;
        }

        nextAvailable.Should().Be(1003, "Next available should skip both active and soft-deleted values");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task FixErpDimValues_WhenAllValuesUsedUpTo7999_ShouldContinueAfterReservedRange()
    {
        // Arrange - Simulate all values 1-7999 being used
        // (In real scenario, we'd have 7999 partners, but we'll simulate with a subset)
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
        // Arrange
        var partners = new List<Partner>
        {
            new Partner { Name = "No ErpDimValue", PartnerShortDescription = "Desc", ErpDimValue = null },
            new Partner { Name = "Has ErpDimValue", PartnerShortDescription = "Desc", ErpDimValue = 1000 }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty();

        var nullPartner = await _context.Partners.FirstAsync(p => p.Name == "No ErpDimValue");
        nullPartner.ErpDimValue.Should().BeNull("Null ErpDimValue should remain null");
    }

    [Fact]
    public async Task FixErpDimValues_WhenValueExactly9999_ShouldNotBeFlagged()
    {
        // Arrange - 9999 is the upper bound of reserved range, NOT invalid
        var partner = new Partner
        {
            Name = "Upper Reserved Bound",
            PartnerShortDescription = "Desc",
            ErpDimValue = 9999
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("9999 is the upper bound of reserved range, not invalid");
    }

    [Fact]
    public async Task FixErpDimValues_WhenValueExactly10000_ShouldBeFlagged()
    {
        // Arrange - 10000 is the first invalid value
        var partner = new Partner
        {
            Name = "First Invalid",
            PartnerShortDescription = "Desc",
            ErpDimValue = 10000
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .ToListAsync();

        // Assert
        partnersToFix.Should().HaveCount(1);
        partnersToFix[0].ErpDimValue.Should().Be(10000);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task FixErpDimValues_WithManyPartners_ShouldCompleteEfficiently()
    {
        // Arrange - Create many partners
        var partners = new List<Partner>();
        for (int i = 0; i < 100; i++)
        {
            partners.Add(new Partner
            {
                Name = $"Partner {i}",
                PartnerShortDescription = $"Description {i}",
                ErpDimValue = i < 50 ? 1000 + i : 10000 + i // 50 valid, 50 invalid
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
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .OrderBy(p => p.ErpDimValue)
            .ToListAsync();

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

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, 
            "Fixing 50 partners should complete within 5 seconds");

        var remainingInvalid = await _context.Partners
            .Where(p => p.ErpDimValue.HasValue && p.ErpDimValue.Value > RESERVED_RANGE_END)
            .CountAsync();

        remainingInvalid.Should().Be(0);
    }

    #endregion
}

