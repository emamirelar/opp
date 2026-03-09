using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Shared;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    #region PartnerCompositeSpecification Tests

    public class PartnerCompositeSpecificationTests
    {
        private static PartnerFilterRequest CreateFilter(Action<PartnerFilterRequest>? configure = null)
        {
            var filter = new PartnerFilterRequest();
            configure?.Invoke(filter);
            return filter;
        }

        private static Partner CreatePartner(
            string name = "UNOPS Foundation",
            string? shortDesc = "UNOPS",
            EntityStatus status = EntityStatus.Active,
            bool canCreate = true) =>
            new()
            {
                Id = 1,
                Name = name,
                PartnerShortDescription = shortDesc,
                PartnerLongDescription = "Long description",
                Status = status,
                CanCreateNewOpportunities = canCreate,
                PooledFund = false,
                UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply,
                CreatedDate = DateTime.UtcNow
            };

        // --- Positive ---
        [Fact]
        public void Constructor_WithEmptyFilter_MatchesAll()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter());
            spec.Criteria.Compile()(CreatePartner()).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithNameFilter_DoesNotMatchDifferentName()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.Name = "UNICEF"));
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeFalse();
        }

        [Fact]

        [Trait("Defect", "DEF-077")]
        public void Criteria_WithStatusFilter_DoesNotMatchDifferentStatus()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.Status = "Inactive"));
            spec.Criteria.Compile()(CreatePartner(status: EntityStatus.Active)).Should().BeFalse();
        }

        [Fact]

        [Trait("Defect", "DEF-077")]
        public void Criteria_WithNewEngagementYes_DoesNotMatchNonEngaged()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.NewEngagement = "yes"));
            spec.Criteria.Compile()(CreatePartner(canCreate: false)).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_WithSearchText_MatchesPartnerName()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.SearchText = "UNOPS"));
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithSearchText_DoesNotMatchUnrelatedPartner()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.SearchText = "xyz_nonexistent"));
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeFalse();
        }

        [Fact]

        [Trait("Defect", "DEF-076")]
        public void Criteria_NullFilter_MatchesAll()
        {
            var spec = new PartnerCompositeSpecification(null!);
            spec.Criteria.Compile()(CreatePartner()).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_SetsDefaultAscendingOrder()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter());
            spec.OrderBy.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithDescendingOrder_SetsOrderByDescending()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f =>
            {
                f.OrderBy = "name";
                f.Ascending = false;
            }));
            spec.OrderByDescending.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithOrderByName_SetsOrderExpression()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.OrderBy = "name"));
            spec.OrderBy.Should().NotBeNull();
        }

        // --- Integration ---
        [Fact]
        [Trait("Defect", "DEF-077")]
        public void Criteria_WithMultipleFilters_CombinesWithAnd()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f =>
            {
                f.Name = "UNOPS";
                f.Status = "Active";
            }));
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation", status: EntityStatus.Active)).Should().BeTrue();
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation", status: EntityStatus.Inactive)).Should().BeFalse();
        }

        [Fact]
        public void Criteria_FiltersCollectionCorrectly()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f => f.Name = "Target"));
            var partners = new[]
            {
                CreatePartner("Target Corp"),
                CreatePartner("Other Corp"),
                CreatePartner("My Target Inc"),
            };
            partners.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_AdvancedSearch_WithJsonCriteria_BuildsExpression()
        {
            var spec = new PartnerCompositeSpecification(CreateFilter(f =>
            {
                f.AdvancedSearch = true;
                f.SearchCriteria = @"[{""field"":""name"",""value"":""UNOPS"",""operator"":""like"",""logicalOperator"":""AND""}]";
            }));
            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeTrue();
        }
    }

    #endregion

    #region PartnerCompositeClassicSearchSpecification Tests

    public class PartnerCompositeClassicSearchSpecificationTests
    {
        private static Partner CreatePartner(
            string name = "UNOPS Foundation",
            string? shortDesc = "UNOPS Short",
            EntityStatus status = EntityStatus.Active,
            bool canCreate = true) =>
            new()
            {
                Id = 1, Name = name,
                PartnerShortDescription = shortDesc,
                PartnerLongDescription = "Long desc",
                Status = status,
                CanCreateNewOpportunities = canCreate,
                PooledFund = false, UNAndStateEntity = false,
                DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
            };

        // --- Positive ---
        [Fact]
        public void Constructor_NoFilters_MatchesAll()
        {
            var spec = new PartnerCompositeClassicSearchSpecification();
            spec.Criteria.Compile()(CreatePartner()).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithName_DoesNotMatchDifferent()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(name: "UNICEF");
            spec.Criteria.Compile()(CreatePartner("UNOPS")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithId_DoesNotMatchDifferent()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(id: 99);
            spec.Criteria.Compile()(CreatePartner()).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithSearchText_DoesNotMatchUnrelated()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(searchText: "xyz_absent");
            spec.Criteria.Compile()(CreatePartner("UNOPS")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_WithNameFilter_IsCaseInsensitive()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(name: "unops");
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithShortName_MatchesPartialShortDesc()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(shortName: "short");
            spec.Criteria.Compile()(CreatePartner(shortDesc: "UNOPS Short")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithStatus_MatchesExact()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(status: "Active");
            spec.Criteria.Compile()(CreatePartner(status: EntityStatus.Active)).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Criteria_CombinesMultipleFilters()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(
                name: "UNOPS", status: "Active");
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation", status: EntityStatus.Active)).Should().BeTrue();
            spec.Criteria.Compile()(CreatePartner("UNOPS Foundation", status: EntityStatus.Inactive)).Should().BeFalse();
        }

        [Fact]
        public void Constructor_SetsOrderByName()
        {
            var spec = new PartnerCompositeClassicSearchSpecification();
            spec.OrderBy.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_SearchText_MatchesNameOrShortDescOrLongDesc()
        {
            var spec = new PartnerCompositeClassicSearchSpecification(searchText: "Long");
            spec.Criteria.Compile()(CreatePartner("Other Name", shortDesc: "Short")).Should().BeTrue();
        }
    }

    #endregion

    #region ContactCompositeSpecification Tests

    public class ContactCompositeSpecificationTests
    {
        private static ContactFilterRequest CreateFilter(Action<ContactFilterRequest>? configure = null)
        {
            var filter = new ContactFilterRequest();
            configure?.Invoke(filter);
            return filter;
        }

        private static Contact CreateContact(
            string firstName = "John",
            string lastName = "Doe",
            string email = "john.doe@example.com",
            string? title = "Manager",
            string? department = "Engineering") =>
            new()
            {
                Id = 1, FirstName = firstName, LastName = lastName,
                Name = $"{firstName} {lastName}",
                Email = email, Title = title, Department = department,
                Status = EntityStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

        // --- Positive ---
        [Fact]
        public void Constructor_WithEmptyFilter_MatchesAll()
        {
            var spec = new ContactCompositeSpecification(CreateFilter());
            spec.Criteria.Compile()(CreateContact()).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithFirstName_DoesNotMatchDifferent()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.FirstName = "Jane"));
            spec.Criteria.Compile()(CreateContact("John")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithEmail_DoesNotMatchDifferent()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.Email = "other@example.com"));
            spec.Criteria.Compile()(CreateContact(email: "john@example.com")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithTitle_DoesNotMatchDifferent()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.Title = "Director"));
            spec.Criteria.Compile()(CreateContact(title: "Manager")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        [Trait("Defect", "DEF-076")]
        public void Criteria_NullFilter_MatchesAll()
        {
            var spec = new ContactCompositeSpecification(null!);
            spec.Criteria.Compile()(CreateContact()).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithSearchText_MatchesFirstName()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.SearchText = "John"));
            spec.Criteria.Compile()(CreateContact("John")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithSearchText_MatchesEmail()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.SearchText = "doe@example"));
            spec.Criteria.Compile()(CreateContact(email: "john.doe@example.com")).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_IncludesPartner()
        {
            var spec = new ContactCompositeSpecification(CreateFilter());
            spec.Includes.Should().NotBeEmpty();
        }

        [Fact]
        public void Constructor_SetsDefaultOrdering()
        {
            var spec = new ContactCompositeSpecification(CreateFilter());
            spec.OrderBy.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithDescending_SetsOrderByDescending()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f =>
            {
                f.OrderBy = "lastname";
                f.Ascending = false;
            }));
            spec.OrderByDescending.Should().NotBeNull();
        }

        // --- Integration ---
        [Fact]
        public void Criteria_AdvancedSearch_WithJsonCriteria()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f =>
            {
                f.AdvancedSearch = true;
                f.SearchCriteria = @"[{""field"":""firstName"",""value"":""John"",""operator"":""like"",""logicalOperator"":""AND""}]";
            }));
            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Compile()(CreateContact("John")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_FiltersCollection()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f => f.FirstName = "John"));
            var contacts = new[]
            {
                CreateContact("John", "Doe"),
                CreateContact("Jane", "Doe"),
                CreateContact("John", "Smith"),
            };
            contacts.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_MultipleFilters_CombineWithAnd()
        {
            var spec = new ContactCompositeSpecification(CreateFilter(f =>
            {
                f.FirstName = "John";
                f.LastName = "Doe";
            }));
            spec.Criteria.Compile()(CreateContact("John", "Doe")).Should().BeTrue();
            spec.Criteria.Compile()(CreateContact("John", "Smith")).Should().BeFalse();
        }
    }

    #endregion

    #region InteractionCompositeSpecification Tests

    public class InteractionCompositeSpecificationTests
    {
        private static InteractionFilterRequest CreateFilter(Action<InteractionFilterRequest>? configure = null)
        {
            var filter = new InteractionFilterRequest();
            configure?.Invoke(filter);
            return filter;
        }

        private static Interaction CreateInteraction(
            string description = "Team meeting",
            InteractionType type = InteractionType.Email,
            string subject = "Weekly Sync") =>
            new()
            {
                Id = 1, Name = "Test Interaction",
                Type = type, Date = DateTime.UtcNow,
                Subject = subject, Description = description,
                InteractionContacts = new List<InteractionContact>(),
                InteractionPartners = new List<InteractionPartner>(),
                InteractionUsers = new List<InteractionUser>()
            };

        // --- Positive ---
        [Fact]
        public void Constructor_WithEmptyFilter_MatchesAll()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter());
            spec.Criteria.Compile()(CreateInteraction()).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        public void Criteria_WithDescription_DoesNotMatchDifferent()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.Description = "budget"));
            spec.Criteria.Compile()(CreateInteraction("team meeting")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithSubject_DoesNotMatchDifferent()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.Subject = "Quarterly"));
            spec.Criteria.Compile()(CreateInteraction(subject: "Weekly Sync")).Should().BeFalse();
        }

        [Fact]
        public void Criteria_WithSearchText_DoesNotMatchUnrelated()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.SearchText = "xyz_absent"));
            spec.Criteria.Compile()(CreateInteraction("Regular meeting")).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        [Trait("Defect", "DEF-076")]
        public void Criteria_NullFilter_MatchesAll()
        {
            var spec = new InteractionCompositeSpecification(null!);
            spec.Criteria.Compile()(CreateInteraction()).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithSearchText_MatchesDescription()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.SearchText = "meeting"));
            spec.Criteria.Compile()(CreateInteraction("Team meeting notes")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_WithSearchText_MatchesSubject()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.SearchText = "Weekly"));
            spec.Criteria.Compile()(CreateInteraction(subject: "Weekly Sync")).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_SetsDefaultOrdering()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter());
            (spec.OrderBy != null || spec.OrderByDescending != null).Should().BeTrue();
        }

        [Fact]
        public void Constructor_WithOrderBy_SetsOrdering()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f =>
            {
                f.OrderBy = "date";
                f.Ascending = false;
            }));
            spec.OrderByDescending.Should().NotBeNull();
        }

        [Fact]
        public void Criteria_ExpressionIsNotNull()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter());
            spec.Criteria.Should().NotBeNull();
        }

        // --- Integration ---
        [Fact]
        public void Criteria_AdvancedSearch_WithJsonCriteria()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f =>
            {
                f.AdvancedSearch = true;
                f.SearchCriteria = @"[{""field"":""description"",""value"":""meeting"",""operator"":""like"",""logicalOperator"":""AND""}]";
            }));
            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Compile()(CreateInteraction("Team meeting")).Should().BeTrue();
        }

        [Fact]
        public void Criteria_FiltersCollection()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f => f.Description = "meeting"));
            var interactions = new[]
            {
                CreateInteraction("Team meeting"),
                CreateInteraction("Budget review"),
                CreateInteraction("Project meeting"),
            };
            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }

        [Fact]
        public void Criteria_AdvancedSearch_WithOrOperator()
        {
            var spec = new InteractionCompositeSpecification(CreateFilter(f =>
            {
                f.AdvancedSearch = true;
                f.SearchCriteria = @"[
                    {""field"":""description"",""value"":""meeting"",""operator"":""like"",""logicalOperator"":""AND""},
                    {""field"":""description"",""value"":""review"",""operator"":""like"",""logicalOperator"":""OR""}
                ]";
            }));
            spec.Criteria.Compile()(CreateInteraction("Budget review")).Should().BeTrue();
            spec.Criteria.Compile()(CreateInteraction("Team meeting")).Should().BeTrue();
        }
    }

    #endregion
}
