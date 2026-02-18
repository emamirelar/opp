using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance tests for Contact operations
/// </summary>
public class ContactPerformanceTests : PerformanceTestBase
{
    [Fact]
    public async Task GetAllContacts_LargeDataset_Should_CompleteWithinThreshold()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 500)
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
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts.ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold);
        result.Should().HaveCount(500);
    }

    [Fact]
    public async Task GetContactById_Should_CompleteWithinThreshold()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            Name = "John Doe",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Title = "Manager",
            Status = EntityStatus.Active
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts.FindAsync(1));

        // Assert
        elapsed.Should().BeLessThan(FastOperationThreshold);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchContacts_Should_CompleteWithinThreshold()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 200)
            .Select(i => new Contact
            {
                Id = i,
                Name = $"Contact {i}",
                FirstName = i % 2 == 0 ? $"John{i}" : $"Jane{i}",
                LastName = $"Smith{i}",
                Email = $"contact{i}@test.com",
                Title = $"Title{i}",
                Status = EntityStatus.Active
            })
            .ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts.Where(c => c.FirstName.StartsWith("John")).ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold);
        result.Should().HaveCount(100);
    }

    [Fact]
    public async Task BulkCreateContacts_Should_CompleteWithinThreshold()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 100)
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

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            await Context.Contacts.AddRangeAsync(contacts);
            await SaveChangesAsync();
        });

        // Assert
        elapsed.Should().BeLessThan(BulkOperationThreshold);
        var count = await Context.Contacts.CountAsync();
        count.Should().Be(100);
    }
}

