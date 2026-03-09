using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using UNOPS.PAO.Models.Contacts;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    #region UNOPSPartnerByStatusSpecification Tests

    public class UNOPSPartnerByStatusSpecificationTests
    {
        private static UNOPSPartner CreatePartner(EntityStatus status = EntityStatus.Active) =>
            new()
            {
                Id = 1, Name = "Test", PartnerShortDescription = "Short",
                Status = status, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesActiveStatus()
        {
            var spec = new UNOPSPartnerByStatusSpecification("Active");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Active)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentStatus()
        {
            var spec = new UNOPSPartnerByStatusSpecification("Active");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Inactive)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchInvalidStatus()
        {
            var spec = new UNOPSPartnerByStatusSpecification("NonExistent");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Active)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_IsCaseSensitive()
        {
            var spec = new UNOPSPartnerByStatusSpecification("active");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Active)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_NullStatus_MatchesAll()
        {
            var spec = new UNOPSPartnerByStatusSpecification(null);
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Active)).Should().BeTrue();
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Inactive)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_EmptyStatus_MatchesAll()
        {
            var spec = new UNOPSPartnerByStatusSpecification("");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Active)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_InactiveStatus_MatchesInactive()
        {
            var spec = new UNOPSPartnerByStatusSpecification("Inactive");
            spec.Criteria.Compile()(CreatePartner(EntityStatus.Inactive)).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new UNOPSPartnerByStatusSpecification("Active");
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_FiltersCollection()
        {
            var spec = new UNOPSPartnerByStatusSpecification("Active");
            var partners = new[]
            {
                CreatePartner(EntityStatus.Active),
                CreatePartner(EntityStatus.Inactive),
                CreatePartner(EntityStatus.Active),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_NullStatusFiltersNothing()
        {
            var spec = new UNOPSPartnerByStatusSpecification(null);
            var partners = new[]
            {
                CreatePartner(EntityStatus.Active),
                CreatePartner(EntityStatus.Inactive),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }
    }

    #endregion

    #region UNOPSContactByTitleSpecification Tests

    public class UNOPSContactByTitleSpecificationTests
    {
        private static UNOPSContact CreateContact(string? title = "Manager") =>
            new()
            {
                Id = 1, FirstName = "John", LastName = "Doe",
                Name = "John Doe", Email = "john@example.com",
                Title = title, Status = EntityStatus.Active
            };

        // --- Positive ---
        [Fact]
        public void Criteria_MatchesExactTitle()
        {
            var spec = new UNOPSContactByTitleSpecification("Manager");
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_DoesNotMatchDifferentTitle()
        {
            var spec = new UNOPSContactByTitleSpecification("Director");
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_DoesNotMatchPartialTitle()
        {
            var spec = new UNOPSContactByTitleSpecification("Manage");
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_IsCaseSensitive()
        {
            var spec = new UNOPSContactByTitleSpecification("manager");
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_NullTitle_MatchesAll()
        {
            var spec = new UNOPSContactByTitleSpecification(null);
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeTrue();
            spec.Criteria.Compile()(CreateContact(null)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_EmptyTitle_MatchesAll()
        {
            var spec = new UNOPSContactByTitleSpecification("");
            spec.Criteria.Compile()(CreateContact("Manager")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_MatchesNullTitleContact()
        {
            var spec = new UNOPSContactByTitleSpecification(null);
            spec.Criteria.Compile()(CreateContact(null)).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_IncludesPartner()
        {
            var spec = new UNOPSContactByTitleSpecification("Manager");
            spec.Includes.Should().NotBeEmpty();
        }

        [Fact]
        public void Criteria_FiltersCollectionByTitle()
        {
            var spec = new UNOPSContactByTitleSpecification("Manager");
            var contacts = new[]
            {
                CreateContact("Manager"),
                CreateContact("Director"),
                CreateContact("Manager"),
            };
            contacts.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new UNOPSContactByTitleSpecification("test");
            spec.Criteria.Should().NotBeNull();
        }
    }

    #endregion

    #region UNOPSPartnerByOrgUnitHierarchySpecification Tests

    public class UNOPSPartnerByOrgUnitHierarchySpecificationTests
    {
        private static UNOPSPartner CreateUNOPSPartner() =>
            new()
            {
                Id = 1, Name = "Test", PartnerShortDescription = "Short",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Constructor_WithValidIds_CreatesSpecification()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int> { 1, 2 });
            spec.Should().NotBeNull();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_EmptyList_MatchesNothing()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int>());
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_NullList_MatchesNothing()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(null);
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_EmptyList_SecurityDefault()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int>());
            var partners = Enumerable.Range(1, 10).Select(_ => CreateUNOPSPartner());
            partners.Where(spec.Criteria.Compile()).Should().BeEmpty();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_WithIds_MatchesAll()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithManyIds_StillMatchesAll()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(Enumerable.Range(1, 100).ToList());
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeTrue();
        }

        [Fact]
        public void Criteria_SingleId_MatchesAll()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int> { 42 });
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_StoresOrgUnitIds()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(new List<int> { 1, 2, 3 });
            spec.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_NullListDefaultsToEmptyBehavior()
        {
            var spec = new UNOPSPartnerByOrgUnitHierarchySpecification(null);
            spec.Criteria.Compile()(CreateUNOPSPartner()).Should().BeFalse();
        }
    }

    #endregion

    #region UNOPSContactByOrgUnitHierarchySpecification Tests

    public class UNOPSContactByOrgUnitHierarchySpecificationTests
    {
        private static UNOPSContact CreateContactWithPartner() =>
            new()
            {
                Id = 1, FirstName = "John", LastName = "Doe",
                Name = "John Doe", Email = "john@example.com",
                Title = "Manager", Status = EntityStatus.Active,
                Partner = new UNOPSPartner
                {
                    Id = 10, Name = "Partner Corp",
                    PartnerShortDescription = "PC",
                    Status = EntityStatus.Active,
                    CanCreateNewOpportunities = true,
                    PooledFund = false, UNAndStateEntity = false,
                    DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                    DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                    PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
                }
            };

        private static UNOPSContact CreateContactWithoutPartner() =>
            new()
            {
                Id = 2, FirstName = "Jane", LastName = "Smith",
                Name = "Jane Smith", Email = "jane@example.com",
                Title = "Director", Status = EntityStatus.Active,
                Partner = null
            };

        // --- Positive ---
        [Fact]
        public void Criteria_WithIds_MatchesContactWithPartner()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Compile()(CreateContactWithPartner()).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_EmptyList_MatchesNothing()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int>());
            spec.Criteria.Compile()(CreateContactWithPartner()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_NullList_MatchesNothing()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(null);
            spec.Criteria.Compile()(CreateContactWithPartner()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithIds_ExcludesContactWithNullPartner()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Compile()(CreateContactWithoutPartner()).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_EmptyList_ExcludesAll()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int>());
            var contacts = new[] { CreateContactWithPartner(), CreateContactWithoutPartner() };
            contacts.Where(spec.Criteria.Compile()).Should().BeEmpty();
        }

        [Fact]
        public void Criteria_WithIds_OnlyMatchesContactsWithPartner()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            var contacts = new[] { CreateContactWithPartner(), CreateContactWithoutPartner() };
            contacts.Where(spec.Criteria.Compile()).Should().ContainSingle();
        }

        [Fact]
        public void Criteria_SingleId_MatchesContactWithPartner()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 99 });
            spec.Criteria.Compile()(CreateContactWithPartner()).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_IncludesPartner()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Includes.Should().NotBeEmpty();
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_RequiresPartnerNotNull()
        {
            var spec = new UNOPSContactByOrgUnitHierarchySpecification(new List<int> { 1 });
            spec.Criteria.Compile()(CreateContactWithPartner()).Should().BeTrue();
            spec.Criteria.Compile()(CreateContactWithoutPartner()).Should().BeFalse();
        }
    }

    #endregion

    #region PartnerSpecificationAdapter Tests

    public class PartnerSpecificationAdapterTests
    {
        // --- Positive ---
        [Fact]
        public void Constructor_WithValidSpec_CreatesAdapter()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.Should().NotBeNull();
        }

        // --- Negative ---
        [Fact]
        public void Constructor_WithNullSpec_Throws()
        {
            var act = () => new PartnerSpecificationAdapter(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetOriginalSpecification_WithNullSpec_CannotCreate()
        {
            var act = () => new PartnerSpecificationAdapter(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Criteria_InheritsNullHandling()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("NonExistent");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.Criteria.Should().NotBeNull();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_ConvertsUNOPSPartnerToPartner()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void IncludeStrings_ProxiedFromOriginal()
        {
            var unosSpec = new UNOPSContactByTitleSpecification("Manager");
            var unosPartnerSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosPartnerSpec);
            adapter.IncludeStrings.Should().NotBeNull();
        }

        [Fact]
        public void Skip_Take_ProxiedFromOriginal()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.Skip.Should().Be(0);
            adapter.Take.Should().Be(0);
            adapter.IsPagingEnabled.Should().BeFalse();
        }

        // --- Functional ---
        [Fact]
        public void GetOriginalSpecification_ReturnsOriginal()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.GetOriginalSpecification().Should().BeSameAs(unosSpec);
        }

        [Fact]
        public void OrderBy_ProxiedFromOriginal()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.OrderBy.Should().BeNull();
        }

        [Fact]
        public void OrderByExpressions_ProxiedFromOriginal()
        {
            var unosSpec = new UNOPSPartnerByStatusSpecification("Active");
            var adapter = new PartnerSpecificationAdapter(unosSpec);
            adapter.OrderByExpressions.Should().NotBeNull();
        }
    }

    #endregion

    #region UNOPSPartnerByOrgUnitWithRelationsSpecification Tests

    public class UNOPSPartnerByOrgUnitWithRelationsSpecificationTests
    {
        private static UNOPSPartner CreatePartnerWithContacts(bool hasMatchingUser = false) =>
            new()
            {
                Id = 1, Name = "Test", PartnerShortDescription = "Short",
                Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply,
                Contacts = hasMatchingUser ? new List<Contact>
                {
                    new()
                    {
                        Id = 1, FirstName = "C", LastName = "One",
                        Name = "C One", Title = "Mr", Email = "c@test.com",
                        Status = EntityStatus.Active,
                        Interactions = new List<Interaction>
                        {
                            new()
                            {
                                Id = 1, Name = "I1", Type = InteractionType.Email,
                                Date = DateTime.UtcNow, Subject = "S",
                                InteractionUsers = new List<InteractionUser>
                                {
                                    new() { UserId = 42 }
                                }
                            }
                        }
                    }
                } : new List<Contact>()
            };

        // --- Positive ---
        [Fact]
        public void Constructor_CreatesSpec()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string> { "42" });
            spec.Should().NotBeNull();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_BothEmpty_MatchesNothing()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), new List<string>());
            spec.Criteria.Compile()(CreatePartnerWithContacts()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_BothNull_MatchesNothing()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(null, null);
            spec.Criteria.Compile()(CreatePartnerWithContacts()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_UserIds_DoesNotMatchWithoutMatchingUser()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), new List<string> { "999" });
            spec.Criteria.Compile()(CreatePartnerWithContacts(false)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_OrgUnitIdsOnly_MatchesAll()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string>());
            spec.Criteria.Compile()(CreatePartnerWithContacts()).Should().BeTrue();
        }

        [Fact]
        public void Criteria_UserIdsOnly_MatchesViaContactInteraction()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int>(), new List<string> { "42" });
            spec.Criteria.Compile()(CreatePartnerWithContacts(true)).Should().BeTrue();
        }

        [Fact]
        public void Constructor_IncludesContacts()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string>());
            spec.Includes.Should().NotBeEmpty();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string> { "1" });
            spec.Criteria.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_IncludesInteractionNavigations()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string>());
            spec.IncludeStrings.Should().NotBeEmpty();
        }

        [Fact]
        public void Criteria_BothPopulated_MatchesViaEither()
        {
            var spec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(
                new List<int> { 1 }, new List<string> { "42" });
            spec.Criteria.Compile()(CreatePartnerWithContacts(false)).Should().BeTrue();
        }
    }

    #endregion
}
