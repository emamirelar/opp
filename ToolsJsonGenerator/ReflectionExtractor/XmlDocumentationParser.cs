using System.Xml;
using ReflectionExtractor.Models;

namespace ReflectionExtractor;

public class XmlDocumentationParser
{
    private readonly XmlDocument _xmlDoc;
    private readonly Dictionary<string, XmlDocumentation> _documentationCache = new();

    public XmlDocumentationParser(string xmlPath)
    {
        _xmlDoc = new XmlDocument();
        _xmlDoc.Load(xmlPath);
        Console.WriteLine($"📖 Loaded XML documentation from {xmlPath}");
        
        // Pre-cache all documentation for performance
        CacheDocumentation();
    }

    private void CacheDocumentation()
    {
        var members = _xmlDoc.SelectNodes("//member");
        if (members == null) return;

        foreach (XmlNode member in members)
        {
            var nameAttr = member.Attributes?["name"]?.Value;
            if (string.IsNullOrEmpty(nameAttr)) continue;

            var documentation = new XmlDocumentation
            {
                Summary = GetNodeText(member, "summary"),
                Returns = GetNodeText(member, "returns"),
                ExampleUses = GetExampleUses(member),
                WhenToUse = GetNodeText(member, "when_to_use"),
                Parameters = GetParameters(member)
            };

            _documentationCache[nameAttr] = documentation;
        }

        Console.WriteLine($"📚 Cached documentation for {_documentationCache.Count} members");
    }

    public XmlDocumentation? GetMethodDocumentation(string methodName)
    {
        _documentationCache.TryGetValue(methodName, out var documentation);
        return documentation;
    }

    private string GetNodeText(XmlNode parentNode, string tagName)
    {
        var node = parentNode.SelectSingleNode(tagName);
        return node?.InnerText?.Trim() ?? string.Empty;
    }

    private List<string> GetExampleUses(XmlNode parentNode)
    {
        var exampleUsesNode = parentNode.SelectSingleNode("example_uses");
        if (exampleUsesNode == null) return new List<string>();

        var examples = new List<string>();
        var lines = exampleUsesNode.InnerText
            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));

        examples.AddRange(lines);
        return examples;
    }

    private Dictionary<string, string> GetParameters(XmlNode parentNode)
    {
        var parameters = new Dictionary<string, string>();
        var paramNodes = parentNode.SelectNodes("param");
        
        if (paramNodes == null) return parameters;

        foreach (XmlNode paramNode in paramNodes)
        {
            var nameAttr = paramNode.Attributes?["name"]?.Value;
            if (!string.IsNullOrEmpty(nameAttr))
            {
                parameters[nameAttr] = paramNode.InnerText?.Trim() ?? string.Empty;
            }
        }

        return parameters;
    }

    public string BuildMethodXmlName(Type controllerType, System.Reflection.MethodInfo method)
    {
        // Build XML documentation member name format: "M:Namespace.Class.Method(paramTypes)"
        var parameterTypes = method.GetParameters()
            .Select(p => p.ParameterType.FullName?.Replace('+', '.') ?? p.ParameterType.Name)
            .ToArray();

        var parameterString = parameterTypes.Length > 0 
            ? $"({string.Join(",", parameterTypes)})" 
            : string.Empty;

        return $"M:{controllerType.FullName}.{method.Name}{parameterString}";
    }
}

public class XmlDocumentation
{
    public string Summary { get; set; } = string.Empty;
    public string Returns { get; set; } = string.Empty;
    public List<string> ExampleUses { get; set; } = new();
    public string WhenToUse { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
} 