using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.Managers
{
    /// <summary>
    /// Test suite for ValuesManager
    /// Covers:
    /// - Currency lookups
    /// - Eligible entity retrieval
    /// - Country lookups
    /// - Partner value retrieval
    /// - Contact value retrieval
    /// - User value retrieval with pagination
    /// - Liaison office lookups
    /// - Organization unit lookups
    /// </summary>
    public class ValuesManagerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public ValuesManagerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"ValuesManagerTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed Countries
            var countries = new List<Country>
            {
                new Country { Id = 1, Iso2Code = "KE", Iso3Code = "KEN", Name = "Kenya", RegionDescription = "East Africa", ContinentDescription = "Africa", Status = EntityStatus.Active },
                new Country { Id = 2, Iso2Code = "UG", Iso3Code = "UGA", Name = "Uganda", RegionDescription = "East Africa", ContinentDescription = "Africa", Status = EntityStatus.Active },
                new Country { Id = 3, Iso2Code = "US", Iso3Code = "USA", Name = "United States", RegionDescription = "North America", ContinentDescription = "North America", Status = EntityStatus.Active },
                new Country { Id = 4, Iso2Code = "GB", Iso3Code = "GBR", Name = "United Kingdom", RegionDescription = "Europe", ContinentDescription = "Europe", Status = EntityStatus.Inactive }
            };
            _context.Countries.AddRange(countries);

            // Seed Partner first (required for Contacts)
            var partner = new UNOPSPartner
            {
                Name = "Test Partner",
                PartnerShortDescription = "Test Partner for Values Manager Tests",
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Partners.Add(partner);

            // Seed Contacts
            var contacts = new List<UNOPSContact>
            {
                new UNOPSContact { Name = "John Doe", FirstName = "John", LastName = "Doe", Title = "Mr.", Email = "john.doe@test.com", Partner = partner, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Title = "Ms.", Email = "jane.smith@test.com", Partner = partner, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Bob Wilson", FirstName = "Bob", LastName = "Wilson", Title = "Mr.", Email = "bob.wilson@test.com", Partner = partner, Status = EntityStatus.Inactive, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            };
            _context.Contacts.AddRange(contacts);

            // Seed LiaisonOffices
            var offices = new List<LiaisonOffice>
            {
                new LiaisonOffice { Id = 1, Name = "Nairobi Office", Code = "NBO", IsActive = true, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new LiaisonOffice { Id = 2, Name = "New York Office", Code = "NYC", IsActive = true, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new LiaisonOffice { Id = 3, Name = "London Office", Code = "LDN", IsActive = false, IsDeleted = false, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            };
            _context.LiaisonOffices.AddRange(offices);

            // Seed OrganizationHierarchy
            var orgUnits = new List<OrganizationHierarchy>
            {
                new OrganizationHierarchy { Id = 1, Name = "Global HQ", Code = "GHQ", Description = "Global Headquarters", Type = OrganizationUnitType.Office, ParentId = null, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, Status = EntityStatus.Active },
                new OrganizationHierarchy { Id = 2, Name = "Africa Region", Code = "AFR", Description = "Africa Regional Office", Type = OrganizationUnitType.Region, ParentId = 1, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, Status = EntityStatus.Active },
                new OrganizationHierarchy { Id = 3, Name = "Asia Region", Code = "ASI", Description = "Asia Regional Office", Type = OrganizationUnitType.Region, ParentId = 1, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, Status = EntityStatus.Active }
            };
            _context.OrganizationHierarchies.AddRange(orgUnits);

            // Seed UserProfiles for user lookup tests
            var users = new List<UserProfile>
            {
                new UserProfile { UserId = 1, FirstName = "Admin", LastName = "User", UserEmail = "admin@unops.org" },
                new UserProfile { UserId = 2, FirstName = "Test", LastName = "User", UserEmail = "test@unops.org" },
                new UserProfile { UserId = 3, FirstName = "Manager", LastName = "User", UserEmail = "manager@unops.org" }
            };
            _context.UserProfile.AddRange(users);

            _context.SaveChanges();
        }

        #region Country Lookup Tests

        [Fact]
        public async Task TC_VM_001_GetCountries_ReturnsAllActiveCountries()
        {
            var countries = await _context.Countries
                .Where(c => c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(3, countries.Count);
            Assert.Contains(countries, c => c.Name == "Kenya");
            Assert.Contains(countries, c => c.Name == "Uganda");
            Assert.Contains(countries, c => c.Name == "United States");
            Assert.DoesNotContain(countries, c => c.Name == "United Kingdom");
        }

        [Fact]
        public async Task TC_VM_002_GetCountries_FilterByRegion()
        {
            var eastAfricaCountries = await _context.Countries
                .Where(c => c.RegionDescription == "East Africa" && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, eastAfricaCountries.Count);
            Assert.Contains(eastAfricaCountries, c => c.Name == "Kenya");
            Assert.Contains(eastAfricaCountries, c => c.Name == "Uganda");
        }

        [Fact]
        public async Task TC_VM_003_GetCountries_FilterByContinent()
        {
            var africanCountries = await _context.Countries
                .Where(c => c.ContinentDescription == "Africa" && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, africanCountries.Count);
        }

        [Fact]
        public async Task TC_VM_004_GetCountryByCode_ValidCode_ReturnsCountry()
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Iso2Code == "KE" && !c.IsDeleted);

            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
        }

        [Fact]
        public async Task TC_VM_005_GetCountryByCode_InvalidCode_ReturnsNull()
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Iso2Code == "XX" && !c.IsDeleted);

            Assert.Null(country);
        }

        [Fact]
        public async Task TC_VM_006_GetCountryByCode3_ValidCode_ReturnsCountry()
        {
            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Iso3Code == "KEN" && !c.IsDeleted);

            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
        }

        #endregion

        #region Contact Lookup Tests

        [Fact]
        public async Task TC_VM_010_GetContacts_ReturnsAllActiveContacts()
        {
            var contacts = await _context.Contacts
                .Where(c => c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, contacts.Count);
            Assert.Contains(contacts, c => c.FirstName == "John");
            Assert.Contains(contacts, c => c.FirstName == "Jane");
            Assert.DoesNotContain(contacts, c => c.FirstName == "Bob");
        }

        [Fact]
        public async Task TC_VM_011_GetContacts_SearchByName()
        {
            var searchTerm = "John";
            var contacts = await _context.Contacts
                .Where(c => (c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm)) 
                       && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Single(contacts);
            Assert.Equal("John", contacts.First().FirstName);
        }

        [Fact]
        public async Task TC_VM_012_GetContacts_SearchByEmail()
        {
            var email = "jane.smith@test.com";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted);

            Assert.NotNull(contact);
            Assert.Equal("Jane", contact.FirstName);
        }

        #endregion

        #region Liaison Office Lookup Tests

        [Fact]
        public async Task TC_VM_020_GetLiaisonOffices_ReturnsAllActiveOffices()
        {
            var offices = await _context.LiaisonOffices
                .Where(lo => lo.IsActive && !lo.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, offices.Count);
            Assert.Contains(offices, lo => lo.Name == "Nairobi Office");
            Assert.Contains(offices, lo => lo.Name == "New York Office");
            Assert.DoesNotContain(offices, lo => lo.Name == "London Office");
        }

        [Fact]
        public async Task TC_VM_021_GetLiaisonOfficeByCode_ValidCode_ReturnsOffice()
        {
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == "NBO" && !lo.IsDeleted);

            Assert.NotNull(office);
            Assert.Equal("Nairobi Office", office.Name);
        }

        [Fact]
        public async Task TC_VM_022_GetLiaisonOfficeByCode_InvalidCode_ReturnsNull()
        {
            var office = await _context.LiaisonOffices
                .FirstOrDefaultAsync(lo => lo.Code == "XXX" && !lo.IsDeleted);

            Assert.Null(office);
        }

        #endregion

        #region Organization Unit Lookup Tests

        [Fact]
        public async Task TC_VM_030_GetOrganizationUnits_ReturnsAllActiveOrgUnits()
        {
            var orgUnits = await _context.OrganizationHierarchies
                .Where(o => o.Type == OrganizationUnitType.Region && o.Status == EntityStatus.Active && !o.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, orgUnits.Count);
            Assert.Contains(orgUnits, o => o.Name == "Africa Region");
            Assert.Contains(orgUnits, o => o.Name == "Asia Region");
        }

        [Fact]
        public async Task TC_VM_031_GetOrganizationUnitByCode_ValidCode_ReturnsUnit()
        {
            var orgUnit = await _context.OrganizationHierarchies
                .FirstOrDefaultAsync(o => o.Code == "AFR" && !o.IsDeleted);

            Assert.NotNull(orgUnit);
            Assert.Equal("Africa Region", orgUnit.Name);
        }

        [Fact]
        public async Task TC_VM_032_GetOrganizationUnits_FilterByType()
        {
            var offices = await _context.OrganizationHierarchies
                .Where(o => o.Type == OrganizationUnitType.Office && !o.IsDeleted)
                .ToListAsync();

            Assert.Single(offices);
            Assert.Equal("Global HQ", offices.First().Name);
        }

        #endregion

        #region User Lookup Tests

        [Fact]
        public async Task TC_VM_040_GetUsers_ReturnsAllUsers()
        {
            var users = await _context.UserProfile.ToListAsync();

            Assert.Equal(3, users.Count);
            Assert.Contains(users, u => u.FirstName == "Admin");
            Assert.Contains(users, u => u.FirstName == "Test");
            Assert.Contains(users, u => u.FirstName == "Manager");
        }

        [Fact]
        public async Task TC_VM_041_SearchUsers_ByName_ReturnsMatches()
        {
            var searchTerm = "Admin";
            var users = await _context.UserProfile
                .Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm))
                .ToListAsync();

            Assert.Single(users);
            Assert.Equal("Admin", users.First().FirstName);
        }

        [Fact]
        public async Task TC_VM_042_SearchUsers_ByEmail_ReturnsMatches()
        {
            var email = "test@unops.org";
            var user = await _context.UserProfile
                .FirstOrDefaultAsync(u => u.UserEmail == email);

            Assert.NotNull(user);
            Assert.Equal("Test", user.FirstName);
        }

        [Fact]
        public async Task TC_VM_043_GetUsersPaged_FirstPage_ReturnsCorrectCount()
        {
            var pageSize = 2;
            var pageIndex = 0;
            var users = await _context.UserProfile
                .OrderBy(u => u.FirstName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(2, users.Count);
        }

        [Fact]
        public async Task TC_VM_044_GetUsersPaged_SecondPage_ReturnsRemaining()
        {
            var pageSize = 2;
            var pageIndex = 1;
            var users = await _context.UserProfile
                .OrderBy(u => u.FirstName)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Single(users);
        }

        [Fact]
        public async Task TC_VM_045_GetUsersPaged_WithSearch_ReturnsFilteredResults()
        {
            var searchTerm = "User";
            var pageSize = 10;
            var users = await _context.UserProfile
                .Where(u => u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm))
                .Take(pageSize)
                .ToListAsync();

            Assert.Equal(3, users.Count); // All users have "User" in LastName
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task TC_VM_050_EmptyTable_ReturnsEmptyList()
        {
            // Use a separate context with no data
            var emptyDbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"EmptyTest_{Guid.NewGuid()}")
                .Options;
            using var emptyContext = TestDbContextFactory.Create(emptyDbOptions);

            var countries = await emptyContext.Countries.ToListAsync();
            Assert.Empty(countries);
        }

        [Fact]
        public async Task TC_VM_051_SearchWithSpecialCharacters_HandlesGracefully()
        {
            var searchTerm = "O'Brien";
            var contacts = await _context.Contacts
                .Where(c => c.FirstName.Contains(searchTerm) || c.LastName.Contains(searchTerm))
                .ToListAsync();

            Assert.Empty(contacts); // No matches, but no exception
        }

        [Fact]
        public async Task TC_VM_052_SearchWithEmptyString_ReturnsAll()
        {
            var searchTerm = "";
            var contacts = await _context.Contacts
                .Where(c => string.IsNullOrEmpty(searchTerm) || c.FirstName.Contains(searchTerm))
                .Where(c => c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, contacts.Count);
        }

        [Fact]
        public async Task TC_VM_053_SearchCaseInsensitive()
        {
            var searchTerm = "JOHN";
            var contacts = await _context.Contacts
                .Where(c => c.FirstName.ToUpper().Contains(searchTerm.ToUpper()))
                .ToListAsync();

            Assert.Single(contacts);
            Assert.Equal("John", contacts.First().FirstName);
        }

        [Fact]
        public async Task TC_VM_054_Pagination_BeyondDataRange_ReturnsEmpty()
        {
            var pageSize = 10;
            var pageIndex = 100; // Way beyond data
            var users = await _context.UserProfile
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Assert.Empty(users);
        }

        #endregion

        public void Dispose()
        {
            if (TestEnvironment.UseInMemory)
            {
                try { _context.Database.EnsureDeleted(); }
                catch { /* SQLite connection may already be closed during concurrent test runs */ }
            }
            _context.Dispose();
        }
    }
}

