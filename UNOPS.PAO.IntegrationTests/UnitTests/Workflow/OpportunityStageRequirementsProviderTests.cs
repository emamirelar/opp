using FluentAssertions;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.StageRequirements;
using UNOPS.Workflow.Models.Requirements;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

/// <summary>
/// Unit tests for OpportunityStageRequirementsProvider.
/// Tests all 21 mandatory field requirements for the GO transition.
/// </summary>
public class OpportunityStageRequirementsProviderTests
{
    private readonly OpportunityStageRequirementsProvider _provider;

    public OpportunityStageRequirementsProviderTests()
    {
        _provider = new OpportunityStageRequirementsProvider();
    }

    [Fact]
    public void EntityNames_ShouldContainOpportunity()
    {
        // Assert
        _provider.EntityNames.Should().Contain("Opportunity");
    }

    [Fact]
    public void SupportsEntity_WithOpportunity_ShouldReturnTrue()
    {
        // Act
        var result = _provider.SupportsEntity("Opportunity");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsEntity_WithOtherEntity_ShouldReturnFalse()
    {
        // Act
        var result = _provider.SupportsEntity("Partner");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetRequirementsForStageChange_IdentifyToGo_ShouldReturn21Requirements()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        requirements.Should().HaveCount(21);
    }

    [Fact]
    public void GetRequirementsForStageChange_IdentifyToNoGo_ShouldReturnEmptyList()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.NoGo);

        // Assert
        requirements.Should().BeEmpty();
    }

    [Fact]
    public void GetRequirementsForStageChange_IdentifyToCancelled_ShouldReturnEmptyList()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Cancelled);

        // Assert
        requirements.Should().BeEmpty();
    }

    [Fact]
    public void GetRequirementsForStageChange_NoGoToIdentify_ShouldReturnEmptyList()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.NoGo,
            OpportunityWorkflow.Stages.IdentifyAndProfile);

        // Assert
        requirements.Should().BeEmpty();
    }

    [Fact]
    public void GetRequirementsForStageChange_CancelledToIdentify_ShouldReturnEmptyList()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.Cancelled,
            OpportunityWorkflow.Stages.IdentifyAndProfile);

        // Assert
        requirements.Should().BeEmpty();
    }

    [Theory]
    [InlineData("name", "text")]
    [InlineData("description", "text")]
    [InlineData("challenges", "text")]
    [InlineData("expectedImpact", "text")]
    [InlineData("expectedOutcomes", "text")]
    [InlineData("opportunityStatementMarkdown", "text")]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredTextFields(
        string fieldName, string expectedType)
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.FieldName == fieldName);
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be(expectedType);
        requirement.Validation.Should().NotBeNull();
        requirement.Validation!.Required.Should().BeTrue();
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeBudgetRequirement()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.FieldName == "initiativeBudgetUSD");
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be(FieldTypes.Number);
        requirement.Validation.Should().NotBeNull();
        requirement.Validation!.Required.Should().BeTrue();
        requirement.Validation.GreaterThan.Should().Be(0);
    }

    [Theory]
    [InlineData("unopsMissions")]
    [InlineData("sdgs")]
    [InlineData("fundingPartners")]
    [InlineData("clientPartners")]
    [InlineData("deliverables")]
    [InlineData("countries")]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeArrayFieldsWithMinLength1(
        string fieldName)
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.FieldName == fieldName);
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be(FieldTypes.Array);
        requirement.Validation.Should().NotBeNull();
        requirement.Validation!.Required.Should().BeTrue();
        requirement.Validation.MinLength.Should().Be(1);
    }

    [Theory]
    [InlineData("targetSigningDate")]
    [InlineData("implementationStartDate")]
    [InlineData("targetDeliveryDate")]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredDateFields(
        string fieldName)
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.FieldName == fieldName);
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be(FieldTypes.Date);
        requirement.Validation.Should().NotBeNull();
        requirement.Validation!.Required.Should().BeTrue();
    }

    [Theory]
    [InlineData("responsibleOrgUnitId")]
    [InlineData("proposedInitiativeTypeId")]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeRequiredSelectFields(
        string fieldName)
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.FieldName == fieldName);
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be(FieldTypes.Select);
        requirement.Validation.Should().NotBeNull();
        requirement.Validation!.Required.Should().BeTrue();
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeBeneficiariesConditionalValidation()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.Name == "beneficiaries");
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be("conditional");
        requirement.CustomValidatorConfig.Should().NotBeNull();
        requirement.CustomValidatorConfig!["validatorName"].Should().Be("BeneficiariesValidator");
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeOpportunityManagerRoleValidation()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.Name == "opportunityManager");
        requirement.Should().NotBeNull();
        requirement!.FieldName.Should().Be("stakeholders");
        requirement.FieldType.Should().Be("roles");
        requirement.CustomValidatorConfig.Should().NotBeNull();
        requirement.CustomValidatorConfig!["requiredRole"].Should().Be("Opportunity Manager");
        requirement.CustomValidatorConfig["minCount"].Should().Be(1);
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_ShouldIncludeDoA2ServerSideValidation()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var requirement = requirements.FirstOrDefault(r => r.Name == "doaHolders");
        requirement.Should().NotBeNull();
        requirement!.FieldType.Should().Be("doaValidation");
        requirement.OnlyServerSideEvaluation.Should().BeTrue();
        requirement.CustomValidatorConfig.Should().NotBeNull();
        requirement.CustomValidatorConfig!["validatorName"].Should().Be("DoA2HolderValidator");
        requirement.CustomValidatorConfig["entityRoleCode"].Should().Be("DoA2_OrganizationHierarchy");
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_AllRequirementsShouldHaveDescriptionKeys()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        requirements.Should().AllSatisfy(r =>
        {
            r.Description.Should().NotBeNullOrEmpty();
            r.Description.Should().StartWith("message.requirements.opportunity.");
        });
    }

    [Fact]
    public void GetRequirementsForStageChange_GoTransition_AllRequirementsShouldHaveUniqueNames()
    {
        // Act
        var requirements = _provider.GetRequirementsForStageChange(
            OpportunityWorkflow.Stages.IdentifyAndProfile,
            OpportunityWorkflow.Stages.Go);

        // Assert
        var names = requirements.Select(r => r.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetRequirementsForProcessStep_ShouldReturnEmptyList()
    {
        // Act
        var requirements = _provider.GetRequirementsForProcessStep("AnyStep");

        // Assert
        requirements.Should().BeEmpty();
    }
}
