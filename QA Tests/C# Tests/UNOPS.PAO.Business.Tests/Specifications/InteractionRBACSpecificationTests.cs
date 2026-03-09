using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Specifications
{
    #region InteractionRBACCompositeSpecification Tests

    public class InteractionRBACSpecificationTests
    {
        private static ClaimsPrincipal CreateUser(int userId = 1, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new("user_id", userId.ToString()),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static InteractionFilterRequest CreateFilter(Action<InteractionFilterRequest>? configure = null)
        {
            var filter = new InteractionFilterRequest();
            configure?.Invoke(filter);
            return filter;
        }

        private static UNOPSInteraction CreateInteraction(
            int id = 1,
            int createdBy = 1,
            InteractionType type = InteractionType.Email,
            List<InteractionUser>? users = null,
            List<InteractionPartner>? partners = null) =>
            new()
            {
                Id = id,
                Name = $"Interaction {id}",
                Type = type,
                Date = DateTime.UtcNow,
                Subject = "Test Subject",
                Description = "Test Description",
                CreatedBy = createdBy,
                InteractionContacts = new List<InteractionContact>(),
                InteractionPartners = partners ?? new List<InteractionPartner>(),
                InteractionUsers = users ?? new List<InteractionUser>()
            };

        // --- Positive ---
        [Fact]
        public void Constructor_GlobalAdmin_MatchesAll()
        {
            var user = CreateUser(1, "PARTNER_GLOB_ADMIN");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            spec.Criteria.Should().NotBeNull();
            spec.Criteria.Compile()(CreateInteraction(createdBy: 99)).Should().BeTrue();
        }

        // --- Negative ---
        [Fact]
        [Trait("Defect", "DEF-060")]
        public void Criteria_InteractionReadRole_ExcludesOtherUsersInteractions()
        {
            var user = CreateUser(1, "INTERACTION_READ");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var otherUserInteraction = CreateInteraction(createdBy: 99);
            spec.Criteria.Compile()(otherUserInteraction).Should().BeFalse();
        }

        [Fact]
        public void Criteria_NoRole_DoesNotFilterForAdmin()
        {
            var user = CreateUser(1);
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            spec.Criteria.Compile()(CreateInteraction(createdBy: 1)).Should().BeTrue();
        }

        [Fact]

        [Trait("Defect", "DEF-060")]
        public void Criteria_InteractionReadRole_ExcludesUnassignedInteractions()
        {
            var user = CreateUser(5, "INTERACTION_READ");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var interaction = CreateInteraction(
                createdBy: 99,
                users: new List<InteractionUser>
                {
                    new() { UserId = 10 },
                    new() { UserId = 20 }
                });

            spec.Criteria.Compile()(interaction).Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Criteria_InteractionReadRole_MatchesOwnInteraction()
        {
            var user = CreateUser(5, "INTERACTION_READ");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            spec.Criteria.Compile()(CreateInteraction(createdBy: 5)).Should().BeTrue();
        }

        [Fact]
        public void Criteria_InteractionReadRole_MatchesAssignedInteraction()
        {
            var user = CreateUser(5, "INTERACTION_READ");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var interaction = CreateInteraction(
                createdBy: 99,
                users: new List<InteractionUser> { new() { UserId = 5 } });

            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        [Fact]
        public void Criteria_InteractionManagerWithOrgUnit_MatchesWithPartners()
        {
            var user = CreateUser(1, "INTERACTION_MANAGER");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user, "NYRO");

            var interaction = CreateInteraction(
                createdBy: 99,
                partners: new List<InteractionPartner>
                {
                    new() { Partner = new UNOPSPartner
                    {
                        Id = 10, Name = "Test", PartnerShortDescription = "T",
                        Status = EntityStatus.Active, CanCreateNewOpportunities = true,
                        PooledFund = false, UNAndStateEntity = false,
                        DueDiligenceRequired = DueDiligenceRequired.NotRequired,
                        DueDiligenceApproval = DueDiligenceApproval.NotApproved,
                        PartnerLevyStatus = PartnerLevyStatus.DoesNotApply
                    }}
                });

            spec.Criteria.Compile()(interaction).Should().BeTrue();
        }

        // --- Functional ---
        [Fact]
        public void Constructor_SetsDefaultSorting()
        {
            var user = CreateUser(1, "PARTNER_GLOB_ADMIN");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);
            spec.OrderByDescending.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_IncludesContacts()
        {
            var user = CreateUser(1, "PARTNER_GLOB_ADMIN");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);
            spec.Includes.Should().NotBeEmpty();
        }

        [Fact]
        public void Constructor_IncludesPartners()
        {
            var user = CreateUser(1, "PARTNER_GLOB_ADMIN");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);
            spec.IncludeStrings.Should().Contain(s => s.Contains("Partner"));
        }

        // --- Integration ---
        [Fact]
        public void Criteria_GlobalAdmin_NoFiltering_MatchesAll()
        {
            var user = CreateUser(1, "PARTNER_GLOB_ADMIN");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var interactions = new[]
            {
                CreateInteraction(1, 1),
                CreateInteraction(2, 99),
                CreateInteraction(3, 50),
            };

            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(3);
        }

        [Fact]

        [Trait("Defect", "DEF-060")]
        public void Criteria_InteractionRead_OnlyOwnOrAssigned()
        {
            var user = CreateUser(5, "INTERACTION_READ");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var interactions = new[]
            {
                CreateInteraction(1, 5),
                CreateInteraction(2, 99),
                CreateInteraction(3, 5),
                CreateInteraction(4, 50, users: new List<InteractionUser> { new() { UserId = 5 } }),
            };

            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(3);
        }

        [Fact]

        [Trait("Defect", "DEF-060")]
        public void Criteria_PartnerManagerNoOrgUnit_OnlyCreated()
        {
            var user = CreateUser(7, "PARTNER_MANAGER");
            var spec = new InteractionRBACCompositeSpecification(CreateFilter(), user);

            var interactions = new[]
            {
                CreateInteraction(1, 7),
                CreateInteraction(2, 99),
                CreateInteraction(3, 7),
            };

            interactions.Where(spec.Criteria.Compile()).Should().HaveCount(2);
        }
    }

    #endregion

    #region InteractionFilterRequest Validation Tests

    public class InteractionFilterRequestValidationTests
    {
        private static List<System.ComponentModel.DataAnnotations.ValidationResult> Validate(InteractionFilterRequest request)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
            return request.Validate(context).ToList();
        }

        // --- Positive ---
        [Fact]
        public void Validate_EmptyFilter_ReturnsNoErrors()
        {
            var request = new InteractionFilterRequest();
            Validate(request).Should().BeEmpty();
        }

        // --- Negative ---
        [Fact]
        public void Validate_FromDateAfterToDate_ReturnsError()
        {
            var request = new InteractionFilterRequest
            {
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                ToDate = DateOnly.FromDateTime(DateTime.Today)
            };
            var errors = Validate(request);
            errors.Should().ContainSingle(r => r.ErrorMessage!.Contains("FromDate cannot be later"));
        }

        [Fact]
        public void Validate_DateInFuture_ReturnsError()
        {
            var request = new InteractionFilterRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("future"));
        }

        [Fact]
        public void Validate_FromDateTooOld_ReturnsError()
        {
            var request = new InteractionFilterRequest
            {
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-11))
            };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("10 years"));
        }

        [Fact]
        public void Validate_AdvancedSearchWithoutCriteria_ReturnsError()
        {
            var request = new InteractionFilterRequest
            {
                AdvancedSearch = true,
                SearchCriteria = null
            };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("SearchCriteria is required"));
        }

        [Fact]
        public void Validate_NegativeId_ReturnsError()
        {
            var request = new InteractionFilterRequest { Id = -1 };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("positive"));
        }

        [Fact]
        public void Validate_ZeroId_ReturnsError()
        {
            var request = new InteractionFilterRequest { Id = 0 };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("positive"));
        }

        [Fact]
        public void Validate_NegativeContactId_ReturnsError()
        {
            var request = new InteractionFilterRequest { ContactId = -5 };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("ContactId"));
        }

        [Fact]
        public void Validate_NegativePartnerId_ReturnsError()
        {
            var request = new InteractionFilterRequest { PartnerId = -1 };
            Validate(request).Should().ContainSingle(r => r.ErrorMessage!.Contains("PartnerId"));
        }

        // --- Edge/Boundary ---
        [Fact]
        public void Validate_SameDates_NoError()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var request = new InteractionFilterRequest { FromDate = today, ToDate = today };
            Validate(request).Should().BeEmpty();
        }

        [Fact]
        public void Validate_DateToday_NoError()
        {
            var request = new InteractionFilterRequest
            {
                Date = DateOnly.FromDateTime(DateTime.Today)
            };
            Validate(request).Should().BeEmpty();
        }

        [Fact]
        public void Validate_FromDateExactly10YearsAgo_NoError()
        {
            var request = new InteractionFilterRequest
            {
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-10))
            };
            Validate(request).Should().BeEmpty();
        }

        [Fact]
        public void Validate_AdvancedSearchWithCriteria_NoError()
        {
            var request = new InteractionFilterRequest
            {
                AdvancedSearch = true,
                SearchCriteria = @"[{""field"":""description"",""value"":""test""}]"
            };
            Validate(request).Should().BeEmpty();
        }

        [Fact]
        public void Validate_PositiveId_NoError()
        {
            var request = new InteractionFilterRequest { Id = 1 };
            Validate(request).Should().BeEmpty();
        }

        // --- Functional ---
        [Fact]
        public void Validate_MultipleErrors_ReturnsAll()
        {
            var request = new InteractionFilterRequest
            {
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                ToDate = DateOnly.FromDateTime(DateTime.Today),
                Id = -1,
                AdvancedSearch = true,
                SearchCriteria = null,
            };
            Validate(request).Count.Should().BeGreaterThanOrEqualTo(3);
        }

        [Fact]
        public void DateConversion_FromDateInterface_ConvertsCorrectly()
        {
            var request = new InteractionFilterRequest
            {
                FromDate = new DateOnly(2025, 6, 15)
            };
            var asFilter = (Domain.Specifications.Interfaces.IInteractionSearchFilter)request;
            asFilter.FromDate.Should().Be(new DateTime(2025, 6, 15));
        }

        [Fact]
        public void DateConversion_TypeInterface_ConvertsCorrectly()
        {
            var request = new InteractionFilterRequest
            {
                Type = InteractionType.Email
            };
            var asFilter = (Domain.Specifications.Interfaces.IInteractionSearchFilter)request;
            asFilter.Type.Should().Be("Email");
        }

        // --- Integration ---
        [Fact]
        public void Validate_ValidCompleteFilter_NoErrors()
        {
            var request = new InteractionFilterRequest
            {
                Id = 1,
                ContactId = 5,
                PartnerId = 10,
                FromDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(-3)),
                ToDate = DateOnly.FromDateTime(DateTime.Today),
                SearchText = "meeting",
                AdvancedSearch = false
            };
            Validate(request).Should().BeEmpty();
        }

        [Fact]
        public void PaginationRequest_DefaultValues()
        {
            var request = new InteractionFilterRequest();
            request.PageIndex.Should().Be(1);
            request.PageSize.Should().Be(10);
        }

        [Fact]
        public void PaginationRequest_CustomValues()
        {
            var request = new InteractionFilterRequest
            {
                PageIndex = 3,
                PageSize = 25,
                OrderBy = "date",
                Ascending = false
            };
            request.PageIndex.Should().Be(3);
            request.PageSize.Should().Be(25);
            request.OrderBy.Should().Be("date");
            request.Ascending.Should().BeFalse();
        }
    }

    #endregion
}
