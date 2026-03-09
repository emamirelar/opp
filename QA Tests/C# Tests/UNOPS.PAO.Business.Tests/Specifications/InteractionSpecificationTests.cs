using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.DataAccess.Context;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    #region InteractionByTypeSpecification Tests

    public class InteractionByTypeSpecificationTests
    {
        private static Interaction CreateInteraction(InteractionType type) =>
            new()
            {
                Id = 1,
                Name = "Test Interaction",
                Type = type,
                Date = DateTime.UtcNow,
                Subject = "Test Subject",
                Description = "Test Description"
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesEmailType()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Email);
            spec.Criteria.Compile()(CreateInteraction(InteractionType.Email)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentType()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Email);
            spec.Criteria.Compile()(CreateInteraction(InteractionType.Call)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchOtherType()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.VirtualMeeting);
            spec.Criteria.Compile()(CreateInteraction(InteractionType.InPersonMeeting)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchChatWhenSearchingCall()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Call);
            spec.Criteria.Compile()(CreateInteraction(InteractionType.Chat)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Theory]
        [InlineData(InteractionType.Email)]
        [InlineData(InteractionType.Chat)]
        [InlineData(InteractionType.Call)]
        [InlineData(InteractionType.VirtualMeeting)]
        [InlineData(InteractionType.InPersonMeeting)]
        [InlineData(InteractionType.Other)]
        public void Criteria_MatchesEachEnumValue(InteractionType type)
        {
            var spec = new InteractionByTypeSpecification(type);
            spec.Criteria.Compile()(CreateInteraction(type)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_FiltersByExactType()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Email);
            var interactions = new[]
            {
                CreateInteraction(InteractionType.Email),
                CreateInteraction(InteractionType.Call),
                CreateInteraction(InteractionType.Email),
                CreateInteraction(InteractionType.Chat),
            };
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_OtherTypeMatchesOnlyOther()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Other);
            spec.Criteria.Compile()(CreateInteraction(InteractionType.Email)).Should().BeFalse();
            spec.Criteria.Compile()(CreateInteraction(InteractionType.Other)).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Email);
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_CanCompileAndInvoke()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.Call);
            var func = spec.Criteria.Compile();
            func.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_FiltersCollectionCorrectly()
        {
            var spec = new InteractionByTypeSpecification(InteractionType.VirtualMeeting);
            var interactions = Enum.GetValues<InteractionType>()
                .Select(CreateInteraction).ToArray();
            interactions.Where(spec.Criteria.Compile()).Should().ContainSingle();
        }
    }

    #endregion

    #region InteractionByTextSpecification Tests

    public class InteractionByTextSpecificationTests
    {
        private static Interaction CreateInteraction(string? description = "Default description") =>
            new()
            {
                Id = 1,
                Name = "Test",
                Type = InteractionType.Email,
                Date = DateTime.UtcNow,
                Subject = "Subject",
                Description = description
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesDescriptionText()
        {
            var spec = new InteractionByTextSpecification("important");
            spec.Criteria.Compile()(CreateInteraction("This is an important meeting")).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentText()
        {
            var spec = new InteractionByTextSpecification("urgent");
            spec.Criteria.Compile()(CreateInteraction("This is a normal meeting")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchNullDescription()
        {
            var spec = new InteractionByTextSpecification("test");
            spec.Criteria.Compile()(CreateInteraction(null)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchEmptyDescription()
        {
            var spec = new InteractionByTextSpecification("test");
            spec.Criteria.Compile()(CreateInteraction("")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Theory]
        [InlineData("important", "IMPORTANT meeting", true)]
        [InlineData("IMPORTANT", "important meeting", true)]
        [InlineData("ImPoRtAnT", "Important meeting", true)]
        public void Criteria_IsCaseInsensitive(string search, string desc, bool expected)
        {
            var spec = new InteractionByTextSpecification(search);
            spec.Criteria.Compile()(CreateInteraction(desc)).Should().Be(expected);
        }

        [Fact]
        public void Criteria_MatchesPartialText()
        {
            var spec = new InteractionByTextSpecification("meet");
            spec.Criteria.Compile()(CreateInteraction("Team meeting notes")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesSingleCharacter()
        {
            var spec = new InteractionByTextSpecification("T");
            spec.Criteria.Compile()(CreateInteraction("Test")).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_FiltersCollectionByText()
        {
            var spec = new InteractionByTextSpecification("quarterly");
            var interactions = new[]
            {
                CreateInteraction("Quarterly review meeting"),
                CreateInteraction("Daily standup"),
                CreateInteraction("Quarterly budget review"),
            };
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionByTextSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_MatchesMiddleOfText()
        {
            var spec = new InteractionByTextSpecification("scrip");
            spec.Criteria.Compile()(CreateInteraction("Default description")).Should().BeTrue();
        }
    }

    #endregion

    #region InteractionByDateRangeSpecification Tests

    public class InteractionByDateRangeSpecificationTests
    {
        private static Interaction CreateInteractionOnDate(DateTime date) =>
            new()
            {
                Id = 1,
                Name = "Test",
                Type = InteractionType.Email,
                Date = date,
                Subject = "Subject",
                Description = "Description"
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesDateWithinRange()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2025, 6, 15))).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDateBeforeRange()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2024, 12, 31))).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchDateAfterRange()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2026, 1, 1))).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchDateWellOutsideRange()
        {
            var from = new DateTime(2025, 6, 1);
            var to = new DateTime(2025, 6, 30);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2020, 1, 1))).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_MatchesExactStartDate()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(from)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesExactEndDate()
        {
            var from = new DateTime(2025, 1, 1);
            var to = new DateTime(2025, 12, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            spec.Criteria.Compile()(CreateInteractionOnDate(to)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_FromOnlyMatchesDatesAfterFrom()
        {
            var from = new DateTime(2025, 6, 1);
            var spec = new InteractionByDateRangeSpecification(from);
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2026, 1, 1))).Should().BeTrue();
            spec.Criteria.Compile()(CreateInteractionOnDate(new DateTime(2024, 1, 1))).Should().BeFalse();
        }

        // --- Functional ---
        [Fact]
        public void LastDays_CreatesCorrectSpecification()
        {
            var spec = InteractionByDateRangeSpecification.LastDays(30);
            spec.Should().NotBeNull();
            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Compile()(CreateInteractionOnDate(DateTime.UtcNow)).Should().BeTrue();
        }

        [Fact]
        public void LastDays_DoesNotMatchOldDates()
        {
            var spec = InteractionByDateRangeSpecification.LastDays(7);
            spec.Criteria.Compile()(CreateInteractionOnDate(DateTime.UtcNow.AddDays(-30))).Should().BeFalse();
        }

        [Fact]
        public void Criteria_FiltersCollectionByDateRange()
        {
            var from = new DateTime(2025, 3, 1);
            var to = new DateTime(2025, 3, 31);
            var spec = new InteractionByDateRangeSpecification(from, to);
            var interactions = new[]
            {
                CreateInteractionOnDate(new DateTime(2025, 2, 28)),
                CreateInteractionOnDate(new DateTime(2025, 3, 15)),
                CreateInteractionOnDate(new DateTime(2025, 3, 1)),
                CreateInteractionOnDate(new DateTime(2025, 4, 1)),
            };
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }
    }

    #endregion

    #region InteractionByContactSpecification Tests

    public class InteractionByContactSpecificationTests
    {
        // --- Positive ---
        [Fact]
        public void Criteria_MatchesInteractionWithContact()
        {
            var spec = new InteractionByContactSpecification(42);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 42 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchInteractionWithoutContact()
        {
            var spec = new InteractionByContactSpecification(42);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 99 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchEmptyContactList()
        {
            var spec = new InteractionByContactSpecification(1);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>()
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchDifferentContactId()
        {
            var spec = new InteractionByContactSpecification(1);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 2 },
                    new() { ContactId = 3 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_MatchesWhenContactIsInMultipleContacts()
        {
            var spec = new InteractionByContactSpecification(5);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 3 },
                    new() { ContactId = 5 },
                    new() { ContactId = 7 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesZeroContactId()
        {
            var spec = new InteractionByContactSpecification(0);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 0 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesSingleContactInList()
        {
            var spec = new InteractionByContactSpecification(1);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject",
                InteractionContacts = new List<InteractionContact>
                {
                    new() { ContactId = 1 }
                }
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionByContactSpecification(1);
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_UsesAnyOnInteractionContacts()
        {
            var spec = new InteractionByContactSpecification(10);
            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Body.ToString().Should().Contain("InteractionContacts");
        }

        [Fact]
        public void Criteria_FiltersCollectionByContact()
        {
            var spec = new InteractionByContactSpecification(42);
            var interactions = new[]
            {
                new Interaction { Id = 1, Name = "T1", Type = InteractionType.Email, Date = DateTime.UtcNow, Subject = "S",
                    InteractionContacts = new List<InteractionContact> { new() { ContactId = 42 } } },
                new Interaction { Id = 2, Name = "T2", Type = InteractionType.Call, Date = DateTime.UtcNow, Subject = "S",
                    InteractionContacts = new List<InteractionContact> { new() { ContactId = 99 } } },
                new Interaction { Id = 3, Name = "T3", Type = InteractionType.Chat, Date = DateTime.UtcNow, Subject = "S",
                    InteractionContacts = new List<InteractionContact> { new() { ContactId = 42 }, new() { ContactId = 10 } } },
            };
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }
    }

    #endregion

    #region InteractionWithContactSpecification Tests

    public class InteractionWithContactSpecificationTests
    {
        // --- Positive ---
        [Fact]
        public void Constructor_Default_MatchesAllInteractions()
        {
            var spec = new InteractionWithContactSpecification();
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Constructor_WithId_DoesNotMatchDifferentId()
        {
            var spec = new InteractionWithContactSpecification(42);
            var interaction = new Interaction
            {
                Id = 99, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithId_DoesNotMatchZeroId()
        {
            var spec = new InteractionWithContactSpecification(1);
            var interaction = new Interaction
            {
                Id = 0, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithId_DoesNotMatchNegativeId()
        {
            var spec = new InteractionWithContactSpecification(5);
            var interaction = new Interaction
            {
                Id = -1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Constructor_WithId_MatchesExactId()
        {
            var spec = new InteractionWithContactSpecification(42);
            var interaction = new Interaction
            {
                Id = 42, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [Fact]
        public void Constructor_Default_MatchesAnyInteraction()
        {
            var spec = new InteractionWithContactSpecification();
            var interactions = Enumerable.Range(1, 10).Select(i => new Interaction
            {
                Id = i, Name = $"Test {i}", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            });
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(10);
        }

        [Fact]
        public void Constructor_IncludesInteractionContacts()
        {
            var spec = new InteractionWithContactSpecification();
            spec.IncludeStrings.Should().NotBeEmpty();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionWithContactSpecification();
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithId_FiltersToSingleResult()
        {
            var spec = new InteractionWithContactSpecification(5);
            var interactions = Enumerable.Range(1, 10).Select(i => new Interaction
            {
                Id = i, Name = $"Test {i}", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            });
            interactions.Where(spec.Criteria.Compile()).Should().ContainSingle();
        }

        [Fact]
        public void Constructor_IncludesContactNavigation()
        {
            var spec = new InteractionWithContactSpecification();
            var hasContactInclude = spec.IncludeStrings.Any(s => s.Contains("Contact"));
            hasContactInclude.Should().BeTrue();
        }
    }

    #endregion

    #region PagedInteractionSpecification Tests

    public class PagedInteractionSpecificationTests
    {
        // --- Positive ---
        [Fact]
        public void Constructor_SetsCorrectPaging()
        {
            var spec = new PagedInteractionSpecification(1, 10);
            spec.IsPagingEnabled.Should().BeTrue();
            spec.Skip.Should().Be(0);
            spec.Take.Should().Be(10);
        }

        // --- Negative ---
        [Fact]
        public void Constructor_NegativePageIndexDefaultsToPage1()
        {
            var spec = new PagedInteractionSpecification(-1, 10);
            spec.Skip.Should().Be(0);
        }

        [Fact]
        public void Constructor_ZeroPageIndexDefaultsToPage1()
        {
            var spec = new PagedInteractionSpecification(0, 10);
            spec.Skip.Should().Be(0);
        }

        [Fact]
        public void Constructor_NegativePageSizeDefaultsTo10()
        {
            var spec = new PagedInteractionSpecification(1, -5);
            spec.Take.Should().Be(10);
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Constructor_Page2Skips10()
        {
            var spec = new PagedInteractionSpecification(2, 10);
            spec.Skip.Should().Be(10);
        }

        [Fact]
        public void Constructor_LargePageIndex()
        {
            var spec = new PagedInteractionSpecification(100, 50);
            spec.Skip.Should().Be(99 * 50);
        }

        [Fact]
        public void Constructor_ZeroPageSizeDefaultsTo10()
        {
            var spec = new PagedInteractionSpecification(1, 0);
            spec.Take.Should().Be(10);
        }

        // --- Functional ---
        [Fact]
        public void Criteria_MatchesAllInteractions()
        {
            var spec = new PagedInteractionSpecification(1, 10);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [Fact]

        [Trait("Defect", "DEF-079")]
        public void Constructor_SetsOrderBy()
        {
            var spec = new PagedInteractionSpecification(1, 10);
            spec.OrderBy.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_EnablesPaging()
        {
            var spec = new PagedInteractionSpecification(1, 10);
            spec.IsPagingEnabled.Should().BeTrue();
        }
    }

    #endregion

    #region InteractionByOrgUnitHierarchySpecification Tests

    public class InteractionByOrgUnitHierarchySpecificationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public InteractionByOrgUnitHierarchySpecificationTests()
        {
            _dbContext = TestDbContextFactory.Create();
            if (TestEnvironment.UsePostgreSQL)
                _transaction = _dbContext.Database.BeginTransaction();
        }

        // --- Positive ---
        [Fact]
        public void Constructor_WithValidIds_CreatesSpecification()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { 1, 2 });
            spec.Should().NotBeNull();
            spec.Criteria.Should().NotBeNull();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithEmptyList_MatchesNothing()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int>());
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithNullList_MatchesNothing()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(null);
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_NonMatchingOrgUnit_ReturnsEmpty()
        {
            var ou = await CreateOrgHierarchyAsync("OU_NM", "NonMatch");
            await CreateInteractionWithOrgUnitAsync("Test", ou.Id);

            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { ou.Id + 999 });
            var query = _dbContext.Interactions.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_WithPopulatedList_MatchesAll()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { 1 });
            var interaction = new Interaction
            {
                Id = 1, Name = "Test", Type = InteractionType.Email,
                Date = DateTime.UtcNow, Subject = "Subject"
            };
            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_EmptyList_ReturnsNoResults()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int>());
            var query = _dbContext.Interactions.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_MultipleOrgUnits_ReturnsAllMatching()
        {
            var ou1 = await CreateOrgHierarchyAsync("OU_M1", "Multi1");
            var ou2 = await CreateOrgHierarchyAsync("OU_M2", "Multi2");
            var ou3 = await CreateOrgHierarchyAsync("OU_M3", "Multi3");

            var i1 = await CreateInteractionWithOrgUnitAsync("I1", ou1.Id);
            var i2 = await CreateInteractionWithOrgUnitAsync("I2", ou2.Id);
            await CreateInteractionWithOrgUnitAsync("I3", ou3.Id);

            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { ou1.Id, ou2.Id });
            var query = _dbContext.Interactions.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().HaveCount(2);
            results.Select(i => i.Id).Should().BeEquivalentTo(new[] { i1.Id, i2.Id });
        }

        // --- Functional ---
        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_UsesInteractionEntityType()
        {
            var ou = await CreateOrgHierarchyAsync("OU_ET", "EntityType");
            var interaction = await CreateInteractionWithOrgUnitAsync("Test", ou.Id);

            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = "Partner-OrgUnit",
                OrganizationHierarchyId = ou.Id,
                EntityId = 999,
                EntityType = "Partner",
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();

            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { ou.Id });
            var query = _dbContext.Interactions.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            results.Should().ContainSingle().Which.Id.Should().Be(interaction.Id);
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Should().NotBeNull();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_SingleOrgUnit_ReturnsSingleMatch()
        {
            var ou = await CreateOrgHierarchyAsync("OU_S", "Single");
            var matched = await CreateInteractionWithOrgUnitAsync("Matched", ou.Id);

            var spec = new InteractionByOrgUnitHierarchySpecification(new List<int> { ou.Id });
            var query = _dbContext.Interactions.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().ContainSingle().Which.Id.Should().Be(matched.Id);
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                try { _transaction.Rollback(); } catch { }
                _transaction.Dispose();
            }
            _dbContext?.Dispose();
        }

        private async Task<OrganizationHierarchy> CreateOrgHierarchyAsync(string code, string name)
        {
            var org = new OrganizationHierarchy
            {
                Code = code, Name = name,
                Type = OrganizationUnitType.OrgUnit,
                Description = name,
                Status = EntityStatus.Active
            };
            await _dbContext.Set<OrganizationHierarchy>().AddAsync(org);
            await _dbContext.SaveChangesAsync();
            return org;
        }

        private async Task<Interaction> CreateInteractionWithOrgUnitAsync(string name, int orgUnitId)
        {
            var interaction = new Interaction
            {
                Name = name,
                Type = InteractionType.Email,
                Date = DateTime.UtcNow,
                Subject = $"{name} subject",
                Description = $"{name} description"
            };
            await _dbContext.Interactions.AddAsync(interaction);
            await _dbContext.SaveChangesAsync();

            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = $"I-{interaction.Id}-OU-{orgUnitId}",
                OrganizationHierarchyId = orgUnitId,
                EntityId = interaction.Id,
                EntityType = "Interaction",
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();
            return interaction;
        }
    }

    #endregion
}
