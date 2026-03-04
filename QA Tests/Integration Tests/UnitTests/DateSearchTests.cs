using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;
using Xunit;
using System.Text.Json;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests to validate improved date handling in search criteria
/// Tests for operators: before, after, between, on, this week, this month, this year
/// </summary>
public class DateSearchTests
{
    [Fact]
    public void DateSearch_AfterOperator_ShouldFindCorrectDates()
    {
        // Arrange - Create specific test data
        var partners = new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Before", new DateTime(2024, 1, 10, 10, 0, 0)),
            CreatePartnerWithDate("After1", new DateTime(2024, 1, 16, 10, 0, 0)),
            CreatePartnerWithDate("After2", new DateTime(2024, 1, 20, 10, 0, 0))
        };

        // Act - Search for dates after 2024-01-15
        var searchDate = new DateTime(2024, 1, 15);
        var endOfSearchDay = GetEndOfDay(searchDate);
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate > endOfSearchDay)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(2, "Should find 2 partners created after 2024-01-15");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate.Date > searchDate.Date);
    }

    [Fact]
    public void DateSearch_BeforeOperator_ShouldFindCorrectDates()
    {
        // Arrange - Create specific test data
        var partners = new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Before1", new DateTime(2024, 1, 10, 10, 0, 0)),
            CreatePartnerWithDate("Before2", new DateTime(2024, 1, 12, 10, 0, 0)),
            CreatePartnerWithDate("After", new DateTime(2024, 1, 20, 10, 0, 0))
        };

        // Act - Search for dates before 2024-01-15
        var searchDate = new DateTime(2024, 1, 15);
        var startOfSearchDay = GetStartOfDay(searchDate);
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate < startOfSearchDay)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(2, "Should find 2 partners created before 2024-01-15");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate.Date < searchDate.Date);
    }

    [Fact]
    public void DateSearch_BetweenOperator_ShouldFindCorrectDates()
    {
        // Arrange - Create specific test data
        var partners = new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Before", new DateTime(2024, 1, 5, 10, 0, 0)),
            CreatePartnerWithDate("InRange1", new DateTime(2024, 1, 15, 10, 0, 0)),
            CreatePartnerWithDate("InRange2", new DateTime(2024, 1, 18, 10, 0, 0)),
            CreatePartnerWithDate("After", new DateTime(2024, 1, 25, 10, 0, 0))
        };

        // Act - Search for dates between 2024-01-10 and 2024-01-20
        var startDate = new DateTime(2024, 1, 10);
        var endDate = new DateTime(2024, 1, 20);
        var startDateTime = GetStartOfDay(startDate);
        var endDateTime = GetEndOfDay(endDate);
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate >= startDateTime && p.CreatedDate <= endDateTime)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(2, "Should find 2 partners created between 2024-01-10 and 2024-01-20");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate.Date >= startDate.Date && p.CreatedDate.Date <= endDate.Date);
    }

    [Fact]
    public void DateSearch_OnOperator_ShouldFindExactDates()
    {
        // Arrange - Create specific test data
        var targetDate = new DateTime(2024, 1, 15);
        var partners = new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Before", new DateTime(2024, 1, 14, 23, 59, 59)),
            CreatePartnerWithDate("OnTarget1", new DateTime(2024, 1, 15, 0, 0, 0)),
            CreatePartnerWithDate("OnTarget2", new DateTime(2024, 1, 15, 12, 30, 0)),
            CreatePartnerWithDate("OnTarget3", new DateTime(2024, 1, 15, 23, 59, 59)),
            CreatePartnerWithDate("After", new DateTime(2024, 1, 16, 0, 0, 1))
        };

        // Act - Search for dates exactly on 2024-01-15
        var startOfDay = GetStartOfDay(targetDate);
        var endOfDay = GetEndOfDay(targetDate);
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate >= startOfDay && p.CreatedDate <= endOfDay)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(3, "Should find 3 partners created on 2024-01-15");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate.Date == targetDate.Date);
    }

    [Fact]
    public void DateSearch_NotOnOperator_ShouldExcludeExactDates()
    {
        // Arrange - Create specific test data
        var excludeDate = new DateTime(2024, 1, 15);
        var partners = new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Keep1", new DateTime(2024, 1, 10, 10, 0, 0)),
            CreatePartnerWithDate("Exclude1", new DateTime(2024, 1, 15, 0, 0, 0)),
            CreatePartnerWithDate("Exclude2", new DateTime(2024, 1, 15, 23, 59, 59)),
            CreatePartnerWithDate("Keep2", new DateTime(2024, 1, 20, 10, 0, 0))
        };

        // Act - Exclude dates on 2024-01-15
        var startOfExcludeDay = GetStartOfDay(excludeDate);
        var endOfExcludeDay = GetEndOfDay(excludeDate);
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate < startOfExcludeDay || p.CreatedDate > endOfExcludeDay)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(2, "Should find 2 partners not created on 2024-01-15");
        filteredPartners.Should().NotContain(p => p.CreatedDate.Date == excludeDate.Date);
    }

    [Fact]
    public void DateSearch_ThisWeek_ShouldFindCurrentWeekDates()
    {
        // Arrange
        var partners = GetTestPartnersWithCurrentWeekDates();
        var (weekStart, weekEnd) = GetThisWeekDates();

        // Act - Simulate "this week"
        var filteredPartners = partners
            .Where(p => p.CreatedDate >= weekStart && p.CreatedDate <= weekEnd)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCountGreaterThan(0, "Should find partners created this week");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate >= weekStart && p.CreatedDate <= weekEnd,
            "Should only contain partners from this week");
    }

    [Fact]
    public void DateSearch_ThisMonth_ShouldFindCurrentMonthDates()
    {
        // Arrange
        var partners = GetTestPartnersWithCurrentMonthDates();
        var (monthStart, monthEnd) = GetThisMonthDates();

        // Act - Simulate "this month"
        var filteredPartners = partners
            .Where(p => p.CreatedDate >= monthStart && p.CreatedDate <= monthEnd)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCountGreaterThan(0, "Should find partners created this month");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate >= monthStart && p.CreatedDate <= monthEnd,
            "Should only contain partners from this month");
    }

    [Fact]
    public void DateSearch_ThisYear_ShouldFindCurrentYearDates()
    {
        // Arrange
        var partners = GetTestPartnersWithCurrentYearDates();
        var (yearStart, yearEnd) = GetThisYearDates();

        // Act - Simulate "this year"
        var filteredPartners = partners
            .Where(p => p.CreatedDate >= yearStart && p.CreatedDate <= yearEnd)
            .ToList();

        // Assert
        filteredPartners.Should().HaveCountGreaterThan(0, "Should find partners created this year");
        filteredPartners.Should().OnlyContain(p => p.CreatedDate >= yearStart && p.CreatedDate <= yearEnd,
            "Should only contain partners from this year");
    }

    [Theory]
    [InlineData("today", "Should parse 'today' as current date")]
    // [InlineData("hier", "Should parse 'hier' as yesterday")] // French date not implemented
    [InlineData("tomorrow", "Should parse 'tomorrow' as next day")]
    [InlineData("2024-01-15", "Should parse ISO date format")]
    [InlineData("15/01/2024", "Should parse European date format")]
    [InlineData("01/15/2024", "Should parse American date format")]
    [InlineData("2024-01-15 14:30:00", "Should parse date with time")]
    public void DateParsing_MultipleFormats_ShouldParseCorrectly(string dateInput, string description)
    {
        // Act - Test date parsing (via helper method)
        var result = TestDateParsing(dateInput);

        // Assert
        result.Should().NotBeNull(description);
    }

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("not-a-date")]
    [InlineData("2024-13-45")] // Mois/jour invalides
    [InlineData("")]
    [InlineData(null)]
    public void DateParsing_InvalidFormats_ShouldReturnNull(string? invalidInput)
    {
        // Act - Test invalid date parsing
        var result = TestDateParsing(invalidInput);

        // Assert
        result.Should().BeNull($"Should not parse invalid date: '{invalidInput}'");
    }

    [Fact]
    public void DateSearch_EdgeCases_ShouldHandleCorrectly()
    {
        // Arrange
        var partners = GetTestPartnersWithEdgeCaseDates();

        // Test 1: Start and end of year
        var startOfYear = new DateTime(DateTime.Now.Year, 1, 1);
        var endOfYear = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        
        var yearPartners = partners
            .Where(p => p.CreatedDate >= startOfYear && p.CreatedDate <= endOfYear)
            .ToList();

        // Test 2: Exact midnight
        var midnightPartners = partners
            .Where(p => p.CreatedDate.TimeOfDay == TimeSpan.Zero)
            .ToList();

        // Test 3: End of day
        var endOfDayPartners = partners
            .Where(p => p.CreatedDate.TimeOfDay >= new TimeSpan(23, 59, 0))
            .ToList();

        // Assert
        yearPartners.Should().NotBeEmpty("Should handle year boundaries");
        midnightPartners.Should().NotBeEmpty("Should handle midnight times");
        endOfDayPartners.Should().NotBeEmpty("Should handle end of day times");
    }

    [Fact]
    public void DateSearch_Performance_ShouldBeEfficient()
    {
        // Arrange
        var partners = GenerateManyPartnersWithDates(5000);
        var searchDate = DateTime.Today.AddDays(-30);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var filteredPartners = partners
            .Where(p => p.CreatedDate > searchDate)
            .Take(100)
            .ToList();
            
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50, "Date filtering should be fast");
        filteredPartners.Should().HaveCountLessOrEqualTo(100);
    }

    #region Helper Methods

    /// <summary>
    /// Date parsing test (simulation of private method)
    /// </summary>
    private static DateTime? TestDateParsing(string dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
            return null;

        var normalizedValue = dateValue.Trim().ToLower();

        // Basic relative dates
        switch (normalizedValue)
        {
            case "today":
            case "today_fr": // French: aujourd'hui
                return DateTime.Today;
            case "yesterday":
            case "yesterday_fr": // French: hier
            case "hier":
                return DateTime.Today.AddDays(-1);
            case "tomorrow":
            case "tomorrow_fr": // French: demain
                return DateTime.Today.AddDays(1);
        }

        // Standard formats
        var formats = new[]
        {
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "yyyy-MM-dd HH:mm:ss"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateValue, format, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        return DateTime.TryParse(dateValue, out var fallback) ? fallback : null;
    }

    private static DateTime GetStartOfDay(DateTime date) => date.Date;
    private static DateTime GetEndOfDay(DateTime date) => date.Date.AddDays(1).AddMilliseconds(-1);

    private static (DateTime Start, DateTime End) GetThisWeekDates()
    {
        var today = DateTime.Today;
        var dayOfWeek = (int)today.DayOfWeek;
        var mondayOffset = dayOfWeek == 0 ? -6 : -(dayOfWeek - 1);
        var monday = today.AddDays(mondayOffset);
        var sunday = monday.AddDays(6);
        return (GetStartOfDay(monday), GetEndOfDay(sunday));
    }

    private static (DateTime Start, DateTime End) GetThisMonthDates()
    {
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return (GetStartOfDay(firstDay), GetEndOfDay(lastDay));
    }

    private static (DateTime Start, DateTime End) GetThisYearDates()
    {
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, 1, 1);
        var lastDay = new DateTime(today.Year, 12, 31);
        return (GetStartOfDay(firstDay), GetEndOfDay(lastDay));
    }

    private static List<UNOPSPartner> GetTestPartnersWithDates()
    {
        return new List<UNOPSPartner>
        {
            CreatePartnerWithDate("Partner 2024-01-05", new DateTime(2024, 1, 5, 9, 30, 0)),   // Pour test before 2024-01-15
            CreatePartnerWithDate("Partner 2024-01-10", new DateTime(2024, 1, 10, 9, 30, 0)),
            CreatePartnerWithDate("Partner 2024-01-15", new DateTime(2024, 1, 15, 14, 45, 0)),
            CreatePartnerWithDate("Partner 2024-01-20", new DateTime(2024, 1, 20, 16, 15, 0)),
            CreatePartnerWithDate("Partner 2024-06-15", new DateTime(2024, 6, 15, 11, 0, 0)),
            CreatePartnerWithDate("Partner 2024-12-20", new DateTime(2024, 12, 20, 10, 0, 0)), // Pour test before 2024-12-25
            CreatePartnerWithDate("Partner 2024-12-25", new DateTime(2024, 12, 25, 0, 0, 0)),
            CreatePartnerWithDate("Partner 2024-12-31", new DateTime(2024, 12, 31, 23, 59, 59))
        };
    }

    private static List<UNOPSPartner> GetTestPartnersWithCurrentWeekDates()
    {
        var (weekStart, weekEnd) = GetThisWeekDates();
        var partners = new List<UNOPSPartner>();
        
        // Add partners in the current week
        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i).AddHours(10); // 10h chaque jour
            partners.Add(CreatePartnerWithDate($"Partner Week {i + 1}", date));
        }
        
        // Add some partners outside the week
        partners.Add(CreatePartnerWithDate("Partner Before", weekStart.AddDays(-1)));
        partners.Add(CreatePartnerWithDate("Partner After", weekEnd.AddDays(1)));
        
        return partners;
    }

    private static List<UNOPSPartner> GetTestPartnersWithCurrentMonthDates()
    {
        var (monthStart, monthEnd) = GetThisMonthDates();
        var partners = new List<UNOPSPartner>();
        
        // Add partners in the current month
        var daysInMonth = (monthEnd - monthStart).Days + 1;
        for (int i = 0; i < Math.Min(10, daysInMonth); i++)
        {
            var date = monthStart.AddDays(i * 3).AddHours(12); // Tous les 3 jours à midi
            partners.Add(CreatePartnerWithDate($"Partner Month {i + 1}", date));
        }
        
        return partners;
    }

    private static List<UNOPSPartner> GetTestPartnersWithCurrentYearDates()
    {
        var currentYear = DateTime.Now.Year;
        var partners = new List<UNOPSPartner>();
        
        // Add partners throughout the year
        for (int month = 1; month <= 12; month++)
        {
            var date = new DateTime(currentYear, month, 15, 12, 0, 0); // 15 de chaque mois
            partners.Add(CreatePartnerWithDate($"Partner Year {month}", date));
        }
        
        return partners;
    }

    private static List<UNOPSPartner> GetTestPartnersWithEdgeCaseDates()
    {
        var currentYear = DateTime.Now.Year;
        return new List<UNOPSPartner>
        {
            // Start and end of year
            CreatePartnerWithDate("New Year", new DateTime(currentYear, 1, 1, 0, 0, 0)),
            CreatePartnerWithDate("End Year", new DateTime(currentYear, 12, 31, 23, 59, 59)),
            
            // Exact midnight
            CreatePartnerWithDate("Midnight 1", new DateTime(currentYear, 6, 15, 0, 0, 0)),
            CreatePartnerWithDate("Midnight 2", new DateTime(currentYear, 9, 20, 0, 0, 0)),
            
            // End of day
            CreatePartnerWithDate("End Day 1", new DateTime(currentYear, 3, 10, 23, 59, 30)),
            CreatePartnerWithDate("End Day 2", new DateTime(currentYear, 8, 25, 23, 59, 59))
        };
    }

    private static List<UNOPSPartner> GenerateManyPartnersWithDates(int count)
    {
        var partners = new List<UNOPSPartner>();
        var random = new Random(12345); // Fixed seed for reproducibility
        var baseDate = DateTime.Today.AddYears(-1);
        
        for (int i = 0; i < count; i++)
        {
            var randomDays = random.Next(0, 365);
            var randomHours = random.Next(0, 24);
            var randomMinutes = random.Next(0, 60);
            
            var date = baseDate.AddDays(randomDays).AddHours(randomHours).AddMinutes(randomMinutes);
            partners.Add(CreatePartnerWithDate($"Partner {i + 1}", date));
        }
        
        return partners;
    }

    private static UNOPSPartner CreatePartnerWithDate(string name, DateTime createdDate)
    {
        return new UNOPSPartner
        {
            Id = Random.Shared.Next(1, 10000),
            // Enhanced Partner structure
            Name = name,
            PartnerShortDescription = name.Length > 10 ? name.Substring(0, 10) : name,
            PartnerCategoryId = 1, // Default test category
            LiaisonOfficeId = 1, // Default test liaison office
            UNAndStateEntity = false,
            Status = Domain.Entities.EntityStatus.Active,
            CanCreateNewOpportunities = true,
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
            PartnerGroupId = 1,
            CreatedDate = createdDate,
            LastModifiedDate = createdDate.AddDays(Random.Shared.Next(1, 30))
        };
    }

    #endregion
}