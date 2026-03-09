/**
 * @fileoverview Comprehensive unit tests for ExchangeRateService
 * Tests currency conversion, exchange rate lookups, and edge cases
 * @author UNOPS Opportunity+ Test Team
 */

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Test suite for ExchangeRateService.
/// Uses InMemory database for isolation. Follows 3:1 ratio (N, E, F, I >= 3×P).
/// </summary>
public class ExchangeRateServiceTests : ServiceTestBase
{
    private readonly ExchangeRateService _service;
    private readonly AppDbContext _context;

    public ExchangeRateServiceTests()
    {
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_ExchangeRate_{Guid.NewGuid()}")
            .Options;
        _context = TestDbContextFactory.CreateUNOPS(options);
        _service = new ExchangeRateService(_context);
    }

    #region Positive Tests (P)

    [Fact]
    public async Task ConvertToUSDAsync_ConvertsEURCorrectly_UsingExchangeRate()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR");

        result.AmountUSD.Should().Be(118.00m);
        result.ExchangeRate.Should().Be(1.18m);
        result.ExchangeRateId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetExchangeRateAsync_ReturnsCorrectRate_ForValidCurrency()
    {
        SeedExchangeRate("GBP", 1.27m, DateTime.UtcNow.Date, 1);

        var rate = await _service.GetExchangeRateAsync("GBP");

        rate.Should().Be(1.27m);
    }

    #endregion

    #region Negative Tests (N)

    [Fact]
    public async Task ConvertToUSDAsync_UnknownCurrency_Throws()
    {
        Func<Task> act = () => _service.ConvertToUSDAsync(100m, "XXX");
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Exchange rate not found*XXX*");
    }

    [Fact]
    public async Task ConvertToUSDAsync_CurrencyWithNoRateBeforeDate_Throws()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date.AddDays(10), 1);

        var asOfDate = DateTime.UtcNow.Date;

        Func<Task> act = () => _service.ConvertToUSDAsync(100m, "EUR", asOfDate);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Exchange rate not found*EUR*");
    }

    [Fact]
    public async Task ConvertToUSDAsync_NullCurrencyCode_ReturnsUSDIdentity()
    {
        var result = await _service.ConvertToUSDAsync(50m, null!);

        result.AmountUSD.Should().Be(50m);
        result.ExchangeRate.Should().Be(1.0m);
        result.ExchangeRateId.Should().Be(0);
    }

    [Fact]
    public async Task ConvertToUSDAsync_EmptyCurrencyCode_ReturnsUSDIdentity()
    {
        var result = await _service.ConvertToUSDAsync(75m, "");

        result.AmountUSD.Should().Be(75m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    [Fact]
    public async Task GetExchangeRateAsync_NonexistentCurrency_Throws()
    {
        Func<Task> act = () => _service.GetExchangeRateAsync("ZZZ");
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Exchange rate not found*ZZZ*");
    }

    [Fact]
    public async Task ConvertToUSDAsync_NullExchangeRateRecord_Throws()
    {
        SeedExchangeRateWithNullRate("CHF", DateTime.UtcNow.Date, 1);

        Func<Task> act = () => _service.ConvertToUSDAsync(100m, "CHF");
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Exchange rate not found*CHF*");
    }

    [Fact]
    public async Task ConvertToUSDAsync_WhitespaceCurrencyCode_ReturnsUSDIdentity()
    {
        var result = await _service.ConvertToUSDAsync(25m, "   ");

        result.AmountUSD.Should().Be(25m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    #endregion

    #region Edge/Boundary Tests (E)

    [Fact]
    public async Task ConvertToUSDAsync_ZeroAmount_ReturnsZero()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(0m, "EUR");

        result.AmountUSD.Should().Be(0m);
        result.ExchangeRate.Should().Be(1.18m);
    }

    [Fact]
    public async Task ConvertToUSDAsync_NegativeAmount_ConvertsCorrectly()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(-100m, "EUR");

        result.AmountUSD.Should().Be(-118.00m);
        result.ExchangeRate.Should().Be(1.18m);
    }

    [Fact]
    public async Task ConvertToUSDAsync_VeryLargeAmount_HandlesPrecision()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(999999999.99m, "EUR");

        result.AmountUSD.Should().Be(1179999999.99m);
        result.ExchangeRate.Should().Be(1.18m);
    }

    [Fact]
    public async Task ConvertToUSDAsync_MultipleRates_UsesMostRecent()
    {
        var baseDate = DateTime.UtcNow.Date.AddDays(-10);
        SeedExchangeRate("EUR", 1.10m, baseDate, 1);
        SeedExchangeRate("EUR", 1.18m, baseDate.AddDays(5), 1);
        SeedExchangeRate("EUR", 1.15m, baseDate.AddDays(3), 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR");

        result.ExchangeRate.Should().Be(1.18m);
        result.AmountUSD.Should().Be(118.00m);
    }

    [Fact]
    public async Task ConvertToUSDAsync_IgnoresSoftDeletedRates()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1, isDeleted: true);

        Func<Task> act = () => _service.ConvertToUSDAsync(100m, "EUR");
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Exchange rate not found*EUR*");
    }

    [Fact]
    public async Task ConvertToUSDAsync_USD_ReturnsIdentity()
    {
        var result = await _service.ConvertToUSDAsync(100m, "USD");

        result.AmountUSD.Should().Be(100m);
        result.ExchangeRate.Should().Be(1.0m);
        result.ExchangeRateId.Should().Be(0);
    }

    [Fact]
    public async Task ConvertToUSDAsync_LowercaseUSD_ReturnsIdentity()
    {
        var result = await _service.ConvertToUSDAsync(50m, "usd");

        result.AmountUSD.Should().Be(50m);
        result.ExchangeRate.Should().Be(1.0m);
    }

    #endregion

    #region Functional Tests (F)

    [Fact]
    public async Task ConvertToUSDAsync_RoundsTo2DecimalPlaces()
    {
        SeedExchangeRate("EUR", 1.18345678m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR");

        result.AmountUSD.Should().Be(118.35m);
    }

    [Fact]
    public async Task ConvertToUSDAsync_ReturnsCorrectExchangeRateDate_UTC()
    {
        var effectiveDate = new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        SeedExchangeRate("EUR", 1.18m, effectiveDate, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR", effectiveDate);

        result.ExchangeRateDate.Should().Be(effectiveDate);
        result.ExchangeRateDate.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task ConvertToUSDAsync_ReturnsCorrectExchangeRateId()
    {
        var er = SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR");

        result.ExchangeRateId.Should().Be(er.Id);
    }

    [Fact]
    public async Task ConvertToUSDAsync_SameDate_SelectsBySequenceNumber()
    {
        var sameDate = DateTime.UtcNow.Date;
        SeedExchangeRate("JPY", 0.0067m, sameDate, 1);
        var higherSeq = SeedExchangeRate("JPY", 0.0068m, sameDate, 2);

        var result = await _service.ConvertToUSDAsync(1000m, "JPY");

        result.ExchangeRate.Should().Be(0.0068m);
        result.ExchangeRateId.Should().Be(higherSeq.Id);
    }

    [Fact]
    public async Task GetExchangeRateAsync_DelegatesToConvertToUSDAsync()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var rate = await _service.GetExchangeRateAsync("EUR");
        var convertResult = await _service.ConvertToUSDAsync(1.0m, "EUR");

        rate.Should().Be(convertResult.ExchangeRate);
    }

    [Fact]
    public async Task ConvertToUSDAsync_HandlesCaseInsensitiveCurrencyCodes()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var resultLower = await _service.ConvertToUSDAsync(100m, "eur");
        var resultUpper = await _service.ConvertToUSDAsync(100m, "EUR");
        var resultMixed = await _service.ConvertToUSDAsync(100m, "Eur");

        resultLower.AmountUSD.Should().Be(118.00m);
        resultUpper.AmountUSD.Should().Be(118.00m);
        resultMixed.AmountUSD.Should().Be(118.00m);
    }

    #endregion

    #region Integration Tests (I)

    [Fact]
    public async Task FullConversionFlow_AddRatesConvert_VerifyResult()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);
        await _context.SaveChangesAsync();

        var result = await _service.ConvertToUSDAsync(500m, "EUR");

        result.AmountUSD.Should().Be(590.00m);
        result.ExchangeRate.Should().Be(1.18m);
        result.ExchangeRateId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MultipleCurrenciesInDB_CorrectOneSelected()
    {
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);
        SeedExchangeRate("GBP", 1.27m, DateTime.UtcNow.Date, 1);
        SeedExchangeRate("JPY", 0.0067m, DateTime.UtcNow.Date, 1);

        var eurResult = await _service.ConvertToUSDAsync(100m, "EUR");
        var gbpResult = await _service.ConvertToUSDAsync(100m, "GBP");
        var jpyResult = await _service.ConvertToUSDAsync(10000m, "JPY");

        eurResult.AmountUSD.Should().Be(118.00m);
        gbpResult.AmountUSD.Should().Be(127.00m);
        jpyResult.AmountUSD.Should().Be(67.00m);
    }

    [Fact]
    public async Task HistoricalRateUsed_WhenAsOfDateSpecified()
    {
        var pastDate = DateTime.UtcNow.Date.AddDays(-30);
        var recentDate = DateTime.UtcNow.Date;
        SeedExchangeRate("EUR", 1.10m, pastDate, 1);
        SeedExchangeRate("EUR", 1.18m, recentDate, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR", pastDate.AddDays(1));

        result.ExchangeRate.Should().Be(1.10m);
        result.AmountUSD.Should().Be(110.00m);
    }

    [Fact]
    public async Task LatestRateUsed_WhenAsOfDateIsNull()
    {
        var pastDate = DateTime.UtcNow.Date.AddDays(-30);
        SeedExchangeRate("EUR", 1.10m, pastDate, 1);
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR", null);

        result.ExchangeRate.Should().Be(1.18m);
        result.AmountUSD.Should().Be(118.00m);
    }

    [Fact]
    public async Task RateWithFutureDate_NotUsedForPastConversion()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(10);
        SeedExchangeRate("EUR", 1.25m, futureDate, 1);
        SeedExchangeRate("EUR", 1.18m, DateTime.UtcNow.Date, 1);

        var result = await _service.ConvertToUSDAsync(100m, "EUR", DateTime.UtcNow.Date);

        result.ExchangeRate.Should().Be(1.18m);
    }

    [Fact]
    public async Task MultipleRatesSameCurrency_SequenceNumberTiebreak()
    {
        var sameDate = DateTime.UtcNow.Date;
        SeedExchangeRate("CAD", 0.72m, sameDate, 1);
        SeedExchangeRate("CAD", 0.74m, sameDate, 3);
        var highestSeq = SeedExchangeRate("CAD", 0.73m, sameDate, 5);

        var result = await _service.ConvertToUSDAsync(100m, "CAD");

        result.ExchangeRate.Should().Be(0.73m);
        result.ExchangeRateId.Should().Be(highestSeq.Id);
    }

    #endregion

    #region Helpers

    private ExchangeRate SeedExchangeRate(
        string currency,
        decimal rate,
        DateTime effectiveDate,
        int sequenceNo,
        bool isDeleted = false)
    {
        var entity = new ExchangeRate
        {
            Currency = currency,
            Exchange_Rate = rate,
            Effective_Date = effectiveDate,
            Exchange_Rate_Sequence_No = sequenceNo,
            Name = $"{currency} - Rate: {rate}",
            IsDeleted = isDeleted
        };
        _context.ExchangeRates.Add(entity);
        _context.SaveChanges();
        return entity;
    }

    private void SeedExchangeRateWithNullRate(string currency, DateTime effectiveDate, int sequenceNo)
    {
        _context.ExchangeRates.Add(new ExchangeRate
        {
            Currency = currency,
            Exchange_Rate = null,
            Effective_Date = effectiveDate,
            Exchange_Rate_Sequence_No = sequenceNo,
            Name = $"{currency} - No Rate",
            IsDeleted = false
        });
        _context.SaveChanges();
    }

    #endregion

    /*
    ### 3:1 Ratio Compliance Check
    | Category | Count | Tests |
    |----------|-------|-------|
    | Positive (P) | 2 | ConvertToUSDAsync_ConvertsEURCorrectly_UsingExchangeRate, GetExchangeRateAsync_ReturnsCorrectRate_ForValidCurrency |
    | Negative (N) | 8 | ConvertToUSDAsync_UnknownCurrency_Throws, ConvertToUSDAsync_CurrencyWithNoRateBeforeDate_Throws, ConvertToUSDAsync_NullCurrencyCode_ReturnsUSDIdentity, ConvertToUSDAsync_EmptyCurrencyCode_ReturnsUSDIdentity, GetExchangeRateAsync_NonexistentCurrency_Throws, ConvertToUSDAsync_NullExchangeRateRecord_Throws, ConvertToUSDAsync_WhitespaceCurrencyCode_ReturnsUSDIdentity |
    | Edge/Boundary (E) | 8 | ConvertToUSDAsync_ZeroAmount_ReturnsZero, ConvertToUSDAsync_NegativeAmount_ConvertsCorrectly, ConvertToUSDAsync_VeryLargeAmount_HandlesPrecision, ConvertToUSDAsync_MultipleRates_UsesMostRecent, ConvertToUSDAsync_IgnoresSoftDeletedRates, ConvertToUSDAsync_USD_ReturnsIdentity, ConvertToUSDAsync_LowercaseUSD_ReturnsIdentity |
    | Functional (F) | 6 | ConvertToUSDAsync_RoundsTo2DecimalPlaces, ConvertToUSDAsync_ReturnsCorrectExchangeRateDate_UTC, ConvertToUSDAsync_ReturnsCorrectExchangeRateId, ConvertToUSDAsync_SameDate_SelectsBySequenceNumber, GetExchangeRateAsync_DelegatesToConvertToUSDAsync, ConvertToUSDAsync_HandlesCaseInsensitiveCurrencyCodes |
    | Integration (I) | 6 | FullConversionFlow_AddRatesConvert_VerifyResult, MultipleCurrenciesInDB_CorrectOneSelected, HistoricalRateUsed_WhenAsOfDateSpecified, LatestRateUsed_WhenAsOfDateIsNull, RateWithFutureDate_NotUsedForPastConversion, MultipleRatesSameCurrency_SequenceNumberTiebreak |
    | **N ≥ 3P?** | ✅ | 8 >= 6 |
    | **E ≥ 3P?** | ✅ | 8 >= 6 |
    | **F ≥ 3P?** | ✅ | 6 >= 6 |
    | **I ≥ 3P?** | ✅ | 6 >= 6 |
    */
}
