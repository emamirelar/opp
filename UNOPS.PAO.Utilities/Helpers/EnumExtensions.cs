namespace UNOPS.PAO.Utilities.Helpers;

using System.Globalization;

public static class EnumExtensions
{
    /// <summary>
    /// Gets all items for an enum value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static IEnumerable<T> GetAllItems<T>(this Enum value)
    {
        foreach (object item in Enum.GetValues(typeof(T)))
        {
            yield return (T)item;
        }
    }

    /// <summary>
    /// Gets all items for an enum type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static IEnumerable<T> GetAllItems<T>() where T : struct
    {
        foreach (object item in Enum.GetValues(typeof(T)))
        {
            yield return (T)item;
        }
    }
    
    /// <summary>
    /// Determines whether the enum value contains a specific value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="request">The request.</param>
    /// <returns>
    ///     <c>true</c> if value contains the specified value; otherwise, <c>false</c>.
    /// </returns>
    /// <example>
    /// <code>
    /// EnumExample dummy = EnumExample.Combi;
    /// if (dummy.Contains<EnumExample>(EnumExample.ValueA))
    /// {
    ///     Console.WriteLine("dummy contains EnumExample.ValueA");
    /// }
    /// </code>
    /// </example>
    public static bool Contains<T>(this Enum value, T request)
    {
        int valueAsInt = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        int requestAsInt = Convert.ToInt32(request, CultureInfo.InvariantCulture);

        if (requestAsInt == (valueAsInt & requestAsInt))
        {
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Checks if the provided value is a valid value for the specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a valid enum value; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if TEnum is not an enum type.</exception>
    public static bool IsValidEnumValue<TEnum>(object value) where TEnum : struct, Enum
    {
        // Check if TEnum is actually an enum type
        if (!typeof(TEnum).IsEnum)
        {
            throw new ArgumentException("TEnum must be an enum type");
        }

        // Check if the value is a valid enum value
        return Enum.IsDefined(typeof(TEnum), value);
    }
}