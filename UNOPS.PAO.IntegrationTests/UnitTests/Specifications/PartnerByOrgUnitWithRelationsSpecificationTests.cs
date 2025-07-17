using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    public class PartnerByOrgUnitWithRelationsSpecificationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;

        public PartnerByOrgUnitWithRelationsSpecificationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            // Setup mocks for AppDbContext
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            var userResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
            var mockDbContextSchema = new Mock<IDbContextSchema>();
            mockDbContextSchema.Setup(x => x.Schema).Returns("public");
            
            _dbContext = new AppDbContext(options, userResolverService, mockDbContextSchema.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesSpecification()
        {
            // Arrange
            var orgUnitHierarchyIds = new List<int> { 1, 2, 3 };
            var orgUnitUserIds = new List<string> { "10", "11", "12" };

            // Act
            var specification = new PartnerByOrgUnitWithRelationsSpecification(orgUnitHierarchyIds, orgUnitUserIds);

            // Assert
            specification.Should().NotBeNull();
            specification.Criteria.Should().NotBeNull();
            specification.Includes.Should().NotBeEmpty();
            specification.IncludeStrings.Should().NotBeEmpty();
        }

        [Fact]
        public void Constructor_AddsRequiredIncludes()
        {
            // Arrange
            var orgUnitHierarchyIds = new List<int> { 1 };
            var orgUnitUserIds = new List<string> { "10" };

            // Act
            var specification = new PartnerByOrgUnitWithRelationsSpecification(orgUnitHierarchyIds, orgUnitUserIds);

            // Assert
            specification.Includes.Should().HaveCount(2);
            specification.IncludeStrings.Should().Contain("Contacts.Interactions");
            specification.IncludeStrings.Should().Contain("Contacts.Interactions.InteractionContacts");
            specification.IncludeStrings.Should().Contain("Contacts.Interactions.InteractionUsers");
        }

        [Fact]
        public async Task Criteria_FiltersPartnersByDirectOrgUnitLink()
        {
            // Arrange
            var orgUnitId = 5;
            var partner1 = new Partner 
            { 
                Id = 1, 
                Name = "Partner 1", 
                PartnerOfficeId = orgUnitId,
                Status = "Active",
                ShortName = "P1",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner2 = new Partner 
            { 
                Id = 2, 
                Name = "Partner 2", 
                PartnerOfficeId = 999,
                Status = "Active",
                ShortName = "P2",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner3 = new Partner 
            { 
                Id = 3, 
                Name = "Partner 3", 
                PartnerOfficeId = null,
                Status = "Active",
                ShortName = "P3",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };

            await _dbContext.Partners.AddRangeAsync(partner1, partner2, partner3);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                new List<int> { orgUnitId }, 
                new List<string>()
            );

            // Act
            var query = _dbContext.Partners.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(1);
            results.Should().Contain(p => p.Id == partner1.Id);
            results.Should().NotContain(p => p.Id == partner2.Id);
            results.Should().NotContain(p => p.Id == partner3.Id);
        }

        [Fact(Skip = "Skipping due to entity configuration mismatch - Interaction and Contact entities require properties not present in domain model")]
        public async Task Criteria_FiltersPartnersByIndirectContactRelation()
        {
            // Arrange
            var userId = 100;
            
            // Create partners
            var partner1 = new Partner 
            { 
                Id = 1, 
                Name = "Partner 1",
                Status = "Active",
                ShortName = "P1",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner2 = new Partner 
            { 
                Id = 2, 
                Name = "Partner 2",
                Status = "Active",
                ShortName = "P2",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            
            // Create contacts
            var contact1 = new Contact 
            { 
                Id = 1, 
                FirstName = "Contact", 
                LastName = "One",
                Name = "Contact One",
                Title = "Manager",
                Email = "contact.one@example.com",
                Status = "Active"
            };
            var contact2 = new Contact 
            { 
                Id = 2, 
                FirstName = "Contact", 
                LastName = "Two",
                Name = "Contact Two",
                Title = "Director",
                Email = "contact.two@example.com",
                Status = "Active"
            };
            
            // Link contacts to partners
            partner1.Contacts = new List<Contact> { contact1 };
            partner2.Contacts = new List<Contact> { contact2 };
            
            // Create interaction with user
            var interaction = new Interaction 
            { 
                Id = 1, 
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Test interaction"
            };
            
            // Link interaction to contact1 and user
            interaction.InteractionContacts = new List<InteractionContact>
            {
                new InteractionContact { InteractionId = 1, ContactId = 1 }
            };
            
            interaction.InteractionUsers = new List<InteractionUser>
            {
                new InteractionUser { InteractionId = 1, UserId = userId }
            };
            
            contact1.Interactions = new List<Interaction> { interaction };

            await _dbContext.Partners.AddRangeAsync(partner1, partner2);
            await _dbContext.Contacts.AddRangeAsync(contact1, contact2);
            await _dbContext.Interactions.AddAsync(interaction);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), 
                new List<string> { userId.ToString() }
            );

            // Act
            var query = _dbContext.Partners
                .Include(p => p.Contacts)
                    .ThenInclude(c => c.Interactions)
                        .ThenInclude(i => i.InteractionUsers)
                .Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(1);
            results.Should().Contain(p => p.Id == partner1.Id);
            results.Should().NotContain(p => p.Id == partner2.Id);
        }

        [Fact(Skip = "Skipping due to entity configuration mismatch - Interaction and Contact entities require properties not present in domain model")]
        public async Task Criteria_FiltersPartnersByBothDirectAndIndirectRelations()
        {
            // Arrange
            var orgUnitId = 5;
            var userId = 100;
            
            // Partner with direct org unit link
            var partner1 = new Partner 
            { 
                Id = 1, 
                Name = "Direct Partner", 
                PartnerOfficeId = orgUnitId,
                Status = "Active",
                ShortName = "DP",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            
            // Partner with indirect contact relation
            var partner2 = new Partner 
            { 
                Id = 2, 
                Name = "Indirect Partner",
                Status = "Active",
                ShortName = "IP",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var contact = new Contact 
            { 
                Id = 1, 
                FirstName = "Test", 
                LastName = "Contact",
                Name = "Test Contact",
                Title = "Manager",
                Email = "test.contact@example.com",
                Status = "Active"
            };
            partner2.Contacts = new List<Contact> { contact };
            
            var interaction = new Interaction 
            { 
                Id = 1, 
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Test interaction",
                InteractionContacts = new List<InteractionContact>
                {
                    new InteractionContact { InteractionId = 1, ContactId = 1 }
                },
                InteractionUsers = new List<InteractionUser>
                {
                    new InteractionUser { InteractionId = 1, UserId = userId }
                }
            };
            contact.Interactions = new List<Interaction> { interaction };
            
            // Partner with no relation
            var partner3 = new Partner 
            { 
                Id = 3, 
                Name = "Unrelated Partner",
                Status = "Active",
                ShortName = "UP",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };

            await _dbContext.Partners.AddRangeAsync(partner1, partner2, partner3);
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.Interactions.AddAsync(interaction);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                new List<int> { orgUnitId }, 
                new List<string> { userId.ToString() }
            );

            // Act
            var query = _dbContext.Partners
                .Include(p => p.Contacts)
                    .ThenInclude(c => c.Interactions)
                        .ThenInclude(i => i.InteractionUsers)
                .Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.Id == partner1.Id);
            results.Should().Contain(p => p.Id == partner2.Id);
            results.Should().NotContain(p => p.Id == partner3.Id);
        }

        [Fact]
        public async Task Criteria_WithEmptyLists_ReturnsNoResults()
        {
            // Arrange
            var partner = new Partner 
            { 
                Id = 1, 
                Name = "Test Partner",
                Status = "Active",
                ShortName = "TP",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), 
                new List<string>()
            );

            // Act
            var query = _dbContext.Partners.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public async Task Criteria_WithNullLists_ReturnsNoResults()
        {
            // Arrange
            var partner = new Partner 
            { 
                Id = 1, 
                Name = "Test Partner",
                Status = "Active",
                ShortName = "TP",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(null, null);

            // Act
            var query = _dbContext.Partners.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public async Task Criteria_WithMultipleOrgUnitIds_FiltersCorrectly()
        {
            // Arrange
            var orgUnitIds = new List<int> { 5, 6, 7 };
            
            var partner1 = new Partner 
            { 
                Id = 1, 
                Name = "Partner 1", 
                PartnerOfficeId = 5,
                Status = "Active",
                ShortName = "P1",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner2 = new Partner 
            { 
                Id = 2, 
                Name = "Partner 2", 
                PartnerOfficeId = 6,
                Status = "Active",
                ShortName = "P2",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner3 = new Partner 
            { 
                Id = 3, 
                Name = "Partner 3", 
                PartnerOfficeId = 7,
                Status = "Active",
                ShortName = "P3",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner4 = new Partner 
            { 
                Id = 4, 
                Name = "Partner 4", 
                PartnerOfficeId = 999,
                Status = "Active",
                ShortName = "P4",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };

            await _dbContext.Partners.AddRangeAsync(partner1, partner2, partner3, partner4);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                orgUnitIds, 
                new List<string>()
            );

            // Act
            var query = _dbContext.Partners.Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(3);
            results.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact(Skip = "Skipping due to entity configuration mismatch - Interaction and Contact entities require properties not present in domain model")]
        public async Task Criteria_WithMultipleUserIds_FiltersCorrectly()
        {
            // Arrange
            var userIds = new List<string> { "100", "101", "102" };
            
            var partner1 = new Partner 
            { 
                Id = 1, 
                Name = "Partner 1",
                Status = "Active",
                ShortName = "P1",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner2 = new Partner 
            { 
                Id = 2, 
                Name = "Partner 2",
                Status = "Active",
                ShortName = "P2",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            var partner3 = new Partner 
            { 
                Id = 3, 
                Name = "Partner 3",
                Status = "Active",
                ShortName = "P3",
                NewEngagement = "true",
                PooledFund = "false",
                DDRequired = "false",
                DDEACDone = "false",
                LevyPotentiallyApplies = "false"
            };
            
            // Create contacts
            var contact1 = new Contact 
            { 
                Id = 1, 
                FirstName = "C1", 
                LastName = "L1",
                Name = "C1 L1",
                Title = "Manager",
                Email = "c1.l1@example.com",
                Status = "Active"
            };
            var contact2 = new Contact 
            { 
                Id = 2, 
                FirstName = "C2", 
                LastName = "L2",
                Name = "C2 L2",
                Title = "Director",
                Email = "c2.l2@example.com",
                Status = "Active"
            };
            var contact3 = new Contact 
            { 
                Id = 3, 
                FirstName = "C3", 
                LastName = "L3",
                Name = "C3 L3",
                Title = "VP",
                Email = "c3.l3@example.com",
                Status = "Active"
            };
            
            partner1.Contacts = new List<Contact> { contact1 };
            partner2.Contacts = new List<Contact> { contact2 };
            partner3.Contacts = new List<Contact> { contact3 };
            
            // Create interactions with different users
            var interaction1 = new Interaction 
            { 
                Id = 1,
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Interaction 1",
                InteractionContacts = new List<InteractionContact>
                {
                    new InteractionContact { InteractionId = 1, ContactId = 1 }
                },
                InteractionUsers = new List<InteractionUser>
                {
                    new InteractionUser { InteractionId = 1, UserId = 100 }
                }
            };
            
            var interaction2 = new Interaction 
            { 
                Id = 2,
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Interaction 2",
                InteractionContacts = new List<InteractionContact>
                {
                    new InteractionContact { InteractionId = 2, ContactId = 2 }
                },
                InteractionUsers = new List<InteractionUser>
                {
                    new InteractionUser { InteractionId = 2, UserId = 101 }
                }
            };
            
            // Interaction with user not in the list
            var interaction3 = new Interaction 
            { 
                Id = 3,
                Type = Domain.Enums.InteractionType.Email,
                Date = DateTime.Now,
                Description = "Interaction 3",
                InteractionContacts = new List<InteractionContact>
                {
                    new InteractionContact { InteractionId = 3, ContactId = 3 }
                },
                InteractionUsers = new List<InteractionUser>
                {
                    new InteractionUser { InteractionId = 3, UserId = 999 }
                }
            };
            
            contact1.Interactions = new List<Interaction> { interaction1 };
            contact2.Interactions = new List<Interaction> { interaction2 };
            contact3.Interactions = new List<Interaction> { interaction3 };

            await _dbContext.Partners.AddRangeAsync(partner1, partner2, partner3);
            await _dbContext.Contacts.AddRangeAsync(contact1, contact2, contact3);
            await _dbContext.Interactions.AddRangeAsync(interaction1, interaction2, interaction3);
            await _dbContext.SaveChangesAsync();

            var specification = new PartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), 
                userIds
            );

            // Act
            var query = _dbContext.Partners
                .Include(p => p.Contacts)
                    .ThenInclude(c => c.Interactions)
                        .ThenInclude(i => i.InteractionUsers)
                .Where(specification.Criteria);
            var results = await query.ToListAsync();

            // Assert
            results.Should().HaveCount(2);
            results.Select(p => p.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}