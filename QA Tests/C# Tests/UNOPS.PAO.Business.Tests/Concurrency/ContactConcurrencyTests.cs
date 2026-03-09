using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Concurrency;

/// <summary>
/// Concurrency tests for Contact operations against PostgreSQL.
/// Uses UNOPSContact and creates parent Partners for FK constraints.
/// Uses test markers to filter own data from the shared database.
/// </summary>
public class ContactConcurrencyTests : ConcurrencyTestBase
{
    private readonly string _testMarker = $"CONC_{Guid.NewGuid():N}";

    [SkipIfNotPostgreSQLFact]
    public async Task ConcurrentGetContacts_ShouldReturnConsistent()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contactIds = new List<int>();
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 20)
                .Select(i => new UNOPSContact
                {
                    Name = $"Contact {i} {_testMarker}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}_{_testMarker}@test.com",
                    Title = $"Title{i}",
                    PartnerId = partnerId,
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    LastModifiedDate = DateTime.UtcNow
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
            contactIds.AddRange(contacts.Select(c => c.Id));
        }
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                using var ctx = TestDbContextFactory.Create();
                var ids = string.Join(",", contactIds);
                await ctx.Database.ExecuteSqlAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Act
        var marker = _testMarker;
        var results = await ExecuteConcurrentlyAsync(10, async (index) =>
        {
            using var context = CreateContext();
            return await context.Contacts
                .Where(c => c.Name.Contains(marker))
                .ToListAsync();
        });

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(list => list.Count == 20);
    }

    [SkipIfNotPostgreSQLFact]
    public async Task ConcurrentContactCreation_ShouldCreateAll()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var createdIds = new System.Collections.Concurrent.ConcurrentBag<int>();
        var marker = _testMarker;

        // Act
        await ExecuteConcurrentlyAsync(10, async (index) =>
        {
            using var context = CreateContext();
            var contact = new UNOPSContact
            {
                Name = $"Contact {index} {marker}",
                FirstName = $"First{index}",
                LastName = $"Last{index}",
                Email = $"contact{index}_{marker}@test.com",
                Title = $"Title{index}",
                PartnerId = partnerId,
                Status = EntityStatus.Active,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.UtcNow
            };
            await context.Contacts.AddAsync(contact);
            await context.SaveChangesAsync();
            createdIds.Add(contact.Id);
            return contact;
        });
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                using var ctx = TestDbContextFactory.Create();
                var ids = string.Join(",", createdIds);
                await ctx.Database.ExecuteSqlAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Assert
        using var verifyContext = CreateContext();
        var count = await verifyContext.Contacts.CountAsync(c => c.Name.Contains(_testMarker));
        count.Should().Be(10);
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldCompleteWithinTimeout()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contactIds = new List<int>();
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 50)
                .Select(i => new UNOPSContact
                {
                    Name = $"Contact {i} {_testMarker}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}_{_testMarker}@test.com",
                    Title = $"Title{i}",
                    PartnerId = partnerId,
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    LastModifiedDate = DateTime.UtcNow
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
            contactIds.AddRange(contacts.Select(c => c.Id));
        }
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                using var ctx = TestDbContextFactory.Create();
                var ids = string.Join(",", contactIds);
                await ctx.Database.ExecuteSqlAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Act
        var marker = _testMarker;
        var completed = await ExecuteWithTimeoutAsync(async () =>
        {
            await ExecuteConcurrentlyAsync(20, async (index) =>
            {
                using var context = CreateContext();
                return await context.Contacts
                    .Where(c => c.Name.Contains(marker))
                    .ToListAsync();
            });
        }, timeoutMs: 10000);

        // Assert
        completed.Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentReadAndWrite_ShouldNotDeadlock()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contactIds = new List<int>();
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 10)
                .Select(i => new UNOPSContact
                {
                    Name = $"Contact {i} {_testMarker}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}_{_testMarker}@test.com",
                    Title = $"Title{i}",
                    PartnerId = partnerId,
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    LastModifiedDate = DateTime.UtcNow
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
            contactIds.AddRange(contacts.Select(c => c.Id));
        }
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                using var ctx = TestDbContextFactory.Create();
                var ids = string.Join(",", contactIds);
                await ctx.Database.ExecuteSqlAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Act - Mix of reads and writes
        var marker = _testMarker;
        var completed = await ExecuteWithTimeoutAsync(async () =>
        {
            await ExecuteConcurrentlyAsync(15, async (index) =>
            {
                using var context = CreateContext();
                if (index % 2 == 0)
                {
                    // Read operation
                    return await context.Contacts
                        .Where(c => c.Name.Contains(marker))
                        .ToListAsync();
                }
                else
                {
                    // Write operation - find by our test marker contacts
                    var contactId = contactIds[index % contactIds.Count];
                    var contact = await context.Contacts.FindAsync(contactId);
                    if (contact != null)
                    {
                        contact.FirstName = $"Updated{index}";
                        await context.SaveChangesAsync();
                    }
                    return new List<Contact> { contact! };
                }
            });
        }, timeoutMs: 15000);

        // Assert
        completed.Should().BeTrue();
    }
}
