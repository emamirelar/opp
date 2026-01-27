using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Concurrency;

/// <summary>
/// Concurrency tests for Contact operations
/// </summary>
public class ContactConcurrencyTests : ConcurrencyTestBase
{
    [Fact]
    public async Task ConcurrentGetContacts_ShouldReturnConsistent()
    {
        // Arrange
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 20)
                .Select(i => new Contact
                {
                    Id = i,
                    Name = $"Contact {i}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}@test.com",
                    Title = $"Title{i}",
                    Status = EntityStatus.Active
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
        }

        // Act
        var results = await ExecuteConcurrentlyAsync(10, async (index) =>
        {
            using var context = CreateContext();
            return await context.Contacts.ToListAsync();
        });

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(list => list.Count == 20);
    }

    [Fact]
    public async Task ConcurrentContactCreation_ShouldCreateAll()
    {
        // Act
        await ExecuteConcurrentlyAsync(10, async (index) =>
        {
            using var context = CreateContext();
            var contact = new Contact
            {
                Id = index + 1,
                Name = $"Contact {index}",
                FirstName = $"First{index}",
                LastName = $"Last{index}",
                Email = $"contact{index}@test.com",
                Title = $"Title{index}",
                Status = EntityStatus.Active
            };
            await context.Contacts.AddAsync(contact);
            await context.SaveChangesAsync();
            return contact;
        });

        // Assert
        using var verifyContext = CreateContext();
        var count = await verifyContext.Contacts.CountAsync();
        count.Should().Be(10);
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldCompleteWithinTimeout()
    {
        // Arrange
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 50)
                .Select(i => new Contact
                {
                    Id = i,
                    Name = $"Contact {i}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}@test.com",
                    Title = $"Title{i}",
                    Status = EntityStatus.Active
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
        }

        // Act
        var completed = await ExecuteWithTimeoutAsync(async () =>
        {
            await ExecuteConcurrentlyAsync(20, async (index) =>
            {
                using var context = CreateContext();
                return await context.Contacts.ToListAsync();
            });
        }, timeoutMs: 10000);

        // Assert
        completed.Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentReadAndWrite_ShouldNotDeadlock()
    {
        // Arrange
        using (var context = CreateContext())
        {
            var contacts = Enumerable.Range(1, 10)
                .Select(i => new Contact
                {
                    Id = i,
                    Name = $"Contact {i}",
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    Email = $"contact{i}@test.com",
                    Title = $"Title{i}",
                    Status = EntityStatus.Active
                })
                .ToList();
            await context.Contacts.AddRangeAsync(contacts);
            await context.SaveChangesAsync();
        }

        // Act - Mix of reads and writes
        var completed = await ExecuteWithTimeoutAsync(async () =>
        {
            await ExecuteConcurrentlyAsync(15, async (index) =>
            {
                using var context = CreateContext();
                if (index % 2 == 0)
                {
                    // Read operation
                    return await context.Contacts.ToListAsync();
                }
                else
                {
                    // Write operation
                    var contact = await context.Contacts.FindAsync((index % 10) + 1);
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

