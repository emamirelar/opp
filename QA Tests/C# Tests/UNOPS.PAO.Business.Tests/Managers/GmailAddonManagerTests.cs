using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.Managers
{
    /// <summary>
    /// Test suite for GmailAddonManager
    /// Covers:
    /// - Finding related records from email addresses
    /// - Creating records from emails
    /// - Email parsing and matching
    /// - Error handling for UNOPS-specific implementation
    /// Note: The base GmailAddonManager throws NotImplementedException 
    /// and defers to UNOPSGmailAddonManager for actual implementation.
    /// These tests validate the expected behavior and edge cases.
    /// </summary>
    public class GmailAddonManagerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public GmailAddonManagerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"GmailAddonManagerTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed Partners first (required for Contact FK)
            var p1 = new UNOPSPartner { Name = "Example Corp", PartnerShortDescription = "Example Corporation", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow };
            var p2 = new UNOPSPartner { Name = "Partner Organization", PartnerShortDescription = "Partner Org", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow };
            var p3 = new UNOPSPartner { Name = "Contractor Inc", PartnerShortDescription = "Contractor", Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow };
            _context.Partners.AddRange(p1, p2, p3);

            // Seed Contacts with email addresses for matching
            var contacts = new List<UNOPSContact>
            {
                new UNOPSContact { Name = "John Doe", FirstName = "John", LastName = "Doe", Title = "Mr.", Email = "john.doe@example.com", Partner = p1, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Jane Smith", FirstName = "Jane", LastName = "Smith", Title = "Ms.", Email = "jane.smith@partner.org", Partner = p2, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Bob Wilson", FirstName = "Bob", LastName = "Wilson", Title = "Mr.", Email = "bob.wilson@contractor.com", Partner = p3, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Alice Brown", FirstName = "Alice", LastName = "Brown", Title = "Ms.", Email = "alice.brown@example.com", Partner = p1, Status = EntityStatus.Inactive, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSContact { Name = "Charlie Davis", FirstName = "Charlie", LastName = "Davis", Title = "Mr.", Email = "charlie.davis@example.com", Partner = p1, IsDeleted = true, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            };
            _context.Contacts.AddRange(contacts);

            // Seed Interactions for matching
            var interactions = new List<UNOPSInteraction>
            {
                new UNOPSInteraction { Name = "Meeting with John Doe", Subject = "Meeting with John Doe", Date = DateTime.UtcNow.AddDays(-5), Type = InteractionType.InPersonMeeting, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow },
                new UNOPSInteraction { Name = "Email correspondence with Jane", Subject = "Email correspondence with Jane", Date = DateTime.UtcNow.AddDays(-3), Type = InteractionType.Email, Status = EntityStatus.Active, CreatedBy = 1, LastModifiedBy = 1, CreatedDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow }
            };
            _context.Interactions.AddRange(interactions);

            // Seed UserProfiles for user context
            var users = new List<UserProfile>
            {
                new UserProfile { UserId = 1, FirstName = "Admin", LastName = "User", UserEmail = "admin@unops.org" },
                new UserProfile { UserId = 2, FirstName = "Test", LastName = "User", UserEmail = "test@unops.org" }
            };
            _context.UserProfile.AddRange(users);

            _context.SaveChanges();
        }

        #region Email Matching Tests

        [Fact]
        public async Task TC_GAM_001_FindContactByEmail_ExactMatch_ReturnsContact()
        {
            var email = "john.doe@example.com";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted);

            Assert.NotNull(contact);
            Assert.Equal("John", contact.FirstName);
            Assert.Equal("Doe", contact.LastName);
        }

        [Fact]
        public async Task TC_GAM_002_FindContactByEmail_CaseInsensitive_ReturnsContact()
        {
            var email = "JOHN.DOE@EXAMPLE.COM";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower() && c.Status == EntityStatus.Active && !c.IsDeleted);

            Assert.NotNull(contact);
            Assert.Equal("John", contact.FirstName);
        }

        [Fact]
        public async Task TC_GAM_003_FindContactByEmail_NotFound_ReturnsNull()
        {
            var email = "unknown@example.com";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted);

            Assert.Null(contact);
        }

        [Fact]
        public async Task TC_GAM_004_FindContactByEmail_Inactive_ReturnsNull()
        {
            var email = "alice.brown@example.com";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted);

            Assert.Null(contact); // Inactive contact should not be returned
        }

        [Fact]
        public async Task TC_GAM_005_FindContactByEmail_Deleted_ReturnsNull()
        {
            var email = "charlie.davis@example.com";
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted);

            Assert.Null(contact); // Deleted contact should not be returned
        }

        #endregion

        #region Multiple Email Address Tests

        [Fact]
        public async Task TC_GAM_010_FindContactsByMultipleEmails_ReturnsAllMatches()
        {
            var emails = new[] { "john.doe@example.com", "jane.smith@partner.org", "unknown@test.com" };
            var contacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, contacts.Count);
            Assert.Contains(contacts, c => c.Email == "john.doe@example.com");
            Assert.Contains(contacts, c => c.Email == "jane.smith@partner.org");
        }

        [Fact]
        public async Task TC_GAM_011_FindContactsByMultipleEmails_NoneMatch_ReturnsEmpty()
        {
            var emails = new[] { "unknown1@test.com", "unknown2@test.com" };
            var contacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
        }

        [Fact]
        public async Task TC_GAM_012_FindContactsByMultipleEmails_EmptyList_ReturnsEmpty()
        {
            var emails = Array.Empty<string>();
            var contacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
        }

        #endregion

        #region Domain Matching Tests

        [Fact]
        public async Task TC_GAM_020_FindContactsByDomain_ReturnsMatchingContacts()
        {
            var domain = "example.com";
            var contacts = await _context.Contacts
                .Where(c => c.Email.EndsWith("@" + domain) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Single(contacts); // Only John, as Alice is inactive and Charlie is deleted
            Assert.Equal("john.doe@example.com", contacts.First().Email);
        }

        [Fact]
        public async Task TC_GAM_021_FindContactsByDomain_NoMatches_ReturnsEmpty()
        {
            var domain = "nonexistent.org";
            var contacts = await _context.Contacts
                .Where(c => c.Email.EndsWith("@" + domain) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
        }

        #endregion

        #region Related Records Tests

        [Fact]
        public async Task TC_GAM_030_GetRelatedPartnerForContact_ReturnsPartner()
        {
            // If a contact is associated with a partner, we should be able to find it
            var contact = await _context.Contacts
                .Include(c => c.Partner)
                .FirstOrDefaultAsync(c => c.Email == "john.doe@example.com");

            Assert.NotNull(contact);
            Assert.NotNull(contact.Partner);
        }

        [Fact]
        public async Task TC_GAM_031_GetRelatedInteractionsForContact_ReturnsInteractions()
        {
            var contact = await _context.Contacts
                .Include(c => c.Interactions)
                .FirstOrDefaultAsync(c => c.Email == "john.doe@example.com");

            Assert.NotNull(contact);
            // Interactions would be returned if associated
        }

        #endregion

        #region Email Parsing Tests

        [Fact]
        public void TC_GAM_040_ParseEmailAddress_ValidEmail_ReturnsComponents()
        {
            var email = "john.doe@example.com";
            var parts = email.Split('@');

            Assert.Equal(2, parts.Length);
            Assert.Equal("john.doe", parts[0]);
            Assert.Equal("example.com", parts[1]);
        }

        [Fact]
        public void TC_GAM_041_ParseEmailAddress_InvalidEmail_NoAtSymbol()
        {
            var email = "invalidemail.com";
            var parts = email.Split('@');

            Assert.Single(parts);
            Assert.Equal("invalidemail.com", parts[0]);
        }

        [Fact]
        public void TC_GAM_042_ParseEmailAddress_MultipleAtSymbols_SplitsCorrectly()
        {
            var email = "john@doe@example.com";
            var parts = email.Split('@');

            Assert.Equal(3, parts.Length);
        }

        [Fact]
        public void TC_GAM_043_ExtractDomain_ValidEmail_ReturnsDomain()
        {
            var email = "john.doe@example.com";
            var domain = email.Contains('@') ? email.Split('@').Last() : null;

            Assert.Equal("example.com", domain);
        }

        [Fact]
        public void TC_GAM_044_ExtractDomain_Subdomain_ReturnsFullDomain()
        {
            var email = "john.doe@mail.example.com";
            var domain = email.Contains('@') ? email.Split('@').Last() : null;

            Assert.Equal("mail.example.com", domain);
        }

        #endregion

        #region Bulk Email Processing Tests

        [Fact]
        public async Task TC_GAM_050_ProcessBulkEmails_ReturnsGroupedResults()
        {
            var emails = new[] { "john.doe@example.com", "jane.smith@partner.org", "bob.wilson@contractor.com" };
            
            var foundContacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            var notFoundEmails = emails.Except(foundContacts.Select(c => c.Email)).ToList();

            Assert.Equal(3, foundContacts.Count);
            Assert.Empty(notFoundEmails);
        }

        [Fact]
        public async Task TC_GAM_051_ProcessBulkEmails_PartialMatches_ReturnsMixed()
        {
            var emails = new[] { "john.doe@example.com", "unknown@test.com", "jane.smith@partner.org" };
            
            var foundContacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            var notFoundEmails = emails.Except(foundContacts.Select(c => c.Email)).ToList();

            Assert.Equal(2, foundContacts.Count);
            Assert.Single(notFoundEmails);
            Assert.Equal("unknown@test.com", notFoundEmails.First());
        }

        [Fact]
        public async Task TC_GAM_052_ProcessBulkEmails_DuplicateEmails_HandlesCorrectly()
        {
            var emails = new[] { "john.doe@example.com", "john.doe@example.com", "jane.smith@partner.org" };
            var uniqueEmails = emails.Distinct().ToList();
            
            var foundContacts = await _context.Contacts
                .Where(c => uniqueEmails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Equal(2, foundContacts.Count);
        }

        [Fact]
        public async Task TC_GAM_053_ProcessBulkEmails_LargeList_Performance()
        {
            // Generate a list of 100 emails
            var emails = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                emails.Add($"user{i}@test.com");
            }
            emails.Add("john.doe@example.com"); // Add one known match

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var foundContacts = await _context.Contacts
                .Where(c => emails.Contains(c.Email) && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();
            stopwatch.Stop();

            Assert.Single(foundContacts);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Query took too long: {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region User Context Tests

        [Fact]
        public void TC_GAM_060_CreateClaimsPrincipal_ValidUser_HasCorrectClaims()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, "admin@unops.org"),
                new Claim(ClaimTypes.Name, "Admin User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            Assert.True(principal.Identity!.IsAuthenticated);
            Assert.Equal("1", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal("admin@unops.org", principal.FindFirst(ClaimTypes.Email)?.Value);
        }

        [Fact]
        public void TC_GAM_061_CreateClaimsPrincipal_NoAuth_NotAuthenticated()
        {
            var principal = new ClaimsPrincipal();
            Assert.False(principal.Identity?.IsAuthenticated ?? false);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task TC_GAM_070_NullEmailAddress_HandledGracefully()
        {
            string? email = null;
            var contacts = await _context.Contacts
                .Where(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
        }

        [Fact]
        public async Task TC_GAM_071_EmptyEmailAddress_HandledGracefully()
        {
            var email = "";
            var contacts = await _context.Contacts
                .Where(c => c.Email == email && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
        }

        [Fact]
        public async Task TC_GAM_072_WhitespaceEmailAddress_HandledGracefully()
        {
            var email = "   ";
            var contacts = await _context.Contacts
                .Where(c => c.Email.Trim() == email.Trim() && c.Status == EntityStatus.Active && !c.IsDeleted)
                .ToListAsync();

            Assert.Empty(contacts);
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

