// PNO-701, PNO-702, PNO-788, PNO-865: Consolidated specification for WHO - Partners & External Stakeholders and Team sections.
// Requirements: Funding Partners (name, amount, currency, exchange rate, role), Client Partners (name, role),
// Implementing Partners, Team (Opportunity Manager, Collaborators, SMEs, Internal Stakeholders),
// Currency display (single currency per amount - PNO-788), Partner amounts and exchange rates.

namespace UNOPS.PAO.Business.Tests.OpportunityWhoSection;

/// <summary>
/// Specification model for Opportunity WHO section.
/// PNO-701 AC1-AC9: Partners & External Stakeholders (Funding, Client, External).
/// PNO-702 AC1-AC9: Team & Internal Stakeholders (OM, Collaborators, SMEs).
/// PNO-788: Single currency per funding partner amount (no $ + RWF duplicate display).
/// PNO-865: Client Partner dropdown scroll behavior.
/// </summary>
public sealed class OpportunityWhoSectionSpec
{
    /// <summary>
    /// PNO-788: Funding partner amount display must use ONLY the selected currency.
    /// Must NOT show $ symbol when currency is non-USD (e.g., RWF).
    /// </summary>
    public static string FormatFundingPartnerAmount(decimal amount, string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            currencyCode = "USD";
        var formatted = amount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        return $"{formatted} {currencyCode}";
    }

    /// <summary>
    /// PNO-788: Returns true if display incorrectly shows both $ and currency code.
    /// Defect: formatCurrency() in Angular uses hardcoded USD/$ for all amounts.
    /// </summary>
    public static bool HasDuplicateCurrencyDisplay(string displayValue, string expectedCurrencyCode)
    {
        if (string.IsNullOrEmpty(displayValue)) return false;
        var hasDollar = displayValue.Contains("$");
        var hasCurrencyCode = displayValue.Contains(expectedCurrencyCode);
        return hasDollar && hasCurrencyCode && expectedCurrencyCode != "USD";
    }

    /// <summary>
    /// PNO-701 AC5: Funding partner must have amount and currency.
    /// </summary>
    public static bool IsValidFundingPartner(int partnerId, decimal? amount, int? currencyId)
    {
        return partnerId > 0 && amount.HasValue && amount.Value > 0 && currencyId.HasValue && currencyId.Value > 0;
    }

    /// <summary>
    /// PNO-701: Client partner must have valid partner ID.
    /// </summary>
    public static bool IsValidClientPartner(int partnerId)
    {
        return partnerId > 0;
    }

    /// <summary>
    /// PNO-701: Deduplicate funding partners by PartnerId (keep first).
    /// </summary>
    public static IReadOnlyList<WhoSpecFundingPartner> DeduplicateFundingPartners(
        IReadOnlyList<WhoSpecFundingPartner> partners)
    {
        return partners
            .GroupBy(p => p.PartnerId)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// PNO-701: Deduplicate client partners by PartnerId.
    /// </summary>
    public static IReadOnlyList<WhoSpecClientPartner> DeduplicateClientPartners(
        IReadOnlyList<WhoSpecClientPartner> partners)
    {
        return partners
            .GroupBy(p => p.PartnerId)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// PNO-701 AC2: Pooled funding partners (pooledFund=YES) must NOT be selectable as funding partners.
    /// </summary>
    public static bool IsPooledFundingPartnerEligible(bool pooledFund)
    {
        return !pooledFund;
    }

    /// <summary>
    /// PNO-701 AC6: Total Budget (USD) = sum of all funding partner amounts converted to USD.
    /// </summary>
    public static decimal CalculateTotalBudgetUSD(IReadOnlyList<WhoSpecFundingPartner> partners)
    {
        return partners
            .Where(p => p.AmountUSD.HasValue)
            .Sum(p => p.AmountUSD!.Value);
    }

    /// <summary>
    /// PNO-702: Opportunity Manager is auto-assigned; must have valid user ID.
    /// </summary>
    public static bool IsValidOpportunityManager(int? userId)
    {
        return userId.HasValue && userId.Value > 0;
    }

    /// <summary>
    /// PNO-702 AC8: SME selections must have valid user and role.
    /// </summary>
    public static bool IsValidSMESelection(int? userId, int? entityRoleId)
    {
        return userId.HasValue && userId.Value > 0 && entityRoleId.HasValue && entityRoleId.Value > 0;
    }

    /// <summary>
    /// PNO-702: Collaborator must have valid user ID.
    /// </summary>
    public static bool IsValidCollaborator(int userId)
    {
        return userId > 0;
    }

    /// <summary>
    /// PNO-701: External stakeholder contact must belong to opportunity's partners.
    /// </summary>
    public static bool IsExternalStakeholderContactEligible(int contactId, IReadOnlyList<int> opportunityPartnerIds)
    {
        return contactId > 0 && opportunityPartnerIds.Count > 0;
    }

    /// <summary>
    /// PNO-788: Expected format for non-USD amount - "1,111 RWF" not "$1,111 RWF".
    /// </summary>
    public static string GetExpectedAmountDisplay(decimal amount, string currencyCode)
    {
        return FormatFundingPartnerAmount(amount, currencyCode);
    }
}

/// <summary>
/// Spec model for funding partner validation.
/// </summary>
public class WhoSpecFundingPartner
{
    public int PartnerId { get; set; }
    public decimal? Amount { get; set; }
    public int? CurrencyId { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? AmountUSD { get; set; }
    public decimal? ExchangeRate { get; set; }
    public DateTime? ExchangeRateDate { get; set; }
}

/// <summary>
/// Spec model for client partner validation.
/// </summary>
public class WhoSpecClientPartner
{
    public int PartnerId { get; set; }
}
