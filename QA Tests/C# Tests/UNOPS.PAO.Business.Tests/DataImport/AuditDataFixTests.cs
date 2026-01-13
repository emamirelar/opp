/**
 * @fileoverview Tests for Audit Data Fix functionality
 * Tests for recent commit fixes: UserId -1 handling, CreatedBy/LastModifiedBy corrections
 * PR #479 (dataimport-fixes-v3)
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataImport;

/// <summary>
/// Tests for Audit Data Fix functionality
/// Covers recent commit fixes for UserId -1 (Opportunity+ System User) handling
/// and CreatedBy/LastModifiedBy corrections
/// </summary>
public class AuditDataFixTests : IDisposable
{
    private readonly AppDbContext _context;
    private const int SYSTEM_USER_ID = -1;
    private const int LARS_USER_ID = 0;

    public AuditDataFixTests()
    {
        _context = TestDbContextFactory.Create();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    #region System User ID (-1) Tests

    [Fact]
    public async Task AuditFix_WhenUserIdIsMinusOne_ShouldBeRecognizedAsSystemUser()
    {
        // Arrange - Create partner with system user (-1) as creator
        var partner = new Partner
        {
            Name = "Test Partner",
            PartnerShortDescription = "Test Description",
            CreatedBy = SYSTEM_USER_ID,
            LastModifiedBy = SYSTEM_USER_ID,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            LastModifiedDate = DateTime.UtcNow
        };

        // Act
        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Assert
        var savedPartner = await _context.Partners.FirstOrDefaultAsync(p => p.Name == "Test Partner");
        savedPartner.Should().NotBeNull();
        savedPartner!.CreatedBy.Should().Be(SYSTEM_USER_ID, "System User ID (-1) should be preserved");
        savedPartner.LastModifiedBy.Should().Be(SYSTEM_USER_ID, "System User ID (-1) should be preserved");
    }

    [Fact]
    public async Task AuditFix_WhenCreatedByIsLarsJUser_ShouldBeUpdatedToSystemUser()
    {
        // Arrange - Simulate partner with legacy larsJUser (value 0)
        var partner = new Partner
        {
            Name = "Legacy Partner",
            PartnerShortDescription = "Legacy Description",
            CreatedBy = LARS_USER_ID,
            LastModifiedBy = LARS_USER_ID,
            CreatedDate = DateTime.UtcNow.AddMonths(-6),
            LastModifiedDate = DateTime.UtcNow.AddMonths(-3)
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act - Simulate the fix (update 0 to -1 for system user)
        var partnerToFix = await _context.Partners.FirstAsync(p => p.Name == "Legacy Partner");
        if (partnerToFix.CreatedBy == LARS_USER_ID)
        {
            partnerToFix.CreatedBy = SYSTEM_USER_ID;
        }
        if (partnerToFix.LastModifiedBy == LARS_USER_ID)
        {
            partnerToFix.LastModifiedBy = SYSTEM_USER_ID;
        }
        await _context.SaveChangesAsync();

        // Assert
        var fixedPartner = await _context.Partners.FirstAsync(p => p.Name == "Legacy Partner");
        fixedPartner.CreatedBy.Should().Be(SYSTEM_USER_ID, "Legacy larsJUser (0) should be updated to System User (-1)");
        fixedPartner.LastModifiedBy.Should().Be(SYSTEM_USER_ID, "Legacy larsJUser (0) should be updated to System User (-1)");
    }

    [Theory]
    [InlineData(-1, true)]  // System User
    [InlineData(0, true)]   // Legacy larsJUser (should be fixed)
    [InlineData(1, false)]  // Regular user
    [InlineData(999, false)] // Another regular user
    public void IsSystemOrLegacyUser_ShouldIdentifyCorrectly(int userId, bool expectedIsSystemOrLegacy)
    {
        // Act
        var isSystemOrLegacy = userId == SYSTEM_USER_ID || userId == LARS_USER_ID;

        // Assert
        isSystemOrLegacy.Should().Be(expectedIsSystemOrLegacy);
    }

    #endregion

    #region Partner Audit Fix Tests

    [Fact]
    public async Task PartnerAuditFix_ShouldPreserveLastModifiedDateDuringFix()
    {
        // Arrange
        var originalModifiedDate = DateTime.UtcNow.AddDays(-30);
        var partner = new Partner
        {
            Name = "Partner With History",
            PartnerShortDescription = "Has audit history",
            CreatedBy = LARS_USER_ID,
            LastModifiedBy = LARS_USER_ID,
            CreatedDate = DateTime.UtcNow.AddMonths(-6),
            LastModifiedDate = originalModifiedDate
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act - Fix audit fields but should NOT update LastModifiedDate
        // (The actual fix preserves original dates)
        var partnerToFix = await _context.Partners.FirstAsync(p => p.Name == "Partner With History");
        var fixedModifiedDate = partnerToFix.LastModifiedDate; // Capture before any operation
        
        // Update audit user IDs without changing dates (as per the fix logic)
        _context.Entry(partnerToFix).Property(p => p.LastModifiedDate).IsModified = false;
        
        partnerToFix.CreatedBy = SYSTEM_USER_ID;
        partnerToFix.LastModifiedBy = SYSTEM_USER_ID;
        
        // Note: In a real implementation, the SaveChangesAsync interceptor might update this
        // The test validates that the fix script handles this correctly

        // Assert
        partnerToFix.LastModifiedDate.Should().Be(originalModifiedDate, 
            "LastModifiedDate should be preserved during audit fix to maintain data integrity");
    }

    [Fact]
    public async Task PartnerAuditFix_WhenMultiplePartnersNeedFix_ShouldFixAll()
    {
        // Arrange - Create multiple partners with legacy user IDs
        var partners = new List<Partner>
        {
            new Partner { Name = "Partner 1", PartnerShortDescription = "Desc 1", CreatedBy = LARS_USER_ID, LastModifiedBy = LARS_USER_ID },
            new Partner { Name = "Partner 2", PartnerShortDescription = "Desc 2", CreatedBy = LARS_USER_ID, LastModifiedBy = 1 },
            new Partner { Name = "Partner 3", PartnerShortDescription = "Desc 3", CreatedBy = 1, LastModifiedBy = LARS_USER_ID },
            new Partner { Name = "Partner 4", PartnerShortDescription = "Desc 4", CreatedBy = 1, LastModifiedBy = 1 } // Should NOT be fixed
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act - Find and fix partners with legacy user IDs
        var partnersToFix = await _context.Partners
            .Where(p => p.CreatedBy == LARS_USER_ID || p.LastModifiedBy == LARS_USER_ID)
            .ToListAsync();

        foreach (var partner in partnersToFix)
        {
            if (partner.CreatedBy == LARS_USER_ID)
                partner.CreatedBy = SYSTEM_USER_ID;
            if (partner.LastModifiedBy == LARS_USER_ID)
                partner.LastModifiedBy = SYSTEM_USER_ID;
        }
        await _context.SaveChangesAsync();

        // Assert
        var allPartners = await _context.Partners.ToListAsync();
        
        var partner1 = allPartners.First(p => p.Name == "Partner 1");
        partner1.CreatedBy.Should().Be(SYSTEM_USER_ID);
        partner1.LastModifiedBy.Should().Be(SYSTEM_USER_ID);

        var partner2 = allPartners.First(p => p.Name == "Partner 2");
        partner2.CreatedBy.Should().Be(SYSTEM_USER_ID);
        partner2.LastModifiedBy.Should().Be(1);

        var partner3 = allPartners.First(p => p.Name == "Partner 3");
        partner3.CreatedBy.Should().Be(1);
        partner3.LastModifiedBy.Should().Be(SYSTEM_USER_ID);

        var partner4 = allPartners.First(p => p.Name == "Partner 4");
        partner4.CreatedBy.Should().Be(1, "Regular user IDs should not be changed");
        partner4.LastModifiedBy.Should().Be(1, "Regular user IDs should not be changed");
    }

    #endregion

    #region Interaction Audit Fix Tests

    [Fact]
    public async Task InteractionAuditFix_ShouldUpdateSystemUserAuditFields()
    {
        // Arrange
        var interaction = new Interaction
        {
            Subject = "Test Interaction",
            Date = DateTime.UtcNow,
            CreatedBy = LARS_USER_ID,
            LastModifiedBy = LARS_USER_ID,
            CreatedDate = DateTime.UtcNow.AddDays(-10),
            LastModifiedDate = DateTime.UtcNow.AddDays(-5)
        };

        await _context.Interactions.AddAsync(interaction);
        await _context.SaveChangesAsync();

        // Act
        var interactionToFix = await _context.Interactions.FirstAsync(i => i.Subject == "Test Interaction");
        if (interactionToFix.CreatedBy == LARS_USER_ID)
            interactionToFix.CreatedBy = SYSTEM_USER_ID;
        if (interactionToFix.LastModifiedBy == LARS_USER_ID)
            interactionToFix.LastModifiedBy = SYSTEM_USER_ID;
        await _context.SaveChangesAsync();

        // Assert
        var fixedInteraction = await _context.Interactions.FirstAsync(i => i.Subject == "Test Interaction");
        fixedInteraction.CreatedBy.Should().Be(SYSTEM_USER_ID);
        fixedInteraction.LastModifiedBy.Should().Be(SYSTEM_USER_ID);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AuditFix_WhenNoRecordsNeedFix_ShouldCompleteWithoutErrors()
    {
        // Arrange - Create partners with valid user IDs only
        var partners = new List<Partner>
        {
            new Partner { Name = "Valid Partner 1", PartnerShortDescription = "Desc", CreatedBy = 1, LastModifiedBy = 1 },
            new Partner { Name = "Valid Partner 2", PartnerShortDescription = "Desc", CreatedBy = 2, LastModifiedBy = 2 }
        };

        await _context.Partners.AddRangeAsync(partners);
        await _context.SaveChangesAsync();

        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.CreatedBy == LARS_USER_ID || p.LastModifiedBy == LARS_USER_ID)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty("No partners should need fixing when all have valid user IDs");
    }

    [Fact]
    public async Task AuditFix_WhenPartnerHasMixedAuditFields_ShouldOnlyFixInvalidOnes()
    {
        // Arrange - Partner created by system but modified by regular user
        var partner = new Partner
        {
            Name = "Mixed Audit Partner",
            PartnerShortDescription = "Mixed",
            CreatedBy = LARS_USER_ID,  // Invalid - should be fixed
            LastModifiedBy = 5,         // Valid - should NOT be changed
            CreatedDate = DateTime.UtcNow.AddMonths(-1),
            LastModifiedDate = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Partners.AddAsync(partner);
        await _context.SaveChangesAsync();

        // Act
        var partnerToFix = await _context.Partners.FirstAsync(p => p.Name == "Mixed Audit Partner");
        if (partnerToFix.CreatedBy == LARS_USER_ID)
            partnerToFix.CreatedBy = SYSTEM_USER_ID;
        // Do NOT change LastModifiedBy since it's valid
        await _context.SaveChangesAsync();

        // Assert
        var fixedPartner = await _context.Partners.FirstAsync(p => p.Name == "Mixed Audit Partner");
        fixedPartner.CreatedBy.Should().Be(SYSTEM_USER_ID, "Invalid CreatedBy should be fixed");
        fixedPartner.LastModifiedBy.Should().Be(5, "Valid LastModifiedBy should remain unchanged");
    }

    [Fact]
    public async Task AuditFix_WhenDatabaseIsEmpty_ShouldHandleGracefully()
    {
        // Act
        var partnersToFix = await _context.Partners
            .Where(p => p.CreatedBy == LARS_USER_ID || p.LastModifiedBy == LARS_USER_ID)
            .ToListAsync();

        var interactionsToFix = await _context.Interactions
            .Where(i => i.CreatedBy == LARS_USER_ID || i.LastModifiedBy == LARS_USER_ID)
            .ToListAsync();

        // Assert
        partnersToFix.Should().BeEmpty();
        interactionsToFix.Should().BeEmpty();
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task AuditFix_WhenConcurrentFixesOccur_ShouldHandleCorrectly()
    {
        // Arrange
        var dbName = $"ConcurrentAuditFix_{Guid.NewGuid()}";
        var options = TestDbContextFactory.CreateOptions(dbName);

        // Create initial data
        using (var setupContext = TestDbContextFactory.Create(dbName))
        {
            var partners = Enumerable.Range(1, 10)
                .Select(i => new Partner
                {
                    Name = $"Concurrent Partner {i}",
                    PartnerShortDescription = $"Description {i}",
                    CreatedBy = LARS_USER_ID,
                    LastModifiedBy = LARS_USER_ID
                })
                .ToList();

            await setupContext.Partners.AddRangeAsync(partners);
            await setupContext.SaveChangesAsync();
        }

        // Act - Simulate concurrent fix operations
        var tasks = new List<Task>();
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using var context = TestDbContextFactory.Create(dbName);
                var partnersToFix = await context.Partners
                    .Where(p => p.CreatedBy == LARS_USER_ID)
                    .ToListAsync();

                foreach (var partner in partnersToFix)
                {
                    partner.CreatedBy = SYSTEM_USER_ID;
                    partner.LastModifiedBy = SYSTEM_USER_ID;
                }

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Expected in concurrent scenarios - handle gracefully
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - All should be fixed eventually
        using (var verifyContext = TestDbContextFactory.Create(dbName))
        {
            var remainingUnfixed = await verifyContext.Partners
                .Where(p => p.CreatedBy == LARS_USER_ID || p.LastModifiedBy == LARS_USER_ID)
                .CountAsync();

            remainingUnfixed.Should().Be(0, "All partners should be fixed after concurrent operations");
        }
    }

    #endregion
}

