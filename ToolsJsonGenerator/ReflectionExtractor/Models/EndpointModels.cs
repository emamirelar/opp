namespace ReflectionExtractor.Models;

public class ExtractedEndpointData
{
    public List<ControllerInfo> Controllers { get; set; } = new();
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
    public string AssemblyName { get; set; } = string.Empty;
    public string AssemblyVersion { get; set; } = string.Empty;
}

public class ControllerInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string BaseRoute { get; set; } = string.Empty;
    public List<MethodInfo> Methods { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
}

public class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string FullRoute { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Returns { get; set; } = string.Empty;
    public List<string> ExampleUses { get; set; } = new();
    public string WhenToUse { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public List<string> Attributes { get; set; } = new();
    public string AccessControl { get; set; } = string.Empty;
}

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsFromBody { get; set; }
    public bool IsFromQuery { get; set; }
    public bool IsFromRoute { get; set; }
    public object? DefaultValue { get; set; }
    public List<PropertyInfo> Properties { get; set; } = new(); // For complex types
}

public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
} 