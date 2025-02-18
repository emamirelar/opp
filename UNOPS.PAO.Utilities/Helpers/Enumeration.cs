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


    public override bool Equals(object obj)
    {
        if (obj is not Enumeration<T> otherValue) return false;

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration<T>
    {
        return typeof(T).GetFields(BindingFlags.Public |
                                   BindingFlags.Static |
                                   BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }
}