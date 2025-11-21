using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Business.Services;

/// <summary>
/// Result of exchange rate conversion
/// </summary>
public class ExchangeRateResult
{
    public decimal AmountUSD { get; set; }
    public decimal ExchangeRate { get; set; }
    public DateTime ExchangeRateDate { get; set; }
    public int ExchangeRateId { get; set; }
}

/// <summary>
/// Service for currency exchange rate operations
/// </summary>
public interface IExchangeRateService
{
    Task<ExchangeRateResult> ConvertToUSDAsync(decimal amount, string currencyCode, DateTime? asOfDate = null);
    Task<decimal> GetExchangeRateAsync(string fromCurrency, DateTime? asOfDate = null);
}

/// <summary>
/// Implementation of exchange rate service
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private readonly AppDbContext _context;

    public ExchangeRateService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Convert an amount to USD using the exchange rate
    /// </summary>
    public async Task<ExchangeRateResult> ConvertToUSDAsync(
        decimal amount, 
        string currencyCode, 
        DateTime? asOfDate = null)
    {
        // If already USD, no conversion needed
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.ToUpper() == "USD")
        {
            return new ExchangeRateResult
            {
                AmountUSD = amount,
                ExchangeRate = 1.0m,
                ExchangeRateDate = DateTime.UtcNow,
                ExchangeRateId = 0
            };
        }

        var effectiveDate = asOfDate ?? DateTime.UtcNow;
        
        // Get the most recent exchange rate for the currency on or before the effective date
        var exchangeRate = await _context.ExchangeRates
            .Where(er => er.Currency == currencyCode.ToUpper() 
                      && er.Effective_Date <= effectiveDate
                      && !er.IsDeleted)
            .OrderByDescending(er => er.Effective_Date)
            .ThenByDescending(er => er.Exchange_Rate_Sequence_No)
            .FirstOrDefaultAsync();

        if (exchangeRate == null || exchangeRate.Exchange_Rate == null)
        {
            throw new Exception($"Exchange rate not found for currency {currencyCode} as of {effectiveDate:yyyy-MM-dd}");
        }

        var amountUSD = amount * exchangeRate.Exchange_Rate.Value;

        // Ensure the date is in UTC format for PostgreSQL
        var rateDate = exchangeRate.Effective_Date ?? DateTime.UtcNow;
        var exchangeRateDate = rateDate.Kind == DateTimeKind.Utc 
            ? rateDate 
            : DateTime.SpecifyKind(rateDate, DateTimeKind.Utc);

        return new ExchangeRateResult
        {
            AmountUSD = Math.Round(amountUSD, 2),
            ExchangeRate = exchangeRate.Exchange_Rate.Value,
            ExchangeRateDate = exchangeRateDate,
            ExchangeRateId = exchangeRate.Id
        };
    }

    /// <summary>
    /// Get exchange rate for a currency
    /// </summary>
    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, DateTime? asOfDate = null)
    {
        var result = await ConvertToUSDAsync(1.0m, fromCurrency, asOfDate);
        return result.ExchangeRate;
    }
}

