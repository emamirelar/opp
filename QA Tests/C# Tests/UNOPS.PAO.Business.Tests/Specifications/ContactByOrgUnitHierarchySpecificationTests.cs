using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    public class ContactByOrgUnitHierarchySpecificationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public ContactByOrgUnitHierarchySpecificationTests()
        {
            // Use the centralized test factory which provides SQLite in-memory
            // (supports relational features) instead of EF Core InMemory provider.
            _dbContext = TestDbContextFactory.Create();
            if (TestEnvironment.UsePostgreSQL)
            {
                _transaction = _dbContext.Database.BeginTransaction();
            }
        }

        [Fact]
        public void Constructor_WithValidOrgUnitIds_CreatesSpecification()
        {
            // Arrange
            var orgUnitHierarchyIds = new List<int> { 1, 2, 3 };

            // Act
            var specification = new ContactByOrgUnitHierarchySpecification(orgUnitHierarchyIds);

            // Assert
            specification.Should().NotBeNull();
            specification.Criteria.Should().NotBeNull();
            // Note: OrganizationUnitRelationships filtering is now handled via ApplyOrgUnitFilter method
        }

        [Fact]
        public void Constructor_AddsRequiredIncludes()
        {
            // Arrange
            var orgUnitHierarchyIds = new List<int> { 1 };

            // Act
            var specification = new ContactByOrgUnitHierarchySpecification(orgUnitHierarchyIds);

            // Assert
            specification.Includes.Should().HaveCount(1);
            specification.Includes.Should().Contain(include => include.Body.ToString().Contains("Partner"));
            // Note: OrganizationUnitRelationships filtering is now handled via ApplyOrgUnitFilter method
        }

        [SkipIfInMemoryFact]
        public async Task Criteria_FiltersContactsByPartnerOrgUnit()
        {
            // Arrange - create org hierarchies first (FK constraint)
            var orgUnit1 = await CreateOrganizationHierarchyAsync("OU1", "Org Unit 1");
            var orgUnit2 = await CreateOrganizationHierarchyAsync("OU2", "Org Unit 2");
            
            // Create partners with different org units
            var partner1 = await CreateTestPartnerAsync("Partner 1", orgUnit1.Id);
            var partner2 = await CreateTestPartnerAsync("Partner 2", orgUnit2.Id);
            var partner3 = await CreateTestPartnerAsync("Partner 3", null);
            
            // Create contacts for each partner
            var contact1 = await CreateTestContactAsync("Contact", "One", partner1.Id);
            var contact2 = await CreateTestContactAsync("Contact", "Two", partner2.Id);
            var contact3 = await CreateTestContactAsync("Contact", "Three", partner3.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(new List<int> { orgUnit1.Id });

            // Act
            var query = _dbContext.Contacts
                .Include(c => c.Partner)
                .Where(specification.Criteria);
            query = specification.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(1);
            results.Should().Contain(c => c.Id == contact1.Id);
            results.Should().NotContain(c => c.Id == contact2.Id);
            results.Should().NotContain(c => c.Id == contact3.Id);
        }

        [SkipIfInMemoryFact]
        public async Task Criteria_WithMultipleOrgUnitIds_FiltersCorrectly()
        {
            // Arrange - create org hierarchies first (FK constraint)
            var orgUnit1 = await CreateOrganizationHierarchyAsync("OU1", "Org Unit 1");
            var orgUnit2 = await CreateOrganizationHierarchyAsync("OU2", "Org Unit 2");
            var orgUnit3 = await CreateOrganizationHierarchyAsync("OU3", "Org Unit 3");
            var orgUnit4 = await CreateOrganizationHierarchyAsync("OU4", "Org Unit 4");
            var orgUnitIds = new List<int> { orgUnit1.Id, orgUnit2.Id, orgUnit3.Id };
            
            // Create partners with different org units
            var partner1 = await CreateTestPartnerAsync("Partner 1", orgUnit1.Id);
            var partner2 = await CreateTestPartnerAsync("Partner 2", orgUnit2.Id);
            var partner3 = await CreateTestPartnerAsync("Partner 3", orgUnit3.Id);
            var partner4 = await CreateTestPartnerAsync("Partner 4", orgUnit4.Id);
            
            // Create contacts
            var contact1 = await CreateTestContactAsync("C1", "L1", partner1.Id);
            var contact2 = await CreateTestContactAsync("C2", "L2", partner2.Id);
            var contact3 = await CreateTestContactAsync("C3", "L3", partner3.Id);
            var contact4 = await CreateTestContactAsync("C4", "L4", partner4.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(orgUnitIds);

            // Act
            var query = _dbContext.Contacts
                .Include(c => c.Partner)
                .Where(specification.Criteria);
            query = specification.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(3);
            results.Select(c => c.Id).Should().BeEquivalentTo(new[] { contact1.Id, contact2.Id, contact3.Id });
            results.Should().NotContain(c => c.Id == contact4.Id);
        }

        [Fact]
        public async Task Criteria_WithEmptyOrgUnitList_ReturnsNoResults()
        {
            // Arrange - partner without org unit (spec uses empty list, so no match needed)
            var partner = await CreateTestPartnerAsync("Test Partner", null);
            var contact = await CreateTestContactAsync("Test", "Contact", partner.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(new List<int>());

            // Act
            var query = _dbContext.Contacts.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public async Task Criteria_WithNullOrgUnitList_ReturnsNoResults()
        {
            // Arrange - partner without org unit (spec uses null list, so no match needed)
            var partner = await CreateTestPartnerAsync("Test Partner", null);
            var contact = await CreateTestContactAsync("Test", "Contact", partner.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(null);

            // Act
            var query = _dbContext.Contacts.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public async Task Criteria_ExcludesContactsWithNullPartner()
        {
            // Arrange - create org hierarchy first (FK constraint)
            var orgUnit = await CreateOrganizationHierarchyAsync("OU", "Test Org Unit");
            
            // Create partners - one with org unit (included by spec), one without (excluded)
            var partnerWithOrgUnit = await CreateTestPartnerAsync("Test Partner", orgUnit.Id);
            var partnerWithoutOrgUnit = await CreateTestPartnerAsync("Other Partner", null);
            
            // Create contacts - one with partner in org unit, one with partner NOT in org unit
            var contactWithPartner = await CreateTestContactAsync("With", "Partner", partnerWithOrgUnit.Id);
            var contactWithoutPartner = await CreateTestContactAsync("Without", "Partner", partnerWithoutOrgUnit.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(new List<int> { orgUnit.Id });

            // Act
            var query = _dbContext.Contacts
                .Include(c => c.Partner)
                .Where(specification.Criteria);
            query = specification.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            // Assert - only contact whose partner has org unit relationship is included
            results.Should().HaveCount(1);
            results.Should().Contain(c => c.Id == contactWithPartner.Id);
            results.Should().NotContain(c => c.Id == contactWithoutPartner.Id);
        }

        [SkipIfInMemoryFact]
        public async Task Criteria_ExcludesContactsWherePartnerHasNullOfficeId()
        {
            // Arrange - create org hierarchy first (FK constraint)
            var orgUnit = await CreateOrganizationHierarchyAsync("OU", "Test Org Unit");
            
            // Create partners - one with office ID, one without
            var partnerWithOffice = await CreateTestPartnerAsync("Partner With Office", orgUnit.Id);
            var partnerWithoutOffice = await CreateTestPartnerAsync("Partner Without Office", null);
            
            // Create contacts for each partner
            var contact1 = await CreateTestContactAsync("Contact", "One", partnerWithOffice.Id);
            var contact2 = await CreateTestContactAsync("Contact", "Two", partnerWithoutOffice.Id);

            var specification = new ContactByOrgUnitHierarchySpecification(new List<int> { orgUnit.Id });

            // Act
            var query = _dbContext.Contacts
                .Include(c => c.Partner)
                .Where(specification.Criteria);
            query = specification.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(1);
            results.Should().Contain(c => c.Id == contact1.Id);
            results.Should().NotContain(c => c.Id == contact2.Id);
        }

        private async Task<OrganizationHierarchy> CreateOrganizationHierarchyAsync(string code, string name)
        {
            var org = new OrganizationHierarchy
            {
                Code = code,
                Name = name,
                Type = OrganizationUnitType.OrgUnit,
                Description = name,
                Status = Domain.Entities.EntityStatus.Active
            };
            await _dbContext.Set<OrganizationHierarchy>().AddAsync(org);
            await _dbContext.SaveChangesAsync();
            return org;
        }

        private async Task<Partner> CreateTestPartnerAsync(string name, int? organizationHierarchyId)
        {
            var partner = new Partner
            {
                Name = name,
                PartnerShortDescription = name.Length > 10 ? name.Substring(0, 10) : name,
                PartnerCategoryId = 1,
                LiaisonOfficeId = 1,
                UNAndStateEntity = false,
                Status = Domain.Entities.EntityStatus.Active,
                CanCreateNewOpportunities = false,
                PooledFund = false,
                DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();

            if (organizationHierarchyId.HasValue)
            {
                var orgRelationship = new OrganizationUnitRelationship
                {
                    Name = $"Partner-{partner.Id}-OrgUnit-{organizationHierarchyId.Value}",
                    OrganizationHierarchyId = organizationHierarchyId.Value,
                    EntityId = partner.Id,
                    EntityType = nameof(Partner),
                    Status = Domain.Entities.EntityStatus.Active
                };
                _dbContext.Set<OrganizationUnitRelationship>().Add(orgRelationship);
                await _dbContext.SaveChangesAsync();
            }

            return partner;
        }

        private async Task<Contact> CreateTestContactAsync(string firstName, string lastName, int partnerId)
        {
            var contact = new Contact
            {
                FirstName = firstName,
                LastName = lastName,
                Name = $"{firstName} {lastName}",
                Title = "Manager",
                Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
                Status = EntityStatus.Active,
                PartnerId = partnerId,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();
            return contact;
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
            _dbContext?.Dispose();
        }
    }
}
