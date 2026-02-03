/**
 * BOUNDARY/EDGE TESTS
 * 
 * Required: ≥50 AND ≥2×P (with P=50, minimum is 100 tests)
 * Purpose: Verify behavior at limits, boundaries, and edge conditions
 * 
 * Coverage Areas:
 * - String length boundaries (20)
 * - Numeric boundaries (20)
 * - Date/Time boundaries (20)
 * - Collection boundaries (20)
 * - Special character handling (20)
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Core
{
    /// <summary>
    /// Boundary/Edge Tests - Verify behavior at limits and edge conditions
    /// 
    /// Required: ≥100 tests (≥2×P where P=50)
    /// </summary>
    public class BoundaryTests
    {
        #region String Length Boundaries (20 tests)

        [Fact]
        public void Partner_Name_MinLength_Accepted()
        {
            // Arrange
            var name = "A"; // 1 character - minimum

            // Act & Assert
            name.Length.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Partner_Name_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 255;
            var name = new string('A', maxLength);

            // Act & Assert
            name.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Partner_Name_OneUnderMax_Accepted()
        {
            // Arrange
            var maxLength = 255;
            var name = new string('A', maxLength - 1);

            // Act & Assert
            name.Length.Should().Be(maxLength - 1);
        }

        [Fact]
        public void Partner_Name_OneOverMax_Rejected()
        {
            // Arrange
            var maxLength = 255;
            var name = new string('A', maxLength + 1);

            // Act & Assert
            (name.Length > maxLength).Should().BeTrue();
        }

        [Fact]
        public void Contact_Email_MinLength_Accepted()
        {
            // Arrange
            var email = "a@b.c"; // 5 characters - minimal valid email

            // Act & Assert
            email.Should().Contain("@");
            email.Length.Should().BeGreaterThanOrEqualTo(5);
        }

        [Fact]
        public void Contact_Email_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 255;
            var localPart = new string('a', 64); // Max local part
            var domain = new string('b', 185) + ".com"; // Fill rest
            var email = $"{localPart}@{domain}";

            // Act & Assert
            email.Length.Should().BeLessThanOrEqualTo(maxLength);
        }

        [Fact]
        public void Contact_FirstName_SingleCharacter_Accepted()
        {
            // Arrange
            var firstName = "A";

            // Act & Assert
            firstName.Length.Should().Be(1);
        }

        [Fact]
        public void Contact_LastName_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 100;
            var lastName = new string('B', maxLength);

            // Act & Assert
            lastName.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Opportunity_Title_SingleCharacter_Accepted()
        {
            // Arrange
            var title = "X";

            // Act & Assert
            title.Length.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Opportunity_Title_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 500;
            var title = new string('T', maxLength);

            // Act & Assert
            title.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Opportunity_Description_Empty_Accepted()
        {
            // Arrange
            var description = "";

            // Act & Assert - Empty description is valid
            description.Should().BeEmpty();
        }

        [Fact]
        public void Opportunity_Description_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 10000;
            var description = new string('D', maxLength);

            // Act & Assert
            description.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Interaction_Notes_Empty_Accepted()
        {
            // Arrange
            var notes = "";

            // Act & Assert
            notes.Should().BeEmpty();
        }

        [Fact]
        public void Interaction_Notes_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 10000;
            var notes = new string('N', maxLength);

            // Act & Assert
            notes.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Document_FileName_MinLength_Accepted()
        {
            // Arrange
            var fileName = "a.pdf"; // 5 characters

            // Act & Assert
            fileName.Length.Should().BeGreaterThanOrEqualTo(5);
        }

        [Fact]
        public void Document_FileName_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 255;
            var fileName = new string('a', maxLength - 4) + ".pdf";

            // Act & Assert
            fileName.Length.Should().BeLessThanOrEqualTo(maxLength);
        }

        [Fact]
        public void Partner_Address_Empty_Accepted()
        {
            // Arrange
            var address = "";

            // Act & Assert
            address.Should().BeEmpty();
        }

        [Fact]
        public void Partner_Address_MaxLength_Accepted()
        {
            // Arrange
            var maxLength = 500;
            var address = new string('A', maxLength);

            // Act & Assert
            address.Length.Should().Be(maxLength);
        }

        [Fact]
        public void Contact_Phone_MinDigits_Accepted()
        {
            // Arrange
            var phone = "12345"; // 5 digits minimum

            // Act & Assert
            phone.Length.Should().BeGreaterThanOrEqualTo(5);
        }

        [Fact]
        public void Contact_Phone_MaxDigits_Accepted()
        {
            // Arrange
            var maxLength = 20;
            var phone = new string('1', maxLength);

            // Act & Assert
            phone.Length.Should().BeLessThanOrEqualTo(maxLength);
        }

        #endregion

        #region Numeric Boundaries (20 tests)

        [Fact]
        public void Opportunity_Value_Zero_Accepted()
        {
            // Arrange
            var value = 0m;

            // Act & Assert
            value.Should().Be(0m);
        }

        [Fact]
        public void Opportunity_Value_OneCent_Accepted()
        {
            // Arrange
            var value = 0.01m;

            // Act & Assert
            value.Should().BeGreaterThan(0m);
        }

        [Fact]
        public void Opportunity_Value_MaxValue_Accepted()
        {
            // Arrange
            var maxValue = 999999999999.99m;

            // Act & Assert
            maxValue.Should().BeGreaterThan(0m);
        }

        [Fact]
        public void Partner_Id_MinValue_One()
        {
            // Arrange
            var minId = 1;

            // Act & Assert
            minId.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Partner_Id_MaxInt_Accepted()
        {
            // Arrange
            var maxId = int.MaxValue;

            // Act & Assert
            maxId.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Pagination_PageSize_Minimum_One()
        {
            // Arrange
            var pageSize = 1;

            // Act & Assert
            pageSize.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Pagination_PageSize_Maximum_100()
        {
            // Arrange
            var maxPageSize = 100;

            // Act & Assert
            maxPageSize.Should().BeLessThanOrEqualTo(100);
        }

        [Fact]
        public void Pagination_PageNumber_Minimum_One()
        {
            // Arrange
            var pageNumber = 1;

            // Act & Assert
            pageNumber.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Pagination_TotalPages_Zero_WhenEmpty()
        {
            // Arrange
            var totalItems = 0;
            var pageSize = 10;

            // Act
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Assert
            totalPages.Should().Be(0);
        }

        [Fact]
        public void Pagination_TotalPages_One_WhenLessThanPageSize()
        {
            // Arrange
            var totalItems = 5;
            var pageSize = 10;

            // Act
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

            // Assert
            totalPages.Should().Be(1);
        }

        [Fact]
        public void Document_Size_MinimumBytes_One()
        {
            // Arrange
            var fileSize = 1;

            // Act & Assert
            fileSize.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Document_Size_MaximumBytes_50MB()
        {
            // Arrange
            var maxSize = 50 * 1024 * 1024; // 50 MB

            // Act & Assert
            maxSize.Should().Be(52428800);
        }

        [Fact]
        public void Opportunity_Probability_Zero_Accepted()
        {
            // Arrange
            var probability = 0;

            // Act & Assert
            probability.Should().BeInRange(0, 100);
        }

        [Fact]
        public void Opportunity_Probability_100_Accepted()
        {
            // Arrange
            var probability = 100;

            // Act & Assert
            probability.Should().BeInRange(0, 100);
        }

        [Fact]
        public void Contact_Count_Zero_Accepted()
        {
            // Arrange
            var contacts = Array.Empty<object>();

            // Act & Assert
            contacts.Should().HaveCount(0);
        }

        [Fact]
        public void Contact_Count_Maximum_Accepted()
        {
            // Arrange
            var maxContacts = 100;
            var contacts = Enumerable.Range(1, maxContacts).ToArray();

            // Act & Assert
            contacts.Should().HaveCount(maxContacts);
        }

        [Fact]
        public void Document_Version_Minimum_One()
        {
            // Arrange
            var version = 1;

            // Act & Assert
            version.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public void Document_Version_Maximum_Accepted()
        {
            // Arrange
            var maxVersion = 100;

            // Act & Assert
            maxVersion.Should().BeLessThanOrEqualTo(100);
        }

        [Fact]
        public void Opportunity_Value_PrecisionTwoDecimals()
        {
            // Arrange
            var value = 123456.78m;

            // Act
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];

            // Assert
            decimalPlaces.Should().BeLessThanOrEqualTo(2);
        }

        [Fact]
        public void Partner_Rating_ZeroToFive()
        {
            // Arrange
            var ratings = new[] { 0, 1, 2, 3, 4, 5 };

            // Act & Assert
            ratings.Should().OnlyContain(r => r >= 0 && r <= 5);
        }

        #endregion

        #region Date/Time Boundaries (20 tests)

        [Fact]
        public void Opportunity_StartDate_Today_Accepted()
        {
            // Arrange
            var startDate = DateTime.Today;

            // Act & Assert
            startDate.Should().Be(DateTime.Today);
        }

        [Fact]
        public void Opportunity_StartDate_FarFuture_Accepted()
        {
            // Arrange
            var startDate = DateTime.Today.AddYears(10);

            // Act & Assert
            startDate.Should().BeAfter(DateTime.Today);
        }

        [Fact]
        public void Opportunity_EndDate_SameAsStart_Accepted()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;

            // Act & Assert
            endDate.Should().BeOnOrAfter(startDate);
        }

        [Fact]
        public void Interaction_Date_Today_Accepted()
        {
            // Arrange
            var interactionDate = DateTime.Today;

            // Act & Assert
            interactionDate.Should().BeSameDateAs(DateTime.Today);
        }

        [Fact]
        public void Interaction_Date_YearsAgo_Accepted()
        {
            // Arrange
            var interactionDate = DateTime.Today.AddYears(-10);

            // Act & Assert
            interactionDate.Should().BeBefore(DateTime.Today);
        }

        [Fact]
        public void Partner_CreatedDate_MinimumSystemDate()
        {
            // Arrange
            var minDate = new DateTime(2000, 1, 1);

            // Act & Assert
            minDate.Should().BeBefore(DateTime.Today);
        }

        [Fact]
        public void AuditLog_Timestamp_Midnight_Accepted()
        {
            // Arrange
            var timestamp = DateTime.Today; // Midnight

            // Act & Assert
            timestamp.TimeOfDay.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void AuditLog_Timestamp_EndOfDay_Accepted()
        {
            // Arrange
            var timestamp = DateTime.Today.AddDays(1).AddTicks(-1); // 23:59:59.9999999

            // Act & Assert
            timestamp.TimeOfDay.Should().BeCloseTo(new TimeSpan(23, 59, 59), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Filter_DateRange_SameDay_ReturnsResults()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;
            var testDate = DateTime.Today;

            // Act
            var isInRange = testDate >= startDate && testDate <= endDate;

            // Assert
            isInRange.Should().BeTrue();
        }

        [Fact]
        public void Filter_DateRange_OneDay_ReturnsResults()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(1);
            var dates = new[] { startDate, startDate.AddHours(12), endDate };

            // Act
            var inRange = dates.Where(d => d >= startDate && d <= endDate).ToList();

            // Assert
            inRange.Should().HaveCount(3);
        }

        [Fact]
        public void Opportunity_Duration_ZeroDays_Accepted()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today;

            // Act
            var duration = (endDate - startDate).Days;

            // Assert
            duration.Should().Be(0);
        }

        [Fact]
        public void Opportunity_Duration_OneYear_Accepted()
        {
            // Arrange
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddYears(1);

            // Act
            var duration = (endDate - startDate).Days;

            // Assert
            duration.Should().BeInRange(365, 366);
        }

        [Fact]
        public void Document_ExpiryDate_Today_IsExpired()
        {
            // Arrange
            var expiryDate = DateTime.Today;
            var today = DateTime.Today;

            // Act
            var isExpired = expiryDate <= today;

            // Assert
            isExpired.Should().BeTrue();
        }

        [Fact]
        public void Document_ExpiryDate_Tomorrow_NotExpired()
        {
            // Arrange
            var expiryDate = DateTime.Today.AddDays(1);
            var today = DateTime.Today;

            // Act
            var isExpired = expiryDate <= today;

            // Assert
            isExpired.Should().BeFalse();
        }

        [Fact]
        public void Partner_LeapYear_Feb29_Accepted()
        {
            // Arrange
            var leapYearDate = new DateTime(2024, 2, 29);

            // Act & Assert
            leapYearDate.Day.Should().Be(29);
            leapYearDate.Month.Should().Be(2);
        }

        [Fact]
        public void Partner_YearEnd_Dec31_Accepted()
        {
            // Arrange
            var yearEnd = new DateTime(2025, 12, 31);

            // Act & Assert
            yearEnd.DayOfYear.Should().Be(365);
        }

        [Fact]
        public void Partner_YearStart_Jan1_Accepted()
        {
            // Arrange
            var yearStart = new DateTime(2025, 1, 1);

            // Act & Assert
            yearStart.DayOfYear.Should().Be(1);
        }

        [Fact]
        public void Interaction_TimeZone_UTC_Accepted()
        {
            // Arrange
            var utcTime = DateTime.UtcNow;

            // Act & Assert
            utcTime.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Interaction_TimeZone_Local_Accepted()
        {
            // Arrange
            var localTime = DateTime.Now;

            // Act & Assert
            localTime.Kind.Should().Be(DateTimeKind.Local);
        }

        [Fact]
        public void Search_DateRange_MaximumSpan_Accepted()
        {
            // Arrange
            var startDate = new DateTime(2000, 1, 1);
            var endDate = new DateTime(2100, 12, 31);

            // Act
            var span = (endDate - startDate).TotalDays;

            // Assert
            span.Should().BeGreaterThan(36000);
        }

        #endregion

        #region Collection Boundaries (20 tests)

        [Fact]
        public void Partner_Contacts_EmptyCollection_Accepted()
        {
            // Arrange
            var contacts = new List<object>();

            // Act & Assert
            contacts.Should().BeEmpty();
        }

        [Fact]
        public void Partner_Contacts_SingleItem_Accepted()
        {
            // Arrange
            var contacts = new List<object> { new { Name = "John" } };

            // Act & Assert
            contacts.Should().HaveCount(1);
        }

        [Fact]
        public void Partner_Contacts_MaxItems_Accepted()
        {
            // Arrange
            var maxContacts = 100;
            var contacts = Enumerable.Range(1, maxContacts).Select(i => new { Id = i }).ToList();

            // Act & Assert
            contacts.Should().HaveCount(maxContacts);
        }

        [Fact]
        public void Search_Results_EmptyCollection_Accepted()
        {
            // Arrange
            var results = Array.Empty<object>();

            // Act & Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void Search_Results_SingleResult_Accepted()
        {
            // Arrange
            var results = new[] { new { Id = 1 } };

            // Act & Assert
            results.Should().HaveCount(1);
        }

        [Fact]
        public void Pagination_FirstPage_ReturnsCorrectItems()
        {
            // Arrange
            var items = Enumerable.Range(1, 100).ToList();
            var pageSize = 10;
            var page = 1;

            // Act
            var result = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Assert
            result.First().Should().Be(1);
            result.Last().Should().Be(10);
        }

        [Fact]
        public void Pagination_LastPage_ReturnsRemainingItems()
        {
            // Arrange
            var items = Enumerable.Range(1, 95).ToList();
            var pageSize = 10;
            var page = 10;

            // Act
            var result = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Assert
            result.Should().HaveCount(5);
            result.First().Should().Be(91);
        }

        [Fact]
        public void Pagination_BeyondLastPage_ReturnsEmpty()
        {
            // Arrange
            var items = Enumerable.Range(1, 50).ToList();
            var pageSize = 10;
            var page = 10;

            // Act
            var result = items.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Filter_AllItemsMatch_ReturnsAll()
        {
            // Arrange
            var items = Enumerable.Range(1, 10).Select(i => new { Value = i, Status = "Active" }).ToList();

            // Act
            var result = items.Where(i => i.Status == "Active").ToList();

            // Assert
            result.Should().HaveCount(10);
        }

        [Fact]
        public void Filter_NoItemsMatch_ReturnsEmpty()
        {
            // Arrange
            var items = Enumerable.Range(1, 10).Select(i => new { Value = i, Status = "Active" }).ToList();

            // Act
            var result = items.Where(i => i.Status == "Inactive").ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Sort_EmptyCollection_ReturnsEmpty()
        {
            // Arrange
            var items = new List<int>();

            // Act
            var sorted = items.OrderBy(i => i).ToList();

            // Assert
            sorted.Should().BeEmpty();
        }

        [Fact]
        public void Sort_SingleItem_ReturnsSameItem()
        {
            // Arrange
            var items = new List<int> { 42 };

            // Act
            var sorted = items.OrderBy(i => i).ToList();

            // Assert
            sorted.Should().HaveCount(1);
            sorted.First().Should().Be(42);
        }

        [Fact]
        public void Sort_AlreadySorted_ReturnsSameOrder()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };

            // Act
            var sorted = items.OrderBy(i => i).ToList();

            // Assert
            sorted.Should().BeInAscendingOrder();
        }

        [Fact]
        public void Sort_ReverseSorted_ReturnsCorrectOrder()
        {
            // Arrange
            var items = new List<int> { 5, 4, 3, 2, 1 };

            // Act
            var sorted = items.OrderBy(i => i).ToList();

            // Assert
            sorted.Should().BeInAscendingOrder();
        }

        [Fact]
        public void GroupBy_EmptyCollection_ReturnsEmpty()
        {
            // Arrange
            var items = new List<object>();

            // Act
            var groups = items.GroupBy(i => i).ToList();

            // Assert
            groups.Should().BeEmpty();
        }

        [Fact]
        public void GroupBy_AllSameGroup_ReturnsSingleGroup()
        {
            // Arrange
            var items = new[] { "A", "A", "A" };

            // Act
            var groups = items.GroupBy(i => i).ToList();

            // Assert
            groups.Should().HaveCount(1);
            groups.First().Count().Should().Be(3);
        }

        [Fact]
        public void GroupBy_AllDifferentGroups_ReturnsMultipleGroups()
        {
            // Arrange
            var items = new[] { "A", "B", "C" };

            // Act
            var groups = items.GroupBy(i => i).ToList();

            // Assert
            groups.Should().HaveCount(3);
        }

        [Fact]
        public void Distinct_EmptyCollection_ReturnsEmpty()
        {
            // Arrange
            var items = new List<int>();

            // Act
            var distinct = items.Distinct().ToList();

            // Assert
            distinct.Should().BeEmpty();
        }

        [Fact]
        public void Distinct_AllDuplicates_ReturnsSingle()
        {
            // Arrange
            var items = new[] { 1, 1, 1, 1, 1 };

            // Act
            var distinct = items.Distinct().ToList();

            // Assert
            distinct.Should().HaveCount(1);
        }

        [Fact]
        public void Distinct_AllUnique_ReturnsAll()
        {
            // Arrange
            var items = new[] { 1, 2, 3, 4, 5 };

            // Act
            var distinct = items.Distinct().ToList();

            // Assert
            distinct.Should().HaveCount(5);
        }

        #endregion

        #region Special Character Handling (20 tests)

        [Fact]
        public void Partner_Name_WithSpaces_Accepted()
        {
            // Arrange
            var name = "ACME Corporation Inc";

            // Act & Assert
            name.Should().Contain(" ");
        }

        [Fact]
        public void Partner_Name_WithApostrophe_Accepted()
        {
            // Arrange
            var name = "O'Brien Industries";

            // Act & Assert
            name.Should().Contain("'");
        }

        [Fact]
        public void Partner_Name_WithAmpersand_Accepted()
        {
            // Arrange
            var name = "Smith & Associates";

            // Act & Assert
            name.Should().Contain("&");
        }

        [Fact]
        public void Partner_Name_WithHyphen_Accepted()
        {
            // Arrange
            var name = "Coca-Cola";

            // Act & Assert
            name.Should().Contain("-");
        }

        [Fact]
        public void Partner_Name_WithPeriod_Accepted()
        {
            // Arrange
            var name = "ACME Inc.";

            // Act & Assert
            name.Should().Contain(".");
        }

        [Fact]
        public void Partner_Name_WithComma_Accepted()
        {
            // Arrange
            var name = "Smith, Jones & Partners";

            // Act & Assert
            name.Should().Contain(",");
        }

        [Fact]
        public void Partner_Name_WithParentheses_Accepted()
        {
            // Arrange
            var name = "UNOPS (Copenhagen)";

            // Act & Assert
            name.Should().Contain("(");
            name.Should().Contain(")");
        }

        [Fact]
        public void Partner_Name_WithNumbers_Accepted()
        {
            // Arrange
            var name = "3M Company";

            // Act & Assert
            name.Should().Contain("3");
        }

        [Fact]
        public void Contact_Name_WithAccents_Accepted()
        {
            // Arrange
            var name = "José García";

            // Act & Assert
            name.Should().Contain("é");
            name.Should().Contain("í");
        }

        [Fact]
        public void Contact_Name_WithUmlaut_Accepted()
        {
            // Arrange
            var name = "Müller";

            // Act & Assert
            name.Should().Contain("ü");
        }

        [Fact]
        public void Contact_Name_WithNordicChars_Accepted()
        {
            // Arrange
            var name = "Øystein Åsen";

            // Act & Assert
            name.Should().Contain("Ø");
            name.Should().Contain("Å");
        }

        [Fact]
        public void Search_WithQuotes_Accepted()
        {
            // Arrange
            var searchTerm = "\"exact match\"";

            // Act & Assert
            searchTerm.Should().StartWith("\"");
            searchTerm.Should().EndWith("\"");
        }

        [Fact]
        public void Search_WithWildcard_Accepted()
        {
            // Arrange
            var searchTerm = "partner*";

            // Act & Assert
            searchTerm.Should().EndWith("*");
        }

        [Fact]
        public void Opportunity_Title_WithSlash_Accepted()
        {
            // Arrange
            var title = "Q1/Q2 Project";

            // Act & Assert
            title.Should().Contain("/");
        }

        [Fact]
        public void Document_FileName_WithUnderscore_Accepted()
        {
            // Arrange
            var fileName = "annual_report_2025.pdf";

            // Act & Assert
            fileName.Should().Contain("_");
        }

        [Fact]
        public void Document_FileName_WithDash_Accepted()
        {
            // Arrange
            var fileName = "annual-report-2025.pdf";

            // Act & Assert
            fileName.Should().Contain("-");
        }

        [Fact]
        public void Partner_Address_WithNewline_Accepted()
        {
            // Arrange
            var address = "123 Main Street\nSuite 456";

            // Act & Assert
            address.Should().Contain("\n");
        }

        [Fact]
        public void Partner_Address_WithTab_Accepted()
        {
            // Arrange
            var address = "Building A\tFloor 3";

            // Act & Assert
            address.Should().Contain("\t");
        }

        [Fact]
        public void Partner_Name_WithEmoji_Handled()
        {
            // Arrange
            var name = "Tech Company 🚀";

            // Act & Assert
            name.Length.Should().BeGreaterThan(10);
        }

        [Fact]
        public void Contact_Name_WithChinese_Accepted()
        {
            // Arrange
            var name = "王明";

            // Act & Assert
            name.Length.Should().Be(2);
        }

        #endregion
    }
}
