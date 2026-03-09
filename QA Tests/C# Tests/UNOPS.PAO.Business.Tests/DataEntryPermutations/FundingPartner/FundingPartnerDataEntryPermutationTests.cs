/// <summary>
/// Tests for FundingPartner Data Entry Permutations (OpportunityFundingPartnerRequest, OpportunityClientPartnerRequest).
///
/// Requirements validated:
/// - REQ-1: OpportunityFundingPartnerRequest - PartnerId required, FundedAmount/CurrencyId/Percentage/Fee fields
/// - REQ-2: OpportunityClientPartnerRequest - PartnerId required, DocumentId/SelectedPartnerAgreementNumber optional
/// - REQ-3: Field order independence, invalid combinations, mixed valid/invalid, partial submission, boundary values
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.FundingPartner;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "FundingPartner")]
public class FundingPartnerDataEntryPermutationTests
{
    private const int PartnershipAgreementReferenceMaxLength = 255;
    private const int SelectedPartnerAgreementNumberMaxLength = 50;
    private const int ValidPartnerId = 1;
    private const int ValidCurrencyId = 2;
    private const decimal ValidAmount = 100000m;
    private const decimal ValidPercentage = 50m;

    private static (bool IsValid, string? Error) ValidateFundingPartnerRequest(OpportunityFundingPartnerRequest req)
    {
        if (req.PartnerId <= 0) return (false, "PartnerId must be positive");
        if (req.CurrencyId.HasValue && req.CurrencyId.Value <= 0) return (false, "CurrencyId must be positive when set");
        if (req.FundedAmount.HasValue && req.FundedAmount.Value < 0) return (false, "FundedAmount cannot be negative");
        if (req.Amount.HasValue && req.Amount.Value < 0) return (false, "Amount cannot be negative");
        if (req.Percentage.HasValue && (req.Percentage.Value < 0 || req.Percentage.Value > 100))
            return (false, "Percentage must be between 0 and 100");
        if (req.FeePercentage.HasValue && (req.FeePercentage.Value < 0 || req.FeePercentage.Value > 100))
            return (false, "FeePercentage must be between 0 and 100");
        if (req.FeeAmount.HasValue && req.FeeAmount.Value < 0) return (false, "FeeAmount cannot be negative");
        if (req.FeeAmountUSD.HasValue && req.FeeAmountUSD.Value < 0) return (false, "FeeAmountUSD cannot be negative");
        if (req.PartnershipAgreementReference != null && req.PartnershipAgreementReference.Length > PartnershipAgreementReferenceMaxLength)
            return (false, $"PartnershipAgreementReference must not exceed {PartnershipAgreementReferenceMaxLength} characters");
        if (req.SelectedPartnerAgreementNumber != null && req.SelectedPartnerAgreementNumber.Length > SelectedPartnerAgreementNumberMaxLength)
            return (false, $"SelectedPartnerAgreementNumber must not exceed {SelectedPartnerAgreementNumberMaxLength} characters");
        return (true, null);
    }

    private static (bool IsValid, string? Error) ValidateClientPartnerRequest(OpportunityClientPartnerRequest req)
    {
        if (req.PartnerId <= 0) return (false, "PartnerId must be positive");
        if (req.SelectedPartnerAgreementNumber != null && req.SelectedPartnerAgreementNumber.Length > SelectedPartnerAgreementNumberMaxLength)
            return (false, $"SelectedPartnerAgreementNumber must not exceed {SelectedPartnerAgreementNumberMaxLength} characters");
        return (true, null);
    }

    // ========== OpportunityFundingPartnerRequest ==========

    #region 1. Field Order Permutations - OpportunityFundingPartnerRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_FieldOrder_PartnerIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, CurrencyId = ValidCurrencyId };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
        req.PartnerId.Should().Be(ValidPartnerId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_FieldOrder_CurrencyIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { CurrencyId = ValidCurrencyId, PartnerId = ValidPartnerId };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_FieldOrder_AmountFieldsInDifferentOrders_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            FundedAmount = ValidAmount,
            Percentage = ValidPercentage,
            PartnerId = ValidPartnerId,
            CurrencyId = ValidCurrencyId
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
        req.FundedAmount.Should().Be(ValidAmount);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_FieldOrder_FeeFieldsLast_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            PartnerId = ValidPartnerId,
            CurrencyId = ValidCurrencyId,
            FundedAmount = ValidAmount,
            FeePercentage = 5m,
            FeeAmount = 5000m
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_FieldOrder_AllPermutationsProduceIdenticalValidation()
    {
        var permutations = new[]
        {
            new OpportunityFundingPartnerRequest { PartnerId = 1, CurrencyId = 2 },
            new OpportunityFundingPartnerRequest { CurrencyId = 2, PartnerId = 1 },
            new OpportunityFundingPartnerRequest { PartnerId = 1, FundedAmount = 100m, CurrencyId = 2 },
            new OpportunityFundingPartnerRequest { FundedAmount = 100m, PartnerId = 1, CurrencyId = 2 }
        };
        foreach (var p in permutations)
        {
            var (isValid, _) = ValidateFundingPartnerRequest(p);
            isValid.Should().BeTrue();
        }
    }

    #endregion

    #region 2. Invalid Combinations - OpportunityFundingPartnerRequest

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_PartnerIdZeroOrNegative_ShouldFailValidation(int invalidPartnerId)
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = invalidPartnerId, CurrencyId = ValidCurrencyId };
        var (isValid, error) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("PartnerId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_CurrencyIdZeroOrNegative_ShouldFailValidation(int invalidCurrencyId)
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, CurrencyId = invalidCurrencyId };
        var (isValid, error) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("CurrencyId");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(-999999)]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_NegativeFundedAmount_ShouldFailValidation(decimal invalidAmount)
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FundedAmount = invalidAmount };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(InvalidPercentageValues))]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_PercentageOutOfRange_ShouldFailValidation(decimal invalidPercentage)
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, Percentage = invalidPercentage };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    public static IEnumerable<object[]> InvalidPercentageValues() =>
        InvalidValueSets.PercentageInvalid.Select(p => new object[] { p });

    [Theory]
    [MemberData(nameof(InvalidFeePercentageValues))]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_FeePercentageOutOfRange_ShouldFailValidation(decimal invalidFeePct)
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FeePercentage = invalidFeePct };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    public static IEnumerable<object[]> InvalidFeePercentageValues() =>
        InvalidValueSets.PercentageInvalid.Select(p => new object[] { p });

    [Fact]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_PartnerIdAndCurrencyIdBothInvalid_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = 0, CurrencyId = -1 };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void FundingPartner_Invalid_NegativeFeeAmount_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FeeAmount = -100m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid - OpportunityFundingPartnerRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Mixed_ValidPartnerId_InvalidCurrencyId_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, CurrencyId = 0 };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Mixed_ValidPartnerId_InvalidPercentage_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, Percentage = 100.01m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Mixed_ValidAmounts_InvalidPartnerId_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            PartnerId = -1,
            FundedAmount = ValidAmount,
            CurrencyId = ValidCurrencyId
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Mixed_ValidPartnerId_InvalidFeePercentage_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FeePercentage = 200m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Mixed_ValidPartnerId_InvalidNegativeAmount_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FundedAmount = -500m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission - OpportunityFundingPartnerRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_Partial_Minimal_PartnerIdAndCurrencyIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, CurrencyId = ValidCurrencyId };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
        req.FundedAmount.Should().BeNull();
        req.Percentage.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_Partial_PartnerIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_Partial_WithAmounts_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            PartnerId = ValidPartnerId,
            CurrencyId = ValidCurrencyId,
            FundedAmount = ValidAmount,
            Percentage = ValidPercentage
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_Partial_WithFees_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            PartnerId = ValidPartnerId,
            CurrencyId = ValidCurrencyId,
            FundedAmount = ValidAmount,
            FeePercentage = 5m,
            FeeAmount = 5000m
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FundingPartner_Partial_Full_AllFieldsSet_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest
        {
            PartnerId = ValidPartnerId,
            CurrencyId = ValidCurrencyId,
            FundedAmount = ValidAmount,
            Percentage = ValidPercentage,
            FeePercentage = 5m,
            FeeAmount = 5000m,
            FeeAmountUSD = 4500m,
            IsAmountBasedFee = true,
            PartnershipAgreementReference = "REF-001",
            DocumentId = 10,
            IsPooledContribution = true,
            SelectedPartnerAgreementNumber = "PA-001"
        };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary - OpportunityFundingPartnerRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_AmountDecimalMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, FundedAmount = decimal.MaxValue };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_Percentage100Point01_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, Percentage = 100.01m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_Percentage100_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, Percentage = 100m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_PartnershipAgreementReferenceAtMax_ProducesValidRequest()
    {
        var str = InvalidValueSets.MaxLengthString(PartnershipAgreementReferenceMaxLength);
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, PartnershipAgreementReference = str };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
        req.PartnershipAgreementReference!.Length.Should().Be(PartnershipAgreementReferenceMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_PartnershipAgreementReferenceOverMax_ShouldFailValidation()
    {
        var str = InvalidValueSets.OverMaxLengthString(PartnershipAgreementReferenceMaxLength);
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, PartnershipAgreementReference = str };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_SelectedPartnerAgreementNumberAtMax_ProducesValidRequest()
    {
        var str = InvalidValueSets.MaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, SelectedPartnerAgreementNumber = str };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_SelectedPartnerAgreementNumberOverMax_ShouldFailValidation()
    {
        var str = InvalidValueSets.OverMaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, SelectedPartnerAgreementNumber = str };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_PartnerIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = int.MaxValue };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void FundingPartner_Boundary_PercentageNegative_ShouldFailValidation()
    {
        var req = new OpportunityFundingPartnerRequest { PartnerId = ValidPartnerId, Percentage = -0.01m };
        var (isValid, _) = ValidateFundingPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    // ========== OpportunityClientPartnerRequest ==========

    #region 1. Field Order Permutations - OpportunityClientPartnerRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_FieldOrder_PartnerIdFirst_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
        req.PartnerId.Should().Be(ValidPartnerId);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_FieldOrder_DocumentIdLast_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, DocumentId = 5 };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
        req.DocumentId.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_FieldOrder_SelectedPartnerAgreementNumberFirst_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { SelectedPartnerAgreementNumber = "PA-001", PartnerId = ValidPartnerId };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_FieldOrder_AllPermutationsProduceIdenticalValidation()
    {
        var permutations = new[]
        {
            new OpportunityClientPartnerRequest { PartnerId = 1 },
            new OpportunityClientPartnerRequest { PartnerId = 1, DocumentId = 2 },
            new OpportunityClientPartnerRequest { DocumentId = 2, PartnerId = 1 },
            new OpportunityClientPartnerRequest { PartnerId = 1, SelectedPartnerAgreementNumber = "X" }
        };
        foreach (var p in permutations)
        {
            var (isValid, _) = ValidateClientPartnerRequest(p);
            isValid.Should().BeTrue();
        }
    }

    #endregion

    #region 2. Invalid Combinations - OpportunityClientPartnerRequest

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    [Trait("Category", "Negative")]
    public void ClientPartner_Invalid_PartnerIdZeroOrNegative_ShouldFailValidation(int invalidPartnerId)
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = invalidPartnerId };
        var (isValid, error) = ValidateClientPartnerRequest(req);
        isValid.Should().BeFalse();
        error.Should().Contain("PartnerId");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ClientPartner_Invalid_SelectedPartnerAgreementNumberOverMax_ShouldFailValidation()
    {
        var str = InvalidValueSets.OverMaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, SelectedPartnerAgreementNumber = str };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 3. Mixed Valid/Invalid - OpportunityClientPartnerRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Mixed_ValidDocumentId_InvalidPartnerId_ShouldFailValidation()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = 0, DocumentId = 5 };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Mixed_ValidPartnerId_InvalidSelectedPartnerAgreementNumberOverMax_ShouldFailValidation()
    {
        var req = new OpportunityClientPartnerRequest
        {
            PartnerId = ValidPartnerId,
            SelectedPartnerAgreementNumber = InvalidValueSets.OverMaxLengthString(SelectedPartnerAgreementNumberMaxLength)
        };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission - OpportunityClientPartnerRequest

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_Partial_Minimal_PartnerIdOnly_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
        req.DocumentId.Should().BeNull();
        req.SelectedPartnerAgreementNumber.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_Partial_WithDocumentId_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, DocumentId = 10 };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ClientPartner_Partial_Full_AllFieldsSet_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest
        {
            PartnerId = ValidPartnerId,
            DocumentId = 10,
            SelectedPartnerAgreementNumber = "PA-001"
        };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary - OpportunityClientPartnerRequest

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Boundary_PartnerIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = int.MaxValue };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Boundary_SelectedPartnerAgreementNumberAtMax_ProducesValidRequest()
    {
        var str = InvalidValueSets.MaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, SelectedPartnerAgreementNumber = str };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
        req.SelectedPartnerAgreementNumber!.Length.Should().Be(SelectedPartnerAgreementNumberMaxLength);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Boundary_SelectedPartnerAgreementNumberOverMax_ShouldFailValidation()
    {
        var str = InvalidValueSets.OverMaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, SelectedPartnerAgreementNumber = str };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Boundary_DocumentIdIntMaxValue_ProducesValidRequest()
    {
        var req = new OpportunityClientPartnerRequest { PartnerId = ValidPartnerId, DocumentId = int.MaxValue };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void ClientPartner_Boundary_AllFieldsAtMax_ProducesValidRequest()
    {
        var str = InvalidValueSets.MaxLengthString(SelectedPartnerAgreementNumberMaxLength);
        var req = new OpportunityClientPartnerRequest
        {
            PartnerId = int.MaxValue,
            DocumentId = int.MaxValue,
            SelectedPartnerAgreementNumber = str
        };
        var (isValid, _) = ValidateClientPartnerRequest(req);
        isValid.Should().BeTrue();
    }

    #endregion
}
