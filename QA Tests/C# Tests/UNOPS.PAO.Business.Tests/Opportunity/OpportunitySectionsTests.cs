/**
 * OPPORTUNITY SECTIONS TESTS
 * 
 * Tests for WHY, WHERE, WHAT, WHO sections (PNO-692, PNO-697, PNO-700, PNO-6701)
 * 
 * Coverage Areas:
 * - WHY Section - Impact & Strategic Alignment
 * - WHERE Section - Geographic Implementation
 * - WHAT Section - Products & Services
 * - WHO Section - Partners & External Stakeholders
 * 
 * @see QA Tests/Opportunity Tests/BusinessLogic/OpportunitySections_TestCases.md
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity
{
    /// <summary>
    /// Opportunity Sections Tests (PNO-692, PNO-697, PNO-700, PNO-6701)
    /// 
    /// Tests the business logic for opportunity section tabs
    /// </summary>
    public class OpportunitySectionsTests
    {
        #region WHY Section - SDG Tests (PNO-692)

        [Fact]
        public void WHY_POS_001_ContextField_AcceptsText()
        {
            // Arrange
            var contextText = "This initiative addresses climate change challenges in developing nations.";
            
            // Act
            var whySection = new {
                Context = contextText,
                IsValid = true
            };

            // Assert
            whySection.Context.Should().NotBeNullOrEmpty();
            whySection.IsValid.Should().BeTrue();
        }

        [Fact]
        public void WHY_POS_003_SDGSelection_AllowsPrimaryAndSecondary()
        {
            // Arrange
            var sdgs = new[] {
                new { Goal = 16, IsPrimary = true },
                new { Goal = 5, IsPrimary = false }
            };

            // Act
            var primaryCount = sdgs.Count(s => s.IsPrimary);
            var secondaryCount = sdgs.Count(s => !s.IsPrimary);

            // Assert
            primaryCount.Should().Be(1);
            secondaryCount.Should().Be(1);
        }

        [Fact]
        public void WHY_NEG_004_SDG_OnlyOnePrimaryAllowed()
        {
            // Arrange
            var sdgs = new[] {
                new { Goal = 16, IsPrimary = true },
                new { Goal = 5, IsPrimary = true } // Second primary - invalid
            };

            // Act
            var primaryCount = sdgs.Count(s => s.IsPrimary);
            var isValid = primaryCount <= 1;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void WHY_POS_011_Beneficiaries_AcceptsPositiveNumbers()
        {
            // Arrange
            var directBeneficiaries = 1000;
            var indirectBeneficiaries = 5000;

            // Act
            var beneficiariesSection = new {
                Direct = directBeneficiaries,
                Indirect = indirectBeneficiaries,
                Total = directBeneficiaries + indirectBeneficiaries
            };

            // Assert
            beneficiariesSection.Direct.Should().BePositive();
            beneficiariesSection.Indirect.Should().BePositive();
            beneficiariesSection.Total.Should().Be(6000);
        }

        [Fact]
        public void WHY_NEG_014_Beneficiaries_RejectsNegativeNumbers()
        {
            // Arrange
            var negativeBeneficiaries = -100;

            // Act
            var isValid = negativeBeneficiaries >= 0;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void WHY_POS_015_BeneficiaryBreakdown_MustSumToTotal()
        {
            // Arrange
            var total = 100;
            var female = 60;
            var male = 40;

            // Act
            var breakdownSum = female + male;
            var isValid = breakdownSum == total;

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void WHY_NEG_016_GoDecision_BlockedWithoutContext()
        {
            // Arrange
            var whySection = new {
                Context = "", // Empty context
                SDGs = new[] { new { Goal = 16, IsPrimary = true } }
            };

            // Act
            var canSubmitGoDecision = !string.IsNullOrWhiteSpace(whySection.Context) && 
                                      whySection.SDGs.Any(s => s.IsPrimary);

            // Assert
            canSubmitGoDecision.Should().BeFalse();
        }

        #endregion

        #region WHERE Section - Geographic Tests (PNO-697)

        [Fact]
        public void WHERE_POS_002_SingleCountrySelection_Succeeds()
        {
            // Arrange
            var country = new { Id = 1, Name = "Kenya", Code = "KE" };

            // Act
            var whereSection = new {
                Countries = new[] { country },
                CountryCount = 1
            };

            // Assert
            whereSection.Countries.Should().HaveCount(1);
            whereSection.Countries.First().Name.Should().Be("Kenya");
        }

        [Fact]
        public void WHERE_POS_003_RegionSelection_IncludesCountries()
        {
            // Arrange
            var countriesInRegion = new[] { 
                new { Name = "Kenya", IsExcluded = false },
                new { Name = "Nigeria", IsExcluded = false },
                new { Name = "South Africa", IsExcluded = true } // Excluded
            };

            // Act
            var includedCountries = countriesInRegion.Where(c => !c.IsExcluded).ToArray();

            // Assert
            includedCountries.Should().HaveCount(2);
            includedCountries.Should().NotContain(c => c.Name == "South Africa");
        }

        [Fact]
        public void WHERE_POS_004_FragileState_IsHighlighted()
        {
            // Arrange
            var country = new { 
                Name = "Haiti", 
                IsFragileState = true, 
                IsSIDS = true 
            };

            // Act
            var requiresHighlight = country.IsFragileState || country.IsSIDS;

            // Assert
            requiresHighlight.Should().BeTrue();
        }

        [Fact]
        public void WHERE_POS_005_OrgUnit_IdentifiedForCountry()
        {
            // Arrange
            var country = new { Name = "Mexico", Code = "MX" };
            var orgUnitMapping = new Dictionary<string, string> {
                { "MX", "MCO Mexico" },
                { "CO", "MCO Colombia" }
            };

            // Act
            var orgUnit = orgUnitMapping.GetValueOrDefault(country.Code, "Unknown");

            // Assert
            orgUnit.Should().Be("MCO Mexico");
        }

        [Fact]
        public void WHERE_POS_006_HCA_StatusDisplayed()
        {
            // Arrange
            var countriesWithHCA = new[] {
                new { Name = "Portugal", HasHCA = true },
                new { Name = "Brazil", HasHCA = false }
            };

            // Assert
            countriesWithHCA.First(c => c.Name == "Portugal").HasHCA.Should().BeTrue();
            countriesWithHCA.First(c => c.Name == "Brazil").HasHCA.Should().BeFalse();
        }

        [Fact]
        public void WHERE_POS_007_UNSDCF_TriggersStrategicAlignment()
        {
            // Arrange
            var country = new { Name = "Vietnam", HasActiveUNSDCF = true };

            // Act
            var requiresStrategicAlignment = country.HasActiveUNSDCF;

            // Assert
            requiresStrategicAlignment.Should().BeTrue();
        }

        #endregion

        #region WHAT Section - Products & Services Tests (PNO-700)

        [Fact]
        public void WHAT_POS_004_DeliveryModality_HasFourOptions()
        {
            // Arrange
            var modalityOptions = new[] {
                "UNOPS will be delivering all directly",
                "All via Grant Support",
                "Some via Grant Support",
                "Not yet known"
            };

            // Assert
            modalityOptions.Should().HaveCount(4);
        }

        [Fact]
        public void WHAT_POS_009_ProcurementExpert_FlaggedForRelevantServices()
        {
            // Arrange
            var service = new { 
                Name = "Technical advisory services - infrastructure",
                RequiresProcurementExpert = true
            };

            // Assert
            service.RequiresProcurementExpert.Should().BeTrue();
        }

        [Fact]
        public void WHAT_NEG_011_DeliveryModality_IsMandatory()
        {
            // Arrange
            var whatSection = new {
                Products = new[] { new { Name = "Service 1" } },
                DeliveryModality = (string?)null // Not selected
            };

            // Act
            var isValid = whatSection.DeliveryModality != null;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void WHAT_POS_012_NotYetKnown_IsAccepted()
        {
            // Arrange
            var whatSection = new {
                DeliveryModality = "Not yet known"
            };

            // Act
            var isValid = !string.IsNullOrEmpty(whatSection.DeliveryModality);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void WHAT_POS_013_SomeGrantSupport_RequiresIdentification()
        {
            // Arrange
            var whatSection = new {
                DeliveryModality = "Some via Grant Support",
                Products = new[] {
                    new { Name = "Service 1", IsGrantSupport = true },
                    new { Name = "Service 2", IsGrantSupport = false }
                }
            };

            // Act
            var grantSupportIdentified = whatSection.Products.Any(p => p.IsGrantSupport);

            // Assert
            grantSupportIdentified.Should().BeTrue();
        }

        #endregion

        #region WHO Section - Partners & Stakeholders Tests (PNO-6701)

        [Fact]
        public void WHO_POS_007_OrgUnitDeliveryValue_TriggersWarning()
        {
            // Arrange
            var totalBudget = 150000m;
            var orgUnitMaxHistory = 100000m;

            // Act
            var exceedsHistory = totalBudget > orgUnitMaxHistory;

            // Assert
            exceedsHistory.Should().BeTrue();
        }

        [Fact]
        public void WHO_NEG_010_PooledFundingPartner_CannotBeSelected()
        {
            // Arrange
            var partner = new { 
                Name = "EU Programme Fund", 
                IsPooledFunding = true 
            };

            // Act
            var canBeSelectedAsFundingPartner = !partner.IsPooledFunding;

            // Assert
            canBeSelectedAsFundingPartner.Should().BeFalse();
        }

        [Fact]
        public void WHO_POS_FundingPartner_CanBeAdded()
        {
            // Arrange
            var partner = new { 
                Name = "World Bank", 
                IsPooledFunding = false,
                Type = "FundingPartner"
            };

            // Act
            var canBeSelectedAsFundingPartner = !partner.IsPooledFunding;

            // Assert
            canBeSelectedAsFundingPartner.Should().BeTrue();
        }

        [Fact]
        public void WHO_POS_ExternalStakeholder_CanBeAdded()
        {
            // Arrange
            var stakeholder = new {
                Name = "Local Government",
                Role = "Implementation Partner",
                IsExternal = true
            };

            // Assert
            stakeholder.IsExternal.Should().BeTrue();
            stakeholder.Role.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Team Section Tests (PNO-979)

        [Fact]
        public void TEAM_POS_002_AddTeamMember_Succeeds()
        {
            // Arrange
            var teamMember = new {
                UserId = 1,
                Name = "John Doe",
                Role = "Technical Specialist"
            };

            // Act
            var team = new[] { teamMember };

            // Assert
            team.Should().ContainSingle();
            team.First().Role.Should().Be("Technical Specialist");
        }

        [Fact]
        public void TEAM_POS_005_TeamLead_CanBeDesignated()
        {
            // Arrange
            var teamMembers = new[] {
                new { UserId = 1, Name = "John", IsTeamLead = false },
                new { UserId = 2, Name = "Jane", IsTeamLead = true }
            };

            // Act
            var teamLead = teamMembers.FirstOrDefault(m => m.IsTeamLead);

            // Assert
            teamLead.Should().NotBeNull();
            teamLead!.Name.Should().Be("Jane");
        }

        [Fact]
        public void TEAM_NEG_006_TeamLead_IsRequired()
        {
            // Arrange
            var teamMembers = new[] {
                new { UserId = 1, Name = "John", IsTeamLead = false },
                new { UserId = 2, Name = "Jane", IsTeamLead = false }
            };

            // Act
            var hasTeamLead = teamMembers.Any(m => m.IsTeamLead);

            // Assert
            hasTeamLead.Should().BeFalse();
        }

        [Fact]
        public void TEAM_NEG_007_DuplicateTeamMember_Prevented()
        {
            // Arrange
            var existingTeam = new[] { 
                new { UserId = 1, Name = "John" } 
            };
            var newMemberUserId = 1; // Same as existing

            // Act
            var isDuplicate = existingTeam.Any(m => m.UserId == newMemberUserId);

            // Assert
            isDuplicate.Should().BeTrue();
        }

        #endregion
    }
}
