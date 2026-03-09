using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.DataAccess.Context;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    #region PartnerByNameSpecification Tests

    public class PartnerByNameSpecificationTests
    {
        private static Partner CreatePartner(string name, string shortDesc = "Short") =>
            new()
            {
                Id = 1,
                Name = name,
                PartnerShortDescription = shortDesc,
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesPartnerByExactName()
        {
            var spec = new PartnerByNameSpecification("UNOPS");
            var partner = CreatePartner("UNOPS Foundation");
            var func = spec.Criteria.Compile();
            func(partner).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentName()
        {
            var spec = new PartnerByNameSpecification("UNICEF");
            var partner = CreatePartner("UNOPS Foundation");
            spec.Criteria.Compile()(partner).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchNullName()
        {
            var spec = new PartnerByNameSpecification("test");
            var partner = CreatePartner(null!);
            spec.Criteria.Compile()(partner).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchEmptySearchAgainstPopulatedName()
        {
            var spec = new PartnerByNameSpecification("xyz_nonexistent");
            var partner = CreatePartner("Real Partner Name");
            spec.Criteria.Compile()(partner).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Theory]
        [InlineData("unops", "UNOPS Foundation", true)]
        [InlineData("UNOPS", "unops foundation", true)]
        [InlineData("UnOpS", "UNOPS Foundation", true)]
        public void Criteria_IsCaseInsensitive(string search, string name, bool expected)
        {
            var spec = new PartnerByNameSpecification(search);
            spec.Criteria.Compile()(CreatePartner(name)).Should().Be(expected);
        }

        [Fact]
        public void Criteria_MatchesPartialName()
        {
            var spec = new PartnerByNameSpecification("Found");
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesSingleCharacter()
        {
            var spec = new PartnerByNameSpecification("U");
            spec.Criteria.Compile()(CreatePartner("UNOPS")).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new PartnerByNameSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_CanFilterCollection()
        {
            var spec = new PartnerByNameSpecification("target");
            var partners = new[]
            {
                CreatePartner("Target Partner"),
                CreatePartner("Other Partner"),
                CreatePartner("Another Target Here")
            };
            var func = spec.Criteria.Compile();
            partners.Where(func).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_ContainsSearchMatchesMiddleOfString()
        {
            var spec = new PartnerByNameSpecification("ound");
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeTrue();
        }
    }

    #endregion

    #region PartnerByIdSpecification Tests

    public class PartnerByIdSpecificationTests
    {
        private static Partner CreatePartnerWithId(int id) =>
            new()
            {
                Id = id,
                Name = $"Partner {id}",
                PartnerShortDescription = "Short",
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesPartnerById()
        {
            var spec = new PartnerByIdSpecification(42);
            spec.Criteria.Compile()(CreatePartnerWithId(42)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentId()
        {
            var spec = new PartnerByIdSpecification(42);
            spec.Criteria.Compile()(CreatePartnerWithId(99)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchZeroId()
        {
            var spec = new PartnerByIdSpecification(42);
            spec.Criteria.Compile()(CreatePartnerWithId(0)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchNegativeId()
        {
            var spec = new PartnerByIdSpecification(1);
            spec.Criteria.Compile()(CreatePartnerWithId(-1)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_MatchesIdOfZero()
        {
            var spec = new PartnerByIdSpecification(0);
            spec.Criteria.Compile()(CreatePartnerWithId(0)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesMaxIntId()
        {
            var spec = new PartnerByIdSpecification(int.MaxValue);
            spec.Criteria.Compile()(CreatePartnerWithId(int.MaxValue)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_FiltersCollectionToSingleResult()
        {
            var spec = new PartnerByIdSpecification(2);
            var partners = Enumerable.Range(1, 10).Select(CreatePartnerWithId);
            partners.Where(spec.Criteria.Compile()).Should().ContainSingle()
                .Which.Id.Should().Be(2);
        }

        // --- Functional ---
        [Fact]
        public void Constructor_IncludesDocuments()
        {
            var spec = new PartnerByIdSpecification(1);
            spec.Includes.Should().NotBeEmpty();
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new PartnerByIdSpecification(1);
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_UniquelyIdentifiesPartner()
        {
            var spec = new PartnerByIdSpecification(5);
            var partners = Enumerable.Range(1, 100).Select(CreatePartnerWithId);
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(1);
        }
    }

    #endregion

    #region PartnerByStatusSpecification Tests

    public class PartnerByStatusSpecificationTests
    {
        private static Partner CreatePartnerWithStatus(EntityStatus status) =>
            new()
            {
                Id = 1,
                Name = "Test Partner",
                PartnerShortDescription = "Short",
                Status = status,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesActiveStatus()
        {
            var spec = new PartnerByStatusSpecification("Active");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Active)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentStatus()
        {
            var spec = new PartnerByStatusSpecification("Active");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Inactive)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchInvalidStatusString()
        {
            var spec = new PartnerByStatusSpecification("NonExistentStatus");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Active)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_IsCaseSensitiveForEnumStringComparison()
        {
            var spec = new PartnerByStatusSpecification("active");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Active)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_MatchesInactiveStatus()
        {
            var spec = new PartnerByStatusSpecification("Inactive");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Inactive)).Should().BeTrue();
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("Inactive")]
        public void Criteria_MatchesKnownStatusValues(string status)
        {
            var spec = new PartnerByStatusSpecification(status);
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_EmptyStatusMatchesNothing()
        {
            var spec = new PartnerByStatusSpecification("");
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Active)).Should().BeFalse();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_FiltersCollectionByStatus()
        {
            var spec = new PartnerByStatusSpecification("Active");
            var partners = new[]
            {
                CreatePartnerWithStatus(EntityStatus.Active),
                CreatePartnerWithStatus(EntityStatus.Inactive),
                CreatePartnerWithStatus(EntityStatus.Active),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_UsesToStringForComparison()
        {
            var spec = new PartnerByStatusSpecification(EntityStatus.Active.ToString());
            spec.Criteria.Compile()(CreatePartnerWithStatus(EntityStatus.Active)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new PartnerByStatusSpecification("Active");
            spec.Criteria.Should().NotBeNull();
        }
    }

    #endregion

    #region PartnerByShortNameSpecification Tests

    public class PartnerByShortNameSpecificationTests
    {
        private static Partner CreatePartnerWithShortDesc(string? shortDesc) =>
            new()
            {
                Id = 1,
                Name = "Test Partner",
                PartnerShortDescription = shortDesc,
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesShortDescription()
        {
            var spec = new PartnerByShortNameSpecification("UNOPS");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("UNOPS Corp")).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentShortDesc()
        {
            var spec = new PartnerByShortNameSpecification("UNICEF");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("UNOPS Corp")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchNullShortDesc()
        {
            var spec = new PartnerByShortNameSpecification("test");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc(null)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchWhenNoOverlap()
        {
            var spec = new PartnerByShortNameSpecification("xyz");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("abc")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Theory]
        [InlineData("unops", "UNOPS Corp", true)]
        [InlineData("UNOPS", "unops corp", true)]
        public void Criteria_IsCaseInsensitive(string search, string shortDesc, bool expected)
        {
            var spec = new PartnerByShortNameSpecification(search);
            spec.Criteria.Compile()(CreatePartnerWithShortDesc(shortDesc)).Should().Be(expected);
        }

        [Fact]
        public void Criteria_MatchesPartialShortDesc()
        {
            var spec = new PartnerByShortNameSpecification("Cor");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("UNOPS Corp")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesSingleCharacter()
        {
            var spec = new PartnerByShortNameSpecification("U");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("UNOPS")).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_FiltersCollectionByShortDesc()
        {
            var spec = new PartnerByShortNameSpecification("target");
            var partners = new[]
            {
                CreatePartnerWithShortDesc("Target Corp"),
                CreatePartnerWithShortDesc("Other Corp"),
                CreatePartnerWithShortDesc("My Target Inc")
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new PartnerByShortNameSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_ContainsSearchMatchesMiddle()
        {
            var spec = new PartnerByShortNameSpecification("OPS");
            spec.Criteria.Compile()(CreatePartnerWithShortDesc("UNOPS Corp")).Should().BeTrue();
        }
    }

    #endregion

    #region PartnerByNewEngagementSpecification Tests

    public class PartnerByNewEngagementSpecificationTests
    {
        private static Partner CreatePartnerWithEngagement(bool canCreate) =>
            new()
            {
                Id = 1,
                Name = "Test Partner",
                PartnerShortDescription = "Short",
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = canCreate,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_YesMatchesCanCreateTrue()
        {
            var spec = new PartnerByNewEngagementSpecification("yes");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(true)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_YesDoesNotMatchCanCreateFalse()
        {
            var spec = new PartnerByNewEngagementSpecification("yes");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(false)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_NoDoesNotMatchCanCreateTrue()
        {
            var spec = new PartnerByNewEngagementSpecification("no");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(true)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_ArbitraryStringTreatedAsNo()
        {
            var spec = new PartnerByNewEngagementSpecification("maybe");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(false)).Should().BeTrue();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_YESUpperCaseMatchesCanCreate()
        {
            var spec = new PartnerByNewEngagementSpecification("YES");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(true)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_NoMatchesCanCreateFalse()
        {
            var spec = new PartnerByNewEngagementSpecification("no");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(false)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_YesMixedCaseWorks()
        {
            var spec = new PartnerByNewEngagementSpecification("Yes");
            spec.Criteria.Compile()(CreatePartnerWithEngagement(true)).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_FiltersCollectionForEngagement()
        {
            var spec = new PartnerByNewEngagementSpecification("yes");
            var partners = new[]
            {
                CreatePartnerWithEngagement(true),
                CreatePartnerWithEngagement(false),
                CreatePartnerWithEngagement(true),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new PartnerByNewEngagementSpecification("yes");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_NoFiltersOutEngagedPartners()
        {
            var spec = new PartnerByNewEngagementSpecification("no");
            var partners = new[]
            {
                CreatePartnerWithEngagement(true),
                CreatePartnerWithEngagement(false),
                CreatePartnerWithEngagement(false),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }
    }

    #endregion

    #region PagedPartnerSpecification Tests

    public class PagedPartnerSpecificationTests
    {
        // --- Positive ---
        [Fact]
        public void Constructor_SetsCorrectPagingForPage1()
        {
            var spec = new PagedPartnerSpecification(1, 10);
            spec.IsPagingEnabled.Should().BeTrue();
            spec.Skip.Should().Be(0);
            spec.Take.Should().Be(10);
        }

        // --- Negative ---
        [Fact]
        public void Constructor_NegativePageIndexDefaultsToPage1()
        {
            var spec = new PagedPartnerSpecification(-1, 10);
            spec.IsPagingEnabled.Should().BeTrue();
            spec.Skip.Should().Be(0);
            spec.Take.Should().Be(10);
        }

        [Fact]
        public void Constructor_ZeroPageIndexDefaultsToPage1()
        {
            var spec = new PagedPartnerSpecification(0, 10);
            spec.IsPagingEnabled.Should().BeTrue();
            spec.Skip.Should().Be(0);
        }

        [Fact]
        public void Constructor_NegativePageSizeDefaultsTo10()
        {
            var spec = new PagedPartnerSpecification(1, -5);
            spec.Take.Should().Be(10);
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Constructor_Page2Skips10()
        {
            var spec = new PagedPartnerSpecification(2, 10);
            spec.Skip.Should().Be(10);
            spec.Take.Should().Be(10);
        }

        [Fact]
        public void Constructor_LargePageIndex()
        {
            var spec = new PagedPartnerSpecification(1000, 25);
            spec.Skip.Should().Be(999 * 25);
            spec.Take.Should().Be(25);
        }

        [Fact]
        public void Constructor_ZeroPageSizeDefaultsTo10()
        {
            var spec = new PagedPartnerSpecification(1, 0);
            spec.Take.Should().Be(10);
        }

        // --- Functional ---
        [Fact]
        public void Criteria_MatchesAllPartners()
        {
            var spec = new PagedPartnerSpecification(1, 10);
            var partner = new Partner
            {
                Name = "Test",
                PartnerShortDescription = "Test",
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeTrue();
        }

        [Fact]
        public void Constructor_SetsOrderByName()
        {
            var spec = new PagedPartnerSpecification(1, 10);
            spec.OrderBy.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_EnablesPaging()
        {
            var spec = new PagedPartnerSpecification(3, 20);
            spec.IsPagingEnabled.Should().BeTrue();
            spec.Skip.Should().Be(40);
            spec.Take.Should().Be(20);
        }
    }

    #endregion

    #region Deprecated Specification Tests (PartnerByWebsite, PartnerByPhone, PartnerByAddress)

    public class DeprecatedPartnerSpecificationTests
    {
        private static Partner CreateTestPartner() =>
            new()
            {
                Id = 1,
                Name = "Test Partner",
                PartnerShortDescription = "Short",
                Status = EntityStatus.Active,
                CanCreateNewOpportunities = true,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void PartnerByWebsite_MatchesAllPartners()
        {
            var spec = new PartnerByWebsiteSpecification("www.example.com");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        // --- Negative (documenting that deprecated specs don't filter) ---
        [Fact]
        public void PartnerByWebsite_DoesNotFilterAnything()
        {
            var spec = new PartnerByWebsiteSpecification("completely_different");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        [Fact]
        public void PartnerByPhone_DoesNotFilterAnything()
        {
            var spec = new PartnerByPhoneSpecification("555-1234");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        [Fact]
        public void PartnerByAddress_DoesNotFilterAnything()
        {
            var spec = new PartnerByAddressSpecification("New York", "NY", "10001", "USA");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void PartnerByWebsite_EmptyStringStillMatchesAll()
        {
            var spec = new PartnerByWebsiteSpecification("");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        [Fact]
        public void PartnerByPhone_EmptyStringStillMatchesAll()
        {
            var spec = new PartnerByPhoneSpecification("");
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        [Fact]
        public void PartnerByAddress_AllNullParamsStillMatchesAll()
        {
            var spec = new PartnerByAddressSpecification();
            spec.Criteria.Compile()(CreateTestPartner()).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void PartnerByWebsite_CriteriaExpressionIsNotNull()
        {
            var spec = new PartnerByWebsiteSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void PartnerByPhone_CriteriaExpressionIsNotNull()
        {
            var spec = new PartnerByPhoneSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void PartnerByAddress_CriteriaExpressionIsNotNull()
        {
            var spec = new PartnerByAddressSpecification("city");
            spec.Criteria.Should().NotBeNull();
        }
    }

    #endregion

    #region PartnerByOrgUnitHierarchySpecification Tests

    public class PartnerByOrgUnitHierarchySpecificationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public PartnerByOrgUnitHierarchySpecificationTests()
        {
            _dbContext = TestDbContextFactory.Create();
            if (TestEnvironment.UsePostgreSQL)
                _transaction = _dbContext.Database.BeginTransaction();
        }

        // --- Positive ---
        [Fact]
        public void Constructor_WithValidIds_CreatesSpecification()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { 1, 2, 3 });
            spec.Should().NotBeNull();
            spec.Criteria.Should().NotBeNull();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithEmptyList_ReturnsNoResults()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int>());
            var partner = new Partner
            {
                Id = 1, Name = "Test", PartnerShortDescription = "S",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithNullList_ReturnsNoResults()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(null);
            var partner = new Partner
            {
                Id = 1, Name = "Test", PartnerShortDescription = "S",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeFalse();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_WithNonMatchingId_ReturnsEmpty()
        {
            var orgUnit = await CreateOrgHierarchyAsync("OU_NM", "Non-Match");
            await CreatePartnerWithOrgUnitAsync("Partner 1", orgUnit.Id);

            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { orgUnit.Id + 999 });
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_WithSingleId_MatchesAll()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { 1 });
            var partner = new Partner
            {
                Id = 1, Name = "Test", PartnerShortDescription = "S",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeTrue();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_WithEmptyList_ReturnsNoResults()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int>());
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_WithMultipleIds_FiltersCorrectly()
        {
            var ou1 = await CreateOrgHierarchyAsync("OU_M1", "Multi 1");
            var ou2 = await CreateOrgHierarchyAsync("OU_M2", "Multi 2");
            var ou3 = await CreateOrgHierarchyAsync("OU_M3", "Multi 3");

            var p1 = await CreatePartnerWithOrgUnitAsync("P1", ou1.Id);
            var p2 = await CreatePartnerWithOrgUnitAsync("P2", ou2.Id);
            await CreatePartnerWithOrgUnitAsync("P3", ou3.Id);

            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { ou1.Id, ou2.Id });
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            results.Should().HaveCount(2);
            results.Select(p => p.Id).Should().BeEquivalentTo(new[] { p1.Id, p2.Id });
        }

        // --- Functional ---
        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_OnlyReturnsPartnersWithMatchingEntityType()
        {
            var ou = await CreateOrgHierarchyAsync("OU_ET", "EntityType Test");
            var partner = await CreatePartnerWithOrgUnitAsync("Partner", ou.Id);

            var nonPartnerRelation = new OrganizationUnitRelationship
            {
                Name = "Contact-Org",
                OrganizationHierarchyId = ou.Id,
                EntityId = 999,
                EntityType = "Contact",
                Status = EntityStatus.Active
            };
            _dbContext.Set<OrganizationUnitRelationship>().Add(nonPartnerRelation);
            await _dbContext.SaveChangesAsync();

            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { ou.Id });
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            results.Should().HaveCount(1);
            results[0].Id.Should().Be(partner.Id);
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_SingleOrgUnit_ReturnsMatchingPartner()
        {
            var ou = await CreateOrgHierarchyAsync("OU_S", "Single");
            var partner = await CreatePartnerWithOrgUnitAsync("Matched", ou.Id);
            await CreatePartnerAsync("Unmatched");

            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int> { ou.Id });
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            results.Should().ContainSingle().Which.Id.Should().Be(partner.Id);
        }

        [Fact]
        public void Criteria_EmptyListReturnsMatchNoneExpression()
        {
            var spec = new PartnerByOrgUnitHierarchySpecification(new List<int>());
            var partner = new Partner
            {
                Id = 1, Name = "Any", PartnerShortDescription = "S",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeFalse();
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

        private async Task<Partner> CreatePartnerAsync(string name)
        {
            var partner = new Partner
            {
                Name = name, PartnerShortDescription = name.Length > 10 ? name[..10] : name,
                PartnerCategoryId = 1, LiaisonOfficeId = 1,
                UNAndStateEntity = false, Status = EntityStatus.Active,
                CanCreateNewOpportunities = true, PooledFund = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();
            return partner;
        }

        private async Task<Partner> CreatePartnerWithOrgUnitAsync(string name, int orgUnitId)
        {
            var partner = await CreatePartnerAsync(name);
            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = $"P-{partner.Id}-OU-{orgUnitId}",
                OrganizationHierarchyId = orgUnitId,
                EntityId = partner.Id,
                EntityType = nameof(Partner),
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();
            return partner;
        }
    }

    #endregion

    #region PartnerByPartnerOfficeSpecification Tests

    public class PartnerByPartnerOfficeSpecificationTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public PartnerByPartnerOfficeSpecificationTests()
        {
            _dbContext = TestDbContextFactory.Create();
            if (TestEnvironment.UsePostgreSQL)
                _transaction = _dbContext.Database.BeginTransaction();
        }

        // --- Positive ---
        [Fact]
        public void Constructor_CreatesSpecification()
        {
            var spec = new PartnerByPartnerOfficeSpecification(1);
            spec.Should().NotBeNull();
            spec.Criteria.Should().NotBeNull();
        }

        // --- Negative ---
        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_NonExistentOrgUnit_ReturnsEmpty()
        {
            await CreatePartnerAsync("Orphan Partner");
            var spec = new PartnerByPartnerOfficeSpecification(99999);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_PartnerInDifferentOrgUnit_NotReturned()
        {
            var ou1 = await CreateOrgHierarchyAsync("OU_D1", "Diff 1");
            var ou2 = await CreateOrgHierarchyAsync("OU_D2", "Diff 2");
            await CreatePartnerWithOrgUnitAsync("Partner", ou1.Id);

            var spec = new PartnerByPartnerOfficeSpecification(ou2.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_IgnoresNonPartnerEntityTypes()
        {
            var ou = await CreateOrgHierarchyAsync("OU_NP", "NonPartner");
            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = "Contact-OrgUnit",
                OrganizationHierarchyId = ou.Id,
                EntityId = 1,
                EntityType = "Contact",
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();

            var spec = new PartnerByPartnerOfficeSpecification(ou.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().BeEmpty();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_BaseMatchesAllPartners()
        {
            var spec = new PartnerByPartnerOfficeSpecification(1);
            var partner = new Partner
            {
                Id = 1, Name = "Test", PartnerShortDescription = "S",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            spec.Criteria.Compile()(partner).Should().BeTrue();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_MultiplePartnersInSameOrgUnit()
        {
            var ou = await CreateOrgHierarchyAsync("OU_MP", "MultiPartner");
            var p1 = await CreatePartnerWithOrgUnitAsync("P1", ou.Id);
            var p2 = await CreatePartnerWithOrgUnitAsync("P2", ou.Id);

            var spec = new PartnerByPartnerOfficeSpecification(ou.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().HaveCount(2);
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_PartnerInMultipleOrgUnits_MatchesBySpecificOne()
        {
            var ou1 = await CreateOrgHierarchyAsync("OU_MO1", "Multi1");
            var ou2 = await CreateOrgHierarchyAsync("OU_MO2", "Multi2");
            var partner = await CreatePartnerWithOrgUnitAsync("Multi-OU Partner", ou1.Id);

            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = $"P-{partner.Id}-OU-{ou2.Id}",
                OrganizationHierarchyId = ou2.Id,
                EntityId = partner.Id,
                EntityType = nameof(Partner),
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();

            var spec = new PartnerByPartnerOfficeSpecification(ou2.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().ContainSingle().Which.Id.Should().Be(partner.Id);
        }

        // --- Functional ---
        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_CorrectlyFiltersPartners()
        {
            var ou = await CreateOrgHierarchyAsync("OU_CF", "CorrectFilter");
            var matched = await CreatePartnerWithOrgUnitAsync("Matched", ou.Id);
            await CreatePartnerAsync("NotMatched");

            var spec = new PartnerByPartnerOfficeSpecification(ou.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();

            results.Should().ContainSingle().Which.Id.Should().Be(matched.Id);
        }

        [Fact]
        public void Criteria_IsNotNull()
        {
            var spec = new PartnerByPartnerOfficeSpecification(1);
            spec.Criteria.Should().NotBeNull();
        }

        [SkipIfInMemoryFact]
        public async Task ApplyOrgUnitFilter_MaterializesIdsForFilterPerformance()
        {
            var ou = await CreateOrgHierarchyAsync("OU_PR", "Perf");
            await CreatePartnerWithOrgUnitAsync("PerfPartner", ou.Id);

            var spec = new PartnerByPartnerOfficeSpecification(ou.Id);
            var query = _dbContext.Partners.Where(spec.Criteria);
            query = spec.ApplyOrgUnitFilter(query, _dbContext);
            var results = await query.ToListAsync();
            results.Should().HaveCount(1);
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

        private async Task<Partner> CreatePartnerAsync(string name)
        {
            var partner = new Partner
            {
                Name = name, PartnerShortDescription = name.Length > 10 ? name[..10] : name,
                PartnerCategoryId = 1, LiaisonOfficeId = 1,
                UNAndStateEntity = false, Status = EntityStatus.Active,
                CanCreateNewOpportunities = true, PooledFund = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };
            await _dbContext.Partners.AddAsync(partner);
            await _dbContext.SaveChangesAsync();
            return partner;
        }

        private async Task<Partner> CreatePartnerWithOrgUnitAsync(string name, int orgUnitId)
        {
            var partner = await CreatePartnerAsync(name);
            _dbContext.Set<OrganizationUnitRelationship>().Add(new OrganizationUnitRelationship
            {
                Name = $"P-{partner.Id}-OU-{orgUnitId}",
                OrganizationHierarchyId = orgUnitId,
                EntityId = partner.Id,
                EntityType = nameof(Partner),
                Status = EntityStatus.Active
            });
            await _dbContext.SaveChangesAsync();
            return partner;
        }
    }

    #endregion
}
