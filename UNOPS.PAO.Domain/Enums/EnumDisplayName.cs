namespace UNOPS.PAO.Domain.Enums;

public class EnumDisplayNameAttribute : Attribute
{
    public EnumDisplayNameAttribute(string value)
    {
        Value = value;
    }

    public string Value { get; set; } = null!;
}