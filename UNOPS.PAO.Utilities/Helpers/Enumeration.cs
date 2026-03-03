using System.Collections.Generic;
using System.Reflection;

namespace UNOPS.PAO.Utilities.Helpers;

public abstract class Enumeration<T> : IComparable
{
    protected Enumeration(T id, string name)
    {
        (Value, Name) = (id, name);
    }

    public string Name { get; private set; }

    public T Value { get; }

    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Value);

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration<T> otherValue) return false;

        var typeMatches = GetType().Equals(otherValue.GetType());
        var valueMatches = EqualityComparer<T>.Default.Equals(Value, otherValue.Value);

        return typeMatches && valueMatches;
    }

    public static IEnumerable<TEnum> GetAll<TEnum>() where TEnum : Enumeration<T>
    {
        return typeof(TEnum).GetFields(BindingFlags.Public |
                                   BindingFlags.Static |
                                   BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<TEnum>();
    }
}