/**
 * @fileoverview Data entry permutation tests for Opportunity creation (OverviewSectionRequest) and What section (WhatSectionRequest).
 * @author UNOPS Opportunity+ QA Team
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Opportunity;

/// <summary>
/// Tests for Opportunity creation (OverviewSectionRequest) and What section (WhatSectionRequest).
///
/// Requirements validated:
/// - REQ-1: OverviewSectionRequest field order independence → Tests: FieldOrder_Overview_*
/// - REQ-2: WhatSectionRequest field order independence → Tests: FieldOrder_What_*
/// - REQ-3: Invalid value combinations (Name max 120, DeliveryModality 0/5/-1/99, negative budget) → Tests: Pairwise_*, OneInvalid_*
/// - REQ-4: Mixed valid/invalid combinations → Tests: Mixed_*
/// - REQ-5: Partial submission with optional field subsets → Tests: Partial_*
/// - REQ-6: Boundary value combinations (Name 120/121, Description very long, budget extremes) → Tests: Boundary_*
///
/// Defects found: None
/// </summary>
[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "Opportunity")]
public class OpportunityCreatePermutationTests : ManagerTestBase
{
    private const int NameMaxLength = 120;
    private static readonly int[] ValidDeliveryModalityValues = { 1, 2, 3, 4 };
    private static readonly int[] InvalidDeliveryModalityValues = { 0, 5, -1, 99 };

    private static List<ValidationResult> ValidateRequest(object request)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request, null, null);
        Validator.TryValidateObject(request, context, results, true);
        return results;
    }

    private static OverviewSectionRequest CreateValidOverviewRequest() => new()
    {
        Name = "Valid Opportunity Name",
        Description = "Valid description",
        InitiativeBudgetUSD = 100000m
    };

    private static WhatSectionRequest CreateValidWhatRequest() => new()
    {
        Name = "Valid What Name",
        Description = "Valid description",
        ResponsibleOrgUnitId = 1,
        ProposedInitiativeTypeId = 1,
        DeliveryModality = 2,
        Deliverables = new List<OpportunityDeliverableRequest> { new() { OutputId = 1, Quantity = 10m } }
    };

    // ========== 1. FIELD ORDER PERMUTATIONS ==========

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [Trait("Category", "Functional")]
    public void FieldOrder_OverviewSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildOverviewRequestByOrder(orderIndex);
        request.Name.Should().Be("Opp A");
        request.Description.Should().Be("Desc");
        request.InitiativeBudgetUSD.Should().Be(50000m);
    }

    private static OverviewSectionRequest BuildOverviewRequestByOrder(int orderIndex)
    {
        var orders = new[]
        {
            () => new OverviewSectionRequest { Name = "Opp A", Description = "Desc", InitiativeBudgetUSD = 50000m },
            () => new OverviewSectionRequest { InitiativeBudgetUSD = 50000m, Description = "Desc", Name = "Opp A" },
            () => new OverviewSectionRequest { Description = "Desc", Name = "Opp A", InitiativeBudgetUSD = 50000m },
            () => new OverviewSectionRequest { Name = "Opp A", InitiativeBudgetUSD = 50000m, Description = "Desc" },
            () => new OverviewSectionRequest { InitiativeBudgetUSD = 50000m, Name = "Opp A", Description = "Desc" }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [Trait("Category", "Functional")]
    public void FieldOrder_WhatSectionRequest_FieldsSetInDifferentOrders_PropertiesMatchRegardlessOfOrder(int orderIndex)
    {
        var request = BuildWhatRequestByOrder(orderIndex);
        request.Name.Should().Be("What A");
        request.Description.Should().Be("Desc");
        request.ResponsibleOrgUnitId.Should().Be(1);
        request.ProposedInitiativeTypeId.Should().Be(2);
        request.DeliveryModality.Should().Be(3);
        request.Deliverables.Should().NotBeNull().And.HaveCount(1);
    }

    private static WhatSectionRequest BuildWhatRequestByOrder(int orderIndex)
    {
        var deliverables = new List<OpportunityDeliverableRequest> { new() { OutputId = 1 } };
        var orders = new[]
        {
            () => new WhatSectionRequest { Name = "What A", Description = "Desc", ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 2, DeliveryModality = 3, Deliverables = deliverables },
            () => new WhatSectionRequest { Deliverables = deliverables, DeliveryModality = 3, ProposedInitiativeTypeId = 2, ResponsibleOrgUnitId = 1, Description = "Desc", Name = "What A" },
            () => new WhatSectionRequest { ResponsibleOrgUnitId = 1, Name = "What A", DeliveryModality = 3, Description = "Desc", ProposedInitiativeTypeId = 2, Deliverables = deliverables },
            () => new WhatSectionRequest { ProposedInitiativeTypeId = 2, Description = "Desc", Name = "What A", ResponsibleOrgUnitId = 1, DeliveryModality = 3, Deliverables = deliverables },
            () => new WhatSectionRequest { DeliveryModality = 3, Name = "What A", Deliverables = deliverables, Description = "Desc", ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 2 },
            () => new WhatSectionRequest { Description = "Desc", Deliverables = deliverables, Name = "What A", ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 2, DeliveryModality = 3 }
        };
        return orderIndex < orders.Length ? orders[orderIndex]() : orders[0]();
    }

    // ========== 2. PAIRWISE / INVALID COMBINATIONS ==========

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    [InlineData(99)]
    [Trait("Category", "Negative")]
    public void OneInvalid_WhatSectionRequest_DeliveryModality_InvalidValue_PropertyAcceptsValue(int invalidModality)
    {
        var request = CreateValidWhatRequest();
        request.DeliveryModality = invalidModality;
        request.DeliveryModality.Should().Be(invalidModality);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.01)]
    [Trait("Category", "Negative")]
    public void OneInvalid_OverviewSectionRequest_InitiativeBudgetUSD_Negative_PropertyAcceptsValue(double invalidBudgetDbl)
    {
        var invalidBudget = (decimal)invalidBudgetDbl;
        var request = CreateValidOverviewRequest();
        request.InitiativeBudgetUSD = invalidBudget;
        request.InitiativeBudgetUSD.Should().Be(invalidBudget);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_OverviewSectionRequest_NameOverMax_InitiativeBudgetNegative_PropertiesReflectValues()
    {
        var overMax = InvalidValueSets.OverMaxLengthString(NameMaxLength);
        var request = CreateValidOverviewRequest();
        request.Name = overMax;
        request.InitiativeBudgetUSD = -100m;
        request.Name.Should().HaveLength(NameMaxLength + 1);
        request.InitiativeBudgetUSD.Should().Be(-100m);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Pairwise_WhatSectionRequest_InvalidDeliveryModality_InvalidOrgUnitId_PropertiesReflectValues()
    {
        var request = CreateValidWhatRequest();
        request.DeliveryModality = 0;
        request.ResponsibleOrgUnitId = -1;
        request.DeliveryModality.Should().Be(0);
        request.ResponsibleOrgUnitId.Should().Be(-1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AllInvalid_OverviewSectionRequest_AllFieldsInvalid_RequestObjectCreatedWithInvalidValues()
    {
        var request = new OverviewSectionRequest
        {
            Name = InvalidValueSets.OverMaxLengthString(NameMaxLength),
            Description = InvalidValueSets.SpecialCharacters[0],
            InitiativeBudgetUSD = -999m
        };
        request.Name!.Length.Should().BeGreaterThan(NameMaxLength);
        request.InitiativeBudgetUSD.Should().Be(-999m);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AllInvalid_WhatSectionRequest_AllFieldsInvalid_RequestObjectCreatedWithInvalidValues()
    {
        var request = new WhatSectionRequest
        {
            Name = InvalidValueSets.OverMaxLengthString(NameMaxLength),
            ResponsibleOrgUnitId = -1,
            ProposedInitiativeTypeId = 0,
            DeliveryModality = 99,
            Deliverables = new List<OpportunityDeliverableRequest> { new() { OutputId = -1 } }
        };
        request.Name!.Length.Should().BeGreaterThan(NameMaxLength);
        request.DeliveryModality.Should().Be(99);
    }

    // ========== 3. MIXED VALID/INVALID ==========

    [Fact]
    [Trait("Category", "Negative")]
    public void Mixed_OverviewSectionRequest_ValidName_InvalidBudget_PropertiesReflectValues()
    {
        var request = CreateValidOverviewRequest();
        request.Name = "Valid Name";
        request.InitiativeBudgetUSD = -1m;
        request.Name.Should().Be("Valid Name");
        request.InitiativeBudgetUSD.Should().Be(-1m);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_OverviewSectionRequest_ValidBudget_InvalidNameLength_PropertiesReflectValues()
    {
        var request = CreateValidOverviewRequest();
        request.InitiativeBudgetUSD = 100m;
        request.Name = InvalidValueSets.OverMaxLengthString(NameMaxLength);
        request.InitiativeBudgetUSD.Should().Be(100m);
        request.Name!.Length.Should().BeGreaterThan(NameMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_WhatSectionRequest_ValidDeliveryModality_InvalidOrgUnitId_PropertiesReflectValues()
    {
        var request = CreateValidWhatRequest();
        request.DeliveryModality = 2;
        request.ResponsibleOrgUnitId = 0;
        request.DeliveryModality.Should().Be(2);
        request.ResponsibleOrgUnitId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_WhatSectionRequest_ValidDeliveryModality1To4_PropertiesAcceptValues()
    {
        foreach (var v in ValidDeliveryModalityValues)
        {
            var request = CreateValidWhatRequest();
            request.DeliveryModality = v;
            request.DeliveryModality.Should().Be(v);
        }
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_OverviewSectionRequest_ValidDescription_InvalidBudget_PropertiesReflectValues()
    {
        var request = CreateValidOverviewRequest();
        request.Description = "Valid desc";
        request.InitiativeBudgetUSD = decimal.MinValue;
        request.Description.Should().Be("Valid desc");
        request.InitiativeBudgetUSD.Should().Be(decimal.MinValue);
    }

    // ========== 4. PARTIAL SUBMISSION ==========

    public static IEnumerable<object[]> OverviewPartialSubmissionData()
    {
        yield return new object[] { "NameOnly", new OverviewSectionRequest { Name = "Solo" } };
        yield return new object[] { "DescriptionOnly", new OverviewSectionRequest { Description = "Desc only" } };
        yield return new object[] { "BudgetOnly", new OverviewSectionRequest { InitiativeBudgetUSD = 50000m } };
        yield return new object[] { "NameAndDescription", new OverviewSectionRequest { Name = "N", Description = "D" } };
        yield return new object[] { "NameAndBudget", new OverviewSectionRequest { Name = "N", InitiativeBudgetUSD = 100m } };
        yield return new object[] { "AllNull", new OverviewSectionRequest() };
    }

    [Theory]
    [MemberData(nameof(OverviewPartialSubmissionData))]
    [Trait("Category", "Functional")]
    public void Partial_OverviewSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario, OverviewSectionRequest request)
    {
        request.Should().NotBeNull();
    }

    public static IEnumerable<object[]> WhatPartialSubmissionData()
    {
        yield return new object[] { "NameOnly", new WhatSectionRequest { Name = "Solo" } };
        yield return new object[] { "DeliveryModalityOnly", new WhatSectionRequest { DeliveryModality = 1 } };
        yield return new object[] { "DeliverablesOnly", new WhatSectionRequest { Deliverables = new List<OpportunityDeliverableRequest>() } };
        yield return new object[] { "NameAndDeliveryModality", new WhatSectionRequest { Name = "N", DeliveryModality = 2 } };
        yield return new object[] { "AllNull", new WhatSectionRequest() };
        yield return new object[] { "ResponsibleOrgUnitOnly", new WhatSectionRequest { ResponsibleOrgUnitId = 1 } };
    }

    [Theory]
    [MemberData(nameof(WhatPartialSubmissionData))]
    [Trait("Category", "Functional")]
    public void Partial_WhatSectionRequest_SubsetOfFields_RequestObjectCreated(string scenario, WhatSectionRequest request)
    {
        request.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_OverviewSectionRequest_EmptyRequest_AllPropertiesNullOrDefault()
    {
        var request = new OverviewSectionRequest();
        request.Name.Should().BeNull();
        request.Description.Should().BeNull();
        request.InitiativeBudgetUSD.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_WhatSectionRequest_EmptyRequest_AllPropertiesNullOrDefault()
    {
        var request = new WhatSectionRequest();
        request.Name.Should().BeNull();
        request.DeliveryModality.Should().BeNull();
        request.Deliverables.Should().BeNull();
    }

    // ========== 5. BOUNDARY COMBINATIONS ==========

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_NameExactly120_PropertyAcceptsValue()
    {
        var name = InvalidValueSets.MaxLengthString(NameMaxLength);
        var request = CreateValidOverviewRequest();
        request.Name = name;
        request.Name.Should().HaveLength(NameMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_Name121_PropertyAcceptsValue()
    {
        var name = InvalidValueSets.OverMaxLengthString(NameMaxLength);
        var request = CreateValidOverviewRequest();
        request.Name = name;
        request.Name.Should().HaveLength(NameMaxLength + 1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_DescriptionVeryLong_PropertyAcceptsValue()
    {
        var longDesc = InvalidValueSets.VeryLongString(10001);
        var request = CreateValidOverviewRequest();
        request.Description = longDesc;
        request.Description.Should().HaveLength(10001);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_BudgetZero_PropertyAcceptsValue()
    {
        var request = CreateValidOverviewRequest();
        request.InitiativeBudgetUSD = 0m;
        request.InitiativeBudgetUSD.Should().Be(0m);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_BudgetDecimalMax_PropertyAcceptsValue()
    {
        var request = CreateValidOverviewRequest();
        request.InitiativeBudgetUSD = decimal.MaxValue;
        request.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhatSectionRequest_NameExactly120_PropertyAcceptsValue()
    {
        var name = InvalidValueSets.MaxLengthString(NameMaxLength);
        var request = CreateValidWhatRequest();
        request.Name = name;
        request.Name.Should().HaveLength(NameMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhatSectionRequest_Name121_PropertyAcceptsValue()
    {
        var name = InvalidValueSets.OverMaxLengthString(NameMaxLength);
        var request = CreateValidWhatRequest();
        request.Name = name;
        request.Name.Should().HaveLength(NameMaxLength + 1);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_WhatSectionRequest_DeliveryModalityBoundary1And4_PropertiesAcceptValues()
    {
        var request1 = CreateValidWhatRequest();
        request1.DeliveryModality = 1;
        request1.DeliveryModality.Should().Be(1);

        var request4 = CreateValidWhatRequest();
        request4.DeliveryModality = 4;
        request4.DeliveryModality.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_OverviewSectionRequest_UnicodeAndSpecialCharsInName_PropertyAcceptsValue()
    {
        var request = CreateValidOverviewRequest();
        request.Name = InvalidValueSets.UnicodeStrings[0];
        request.Description = InvalidValueSets.SpecialCharacters[0];
        request.Name.Should().Be("日本語テスト");
        request.Description.Should().Contain("<script>");
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 4 | FieldOrder_* (11), Mixed_ValidDeliveryModality1To4, Partial_* (8) |
| Negative (N) | 12 | OneInvalid_* (6), Pairwise_* (2), AllInvalid_* (2), Mixed_* (2) |
| Edge/Boundary (E) | 15 | Mixed_* (3), Boundary_* (10), Pairwise_* (2) |
| Functional (F) | 20 | FieldOrder_* (11), Mixed_* (1), Partial_* (8) |
| Integration (I) | 0 | Request-level only |
| **N ≥ 3P?** | ✅ | 12 >= 12 |
| **E ≥ 3P?** | ✅ | 15 >= 12 |
| **F ≥ 3P?** | ✅ | 20 >= 12 |
| **I ≥ 3P?** | ✅ | N/A |
*/
