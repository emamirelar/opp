using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Performance;

/// <summary>
/// Performance tests for Contact operations against PostgreSQL.
/// Uses UNOPSContact and creates parent Partners for FK constraints.
/// Uses test markers to filter own data from the shared database.
/// </summary>
public class ContactPerformanceTests : PerformanceTestBase
{
    private readonly string _testMarker = $"PERF_{Guid.NewGuid():N}";
    private readonly List<int> _createdContactIds = new();

    [Fact]
    public async Task GetAllContacts_LargeDataset_Should_CompleteWithinThreshold()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = Enumerable.Range(1, 500)
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
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        foreach (var c in contacts) { _createdContactIds.Add(c.Id); }
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL && _createdContactIds.Any())
            {
                var ids = string.Join(",", _createdContactIds);
                await Context.Database.ExecuteSqlRawAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts.Where(c => c.Name.Contains(_testMarker)).ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold);
        result.Should().HaveCount(500);
    }

    [Fact]
    public async Task GetContactById_Should_CompleteWithinThreshold()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contact = new UNOPSContact
        {
            Name = $"John Doe {_testMarker}",
            FirstName = "John",
            LastName = "Doe",
            Email = $"john_{_testMarker}@test.com",
            Title = "Manager",
            PartnerId = partnerId,
            Status = EntityStatus.Active,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Contacts.AddAsync(contact);
        await SaveChangesAsync();
        RegisterTableCleanup("Contacts", $"\"Id\" = {contact.Id}");

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts.FindAsync(contact.Id));

        // Assert
        elapsed.Should().BeLessThan(FastOperationThreshold);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchContacts_Should_CompleteWithinThreshold()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = Enumerable.Range(1, 200)
            .Select(i => new UNOPSContact
            {
                Name = $"Contact {i} {_testMarker}",
                FirstName = i % 2 == 0 ? $"John{i}" : $"Jane{i}",
                LastName = $"Smith{i}",
                Email = $"contact{i}_{_testMarker}@test.com",
                Title = $"Title{i}",
                PartnerId = partnerId,
                Status = EntityStatus.Active,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.UtcNow
            })
            .ToList();
        await Context.Contacts.AddRangeAsync(contacts);
        await SaveChangesAsync();
        var createdIds = contacts.Select(c => c.Id).ToList();
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                var ids = string.Join(",", createdIds);
                await Context.Database.ExecuteSqlRawAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Act
        var (result, elapsed) = await MeasureAsync(async () =>
            await Context.Contacts
                .Where(c => c.Name.Contains(_testMarker) && c.FirstName!.StartsWith("John"))
                .ToListAsync());

        // Assert
        elapsed.Should().BeLessThan(NormalOperationThreshold);
        result.Should().HaveCount(100);
    }

    [Fact]
    public async Task BulkCreateContacts_Should_CompleteWithinThreshold()
    {
        // Arrange
        var partnerId = await CreateTestPartnerAsync($"Partner_{_testMarker}");
        var contacts = Enumerable.Range(1, 100)
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

        // Act
        var elapsed = await MeasureAsync(async () =>
        {
            await Context.Contacts.AddRangeAsync(contacts);
            await SaveChangesAsync();
        });
        var createdIds = contacts.Select(c => c.Id).ToList();
        RegisterCleanup(async () =>
        {
            if (TestEnvironment.UsePostgreSQL)
            {
                var ids = string.Join(",", createdIds);
                await Context.Database.ExecuteSqlRawAsync($"DELETE FROM public.\"Contacts\" WHERE \"Id\" IN ({ids})");
            }
        });

        // Assert
        elapsed.Should().BeLessThan(BulkOperationThreshold);
        var count = await Context.Contacts.CountAsync(c => c.Name.Contains(_testMarker));
        count.Should().Be(100);
    }
}
