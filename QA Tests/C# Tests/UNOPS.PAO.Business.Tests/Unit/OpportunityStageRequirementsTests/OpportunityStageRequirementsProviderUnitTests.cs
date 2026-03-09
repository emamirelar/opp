/**
 * @fileoverview Comprehensive unit tests for OpportunityStageRequirementsProvider.
 * Validates all 21 mandatory field requirements for IDENTIFY & PROFILE → GO transition.
 * Covers EntityNames, stage transitions, requirement definitions, custom validators, and edge cases.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.StageRequirements;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models.Requirements;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityStageRequirementsTests;

/// <summary>
/// Unit tests for OpportunityStageRequirementsProvider.
/// Requirements source: UNOPS.PAO.Business/Workflow/StageRequirements/OpportunityStageRequirementsProvider.cs
/// PRD FR-2.1: 21 mandatory fields for GO transition.
/// </summary>
[Collection("Unit")]
[Trait("Category", "Unit")]
[Trait("Type", "Unit")]
public class OpportunityStageRequirementsProviderUnitTests
{
    private readonly IStageRequirementsProvider _provider = new OpportunityStageRequirementsProvider();

    #region EntityNames

    [Fact]
    [Trait("SubCategory", "EntityNames")]
    public void EntityNames_ReturnsExactlyOpportunity()
    {
        var names = _provider.EntityNames.ToList();
        names.Should().BeEquivalentTo(new[] { "Opportunity" });
        names.Should().HaveCount(1);
    }

    [Fact]
    [Trait("SubCategory", "EntityNames")]
    public void EntityNames_ContainsOpportunity()
    {
        _provider.EntityNames.Should().Contain("Opportunity");
    }

    [Fact]
    [Trait("SubCategory", "EntityNames")]
    public void EntityNames_IsEnumerable()
    {
        var count = _provider.EntityNames.Count();
        count.Should().Be(1);
    }

    #endregion

    #region SupportsEntity

    [Fact]
    [Trait("SubCategory", "SupportsEntity")]
    public void SupportsEntity_WithOpportunity_ReturnsTrue()
    {
        _provider.SupportsEntity("Opportunity").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "SupportsEntity")]
    public void SupportsEntity_WithOpportunityLowercase_ReturnsTrue()
    {
        _provider.SupportsEntity("opportunity").Should().BeTrue();
    }

    [Fact]
    [Trait("SubCategory", "SupportsEntity")]
    public void SupportsEntity_WithPartner_ReturnsFalse()
    {
        _provider.SupportsEntity("Partner").Should().BeFalse();
    }

    [Fact]
    [Trait("SubCategory", "SupportsEntity")]
    public void SupportsEntity_WithEmptyString_ReturnsFalse()
    {
        _provider.SupportsEntity("").Should().BeFalse();
    }

    #endregion

    #region GetRequirementsForStageChange — GO Transition (21 requirements)

    [Fact]
    [Trait("SubCategory", "GoTransition")]
    public void GetRequirementsForStageChange_IdentifyToGo_Returns21Requirements()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        requirements.Should().HaveCount(21);
    }

    [Fact]
    [Trait("SubCategory", "GoTransition")]
    public void GetRequirementsForStageChange_IdentifyToGo_AllRequirementsHaveUniqueNames()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var names = requirements.Select(r => r.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("SubCategory", "GoTransition")]
    public void GetRequirementsForStageChange_IdentifyToGo_AllRequirementsHaveDescriptionKeys()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        requirements.Should().AllSatisfy(r =>
        {
            r.Description.Should().NotBeNullOrEmpty();
            r.Description.Should().StartWith("message.requirements.opportunity.");
        });
    }

    [Fact]
    [Trait("SubCategory", "GoTransition")]
    public void GetRequirementsForStageChange_IdentifyToGo_RequirementNamesMatchExpected()
    {
        var expectedNames = new[]
        {
            "name", "description", "initiativeBudgetUSD", "deliverables", "challenges",
            "expectedImpact", "expectedOutcomes", "beneficiaries", "sdgs", "unopsMissions",
            "fundingPartners", "clientPartners", "countries", "targetSigningDate",
            "implementationStartDate", "targetDeliveryDate", "opportunityStatementMarkdown",
            "opportunityManager", "responsibleOrgUnitId", "proposedInitiativeTypeId", "doaHolders"
        };

        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var actualNames = requirements.Select(r => r.Name).OrderBy(n => n).ToList();
        var expectedOrdered = expectedNames.OrderBy(n => n).ToList();
        actualNames.Should().BeEquivalentTo(expectedOrdered);
    }

    #endregion

    #region Individual Requirement Definitions — Text Fields

    [Theory]
    [InlineData("name", "name", FieldTypes.Text, "message.requirements.opportunity.nameRequired")]
    [InlineData("description", "description", FieldTypes.Text, "message.requirements.opportunity.descriptionRequired")]
    [InlineData("challenges", "challenges", FieldTypes.Text, "message.requirements.opportunity.challengesRequired")]
    [InlineData("expectedImpact", "expectedImpact", FieldTypes.Text, "message.requirements.opportunity.impactRequired")]
    [InlineData("expectedOutcomes", "expectedOutcomes", FieldTypes.Text, "message.requirements.opportunity.outcomesRequired")]
    [InlineData("opportunityStatementMarkdown", "opportunityStatementMarkdown", FieldTypes.Text, "message.requirements.opportunity.statementRequired")]
    [Trait("SubCategory", "TextRequirements")]
    public void GetRequirementsForStageChange_GoTransition_TextRequirement_HasCorrectStructure(
        string name, string fieldName, string fieldType, string description)
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == name);
        req.Should().NotBeNull();
        req!.Name.Should().Be(name);
        req.FieldName.Should().Be(fieldName);
        req.FieldType.Should().Be(fieldType);
        req.Description.Should().Be(description);
        req.Validation.Should().NotBeNull();
        req.Validation!.Required.Should().BeTrue();
    }

    #endregion

    #region Individual Requirement Definitions — Number (Budget)

    [Fact]
    [Trait("SubCategory", "NumberRequirements")]
    public void GetRequirementsForStageChange_GoTransition_InitiatveBudgetUSD_HasCorrectStructure()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "initiativeBudgetUSD");
        req.Should().NotBeNull();
        req!.FieldName.Should().Be("initiativeBudgetUSD");
        req.FieldType.Should().Be(FieldTypes.Number);
        req.Description.Should().Be("message.requirements.opportunity.budgetRequired");
        req.Validation.Should().NotBeNull();
        req.Validation!.Required.Should().BeTrue();
        req.Validation.GreaterThan.Should().Be(0);
    }

    #endregion

    #region Individual Requirement Definitions — Array Fields

    [Theory]
    [InlineData("deliverables", "message.requirements.opportunity.productsRequired")]
    [InlineData("sdgs", "message.requirements.opportunity.sdgRequired")]
    [InlineData("unopsMissions", "message.requirements.opportunity.missionsRequired")]
    [InlineData("fundingPartners", "message.requirements.opportunity.fundingPartnerRequired")]
    [InlineData("clientPartners", "message.requirements.opportunity.clientPartnerRequired")]
    [InlineData("countries", "message.requirements.opportunity.countriesRequired")]
    [Trait("SubCategory", "ArrayRequirements")]
    public void GetRequirementsForStageChange_GoTransition_ArrayRequirement_HasMinLength1(
        string name, string description)
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == name);
        req.Should().NotBeNull();
        req!.FieldType.Should().Be(FieldTypes.Array);
        req.Description.Should().Be(description);
        req.Validation.Should().NotBeNull();
        req.Validation!.Required.Should().BeTrue();
        req.Validation.MinLength.Should().Be(1);
    }

    #endregion

    #region Individual Requirement Definitions — Date Fields

    [Theory]
    [InlineData("targetSigningDate", "message.requirements.opportunity.signingDateRequired")]
    [InlineData("implementationStartDate", "message.requirements.opportunity.startDateRequired")]
    [InlineData("targetDeliveryDate", "message.requirements.opportunity.endDateRequired")]
    [Trait("SubCategory", "DateRequirements")]
    public void GetRequirementsForStageChange_GoTransition_DateRequirement_HasCorrectStructure(
        string name, string description)
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == name);
        req.Should().NotBeNull();
        req!.FieldName.Should().Be(name);
        req.FieldType.Should().Be(FieldTypes.Date);
        req.Description.Should().Be(description);
        req.Validation.Should().NotBeNull();
        req.Validation!.Required.Should().BeTrue();
    }

    #endregion

    #region Individual Requirement Definitions — Select Fields

    [Theory]
    [InlineData("responsibleOrgUnitId", "message.requirements.opportunity.orgUnitRequired")]
    [InlineData("proposedInitiativeTypeId", "message.requirements.opportunity.initiativeTypeRequired")]
    [Trait("SubCategory", "SelectRequirements")]
    public void GetRequirementsForStageChange_GoTransition_SelectRequirement_HasCorrectStructure(
        string name, string description)
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == name);
        req.Should().NotBeNull();
        req!.FieldName.Should().Be(name);
        req.FieldType.Should().Be(FieldTypes.Select);
        req.Description.Should().Be(description);
        req.Validation.Should().NotBeNull();
        req.Validation!.Required.Should().BeTrue();
    }

    #endregion

    #region Custom Validators — Beneficiaries

    [Fact]
    [Trait("SubCategory", "BeneficiariesValidator")]
    public void GetRequirementsForStageChange_GoTransition_Beneficiaries_HasBeneficiariesValidator()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "beneficiaries");
        req.Should().NotBeNull();
        req!.FieldType.Should().Be("conditional");
        req.FieldName.Should().Be("beneficiaries");
        req.Description.Should().Be("message.requirements.opportunity.beneficiariesRequired");
        req.CustomValidatorConfig.Should().NotBeNull();
        req.CustomValidatorConfig!["validatorName"].Should().Be("BeneficiariesValidator");
    }

    [Fact]
    [Trait("SubCategory", "BeneficiariesValidator")]
    public void GetRequirementsForStageChange_GoTransition_Beneficiaries_HasCorrectFieldsConfig()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "beneficiaries");
        req.Should().NotBeNull();
        var fields = req!.CustomValidatorConfig!["fields"] as string[];
        fields.Should().NotBeNull();
        fields.Should().BeEquivalentTo(new[] { "beneficiariesToBeDetermined", "estimatedDirectBeneficiaries", "estimatedIndirectBeneficiaries" });
    }

    [Fact]
    [Trait("SubCategory", "BeneficiariesValidator")]
    public void GetRequirementsForStageChange_GoTransition_Beneficiaries_HasRuleConfig()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "beneficiaries");
        req.Should().NotBeNull();
        var rule = req!.CustomValidatorConfig!["rule"]?.ToString();
        rule.Should().NotBeNullOrEmpty();
        rule.Should().Contain("BeneficiariesToBeDetermined");
        rule.Should().Contain("EstimatedDirectBeneficiaries");
        rule.Should().Contain("EstimatedIndirectBeneficiaries");
    }

    #endregion

    #region Custom Validators — StakeholderRole (Opportunity Manager)

    [Fact]
    [Trait("SubCategory", "StakeholderRoleValidator")]
    public void GetRequirementsForStageChange_GoTransition_OpportunityManager_HasStakeholderRoleValidator()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "opportunityManager");
        req.Should().NotBeNull();
        req!.FieldName.Should().Be("stakeholders");
        req.FieldType.Should().Be("roles");
        req.Description.Should().Be("message.requirements.opportunity.managerRequired");
        req.CustomValidatorConfig.Should().NotBeNull();
        req.CustomValidatorConfig!["validatorName"].Should().Be("StakeholderRoleValidator");
    }

    [Fact]
    [Trait("SubCategory", "StakeholderRoleValidator")]
    public void GetRequirementsForStageChange_GoTransition_OpportunityManager_HasRequiredRoleAndMinCount()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "opportunityManager");
        req.Should().NotBeNull();
        req!.CustomValidatorConfig!["requiredRole"].Should().Be("Opportunity Manager");
        req.CustomValidatorConfig["minCount"].Should().Be(1);
    }

    #endregion

    #region Custom Validators — DoA Holder

    [Fact]
    [Trait("SubCategory", "DoAHolderValidator")]
    public void GetRequirementsForStageChange_GoTransition_DoAHolders_HasDoAHolderValidator()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "doaHolders");
        req.Should().NotBeNull();
        req!.FieldName.Should().Be("doaHolders");
        req.FieldType.Should().Be("doaValidation");
        req.Description.Should().Be("message.requirements.opportunity.doaHolderRequired");
        req.OnlyServerSideEvaluation.Should().BeTrue();
        req.CustomValidatorConfig.Should().NotBeNull();
        req.CustomValidatorConfig!["validatorName"].Should().Be("DoAHolderValidator");
    }

    [Fact]
    [Trait("SubCategory", "DoAHolderValidator")]
    public void GetRequirementsForStageChange_GoTransition_DoAHolders_HasEntityRoleCodesAndLookupField()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "doaHolders");
        req.Should().NotBeNull();
        var entityRoleCodes = req!.CustomValidatorConfig!["entityRoleCodes"] as string[];
        entityRoleCodes.Should().NotBeNull();
        entityRoleCodes.Should().BeEquivalentTo(new[] { "DoA2_OrganizationHierarchy", "DoA3_OrganizationHierarchy" });
        req.CustomValidatorConfig["lookupField"].Should().Be("responsibleOrgUnitId");
    }

    #endregion

    #region unopsMissions Conditional Validation

    [Fact]
    [Trait("SubCategory", "ConditionalValidation")]
    public void GetRequirementsForStageChange_GoTransition_UnopsMissions_HasConditionalValidation()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        var req = requirements.FirstOrDefault(r => r.Name == "unopsMissions");
        req.Should().NotBeNull();
        req!.Validation.Should().NotBeNull();
        req.Validation!.Conditional.Should().NotBeNull();
        req.Validation.Conditional!.Field.Should().Be("unopsMissionsNotApplicable");
        req.Validation.Conditional.Value.Should().Be(false);
    }

    #endregion

    #region Non-GO Transitions — Empty List

    [Theory]
    [InlineData(OpportunityWorkflow.Stages.IdentifyAndProfile, OpportunityWorkflow.Stages.NoGo)]
    [InlineData(OpportunityWorkflow.Stages.IdentifyAndProfile, OpportunityWorkflow.Stages.Cancelled)]
    [InlineData(OpportunityWorkflow.Stages.NoGo, OpportunityWorkflow.Stages.IdentifyAndProfile)]
    [InlineData(OpportunityWorkflow.Stages.Cancelled, OpportunityWorkflow.Stages.IdentifyAndProfile)]
    [Trait("SubCategory", "NonGoTransitions")]
    public void GetRequirementsForStageChange_NonGoTransition_ReturnsEmptyList(string currentStage, string nextStage)
    {
        var requirements = _provider.GetRequirementsForStageChange(currentStage, nextStage);
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "NonGoTransitions")]
    public void GetRequirementsForStageChange_GoToIdentify_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.Go,
            OpportunityWorkflow.Stages.IdentifyAndProfile);
        requirements.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases — Stage Strings

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_NullCurrentStage_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(null!, OpportunityWorkflow.Stages.Go);
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_NullNextStage_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(OpportunityWorkflow.Stages.IdentifyAndProfile, null!);
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_EmptyCurrentStage_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange("", OpportunityWorkflow.Stages.Go);
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_EmptyNextStage_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(OpportunityWorkflow.Stages.IdentifyAndProfile, "");
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_InvalidStageNames_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange("Draft", "Submitted");
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_LowercaseGo_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            "go");
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "EdgeCases")]
    public void GetRequirementsForStageChange_WhitespaceStages_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForStageChange(" IDENTIFY & PROFILE ", " GO ");
        requirements.Should().BeEmpty();
    }

    #endregion

    #region GetRequirementsForProcessStep

    [Fact]
    [Trait("SubCategory", "ProcessStep")]
    public void GetRequirementsForProcessStep_AnyStep_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForProcessStep("AnyStep");
        requirements.Should().BeEmpty();
    }

    [Fact]
    [Trait("SubCategory", "ProcessStep")]
    public void GetRequirementsForProcessStep_EmptyStep_ReturnsEmptyList()
    {
        var requirements = _provider.GetRequirementsForProcessStep("");
        requirements.Should().BeEmpty();
    }

    #endregion

    #region IStageRequirementsProvider Interface

    [Fact]
    [Trait("SubCategory", "Interface")]
    public void Provider_ImplementsIStageRequirementsProvider()
    {
        _provider.Should().BeAssignableTo<IStageRequirementsProvider>();
    }

    [Fact]
    [Trait("SubCategory", "Interface")]
    public void GetRequirementsForStageChange_ReturnsNewListEachCall()
    {
        var req1 = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);
        var req2 = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        req1.Should().NotBeSameAs(req2);
        req1.Should().BeEquivalentTo(req2);
    }

    #endregion

    #region Requirement Order (UI Display Order)

    [Fact]
    [Trait("SubCategory", "Order")]
    public void GetRequirementsForStageChange_GoTransition_FirstRequirementIsName()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        requirements.First().Name.Should().Be("name");
    }

    [Fact]
    [Trait("SubCategory", "Order")]
    public void GetRequirementsForStageChange_GoTransition_LastRequirementIsDoaHolders()
    {
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        requirements.Last().Name.Should().Be("doaHolders");
    }

    #endregion
}
