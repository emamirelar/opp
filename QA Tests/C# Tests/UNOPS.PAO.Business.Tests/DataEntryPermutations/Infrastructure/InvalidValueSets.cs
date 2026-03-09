using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;

/// <summary>
/// Reusable invalid, boundary, and special-case value generators for permutation testing.
/// Used by all entity permutation tests to ensure consistent coverage of edge cases.
/// 
/// Categories covered:
/// - Null / Empty / Whitespace for strings
/// - Boundary min/max for strings and numbers
/// - Invalid format for typed fields (emails, dates, enums)
/// - Special characters / injection patterns
/// - Unicode / multi-byte strings
/// - Very long strings exceeding max lengths
/// </summary>
public static class InvalidValueSets
{
    // ========== STRING INVALID VALUES ==========

    public static readonly string?[] NullEmptyWhitespace = { null, "", "   ", "\t", "\n", "\r\n" };

    public static readonly string[] SpecialCharacters =
    {
        "<script>alert('xss')</script>",
        "'; DROP TABLE Partners;--",
        "..\\..\\etc\\passwd",
        "Robert'); DROP TABLE Students;--",
        "<img src=x onerror=alert(1)>",
        "{{constructor.constructor('return this')()}}"
    };

    public static readonly string[] UnicodeStrings =
    {
        "日本語テスト",
        "Ñoño España",
        "العربية",
        "Ελληνικά",
        "🎉🚀💻🔥",
        "Tes\u0301t with combining characters",
        "\u200B\u200B\u200B",
        "مرحبا بالعالم"
    };

    public static string VeryLongString(int length = 10001) => new('x', length);
    public static string MaxLengthString(int maxLength) => new('A', maxLength);
    public static string OverMaxLengthString(int maxLength) => new('B', maxLength + 1);

    // ========== EMAIL INVALID VALUES ==========

    public static readonly string?[] InvalidEmails =
    {
        null, "", "   ",
        "not-an-email",
        "missing@domain",
        "@nodomain.com",
        "spaces in@email.com",
        "double@@at.com",
        "no.at.sign.com",
        ".starts.with.dot@example.com",
        "ends.with.dot.@example.com",
        "user@.com",
        "user@com",
        new string('a', 250) + "@test.com"
    };

    public static readonly string[] ValidEmails =
    {
        "user@example.com",
        "test.user@domain.org",
        "user+tag@example.co.uk",
        "first.last@subdomain.domain.com"
    };

    // ========== PHONE INVALID VALUES ==========

    public static readonly string?[] InvalidPhones =
    {
        "abc",
        "12",
        new string('9', 50),
        "+++---",
        "<script>",
        "phone@email.com"
    };

    public static readonly string[] ValidPhones =
    {
        "+1-555-0100",
        "+44 20 7946 0958",
        "212-555-1234",
        "+33 1 23 45 67 89"
    };

    // ========== NUMERIC INVALID VALUES ==========

    public static readonly int[] InvalidPositiveInts = { -1, -100, int.MinValue, 0 };
    public static readonly int[] ValidPositiveInts = { 1, 10, 100, 1000 };
    public static readonly int[] BoundaryInts = { int.MinValue, -1, 0, 1, int.MaxValue };

    public static readonly decimal[] InvalidPositiveDecimals = { -1m, -0.01m, -999999m };
    public static readonly decimal[] ValidPositiveDecimals = { 0.01m, 1m, 100m, 999999.99m };
    public static readonly decimal[] BoundaryDecimals =
    {
        decimal.MinValue, -0.01m, 0m, 0.01m, decimal.MaxValue,
        9999999999999999.99m, -9999999999999999.99m
    };

    public static readonly decimal[] PercentageInvalid = { -1m, -0.01m, 100.01m, 200m };
    public static readonly decimal[] PercentageValid = { 0m, 0.01m, 50m, 99.99m, 100m };
    public static readonly decimal[] PercentageBoundary = { -0.01m, 0m, 0.01m, 99.99m, 100m, 100.01m };

    // ========== DATE INVALID VALUES ==========

    public static readonly DateTime[] PastDates =
    {
        DateTime.MinValue,
        new DateTime(1900, 1, 1),
        DateTime.UtcNow.AddYears(-100)
    };

    public static readonly DateTime[] FutureDates =
    {
        DateTime.UtcNow.AddYears(100),
        DateTime.MaxValue.AddYears(-1)
    };

    public static readonly DateTime[] BoundaryDates =
    {
        DateTime.MinValue,
        new DateTime(1900, 1, 1),
        new DateTime(2000, 1, 1),
        new DateTime(2000, 2, 29),
        DateTime.UtcNow.Date,
        DateTime.UtcNow.AddDays(1).Date,
        new DateTime(2099, 12, 31),
        DateTime.MaxValue.AddYears(-1)
    };

    // ========== BOOLEAN TOGGLE PERMUTATIONS ==========

    public static IEnumerable<bool[]> BooleanPermutations(int count)
    {
        var total = (int)Math.Pow(2, count);
        for (int i = 0; i < total; i++)
        {
            var perm = new bool[count];
            for (int j = 0; j < count; j++)
                perm[j] = (i & (1 << j)) != 0;
            yield return perm;
        }
    }

    // ========== STATUS VALUES ==========

    public static readonly string?[] InvalidStatuses =
    {
        null, "", "   ", "InvalidStatus", "ACTIVE", "draft",
        "Deleted", "Suspended", "Unknown", "123"
    };

    public static readonly string[] ValidStatuses = { "Draft", "Active", "Closed", "Archived" };

    // ========== INTERACTION TYPE VALUES ==========

    public static readonly InteractionType[] AllInteractionTypes =
        Enum.GetValues<InteractionType>();

    // ========== COLLECTION VALUES ==========

    public static readonly List<int>? NullList = null;
    public static readonly List<int> EmptyList = new();
    public static readonly List<int> SingleItemList = new() { 1 };
    public static readonly List<int> MultiItemList = new() { 1, 2, 3 };
    public static readonly List<int> DuplicateItemList = new() { 1, 1, 2, 2 };
    public static readonly List<int> LargeList = Enumerable.Range(1, 100).ToList();
    public static readonly List<int> NegativeIdList = new() { -1, -2, 0 };
    public static readonly List<int> NonExistentIdList = new() { 999999, 888888 };

    // ========== MARKDOWN / RICH TEXT ==========

    public static readonly string[] MarkdownStrings =
    {
        "# Heading\n## Subheading\n- List item",
        "**bold** and *italic*",
        "[link](http://example.com)",
        "```code block```",
        "| Table | Header |\n|---|---|\n| Cell | Cell |",
        "> Blockquote with <script>alert('xss')</script>"
    };

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Generates pairwise combinations for two arrays of values.
    /// Returns all unique pairs (a[i], b[j]) for complete pairwise coverage.
    /// </summary>
    public static IEnumerable<(T1, T2)> PairwiseCombine<T1, T2>(T1[] set1, T2[] set2)
    {
        foreach (var v1 in set1)
            foreach (var v2 in set2)
                yield return (v1, v2);
    }

    /// <summary>
    /// Generates one-invalid-at-a-time combinations from multiple field value sets.
    /// For N fields, returns N test cases where field i is invalid and all others are valid.
    /// </summary>
    public static IEnumerable<object[]> OneInvalidAtATime(
        params (object? validValue, object?[] invalidValues, string fieldName)[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            foreach (var invalidValue in fields[i].invalidValues)
            {
                var values = new object?[fields.Length + 1];
                for (int j = 0; j < fields.Length; j++)
                    values[j] = j == i ? invalidValue : fields[j].validValue;
                values[fields.Length] = fields[i].fieldName;
                yield return values!;
            }
        }
    }
}
